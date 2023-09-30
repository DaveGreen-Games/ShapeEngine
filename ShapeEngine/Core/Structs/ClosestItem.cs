using System.Numerics;

namespace ShapeEngine.Core.Structs;

public readonly struct ClosestItem<T> where T : struct
{
    public readonly T Item;
    public readonly ClosestPoint Point;
    public readonly bool Valid => Point.Valid;
    public ClosestItem()
    {
        Item = default(T);
        Point = new();
    }
    public ClosestItem(T item, CollisionPoint point, float distance)
    {
        Item = item;
        Point = new(point, distance);
    }
    public ClosestItem(T item, Vector2 point, Vector2 normal, float distance)
    {
        Item = item;
        Point = new(point, normal, distance);
            
    }
}