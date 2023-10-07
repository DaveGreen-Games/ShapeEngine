using ShapeEngine.Lib;

namespace ShapeEngine.Input;

/*public class InputLayout
{
    public uint ID { get; private set; }
    public InputState State { get; private set; } = new();
    private readonly List<IInputType> inputs;
    
    public InputLayout(uint id, params IInputType[] inputTypes)
    {
        this.ID = id;
        inputs = inputTypes.ToList();
    }
    
    public void Update(float dt, int gamepad, float axisSensitivity, float axisGravitiy)
    {
        InputState current = new();
        foreach (var input in inputs)
        {
            var state = input.GetState(gamepad);
            current = current.Accumulate(state);
        }
        State = new(State, current);
        
        if (axisSensitivity > 0 || axisGravitiy > 0)
        {
            int raw = MathF.Sign(State.AxisRaw);
            float dif = State.AxisRaw - State.Axis;
            int difSign = MathF.Sign(dif);

            if (difSign != 0)
            {
                var axisChange = 0f;
                var snapValue = 0f;
                if (difSign > 1 || difSign < -1) //snap
                {
                    snapValue = -State.AxisRaw;//snapping to 0
                    axisChange = difSign * axisSensitivity * dt;
                }
                else //move
                {
                    if (raw == 0)//gravity
                    {
                        axisChange = difSign * axisGravitiy * dt;
                    }
                    else//sensitivity
                    {
                        axisChange = difSign * axisSensitivity * dt;
                    }
                }

                if (axisChange != 0f)
                {

                    if (MathF.Abs(axisChange) > MathF.Abs(dif)) axisChange = dif - snapValue;
                    State = State.AdjustAxis(axisChange + snapValue);
                }
            }
        }
    }
}*/

public class InputAction
{
    public uint ID { get; private set; }
    public uint AccessTag { get; private set; } = ShapeInput.AllAccessTag;
    
    public int Gamepad = -1;

    private float axisSensitivity = 1f;
    private float axisGravitiy = 1f;
    /// <summary>
    /// How fast an axis moves towards the max value (1 / -1) in seconds.
    /// Used for calculating InputState.Axis values.
    /// </summary>
    public float AxisSensitivity 
    {
        get => axisSensitivity;
        set => axisSensitivity = MathF.Max(0f, value);
    }
    /// <summary>
    /// How fast an axis moves towards 0 after no input is detected.
    /// Used for calculating InputState.Axis values.
    /// </summary>
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
    
