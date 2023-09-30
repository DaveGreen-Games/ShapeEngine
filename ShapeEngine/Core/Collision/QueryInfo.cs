using System.Numerics;
using ShapeEngine.Core.Interfaces;

namespace ShapeEngine.Core.Collision;

public class QueryInfo
{
    public readonly Vector2 Origin;
    public readonly ICollidable Collidable;
    public readonly QueryPoints Points;

    public QueryInfo(ICollidable collidable, Vector2 origin)
    {
        this.Collidable = collidable;
        this.Origin = origin;
        this.Points = new();
    }
    public QueryInfo(ICollidable collidable, Vector2 origin, CollisionPoints points)
    {
        this.Collidable = collidable;
        this.Origin = origin;
        this.Points = new(points, origin);
    }
}