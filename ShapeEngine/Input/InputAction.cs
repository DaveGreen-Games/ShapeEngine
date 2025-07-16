using System.Text;
using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Input;

/// <summary>
/// Represents an input action, which can be triggered by various input types and devices.
/// Handles state, multi-tap, hold, and axis sensitivity/gravity.
/// </summary>
public partial class InputAction
{
    /// <summary>
    /// Represents the toggle state for an input action.
    /// </summary>
    public enum ToggleState
    {
        /// <summary>
        /// The toggle is off.
        /// </summary>
        Off,
        /// <summary>
        /// The toggle is on.
        /// </summary>
        On
    }
    
    #region Members

    /// <summary>
    /// Indicates if this input action is enabled.
    /// </summary>
    public bool Enabled = true;
    /// <summary>
    /// The unique identifier for this input action.
    /// </summary>
    public uint ID { get; private set; }
    /// <summary>
    /// The access tag used for locking/unlocking this action.
    /// <remarks>
    /// Determines access of this action.
    /// Only used when input actions are locked.
    /// If this tag is contained in the lock blacklist while the lock is active, the action will not trigger.
    /// If this tag is contained in the lock whitelist while the lock is active, the action will trigger.
    /// If <see cref="AllAccessTag"/> is used the action will always trigger.
    /// </remarks>
    /// </summary>
    public uint AccessTag { get; private set; } = DefaultAccessTag;

    /// <summary>
    /// The associated gamepad device, if any.
    /// </summary>
    public GamepadDevice? Gamepad;
    /// <summary>
    /// The display title for this input action.
    /// </summary>
    public string Title = "Input Action";

    private float holdTimer;
    private float multiTapTimer;
    private float holdDuration = 1f;
    private float multiTapDuration = 0.25f;
    private int multiTapCount;
    private int multiTapTarget;

    /// <summary>
    /// The number of taps required for a multi-tap action.
    /// </summary>
    public int MultiTapTarget
    {
        get => multiTapTarget;
        set => multiTapTarget = ShapeMath.MaxInt(0, value);
    }
    /// <summary>
    /// The duration required to trigger a hold action (in seconds).
    /// Default is 1 second.
    /// </summary>
    public float HoldDuration
    {
        get => holdDuration;
        set => holdDuration = MathF.Max(0f, value);
    }
    /// <summary>
    /// The duration allowed between taps for a multi-tap action (in seconds).
    /// Default is 0.25 seconds.
    /// </summary>
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
    
    private InputState state = new();
    /// <summary>
    /// The current state of this input action.
    /// </summary>
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
    private readonly List<IInputType> inputs = [];

    /// <summary>
    /// Indicates whether the input action is toggled. Changes state on each press.
    /// </summary>
    public ToggleState Toggle { get; private set; } = ToggleState.Off;
    
    /// <summary>
    /// Resets the toggle state to off.
    /// </summary>
    public void ResetToggle() => Toggle = ToggleState.Off;
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of <see cref="InputAction"/> with a unique ID.
    /// </summary>
    public InputAction()
    {
        ID = ShapeID.NextID;
    }
    /// <summary>
    /// Initializes a new instance of <see cref="InputAction"/> with a specific access tag.
    /// </summary>
    /// <param name="accessTag">The access tag for this action.</param>
    public InputAction(uint accessTag)
    {
        ID = ShapeID.NextID;
        AccessTag = accessTag;
    }
    /// <summary>
    /// Initializes a new instance of <see cref="InputAction"/> with a specific access tag and gamepad.
    /// </summary>
    /// <param name="accessTag">The access tag for this action.</param>
    /// <param name="gamepad">The associated gamepad device.</param>
    public InputAction(uint accessTag, GamepadDevice gamepad)
    {
        ID = ShapeID.NextID;
        Gamepad = gamepad;
        AccessTag = accessTag;
    }
    /// <summary>
    /// Initializes a new instance of <see cref="InputAction"/> with input types.
    /// </summary>
    /// <param name="inputTypes">The input types for this action.</param>
    public InputAction(params IInputType[] inputTypes)
    {
        ID = ShapeID.NextID;
        inputs.AddRange(inputTypes);
    }
    /// <summary>
    /// Initializes a new instance of <see cref="InputAction"/> with a specific access tag and input types.
    /// </summary>
    /// <param name="accessTag">The access tag for this action.</param>
    /// <param name="inputTypes">The input types for this action.</param>
    public InputAction(uint accessTag, params IInputType[] inputTypes)
    {
        ID = ShapeID.NextID;
        AccessTag = accessTag;
        inputs.AddRange(inputTypes);
    }
    /// <summary>
    /// Initializes a new instance of <see cref="InputAction"/> with a specific access tag, gamepad, and input types.
    /// </summary>
    /// <param name="accessTag">The access tag for this action.</param>
    /// <param name="gamepad">The associated gamepad device.</param>
    /// <param name="inputTypes">The input types for this action.</param>
    public InputAction(uint accessTag, GamepadDevice gamepad, params IInputType[] inputTypes)
    {
        ID = ShapeID.NextID;
        AccessTag = accessTag;
        Gamepad = gamepad;
        inputs.AddRange(inputTypes);
    }
    #endregion

