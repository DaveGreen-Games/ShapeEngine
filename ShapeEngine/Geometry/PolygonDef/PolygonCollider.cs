using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;

namespace ShapeEngine.Geometry.PolygonDef;

/// <summary>
/// Represents a polygon collider for collision detection, using a set of relative points.
/// </summary>
/// <remarks>
/// The polygon shape is recalculated based on the current transform.
/// The relative points define the shape in local space.
/// </remarks>
public class PolygonCollider : Collider
{
    /// <summary>
    /// The polygon shape defined in local (relative) space.
    /// </summary>
    public Polygon RelativeShape;
    /// <summary>
    /// The polygon shape in world (absolute) space.
    /// </summary>
    private readonly Polygon shape;
    
   
    /// <summary>
    /// Initializes a new instance of <see cref="PolygonCollider"/> with a set of relative points.
    /// </summary>
    /// <param name="offset">The transform offset for the collider.</param>
    /// <param name="relativePoints">The points defining the polygon in local space.</param>
    public PolygonCollider(Transform2D offset, Points relativePoints) : base(offset)
    {
        RelativeShape = relativePoints.ToPolygon();
        shape = new(RelativeShape.Count);
    }
    /// <summary>
    /// Initializes a new instance of <see cref="PolygonCollider"/> with a relative polygon.
    /// </summary>
    /// <param name="offset">The transform offset for the collider.</param>
    /// <param name="relativePoints">The polygon in local space.</param>
    public PolygonCollider(Transform2D offset, Polygon relativePoints) : base(offset)
    {
        RelativeShape = relativePoints;
        shape = new(RelativeShape.Count);
    }
    /// <summary>
    /// Initializes a new instance of <see cref="PolygonCollider"/> from a segment, inflating it to a polygon.
    /// </summary>
    /// <param name="offset">The transform offset for the collider.</param>
    /// <param name="s">The segment to inflate.</param>
    /// <param name="inflation">The amount to inflate the segment.</param>
    /// <param name="alignment">The alignment of the inflated polygon (0=start, 1=end, 0.5=center).</param>
    public PolygonCollider(Transform2D offset, Segment s, float inflation, float alignment = 0.5f) : base(offset)
    {
        shape = s.Inflate(inflation, alignment).ToPolygon();
        RelativeShape = new(shape.Count);
    }
    /// <summary>
    /// Called when the collider is initialized.
    /// </summary>
    protected override void OnInitialized()
    {
        if (RelativeShape.Count <= 0 && shape.Count > 0)
        {
            RelativeShape = shape.ToRelativePolygon(CurTransform);
        }
        else RecalculateShape();
    }
    /// <summary>
    /// Recalculates the absolute polygon shape based on the current transform.
    /// </summary>
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
    /// <summary>
    /// Called when the shape's transform changes.
    /// </summary>
    /// <param name="transformChanged">Indicates if the transform has changed.</param>
    protected override void OnShapeTransformChanged(bool transformChanged)
    {
        if (!transformChanged) return;
        RecalculateShape();
    }
    /// <summary>
    /// Gets the bounding box of the polygon collider.
    /// </summary>
    /// <returns>The bounding <see cref="Rect"/>.</returns>
    public override Rect GetBoundingBox() => GetPolygonShape().GetBoundingBox();
    /// <summary>
    /// Gets the shape type for this collider.
    /// </summary>
    /// <returns><see cref="ShapeType.Poly"/></returns>
    public override ShapeType GetShapeType() => ShapeType.Poly;
    /// <summary>
    /// Gets the polygon shape in world (absolute) space.
    /// </summary>
    /// <returns>The <see cref="Polygon"/> shape.</returns>
    public override Polygon GetPolygonShape() => shape;
}
