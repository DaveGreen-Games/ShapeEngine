using Raylib_cs;

namespace ShapeEngine.Input;

/// <summary>
/// Represents a gamepad input device, providing access to gamepad buttons and axes,
/// state tracking, calibration, and utility methods for gamepad input.
/// </summary>
public sealed class GamepadDevice : InputDevice
{
    /// <summary>
    /// Represents the minimum and maximum values for a gamepad axis, used for calibration.
    /// </summary>
    private readonly struct AxisRange
    {
        /// <summary>
        /// The minimum value recorded for the axis.
        /// </summary>
        public readonly float Minimum;
        /// <summary>
        /// The maximum value recorded for the axis.
        /// </summary>
        public readonly float Maximum;

        /// <summary>
        /// The total range between minimum and maximum.
        /// </summary>
        public float TotalRange => MathF.Abs(Maximum - Minimum);

        /// <summary>
        /// Initializes a new instance of the <see cref="AxisRange"/> struct.
        /// </summary>
        public AxisRange(float minimum, float maximum)
        {
            Minimum = minimum;
            Maximum = maximum;
        }

        /// <summary>
        /// Updates the axis range with a new value.
        /// </summary>
        public AxisRange UpdateRange(float newValue)
        {
            if (newValue < Minimum) return new(newValue, Maximum);
            if (newValue > Maximum) return new(Minimum, newValue);
            return new(Minimum, Maximum);
        }
    }
    
    /// <summary>
    /// All available Raylib gamepad buttons.
    /// </summary>
    public static readonly GamepadButton[] AllGamepadButtons = Enum.GetValues<GamepadButton>();
    /// <summary>
    /// All available Raylib gamepad axes.
    /// </summary>
    public static readonly GamepadAxis[] AllGamepadAxis = Enum.GetValues<GamepadAxis>();
    /// <summary>
    /// All available Shape gamepad buttons.
    /// </summary>
    public static readonly ShapeGamepadButton[] AllShapeGamepadButtons = Enum.GetValues<ShapeGamepadButton>();
    /// <summary>
    /// All available Shape gamepad joystick axes.
    /// </summary>
    public static readonly ShapeGamepadJoyAxis[] AllShapeGamepadJoyAxis = Enum.GetValues<ShapeGamepadJoyAxis>();
    /// <summary>
    /// All available Shape gamepad trigger axes.
    /// </summary>
    public static readonly ShapeGamepadTriggerAxis[] AllShapeGamepadTriggerAxis = Enum.GetValues<ShapeGamepadTriggerAxis>();
    
    private readonly Dictionary<ShapeGamepadButton, InputState> buttonStates = new (AllShapeGamepadButtons.Length);
    private readonly Dictionary<ShapeGamepadJoyAxis, InputState> joyAxisStates = new (AllShapeGamepadJoyAxis.Length);
    private readonly Dictionary<ShapeGamepadTriggerAxis, InputState> triggerAxisStates = new (AllShapeGamepadTriggerAxis.Length);
    /// <summary>
    /// Event triggered when a gamepad button is pressed.
    /// </summary>
    public event Action<GamepadDevice, ShapeGamepadButton>? OnButtonPressed;
    /// <summary>
    /// Event triggered when a gamepad button is released.
    /// </summary>
    public event Action<GamepadDevice, ShapeGamepadButton>? OnButtonReleased;

    internal event Action<GamepadDevice, InputDeviceUsageDetectionSettings>? OnInputDeviceChangeSettingsChanged;
    
    /// <summary>
    /// The index of this gamepad device.
    /// </summary>
    public readonly int Index;
    
    /// <summary>
    /// Gets whether this gamepad is available for use.
    /// </summary>
    public bool Available { get; private set; } = true;
    /// <summary>
    /// Gets whether this gamepad is currently connected.
    /// </summary>
    public bool Connected { get; private set; }

    /// <summary>
    /// The name of the gamepad device.
    /// </summary>
    public string Name { get; private set; } = "No Device";
    /// <summary>
    /// The number of axes supported by this gamepad.
    /// </summary>
    public int AxisCount { get; private set; }

    /// <summary>
    /// Gets the usage detection settings for the gamepad input device.
    /// </summary>
    public InputDeviceUsageDetectionSettings.GamepadSettings UsageDetectionSettings { get; private set; } = new();

    /// <summary>
    /// <para>
    /// The process priority of this gamepad device instance.
    /// </para>
    /// <para>
    /// Change this value to change the order in which this device is processed in <see cref="ShapeInput"/>.
    /// Lower priorities are processed first.
    /// </para>
    /// </summary>
    /// <remarks>
    /// A unique value based on the order of instantiation is assigned per default.
    /// </remarks>
    public uint DeviceProcessPriority = processPriorityCounter++;
    
    private bool isActive;
    private bool isLocked;
    private bool wasUsed;
    private bool wasUsedRaw;
    private int pressedCount;
    private float pressedCountDurationTimer;
    private float usedDurationTimer;
    
    /// <summary>
    /// List of gamepad buttons pressed  since the last update.
    /// </summary>
    public readonly List<ShapeGamepadButton> PressedButtons = [];
    /// <summary>
    /// List of gamepad buttons released since the last update.
    /// </summary>
    public readonly List<ShapeGamepadButton> ReleasedButtons = [];
    /// <summary>
    /// List of gamepad buttons in use.
    /// </summary>
    public readonly List<ShapeGamepadButton> HeldDownButtons = [];
    
    
    /// <summary>
    /// List of gamepad axes used this frame, but not being in use last frame.
    /// </summary>
    public readonly List<ShapeGamepadJoyAxis> PressedJoyAxis = [];
    
