using ShapeEngine.StaticLib;
//TODO: Move to core namespace! Definitely not in struct namespace!
namespace ShapeEngine.Core.Structs;
/// <summary>
/// Represents a curve of <see cref="float"/> values, allowing interpolation and sampling over a normalized time range [0, 1].
/// </summary>
/// <remarks>
/// Useful for animating or blending floating-point values over time.
/// </remarks>
public class CurveFloat(int capacity) : Curve<float>(capacity)
{
    /// <summary>
    /// Interpolates between two floating-point values using linear interpolation.
    /// </summary>
    /// <param name="a">The start value.</param>
    /// <param name="b">The end value.</param>
    /// <param name="time">The interpolation factor between 0 and 1.</param>
    /// <returns>The interpolated float value.</returns>
    protected override float Interpolate(float a, float b, float time)
    {
        return ShapeMath.LerpFloat(a, b, time);
    }

    /// <summary>
    /// Gets the default value for the curve, which is 0.0f.
    /// </summary>
    /// <returns>The default float value.</returns>
    protected override float GetDefaultValue() => 0f;
}