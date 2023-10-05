using ShapeEngine.Lib;

namespace ShapeEngine.Input;

public readonly struct ShapeInputState
{
    public readonly  bool Down;
    public readonly bool Up;
    public readonly bool Released;
    public readonly bool Pressed;
    public readonly float Axis;
    public readonly int Gamepad;

    public ShapeInputState()
    {
        Down = false;
        Up = true;
        Released = false;
        Pressed = false;
        Axis = 0f;
        Gamepad = -1;
    }
    public ShapeInputState(bool down, bool up, float axis, int gamepad)
    {
        Down = down;
        Up = up;
        Released = false;
        Pressed = false;
        Axis = axis;
        Gamepad = gamepad;
    }

    public ShapeInputState(ShapeInputState prev, ShapeInputState cur)
    {
        Gamepad = cur.Gamepad;
        Axis = cur.Axis;
        Down = cur.Down;
        Up = cur.Up;
        Pressed = prev.Up && cur.Down;
        Released = prev.Down && cur.Up;
    }

    public ShapeInputState Accumulate(ShapeInputState other)
    {
        float axis = ShapeMath.Clamp(Axis + other.Axis, 0f, 1f);
        bool down = Down || other.Down;
        bool up = Up && other.Up;
        return new(down, up, axis, Gamepad);
    }
}