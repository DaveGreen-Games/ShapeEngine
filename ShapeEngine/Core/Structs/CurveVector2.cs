using System.Numerics;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Core.Structs;
//TODO: Move to core namespace! Definetely not in struct namespace!
public class CurveVector2(int capacity) : Curve<Vector2>(capacity)
{
    
    protected override Vector2 Interpolate(Vector2 a, Vector2 b, float time)
    {
        return a.Lerp(b, time);
    }

    protected override Vector2 GetDefaultValue() => Vector2.Zero;
}