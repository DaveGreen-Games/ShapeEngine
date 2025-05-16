using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.StaticLib.Drawing;

public static class ShapeTriangleDrawing
{
    public static void DrawTriangle(Vector2 a, Vector2 b, Vector2 c, ColorRgba color) => Raylib.DrawTriangle(a, b, c, color.ToRayColor());

    
    public static void DrawTriangleLines(Vector2 a, Vector2 b, Vector2 c, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        ShapeSegmentDrawing.DrawSegment(a, b, lineThickness, color, capType, capPoints);
        ShapeSegmentDrawing.DrawSegment(b, c, lineThickness, color, capType, capPoints);
        ShapeSegmentDrawing.DrawSegment(c, a, lineThickness, color, capType, capPoints);
    }
    public static void DrawTriangleLines(Vector2 a, Vector2 b, Vector2 c, float lineThickness, ColorRgba color, float sideLengthFactor, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        var side1 = b - a;
        var end1 = a + side1 * sideLengthFactor;
        
        var side2 = c - b;
        var end2 = b + side2 * sideLengthFactor;
        
        var side3 = a - c;
        var end3 = c + side3 * sideLengthFactor;
        
        ShapeSegmentDrawing.DrawSegment(a, end1, lineThickness, color, capType, capPoints);
        ShapeSegmentDrawing.DrawSegment(b, end2, lineThickness, color, capType, capPoints);
        ShapeSegmentDrawing.DrawSegment(c, end3, lineThickness, color, capType, capPoints);
    }
    public static void DrawTriangleLines(Vector2 a, Vector2 b, Vector2 c, LineDrawingInfo lineInfo)
    {
        ShapeSegmentDrawing.DrawSegment(a, b, lineInfo);
        ShapeSegmentDrawing.DrawSegment(b, c, lineInfo);
        ShapeSegmentDrawing.DrawSegment(c, a, lineInfo);
        
    }

    
    public static void Draw(this Triangle t, ColorRgba color) => Raylib.DrawTriangle(t.A, t.B, t.C, color.ToRayColor());

    public static void DrawLines(this Triangle t, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        DrawTriangleLines(t.A, t.B, t.C, lineThickness, color, capType, capPoints);
    }
    public static void DrawLines(this Triangle t, float lineThickness, ColorRgba color, float sideLengthFactor,  LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        DrawTriangleLines(t.A, t.B, t.C, lineThickness, color, sideLengthFactor, capType, capPoints);
    }
    
    public static void DrawLines(this Triangle t, LineDrawingInfo lineInfo) => DrawTriangleLines(t.A, t.B, t.C, lineInfo);
    public static void DrawLines(this Triangle t, LineDrawingInfo lineInfo, float rotDeg, Vector2 rotOrigin)
    {
        t = t.ChangeRotation(rotDeg * ShapeMath.DEGTORAD, rotOrigin);
        DrawTriangleLines(t.A, t.B, t.C, lineInfo);
    }

    
    public static void DrawVertices(this Triangle t, float vertexRadius, ColorRgba color, int circleSegments = 8)
    {
        ShapeCircleDrawing.DrawCircle(t.A, vertexRadius, color, circleSegments);
        ShapeCircleDrawing.DrawCircle(t.B, vertexRadius, color, circleSegments);
        ShapeCircleDrawing.DrawCircle(t.C, vertexRadius, color, circleSegments);
    }
    
    public static void Draw(this Triangulation triangles, ColorRgba color) { foreach (var t in triangles) t.Draw(color); }

    
    public static void DrawLines(this Triangulation triangles, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        foreach (var t in triangles) t.DrawLines(lineThickness, color, capType, capPoints);
    }
    public static void DrawLines(this Triangulation triangles, LineDrawingInfo lineInfo)
    {
        foreach (var t in triangles) t.DrawLines(lineInfo);
    }

