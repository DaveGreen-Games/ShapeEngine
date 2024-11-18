using System.Numerics;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;
using ShapeEngine.UI;

namespace ShapeEngine.Core.CollisionSystem;


public class Collision
{
    public readonly bool FirstContact;
    public readonly Collider Self;
    public readonly Collider Other;
    public readonly Vector2 SelfVel;
    public readonly Vector2 OtherVel;
    public readonly CollisionPoints? Points = null;
    public Overlap Overlap => new(Self, Other, FirstContact);
    
    public Collision(Collider self, Collider other, bool firstContact)
    {
        Self = self;
        Other = other;
        SelfVel = self.Velocity;
        OtherVel = other.Velocity;
        FirstContact = firstContact;
        Points = null;
    }
    public Collision(Collider self, Collider other, bool firstContact, CollisionPoints? collisionPoints)
    {
        Self = self;
        Other = other;
        SelfVel = self.Velocity;
        OtherVel = other.Velocity;
        FirstContact = firstContact;
        if (collisionPoints == null) Points = null;
        else if (collisionPoints.Count <= 0) Points = null;
        else Points = collisionPoints;
        
    }
    private Collision(Collision collision)
    {
        Self = collision.Self;
        Other = collision.Other;
        SelfVel = collision.SelfVel;
        OtherVel = collision.OtherVel;
        FirstContact = collision.FirstContact;
        
        Points = collision.Points == null ? null : collision.Points.Copy();
        
    }
    
    public Collision Copy() => new(this);

    
    /// <summary>
    /// Validates the collision points using SelfVel as reference direction and Self.CurTransform.Position as reference point
    /// </summary>
    /// <param name="combined">The average CollisionPoint of all valid collision points.</param>
    /// <returns>Returns true if there are valid collision points left.</returns>
    public bool ValidateCollisionPoints(out CollisionPoint combined)
    {
        if (Points == null || Points.Count <= 0)
        {
            combined = new CollisionPoint();
            return false;
        }
        return Points.Validate(SelfVel, Self.CurTransform.Position, out combined);;
        
    }
    /// <summary>
    /// Validates the collision points using SelfVel as reference direction and Self.CurTransform.Position as reference point
    /// </summary>
    /// <param name="combined">The average CollisionPoint of all valid collision points.</param>
    /// <param name="closest">The closest CollisionPoint to the reference point (Self.CurTransform.Position)</param>
    /// <returns>Returns true if there are valid collision points left.</returns>
    public bool ValidateCollisionPoints(out CollisionPoint combined, out CollisionPoint closest)
    {
        if (Points == null || Points.Count <= 0)
        {
            combined = new CollisionPoint();
            closest = new CollisionPoint(); 
            return false;
        }
        
        return Points.Validate(SelfVel, Self.CurTransform.Position, out combined, out closest);
    }
    /// <summary>
    /// Validates the collision points using SelfVel as reference direction and Self.CurTransform.Position as reference point
    /// </summary>
    /// <param name="result">A combination of useful collision points.</param>
    /// <returns>Returns true if there are valid collision points left.</returns>
    public bool ValidateCollisionPoints(out CollisionPointValidationResult result)
    {
        if (Points == null || Points.Count <= 0)
        {
            result = new CollisionPointValidationResult();
            return false;
        }
        
        return Points.Validate(SelfVel, Self.CurTransform.Position, out result);
    }
   
}


/*public CollisionPoint GetClosestCollisionPoint(Vector2 referencePoint)
{
    if (Intersection == null) return new();
    return Intersection.GetClosestCollisionPoint(referencePoint);
}
/// <summary>
/// Finds the collision point with the normal facing most towards the reference direction.
/// </summary>
/// <returns></returns>
public CollisionPoint GetCollisionPointFacingTowardsDir(Vector2 referenceDir)
{
    if (Intersection == null) return new();
    return Intersection.GetCollisionPointFacingTowardsDir(referenceDir);
}
/// <summary>
/// Finds the collision point with the normal facing most towards the reference point.
/// The direction used is from each collision point towards the reference point.
/// </summary>
/// <returns></returns>
public CollisionPoint GetCollisionPointFacingTowardsPoint(Vector2 referencePoint)
{
    if (Intersection == null) return new();
    return Intersection.GetCollisionPointFacingTowardsPoint(referencePoint);
}*/

