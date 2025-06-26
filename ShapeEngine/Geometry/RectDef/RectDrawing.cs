using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.RectDef;

/// <summary>
/// Provides static extension methods for drawing rectangles and grids, including advanced features such as nine-patch, rounded corners, slanted corners, partial outlines, and more.
/// </summary>
/// <remarks>
/// All methods are designed for use with Raylib and ShapeEngine types.
/// Methods are implemented as extensions for convenient usage.
/// </remarks>
public static class RectDrawing
{
    /// <summary>
    /// Draws a <see cref="NinePatchRect"/> using a single color for all patches.
    /// </summary>
    /// <param name="npr">The nine-patch rectangle to draw.</param>
    /// <param name="color">The color to use for all patches.</param>
    public static void Draw(this NinePatchRect npr, ColorRgba color)
    {
        var rects = npr.Rects;
        foreach (var r in rects)
        {
            r.Draw(color);
        }
    }

    /// <summary>
    /// Draws a <see cref="NinePatchRect"/> using separate colors for the source and patch rectangles.
    /// </summary>
    /// <param name="npr">The nine-patch rectangle to draw.</param>
    /// <param name="sourceColorRgba">The color for the source rectangle.</param>
    /// <param name="patchColorRgba">The color for the patch rectangles.</param>
    public static void Draw(this NinePatchRect npr, ColorRgba sourceColorRgba, ColorRgba patchColorRgba)
    {
        npr.Source.Draw(sourceColorRgba);
        var rects = npr.Rects;
        foreach (var r in rects)
        {
            r.Draw(patchColorRgba);
        }
    }

    /// <summary>
    /// Draws the outlines of a <see cref="NinePatchRect"/> using the specified line thickness and color.
    /// </summary>
    /// <param name="npr">The nine-patch rectangle to draw.</param>
    /// <param name="lineThickness">The thickness of the outline lines.</param>
    /// <param name="color">The color of the outline lines.</param>
    public static void DrawLines(this NinePatchRect npr, float lineThickness, ColorRgba color)
    {
        var rects = npr.Rects;
        foreach (var r in rects)
        {
            r.DrawLines(lineThickness, color);
        }
    }

    /// <summary>
    /// Draws the outlines of a <see cref="NinePatchRect"/> using separate line thickness and color for the source and patch rectangles.
    /// </summary>
    /// <param name="npr">The nine-patch rectangle to draw.</param>
    /// <param name="sourceLineThickness">The line thickness for the source rectangle.</param>
    /// <param name="patchLineThickness">The line thickness for the patch rectangles.</param>
    /// <param name="sourceColorRgba">The color for the source rectangle outline.</param>
    /// <param name="patchColorRgba">The color for the patch rectangles outlines.</param>
    public static void DrawLines(this NinePatchRect npr, float sourceLineThickness, float patchLineThickness, ColorRgba sourceColorRgba, ColorRgba patchColorRgba)
    {
        npr.Source.DrawLines(sourceLineThickness, sourceColorRgba);
        var rects = npr.Rects;
        foreach (var r in rects)
        {
            r.DrawLines(patchLineThickness, patchColorRgba);
        }
    }

    /// <summary>
    /// Draws a grid within the specified bounds.
    /// </summary>
    /// <param name="grid">The grid definition (rows and columns).</param>
    /// <param name="bounds">The rectangle bounds in which to draw the grid.</param>
    /// <param name="lineThickness">The thickness of the grid lines.</param>
    /// <param name="color">The color of the grid lines.</param>
    public static void Draw(this Grid grid, Rect bounds, float lineThickness, ColorRgba color)
    {
        Vector2 rowSpacing = new(0f, bounds.Height / grid.Rows);
        for (int row = 0; row < grid.Rows + 1; row++)
        {
            SegmentDrawing.DrawSegment(bounds.TopLeft + rowSpacing * row, bounds.TopRight + rowSpacing * row, lineThickness, color);
        }
        Vector2 colSpacing = new(bounds.Width / grid.Cols, 0f);
        for (int col = 0; col < grid.Cols + 1; col++)
        {
            SegmentDrawing.DrawSegment(bounds.TopLeft + colSpacing * col, bounds.BottomLeft + colSpacing * col, lineThickness, color);
        }
    }

    /// <summary>
    /// Draws a grid inside a rectangle, with the specified number of lines and line drawing information.
    /// </summary>
    /// <param name="r">The rectangle in which to draw the grid.</param>
    /// <param name="lines">The number of grid lines (both horizontal and vertical).</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, etc.).</param>
    public static void DrawGrid(this Rect r, int lines, LineDrawingInfo lineInfo)
    {
        var xOffset = new Vector2(r.Width / lines, 0f);// * i;
        var yOffset = new Vector2(0f, r.Height / lines);// * i;

        var tl = r.TopLeft;
        var tr = tl + new Vector2(r.Width, 0);
        var bl = tl + new Vector2(0, r.Height);

        for (var i = 0; i < lines; i++)
        {
            SegmentDrawing.DrawSegment(tl + xOffset * i, bl + xOffset * i, lineInfo);
            SegmentDrawing.DrawSegment(tl + yOffset * i, tr + yOffset * i, lineInfo);
        }
    }

