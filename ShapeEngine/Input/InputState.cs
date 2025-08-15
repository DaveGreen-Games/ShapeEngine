using ShapeEngine.StaticLib;

namespace ShapeEngine.Input;


/// <summary>
/// Represents the state of an input, including button presses, axis values, device type, and input gesture details.
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
    /// Indicates if the input axis is inverted.
    /// </summary>
    public readonly bool Inverted;
    /// <summary>
    /// The result of the input action gesture, containing details about how the input was triggered (e.g., multi-tap, hold, etc.)
    /// </summary>
    public readonly InputGesture.Result GestureResult;
    
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
    /// Initializes a new instance of the <see cref="InputState"/> struct with default values.
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
        Inverted = false;
        GestureResult = new();
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="InputState"/> struct with specified values.
    /// </summary>
    /// <param name="down">Indicates if the input is currently held down.</param>
    /// <param name="up">Indicates if the input is currently up (not pressed).</param>
    /// <param name="axisRaw">The raw axis value, unprocessed.</param>
    /// <param name="gamepad">The index of the gamepad associated with this input, or -1 if not applicable.</param>
    /// <param name="inputDeviceType">The type of input device (keyboard, mouse, gamepad, etc.).</param>
    /// <param name="inverted">Indicates if the input axis is inverted.</param>
    public InputState(bool down, bool up, float axisRaw, int gamepad, InputDeviceType inputDeviceType, bool inverted = false)
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
        Inverted = inverted;
        GestureResult = new();
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="InputState"/> struct with specified values, including input gesture result.
    /// </summary>
    /// <param name="down">Indicates if the input is currently held down.</param>
    /// <param name="up">Indicates if the input is currently up (not pressed).</param>
    /// <param name="axisRaw">The raw axis value, unprocessed.</param>
    /// <param name="gamepad">The index of the gamepad associated with this input, or -1 if not applicable.</param>
    /// <param name="inputDeviceType">The type of input device (keyboard, mouse, gamepad, etc.).</param>
    /// <param name="gestureResult">The result of the input action gesture.</param>
    /// <param name="inverted">Indicates if the input axis is inverted.</param>
    public InputState(bool down, bool up, float axisRaw, int gamepad, InputDeviceType inputDeviceType, InputGesture.Result gestureResult,  bool inverted = false)
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
        Inverted = inverted;
        GestureResult = gestureResult;
    }
    
    /// <summary>
    /// Creates a new <see cref="InputState"/> by copying all values from an existing state,
    /// but replacing the <see cref="GestureResult"/> with the specified value.
    /// </summary>
    /// <param name="state">The source <see cref="InputState"/> to copy values from.</param>
    /// <param name="gestureResult">The new input gesture result to use.</param> 
    public InputState(InputState state, InputGesture.Result gestureResult)
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
        Inverted = state.Inverted;
        GestureResult = gestureResult;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="InputState"/> struct by comparing the previous and current input states.
    /// Calculates <see cref="Pressed"/> and <see cref="Released"/> based on transitions between <paramref name="prev"/> and <paramref name="cur"/>.
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
        GestureResult = cur.GestureResult;
        Inverted = cur.Inverted;

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InputState"/> struct by comparing the previous and current input states,
    /// and allows overriding the input device type.
    /// Calculates <see cref="Pressed"/> and <see cref="Released"/> based on transitions between <paramref name="prev"/> and <paramref name="cur"/>.
    /// </summary>
    /// <param name="prev">The previous input state.</param>
    /// <param name="cur">The current input state.</param>
    /// <param name="inputDeviceType">The input device type to use for this state.</param>
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
        GestureResult = cur.GestureResult;
        Inverted = cur.Inverted;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InputState"/> struct by copying all values from an existing state,
    /// but replacing the <see cref="Consumed"/> property with the specified value.
    /// </summary>
    /// <param name="state">The source <see cref="InputState"/> to copy values from.</param>
    /// <param name="consumed">The value to set for <see cref="Consumed"/>.</param>
    private InputState(InputState state, bool consumed)
    {
        Down = state.Down;
        Up = state.Up;
        Released = state.Released;
        Pressed = state.Pressed;
        Gamepad = state.Gamepad;
        AxisRaw = state.AxisRaw;
        Axis = state.Axis;
        Consumed = consumed;
        InputDeviceType = state.InputDeviceType;
        Inverted = state.Inverted;
        GestureResult = state.GestureResult;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InputState"/> struct by copying all values from an existing state,
    /// but replacing the <see cref="Axis"/> property with the specified value (clamped between -1 and 1).
    /// </summary>
    /// <param name="state">The source <see cref="InputState"/> to copy values from.</param>
    /// <param name="axis">The new axis value to use (will be clamped between -1 and 1).</param>
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
        Inverted = state.Inverted;
        GestureResult = state.GestureResult;
    }
    /// <summary>
    /// Accumulates another <see cref="InputState"/> into this one, combining their values.
    /// </summary>
    /// <param name="other">The other input state to accumulate.</param>
    /// <returns>A new accumulated <see cref="InputState"/>.</returns>
    internal InputState Accumulate(InputState other)
    {
        var inputDevice = InputDeviceType;
        var inverted = Inverted;
        if (other.Down)
        {
            if (!Down || MathF.Abs(other.AxisRaw) > MathF.Abs(AxisRaw))
            {
                inputDevice = other.InputDeviceType;
                inverted = other.Inverted;
            }
        }

        float axis = AxisRaw;
        if (MathF.Abs(other.AxisRaw) > MathF.Abs((AxisRaw))) axis = other.AxisRaw;
        bool down = Down || other.Down;
        bool up = Up && other.Up;
        
        return new(down, up, axis, other.Gamepad, inputDevice, new(), inverted);
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