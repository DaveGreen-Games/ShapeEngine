using ShapeEngine.Lib;

namespace ShapeEngine.Core;

public class RangeFloat
{
    public float min;
    public float max;

    public RangeFloat() { min = 0.0f; max = 1.0f; }
    public RangeFloat(float min, float max)
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
    public RangeFloat(float max)
    {
        if (max < 0.0f)
        {
            min = max;
            this.max = 0.0f;
        }
        else
        {
            min = 0.0f;
            this.max = max;
        }
    }
    public void Sort()
    {
        if (min > max)
        {
            float temp = max;
            max = min;
            min = temp;
        }
    }
    public float Rand() { return ShapeRandom.randF(min, max); }
    public float Lerp(float f) { return ShapeMath.LerpFloat(min, max, f); }
    public float Inverse(float value) { return (value - min) / (max - min); }
    public float Remap(RangeFloat to, float value) { return to.Lerp(Inverse(value)); }
    public float Remap(float newMin, float newMax, float value) { return ShapeMath.LerpFloat(newMin, newMax, Inverse(value)); }
}