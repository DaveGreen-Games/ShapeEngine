using System.Numerics;

namespace ShapeEngineCore.Globals
{

    public interface IPoolable
    {
        public void Despawn();
        public void Destroy();
    }

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


    public static class Pooling
    {
        //create global accessible pools here
    }
}
