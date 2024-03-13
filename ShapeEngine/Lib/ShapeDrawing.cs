
using System.Numerics;
using System.Xml;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.UI;

namespace ShapeEngine.Lib;

/// <summary>
/// Determines how the end of a line is drawn.
/// </summary>
public enum LineCapType
{
    /// <summary>
    /// Line is drawn exactly from start to end without any cap.
    /// </summary>
    None = 0,
    /// <summary>
    /// The line is extended by the thickness without any cap.
    /// </summary>
    Extended = 1,
    /// <summary>
    /// The line remains the same length and is drawn with a cap.
    /// Roundness is determined by the cap points.
    /// </summary>
    Capped = 2,
    /// <summary>
    /// The line is extended by the thickness and is drawn with a cap.
    /// Roundness is determined by the cap points.
    /// </summary>
    CappedExtended = 3
}


public static class ShapeDrawing
{
    
    public static float LineMinThickness = 0.5f;

    
    #region Custom Line Drawing
    public static void DrawLine(Vector2 start, Vector2 end, float thickness, ColorRgba color, LineCapType capType = LineCapType.None, int capPoints = 0)
    {
        if (thickness < LineMinThickness) thickness = LineMinThickness;
        var w = end - start;
        float ls = w.X * w.X + w.Y * w.Y; // w.LengthSquared();
        if (ls <= 0f) return;
        
        var dir = w / MathF.Sqrt(ls);
        var pR = new Vector2(-dir.Y, dir.X);//perpendicular right
        var pL = new Vector2(dir.Y, -dir.X);//perpendicular left
        
        if (capType == LineCapType.Extended) //expand outwards
        {
            start -= dir * thickness;
            end += dir * thickness;
        }
        else if (capType == LineCapType.Capped)//shrink inwards so that the line with cap is the same length
        {
            start += dir * thickness;
            end -= dir * thickness;
        }
        
        var tl = start + pL * thickness;
        var bl = start + pR * thickness;
        var br = end + pR * thickness;
        var tr = end + pL * thickness;
        
        Raylib.DrawTriangle(tl, bl, br, color.ToRayColor());
        Raylib.DrawTriangle(tl, br, tr, color.ToRayColor());

        if (capType is LineCapType.None or LineCapType.Extended) return;
        if (capPoints <= 0) return;
        
        //Draw Cap
        if (capPoints == 1)
        {
            var capStart = start - dir * thickness;
            var capEnd = end + dir * thickness;
            
            Raylib.DrawTriangle(tl, capStart, bl, color.ToRayColor());
            Raylib.DrawTriangle(tr, br, capEnd, color.ToRayColor());
        }
        else
        {
            var curStart = tl;
            var curEnd = br;
            float angleStep = (180f / (capPoints + 1)) * ShapeMath.DEGTORAD;
                
            for (var i = 1; i <= capPoints; i++)
            {
                var pStart = start + pL.Rotate(- angleStep * i) * thickness;
                Raylib.DrawTriangle(pStart, start, curStart, color.ToRayColor());
                curStart = pStart;
                    
                var pEnd = end + pR.Rotate(- angleStep * i) * thickness;
                Raylib.DrawTriangle(pEnd, end, curEnd, color.ToRayColor());
                curEnd = pEnd;
            }
            Raylib.DrawTriangle(curStart, bl, start, color.ToRayColor());
            Raylib.DrawTriangle(curEnd, tr, end, color.ToRayColor());

        }
    }

    public static void DrawLine(float startX, float startY, float endX, float endY, float thickness, 
        ColorRgba color, LineCapType capType = LineCapType.None, int capPoints = 0) 
        => DrawLine(new(startX, startY), new(endX, endY), thickness, color, capType, capPoints);

    // public static void DrawLineBackup(Vector2 start, Vector2 end, float thickness, ShapeColor color, LineEndCap lineEndCap = LineEndCap.None, int endCapPoints = 0)
    // {
    //     if (thickness < LineMinThickness) thickness = LineMinThickness;
    //     var w = (end - start);
    //     if (w.LengthSquared() <= 0f) return;
    //     
    //     var dir = w.Normalize();
    //     var pR = dir.GetPerpendicularRight();
    //     var pL = dir.GetPerpendicularLeft();
    //     
    //     if (lineEndCap == LineEndCap.Extended)
    //     {
    //         start -= dir * thickness;
    //         end += dir * thickness;
    //     }
    //     
    //     var tl = start + pL * thickness;
    //     var bl = start + pR * thickness;
    //     var br = end + pR * thickness;
    //     var tr = end + pL * thickness;
    //     Raylib.DrawTriangle(tl, bl, br, color);
    //     Raylib.DrawTriangle(tl, br, tr, color);
    //     
    //     if (lineEndCap == LineEndCap.Capped && endCapPoints > 0)
    //     {
    //         if (endCapPoints == 1)
    //         {
    //             var capStart = start - dir * thickness;
    //             var capEnd = end + dir * thickness;
    //         
    //             Raylib.DrawTriangle(tl, capStart, bl, color);
    //             Raylib.DrawTriangle(tr, br, capEnd, color);
    //         }
    //         else
    //         {
    //             var curStart = tl;
    //             var curEnd = br;
    //             float angleStep = (180f / (endCapPoints + 1)) * ShapeMath.DEGTORAD;
    //             
    //             // DrawCircleV(curEnd, 6f, GREEN);
    //             for (var i = 1; i <= endCapPoints; i++)
    //             {
    //                 var pStart = start + pL.Rotate(- angleStep * i) * thickness;
    //                 Raylib.DrawTriangle(pStart, start, curStart, color);
    //                 curStart = pStart;
    //                 
    //                 var pEnd = end + pR.Rotate(- angleStep * i) * thickness;
    //                 Raylib.DrawTriangle(pEnd, end, curEnd, color);
    //                 // DrawCircleV(pEnd, 6f, WHITE);
    //                 curEnd = pEnd;
    //             }
    //             Raylib.DrawTriangle(curStart, bl, start, color);
    //             Raylib.DrawTriangle(curEnd, tr, end, color);
    //             // DrawCircleV(tr, 6f, RED);
    //
    //         }
    //     }
    // }
    #endregion
    
    #region Intersection
    public static void Draw(this Intersection intersection, float lineThickness, ColorRgba intersectColorRgba, ColorRgba normalColorRgba)
    {
        if (intersection.ColPoints == null || intersection.ColPoints.Count <= 0) return;
        
        foreach (var i in intersection.ColPoints)
        {
            DrawCircle(i.Point, lineThickness * 2f, intersectColorRgba, 12);
            DrawLine(i.Point, i.Point + i.Normal * lineThickness * 10f, lineThickness, normalColorRgba);
            // Segment normal = new(i.Point, i.Point + i.Normal * lineThickness * 10f);
            // normal.Draw(lineThickness, normalColorRgba);
        }
    }

    #endregion
    
    #region Pixel
    public static void DrawPixel(Vector2 pos, ColorRgba color) => Raylib.DrawPixelV(pos, color.ToRayColor()); 
    public static void DrawPixel(float x, float y, ColorRgba color) => Raylib.DrawPixelV(new(x, y), color.ToRayColor());
    #endregion

    #region Point
    public static void Draw(this Vector2 p, float radius, ColorRgba color, int segments = 16) => DrawCircle(p, radius, color, segments);

    public static void Draw(this Points points, float r, ColorRgba color, int segments = 16)
    {
        foreach (var p in points)
        {
            p.Draw(r, color, segments);
        }
    }

    #endregion

    #region Segment
    
    public static void Draw(this Segment segment, float thickness, ColorRgba color, LineCapType capType = LineCapType.None, int capPoints = 0 ) 
        => DrawLine(segment.Start, segment.End, thickness, color, capType, capPoints);
    
    public static void Draw(this Segments segments, float thickness, ColorRgba color, LineCapType capType = LineCapType.None, int capPoints = 0)
    {
        if (segments.Count <= 0) return;
        foreach (var seg in segments)
        {
            seg.Draw(thickness, color, capType, capPoints);
        }
    }
    public static void Draw(this Segments segments, float thickness, List<ColorRgba> colors, LineCapType capType = LineCapType.None, int capPoints = 0)
    {
        if (segments.Count <= 0 || colors.Count <= 0) return;
        for (var i = 0; i < segments.Count; i++)
        {
            var c = colors[i % colors.Count];
            segments[i].Draw(thickness, c, capType, capPoints);
        }
    }
    
    public static void DrawVertices(this Segment segment, float vertexRadius, ColorRgba color, int vertexSegments = 16)
    {
        segment.Start.Draw( vertexRadius, color, vertexSegments);
        segment.End.Draw(vertexRadius, color, vertexSegments);
    }
   
