using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.Shapes;

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
    /// </summary>
    public AnchorPoint Alignement { get; set; }
        
    /// <summary>
    /// Initializes a new instance of the <see cref="RectShape"/> class with a default alignment.
    /// </summary>
    /// <param name="offset">The transform offset to apply to the shape.</param>
    public RectShape(Transform2D offset)
    {
        Offset = offset;
        Alignement = new();
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="RectShape"/> class with a specified alignment.
    /// </summary>
    /// <param name="offset">The transform offset to apply to the shape.</param>
    /// <param name="alignement">The anchor point for alignment.</param>
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