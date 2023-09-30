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