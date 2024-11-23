using System.Numerics;

namespace ShapeEngine.Core.CollisionSystem;


public class CastSpaceResult(int capacity) : Dictionary<CollisionObject, CastSpaceEntry>(capacity)
{
    private CastSpaceResult(Dictionary<CollisionObject, CastSpaceEntry> other) : this(other.Count)
    {
        foreach (var kvp in other)
        {
            this.Add(kvp.Key, kvp.Value.Copy());
        }
    }
     
    #region Sorting
    
    public bool SortClosest(Vector2 referencePoint)
    {
        if(Count <= 0) return false;
        foreach (var reg in this.Values)
        {
            reg.SortClosestFirst(referencePoint);
        }

        if (Count > 1)
        {
            var sorted = this.OrderBy(kvp => (kvp.Key.Transform.Position - referencePoint).LengthSquared()).ToDictionary();
        
            Clear();
            
            foreach (var kvp in sorted)
            {
                Add(kvp.Key, kvp.Value);
            }
        }
        
        return true;
    }
    public bool SortFurthest(Vector2 referencePoint)
    {
        if(Count <= 0) return false;
        foreach (var reg in this.Values)
        {
            reg.SortFurthestFirst(referencePoint);
        }

        if (Count > 1)
        {
            var sorted = this.OrderByDescending(kvp => (kvp.Key.Transform.Position - referencePoint).LengthSquared()).ToDictionary();
        
            Clear();
            
            foreach (var kvp in sorted)
            {
                Add(kvp.Key, kvp.Value);
            }
        }
        
        return true;
    }
   
    #endregion

    #region Public Functions

    public CastSpaceEntry? GetFirstEntry()
    {
        if(Count <= 0) return null;
        var entry = this.First().Value;
        return entry.Count > 0 ? entry : null;
    }
    public CastSpaceEntry? GetLastEntry()
    {
        if(Count <= 0) return null;
        var entry = this.Last().Value;
        return entry.Count > 0 ? entry : null;
    }
    public CastSpaceResult Copy() => new(this);
    public CastSpaceEntry? GetEntry(CollisionObject obj) => TryGetValue(obj, out var entry) ? entry : null;
    public bool AddCollider(Collider collider)
    {
        var parent = collider.Parent;
        if (parent == null) return false;

        if (TryGetValue(parent, out var entry))
        {
            entry.Add(collider);
        }
        else
        {
            var newEntry = new CastSpaceEntry(parent, 2) { collider };
            Add(parent, newEntry);
        }

        return true;
    }
    public bool IsFirstContact(Collider collider)
    {
        if(collider.Parent == null) return false;
        return TryGetValue(collider.Parent, out var entry) && entry.IsFirstContact(collider);
    }
    
    #endregion
    
    #region Closet/Furthest Entry
    
    public CastSpaceEntry? GetClosestEntry(Vector2 referencePoint, out float minDistanceSquared)
    {
        minDistanceSquared = -1;
        if (Count <= 0) return null;
        if (Count == 1)
        {
            var entry = this.First().Value;
            minDistanceSquared = (referencePoint - entry.OtherCollisionObject.Transform.Position).LengthSquared();
            return entry;
        }
        CastSpaceEntry? closestEntry = null;

        foreach (var entry in Values)
        {
            var distanceSquared = (referencePoint - entry.OtherCollisionObject.Transform.Position).LengthSquared();
            
            if (distanceSquared < minDistanceSquared)
            {
                minDistanceSquared = distanceSquared;
                closestEntry = entry;
            }
        }
        
        return closestEntry;
    }
    public CastSpaceEntry? GetFurthestEntry(Vector2 referencePoint, out float maxDistanceSquared)
    {
        maxDistanceSquared = -1;
        if (Count <= 0) return null;
        if (Count == 1)
        {
            var entry = this.Last().Value;
            maxDistanceSquared = (referencePoint - entry.OtherCollisionObject.Transform.Position).LengthSquared();
            return entry;
        }
        CastSpaceEntry? furthestEntry = null;

        foreach (var entry in Values)
        {
            var distanceSquared = (referencePoint - entry.OtherCollisionObject.Transform.Position).LengthSquared();
            
            if (distanceSquared > maxDistanceSquared)
            {
                maxDistanceSquared = distanceSquared;
                furthestEntry = entry;
            }
        }
        
        return furthestEntry;
    }
    
    #endregion
    
    #region Closest/Furthest CollisionObject
    
    public CollisionObject? GetClosestCollisionObject(Vector2 referencePoint, out float minDistanceSquared)
    {
        minDistanceSquared = -1;
        if (Count <= 0) return null;
        if (Count == 1)
        {
            var obj = this.First().Key;
            minDistanceSquared = (referencePoint - obj.Transform.Position).LengthSquared();
            return obj;
        }
        CollisionObject? closestCollisionObject = null;

        foreach (var obj in Keys)
        {
            var distanceSquared = (referencePoint - obj.Transform.Position).LengthSquared();
            
            if (distanceSquared < minDistanceSquared)
            {
                minDistanceSquared = distanceSquared;
                closestCollisionObject = obj;
            }
        }
        
        return closestCollisionObject;
    }
    public CollisionObject? GetFurthestCollisionObject(Vector2 referencePoint, out float maxDistanceSquared)
    {
        maxDistanceSquared = -1;
        if (Count <= 0) return null;
        if (Count == 1)
        {
            var obj = this.First().Key;
            maxDistanceSquared = (referencePoint - obj.Transform.Position).LengthSquared();
            return obj;
        }
        CollisionObject? furthestCollisionObject = null;

        foreach (var obj in Keys)
        {
            var distanceSquared = (referencePoint - obj.Transform.Position).LengthSquared();
            
            if (distanceSquared > maxDistanceSquared)
            {
                maxDistanceSquared = distanceSquared;
                furthestCollisionObject = obj;
            }
        }
        
        return furthestCollisionObject;
    }
    
    #endregion
    
    #region Closest/Furthest Collider
    
    public Collider? GetClosestCollider(Vector2 referencePoint, out float minDistanceSquared)
    {
        minDistanceSquared = -1;
        if (Count <= 0) return null;
        if (Count == 1)
        {
            var col = this.GetClosestCollider(referencePoint, out minDistanceSquared);
            return col;
        }
        Collider? closestCollider = null;

        foreach (var entry in Values)
        {
            var collider = entry.GetClosestCollider(referencePoint, out var distanceSquared);
            if (distanceSquared < minDistanceSquared)
            {
                minDistanceSquared = distanceSquared;
                closestCollider = collider;
            }
        }
        
        return closestCollider;
    }
   
    public Collider? GetFurthestCollider(Vector2 referencePoint, out float maxDistanceSquared)
    {
        maxDistanceSquared = -1;
        if (Count <= 0) return null;
        if (Count == 1)
        {
            var col = this.GetFurthestCollider(referencePoint, out maxDistanceSquared);
            return col;
        }
        Collider? furthestCollider = null;

        foreach (var entry in Values)
        {
            var collider = entry.GetFurthestCollider(referencePoint, out var distanceSquared);
            if (distanceSquared > maxDistanceSquared)
            {
                maxDistanceSquared = distanceSquared;
                furthestCollider = collider;
            }
        }
        
        return furthestCollider;
    }
    
    #endregion
}