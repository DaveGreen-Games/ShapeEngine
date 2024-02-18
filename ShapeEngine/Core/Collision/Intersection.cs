using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Collision;

public class Intersection
{
    public readonly bool Valid;
    public readonly CollisionSurface CollisionSurface;
    public readonly CollisionPoints? ColPoints;

    public Intersection() { this.Valid = false; this.CollisionSurface = new(); this.ColPoints = null; }
    public Intersection(CollisionPoints? points, Vector2 vel, Vector2 refPoint)
    {
        if (points == null || points.Count <= 0)
        {
            this.Valid = false;
            this.CollisionSurface = new();
            this.ColPoints = null;
        }
        else
        {
            Vector2 avgPoint = new();
            Vector2 avgNormal = new();
            var count = 0;
            foreach (var p in points)
            {
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
        }
    }
    public Intersection(CollisionPoints? points)
    {
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