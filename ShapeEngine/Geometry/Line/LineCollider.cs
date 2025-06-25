using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.Rect;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.Line;

/// <summary>
/// Represents an infinite line collider for collision detection.
/// </summary>
/// <remarks>
/// The line is defined by a point and a direction vector.
/// The collider uses the current transform to determine its position and orientation.
/// </remarks>
public class LineCollider(Transform2D offset) : Collider(offset)
{
    /// <summary>
    /// Gets the direction vector of the line, based on the current rotation.
    /// </summary>
    public Vector2 Direction => ShapeVec.VecFromAngleRad(CurTransform.RotationRad);
    /// <summary>
    /// Gets the point on the line, based on the current position.
    /// </summary>
    public Vector2 Point => CurTransform.Position;
    /// <summary>
    /// Returns the <see cref="Line"/> shape represented by this collider.
    /// </summary>
    /// <returns>The <see cref="Line"/> shape.</returns>
    public override Geometry.Line.Line GetLineShape() => new(Point, Direction);
    /// <summary>
    /// Gets the shape type for this collider.
    /// </summary>
    /// <returns><see cref="ShapeType.Line"/></returns>
    public override ShapeType GetShapeType() => ShapeType.Line;
    /// <summary>
    /// Gets the bounding box for this line collider. Returns the bounding box of the corresponding ray shape.
    /// </summary>
    /// <returns>The bounding <see cref="Rect"/>.</returns>
    public override Rect.Rect GetBoundingBox() => GetRayShape().GetBoundingBox();
}