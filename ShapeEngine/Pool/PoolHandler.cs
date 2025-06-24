namespace ShapeEngine.Pool;

/// <summary>
/// Handles and manages multiple object pools implementing the <see cref="IPool"/> interface.
/// Provides methods to add, remove, clear, and retrieve pools and their instances.
/// </summary>
public class PoolHandler
{
    /// <summary>
    /// Dictionary mapping pool IDs to their corresponding <see cref="IPool"/> instances.
    /// </summary>
    private readonly Dictionary<uint, IPool> pools = new();

    /// <summary>
    /// Checks if a pool with the specified ID exists.
    /// </summary>
    /// <param name="poolId">The unique ID of the pool.</param>
    /// <returns>True if the pool exists; otherwise, false.</returns>
    public bool ContainsPool(uint poolId) { return pools.ContainsKey(poolId); }

    /// <summary>
    /// Adds a pool to the handler. If a pool with the same ID exists, it is cleared and replaced.
    /// </summary>
    /// <param name="pool">The pool to add.</param>
    public void AddPool(IPool pool)
    {
        var id = pool.GetId();
        if (ContainsPool(id))
        {
            pools[id].Clear();
            pools[id] = pool;
        }
        else pools.Add(id, pool);
    }

    /// <summary>
    /// Adds multiple pools to the handler.
    /// </summary>
    /// <param name="newPools">An array of pools to add.</param>
    public void AddPools(params IPool[] newPools)
    {
        foreach (var pool in newPools)
        {
            AddPool(pool);
        }
    }

    /// <summary>
    /// Removes a pool with the specified ID, clearing its contents first.
    /// </summary>
    /// <param name="id">The unique ID of the pool to remove.</param>
    public void RemovePool(uint id)
    {
        if (!ContainsPool(id)) return;
        pools[id].Clear();
        pools.Remove(id);
    }

    /// <summary>
    /// Removes multiple pools by their IDs.
    /// </summary>
    /// <param name="ids">An array of pool IDs to remove.</param>
    public void RemovePools(params uint[] ids)
    {
        foreach (var id in ids)
        {
            RemovePool(id);
        }
    }

    /// <summary>
    /// Removes all pools, clearing their contents first.
    /// </summary>
    public void RemoveAllPools()
    {
        ClearAllPools();
        pools.Clear();
    }

    /// <summary>
    /// Retrieves a pool by its ID.
    /// </summary>
    /// <param name="id">The unique ID of the pool.</param>
    /// <returns>The <see cref="IPool"/> if found; otherwise, null.</returns>
    public IPool? GetPool(uint id)
    {
        if (!ContainsPool(id)) return null;
        return pools[id];
    }

    /// <summary>
    /// Retrieves a pool by its ID and type.
    /// </summary>
    /// <typeparam name="T">The type of pool to retrieve.</typeparam>
    /// <param name="id">The unique ID of the pool.</param>
    /// <returns>The pool of type <typeparamref name="T"/> if found and type matches; otherwise, default value.</returns>
    public T? GetPool<T>(uint id) where T : IPool
    {
        if (!ContainsPool(id)) return default(T);
        var pool = pools[id];
        if (pool is T) return (T)pool;
        else return default(T);
    }

    /// <summary>
    /// Clears the contents of a specific pool by its ID.
    /// </summary>
    /// <param name="id">The unique ID of the pool to clear.</param>
    public void ClearPool(uint id)
    {
        if (!ContainsPool(id)) return;
        pools[id].Clear();
    }

    /// <summary>
    /// Clears the contents of all pools.
    /// </summary>
    public void ClearAllPools()
    {
        foreach (var pool in pools.Values)
        {
            pool.Clear();
        }
    }

    /// <summary>
    /// Retrieves an instance of type <typeparamref name="T"/> from the pool with the specified ID.
    /// </summary>
    /// <typeparam name="T">The type of instance to retrieve, must implement <see cref="IPoolable"/>.</typeparam>
    /// <param name="id">The unique ID of the pool.</param>
    /// <returns>An instance of type <typeparamref name="T"/> if available; otherwise, default value.</returns>
    public T? GetInstance<T>(uint id) where T : IPoolable
    {
        if (!ContainsPool(id)) return default(T);
        return pools[id].GetInstance<T>();
    }
}

