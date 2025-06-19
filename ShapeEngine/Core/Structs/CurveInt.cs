using ShapeEngine.StaticLib;
//TODO: Move to core namespace! Definitely not in struct namespace!
namespace ShapeEngine.Core.Structs;
/// <summary>
/// Represents a curve of <see cref="int"/> values, allowing interpolation and sampling over a normalized time range [0, 1].
/// </summary>
/// <remarks>
/// Useful for animating or blending integer values over time.
/// </remarks>
public class CurveInt(int capacity) : Curve<int>(capacity)
{
    /// <summary>
    /// Interpolates between two integer values using linear interpolation.
    /// </summary>
    /// <param name="a">The start value.</param>
    /// <param name="b">The end value.</param>
    /// <param name="time">The interpolation factor between 0 and 1.</param>
    /// <returns>The interpolated integer value.</returns>
    protected override int Interpolate(int a, int b, float time)
    {
        return ShapeMath.LerpInt(a, b, time);
    }

    /// <summary>
    /// Gets the default value for the curve, which is 0.
    /// </summary>
    /// <returns>The default integer value.</returns>
    protected override int GetDefaultValue() => 0;
}