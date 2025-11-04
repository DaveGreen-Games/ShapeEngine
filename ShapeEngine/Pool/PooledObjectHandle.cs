namespace ShapeEngine.Pool;

/// <summary>
/// Handle for a pooled object returned by <see cref="ObjectPool{T}"/>.
/// Disposing this handle returns the underlying instance to the pool.
/// </summary>
/// <typeparam name="T">The reference type stored in the pool.</typeparam>
public sealed class PooledObjectHandle<T> : IDisposable where T : class
{
    /// <summary>
    /// Gets the pooled instance held by this handle.
    /// Accessing this property after the handle has been disposed will throw <see cref="ObjectDisposedException"/>.
    /// </summary>
    /// <value>The instance of <typeparamref name="T"/> provided by the pool.</value>
    /// <exception cref="ObjectDisposedException">Thrown when the handle has already been disposed.</exception>
    public T Value
    {
        get => pool == null ? throw new ObjectDisposedException(nameof(PooledObjectHandle<T>)) : value!;
        private set => this.value = value;
    }

    private T? value;

    /// <summary>
    /// Reference to the pool that provided the instance.
    /// This is set to <c>null</c> when the instance has been returned to ensure dispose is idempotent.
    /// </summary>
    private ObjectPool<T>? pool;

    /// <summary>
    /// Initializes a new instance of <see cref="PooledObjectHandle{T}"/> for the specified value and pool.
    /// </summary>
    /// <param name="value">The pooled instance. Must not be <c>null</c>.</param>
    /// <param name="pool">The <see cref="ObjectPool{T}"/> that owns the instance. Must not be <c>null</c>.</param>
    internal PooledObjectHandle(T value, ObjectPool<T> pool)
    {
        this.value = value ?? throw new ArgumentNullException(nameof(value));
        this.pool = pool ?? throw new ArgumentNullException(nameof(pool));
    }
    
    /// <summary>
    /// Gets whether this handle has been disposed.
    /// True when the internal pool reference has been cleared (the value was returned).
    /// </summary>
    public bool IsDisposed => pool == null;
    
    /// <summary>
    /// Gets whether this handle is still valid (not disposed).
    /// True when the internal pool reference is non-null and the value may be used.
    /// </summary>
    public bool IsValid => pool != null;

    /// <summary>
    /// Disposes the handle, returning the contained instance to its owning pool.
    /// </summary>
    /// <remarks>
    /// This method is idempotent and thread-safe:
    /// it atomically clears the internal pool reference so that only the first caller actually returns the instance.
    /// After disposal, accessing <see cref="Value"/> throws <see cref="ObjectDisposedException"/>.
    /// </remarks>
    public void Dispose()
    {
        var p = Interlocked.Exchange(ref pool, null);
        if (p != null)
        {
            p.Return(value!);
            value = null;
        }
    }
    
}