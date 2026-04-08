using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.LineDef;

/// <summary>
/// Provides drawing methods for <see cref="Line"/> values.
/// </summary>
public readonly partial struct Line
{
    #region Draw Masked
    /// <summary>
    /// Draws a masked portion of the line centered on this line with the specified <paramref name="length"/>,
    /// using the provided <see cref="Triangle"/> <paramref name="mask"/> and drawing parameters in <paramref name="lineInfo"/>.
    /// </summary>
    /// <param name="length">Total length of the segment to draw (must be greater than 0).</param>
    /// <param name="mask">Triangle mask used to clip the drawn segment.</param>
    /// <param name="lineInfo">Line drawing configuration (thickness, color, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public void DrawMasked(float length, Triangle mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        if(!IsValid || length <= 0f || lineInfo.Thickness <= 0f) return;
        var start = Point - (Direction * length * 0.5f);
        var end = Point + (Direction * length * 0.5f);
        var segment = new Segment(start, end);
        segment.DrawMasked(mask, lineInfo, reversedMask);
    }

    /// <summary>
    /// Draws a masked portion of the line centered on this line with the specified <paramref name="length"/>,
    /// using the provided <see cref="Circle"/> <paramref name="mask"/> and drawing parameters in <paramref name="lineInfo"/>.
    /// </summary>
    /// <param name="length">Total length of the segment to draw (must be greater than 0).</param>
    /// <param name="mask">Circle mask used to clip the drawn segment.</param>
    /// <param name="lineInfo">Line drawing configuration (thickness, color, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public void DrawMasked(float length, Circle mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        if(!IsValid || length <= 0f || lineInfo.Thickness <= 0f) return;
        var start = Point - (Direction * length * 0.5f);
        var end = Point + (Direction * length * 0.5f);
        var segment = new Segment(start, end);
        segment.DrawMasked(mask, lineInfo, reversedMask);
    }

    /// <summary>
    /// Draws a masked portion of the line centered on this line with the specified <paramref name="length"/>,
    /// using the provided <see cref="Rect"/> <paramref name="mask"/> and drawing parameters in <paramref name="lineInfo"/>.
    /// </summary>
    /// <param name="length">Total length of the segment to draw (must be greater than 0).</param>
    /// <param name="mask">Rect mask used to clip the drawn segment.</param>
    /// <param name="lineInfo">Line drawing configuration (thickness, color, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public void DrawMasked(float length, Rect mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        if(!IsValid || length <= 0f || lineInfo.Thickness <= 0f) return;
        var start = Point - (Direction * length * 0.5f);
        var end = Point + (Direction * length * 0.5f);
        var segment = new Segment(start, end);
        segment.DrawMasked(mask, lineInfo, reversedMask);
    }

    /// <summary>
    /// Draws a masked portion of the line centered on this line with the specified <paramref name="length"/>,
    /// using the provided <see cref="Quad"/> <paramref name="mask"/> and drawing parameters in <paramref name="lineInfo"/>.
    /// </summary>
    /// <param name="length">Total length of the segment to draw (must be greater than 0).</param>
    /// <param name="mask">Quad mask used to clip the drawn segment.</param>
    /// <param name="lineInfo">Line drawing configuration (thickness, color, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public void DrawMasked(float length, Quad mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        if(!IsValid || length <= 0f || lineInfo.Thickness <= 0f) return;
        var start = Point - (Direction * length * 0.5f);
        var end = Point + (Direction * length * 0.5f);
        var segment = new Segment(start, end);
        segment.DrawMasked(mask, lineInfo, reversedMask);
    }

    /// <summary>
    /// Draws a masked portion of the line centered on this line with the specified <paramref name="length"/>,
    /// using the provided <see cref="Polygon"/> <paramref name="mask"/> and drawing parameters in <paramref name="lineInfo"/>.
    /// </summary>
    /// <param name="length">Total length of the segment to draw (must be greater than 0).</param>
    /// <param name="mask">Polygon mask used to clip the drawn segment.</param>
    /// <param name="lineInfo">Line drawing configuration (thickness, color, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public void DrawMasked(float length, Polygon mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        if(!IsValid || length <= 0f || lineInfo.Thickness <= 0f) return;
        var start = Point - (Direction * length * 0.5f);
        var end = Point + (Direction * length * 0.5f);
        var segment = new Segment(start, end);
        segment.DrawMasked(mask, lineInfo, reversedMask);
    }

    /// <summary>
    /// Draws a masked portion of the line centered on this line with the specified <paramref name="length"/>,
    /// using a mask of generic type <typeparamref name="T"/> which implements <see cref="IClosedShapeTypeProvider"/>.
    /// </summary>
    /// <typeparam name="T">The mask type that provides a closed shape for clipping (must implement <see cref="IClosedShapeTypeProvider"/>).</typeparam>
    /// <param name="length">Total length of the segment to draw (must be greater than 0).</param>
    /// <param name="mask">Mask used to clip the drawn segment.</param>
    /// <param name="lineInfo">Line drawing configuration (thickness, color, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    /// <remarks>
    /// If this line is not valid, <paramref name="length"/> is less than or equal to 0,
    /// or <paramref name="lineInfo"/> has non-positive thickness, nothing will be drawn.
    /// </remarks>
    public void DrawMasked<T>(float length, T mask, LineDrawingInfo lineInfo, bool reversedMask = false) where T : IClosedShapeTypeProvider
    {
        if(!IsValid || length <= 0f || lineInfo.Thickness <= 0f) return;
        var start = Point - (Direction * length * 0.5f);
        var end = Point + (Direction * length * 0.5f);
        var segment = new Segment(start, end);
        segment.DrawMasked(mask, lineInfo, reversedMask);
    }
    #endregion
    
    /// <summary>
    /// Draws a line at a given point, in a specified direction, with a given length, thickness, and color.
    /// </summary>
    /// <param name="point">The center point of the line.</param>
    /// <param name="direction">The normalized direction vector of the line.</param>
    /// <param name="length">The length of the line.</param>
    /// <param name="thickness">The thickness of the line.</param>
    /// <param name="color">The color of the line.</param>
    /// <remarks>
    /// If <paramref name="length"/> is less than or equal to 0,
    /// <paramref name="thickness"/> is less than or equal to 0,
    /// or <paramref name="direction"/> is a zero vector, the line is not drawn.
    /// </remarks>
    public static void DrawLine(Vector2 point, Vector2 direction, float length, float thickness, ColorRgba color)
    {
        if(length <= 0 || thickness <= 0 || (direction.X == 0f && direction.Y == 0f)) return;
        Segment.DrawSegment(point - direction * length * 0.5f, point + direction * length * 0.5f, thickness, color);
    }

    /// <summary>
    /// Draws this line with the specified length, thickness, and color.
    /// </summary>
    /// <param name="length">The length of the line.</param>
    /// <param name="thickness">The thickness of the line.</param>
    /// <param name="color">The color of the line.</param>
    /// /// <remarks>
    /// If <paramref name="length"/> is less than or equal to 0,
    /// or <paramref name="thickness"/> is less than or equal to 0, the line is not drawn.
    /// </remarks>
    public void Draw(float length, float thickness, ColorRgba color)
    {
        if(!IsValid || length <= 0f || thickness <= 0f) return;
        Segment.DrawSegment(Point - (Direction * length * 0.5f), Point + (Direction * length * 0.5f), thickness, color);
    }

}