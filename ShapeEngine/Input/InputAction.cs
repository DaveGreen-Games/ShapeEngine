using System.Text;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Input;

//TODO: Test if this works good or not.

//Q: InputActionTree could also be a Dictionary<GamepadDevice, SortedSet<InputAction>> to allow multiple gamepads to be used at the same time?
//NOTE: Currently a different InputActionTree is used for each gamepad.


/// <summary>
/// Represents a sorted collection of <see cref="InputActionTree"/> objects.
/// Provides methods for updating and retrieving input action trees based on various criteria.
/// </summary>
public class InputActionTrees : SortedSet<InputActionTree>
{
    /// <summary>
    /// Updates all <see cref="InputActionTree"/> instances in this collection by calling their <c>Update</c> method with the specified time delta.
    /// </summary>
    /// <param name="dt">The time delta in seconds.</param>
    public void Update(float dt)
    {
        foreach (var tree in this)
        {
            tree.Update(dt);
        }
    }
    
        
    #region Get Input Action Trees
    
    /// <summary>
    /// Gets the first <see cref="InputActionTree"/> in the collection with the specified ID.
    /// </summary>
    /// <param name="id">The unique identifier of the input action tree.</param>
    /// <returns>The matching <see cref="InputActionTree"/>, or null if not found.</returns>
    public InputActionTree? GetById(int id)
    {
       foreach (var action in this)
       {
           if (action.Id == id) return action;
       }
       return null;
    }

    /// <summary>
    /// Gets the first <see cref="InputActionTree"/> in the collection with the specified execution order.
    /// </summary>
    /// <param name="executionOrder">The execution order to search for.</param>
    /// <returns>The matching <see cref="InputActionTree"/>, or null if not found.</returns>
    public InputActionTree? GetFirstByExecutionOrder(int executionOrder)
    {
       foreach (var action in this)
       {
           if (action.ExecutionOrder == executionOrder) return action;
       }
       return null;
    }

    /// <summary>
    /// Gets all <see cref="InputActionTree"/> instances in the collection with the specified execution order.
    /// </summary>
    /// <param name="executionOrder">The execution order to search for.</param>
    /// <returns>A list of matching <see cref="InputActionTree"/> instances, or null if none found.</returns>
    public List<InputActionTree>? GetAllWithExecutionOrder(int executionOrder)
    {
       List<InputActionTree>? result = null;
       foreach (var action in this)
       {
           if (action.ExecutionOrder != executionOrder) continue;
           result ??= [];
           result.Add(action);
       }
       return result;
    }
    #endregion
    
}


/// <summary>
/// Represents a sorted collection of <see cref="InputAction"/> objects, ordered by their execution order and ID.
/// Provides methods for updating and retrieving input actions based on various criteria.
/// <para>
/// Implements a blocking system for <see cref="IInputType"/>s.
/// When an <see cref="InputAction"/> in this tree is processed and its <see cref="InputAction.BlocksInput"/> property is set to <c>true</c>,
/// it blocks the associated <see cref="IInputType"/>s from being used by subsequent <see cref="InputAction"/>s in the same update cycle.
/// This ensures that input types are only handled by the first eligible action, preventing conflicts or duplicate processing.
/// </para>
/// </summary>
/// <remarks>
/// A different tree for each used gamepad is recommended.
/// </remarks>
public class InputActionTree : SortedSet<InputAction>, IComparable<InputActionTree>
{
    #region Blocking System

    private HashSet<IInputType> inputTypeBlockSet = [];
    private void ClearInputTypeBlockSet()
    {
        inputTypeBlockSet.Clear();
    }
    #endregion
    
    private static int executionOrderCounter = 0;
    /// <summary>
    /// Gets or sets the execution order for this <see cref="InputActionTree"/> instance.
    /// Lower values are processed first. This value is automatically assigned when the instance is created,
    /// but can be set manually if needed. Used for sorting and determining update order like in a SortedSet.
    /// </summary>
    /// <remarks>
    /// <see cref="ShapeInput.InputActionTrees"/> uses this to determine the order in which input action trees are processed.
    /// </remarks>
    public int ExecutionOrder = executionOrderCounter++;
    
    private static uint idCounter = 0;
    /// <summary>
    /// The unique identifier for this <see cref="InputActionTree"/> instance.
    /// </summary>
    public readonly uint Id = idCounter++;
    
    #region Class
    
    /// <summary>
    /// Gets or sets the current gamepad device associated with this input action tree.
    /// If set, all input actions in the tree will use this gamepad for input processing during update.
    /// </summary>
    public GamepadDevice? CurrentGamepad { get; set; } = null;
    
