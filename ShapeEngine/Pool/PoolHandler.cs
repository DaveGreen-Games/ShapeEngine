
namespace ShapeEngine.Pool
{
    public class PoolHandler
    {
        private Dictionary<uint, IPool> pools = new();

        public bool ContainsPool(uint poolId) { return pools.ContainsKey(poolId); }

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
        public void AddPools(params IPool[] pools)
        {
            foreach (var pool in pools)
            {
                AddPool(pool);
            }
        }

        public void RemovePool(uint id)
        {
            if (!ContainsPool(id)) return;
            pools[id].Clear();
            pools.Remove(id);
        }
        public void RemovePools(params uint[] ids)
        {
            foreach (var id in ids)
            {
                RemovePool(id);
            }
        }
        public void RemoveAllPools()
        {
            ClearAllPools();
            pools.Clear();
        }

        public IPool? GetPool(uint id)
        {
            if (!ContainsPool(id)) return null;
            return pools[id];
        }
        public T? GetPool<T>(uint id) where T : IPool
        {
            if (!ContainsPool(id)) return default(T);
            var pool = pools[id];
            if (pool is T) return (T)pool;
            else return default(T);
        }

        public void ClearPool(uint id)
        {
            if (!ContainsPool(id)) return;
            pools[id].Clear();
        }
        public void ClearAllPools()
        {
            foreach (var pool in pools.Values)
            {
                pool.Clear();
            }
        }

        public T? GetInstance<T>(uint id) where T : IPoolable
        {
            if (!ContainsPool(id)) return default(T);
            return pools[id].GetInstance<T>();
        }


    }
}
