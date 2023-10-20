using System.IO.Pipes;
using System.Numerics;
using System.Text;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Lib;

namespace ShapeEngine.Input;

public class InputAction
{
    public uint ID { get; private set; }
    public uint AccessTag { get; private set; } = ShapeInput.AllAccessTag;
    
    public int Gamepad = -1;
    public string Title = "Input Action";

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

    // public InputDevice LastInputDevice { get; private set; } = InputDevice.Keyboard;
    // private float lastInputDeviceMagnitude = 0f;
    public InputState State { get; private set; } = new();

    public void ClearState()
    {
        State = new InputState(false, true, 0f, Gamepad, InputDevice.Keyboard);
    }
    public InputState Consume()
    {
        var returnValue = State;
        State = State.Consume();
        return returnValue;
    }
    
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
        // lastInputDeviceMagnitude = 0f;
        
        InputState current = new();
        foreach (var input in inputs)
        {
            var state = input.GetState(Gamepad);
            // if (state.Down)
            // {
            //     float magnitude = MathF.Abs(state.AxisRaw);
            //     if (magnitude > lastInputDeviceMagnitude)
            //     {
            //         LastInputDevice = input.GetInputDevice();
            //         lastInputDeviceMagnitude = magnitude;
            //     }
            // }
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
                float gravity = axisGravitiy <= 0f ? 0f : 1f / axisGravitiy;
                float sensitivity = axisSensitivity <= 0f ? 0f : 1f / axisSensitivity;
                if (dif is > 1 or < -1) //snap
                {
                    axisChange += -State.Axis;//snapping to 0
                    axisChange += difSign * sensitivity * dt;
                }
                else //move
                {
                    if (raw == 0)//gravity
                    {
                        axisChange += difSign * gravity * dt;
                    }
                    else//sensitivity
                    {
                        axisChange += difSign * sensitivity * dt;
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
    public List<IInputType> GetInputs(int maxCount)
    {
        if (inputs.Count <= 0) return new();
        
        var list = new List<IInputType>();
        if(maxCount > 0 && maxCount < inputs.Count) list.AddRange(inputs.GetRange(0, maxCount));
        else list.AddRange(inputs);
        return list;
    }
    public List<IInputType> GetInputs(InputDevice filter, int maxCount)
    {
        if (inputs.Count <= 0) return new();
        
        var filtered = new List<IInputType>();
        foreach (var input in inputs)
        {
            if (filtered.Count >= maxCount && maxCount > 0) return filtered;
            if(input.GetInputDevice() == filter)filtered.Add(input);
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

    
    /// <summary>
    /// Generate a description for this action based on the parameters. Layout-> "Title: [type a][type b][type c] ..."
    /// </summary>
    /// <param name="device">Only input types of the specified device are used.</param>
    /// <param name="shorthand">Should the shorthand name or full name of the input type be used?</param>
    /// <param name="count">Limits the amount input types used. If count is smaller or equal to 0 all input types are used.</param>
    /// <param name="useTitle">Should the title of this input action be used as a prefix? "Title: [input type]"</param>
    /// <returns>The combined names of all input types.</returns>
    public string GetInputTypeDescription(InputDevice device, bool shorthand, int count = 1, bool useTitle = false)
    {
        StringBuilder b = new();
        if(useTitle) b.Append(Title);
        var inputNames = GetInputTypeNames(device, shorthand, count);
        if (inputNames.Count > 0)
        {
            if(useTitle) b.Append(": ");
            foreach (string inputName in inputNames)
            {
                b.Append($"[{inputName}]");
            }
        }
        return b.ToString();
    }
    /// <summary>
    /// Generate a description for this action based on the parameters. Layout-> "Title: [type a][type b][type c] ..."
    /// </summary>
    /// <param name="shorthand">Should the shorthand name or full name of the input type be used?</param>
    /// <param name="count">Limits the amount input types used. If count is smaller or equal to 0 all input types are used.</param>
    /// <param name="useTitle">Should the title of this input action be used as a prefix? "Title: [input type]"</param>
    /// <returns>The combined names of all input types.</returns>
    public string GetInputTypeDescription(bool shorthand, int count = 1, bool useTitle = false)
    {
        StringBuilder b = new();
        if(useTitle) b.Append(Title);
        var inputNames = GetInputTypeNamesLimited(shorthand, count);
        if (inputNames.Count > 0)
        {
            if(useTitle) b.Append(": ");
            foreach (string inputName in inputNames)
            {
                b.Append($"[{inputName}]");
            }
        }
        return b.ToString();
    }
    /// <summary>
    /// Generate a description for this action based on the parameters. Layout-> "Title: [type a][type b][type c] ..."
    /// </summary>
    /// <param name="shorthand">Should the shorthand name or full name of the input type be used?</param>
    /// <param name="count">Limits the amount input types used per input device.
    /// A count of 1 means 1 input type per available input device is used.
    /// If count is smaller or equal to 0 all input types are used.</param>
    /// <param name="useTitle">Should the title of this input action be used as a prefix? "Title: [input type]"</param>
    /// <returns>The combined names of all input types.</returns>
    public string GetInputTypeDescriptionPerDevice(bool shorthand, int count = 1, bool useTitle = false)
    {
        StringBuilder b = new();
        if(useTitle) b.Append(Title);
        var inputNames = new List<string>();
        inputNames.AddRange(GetInputTypeNames(InputDevice.Keyboard, shorthand, count));
        inputNames.AddRange(GetInputTypeNames(InputDevice.Mouse, shorthand, count));
        inputNames.AddRange(GetInputTypeNames(InputDevice.Gamepad, shorthand, count));
        
        if (inputNames.Count > 0)
        {
            if(useTitle) b.Append(": ");
            foreach (string inputName in inputNames)
            {
                b.Append($"[{inputName}]");
            }
        }
        return b.ToString();
    }
    /// <summary>
    /// Get the names of all input types used in this input action.
    /// </summary>
    /// <param name="device">Only input types of the specified device are used.</param>
    /// <param name="shorthand">Should the shorthand name or full name of the input type be used?</param>
    /// <param name="count">Limits the amount input types used. If count is smaller or equal to 0 all input types are used.</param>
    /// <returns>A list of all the input type names found in the action based on the parameters</returns>
    public List<string> GetInputTypeNames(InputDevice device, bool shorthand = true, int count = -1)
    {
        var inputs = GetInputs(device, count);
        var names = new List<string>();
        foreach (var input in inputs)
        {
            //if (names.Count >= count && count > 0) return names;
            names.Add(input.GetName(shorthand));
        }
        return names;
    }
    /// <summary>
    /// Get the names of all input types used in this input action.
    /// </summary>
    /// <param name="shorthand">Should the shorthand name or full name of the input type be used?</param>
    /// <param name="count">Limits the amount input types used. If count is smaller or equal to 0 all input types are used.</param>
    /// <returns>A list of all the input type names found in the action based on the parameters</returns>
    public List<string> GetInputTypeNamesLimited(bool shorthand = true, int count = -1)
    {
        var inputs = GetInputs(count);
        var names = new List<string>();
        foreach (var input in inputs)
        {
            names.Add(input.GetName(shorthand));
        }
        return names;
    }
    /// <summary>
    /// Get the names of all input types used in this input action.
    /// </summary>
    /// <param name="shorthand">Should the shorthand name or full name of the input type be used?</param>
    /// <param name="count">Limits the amount input types used per input device. If count is smaller or equal to 0 all input types are used.</param>
    /// <returns>A list of all the input type names found in the action based on the parameters</returns>
    public List<string> GetInputTypeNamesLimitedPerDevice(bool shorthand = true, int count = -1)
    {
        var names = new List<string>();
        names.AddRange(GetInputTypeNames(InputDevice.Keyboard, shorthand, count));
        names.AddRange(GetInputTypeNames(InputDevice.Mouse, shorthand, count));
        names.AddRange(GetInputTypeNames(InputDevice.Gamepad, shorthand, count));
        
        foreach (var input in inputs)
        {
            names.Add(input.GetName(shorthand));
        }
        return names;
    }
    /// <summary>
    /// Get a list of a input device and name for each input type used in this action.
    /// </summary>
    /// <param name="shorthand">Should the shorthand name or full name of the input type be used?</param>
    /// <returns>A list of all the input types device and name found</returns>
    public List<InputName> GetInputTypeNames(bool shorthand = true)
    {
        var inputs = GetInputs();
        var names = new List<InputName>();
        foreach (var input in inputs)
        {
            var name = input.GetName(shorthand);
            var type = input.GetInputDevice();
            var inputName = new InputName(name, type);
            names.Add(inputName);
        }

        return names;

    }
    
}
