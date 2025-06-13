namespace ShapeEngine.UI;

/// <summary>
/// Specifies how a control node can be selected in the UI system.
/// </summary>
public enum SelectFilter
{
    /// <summary>
    /// The control node cannot be selected.
    /// </summary>
    None = 0,
    /// <summary>
    /// The control node can be selected by mouse interaction only.
    /// </summary>
    Mouse = 1,
    /// <summary>
    /// The control node can be selected by navigation (keyboard/gamepad) only.
    /// </summary>
    Navigation = 2,
    /// <summary>
    /// The control node can be selected by both mouse and navigation.
    /// </summary>
    All = 3
}