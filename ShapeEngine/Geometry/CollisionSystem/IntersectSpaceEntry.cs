using System.Numerics;

namespace ShapeEngine.Geometry.CollisionSystem;

/// <summary>
/// Represents an entry in the intersection space, containing collision points and information about the other collider involved in the intersection.
/// </summary>
/// <remarks>
/// This class extends <see cref="IntersectionPoints"/> and is used to store collision points resulting from intersection tests with another collider.
/// It also stores the velocity of the other collider at the time of intersection.
/// </remarks>
public class IntersectSpaceEntry : IntersectionPoints
{
    /// <summary>
    /// The collider that this entry represents as the other collider in the intersection.
    /// </summary>
    public readonly Collider OtherCollider;
    /// <summary>
    /// The velocity of the other collider at the time of intersection.
    /// </summary>
    public readonly Vector2 OtherVel;

    /// <summary>
    /// Initializes a new instance of the <see cref="IntersectSpaceEntry"/> class with a specified collider and capacity for collision points.
    /// </summary>
    /// <param name="otherCollider">The other collider involved in the intersection.</param>
    /// <param name="capacity">The initial capacity for collision points.</param>
    public IntersectSpaceEntry(Collider otherCollider, int capacity) : base(capacity)
    {
        OtherCollider = otherCollider;
        OtherVel = otherCollider.Velocity;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="IntersectSpaceEntry"/> class with a specified collider and a list of collision points.
    /// </summary>
    /// <param name="otherCollider">The other collider involved in the intersection.</param>
    /// <param name="points">The list of collision points to add to this entry.</param>
    public IntersectSpaceEntry(Collider otherCollider, List<IntersectionPoint> points) : base(points.Count)
    {
        OtherCollider = otherCollider;
        OtherVel = otherCollider.Velocity;
        AddRange(points);
    }
    
    
    #region Pointing Towards
    
    /// <summary>
    /// Gets the intersection point whose normal is most closely facing towards the position of the other collider.
    /// </summary>
    /// <returns>The <see cref="IntersectionPoint"/> facing towards the other collider's position.</returns>
    public IntersectionPoint GetCollisionPointFacingTowardsPoint()
    {
        return GetCollisionPointFacingTowardsPoint(OtherCollider.CurTransform.Position);
    }
    /// <summary>
    /// Gets the intersection point whose normal is most closely facing towards the velocity direction of the other collider.
    /// </summary>
    /// <returns>The <see cref="IntersectionPoint"/> facing towards the other collider's velocity direction.</returns>
    public IntersectionPoint GetCollisionPointFacingTowardsDir()
    {
        return GetCollisionPointFacingTowardsDir(OtherVel);
    }

    #endregion
    
    #region Validation
    
    /// <summary>
    /// Validates the collision points in this entry using the other collider's velocity as the reference direction and its position as the reference point.
    /// </summary>
    /// <param name="combined">An averaged <see cref="IntersectionPoint"/> of all remaining valid collision points.</param>
    /// <param name="closest">The <see cref="IntersectionPoint"/> that is closest to the reference point.</param>
    /// <returns>Returns <c>true</c> if there are valid points remaining after validation; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Removes:
    /// - Invalid <see cref="IntersectionPoint"/>s
    /// - Points with normals facing in the same direction as the reference direction
    /// - Points with normals facing in the opposite direction as the reference point (from <see cref="IntersectionPoint"/> towards the reference point)
    /// </remarks>
    public bool ValidateByOther( out IntersectionPoint combined, out IntersectionPoint closest)
    {
        return Validate(OtherVel, OtherCollider.CurTransform.Position, out combined, out closest);
    }
    /// <summary>
    /// Validates the collision points in this entry using the other collider's velocity as the reference direction and its position as the reference point.
    /// </summary>
    /// <param name="validationResult">The result containing the combined <see cref="IntersectionPoint"/>, the closest and furthest points from the reference point, and the point with normal facing towards the reference point.</param>
    /// <returns>Returns <c>true</c> if there are valid points remaining after validation; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Removes:
    /// - Invalid <see cref="IntersectionPoint"/>s
    /// - Points with normals facing in the same direction as the reference direction
    /// - Points with normals facing in the opposite direction as the reference point (from <see cref="IntersectionPoint"/> towards the reference point)
    /// </remarks>
    public bool ValidateByOther(out CollisionPointValidationResult validationResult)
    {
        return Validate(OtherVel, OtherCollider.CurTransform.Position, out validationResult);
    }
   
    #endregion
}