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
    
    private bool wasUsed;
    private bool wasUsedRaw;
    private bool isLocked;
    private bool isActive;

    private int pressedCount;
    private float pressedCountDurationTimer;
    private float usedDurationTimer;

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
    }

    // private Vector2 lastMousePosition;
    // public Vector2 MouseDelta { get; private set; }

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
    /// <returns>The consumed <see cref="InputState"/> or null if already consumed.</returns>
    public InputState? ConsumeButtonState(ShapeMouseButton button)
    {
        var state = buttonStates[button];
        if (state.Consumed) return null;
    
        state = state.Consume();
        buttonStates[button] = state;
        return state;
    }
    
    /// <summary>
    /// Consumes the input state for the specified mouse axis.
    /// Marks the state as consumed and updates the axis state.
    /// Returns the consumed state, or null if already consumed.
    /// </summary>
    /// <param name="axis">The mouse axis to consume.</param>
    /// <returns>The consumed <see cref="InputState"/> or null if already consumed.</returns>
    public InputState? ConsumeAxisState(ShapeMouseAxis axis)
    {
        var state = axisStates[axis];
        if (state.Consumed) return null;
    
        state = state.Consume();
        axisStates[axis] = state;
        return state;
    }
    
    /// <summary>
    /// Consumes the input state for the specified mouse wheel axis.
    /// Marks the state as consumed and updates the wheel axis state.
    /// Returns the consumed state, or null if already consumed.
    /// </summary>
    /// <param name="axis">The mouse wheel axis to consume.</param>
    /// <returns>The consumed <see cref="InputState"/> or null if already consumed.</returns>
    public InputState? ConsumeWheelAxisState(ShapeMouseWheelAxis axis)
    {
        var state = wheelAxisStates[axis];
        if (state.Consumed) return null;
    
        state = state.Consume();
        wheelAxisStates[axis] = state;
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
    /// </summary>
    public override bool IsLocked() => isLocked;

    /// <summary>
    /// Locks the mouse device, preventing input from being registered.
    /// </summary>
    public override void Lock()
    {
        if(isLocked) return;
        isLocked = true;
        usedDurationTimer = 0f;
        pressedCount = 0;
        pressedCountDurationTimer = 0f;
        
        PressedButtons.Clear();
        HeldDownButtons.Clear();
        ReleasedButtons.Clear();
    }

    /// <summary>
    /// Unlocks the mouse device, allowing input to be registered.
    /// </summary>
    public override void Unlock()
    {
        if(!isLocked) return;
        isLocked = false;
    }

    /// <inheritdoc cref="InputDevice.Update"/>
    public override bool Update(float dt, bool wasOtherDeviceUsed)
    {
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

    /// <summary>
    /// Calibrates the mouse device. (Currently not implemented.)
    /// </summary>
    public override void Calibrate(){ }

    /// <summary>
    /// Indicates whether the mouse device is currently active.
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
            
        if (UsageDetectionSettings.SpecialButtonSelectionSystemEnabled)
        {
            if (
                IsDown(UsageDetectionSettings.SelectionButtonPrimary, UsageDetectionSettings.MoveThreshold, UsageDetectionSettings.WheelThreshold) ||
                IsDown(UsageDetectionSettings.SelectionButtonSecondary, UsageDetectionSettings.MoveThreshold, UsageDetectionSettings.WheelThreshold)
            )
            {
                used = true;
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
                if(HeldDownButtons.Count > 0)
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
                else if(usedDurationTimer > 0f)
                {
                    usedDurationTimer = 0f;
                }
                
               
            }
            
            if (pressCountEnabled && PressedButtons.Count > 0)
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
            var curState = CreateInputState(axis, UsageDetectionSettings.MoveThreshold);;
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
    /// Gets the display name for a mouse axis.
    /// </summary>
    /// <param name="axis">The axis.</param>
    /// <param name="shortHand">Whether to use shorthand notation.</param>
    /// <returns>The axis name.</returns>
    public static string GetAxisName(ShapeMouseAxis axis, bool shortHand = true)
    {
        switch (axis)
        {
            case ShapeMouseAxis.HORIZONTAL: return shortHand ? "Mx" : "Mouse Horizontal";
            case ShapeMouseAxis.VERTICAL: return shortHand ? "My" : "Mouse Vertical";
            default: return "No Key";
        }
    }

    /// <summary>
    /// Determines if the specified mouse axis is considered "down" (moved past deadzone).
    /// </summary>
    public bool IsDown(ShapeMouseAxis axis, float deadzone = 0.5f)
    {
        return GetValue(axis, deadzone) != 0f;
    }

    /// <summary>
    /// Determines if the specified mouse axis is "down" with modifier keys.
    /// </summary>
    public bool IsDown(ShapeMouseAxis axis, float deadzone, ModifierKeyOperator modifierOperator,
        params IModifierKey[] modifierKeys)
    {
        return GetValue(axis, deadzone, modifierOperator, modifierKeys) != 0;
    }
    
    /// <summary>
    /// Gets the value of the specified mouse axis, considering deadzone and modifier keys.
    /// </summary>
    public float GetValue(ShapeMouseAxis axis, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        if (isLocked) return 0f;
        if (!GameWindow.Instance.MouseOnScreen) return 0f;
        if (!IModifierKey.IsActive(modifierOperator, modifierKeys)) return 0f;
        return GetValue(axis, deadzone);
    }
    /// <summary>
    /// Gets the value of the specified mouse axis, considering deadzone.
    /// </summary>
    public float GetValue(ShapeMouseAxis axis, float deadzone = 0.5f)
    {
        if (isLocked) return 0f;
        if (!GameWindow.Instance.MouseOnScreen) return 0f;

        var value = MouseDelta; // Raylib.GetMouseDelta();
        float returnValue = axis == ShapeMouseAxis.VERTICAL ? value.Y : value.X;
        if (MathF.Abs(returnValue) < deadzone) return 0f;
        return returnValue;
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified mouse axis.
    /// </summary>
    public InputState CreateInputState(ShapeMouseAxis axis, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        float axisValue = GetValue(axis, deadzone, modifierOperator, modifierKeys);
        bool down = axisValue != 0f;
        return new(down, !down, axisValue, -1, InputDeviceType.Mouse);
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified mouse axis, using a previous state.
    /// </summary>
    public InputState CreateInputState(ShapeMouseAxis axis, InputState previousState, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        return new(previousState, CreateInputState(axis, deadzone, modifierOperator, modifierKeys));
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified mouse axis.
    /// </summary>
    public InputState CreateInputState(ShapeMouseAxis axis, float deadzone = 0.5f)
    {
        float axisValue = GetValue(axis, deadzone);
        bool down = axisValue != 0f;
        return new(down, !down, axisValue, -1, InputDeviceType.Mouse);
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified mouse axis, using a previous state.
    /// </summary>
    public InputState CreateInputState(ShapeMouseAxis axis, InputState previousState, float deadzone = 0.5f)
    {
        return new(previousState, CreateInputState(axis, deadzone));
    }
    #endregion

    #region Wheel Axis

    /// <summary>
    /// Determines if the specified mouse wheel axis is "down" with modifier keys.
    /// </summary>
    public bool IsDown(ShapeMouseWheelAxis axis, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        return GetValue(axis, deadzone, modifierOperator, modifierKeys) != 0f;
    }
    /// <summary>
    /// Determines if the specified mouse wheel axis is "down".
    /// </summary>
    public bool IsDown(ShapeMouseWheelAxis axis, float deadzone = 0.2f)
    {
        return GetValue(axis, deadzone) != 0f;
    }
    /// <summary>
    /// Gets the value of the specified mouse wheel axis, considering deadzone and modifier keys.
    /// </summary>
    public float GetValue(ShapeMouseWheelAxis axis, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        if (isLocked) return 0f;
        if (!GameWindow.Instance.MouseOnScreen) return 0f;
        if (!IModifierKey.IsActive(modifierOperator, modifierKeys)) return 0f;
        return GetValue(axis, deadzone);
    }
    /// <summary>
    /// Gets the value of the specified mouse wheel axis, considering deadzone.
    /// </summary>
    public float GetValue(ShapeMouseWheelAxis axis, float deadzone = 0.2f)
    {
        if (isLocked) return 0f;
        if (!GameWindow.Instance.MouseOnScreen) return 0f;

        var value = MouseWheelV; // Raylib.GetMouseWheelMoveV();
        float returnValue = axis == ShapeMouseWheelAxis.VERTICAL ? value.Y : value.X;
        if (MathF.Abs(returnValue) < deadzone) return 0f;
        return returnValue;
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified mouse wheel axis.
    /// </summary>
    public InputState CreateInputState(ShapeMouseWheelAxis axis, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        
        float axisValue = GetValue(axis, deadzone, modifierOperator, modifierKeys);
        bool down = axisValue != 0f;
        return new(down, !down, axisValue, -1, InputDeviceType.Mouse);
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified mouse wheel axis, using a previous state.
    /// </summary>
    public InputState CreateInputState(ShapeMouseWheelAxis axis, InputState previousState, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        return new(previousState, CreateInputState(axis, deadzone, modifierOperator, modifierKeys));
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified mouse wheel axis.
    /// </summary>
    public InputState CreateInputState(ShapeMouseWheelAxis axis, float deadzone = 0.2f)
    {
        
        float axisValue = GetValue(axis, deadzone);
        bool down = axisValue != 0f;
        return new(down, !down, axisValue, -1, InputDeviceType.Mouse);
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified mouse wheel axis, using a previous state.
    /// </summary>
    public InputState CreateInputState(ShapeMouseWheelAxis axis, InputState previousState, float deadzone = 0.2f)
    {
        return new(previousState, CreateInputState(axis, deadzone));
    }
    
    /// <summary>
    /// Gets the display name for a mouse wheel axis.
    /// </summary>
    /// <param name="axis">The wheel axis.</param>
    /// <param name="shortHand">Whether to use shorthand notation.</param>
    /// <returns>The wheel axis name.</returns>
    public static string GetWheelAxisName(ShapeMouseWheelAxis axis, bool shortHand = true)
    {
        switch (axis)
        {
            case ShapeMouseWheelAxis.HORIZONTAL: return shortHand ? "MWx" : "Mouse Wheel Horizontal";
            case ShapeMouseWheelAxis.VERTICAL: return shortHand ? "MWy" : "Mouse Wheel Vertical";
            default: return "No Key";
        }
    }

    #endregion
    
    #region Button
    /// <summary>
    /// Gets the display name for a mouse button.
    /// </summary>
    /// <param name="button">The mouse button.</param>
    /// <param name="shortHand">Whether to use shorthand notation.</param>
    /// <returns>The button name.</returns>
    public static string GetButtonName(ShapeMouseButton button, bool shortHand = true)
    {
        switch (button)
        {
            case ShapeMouseButton.LEFT: return shortHand ? "LMB" : "Left Mouse Button";
            case ShapeMouseButton.RIGHT: return shortHand ? "RMB" : "Right Mouse Button";
            case ShapeMouseButton.MIDDLE: return shortHand ? "MMB" : "Middle Mouse Button";
            case ShapeMouseButton.SIDE: return shortHand ? "SMB" : "Side Mouse Button";
            case ShapeMouseButton.EXTRA: return shortHand ? "EMB" : "Extra Mouse Button";
            case ShapeMouseButton.FORWARD: return shortHand ? "FMB" : "Forward Mouse Button";
            case ShapeMouseButton.BACK: return shortHand ? "BMB" : "Back Mouse Button";
            case ShapeMouseButton.MW_UP: return shortHand ? "MW U" : "Mouse Wheel Up";
            case ShapeMouseButton.MW_DOWN: return shortHand ? "MW D" : "Mouse Wheel Down";
            case ShapeMouseButton.MW_LEFT: return shortHand ? "MW L" : "Mouse Wheel Left";
            case ShapeMouseButton.MW_RIGHT: return shortHand ? "MW R" : "Mouse Wheel Right";
            case ShapeMouseButton.UP_AXIS: return shortHand ? "M Up" : "Mouse Up";
            case ShapeMouseButton.DOWN_AXIS: return shortHand ? "M Dwn" : "Mouse Down";
            case ShapeMouseButton.LEFT_AXIS: return shortHand ? "M Lft" : "Mouse Left";
            case ShapeMouseButton.RIGHT_AXIS: return shortHand ? "M Rgt" : "Mouse Right";
            default: return "No Key";
        }
    }

    /// <summary>
    /// Checks if a modifier key is active, optionally reversing the logic.
    /// </summary>
    /// <param name="modifierKey">The modifier key to check.</param>
    /// <param name="reverseModifier">If true, reverses the modifier logic.</param>
    /// <returns>True if the modifier is active (or inactive if reversed).</returns>
    public bool IsModifierActive(ShapeMouseButton modifierKey, bool reverseModifier) => IsDown(modifierKey) != reverseModifier;

    /// <summary>
    /// Determines if the specified mouse button is down, considering move and wheel deadzones.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <param name="mouseMoveDeadzone">Deadzone for mouse movement axis buttons.</param>
    /// <param name="mouseWheelDeadzone">Deadzone for mouse wheel axis buttons.</param>
    /// <returns>True if the button is down.</returns>
    public bool IsDown(ShapeMouseButton button, float mouseMoveDeadzone = 0f, float mouseWheelDeadzone = 0f)
    {
       return GetValue(button, mouseMoveDeadzone, mouseWheelDeadzone) != 0;
    }

    /// <summary>
    /// Determines if the specified mouse button is down, considering deadzone and modifier keys.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <param name="deadzone">Deadzone threshold.</param>
    /// <param name="modifierOperator">Modifier key operator.</param>
    /// <param name="modifierKeys">Modifier keys to check.</param>
    /// <returns>True if the button is down.</returns>
    public bool IsDown(ShapeMouseButton button, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
       return GetValue(button, deadzone, modifierOperator, modifierKeys) != 0f;
    }

    /// <summary>
    /// Determines if the specified mouse button is down, considering move/wheel deadzones and modifier keys.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <param name="mouseMoveDeadzone">Deadzone for mouse movement axis buttons.</param>
    /// <param name="mouseWheelDeadzone">Deadzone for mouse wheel axis buttons.</param>
    /// <param name="modifierOperator">Modifier key operator.</param>
    /// <param name="modifierKeys">Modifier keys to check.</param>
    /// <returns>True if the button is down.</returns>
    public bool IsDown(ShapeMouseButton button, float mouseMoveDeadzone, float mouseWheelDeadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
       return GetValue(button, mouseMoveDeadzone, mouseWheelDeadzone, modifierOperator, modifierKeys) != 0f;
    }

    /// <summary>
    /// Gets the value of the specified mouse button, considering deadzone and modifier keys.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <param name="deadzone">Deadzone threshold.</param>
    /// <param name="modifierOperator">Modifier key operator.</param>
    /// <param name="modifierKeys">Modifier keys to check.</param>
    /// <returns>The button value.</returns>
    public float GetValue(ShapeMouseButton button, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
       if (isLocked) return 0f;
       if (!GameWindow.Instance.MouseOnScreen) return 0f;
       if (!IModifierKey.IsActive(modifierOperator, modifierKeys)) return 0f;
       return GetValue(button, deadzone, deadzone);
    }

    /// <summary>
    /// Gets the value of the specified mouse button, considering move/wheel deadzones and modifier keys.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <param name="mouseMoveDeadzone">Deadzone for mouse movement axis buttons.</param>
    /// <param name="mouseWheelDeadzone">Deadzone for mouse wheel axis buttons.</param>
    /// <param name="modifierOperator">Modifier key operator.</param>
    /// <param name="modifierKeys">Modifier keys to check.</param>
    /// <returns>The button value.</returns>
    public float GetValue(ShapeMouseButton button, float mouseMoveDeadzone, float mouseWheelDeadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
       if (isLocked) return 0f;
       if (!GameWindow.Instance.MouseOnScreen) return 0f;
       if (!IModifierKey.IsActive(modifierOperator, modifierKeys)) return 0f;
       return GetValue(button, mouseMoveDeadzone, mouseWheelDeadzone);
    }

    /// <summary>
    /// Gets the value of the specified <see cref="ShapeMouseButton"/>.
    /// For wheel and axis buttons, applies the respective deadzone thresholds.
    /// For standard buttons, returns 1f if pressed, otherwise 0f.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <param name="mouseMoveDeadzone">Deadzone threshold for mouse movement axis buttons.</param>
    /// <param name="mouseWheelDeadzone">Deadzone threshold for mouse wheel axis buttons.</param>
    /// <returns>
    /// A float representing the button value:
    /// - For wheel/axis buttons: the movement value if above deadzone, otherwise 0f.
    /// - For standard buttons: 1f if pressed, otherwise 0f.
    /// </returns>
    public float GetValue(ShapeMouseButton button, float mouseMoveDeadzone = 0f, float mouseWheelDeadzone = 0f)
    {
        // if (button == ShapeMouseButton.NONE) return 0f;
        if (isLocked) return 0f;
        if (!GameWindow.Instance.MouseOnScreen) return 0f;
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
            if(button == ShapeMouseButton.UP_AXIS) return mouseDelta.Y < -mouseMoveDeadzone ? MathF.Abs(mouseDelta.X) : 0f;
            if(button == ShapeMouseButton.DOWN_AXIS) return mouseDelta.Y > mouseMoveDeadzone ? mouseDelta.Y : 0f;
            return 0f;
        }
        return Raylib.IsMouseButtonDown((MouseButton)id) ? 1f : 0f;
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified mouse button.
    /// </summary>
    public InputState CreateInputState(ShapeMouseButton button, float mouseMoveDeadzone, float mouseWheelDeadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        var value = GetValue(button, mouseMoveDeadzone, mouseWheelDeadzone, modifierOperator, modifierKeys);
        bool down = value != 0f;
        return new(down, !down, down ? 1f : 0f, -1, InputDeviceType.Mouse);
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified mouse button, using a previous state.
    /// </summary>
    public InputState CreateInputState(ShapeMouseButton button, InputState previousState, float mouseMoveDeadzone, float mouseWheelDeadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        return new(previousState, CreateInputState(button, mouseMoveDeadzone, mouseWheelDeadzone, modifierOperator, modifierKeys));
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified mouse button.
    /// </summary>
    public InputState CreateInputState(ShapeMouseButton button, float mouseMoveDeadzone = 0f, float mouseWheelDeadzone = 0f)
    {
        var value = GetValue(button, mouseMoveDeadzone, mouseWheelDeadzone);
        bool down = value != 0f;
        return new(down, !down, down ? 1f : 0f, -1, InputDeviceType.Mouse);
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified mouse button, using a previous state.
    /// </summary>
    public InputState CreateInputState(ShapeMouseButton button, InputState previousState, float mouseMoveDeadzone = 0f, float mouseWheelDeadzone = 0f)
    {
        return new(previousState, CreateInputState(button, mouseMoveDeadzone, mouseWheelDeadzone));
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified mouse button.
    /// </summary>
    public InputState CreateInputState(ShapeMouseButton button, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        var value = GetValue(button, deadzone, modifierOperator, modifierKeys);
        bool down = value != 0f;
        return new(down, !down, down ? 1f : 0f, -1, InputDeviceType.Mouse);
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified mouse button, using a previous state.
    /// </summary>
    public InputState CreateInputState(ShapeMouseButton button, InputState previousState, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        return new(previousState, CreateInputState(button, deadzone, modifierOperator, modifierKeys));
    }
    #endregion

    #region ButtonAxis

    /// <summary>
    /// Gets the display name for a button axis (negative and positive button pair).
    /// </summary>
    /// <param name="neg">Negative direction button.</param>
    /// <param name="pos">Positive direction button.</param>
    /// <param name="shorthand">Whether to use shorthand notation.</param>
    /// <returns>The button axis name.</returns>
    public static string GetButtonAxisName(ShapeMouseButton neg, ShapeMouseButton pos, bool shorthand = true)
    {
        StringBuilder sb = new();
        
        string negName = GetButtonName(neg, shorthand);
        string posName = GetButtonName(pos, shorthand);
        sb.Append(negName);
        sb.Append('|');
        sb.Append(posName);
        return sb.ToString();
    }

    /// <summary>
    /// Determines if the button axis (negative/positive) is "down" with modifier keys.
    /// </summary>
    public bool IsDown(ShapeMouseButton neg, ShapeMouseButton pos, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        return GetValue(neg, pos, deadzone, modifierOperator, modifierKeys) != 0f;
    }
    /// <summary>
    /// Gets the value of the button axis (negative/positive), considering deadzone and modifier keys.
    /// </summary>
    public float GetValue(ShapeMouseButton neg, ShapeMouseButton pos, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        if (isLocked) return 0f;
        if (!IModifierKey.IsActive(modifierOperator, modifierKeys)) return 0f;
        return GetValue(neg, pos, deadzone);
    }
   
    /// <summary>
    /// Creates an <see cref="InputState"/> for the button axis (negative/positive).
    /// </summary>
    public InputState CreateInputState(ShapeMouseButton neg, ShapeMouseButton pos, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        float axis = GetValue(neg, pos, deadzone, modifierOperator, modifierKeys);
        bool down = axis != 0f;
        return new(down, !down, axis, -1, InputDeviceType.Mouse);
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the button axis (negative/positive), using a previous state.
    /// </summary>
    public InputState CreateInputState(ShapeMouseButton neg, ShapeMouseButton pos, InputState previousState, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        return new(previousState, CreateInputState(neg, pos, deadzone, modifierOperator, modifierKeys));
    }
    /// <summary>
    /// Determines if the button axis (negative/positive) is "down" with modifier keys.
    /// </summary>
    public bool IsDown(ShapeMouseButton neg, ShapeMouseButton pos, float mouseMoveDeadzone, float mouseWheelDeadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        return GetValue(neg, pos, mouseMoveDeadzone, mouseWheelDeadzone, modifierOperator, modifierKeys) != 0f;
    }

    /// <summary>
    /// Determines if the button axis (negative/positive) is "down".
    /// </summary>
    public bool IsDown(ShapeMouseButton neg, ShapeMouseButton pos, float mouseMoveDeadzone = 0f, float mouseWheelDeadzone = 0f)
    {
        return GetValue(neg, pos, mouseMoveDeadzone, mouseWheelDeadzone) != 0f;
    }
    /// <summary>
    /// Gets the value of the button axis (negative/positive), considering deadzone and modifier keys.
    /// </summary>
    public float GetValue(ShapeMouseButton neg, ShapeMouseButton pos, float mouseMoveDeadzone, float mouseWheelDeadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        if (isLocked) return 0f;
        if (!IModifierKey.IsActive(modifierOperator, modifierKeys)) return 0f;
        return GetValue(neg, pos, mouseMoveDeadzone, mouseWheelDeadzone);
    }
    /// <summary>
    /// Gets the value of the button axis (negative/positive), considering deadzone.
    /// </summary>
    public float GetValue(ShapeMouseButton neg, ShapeMouseButton pos, float mouseMoveDeadzone = 0f, float mouseWheelDeadzone = 0f)
    {
        if (isLocked) return 0f;
        float vNegative = GetValue(neg, mouseMoveDeadzone, mouseWheelDeadzone);
        float vPositive = GetValue(pos, mouseMoveDeadzone, mouseWheelDeadzone);
        return vPositive - vNegative;
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the button axis (negative/positive).
    /// </summary>
    public InputState CreateInputState(ShapeMouseButton neg, ShapeMouseButton pos, float mouseMoveDeadzone, float mouseWheelDeadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        float axis = GetValue(neg, pos, mouseMoveDeadzone, mouseWheelDeadzone, modifierOperator, modifierKeys);
        bool down = axis != 0f;
        return new(down, !down, axis, -1, InputDeviceType.Mouse);
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the button axis (negative/positive), using a previous state.
    /// </summary>
    public InputState CreateInputState(ShapeMouseButton neg, ShapeMouseButton pos, InputState previousState, float mouseMoveDeadzone, float mouseWheelDeadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        return new(previousState, CreateInputState(neg, pos, mouseMoveDeadzone, mouseWheelDeadzone, modifierOperator, modifierKeys));
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the button axis (negative/positive).
    /// </summary>
    public InputState CreateInputState(ShapeMouseButton neg, ShapeMouseButton pos, float mouseMoveDeadzone = 0f, float mouseWheelDeadzone = 0f)
    {
        float axis = GetValue(neg, pos, mouseMoveDeadzone, mouseWheelDeadzone);
        bool down = axis != 0f;
        return new(down, !down, axis, -1, InputDeviceType.Mouse);
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the button axis (negative/positive), using a previous state.
    /// </summary>
    public InputState CreateInputState(ShapeMouseButton neg, ShapeMouseButton pos, InputState previousState, float mouseMoveDeadzone = 0f, float mouseWheelDeadzone = 0f)
    {
        return new(previousState, CreateInputState(neg, pos, mouseMoveDeadzone, mouseWheelDeadzone));
    }
    

    #endregion
}
