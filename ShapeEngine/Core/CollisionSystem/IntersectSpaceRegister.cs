using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Core.CollisionSystem;

/// <summary>
/// Represents a register of intersection entries for a specific <see cref="CollisionObject"/>.
/// </summary>
/// <remarks>
/// This class is used to store and manage multiple <see cref="IntersectSpaceEntry"/> objects that result from intersection tests with a single collision object.
/// Provides utilities for sorting, validation, and querying closest/furthest entries and collision points.
/// </remarks>
public class IntersectSpaceRegister : List<IntersectSpaceEntry>
{
    #region Members
    /// <summary>
    /// The collision object that all entries in this register are associated with.
    /// </summary>
    public readonly CollisionObject OtherCollisionObject;
    /// <summary>
    /// Gets the first <see cref="IntersectSpaceEntry"/> in the register, or null if the register is empty.
    /// </summary>
    public IntersectSpaceEntry? First => Count <= 0 ? null : this[0];
    /// <summary>
    /// Gets the last <see cref="IntersectSpaceEntry"/> in the register, or null if the register is empty.
    /// </summary>
    public IntersectSpaceEntry? Last => Count <= 0 ? null : this[Count - 1];
    #endregion
    
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="IntersectSpaceRegister"/> class for a given collision object and with a specified capacity.
    /// </summary>
    /// <param name="colOtherCollisionObject">The collision object associated with this register.</param>
    /// <param name="capacity">The initial capacity for the register.</param>
    public IntersectSpaceRegister(CollisionObject colOtherCollisionObject, int capacity) : base(capacity)
    {
        OtherCollisionObject = colOtherCollisionObject;
    }
    #endregion
    
    #region Public Functions
    /// <summary>
    /// Adds an <see cref="IntersectSpaceEntry"/> to the register if it is valid.
    /// </summary>
    /// <param name="entry">The entry to add.</param>
    /// <returns>True if the entry was added; otherwise, false.</returns>
    /// <remarks>
    /// The entry is only added if it contains collision points and its parent is not null or the same as the register's collision object.
    /// </remarks>
    public bool AddEntry(IntersectSpaceEntry entry)
    {
        if (entry.Count <= 0) return false;
        if (entry.OtherCollider.Parent == null || entry.OtherCollider.Parent == OtherCollisionObject) return false;
        Add(entry);
        return true;
    }

    /// <summary>
    /// Calculates the average collision point and normal from all entries in the register.
    /// </summary>
    /// <returns>A <see cref="CollisionPoint"/> representing the average point and normal.</returns>
    public CollisionPoint GetAverageCollisionPoint()
    {
        var avgPoint = new Vector2();
        var avgNormal = new Vector2();
        foreach (var entry in this)
        {
            var sum = entry.GetCombinedCollisionPoint();
            avgPoint += sum.Point;
            avgNormal += sum.Normal;
        }
        return new CollisionPoint(avgPoint / Count, avgNormal.Normalize());
    }
    #endregion
    
