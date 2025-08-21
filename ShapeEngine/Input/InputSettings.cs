
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
    /// Indicates whether to load and apply the embedded gamepad mapping file on startup.
    /// If set to false, you can use <see cref="GamepadMappingsFilePath"/> to specify a custom mapping file.
    /// The default mapping file source is: https://github.com/mdqinc/SDL_GameControllerDB/blob/master/gamecontrollerdb.txt
    /// </summary>
    public bool LoadEmbeddedGamepadMappings;
    
    /// <summary>
    /// Additional gamepad mappings to apply at startup.
    /// Works regardless of the value of <see cref="LoadEmbeddedGamepadMappings"/> and with <see cref="GamepadMappingsFilePath"/>.
    /// Each string should represent a valid gamepad mapping.
    /// </summary>
    /// <remarks>
    /// These mappings are always applied after the embedded mappings (if <see cref="LoadEmbeddedGamepadMappings"/> is true)
    /// or after the mappings loaded from the file specified by <see cref="GamepadMappingsFilePath"/>.
    /// You can look at the format of the mappings in the default mapping file source: https://github.com/mdqinc/SDL_GameControllerDB/blob/master/gamecontrollerdb.txt for reference.
    /// </remarks>
    public List<string>? GamepadMappings;
    
    /// <summary>
    /// Absolute path to a text file containing gamepad mappings.
    /// The entire file will be loaded and the mappings applied.
    /// Only used if <see cref="LoadEmbeddedGamepadMappings"/> is false.
    /// </summary>
    public string? GamepadMappingsFilePath;
    
    
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
        GamepadMappingsFilePath = null;
        GamepadMappings = null;
        LoadEmbeddedGamepadMappings = true;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="InputSettings"/> class
    /// with the specified settings for mouse, keyboard, and gamepad input device usage detection.
    /// </summary>
    /// <param name="mouse">The mouse input device usage detection settings.</param>
    /// <param name="keyboard">The keyboard input device usage detection settings.</param>
    /// <param name="gamepad">The gamepad input device usage detection settings.</param>
    /// <param name="maxGamepadCount">Specifies the maximum number of gamepads supported by the input system. Setting this to 0 or negative values disables gamepad handling.</param>
    /// <param name="loadEmbeddedGamepadMappings">Indicates whether to load and apply the embedded gamepad mapping file on startup.</param>
    /// <param name="gamepadMappingsFilePath">Absolute path to a text file containing gamepad mappings. Only used if <paramref name="loadEmbeddedGamepadMappings"/> is false.</param>
    /// <param name="gamepadMappings">Additional gamepad mappings to apply at startup. Each string should represent a valid gamepad mapping.</param>
    public InputSettings(MouseSettings mouse, KeyboardSettings keyboard, GamepadSettings gamepad, int maxGamepadCount = DefaultMaxGamepadCount, bool loadEmbeddedGamepadMappings = true, string? gamepadMappingsFilePath = null, List<string>? gamepadMappings = null)
        : this()
    {
        Mouse = mouse;
        Keyboard = keyboard;
        Gamepad = gamepad;
        MaxGamepadCount = maxGamepadCount;
        LoadEmbeddedGamepadMappings = loadEmbeddedGamepadMappings;
        GamepadMappingsFilePath = gamepadMappingsFilePath;
        GamepadMappings = gamepadMappings;
    }
}