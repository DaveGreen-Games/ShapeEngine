using ShapeEngine.Lib;

namespace ShapeEngine.Core;

public class RangeInt
{
    public int min;
    public int max;

    public RangeInt() { min = 0; max = 100; }
    public RangeInt(int min, int max)
    {
        if (min > max)
        {
            this.max = min;
            this.min = max;
        }
        else
        {
            this.min = min;
            this.max = max;
        }
    }
    public RangeInt(int max)
    {
        if (max < 0)
        {
            min = max;
            this.max = 0;
        }
        else
        {
            min = 0;
            this.max = max;
        }
    }

    public int Rand() { return ShapeRandom.randI(min, max); }
    public int Lerp(float f) { return (int)ShapeMath.LerpFloat(min, max, f); }
    public float Inverse(int value) { return (value - min) / (max - min); }
    public int Remap(RangeInt to, int value) { return to.Lerp(Inverse(value)); }
    public int Remap(int newMin, int newMax, int value) { return ShapeMath.LerpInt(newMin, newMax, Inverse(value)); }
}