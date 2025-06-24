using System.Numerics;

namespace ShapeEngine.Core.Structs;

/// <summary>
/// Represents conversion factors between two <see cref="Dimensions"/>.
/// Provides scale factors for length and area conversions, as well as the source and target dimensions.
/// </summary>
public readonly struct DimensionConversionFactors
{
    /// <summary>
    /// Indicates whether the conversion factors are valid.
    /// </summary>
    public readonly bool Valid;

    /// <summary>
    /// The scale factor for converting lengths from <see cref="From"/> to <see cref="To"/>.
    /// </summary>
    public readonly Vector2 Factor;

    /// <summary>
    /// The scale factor for converting areas from <see cref="From"/> to <see cref="To"/>.
    /// </summary>
    public readonly float AreaFactor;

    /// <summary>
    /// The scale factor for converting the side of an area from <see cref="From"/> to <see cref="To"/>.
    /// </summary>
    public readonly float AreaSideFactor;

    /// <summary>
    /// The source dimensions.
    /// </summary>
    public readonly Dimensions From;

    /// <summary>
    /// The target dimensions.
    /// </summary>
    public readonly Dimensions To;
    
    /// <summary>
    /// Initializes a new instance of <see cref="DimensionConversionFactors"/> with default values and sets <see cref="Valid"/> to false.
    /// </summary>
    public DimensionConversionFactors()
    {
        Factor = new(1);
        AreaFactor = 1f;
        AreaSideFactor = 1f;
        From = new();
        To = new();
        Valid = false;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="DimensionConversionFactors"/> using the specified source and target dimensions.
    /// Calculates the scale factors and sets <see cref="Valid"/> to true.
    /// </summary>
    /// <param name="from">The source dimensions.</param>
    /// <param name="to">The target dimensions.</param>
    public DimensionConversionFactors(Dimensions from, Dimensions to)
    {
        From = from;
        To = to;
        Factor = from.ScaleFactor(to);
        AreaFactor = from.ScaleFactorArea(to);
        AreaSideFactor = from.ScaleFactorAreaSide(to);
        Valid = true;
    }
}