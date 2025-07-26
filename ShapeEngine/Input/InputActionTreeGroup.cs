using ShapeEngine.Core;

namespace ShapeEngine.Input;


/// <summary>
/// Represents a sorted collection of <see cref="InputActionTree"/> objects.
/// Provides methods for updating and retrieving input action trees based on various criteria.
/// </summary>
public class InputActionTreeGroup : SortedSet<InputActionTree>, ICopyable<InputActionTreeGroup>, IComparable<InputActionTreeGroup>, IEquatable<InputActionTreeGroup>
{
 
    public readonly HashSet<IInputType> UsedInputs = [];
    
    /// <summary>
    /// Updates all contained <see cref="InputActionTree"/> objects with the given delta time.
    /// Returns the first non-None <see cref="InputDeviceType"/> detected during the update,
    /// or <see cref="InputDeviceType.None"/> if none are detected.
    /// </summary>
    /// <param name="dt">The delta time to pass to each tree's update.</param>
    /// <returns>
    /// The <see cref="InputDeviceType"/> used during the update, or <see cref="InputDeviceType.None"/> if none.
    /// </returns>
    public InputDeviceType Update(float dt)
    {
        UsedInputs.Clear();
        var usedDeviceType = InputDeviceType.None;
        foreach (var tree in this)
        {
            var updated = tree.Update(dt, out var deviceType);
            UsedInputs.UnionWith(tree.UsedInputs);
            if (!updated || deviceType == InputDeviceType.None || usedDeviceType != InputDeviceType.None) continue;
            usedDeviceType = deviceType;
        }
        return usedDeviceType;
    }
    

    /// <summary>
    /// Adds an <see cref="InputActionTree"/> to the collection.
    /// If the tree is successfully added, calls <c>EnterTrees</c> on the tree with this collection.
    /// </summary>
    /// <param name="tree">The <see cref="InputActionTree"/> to add.</param>
    /// <returns><c>true</c> if the tree was added; otherwise, <c>false</c>.</returns>
    public new bool Add(InputActionTree tree)
    {
        var added = base.Add(tree);
        
        if (added)
        {
            tree.EnterGroup(this);
        }
    
        return added;
    }

    /// <summary>
    /// Removes an <see cref="InputActionTree"/> from the collection.
    /// If the tree is successfully removed, calls <c>ExitTrees</c> on the tree.
    /// </summary>
    /// <param name="tree">The <see cref="InputActionTree"/> to remove.</param>
    /// <returns><c>true</c> if the tree was removed; otherwise, <c>false</c>.</returns>
    public new bool Remove(InputActionTree tree)
    {
        var removed = base.Remove(tree);
    
        if (removed)
        {
            tree.ExitGroup();
        }
        
        return removed;
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
    
    #endregion

    /// <summary>
    /// Creates a deep copy of this <see cref="InputActionTreeGroup"/> collection,
    /// including deep copies of all contained <see cref="InputActionTree"/> objects.
    /// </summary>
    public InputActionTreeGroup Copy()
    {
        var copy = new InputActionTreeGroup();
        foreach (var tree in this)
        {
            copy.Add(tree.Copy());
        }
        return copy;
    }

    /// <summary>
    /// Compares this <see cref="InputActionTreeGroup"/> instance with another, returning:
    /// <list type="bullet">
    /// <item><description>1 if <paramref name="other"/> is <c>null</c> (this instance is considered greater)</description></item>
    /// <item><description>0 if both references are the same instance</description></item>
    /// <item><description>Otherwise, compares by the count of contained <see cref="InputActionTree"/> objects</description></item>
    /// </list>
    /// </summary>
    /// <param name="other">The other <see cref="InputActionTreeGroup"/> instance to compare with.</param>
    /// <returns>
    /// An integer indicating the relative order of the objects being compared.
    /// </returns>
    public int CompareTo(InputActionTreeGroup? other)
    {
        if (other == null) return 1; // Null is considered less than any instance
        if (ReferenceEquals(this, other)) return 0; // Same instance
        // Compare by the count of input action trees
        return Count.CompareTo(other.Count);
    }


    /// <summary>
    /// Determines whether the specified <see cref="InputActionTreeGroup"/> is equal to the current instance.
    /// Equality is based on reference, count, and sequence equality of contained <see cref="InputActionTree"/> objects.
    /// </summary>
    /// <param name="other">The <see cref="InputActionTreeGroup"/> to compare with the current instance.</param>
    /// <returns><c>true</c> if the specified object is equal to the current instance; otherwise, <c>false</c>.</returns>
    public bool Equals(InputActionTreeGroup? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (Count != other.Count) return false;
        return this.SequenceEqual(other);
    }
    
    /// <summary>
    /// Determines whether the specified object is equal to the current <see cref="InputActionTreeGroup"/> instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><c>true</c> if the specified object is equal to the current instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((InputActionTreeGroup)obj);
    }
    
    /// <summary>
    /// Returns a hash code for the current <see cref="InputActionTreeGroup"/> instance,
    /// based on the hash codes of the contained <see cref="InputActionTree"/> objects.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
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