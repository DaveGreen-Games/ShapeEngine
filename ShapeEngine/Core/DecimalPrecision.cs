using System.Numerics;

namespace ShapeEngine.Core;

//Q: Does this make sense?
public readonly struct Fnv1aHash
{
    public readonly ulong Value;

    public Fnv1aHash(float value, int decimalPlaces = 4)
    {
        var quantizer = new Fnv1aHashQuantizer(decimalPlaces);
        Value = quantizer.GetHash(value);
    }
    //TODO: Add constructors for all GetHash overloads
}

//Q: Should this be a class?
//TODO: Use in shape classes
//TODO: Docs
public struct Fnv1aHashQuantizer
{
    /// <summary>
    /// Gets the FNV-1a 64-bit offset basis used by the shared hash helpers.
    /// </summary>
    public const ulong FnvOffset = 14695981039346656037UL;

    /// <summary>
    /// Gets the FNV-1a 64-bit prime used by the shared hash helpers.
    /// </summary>
    public const ulong FnvPrime = 1099511628211UL;

    public static readonly Fnv1aHashQuantizer Default = new Fnv1aHashQuantizer(DecimalPrecision.DefaultDecimalPlaces);
    
    public readonly DecimalQuantizer Quantizer;
    private ulong currentHash; //NOTE: I dont like this in a struct... make an extra class for Start/EndHash functionality?
    
    public Fnv1aHashQuantizer(int decimalPlaces = 4)
    {
        Quantizer = new DecimalQuantizer(decimalPlaces);
        currentHash = FnvOffset;
    }

    
    private DecimalQuantizer GetEffectiveQuantizer()
    {
        return Quantizer.Precision.Scale > 0.0 ? Quantizer : DecimalQuantizer.Default;
    }

    
    public void StartHash(int count)
    {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));

        currentHash = FnvOffset;
        unchecked
        {
            currentHash ^= (ulong)count;
            currentHash *= FnvPrime;
        }
    }

    public ulong EndHash() => currentHash;

    
    public void Add(float value)
    {
        currentHash = HashQuantized(currentHash, value);
    }

    public void Add(double value)
    {
        currentHash = HashQuantized(currentHash, value);
    }

    public void Add(Vector2 value)
    {
        Add(value.X);
        Add(value.Y);
    }
    
    
    public ulong HashQuantized(ulong hash, float value)
    {
        long quantized = GetEffectiveQuantizer().Quantize(value);

        unchecked
        {
            hash ^= (ulong)quantized;
            hash *= FnvPrime;
        }

        return hash;
    }
    
    public ulong HashQuantized(ulong hash, double value)
    {
        long quantized = GetEffectiveQuantizer().Quantize(value);

        unchecked
        {
            hash ^= (ulong)quantized;
            hash *= FnvPrime;
        }

        return hash;
    }

    public ulong HashQuantized(ulong hash, Vector2 value)
    {
        hash = HashQuantized(hash, value.X);
        hash = HashQuantized(hash, value.Y);
        return hash;
    }

    
    public ulong GetHash(float value)
    {
        var hasher = this;
        hasher.StartHash(1);
        hasher.Add(value);
        return hasher.EndHash();
    }

    public ulong GetHash(float valueA, float valueB)
    {
        var hasher = this;
        hasher.StartHash(2);
        hasher.Add(valueA);
        hasher.Add(valueB);
        return hasher.EndHash();
    }

    public ulong GetHash(float valueA, float valueB, float valueC)
    {
        var hasher = this;
        hasher.StartHash(3);
        hasher.Add(valueA);
        hasher.Add(valueB);
        hasher.Add(valueC);
        return hasher.EndHash();
    }

    public ulong GetHash(float valueA, float valueB, float valueC, float valueD)
    {
        var hasher = this;
        hasher.StartHash(4);
        hasher.Add(valueA);
        hasher.Add(valueB);
        hasher.Add(valueC);
        hasher.Add(valueD);
        return hasher.EndHash();
    }

    public ulong GetHash(IReadOnlyList<float> values)
    {
        if (values == null) throw new ArgumentNullException(nameof(values));

        var hasher = this;
        hasher.StartHash(values.Count);
        for (int i = 0; i < values.Count; i++)
        {
            hasher.Add(values[i]);
        }
        return hasher.EndHash();
    }

    
    public ulong GetHash(double value)
    {
        var hasher = this;
        hasher.StartHash(1);
        hasher.Add(value);
        return hasher.EndHash();
    }

    public ulong GetHash(double valueA, double valueB)
    {
        var hasher = this;
        hasher.StartHash(2);
        hasher.Add(valueA);
        hasher.Add(valueB);
        return hasher.EndHash();
    }

    public ulong GetHash(double valueA, double valueB, double valueC)
    {
        var hasher = this;
        hasher.StartHash(3);
        hasher.Add(valueA);
        hasher.Add(valueB);
        hasher.Add(valueC);
        return hasher.EndHash();
    }

    public ulong GetHash(double valueA, double valueB, double valueC, double valueD)
    {
        var hasher = this;
        hasher.StartHash(4);
        hasher.Add(valueA);
        hasher.Add(valueB);
        hasher.Add(valueC);
        hasher.Add(valueD);
        return hasher.EndHash();
    }

    public ulong GetHash(IReadOnlyList<double> values)
    {
        if (values == null) throw new ArgumentNullException(nameof(values));

        var hasher = this;
        hasher.StartHash(values.Count);
        for (int i = 0; i < values.Count; i++)
        {
            hasher.Add(values[i]);
        }
        return hasher.EndHash();
    }

    
    public ulong GetHash(Vector2 value)
    {
        var hasher = this;
        hasher.StartHash(1);
        hasher.Add(value);
        return hasher.EndHash();
    }

    public ulong GetHash(Vector2 valueA, Vector2 valueB)
    {
        var hasher = this;
        hasher.StartHash(2);
        hasher.Add(valueA);
        hasher.Add(valueB);
        return hasher.EndHash();
    }

    public ulong GetHash(Vector2 valueA, Vector2 valueB, Vector2 valueC)
    {
        var hasher = this;
        hasher.StartHash(3);
        hasher.Add(valueA);
        hasher.Add(valueB);
        hasher.Add(valueC);
        return hasher.EndHash();
    }

    public ulong GetHash(Vector2 valueA, Vector2 valueB, Vector2 valueC, Vector2 valueD)
    {
        var hasher = this;
        hasher.StartHash(4);
        hasher.Add(valueA);
        hasher.Add(valueB);
        hasher.Add(valueC);
        hasher.Add(valueD);
        return hasher.EndHash();
    }

    public ulong GetHash(IReadOnlyList<Vector2> values)
    {
        if (values == null) throw new ArgumentNullException(nameof(values));

        var hasher = this;
        hasher.StartHash(values.Count);
        for (int i = 0; i < values.Count; i++)
        {
            hasher.Add(values[i]);
        }
        return hasher.EndHash();
    }
}


