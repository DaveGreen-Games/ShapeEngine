namespace ShapeEngine.Stats;

/// <summary>
/// Manages stats and their active modifier sources.
/// </summary>
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
