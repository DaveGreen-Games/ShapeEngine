namespace ShapeEngine.Pool;

//TODO: Remove
public interface IPoolable
{
    public event Action<IPoolable>? OnInstanceFinished; 
    public void Reset();
}