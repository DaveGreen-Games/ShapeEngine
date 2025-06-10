namespace ShapeEngine.Pool;

/// <summary>
/// Defines the interface for an object pool, providing methods for managing pooled instances.
/// </summary>
public interface IPool
{
    /// <summary>
    /// Gets the unique identifier for this pool.
    /// </summary>
    /// <returns>The unique pool ID.</returns>
    public uint GetId();
    /// <summary>
    /// Clears all instances from the pool, resetting its state.
    /// </summary>
    public void Clear();
    /// <summary>
    /// Determines whether the pool has any usable (available) instances.
    /// </summary>
    /// <returns>True if there are usable instances; otherwise, false.</returns>
    public bool HasUsableInstances(); 
    /// <summary>
    /// Determines whether the pool contains any instances (usable or in use).
    /// </summary>
    /// <returns>True if the pool contains any instances; otherwise, false.</returns>
    public bool HasInstances();
    /// <summary>
    /// Retrieves an available instance from the pool.
    /// </summary>
    /// <returns>An <see cref="IPoolable"/> instance if available; otherwise, null.</returns>
    public IPoolable? GetInstance();
    /// <summary>
    /// Retrieves an available instance of type <typeparamref name="T"/> from the pool.
    /// </summary>
    /// <typeparam name="T">The type of instance to retrieve.</typeparam>
    /// <returns>An instance of type <typeparamref name="T"/> if available; otherwise, null.</returns>
    public T? GetInstance<T>();
    /// <summary>
    /// Returns an instance to the pool, making it available for reuse.
    /// </summary>
    /// <param name="instance">The instance to return.</param>
    public void ReturnInstance(IPoolable instance);
    /// <summary>
    /// Handles the event when an instance has finished its work and should be returned to the pool.
    /// </summary>
    /// <param name="instance">The finished instance.</param>
    public void OnInstanceFinished(IPoolable instance);
}