    /// <summary>
    /// Draws a filled rectangle using the specified top-left and bottom-right coordinates and color.
    /// </summary>
    /// <param name="topLeft">The top-left corner of the rectangle.</param>
    /// <param name="bottomRight">The bottom-right corner of the rectangle.</param>
    /// <param name="color">The fill color.</param>
    public static void DrawRect(Vector2 topLeft, Vector2 bottomRight, ColorRgba color)
    {
        Raylib.DrawRectangleV(topLeft, bottomRight - topLeft, color.ToRayColor());
    }

    /// <summary>
    /// Draws a filled, rotated rectangle using the specified corners, pivot, rotation, and color.
    /// </summary>
    /// <param name="topLeft">The top-left corner of the rectangle.</param>
    /// <param name="bottomRight">The bottom-right corner of the rectangle.</param>
    /// <param name="pivot">The pivot point for rotation.</param>
    /// <param name="rotDeg">The rotation in degrees.</param>
    /// <param name="color">The fill color.</param>
    public static void DrawRect(Vector2 topLeft, Vector2 bottomRight, Vector2 pivot, float rotDeg, ColorRgba color)
    {
        var a = pivot + (topLeft - pivot).RotateDeg(rotDeg);
        var b = pivot + (new Vector2(topLeft.X, bottomRight.Y) - pivot).RotateDeg(rotDeg);
        var c = pivot + (bottomRight - pivot).RotateDeg(rotDeg);
        var d = pivot + (new Vector2(bottomRight.X, topLeft.Y) - pivot).RotateDeg(rotDeg);
        QuadDrawing.DrawQuad(a, b, c, d, color);
    }

    /// <summary>
    /// Draws the outline of a rectangle using the specified corners, line thickness, and color.
    /// </summary>
    /// <param name="topLeft">The top-left corner of the rectangle.</param>
    /// <param name="bottomRight">The bottom-right corner of the rectangle.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    public static void DrawRectLines(Vector2 topLeft, Vector2 bottomRight, float lineThickness, ColorRgba color) => DrawLines(new Rect(topLeft, bottomRight), lineThickness, color);

    /// <summary>
    /// Draws the outline of a rectangle with each side scaled by a factor,
    /// using the specified line thickness, color, and cap style.
    /// </summary>
    /// <param name="topLeft">The top-left corner of the rectangle.</param>
    /// <param name="bottomRight">The bottom-right corner of the rectangle.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="sideLengthFactor">The factor by which to scale each side (0 to 1).</param>
    /// <param name="capType">The type of line cap to use.</param>
    /// <param name="capPoints">The number of points for the cap.</param>
    /// <remarks>
    /// Useful for drawing partial outlines or stylized rectangles.
    /// </remarks>
    public static void DrawRectLines(Vector2 topLeft, Vector2 bottomRight, float lineThickness, ColorRgba color, float sideLengthFactor,
        LineCapType capType = LineCapType.Extended, int capPoints = 0)
    {
        var a = topLeft;
        var b = new Vector2(topLeft.X, bottomRight.Y);
        var c = bottomRight;
        var d = new Vector2(bottomRight.X, topLeft.Y);

        var side1 = b - a;
        var end1 = a + side1 * sideLengthFactor;

        var side2 = c - b;
        var end2 = b + side2 * sideLengthFactor;

        var side3 = d - c;
        var end3 = c + side3 * sideLengthFactor;

        var side4 = a - d;
        var end4 = d + side4 * sideLengthFactor;

        SegmentDrawing.DrawSegment(a, end1, lineThickness, color, capType, capPoints);
        SegmentDrawing.DrawSegment(b, end2, lineThickness, color, capType, capPoints);
        SegmentDrawing.DrawSegment(c, end3, lineThickness, color, capType, capPoints);
        SegmentDrawing.DrawSegment(d, end4, lineThickness, color, capType, capPoints);
    }

    /// <summary>
    /// Draws the outline of a rectangle using the specified line drawing information.
    /// </summary>
    /// <param name="topLeft">The top-left corner of the rectangle.</param>
    /// <param name="bottomRight">The bottom-right corner of the rectangle.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, etc.).</param>
    public static void DrawRectLines(Vector2 topLeft, Vector2 bottomRight, LineDrawingInfo lineInfo)
    {
        var a = topLeft;
        var b = new Vector2(topLeft.X, bottomRight.Y);
        var c = bottomRight;
        var d = new Vector2(bottomRight.X, topLeft.Y);

        SegmentDrawing.DrawSegment(a, b, lineInfo);
        SegmentDrawing.DrawSegment(b, c, lineInfo);
        SegmentDrawing.DrawSegment(c, d, lineInfo);
        SegmentDrawing.DrawSegment(d, a, lineInfo);
    }

