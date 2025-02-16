using System.Numerics;
using ShapeEngine.StaticLib;
using ShapeEngine.Random;

namespace ShapeEngine.Core.Structs;

public readonly struct ValueRange
{
    public readonly float Min;
    public readonly float Max;

    #region Constructors

    public ValueRange() { Min = 0.0f; Max = 1.0f; }
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
    /// Returns true if min and max are not equal.
    /// </summary>
    /// <returns></returns>
    public bool HasRange() => Math.Abs(Max - Min) > 0.00000001f;
    /// <summary>
    /// Returns if min and max are not equal and positive.
    /// </summary>
    /// <returns></returns>
    public bool HasPositiveRange() => Min >= 0.0f && Max >= 0.0f && HasRange();
    public float GetFactor(float value)
    {
        if(value < Min) return 0.0f;
        return (value - Min) / (Max - Min);
    }
    public ValueRangeInt ToValueRangeInt() => new ValueRangeInt(Min, Max);
    public Vector2 ToVector2() => new(Min, Max);
    public float Rand() { return Rng.Instance.RandF(Min, Max); }
    public float Lerp(float f) { return ShapeMath.LerpFloat(Min, Max, f); }
    public float Inverse(float value) { return (value - Min) / (Max - Min); }
    public float Remap(ValueRange to, float value) { return to.Lerp(Inverse(value)); }
    public float Remap(float newMin, float newMax, float value) { return ShapeMath.LerpFloat(newMin, newMax, Inverse(value)); }

    public float Clamp(float value) => ShapeMath.Clamp(value, Min, Max);
    
    public bool OverlapValueRange(ValueRange other) => other.Min <= Max && Min <= other.Max;

    public static bool OverlapValueRange(float aMin, float aMax, float bMin, float bMax)
    {
        var aRange = new ValueRange(aMin, aMax);
        var bRange = new ValueRange(bMin, bMax);
        return aRange.OverlapValueRange(bRange);
    }
    
    public ValueRange SetMin(float min) => new(min, Max);
    public ValueRange SetMax(float max) => new(Min, max);
    public ValueRange Set(float min, float max) => new(min, max);
    
    #endregion
    
    #region Operators
    public static ValueRange operator +(ValueRange left, ValueRange right)
    {
        return new(
            left.Min + right.Min, 
            left.Max + right.Max);
    }
    public static ValueRange operator -(ValueRange left, ValueRange right)
    {
        return new(
            left.Min - right.Min, 
            left.Max - right.Max);
    }
    public static ValueRange operator *(ValueRange left, ValueRange right)
    {
        return new(
            left.Min * right.Min, 
            left.Max * right.Max);
    }
    public static ValueRange operator /(ValueRange left, ValueRange right)
    {
        return new(
            right.Min == 0 ? 0 : left.Min / right.Min, 
            right.Max == 0 ? 0 :left.Max / right.Max);
    }
    
    public static ValueRange operator +(ValueRange left, ValueRangeInt right)
    {
        return new(
            left.Min + right.Min, 
            left.Max + right.Max);
    }
    public static ValueRange operator -(ValueRange left, ValueRangeInt right)
    {
        return new(
            left.Min - right.Min, 
            left.Max - right.Max);
    }
    public static ValueRange operator *(ValueRange left, ValueRangeInt right)
    {
        return new(
            left.Min * right.Min, 
            left.Max * right.Max);
    }
    public static ValueRange operator /(ValueRange left, ValueRangeInt right)
    {
        return new(
            right.Min == 0 ? 0 : left.Min / right.Min, 
            right.Max == 0 ? 0 :left.Max / right.Max);
    }
    
    public static ValueRange operator +(ValueRange left, float right)
    {
        return new(
            left.Min + right, 
            left.Max + right);
    }
    public static ValueRange operator -(ValueRange left, float right)
    {
        return new(
            left.Min - right, 
            left.Max - right);
    }
    public static ValueRange operator *(ValueRange left, float right)
    {
        return new(
            left.Min * right, 
            left.Max * right);
    }
    public static ValueRange operator /(ValueRange left, float right)
    {
        if (right == 0) return new(0, 0);
        return new(
            left.Min / right, 
            left.Max / right);
    }
    
    public static ValueRange operator +(ValueRange left, int right)
    {
        return new(
            left.Min + right, 
            left.Max + right);
    }
    public static ValueRange operator -(ValueRange left, int right)
    {
        return new(
            left.Min - right, 
            left.Max - right);
    }
    public static ValueRange operator *(ValueRange left, int right)
    {
        return new(
            left.Min * right, 
            left.Max * right);
    }
    public static ValueRange operator /(ValueRange left, int right)
    {
        if (right == 0) return new(0, 0);
        return new(
            left.Min / right, 
            left.Max / right);
    }
    #endregion
}