namespace ShapeEngine.Input;

public partial class InputDeviceUsageDetectionSettings
{
    /// <summary>
    /// Represents the settings used for detecting keyboard input device usage,
    /// including thresholds and detection options.
    /// </summary>
    public class KeyboardSettings
    {
        /// <summary>
        /// Gets the default <see cref="ShapeEngine.Input.InputDeviceUsageDetectionSettings.KeyboardSettings"/> instance with preset values.
        /// </summary>
        /// <code>
        /// Detection = true;
        /// MinPressCount = 2;
        /// MinPressInterval = 1f;
        /// MinUsedDuration = 0.5f;
        /// SelectionButtons = null;
        /// ExceptionButtons = null;
        /// SelectionCooldownDuration = 2f;
        /// </code>
        public static readonly KeyboardSettings Default = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeEngine.Input.InputDeviceUsageDetectionSettings.KeyboardSettings"/> struct with default values.
        /// </summary>
        /// <code>
        /// Detection = true;
        /// MinPressCount = 2;
        /// MinPressInterval = 1f;
        /// MinUsedDuration = 0.5f;
        /// SelectionButtons = null;
        /// ExceptionButtons = null;
        /// SelectionCooldownDuration = 2f;
        /// </code>
        public KeyboardSettings()
        {
            Detection = true;
            MinPressCount = 2;
            MinPressInterval = 1f;
            MinUsedDuration = 0.5f;

            SelectionButtons = null;
            ExceptionButtons = null;

            SelectionCooldownDuration = 2f;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeEngine.Input.InputDeviceUsageDetectionSettings.KeyboardSettings"/> struct with custom detection, minimum press count,
        /// minimum press interval, and minimum used duration values. Other values are set to their defaults.
        /// </summary>
        /// <param name="detection">Indicates whether keyboard detection is enabled.</param>
        /// <param name="minPressCount">The minimum number of keyboard button presses required within the specified interval.</param>
        /// <param name="minPressInterval">The time interval (in seconds) from the first keyboard button press within which <paramref name="minPressCount"/> must be reached.</param>
        /// <param name="minUsedDuration">The minimum duration (in seconds) of consecutive keyboard use required to consider the keyboard device as "used".</param>
        /// <param name="selectionCooldownDuration">
        /// Specifies the cooldown duration (in seconds) after this device is selected
        /// during which no other input device can be selected.
        /// <c>Default is 2 seconds.</c>
        /// </param>
        /// <param name="exceptionButtons">
        /// The set of keyboard buttons that will never select this device, even if pressed. Optional.
        /// </param>
        public KeyboardSettings(bool detection, int minPressCount = 2, float minPressInterval = 1f, 
            float minUsedDuration = 0.5f, float selectionCooldownDuration = 2f, HashSet<ShapeKeyboardButton>? exceptionButtons = null)
        {
            Detection = detection;
            MinPressCount = minPressCount;
            MinPressInterval = minPressInterval;
            MinUsedDuration = minUsedDuration;
            SelectionButtons = null;
            ExceptionButtons = exceptionButtons;
            SelectionCooldownDuration = selectionCooldownDuration;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeEngine.Input.InputDeviceUsageDetectionSettings.KeyboardSettings"/> class
        /// with custom minimum press count, press interval, and used duration values. Other values are set to their defaults.
        /// </summary>
        /// <param name="minPressCount">The minimum number of keyboard button presses required within the specified interval.</param>
        /// <param name="minPressInterval">The time interval (in seconds) from the first keyboard button press within which <paramref name="minPressCount"/> must be reached.</param>
        /// <param name="minUsedDuration">The minimum duration (in seconds) of consecutive keyboard use required to consider the keyboard device as "used".</param>
        /// <param name="selectionCooldownDuration">Specifies the cooldown duration (in seconds) after this device is selected during which no other input device can be selected. Default is 2 seconds.</param>
        /// <param name="exceptionButtons">The set of keyboard buttons that will never select this device, even if pressed. Optional.</param>
        public KeyboardSettings(int minPressCount, float minPressInterval, float minUsedDuration, 
           float selectionCooldownDuration = 2f, HashSet<ShapeKeyboardButton>? exceptionButtons = null)
        {
           Detection = true;
           MinPressCount = minPressCount;
           MinPressInterval = minPressInterval;
           MinUsedDuration = minUsedDuration;
           SelectionButtons = null;
           ExceptionButtons = exceptionButtons;
           SelectionCooldownDuration = selectionCooldownDuration;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeEngine.Input.InputDeviceUsageDetectionSettings.KeyboardSettings"/> class
        /// with a custom minimum used duration. Other values are set to their defaults.
        /// </summary>
        /// <param name="minUsedDuration">The minimum duration (in seconds) of consecutive keyboard use required to consider the keyboard device as "used".</param>
        /// <param name="selectionCooldownDuration">Specifies the cooldown duration (in seconds) after this device is selected during which no other input device can be selected. Default is 2 seconds.</param>
        /// <param name="exceptionButtons">The set of keyboard buttons that will never select this device, even if pressed. Optional.</param>
        public KeyboardSettings(float minUsedDuration, float selectionCooldownDuration = 2f, HashSet<ShapeKeyboardButton>? exceptionButtons = null)
        {
           Detection = true;
           MinPressCount = -1;
           MinPressInterval = -1f;
           MinUsedDuration = minUsedDuration;
           SelectionButtons = null;
           ExceptionButtons = exceptionButtons;
           SelectionCooldownDuration = selectionCooldownDuration;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeEngine.Input.InputDeviceUsageDetectionSettings.KeyboardSettings"/> class
        /// with custom minimum press count and press interval. Used duration is disabled. Other values are set to their defaults.
        /// </summary>
        /// <param name="minPressCount">The minimum number of keyboard button presses required within the specified interval.</param>
        /// <param name="minPressInterval">The time interval (in seconds) from the first keyboard button press within which <paramref name="minPressCount"/> must be reached.</param>
        /// <param name="selectionCooldownDuration">Specifies the cooldown duration (in seconds) after this device is selected during which no other input device can be selected. Default is 2 seconds.</param>
        /// <param name="exceptionButtons">The set of keyboard buttons that will never select this device, even if pressed. Optional.</param>
        public KeyboardSettings(int minPressCount, float minPressInterval, float selectionCooldownDuration = 2f, HashSet<ShapeKeyboardButton>? exceptionButtons = null)
        {
           Detection = true;
           MinPressCount = minPressCount;
           MinPressInterval = minPressInterval;
           MinUsedDuration = -1f;
           SelectionButtons = null;
           ExceptionButtons = exceptionButtons;
           SelectionCooldownDuration = selectionCooldownDuration;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeEngine.Input.InputDeviceUsageDetectionSettings.KeyboardSettings"/> class
        /// with custom selection and exception buttons, and an optional selection cooldown duration.
        /// </summary>
        /// <param name="selectionButtons">
        /// The set of keyboard buttons that can be used to select this device. When set and not empty, all other means of selection are disabled.
        /// </param>
        /// <param name="selectionCooldownDuration">
        /// The cooldown duration (in seconds) after this device is selected, during which no other input device can be selected. Default is 2 seconds.
        /// </param>
        /// <param name="exceptionButtons">
        /// The set of keyboard buttons that will never select this device, even if pressed. Optional.
        /// </param>
        public KeyboardSettings(HashSet<ShapeKeyboardButton> selectionButtons, float selectionCooldownDuration = 2f, HashSet<ShapeKeyboardButton>? exceptionButtons = null)
        {
            Detection = true;
            MinPressCount = -1;
            MinPressInterval = -1f;
            MinUsedDuration = -1f;
            SelectionButtons = selectionButtons;
            ExceptionButtons = exceptionButtons;
            SelectionCooldownDuration = selectionCooldownDuration;
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
        /// Indicates whether the input device can be changed to keyboard.
        /// </summary>
        /// <remarks>
        /// <c>Default is true</c>
        /// Does not affect <see cref="KeyboardDevice.WasUsedRaw"/> values. It does affect the automatic system in <see cref="ShapeInput"/> to switch between devices.
        /// </remarks>
        public bool Detection;

        /// <summary>
        /// The minimum number of keyboard button presses required, within the specified interval, 
        /// to consider the keyboard device as "used".
        /// </summary>
        /// <remarks>
        /// <c>Default is 2 presses within 1 second.</c>
        /// Does work in combination with <see cref="MinUsedDuration"/>  but has a lower priority.
        /// <see cref="MinUsedDuration"/> system will be checked first and if it triggers,
        /// <see cref="MinPressCount"/> / <see cref="MinPressInterval"/> system will be reset as well and not checked in the same frame.
        /// </remarks>
        public int MinPressCount;

        /// <summary>
        /// The time interval (in seconds) from the first keyboard button press within which 
        /// <c>MinPressCount</c> must be reached for the keyboard device to be considered "used".
        /// </summary>
        /// <remarks>
        /// <c>Default is 1 second.</c>
        /// Does work in combination with <see cref="MinUsedDuration"/>  but has a lower priority.
        /// <see cref="MinUsedDuration"/> system will be checked first and if it triggers,
        /// <see cref="MinPressCount"/> / <see cref="MinPressInterval"/> system will be reset as well and not checked in the same frame.
        /// </remarks>
        public float MinPressInterval;

        /// <summary>
        /// The minimum duration (in seconds) of consecutive keyboard use (movement or holding at least one button down)
        /// required to consider the keyboard device as "used".
        /// </summary>
        /// <remarks>
        /// <c>Default is 0.5f seconds.</c>
        /// Does work in combination with <see cref="MinPressCount"/> / <see cref="MinPressInterval"/> but has a higher priority.
        /// <see cref="MinUsedDuration"/> system will be checked first and if it triggers,
        /// <see cref="MinPressCount"/> / <see cref="MinPressInterval"/> system will be reset as well and not checked in the same frame.
        /// </remarks>
        public float MinUsedDuration;

        /// <summary>
        /// Gets a value indicating whether selection buttons are enabled for this device.
        /// Returns <c>true</c> if <see cref="SelectionButtons"/> is not null and contains at least one button.
        /// When enabled, the device can only be selected with the specified special buttons.
        /// </summary>
        public bool SelectionButtonsEnabled => SelectionButtons is { Count: > 0 };
        
        /// <summary>
        /// Gets a value indicating whether exception buttons are enabled for this device.
        /// Returns <c>true</c> if <see cref="ExceptionButtons"/> is not null and contains at least one button.
        /// These buttons will never select this device, even if pressed.
        /// </summary>
        public bool ExceptionButtonsEnabled => ExceptionButtons is { Count: > 0 };
        
        /// <summary>
        /// The set of gamepad buttons that can be used to select this device.
        /// When set and not empty, all other means of selection are disabled.
        /// </summary>
        public readonly HashSet<ShapeKeyboardButton>? SelectionButtons;
        
        /// <summary>
        /// The set of gamepad buttons that will never select this device, even if pressed.
        /// </summary>
        public readonly HashSet<ShapeKeyboardButton>? ExceptionButtons;

        /// <summary>
        /// Specifies the duration (in seconds) after this device is selected during which no other input device can be selected.
        /// Used to prevent rapid switching between input devices.
        /// </summary>
        /// <remarks>
        /// <c>Default is 2 seconds</c>.
        /// Zero and negative values disable the cooldown.
        /// </remarks>
        public float SelectionCooldownDuration;
    }
}