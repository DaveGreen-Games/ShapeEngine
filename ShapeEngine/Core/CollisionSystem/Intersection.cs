using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.CollisionSystem;

//todo should all the filtering and finding of closest/furthest/combined be done automatically?
//todo there could be a simple function to all of this and save it here?
//todo maybe some lazy initialization?
public class Intersection : CollisionPoints
{
    /// <summary>
    /// Combined and averaged collision point of all collision points.
    /// </summary>
    public readonly CollisionPoint Combined;
    /// <summary>
    /// Closest collision point to the reference point.
    /// </summary>
    public readonly CollisionPoint Closest;
    /// <summary>
    /// Furthest collision point to the reference point.
    /// </summary>
    public readonly CollisionPoint Furthest;
    public CollisionPoint FirstCollisionPoint =>  this  is { Count: > 0 } ? this[0] : new();
    
    public Intersection(CollisionPoints? points, Vector2 refPoint)
    {
        if (points == null || points.Count <= 0)
        {
            this.Combined = new();
            this.Closest = new();
            this.Furthest = new();
        }
        else if (points.Count == 1)
        {
            this.Combined = points[0];
            this.Closest = points[0];
            this.Furthest = points[0];
            Add(points[0]);
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
                if (!p.Valid) continue;
                
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
                
                count++;
                avgPoint += p.Point;
                avgNormal += p.Normal;
                Add(p);
            }
            
            if (count > 0)
            {
                avgPoint /= count;
                avgNormal = avgNormal.Normalize();
                this.Combined = new(avgPoint, avgNormal);
            }
            else
            {
                this.Combined = new();
            }
            
            this.Closest = closest;
            this.Furthest = furthest;
        }
    }
    public Intersection(CollisionPoints? points, Vector2 vel, Vector2 refPoint, bool filterCollisionPoints)
    {
        if (points == null || points.Count <= 0)
        {
            this.Combined = new();
            this.Closest = new();
            this.Furthest = new();
        }
        else if (points.Count == 1)
        {
            this.Combined = points[0];
            this.Closest = points[0];
            this.Furthest = points[0];
            Add(points[0]);
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
                if (!p.Valid) continue;
                
                if (filterCollisionPoints)
                {
                    if (DiscardNormal(p.Normal, vel)) continue;
                    if (DiscardNormal(p, refPoint)) continue;
                }
                
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
                
                count++;
                avgPoint += p.Point;
                avgNormal += p.Normal;
                Add(p);
            }
            
            if (count > 0)
            {
                avgPoint /= count;
                avgNormal = avgNormal.Normalize();
                this.Combined = new(avgPoint, avgNormal);
            }
            else
            {
                this.Combined = new();
            }
            
            this.Closest = closest;
            this.Furthest = furthest;
        }
    }
    
    private Intersection(Intersection intersection)
    {
        Combined = intersection.Combined;
        Closest = intersection.Closest;
        Furthest = intersection.Furthest;
        if (intersection.Count > 0) AddRange(intersection);
        
    }
    public new Intersection Copy() => new(this);
    
    
    private static bool DiscardNormal(Vector2 n, Vector2 vel) => n.IsFacingTheSameDirection(vel);
    private static bool DiscardNormal(CollisionPoint p, Vector2 refPoint) => p.Normal.IsFacingTheSameDirection( p.Point - refPoint);
}

/*public CollisionPoint GetClosestCollisionPoint(Vector2 referencePoint)
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
    */
    
    