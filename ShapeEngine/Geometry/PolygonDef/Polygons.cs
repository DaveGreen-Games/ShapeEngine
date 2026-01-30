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
}