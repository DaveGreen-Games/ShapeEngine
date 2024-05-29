using System.Text;
using ShapeEngine.Core;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Input;

public class InputAction
{
    #region Members

    public bool Enabled = true;
    public uint ID { get; private set; }
    public uint AccessTag { get; private set; } = DefaultAccessTag;

    public ShapeGamepadDevice? Gamepad = null;
    public string Title = "Input Action";

    private float holdTimer = 0f;
    private float multiTapTimer = 0f;
    private float holdDuration = 1f;
    private float multiTapDuration = 0.25f;
    private int multiTapCount = 0;
    private int multiTapTarget = 0;

    public int MultiTapTarget
    {
        get => multiTapTarget;
        set => multiTapTarget = ShapeMath.MaxInt(0, value);
    }
    public float HoldDuration
    {
        get => holdDuration;
        set => holdDuration = MathF.Max(0f, value);
    }
    public float MultiTapDuration
    {
        get => multiTapDuration;
        set => multiTapDuration = MathF.Max(0f, value);
    }
    
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

    private InputState state = new();
    public InputState State
    {
        get
        {
            if (Locked && !HasAccess(AccessTag)) return new();
            if (!Enabled) return new();

            return state;
        }
        private set => state = value;
        
    }
    private readonly List<IInputType> inputs = new();
    #endregion

