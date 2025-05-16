using ShapeEngine.StaticLib;

namespace ShapeEngine.Core.Shapes;

public class FractureHelper
{
    public float MinArea { get; set; }
    public float MaxArea { get; set; }
    public float KeepChance { get; set; }
    public float NarrowValue { get; set; }

    //public float DivisionChance { get; set; } = 0.5f;
    //public int MinDivisionCount { get; set; } = 3;
    //public int MaxDivisionCount { get; set; } = 9;

    public FractureHelper(float minArea, float maxArea, float keepChance = 0.5f, float narrowValue = 0.2f)
    {
        this.MinArea = minArea;
        this.MaxArea = maxArea;
        this.KeepChance = keepChance;
        this.NarrowValue = narrowValue;
    }

    public FractureInfo Fracture(Polygon shape, Polygon cutShape)
    {
        var cutOuts = ShapeClipper.Intersect(shape, cutShape).ToPolygons(true);
        var newShapes = ShapeClipper.Difference(shape, cutShape).ToPolygons(true);
        Triangulation pieces = new();
        foreach (var cutOut in cutOuts)
        {
            var fracturePieces = cutOut.Triangulate().Subdivide(MinArea, MaxArea, KeepChance, NarrowValue);
            pieces.AddRange(fracturePieces);
        }

        return new(newShapes, cutOuts, pieces);
    }
}