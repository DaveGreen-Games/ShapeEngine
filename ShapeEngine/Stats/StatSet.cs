namespace ShapeEngine.Stats;

/// <summary>
/// Manages stats and their active modifier sources. This is the new entry point of the stat system.
/// You create stats, add modifier sources, call Update(dt) each frame if you have timed effects, and read the final stat values back out.
/// <list type="bullet">
/// <item>Holds <see cref="Stat"/> objects and <see cref="IStatModifierSource"/> (like <see cref="StatModifierSource"/>, <see cref="TimedStatModifierSource"/>, <see cref="StackableStatModifierSource"/>)</item>
/// <item>Formula: ((Base + Flat) * (1 + AdditivePercent)) * MultiplicativeFactor</item>
/// </list>
/// </summary>
/// <remarks>
/// Main flow:
/// <list type="bullet">
/// <item>You define a stat with new Stat(...).</item>
/// <item>You register it in StatSet.AddStat(...).</item>
/// <item>You register one or more sources with StatSet.AddSource(...).</item>
/// <item>StatSet recalculates all stats from all active sources.</item>
/// <item>If you have timed sources, you call StatSet.Update(dt) each frame.</item>
/// <item>You read the result with StatSet.GetValue(statId) or stat.Value.</item>
/// </list>
///
/// What each class is for
/// <list type="bullet">
/// <item><see cref="StatId"/>: a lightweight wrapper around uint, used as the stable key for stats.</item>
/// <item><see cref="Stat"/>: one actual stat definition plus its current computed value, name, base value, optional min/max, and optional tags.</item>
/// <item><see cref="StatModifier"/>: one contribution to one stat, like “+5 flat” or “+20% multiplicative”.</item>
/// <item><see cref="StatModifierKind"/>: says how the modifier participates in the calculation pipeline.</item>
/// <item><see cref="StatModifierSource"/>: a permanent or manually removed source, like an item affix or passive.</item>
/// <item><see cref="TimedStatModifierSource"/>: a source with duration and expiry.</item>
/// <item><see cref="StackableStatModifierSource"/>: a timed source whose modifiers scale by stack count.</item>
/// <item><see cref="StatSet"/>: the manager that holds everything, updates timed sources, and recalculates values.</item>
/// </list>
///
/// How stacking works:
/// <list type="bullet">
/// <item>Sources have a stable Id.</item>
/// <item>If you add a source with an id that already exists, StatSet does not add a second copy.</item>
/// <item>Instead, it calls Reapply(...) on the existing source.</item>
/// <item>For timed sources, Reapply(...) refreshes duration.</item>
/// <item>For stackable sources, Reapply(...) adds stacks and refreshes duration by default.</item>
/// <item><see cref="AddStacks"/> and <see cref="RemoveStacks"/> let you manipulate an existing source directly.</item>
/// </list>
/// </remarks>
public class StatSet
{
    private readonly Dictionary<StatId, Stat> stats = new(16);
    private readonly Dictionary<uint, IStatModifierSource> sources = new(24);
    private readonly List<uint> expiredSourceIds = new(8);

    /// <summary>
    /// Raised when a stat value changes after recalculation.
    /// </summary>
    public event Action<Stat, float, float>? OnStatChanged;

    /// <summary>
    /// Raised when a modifier source is added.
    /// </summary>
    public event Action<IStatModifierSource>? OnSourceAdded;

    /// <summary>
    /// Raised when a modifier source is removed.
    /// </summary>
    public event Action<IStatModifierSource>? OnSourceRemoved;

    /// <summary>
    /// Raised when a source receives stacks.
    /// </summary>
    public event Action<IStatModifierSource, int>? OnSourceStacksAdded;

    /// <summary>
    /// Raised when stacks are removed from a source.
    /// </summary>
    public event Action<IStatModifierSource, int>? OnSourceStacksRemoved;

    /// <summary>
    /// The registered stats.
    /// </summary>
    public IReadOnlyCollection<Stat> Stats => stats.Values;

    /// <summary>
    /// The active modifier sources.
    /// </summary>
    public IReadOnlyCollection<IStatModifierSource> Sources => sources.Values;

    /// <summary>
    /// Adds or replaces a stat.
    /// </summary>
    /// <param name="stat">The stat to register.</param>
    public void AddStat(Stat stat)
    {
        stats[stat.Id] = stat;
        Recalculate();
    }

    /// <summary>
    /// Removes a stat by id.
    /// </summary>
    /// <param name="id">The stat id to remove.</param>
    /// <returns>True if a stat was removed.</returns>
    public bool RemoveStat(StatId id)
    {
        var removed = stats.Remove(id);
        if (removed) Recalculate();
        return removed;
    }

