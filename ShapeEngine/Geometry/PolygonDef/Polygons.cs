using System.Numerics;
using ShapeEngine.Geometry.PointsDef;

namespace ShapeEngine.Geometry.PolygonDef;

/// <summary>
/// Represents a collection of <see cref="Polygon"/> objects.
/// </summary>
public class Polygons : List<Polygon>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Polygons"/> class.
    /// </summary>
    public Polygons() { }
    /// <summary>
    /// Initializes a new instance of the <see cref="Polygons"/> class with the specified capacity.
    /// </summary>
    /// <param name="capacity">The number of elements that the new list can initially store.</param>
    public Polygons(int capacity) : base(capacity) { }
    /// <summary>
    /// Initializes a new instance of the <see cref="Polygons"/> class containing the specified polygons.
    /// </summary>
    /// <param name="polygons">An array of polygons to add.</param>
    public Polygons(params Polygon[] polygons) { AddRange(polygons); }
    /// <summary>
    /// Initializes a new instance of the <see cref="Polygons"/> class containing the specified polygons.
    /// </summary>
    /// <param name="polygons">An enumerable collection of polygons to add.</param>
    public Polygons(IEnumerable<Polygon> polygons) { AddRange(polygons); }

    #region Convex Hull
    
    private static Points pointsBuffer = new();
    
    /// <summary>
    /// Computes the convex hull from all points contained in the polygons in this collection.
    /// </summary>
    /// <returns>
    /// A <see cref="Points"/> instance containing the convex hull points.
    /// </returns>
    public Points FindConvexHull()
    {
        pointsBuffer.Clear();
        foreach(var poly in this)
        {
            pointsBuffer.AddRange(poly);
        }
        return pointsBuffer.FindConvexHull();
    }
    
    /// <summary>
    /// Computes the convex hull from all points contained in the polygons in this collection
    /// and stores the resulting hull points in the provided list.
    /// </summary>
    /// <param name="result">The list to populate with the convex hull points.</param>
    public void FindConvexHull(List<Vector2> result)
    {
        pointsBuffer.Clear();
        foreach(var poly in this)
        {
            pointsBuffer.AddRange(poly);
        }
        pointsBuffer.FindConvexHull(result);
    }
    #endregion
}