using System.Numerics;
using ShapeEngine.Core.Interfaces;

namespace ShapeEngine.Core.Collision;

public class QueryInfo
{
    public readonly Vector2 Origin;
    public readonly Collider Collider;
    public readonly QueryPoints Points;

    public QueryInfo(Collider collider, Vector2 origin)
    {
        this.Collider = collider;
        this.Origin = origin;
        this.Points = new();
    }
    public QueryInfo(Collider collider, Vector2 origin, CollisionPoints points)
    {
        this.Collider = collider;
        this.Origin = origin;
        this.Points = new(points, origin);
    }
}