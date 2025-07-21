using ShapeEngine.Core;

namespace ShapeEngine.Input;


/// <summary>
/// Represents a sorted collection of <see cref="InputActionTree"/> objects.
/// Provides methods for updating and retrieving input action trees based on various criteria.
/// </summary>
public class InputActionTrees : SortedSet<InputActionTree>, ICopyable<InputActionTrees>, IComparable<InputActionTrees>
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
    
    #endregion

    /// <summary>
    /// Creates a deep copy of this <see cref="InputActionTrees"/> collection,
    /// including deep copies of all contained <see cref="InputActionTree"/> objects.
    /// </summary>
    public InputActionTrees Copy()
    {
        var copy = new InputActionTrees();
        foreach (var tree in this)
        {
            copy.Add(tree.Copy());
        }
        return copy;
    }

    /// <summary>
    /// Compares this <see cref="InputActionTrees"/> instance with another, returning:
    /// <list type="bullet">
    /// <item><description>1 if <paramref name="other"/> is <c>null</c> (this instance is considered greater)</description></item>
    /// <item><description>0 if both references are the same instance</description></item>
    /// <item><description>Otherwise, compares by the count of contained <see cref="InputActionTree"/> objects</description></item>
    /// </list>
    /// </summary>
    /// <param name="other">The other <see cref="InputActionTrees"/> instance to compare with.</param>
    /// <returns>
    /// An integer indicating the relative order of the objects being compared.
    /// </returns>
    public int CompareTo(InputActionTrees? other)
    {
        if (other == null) return 1; // Null is considered less than any instance
        if (ReferenceEquals(this, other)) return 0; // Same instance
        // Compare by the count of input action trees
        return Count.CompareTo(other.Count);
    }
}