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
/// A different tree for each used gamepad is recommended.
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

    #region Class
    
    private static uint idCounter = 0;
    /// <summary>
    /// The unique identifier for this <see cref="InputActionTree"/> instance.
    /// </summary>
    public readonly uint Id = idCounter++;

    
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
            action.Update(dt, inputTypeBlockSet);
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
            CurrentGamepad = CurrentGamepad
        };
        foreach (var action in this) copy.Add(action.Copy());
        
        return copy;
    }

    #endregion
    
    /// <summary>
    /// Compares this <see cref="InputActionTree"/> to another for sorting purposes.
    /// Comparison is based on the unique <see cref="Id"/> of each tree.
    /// </summary>
    /// <param name="other">The other <see cref="InputActionTree"/> to compare to.</param>
    /// <returns>
    /// 0 if both instances are the same; 1 if <paramref name="other"/> is null; 
    /// otherwise, the result of comparing <see cref="Id"/> values.
    /// </returns>
    public int CompareTo(InputActionTree? other)
    {
        if (other is null) return 1;
        if (ReferenceEquals(this, other)) return 0;
        return Id.CompareTo(other.Id);
    }


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
}