using ShapeEngine.StaticLib;

namespace ShapeEngine.Random;

/// <summary>
/// Represents an item with an associated weight for use in weighted random selection.
/// </summary>
public struct WeightedItem <T>
{
    /// <summary>
    /// The item value.
    /// </summary>
    public T item { get; set; }
    /// <summary>
    /// The weight associated with the item. Must be non-negative.
    /// </summary>
    public int weight { get; set; }
    /// <summary>
    /// Initializes a new instance of the <see cref="WeightedItem{T}"/> struct with the specified item and weight.
    /// </summary>
    /// <param name="item">The item value.</param>
    /// <param name="weight">The weight for the item.</param>
    public WeightedItem(T item,  int weight)
    {
        this.item = item;
        this.weight = ShapeMath.AbsInt(weight);
    }
}