//TODO: Docs
public readonly struct DecimalQuantizer
{
    public static readonly DecimalQuantizer Default = new DecimalQuantizer(DecimalPrecision.DefaultDecimalPlaces);
    
    public readonly DecimalPrecision Precision;
    
    public DecimalQuantizer(int decimalPlaces = 4)
    {
        Precision = new DecimalPrecision(decimalPlaces);
    }
    
    
    public long Quantize(float value)
    {
        if (float.IsNaN(value)) return long.MinValue;
        if (float.IsPositiveInfinity(value)) return long.MaxValue;
        if (float.IsNegativeInfinity(value)) return long.MinValue + 1;

        long quantized = (long)Math.Round(value * Precision.Scale);
        return quantized == 0L ? 0L : quantized;
    }
    
    public long Quantize(double value)
    {
        if (double.IsNaN(value)) return long.MinValue;
        if (double.IsPositiveInfinity(value)) return long.MaxValue;
        if (double.IsNegativeInfinity(value)) return long.MinValue + 1;

        long quantized = (long)Math.Round(value * Precision.Scale);
        return quantized == 0L ? 0L : quantized;
    }
    
    public bool QuantizedEquals(Vector2 a, Vector2 b)
    {
        return Quantize(a.X) == Quantize(b.X) &&
               Quantize(a.Y) == Quantize(b.Y);
    }
    
    public bool QuantizedEquals(float a, float b)
    {
        return Quantize(a) == Quantize(b);
    }
    
    public bool QuantizedEquals(double a, double b)
    {
        return Quantize(a) == Quantize(b);
    }

}

