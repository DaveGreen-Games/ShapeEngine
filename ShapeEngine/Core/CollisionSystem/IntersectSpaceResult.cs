using System.Numerics;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.CollisionSystem;

public class IntersectSpaceResult : List<IntersectSpaceRegister>
{
    public readonly Vector2 Origin;
    
    public IntersectSpaceResult(Vector2 origin, int capacity) : base(capacity)
    {
        Origin = origin;
    }

    public bool AddRegister(IntersectSpaceRegister reg)
    {
        if (reg.Count <= 0) return false;
        Add(reg);
        return true;
    }

    public void SortClosestFirst()
    {
        foreach (var reg in this)
        {
            reg.SortClosestFirst(Origin);
        }    
    }
    public void SortFurthestFirst()
    {
        foreach (var reg in this)
        {
            reg.SortFurthestFirst(Origin);
        }
    }

    public IntersectSpaceEntry? GetClosestEntry()
    {
        if (Count <= 0) return null;
        return GetClosestEntry(Origin);
    }
    public IntersectSpaceEntry? GetClosestEntry(Vector2 referencePoint)
    {
        if(Count <= 0) return null;
        IntersectSpaceEntry? closestEntry = null;
        var closestDistanceSquared = 0f;
        foreach (var reg in this)
        {
            var entry = reg.GetClosestEntry(referencePoint, out var distanceSquared);
            if (closestEntry == null)
            {
                closestEntry = entry;
                closestDistanceSquared = distanceSquared;
            }
            else if (distanceSquared < closestDistanceSquared)
            {
                closestEntry = entry;
                closestDistanceSquared = distanceSquared;
            }
            
        }
        return closestEntry;
    }
    public CollisionPoint GetClosestCollisionPoint()
    {
        return GetClosestCollisionPoint(Origin);
    }
    public CollisionPoint GetClosestCollisionPoint(Vector2 position)
    {
        foreach (var reg in this)
        {
            
        }

        return new();
    }
    
    
    //TODO: Add filter / query methods here to get specific IntersectSpaceResults
    //--- Find Closest/Furthest Register (based on Object transform position)
    //--- Find Closest/Furhtest Entry. (based on Collider transform position) 
    //--- Find Closest/Furhtest Entry. (based on CollisionPoint) 
    //--- Find CLosest/Furthest CollisionPoint 
    //--- Validate all registers and get closest collision point/entry
}