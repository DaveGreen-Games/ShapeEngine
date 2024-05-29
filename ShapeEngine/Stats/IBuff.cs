namespace ShapeEngine.Stats;

public interface IBuff
{
    public uint GetId();
    public void AddStacks(int amount);
    public bool RemoveStacks(int amount);
    public void ApplyTo(IStat stat);
    public void Update(float dt);
    public bool IsFinished();
    public IBuff Clone();
}