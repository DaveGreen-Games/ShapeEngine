using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CollisionSystem;

namespace ShapeEngine.Geometry.RectDef;

/// <summary>
/// Represents a rectangle collider for collision detection.
/// </summary>
/// <remarks>
/// The rectangle is defined by its position, size, and alignment.
/// The collider uses the current transform to determine its position and orientation.
/// </remarks>
public class RectCollider : Collider
{
    /// <summary>
    /// Gets or sets the alignment anchor point for the rectangle.
    /// </summary>
    public AnchorPoint Alignment { get; set; }
    /// <summary>
    /// Initializes a new instance of <see cref="RectCollider"/> with a transform offset.
    /// </summary>
    /// <param name="offset">The transform offset for the collider.</param>
    public RectCollider(Transform2D offset) : base(offset)
    {
        Alignment = new();
    }
    /// <summary>
    /// Initializes a new instance of <see cref="RectCollider"/> with a transform offset and alignment.
    /// </summary>
    /// <param name="offset">The transform offset for the collider.</param>
    /// <param name="alignment">The alignment anchor point.</param>
    public RectCollider(Transform2D offset, AnchorPoint alignment) : base(offset)
    {
        Alignment = alignment;
    }
    /// <summary>
    /// Gets the bounding box of the rectangle collider.
    /// </summary>
    /// <returns>The bounding <see cref="Rect"/>.</returns>
    public override Rect GetBoundingBox() => GetRectShape();
    /// <summary>
    /// Gets the shape type for this collider.
    /// </summary>
    /// <returns><see cref="ShapeType.Rect"/></returns>
    public override ShapeType GetShapeType() => ShapeType.Rect;
    /// <summary>
    /// Gets the rectangle shape in world (absolute) space.
    /// </summary>
    /// <returns>The <see cref="Rect"/> shape.</returns>
    public override Rect GetRectShape() => new(CurTransform.Position, CurTransform.ScaledSize, Alignment);
}