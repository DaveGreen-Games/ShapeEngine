using System.Text;
using Raylib_cs;
using ShapeEngine.Core;

namespace ShapeEngine.Input;

//TODO: keyboard and gamepad device usage should cancel mouse usage detection!
//If 2 mouse buttons were pressed in succession and a third press is needed for the mouse to count as used,
//but a keyboard press happens before the third mouse press, the mouse count / timer is reset.
//counts for all other devices as well

/// <summary>
/// Represents a mouse input device, providing access to mouse buttons, axes, and wheel axes,
/// as well as state tracking and utility methods for mouse input.
/// </summary>
public sealed class ShapeMouseDevice : ShapeInputDevice
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

    private int pressedCount = 0;
    private float pressedCountDurationTimer = 0f;
    private float usedDurationTimer = 0f;

    private readonly Dictionary<ShapeMouseButton, InputState> buttonStates = new(AllShapeMouseButtons.Length);
    private readonly Dictionary<ShapeMouseAxis, InputState> axisStates = new(2);
    private readonly Dictionary<ShapeMouseWheelAxis, InputState> wheelAxisStates = new(2);

    /// <summary>
    /// Event triggered when a mouse button is pressed.
    /// </summary>
    public event Action<ShapeMouseButton>? OnButtonPressed;
    /// <summary>
    /// Event triggered when a mouse button is released.
    /// </summary>
    public event Action<ShapeMouseButton>? OnButtonReleased;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ShapeMouseDevice"/> class.
    /// </summary>
    internal ShapeMouseDevice()
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
    
    /// <inheritdoc cref="ShapeInputDevice"/>
    public void ApplyInputDeviceChangeSettings(InputDeviceUsageDetectionSettings settings) => UsageDetectionSettings = settings.Mouse;

    /// <inheritdoc cref="ShapeInputDevice"/>
    public bool WasUsed() => wasUsed;
    
    /// <inheritdoc cref="ShapeInputDevice"/>
    public bool WasUsedRaw() => wasUsedRaw;
    
    /// <summary>
    /// Returns whether the mouse device is currently locked.
    /// </summary>
    public bool IsLocked() => isLocked;

    /// <summary>
    /// Locks the mouse device, preventing input from being registered.
    /// </summary>
    public void Lock()
    {
        isLocked = true;
    }

    /// <summary>
    /// Unlocks the mouse device, allowing input to be registered.
    /// </summary>
    public void Unlock()
    {
        isLocked = false;
    }

    /// <summary>
    /// Updates the mouse device state, including button, axis, and wheel axis states.
    /// </summary>
    public bool Update(float dt, bool wasOtherDeviceUsed)
    {
        UpdateStates();
        
        WasMouseUsed(dt, wasOtherDeviceUsed, out wasUsed, out wasUsedRaw);
        
        return wasUsed;
    }

    /// <summary>
    /// Calibrates the mouse device. (Currently not implemented.)
    /// </summary>
    public void Calibrate(){ }

    /// <summary>
    /// Determines if the mouse was used based on movement or button/wheel activity.
    /// </summary>
    /// <returns>True if the mouse was used, otherwise false.</returns>
    private void WasMouseUsed(float dt, bool wasOtherDeviceUsed, out bool used, out bool usedRaw)
    {
        used = false;
        usedRaw = false;
        
        if (isLocked) return;
        
        var moveThreshold = UsageDetectionSettings.MoveThreshold;
        var mouseWheelThreshold = UsageDetectionSettings.MouseWheelThreshold;
        var pressCountEnabled = UsageDetectionSettings.PressCountEnabled;
        var usedDurationEnabled = UsageDetectionSettings.UsedDurationEnabled;
        
        if (!UsageDetectionSettings.MouseDetection || (!pressCountEnabled && !usedDurationEnabled))
        {
            if (Raylib.GetMouseDelta().LengthSquared() > moveThreshold * moveThreshold)
            {
                usedRaw = true;
                return;
            }
            if (Raylib.GetMouseWheelMoveV().LengthSquared() > mouseWheelThreshold * mouseWheelThreshold || 
                Raylib.IsMouseButtonDown(MouseButton.Left) || 
                Raylib.IsMouseButtonDown(MouseButton.Right) || 
                Raylib.IsMouseButtonDown(MouseButton.Middle) || 
                Raylib.IsMouseButtonDown(MouseButton.Extra) || 
                Raylib.IsMouseButtonDown(MouseButton.Forward) || 
                Raylib.IsMouseButtonDown(MouseButton.Back) || 
                Raylib.IsMouseButtonDown(MouseButton.Side))
            {
                usedRaw = true;
            }

            return;
        }

        if (wasOtherDeviceUsed)
        {
            usedDurationTimer = 0f;
            pressedCount = 0;
            pressedCountDurationTimer = 0f;
        }
        
        if (pressCountEnabled)
        {
            pressedCountDurationTimer += dt;
            if (pressedCountDurationTimer >= UsageDetectionSettings.MouseMinPressInterval)
            {
                pressedCountDurationTimer -= UsageDetectionSettings.MouseMinPressInterval;
                pressedCount = 0;
            }
        }
        
        bool movement = Raylib.GetMouseDelta().LengthSquared() > moveThreshold * moveThreshold ||
                        Raylib.GetMouseWheelMoveV().LengthSquared() > mouseWheelThreshold * mouseWheelThreshold;
        
        bool mouseButtonDown = Raylib.IsMouseButtonDown(MouseButton.Left) ||
                               Raylib.IsMouseButtonDown(MouseButton.Right) ||
                               Raylib.IsMouseButtonDown(MouseButton.Middle) ||
                               Raylib.IsMouseButtonDown(MouseButton.Extra) ||
                               Raylib.IsMouseButtonDown(MouseButton.Forward) ||
                               Raylib.IsMouseButtonDown(MouseButton.Back) ||
                               Raylib.IsMouseButtonDown(MouseButton.Side);
        
        if (movement || mouseButtonDown)
        {
            usedRaw = true;
            if (usedDurationEnabled)
            {
                usedDurationTimer += dt;
                if (usedDurationTimer > UsageDetectionSettings.MouseMinUsedDuration)
                {
                    usedDurationTimer -= UsageDetectionSettings.MouseMinUsedDuration;
                    used = true;
                    pressedCount = 0;
                    pressedCountDurationTimer = 0f;
                    return;
                }
            }

            if (pressCountEnabled)
            {
                bool mouseButtonPressedThisFrame = 
                    Raylib.IsMouseButtonPressed(MouseButton.Left) ||
                    Raylib.IsMouseButtonPressed(MouseButton.Right) ||
                    Raylib.IsMouseButtonPressed(MouseButton.Middle) ||
                    Raylib.IsMouseButtonPressed(MouseButton.Extra) ||
                    Raylib.IsMouseButtonPressed(MouseButton.Forward) ||
                    Raylib.IsMouseButtonPressed(MouseButton.Back) ||
                    Raylib.IsMouseButtonPressed(MouseButton.Side);

                if (mouseButtonPressedThisFrame)
                {
                    pressedCount++;
                    if (pressedCount >= UsageDetectionSettings.MouseMinPressCount)
                    {
                        used = true;
                        pressedCountDurationTimer = 0f;
                        usedDurationTimer = 0f;
                    }
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
            var curState = CreateInputState(button);
            var nextState = new InputState(prevState, curState);
            buttonStates[button] = nextState;
            
            if(nextState.Pressed) OnButtonPressed?.Invoke(button);
            else if(nextState.Released) OnButtonReleased?.Invoke(button);
        }
        foreach (var state in axisStates)
        {
            var axis = state.Key;
            var prevState = state.Value;
            var curState = CreateInputState(axis);
            axisStates[axis] = new InputState(prevState, curState);
        }
        foreach (var state in wheelAxisStates)
        {
            var axis = state.Key;
            var prevState = state.Value;
            var curState = CreateInputState(axis);
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
       
        var value = Raylib.GetMouseDelta();
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
        
        var value = Raylib.GetMouseWheelMoveV();
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
    /// Checks if a modifier mouse button is active, optionally reversing the logic.
    /// </summary>
    public bool IsModifierActive(ShapeMouseButton modifierKey, bool reverseModifier)
    {
        return IsDown(modifierKey) != reverseModifier;
    }
    /// <summary>
    /// Determines if the specified mouse button is "down".
    /// </summary>
    public bool IsDown(ShapeMouseButton button, float deadzone = 0f)
    {
        return GetValue(button, deadzone) != 0;
    }

    /// <summary>
    /// Determines if the specified mouse button is "down" with modifier keys.
    /// </summary>
    public bool IsDown(ShapeMouseButton button, float deadzone, ModifierKeyOperator modifierOperator,
        params IModifierKey[] modifierKeys)
    {
        return GetValue(button, deadzone, modifierOperator, modifierKeys) != 0f;
    }
    /// <summary>
    /// Gets the value of the specified mouse button, considering deadzone and modifier keys.
    /// </summary>
    public float GetValue(ShapeMouseButton button, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        if (isLocked) return 0f;
        if (!GameWindow.Instance.MouseOnScreen) return 0f;
        if (!IModifierKey.IsActive(modifierOperator, modifierKeys)) return 0f;
        return GetValue(button, deadzone);
    }
    /// <summary>
    /// Gets the value of the specified mouse button, considering deadzone.
    /// </summary>
    public float GetValue(ShapeMouseButton button, float deadzone = 0f)
    {
        if (isLocked) return 0f;
        if (!GameWindow.Instance.MouseOnScreen) return 0f;
        int id = (int)button;
        if (id is >= 10 and < 20)
        {
            var value = Raylib.GetMouseWheelMoveV();//.Clamp(-1f, 1f);
            
            if (button == ShapeMouseButton.MW_LEFT) return value.X < -deadzone ? MathF.Abs(value.X) : 0f;
            if (button == ShapeMouseButton.MW_RIGHT) return value.X > deadzone ? value.X : 0f;
            if (button == ShapeMouseButton.MW_UP) return value.Y < -deadzone ? MathF.Abs(value.Y) : 0f;
            if (button == ShapeMouseButton.MW_DOWN) return value.Y > deadzone ? value.Y : 0f;
            return 0f;
        }
        if (id >= 20)
        {
            var mouseDelta = Raylib.GetMouseDelta();
            if (button == ShapeMouseButton.LEFT_AXIS) return mouseDelta.X < -deadzone ? MathF.Abs(mouseDelta.X) : 0f;
            if(button == ShapeMouseButton.RIGHT_AXIS) return mouseDelta.X > deadzone ? mouseDelta.X : 0f;
            if(button == ShapeMouseButton.UP_AXIS) return mouseDelta.Y < -deadzone ? MathF.Abs(mouseDelta.X) : 0f;
            if(button == ShapeMouseButton.DOWN_AXIS) return mouseDelta.Y > deadzone ? mouseDelta.Y : 0f;
            return 0f;
        }
        return Raylib.IsMouseButtonDown((MouseButton)id) ? 1f : 0f;
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
    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified mouse button.
    /// </summary>
    public InputState CreateInputState(ShapeMouseButton button, float deadzone = 0f)
    {
        var value = GetValue(button, deadzone);
        bool down = value != 0f;
        return new(down, !down, down ? 1f : 0f, -1, InputDeviceType.Mouse);
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified mouse button, using a previous state.
    /// </summary>
    public InputState CreateInputState(ShapeMouseButton button, InputState previousState, float deadzone = 0f)
    {
        return new(previousState, CreateInputState(button, deadzone));
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
    /// Determines if the button axis (negative/positive) is "down".
    /// </summary>
    public bool IsDown(ShapeMouseButton neg, ShapeMouseButton pos, float deadzone = 0f)
    {
        return GetValue(neg, pos, deadzone) != 0f;
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
    /// Gets the value of the button axis (negative/positive), considering deadzone.
    /// </summary>
    public float GetValue(ShapeMouseButton neg, ShapeMouseButton pos, float deadzone = 0f)
    {
        if (isLocked) return 0f;
        float vNegative = GetValue(neg, deadzone);
        float vPositive = GetValue(pos, deadzone);
        return vPositive - vNegative;
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
    /// Creates an <see cref="InputState"/> for the button axis (negative/positive).
    /// </summary>
    public InputState CreateInputState(ShapeMouseButton neg, ShapeMouseButton pos, float deadzone = 0f)
    {
        float axis = GetValue(neg, pos, deadzone);
        bool down = axis != 0f;
        return new(down, !down, axis, -1, InputDeviceType.Mouse);
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for the button axis (negative/positive), using a previous state.
    /// </summary>
    public InputState CreateInputState(ShapeMouseButton neg, ShapeMouseButton pos, InputState previousState, float deadzone = 0f)
    {
        return new(previousState, CreateInputState(neg, pos, deadzone));
    }
    

    #endregion
}