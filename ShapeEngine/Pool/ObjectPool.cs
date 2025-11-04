using System.Collections.Concurrent;

namespace ShapeEngine.Pool;

/// <summary>
/// A thread-safe object pool for reusing instances of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The reference type being pooled.</typeparam>
/// <remarks>
/// Uses a thread-safe <see cref="System.Collections.Concurrent.ConcurrentBag{T}"/> to store available instances.
/// Provide a <see cref="System.Func{T}"/> to create new instances and an optional
/// <see cref="System.Action{T}"/> to reset instances when they are returned to the pool.
/// If extension is needed, prefer a wrapper that delegates to <c>ObjectPool</c> rather than deriving from it.
/// </remarks>
public sealed class ObjectPool<T> where T : class
{
    /// <summary>
    /// Thread-safe container holding available instances ready to be reused.
    /// </summary>
    /// <remarks>
    /// Backed by a <see cref="ConcurrentBag{T}"/> to allow concurrent add/take operations.
    /// </remarks>
    private readonly ConcurrentBag<T> objects;

    /// <summary>
    /// Factory delegate used to create new instances when the pool has none available.
    /// </summary>
    private readonly Func<T> objectGenerator;

    /// <summary>
    /// Optional delegate invoked to reset an instance before it's returned to the pool.
    /// If null, no reset is performed.
    /// </summary>
    private readonly Action<T>? objectResetter;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectPool{T}"/> class.
    /// </summary>
    /// <param name="objectGenerator">A factory function used to create new instances when the pool is empty. Must not be null.</param>
    /// <param name="objectResetter">An optional action invoked to reset an instance before it is returned to the pool.</param>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="objectGenerator"/> is null.</exception>
    public ObjectPool(Func<T> objectGenerator, Action<T>? objectResetter = null)
    {
        this.objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
        this.objectResetter = objectResetter;
        objects = [];
    }

    /// <summary>
    /// Retrieves an instance from the pool if one is available; otherwise creates a new instance
    /// using the factory provided to the pool constructor.
    /// </summary>
    /// <returns>
    /// A pooled instance of <typeparamref name="T"/>. The caller should return the instance
    /// to the pool by calling <see cref="Return(T)"/> when finished.
    /// </returns>
    public T Get()
    {
        return objects.TryTake(out var instance) ? instance : objectGenerator();
    }

    /// <summary>
    /// Returns an instance to the pool. The instance will be reset using the optional
    /// reset delegate (if provided) and then added back to the internal storage for reuse.
    /// </summary>
    /// <param name="instance">The instance to return to the pool. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is null.</exception>
    public void Return(T instance)
    {
        ArgumentNullException.ThrowIfNull(instance);
        objectResetter?.Invoke(instance);
        objects.Add(instance);
    }

    /// <summary>
    /// Removes all objects currently stored in the pool's internal container.
    /// </summary>
    /// <remarks>
    /// This clears only the available instances held by the pool; instances already checked out
    /// via <see cref="Get"/> are not affected. The underlying operation uses the thread-safe
    /// <see cref="System.Collections.Concurrent.ConcurrentBag{T}"/> implementation.
    /// </remarks>
    public void Clear()
    {
        objects.Clear();
    }
    
    /// <summary>
    /// Creates a disposable handle that wraps a pooled instance.
    /// The handle will return the instance to this pool when the handle is disposed.
    /// </summary>
    /// <returns>
    /// A <see cref="PooledObjectHandle{T}"/> containing a pooled <typeparamref name="T"/> instance.
    /// </returns>
    /// <remarks>
    /// Use the returned handle in a `using` statement (or dispose it manually) to ensure the instance
    /// is returned to the pool promptly. This method obtains a single instance via <see cref="Get"/>.
    /// </remarks>
    public PooledObjectHandle<T> GetHandle()
    {
        var instance = Get();
        return new PooledObjectHandle<T>(instance, this);
    }
}

