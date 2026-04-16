namespace ShapeEngine.Stats;

/// <summary>
/// Manages a collection of stats and buffs, providing methods to update, add, and remove stats and buffs.
/// </summary>
/// <remarks>
/// <see cref="Stats"/> coordinates the application of all active buffs to all registered stats.
/// Stat values are recalculated during <see cref="Update(float)"/>, and buff-related events are raised when buffs are added, removed, or stacked.
/// </remarks>
public class Stats
{
    #region Events
    
    /// <summary>
    /// Event triggered when a buff stack is added.
    /// </summary>
    public event Action<IBuff, int>? OnBuffStackAdded;
   
    /// <summary>
    /// Event triggered when a buff stack is removed.
    /// </summary>
    public event Action<IBuff, int>? OnBuffStackRemoved;
    
    /// <summary>
    /// Event triggered when a buff is removed.
    /// </summary>
    public event Action<IBuff>? OnBuffRemoved;
    
    /// <summary>
    /// Event triggered when a buff is added.
    /// </summary>
    public event Action<IBuff>? OnBuffAdded;

    #endregion
    
    #region Private Fields
    
    private readonly Dictionary<uint, IStat> stats;
    private readonly Dictionary<uint, IBuff> buffs;
    private readonly List<uint> expiredBuffIds;

    #endregion
    
    #region Constructors
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Stats"/> class.
    /// </summary>
    public Stats()
    {
        stats = new(16);
        buffs = new(24);
        expiredBuffIds = new(8);
    }

    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Updates all buffs, recalculates all registered stats, and removes finished buffs.
    /// </summary>
    /// <param name="dt">The time delta since the last update.</param>
    public void Update(float dt)
    {
        foreach (var stat in stats.Values)
        {
            stat.Reset();
        }

        expiredBuffIds.Clear();
        foreach (var kvp in buffs)
        {
            var buff = kvp.Value;
            buff.Update(dt);
            if (buff.IsFinished())
            {
                expiredBuffIds.Add(kvp.Key);
            }
            else
            {
                foreach (var stat in stats.Values)
                {
                    buff.ApplyTo(stat);
                }
            }
        }

        foreach (var buffId in expiredBuffIds)
        {
            if (buffs.Remove(buffId, out var buff))
            {
                ResolveOnBuffRemoved(buff);
            }
        }
    }
    
    /// <summary>
    /// Adds a stat to the collection, replacing any existing stat with the same identifier.
    /// </summary>
    /// <param name="stat">The stat to add.</param>
    public void AddStat(Stat stat)
    {
        stats[stat.Id] = stat;
    }
    
    /// <summary>
    /// Removes a stat from the collection and resets it.
    /// </summary>
    /// <param name="stat">The stat to remove.</param>
    /// <returns>True if the stat was removed; otherwise, false.</returns>
    public bool RemoveStat(Stat stat)
    {
        var removed = stats.Remove(stat.Id);
        if(removed) stat.Reset();
        return removed;
    }
    
    /// <summary>
    /// Adds a stack to an existing buff, or adds the buff if it is not already tracked.
    /// </summary>
    /// <param name="buff">The buff to add or stack.</param>
    /// <returns>True if the buff was newly added; false if a stack was added to an existing buff.</returns>
    public bool AddBuffStack(IBuff buff)
    {
        var id = buff.GetId();
        if (buffs.TryGetValue(id, out var storedBuff))
        {
            storedBuff.AddStacks(1);
            ResolveOnBuffStackAdded(storedBuff, 1);
            return false;
        }

        buff.AddStacks(1);
        buffs.Add(id, buff);
        ResolveOnBuffAdded(buff);
        return true;
    }
    
    /// <summary>
    /// Removes a stack from a buff, or removes the buff entirely if no stacks remain.
    /// </summary>
    /// <param name="buff">The buff to remove a stack from.</param>
    /// <returns>True if the buff was removed; false if only a stack was removed or the buff was not found.</returns>
    public bool RemoveBuffStack(IBuff buff)
    {
        var id = buff.GetId();
        if (buffs.TryGetValue(id, out var storedBuff))
        {
            if (storedBuff.RemoveStacks(1))
            {
                buffs.Remove(id);
                ResolveOnBuffRemoved(storedBuff);
                return true;
            }
            
            ResolveOnBuffStackRemoved(storedBuff, 1);
        }

        return false;
    }
    
    /// <summary>
    /// Deletes a buff from the collection without triggering events or stack logic.
    /// </summary>
    /// <param name="buff">The buff to delete.</param>
    /// <returns>True if the buff was deleted; otherwise, false.</returns>
    public bool DeleteBuff(IBuff buff) => buffs.Remove(buff.GetId());
    
    #endregion
    
    #region Protected Methods
    
    /// <summary>
    /// Called when a buff is removed. Override to add custom logic.
    /// </summary>
    /// <param name="buff">The buff that was removed.</param>
    protected virtual void BuffWasRemoved(IBuff buff) { }
    
    /// <summary>
    /// Called when a buff is added. Override to add custom logic.
    /// </summary>
    /// <param name="buff">The buff that was added.</param>
    protected virtual void BuffWasAdded(IBuff buff) { }
    
    /// <summary>
    /// Called when a buff stack is added. Override to add custom logic.
    /// </summary>
    /// <param name="buff">The buff whose stack was added.</param>
    /// <param name="amount">The number of stacks added.</param>
    protected virtual void BuffStackWasAdded(IBuff buff, int amount) { }
    
    /// <summary>
    /// Called when a buff stack is removed. Override to add custom logic.
    /// </summary>
    /// <param name="buff">The buff whose stack was removed.</param>
    /// <param name="amount">The number of stacks removed.</param>
    protected virtual void BuffStackWasRemoved(IBuff buff, int amount) { }
    
    #endregion
    
    #region Private Methods
    
    private void ResolveOnBuffRemoved(IBuff buff)
    {
        BuffWasRemoved(buff);
        OnBuffRemoved?.Invoke(buff);
    }
    
    private void ResolveOnBuffAdded(IBuff buff)
    {
        BuffWasAdded(buff);
        OnBuffAdded?.Invoke(buff);
    }
    
    private void ResolveOnBuffStackAdded(IBuff buff, int amount)
    {
        BuffStackWasAdded(buff, amount);
        OnBuffStackAdded?.Invoke(buff, amount);
    }
    
    private void ResolveOnBuffStackRemoved(IBuff buff, int amount)
    {
        BuffStackWasRemoved(buff, amount);
        OnBuffStackRemoved?.Invoke(buff, amount);
    }
    
    #endregion
}