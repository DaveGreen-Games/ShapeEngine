using System.Numerics;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.Collision;

public class Collision
{
    public readonly bool FirstContact;
    public readonly Collider Self;
    public readonly Collider Other;
    public readonly Vector2 SelfVel;
    public readonly Vector2 OtherVel;
    public readonly Intersection Intersection;
    public CollisionPoint FirstCollisionPoint => Intersection.Valid ? Intersection.FirstCollisionPoint : new();

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

}