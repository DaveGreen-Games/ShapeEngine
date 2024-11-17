using System.Numerics;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.CollisionSystem;

public class CollisionPoints : ShapeList<CollisionPoint>
{
    public CollisionPoints()
    {
        
    }
    public CollisionPoints(params CollisionPoint[] points) { AddRange(points); }
    public CollisionPoints(IEnumerable<CollisionPoint> points) { AddRange(points); }


    
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

    public bool Valid => Count > 0;

    public void FlipNormals(Vector2 referencePoint)
    {
        for (var i = 0; i < Count; i++)
        {
            var p = this[i];
            var dir = referencePoint - p.Point;
            if (dir.IsFacingTheOppositeDirection(p.Normal))
                this[i] = this[i].FlipNormal();
        }
    }

    
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
    
    public void SortClosestFirst(Vector2 referencePoint)
    {
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
    }
    public void SortFurthestFirst(Vector2 referencePoint)
    {
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
        return new(unique);
    }

    
}



    
/*public CollisionPoint GetClosestCollisionPoint(Vector2 p)
{
    if (Count <= 0) return new();

    if (Count == 1) return this[0];


    var closestPoint = this[0];
    var minDisSq = (closestPoint.Point - p).LengthSquared();

    for (var i = 1; i < Count; i++)
    {
        var disSq = (this[i].Point - p).LengthSquared();
        if (disSq >= minDisSq) continue;
        minDisSq = disSq;
        closestPoint = this[i];
    }

    return closestPoint;
}
public ClosestDistance GetClosestDistanceTo(Vector2 p)
{
    if (Count <= 0) return new();

    if (Count == 1) return new(this[0].Point, p);


    var closestPoint = this[0];
    var minDisSq = (closestPoint.Point - p).LengthSquared();

    for (var i = 1; i < Count; i++)
    {
        var disSq = (this[i].Point - p).LengthSquared();
        if (disSq >= minDisSq) continue;
        minDisSq = disSq;
        closestPoint = this[i];
    }

    return new(closestPoint.Point, p);
}
*/


