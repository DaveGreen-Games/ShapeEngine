using System.Numerics;

namespace ShapeEngine.Geometry.CollisionSystem;

/// <summary>
/// Represents the result of a cast space query, mapping collision objects to their associated colliders.
/// </summary>
/// <remarks>
/// Provides sorting, copying, and utility methods for managing cast space results in collision detection.
/// </remarks>
public class CastSpaceResult(int capacity) : Dictionary<CollisionObject, CastSpaceEntry>(capacity)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CastSpaceResult"/> class with a default capacity of 4.
    /// </summary>
    public CastSpaceResult() : this(4) { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="CastSpaceResult"/> class by copying another result.
    /// </summary>
    /// <param name="other">The dictionary to copy from.</param>
    private CastSpaceResult(Dictionary<CollisionObject, CastSpaceEntry> other) : this(other.Count)
    {
        foreach (var kvp in other)
        {
            this.Add(kvp.Key, kvp.Value.Copy());
        }
    }
    
    #region Sorting
    
    /// <summary>
    /// Sorts all entries and their colliders by closest to the reference point.
    /// </summary>
    /// <param name="referencePoint">The point to compare distances from.</param>
    /// <returns>True if sorting was performed; otherwise, false.</returns>
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
    /// <summary>
    /// Sorts all entries and their colliders by furthest from the reference point.
    /// </summary>
    /// <param name="referencePoint">The point to compare distances from.</param>
    /// <returns>True if sorting was performed; otherwise, false.</returns>
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
    /// <summary>
    /// Gets whether this result contains any entries.
    /// </summary>
    public bool Valid => Count > 0;
    /// <summary>
    /// Returns true if there are any entries in the result.
    /// </summary>
    public bool HasEntries() => Count > 0;
    /// <summary>
    /// Gets the first entry with at least one collider, or null if none exist.
    /// </summary>
    /// <returns>The first <see cref="CastSpaceEntry"/> with colliders, or null.</returns>
    public CastSpaceEntry? GetFirstEntry()
    {
        if(Count <= 0) return null;
        var entry = this.First().Value;
        return entry.Count > 0 ? entry : null;
    }
    /// <summary>
    /// Gets the last entry with at least one collider, or null if none exist.
    /// </summary>
    /// <returns>The last <see cref="CastSpaceEntry"/> with colliders, or null.</returns>
    public CastSpaceEntry? GetLastEntry()
    {
        if(Count <= 0) return null;
        var entry = this.Last().Value;
        return entry.Count > 0 ? entry : null;
    }
    /// <summary>
    /// Creates a deep copy of this result.
    /// </summary>
    /// <returns>A new <see cref="CastSpaceResult"/> with copied entries and colliders.</returns>
    public CastSpaceResult Copy() => new(this);
    /// <summary>
    /// Gets the entry for a specific collision object, or null if not found.
    /// </summary>
    /// <param name="obj">The collision object to look up.</param>
    /// <returns>The <see cref="CastSpaceEntry"/> for the object, or null.</returns>
    public CastSpaceEntry? GetEntry(CollisionObject obj) => TryGetValue(obj, out var entry) ? entry : null;
    /// <summary>
    /// Adds a collider to the appropriate entry, creating a new entry if needed.
    /// </summary>
    /// <param name="collider">The collider to add.</param>
    /// <returns>True if the collider was added; otherwise, false.</returns>
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
    /// <summary>
    /// Determines if the specified collider is a first contact (not already present in its entry).
    /// </summary>
    /// <param name="collider">The collider to check.</param>
    /// <returns>True if the collider is not present in its entry; otherwise, false.</returns>
    public bool IsFirstContact(Collider collider)
    {
        if(collider.Parent == null) return false;
        return TryGetValue(collider.Parent, out var entry) && entry.IsFirstContact(collider);
    }
    #endregion
    
    #region Closet/Furthest Entry
    
    /// <summary>
    /// Gets the closest entry to the reference point, or null if no entries exist.
    /// </summary>
    /// <param name="referencePoint">The point to measure distance from.</param>
    /// <param name="minDistanceSquared">The square of the minimum distance found.</param>
    /// <returns>The closest <see cref="CastSpaceEntry"/> to the reference point, or null.</returns>
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
    /// <summary>
    /// Gets the furthest entry from the reference point, or null if no entries exist.
    /// </summary>
    /// <param name="referencePoint">The point to measure distance from.</param>
    /// <param name="maxDistanceSquared">The square of the maximum distance found.</param>
    /// <returns>The furthest <see cref="CastSpaceEntry"/> from the reference point, or null.</returns>
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
    
    /// <summary>
    /// Gets the closest collision object to the reference point, or null if no objects exist.
    /// </summary>
    /// <param name="referencePoint">The point to measure distance from.</param>
    /// <param name="minDistanceSquared">The square of the minimum distance found.</param>
    /// <returns>The closest <see cref="CollisionObject"/> to the reference point, or null.</returns>
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
    /// <summary>
    /// Gets the furthest collision object from the reference point, or null if no objects exist.
    /// </summary>
    /// <param name="referencePoint">The point to measure distance from.</param>
    /// <param name="maxDistanceSquared">The square of the maximum distance found.</param>
    /// <returns>The furthest <see cref="CollisionObject"/> from the reference point, or null.</returns>
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
    
    /// <summary>
    /// Gets the closest collider to the reference point, or null if no colliders exist.
    /// </summary>
    /// <param name="referencePoint">The point to measure distance from.</param>
    /// <param name="minDistanceSquared">The square of the minimum distance found.</param>
    /// <returns>The closest <see cref="Collider"/> to the reference point, or null.</returns>
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
   
    /// <summary>
    /// Gets the furthest collider from the reference point, or null if no colliders exist.
    /// </summary>
    /// <param name="referencePoint">The point to measure distance from.</param>
    /// <param name="maxDistanceSquared">The square of the maximum distance found.</param>
    /// <returns>The furthest <see cref="Collider"/> from the reference point, or null.</returns>
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