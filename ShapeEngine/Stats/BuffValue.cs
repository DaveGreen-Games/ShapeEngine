namespace ShapeEngine.Stats;

/// <summary>
/// Represents a buff contribution consisting of a percentage bonus and a flat value.
/// </summary>
public readonly struct BuffValue
{
    #region Public Properties
    
    /// <summary>
    /// The percentage bonus (as a multiplier, e.g., 0.1 for +10%).
    /// </summary>
    public readonly float Bonus;

    /// <summary>
    /// The flat value to add to the stat.
    /// </summary>
    public readonly float Flat;
    
    #endregion
    
    #region Constructors
    
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
    
    #endregion
    
    #region Public Methods
    
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
    /// <returns>A string describing the buff value using signed percentage and flat values.</returns>
    public string ToText() => FormatText(Bonus, Flat);

    #endregion

    #region Private Static Methods
    
    internal static string FormatText(float bonus, float flat)
    {
        return $"{FormatSignedPercent(bonus * 100f)} {FormatSignedNumber(flat)}";
    }

    private static string FormatSignedPercent(float value)
    {
        return $"{(value >= 0f ? "+" : "")}{value:0.##}%";
    }

    private static string FormatSignedNumber(float value)
    {
        return $"{(value >= 0f ? "+" : "")}{value:0.##}";
    }
    
    #endregion
}