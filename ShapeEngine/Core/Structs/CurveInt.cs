using ShapeEngine.StaticLib;

namespace ShapeEngine.Core.Structs;
//TODO: Move to core namespace! Definetely not in struct namespace!
public class CurveInt(int capacity) : Curve<int>(capacity)
{
    protected override int Interpolate(int a, int b, float time)
    {
        return ShapeMath.LerpInt(a, b, time);
    }

    protected override int GetDefaultValue() => 0;
}