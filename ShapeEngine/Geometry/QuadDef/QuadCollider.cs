using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.RectDef;

namespace ShapeEngine.Geometry.QuadDef;
/// <summary>
/// Represents a quadrilateral (quad) collider used for collision detection in 2D space.
/// </summary>
/// <remarks>
/// The quad's position, size, and rotation are determined by the current transform (<see cref="Collider.CurTransform"/>).
/// Alignment is controlled by an anchor point, allowing flexible positioning relative to the transform.
/// </remarks>
public class QuadCollider : Collider
{
    /// <summary>
    /// Gets or sets the alignment anchor point for the quad.
    /// </summary>
    /// <remarks>
    /// The alignment determines how the quad is positioned relative to its transform.
    /// </remarks>
    public AnchorPoint Alignement { get; set; }
    /// <summary>
    /// Initializes a new instance of <see cref="QuadCollider"/> with a transform offset.
    /// </summary>
    /// <param name="offset">The transform offset for the collider.
    /// This determines the collider's position, rotation,
    /// and scale relative to its parent.</param>
    /// <remarks>
    /// The alignment will default to <see cref="AnchorPoint"/>'s default value.
    /// </remarks>
    public QuadCollider(Transform2D offset) : base(offset)
    {
        this.Alignement = new();
    }
    /// <summary>
    /// Initializes a new instance of <see cref="QuadCollider"/> with a transform offset and alignment.
    /// </summary>
    /// <param name="offset">The transform offset for the collider.
    /// This determines the collider's position, rotation, and scale relative to its parent.</param>
    /// <param name="alignement">The alignment anchor point.
    /// Determines how the quad is positioned relative to its transform.</param>
    public QuadCollider(Transform2D offset, AnchorPoint alignement) : base(offset)
    {
        this.Alignement = alignement;
    }
    /// <summary>
    /// Gets the bounding box of the quad collider in world space.
    /// </summary>
    /// <returns>The bounding <see cref="Rect"/> that fully contains the quad.</returns>
    /// <remarks>
    /// The bounding box is axis-aligned and may be larger than the quad if the quad is rotated.
    /// </remarks>
    public override Rect GetBoundingBox() => GetQuadShape().GetBoundingBox();
    /// <summary>
    /// Gets the shape type for this collider.
    /// </summary>
    /// <returns><see cref="ShapeType.Quad"/> indicating this collider is a quad.</returns>
    public override ShapeType GetShapeType() => ShapeType.Quad;
    /// <summary>
    /// Gets the quad shape in world (absolute) space, using the current transform and alignment.
    /// </summary>
    /// <returns>The <see cref="Quad"/> shape representing this collider in world space.</returns>
    /// <remarks>
    /// The quad is constructed from the current transform's position, scaled size, rotation, and the alignment anchor point.
    /// </remarks>
    public override Quad GetQuadShape() => new Quad(CurTransform.Position, CurTransform.ScaledSize, CurTransform.RotationRad, Alignement);
}