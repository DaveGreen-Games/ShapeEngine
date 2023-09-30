using System.Numerics;
using ShapeEngine.Core.Interfaces;

namespace ShapeEngine.Core.Collision;

public class Collision
{
    public readonly bool FirstContact;
    public readonly ICollidable Self;
    public readonly ICollidable Other;
    public readonly Vector2 SelfVel;
    public readonly Vector2 OtherVel;
    public readonly Intersection Intersection;

    public Collision(ICollidable self, ICollidable other, bool firstContact)
    {
        this.Self = self;
        this.Other = other;
        this.SelfVel = self.GetCollider().Vel;
        this.OtherVel = other.GetCollider().Vel;
        this.Intersection = new();
        this.FirstContact = firstContact;
    }
    public Collision(ICollidable self, ICollidable other, bool firstContact, CollisionPoints collisionPoints)
    {
        this.Self = self;
        this.Other = other;
        this.SelfVel = self.GetCollider().Vel;
        this.OtherVel = other.GetCollider().Vel;
        this.Intersection = new(collisionPoints, SelfVel, self.GetCollider().Pos);
        this.FirstContact = firstContact;
    }

}