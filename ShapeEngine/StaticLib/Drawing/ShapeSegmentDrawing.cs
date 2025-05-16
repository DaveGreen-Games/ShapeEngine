using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Random;

namespace ShapeEngine.StaticLib.Drawing;

public static class ShapeSegmentDrawing
{
    
    public static void DrawSegment(Vector2 start, Vector2 end, float thickness, ColorRgba color, float sideLengthFactor, LineCapType capType = LineCapType.None, int capPoints = 0)
    {
        var dir = end - start;
        var newEnd = start + dir * sideLengthFactor;
        DrawSegment(start, newEnd, thickness, color, capType, capPoints);
    }

    public static void DrawSegment(Vector2 start, Vector2 end, float thickness, ColorRgba color, LineCapType capType = LineCapType.None, int capPoints = 0)
    {
        if (thickness < LineDrawingInfo.LineMinThickness) thickness = LineDrawingInfo.LineMinThickness;
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

    /// <summary>
    /// Draws part of a line from start to end depending on f.
    /// </summary>
    /// <param name="start">The start point.</param>
    /// <param name="end">The end point.</param>
    /// <param name="f">The percentage of the line to draw. A negative value goes from end to start.</param>
    /// <param name="thickness">The thickness of the line.</param>
    /// <param name="color">The color of the line.</param>
    /// <param name="capType">The cap type of the line.</param>
    /// <param name="capPoints">How many points are used to draw the cap.</param>
    public static void DrawSegmentPercentage(Vector2 start, Vector2 end, float f, float thickness, ColorRgba color,
        LineCapType capType = LineCapType.None, int capPoints = 0)
    {
        if (f == 0) return;
        if (f < 0)
        {
            var newStart = end.Lerp(start, f * -1);
            DrawSegment(newStart, end, thickness, color, capType, capPoints);
        }
        else
        {
            var newEnd = start.Lerp(end, f);
            DrawSegment(start, newEnd, thickness, color, capType, capPoints);
        }
        
        
    }
    
    
    public static void DrawSegment(float startX, float startY, float endX, float endY, float thickness, 
        ColorRgba color, LineCapType capType = LineCapType.None, int capPoints = 0) 
        => DrawSegment(new(startX, startY), new(endX, endY), thickness, color, capType, capPoints);

    public static void DrawSegment(Vector2 start, Vector2 end, LineDrawingInfo info) => DrawSegment(start, end, info.Thickness, info.Color, info.CapType, info.CapPoints);
    /// <summary>
    /// Draws part of a line from start to end depending on f.
    /// </summary>
    /// <param name="start">The start point.</param>
    /// <param name="end">The end point.</param>
    /// <param name="f">The percentage of the line to draw. A negative value goes from end to start.</param>
    /// <param name="info">The line drawing info for how to draw the line.</param>
    public static void DrawSegmentPercentage(Vector2 start, Vector2 end, float f, LineDrawingInfo info) 
        => DrawSegmentPercentage(start, end, f, info.Thickness, info.Color, info.CapType, info.CapPoints);
    public static void DrawSegment(Vector2 start, Vector2 end, LineDrawingInfo info, float scaleFactor, float scaleOrigin = 0.5f)
    {
        var p = start.Lerp(end, scaleOrigin);
        var s = start - p;
        var e = end - p;

        var newStart = p + s * scaleFactor;
        var newEnd = p + e * scaleFactor;
        DrawSegment(newStart, newEnd, info);
    }

    public static void DrawSegment(float startX, float startY, float endX, float endY, LineDrawingInfo info) 
        => DrawSegment(new(startX, startY), new(endX, endY), info.Thickness, info.Color, info.CapType, info.CapPoints);
    
    public static void Draw(this Segment segment, float thickness, ColorRgba color, LineCapType capType = LineCapType.None, int capPoints = 0 ) 
        => DrawSegment(segment.Start, segment.End, thickness, color, capType, capPoints);
    
    public static void DrawPercentage(this Segment segment, float f, float thickness, ColorRgba color, LineCapType capType = LineCapType.None, int capPoints = 0 ) 
        => DrawSegmentPercentage(segment.Start, segment.End, f, thickness, color, capType, capPoints);
    public static void Draw(this Segment segment, LineDrawingInfo lineInfo) 
        => DrawSegment(segment.Start, segment.End, lineInfo);
    
    public static void DrawPercentage(this Segment segment, float f, LineDrawingInfo lineInfo) 
        => DrawSegmentPercentage(segment.Start, segment.End, f, lineInfo);
    public static void Draw(this Segment segment, float originF, float angleRad, LineDrawingInfo lineInfo)
    {
        if (angleRad != 0f)
        {
            segment.ChangeRotation(angleRad, originF).Draw(lineInfo);
            return;

        }
        
        DrawSegment(segment.Start, segment.End, lineInfo);
    }

    public static void Draw(this Segments segments, LineDrawingInfo lineInfo)
    {
        if (segments.Count <= 0) return;
        foreach (var seg in segments)
        {
            seg.Draw(lineInfo);
        }
    }
    public static void Draw(this Segments segments, float thickness, List<ColorRgba> colors, LineCapType capType = LineCapType.None, int capPoints = 0)
    {
        if (segments.Count <= 0 || colors.Count <= 0) return;
        LineDrawingInfo info = new(thickness, ColorRgba.White, capType, capPoints);
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
    
    public static void DrawSegmentGlow(Vector2 start, Vector2 end, float width, float endWidth, ColorRgba color, ColorRgba endColorRgba, int steps, LineCapType capType = LineCapType.None, int capPoints = 0)
    {
        float wStep = (endWidth - width) / steps;

        int rStep = (endColorRgba.R - color.R) / steps;
        int gStep = (endColorRgba.G - color.G) / steps;
        int bStep = (endColorRgba.B - color.B) / steps;
        int aStep = (endColorRgba.A - color.A) / steps;

        for (int i = steps; i >= 0; i--)
        {
            DrawSegment
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
    public static void DrawGlow(this Segment segment, float width, float endWidth, ColorRgba color, ColorRgba endColorRgba, int steps, LineCapType capType = LineCapType.None, int capPoints = 0)
    {
        DrawSegmentGlow(segment.Start, segment.End, width, endWidth, color, endColorRgba, steps, capType, capPoints);
    }
    public static void DrawGlow(this Segments segments, float width, float endWidth, ColorRgba color, ColorRgba endColorRgba, int steps, LineCapType capType = LineCapType.None, int capPoints = 0)
    {
        foreach (var seg in segments)
        {
            seg.DrawGlow(width, endWidth, color, endColorRgba, steps, capType, capPoints);
        }
    }

     
    /// <summary>
    /// Draws a segment scaled towards the origin.
    /// </summary>
    /// <param name="s">The segment to draw.</param>
    /// <param name="lineInfo">How to draw the lines.</param>
    /// <param name="sideScaleFactor">The scale factor for each side. 0f means no triangle is drawn, 1f means normal triangle is drawn,
    /// 0.5 means each side is half as long.</param>
    /// <param name="sideScaleOrigin">The point along the line to scale from in both directions.</param>
    public static void DrawScaled(this Segment s, LineDrawingInfo lineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0) return;
        if (sideScaleFactor >= 1)
        {
            s.Draw(lineInfo);
            return;
        }
        
        DrawSegment(s.Start, s.End, lineInfo, sideScaleFactor, sideScaleOrigin);
    }
    /// <summary>
    /// Draws a segment scaled towards the origin.
    /// </summary>
    /// <param name="s">The segment to draw.</param>
    /// <param name="angleRad">The rotation of the segment.</param>
    /// <param name="originF">Point to rotate the segment around. Value between 0 - 1. (0 = Start, 1 = End)</param>
    /// <param name="lineInfo">How to draw the lines.</param>
    /// <param name="sideScaleFactor">The scale factor for each side. 0f means no triangle is drawn, 1f means normal triangle is drawn,
    /// 0.5 means each side is half as long.</param>
    /// <param name="sideScaleOrigin">The point along the line to scale from in both directions.</param>
    public static void DrawScaled(this Segment s, float originF, float angleRad, LineDrawingInfo lineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0) return;
        if (sideScaleFactor >= 1)
        {
            s.Draw(originF, angleRad, lineInfo);
            return;
        }
        
        if(angleRad == 0f) DrawSegment(s.Start, s.End, lineInfo, sideScaleFactor, sideScaleOrigin);
        else
        {
            var origin = s.GetPoint(originF);
            var rStart = origin +  (s.Start - origin).Rotate(angleRad);
            var rEnd = origin + (s.End - origin).Rotate(angleRad);
            DrawSegment(rStart, rEnd, lineInfo, sideScaleFactor, sideScaleOrigin);
        }
    }

}