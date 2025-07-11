namespace ShapeEngine.Input;

public readonly partial struct InputDeviceUsageDetectionSettings
{
    /// <summary>
    /// Represents the settings used for detecting gamepad input device usage,
    /// including thresholds and detection options.
    /// </summary>
    public readonly struct GamepadSettings
    {
        /// <summary>
        /// Gets the default <see cref="MouseSettings"/> instance with preset values.
        /// </summary>
        /// <code>
        /// Detection = true;
        /// LeftAxisThreshold = 0.5f;
        /// RightAxisThreshold = 0.5f;
        /// LeftTriggerThreshold = 0.5f;
        /// RightTriggerThreshold = 0.5f;
        /// MinPressCount = 2;
        /// MinPressInterval = 1f;
        /// MinUsedDuration = 0.5f;
        /// RequireSpecialButtonForSelection = false;
        /// SelectionButtonPrimary = ShapeGamepadButton.NONE;
        /// SelectionButtonSecondary = ShapeGamepadButton.NONE;
        /// SelectionCooldownDuration = 2f;
        /// </code>
        public static readonly GamepadSettings Default = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeEngine.Input.InputDeviceUsageDetectionSettings.GamepadSettings"/> struct with default values.
        /// </summary>
        /// <code>
        /// Detection = true;
        /// LeftAxisThreshold = 0.5f;
        /// RightAxisThreshold = 0.5f;
        /// LeftTriggerThreshold = 0.5f;
        /// RightTriggerThreshold = 0.5f;
        /// MinPressCount = 2;
        /// MinPressInterval = 1f;
        /// MinUsedDuration = 0.5f;
        /// RequireSpecialButtonForSelection = false;
        /// SelectionButtonPrimary = ShapeGamepadButton.NONE;
        /// SelectionButtonSecondary = ShapeGamepadButton.NONE;
        /// SelectionCooldownDuration = 2f;
        /// </code>
        public GamepadSettings()
        {
            Detection = true;
            LeftAxisThreshold = 0.5f;
            RightAxisThreshold = 0.5f;
            LeftTriggerThreshold = 0.5f;
            RightTriggerThreshold = 0.5f;
            MinPressCount = 2;
            MinPressInterval = 1f;
            MinUsedDuration = 0.5f;
            RequireSpecialButtonForSelection = false;
            SelectionButtonPrimary = ShapeGamepadButton.NONE;
            SelectionButtonSecondary = ShapeGamepadButton.NONE;
            SelectionCooldownDuration = 2f;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeEngine.Input.InputDeviceUsageDetectionSettings.GamepadSettings"/> struct with custom detection, axis, and trigger threshold values.
        /// Other values are set to their defaults: MinPressCount = 2, MinPressInterval = 1f, MinUsedDuration = 0.5f.
        /// </summary>
        /// <param name="detection">Indicates whether gamepad detection is enabled.</param>
        /// <param name="leftAxisThreshold">The minimum movement threshold for the left analog stick to consider the gamepad as "used".</param>
        /// <param name="rightAxisThreshold">The minimum movement threshold for the right analog stick to consider the gamepad as "used".</param>
        /// <param name="leftTriggerThreshold">The minimum threshold for the left trigger to consider the gamepad as "used".</param>
        /// <param name="rightTriggerThreshold">The minimum threshold for the right trigger to consider the gamepad as "used".</param>
        /// <param name="selectionCooldownDuration">
        /// Specifies the cooldown duration (in seconds) after this device is selected
        /// during which no other input device can be selected.
        /// <c>Default is 2 seconds.</c>
        /// </param>
        public GamepadSettings(bool detection, float leftAxisThreshold, float rightAxisThreshold, float leftTriggerThreshold, float rightTriggerThreshold,
            float selectionCooldownDuration = 2f)
        {
            Detection = detection;
            LeftAxisThreshold = leftAxisThreshold;
            RightAxisThreshold = rightAxisThreshold;
            ;
            LeftTriggerThreshold = leftTriggerThreshold;
            RightTriggerThreshold = rightTriggerThreshold;
            MinPressCount = 2;
            MinPressInterval = 1f;
            MinUsedDuration = 0.5f;
            RequireSpecialButtonForSelection = false;
            SelectionButtonPrimary = ShapeGamepadButton.NONE;
            SelectionButtonSecondary = ShapeGamepadButton.NONE;
            SelectionCooldownDuration = selectionCooldownDuration;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeEngine.Input.InputDeviceUsageDetectionSettings.GamepadSettings"/> struct with custom axis and trigger thresholds,
        /// minimum press count, minimum press interval, minimum used duration, and selection cooldown duration.
        /// <see cref="Detection"/> is set to <c>true</c>.
        /// </summary>
        /// <param name="leftAxisThreshold">The minimum movement threshold for the left analog stick to consider the gamepad as "used".</param>
        /// <param name="rightAxisThreshold">The minimum movement threshold for the right analog stick to consider the gamepad as "used".</param>
        /// <param name="leftTriggerThreshold">The minimum threshold for the left trigger to consider the gamepad as "used".</param>
        /// <param name="rightTriggerThreshold">The minimum threshold for the right trigger to consider the gamepad as "used".</param>
        /// <param name="minPressCount">The minimum number of gamepad button presses required within the specified interval.</param>
        /// <param name="minPressInterval">The time interval (in seconds) from the first gamepad button press within which <paramref name="minPressCount"/> must be reached.</param>
        /// <param name="minUsedDuration">The minimum duration (in seconds) of consecutive gamepad use required to consider the gamepad device as "used".</param>
        /// <param name="selectionCooldownDuration">
        /// Specifies the cooldown duration (in seconds) after this device is selected
        /// during which no other input device can be selected.
        /// <c>Default is 2 seconds.</c>
        /// </param>
        public GamepadSettings(float leftAxisThreshold, float rightAxisThreshold, float leftTriggerThreshold, float rightTriggerThreshold, int minPressCount,
            float minPressInterval, float minUsedDuration, float selectionCooldownDuration = 2f)
        {
            Detection = true;
            LeftAxisThreshold = leftAxisThreshold;
            RightAxisThreshold = rightAxisThreshold;
            ;
            LeftTriggerThreshold = leftTriggerThreshold;
            RightTriggerThreshold = rightTriggerThreshold;
            MinPressCount = minPressCount;
            MinPressInterval = minPressInterval;
            MinUsedDuration = minUsedDuration;
            RequireSpecialButtonForSelection = false;
            SelectionButtonPrimary = ShapeGamepadButton.NONE;
            SelectionButtonSecondary = ShapeGamepadButton.NONE;
            SelectionCooldownDuration = selectionCooldownDuration;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeEngine.Input.InputDeviceUsageDetectionSettings.GamepadSettings"/> struct with custom values,
        /// including detection, move threshold, wheel threshold, minimum press count, minimum press interval,
        /// minimum used duration, and custom selection buttons for device selection.
        /// <see cref="RequireSpecialButtonForSelection"/> and <see cref="Detection"/> is set to <c>true</c>.
        /// Other values are disabled because the can not be used together with special buttons.
        /// <see cref="LeftAxisThreshold"/>/<see cref="RightAxisThreshold"/> and <see cref="LeftTriggerThreshold"/>/<see cref="RightTriggerThreshold"/> are set to default values
        /// for the <see cref="ShapeGamepadDevice.WasUsedRaw"/> system.
        /// </summary>
        /// <param name="selectionButtonPrimary">Specifies the primary gamepad button to be used for device selection.
        /// If not a valid Raylib gamepad button,
        /// <c>ShapeGamepadButton.NONE</c> is used.</param>
        /// <param name="selectionButtonSecondary">Specifies the secondary gamepad button to be used for device selection.
        /// If not a valid Raylib gamepad button,
        /// <c>ShapeGamepadButton.NONE</c> is used.</param>
        /// <param name="selectionCooldownDuration">
        /// Specifies the cooldown duration (in seconds) after this device is selected
        /// during which no other input device can be selected.
        /// <c>Default is 2 seconds.</c>
        /// </param>
        public GamepadSettings(ShapeGamepadButton selectionButtonPrimary, ShapeGamepadButton selectionButtonSecondary, float selectionCooldownDuration = 2f)
        {
            Detection = true;
            LeftAxisThreshold = 0.5f;
            RightAxisThreshold = 0.5f;
            LeftTriggerThreshold = 0.5f;
            RightTriggerThreshold = 0.5f;
            MinPressCount = -1;
            MinPressInterval = -1f;
            MinUsedDuration = -1f;
            RequireSpecialButtonForSelection = true;
            SelectionButtonPrimary = ShapeGamepadDevice.IsValidRaylibGamepadButton(selectionButtonPrimary) ? selectionButtonPrimary : ShapeGamepadButton.NONE;
            SelectionButtonSecondary = ShapeGamepadDevice.IsValidRaylibGamepadButton(selectionButtonSecondary)
                ? selectionButtonSecondary
                : ShapeGamepadButton.NONE;
            SelectionCooldownDuration = selectionCooldownDuration;
        }

        private GamepadSettings(
            bool detection,
            float leftAxisThreshold, float rightAxisThreshold, float leftTriggerThreshold, float rightTriggerThreshold,
            int minPressCount, float minPressInterval, float minUsedDuration,
            bool requireSpecialButtonForSelection, ShapeGamepadButton selectionButtonPrimary, ShapeGamepadButton selectionButtonSecondary,
            float selectionCooldownDuration)
        {
            Detection = detection;
            LeftAxisThreshold = leftAxisThreshold;
            RightAxisThreshold = rightAxisThreshold;
            LeftTriggerThreshold = leftTriggerThreshold;
            RightTriggerThreshold = rightTriggerThreshold;
            MinPressCount = minPressCount;
            MinPressInterval = minPressInterval;
            MinUsedDuration = minUsedDuration;
            RequireSpecialButtonForSelection = requireSpecialButtonForSelection;
            SelectionButtonPrimary = selectionButtonPrimary;
            SelectionButtonSecondary = selectionButtonSecondary;
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
        /// Indicates whether any gamepad axis threshold is enabled (greater than 0).
        /// Returns <c>true</c> if either <see cref="LeftAxisThreshold"/> or <see cref="RightAxisThreshold"/> is greater than 0.
        /// </summary>
        public bool AxisThresholdEnabled => LeftAxisThreshold > 0f || RightAxisThreshold > 0f;

        /// <summary>
        /// Indicates whether any gamepad trigger threshold is enabled (greater than 0).
        /// Returns <c>true</c> if either <see cref="LeftTriggerThreshold"/> or <see cref="RightTriggerThreshold"/> is greater than 0.
        /// </summary>
        public bool TriggerThresholdEnabled => LeftTriggerThreshold > 0f || RightTriggerThreshold > 0f;

        /// <summary>
        /// Gets a value indicating whether selection button detection is enabled for the gamepad.
        /// Returns <c>true</c> if <see cref="RequireSpecialButtonForSelection"/> is enabled and at least one of
        /// <see cref="SelectionButtonPrimary"/> or <see cref="SelectionButtonSecondary"/> is not <c>ShapeGamepadButton.NONE</c>.
        /// </summary>
        public bool SpecialButtonSelectionSystemEnabled => RequireSpecialButtonForSelection &&
                                                           (SelectionButtonPrimary != ShapeGamepadButton.NONE ||
                                                            SelectionButtonSecondary != ShapeGamepadButton.NONE);

        /// <summary>
        /// Returns a new GamepadSettings with the specified axis thresholds, keeping all other values the same.
        /// </summary>
        public GamepadSettings SetAxisThresholds(float leftAxisThreshold, float rightAxisThreshold)
        {
            return new GamepadSettings(
                Detection,
                leftAxisThreshold,
                rightAxisThreshold,
                LeftTriggerThreshold,
                RightTriggerThreshold,
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
        /// Returns a new GamepadSettings with the specified trigger thresholds, keeping all other values the same.
        /// </summary>
        public GamepadSettings SetTriggerThresholds(float leftTriggerThreshold, float rightTriggerThreshold)
        {
            return new GamepadSettings(
                Detection,
                LeftAxisThreshold,
                RightAxisThreshold,
                leftTriggerThreshold,
                rightTriggerThreshold,
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
        /// Indicates whether the input device can be changed to gamepad.
        /// </summary>
        /// <remarks>
        /// <c>Default is true</c>
        /// Does not affect <see cref="ShapeGamepadDevice.WasUsedRaw"/> values. It does affect the automatic system in <see cref="ShapeInput"/> to switch between devices.
        /// </remarks>
        public readonly bool Detection;

        /// <summary>
        /// The minimum movement threshold for the left analog stick to consider the gamepad as "used".
        /// </summary>
        /// <remarks>
        /// <c>Default is 0.5f.</c>
        /// Even if <see cref="Detection"/> is set to <c>false</c>, <see cref="LeftAxisThreshold"/> will still be used for <see cref="ShapeGamepadDevice.WasUsedRaw"/> system.
        /// </remarks>
        public readonly float LeftAxisThreshold;

        /// <summary>
        /// The minimum movement threshold for the right analog stick to consider the gamepad as "used".
        /// </summary>
        /// <remarks>
        /// <c>Default is 0.5f.</c>
        /// Even if <see cref="Detection"/> is set to <c>false</c>, <see cref="RightAxisThreshold"/> will still be used for <see cref="ShapeGamepadDevice.WasUsedRaw"/> system.
        /// </remarks>
        public readonly float RightAxisThreshold;

        /// <summary>
        /// The minimum threshold for the left trigger to consider the gamepad as "used".
        /// </summary>
        /// <remarks>
        /// <c>Default is 0.5f.</c>
        /// Even if <see cref="Detection"/> is set to <c>false</c>, <see cref="LeftTriggerThreshold"/> will still be used for <see cref="ShapeGamepadDevice.WasUsedRaw"/> system.
        /// </remarks>
        public readonly float LeftTriggerThreshold;

        /// <summary>
        /// The minimum threshold for the right trigger to consider the gamepad as "used".
        /// </summary>
        /// <remarks>
        /// <c>Default is 0.5f.</c>
        /// Even if <see cref="Detection"/> is set to <c>false</c>, <see cref="RightTriggerThreshold"/> will still be used for <see cref="ShapeGamepadDevice.WasUsedRaw"/> system.
        /// </remarks>
        public readonly float RightTriggerThreshold;

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
        public readonly int MinPressCount;

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
        public readonly float MinPressInterval;

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
        public readonly float MinUsedDuration;

        /// <summary>
        /// When enabled, and at least one selection button is set to a value other than <c>ShapeGamepadButton.NONE</c>,
        /// the input device can only be selected if any of the specified special buttons is pressed.
        /// </summary>
        /// <remarks>
        /// <c>Default is true.</c>
        /// This system automatically disables all other systems
        /// and only the 3 specified <c>SelectionButtons</c> will be checked for input!
        /// </remarks>
        public readonly bool RequireSpecialButtonForSelection;

        /// <summary>
        /// The primary gamepad button used for device selection (e.g., A Button).
        /// </summary>
        /// <remarks>
        /// <see cref="RequireSpecialButtonForSelection"/> needs to be <c>true</c> for this button to be considered.
        /// <c>Default is ShapeGamepadButton.NONE.</c>
        /// </remarks>
        public readonly ShapeGamepadButton SelectionButtonPrimary;

        /// <summary>
        /// The secondary keyboard button used for device selection (e.g., Start Button).
        /// </summary>
        /// <remarks>
        /// <see cref="RequireSpecialButtonForSelection"/> needs to be <c>true</c> for this button to be considered.
        /// <c>Default is ShapeGamepadButton.NONE.</c>
        /// </remarks>
        public readonly ShapeGamepadButton SelectionButtonSecondary;

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