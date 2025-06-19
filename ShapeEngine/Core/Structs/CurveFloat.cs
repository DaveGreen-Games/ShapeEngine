using ShapeEngine.StaticLib;

namespace ShapeEngine.Core.Structs;
//TODO: Move to core namespace! Definetely not in struct namespace!
public class CurveFloat(int capacity) : Curve<float>(capacity)
{
    protected override float Interpolate(float a, float b, float time)
    {
        return ShapeMath.LerpFloat(a, b, time);
    }

    protected override float GetDefaultValue() => 0f;
}