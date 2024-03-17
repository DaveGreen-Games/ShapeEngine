using System.Numerics;

namespace ShapeEngine.Core.Structs;

public readonly struct ClosestItem<T> where T : struct
{
    public readonly T Item;
    public readonly ClosestDistance ClosestDistance;
    public readonly bool Valid => ClosestDistance.Valid;
    public ClosestItem()
    {
        Item = default(T);
        ClosestDistance = new();
    }
    public ClosestItem(T item, ClosestDistance cd)
    {
        Item = item;
        ClosestDistance = cd;
    }
    public ClosestItem(T item, Vector2 itemPoint, Vector2 point)
    {
        Item = item;
        ClosestDistance = new(itemPoint, point);
            
    }
}