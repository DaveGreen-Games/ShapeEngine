using System.Numerics;
using ShapeEngine.Color;

namespace ShapeEngine.Core.BackendInterfaces;

/// <summary>
/// Interface for drawing backends that support basic drawing functionality..
/// Currently, this just serves as a placeholder and reminder for future development.
/// Has no use-case and should not be used!
/// </summary>
public interface IDrawingBackend
{
    /// <summary>
    /// Draws a triangle using three points specified by their x and y coordinates and a color.
    /// </summary>
    /// <param name="x1">The x-coordinate of the first vertex.</param>
    /// <param name="y1">The y-coordinate of the first vertex.</param>
    /// <param name="x2">The x-coordinate of the second vertex.</param>
    /// <param name="y2">The y-coordinate of the second vertex.</param>
    /// <param name="x3">The x-coordinate of the third vertex.</param>
    /// <param name="y3">The y-coordinate of the third vertex.</param>
    /// <param name="color">The color of the triangle.</param>
    public void DrawTriangle(float x1, float y1, float x2, float y2, float x3, float y3, ColorRgba color);

    /// <summary>
    /// Draws a triangle using three <see cref="Vector2"/> points and a color.
    /// </summary>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <param name="color">The color of the triangle.</param>
    public void DrawTriangle(Vector2 a, Vector2 b, Vector2 c, ColorRgba color);
}