    public static Segments CreateLightningLine(this Segment segment, int segments = 10, float maxSway = 80f)
    {
        Segments result = new();
        var w = segment.End - segment.Start;
        var dir = w.Normalize();
        var n = new Vector2(dir.Y, -dir.X);
        float length = w.Length();

        float prevDisplacement = 0;
        var cur = segment.Start;
        //result.Add(start);

        float segmentLength = length / segments;
        float remainingLength = length;
        List<Vector2> accumulator = new()
        {
            segment.Start
        };
        while (remainingLength > 0f)
        {
            float randSegmentLength = ShapeRandom.RandF() * segmentLength;
            remainingLength -= randSegmentLength;
            if (remainingLength <= 0f)
            {
                if(accumulator.Count == 1)
                {
                    result.Add(new(accumulator[0], segment.End));
                }
                else
                {
                    result.Add(new(result[result.Count - 1].End, segment.End));
                }
                break;
            }
            float scale = randSegmentLength / segmentLength;
            float displacement = ShapeRandom.RandF(-maxSway, maxSway);
            displacement -= (displacement - prevDisplacement) * (1 - scale);
            cur = cur + dir * randSegmentLength;
            var p = cur + displacement * n;
            accumulator.Add(p);
            if(accumulator.Count == 2)
            {
                result.Add(new(accumulator[0], accumulator[1]));
                accumulator.Clear();
            }
            prevDisplacement = displacement;
        }
        return result;
    }
    public static Segments CreateLightningLine(this Segment segment, float segmentLength = 5f, float maxSway = 80f)
    {
        Segments result = new();
        var w = segment.End - segment.Start;
        var dir = w.Normalize();
        var n = new Vector2(dir.Y, -dir.X);
        float length = w.Length();

        float prevDisplacement = 0;
        var cur = segment.Start;
        List<Vector2> accumulator = new()
        {
            segment.Start
        };
        float remainingLength = length;
        while (remainingLength > 0f)
        {
            float randSegmentLength = ShapeRandom.RandF() * segmentLength;
            remainingLength -= randSegmentLength;
            if (remainingLength <= 0f)
            {
                if (accumulator.Count == 1)
                {
                    result.Add(new(accumulator[0], segment.End));
                }
                else
                {
                    result.Add(new(result[result.Count - 1].End, segment.End));
                }
                break;
            }
            float scale = randSegmentLength / segmentLength;
            float displacement = ShapeRandom.RandF(-maxSway, maxSway);
            displacement -= (displacement - prevDisplacement) * (1 - scale);
            cur = cur + dir * randSegmentLength;
            var p = cur + displacement * n;
            accumulator.Add(p);
            if (accumulator.Count == 2)
            {
                result.Add(new(accumulator[0], accumulator[1]));
                accumulator.Clear();
            }
            prevDisplacement = displacement;
        }
        return result;
    }
    
    public static void DrawLineDotted(Vector2 start, Vector2 end, int gaps, float thickness, ColorRgba color, LineCapType capType = LineCapType.None, int capPoints = 0)
    {
        if (gaps <= 0) DrawLine(start, end, thickness, color, capType, capPoints);
        else
        {
            var w = end - start;
            float l = w.Length();
            var dir = w / l;
            int totalGaps = gaps * 2 + 1;
            float size = l / totalGaps;
            var offset = dir * size;

            var cur = start;
            for (var i = 0; i < totalGaps; i++)
            {
                if (i % 2 == 0)
                {
                    var next = cur + offset;
                    DrawLine(cur, next, thickness, color, capType, capPoints);
                    cur = next;

                }
                else
                {
                    cur += offset; //gap
                }
            }
        }
    }
    public static void DrawLineDotted(Vector2 start, Vector2 end, int gaps, float gapSizeF, float thickness, ColorRgba color, LineCapType capType = LineCapType.None, int capPoints = 0)
    {
        if (gaps <= 0) DrawLine(start, end, thickness, color, capType, capPoints);
        else
        {
            var w = end - start;
            float l = w.Length();
            var dir = w / l;

            float totalGapSize = l * gapSizeF;
            float remaining = l - totalGapSize;
            float gapSize = totalGapSize / gaps;
            float size = remaining / (gaps + 1);

            var gapOffset = dir * gapSize;
            var offset = dir * size;

            int totalGaps = gaps * 2 + 1;
            var cur = start;
            for (var i = 0; i < totalGaps; i++)
            {
                if (i % 2 == 0)
                {
                    var next = cur + offset;
                    DrawLine(cur, next, thickness, color, capType, capPoints);
                    cur = next;
                }
                else
                {
                    cur += gapOffset; //gap
                }
            }
        }
    }
    public static void DrawLineGlow(Vector2 start, Vector2 end, float width, float endWidth, ColorRgba color, ColorRgba endColorRgba, int steps, LineCapType capType = LineCapType.None, int capPoints = 0)
    {
        float wStep = (endWidth - width) / steps;

        int rStep = (endColorRgba.R - color.R) / steps;
        int gStep = (endColorRgba.G - color.G) / steps;
        int bStep = (endColorRgba.B - color.B) / steps;
        int aStep = (endColorRgba.A - color.A) / steps;

        for (int i = steps; i >= 0; i--)
        {
            DrawLine
            (
                start, end, width + wStep * i,
                new
                (
                    color.R + rStep * i,
                    color.G + gStep * i,
                    color.B + bStep * i,
                    color.A + aStep * i
                ),
                capType,
                capPoints
            );
        }
    }
    public static void DrawDotted(this Segment segment, int gaps, float thickness, ColorRgba color, LineCapType capType = LineCapType.None, int capPoints = 0)
    {
        DrawLineDotted(segment.Start, segment.End, gaps, thickness, color, capType, capPoints);
    }
    public static void DrawDotted(this Segment segment, int gaps, float gapSizeF, float thickness, ColorRgba color, LineCapType capType = LineCapType.None, int capPoints = 0)
    {
        DrawLineDotted(segment.Start, segment.End, gaps, gapSizeF, thickness, color, capType, capPoints);
    }
    public static void DrawDotted(this Segments segments, int gaps, float thickness, ColorRgba color, LineCapType capType = LineCapType.None, int capPoints = 0)
    {
        foreach (var seg in segments)
        {
            seg.DrawDotted(gaps, thickness, color, capType, capPoints);
        }
    }
    public static void DrawDotted(this Segments segments, int gaps, float gapSizeF, float thickness, ColorRgba color, LineCapType capType = LineCapType.None, int capPoints = 0)
    {
        foreach (var seg in segments)
        {
            seg.DrawDotted(gaps, gapSizeF, thickness, color, capType, capPoints);
        }
    }

    public static void DrawGlow(this Segment segment, float width, float endWidth, ColorRgba color, ColorRgba endColorRgba, int steps, LineCapType capType = LineCapType.None, int capPoints = 0)
    {
        DrawLineGlow(segment.Start, segment.End, width, endWidth, color, endColorRgba, steps, capType, capPoints);
    }
    public static void DrawGlow(this Segments segments, float width, float endWidth, ColorRgba color, ColorRgba endColorRgba, int steps, LineCapType capType = LineCapType.None, int capPoints = 0)
    {
        foreach (var seg in segments)
        {
            seg.DrawGlow(width, endWidth, color, endColorRgba, steps, capType, capPoints);
        }
    }

    
    #endregion

    #region Circle
    public static void DrawCircle(Vector2 center, float radius, ColorRgba color, int segments = 16)
    {
        if (segments < 6) segments = 6;
        Raylib.DrawCircleSector(center, radius, 0, 360, segments, color.ToRayColor());
    }
    public static void Draw(this Circle c, ColorRgba color) => DrawCircle(c.Center, c.Radius, color);
    public static void Draw(this Circle c, ColorRgba color, int segments) => DrawCircle(c.Center, c.Radius, color, segments);
    public static void DrawLines(this Circle c, float lineThickness, int sides, ColorRgba color) => Raylib.DrawPolyLinesEx(c.Center, sides, c.Radius, 0f, lineThickness, color.ToRayColor());
    public static void DrawLines(this Circle c, float lineThickness, float rotDeg, int sides, ColorRgba color) => Raylib.DrawPolyLinesEx(c.Center, sides, c.Radius, rotDeg, lineThickness, color.ToRayColor());
    public static void DrawLines(this Circle c, float lineThickness, ColorRgba color, float sideLength = 8f)
    {
        int sides = GetCircleSideCount(c.Radius, sideLength);
        Raylib.DrawPolyLinesEx(c.Center, sides, c.Radius, 0f, lineThickness, color.ToRayColor());
    }
    
    
    /// <summary>
    /// Very usefull for drawing small/tiny circles. Drawing the circle as rect increases performance a lot.
    /// </summary>
    /// <param name="center"></param>
    /// <param name="radius"></param>
    /// <param name="color"></param>
    public static void DrawCircleFast(Vector2 center, float radius, ColorRgba color)
    {
        // Rect r = new(center, new Vector2(radius * 2f), new Vector2(0.5f));
        // r.Draw(color);
        DrawRect(center - new Vector2(radius, radius), center + new Vector2(radius, radius), color);
    }
    
