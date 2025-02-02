using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Lib.Drawing;

public static class CircleDrawing
{
     public static void DrawCircle(Vector2 center, float radius, ColorRgba color, int segments = 16)
    {
        if (segments < 3) segments = 3;
        Raylib.DrawCircleSector(center, radius, 0, 360, segments, color.ToRayColor());
    }
    public static void DrawCircle(Vector2 center, float radius, ColorRgba color, float rotDeg, int segments = 16)
    {
        if (segments < 3) segments = 3;
        Raylib.DrawCircleSector(center, radius, rotDeg, 360 + rotDeg, segments, color.ToRayColor());
    }

    
    public static void Draw(this Circle c, ColorRgba color, float rotDeg, int segments = 16)
    {
        if (segments < 3) segments = 3;
        Raylib.DrawCircleSector(c.Center, c.Radius, rotDeg, 360 + rotDeg, segments, color.ToRayColor());
    }
    public static void Draw(this Circle c, ColorRgba color) => DrawCircle(c.Center, c.Radius, color);
    public static void Draw(this Circle c, ColorRgba color, int segments) => DrawCircle(c.Center, c.Radius, color, segments);
    
    
    public static void DrawLines(this Circle c, float lineThickness, int sides, ColorRgba color) => Raylib.DrawPolyLinesEx(c.Center, sides, c.Radius, 0f, lineThickness * 2, color.ToRayColor());
    public static void DrawLines(this Circle c, LineDrawingInfo lineInfo, int sides) => DrawLines(c, lineInfo, 0f, sides);
    public static void DrawLines(this Circle c, LineDrawingInfo lineInfo, float rotDeg, int sides)
    {
        if (sides < 3) return;
        var angleStep = (2f * ShapeMath.PI) / sides;
        var rotRad = rotDeg * ShapeMath.DEGTORAD;
        for (int i = 0; i < sides; i++)
        {
            var nextIndex = (i + 1) % sides;
            var curP = c.Center + new Vector2(c.Radius, 0f).Rotate(rotRad + angleStep * i);
            var nextP = c.Center + new Vector2(c.Radius, 0f).Rotate(rotRad + angleStep * nextIndex);
            
            SegmentDrawing.DrawSegment(curP, nextP, lineInfo);
            
        }
    }
    
    /// <summary>
    /// Draws part of a circle outline depending on f.
    /// </summary>
    /// <param name="c">The circle parameters.</param>
    /// <param name="f">The percentage of the outline to draw. A positive value goes counter-clockwise
    /// and a negative value goes clockwise.</param>
    /// <param name="lineThickness">The line drawing parameters.</param>
    /// <param name="rotDeg">The rotation of the circle. The lower the resolution of the circle the more visible is rotation</param>
    /// <param name="sides">The resolution of the circle. The more sides are used the closer it represents a circle.</param>
    /// <param name="color">The color of the line.</param>
    /// <param name="lineCapType">The end cap type of the line.</param>
    /// <param name="capPoints">How many points are used to draw the end cap.</param>
    public static void DrawLinesPercentage(this Circle c, float f, float lineThickness, float rotDeg, int sides, ColorRgba color, LineCapType lineCapType, int capPoints)
    {
        if (sides < 3 || f == 0) return;
        
        DrawCircleLinesPercentage(c.Center, c.Radius, f, lineThickness, rotDeg, sides, color, lineCapType, capPoints);
    }
    
