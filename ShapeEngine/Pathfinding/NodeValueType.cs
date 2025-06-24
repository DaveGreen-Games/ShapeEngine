namespace ShapeEngine.Pathfinding;

/// <summary>
/// Enumerates the types of node value operations that can be applied to a node.
/// </summary>
public enum NodeValueType
{
    /// <summary>
    /// No operation; the node value remains unchanged.
    /// </summary>
    None = -1,

    /// <summary>
    /// Resets the node to its default value.
    /// </summary>
    Reset = 0,

    /// <summary>
    /// Sets the node to a specific value.
    /// </summary>
    SetValue = 1,

    /// <summary>
    /// Resets the node and then sets it to a specific value.
    /// </summary>
    ResetThenSet = 2,

    /// <summary>
    /// Blocks the node, making it impassable.
    /// </summary>
    Block = 3,

    /// <summary>
    /// Unblocks the node, making it passable.
    /// </summary>
    Unblock = 4,

    /// <summary>
    /// Resets the node and then blocks it.
    /// </summary>
    ResetThenBlock = 5,

    /// <summary>
    /// Adds a bonus value to the node.
    /// </summary>
    AddBonus = 6,

    /// <summary>
    /// Removes a bonus value from the node.
    /// </summary>
    RemoveBonus = 7,

    /// <summary>
    /// Resets the bonus value of the node.
    /// </summary>
    ResetBonus = 8,

    /// <summary>
    /// Adds a flat value to the node.
    /// </summary>
    AddFlat = 9,

    /// <summary>
    /// Removes a flat value from the node.
    /// </summary>
    RemoveFlat = 10,

    /// <summary>
    /// Resets the flat value of the node.
    /// </summary>
    ResetFlat = 11
}