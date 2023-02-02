
namespace ShapePool
{
    public class PoolHandler
    {
        private Dictionary<string, IPool> pools = new();

        public bool ContainsPool(string poolName) { return pools.ContainsKey(poolName); }

        public void AddPool(string name, IPool pool)
        {
            if (ContainsPool(name))
            {
                pools[name].Clear();
                pools[name] = pool;
            }
            else pools.Add(name, pool);
        }
        public void AddPools(params (string name, IPool pool)[] pools)
        {
            foreach (var pool in pools)
            {
                AddPool(pool.name, pool.pool);
            }
        }

        public void RemovePool(string name)
        {
            if (!ContainsPool(name)) return;
            pools[name].Clear();
            pools.Remove(name);
        }
        public void RemovePools(params string[] names)
        {
            foreach (var name in names)
            {
                RemovePool(name);
            }
        }
        public void RemoveAllPools()
        {
            ClearAllPools();
            pools.Clear();
        }

        public IPool? GetPool(string name)
        {
            if (!ContainsPool(name)) return null;
            return pools[name];
        }
        public T? GetPool<T>(string name) where T : IPool
        {
            if (!ContainsPool(name)) return default(T);
            var pool = pools[name];
            if (pool is T) return (T)pool;
            else return default(T);
        }

        public void ClearPool(string name)
        {
            if (!ContainsPool(name)) return;
            pools[name].Clear();
        }
        public void ClearAllPools()
        {
            foreach (var pool in pools.Values)
            {
                pool.Clear();
            }
        }

        public T? GetInstance<T>(string poolName) where T : IPoolable
        {
            if (!ContainsPool(poolName)) return default(T);
            return pools[poolName].GetInstance<T>();
        }


    }
}
