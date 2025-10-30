using System.Collections.Concurrent;
using System.Numerics;
using ShapeEngine.Geometry.RectDef;

namespace ShapeEngine.Pathfinding;

/// <summary>
/// Represents a path used by the pathfinding system.
/// Contains the start and end positions and the sequence of rectangles that form the path.
/// Instances are intended to be rented from and returned to an internal pool
/// using <see cref="RentPath(Vector2,Vector2,int)"/> and <see cref="ReturnPath(Path)"/> to minimize allocations.
/// </summary>
/// <remarks>
/// Returned instances will have their <see cref="Rects"/> collection cleared before being reused.
/// Returned instances should not be accessed after being returned to the pool.
/// </remarks>
public class Path
{
    private static readonly ConcurrentQueue<Path> PathPool = new();
    
    /// <summary>
    /// Rent a <see cref="Path"/> instance from the pool or create a new one if none are available.
    /// The returned instance will have its <see cref="Start"/> and <see cref="End"/> properties set.
    /// </summary>
    /// <param name="start">The starting position of the path.</param>
    /// <param name="end">The ending position of the path.</param>
    /// <param name="capacity">Initial capacity for the internal <see cref="Rects"/> list.</param>
    /// <returns>A <see cref="Path"/> instance ready for use.</returns>
    public static Path RentPath(Vector2 start, Vector2 end, int capacity)
    {
        if (!PathPool.TryDequeue(out var instance))
        {
            return new Path(start, end, capacity);
        }

        instance.Start = start;
        instance.End = end;
        if (instance.Rects.Capacity < capacity)
        {
            instance.Rects.Capacity = capacity;
        }
        return instance;
    }
    
    /// <summary>
    /// Returns a rented <see cref="Path"/> instance to the internal pool.
    /// The path's <see cref="Rects"/> collection will be cleared by the caller
    /// </summary>
    /// <param name="path">The <see cref="Path"/> instance to enqueue for reuse.</param>
    public static void ReturnPath(Path path)
    {
        path.Rects.Clear();
        PathPool.Enqueue(path);
    }
    
    /// <summary>
    /// Returns a collection of rented <see cref="Path"/> instances to the internal pool.
    /// Each path will be cleared and enqueued via <see cref="ReturnPath(Path)"/>; after calling this
    /// the caller must not access the returned instances. 
    /// </summary>
    /// <param name="paths">Enumerable of <see cref="Path"/> instances to return.</param>
    /// <remarks>
    /// <paramref name="paths"/> should be cleared after calling this method to avoid accidental access
    /// to returned instances.
    /// </remarks>
    public static void ReturnPaths(IEnumerable<Path> paths)
    {
        foreach (var path in paths)
        {
            ReturnPath(path);
        }
    }

    /// <summary>
    /// The starting position of the path.
    /// </summary>
    public Vector2 Start;
    /// <summary>
    /// The ending position of the path.
    /// </summary>
    public Vector2 End;
    /// <summary>
    /// The list of rectangles (cells) that make up the path.
    /// </summary>
    public readonly List<Rect> Rects;
    
    /// <summary>
    /// Gets a value indicating whether this path contains at least one rectangle.
    /// </summary>
    /// <remarks>
    /// A path is considered valid when its <see cref="Rects"/> collection is not empty.
    /// </remarks>
    public bool IsValid => Rects.Count > 0;
    

    private Path(Vector2 start, Vector2 end, int capacity)
    {
        Start = start;
        End = end;
        Rects = new List<Rect>(capacity);
    }

    /// <summary>
    /// Returns this rented <see cref="Path"/> instance to the internal pool for reuse.
    /// </summary>
    /// <remarks>
    /// This method forwards to <see cref="ReturnPath(Path)"/>. After calling this, the instance
    /// may be reused by other code and should not be accessed by the caller.
    /// </remarks>
    public void ReturnInstance()
    {
        ReturnPath(this);
    }
}