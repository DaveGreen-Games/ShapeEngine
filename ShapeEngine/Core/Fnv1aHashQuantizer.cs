using System.Numerics;

namespace ShapeEngine.Core;

/// <summary>
/// Provides stable 64-bit FNV-1a hashing for quantized floating-point and <see cref="Vector2"/> values.
/// </summary>
/// <remarks>
/// Values are first quantized through the configured <see cref="DecimalQuantizer"/>, then folded into
/// an FNV-1a hash. This allows callers to generate deterministic hashes that ignore insignificant
/// floating-point noise at the selected decimal precision.
/// </remarks>
public readonly struct Fnv1aHashQuantizer
{
    #region Member
    
    /// <summary>
    /// Gets the FNV-1a 64-bit offset basis used by the shared hash helpers.
    /// </summary>
    public const ulong FnvOffset = 14695981039346656037UL;

    /// <summary>
    /// Gets the FNV-1a 64-bit prime used by the shared hash helpers.
    /// </summary>
    public const ulong FnvPrime = 1099511628211UL;
    
    /// <summary>
    /// Gets a default hash quantizer using <see cref="DecimalPrecision.DefaultDecimalPlaces"/>.
    /// </summary>
    public static readonly Fnv1aHashQuantizer Default = new Fnv1aHashQuantizer(DecimalPrecision.DefaultDecimalPlaces);
    
    /// <summary>
    /// Gets the quantizer used to convert values into integer-like buckets before hashing.
    /// </summary>
    public readonly DecimalQuantizer Quantizer;
    
    #endregion
    
    #region Constructors
    
    /// <summary>
    /// Creates a new FNV-1a hash quantizer for the specified decimal precision.
    /// </summary>
    /// <param name="decimalPlaces">
    /// The number of decimal places used when quantizing values before they are hashed.
    /// </param>
    public Fnv1aHashQuantizer(int decimalPlaces = 4)
    {
        Quantizer = new DecimalQuantizer(decimalPlaces);
    }

    #endregion
    
    #region Start Hash / Add
    /// <summary>
    /// Starts a new FNV-1a hash seeded with the number of logical items that will be added.
    /// </summary>
    /// <param name="count">The number of logical values or elements that will contribute to the hash.</param>
    /// <returns>The initialized hash value.</returns>
    public ulong StartHash(int count)
    {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));

        var hash = FnvOffset;
        unchecked
        {
            hash ^= (ulong)count;
            hash *= FnvPrime;
        }
        return hash;
    }

    /// <summary>
    /// Adds a quantized <see cref="float"/> value to an in-progress hash.
    /// </summary>
    /// <param name="currentHash">The current hash value.</param>
    /// <param name="value">The value to quantize and add.</param>
    /// <returns>The updated hash value.</returns>
    public ulong Add(ulong currentHash, float value)
    {
        return HashQuantized(currentHash, value);
    }

    /// <summary>
    /// Adds a quantized <see cref="double"/> value to an in-progress hash.
    /// </summary>
    /// <param name="currentHash">The current hash value.</param>
    /// <param name="value">The value to quantize and add.</param>
    /// <returns>The updated hash value.</returns>
    public ulong Add(ulong currentHash, double value)
    {
        return HashQuantized(currentHash, value);
    }

    /// <summary>
    /// Adds a quantized <see cref="Vector2"/> value to an in-progress hash by hashing its X and Y components.
    /// </summary>
    /// <param name="currentHash">The current hash value.</param>
    /// <param name="value">The vector to quantize and add.</param>
    /// <returns>The updated hash value.</returns>
    public ulong Add(ulong currentHash, Vector2 value)
    {
        currentHash = Add(currentHash, value.X);
        currentHash = Add(currentHash, value.Y);
        return currentHash;
    }
    #endregion
    
    #region Hash Quantized
    
    /// <summary>
    /// Quantizes a <see cref="float"/> value and folds it into the supplied FNV-1a hash.
    /// </summary>
    /// <param name="hash">The current hash value.</param>
    /// <param name="value">The value to quantize and hash.</param>
    /// <returns>The updated hash value.</returns>
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
    
    /// <summary>
    /// Quantizes a <see cref="double"/> value and folds it into the supplied FNV-1a hash.
    /// </summary>
    /// <param name="hash">The current hash value.</param>
    /// <param name="value">The value to quantize and hash.</param>
    /// <returns>The updated hash value.</returns>
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

    /// <summary>
    /// Quantizes a <see cref="Vector2"/> value and folds both components into the supplied FNV-1a hash.
    /// </summary>
    /// <param name="hash">The current hash value.</param>
    /// <param name="value">The vector to quantize and hash.</param>
    /// <returns>The updated hash value.</returns>
    public ulong HashQuantized(ulong hash, Vector2 value)
    {
        hash = HashQuantized(hash, value.X);
        hash = HashQuantized(hash, value.Y);
        return hash;
    }
    
    #endregion
    
    #region Hash Float
    
    /// <summary>
    /// Creates a stable hash for a single <see cref="float"/> value.
    /// </summary>
    public ulong GetHash(float value)
    {
        ulong hash = StartHash(1);
        hash = Add(hash, value);
        return hash;
    }

    /// <summary>
    /// Creates a stable hash for two <see cref="float"/> values in the given order.
    /// </summary>
    public ulong GetHash(float valueA, float valueB)
    {
        ulong hash = StartHash(2);
        hash = Add(hash, valueA);
        hash = Add(hash, valueB);
        return hash;
    }

    /// <summary>
    /// Creates a stable hash for three <see cref="float"/> values in the given order.
    /// </summary>
    public ulong GetHash(float valueA, float valueB, float valueC)
    {
        ulong hash = StartHash(3);
        hash = Add(hash, valueA);
        hash = Add(hash, valueB);
        hash = Add(hash, valueC);
        return hash;
    }

    /// <summary>
    /// Creates a stable hash for four <see cref="float"/> values in the given order.
    /// </summary>
    public ulong GetHash(float valueA, float valueB, float valueC, float valueD)
    {
        ulong hash = StartHash(4);
        hash = Add(hash, valueA);
        hash = Add(hash, valueB);
        hash = Add(hash, valueC);
        hash = Add(hash, valueD);
        return hash;
    }

    /// <summary>
    /// Creates a stable hash for a sequence of <see cref="float"/> values in enumeration order.
    /// </summary>
    /// <param name="values">The values to hash.</param>
    public ulong GetHash(IReadOnlyList<float> values)
    {
        if (values == null) throw new ArgumentNullException(nameof(values));

        ulong hash = StartHash(values.Count);
        for (int i = 0; i < values.Count; i++)
        {
            hash = Add(hash, values[i]);
        }
        return hash;
    }

    #endregion

    #region Hash Double
    
    /// <summary>
    /// Creates a stable hash for a single <see cref="double"/> value.
    /// </summary>
    public ulong GetHash(double value)
    {
        ulong hash = StartHash(1);
        hash = Add(hash, value);
        return hash;
    }

    /// <summary>
    /// Creates a stable hash for two <see cref="double"/> values in the given order.
    /// </summary>
    public ulong GetHash(double valueA, double valueB)
    {
        ulong hash = StartHash(2);
        hash = Add(hash, valueA);
        hash = Add(hash, valueB);
        return hash;
    }

    /// <summary>
    /// Creates a stable hash for three <see cref="double"/> values in the given order.
    /// </summary>
    public ulong GetHash(double valueA, double valueB, double valueC)
    {
        ulong hash = StartHash(3);
        hash = Add(hash, valueA);
        hash = Add(hash, valueB);
        hash = Add(hash, valueC);
        return hash;
    }

    /// <summary>
    /// Creates a stable hash for four <see cref="double"/> values in the given order.
    /// </summary>
    public ulong GetHash(double valueA, double valueB, double valueC, double valueD)
    {
        ulong hash = StartHash(4);
        hash = Add(hash, valueA);
        hash = Add(hash, valueB);
        hash = Add(hash, valueC);
        hash = Add(hash, valueD);
        return hash;
    }

    /// <summary>
    /// Creates a stable hash for a sequence of <see cref="double"/> values in enumeration order.
    /// </summary>
    /// <param name="values">The values to hash.</param>
    public ulong GetHash(IReadOnlyList<double> values)
    {
        if (values == null) throw new ArgumentNullException(nameof(values));

        ulong hash = StartHash(values.Count);
        for (int i = 0; i < values.Count; i++)
        {
            hash = Add(hash, values[i]);
        }
        return hash;
    }

    #endregion

    #region Hash Vector2
    
    /// <summary>
    /// Creates a stable hash for a single <see cref="Vector2"/> value.
    /// </summary>
    public ulong GetHash(Vector2 value)
    {
        ulong hash = StartHash(1);
        hash = Add(hash, value);
        return hash;
    }

    /// <summary>
    /// Creates a stable hash for two <see cref="Vector2"/> values in the given order.
    /// </summary>
    public ulong GetHash(Vector2 valueA, Vector2 valueB)
    {
        ulong hash = StartHash(2);
        hash = Add(hash, valueA);
        hash = Add(hash, valueB);
        return hash;
    }

    /// <summary>
    /// Creates a stable hash for three <see cref="Vector2"/> values in the given order.
    /// </summary>
    public ulong GetHash(Vector2 valueA, Vector2 valueB, Vector2 valueC)
    {
        ulong hash = StartHash(3);
        hash = Add(hash, valueA);
        hash = Add(hash, valueB);
        hash = Add(hash, valueC);
        return hash;
    }

    /// <summary>
    /// Creates a stable hash for four <see cref="Vector2"/> values in the given order.
    /// </summary>
    public ulong GetHash(Vector2 valueA, Vector2 valueB, Vector2 valueC, Vector2 valueD)
    {
        ulong hash = StartHash(4);
        hash = Add(hash, valueA);
        hash = Add(hash, valueB);
        hash = Add(hash, valueC);
        hash = Add(hash, valueD);
        return hash;
    }

    /// <summary>
    /// Creates a stable hash for a sequence of <see cref="Vector2"/> values in enumeration order.
    /// </summary>
    /// <param name="values">The values to hash.</param>
    public ulong GetHash(IReadOnlyList<Vector2> values)
    {
        if (values == null) throw new ArgumentNullException(nameof(values));

        ulong hash = StartHash(values.Count);
        for (int i = 0; i < values.Count; i++)
        {
            hash = Add(hash, values[i]);
        }
        return hash;
    }
    
    #endregion
    
    #region Private Methods
    
    private DecimalQuantizer GetEffectiveQuantizer()
    {
        return Quantizer.Precision.Scale > 0.0 ? Quantizer : DecimalQuantizer.Default;
    }
    
    #endregion
}