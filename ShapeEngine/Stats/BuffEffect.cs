namespace ShapeEngine.Stats;

/// <summary>
/// Represents a single effect that can be applied by a buff,
/// including a tag, bonus multiplier, flat value, and tag name.
/// </summary>
public readonly struct BuffEffect
{
    /// <summary>
    /// The tag used to identify which stats this effect applies to.
    /// </summary>
    public readonly uint Tag;
    /// <summary>
    /// The percentage bonus (as a multiplier, e.g., 0.1 for +10%).
    /// </summary>
    public readonly float Bonus;
    /// <summary>
    /// The flat value to add to the stat.
    /// </summary>
    public readonly float Flat;
    /// <summary>
    /// The name of the tag for display purposes.
    /// </summary>
    public readonly string TagName;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="BuffEffect"/> struct.
    /// </summary>
    /// <param name="tag">The tag for stat matching.</param>
    /// <param name="bonus">The percentage bonus (as a multiplier).</param>
    /// <param name="flat">The flat value to add.</param>
    /// <param name="tagName">The display name for the tag.</param>
    public BuffEffect(uint tag, float bonus = 0f, float flat = 0f, string tagName = "")
    {
        Tag = tag;
        Bonus = bonus;
        Flat = flat;
        TagName = tagName;
    }
    
    /// <summary>
    /// Returns a string representation of the effect for UI or debugging.
    /// </summary>
    /// <returns>A string describing the effect.</returns>
    public string ToText()
    {
        float bonusPercentage = (1 + Bonus) * 100;
        return $"{TagName} +{(int)bonusPercentage}% +{(int)Flat}";
    }
}