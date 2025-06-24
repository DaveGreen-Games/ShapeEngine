namespace ShapeEngine.UI;

/// <summary>
/// Specifies how a control node handles mouse interaction in the UI system.
/// </summary>
public enum MouseFilter
{
    /// <summary>
    /// The control node ignores mouse input.
    /// </summary>
    Ignore = 0,
    /// <summary>
    /// The control node processes mouse input and allows it to pass to underlying nodes.
    /// </summary>
    Pass = 1,
    /// <summary>
    /// The control node processes mouse input and stops it from passing to underlying nodes.
    /// </summary>
    Stop = 2
}