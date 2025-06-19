using ShapeEngine.Color;

namespace ShapeEngine.Core.Structs;
//TODO: Move to core namespace! Definetely not in struct namespace!
public class CurveColor(int capacity) : Curve<ColorRgba>(capacity)
{
    protected override ColorRgba Interpolate(ColorRgba a, ColorRgba b, float time)
    {
        return a.Lerp(b, time);
    }

    protected override ColorRgba GetDefaultValue() => ColorRgba.Clear;
}