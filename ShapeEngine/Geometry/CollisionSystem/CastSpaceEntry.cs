using System.Numerics;

namespace ShapeEngine.Geometry.CollisionSystem;

/// <summary>
/// Represents a collection of colliders associated with a specific collision object, used for cast space queries.
/// </summary>
/// <remarks>
/// Provides sorting and utility methods for managing colliders in the context of collision detection.
/// </remarks>
public class CastSpaceEntry : List<Collider>
{
    /// <summary>
    /// The collision object associated with this entry.
    /// </summary>
    public readonly CollisionObject OtherCollisionObject;

    #region Constructors
   
    /// <summary>
    /// Initializes a new instance of the <see cref="CastSpaceEntry"/> class with a specified capacity.
    /// </summary>
    /// <param name="otherCollisionObject">The associated collision object.</param>
    /// <param name="capacity">The initial capacity of the list.</param>
    public CastSpaceEntry(CollisionObject otherCollisionObject, int capacity) : base(capacity)
    {
        OtherCollisionObject = otherCollisionObject;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CastSpaceEntry"/> class.
    /// </summary>
    /// <param name="otherCollisionObject">The associated collision object.</param>
    public CastSpaceEntry(CollisionObject otherCollisionObject)
    {
        OtherCollisionObject = otherCollisionObject;
    }
    
    #endregion
    
    #region Public Functions

    /// <summary>
    /// Gets the first collider in the entry, or null if empty.
    /// </summary>
    /// <returns>The first <see cref="Collider"/>, or null if none exist.</returns>
    public Collider? GetFirstCollider()
    {
        if(Count <= 0) return null;
        return this[0];
    }
    /// <summary>
    /// Gets the last collider in the entry, or null if empty.
    /// </summary>
    /// <returns>The last <see cref="Collider"/>, or null if none exist.</returns>
    public Collider? GetLastCollider()
    {
        if(Count <= 0) return null;
        return this[Count - 1];
    }
    
    /// <summary>
    /// Determines if the specified collider is a first contact (not already present in the entry).
    /// </summary>
    /// <param name="collider">The collider to check.</param>
    /// <returns>True if the collider is not present; otherwise, false.</returns>
    public bool IsFirstContact(Collider collider)
    {
        return !Contains(collider);
    }

    /// <summary>
    /// Creates a copy of this entry, including all colliders.
    /// </summary>
    /// <returns>A new <see cref="CastSpaceEntry"/> with the same colliders and collision object.</returns>
    public CastSpaceEntry Copy()
    {
        var newEntry = new CastSpaceEntry(OtherCollisionObject);
        newEntry.AddRange(this);
        return newEntry;
    }
    
    #endregion
    
    #region Sorting
    
    /// <summary>
    /// Sorts the colliders so that the closest to the reference point comes first.
    /// </summary>
    /// <param name="referencePoint">The point to compare distances from.</param>
    /// <returns>True if sorting was performed; otherwise, false.</returns>
    public bool SortClosestFirst(Vector2 referencePoint)
    {
        if(Count <= 0) return false;
        if(Count == 1) return true;
        this.Sort
        (
            comparison: (a, b) =>
            {
                float la = (referencePoint - a.CurTransform.Position).LengthSquared();
                float lb = (referencePoint - b.CurTransform.Position).LengthSquared();

                if (la > lb) return 1;
                if (MathF.Abs(x: la - lb) < 0.01f) return 0;
                return -1;
            }
        );
        return true;
    }
    /// <summary>
    /// Sorts the colliders so that the furthest from the reference point comes first.
    /// </summary>
    /// <param name="referencePoint">The point to compare distances from.</param>
    /// <returns>True if sorting was performed; otherwise, false.</returns>
    public bool SortFurthestFirst(Vector2 referencePoint)
    {
        if(Count <= 0) return false;
        if(Count == 1) return true;
        this.Sort
        (
            comparison: (a, b) =>
            {
                float la = (referencePoint - a.CurTransform.Position).LengthSquared();
                float lb = (referencePoint - b.CurTransform.Position).LengthSquared();

                if (la < lb) return 1;
                if (MathF.Abs(x: la - lb) < 0.01f) return 0;
                return -1;
            }
        );
        return true;
    }

    
    #endregion
    
    #region Closest/Furthest Collider

    /// <summary>
    /// Gets the closest collider to the reference point.
    /// </summary>
    /// <param name="referencePoint">The point to compare distances from.</param>
    /// <param name="minDistanceSquared">The squared distance to the closest collider, or -1 if none exist.</param>
    /// <returns>The closest <see cref="Collider"/>, or null if none exist.</returns>
    public Collider? GetClosestCollider(Vector2 referencePoint, out float minDistanceSquared)
    {
        minDistanceSquared = -1;
        if(Count <= 0) return null;
        if (Count == 1)
        {
            var col = this[0];
            minDistanceSquared = (referencePoint - col.CurTransform.Position).LengthSquared();
            return col;
        }
        Collider? closestCollider = null;

        foreach (var collider in this)
        {
            var distanceSquared = (referencePoint - collider.CurTransform.Position).LengthSquared();
            if (distanceSquared < minDistanceSquared)
            {
                minDistanceSquared = distanceSquared;
                closestCollider = collider;
            }
        }
        
        return closestCollider;
    }
    /// <summary>
    /// Gets the furthest collider from the reference point.
    /// </summary>
    /// <param name="referencePoint">The point to compare distances from.</param>
    /// <param name="maxDistanceSquared">The squared distance to the furthest collider, or -1 if none exist.</param>
    /// <returns>The furthest <see cref="Collider"/>, or null if none exist.</returns>
    public Collider? GetFurthestCollider(Vector2 referencePoint, out float maxDistanceSquared)
    {
        maxDistanceSquared = -1;
        if(Count <= 0) return null;
        if (Count == 1)
        {
            var col = this[0];
            maxDistanceSquared = (referencePoint - col.CurTransform.Position).LengthSquared();
            return col;
        }
        Collider? furthestCollider = null;

        foreach (var collider in this)
        {
            var distanceSquared = (referencePoint - collider.CurTransform.Position).LengthSquared();
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