
using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.RectDef;


/// <summary>
/// Provides drawing helpers and instance drawing methods for <see cref="Rect"/> values.
/// </summary>
public readonly partial struct Rect
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
    /// <param name="color">The fill color.</param>
    public void Draw(ColorRgba color)
    {
        Raylib.DrawRectangleV(TopLeft, BottomRight - TopLeft, color.ToRayColor());
    }
    #endregion

    #region Draw Rounded
    /// <summary>
    /// Draws a filled rectangle with rounded corners.
    /// </summary>
    /// <param name="color">The fill color.</param>
    /// <param name="roundness">The normalized corner roundness passed to Raylib.</param>
    /// <param name="segments">The number of segments used for each rounded corner.</param>
    /// <remarks>
    /// This method forwards to Raylib's rounded rectangle drawing API.
    /// </remarks>
    public void DrawRounded(ColorRgba color, float roundness, int segments)
    {
        Raylib.DrawRectangleRounded(Rectangle, roundness, segments, color);
    }

    #endregion
    
    #region Draw Scaled
    /// <summary>
    /// Draws a rect with scaled sides based on a specific draw type.
    /// </summary>
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
    public void DrawScaled(ColorRgba color, float sideScaleFactor, float sideScaleOrigin, int drawType)
    {
        var q = ToQuad();
        q.DrawScaled(color, sideScaleFactor, sideScaleOrigin, drawType);
    }
    #endregion
    
    #region Draw Lines
    
    /// <summary>
    /// Draws the outline of a rectangle.
    /// </summary>
    /// <param name="lineThickness">The outline thickness.</param>
    /// <param name="color">The outline color.</param>
    /// <remarks>
    /// The rectangle is converted to a <see cref="Quad"/> and drawn using quad line rendering.
    /// </remarks>
    public void DrawLines(float lineThickness, ColorRgba color)
    {
        var q = ToQuad();
        q.DrawLines(lineThickness, color);
        // var thickness = MathF.Min(lineThickness, rect.Size.Min() * 0.5f);
        // rect = rect.ChangeSize(thickness * 2f, AnchorPoint.Center);
        // Raylib.DrawRectangleLinesEx(rect.Rectangle, thickness * 2f, color);
    }
    
    /// <summary>
    /// Draws the outline of a rectangle using a <see cref="LineDrawingInfo"/> configuration.
    /// </summary>
    /// <param name="lineInfo">The line drawing settings to use.</param>
    /// <remarks>
    /// The rectangle is converted to a <see cref="Quad"/> and drawn using quad line rendering.
    /// </remarks>
    public void DrawLines(LineDrawingInfo lineInfo)
    {
        var q  = ToQuad();
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
    /// <param name="lineThickness">The outline thickness.</param>
    /// <param name="color">The outline color.</param>
    /// <param name="roundness">The normalized corner roundness passed to Raylib.</param>
    /// <param name="segments">The number of segments used for each rounded corner.</param>
    /// <remarks>
    /// If <paramref name="roundness"/> or <paramref name="segments"/> is not positive, this falls back to <see cref="DrawLines(float, ColorRgba)"/>.
    /// </remarks>
    public void DrawLinesRounded(float lineThickness, ColorRgba color, float roundness, int segments)
    {
        if (roundness <= 0f || segments <= 0)
        {
            DrawLines(lineThickness, color);
            return;
        }
        var thickness = MathF.Min(lineThickness, Size.Min() * 0.5f);
        var rect = ChangeSize(-thickness * 1.99f, AnchorPoint.Center);
        Raylib.DrawRectangleRoundedLinesEx(rect.Rectangle, roundness, segments, thickness * 2f, color);
    }
    #endregion
    
    #region Draw Lines Scaled
  
    /// <summary>
    /// Draws a rectangle outline where each side can be scaled towards the origin of the side.
    /// </summary>
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
    public void DrawLinesScaled(LineDrawingInfo lineInfo,  float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0f) return;
        if (sideScaleFactor >= 1f)
        {
            DrawLines(lineInfo);
            return;
        }
        
        lineInfo = lineInfo.SetThickness(MathF.Min(lineInfo.Thickness, Size.Min() * 0.5f));
        
        Segment.DrawSegment(TopLeft, BottomLeft, lineInfo, sideScaleFactor, sideScaleOrigin);
        Segment.DrawSegment(BottomLeft, BottomRight, lineInfo, sideScaleFactor, sideScaleOrigin);
        Segment.DrawSegment(BottomRight, TopRight, lineInfo, sideScaleFactor, sideScaleOrigin);
        Segment.DrawSegment(TopRight, TopLeft, lineInfo, sideScaleFactor, sideScaleOrigin);
    }

    #endregion
    
    #region Draw Lines Percentage
    
    /// <summary>
    /// Draws part of the rectangle outline based on a perimeter percentage.
    /// </summary>
    /// <param name="f">The fraction of the perimeter to draw.</param>
    /// <param name="startIndex">The starting edge index used by the underlying quad draw order.</param>
    /// <param name="lineThickness">The outline thickness.</param>
    /// <param name="color">The outline color.</param>
    /// <remarks>
    /// This method converts the rectangle to a <see cref="Quad"/> and delegates to quad percentage line drawing.
    /// </remarks>
    public void DrawLinesPercentage(float f, int startIndex, float lineThickness, ColorRgba color)
    {
        var quad = new Quad(this);
        quad.DrawLinesPercentage(f, startIndex, new LineDrawingInfo(lineThickness, color));
    }
    
    /// <summary>
    /// Draws part of the rectangle outline based on a perimeter percentage using line settings.
    /// </summary>
    /// <param name="f">The fraction of the perimeter to draw.</param>
    /// <param name="startIndex">The starting edge index used by the underlying quad draw order.</param>
    /// <param name="lineInfo">The line drawing settings to use.</param>
    /// <remarks>
    /// This method converts the rectangle to a <see cref="Quad"/> and delegates to quad percentage line drawing.
    /// </remarks>
    public void DrawLinesPercentage(float f, int startIndex, LineDrawingInfo lineInfo)
    {
        var quad = new Quad(this);
        quad.DrawLinesPercentage(f, startIndex, lineInfo);
    }
    
    #endregion
    
    #region Draw Corners
    /// <summary>
    /// Draws the corners of the rectangle with independent lengths for each corner.
    /// </summary>
    /// <param name="lineThickness">The thickness of the corner lines.</param>
    /// <param name="color">The color of the corner lines.</param>
    /// <param name="tlCorner">The length of the top-left corner.</param>
    /// <param name="trCorner">The length of the top-right corner.</param>
    /// <param name="brCorner">The length of the bottom-right corner.</param>
    /// <param name="blCorner">The length of the bottom-left corner.</param>
    public void DrawCorners(float lineThickness, ColorRgba color, float tlCorner, float trCorner, float brCorner, float blCorner)
    {
        var quad = ToQuad();
        quad.DrawCorners(lineThickness, color, tlCorner, trCorner, brCorner, blCorner);
    }
    
    /// <summary>
    /// Draws all corners of the rectangle with the same length.
    /// </summary>
    /// <param name="lineThickness">The thickness of the corner lines.</param>
    /// <param name="color">The color of the corner lines.</param>
    /// <param name="cornerLength">The length of the corner segments.</param>
    public void DrawCorners(float lineThickness, ColorRgba color, float cornerLength)
    {
        DrawCorners(lineThickness, color, cornerLength, cornerLength, cornerLength, cornerLength);
    }
    
    #endregion
    
    #region Draw Corners Relative
    /// <summary>
    /// Draws the corners of the rectangle with independent lengths relative to the rectangle's minimum dimension.
    /// </summary>
    /// <param name="lineThickness">The thickness of the corner lines.</param>
    /// <param name="color">The color of the corner lines.</param>
    /// <param name="tlCornerFactor">Factor (0-1) for the top-left corner length relative to the rectangle's minimum size.</param>
    /// <param name="trCornerFactor">Factor (0-1) for the top-right corner length relative to the rectangle's minimum size.</param>
    /// <param name="brCornerFactor">Factor (0-1) for the bottom-right corner length relative to the rectangle's minimum size.</param>
    /// <param name="blCornerFactor">Factor (0-1) for the bottom-left corner length relative to the rectangle's minimum size.</param>
    public void DrawCornersRelative(float lineThickness, ColorRgba color, float tlCornerFactor, float trCornerFactor, float brCornerFactor, float blCornerFactor)
    {
        float minSize = MathF.Min(Width, Height);
        DrawCorners(lineThickness, color, tlCornerFactor * minSize, trCornerFactor * minSize, brCornerFactor * minSize, blCornerFactor * minSize);
    }
    
    /// <summary>
    /// Draws all corners of the rectangle with the same length relative to the rectangle's minimum dimension.
    /// </summary>
    /// <param name="lineThickness">The thickness of the corner lines.</param>
    /// <param name="color">The color of the corner lines.</param>
    /// <param name="cornerLengthFactor">Factor (0-1) for the corner length relative to the rectangle's minimum size.</param>
    public void DrawCornersRelative(float lineThickness, ColorRgba color, float cornerLengthFactor)
    {
        DrawCornersRelative(lineThickness, color, cornerLengthFactor, cornerLengthFactor, cornerLengthFactor, cornerLengthFactor);
    }

    #endregion
    
    #region Draw Chamfered Corners
    
    /// <summary>
    /// Draws a filled rectangle with equally chamfered corners.
    /// </summary>
    /// <param name="color">The fill color.</param>
    /// <param name="cornerLength">The chamfer length applied to all corners.</param>
    /// <remarks>
    /// The rectangle is converted to a <see cref="Quad"/> and drawn using quad chamfer rendering.
    /// </remarks>
    public void DrawChamferedCorners(ColorRgba color, float cornerLength)
    {
        var q = ToQuad();
        q.DrawChamferedCorners(color, cornerLength);
    }
    
    /// <summary>
    /// Draws a filled rectangle with chamfered corners using separate horizontal and vertical chamfer lengths.
    /// </summary>
    /// <param name="color">The fill color.</param>
    /// <param name="cornerLengthHorizontal">The chamfer length measured along horizontal edges.</param>
    /// <param name="cornerLengthVertical">The chamfer length measured along vertical edges.</param>
    /// <remarks>
    /// The rectangle is converted to a <see cref="Quad"/> and drawn using quad chamfer rendering.
    /// </remarks>
    public void DrawChamferedCorners(ColorRgba color, float cornerLengthHorizontal, float cornerLengthVertical)
    {
        var q = ToQuad();
        q.DrawChamferedCorners(color, cornerLengthHorizontal, cornerLengthVertical);
    }
    
    /// <summary>
    /// Draws a filled rectangle with independent chamfer lengths for each corner.
    /// </summary>
    /// <param name="color">The fill color.</param>
    /// <param name="tlCorner">The chamfer length for the top-left corner.</param>
    /// <param name="blCorner">The chamfer length for the bottom-left corner.</param>
    /// <param name="brCorner">The chamfer length for the bottom-right corner.</param>
    /// <param name="trCorner">The chamfer length for the top-right corner.</param>
    /// <remarks>
    /// The rectangle is converted to a <see cref="Quad"/> and drawn using quad chamfer rendering.
    /// </remarks>
    public void DrawChamferedCorners(ColorRgba color, float tlCorner, float blCorner, float brCorner, float trCorner)
    {
        var q = ToQuad();
        q.DrawChamferedCorners(color, tlCorner, blCorner, brCorner, trCorner);
    }
    #endregion
    
    #region Draw Chamfered Corners Relative
    /// <summary>
    /// Draws a filled rectangle with equally chamfered corners using a relative factor.
    /// </summary>
    /// <param name="color">The fill color.</param>
    /// <param name="cornerLengthFactor">The normalized chamfer factor applied to all corners.</param>
    /// <remarks>
    /// The factor is interpreted by the underlying <see cref="Quad"/> implementation relative to the rectangle size.
    /// </remarks>
    public void DrawChamferedCornersRelative(ColorRgba color, float cornerLengthFactor)
    {
        var q = ToQuad();
        q.DrawChamferedCornersRelative(color, cornerLengthFactor);
    }
    
    /// <summary>
    /// Draws a filled rectangle with chamfered corners using separate relative horizontal and vertical factors.
    /// </summary>
    /// <param name="color">The fill color.</param>
    /// <param name="cornerLengthFactorHorizontal">The normalized chamfer factor relative to the rectangle width.</param>
    /// <param name="cornerLengthFactorVertical">The normalized chamfer factor relative to the rectangle height.</param>
    /// <remarks>
    /// The factors are interpreted by the underlying <see cref="Quad"/> implementation relative to the rectangle size.
    /// </remarks>
    public void DrawChamferedCornersRelative(ColorRgba color, float cornerLengthFactorHorizontal, float cornerLengthFactorVertical)
    {
        var q = ToQuad();
        q.DrawChamferedCornersRelative(color, cornerLengthFactorHorizontal, cornerLengthFactorVertical);
    }
    
    /// <summary>
    /// Draws a filled rectangle with independent relative chamfer factors for each corner.
    /// </summary>
    /// <param name="color">The fill color.</param>
    /// <param name="tlCornerFactor">The normalized chamfer factor for the top-left corner.</param>
    /// <param name="blCornerFactor">The normalized chamfer factor for the bottom-left corner.</param>
    /// <param name="brCornerFactor">The normalized chamfer factor for the bottom-right corner.</param>
    /// <param name="trCornerFactor">The normalized chamfer factor for the top-right corner.</param>
    /// <remarks>
    /// The factors are interpreted by the underlying <see cref="Quad"/> implementation relative to the rectangle size.
    /// </remarks>
    public void DrawChamferedCornersRelative(ColorRgba color,float tlCornerFactor, float blCornerFactor, float brCornerFactor, float trCornerFactor)
    {
        var q = ToQuad();
        q.DrawChamferedCornersRelative(color, tlCornerFactor, blCornerFactor, brCornerFactor, trCornerFactor);
    }
    #endregion
    
    #region Draw Chamfered Corners Lines
    
    /// <summary>
    /// Draws a rectangle outline with equally chamfered corners.
    /// </summary>
    /// <param name="lineThickness">The outline thickness.</param>
    /// <param name="color">The outline color.</param>
    /// <param name="cornerLength">The chamfer length applied to all corners.</param>
    /// <remarks>
    /// The rectangle is converted to a <see cref="Quad"/> and drawn using quad chamfer outline rendering.
    /// </remarks>
    public void DrawChamferedCornersLines(float lineThickness, ColorRgba color, float cornerLength)
    {
        var q = ToQuad();
        q.DrawChamferedCornersLines(lineThickness, color, cornerLength);
    }
    
    /// <summary>
    /// Draws a rectangle outline with chamfered corners using separate horizontal and vertical chamfer lengths.
    /// </summary>
    /// <param name="lineThickness">The outline thickness.</param>
    /// <param name="color">The outline color.</param>
    /// <param name="cornerLengthHorizontal">The chamfer length measured along horizontal edges.</param>
    /// <param name="cornerLengthVertical">The chamfer length measured along vertical edges.</param>
    /// <remarks>
    /// The rectangle is converted to a <see cref="Quad"/> and drawn using quad chamfer outline rendering.
    /// </remarks>
    public void DrawChamferedCornersLines(float lineThickness, ColorRgba color, float cornerLengthHorizontal, float cornerLengthVertical)
    {
        var q = ToQuad();
        q.DrawChamferedCornersLines(lineThickness, color, cornerLengthHorizontal, cornerLengthVertical);
    }
    
    /// <summary>
    /// Draws a rectangle outline with independent chamfer lengths for each corner.
    /// </summary>
    /// <param name="lineThickness">The outline thickness.</param>
    /// <param name="color">The outline color.</param>
    /// <param name="tlCorner">The chamfer length for the top-left corner.</param>
    /// <param name="blCorner">The chamfer length for the bottom-left corner.</param>
    /// <param name="brCorner">The chamfer length for the bottom-right corner.</param>
    /// <param name="trCorner">The chamfer length for the top-right corner.</param>
    /// <remarks>
    /// The rectangle is converted to a <see cref="Quad"/> and drawn using quad chamfer outline rendering.
    /// </remarks>
    public void DrawChamferedCornersLines(float lineThickness, ColorRgba color, float tlCorner, float blCorner, float brCorner, float trCorner)
    {
        var q = ToQuad();
        q.DrawChamferedCornersLines(lineThickness, color, tlCorner, blCorner, brCorner, trCorner);
    }
    
    #endregion
    
    #region Draw Chamfered Corners Relative Lines
    
    /// <summary>
    /// Draws a rectangle outline with equally chamfered corners using a relative factor.
    /// </summary>
    /// <param name="lineThickness">The outline thickness.</param>
    /// <param name="color">The outline color.</param>
    /// <param name="cornerLengthFactor">The normalized chamfer factor applied to all corners.</param>
    /// <remarks>
    /// The factor is interpreted by the underlying <see cref="Quad"/> implementation relative to the rectangle size.
    /// </remarks>
    public void DrawChamferedCornersLinesRelative(float lineThickness, ColorRgba color, float cornerLengthFactor)
    {
        var q = ToQuad();
        q.DrawChamferedCornersLinesRelative(lineThickness, color, cornerLengthFactor);
    }
 
    /// <summary>
    /// Draws a rectangle outline with chamfered corners using separate relative horizontal and vertical factors.
    /// </summary>
    /// <param name="lineThickness">The outline thickness.</param>
    /// <param name="color">The outline color.</param>
    /// <param name="cornerLengthFactorHorizontal">The normalized chamfer factor relative to the rectangle width.</param>
    /// <param name="cornerLengthFactorVertical">The normalized chamfer factor relative to the rectangle height.</param>
    /// <remarks>
    /// The factors are interpreted by the underlying <see cref="Quad"/> implementation relative to the rectangle size.
    /// </remarks>
    public void DrawChamferedCornersLinesRelative(float lineThickness, ColorRgba color, float cornerLengthFactorHorizontal, float cornerLengthFactorVertical)
    {
        var q = ToQuad();
        q.DrawChamferedCornersLinesRelative(lineThickness, color, cornerLengthFactorHorizontal, cornerLengthFactorVertical);
    }
    
    /// <summary>
    /// Draws a rectangle outline with independent relative chamfer factors for each corner.
    /// </summary>
    /// <param name="lineThickness">The outline thickness.</param>
    /// <param name="color">The outline color.</param>
    /// <param name="tlCornerFactor">The normalized chamfer factor for the top-left corner.</param>
    /// <param name="blCornerFactor">The normalized chamfer factor for the bottom-left corner.</param>
    /// <param name="brCornerFactor">The normalized chamfer factor for the bottom-right corner.</param>
    /// <param name="trCornerFactor">The normalized chamfer factor for the top-right corner.</param>
    /// <remarks>
    /// The factors are interpreted by the underlying <see cref="Quad"/> implementation relative to the rectangle size.
    /// </remarks>
    public void DrawChamferedCornersLinesRelative(float lineThickness, ColorRgba color, float tlCornerFactor, float blCornerFactor, float brCornerFactor, float trCornerFactor)
    {
       var q = ToQuad();
       q.DrawChamferedCornersLinesRelative(lineThickness, color, tlCornerFactor, blCornerFactor, brCornerFactor, trCornerFactor);
    }
    #endregion
    
    /// <summary>
    /// Draws a grid inside a rectangle, with the specified number of lines and line drawing information.
    /// </summary>
    /// <param name="lines">The number of grid lines (both horizontal and vertical).</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, etc.).</param>
    public  void DrawGrid(int lines, LineDrawingInfo lineInfo)
    {
        var w = Width;
        var h = Height;
        var xOffset = new Vector2(w / lines, 0f);
        var yOffset = new Vector2(0f, h / lines);

        var tl = TopLeft;
        var tr = tl + new Vector2(w, 0);
        var bl = tl + new Vector2(0, h);
        
        var thickness = lineInfo.Thickness;
        var maxThicknessH = MathF.Min((w / lines) * 0.5f, thickness);
        var maxThicknessV = MathF.Min((h / lines) * 0.5f, thickness);
        var lineInfoH = lineInfo.SetThickness(maxThicknessH);
        var lineInfoV = lineInfo.SetThickness(maxThicknessV);

        for (var i = 1; i < lines; i++)
        {
            Segment.DrawSegment(tl + xOffset * i, bl + xOffset * i, lineInfoH);
            Segment.DrawSegment(tl + yOffset * i, tr + yOffset * i, lineInfoV);
        }
    }
    
    #region Draw Vertices
    /// <summary>
    /// Draws circles at each vertex of the rectangle.
    /// </summary>
    /// <param name="vertexRadius">The radius of each vertex circle.</param>
    /// <param name="color">The color of the vertex circles.</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="Circle.CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    public void DrawVertices(float vertexRadius, ColorRgba color, float smoothness = 0.5f)
    {
        var circle = new Circle(TopLeft, vertexRadius);
        circle.Draw(color, smoothness);
        circle = circle.SetPosition(TopRight);
        circle.Draw(color, smoothness);
        circle = circle.SetPosition(BottomLeft);
        circle.Draw(color, smoothness);
        circle = circle.SetPosition(BottomRight);
        circle.Draw(color, smoothness);
    }
    #endregion
    
    #region Draw Masked
    
    /// <summary>
    /// Draws the rectangle's four side segments, but only where they intersect the given triangular mask.
    /// Each side is drawn by forwarding the call to the segment-level masked draw method.
    /// </summary>
    /// <param name="mask">The triangular mask used to clip each side.</param>
    /// <param name="lineInfo">Line drawing parameters (thickness, color, cap style, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public void DrawLinesMasked(Triangle mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        TopSegment.DrawMasked(mask, lineInfo, reversedMask);
        LeftSegment.DrawMasked(mask, lineInfo, reversedMask);
        BottomSegment.DrawMasked(mask, lineInfo, reversedMask);
        RightSegment.DrawMasked(mask, lineInfo, reversedMask);
    }
    /// <summary>
    /// Draws the rectangle's four side segments, but only where they intersect the given circular mask.
    /// Each side is drawn by forwarding the call to the segment-level masked draw method.
    /// </summary>
    /// <param name="mask">The circular mask used to clip each side.</param>
    /// <param name="lineInfo">Line drawing parameters (thickness, color, cap style, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public void DrawLinesMasked(Circle mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        TopSegment.DrawMasked(mask, lineInfo, reversedMask);
        LeftSegment.DrawMasked(mask, lineInfo, reversedMask);
        BottomSegment.DrawMasked(mask, lineInfo, reversedMask);
        RightSegment.DrawMasked(mask, lineInfo, reversedMask);
    }
    /// <summary>
    /// Draws the rectangle's four side segments, but only where they intersect the given rectangular mask.
    /// Each side is drawn by forwarding the call to the segment-level masked draw method which performs
    /// clipping against the provided <see cref="Rect"/> mask.
    /// </summary>
    /// <param name="mask">The rectangular mask used to clip each side.</param>
    /// <param name="lineInfo">Line drawing parameters (thickness, color, cap style, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public void DrawLinesMasked(Rect mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        TopSegment.DrawMasked(mask, lineInfo, reversedMask);
        LeftSegment.DrawMasked(mask, lineInfo, reversedMask);
        BottomSegment.DrawMasked(mask, lineInfo, reversedMask);
        RightSegment.DrawMasked(mask, lineInfo, reversedMask);
    }
    /// <summary>
    /// Draws the rectangle's four side segments, but only where they intersect the given quadrilateral mask.
    /// Each side is forwarded to the corresponding segment-level masked draw method which handles clipping against the provided <see cref="Quad"/> mask.
    /// </summary>
    /// <param name="mask">The quadrilateral mask used to clip each side.</param>
    /// <param name="lineInfo">Line drawing parameters (thickness, color, cap style, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public void DrawLinesMasked(Quad mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        TopSegment.DrawMasked(mask, lineInfo, reversedMask);
        LeftSegment.DrawMasked(mask, lineInfo, reversedMask);
        BottomSegment.DrawMasked(mask, lineInfo, reversedMask);
        RightSegment.DrawMasked(mask, lineInfo, reversedMask);
    }
    /// <summary>
    /// Draws the rectangle's four side segments, but only where they intersect the given polygon mask.
    /// Each side is drawn by forwarding the call to the segment-level masked draw method which performs
    /// clipping against the provided <see cref="Polygon"/> mask.
    /// </summary>
    /// <param name="mask">The polygonal mask used to clip each side.</param>
    /// <param name="lineInfo">Line drawing parameters (thickness, color, cap style, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public void DrawLinesMasked(Polygon mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        TopSegment.DrawMasked(mask, lineInfo, reversedMask);
        LeftSegment.DrawMasked(mask, lineInfo, reversedMask);
        BottomSegment.DrawMasked(mask, lineInfo, reversedMask);
        RightSegment.DrawMasked(mask, lineInfo, reversedMask);
    }
    /// <summary>
    /// Draws the rectangle's four side segments clipped against a generic closed-shape mask.
    /// </summary>
    /// <typeparam name="T">
    /// The mask type implementing <see cref="IClosedShapeTypeProvider"/> (for example <see cref="Circle"/>, <see cref="Polygon"/>, <see cref="Quad"/>, etc.).
    /// </typeparam>
    /// <param name="mask">The mask used to clip each side.</param>
    /// <param name="lineInfo">Line drawing parameters (thickness, color, cap style, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public void DrawLinesMasked<T>(T mask, LineDrawingInfo lineInfo, bool reversedMask = false) where T : IClosedShapeTypeProvider
    {
        TopSegment.DrawMasked(mask, lineInfo, reversedMask);
        LeftSegment.DrawMasked(mask, lineInfo, reversedMask);
        BottomSegment.DrawMasked(mask, lineInfo, reversedMask);
        RightSegment.DrawMasked(mask, lineInfo, reversedMask);
    }
    #endregion
    
    #region Draw Vignette

    /// <summary>
    /// Draws a "vignette" effect inside the rect, creating a circular hole in the center.
    /// The area between the inner circle and the rect's outer edges is filled with the specified color.
    /// </summary>
    /// <param name="circleRadius">The radius of the inner circular hole.</param>
    /// <param name="circleRotDeg">The starting rotation angle of the inner circle in degrees.</param>
    /// <param name="color">The color of the filled area.</param>
    /// <param name="circleSmoothness">
    /// Determines the smoothness of the inner circle (0.0 to 1.0). 
    /// Higher values result in more segments and a smoother circle.
    /// </param>
    public void DrawVignette(float circleRadius, float circleRotDeg, ColorRgba color, float circleSmoothness = 0.5f)
    {
        var q = ToQuad();
        q.DrawVignette(circleRadius, circleRotDeg, color, circleSmoothness);
    }
    #endregion
    
    #region Gapped
    /// <summary>
    /// Draws a gapped outline for a rectangle, creating a dashed or segmented effect along the rectangle's perimeter.
    /// </summary>
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
    public float DrawGappedOutline(float perimeter, LineDrawingInfo lineInfo, GappedOutlineDrawingInfo gapDrawingInfo)
    {
        if (gapDrawingInfo.Gaps <= 0 || gapDrawingInfo.GapPerimeterPercentage <= 0f)
        {
            DrawLines(lineInfo);
            return perimeter > 0f ? perimeter : -1f;
        }

        if (gapDrawingInfo.GapPerimeterPercentage >= 1f) return perimeter > 0f ? perimeter : -1f;

        var nonGapPercentage = 1f - gapDrawingInfo.GapPerimeterPercentage;

        var gapPercentageRange = gapDrawingInfo.GapPerimeterPercentage / gapDrawingInfo.Gaps;
        var nonGapPercentageRange = nonGapPercentage / gapDrawingInfo.Gaps;

        
        var shapePoints = new[] {A, B, C, D};
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
                        Segment.DrawSegment(points[0], points[1], lineInfo);
                    }
                    else
                    {
                        for (var i = 0; i < points.Count - 1; i++)
                        {
                            var p1 = points[i];
                            var p2 = points[(i + 1) % points.Count];
                            Segment.DrawSegment(p1, p2, lineInfo);
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
    
    #region UI
    /// <summary>
    /// Draws a progress-style outline along the rectangle perimeter.
    /// </summary>
    /// <param name="f">The fraction of the perimeter to draw.</param>
    /// <param name="startIndex">The starting edge index for the outline progress.</param>
    /// <param name="lineThickness">The outline thickness.</param>
    /// <param name="color">The outline color.</param>
    /// <remarks>
    /// This is a convenience wrapper around <see cref="DrawLinesPercentage(float, int, float, ColorRgba)"/>.
    /// </remarks>
    public void DrawOutlineBar(float f, int startIndex, float lineThickness, ColorRgba color)
    {
        DrawLinesPercentage(f, startIndex, lineThickness, color);
    }

    /// <summary>
    /// Draws a rotated progress-style outline along the rectangle perimeter.
    /// </summary>
    /// <param name="f">The fraction of the perimeter to draw.</param>
    /// <param name="startIndex">The starting edge index for the outline progress.</param>
    /// <param name="angleDeg">The rotation angle in degrees.</param>
    /// <param name="pivot">The anchor point used as the rotation pivot.</param>
    /// <param name="lineThickness">The outline thickness.</param>
    /// <param name="color">The outline color.</param>
    /// <remarks>
    /// The rectangle is converted to a rotated <see cref="Quad"/> before drawing the partial outline.
    /// </remarks>
    public void DrawOutlineBar(float f, int startIndex, float angleDeg, AnchorPoint pivot, float lineThickness, ColorRgba color)
    {
        var q = new Quad(this, angleDeg, pivot);
        q.DrawLinesPercentage(f, startIndex, lineThickness, color);
    }
    
    /// <summary>
    /// Draws a rotated progress-style outline along the rectangle perimeter.
    /// </summary>
    /// <param name="f">The fraction of the perimeter to draw.</param>
    /// <param name="startIndex">The starting edge index for the outline progress.</param>
    /// <param name="angleDeg">The rotation angle in degrees.</param>
    /// <param name="pivot">The world-space point used as the rotation pivot.</param>
    /// <param name="lineThickness">The outline thickness.</param>
    /// <param name="color">The outline color.</param>
    /// <remarks>
    /// The rectangle is converted to a rotated <see cref="Quad"/> before drawing the partial outline.
    /// </remarks>
    public void DrawOutlineBar(float f, int startIndex, float angleDeg, Vector2 pivot, float lineThickness, ColorRgba color)
    {
        var q = new Quad(this, angleDeg, pivot);
        q.DrawLinesPercentage(f, startIndex, lineThickness, color);
    }
    
    /// <summary>
    /// Draws a filled bar inside a rectangle, representing progress with customizable margins and colors.
    /// </summary>
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
    public void DrawBar(float f, ColorRgba barColorRgba, ColorRgba bgColorRgba, float left = 0f, float right = 1f, float top = 0f, float bottom = 0f)
    {
        f = 1.0f - f;
        Margins progressMargins = new(f * top, f * right, f * bottom, f * left);
        var progressRect = ApplyMargins(progressMargins);
        Draw(bgColorRgba);
        progressRect.Draw(barColorRgba);
    }

    /// <summary>
    /// Draws a rotated filled bar inside a rectangle using an anchor-based pivot.
    /// </summary>
    /// <param name="f">The progress value that determines the filled portion.</param>
    /// <param name="angleDeg">The rotation angle in degrees.</param>
    /// <param name="pivot">The anchor point used as the rotation pivot.</param>
    /// <param name="barColorRgba">The fill color of the progress bar.</param>
    /// <param name="bgColorRgba">The background color drawn behind the bar.</param>
    /// <param name="left">The normalized left margin used to shape the fill direction.</param>
    /// <param name="right">The normalized right margin used to shape the fill direction.</param>
    /// <param name="top">The normalized top margin used to shape the fill direction.</param>
    /// <param name="bottom">The normalized bottom margin used to shape the fill direction.</param>
    /// <remarks>
    /// Margins are applied before the bar is converted to rotated quads for drawing.
    /// </remarks>
    public void DrawBar(float f, float angleDeg, AnchorPoint pivot, ColorRgba barColorRgba, ColorRgba bgColorRgba, float left = 0f, float right = 1f, float top = 0f, float bottom = 0f)
    {
        f = 1.0f - f;
        Margins progressMargins = new(f * top, f * right, f * bottom, f * left);
        var progressRect = ApplyMargins(progressMargins);
        var quad = new Quad(this, angleDeg, pivot);
        quad.Draw(bgColorRgba);
        var progressQuad = new Quad(progressRect, angleDeg, pivot);
        progressQuad.Draw(barColorRgba);
    }
    
    /// <summary>
    /// Draws a rotated filled bar inside a rectangle using a world-space pivot.
    /// </summary>
    /// <param name="f">The progress value that determines the filled portion.</param>
    /// <param name="angleDeg">The rotation angle in degrees.</param>
    /// <param name="pivot">The world-space point used as the rotation pivot.</param>
    /// <param name="barColorRgba">The fill color of the progress bar.</param>
    /// <param name="bgColorRgba">The background color drawn behind the bar.</param>
    /// <param name="left">The normalized left margin used to shape the fill direction.</param>
    /// <param name="right">The normalized right margin used to shape the fill direction.</param>
    /// <param name="top">The normalized top margin used to shape the fill direction.</param>
    /// <param name="bottom">The normalized bottom margin used to shape the fill direction.</param>
    /// <remarks>
    /// Margins are applied before the bar is converted to rotated quads for drawing.
    /// </remarks>
    public void DrawBar(float f, float angleDeg, Vector2 pivot, ColorRgba barColorRgba, ColorRgba bgColorRgba, float left = 0f, float right = 1f, float top = 0f, float bottom = 0f)
    {
        f = 1.0f - f;
        Margins progressMargins = new(f * top, f * right, f * bottom, f * left);
        var progressRect = ApplyMargins(progressMargins);
        var quad = new Quad(this, angleDeg, pivot);
        quad.Draw(bgColorRgba);
        var progressQuad = new Quad(progressRect, angleDeg, pivot);
        progressQuad.Draw(barColorRgba);
    }
    #endregion
}