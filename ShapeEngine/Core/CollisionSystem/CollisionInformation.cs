using System.Numerics;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.CollisionSystem;

public class CollisionInformation : List<Collision>
{
    public readonly CollisionObject Self;
    public readonly CollisionObject Other;
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

    public CollisionInformation Copy()
    {
        var newCollisions = new List<Collision>();
        foreach (var collision in this)
        {
            newCollisions.Add(collision.Copy());
        }
        return new CollisionInformation(Self, Other, newCollisions);
    }
    //todo add filter functions agains
    
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
    
    