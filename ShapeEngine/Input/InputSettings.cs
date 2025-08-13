
namespace ShapeEngine.Input;

/// <summary>
/// Represents the settings for detecting input device usage, including mouse, keyboard, and gamepad.
/// </summary>
public partial class InputSettings
{
    /// <summary>
    /// The default maximum number of gamepads supported by the manager.
    /// </summary>
    public const int DefaultMaxGamepadCount = 4;
    
    /// <summary>
    /// Gets the default <see cref="InputSettings"/> instance with preset values for mouse, keyboard, and gamepad.
    /// </summary>
    public static readonly InputSettings Default = new();
    
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
    /// Specifies the maximum number of gamepads supported by the input system.
    /// </summary>
    /// <remarks>
    /// Setting this to 0 or negative values disables gamepad handling.
    /// <see cref="GamepadDeviceManager"/> will not create any gamepad devices, even if gamepads are connected.
    /// </remarks>
    public readonly int MaxGamepadCount;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="InputSettings"/> struct
    /// with default settings for mouse, keyboard, and gamepad input device usage detection.
    /// </summary>
    public InputSettings()
    {
        Mouse = MouseSettings.Default;
        Keyboard = KeyboardSettings.Default;
        Gamepad = GamepadSettings.Default;
        MaxGamepadCount = DefaultMaxGamepadCount;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InputSettings"/> struct
    /// with the specified settings for mouse, keyboard, and gamepad input device usage detection.
    /// </summary>
    /// <param name="mouse">The mouse input device usage detection settings.</param>
    /// <param name="keyboard">The keyboard input device usage detection settings.</param>
    /// <param name="gamepad">The gamepad input device usage detection settings.</param>
    /// <param name="maxGamepadCount">Specifies the maximum number of gamepads supported by the input system. Setting this to 0 or negative values disables gamepad handling.</param>
    public InputSettings(MouseSettings mouse, KeyboardSettings keyboard, GamepadSettings gamepad, int maxGamepadCount = DefaultMaxGamepadCount)
        : this()
    {
        Mouse = mouse;
        Keyboard = keyboard;
        Gamepad = gamepad;
        MaxGamepadCount = maxGamepadCount;
    }
}