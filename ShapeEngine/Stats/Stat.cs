using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.RectDef;

namespace ShapeEngine.Stats;

/// <summary>
/// Represents a stat that can be affected by buffs, supporting base value, current value, and locking.
/// </summary>
/// <remarks>
/// Implements the <see cref="IStat"/> interface and provides methods for applying and resetting buffs.
/// </remarks>
public class Stat : IStat
{
    private readonly BitFlag mask;
    /// <summary>
    /// The unique identifier for this stat.
    /// </summary>
    public uint Id { get; private set; }
    /// <summary>
    /// The display name of the stat.
    /// </summary>
    public string Name = "";
    /// <summary>
    /// The abbreviated display name of the stat.
    /// </summary>
    public string NameAbbreviation = "";
    /// <summary>
    /// The base value of the stat before buffs are applied.
    /// </summary>
    public float BaseValue { get; set; }
    /// <summary>
    /// The current value of the stat after all buffs are applied.
    /// </summary>
    public float CurValue => total.ApplyTo(BaseValue);
    /// <summary>
    /// Gets or sets whether the stat is locked (prevents further buff application).
    /// </summary>
    /// <remarks>Setting Locked to true will reset the stat.</remarks>
    public bool Locked
    {
        get => locked;
        set
        {
            if (value && !locked) Reset();
            locked = value;
        }
    }
    
    private bool locked;
    private BuffValue total = new();
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Stat"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the stat.</param>
    /// <param name="baseValue">The base value of the stat.</param>
    /// <param name="tagMask">The tag mask for determining which buffs affect this stat.</param>
    public Stat(uint id, float baseValue, BitFlag tagMask)
    {
        Id = id;
        BaseValue = baseValue;
        mask = tagMask;
    }
    
    /// <summary>
    /// Draws the stat in the specified rectangle.
    /// Default implementation does nothing.
    /// </summary>
    /// <param name="rect">The rectangle to draw in.</param>
    public virtual void Draw(Rect rect) { }

    /// <summary>
    /// Returns a string representation of the stat, including its name and current value.
    /// </summary>
    /// <returns>A string describing the stat.</returns>
    public new string ToString()
    {
        float bonusPercent = (1 + total.Bonus) * 100;
        return $"{Name}: {CurValue} [+{(int)bonusPercent}% +{(int)total.Flat}]";
    }
    /// <summary>
    /// Returns a string representation of the stat, optionally abbreviated.
    /// </summary>
    /// <param name="abbreviated">Whether to use the abbreviated name.</param>
    /// <returns>A string describing the stat.</returns>
    public virtual string ToText(bool abbreviated)
    {
        float bonusPercent = (1 + total.Bonus) * 100;
        return $"{(abbreviated ? NameAbbreviation : Name)}: {CurValue} [+{(int)bonusPercent}% +{(int)total.Flat}]";
    }
    
  
    /// <inheritdoc cref="IStat.IsAffected(uint)"/>
    public bool IsAffected(uint tag) => mask.Has(tag);
    /// <inheritdoc cref="IStat.Reset()"/>
    public void Reset()
    {
        total = new();
    }
    /// <summary>
    /// Applies a buff value to this stat if the stat is not locked.
    /// </summary>
    /// <param name="buffValue">The buff value to apply.</param>
    public void Apply(BuffValue buffValue)
    {
        if (Locked) return;
        total = total.Add(buffValue);
    }
}