using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.CollisionSystem;

/// <summary>
/// Represents a polyline collider for collision detection, using a set of relative points.
/// </summary>
/// <remarks>
/// The polyline shape is recalculated based on the current transform.
/// The relative points define the shape in local space.
/// </remarks>
public class PolyLineCollider : Collider
{
    /// <summary>
    /// The polyline shape defined in local (relative) space.
    /// </summary>
    public Polyline RelativeShape;
    /// <summary>
    /// The polyline shape in world (absolute) space.
    /// </summary>
    private Polyline shape;
    
   
    /// <summary>
    /// Initializes a new instance of <see cref="PolyLineCollider"/> with a set of relative points.
    /// </summary>
    /// <param name="offset">The transform offset for the collider.</param>
    /// <param name="relativePoints">The points defining the polyline in local space.</param>
    public PolyLineCollider(Transform2D offset, Points relativePoints) : base(offset)
    {
        RelativeShape = relativePoints.ToPolyline();
        shape = new(RelativeShape.Count);
    }
    /// <summary>
    /// Initializes a new instance of <see cref="PolyLineCollider"/> with a relative polyline.
    /// </summary>
    /// <param name="offset">The transform offset for the collider.</param>
    /// <param name="relativePoints">The polyline in local space.</param>
    public PolyLineCollider(Transform2D offset, Polyline relativePoints) : base(offset)
    {
        RelativeShape = relativePoints;
        shape = new(RelativeShape.Count);
    }
    

    /// <summary>
    /// Called when the collider is initialized.
    /// </summary>
    protected override void OnInitialized()
    {
        RecalculateShape();
    }
    /// <summary>
    /// Recalculates the absolute polyline shape based on the current transform.
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
    /// Gets the bounding box of the polyline collider.
    /// </summary>
    /// <returns>The bounding <see cref="Rect"/>.</returns>
    public override Rect GetBoundingBox() => GetPolygonShape().GetBoundingBox();
    /// <summary>
    /// Gets the shape type for this collider.
    /// </summary>
    /// <returns><see cref="ShapeType.PolyLine"/></returns>
    public override ShapeType GetShapeType() => ShapeType.PolyLine;
    /// <summary>
    /// Gets the polyline shape in world (absolute) space.
    /// </summary>
    /// <returns>The <see cref="Polyline"/> shape.</returns>
    public override Polyline GetPolylineShape() => shape;
   
}
