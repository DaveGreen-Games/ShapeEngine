namespace ShapeEngine.Core;

/// <summary>
/// Defines the border style and resizing behavior of a window.
/// </summary>
public enum WindowBorder
{
    /// <summary>
    /// Window with a border that can be resized by the user.
    /// </summary>
    Resizabled = 0,
    
    /// <summary>
    /// Window with a border that cannot be resized by the user.
    /// </summary>
    Fixed = 1,
    
    /// <summary>
    /// Window without any border decoration or controls.
    /// </summary>
    Undecorated = 2
}