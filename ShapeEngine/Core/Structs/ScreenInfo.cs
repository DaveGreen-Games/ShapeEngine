using System.Numerics;
using ShapeEngine.Core.Shapes;
namespace ShapeEngine.Core.Structs;

public readonly struct ScreenInfo
{
    public readonly Rect Area;
    public readonly Vector2 MousePos;

    /// <summary>
    /// Returns the mouse position in a range between 0 and 1.
    /// 0,0 is the topleft corner of the area
    /// 1, 1 is the bottom right corner of the area
    /// </summary>
    public Vector2 RelativeMousePosition => MousePos / Area.Size.ToVector2();

    public ScreenInfo(Rect area, Vector2 mousePos)
    {
        this.Area = area;
        this.MousePos = mousePos;
    }
}