    /// <summary>
    /// Draws a certain percentage of an outline.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="f">The percentage of the outline to draw. Negative value reverses the direction (cw).
    /// Integer part can be used to change starting corner.
    /// 0.35 would start at corner a go in ccw direction and draw 35% of the outline.
    /// -2.7 would start at b (the third corner in cw direction) and draw in cw direction 70% of the outline.</param>
    /// <param name="lineThickness"></param>
    /// <param name="color"></param>
    /// <param name="capType"></param>
    /// <param name="capPoints"></param>
    public static void DrawTriangleLinesPercentage(Vector2 a, Vector2 b, Vector2 c, float f, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (f == 0) return;

        bool negative = false;
        if (f < 0)
        {
            negative = true;
            f *= -1;
        }
        
        int startCorner = (int)f;
        float percentage = f - startCorner;
        if (percentage <= 0) return;
        
        startCorner = ShapeMath.Clamp(startCorner, 0, 2);
        
        if (startCorner == 0)
        {
            if (negative)
            {
                DrawTriangleLinesPercentageHelper(a, c, b, percentage, lineThickness, color, capType, capPoints);
            }
            else
            {
                DrawTriangleLinesPercentageHelper(a, b, c, percentage, lineThickness, color, capType, capPoints);
            }
        }
        else if (startCorner == 1)
        {
            if (negative)
            {
                DrawTriangleLinesPercentageHelper(b, a, c,  percentage, lineThickness, color, capType, capPoints);
            }
            else
            {
                DrawTriangleLinesPercentageHelper(b, c, a,  percentage, lineThickness, color, capType, capPoints);
            }
        }
        else if (startCorner == 2)
        {
            if (negative)
            {
                DrawTriangleLinesPercentageHelper(c, b, a, percentage, lineThickness, color, capType, capPoints);
            }
            else
            {
                DrawTriangleLinesPercentageHelper(c, a, b, percentage, lineThickness, color, capType, capPoints);
            }
        }
    }
    /// <summary>
    /// Draws a certain percentage of an outline.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="f">The percentage of the outline to draw. Negative value reverses the direction (cw).
    /// Integer part can be used to change starting corner.
    /// 0.35 would start at corner a go in ccw direction and draw 35% of the outline.
    /// -2.7 would start at b (the third corner in cw direction) and draw in cw direction 70% of the outline.</param>
    /// <param name="lineInfo"></param>
    public static void DrawTriangleLinesPercentage(Vector2 a, Vector2 b, Vector2 c, float f, LineDrawingInfo lineInfo)
    {
        DrawTriangleLinesPercentage(a, b, c, f, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
    }
    
    /// <summary>
    /// Draws a certain percentage of an outline.
    /// </summary>
    /// <param name="t"></param>
    /// <param name="f">The percentage of the outline to draw. Negative value reverses the direction (cw).
    /// Integer part can be used to change starting corner.
    /// 0.35 would start at corner a go in ccw direction and draw 35% of the outline.
    /// -2.7 would start at b (the third corner in cw direction) and draw in cw direction 70% of the outline.</param>
    /// <param name="lineThickness"></param>
    /// <param name="color"></param>
    /// <param name="capType"></param>
    /// <param name="capPoints"></param>
    public static void DrawLinesPercentage(this Triangle t, float f, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        DrawTriangleLinesPercentage(t.A, t.B, t.C, f, lineThickness, color, capType, capPoints);
    }
    /// <summary>
    /// Draws a certain percentage of an outline.
    /// </summary>
    /// <param name="t"></param>
    /// <param name="f">The percentage of the outline to draw. Negative value reverses the direction (cw).
    /// Integer part can be used to change starting corner.
    /// 0.35 would start at corner a go in ccw direction and draw 35% of the outline.
    /// -2.7 would start at b (the third corner in cw direction) and draw in cw direction 70% of the outline.</param>
    /// <param name="lineInfo"></param>
    public static void DrawLinesPercentage(this Triangle t, float f, LineDrawingInfo lineInfo)
    {
        DrawTriangleLinesPercentage(t.A, t.B, t.C, f, lineInfo);
    }
    /// <summary>
    /// Draws a certain percentage of an outline.
    /// </summary>
    /// <param name="t"></param>
    /// <param name="f">The percentage of the outline to draw. Negative value reverses the direction (cw).
    /// Integer part can be used to change starting corner.
    /// 0.35 would start at corner a go in ccw direction and draw 35% of the outline.
    /// -2.7 would start at b (the third corner in cw direction) and draw in cw direction 70% of the outline.</param>
    /// <param name="lineInfo"></param>
    /// <param name="rotDeg"></param>
    /// <param name="rotOrigin"> Origin is in absolute space.</param>
    public static void DrawLinesPercentage(this Triangle t, float f, LineDrawingInfo lineInfo, float rotDeg, Vector2 rotOrigin)
    {
        t = t.ChangeRotation(rotDeg * ShapeMath.DEGTORAD, rotOrigin);
        DrawTriangleLinesPercentage(t.A, t.B, t.C, f, lineInfo);
    }

    
    
    /// <summary>
    /// Draws a rect where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="t">The triangle to draw.</param>
    /// <param name="lineInfo">How to draw the lines.</param>
    /// <param name="rotDeg">The rotation of the triangle.</param>
    /// <param name="rotOrigin">Point to rotate the triangle around.</param>
    /// <param name="sideScaleFactor">The scale factor for each side. 0f means no triangle is drawn, 1f means normal triangle is drawn,
    /// 0.5 means each side is half as long.</param>
    /// <param name="sideScaleOrigin">The point along the line to scale from in both directions.</param>
    public static void DrawLinesScaled(this Triangle t, LineDrawingInfo lineInfo, float rotDeg, Vector2 rotOrigin, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0) return;
        if (sideScaleFactor >= 1)
        {
            t.DrawLines(lineInfo, rotDeg, rotOrigin);
            return;
        }
        
        if(rotDeg != 0) t = t.ChangeRotation(rotDeg * ShapeMath.DEGTORAD, rotOrigin);
        
        ShapeSegmentDrawing.DrawSegment(t.A, t.B, lineInfo, sideScaleFactor, sideScaleOrigin);
        ShapeSegmentDrawing.DrawSegment(t.B, t.C, lineInfo, sideScaleFactor, sideScaleOrigin);
        ShapeSegmentDrawing.DrawSegment(t.C, t.A, lineInfo, sideScaleFactor, sideScaleOrigin);
        
    }
    private static void DrawTriangleLinesPercentageHelper(Vector2 p1, Vector2 p2, Vector2 p3, float percentage, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        var l1 = (p2 - p1).Length();
        var l2 = (p3 - p2).Length();
        var l3 = (p1 - p3).Length();
        var perimeterToDraw = (l1 + l2 + l3) * percentage;
        
        // Draw first segment
        var curP = p1;
        var nextP = p2;
        if (perimeterToDraw < l1)
        {
            float p = perimeterToDraw / l1;
            nextP = curP.Lerp(nextP, p);
            ShapeSegmentDrawing.DrawSegment(curP, nextP, lineThickness, color);
            return;
        }
                
        ShapeSegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, capType, capPoints);
        perimeterToDraw -= l1;
                
        // Draw second segment
        curP = nextP;
        nextP = p3;
        if (perimeterToDraw < l2)
        {
            float p = perimeterToDraw / l2;
            nextP = curP.Lerp(nextP, p);
            ShapeSegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, capType, capPoints);
            return;
        }
                
        ShapeSegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, capType, capPoints);
        perimeterToDraw -= l2;
                
        // Draw third segment
        curP = nextP;
        nextP = p1;
        if (perimeterToDraw < l3)
        {
            float p = perimeterToDraw / l3;
            nextP = curP.Lerp(nextP, p);
        }
        
        ShapeSegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, capType, capPoints);
       
    }

}