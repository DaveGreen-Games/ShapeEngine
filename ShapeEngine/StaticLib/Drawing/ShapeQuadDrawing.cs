using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.StaticLib.Drawing;

public static class ShapeQuadDrawing
{
     public static void DrawQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d, ColorRgba color)
    {
        Raylib.DrawTriangle(a, b, c, color.ToRayColor());
        Raylib.DrawTriangle(a, c, d, color.ToRayColor());
    }

    public static void DrawQuadLines(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        ShapeSegmentDrawing.DrawSegment(a, b, lineThickness, color, capType, capPoints);
        ShapeSegmentDrawing.DrawSegment(b, c, lineThickness, color, capType, capPoints);
        ShapeSegmentDrawing.DrawSegment(c, d, lineThickness, color, capType, capPoints);
        ShapeSegmentDrawing.DrawSegment(d, a, lineThickness, color, capType, capPoints);
    }
    public static void DrawQuadLines(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float lineThickness, ColorRgba color, float sideLengthFactor, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        var side1 = b - a;
        var end1 = a + side1 * sideLengthFactor;
        
        var side2 = c - b;
        var end2 = b + side2 * sideLengthFactor;
        
        var side3 = d - c;
        var end3 = c + side3 * sideLengthFactor;
        
        var side4 = a - d;
        var end4 = d + side4 * sideLengthFactor;
        
        ShapeSegmentDrawing.DrawSegment(a, end1, lineThickness, color, capType, capPoints);
        ShapeSegmentDrawing.DrawSegment(b, end2, lineThickness, color, capType, capPoints);
        ShapeSegmentDrawing.DrawSegment(c, end3, lineThickness, color, capType, capPoints);
        ShapeSegmentDrawing.DrawSegment(d, end4, lineThickness, color, capType, capPoints);
    }
    
    /// <summary>
    /// Draws a certain percentage of an outline.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="d"></param>
    /// <param name="f">The percentage of the outline to draw. Negative value reverses the direction (cw).
    /// Integer part can be used to change starting corner.
    /// 0.35 would start at corner a go in ccw direction and draw 35% of the outline.
    /// -2.7 would start at b (the third corner in cw direction) and draw in cw direction 70% of the outline.</param>
    /// <param name="lineThickness"></param>
    /// <param name="color"></param>
    /// <param name="capType"></param>
    /// <param name="capPoints"></param>
    public static void DrawQuadLinesPercentage(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float f, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
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
        
        startCorner = ShapeMath.Clamp(startCorner, 0, 3);
        
        if (startCorner == 0)
        {
            if (negative)
            {
               DrawQuadLinesPercentageHelper(a, d, c, b, percentage, lineThickness, color, capType, capPoints);
            }
            else
            {
                DrawQuadLinesPercentageHelper(a, b, c, d, percentage, lineThickness, color, capType, capPoints);
            }
        }
        else if (startCorner == 1)
        {
            if (negative)
            {
                DrawQuadLinesPercentageHelper(d, c, b, a, percentage, lineThickness, color, capType, capPoints);
            }
            else
            {
                DrawQuadLinesPercentageHelper(b, c, d, a, percentage, lineThickness, color, capType, capPoints);
            }
        }
        else if (startCorner == 2)
        {
            if (negative)
            {
                DrawQuadLinesPercentageHelper(c, b, a, d, percentage, lineThickness, color, capType, capPoints);
            }
            else
            {
                DrawQuadLinesPercentageHelper(c, d, a, b, percentage, lineThickness, color, capType, capPoints);
            }
        }
        else if (startCorner == 3)
        {
            if (negative)
            {
                DrawQuadLinesPercentageHelper(b, a, d, c, percentage, lineThickness, color, capType, capPoints);
            }
            else
            {
                DrawQuadLinesPercentageHelper(d, a, b, c, percentage, lineThickness, color, capType, capPoints);
            }
        }
    }
    
    public static void DrawQuadLines(Vector2 a, Vector2 b, Vector2 c, Vector2 d, LineDrawingInfo lineInfo)
    {
        ShapeSegmentDrawing.DrawSegment(a, b, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
        ShapeSegmentDrawing.DrawSegment(b, c, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
        ShapeSegmentDrawing.DrawSegment(c, d, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
        ShapeSegmentDrawing.DrawSegment(d, a, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
        
        // new Triangle(a, b, c).GetEdges().Draw(lineThickness, color);
    }
    public static void Draw(this Quad q, ColorRgba color) => DrawQuad(q.A, q.B, q.C, q.D, color);

    public static void DrawLines(this Quad q, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        DrawQuadLines(q.A, q.B, q.C, q.D, lineThickness, color, capType, capPoints);
    }
    public static void DrawLines(this Quad q, float lineThickness, ColorRgba color, float sideLengthFactor, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        DrawQuadLines(q.A, q.B, q.C, q.D, lineThickness, color, sideLengthFactor, capType, capPoints);
    }
    public static void DrawLines(this Quad q, LineDrawingInfo lineInfo) => DrawQuadLines(q.A, q.B, q.C, q.D, lineInfo);

    /// <summary>
    /// Draws a certain percentage of an outline.
    /// </summary>
    /// <param name="q"></param>
    /// <param name="f">The percentage of the outline to draw. Negative value reverses the direction (cw).
    /// Integer part can be used to change starting corner.
    /// 0.35 would start at corner a go in ccw direction and draw 35% of the outline.
    /// -2.7 would start at b (the third corner in cw direction) and draw in cw direction 70% of the outline.</param>
    /// <param name="lineThickness"></param>
    /// <param name="color"></param>
    /// <param name="capType"></param>
    /// <param name="capPoints"></param>
    public static void DrawLinesPercentage(this Quad q, float f, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        DrawQuadLinesPercentage(q.A, q.B, q.C, q.D, f, lineThickness, color, capType, capPoints);
    }

    /// <summary>
    /// Draws a certain percentage of an outline.
    /// </summary>
    /// <param name="q"></param>
    /// <param name="f">The percentage of the outline to draw. Negative value reverses the direction (cw).
    /// Integer part can be used to change starting corner.
    /// 0.35 would start at corner a go in ccw direction and draw 35% of the outline.
    /// -2.7 would start at b (the third corner in cw direction) and draw in cw direction 70% of the outline.</param>
    /// <param name="lineInfo"></param>
    public static void DrawLinesPercentage(this Quad q, float f, LineDrawingInfo lineInfo)
    {
        DrawQuadLinesPercentage(q.A, q.B, q.C, q.D, f, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
    }

    
    public static void DrawVertices(this Quad q, float vertexRadius, ColorRgba color, int circleSegments = 8)
    {
        ShapeCircleDrawing.DrawCircle(q.A, vertexRadius, color, circleSegments);
        ShapeCircleDrawing.DrawCircle(q.B, vertexRadius, color, circleSegments);
        ShapeCircleDrawing.DrawCircle(q.C, vertexRadius, color, circleSegments);
        ShapeCircleDrawing.DrawCircle(q.D, vertexRadius, color, circleSegments);
    }

    /// <summary>
    /// Draws a rect where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="q">The quad to draw.</param>
    /// <param name="lineInfo">How to draw the lines.</param>
    /// <param name="rotDeg">The rotation of the quad.</param>
    /// <param name="alignement">Alignement to rotate the quad.</param>
    /// <param name="sideScaleFactor">The scale factor for each side. 0f means no quad is drawn, 1f means normal quad is drawn,
    /// 0.5 means each side is half as long.</param>
    /// <param name="sideScaleOrigin">The point along the line to scale from in both directions.</param>
    public static void DrawLinesScaled(this Quad q, LineDrawingInfo lineInfo, float rotDeg, AnchorPoint alignement, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0) return;
        
        if(rotDeg != 0) q = q.ChangeRotation(rotDeg * ShapeMath.DEGTORAD, alignement);
        
        if (sideScaleFactor >= 1)
        {
            q.DrawLines(lineInfo);
            return;
        }
        
        ShapeSegmentDrawing.DrawSegment(q.A, q.B, lineInfo, sideScaleFactor, sideScaleOrigin);
        ShapeSegmentDrawing.DrawSegment(q.B, q.C, lineInfo, sideScaleFactor, sideScaleOrigin);
        ShapeSegmentDrawing.DrawSegment(q.C, q.D, lineInfo, sideScaleFactor, sideScaleOrigin);
        ShapeSegmentDrawing.DrawSegment(q.D, q.A, lineInfo, sideScaleFactor, sideScaleOrigin);
        
    }
    private static void DrawQuadLinesPercentageHelper(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, float percentage, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        var l1 = (p2 - p1).Length();
        var l2 = (p3 - p2).Length();
        var l3 = (p4 - p3).Length();
        var l4 = (p1 - p4).Length();
        var perimeterToDraw = (l1 + l2 + l3 + l4) * percentage;
        
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
        nextP = p4;
        if (perimeterToDraw < l3)
        {
            float p = perimeterToDraw / l3;
            nextP = curP.Lerp(nextP, p);
            ShapeSegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, capType, capPoints);
            return;
        }
        
        ShapeSegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, capType, capPoints);
        perimeterToDraw -= l3;
               
        // Draw fourth segment
        curP = nextP;
        nextP = p1;
        if (perimeterToDraw < l4)
        {
            float p = perimeterToDraw / l4;
            nextP = curP.Lerp(nextP, p);
        }
        ShapeSegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, capType, capPoints);
    }
   
}