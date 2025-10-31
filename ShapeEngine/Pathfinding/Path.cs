using System.Collections.Concurrent;
using System.Numerics;
using ShapeEngine.Geometry.RectDef;

namespace ShapeEngine.Pathfinding;

/// <summary>
/// Represents a path used by the pathfinding system.
/// Contains the start and end positions and the sequence of rectangles that form the path.
/// Instances are intended to be rented from and returned to an internal pool
/// using <see cref="Path.RentPath(Vector2,Vector2,int)"/> and <see cref="Path.ReturnPath(Path)"/> to minimize allocations.
/// </summary>
/// <remarks>
/// Returned instances will have their <see cref="Rects"/> collection cleared before being reused.
/// Returned instances should not be accessed after being returned to the pool.
/// </remarks>
public class Path
{
    #region Pooling
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
        if (instance.RectsList.Capacity < capacity)
        {
            instance.RectsList.Capacity = capacity;
        }

        instance.rented = true;
        return instance;
    }
    
    /// <summary>
    /// Returns a rented <see cref="Path"/> instance to the internal pool.
    /// The path's <see cref="Rects"/> collection will be cleared by the caller
    /// </summary>
    /// <param name="path">The <see cref="Path"/> instance to enqueue for reuse.</param>
    public static void ReturnPath(Path path)
    {
        if (!path.rented) return;
        path.RectsList.Clear();
        path.rented = false;
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

    #endregion
    
    #region Members

    /// <summary>
    /// The starting position of the path.
    /// </summary>
    public Vector2 Start { get; private set; }

    /// <summary>
    /// The ending position of the path.
    /// </summary>
    public Vector2 End { get; private set; }


    /// <summary>
    /// Internal list that stores the sequence of <see cref="Rect"/> instances composing the path.
    /// This is the mutable backing store for the public <see cref="Rects"/> property.
    /// </summary>
    /// <remarks>
    /// Intended for internal use only. Do not modify directly from external code; use the public APIs
    /// such as <see cref="GetRectsCopy"/> or the pool rent/return methods. The list is cleared when a
    /// <see cref="Path"/> is returned to the pool.
    /// </remarks>
    internal readonly List<Rect> RectsList;
    /// <summary>
    /// Indicates whether this instance is currently rented from the internal pool.
    /// When true the instance is considered in-use and should not be enqueued again
    /// or accessed after being returned via <see cref="ReturnInstance"/> / <see cref="ReturnPath(Path)"/>.
    /// </summary>
    private bool rented;
    #endregion

    #region Getters
    
    /// <summary>
    /// Provides a read-only view of the internal list of rectangles that compose this path.
    /// </summary>
    /// <remarks>
    /// Returns the internal <see cref="RectsList"/> as an <see cref="IReadOnlyList{Rect}"/>.
    /// Treat the returned collection as read-only: the underlying list is cleared when the
    /// instance is returned to the pool and must not be retained or modified by callers.
    /// </remarks>
    public IReadOnlyList<Rect> Rects => RectsList; 
    /// <summary>
    /// Creates and returns a new list containing the rectangles from the internal backing list.
    /// </summary>
    /// <returns>
    /// A new <see cref="List{Rect}"/> containing the path's rectangles. The returned list is a copy and
    /// can be modified independently of the <see cref="Path"/> instance.
    /// </returns>
    /// <remarks>
    /// This method allocates a new list. Prefer using the <see cref="Rects"/> property for a non-allocating
    /// read-only view when possible. Do not rely on the original <see cref="Path"/> after it has been
    /// returned to the pool; the returned copy is the only safe representation to hold beyond that.
    /// </remarks>
    public List<Rect> GetRectsCopy()
    {
        return RectsList.ToList();
    }
    /// <summary>
    /// Gets a value indicating whether this path contains at least one rectangle.
    /// </summary>
    /// <remarks>
    /// A path is considered valid when its <see cref="Rects"/> collection is not empty.
    /// </remarks>
    public bool IsValid => RectsList.Count > 0;
    #endregion
    
    #region Constructor
    private Path(Vector2 start, Vector2 end, int capacity)
    {
        Start = start;
        End = end;
        RectsList = new List<Rect>(capacity);
        rented = true;
    }
    private Path(Vector2 start, Vector2 end, List<Rect> rectsList)
    {
        Start = start;
        End = end;
        RectsList = rectsList;
        rented = true;
    }
    #endregion

    #region Methods
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

    /// <summary>
    /// Creates a deep copy of this <see cref="Path"/>.
    /// </summary>
    /// <returns>
    /// A new <see cref="Path"/> instance with the same <see cref="Start"/> and <see cref="End"/> values
    /// and an independent copy of the internal rectangles list (created via <see cref="GetRectsCopy"/>).
    /// The returned instance is marked as rented; callers can return it to the pool using
    /// <see cref="ReturnInstance"/> / <see cref="ReturnPath(Path)"/> when finished or keep it for independent use.
    /// Once returned to the pool, the original instance should not be accessed further.
    /// </returns>
    public Path Copy()
    {
        return new Path(Start, End, GetRectsCopy());
    }
    
    #endregion
}