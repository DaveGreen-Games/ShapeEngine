using System.Numerics;
using ShapeEngine.Lib;
using ShapeEngine.Random;

namespace ShapeEngine.Core;

public readonly struct ValueRange
{
    public readonly float Min;
    public readonly float Max;

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
}