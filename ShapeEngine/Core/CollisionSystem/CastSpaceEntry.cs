using System.Numerics;

namespace ShapeEngine.Core.CollisionSystem;

public class CastSpaceEntry : List<Collider>
{
    public readonly CollisionObject OtherCollisionObject;

    #region Constructors
   
    public CastSpaceEntry(CollisionObject otherCollisionObject, int capacity) : base(capacity)
    {
        OtherCollisionObject = otherCollisionObject;
    }

    public CastSpaceEntry(CollisionObject otherCollisionObject)
    {
        OtherCollisionObject = otherCollisionObject;
    }
    
    #endregion
    
    #region Public Functions

    public Collider? GetFirstCollider()
    {
        if(Count <= 0) return null;
        return this[0];
    }
    public Collider? GetLastCollider()
    {
        if(Count <= 0) return null;
        return this[Count - 1];
    }
    
    public bool IsFirstContact(Collider collider)
    {
        return !Contains(collider);
    }

    public CastSpaceEntry Copy()
    {
        var newEntry = new CastSpaceEntry(OtherCollisionObject);
        newEntry.AddRange(this);
        return newEntry;
    }
    
    #endregion
    
    #region Sorting
    
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