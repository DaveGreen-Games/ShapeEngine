using ShapeEngine.Lib;

namespace ShapeEngine.Core;

public class RangeInt
{
    public int Min;
    public int Max;

    public RangeInt() { Min = 0; Max = 100; }
    public RangeInt(int min, int max)
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
    public RangeInt(int max)
    {
        if (max < 0)
        {
            Min = max;
            this.Max = 0;
        }
        else
        {
            Min = 0;
            this.Max = max;
        }
    }

    public int Rand() { return ShapeRandom.randI(Min, Max); }
    public int Lerp(float f) { return (int)ShapeMath.LerpFloat(Min, Max, f); }
    public float Inverse(int value) { return (value - Min) / (Max - Min); }
    public int Remap(RangeInt to, int value) { return to.Lerp(Inverse(value)); }
    public int Remap(int newMin, int newMax, int value) { return ShapeMath.LerpInt(newMin, newMax, Inverse(value)); }
    public int Clamp(int value)
    {
        return ShapeMath.Clamp(value, Min, Max);
    }
}