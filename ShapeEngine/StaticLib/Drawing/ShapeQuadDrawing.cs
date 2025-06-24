using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.StaticLib.Drawing;

/// <summary>
/// Provides static methods for drawing quads (quadrilaterals) and their outlines, including partial outlines and vertex markers.
/// </summary>
/// <remarks>
/// This class contains utility methods for rendering quads with various options such as line thickness, color, partial outlines, and scaling.
/// </remarks>
public static class ShapeQuadDrawing
{
    /// <summary>
    /// Draws a filled quadrilateral using four vertices.
    /// </summary>
    /// <param name="a">The first vertex of the quad.</param>
    /// <param name="b">The second vertex of the quad.</param>
    /// <param name="c">The third vertex of the quad.</param>
    /// <param name="d">The fourth vertex of the quad.</param>
    /// <param name="color">The color to fill the quad.</param>
    public static void DrawQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d, ColorRgba color)
    {
        Raylib.DrawTriangle(a, b, c, color.ToRayColor());
        Raylib.DrawTriangle(a, c, d, color.ToRayColor());
    }

    /// <summary>
    /// Draws the outline of a quadrilateral with specified line thickness and style.
    /// </summary>
    /// <param name="a">The first vertex of the quad.</param>
    /// <param name="b">The second vertex of the quad.</param>
    /// <param name="c">The third vertex of the quad.</param>
    /// <param name="d">The fourth vertex of the quad.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="capType">The style of the line caps.</param>
    /// <param name="capPoints">The number of points used for the cap style.</param>
    public static void DrawQuadLines(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        ShapeSegmentDrawing.DrawSegment(a, b, lineThickness, color, capType, capPoints);
        ShapeSegmentDrawing.DrawSegment(b, c, lineThickness, color, capType, capPoints);
        ShapeSegmentDrawing.DrawSegment(c, d, lineThickness, color, capType, capPoints);
        ShapeSegmentDrawing.DrawSegment(d, a, lineThickness, color, capType, capPoints);
    }

    /// <summary>
    /// Draws the outline of a quadrilateral, scaling each side by a specified factor.
    /// </summary>
    /// <param name="a">The first vertex of the quad.</param>
    /// <param name="b">The second vertex of the quad.</param>
    /// <param name="c">The third vertex of the quad.</param>
    /// <param name="d">The fourth vertex of the quad.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="sideLengthFactor">The factor by which to scale each side (0 = no line, 1 = full length).</param>
    /// <param name="capType">The style of the line caps.</param>
    /// <param name="capPoints">The number of points used for the cap style.</param>
    /// <remarks>
    /// Each side is drawn from its starting vertex towards its ending vertex, scaled by <paramref name="sideLengthFactor"/>.
    /// </remarks>
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
    /// Draws a specified percentage of the outline of a quadrilateral.
    /// </summary>
    /// <param name="a">The first vertex of the quad.</param>
    /// <param name="b">The second vertex of the quad.</param>
    /// <param name="c">The third vertex of the quad.</param>
    /// <param name="d">The fourth vertex of the quad.</param>
    /// <param name="f">
    /// The percentage of the outline to draw. 
    /// <list type="bullet">
    /// <item><description>Negative value reverses the direction (clockwise).</description></item>
    /// <item><description>Integer part changes the starting corner (0 = a, 1 = b, etc.).</description></item>
    /// <item><description>Fractional part is the percentage of the outline to draw.</description></item>
    /// <item><description>Example: 0.35 starts at corner a, goes counter-clockwise, and draws 35% of the outline.</description></item>
    /// <item><description>Example: -2.7 starts at b (third corner in cw direction), draws 70% of the outline in cw direction.</description></item>
    /// </list>
    /// </param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="capType">The style of the line caps.</param>
    /// <param name="capPoints">The number of points used for the cap style.</param>
    /// <remarks>
    /// Useful for animating outlines or highlighting portions of a quad.
    /// </remarks>
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
    
    /// <summary>
    /// Draws the outline of a quadrilateral using a <see cref="LineDrawingInfo"/> structure.
    /// </summary>
    /// <param name="a">The first vertex of the quad.</param>
    /// <param name="b">The second vertex of the quad.</param>
    /// <param name="c">The third vertex of the quad.</param>
    /// <param name="d">The fourth vertex of the quad.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    public static void DrawQuadLines(Vector2 a, Vector2 b, Vector2 c, Vector2 d, LineDrawingInfo lineInfo)
    {
        ShapeSegmentDrawing.DrawSegment(a, b, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
        ShapeSegmentDrawing.DrawSegment(b, c, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
        ShapeSegmentDrawing.DrawSegment(c, d, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
        ShapeSegmentDrawing.DrawSegment(d, a, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
    }

    /// <summary>
    /// Draws a filled quadrilateral using the vertices of a <see cref="Quad"/>.
    /// </summary>
    /// <param name="q">The quad to draw.</param>
    /// <param name="color">The color to fill the quad.</param>
    public static void Draw(this Quad q, ColorRgba color) => DrawQuad(q.A, q.B, q.C, q.D, color);

    /// <summary>
    /// Draws the outline of a <see cref="Quad"/> with specified line thickness and style.
    /// </summary>
    /// <param name="q">The quad to outline.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="capType">The style of the line caps.</param>
    /// <param name="capPoints">The number of points used for the cap style.</param>
    public static void DrawLines(this Quad q, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        DrawQuadLines(q.A, q.B, q.C, q.D, lineThickness, color, capType, capPoints);
    }

    /// <summary>
    /// Draws the outline of a <see cref="Quad"/>, scaling each side by a specified factor.
    /// </summary>
    /// <param name="q">The quad to outline.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="sideLengthFactor">The factor by which to scale each side (0 = no line, 1 = full length).</param>
    /// <param name="capType">The style of the line caps.</param>
    /// <param name="capPoints">The number of points used for the cap style.</param>
    /// <remarks>
    /// Each side is drawn from its starting vertex towards its ending vertex, scaled by <paramref name="sideLengthFactor"/>.
    /// </remarks>
    public static void DrawLines(this Quad q, float lineThickness, ColorRgba color, float sideLengthFactor, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        DrawQuadLines(q.A, q.B, q.C, q.D, lineThickness, color, sideLengthFactor, capType, capPoints);
    }

    /// <summary>
    /// Draws the outline of a <see cref="Quad"/> using a <see cref="LineDrawingInfo"/> structure.
    /// </summary>
    /// <param name="q">The quad to outline.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    public static void DrawLines(this Quad q, LineDrawingInfo lineInfo) => DrawQuadLines(q.A, q.B, q.C, q.D, lineInfo);

    /// <summary>
    /// Draws a specified percentage of the outline of a <see cref="Quad"/>.
    /// </summary>
    /// <param name="q">The quad to outline.</param>
    /// <param name="f">
    /// The percentage of the outline to draw. 
    /// <list type="bullet">
    /// <item><description>Negative value reverses the direction (clockwise).</description></item>
    /// <item><description>Integer part changes the starting corner (0 = a, 1 = b, etc.).</description></item>
    /// <item><description>Fractional part is the percentage of the outline to draw.</description></item>
    /// <item><description>Example: 0.35 starts at corner a, goes counter-clockwise, and draws 35% of the outline.</description></item>
    /// <item><description>Example: -2.7 starts at b (third corner in cw direction), draws 70% of the outline in cw direction.</description></item>
    /// </list>
    /// </param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="capType">The style of the line caps.</param>
    /// <param name="capPoints">The number of points used for the cap style.</param>
    /// <remarks>
    /// Useful for animating outlines or highlighting portions of a quad.
    /// </remarks>
    public static void DrawLinesPercentage(this Quad q, float f, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        DrawQuadLinesPercentage(q.A, q.B, q.C, q.D, f, lineThickness, color, capType, capPoints);
    }

    /// <summary>
    /// Draws a specified percentage of the outline of a <see cref="Quad"/> using a <see cref="LineDrawingInfo"/> structure.
    /// </summary>
    /// <param name="q">The quad to outline.</param>
    /// <param name="f">
    /// The percentage of the outline to draw. 
    /// <list type="bullet">
    /// <item><description>Negative value reverses the direction (clockwise).</description></item>
    /// <item><description>Integer part changes the starting corner (0 = a, 1 = b, etc.).</description></item>
    /// <item><description>Fractional part is the percentage of the outline to draw.</description></item>
    /// <item><description>Example: 0.35 starts at corner a, goes counter-clockwise, and draws 35% of the outline.</description></item>
    /// <item><description>Example: -2.7 starts at b (third corner in cw direction), draws 70% of the outline in cw direction.</description></item>
    /// </list>
    /// </param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <remarks>
    /// Useful for animating outlines or highlighting portions of a quad.
    /// </remarks>
    public static void DrawLinesPercentage(this Quad q, float f, LineDrawingInfo lineInfo)
    {
        DrawQuadLinesPercentage(q.A, q.B, q.C, q.D, f, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
    }

    /// <summary>
    /// Draws circles at each vertex of a <see cref="Quad"/>.
    /// </summary>
    /// <param name="q">The quad whose vertices to draw.</param>
    /// <param name="vertexRadius">The radius of each vertex circle.</param>
    /// <param name="color">The color of the vertex circles.</param>
    /// <param name="circleSegments">The number of segments to use for each circle (default is 8).</param>
    /// <remarks>
    /// Useful for visualizing or highlighting the corners of a quad.
    /// </remarks>
    public static void DrawVertices(this Quad q, float vertexRadius, ColorRgba color, int circleSegments = 8)
    {
        ShapeCircleDrawing.DrawCircle(q.A, vertexRadius, color, circleSegments);
        ShapeCircleDrawing.DrawCircle(q.B, vertexRadius, color, circleSegments);
        ShapeCircleDrawing.DrawCircle(q.C, vertexRadius, color, circleSegments);
        ShapeCircleDrawing.DrawCircle(q.D, vertexRadius, color, circleSegments);
    }

    /// <summary>
    /// Draws the outline of a <see cref="Quad"/> where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="q">The quad to outline.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <param name="rotDeg">The rotation of the quad in degrees.</param>
    /// <param name="alignement">The anchor point for rotation alignment.</param>
    /// <param name="sideScaleFactor">
    /// <para>The scale factor for each side.</para>
    /// <list type="bullet">
    /// <item><description>0: No quad is drawn.</description></item>
    /// <item><description>1: The normal quad is drawn.</description></item>
    /// <item><description>0.5: Each side is half as long.</description></item>
    /// </list>
    /// </param>
    /// <param name="sideScaleOrigin">
    /// The point along the line to scale from, in both directions (0 to 1).
    /// <list type="bullet">
    /// <item><description>0: Start of Segment</description></item>
    /// <item><description>0.5: Center of Segment</description></item>
    /// <item><description>1: End of Segment</description></item>
    /// </list>
    /// </param>
    /// <remarks>
    /// Allows for dynamic scaling and rotation of quad outlines, useful for effects and animations.
    /// </remarks>
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