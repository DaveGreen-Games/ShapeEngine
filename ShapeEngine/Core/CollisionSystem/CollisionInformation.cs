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
    
    #endregion
    
    #region Constructors
    
    public CollisionInformation(CollisionObject self, CollisionObject other)
    {
        Self = self;
        Other = other;
        
    }
    public CollisionInformation(CollisionObject self, CollisionObject other, List<Collision> collisions)
    {
        Self = self;
        Other = other;
        AddRange(collisions);
    }

    #endregion
    
    #region Validation
    
    
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
        return new CollisionInformation(Self, Other, newCollisions);
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
 
    #endregion
}


/*
    public readonly List<Collision> Collisions;
    public readonly CollisionSurface CollisionSurface;
    public CollisionInformation(List<Collision> collisions, bool computesIntersections)
    {
        this.Collisions = collisions;
        if (!computesIntersections) this.CollisionSurface = new();
        else
        {
            Vector2 avgPoint = new();
            Vector2 avgNormal = new();
            var count = 0;
            foreach (var col in collisions)
            {
                if (col.Intersection.Valid)
                {
                    count++;
                    var surface = col.Intersection.CollisionSurface;
                    avgPoint += surface.Point;
                    avgNormal += surface.Normal;
                }
            }

            if (count > 0)
            {
                avgPoint /= count;
                avgNormal = avgNormal.Normalize();
                this.CollisionSurface = new(avgPoint, avgNormal);
            }
            else
            {
                this.CollisionSurface = new();
            }
        }
    }

    //return a dictionary of sorted collisions based on parent
    public Dictionary<CollisionObject, List<Collision>> GetCollisionsSortedByParent()
    {
        Dictionary<CollisionObject, List<Collision>> sortedCollisions = new();

        foreach (var col in Collisions)
        {
            var parent = col.Other.Parent;
            if (parent == null) continue;
            
            if (sortedCollisions.TryGetValue(parent, out var value))
            {
                value.Add(col);
            }
            else
            {
                var list = new List<Collision>() { col };
                sortedCollisions[parent] = list;
            }
        }
        
        return sortedCollisions;
    }
    
    public List<Collision> FilterCollisions(Predicate<Collision> match)
    {
        List<Collision> filtered = new();
        foreach (var c in Collisions)
        {
            if (match(c)) filtered.Add(c);
        }
        return filtered;
    }
    public HashSet<Collider> FilterColliders(Predicate<Collider> match)
    {
        HashSet<Collider> filtered = new();
        foreach (var c in Collisions)
        {
            if (match(c.Other)) filtered.Add(c.Other);
        }
        return filtered;
    }
    public HashSet<Collider> GetAllColliders()
    {
        HashSet<Collider> others = new();
        foreach (var c in Collisions)
        {
            others.Add(c.Other);

        }
        return others;
    }
    public List<Collision> GetFirstContactCollisions()
    {
        return FilterCollisions((c) => c.FirstContact);
    }
    public HashSet<Collider> GetFirstContactColliders()
    {
        var filtered = GetFirstContactCollisions();
        HashSet<Collider> others = new();
        foreach (var c in filtered)
        {
            others.Add(c.Other);
        }
        return others;
    }
    */
    
    