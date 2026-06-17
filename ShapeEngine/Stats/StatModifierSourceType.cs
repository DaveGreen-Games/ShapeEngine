namespace ShapeEngine.Stats;

/// <summary>
/// Describes the gameplay category of a modifier source.
/// </summary>
public enum StatModifierSourceType
{
    /// <summary>
    /// A generic source with no more specific category.
    /// </summary>
    Custom = 0,

    /// <summary>
    /// A beneficial active effect.
    /// </summary>
    Buff = 1,

    /// <summary>
    /// A harmful active effect.
    /// </summary>
    Debuff = 2,

    /// <summary>
    /// A source granted by equipped items or loot affixes.
    /// </summary>
    Equipment = 3,

    /// <summary>
    /// A passive character, item, or account effect.
    /// </summary>
    Passive = 4,

    /// <summary>
    /// A source granted by an upgrade, level, perk, or incremental purchase.
    /// </summary>
    Upgrade = 5
}
