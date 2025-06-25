using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.Rect;

namespace ShapeEngine.Geometry.Quad;

/// <summary>
/// Represents a quadrilateral (quad) collider for collision detection.
/// </summary>
/// <remarks>
/// The quad is defined by its position, size, rotation, and alignment.
/// The quad uses the <see cref="Collider.CurTransform"/> for its position, rotation, and size.
/// The collider uses the current transform to determine its position and orientation.
/// </remarks>
public class QuadCollider : Collider
{
    /// <summary>
    /// Gets or sets the alignment anchor point for the quad.
    /// </summary>
    public AnchorPoint Alignement { get; set; }
    /// <summary>
    /// Initializes a new instance of <see cref="QuadCollider"/> with a transform offset.
    /// </summary>
    /// <param name="offset">The transform offset for the collider.</param>
    public QuadCollider(Transform2D offset) : base(offset)
    {
        this.Alignement = new();
    }
    /// <summary>
    /// Initializes a new instance of <see cref="QuadCollider"/> with a transform offset and alignment.
    /// </summary>
    /// <param name="offset">The transform offset for the collider.</param>
    /// <param name="alignement">The alignment anchor point.</param>
    public QuadCollider(Transform2D offset, AnchorPoint alignement) : base(offset)
    {
        this.Alignement = alignement;
    }
    /// <summary>
    /// Gets the bounding box of the quad collider.
    /// </summary>
    /// <returns>The bounding <see cref="Rect"/>.</returns>
    public override Rect.Rect GetBoundingBox() => GetQuadShape().GetBoundingBox();
    /// <summary>
    /// Gets the shape type for this collider.
    /// </summary>
    /// <returns><see cref="ShapeType.Quad"/></returns>
    public override ShapeType GetShapeType() => ShapeType.Quad;
    /// <summary>
    /// Gets the quad shape in world (absolute) space.
    /// </summary>
    /// <returns>The <see cref="Quad"/> shape.</returns>
    public override Geometry.Quad.Quad GetQuadShape() => new Geometry.Quad.Quad(CurTransform.Position, CurTransform.ScaledSize, CurTransform.RotationRad, Alignement);
}