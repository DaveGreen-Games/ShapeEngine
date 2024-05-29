namespace ShapeEngine.Stats;

public interface IStat
{
    public void Reset();
    public bool IsAffected(uint tag);
    public void Apply(BuffValue buffValue);
}