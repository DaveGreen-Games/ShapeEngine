namespace ShapeEngine.Pathfinding;

/// <summary>
/// Represents a value that can be applied to a node, affecting its traversability or weight.
/// </summary>
public readonly struct NodeValue
{
    /// <summary>
    /// The value to apply. Higher numbers are more favorable, lower are less favorable.
    /// <remarks>
    /// For instance a value of 5 makes the cell distance 5 times shorter (more favorable) than a value of 0.
    /// A value of -5 makes the cell distance 5 times longer (less favorable) than a value of 0.
    /// BaseValue/Flat/Bonus default values are 0.
    /// </remarks>
    /// </summary>
    public readonly float Value;
    /// <summary>
    /// The type of value operation to apply.
    /// </summary>
    public readonly NodeValueType Type;
    /// <summary>
    /// The layer this value applies to (0 for default).
    /// </summary>
    public readonly uint Layer;
    /// <summary>
    /// Gets whether this value is valid (Type is not None).
    /// </summary>
    public bool Valid => Type != NodeValueType.None;
    /// <summary>
    /// Initializes a new default instance of <see cref="NodeValue"/>.
    /// </summary>
    public NodeValue()
    {
        Value = 0;
        Type = NodeValueType.None;
        Layer = 0;
    }
    /// <summary>
    /// Initializes a new instance of <see cref="NodeValue"/> with a type.
    /// </summary>
    /// <param name="type">The value type.</param>
    public NodeValue(NodeValueType type)
    {
        Value = 0f;
        Type = type;
        Layer = 0;
    }
    /// <summary>
    /// Initializes a new instance of <see cref="NodeValue"/> with a type and layer.
    /// </summary>
    /// <param name="type">The value type.</param>
    /// <param name="layer">The layer.</param>
    public NodeValue(NodeValueType type, uint layer)
    {
        Value = 0f;
        Type = type;
        Layer = layer;
    }
    /// <summary>
    /// Initializes a new instance of <see cref="NodeValue"/> with a value and type.
    /// </summary>
    /// <param name="value">The value to apply.</param>
    /// <param name="type">The value type.</param>
    public NodeValue(float value, NodeValueType type)
    {
        Value = value;
        Type = type;
        Layer = 0;
    }
    /// <summary>
    /// Initializes a new instance of <see cref="NodeValue"/> with a value, type, and layer.
    /// </summary>
    /// <param name="value">The value to apply.</param>
    /// <param name="type">The value type.</param>
    /// <param name="layer">The layer.</param>
    public NodeValue(float value, NodeValueType type, uint layer)
    {
        Value = value;
        Type = type;
        Layer = layer;
    }
}