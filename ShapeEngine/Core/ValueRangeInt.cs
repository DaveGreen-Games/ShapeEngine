using System.Numerics;
using ShapeEngine.Lib;
using ShapeEngine.Random;

namespace ShapeEngine.Core;

public readonly struct ValueRangeInt
{
    public readonly int Min;
    public readonly int Max;

    #region Constructors
    
    public ValueRangeInt() { Min = 0; Max = 1; }
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
    
    public ValueRange ToValueRange() => new ValueRange(Min, Max);
    public Vector2 ToVector2() => new(Min, Max);
    public int Rand() { return Rng.Instance.RandI(Min, Max); }
    public int Lerp(float f) { return ShapeMath.LerpInt(Min, Max, f); }
    public float Inverse(int value) { return (value - Min) / (float)(Max - Min); }
    
    public int Remap(ValueRangeInt to, int value) { return to.Lerp(Inverse(value)); }

    public int Remap(int newMin, int newMax, int value) { return ShapeMath.LerpInt(newMin, newMax, Inverse(value)); }
    public int Clamp(int value) => ShapeMath.Clamp(value, Min, Max);
    
    public bool OverlapValueRange(ValueRangeInt other) => other.Min <= Max && Min <= other.Max;
    
    public ValueRangeInt SetMin(int min) => new(min, Max);
    public ValueRangeInt SetMax(int max) => new(Min, max);
    public ValueRangeInt Set(int min, int max) => new(min, max);
    #endregion
    
    #region Operators
    
    public static ValueRangeInt operator +(ValueRangeInt left, ValueRangeInt right)
    {
        return new(
            left.Min + right.Min, 
            left.Max + right.Max);
    }
    public static ValueRangeInt operator -(ValueRangeInt left, ValueRangeInt right)
    {
        return new(
            left.Min - right.Min, 
            left.Max - right.Max);
    }
    public static ValueRangeInt operator *(ValueRangeInt left, ValueRangeInt right)
    {
        return new(
            left.Min * right.Min, 
            left.Max * right.Max);
    }
    public static ValueRangeInt operator /(ValueRangeInt left, ValueRangeInt right)
    {
        return new(
            right.Min == 0 ? 0 : left.Min / right.Min, 
            right.Max == 0 ? 0 :left.Max / right.Max);
    }
    
    public static ValueRangeInt operator +(ValueRangeInt left, ValueRange right)
    {
        return new(
            left.Min + right.Min, 
            left.Max + right.Max);
    }
    public static ValueRangeInt operator -(ValueRangeInt left, ValueRange right)
    {
        return new(
            left.Min - right.Min, 
            left.Max - right.Max);
    }
    public static ValueRangeInt operator *(ValueRangeInt left, ValueRange right)
    {
        return new(
            left.Min * right.Min, 
            left.Max * right.Max);
    }
    public static ValueRangeInt operator /(ValueRangeInt left, ValueRange right)
    {
        return new(
            right.Min == 0 ? 0 : left.Min / right.Min, 
            right.Max == 0 ? 0 :left.Max / right.Max);
    }
    
    public static ValueRangeInt operator +(ValueRangeInt left, float right)
    {
        return new(
            left.Min + (int)right, 
            left.Max + (int)right);
    }
    public static ValueRangeInt operator -(ValueRangeInt left, float right)
    {
        return new(
            left.Min - (int)right, 
            left.Max - (int)right);
    }
    public static ValueRangeInt operator *(ValueRangeInt left, float right)
    {
        return new(
            (int)(left.Min * right), 
            (int)(left.Max * right));
    }
    public static ValueRangeInt operator /(ValueRangeInt left, float right)
    {
        if (right == 0) return new(0, 0);
        return new(
            (int)(left.Min / right), 
            (int)(left.Max / right));
    }
    
    public static ValueRangeInt operator +(ValueRangeInt left, int right)
    {
        return new(
            left.Min + right, 
            left.Max + right);
    }
    public static ValueRangeInt operator -(ValueRangeInt left, int right)
    {
        return new(
            left.Min - right, 
            left.Max - right);
    }
    public static ValueRangeInt operator *(ValueRangeInt left, int right)
    {
        return new(
            left.Min * right, 
            left.Max * right);
    }
    public static ValueRangeInt operator /(ValueRangeInt left, int right)
    {
        if (right == 0) return new(0, 0);
        return new(
            left.Min / right, 
            left.Max / right);
    }
    #endregion
}