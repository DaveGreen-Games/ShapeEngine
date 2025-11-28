using System.Drawing;
using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;
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
    private static Polygon polygonHelper = new(12);
    
    #region Draw Masked
    
    /// <summary>
    /// Draws the rectangle's four side segments, but only where they intersect the given triangular mask.
    /// Each side is drawn by forwarding the call to the segment-level masked draw method.
    /// </summary>
    /// <param name="rect">The rectangle whose sides will be drawn.</param>
    /// <param name="mask">The triangular mask used to clip each side.</param>
    /// <param name="lineInfo">Line drawing parameters (thickness, color, cap style, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawLinesMasked(this Rect rect, Triangle mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        rect.TopSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.LeftSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.BottomSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.RightSegment.DrawMasked(mask, lineInfo, reversedMask);
    }
    /// <summary>
    /// Draws the rectangle's four side segments, but only where they intersect the given circular mask.
    /// Each side is drawn by forwarding the call to the segment-level masked draw method.
    /// </summary>
    /// <param name="rect">The rectangle whose sides will be drawn.</param>
    /// <param name="mask">The circular mask used to clip each side.</param>
    /// <param name="lineInfo">Line drawing parameters (thickness, color, cap style, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawLinesMasked(this Rect rect, Circle mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        rect.TopSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.LeftSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.BottomSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.RightSegment.DrawMasked(mask, lineInfo, reversedMask);
    }
    /// <summary>
    /// Draws the rectangle's four side segments, but only where they intersect the given rectangular mask.
    /// Each side is drawn by forwarding the call to the segment-level masked draw method which performs
    /// clipping against the provided <see cref="Rect"/> mask.
    /// </summary>
    /// <param name="rect">The rectangle whose sides will be drawn.</param>
    /// <param name="mask">The rectangular mask used to clip each side.</param>
    /// <param name="lineInfo">Line drawing parameters (thickness, color, cap style, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawLinesMasked(this Rect rect, Rect mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        rect.TopSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.LeftSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.BottomSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.RightSegment.DrawMasked(mask, lineInfo, reversedMask);
    }
    /// <summary>
    /// Draws the rectangle's four side segments, but only where they intersect the given quadrilateral mask.
    /// Each side is forwarded to the corresponding segment-level masked draw method which handles clipping against the provided <see cref="Quad"/> mask.
    /// </summary>
    /// <param name="rect">The rectangle whose sides will be drawn.</param>
    /// <param name="mask">The quadrilateral mask used to clip each side.</param>
    /// <param name="lineInfo">Line drawing parameters (thickness, color, cap style, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawLinesMasked(this Rect rect, Quad mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        rect.TopSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.LeftSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.BottomSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.RightSegment.DrawMasked(mask, lineInfo, reversedMask);
    }
    /// <summary>
    /// Draws the rectangle's four side segments, but only where they intersect the given polygon mask.
    /// Each side is drawn by forwarding the call to the segment-level masked draw method which performs
    /// clipping against the provided <see cref="Polygon"/> mask.
    /// </summary>
    /// <param name="rect">The rectangle whose sides will be drawn.</param>
    /// <param name="mask">The polygonal mask used to clip each side.</param>
    /// <param name="lineInfo">Line drawing parameters (thickness, color, cap style, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawLinesMasked(this Rect rect, Polygon mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        rect.TopSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.LeftSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.BottomSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.RightSegment.DrawMasked(mask, lineInfo, reversedMask);
    }
    /// <summary>
    /// Draws the rectangle's four side segments clipped against a generic closed-shape mask.
    /// </summary>
    /// <typeparam name="T">
    /// The mask type implementing <see cref="IClosedShapeTypeProvider"/> (for example <see cref="Circle"/>, <see cref="Polygon"/>, <see cref="Quad"/>, etc.).
    /// </typeparam>
    /// <param name="rect">The rectangle whose sides will be drawn.</param>
    /// <param name="mask">The mask used to clip each side.</param>
    /// <param name="lineInfo">Line drawing parameters (thickness, color, cap style, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawLinesMasked<T>(this Rect rect, T mask, LineDrawingInfo lineInfo, bool reversedMask = false) where T : IClosedShapeTypeProvider
    {
        rect.TopSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.LeftSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.BottomSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.RightSegment.DrawMasked(mask, lineInfo, reversedMask);
    }
    #endregion
    
    #region Draw
    
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
    /// Draws a filled rectangle using the specified color.
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="color">The fill color.</param>
    public static void Draw(this Rect rect, ColorRgba color)
    {
        Raylib.DrawRectangleRec(rect.Rectangle, color.ToRayColor());
    }

    #endregion
    
    #region Draw Rounded
    /// <summary>
    /// Draws a filled rectangle with rounded corners.
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="roundness">The roundness of the corners (0 to 1).</param>
    /// <param name="segments">The number of segments to approximate the roundness.</param>
    /// <param name="color">The fill color.</param>
    public static void DrawRounded(this Rect rect, float roundness, int segments, ColorRgba color)
    {
        QuadDrawing.DrawRoundedHelper(rect.TopLeft, rect.BottomLeft, rect.BottomRight, rect.TopRight, roundness, segments, color);
        // Raylib.DrawRectangleRounded(rect.Rectangle, roundness, segments, color.ToRayColor());
    }

    #endregion
    
    #region Draw Lines
    
    /// <summary>
    /// Draws the outline of a rectangle using the specified corners, line thickness, and color.
    /// </summary>
    /// <param name="topLeft">The top-left corner of the rectangle.</param>
    /// <param name="bottomRight">The bottom-right corner of the rectangle.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    public static void DrawRectLines(Vector2 topLeft, Vector2 bottomRight, float lineThickness, ColorRgba color)
    {
        DrawRectLinesHelper(new Rect(topLeft, bottomRight), lineThickness, color);
    }

    /// <summary>
    /// Draws the outline of a rectangle using the specified line drawing information.
    /// </summary>
    /// <param name="topLeft">The top-left corner of the rectangle.</param>
    /// <param name="bottomRight">The bottom-right corner of the rectangle.</param>
    /// <param name="lineInfo">The line drawing information used for drawing the outline. <see cref="LineDrawingInfo.CapType"/> is not used! </param>
    /// <param name="roundness">Can only be used when <see cref="LineDrawingInfo.CapPoints"/> is bigger than 0. Determines roundness of the corners. </param>
    public static void DrawRectLines(Vector2 topLeft, Vector2 bottomRight, LineDrawingInfo lineInfo, float roundness = 0f)
    {
        DrawRectLinesHelper(new Rect(topLeft, bottomRight), lineInfo.Thickness, lineInfo.Color, lineInfo.CapPoints, roundness);
    }
    
    /// <summary>
    /// Draws the outline of a rectangle using the specified line thickness and color.
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    public static void DrawLines(this Rect rect, float lineThickness, ColorRgba color)
    {
        DrawRectLinesHelper(rect, lineThickness, color);
    }

    /// <summary>
    /// Draws the outline of a rectangle using the specified line drawing information.
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="lineInfo">The line drawing information used for drawing the outline. <see cref="LineDrawingInfo.CapType"/> is not used! </param>
    /// <param name="roundness">Can only be used when <see cref="LineDrawingInfo.CapPoints"/> is bigger than 0. Determines roundness of the corners. </param>
    public static void DrawLines(this Rect rect, LineDrawingInfo lineInfo, float roundness = 0f)
    {
        DrawRectLinesHelper(rect, lineInfo.Thickness, lineInfo.Color, lineInfo.CapPoints, roundness);
    }
    #endregion
    
    #region Draw Lines Percentage
    public static void DrawRectLinesPercentage(Vector2 topLeft, Vector2 bottomRight, float f, float lineThickness, ColorRgba color, float roundness = 0,  int cornerPoints = 0)
    {
        var rect = new Rect(topLeft, bottomRight);
        QuadDrawing.DrawQuadLinesPercentage(rect.TopLeft, rect.BottomLeft, rect.BottomRight, rect.TopRight, f, lineThickness, color, roundness, cornerPoints);
        // if (f == 0) return;
        // var r = new Rect(topLeft, bottomRight);
        // if (r.Width <= 0 || r.Height <= 0) return;
        // if (MathF.Abs(f) >= 1f)
        // {
        //     DrawRectLines(topLeft, bottomRight, lineThickness, color);
        //     return;
        // }
        //
        // bool negative = false;
        // if (f < 0)
        // {
        //     negative = true;
        //     f *= -1;
        // }
        //
        // int startCorner = (int)f;
        // float percentage = f - startCorner;
        // if (percentage <= 0) return;
        //
        // startCorner = ShapeMath.Clamp(startCorner, 0, 3);
        //
        // var perimeter = r.Width * 2 + r.Height * 2;
        // var perimeterToDraw = perimeter * percentage;
        //
        // if (startCorner == 0)
        // {
        //     if (negative)
        //     {
        //         DrawRectLinesPercentageHelper(r.TopLeft, r.TopRight, r.BottomRight, r.BottomLeft, perimeterToDraw, r.Width, r.Height, lineThickness, color, capType, capPoints);
        //     }
        //     else
        //     {
        //         DrawRectLinesPercentageHelper(r.TopLeft, r.BottomLeft, r.BottomRight, r.TopRight, perimeterToDraw, r.Height, r.Width, lineThickness, color, capType, capPoints);
        //     }
        // }
        // else if (startCorner == 1)
        // {
        //     if (negative)
        //     {
        //         DrawRectLinesPercentageHelper(r.TopRight, r.BottomRight, r.BottomLeft, r.TopLeft, perimeterToDraw, r.Height, r.Width, lineThickness, color, capType, capPoints);
        //     }
        //     else
        //     {
        //         DrawRectLinesPercentageHelper(r.BottomLeft, r.BottomRight, r.TopRight, r.TopLeft, perimeterToDraw, r.Width, r.Height, lineThickness, color, capType, capPoints);
        //     }
        // }
        // else if (startCorner == 2)
        // {
        //     if (negative)
        //     {
        //         DrawRectLinesPercentageHelper(r.BottomRight, r.BottomLeft, r.TopLeft, r.TopRight, perimeterToDraw, r.Width, r.Height, lineThickness, color, capType, capPoints);
        //     }
        //     else
        //     {
        //         DrawRectLinesPercentageHelper(r.BottomRight, r.TopRight, r.TopLeft, r.BottomLeft, perimeterToDraw, r.Height, r.Width, lineThickness, color, capType, capPoints);
        //     }
        // }
        // else if (startCorner == 3)
        // {
        //     if (negative)
        //     {
        //         DrawRectLinesPercentageHelper(r.BottomLeft, r.TopLeft, r.TopRight, r.BottomRight, perimeterToDraw, r.Height, r.Width, lineThickness, color, capType, capPoints);
        //     }
        //     else
        //     {
        //         DrawRectLinesPercentageHelper(r.TopRight, r.TopLeft, r.BottomLeft, r.BottomRight, perimeterToDraw, r.Width, r.Height, lineThickness, color, capType, capPoints);
        //     }
        // }
    }

    public static void DrawLinesPercentage(this Rect rect, float f, float lineThickness, ColorRgba color, float roundness = 0,  int capPoints = 0)
    {
        QuadDrawing.DrawQuadLinesPercentage(rect.TopLeft, rect.BottomLeft, rect.BottomRight, rect.TopRight, f, lineThickness, color, roundness, capPoints);
    }

    public static void DrawLinesPercentage(this Rect rect, float f,  LineDrawingInfo lineInfo, float roundness = 0)
    {
        QuadDrawing.DrawQuadLinesPercentage(rect.TopLeft, rect.BottomLeft, rect.BottomRight, rect.TopRight, f, lineInfo.Thickness, lineInfo.Color, roundness, lineInfo.CapPoints);
    }

    public static void DrawRectLinesPercentage(Vector2 topLeft, Vector2 bottomRight, float f, LineDrawingInfo lineInfo, float roundness = 0)
    {
        var rect = new Rect(topLeft, bottomRight);
        QuadDrawing.DrawQuadLinesPercentage(rect.TopLeft, rect.BottomLeft, rect.BottomRight, rect.TopRight, f, lineInfo.Thickness, lineInfo.Color, roundness, lineInfo.CapPoints);
    }

    #endregion
    
    #region Draw Lines Scaled
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
        if(sideLengthFactor <= 0) return;
        if (sideLengthFactor >= 1)
        {
            DrawRectLines(topLeft, bottomRight, lineThickness, color);
            return;
        }
        
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
    /// Draws a rectangle outline where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="r">The rectangle to draw.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, etc.).</param>
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
    public static void DrawLinesScaled(this Rect r, LineDrawingInfo lineInfo,  float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0f) return;
        if (sideScaleFactor >= 1f)
        {
            r.DrawLines(lineInfo);
            return;
        }
        SegmentDrawing.DrawSegment(r.TopLeft, r.BottomLeft, lineInfo, sideScaleFactor, sideScaleOrigin);
        SegmentDrawing.DrawSegment(r.BottomLeft, r.BottomRight, lineInfo, sideScaleFactor, sideScaleOrigin);
        SegmentDrawing.DrawSegment(r.BottomRight, r.TopRight, lineInfo, sideScaleFactor, sideScaleOrigin);
        SegmentDrawing.DrawSegment(r.TopRight, r.TopLeft, lineInfo, sideScaleFactor, sideScaleOrigin);
    }

    #endregion

    #region Draw Grid

        /// <summary>
    /// Draws a grid within the specified bounds using the given line thickness and color.
    /// </summary>
    /// <param name="grid">The grid definition specifying the number of rows and columns.</param>
    /// <param name="bounds">The rectangle bounds in which to draw the grid.</param>
    /// <param name="lineThickness">The thickness of the grid lines.</param>
    /// <param name="color">The color of the grid lines.</param>
    /// <remarks>
    /// The grid is drawn using horizontal and vertical lines spaced according to the number of rows and columns.
    /// </remarks>
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

    #endregion

    #region Draw Nine Patch Rect
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

    #endregion

    #region Draw Slanted Corners
    
    /// <summary>
    /// Draws a filled rectangle with identical slanted corners on all four corners.
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="color">The fill color used for the slanted corners and remaining area.</param>
    /// <param name="cornerLength">
    /// The slant length applied to both horizontal and vertical directions.
    /// If smaller or equal to 0 the underlying implementation will draw the full rectangle.
    /// Values larger than half the rect dimension are handled by the called overload.
    /// </param>
    public static void DrawSlantedCorners(this Rect rect, ColorRgba color, float cornerLength)
    {
        DrawSlantedCorners(rect, color, cornerLength, cornerLength);
    }
    /// <summary>
    /// Draws a filled rectangle with identical slanted corners on all four corners,
    /// where the slant size is specified relative to the rectangle's half-size.
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="color">The fill color used for the slanted corners and remaining area.</param>
    /// <param name="cornerLengthFactor">
    /// A relative factor (0..1) that controls the slant length.
    /// 0 = no slant (draw full rectangle), 1 = maximum symmetric slant (half the rect dimension).
    /// Values outside this range are handled by the implementation (clamped or treated appropriately).
    /// </param>
    public static void DrawSlantedCornersRelative(this Rect rect, ColorRgba color, float cornerLengthFactor)
    {
        if(rect.Width <= 0 || rect.Height <= 0) return;
        if (cornerLengthFactor <= 0)
        {
            rect.Draw(color);
            return;
        }
        
        float halfWidth = rect.Width / 2f;
        float halfHeight = rect.Height / 2f;

        if (cornerLengthFactor >= 1f) cornerLengthFactor = 1f;
        DrawSlantedCorners(rect, color, halfWidth * cornerLengthFactor, halfHeight * cornerLengthFactor);
    }
    /// <summary>
    /// Draws a filled rectangle with independent slanted corners for horizontal and vertical directions.
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="color">The fill color used for the slanted corners and remaining area.</param>
    /// <param name="cornerLengthHorizontal">
    /// The slant length applied along the horizontal axis for each corner.
    /// Values &lt;= 0 cause the full rectangle to be drawn. Values larger than half the rectangle width
    /// are handled by the implementation and clamped or routed to alternate drawing logic.
    /// </param>
    /// <param name="cornerLengthVertical">
    /// The slant length applied along the vertical axis for each corner.
    /// Values &lt;= 0 cause the full rectangle to be drawn. Values larger than half the rectangle height
    /// are handled by the implementation and clamped or routed to alternate drawing logic.
    /// </param>
    /// <remarks>
    /// If the rectangle has non-positive width or height, nothing is drawn.
    /// This method composes the filled shape from triangles and handles the special cases when
    /// one or both slant lengths exceed the rectangle's half-dimensions.
    /// </remarks>
    public static void DrawSlantedCorners(this Rect rect, ColorRgba color, float cornerLengthHorizontal, float cornerLengthVertical)
    {
        if(rect.Width <= 0 || rect.Height <= 0) return;
        if (cornerLengthHorizontal <= 0 || cornerLengthVertical <= 0)
        {
            rect.Draw(color);
            return;
        }

        float halfWidth = rect.Width / 2f;
        float halfHeight = rect.Height / 2f;
        
        var tl = rect.TopLeft;
        var br = rect.BottomRight;
        
        if (cornerLengthHorizontal >= halfWidth && cornerLengthVertical >= halfHeight)
        {
            var p1 = tl + new Vector2(halfWidth, 0f);
            var p2 = tl + new Vector2(0f, halfHeight);
            var p3 = br - new Vector2(halfWidth, 0f);
            var p4 = br - new Vector2(0f, halfHeight);
            TriangleDrawing.DrawTriangle(p1, p2, p3, color);
            TriangleDrawing.DrawTriangle(p1, p3, p4, color);
            return;
        }
        
        var bl = rect.BottomLeft;
        var tr = rect.TopRight;


        if (cornerLengthHorizontal >= halfWidth)
        {
            var h = new Vector2(halfWidth, 0f);
            var v = new Vector2(0f, cornerLengthVertical);
            var top = tl + h;
            var bottom = bl + h;
            var tlV = tl + v;
            var blV = bl - v;
            var brV = br - v;
            var trV = tr + v;
            
            TriangleDrawing.DrawTriangle(top, tlV, blV, color);
            TriangleDrawing.DrawTriangle(top, blV, bottom, color);
            
            TriangleDrawing.DrawTriangle(top, bottom, brV, color);
            TriangleDrawing.DrawTriangle(top, brV, trV, color);
        }
        else if (cornerLengthVertical >= halfHeight)
        {
            var h = new Vector2(cornerLengthHorizontal, 0f);
            var v = new Vector2(0f, halfHeight);
            var left = tl + v;
            var right = tr + v;
            var tlH = tl + h;
            var blH = bl + h;
            var brH = br - h;
            var trH = tr - h;
            
            TriangleDrawing.DrawTriangle(tlH, left, blH, color);
            TriangleDrawing.DrawTriangle(tlH, blH, trH, color);
            
            TriangleDrawing.DrawTriangle(trH, blH, brH, color);
            TriangleDrawing.DrawTriangle(trH, brH, right, color);
        }
        else
        {
            var cornerHorizontal = new Vector2(cornerLengthHorizontal, 0f);
            var cornerVertical = new Vector2(0f, cornerLengthVertical);
            var tlH = tl + cornerHorizontal;
            var tlV = tl + cornerVertical;
        
            var blV = bl - cornerVertical;
            var blH = bl + cornerHorizontal;
        
            var brH = br - cornerHorizontal;
            var brV = br - cornerVertical;
       
            var trV = tr + cornerVertical;
            var trH = tr - cornerHorizontal;

            //left triangles
            TriangleDrawing.DrawTriangle(tlV, blV, tlH, color);
            TriangleDrawing.DrawTriangle(tlH, blV, blH, color);
        
            //center triangles
            TriangleDrawing.DrawTriangle(tlH, blH, trH, color);
            TriangleDrawing.DrawTriangle(trH, blH, brH, color);
        
            //right triangles
            TriangleDrawing.DrawTriangle(trH, brH, trV, color);
            TriangleDrawing.DrawTriangle(trV, brH, brV, color);
        }
    }
    /// <summary>
    /// Draws a filled rectangle with independent slanted corner sizes specified as relative factors.
    /// </summary>
    /// <param name="rect">The rectangle to draw. Nothing is drawn for non-positive width or height.</param>
    /// <param name="color">The fill color used for the slanted corners and remaining area.</param>
    /// <param name="cornerLengthFactorHorizontal">
    /// Relative horizontal slant factor (0..1):
    /// 0 = no slant (full rectangle), 1 = maximum symmetric slant (half the rectangle width).
    /// Values outside this range are clamped or handled by the implementation.
    /// </param>
    /// <param name="cornerLengthFactorVertical">
    /// Relative vertical slant factor (0..1):
    /// 0 = no slant (full rectangle), 1 = maximum symmetric slant (half the rectangle height).
    /// Values outside this range are clamped or handled by the implementation.
    /// </param>
    /// <remarks>
    /// This method computes absolute corner lengths from the provided relative factors and forwards to <see cref="DrawSlantedCorners(Rect, ColorRgba, float, float)"/>.
    /// </remarks>
    public static void DrawSlantedCornersRelative(this Rect rect, ColorRgba color, float cornerLengthFactorHorizontal, float cornerLengthFactorVertical)
    {
        if(rect.Width <= 0 || rect.Height <= 0) return;
        if (cornerLengthFactorHorizontal <= 0 && cornerLengthFactorVertical <= 0)
        {
            rect.Draw(color);
            return;
        }

        if (cornerLengthFactorHorizontal >= 1f) cornerLengthFactorHorizontal = 1f;
        if(cornerLengthFactorVertical >= 1f) cornerLengthFactorVertical = 1f;
        
        float cornerLengthH = cornerLengthFactorHorizontal * rect.Width * 0.5f;
        float cornerLengthV = cornerLengthFactorVertical * rect.Height * 0.5f;
        DrawSlantedCorners(rect, color, cornerLengthH, cornerLengthV);
    }
    /// <summary>
    /// Draws the outline (lines) of a rectangle with identical slanted corners on all four corners.
    /// </summary>
    /// <param name="rect">The rectangle to draw. Nothing is drawn for non-positive width or height.</param>
    /// <param name="thickness">The thickness of the outline lines.</param>
    /// <param name="color">The color of the outline lines.</param>
    /// <param name="cornerLength">
    /// The slant length applied along both horizontal and vertical axes for each corner.
    /// If &lt;= 0 the full rectangle is drawn. Values larger than half the rectangle dimension are handled by the implementation
    /// and result in an adjusted polygonal outline.
    /// </param>
    /// <remarks>
    /// The implementation builds a polygon representing the slanted-corner outline and draws its edges using the shared
    /// `polygonHelper`. Special cases where the slant exceeds half the rectangle dimension are handled by constructing
    /// an appropriate reduced polygon so the resulting outline remains valid.
    /// </remarks>
    public static void DrawSlantedCornersLines(this Rect rect, float thickness, ColorRgba color, float cornerLength)
    {
        DrawSlantedCornersLines(rect, thickness, color, cornerLength, cornerLength);
    }
    /// <summary>
    /// Draws the outline (lines) of a rectangle with slanted corners using a relative corner length factor.
    /// </summary>
    /// <param name="rect">The rectangle to draw. Nothing is drawn for non-positive width or height.</param>
    /// <param name="thickness">The thickness of the outline lines.</param>
    /// <param name="color">The color of the outline lines.</param>
    /// <param name="cornerLengthFactor">
    /// Relative slant factor (0..1):
    /// 0 = no slant (full rectangle),
    /// 1 = maximum symmetric slant (half the rectangle dimension).
    /// Values outside this range are clamped or handled by the implementation.
    /// </param>
    /// <remarks>
    /// This overload forwards to the two-parameter relative overload using the same factor for both horizontal and vertical slants.
    /// </remarks>
    public static void DrawSlantedCornersRelativeLines(this Rect rect, float thickness, ColorRgba color, float cornerLengthFactor)
    {
        DrawSlantedCornersRelativeLines(rect, thickness, color, cornerLengthFactor, cornerLengthFactor);
    }
    /// <summary>
    /// Draws the outline (lines) of a rectangle with slanted corners using independent
    /// horizontal and vertical corner lengths.
    /// </summary>
    /// <param name="rect">The rectangle to draw. Nothing is drawn for non-positive width or height.</param>
    /// <param name="thickness">The thickness of the outline lines.</param>
    /// <param name="color">The color of the outline lines.</param>
    /// <param name="cornerLengthHorizontal">
    /// The slant length applied along the horizontal axis for each corner.
    /// Values &lt;= 0 cause the full rectangle to be drawn. Values larger than half the rectangle width
    /// are handled by the implementation and clamped or routed to alternate drawing logic.
    /// </param>
    /// <param name="cornerLengthVertical">
    /// The slant length applied along the vertical axis for each corner.
    /// Values &lt;= 0 cause the full rectangle to be drawn. Values larger than half the rectangle height
    /// are handled by the implementation and clamped or routed to alternate drawing logic.
    /// </param>
    /// <remarks>
    /// The method builds a polygon representing the slanted-corner outline and draws its edges
    /// using an internal polygon helper. Special cases where one or both slant lengths exceed
    /// the rectangle's half-dimensions are handled to ensure a valid outline.
    /// </remarks>
    public static void DrawSlantedCornersLines(this Rect rect, float thickness, ColorRgba color, float cornerLengthHorizontal, float cornerLengthVertical)
    {
        if(rect.Width <= 0 || rect.Height <= 0) return;
        if (cornerLengthHorizontal <= 0 || cornerLengthVertical <= 0)
        {
            rect.Draw(color);
            return;
        }

        float halfWidth = rect.Width / 2f;
        float halfHeight = rect.Height / 2f;
        
        var tl = rect.TopLeft;
        var br = rect.BottomRight;
        
        if (cornerLengthHorizontal >= halfWidth && cornerLengthVertical >= halfHeight)
        {
            polygonHelper.Clear();
            polygonHelper.Add(tl + new Vector2(halfWidth, 0f));
            polygonHelper.Add(tl + new Vector2(0f, halfHeight));
            polygonHelper.Add(br - new Vector2(halfWidth, 0f));
            polygonHelper.Add(br - new Vector2(0f, halfHeight));
            polygonHelper.DrawLines(thickness, color, LineCapType.None, 0);
            return;
        }
        
        var bl = rect.BottomLeft;
        var tr = rect.TopRight;
        
        if (cornerLengthHorizontal >= halfWidth)
        {
            var h = new Vector2(halfWidth, 0f);
            var v = new Vector2(0f, cornerLengthVertical);
            
            polygonHelper.Clear();
            polygonHelper.Add(tl + h);
            polygonHelper.Add(tl + v);
            polygonHelper.Add(bl - v);
            polygonHelper.Add(bl + h);
            polygonHelper.Add(br - v);
            polygonHelper.Add(tr + v);
            polygonHelper.DrawLines(thickness, color, LineCapType.None, 0);
            
            
        }
        else if (cornerLengthVertical >= halfHeight)
        {
            var h = new Vector2(cornerLengthHorizontal, 0f);
            var v = new Vector2(0f, halfHeight);

            polygonHelper.Clear();
            polygonHelper.Add(tl + h);
            polygonHelper.Add(tl + v);
            polygonHelper.Add(bl + h);
            polygonHelper.Add(br - h);
            polygonHelper.Add(tr + v);
            polygonHelper.Add(tr - h);
            polygonHelper.DrawLines(thickness, color, LineCapType.None, 0);
            
        }
        else
        {
            var cornerHorizontal = new Vector2(cornerLengthHorizontal, 0f);
            var cornerVertical = new Vector2(0f, cornerLengthVertical);

            polygonHelper.Clear();
            polygonHelper.Add(tl + cornerHorizontal);
            polygonHelper.Add(tl + cornerVertical);
            polygonHelper.Add(bl - cornerVertical);
            polygonHelper.Add(bl + cornerHorizontal);
            polygonHelper.Add(br - cornerHorizontal);
            polygonHelper.Add(br - cornerVertical);
            polygonHelper.Add(tr + cornerVertical);
            polygonHelper.Add(tr - cornerHorizontal);
            polygonHelper.DrawLines(thickness, color, LineCapType.None, 0);
           
        }
    }
    /// <summary>
    /// Draws the outline (lines) of a rectangle with slanted corners using relative horizontal and vertical corner factors.
    /// </summary>
    /// <param name="rect">The rectangle to draw. Nothing is drawn for non-positive width or height.</param>
    /// <param name="thickness">The thickness of the outline lines.</param>
    /// <param name="color">The color of the outline lines.</param>
    /// <param name="cornerLengthFactorHorizontal">
    /// Relative horizontal slant factor (0..1):
    /// 0 = no slant (full rectangle),
    /// 1 = maximum symmetric slant (half the rectangle width).
    /// Values outside this range are clamped or handled by the implementation.
    /// </param>
    /// <param name="cornerLengthFactorVertical">
    /// Relative vertical slant factor (0..1):
    /// 0 = no slant (full rectangle),
    /// 1 = maximum symmetric slant (half the rectangle height).
    /// Values outside this range are clamped or handled by the implementation.
    /// </param>
    /// <remarks>
    /// This method computes absolute corner lengths from the provided relative factors and forwards to
    /// the two-parameter overload that performs the actual outline drawing.
    /// </remarks>
    public static void DrawSlantedCornersRelativeLines(this Rect rect, float thickness, ColorRgba color, float cornerLengthFactorHorizontal, float cornerLengthFactorVertical)
    {
        var halfWidth = rect.Width / 2f;
        var halfHeight = rect.Height / 2f;
        if(cornerLengthFactorHorizontal >= 1f) cornerLengthFactorHorizontal = 1f;
        if(cornerLengthFactorVertical >= 1f) cornerLengthFactorVertical = 1f;
        float cornerLengthH = cornerLengthFactorHorizontal * halfWidth;
        float cornerLengthV = cornerLengthFactorVertical * halfHeight;
        DrawSlantedCornersLines(rect, thickness, color, cornerLengthH, cornerLengthV);
    }
    
    
    
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
        polygonHelper.Clear();
        FillSlantedCornerPoints(rect, tlCorner, trCorner, brCorner, blCorner, ref polygonHelper);
        polygonHelper.DrawPolygonConvex(rect.Center, color);

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
        polygonHelper.Clear();
        FillSlantedCornerPoints(rect, tlCorner, trCorner, brCorner, blCorner, ref polygonHelper);
        polygonHelper.ChangeRotation(rotDeg * ShapeMath.DEGTORAD, pivot);
        polygonHelper.DrawPolygonConvex(rect.Center, color);
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
        polygonHelper.Clear();
        FillSlantedCornerPoints(rect, tlCorner, trCorner, brCorner, blCorner, ref polygonHelper);
        polygonHelper.DrawLines(lineInfo);
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
        polygonHelper.Clear();
        FillSlantedCornerPoints(rect, tlCorner, trCorner, brCorner, blCorner, ref polygonHelper);
        polygonHelper.ChangeRotation(rotDeg * ShapeMath.DEGTORAD, pivot);
        polygonHelper.DrawLines(lineInfo);
    }
    
    private static void FillSlantedCornerPoints(Rect rect, float tlCorner, float trCorner, float brCorner, float blCorner, ref Polygon points)
    {
        var halfWidth = rect.Width / 2f;
        var halfHeight = rect.Height / 2f;
        if (tlCorner <= 0)
        {
            points.Add(rect.TopLeft);
        }
        else
        {
            points.Add(rect.TopLeft + new Vector2(MathF.Min(tlCorner, halfWidth), 0f));
            points.Add(rect.TopLeft + new Vector2(0f, MathF.Min(tlCorner, halfHeight)));
        }
        
        if (blCorner <= 0)
        {
            points.Add(rect.BottomLeft);
        }
        else
        {
            if (blCorner < halfHeight)
            {
                points.Add(rect.BottomLeft - new Vector2(0f, MathF.Min(blCorner, halfHeight)));
            }
            
            if (blCorner < halfWidth)
            {
                points.Add(rect.BottomLeft + new Vector2(MathF.Min(blCorner, halfWidth), 0f));
            }
        }
        
        if (brCorner <= 0)
        {
            points.Add(rect.BottomRight);
        }
        else
        {
            points.Add(rect.BottomRight - new Vector2(MathF.Min(brCorner, halfWidth), 0f));
            points.Add(rect.BottomRight - new Vector2(0f, MathF.Min(brCorner, halfHeight)));
        }
        
        if (trCorner <= 0)
        {
            points.Add(rect.TopRight);
        }
        else
        {
            if (trCorner < halfHeight)
            {
                points.Add(rect.TopRight + new Vector2(0f, MathF.Min(trCorner, halfHeight)));
            }
            
            if (trCorner < halfWidth)
            {
                points.Add(rect.TopRight - new Vector2(MathF.Min(trCorner, halfWidth), 0f));
            }
        }
    }
    
    #endregion
    
    #region Draw Corners
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
        if(lineInfo.Thickness <= 0f || lineInfo.Color.A <= 0 || rect.Width <= 0 || rect.Height <= 0) return;
        
        var tl = rect.TopLeft;
        var tr = rect.TopRight;
        var br = rect.BottomRight;
        var bl = rect.BottomLeft;

        var nL = new Vector2(-1, 0f);
        var nR = new Vector2(1, 0f);
        var nU = new Vector2(0f, -1);
        var nD = new Vector2(0f, 1);
        var miterLength = MathF.Sqrt(lineInfo.Thickness * lineInfo.Thickness * 2f);
        var halfWidth = rect.Width / 2f;
        var halfHeight = rect.Height / 2f;

        if (tlCorner > 0f)
        {
            DrawRectCornerSharp(tl, nU, nL, MathF.Min(tlCorner, halfHeight), MathF.Min(tlCorner, halfWidth), lineInfo.Thickness, miterLength,
                lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
        }

        if (trCorner > 0f)
        {
            DrawRectCornerSharp(tr, nR, nU, MathF.Min(trCorner, halfWidth), MathF.Min(trCorner, halfHeight), lineInfo.Thickness, miterLength,
                lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
        }

        if (brCorner > 0f)
        {
            DrawRectCornerSharp(br, nD, nR, MathF.Min(brCorner, halfHeight), MathF.Min(brCorner, halfWidth), lineInfo.Thickness, miterLength,
                lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
        }

        if (blCorner > 0f)
        {
            DrawRectCornerSharp(bl, nL, nD, MathF.Min(blCorner, halfWidth), MathF.Min(blCorner, halfHeight), lineInfo.Thickness, miterLength,
                lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
        }
        

        // if (lineInfo.CapPoints <= 0)
        // {
        //     var tl = rect.TopLeft;
        //     var tr = rect.TopRight;
        //     var br = rect.BottomRight;
        //     var bl = rect.BottomLeft;
        //
        //     var nL = new Vector2(-1, 0f);
        //     var nR = new Vector2(1, 0f);
        //     var nU = new Vector2(0f, -1);
        //     var nD = new Vector2(0f, 1);
        //     var miterLength = MathF.Sqrt(lineInfo.Thickness * lineInfo.Thickness * 2f);
        //     var halfWidth = rect.Width / 2f;
        //     var halfHeight = rect.Height / 2f;
        //
        //     if (tlCorner > 0f)
        //     {
        //         DrawRectCornerSharp(tl, nU, nL, MathF.Min(tlCorner, halfHeight), MathF.Min(tlCorner, halfWidth), lineInfo.Thickness, miterLength,
        //             lineInfo.Color);
        //     }
        //
        //     if (trCorner > 0f)
        //     {
        //         DrawRectCornerSharp(tr, nR, nU, MathF.Min(trCorner, halfWidth), MathF.Min(trCorner, halfHeight), lineInfo.Thickness, miterLength,
        //             lineInfo.Color);
        //     }
        //
        //     if (brCorner > 0f)
        //     {
        //         DrawRectCornerSharp(br, nD, nR, MathF.Min(brCorner, halfHeight), MathF.Min(brCorner, halfWidth), lineInfo.Thickness, miterLength,
        //             lineInfo.Color);
        //     }
        //
        //     if (blCorner > 0f)
        //     {
        //         DrawRectCornerSharp(bl, nL, nD, MathF.Min(blCorner, halfWidth), MathF.Min(blCorner, halfHeight), lineInfo.Thickness, miterLength,
        //             lineInfo.Color);
        //     }
        // }
        // else
        // {
        //     const float roundness = 0.53f;
        //     float radius = (rect.Width > rect.Height) ? (rect.Height * roundness) / 2f : (rect.Width * roundness) / 2f;
        //     if (radius <= 0f) return;
        //
        //     if (tlCorner > 0f)
        //     {
        //         DrawRectCornerRounded(rect.TopLeft, new Vector2(0, -1), new Vector2(-1, 0), radius, lineInfo.CapPoints, lineInfo.Thickness, lineInfo.Color);
        //     }
        //
        //     if (blCorner > 0f)
        //     {
        //         DrawRectCornerRounded(rect.BottomLeft, new Vector2(-1, 0), new Vector2(0, 1), radius, lineInfo.CapPoints, lineInfo.Thickness, lineInfo.Color);
        //     }
        //
        //     if (brCorner > 0f)
        //     {
        //         DrawRectCornerRounded(rect.BottomRight, new Vector2(0, 1), new Vector2(1, 0), radius, lineInfo.CapPoints, lineInfo.Thickness, lineInfo.Color);
        //     }
        //
        //     if (trCorner > 0f)
        //     {
        //         DrawRectCornerRounded(rect.TopRight, new Vector2(1, 0), new Vector2(0, -1), radius, lineInfo.CapPoints, lineInfo.Thickness, lineInfo.Color);
        //     }
        // }
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
    {
        DrawCorners(rect, lineInfo, cornerLength, cornerLength, cornerLength, cornerLength);
    }

    /// <summary>
    /// Draws corner lines for a rectangle, with each corner having a specified relative length (0 to 1).
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, etc.).</param>
    /// <param name="tlCornerFactor">The relative length of the top-left corner lines (0-1).</param>
    /// <param name="trCornerFactor">The relative length of the top-right corner lines (0-1).</param>
    /// <param name="brCornerFactor">The relative length of the bottom-right corner lines (0-1).</param>
    /// <param name="blCornerFactor">The relative length of the bottom-left corner lines (0-1).</param>
    public static void DrawCornersRelative(this Rect rect, LineDrawingInfo lineInfo, float tlCornerFactor, float trCornerFactor, float brCornerFactor, float blCornerFactor)
    {
        var minSize = MathF.Min(rect.Width, rect.Height);
        DrawCorners(rect, lineInfo, tlCornerFactor * minSize, trCornerFactor * minSize, brCornerFactor * minSize, blCornerFactor * minSize);
    }

    /// <summary>
    /// Draws corner lines for a rectangle, with all corners having the same relative length (0 to 1).
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, etc.).</param>
    /// <param name="cornerLengthFactor">The relative length of the corner lines for all corners (0 to 1).</param>
    public static void DrawCornersRelative(this Rect rect, LineDrawingInfo lineInfo, float cornerLengthFactor)
    {
        DrawCornersRelative(rect, lineInfo, cornerLengthFactor, cornerLengthFactor, cornerLengthFactor, cornerLengthFactor);
    }

    #endregion
    
    #region Draw Vertices
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
    #endregion
    
    #region Helper
    private static void DrawRectLinesHelper(Rect rect, float thickness, ColorRgba color, int cornerPoints = 0, float roundness = 0f)
    {
        if (cornerPoints <= 0 || roundness <= 0f)
        {
            // rect = rect.ChangeSize(thickness * 2, AnchorPoint.Center);//To make it consistent with ShapeEngine rect drawing
            // Raylib.DrawRectangleLinesEx(rect.Rectangle, thickness * 2, color.ToRayColor());
            QuadDrawing.DrawQuadLines(rect.TopLeft, rect.BottomLeft, rect.BottomRight, rect.TopRight, thickness, color);
        }
        else
        {
            // rect = rect.ChangeSize(-thickness * 2, AnchorPoint.Center);//To make it consistent with ShapeEngine rect drawing
            // Raylib.DrawRectangleRoundedLinesEx(rect.Rectangle, roundness, cornerPoints, thickness * 2, color.ToRayColor());
            QuadDrawing.DrawLinesRoundedHelper(rect.TopLeft, rect.BottomLeft, rect.BottomRight, rect.TopRight, roundness, cornerPoints, thickness, color);
        }
    }
    private static void DrawRectCornerSharp(Vector2 p, Vector2 n1, Vector2 n2, float cornerLength1, float cornerLength2, float thickness, float miterLength, ColorRgba color, LineCapType capType = LineCapType.None, int capPoints = 0)
    {
        if (capType == LineCapType.Extended || (capType is LineCapType.Capped or LineCapType.CappedExtended && capPoints > 0))
        {
            cornerLength1 = MathF.Max(thickness, cornerLength1);
            cornerLength2 = MathF.Max(thickness, cornerLength2);
        }
        var miterDir = (n1 + n2).Normalize();
        var maxMiterLength = MathF.Sqrt(cornerLength1 * cornerLength1 + cornerLength2 * cornerLength2);
        miterLength = MathF.Min(miterLength, maxMiterLength);
        
        var innerMiter = p - miterDir * miterLength;
        var end1 = p - n1 * cornerLength1;
        var end2 = p - n2 * cornerLength2;
        if (capType == LineCapType.Extended)
        {
            end1 -= n1 * thickness;
            end2 -= n2 * thickness;
        }
        else if (capType == LineCapType.Capped && capPoints > 0)
        {
            end1 += n1 * thickness;
            end2 += n2 * thickness;
        }
        
        var end1Inner = end1 - n2 * thickness;
        var end2Inner = end2 - n1 * thickness;
        
        var outerMiter = p + miterDir * miterLength;
        var end1Outer = end1 + n2 * thickness;
        var end2Outer = end2 + n1 * thickness;
        TriangleDrawing.DrawTriangle(outerMiter, end1Outer, innerMiter, color);
        TriangleDrawing.DrawTriangle(end1Outer, end1Inner, innerMiter, color);
        TriangleDrawing.DrawTriangle(outerMiter, innerMiter, end2Outer, color); 
        TriangleDrawing.DrawTriangle(innerMiter, end2Inner, end2Outer, color);

        if (capType is LineCapType.Capped or LineCapType.CappedExtended && capPoints > 0)
        {
            SegmentDrawing.DrawRoundCap(end1, -n1, thickness, capPoints, color);
            SegmentDrawing.DrawRoundCap(end2, -n2, thickness, capPoints, color);
        }
        
        
        // if (insideOnly)
        // {
        //     TriangleDrawing.DrawTriangle(p, end1, innerMiter, color);
        //     TriangleDrawing.DrawTriangle(end1, end1Inner, innerMiter, color);
        //     TriangleDrawing.DrawTriangle(p, innerMiter, end2, color); 
        //     TriangleDrawing.DrawTriangle(innerMiter, end2Inner, end2, color); 
        // }
        // else
        // {
        //     var outerMiter = p + miterDir * miterLength;
        //     var end1Outer = end1 + n2 * thickness;
        //     var end2Outer = end2 + n1 * thickness;
        //     TriangleDrawing.DrawTriangle(outerMiter, end1Outer, innerMiter, color);
        //     TriangleDrawing.DrawTriangle(end1Outer, end1Inner, innerMiter, color);
        //     TriangleDrawing.DrawTriangle(outerMiter, innerMiter, end2Outer, color); 
        //     TriangleDrawing.DrawTriangle(innerMiter, end2Inner, end2Outer, color); 
        // }
        
        
    }
    #endregion
}
// private static void DrawRectLinesPercentageHelper(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, float perimeterToDraw, float size1, float size2, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    // {
    //     //NOTE: Should handled rounded corners as well (with corner points and roundness like DrawLines/Draw functions)
    //     
    //     //TODO: Fix with new system
    //     // - Remove cap type for percentage drawing - its either sharp (cap points <= 0 or round (cap points > 0)
    //     
    //     // Draw first segment
    //     var curP = p1;
    //     var nextP = p2;
    //     if (perimeterToDraw < size1)
    //     {
    //         float p = perimeterToDraw / size1;
    //         nextP = curP.Lerp(nextP, p);
    //         SegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, capType, capPoints);
    //         return;
    //     }
    //
    //     SegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, capType, capPoints);
    //     perimeterToDraw -= size1;
    //
    //     // Draw second segment
    //     curP = nextP;
    //     nextP = p3;
    //     if (perimeterToDraw < size2)
    //     {
    //         float p = perimeterToDraw / size2;
    //         nextP = curP.Lerp(nextP, p);
    //         SegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, capType, capPoints);
    //         return;
    //     }
    //
    //     SegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, capType, capPoints);
    //     perimeterToDraw -= size2;
    //
    //     // Draw third segment
    //     curP = nextP;
    //     nextP = p4;
    //     if (perimeterToDraw < size1)
    //     {
    //         float p = perimeterToDraw / size1;
    //         nextP = curP.Lerp(nextP, p);
    //         SegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, capType, capPoints);
    //         return;
    //     }
    //
    //     SegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, capType, capPoints);
    //     perimeterToDraw -= size1;
    //
    //     // Draw fourth segment
    //     curP = nextP;
    //     nextP = p1;
    //     if (perimeterToDraw < size2)
    //     {
    //         float p = perimeterToDraw / size2;
    //         nextP = curP.Lerp(nextP, p);
    //     }
    //     SegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, capType, capPoints);
    // }
    //
 /*
    //TODO: Make usable for draw rect rounded and draw rect rounded lines
    //Translate of DrawRectangleRoundedLinesEx from raylib (C) to C#
    //Draw rectangle with rounded edges outline
    public static void DrawRectangleRoundedLinesEx(Rect rec, float roundness, int segments, float lineThick, ColorRgba color)
    {
        if (lineThick < 0f) lineThick = 0f;
    
        // Not a rounded rectangle -> fallback to rectangle lines (expand by thickness)
        if (roundness <= 0f)
        {
            // Expand the rect by line thickness so it matches C behaviour
            var exp = new Rect(rec.X - lineThick, rec.Y - lineThick, rec.Width + 2f * lineThick, rec.Height + 2f * lineThick);
            Raylib.DrawRectangleLinesEx(exp.Rectangle, lineThick, color.ToRayColor());
            return;
        }
    
        if (roundness >= 1.0f) roundness = 1.0f;
    
        // Calculate corner radius
        float radius = (rec.Width > rec.Height) ? (rec.Height * roundness) / 2f : (rec.Width * roundness) / 2f;
        if (radius <= 0f) return;
    
        // If segments not provided or too small, compute a reasonable default
        if (segments < 4)
        {
            const float SMOOTH_CIRCLE_ERROR_RATE = 0.5f;
            float th = MathF.Acos(2f * MathF.Pow(1f - SMOOTH_CIRCLE_ERROR_RATE / radius, 2f) - 1f);
            // Follow original logic: segments = (int)(ceilf(2*PI/th)/2.0f);
            float raw = MathF.Ceiling((2f * MathF.PI) / th);
            segments = (int)(raw / 2f);
            if (segments <= 0) segments = 4;
        }
    
        float stepLength = 90.0f / (float)segments; // degrees per segment on each corner
        float outerRadius = radius + lineThick;
        float innerRadius = radius;
    
        // Corner centers (clockwise from top-left)
        var topLeftCenter = new Vector2(rec.X + radius, rec.Y + radius);
        var topRightCenter = new Vector2(rec.X + rec.Width - radius, rec.Y + radius);
        var bottomRightCenter = new Vector2(rec.X + rec.Width - radius, rec.Y + rec.Height - radius);
        var bottomLeftCenter = new Vector2(rec.X + radius, rec.Y + rec.Height - radius);
    
        // Angles for corners in degrees (clockwise around rect)
        // top-left: 180 -> 270
        // top-right: 270 -> 360
        // bottom-right: 0 -> 90
        // bottom-left: 90 -> 180
        var cornerStarts = new float[] { 180f, 270f, 0f, 90f };
        var centers = new Vector2[] { topLeftCenter, topRightCenter, bottomRightCenter, bottomLeftCenter };
    
        // Build points for outer and inner arcs (include endpoints so we can seamlessly stitch corners)
        List<Vector2> outerPoints = [];
        List<Vector2> innerPoints = [];
    
        const float deg2Rad = MathF.PI / 180f;
    
        for (int c = 0; c < 4; c++)
        {
            float startAng = cornerStarts[c];
            Vector2 center = centers[c];
    
            for (int i = 0; i <= segments; i++) // inclusive to include corner endpoints
            {
                float ang = startAng + i * stepLength;
                float rad = ang * deg2Rad;
    
                float cos = MathF.Cos(rad);
                float sin = MathF.Sin(rad);
    
                Vector2 outerP = new Vector2(center.X + cos * outerRadius, center.Y + sin * outerRadius);
                Vector2 innerP = new Vector2(center.X + cos * innerRadius, center.Y + sin * innerRadius);
    
                outerPoints.Add(outerP);
                innerPoints.Add(innerP);
            }
        }
    
        int count = innerPoints.Count;
        if (count < 2) return;
    
        // // If thin lines, just draw lines following the inner polyline
        // if (lineThick <= 1.0f)
        // {
        //     for (int i = 0; i < count; i++)
        //     {
        //         Vector2 a = innerPoints[i];
        //         Vector2 b = innerPoints[(i + 1) % count];
        //         // Use DrawLineEx to allow anti-aliased thicker lines where supported
        //         Raylib.DrawLineEx(a, b, MathF.Max(1f, lineThick), color);
        //     }
        //     return;
        // }
    
        // Thick outline: draw quads between outer and inner loops using two triangles per segment
        for (var i = 0; i < count; i++)
        {
            int next = (i + 1) % count;
    
            var o1 = outerPoints[i];
            var o2 = outerPoints[next];
            var i1 = innerPoints[i];
            var i2 = innerPoints[next];
    
            // Draw two triangles that form the quad between o1-o2-i2-i1
            Raylib.DrawTriangle(o1, i1, i2, color.ToRayColor());
            Raylib.DrawTriangle(o1, i2, o2, color.ToRayColor());
        }
    }
    */
