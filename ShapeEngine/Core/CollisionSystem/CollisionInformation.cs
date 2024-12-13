using System.Numerics;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.CollisionSystem;

/// <summary>
/// Contains the information of a collision between two collision objects in form of a list of collisions.
/// Each collision is an intersection or overlap between two colliders
/// </summary>
public class CollisionInformation : List<Collision>
{
    #region Members
    
    public readonly CollisionObject Self;
    public readonly CollisionObject Other;
    public readonly bool FirstContact;
    #endregion
    
    #region Constructors
    
    public CollisionInformation(CollisionObject self, CollisionObject other, bool firstContact)
    {
        Self = self;
        Other = other;
        FirstContact = firstContact;
        
    }
    public CollisionInformation(CollisionObject self, CollisionObject other, bool firstContact, List<Collision> collisions)
    {
        Self = self;
        Other = other;
        FirstContact = firstContact;
        AddRange(collisions);
    }

    #endregion
    
    #region Validation
    
    /// <summary>
    /// Validates the collisions  and removes invalid ones using SelfVel as referenceDirection and Self.CurTransform.Position as reference point.
    /// </summary>
    /// <param name="combined">The average CollisionPoint of all valid collision points.</param>
    /// <returns>Returns true if there are valid collision points left.</returns>
    public bool Validate(out CollisionPoint combined)
    {
        combined = new CollisionPoint();
        if(Count <= 0) return false;

        for (int i = Count - 1; i >= 0; i--)
        {
            var collision = this[i];
            if (collision.Validate(out CollisionPoint combinedCollisionPoint))
            {
                combined = combinedCollisionPoint.Average(combined);
            }
            else RemoveAt(i);
        }

        return Count > 0;
    }
    /// <summary>
    /// Validates the collisions  and removes invalid ones using SelfVel as referenceDirection and Self.CurTransform.Position as reference point.
    /// </summary>
    /// <param name="combined">The average CollisionPoint of all valid collision points.</param>
    /// <param name="closest">The closest CollisionPoint to the reference point (Self.CurTransform.Position)</param>
    /// <returns>Returns true if there are valid collision points left.</returns>
    public bool Validate(out CollisionPoint combined, out CollisionPoint closest)
    {
        combined = new CollisionPoint();
        closest = new CollisionPoint();
        var closestDistanceSquared = -1f;
        if(Count <= 0) return false;

        for (int i = Count - 1; i >= 0; i--)
        {
            var collision = this[i];
            if (collision.Validate(out CollisionPoint combinedCollisionPoint, out var closestCollisionPoint))
            {
                combined = combinedCollisionPoint.Average(combined);
                var dis = (collision.Self.CurTransform.Position - closestCollisionPoint.Point).LengthSquared();
                if (closestDistanceSquared < 0f || dis < closestDistanceSquared)
                {
                    closestDistanceSquared = dis;
                    closest = closestCollisionPoint;
                }
            }
            else RemoveAt(i);
        }

        return Count > 0;
    }
    /// <summary>
    /// Validates the collisions  and removes invalid ones using SelfVel as referenceDirection and Self.CurTransform.Position as reference point.
    /// </summary>
    /// <param name="result">A combination of useful collision points.</param>
    /// <returns>Returns true if there are valid collision points left.</returns>
    public bool Validate(out CollisionPointValidationResult result)
    {
        result = new CollisionPointValidationResult();
        var combined = new CollisionPoint();
        var closest = new CollisionPoint();
        var furthest = new CollisionPoint();
        var pointing = new CollisionPoint();
        var closestDistanceSquared = -1f;
        var furthestDistanceSquared = -1f;
        var maxDot = -1f;
        
        if(Count <= 0) return false;

        for (int i = Count - 1; i >= 0; i--)
        {
            var collision = this[i];
            if (collision.Validate(out CollisionPointValidationResult validationResult))
            {
                combined = validationResult.Combined.Average(combined);
                
                if (collision.SelfVel.X != 0 || collision.SelfVel.Y != 0)
                {
                    var dot = collision.SelfVel.Dot(validationResult.PointingTowards.Normal);
                    if (dot > maxDot)
                    {
                        maxDot = dot;
                        pointing = validationResult.PointingTowards;
                    }
                }
               
                var dis = (collision.Self.CurTransform.Position - validationResult.Closest.Point).LengthSquared();
                if (closestDistanceSquared < 0f || dis < closestDistanceSquared)
                {
                    closestDistanceSquared = dis;
                    closest = validationResult.Closest;
                }
                else if (furthestDistanceSquared < 0f || dis > furthestDistanceSquared)
                {
                    furthestDistanceSquared = dis;
                    furthest = validationResult.Furthest;
                }
            }
            else RemoveAt(i);
        }

        result = new(combined, closest, furthest, pointing);
        return Count > 0;
    }
   
    #endregion
    
    #region Public Functions
    
    public CollisionInformation Copy()
    {
        var newCollisions = new List<Collision>();
        foreach (var collision in this)
        {
            newCollisions.Add(collision.Copy());
        }
        return new CollisionInformation(Self, Other, FirstContact,  newCollisions);
    }
    
    public List<Collision>? FilterCollisions(Predicate<Collision> match)
    {
        if(Count <= 0) return null;
        List<Collision>? filtered = null;
        foreach (var c in this)
        {
            if (match(c))
            {
                filtered??= new();
                filtered.Add(c);
            }
        }
        return filtered;
    }
    public HashSet<Collider>? GetAllOtherColliders()
    {
        if(Count <= 0) return null;
        HashSet<Collider> others = new();
        foreach (var c in this)
        {
            others.Add(c.Other);
        }
        return others;
    }
    public List<Collision>? GetAllFirstContactCollisions()
    {
        return FilterCollisions((c) => c.FirstContact);
    }
    public HashSet<Collider>? GetAllOtherFirstContactColliders()
    {
        var filtered = GetAllFirstContactCollisions();
        if(filtered == null) return null;
        HashSet<Collider> others = new();
        foreach (var c in filtered)
        {
            others.Add(c.Other);
        }
        return others;
    }

    
    //TODO: Implement all functions!
    public CollisionPoint GetClosestCollisionPoint()
    {
        return new();
    }

    public CollisionPoint GetFurthestCollisionPoint()
    {
        return new();
    }
    
    public CollisionPoint GetCombinedCollisionPoint()
    {
        return new();
    }
    
    public CollisionPoint GetClosestCollisionPoint(Vector2 referencePoint)
    {
        return new();
    }

    public CollisionPoint GetFurthestCollisionPoint(Vector2 referencePoint)
    {
        return new();
    }
    
    public CollisionPoint GetCombinedCollisionPoint(Vector2 referencePoint)
    {
        return new();
    }
    
    public CollisionPoint GetClosestCollisionPoint(out float minDistanceSquared)
    {
        minDistanceSquared = -1f;
        return new();
    }

    public CollisionPoint GetFurthestCollisionPoint(out float maxDistanceSquared)
    {
        maxDistanceSquared = -1f;
        return new();
    }
    
    public CollisionPoint GetClosestCollisionPoint(Vector2 referencePoint, out float minDistanceSquared)
    {
        minDistanceSquared = -1f;
        return new();
    }

    public CollisionPoint GetFurthestCollisionPoint(Vector2 referencePoint, out float maxDistanceSquared)
    {
        maxDistanceSquared = -1f;
        return new();
    }
    #endregion
}

