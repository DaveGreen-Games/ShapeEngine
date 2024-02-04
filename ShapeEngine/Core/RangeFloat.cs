using System.Numerics;
using ShapeEngine.Lib;

namespace ShapeEngine.Core;

public class RangeFloat
{
    public float Min;
    public float Max;

    public RangeFloat() { Min = 0.0f; Max = 1.0f; }

    public RangeFloat(Vector2 range)
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
    public RangeFloat(float min, float max)
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
    public RangeFloat(float max)
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
    public void Sort()
    {
        if (Min > Max)
        {
            float temp = Max;
            Max = Min;
            Min = temp;
        }
    }

    public Vector2 ToVector2() => new Vector2(Min, Max);
    public float Rand() { return ShapeRandom.RandF(Min, Max); }
    public float Lerp(float f) { return ShapeMath.LerpFloat(Min, Max, f); }
    public float Inverse(float value) { return (value - Min) / (Max - Min); }
    public float Remap(RangeFloat to, float value) { return to.Lerp(Inverse(value)); }
    public float Remap(float newMin, float newMax, float value) { return ShapeMath.LerpFloat(newMin, newMax, Inverse(value)); }

    public float Clamp(float value)
    {
        return ShapeMath.Clamp(value, Min, Max);
    }
}