    #region Sorting
    /// <summary>
    /// Sorts all entries and their collision points so that the closest points to the reference point come first.
    /// </summary>
    /// <param name="referencePoint">The point to compare distances from.</param>
    /// <returns>True if sorting was performed; otherwise, false.</returns>
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
    /// <summary>
    /// Sorts all entries and their collision points so that the furthest points from the reference point come first.
    /// </summary>
    /// <param name="referencePoint">The point to compare distances from.</param>
    /// <returns>True if sorting was performed; otherwise, false.</returns>
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
    /// <summary>
    /// Gets the entry whose collider's position is closest to the reference point.
    /// </summary>
    /// <param name="referencePoint">The point to compare distances from.</param>
    /// <param name="closestDistanceSquared">The squared distance to the closest collider.</param>
    /// <returns>The closest <see cref="IntersectSpaceEntry"/> or null if none exist.</returns>
    public IntersectSpaceEntry? GetClosestEntryCollider(Vector2 referencePoint, out float closestDistanceSquared)
    {
        closestDistanceSquared = -1;
        if(Count <= 0) return null;
        if(Count == 1) return this[0];
        
        var closestEntry = this[0];
        closestDistanceSquared = (referencePoint - closestEntry.OtherCollider.CurTransform.Position).LengthSquared();
        for (int i = 1; i < Count; i++)
        {
            var entry = this[i];
            var distanceSquared = (referencePoint - entry.OtherCollider.CurTransform.Position).LengthSquared();
            if (distanceSquared < closestDistanceSquared)
            {
                closestDistanceSquared = distanceSquared;
                closestEntry = entry;
            }
        }
        return closestEntry;
    }
    /// <summary>
    /// Gets the entry whose collider's position is furthest from the reference point.
    /// </summary>
    /// <param name="referencePoint">The point to compare distances from.</param>
    /// <param name="furthestDistanceSquared">The squared distance to the furthest collider.</param>
    /// <returns>The furthest <see cref="IntersectSpaceEntry"/> or null if none exist.</returns>
    public IntersectSpaceEntry? GetFurthestEntryCollider(Vector2 referencePoint, out float furthestDistanceSquared)
    {
        furthestDistanceSquared = -1;
        if(Count <= 0) return null;
        if(Count == 1) return this[0];
        
        var furthestEntry = this[0];
        furthestDistanceSquared = (referencePoint - furthestEntry.OtherCollider.CurTransform.Position).LengthSquared();
        for (int i = 1; i < Count; i++)
        {
            var entry = this[i];
            var distanceSquared = (referencePoint - entry.OtherCollider.CurTransform.Position).LengthSquared();
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
    /// <summary>
    /// Gets the entry whose closest collision point is nearest to the reference point.
    /// </summary>
    /// <param name="referencePoint">The point to compare distances from.</param>
    /// <param name="closestDistanceSquared">The squared distance to the closest collision point.</param>
    /// <returns>The closest <see cref="IntersectSpaceEntry"/> or null if none exist.</returns>
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
    /// <summary>
    /// Gets the entry whose furthest collision point is furthest from the reference point.
    /// </summary>
    /// <param name="referencePoint">The point to compare distances from.</param>
    /// <param name="furthestDistanceSquared">The squared distance to the furthest collision point.</param>
    /// <returns>The furthest <see cref="IntersectSpaceEntry"/> or null if none exist.</returns>
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
    /// <summary>
    /// Gets the collision point in all entries that is closest to the reference point.
    /// </summary>
    /// <param name="referencePoint">The point to compare distances from.</param>
    /// <param name="closestDistanceSquared">The squared distance to the closest collision point.</param>
    /// <returns>The closest <see cref="CollisionPoint"/> or a default value if none exist.</returns>
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
    /// <summary>
    /// Gets the collision point in all entries that is furthest from the reference point.
    /// </summary>
    /// <param name="referencePoint">The point to compare distances from.</param>
    /// <param name="furthestDistanceSquared">The squared distance to the furthest collision point.</param>
    /// <returns>The furthest <see cref="CollisionPoint"/> or a default value if none exist.</returns>
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
    /// <summary>
    /// Gets the collision point whose normal is most closely facing towards the specified reference point.
    /// </summary>
    /// <param name="referencePoint">The point to compare direction against.</param>
    /// <returns>The <see cref="CollisionPoint"/> facing most towards the reference point, or a default value if none exist.</returns>
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
    /// <summary>
    /// Gets the collision point whose normal is most closely facing towards the specified reference direction.
    /// </summary>
    /// <param name="referenceDirection">The direction to compare against.</param>
    /// <returns>The <see cref="CollisionPoint"/> facing most towards the reference direction, or a default value if none exist.</returns>
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
    /// <summary>
    /// Gets the collision point whose normal is most closely facing towards the position of the associated collision object.
    /// </summary>
    /// <returns>The <see cref="CollisionPoint"/> facing most towards the object's position.</returns>
    public CollisionPoint GetCollisionPointFacingTowardsPoint()
    {
        return GetCollisionPointFacingTowardsPoint(OtherCollisionObject.Transform.Position);
    }
    /// <summary>
    /// Gets the collision point whose normal is most closely facing towards the velocity of the associated collision object.
    /// </summary>
    /// <returns>The <see cref="CollisionPoint"/> facing most towards the object's velocity.</returns>
    public CollisionPoint GetCollisionPointFacingTowardsDir()
    {
        return GetCollisionPointFacingTowardsDir(OtherCollisionObject.Velocity);
    }
    #endregion
    
    
    #region Validation
    /// <summary>
    /// Validates all entries using the specified reference direction, removing invalid or misaligned collision points.
    /// </summary>
    /// <param name="referenceDirection">The direction to check collision point normals against.</param>
    /// <param name="combined">The averaged collision point of all remaining valid points.</param>
    /// <returns>True if there are valid points remaining; otherwise, false.</returns>
    /// <remarks>
    /// Removes:
    /// - Invalid <see cref="CollisionPoint"/>s
    /// - Points with normals facing in the same direction as the reference direction
    /// - Points with normals facing in the opposite direction as the reference point (from CollisionPoint towards the reference point)
    /// </remarks>
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
            var entry = this[i];
            var valid = entry.Validate(referenceDirection, out CollisionPoint combinedEntryPoint);
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
    /// Validates all entries using the specified reference direction and point, removing invalid or misaligned collision points.
    /// </summary>
    /// <param name="referenceDirection">The direction to check collision point normals against.</param>
    /// <param name="referencePoint">The point to check direction from.</param>
    /// <param name="combined">The averaged collision point of all remaining valid points.</param>
    /// <returns>True if there are valid points remaining; otherwise, false.</returns>
    /// <remarks>
    /// Removes:
    /// - Invalid <see cref="CollisionPoint"/>s
    /// - Points with normals facing in the same direction as the reference direction
    /// - Points with normals facing in the opposite direction as the reference point (from CollisionPoint towards the reference point)
    /// </remarks>
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
            var entry = this[i];
            // var valid = entry.ValidateSelf(out var combinedEntryPoint, out var closestToEntry);
            var valid = entry.Validate(referenceDirection, referencePoint, out CollisionPoint combinedEntryPoint);
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
    /// Validates all entries and provides both the averaged and closest collision points.
    /// </summary>
    /// <param name="referenceDirection">The direction to check collision point normals against.</param>
    /// <param name="referencePoint">The point to check direction from.</param>
    /// <param name="combined">The averaged collision point of all remaining valid points.</param>
    /// <param name="closest">The closest collision point to the reference point.</param>
    /// <returns>True if there are valid points remaining; otherwise, false.</returns>
    /// <remarks>
    /// Removes:
    /// - Invalid <see cref="CollisionPoint"/>s
    /// - Points with normals facing in the same direction as the reference direction
    /// - Points with normals facing in the opposite direction as the reference point (from CollisionPoint towards the reference point)
    /// </remarks>
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
            var valid = entry.Validate(referenceDirection, referencePoint, out CollisionPoint combinedEntryPoint, out CollisionPoint closestToEntry);
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
    /// Validates all entries using only a reference point, providing the averaged and closest collision points.
    /// </summary>
    /// <param name="referencePoint">The point to check direction from.</param>
    /// <param name="combined">The averaged collision point of all remaining valid points.</param>
    /// <param name="closest">The closest collision point to the reference point.</param>
    /// <returns>True if there are valid points remaining; otherwise, false.</returns>
    /// <remarks>
    /// Removes:
    /// - Invalid <see cref="CollisionPoint"/>s
    /// - Points with normals facing in the same direction as the reference direction
    /// - Points with normals facing in the opposite direction as the reference point (from CollisionPoint towards the reference point)
    /// </remarks>
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
            var entry = this[i];
            var valid = entry.Validate(referencePoint, out var combinedEntryPoint, out var closestToEntry);
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
    /// Validates all entries and provides a detailed validation result.
    /// </summary>
    /// <param name="referencePoint">The point to check direction from.</param>
    /// <param name="validationResult">The result containing combined, closest, furthest, and pointing-towards collision points.</param>
    /// <returns>True if there are valid points remaining; otherwise, false.</returns>
    /// <remarks>
    /// Removes:
    /// - Invalid <see cref="CollisionPoint"/>s
    /// - Points with normals facing in the same direction as the reference direction
    /// - Points with normals facing in the opposite direction as the reference point (from CollisionPoint towards the reference point)
    /// </remarks>
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
            var entry = this[i];
            var valid = entry.Validate(referencePoint, out CollisionPointValidationResult result);
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
    /// Validates all entries using both a reference direction and point, providing a detailed validation result.
    /// </summary>
    /// <param name="referenceDirection">The direction to check collision point normals against.</param>
    /// <param name="referencePoint">The point to check direction from.</param>
    /// <param name="validationResult">The result containing combined, closest, furthest, and pointing-towards collision points.</param>
    /// <returns>True if there are valid points remaining; otherwise, false.</returns>
    /// <remarks>
    /// Removes:
    /// - Invalid <see cref="CollisionPoint"/>s
    /// - Points with normals facing in the same direction as the reference direction
    /// - Points with normals facing in the opposite direction as the reference point (from CollisionPoint towards the reference point)
    /// </remarks>
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
    public bool ValidateByOther(out CollisionPoint combined, out CollisionPoint closest)
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
            var valid = entry.Validate(OtherCollisionObject.Velocity, OtherCollisionObject.Transform.Position, out var combinedEntryPoint, out var closestToEntry);
            if (!valid)
            {
                RemoveAt(i);
                continue;
            }
            avgPoint += combinedEntryPoint.Point;
            avgNormal += combinedEntryPoint.Normal;
            count++;
            var distanceSquared = (OtherCollisionObject.Transform.Position - combinedEntryPoint.Point).LengthSquared();
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
    public bool ValidateByOther(out CollisionPointValidationResult validationResult)
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
            var valid = entry.Validate(OtherCollisionObject.Velocity, OtherCollisionObject.Transform.Position, out CollisionPointValidationResult result);
            if (!valid)
            {
                RemoveAt(i);
                continue;
            }
            avgPoint += result.Combined.Point;
            avgNormal += result.Combined.Normal;
            count++;
            var dot = OtherCollisionObject.Velocity.Dot(result.PointingTowards.Normal);
            if (maxDot < 0 || dot > maxDot)
            {
                maxDot = dot;
                pointing = result.PointingTowards;
            }
            var distanceSquared = (OtherCollisionObject.Transform.Position - result.Closest.Point).LengthSquared();
            if (closestDistanceSquared < 0 ||  distanceSquared  < closestDistanceSquared)
            {
                closestDistanceSquared = distanceSquared;
                closest = result.Closest;
            }
            
            distanceSquared = (OtherCollisionObject.Transform.Position - result.Furthest.Point).LengthSquared();
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