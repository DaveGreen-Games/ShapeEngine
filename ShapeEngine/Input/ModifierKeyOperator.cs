namespace ShapeEngine.Input;

/// <summary>
/// Specifies the logical operator to use when evaluating modifier keys.
/// </summary>
public enum ModifierKeyOperator
{
    /// <summary>
    /// At least one modifier key must be pressed (logical OR).
    /// </summary>
    Or = 0,
    /// <summary>
    /// All specified modifier keys must be pressed (logical AND).
    /// </summary>
    And = 1
}