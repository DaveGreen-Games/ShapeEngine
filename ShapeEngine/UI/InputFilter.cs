namespace ShapeEngine.UI;

/// <summary>
/// Specifies which input methods a control node responds to in the UI system.
/// </summary>
public enum InputFilter
{
    /// <summary>
    /// The control node does not respond to any input.
    /// </summary>
    None = 0,
    /// <summary>
    /// The control node responds only to mouse input.
    /// </summary>
    MouseOnly = 1,
    /// <summary>
    /// The control node responds to all input except mouse.
    /// </summary>
    MouseNever = 2,
    /// <summary>
    /// The control node responds to all input methods (mouse, keyboard, gamepad, etc.).
    /// </summary>
    All = 3
}