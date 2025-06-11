namespace ShapeEngine.Stats;


/// <summary>
/// Represents a simple stat with a base value and a current value that reflects all applied buffs.
/// </summary>
/// <example>
/// <code>
/// var id = 1;
/// var baseValue = 10f;
/// var stat = new StatSimple(id, baseValue);
/// var flatValue = 4f;
/// var bonusValue = 1f; (+100%)
/// stat.AddBuff(new BuffValue(bonusValue, flatValue));
/// float current = stat.CurValue; // 28 -> (10 + flatValue) * (1 + bonusValue)
/// </code>
/// </example>
/// <remarks>
/// Add or remove buff values to modify the total buff. <see cref="CurValue"/> always reflects the base value with all buffs applied.
/// </remarks>
public class StatSimple
{
    /// <summary>
    /// The unique identifier for this stat.
    /// </summary>
    public uint Id { get; private set; }
    /// <summary>
    /// The base value of the stat before buffs are applied.
    /// </summary>
    public float BaseValue { get; set; }
    /// <summary>
    /// The current value of the stat after all buffs are applied.
    /// </summary>
    public float CurValue => total.ApplyTo(BaseValue);
    
    private BuffValue total = new();
    
    /// <summary>
    /// Initializes a new instance of the <see cref="StatSimple"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the stat.</param>
    /// <param name="baseValue">The base value of the stat.</param>
    public StatSimple(uint id, float baseValue)
    {
        Id = id;
        BaseValue = baseValue;
    }

    /// <summary>
    /// Adds a buff value to this stat.
    /// </summary>
    /// <param name="value">The buff value to add.</param>
    public void AddBuff(BuffValue value)
    {
        total = total.Add(value);
    }

    /// <summary>
    /// Removes a buff value from this stat.
    /// </summary>
    /// <param name="value">The buff value to remove.</param>
    public void RemoveBuff(BuffValue value)
    {
        total = total.Remove(value);
    }

    /// <summary>
    /// Resets the stat to its base value, removing all applied buffs.
    /// </summary>
    public void Reset() => total = new();

}