namespace ShapeEngine.Stats;

public readonly struct BuffValue
{
    public readonly float Bonus;
    public readonly float Flat;
    public BuffValue()
    {
        this.Bonus = 0f;
        this.Flat = 0f;
    }
    public BuffValue(float bonus, float flat)
    {
        this.Bonus = bonus;
        this.Flat = flat;
    }

    public float ApplyTo(float baseValue) => (baseValue + Flat) * (1f + Bonus);

    public BuffValue Add(BuffValue other) => new(Bonus + other.Bonus, Flat + other.Flat);
    public BuffValue Remove(BuffValue other) => new(Bonus - other.Bonus, Flat - other.Flat);
    // public Value Add(Effect other) => new(Bonus + other.Bonus, Flat + other.Flat);
        
    public string ToText()
    {
        float bonusPercentage = (1 + Bonus) * 100;
        return $"+{(int)bonusPercentage}% +{(int)Flat}";
    }
}