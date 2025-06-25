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
    /// How many points to draw capped & capped extended LineCapType.
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
    public static LineDrawingInfo Capped(float thickness, ColorRgba color, int capPoints) =>
        new(thickness, color, LineCapType.Capped, capPoints);

    /// <summary>
    /// Create a line info with "CappedExtended" LineCapType.
    /// </summary>
    public static LineDrawingInfo CappedExtended(float thickness, ColorRgba color, int capPoints) =>
        new(thickness, color, LineCapType.CappedExtended, capPoints);

    /// <summary>
    /// Create a line info with "Extended" LineCapType.
    /// </summary>
    public static LineDrawingInfo Extended(float thickness, ColorRgba color) => 
        new(thickness, color, LineCapType.Extended, 0);

    /// <summary>
    /// Create a line info with "None" LineCapType.
    /// </summary>
    public static LineDrawingInfo None(float thickness, ColorRgba color) => 
        new(thickness, color, LineCapType.None, 0);
    
    
    public LineDrawingInfo(float thickness, ColorRgba color)
    {
        Thickness = MathF.Max(thickness, LineMinThickness);
        Color = color;
        CapType = LineCapType.None;
        CapPoints = 0;
    }
    public LineDrawingInfo(float thickness, ColorRgba color, LineCapType capType, int capPoints)
    {
        Thickness = MathF.Max(thickness, LineMinThickness);
        Color = color;
        CapType = capType;
        CapPoints = capPoints;
    }

    
    public LineDrawingInfo ChangeThickness(float newThickness) => new(newThickness, Color, CapType, CapPoints);
    public LineDrawingInfo ChangeColor(ColorRgba newColor) => new(Thickness, newColor, CapType, CapPoints);
    public LineDrawingInfo ChangeCapType(LineCapType newCapType) => new(Thickness, Color, newCapType, CapPoints);
    public LineDrawingInfo ChangeCapPoints(int newCapPoints) => new(Thickness, Color, CapType, newCapPoints);

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