    /// <summary>
    /// List of gamepad axes released since the last update.
    /// </summary>
    public readonly List<ShapeGamepadJoyAxis> ReleasedJoyAxis = [];

    /// <summary>
    /// List of gamepad axes in use since multiple frames.
    /// </summary>
    public readonly List<ShapeGamepadJoyAxis> HeldJoyAxis = [];
 
    
    /// <summary>
    /// List of trigger axes pressed since the last update.
    /// </summary>
    public readonly List<ShapeGamepadTriggerAxis> PressedTriggerAxis = [];
    
    /// <summary>
    /// List of trigger axes released since the last update.
    /// </summary>
    public readonly List<ShapeGamepadTriggerAxis> ReleasedTriggerAxis = [];
    
    /// <summary>
    /// List of trigger axes held down (in use) since multiple frames.
    /// </summary>
    public readonly List<ShapeGamepadTriggerAxis> HeldTriggerAxis = [];
    
    /// <summary>
    /// Event triggered when the connection state changes.
    /// </summary>
    public event Action? OnConnectionChanged;
    /// <summary>
    /// Event triggered when the availability state changes.
    /// </summary>
    public event Action? OnAvailabilityChanged;

    private readonly Dictionary<ShapeGamepadJoyAxis, float> joyAxisZeroCalibration = new();
    private readonly Dictionary<ShapeGamepadTriggerAxis, float> triggerZeroCalibration = new();
    private Dictionary<ShapeGamepadJoyAxis, AxisRange> joyAxisRanges = new();
    private Dictionary<ShapeGamepadTriggerAxis, AxisRange> triggerAxisRanges = new();
    
    /// <summary>
    /// Initializes a new instance of the <see cref="GamepadDevice"/> class.
    /// </summary>
    /// <param name="index">The gamepad index.</param>
    /// <param name="connected">Whether the gamepad is initially connected.</param>
    public GamepadDevice(int index, bool connected)
    {
        Index = index;
        
        Connected = connected;
        if (Connected)
        {
            unsafe
            {
                Name = Raylib.GetGamepadName(index)->ToString();
            }

            AxisCount = Raylib.GetGamepadAxisCount(index);
            
            Calibrate();
        }
        
        foreach (var button in AllShapeGamepadButtons)
        {
            buttonStates.Add(button, new());
        }
        foreach (var axis in AllShapeGamepadJoyAxis)
        {
            joyAxisStates.Add(axis, new());
        }
        foreach (var axis in AllShapeGamepadTriggerAxis)
        {
            triggerAxisStates.Add(axis, new());
        }
    }

    /// <inheritdoc cref="InputDevice.GetDeviceProcessPriority"/>
    public override uint GetDeviceProcessPriority() => DeviceProcessPriority;
    
    /// <summary>
    /// Gets the type of this input device, which is always <see cref="InputDeviceType.Gamepad"/>.
    /// </summary>
    public override InputDeviceType GetDeviceType() => InputDeviceType.Gamepad;
    
