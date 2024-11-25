using System.Numerics;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.CollisionSystem;

public class IntersectSpaceEntry : CollisionPoints
{
    public readonly Collider OtherCollider;
    public readonly Vector2 OtherVel;

    public IntersectSpaceEntry(Collider otherCollider, int capacity) : base(capacity)
    {
        OtherCollider = otherCollider;
        OtherVel = otherCollider.Velocity;
    }
    public IntersectSpaceEntry(Collider otherCollider, List<CollisionPoint> points) : base(points.Count)
    {
        OtherCollider = otherCollider;
        OtherVel = otherCollider.Velocity;
        AddRange(points);
    }
    
    
    #region Pointing Towards
    
    public CollisionPoint GetCollisionPointFacingTowardsPoint()
    {
        return GetCollisionPointFacingTowardsPoint(OtherCollider.CurTransform.Position);
    }
    public CollisionPoint GetCollisionPointFacingTowardsDir()
    {
        return GetCollisionPointFacingTowardsDir(OtherVel);
    }

    #endregion
    
    #region Validation
    
    /// <summary>
    /// Removes:
    /// - invalid CollisionPoints
    /// - CollisionPoints with normals facing in the same direction as the reference direction
    /// - CollisionPoints with normals facing in the opposite direction as the reference point (from CollisionPoint towards the reference point)
    /// - Uses Collider.Transform.Position as reference point and OtherVel as reference direction
    /// </summary>
    /// <param name="combined">An averaged CollisionPoint of all remaining CollisionPoints.</param>
    /// <param name="closest">The CollisionPoint that is closest to the referencePoint.</param>
    /// <returns>Returns true if there are valid points remaining</returns>
    public bool ValidateByOther( out CollisionPoint combined, out CollisionPoint closest)
    {
        return Validate(OtherVel, OtherCollider.CurTransform.Position, out combined, out closest);
    }
    /// <summary>
    /// Removes:
    /// - invalid CollisionPoints
    /// - CollisionPoints with normals facing in the same direction as the reference direction
    /// - CollisionPoints with normals facing in the opposite direction as the reference point (from CollisionPoint towards the reference point)
    /// - Uses Collider.Transform.Position as reference point and OtherVel as reference direction
    /// </summary>
    /// <param name="validationResult">The result of the combined CollisionPoint, and the  closest/furthest collision point from the reference point, and the CollisionPoint with normal facing towards the referencePoint.</param>
    /// <returns>Returns true if there are valid points remaining</returns>
    public bool ValidateByOther(out CollisionPointValidationResult validationResult)
    {
        return Validate(OtherVel, OtherCollider.CurTransform.Position, out validationResult);
    }
   
    #endregion
}