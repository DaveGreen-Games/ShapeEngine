using ShapeEngine.Geometry.TriangulationDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry;

/// <summary>
/// Contains the results of a fracture operation, including new shapes, cutouts, and triangulated pieces.
/// </summary>
/// <remarks>
/// This class is used to encapsulate the output of <see cref="FractureHelper.Fracture"/>.
/// </remarks>
public class FractureInfo
{
    /// <summary>
    /// The resulting shapes after the fracture operation (difference between original and cut shapes).
    /// </summary>
    public readonly Polygons NewShapes;

    /// <summary>
    /// The cutout polygons (intersection between original and cut shapes).
    /// </summary>
    public readonly Polygons Cutouts;

    /// <summary>
    /// The triangulated pieces generated from the cutouts.
    /// </summary>
    public readonly Triangulation Pieces;

    /// <summary>
    /// Initializes a new instance of the <see cref="FractureInfo"/> class.
    /// </summary>
    /// <param name="newShapes">The resulting shapes after the fracture operation.</param>
    /// <param name="cutouts">The cutout polygons from the fracture operation.</param>
    /// <param name="pieces">The triangulated pieces generated from the cutouts.</param>
    public FractureInfo(Polygons newShapes, Polygons cutouts, Triangulation pieces)
    {
        this.NewShapes = newShapes;
        this.Cutouts = cutouts;
        this.Pieces = pieces;
    }
}