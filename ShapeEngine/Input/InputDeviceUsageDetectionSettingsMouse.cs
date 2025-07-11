namespace ShapeEngine.Input;

public readonly partial struct InputDeviceUsageDetectionSettings
{
    /// <summary>
    /// Represents the settings used for detecting mouse input device usage,
    /// including thresholds and detection options.
    /// </summary>
    public readonly struct MouseSettings
    {
        /// <summary>
        /// Gets the default <see cref="ShapeEngine.Input.InputDeviceUsageDetectionSettings.MouseSettings"/> instance with preset values.
        /// </summary>
        /// <code>
        /// Detection = true;
        /// MoveThreshold = 0.5f;
        /// WheelThreshold = 0.25f;
        /// MinPressCount = 2;
        /// MinPressInterval = 1f;
        /// MinUsedDuration = 0.5f;
        /// RequireSpecialButtonForSelection = false;
        /// SelectionButtonPrimary = ShapeMouseButton.LEFT;
        /// SelectionButtonSecondary = ShapeMouseButton.RIGHT;
        /// SelectionCooldownDuration = 2f;
        /// </code>
        public static readonly MouseSettings Default = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeEngine.Input.InputDeviceUsageDetectionSettings.MouseSettings"/> struct with default values.
        /// </summary>
        /// <code>
        /// Detection = true;
        /// MoveThreshold = 0.5f;
        /// WheelThreshold = 0.25f;
        /// MinPressCount = 2;
        /// MinPressInterval = 1f;
        /// MinUsedDuration = 0.5f;
        /// RequireSpecialButtonForSelection = false;
        /// SelectionButtonPrimary = ShapeMouseButton.LEFT;
        /// SelectionButtonSecondary = ShapeMouseButton.RIGHT;
        /// SelectionCooldownDuration = 2f;
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
            SelectionButtonPrimary = ShapeMouseButton.LEFT;
            SelectionButtonSecondary = ShapeMouseButton.RIGHT;
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
        public MouseSettings(bool detection, float moveThreshold, float wheelThreshold, float selectionCooldownDuration = 2f)
        {
            Detection = detection;
            MoveThreshold = moveThreshold;
            WheelThreshold = wheelThreshold;
            MinPressCount = 2;
            MinPressInterval = 1f;
            MinUsedDuration = 0.5f;
            RequireSpecialButtonForSelection = false;
            SelectionButtonPrimary = ShapeMouseButton.LEFT;
            SelectionButtonSecondary = ShapeMouseButton.RIGHT;
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
        public MouseSettings(float moveThreshold, float wheelThreshold, int minPressCount, float minPressInterval, float minUsedDuration,
            float selectionCooldownDuration = 2f)
        {
            Detection = true;
            MoveThreshold = moveThreshold;
            WheelThreshold = wheelThreshold;
            MinPressCount = minPressCount;
            MinPressInterval = minPressInterval;
            MinUsedDuration = minUsedDuration;
            RequireSpecialButtonForSelection = false;
            SelectionButtonPrimary = ShapeMouseButton.LEFT;
            SelectionButtonSecondary = ShapeMouseButton.RIGHT;
            SelectionCooldownDuration = selectionCooldownDuration;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeEngine.Input.InputDeviceUsageDetectionSettings.MouseSettings"/> struct with custom values,
        /// including detection, move threshold, wheel threshold, minimum press count, minimum press interval,
        /// minimum used duration, and custom selection buttons for device selection.
        /// <see cref="RequireSpecialButtonForSelection"/> and <see cref="Detection"/> is set to <c>true</c>.
        /// Other values are disabled because the can not be used together with special buttons.
        /// <see cref="MoveThreshold"/> and <see cref="WheelThreshold"/> are set to default for the <see cref="MouseDevice.WasUsedRaw"/> system.
        /// </summary>
        /// <param name="selectionButtonPrimary">Specifies the primary mouse button to be used for device selection.</param>
        /// <param name="selectionButtonSecondary">Specifies the secondary mouse button to be used for device selection.</param>
        /// <param name="selectionCooldownDuration">
        /// Specifies the cooldown duration (in seconds) after this device is selected
        /// during which no other input device can be selected.
        /// <c>Default is 2 seconds.</c>
        /// </param>
        public MouseSettings(ShapeMouseButton selectionButtonPrimary, ShapeMouseButton selectionButtonSecondary, float selectionCooldownDuration = 2f)
        {
            Detection = true;
            MoveThreshold = 0.5f;
            WheelThreshold = 0.25f;
            MinPressCount = -1;
            MinPressInterval = -1f;
            MinUsedDuration = -1f;
            RequireSpecialButtonForSelection = true;
            SelectionButtonPrimary = selectionButtonPrimary;
            SelectionButtonSecondary = selectionButtonSecondary;
            SelectionCooldownDuration = selectionCooldownDuration;
        }

        private MouseSettings(
            bool detection, float moveThreshold, float wheelThreshold,
            int minPressCount, float minPressInterval, float minUsedDuration,
            bool requireSpecialButtonForSelection, ShapeMouseButton selectionButtonPrimary, ShapeMouseButton selectionButtonSecondary,
            float selectionCooldownDuration)
        {
            Detection = detection;
            MoveThreshold = moveThreshold;
            WheelThreshold = wheelThreshold;
            MinPressCount = minPressCount;
            MinPressInterval = minPressInterval;
            MinUsedDuration = minUsedDuration;
            RequireSpecialButtonForSelection = requireSpecialButtonForSelection;
            SelectionButtonPrimary = selectionButtonPrimary;
            SelectionButtonSecondary = selectionButtonSecondary;
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
        /// Gets a value indicating whether selection button detection is enabled for the mouse.
        /// Returns <c>true</c> if <see cref="RequireSpecialButtonForSelection"/> is enabled.
        /// </summary>
        public bool SpecialButtonSelectionSystemEnabled => RequireSpecialButtonForSelection;

        /// <summary>
        /// Returns a new MouseSettings with the specified MoveThreshold, keeping all other values the same.
        /// </summary>
        public MouseSettings SetMoveThreshold(float moveThreshold)
        {
            return new MouseSettings(
                Detection,
                moveThreshold,
                WheelThreshold,
                MinPressCount,
                MinPressInterval,
                MinUsedDuration,
                RequireSpecialButtonForSelection,
                SelectionButtonPrimary,
                SelectionButtonSecondary,
                SelectionCooldownDuration
            );
        }

        /// <summary>
        /// Returns a new MouseSettings with the specified WheelThreshold, keeping all other values the same.
        /// </summary>
        public MouseSettings SetWheelThreshold(float wheelThreshold)
        {
            return new MouseSettings(
                Detection,
                MoveThreshold,
                wheelThreshold,
                MinPressCount,
                MinPressInterval,
                MinUsedDuration,
                RequireSpecialButtonForSelection,
                SelectionButtonPrimary,
                SelectionButtonSecondary,
                SelectionCooldownDuration
            );
        }

        /// <summary>
        /// Indicates whether the input device can be changed to mouse.
        /// </summary>
        /// <remarks>
        /// <c>Default is true</c>
        /// Does not affect <see cref="MouseDevice.WasUsedRaw"/> values. It does affect the automatic system in <see cref="ShapeInput"/> to switch between devices.
        /// </remarks>
        public readonly bool Detection;

        /// <summary>
        /// The minimum movement threshold to consider the mouse as "used".
        /// </summary>
        /// <remarks>
        /// <c>Default is 0.5f.</c>
        /// Even if <see cref="Detection"/> is set to <c>false</c>, <see cref="MoveThreshold"/> will still be used for <see cref="MouseDevice.WasUsedRaw"/> system.
        /// </remarks>
        public readonly float MoveThreshold;

        /// <summary>
        /// The minimum mouse wheel movement threshold to consider the mouse as "used".
        /// </summary>
        /// <remarks>
        /// <c>Default is 0.25f.</c>
        /// Even if <see cref="Detection"/> is set to <c>false</c>, <see cref="WheelThreshold"/> will still be used for <see cref="MouseDevice.WasUsedRaw"/> system.
        /// </remarks>
        public readonly float WheelThreshold;

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
        public readonly int MinPressCount;

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
        public readonly float MinPressInterval;

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
        public readonly float MinUsedDuration;

        /// <summary>
        /// When enabled, and at least one selection button is set to a value other than <c>ShapeMouseButton.NONE</c>,
        /// the input device can only be selected if any of the specified special buttons is pressed.
        /// </summary>
        /// <remarks>
        /// <c>Default is true.</c>
        /// This system automatically disables all other systems
        /// and only the 3 specified <c>SelectionButtons</c> will be checked for input!
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

        /// <summary>
        /// Specifies the duration (in seconds) after this device is selected during which no other input device can be selected.
        /// Used to prevent rapid switching between input devices.
        /// </summary>
        /// <remarks>
        /// <c>Default is 2 seconds</c>.
        /// Zero and negative values disable the cooldown.
        /// </remarks>
        public readonly float SelectionCooldownDuration;
    }
}