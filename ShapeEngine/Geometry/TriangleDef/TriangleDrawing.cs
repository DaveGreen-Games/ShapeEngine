using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.TriangleDef;

/// <summary>
/// Provides static methods for drawing triangles and collections of triangles with various styles and options.
/// </summary>
/// <remarks>
/// This class contains extension methods for drawing <see cref="Triangle"/> and <see cref="Triangulation"/> objects,
/// as well as static methods for drawing triangles using raw vertex data. Supports filled, outlined, partial, and scaled outlines.
/// </remarks>
public static class TriangleDrawing
{
    /// <summary>
    /// Draws a filled triangle using the specified vertices and color.
    /// </summary>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <param name="color">The color to fill the triangle with.</param>
    public static void DrawTriangle(Vector2 a, Vector2 b, Vector2 c, ColorRgba color) => Raylib.DrawTriangle(a, b, c, color.ToRayColor());

    /// <summary>
    /// Draws the outline of a triangle with specified line thickness and style.
    /// </summary>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="capType">The style of the line caps.</param>
    /// <param name="capPoints">The number of points used for the cap style.</param>
    public static void DrawTriangleLines(Vector2 a, Vector2 b, Vector2 c, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        SegmentDrawing.DrawSegment(a, b, lineThickness, color, capType, capPoints);
        SegmentDrawing.DrawSegment(b, c, lineThickness, color, capType, capPoints);
        SegmentDrawing.DrawSegment(c, a, lineThickness, color, capType, capPoints);
    }

    /// <summary>
    /// Draws the outline of a triangle with each side scaled by a factor.
    /// </summary>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="sideLengthFactor">The factor by which to scale each side <c>(0 = no triangle, 1 = full side length).</c></param>
    /// <param name="capType">The style of the line caps.</param>
    /// <param name="capPoints">The number of points used for the cap style.</param>
    /// <remarks>
    /// Each side is drawn from its starting vertex towards the next, scaled by <paramref name="sideLengthFactor"/>.
    /// </remarks>
    public static void DrawTriangleLines(Vector2 a, Vector2 b, Vector2 c, float lineThickness, ColorRgba color, float sideLengthFactor, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        var side1 = b - a;
        var end1 = a + side1 * sideLengthFactor;

        var side2 = c - b;
        var end2 = b + side2 * sideLengthFactor;

        var side3 = a - c;
        var end3 = c + side3 * sideLengthFactor;

        SegmentDrawing.DrawSegment(a, end1, lineThickness, color, capType, capPoints);
        SegmentDrawing.DrawSegment(b, end2, lineThickness, color, capType, capPoints);
        SegmentDrawing.DrawSegment(c, end3, lineThickness, color, capType, capPoints);
    }

    /// <summary>
    /// Draws the outline of a triangle using a <see cref="LineDrawingInfo"/> object for style.
    /// </summary>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    public static void DrawTriangleLines(Vector2 a, Vector2 b, Vector2 c, LineDrawingInfo lineInfo)
    {
        SegmentDrawing.DrawSegment(a, b, lineInfo);
        SegmentDrawing.DrawSegment(b, c, lineInfo);
        SegmentDrawing.DrawSegment(c, a, lineInfo);
    }

    /// <summary>
    /// Draws a filled triangle using the specified <see cref="Triangle"/> and color.
    /// </summary>
    /// <param name="t">The triangle to draw.</param>
    /// <param name="color">The color to fill the triangle with.</param>
    public static void Draw(this Triangle t, ColorRgba color) => Raylib.DrawTriangle(t.A, t.B, t.C, color.ToRayColor());

    /// <summary>
    /// Draws the outline of a triangle with specified line thickness and style.
    /// </summary>
    /// <param name="t">The triangle to draw.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="capType">The style of the line caps.</param>
    /// <param name="capPoints">The number of points used for the cap style.</param>
    public static void DrawLines(this Triangle t, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        DrawTriangleLines(t.A, t.B, t.C, lineThickness, color, capType, capPoints);
    }

    /// <summary>
    /// Draws the outline of a triangle with each side scaled by a factor.
    /// </summary>
    /// <param name="t">The triangle to draw.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="sideLengthFactor">The factor by which to scale each side <c>(0 = no triangle, 1 = full side length)</c>.</param>
    /// <param name="capType">The style of the line caps.</param>
    /// <param name="capPoints">The number of points used for the cap style.</param>
    /// <remarks>
    /// Each side is drawn from its starting vertex towards the next, scaled by <paramref name="sideLengthFactor"/>.
    /// </remarks>
    public static void DrawLines(this Triangle t, float lineThickness, ColorRgba color, float sideLengthFactor, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        DrawTriangleLines(t.A, t.B, t.C, lineThickness, color, sideLengthFactor, capType, capPoints);
    }

