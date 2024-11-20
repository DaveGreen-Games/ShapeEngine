using System.Numerics;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.CollisionSystem;

public class IntersectSpaceRegister : List<IntersectSpaceEntry>
{
    #region Members
    public readonly CollisionObject Object;
    public IntersectSpaceEntry? First => Count <= 0 ? null : this[0];
    public IntersectSpaceEntry? Last => Count <= 0 ? null : this[Count - 1];

    #endregion
    
    #region Constructors
    public IntersectSpaceRegister(CollisionObject colObject, int capacity) : base(capacity)
    {
        Object = colObject;
    }

    #endregion
    
    #region Public Functions
    
    public bool AddEntry(IntersectSpaceEntry entry)
    {
        if (entry.Count <= 0) return false;
        if (entry.Collider.Parent == null || entry.Collider.Parent == Object) return false;
        Add(entry);
        return true;
    }

    #endregion
    
    #region Sorting
    public bool SortClosestFirst(Vector2 referencePoint)
    {
        if (Count <= 0) return false;
        if (Count == 1)
        {
            this[0].SortClosestFirst(referencePoint);
            return true;
        }
        foreach (var entry in this)
        {
            entry.SortClosestFirst(referencePoint);
        }
        this.Sort
        (
            comparison: (a, b) =>
            {
                if (a.Count <= 0) return 1;
                if (b.Count <= 0) return -1;
                
                float la = (referencePoint - a.First.Point).LengthSquared();
                float lb = (referencePoint - b.First.Point).LengthSquared();

                if (la > lb) return 1;
                if (MathF.Abs(x: la - lb) < 0.01f) return 0;
                return -1;
            }
        );
        return true;
    }
    public bool SortFurthestFirst(Vector2 referencePoint)
    {
        if (Count <= 0) return false;
        if (Count == 1)
        {
            this[0].SortFurthestFirst(referencePoint);
            return true;
        }
        foreach (var entry in this)
        {
            entry.SortFurthestFirst(referencePoint);
        }
        this.Sort
        (
            comparison: (a, b) =>
            {
                if (a.Count <= 0) return -1;
                if (b.Count <= 0) return 1;
                
                float la = (referencePoint - a.First.Point).LengthSquared();
                float lb = (referencePoint - b.First.Point).LengthSquared();

                if (la < lb) return 1;
                if (MathF.Abs(x: la - lb) < 0.01f) return 0;
                return -1;
            }
        );
        return true;
    }

    #endregion
    
    #region Closest/Furthest Collider
    public IntersectSpaceEntry? GetClosestEntryCollider(Vector2 referencePoint, out float closestDistanceSquared)
    {
        closestDistanceSquared = -1;
        if(Count <= 0) return null;
        if(Count == 1) return this[0];
        
        var closestEntry = this[0];
        closestDistanceSquared = (referencePoint - closestEntry.Collider.CurTransform.Position).LengthSquared();
        for (int i = 1; i < Count; i++)
        {
            var entry = this[i];
            var distanceSquared = (referencePoint - entry.Collider.CurTransform.Position).LengthSquared();
            if (distanceSquared < closestDistanceSquared)
            {
                closestDistanceSquared = distanceSquared;
                closestEntry = entry;
            }
        }
        return closestEntry;
    }
    public IntersectSpaceEntry? GetFurthestEntryCollider(Vector2 referencePoint, out float furthestDistanceSquared)
    {
        furthestDistanceSquared = -1;
        if(Count <= 0) return null;
        if(Count == 1) return this[0];
        
        var furthestEntry = this[0];
        furthestDistanceSquared = (referencePoint - furthestEntry.Collider.CurTransform.Position).LengthSquared();
        for (int i = 1; i < Count; i++)
        {
            var entry = this[i];
            var distanceSquared = (referencePoint - entry.Collider.CurTransform.Position).LengthSquared();
            if (distanceSquared > furthestDistanceSquared)
            {
                furthestDistanceSquared = distanceSquared;
                furthestEntry = entry;
            }
        }
        return furthestEntry;
    }
    
    #endregion
    
    #region Closest/Furthest Entry
    
    public IntersectSpaceEntry? GetClosestEntry(Vector2 referencePoint, out float closestDistanceSquared)
    {
        closestDistanceSquared = -1;
        if(Count <= 0) return null;
        if (Count == 1)
        {
            var entry = this[0];
            entry.GetClosestCollisionPoint(referencePoint, out closestDistanceSquared);
            return entry;
        }
        
        var closestEntry = this[0];
        closestEntry.GetClosestCollisionPoint(referencePoint, out closestDistanceSquared);
        for (int i = 1; i < Count; i++)
        {
            var entry = this[i];
            entry.GetClosestCollisionPoint(referencePoint, out var distanceSquared);
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
        if (Count == 1)
        {
            var entry = this[0];
            entry.GetFurthestCollisionPoint(referencePoint, out furthestDistanceSquared);
            return entry;
        }
        
        var furthestEntry = this[0];
        furthestEntry.GetFurthestCollisionPoint(referencePoint, out furthestDistanceSquared);
        for (int i = 1; i < Count; i++)
        {
            var entry = this[i];
            entry.GetFurthestCollisionPoint(referencePoint, out var distanceSquared);
            if (distanceSquared < furthestDistanceSquared)
            {
                furthestDistanceSquared = distanceSquared;
                furthestEntry = entry;
            }
        }
        return furthestEntry;
    }

    #endregion
    
    #region Closest/Furthest Collision Point
    public CollisionPoint GetClosestCollisionPoint(Vector2 referencePoint, out float closestDistanceSquared)
    {
        closestDistanceSquared = -1;
        if(Count <= 0) return new();
        if(Count == 1) return this[0].GetClosestCollisionPoint(referencePoint, out closestDistanceSquared);
        
        var closestPoint = this[0].GetClosestCollisionPoint(referencePoint, out closestDistanceSquared);
        for (int i = 1; i < Count; i++)
        {
            var point = this[i].GetClosestCollisionPoint(referencePoint, out var distanceSquared);
            
            if (distanceSquared < closestDistanceSquared)
            {
                closestDistanceSquared = distanceSquared;
                closestPoint = point;
            }
        }
        return closestPoint;
    }
    public CollisionPoint GetFurthestCollisionPoint(Vector2 referencePoint, out float furthestDistanceSquared)
    {
        furthestDistanceSquared = -1;
        if(Count <= 0) return new();
        if(Count == 1) return this[0].GetFurthestCollisionPoint(referencePoint, out furthestDistanceSquared);
        
        var furthestPoint = this[0].GetFurthestCollisionPoint(referencePoint, out furthestDistanceSquared);
        for (int i = 1; i < Count; i++)
        {
            var point = this[i].GetFurthestCollisionPoint(referencePoint, out var distanceSquared);
            
            if (distanceSquared > furthestDistanceSquared)
            {
                furthestDistanceSquared = distanceSquared;
                furthestPoint = point;
            }
        }
        return furthestPoint;
    }

    #endregion
    
    #region Validation
    //TODO: Validate all entries and get closest collision point/entry
    
    #endregion
}