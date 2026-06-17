namespace ShapeEngine.Stats;

/// <summary>
/// Defines how a stat modifier participates in the stat calculation pipeline.
/// </summary>
public enum StatModifierKind
{
    /// <summary>
    /// Adds directly to the base value before percentage modifiers are applied.
    /// </summary>
    Flat = 0,

    /// <summary>
    /// Adds to the shared additive percentage bucket. A value of <c>0.25</c> means +25%.
    /// </summary>
    AdditivePercent = 1,

    /// <summary>
    /// Multiplies the value after flat and additive percentage modifiers. A value of <c>0.25</c> means x1.25.
    /// </summary>
    MultiplicativePercent = 2,

    /// <summary>
    /// Replaces the calculated value. When multiple overrides are present, the highest priority wins.
    /// </summary>
    Override = 3,

    /// <summary>
    /// Contributes a lower clamp. The highest contributed minimum is used.
    /// </summary>
    Min = 4,

    /// <summary>
    /// Contributes an upper clamp. The lowest contributed maximum is used.
    /// </summary>
    Max = 5
}
