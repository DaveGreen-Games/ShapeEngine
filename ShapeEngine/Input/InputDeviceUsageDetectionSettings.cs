using Raylib_cs;

namespace ShapeEngine.Input;

/// <summary>
/// Represents the settings for detecting input device usage, including mouse, keyboard, and gamepad.
/// </summary>
public readonly partial struct InputDeviceUsageDetectionSettings
{
    /// <summary>
    /// Gets the default <see cref="InputDeviceUsageDetectionSettings"/> instance with preset values for mouse, keyboard, and gamepad.
    /// </summary>
    public static readonly InputDeviceUsageDetectionSettings DefaultSettings = new();
    
    /// <summary>
    /// The mouse input device usage detection settings.
    /// </summary>
    public readonly MouseSettings Mouse;
    
    /// <summary>
    /// The keyboard input device usage detection settings.
    /// </summary>
    public readonly KeyboardSettings Keyboard;
    
    /// <summary>
    /// The gamepad input device usage detection settings.
    /// </summary>
    public readonly GamepadSettings Gamepad;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="InputDeviceUsageDetectionSettings"/> struct
    /// with default settings for mouse, keyboard, and gamepad input device usage detection.
    /// </summary>
    public InputDeviceUsageDetectionSettings()
    {
        Mouse = MouseSettings.Default;
        Keyboard = KeyboardSettings.Default;
        Gamepad = GamepadSettings.Default;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="InputDeviceUsageDetectionSettings"/> struct
    /// with the specified settings for mouse, keyboard, and gamepad input device usage detection.
    /// </summary>
    /// <param name="mouse">The mouse input device usage detection settings.</param>
    /// <param name="keyboard">The keyboard input device usage detection settings.</param>
    /// <param name="gamepad">The gamepad input device usage detection settings.</param>
    public InputDeviceUsageDetectionSettings(MouseSettings mouse, KeyboardSettings keyboard, GamepadSettings gamepad)
    {
        Mouse = mouse;
        Keyboard = keyboard;
        Gamepad = gamepad;
    }
}