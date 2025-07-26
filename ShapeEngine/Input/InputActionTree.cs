using ShapeEngine.Core;

namespace ShapeEngine.Input;

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
/// A different tree for each used gamepad is recommended. Multiple trees can be grouped together using <see cref="InputActionTreeGroup"/>.
/// </remarks>
public class InputActionTree : SortedSet<InputAction>, IComparable<InputActionTree>, ICopyable<InputActionTree>, IEquatable<InputActionTree>
{
    #region Blocking System

    private readonly HashSet<IInputType> inputTypeBlockSet = [];
    private void ClearInputTypeBlockSet()
    {
        inputTypeBlockSet.Clear();
    }
    #endregion
    
    #region Group System
    
    /// <summary>
    /// Gets the group this <see cref="InputActionTree"/> belongs to, if any.
    /// </summary>
    public InputActionTreeGroup? Group { get; private set; }
    
    /// <summary>
    /// Indicates whether this <see cref="InputActionTree"/> is part of a group.
    /// </summary>
    public bool IsInGroup => Group != null;
    
    /// <summary>
    /// Adds this <see cref="InputActionTree"/> to the specified <see cref="InputActionTreeGroup"/>.
    /// If already in a group, removes it from the current group before joining the new one.
    /// </summary>
    /// <param name="treeGroup">The group to join.</param>
    internal void EnterGroup(InputActionTreeGroup treeGroup)
    {
        if (Group != null)
        {
            if (ReferenceEquals(Group, treeGroup)) return;
            Group.Remove(this);
        }
        Group = treeGroup;
    }
    
    /// <summary>
    /// Removes this <see cref="InputActionTree"/> from its current group, if any.
    /// </summary>
    internal void ExitGroup()
    {
        if(Group == null) return;
        Group = null;
    }

    #endregion

    #region Active System

    /// <summary>
    /// Gets a value indicating whether this <see cref="InputActionTree"/> is active.
    /// When inactive, all contained input actions are reset and tree can not be updated until reactivated.
    /// </summary>
    public bool Active { get; private set; } = true;
    
    /// <summary>
    /// Sets the active state of the tree.
    /// If deactivated, resets all contained input actions.
    /// Returns true if the state changed, false otherwise.
    /// Inactive trees can not be updated until reactivated.
    /// </summary>
    /// <param name="active">The desired active state.</param>
    /// <returns>True if the state was changed; otherwise, false.</returns>
    public bool SetActive(bool active)
    {
        if (Active == active) return false;
        Active = active;
        if (!Active)
        {
            foreach (var input in this)
            {
                input.Reset();
            }
        }
        return true;
    }

    #endregion
    
    #region Class

    public readonly HashSet<IInputType> UsedInputs = [];
    
    private static uint idCounter = 0;
    /// <summary>
    /// The unique identifier for this <see cref="InputActionTree"/> instance.
    /// </summary>
    public readonly uint Id = idCounter++;

    private static int excecutionOrderCounter = 0;
    /// <summary>
    /// The execution order of this <see cref="InputActionTree"/> instance.
    /// Used to determine the order in which input action trees are processed in the <see cref="InputActionTreeGroup"/> class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <c>ExecutionOrder</c> property is assigned during construction using a static counter,
    /// guaranteeing each tree a unique and incrementing execution order.
    /// Set the <c>ExecutionOrder</c> property to a specific value if you need to control the order of processing manually.
    /// </para>
    /// <para>
    /// The <see cref="CompareTo"/> method sorts trees by <c>ExecutionOrder</c> first, and by <c>Id</c> if execution orders are equal.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var tree5 = new InputActionTree();
    /// var tree1 = new InputActionTree();
    /// var tree2 = new InputActionTree();
    /// var tree3 = new InputActionTree();
    /// var tree4 = new InputActionTree();
    /// tree4.ExecutionOrder = -10;
    /// InputActionTrees trees = [tree1, tree2, tree3, tree4, tree5];
    /// trees.Update(dt);// The update function will process tree4 first, then tree5, then tree1, tree2, and tree3.
    /// </code>
    /// </example>
    public int ExecutionOrder = excecutionOrderCounter++;
    
    
    /// <summary>
    /// Gets or sets the current gamepad device associated with this input action tree.
    /// If set, all input actions in the tree will use this gamepad for input processing during update.
    /// </summary>
    public GamepadDevice? CurrentGamepad { get; set; }
    
    /// <summary>
    /// Adds an <see cref="InputAction"/> to the tree.
    /// If the action is added successfully, it is associated with this tree by calling <see cref="InputAction.EnterTree"/>.
    /// </summary>
    /// <param name="action">The input action to add.</param>
    /// <returns>True if the action was added; otherwise, false.</returns>
    public new bool Add(InputAction action)
    {
        var added = base.Add(action);
        
        if (added)
        {
            action.EnterTree(this);
        }
    
        return added;
    }
    
    /// <summary>
    /// Removes an <see cref="InputAction"/> from the tree.
    /// If the action is removed successfully, it is disassociated from this tree by calling <see cref="InputAction.ExitTree"/>.
    /// </summary>
    /// <param name="action">The input action to remove.</param>
    /// <returns>True if the action was removed; otherwise, false.</returns>
    public new bool Remove(InputAction action)
    {
        var removed = base.Remove(action);
    
        if (removed)
        {
            action.ExitTree();
        }
        
        return removed;
    }
    
