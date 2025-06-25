using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Geometry;

namespace ShapeEngine.StaticLib.Drawing;

/// <summary>
/// Provides extension methods for drawing points and collections of points.
/// </summary>
public static class ShapePointDrawing
{
    /// <summary>
    /// Draws a point as a circle at the specified position.
    /// </summary>
    /// <param name="p">The position of the point.</param>
    /// <param name="radius">The radius of the circle to draw.</param>
    /// <param name="color">The color of the circle.</param>
    /// <param name="segments">The number of segments to use for the circle. Default is 16.</param>
    public static void Draw(this Vector2 p, float radius, ColorRgba color, int segments = 16) => ShapeCircleDrawing.DrawCircle(p, radius, color, segments);

    /// <summary>
    /// Draws each point in a collection as a circle.
    /// </summary>
    /// <param name="points">The collection of points to draw.</param>
    /// <param name="r">The radius of each circle.</param>
    /// <param name="color">The color of the circles.</param>
    /// <param name="segments">The number of segments to use for each circle. Default is 16.</param>
    public static void Draw(this Points points, float r, ColorRgba color, int segments = 16)
    {
        foreach (var p in points)
        {
            p.Draw(r, color, segments);
        }
    }

}