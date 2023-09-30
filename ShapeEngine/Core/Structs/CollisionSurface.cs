using System.Numerics;

namespace ShapeEngine.Core.Structs;

public readonly struct CollisionSurface
{
    public readonly Vector2 Point;
    public readonly Vector2 Normal;
    public readonly bool Valid => Normal.X != 0f || Normal.Y != 0f;

    public CollisionSurface() { Point = new(); Normal = new();}
    public CollisionSurface(Vector2 point, Vector2 normal)
    {
        this.Point = point;
        this.Normal = normal;
    }

}