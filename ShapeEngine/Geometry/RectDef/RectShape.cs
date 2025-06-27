using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CollisionSystem;

namespace ShapeEngine.Geometry.RectDef;

/// <summary>
/// Represents a rectangle shape that can be transformed and aligned using an anchor point.
/// </summary>
/// <remarks>
/// This class is used to define a rectangle in a transformed space.
/// The alignment can be customized using an <see cref="AnchorPoint"/>.
/// </remarks>
public class RectShape : ShapeContainer
{
    /// <summary>
    /// Gets or sets the alignment anchor point for the rectangle.
    /// Determines how the rectangle is positioned relative to its transform.
    /// </summary>
    /// <remarks>
    /// The <see cref="AnchorPoint"/> specifies the reference point for alignment, such as center, top-left, etc.
    /// </remarks>
    public AnchorPoint Alignement { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="RectShape"/> class with a default alignment.
    /// </summary>
    /// <param name="offset">The <see cref="Transform2D"/> offset to apply to the shape, specifying position, rotation, and scale.</param>
    /// <remarks>
    /// The alignment anchor will be set to its default value.
    /// </remarks>
    public RectShape(Transform2D offset)
    {
        Offset = offset;
        Alignement = new();
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="RectShape"/> class with a specified alignment.
    /// </summary>
    /// <param name="offset">The <see cref="Transform2D"/> offset to apply to the shape, specifying position, rotation, and scale.</param>
    /// <param name="alignement">The <see cref="AnchorPoint"/> to use for alignment, determining how the rectangle is anchored.</param>
    /// <remarks>
    /// Use this constructor to explicitly set the alignment anchor point.
    /// </remarks>
    public RectShape(Transform2D offset, AnchorPoint alignement)
    {
        Offset = offset;
        Alignement = alignement;
    }
    /// <inheritdoc/>
    public override ShapeType GetShapeType() => ShapeType.Rect;
    /// <inheritdoc/>
    public override Rect GetRectShape() => new(CurTransform.Position, CurTransform.ScaledSize, Alignement);
}