using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.TriangulationDef;
using ShapeEngine.ShapeClipper;

namespace ShapeEngine.Geometry;

/// <summary>
/// Helper class for fracturing polygons using specified area and randomness parameters.
/// </summary>
/// <remarks>
/// This class provides methods to fracture a polygon by cutting it with another polygon and subdividing the resulting pieces.
/// </remarks>
public class FractureHelper
{
    private static Triangulation buffer = new();
    private static Triangulation buffer2 = new();
    
    /// <summary>
    /// The minimum area for a fracture piece to be kept.
    /// </summary>
    public float MinArea { get; set; }

    /// <summary>
    /// The maximum area for a fracture piece to be kept.
    /// </summary>
    public float MaxArea { get; set; }

    /// <summary>
    /// The probability that a generated fracture piece will be kept.
    /// </summary>
    /// <remarks>
    /// Value should be between <c>0</c> (never keep) and <c>1</c> (always keep).
    /// </remarks>
    public float KeepChance { get; set; }

    /// <summary>
    /// Controls how narrow the generated fracture pieces can be.
    /// </summary>
    /// <remarks>
    /// Lower values allow for narrower pieces.
    /// </remarks>
    public float NarrowValue { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FractureHelper"/> class.
    /// </summary>
    /// <param name="minArea">The minimum area for a fracture piece to be kept.</param>
    /// <param name="maxArea">The maximum area for a fracture piece to be kept.</param>
    /// <param name="keepChance">The probability that a generated fracture piece will be kept. Default is 0.5.</param>
    /// <param name="narrowValue">Controls how narrow the generated fracture pieces can be. Default is 0.2.</param>
    public FractureHelper(float minArea, float maxArea, float keepChance = 0.5f, float narrowValue = 0.2f)
    {
        this.MinArea = minArea;
        this.MaxArea = maxArea;
        this.KeepChance = keepChance;
        this.NarrowValue = narrowValue;
    }
    
    /// <summary>
    /// Fractures a polygon by cutting it with another polygon and subdividing the resulting pieces.
    /// </summary>
    /// <param name="shape">The original polygon to be fractured.</param>
    /// <param name="cutShape">The polygon used to cut the original shape.</param>
    /// <param name="result">A <see cref="FractureInfo"/> object containing the new shapes, cutouts, and fracture pieces.</param>
     /// <remarks>
     /// <list type="bullet">
     ///   <item>
     ///     <description>The method first computes the intersection and difference between the original shape and the cut shape.</description>
     ///   </item>
     ///   <item>
     ///     <description>The intersection pieces are triangulated and subdivided according to the area and randomness parameters.</description>
     ///   </item>
     /// </list>
     /// </remarks>
    public void Fracture(Polygon shape, Polygon cutShape, FractureInfo result)
    {
        ClipperImmediate2D.ClipEngine.Execute(shape, cutShape, ShapeClipperClipType.Intersection, result.Cutouts);
        ClipperImmediate2D.ClipEngine.Execute(shape, cutShape, ShapeClipperClipType.Difference, result.NewShapes);
        
        result.Cutouts.RemoveAllHoles();
        result.NewShapes.RemoveAllHoles();
        
        result.Pieces.Clear();
        foreach (var cutOut in result.Cutouts)
        {
            buffer.Clear();
            buffer2.Clear();
            cutOut.Triangulate(buffer);
            buffer.Subdivide(buffer2, MinArea, MaxArea, KeepChance, NarrowValue);
            result.Pieces.AddRange(buffer2);
        }
    }
}