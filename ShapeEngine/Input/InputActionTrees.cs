namespace ShapeEngine.Input;

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
    
    #endregion
    
}