namespace ShapeEngine.Stats;

public readonly struct BuffEffect
{
    public readonly uint Tag;
    public readonly float Bonus;
    public readonly float Flat;
    public readonly string TagName;
        
    public BuffEffect(uint tag, float bonus = 0f, float flat = 0f, string tagName = "")
    {
        Tag = tag;
        Bonus = bonus;
        Flat = flat;
        TagName = tagName;
    }
    
    public string ToText()
    {
        float bonusPercentage = (1 + Bonus) * 100;
        return $"{TagName} +{(int)bonusPercentage}% +{(int)Flat}";
    }
    
}