    /// <summary>
    /// Draws part of a circle outline depending on f.
    /// </summary>
    /// <param name="c">The circle parameters.</param>
    /// <param name="f">The percentage of the outline to draw. A positive value goes counter-clockwise
    /// and a negative value goes clockwise.</param>
    /// <param name="lineInfo">The line drawing parameters.</param>
    /// <param name="rotDeg">The rotation of the circle. The lower the resolution of the circle the more visible is rotation</param>
    /// <param name="sides">The resolution of the circle. The more sides are used the closer it represents a circle.</param>
    public static void DrawLinesPercentage(this Circle c, float f, LineDrawingInfo lineInfo, float rotDeg, int sides)
    {
        DrawLinesPercentage(c, f, lineInfo.Thickness, rotDeg, sides, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
    }
    
    public static void DrawLines(this Circle c, float lineThickness, float rotDeg, int sides, ColorRgba color) => Raylib.DrawPolyLinesEx(c.Center, sides, c.Radius, rotDeg, lineThickness * 2, color.ToRayColor());
    public static void DrawLines(this Circle c, float lineThickness, ColorRgba color, float sideLength = 8f)
    {
        int sides = GetCircleSideCount(c.Radius, sideLength);
        Raylib.DrawPolyLinesEx(c.Center, sides, c.Radius, 0f, lineThickness * 2, color.ToRayColor());
    }
    public static void DrawLines(this Circle c, LineDrawingInfo lineInfo, float rotDeg, float sideLength = 8f)
    {
        int sides = GetCircleSideCount(c.Radius, sideLength);
        DrawLines(c, lineInfo, rotDeg, sides);
    }
    public static void DrawLinesPercentage(this Circle c, float f, float lineThickness, ColorRgba color, float sideLength = 8f)
    {
        if (f == 0) return;
        int sides = GetCircleSideCount(c.Radius, sideLength);
        DrawLinesPercentage(c, f, lineThickness, 0f, sides, color, LineCapType.None, 0);
    }
    public static void DrawLinesPercentage(this Circle c, float f, float lineThickness, float rotDeg, ColorRgba color, LineCapType capType, int capPoints, float sideLength = 8f)
    {
        if (f == 0) return;
        int sides = GetCircleSideCount(c.Radius, sideLength);
        DrawLinesPercentage(c, f, lineThickness, rotDeg, sides, color, capType, capPoints);
    }
    public static void DrawLinesPercentage(this Circle c, float f, LineDrawingInfo lineInfo, float rotDeg, float sideLength = 8f)
    {
        if (f == 0) return;
        int sides = GetCircleSideCount(c.Radius, sideLength);
        DrawLinesPercentage(c, f, lineInfo, rotDeg, sides);
    }
    
    /// <summary>
    /// Draws a circle where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="lineInfo">How to draw the lines.</param>
    /// <param name="sides">How many sides the circle should be drawn with.</param>
    /// <param name="sideScaleFactor">The scale factor for each side. 0f means no circle is drawn, 1f means normal circle is drawn,
    /// 0.5 means each side is half as long.</param>
    /// <param name="sideScaleOrigin">The point along the line to scale from in both directions.</param>
    public static void DrawLinesScaled(this Circle c, LineDrawingInfo lineInfo, int sides, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        DrawLinesScaled(c, lineInfo, 0f, sides, sideScaleFactor, sideScaleOrigin);
    }
    
    /// <summary>
    /// Draws a circle where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="lineInfo">How to draw the lines.</param>
    /// <param name="rotDeg">The rotation of the circle.</param>
    /// <param name="sides">How many sides the circle should be drawn with.</param>
    /// <param name="sideScaleFactor">The scale factor for each side. 0f means no circle is drawn, 1f means normal circle is drawn,
    /// 0.5 means each side is half as long.</param>
    /// <param name="sideScaleOrigin">The point along the line to scale from in both directions.</param>
    public static void DrawLinesScaled(this Circle c, LineDrawingInfo lineInfo, float rotDeg, int sides, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0f) return;
        if (sideScaleFactor >= 1f)
        {
            DrawLines(c, lineInfo, sides);
            return;
        }
        
        var angleStep = (2f * ShapeMath.PI) / sides;
        var rotRad = rotDeg * ShapeMath.DEGTORAD;
        
        for (int i = 0; i < sides; i++)
        {
            var nextIndex = (i + 1) % sides;
            var start = c.Center + new Vector2(c.Radius, 0f).Rotate(rotRad + angleStep * i);
            var end = c.Center + new Vector2(c.Radius, 0f).Rotate(rotRad + angleStep * nextIndex);
            
            SegmentDrawing.DrawSegment(start, end, lineInfo, sideScaleFactor, sideScaleOrigin);
            
        }
    }
    