    /// <summary>
    /// Draws the outline of a triangle using a <see cref="LineDrawingInfo"/> object for style.
    /// </summary>
    /// <param name="t">The triangle to draw.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    public static void DrawLines(this Triangle t, LineDrawingInfo lineInfo) => DrawTriangleLines(t.A, t.B, t.C, lineInfo);

    /// <summary>
    /// Draws the outline of a triangle using a <see cref="LineDrawingInfo"/> object, with rotation applied.
    /// </summary>
    /// <param name="t">The triangle to draw.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <param name="rotDeg">The rotation in degrees to apply to the triangle.</param>
    /// <param name="rotOrigin">The origin point to rotate around (absolute coordinates).</param>
    public static void DrawLines(this Triangle t, LineDrawingInfo lineInfo, float rotDeg, Vector2 rotOrigin)
    {
        t = t.ChangeRotation(rotDeg * ShapeMath.DEGTORAD, rotOrigin);
        DrawTriangleLines(t.A, t.B, t.C, lineInfo);
    }

    /// <summary>
    /// Draws circles at each vertex of the triangle.
    /// </summary>
    /// <param name="t">The triangle whose vertices to draw.</param>
    /// <param name="vertexRadius">The radius of each vertex circle.</param>
    /// <param name="color">The color of the vertex circles.</param>
    /// <param name="circleSegments">The number of segments to use for each circle (default is 8).</param>
    public static void DrawVertices(this Triangle t, float vertexRadius, ColorRgba color, int circleSegments = 8)
    {
        CircleDrawing.DrawCircle(t.A, vertexRadius, color, circleSegments);
        CircleDrawing.DrawCircle(t.B, vertexRadius, color, circleSegments);
        CircleDrawing.DrawCircle(t.C, vertexRadius, color, circleSegments);
    }

    /// <summary>
    /// Draws a collection of triangles filled with the specified color.
    /// </summary>
    /// <param name="triangles">The collection of triangles to draw.</param>
    /// <param name="color">The color to fill each triangle with.</param>
    public static void Draw(this Triangulation triangles, ColorRgba color) { foreach (var t in triangles) t.Draw(color); }

    /// <summary>
    /// Draws the outlines of a collection of triangles with specified line thickness and style.
    /// </summary>
    /// <param name="triangles">The collection of triangles to draw.</param>
    /// <param name="lineThickness">The thickness of the outlines.</param>
    /// <param name="color">The color of the outlines.</param>
    /// <param name="capType">The style of the line caps.</param>
    /// <param name="capPoints">The number of points used for the cap style.</param>
    public static void DrawLines(this Triangulation triangles, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        foreach (var t in triangles) t.DrawLines(lineThickness, color, capType, capPoints);
    }

    /// <summary>
    /// Draws the outlines of a collection of triangles using a <see cref="LineDrawingInfo"/> object for style.
    /// </summary>
    /// <param name="triangles">The collection of triangles to draw.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    public static void DrawLines(this Triangulation triangles, LineDrawingInfo lineInfo)
    {
        foreach (var t in triangles) t.DrawLines(lineInfo);
    }

