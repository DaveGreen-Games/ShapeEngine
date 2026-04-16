using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.RayDef;

/// <summary>
/// Provides drawing methods for <see cref="Ray"/> values.
/// </summary>
public readonly partial struct Ray
{
    #region Draw Masked
    /// <summary>
    /// Draws a masked ray by creating a segment from the ray origin in its direction for the specified length,
    /// then drawing that segment clipped by the provided <see cref="Triangle"/> mask.
    /// </summary>
    /// <param name="length">The length of the ray to draw. If less than or equal to zero, the method returns immediately.</param>
    /// <param name="mask">The <see cref="Triangle"/> used as a clipping mask.</param>
    /// <param name="lineInfo">Line drawing parameters (thickness, color, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public void DrawMasked(float length, Triangle mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        if(!IsValid || length <= 0f || lineInfo.Thickness <= 0f) return;
        var segment = new Segment(Point, Point + Direction * length);
        segment.DrawMasked(mask, lineInfo, reversedMask);
    }

    /// <summary>
    /// Draws a masked ray by creating a segment from the ray origin in its direction for the specified length,
    /// then drawing that segment clipped by the provided <see cref="Circle"/> mask.
    /// </summary>
    /// <param name="length">The length of the ray to draw. If less than or equal to zero, the method returns immediately.</param>
    /// <param name="mask">The <see cref="Circle"/> used as a clipping mask.</param>
    /// <param name="lineInfo">Line drawing parameters (thickness, color, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public void DrawMasked(float length, Circle mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        if(!IsValid || length <= 0f || lineInfo.Thickness <= 0f) return;
        var segment = new Segment(Point, Point + Direction * length);
        segment.DrawMasked(mask, lineInfo, reversedMask);
    }

    /// <summary>
    /// Draws a masked ray by creating a segment from the ray origin in its direction for the specified length,
    /// then drawing that segment clipped by the provided <see cref="Rect"/> mask.
    /// </summary>
    /// <param name="length">The length of the ray to draw. If less than or equal to zero, the method returns immediately.</param>
    /// <param name="mask">The <see cref="Rect"/> used as a clipping mask.</param>
    /// <param name="lineInfo">Line drawing parameters (thickness, color, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public void DrawMasked(float length, Rect mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        if(!IsValid || length <= 0f || lineInfo.Thickness <= 0f) return;
        var segment = new Segment(Point, Point + Direction * length);
        segment.DrawMasked(mask, lineInfo, reversedMask);
    }

    /// <summary>
    /// Draws a masked ray by creating a segment from the ray origin in its direction for the specified length,
    /// then drawing that segment clipped by the provided <see cref="Quad"/> mask.
    /// </summary>
    /// <param name="length">The length of the ray to draw. If less than or equal to zero, the method returns immediately.</param>
    /// <param name="mask">The <see cref="Quad"/> used as a clipping mask.</param>
    /// <param name="lineInfo">Line drawing parameters (thickness, color, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public void DrawMasked(float length, Quad mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        if(!IsValid || length <= 0f || lineInfo.Thickness <= 0f) return;
        var segment = new Segment(Point, Point + Direction * length);
        segment.DrawMasked(mask, lineInfo, reversedMask);
    }

    /// <summary>
    /// Draws a masked ray by creating a segment from the ray origin in its direction for the specified length,
    /// then drawing that segment clipped by the provided <see cref="Polygon"/> mask.
    /// </summary>
    /// <param name="length">The length of the ray to draw. If less than or equal to zero, the method returns immediately.</param>
    /// <param name="mask">The <see cref="Polygon"/> used as a clipping mask.</param>
    /// <param name="lineInfo">Line drawing parameters (thickness, color, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public void DrawMasked(float length, Polygon mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        if(!IsValid || length <= 0f || lineInfo.Thickness <= 0f) return;
        var segment = new Segment(Point, Point + Direction * length);
        segment.DrawMasked(mask, lineInfo, reversedMask);
    }

    /// <summary>
    /// Draws a masked ray by creating a segment from the ray origin in its direction for the specified length,
    /// then drawing that segment clipped by the provided mask of a generic closed-shape type.
    /// </summary>
    /// <typeparam name="T">Type of the mask that implements <see cref="IClosedShapeTypeProvider"/>.</typeparam>
    /// <param name="length">The length of the ray to draw. If less than or equal to zero, the method returns immediately.</param>
    /// <param name="mask">The mask shape used for clipping.</param>
    /// <param name="lineInfo">Line drawing parameters (thickness, color, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public void DrawMasked<T>(float length, T mask, LineDrawingInfo lineInfo, bool reversedMask = false) where T : IClosedShapeTypeProvider
    {
        if(!IsValid || length <= 0f || lineInfo.Thickness <= 0f) return;
        var segment = new Segment(Point, Point + Direction * length);
        segment.DrawMasked(mask, lineInfo, reversedMask);
    }
    #endregion
    
    /// <summary>
    /// Draws a ray starting from a given point in a specified direction, with a given length, thickness, and color.
    /// </summary>
    /// <param name="point">The starting point of the ray.</param>
    /// <param name="direction">The direction vector of the ray. Must not be zero.</param>
    /// <param name="length">The length of the ray. Must be greater than zero.</param>
    /// <param name="thickness">The thickness of the ray. Must be greater than zero.</param>
    /// <param name="color">The color of the ray.</param>
    /// <remarks>
    /// Returns immediately without drawing if <paramref name="length"/> or <paramref name="thickness"/> is less than or equal to zero,
    /// or if <paramref name="direction"/> is a zero vector.
    /// </remarks>
    public static void DrawRay(Vector2 point, Vector2 direction, float length, float thickness, ColorRgba color)
    {
        if(length <= 0 || thickness <= 0 || (direction.X == 0f && direction.Y == 0f)) return;
        Segment.DrawSegment(point, point + direction * length, thickness, color);
    }

    /// <summary>
    /// Draws this ray with a specified length, thickness, and color.
    /// </summary>
    /// <param name="length">The length of the ray. Must be greater than zero.</param>
    /// <param name="thickness">The thickness of the ray. Must be greater than zero.</param>
    /// <param name="color">The color of the ray.</param>
    /// <remarks>
    /// Returns immediately without drawing if <paramref name="ray"/> is not valid,
    /// or if <paramref name="length"/> or <paramref name="thickness"/> is less than or equal to zero.
    /// </remarks>
    public void Draw(float length, float thickness, ColorRgba color)
    {
        if(!IsValid || length <= 0f || thickness <= 0f) return;
        Segment.DrawSegment(Point, Point + Direction * length, thickness, color);
    }

}