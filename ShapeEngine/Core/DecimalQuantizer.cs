using System.Numerics;

namespace ShapeEngine.Core;

/// <summary>
/// Quantizes floating-point values and vectors using a shared <see cref="DecimalPrecision"/> setting.
/// </summary>
/// <remarks>
/// This type converts floating-point values into deterministic integer-like buckets so that equality and hashing
/// can be made tolerant to small rounding differences.
/// </remarks>
public readonly struct DecimalQuantizer
{
    /// <summary>
    /// Gets a default quantizer using <see cref="DecimalPrecision.DefaultDecimalPlaces"/>.
    /// </summary>
    public static readonly DecimalQuantizer Default = new DecimalQuantizer(DecimalPrecision.DefaultDecimalPlaces);
    
    /// <summary>
    /// Gets the precision configuration used by this quantizer.
    /// </summary>
    public readonly DecimalPrecision Precision;
    
    /// <summary>
    /// Creates a new quantizer for the specified decimal precision.
    /// </summary>
    /// <param name="decimalPlaces">
    /// The number of decimal places used when quantizing values.
    /// </param>
    public DecimalQuantizer(int decimalPlaces = 4)
    {
        Precision = new DecimalPrecision(decimalPlaces);
    }
    
    #region Public Methods
    
    /// <summary>
    /// Quantizes a <see cref="float"/> value to a stable integer-like bucket.
    /// </summary>
    /// <param name="value">The value to quantize.</param>
    /// <returns>The quantized representation of <paramref name="value"/>.</returns>
    public long Quantize(float value)
    {
        if (float.IsNaN(value)) return long.MinValue;
        if (float.IsPositiveInfinity(value)) return long.MaxValue;
        if (float.IsNegativeInfinity(value)) return long.MinValue + 1;

        long quantized = (long)Math.Round(value * Precision.Scale);
        return quantized == 0L ? 0L : quantized;
    }
    
    /// <summary>
    /// Quantizes a <see cref="double"/> value to a stable integer-like bucket.
    /// </summary>
    /// <param name="value">The value to quantize.</param>
    /// <returns>The quantized representation of <paramref name="value"/>.</returns>
    public long Quantize(double value)
    {
        if (double.IsNaN(value)) return long.MinValue;
        if (double.IsPositiveInfinity(value)) return long.MaxValue;
        if (double.IsNegativeInfinity(value)) return long.MinValue + 1;

        long quantized = (long)Math.Round(value * Precision.Scale);
        return quantized == 0L ? 0L : quantized;
    }
    
    /// <summary>
    /// Compares two <see cref="Vector2"/> values after quantizing their components.
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns><c>true</c> if both vectors are equal after quantization; otherwise, <c>false</c>.</returns>
    public bool QuantizedEquals(Vector2 a, Vector2 b)
    {
        return Quantize(a.X) == Quantize(b.X) &&
               Quantize(a.Y) == Quantize(b.Y);
    }
    
    /// <summary>
    /// Compares two <see cref="float"/> values after quantization.
    /// </summary>
    /// <param name="a">The first value.</param>
    /// <param name="b">The second value.</param>
    /// <returns><c>true</c> if both values are equal after quantization; otherwise, <c>false</c>.</returns>
    public bool QuantizedEquals(float a, float b)
    {
        return Quantize(a) == Quantize(b);
    }
    
    /// <summary>
    /// Compares two <see cref="double"/> values after quantization.
    /// </summary>
    /// <param name="a">The first value.</param>
    /// <param name="b">The second value.</param>
    /// <returns><c>true</c> if both values are equal after quantization; otherwise, <c>false</c>.</returns>
    public bool QuantizedEquals(double a, double b)
    {
        return Quantize(a) == Quantize(b);
    }
    
    #endregion
}