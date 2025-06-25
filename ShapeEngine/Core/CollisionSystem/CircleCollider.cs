using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.Circle;
using ShapeEngine.Geometry.Rect;

namespace ShapeEngine.Core.CollisionSystem;

/// <summary>
/// Represents a circle collider for collision detection.
/// </summary>
/// <remarks>
/// The circle is defined by its center and radius, which are determined by the current transform.
/// </remarks>
public class CircleCollider : Collider
{
    /// <summary>
    /// Initializes a new instance of <see cref="CircleCollider"/> with a transform offset.
    /// </summary>
    /// <param name="offset">The transform offset for the collider.</param>
    public CircleCollider(Transform2D offset) : base(offset)
    {
        
    }
    /// <summary>
    /// Gets the bounding box of the circle collider.
    /// </summary>
    /// <returns>The bounding <see cref="Rect"/>.</returns>
    public override Rect GetBoundingBox() => GetCircleShape().GetBoundingBox();
    /// <summary>
    /// Gets the shape type for this collider.
    /// </summary>
    /// <returns><see cref="ShapeType.Circle"/></returns>
    public override ShapeType GetShapeType() => ShapeType.Circle;
    /// <summary>
    /// Gets the circle shape in world (absolute) space.
    /// </summary>
    /// <returns>The <see cref="Circle"/> shape.</returns>
    public override Circle GetCircleShape() => new(CurTransform.Position, CurTransform.ScaledSize.Radius);
}