namespace ShapeEngine.Core.Structs;

/// <summary>
/// Represents a float value constrained to the <c>[0, 1]</c> range.
/// </summary>
public readonly struct NormalizedFloat : IEquatable<NormalizedFloat>
{
    /// <summary>
    /// Represents the normalized float value 0.
    /// </summary>
    public static readonly NormalizedFloat Zero = new(0f);
    
    /// <summary>
    /// Represents the normalized float value 1.
    /// </summary>
    public static readonly NormalizedFloat One = new(1f);
    
    /// <summary>
    /// Initializes a new instance of <see cref="NormalizedFloat"/>, clamping the value to <c>[0, 1]</c>.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    public NormalizedFloat(float value)
    {
        this.Value = Math.Clamp(value, 0f, 1f);
    }

    /// <summary>
    /// Gets the normalized value in the range <c>[0, 1]</c>.
    /// </summary>
    public float Value { get; }
    
    /// <summary>
    /// Gets the inverse of the normalized value, i.e., <c>1 - Value</c>.
    /// </summary>
    public NormalizedFloat Inverse => new(1f - Value);
    
    /// <summary>
    /// Returns a <see cref="NormalizedFloat"/> representing the minimum of this value and another.
    /// </summary>
    /// <param name="other">The other <see cref="NormalizedFloat"/> to compare with.</param>
    /// <returns>The minimum value as a <see cref="NormalizedFloat"/>.</returns>
    public NormalizedFloat Min(NormalizedFloat other) => new(MathF.Min(Value, other.Value));
    
    /// <summary>
    /// Returns a <see cref="NormalizedFloat"/> representing the maximum of this value and another.
    /// </summary>
    /// <param name="other">The other <see cref="NormalizedFloat"/> to compare with.</param>
    /// <returns>The maximum value as a <see cref="NormalizedFloat"/>.</returns>
    public NormalizedFloat Max(NormalizedFloat other) => new(MathF.Max(Value, other.Value));

    /// <summary>
    /// Linearly interpolates between two float values using a normalized interpolation factor.
    /// </summary>
    /// <param name="a">The start value.</param>
    /// <param name="b">The end value.</param>
    /// <param name="t">The interpolation factor, constrained to <c>[0, 1]</c>.</param>
    /// <returns>The interpolated float value.</returns>
    public static float Lerp(float a, float b, NormalizedFloat t) => a + (b - a) * t.Value;

    /// <summary>
    /// Calculates the interpolation factor between two values for a given value.
    /// </summary>
    /// <param name="a">The start value.</param>
    /// <param name="b">The end value.</param>
    /// <param name="value">The value to find the interpolation factor for.</param>
    /// <returns>The normalized interpolation factor in the range <c>[0, 1]</c>.</returns>
    public static NormalizedFloat InverseLerp(float a, float b, float value) => new((value - a) / (b - a));
    

    /// <summary>
    /// Adds two <see cref="NormalizedFloat"/> values, clamping the result.
    /// </summary>
    public static NormalizedFloat operator +(NormalizedFloat a, NormalizedFloat b) => new(a.Value + b.Value);

    /// <summary>
    /// Subtracts one <see cref="NormalizedFloat"/> from another, clamping the result.
    /// </summary>
    public static NormalizedFloat operator -(NormalizedFloat a, NormalizedFloat b) => new(a.Value - b.Value);
    
    /// <summary>
    /// Multiplies two <see cref="NormalizedFloat"/> values, clamping the result to <c>[0, 1]</c>.
    /// </summary>
    public static NormalizedFloat operator *(NormalizedFloat a, NormalizedFloat b) => new(a.Value * b.Value);
    
    /// <summary>
    /// Divides one <see cref="NormalizedFloat"/> by another, clamping the result to <c>[0, 1]</c>. Returns 0 if the divisor is less than or equal to 0.
    /// </summary>
    public static NormalizedFloat operator /(NormalizedFloat a, NormalizedFloat b) => b.Value <= 0f ? new(0f) : new(a.Value / b.Value);
    
    /// <summary>
    /// Adds a float and a <see cref="NormalizedFloat"/>, clamping the result to <c>[0, 1]</c>.
    /// </summary>
    public static NormalizedFloat operator +(float a, NormalizedFloat b) => new(a + b.Value);
    
    /// <summary>
    /// Subtracts a <see cref="NormalizedFloat"/> from a float, clamping the result to <c>[0, 1]</c>.
    /// </summary>
    public static NormalizedFloat operator -(float a, NormalizedFloat b) => new(a - b.Value);
    
    /// <summary>
    /// Multiplies a float by a <see cref="NormalizedFloat"/> and divides by the normalized value, clamping the result to <c>[0, 1]</c>. Returns 0 if the divisor is less than or equal to 0.
    /// </summary>
    public static NormalizedFloat operator /(float a, NormalizedFloat b) => b.Value <= 0 ? new(0f) : new(a * b.Value);
    
    /// <summary>
    /// Multiplies a float by a <see cref="NormalizedFloat"/>, clamping the result.
    /// </summary>
    public static NormalizedFloat operator *(float a, NormalizedFloat b) => new(a * b.Value);
    
    /// <summary>
    /// Adds a <see cref="NormalizedFloat"/> and a float, clamping the result to <c>[0, 1]</c>.
    /// </summary>
    public static NormalizedFloat operator +(NormalizedFloat a, float b) => new(a.Value + b);
    
    /// <summary>
    /// Subtracts a float from a <see cref="NormalizedFloat"/>, clamping the result to <c>[0, 1]</c>.
    /// </summary>
    public static NormalizedFloat operator -(NormalizedFloat a, float b) => new(a.Value - b);
    
    /// <summary>
    /// Multiplies a <see cref="NormalizedFloat"/> by a float, clamping the result.
    /// </summary>
    public static NormalizedFloat operator *(NormalizedFloat a, float b) => new(a.Value * b);
    
    /// <summary>
    /// Divides a <see cref="NormalizedFloat"/> by a float, clamping the result.
    /// </summary>
    public static NormalizedFloat operator /(NormalizedFloat a, float b) => new(a.Value / b);
    
    
    /// <summary>
    /// Explicitly converts a float to a <see cref="NormalizedFloat"/>, clamping to <c>[0, 1]</c>.
    /// </summary>
    public static explicit operator NormalizedFloat(float value) => new(value);

    /// <summary>
    /// Implicitly converts a <see cref="NormalizedFloat"/> to a float.
    /// </summary>
    public static implicit operator float(NormalizedFloat normalized) => normalized.Value;
    
    /// <summary>
    /// Explicitly converts a <see cref="NormalizedFloat"/> to a <see cref="NormalizedDouble"/>, preserving the normalized value.
    /// </summary>
    public static explicit operator NormalizedDouble(NormalizedFloat value) => new(value.Value);
    
    /// <summary>
    /// Explicitly converts a <see cref="NormalizedFloat"/> to a <see cref="SignedNormalizedFloat"/>, mapping [0, 1] to [-1, 1].
    /// </summary>
    public static explicit operator SignedNormalizedFloat(NormalizedFloat value) => new(value.Value * 2f - 1f);
    
    /// <summary>
    /// Explicitly converts a <see cref="NormalizedFloat"/> to a <see cref="SignedNormalizedDouble"/>, mapping [0, 1] to [-1, 1].
    /// </summary>
    public static explicit operator SignedNormalizedDouble(NormalizedFloat value) => new(value.Value * 2.0 - 1.0);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("F2");

    /// <summary>
    /// Determines whether the specified <see cref="NormalizedFloat"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="NormalizedFloat"/> to compare with the current instance.</param>
    /// <returns><c>true</c> if the values are equal; otherwise, <c>false</c>.</returns>
    public bool Equals(NormalizedFloat other)
    {
        return Value.Equals(other.Value);
    }
    
    /// <summary>
    /// Determines whether the specified object is equal to the current <see cref="NormalizedFloat"/>.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><c>true</c> if the specified object is a <see cref="NormalizedFloat"/> and is equal to the current instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is NormalizedFloat other && Equals(other);
    }
    
    /// <summary>
    /// Returns a hash code for the current <see cref="NormalizedFloat"/>.
    /// </summary>
    /// <returns>A hash code for the current instance.</returns>
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    /// <summary>
    /// Determines whether two <see cref="NormalizedFloat"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="NormalizedFloat"/> to compare.</param>
    /// <param name="right">The second <see cref="NormalizedFloat"/> to compare.</param>
    /// <returns><c>true</c> if the values are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(NormalizedFloat left, NormalizedFloat right)
    {
        return left.Equals(right);
    }
    
    /// <summary>
    /// Determines whether two <see cref="NormalizedFloat"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="NormalizedFloat"/> to compare.</param>
    /// <param name="right">The second <see cref="NormalizedFloat"/> to compare.</param>
    /// <returns><c>true</c> if the values are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(NormalizedFloat left, NormalizedFloat right)
    {
        return !(left == right);
    }
}