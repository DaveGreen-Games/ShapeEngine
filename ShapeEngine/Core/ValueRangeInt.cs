using System.Numerics;
using ShapeEngine.Lib;
using ShapeEngine.Random;

namespace ShapeEngine.Core;

public readonly struct ValueRangeInt
{
    public readonly int Min;
    public readonly int Max;

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
    

    public Vector2 ToVector2() => new(Min, Max);
    public int Rand() { return Rng.Instance.RandI(Min, Max); }
    public int Lerp(float f) { return ShapeMath.LerpInt(Min, Max, f); }
    public float Inverse(int value) { return (value - Min) / (float)(Max - Min); }
    
    public int Remap(ValueRangeInt to, int value) { return to.Lerp(Inverse(value)); }

    public int Remap(int newMin, int newMax, int value) { return ShapeMath.LerpInt(newMin, newMax, Inverse(value)); }
    public int Clamp(int value) => ShapeMath.Clamp(value, Min, Max);
    
    public bool OverlapValueRange(ValueRangeInt other) => other.Min <= Max && Min <= other.Max;
}