/// <summary>
/// Represents a decimal-place precision setting together with its derived base-10 scale factors.
/// </summary>
/// <remarks>
/// This type is useful when converting floating-point values into quantized integer-like values,
/// for example during geometric hashing or when interfacing with systems that operate on scaled coordinates.
/// Supported precision values are clamped to the inclusive range <c>0</c> to <c>16</c>.
/// </remarks>
public readonly struct DecimalPrecision
{
    /// <summary>
    /// Gets the default number of decimal places used by quantized hashing and equality helpers.
    /// </summary>
    public const int DefaultDecimalPlaces = 3;

    /// <summary>
    /// Gets the FNV-1a 64-bit offset basis used by the shared hash helpers.
    /// </summary>
    public const ulong FnvOffset = 14695981039346656037UL;

    /// <summary>
    /// Gets the FNV-1a 64-bit prime used by the shared hash helpers.
    /// </summary>
    public const ulong FnvPrime = 1099511628211UL;

    /// <summary>
    /// Gets the number of decimal places used for quantization.
    /// </summary>
    public readonly int DecimalPlaces;

    /// <summary>
    /// Gets the base-10 scale factor for <see cref="DecimalPlaces"/>.
    /// </summary>
    /// <remarks>
    /// For example, a precision of <c>4</c> yields a scale factor of <c>10000</c>.
    /// </remarks>
    public readonly double Scale;

    /// <summary>
    /// Gets the reciprocal of <see cref="Scale"/>.
    /// </summary>
    public readonly double InvScale;
    
    /// <summary>
    /// Creates a new <see cref="DecimalPrecision"/> value.
    /// </summary>
    /// <param name="decimalPlaces">
    /// The requested number of decimal places. Values are clamped to the inclusive range <c>0</c> to <c>16</c>.
    /// </param>
    public DecimalPrecision(int decimalPlaces = 4)
    {
        DecimalPlaces = Math.Clamp(decimalPlaces, 0, 16);
        Scale = GetScaleFactor(DecimalPlaces);
        InvScale = 1.0 / Scale;
    }

    private static readonly double[] scaleTable =
    {
        1d,
        10d,
        100d,
        1000d,
        10000d,
        100000d,
        1000000d,
        10000000d,
        100000000d,
        1000000000d,
        10000000000d,
        100000000000d,
        1000000000000d,
        10000000000000d,
        100000000000000d,
        1000000000000000d,
        10000000000000000d
    };
    
    /// <summary>Gets 10^0.</summary>
    public static double Pow10_0  => 1d;
    /// <summary>Gets 10^1.</summary>
    public static double Pow10_1  => 10d;
    /// <summary>Gets 10^2.</summary>
    public static double Pow10_2  => 100d;
    /// <summary>Gets 10^3.</summary>
    public static double Pow10_3  => 1000d;
    /// <summary>Gets 10^4.</summary>
    public static double Pow10_4  => 10000d;
    /// <summary>Gets 10^5.</summary>
    public static double Pow10_5  => 100000d;
    /// <summary>Gets 10^6.</summary>
    public static double Pow10_6  => 1000000d;
    /// <summary>Gets 10^7.</summary>
    public static double Pow10_7  => 10000000d;
    /// <summary>Gets 10^8.</summary>
    public static double Pow10_8  => 100000000d;
    /// <summary>Gets 10^9.</summary>
    public static double Pow10_9  => 1000000000d;
    /// <summary>Gets 10^10.</summary>
    public static double Pow10_10 => 10000000000d;
    /// <summary>Gets 10^11.</summary>
    public static double Pow10_11 => 100000000000d;
    /// <summary>Gets 10^12.</summary>
    public static double Pow10_12 => 1000000000000d;
    /// <summary>Gets 10^13.</summary>
    public static double Pow10_13 => 10000000000000d;
    /// <summary>Gets 10^14.</summary>
    public static double Pow10_14 => 100000000000000d;
    /// <summary>Gets 10^15.</summary>
    public static double Pow10_15 => 1000000000000000d;
    /// <summary>Gets 10^16.</summary>
    public static double Pow10_16 => 10000000000000000d;
    
    /// <summary>
    /// Gets the base-10 scale factor for a decimal-place count.
    /// </summary>
    /// <param name="decimalPlaces">
    /// The requested number of decimal places. Values are clamped to the inclusive range <c>0</c> to <c>16</c>.
    /// </param>
    /// <returns>
    /// A base-10 scale factor equivalent to <c>10^decimalPlaces</c> after clamping.
    /// </returns>
    public static double GetScaleFactor(int decimalPlaces)
    {
        decimalPlaces = Math.Clamp(decimalPlaces, 0, 16);
        return scaleTable[decimalPlaces];
    }

    /// <summary>
    /// Quantizes a floating-point value using the supplied base-10 scale factor.
    /// </summary>
    /// <param name="value">The value to quantize.</param>
    /// <param name="scale">The scale factor used before rounding.</param>
    /// <returns>
    /// The rounded integer-like value after scaling. Special floating-point values are mapped to sentinel values.
    /// </returns>
    public static long Quantize(float value, double scale)
    {
        if (float.IsNaN(value)) return long.MinValue;
        if (float.IsPositiveInfinity(value)) return long.MaxValue;
        if (float.IsNegativeInfinity(value)) return long.MinValue + 1;

        long quantized = (long)Math.Round(value * scale);
        return quantized == 0L ? 0L : quantized;
    }

    /// <summary>
    /// Compares two points by quantizing their coordinates with the supplied scale factor.
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <param name="scale">The scale factor used before rounding.</param>
    /// <returns><c>true</c> if both coordinates match after quantization; otherwise, <c>false</c>.</returns>
    public static bool QuantizedEquals(Vector2 a, Vector2 b, double scale)
    {
        return Quantize(a.X, scale) == Quantize(b.X, scale) &&
               Quantize(a.Y, scale) == Quantize(b.Y, scale);
    }

    /// <summary>
    /// Incorporates a quantized floating-point value into an FNV-1a 64-bit hash.
    /// </summary>
    /// <param name="hash">The current hash value.</param>
    /// <param name="value">The value to quantize and hash.</param>
    /// <param name="scale">The scale factor used before rounding.</param>
    /// <returns>The updated hash value.</returns>
    public static ulong HashQuantized(ulong hash, float value, double scale)
    {
        long quantized = Quantize(value, scale);

        unchecked
        {
            hash ^= (ulong)quantized;
            hash *= FnvPrime;
        }

        return hash;
    }
}