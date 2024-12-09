using System.Reflection.Emit;
using ShapeEngine.Core.CollisionSystem;

namespace ShapeEngine.Core.CollisionSystem;

/// <summary>
/// Contains the information of an overlap between two collision objects in form of a list of overlaps.
/// An overlap contains the information of the two overlapping colliders.
/// </summary>
public class OverlapInformation : List<Overlap>
{
    public readonly CollisionObject Self;
    public readonly CollisionObject Other;
        
    public OverlapInformation(CollisionObject self, CollisionObject other)
    {
        Self = self;
        Other = other;
    }

    public OverlapInformation(CollisionObject self, CollisionObject other, List<Overlap> overlaps)
    {
        Self = self;
        Other = other;
        AddRange(overlaps);
    }

    public OverlapInformation Copy()
    {
        var newOverlaps = new List<Overlap>();
        foreach (var overlap in this)
        {
            newOverlaps.Add(overlap.Copy());
        }
        return new (Self, Other, newOverlaps);
    }
    internal Overlap? PopOverlap(Collider self, Collider other)
    {
        foreach (var overlap in this)
        {
            if (overlap.Self == self && overlap.Other == other)
            {
                Remove(overlap);
                return overlap;
            }
        }
        return null;
    }
    
    public List<Overlap>? FilterOverlaps(Predicate<Overlap> match)
    {
        if(Count <= 0) return null;
        List<Overlap>? filtered = null;
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
    public List<Overlap>? GetAllFirstContactOverlaps()
    {
        return FilterOverlaps((c) => c.FirstContact);
    }
    public HashSet<Collider>? GetAllOtherFirstContactColliders()
    {
        var filtered = GetAllFirstContactOverlaps();
        if(filtered == null) return null;
        HashSet<Collider> others = new();
        foreach (var c in filtered)
        {
            others.Add(c.Other);
        }
        return others;
    }

}