using ShapeEngine.Color;

namespace ShapeEngine.Core.Structs;

/// <summary>
/// Represents an inclusive range between two <see cref="ColorRgba"/> values.
/// </summary>
/// <remarks>
/// Provides utility methods for working with color ranges, including random color generation,
/// interpolation, immutable updates, equality checks, and stable hash creation.
/// </remarks>
public readonly struct ValueRangeColor : IEquatable<ValueRangeColor>
{
    #region Members
    
    /// <summary>
    /// The minimum color value of the range.
    /// </summary>
    public readonly ColorRgba Min;
    
    /// <summary>
    /// The maximum color value of the range.
    /// </summary>
    public readonly ColorRgba Max;
    
    #endregion

    #region Getters

    /// <summary>
    /// Gets the center color of the range by linearly interpolating halfway between <see cref="Min"/> and <see cref="Max"/>.
    /// </summary>
    public ColorRgba Center => Lerp(0.5f);

    /// <summary>
    /// Gets the red channel range.
    /// </summary>
    public ValueRangeInt RedRange => new(Min.R, Max.R);

    /// <summary>
    /// Gets the green channel range.
    /// </summary>
    public ValueRangeInt GreenRange => new(Min.G, Max.G);

    /// <summary>
    /// Gets the blue channel range.
    /// </summary>
    public ValueRangeInt BlueRange => new(Min.B, Max.B);

    /// <summary>
    /// Gets the alpha channel range.
    /// </summary>
    public ValueRangeInt AlphaRange => new(Min.A, Max.A);

    /// <summary>
    /// Gets the largest span among the four channel ranges.
    /// </summary>
    public int MaxRange => Math.Max(Math.Max(RedRange.TotalRange, GreenRange.TotalRange), Math.Max(BlueRange.TotalRange, AlphaRange.TotalRange));

    /// <summary>
    /// Gets the smallest span among the four channel ranges.
    /// </summary>
    public int MinRange => Math.Min(Math.Min(RedRange.TotalRange, GreenRange.TotalRange), Math.Min(BlueRange.TotalRange, AlphaRange.TotalRange));

    #endregion

    #region Constructors
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueRangeColor"/> struct with the specified minimum and maximum colors.
    /// </summary>
    /// <param name="min">The minimum color value.</param>
    /// <param name="max">The maximum color value.</param>
    public ValueRangeColor(ColorRgba min, ColorRgba max)
    {
        Min = min;
        Max = max;
    }
    
    #endregion
    
    #region Public Functions

    /// <summary>
    /// Determines whether any channel in the range has a non-zero span.
    /// </summary>
    /// <returns><see langword="true"/> if at least one channel range is non-zero; otherwise, <see langword="false"/>.</returns>
    public bool HasRange() => HasRgbRange() || HasAlphaRange();

    /// <summary>
    /// Determines whether any RGB channel in the range has a non-zero span.
    /// </summary>
    /// <returns><see langword="true"/> if the red, green, or blue channel range is non-zero; otherwise, <see langword="false"/>.</returns>
    public bool HasRgbRange() => RedRange.HasRange() || GreenRange.HasRange() || BlueRange.HasRange();

    /// <summary>
    /// Determines whether the alpha channel in the range has a non-zero span.
    /// </summary>
    /// <returns><see langword="true"/> if the alpha channel range is non-zero; otherwise, <see langword="false"/>.</returns>
    public bool HasAlphaRange() => AlphaRange.HasRange();

    /// <summary>
    /// Determines whether the specified color lies within this range for all channels.
    /// </summary>
    /// <param name="value">The color to test.</param>
    /// <returns><see langword="true"/> if all RGBA components of <paramref name="value"/> are within the corresponding channel ranges; otherwise, <see langword="false"/>.</returns>
    public bool Contains(ColorRgba value)
    {
        return value.R >= RedRange.Min && value.R <= RedRange.Max
                                       && value.G >= GreenRange.Min && value.G <= GreenRange.Max
                                       && value.B >= BlueRange.Min && value.B <= BlueRange.Max
                                       && value.A >= AlphaRange.Min && value.A <= AlphaRange.Max;
    }

    /// <summary>
    /// Returns a new <see cref="ValueRangeColor"/> updated to include <paramref name="value"/> in all channels.
    /// </summary>
    /// <param name="value">The color to include in the range.</param>
    /// <returns>A new <see cref="ValueRangeColor"/> that includes <paramref name="value"/>.</returns>
    public ValueRangeColor UpdateRange(ColorRgba value)
    {
        ColorRgba min = new(
            (byte)Math.Min(RedRange.Min, value.R),
            (byte)Math.Min(GreenRange.Min, value.G),
            (byte)Math.Min(BlueRange.Min, value.B),
            (byte)Math.Min(AlphaRange.Min, value.A)
        );

        ColorRgba max = new(
            (byte)Math.Max(RedRange.Max, value.R),
            (byte)Math.Max(GreenRange.Max, value.G),
            (byte)Math.Max(BlueRange.Max, value.B),
            (byte)Math.Max(AlphaRange.Max, value.A)
        );

        return new(min, max);
    }

    /// <summary>
    /// Clamps a color to this range on a per-channel basis.
    /// </summary>
    /// <param name="value">The color to clamp.</param>
    /// <returns>A new <see cref="ColorRgba"/> whose RGBA components are clamped to the corresponding channel ranges.</returns>
    public ColorRgba Clamp(ColorRgba value)
    {
        return new ColorRgba
        (
            ColorRgba.Clamp(value.R, (byte)RedRange.Min, (byte)RedRange.Max),
            ColorRgba.Clamp(value.G, (byte)GreenRange.Min, (byte)GreenRange.Max),
            ColorRgba.Clamp(value.B, (byte)BlueRange.Min, (byte)BlueRange.Max),
            ColorRgba.Clamp(value.A, (byte)AlphaRange.Min, (byte)AlphaRange.Max)
        );
    }

    /// <summary>
    /// Determines whether this color range overlaps with another <see cref="ValueRangeColor"/> on all channels.
    /// </summary>
    /// <param name="other">The other color range to check for overlap.</param>
    /// <returns><see langword="true"/> if all channel ranges overlap; otherwise, <see langword="false"/>.</returns>
    public bool OverlapValueRange(ValueRangeColor other)
    {
        return RedRange.OverlapValueRange(other.RedRange)
               && GreenRange.OverlapValueRange(other.GreenRange)
               && BlueRange.OverlapValueRange(other.BlueRange)
               && AlphaRange.OverlapValueRange(other.AlphaRange);
    }
    
    /// <summary>
    /// Returns a new <see cref="ValueRangeColor"/> with the specified minimum color and the same maximum color.
    /// </summary>
    /// <param name="min">The new minimum color.</param>
    /// <returns>A new <see cref="ValueRangeColor"/>.</returns>
    public ValueRangeColor SetMin(ColorRgba min) => new(min, Max);
    
    /// <summary>
    /// Returns a new <see cref="ValueRangeColor"/> with the specified maximum color and the same minimum color.
    /// </summary>
    /// <param name="max">The new maximum color.</param>
    /// <returns>A new <see cref="ValueRangeColor"/>.</returns>
    public ValueRangeColor SetMax(ColorRgba max) => new(Min, max);

    /// <summary>
    /// Returns a new <see cref="ValueRangeColor"/> with the specified red channel range.
    /// </summary>
    /// <param name="range">The new red channel range.</param>
    /// <returns>A new <see cref="ValueRangeColor"/>.</returns>
    public ValueRangeColor WithRedRange(ValueRangeInt range)
    {
        ColorRgba min = Min.SetRed((byte)range.Min);
        ColorRgba max = Max.SetRed((byte)range.Max);
        return new(min, max);
    }

    /// <summary>
    /// Returns a new <see cref="ValueRangeColor"/> with the specified green channel range.
    /// </summary>
    /// <param name="range">The new green channel range.</param>
    /// <returns>A new <see cref="ValueRangeColor"/>.</returns>
    public ValueRangeColor WithGreenRange(ValueRangeInt range)
    {
        ColorRgba min = Min.SetGreen((byte)range.Min);
        ColorRgba max = Max.SetGreen((byte)range.Max);
        return new(min, max);
    }

    /// <summary>
    /// Returns a new <see cref="ValueRangeColor"/> with the specified blue channel range.
    /// </summary>
    /// <param name="range">The new blue channel range.</param>
    /// <returns>A new <see cref="ValueRangeColor"/>.</returns>
    public ValueRangeColor WithBlueRange(ValueRangeInt range)
    {
        ColorRgba min = Min.SetBlue((byte)range.Min);
        ColorRgba max = Max.SetBlue((byte)range.Max);
        return new(min, max);
    }

    /// <summary>
    /// Returns a new <see cref="ValueRangeColor"/> with the specified alpha channel range.
    /// </summary>
    /// <param name="range">The new alpha channel range.</param>
    /// <returns>A new <see cref="ValueRangeColor"/>.</returns>
    public ValueRangeColor WithAlphaRange(ValueRangeInt range)
    {
        ColorRgba min = Min.SetAlpha((byte)range.Min);
        ColorRgba max = Max.SetAlpha((byte)range.Max);
        return new(min, max);
    }
    
    /// <summary>
    /// Returns a new <see cref="ValueRangeColor"/> with the specified minimum and maximum colors.
    /// </summary>
    /// <param name="min">The new minimum color.</param>
    /// <param name="max">The new maximum color.</param>
    /// <returns>A new <see cref="ValueRangeColor"/>.</returns>
    public ValueRangeColor Set(ColorRgba min, ColorRgba max) => new(min, max);
    
    /// <summary>
    /// Returns a random color whose channels are independently selected between <see cref="Min"/> and <see cref="Max"/>.
    /// </summary>
    /// <returns>A random <see cref="ColorRgba"/> within the inclusive color range.</returns>
    public ColorRgba Rand()
    {
        return new ColorRgba
        (
            RedRange.Rand(),
            GreenRange.Rand(),
            BlueRange.Rand(),
            AlphaRange.Rand()
        );
    }

    /// <summary>
    /// Linearly interpolates between <see cref="Min"/> and <see cref="Max"/> by the given factor.
    /// </summary>
    /// <param name="f">The interpolation factor, typically in the range [0, 1].</param>
    /// <returns>The interpolated <see cref="ColorRgba"/> value.</returns>
    public ColorRgba Lerp(float f) { return Min.Lerp(Max, f); }

    /// <summary>
    /// Linearly interpolates between <see cref="Min"/> and <see cref="Max"/> using the inverse factor <c>1 - f</c>.
    /// </summary>
    /// <param name="f">The interpolation factor, typically in the range [0, 1]. A value of 0 returns <see cref="Max"/> and 1 returns <see cref="Min"/>.</param>
    /// <returns>The interpolated <see cref="ColorRgba"/> value.</returns>
    public ColorRgba LerpInverse(float f) { return Min.Lerp(Max, 1f - f); }

    /// <summary>
    /// Deconstructs this color range into its minimum and maximum colors.
    /// </summary>
    /// <param name="min">Receives the minimum color.</param>
    /// <param name="max">Receives the maximum color.</param>
    public void Deconstruct(out ColorRgba min, out ColorRgba max)
    {
        min = Min;
        max = Max;
    }

    /// <summary>
    /// Deconstructs this color range into its per-channel ranges.
    /// </summary>
    /// <param name="redRange">Receives the red channel range.</param>
    /// <param name="greenRange">Receives the green channel range.</param>
    /// <param name="blueRange">Receives the blue channel range.</param>
    /// <param name="alphaRange">Receives the alpha channel range.</param>
    public void Deconstruct(out ValueRangeInt redRange, out ValueRangeInt greenRange, out ValueRangeInt blueRange, out ValueRangeInt alphaRange)
    {
        redRange = RedRange;
        greenRange = GreenRange;
        blueRange = BlueRange;
        alphaRange = AlphaRange;
    }

    #endregion
    
    #region Equality
    
    /// <summary>
    /// Determines whether the current <see cref="ValueRangeColor"/> is equal to another <see cref="ValueRangeColor"/>.
    /// </summary>
    /// <param name="other">The other color range to compare with.</param>
    /// <returns><see langword="true"/> if both <see cref="Min"/> and <see cref="Max"/> are equal; otherwise, <see langword="false"/>.</returns>
    public bool Equals(ValueRangeColor other)
    {
        return Min.Equals(other.Min) && Max.Equals(other.Max);
    }
    
    /// <summary>
    /// Determines whether the current <see cref="ValueRangeColor"/> is equal to another object.
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="ValueRangeColor"/> with equal bounds; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is ValueRangeColor other && Equals(other);
    }
    
    /// <summary>
    /// Returns a hash code for the current <see cref="ValueRangeColor"/> based on its stable quantized hash key.
    /// </summary>
    /// <remarks>
    /// This method folds the 64-bit value returned by <see cref="GetHashKey(int)"/> into a 32-bit hash code
    /// suitable for .NET hashing APIs such as dictionaries and hash sets.
    /// </remarks>
    /// <returns>A 32-bit hash code representing this color range.</returns>
    public override int GetHashCode()
    {
        ulong hashKey = GetHashKey();
        return unchecked((int)(hashKey ^ (hashKey >> 32)));
    }

    /// <summary>
    /// Creates a stable 64-bit hash key for this color range.
    /// </summary>
    /// <param name="decimalPlaces">The number of decimal places used to quantize channel values before hashing.</param>
    /// <returns>A 64-bit hash key suitable for cache keys and change detection.</returns>
    public ulong GetHashKey(int decimalPlaces = DecimalPrecision.DefaultDecimalPlaces)
    {
        if (decimalPlaces < 0) decimalPlaces = DecimalPrecision.DefaultDecimalPlaces;

        Fnv1aHashQuantizer hashQuantizer = new(decimalPlaces);
        ulong hash = hashQuantizer.StartHash(8);
        hash = hashQuantizer.Add(hash, Min.R);
        hash = hashQuantizer.Add(hash, Min.G);
        hash = hashQuantizer.Add(hash, Min.B);
        hash = hashQuantizer.Add(hash, Min.A);
        hash = hashQuantizer.Add(hash, Max.R);
        hash = hashQuantizer.Add(hash, Max.G);
        hash = hashQuantizer.Add(hash, Max.B);
        hash = hashQuantizer.Add(hash, Max.A);
        return hash;
    }

    /// <summary>
    /// Creates a fixed-width hexadecimal string representation of this color range hash key.
    /// </summary>
    /// <param name="decimalPlaces">The number of decimal places used to quantize channel values before hashing.</param>
    /// <returns>A 16-character uppercase hexadecimal hash key string.</returns>
    public string GetHashKeyHex(int decimalPlaces = DecimalPrecision.DefaultDecimalPlaces) => GetHashKey(decimalPlaces).ToString("X16");

    /// <summary>
    /// Creates a string representation of this color range hash key.
    /// </summary>
    /// <param name="decimalPlaces">The number of decimal places used to quantize channel values before hashing.</param>
    /// <returns>A stable hexadecimal hash key string.</returns>
    public string GetHashKeyString(int decimalPlaces = DecimalPrecision.DefaultDecimalPlaces) => GetHashKeyHex(decimalPlaces);
    
    /// <summary>
    /// Determines whether two <see cref="ValueRangeColor"/> instances are equal.
    /// </summary>
    /// <param name="left">The first color range to compare.</param>
    /// <param name="right">The second color range to compare.</param>
    /// <returns><see langword="true"/> if both instances are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(ValueRangeColor left, ValueRangeColor right)
    {
        return left.Equals(right);
    }
    
    /// <summary>
    /// Determines whether two <see cref="ValueRangeColor"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first color range to compare.</param>
    /// <param name="right">The second color range to compare.</param>
    /// <returns><see langword="true"/> if the instances are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(ValueRangeColor left, ValueRangeColor right)
    {
        return !(left == right);
    }
    
    #endregion
}