    //TODO filter system for getting the names of inputs based on input device
    private readonly List<IInputType> inputs = new();

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
        inputs.AddRange(inputTypes);
    }
    public InputAction(uint accessTag, params IInputType[] inputTypes)
    {
        ID = ShapeID.NextID;
        AccessTag = accessTag;
        inputs.AddRange(inputTypes);
    }
    public InputAction(uint accessTag, uint id, params IInputType[] inputTypes)
    {
        ID = id;
        AccessTag = accessTag;
        inputs.AddRange(inputTypes);
    }
    public InputAction(uint accessTag, uint id, int gamepad, params IInputType[] inputTypes)
    {
        ID = id;
        AccessTag = accessTag;
        Gamepad = gamepad;
        inputs.AddRange(inputTypes);
    }

    public void Update(float dt)
    {
        InputState current = new();
        foreach (var input in inputs)
        {
            var state = input.GetState(Gamepad);
            current = current.Accumulate(state);
        }
        State = new(State, current);
        
        if (axisSensitivity > 0 || axisGravitiy > 0)
        {
            int raw = MathF.Sign(State.AxisRaw);
            float dif = State.AxisRaw - State.Axis;
            int difSign = MathF.Sign(dif);

            if (difSign != 0)
            {
                var axisChange = 0f;
                //var snapValue = 0f;
                if (dif is > 1 or < -1) //snap
                {
                    axisChange += -State.Axis;//snapping to 0
                    axisChange += difSign * AxisSensitivity * dt;
                }
                else //move
                {
                    if (raw == 0)//gravity
                    {
                        axisChange += difSign * AxisGravity * dt;
                    }
                    else//sensitivity
                    {
                        axisChange += difSign * AxisSensitivity * dt;
                    }
                }

                if (axisChange != 0f)
                {
                    // float totalChange = axisChange + snapValue;
                    // if (MathF.Abs(totalChange) > MathF.Abs(dif)) totalChange = dif;
                    // State = State.AdjustAxis(totalChange);
                    if (MathF.Abs(axisChange) > MathF.Abs(dif)) axisChange = dif;
                    State = State.AdjustAxis(axisChange);
                }
            }
        }
    }

    /// <summary>
    /// Copies this instance and all inputs contained. A new ID is generated for the copy.
    /// </summary>
    /// <returns></returns>
    public InputAction Copy()
    {
        var copied = GetInputsCopied().ToArray();
        var copy = new InputAction(AccessTag, ShapeID.NextID, Gamepad, copied)
        {
            axisSensitivity = axisSensitivity,
            axisGravitiy = axisGravitiy
        };
        return copy;
    }
    public List<IInputType> GetInputs()
    {
        var list = new List<IInputType>();
        list.AddRange(inputs);
        return list;
    }
    public List<IInputType> GetInputsCopied()
    {
        var list = new List<IInputType>();
        foreach (var input in inputs)
        {
            list.Add(input.Copy());
        }
        return list;
    }
    public List<IInputType> GetInputs(InputDevice filter)
    {
        if (inputs.Count <= 0) return new();
        
        var filtered = new List<IInputType>();
        foreach (var input in inputs)
        {
            if(input.GetInputDevice() == filter)filtered.Add(input);
        }
        return filtered;
    }
    public List<IInputType> GetInputsCopied(InputDevice filter)
    {
        if (inputs.Count <= 0) return new();
        
        var filtered = new List<IInputType>();
        foreach (var input in inputs)
        {
            if(input.GetInputDevice() == filter)filtered.Add(input.Copy());
        }
        return filtered;
    }

    public void ClearInputs() => inputs.Clear();
    public bool RemoveInput(IInputType inputType) => inputs.Remove(inputType);
    public List<IInputType> RemoveInputs(InputDevice filter)
    {
        if (inputs.Count <= 0) return new();
        var removed = new List<IInputType>();
        for (int i = inputs.Count - 1; i >= 0; i--)
        {
            var input = inputs[i];
            if (input.GetInputDevice() == filter)
            {
                removed.Add(input);
                inputs.RemoveAt(i);
            }
        }
        return removed;
    }
    public void AddInput(IInputType newType) => inputs.Add(newType);
    public void AddInputs(params IInputType[] inputTypes) => inputs.AddRange(inputTypes);
    public bool HasInput(IInputType inputType) => inputs.Contains(inputType);
    public bool HasInput(InputDevice inputDevice)
    {
        foreach (var input in inputs)
        {
            if (input.GetInputDevice() == inputDevice) return true;
        }

        return false;
    }
    
    
    
    public static IInputType CreateInputType(ShapeKeyboardButton button) => new InputTypeKeyboardButton(button);
    public static IInputType CreateInputType(ShapeMouseButton button) => new InputTypeMouseButton(button);
    public static IInputType CreateInputType(ShapeGamepadButton button, float deadzone = 0.2f) => new InputTypeGamepadButton(button, deadzone);
    public static IInputType CreateInputType(ShapeKeyboardButton neg, ShapeKeyboardButton pos) => new InputTypeKeyboardButtonAxis(neg, pos);
    public static IInputType CreateInputType(ShapeMouseButton neg, ShapeMouseButton pos) => new InputTypeMouseButtonAxis(neg, pos);
    public static IInputType CreateInputType(ShapeGamepadButton neg, ShapeGamepadButton pos, float deadzone = 0.2f) => new InputTypeGamepadButtonAxis(neg, pos, deadzone);
    public static IInputType CreateInputType(ShapeMouseWheelAxis mouseWheelAxis) => new InputTypeMouseWheelAxis(mouseWheelAxis);
    public static IInputType CreateInputType(ShapeMouseAxis mouseAxis) => new InputTypeMouseAxis(mouseAxis);
    public static IInputType CreateInputType(ShapeGamepadAxis gamepadAxis, float deadzone = 0.2f) => new InputTypeGamepadAxis(gamepadAxis, deadzone);
}