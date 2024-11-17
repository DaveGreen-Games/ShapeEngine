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
    public readonly Intersection? Intersection;
    public Overlap Overlap => new(Self, Other, FirstContact);
    
    //todo review usefullness
    public CollisionPoint FirstCollisionPoint => Intersection != null ? Intersection.FirstCollisionPoint : new();
    public CollisionPoint ClosestCollisionPoint => Intersection != null ? Intersection.Closest : new();
    public CollisionPoint FurthestCollisionPoint => Intersection != null ? Intersection.Furthest : new();

    
    public Collision(Collider self, Collider other, bool firstContact)
    {
        Self = self;
        Other = other;
        SelfVel = self.Velocity;
        OtherVel = other.Velocity;
        FirstContact = firstContact;
        Intersection = null;
    }
    public Collision(Collider self, Collider other, bool firstContact, CollisionPoints? collisionPoints, bool filterCollisionPoints = true)
    {
        Self = self;
        Other = other;
        SelfVel = self.Velocity;
        OtherVel = other.Velocity;
        FirstContact = firstContact;
        if (collisionPoints == null) Intersection = null;
        else if (collisionPoints.Count <= 0) Intersection = null;
        else
        {
            var refPoint =  self.CurTransform.Position;
            Intersection =  new(collisionPoints, SelfVel, refPoint, filterCollisionPoints);
            if(Intersection.Count <= 0) Intersection = null;
        }
        
    }

    private Collision(Collision collision)
    {
        Self = collision.Self;
        Other = collision.Other;
        SelfVel = collision.SelfVel;
        OtherVel = collision.OtherVel;
        FirstContact = collision.FirstContact;
        Intersection = collision.Intersection == null ? null : collision.Intersection.Copy();
    }
    
    public Collision Copy() => new(this);

    
   
    public CollisionPoint GetClosestCollisionPoint(Vector2 referencePoint)
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
    }

    
}


/*
/// <summary>
/// Finds the collision point with the normal facing most towards the parent of self.
/// </summary>
/// <returns></returns>
public CollisionPoint GetCollisionPointFacingTowardsParent()
{
    if (Intersection == null) return new();

    var parent = Self.Parent;
    return parent == null ? new() : Intersection.GetCollisionPointFacingTowardsPoint(parent.Transform.Position);
}

/// <summary>
/// Finds the collision point with the normal facing most towards self.
/// </summary>
/// <returns></returns>
public CollisionPoint GetCollisionPointFacingTowardsSelf()
{
    if (Intersection == null) return new();
    return  Intersection.GetCollisionPointFacingTowardsPoint(Self.CurTransform.Position);
}
*/