    /// <summary>
    /// Gets the current input state for the specified gamepad button.
    /// </summary>
    public InputState GetButtonState(ShapeGamepadButton button) => buttonStates[button];
    /// <summary>
    /// Gets the current input state for the specified gamepad axis.
    /// </summary>
    public InputState GetAxisState(ShapeGamepadJoyAxis axis) => joyAxisStates[axis];
    /// <summary>
    /// Gets the current input state for the specified gamepad trigger axis.
    /// </summary>
    /// <param name="axis">The gamepad trigger axis.</param>
    /// <returns>The current <see cref="InputState"/> for the specified trigger axis.</returns>
    public InputState GetAxisState(ShapeGamepadTriggerAxis axis) => triggerAxisStates[axis];
    /// <summary>
    /// Consumes the input state for the specified gamepad button.
    /// Marks the state as consumed and updates the dictionary.
    /// Returns null if the state was already consumed.
    /// </summary>
    /// <param name="button">The gamepad button to consume the state for.</param>
    /// <param name="valid">True if the state was not already consumed; otherwise, false.</param>
    /// <returns>The consumed <see cref="InputState"/> or null if already consumed.</returns>
    public InputState ConsumeButtonState(ShapeGamepadButton button, out bool valid)
    {
        valid = false;
        var state = buttonStates[button];
        if (state.Consumed) return state;

        valid = true;
        buttonStates[button] = state.Consume();
        return state;
    }
    /// <summary>
    /// Consumes the input state for the specified gamepad axis.
    /// Marks the state as consumed and updates the dictionary.
    /// Returns null if the state was already consumed.
    /// </summary>
    /// <param name="axis">The gamepad axis to consume the state for.</param>
    /// <param name="valid">True if the state was not already consumed; otherwise, false.</param>
    /// <returns>The consumed <see cref="InputState"/> or null if already consumed.</returns>
    public InputState ConsumeAxisState(ShapeGamepadJoyAxis axis, out bool valid)
    {
        valid = false;
        var state = joyAxisStates[axis];
        if (state.Consumed) return state;

        valid = true;
        joyAxisStates[axis] = state.Consume();
        return state;
    }
    /// <summary>
    /// Consumes the input state for the specified gamepad trigger axis.
    /// Marks the state as consumed and updates the dictionary.
    /// Returns the consumed <see cref="InputState"/> or the current state if already consumed.
    /// </summary>
    /// <param name="axis">The gamepad trigger axis to consume the state for.</param>
    /// <param name="valid">True if the state was not already consumed; otherwise, false.</param>
    /// <returns>The consumed <see cref="InputState"/> or the current state if already consumed.</returns>
    public InputState ConsumeAxisState(ShapeGamepadTriggerAxis axis, out bool valid)
    {
        valid = false;
        var state = triggerAxisStates[axis];
        if (state.Consumed) return state;

        valid = true;
        triggerAxisStates[axis] = state.Consume();
        return state;
    }
    /// <summary>
    /// Maps a <see cref="ShapeGamepadButton"/> to its corresponding <see cref="ShapeGamepadJoyAxis"/> or <see cref="ShapeGamepadTriggerAxis"/>.
    /// Returns a tuple where only one of the axes is set, depending on the button.
    /// </summary>
    /// <param name="button">The gamepad button to map.</param>
    /// <returns>
    /// A tuple containing the corresponding <see cref="ShapeGamepadJoyAxis"/> or <see cref="ShapeGamepadTriggerAxis"/>.
    /// If the button does not map to an axis, both values are null.
    /// </returns>
    public static (ShapeGamepadJoyAxis? joyAxis, ShapeGamepadTriggerAxis? triggerAxis) ToShapeGamepadAxis(ShapeGamepadButton button)
    {
        return button switch
        {
            ShapeGamepadButton.LEFT_STICK_RIGHT =>  (ShapeGamepadJoyAxis.LEFT_X, null),
            ShapeGamepadButton.LEFT_STICK_LEFT =>   (ShapeGamepadJoyAxis.LEFT_X, null),
            ShapeGamepadButton.LEFT_STICK_UP =>     (ShapeGamepadJoyAxis.LEFT_Y, null),
            ShapeGamepadButton.LEFT_STICK_DOWN =>   (ShapeGamepadJoyAxis.LEFT_Y, null),
            ShapeGamepadButton.RIGHT_STICK_RIGHT => (ShapeGamepadJoyAxis.RIGHT_X, null),
            ShapeGamepadButton.RIGHT_STICK_LEFT =>  (ShapeGamepadJoyAxis.RIGHT_X, null),
            ShapeGamepadButton.RIGHT_STICK_UP =>    (ShapeGamepadJoyAxis.RIGHT_Y, null),
            ShapeGamepadButton.RIGHT_STICK_DOWN =>  (ShapeGamepadJoyAxis.RIGHT_Y, null),
            ShapeGamepadButton.LEFT_TRIGGER_BOTTOM =>  (null, ShapeGamepadTriggerAxis.LEFT),
            ShapeGamepadButton.RIGHT_TRIGGER_BOTTOM => (null, ShapeGamepadTriggerAxis.RIGHT),
            _ => (null, null)
        };
    }
    
    /// <summary>
    /// Applies the specified change settings to this gamepad device,
    /// modifying how device usage is detected and processed.
    /// Also propagates the settings to all other <see cref="GamepadDevice"/> instances and the <see cref="GamepadDeviceManager"/>.
    /// </summary>
    /// <param name="settings">The change settings to apply to the input device.</param>
    public override void ApplyInputDeviceChangeSettings(InputDeviceUsageDetectionSettings settings)
    {
        UsageDetectionSettings = settings.Gamepad;
        OnInputDeviceChangeSettingsChanged?.Invoke(this, settings);
    }
    internal void OverrideInputDeviceChangeSettings(InputDeviceUsageDetectionSettings settings)
    {
        UsageDetectionSettings = settings.Gamepad;
    }

    /// <inheritdoc cref="InputDevice.WasUsed"/>
    public override bool WasUsed() => wasUsed;
    
    /// <inheritdoc cref="InputDevice.WasUsedRaw"/>
    public override bool WasUsedRaw() => wasUsedRaw;
    
    /// <summary>
    /// Returns whether the gamepad device is currently locked.
    /// Locking can be used to temporarily disable input processing.
    /// This does not affect whether the device is active or not.
    /// </summary>
    public override bool IsLocked() => isLocked;

    /// <summary>
    /// Locks the gamepad device, preventing input from being registered.
    /// </summary>
    public override void Lock()
    {
        if (isLocked) return;
        isLocked = true;
        
        usedDurationTimer = 0f;
        pressedCount = 0;
        pressedCountDurationTimer = 0f;
        
        PressedButtons.Clear();
        ReleasedButtons.Clear();
        HeldDownButtons.Clear();
        PressedJoyAxis.Clear();
        ReleasedJoyAxis.Clear();
        HeldJoyAxis.Clear();
        PressedTriggerAxis.Clear();
        ReleasedTriggerAxis.Clear();
        HeldTriggerAxis.Clear();
    }

    /// <summary>
    /// Unlocks the gamepad device, allowing input to be registered.
    /// </summary>
    public override void Unlock()
    {
        if (!isLocked) return;
        isLocked = false;
    }
    
    /// <summary>
    /// Indicates whether the mouse device is currently active, as in being used by the <see cref="ShapeInput"/> class to generate input.
    /// </summary>
    public override bool IsActive() => isActive;
    
    /// <summary>
    /// Activates the mouse device, enabling input processing.
    /// </summary>
    public override void Activate()
    {
        if (isActive) return;
        isActive = true;
    }
    
