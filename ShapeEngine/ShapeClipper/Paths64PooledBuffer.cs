using Clipper2Lib;

namespace ShapeEngine.ShapeClipper;

/// <summary>
/// Maintains a reusable <see cref="Paths64"/> buffer backed by a pool of <see cref="Path64"/> instances.
/// </summary>
/// <remarks>
/// This type helps reduce allocations when repeatedly resizing collections of Clipper paths.
/// Paths moved out of <see cref="Buffer"/> are returned to the internal pool for later reuse.
/// </remarks>
public sealed class Paths64PooledBuffer
{
    private Stack<Path64> path64Pool;

    /// <summary>
    /// Gets the reusable buffer of pooled <see cref="Path64"/> instances.
    /// </summary>
    public Paths64 Buffer = new();
        
    /// <summary>
    /// Initializes a new <see cref="Paths64PooledBuffer"/> with the specified initial pool capacity.
    /// </summary>
    /// <param name="poolCapacity">The initial capacity of the internal stack used to store reusable <see cref="Path64"/> instances.</param>
    public Paths64PooledBuffer(int poolCapacity = 64)
    {
        path64Pool = new Stack<Path64>(poolCapacity);
    }

    /// <summary>
    /// Resizes <see cref="Buffer"/> to contain exactly <paramref name="targetCount"/> paths by renting or returning pooled paths as needed.
    /// </summary>
    /// <param name="targetCount">The desired number of <see cref="Path64"/> instances in <see cref="Buffer"/>.</param>
    /// <remarks>
    /// Existing path objects already stored in <see cref="Buffer"/> are preserved when possible.
    /// Excess paths are removed from the end of the buffer and returned to the pool.
    /// </remarks>
    public void PrepareBuffer(int targetCount)
    {
        if (Buffer.Count > targetCount)
        {
            for (int i = Buffer.Count - 1; i >= targetCount; i--)
            {
                var path = Buffer[i];
                Buffer.RemoveAt(i);
                ReturnPath64(path);
            }
        }
        else if (Buffer.Count < targetCount)
        {
            var diff = targetCount - Buffer.Count;
            for (int i = 0; i < diff; i++)
            {
                Buffer.Add(RentPath64());
            }
        }
    }

    /// <summary>
    /// Returns all paths currently stored in <see cref="Buffer"/> to the pool and clears the buffer.
    /// </summary>
    public void ClearBuffer()
    {
        foreach (var path in Buffer)
        {
            ReturnPath64(path);
        }
        Buffer.Clear();
    }
        
    private Path64 RentPath64()
    {
        if (path64Pool.Count > 0) return path64Pool.Pop();
        return new Path64();
    }
    private void ReturnPath64(Path64 path64)
    {
        path64Pool.Push(path64);
    }
}