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
    /// Instance was added to the pool, making it available for reuse.
    /// </summary>
    public void OnAddedToPool();
    
    /// <summary>
    /// Instance was removed from the pool to be in use.
    /// </summary>
    public void OnTakenFromPool();
    
    /// <summary>
    /// Instance was removed from the pool permanently, typically for cleanup or disposal.
    /// </summary>
    public void OnDeletedFromPool();
}