using System.Numerics;
using System.Text;
using Raylib_cs;
using ShapeEngine.Core;

namespace ShapeEngine.Input;


/// <summary>
/// Represents a mouse input device, providing access to mouse buttons, axes, and wheel axes,
/// as well as state tracking and utility methods for mouse input.
/// </summary>
public sealed class MouseDevice : InputDevice
{ 
    /// <summary>
    /// All available Raylib mouse buttons.
    /// </summary>
    public static readonly MouseButton[] AllMouseButtons = Enum.GetValues<MouseButton>();
    /// <summary>
    /// All available Shape mouse buttons.
    /// </summary>
    public static readonly ShapeMouseButton[] AllShapeMouseButtons = Enum.GetValues<ShapeMouseButton>();

    /// <summary>
    /// Gets the usage detection settings for the mouse input device.
    /// </summary>
    public InputDeviceUsageDetectionSettings.MouseSettings UsageDetectionSettings { get; private set; } = new();
    
    /// <summary>
    /// <para>
    /// The process priority of this mouse device instance.
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
    
    private bool wasUsed;
    private bool wasUsedRaw;
    private bool isLocked;
    private bool isAttached;
    private int pressedCount;
    private float pressedCountDurationTimer;
    private float usedDurationTimer;

    /// <summary>
    /// Gets the current mouse position in screen coordinates.
    /// </summary>
    public Vector2 MousePosition { get; private set; }
    /// <summary>
    /// Gets the raw mouse wheel movement vector since the last frame.
    /// </summary>
    public Vector2 MouseWheelV { get; private set; }
    
    /// <summary>
    /// Gets the mouse wheel movement vector with smoothing applied since the last frame.
    /// </summary>
    public Vector2 SmoothedMouseWheelV { get; private set; }
    
    /// <summary>
    /// Gets the raw mouse movement delta since the last frame.
    /// </summary>
    public Vector2 MouseDelta { get; private set; }
    
    /// <summary>
    /// Gets the mouse movement delta with the move threshold from <see cref="UsageDetectionSettings"/> applied.
    /// </summary>
    public Vector2 SmoothedMouseDelta { get; private set; }

    private readonly Dictionary<ShapeMouseButton, InputState> buttonStates = new(AllShapeMouseButtons.Length);
    private readonly Dictionary<ShapeMouseAxis, InputState> axisStates = new(2);
    private readonly Dictionary<ShapeMouseWheelAxis, InputState> wheelAxisStates = new(2);

    /// <summary>
    /// List of mouse buttons pressed since the last update.
    /// </summary>
    public readonly List<ShapeMouseButton> PressedButtons = [];
    
    
    
    /// <summary>
    /// List of mouse buttons released since the last update.
    /// </summary>
    public readonly List<ShapeMouseButton> ReleasedButtons = [];
    
    
    /// <summary>
    /// List of mouse buttons currently held down.
    /// </summary>
    public readonly List<ShapeMouseButton> HeldDownButtons = [];
    
    /// <summary>
    /// Event triggered when a mouse button is pressed.
    /// </summary>
    public event Action<ShapeMouseButton>? OnButtonPressed;
    /// <summary>
    /// Event triggered when a mouse button is released.
    /// </summary>
    public event Action<ShapeMouseButton>? OnButtonReleased;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MouseDevice"/> class.
    /// </summary>
    internal MouseDevice()
    {
        foreach (var button in AllShapeMouseButtons)
        {
            buttonStates.Add(button, new());
        }
        
        axisStates.Add(ShapeMouseAxis.HORIZONTAL, new());
        axisStates.Add(ShapeMouseAxis.VERTICAL, new());
        
        wheelAxisStates.Add(ShapeMouseWheelAxis.HORIZONTAL, new());
        wheelAxisStates.Add(ShapeMouseWheelAxis.VERTICAL, new());
        
        MousePosition = Raylib.GetMousePosition();
    }
    
    /// <inheritdoc cref="InputDevice.GetDeviceProcessPriority"/>
    public override uint GetDeviceProcessPriority() => DeviceProcessPriority;

    /// <summary>
    /// Gets the type of this input device, which is always <see cref="InputDeviceType.Mouse"/>.
    /// </summary>
    public override InputDeviceType GetDeviceType() => InputDeviceType.Mouse;
    
    /// <summary>
    /// Gets the current input state for the specified mouse button.
    /// </summary>
    public InputState GetButtonState(ShapeMouseButton button) => buttonStates[button];
    /// <summary>
    /// Gets the current input state for the specified mouse axis.
    /// </summary>
    public InputState GetAxisState(ShapeMouseAxis axis) => axisStates[axis];
    /// <summary>
    /// Gets the current input state for the specified mouse wheel axis.
    /// </summary>
    public InputState GetWheelAxisState(ShapeMouseWheelAxis axis) => wheelAxisStates[axis];
    
