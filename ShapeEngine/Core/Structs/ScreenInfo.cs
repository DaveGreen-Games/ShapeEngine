using System.Numerics;
using ShapeEngine.Geometry.RectDef;

namespace ShapeEngine.Core.Structs;

/// <summary>
/// Represents information about the screen, including its area and mouse position.
/// </summary>
public readonly struct ScreenInfo
{
    /// <summary>
    /// The rectangular area of the screen.
    /// </summary>
    public readonly Rect Area;

    /// <summary>
    /// The current mouse position in screen coordinates.
    /// </summary>
    public readonly Vector2 MousePos;

    /// <summary>
    /// Returns the mouse position in a range between 0 and 1.
    /// 0,0 is the top-left corner of the area
    /// 1, 1 is the bottom right corner of the area
    /// </summary>
    public Vector2 RelativeMousePosition
    {
        get
        {
            var size = Area.Size.ToVector2();
            var pos = MousePos + size / 2;
            return pos / size;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScreenInfo"/> struct.
    /// </summary>
    /// <param name="area">The rectangular area of the screen.</param>
    /// <param name="mousePos">The current mouse position in screen coordinates.</param>
    public ScreenInfo(Rect area, Vector2 mousePos)
    {
        this.Area = area;
        this.MousePos = mousePos;
    }
}