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
    /// Interpolation factor used when running with a fixed framerate.
    /// Typical values are in the range [0, 1]; 1 means no interpolation.
    /// </summary>
    /// <remarks>
    /// This factor is used to interpolate between the previous and current states
    /// of objects when rendering at a fixed framerate, providing smoother motion.
    /// Represents the fractional time between the last completed physics update and the time the renderer wants to draw.
    /// After all fixed update substeps are complete, a certain amount of unused time may remain before the next fixed update is due.
    /// This remaining time is expressed as a fraction of the fixed timestep and provided here as the interpolation factor.
    /// </remarks>
    public readonly double FixedFramerateInterpolationFactor;
    
    /// <summary>
    /// Returns a copy of this <see cref="ScreenInfo"/> with the specified fixed-framerate interpolation factor.
    /// </summary>
    /// <param name="factor">The interpolation factor to use for fixed-framerate rendering.</param>
    /// <returns>
    /// A new <see cref="ScreenInfo"/> instance with <see cref="FixedFramerateInterpolationFactor"/>
    /// set to <paramref name="factor"/>.
    /// </returns>
    public ScreenInfo SetFixedFramerateInterpolationFactor(double factor)
    {
        return new ScreenInfo(Area, MousePos, factor);
    }
    
    
    /// <summary>
    /// Returns the mouse position in a range between 0 and 1.
    /// 0,0 is the top-left corner of the area
    /// 1, 1 is the bottom right corner of the area
    /// </summary>
    public Vector2 RelativeMousePosition => MousePos / Area.Size.ToVector2();

    /// <summary>
    /// Initializes a new instance of the <see cref="ScreenInfo"/> struct.
    /// </summary>
    /// <param name="area">The rectangular area of the screen.</param>
    /// <param name="mousePos">The current mouse position in screen coordinates.</param>
    public ScreenInfo(Rect area, Vector2 mousePos)
    {
        Area = area;
        MousePos = mousePos;
        FixedFramerateInterpolationFactor = 1f;
    }
    private ScreenInfo(Rect area, Vector2 mousePos, double fixedFramerateInterpolationFactor)
    {
        Area = area;
        MousePos = mousePos;
        FixedFramerateInterpolationFactor = fixedFramerateInterpolationFactor;
    }
    
}