    /// <summary>
    /// Updates all <see cref="InputAction"/> instances in the tree for the current frame.
    /// Skips update if the tree is part of a group or inactive.
    /// Clears the input type block set before processing.
    /// Assigns the current gamepad to each action and calls their internal update.
    /// </summary>
    /// <param name="dt">Delta time since last update.</param>
    /// <param name="usedDeviceType">The first input device type used by an InputAction during this update cycle.</param>
    /// <returns>True if the update was performed; otherwise, false.</returns>
    public bool Update(float dt, out InputDeviceType usedDeviceType)
    {
        UsedInputs.Clear();
        usedDeviceType = InputDeviceType.None;
        if (IsInGroup || !Active) return false;
        ClearInputTypeBlockSet();
        
        foreach (var action in this)
        {
            action.Gamepad = CurrentGamepad;
            var updated = action.UpdateInternal(dt, inputTypeBlockSet, out var deviceType);
            UsedInputs.UnionWith(action.UsedInputs);
            if(!updated || deviceType == InputDeviceType.None || usedDeviceType != InputDeviceType.None) continue;
            usedDeviceType = deviceType;
        }

        return true;
    }
    /// <summary>
    /// Updates all <see cref="InputAction"/> instances in the tree for the current frame.
    /// Skips update if the tree is part of a group or inactive.
    /// Clears the input type block set before processing.
    /// Assigns the current gamepad to each action and calls their internal update.
    /// </summary>
    /// <param name="dt">Delta time since last update.</param>
    /// <returns>True if the update was performed; otherwise, false.</returns>
    public bool Update(float dt)
    {
        UsedInputs.Clear();
        if (IsInGroup || !Active) return false;
        ClearInputTypeBlockSet();
        
        foreach (var action in this)
        {
            action.Gamepad = CurrentGamepad;
            action.UpdateInternal(dt, inputTypeBlockSet, out var deviceType);
            UsedInputs.UnionWith(action.UsedInputs);
        }

        return true;
    }
    // /// <summary>
    // /// Updates all <see cref="InputAction"/> instances in the tree for the current frame and determines the used input device type.
    // /// Only updates if the tree is in a group and active.
    // /// Clears the input type block set before processing.
    // /// Assigns the current gamepad to each action and calls their internal update.
    // /// Sets <paramref name="usedDeviceType"/> to the first non-None device type used by an action.
    // /// </summary>
    // /// <param name="dt">Delta time since last update.</param>
    // /// <param name="usedDeviceType">The first input device type used by an InputAction during this update cycle.</param>
    // /// <returns>True if the update was performed; otherwise, false.</returns>
    // internal bool UpdateInternal(float dt, out InputDeviceType usedDeviceType)
    // {
    //     UsedInputs.Clear();
    //     usedDeviceType = InputDeviceType.None;
    //     if (!IsInGroup || !Active) return false;
    //     ClearInputTypeBlockSet();
    //     
    //     foreach (var action in this)
    //     {
    //         action.Gamepad = CurrentGamepad;
    //         var updated = action.UpdateInternal(dt, inputTypeBlockSet, out var deviceType);
    //         UsedInputs.UnionWith(action.UsedInputs);
    //         if(!updated || deviceType == InputDeviceType.None || usedDeviceType != InputDeviceType.None) continue;
    //         usedDeviceType = deviceType;
    //     }
    //     return true;
    // }
    
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
            if (action.Id == id) return action;
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

    #region Copying
    
    /// <summary>
    /// Creates a deep copy of this <see cref="InputActionTree"/>, including all contained <see cref="InputAction"/> instances.
    /// The copied tree will have the same <see cref="CurrentGamepad"/> reference and new copies of all actions.
    /// </summary>
    /// <returns>A new <see cref="InputActionTree"/> instance with copied actions.</returns>
    public InputActionTree Copy()
    {
        var copy = new InputActionTree
        {
            CurrentGamepad = CurrentGamepad,
            ExecutionOrder = ExecutionOrder,
            Active = Active,
        };
        foreach (var action in this) copy.Add(action.Copy());
        
        return copy;
    }

    #endregion
    
    /// <summary>
    /// Determines whether the specified <see cref="InputActionTree"/> is equal to the current <see cref="InputActionTree"/>.
    /// Equality is based on reference, count, and sequence equality of contained <see cref="InputAction"/> instances.
    /// </summary>
    /// <param name="other">The <see cref="InputActionTree"/> to compare with the current tree.</param>
    /// <returns><c>true</c> if the specified tree is equal to the current tree; otherwise, <c>false</c>.</returns>
    public bool Equals(InputActionTree? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (Count != other.Count) return false;
        return this.SequenceEqual(other);
    }
    
    /// <summary>
    /// Determines whether the specified object is equal to the current <see cref="InputActionTree"/>.
    /// </summary>
    /// <param name="obj">The object to compare with the current tree.</param>
    /// <returns><c>true</c> if the specified object is equal to the current tree; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((InputActionTree)obj);
    }
    
    /// <summary>
    /// Returns a hash code for the current <see cref="InputActionTree"/>.
    /// The hash code is computed based on all contained <see cref="InputAction"/> instances.
    /// </summary>
    /// <returns>A hash code for the current tree.</returns>
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
    
        foreach (var action in this)
        {
            hashCode.Add(action);
        }
        
        return hashCode.ToHashCode();
    }

    /// <summary>
    /// Compares this <see cref="InputActionTree"/> to another for sorting purposes.
    /// Comparison is based on <see cref="ExecutionOrder"/> first, then <see cref="Id"/> if execution orders are equal.
    /// </summary>
    /// <param name="other">The other <see cref="InputActionTree"/> to compare to.</param>
    /// <returns>
    /// 0 if equal, -1 if this is less than other, 1 if greater.
    /// </returns>
    public int CompareTo(InputActionTree? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        int executionOrderComparison = ExecutionOrder.CompareTo(other.ExecutionOrder);
        return executionOrderComparison != 0 ? executionOrderComparison : Id.CompareTo(other.Id);
    }
}