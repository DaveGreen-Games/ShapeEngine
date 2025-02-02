using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Core.CollisionSystem;

public class IntersectSpaceResult : List<IntersectSpaceRegister>
{
    #region Members
    
    public readonly Vector2 Origin;
    public IntersectSpaceRegister? First => Count <= 0 ? null : this[0];
    public IntersectSpaceRegister? Last => Count <= 0 ? null : this[Count - 1];
    
    #endregion
    
    #region Constructors
    public IntersectSpaceResult(Vector2 origin, int capacity) : base(capacity)
    {
        Origin = origin;
    }

    #endregion
    
    #region Public Functions
    
    public bool AddRegister(IntersectSpaceRegister reg)
    {
        if (reg.Count <= 0) return false;
        Add(reg);
        return true;
    }
    
    #endregion
    
    #region Sorting
    public bool SortClosestFirst() => SortClosestFirst(Origin);
    public bool SortFurthestFirst() => SortFurthestFirst(Origin);

    public bool SortClosestFirst(Vector2 referencePoint)
    {
        if(Count <= 0) return false;
        if (Count == 1)
        {
            this[0].SortClosestFirst(referencePoint);
            return true;
        }
        foreach (var reg in this)
        {
            reg.SortClosestFirst(referencePoint);
        }  
        this.Sort
        (
            comparison: (a, b) =>
            {
                if (a.Count <= 0) return 1;
                if (b.Count <= 0) return -1;
                
                var aEntry = a.First;
                if(aEntry == null) return 1;
                
                var bEntry = b.First;
                if(bEntry == null) return -1;
                
                float la = (referencePoint - aEntry.First.Point).LengthSquared();
                float lb = (referencePoint - bEntry.First.Point).LengthSquared();

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
        if (Count == 1)
        {
            this[0].SortFurthestFirst(referencePoint);
            return true;
        }
        foreach (var reg in this)
        {
            reg.SortFurthestFirst(referencePoint);
        }
        this.Sort
        (
            comparison: (a, b) =>
            {
                if (a.Count <= 0) return -1;
                if (b.Count <= 0) return 1;
                
                var aEntry = a.First;
                if(aEntry == null) return -1;
                
                var bEntry = b.First;
                if(bEntry == null) return 1;
                
                float la = (referencePoint - aEntry.First.Point).LengthSquared();
                float lb = (referencePoint - bEntry.First.Point).LengthSquared();

                if (la > lb) return 1;
                if (MathF.Abs(x: la - lb) < 0.01f) return 0;
                return -1;
            }
        );
        return true;
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
    /// <param name="referenceDirection">The direction to check CollisionPoint Normals against.</param>
    /// <param name="combined">An averaged CollisionPoint of all remaining CollisionPoints of all entries.</param>
    /// <returns>Returns true if there are valid points remaining</returns>
    public bool Validate(Vector2 referenceDirection, out CollisionPoint combined)
    {
        if (Count <= 0)
        {
            combined = new CollisionPoint();
            return false;
        }
        
        var avgPoint = new Vector2();
        var avgNormal = new Vector2();
        var count = 0;
        for (int i = Count - 1; i >= 0; i--)
        {
            var register = this[i];
            var valid = register.Validate(referenceDirection, out CollisionPoint combinedEntryPoint);
            if (!valid)
            {
                RemoveAt(i);
                continue;
            }
            avgPoint += combinedEntryPoint.Point;
            avgNormal += combinedEntryPoint.Normal;
            count++;
        }

        if (Count <= 0)
        {
            combined = new CollisionPoint();
            return false;
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
    /// <param name="referenceDirection">The direction to check CollisionPoint Normals against.</param>
    /// <param name="referencePoint">The direction from the reference point towards to CollisionPoint  to check CollisionPoint Normals against.</param>
    /// <param name="combined">An averaged CollisionPoint of all remaining CollisionPoints of all entries.</param>
    /// <returns>Returns true if there are valid points remaining</returns>
    public bool Validate(Vector2 referenceDirection, Vector2 referencePoint, out CollisionPoint combined)
    {
        if (Count <= 0)
        {
            combined = new CollisionPoint();
            return false;
        }
        
        var avgPoint = new Vector2();
        var avgNormal = new Vector2();
        var count = 0;
        for (int i = Count - 1; i >= 0; i--)
        {
            var register = this[i];
            var valid = register.Validate(referenceDirection, referencePoint, out CollisionPoint combinedEntryPoint);
            if (!valid)
            {
                RemoveAt(i);
                continue;
            }
            avgPoint += combinedEntryPoint.Point;
            avgNormal += combinedEntryPoint.Normal;
            count++;
        }
        
        if (Count <= 0)
        {
            combined = new CollisionPoint();
            return false;
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
    /// <param name="referenceDirection">The direction to check CollisionPoint Normals against.</param>
    /// <param name="referencePoint">The direction from the reference point towards to CollisionPoint  to check CollisionPoint Normals against.</param>
    /// <param name="combined">An averaged CollisionPoint of all remaining CollisionPoints of all entries.</param>
    /// <param name="closest">The CollisionPoint that is closest to the referencePoint.</param>
    /// <returns>Returns true if there are valid points remaining</returns>
    public bool Validate(Vector2 referenceDirection, Vector2 referencePoint, out CollisionPoint combined, out CollisionPoint closest)
    {
        if (Count <= 0)
        {
            combined = new CollisionPoint();
            closest = new CollisionPoint();
            return false;
        }
        
        var avgPoint = new Vector2();
        var avgNormal = new Vector2();
        closest  = new CollisionPoint();
        var closestDistanceSquared = -1f;
        var count = 0;
        for (int i = Count - 1; i >= 0; i--)
        {
            var entry = this[i];
            // var valid = entry.ValidateSelf(out var combinedEntryPoint, out var closestToEntry);
            var valid = entry.Validate(referenceDirection, referencePoint, out var combinedEntryPoint, out var closestToEntry);
            if (!valid)
            {
                RemoveAt(i);
                continue;
            }
            avgPoint += combinedEntryPoint.Point;
            avgNormal += combinedEntryPoint.Normal;
            count++;
            var distanceSquared = (referencePoint - combinedEntryPoint.Point).LengthSquared();
            if (closestDistanceSquared < 0 ||  distanceSquared  < closestDistanceSquared)
            {
                closestDistanceSquared = distanceSquared;
                closest = closestToEntry;
            }
        }
        
        if (Count <= 0)
        {
            combined = new CollisionPoint();
            return false;
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
    /// <param name="referencePoint">The direction from the reference point towards to CollisionPoint to check CollisionPoint Normals against.</param>
    /// <param name="combined">An averaged CollisionPoint of all remaining CollisionPoints of all entries.</param>
    /// <param name="closest">The CollisionPoint that is closest to the referencePoint.</param>
    /// <returns>Returns true if there are valid points remaining</returns>
    public bool Validate(Vector2 referencePoint, out CollisionPoint combined, out CollisionPoint closest)
    {
        if (Count <= 0)
        {
            combined = new CollisionPoint();
            closest = new CollisionPoint();
            return false;
        }
        
        var avgPoint = new Vector2();
        var avgNormal = new Vector2();
        closest  = new CollisionPoint();
        var closestDistanceSquared = -1f;
        var count = 0;
        for (int i = Count - 1; i >= 0; i--)
        {
            var register = this[i];
            var valid = register.Validate(referencePoint, out var combinedEntryPoint, out var closestToEntry);
            if (!valid)
            {
                RemoveAt(i);
                continue;
            }
            avgPoint += combinedEntryPoint.Point;
            avgNormal += combinedEntryPoint.Normal;
            count++;
            var distanceSquared = (referencePoint - combinedEntryPoint.Point).LengthSquared();
            if (closestDistanceSquared < 0 ||  distanceSquared  < closestDistanceSquared)
            {
                closestDistanceSquared = distanceSquared;
                closest = closestToEntry;
            }
        }
        
        if (Count <= 0)
        {
            combined = new CollisionPoint();
            return false;
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
    /// <param name="referencePoint">The direction from the reference point towards to CollisionPoint  to check CollisionPoint Normals against.</param>
    /// <param name="validationResult">The result of the combined CollisionPoint, and the  closest/furthest collision point from the reference point, and the CollisionPoint with normal facing towards the referencePoint.</param>
    /// <returns>Returns true if there are valid points remaining</returns>
    public bool Validate(Vector2 referencePoint, out CollisionPointValidationResult validationResult)
    {
        if (Count <= 0)
        {
            validationResult = new CollisionPointValidationResult();
            return false;
        }
        
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
            var register = this[i];
            var valid = register.Validate(referencePoint, out CollisionPointValidationResult result);
            if (!valid)
            {
                RemoveAt(i);
                continue;
            }
            avgPoint += result.Combined.Point;
            avgNormal += result.Combined.Normal;
            count++;
            var referenceDirection = (result.PointingTowards.Point - referencePoint).Normalize();
            var dot = referenceDirection.Dot(result.PointingTowards.Normal);
            if (maxDot < 0 || dot > maxDot)
            {
                maxDot = dot;
                pointing = result.PointingTowards;
            }
            var distanceSquared = (referencePoint - result.Closest.Point).LengthSquared();
            if (closestDistanceSquared < 0 ||  distanceSquared  < closestDistanceSquared)
            {
                closestDistanceSquared = distanceSquared;
                closest = result.Closest;
            }
            
            distanceSquared = (referencePoint - result.Furthest.Point).LengthSquared();
            if (furthestDistanceSquared < 0 ||  distanceSquared  > furthestDistanceSquared)
            {
             furthestDistanceSquared = distanceSquared;
             furthest = result.Furthest;
            }
        }
        
        if (Count <= 0)
        {
            validationResult = new();
            return false;
        }
        
        var combined = new CollisionPoint(avgPoint / count, avgNormal.Normalize());
        validationResult = new CollisionPointValidationResult(combined, closest, furthest, pointing);
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
    /// <param name="referenceDirection">The direction to check CollisionPoint Normals against.</param>
    /// <param name="referencePoint">The direction from the reference point towards to CollisionPoint  to check CollisionPoint Normals against.</param>
    /// <param name="validationResult">The result of the combined CollisionPoint, and the  closest/furthest collision point from the reference point, and the CollisionPoint with normal facing towards the referencePoint.</param>
    /// <returns>Returns true if there are valid points remaining</returns>
    public bool Validate(Vector2 referenceDirection, Vector2 referencePoint, out CollisionPointValidationResult validationResult)
    {
        if (Count <= 0)
        {
            validationResult = new CollisionPointValidationResult();
            return false;
        }
        
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
            var valid = entry.Validate(referenceDirection, referencePoint, out CollisionPointValidationResult result);
            if (!valid)
            {
                RemoveAt(i);
                continue;
            }
            avgPoint += result.Combined.Point;
            avgNormal += result.Combined.Normal;
            count++;
            var dot = referenceDirection.Dot(result.PointingTowards.Normal);
            if (maxDot < 0 || dot > maxDot)
            {
                maxDot = dot;
                pointing = result.PointingTowards;
            }
            var distanceSquared = (referencePoint - result.Closest.Point).LengthSquared();
            if (closestDistanceSquared < 0 ||  distanceSquared  < closestDistanceSquared)
            {
                closestDistanceSquared = distanceSquared;
                closest = result.Closest;
            }
            
            distanceSquared = (referencePoint - result.Furthest.Point).LengthSquared();
            if (furthestDistanceSquared < 0 ||  distanceSquared  > furthestDistanceSquared)
            {
             furthestDistanceSquared = distanceSquared;
             furthest = result.Furthest;
            }
        }
        
        if (Count <= 0)
        {
            validationResult = new CollisionPointValidationResult();
            return false;
        }
        
        var combined = new CollisionPoint(avgPoint / count, avgNormal.Normalize());
        validationResult = new CollisionPointValidationResult(combined, closest, furthest, pointing);
        return true;
    }
    
    #endregion
    
    #region Closest/Furthest Entry
    
    public IntersectSpaceEntry? GetClosestEntry(out float closestDistanceSquared)
    {
        closestDistanceSquared = -1;
        if (Count <= 0) return null;
        return GetClosestEntry(Origin, out closestDistanceSquared);
    }

    public IntersectSpaceEntry? GetClosestEntry(Vector2 referencePoint, out float closestDistanceSquared)
    {
        closestDistanceSquared = -1f;
        if (Count <= 0) return null;
        if (Count == 1)
        {
            var reg = this[0];
            return reg.GetClosestEntry(referencePoint, out closestDistanceSquared);
        }
        IntersectSpaceEntry? closestEntry = null;
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
    public IntersectSpaceEntry? GetFurthestEntry(out float furthestDistanceSquared)
    {
        furthestDistanceSquared = -1f;
        if (Count <= 0) return null;
        return GetFurthestEntry(Origin, out furthestDistanceSquared);
    }
    public IntersectSpaceEntry? GetFurthestEntry(Vector2 referencePoint, out float furthestDistanceSquared)
    {
        furthestDistanceSquared = -1f;
        if(Count <= 0) return null;
        if (Count == 1)
        {
            var reg = this[0];
            return reg.GetFurthestEntry(referencePoint, out furthestDistanceSquared);
        }
        IntersectSpaceEntry? furthestEntry = null;
        foreach (var reg in this)
        {
            var entry = reg.GetFurthestEntry(referencePoint, out var distanceSquared);
            if (furthestEntry == null)
            {
                furthestEntry = entry;
                furthestDistanceSquared = distanceSquared;
            }
            else if (distanceSquared > furthestDistanceSquared)
            {
                furthestEntry = entry;
                furthestDistanceSquared = distanceSquared;
            }
            
        }
        return furthestEntry;
    }

    #endregion
    
    #region Closest/Furthest Collider
    
    public IntersectSpaceEntry? GetClosestEntryCollider(out float closestDistanceSquared)
    {
        closestDistanceSquared = -1;
        if (Count <= 0) return null;
        return GetClosestEntryCollider(Origin, out closestDistanceSquared);
    }
    public IntersectSpaceEntry? GetClosestEntryCollider(Vector2 referencePoint, out float closestDistanceSquared)
    {
        closestDistanceSquared = -1f;
        if(Count <= 0) return null;
        if (Count == 1)
        {
            var reg = this[0];
            return reg.GetClosestEntryCollider(referencePoint, out closestDistanceSquared);
        }
        IntersectSpaceEntry? closestEntry = null;
        foreach (var reg in this)
        {
            var entry = reg.GetClosestEntryCollider(referencePoint, out var distanceSquared);
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
    public IntersectSpaceEntry? GetFurthestEntryCollider(out float furthestDistanceSquared)
    {
        furthestDistanceSquared = -1f;
        if (Count <= 0) return null;
        return GetFurthestEntryCollider(Origin, out furthestDistanceSquared);
    }
    public IntersectSpaceEntry? GetFurthestEntryCollider(Vector2 referencePoint, out float furthestDistanceSquared)
    {
        furthestDistanceSquared = -1f;
        if(Count <= 0) return null;
        if (Count == 1)
        {
            var reg = this[0];
            return reg.GetFurthestEntryCollider(referencePoint, out furthestDistanceSquared);
        }
        IntersectSpaceEntry? furthestEntry = null;
        foreach (var reg in this)
        {
            var entry = reg.GetFurthestEntryCollider(referencePoint, out var distanceSquared);
            if (furthestEntry == null)
            {
                furthestEntry = entry;
                furthestDistanceSquared = distanceSquared;
            }
            else if (distanceSquared > furthestDistanceSquared)
            {
                furthestEntry = entry;
                furthestDistanceSquared = distanceSquared;
            }
            
        }
        return furthestEntry;
    }

    #endregion
    
    #region Closest/Furthest Register
    
    public IntersectSpaceRegister? GetClosestRegister(out float closestDistanceSquared)
    {
        closestDistanceSquared = -1;
        if(Count <= 0) return null;
        return GetClosestRegister(Origin, out closestDistanceSquared);
    }
    public IntersectSpaceRegister? GetClosestRegister(Vector2 referencePoint, out float closestDistanceSquared)
    {
        closestDistanceSquared = -1;
        if(Count <= 0) return null;
        if (Count == 1)
        {
            var reg = this[0];
            closestDistanceSquared = (referencePoint - reg.OtherCollisionObject.Transform.Position).LengthSquared();
            return reg;
        }
        
        var closestReg = this[0];
        closestDistanceSquared = (referencePoint - closestReg.OtherCollisionObject.Transform.Position).LengthSquared();
        for (int i = 1; i < Count; i++)
        {
            var reg = this[i];
            var distanceSquared = (referencePoint - reg.OtherCollisionObject.Transform.Position).LengthSquared();
            if (distanceSquared < closestDistanceSquared)
            {
                closestDistanceSquared = distanceSquared;
                closestReg = reg;
            }
        }
        return closestReg;
    }
    public IntersectSpaceRegister? GetFurthestRegister(out float furthestDistanceSquared)
    {
        furthestDistanceSquared = -1;
        if(Count <= 0) return null;
        return GetFurthestRegister(Origin, out furthestDistanceSquared);
    }
    public IntersectSpaceRegister? GetFurthestRegister(Vector2 referencePoint, out float furthestDistanceSquared)
    {
        furthestDistanceSquared = -1;
        if(Count <= 0) return null;
        if (Count == 1)
        {
            var reg = this[0];
            furthestDistanceSquared = (referencePoint - reg.OtherCollisionObject.Transform.Position).LengthSquared();
            return this[0];
        }
        
        var furthestReg = this[0];
        furthestDistanceSquared = (referencePoint - furthestReg.OtherCollisionObject.Transform.Position).LengthSquared();
        for (int i = 1; i < Count; i++)
        {
            var reg = this[i];
            var distanceSquared = (referencePoint - reg.OtherCollisionObject.Transform.Position).LengthSquared();
            if (distanceSquared > furthestDistanceSquared)
            {
                furthestDistanceSquared = distanceSquared;
                furthestReg = reg;
            }
        }
        return furthestReg;
    }

    #endregion
    
    #region Closest/Furthest Collision Points
    
    public CollisionPoint GetClosestCollisionPoint(out float closestDistanceSquared)
    {
        closestDistanceSquared = -1;
        if (Count <= 0) return new();
        return GetClosestCollisionPoint(Origin, out closestDistanceSquared);
    }
    public CollisionPoint GetClosestCollisionPoint(Vector2 position, out float closestDistanceSquared)
    {
        closestDistanceSquared = -1;
        if (Count <= 0) return new();
        if (Count == 1)
        {
            var point = this[0].GetClosestCollisionPoint(position, out closestDistanceSquared);
            return point;
        }
        var closestPoint = this[0].GetClosestCollisionPoint(position, out closestDistanceSquared);
        for (int i = 1; i < Count; i++)
        {
            var point = this[i].GetClosestCollisionPoint(position, out var distanceSquared);
            if (distanceSquared < closestDistanceSquared)
            {
                closestDistanceSquared = distanceSquared;
                closestPoint = point;
            }
        }

        return closestPoint;
    }
    public CollisionPoint GetFurthestCollisionPoint(out float furthestDistanceSquared)
    {
        furthestDistanceSquared = -1;
        if (Count <= 0) return new();
        return GetFurthestCollisionPoint(Origin, out furthestDistanceSquared);
    }
    public CollisionPoint GetFurthestCollisionPoint(Vector2 position, out float furthestDistanceSquared)
    {
        furthestDistanceSquared = -1;
        if (Count <= 0) return new();
        if (Count == 1)
        {
            var point = this[0].GetFurthestCollisionPoint(position, out furthestDistanceSquared);
            return point;
        }
        var furthestPoint = this[0].GetClosestCollisionPoint(position, out furthestDistanceSquared);
        for (int i = 1; i < Count; i++)
        {
            var point = this[i].GetFurthestCollisionPoint(position, out var distanceSquared);
            if (distanceSquared < furthestDistanceSquared)
            {
                furthestDistanceSquared = distanceSquared;
                furthestPoint = point;
            }
        }

        return furthestPoint;
    }
   
    #endregion
    
    #region Point Towards

    public CollisionPoint GetCollisionPointFacingTowardsPoint(Vector2 referencePoint)
    {
     if(Count <= 0) return new();
     if(Count == 1) return this[0].GetCollisionPointFacingTowardsPoint(referencePoint);        
     var pointing  = new CollisionPoint();
     var maxDot = -1f;
     for (int i = Count - 1; i >= 0; i--)
     {
         var register = this[i];
         if(register.Count <= 0) continue;
        
         var pointingTowards = register.GetCollisionPointFacingTowardsPoint(referencePoint);
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
         var register = this[i];
         if(register.Count <= 0) continue;
        
         var pointingTowards = register.GetCollisionPointFacingTowardsDir(referenceDirection);
         var dot = referenceDirection.Dot(pointingTowards.Normal);
         if (maxDot < 0 || dot > maxDot)
         {
             maxDot = dot;
             pointing = pointingTowards;
         }
     }

     return pointing;
    }
    public CollisionPoint GetCollisionPointFacingTowardsOrigin()
    {
     return GetCollisionPointFacingTowardsPoint(Origin);
    }

    #endregion
}
