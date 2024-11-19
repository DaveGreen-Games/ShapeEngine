using System.Numerics;

namespace ShapeEngine.Core.CollisionSystem;

public class IntersectSpaceRegister : List<IntersectSpaceEntry>
{
    public readonly CollisionObject Object;

    public IntersectSpaceRegister(CollisionObject colObject, int capacity) : base(capacity)
    {
        Object = colObject;
    }

    public bool AddEntry(IntersectSpaceEntry entry)
    {
        if (entry.Count <= 0) return false;
        if (entry.Collider.Parent == null || entry.Collider.Parent == Object) return false;
        Add(entry);
        return true;
    }

    public void SortClosestFirst(Vector2 referencePoint)
    {
        foreach (var entry in this)
        {
            entry.SortClosestFirst(referencePoint);
        }
        this.Sort
        (
            comparison: (a, b) =>
            {
                float la = (referencePoint - a.First.Point).LengthSquared();
                float lb = (referencePoint - b.First.Point).LengthSquared();

                if (la > lb) return 1;
                if (MathF.Abs(x: la - lb) < 0.01f) return 0;
                return -1;
            }
        );
    }
    public void SortFurthestFirst(Vector2 referencePoint)
    {
        foreach (var entry in this)
        {
            entry.SortFurthestFirst(referencePoint);
        }
        this.Sort
        (
            comparison: (a, b) =>
            {
                float la = (referencePoint - a.First.Point).LengthSquared();
                float lb = (referencePoint - b.First.Point).LengthSquared();

                if (la < lb) return 1;
                if (MathF.Abs(x: la - lb) < 0.01f) return 0;
                return -1;
            }
        );
    }

    public IntersectSpaceEntry? GetClosestEntry(Vector2 referencePoint, out float closestDistanceSquared)
    {
        closestDistanceSquared = -1;
        if(Count <= 0) return null;
        if(Count == 1) return this[0];
        
        var closestEntry = this[0];
        closestDistanceSquared = (referencePoint - closestEntry.First.Point).LengthSquared();
        for (int i = 1; i < Count; i++)
        {
            var entry = this[i];
            var closestPoint = entry.GetClosestCollisionPoint(referencePoint, out var distanceSquared);
            // var distanceSquared = (referencePoint - closestPoint.Point).LengthSquared();
            if (distanceSquared < closestDistanceSquared)
            {
                closestDistanceSquared = distanceSquared;
                closestEntry = entry;
            }
        }
        return closestEntry;
    }
    public IntersectSpaceEntry? GetFurthestEntry(Vector2 referencePoint, out float furthestDistanceSquared)
    {
        furthestDistanceSquared = -1;
        if(Count <= 0) return null;
        if(Count == 1) return this[0];
        
        var furthestEntry = this[0];
        furthestDistanceSquared = (referencePoint - furthestEntry.First.Point).LengthSquared();
        for (int i = 1; i < Count; i++)
        {
            var entry = this[i];
            var furthestPoint = entry.GetFurthestCollisionPoint(referencePoint, out var distanceSquared);
            // var distanceSquared = (referencePoint - furthestPoint.Point).LengthSquared();
            if (distanceSquared > furthestDistanceSquared)
            {
                furthestDistanceSquared = distanceSquared;
                furthestEntry = entry;
            }
        }
        return furthestEntry;
    }

    //TODO: Add filter / query methods here to get specific IntersectSpaceEntries
    //--- Find Closest/Furhtest Entry. (based on Collider transform position) 
    //--- Find Closest/Furhtest Entry. (based on CollisionPoint) 
    //--- Find CLosest/Furthest CollisionPoint 
    //--- Validate all entries and get closest collision point/entry
}