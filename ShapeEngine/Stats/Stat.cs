namespace ShapeEngine.Stats;

/// <summary>
/// Represents one calculated stat.
/// </summary>
public sealed class Stat
{
    private readonly HashSet<uint> tags;

    /// <summary>
    /// The stable id of this stat.
    /// </summary>
    public StatId Id { get; }

    /// <summary>
    /// The display name of this stat.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The abbreviated display name of this stat.
    /// </summary>
    public string NameAbbreviation { get; set; }

    /// <summary>
    /// Optional display or documentation text for this stat.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The base value before modifiers are applied.
    /// </summary>
    public float BaseValue { get; set; }

    /// <summary>
    /// Optional lower clamp from the stat definition.
    /// </summary>
    public float? MinValue { get; set; }

    /// <summary>
    /// Optional upper clamp from the stat definition.
    /// </summary>
    public float? MaxValue { get; set; }

    /// <summary>
    /// The last calculated value.
    /// </summary>
    public float Value { get; private set; }

    /// <summary>
    /// The last calculated value.
    /// </summary>
    public float CurrentValue => Value;

    /// <summary>
    /// The tags assigned to this stat for grouping and filtering.
    /// </summary>
    public IReadOnlyCollection<uint> Tags => tags;

    /// <summary>
    /// Creates a new stat.
    /// </summary>
    /// <param name="id">The stable stat id.</param>
    /// <param name="baseValue">The base value.</param>
    /// <param name="name">The display name.</param>
    /// <param name="nameAbbreviation">The abbreviated display name.</param>
    /// <param name="description">Optional display or documentation text.</param>
    /// <param name="minValue">Optional lower clamp.</param>
    /// <param name="maxValue">Optional upper clamp.</param>
    /// <param name="tags">Optional grouping tags.</param>
    public Stat(
        StatId id,
        float baseValue,
        string name = "",
        string nameAbbreviation = "",
        string description = "",
        float? minValue = null,
        float? maxValue = null,
        params uint[] tags)
    {
        Id = id;
        BaseValue = baseValue;
        Name = name;
        NameAbbreviation = nameAbbreviation;
        Description = description;
        MinValue = minValue;
        MaxValue = maxValue;
        Value = baseValue;
        this.tags = tags.Length > 0 ? new HashSet<uint>(tags) : new HashSet<uint>();
    }

    /// <summary>
    /// Adds a grouping tag to this stat.
    /// </summary>
    /// <param name="tag">The tag to add.</param>
    /// <returns>True if the tag was added.</returns>
    public bool AddTag(uint tag) => tags.Add(tag);

    /// <summary>
    /// Removes a grouping tag from this stat.
    /// </summary>
    /// <param name="tag">The tag to remove.</param>
    /// <returns>True if the tag was removed.</returns>
    public bool RemoveTag(uint tag) => tags.Remove(tag);

    /// <summary>
    /// Checks whether this stat has a grouping tag.
    /// </summary>
    /// <param name="tag">The tag to check.</param>
    /// <returns>True if the stat has the tag.</returns>
    public bool HasTag(uint tag) => tags.Contains(tag);

    /// <summary>
    /// Recalculates the stat using the supplied modifiers.
    /// </summary>
    /// <param name="modifiers">The modifiers targeting this stat.</param>
    /// <returns>The new calculated value.</returns>
    public float Recalculate(IEnumerable<StatModifier> modifiers)
    {
        var flat = 0f;
        var additivePercent = 0f;
        var multiplicativeFactor = 1f;
        float? overrideValue = null;
        var overridePriority = int.MinValue;
        var min = MinValue;
        var max = MaxValue;

        foreach (var modifier in modifiers)
        {
            if (modifier.Target != Id) continue;

            switch (modifier.Kind)
            {
                case StatModifierKind.Flat:
                    flat += modifier.Amount;
                    break;
                case StatModifierKind.AdditivePercent:
                    additivePercent += modifier.Amount;
                    break;
                case StatModifierKind.MultiplicativePercent:
                    multiplicativeFactor *= 1f + modifier.Amount;
                    break;
                case StatModifierKind.Override:
                    if (overrideValue == null || modifier.Priority >= overridePriority)
                    {
                        overrideValue = modifier.Amount;
                        overridePriority = modifier.Priority;
                    }
                    break;
                case StatModifierKind.Min:
                    min = min == null ? modifier.Amount : MathF.Max(min.Value, modifier.Amount);
                    break;
                case StatModifierKind.Max:
                    max = max == null ? modifier.Amount : MathF.Min(max.Value, modifier.Amount);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(modifiers), $"Unknown stat modifier kind {modifier.Kind}.");
            }
        }

        var value = (BaseValue + flat) * (1f + additivePercent) * multiplicativeFactor;
        if (overrideValue != null) value = overrideValue.Value;
        if (min != null && value < min.Value) value = min.Value;
        if (max != null && value > max.Value) value = max.Value;

        Value = value;
        return Value;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var label = string.IsNullOrWhiteSpace(Name) ? Id.ToString() : Name;
        return $"{label}: {Value:0.##}";
    }
}
