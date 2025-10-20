namespace ShapeEngine.Geometry.CollisionSystem;

/// <summary>
/// Manages a register of colliders and their associated data for broadphase collision detection.
/// </summary>
public class BroadphaseColliderRegister<T>
{
    /// <summary>
    /// Stores the mapping between <see cref="Collider"/> instances and their associated sets of type <typeparamref name="T"/>.
    /// Used for broadphase collision detection.
    /// </summary>
    private readonly Dictionary<Collider, HashSet<T>> register = new();
    
    /// <summary>
    /// Tracks colliders that were not used in the current frame and are candidates for removal from the register.
    /// </summary>
    private readonly HashSet<Collider> unusedRegisterColliders = [];
    
    /// <summary>
    /// Adds a new entry for the specified <paramref name="collider"/> to the register or resets an existing entry.
    /// If the collider was already added this frame, returns null. Otherwise, returns a cleared or new <see cref="HashSet{T}"/> with the specified <paramref name="capacity"/>.
    /// </summary>
    /// <param name="collider">The collider to add or reset in the register.</param>
    /// <param name="capacity">The initial capacity for the associated <see cref="HashSet{T}"/>.</param>
    /// <returns>
    /// The <see cref="HashSet{T}"/> associated with the collider, or null if the collider was already added this frame.
    /// </returns>
    public HashSet<T>? AddEntry(Collider collider, int capacity)
    {
        HashSet<T> registerSet;
        if (register.TryGetValue(collider, out var value))
        {
            //already added this frame
            if (!unusedRegisterColliders.Contains(collider))
            {
                return null;
            }
            registerSet = value;
            registerSet.Clear();//clean up from last frame
            unusedRegisterColliders.Remove(collider);
        }
        else
        {
            registerSet = new HashSet<T>(capacity);
            register[collider] = registerSet;
        }
        return registerSet;
    }
    /// <summary>
    /// Attempts to retrieve the <see cref="HashSet{T}"/> associated with the specified <paramref name="collider"/>.
    /// </summary>
    /// <param name="collider">The collider whose entry is to be retrieved.</param>
    /// <param name="value">When this method returns, contains the <see cref="HashSet{T}"/> associated with the collider, if found; otherwise, null.</param>
    /// <returns>
    /// True if the entry exists in the register; otherwise, false.
    /// </returns>
    public bool TryGetEntry(Collider collider, out HashSet<T>? value)
    {
        return register.TryGetValue(collider, out value);
    }
    /// <summary>
    /// Cleans up the register by removing colliders that were not used in the current frame,
    /// and prepares the register for the next frame by marking all remaining colliders as unused.
    /// </summary>
    public void Clean()
    {
        //remaining colliders that were not used this frame. Remove them from the register.
        foreach (var collider in unusedRegisterColliders)
        {
            register.Remove(collider);
        }
        //set up for next frame
        unusedRegisterColliders.Clear();
        //all keys in register are now candidates for removal next frame if not used again
        unusedRegisterColliders.UnionWith(register.Keys);
    }
    /// <summary>
    /// Clears all entries from the register and unused colliders, effectively closing and resetting the register.
    /// </summary>
    public void Close()
    {
        register.Clear();
        unusedRegisterColliders.Clear();
    }
}