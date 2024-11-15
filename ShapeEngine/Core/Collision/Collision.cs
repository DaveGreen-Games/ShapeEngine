using System.Numerics;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.CollisionSystem;

public class Collision
{
    public readonly bool FirstContact;
    public readonly Collider Self;
    public readonly Collider Other;
    public readonly Vector2 SelfVel;
    public readonly Vector2 OtherVel;
    public readonly Intersection Intersection;
    
    public CollisionPoint FirstCollisionPoint => Intersection.Valid ? Intersection.FirstCollisionPoint : new();
    public CollisionPoint ClosestCollisionPoint => Intersection.Valid ? Intersection.Closest : new();
    public CollisionPoint FurthestCollisionPoint => Intersection.Valid ? Intersection.Furthest : new();

    
    public Collision(Collider self, Collider other, bool firstContact)
    {
        this.Self = self;
        this.Other = other;
        this.SelfVel = self.Velocity;
        this.OtherVel = other.Velocity;
        this.Intersection = new();
        this.FirstContact = firstContact;
    }
    public Collision(Collider self, Collider other, bool firstContact, CollisionPoints? collisionPoints)
    {
        this.Self = self;
        this.Other = other;
        this.SelfVel = self.Velocity;
        this.OtherVel = other.Velocity;
        this.Intersection = new(collisionPoints, SelfVel, self.CurTransform.Position);
        this.FirstContact = firstContact;
    }

    
    public CollisionPoint GetClosestCollisionPointToParent()
    {
        if (!Intersection.Valid) return new();
        var parent = Self.Parent;
        return parent == null ? new() : Intersection.GetClosestCollisionPoint(parent.Transform.Position);
    }
    public CollisionPoint GetClosestCollisionPoint()
    {
        if (!Intersection.Valid) return new();
        return Intersection.GetClosestCollisionPoint(Self.CurTransform.Position);
    }
    public CollisionPoint GetClosestCollisionPoint(Vector2 referencePoint)
    {
        if (!Intersection.Valid) return new();
        return Intersection.GetClosestCollisionPoint(referencePoint);
    }

    
    /// <summary>
    /// Finds the collision point with the normal facing most towards the reference direction.
    /// </summary>
    /// <returns></returns>
    public CollisionPoint GetCollisionPointFacingTowardsDir(Vector2 referenceDir)
    {
        if (!Intersection.Valid) return new();
        return Intersection.GetCollisionPointFacingTowardsDir(referenceDir);
    }
   
    /// <summary>
    /// Finds the collision point with the normal facing most towards the reference point.
    /// The direction used is from each collision point towards the reference point.
    /// </summary>
    /// <returns></returns>
    public CollisionPoint GetCollisionPointFacingTowardsPoint(Vector2 referencePoint)
    {
        if (!Intersection.Valid) return new();
        return Intersection.GetCollisionPointFacingTowardsPoint(referencePoint);
    }

    
    /// <summary>
    /// Finds the collision point with the normal facing most towards the parent of self.
    /// </summary>
    /// <returns></returns>
    public CollisionPoint GetCollisionPointFacingTowardsParent()
    {
        if (!Intersection.Valid) return new();
        
        var parent = Self.Parent;
        return parent == null ? new() : Intersection.GetCollisionPointFacingTowardsPoint(parent.Transform.Position);
    }
    
    /// <summary>
    /// Finds the collision point with the normal facing most towards self.
    /// </summary>
    /// <returns></returns>
    public CollisionPoint GetCollisionPointFacingTowardsSelf()
    {
        if (!Intersection.Valid) return new();
        return  Intersection.GetCollisionPointFacingTowardsPoint(Self.CurTransform.Position);
    }

}