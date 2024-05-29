using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Stats;

public class Stat : IStat
{
    private readonly BitFlag mask;
    public uint Id { get; private set; }
    public string Name = "";
    public string NameAbbreviation = "";
    public float BaseValue { get; set; }
    public float CurValue => total.ApplyTo(BaseValue);
    public bool Locked
    {
        get => locked;
        set
        {
            if (value && !locked) Reset();
            locked = value;
        }
    }
    
    private bool locked = false;
    private BuffValue total = new();
    
    public Stat(uint id, float baseValue, BitFlag tagMask)
    {
        Id = id;
        BaseValue = baseValue;
        mask = tagMask;
    }
    
    
    public virtual void Draw(Rect rect) { }

    public new string ToString()
    {
        float bonusPercent = (1 + total.Bonus) * 100;
        return $"{Name}: {CurValue} [+{(int)bonusPercent}% +{(int)total.Flat}]";
    }
    public virtual string ToText(bool abbreviated)
    {
        float bonusPercent = (1 + total.Bonus) * 100;
        return $"{(abbreviated ? NameAbbreviation : Name)}: {CurValue} [+{(int)bonusPercent}% +{(int)total.Flat}]";
    }
    
    
    public bool IsAffected(uint tag) => mask.Has(tag);
    public void Reset()
    {
        total = new();
    }
    public void Apply(BuffValue buffValue)
    {
        if (Locked) return;
        total = total.Add(buffValue);
    }
}