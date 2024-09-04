using ShapeEngine.Lib;
using ShapeEngine.Random;
namespace ShapeEngine.Core.Shapes;

public class ShapeList<T> : List<T>
{
    public ShapeList(){}

    public ShapeList(int capacity) : base(capacity)
    {
        
    }
    public void AddRange(params T[] items) { AddRange(items as IEnumerable<T>);}
    
    /// <summary>
    /// Does not deep copy
    /// </summary>
    /// <returns></returns>
    public virtual ShapeList<T> Copy()
    {
        ShapeList<T> newList = new();
        newList.AddRange(this);
        return newList;
    }
    public bool IsIndexValid(int index)
    {
        return index >= 0 && index < Count;
    }
    public override int GetHashCode()
    {
        HashCode hash = new();
        foreach (var element in this)
        {
            hash.Add(element);
        }
        return hash.ToHashCode();
    }

    public T? GetRandomItem() => Rng.Instance.RandCollection(this);

    public List<T> GetRandomItems(int amount) => Rng.Instance.RandCollection(this, amount);
    public T? GetItem(int index) => Count <= 0 ? default(T) : this[ShapeMath.WrapIndex(Count, index)];
}