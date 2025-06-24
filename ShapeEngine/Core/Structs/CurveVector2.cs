using System.Numerics;
using ShapeEngine.StaticLib;
//TODO: Move to core namespace! Definitely not in struct namespace!
namespace ShapeEngine.Core.Structs;
/// <summary>
/// Represents a curve of <see cref="Vector2"/> values, allowing interpolation and sampling over a normalized time range [0, 1].
/// </summary>
/// <remarks>
/// Useful for animating or blending 2D vectors over time.
/// </remarks>
public class CurveVector2(int capacity) : Curve<Vector2>(capacity)
{
    /// <summary>
    /// Interpolates between two <see cref="Vector2"/> values using linear interpolation.
    /// </summary>
    /// <param name="a">The start vector.</param>
    /// <param name="b">The end vector.</param>
    /// <param name="time">The interpolation factor between 0 and 1.</param>
    /// <returns>The interpolated <see cref="Vector2"/>.</returns>
    protected override Vector2 Interpolate(Vector2 a, Vector2 b, float time)
    {
        return a.Lerp(b, time);
    }

    /// <summary>
    /// Gets the default value for the curve, which is <see cref="Vector2.Zero"/>.
    /// </summary>
    /// <returns>The default <see cref="Vector2"/> value.</returns>
    protected override Vector2 GetDefaultValue() => Vector2.Zero;
}