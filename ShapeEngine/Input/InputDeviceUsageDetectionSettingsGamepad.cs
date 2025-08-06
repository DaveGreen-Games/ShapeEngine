namespace ShapeEngine.Input;

public partial class InputDeviceUsageDetectionSettings
{
    /// <summary>
    /// Represents the settings used for detecting gamepad input device usage,
    /// including thresholds and detection options.
    /// </summary>
    public class GamepadSettings
    {
        /// <summary>
        /// The default axis threshold value for detecting analog stick movement.
        /// </summary>
        public const float DefaultJoyAxisThreshold = 0.15f;
        
        /// <summary>
        /// The default trigger threshold value for detecting trigger input.
        /// </summary>
        public const float DefaultTriggerAxisThreshold = 0.15f;
        
        /// <summary>
        /// The default threshold for all axis inputs, including triggers.
        /// Use this value when only one threshold can be supplied for both joystick and trigger axes.
        /// </summary>
        public const float DefaultAxisThreshold = 0.15f;
        
        
        /// <summary>
        /// Gets the default <see cref="MouseSettings"/> instance with preset values.
        /// </summary>
        /// <code>
        /// Detection = true;
        /// AxisThreshold = DefaultJoyAxisThreshold;
        /// TriggerThreshold = DefaultTriggerAxisThreshold;
        /// MinPressCount = 2;
        /// MinPressInterval = 1f;
        /// MinUsedDuration = 0.5f;
        /// SelectionButtons = null;
        /// ExceptionButtons = null;
        /// SelectionCooldownDuration = 2f;
        /// </code>
        public static readonly GamepadSettings Default = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeEngine.Input.InputDeviceUsageDetectionSettings.GamepadSettings"/> struct with default values.
        /// </summary>
        /// <code>
        /// Detection = true;
        /// AxisThreshold = DefaultJoyAxisThreshold;
        /// TriggerThreshold = DefaultTriggerAxisThreshold;
        /// MinPressCount = 2;
        /// MinPressInterval = 1f;
        /// MinUsedDuration = 0.5f;
        /// SelectionButtons = null;
        /// ExceptionButtons = null;
        /// SelectionCooldownDuration = 2f;
        /// </code>
        public GamepadSettings()
        {
            Detection = true;
            AxisThreshold = DefaultJoyAxisThreshold;
            TriggerThreshold = DefaultTriggerAxisThreshold;
            MinPressCount = 2;
            MinPressInterval = 1f;
            MinUsedDuration = 0.5f;
            SelectionButtons = [];
            ExceptionButtons = [];
            SelectionCooldownDuration = 2f;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeEngine.Input.InputDeviceUsageDetectionSettings.GamepadSettings"/> struct with custom detection, axis, and trigger threshold values.
        /// Other values are set to their defaults: MinPressCount = 2, MinPressInterval = 1f, MinUsedDuration = 0.5f.
        /// </summary>
        /// <param name="detection">Indicates whether gamepad detection is enabled.</param>
        /// <param name="axisThreshold">The minimum movement threshold for the analog sticks to consider the gamepad as "used".</param>
        /// <param name="triggerThreshold">The minimum threshold for the triggers to consider the gamepad as "used".</param>
        /// <param name="selectionCooldownDuration">
        /// Specifies the cooldown duration (in seconds) after this device is selected
        /// during which no other input device can be selected.
        /// <c>Default is 2 seconds.</c>
        /// </param>
        /// <param name="exceptionButtons">A set of gamepad buttons that will never select this device, even if pressed.
        /// Optional.</param>
        public GamepadSettings(bool detection, float axisThreshold, float triggerThreshold, float selectionCooldownDuration = 2f, HashSet<ShapeGamepadButton>? exceptionButtons = null)
        {
            Detection = detection;
            AxisThreshold = axisThreshold;
            TriggerThreshold = triggerThreshold;
            MinPressCount = 2;
            MinPressInterval = 1f;
            MinUsedDuration = 0.5f;
            SelectionButtons = [];
            ExceptionButtons = exceptionButtons ?? [];
            SelectionCooldownDuration = selectionCooldownDuration;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeEngine.Input.InputDeviceUsageDetectionSettings.GamepadSettings"/> struct with custom axis and trigger thresholds,
        /// minimum press count, minimum press interval, minimum used duration, and selection cooldown duration.
        /// <see cref="Detection"/> is set to <c>true</c>.
        /// </summary>
        /// <param name="axisThreshold">The minimum movement threshold for the analog sticks to consider the gamepad as "used".</param>
        /// <param name="triggerThreshold">The minimum threshold for the triggers to consider the gamepad as "used".</param>
        /// <param name="minPressCount">The minimum number of gamepad button presses required within the specified interval.</param>
        /// <param name="minPressInterval">The time interval (in seconds) from the first gamepad button press within which <paramref name="minPressCount"/> must be reached.</param>
        /// <param name="minUsedDuration">The minimum duration (in seconds) of consecutive gamepad use required to consider the gamepad device as "used".</param>
        /// <param name="selectionCooldownDuration">
        /// Specifies the cooldown duration (in seconds) after this device is selected
        /// during which no other input device can be selected.
        /// <c>Default is 2 seconds.</c>
        /// </param>
        /// <param name="exceptionButtons">A set of gamepad buttons that will never select this device, even if pressed.
        /// Optional.</param>
        public GamepadSettings(float axisThreshold, float triggerThreshold, int minPressCount,
            float minPressInterval, float minUsedDuration, float selectionCooldownDuration = 2f, HashSet<ShapeGamepadButton>? exceptionButtons = null)
        {
            Detection = true;
            AxisThreshold = axisThreshold;
            TriggerThreshold = triggerThreshold;
            MinPressCount = minPressCount;
            MinPressInterval = minPressInterval;
            MinUsedDuration = minUsedDuration;
            SelectionButtons = [];
            ExceptionButtons = exceptionButtons ?? [];;
            SelectionCooldownDuration = selectionCooldownDuration;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeEngine.Input.InputDeviceUsageDetectionSettings.GamepadSettings"/> class
        /// with custom minimum press count and interval, optional cooldown duration, and optional exception buttons.
        /// <para>
        /// <see cref="Detection"/> is set to <c>true</c>,
        /// <see cref="TriggerThreshold"/> is set to <see cref="DefaultTriggerAxisThreshold"/>, and <see cref="AxisThreshold"/> is set to <see cref="DefaultJoyAxisThreshold"/>,
        /// <see cref="MinUsedDuration"/> is disabled (-1).
        /// </para>
        /// </summary>
        /// <param name="minPressCount">The minimum number of button presses required within the interval to consider the gamepad as "used".</param>
        /// <param name="minPressInterval">The time interval (in seconds) for the button presses.</param>
        /// <param name="selectionCooldownDuration">Cooldown duration (in seconds) after selection. Default is 2 seconds.</param>
        /// <param name="exceptionButtons">A set of gamepad buttons that will never select this device, even if pressed. Optional.</param>
        public GamepadSettings(int minPressCount, float minPressInterval, float selectionCooldownDuration = 2f, HashSet<ShapeGamepadButton>? exceptionButtons = null)
        {
            Detection = true;
            AxisThreshold = DefaultJoyAxisThreshold;
            TriggerThreshold = DefaultTriggerAxisThreshold;
            MinPressCount = minPressCount;
            MinPressInterval = minPressInterval;
            MinUsedDuration = -1f;
            SelectionButtons = [];
            ExceptionButtons = exceptionButtons ?? [];;
            SelectionCooldownDuration = selectionCooldownDuration;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeEngine.Input.InputDeviceUsageDetectionSettings.GamepadSettings"/> class
        /// with custom minimum used duration, optional cooldown duration, and optional exception buttons.
        /// <para>
        /// <see cref="Detection"/> is set to <c>true</c>,
        /// <see cref="TriggerThreshold"/> is set to <see cref="DefaultTriggerAxisThreshold"/>, and <see cref="AxisThreshold"/> is set to <see cref="DefaultJoyAxisThreshold"/>,
        /// <see cref="MinPressCount"/> and <see cref="MinPressInterval"/> are disabled (-1).
        /// </para>
        /// </summary>
        /// <param name="minUsedDuration">The minimum duration (in seconds) of consecutive gamepad use required to consider the gamepad as "used".</param>
        /// <param name="selectionCooldownDuration">Cooldown duration (in seconds) after selection. Default is 2 seconds.</param>
        /// <param name="exceptionButtons">A set of gamepad buttons that will never select this device, even if pressed. Optional.</param>
        public GamepadSettings(float minUsedDuration, float selectionCooldownDuration = 2f, HashSet<ShapeGamepadButton>? exceptionButtons = null)
        {
            Detection = true;
            AxisThreshold = DefaultJoyAxisThreshold;
            TriggerThreshold = DefaultTriggerAxisThreshold;
            MinPressCount = -1;
            MinPressInterval = -1f;
            MinUsedDuration = minUsedDuration;
            SelectionButtons = [];
            ExceptionButtons = exceptionButtons ?? [];;
            SelectionCooldownDuration = selectionCooldownDuration;
        }
        /// <summary>
        /// <para>
        /// Initializes a new instance of the <see cref="ShapeEngine.Input.InputDeviceUsageDetectionSettings.GamepadSettings"/> class
        /// with a set of selection buttons, an optional cooldown duration, and optional exception buttons.
        /// </para>
        /// <para>
        /// This constructor enables selection button mode, where only the specified <paramref name="selectionButtons"/> can be used
        /// to select this device. All other detection methods are disabled by setting <c>MinPressCount</c>, <c>MinPressInterval</c>,
        /// and <c>MinUsedDuration</c> to -1.
        /// </para>
        /// <para>
        /// <see cref="TriggerThreshold"/> is set to <see cref="DefaultTriggerAxisThreshold"/>, and <see cref="AxisThreshold"/> is set to <see cref="DefaultJoyAxisThreshold"/>.
        /// </para>
        /// </summary>
        /// <param name="selectionButtons">
        /// The set of gamepad buttons that can be used to select this device.
        /// All other means of selection are disabled, even if an empty hash set is used.
        /// </param>
        /// <param name="selectionCooldownDuration">
        /// Specifies the cooldown duration (in seconds) after this device is selected during which no other input device can be selected.
        /// Default is 2 seconds. Zero or negative values disable the cooldown.
        /// </param>
        /// <param name="exceptionButtons">
        /// A set of gamepad buttons that will never select this device, even if pressed. Optional.
        /// </param>
        public GamepadSettings(HashSet<ShapeGamepadButton> selectionButtons, float selectionCooldownDuration = 2f, HashSet<ShapeGamepadButton>? exceptionButtons = null)
        {
            Detection = true;
            TriggerThreshold = DefaultJoyAxisThreshold;
            AxisThreshold = DefaultTriggerAxisThreshold;
            MinPressCount = -1;
            MinPressInterval = -1f;
            MinUsedDuration = -1f;
            SelectionButtons = selectionButtons;
            ExceptionButtons = exceptionButtons ?? [];
            SelectionCooldownDuration = selectionCooldownDuration;
        }
        

        /// <summary>
        /// Gets a value indicating whether press count detection is enabled for the gamepad.
        /// Detection is enabled when <c>MinPressCount</c> is greater than 1 and <c>MinPressInterval</c> is greater than 0.
        /// </summary>
        public bool PressCountEnabled => MinPressCount > 1 && MinPressInterval > 0f;

        /// <summary>
        /// Indicates whether used duration detection is enabled (requires <c>MinUsedDuration</c> to be greater than zero).
        /// </summary>
        public bool UsedDurationEnabled => MinUsedDuration > 0f;
        
        /// <summary>
        /// Gets a value indicating whether axis threshold detection is enabled.
        /// Detection is enabled when <c>AxisThreshold</c> is greater than 0.
        /// </summary>
        public bool AxisThresholdEnabled => AxisThreshold > 0f;
        
        /// <summary>
        /// Gets a value indicating whether trigger threshold detection is enabled.
        /// Detection is enabled when <c>TriggerThreshold</c> is greater than 0.
        /// </summary>
        public bool TriggerThresholdEnabled => TriggerThreshold > 0f;
        

        /// <summary>
        /// Indicates whether the input device can be changed to gamepad.
        /// </summary>
        /// <remarks>
        /// <c>Default is true</c>
        /// Does not affect <see cref="GamepadDevice.WasUsedRaw"/> values. It does affect the automatic system in <see cref="ShapeInput"/> to switch between devices.
        /// </remarks>
        public bool Detection;

        /// <summary>
        /// The minimum movement threshold for the left analog stick to consider the gamepad as "used".
        /// This effectively represents a movement detection deadzone for the left joystick.
        /// <c>Value Range 0-1</c>
        /// </summary>
        /// <remarks>
        /// Default is <see cref="DefaultAxisThreshold"/>
        /// Even if <see cref="Detection"/> is set to <c>false</c>, <see cref="AxisThreshold"/> will still be used for <see cref="GamepadDevice.WasUsedRaw"/> system.
        /// </remarks>
        public float AxisThreshold;

        /// <summary>
        /// The minimum threshold for the left trigger to consider the gamepad as "used".
        /// This effectively represents a movement detection deadzone for the left trigger.
        /// <c>Value Range 0-1</c>
        /// </summary>
        /// <remarks>
        /// Default is <see cref="DefaultTriggerAxisThreshold"/>
        /// Even if <see cref="Detection"/> is set to <c>false</c>, <see cref="TriggerThreshold"/> will still be used for <see cref="GamepadDevice.WasUsedRaw"/> system.
        /// </remarks>
        public float TriggerThreshold;
        

        /// <summary>
        /// The minimum number of gamepad button presses required, within the specified interval, 
        /// to consider the gamepad device as "used".
        /// </summary>
        /// <remarks>
        /// <c>Default is 2 presses within 1 second.</c>
        /// Does work in combination with <see cref="MinUsedDuration"/>  but has a lower priority.
        /// <see cref="MinUsedDuration"/> system will be checked first and if it triggers,
        /// <see cref="MinPressCount"/> / <see cref="MinPressInterval"/> system will be reset as well and not checked in the same frame.
        /// </remarks>
        public int MinPressCount;

        /// <summary>
        /// The time interval (in seconds) from the first gamepad button press within which 
        /// <c>MouseMinPressCount</c> must be reached for the gamepad device to be considered "used".
        /// </summary>
        /// <remarks>
        /// <c>Default is 1 second.</c>
        /// Does work in combination with <see cref="MinUsedDuration"/>  but has a lower priority.
        /// <see cref="MinUsedDuration"/> system will be checked first and if it triggers,
        /// <see cref="MinPressCount"/> / <see cref="MinPressInterval"/> system will be reset as well and not checked in the same frame.
        /// </remarks>
        public float MinPressInterval;

        /// <summary>
        /// The minimum duration (in seconds) of consecutive gamepad use (movement or holding at least one button down)
        /// required to consider the gamepad device as "used".
        /// </summary>
        /// <remarks>
        /// <c>Default is 0.5 seconds</c>
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
        public readonly HashSet<ShapeGamepadButton> SelectionButtons;
        
        /// <summary>
        /// The set of gamepad buttons that will never select this device, even if pressed.
        /// </summary>
        public readonly HashSet<ShapeGamepadButton> ExceptionButtons;
        
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