    #region Constructors
    public InputAction()
    {
        ID = ShapeID.NextID;
    }
    public InputAction(uint accessTag)
    {
        ID = ShapeID.NextID;
        AccessTag = accessTag;
    }
    public InputAction(uint accessTag, ShapeGamepadDevice gamepad)
    {
        ID = ShapeID.NextID;
        Gamepad = gamepad;
        AccessTag = accessTag;
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
    
    public InputAction(uint accessTag, ShapeGamepadDevice gamepad, params IInputType[] inputTypes)
    {
        ID = ShapeID.NextID;
        AccessTag = accessTag;
        Gamepad = gamepad;
        inputs.AddRange(inputTypes);
    }
    #endregion

    #region Class

    public int GetGamepadIndex() => Gamepad?.Index ?? -1;
    public void ClearState()
    {
        State = new InputState(false, true, 0f, GetGamepadIndex(), InputDeviceType.Keyboard);
    }
    public InputState Consume()
    {
        var returnValue = State;
        State = State.Consume();
        return returnValue;
    }

    public void Update(float dt)
    {
        //Accumulate All Input States From All Input Types
        InputState current = new();
        foreach (var input in inputs)
        {
            var state = input.GetState(Gamepad);
            current = current.Accumulate(state);
        }

        
        //Multi Tap & Hold System
        float multiTapF = 0f;
        if (multiTapTimer > 0f)
        {
            multiTapTimer -= dt;
            if (multiTapTimer <= 0f)
            {
                multiTapTimer = 0f;
                multiTapCount = 0;
            }
        }
        
        float holdF = 0f;
        if (State.Up && current.Down) //pressed
        {
            if (holdDuration > 0)
            {
                holdTimer = holdDuration;
            }
            if (multiTapDuration > 0f && multiTapTarget > 1)
            {
                if (multiTapTimer <= 0f) multiTapTimer = multiTapDuration;
                multiTapCount++;
            }

        }
        else if (State.Down && current.Up) //released
        {
            if (holdTimer > 0f)
            {
                holdTimer = 0f;
                holdF = 0f;
            }
            
        }

        if (holdTimer > 0f)
        {
            holdTimer -= dt;
            if (holdTimer <= 0)
            {
                holdTimer = 0f;
                holdF = 1f;
            }
            else holdF = 1f - (holdTimer / holdDuration);
        }
        
        if (multiTapTimer > 0f)
        {
            if (multiTapCount >= multiTapTarget)
            {
                multiTapF = 1f;
                if (holdTimer > 0f)
                {
                    holdTimer = 0f;
                    holdF = 0f;
                }
            }
            else multiTapF = (float)multiTapCount / (float)multiTapTarget;
        }
        
        //Calculate New State
        current = new(current, holdF, multiTapF);
        State = new(State, current);
        
        //Reset Multitap Count On Successful Multitap
        if (multiTapF >= 1f) multiTapCount = 0;
        
        //Axis Sensitivity & Gravity
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
        var copy = new InputAction(AccessTag, Gamepad, copied)
        {
            axisSensitivity = axisSensitivity,
            axisGravitiy = axisGravitiy,
            Enabled = this.Enabled
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
    public List<IInputType> GetInputs(InputDeviceType filter)
    {
        if (inputs.Count <= 0) return new();
        
        var filtered = new List<IInputType>();
        foreach (var input in inputs)
        {
            if(input.GetInputDevice() == filter)filtered.Add(input);
        }
        return filtered;
    }
    public List<IInputType> GetInputsCopied(InputDeviceType filter)
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
    public List<IInputType> GetInputs(InputDeviceType filter, int maxCount)
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
    public List<IInputType> RemoveInputs(InputDeviceType filter)
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
    public bool HasInput(InputDeviceType inputDeviceType)
    {
        foreach (var input in inputs)
        {
            if (input.GetInputDevice() == inputDeviceType) return true;
        }

        return false;
    }

    
    //TODO add encapsulation parameter $"[ ]" somehow to change bracket layout etc -> add to all similar functions
    /// <summary>
    /// Generate a description for this action based on the parameters. Layout-> "Title: [type a][type b][type c] ..."
    /// </summary>
    /// <param name="deviceType">Only input types of the specified device are used.</param>
    /// <param name="shorthand">Should the shorthand name or full name of the input type be used?</param>
    /// <param name="count">Limits the amount input types used. If count is smaller or equal to 0 all input types are used.</param>
    /// <param name="useTitle">Should the title of this input action be used as a prefix? "Title: [input type]"</param>
    /// <param name="brackets">Should the input type be encapsulated in square brackets ["Input Type"]?"</param>
    /// <returns>The combined names of all input types.</returns>
    public string GetInputTypeDescription(InputDeviceType deviceType, bool shorthand, int count = 1, bool useTitle = false, bool brackets = true)
    {
        StringBuilder b = new();
        if(useTitle) b.Append(Title);
        var inputNames = GetInputTypeNames(deviceType, shorthand, count);
        if (inputNames.Count > 0)
        {
            if(useTitle) b.Append(": ");
            foreach (string inputName in inputNames)
            {
                if(brackets) b.Append($"[{inputName}]");
                else b.Append(inputName);
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
        inputNames.AddRange(GetInputTypeNames(InputDeviceType.Keyboard, shorthand, count));
        inputNames.AddRange(GetInputTypeNames(InputDeviceType.Mouse, shorthand, count));
        inputNames.AddRange(GetInputTypeNames(InputDeviceType.Gamepad, shorthand, count));
        
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
    /// <param name="deviceType">Only input types of the specified device are used.</param>
    /// <param name="shorthand">Should the shorthand name or full name of the input type be used?</param>
    /// <param name="count">Limits the amount input types used. If count is smaller or equal to 0 all input types are used.</param>
    /// <returns>A list of all the input type names found in the action based on the parameters</returns>
    public List<string> GetInputTypeNames(InputDeviceType deviceType, bool shorthand = true, int count = -1)
    {
        var inputs = GetInputs(deviceType, count);
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
        names.AddRange(GetInputTypeNames(InputDeviceType.Keyboard, shorthand, count));
        names.AddRange(GetInputTypeNames(InputDeviceType.Mouse, shorthand, count));
        names.AddRange(GetInputTypeNames(InputDeviceType.Gamepad, shorthand, count));
        
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
    #endregion

    #region Static

    #region Members

    public static readonly uint AllAccessTag = NextTag; // BitFlag.GetFlagUint(31);
    public static readonly uint DefaultAccessTag = NextTag; // BitFlag.GetFlagUint(30);
    public static bool Locked { get; private set; } = false;
    private static BitFlag lockWhitelist;
    private static BitFlag lockBlacklist;
    private static uint tagPowerCounter = 1;
    public static uint NextTag => BitFlag.GetFlagUint(tagPowerCounter++);
    #endregion
    
    
    #region Lock System
    public static void Lock()
    {
        Locked = true;
        lockWhitelist = BitFlag.Clear();
        lockBlacklist = BitFlag.Clear();
    }

    public static void Lock(BitFlag whitelist, BitFlag blacklist)
    {
        Locked = true;
        lockWhitelist = whitelist;
        lockBlacklist = blacklist;
        // lockWhitelist.Clear();
        // lockBlacklist.Clear();
        // if(whitelist.Length > 0) lockWhitelist.AddRange(whitelist);
        // if(blacklist.Length > 0) lockWhitelist.AddRange(blacklist);
    }
    public static void LockWhitelist(BitFlag whitelist)
    {
        Locked = true;
        lockWhitelist = whitelist;
        lockBlacklist = BitFlag.Clear();
        // lockWhitelist.Clear();
        // lockBlacklist.Clear();
        // if(whitelist.Length > 0) lockWhitelist.AddRange(whitelist);

    }
    public static void LockBlacklist(BitFlag blacklist)
    {
        Locked = true;
        lockBlacklist = blacklist;
        lockWhitelist = BitFlag.Clear();
    }
    public static void Unlock()
    {
        Locked = false;
        lockWhitelist = BitFlag.Clear();
        lockBlacklist = BitFlag.Clear();
    }
    public static bool HasAccess(uint tag)
    {
        if (tag == AllAccessTag) return true;
        return (lockWhitelist.IsEmpty() || lockWhitelist.Has(tag)) && !lockBlacklist.Has(tag);
        // return tag == AllAccessTag || (lockWhitelist.Has(tag) && !lockBlacklist.Has(tag));
        // return tag == AllAccessTag || (lockWhitelist.Contains(tag) && !lockBlacklist.Contains(tag));
    }

    public static bool HasAccess(InputAction action) => HasAccess(action.AccessTag);
    #endregion
    
    #region Input Actions
    public static void UpdateActions(float dt, ShapeGamepadDevice? gamepad, params InputAction[] actions)
    {
        foreach (var action in actions)
        {
            action.Gamepad = gamepad;
            action.Update(dt);
        }
    }
    public static void UpdateActions(float dt, ShapeGamepadDevice? gamepad, List<InputAction> actions)
    {
        foreach (var action in actions)
        {
            action.Gamepad = gamepad;
            action.Update(dt);
        }
    }
    public static List<string> GetActionDescriptions(InputDeviceType inputDeviceType, bool shorthand, int typesPerActionCount, List<InputAction> actions)
    {
        var final = new List<string>();
        foreach (var action in actions)
        {
            var description = action.GetInputTypeDescription(inputDeviceType, shorthand, typesPerActionCount, true);
            
            final.Add(description);
        }

        return final;
    }
    public static List<string> GetActionDescriptions(InputDeviceType inputDeviceType, bool shorthand, int typesPerActionCount, params InputAction[] actions)
    {
        var final = new List<string>();
        foreach (var action in actions)
        {
            var description = action.GetInputTypeDescription(inputDeviceType, shorthand, typesPerActionCount, true);
            
            final.Add(description);
        }

        return final;
    }

    #endregion
   
    #region Basic
    public static InputState GetState(ShapeKeyboardButton button, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ShapeInput.KeyboardDevice.CreateInputState(button);// InputTypeKeyboardButton.GetState(button);
    }
    public static InputState GetState(ShapeMouseButton button, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ShapeInput.MouseDevice.CreateInputState(button); // InputTypeMouseButton.GetState(button);
    }
    public static InputState GetState(ShapeGamepadButton button, uint accessTag, int gamepadIndex, float deadzone = 0.2f)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        var gamepad = ShapeInput.GamepadDeviceManager.GetGamepad(gamepadIndex);
        return gamepad == null ? new() : gamepad.CreateInputState(button, deadzone); //  InputTypeGamepadButton.GetState(button, gamepad, deadzone);
    }
    public static InputState GetState(ShapeKeyboardButton neg, ShapeKeyboardButton pos, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ShapeInput.KeyboardDevice.CreateInputState(neg, pos); // InputTypeKeyboardButtonAxis.GetState(neg, pos);
    }
    public static InputState GetState(ShapeMouseButton neg, ShapeMouseButton pos, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ShapeInput.MouseDevice.CreateInputState(neg, pos); // InputTypeMouseButtonAxis.GetState(neg, pos);
    }
    public static InputState GetState(ShapeGamepadButton neg, ShapeGamepadButton pos, uint accessTag, int gamepadIndex, float deadzone = 0.2f)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        var gamepad = ShapeInput.GamepadDeviceManager.GetGamepad(gamepadIndex);
        return gamepad == null ? new() : gamepad.CreateInputState(neg, pos, deadzone);
        // return InputTypeGamepadButtonAxis.GetState(neg, pos, gamepad, deadzone);
    }
    public static InputState GetState(ShapeMouseWheelAxis axis, uint accessTag, float deadzone = 1f)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ShapeInput.MouseDevice.CreateInputState(axis, deadzone); // InputTypeMouseWheelAxis.GetState(axis);
    }
    public static InputState GetState(ShapeGamepadAxis axis, uint accessTag, int gamepadIndex, float deadzone = 0.2f)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        var gamepad = ShapeInput.GamepadDeviceManager.GetGamepad(gamepadIndex);
        return gamepad == null ? new() : gamepad.CreateInputState(axis, deadzone);
        // return InputTypeGamepadAxis.GetState(axis, gamepad, deadzone);
    }
    
    #endregion
    
    #endregion
}
