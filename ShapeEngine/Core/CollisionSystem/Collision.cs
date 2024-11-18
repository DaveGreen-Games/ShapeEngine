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

    // NOTE: Add convenience functions here for cleaning Points?
    // NOTE: Could source Position/Velocity from the members and use them automatically?
    // NOTE: Is there a good way to automate the cleaning of Points into a constructor that automatically sets Points to null if no points remain after cleaning?

    
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

    
}