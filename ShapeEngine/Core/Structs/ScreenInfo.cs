using System.Numerics;
using ShapeEngine.Core.Shapes;
namespace ShapeEngine.Core.Structs;

public readonly struct ScreenInfo
{
    public readonly Rect Area;
    public readonly Vector2 MousePos;

    public ScreenInfo(Rect area, Vector2 mousePos)
    {
        this.Area = area;
        this.MousePos = mousePos;
    }
}