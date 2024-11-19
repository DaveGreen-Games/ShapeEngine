
namespace ShapeEngine.Stats;



public class Stats
{
    public event Action<IBuff, int>? OnBuffStackAdded;
    public event Action<IBuff, int>? OnBuffStackRemoved;
    public event Action<IBuff>? OnBuffRemoved;
    public event Action<IBuff>? OnBuffAdded;

    private readonly Dictionary<uint, IStat> stats;
    private readonly Dictionary<uint, IBuff> buffs;

    public Stats()
    {
        stats = new(16);
        buffs = new(24);
    }

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
    public void AddStat(Stat stat)
    {
        stats[stat.Id] = stat;
    }
    public bool RemoveStat(Stat stat)
    {
        var removed = stats.Remove(stat.Id);
        if(removed) stat.Reset();
        return removed;
    }
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
    public bool DeleteBuff(IBuff buff) => buffs.Remove(buff.GetId());
    
    protected virtual void BuffWasRemoved(IBuff buff) { }
    protected virtual void BuffWasAdded(IBuff buff) { }
    protected virtual void BuffStackWasAdded(IBuff buff, int amount) { }
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