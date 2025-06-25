using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Geometry.Segment;

namespace ShapeEngine.Geometry.Ray;

/// <summary>
/// Provides static methods for drawing rays using specified parameters or a <c>Ray</c> object.
/// </summary>
public static class RayDrawing
{
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
        SegmentDrawing.DrawSegment(point, point + direction * length, thickness, color);
    }

    /// <summary>
    /// Draws a ray using a <c>Ray</c> object, with a specified length, thickness, and color.
    /// </summary>
    /// <param name="ray">The <c>Ray</c> object to draw. Must be valid.</param>
    /// <param name="length">The length of the ray. Must be greater than zero.</param>
    /// <param name="thickness">The thickness of the ray. Must be greater than zero.</param>
    /// <param name="color">The color of the ray.</param>
    /// <remarks>
    /// Returns immediately without drawing if <paramref name="ray"/> is not valid,
    /// or if <paramref name="length"/> or <paramref name="thickness"/> is less than or equal to zero.
    /// </remarks>
    public static void Draw(this Ray ray, float length, float thickness, ColorRgba color)
    {
        if(!ray.IsValid || length <= 0f || thickness <= 0f) return;
        SegmentDrawing.DrawSegment(ray.Point, ray.Point + ray.Direction * length, thickness, color);
    }

}