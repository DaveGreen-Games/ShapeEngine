namespace ShapeEngine.Geometry.PolylineDef;

/// <summary>
/// Represents a collection of <see cref="Polyline"/> objects.
/// </summary>
public class Polylines : List<Polyline>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Polylines"/> class.
    /// </summary>
    public Polylines() { }
    /// <summary>
    /// Initializes a new instance of the <see cref="Polylines"/> class containing the specified polylines.
    /// </summary>
    /// <param name="polylines">An array of polylines to add.</param>
    public Polylines(params Polyline[] polylines) { AddRange(polylines); }
    /// <summary>
    /// Initializes a new instance of the <see cref="Polylines"/> class containing the specified polylines.
    /// </summary>
    /// <param name="polylines">An enumerable collection of polylines to add.</param>
    public Polylines(IEnumerable<Polyline> polylines) { AddRange(polylines); }
}