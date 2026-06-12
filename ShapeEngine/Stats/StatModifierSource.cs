namespace ShapeEngine.Stats;

/// <summary>
/// Represents a permanent or manually removed source of stat modifiers.
/// </summary>
public class StatModifierSource : IStatModifierSource
{
    private readonly List<StatModifier> modifiers;

    /// <inheritdoc />
    public uint Id { get; }

    /// <inheritdoc />
    public StatModifierSourceType SourceType { get; }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string Description { get; }

    /// <inheritdoc />
    public virtual int Stacks { get; protected set; } = 1;

    /// <inheritdoc />
    public virtual int MaxStacks => 1;

    /// <inheritdoc />
    public virtual float Duration => 0f;

    /// <inheritdoc />
    public virtual float RemainingDuration { get; protected set; }

    /// <inheritdoc />
    public virtual bool IsExpired => false;

    /// <summary>
    /// The unscaled modifiers configured for this source.
    /// </summary>
    public IReadOnlyList<StatModifier> Modifiers => modifiers;

    /// <summary>
    /// Creates a new modifier source.
    /// </summary>
    /// <param name="id">The stable source id.</param>
    /// <param name="sourceType">The gameplay category of the source.</param>
    /// <param name="modifiers">The modifiers contributed by the source.</param>
    /// <param name="name">The display name.</param>
    /// <param name="description">The display description.</param>
    public StatModifierSource(
        uint id,
        StatModifierSourceType sourceType,
        IEnumerable<StatModifier> modifiers,
        string name = "",
        string description = "")
    {
        Id = id;
        SourceType = sourceType;
        Name = name;
        Description = description;
        this.modifiers = new List<StatModifier>();

        foreach (var modifier in modifiers)
        {
            this.modifiers.Add(modifier.SourceId == 0 ? modifier with { SourceId = id } : modifier);
        }
    }

    /// <summary>
    /// Creates a new modifier source.
    /// </summary>
    /// <param name="id">The stable source id.</param>
    /// <param name="sourceType">The gameplay category of the source.</param>
    /// <param name="name">The display name.</param>
    /// <param name="description">The display description.</param>
    /// <param name="modifiers">The modifiers contributed by the source.</param>
    public StatModifierSource(
        uint id,
        StatModifierSourceType sourceType,
        string name = "",
        string description = "",
        params StatModifier[] modifiers) : this(id, sourceType, modifiers.AsEnumerable(), name, description)
    {
    }

    /// <inheritdoc />
    public virtual IEnumerable<StatModifier> GetModifiers() => modifiers;

    /// <inheritdoc />
    public virtual void Update(float dt) { }

    /// <inheritdoc />
    public virtual void Reapply(IStatModifierSource incoming) { }

    /// <inheritdoc />
    public virtual int AddStacks(int amount) => 0;

    /// <inheritdoc />
    public virtual int RemoveStacks(int amount) => 0;
}