    /// <summary>
    /// Updates all <see cref="InputAction"/> instances in the tree.
    /// If <see cref="CurrentGamepad"/> is set, assigns it to each action before updating.
    /// </summary>
    /// <param name="dt">The time delta in seconds.</param>
    public void Update(float dt)
    {
        ClearInputTypeBlockSet();
        
        foreach (var action in this)
        {
            action.Gamepad = CurrentGamepad;
            action.Update(dt, ref inputTypeBlockSet);
        }
    }
    #endregion
    
    #region Get Input Actions
    /// <summary>
    /// Gets the first <see cref="InputAction"/> in the tree with the specified ID.
    /// </summary>
    /// <param name="id">The unique identifier of the input action.</param>
    /// <returns>The matching <see cref="InputAction"/>, or null if not found.</returns>
    public InputAction? GetById(int id)
    {
        foreach (var action in this)
        {
            if (action.ID == id) return action;
        }
        return null;
    }
    
    /// <summary>
    /// Gets the first <see cref="InputAction"/> in the tree with the specified execution order.
    /// </summary>
    /// <param name="executionOrder">The execution order to search for.</param>
    /// <returns>The matching <see cref="InputAction"/>, or null if not found.</returns>
    public InputAction? GetFirstByExecutionOrder(int executionOrder)
    {
        foreach (var action in this)
        {
            if (action.ExecutionOrder == executionOrder) return action;
        }
        return null;
    }
    
    /// <summary>
    /// Gets the first <see cref="InputAction"/> in the tree with the specified name.
    /// </summary>
    /// <param name="name">The title of the input action.</param>
    /// <returns>The matching <see cref="InputAction"/>, or null if not found.</returns>
    public InputAction? GetFirstByName(string name)
    {
        foreach (var action in this)
        {
            if (action.Title == name) return action;
        }
        return null;
    }
    
    /// <summary>
    /// Gets the first <see cref="InputAction"/> in the tree with the specified access tag.
    /// </summary>
    /// <param name="accessTag">The access tag to search for.</param>
    /// <returns>The matching <see cref="InputAction"/>, or null if not found.</returns>
    public InputAction? GetFirstByAccessTag(uint accessTag)
    {
        foreach (var action in this)
        {
            if (action.AccessTag == accessTag) return action;
        }
        return null;
        // return this.FirstOrDefault(action => action.AccessTag == accessTag);
    }
    
    /// <summary>
    /// Gets all <see cref="InputAction"/> instances in the tree with the specified execution order.
    /// </summary>
    /// <param name="executionOrder">The execution order to search for.</param>
    /// <returns>A list of matching <see cref="InputAction"/> instances, or null if none found.</returns>
    public List<InputAction>? GetAllWithExecutionOrder(int executionOrder)
    {
        List<InputAction>? result = null;
        foreach (var action in this)
        {
            if (action.ExecutionOrder != executionOrder) continue;
            result ??= [];
            result.Add(action);
        }
        return result;
    }
    
    /// <summary>
    /// Gets all <see cref="InputAction"/> instances in the tree with the specified name.
    /// </summary>
    /// <param name="name">The title of the input actions to search for.</param>
    /// <returns>A list of matching <see cref="InputAction"/> instances, or null if none found.</returns>
    public List<InputAction>? GetAllWithName(string name)
    {
        List<InputAction>? result = null;
        foreach (var action in this)
        {
            if (action.Title != name) continue;
            result ??= [];
            result.Add(action);
        }
        return result;
    }
    
    /// <summary>
    /// Gets all <see cref="InputAction"/> instances in the tree with the specified access tag.
    /// </summary>
    /// <param name="accessTag">The access tag to search for.</param>
    /// <returns>A list of matching <see cref="InputAction"/> instances, or null if none found.</returns>
    public List<InputAction>? GetAllWithAccessTag(uint accessTag)
    {
        List<InputAction>? result = null;
        foreach (var action in this)
        {
            if (action.AccessTag != accessTag) continue;
            result ??= [];
            result.Add(action);
        }
        return result;
    }

    #endregion
    
    #region Set Active
    /// <summary>
    /// Sets the <c>Active</c> property for all <see cref="InputAction"/> instances in the tree.
    /// </summary>
    /// <param name="active">The value to set for the <c>Active</c> property.</param>
    public void SetAllActive(bool active)
    {
        foreach (var action in this)
        {
            action.Active = active;
        }
    }
    