    /// <summary>
    /// Draws the outline of a rotated rectangle using the specified corners,
    /// pivot, rotation, line thickness, color, and cap style.
    /// </summary>
    /// <param name="topLeft">The top-left corner of the rectangle.</param>
    /// <param name="bottomRight">The bottom-right corner of the rectangle.</param>
    /// <param name="pivot">The pivot point for rotation.</param>
    /// <param name="rotDeg">The rotation in degrees.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="capType">The type of line cap to use.</param>
    /// <param name="capPoints">The number of points for the cap.</param>
    public static void DrawRectLines(Vector2 topLeft, Vector2 bottomRight, Vector2 pivot, float rotDeg, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.Extended, int capPoints = 0)
    {
        var a = pivot + (topLeft - pivot).RotateDeg(rotDeg);
        var b = pivot + (new Vector2(topLeft.X, bottomRight.Y) - pivot).RotateDeg(rotDeg);
        var c = pivot + (bottomRight - pivot).RotateDeg(rotDeg);
        var d = pivot + (new Vector2(bottomRight.X, topLeft.Y) - pivot).RotateDeg(rotDeg);
        QuadDrawing.DrawQuadLines(a, b, c, d, lineThickness, color, capType, capPoints);
    }

    /// <summary>
    /// Draws the outline of a rotated rectangle using the specified line drawing information.
    /// </summary>
    /// <param name="topLeft">The top-left corner of the rectangle.</param>
    /// <param name="bottomRight">The bottom-right corner of the rectangle.</param>
    /// <param name="pivot">The pivot point for rotation.</param>
    /// <param name="rotDeg">The rotation in degrees.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, etc.).</param>
    public static void DrawRectLines(Vector2 topLeft, Vector2 bottomRight, Vector2 pivot, float rotDeg, LineDrawingInfo lineInfo)
        => DrawRectLines(topLeft, bottomRight, pivot, rotDeg, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);

    /// <summary>
    /// Draws a filled rectangle using the specified color.
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="color">The fill color.</param>
    public static void Draw(this Rect rect, ColorRgba color) => Raylib.DrawRectangleRec(rect.Rectangle, color.ToRayColor());

    /// <summary>
    /// Draws a filled, rotated rectangle using the specified pivot, rotation, and color.
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="pivot">The pivot point for rotation.</param>
    /// <param name="rotDeg">The rotation in degrees.</param>
    /// <param name="color">The fill color.</param>
    public static void Draw(this Rect rect, Vector2 pivot, float rotDeg, ColorRgba color) => DrawRect(rect.TopLeft, rect.BottomRight, pivot, rotDeg, color);

    /// <summary>
    /// Draws the outline of a rectangle using the specified line thickness and color.
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    public static void DrawLines(this Rect rect, float lineThickness, ColorRgba color) => Raylib.DrawRectangleLinesEx(rect.Rectangle, lineThickness * 2, color.ToRayColor());

