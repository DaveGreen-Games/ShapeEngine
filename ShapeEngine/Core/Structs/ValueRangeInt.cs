using System.Numerics;
using ShapeEngine.StaticLib;
using ShapeEngine.Random;

namespace ShapeEngine.Core.Structs;

/// <summary>
/// Represents a range of integer values with inclusive minimum and maximum bounds.
/// </summary>
/// <remarks>
/// Provides utility methods for range operations, random value generation, clamping, remapping, and arithmetic operations.
/// </remarks>
public readonly struct ValueRangeInt : IEquatable<ValueRangeInt>, IComparable<ValueRangeInt>
{
    /// <summary>
    /// The minimum value of the range.
    /// </summary>
    public readonly int Min;
    /// <summary>
    /// The maximum value of the range.
    /// </summary>
    public readonly int Max;
    /// <summary>
    /// Gets the absolute difference between <see cref="Min"/> and <see cref="Max"/>,
    /// representing the total span of the range.
    /// </summary>
    public int TotalRange => ShapeMath.AbsInt(Max- Min);
    
    
    #region Constructors
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueRangeInt"/> struct with Min = 0 and Max = 1.
    /// </summary>
    public ValueRangeInt() { Min = 0; Max = 1; }
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueRangeInt"/> struct from a <see cref="Vector2"/>.
    /// </summary>
    /// <param name="range">A vector where X is the minimum and Y is the maximum value (cast to int).</param>
    public ValueRangeInt(Vector2 range)
    {
        var min = (int)range.X;
        var max = (int)range.Y;
        if (min > max)
        {
            Max = min;
            Min = max;
        }
        else
        {
            Min = min;
            Max = max;
        }
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueRangeInt"/> struct with specified minimum and maximum values.
    /// </summary>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    public ValueRangeInt(int min, int max)
    {
        if (min > max)
        {
            Max = min;
            Min = max;
        }
        else
        {
            Min = min;
            Max = max;
        }
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueRangeInt"/> struct with float minimum and maximum values (cast to int).
    /// </summary>
    /// <param name="min">The minimum value (float, cast to int).</param>
    /// <param name="max">The maximum value (float, cast to int).</param>
    public ValueRangeInt(float min, float max)
    {
        if (min > max)
        {
            Max = (int)min;
            Min = (int)max;
        }
        else
        {
            Min = (int)min;
            Max = (int)max;
        }
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueRangeInt"/> struct from a single maximum value. Minimum is set to 0.
    /// </summary>
    /// <param name="max">The maximum value. If negative, Min is set to max and Max to 0.</param>
    public ValueRangeInt(int max)
    {
        if (max < 0.0f)
        {
            Min = max;
            Max = 0;
        }
        else
        {
            Min = 0;
            Max = max;
        }
    }
    
    #endregion
    
    #region Public Functions
    /// <summary>
    /// Returns a new <see cref="ValueRangeInt"/> updated to include <paramref name="newValue"/>.
    /// If <paramref name="newValue"/> is less than Min, sets Min to <paramref name="newValue"/>.
    /// If <paramref name="newValue"/> is greater than Max, sets Max to <paramref name="newValue"/>.
    /// Otherwise, returns the current range unchanged.
    /// </summary>
    /// <param name="newValue">The value to include in the range.</param>
    /// <returns>A new <see cref="ValueRangeInt"/> that includes <paramref name="newValue"/>.</returns>
    public ValueRangeInt UpdateRange(float newValue)
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
    public float GetFactor(int value)
    {
        if(value < Min) return 0.0f;
        return (float)(value - Min) / (Max - Min);
    }
    /// <summary>
    /// Converts this range to a <see cref="ValueRange"/> by casting Min and Max to float.
    /// </summary>
    /// <returns>A <see cref="ValueRange"/> representation of this range.</returns>
    public ValueRange ToValueRange() => new ValueRange(Min, Max);
    /// <summary>
    /// Converts this range to a <see cref="Vector2"/>.
    /// </summary>
    /// <returns>A vector where X is Min and Y is Max.</returns>
    public Vector2 ToVector2() => new(Min, Max);
    /// <summary>
    /// Returns a random integer value within the range [Min, Max].
    /// </summary>
    /// <returns>A random integer value.</returns>
    public int Rand() { return Rng.Instance.RandI(Min, Max); }
    /// <summary>
    /// Linearly interpolates between Min and Max by the given factor.
    /// </summary>
    /// <param name="f">The interpolation factor (0-1).</param>
    /// <returns>The interpolated integer value.</returns>
    public int Lerp(float f) { return ShapeMath.LerpInt(Min, Max, f); }
    /// <summary>
    /// Calculates the normalized position of a value within the range.
    /// </summary>
    /// <param name="value">The value to evaluate.</param>
    /// <returns>The normalized position (0-1).</returns>
    public float Inverse(int value) { return (value - Min) / (float)(Max - Min); }
    /// <summary>
    /// Remaps a value from this range to another <see cref="ValueRangeInt"/>.
    /// </summary>
    /// <param name="to">The target range.</param>
    /// <param name="value">The value to remap.</param>
    /// <returns>The remapped value in the target range.</returns>
    public int Remap(ValueRangeInt to, int value) { return to.Lerp(Inverse(value)); }
    /// <summary>
    /// Remaps a value from this range to a new range defined by newMin and newMax.
    /// </summary>
    /// <param name="newMin">The new minimum value.</param>
    /// <param name="newMax">The new maximum value.</param>
    /// <param name="value">The value to remap.</param>
    /// <returns>The remapped value.</returns>
    public int Remap(int newMin, int newMax, int value) { return ShapeMath.LerpInt(newMin, newMax, Inverse(value)); }
    /// <summary>
    /// Clamps a value to the range [Min, Max].
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <returns>The clamped value.</returns>
    public int Clamp(int value) => ShapeMath.Clamp(value, Min, Max);
    /// <summary>
    /// Determines whether this range overlaps with another <see cref="ValueRangeInt"/>.
    /// </summary>
    /// <param name="other">The other range to check for overlap.</param>
    /// <returns>True if the ranges overlap; otherwise, false.</returns>
    public bool OverlapValueRange(ValueRangeInt other) => other.Min <= Max && Min <= other.Max;
    /// <summary>
    /// Returns a new <see cref="ValueRangeInt"/> with the specified minimum value and the same maximum.
    /// </summary>
    /// <param name="min">The new minimum value.</param>
    /// <returns>A new <see cref="ValueRangeInt"/>.</returns>
    public ValueRangeInt SetMin(int min) => new(min, Max);
    /// <summary>
    /// Returns a new <see cref="ValueRangeInt"/> with the specified maximum value and the same minimum.
    /// </summary>
    /// <param name="max">The new maximum value.</param>
    /// <returns>A new <see cref="ValueRangeInt"/>.</returns>
    public ValueRangeInt SetMax(int max) => new(Min, max);
    /// <summary>
    /// Returns a new <see cref="ValueRangeInt"/> with the specified minimum and maximum values.
    /// </summary>
    /// <param name="min">The new minimum value.</param>
    /// <param name="max">The new maximum value.</param>
    /// <returns>A new <see cref="ValueRangeInt"/>.</returns>
    public ValueRangeInt Set(int min, int max) => new(min, max);
    #endregion
    
    #region Operators
    /// <summary>
    /// Adds two <see cref="ValueRangeInt"/> instances element-wise.
    /// </summary>
    /// <param name="left">The first range.</param>
    /// <param name="right">The second range.</param>
    /// <returns>A new <see cref="ValueRangeInt"/> with Min and Max added.</returns>
    public static ValueRangeInt operator +(ValueRangeInt left, ValueRangeInt right)
    {
        return new(
            left.Min + right.Min, 
            left.Max + right.Max);
    }
    /// <summary>
    /// Subtracts the elements of one <see cref="ValueRangeInt"/> from another element-wise.
    /// </summary>
    /// <param name="left">The first range.</param>
    /// <param name="right">The second range.</param>
    /// <returns>A new <see cref="ValueRangeInt"/> with Min and Max subtracted.</returns>
    public static ValueRangeInt operator -(ValueRangeInt left, ValueRangeInt right)
    {
        return new(
            left.Min - right.Min, 
            left.Max - right.Max);
    }
    /// <summary>
    /// Multiplies two <see cref="ValueRangeInt"/> instances element-wise.
    /// </summary>
    /// <param name="left">The first range.</param>
    /// <param name="right">The second range.</param>
    /// <returns>A new <see cref="ValueRangeInt"/> with Min and Max multiplied.</returns>
    public static ValueRangeInt operator *(ValueRangeInt left, ValueRangeInt right)
    {
        return new(
            left.Min * right.Min, 
            left.Max * right.Max);
    }
    /// <summary>
    /// Divides the elements of one <see cref="ValueRangeInt"/> by another element-wise.
    /// </summary>
    /// <param name="left">The numerator range.</param>
    /// <param name="right">The denominator range.</param>
    /// <returns>A new <see cref="ValueRangeInt"/> with Min and Max divided.
    /// If the denominator is zero, returns 0 for that element.</returns>
    public static ValueRangeInt operator /(ValueRangeInt left, ValueRangeInt right)
    {
        return new(
            right.Min == 0 ? 0 : left.Min / right.Min, 
            right.Max == 0 ? 0 :left.Max / right.Max);
    }
    
    /// <summary>
    /// Adds a <see cref="ValueRange"/> to a <see cref="ValueRangeInt"/> element-wise.
    /// </summary>
    /// <param name="left">The ValueRangeInt.</param>
    /// <param name="right">The ValueRange.</param>
    /// <returns>A new <see cref="ValueRangeInt"/> with Min and Max added.</returns>
    public static ValueRangeInt operator +(ValueRangeInt left, ValueRange right)
    {
        return new(
            left.Min + right.Min, 
            left.Max + right.Max);
    }
    /// <summary>
    /// Subtracts a <see cref="ValueRange"/> from a <see cref="ValueRangeInt"/> element-wise.
    /// </summary>
    /// <param name="left">The ValueRangeInt.</param>
    /// <param name="right">The ValueRange.</param>
    /// <returns>A new <see cref="ValueRangeInt"/> with Min and Max subtracted.</returns>
    public static ValueRangeInt operator -(ValueRangeInt left, ValueRange right)
    {
        return new(
            left.Min - right.Min, 
            left.Max - right.Max);
    }
    /// <summary>
    /// Multiplies a <see cref="ValueRangeInt"/> by a <see cref="ValueRange"/> element-wise.
    /// </summary>
    /// <param name="left">The ValueRangeInt.</param>
    /// <param name="right">The ValueRange.</param>
    /// <returns>A new <see cref="ValueRangeInt"/> with Min and Max multiplied.</returns>
    public static ValueRangeInt operator *(ValueRangeInt left, ValueRange right)
    {
        return new(
            left.Min * right.Min, 
            left.Max * right.Max);
    }
    /// <summary>
    /// Divides a <see cref="ValueRangeInt"/> by a <see cref="ValueRange"/> element-wise.
    /// </summary>
    /// <param name="left">The ValueRangeInt (numerator).</param>
    /// <param name="right">The ValueRange (denominator).</param>
    /// <returns>A new <see cref="ValueRangeInt"/> with Min and Max divided. If denominator is zero, returns 0 for that element.</returns>
    public static ValueRangeInt operator /(ValueRangeInt left, ValueRange right)
    {
        return new(
            right.Min == 0 ? 0 : left.Min / right.Min, 
            right.Max == 0 ? 0 :left.Max / right.Max);
    }
    /// <summary>
    /// Adds a float value to both Min and Max of a <see cref="ValueRangeInt"/>.
    /// </summary>
    /// <param name="left">The ValueRangeInt.</param>
    /// <param name="right">The float value to add.</param>
    /// <returns>A new <see cref="ValueRangeInt"/> with Min and Max increased by the value.</returns>
    public static ValueRangeInt operator +(ValueRangeInt left, float right)
    {
        return new(
            left.Min + (int)right, 
            left.Max + (int)right);
    }
    /// <summary>
    /// Subtracts a float value from both Min and Max of a <see cref="ValueRangeInt"/>.
    /// </summary>
    /// <param name="left">The ValueRangeInt.</param>
    /// <param name="right">The float value to subtract.</param>
    /// <returns>A new <see cref="ValueRangeInt"/> with Min and Max decreased by the value.</returns>
    public static ValueRangeInt operator -(ValueRangeInt left, float right)
    {
        return new(
            left.Min - (int)right, 
            left.Max - (int)right);
    }
    /// <summary>
    /// Multiplies both Min and Max of a <see cref="ValueRangeInt"/> by a float value.
    /// </summary>
    /// <param name="left">The ValueRangeInt.</param>
    /// <param name="right">The float value to multiply by.</param>
    /// <returns>A new <see cref="ValueRangeInt"/> with Min and Max multiplied by the value.</returns>
    public static ValueRangeInt operator *(ValueRangeInt left, float right)
    {
        return new(
            (int)(left.Min * right), 
            (int)(left.Max * right));
    }
    /// <summary>
    /// Divides both Min and Max of a <see cref="ValueRangeInt"/> by a float value.
    /// </summary>
    /// <param name="left">The ValueRangeInt (numerator).</param>
    /// <param name="right">The float value (denominator).</param>
    /// <returns>A new <see cref="ValueRangeInt"/> with Min and Max divided by the value. If denominator is zero, returns 0 for both elements.</returns>
    public static ValueRangeInt operator /(ValueRangeInt left, float right)
    {
        if (right == 0) return new(0, 0);
        return new(
            (int)(left.Min / right), 
            (int)(left.Max / right));
    }
    /// <summary>
    /// Adds an integer value to both Min and Max of a <see cref="ValueRangeInt"/>.
    /// </summary>
    /// <param name="left">The ValueRangeInt.</param>
    /// <param name="right">The integer value to add.</param>
    /// <returns>A new <see cref="ValueRangeInt"/> with Min and Max increased by the value.</returns>
    public static ValueRangeInt operator +(ValueRangeInt left, int right)
    {
        return new(
            left.Min + right, 
            left.Max + right);
    }
    /// <summary>
    /// Subtracts an integer value from both Min and Max of a <see cref="ValueRangeInt"/>.
    /// </summary>
    /// <param name="left">The ValueRangeInt.</param>
    /// <param name="right">The integer value to subtract.</param>
    /// <returns>A new <see cref="ValueRangeInt"/> with Min and Max decreased by the value.</returns>
    public static ValueRangeInt operator -(ValueRangeInt left, int right)
    {
        return new(
            left.Min - right, 
            left.Max - right);
    }
    /// <summary>
    /// Multiplies both Min and Max of a <see cref="ValueRangeInt"/> by an integer value.
    /// </summary>
    /// <param name="left">The ValueRangeInt.</param>
    /// <param name="right">The integer value to multiply by.</param>
    /// <returns>A new <see cref="ValueRangeInt"/> with Min and Max multiplied by the value.</returns>
    public static ValueRangeInt operator *(ValueRangeInt left, int right)
    {
        return new(
            left.Min * right, 
            left.Max * right);
    }
    /// <summary>
    /// Divides both Min and Max of a <see cref="ValueRangeInt"/> by an integer value.
    /// </summary>
    /// <param name="left">The ValueRangeInt (numerator).</param>
    /// <param name="right">The integer value (denominator).</param>
    /// <returns>A new <see cref="ValueRangeInt"/> with Min and Max divided by the value. If denominator is zero, returns 0 for both elements.</returns>
    public static ValueRangeInt operator /(ValueRangeInt left, int right)
    {
        if (right == 0) return new(0, 0);
        return new(
            left.Min / right, 
            left.Max / right);
    }
    #endregion
    
    #region Equality & Comparison
    /// <summary>
    /// Determines whether the current <see cref="ValueRangeInt"/> is equal to another <see cref="ValueRangeInt"/>.
    /// </summary>
    /// <param name="other">The other <see cref="ValueRangeInt"/> to compare with.</param>
    /// <returns>True if both Min and Max are equal; otherwise, false.</returns>
    public bool Equals(ValueRangeInt other)
    {
        return Min.Equals(other.Min) && Max.Equals(other.Max);
    }
    
    /// <summary>
    /// Determines whether the current <see cref="ValueRangeInt"/> is equal to another object.
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns>True if the object is a <see cref="ValueRangeInt"/> and both Min and Max are equal; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return obj is ValueRangeInt other && Equals(other);
    }
    
    /// <summary>
    /// Returns a hash code for the current <see cref="ValueRangeInt"/>.
    /// </summary>
    /// <returns>A hash code for the current <see cref="ValueRangeInt"/>.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Min, Max);
    }
    
    /// <summary>
    /// Compares the current <see cref="ValueRangeInt"/> with another <see cref="ValueRangeInt"/>.
    /// </summary>
    /// <param name="other">The other <see cref="ValueRangeInt"/> to compare with.</param>
    /// <returns>
    /// A value less than zero if this instance is less than <paramref name="other"/>.
    /// Zero if this instance is equal to <paramref name="other"/>.
    /// A value greater than zero if this instance is greater than <paramref name="other"/>.
    /// </returns>
    public int CompareTo(ValueRangeInt other)
    {
        int minComparison = Min.CompareTo(other.Min);
        if (minComparison != 0) return minComparison;
        return Max.CompareTo(other.Max);
    }

    /// <summary>
    /// Determines whether two <see cref="ValueRangeInt"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="ValueRangeInt"/> to compare.</param>
    /// <param name="right">The second <see cref="ValueRangeInt"/> to compare.</param>
    /// <returns>True if both instances are equal; otherwise, false.</returns>
    public static bool operator ==(ValueRangeInt left, ValueRangeInt right)
    {
        return left.Equals(right);
    }
    
    /// <summary>
    /// Determines whether two <see cref="ValueRangeInt"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="ValueRangeInt"/> to compare.</param>
    /// <param name="right">The second <see cref="ValueRangeInt"/> to compare.</param>
    /// <returns>True if the instances are not equal; otherwise, false.</returns>
    public static bool operator !=(ValueRangeInt left, ValueRangeInt right)
    {
        return !(left == right);
    }
    #endregion
}