namespace ShapeEngine.Stats;

/// <summary>
/// Represents a timed source whose modifiers scale by stack count.
/// </summary>
public class StackableStatModifierSource : TimedStatModifierSource
{
    /// <inheritdoc />
    public override int Stacks { get; protected set; }

    /// <inheritdoc />
    public override int MaxStacks { get; }

    /// <summary>
    /// If true, adding or reapplying stacks refreshes the shared duration.
    /// </summary>
    public bool RefreshDurationOnStack { get; }

    /// <summary>
    /// Creates a new stackable modifier source.
    /// </summary>
    /// <param name="id">The stable source id.</param>
    /// <param name="sourceType">The gameplay category of the source.</param>
    /// <param name="duration">The duration in seconds.</param>
    /// <param name="maxStacks">The maximum stack count, or a value below zero for unlimited stacks.</param>
    /// <param name="modifiers">The per-stack modifiers contributed by the source.</param>
    /// <param name="initialStacks">The initial stack count.</param>
    /// <param name="refreshDurationOnStack">If true, stack additions refresh the shared duration.</param>
    /// <param name="name">The display name.</param>
    /// <param name="description">The display description.</param>
    public StackableStatModifierSource(
        uint id,
        StatModifierSourceType sourceType,
        float duration,
        int maxStacks,
        IEnumerable<StatModifier> modifiers,
        int initialStacks = 1,
        bool refreshDurationOnStack = true,
        string name = "",
        string description = "") : base(id, sourceType, duration, modifiers, name, description)
    {
        MaxStacks = maxStacks;
        RefreshDurationOnStack = refreshDurationOnStack;
        Stacks = ClampStacks(initialStacks);
        if (Stacks <= 0) RemainingDuration = 0f;
    }

    /// <summary>
    /// Creates a new stackable modifier source.
    /// </summary>
    /// <param name="id">The stable source id.</param>
    /// <param name="sourceType">The gameplay category of the source.</param>
    /// <param name="duration">The duration in seconds.</param>
    /// <param name="maxStacks">The maximum stack count, or a value below zero for unlimited stacks.</param>
    /// <param name="initialStacks">The initial stack count.</param>
    /// <param name="refreshDurationOnStack">If true, stack additions refresh the shared duration.</param>
    /// <param name="name">The display name.</param>
    /// <param name="description">The display description.</param>
    /// <param name="modifiers">The per-stack modifiers contributed by the source.</param>
    public StackableStatModifierSource(
        uint id,
        StatModifierSourceType sourceType,
        float duration,
        int maxStacks,
        int initialStacks = 1,
        bool refreshDurationOnStack = true,
        string name = "",
        string description = "",
        params StatModifier[] modifiers) : this(id, sourceType, duration, maxStacks, modifiers.AsEnumerable(), initialStacks, refreshDurationOnStack, name, description)
    {
    }

    /// <inheritdoc />
    public override bool IsExpired => Stacks <= 0 || base.IsExpired;

    /// <inheritdoc />
    public override IEnumerable<StatModifier> GetModifiers()
    {
        if (Stacks <= 0) yield break;

        foreach (var modifier in Modifiers)
        {
            yield return modifier with { Amount = modifier.Amount * Stacks };
        }
    }

    /// <inheritdoc />
    public override void Reapply(IStatModifierSource incoming)
    {
        AddStacks(Math.Max(1, incoming.Stacks));
    }

    /// <inheritdoc />
    public override int AddStacks(int amount)
    {
        if (amount <= 0) return 0;

        var before = Stacks;
        Stacks = ClampStacks(Stacks + amount);

        if (RefreshDurationOnStack && Duration > 0f)
        {
            RemainingDuration = Duration;
        }

        return Stacks - before;
    }

    /// <inheritdoc />
    public override int RemoveStacks(int amount)
    {
        if (amount <= 0 || Stacks <= 0) return 0;

        var before = Stacks;
        Stacks -= amount;
        if (Stacks < 0) Stacks = 0;
        if (Stacks <= 0) RemainingDuration = 0f;

        return before - Stacks;
    }

    private int ClampStacks(int value)
    {
        if (value < 0) return 0;
        if (MaxStacks >= 0 && value > MaxStacks) return MaxStacks;
        return value;
    }
}
