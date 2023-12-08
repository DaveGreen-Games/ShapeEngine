using ShapeEngine.Lib;

namespace ShapeEngine.Input;


public enum MultiTapState
{
    None = 0,
    InProgress = 1,
    Completed = 2,
    Failed = 3
}

public enum PressedType
{
    None = 0,
    Hold = 1,
    MultiTap = 2,
    SingleTap = 3
}
public readonly struct InputState
{
    public readonly bool Down;
    public readonly bool Up;
    public readonly bool Released;
    public readonly bool Pressed;

    public readonly float Axis;
    public readonly float AxisRaw;
    public readonly int Gamepad;
    public readonly InputDeviceType InputDeviceType;
    
    public readonly bool Consumed;

    public readonly float HoldF;
    public readonly MultiTapState HoldState;
    //public readonly bool HoldFinished;
    public readonly float MultiTapF;
    public readonly MultiTapState MultiTapState;

    public readonly PressedType GetPressedType()
    {
        if (HoldState == MultiTapState.Completed) return PressedType.Hold;
        if (MultiTapState == MultiTapState.Completed) return PressedType.MultiTap;
        
        if (HoldF <= 0f && MultiTapState == MultiTapState.Failed) return PressedType.SingleTap;
        if (MultiTapF <= 0f && HoldState == MultiTapState.Failed) return PressedType.SingleTap;
        
        
        return PressedType.None;

    }
    
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
        //HoldFinished = false;
        MultiTapF = 0f;
        MultiTapState = MultiTapState.None;
    }
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
    public InputState Accumulate(InputState other)
    {
        var inputDevice = InputDeviceType;
        if (other.Down)
        {
            if(!Down || MathF.Abs(other.AxisRaw) > MathF.Abs(AxisRaw))inputDevice = other.InputDeviceType;
        }

        float axis = AxisRaw; // ShapeMath.Clamp(AxisRaw + other.AxisRaw, -1f, 1f); // MathF.Max(AxisRaw, other.AxisRaw); // ShapeMath.Clamp(Axis + other.Axis, 0f, 1f);
        if (MathF.Abs(other.AxisRaw) > MathF.Abs((AxisRaw))) axis = other.AxisRaw;
        bool down = Down || other.Down;
        bool up = Up && other.Up;

        //float holdF = MathF.Max(HoldF, other.HoldF);
        
        //bool holdFinished = HoldFinished || other.HoldFinished;
        //bool doubleTap = DoubleTap || other.DoubleTap;
        
        return new(down, up, axis, other.Gamepad, inputDevice);
        // InputState newState = new(down, up, axis, other.Gamepad, inputDevice);
        // return new(newState, holdF, 0, false, false);
    }
    public InputState AdjustAxis(float value) => value == 0f ? this : new InputState(this, Axis + value);
    public InputState Consume() => new(this, true);
}