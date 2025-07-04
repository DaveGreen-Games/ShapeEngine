namespace ShapeEngine.Core.Structs;

/// <summary>
/// Represents a double value normalized to the range <c>[-1, 1]</c> with sign.
/// Implements <see cref="IEquatable{SignedNormalizedDouble}"/> for value equality.
/// </summary>
public readonly struct SignedNormalizedDouble : IEquatable<SignedNormalizedDouble>
{
        /// <summary>
    /// Initializes a new instance of <see cref="SignedNormalizedDouble"/>, clamping the value to <c>[0, 1]</c>.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    public SignedNormalizedDouble(double value)
    {
        Value = Math.Clamp(value, -1f, 1f);
    }

    /// <summary>
    /// Gets the signed normalized value in the range <c>[0, 1]</c>.
    /// </summary>
    public double Value { get; }

  
    /// <summary>
    /// Linearly interpolates between two double values using a normalized interpolation factor.
    /// </summary>
    /// <param name="a">The start value.</param>
    /// <param name="b">The end value.</param>
    /// <param name="t">The interpolation factor, typically in the range [0, 1].</param>
    /// <returns>The interpolated double value.</returns>
    public static double Lerp(double a, double b, NormalizedFloat t) => a + (b - a) * t.Value;

    /// <summary>
    /// Calculates the interpolation factor between two values for a given value.
    /// </summary>
    public static SignedNormalizedDouble InverseLerp(double a, double b, double value)
        => new((value - a) / (b - a));

    /// <summary>
    /// Explicitly converts a double to a <see cref="SignedNormalizedDouble"/>, clamping to <c>[0, 1]</c>.
    /// </summary>
    public static explicit operator SignedNormalizedDouble(double value) => new(value);

    /// <summary>
    /// Implicitly converts a <see cref="SignedNormalizedDouble"/> to a double.
    /// </summary>
    public static implicit operator double(SignedNormalizedDouble normalized) => normalized.Value;

    /// <summary>
    /// Adds two <see cref="SignedNormalizedDouble"/> values, clamping the result.
    /// </summary>
    public static SignedNormalizedDouble operator +(SignedNormalizedDouble a, SignedNormalizedDouble b)
        => new(a.Value + b.Value);

    /// <summary>
    /// Subtracts one <see cref="SignedNormalizedDouble"/> from another, clamping the result.
    /// </summary>
    public static SignedNormalizedDouble operator -(SignedNormalizedDouble a, SignedNormalizedDouble b)
        => new(a.Value - b.Value);

    /// <summary>
    /// Multiplies a <see cref="SignedNormalizedDouble"/> by a double, clamping the result.
    /// </summary>
    public static SignedNormalizedDouble operator *(SignedNormalizedDouble a, double b)
        => new(a.Value * b);

    /// <summary>
    /// Multiplies a double by a <see cref="SignedNormalizedDouble"/>, clamping the result.
    /// </summary>
    public static SignedNormalizedDouble operator *(double a, SignedNormalizedDouble b)
        => new(a * b.Value);

    /// <summary>
    /// Divides a <see cref="SignedNormalizedDouble"/> by a double, clamping the result.
    /// </summary>
    public static SignedNormalizedDouble operator /(SignedNormalizedDouble a, double b)
        => new(a.Value / b);

    /// <summary>
    /// Explicitly converts a <see cref="SignedNormalizedDouble"/> to a <see cref="NormalizedDouble"/>.
    /// The conversion maps the range [-1, 1] to [0, 1].
    /// </summary>
    public static explicit operator NormalizedDouble(SignedNormalizedDouble value) => new((value.Value + 1.0) / 2.0);
    
    /// <summary>
    /// Explicitly converts a <see cref="SignedNormalizedDouble"/> to a <see cref="NormalizedFloat"/>.
    /// The conversion maps the range [-1, 1] to [0, 1] as a float.
    /// </summary>
    public static explicit operator NormalizedFloat(SignedNormalizedDouble value) => new((float)((value.Value + 1.0) / 2.0));
    
    /// <summary>
    /// Explicitly converts a <see cref="SignedNormalizedDouble"/> to a <see cref="SignedNormalizedFloat"/>.
    /// The conversion casts the value to float, preserving the range [-1, 1].
    /// </summary>
    public static explicit operator SignedNormalizedFloat(SignedNormalizedDouble value) => new((float)value.Value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("F2");

    /// <summary>
    /// Determines whether the specified <see cref="SignedNormalizedDouble"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="SignedNormalizedDouble"/> to compare with the current instance.</param>
    /// <returns><c>true</c> if the values are equal; otherwise, <c>false</c>.</returns>
    public bool Equals(SignedNormalizedDouble other)
    {
        return Value.Equals(other.Value);
    }
    
    /// <summary>
    /// Determines whether the specified object is equal to the current instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><c>true</c> if the object is a <see cref="SignedNormalizedDouble"/> and the values are equal; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is SignedNormalizedDouble other && Equals(other);
    }
    
    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A hash code for the current instance.</returns>
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    /// <summary>
    /// Determines whether two <see cref="SignedNormalizedDouble"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="SignedNormalizedDouble"/> to compare.</param>
    /// <param name="right">The second <see cref="SignedNormalizedDouble"/> to compare.</param>
    /// <returns><c>true</c> if the values are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(SignedNormalizedDouble left, SignedNormalizedDouble right)
    {
        return left.Equals(right);
    }
    
    /// <summary>
    /// Determines whether two <see cref="SignedNormalizedDouble"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="SignedNormalizedDouble"/> to compare.</param>
    /// <param name="right">The second <see cref="SignedNormalizedDouble"/> to compare.</param>
    /// <returns><c>true</c> if the values are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(SignedNormalizedDouble left, SignedNormalizedDouble right)
    {
        return !(left == right);
    }
}