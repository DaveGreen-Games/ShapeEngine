using System.Numerics;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.CollisionSystem;

public class QueryPoints
{
    public readonly bool Valid;
    public readonly CollisionPoints Points;
    public readonly CollisionPoint Closest;

    public QueryPoints()
    {
        this.Valid = false;
        this.Points = new();
        this.Closest = new();
    }
    public QueryPoints(CollisionPoints points, Vector2 origin)
    {
        if(points.Count <= 0)
        {
            this.Valid = false;
            this.Points = new();
            this.Closest = new();
        }
        else
        {
            this.Valid = true;
            points.SortClosestFirst(origin);
            this.Points = points;
            this.Closest = points[0];
        }
    }
}