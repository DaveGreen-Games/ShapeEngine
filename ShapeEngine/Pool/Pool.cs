using ShapeEngine.Core;

namespace ShapeEngine.Pool;

/// <summary>
/// Represents a generic object pool for managing reusable instances of <see cref="IPoolable"/> objects.
/// </summary>
public class Pool : IPool
{
    private static readonly IdCounter IdCounter = new();
    
    private int count;
    private readonly int maxSize;
    private readonly List<IPoolable> usable = [];
    private readonly List<IPoolable> inUse = [];
    private readonly Func<IPoolable> createInstance;

    private readonly uint id;

    /// <summary>
    /// Initializes a new instance of the <see cref="Pool"/> class with the specified start size,
    /// instance creation function, and optional maximum size.
    /// </summary>
    /// <param name="startSize">The initial number of instances to create in the pool.</param>
    /// <param name="createInstance">A function that creates a new <see cref="IPoolable"/> instance.</param>
    /// <param name="maxSize">The maximum number of instances allowed in the pool.
    /// If negative, the pool size is unlimited.</param>
    public Pool(int startSize, Func<IPoolable> createInstance, int maxSize = -1)
    {
        this.maxSize = maxSize;
        if (maxSize > 0) startSize = (int)MathF.Min(startSize, maxSize);
        this.count = startSize;
        this.createInstance = createInstance;
        for (var i = 0; i < startSize; i++)
        {
            var instance = this.createInstance();
            instance.OnInstanceFinished += OnInstanceFinished;
            usable.Add(instance);
        }

        this.id = IdCounter.NextId;
    }


    /// <inheritdoc/>
    public uint GetId() => id;


    /// <inheritdoc/>
    public void Clear()
    {
        foreach (var item in usable)
        {
            item.RemoveFromPool();
        }
        usable.Clear();
        foreach (var item in inUse)
        {
            item.ReturnToPool();
            item.RemoveFromPool();
        }
        inUse.Clear();
        count = 0;
    }


    /// <inheritdoc/>
    public bool HasUsableInstances() { return usable.Count > 0; }

    /// <inheritdoc/>
    public bool HasInstances() { return count > 0; }
    /// <summary>
    /// Retrieves an available instance from the pool.
    /// If none are available, creates a new one or reuses the oldest in-use instance if the pool is at maximum size.
    /// </summary>
    /// <returns>An <see cref="IPoolable"/> instance.</returns>
    public IPoolable GetInstance()
    {
        if (usable.Count <= 0)
        {
            if (maxSize <= 0 || count < maxSize)
            {
                var newInstance = createInstance();
                newInstance.OnInstanceFinished += OnInstanceFinished;
                usable.Add(newInstance);
                count++;
            }
            else //reuse instance from in-use stack
            {
                var oldest = PopBack(inUse);
                oldest.ReturnToPool();
                usable.Add(oldest);
            }
        }
        var instance = Pop(usable);
        inUse.Add(instance);
        return instance;
    }

    /// <inheritdoc/>
    public T GetInstance<T>()
    {
        return (T)GetInstance();
    }

    /// <inheritdoc/>
    public void ReturnInstance(IPoolable instance)
    {
        if (usable.Contains(instance)) return;
        if (!inUse.Contains(instance)) return;
        instance.ReturnToPool();
        usable.Add(instance);
    }


    /// <inheritdoc/>
    public void OnInstanceFinished(IPoolable instance)
    {
        //TODO: Would it not be better if ReturnInstance() and OnInstanceFinished() were private or removed altogether from the IPool interface?
        // An IPoolable instance should trigger an event if finished and then it should automatically return to the pool...
        // why this complicated logic?
        ReturnInstance(instance);
    }

    /// <summary>
    /// Removes and returns the last instance from the specified list.
    /// </summary>
    /// <param name="list">The list to pop from.</param>
    /// <returns>The last <see cref="IPoolable"/> instance in the list.</returns>
    private static IPoolable Pop(List<IPoolable> list)
    {
        int index = list.Count - 1;
        var instance = list[index];
        list.RemoveAt(index);
        return instance;
    }
    /// <summary>
    /// Removes and returns the first instance from the specified list.
    /// </summary>
    /// <param name="list">The list to pop from.</param>
    /// <returns>The first <see cref="IPoolable"/> instance in the list.</returns>
    private static IPoolable PopBack(List<IPoolable> list)
    {
        int index = 0;
        var instance = list[index];
        list.RemoveAt(index);
        return instance;
    }
}