    /// <summary>
    /// Draws part of a circle outline depending on f.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="f">The percentage of the outline to draw. A positive value goes counter-clockwise
    /// and a negative value goes clockwise.</param>
    /// <param name="lineThickness">The line drawing parameters.</param>
    /// <param name="rotDeg">The rotation of the circle. The lower the resolution of the circle the more visible is rotation</param>
    /// <param name="sides">The resolution of the circle. The more sides are used the closer it represents a circle.</param>
    /// <param name="color">The color of the line.</param>
    /// <param name="lineCapType">The end cap type of the line.</param>
    /// <param name="capPoints">How many points are used to draw the end cap.</param>
    public static void DrawCircleLinesPercentage(Vector2 center, float radius, float f, float lineThickness, float rotDeg, int sides, ColorRgba color, LineCapType lineCapType, int capPoints)
    {
        
        if (sides < 3 || f == 0 || radius <= 0) return;
        
        float angleStep; // = (2f * ShapeMath.PI) / sides;
        float percentage; // = ShapeMath.Clamp(negative ? f * -1 : f, 0f, 1f);
        if (f < 0)
        {
            angleStep = (-2f * ShapeMath.PI) / sides;
            percentage = ShapeMath.Clamp(-f, 0f, 1f);
        }
        else
        {
            angleStep = (2f * ShapeMath.PI) / sides;
            percentage = ShapeMath.Clamp(f, 0f, 1f);
        }
        
        var rotRad = rotDeg * ShapeMath.DEGTORAD;
        var perimeter = Circle.GetCircumference(radius);
        var sideLength = perimeter / sides;
        var perimeterToDraw = perimeter * percentage;
        for (int i = 0; i < sides; i++)
        {
            var nextIndex = (i + 1) % sides;
            var curP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * i);
            var nextP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * nextIndex);

