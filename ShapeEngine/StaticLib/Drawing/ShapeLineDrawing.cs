using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Geometry.Line;

namespace ShapeEngine.StaticLib.Drawing;

/// <summary>
/// Provides static methods for drawing lines using specified parameters or a <c>Line</c> object.
/// </summary>
public static class ShapeLineDrawing
{
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
        ShapeSegmentDrawing.DrawSegment(point - direction * length * 0.5f, point + direction * length * 0.5f, thickness, color);
    }

    /// <summary>
    /// Draws a line using a <c>Line</c> object, with the specified length, thickness, and color.
    /// </summary>
    /// <param name="line">The <c>Line</c> object to draw.</param>
    /// <param name="length">The length of the line.</param>
    /// <param name="thickness">The thickness of the line.</param>
    /// <param name="color">The color of the line.</param>
    /// /// <remarks>
    /// If <paramref name="line"/> is not valid,
    /// or <paramref name="length"/> is less than or equal to 0,
    /// or <paramref name="thickness"/> is less than or equal to 0, the line is not drawn.
    /// </remarks>
    public static void Draw(this Line line, float length, float thickness, ColorRgba color)
    {
        if(!line.IsValid || length <= 0f || thickness <= 0f) return;
        ShapeSegmentDrawing.DrawSegment(line.Point - (line.Direction * length * 0.5f), line.Point + (line.Direction * length * 0.5f), thickness, color);
    }

}