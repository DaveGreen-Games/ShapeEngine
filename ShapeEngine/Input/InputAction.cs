using System.Text;
using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Input;

/// <summary>
/// Represents an input action, which can be triggered by various input types and devices.
/// Handles state, multi-tap, hold, and axis sensitivity/gravity.
/// </summary>
public class InputAction
{
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
    private readonly List<IInputType> inputs = new();
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
    /// <returns>The previous input state before consumption.</returns>
    public InputState Consume()
    {
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

    #region Static

    #region Members

    /// <summary>
    /// This access tag grants access regardless of the input system lock.
    /// </summary>
    public static readonly uint AllAccessTag = NextTag;
    /// <summary>
    /// The default access tag for actions.
    /// </summary>
    public static readonly uint DefaultAccessTag = NextTag;
    /// <summary>
    /// Indicates if the input system is currently locked.
    /// </summary>
    public static bool Locked { get; private set; }
    private static BitFlag lockWhitelist;
    private static BitFlag lockBlacklist;
    private static uint tagPowerCounter = 1;
    /// <summary>
    /// Gets the next available access tag.
    /// </summary>
    public static uint NextTag => BitFlag.GetFlagUint(tagPowerCounter++);
    #endregion
    
    
    #region Lock System
    /// <summary>
    /// Locks the input system, clearing all whitelists and blacklists.
    /// <remarks>
    /// Only <see cref="InputAction"/> with <see cref="AllAccessTag"/> will be able to trigger.
    /// </remarks>
    /// </summary>
    public static void Lock()
    {
        Locked = true;
        lockWhitelist = BitFlag.Clear();
        lockBlacklist = BitFlag.Clear();
    }

    /// <summary>
    /// Locks the input system with a specific whitelist and blacklist.
    /// <remarks>
    /// All <see cref="InputAction"/> with a tag contained in the whitelist will be able to trigger.
    /// All <see cref="InputAction"/> with a tag contained in the blacklist will not be able to trigger.
    /// </remarks>
    /// </summary>
    /// <param name="whitelist">The whitelist of access tags.</param>
    /// <param name="blacklist">The blacklist of access tags.</param>
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
    /// <summary>
    /// Locks the input system with a specific whitelist.
    /// <remarks>
    /// All <see cref="InputAction"/> with a tag contained in the whitelist will be able to trigger.
    /// </remarks>
    /// </summary>
    /// <param name="whitelist">The whitelist of access tags.</param>
    public static void LockWhitelist(BitFlag whitelist)
    {
        Locked = true;
        lockWhitelist = whitelist;
        lockBlacklist = BitFlag.Clear();
        // lockWhitelist.Clear();
        // lockBlacklist.Clear();
        // if(whitelist.Length > 0) lockWhitelist.AddRange(whitelist);

    }
    /// <summary>
    /// Locks the input system with a specific blacklist.
    /// <remarks>
    /// All <see cref="InputAction"/> with a tag contained in the blacklist will not be able to trigger.
    /// </remarks>
    /// </summary>
    /// <param name="blacklist">The blacklist of access tags.</param>
    public static void LockBlacklist(BitFlag blacklist)
    {
        Locked = true;
        lockBlacklist = blacklist;
        lockWhitelist = BitFlag.Clear();
    }
    /// <summary>
    /// Unlocks the input system, clearing all whitelists and blacklists.
    /// </summary>
    public static void Unlock()
    {
        Locked = false;
        lockWhitelist = BitFlag.Clear();
        lockBlacklist = BitFlag.Clear();
    }
    /// <summary>
    /// Determines if the specified access tag has access.
    /// <remarks>
    /// <see cref="AllAccessTag"/> always returns true (has access).
    /// <list type="bullet">
    /// <item>If <c>tag</c> is contained in the current blacklist, this function will return false (no access).</item>
    /// <item>If <c>tag</c> is not contained in the current blacklist and <c>tag</c> is contained in the current whitelist,
    /// or the current whitelist is empty, this function will return true (has access).</item>
    /// </list>
    /// </remarks>
    /// </summary>
    /// <param name="tag">The access tag to check.</param>
    /// <returns>True if access is granted; otherwise, false.</returns>
    public static bool HasAccess(uint tag)
    {
        if (tag == AllAccessTag) return true;
        return (lockWhitelist.IsEmpty() || lockWhitelist.Has(tag)) && !lockBlacklist.Has(tag);
    }

    /// <summary>
    /// Determines if input is available for the specified access tag.
    /// <remarks>
    /// Always returns true if <see cref="Locked"/> is false.
    /// Otherwise returns <see cref="HasAccess(uint)"/> with the <c>tag</c> parameter.
    /// </remarks>
    /// </summary>
    /// <param name="tag">The access tag to check.</param>
    /// <returns>True if input is available; otherwise, false.</returns>
    public static bool IsInputAvailable(uint tag)
    {
        if (!Locked) return true;
        return HasAccess(tag);
    }

    /// <summary>
    /// Determines if the specified action has access.
    /// </summary>
    /// <param name="action">The input action to check.</param>
    /// <returns>True if access is granted; otherwise, false.</returns>
    public static bool HasAccess(InputAction action) => HasAccess(action.AccessTag);
    #endregion
    
    #region Input Actions
    /// <summary>
    /// Updates a set of input actions with the specified gamepad and time delta.
    /// </summary>
    /// <param name="dt">The time delta in seconds.</param>
    /// <param name="gamepad">The gamepad device to use.</param>
    /// <param name="actions">The input actions to update.</param>
    public static void UpdateActions(float dt, GamepadDevice? gamepad, params InputAction[] actions)
    {
        foreach (var action in actions)
        {
            action.Gamepad = gamepad;
            action.Update(dt);
        }
    }
    /// <summary>
    /// Updates a list of input actions with the specified gamepad and time delta.
    /// </summary>
    /// <param name="dt">The time delta in seconds.</param>
    /// <param name="gamepad">The gamepad device to use.</param>
    /// <param name="actions">The input actions to update.</param>
    public static void UpdateActions(float dt, GamepadDevice? gamepad, List<InputAction> actions)
    {
        foreach (var action in actions)
        {
            action.Gamepad = gamepad;
            action.Update(dt);
        }
    }
    /// <summary>
    /// Gets descriptions for a list of input actions.
    /// </summary>
    /// <param name="inputDeviceType">The device type to filter by.</param>
    /// <param name="shorthand">Whether to use shorthand names.</param>
    /// <param name="typesPerActionCount">The number of types per action.</param>
    /// <param name="actions">The input actions to describe.</param>
    /// <returns>A list of action descriptions.</returns>
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
    /// <summary>
    /// Gets descriptions for a set of input actions.
    /// </summary>
    /// <param name="inputDeviceType">The device type to filter by.</param>
    /// <param name="shorthand">Whether to use shorthand names.</param>
    /// <param name="typesPerActionCount">The number of types per action.</param>
    /// <param name="actions">The input actions to describe.</param>
    /// <returns>A list of action descriptions.</returns>
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
    /// <summary>
    /// Gets the input state for a keyboard button.
    /// </summary>
    /// <param name="button">The keyboard button.</param>
    /// <param name="accessTag">The access tag.</param>
    /// <returns>The input state.</returns>
    public static InputState GetState(ShapeKeyboardButton button, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ShapeInput.KeyboardDevice.CreateInputState(button);// InputTypeKeyboardButton.GetState(button);
    }
    /// <summary>
    /// Gets the input state for a mouse button.
    /// </summary>
    /// <param name="button">The mouse button.</param>
    /// <param name="accessTag">The access tag.</param>
    /// <returns>The input state.</returns>
    public static InputState GetState(ShapeMouseButton button, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ShapeInput.MouseDevice.CreateInputState(button); // InputTypeMouseButton.GetState(button);
    }
    /// <summary>
    /// Gets the input state for a gamepad button.
    /// </summary>
    /// <param name="button">The gamepad button.</param>
    /// <param name="accessTag">The access tag.</param>
    /// <param name="gamepadIndex">The gamepad index.</param>
    /// <param name="deadzone">The deadzone threshold.</param>
    /// <returns>The input state.</returns>
    public static InputState GetState(ShapeGamepadButton button, uint accessTag, int gamepadIndex, float deadzone = 0.2f)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        var gamepad = ShapeInput.GamepadDeviceManager.GetGamepad(gamepadIndex);
        return gamepad == null ? new() : gamepad.CreateInputState(button, deadzone); //  InputTypeGamepadButton.GetState(button, gamepad, deadzone);
    }
    /// <summary>
    /// Gets the input state for a keyboard button axis.
    /// </summary>
    /// <param name="neg">The negative direction button.</param>
    /// <param name="pos">The positive direction button.</param>
    /// <param name="accessTag">The access tag.</param>
    /// <returns>The input state.</returns>
    public static InputState GetState(ShapeKeyboardButton neg, ShapeKeyboardButton pos, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ShapeInput.KeyboardDevice.CreateInputState(neg, pos); // InputTypeKeyboardButtonAxis.GetState(neg, pos);
    }
    /// <summary>
    /// Gets the input state for a mouse button axis.
    /// </summary>
    /// <param name="neg">The negative direction button.</param>
    /// <param name="pos">The positive direction button.</param>
    /// <param name="accessTag">The access tag.</param>
    /// <returns>The input state.</returns>
    public static InputState GetState(ShapeMouseButton neg, ShapeMouseButton pos, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ShapeInput.MouseDevice.CreateInputState(neg, pos); // InputTypeMouseButtonAxis.GetState(neg, pos);
    }
    /// <summary>
    /// Gets the input state for a gamepad button axis.
    /// </summary>
    /// <param name="neg">The negative direction button.</param>
    /// <param name="pos">The positive direction button.</param>
    /// <param name="accessTag">The access tag.</param>
    /// <param name="gamepadIndex">The gamepad index.</param>
    /// <param name="deadzone">The deadzone threshold.</param>
    /// <returns>The input state.</returns>
    public static InputState GetState(ShapeGamepadButton neg, ShapeGamepadButton pos, uint accessTag, int gamepadIndex, float deadzone = 0.2f)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        var gamepad = ShapeInput.GamepadDeviceManager.GetGamepad(gamepadIndex);
        return gamepad == null ? new() : gamepad.CreateInputState(neg, pos, deadzone);
        // return InputTypeGamepadButtonAxis.GetState(neg, pos, gamepad, deadzone);
    }
    /// <summary>
    /// Gets the input state for a mouse wheel axis.
    /// </summary>
    /// <param name="axis">The mouse wheel axis.</param>
    /// <param name="accessTag">The access tag.</param>
    /// <param name="deadzone">The deadzone threshold.</param>
    /// <returns>The input state.</returns>
    public static InputState GetState(ShapeMouseWheelAxis axis, uint accessTag, float deadzone = 1f)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ShapeInput.MouseDevice.CreateInputState(axis, deadzone); // InputTypeMouseWheelAxis.GetState(axis);
    }
    /// <summary>
    /// Gets the input state for a gamepad axis.
    /// </summary>
    /// <param name="axis">The gamepad axis.</param>
    /// <param name="accessTag">The access tag.</param>
    /// <param name="gamepadIndex">The gamepad index.</param>
    /// <param name="deadzone">The deadzone threshold.</param>
    /// <returns>The input state.</returns>
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