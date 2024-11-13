using System.Numerics;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Structs;

public readonly struct CollisionPoint : IEquatable<CollisionPoint>
{
    public readonly bool Valid => Normal.X != 0f || Normal.Y != 0f;
    public readonly Vector2 Point;
    public readonly Vector2 Normal;

    public CollisionPoint() 
    { 
        Point = new(); 
        Normal = new();
    }

    public CollisionPoint(Vector2 p, Vector2 n)
    {
        Point = p; 
        Normal = n;
    }

    public CollisionPoint(CollisionSurface surface)
    {
        Point = surface.Point;
        Normal = surface.Normal;
    }
    public CollisionSurface ToCollisionSurface() => new CollisionSurface(Point, Normal);
    
    public CollisionSurface Average(CollisionPoint other) => new((Point + other.Point) / 2, (Normal + other.Normal).Normalize());

    public static CollisionSurface Average(CollisionPoint a, CollisionPoint b) => new((a.Point + b.Point) / 2, (a.Normal + b.Normal).Normalize());

    public static CollisionSurface Average(params CollisionPoint[] points)
    {
        if(points.Length == 0) return new CollisionSurface();
        var avgPoint = Vector2.Zero;
        var avgNormal = Vector2.Zero;
        foreach (var point in points)
        {
            avgPoint += point.Point;
            avgNormal += point.Normal;
        }
        return new CollisionSurface(avgPoint / points.Length, avgNormal.Normalize());
    }
    
    public bool Equals(CollisionPoint other)
    {
        return other.Point == Point && other.Normal == Normal;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(Point, Normal);
    }

    public CollisionPoint FlipNormal()
    {
        return new(Point, Normal.Flip());
    }
    public CollisionPoint FlipNormal(Vector2 referencePoint)
    {
        Vector2 dir = referencePoint - Point;
        if (dir.IsFacingTheOppositeDirection(Normal)) return FlipNormal();

        return this;
    }
}