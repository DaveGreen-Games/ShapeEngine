using ShapeEngine.Lib;

namespace ShapeEngine.Input;


public class InputAction
{
    public uint ID { get; private set; }
    public uint AccessTag { get; private set; } = ShapeInput.AllAccessTag;
    
    public int Gamepad = -1;

    private float axisSensitivity = 1f;
    private float axisGravitiy = 1f;
    public float AxisSensitivity 
    {
        get => axisSensitivity;
        set => axisSensitivity = MathF.Max(0f, value);
    }
    public float AxisGravity 
    {
        get => axisGravitiy;
        set => axisGravitiy = MathF.Max(0f, value);
    }
    
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
        State = new(State, current);

        if (axisSensitivity > 0 || axisGravitiy > 0)
        {
            int raw = MathF.Sign(State.AxisRaw);
            int exact = MathF.Sign(State.Axis);
            int dif = raw - exact;

            if (dif != 0)
            {
                var axisChange = 0f;
                if (dif > 1 || dif < -1) //snap
                {
                    axisChange = -State.Axis;//snapping to 0
                    axisChange += dif * AxisSensitivity * dt;
                }
                else //move
                {
                    if (raw == 0)//gravity
                    {
                        axisChange = dif * AxisGravity * dt;
                    }
                    else//sensitivity
                    {
                        axisChange = dif * AxisSensitivity * dt;
                    }
                }
            
                if(axisChange != 0f) State = State.AdjustAxis(axisChange);
            }
        }
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