using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.SegmentDef;

namespace ShapeEngine.Geometry.PolygonDef;

/// <summary>
/// Represents a polygon shape that can be transformed and recalculated based on a relative set of points or a segment.
/// </summary>
/// <remarks>
/// This class is used to define a polygon in a transformed space.
/// The shape is recalculated whenever the transform changes.
/// </remarks>
public class PolygonShape : ShapeContainer
{
    /// <summary>
    /// The polygon defined in the local (relative) coordinate space.
    /// </summary>
    public Polygon RelativeShape;
    /// <summary>
    /// The polygon in world (absolute) coordinates after applying the current transform.
    /// </summary>
    private readonly Polygon shape;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="PolygonShape"/> class using relative points.
    /// </summary>
    /// <param name="offset">The transform offset to apply to the shape.</param>
    /// <param name="relativePoints">The points that define the polygon in local space.</param>
    public PolygonShape(Transform2D offset, Points relativePoints)
    {
        if (relativePoints.Count < 3) throw new ArgumentException("A polygon must have at least 3 points.");
        Offset = offset;
        RelativeShape = relativePoints.ToPolygon();
        shape = new(RelativeShape.Count);
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="PolygonShape"/> class using a relative polygon.
    /// </summary>
    /// <param name="offset">The transform offset to apply to the shape.</param>
    /// <param name="relativePoints">The polygon defined in local space.</param>
    public PolygonShape(Transform2D offset, Polygon relativePoints)
    {
        if (relativePoints.Count < 3) throw new ArgumentException("A polygon must have at least 3 points.");
        Offset = offset;
        RelativeShape = relativePoints;
        shape = new(RelativeShape.Count);
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="PolygonShape"/> class by inflating a segment.
    /// </summary>
    /// <param name="offset">The transform offset to apply to the shape.</param>
    /// <param name="s">The segment to inflate into a polygon.</param>
    /// <param name="inflation">The amount to inflate the segment.</param>
    /// <param name="alignment">The alignment factor for inflation (default is 0.5).</param>
    /// <remarks>
    /// Uses <see cref="Segment.Inflate"/> function.
    /// </remarks>
    public PolygonShape(Transform2D offset, Segment s, float inflation, float alignment = 0.5f)
    {
        Offset = offset;
        shape = s.Inflate(inflation, alignment).ToPolygon();
        RelativeShape = new(shape.Count);
    }
    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        if (RelativeShape.Count <= 0 && shape.Count > 0)
        {
            RelativeShape = shape.ToRelativePolygon(CurTransform);
        }
        else RecalculateShape();
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
    public override ShapeType GetShapeType() => ShapeType.Poly;
    /// <inheritdoc/>
    public override Polygon GetPolygonShape() => shape;

}