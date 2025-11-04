namespace ShapeEngine.Pool;

/// <summary>
/// Handle for a pooled object returned by <see cref="ObjectPool{T}"/>.
/// Disposing this handle returns the underlying instance to the pool.
/// </summary>
/// <typeparam name="T">The reference type stored in the pool.</typeparam>
public sealed class PooledObjectHandle<T> : IDisposable where T : class
{
    /// <summary>
    /// The pooled instance held by this handle.
    /// Disposing the handle returns this instance to the pool and clears this property.
    /// </summary>
    public T Value { get; private set; }

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
        Value = value ?? throw new ArgumentNullException(nameof(value));
        this.pool = pool ?? throw new ArgumentNullException(nameof(pool));
    }

    /// <summary>
    /// Disposes the handle, returning the contained instance to its owning pool.
    /// </summary>
    /// <remarks>
    /// This method is idempotent and thread-safe: it atomically clears the
    /// internal pool reference so that only the first caller actually returns
    /// the instance. After disposal, <see cref="Value"/> is set to <c>null</c>.
    /// </remarks>
    public void Dispose()
    {
        // Atomically set pool to null and get the previous value.
        // Only the first caller receives the non-null pool and performs the return.
        var p = Interlocked.Exchange(ref pool, null);
        if (p != null)
        {
            p.Return(Value);
            Value = null!;
        }
    }
}

// Disposable handle that returns the instance to the pool on Dispose.
// public sealed class PooledObject<T> : IDisposable where T : class
// {
//     public T Value { get; private set; }
//     private ShapePool<T>? pool;
//     private bool disposed;
//
//     internal PooledObject(T value, ShapePool<T> pool)
//     {
//         Value = value ?? throw new ArgumentNullException(nameof(value));
//         this.pool = pool ?? throw new ArgumentNullException(nameof(pool));
//     }
//
//     public void Dispose()
//     {
//         if (disposed) return;
//         disposed = true;
//
//         var p = pool;
//         pool = null;
//
//         if (p != null && Value != null)
//         {
//             p.Return(Value);
//         }
//
//         Value = null!;
//     }
// }


// public ref struct PooledObjectHandle<T> where T : class
// {
//     public T Value { get; private set; }
//     private ObjectPool<T>? pool;
//
//     internal PooledObjectHandle(T value, ObjectPool<T> objectPool)
//     {
//         Value = value ?? throw new ArgumentNullException(nameof(value));
//         pool = objectPool ?? throw new ArgumentNullException(nameof(objectPool));
//     }
//     public void Dispose()
//     {
//         // Make dispose idempotent and safe against copies using atomic exchange.
//         var p = Interlocked.Exchange(ref pool, null);
//         if (p != null && Value != null)
//         {
//             p.Return(Value);
//             Value = null!;
//         }
//     }
// }