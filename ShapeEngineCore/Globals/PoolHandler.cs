

namespace ShapeEngineCore.Globals
{

    public interface IPoolable
    {
        public event Pool.OnPoolableInstanceFinished OnInstanceFinished;
        public void ReturnToPool();
        public void RemoveFromPool();
    }

    public class Pool
    {
        public delegate void OnPoolableInstanceFinished(IPoolable instance);


        public virtual void Clear() { }
        public virtual bool HasUsableInstances() { return false; }
        public virtual bool HasInstances() { return false; }

        public virtual IPoolable? GetInstance() { return null; }
        public virtual T? GetInstance<T>() { return default; }
        public virtual void ReturnInstance(IPoolable instance) { }

        protected virtual void OnInstanceFinished(IPoolable instance)
        {
            ReturnInstance(instance);
        }
    }

    public class PoolBasic : Pool
    {
        private int count = 0;
        private int maxSize = -1;
        private List<IPoolable> usable = new();
        private List<IPoolable> inUse = new();
        private Func<IPoolable> createInstance;

        public PoolBasic(int startSize, Func<IPoolable> createInstance, int maxSize = -1)
        {
            this.maxSize = maxSize;
            if (maxSize > 0) startSize = (int)MathF.Min(startSize, maxSize);
            this.count = startSize;
            this.createInstance = createInstance;
            for (int i = 0; i < startSize; i++)
            {
                var instance = this.createInstance();
                instance.OnInstanceFinished += OnInstanceFinished;
                usable.Add(instance);
            }
        }

        public override void Clear()
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

        public override bool HasUsableInstances() { return usable.Count > 0; }
        public override bool HasInstances() { return count > 0; }
        public override IPoolable GetInstance()
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
                else //reuse instance from inuse stack
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
        public override T GetInstance<T>()
        {
            return (T)GetInstance();
        }
        public override void ReturnInstance(IPoolable instance)
        {
            if (usable.Contains(instance)) return;
            if (!inUse.Contains(instance)) return;
            instance.ReturnToPool();
            usable.Add(instance);
        }


        

        private IPoolable Pop(List<IPoolable> list)
        {
            int index = list.Count - 1;
            var instance = list[index];
            list.RemoveAt(index);
            return instance;
        }
        private IPoolable PopBack(List<IPoolable> list)
        {
            int index = 0;
            var instance = list[index];
            list.RemoveAt(index);
            return instance;
        }
    }


    public static class PoolHandler
    {
        private static Dictionary<string, Pool> pools = new();

        public static bool ContainsPool(string poolName) { return pools.ContainsKey(poolName); }
        
        public static void AddPool(string name, Pool pool)
        {
            if (ContainsPool(name))
            {
                pools[name].Clear();
                pools[name] = pool;
            }
            else pools.Add(name, pool);
        }
        public static void AddPools(params (string name, Pool pool)[] pools)
        {
            foreach (var pool in pools)
            {
                AddPool(pool.name, pool.pool);
            }
        }
        
        public static void RemovePool(string name)
        {
            if (!ContainsPool(name)) return;
            pools[name].Clear();
            pools.Remove(name);
        }
        public static void RemovePools(params string[] names)
        {
            foreach (var name in names)
            {
                RemovePool(name);
            }
        }
        public static void RemoveAllPools()
        {
            ClearAllPools();
            pools.Clear();
        }

        public static Pool? GetPool(string name)
        {
            if (!ContainsPool(name)) return null;
            return pools[name];
        }
        public static T? GetPool<T>(string name) where T : Pool
        {
            if (!ContainsPool(name)) return null;
            var pool = pools[name];
            if (pool is T) return (T)pool;
            else return null;
        }
        
        public static void ClearPool(string name)
        {
            if (!ContainsPool(name)) return;
            pools[name].Clear();
        }
        public static void ClearAllPools()
        {
            foreach (var pool in pools.Values)
            {
                pool.Clear();
            }
        }
        
        public static T? GetInstance<T>(string poolName) where T : IPoolable
        {
            if (!ContainsPool(poolName)) return default(T);
            return pools[poolName].GetInstance<T>();
        }


    }


    /*
    public abstract class PoolGeneric<T>
    {
        protected Stack<T> pool;


        public PoolGeneric(int startSize)
        {
            pool = new Stack<T>();

            for (int i = 0; i < startSize; i++)
            {
                pool.Push(Activator.CreateInstance<T>());
            }
        }


        public int GetCount() { return pool.Count; }
        public virtual void Clear()
        {
            pool.Clear();
        }
        public T GetInstance()
        {
            if (pool.Count <= 0)
            {
                return Activator.CreateInstance<T>();
            }
            else
            {
                return pool.Pop();
            }
        }
        public virtual void ReturnInstance(T instance)
        {
            if (pool.Contains(instance)) { return; }
            pool.Push(instance);
        }

    }

    public class PoolIPoolable<T> : PoolGeneric<T> where T : IPoolable
    {
        public PoolIPoolable(int startSize) : base(startSize)
        {
        }
        public override void Clear()
        {
            foreach (var item in pool)
            {
                item.Despawn();
                item.Destroy();
            }
            pool.Clear();
        }
        public override void ReturnInstance(T instance)
        {
            if (pool.Contains(instance)) { return; }
            instance.Despawn();
            pool.Push(instance);
        }

    }
 
    */
}
