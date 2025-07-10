namespace ShapeEngine.Input;

/// <summary>
/// Represents the type of input device.
/// </summary>
public enum InputDeviceType
{
    /// <summary>
    /// No input device.
    /// </summary>
    None = 0,
    
    /// <summary>
    /// Keyboard input device.
    /// </summary>
    Keyboard = 1,

    /// <summary>
    /// Mouse input device.
    /// </summary>
    Mouse = 2,

    /// <summary>
    /// Gamepad input device.
    /// </summary>
    Gamepad = 3
}