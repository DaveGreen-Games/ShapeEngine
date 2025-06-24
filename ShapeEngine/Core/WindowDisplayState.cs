namespace ShapeEngine.Core;

/// <summary>
/// Represents the display state of a window.
/// </summary>
public enum WindowDisplayState
{
    /// <summary>
    /// The window is in its normal state.
    /// </summary>
    Normal = 0,
    /// <summary>
    /// The window is minimized.
    /// </summary>
    Minimized = 1,
    /// <summary>
    /// The window is maximized.
    /// </summary>
    Maximized = 2,
    /// <summary>
    /// The window is in fullscreen mode.
    /// </summary>
    Fullscreen = 3,
    /// <summary>
    /// The window is in borderless fullscreen mode.
    /// </summary>
    BorderlessFullscreen = 4
}