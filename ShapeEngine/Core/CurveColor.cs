using ShapeEngine.Color;

namespace ShapeEngine.Core;
/// <summary>
/// Represents a curve of <see cref="ColorRgba"/> values, allowing interpolation and sampling over a normalized time range [0, 1].
/// </summary>
/// <remarks>
/// Useful for animating or blending color values over time.
/// </remarks>
public class CurveColor(int capacity) : Curve<ColorRgba>(capacity)
{
    /// <summary>
    /// Interpolates between two <see cref="ColorRgba"/> values using linear interpolation.
    /// </summary>
    /// <param name="a">The start color.</param>
    /// <param name="b">The end color.</param>
    /// <param name="time">The interpolation factor between 0 and 1.</param>
    /// <returns>The interpolated <see cref="ColorRgba"/> value.</returns>
    protected override ColorRgba Interpolate(ColorRgba a, ColorRgba b, float time)
    {
        return a.Lerp(b, time);
    }

    /// <summary>
    /// Gets the default value for the curve, which is <see cref="ColorRgba.Clear"/>.
    /// </summary>
    /// <returns>The default <see cref="ColorRgba"/> value.</returns>
    protected override ColorRgba GetDefaultValue() => ColorRgba.Clear;
}