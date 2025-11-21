using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangulationDef;
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
    #region Draw Masked
    
    /// <summary>
    /// Draws the triangle's three segments using the provided <see cref="LineDrawingInfo"/>,
    /// clipped by a triangular mask.
    /// </summary>
    /// <param name="triangle">The source triangle whose edges will be drawn.</param>
    /// <param name="mask">Triangle used as the clipping mask.
    /// Only portions outside (or inside when <paramref name="reversedMask"/> is true) will be drawn.</param>
    /// <param name="lineInfo">Styling and thickness information for the lines.</param>
    /// <param name="reversedMask">If true, inverts the mask so drawing occurs inside instead of outside the mask.</param>
    public static void DrawLinesMasked(this Triangle triangle, Triangle mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        triangle.SegmentAToB.DrawMasked(mask, lineInfo, reversedMask);
        triangle.SegmentBToC.DrawMasked(mask, lineInfo, reversedMask);
        triangle.SegmentCToA.DrawMasked(mask, lineInfo, reversedMask);
    }
    /// <summary>
    /// Draws the triangle's three segments using the provided <see cref="LineDrawingInfo"/>,
    /// clipped by a circular <see cref="Circle"/> mask.
    /// </summary>
    /// <param name="triangle">The source triangle whose edges will be drawn.</param>
    /// <param name="mask">Circle used as the clipping mask.
    /// Only portions outside (or inside when <paramref name="reversedMask"/> is true) will be drawn.</param>
    /// <param name="lineInfo">Styling and thickness information for the lines.</param>
    /// <param name="reversedMask">If true, inverts the mask so drawing occurs inside instead of outside the mask.</param>
    public static void DrawLinesMasked(this Triangle triangle, Circle mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        triangle.SegmentAToB.DrawMasked(mask, lineInfo, reversedMask);
        triangle.SegmentBToC.DrawMasked(mask, lineInfo, reversedMask);
        triangle.SegmentCToA.DrawMasked(mask, lineInfo, reversedMask);
    }
    /// <summary>
    /// Draws the triangle's three segments using the provided <see cref="LineDrawingInfo"/>,
    /// clipped by a rectangular <see cref="Rect"/> mask.
    /// </summary>
    /// <param name="triangle">The source triangle whose edges will be drawn.</param>
    /// <param name="mask">Rect used as the clipping mask.
    /// Only portions outside (or inside when <paramref name="reversedMask"/> is true) will be drawn.</param>
    /// <param name="lineInfo">Styling and thickness information for the lines.</param>
    /// <param name="reversedMask">If true, inverts the mask so drawing occurs inside instead of outside the mask.</param>
    public static void DrawLinesMasked(this Triangle triangle, Rect mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        triangle.SegmentAToB.DrawMasked(mask, lineInfo, reversedMask);
        triangle.SegmentBToC.DrawMasked(mask, lineInfo, reversedMask);
        triangle.SegmentCToA.DrawMasked(mask, lineInfo, reversedMask);
    }
    /// <summary>
    /// Draws the triangle's three segments using the provided <see cref="LineDrawingInfo"/>,
    /// clipped by a quadrilateral <see cref="Quad"/> mask.
    /// </summary>
    /// <param name="triangle">The source triangle whose edges will be drawn.</param>
    /// <param name="mask">Quad used as the clipping mask.
    /// Only portions outside (or inside when <paramref name="reversedMask"/> is true) will be drawn.</param>
    /// <param name="lineInfo">Styling and thickness information for the lines.</param>
    /// <param name="reversedMask">If true, inverts the mask so drawing occurs inside instead of outside the mask.</param>
    public static void DrawLinesMasked(this Triangle triangle, Quad mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        triangle.SegmentAToB.DrawMasked(mask, lineInfo, reversedMask);
        triangle.SegmentBToC.DrawMasked(mask, lineInfo, reversedMask);
        triangle.SegmentCToA.DrawMasked(mask, lineInfo, reversedMask);
    }
    /// <summary>
    /// Draws the triangle's three segments using the provided <see cref="LineDrawingInfo"/>,
    /// clipped by a polygonal <see cref="Polygon"/> mask.
    /// </summary>
    /// <param name="triangle">The source triangle whose edges will be drawn.</param>
    /// <param name="mask">Polygon used as the clipping mask.
    /// Only portions outside (or inside when <paramref name="reversedMask"/> is true) will be drawn.</param>
    /// <param name="lineInfo">Styling and thickness information for the lines.</param>
    /// <param name="reversedMask">If true, inverts the mask so drawing occurs inside instead of outside the mask.</param>
    public static void DrawLinesMasked(this Triangle triangle, Polygon mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        triangle.SegmentAToB.DrawMasked(mask, lineInfo, reversedMask);
        triangle.SegmentBToC.DrawMasked(mask, lineInfo, reversedMask);
        triangle.SegmentCToA.DrawMasked(mask, lineInfo, reversedMask);
    }
    /// <summary>
    /// Draws the triangle's three segments using the provided <see cref="LineDrawingInfo"/>,
    /// clipped by a closed-shape mask of the generic type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The mask type. Must implement <see cref="IClosedShapeTypeProvider"/> (for example: Triangle, Circle, Rect, Polygon, Quad).</typeparam>
    /// <param name="triangle">The source triangle whose edges will be drawn.</param>
    /// <param name="mask">The clipping mask instance.
    /// Only portions outside (or inside when <paramref name="reversedMask"/> is true) will be drawn.</param>
    /// <param name="lineInfo">Styling and thickness information for the lines.</param>
    /// <param name="reversedMask">If true, inverts the mask so drawing occurs inside instead of outside the mask.</param>
    /// <remarks>
    /// This generic overload delegates to the segment-level DrawMasked extension for each triangle edge.
    /// </remarks>
    public static void DrawLinesMasked<T>(this Triangle triangle, T mask, LineDrawingInfo lineInfo, bool reversedMask = false) where T : IClosedShapeTypeProvider
    {
        triangle.SegmentAToB.DrawMasked(mask, lineInfo, reversedMask);
        triangle.SegmentBToC.DrawMasked(mask, lineInfo, reversedMask);
        triangle.SegmentCToA.DrawMasked(mask, lineInfo, reversedMask);
    }
    #endregion
    
    #region Draw
    /// <summary>
    /// Draws a filled triangle using the specified vertices and color.
    /// </summary>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <param name="color">The color to fill the triangle with.</param>
    public static void DrawTriangle(Vector2 a, Vector2 b, Vector2 c, ColorRgba color) => Raylib.DrawTriangle(a, b, c, color.ToRayColor());
    
    /// <summary>
    /// Draws a filled triangle using the specified <see cref="Triangle"/> and color.
    /// </summary>
    /// <param name="t">The triangle to draw.</param>
    /// <param name="color">The color to fill the triangle with.</param>
    public static void Draw(this Triangle t, ColorRgba color) => Raylib.DrawTriangle(t.A, t.B, t.C, color.ToRayColor());
    
    /// <summary>
    /// Draws a collection of triangles filled with the specified color.
    /// </summary>
    /// <param name="triangles">The collection of triangles to draw.</param>
    /// <param name="color">The color to fill each triangle with.</param>
    public static void Draw(this Triangulation triangles, ColorRgba color) { foreach (var t in triangles) t.Draw(color); }
    #endregion
    
    #region Draw Lines
    /// <summary>
    /// Draws the outline of a triangle with specified line thickness and style.
    /// </summary>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="cornerPoints"> How many extra points should be used for the outside edges of the outline.</param>
    public static void DrawTriangleLines(Vector2 a, Vector2 b, Vector2 c, float lineThickness, ColorRgba color, int cornerPoints = 0)
    {
        DrawTriangleLinesHelper(a, b, c, lineThickness, color, cornerPoints);
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
        if (sideLengthFactor <= 0f) return;
        if (sideLengthFactor >= 1f)
        {
            DrawTriangleLinesHelper(a, b, c, lineThickness, color);
            return;
        }
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
        DrawTriangleLinesHelper(a, b, c, lineInfo.Thickness, lineInfo.Color, lineInfo.CapPoints);
    }

    /// <summary>
    /// Draws the outline of a triangle with specified line thickness and style.
    /// </summary>
    /// <param name="t">The triangle to draw.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="cornerPoints"> How many extra points should be used for the outside edges of the outline.</param>
    public static void DrawLines(this Triangle t, float lineThickness, ColorRgba color, int cornerPoints = 0)
    {
        DrawTriangleLines(t.A, t.B, t.C, lineThickness, color, cornerPoints);
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
    /// Draws the outlines of a collection of triangles with specified line thickness and style.
    /// </summary>
    /// <param name="triangles">The collection of triangles to draw.</param>
    /// <param name="lineThickness">The thickness of the outlines.</param>
    /// <param name="color">The color of the outlines.</param>
    /// <param name="cornerPoints"> How many extra points should be used for the outside edges of the outline.</param>
    public static void DrawLines(this Triangulation triangles, float lineThickness, ColorRgba color, int cornerPoints = 0)
    {
        foreach (var t in triangles) t.DrawLines(lineThickness, color, cornerPoints);
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

    #endregion
    
    #region Draw Vertices
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
    #endregion
    
    #region Draw Lines Percentage
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
    /// <param name="capPoints">The number of points used for the cap style.</param>
    /// <remarks>
    /// Useful for animating or highlighting portions of a triangle's outline.
    /// </remarks>
    public static void DrawTriangleLinesPercentage(Vector2 a, Vector2 b, Vector2 c, float f, float lineThickness, ColorRgba color, int capPoints = 2)
    {
        if (f == 0) return;
        if (f is <= -1 or >= 1)
        {
            DrawTriangleLines(a, b, c, lineThickness, color, capPoints);
            return;
        }
        
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
            if (negative) //CW
            {
                DrawTriangleLinesPercentageHelper(a, c, b, percentage, lineThickness, color, capPoints);
            }
            else //CCW
            {
                DrawTriangleLinesPercentageHelper(a, b, c, percentage, lineThickness, color, capPoints);
            }
        }
        else if (startCorner == 1)
        {
            if (negative) //CW
            {
                DrawTriangleLinesPercentageHelper(c, b, a,  percentage, lineThickness, color, capPoints);
            }
            else //CCW
            {
                DrawTriangleLinesPercentageHelper(b, c, a,  percentage, lineThickness, color, capPoints);
            }
        }
        else if (startCorner == 2)
        {
            if (negative) //CW
            {
                DrawTriangleLinesPercentageHelper(b, a, c, percentage, lineThickness, color, capPoints);
            }
            else //CCW
            {
                DrawTriangleLinesPercentageHelper(c, a, b, percentage, lineThickness, color, capPoints);
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
        DrawTriangleLinesPercentage(a, b, c, f, lineInfo.Thickness, lineInfo.Color, lineInfo.CapPoints);
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
    /// <param name="capPoints">The number of points used for the cap style.</param>
    /// <remarks>
    /// Useful for animating or highlighting portions of a triangle's outline.
    /// </remarks>
    public static void DrawLinesPercentage(this Triangle t, float f, float lineThickness, ColorRgba color, int capPoints = 2)
    {
        DrawTriangleLinesPercentage(t.A, t.B, t.C, f, lineThickness, color, capPoints);
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
    
    #endregion
    
    #region Draw Lines Scaled
    /// <summary>
    /// Draws a triangle outline where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="t">The triangle to draw.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <param name="sideScaleFactor">The scale factor for each side <c>(0 = No Side, 1 = Full Side).</c></param>
    /// <param name="sideScaleOrigin">The point along each side to scale from in both directions <c>(0 = Start, 1 = End)</c>.</param>
    /// <remarks>
    /// Allows for dynamic scaling of triangle sides, useful for effects or partial outlines.
    /// </remarks>
    public static void DrawLinesScaled(this Triangle t, LineDrawingInfo lineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0) return;
        if (sideScaleFactor >= 1)
        {
            t.DrawLines(lineInfo);
            return;
        }

        SegmentDrawing.DrawSegment(t.A, t.B, lineInfo, sideScaleFactor, sideScaleOrigin);
        SegmentDrawing.DrawSegment(t.B, t.C, lineInfo, sideScaleFactor, sideScaleOrigin);
        SegmentDrawing.DrawSegment(t.C, t.A, lineInfo, sideScaleFactor, sideScaleOrigin);
    }
    #endregion
    
    #region Helper
    private static void DrawTriangleLinesPercentageHelper(Vector2 p1, Vector2 p2, Vector2 p3, float percentage, float lineThickness, ColorRgba color, int capPoints)
    {
        if (lineThickness <= 0 || percentage <= 0 || percentage >= 1) return;

        float maxThickness = CalculateMaxLineThickness(p1, p2, p3);
        float thickness = MathF.Min(lineThickness, maxThickness);

        if (capPoints <= 0)
        {
            DrawTriangleLinesPercentageHelperAlpha(p1, p2, p3, percentage, thickness, color);
        }
        else
        {
            if (color.A < 255)
            {
                DrawTriangleLinesPercentageHelperAlphaCapped(p1, p2, p3, percentage, lineThickness, color, capPoints);
            }
            else
            {
                DrawTriangleLinesPercentageHelperNoAlpha(p1, p2, p3, percentage, thickness, color, LineCapType.CappedExtended, capPoints);
            }
        }
    }
    private static void DrawTriangleLinesPercentageHelperAlpha(Vector2 p1, Vector2 p2, Vector2 p3, float percentage, float lineThickness, ColorRgba color)
    {
        if (lineThickness <= 0 || percentage <= 0 || percentage >= 1) return;

        float maxThickness = CalculateMaxLineThickness(p1, p2, p3);
        float thickness = MathF.Min(lineThickness, maxThickness);

        var edge1 = p2 - p1;
        var edge2 = p3 - p2;
        var edge3 = p1 - p3;

        var normal1 = new Vector2(-edge1.Y, edge1.X);
        if (normal1.LengthSquared() > 0) normal1 = Vector2.Normalize(normal1);

        var normal2 = new Vector2(-edge2.Y, edge2.X);
        if (normal2.LengthSquared() > 0) normal2 = Vector2.Normalize(normal2);

        var normal3 = new Vector2(-edge3.Y, edge3.X);
        if (normal3.LengthSquared() > 0) normal3 = Vector2.Normalize(normal3);

        float l1 = edge1.Length();
        float l2 = edge2.Length();
        float l3 = edge3.Length();
        float totalPerimeter = l1 + l2 + l3;
        float perimeterToDraw = totalPerimeter * percentage;

        var miter1Inner = CalculateMiterPoint(p1, normal3, normal1, thickness, false);
        var miter1Outer = CalculateMiterPoint(p1, normal3, normal1, thickness, true);
        
        var miter2Inner = CalculateMiterPoint(p2, normal1, normal2, thickness, false);
        var miter2Outer = CalculateMiterPoint(p2, normal1, normal2, thickness, true);

        Vector2 a, b, c, d;
        float f = 1f;
        if (l1 > perimeterToDraw)
        {
            f = perimeterToDraw / l1;
        }
        
        a = miter1Inner;
        b = miter1Outer;
        c = f >= 1f ? miter2Outer : b.Lerp(miter2Outer, f);
        d = f >= 1f ? miter2Inner : a.Lerp(miter2Inner, f);
        DrawTriangle(a,b,c,color);
        DrawTriangle(a,c,d,color);
        
        if(f < 1f) return;
        f = 1f;
        perimeterToDraw -= l1;
        
        var miter3Inner = CalculateMiterPoint(p3, normal2, normal3, thickness, false);
        var miter3Outer = CalculateMiterPoint(p3, normal2, normal3, thickness, true);
        
        if (l2 > perimeterToDraw)
        {
            f = perimeterToDraw / l2;
        }
        
        a = miter2Inner;
        b = miter2Outer;
        c = f >= 1f ? miter3Outer : b.Lerp(miter3Outer, f);
        d = f >= 1f ? miter3Inner : a.Lerp(miter3Inner, f);
        DrawTriangle(a,b,c,color);
        DrawTriangle(a,c,d,color);
        
        if(f < 1f) return;
        f = 1f;
        perimeterToDraw -= l2;
        
        if (l3 > perimeterToDraw)
        {
            f = perimeterToDraw / l3;
        }
        
        a = miter3Inner;
        b = miter3Outer;
        c = f >= 1f ? miter1Outer : b.Lerp(miter1Outer, f);
        d = f >= 1f ? miter1Inner : a.Lerp(miter1Inner, f);
        DrawTriangle(a,b,c,color);
        DrawTriangle(a,c,d,color);
    }


    private static void DrawTriangleLinesPercentageHelperAlphaCapped(Vector2 p1, Vector2 p2, Vector2 p3, float percentage, float lineThickness, ColorRgba color, int cornerPoints = 2)
    {
        if (MathF.Abs(percentage) < 0.001f || lineThickness <= 0f || cornerPoints <= 0) return;
    
        if (MathF.Abs(percentage) >= 1f)
        {
            DrawTriangleLinesHelper(p1, p2, p3, lineThickness, color, cornerPoints);
            return;
        }
    
        float maxThickness = CalculateMaxLineThickness(p1, p2, p3);
        float thickness = MathF.Min(lineThickness, maxThickness);
        float absPercentage = MathF.Abs(percentage);
        bool ccw = percentage > 0;
       
        // Reorder vertices for CW drawing so the main logic can be the same
        Vector2 v1 = p1;
        Vector2 v2 = ccw ? p2 : p3;
        Vector2 v3 = ccw ? p3 : p2;
    
        var e1 = v2 - v1;
        var e2 = v3 - v2;
        var e3 = v1 - v3;
    
        var n1 = Vector2.Normalize(new(-e1.Y, e1.X));
        var n2 = Vector2.Normalize(new(-e2.Y, e2.X));
        var n3 = Vector2.Normalize(new(-e3.Y, e3.X));
    
        // Inner miter points are calculated based on the potentially reordered vertices
        var m1Inner = CalculateMiterPoint(v1, n3, n1, thickness, false);
        var m2Inner = CalculateMiterPoint(v2, n1, n2, thickness, false);
        var m3Inner = CalculateMiterPoint(v3, n2, n3, thickness, false);
    
        float aN1 = MathF.Atan2(n1.Y, n1.X);
        float aN2 = MathF.Atan2(n2.Y, n2.X);
        float aN3 = MathF.Atan2(n3.Y, n3.X);
    
        float arcAngle1 = AngleDelta(aN3, aN1);
        float arcAngle2 = AngleDelta(aN1, aN2);
        float arcAngle3 = AngleDelta(aN2, aN3);
    
        float l1 = e1.Length();
        float l2 = e2.Length();
        float l3 = e3.Length();
    
        float arcLen1 = MathF.Abs(arcAngle1) * thickness;
        float arcLen2 = MathF.Abs(arcAngle2) * thickness;
        float arcLen3 = MathF.Abs(arcAngle3) * thickness;
    
        float totalPerimeter = l1 + l2 + l3 + arcLen1 + arcLen2 + arcLen3;
        float targetLen = totalPerimeter * absPercentage;
        float remaining = targetLen;
    
        // Unified drawing logic starts here
        Vector2 prevOuter = v1 + ShapeVec.VecFromAngleRad(aN3 + arcAngle1 * 0.5f) * thickness;
        remaining = EmitArc(v1, aN3 + arcAngle1 * 0.5f, arcAngle1 * 0.5f, m1Inner, prevOuter, remaining, arcAngle1);
        if (remaining <= 0) return;
    
        remaining = EmitEdge(v1 + n1 * thickness, v2 + n1 * thickness, m1Inner, m2Inner, remaining);
        if (remaining <= 0) return;
    
        remaining = EmitArc(v2, aN1, arcAngle2, m2Inner, v2 + n1 * thickness, remaining, arcAngle2);
        if (remaining <= 0) return;
    
        remaining = EmitEdge(v2 + n2 * thickness, v3 + n2 * thickness, m2Inner, m3Inner, remaining);
        if (remaining <= 0) return;
    
        remaining = EmitArc(v3, aN2, arcAngle3, m3Inner, v3 + n2 * thickness, remaining, arcAngle3);
        if (remaining <= 0) return;
    
        remaining = EmitEdge(v3 + n3 * thickness, v1 + n3 * thickness, m3Inner, m1Inner, remaining);
        if (remaining <= 0) return;
    
        EmitArc(v1, aN3, arcAngle1 * 0.5f, m1Inner, v1 + n3 * thickness, remaining, arcAngle1);
    
    
        float EmitArc(Vector2 corner, float startAngle, float angle, Vector2 inner, Vector2 currentOuter, float rem, float baseArcAngle)
        {
            if (rem <= 0f || angle == 0) return rem;
            int steps = (int)MathF.Ceiling(cornerPoints * (MathF.Abs(angle) / MathF.Abs(baseArcAngle)));
            if (steps <= 0) steps = 1;
    
            float stepAngle = angle / steps;
            float stepLen = MathF.Abs(stepAngle) * thickness;
    
            for (int i = 1; i <= steps; i++)
            {
                if (rem <= 0) break;
                var nextOuter = corner + ShapeVec.VecFromAngleRad(startAngle + stepAngle * i) * thickness;
                if (rem >= stepLen)
                {
                    DrawTriangle(inner, currentOuter, nextOuter, color);
                    rem -= stepLen;
                }
                else
                {
                    float f = rem / stepLen;
                    var partialOuter = currentOuter.Lerp(nextOuter, f);
                    DrawTriangle(inner, currentOuter, partialOuter, color);
                    rem = 0;
                }
                currentOuter = nextOuter;
            }
            return rem;
        }
    
        float EmitEdge(Vector2 start, Vector2 end, Vector2 innerStart, Vector2 innerEnd, float rem)
        {
            if (rem <= 0f) return rem;
            float len = (end - start).Length();
            if (len <= 0) return rem;
    
            if (rem >= len)
            {
                DrawTriangle(innerStart, start, end, color);
                DrawTriangle(innerStart, end, innerEnd, color);
                rem -= len;
            }
            else
            {
                float f = rem / len;
                var pOuter = start.Lerp(end, f);
                var pInner = innerStart.Lerp(innerEnd, f);
                DrawTriangle(innerStart, start, pOuter, color);
                DrawTriangle(innerStart, pOuter, pInner, color);
                rem = 0;
            }
            return rem;
        }
    
        float AngleDelta(float a0, float a1)
        {
            float d = a1 - a0;
            while (d > MathF.PI) d -= 2f * MathF.PI;
            while (d < -MathF.PI) d += 2f * MathF.PI;
            return d;
        }
    }
    
    
    private static void DrawTriangleLinesPercentageHelperNoAlpha(Vector2 p1, Vector2 p2, Vector2 p3, float percentage, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
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
            SegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, capType, capPoints);
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
    
    
    private static void DrawTriangleLinesHelper(Vector2 p1, Vector2 p2, Vector2 p3, float lineThickness, ColorRgba color, int cornerPoints = 0)
    {
        if (lineThickness <= 0) return;

        // Calculate maximum safe thickness based on inradius
        float maxThickness = CalculateMaxLineThickness(p1, p2, p3);
        float thickness = MathF.Min(lineThickness, maxThickness);

        // Calculate edge vectors and perpendicular normals
        var edge1 = p2 - p1;
        var edge2 = p3 - p2;
        var edge3 = p1 - p3;

        var normal1 = new Vector2(-edge1.Y, edge1.X);
        if (normal1.LengthSquared() > 0) normal1 = Vector2.Normalize(normal1);

        var normal2 = new Vector2(-edge2.Y, edge2.X);
        if (normal2.LengthSquared() > 0) normal2 = Vector2.Normalize(normal2);

        var normal3 = new Vector2(-edge3.Y, edge3.X);
        if (normal3.LengthSquared() > 0) normal3 = Vector2.Normalize(normal3);

        // Calculate inner miter points (always sharp)
        var miter1Inner = CalculateMiterPoint(p1, normal3, normal1, thickness, false);
        var miter2Inner = CalculateMiterPoint(p2, normal1, normal2, thickness, false);
        var miter3Inner = CalculateMiterPoint(p3, normal2, normal3, thickness, false);

        if (cornerPoints > 0)
        {
            // Rounded corners: use simple outer edge points, not miters
            var p1Outer = p1 + normal1 * thickness;
            var p2Outer1 = p2 + normal1 * thickness;
            var p2Outer2 = p2 + normal2 * thickness;
            var p3Outer2 = p3 + normal2 * thickness;
            var p3Outer3 = p3 + normal3 * thickness;
            var p1Outer3 = p1 + normal3 * thickness;

            // Draw straight edge segments (no miter)
            DrawEdgeQuad(miter1Inner, miter2Inner, p1Outer, p2Outer1, color);
            DrawEdgeQuad(miter2Inner, miter3Inner, p2Outer2, p3Outer2, color);
            DrawEdgeQuad(miter3Inner, miter1Inner, p3Outer3, p1Outer3, color);

            // Draw rounded corners to fill the gaps
            DrawOuterCorner(p1, normal3, normal1, miter1Inner, thickness, color, cornerPoints);
            DrawOuterCorner(p2, normal1, normal2, miter2Inner, thickness, color, cornerPoints);
            DrawOuterCorner(p3, normal2, normal3, miter3Inner, thickness, color, cornerPoints);
        }
        else
        {
            // Sharp corners: use miter points
            var miter1Outer = CalculateMiterPoint(p1, normal3, normal1, thickness, true);
            var miter2Outer = CalculateMiterPoint(p2, normal1, normal2, thickness, true);
            var miter3Outer = CalculateMiterPoint(p3, normal2, normal3, thickness, true);

            DrawEdgeQuad(miter1Inner, miter2Inner, miter1Outer, miter2Outer, color);
            DrawEdgeQuad(miter2Inner, miter3Inner, miter2Outer, miter3Outer, color);
            DrawEdgeQuad(miter3Inner, miter1Inner, miter3Outer, miter1Outer, color);
        }
    }
    private static Vector2 CalculateMiterPoint(Vector2 corner, Vector2 normalPrev, Vector2 normalNext, float halfThickness, bool outer)
    {
        // Calculate miter direction (average of normals)
        var miterDir = Vector2.Normalize(normalPrev + normalNext);
        
        // Calculate miter length based on angle
        float dot = Vector2.Dot(normalPrev, normalNext);
        float miterLength = halfThickness / MathF.Sqrt((1f + dot) * 0.5f);
        
        return corner + miterDir * (outer ? miterLength : -miterLength);
    }
    private static void DrawEdgeQuad(Vector2 innerStart, Vector2 innerEnd, Vector2 outerStart, Vector2 outerEnd, ColorRgba color)
    {
        DrawTriangle(innerStart, outerStart, innerEnd, color);
        DrawTriangle(outerStart, outerEnd, innerEnd, color);
    }
    private static void DrawOuterCorner(Vector2 corner, Vector2 normalPrev, Vector2 normalNext, Vector2 innerCorner, float halfThickness, ColorRgba color, int cornerPoints)
    {
        // Calculate angle between normals
        float anglePrev = MathF.Atan2(normalPrev.Y, normalPrev.X);
        float angleNext = MathF.Atan2(normalNext.Y, normalNext.X);
        
        // Ensure we sweep in the correct direction
        float angleDiff = angleNext - anglePrev;
        if (angleDiff > MathF.PI) angleDiff -= 2 * MathF.PI;
        if (angleDiff < -MathF.PI) angleDiff += 2 * MathF.PI;

        var prevOuter = corner + normalPrev * halfThickness;

        for (int i = 1; i <= cornerPoints + 1; i++)
        {
            float t = i / (float)(cornerPoints + 1);
            float angle = anglePrev + angleDiff * t;
            var normal = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            var curOuter = corner + normal * halfThickness;

            DrawTriangle(innerCorner, prevOuter, curOuter, color);

            prevOuter = curOuter;
        }
    }
    
    private static float CalculateMaxLineThickness(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        // Calculate side lengths
        float a = (p2 - p3).Length();
        float b = (p3 - p1).Length();
        float c = (p1 - p2).Length();
    
        // Calculate semi-perimeter
        float s = (a + b + c) * 0.5f;
    
        // Calculate area using Heron's formula
        float area = MathF.Sqrt(s * (s - a) * (s - b) * (s - c));
    
        // Inradius formula: r = area / s
        float inradius = area / s;
    
        // Return inradius as max thickness (or slightly less to be safe)
        return inradius * 0.95f; // 0.95 leaves a small margin
    }
    #endregion
}

    
    