using ShapeEngine.Lib;

namespace ShapeEngine.Input;

public readonly struct InputState
{
    public readonly  bool Down;
    public readonly bool Up;
    public readonly bool Released;
    public readonly bool Pressed;

    public readonly float Axis;
    public readonly float AxisRaw;
    public readonly int Gamepad;
    
    public readonly bool Consumed;

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
    }
    public InputState(bool down, bool up, float axisRaw, int gamepad)
    {
        Down = down;
        Up = up;
        Released = false;
        Pressed = false;
        AxisRaw = axisRaw;
        Axis = axisRaw;
        Gamepad = gamepad;
        Consumed = false;
    }
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
    }
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
    }
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
    }
    public InputState Accumulate(InputState other)
    {
        float axis = MathF.Max(AxisRaw, other.AxisRaw); // ShapeMath.Clamp(Axis + other.Axis, 0f, 1f);
        bool down = Down || other.Down;
        bool up = Up && other.Up;
        return new(down, up, axis, Gamepad);
    }
    public InputState AdjustAxis(float value) => value == 0f ? this : new InputState(this, Axis + value);
    public InputState Consume() => new(this, true);
}