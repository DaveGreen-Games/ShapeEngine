

using ShapeEngine.StaticLib;

namespace ShapeEngine.Pool
{
    public interface IPoolable
    {
        public event Action<IPoolable>? OnInstanceFinished;
        public void ReturnToPool();
        public void RemoveFromPool();
    }
    public interface IPool
    {
        public uint GetId();
        public void Clear();
        public bool HasUsableInstances(); 
        public bool HasInstances();

        public IPoolable? GetInstance();
        public T? GetInstance<T>();
        public void ReturnInstance(IPoolable instance);

        public void OnInstanceFinished(IPoolable instance);
    }

    public class Pool : IPool
    {
        private static IdCounter IdCounter = new();
        
        private int count = 0;
        private int maxSize = -1;
        private readonly List<IPoolable> usable = new();
        private readonly List<IPoolable> inUse = new();
        private readonly Func<IPoolable> createInstance;

        private uint id;

        public Pool(int startSize, Func<IPoolable> createInstance, int maxSize = -1)
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

            this.id = IdCounter.NextId;
        }

        public uint GetId() => id;

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

        public bool HasUsableInstances() { return usable.Count > 0; }
        public bool HasInstances() { return count > 0; }
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
        public T GetInstance<T>()
        {
            return (T)GetInstance();
        }
        public void ReturnInstance(IPoolable instance)
        {
            if (usable.Contains(instance)) return;
            if (!inUse.Contains(instance)) return;
            instance.ReturnToPool();
            usable.Add(instance);
        }

        public void OnInstanceFinished(IPoolable instance)
        {
            ReturnInstance(instance);
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

}
