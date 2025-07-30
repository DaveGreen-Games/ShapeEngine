using System.Numerics;
using ShapeEngine.StaticLib;
using ShapeEngine.Random;

namespace ShapeEngine.Core.Structs;

/// <summary>
/// Represents a range of floating-point values with inclusive minimum and maximum bounds.
/// </summary>
/// <remarks>
/// Provides utility methods for range operations, random value generation, clamping, remapping, and arithmetic operations.
/// </remarks>
public readonly struct ValueRange : IEquatable<ValueRange>, IComparable<ValueRange>
{
    /// <summary>
    /// The minimum value of the range.
    /// </summary>
    public readonly float Min;
    /// <summary>
    /// The maximum value of the range.
    /// </summary>
    public readonly float Max;
    /// <summary>
    /// Gets the absolute difference between Max and Min, representing the total span of the range.
    /// </summary>
    public float TotalRange => MathF.Abs(Max- Min);
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueRange"/> struct with Min = 0.0f and Max = 1.0f.
    /// </summary>
    public ValueRange() { Min = 0.0f; Max = 1.0f; }
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueRange"/> struct from a <see cref="Vector2"/>.
    /// </summary>
    /// <param name="range">A vector where X is the minimum and Y is the maximum value.</param>
    public ValueRange(Vector2 range)
    {
        float min = range.X;
        float max = range.Y;
        if (min > max)
        {
            this.Max = min;
            this.Min = max;
        }
        else
        {
            this.Min = min;
            this.Max = max;
        }
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueRange"/> struct with specified minimum and maximum values.
    /// </summary>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    public ValueRange(float min, float max)
    {
        if (min > max)
        {
            this.Max = min;
            this.Min = max;
        }
        else
        {
            this.Min = min;
            this.Max = max;
        }
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueRange"/> struct with integer minimum and maximum values.
    /// </summary>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    public ValueRange(int min, int max)
    {
        if (min > max)
        {
            this.Max = min;
            this.Min = max;
        }
        else
        {
            this.Min = min;
            this.Max = max;
        }
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueRange"/> struct from a single maximum value.
    /// Minimum is set to 0.0f.
    /// </summary>
    /// <param name="max">The maximum value. If negative, Min is set to max and Max to 0.0f.</param>
    public ValueRange(float max)
    {
        if (max < 0.0f)
        {
            Min = max;
            this.Max = 0.0f;
        }
        else
        {
            Min = 0.0f;
            this.Max = max;
        }
    }
    #endregion

    #region Public Functions

    /// <summary>
    /// Returns a new <see cref="ValueRange"/> updated to include <paramref name="newValue"/>.
    /// If <paramref name="newValue"/> is less than Min, sets Min to <paramref name="newValue"/>.
    /// If <paramref name="newValue"/> is greater than Max, sets Max to <paramref name="newValue"/>.
    /// Otherwise, returns the current range unchanged.
    /// </summary>
    /// <param name="newValue">The value to include in the range.</param>
    /// <returns>A new <see cref="ValueRange"/> that includes <paramref name="newValue"/>.</returns>
    public ValueRange UpdateRange(float newValue)
    {
        if (newValue < Min) return new(newValue, Max);
        if (newValue > Max) return new(Min, newValue);
        return new(Min, Max);
    }
    
    
    /// <summary>
    /// Determines whether the range has a non-zero span (Min and Max are not equal).
    /// </summary>
    /// <returns>True if Min and Max are not equal; otherwise, false.</returns>
    public bool HasRange() => TotalRange > 0.00000001f;
    /// <summary>
    /// Determines whether the range is positive and has a non-zero span.
    /// </summary>
    /// <returns>True if Min and Max are not equal and both are positive; otherwise, false.</returns>
    public bool HasPositiveRange() => Min >= 0.0f && Max >= 0.0f && HasRange();
    /// <summary>
    /// Gets the normalized factor (0-1) of a value within the range.
    /// </summary>
    /// <param name="value">The value to evaluate.</param>
    /// <returns>The factor representing the value's position between Min and Max.</returns>
    public float GetFactor(float value)
    {
        if(value < Min) return 0.0f;
        return (value - Min) / (Max - Min);
    }
    /// <summary>
    /// Converts this range to a <see cref="ValueRangeInt"/> by casting Min and Max to integers.
    /// </summary>
    /// <returns>A <see cref="ValueRangeInt"/> representation of this range.</returns>
    public ValueRangeInt ToValueRangeInt() => new ValueRangeInt(Min, Max);
    /// <summary>
    /// Converts this range to a <see cref="Vector2"/>.
    /// </summary>
    /// <returns>A vector where X is Min and Y is Max.</returns>
    public Vector2 ToVector2() => new(Min, Max);
    /// <summary>
    /// Returns a random float value within the range [Min, Max].
    /// </summary>
    /// <returns>A random float value.</returns>
    public float Rand() { return Rng.Instance.RandF(Min, Max); }
    /// <summary>
    /// Linearly interpolates between Min and Max by the given factor.
    /// </summary>
    /// <param name="f">The interpolation factor (0-1).</param>
    /// <returns>The interpolated value.</returns>
    public float Lerp(float f) { return ShapeMath.LerpFloat(Min, Max, f); }
    /// <summary>
    /// Calculates the normalized position of a value within the range.
    /// </summary>
    /// <param name="value">The value to evaluate.</param>
    /// <returns>The normalized position (0-1).</returns>
    public float Inverse(float value) { return (value - Min) / (Max - Min); }
    /// <summary>
    /// Remaps a value from this range to another <see cref="ValueRange"/>.
    /// </summary>
    /// <param name="to">The target range.</param>
    /// <param name="value">The value to remap.</param>
    /// <returns>The remapped value in the target range.</returns>
    public float Remap(ValueRange to, float value) { return to.Lerp(Inverse(value)); }
    /// <summary>
    /// Remaps a value from this range to a new range defined by newMin and newMax.
    /// </summary>
    /// <param name="newMin">The new minimum value.</param>
    /// <param name="newMax">The new maximum value.</param>
    /// <param name="value">The value to remap.</param>
    /// <returns>The remapped value.</returns>
    public float Remap(float newMin, float newMax, float value) { return ShapeMath.LerpFloat(newMin, newMax, Inverse(value)); }
    /// <summary>
    /// Clamps a value to the range [Min, Max].
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <returns>The clamped value.</returns>
    public float Clamp(float value) => ShapeMath.Clamp(value, Min, Max);
    /// <summary>
    /// Determines whether this range overlaps with another <see cref="ValueRange"/>.
    /// </summary>
    /// <param name="other">The other range to check for overlap.</param>
    /// <returns>True if the ranges overlap; otherwise, false.</returns>
    public bool OverlapValueRange(ValueRange other) => other.Min <= Max && Min <= other.Max;
    /// <summary>
    /// Determines whether two ranges defined by their min and max values overlap.
    /// </summary>
    /// <param name="aMin">First range minimum.</param>
    /// <param name="aMax">First range maximum.</param>
    /// <param name="bMin">Second range minimum.</param>
    /// <param name="bMax">Second range maximum.</param>
    /// <returns>True if the ranges overlap; otherwise, false.</returns>
    public static bool OverlapValueRange(float aMin, float aMax, float bMin, float bMax)
    {
        var aRange = new ValueRange(aMin, aMax);
        var bRange = new ValueRange(bMin, bMax);
        return aRange.OverlapValueRange(bRange);
    }
    /// <summary>
    /// Returns a new <see cref="ValueRange"/> with the specified minimum value and the same maximum.
    /// </summary>
    /// <param name="min">The new minimum value.</param>
    /// <returns>A new <see cref="ValueRange"/>.</returns>
    public ValueRange SetMin(float min) => new(min, Max);
    /// <summary>
    /// Returns a new <see cref="ValueRange"/> with the specified maximum value and the same minimum.
    /// </summary>
    /// <param name="max">The new maximum value.</param>
    /// <returns>A new <see cref="ValueRange"/>.</returns>
    public ValueRange SetMax(float max) => new(Min, max);
    /// <summary>
    /// Returns a new <see cref="ValueRange"/> with the specified minimum and maximum values.
    /// </summary>
    /// <param name="min">The new minimum value.</param>
    /// <param name="max">The new maximum value.</param>
    /// <returns>A new <see cref="ValueRange"/>.</returns>
    public ValueRange Set(float min, float max) => new(min, max);
    #endregion
    
    #region Operators
    /// <summary>
    /// Adds two <see cref="ValueRange"/> instances element-wise.
    /// </summary>
    /// <param name="left">The first range.</param>
    /// <param name="right">The second range.</param>
    /// <returns>A new <see cref="ValueRange"/> with Min and Max added.</returns>
    public static ValueRange operator +(ValueRange left, ValueRange right)
    {
        return new(
            left.Min + right.Min, 
            left.Max + right.Max);
    }
    /// <summary>
    /// Subtracts the elements of one <see cref="ValueRange"/> from another element-wise.
    /// </summary>
    /// <param name="left">The first range.</param>
    /// <param name="right">The second range.</param>
    /// <returns>A new <see cref="ValueRange"/> with Min and Max subtracted.</returns>
    public static ValueRange operator -(ValueRange left, ValueRange right)
    {
        return new(
            left.Min - right.Min, 
            left.Max - right.Max);
    }
    /// <summary>
    /// Multiplies two <see cref="ValueRange"/> instances element-wise.
    /// </summary>
    /// <param name="left">The first range.</param>
    /// <param name="right">The second range.</param>
    /// <returns>A new <see cref="ValueRange"/> with Min and Max multiplied.</returns>
    public static ValueRange operator *(ValueRange left, ValueRange right)
    {
        return new(
            left.Min * right.Min, 
            left.Max * right.Max);
    }
    /// <summary>
    /// Divides the elements of one <see cref="ValueRange"/> by another element-wise.
    /// </summary>
    /// <param name="left">The numerator range.</param>
    /// <param name="right">The denominator range.</param>
    /// <returns>A new <see cref="ValueRange"/> with Min and Max divided. If denominator is zero, returns 0 for that element.</returns>
    public static ValueRange operator /(ValueRange left, ValueRange right)
    {
        return new(
            right.Min == 0 ? 0 : left.Min / right.Min, 
            right.Max == 0 ? 0 :left.Max / right.Max);
    }
    
    /// <summary>
    /// Adds a <see cref="ValueRangeInt"/> to a <see cref="ValueRange"/> element-wise.
    /// </summary>
    /// <param name="left">The ValueRange.</param>
    /// <param name="right">The ValueRangeInt.</param>
    /// <returns>A new <see cref="ValueRange"/> with Min and Max added.</returns>
    public static ValueRange operator +(ValueRange left, ValueRangeInt right)
    {
        return new(
            left.Min + right.Min, 
            left.Max + right.Max);
    }
    /// <summary>
    /// Subtracts a <see cref="ValueRangeInt"/> from a <see cref="ValueRange"/> element-wise.
    /// </summary>
    /// <param name="left">The ValueRange.</param>
    /// <param name="right">The ValueRangeInt.</param>
    /// <returns>A new <see cref="ValueRange"/> with Min and Max subtracted.</returns>
    public static ValueRange operator -(ValueRange left, ValueRangeInt right)
    {
        return new(
            left.Min - right.Min, 
            left.Max - right.Max);
    }
    /// <summary>
    /// Multiplies a <see cref="ValueRange"/> by a <see cref="ValueRangeInt"/> element-wise.
    /// </summary>
    /// <param name="left">The ValueRange.</param>
    /// <param name="right">The ValueRangeInt.</param>
    /// <returns>A new <see cref="ValueRange"/> with Min and Max multiplied.</returns>
    public static ValueRange operator *(ValueRange left, ValueRangeInt right)
    {
        return new(
            left.Min * right.Min, 
            left.Max * right.Max);
    }
    /// <summary>
    /// Divides a <see cref="ValueRange"/> by a <see cref="ValueRangeInt"/> element-wise.
    /// </summary>
    /// <param name="left">The ValueRange (numerator).</param>
    /// <param name="right">The ValueRangeInt (denominator).</param>
    /// <returns>A new <see cref="ValueRange"/> with Min and Max divided. If denominator is zero, returns 0 for that element.</returns>
    public static ValueRange operator /(ValueRange left, ValueRangeInt right)
    {
        return new(
            right.Min == 0 ? 0 : left.Min / right.Min, 
            right.Max == 0 ? 0 :left.Max / right.Max);
    }
    
    /// <summary>
    /// Adds a float value to both Min and Max of a <see cref="ValueRange"/>.
    /// </summary>
    /// <param name="left">The ValueRange.</param>
    /// <param name="right">The float value to add.</param>
    /// <returns>A new <see cref="ValueRange"/> with Min and Max increased by the value.</returns>
    public static ValueRange operator +(ValueRange left, float right)
    {
        return new(
            left.Min + right, 
            left.Max + right);
    }
    /// <summary>
    /// Subtracts a float value from both Min and Max of a <see cref="ValueRange"/>.
    /// </summary>
    /// <param name="left">The ValueRange.</param>
    /// <param name="right">The float value to subtract.</param>
    /// <returns>A new <see cref="ValueRange"/> with Min and Max decreased by the value.</returns>
    public static ValueRange operator -(ValueRange left, float right)
    {
        return new(
            left.Min - right, 
            left.Max - right);
    }
    /// <summary>
    /// Multiplies both Min and Max of a <see cref="ValueRange"/> by a float value.
    /// </summary>
    /// <param name="left">The ValueRange.</param>
    /// <param name="right">The float value to multiply by.</param>
    /// <returns>A new <see cref="ValueRange"/> with Min and Max multiplied by the value.</returns>
    public static ValueRange operator *(ValueRange left, float right)
    {
        return new(
            left.Min * right, 
            left.Max * right);
    }
    /// <summary>
    /// Divides both Min and Max of a <see cref="ValueRange"/> by a float value.
    /// </summary>
    /// <param name="left">The ValueRange (numerator).</param>
    /// <param name="right">The float value (denominator).</param>
    /// <returns>A new <see cref="ValueRange"/> with Min and Max divided by the value.
    /// If the denominator is zero, returns 0 for both elements.</returns>
    public static ValueRange operator /(ValueRange left, float right)
    {
        if (right == 0) return new(0, 0);
        return new(
            left.Min / right, 
            left.Max / right);
    }
    /// <summary>
    /// Adds an integer value to both Min and Max of a <see cref="ValueRange"/>.
    /// </summary>
    /// <param name="left">The ValueRange.</param>
    /// <param name="right">The integer value to add.</param>
    /// <returns>A new <see cref="ValueRange"/> with Min and Max increased by the value.</returns>
    public static ValueRange operator +(ValueRange left, int right)
    {
        return new(
            left.Min + right, 
            left.Max + right);
    }
    /// <summary>
    /// Subtracts an integer value from both Min and Max of a <see cref="ValueRange"/>.
    /// </summary>
    /// <param name="left">The ValueRange.</param>
    /// <param name="right">The integer value to subtract.</param>
    /// <returns>A new <see cref="ValueRange"/> with Min and Max decreased by the value.</returns>
    public static ValueRange operator -(ValueRange left, int right)
    {
        return new(
            left.Min - right, 
            left.Max - right);
    }
    /// <summary>
    /// Multiplies both Min and Max of a <see cref="ValueRange"/> by an integer value.
    /// </summary>
    /// <param name="left">The ValueRange.</param>
    /// <param name="right">The integer value to multiply by.</param>
    /// <returns>A new <see cref="ValueRange"/> with Min and Max multiplied by the value.</returns>
    public static ValueRange operator *(ValueRange left, int right)
    {
        return new(
            left.Min * right, 
            left.Max * right);
    }
    /// <summary>
    /// Divides both Min and Max of a <see cref="ValueRange"/> by an integer value.
    /// </summary>
    /// <param name="left">The ValueRange (numerator).</param>
    /// <param name="right">The integer value (denominator).</param>
    /// <returns>A new <see cref="ValueRange"/> with Min and Max divided by the value.
    /// If the denominator is zero, returns 0 for both elements.</returns>
    public static ValueRange operator /(ValueRange left, int right)
    {
        if (right == 0) return new(0, 0);
        return new(
            left.Min / right, 
            left.Max / right);
    }
    #endregion
    
    #region Equality & Comparison
    /// <summary>
    /// Determines whether the current <see cref="ValueRange"/> is equal to another <see cref="ValueRange"/>.
    /// </summary>
    /// <param name="other">The other <see cref="ValueRange"/> to compare with.</param>
    /// <returns>True if both Min and Max are equal; otherwise, false.</returns>
    public bool Equals(ValueRange other)
    {
        return Min.Equals(other.Min) && Max.Equals(other.Max);
    }
    
    /// <summary>
    /// Determines whether the current <see cref="ValueRange"/> is equal to another object.
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns>True if the object is a <see cref="ValueRange"/> and both Min and Max are equal; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return obj is ValueRange other && Equals(other);
    }
    
    /// <summary>
    /// Returns a hash code for the current <see cref="ValueRange"/>.
    /// </summary>
    /// <returns>A hash code for the current <see cref="ValueRange"/>.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Min, Max);
    }
    
    /// <summary>
    /// Compares the current <see cref="ValueRange"/> with another <see cref="ValueRange"/>.
    /// </summary>
    /// <param name="other">The other <see cref="ValueRange"/> to compare with.</param>
    /// <returns>
    /// A value less than zero if this instance is less than <paramref name="other"/>.
    /// Zero if this instance is equal to <paramref name="other"/>.
    /// A value greater than zero if this instance is greater than <paramref name="other"/>.
    /// </returns>
    public int CompareTo(ValueRange other)
    {
        int minComparison = Min.CompareTo(other.Min);
        if (minComparison != 0) return minComparison;
        return Max.CompareTo(other.Max);
    }

    /// <summary>
    /// Determines whether two <see cref="ValueRange"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="ValueRange"/> to compare.</param>
    /// <param name="right">The second <see cref="ValueRange"/> to compare.</param>
    /// <returns>True if both instances are equal; otherwise, false.</returns>
    public static bool operator ==(ValueRange left, ValueRange right)
    {
        return left.Equals(right);
    }
    
    /// <summary>
    /// Determines whether two <see cref="ValueRange"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="ValueRange"/> to compare.</param>
    /// <param name="right">The second <see cref="ValueRange"/> to compare.</param>
    /// <returns>True if the instances are not equal; otherwise, false.</returns>
    public static bool operator !=(ValueRange left, ValueRange right)
    {
        return !(left == right);
    }
    #endregion
}