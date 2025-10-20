using ShapeEngine.Core.Structs;

namespace ShapeEngine.Geometry.CollisionSystem;

/// <summary>
/// Represents a collection of colliders within a single spatial hash bucket.
/// </summary>
public class BroadphaseBucket : HashSet<Collider>
{
    /// <summary>
    /// Returns a new <see cref="BroadphaseBucket"/> containing only colliders matching the given collision mask.
    /// </summary>
    /// <param name="mask">The collision mask to filter colliders by.</param>
    /// <returns>A filtered <see cref="BroadphaseBucket"/> or null if no colliders match.</returns>
    public BroadphaseBucket? FilterObjects(BitFlag mask)
    {
        if (Count <= 0 || mask.IsEmpty()) return null;
            
        BroadphaseBucket? objects = null;
        foreach (var collidable in this)
        {
            if (mask.Has(collidable.CollisionLayer))
            {
                objects ??= new();
                objects.Add(collidable);
            }
        }
        return objects;
    }
    /// <summary>
    /// Creates a shallow copy of this <see cref="BroadphaseBucket"/>.
    /// </summary>
    /// <returns>A new <see cref="BroadphaseBucket"/> with the same colliders, or null if empty.</returns>
    public BroadphaseBucket? Copy() => Count <= 0 ? null : (BroadphaseBucket)this.ToHashSet();
}