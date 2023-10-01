using System.Numerics;
using ShapeEngine.Lib;

namespace ShapeEngine.Core;

public class RangeVector2
{
    public float min;
    public float max;
    public Vector2 center;
    public RangeVector2() { center = new(); min = 0.0f; max = 1.0f; }
    public RangeVector2(Vector2 center) { this.center = center; min = 0f; max = 1f; }
    public RangeVector2(float min, float max)
    {
        center = new();
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
    public RangeVector2(Vector2 center, float min, float max)
    {
        this.center = center;
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
    public RangeVector2(float max)
    {
        center = new();
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
    public RangeVector2(Vector2 center, float max)
    {
        this.center = center;
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

    public Vector2 Rand() { return center + ShapeRandom.randVec2(min, max); }
    public Vector2 Lerp(Vector2 end, float f) { return ShapeVec.Lerp(center, end, f); }
}