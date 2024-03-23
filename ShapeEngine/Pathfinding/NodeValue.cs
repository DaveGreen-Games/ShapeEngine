namespace ShapeEngine.Pathfinding;

public readonly struct NodeValue
{
    /// <summary>
    /// Higher numbers mean the cell is more favorable
    /// Smaller numbers mean the cell is less favorable
    /// BaseValue/Flat/Bonus of 0 is default
    /// A value of 5 makes the cell distance 5 timer shorter, a value -5 make the cell distance 5 timer longer
    /// </summary>
    public readonly float Value;
    public readonly NodeValueType Type;
    public readonly uint Layer;

    public bool Valid => Type != NodeValueType.None;

    public NodeValue()
    {
        Value = 0;
        Type = NodeValueType.None;
        Layer = 0;
    }
    public NodeValue(NodeValueType type)
    {
        Value = 0f;
        Type = type;
        Layer = 0;
    }
    public NodeValue(NodeValueType type, uint layer)
    {
        Value = 0f;
        Type = type;
        Layer = layer;
    }
    public NodeValue(float value, NodeValueType type)
    {
        Value = value;
        Type = type;
        Layer = 0;
    }
    
    public NodeValue(float value, NodeValueType type, uint layer)
    {
        Value = value;
        Type = type;
        Layer = layer;
    }
}