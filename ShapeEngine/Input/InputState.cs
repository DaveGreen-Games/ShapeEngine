using ShapeEngine.Lib;

namespace ShapeEngine.Input;



public readonly struct InputState
{
    public readonly bool Down;
    public readonly bool Up;
    public readonly bool Released;
    public readonly bool Pressed;

    public readonly float Axis;
    public readonly float AxisRaw;
    public readonly int Gamepad;
    public readonly InputDevice InputDevice;
    
    public readonly bool Consumed;

    public readonly float HoldF;
    public readonly bool HoldFinished;
    public readonly int MultiTapCount;
    
    
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
        InputDevice = InputDevice.Keyboard;
        HoldF = 0f;
        HoldFinished = false;
        MultiTapCount = 0;
    }
    public InputState(bool down, bool up, float axisRaw, int gamepad, InputDevice inputDevice)
    {
        Down = down;
        Up = up;
        Released = false;
        Pressed = false;
        AxisRaw = axisRaw;
        Axis = axisRaw;
        Gamepad = gamepad;
        Consumed = false;
        InputDevice = inputDevice;
        HoldF = 0f;
        HoldFinished = false;
        MultiTapCount = 0;
    }

    public InputState(InputState state, float holdF, int multiTapCount, bool multiTapFinished, bool multiTapFailed)
    {
        if (multiTapFailed) //failed
        {
            Down = true;
            Up = false;
            Released = false;
            Pressed = true;
        }
        else if (multiTapCount > 0)
        {
            if (multiTapFinished)
            {
                Down = true;
                Up = false;
                Released = false;
                Pressed = true;
            }
            else
            {
                Down = false;
                Up = true;
                Released = false;
                Pressed = false;
            }
        }
        else
        {
            Down = state.Down;
            Up = state.Up;
            Released = state.Released;
            Pressed = state.Pressed;
        }
        
        
        Gamepad = state.Gamepad;
        AxisRaw = state.AxisRaw;
        Axis = state.Axis;
        Consumed = state.Consumed;
        InputDevice = state.InputDevice;

        HoldF = holdF;
        HoldFinished = false;
        if (multiTapFinished) MultiTapCount = multiTapCount;
        else MultiTapCount = 0;
        // MultiTapCount = multiTapCount;
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
        InputDevice = cur.InputDevice;

        if (prev.HoldF < 1f && cur.HoldF >= 1f) HoldFinished = true;
        else HoldFinished = false;
        HoldF = cur.HoldF;

        MultiTapCount = cur.MultiTapCount;

    }
    public InputState(InputState prev, InputState cur, InputDevice inputDevice)
    {
        Gamepad = cur.Gamepad;
        AxisRaw = cur.AxisRaw;
        Axis = prev.Axis;
        Down = cur.Down;
        Up = cur.Up;
        Pressed = prev.Up && cur.Down;
        Released = prev.Down && cur.Up;
        Consumed = false;
        InputDevice = inputDevice;

        if (prev.HoldF < 1f && cur.HoldF >= 1f) HoldFinished = true;
        else HoldFinished = false;
        HoldF = cur.HoldF;
        
        MultiTapCount = cur.MultiTapCount;
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
        InputDevice = other.InputDevice;

        HoldF = other.HoldF;
        HoldFinished = other.HoldFinished;
        MultiTapCount = other.MultiTapCount;
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
        InputDevice = state.InputDevice;
        
        HoldF = state.HoldF;
        HoldFinished = state.HoldFinished;
        MultiTapCount = state.MultiTapCount;
    }
    public InputState Accumulate(InputState other)
    {
        var inputDevice = InputDevice;
        if (other.Down)
        {
            if(!Down || MathF.Abs(other.AxisRaw) > MathF.Abs(AxisRaw))inputDevice = other.InputDevice;
        }

        float axis = AxisRaw; // ShapeMath.Clamp(AxisRaw + other.AxisRaw, -1f, 1f); // MathF.Max(AxisRaw, other.AxisRaw); // ShapeMath.Clamp(Axis + other.Axis, 0f, 1f);
        if (MathF.Abs(other.AxisRaw) > MathF.Abs((AxisRaw))) axis = other.AxisRaw;
        bool down = Down || other.Down;
        bool up = Up && other.Up;

        float holdF = MathF.Max(HoldF, other.HoldF);
        
        //bool holdFinished = HoldFinished || other.HoldFinished;
        //bool doubleTap = DoubleTap || other.DoubleTap;
        InputState newState = new(down, up, axis, other.Gamepad, inputDevice);
        return new(newState, holdF, 0, false, false);
    }
    public InputState AdjustAxis(float value) => value == 0f ? this : new InputState(this, Axis + value);
    public InputState Consume() => new(this, true);
}