namespace ShapeEngine.Core.Structs;

/// <summary>
/// Represents a double value normalized to the range <c>[0, 1]</c>.
/// </summary>
public readonly struct NormalizedDouble  : IEquatable<NormalizedDouble>
{
    /// <summary>
    /// Initializes a new instance of <see cref="NormalizedDouble"/>, clamping the value to <c>[0, 1]</c>.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    public NormalizedDouble(double value)
    {
        Value = Math.Clamp(value, 0.0, 1.0);
    }

    /// <summary>
    /// Gets the normalized value in the range <c>[0, 1]</c>.
    /// </summary>
    public double Value { get; }
    
    /// <summary>
    /// Gets the inverse of the normalized value, i.e., <c>1.0 - Value</c>.
    /// </summary>
    public NormalizedDouble Inverse => new(1.0 - Value);

    /// <summary>
    /// Linearly interpolates between two double values using a normalized interpolation factor.
    /// </summary>
    /// <param name="a">The start value.</param>
    /// <param name="b">The end value.</param>
    /// <param name="t">The interpolation factor, constrained to <c>[0, 1]</c>.</param>
    /// <returns>The interpolated double value.</returns>
    public static double Lerp(double a, double b, NormalizedDouble t) => a + (b - a) * t.Value;

    /// <summary>
    /// Calculates the interpolation factor between two values for a given value.
    /// </summary>
    public static NormalizedDouble InverseLerp(double a, double b, double value) => new((value - a) / (b - a));
    

    /// <summary>
    /// Adds two <see cref="NormalizedDouble"/> values, clamping the result.
    /// </summary>
    public static NormalizedDouble operator +(NormalizedDouble a, NormalizedDouble b) => new(a.Value + b.Value);

    /// <summary>
    /// Subtracts one <see cref="NormalizedDouble"/> from another, clamping the result.
    /// </summary>
    public static NormalizedDouble operator -(NormalizedDouble a, NormalizedDouble b) => new(a.Value - b.Value);
    
    /// <summary>
    /// Multiplies two <see cref="NormalizedDouble"/> values, clamping the result to <c>[0, 1]</c>.
    /// </summary>
    public static NormalizedDouble operator *(NormalizedDouble a, NormalizedDouble b) => new(a.Value * b.Value);
    
    /// <summary>
    /// Divides one <see cref="NormalizedDouble"/> by another, clamping the result to <c>[0, 1]</c>. Returns 0 if the divisor is less than or equal to 0.
    /// </summary>
    public static NormalizedDouble operator /(NormalizedDouble a, NormalizedDouble b) => b.Value <= 0f ? new(0f) : new(a.Value / b.Value);
    
    /// <summary>
    /// Adds a double and a <see cref="NormalizedDouble"/>, clamping the result to <c>[0, 1]</c>.
    /// </summary>
    public static NormalizedDouble operator +(double a, NormalizedDouble b) => new(a + b.Value);
    
    /// <summary>
    /// Subtracts a <see cref="NormalizedDouble"/> from a double, clamping the result to <c>[0, 1]</c>.
    /// </summary>
    public static NormalizedDouble operator -(double a, NormalizedDouble b) => new(a - b.Value);
    
    /// <summary>
    /// Multiplies a double by a <see cref="NormalizedDouble"/> and divides by the normalized value, clamping the result to <c>[0, 1]</c>. Returns 0 if the divisor is less than or equal to 0.
    /// </summary>
    public static NormalizedDouble operator /(double a, NormalizedDouble b) => b.Value <= 0 ? new(0f) : new(a * b.Value);
    
    /// <summary>
    /// Multiplies a double by a <see cref="NormalizedDouble"/>, clamping the result.
    /// </summary>
    public static NormalizedDouble operator *(double a, NormalizedDouble b) => new(a * b.Value);
    
    /// <summary>
    /// Adds a <see cref="NormalizedDouble"/> and a double, clamping the result to <c>[0, 1]</c>.
    /// </summary>
    public static NormalizedDouble operator +(NormalizedDouble a, double b) => new(a.Value + b);
    
    /// <summary>
    /// Subtracts a double from a <see cref="NormalizedDouble"/>, clamping the result to <c>[0, 1]</c>.
    /// </summary>
    public static NormalizedDouble operator -(NormalizedDouble a, double b) => new(a.Value - b);
    
    /// <summary>
    /// Multiplies a <see cref="NormalizedDouble"/> by a double, clamping the result.
    /// </summary>
    public static NormalizedDouble operator *(NormalizedDouble a, double b) => new(a.Value * b);
    
    /// <summary>
    /// Divides a <see cref="NormalizedDouble"/> by a double, clamping the result.
    /// </summary>
    public static NormalizedDouble operator /(NormalizedDouble a, double b) => new(a.Value / b);

    
    /// <inheritdoc/>
    public override string ToString() => Value.ToString("F4");

    /// <summary>
    /// Determines whether the specified <see cref="NormalizedDouble"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="NormalizedDouble"/> to compare with the current instance.</param>
    /// <returns><c>true</c> if the values are equal; otherwise, <c>false</c>.</returns>
    public bool Equals(NormalizedDouble other)
    {
        return Value.Equals(other.Value);
    }
    
    /// <summary>
    /// Determines whether the specified object is equal to the current <see cref="NormalizedDouble"/>.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><c>true</c> if the specified object is a <see cref="NormalizedDouble"/> and is equal to the current instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is NormalizedDouble other && Equals(other);
    }
    
    /// <summary>
    /// Returns a hash code for the current <see cref="NormalizedDouble"/>.
    /// </summary>
    /// <returns>A hash code for the current instance.</returns>
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    
    /// <summary>
    /// Explicitly converts a double to a <see cref="NormalizedDouble"/>, clamping to <c>[0, 1]</c>.
    /// </summary>
    public static explicit operator NormalizedDouble(double value) => new(value);

    /// <summary>
    /// Implicitly converts a <see cref="NormalizedDouble"/> to a double.
    /// </summary>
    public static implicit operator double(NormalizedDouble normalized) => normalized.Value;
    
    /// <summary>
    /// Explicitly converts a <see cref="NormalizedDouble"/> to a <see cref="NormalizedFloat"/>, clamping to <c>[0, 1]</c>.
    /// </summary>
    /// <param name="value">The <see cref="NormalizedDouble"/> value to convert.</param>
    /// <returns>A <see cref="NormalizedFloat"/> representing the same normalized value.</returns>
    public static explicit operator NormalizedFloat(NormalizedDouble value) => new((float)value.Value);
    
    /// <summary>
    /// Explicitly converts a <see cref="NormalizedDouble"/> to a <see cref="SignedNormalizedDouble"/>, mapping <c>[0, 1]</c> to <c>[-1, 1]</c>.
    /// </summary>
    /// <param name="value">The <see cref="NormalizedDouble"/> value to convert.</param>
    /// <returns>A <see cref="SignedNormalizedDouble"/> representing the mapped value.</returns>
    public static explicit operator SignedNormalizedDouble(NormalizedDouble value) => new(value.Value * 2.0 - 1.0);
    
    /// <summary>
    /// Explicitly converts a <see cref="NormalizedDouble"/> to a <see cref="SignedNormalizedFloat"/>, mapping <c>[0, 1]</c> to <c>[-1, 1]</c>.
    /// </summary>
    /// <param name="value">The <see cref="NormalizedDouble"/> value to convert.</param>
    /// <returns>A <see cref="SignedNormalizedFloat"/> representing the mapped value.</returns>
    public static explicit operator SignedNormalizedFloat(NormalizedDouble value) => new((float)(value.Value * 2.0 - 1.0));

    /// <summary>
    /// Determines whether two <see cref="NormalizedDouble"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="NormalizedDouble"/> to compare.</param>
    /// <param name="right">The second <see cref="NormalizedDouble"/> to compare.</param>
    /// <returns><c>true</c> if the values are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(NormalizedDouble left, NormalizedDouble right)
    {
        return left.Equals(right);
    }
    
    /// <summary>
    /// Determines whether two <see cref="NormalizedDouble"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="NormalizedDouble"/> to compare.</param>
    /// <param name="right">The second <see cref="NormalizedDouble"/> to compare.</param>
    /// <returns><c>true</c> if the values are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(NormalizedDouble left, NormalizedDouble right)
    {
        return !(left == right);
    }
}