using System.Text;
using ShapeEngine.Core;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Input;

/// <summary>
/// Represents an input action, which can be triggered by various input types and devices.
/// Handles state, multi-tap, hold, and axis sensitivity/gravity.
/// </summary>
/// <remarks>
/// Use <see cref="InputActionTree"/> for updating and managing multiple <see cref="InputAction"/> instances.
/// </remarks>
public class InputAction : IComparable<InputAction>, ICopyable<InputAction>, IEquatable<InputAction>
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
    
    #region Input Action Tree Management
    /// <summary>
    /// Gets the parent <see cref="InputActionTree"/> of this action, if any.
    /// </summary>
    public InputActionTree? Parent { get; private set; }
    
    /// <summary>
    /// Indicates whether this action is currently part of an <see cref="InputActionTree"/>.
    /// </summary>
    public bool IsInTree => Parent != null;
    
    /// <summary>
    /// Adds this action to the specified <see cref="InputActionTree"/>.
    /// If already in a different tree, removes it from the previous tree first.
    /// </summary>
    /// <param name="tree">The tree to add this action to.</param>
    internal void EnterTree(InputActionTree tree)
    {
        if (Parent != null)
        {
            //reference equals is enough here
            //duplicates can not be added to the same tree, therefore, EnterTree will not be called for an InputAction that is already in the tree.
            if (ReferenceEquals(Parent, tree)) return;
            Parent.Remove(this);
        }
        Parent = tree;
    }
    
    /// <summary>
    /// Removes this action from its parent <see cref="InputActionTree"/>, if any.
    /// </summary>
    internal void ExitTree()
    {
        if(Parent == null) return;
        Parent = null;
    }
    #endregion
    
    #region Members

   
    /// <summary>
    /// Indicates if this input action is active. If set to <c>false</c>, the action will not process input or update its state.
    /// </summary>
    public bool Active
    {
        get => active;
        set
        {
            if(active == value) return;
            
            active = value;
            if (active) return;
            Reset();
        }
    }

    private bool active = true;
    /// <summary>
    /// Gets or sets whether this input action blocks input types after use.
    /// <see cref="InputAction"/> has to be part of an <see cref="InputActionTree"/> to participate in the blocking system.
    /// <para>
    /// If set to <c>true</c>, input types used by this action will be added to the block list for the current frame,
    /// preventing other actions from using the same input types until the next frame.
    /// </para>
    /// <para>
    /// If <see cref="Active"/> is set to <c>false</c>,
    /// or if <see cref="AccessTag"/> does not have access,
    /// this action will not block input types.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Only <see cref="InputAction"/>s with <c>BlocksInput</c> enabled participate in this blocking system.
    /// If set to <c>false</c>, this action neither blocks input types nor can it be blocked by others.
    /// </remarks>
    public bool BlocksInput { get; set; }
    
    /// <summary>
    /// Gets or sets the execution order for this <see cref="InputAction"/>.
    /// <para>Input actions with a lower <see cref="ExecutionOrder"/> value are processed first.</para>
    /// <para>This value is automatically assigned when the action is created, but can be set manually if needed.</para>
    /// <para>This only has an effect when used with <see cref="InputActionTree"/> or in other sorted collections
    /// that use <see cref="CompareTo"/> for determining order.</para>
    /// </summary>
    /// <remarks>
    /// If two <see cref="InputAction"/> have the same <see cref="ExecutionOrder"/>,
    /// <see cref="Id"/> is used to determine the order.
    /// A lower <see cref="Id"/> value is processed first.
    /// <para>
    /// This is especially useful when combined with <see cref="BlocksInput"/>,
    /// as it allows precise control over which <see cref="InputAction"/> instances can block others based on their execution order.
    /// </para>
    /// </remarks>
    public int ExecutionOrder { get; set; } = nextExecutionOrder++;
    private static int nextExecutionOrder = 0;
    
    /// <summary>
    /// The unique identifier for this input action.
    /// </summary>
    public uint Id { get; }
    /// <summary>
    /// The access tag used for locking/unlocking this action.
    /// <remarks>
    /// <list type="bullet">
    /// <item>Determines access of this action.</item>
    /// <item>Only used when input actions are locked.</item>
    /// <item>If this tag is contained in the lock blacklist while the lock is active, the <see cref="InputState"/> of this <see cref="InputAction"/> will not be available.</item>
    /// <item>If this tag is contained in the lock whitelist while the lock is active, the <see cref="InputState"/> of this <see cref="InputAction"/> will always be available.</item>
    /// <item>If <see cref="ShapeInput.AllAccessTag"/> is used the <see cref="InputState"/> of this <see cref="InputAction"/> will always be available, regardless of the lock state.</item>
    /// </list>
    /// </remarks>
    /// </summary>
    public uint AccessTag { get; private set; } = ShapeInput.DefaultAccessTag;

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
    private int multiTapCount;

    /// <summary>
    /// The settings that define the behavior and configuration of this input action.
    /// </summary>
    public readonly InputActionSettings Settings;
    
    /// <summary>
    /// Gets the current input state for this action.
    /// This property is updated each frame,
    /// reflecting the accumulated state from all associated input types.
    /// </summary>
    public InputState State { get; private set; }

    private readonly List<IInputType> inputs = [];

    public readonly HashSet<IInputType> UsedInputs = [];
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
    /// <see cref="Settings"/> are set to default values.
    /// </summary>
    public InputAction()
    {
        Id = ShapeID.NextID;
        Settings = new InputActionSettings();
    }

    /// <summary>
    /// Initializes a new instance of <see cref="InputAction"/> with the specified access tag and settings.
    /// </summary>
    /// <param name="accessTag">The access tag for this action.</param>
    /// <param name="settings">The settings for this action.</param>
    public InputAction(uint accessTag, InputActionSettings settings)
    {
        Id = ShapeID.NextID;
        AccessTag = accessTag;
        Settings = settings;
    }
    
    /// <summary>
    /// Initializes a new instance of <see cref="InputAction"/> with the specified access tag, gamepad, and settings.
    /// </summary>
    /// <param name="accessTag">The access tag for this action.</param>
    /// <param name="gamepad">The associated gamepad device.</param>
    /// <param name="settings">The settings for this action.</param>
    public InputAction(uint accessTag, GamepadDevice gamepad, InputActionSettings settings)
    {
        Id = ShapeID.NextID;
        Gamepad = gamepad;
        AccessTag = accessTag;
        Settings = settings;
    }
    
    /// <summary>
    /// Initializes a new instance of <see cref="InputAction"/> with the specified settings and input types.
    /// </summary>
    /// <param name="settings">The settings for this action.</param>
    /// <param name="inputTypes">The input types to associate with this action.</param>
    public InputAction(InputActionSettings settings, params IInputType[] inputTypes)
    {
        Id = ShapeID.NextID;
        inputs.AddRange(inputTypes);
        Settings = settings;
    }
    
    /// <summary>
    /// Initializes a new instance of <see cref="InputAction"/> with the specified access tag, settings, and input types.
    /// </summary>
    /// <param name="accessTag">The access tag for this action.</param>
    /// <param name="settings">The settings for this action.</param>
    /// <param name="inputTypes">The input types to associate with this action.</param>
    public InputAction(uint accessTag, InputActionSettings settings, params IInputType[] inputTypes)
    {
        Id = ShapeID.NextID;
        AccessTag = accessTag;
        inputs.AddRange(inputTypes);
        Settings = settings;
    }
    
    /// <summary>
    /// Initializes a new instance of <see cref="InputAction"/> with the specified access tag, gamepad, settings, and input types.
    /// </summary>
    /// <param name="accessTag">The access tag for this action.</param>
    /// <param name="gamepad">The associated gamepad device.</param>
    /// <param name="settings">The settings for this action.</param>
    /// <param name="inputTypes">The input types to associate with this action.</param>
    public InputAction(uint accessTag, GamepadDevice gamepad, InputActionSettings settings, params IInputType[] inputTypes)
    {
        Id = ShapeID.NextID;
        AccessTag = accessTag;
        Gamepad = gamepad;
        inputs.AddRange(inputTypes);
        Settings = settings;
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
        Toggle = ToggleState.Off;
        
    }

    /// <summary>
    /// Resets the input action to its initial state, clearing the current input state,
    /// hold timer, multi-tap timer, and multi-tap count.
    /// </summary>
    public void Reset()
    {
        ClearState();
        holdTimer = 0f;
        multiTapTimer = 0f;
        multiTapCount = 0;
        UsedInputs.Clear();
    }
    /// <summary>
    /// Consumes the current input <see cref="State"/> and marks it as consumed.
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
    /// Updates the input action state when it is not part of an <see cref="InputActionTree"/>.
    /// This method should only be used for standalone actions.
    /// If the action is in a tree, the update is skipped, because the <see cref="InputActionTree"/> will handle the update automatically.
    /// </summary>
    /// <param name="dt">The time delta in seconds.</param>
    /// <returns><c>true</c> if the action was updated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Because the <see cref="InputAction"/> is not part of an <see cref="InputActionTree"/>,
    /// the <see cref="IInputType"/> Block system is not used.
    /// </remarks>
    public bool Update(float dt)
    {
        UsedInputs.Clear();
        if (IsInTree) return false;
        return UpdateAction(dt, null, out _);
    }

    /// <summary>
    /// Updates the input action state when it is part of an <see cref="InputActionTree"/>.
    /// Uses the provided blocklist to manage input type blocking for this frame.
    /// </summary>
    /// <param name="dt">The time delta in seconds.</param>
    /// <param name="blocklist">A set of input types that are blocked for this frame.</param>
    /// <param name="inputDeviceType">Outputs the detected input device type.</param>
    /// <returns><c>true</c> if the action was updated; otherwise, <c>false</c>.</returns>
    internal bool UpdateInternal(float dt, HashSet<IInputType>? blocklist, out InputDeviceType inputDeviceType)
    {
        UsedInputs.Clear();
        inputDeviceType = InputDeviceType.None;
        if (!IsInTree) return false;
        return UpdateAction(dt, blocklist, out inputDeviceType);
    }
    
    private bool UpdateAction(float dt, HashSet<IInputType>? blocklist, out InputDeviceType inputDeviceType)
    {
        inputDeviceType = InputDeviceType.None;
        if (!Active) return false;
        if (ShapeInput.Locked && !ShapeInput.HasAccess(AccessTag))
        {
            Reset();//Good idea? It does not update anything, therefore, it should be reset.
            return false;
        }
        
        //Accumulate All Input States From All Input Types
        InputState current = new();
        foreach (var input in inputs)
        {
            var inputState = input.GetState(Gamepad);
            
            //if it can not be added to the input type block list, it was already used this frame before and therefore it is skipped now.
            if (blocklist != null && inputState.Down && BlocksInput && !blocklist.Add(input)) continue;

            //if input was used and no input device type was set yet, set it to the input type of this input.
            if (inputState.Down)
            {
                UsedInputs.Add(input);
                if (inputDeviceType == InputDeviceType.None)
                {
                    inputDeviceType = input.GetInputDevice();
                }
            }
            
            current = current.Accumulate(inputState);
        }
        
        //Multi Tap & Hold System
        var multiTapF = 0f;
        if (multiTapTimer > 0f)
        {
            multiTapTimer -= dt;
            if (multiTapTimer <= 0f)
            {
                multiTapTimer = 0f;
                multiTapCount = 0;
            }
        }
        
        var holdF = 0f;
        if (State.Up && current.Down) //pressed
        {
            if (Settings.HoldDuration > 0)
            {
                holdTimer = Settings.HoldDuration;
            }
            if (Settings.MultiTapDuration > 0f && Settings.MultiTapTarget > 1)
            {
                if (multiTapTimer <= 0f) multiTapTimer = Settings.MultiTapDuration;
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
            else holdF = 1f - (holdTimer / Settings.HoldDuration);
        }
        
        if (multiTapTimer > 0f)
        {
            if (multiTapCount >= Settings.MultiTapTarget)
            {
                multiTapF = 1f;
                if (holdTimer > 0f)
                {
                    holdTimer = 0f;
                    holdF = 0f;
                }
            }
            else multiTapF = multiTapCount / (float)Settings.MultiTapTarget;
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
        }
        
        //Reset Multitap Count On Successful Multitap
        if (multiTapF >= 1f) multiTapCount = 0;
        
        //Axis Sensitivity & Gravity
        if (Settings.AxisSensitivity > 0 || Settings.AxisGravitiy > 0)
        {
            int raw = MathF.Sign(State.AxisRaw);
            float dif = State.AxisRaw - State.Axis;
            int difSign = MathF.Sign(dif);

            if (difSign != 0)
            {
                var axisChange = 0f;
                //var snapValue = 0f;
                float gravity = Settings.AxisGravitiy <= 0f ? 0f : 1f / Settings.AxisGravitiy;
                float sensitivity = Settings.AxisSensitivity <= 0f ? 0f : 1f / Settings.AxisSensitivity;
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

        return true;
    }

    /// <summary>
    /// Creates a deep of copy of this <see cref="InputAction"/>, including deep copies of all associated <see cref="IInputType"/>s.
    ///  A new ID is generated for the copy and the <see cref="Parent"/> is not set, as the copy should not be part of the same tree as the original.
    /// </summary>
    /// <returns>A new <see cref="InputAction"/> copy.</returns>
    public InputAction Copy()
    {
        var copied = GetInputsCopiedArray();
        var copy = new InputAction(AccessTag, Settings, copied)
        {
            Gamepad = Gamepad,
            Active = Active,
            ExecutionOrder = ExecutionOrder,
            Title = Title,
            BlocksInput = BlocksInput,
            Toggle = Toggle,
            State = State,
            holdTimer = holdTimer,
            multiTapTimer = multiTapTimer,
            multiTapCount = multiTapCount,
        };
        return copy;
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
    /// Returns a new list containing deep copies of all input types associated with this action.
    /// </summary>
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
    /// Returns a new array containing deep copies of all input types associated with this action.
    /// </summary>
    public IInputType[] GetInputsCopiedArray()
    {
        var arr = new IInputType[inputs.Count];
        for (int i = 0; i < inputs.Count; i++)
        {
            arr[i] = inputs[i].Copy();
        }

        return arr;
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
    /// Returns a new list containing deep copies of all input types of the specified device type associated with this action.
    /// </summary>
    /// <param name="filter">The device type to filter by.</param>
    /// <returns>A list of copied input types matching the specified device type.</returns>
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
    public List<IInputType>? RemoveInputs(InputDeviceType filter)
    {
        if (inputs.Count <= 0) return null;
        List<IInputType>? removed = null;
        for (int i = inputs.Count - 1; i >= 0; i--)
        {
            var input = inputs[i];
            if (input.GetInputDevice() == filter)
            {
                removed ??= [];
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

    /// <summary>
    /// Compares this <see cref="InputAction"/> to another for sorting purposes.
    /// <para>
    /// First compares <see cref="ExecutionOrder"/>; if equal, compares <see cref="Id"/>.
    /// </para>
    /// </summary>
    /// <param name="other">The other <see cref="InputAction"/> to compare to.</param>
    /// <returns>
    /// -1 if this instance precedes <paramref name="other"/>; 1 if it follows; 0 if equal.
    /// </returns>
    public int CompareTo(InputAction? other)
    {
        if (other is null) return 1;
        if (ReferenceEquals(this, other)) return 0;
        int executionOrderComparison = ExecutionOrder.CompareTo(other.ExecutionOrder);
        if (executionOrderComparison != 0) return executionOrderComparison;
        return Id.CompareTo(other.Id);
    }
    
    /// <summary>
    /// Determines whether the specified <see cref="InputAction"/> is equal to the current <see cref="InputAction"/>.
    /// Compares the <see cref="Settings"/> and the sequence of input types.
    /// </summary>
    /// <param name="other">The <see cref="InputAction"/> to compare with the current instance.</param>
    /// <returns><c>true</c> if the specified <see cref="InputAction"/> is equal to the current instance; otherwise, <c>false</c>.</returns>
    public bool Equals(InputAction? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return 
            Settings.Equals(other.Settings) &&
            inputs.SequenceEqual(other.inputs);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current <see cref="InputAction"/>.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><c>true</c> if the specified object is equal to the current instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((InputAction)obj);
    }

    /// <summary>
    /// Serves as the default hash function for <see cref="InputAction"/>.
    /// Combines the hash codes of the input types and settings.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(inputs);
        hashCode.Add(Settings);
        return hashCode.ToHashCode();
    }
}
