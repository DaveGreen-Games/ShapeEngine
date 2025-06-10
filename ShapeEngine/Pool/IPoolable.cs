namespace ShapeEngine.Pool;

/// <summary>
/// Represents an object that can be managed by a pool. Provides events and methods for pool lifecycle management.
/// </summary>
public interface IPoolable
{
    /// <summary>
    /// Event triggered when the instance has finished its work and is ready to be returned to the pool.
    /// </summary>
    public event Action<IPoolable>? OnInstanceFinished;
    /// <summary>
    /// Returns the instance to its pool, making it available for reuse.
    /// </summary>
    public void ReturnToPool(); //Todo: Should be called ReturnedToPool() and should be a simple informational event
    /// <summary>
    /// Removes the instance from its pool, typically for cleanup or disposal.
    /// </summary>
    public void RemoveFromPool();//Todo: Should be called RemovedFromPool() and should be a simple informational event
}