    /// <summary>
    /// Attempts to get a stat by id.
    /// </summary>
    /// <param name="id">The stat id.</param>
    /// <param name="stat">The found stat.</param>
    /// <returns>True if the stat exists.</returns>
    public bool TryGetStat(StatId id, out Stat stat) => stats.TryGetValue(id, out stat!);

    /// <summary>
    /// Gets a stat by id.
    /// </summary>
    /// <param name="id">The stat id.</param>
    /// <returns>The registered stat.</returns>
    public Stat GetStat(StatId id) => stats[id];

    /// <summary>
    /// Gets the current value of a stat by id.
    /// </summary>
    /// <param name="id">The stat id.</param>
    /// <returns>The current stat value.</returns>
    public float GetValue(StatId id) => stats[id].Value;

    /// <summary>
    /// Adds a modifier source. If a source with the same id already exists, it is reapplied instead.
    /// </summary>
    /// <param name="source">The source to add or reapply.</param>
    /// <returns>True if the source was newly added; false if an existing source was reapplied.</returns>
    public bool AddSource(IStatModifierSource source)
    {
        if (sources.TryGetValue(source.Id, out var existing))
        {
            var beforeStacks = existing.Stacks;
            existing.Reapply(source);
            var addedStacks = existing.Stacks - beforeStacks;
            if (addedStacks > 0) OnSourceStacksAdded?.Invoke(existing, addedStacks);
            Recalculate();
            return false;
        }

        sources.Add(source.Id, source);
        OnSourceAdded?.Invoke(source);
        Recalculate();
        return true;
    }

    /// <summary>
    /// Removes a modifier source by id.
    /// </summary>
    /// <param name="sourceId">The source id.</param>
    /// <returns>True if a source was removed.</returns>
    public bool RemoveSource(uint sourceId)
    {
        if (!sources.Remove(sourceId, out var source)) return false;

        OnSourceRemoved?.Invoke(source);
        Recalculate();
        return true;
    }

    /// <summary>
    /// Attempts to get a modifier source by id.
    /// </summary>
    /// <param name="sourceId">The source id.</param>
    /// <param name="source">The found source.</param>
    /// <returns>True if the source exists.</returns>
    public bool TryGetSource(uint sourceId, out IStatModifierSource source) => sources.TryGetValue(sourceId, out source!);

    /// <summary>
    /// Adds stacks to an existing source.
    /// </summary>
    /// <param name="sourceId">The source id.</param>
    /// <param name="amount">The number of stacks to add.</param>
    /// <returns>The number of stacks actually added.</returns>
    public int AddStacks(uint sourceId, int amount)
    {
        if (!sources.TryGetValue(sourceId, out var source)) return 0;

        var added = source.AddStacks(amount);
        if (added > 0)
        {
            OnSourceStacksAdded?.Invoke(source, added);
            Recalculate();
        }

        return added;
    }

    /// <summary>
    /// Removes stacks from an existing source.
    /// </summary>
    /// <param name="sourceId">The source id.</param>
    /// <param name="amount">The number of stacks to remove.</param>
    /// <returns>The number of stacks actually removed.</returns>
    public int RemoveStacks(uint sourceId, int amount)
    {
        if (!sources.TryGetValue(sourceId, out var source)) return 0;

        var removed = source.RemoveStacks(amount);
        if (removed <= 0) return 0;

        OnSourceStacksRemoved?.Invoke(source, removed);
        if (source.IsExpired) RemoveSource(sourceId);
        else Recalculate();

        return removed;
    }

    /// <summary>
    /// Advances all sources, removes expired sources, and recalculates stats.
    /// </summary>
    /// <param name="dt">The elapsed time in seconds.</param>
    public void Update(float dt)
    {
        expiredSourceIds.Clear();

        foreach (var kvp in sources)
        {
            var source = kvp.Value;
            source.Update(dt);
            if (source.IsExpired) expiredSourceIds.Add(kvp.Key);
        }

        foreach (var sourceId in expiredSourceIds)
        {
            if (sources.Remove(sourceId, out var source))
            {
                OnSourceRemoved?.Invoke(source);
            }
        }

        Recalculate();
    }

    /// <summary>
    /// Recalculates all stats from the currently active sources.
    /// </summary>
    public void Recalculate()
    {
        var modifiers = sources.Values.SelectMany(source => source.GetModifiers()).ToArray();

        foreach (var stat in stats.Values)
        {
            var previous = stat.Value;
            var current = stat.Recalculate(modifiers);
            if (!previous.Equals(current))
            {
                OnStatChanged?.Invoke(stat, previous, current);
            }
        }
    }
}
