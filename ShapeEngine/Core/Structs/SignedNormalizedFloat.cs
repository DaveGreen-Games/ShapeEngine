namespace ShapeEngine.Core.Structs;

/// <summary>
/// Represents a float value constrained to the <c>[0, 1]</c> range.
/// </summary>
public readonly struct SignedNormalizedFloat : IEquatable<SignedNormalizedFloat>
{
    /// <summary>
    /// Initializes a new instance of <see cref="SignedNormalizedFloat"/>, clamping the value to <c>[0, 1]</c>.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    public SignedNormalizedFloat(float value)
    {
        Value = Math.Clamp(value, -1f, 1f);
    }

    /// <summary>
    /// Gets the signed normalized value in the range <c>[0, 1]</c>.
    /// </summary>
    public float Value { get; }

  
    /// <summary>
    /// Linearly interpolates between two float values using a normalized interpolation factor.
    /// </summary>
    /// <param name="a">The start value.</param>
    /// <param name="b">The end value.</param>
    /// <param name="t">The interpolation factor, typically in the range [0, 1].</param>
    /// <returns>The interpolated float value.</returns>
    public static float Lerp(float a, float b, NormalizedFloat t) => a + (b - a) * t.Value;

    /// <summary>
    /// Calculates the interpolation factor between two values for a given value.
    /// </summary>
    public static SignedNormalizedFloat InverseLerp(float a, float b, float value)
        => new((value - a) / (b - a));

    /// <summary>
    /// Explicitly converts a float to a <see cref="SignedNormalizedFloat"/>, clamping to <c>[0, 1]</c>.
    /// </summary>
    public static explicit operator SignedNormalizedFloat(float value) => new(value);

    /// <summary>
    /// Implicitly converts a <see cref="SignedNormalizedFloat"/> to a float.
    /// </summary>
    public static implicit operator float(SignedNormalizedFloat normalized) => normalized.Value;

    /// <summary>
    /// Adds two <see cref="SignedNormalizedFloat"/> values, clamping the result.
    /// </summary>
    public static SignedNormalizedFloat operator +(SignedNormalizedFloat a, SignedNormalizedFloat b)
        => new(a.Value + b.Value);

    /// <summary>
    /// Subtracts one <see cref="SignedNormalizedFloat"/> from another, clamping the result.
    /// </summary>
    public static SignedNormalizedFloat operator -(SignedNormalizedFloat a, SignedNormalizedFloat b)
        => new(a.Value - b.Value);

    /// <summary>
    /// Multiplies a <see cref="SignedNormalizedFloat"/> by a float, clamping the result.
    /// </summary>
    public static SignedNormalizedFloat operator *(SignedNormalizedFloat a, float b)
        => new(a.Value * b);

    /// <summary>
    /// Multiplies a float by a <see cref="SignedNormalizedFloat"/>, clamping the result.
    /// </summary>
    public static SignedNormalizedFloat operator *(float a, SignedNormalizedFloat b)
        => new(a * b.Value);

    /// <summary>
    /// Divides a <see cref="SignedNormalizedFloat"/> by a float, clamping the result.
    /// </summary>
    public static SignedNormalizedFloat operator /(SignedNormalizedFloat a, float b)
        => new(a.Value / b);

   /// <summary>
   /// Explicitly converts a <see cref="SignedNormalizedFloat"/> to a <see cref="NormalizedDouble"/>.
   /// The value is mapped from \[-1, 1\] to \[0, 1\].
   /// </summary>
   public static explicit operator NormalizedDouble(SignedNormalizedFloat value) => new((value.Value + 1.0) / 2.0);
   
   /// <summary>
   /// Explicitly converts a <see cref="SignedNormalizedFloat"/> to a <see cref="NormalizedFloat"/>.
   /// The value is mapped from \[-1, 1\] to \[0, 1\].
   /// </summary>
   public static explicit operator NormalizedFloat(SignedNormalizedFloat value) => new((value.Value + 1.0f) / 2.0f);
   
   /// <summary>
   /// Explicitly converts a <see cref="SignedNormalizedFloat"/> to a <see cref="SignedNormalizedDouble"/>.
   /// </summary>
   public static explicit operator SignedNormalizedDouble(SignedNormalizedFloat value) => new(value.Value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("F2");

    /// <summary>
    /// Determines whether the specified <see cref="SignedNormalizedFloat"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="SignedNormalizedFloat"/> to compare with the current instance.</param>
    /// <returns><c>true</c> if the values are equal; otherwise, <c>false</c>.</returns>
    public bool Equals(SignedNormalizedFloat other)
    {
        return Value.Equals(other.Value);
    }
    
    /// <summary>
    /// Determines whether the specified object is equal to the current instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><c>true</c> if the object is a <see cref="SignedNormalizedFloat"/> and the values are equal; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is SignedNormalizedFloat other && Equals(other);
    }
    
    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A hash code for the current instance.</returns>
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}