    public static void DrawCircleLines(Vector2 center, float radius, float lineThickness, int sides, ColorRgba color) => Raylib.DrawPolyLinesEx(center, sides, radius, 0f, lineThickness, color.ToRayColor());
    public static void DrawCircleLines(Vector2 center, float radius, float lineThickness, float rotDeg, int sides, ColorRgba color) => Raylib.DrawPolyLinesEx(center, sides, radius, rotDeg, lineThickness, color.ToRayColor());
    public static void DrawCircleLines(Vector2 center, float radius, float lineThickness, ColorRgba color, float sideLength = 8f)
    {
        int sides = GetCircleSideCount(radius, sideLength);
        Raylib.DrawPolyLinesEx(center, sides, radius, 0f, lineThickness, color.ToRayColor());
    }

    public static void DrawSector(this Circle c, float startAngleDeg, float endAngleDeg, int segments, ColorRgba color)
    {
        Raylib.DrawCircleSector(c.Center, c.Radius, TransformAngleDegToRaylib(startAngleDeg), TransformAngleDegToRaylib(endAngleDeg), segments, color.ToRayColor());
    }
    public static void DrawCircleSector(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, int segments, ColorRgba color)
    {
        Raylib.DrawCircleSector(center, radius, TransformAngleDegToRaylib(startAngleDeg), TransformAngleDegToRaylib(endAngleDeg), segments, color.ToRayColor());
    }
    