    /// <summary>
    /// Draws the outline of a rectangle using the specified line drawing information.
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, etc.).</param>
    public static void DrawLines(this Rect rect, LineDrawingInfo lineInfo)
    {
        SegmentDrawing.DrawSegment(rect.TopLeft, rect.BottomLeft, lineInfo);
        SegmentDrawing.DrawSegment(rect.BottomLeft, rect.BottomRight, lineInfo);
        SegmentDrawing.DrawSegment(rect.BottomRight, rect.TopRight, lineInfo);
        SegmentDrawing.DrawSegment(rect.TopRight, rect.TopLeft, lineInfo);
    }

    /// <summary>
    /// Draws the outline of a rotated rectangle using the specified pivot,
    /// rotation, line thickness, color, and cap style.
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="pivot">The pivot point for rotation.</param>
    /// <param name="rotDeg">The rotation in degrees.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="capType">The type of line cap to use.</param>
    /// <param name="capPoints">The number of points for the cap.</param>
    public static void DrawLines(this Rect rect, Vector2 pivot, float rotDeg, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.Extended, int capPoints = 0)
    {
        DrawRectLines(rect.TopLeft, rect.BottomRight, pivot, rotDeg, lineThickness, color, capType, capPoints);
    }

    /// <summary>
    /// Draws the outline of a rectangle with each side scaled by a factor,
    /// using the specified line thickness, color, and cap style.
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="sideLengthFactor">The factor by which to scale each side (0 to 1).</param>
    /// <param name="capType">The type of line cap to use.</param>
    /// <param name="capPoints">The number of points for the cap.</param>
    public static void DrawLines(this Rect rect, float lineThickness, ColorRgba color, float sideLengthFactor, LineCapType capType = LineCapType.Extended, int capPoints = 0)
    {
        DrawRectLines(rect.TopLeft, rect.BottomRight, lineThickness, color, sideLengthFactor, capType, capPoints);
    }

    /// <summary>
    /// Draws the outline of a rotated rectangle using the specified line drawing information.
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="pivot">The pivot point for rotation.</param>
    /// <param name="rotDeg">The rotation in degrees.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, etc.).</param>
    public static void DrawLines(this Rect rect, Vector2 pivot, float rotDeg, LineDrawingInfo lineInfo)
    {
        DrawRectLines(rect.TopLeft, rect.BottomRight, pivot, rotDeg, lineInfo);
    }

    /// <summary>
    /// Draws a certain percentage of a rectangle's outline, starting at a specified corner and direction.
    /// </summary>
    /// <param name="topLeft">The top-left corner of the rectangle.</param>
    /// <param name="bottomRight">The bottom-right corner of the rectangle.</param>
    /// <param name="f">
    /// Specifies which portion of the rectangle's outline to draw, as well as the starting corner and direction.
        /// <list type="bullet">
        /// <item>The integer part (0-3) selects the starting corner:
            /// <list type="bullet">
            /// <item><c>Counter-Clockwise</c> -> 0 = top-left, 1 = bottom-left, 2 = bottom-right, 3 = top-right.</item>
            /// <item><c>Clockwise</c> -> 0 = top-left, 1 = top-right, 2 = bottom-right, 3 = bottom-left.</item>
            /// </list>
        /// </item>
        /// <item>The fractional part (0.0-1.0) determines the percentage of the outline to draw, relative to the full perimeter.</item>
        /// <item>A negative value reverses the drawing direction (clockwise instead of counter-clockwise).</item>
        /// </list>
    /// Examples:
        /// <list type="bullet">
        /// <item><c>0.35</c> - Start at top-left, draw 35% of the outline counter-clockwise.</item>
        /// <item><c>-2.7</c> - Start at bottom-right, draw 70% of the outline clockwise.</item>
        /// </list>
    /// </param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="capType">The type of line cap to use.</param>
    /// <param name="capPoints">The number of points for the cap.</param>
    /// <remarks>
    /// Useful for progress indicators or animated outlines.
    /// </remarks>
    public static void DrawRectLinesPercentage(Vector2 topLeft, Vector2 bottomRight, float f, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (f == 0) return;
        var r = new Rect(topLeft, bottomRight);
        if (r.Width <= 0 || r.Height <= 0) return;

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

        var perimeter = r.Width * 2 + r.Height * 2;
        var perimeterToDraw = perimeter * percentage;

        if (startCorner == 0)
        {
            if (negative)
            {
                DrawRectLinesPercentageHelper(r.TopLeft, r.TopRight, r.BottomRight, r.BottomLeft, perimeterToDraw, r.Width, r.Height, lineThickness, color, capType, capPoints);
            }
            else
            {
                DrawRectLinesPercentageHelper(r.TopLeft, r.BottomLeft, r.BottomRight, r.TopRight, perimeterToDraw, r.Height, r.Width, lineThickness, color, capType, capPoints);
            }
        }
        else if (startCorner == 1)
        {
            if (negative)
            {
                DrawRectLinesPercentageHelper(r.TopRight, r.BottomRight, r.BottomLeft, r.TopLeft, perimeterToDraw, r.Height, r.Width, lineThickness, color, capType, capPoints);
            }
            else
            {
                DrawRectLinesPercentageHelper(r.BottomLeft, r.BottomRight, r.TopRight, r.TopLeft, perimeterToDraw, r.Width, r.Height, lineThickness, color, capType, capPoints);
            }
        }
        else if (startCorner == 2)
        {
            if (negative)
            {
                DrawRectLinesPercentageHelper(r.BottomRight, r.BottomLeft, r.TopLeft, r.TopRight, perimeterToDraw, r.Width, r.Height, lineThickness, color, capType, capPoints);
            }
            else
            {
                DrawRectLinesPercentageHelper(r.BottomRight, r.TopRight, r.TopLeft, r.BottomLeft, perimeterToDraw, r.Height, r.Width, lineThickness, color, capType, capPoints);
            }
        }
        else if (startCorner == 3)
        {
            if (negative)
            {
                DrawRectLinesPercentageHelper(r.BottomLeft, r.TopLeft, r.TopRight, r.BottomRight, perimeterToDraw, r.Height, r.Width, lineThickness, color, capType, capPoints);
            }
            else
            {
                DrawRectLinesPercentageHelper(r.TopRight, r.TopLeft, r.BottomLeft, r.BottomRight, perimeterToDraw, r.Width, r.Height, lineThickness, color, capType, capPoints);
            }
        }

    }

    /// <summary>
    /// Draws a certain percentage of a rotated rectangle's outline, starting at a specified corner and direction.
    /// </summary>
    /// <param name="topLeft">The top-left corner of the rectangle.</param>
    /// <param name="bottomRight">The bottom-right corner of the rectangle.</param>
    /// <param name="f">See <see cref="DrawRectLinesPercentage(Vector2, Vector2, float, float, ColorRgba, LineCapType, int)"/> for details.</param>
    /// <param name="pivot">The pivot point for rotation.</param>
    /// <param name="rotDeg">The rotation in degrees.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="capType">The type of line cap to use.</param>
    /// <param name="capPoints">The number of points for the cap.</param>
    public static void DrawRectLinesPercentage(Vector2 topLeft, Vector2 bottomRight, float f, Vector2 pivot, float rotDeg, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.Extended, int capPoints = 0)
    {
        var a = pivot + (topLeft - pivot).RotateDeg(rotDeg);
        var b = pivot + (new Vector2(topLeft.X, bottomRight.Y) - pivot).RotateDeg(rotDeg);
        var c = pivot + (bottomRight - pivot).RotateDeg(rotDeg);
        var d = pivot + (new Vector2(bottomRight.X, topLeft.Y) - pivot).RotateDeg(rotDeg);

        QuadDrawing.DrawQuadLinesPercentage(a, b, c, d, f, lineThickness, color, capType, capPoints);
    }

    /// <summary>
    /// Draws a certain percentage of a rotated rectangle's outline using the specified line drawing information.
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="f">See <see cref="DrawRectLinesPercentage(Vector2, Vector2, float, float, ColorRgba, LineCapType, int)"/> for details.</param>
    /// <param name="pivot">The pivot point for rotation.</param>
    /// <param name="rotDeg">The rotation in degrees.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="capType">The type of line cap to use.</param>
    /// <param name="capPoints">The number of points for the cap.</param>
    public static void DrawLinesPercentage(this Rect rect, float f, Vector2 pivot, float rotDeg, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.Extended, int capPoints = 0)
    {
        DrawRectLinesPercentage(rect.TopLeft, rect.BottomRight, f, pivot, rotDeg, lineThickness, color, capType, capPoints);
    }

    /// <summary>
    /// Draws a certain percentage of a rotated rectangle's outline using the specified line drawing information.
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="f">See <see cref="DrawRectLinesPercentage(Vector2, Vector2, float, float, ColorRgba, LineCapType, int)"/> for details.</param>
    /// <param name="pivot">The pivot point for rotation.</param>
    /// <param name="rotDeg">The rotation in degrees.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, etc.).</param>
    public static void DrawLinesPercentage(this Rect rect, float f, Vector2 pivot, float rotDeg, LineDrawingInfo lineInfo)
    {
        DrawRectLinesPercentage(rect.TopLeft, rect.BottomRight, f, pivot, rotDeg, lineInfo);
    }

    /// <summary>
    /// Draws a certain percentage of a rotated rectangle's outline using the specified line drawing information.
    /// </summary>
    /// <param name="topLeft">The top-left corner of the rectangle.</param>
    /// <param name="bottomRight">The bottom-right corner of the rectangle.</param>
    /// <param name="f">See <see cref="DrawRectLinesPercentage(Vector2, Vector2, float, float, ColorRgba, LineCapType, int)"/> for details.</param>
    /// <param name="pivot">The pivot point for rotation.</param>
    /// <param name="rotDeg">The rotation in degrees.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, etc.).</param>
    public static void DrawRectLinesPercentage(Vector2 topLeft, Vector2 bottomRight, float f, Vector2 pivot, float rotDeg, LineDrawingInfo lineInfo)
        => DrawRectLinesPercentage(topLeft, bottomRight, f, pivot, rotDeg, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);

    /// <summary>
    /// Draws a rectangle outline where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="r">The rectangle to draw.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, etc.).</param>
    /// <param name="rotDeg">The rotation in degrees.</param>
    /// <param name="pivot">The pivot point for rotation.</param>
    /// <param name="sideScaleFactor">
    /// <para>The scale factor for each side.</para>
        /// <list type="bullet">
        /// <item><description>0: No Rect is drawn.</description></item>
        /// <item><description>1: The normal Rect is drawn.</description></item>
        /// <item><description>0.5: Each side is half as long.</description></item>
        /// </list>
    /// </param>
    /// <param name="sideScaleOrigin">
    /// <para>The point along each side to scale from, in both directions (0 to 1).</para>
        /// <list type="bullet">
        /// <item><description>0: Start of Side</description></item>
        /// <item><description>0.5: Center of Side</description></item>
        /// <item><description>1: End of Side</description></item>
        /// </list>
    /// </param>
    /// <remarks>
    /// Useful for creating stylized or animated rectangles.
    /// </remarks>
    public static void DrawLinesScaled(this Rect r, LineDrawingInfo lineInfo, float rotDeg, Vector2 pivot, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0f) return;
        if (sideScaleFactor >= 1f)
        {
            r.DrawLines(pivot, rotDeg, lineInfo);
            return;
        }
        if (rotDeg == 0f)
        {
            SegmentDrawing.DrawSegment(r.TopLeft, r.BottomLeft, lineInfo, sideScaleFactor, sideScaleOrigin);
            SegmentDrawing.DrawSegment(r.BottomLeft, r.BottomRight, lineInfo, sideScaleFactor, sideScaleOrigin);
            SegmentDrawing.DrawSegment(r.BottomRight, r.TopRight, lineInfo, sideScaleFactor, sideScaleOrigin);
            SegmentDrawing.DrawSegment(r.TopRight, r.TopLeft, lineInfo, sideScaleFactor, sideScaleOrigin);
        }
        else
        {
            var corners = r.RotateCorners(pivot, rotDeg);
            SegmentDrawing.DrawSegment(corners.tl, corners.bl, lineInfo, sideScaleFactor, sideScaleOrigin);
            SegmentDrawing.DrawSegment(corners.bl, corners.br, lineInfo, sideScaleFactor, sideScaleOrigin);
            SegmentDrawing.DrawSegment(corners.br, corners.tr, lineInfo, sideScaleFactor, sideScaleOrigin);
            SegmentDrawing.DrawSegment(corners.tr, corners.tl, lineInfo, sideScaleFactor, sideScaleOrigin);
        }
    }

    /// <summary>
    /// Draws circles at each vertex of the rectangle.
    /// </summary>
    /// <param name="rect">The rectangle whose vertices to draw.</param>
    /// <param name="vertexRadius">The radius of each vertex circle.</param>
    /// <param name="color">The color of the vertex circles.</param>
    /// <param name="circleSegments">The number of segments for each circle (default: 8).</param>
    public static void DrawVertices(this Rect rect, float vertexRadius, ColorRgba color, int circleSegments = 8)
    {
        CircleDrawing.DrawCircle(rect.TopLeft, vertexRadius, color, circleSegments);
        CircleDrawing.DrawCircle(rect.TopRight, vertexRadius, color, circleSegments);
        CircleDrawing.DrawCircle(rect.BottomLeft, vertexRadius, color, circleSegments);
        CircleDrawing.DrawCircle(rect.BottomRight, vertexRadius, color, circleSegments);
    }

    /// <summary>
    /// Draws a filled rectangle with rounded corners.
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="roundness">The roundness of the corners (0 to 1).</param>
    /// <param name="segments">The number of segments to approximate the roundness.</param>
    /// <param name="color">The fill color.</param>
    public static void DrawRounded(this Rect rect, float roundness, int segments, ColorRgba color) => Raylib.DrawRectangleRounded(rect.Rectangle, roundness, segments, color.ToRayColor());

    /// <summary>
    /// Draws the outline of a rectangle with rounded corners.
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="roundness">The roundness of the corners (0 to 1).</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="segments">The number of segments to approximate the roundness.</param>
    /// <param name="color">The color of the outline.</param>
    public static void DrawRoundedLines(this Rect rect, float roundness, float lineThickness, int segments, ColorRgba color)
        => Raylib.DrawRectangleRoundedLinesEx(rect.Rectangle, roundness, segments, lineThickness * 2, color.ToRayColor());

    /// <summary>
    /// Draws a filled rectangle with slanted corners.
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="color">The fill color.</param>
    /// <param name="tlCorner">The slant amount for the top-left corner.</param>
    /// <param name="trCorner">The slant amount for the top-right corner.</param>
    /// <param name="brCorner">The slant amount for the bottom-right corner.</param>
    /// <param name="blCorner">The slant amount for the bottom-left corner.</param>
    /// <remarks>
    /// Uses absolute values for corner values from 0 to Min(width,height) of the rect.
    /// Therefore, the corner values should be positive and not exceed the smallest dimension of the rect.
    /// </remarks>
    public static void DrawSlantedCorners(this Rect rect, ColorRgba color, float tlCorner, float trCorner, float brCorner, float blCorner)
    {
        var points = rect.GetSlantedCornerPoints(tlCorner, trCorner, brCorner, blCorner);
        points.DrawPolygonConvex(rect.Center, color);
    }

    /// <summary>
    /// Draws a filled, rotated rectangle with slanted corners.
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="pivot">The pivot point for rotation.</param>
    /// <param name="rotDeg">The rotation in degrees.</param>
    /// <param name="color">The fill color.</param>
    /// <param name="tlCorner">The slant amount for the top-left corner.</param>
    /// <param name="trCorner">The slant amount for the top-right corner.</param>
    /// <param name="brCorner">The slant amount for the bottom-right corner.</param>
    /// <param name="blCorner">The slant amount for the bottom-left corner.</param>
    /// <remarks>
    /// Uses absolute values for corner values from 0 to Min(width,height) of the rect.
    /// Therefore, the corner values should be positive and not exceed the smallest dimension of the rect.
    /// </remarks>
    public static void DrawSlantedCorners(this Rect rect, Vector2 pivot, float rotDeg, ColorRgba color, float tlCorner, float trCorner, float brCorner, float blCorner)
    {
        var poly = rect.GetSlantedCornerPoints(tlCorner, trCorner, brCorner, blCorner);
        poly.ChangeRotation(rotDeg * ShapeMath.DEGTORAD, pivot);
        poly.DrawPolygonConvex(rect.Center, color);
    }

    /// <summary>
    /// Draws the outline of a rectangle with slanted corners.
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, etc.).</param>
    /// <param name="tlCorner">The slant amount for the top-left corner.</param>
    /// <param name="trCorner">The slant amount for the top-right corner.</param>
    /// <param name="brCorner">The slant amount for the bottom-right corner.</param>
    /// <param name="blCorner">The slant amount for the bottom-left corner.</param>
    /// <remarks>
    /// Uses absolute values for corner values from 0 to Min(width,height) of the rect.
    /// Therefore, the corner values should be positive and not exceed the smallest dimension of the rect.
    /// </remarks>
    public static void DrawSlantedCornersLines(this Rect rect, LineDrawingInfo lineInfo, float tlCorner, float trCorner, float brCorner, float blCorner)
    {
        var points = rect.GetSlantedCornerPoints(tlCorner, trCorner, brCorner, blCorner);
        points.DrawLines(lineInfo);
    }

    /// <summary>
    /// Draws the outline of a rotated rectangle with slanted corners.
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="pivot">The pivot point for rotation.</param>
    /// <param name="rotDeg">The rotation in degrees.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, etc.).</param>
    /// <param name="tlCorner">The slant amount for the top-left corner.</param>
    /// <param name="trCorner">The slant amount for the top-right corner.</param>
    /// <param name="brCorner">The slant amount for the bottom-right corner.</param>
    /// <param name="blCorner">The slant amount for the bottom-left corner.</param>
    /// <remarks>
    /// Uses absolute values for corner values from 0 to Min(width,height) of the rect.
    /// Therefore, the corner values should be positive and not exceed the smallest dimension of the rect.
    /// </remarks>
    public static void DrawSlantedCornersLines(this Rect rect, Vector2 pivot, float rotDeg, LineDrawingInfo lineInfo, float tlCorner, float trCorner, float brCorner, float blCorner)
    {
        var poly = rect.GetSlantedCornerPoints(tlCorner, trCorner, brCorner, blCorner);
        poly.ChangeRotation(rotDeg * ShapeMath.DEGTORAD, pivot);
        poly.DrawLines(lineInfo);
    }

    /// <summary>
    /// Draws corner lines for a rectangle, with each corner having a specified length.
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, etc.).</param>
    /// <param name="tlCorner">The length of the top-left corner lines.</param>
    /// <param name="trCorner">The length of the top-right corner lines.</param>
    /// <param name="brCorner">The length of the bottom-right corner lines.</param>
    /// <param name="blCorner">The length of the bottom-left corner lines.</param>
    /// <remarks>
    /// Uses absolute values for corner values from 0 to Min(width,height) of the rect.
    /// Therefore, the corner values should be positive and not exceed the smallest dimension of the rect.
    /// </remarks>
    public static void DrawCorners(this Rect rect, LineDrawingInfo lineInfo, float tlCorner, float trCorner, float brCorner, float blCorner)
    {
        var tl = rect.TopLeft;
        var tr = rect.TopRight;
        var br = rect.BottomRight;
        var bl = rect.BottomLeft;

        if (tlCorner > 0f)
        {
            SegmentDrawing.DrawSegment(tl, tl + new Vector2(MathF.Min(tlCorner, rect.Width), 0f), lineInfo);
            SegmentDrawing.DrawSegment(tl, tl + new Vector2(0f, MathF.Min(tlCorner, rect.Height)), lineInfo);
        }
        if (trCorner > 0f)
        {
            SegmentDrawing.DrawSegment(tr, tr - new Vector2(MathF.Min(trCorner, rect.Width), 0f), lineInfo);
            SegmentDrawing.DrawSegment(tr, tr + new Vector2(0f, MathF.Min(trCorner, rect.Height)), lineInfo);
        }
        if (brCorner > 0f)
        {
            SegmentDrawing.DrawSegment(br, br - new Vector2(MathF.Min(brCorner, rect.Width), 0f), lineInfo);
            SegmentDrawing.DrawSegment(br, br - new Vector2(0f, MathF.Min(brCorner, rect.Height)), lineInfo);
        }
        if (blCorner > 0f)
        {
            SegmentDrawing.DrawSegment(bl, bl + new Vector2(MathF.Min(blCorner, rect.Width), 0f), lineInfo);
            SegmentDrawing.DrawSegment(bl, bl - new Vector2(0f, MathF.Min(blCorner, rect.Height)), lineInfo);
        }
    }

    /// <summary>
    /// Draws corner lines for a rectangle, with all corners having the same length.
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, etc.).</param>
    /// <param name="cornerLength">
    /// The length of the corner lines for all corners.
    /// Uses an absolute value from 0 to Min(width,height) of the rect.
    /// Therefore, the corner value should be positive and not exceed the smallest dimension of the rect.
    /// </param>
    public static void DrawCorners(this Rect rect, LineDrawingInfo lineInfo, float cornerLength)
        => DrawCorners(rect, lineInfo, cornerLength, cornerLength, cornerLength, cornerLength);

    /// <summary>
    /// Draws corner lines for a rectangle, with each corner having a specified relative length (0 to 1).
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, etc.).</param>
    /// <param name="tlCorner">The relative length of the top-left corner lines (0-1).</param>
    /// <param name="trCorner">The relative length of the top-right corner lines (0-1).</param>
    /// <param name="brCorner">The relative length of the bottom-right corner lines (0-1).</param>
    /// <param name="blCorner">The relative length of the bottom-left corner lines (0-1).</param>
    public static void DrawCornersRelative(this Rect rect, LineDrawingInfo lineInfo, float tlCorner, float trCorner, float brCorner, float blCorner)
    {
        var tl = rect.TopLeft;
        var tr = rect.TopRight;
        var br = rect.BottomRight;
        var bl = rect.BottomLeft;

        if (tlCorner > 0f && tlCorner < 1f)
        {
            SegmentDrawing.DrawSegment(tl, tl + new Vector2(tlCorner * rect.Width, 0f), lineInfo);
            SegmentDrawing.DrawSegment(tl, tl + new Vector2(0f, tlCorner * rect.Height), lineInfo);
        }
        if (trCorner > 0f && trCorner < 1f)
        {
            SegmentDrawing.DrawSegment(tr, tr - new Vector2(tlCorner * rect.Width, 0f), lineInfo);
            SegmentDrawing.DrawSegment(tr, tr + new Vector2(0f, tlCorner * rect.Height), lineInfo);
        }
        if (brCorner > 0f && brCorner < 1f)
        {
            SegmentDrawing.DrawSegment(br, br - new Vector2(tlCorner * rect.Width, 0f), lineInfo);
            SegmentDrawing.DrawSegment(br, br - new Vector2(0f, tlCorner * rect.Height), lineInfo);
        }
        if (blCorner > 0f && blCorner < 1f)
        {
            SegmentDrawing.DrawSegment(bl, bl + new Vector2(tlCorner * rect.Width, 0f), lineInfo);
            SegmentDrawing.DrawSegment(bl, bl - new Vector2(0f, tlCorner * rect.Height), lineInfo);
        }
    }

    /// <summary>
    /// Draws corner lines for a rectangle, with all corners having the same relative length (0 to 1).
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, etc.).</param>
    /// <param name="cornerLengthFactor">The relative length of the corner lines for all corners (0 to 1).</param>
    public static void DrawCornersRelative(this Rect rect, LineDrawingInfo lineInfo, float cornerLengthFactor)
        => DrawCornersRelative(rect, lineInfo, cornerLengthFactor, cornerLengthFactor, cornerLengthFactor, cornerLengthFactor);

    private static void DrawRectLinesPercentageHelper(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, float perimeterToDraw, float size1, float size2, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        // Draw first segment
        var curP = p1;
        var nextP = p2;
        if (perimeterToDraw < size1)
        {
            float p = perimeterToDraw / size1;
            nextP = curP.Lerp(nextP, p);
            SegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, capType, capPoints);
            return;
        }

        SegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, capType, capPoints);
        perimeterToDraw -= size1;

        // Draw second segment
        curP = nextP;
        nextP = p3;
        if (perimeterToDraw < size2)
        {
            float p = perimeterToDraw / size2;
            nextP = curP.Lerp(nextP, p);
            SegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, capType, capPoints);
            return;
        }

        SegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, capType, capPoints);
        perimeterToDraw -= size2;

        // Draw third segment
        curP = nextP;
        nextP = p4;
        if (perimeterToDraw < size1)
        {
            float p = perimeterToDraw / size1;
            nextP = curP.Lerp(nextP, p);
            SegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, capType, capPoints);
            return;
        }

        SegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, capType, capPoints);
        perimeterToDraw -= size1;

        // Draw fourth segment
        curP = nextP;
        nextP = p1;
        if (perimeterToDraw < size2)
        {
            float p = perimeterToDraw / size2;
            nextP = curP.Lerp(nextP, p);
        }
        SegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, capType, capPoints);
    }

}