    /// <summary>
    /// Sets the <c>Active</c> property for all <see cref="InputAction"/> instances in the tree
    /// that have the specified <paramref name="accessTag"/>.
    /// </summary>
    /// <param name="active">The value to set for the <c>Active</c> property.</param>
    /// <param name="accessTag">The access tag to filter actions by.</param>
    /// <returns>The number of actions that were updated.</returns>
    public int SetAllActive(bool active, int accessTag)
    {
        int num = 0;
        foreach (var action in this)
        {
            if (action.AccessTag != accessTag) continue;
            num++;
            action.Active = active;
        }
    
        return num;
    }
    #endregion
    
    /// <summary>
    /// Compares this <see cref="InputActionTree"/> to another for sorting purposes.
    /// First compares <see cref="ExecutionOrder"/>; if equal, compares <see cref="Id"/>.
    /// </summary>
    /// <param name="other">The other <see cref="InputActionTree"/> to compare to.</param>
    /// <returns>
    /// -1 if this instance precedes <paramref name="other"/>; 1 if it follows; 0 if equal.
    /// </returns>
    public int CompareTo(InputActionTree? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        int executionOrderComparison = ExecutionOrder.CompareTo(other.ExecutionOrder);
        if (executionOrderComparison != 0) return executionOrderComparison;
        return Id.CompareTo(other.Id);
    }
}


/// <summary>
/// Represents an input action, which can be triggered by various input types and devices.
/// Handles state, multi-tap, hold, and axis sensitivity/gravity.
/// </summary>
public class InputAction : IComparable<InputAction>
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
    public bool BlocksInput { get; set; } = false;
    
    /// <summary>
    /// Gets or sets the execution order for this <see cref="InputAction"/>.
    /// <para>Input actions with a lower <see cref="ExecutionOrder"/> value are processed first.</para>
    /// <para>This value is automatically assigned when the action is created, but can be set manually if needed.</para>
    /// <para>This only has an effect when used with <see cref="InputActionTree"/> or in other sorted collections
    /// that use <see cref="CompareTo"/> for determining order.</para>
    /// </summary>
    /// <remarks>
    /// If two <see cref="InputAction"/> have the same <see cref="ExecutionOrder"/>,
    /// <see cref="ID"/> is used to determine the order.
    /// A lower <see cref="ID"/> value is processed first.
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
    public uint ID { get; private set; }
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
    
    /// <summary>
    /// Gets the current input state for this action.
    /// This property is updated each frame,
    /// reflecting the accumulated state from all associated input types.
    /// </summary>
    public InputState State { get; private set; }

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

    //Todo: needs to know if input action is currently in a InputActionTree. Only works when not in a tree.
    
    // public void Update(float dt)
    // {
    //     if (IsInInputActionTree) return;
    //     Update(dt, null);
    // }
    
    /// <summary>
    /// Updates the input action state based on the current input and time delta.
    /// </summary>
    /// <param name="dt">The time delta in seconds.</param>
    /// <param name="blocklist">A reference to the set of input types blocked for this frame.</param>
    /// <remarks>
    /// <see cref="InputAction"/> is not updated when <see cref="Active"/> is set to <c>false</c>
    /// or <see cref="ShapeInput.Locked"/> is set to <c>true</c>
    /// and the <see cref="AccessTag"/> does not allow access.
    /// </remarks>
    internal void Update(float dt, ref HashSet<IInputType> blocklist)
    {
        if (!Active) return;
        if (ShapeInput.Locked && !ShapeInput.HasAccess(AccessTag))
        {
            Reset();//Good idea? It does not update anything, therefore, it should be reset.
            return;
        }
        
        //Accumulate All Input States From All Input Types
        InputState current = new();
        foreach (var input in inputs)
        {
            var inputState = input.GetState(Gamepad);
            
            //if it can not be added to the input type block list, it was already used this frame before and therefore it is skipped now.
            if (inputState.Down && BlocksInput && !blocklist.Add(input)) continue;
            
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
                Active = this.Active
            };
            return copy;
        }
        else
        {
            var copy = new InputAction(AccessTag, Gamepad, copied)
            {
                axisSensitivity = axisSensitivity,
                axisGravitiy = axisGravitiy,
                Active = this.Active
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
    /// First compares <see cref="ExecutionOrder"/>; if equal, compares <see cref="ID"/>.
    /// </para>
    /// </summary>
    /// <param name="other">The other <see cref="InputAction"/> to compare to.</param>
    /// <returns>
    /// -1 if this instance precedes <paramref name="other"/>; 1 if it follows; 0 if equal.
    /// </returns>
    public int CompareTo(InputAction? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        int executionOrderComparison = ExecutionOrder.CompareTo(other.ExecutionOrder);
        if (executionOrderComparison != 0) return executionOrderComparison;
        return ID.CompareTo(other.ID);
    }
}
