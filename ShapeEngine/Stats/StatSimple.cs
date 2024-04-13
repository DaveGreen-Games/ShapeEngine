namespace ShapeEngine.Stats;

public class StatSimple
{
    public uint Id { get; private set; }
    public float BaseValue { get; set; }
    public float CurValue => total.ApplyTo(BaseValue);
    
    private BuffValue total = new();
    
    public StatSimple(uint id, float baseValue)
    {
        Id = id;
        BaseValue = baseValue;
    }

    public void AddBuff(BuffValue value)
    {
        total = total.Add(value);
    }

    public void RemoveBuff(BuffValue value)
    {
        total = total.Remove(value);
    }

    public void Reset() => total = new();

}