    /// <summary>
    /// Deactivates the mouse device, disabling input processing and resetting state.
    /// </summary>
    public override void Deactivate()
    {
        if (!isActive) return;
        isActive = false;
        
        usedDurationTimer = 0f;
        pressedCount = 0;
        pressedCountDurationTimer = 0f;
        
        PressedButtons.Clear();
        ReleasedButtons.Clear();
        HeldDownButtons.Clear();
        PressedJoyAxis.Clear();
        ReleasedJoyAxis.Clear();
        HeldJoyAxis.Clear();
        PressedTriggerAxis.Clear();
        ReleasedTriggerAxis.Clear();
        HeldTriggerAxis.Clear();
    }

    /// <inheritdoc cref="InputDevice.Update"/>
    public override bool Update(float dt, bool wasOtherDeviceUsed)
    {
        PressedButtons.Clear();
        ReleasedButtons.Clear();
        HeldDownButtons.Clear();
        PressedJoyAxis.Clear();
        ReleasedJoyAxis.Clear();
        HeldJoyAxis.Clear();
        PressedTriggerAxis.Clear();
        ReleasedTriggerAxis.Clear();
        HeldTriggerAxis.Clear();
        
        UpdateButtonStates();
        UpdateJoyAxisStates();
        UpdateTriggerAxisStates();
        
        if (!Connected || isLocked)
        {
            wasUsed = false;
            wasUsedRaw = false;
            return false;
        }
        
        WasGamepadUsed(dt, wasOtherDeviceUsed, out wasUsed, out wasUsedRaw);

        return wasUsed;
    }

    private void WasGamepadUsed(float dt, bool wasOtherDeviceUsed, out bool used, out bool usedRaw)
    {
        used = false;
        usedRaw = false;
        
        if (wasOtherDeviceUsed)
        {
            usedDurationTimer = 0f;
            pressedCount = 0;
            pressedCountDurationTimer = 0f;
        }
        
        if (isLocked) return;

        usedRaw = PressedButtons.Count > 0;

        if (!UsageDetectionSettings.Detection || wasOtherDeviceUsed) return;
            
        if (UsageDetectionSettings.SelectionButtons is { Count: > 0 })
        {
            if (UsageDetectionSettings.ExceptionButtons is { Count: > 0 })
            {
                foreach (var button in UsageDetectionSettings.SelectionButtons)
                {
                    if (UsageDetectionSettings.ExceptionButtons.Contains(button)) continue;
                    if (IsDown(button, UsageDetectionSettings.AxisThreshold, UsageDetectionSettings.TriggerThreshold))
                    {
                        used = true;
                        break;
                    }
                }
            }
            else
            {
                foreach (var button in UsageDetectionSettings.SelectionButtons)
                {
                    if (IsDown(button, UsageDetectionSettings.AxisThreshold, UsageDetectionSettings.TriggerThreshold))
                    {
                        used = true;
                        break;
                    }
                }
            }
        }
        else
        {
            var pressCountEnabled = UsageDetectionSettings.PressCountEnabled;
            var usedDurationEnabled = UsageDetectionSettings.UsedDurationEnabled;
            
            if (pressCountEnabled)
            {
                pressedCountDurationTimer += dt;
                if (pressedCountDurationTimer >= UsageDetectionSettings.MinPressInterval)
                {
                    pressedCountDurationTimer -= UsageDetectionSettings.MinPressInterval;
                    pressedCount = 0;
                }
            }

            if (usedDurationEnabled)
            {
                // Checks if any held down button is not in the exception list (or if the exception list is null)
                if (HeldDownButtons.Any(b => !UsageDetectionSettings.ExceptionButtons.Contains(b)))
                // if (usedDurationEnabled && HeldDownButtons.Count > 0)
                {
                    usedDurationTimer += dt;
                    if (usedDurationTimer > UsageDetectionSettings.MinUsedDuration)
                    {
                        usedDurationTimer -= UsageDetectionSettings.MinUsedDuration;
                        used = true;
                        pressedCount = 0;
                        pressedCountDurationTimer = 0f;
                        return;
                    }
                }
                else if (usedDurationTimer > 0f) usedDurationTimer = 0f;
            }
            
            
            if (pressCountEnabled && PressedButtons.Any(b => !UsageDetectionSettings.ExceptionButtons.Contains(b)))
            // if (pressCountEnabled && PressedButtons.Count > 0)
            {
                pressedCount++;
                if (pressedCount >= UsageDetectionSettings.MinPressCount)
                {
                    used = true;
                    pressedCountDurationTimer = 0f;
                    usedDurationTimer = 0f;
                    pressedCount = 0;
                }
            }
        }
    }
    
