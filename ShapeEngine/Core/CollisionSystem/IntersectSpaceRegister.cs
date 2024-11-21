using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

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

    public CollisionPoint GetAverageCollisionPoint()
    {
        var avgPoint = new Vector2();
        var avgNormal = new Vector2();
        foreach (var entry in this)
        {
            var sum = entry.GetAverageCollisionPoint();
            avgPoint += sum.Point;
            avgNormal += sum.Normal;
        }
        return new CollisionPoint(avgPoint / Count, avgNormal.Normalize());
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
    
    #region Pointing Towards
    public CollisionPoint GetCollisionPointFacingTowardsPoint(Vector2 referencePoint)
    {
        if(Count <= 0) return new();
        if(Count == 1) return this[0].GetCollisionPointFacingTowardsPoint(referencePoint);        
        var pointing  = new CollisionPoint();
        var maxDot = -1f;
        for (int i = Count - 1; i >= 0; i--)
        {
            var entry = this[i];
            if(entry.Count <= 0) continue;
            
            var pointingTowards = entry.GetCollisionPointFacingTowardsPoint(referencePoint);
            var dir = (referencePoint - pointingTowards.Point).Normalize();
            var dot = dir.Dot(pointingTowards.Normal);
            if (maxDot < 0 || dot > maxDot)
            {
                maxDot = dot;
                pointing = pointingTowards;
            }
        }

        return pointing;
    }
    public CollisionPoint GetCollisionPointFacingTowardsDir(Vector2 referenceDirection)
    {
        if(Count <= 0) return new();
        if(Count == 1) return this[0].GetCollisionPointFacingTowardsDir(referenceDirection);        
        var pointing  = new CollisionPoint();
        var maxDot = -1f;
        for (int i = Count - 1; i >= 0; i--)
        {
            var entry = this[i];
            if(entry.Count <= 0) continue;
            
            var pointingTowards = entry.GetCollisionPointFacingTowardsDir(referenceDirection);
            var dot = referenceDirection.Dot(pointingTowards.Normal);
            if (maxDot < 0 || dot > maxDot)
            {
                maxDot = dot;
                pointing = pointingTowards;
            }
        }

        return pointing;
    }
    public CollisionPoint GetCollisionPointFacingTowardsPoint()
    {
        return GetCollisionPointFacingTowardsPoint(Object.Transform.Position);
    }
    public CollisionPoint GetCollisionPointFacingTowardsDir()
    {
        return GetCollisionPointFacingTowardsDir(Object.Velocity);
    }

    #endregion
    
    #region Validation
    
    /// <summary>
    /// Removes:
    /// - invalid CollisionPoints
    /// - CollisionPoints with normals facing in the same direction as the reference direction
    /// - CollisionPoints with normals facing in the opposite direction as the reference point (from CollisionPoint towards the reference point)
    /// - Uses Object.Velocity as reference direction.
    /// - Uses Object.Transform.Position as reference point.
    /// </summary>
    /// <param name="combined">An averaged CollisionPoint of all remaining CollisionPoints of all entries.</param>
    /// <param name="closest">The CollisionPoint that is closest to the referencePoint.</param>
    /// <returns>Returns true if there are valid points remaining</returns>
    public bool ValidateSelf(out CollisionPoint combined, out CollisionPoint closest)
    {
        var avgPoint = new Vector2();
        var avgNormal = new Vector2();
        closest  = new CollisionPoint();
        var closestDistanceSquared = -1f;
        var count = 0;
        for (int i = Count - 1; i >= 0; i--)
        {
            var entry = this[i];
            // var valid = entry.ValidateSelf(out var combinedEntryPoint, out var closestToEntry);
            var valid = entry.Validate(Object.Velocity, Object.Transform.Position, out var combinedEntryPoint, out var closestToEntry);
            if (!valid)
            {
                RemoveAt(i);
                continue;
            }
            avgPoint += combinedEntryPoint.Point;
            avgNormal += combinedEntryPoint.Normal;
            count++;
            var distanceSquared = (Object.Transform.Position - combinedEntryPoint.Point).LengthSquared();
            if (closestDistanceSquared < 0 ||  distanceSquared  < closestDistanceSquared)
            {
                closestDistanceSquared = distanceSquared;
                closest = closestToEntry;
            }
        }
        combined = new CollisionPoint(avgPoint / count, avgNormal.Normalize());
        return true;
    }
    /// <summary>
    /// Removes:
    /// - invalid CollisionPoints
    /// - CollisionPoints with normals facing in the same direction as the reference direction
    /// - CollisionPoints with normals facing in the opposite direction as the reference point (from CollisionPoint towards the reference point)
    /// - Uses Object.Velocity as reference direction.
    /// - Uses Object.Transform.Position as reference point.
    /// </summary>
    /// <param name="validationResult">The result of the combined CollisionPoint, and the  closest/furthest collision point from the reference point, and the CollisionPoint with normal facing towards the referencePoint.</param>
    /// <returns>Returns true if there are valid points remaining</returns>
    public bool ValidateSelf(out CollisionPointValidationResult validationResult)
    {
        var avgPoint = new Vector2();
        var avgNormal = new Vector2();
        var closest  = new CollisionPoint();
        var furthest  = new CollisionPoint();
        var pointing  = new CollisionPoint();
        var maxDot = -1f;
        var closestDistanceSquared = -1f;
        var furthestDistanceSquared = -1f;
        var count = 0;
        for (int i = Count - 1; i >= 0; i--)
        {
            var entry = this[i];
            // var valid = entry.ValidateSelf(out var result);
            var valid = entry.Validate(Object.Velocity, Object.Transform.Position, out CollisionPointValidationResult result);
            if (!valid)
            {
                RemoveAt(i);
                continue;
            }
            avgPoint += result.Combined.Point;
            avgNormal += result.Combined.Normal;
            count++;
            var dot = Object.Velocity.Dot(result.PointingTowards.Normal);
            if (maxDot < 0 || dot > maxDot)
            {
                maxDot = dot;
                pointing = result.PointingTowards;
            }
            var distanceSquared = (Object.Transform.Position - result.Closest.Point).LengthSquared();
            if (closestDistanceSquared < 0 ||  distanceSquared  < closestDistanceSquared)
            {
                closestDistanceSquared = distanceSquared;
                closest = result.Closest;
            }
            
            distanceSquared = (Object.Transform.Position - result.Furthest.Point).LengthSquared();
            if (furthestDistanceSquared < 0 ||  distanceSquared  > furthestDistanceSquared)
            {
             furthestDistanceSquared = distanceSquared;
             furthest = result.Furthest;
            }
        }
        var combined = new CollisionPoint(avgPoint / count, avgNormal.Normalize());
        validationResult = new CollisionPointValidationResult(combined, closest, furthest, pointing);
        return true;
    }
   
    #endregion
}