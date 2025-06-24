namespace ShapeEngine.Stats;

/// <summary>
/// Manages a collection of stats and buffs, providing methods to update, add, and remove stats and buffs.
/// </summary>
/// <remarks>
/// The <see cref="Stats"/> class coordinates the application and removal of buffs to stats,
/// and raises events for buff changes.
/// </remarks>
public class Stats
{
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

    private readonly Dictionary<uint, IStat> stats;
    private readonly Dictionary<uint, IBuff> buffs;

    /// <summary>
    /// Initializes a new instance of the <see cref="Stats"/> class.
    /// </summary>
    public Stats()
    {
        stats = new(16);
        buffs = new(24);
    }

    /// <summary>
    /// Updates all buffs and stats, applying and removing buffs as needed.
    /// </summary>
    /// <param name="dt">The time delta since the last update.</param>
    public void Update(float dt)
    {
        var statValues = this.stats.Values;
        foreach (var stat in statValues)
        {
            stat.Reset();
        }
        foreach (var kvp in buffs)
        {
            var buff = kvp.Value;
            buff.Update(dt);
            if (buff.IsFinished())
            {
                buffs.Remove(kvp.Key);
                ResolveOnBuffRemoved(buff);
            }
            else
            {
                foreach (var stat in statValues)
                {
                    buff.ApplyTo(stat);
                }
            }
        }
    }
    /// <summary>
    /// Adds a stat to the collection, replacing any existing stat with the same ID.
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
    /// Adds a stack to a buff, or adds the buff if it does not exist.
    /// </summary>
    /// <param name="buff">The buff to add or stack.</param>
    /// <returns>True if the buff was newly added; false if a stack was added to an existing buff.</returns>
    public bool AddBuffStack(IBuff buff)
    {
        if (buffs.ContainsKey(buff.GetId()))
        {
            buffs[buff.GetId()].AddStacks(1);
            ResolveOnBuffStackAdded(buff, 1);
            return false;
        }
        else
        {
            buffs.Add(buff.GetId(), buff);
            ResolveOnBuffAdded(buff);
            return true;
        }
    }
    /// <summary>
    /// Removes a stack from a buff, or removes the buff if no stacks remain.
    /// </summary>
    /// <param name="buff">The buff to remove a stack from.</param>
    /// <returns>True if the buff was removed; false if only a stack was removed or the buff was not found.</returns>
    public bool RemoveBuffStack(IBuff buff)
    {
        if (buffs.ContainsKey(buff.GetId()))
        {
            if (buffs[buff.GetId()].RemoveStacks(1))
            {
                buffs.Remove(buff.GetId());
                ResolveOnBuffRemoved(buff);
                return true;
            }
            
            ResolveOnBuffStackRemoved(buff, 1);
        }

        return false;
    }
    /// <summary>
    /// Deletes a buff from the collection without triggering events or stack logic.
    /// </summary>
    /// <param name="buff">The buff to delete.</param>
    /// <returns>True if the buff was deleted; otherwise, false.</returns>
    public bool DeleteBuff(IBuff buff) => buffs.Remove(buff.GetId());
    
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
}