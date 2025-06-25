using System.Numerics;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Geometry.Circle;

/// <summary>
/// Represents a shape container for a circle, providing access to its center and radius.
/// </summary>
/// <remarks>
/// The <see cref="CircleShape"/> class is used to wrap a circle shape with a transform offset and expose its geometric properties.
/// </remarks>
public class CircleShape : ShapeContainer
{
    /// <summary>
    /// Gets the radius of the circle (from the current transform's scaled size).
    /// </summary>
    public float Radius => CurTransform.ScaledSize.Radius;
    /// <summary>
    /// Gets the center of the circle (from the current transform's position).
    /// </summary>
    public Vector2 Center => CurTransform.Position;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="CircleShape"/> class with the given transform offset.
    /// </summary>
    /// <param name="offset">The transform offset to apply to the circle.</param>
    public CircleShape(Transform2D offset)
    {
        Offset = offset;
    }
    /// <summary>
    /// Gets the shape type for this container (always <see cref="ShapeType.Circle"/>).
    /// </summary>
    /// <returns>The <see cref="ShapeType.Circle"/> value.</returns>
    public override ShapeType GetShapeType() => ShapeType.Circle;
    /// <summary>
    /// Gets the <see cref="Circle"/> shape represented by this container.
    /// </summary>
    /// <returns>A <see cref="Circle"/> with the current position and radius.</returns>
    public override Circle GetCircleShape() => new(CurTransform.Position, CurTransform.ScaledSize.Radius);
    
}