using System.Diagnostics;

namespace ShapeEngine.Stats;

/// <summary>
/// Manages stats and their active modifier sources. This is the new entry point of the stat system.
/// You create stats, add modifier sources, call Update(dt) each frame if you have timed effects, and read the final stat values back out.
/// <list type="bullet">
/// <item>Holds <see cref="Stat"/> objects and <see cref="IStatModifierSource"/> (like <see cref="StatModifierSource"/>, <see cref="TimedStatModifierSource"/>, <see cref="StackableStatModifierSource"/>)</item>
/// <item>Formula: ((Base + Flat) * (1 + AdditivePercent)) * MultiplicativeFactor</item>
/// </list>
/// </summary>
/// <summary>
///Use <see cref="StatSet"/> When:
/// <list type="bullet">
/// <item>RPGs/Complex Games: Many stats, many sources, complex interactions</item>
/// <item>Buff/Debuff systems: Timed effects, stacking, reapplication</item>
/// <item>Equipment systems: Items affecting multiple stats</item>
/// <item>UI-heavy: Tooltips showing all active effects</item>
/// <item>Modding support: Clean API for external content</item>
/// <item>Long-term project: Complexity pays off over time</item>
/// </list>
/// For a simpler alternative, see <see cref="SimpleStat"/>.
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
    private class BatchScope : IDisposable
    {
        private readonly StatSet statSet;
    
        public BatchScope(StatSet statSet)
        {
            this.statSet = statSet;
            statSet.BeginUpdate();
        }
    
        public void Dispose() => statSet.EndUpdate();
    }
    
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

    /// <summary>
    /// The stats that are marked "dirty" and need to be recalculated.
    /// </summary>
    public IReadOnlyCollection<StatId> GetDirtyStats() => dirtyStats;
    #endregion
    
    #region Private Collections
    
    private readonly Dictionary<StatId, Stat> stats = new(16);
    private readonly Dictionary<uint, IStatModifierSource> sources = new(24);
    private readonly List<uint> expiredSourceIds = new(8);
    private  List<StatModifier> statModifierBuffer = new(32);
    
    private readonly HashSet<StatId> dirtyStats = new(16);
    private int updateDepth = 0;
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
                
                // Mark stats affected by expired source as dirty
                MarkAffectedStatsDirty(source);
            }
        }

        // Recalculate dirty stats (from expired sources)
        // Note - Update() is never called during batching, so we don't check updateDepth
        if (dirtyStats.Count > 0)
        {
            RecalculateDirtyStats();
        }
    }

    /// <summary>
    /// Recalculates all stats from the currently active sources.
    /// </summary>
    public void Recalculate()
    {
        // Mark all stats dirty and recalculate
        MarkAllStatsDirty();
        RecalculateDirtyStats();
        
        // CollectAllStatModifiers();
        //
        // foreach (var kvp in stats)
        // {
        //     var stat = kvp.Value;
        //     var previous = stat.Value;
        //     var current = stat.Recalculate(statModifierBuffer);
        //     
        //     const float epsilon = 0.001f;
        //     if (Math.Abs(previous - current) > epsilon)
        //     {
        //         //This avoids potential race issues if subscribers change between check and call.
        //         var handler = OnStatChanged;
        //         handler?.Invoke(stat, previous, current);
        //     }
        // }
    }

    /// <summary>
    /// Clears all stats and modifier sources from the set and recalculates values.
    /// </summary>
    /// <remarks>
    /// Raises <see cref="OnStatRemoved"/> for each removed stat and <see cref="OnSourceRemoved"/> for each removed source.
    /// </remarks>
    public void Clear()
    {
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
            }
        
            sources.Clear();
        }
        
        // Clear dirty set
        dirtyStats.Clear();
    
        // No recalculation needed—everything is gone
    }
    
    /// <summary>
    /// Creates a disposable scope for batching multiple stat changes.
    /// When the scope is disposed, all pending recalculations are performed at once.
    /// Supports nesting: each Batch() call must be paired with Dispose().
    /// </summary>
    /// <returns>An IDisposable that ends the batch when disposed.</returns>
    /// <code>
    /// using (statSet.Batch())
    /// {
    ///     statSet.AddSource(buff1);
    ///     statSet.AddSource(buff2);
    /// } // Auto EndUpdate()
    /// </code>
    public IDisposable Batch()
    {
        return new BatchScope(this);
    }
    #endregion
    
    #region Batch Update API
    
    /// <summary>
    /// Begins a batch update. Multiple changes can be made without triggering recalculation until <see cref="EndUpdate"/> is called.
    /// </summary>
    /// <remarks>
    /// Supports nesting: each BeginUpdate must be paired with an EndUpdate.
    /// Recalculation happens only when the outermost EndUpdate is called.
    /// </remarks>
    public void BeginUpdate()
    {
        updateDepth++;
    }
    
    /// <summary>
    /// Ends a batch update. If this is the outermost EndUpdate, recalculates all dirty stats.
    /// </summary>
    public void EndUpdate()
    {
        if (updateDepth <= 0)
        {
            throw new InvalidOperationException("EndUpdate called without matching BeginUpdate.");
        }
        
        updateDepth--;
        
        if (updateDepth == 0)
        {
            RecalculateDirtyStats();
        }
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
        
        // New stat might be affected by existing sources
        dirtyStats.Add(stat.Id);
    
        if (updateDepth == 0)
        {
            RecalculateDirtyStats();
        }
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
            
            // Remove from dirty set if it was pending recalculation
            dirtyStats.Remove(id);
        
            // No recalculation needed—stat is gone
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
        
        foreach (var kvp in stats)
        {
            var statId = kvp.Key;
            if (!stats.TryGetValue(statId, out var stat)) continue;
            if (!stats.Remove(statId)) continue;
            //This avoids potential race issues if subscribers change between check and call.
            var handler = OnStatRemoved;
            handler?.Invoke(stat);
        }
        
        stats.Clear();
        
        // Clear dirty set since all stats are gone
        dirtyStats.Clear();
    
        // No recalculation needed—nothing to recalculate
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
            
            // Mark stats affected by this source as dirty
            MarkAffectedStatsDirty(existing);
            
            // Only recalculate if not batching
            if (updateDepth == 0)
            {
                RecalculateDirtyStats();
            }
            
            return false;
        }

        sources.Add(source.Id, source);
        
        //This avoids potential race issues if subscribers change between check and call.
        var handler2 = OnSourceAdded;
        handler2?.Invoke(source);
        
        // Mark stats affected by this source as dirty
        MarkAffectedStatsDirty(source);
    
        // Only recalculate if not batching
        if (updateDepth == 0)
        {
            RecalculateDirtyStats();
        }
        
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
        
        // Mark stats affected by this source as dirty
        MarkAffectedStatsDirty(source);
    
        // Only recalculate if not batching
        if (updateDepth == 0)
        {
            RecalculateDirtyStats();
        }
        
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
            
            // Mark stats affected by this source as dirty
            MarkAffectedStatsDirty(source);
    
            // Only recalculate if not batching
            if (updateDepth == 0)
            {
                RecalculateDirtyStats();
            }
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
        
        // Mark stats affected by this source as dirty
        MarkAffectedStatsDirty(source);
        
        if (source.IsExpired)
        {
            // This will recalculate if needed
            RemoveSource(sourceId);
        }
        else
        {
            // Only recalculate if not batching
            if (updateDepth == 0)
            {
                RecalculateDirtyStats();
            }
        }

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
        
        if (changed)
        {
            // Removing all sources affects all stats
            MarkAllStatsDirty();
        
            if (updateDepth == 0)
            {
                RecalculateDirtyStats();
            }
        }
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
    
    /// <summary>
    /// Recalculates only the stats that have been marked dirty since the last recalculation.
    /// </summary>
    /// <remarks>
    /// This is the core optimization: instead of recalculating all stats on every change,
    /// we only recalculate stats that are actually affected by source changes.
    /// </remarks>
    private void RecalculateDirtyStats()
    {
        // Early exit if nothing changed
        if (dirtyStats.Count == 0) return;
        
        // Collect all modifiers from all sources once
        // (This is still O(sources), but we only filter for dirty stats)
        CollectAllStatModifiers();
        
        // Recalculate only dirty stats
        foreach (var statId in dirtyStats)
        {
            if (!stats.TryGetValue(statId, out var stat))
            {
                // Stat was removed or doesn't exist—skip it
                continue;
            }
            
            var previous = stat.Value;
            var current = stat.Recalculate(statModifierBuffer);
            
            const float epsilon = 0.001f;
            if (Math.Abs(previous - current) > epsilon)
            {
                // This avoids potential race issues if subscribers change between check and call.
                var handler = OnStatChanged;
                handler?.Invoke(stat, previous, current);
            }
        }
        
        // Clear dirty set for next batch
        dirtyStats.Clear();
    }
    
    /// <summary>
    /// Marks stats affected by the given source as dirty.
    /// </summary>
    /// <param name="source">The source whose affected stats should be marked.</param>
    private void MarkAffectedStatsDirty(IStatModifierSource source)
    {
        statModifierBuffer.Clear();
        source.CollectAllModifiers(ref statModifierBuffer);
        // Get all modifiers from the source and mark their target stats as dirty
        foreach (var modifier in statModifierBuffer)
        {
            dirtyStats.Add(modifier.Target);
        }
    }
    
    /// <summary>
    /// Marks all registered stats as dirty.
    /// </summary>
    /// <remarks>
    /// Used when we can't determine which stats are affected (e.g., ClearSources, ClearStats).
    /// </remarks>
    private void MarkAllStatsDirty()
    {
        foreach (var kvp in stats)
        {
            var statId = kvp.Key;
            dirtyStats.Add(statId);
        }
    }
    #endregion
    
    #region Console Tests (Temporary - Remove in Production)
    
    /// <summary>
    /// Test 1: Verifies that only affected stats are recalculated when a source is added.
    /// </summary>
    private static void Test_DirtyTracking_OnlyRecalculatesAffectedStats()
    {
        Console.WriteLine("=== Test 1: Dirty Tracking - Only Recalculates Affected Stats ===");
        
        var statSet = new StatSet();
        
        var health = new Stat(new StatId(1), 100f, "Health");
        var mana = new Stat(new StatId(2), 50f, "Mana");
        var stamina = new Stat(new StatId(3), 75f, "Stamina");
        
        statSet.AddStat(health);
        statSet.AddStat(mana);
        statSet.AddStat(stamina);
        
        int healthChanges = 0;
        int manaChanges = 0;
        int staminaChanges = 0;
        
        statSet.OnStatChanged += (stat, prev, curr) =>
        {
            if (stat.Id.Value == 1) healthChanges++;
            if (stat.Id.Value == 2) manaChanges++;
            if (stat.Id.Value == 3) staminaChanges++;
        };
        
        // Add source that only affects health
        var healthBuff = new StatModifierSource(
            id: 100,
            sourceType: StatModifierSourceType.Buff,
            modifiers: new[] { StatModifier.Flat(new StatId(1), 20f) }
        );
        
        statSet.AddSource(healthBuff);
        
        // Verify results
        bool test1 = healthChanges == 1;
        bool test2 = manaChanges == 0;
        bool test3 = staminaChanges == 0;
        bool test4 = Math.Abs(health.Value - 120f) < 0.001f;
        bool test5 = Math.Abs(mana.Value - 50f) < 0.001f;
        bool test6 = Math.Abs(stamina.Value - 75f) < 0.001f;
        
        Console.WriteLine($"  Health changes: {healthChanges} (expected 1) - {(test1 ? "PASS" : "FAIL")}");
        Console.WriteLine($"  Mana changes: {manaChanges} (expected 0) - {(test2 ? "PASS" : "FAIL")}");
        Console.WriteLine($"  Stamina changes: {staminaChanges} (expected 0) - {(test3 ? "PASS" : "FAIL")}");
        Console.WriteLine($"  Health value: {health.Value} (expected 120) - {(test4 ? "PASS" : "FAIL")}");
        Console.WriteLine($"  Mana value: {mana.Value} (expected 50) - {(test5 ? "PASS" : "FAIL")}");
        Console.WriteLine($"  Stamina value: {stamina.Value} (expected 75) - {(test6 ? "PASS" : "FAIL")}");
        
        bool allPassed = test1 && test2 && test3 && test4 && test5 && test6;
        Console.WriteLine($"  Overall: {(allPassed ? "✓ PASSED" : "✗ FAILED")}");
        Console.WriteLine();
    }
    
    /// <summary>
    /// Test 2: Verifies that BeginUpdate/EndUpdate defers recalculation until EndUpdate is called.
    /// </summary>
    private static void Test_BatchUpdate_DefersRecalculation()
    {
        Console.WriteLine("=== Test 2: Batch Update - Defers Recalculation ===");
        
        var statSet = new StatSet();
        
        var attack = new Stat(new StatId(1), 50f, "Attack");
        statSet.AddStat(attack);
        
        int changeCount = 0;
        statSet.OnStatChanged += (stat, prev, curr) => changeCount++;
        
        statSet.BeginUpdate();
        
        statSet.AddSource(new StatModifierSource(100, StatModifierSourceType.Buff,
            modifiers: new[] { StatModifier.Flat(new StatId(1), 10f) }));
        
        statSet.AddSource(new StatModifierSource(101, StatModifierSourceType.Buff,
            modifiers: new[] { StatModifier.Flat(new StatId(1), 5f) }));
        
        statSet.AddSource(new StatModifierSource(102, StatModifierSourceType.Buff,
            modifiers: new[] { StatModifier.AdditivePercent(new StatId(1), 0.2f) }));
        
        bool test1 = changeCount == 0;
        Console.WriteLine($"  Changes during batch: {changeCount} (expected 0) - {(test1 ? "PASS" : "FAIL")}");
        
        statSet.EndUpdate();
        
        bool test2 = changeCount == 1;
        bool test3 = Math.Abs(attack.Value - 78f) < 0.001f; // (50 + 10 + 5) * 1.2 = 78
        
        Console.WriteLine($"  Changes after EndUpdate: {changeCount} (expected 1) - {(test2 ? "PASS" : "FAIL")}");
        Console.WriteLine($"  Final attack value: {attack.Value} (expected 78) - {(test3 ? "PASS" : "FAIL")}");
        
        bool allPassed = test1 && test2 && test3;
        Console.WriteLine($"  Overall: {(allPassed ? "✓ PASSED" : "✗ FAILED")}");
        Console.WriteLine();
    }
    
    /// <summary>
    /// Test 3: Verifies that nested BeginUpdate/EndUpdate calls only recalculate on the outermost EndUpdate.
    /// </summary>
    private static void Test_NestedBatching_OnlyRecalculatesOnOutermostEnd()
    {
        Console.WriteLine("=== Test 3: Nested Batching - Only Recalculates On Outermost End ===");
        
        var statSet = new StatSet();
        var stat = new Stat(new StatId(1), 100f);
        statSet.AddStat(stat);
        
        int changeCount = 0;
        statSet.OnStatChanged += (s, p, c) => changeCount++;
        
        statSet.BeginUpdate(); // Depth = 1
        
        statSet.AddSource(new StatModifierSource(1, StatModifierSourceType.Buff,
            modifiers: new[] { StatModifier.Flat(new StatId(1), 10f) }));
        
        bool test1 = changeCount == 0;
        Console.WriteLine($"  Changes after first AddSource: {changeCount} (expected 0) - {(test1 ? "PASS" : "FAIL")}");
        
        statSet.BeginUpdate(); // Depth = 2
        
        statSet.AddSource(new StatModifierSource(2, StatModifierSourceType.Buff,
            modifiers: new[] { StatModifier.Flat(new StatId(1), 5f) }));
        
        bool test2 = changeCount == 0;
        Console.WriteLine($"  Changes after second AddSource: {changeCount} (expected 0) - {(test2 ? "PASS" : "FAIL")}");
        
        statSet.EndUpdate(); // Depth = 1, no recalc
        
        bool test3 = changeCount == 0;
        Console.WriteLine($"  Changes after inner EndUpdate: {changeCount} (expected 0) - {(test3 ? "PASS" : "FAIL")}");
        
        statSet.EndUpdate(); // Depth = 0, recalc!
        
        bool test4 = changeCount == 1;
        bool test5 = Math.Abs(stat.Value - 115f) < 0.001f;
        
        Console.WriteLine($"  Changes after outer EndUpdate: {changeCount} (expected 1) - {(test4 ? "PASS" : "FAIL")}");
        Console.WriteLine($"  Final value: {stat.Value} (expected 115) - {(test5 ? "PASS" : "FAIL")}");
        
        bool allPassed = test1 && test2 && test3 && test4 && test5;
        Console.WriteLine($"  Overall: {(allPassed ? "✓ PASSED" : "✗ FAILED")}");
        Console.WriteLine();
    }
    
    /// <summary>
    /// Test 4: Verifies that expired sources only recalculate affected stats.
    /// </summary>
    private static void Test_ExpiredSources_OnlyRecalculatesAffectedStats()
    {
        Console.WriteLine("=== Test 4: Expired Sources - Only Recalculates Affected Stats ===");
        
        var statSet = new StatSet();
        
        var health = new Stat(new StatId(1), 100f);
        var mana = new Stat(new StatId(2), 50f);
        
        statSet.AddStat(health);
        statSet.AddStat(mana);
        
        statSet.AddSource(new TimedStatModifierSource(
            id: 100,
            sourceType: StatModifierSourceType.Buff,
            duration: 5f,
            modifiers: new[] { StatModifier.Flat(new StatId(1), 20f) }
        ));
        
        bool test1 = Math.Abs(health.Value - 120f) < 0.001f;
        Console.WriteLine($"  Health after adding buff: {health.Value} (expected 120) - {(test1 ? "PASS" : "FAIL")}");
        
        int healthChanges = 0;
        int manaChanges = 0;
        
        statSet.OnStatChanged += (stat, prev, curr) =>
        {
            if (stat.Id.Value == 1) healthChanges++;
            if (stat.Id.Value == 2) manaChanges++;
        };
        
        // Expire the buff
        statSet.Update(6f);
        
        bool test2 = healthChanges == 1;
        bool test3 = manaChanges == 0;
        bool test4 = Math.Abs(health.Value - 100f) < 0.001f;
        bool test5 = Math.Abs(mana.Value - 50f) < 0.001f;
        
        Console.WriteLine($"  Health changes: {healthChanges} (expected 1) - {(test2 ? "PASS" : "FAIL")}");
        Console.WriteLine($"  Mana changes: {manaChanges} (expected 0) - {(test3 ? "PASS" : "FAIL")}");
        Console.WriteLine($"  Health after expiry: {health.Value} (expected 100) - {(test4 ? "PASS" : "FAIL")}");
        Console.WriteLine($"  Mana after expiry: {mana.Value} (expected 50) - {(test5 ? "PASS" : "FAIL")}");
        
        bool allPassed = test1 && test2 && test3 && test4 && test5;
        Console.WriteLine($"  Overall: {(allPassed ? "✓ PASSED" : "✗ FAILED")}");
        Console.WriteLine();
    }
    
    /// <summary>
    /// Test 5: Verifies that manual Recalculate() updates all stats.
    /// </summary>
    private static void Test_ManualRecalculate_RecalculatesAllStats()
    {
        Console.WriteLine("=== Test 5: Manual Recalculate - Recalculates All Stats ===");
        
        var statSet = new StatSet();
        
        var stat = new Stat(new StatId(1), 100f);
        statSet.AddStat(stat);
        
        bool test1 = Math.Abs(stat.Value - 100f) < 0.001f;
        Console.WriteLine($"  Initial value: {stat.Value} (expected 100) - {(test1 ? "PASS" : "FAIL")}");
        
        // Manually change base value (bypasses dirty tracking)
        stat.BaseValue = 200f;
        
        bool test2 = Math.Abs(stat.Value - 100f) < 0.001f;
        Console.WriteLine($"  Value after changing BaseValue: {stat.Value} (expected 100 - not updated yet) - {(test2 ? "PASS" : "FAIL")}");
        
        // Manual recalculate
        statSet.Recalculate();
        
        bool test3 = Math.Abs(stat.Value - 200f) < 0.001f;
        Console.WriteLine($"  Value after Recalculate(): {stat.Value} (expected 200) - {(test3 ? "PASS" : "FAIL")}");
        
        bool allPassed = test1 && test2 && test3;
        Console.WriteLine($"  Overall: {(allPassed ? "✓ PASSED" : "✗ FAILED")}");
        Console.WriteLine();
    }
    
    /// <summary>
    /// Performance comparison: Old approach (recalc all) vs New approach (dirty tracking).
    /// </summary>
    private static void Test_Performance_Comparison()
    {
        Console.WriteLine("=== Performance Test: Dirty Tracking Optimization ===");
    
        var statSet = new StatSet();
    
        // Create 100 stats
        for (uint i = 1; i <= 100; i++)
        {
            statSet.AddStat(new Stat(new StatId(i), 100f, $"Stat{i}"));
        }
    
        int totalRecalculations = 0;
        statSet.OnStatChanged += (stat, prev, curr) => totalRecalculations++;
    
        // Add 10 sources, each affecting only 3 stats
        var sw = System.Diagnostics.Stopwatch.StartNew();
    
        for (uint sourceId = 1; sourceId <= 10; sourceId++)
        {
            var modifiers = new[]
            {
                StatModifier.Flat(new StatId(sourceId), 10f),
                StatModifier.Flat(new StatId(sourceId + 1), 5f),
                StatModifier.Flat(new StatId(sourceId + 2), 3f)
            };
        
            statSet.AddSource(new StatModifierSource(
                id: sourceId,
                sourceType: StatModifierSourceType.Buff,
                modifiers: modifiers
            ));
        }
    
        sw.Stop();
    
        Console.WriteLine($"  100 stats, 10 sources (3 stats each)");
        Console.WriteLine($"  Total recalculations: {totalRecalculations} (expected ~30)");
        Console.WriteLine($"  Time elapsed: {sw.Elapsed.TotalMilliseconds:F2}ms");
        Console.WriteLine($"  Without optimization would recalculate: 1000 times (100 stats × 10 operations)");
        Console.WriteLine($"  Optimization factor: ~{1000.0 / totalRecalculations:F1}x faster");
    
        bool efficient = totalRecalculations < 100; // Should be around 30, definitely less than 100
        Console.WriteLine($"  Overall: {(efficient ? "✓ OPTIMIZED" : "✗ NOT OPTIMIZED")}");
        Console.WriteLine();
    }
    
    /// <summary>
    /// Runs all console tests.
    /// </summary>
    private static void RunAllTests()
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║         StatSet Optimization Tests (Console Mode)          ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        
        Test_DirtyTracking_OnlyRecalculatesAffectedStats();
        Test_BatchUpdate_DefersRecalculation();
        Test_NestedBatching_OnlyRecalculatesOnOutermostEnd();
        Test_ExpiredSources_OnlyRecalculatesAffectedStats();
        Test_ManualRecalculate_RecalculatesAllStats();
        Test_Performance_Comparison();
        
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                      Tests Complete                        ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
    }
    
    #endregion
}

