using ShapeEngine.StaticLib;

namespace ShapeEngine.Core.Shapes;

public class FractureInfo
{
    public readonly Polygons NewShapes;
    public readonly Polygons Cutouts;
    public readonly Triangulation Pieces;

    public FractureInfo(Polygons newShapes, Polygons cutouts, Triangulation pieces)
    {
        this.NewShapes = newShapes;
        this.Cutouts = cutouts;
        this.Pieces = pieces;
    }
}