namespace ShapeEngine.Input;

public readonly partial struct InputDeviceUsageDetectionSettings
{
    /// <summary>
    /// Represents the settings used for detecting keyboard input device usage,
    /// including thresholds and detection options.
    /// </summary>
    public readonly struct KeyboardSettings
    {
        /// <summary>
        /// Gets the default <see cref="ShapeEngine.Input.InputDeviceUsageDetectionSettings.KeyboardSettings"/> instance with preset values.
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
        /// RequireSpecialButtonForSelection = false;
        /// SelectionButtonPrimary = ShapeKeyboardButton.NONE;
        /// SelectionButtonSecondary = ShapeKeyboardButton.NONE;
        /// SelectionButtonTertiary = ShapeKeyboardButton.NONE;
        /// SelectionCooldownDuration = 2f;
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
        public KeyboardSettings(bool detection, int minPressCount = 2, float minPressInterval = 1f, float minUsedDuration = 0.5f,
            float selectionCooldownDuration = 2f)
        {
            Detection = detection;
            MinPressCount = minPressCount;
            MinPressInterval = minPressInterval;
            MinUsedDuration = minUsedDuration;
            RequireSpecialButtonForSelection = false;
            SelectionButtonPrimary = ShapeKeyboardButton.NONE;
            SelectionButtonSecondary = ShapeKeyboardButton.NONE;
            SelectionButtonTertiary = ShapeKeyboardButton.NONE;
            SelectionCooldownDuration = selectionCooldownDuration;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeEngine.Input.InputDeviceUsageDetectionSettings.KeyboardSettings"/> struct with custom values,
        /// including detection, minimum press count, minimum press interval, minimum used duration,
        /// and custom selection buttons for device selection.
        /// <see cref="RequireSpecialButtonForSelection"/> and <see cref="Detection"/> is set to <c>true</c>.
        /// Other values are disabled because they do not work with special button selection.
        /// </summary>
        /// <param name="selectionButtonPrimary">The primary keyboard button used for device selection.</param>
        /// <param name="selectionButtonSecondary">The secondary keyboard button used for device selection.</param>
        /// <param name="selectionButtonTertiary">The tertiary keyboard button used for device selection.</param>
        /// <param name="selectionCooldownDuration">
        /// Specifies the cooldown duration (in seconds) after this device is selected
        /// during which no other input device can be selected.
        /// <c>Default is 2 seconds.</c>
        /// </param>
        public KeyboardSettings(ShapeKeyboardButton selectionButtonPrimary, ShapeKeyboardButton selectionButtonSecondary,
            ShapeKeyboardButton selectionButtonTertiary, float selectionCooldownDuration = 2f)
        {
            Detection = true;
            MinPressCount = -1;
            MinPressInterval = -1f;
            MinUsedDuration = -1f;
            RequireSpecialButtonForSelection = true;
            SelectionButtonPrimary = selectionButtonPrimary;
            SelectionButtonSecondary = selectionButtonSecondary;
            SelectionButtonTertiary = selectionButtonTertiary;
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
        /// Does not affect <see cref="ShapeKeyboardDevice.WasUsedRaw"/> values. It does affect the automatic system in <see cref="ShapeInput"/> to switch between devices.
        /// </remarks>
        public readonly bool Detection;

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
        public readonly int MinPressCount;

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
        public readonly float MinPressInterval;

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
        public readonly float MinUsedDuration;

        /// <summary>
        /// When enabled, and at least one selection button is set to a value other than <c>ShapeKeyboardButton.NONE</c>,
        /// the input device can only be selected if any of the specified special buttons is pressed.
        /// </summary>
        /// <remarks>
        /// <c>Default is true.</c>
        /// This system automatically disables all other systems
        /// and only the 3 specified <c>SelectionButtons</c> will be checked for input!
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