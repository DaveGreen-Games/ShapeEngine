using System.Numerics;

namespace ShapeEngine.Core.Structs;

public readonly struct DimensionConversionFactors
{
    public readonly bool Valid;
    public readonly Vector2 Factor;
    public readonly float AreaFactor;
    public readonly float AreaSideFactor;
    public readonly Dimensions From;
    public readonly Dimensions To;
    
    public DimensionConversionFactors()
    {
        Factor = new(1);
        AreaFactor = 1f;
        AreaSideFactor = 1f;
        From = new();
        To = new();
        Valid = false;
    }
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