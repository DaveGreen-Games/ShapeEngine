using System.Numerics;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.CollisionSystem;

public class CollisionPoints : ShapeList<CollisionPoint>
{
    #region Constructors

    public CollisionPoints(int capacity = 0) : base(capacity)
    {
        
    }
    public CollisionPoints(params CollisionPoint[] points) : base(points.Length) { AddRange(points); }
    public CollisionPoints(IEnumerable<CollisionPoint> points, int count) : base(count)  { AddRange(points); }

    public CollisionPoints(CollisionPoints other) : base(other.Count)
    {
        AddRange(other);
    }
    
    #endregion

    #region Members

    public CollisionPoint First => Count > 0 ? this[0] : new CollisionPoint();
    public CollisionPoint Last => Count > 0 ? this[Count - 1] : new CollisionPoint();
    public bool Valid => Count > 0;
   
    #endregion
    
    #region Validation

    /// <summary>
    /// Removes:
    ///     - invalid CollisionPoints
    /// </summary>
    /// <param name="combined">An averaged CollisionPoint of all remaining CollisionPoints.</param>
    /// <returns>Returns true if there are valid points remaining</returns>
    public bool Validate(out CollisionPoint combined)
    {
        combined = new CollisionPoint();
        
        if (Count <= 0) return false;
        if (Count == 1)
        {
            var p = this[0];
            if (!p.Valid)
            {
                Clear();
                return false;
            } 
            combined = this[0];
            return true;
        }
        
        Vector2 avgPoint = new();
        Vector2 avgNormal = new();
        var count = 0;
        for (var i = Count - 1; i >= 0; i--)
        {
            var p = this[i];
            if (!p.Valid)
            {
                RemoveAt(i);
                continue;
            }
            
            count++;
            avgPoint += p.Point;
            avgNormal += p.Normal;
        }

        if (count <= 0) return false;
        
        avgPoint /= count;
        avgNormal = avgNormal.Normalize();
        combined = new CollisionPoint(avgPoint, avgNormal);
        return true;
    }
    /// <summary>
    /// Removes:
    /// - invalid CollisionPoints
    /// - CollisionPoints with normals facing in the same direction as the reference direction
    /// </summary>
    /// <param name="referenceDirection">The direction to check CollisionPoint Normals against.</param>
    /// <param name="combined">An averaged CollisionPoint of all remaining CollisionPoints.</param>
    /// <returns>Returns true if there are valid points remaining</returns>
    public bool Validate(Vector2 referenceDirection, out CollisionPoint combined)
    {
        combined = new CollisionPoint();
        
        if (Count <= 0) return false;
        if (Count == 1)
        {
            var p = this[0];
            if (!p.Valid || p.IsNormalFacing(referenceDirection))
            {
                Clear();
                return false;
            } 
            
            combined = this[0];
            return true;
        }
        
        Vector2 avgPoint = new();
        Vector2 avgNormal = new();
        var count = 0;
        for (var i = Count - 1; i >= 0; i--)
        {
            var p = this[i];
            if (!p.Valid || p.IsNormalFacing(referenceDirection))
            {
                RemoveAt(i);
                continue;
            }
            
            count++;
            avgPoint += p.Point;
            avgNormal += p.Normal;
        }

        if (count <= 0) return false;
        
        avgPoint /= count;
        avgNormal = avgNormal.Normalize();
        combined = new CollisionPoint(avgPoint, avgNormal);
        return true;
    }
    /// <summary>
    /// Removes:
    /// - invalid CollisionPoints
    /// - CollisionPoints with normals facing in the same direction as the reference direction
    /// - CollisionPoints with normals facing in the opposite direction as the reference point (from CollisionPoint towards the reference point)
    /// </summary>
    /// <param name="referenceDirection">The direction to check CollisionPoint normals against.</param>
    /// <param name="referencePoint">The direction from the reference point towards to CollisionPoint  to check CollisionPoint Normals against.</param>
    /// <param name="combined">An averaged CollisionPoint of all remaining CollisionPoints.</param>
    /// <returns>Returns true if there are valid points remaining</returns>
    public bool Validate(Vector2 referenceDirection, Vector2 referencePoint,  out CollisionPoint combined)
    {
        combined = new CollisionPoint();
        
        if (Count <= 0) return false;
        if (Count == 1)
        {
            var p = this[0];
            if (!p.Valid || p.IsNormalFacing(referenceDirection) || !p.IsNormalFacingPoint(referencePoint))
            {
                Clear();
                return false;
            } 
            
            combined = this[0];
            return true;
        }
        
        Vector2 avgPoint = new();
        Vector2 avgNormal = new();
        var count = 0;
        for (var i = Count - 1; i >= 0; i--)
        {
            var p = this[i];
            if (!p.Valid || p.IsNormalFacing(referenceDirection) || !p.IsNormalFacingPoint(referencePoint))
            {
                RemoveAt(i);
                continue;
            }
            
            count++;
            avgPoint += p.Point;
            avgNormal += p.Normal;
        }

        if (count <= 0) return false;
        
        avgPoint /= count;
        avgNormal = avgNormal.Normalize();
        combined = new CollisionPoint(avgPoint, avgNormal);
        return true;
    }
    /// <summary>
    /// Removes:
    /// - invalid CollisionPoints
    /// - CollisionPoints with normals facing in the same direction as the reference direction
    /// - CollisionPoints with normals facing in the opposite direction as the reference point (from CollisionPoint towards the reference point)
    /// </summary>
    /// <param name="referenceDirection">The direction to check CollisionPoint normals against.</param>
    /// <param name="referencePoint">The direction from the reference point towards to CollisionPoint  to check CollisionPoint Normals against.</param>
    /// <param name="combined">An averaged CollisionPoint of all remaining CollisionPoints.</param>
    /// <param name="closest">The CollisionPoint that is closest to the referencePoint.</param>
    /// <returns>Returns true if there are valid points remaining</returns>
    public bool Validate(Vector2 referenceDirection, Vector2 referencePoint,  out CollisionPoint combined, out CollisionPoint closest)
    {
        combined = new CollisionPoint();
        closest = new CollisionPoint();
        
        if (Count <= 0) return false;
        if (Count == 1)
        {
            var p = this[0];
            if (!p.Valid || p.IsNormalFacing(referenceDirection) || !p.IsNormalFacingPoint(referencePoint))
            {
                Clear();
                return false;
            } 
            
            combined = this[0];
            return true;
        }
        
        closest = this[0];
        var closestDist = Vector2.DistanceSquared(closest.Point, referencePoint);
           
        
        Vector2 avgPoint = new();
        Vector2 avgNormal = new();
        var count = 0;
        for (var i = Count - 1; i >= 0; i--)
        {
            var p = this[i];
            if (!p.Valid || p.IsNormalFacing(referenceDirection) || !p.IsNormalFacingPoint(referencePoint))
            {
                RemoveAt(i);
                continue;
            }
            
            var dis = Vector2.DistanceSquared(p.Point, referencePoint);
            if (dis < closestDist)
            {
                closestDist = dis;
                closest = p;
            }
            
            count++;
            avgPoint += p.Point;
            avgNormal += p.Normal;
        }

        if (count <= 0) return false;
        
        avgPoint /= count;
        avgNormal = avgNormal.Normalize();
        combined = new CollisionPoint(avgPoint, avgNormal);
        return true;
    }
    /// <summary>
    /// Removes:
    /// - invalid CollisionPoints
    /// - CollisionPoints with normals facing in the same direction as the reference direction
    /// - CollisionPoints with normals facing in the opposite direction as the reference point (from CollisionPoint towards the reference point)
    /// </summary>
    /// <param name="referenceDirection">The direction to check CollisionPoint normals against.</param>
    /// <param name="referencePoint">The direction from the reference point towards to CollisionPoint  to check CollisionPoint Normals against.</param>
    /// <param name="cleanResult">The result of the combined CollisionPoint, and the  closest/furthest collision point from the reference point, and the CollisionPoint with normal facing towards the referencePoint.</param>
    /// <returns>Returns true if there are valid points remaining</returns>
    public bool Validate(Vector2 referenceDirection, Vector2 referencePoint,  out CollisionPointValidationResult cleanResult)
    {
        CollisionPoint combined;
        CollisionPoint closest;
        CollisionPoint furthest;
        CollisionPoint pointingTowards;
        cleanResult = new CollisionPointValidationResult();
        
        if (Count <= 0) return false;
        if (Count == 1)
        {
            var p = this[0];
            if (!p.Valid || p.IsNormalFacing(referenceDirection) || !p.IsNormalFacingPoint(referencePoint))
            {
                Clear();
                return false;
            } 
            
            combined = this[0];
            closest = combined;
            furthest = combined;
            pointingTowards = combined;
            cleanResult = new CollisionPointValidationResult(combined, closest, furthest, pointingTowards);
            return true;
        }
        
        var startPoint = this[0];
        closest = startPoint;
        var closestDist = Vector2.DistanceSquared(closest.Point, referencePoint);
           
        furthest = startPoint;
        var furthestDist = closestDist;
        
        pointingTowards = startPoint;
        var maxDot = referenceDirection.Dot(pointingTowards.Normal);
        
        Vector2 avgPoint = new();
        Vector2 avgNormal = new();
        var count = 0;
        for (var i = Count - 1; i >= 0; i--)
        {
            var p = this[i];
            if (!p.Valid || p.IsNormalFacing(referenceDirection) || !p.IsNormalFacingPoint(referencePoint))
            {
                RemoveAt(i);
                continue;
            }
            
            var dis = Vector2.DistanceSquared(p.Point, referencePoint);
            if (dis < closestDist)
            {
                closestDist = dis;
                closest = p;
            }
            else if (dis > furthestDist)
            {
                furthestDist = dis;
                furthest = p;
            }
            var dot = referenceDirection.Dot(p.Normal);
            if (dot > maxDot)
            {
                pointingTowards = p;
                maxDot = dot;
            }
            count++;
            avgPoint += p.Point;
            avgNormal += p.Normal;
        }

        if (count <= 0) return false;
        
        avgPoint /= count;
        avgNormal = avgNormal.Normalize();
        combined = new CollisionPoint(avgPoint, avgNormal);
        cleanResult = new CollisionPointValidationResult(combined, closest, furthest, pointingTowards);
        return true;
    }
    /// <summary>
    /// Removes:
    /// - invalid CollisionPoints
    /// </summary>
    /// <param name="referencePoint">The direction from the reference point towards to CollisionPoint  to check CollisionPoint Normals against.</param>
    /// <param name="combined">An averaged CollisionPoint of all remaining CollisionPoints.</param>
    /// <param name="closest">The CollisionPoint that is closest to the referencePoint.</param>
    /// <returns>Returns true if there are valid points remaining</returns>
    public bool Validate(Vector2 referencePoint,  out CollisionPoint combined, out CollisionPoint closest)
    {
        combined = new CollisionPoint();
        closest = new CollisionPoint();
        
        if (Count <= 0) return false;
        if (Count == 1)
        {
            var p = this[0];
            if (!p.Valid)
            {
                Clear();
                return false;
            } 
            
            combined = this[0];
            return true;
        }
        
        closest = this[0];
        var closestDist = Vector2.DistanceSquared(closest.Point, referencePoint);
           
        
        Vector2 avgPoint = new();
        Vector2 avgNormal = new();
        var count = 0;
        for (var i = Count - 1; i >= 0; i--)
        {
            var p = this[i];
            if (!p.Valid)
            {
                RemoveAt(i);
                continue;
            }
            
            var dis = Vector2.DistanceSquared(p.Point, referencePoint);
            if (dis < closestDist)
            {
                closestDist = dis;
                closest = p;
            }
            
            count++;
            avgPoint += p.Point;
            avgNormal += p.Normal;
        }

        if (count <= 0) return false;
        
        avgPoint /= count;
        avgNormal = avgNormal.Normalize();
        combined = new CollisionPoint(avgPoint, avgNormal);
        return true;
    }
    /// <summary>
    /// Removes:
    /// - invalid CollisionPoints
    /// </summary>
    /// <param name="referencePoint">The direction from the reference point towards to CollisionPoint  to check CollisionPoint Normals against.</param>
    /// <param name="cleanResult">The result of the combined CollisionPoint, and the  closest/furthest collision point from the reference point, and the CollisionPoint with normal facing towards the referencePoint.</param>
    /// <returns>Returns true if there are valid points remaining</returns>
    public bool Validate(Vector2 referencePoint,  out CollisionPointValidationResult cleanResult)
    {
        CollisionPoint combined;
        CollisionPoint closest;
        CollisionPoint furthest;
        CollisionPoint pointingTowards;
        cleanResult = new CollisionPointValidationResult();
        
        if (Count <= 0) return false;
        if (Count == 1)
        {
            var p = this[0];
            if (!p.Valid)
            {
                Clear();
                return false;
            } 
            
            combined = this[0];
            closest = combined;
            furthest = combined;
            pointingTowards = combined;
            cleanResult = new CollisionPointValidationResult(combined, closest, furthest, pointingTowards);
            return true;
        }
        
        var startPoint = this[0];
        closest = startPoint;
        var closestDist = Vector2.DistanceSquared(closest.Point, referencePoint);
           
        furthest = startPoint;
        var furthestDist = closestDist;
        
        pointingTowards = startPoint;
        var referenceDirection = (startPoint.Point - referencePoint).Normalize();
        var maxDot = referenceDirection.Dot(pointingTowards.Normal);
        
        Vector2 avgPoint = new();
        Vector2 avgNormal = new();
        var count = 0;
        for (var i = Count - 1; i >= 0; i--)
        {
            var p = this[i];
            if (!p.Valid)
            {
                RemoveAt(i);
                continue;
            }
            
            var dis = Vector2.DistanceSquared(p.Point, referencePoint);
            if (dis < closestDist)
            {
                closestDist = dis;
                closest = p;
            }
            else if (dis > furthestDist)
            {
                furthestDist = dis;
                furthest = p;
            }
            
            referenceDirection = (p.Point - referencePoint).Normalize();
            var dot = referenceDirection.Dot(p.Normal);
            if (dot > maxDot)
            {
                pointingTowards = p;
                maxDot = dot;
            }
            count++;
            avgPoint += p.Point;
            avgNormal += p.Normal;
        }

        if (count <= 0) return false;
        
        avgPoint /= count;
        avgNormal = avgNormal.Normalize();
        combined = new CollisionPoint(avgPoint, avgNormal);
        cleanResult = new CollisionPointValidationResult(combined, closest, furthest, pointingTowards);
        return true;
    }
    