    /// <summary>
    /// Draws a certain percentage of a triangle's outline.
    /// </summary>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    ///  <param name="f">
    /// Specifies which portion of the triangle's outline to draw, as well as the starting corner and direction.
    /// <list type="bullet">
    /// <item>The integer part (0-2) selects the starting corner:
    /// <list type="bullet">
    /// <item><c>Counter-Clockwise</c> -> 0 = a, 1 = b, 2 = c</item>
    /// <item><c>Clockwise</c> -> 0 = a, 1 = c, 2 = b</item>
    /// </list>
    /// </item>
    /// <item>The fractional part (0.0-1.0) determines the percentage of the outline to draw, relative to the full perimeter.</item>
    /// <item>A negative value reverses the drawing direction (clockwise instead of counter-clockwise).</item>
    /// </list>
    /// Examples:
    /// <list type="bullet">
    /// <item><c>0.35</c> - Start at a, draw 35% of the outline counter-clockwise.</item>
    /// <item><c>-2.7</c> - Start at b, draw 70% of the outline clockwise.</item>
    /// </list>
    /// </param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="capType">The style of the line caps.</param>
    /// <param name="capPoints">The number of points used for the cap style.</param>
    /// <remarks>
    /// Useful for animating or highlighting portions of a triangle's outline.
    /// </remarks>
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
    /// Draws a certain percentage of a triangle's outline using a <see cref="LineDrawingInfo"/> object.
    /// </summary>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <param name="f">
    /// Specifies which portion of the triangle's outline to draw, as well as the starting corner and direction.
    /// <list type="bullet">
    /// <item>The integer part (0-2) selects the starting corner:
    /// <list type="bullet">
    /// <item><c>Counter-Clockwise</c> -> 0 = a, 1 = b, 2 = c</item>
    /// <item><c>Clockwise</c> -> 0 = a, 1 = c, 2 = b</item>
    /// </list>
    /// </item>
    /// <item>The fractional part (0.0-1.0) determines the percentage of the outline to draw, relative to the full perimeter.</item>
    /// <item>A negative value reverses the drawing direction (clockwise instead of counter-clockwise).</item>
    /// </list>
    /// Examples:
    /// <list type="bullet">
    /// <item><c>0.35</c> - Start at a, draw 35% of the outline counter-clockwise.</item>
    /// <item><c>-2.7</c> - Start at b, draw 70% of the outline clockwise.</item>
    /// </list>
    /// </param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <remarks>
    /// Useful for animating or highlighting portions of a triangle's outline.
    /// </remarks>
    public static void DrawTriangleLinesPercentage(Vector2 a, Vector2 b, Vector2 c, float f, LineDrawingInfo lineInfo)
    {
        DrawTriangleLinesPercentage(a, b, c, f, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
    }

    /// <summary>
    /// Draws a certain percentage of a triangle's outline.
    /// </summary>
    /// <param name="t">The triangle to draw.</param>
    /// <param name="f">
    /// Specifies which portion of the triangle's outline to draw, as well as the starting corner and direction.
    /// <list type="bullet">
    /// <item>The integer part (0-2) selects the starting corner:
    /// <list type="bullet">
    /// <item><c>Counter-Clockwise</c> -> 0 = a, 1 = b, 2 = c</item>
    /// <item><c>Clockwise</c> -> 0 = a, 1 = c, 2 = b</item>
    /// </list>
    /// </item>
    /// <item>The fractional part (0.0-1.0) determines the percentage of the outline to draw, relative to the full perimeter.</item>
    /// <item>A negative value reverses the drawing direction (clockwise instead of counter-clockwise).</item>
    /// </list>
    /// Examples:
    /// <list type="bullet">
    /// <item><c>0.35</c> - Start at a, draw 35% of the outline counter-clockwise.</item>
    /// <item><c>-2.7</c> - Start at b, draw 70% of the outline clockwise.</item>
    /// </list>
    /// </param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="capType">The style of the line caps.</param>
    /// <param name="capPoints">The number of points used for the cap style.</param>
    /// <remarks>
    /// Useful for animating or highlighting portions of a triangle's outline.
    /// </remarks>
    public static void DrawLinesPercentage(this Triangle t, float f, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        DrawTriangleLinesPercentage(t.A, t.B, t.C, f, lineThickness, color, capType, capPoints);
    }

    /// <summary>
    /// Draws a certain percentage of a triangle's outline using a <see cref="LineDrawingInfo"/> object.
    /// </summary>
    /// <param name="t">The triangle to draw.</param>
    /// <param name="f">
    /// Specifies which portion of the triangle's outline to draw, as well as the starting corner and direction.
    /// <list type="bullet">
    /// <item>The integer part (0-2) selects the starting corner:
    /// <list type="bullet">
    /// <item><c>Counter-Clockwise</c> -> 0 = a, 1 = b, 2 = c</item>
    /// <item><c>Clockwise</c> -> 0 = a, 1 = c, 2 = b</item>
    /// </list>
    /// </item>
    /// <item>The fractional part (0.0-1.0) determines the percentage of the outline to draw, relative to the full perimeter.</item>
    /// <item>A negative value reverses the drawing direction (clockwise instead of counter-clockwise).</item>
    /// </list>
    /// Examples:
    /// <list type="bullet">
    /// <item><c>0.35</c> - Start at a, draw 35% of the outline counter-clockwise.</item>
    /// <item><c>-2.7</c> - Start at b, draw 70% of the outline clockwise.</item>
    /// </list>
    /// </param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <remarks>
    /// Useful for animating or highlighting portions of a triangle's outline.
    /// </remarks>
    public static void DrawLinesPercentage(this Triangle t, float f, LineDrawingInfo lineInfo)
    {
        DrawTriangleLinesPercentage(t.A, t.B, t.C, f, lineInfo);
    }

    /// <summary>
    /// Draws a certain percentage of a triangle's outline using a <see cref="LineDrawingInfo"/> object, with rotation applied.
    /// </summary>
    /// <param name="t">The triangle to draw.</param>
    /// <param name="f">
    /// Specifies which portion of the triangle's outline to draw, as well as the starting corner and direction.
    /// <list type="bullet">
    /// <item>The integer part (0-2) selects the starting corner:
    /// <list type="bullet">
    /// <item><c>Counter-Clockwise</c> -> 0 = a, 1 = b, 2 = c</item>
    /// <item><c>Clockwise</c> -> 0 = a, 1 = c, 2 = b</item>
    /// </list>
    /// </item>
    /// <item>The fractional part (0.0-1.0) determines the percentage of the outline to draw, relative to the full perimeter.</item>
    /// <item>A negative value reverses the drawing direction (clockwise instead of counter-clockwise).</item>
    /// </list>
    /// Examples:
    /// <list type="bullet">
    /// <item><c>0.35</c> - Start at a, draw 35% of the outline counter-clockwise.</item>
    /// <item><c>-2.7</c> - Start at b, draw 70% of the outline clockwise.</item>
    /// </list>
    /// </param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <param name="rotDeg">The rotation in degrees to apply to the triangle.</param>
    /// <param name="rotOrigin">The origin point to rotate around (absolute coordinates).</param>
    /// <remarks>
    /// Useful for animating or highlighting portions of a triangle's outline.
    /// </remarks>
    public static void DrawLinesPercentage(this Triangle t, float f, LineDrawingInfo lineInfo, float rotDeg, Vector2 rotOrigin)
    {
        t = t.ChangeRotation(rotDeg * ShapeMath.DEGTORAD, rotOrigin);
        DrawTriangleLinesPercentage(t.A, t.B, t.C, f, lineInfo);
    }

    /// <summary>
    /// Draws a triangle outline where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="t">The triangle to draw.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <param name="rotDeg">The rotation in degrees to apply to the triangle.</param>
    /// <param name="rotOrigin">The origin point to rotate around (absolute coordinates).</param>
    /// <param name="sideScaleFactor">The scale factor for each side <c>(0 = No Side, 1 = Full Side).</c></param>
    /// <param name="sideScaleOrigin">The point along each side to scale from in both directions <c>(0 = Start, 1 = End)</c>.</param>
    /// <remarks>
    /// Allows for dynamic scaling of triangle sides, useful for effects or partial outlines.
    /// </remarks>
    public static void DrawLinesScaled(this Triangle t, LineDrawingInfo lineInfo, float rotDeg, Vector2 rotOrigin, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0) return;
        if (sideScaleFactor >= 1)
        {
            t.DrawLines(lineInfo, rotDeg, rotOrigin);
            return;
        }

        if(rotDeg != 0) t = t.ChangeRotation(rotDeg * ShapeMath.DEGTORAD, rotOrigin);

        SegmentDrawing.DrawSegment(t.A, t.B, lineInfo, sideScaleFactor, sideScaleOrigin);
        SegmentDrawing.DrawSegment(t.B, t.C, lineInfo, sideScaleFactor, sideScaleOrigin);
        SegmentDrawing.DrawSegment(t.C, t.A, lineInfo, sideScaleFactor, sideScaleOrigin);
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
            SegmentDrawing.DrawSegment(curP, nextP, lineThickness, color);
            return;
        }

        SegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, capType, capPoints);
        perimeterToDraw -= l1;

        // Draw second segment
        curP = nextP;
        nextP = p3;
        if (perimeterToDraw < l2)
        {
            float p = perimeterToDraw / l2;
            nextP = curP.Lerp(nextP, p);
            SegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, capType, capPoints);
            return;
        }

        SegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, capType, capPoints);
        perimeterToDraw -= l2;

        // Draw third segment
        curP = nextP;
        nextP = p1;
        if (perimeterToDraw < l3)
        {
            float p = perimeterToDraw / l3;
            nextP = curP.Lerp(nextP, p);
        }

        SegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, capType, capPoints);
    }
}