    /// <summary>
    /// Consumes the input state for the specified mouse button.
    /// Marks the state as consumed and updates the button state.
    /// Returns the consumed state, or null if already consumed.
    /// </summary>
    /// <param name="button">The mouse button to consume.</param>
    /// <param name="valid">True if the state was not already consumed; otherwise, false.</param>
    /// <returns>The consumed <see cref="InputState"/> or null if already consumed.</returns>
    public InputState ConsumeButtonState(ShapeMouseButton button, out bool valid)
    {
        valid = false;
        var state = buttonStates[button];
        if (state.Consumed) return state;

        valid = true;
        buttonStates[button] = state.Consume();
        return state;
    }
    
    /// <summary>
    /// Consumes the input state for the specified mouse axis.
    /// Marks the state as consumed and updates the axis state.
    /// Returns the consumed state, or null if already consumed.
    /// </summary>
    /// <param name="axis">The mouse axis to consume.</param>
    /// <param name="valid">True if the state was not already consumed; otherwise, false.</param>
    /// <returns>The consumed <see cref="InputState"/> or null if already consumed.</returns>
    public InputState ConsumeAxisState(ShapeMouseAxis axis, out bool valid)
    {
        valid = false;
        var state = axisStates[axis];
        if (state.Consumed) return state;

        valid = true;
        axisStates[axis] = state.Consume();
        return state;
    }
    
    /// <summary>
    /// Consumes the input state for the specified mouse wheel axis.
    /// Marks the state as consumed and updates the wheel axis state.
    /// Returns the consumed state, or null if already consumed.
    /// </summary>
    /// <param name="axis">The mouse wheel axis to consume.</param>
    /// <param name="valid">True if the state was not already consumed; otherwise, false.</param>
    /// <returns>The consumed <see cref="InputState"/> or null if already consumed.</returns>
    public InputState ConsumeWheelAxisState(ShapeMouseWheelAxis axis, out bool valid)
    {
        valid = false;
        var state = wheelAxisStates[axis];
        if (state.Consumed) return state;

        valid = true;
        wheelAxisStates[axis] = state.Consume();
        return state;
    }
    
    /// <inheritdoc cref="InputDevice.ApplyInputDeviceChangeSettings"/>
    public override void ApplyInputDeviceChangeSettings(InputDeviceUsageDetectionSettings settings) => UsageDetectionSettings = settings.Mouse;

    /// <inheritdoc cref="InputDevice.WasUsed"/>
    public override bool WasUsed() => wasUsed;
    
    /// <inheritdoc cref="InputDevice.WasUsedRaw"/>
    public override bool WasUsedRaw() => wasUsedRaw;
    
    /// <summary>
    /// Returns whether the mouse device is currently locked.
    /// Locking can be used to temporarily disable input processing.
    /// This does not affect whether the device is active or not.
    /// </summary>
    public override bool IsLocked() => isLocked;

    /// <summary>
    /// Locks the mouse device, preventing input from being registered.
    /// </summary>
    public override void Lock()
    {
        if(isLocked) return;
        isLocked = true;
        
        ResetState();
    }

    /// <summary>
    /// Unlocks the mouse device, allowing input to be registered.
    /// </summary>
    public override void Unlock()
    {
        if(!isLocked) return;
        isLocked = false;
    }
    
    /// <summary>
    /// Indicates whether the device is currently attached.
    /// </summary>
    public override bool IsAttached() => isAttached;
    
    /// <summary>
    /// Attaches the device, marking it as attached.
    /// </summary>
    internal override void Attach()
    {
        if (isAttached) return;
        isAttached = true;
    }
    
    /// <summary>
    /// Detaches the  device, marking it as detached and resetting its state.
    /// </summary>
    internal override void Detach()
    {
        if (!isAttached) return;
        isAttached = false;
        
        ResetState();
    }
    

    /// <inheritdoc cref="InputDevice.Update"/>
    public override bool Update(float dt, bool wasOtherDeviceUsed)
    {
        if (!isAttached) return false;
        MousePosition = Raylib.GetMousePosition();
        
        MouseDelta = Raylib.GetMouseDelta();
        float moveThreshold = UsageDetectionSettings.MoveThreshold;
        float smoothedX = MathF.Abs(MouseDelta.X) < moveThreshold ? 0f : MouseDelta.X;
        float smoothedY = MathF.Abs(MouseDelta.Y) < moveThreshold ? 0f : MouseDelta.Y;
        SmoothedMouseDelta = new(smoothedX, smoothedY);
        
        MouseWheelV = Raylib.GetMouseWheelMoveV();
        float wheelThreshold = UsageDetectionSettings.WheelThreshold;
        float smoothedWheelX = MathF.Abs(MouseWheelV.X) < wheelThreshold ? 0f : MouseWheelV.X;
        float smoothedWheelY = MathF.Abs(MouseWheelV.Y) < wheelThreshold ? 0f : MouseWheelV.Y;
        SmoothedMouseWheelV = new(smoothedWheelX, smoothedWheelY);
        
        PressedButtons.Clear();
        HeldDownButtons.Clear();
        ReleasedButtons.Clear();
        UpdateStates();

        if (isLocked)
        {
            wasUsed = false;
            wasUsedRaw = false;
            return false;
        }
        
        WasMouseUsed(dt, wasOtherDeviceUsed, out wasUsed, out wasUsedRaw);
        
        return wasUsed && !wasOtherDeviceUsed;//safety precaution
    }

