namespace ShapeEngine.Stats;

/// <summary>
/// Represents a value for a buff, including a percentage bonus and a flat value.
/// </summary>
public readonly struct BuffValue
{
    /// <summary>
    /// The percentage bonus (as a multiplier, e.g., 0.1 for +10%).
    /// </summary>
    public readonly float Bonus;
    /// <summary>
    /// The flat value to add to the stat.
    /// </summary>
    public readonly float Flat;
    /// <summary>
    /// Initializes a new instance of the <see cref="BuffValue"/> struct with zero values.
    /// </summary>
    public BuffValue()
    {
        this.Bonus = 0f;
        this.Flat = 0f;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="BuffValue"/> struct with specified bonus and flat values.
    /// </summary>
    /// <param name="bonus">The percentage bonus (as a multiplier).</param>
    /// <param name="flat">The flat value to add.</param>
    public BuffValue(float bonus, float flat)
    {
        this.Bonus = bonus;
        this.Flat = flat;
    }
    /// <summary>
    /// Applies this buff value to a base value.
    /// </summary>
    /// <code>=> (baseValue + Flat) * (1f + Bonus);</code>
    /// <param name="baseValue">The base value to apply the buff to.</param>
    /// <returns>The resulting value after applying the buff.</returns>
    public float ApplyTo(float baseValue) => (baseValue + Flat) * (1f + Bonus);
    /// <summary>
    /// Adds another <see cref="BuffValue"/> to this one.
    /// </summary>
    /// <code>=> new(Bonus + other.Bonus, Flat + other.Flat);</code>
    /// <param name="other">The other buff value to add.</param>
    /// <returns>The sum of the two buff values.</returns>
    public BuffValue Add(BuffValue other) => new(Bonus + other.Bonus, Flat + other.Flat);
    /// <summary>
    /// Removes another <see cref="BuffValue"/> from this one.
    /// </summary>
    /// <code>=> new(Bonus - other.Bonus, Flat - other.Flat);</code>
    /// <param name="other">The other buff value to remove.</param>
    /// <returns>The result of the subtraction.</returns>
    public BuffValue Remove(BuffValue other) => new(Bonus - other.Bonus, Flat - other.Flat);
    /// <summary>
    /// Returns a string representation of the buff value for UI or debugging.
    /// </summary>
    /// <returns>A string describing the buff value.</returns>
    public string ToText()
    {
        float bonusPercentage = (1 + Bonus) * 100;
        return $"+{(int)bonusPercentage}% +{(int)Flat}";
    }
}