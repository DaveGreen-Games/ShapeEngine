using Clipper2Lib;

namespace ShapeEngine.ShapeClipper;

public sealed class Paths64PooledBuffer
{
    private Stack<Path64> path64Pool;

    public Paths64 Buffer = new();
        
    public Paths64PooledBuffer(int poolCapacity = 64)
    {
        path64Pool = new Stack<Path64>(poolCapacity);
    }

    public void PrepareBuffer(int targetCount)
    {
        if (Buffer.Count > targetCount)
        {
            for (int i = Buffer.Count - 1; i >= targetCount; i--)
            {
                var path = Buffer[i];
                Buffer.RemoveAt(i);
                ReturnPath64(path);
            }
        }
        else if (Buffer.Count < targetCount)
        {
            var diff = targetCount - Buffer.Count;
            for (int i = 0; i < diff; i++)
            {
                Buffer.Add(RentPath64());
            }
        }
    }
    public void ClearBuffer()
    {
        foreach (var path in Buffer)
        {
            ReturnPath64(path);
        }
        Buffer.Clear();
    }
        
    private Path64 RentPath64()
    {
        if (path64Pool.Count > 0) return path64Pool.Pop();
        return new Path64();
    }
    private void ReturnPath64(Path64 path64)
    {
        path64Pool.Push(path64);
    }
}