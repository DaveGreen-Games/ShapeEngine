using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Collision;

public class Intersection
{
    public readonly bool Valid;
    public readonly CollisionSurface CollisionSurface;
    public readonly CollisionPoint Closest;
    public readonly CollisionPoint Furthest;
    public readonly CollisionPoints? ColPoints;
    
    
    public CollisionPoint FirstCollisionPoint =>  ColPoints is { Count: > 0 } ? ColPoints[0] : new();
    
    public Intersection() { this.Valid = false; this.CollisionSurface = new(); this.ColPoints = null; Closest = new(); Furthest = new(); }
    public Intersection(CollisionPoints? points, Vector2 vel, Vector2 refPoint)
    {
        if (points == null || points.Count <= 0)
        {
            this.Valid = false;
            this.CollisionSurface = new();
            this.ColPoints = null;
            this.Closest = new();
            this.Furthest = new();
        }
        else if (points.Count == 1)
        {
            this.Valid = true;
            this.ColPoints = points;
            this.CollisionSurface = new(points[0].Point, points[0].Normal);
            this.Closest = points[0];
            this.Furthest = points[0];
        }
        else
        {
            var closest = points[0];
            var closestDist = Vector2.DistanceSquared(closest.Point, refPoint);
           
            var furthest = points[0];
            var furthestDist = closestDist;
            
            Vector2 avgPoint = new();
            Vector2 avgNormal = new();
            var count = 0;
            foreach (var p in points)
            {
                var dis = Vector2.DistanceSquared(p.Point, refPoint);
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
                
                if (DiscardNormal(p.Normal, vel)) continue;
                if (DiscardNormal(p, refPoint)) continue;

                count++;
                avgPoint += p.Point;
                avgNormal += p.Normal;
            }
            
            if (count > 0)
            {
                this.Valid = true;
                this.ColPoints = points;
                avgPoint /= count;
                avgNormal = avgNormal.Normalize();
                this.CollisionSurface = new(avgPoint, avgNormal);
            }
            else
            {
                this.Valid = false;
                this.ColPoints = points;
                this.CollisionSurface = new();
            }
            
            this.Closest = closest;
            this.Furthest = furthest;
        }
    }
    public Intersection(CollisionPoints? points)
    {
        this.Furthest = new();
        this.Closest = new();
        if (points == null || points.Count <= 0)
        {
            this.Valid = false;
            this.CollisionSurface = new();
            this.ColPoints = null;
        }
        else
        {
            this.Valid = true;
            this.ColPoints = points;

            Vector2 avgPoint = new();
            Vector2 avgNormal = new();
            foreach (var p in points)
            {
                avgPoint += p.Point;
                avgNormal += p.Normal;
            }
            if (points.Count > 0)
            {
                avgPoint /= points.Count;
                avgNormal = avgNormal.Normalize();
                this.CollisionSurface = new(avgPoint, avgNormal);
            }
            else this.CollisionSurface = new();
        }
    }

    public CollisionPoint GetClosestCollisionPoint(Vector2 referencePoint)
    {
        if (!Valid) return new();
        if(ColPoints == null || ColPoints.Count <= 0) return new();
        if(ColPoints.Count == 1) return ColPoints[0];
        
        var closest = ColPoints[0];
        var closestDist = (closest.Point - referencePoint).LengthSquared();
        for (var i = 1; i < ColPoints.Count; i++)
        {
            var p = ColPoints[i];
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
        if(ColPoints == null || ColPoints.Count <= 0) return new();
        if(ColPoints.Count == 1) return ColPoints[0];
        
        var furthest = ColPoints[0];
        var furthestDis = (furthest.Point - referencePoint).LengthSquared();
        for (var i = 1; i < ColPoints.Count; i++)
        {
            var p = ColPoints[i];
            var dis = (p.Point - referencePoint).LengthSquared();
            if (dis > furthestDis)
            {
                furthest = p;
                furthestDis = dis;
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
        if(ColPoints == null || ColPoints.Count <= 0) return new();
        if(ColPoints.Count == 1) return ColPoints[0];
        
        var best = ColPoints[0];
        var dir = (referencePoint - best.Point).Normalize();
        var maxDot = dir.Dot(best.Normal);
        
        for (var i = 1; i < ColPoints.Count; i++)
        {
            var p = ColPoints[i];
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
        if(ColPoints == null || ColPoints.Count <= 0) return new();
        if(ColPoints.Count == 1) return ColPoints[0];
        
        var best = ColPoints[0];
        var maxDot = referenceDir.Dot(best.Normal);
        
        for (var i = 1; i < ColPoints.Count; i++)
        {
            var p = ColPoints[i];
            var dot = referenceDir.Dot(p.Normal);
            if (dot > maxDot)
            {
                best = p;
                maxDot = dot;
            }
        }
        
        return best;
    }
    
    
    private static bool DiscardNormal(Vector2 n, Vector2 vel)
    {
        return n.IsFacingTheSameDirection(vel);
    }
    private static bool DiscardNormal(CollisionPoint p, Vector2 refPoint)
    {
        Vector2 dir = p.Point - refPoint;
        return p.Normal.IsFacingTheSameDirection(dir);
    }

    //public void FlipNormals(Vector2 refPoint)
    //{
    //    if (points.Count <= 0) return;
    //
    //    List<(Vector2 p, Vector2 n)> newPoints = new();
    //    foreach (var p in points)
    //    {
    //        Vector2 dir = refPoint - p.p;
    //        if (dir.IsFacingTheOppositeDirection(p.n)) newPoints.Add((p.p, p.n.Flip()));
    //        else newPoints.Add(p);
    //    }
    //    this.points = newPoints;
    //    this.n = points[0].n;
    //}
    //public Intersection CheckVelocityNew(Vector2 vel)
    //{
    //    List<(Vector2 p, Vector2 n)> newPoints = new();
    //    
    //    for (int i = points.Count - 1; i >= 0; i--)
    //    {
    //        var intersection = points[i];
    //        if (intersection.n.IsFacingTheSameDirection(vel)) continue;
    //        newPoints.Add(intersection);
    //    }
    //    return new(newPoints);
    //}

}