    private void ResetState()
    {
        usedDurationTimer = 0f;
        pressedCount = 0;
        pressedCountDurationTimer = 0f;
        
        wasUsed = false;
        wasUsedRaw = false;
        
        PressedButtons.Clear();
        HeldDownButtons.Clear();
        ReleasedButtons.Clear();
    }

    private void WasMouseUsed(float dt, bool wasOtherDeviceUsed, out bool used, out bool usedRaw)
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

        if (!UsageDetectionSettings.Detection || wasOtherDeviceUsed)
        {
            return;
        }
            
        if (UsageDetectionSettings.SelectionButtons is { Count: > 0 })
        {
            if (UsageDetectionSettings.ExceptionButtons is { Count: > 0 })
            {
                foreach (var button in UsageDetectionSettings.SelectionButtons)
                {
                    if (UsageDetectionSettings.ExceptionButtons.Contains(button)) continue;
                    if (IsDown(button, UsageDetectionSettings.MoveThreshold, UsageDetectionSettings.WheelThreshold))
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
                    if (IsDown(button, UsageDetectionSettings.MoveThreshold, UsageDetectionSettings.WheelThreshold))
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
                if(HeldDownButtons.Any(b => !UsageDetectionSettings.ExceptionButtons.Contains(b)))
                // if(HeldDownButtons.Count > 0)
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
                else if(usedDurationTimer > 0f) usedDurationTimer = 0f;
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
    /// Updates the states of all mouse buttons, axes, and wheel axes.
    /// </summary>
    private void UpdateStates()
    {
        foreach (var state in buttonStates)
        {
            var button = state.Key;
            var prevState = state.Value;
            var curState = CreateInputState(button, UsageDetectionSettings.MoveThreshold, UsageDetectionSettings.WheelThreshold);
            var nextState = new InputState(prevState, curState);
            buttonStates[button] = nextState;

            if(nextState.Down) HeldDownButtons.Add(button);
            if (nextState.Pressed)
            {
                PressedButtons.Add(button);
                OnButtonPressed?.Invoke(button);
            }
            else if (nextState.Released)
            {
                ReleasedButtons.Add(button);
                OnButtonReleased?.Invoke(button);
            }
        }
        foreach (var state in axisStates)
        {
            var axis = state.Key;
            var prevState = state.Value;
            var curState = CreateInputState(axis, UsageDetectionSettings.MoveThreshold);
            axisStates[axis] = new InputState(prevState, curState);
        }
        foreach (var state in wheelAxisStates)
        {
            var axis = state.Key;
            var prevState = state.Value;
            var curState = CreateInputState(axis, UsageDetectionSettings.WheelThreshold);
            wheelAxisStates[axis] = new InputState(prevState, curState);
        }
    }
    
    #region Axis
    /// <summary>
    /// Gets the value of the specified mouse axis (horizontal or vertical) based on the current mouse delta,
    /// applying a deadzone threshold and optional modifier key set.
    /// Returns 0 if the device is locked, the mouse is not on screen, or the modifier key set is not active.
    /// </summary>
    /// <param name="axis">The mouse axis to query (horizontal or vertical).</param>
    /// <param name="deadzone">The minimum movement required to register a value (default is the configured move threshold).</param>
    /// <param name="modifierKeySet">Optional modifier key set that must be active for the value to be returned.</param>
    /// <returns>The axis value, or 0 if below the deadzone or input is not valid.</returns>
    public float GetValue(ShapeMouseAxis axis, float deadzone = InputDeviceUsageDetectionSettings.MouseSettings.DefaultMouseMoveThreshold, ModifierKeySet? modifierKeySet = null)
    {
        if (isLocked) return 0f;
        if (!GameWindow.Instance.MouseOnScreen) return 0f;
        if (modifierKeySet != null && !modifierKeySet.IsActive()) return 0f;
        
        var value = MouseDelta;
        float returnValue = axis == ShapeMouseAxis.VERTICAL ? value.Y : value.X;
        return MathF.Abs(returnValue) < deadzone ? 0f : returnValue;
    }
    /// <summary>
    /// Gets the value of the specified mouse axis (horizontal or vertical) based on the difference between the current mouse position and a target position,
    /// applying a threshold and optional modifier key set.
    /// Returns 0 if the device is locked, the mouse is not on screen, or the modifier key set is not active.
    /// </summary>
    /// <param name="axis">The mouse axis to query (horizontal or vertical).</param>
    /// <param name="targetPosition">The position to compare the current mouse position against.</param>
    /// <param name="threshold">The minimum movement required to register a value.</param>
    /// <param name="modifierKeySet">Optional modifier key set that must be active for the value to be returned.</param>
    /// <returns>The axis value, or 0 if below the threshold or input is not valid.</returns>
    public float GetValue(ShapeMouseAxis axis, Vector2 targetPosition, float threshold, ModifierKeySet? modifierKeySet = null)
    {
        if (isLocked) return 0f;
        if (!GameWindow.Instance.MouseOnScreen) return 0f;
        if (modifierKeySet != null && !modifierKeySet.IsActive()) return 0f;
        var delta = MousePosition - targetPosition;
        var value = axis == ShapeMouseAxis.VERTICAL ? delta.Y : delta.X;
        return MathF.Abs(value) < threshold ? 0f : value;
    }
    /// <summary>
    /// Determines whether the specified mouse axis is considered "down" (i.e., has moved beyond the given deadzone threshold).
    /// Returns true if the axis value is not zero, otherwise false.
    /// </summary>
    /// <param name="axis">The mouse axis to check (horizontal or vertical).</param>
    /// <param name="deadzone">The minimum movement required to register as "down" (default is the configured move threshold).</param>
    /// <param name="modifierKeySet">Optional modifier key set that must be active for the axis to be considered "down".</param>
    /// <returns>True if the axis is down, otherwise false.</returns>
    public bool IsDown(ShapeMouseAxis axis, float deadzone = InputDeviceUsageDetectionSettings.MouseSettings.DefaultMouseMoveThreshold, ModifierKeySet? modifierKeySet = null)
    {
        return GetValue(axis, deadzone, modifierKeySet) != 0;
    }
    /// <summary>
    /// Determines whether the specified mouse axis is considered "down" (i.e., has moved beyond the given threshold)
    /// relative to a target position. Returns true if the axis value is not zero, otherwise false.
    /// </summary>
    /// <param name="axis">The mouse axis to check (horizontal or vertical).</param>
    /// <param name="targetPosition">The position to compare the current mouse position against.</param>
    /// <param name="threshold">The minimum movement required to register as "down".</param>
    /// <param name="modifierKeySet">Optional modifier key set that must be active for the axis to be considered "down".</param>
    /// <returns>True if the axis is down, otherwise false.</returns>
    public bool IsDown(ShapeMouseAxis axis, Vector2 targetPosition, float threshold, ModifierKeySet? modifierKeySet = null)
    {
        return GetValue(axis, targetPosition, threshold, modifierKeySet) != 0;
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified mouse axis,
    /// using the current axis value, a deadzone threshold, and an optional modifier key set.
    /// </summary>
    /// <param name="axis">The mouse axis to evaluate (horizontal or vertical).</param>
    /// <param name="deadzone">The minimum movement required to register as "down" (default is the configured move threshold).</param>
    /// <param name="modifierKeySet">Optional modifier key set that must be active for the axis to be considered "down".</param>
    /// <returns>
    /// An <see cref="InputState"/> representing the current state of the axis,
    /// including whether it is down, up, and its value.
    /// </returns>
    public InputState CreateInputState(ShapeMouseAxis axis, float deadzone = InputDeviceUsageDetectionSettings.MouseSettings.DefaultMouseMoveThreshold, ModifierKeySet? modifierKeySet = null)
    {
        float axisValue = GetValue(axis, deadzone, modifierKeySet);
        bool down = axisValue != 0f;
        return new(down, !down, axisValue, -1, InputDeviceType.Mouse);
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified mouse axis,
    /// using the difference between the current mouse position and a target position,
    /// a deadzone threshold, and an optional modifier key set.
    /// </summary>
    /// <param name="axis">The mouse axis to evaluate (horizontal or vertical).</param>
    /// <param name="targetPosition">The position to compare the current mouse position against.</param>
    /// <param name="deadzone">The minimum movement required to register as "down" (default is the configured move threshold).</param>
    /// <param name="modifierKeySet">Optional modifier key set that must be active for the axis to be considered "down".</param>
    /// <returns>
    /// An <see cref="InputState"/> representing the current state of the axis,
    /// including whether it is down, up, and its value.
    /// </returns>
    public InputState CreateInputState(ShapeMouseAxis axis, Vector2 targetPosition,  float deadzone = InputDeviceUsageDetectionSettings.MouseSettings.DefaultMouseMoveThreshold, ModifierKeySet? modifierKeySet = null)
    {
        float axisValue = GetValue(axis, targetPosition, deadzone, modifierKeySet);
        bool down = axisValue != 0f;
        return new(down, !down, axisValue, -1, InputDeviceType.Mouse);
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified mouse axis,
    /// using the difference between the current mouse position and a target position,
    /// a deadzone threshold, an optional modifier key set, and the previous input state.
    /// </summary>
    /// <param name="axis">The mouse axis to evaluate (horizontal or vertical).</param>
    /// <param name="targetPosition">The position to compare the current mouse position against.</param>
    /// <param name="previousState">The previous input state to base the new state on.</param>
    /// <param name="deadzone">The minimum movement required to register as "down" (default is the configured move threshold).</param>
    /// <param name="modifierKeySet">Optional modifier key set that must be active for the axis to be considered "down".</param>
    /// <returns>
    /// An <see cref="InputState"/> representing the current state of the axis,
    /// including whether it is down, up, and its value, based on the previous state.
    /// </returns>
    public InputState CreateInputState(ShapeMouseAxis axis, Vector2 targetPosition,  InputState previousState, float deadzone = InputDeviceUsageDetectionSettings.MouseSettings.DefaultMouseMoveThreshold, ModifierKeySet? modifierKeySet = null)
    {
        return new(previousState, CreateInputState(axis, targetPosition, deadzone, modifierKeySet));
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified mouse axis,
    /// using the current axis value, a deadzone threshold, an optional modifier key set,
    /// and the previous input state to base the new state on.
    /// </summary>
    /// <param name="axis">The mouse axis to evaluate (horizontal or vertical).</param>
    /// <param name="previousState">The previous input state to base the new state on.</param>
    /// <param name="deadzone">The minimum movement required to register as "down" (default is the configured move threshold).</param>
    /// <param name="modifierKeySet">Optional modifier key set that must be active for the axis to be considered "down".</param>
    /// <returns>
    /// An <see cref="InputState"/> representing the current state of the axis,
    /// including whether it is down, up, and its value, based on the previous state.
    /// </returns>
    public InputState CreateInputState(ShapeMouseAxis axis, InputState previousState, float deadzone = InputDeviceUsageDetectionSettings.MouseSettings.DefaultMouseMoveThreshold, ModifierKeySet? modifierKeySet = null)
    {
        return new(previousState, CreateInputState(axis, deadzone, modifierKeySet));
    }
    
    #endregion

    #region Wheel Axis

    /// <summary>
    /// Gets the value of the specified mouse wheel axis (horizontal or vertical) based on the current mouse wheel movement,
    /// applying a deadzone threshold and optional modifier key set.
    /// Returns 0 if the device is locked, the mouse is not on screen, or the modifier key set is not active.
    /// </summary>
    /// <param name="axis">The mouse wheel axis to query (horizontal or vertical).</param>
    /// <param name="deadzone">The minimum movement required to register a value (default is the configured wheel threshold).</param>
    /// <param name="modifierKeySet">Optional modifier key set that must be active for the value to be returned.</param>
    /// <returns>The axis value, or 0 if below the deadzone or input is not valid.</returns>
    public float GetValue(ShapeMouseWheelAxis axis, float deadzone = InputDeviceUsageDetectionSettings.MouseSettings.DefaultMouseWheelThreshold, ModifierKeySet? modifierKeySet = null)
    {
        if (isLocked) return 0f;
        if (!GameWindow.Instance.MouseOnScreen) return 0f;
        if (modifierKeySet != null && !modifierKeySet.IsActive()) return 0f;
        var value = MouseWheelV;
        float returnValue = axis == ShapeMouseWheelAxis.VERTICAL ? value.Y : value.X;
        return MathF.Abs(returnValue) < deadzone ? 0f : returnValue;
    }

    /// <summary>
    /// Determines whether the specified mouse wheel axis is considered "down" (i.e., has moved beyond the given deadzone threshold).
    /// Returns true if the axis value is not zero, otherwise false.
    /// </summary>
    /// <param name="axis">The mouse wheel axis to check (horizontal or vertical).</param>
    /// <param name="deadzone">The minimum movement required to register as "down" (default is the configured wheel threshold).</param>
    /// <param name="modifierKeySet">Optional modifier key set that must be active for the axis to be considered "down".</param>
    /// <returns>True if the axis is down, otherwise false.</returns>
    public bool IsDown(ShapeMouseWheelAxis axis, float deadzone = InputDeviceUsageDetectionSettings.MouseSettings.DefaultMouseWheelThreshold, ModifierKeySet? modifierKeySet = null)
    {
        return GetValue(axis, deadzone, modifierKeySet) != 0f;
    }

    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified mouse wheel axis,
    /// using the current axis value, a deadzone threshold, and an optional modifier key set.
    /// </summary>
    /// <param name="axis">The mouse wheel axis to evaluate (horizontal or vertical).</param>
    /// <param name="deadzone">The minimum movement required to register as "down" (default is the configured wheel threshold).</param>
    /// <param name="modifierKeySet">Optional modifier key set that must be active for the axis to be considered "down".</param>
    /// <returns>
    /// An <see cref="InputState"/> representing the current state of the wheel axis,
    /// including whether it is down, up, and its value.
    /// </returns>
    public InputState CreateInputState(ShapeMouseWheelAxis axis, float deadzone = InputDeviceUsageDetectionSettings.MouseSettings.DefaultMouseWheelThreshold, ModifierKeySet? modifierKeySet = null)
    {
        float axisValue = GetValue(axis, deadzone, modifierKeySet);
        bool down = axisValue != 0f;
        return new(down, !down, axisValue, -1, InputDeviceType.Mouse);
    }

    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified mouse wheel axis,
    /// using the current axis value, a deadzone threshold, an optional modifier key set,
    /// and the previous input state to base the new state on.
    /// </summary>
    /// <param name="axis">The mouse wheel axis to evaluate (horizontal or vertical).</param>
    /// <param name="previousState">The previous input state to base the new state on.</param>
    /// <param name="deadzone">The minimum movement required to register as "down" (default is the configured wheel threshold).</param>
    /// <param name="modifierKeySet">Optional modifier key set that must be active for the axis to be considered "down".</param>
    /// <returns>
    /// An <see cref="InputState"/> representing the current state of the wheel axis,
    /// including whether it is down, up, and its value, based on the previous state.
    /// </returns>
    public InputState CreateInputState(ShapeMouseWheelAxis axis, InputState previousState, float deadzone = InputDeviceUsageDetectionSettings.MouseSettings.DefaultMouseWheelThreshold, ModifierKeySet? modifierKeySet = null)
    {
        return new(previousState, CreateInputState(axis, deadzone, modifierKeySet));
    }
    #endregion
    
    #region Button
    
    /// <summary>
    /// Gets the value of the specified mouse button, mouse wheel, or mouse axis button.
    /// Returns 1 for pressed buttons, or the axis/wheel value if applicable, otherwise 0.
    /// Returns 0 if the device is locked, the mouse is not on screen, or the modifier key set is not active.
    /// </summary>
    /// <param name="button">The mouse button to query.</param>
    /// <param name="mouseMoveDeadzone">The minimum movement required to register a value for axis buttons (default is the configured move threshold).</param>
    /// <param name="mouseWheelDeadzone">The minimum movement required to register a value for wheel buttons (default is the configured wheel threshold).</param>
    /// <param name="modifierKeySet">Optional modifier key set that must be active for the value to be returned.</param>
    /// <returns>The button value, axis/wheel value, or 0 if not active or below threshold.</returns>
    public float GetValue(
        ShapeMouseButton button, 
        float mouseMoveDeadzone = InputDeviceUsageDetectionSettings.MouseSettings.DefaultMouseMoveThreshold, 
        float mouseWheelDeadzone = InputDeviceUsageDetectionSettings.MouseSettings.DefaultMouseMoveThreshold,
        ModifierKeySet? modifierKeySet = null)
    {
        if (isLocked) return 0f;
        if (!GameWindow.Instance.MouseOnScreen) return 0f;
        if (modifierKeySet != null && !modifierKeySet.IsActive()) return 0f;
        int id = (int)button;
        if (id is >= 10 and < 20)
        {
            var value = MouseWheelV; // Raylib.GetMouseWheelMoveV();
            if (button == ShapeMouseButton.MW_LEFT) return value.X < -mouseWheelDeadzone ? MathF.Abs(value.X) : 0f;
            if (button == ShapeMouseButton.MW_RIGHT) return value.X > mouseWheelDeadzone ? value.X : 0f;
            if (button == ShapeMouseButton.MW_UP) return value.Y < -mouseWheelDeadzone ? MathF.Abs(value.Y) : 0f;
            if (button == ShapeMouseButton.MW_DOWN) return value.Y > mouseWheelDeadzone ? value.Y : 0f;
            return 0f;
        }
        if (id >= 20)
        {
            var mouseDelta = MouseDelta;
            if (button == ShapeMouseButton.LEFT_AXIS) return mouseDelta.X < -mouseMoveDeadzone ? MathF.Abs(mouseDelta.X) : 0f;
            if(button == ShapeMouseButton.RIGHT_AXIS) return mouseDelta.X > mouseMoveDeadzone ? mouseDelta.X : 0f;
            if(button == ShapeMouseButton.UP_AXIS) return mouseDelta.Y < -mouseMoveDeadzone ? MathF.Abs(mouseDelta.Y) : 0f;
            if(button == ShapeMouseButton.DOWN_AXIS) return mouseDelta.Y > mouseMoveDeadzone ? mouseDelta.Y : 0f;
            return 0f;
        }
        return Raylib.IsMouseButtonDown((MouseButton)id) ? 1f : 0f;
    }
    
    /// <summary>
    /// Determines whether the specified mouse button is currently considered "down".
    /// Returns true if the button or associated axis/wheel is active, otherwise false.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <param name="mouseMoveDeadzone">The minimum movement required to register as "down" for axis buttons (default is the configured move threshold).</param>
    /// <param name="mouseWheelDeadzone">The minimum movement required to register as "down" for wheel buttons (default is the configured wheel threshold).</param>
    /// <param name="modifierKeySet">Optional modifier key set that must be active for the button to be considered "down".</param>
    /// <returns>True if the button is down, otherwise false.</returns>
    public bool IsDown(ShapeMouseButton button, 
        float mouseMoveDeadzone = InputDeviceUsageDetectionSettings.MouseSettings.DefaultMouseMoveThreshold, 
        float mouseWheelDeadzone = InputDeviceUsageDetectionSettings.MouseSettings.DefaultMouseMoveThreshold,
        ModifierKeySet? modifierKeySet = null)
    {
       return GetValue(button, mouseMoveDeadzone, mouseWheelDeadzone, modifierKeySet) != 0f;
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified mouse button,
    /// using the current button value, mouse move deadzone, mouse wheel deadzone,
    /// and an optional modifier key set.
    /// </summary>
    /// <param name="button">The mouse button to evaluate.</param>
    /// <param name="mouseMoveDeadzone">The minimum movement required to register as "down" for axis buttons (default is the configured move threshold).</param>
    /// <param name="mouseWheelDeadzone">The minimum movement required to register as "down" for wheel buttons (default is the configured wheel threshold).</param>
    /// <param name="modifierKeySet">Optional modifier key set that must be active for the button to be considered "down".</param>
    /// <returns>
    /// An <see cref="InputState"/> representing the current state of the button,
    /// including whether it is down, up, and its value.
    /// </returns>
    public InputState CreateInputState(ShapeMouseButton button, 
        float mouseMoveDeadzone = InputDeviceUsageDetectionSettings.MouseSettings.DefaultMouseMoveThreshold, 
        float mouseWheelDeadzone = InputDeviceUsageDetectionSettings.MouseSettings.DefaultMouseMoveThreshold,
        ModifierKeySet? modifierKeySet = null)
    {
        var value = GetValue(button, mouseMoveDeadzone, mouseWheelDeadzone, modifierKeySet);
        bool down = value != 0f;
        return new(down, !down, down ? 1f : 0f, -1, InputDeviceType.Mouse);
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified mouse button,
    /// using the previous input state, mouse move deadzone, mouse wheel deadzone,
    /// and an optional modifier key set.
    /// </summary>
    /// <param name="button">The mouse button to evaluate.</param>
    /// <param name="previousState">The previous input state to base the new state on.</param>
    /// <param name="mouseMoveDeadzone">The minimum movement required to register as "down" for axis buttons (default is the configured move threshold).</param>
    /// <param name="mouseWheelDeadzone">The minimum movement required to register as "down" for wheel buttons (default is the configured wheel threshold).</param>
    /// <param name="modifierKeySet">Optional modifier key set that must be active for the button to be considered "down".</param>
    /// <returns>
    /// An <see cref="InputState"/> representing the current state of the button,
    /// including whether it is down, up, and its value, based on the previous state.
    /// </returns>
    public InputState CreateInputState(ShapeMouseButton button, InputState previousState, 
        float mouseMoveDeadzone = InputDeviceUsageDetectionSettings.MouseSettings.DefaultMouseMoveThreshold, 
        float mouseWheelDeadzone = InputDeviceUsageDetectionSettings.MouseSettings.DefaultMouseMoveThreshold,
        ModifierKeySet? modifierKeySet = null)
    {
        return new(previousState, CreateInputState(button, mouseMoveDeadzone, mouseWheelDeadzone, modifierKeySet));
    }
    
    #endregion

    #region ButtonAxis
    /// <summary>
    /// Gets the value of a virtual axis defined by two mouse buttons (negative and positive).
    /// Returns the difference between the positive and negative button values.
    /// Returns 0 if the device is locked or the modifier key set is not active.
    /// </summary>
    /// <param name="neg">The button representing the negative direction.</param>
    /// <param name="pos">The button representing the positive direction.</param>
    /// <param name="mouseMoveDeadzone">The minimum movement required to register a value for axis buttons (default is the configured move threshold).</param>
    /// <param name="mouseWheelDeadzone">The minimum movement required to register a value for wheel buttons (default is the configured wheel threshold).</param>
    /// <param name="modifierKeySet">Optional modifier key set that must be active for the value to be returned.</param>
    /// <returns>The axis value, or 0 if not active or below threshold.</returns>
    public float GetValue(ShapeMouseButton neg, ShapeMouseButton pos, 
        float mouseMoveDeadzone = InputDeviceUsageDetectionSettings.MouseSettings.DefaultMouseMoveThreshold, 
        float mouseWheelDeadzone = InputDeviceUsageDetectionSettings.MouseSettings.DefaultMouseMoveThreshold,
        ModifierKeySet? modifierKeySet = null)
   {
       if (isLocked) return 0f;
       if (modifierKeySet != null && !modifierKeySet.IsActive()) return 0f;
       float vNegative = GetValue(neg, mouseMoveDeadzone, mouseWheelDeadzone);
       float vPositive = GetValue(pos, mouseMoveDeadzone, mouseWheelDeadzone);
       return vPositive - vNegative;
   }

    /// <summary>
    /// Determines whether a virtual axis, defined by two mouse buttons (negative and positive), is currently considered "down".
    /// Returns true if either button is active (i.e., the axis value is not zero), otherwise false.
    /// </summary>
    /// <param name="neg">The button representing the negative direction.</param>
    /// <param name="pos">The button representing the positive direction.</param>
    /// <param name="mouseMoveDeadzone">The minimum movement required to register as "down" for axis buttons (default is the configured move threshold).</param>
    /// <param name="mouseWheelDeadzone">The minimum movement required to register as "down" for wheel buttons (default is the configured wheel threshold).</param>
    /// <param name="modifierKeySet">Optional modifier key set that must be active for the axis to be considered "down".</param>
    /// <returns>True if the virtual axis is down, otherwise false.</returns>
    public bool IsDown(ShapeMouseButton neg, ShapeMouseButton pos, 
        float mouseMoveDeadzone = InputDeviceUsageDetectionSettings.MouseSettings.DefaultMouseMoveThreshold, 
        float mouseWheelDeadzone = InputDeviceUsageDetectionSettings.MouseSettings.DefaultMouseMoveThreshold,
        ModifierKeySet? modifierKeySet = null)
    {
        return GetValue(neg, pos, mouseMoveDeadzone, mouseWheelDeadzone, modifierKeySet) != 0f;
    }

    /// <summary>
    /// Creates an <see cref="InputState"/> for a virtual axis defined by two mouse buttons (negative and positive).
    /// Uses the current values of the negative and positive buttons, mouse move deadzone, mouse wheel deadzone,
    /// and an optional modifier key set.
    /// </summary>
    /// <param name="neg">The button representing the negative direction.</param>
    /// <param name="pos">The button representing the positive direction.</param>
    /// <param name="mouseMoveDeadzone">The minimum movement required to register as "down" for axis buttons (default is the configured move threshold).</param>
    /// <param name="mouseWheelDeadzone">The minimum movement required to register as "down" for wheel buttons (default is the configured wheel threshold).</param>
    /// <param name="modifierKeySet">Optional modifier key set that must be active for the axis to be considered "down".</param>
    /// <returns>
    /// An <see cref="InputState"/> representing the current state of the virtual axis,
    /// including whether it is down, up, and its value.
    /// </returns>
    public InputState CreateInputState(ShapeMouseButton neg, ShapeMouseButton pos, 
        float mouseMoveDeadzone = InputDeviceUsageDetectionSettings.MouseSettings.DefaultMouseMoveThreshold, 
        float mouseWheelDeadzone = InputDeviceUsageDetectionSettings.MouseSettings.DefaultMouseMoveThreshold,
        ModifierKeySet? modifierKeySet = null)
    {
        float axis = GetValue(neg, pos, mouseMoveDeadzone, mouseWheelDeadzone, modifierKeySet);
        bool down = axis != 0f;
        return new(down, !down, axis, -1, InputDeviceType.Mouse);
    }

    /// <summary>
    /// Creates an <see cref="InputState"/> for a virtual axis defined by two mouse buttons (negative and positive),
    /// using the previous input state, mouse move deadzone, mouse wheel deadzone, and an optional modifier key set.
    /// </summary>
    /// <param name="neg">The button representing the negative direction.</param>
    /// <param name="pos">The button representing the positive direction.</param>
    /// <param name="previousState">The previous input state to base the new state on.</param>
    /// <param name="mouseMoveDeadzone">The minimum movement required to register as "down" for axis buttons (default is the configured move threshold).</param>
    /// <param name="mouseWheelDeadzone">The minimum movement required to register as "down" for wheel buttons (default is the configured wheel threshold).</param>
    /// <param name="modifierKeySet">Optional modifier key set that must be active for the axis to be considered "down".</param>
    /// <returns>
    /// An <see cref="InputState"/> representing the current state of the virtual axis,
    /// including whether it is down, up, and its value, based on the previous state.
    /// </returns>
    public InputState CreateInputState(ShapeMouseButton neg, ShapeMouseButton pos, InputState previousState, 
        float mouseMoveDeadzone = InputDeviceUsageDetectionSettings.MouseSettings.DefaultMouseMoveThreshold, 
        float mouseWheelDeadzone = InputDeviceUsageDetectionSettings.MouseSettings.DefaultMouseMoveThreshold,
        ModifierKeySet? modifierKeySet = null)
    {
        return new(previousState, CreateInputState(neg, pos, mouseMoveDeadzone, mouseWheelDeadzone, modifierKeySet));
    }
    

    #endregion
}