            if (sideLength > perimeterToDraw)
            {
                nextP = curP.Lerp(nextP, perimeterToDraw / sideLength);
                SegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, lineCapType, capPoints);
                return;
            }
            else
            {
                SegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, lineCapType, capPoints);
                perimeterToDraw -= sideLength;
            }
            
            
            
        }
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
        RectDrawing.DrawRect(center - new Vector2(radius, radius), center + new Vector2(radius, radius), color);
    }
    
    public static void DrawCircleLines(Vector2 center, float radius, float lineThickness, int sides, ColorRgba color) 
        => Raylib.DrawPolyLinesEx(center, sides, radius, 0f, lineThickness * 2, color.ToRayColor());
    public static void DrawCircleLines(Vector2 center, float radius, LineDrawingInfo lineInfo, int sides) 
        => DrawCircleLines(center, radius, lineInfo, 0f, sides);
    public static void DrawCircleLinesPercentage(Vector2 center, float radius, float f, LineDrawingInfo lineInfo, float rotDeg, int sides) 
        => DrawCircleLinesPercentage(center, radius, f, lineInfo.Thickness, rotDeg, sides, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
    public static void DrawCircleLines(Vector2 center, float radius, float lineThickness, float rotDeg, int sides, ColorRgba color) 
        => Raylib.DrawPolyLinesEx(center, sides, radius, rotDeg, lineThickness * 2, color.ToRayColor());
    public static void DrawCircleLines(Vector2 center, float radius, LineDrawingInfo lineInfo, float rotDeg, int sides)
    {
        if (sides < 3) return;
        var angleStep = (2f * ShapeMath.PI) / sides;
        var rotRad = rotDeg * ShapeMath.DEGTORAD;
        
        for (int i = 0; i < sides; i++)
        {
            var nextIndex = (i + 1) % sides;
            var curP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * i);
            var nextP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * nextIndex);
            
            SegmentDrawing.DrawSegment(curP, nextP, lineInfo);
            
        }
    }
    public static void DrawCircleLines(Vector2 center, float radius, float lineThickness, ColorRgba color, float sideLength = 8f)
    {
        int sides = GetCircleSideCount(radius, sideLength);
        Raylib.DrawPolyLinesEx(center, sides, radius, 0f, lineThickness * 2, color.ToRayColor());
    }
    public static void DrawCircleLines(Vector2 center, float radius, LineDrawingInfo lineInfo, float rotDeg, float sideLength = 8f)
    {
        int sides = GetCircleSideCount(radius, sideLength);
        DrawCircleLines(center, radius, lineInfo, rotDeg, sides);
    }
    public static void DrawCircleLinesPercentage(Vector2 center, float radius, float f, LineDrawingInfo lineInfo, float rotDeg, float sideLength = 8f)
    {
        int sides = GetCircleSideCount(radius, sideLength);
        DrawCircleLinesPercentage(center, radius, f, lineInfo, rotDeg, sides);
    }
    
    public static void DrawSector(this Circle c, float startAngleDeg, float endAngleDeg, int segments, ColorRgba color)
    {
        Raylib.DrawCircleSector(c.Center, c.Radius, startAngleDeg, endAngleDeg, segments, color.ToRayColor());
    }
    public static void DrawCircleSector(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, int segments, ColorRgba color)
    {
        Raylib.DrawCircleSector(center, radius, startAngleDeg, endAngleDeg, segments, color.ToRayColor());
    }
    
    public static void DrawSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, LineDrawingInfo lineInfo, bool closed = true, float sideLength = 8f)
    {
        DrawCircleSectorLines(c.Center, c.Radius, startAngleDeg, endAngleDeg, lineInfo, closed, sideLength);
    }
    public static void DrawSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, LineDrawingInfo lineInfo, bool closed = true, float sideLength = 8f)
    {
        DrawCircleSectorLines(c.Center, c.Radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, lineInfo, closed, sideLength);
    }
    public static void DrawSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, int sides, LineDrawingInfo lineInfo, bool closed = true)
    {
        DrawCircleSectorLines(c.Center, c.Radius, startAngleDeg, endAngleDeg, sides, lineInfo, closed);
    }
    public static void DrawSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, int sides, LineDrawingInfo lineInfo, bool closed = true)
    {
        DrawCircleSectorLines(c.Center, c.Radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, sides, lineInfo, closed);
    }
    
    /// <summary>
    /// Draws a circle where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="lineInfo">How to draw the lines.</param>
    /// <param name="startAngleDeg">The starting rotation of the sector.</param>
    /// <param name="endAngleDeg">The end rotation of the sector.</param>
    /// <param name="rotOffsetDeg">Rotation offset for the sector.</param>
    /// <param name="sides">How many sides the circle should be drawn with.</param>
    /// <param name="sideScaleFactor">The scale factor for each side. 0f means no circle is drawn, 1f means normal circle is drawn,
    /// 0.5 means each side is half as long.</param>
    /// <param name="sideScaleOrigin">The point along the line to scale from in both directions.</param>
    /// <param name="closed">Should the sector be closed.</param>
    public static void DrawSectorLinesScaled(this Circle c, LineDrawingInfo lineInfo, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, int sides, float sideScaleFactor, float sideScaleOrigin = 0.5f, bool closed = true)
    {
        if (sideScaleFactor <= 0f) return;
        if (sideScaleFactor >= 1f)
        {
            DrawSectorLines(c, startAngleDeg, endAngleDeg, rotOffsetDeg, sides, lineInfo, closed);
            return;
        }
        
        float startAngleRad = (startAngleDeg + rotOffsetDeg) * ShapeMath.DEGTORAD;
        float endAngleRad = (endAngleDeg + rotOffsetDeg) * ShapeMath.DEGTORAD;
        float anglePiece = endAngleRad - startAngleRad;
        float angleStep = anglePiece / sides;
        if (closed)
        {
            var sectorStart = c.Center + (ShapeVec.Right() * c.Radius).Rotate(startAngleRad);
            SegmentDrawing.DrawSegment(c.Center, sectorStart, lineInfo, sideScaleFactor, sideScaleOrigin);
        
            var sectorEnd = c.Center + (ShapeVec.Right() * c.Radius).Rotate(endAngleRad);
            SegmentDrawing.DrawSegment(c.Center, sectorEnd, lineInfo, sideScaleFactor, sideScaleOrigin);
        }
        for (int i = 0; i < sides; i++)
        {
            var nextIndex = (i + 1) % sides;
            var start = c.Center + new Vector2(c.Radius, 0f).Rotate(startAngleRad + angleStep * i);
            var end = c.Center + new Vector2(c.Radius, 0f).Rotate(startAngleRad + angleStep * nextIndex);
            
            SegmentDrawing.DrawSegment(start, end, lineInfo, sideScaleFactor, sideScaleOrigin);
            
        }
    }

    public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, LineDrawingInfo lineInfo, bool closed = true, float sideLength = 8f)
    {
        float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
        float endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
        float anglePiece = endAngleRad - startAngleRad;
        int sides = GetCircleArcSideCount(radius, MathF.Abs(anglePiece * ShapeMath.RADTODEG), sideLength);
        float angleStep = anglePiece / sides;
        if (closed)
        {
            var sectorStart = center + (ShapeVec.Right() * radius + new Vector2(lineInfo.Thickness / 2, 0)).Rotate(startAngleRad);
            SegmentDrawing.DrawSegment(center, sectorStart, lineInfo);

            var sectorEnd = center + (ShapeVec.Right() * radius + new Vector2(lineInfo.Thickness / 2, 0)).Rotate(endAngleRad);
            SegmentDrawing.DrawSegment(center, sectorEnd, lineInfo);
        }
        for (var i = 0; i < sides; i++)
        {
            var start = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * i);
            var end = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * (i + 1));
            SegmentDrawing.DrawSegment(start, end, lineInfo);
        }
    }
    public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, LineDrawingInfo lineInfo, bool closed = true, float sideLength = 8f)
    {
        DrawCircleSectorLines(center, radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, lineInfo, closed, sideLength);
    }
    public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, int sides, LineDrawingInfo lineInfo, bool closed = true)
    {
        float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
        float endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
        float anglePiece = endAngleDeg - startAngleRad;
        float angleStep = MathF.Abs(anglePiece) / sides;
        if (closed)
        {
            var sectorStart = center + (ShapeVec.Right() * radius + new Vector2(lineInfo.Thickness / 2, 0)).Rotate(startAngleRad);
            SegmentDrawing.DrawSegment(center, sectorStart, lineInfo);

            var sectorEnd = center + (ShapeVec.Right() * radius + new Vector2(lineInfo.Thickness / 2, 0)).Rotate(endAngleRad);
            SegmentDrawing.DrawSegment(center, sectorEnd, lineInfo);
        }
        for (var i = 0; i < sides; i++)
        {
            var start = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * i);
            var end = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * (i + 1));
            SegmentDrawing.DrawSegment(start, end, lineInfo);
        }
    }
    public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, int sides, LineDrawingInfo lineInfo, bool closed = true)
    {
        DrawCircleSectorLines(center, radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, sides, lineInfo, closed);
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
            SegmentDrawing.DrawSegment(center, sectorStart, lineThickness, color, LineCapType.CappedExtended, 4);

            var sectorEnd = center + (ShapeVec.Right() * radius + new Vector2(lineThickness / 2, 0)).Rotate(endAngleRad);
            SegmentDrawing.DrawSegment(center, sectorEnd, lineThickness, color, LineCapType.CappedExtended, 4);
        }
        for (var i = 0; i < sides; i++)
        {
            var start = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * i);
            var end = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * (i + 1));
            SegmentDrawing.DrawSegment(start, end, lineThickness, color, LineCapType.CappedExtended, 4);
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
            SegmentDrawing.DrawSegment(center, sectorStart, lineThickness, color, LineCapType.CappedExtended, 2);

            var sectorEnd = center + (ShapeVec.Right() * radius + new Vector2(lineThickness / 2, 0)).Rotate(endAngleRad);
            SegmentDrawing.DrawSegment(center, sectorEnd, lineThickness, color, LineCapType.CappedExtended, 2);
        }
        for (var i = 0; i < sides; i++)
        {
            var start = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * i);
            var end = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * (i + 1));
            SegmentDrawing.DrawSegment(start, end, lineThickness, color, LineCapType.CappedExtended, 2);
        }
    }
    public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, int sides, float lineThickness, ColorRgba color, bool closed = true)
    {
        DrawCircleSectorLines(center, radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, sides, lineThickness, color, closed);
    }

    public static void DrawCircleCheckeredLines(Vector2 pos, AnchorPoint alignement, float radius, float spacing, float lineThickness, float angleDeg, ColorRgba lineColorRgba, ColorRgba bgColorRgba, int circleSegments)
    {

        float maxDimension = radius;
        var size = new Vector2(radius, radius) * 2f;
        var aVector = alignement.ToVector2() * size;
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
            SegmentDrawing.DrawSegment(start, end, lineThickness, lineColorRgba);
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
            SegmentDrawing.DrawSegment(start, end, lineThickness, lineColorRgba);
            cur.X += spacing;
        }

    }
    
    public static int GetCircleSideCount(float radius, float maxLength = 10f)
    {
        float circumference = 2.0f * ShapeMath.PI * radius;
        return (int)MathF.Max(circumference / maxLength, 5);
    }
    public static int GetCircleArcSideCount(float radius, float angleDeg, float maxLength = 10f)
    {
        float circumference = 2.0f * ShapeMath.PI * radius * (angleDeg / 360f);
        return (int)MathF.Max(circumference / maxLength, 1);
    }
    // private static float TransformAngleDegToRaylib(float angleDeg) { return 450f - angleDeg; }
    
}