using ShapeEngine.StaticLib;

namespace ShapeEngine.Random;

public struct WeightedItem <T>
{
    public T item { get; set; }
    public int weight { get; set; }
    public WeightedItem(T item,  int weight)
    {
        this.item = item;
        this.weight = ShapeMath.AbsInt(weight);
    }
}