namespace ShapeEngine.Input;

public partial class InputDeviceUsageDetectionSettings
{
    /// <summary>
    /// Represents the settings used for detecting mouse input device usage,
    /// including thresholds and detection options.
    /// </summary>
    public class MouseSettings
    {
        /// <summary>
        /// The default minimum movement threshold to consider the mouse as "used".
        /// </summary>
        public const float DefaultMouseMoveThreshold = 25f;
    
        /// <summary>
        /// The default minimum mouse wheel movement threshold to consider the mouse as "used".
        /// </summary>
        public const float DefaultMouseWheelThreshold = 3f;
        
        /// <summary>
        /// Gets the default <see cref="ShapeEngine.Input.InputDeviceUsageDetectionSettings.MouseSettings"/> instance with preset values.
        /// </summary>
        /// <code>
        /// Detection = true;
        /// MoveThreshold = 25f;
        /// WheelThreshold = 3f;
        /// MinPressCount = 2;
        /// MinPressInterval = 1f;
        /// MinUsedDuration = 0.5f;
        /// SelectionButtons = null;
        /// ExceptionButtons = null;
        /// SelectionCooldownDuration = 2f;
        /// </code>
        public static readonly MouseSettings Default = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeEngine.Input.InputDeviceUsageDetectionSettings.MouseSettings"/> struct with default values.
        /// </summary>
        /// <code>
        /// Detection = true;
        /// MoveThreshold = 25f;
        /// WheelThreshold = 3f;
        /// MinPressCount = 2;
        /// MinPressInterval = 1f;
        /// MinUsedDuration = 0.5f;
        /// SelectionButtons = null;
        /// ExceptionButtons = null;
        /// SelectionCooldownDuration = 2f;
        /// </code>
        public MouseSettings()
        {
            Detection = true;
            MoveThreshold = DefaultMouseMoveThreshold;
            WheelThreshold = DefaultMouseWheelThreshold;
            MinPressCount = 2;
            MinPressInterval = 1f;
            MinUsedDuration = 0.5f;
            SelectionButtons = [];
            ExceptionButtons = [];
            SelectionCooldownDuration = 2f;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeEngine.Input.InputDeviceUsageDetectionSettings.MouseSettings"/> struct with custom detection, move threshold, and wheel threshold values.
        /// Other values are set to their defaults: MinPressCount = 2, MinPressInterval = 1f, MinUsedDuration = 0.5f.
        /// </summary>
        /// <param name="detection">Indicates whether mouse detection is enabled.</param>
        /// <param name="moveThreshold">The minimum movement threshold to consider the mouse as "used".</param>
        /// <param name="wheelThreshold">The minimum mouse wheel movement threshold to consider the mouse as "used".</param>
        /// <param name="selectionCooldownDuration">
        /// Specifies the cooldown duration (in seconds) after this device is selected
        /// during which no other input device can be selected.
        /// <c>Default is 2 seconds.</c>
        /// </param>
        /// <param name="exceptionButtons">
        /// The set of mouse buttons that will never select this device, even if pressed.
        /// </param>
        public MouseSettings(bool detection, float moveThreshold, float wheelThreshold, float selectionCooldownDuration = 2f, HashSet<ShapeMouseButton>? exceptionButtons = null)
        {
            Detection = detection;
            MoveThreshold = moveThreshold;
            WheelThreshold = wheelThreshold;
            MinPressCount = 2;
            MinPressInterval = 1f;
            MinUsedDuration = 0.5f;
            SelectionButtons = [];
            ExceptionButtons = exceptionButtons ?? [];
            SelectionCooldownDuration = selectionCooldownDuration;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeEngine.Input.InputDeviceUsageDetectionSettings.MouseSettings"/> struct with custom values.
        /// <see cref="Detection"/> is set to <c>true</c>.
        /// </summary>
        /// <param name="moveThreshold">The minimum movement threshold to consider the mouse as "used".</param>
        /// <param name="wheelThreshold">The minimum mouse wheel movement threshold to consider the mouse as "used".</param>
        /// <param name="minPressCount">The minimum number of mouse button presses required within the specified interval.</param>
        /// <param name="minPressInterval">The time interval (in seconds) from the first mouse button press within which <paramref name="minPressCount"/> must be reached.</param>
        /// <param name="minUsedDuration">The minimum duration (in seconds) of consecutive mouse use required to consider the mouse device as "used".</param>
        /// <param name="selectionCooldownDuration">
        /// Specifies the cooldown duration (in seconds) after this device is selected
        /// during which no other input device can be selected.
        /// <c>Default is 2 seconds.</c>
        /// </param>
        /// <param name="exceptionButtons">
        /// The set of mouse buttons that will never select this device, even if pressed.
        /// </param>
        public MouseSettings(float moveThreshold, float wheelThreshold, int minPressCount, float minPressInterval, float minUsedDuration,
            float selectionCooldownDuration = 2f, HashSet<ShapeMouseButton>? exceptionButtons = null)
        {
            Detection = true;
            MoveThreshold = moveThreshold;
            WheelThreshold = wheelThreshold;
            MinPressCount = minPressCount;
            MinPressInterval = minPressInterval;
            MinUsedDuration = minUsedDuration;
            SelectionButtons = [];
            ExceptionButtons = exceptionButtons ?? [];
            SelectionCooldownDuration = selectionCooldownDuration;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeEngine.Input.InputDeviceUsageDetectionSettings.MouseSettings"/> class
        /// with custom move and wheel thresholds, press count, and press interval.
        /// <see cref="Detection"/> is set to <c>true</c>.
        /// <see cref="MinUsedDuration"/> is disabled (set to -1).
        /// </summary>
        /// <param name="moveThreshold">The minimum movement threshold to consider the mouse as "used".</param>
        /// <param name="wheelThreshold">The minimum mouse wheel movement threshold to consider the mouse as "used".</param>
        /// <param name="minPressCount">The minimum number of mouse button presses required within the specified interval.</param>
        /// <param name="minPressInterval">The time interval (in seconds) from the first mouse button press within which <paramref name="minPressCount"/> must be reached.</param>
        /// <param name="selectionCooldownDuration">
        /// Specifies the cooldown duration (in seconds) after this device is selected
        /// during which no other input device can be selected. Default is 2 seconds.
        /// </param>
        /// <param name="exceptionButtons">
        /// The set of mouse buttons that will never select this device, even if pressed.
        /// </param>
        public MouseSettings(float moveThreshold, float wheelThreshold, int minPressCount, float minPressInterval, float selectionCooldownDuration = 2f, HashSet<ShapeMouseButton>? exceptionButtons = null)
        {
            Detection = true;
            MoveThreshold = moveThreshold;
            WheelThreshold = wheelThreshold;
            MinPressCount = minPressCount;
            MinPressInterval = minPressInterval;
            MinUsedDuration = -1;
            SelectionButtons = [];
            ExceptionButtons = exceptionButtons ?? [];
            SelectionCooldownDuration = selectionCooldownDuration;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeEngine.Input.InputDeviceUsageDetectionSettings.MouseSettings"/> class
        /// with custom move and wheel thresholds and used duration.
        /// <see cref="Detection"/> is set to <c>true</c>.
        /// <see cref="MinPressCount"/> and <see cref="MinPressInterval"/> are disabled (set to -1).
        /// </summary>
        /// <param name="moveThreshold">The minimum movement threshold to consider the mouse as "used".</param>
        /// <param name="wheelThreshold">The minimum mouse wheel movement threshold to consider the mouse as "used".</param>
        /// <param name="minUsedDuration">The minimum duration (in seconds) of consecutive mouse use required to consider the mouse device as "used".</param>
        /// <param name="selectionCooldownDuration">
        /// Specifies the cooldown duration (in seconds) after this device is selected
        /// during which no other input device can be selected. Default is 2 seconds.
        /// </param>
        /// <param name="exceptionButtons">
        /// The set of mouse buttons that will never select this device, even if pressed.
        /// </param>
        public MouseSettings(float moveThreshold, float wheelThreshold,  float minUsedDuration, float selectionCooldownDuration = 2f, HashSet<ShapeMouseButton>? exceptionButtons = null)
        {
            Detection = true;
            MoveThreshold = moveThreshold;
            WheelThreshold = wheelThreshold;
            MinPressCount = -1;
            MinPressInterval = -1f;
            MinUsedDuration = minUsedDuration;
            SelectionButtons = [];
            ExceptionButtons = exceptionButtons ?? [];
            SelectionCooldownDuration = selectionCooldownDuration;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeEngine.Input.InputDeviceUsageDetectionSettings.MouseSettings"/> class
        /// with custom used duration. Move and wheel thresholds are set to defaults (25f, 3f).
        /// <see cref="Detection"/> is set to <c>true</c>.
        /// <see cref="MinPressCount"/> and <see cref="MinPressInterval"/> are disabled (set to -1).
        /// </summary>
        /// <param name="minUsedDuration">The minimum duration (in seconds) of consecutive mouse use required to consider the mouse device as "used".</param>
        /// <param name="selectionCooldownDuration">
        /// Specifies the cooldown duration (in seconds) after this device is selected
        /// during which no other input device can be selected. Default is 2 seconds.
        /// </param>
        /// <param name="exceptionButtons">
        /// The set of mouse buttons that will never select this device, even if pressed.
        /// </param>
        public MouseSettings(float minUsedDuration, float selectionCooldownDuration = 2f, HashSet<ShapeMouseButton>? exceptionButtons = null)
        {
            Detection = true;
            MoveThreshold = DefaultMouseMoveThreshold;
            WheelThreshold = DefaultMouseWheelThreshold;
            MinPressCount = -1;
            MinPressInterval = -1f;
            MinUsedDuration = minUsedDuration;
            SelectionButtons = [];
            ExceptionButtons = exceptionButtons ?? [];
            SelectionCooldownDuration = selectionCooldownDuration;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeEngine.Input.InputDeviceUsageDetectionSettings.MouseSettings"/> class
        /// with custom press count and interval. Move and wheel thresholds are set to defaults (25f, 3f).
        /// <see cref="Detection"/> is set to <c>true</c>.
        /// <see cref="MinUsedDuration"/> is disabled (set to -1).
        /// </summary>
        /// <param name="minPressCount">The minimum number of mouse button presses required within the specified interval.</param>
        /// <param name="minPressInterval">The time interval (in seconds) from the first mouse button press within which <paramref name="minPressCount"/> must be reached.</param>
        /// <param name="selectionCooldownDuration">
        /// Specifies the cooldown duration (in seconds) after this device is selected
        /// during which no other input device can be selected. Default is 2 seconds.
        /// </param>
        /// <param name="exceptionButtons">
        /// The set of mouse buttons that will never select this device, even if pressed.
        /// </param>
        public MouseSettings(int minPressCount, float minPressInterval, float selectionCooldownDuration = 2f, HashSet<ShapeMouseButton>? exceptionButtons = null)
        {
            Detection = true;
            MoveThreshold = DefaultMouseMoveThreshold;
            WheelThreshold = DefaultMouseWheelThreshold;
            MinPressCount = minPressCount;
            MinPressInterval = minPressInterval;
            MinUsedDuration = -1f;
            SelectionButtons = [];
            ExceptionButtons = exceptionButtons ?? [];
            SelectionCooldownDuration = selectionCooldownDuration;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeEngine.Input.InputDeviceUsageDetectionSettings.MouseSettings"/> class
        /// with a set of selection buttons. Move and wheel thresholds are set to 0.5f and 0.25f.
        /// All other detection options are disabled.
        /// <see cref="Detection"/> is set to <c>true</c>.
        /// </summary>
        /// <param name="selectionButtons">
        /// The set of mouse buttons that can be used to select this device. When set and not empty, all other means of selection are disabled.
        /// </param>
        /// <param name="selectionCooldownDuration">
        /// Specifies the cooldown duration (in seconds) after this device is selected
        /// during which no other input device can be selected. Default is 2 seconds.
        /// </param>
        /// <param name="exceptionButtons">
        /// The set of mouse buttons that will never select this device, even if pressed.
        /// </param>
        public MouseSettings(HashSet<ShapeMouseButton> selectionButtons, float selectionCooldownDuration = 2f, HashSet<ShapeMouseButton>? exceptionButtons = null)
        {
            Detection = true;
            MoveThreshold = DefaultMouseMoveThreshold;
            WheelThreshold = DefaultMouseWheelThreshold;
            MinPressCount = -1;
            MinPressInterval = -1f;
            MinUsedDuration = -1f;
            SelectionButtons = selectionButtons;
            ExceptionButtons = exceptionButtons ?? [];
            SelectionCooldownDuration = selectionCooldownDuration;
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
        /// Indicates whether the input device can be changed to mouse.
        /// </summary>
        /// <remarks>
        /// <c>Default is true</c>
        /// Does not affect <see cref="MouseDevice.WasUsedRaw"/> values. It does affect the automatic system in <see cref="ShapeInput"/> to switch between devices.
        /// </remarks>
        public bool Detection;

        /// <summary>
        /// The minimum movement threshold to consider the mouse as "used".
        /// </summary>
        /// <remarks>
        /// <c>Default is 0.5f.</c>
        /// Even if <see cref="Detection"/> is set to <c>false</c>, <see cref="MoveThreshold"/> will still be used for <see cref="MouseDevice.WasUsedRaw"/> system.
        /// </remarks>
        public float MoveThreshold;

        /// <summary>
        /// The minimum mouse wheel movement threshold to consider the mouse as "used".
        /// </summary>
        /// <remarks>
        /// <c>Default is 0.25f.</c>
        /// Even if <see cref="Detection"/> is set to <c>false</c>, <see cref="WheelThreshold"/> will still be used for <see cref="MouseDevice.WasUsedRaw"/> system.
        /// </remarks>
        public float WheelThreshold;

        /// <summary>
        /// The minimum number of mouse button presses required, within the specified interval, 
        /// to consider the mouse device as "used".
        /// </summary>
        /// <remarks>
        /// <c>Default is 2 presses within 1 second.</c>
        /// Does work in combination with <see cref="MinUsedDuration"/>  but has a lower priority.
        /// <see cref="MinUsedDuration"/> system will be checked first and if it triggers,
        /// <see cref="MinPressCount"/> / <see cref="MinPressInterval"/> system will be reset as well and not checked in the same frame.
        /// </remarks>
        public int MinPressCount;

        /// <summary>
        /// The time interval (in seconds) from the first mouse button press within which 
        /// <c>MouseMinPressCount</c> must be reached for the mouse device to be considered "used".
        /// </summary>
        /// <remarks>
        /// <c>Default is 1 second.</c>
        /// Does work in combination with <see cref="MinUsedDuration"/>  but has a lower priority.
        /// <see cref="MinUsedDuration"/> system will be checked first and if it triggers,
        /// <see cref="MinPressCount"/> / <see cref="MinPressInterval"/> system will be reset as well and not checked in the same frame.
        /// </remarks>
        public float MinPressInterval;

        /// <summary>
        /// The minimum duration (in seconds) of consecutive mouse use (movement or holding at least one button down)
        /// required to consider the mouse device as "used".
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
        public readonly HashSet<ShapeMouseButton> SelectionButtons;
        
        /// <summary>
        /// The set of gamepad buttons that will never select this device, even if pressed.
        /// </summary>
        public readonly HashSet<ShapeMouseButton> ExceptionButtons;

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