using ShapeEngine.Lib;

namespace ShapeEngine.Input;


public class InputAction
{
    public uint ID { get; private set; }
    public uint AccessTag { get; private set; } = ShapeInput.AllAccessTag;
    
    public int Gamepad = -1;
    
    public float AxisSensitivity { get; set; } = 1f;
    public float AxisGravity { get; set; } = 1f;
    
    public InputState State { get; private set; } = new();
    public InputState Consume()
    {
        var returnValue = State;
        State = State.Consume();
        return returnValue;
    }
    
    public readonly List<IInputType> Inputs = new();

    public InputAction()
    {
        ID = ShapeID.NextID;
    }
    public InputAction(uint accessTag)
    {
        ID = ShapeID.NextID;
        AccessTag = accessTag;
    }
    public InputAction(uint accessTag, int gamepad)
    {
        ID = ShapeID.NextID;
        Gamepad = gamepad;
        AccessTag = accessTag;
    }
    public InputAction(uint accessTag, uint id)
    {
        ID = id;
        AccessTag = accessTag;
    }
    public InputAction(uint accessTag, uint id, int gamepad)
    {
        ID = id;
        AccessTag = accessTag;
        Gamepad = gamepad;
    }
    public InputAction(params IInputType[] inputTypes)
    {
        ID = ShapeID.NextID;
        Inputs.AddRange(inputTypes);
    }
    public InputAction(uint accessTag, params IInputType[] inputTypes)
    {
        ID = ShapeID.NextID;
        AccessTag = accessTag;
        Inputs.AddRange(inputTypes);
    }
    public InputAction(uint accessTag, uint id, params IInputType[] inputTypes)
    {
        ID = id;
        AccessTag = accessTag;
        Inputs.AddRange(inputTypes);
    }
    public InputAction(uint accessTag, uint id, int gamepad, params IInputType[] inputTypes)
    {
        ID = id;
        AccessTag = accessTag;
        Gamepad = gamepad;
        Inputs.AddRange(inputTypes);
    }

    public void Update(float dt)
    {
        InputState current = new();
        foreach (var input in Inputs)
        {
            var state = input.GetState(Gamepad);
            current = current.Accumulate(state);
        }

        // float axis = current.Axis;
        // if (State.Axis == 0f && AxisSensitivity > 0)//prev was at 0
        // {
        //     if (axis > 0f)
        //     {
        //         axis = MathF.Min(1f, State.Axis + (1f / AxisSensitivity) * dt);
        //     }
        //     else if(axis < 0)
        //     {
        //         axis = MathF.Max(-1f, State.Axis - (1f / AxisSensitivity) * dt);
        //     }
        // }
        // else if(AxisGravity > 0)
        // {
        //     if (axis > 0f)
        //     {
        //         axis = MathF.Max(0f, State.Axis - (1f / AxisGravity) * dt);
        //     }
        //     else if(axis < 0)
        //     {
        //         axis = MathF.Min(0f, State.Axis + (1f / AxisGravity) * dt);
        //     }
        // }
        
        State = new(State, current);
    }
    
    
    public static IInputType CreateInputType(ShapeKeyboardButton button) => new InputTypeKeyboardButton(button);
    public static IInputType CreateInputType(ShapeMouseButton button) => new InputTypeMouseButton(button);
    public static IInputType CreateInputType(ShapeGamepadButton button, float deadzone = 0.2f) => new InputTypeGamepadButton(button, deadzone);
    public static IInputType CreateInputType(ShapeKeyboardButton neg, ShapeKeyboardButton pos) => new InputTypeKeyboardButtonAxis(neg, pos);
    public static IInputType CreateInputType(ShapeMouseButton neg, ShapeMouseButton pos) => new InputTypeMouseButtonAxis(neg, pos);
    public static IInputType CreateInputType(ShapeGamepadButton neg, ShapeGamepadButton pos, float deadzone = 0.2f) => new InputTypeGamepadButtonAxis(neg, pos, deadzone);
    public static IInputType CreateInputType(ShapeMouseWheelAxis mouseWheelAxis) => new InputTypeMouseWheelAxis(mouseWheelAxis);
    public static IInputType CreateInputType(ShapeGamepadAxis gamepadAxis, float deadzone = 0.2f) => new InputTypeGamepadAxis(gamepadAxis, deadzone);
}