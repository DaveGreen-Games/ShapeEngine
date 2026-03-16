
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
    #region Draw
    
    /// <summary>
    /// Draws a filled axis-aligned rectangle from two corner points.
    /// </summary>
    /// <param name="topLeft">The rectangle's top-left corner.</param>
    /// <param name="bottomRight">The rectangle's bottom-right corner.</param>
    /// <param name="color">The fill color.</param>
    public static void DrawRect(Vector2 topLeft, Vector2 bottomRight, ColorRgba color)
    {
        Raylib.DrawRectangleV(topLeft, bottomRight - topLeft, color.ToRayColor());
    }
    
    /// <summary>
    /// Draws a filled rectangle.
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="color">The fill color.</param>
    public static void Draw(this Rect rect, ColorRgba color)
    {
        Raylib.DrawRectangleV(rect.TopLeft, rect.BottomRight - rect.TopLeft, color.ToRayColor());
    }
    #endregion

    #region Draw Rounded
    /// <summary>
    /// Draws a filled rectangle with rounded corners.
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="color">The fill color.</param>
    /// <param name="roundness">The normalized corner roundness passed to Raylib.</param>
    /// <param name="segments">The number of segments used for each rounded corner.</param>
    /// <remarks>
    /// This method forwards to Raylib's rounded rectangle drawing API.
    /// </remarks>
    public static void DrawRounded(this Rect rect, ColorRgba color, float roundness, int segments)
    {
        Raylib.DrawRectangleRounded(rect.Rectangle, roundness, segments, color);
    }

    #endregion
    
    #region Draw Scaled
    /// <summary>
    /// Draws a rect with scaled sides based on a specific draw type.
    /// </summary>
    /// <param name="r">The rect to draw.</param>
    /// <param name="color">The color of the drawn shape.</param>
    /// <param name="sideScaleFactor">The scale factor of the sides (0 to 1). If >= 1, the full quad is drawn. If &lt;= 0, nothing is drawn.</param>
    /// <param name="sideScaleOrigin">The origin point for scaling the sides (0 = start, 1 = end, 0.5 = center).</param>
    /// <param name="drawType">
    /// The style of drawing:
    /// <list type="bullet">
    /// <item><description>0: [Filled] Drawn as 6 filled triangles, effectivly cutting of corners.</description></item>
    /// <item><description>1: [Sides] Each side is connected to the quad's center.</description></item>
    /// <item><description>2: [Sides Inverse] The start of 1 side is connected to the end of the next side and is connected to the quad's center.</description></item>
    /// </list>
    /// </param>
    public static void DrawScaled(this Rect r, ColorRgba color, float sideScaleFactor, float sideScaleOrigin, int drawType)
    {
        var q = r.ToQuad();
        q.DrawScaled(color, sideScaleFactor, sideScaleOrigin, drawType);
    }
    #endregion
    
    #region Draw Lines
    
    /// <summary>
    /// Draws the outline of a rectangle.
    /// </summary>
    /// <param name="rect">The rectangle to outline.</param>
    /// <param name="lineThickness">The outline thickness.</param>
    /// <param name="color">The outline color.</param>
    /// <remarks>
    /// The rectangle is converted to a <see cref="Quad"/> and drawn using quad line rendering.
    /// </remarks>
    public static void DrawLines(this Rect rect, float lineThickness, ColorRgba color)
    {
        var q = rect.ToQuad();
        q.DrawLines(lineThickness, color);
        // var thickness = MathF.Min(lineThickness, rect.Size.Min() * 0.5f);
        // rect = rect.ChangeSize(thickness * 2f, AnchorPoint.Center);
        // Raylib.DrawRectangleLinesEx(rect.Rectangle, thickness * 2f, color);
    }
    
    /// <summary>
    /// Draws the outline of a rectangle using a <see cref="LineDrawingInfo"/> configuration.
    /// </summary>
    /// <param name="rect">The rectangle to outline.</param>
    /// <param name="lineInfo">The line drawing settings to use.</param>
    /// <remarks>
    /// The rectangle is converted to a <see cref="Quad"/> and drawn using quad line rendering.
    /// </remarks>
    public static void DrawLines(this Rect rect, LineDrawingInfo lineInfo)
    {
        var q  = rect.ToQuad();
        q.DrawLines(lineInfo);
        // var thickness = MathF.Min(lineInfo.Thickness, rect.Size.Min() * 0.5f);
        // rect = rect.ChangeSize(thickness * 2f, AnchorPoint.Center);
        // Raylib.DrawRectangleLinesEx(rect.Rectangle, thickness, lineInfo.Color);
    }
    #endregion
    
    #region Draw Rounded Lines
    
    /// <summary>
    /// Draws a rounded rectangle outline.
    /// </summary>
    /// <param name="rect">The rectangle to outline.</param>
    /// <param name="lineThickness">The outline thickness.</param>
    /// <param name="color">The outline color.</param>
    /// <param name="roundness">The normalized corner roundness passed to Raylib.</param>
    /// <param name="segments">The number of segments used for each rounded corner.</param>
    /// <remarks>
    /// If <paramref name="roundness"/> or <paramref name="segments"/> is not positive, this falls back to <see cref="DrawLines(Rect,float,ColorRgba)"/>.
    /// </remarks>
    public static void DrawLinesRounded(this Rect rect, float lineThickness, ColorRgba color, float roundness, int segments)
    {
        if (roundness <= 0f || segments <= 0)
        {
            rect.DrawLines(lineThickness, color);
            return;
        }
        var thickness = MathF.Min(lineThickness, rect.Size.Min() * 0.5f);
        rect = rect.ChangeSize(-thickness * 1.99f, AnchorPoint.Center);
        Raylib.DrawRectangleRoundedLinesEx(rect.Rectangle, roundness, segments, thickness * 2f, color);
    }
    #endregion
    
    #region Draw Lines Scaled
  
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
        
        lineInfo = lineInfo.SetThickness(MathF.Min(lineInfo.Thickness, r.Size.Min() * 0.5f));
        
        SegmentDrawing.DrawSegment(r.TopLeft, r.BottomLeft, lineInfo, sideScaleFactor, sideScaleOrigin);
        SegmentDrawing.DrawSegment(r.BottomLeft, r.BottomRight, lineInfo, sideScaleFactor, sideScaleOrigin);
        SegmentDrawing.DrawSegment(r.BottomRight, r.TopRight, lineInfo, sideScaleFactor, sideScaleOrigin);
        SegmentDrawing.DrawSegment(r.TopRight, r.TopLeft, lineInfo, sideScaleFactor, sideScaleOrigin);
    }

    #endregion
    
    #region Draw Lines Percentage
    
    /// <summary>
    /// Draws part of the rectangle outline based on a perimeter percentage.
    /// </summary>
    /// <param name="rect">The rectangle whose outline is drawn.</param>
    /// <param name="f">The fraction of the perimeter to draw.</param>
    /// <param name="startIndex">The starting edge index used by the underlying quad draw order.</param>
    /// <param name="lineThickness">The outline thickness.</param>
    /// <param name="color">The outline color.</param>
    /// <remarks>
    /// This method converts the rectangle to a <see cref="Quad"/> and delegates to quad percentage line drawing.
    /// </remarks>
    public static void DrawLinesPercentage(this Rect rect, float f, int startIndex, float lineThickness, ColorRgba color)
    {
        var quad = new Quad(rect);
        quad.DrawLinesPercentage(f, startIndex, new LineDrawingInfo(lineThickness, color));
    }
    
    /// <summary>
    /// Draws part of the rectangle outline based on a perimeter percentage using line settings.
    /// </summary>
    /// <param name="rect">The rectangle whose outline is drawn.</param>
    /// <param name="f">The fraction of the perimeter to draw.</param>
    /// <param name="startIndex">The starting edge index used by the underlying quad draw order.</param>
    /// <param name="lineInfo">The line drawing settings to use.</param>
    /// <remarks>
    /// This method converts the rectangle to a <see cref="Quad"/> and delegates to quad percentage line drawing.
    /// </remarks>
    public static void DrawLinesPercentage(this Rect rect, float f, int startIndex, LineDrawingInfo lineInfo)
    {
        var quad = new Quad(rect);
        quad.DrawLinesPercentage(f, startIndex, lineInfo);
    }
    
    #endregion
    
    #region Draw Corners
    /// <summary>
    /// Draws the corners of the rectangle with independent lengths for each corner.
    /// </summary>
    /// <param name="rect">The rectangle to draw corners for.</param>
    /// <param name="lineThickness">The thickness of the corner lines.</param>
    /// <param name="color">The color of the corner lines.</param>
    /// <param name="tlCorner">The length of the top-left corner.</param>
    /// <param name="trCorner">The length of the top-right corner.</param>
    /// <param name="brCorner">The length of the bottom-right corner.</param>
    /// <param name="blCorner">The length of the bottom-left corner.</param>
    public static void DrawCorners(this Rect rect, float lineThickness, ColorRgba color, float tlCorner, float trCorner, float brCorner, float blCorner)
    {
        var quad = rect.ToQuad();
        quad.DrawCorners(lineThickness, color, tlCorner, trCorner, brCorner, blCorner);
    }
    
    /// <summary>
    /// Draws all corners of the rectangle with the same length.
    /// </summary>
    /// <param name="rect">The rectangle to draw corners for.</param>
    /// <param name="lineThickness">The thickness of the corner lines.</param>
    /// <param name="color">The color of the corner lines.</param>
    /// <param name="cornerLength">The length of the corner segments.</param>
    public static void DrawCorners(this Rect rect, float lineThickness, ColorRgba color, float cornerLength)
    {
        rect.DrawCorners(lineThickness, color, cornerLength, cornerLength, cornerLength, cornerLength);
    }
    
    #endregion
    
    #region Draw Corners Relative
    /// <summary>
    /// Draws the corners of the rectangle with independent lengths relative to the rectangle's minimum dimension.
    /// </summary>
    /// <param name="rect">The rectangle to draw corners for.</param>
    /// <param name="lineThickness">The thickness of the corner lines.</param>
    /// <param name="color">The color of the corner lines.</param>
    /// <param name="tlCornerFactor">Factor (0-1) for the top-left corner length relative to the rectangle's minimum size.</param>
    /// <param name="trCornerFactor">Factor (0-1) for the top-right corner length relative to the rectangle's minimum size.</param>
    /// <param name="brCornerFactor">Factor (0-1) for the bottom-right corner length relative to the rectangle's minimum size.</param>
    /// <param name="blCornerFactor">Factor (0-1) for the bottom-left corner length relative to the rectangle's minimum size.</param>
    public static void DrawCornersRelative(this Rect rect, float lineThickness, ColorRgba color, float tlCornerFactor, float trCornerFactor, float brCornerFactor, float blCornerFactor)
    {
        float minSize = MathF.Min(rect.Width, rect.Height);
        rect.DrawCorners(lineThickness, color, tlCornerFactor * minSize, trCornerFactor * minSize, brCornerFactor * minSize, blCornerFactor * minSize);
    }
    
    /// <summary>
    /// Draws all corners of the rectangle with the same length relative to the rectangle's minimum dimension.
    /// </summary>
    /// <param name="rect">The rectangle to draw corners for.</param>
    /// <param name="lineThickness">The thickness of the corner lines.</param>
    /// <param name="color">The color of the corner lines.</param>
    /// <param name="cornerLengthFactor">Factor (0-1) for the corner length relative to the rectangle's minimum size.</param>
    public static void DrawCornersRelative(this Rect rect, float lineThickness, ColorRgba color, float cornerLengthFactor)
    {
        rect.DrawCornersRelative(lineThickness, color, cornerLengthFactor, cornerLengthFactor, cornerLengthFactor, cornerLengthFactor);
    }

    #endregion
    
    #region Draw Chamfered Corners
    
    /// <summary>
    /// Draws a filled rectangle with equally chamfered corners.
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="color">The fill color.</param>
    /// <param name="cornerLength">The chamfer length applied to all corners.</param>
    /// <remarks>
    /// The rectangle is converted to a <see cref="Quad"/> and drawn using quad chamfer rendering.
    /// </remarks>
    public static void DrawChamferedCorners(this Rect rect, ColorRgba color, float cornerLength)
    {
        var q = rect.ToQuad();
        q.DrawChamferedCorners(color, cornerLength);
    }
    
    /// <summary>
    /// Draws a filled rectangle with chamfered corners using separate horizontal and vertical chamfer lengths.
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="color">The fill color.</param>
    /// <param name="cornerLengthHorizontal">The chamfer length measured along horizontal edges.</param>
    /// <param name="cornerLengthVertical">The chamfer length measured along vertical edges.</param>
    /// <remarks>
    /// The rectangle is converted to a <see cref="Quad"/> and drawn using quad chamfer rendering.
    /// </remarks>
    public static void DrawChamferedCorners(this Rect rect, ColorRgba color, float cornerLengthHorizontal, float cornerLengthVertical)
    {
        var q = rect.ToQuad();
        q.DrawChamferedCorners(color, cornerLengthHorizontal, cornerLengthVertical);
    }
    
    /// <summary>
    /// Draws a filled rectangle with independent chamfer lengths for each corner.
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="color">The fill color.</param>
    /// <param name="tlCorner">The chamfer length for the top-left corner.</param>
    /// <param name="blCorner">The chamfer length for the bottom-left corner.</param>
    /// <param name="brCorner">The chamfer length for the bottom-right corner.</param>
    /// <param name="trCorner">The chamfer length for the top-right corner.</param>
    /// <remarks>
    /// The rectangle is converted to a <see cref="Quad"/> and drawn using quad chamfer rendering.
    /// </remarks>
    public static void DrawChamferedCorners(this Rect rect, ColorRgba color, float tlCorner, float blCorner, float brCorner, float trCorner)
    {
        var q = rect.ToQuad();
        q.DrawChamferedCorners(color, tlCorner, blCorner, brCorner, trCorner);
    }
    #endregion
    
    #region Draw Chamfered Corners Relative
    /// <summary>
    /// Draws a filled rectangle with equally chamfered corners using a relative factor.
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="color">The fill color.</param>
    /// <param name="cornerLengthFactor">The normalized chamfer factor applied to all corners.</param>
    /// <remarks>
    /// The factor is interpreted by the underlying <see cref="Quad"/> implementation relative to the rectangle size.
    /// </remarks>
    public static void DrawChamferedCornersRelative(this Rect rect, ColorRgba color, float cornerLengthFactor)
    {
        var q = rect.ToQuad();
        q.DrawChamferedCornersRelative(color, cornerLengthFactor);
    }
    
    /// <summary>
    /// Draws a filled rectangle with chamfered corners using separate relative horizontal and vertical factors.
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="color">The fill color.</param>
    /// <param name="cornerLengthFactorHorizontal">The normalized chamfer factor relative to the rectangle width.</param>
    /// <param name="cornerLengthFactorVertical">The normalized chamfer factor relative to the rectangle height.</param>
    /// <remarks>
    /// The factors are interpreted by the underlying <see cref="Quad"/> implementation relative to the rectangle size.
    /// </remarks>
    public static void DrawChamferedCornersRelative(this Rect rect, ColorRgba color, float cornerLengthFactorHorizontal, float cornerLengthFactorVertical)
    {
        var q = rect.ToQuad();
        q.DrawChamferedCornersRelative(color, cornerLengthFactorHorizontal, cornerLengthFactorVertical);
    }
    
    /// <summary>
    /// Draws a filled rectangle with independent relative chamfer factors for each corner.
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="color">The fill color.</param>
    /// <param name="tlCornerFactor">The normalized chamfer factor for the top-left corner.</param>
    /// <param name="blCornerFactor">The normalized chamfer factor for the bottom-left corner.</param>
    /// <param name="brCornerFactor">The normalized chamfer factor for the bottom-right corner.</param>
    /// <param name="trCornerFactor">The normalized chamfer factor for the top-right corner.</param>
    /// <remarks>
    /// The factors are interpreted by the underlying <see cref="Quad"/> implementation relative to the rectangle size.
    /// </remarks>
    public static void DrawChamferedCornersRelative(this Rect rect, ColorRgba color,float tlCornerFactor, float blCornerFactor, float brCornerFactor, float trCornerFactor)
    {
        var q = rect.ToQuad();
        q.DrawChamferedCornersRelative(color, tlCornerFactor, blCornerFactor, brCornerFactor, trCornerFactor);
    }
    #endregion
    
    #region Draw Chamfered Corners Lines
    
    /// <summary>
    /// Draws a rectangle outline with equally chamfered corners.
    /// </summary>
    /// <param name="rect">The rectangle whose outline is drawn.</param>
    /// <param name="lineThickness">The outline thickness.</param>
    /// <param name="color">The outline color.</param>
    /// <param name="cornerLength">The chamfer length applied to all corners.</param>
    /// <remarks>
    /// The rectangle is converted to a <see cref="Quad"/> and drawn using quad chamfer outline rendering.
    /// </remarks>
    public static void DrawChamferedCornersLines(this Rect rect, float lineThickness, ColorRgba color, float cornerLength)
    {
        var q = rect.ToQuad();
        q.DrawChamferedCornersLines(lineThickness, color, cornerLength);
    }
    
    /// <summary>
    /// Draws a rectangle outline with chamfered corners using separate horizontal and vertical chamfer lengths.
    /// </summary>
    /// <param name="rect">The rectangle whose outline is drawn.</param>
    /// <param name="lineThickness">The outline thickness.</param>
    /// <param name="color">The outline color.</param>
    /// <param name="cornerLengthHorizontal">The chamfer length measured along horizontal edges.</param>
    /// <param name="cornerLengthVertical">The chamfer length measured along vertical edges.</param>
    /// <remarks>
    /// The rectangle is converted to a <see cref="Quad"/> and drawn using quad chamfer outline rendering.
    /// </remarks>
    public static void DrawChamferedCornersLines(this Rect rect, float lineThickness, ColorRgba color, float cornerLengthHorizontal, float cornerLengthVertical)
    {
        var q = rect.ToQuad();
        q.DrawChamferedCornersLines(lineThickness, color, cornerLengthHorizontal, cornerLengthVertical);
    }
    
    /// <summary>
    /// Draws a rectangle outline with independent chamfer lengths for each corner.
    /// </summary>
    /// <param name="rect">The rectangle whose outline is drawn.</param>
    /// <param name="lineThickness">The outline thickness.</param>
    /// <param name="color">The outline color.</param>
    /// <param name="tlCorner">The chamfer length for the top-left corner.</param>
    /// <param name="blCorner">The chamfer length for the bottom-left corner.</param>
    /// <param name="brCorner">The chamfer length for the bottom-right corner.</param>
    /// <param name="trCorner">The chamfer length for the top-right corner.</param>
    /// <remarks>
    /// The rectangle is converted to a <see cref="Quad"/> and drawn using quad chamfer outline rendering.
    /// </remarks>
    public static void DrawChamferedCornersLines(this Rect rect, float lineThickness, ColorRgba color, float tlCorner, float blCorner, float brCorner, float trCorner)
    {
        var q = rect.ToQuad();
        q.DrawChamferedCornersLines(lineThickness, color, tlCorner, blCorner, brCorner, trCorner);
    }
    
    #endregion
    
    #region Draw Chamfered Corners Relative Lines
    
    /// <summary>
    /// Draws a rectangle outline with equally chamfered corners using a relative factor.
    /// </summary>
    /// <param name="rect">The rectangle whose outline is drawn.</param>
    /// <param name="lineThickness">The outline thickness.</param>
    /// <param name="color">The outline color.</param>
    /// <param name="cornerLengthFactor">The normalized chamfer factor applied to all corners.</param>
    /// <remarks>
    /// The factor is interpreted by the underlying <see cref="Quad"/> implementation relative to the rectangle size.
    /// </remarks>
    public static void DrawChamferedCornersLinesRelative(this Rect rect, float lineThickness, ColorRgba color, float cornerLengthFactor)
    {
        var q = rect.ToQuad();
        q.DrawChamferedCornersLinesRelative(lineThickness, color, cornerLengthFactor);
    }
 
    /// <summary>
    /// Draws a rectangle outline with chamfered corners using separate relative horizontal and vertical factors.
    /// </summary>
    /// <param name="rect">The rectangle whose outline is drawn.</param>
    /// <param name="lineThickness">The outline thickness.</param>
    /// <param name="color">The outline color.</param>
    /// <param name="cornerLengthFactorHorizontal">The normalized chamfer factor relative to the rectangle width.</param>
    /// <param name="cornerLengthFactorVertical">The normalized chamfer factor relative to the rectangle height.</param>
    /// <remarks>
    /// The factors are interpreted by the underlying <see cref="Quad"/> implementation relative to the rectangle size.
    /// </remarks>
    public static void DrawChamferedCornersLinesRelative(this Rect rect, float lineThickness, ColorRgba color, float cornerLengthFactorHorizontal, float cornerLengthFactorVertical)
    {
        var q = rect.ToQuad();
        q.DrawChamferedCornersLinesRelative(lineThickness, color, cornerLengthFactorHorizontal, cornerLengthFactorVertical);
    }
    
    /// <summary>
    /// Draws a rectangle outline with independent relative chamfer factors for each corner.
    /// </summary>
    /// <param name="rect">The rectangle whose outline is drawn.</param>
    /// <param name="lineThickness">The outline thickness.</param>
    /// <param name="color">The outline color.</param>
    /// <param name="tlCornerFactor">The normalized chamfer factor for the top-left corner.</param>
    /// <param name="blCornerFactor">The normalized chamfer factor for the bottom-left corner.</param>
    /// <param name="brCornerFactor">The normalized chamfer factor for the bottom-right corner.</param>
    /// <param name="trCornerFactor">The normalized chamfer factor for the top-right corner.</param>
    /// <remarks>
    /// The factors are interpreted by the underlying <see cref="Quad"/> implementation relative to the rectangle size.
    /// </remarks>
    public static void DrawChamferedCornersLinesRelative(this Rect rect, float lineThickness, ColorRgba color, float tlCornerFactor, float blCornerFactor, float brCornerFactor, float trCornerFactor)
    {
       var q = rect.ToQuad();
       q.DrawChamferedCornersLinesRelative(lineThickness, color, tlCornerFactor, blCornerFactor, brCornerFactor, trCornerFactor);
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
        var w = r.Width;
        var h = r.Height;
        var xOffset = new Vector2(w / lines, 0f);
        var yOffset = new Vector2(0f, h / lines);

        var tl = r.TopLeft;
        var tr = tl + new Vector2(w, 0);
        var bl = tl + new Vector2(0, h);
        
        var thickness = lineInfo.Thickness;
        var maxThicknessH = MathF.Min((w / lines) * 0.5f, thickness);
        var maxThicknessV = MathF.Min((h / lines) * 0.5f, thickness);
        var lineInfoH = lineInfo.SetThickness(maxThicknessH);
        var lineInfoV = lineInfo.SetThickness(maxThicknessV);

        for (var i = 1; i < lines; i++)
        {
            SegmentDrawing.DrawSegment(tl + xOffset * i, bl + xOffset * i, lineInfoH);
            SegmentDrawing.DrawSegment(tl + yOffset * i, tr + yOffset * i, lineInfoV);
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
    
    #region Draw Vertices
    /// <summary>
    /// Draws circles at each vertex of the rectangle.
    /// </summary>
    /// <param name="rect">The rectangle whose vertices to draw.</param>
    /// <param name="vertexRadius">The radius of each vertex circle.</param>
    /// <param name="color">The color of the vertex circles.</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleDrawing.CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    public static void DrawVertices(this Rect rect, float vertexRadius, ColorRgba color, float smoothness = 0.5f)
    {
        var circle = new Circle(rect.TopLeft, vertexRadius);
        circle.Draw(color, smoothness);
        circle = circle.SetPosition(rect.TopRight);
        circle.Draw(color, smoothness);
        circle = circle.SetPosition(rect.BottomLeft);
        circle.Draw(color, smoothness);
        circle = circle.SetPosition(rect.BottomRight);
        circle.Draw(color, smoothness);
    }
    #endregion
    
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
    
    #region Draw Vignette

    /// <summary>
    /// Draws a "vignette" effect inside the rect, creating a circular hole in the center.
    /// The area between the inner circle and the rect's outer edges is filled with the specified color.
    /// </summary>
    /// <param name="r">The rect to draw the vignette within.</param>
    /// <param name="circleRadius">The radius of the inner circular hole.</param>
    /// <param name="circleRotDeg">The starting rotation angle of the inner circle in degrees.</param>
    /// <param name="color">The color of the filled area.</param>
    /// <param name="circleSmoothness">
    /// Determines the smoothness of the inner circle (0.0 to 1.0). 
    /// Higher values result in more segments and a smoother circle.
    /// </param>
    public static void DrawVignette(this Rect r, float circleRadius, float circleRotDeg, ColorRgba color, float circleSmoothness = 0.5f)
    {
        var q = r.ToQuad();
        q.DrawVignette(circleRadius, circleRotDeg, color, circleSmoothness);
    }
    #endregion
    
    #region Gapped
    /// <summary>
    /// Draws a gapped outline for a rectangle, creating a dashed or segmented effect along the rectangle's perimeter.
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="perimeter">
    /// The total length of the rectangle's perimeter.
    /// If zero or negative, the method calculates it automatically.
    /// Providing a known length avoids redundant calculations and improves performance, especially for static segments.
    /// </param>
    /// <param name="lineInfo">Parameters describing how to draw the outline.</param>
    /// <param name="gapDrawingInfo">Parameters describing the gap configuration.</param>
    /// <returns>
    /// The perimeter of the rectangle if positive; otherwise, -1.
    /// If the shape does not change, the valid length can be reused in subsequent frames to avoid recalculating.
    /// </returns>
    /// <remarks>
    /// - If <paramref name="gapDrawingInfo.Gaps"/> is 0 or <paramref name="gapDrawingInfo.GapPerimeterPercentage"/> is 0, the outline is drawn solid.
    /// - If <paramref name="gapDrawingInfo.GapPerimeterPercentage"/> is 1 or greater, no outline is drawn.
    /// </remarks>
    public static float DrawGappedOutline(this Rect rect, float perimeter, LineDrawingInfo lineInfo, GappedOutlineDrawingInfo gapDrawingInfo)
    {
        if (gapDrawingInfo.Gaps <= 0 || gapDrawingInfo.GapPerimeterPercentage <= 0f)
        {
            rect.DrawLines(lineInfo);
            return perimeter > 0f ? perimeter : -1f;
        }

        if (gapDrawingInfo.GapPerimeterPercentage >= 1f) return perimeter > 0f ? perimeter : -1f;

        var nonGapPercentage = 1f - gapDrawingInfo.GapPerimeterPercentage;

        var gapPercentageRange = gapDrawingInfo.GapPerimeterPercentage / gapDrawingInfo.Gaps;
        var nonGapPercentageRange = nonGapPercentage / gapDrawingInfo.Gaps;

        
        var shapePoints = new[] {rect.A, rect.B, rect.C, rect.D};
        int sides = shapePoints.Length;

        if (perimeter <= 0f)
        {
            perimeter = 0f;
            for (int i = 0; i < sides; i++)
            {
                var curP = shapePoints[i];
                var nextP = shapePoints[(i + 1) % sides];
                perimeter += (nextP - curP).Length();
            }
        }

        var startDistance = perimeter * gapDrawingInfo.StartOffset;
        var curDistance = 0f;
        var nextDistance = startDistance;
        
        var curIndex = 0;
        var curPoint = shapePoints[0];
        var nextPoint= shapePoints[1];
        var curW = nextPoint - curPoint;
        var curDis = curW.Length();
        
        var points = new List<Vector2>(3);

        int whileCounter = gapDrawingInfo.Gaps;
        
        while (whileCounter > 0)
        {
            if (curDistance + curDis >= nextDistance)
            {
                var p = curPoint + (curW / curDis) * (nextDistance - curDistance);
                
                
                if (points.Count == 0)
                {
                    nextDistance += nonGapPercentageRange * perimeter;
                    points.Add(p);

                }
                else
                {
                    nextDistance += gapPercentageRange * perimeter;
                    points.Add(p);
                    if (points.Count == 2)
                    {
                        SegmentDrawing.DrawSegment(points[0], points[1], lineInfo);
                    }
                    else
                    {
                        for (var i = 0; i < points.Count - 1; i++)
                        {
                            var p1 = points[i];
                            var p2 = points[(i + 1) % points.Count];
                            SegmentDrawing.DrawSegment(p1, p2, lineInfo);
                        }
                    }
                    
                    points.Clear();
                    whileCounter--;
                }

            }
            else
            {
                
                if(points.Count > 0) points.Add(nextPoint);
                
                curDistance += curDis;
                curIndex = (curIndex + 1) % sides;
                curPoint = shapePoints[curIndex];
                nextPoint = shapePoints[(curIndex + 1) % sides];
                curW = nextPoint - curPoint;
                curDis = curW.Length();
            }
            
        }

        return perimeter;
    }
   
    #endregion
    
    //TODO: Check if it works and if implemantion can be simplified
    // - Should quad have that?
    #region UI
    /// <summary>
    /// Draws an outline bar along the border of a rectangle, filling the outline based on the specified progress value.
    /// </summary>
    /// <param name="rect">The rectangle to draw the outline on.</param>
    /// <param name="thickness">The thickness of the outline.</param>
    /// <param name="f">The progress value (0 to 1) indicating how much of the outline to draw.</param>
    /// <param name="color">The color of the outline.</param>
    /// <remarks>
    /// The outline is drawn in four segments (top, right, bottom, left), and the progress value determines how many segments are filled.
    /// </remarks>
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
            SegmentDrawing.DrawSegment(start, end, thickness, color);
            // DrawLineEx(start, end, thickness, color.ToRayColor());
        }
    }

    /// <summary>
    /// Draws an outline bar along the border of a rotated rectangle, filling the outline based on the specified progress value.
    /// </summary>
    /// <param name="rect">The rectangle to draw the outline on.</param>
    /// <param name="pivot">The pivot point for rotation.</param>
    /// <param name="angleDeg">The rotation angle in degrees.</param>
    /// <param name="thickness">The thickness of the outline.</param>
    /// <param name="f">The progress value (0 to 1) indicating how much of the outline to draw.</param>
    /// <param name="color">The color of the outline.</param>
    /// <remarks>
    /// The outline is drawn in four segments (top, right, bottom, left), and the progress value determines how many segments are filled.
    /// The rectangle is rotated around the specified pivot point.
    /// </remarks>
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
            SegmentDrawing.DrawSegment(start, end, thickness, color);
            // Raylib.DrawLineEx(start, end, thickness, color.ToRayColor());
        }
    }

    /// <summary>
    /// Draws an outline bar along the circumference of a circle, filling the outline based on the specified progress value.
    /// </summary>
    /// <param name="c">The circle to draw the outline on.</param>
    /// <param name="thickness">The thickness of the outline.</param>
    /// <param name="f">The progress value (0 to 1) indicating how much of the outline to draw (as a fraction of the circle).</param>
    /// <param name="color">The color of the outline.</param>
    /// <remarks>
    /// The outline is drawn as a sector of the circle, starting from 0 degrees.
    /// </remarks>
    public static void DrawOutlineBar(this Circle c, float thickness, float f, ColorRgba color, float smoothness = 0.5f)
    {
        c.DrawSectorLines(0, 360 * f, 0f, thickness, color, smoothness);
    }

    /// <summary>
    /// Draws an outline bar along the circumference of a circle, starting at a specified angle offset, and filling based on the progress value.
    /// </summary>
    /// <param name="c">The circle to draw the outline on.</param>
    /// <param name="startOffsetDeg">The starting angle offset in degrees.</param>
    /// <param name="thickness">The thickness of the outline.</param>
    /// <param name="f">The progress value (0 to 1) indicating how much of the outline to draw (as a fraction of the circle).</param>
    /// <param name="color">The color of the outline.</param>
    /// <remarks>
    /// The outline is drawn as a sector of the circle, starting from the specified angle offset.
    /// </remarks>
    public static void DrawOutlineBar(this Circle c, float startOffsetDeg, float thickness, float f, ColorRgba color, float smoothness = 0.5f)
    {
        c.DrawSectorLines(0, 360 * f, startOffsetDeg, thickness, color, smoothness);
    }

    /// <summary>
    /// Draws a filled bar inside a rectangle, representing progress with customizable margins and colors.
    /// </summary>
    /// <param name="rect">The rectangle to draw the bar in.</param>
    /// <param name="f">The progress value (0 to 1) indicating how much of the bar to fill.</param>
    /// <param name="barColorRgba">The color of the filled bar.</param>
    /// <param name="bgColorRgba">The background color of the rectangle.</param>
    /// <param name="left">The left margin <c>left * (1 - f)</c> to determine the fill behavior (default 0).</param>
    /// <param name="right">The right margin <c>right * (1 - f)</c> to determine the fill behavior (default 1).</param>
    /// <param name="top">The top margin <c>top * (1 - f)</c> to determine the fill behavior  (default 0).</param>
    /// <param name="bottom">The bottom margin <c>bottom * (1 - f)</c> to determine the fill behavior (default 0).</param>
    /// <remarks>
    /// The bar is drawn inside the rectangle, with the filled area shrinking as the progress value increases.
    /// The default margin values represent a bar that fills from the left to the right.
    /// </remarks>
    /// <example>
    /// <list type="bullet">
    /// <item><description>left 1, right 0, top 0, bottom 0 -> bar fills from right to left. </description></item>
    /// <item><description>left 0, right 0, top 1, bottom 0 -> bar fills from bottom to top. </description></item>
    /// <item><description>left 0, right 0, top 0, bottom 1 -> bar fills from top to bottom. </description></item>
    /// <item><description>left 0.5, right 0.5, top 0, bottom 0 -> bar fills from center to left and right edges. </description></item>
    /// </list>
    /// </example>
    public static void DrawBar(this Rect rect, float f, ColorRgba barColorRgba, ColorRgba bgColorRgba, float left = 0f, float right = 1f, float top = 0f, float bottom = 0f)
    {
        f = 1.0f - f;
        Rect.Margins progressMargins = new(f * top, f * right, f * bottom, f * left);
        var progressRect = rect.ApplyMargins(progressMargins); // progressMargins.Apply(rect);
        rect.Draw(bgColorRgba);
        progressRect.Draw(barColorRgba);
    }

    /// <summary>
    /// Draws a filled bar inside a rotated rectangle, representing progress with customizable margins and colors.
    /// </summary>
    /// <param name="rect">The rectangle to draw the bar in.</param>
    /// <param name="pivot">The pivot point for rotation.</param>
    /// <param name="angleDeg">The rotation angle in degrees.</param>
    /// <param name="f">The progress value (0 to 1) indicating how much of the bar to fill.</param>
    /// <param name="barColorRgba">The color of the filled bar.</param>
    /// <param name="bgColorRgba">The background color of the rectangle.</param>
    /// <param name="left">The left margin <c>left * (1 - f)</c> to determine the fill behavior (default 0).</param>
    /// <param name="right">The right margin <c>right * (1 - f)</c> to determine the fill behavior (default 1).</param>
    /// <param name="top">The top margin <c>top * (1 - f)</c> to determine the fill behavior  (default 0).</param>
    /// <param name="bottom">The bottom margin <c>bottom * (1 - f)</c> to determine the fill behavior (default 0).</param>
    /// <remarks>
    /// The bar is drawn inside the rectangle, with the filled area shrinking as the progress value increases.
    /// The default margin values represent a bar that fills from the left to the right.
    /// The rectangle is rotated around the specified pivot point.
    /// </remarks>
    /// <example>
    /// <list type="bullet">
    /// <item><description>left 1, right 0, top 0, bottom 0 -> bar fills from right to left. </description></item>
    /// <item><description>left 0, right 0, top 1, bottom 0 -> bar fills from bottom to top. </description></item>
    /// <item><description>left 0, right 0, top 0, bottom 1 -> bar fills from top to bottom. </description></item>
    /// <item><description>left 0.5, right 0.5, top 0, bottom 0 -> bar fills from center to left and right edges. </description></item>
    /// </list>
    /// </example>
    public static void DrawBar(this Rect rect, Vector2 pivot, float angleDeg, float f, ColorRgba barColorRgba, ColorRgba bgColorRgba, float left = 0f, float right = 1f, float top = 0f, float bottom = 0f)
    {
        f = 1.0f - f;
        Rect.Margins progressMargins = new(f * top, f * right, f * bottom, f * left);
        var progressRect = rect.ApplyMargins(progressMargins); // progressMargins.Apply(rect);
        var quad = new Quad(rect, angleDeg, pivot);
        quad.Draw(bgColorRgba);
        var progressQuad = new Quad(progressRect, angleDeg, pivot);
        progressQuad.Draw( barColorRgba);
    }
    #endregion
}
    
   