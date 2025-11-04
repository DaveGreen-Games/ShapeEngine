using System.Collections.Concurrent;

namespace ShapeEngine.Pool;

//TODO: Remove
public sealed class ObjectPoolPoolable<T> where T : IPoolable
{
    private readonly ConcurrentBag<T> objects;
    private readonly Func<T> objectGenerator;

    public ObjectPoolPoolable(Func<T> objectGenerator)
    {
        this.objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
        objects = [];
    }

    public T Get()
    {
        if (objects.TryTake(out var instance))
        {
            return instance;
        }
        var newInstance = objectGenerator();
        newInstance.OnInstanceFinished += Return;
        return newInstance;
    }

    public void Return(T instance)
    {
        ArgumentNullException.ThrowIfNull(instance);
        //make sure to not have multiple subscriptions
        instance.OnInstanceFinished -= Return;
        instance.OnInstanceFinished += Return;
        objects.Add(instance);
        
    }

    private void Return(IPoolable instance)
    {
        instance.Reset();
        Return((T)instance);
    }
}