    #endregion
    
    #region Flip Normals
    public void FlipAllNormals()
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].FlipNormal();
        }
    }
    public void FlipNormalsTowardsPoint(Vector2 referencePoint)
    {
        for (var i = 0; i < Count; i++)
        {
            var p = this[i];
            var dir = referencePoint - p.Point;
            if (dir.IsFacingTheOppositeDirection(p.Normal))
                this[i] = p.FlipNormal();
        }
    }
    public void FlipNormalsTowardsDirection(Vector2 referenceDirection)
    {
        for (var i = 0; i < Count; i++)
        {
            var p = this[i];
            if (referenceDirection.IsFacingTheOppositeDirection(p.Normal))
                this[i] = p.FlipNormal();
        }
    }
    #endregion

    #region Equality

    public override int GetHashCode() { return Game.GetHashCode(this); }
    public bool Equals(CollisionPoints? other)
    {
        if (other == null) return false;
        if (Count != other.Count) return false;
        for (var i = 0; i < Count; i++)
        {
            if (this[i].Equals(other[i])) return false;
        }
        return true;
    }


    #endregion
    
    #region CollisionPoint 
    public CollisionPoint GetClosestCollisionPoint(Vector2 referencePoint)
    {
        if (!Valid) return new();
        if(Count == 1) return this[0];
        
        var closest = this[0];
        var closestDist = (closest.Point - referencePoint).LengthSquared();
        for (var i = 1; i < Count; i++)
        {
            var p = this[i];
            var dis = (p.Point - referencePoint).LengthSquared();
            if (dis < closestDist)
            {
                closest = p;
                closestDist = dis;
            }
        }
        
        return closest;
    }
    public CollisionPoint GetFurthestCollisionPoint(Vector2 referencePoint)
    {
        if (!Valid) return new();
        if(Count == 1) return this[0];
        
        var furthest = this[0];
        var furthestDis = (furthest.Point - referencePoint).LengthSquared();
        for (var i = 1; i < Count; i++)
        {
            var p = this[i];
            var dis = (p.Point - referencePoint).LengthSquared();
            if (dis > furthestDis)
            {
                furthest = p;
                furthestDis = dis;
            }
        }
        
        return furthest;
    }
    public CollisionPoint GetClosestCollisionPoint(Vector2 referencePoint, out float closestDistanceSquared)
    {
        closestDistanceSquared = -1;
        if (!Valid) return new();
        if(Count == 1) return this[0];
        
        var closest = this[0];
        closestDistanceSquared = (closest.Point - referencePoint).LengthSquared();
        for (var i = 1; i < Count; i++)
        {
            var p = this[i];
            var dis = (p.Point - referencePoint).LengthSquared();
            if (dis < closestDistanceSquared)
            {
                closest = p;
                closestDistanceSquared = dis;
            }
        }
        
        return closest;
    }
    public CollisionPoint GetFurthestCollisionPoint(Vector2 referencePoint, out float furthestDistanceSquared)
    {
        furthestDistanceSquared = -1;
        if (!Valid) return new();
        if(Count == 1) return this[0];
        
        var furthest = this[0];
        furthestDistanceSquared = (furthest.Point - referencePoint).LengthSquared();
        for (var i = 1; i < Count; i++)
        {
            var p = this[i];
            var dis = (p.Point - referencePoint).LengthSquared();
            if (dis > furthestDistanceSquared)
            {
                furthest = p;
                furthestDistanceSquared = dis;
            }
        }
        
        return furthest;
    }

    /// <summary>
    /// Finds the collision point with the normal facing most in the direction as the reference point.
    /// Each collision point normal is checked against the direction from the collision point towards the reference point.
    /// </summary>
    /// <returns></returns>
    public CollisionPoint GetCollisionPointFacingTowardsPoint(Vector2 referencePoint)
    {
        if (!Valid) return new();
        if(Count == 1) return this[0];
        
        var best = this[0];
        var dir = (referencePoint - best.Point).Normalize();
        var maxDot = dir.Dot(best.Normal);
        
        for (var i = 1; i < Count; i++)
        {
            var p = this[i];
            dir = (referencePoint - p.Point).Normalize();
            var dot = dir.Dot(p.Normal);
            if (dot > maxDot)
            {
                best = p;
                maxDot = dot;
            }
        }
        
        return best;
    }
   
    /// <summary>
    /// Finds the collision point with the normal facing most in the direction as the reference direction.
    /// </summary>
    /// <returns></returns>
    public CollisionPoint GetCollisionPointFacingTowardsDir(Vector2 referenceDir)
    {
        if (!Valid) return new();
        if(Count == 1) return this[0];
        
        var best = this[0];
        var maxDot = referenceDir.Dot(best.Normal);
        
        for (var i = 1; i < Count; i++)
        {
            var p = this[i];
            var dot = referenceDir.Dot(p.Normal);
            if (dot > maxDot)
            {
                best = p;
                maxDot = dot;
            }
        }
        
        return best;
    }
    
    #endregion
    
    #region Public
    public new CollisionPoints Copy() => new(this);
    public bool SortClosestFirst(Vector2 referencePoint)
    {
        if(Count <= 0) return false;
        if(Count == 1) return true;
        this.Sort
        (
            comparison: (a, b) =>
            {
                float la = (referencePoint - a.Point).LengthSquared();
                float lb = (referencePoint - b.Point).LengthSquared();

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
                float la = (referencePoint - a.Point).LengthSquared();
                float lb = (referencePoint - b.Point).LengthSquared();

                if (la < lb) return 1;
                if (MathF.Abs(x: la - lb) < 0.01f) return 0;
                return -1;
            }
        );
        return true;
    }
    

    public Points GetUniquePoints()
    {
        var uniqueVertices = new HashSet<Vector2>();
        for (var i = 0; i < Count; i++)
        {
            uniqueVertices.Add(this[i].Point);
        }
        return new(uniqueVertices);
    }
    public CollisionPoints GetUniqueCollisionPoints()
    {
        var unique = new HashSet<CollisionPoint>();
        for (var i = 0; i < Count; i++)
        {
            unique.Add(this[i]);
        }
        return new(unique, unique.Count);
    }
    
    #endregion
}