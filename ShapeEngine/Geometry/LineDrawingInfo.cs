using ShapeEngine.Color;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry;

/// <summary>
/// Used for defining how a line is drawn.
/// </summary>
public readonly struct LineDrawingInfo
{
    /// <summary>
    /// The minimal possible line thickness. All thickness values a clamped to this value if lower.
    /// </summary>
    public static float LineMinThickness = 0.5f;
    
    /// <summary>
    /// The thickness of the line.
    /// </summary>
    public readonly float Thickness;
    
    /// <summary>
    /// The color of the line.
    /// </summary>
    public readonly ColorRgba Color;
    
    /// <summary>
    /// The end cap type of the line.
    /// </summary>
    public readonly LineCapType CapType;
    
    /// <summary>
    /// How many points to draw capped &amp; capped extended LineCapType.
    /// </summary>
    public readonly int CapPoints;


    /// <summary>
    /// Create Default LineDrawingInfo. (Thickness = 1f, Color = White, LineCapType = None, CapPoints = 0)
    /// </summary>
    public static LineDrawingInfo Default => new(1f, ColorRgba.White, LineCapType.None, 0);
    /// <summary>
    /// Create Default LineDrawingInfo. (Thickness = 1f, Color = White, LineCapType = Extended, CapPoints = 0)
    /// </summary>
    public static LineDrawingInfo Line => new(1f, ColorRgba.White, LineCapType.Extended, 0);
    /// <summary>
    /// Create Default LineDrawingInfo. (Thickness = 1f, Color = White, LineCapType = CappedExtended, CapPoints = 4)
    /// </summary>
    public static LineDrawingInfo Outline => new(1f, ColorRgba.White, LineCapType.CappedExtended, 4);

    
    /// <summary>
    /// Create a line info with "Capped" LineCapType.
    /// </summary>
    /// <param name="thickness">The thickness of the line.</param>
    /// <param name="color">The color of the line.</param>
    /// <param name="capPoints">The amount of points for the cap.</param>
    /// <returns>A new line drawing info with a capped line cap type.</returns>
    public static LineDrawingInfo Capped(float thickness, ColorRgba color, int capPoints) =>
        new(thickness, color, LineCapType.Capped, capPoints);

    /// <summary>
    /// Create a line info with "CappedExtended" LineCapType.
    /// </summary>
    /// <param name="thickness">The thickness of the line.</param>
    /// <param name="color">The color of the line.</param>
    /// <param name="capPoints">The amount of points for the cap.</param>
    /// <returns>A new line drawing info with a capped extended line cap type.</returns>
    public static LineDrawingInfo CappedExtended(float thickness, ColorRgba color, int capPoints) =>
        new(thickness, color, LineCapType.CappedExtended, capPoints);

    /// <summary>
    /// Create a line info with "Extended" LineCapType.
    /// </summary>
    /// <param name="thickness">The thickness of the line.</param>
    /// <param name="color">The color of the line.</param>
    /// <returns>A new line drawing info with an extended line cap type.</returns>
    public static LineDrawingInfo Extended(float thickness, ColorRgba color) => 
        new(thickness, color, LineCapType.Extended, 0);

    /// <summary>
    /// Create a line info with "None" LineCapType.
    /// </summary>
    /// <param name="thickness">The thickness of the line.</param>
    /// <param name="color">The color of the line.</param>
    /// <returns>A new line drawing info with no line cap type.</returns>
    public static LineDrawingInfo None(float thickness, ColorRgba color) => 
        new(thickness, color, LineCapType.None, 0);
    
    /// <summary>
    /// Creates a new line drawing info.
    /// </summary>
    /// <param name="thickness">The thickness of the line.</param>
    /// <param name="color">The color of the line.</param>
    public LineDrawingInfo(float thickness, ColorRgba color)
    {
        Thickness = MathF.Max(thickness, LineMinThickness);
        Color = color;
        CapType = LineCapType.None;
        CapPoints = 0;
    }
    /// <summary>
    /// Creates a new line drawing info.
    /// </summary>
    /// <param name="thickness">The thickness of the line.</param>
    /// <param name="color">The color of the line.</param>
    /// <param name="capPoints">The number of cap points to use.</param>
    public LineDrawingInfo(float thickness, ColorRgba color, int capPoints)
    {
        Thickness = MathF.Max(thickness, LineMinThickness);
        Color = color;
        CapType = LineCapType.None;
        CapPoints = capPoints;
    }
    /// <summary>
    /// Creates a new line drawing info.
    /// </summary>
    /// <param name="thickness">The thickness of the line.</param>
    /// <param name="color">The color of the line.</param>
    /// <param name="capType">The line cap type.</param>
    /// <param name="capPoints">The amount of points for the cap.</param>
    public LineDrawingInfo(float thickness, ColorRgba color, LineCapType capType, int capPoints)
    {
        Thickness = MathF.Max(thickness, LineMinThickness);
        Color = color;
        CapType = capType;
        CapPoints = capPoints;
    }

    
    /// <summary>
    /// Creates a new line drawing info with a new thickness.
    /// </summary>
    /// <param name="newThickness">The new thickness.</param>
    /// <returns>A new line drawing info with the new thickness.</returns>
    public LineDrawingInfo ChangeThickness(float newThickness) => new(newThickness, Color, CapType, CapPoints);
    /// <summary>
    /// Creates a new line drawing info with a new color.
    /// </summary>
    /// <param name="newColor">The new color.</param>
    /// <returns>A new line drawing info with the new color.</returns>
    public LineDrawingInfo ChangeColor(ColorRgba newColor) => new(Thickness, newColor, CapType, CapPoints);
    /// <summary>
    /// Creates a new line drawing info with a new line cap type.
    /// </summary>
    /// <param name="newCapType">The new line cap type.</param>
    /// <returns>A new line drawing info with the new line cap type.</returns>
    public LineDrawingInfo ChangeCapType(LineCapType newCapType) => new(Thickness, Color, newCapType, CapPoints);
    /// <summary>
    /// Creates a new line drawing info with a new amount of cap points.
    /// </summary>
    /// <param name="newCapPoints">The new amount of cap points.</param>
    /// <returns>A new line drawing info with the new amount of cap points.</returns>
    public LineDrawingInfo ChangeCapPoints(int newCapPoints) => new(Thickness, Color, CapType, newCapPoints);

    /// <summary>
    /// Linearly interpolates between two line drawing infos.
    /// </summary>
    /// <param name="to">The line drawing info to interpolate to.</param>
    /// <param name="f">The interpolation factor.</param>
    /// <returns>The interpolated line drawing info.</returns>
    public LineDrawingInfo Lerp(LineDrawingInfo to, float f)
    {
        return new
        (
            ShapeMath.LerpFloat(Thickness, to.Thickness, f),
            Color.Lerp(to.Color, f),
            CapType,
            CapPoints
        );
    }
    /// <summary>
    /// Interpolates between two line drawing infos using a power function.
    /// </summary>
    /// <param name="to">The line drawing info to interpolate to.</param>
    /// <param name="remainder">The remainder of the interpolation.</param>
    /// <param name="dt">The delta time.</param>
    /// <returns>The interpolated line drawing info.</returns>
    public LineDrawingInfo PowLerp(LineDrawingInfo to, float remainder, float dt)
    {
        return new
        (
            ShapeMath.PowLerpFloat(Thickness, to.Thickness, remainder, dt),
            Color.ExpDecayLerp(to.Color, remainder, dt),
            CapType,
            CapPoints
        );
    }
    /// <summary>
    /// Interpolates between two line drawing infos using exponential decay.
    /// </summary>
    /// <param name="to">The line drawing info to interpolate to.</param>
    /// <param name="f">The interpolation factor.</param>
    /// <param name="dt">The delta time.</param>
    /// <returns>The interpolated line drawing info.</returns>
    public LineDrawingInfo ExpDecayLerp(LineDrawingInfo to, float f, float dt)
    {
        return new
        (
            ShapeMath.ExpDecayLerpFloat(Thickness, to.Thickness, f, dt),
            Color.ExpDecayLerp(to.Color, f, dt),
            CapType,
            CapPoints
        );
    }
}