    public static void DrawSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, float lineThickness, ColorRgba color, bool closed = true, float sideLength = 8f)
    {
        DrawCircleSectorLines(c.Center, c.Radius, startAngleDeg, endAngleDeg, lineThickness, color, closed, sideLength);
    }
    public static void DrawSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, float lineThickness, ColorRgba color, bool closed = true, float sideLength = 8f)
    {
        DrawCircleSectorLines(c.Center, c.Radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, lineThickness, color, closed, sideLength);
    }
    public static void DrawSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, int sides, float lineThickness, ColorRgba color, bool closed = true)
    {
        DrawCircleSectorLines(c.Center, c.Radius, startAngleDeg, endAngleDeg, sides, lineThickness, color, closed);
    }
    public static void DrawSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, int sides, float lineThickness, ColorRgba color, bool closed = true)
    {
        DrawCircleSectorLines(c.Center, c.Radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, sides, lineThickness, color, closed);
    }
    
    
    public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, float lineThickness, ColorRgba color, bool closed = true, float sideLength = 8f)
    {
        float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
        float endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
        float anglePiece = endAngleRad - startAngleRad;
        int sides = GetCircleArcSideCount(radius, MathF.Abs(anglePiece * ShapeMath.RADTODEG), sideLength);
        float angleStep = anglePiece / sides;
        if (closed)
        {
            var sectorStart = center + (ShapeVec.Right() * radius + new Vector2(lineThickness / 2, 0)).Rotate(startAngleRad);
            DrawLine(center, sectorStart, lineThickness, color, LineCapType.CappedExtended, 2);

            var sectorEnd = center + (ShapeVec.Right() * radius + new Vector2(lineThickness / 2, 0)).Rotate(endAngleRad);
            DrawLine(center, sectorEnd, lineThickness, color, LineCapType.CappedExtended, 2);
        }
        for (var i = 0; i < sides; i++)
        {
            var start = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * i);
            var end = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * (i + 1));
            DrawLine(start, end, lineThickness, color, LineCapType.CappedExtended, 2);
        }
    }
    public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, float lineThickness, ColorRgba color, bool closed = true, float sideLength = 8f)
    {
        DrawCircleSectorLines(center, radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, lineThickness, color, closed, sideLength);
    }
    public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, int sides, float lineThickness, ColorRgba color, bool closed = true)
    {
        float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
        float endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
        float anglePiece = endAngleDeg - startAngleRad;
        float angleStep = MathF.Abs(anglePiece) / sides;
        if (closed)
        {
            var sectorStart = center + (ShapeVec.Right() * radius + new Vector2(lineThickness / 2, 0)).Rotate(startAngleRad);
            DrawLine(center, sectorStart, lineThickness, color, LineCapType.CappedExtended, 2);

            var sectorEnd = center + (ShapeVec.Right() * radius + new Vector2(lineThickness / 2, 0)).Rotate(endAngleRad);
            DrawLine(center, sectorEnd, lineThickness, color, LineCapType.CappedExtended, 2);
        }
        for (var i = 0; i < sides; i++)
        {
            var start = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * i);
            var end = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * (i + 1));
            DrawLine(start, end, lineThickness, color, LineCapType.CappedExtended, 2);
        }
    }
    public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, int sides, float lineThickness, ColorRgba color, bool closed = true)
    {
        DrawCircleSectorLines(center, radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, sides, lineThickness, color, closed);
    }

    public static void DrawCircleLinesDotted(Vector2 center, float radius, int sidesPerGap, float lineThickness, ColorRgba color, float sideLength = 8f, LineCapType capType = LineCapType.CappedExtended,  int capPoints = 2)
    {
        const float anglePieceRad = 360f * ShapeMath.DEGTORAD;
        int sides = GetCircleArcSideCount(radius, MathF.Abs(anglePieceRad * ShapeMath.RADTODEG), sideLength);
        float angleStep = anglePieceRad / sides;

        //int totalGaps = gaps * 2 + 1;
        //float circum = 2f * PI * radius;
        //float size = circum / totalGaps;
        float size = sideLength * sidesPerGap;
        float remainingSize = size;
        var gap = false;
        for (var i = 0; i < sides; i++)
        {
            if (!gap)
            {
                Vector2 start = center + ShapeVec.Rotate(ShapeVec.Right() * radius, angleStep * i);
                Vector2 end = center + ShapeVec.Rotate(ShapeVec.Right() * radius, angleStep * (i + 1));
                DrawLine(start, end, lineThickness, color, capType, capPoints);
            }

            remainingSize -= sideLength;
            if (remainingSize <= 0f)
            {
                gap = !gap;
                remainingSize = size;
            }
        }
    }
    public static void DrawCircleCheckeredLines(Vector2 pos, Vector2 alignement, float radius, float spacing, float lineThickness, float angleDeg, ColorRgba lineColorRgba, ColorRgba bgColorRgba, int circleSegments)
    {

        float maxDimension = radius;
        var size = new Vector2(radius, radius) * 2f;
        var aVector = alignement * size;
        var center = pos - aVector + size / 2;
        float rotRad = angleDeg * ShapeMath.DEGTORAD;

        if (bgColorRgba.A > 0) DrawCircle(center, radius, bgColorRgba, circleSegments);

        var cur = new Vector2(-spacing / 2, 0f);
        while (cur.X > -maxDimension)
        {
            var p = center + cur.Rotate(rotRad);

            //float y = MathF.Sqrt((radius * radius) - (cur.X * cur.X));
            float angle = MathF.Acos(cur.X / radius);
            float y = radius * MathF.Sin(angle);

            var up = new Vector2(0f, -y);
            var down = new Vector2(0f, y);
            var start = p + up.Rotate(rotRad);
            var end = p + down.Rotate(rotRad);
            DrawLine(start, end, lineThickness, lineColorRgba);
            cur.X -= spacing;
        }

        cur = new(spacing / 2, 0f);
        while (cur.X < maxDimension)
        {
            var p = center + cur.Rotate(rotRad);
            //float y = MathF.Sqrt((radius * radius) - (cur.X * cur.X));
            float angle = MathF.Acos(cur.X / radius);
            float y = radius * MathF.Sin(angle);

            var up = new Vector2(0f, -y);
            var down = new Vector2(0f, y);
            var start = p + up.Rotate(rotRad);
            var end = p + down.Rotate(rotRad);
            DrawLine(start, end, lineThickness, lineColorRgba);
            cur.X += spacing;
        }

    }
    
    private static int GetCircleSideCount(float radius, float maxLength = 10f)
    {
        float circumference = 2.0f * ShapeMath.PI * radius;
        return (int)MathF.Max(circumference / maxLength, 1);
    }
    private static int GetCircleArcSideCount(float radius, float angleDeg, float maxLength = 10f)
    {
        float circumference = 2.0f * ShapeMath.PI * radius * (angleDeg / 360f);
        return (int)MathF.Max(circumference / maxLength, 1);
    }
    private static float TransformAngleDegToRaylib(float angleDeg) { return 450f - angleDeg; }
    // private static float TransformAngleRad(float angleRad) { return 2.5f * ShapeMath.PI - angleRad; }
    
    #endregion

    #region Ring
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float lineThickness, ColorRgba color, float sideLength = 8f)
    {
        DrawCircleLines(center, innerRadius, lineThickness, color, sideLength);
        DrawCircleLines(center, outerRadius, lineThickness, color, sideLength);
    }
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float lineThickness, ColorRgba color, float sideLength = 8f)
    {
        DrawCircleSectorLines(center, innerRadius, startAngleDeg, endAngleDeg, lineThickness, color, false, sideLength);
        DrawCircleSectorLines(center, outerRadius, startAngleDeg, endAngleDeg, lineThickness, color, false, sideLength);

        float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
        float endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
        var innerStart = center + (ShapeVec.Right() * innerRadius - new Vector2(lineThickness / 2, 0)).Rotate(startAngleRad);
        var outerStart = center + (ShapeVec.Right() * outerRadius + new Vector2(lineThickness / 2, 0)).Rotate(startAngleRad);
        DrawLine(innerStart, outerStart, lineThickness, color, LineCapType.CappedExtended, 2);

        var innerEnd = center + (ShapeVec.Right() * innerRadius - new Vector2(lineThickness / 2, 0)).Rotate(endAngleRad);
        var outerEnd = center + (ShapeVec.Right() * outerRadius + new Vector2(lineThickness / 2, 0)).Rotate(endAngleRad);
        DrawLine(innerEnd, outerEnd, lineThickness, color, LineCapType.CappedExtended, 2);
    }
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, float lineThickness, ColorRgba color, float sideLength = 8f)
    {
        DrawRingLines(center, innerRadius, outerRadius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, lineThickness, color, sideLength);
    }
    public static void DrawRing(Vector2 center, float innerRadius, float outerRadius, ColorRgba color, float sideLength = 8f)
    {
        DrawRing(center, innerRadius, outerRadius, 0, 360, color, sideLength);
    }
    public static void DrawRing(Vector2 center, float innerRadius, float outerRadius, int sides, ColorRgba color)
    {
        DrawRing(center, innerRadius, outerRadius, 0, 360, sides, color);
    }
    public static void DrawRing(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, ColorRgba color, float sideLength = 10f)
    {
        float start = TransformAngleDegToRaylib(startAngleDeg);
        float end = TransformAngleDegToRaylib(endAngleDeg);
        float anglePiece = end - start;
        int sides = GetCircleArcSideCount(outerRadius, MathF.Abs(anglePiece), sideLength);
        Raylib.DrawRing(center, innerRadius, outerRadius, start, end, sides, color.ToRayColor());
    }
    public static void DrawRing(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, int sides, ColorRgba color)
    {
        Raylib.DrawRing(center, innerRadius, outerRadius, TransformAngleDegToRaylib(startAngleDeg), TransformAngleDegToRaylib(endAngleDeg), sides, color.ToRayColor());
    }
    public static void DrawRing(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, ColorRgba color, float sideLength = 10f)
    {
        DrawRing(center, innerRadius, outerRadius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, color, sideLength);
    }
    public static void DrawRing(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, int sides, ColorRgba color)
    {
        DrawRing(center, innerRadius, outerRadius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, sides, color);
    }

    #endregion

    #region Rectangle

    public static void Draw(this NinePatchRect npr, ColorRgba color)
    {
        var rects = npr.Rects;
        foreach (var r in rects)
        {
            r.Draw(color);
        }
    }
    public static void Draw(this NinePatchRect npr, ColorRgba sourceColorRgba, ColorRgba patchColorRgba)
    {
        npr.Source.Draw(sourceColorRgba);
        var rects = npr.Rects;
        foreach (var r in rects)
        {
            r.Draw(patchColorRgba);
        }
    }
    public static void DrawLines(this NinePatchRect npr, float lineThickness, ColorRgba color)
    {
        var rects = npr.Rects;
        foreach (var r in rects)
        {
            r.DrawLines(lineThickness, color);
        }
    }
    public static void DrawLines(this NinePatchRect npr, float sourceLineThickness, float patchLineThickness, ColorRgba sourceColorRgba, ColorRgba patchColorRgba)
    {
        npr.Source.DrawLines(sourceLineThickness, sourceColorRgba);
        var rects = npr.Rects;
        foreach (var r in rects)
        {
            r.DrawLines(patchLineThickness, patchColorRgba);
        }
    }

    public static void Draw(this Grid grid, Rect bounds, float lineThickness, ColorRgba color)
    {
        Vector2 rowSpacing = new(0f, bounds.Height / grid.Rows);
        for (int row = 0; row < grid.Rows + 1; row++)
        {
            DrawLine(bounds.TopLeft + rowSpacing * row, bounds.TopRight + rowSpacing * row, lineThickness, color);
        }
        Vector2 colSpacing = new(bounds.Width / grid.Cols, 0f);
        for (int col = 0; col < grid.Cols + 1; col++)
        {
            DrawLine(bounds.TopLeft + colSpacing * col, bounds.BottomLeft + colSpacing * col, lineThickness, color);
        }
    }
    public static void DrawGrid(this Rect r, int lines, float lineThickness, ColorRgba color)
    {
        //float hGap = r.width / lines;
        //float vGap = r.height / lines;
        var xOffset = new Vector2(r.Width / lines, 0f);// * i;
        var yOffset = new Vector2(0f, r.Height / lines);// * i;
 
        var tl = r.TopLeft;
        var tr = tl + new Vector2(r.Width, 0);
        var bl = tl + new Vector2(0, r.Height);

        for (var i = 0; i < lines; i++)
        {
            Raylib.DrawLineEx(tl + xOffset * i, bl + xOffset * i, lineThickness, color.ToRayColor());
            Raylib.DrawLineEx(tl + yOffset * i, tr + yOffset * i, lineThickness, color.ToRayColor());
        }
    }

    public static void DrawRect(Vector2 topLeft, Vector2 bottomRight, ColorRgba color)
    {
        Raylib.DrawRectangleV(topLeft, bottomRight - topLeft, color.ToRayColor());
        // Raylib.DrawRectangleRec(new Rect(topLeft, bottomRight).Rectangle, color.ToRayColor());
    }
    public static void DrawRect(Vector2 topLeft, Vector2 bottomRight, Vector2 pivot, float rotDeg, ColorRgba color)
    {
        var a = pivot + (topLeft - pivot).RotateDeg(rotDeg);
        var b = pivot + (new Vector2(topLeft.X, bottomRight.Y) - pivot).RotateDeg(rotDeg);
        var c = pivot + (bottomRight - pivot).RotateDeg(rotDeg);
        var d = pivot + (new Vector2(bottomRight.X, topLeft.Y) -pivot).RotateDeg(rotDeg);
        DrawQuad(a,b,c,d, color);
        
        // Draw(new Rect(topLeft, bottomRight), pivot, rotDeg, color);
    }
    public static void DrawRectLines(Vector2 topLeft, Vector2 bottomRight, float lineThickness, ColorRgba color) => DrawLines(new Rect(topLeft, bottomRight),lineThickness,color);
    public static void DrawRectLines(Vector2 topLeft, Vector2 bottomRight, Vector2 pivot, float rotDeg, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.Extended, int capPoints = 0)
    {
        var a = pivot + (topLeft - pivot).RotateDeg(rotDeg);
        var b = pivot + (new Vector2(topLeft.X, bottomRight.Y) - pivot).RotateDeg(rotDeg);
        var c = pivot + (bottomRight - pivot).RotateDeg(rotDeg);
        var d = pivot + (new Vector2(bottomRight.X, topLeft.Y) -pivot).RotateDeg(rotDeg);
        DrawQuadLines(a,b,c,d, lineThickness, color, capType, capPoints);
        // DrawLines(new Rect(topLeft, bottomRight), pivot, rotDeg, lineThickness, color, capType, capPoints);

    }

    public static void Draw(this Rect rect, ColorRgba color) => Raylib.DrawRectangleRec(rect.Rectangle, color.ToRayColor());
    public static void Draw(this Rect rect, Vector2 pivot, float rotDeg, ColorRgba color)
    {
        DrawRect(rect.TopLeft, rect.BottomRight, pivot, rotDeg, color);
        // var rr = rect.RotateCorners(pivot, rotDeg); // SRect.RotateRect(rect, pivot, rotDeg);
        // Raylib.DrawTriangle(rr.tl, rr.bl, rr.br, color.ToRayColor());
        // Raylib.DrawTriangle(rr.br, rr.tr, rr.tl, color.ToRayColor());
    }
    public static void DrawLines(this Rect rect, float lineThickness, ColorRgba color) => Raylib.DrawRectangleLinesEx(rect.Rectangle, lineThickness, color.ToRayColor());
    public static void DrawLines(this Rect rect, Vector2 pivot, float rotDeg, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.Extended, int capPoints = 0)
    {
        DrawRectLines(rect.TopLeft, rect.BottomRight, pivot, rotDeg, lineThickness, color, capType, capPoints);
        
        // var rr = rect.RotateCorners(pivot, rotDeg); // ShapeRect.Rotate(rect, pivot, rotDeg);

        // DrawLine(rr.tl, rr.tr, lineThickness, color, capType, capPoints);
        // DrawLine(rr.bl, rr.br, lineThickness, color, capType, capPoints);
        // DrawLine(rr.tl, rr.bl, lineThickness, color, capType, capPoints);
        // DrawLine(rr.tr, rr.br, lineThickness, color, capType, capPoints);
    }

    public static void DrawVertices(this Rect rect, float vertexRadius, ColorRgba color, int circleSegments = 8)
    {
        DrawCircle(rect.TopLeft, vertexRadius, color    , circleSegments);
        DrawCircle(rect.TopRight, vertexRadius, color   , circleSegments);
        DrawCircle(rect.BottomLeft, vertexRadius, color , circleSegments);
        DrawCircle(rect.BottomRight, vertexRadius, color, circleSegments);
    }
    public static void DrawRounded(this Rect rect, float roundness, int segments, ColorRgba color) => Raylib.DrawRectangleRounded(rect.Rectangle, roundness, segments, color.ToRayColor());
    public static void DrawRoundedLines(this Rect rect, float roundness, float lineThickness, int segments, ColorRgba color) => Raylib.DrawRectangleRoundedLines(rect.Rectangle, roundness, segments, lineThickness, color.ToRayColor());

    //remove polygon use possible?
    public static void DrawSlantedCorners(this Rect rect, ColorRgba color, float tlCorner, float trCorner, float brCorner, float blCorner)
    {
        var points = GetSlantedCornerPoints(rect, tlCorner, trCorner, brCorner, blCorner);
        points.DrawPolygonConvex(rect.Center, color);
    }
    public static void DrawSlantedCorners(this Rect rect, Vector2 pivot, float rotDeg, ColorRgba color, float tlCorner, float trCorner, float brCorner, float blCorner)
    {
        var poly = GetSlantedCornerPoints(rect, tlCorner, trCorner, brCorner, blCorner);
        poly.RotateSelf(pivot, rotDeg * ShapeMath.DEGTORAD);
        poly.DrawPolygonConvex(rect.Center, color);
        //DrawPolygonConvex(poly, rect.Center, color);
        //var points = SPoly.Rotate(GetSlantedCornerPoints(rect, tlCorner, trCorner, brCorner, blCorner), pivot, rotDeg * SUtils.DEGTORAD);
        //DrawPolygonConvex(points, rect.Center, color);
    }
    public static void DrawSlantedCornersLines(this Rect rect, float lineThickness, ColorRgba color, float tlCorner, float trCorner, float brCorner, float blCorner)
    {
        var points = GetSlantedCornerPoints(rect, tlCorner, trCorner, brCorner, blCorner);
        points.DrawLines(lineThickness, color);
        // DrawLines(points, lineThickness, color);
    }
    public static void DrawSlantedCornersLines(this Rect rect, Vector2 pivot, float rotDeg, float lineThickness, ColorRgba color, float tlCorner, float trCorner, float brCorner, float blCorner)
    {
        var poly = GetSlantedCornerPoints(rect, tlCorner, trCorner, brCorner, blCorner);
        poly.RotateSelf(pivot, rotDeg * ShapeMath.DEGTORAD);
        poly.DrawLines(lineThickness, color);
        // DrawLines(poly, lineThickness, color);
        //var points = SPoly.Rotate(GetSlantedCornerPoints(rect, tlCorner, trCorner, brCorner, blCorner), pivot, rotDeg * SUtils.DEGTORAD);
        //DrawLines(points, lineThickness, color);
    }

    /// <summary>
    /// Get the points to draw a rectangle with slanted corners.
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="tlCorner"></param>
    /// <param name="trCorner"></param>
    /// <param name="brCorner"></param>
    /// <param name="blCorner"></param>
    /// <returns>Returns points in ccw order.</returns>
    private static Polygon GetSlantedCornerPoints(this Rect rect, float tlCorner, float trCorner, float brCorner, float blCorner)
    {
        var tl = rect.TopLeft;
        var tr = rect.TopRight;
        var br = rect.BottomRight;
        var bl = rect.BottomLeft;
        Polygon points = new();
        if (tlCorner > 0f && tlCorner < 1f)
        {
            points.Add(tl + new Vector2(MathF.Min(tlCorner, rect.Width), 0f));
            points.Add(tl + new Vector2(0f, MathF.Min(tlCorner, rect.Height)));
        }
        if (blCorner > 0f && blCorner < 1f)
        {
            points.Add(bl - new Vector2(0f, MathF.Min(tlCorner, rect.Height)));
            points.Add(bl + new Vector2(MathF.Min(tlCorner, rect.Width), 0f));
        }
        if (brCorner > 0f && brCorner < 1f)
        {
            points.Add(br - new Vector2(MathF.Min(tlCorner, rect.Width), 0f));
            points.Add(br - new Vector2(0f, MathF.Min(tlCorner, rect.Height)));
        }
        if (trCorner > 0f && trCorner < 1f)
        {
            points.Add(tr + new Vector2(0f, MathF.Min(tlCorner, rect.Height)));
            points.Add(tr - new Vector2(MathF.Min(tlCorner, rect.Width), 0f));
        }
        return points;
    }
    /// <summary>
    /// Get the points to draw a rectangle with slanted corners. The corner values are the percentage of the width/height of the rectange the should be used for the slant.
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="tlCorner">Should be bewteen 0 - 1</param>
    /// <param name="trCorner">Should be bewteen 0 - 1</param>
    /// <param name="brCorner">Should be bewteen 0 - 1</param>
    /// <param name="blCorner">Should be bewteen 0 - 1</param>
    /// <returns>Returns points in ccw order.</returns>
    private static Polygon GetSlantedCornerPointsRelative(this Rect rect, float tlCorner, float trCorner, float brCorner, float blCorner)
    {
        var tl = rect.TopLeft;
        var tr = rect.TopRight;
        var br = rect.BottomRight;
        var bl = rect.BottomLeft;
        Polygon points = new();
        if (tlCorner > 0f && tlCorner < 1f)
        {
            points.Add(tl + new Vector2(tlCorner * rect.Width, 0f));
            points.Add(tl + new Vector2(0f, tlCorner * rect.Height));
        }
        if (blCorner > 0f && blCorner < 1f)
        {
            points.Add(bl - new Vector2(0f, tlCorner * rect.Height));
            points.Add(bl + new Vector2(tlCorner * rect.Width, 0f));
        }
        if (brCorner > 0f && brCorner < 1f)
        {
            points.Add(br - new Vector2(tlCorner * rect.Width, 0f));
            points.Add(br - new Vector2(0f, tlCorner * rect.Height));
        }
        if (trCorner > 0f && trCorner < 1f)
        {
            points.Add(tr + new Vector2(0f, tlCorner * rect.Height));
            points.Add(tr - new Vector2(tlCorner * rect.Width, 0f));
        }
        return points;
    }
    //-------------------------------
    
    public static void DrawCorners(this Rect rect, float lineThickness, ColorRgba color, float tlCorner, float trCorner, float brCorner, float blCorner, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        var tl = rect.TopLeft;
        var tr = rect.TopRight;
        var br = rect.BottomRight;
        var bl = rect.BottomLeft;

        if (tlCorner > 0f)
        {
            //DrawCircle(tl, lineThickness / 2, color);
            DrawLine(tl, tl + new Vector2(MathF.Min(tlCorner, rect.Width), 0f), lineThickness, color, capType, capPoints);
            DrawLine(tl, tl + new Vector2(0f, MathF.Min(tlCorner, rect.Height)), lineThickness, color, capType, capPoints);
        }
        if (trCorner > 0f)
        {
            //DrawCircle(tr, lineThickness / 2, color);
            DrawLine(tr, tr - new Vector2(MathF.Min(trCorner, rect.Width), 0f), lineThickness, color, capType, capPoints);
            DrawLine(tr, tr + new Vector2(0f, MathF.Min(trCorner, rect.Height)), lineThickness, color, capType, capPoints);
        }
        if (brCorner > 0f)
        {
            //DrawCircle(br, lineThickness / 2, color);
            DrawLine(br, br - new Vector2(MathF.Min(brCorner, rect.Width), 0f), lineThickness, color, capType, capPoints);
            DrawLine(br, br - new Vector2(0f, MathF.Min(brCorner, rect.Height)), lineThickness, color, capType, capPoints);
        }
        if (blCorner > 0f)
        {
            //DrawCircle(bl, lineThickness / 2, color);
            DrawLine(bl, bl + new Vector2(MathF.Min(blCorner, rect.Width), 0f), lineThickness, color, capType, capPoints);
            DrawLine(bl, bl - new Vector2(0f, MathF.Min(blCorner, rect.Height)), lineThickness, color, capType, capPoints);
        }
    }
    public static void DrawCorners(this Rect rect, float lineThickness, ColorRgba color, float cornerLength, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
        => DrawCorners(rect, lineThickness, color, cornerLength, cornerLength, cornerLength, cornerLength, capType, capPoints);
    public static void DrawCornersRelative(this Rect rect, float lineThickness, ColorRgba color, float tlCorner, float trCorner, float brCorner, float blCorner, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        var tl = rect.TopLeft;
        var tr = rect.TopRight;
        var br = rect.BottomRight;
        var bl = rect.BottomLeft;

        if (tlCorner > 0f && tlCorner < 1f)
        {
            DrawCircle(tl, lineThickness / 2, color);
            DrawLine(tl, tl + new Vector2(tlCorner * rect.Width, 0f), lineThickness, color, capType, capPoints);
            DrawLine(tl, tl + new Vector2(0f, tlCorner * rect.Height), lineThickness, color, capType, capPoints);
        }
        if (trCorner > 0f && trCorner < 1f)
        {
            DrawCircle(tr, lineThickness / 2, color);
            DrawLine(tr, tr - new Vector2(tlCorner * rect.Width, 0f), lineThickness, color, capType, capPoints);
            DrawLine(tr, tr + new Vector2(0f, tlCorner * rect.Height), lineThickness, color, capType, capPoints);
        }
        if (brCorner > 0f && brCorner < 1f)
        {
            DrawCircle(br, lineThickness / 2, color);
            DrawLine(br, br - new Vector2(tlCorner * rect.Width, 0f), lineThickness, color, capType, capPoints);
            DrawLine(br, br - new Vector2(0f, tlCorner * rect.Height), lineThickness, color, capType, capPoints);
        }
        if (blCorner > 0f && blCorner < 1f)
        {
            DrawCircle(bl, lineThickness / 2, color);
            DrawLine(bl, bl + new Vector2(tlCorner * rect.Width, 0f), lineThickness, color, capType, capPoints);
            DrawLine(bl, bl - new Vector2(0f, tlCorner * rect.Height), lineThickness, color, capType, capPoints);
        }
    }
    public static void DrawCornersRelative(this Rect rect, float lineThickness, ColorRgba color, float cornerLengthFactor, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2) 
        => DrawCornersRelative(rect, lineThickness, color, cornerLengthFactor, cornerLengthFactor, cornerLengthFactor, cornerLengthFactor, capType, capPoints);
    
    public static void DrawCheckered(this Rect rect, float spacing, float lineThickness, float angleDeg, ColorRgba lineColor, ColorRgba outlineColor, ColorRgba bgColor)
    {
        var size = new Vector2(rect.Width, rect.Height);
        var center = new Vector2(rect.X, rect.Y) + size / 2;
        float maxDimension = MathF.Max(size.X, size.Y);
        float rotRad = angleDeg * ShapeMath.DEGTORAD;

        //var tl = new Vector2(rect.X, rect.Y);
        //var tr = new Vector2(rect.X + rect.Width, rect.Y);
        //var bl = new Vector2(rect.X, rect.Y + rect.Height);
        //var br = new Vector2(rect.X + rect.Width, rect.Y + rect.Height);

        if (bgColor.A > 0) rect.Draw(bgColor); //DrawRectangleRec(rect.Rectangle, bgColor.ToRayColor());

        Vector2 cur = new(-spacing / 2, 0f);

        //safety for while loops
        int whileMaxCount = (int)(maxDimension / spacing) * 2;
        int whileCounter = 0;

        //left half of rectangle
        while (whileCounter < whileMaxCount)
        {
            var p = center + cur.Rotate(rotRad);
            var up = new Vector2(0f, -maxDimension * 2);//make sure that lines are going outside of the rectangle
            var down = new Vector2(0f, maxDimension * 2);
            var start = p + up.Rotate(rotRad);
            var end = p + down.Rotate(rotRad);
            var seg = new Segment(start, end);
            var collisionPoints = seg.IntersectShape(rect);

            
            
            if (collisionPoints != null && collisionPoints.Count >= 2) 
                DrawLine(collisionPoints[0].Point, collisionPoints[1].Point, lineThickness, lineColor);
            else break;
            
            cur.X -= spacing;
            whileCounter++;
        }

        cur = new(spacing / 2, 0f);
        whileCounter = 0;
        //right half of rectangle
        while (whileCounter < whileMaxCount)
        {
            var p = center + ShapeVec.Rotate(cur, rotRad);
            var up = new Vector2(0f, -maxDimension * 2);
            var down = new Vector2(0f, maxDimension * 2);
            var start = p + ShapeVec.Rotate(up, rotRad);
            var end = p + ShapeVec.Rotate(down, rotRad);
            var seg = new Segment(start, end);
            var collisionPoints = seg.IntersectShape(rect); //SGeometry.IntersectionSegmentRect(center, start, end, tl, tr, br, bl).points;
            
            
            if (collisionPoints != null && collisionPoints.Count >= 2 ) 
                DrawLine(collisionPoints[0].Point, collisionPoints[1].Point, lineThickness, lineColor);
            else break;
            cur.X += spacing;
            whileCounter++;
        }

        if (outlineColor.A > 0) DrawLines(rect, new Vector2(0.5f, 0.5f), 0f, lineThickness, outlineColor);
    }

    //possible to remove segments use?
    public static void DrawLinesDotted(this Rect rect, int gapsPerSide, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 3)
    {
        // if (cornerCircleSectors > 5)
        // {
        //     var corners = ShapeRect.GetCorners(rect);
        //     float r = lineThickness * 0.5f;
        //     DrawCircle(corners.tl, r, color, cornerCircleSectors);
        //     DrawCircle(corners.tr, r, color, cornerCircleSectors);
        //     DrawCircle(corners.br, r, color, cornerCircleSectors);
        //     DrawCircle(corners.bl, r, color, cornerCircleSectors);
        // }
        var segments = rect.GetEdges();
        foreach (var s in segments)
        {
            DrawLineDotted(s.Start, s.End, gapsPerSide, lineThickness, color, capType, capPoints);
        }
    }
    public static void DrawLinesDotted(this Rect rect, int gapsPerSide, float gapSizeF, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 3)
    {
        // if (cornerCircleSegments > 5)
        // {
        //     var corners = ShapeRect.GetCorners(rect);
        //     float r = lineThickness * 0.5f;
        //     DrawCircle(corners.tl, r, color, cornerCircleSegments);
        //     DrawCircle(corners.tr, r, color, cornerCircleSegments);
        //     DrawCircle(corners.br, r, color, cornerCircleSegments);
        //     DrawCircle(corners.bl, r, color, cornerCircleSegments);
        // }
        var segments = rect.GetEdges(); // SRect.GetEdges(rect);
        foreach (var s in segments)
        {
            DrawLineDotted(s.Start, s.End, gapsPerSide, gapSizeF, lineThickness, color, capType, capPoints);
        }
    }
    #endregion

    #region Triangle
    public static void DrawTriangle(Vector2 a, Vector2 b, Vector2 c, ColorRgba color) => Raylib.DrawTriangle(a, b, c, color.ToRayColor());

    public static void DrawTriangleLines(Vector2 a, Vector2 b, Vector2 c, float lineThickness,
        ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        DrawLine(a, b, lineThickness, color, capType, capPoints);
        DrawLine(b, c, lineThickness, color, capType, capPoints);
        DrawLine(c, a, lineThickness, color, capType, capPoints);
        
        // new Triangle(a, b, c).GetEdges().Draw(lineThickness, color);
    }

    public static void Draw(this Triangle t, ColorRgba color) => Raylib.DrawTriangle(t.A, t.B, t.C, color.ToRayColor());

    public static void DrawLines(this Triangle t, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        DrawTriangleLines(t.A, t.B, t.C, lineThickness, color);
        // t.GetEdges().Draw(lineThickness, color);
    }

    public static void DrawVertices(this Triangle t, float vertexRadius, ColorRgba color, int circleSegments = 8)
    {
        DrawCircle(t.A, vertexRadius, color, circleSegments);
        DrawCircle(t.B, vertexRadius, color, circleSegments);
        DrawCircle(t.C, vertexRadius, color, circleSegments);
    }
    public static void Draw(this Triangulation triangles, ColorRgba color) { foreach (var t in triangles) t.Draw(color); }

    public static void DrawLines(this Triangulation triangles, float lineThickness, ColorRgba color,
        LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        foreach (var t in triangles) t.DrawLines(lineThickness, color, capType, capPoints);
    }
    
    #endregion

    #region Quad
    public static void DrawQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d, ColorRgba color)
    {
        Raylib.DrawTriangle(a, b, c, color.ToRayColor());
        Raylib.DrawTriangle(a, c, d, color.ToRayColor());
    }

    public static void DrawQuadLines(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float lineThickness,
        ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        DrawLine(a, b, lineThickness, color, capType, capPoints);
        DrawLine(b, c, lineThickness, color, capType, capPoints);
        DrawLine(c, d, lineThickness, color, capType, capPoints);
        DrawLine(d, a, lineThickness, color, capType, capPoints);
        
        // new Triangle(a, b, c).GetEdges().Draw(lineThickness, color);
    }

    public static void Draw(this Quad q, ColorRgba color) => DrawQuad(q.A, q.B, q.C, q.D, color);

    public static void DrawLines(this Quad q, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        DrawQuadLines(q.A, q.B, q.C, q.D, lineThickness, color);
        // t.GetEdges().Draw(lineThickness, color);
    }

    public static void DrawVertices(this Quad q, float vertexRadius, ColorRgba color, int circleSegments = 8)
    {
        DrawCircle(q.A, vertexRadius, color, circleSegments);
        DrawCircle(q.B, vertexRadius, color, circleSegments);
        DrawCircle(q.C, vertexRadius, color, circleSegments);
        DrawCircle(q.D, vertexRadius, color, circleSegments);
    }

    #endregion
    
    #region Polygon

    public static void DrawPolygonConvex(this Polygon poly, ColorRgba color, bool clockwise = false) { DrawPolygonConvex(poly, poly.GetCentroid(), color, clockwise); }
    public static void DrawPolygonConvex(this Polygon poly, Vector2 center, ColorRgba color, bool clockwise = false)
    {
        if (clockwise)
        {
            for (var i = 0; i < poly.Count - 1; i++)
            {
                Raylib.DrawTriangle(poly[i], center, poly[i + 1], color.ToRayColor());
            }
            Raylib.DrawTriangle(poly[poly.Count - 1], center, poly[0], color.ToRayColor());
        }
        else
        {
            for (var i = 0; i < poly.Count - 1; i++)
            {
                Raylib.DrawTriangle(poly[i], poly[i + 1], center, color.ToRayColor());
            }
            Raylib.DrawTriangle(poly[poly.Count - 1], poly[0], center, color.ToRayColor());
        }
    }
    public static void DrawPolygonConvex(this Polygon relativePoly, Vector2 pos, float scale, float rotDeg, ColorRgba color, bool clockwise = false)
    {
        if (clockwise)
        {
            for (int i = 0; i < relativePoly.Count - 1; i++)
            {
                var a = pos + ShapeVec.Rotate(relativePoly[i] * scale, rotDeg * ShapeMath.DEGTORAD);
                var b = pos;
                var c = pos + ShapeVec.Rotate(relativePoly[i + 1] * scale, rotDeg * ShapeMath.DEGTORAD);
                Raylib.DrawTriangle(a, b, c, color.ToRayColor());
            }

            var aFinal = pos + (relativePoly[relativePoly.Count - 1] * scale).Rotate(rotDeg * ShapeMath.DEGTORAD);
            var bFinal = pos;
            var cFinal = pos + (relativePoly[0] * scale).Rotate(rotDeg * ShapeMath.DEGTORAD);
            Raylib.DrawTriangle(aFinal, bFinal, cFinal, color.ToRayColor());
        }
        else
        {
            for (int i = 0; i < relativePoly.Count - 1; i++)
            {
                var a = pos + ShapeVec.Rotate(relativePoly[i] * scale, rotDeg * ShapeMath.DEGTORAD);
                var b = pos + ShapeVec.Rotate(relativePoly[i + 1] * scale, rotDeg * ShapeMath.DEGTORAD);
                var c = pos;
                Raylib.DrawTriangle(a, b, c, color.ToRayColor());
            }

            var aFinal = pos + (relativePoly[relativePoly.Count - 1] * scale).Rotate(rotDeg * ShapeMath.DEGTORAD);
            var bFinal = pos + (relativePoly[0] * scale).Rotate(rotDeg * ShapeMath.DEGTORAD);
            var cFinal = pos;
            Raylib.DrawTriangle(aFinal, bFinal, cFinal, color.ToRayColor());
        }
    }
    public static void Draw(this Polygon poly, ColorRgba color)
    {
        if (poly.Count < 3) return;
        if (poly.Count == 3)
        {
            DrawTriangle(poly[0], poly[1], poly[2], color);
            return;
        }
        poly.Triangulate().Draw(color);
    }
    public static void DEBUG_DrawLinesCCW(this Polygon poly, float lineThickness, ColorRgba startColorRgba, ColorRgba endColorRgba)
    {
        if (poly.Count < 3) return;

        DrawLines(poly, lineThickness, startColorRgba, endColorRgba);
        ShapeDrawing.DrawCircle(poly[0], lineThickness * 2f, startColorRgba);
        ShapeDrawing.DrawCircle(poly[poly.Count - 1], lineThickness * 2f, endColorRgba);
        // var edges = poly.GetEdges();
        // int redStep =   (endColor.r - startColor.r) / edges.Count;
        // int greenStep = (endColor.g - startColor.g) / edges.Count;
        // int blueStep =  (endColor.b - startColor.b) / edges.Count;
        // int alphaStep = (endColor.a - startColor.a) / edges.Count;
        //
        // for (int i = 0; i < edges.Count; i++)
        // {
        //     var edge = edges[i];
        //     ShapeColor finalColor = new
        //         (
        //             startColor.r + redStep * i,
        //             startColor.g + greenStep * i,
        //             startColor.b + blueStep * i,
        //             startColor.a + alphaStep * i
        //         );
        //     edge.Draw(lineThickness, finalColor, LineCapType.CappedExtended, 2);
        // }
        
    }
    
    public static void DrawLines(this Polygon poly, float lineThickness, ColorRgba startColorRgba, ColorRgba endColorRgba, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (poly.Count < 3) return;

        int redStep = (endColorRgba.R - startColorRgba.R) / poly.Count;
        int greenStep = (endColorRgba.G - startColorRgba.G) / poly.Count;
        int blueStep = (endColorRgba.B - startColorRgba.B) / poly.Count;
        int alphaStep = (endColorRgba.A - startColorRgba.A) / poly.Count;
        for (var i = 0; i < poly.Count; i++)
        {
            var start = poly[i];
            var end = poly[(i + 1) % poly.Count];
            ColorRgba finalColorRgba = new
            (
                startColorRgba.R + redStep * i,
                startColorRgba.G + greenStep * i,
                startColorRgba.B + blueStep * i,
                startColorRgba.A + alphaStep * i
            );
            DrawLine(start, end, lineThickness, finalColorRgba, capType, capPoints);
        }
        
        
        // var edges = poly.GetEdges();
        // int redStep = (endColor.r - startColor.r) / edges.Count;
        // int greenStep = (endColor.g - startColor.g) / edges.Count;
        // int blueStep = (endColor.b - startColor.b) / edges.Count;
        // int alphaStep = (endColor.a - startColor.a) / edges.Count;

        // for (int i = 0; i < edges.Count; i++)
        // {
            // var edge = edges[i];
            // ShapeColor finalColor = new
                // (
                    // startColor.r + redStep * i,
                    // startColor.g + greenStep * i,
                    // startColor.b + blueStep * i,
                    // startColor.a + alphaStep * i
                // );
            //// if(cornerSegments > 5) DrawCircle(edge.Start, lineThickness * 0.5f, finalColor, cornerSegments);
            // edge.Draw(lineThickness, finalColor);
        // }
    }
    // public static void DrawLines(this Polygon poly, float lineThickness, ShapeColor color)
    // {
    //     poly.DrawLines(lineThickness, color, 2);
    // }
    public static void DrawLines(this Polygon poly, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (poly.Count < 3) return;
        // for (int i = 0; i < poly.Count - 1; i++)
        // {
            // DrawLine(poly[i], poly[i + 1], lineThickness, color, capType, capPoints);
        // }
        // DrawLine(poly[poly.Count - 1], poly[0], lineThickness, color, capType, capPoints);
        
        for (var i = 0; i < poly.Count; i++)
        {
            var start = poly[i];
            var end = poly[(i + 1) % poly.Count];
            DrawLine(start, end, lineThickness, color, capType, capPoints);
        }
    }
    public static void DrawLines(this Polygon poly, Vector2 pos, Vector2 size, float rotDeg, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (poly.Count < 3) return;
        
        for (var i = 0; i < poly.Count; i++)
        {
            var start = pos + (poly[i] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
            var end = pos + (poly[(i + 1) % poly.Count] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
            DrawLine(start, end, lineThickness, color, capType, capPoints);
        }
        
        // for (int i = 0; i < poly.Count - 1; i++)
        // {
            // Vector2 p1 = pos + ShapeVec.Rotate(poly[i] * size, rotDeg * ShapeMath.DEGTORAD);
            // Vector2 p2 = pos + ShapeVec.Rotate(poly[i + 1] * size, rotDeg * ShapeMath.DEGTORAD);
            // DrawLine(p1, p2, lineThickness, color, capType, capPoints);
        // }
        // DrawLineEx(pos + ShapeVec.Rotate(poly[poly.Count - 1] * size, rotDeg * ShapeMath.DEGTORAD), pos + ShapeVec.Rotate(poly[0] * size, rotDeg * ShapeMath.DEGTORAD), lineThickness, outlineColor);
    }
    public static void DrawVertices(this Polygon poly, float vertexRadius, ColorRgba color, int circleSegments)
    {
        foreach (var p in poly)
        {
            DrawCircle(p, vertexRadius, color, circleSegments);
        }
    }
    
    public static void DrawCornered(this Polygon poly, float lineThickness, ColorRgba color, float cornerLength, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        for (var i = 0; i < poly.Count; i++)
        {
            var prev = poly[(i-1)%poly.Count];
            var cur = poly[i];
            var next = poly[(i+1)%poly.Count];
            DrawLine(cur, cur + next.Normalize() * cornerLength, lineThickness, color, capType, capPoints);
            DrawLine(cur, cur + prev.Normalize() * cornerLength, lineThickness, color, capType, capPoints);
        }
    }
    public static void DrawCornered(this Polygon poly, float lineThickness, ColorRgba color, List<float> cornerLengths, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        for (var i = 0; i < poly.Count; i++)
        {
            float cornerLength = cornerLengths[i%cornerLengths.Count];
            var prev = poly[(i - 1) % poly.Count];
            var cur = poly[i];
            var next = poly[(i + 1) % poly.Count];
            DrawLine(cur, cur + next.Normalize() * cornerLength, lineThickness, color, capType, capPoints);
            DrawLine(cur, cur + prev.Normalize() * cornerLength, lineThickness, color, capType, capPoints);
        }
    }
    public static void DrawCorneredRelative(this Polygon poly, float lineThickness, ColorRgba color, float cornerF, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        for (var i = 0; i < poly.Count; i++)
        {
            var prev = poly[(i - 1) % poly.Count];
            var cur = poly[i];
            var next = poly[(i + 1) % poly.Count];
            DrawLine(cur, cur.Lerp(next, cornerF), lineThickness, color, capType, capPoints);
            DrawLine(cur, cur.Lerp(prev, cornerF), lineThickness, color, capType, capPoints);
        }
    }
    public static void DrawCorneredRelative(this Polygon poly, float lineThickness, ColorRgba color, List<float> cornerFactors, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        for (var i = 0; i < poly.Count; i++)
        {
            float cornerF = cornerFactors[i%cornerFactors.Count];
            var prev = poly[(i - 1) % poly.Count];
            var cur = poly[i];
            var next = poly[(i + 1) % poly.Count];
            DrawLine(cur, cur.Lerp(next, cornerF), lineThickness, color, capType, capPoints);
            DrawLine(cur, cur.Lerp(prev, cornerF), lineThickness, color, capType, capPoints);
        }
    }
    #endregion

    #region Polyline

    public static void Draw(this Polyline polyline, float thickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (polyline.Count < 2) return;
        for (var i = 0; i < polyline.Count - 1; i++)
        {
            var start = polyline[i];
            var end = polyline[i + 1];
            DrawLine(start, end, thickness, color, capType, capPoints);
        }
        // polyline.GetEdges().Draw(thickness, color);
    }

    public static void Draw(this Polyline polyline, float thickness, List<ColorRgba> colors, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (polyline.Count < 2) return;
        for (var i = 0; i < polyline.Count - 1; i++)
        {
            var start = polyline[i];
            var end = polyline[i + 1];
            var c = colors[i % colors.Count];
            DrawLine(start, end, thickness, c, capType, capPoints);
        }
        // polyline.GetEdges().Draw(thickness, colors);
    }
    public static void DrawVertices(this Polyline polyline, float vertexRadius, ColorRgba color, int circleSegments)
    {
        foreach (var p in polyline)
        {
            DrawCircle(p, vertexRadius, color, circleSegments);
        }
    }

    public static void DrawDotted(this Polyline polyline, int gaps, float thickness, ColorRgba color, LineCapType capType = LineCapType.Capped, int capPoints = 3)
    {
        if (polyline.Count < 2) return;
        for (var i = 0; i < polyline.Count - 1; i++)
        {
            var start = polyline[i];
            var end = polyline[i + 1];
            DrawLineDotted(start, end, gaps, thickness, color, capType, capPoints);
        }
         // polyline.GetEdges().DrawDotted(gaps, thickness, color, endCapSegments);
    }

    public static void DrawDotted(this Polyline polyline, int gaps, float gapSizeF, 
        float thickness, ColorRgba color, LineCapType capType = LineCapType.Capped, int capPoints = 3)
    {
        if (polyline.Count < 2) return;
        for (var i = 0; i < polyline.Count - 1; i++)
        {
            var start = polyline[i];
            var end = polyline[i + 1];
            DrawLineDotted(start, end, gaps, gapSizeF, thickness, color, capType, capPoints);
        }
        // polyline.GetEdges().DrawDotted(gaps, gapSizeF, thickness, color, endCapSegments);
    }

    public static void DrawGlow(this Polyline polyline, float width, float endWidth, ColorRgba color,
        ColorRgba endColorRgba, int steps, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (polyline.Count < 2) return;
        for (var i = 0; i < polyline.Count - 1; i++)
        {
            var start = polyline[i];
            var end = polyline[i + 1];
            DrawLineGlow(start, end, width, endWidth, color, endColorRgba, steps, capType, capPoints);
        }
        // polyline.GetEdges().DrawGlow(width, endWidth, color, endColor, steps);
    }

    #endregion
    
    #region UI
    public static void DrawOutlineBar(this Rect rect, float thickness, float f, ColorRgba color)
    {
        var thicknessOffsetX = new Vector2(thickness, 0f);
        var thicknessOffsetY = new Vector2(0f, thickness);

        var tl = new Vector2(rect.X, rect.Y);
        var br = tl + new Vector2(rect.Width, rect.Height);
        var tr = tl + new Vector2(rect.Width, 0);
        var bl = tl + new Vector2(0, rect.Height);

        var lines = (int)MathF.Ceiling(4 * ShapeMath.Clamp(f, 0f, 1f));
        float fMin = 0.25f * (lines - 1);
        float fMax = fMin + 0.25f;
        float newF = ShapeMath.RemapFloat(f, fMin, fMax, 0f, 1f);
        for (var i = 0; i < lines; i++)
        {
            Vector2 end;
            Vector2 start;
            if (i == 0)
            {
                start = tl - thicknessOffsetX / 2;
                end = tr - thicknessOffsetX / 2;
            }
            else if (i == 1)
            {
                start = tr - thicknessOffsetY / 2;
                end = br - thicknessOffsetY / 2;
            }
            else if (i == 2)
            {
                start = br + thicknessOffsetX / 2;
                end = bl + thicknessOffsetX / 2;
            }
            else
            {
                start = bl + thicknessOffsetY / 2;
                end = tl + thicknessOffsetY / 2;
            }

            //last line
            if (i == lines - 1) end = ShapeVec.Lerp(start, end, newF);
            ShapeDrawing.DrawLine(start, end, thickness, color);
            // DrawLineEx(start, end, thickness, color.ToRayColor());
        }
    }
    public static void DrawOutlineBar(this Rect rect, Vector2 pivot, float angleDeg, float thickness, float f, ColorRgba color)
    {
        var rr = rect.RotateCorners(pivot, angleDeg);
        //Vector2 thicknessOffsetX = new Vector2(thickness, 0f);
        //Vector2 thicknessOffsetY = new Vector2(0f, thickness);

        var leftExtension = new Vector2(-thickness / 2, 0f).Rotate(angleDeg * ShapeMath.DEGTORAD);
        var rightExtension = new Vector2(thickness / 2, 0f).Rotate(angleDeg * ShapeMath.DEGTORAD);

        var tl = rr.tl;
        var br = rr.br;
        var tr = rr.tr;
        var bl = rr.bl;

        int lines = (int)MathF.Ceiling(4 * ShapeMath.Clamp(f, 0f, 1f));
        float fMin = 0.25f * (lines - 1);
        float fMax = fMin + 0.25f;
        float newF = ShapeMath.RemapFloat(f, fMin, fMax, 0f, 1f);
        for (int i = 0; i < lines; i++)
        {
            Vector2 end;
            Vector2 start;
            if (i == 0)
            {
                start = tl + leftExtension;
                end = tr + rightExtension;
            }
            else if (i == 1)
            {
                start = tr;
                end = br;
            }
            else if (i == 2)
            {
                start = br + rightExtension;
                end = bl + leftExtension;
            }
            else
            {
                start = bl;
                end = tl;
            }

            //last line
            if (i == lines - 1) end = ShapeVec.Lerp(start, end, newF);
            ShapeDrawing.DrawLine(start, end, thickness, color);
            // Raylib.DrawLineEx(start, end, thickness, color.ToRayColor());
        }
    }

    public static void DrawOutlineBar(this Circle c, float thickness, float f, ColorRgba color) => DrawCircleSectorLines(c.Center, c.Radius, 0, 360 * f, thickness, color, false);
    public static void DrawOutlineBar(this Circle c, float startOffsetDeg, float thickness, float f, ColorRgba color) => DrawCircleSectorLines(c.Center, c.Radius, 0, 360 * f, startOffsetDeg, thickness, color, false);
    public static void DrawBar(this Rect rect, float f, ColorRgba barColorRgba, ColorRgba bgColorRgba, float left = 0f, float right = 1f, float top = 0f, float bottom = 0f)
    {
        f = 1.0f - f;
        Rect.Margins progressMargins = new(f * top, f * right, f * bottom, f * left);
        var progressRect = rect.ApplyMargins(progressMargins); // progressMargins.Apply(rect);
        rect.Draw(bgColorRgba);
        progressRect.Draw(barColorRgba);
    }
    public static void DrawBar(this Rect rect, Vector2 pivot, float angleDeg, float f, ColorRgba barColorRgba, ColorRgba bgColorRgba, float left = 0f, float right = 1f, float top = 0f, float bottom = 0f)
    {
        f = 1.0f - f;
        Rect.Margins progressMargins = new(f * top, f * right, f * bottom, f * left);
        var progressRect = rect.ApplyMargins(progressMargins); // progressMargins.Apply(rect);
        rect.Draw(pivot, angleDeg, bgColorRgba);
        progressRect.Draw(pivot, angleDeg, barColorRgba);
    }
    #endregion

}


    