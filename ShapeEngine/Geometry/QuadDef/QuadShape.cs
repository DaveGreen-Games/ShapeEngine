using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CollisionSystem;

namespace ShapeEngine.Geometry.QuadDef;

/// <summary>
/// Represents a quadrilateral (quad) shape that can be transformed and aligned using an anchor point.
/// </summary>
/// <remarks>
/// This class is used to define a quad in a transformed space.
/// The alignment can be customized using an <see cref="AnchorPoint"/>.
/// </remarks>
public class QuadShape : ShapeContainer
{
    /// <summary>
    /// Gets or sets the alignment anchor point for the quad.
    /// </summary>
    public AnchorPoint Alignement { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="QuadShape"/> class with a default alignment.
    /// </summary>
    /// <param name="offset">The transform offset to apply to the shape.</param>
    /// <remarks>Creates a quad shape with the default anchor point alignment.</remarks>
    public QuadShape(Transform2D offset)
    {
        Offset = offset;
        this.Alignement = new();
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="QuadShape"/> class with a specified alignment.
    /// </summary>
    /// <param name="offset">The transform offset to apply to the shape.</param>
    /// <param name="alignement">The anchor point for alignment.</param>
    /// <remarks>Creates a quad shape with the specified anchor point alignment.</remarks>
    public QuadShape(Transform2D offset, AnchorPoint alignement)
    {
        Offset = offset;
        this.Alignement = alignement;
    }
    /// <inheritdoc/>
    public override ShapeType GetShapeType() => ShapeType.Quad;
    /// <inheritdoc/>
    public override Quad GetQuadShape() => new (CurTransform.Position, CurTransform.ScaledSize, CurTransform.RotationRad, Alignement);
}