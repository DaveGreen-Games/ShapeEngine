using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.RayDef;

/// <summary>
/// Represents a ray collider for collision detection.
/// </summary>
/// <remarks>
/// The ray is defined by a point and a direction vector.
/// The collider uses the current transform to determine its position and orientation.
/// </remarks>
public class RayCollider(Transform2D offset) : Collider(offset)
{
    /// <summary>
    /// Gets the direction vector of the ray, based on the current rotation.
    /// </summary>
    /// <remarks>
    /// The direction is calculated from the current transform's rotation in radians.
    /// </remarks>
    public Vector2 Direction => ShapeVec.VecFromAngleRad(CurTransform.RotationRad);

    /// <summary>
    /// Gets the point on the ray, based on the current position.
    /// </summary>
    /// <remarks>
    /// The point is taken from the current transform's position.
    /// </remarks>
    public Vector2 Point => CurTransform.Position;

    /// <summary>
    /// Returns the <see cref="Ray"/> shape represented by this collider.
    /// </summary>
    /// <returns>The <see cref="Ray"/> shape.</returns>
    /// <remarks>
    /// The ray is constructed using the current point and direction.
    /// </remarks>
    public override Ray GetRayShape() => new(Point, Direction);

    /// <summary>
    /// Gets the shape type for this collider.
    /// </summary>
    /// <returns><see cref="ShapeType.Ray"/></returns>
    /// <remarks>
    /// This collider always returns <see cref="ShapeType.Ray"/> as its shape type.
    /// </remarks>
    public override ShapeType GetShapeType() => ShapeType.Ray;

    /// <summary>
    /// Gets the bounding box for this ray collider.
    /// </summary>
    /// <returns>The bounding <see cref="Rect"/>.</returns>
    /// <remarks>
    /// The bounding box is calculated from the ray shape using its maximum length.
    /// </remarks>
    public override Rect GetBoundingBox() => GetRayShape().GetBoundingBox();
}