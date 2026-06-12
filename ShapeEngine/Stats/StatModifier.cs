namespace ShapeEngine.Stats;

/// <summary>
/// Represents one contribution to one stat.
/// </summary>
/// <param name="Target">The stat this modifier affects.</param>
/// <param name="Kind">How the amount is applied.</param>
/// <param name="Amount">The numeric modifier amount.</param>
/// <param name="SourceId">Optional id of the source that produced this modifier.</param>
/// <param name="Priority">Optional ordering value for advanced resolution, especially overrides.</param>
/// <param name="Name">Optional display name.</param>
/// <param name="Description">Optional display description.</param>
public readonly record struct StatModifier(
    StatId Target,
    StatModifierKind Kind,
    float Amount,
    uint SourceId = 0,
    int Priority = 0,
    string Name = "",
    string Description = "")
{
    /// <summary>
    /// Creates a flat modifier.
    /// </summary>
    public static StatModifier Flat(StatId target, float amount, uint sourceId = 0, int priority = 0, string name = "", string description = "") =>
        new(target, StatModifierKind.Flat, amount, sourceId, priority, name, description);

    /// <summary>
    /// Creates an additive percentage modifier. A value of <c>0.25</c> means +25%.
    /// </summary>
    public static StatModifier AdditivePercent(StatId target, float amount, uint sourceId = 0, int priority = 0, string name = "", string description = "") =>
        new(target, StatModifierKind.AdditivePercent, amount, sourceId, priority, name, description);

    /// <summary>
    /// Creates a multiplicative percentage modifier. A value of <c>0.25</c> means x1.25.
    /// </summary>
    public static StatModifier MultiplicativePercent(StatId target, float amount, uint sourceId = 0, int priority = 0, string name = "", string description = "") =>
        new(target, StatModifierKind.MultiplicativePercent, amount, sourceId, priority, name, description);

    /// <summary>
    /// Creates an override modifier.
    /// </summary>
    public static StatModifier Override(StatId target, float amount, uint sourceId = 0, int priority = 0, string name = "", string description = "") =>
        new(target, StatModifierKind.Override, amount, sourceId, priority, name, description);

    /// <summary>
    /// Creates a minimum clamp modifier.
    /// </summary>
    public static StatModifier Min(StatId target, float amount, uint sourceId = 0, int priority = 0, string name = "", string description = "") =>
        new(target, StatModifierKind.Min, amount, sourceId, priority, name, description);

    /// <summary>
    /// Creates a maximum clamp modifier.
    /// </summary>
    public static StatModifier Max(StatId target, float amount, uint sourceId = 0, int priority = 0, string name = "", string description = "") =>
        new(target, StatModifierKind.Max, amount, sourceId, priority, name, description);
}
