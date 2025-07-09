using Raylib_cs;

namespace ShapeEngine.Input;

/// <summary>
/// Represents the settings for detecting input device usage, including mouse, keyboard, and gamepad.
/// </summary>
public readonly struct InputDeviceUsageDetectionSettings
{
    /// <summary>
    /// Represents the settings used for detecting mouse input device usage,
    /// including thresholds and detection options.
    /// </summary>
    public readonly struct MouseSettings
    {
        /// <summary>
        /// Gets the default <see cref="MouseSettings"/> instance with preset values.
        /// </summary>
        /// <code>
        /// Detection = true;
        /// MoveThreshold = 0.5f;
        /// WheelThreshold = 0.25f;
        /// MinPressCount = 2;
        /// MinPressInterval = 1f;
        /// MinUsedDuration = 0.5f;
        /// RequireSpecialButtonForSelection = false;
        /// SelectionButtonPrimary = ShapeMouseButton.NONE;
        /// SelectionButtonSecondary = ShapeMouseButton.NONE;
        /// </code>
        public static readonly MouseSettings Default = new();
        /// <summary>
        /// Initializes a new instance of the <see cref="MouseSettings"/> struct with default values.
        /// </summary>
        /// <code>
        /// Detection = true;
        /// MoveThreshold = 0.5f;
        /// WheelThreshold = 0.25f;
        /// MinPressCount = 2;
        /// MinPressInterval = 1f;
        /// MinUsedDuration = 0.5f;
        /// RequireSpecialButtonForSelection = false;
        /// SelectionButtonPrimary = ShapeMouseButton.NONE;
        /// SelectionButtonSecondary = ShapeMouseButton.NONE;
        /// </code>
        public MouseSettings()
        {
            Detection = true;
            MoveThreshold = 0.5f;
            WheelThreshold = 0.25f;
            MinPressCount = 2;
            MinPressInterval = 1f;
            MinUsedDuration = 0.5f;
            RequireSpecialButtonForSelection = false;
            SelectionButtonPrimary = ShapeMouseButton.NONE;
            SelectionButtonSecondary = ShapeMouseButton.NONE;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MouseSettings"/> struct with custom detection, move threshold, and wheel threshold values.
        /// Other values are set to their defaults: MinPressCount = 2, MinPressInterval = 1f, MinUsedDuration = 0.5f.
        /// </summary>
        /// <param name="detection">Indicates whether mouse detection is enabled.</param>
        /// <param name="moveThreshold">The minimum movement threshold to consider the mouse as "used".</param>
        /// <param name="wheelThreshold">The minimum mouse wheel movement threshold to consider the mouse as "used".</param>
        public MouseSettings(bool detection, float moveThreshold, float wheelThreshold)
        {
            Detection = detection;
            MoveThreshold = moveThreshold;
            WheelThreshold = wheelThreshold;
            MinPressCount = 2;
            MinPressInterval = 1f;
            MinUsedDuration = 0.5f;
            RequireSpecialButtonForSelection = false;
            SelectionButtonPrimary = ShapeMouseButton.NONE;
            SelectionButtonSecondary = ShapeMouseButton.NONE;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MouseSettings"/> struct with custom values.
        /// <see cref="Detection"/> is set to <c>true</c>.
        /// </summary>
        /// <param name="moveThreshold">The minimum movement threshold to consider the mouse as "used".</param>
        /// <param name="wheelThreshold">The minimum mouse wheel movement threshold to consider the mouse as "used".</param>
        /// <param name="minPressCount">The minimum number of mouse button presses required within the specified interval.</param>
        /// <param name="minPressInterval">The time interval (in seconds) from the first mouse button press within which <paramref name="minPressCount"/> must be reached.</param>
        /// <param name="minUsedDuration">The minimum duration (in seconds) of consecutive mouse use required to consider the mouse device as "used".</param>
        public MouseSettings(float moveThreshold, float wheelThreshold, int minPressCount, float minPressInterval, float minUsedDuration)
        {
            Detection = true;
            MoveThreshold = moveThreshold;
            WheelThreshold = wheelThreshold;
            MinPressCount = minPressCount;
            MinPressInterval = minPressInterval;
            MinUsedDuration = minUsedDuration;
            RequireSpecialButtonForSelection = false;
            SelectionButtonPrimary = ShapeMouseButton.NONE;
            SelectionButtonSecondary = ShapeMouseButton.NONE;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MouseSettings"/> struct with custom values,
        /// including detection, move threshold, wheel threshold, minimum press count, minimum press interval,
        /// minimum used duration, and custom selection buttons for device selection.
        /// <see cref="RequireSpecialButtonForSelection"/> and <see cref="Detection"/> is set to <c>true</c>.
        /// Other values are disabled because the can not be used together with special buttons.
        /// </summary>
        /// <param name="selectionButtonPrimary">Specifies the primary mouse button to be used for device selection.
        /// If not a valid Raylib mouse button,
        /// <c>ShapeMouseButton.NONE</c> is used.</param>
        /// <param name="selectionButtonSecondary">Specifies the secondary mouse button to be used for device selection.
        /// If not a valid Raylib mouse button,
        /// <c>ShapeMouseButton.NONE</c> is used.</param>
        public MouseSettings(ShapeMouseButton selectionButtonPrimary, ShapeMouseButton selectionButtonSecondary)
        {
            Detection = true;
            MoveThreshold = -1f;
            WheelThreshold = -1f;
            MinPressCount = -1;
            MinPressInterval = -1f;
            MinUsedDuration = -1f;
            RequireSpecialButtonForSelection = true;
            SelectionButtonPrimary = ShapeMouseDevice.IsValidRaylibMouseButton(selectionButtonPrimary) ? selectionButtonPrimary : ShapeMouseButton.NONE;
            SelectionButtonSecondary = ShapeMouseDevice.IsValidRaylibMouseButton(selectionButtonSecondary) ? selectionButtonSecondary : ShapeMouseButton.NONE;
        }
        
        /// <summary>
        /// Gets a value indicating whether press count detection is enabled for the mouse.
        /// Detection is enabled when <c>MinPressCount</c> is greater than 1 and <c>MinPressInterval</c> is greater than 0.
        /// </summary>
        public bool PressCountEnabled => MinPressCount > 1 && MinPressInterval > 0f;
        
        /// <summary>
        /// Indicates whether used duration detection is enabled (requires <c>MinUsedDuration</c> to be greater than zero).
        /// </summary>
        public bool UsedDurationEnabled => MinUsedDuration > 0f;
        
        /// <summary>
        /// Indicates whether the mouse move threshold is enabled (greater than 0).
        /// </summary>
        public bool MoveThresholdEnabled => MoveThreshold > 0f;
        
        /// <summary>
        /// Indicates whether the mouse wheel threshold is enabled (greater than 0).
        /// </summary>
        public bool WheelThresholdEnabled => WheelThreshold > 0f;
        
        /// <summary>
        /// Gets a value indicating whether selection button detection is enabled for the mouse.
        /// Returns <c>true</c> if <see cref="RequireSpecialButtonForSelection"/> is enabled and at least one of
        /// <see cref="SelectionButtonPrimary"/> or <see cref="SelectionButtonSecondary"/> is not <c>ShapeMouseButton.NONE</c>.
        /// </summary>
        public bool SpecialButtonSelectionSystemEnabled => RequireSpecialButtonForSelection && (SelectionButtonPrimary != ShapeMouseButton.NONE || SelectionButtonSecondary != ShapeMouseButton.NONE);
        
        /// <summary>
        /// Indicates whether the input device can be changed to mouse.
        /// </summary>
        /// <remarks>
        /// <c>Default is true</c>
        /// </remarks>
        public readonly bool Detection;
        /// <summary>
        /// The minimum movement threshold to consider the mouse as "used".
        /// </summary>
        /// <remarks>
        /// <c>Default is 0.5f.</c>
        /// </remarks>
        public readonly float MoveThreshold;
        /// <summary>
        /// The minimum mouse wheel movement threshold to consider the mouse as "used".
        /// </summary>
        /// <remarks>
        /// <c>Default is 0.25f.</c>
        /// </remarks>
        public readonly float WheelThreshold;

        /// <summary>
        /// The minimum number of mouse button presses required, within the specified interval, 
        /// to consider the mouse device as "used".
        /// </summary>
        /// <remarks>
        /// <c>Default is 2 presses within 1 second.</c>
        /// </remarks>
        public readonly int MinPressCount;
    
        /// <summary>
        /// The time interval (in seconds) from the first mouse button press within which 
        /// <c>MouseMinPressCount</c> must be reached for the mouse device to be considered "used".
        /// </summary>
        /// <remarks>
        /// <c>Default is 1 second.</c>
        /// </remarks>
        public readonly float MinPressInterval;
    
        /// <summary>
        /// The minimum duration (in seconds) of consecutive mouse use (movement or holding at least one button down)
        /// required to consider the mouse device as "used".
        /// </summary>
        /// <remarks>
        /// <c>Default is 0.5 seconds</c>
        /// </remarks>
        public readonly float MinUsedDuration;
        
        /// <summary>
        /// When enabled, and at least one selection button is set to a value other than <c>ShapeMouseButton.NONE</c>,
        /// the input device can only be selected if any of the specified special buttons is pressed.
        /// </summary>
        /// <remarks>
        /// <c>Default is true.</c>
        /// </remarks>
        public readonly bool RequireSpecialButtonForSelection;
        
        /// <summary>
        /// The primary mouse button used for device selection (e.g., Left Mouse Button).
        /// </summary>
        /// <remarks>
        /// <see cref="RequireSpecialButtonForSelection"/> needs to be <c>true</c> for this button to be considered.
        /// <c>Default is ShapeMouseButton.NONE.</c>
        /// </remarks>
        public readonly ShapeMouseButton SelectionButtonPrimary;
        
        /// <summary>
        /// The secondary keyboard button used for device selection (e.g., Middle Mouse Button).
        /// </summary>
        /// <remarks>
        /// <see cref="RequireSpecialButtonForSelection"/> needs to be <c>true</c> for this button to be considered.
        /// <c>Default is ShapeMouseButton.NONE.</c>
        /// </remarks>
        public readonly ShapeMouseButton SelectionButtonSecondary;
    }
   
    /// <summary>
    /// Represents the settings used for detecting keyboard input device usage,
    /// including thresholds and detection options.
    /// </summary>
    public readonly struct KeyboardSettings
    {
        /// <summary>
        /// Gets the default <see cref="KeyboardSettings"/> instance with preset values.
        /// </summary>
        /// <code>
        /// Detection = true;
        /// MinPressCount = 2;
        /// MinPressInterval = 1f;
        /// MinUsedDuration = 0.5f;
        /// RequireSpecialButtonForSelection = false;
        /// SelectionButtonPrimary = ShapeKeyboardButton.NONE;
        /// SelectionButtonSecondary = ShapeKeyboardButton.NONE;
        /// SelectionButtonTertiary = ShapeKeyboardButton.NONE;
        /// </code>
        public static readonly KeyboardSettings Default = new();
       
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardSettings"/> struct with default values.
        /// </summary>
        /// <code>
        /// Detection = true;
        /// MinPressCount = 2;
        /// MinPressInterval = 1f;
        /// MinUsedDuration = 0.5f;
        /// RequireSpecialButtonForSelection = false;
        /// SelectionButtonPrimary = ShapeKeyboardButton.NONE;
        /// SelectionButtonSecondary = ShapeKeyboardButton.NONE;
        /// SelectionButtonTertiary = ShapeKeyboardButton.NONE;
        /// </code>
        public KeyboardSettings()
        {
            Detection = true;
            MinPressCount = 2;
            MinPressInterval = 1f;
            MinUsedDuration = 0.5f;

            RequireSpecialButtonForSelection = false;
            SelectionButtonPrimary = ShapeKeyboardButton.NONE;
            SelectionButtonSecondary = ShapeKeyboardButton.NONE;
            SelectionButtonTertiary = ShapeKeyboardButton.NONE;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardSettings"/> struct with custom detection, minimum press count,
        /// minimum press interval, and minimum used duration values. Other values are set to their defaults.
        /// </summary>
        /// <param name="detection">Indicates whether keyboard detection is enabled.</param>
        /// <param name="minPressCount">The minimum number of keyboard button presses required within the specified interval.</param>
        /// <param name="minPressInterval">The time interval (in seconds) from the first keyboard button press within which <paramref name="minPressCount"/> must be reached.</param>
        /// <param name="minUsedDuration">The minimum duration (in seconds) of consecutive keyboard use required to consider the keyboard device as "used".</param>
        public KeyboardSettings(bool detection, int minPressCount = 2, float minPressInterval = 1f, float minUsedDuration = 0.5f)
        {
            Detection = detection;
            MinPressCount = minPressCount;
            MinPressInterval = minPressInterval;
            MinUsedDuration = minUsedDuration;
            RequireSpecialButtonForSelection = false;
            SelectionButtonPrimary = ShapeKeyboardButton.NONE;
            SelectionButtonSecondary = ShapeKeyboardButton.NONE;
            SelectionButtonTertiary = ShapeKeyboardButton.NONE;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardSettings"/> struct with custom values,
        /// including detection, minimum press count, minimum press interval, minimum used duration,
        /// and custom selection buttons for device selection.
        /// <see cref="RequireSpecialButtonForSelection"/> and <see cref="Detection"/> is set to <c>true</c>.
        /// Other values are disabled because they do not work with special button selection.
        /// </summary>
        /// <param name="selectionButtonPrimary">The primary keyboard button used for device selection.</param>
        /// <param name="selectionButtonSecondary">The secondary keyboard button used for device selection.</param>
        /// <param name="selectionButtonTertiary">The tertiary keyboard button used for device selection.</param>
        public KeyboardSettings(ShapeKeyboardButton selectionButtonPrimary, ShapeKeyboardButton selectionButtonSecondary, ShapeKeyboardButton selectionButtonTertiary)
        {
            Detection = true;
            MinPressCount = -1;
            MinPressInterval = -1f;
            MinUsedDuration = -1f;
            RequireSpecialButtonForSelection = true;
            SelectionButtonPrimary = selectionButtonPrimary;
            SelectionButtonSecondary = selectionButtonSecondary;
            SelectionButtonTertiary = selectionButtonTertiary;
        }
        
        /// <summary>
        /// Gets a value indicating whether press count detection is enabled for the keyboard.
        /// Detection is enabled when <c>MinPressCount</c> is greater than 1 and <c>MinPressInterval</c> is greater than 0.
        /// </summary>
        public bool PressCountEnabled => MinPressCount > 1 && MinPressInterval > 0f;
        
        /// <summary>
        /// Indicates whether used duration detection is enabled (requires <c>MinUsedDuration</c> to be greater than zero).
        /// </summary>
        public bool UsedDurationEnabled => MinUsedDuration > 0f;
        
        /// <summary>
        /// Gets a value indicating whether special button detection is enabled for the keyboard.
        /// Returns <c>true</c> if <see cref="RequireSpecialButtonForSelection"/> is enabled and at least one of
        /// <see cref="SelectionButtonPrimary"/>, <see cref="SelectionButtonSecondary"/>, or <see cref="SelectionButtonTertiary"/> is not <c>ShapeKeyboardButton.NONE</c>.
        /// </summary>
        public bool SpecialButtonSelectionSystemEnabled => 
            RequireSpecialButtonForSelection && 
            (SelectionButtonPrimary != ShapeKeyboardButton.NONE || 
             SelectionButtonSecondary != ShapeKeyboardButton.NONE || 
             SelectionButtonTertiary != ShapeKeyboardButton.NONE);
        
        /// <summary>
        /// Indicates whether the input device can be changed to keyboard.
        /// </summary>
        /// <remarks>
        /// <c>Default is true</c>
        /// </remarks>
        public readonly bool Detection;

        /// <summary>
        /// The minimum number of keyboard button presses required, within the specified interval, 
        /// to consider the keyboard device as "used".
        /// </summary>
        /// <remarks>
        /// <c>Default is 2 presses within 1 second.</c>
        /// </remarks>
        public readonly int MinPressCount;
    
        /// <summary>
        /// The time interval (in seconds) from the first keyboard button press within which 
        /// <c>MinPressCount</c> must be reached for the keyboard device to be considered "used".
        /// </summary>
        /// <remarks>
        /// <c>Default is 1 second.</c>
        /// </remarks>
        public readonly float MinPressInterval;
    
        /// <summary>
        /// The minimum duration (in seconds) of consecutive keyboard use (movement or holding at least one button down)
        /// required to consider the keyboard device as "used".
        /// </summary>
        /// <remarks>
        /// <c>Default is 0.5f seconds.</c>
        /// </remarks>
        public readonly float MinUsedDuration;

        /// <summary>
        /// When enabled, and at least one selection button is set to a value other than <c>ShapeKeyboardButton.NONE</c>,
        /// the input device can only be selected if any of the specified special buttons is pressed.
        /// </summary>
        /// <remarks>
        /// <c>Default is true.</c>
        /// </remarks>
        public readonly bool RequireSpecialButtonForSelection;
        
        /// <summary>
        /// The primary keyboard button used for device selection (e.g., ESCAPE).
        /// </summary>
        /// <remarks>
        /// <see cref="RequireSpecialButtonForSelection"/> needs to be <c>true</c> for this button to be considered.
        /// <c>Default is ShapeKeyboardButton.ESCAPE.</c>
        /// </remarks>
        public readonly ShapeKeyboardButton SelectionButtonPrimary;
        
        /// <summary>
        /// The secondary keyboard button used for device selection (e.g., ENTER).
        /// </summary>
        /// <remarks>
        /// <see cref="RequireSpecialButtonForSelection"/> needs to be <c>true</c> for this button to be considered.
        /// <c>Default is ShapeKeyboardButton.ENTER.</c>
        /// </remarks>
        public readonly ShapeKeyboardButton SelectionButtonSecondary;
        
        /// <summary>
        /// The tertiary keyboard button used for device selection (e.g., SPACE).
        /// </summary>
        /// <remarks>
        /// <see cref="RequireSpecialButtonForSelection"/> needs to be <c>true</c> for this button to be considered.
        /// <c>Default is ShapeKeyboardButton.SPACE.</c>
        /// </remarks>
        public readonly ShapeKeyboardButton SelectionButtonTertiary;

    }
    
    public readonly struct GamepadSettings
    {
        public static readonly GamepadSettings Default = new();
    }
    
    
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