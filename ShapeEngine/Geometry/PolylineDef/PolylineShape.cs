using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.PointsDef;

namespace ShapeEngine.Geometry.PolylineDef;

/// <summary>
/// Represents a polyline shape that can be transformed and recalculated based on a relative set of points.
/// </summary>
/// <remarks>
/// This class is used to define a polyline (a series of connected line segments) in a transformed space.
/// The shape is recalculated whenever the transform changes.
/// </remarks>
public class PolylineShape : ShapeContainer
{
    /// <summary>
    /// The polyline defined in the local (relative) coordinate space.
    /// </summary>
    public readonly Polyline RelativeShape;
    /// <summary>
    /// The polyline in world (absolute) coordinates after applying the current transform.
    /// </summary>
    private readonly Polyline shape;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="PolylineShape"/> class using relative points.
    /// </summary>
    /// <param name="offset">The transform offset to apply to the shape.</param>
    /// <param name="relativePoints">The points that define the polyline in local space.</param>
    public PolylineShape(Transform2D offset, Points relativePoints)
    {
        if (relativePoints.Count < 2) throw new ArgumentException("A polyline must have at least 2 points.");
        Offset = offset;
        RelativeShape = relativePoints.ToPolyline();
        shape = new(RelativeShape.Count);
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="PolylineShape"/> class using a relative polyline.
    /// </summary>
    /// <param name="offset">The transform offset to apply to the shape.</param>
    /// <param name="relativePoints">The polyline defined in local space.</param>
    public PolylineShape(Transform2D offset, Polyline relativePoints)
    {
        if (relativePoints.Count < 2) throw new ArgumentException("A polyline must have at least 2 points.");
        Offset = offset;
        RelativeShape = relativePoints;
        shape = new(RelativeShape.Count);
    }
    
    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        RecalculateShape();
    }
    /// <inheritdoc/>
    public override void RecalculateShape()
    {
        for (int i = 0; i < RelativeShape.Count; i++)
        {
            var p = CurTransform.ApplyTransformTo(RelativeShape[i]);
            if (shape.Count <= i)
            {
                shape.Add(p);
            }
            else
            {
                shape[i] = p;
            }
        }
    }
    /// <inheritdoc/>
    protected override void OnShapeTransformChanged(bool transformChanged)
    {
        if (!transformChanged) return;
        RecalculateShape();
    }
    /// <inheritdoc/>
    public override ShapeType GetShapeType() => ShapeType.PolyLine;
    /// <inheritdoc/>
    public override Polyline GetPolylineShape() => shape;
   
}