    /// <summary>
    /// Marks the gamepad as connected and calibrates its axes.
    /// </summary>
    public void Connect()
    {
        if (Connected) return;
        Connected = true;
        unsafe
        {
            Name = Raylib.GetGamepadName(Index)->ToString();
        }

        AxisCount = Raylib.GetGamepadAxisCount(Index);
        OnConnectionChanged?.Invoke();
        
        joyAxisRanges.Clear();
        
        Calibrate();
    }
    /// <summary>
    /// Marks the gamepad as disconnected.
    /// </summary>
    public void Disconnect()
    {
        if (!Connected) return;
        Connected = false;
        OnConnectionChanged?.Invoke();
        
        usedDurationTimer = 0f;
        pressedCount = 0;
        pressedCountDurationTimer = 0f;
        
        PressedButtons.Clear();
        ReleasedButtons.Clear();
        HeldDownButtons.Clear();
        PressedJoyAxis.Clear();
        ReleasedJoyAxis.Clear();
        HeldJoyAxis.Clear();
        PressedTriggerAxis.Clear();
        ReleasedTriggerAxis.Clear();
        HeldTriggerAxis.Clear();
    }
    /// <summary>
    /// Claims the gamepad for use, marking it as unavailable.
    /// </summary>
    /// <returns>True if successfully claimed, otherwise false.</returns>
    public bool Claim()
    {
        if (!Connected || !Available) return false;
        Available = false;
        OnAvailabilityChanged?.Invoke();
        return true;
    }
    /// <summary>
    /// Frees the gamepad, marking it as available.
    /// </summary>
    /// <returns>True if successfully freed, otherwise false.</returns>
    public bool Free()
    {
        if (Available) return false;
        Available = true;
        OnAvailabilityChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Calibrates the gamepad axes by recording their current zero positions.
    /// </summary>
    public override void Calibrate()
    {
        float leftX = Raylib.GetGamepadAxisMovement(Index, GamepadAxis.LeftX);
        float leftY = Raylib.GetGamepadAxisMovement(Index, GamepadAxis.LeftY);
        
        float rightX = Raylib.GetGamepadAxisMovement(Index, GamepadAxis.RightX);
        float rightY = Raylib.GetGamepadAxisMovement(Index, GamepadAxis.RightY);
        
        float triggerRight = Raylib.GetGamepadAxisMovement(Index, GamepadAxis.LeftTrigger);
        float triggerLeft = Raylib.GetGamepadAxisMovement(Index, GamepadAxis.RightTrigger);

        joyAxisZeroCalibration[ShapeGamepadJoyAxis.LEFT_X] = leftX;
        joyAxisZeroCalibration[ShapeGamepadJoyAxis.LEFT_Y] = leftY;
        joyAxisZeroCalibration[ShapeGamepadJoyAxis.RIGHT_X] = rightX;
        joyAxisZeroCalibration[ShapeGamepadJoyAxis.RIGHT_Y] = rightY;
        
        triggerZeroCalibration[ShapeGamepadTriggerAxis.RIGHT] = triggerRight;
        triggerZeroCalibration[ShapeGamepadTriggerAxis.LEFT] = triggerLeft;

    }
    
    /// <summary>
    /// Updates the states of all gamepad buttons.
    /// </summary>
    private void UpdateButtonStates()
    {
        foreach (var state in buttonStates)
        {
            var button = state.Key;
            var prevState = state.Value;
            var curState = CreateInputState(button, UsageDetectionSettings.AxisThreshold, UsageDetectionSettings.TriggerThreshold);
            var nextState = new InputState(prevState, curState);
            buttonStates[button] = nextState;

            if(nextState.Down) HeldDownButtons.Add(button);
            
            if (nextState.Pressed)
            {
                PressedButtons.Add(button);
                OnButtonPressed?.Invoke(this, button);
            }
            else if (nextState.Released)
            {
                ReleasedButtons.Add(button);
                OnButtonReleased?.Invoke(this, button);
            }

        }
    }

    /// <summary>
    /// Updates the states of all gamepad axes.
    /// </summary>
    private void UpdateJoyAxisStates()
    {
        foreach (var state in joyAxisStates)
        {
            var axis = state.Key;
            var prevState = state.Value;
            var curState = CreateInputState(axis, UsageDetectionSettings.AxisThreshold);
            var nextState = new InputState(prevState, curState);
            joyAxisStates[axis] = nextState;
            
            if(nextState.Down) HeldJoyAxis.Add(axis);
            if(nextState.Pressed) PressedJoyAxis.Add(axis);
            else if(nextState.Released) ReleasedJoyAxis.Add(axis);
        }
    }
    private void UpdateTriggerAxisStates()
    {
        foreach (var state in triggerAxisStates)
        {
            var axis = state.Key;
            var prevState = state.Value;
            var curState = CreateInputState(axis, UsageDetectionSettings.TriggerThreshold);
            var nextState = new InputState(prevState, curState);
            triggerAxisStates[axis] = nextState;
            
            if(nextState.Down) HeldTriggerAxis.Add(axis);
            if(nextState.Pressed) PressedTriggerAxis.Add(axis);
            else if(nextState.Released) ReleasedTriggerAxis.Add(axis);
        }
    }
    
    #region Button
    /// <summary>
    /// Gets the value of the specified gamepad button.
    /// Returns a float representing the button's value, considering axis and trigger deadzones and an optional modifier key set.
    /// </summary>
    /// <param name="button">The gamepad button to query.</param>
    /// <param name="axisDeadzone">Deadzone value for axis input. </param>
    /// <param name="triggerDeadzone">Deadzone value for trigger input. </param>
    /// <param name="modifierKeySet">Optional modifier key set to check if active.</param>
    /// <returns>The value of the button as a float.</returns>
    public float GetValue(ShapeGamepadButton button, 
        float axisDeadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultJoyAxisThreshold, 
        float triggerDeadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultTriggerAxisThreshold, 
        ModifierKeySet? modifierKeySet = null)
    {
        if (Index < 0 || isLocked || !Connected) return 0f;
        if (modifierKeySet != null && !modifierKeySet.IsActive(this)) return 0f;
        if (button == ShapeGamepadButton.LEFT_TRIGGER_BOTTOM)
        {
            float value = GetValue(ShapeGamepadTriggerAxis.LEFT, triggerDeadzone);
            if (MathF.Abs(value) < triggerDeadzone) value = 0f;
            return value;
        }
        
        if (button == ShapeGamepadButton.RIGHT_TRIGGER_BOTTOM)
        {
            float value = GetValue(ShapeGamepadTriggerAxis.RIGHT, triggerDeadzone);
            if (MathF.Abs(value) < triggerDeadzone) value = 0f;
            return value;
        }
        
        var id = (int)button;
        if (id >= 30 && id <= 33)
        {
            id -= 30;
            float value = GetValue((ShapeGamepadJoyAxis)id, axisDeadzone);
            if (MathF.Abs(value) < axisDeadzone) value = 0f;
            return value;
        }
        
        if (id >= 40 && id <= 43)
        {
            id -= 40;
            float value = GetValue((ShapeGamepadJoyAxis)id, axisDeadzone);
            if (MathF.Abs(value) < axisDeadzone) value = 0f;
            return value;
        }
        
        return Raylib.IsGamepadButtonDown(Index, (GamepadButton)id) ? 1f : 0f;
    }
    /// <summary>
    /// Determines if the specified gamepad button is "down" using separate deadzone values for axis and trigger.
    /// </summary>
    /// <param name="button">The gamepad button.</param>
    /// <param name="axisDeadzone">Deadzone for axis input.</param>
    /// <param name="triggerDeadzone">Deadzone for trigger input.</param>
    /// <param name="modifierKeySet">Optional modifier key set.</param>
    /// <returns>True if the button is down; otherwise, false.</returns>
    public bool IsDown(ShapeGamepadButton button, 
        float axisDeadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultJoyAxisThreshold, 
        float triggerDeadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultTriggerAxisThreshold, 
        ModifierKeySet? modifierKeySet = null)
    {
        return GetValue(button, axisDeadzone, triggerDeadzone, modifierKeySet) != 0f;
    }

    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified gamepad button,
    /// with separate axis and trigger deadzone values.
    /// </summary>
    /// <param name="button">The gamepad button.</param>
    /// <param name="axisDeadzone">Deadzone for axis input.</param>
    /// <param name="triggerDeadzone">Deadzone for trigger input.</param>
    /// <param name="modifierKeySet">Optional modifier key set.</param>
    /// <returns>The created <see cref="InputState"/>.</returns>
    public InputState CreateInputState(ShapeGamepadButton button, 
        float axisDeadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultJoyAxisThreshold, 
        float triggerDeadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultTriggerAxisThreshold, 
        ModifierKeySet? modifierKeySet = null)
    {
        bool down = IsDown(button, axisDeadzone, triggerDeadzone, modifierKeySet);
        return new(down, !down, down ? 1f : 0f, Index, InputDeviceType.Gamepad);
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified gamepad button,
    /// using a previous state and separate axis/trigger deadzone values.
    /// </summary>
    /// <param name="button">The gamepad button.</param>
    /// <param name="previousState">The previous input state.</param>
    /// <param name="axisDeadzone">Deadzone for axis input.</param>
    /// <param name="triggerDeadzone">Deadzone for trigger input.</param>
    /// <param name="modifierKeySet">Optional modifier key set.</param>
    /// <returns>The created <see cref="InputState"/>.</returns>
    public InputState CreateInputState(ShapeGamepadButton button, InputState previousState, 
        float axisDeadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultJoyAxisThreshold, 
        float triggerDeadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultTriggerAxisThreshold, 
        ModifierKeySet? modifierKeySet = null)
    {
        return new(previousState, CreateInputState(button, axisDeadzone, triggerDeadzone, modifierKeySet));
    }
    #endregion
    
    #region Joy Axis

    /// <summary>
    /// Gets the value of the specified gamepad joystick axis.
    /// Applies deadzone and optional modifier key set.
    /// Returns the calibrated and normalized axis value.
    /// </summary>
    /// <param name="axis">The joystick axis to query.</param>
    /// <param name="deadzone">Deadzone value for axis input. </param>
    /// <param name="inverted">Whether to invert the axis value. Default is false.</param>
    /// <param name="modifierKeySet">Optional modifier key set to check if active.</param>
    /// <returns>The value of the joystick axis as a float.</returns>
    public float GetValue(ShapeGamepadJoyAxis axis, 
        float deadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultJoyAxisThreshold, bool inverted = false,  ModifierKeySet? modifierKeySet = null)
    {
        if (Index < 0 || isLocked || !Connected) return 0f;
        if(modifierKeySet != null && !modifierKeySet.IsActive(this)) return 0f;
        float value = Raylib.GetGamepadAxisMovement(Index, (GamepadAxis)axis);
        if(MathF.Abs(value) < deadzone) return 0f;
        
        var calibrationValue = joyAxisZeroCalibration[axis];
        value -= calibrationValue;
        value = CalibrateAxis(value, axis);
        if(inverted) value *= -1f;
        return value;
    }

    /// <summary>
    /// Determines if the specified joystick axis is "down", i.e., its value exceeds the given deadzone.
    /// Optionally checks if a modifier key set is active.
    /// </summary>
    /// <param name="axis">The joystick axis to check.</param>
    /// <param name="deadzone">The deadzone threshold for the axis. </param>
    /// <param name="inverted">Whether to invert the axis value. Default is false.</param>
    /// <param name="modifierKeySet">Optional modifier key set to check if active.</param>
    /// <returns>True if the axis value is outside the deadzone; otherwise, false.</returns>
    public bool IsDown(ShapeGamepadJoyAxis axis, 
        float deadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultJoyAxisThreshold, bool inverted = false, ModifierKeySet? modifierKeySet = null)
    {
        return GetValue(axis, deadzone, inverted, modifierKeySet) != 0f;
    }

    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified gamepad joystick axis,
    /// applying the given deadzone and optional modifier key set.
    /// </summary>
    /// <param name="axis">The joystick axis to create the input state for.</param>
    /// <param name="deadzone">The deadzone threshold for the axis. </param>
    /// <param name="inverted">Whether to invert the axis value. Default is false.</param>
    /// <param name="modifierKeySet">Optional modifier key set to check if active.</param>
    /// <returns>The created <see cref="InputState"/> for the joystick axis.</returns>
    public InputState CreateInputState(ShapeGamepadJoyAxis axis, 
        float deadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultJoyAxisThreshold, bool inverted = false, ModifierKeySet? modifierKeySet = null)
    {
        float axisValue = GetValue(axis, deadzone, inverted, modifierKeySet);
        bool down = axisValue != 0f;
        return new(down, !down, axisValue, Index, InputDeviceType.Gamepad);
    }
    
    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified gamepad joystick axis,
    /// using a previous state and applying the given deadzone and optional modifier key set.
    /// </summary>
    /// <param name="axis">The joystick axis to create the input state for.</param>
    /// <param name="previousState">The previous input state.</param>
    /// <param name="deadzone">The deadzone threshold for the axis. </param>
    /// <param name="inverted">Whether to invert the axis value. Default is false.</param>
    /// <param name="modifierKeySet">Optional modifier key set to check if active.</param>
    /// <returns>The created <see cref="InputState"/> for the joystick axis.</returns>
    public InputState CreateInputState(ShapeGamepadJoyAxis axis, InputState previousState, 
        float deadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultJoyAxisThreshold, bool inverted = false, ModifierKeySet? modifierKeySet = null)
    {
        return new(previousState, CreateInputState(axis, deadzone, inverted, modifierKeySet));
    }
    
    
    /// <summary>
    /// Calibrates and normalizes the axis value based on its range.
    /// </summary>
    private float CalibrateAxis(float value, ShapeGamepadJoyAxis axis)
    {
        if (joyAxisRanges.TryGetValue(axis, out var range))
        {
            joyAxisRanges[axis] = range.UpdateRange(value);
        }
        else
        {
            joyAxisRanges[axis] = new(value, value);
        }
        
        return value;
    }
    
    #endregion

    #region Trigger Axis
    /// <summary>
    /// Gets the value of the specified gamepad trigger axis.
    /// Applies the given deadzone and optionally checks if a modifier key set is active.
    /// Returns the calibrated and normalized trigger axis value.
    /// </summary>
    /// <param name="axis">The trigger axis to query.</param>
    /// <param name="deadzone">Deadzone value for trigger input. </param>
    /// <param name="modifierKeySet">Optional modifier key set to check if active.</param>
    /// <returns>The value of the trigger axis as a float.</returns>
    public float GetValue(ShapeGamepadTriggerAxis axis, 
        float deadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultTriggerAxisThreshold, ModifierKeySet? modifierKeySet = null)
    {
        if (Index < 0 || isLocked || !Connected) return 0f;
        if(modifierKeySet != null &&  !modifierKeySet.IsActive(this)) return 0f;
        float value = Raylib.GetGamepadAxisMovement(Index, (GamepadAxis)axis);
        if(MathF.Abs(value) < deadzone) return 0f;
        
        var calibrationValue = triggerZeroCalibration[axis];
        value -= calibrationValue;
        value = CalibrateAxis(value, axis);
        return value;
    }
    /// <summary>
    /// Determines if the specified trigger axis is "down", i.e., its value exceeds the given deadzone.
    /// Optionally checks if a modifier key set is active.
    /// </summary>
    /// <param name="axis">The trigger axis to check.</param>
    /// <param name="deadzone">The deadzone threshold for the trigger axis. </param>
    /// <param name="modifierKeySet">Optional modifier key set to check if active.</param>
    /// <returns>True if the trigger axis value is outside the deadzone; otherwise, false.</returns>
    public bool IsDown(ShapeGamepadTriggerAxis axis, 
        float deadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultTriggerAxisThreshold, ModifierKeySet? modifierKeySet = null)
    {
        return GetValue(axis, deadzone, modifierKeySet) != 0f;
    }

    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified gamepad trigger axis,
    /// applying the given deadzone and optional modifier key set.
    /// </summary>
    /// <param name="axis">The trigger axis to create the input state for.</param>
    /// <param name="deadzone">The deadzone threshold for the trigger axis. </param>
    /// <param name="modifierKeySet">Optional modifier key set to check if active.</param>
    /// <returns>The created <see cref="InputState"/> for the trigger axis.</returns>
    public InputState CreateInputState(ShapeGamepadTriggerAxis axis, 
        float deadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultTriggerAxisThreshold, ModifierKeySet? modifierKeySet = null)
    {
        float axisValue = GetValue(axis, deadzone, modifierKeySet);
        bool down = axisValue != 0f;
        return new(down, !down, axisValue, Index, InputDeviceType.Gamepad);
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified gamepad trigger axis,
    /// using a previous state and applying the given deadzone and optional modifier key set.
    /// </summary>
    /// <param name="axis">The trigger axis to create the input state for.</param>
    /// <param name="previousState">The previous input state.</param>
    /// <param name="deadzone">The deadzone threshold for the trigger axis. </param>
    /// <param name="modifierKeySet">Optional modifier key set to check if active.</param>
    /// <returns>The created <see cref="InputState"/> for the trigger axis.</returns>
    public InputState CreateInputState(ShapeGamepadTriggerAxis axis, InputState previousState, 
        float deadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultTriggerAxisThreshold, ModifierKeySet? modifierKeySet = null)
    {
        return new(previousState, CreateInputState(axis, deadzone, modifierKeySet));
    }
    
    private float CalibrateAxis(float value, ShapeGamepadTriggerAxis axis)
    {
        if (triggerAxisRanges.TryGetValue(axis, out var range))
        {
            triggerAxisRanges[axis] = range.UpdateRange(value);
        }
        else
        {
            triggerAxisRanges[axis] = new(value, value);
        }
        
        var axisRange = triggerAxisRanges[axis];
        if (axisRange.TotalRange > 1)
        {
            value /= axisRange.TotalRange;
        }
        
        return value;
    }
    
    #endregion
    
    #region Button Axis
    /// <summary>
    /// Gets the combined value of two gamepad buttons as a single axis.
    /// The negative button decreases the value, the positive button increases it.
    /// Applies axis and trigger deadzones, and optionally checks a modifier key set.
    /// Returns a float in the range \[-1, 1\].
    /// </summary>
    /// <param name="neg">The button representing the negative direction.</param>
    /// <param name="pos">The button representing the positive direction.</param>
    /// <param name="axisDeadzone">Deadzone for axis input. </param>
    /// <param name="triggerDeadzone">Deadzone for trigger input. </param>
    /// <param name="modifierKeySet">Optional modifier key set to check if active.</param>
    /// <returns>The axis value as a float.</returns>
    public float GetValue(ShapeGamepadButton neg, ShapeGamepadButton pos, 
        float axisDeadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultJoyAxisThreshold, 
        float triggerDeadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultTriggerAxisThreshold, 
        ModifierKeySet? modifierKeySet = null)
    {
        if (Index < 0 || isLocked || !Connected) return 0f;
        if(modifierKeySet != null && !modifierKeySet.IsActive(this)) return 0f;
        float vNegative = GetValue(neg, axisDeadzone, triggerDeadzone);
        float vPositive = GetValue(pos, axisDeadzone, triggerDeadzone);
        return vPositive - vNegative;
    }
    /// <summary>
    /// Determines if the combined axis (from two buttons) is "down".
    /// Returns true if either button is pressed enough to exceed the deadzone thresholds,
    /// considering an optional modifier key set.
    /// </summary>
    /// <param name="neg">The button representing the negative direction.</param>
    /// <param name="pos">The button representing the positive direction.</param>
    /// <param name="axisDeadzone">Deadzone for axis input. </param>
    /// <param name="triggerDeadzone">Deadzone for trigger input. </param>
    /// <param name="modifierKeySet">Optional modifier key set to check if active.</param>
    /// <returns>True if the axis is down; otherwise, false.</returns>
    public bool IsDown(ShapeGamepadButton neg, ShapeGamepadButton pos, 
        float axisDeadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultJoyAxisThreshold, 
        float triggerDeadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultTriggerAxisThreshold, 
        ModifierKeySet? modifierKeySet = null)
    {
        return GetValue(neg, pos, axisDeadzone, triggerDeadzone, modifierKeySet) != 0f;
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for a button axis composed of a negative and positive button.
    /// Applies axis and trigger deadzones, and optionally checks a modifier key set.
    /// Returns an <see cref="InputState"/> representing the combined axis value.
    /// </summary>
    /// <param name="neg">The button representing the negative direction.</param>
    /// <param name="pos">The button representing the positive direction.</param>
    /// <param name="axisDeadzone">Deadzone for axis input. </param>
    /// <param name="triggerDeadzone">Deadzone for trigger input. </param>
    /// <param name="modifierKeySet">Optional modifier key set to check if active.</param>
    /// <returns>The created <see cref="InputState"/> for the button axis.</returns>
    public InputState CreateInputState(ShapeGamepadButton neg, ShapeGamepadButton pos, 
        float axisDeadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultJoyAxisThreshold, 
        float triggerDeadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultTriggerAxisThreshold, 
        ModifierKeySet? modifierKeySet = null)
    {
        float axis = GetValue(neg, pos, axisDeadzone, triggerDeadzone, modifierKeySet);
        bool down = axis != 0f;
        return new(down, !down, axis, Index, InputDeviceType.Gamepad);
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for a button axis composed of a negative and positive button,
    /// using a previous state and applying axis/trigger deadzones and an optional modifier key set.
    /// </summary>
    /// <param name="neg">The button representing the negative direction.</param>
    /// <param name="pos">The button representing the positive direction.</param>
    /// <param name="previousState">The previous input state.</param>
    /// <param name="axisDeadzone">Deadzone for axis input. </param>
    /// <param name="triggerDeadzone">Deadzone for trigger input. </param>
    /// <param name="modifierKeySet">Optional modifier key set to check if active.</param>
    /// <returns>The created <see cref="InputState"/> for the button axis.</returns>
    public InputState CreateInputState(ShapeGamepadButton neg, ShapeGamepadButton pos, InputState previousState, 
        float axisDeadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultJoyAxisThreshold, 
        float triggerDeadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultTriggerAxisThreshold, 
        ModifierKeySet? modifierKeySet = null)
    {
        return new(previousState, CreateInputState(neg, pos, axisDeadzone, triggerDeadzone, modifierKeySet));
    }
    #endregion

}