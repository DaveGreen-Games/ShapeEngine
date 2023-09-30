using System.Numerics;

namespace ShapeEngine.Core.Structs;

public readonly struct ClosestPoint
{
    public readonly bool Valid => Closest.Valid;
    public readonly CollisionPoint Closest;
    public readonly float Distance;
    public readonly float DistanceSquared => Distance * Distance;
        
    public ClosestPoint()
    {
        Closest = new();
        Distance = 0f;
    }
    public ClosestPoint(Vector2 point, Vector2 normal, float distance)
    {
        Closest = new(point, normal);
        Distance = distance;
    }
    public ClosestPoint(CollisionPoint closest, float distance)
    {
        Closest = closest;
        Distance = distance;
    }
}