    #region Class

    /// <summary>
    /// Gets the index of the associated gamepad, or -1 if none.
    /// </summary>
    /// <returns>The gamepad index or -1.</returns>
    public int GetGamepadIndex() => Gamepad?.Index ?? -1;

    /// <summary>
    /// Clears the current input state.
    /// </summary>
    public void ClearState()
    {
        State = new InputState(false, true, 0f, GetGamepadIndex(), InputDeviceType.Keyboard);
    }

    /// <summary>
    /// Consumes the current input state and marks it as consumed.
    /// </summary>
    /// <param name="valid">True if the state was not already consumed; otherwise, false.</param>
    /// <returns>The previous input state before consumption.</returns>
    public InputState Consume(out bool valid)
    {
        valid = false;   
        if (State.Consumed) return State;
        
        valid = true;
        var returnValue = State;
        State = State.Consume();
        return returnValue;
    }

    /// <summary>
    /// Updates the input action state based on the current input and time delta.
    /// </summary>
    /// <param name="dt">The time delta in seconds.</param>
    public void Update(float dt)
    {
        //Accumulate All Input States From All Input Types
        InputState current = new();
        foreach (var input in inputs)
        {
            var inputState = input.GetState(Gamepad);
            current = current.Accumulate(inputState);
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
            else multiTapF = multiTapCount / (float)multiTapTarget;
        }
        
        //Calculate New State
        current = new(current, holdF, multiTapF);
        State = new(State, current);

        //switch between off and on
        if (State.Pressed)
        {
            Toggle = Toggle switch
            {
                ToggleState.Off => ToggleState.On,
                _ => ToggleState.Off
            };
            // Toggle = (Toggle == ToggleState.Off) ? ToggleState.On : ToggleState.Off;
        }
        
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
    /// <returns>A new <see cref="InputAction"/> copy.</returns>
    public InputAction Copy()
    {
        var copied = GetInputsCopied().ToArray();
        if (Gamepad == null)
        {
            var copy = new InputAction(AccessTag, copied)
            {
                axisSensitivity = axisSensitivity,
                axisGravitiy = axisGravitiy,
                Enabled = this.Enabled
            };
            return copy;
        }
        else
        {
            var copy = new InputAction(AccessTag, Gamepad, copied)
            {
                axisSensitivity = axisSensitivity,
                axisGravitiy = axisGravitiy,
                Enabled = this.Enabled
            };
            return copy;
        }
        
    }

    /// <summary>
    /// Gets a list of all input types associated with this action.
    /// </summary>
    /// <returns>A list of input types.</returns>
    public List<IInputType> GetInputs()
    {
        var list = new List<IInputType>();
        list.AddRange(inputs);
        return list;
    }

    /// <summary>
    /// Gets a list of all input types associated with this action, as copies.
    /// </summary>
    /// <returns>A list of copied input types.</returns>
    public List<IInputType> GetInputsCopied()
    {
        var list = new List<IInputType>();
        foreach (var input in inputs)
        {
            list.Add(input.Copy());
        }
        return list;
    }

    /// <summary>
    /// Gets a list of input types filtered by device type.
    /// </summary>
    /// <param name="filter">The device type to filter by.</param>
    /// <returns>A list of filtered input types.</returns>
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

    /// <summary>
    /// Gets a list of copied input types filtered by device type.
    /// </summary>
    /// <param name="filter">The device type to filter by.</param>
    /// <returns>A list of filtered, copied input types.</returns>
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

    /// <summary>
    /// Gets a list of input types, limited to a maximum count.
    /// </summary>
    /// <param name="maxCount">The maximum number of input types to return.</param>
    /// <returns>A list of input types.</returns>
    public List<IInputType> GetInputs(int maxCount)
    {
        if (inputs.Count <= 0) return new();
        
        var list = new List<IInputType>();
        if(maxCount > 0 && maxCount < inputs.Count) list.AddRange(inputs.GetRange(0, maxCount));
        else list.AddRange(inputs);
        return list;
    }

    /// <summary>
    /// Gets a list of input types filtered by device type and limited to a maximum count.
    /// </summary>
    /// <param name="filter">The device type to filter by.</param>
    /// <param name="maxCount">The maximum number of input types to return.</param>
    /// <returns>A list of filtered input types.</returns>
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

    /// <summary>
    /// Removes all input types from this action.
    /// </summary>
    public void ClearInputs() => inputs.Clear();

    /// <summary>
    /// Removes a specific input type from this action.
    /// </summary>
    /// <param name="inputType">The input type to remove.</param>
    /// <returns>True if removed; otherwise, false.</returns>
    public bool RemoveInput(IInputType inputType) => inputs.Remove(inputType);

    /// <summary>
    /// Removes all input types of a specific device type.
    /// </summary>
    /// <param name="filter">The device type to filter by.</param>
    /// <returns>A list of removed input types.</returns>
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

    /// <summary>
    /// Adds a new input type to this action.
    /// </summary>
    /// <param name="newType">The input type to add.</param>
    public void AddInput(IInputType newType) => inputs.Add(newType);

    /// <summary>
    /// Adds multiple input types to this action.
    /// </summary>
    /// <param name="inputTypes">The input types to add.</param>
    public void AddInputs(params IInputType[] inputTypes) => inputs.AddRange(inputTypes);

    /// <summary>
    /// Checks if this action contains a specific input type.
    /// </summary>
    /// <param name="inputType">The input type to check for.</param>
    /// <returns>True if present; otherwise, false.</returns>
    public bool HasInput(IInputType inputType) => inputs.Contains(inputType);

    /// <summary>
    /// Checks if this action contains any input of the specified device type.
    /// </summary>
    /// <param name="inputDeviceType">The device type to check for.</param>
    /// <returns>True if present; otherwise, false.</returns>
    public bool HasInput(InputDeviceType inputDeviceType)
    {
        foreach (var input in inputs)
        {
            if (input.GetInputDevice() == inputDeviceType) return true;
        }

        return false;
    }

    
    /// <summary>
    /// Generate a description for this action based on the parameters. Layout-> "Title: [type a][type b][type c] ..."
    /// </summary>
    /// <param name="deviceType">Only input types of the specified device are used.</param>
    /// <param name="shorthand">Should the shorthand name or full name of the input type be used?</param>
    /// <param name="count">Limits the amount input types used. If count is smaller or equal to 0 all input types are used.</param>
    /// <param name="useTitle">Should the title of this input action be used as a prefix? "Title: [input type]"</param>
    /// <param name="brackets">Should the input type be encapsulated in square brackets ["Input Type"]?</param>
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
        var inputTypes = GetInputs(deviceType, count);
        var names = new List<string>();
        foreach (var input in inputTypes)
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
        var inputTypes = GetInputs(count);
        var names = new List<string>();
        foreach (var input in inputTypes)
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
        var inputTypes = GetInputs();
        var names = new List<InputName>();
        foreach (var input in inputTypes)
        {
            var name = input.GetName(shorthand);
            var type = input.GetInputDevice();
            var inputName = new InputName(name, type);
            names.Add(inputName);
        }

        return names;

    }
    #endregion
}