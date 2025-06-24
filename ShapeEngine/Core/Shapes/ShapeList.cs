using ShapeEngine.StaticLib;
using ShapeEngine.Random;
namespace ShapeEngine.Core.Shapes;

/// <summary>
/// A strongly-typed list for shapes, providing utility methods for copying, random access, and index validation.
/// </summary>
/// <typeparam name="T">The type of elements in the list.</typeparam>
/// <remarks>
/// ShapeList does not perform deep copies in its <see cref="Copy"/> method.
/// </remarks>
public class ShapeList<T> : List<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShapeList{T}"/> class.
    /// </summary>
    public ShapeList(){}

    /// <summary>
    /// Initializes a new instance of the <see cref="ShapeList{T}"/> class with the specified capacity.
    /// </summary>
    /// <param name="capacity">The number of elements the list can initially store.</param>
    public ShapeList(int capacity) : base(capacity)
    {
        
    }
    /// <summary>
    /// Adds a range of items to the list.
    /// </summary>
    /// <param name="items">The items to add.</param>
    public void AddRange(params T[] items) { AddRange(items as IEnumerable<T>);}
    
    /// <summary>
    /// Creates a shallow copy of this list.
    /// </summary>
    /// <returns>A new <see cref="ShapeList{T}"/> containing the same elements.</returns>
    /// <remarks>
    /// This method does not perform a deep copy of the elements.
    /// </remarks>
    public virtual ShapeList<T> Copy()
    {
        ShapeList<T> newList = new();
        newList.AddRange(this);
        return newList;
    }
    /// <summary>
    /// Determines whether the specified index is valid for this list.
    /// </summary>
    /// <param name="index">The index to check.</param>
    /// <returns>True if the index is valid; otherwise, false.</returns>
    public bool IsIndexValid(int index)
    {
        return index >= 0 && index < Count;
    }
    /// <summary>
    /// Returns a hash code for the list based on its elements.
    /// </summary>
    /// <returns>The hash code for the list.</returns>
    public override int GetHashCode()
    {
        HashCode hash = new();
        foreach (var element in this)
        {
            hash.Add(element);
        }
        return hash.ToHashCode();
    }

    /// <summary>
    /// Gets a random item from the list, or null if the list is empty.
    /// </summary>
    /// <returns>A random item, or null if the list is empty.</returns>
    public T? GetRandomItem() => Rng.Instance.RandCollection(this);

    /// <summary>
    /// Gets a list of random items from the list.
    /// </summary>
    /// <param name="amount">The number of random items to retrieve.</param>
    /// <returns>A list of random items.</returns>
    public List<T> GetRandomItems(int amount) => Rng.Instance.RandCollection(this, amount);
    /// <summary>
    /// Gets the item at the specified index, wrapping the index if necessary.
    /// </summary>
    /// <param name="index">The index of the item to retrieve.</param>
    /// <returns>The item at the wrapped index, or the default value if the list is empty.</returns>
    public T? GetItem(int index) => Count <= 0 ? default(T) : this[ShapeMath.WrapIndex(Count, index)];
    
    /// <summary>
    /// Retrieves and removes a random item from the list.
    /// </summary>
    /// <returns>The removed random item, or the default value if the list is empty.</returns>
    public T? TakeRandomItem()
    {
        if (Count == 0) return default(T);
        int index = Rng.Instance.RandI(0, Count);
        var item = this[index];
        RemoveAt(index);
        return item;
    }
}