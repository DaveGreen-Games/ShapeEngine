using ShapeEngine.Geometry.RectDef;

namespace ShapeEngine.Geometry.CollisionSystem;

/// <summary>
/// Manages registration and tracking of static colliders and their associated data for broadphase collision detection.
/// </summary>
public class BroadphaseStaticColliderRegister<T>
{
    /// <summary>
    /// Stores the mapping between registered <see cref="Collider"/> instances and their associated <see cref="Rect"/> bounds.
    /// Used for broadphase collision detection.
    /// </summary>
    private readonly Dictionary<Collider, Rect> registerRects = new();
    /// <summary>
    /// Stores the mapping between registered <see cref="Collider"/> instances and their associated sets of type <typeparamref name="T"/>.
    /// Each collider is mapped to a <see cref="HashSet{T}"/> containing relevant data for broadphase collision detection.
    /// </summary>
    private readonly Dictionary<Collider, HashSet<T>> register = new();
    /// <summary>
    /// Tracks colliders that were not used during the current frame.
    /// These colliders are candidates for removal from the register in the next cleanup cycle.
    /// </summary>
    private readonly HashSet<Collider> unusedRegisterColliders = [];
    /// <summary>
    /// Adds a new entry for the specified <paramref name="collider"/> to the register.
    /// If the collider already exists, retrieves its associated <see cref="HashSet{T}"/> and <see cref="Rect"/>.
    /// Otherwise, creates a new entry with the given <paramref name="capacity"/> for the <see cref="HashSet{T}"/>.
    /// Returns <c>true</c> if the collider was already registered; <c>false</c> if a new entry was created.
    /// </summary>
    /// <param name="collider">The collider to register or retrieve.</param>
    /// <param name="capacity">Initial capacity for the <see cref="HashSet{T}"/> if a new entry is created.</param>
    /// <param name="coordinates">The set of type <typeparamref name="T"/> associated with the collider.</param>
    /// <param name="rect">The bounding <see cref="Rect"/> associated with the collider.</param>
    /// <returns><c>true</c> if the collider was already registered; otherwise, <c>false</c>.</returns>
    public bool AddEntry(Collider collider, int capacity, out HashSet<T> coordinates, out Rect rect)
    {
        if (register.TryGetValue(collider, out var value))
        {
            unusedRegisterColliders.Remove(collider);
            coordinates =  value;
            rect = registerRects[collider];
            return true;
        }
        coordinates = new HashSet<T>(capacity);
        register[collider] = coordinates;
        registerRects[collider] = new Rect();
        rect = new();
        return false;
    }
    /// <summary>
    /// Updates the bounding <see cref="Rect"/> associated with the specified <paramref name="collider"/>.
    /// Returns <c>true</c> if the collider exists in the register; otherwise, <c>false</c>.
    /// </summary>
    /// <param name="collider">The collider whose bounding rect is to be updated.</param>
    /// <param name="rect">The new bounding <see cref="Rect"/> to associate with the collider.</param>
    /// <returns><c>true</c> if the update was successful; otherwise, <c>false</c>.</returns>
    public bool UpdateRect(Collider collider, Rect rect)
    {
        if (!registerRects.ContainsKey(collider)) return false;
        registerRects[collider] = rect;
        return true;

    }
    /// <summary>
    /// Removes colliders that were not used during the current frame from the register.
    /// Prepares the register for the next frame by marking all remaining colliders as unused.
    /// </summary>
    public void Clean()
    {
        //remaining colliders that were not used this frame. Remove them from the register.
        foreach (var collider in unusedRegisterColliders)
        {
            register.Remove(collider);
            registerRects.Remove(collider);
        }
        //set up for next frame
        unusedRegisterColliders.Clear();
        //all keys in register are now candidates for removal next frame if not used again
        unusedRegisterColliders.UnionWith(register.Keys);
    }
    /// <summary>
    /// Clears all registered colliders, their associated bounding rects, and unused collider tracking.
    /// Effectively resets the register to its initial state.
    /// </summary>
    public void Close()
    {
        register.Clear();
        registerRects.Clear();
        unusedRegisterColliders.Clear();
    }
}