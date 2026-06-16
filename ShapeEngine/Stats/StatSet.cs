namespace ShapeEngine.Stats;



//TODO: Recalculate() is Aggressive -> use dirty system somehow?
// With 100 stats and 50 sources, every AddSource() call does 100 stat recalculations even if only 2 stats are affected. Consider:
// - Dirty flag per stat (like SimpleStat)
// - Track which stats are affected by a source
// - Batch operations: BeginChanges() / EndChanges()
// Example:
// statSet.BeginUpdate();
// statSet.AddSource(buff1);
// statSet.AddSource(buff2);
// statSet.AddSource(buff3);
// statSet.EndUpdate(); // Recalculate once here


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
    #region Events
    
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
    /// Raised when a new stat is added to the set.
    /// </summary>
    public event Action<Stat>? OnStatAdded;
   
    /// <summary>
    /// Raised when a stat is removed from the set.
    /// </summary>
    public event Action<Stat>? OnStatRemoved;
    #endregion
    
    #region Public Collections
    
    /// <summary>
    /// The registered stats.
    /// </summary>
    public IReadOnlyCollection<Stat> Stats => stats.Values;

    /// <summary>
    /// The active modifier sources.
    /// </summary>
    public IReadOnlyCollection<IStatModifierSource> Sources => sources.Values;

    #endregion
    
    #region Private Collections
    
    private readonly Dictionary<StatId, Stat> stats = new(16);
    private readonly Dictionary<uint, IStatModifierSource> sources = new(24);
    private readonly List<uint> expiredSourceIds = new(8);
    private  List<StatModifier> statModifierBuffer = new(32);
    #endregion
    
    #region Public Functions
    
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
                //This avoids potential race issues if subscribers change between check and call.
                var handler = OnSourceRemoved;
                handler?.Invoke(source);
            }
        }

        Recalculate();
    }

    /// <summary>
    /// Recalculates all stats from the currently active sources.
    /// </summary>
    public void Recalculate()
    {
        CollectAllStatModifiers();
        
        foreach (var kvp in stats)
        {
            var stat = kvp.Value;
            var previous = stat.Value;
            var current = stat.Recalculate(statModifierBuffer);
            
            const float epsilon = 0.001f;
            if (Math.Abs(previous - current) > epsilon)
            {
                //This avoids potential race issues if subscribers change between check and call.
                var handler = OnStatChanged;
                handler?.Invoke(stat, previous, current);
            }
        }
    }

    /// <summary>
    /// Clears all stats and modifier sources from the set and recalculates values.
    /// </summary>
    /// <remarks>
    /// Raises <see cref="OnStatRemoved"/> for each removed stat and <see cref="OnSourceRemoved"/> for each removed source.
    /// </remarks>
    public void Clear()
    {
        bool changed = false;
        if (stats.Count > 0)
        {
            foreach (var kvp in stats)
            {
                var statId = kvp.Key;
                if (!stats.TryGetValue(statId, out var stat)) continue;
                if (!stats.Remove(statId)) continue;
                //This avoids potential race issues if subscribers change between check and call.
                var handler = OnStatRemoved;
                handler?.Invoke(stat);
                changed = true;
            }
        
            stats.Clear();
        }

        if (sources.Count > 0)
        {
            foreach (var kvp in sources)
            {
                var sourceId = kvp.Key;
                if (!sources.Remove(sourceId, out var source)) continue;
                //This avoids potential race issues if subscribers change between check and call.
                var handler = OnSourceRemoved;
                handler?.Invoke(source);
                changed = true;
            }
        
            sources.Clear();
        }
        
        if(changed) Recalculate();
    }
    
    #endregion
    
    #region Stat Functions
    
    /// <summary>
    /// Adds or replaces a stat.
    /// </summary>
    /// <param name="stat">The stat to register.</param>
    public void AddStat(Stat stat)
    {
        stats[stat.Id] = stat;
        //This avoids potential race issues if subscribers change between check and call.
        var handler = OnStatAdded;
        handler?.Invoke(stat);
        Recalculate();
    }

    /// <summary>
    /// Removes a stat by id.
    /// </summary>
    /// <param name="id">The stat id to remove.</param>
    /// <returns>True if a stat was removed.</returns>
    public bool RemoveStat(StatId id)
    {
        //StatSources can have multiple modifiers, each targeting a different stat.
        //So removing a stat should not remove all sources that target it.

        if (!stats.TryGetValue(id, out var stat)) return false;
        
        var removed = stats.Remove(id);
        if (removed)
        {
            //This avoids potential race issues if subscribers change between check and call.
            var handler = OnStatRemoved;
            handler?.Invoke(stat);
            Recalculate();
        }
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
    /// Attempts to get a stat value by id.
    /// </summary>
    /// <param name="id">The stat id.</param>
    /// <param name="value">The value of the found stat.</param>
    /// <returns>True if the stat exists.</returns>
    public bool TryGetValue(StatId id, out float value)
    {
        value = 0f;
        if (TryGetStat(id, out var stat))
        {
            value = stat.Value;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Removes all stats from the set and recalculates values.
    /// </summary>
    /// <remarks>
    /// Raises <see cref="OnStatRemoved"/> for each removed stat.
    /// </remarks>
    public void ClearStats()
    {
        if(stats.Count <= 0) return;
        
        bool changed = false;
        foreach (var kvp in stats)
        {
            var statId = kvp.Key;
            if (!stats.TryGetValue(statId, out var stat)) continue;
            if (!stats.Remove(statId)) continue;
            //This avoids potential race issues if subscribers change between check and call.
            var handler = OnStatRemoved;
            handler?.Invoke(stat);
            changed = true;
        }
        
        stats.Clear();
        
        if(changed) Recalculate();
    }
    
    #endregion

    #region Source Functions
    
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
            if (addedStacks > 0)
            {
                //This avoids potential race issues if subscribers change between check and call.
                var handler = OnSourceStacksAdded;
                handler?.Invoke(existing, addedStacks);
            }
            Recalculate();
            return false;
        }

        sources.Add(source.Id, source);
        //This avoids potential race issues if subscribers change between check and call.
        var handler2 = OnSourceAdded;
        handler2?.Invoke(source);
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

        //This avoids potential race issues if subscribers change between check and call.
        var handler = OnSourceRemoved;
        handler?.Invoke(source);
        
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
            //This avoids potential race issues if subscribers change between check and call.
            var handler = OnSourceStacksAdded;
            handler?.Invoke(source, added);
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

        //This avoids potential race issues if subscribers change between check and call.
        var handler = OnSourceStacksRemoved;
        handler?.Invoke(source, removed);
        
        if (source.IsExpired) RemoveSource(sourceId);
        else Recalculate();

        return removed;
    }

    /// <summary>
    /// Removes all modifier sources from the set and recalculates values.
    /// </summary>
    /// <remarks>
    /// Raises <see cref="OnSourceRemoved"/> for each removed source.
    /// </remarks>
    public void ClearSources()
    {
        if(sources.Count <= 0) return;
        
        var changed = false;
        foreach (var kvp in sources)
        {
            var sourceId = kvp.Key;
            if (!sources.Remove(sourceId, out var source)) continue;
            
            //This avoids potential race issues if subscribers change between check and call.
            var handler = OnSourceRemoved;
            handler?.Invoke(source);
            
            changed = true;
        }
        
        sources.Clear();
        
        if(changed) Recalculate();
    }
    
    #endregion
    
    #region Private Functions

    /// <summary>
    /// Collects all modifiers from all active sources into the stat modifier buffer.
    /// </summary>
    /// <returns>The total count of modifiers collected.</returns>
    private int CollectAllStatModifiers()
    {
        statModifierBuffer.Clear();
        foreach (var kvp in sources)
        {
            var source = kvp.Value;
            source.CollectAllModifiers(ref statModifierBuffer);
        }
        return statModifierBuffer.Count;
    }
    
    #endregion
}
