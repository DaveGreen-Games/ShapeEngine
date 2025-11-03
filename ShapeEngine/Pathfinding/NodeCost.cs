namespace ShapeEngine.Pathfinding;

/// <summary>
/// Represents a cost that can be applied to a node, affecting its traversability or weight.
/// </summary>
public readonly struct NodeCost
{
    /// <summary>
    /// Resets any existing base value on the specified <paramref name="layer"/> and then sets the base value to <paramref name="baseValue"/>.
    /// </summary>
    /// <param name="baseValue">The base value to set.</param>
    /// <param name="layer">The layer this value applies to (0 for default).</param>
    /// <returns>A new <see cref="NodeCost"/> configured to reset then set the base value.</returns>
    public static NodeCost ResetThenSetBaseValue(float baseValue, uint layer = 0) => new(baseValue, NodeCostType.ResetThenSetBaseValue, layer);

    /// <summary>
    /// Sets the base value for the specified <paramref name="layer"/>.
    /// </summary>
    /// <param name="baseValue">The base value to set.</param>
    /// <param name="layer">The layer this value applies to (0 for default).</param>
    /// <returns>A new <see cref="NodeCost"/> configured to set the base value.</returns>
    public static NodeCost SetBaseValue(float baseValue, uint layer = 0) => new(baseValue, NodeCostType.SetBaseValue, layer);

    /// <summary>
    /// Adds a bonus value to the node's cost on the specified <paramref name="layer"/>.
    /// Higher numbers increase the cost; lower/negative numbers reduce it.
    /// </summary>
    /// <param name="bonusValue">The bonus value to add.</param>
    /// <param name="layer">The layer this value applies to (0 for default).</param>
    /// <returns>A new <see cref="NodeCost"/> configured to add a bonus.</returns>
    public static NodeCost AddBonus(float bonusValue, uint layer = 0) => new(bonusValue, NodeCostType.AddBonus, layer);

    /// <summary>
    /// Removes a previously applied bonus value from the node's cost on the specified <paramref name="layer"/>.
    /// </summary>
    /// <param name="bonusValue">The bonus value to remove.</param>
    /// <param name="layer">The layer this value applies to (0 for default).</param>
    /// <returns>A new <see cref="NodeCost"/> configured to remove a bonus.</returns>
    public static NodeCost RemoveBonus(float bonusValue, uint layer = 0) => new(bonusValue, NodeCostType.RemoveBonus, layer);

    /// <summary>
    /// Resets all bonus values on the specified <paramref name="layer"/> to default.
    /// </summary>
    /// <param name="layer">The layer to reset (0 for default).</param>
    /// <returns>A new <see cref="NodeCost"/> configured to reset bonuses.</returns>
    public static NodeCost ResetBonus(uint layer = 0) => new(NodeCostType.ResetBonus, layer);

    /// <summary>
    /// Adds a flat (additive) cost value to the node on the specified <paramref name="layer"/>.
    /// </summary>
    /// <param name="flatValue">The flat value to add.</param>
    /// <param name="layer">The layer this value applies to (0 for default).</param>
    /// <returns>A new <see cref="NodeCost"/> configured to add a flat value.</returns>
    public static NodeCost AddFlat(float flatValue, uint layer = 0) => new(flatValue, NodeCostType.AddFlat, layer);

    /// <summary>
    /// Removes a flat (additive) cost value from the node on the specified <paramref name="layer"/>.
    /// </summary>
    /// <param name="flatValue">The flat value to remove.</param>
    /// <param name="layer">The layer this value applies to (0 for default).</param>
    /// <returns>A new <see cref="NodeCost"/> configured to remove a flat value.</returns>
    public static NodeCost RemoveFlat(float flatValue, uint layer = 0) => new(flatValue, NodeCostType.RemoveFlat, layer);

    /// <summary>
    /// Resets all flat cost values on the specified <paramref name="layer"/> to default.
    /// </summary>
    /// <param name="layer">The layer to reset (0 for default).</param>
    /// <returns>A new <see cref="NodeCost"/> configured to reset flat values.</returns>
    public static NodeCost ResetFlat(uint layer = 0) => new(NodeCostType.ResetFlat, layer);

    /// <summary>
    /// Marks the node as blocked on the specified <paramref name="layer"/>.
    /// </summary>
    /// <param name="layer">The layer to block (0 for default).</param>
    /// <returns>A new <see cref="NodeCost"/> configured to block the node.</returns>
    public static NodeCost Block(uint layer = 0) => new(NodeCostType.Block, layer);

    /// <summary>
    /// Resets state on the specified <paramref name="layer"/> and then marks the node as blocked.
    /// </summary>
    /// <param name="layer">The layer to reset and block (0 for default).</param>
    /// <returns>A new <see cref="NodeCost"/> configured to reset then block the node.</returns>
    public static NodeCost ResetThenBlock(uint layer = 0) => new(NodeCostType.ResetThenBlock, layer);

    /// <summary>
    /// Removes a block from the node on the specified <paramref name="layer"/>, making it traversable.
    /// </summary>
    /// <param name="layer">The layer to unblock (0 for default).</param>
    /// <returns>A new <see cref="NodeCost"/> configured to unblock the node.</returns>
    public static NodeCost Unblock(uint layer = 0) => new(NodeCostType.Unblock, layer);

    /// <summary>
    /// Resets all cost modifications on the specified <paramref name="layer"/> to defaults.
    /// </summary>
    /// <param name="layer">The layer to reset (0 for default).</param>
    /// <returns>A new <see cref="NodeCost"/> configured to reset the node's cost.</returns>
    public static NodeCost Reset(uint layer = 0) => new(NodeCostType.Reset, layer);
    
    
    /// <summary>
    /// The value to apply.
    /// Higher numbers increase the cost /  are less favorable,
    /// lower/negative numbers reduce the cost/ are more favorable.
    /// The final value of a node will always be positive.
    /// </summary>
    public readonly float Value;
    /// <summary>
    /// The type of value operation to apply.
    /// </summary>
    public readonly NodeCostType Type;
    /// <summary>
    /// The layer this value applies to (0 for default).
    /// </summary>
    public readonly uint Layer;
    /// <summary>
    /// Gets whether this value is valid (Type is not None).
    /// </summary>
    public bool Valid => Type != NodeCostType.None;
    /// <summary>
    /// Initializes a new default instance of <see cref="NodeCost"/>.
    /// </summary>
    public NodeCost()
    {
        Value = 0;
        Type = NodeCostType.None;
        Layer = 0;
    }
    /// <summary>
    /// Initializes a new instance of <see cref="NodeCost"/> with a type.
    /// </summary>
    /// <param name="type">The value type.</param>
    public NodeCost(NodeCostType type)
    {
        Value = 0f;
        Type = type;
        Layer = 0;
    }
    /// <summary>
    /// Initializes a new instance of <see cref="NodeCost"/> with a type and layer.
    /// </summary>
    /// <param name="type">The value type.</param>
    /// <param name="layer">The layer.</param>
    public NodeCost(NodeCostType type, uint layer)
    {
        Value = 0f;
        Type = type;
        Layer = layer;
    }
    /// <summary>
    /// Initializes a new instance of <see cref="NodeCost"/> with a value and type.
    /// </summary>
    /// <param name="value">The value to apply.</param>
    /// <param name="type">The value type.</param>
    public NodeCost(float value, NodeCostType type)
    {
        Value = value;
        Type = type;
        Layer = 0;
    }
    /// <summary>
    /// Initializes a new instance of <see cref="NodeCost"/> with a value, type, and layer.
    /// </summary>
    /// <param name="value">The value to apply.</param>
    /// <param name="type">The value type.</param>
    /// <param name="layer">The layer.</param>
    public NodeCost(float value, NodeCostType type, uint layer)
    {
        Value = value;
        Type = type;
        Layer = layer;
    }
}