using ShapeEngine.StaticLib;

namespace ShapeEngine.Input;

/// <summary>
/// Represents the state of an input, including button states, axis values, device type, and multi-tap/hold information.
/// </summary>
public readonly struct InputState
{
    /// <summary>
    /// Indicates if the input is currently held down.
    /// </summary>
    public readonly bool Down;
    /// <summary>
    /// Indicates if the input is currently up (not pressed).
    /// </summary>
    public readonly bool Up;
    /// <summary>
    /// Indicates if the input was released this frame.
    /// </summary>
    public readonly bool Released;
    /// <summary>
    /// Indicates if the input was pressed this frame.
    /// </summary>
    public readonly bool Pressed;

    /// <summary>
    /// The processed axis value, typically smoothed or filtered.
    /// <remarks>
    /// Use <see cref="AxisRaw"/> if you want the raw axis value recorded from the device without any modification or changes to it.
    /// </remarks>
    /// </summary>
    public readonly float Axis;
    /// <summary>
    /// The raw axis value, unprocessed.
    /// <remarks>
    /// This value is the axis value recorded from the device without any modification or changes to it.
    /// Use <see cref="Axis"/> if you want a filtered or smoothed value.
    /// </remarks>
    /// </summary>
    public readonly float AxisRaw;
    /// <summary>
    /// The index of the gamepad associated with this input, or -1 if not applicable.
    /// </summary>
    public readonly int Gamepad;
    /// <summary>
    /// The type of input device (keyboard, mouse, gamepad, etc.).
    /// </summary>
    public readonly InputDeviceType InputDeviceType;
    
    /// <summary>
    /// Indicates if this input state has been consumed.
    /// </summary>
    public readonly bool Consumed;

    /// <summary>
    /// The normalized hold progress (0 to 1).
    /// </summary>
    public readonly float HoldF;
    /// <summary>
    /// The current state of the hold action (None, InProgress, Completed, Failed).
    /// </summary>
    public readonly MultiTapState HoldState;
    /// <summary>
    /// The normalized multi-tap progress (0 to 1).
    /// </summary>
    public readonly float MultiTapF;
    /// <summary>
    /// The current state of the multi-tap action (None, InProgress, Completed, Failed).
    /// </summary>
    public readonly MultiTapState MultiTapState;

    /// <summary>
    /// Determines the type of press (Hold, MultiTap, SingleTap, or None) based on the current state.
    /// </summary>
    /// <returns>The detected <see cref="PressedType"/>.</returns>
    public PressedType GetPressedType()
    {
        if (HoldState == MultiTapState.Completed) return PressedType.Hold;
        if (MultiTapState == MultiTapState.Completed) return PressedType.MultiTap;
        
        if (HoldF <= 0f && MultiTapState == MultiTapState.Failed) return PressedType.SingleTap;
        if (MultiTapF <= 0f && HoldState == MultiTapState.Failed) return PressedType.SingleTap;
        
        
        return PressedType.None;

    }
    
    /// <summary>
    /// Initializes a new default instance of <see cref="InputState"/> with all values set to their defaults.
    /// <remarks>
    /// Down = false;
    /// Up = true;
    /// Released = false;
    /// Pressed = false;
    /// AxisRaw = 0f;
    /// Axis = 0f;
    /// Gamepad = -1;
    /// Consumed = false;
    /// InputDeviceType = InputDeviceType.Keyboard;
    /// HoldF = 0f;
    /// HoldState = MultiTapState.None;
    /// MultiTapF = 0f;
    /// MultiTapState = MultiTapState.None;
    /// </remarks>
    /// </summary>
    public InputState()
    {
        Down = false;
        Up = true;
        Released = false;
        Pressed = false;
        AxisRaw = 0f;
        Axis = 0f;
        Gamepad = -1;
        Consumed = false;
        InputDeviceType = InputDeviceType.Keyboard;
        HoldF = 0f;
        HoldState = MultiTapState.None;
        MultiTapF = 0f;
        MultiTapState = MultiTapState.None;
    }
    /// <summary>
    /// Initializes a new instance of <see cref="InputState"/> with specified button and axis values.
    /// </summary>
    /// <param name="down">Whether the input is down.</param>
    /// <param name="up">Whether the input is up.</param>
    /// <param name="axisRaw">The raw axis value.</param>
    /// <param name="gamepad">The gamepad index.</param>
    /// <param name="inputDeviceType">The input device type.</param>
    public InputState(bool down, bool up, float axisRaw, int gamepad, InputDeviceType inputDeviceType)
    {
        Down = down;
        Up = up;
        Released = false;
        Pressed = false;
        AxisRaw = axisRaw;
        Axis = axisRaw;
        Gamepad = gamepad;
        Consumed = false;
        InputDeviceType = inputDeviceType;
        HoldF = 0f;
        HoldState = MultiTapState.None;
        // HoldFinished = false;
        MultiTapF = 0f;
        MultiTapState = MultiTapState.None;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="InputState"/> based on another state and hold/multitap progress.
    /// </summary>
    /// <param name="state">The base input state.</param>
    /// <param name="holdF">The hold progress.</param>
    /// <param name="multiTapF">The multitap progress.</param>
    public InputState(InputState state, float holdF, float multiTapF)
    {
        Down = state.Down;
        Up = state.Up;
        Released = state.Released;
        Pressed = state.Pressed;
        
        
        Gamepad = state.Gamepad;
        AxisRaw = state.AxisRaw;
        Axis = state.Axis;
        Consumed = state.Consumed;
        InputDeviceType = state.InputDeviceType;

        HoldF = holdF;
        HoldState = MultiTapState.None;
        // HoldFinished = false;
        MultiTapF = multiTapF;
        MultiTapState = MultiTapState.None; 
        
    }
    /// <summary>
    /// Initializes a new instance of <see cref="InputState"/> based on previous and current states.
    /// </summary>
    /// <param name="prev">The previous input state.</param>
    /// <param name="cur">The current input state.</param>
    public InputState(InputState prev, InputState cur)
    {
        Gamepad = cur.Gamepad;
        AxisRaw = cur.AxisRaw;
        Axis = prev.Axis;
        Down = cur.Down;
        Up = cur.Up;
        Pressed = prev.Up && cur.Down;
        Released = prev.Down && cur.Up;
        Consumed = false;
        InputDeviceType = cur.InputDeviceType;

        if (prev.HoldF is > 0f and < 1f)
        {
            if(cur.HoldF >= 1f) HoldState = MultiTapState.Completed;
            else if (cur.HoldF <= 0f) HoldState = MultiTapState.Failed;
            else HoldState = MultiTapState.InProgress;
        }
        else HoldState = MultiTapState.None;
        HoldF = cur.HoldF;

        if (prev.MultiTapF is > 0f and < 1f)
        {
            if(cur.MultiTapF >= 1f) MultiTapState = MultiTapState.Completed;
            else if (cur.MultiTapF <= 0f) MultiTapState = MultiTapState.Failed;
            else MultiTapState = MultiTapState.InProgress;
        }
        else MultiTapState = MultiTapState.None;
        MultiTapF = cur.MultiTapF;

    }
    /// <summary>
    /// Initializes a new instance of <see cref="InputState"/> based on previous and current states and a specific device type.
    /// </summary>
    /// <param name="prev">The previous input state.</param>
    /// <param name="cur">The current input state.</param>
    /// <param name="inputDeviceType">The input device type.</param>
    public InputState(InputState prev, InputState cur, InputDeviceType inputDeviceType)
    {
        Gamepad = cur.Gamepad;
        AxisRaw = cur.AxisRaw;
        Axis = prev.Axis;
        Down = cur.Down;
        Up = cur.Up;
        Pressed = prev.Up && cur.Down;
        Released = prev.Down && cur.Up;
        Consumed = false;
        InputDeviceType = inputDeviceType;

        // if (prev.HoldF < 1f && cur.HoldF >= 1f) HoldFinished = true;
        // else HoldFinished = false;
        if (prev.HoldF is > 0f and < 1f)
        {
            if(cur.HoldF >= 1f) HoldState = MultiTapState.Completed;
            else if (cur.HoldF <= 0f) HoldState = MultiTapState.Failed;
            else HoldState = MultiTapState.InProgress;
        }
        else HoldState = MultiTapState.None;
        HoldF = cur.HoldF;
        
        if (prev.MultiTapF is > 0f and < 1f)
        {
            if(cur.MultiTapF >= 1f) MultiTapState = MultiTapState.Completed;
            else if (cur.MultiTapF <= 0f) MultiTapState = MultiTapState.Failed;
            else MultiTapState = MultiTapState.InProgress;
        }
        else MultiTapState = MultiTapState.None;
        MultiTapF = cur.MultiTapF;
    }
    /// <summary>
    /// Initializes a new instance of <see cref="InputState"/> with a consumed flag.
    /// </summary>
    /// <param name="other">The base input state.</param>
    /// <param name="consumed">Whether the input is consumed.</param>
    private InputState(InputState other, bool consumed)
    {
        Down = other.Down;
        Up = other.Up;
        Released = other.Released;
        Pressed = other.Pressed;
        Gamepad = other.Gamepad;
        AxisRaw = other.AxisRaw;
        Axis = other.Axis;
        Consumed = consumed;
        InputDeviceType = other.InputDeviceType;

        HoldF = other.HoldF;
        HoldState = other.HoldState;
        // HoldFinished = other.HoldFinished;
        MultiTapF = other.MultiTapF;
        MultiTapState = other.MultiTapState;
    }
    /// <summary>
    /// Initializes a new instance of <see cref="InputState"/> with a modified axis value.
    /// </summary>
    /// <param name="state">The base input state.</param>
    /// <param name="axis">The new axis value.</param>
    private InputState(InputState state, float axis)
    {
        Down = state.Down;
        Up = state.Up;
        Released = state.Released;
        Pressed = state.Pressed;
        Gamepad = state.Gamepad;
        AxisRaw = state.AxisRaw;
        Axis = ShapeMath.Clamp(axis, -1f, 1f);
        Consumed = state.Consumed;
        InputDeviceType = state.InputDeviceType;
        
        HoldF = state.HoldF;
        HoldState = state.HoldState;
        // HoldFinished = state.HoldFinished;
        MultiTapF = state.MultiTapF;
        MultiTapState = state.MultiTapState;
    }
    /// <summary>
    /// Accumulates another <see cref="InputState"/> into this one, combining their values.
    /// </summary>
    /// <param name="other">The other input state to accumulate.</param>
    /// <returns>A new accumulated <see cref="InputState"/>.</returns>
    public InputState Accumulate(InputState other)
    {
        var inputDevice = InputDeviceType;
        if (other.Down)
        {
            if(!Down || MathF.Abs(other.AxisRaw) > MathF.Abs(AxisRaw))inputDevice = other.InputDeviceType;
        }

        float axis = AxisRaw;
        if (MathF.Abs(other.AxisRaw) > MathF.Abs((AxisRaw))) axis = other.AxisRaw;
        bool down = Down || other.Down;
        bool up = Up && other.Up;
        
        return new(down, up, axis, other.Gamepad, inputDevice);
    }
    /// <summary>
    /// Returns a new <see cref="InputState"/> with the axis adjusted by the specified value.
    /// </summary>
    /// <param name="value">The value to adjust the axis by.</param>
    /// <returns>A new <see cref="InputState"/> with the adjusted axis.</returns>
    public InputState AdjustAxis(float value) => value == 0f ? this : new InputState(this, Axis + value);

    /// <summary>
    /// Returns a new <see cref="InputState"/> marked as consumed.
    /// </summary>
    /// <returns>A new <see cref="InputState"/> with <see cref="Consumed"/> set to true.</returns>
    public InputState Consume() => new(this, true);
}