
namespace ShapeEngine.Stats;

//TESTING--------
// public static class BuffTags
// {
//     public static readonly uint MovementSpeed = BitFlag.NextFlag;
//     public static readonly uint ReloadSpeed = BitFlag.NextFlag;
// }
// public static class BuffEffects
// {
//     public static readonly BuffCreator.Buff.Effect MovementSpeedFreezeEffect = new (BuffTags.MovementSpeed, -0.1f, 0f, "Movement Speed");
//
//     public static readonly BuffCreator.Buff.Effect ReloadSpeedFreezeEffect = new (BuffTags.ReloadSpeed, 0f, 0.25f, "Reload Speed");
// }
// public static class Buffs
// {
//     public static readonly BuffCreator FreezeNerf = new(0, 5, 2, true, false, false, BuffEffects.MovementSpeedFreezeEffect, BuffEffects.ReloadSpeedFreezeEffect);
// }
//
// public class Test
// {
//     
//     public ShapeStatSystem playerStatSystem = new ShapeStatSystem();
//
//     public void OnPlayerHit()
//     {
//         playerStatSystem.AddBuff(Buffs.FreezeNerf.Clone());
//     }
// }



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



// internal class StatValue
    // {
    //     public float value = 0f;
    //     float duration = 0f;
    //     float timer = 0f;
    //     public StatValue(float value, float duration = -1f)
    //     {
    //         this.value = value;
    //         Start(duration);
    //     }
    //     public float GetRemaining() => timer;
    //
    //     public float GetDuration() => duration;
    //
    //     //public float Get() { return value; }
    //     public void Add(float amount) { value += amount; }
    //     public void Start(float duration) { this.duration = duration; timer = duration; }
    //     public void Restart() { if (duration > 0f) timer = duration; }
    //     public bool Update(float dt)
    //     {
    //         if (timer == 0f) return true;
    //         else if (timer > 0f)
    //         {
    //             timer -= dt;
    //             if (timer <= 0f) { timer = 0f; return true; }
    //         }
    //
    //         return false;
    //     }
    // }
    // public class StatSimple
    // {
    //     private float baseValue = 0f;
    //     private float flatTotal = 0f;
    //     private float bonusTotal = 1f;
    //     private List<StatValue> timedBonuses = new();
    //     private List<StatValue> timedFlats = new();
    //
    //     public string ID { get; set; } = "";
    //
    //     public event Action<StatSimple>? Changed;
    //     
    //     public StatSimple(float baseValue) { this.baseValue = baseValue; }
    //     public StatSimple(float baseValue, string id) { this.baseValue = baseValue; this.ID = id;}
    //     public void SetBase(float value) 
    //     {
    //         float prev = baseValue;
    //         baseValue = value;
    //         if (prev != baseValue) Changed?.Invoke(this);
    //     }
    //     public float Base => baseValue;
    //
    //     public float Cur => (baseValue + flatTotal) * bonusTotal;
    //     public float BonusTotal => bonusTotal;
    //     public float FlatTotal => flatTotal;
    //
    //     public bool Update(float dt)
    //     {
    //         bool changed = false;
    //         if (timedBonuses.Count > 0)
    //         {
    //             for (int i = timedBonuses.Count - 1; i >= 0; i--)
    //             {
    //                 if (timedBonuses[i].Update(dt))
    //                 {
    //                     bonusTotal -= timedBonuses[i].value;
    //                     timedBonuses.RemoveAt(i);
    //                     if (!changed) changed = true;
    //                 }
    //             }
    //         }
    //
    //         if (timedFlats.Count > 0)
    //         {
    //             for (int i = timedFlats.Count - 1; i >= 0; i--)
    //             {
    //                 if (timedFlats[i].Update(dt))
    //                 {
    //                     flatTotal -= timedFlats[i].value;
    //                     timedFlats.RemoveAt(i);
    //                     if (!changed) changed = true;
    //                 }
    //             }
    //         }
    //
    //         if (changed) Changed?.Invoke(this);
    //
    //         return changed;
    //     }
    //
    //     public float GetF()
    //     {
    //         if (Base == 0f) return 0f;
    //         return Cur / baseValue;
    //     }
    //     public void Set(StatSimple other)
    //     {
    //         baseValue = other.baseValue;
    //         SetBonuses(other);
    //     }
    //     public void SetBonuses(StatSimple other)
    //     {
    //         flatTotal += other.flatTotal;
    //         bonusTotal += other.bonusTotal - 1f;
    //
    //         foreach (var bonus in other.timedBonuses)
    //         {
    //             AddBonus(bonus.value, bonus.GetRemaining());
    //         }
    //         foreach (var flat in other.timedFlats)
    //         {
    //             AddFlat(flat.value, flat.GetRemaining());
    //         }
    //
    //         Changed?.Invoke(this);
    //     }
    //
    //     public void AddBonuses(params float[] bonuses)
    //     {
    //         foreach (var bonus in bonuses)
    //         {
    //             bonusTotal += bonus;
    //         }
    //
    //         Changed?.Invoke(this);
    //     }
    //     public void AddFlats(params float[] flats)
    //     {
    //         foreach (var flat in flats)
    //         {
    //             flatTotal += flat;
    //         }
    //         Changed?.Invoke(this);
    //     }
    //     public void AddBonus(float bonus, float duration = -1f)
    //     {
    //         bonusTotal += bonus;
    //         if (duration > 0f)
    //         {
    //             timedBonuses.Add(new(bonus, duration));
    //         }
    //         Changed?.Invoke(this);
    //     }
    //     public void AddFlat(float flat, float duration = -1f)
    //     {
    //         flatTotal += flat;
    //         if (duration > 0f)
    //         {
    //             timedFlats.Add(new(flat, duration));
    //         }
    //         Changed?.Invoke(this);
    //     }
    //     public void RemoveBonus(float bonus)
    //     {
    //         bonusTotal -= bonus;
    //         Changed?.Invoke(this);
    //     }
    //     public void RemoveFlat(float flat)
    //     {
    //         flatTotal -= flat;
    //         Changed?.Invoke(this);
    //     }
    //     public void ResetBonus() { bonusTotal = 1f; timedBonuses.Clear(); Changed?.Invoke(this); }
    //     public void ResetFlat() { flatTotal = 0f; timedFlats.Clear(); Changed?.Invoke(this); }
    //     public void Reset() 
    //     {
    //         bonusTotal = 1f; 
    //         timedBonuses.Clear();
    //         
    //         flatTotal = 0f; 
    //         timedFlats.Clear(); 
    //         
    //         Changed?.Invoke(this);
    //     }
    // }
    
 // public interface IBuff
    // {
    //     public int GetID();
    //     public bool IsEmpty();
    //     public bool DrawToUI();
    //     public (float totalBonus, float totalFlat) Get(params int[] tags);
    //     public void AddStack();
    //     public bool RemoveStack();
    //     public void Update(float dt);
    //     public void DrawUI(Rect r, ColorRgba barColorRgba, ColorRgba bgColorRgba, ColorRgba textColorRgba);
    // }
    // public struct BuffValue
    // {
    //     public float bonus = 0f;
    //     public float flat = 0f;
    //     public int id = -1;
    //     public BuffValue(int id)
    //     {
    //         this.bonus = 0f;
    //         this.flat = 0f;
    //         this.id = id;
    //     }
    //     public BuffValue(int id, float bonus, float flat)
    //     {
    //         this.id = id;
    //         this.bonus = bonus;
    //         this.flat = flat;
    //     }
    // }
    //
    // public class Buff : IBuff
    // {
    //     protected Dictionary<int, BuffValue> buffValues = new();
    //     private int id = -1;
    //     public int MaxStacks { get; private set; } = -1;
    //     public int CurStacks { get; private set; } = 1;
    //     public float Duration { get; private set; } = -1f;
    //     public float Timer { get; private set; } = 0f;
    //     public float TimerF
    //     {
    //         get
    //         {
    //             if (Duration <= 0f) return 0f;
    //             return 1f - (Timer / Duration);
    //         }
    //     }
    //     public float StackF
    //     {
    //         get
    //         {
    //             if (MaxStacks <= 0) return 0f;
    //             return (float)CurStacks / (float)MaxStacks;
    //         }
    //     }
    //     public string Name { get; set; } = "";
    //     public string Abbreviation { get; set; } = "";
    //     public bool clearAllStacksOnDurationEnd = false;
    //     public bool Degrading = false;
    //     public bool IsEmpty() { return CurStacks <= 0; }
    //     public bool DrawToUI() { return Abbreviation != ""; }
    //     public int GetID() { return id; }
    //
    //     public Buff(int id, float duration = -1, int maxStacks = -1, params BuffValue[] buffValues)
    //     {
    //         this.id = id;
    //         this.MaxStacks = maxStacks;
    //         this.Duration = duration;
    //         if (this.Duration > 0f) Timer = this.Duration;
    //         foreach (var buffValue in buffValues)
    //         {
    //             this.buffValues.Add(buffValue.id, buffValue);
    //         }
    //     }
    //
    //     public virtual (float totalBonus, float totalFlat) Get(params int[] tags)
    //     {
    //         float totalBonus = 0f;
    //         float totalFlat = 0f;
    //         if (IsEmpty()) return new(0f, 0f);
    //
    //         foreach (var buffValue in buffValues.Values)
    //         {
    //             if (tags.Contains(buffValue.id))
    //             {
    //                 totalBonus += buffValue.bonus;
    //                 totalFlat += buffValue.flat;
    //             }
    //         }
    //
    //         float f = 1f;
    //         if (Degrading && Duration > 0) f = TimerF;
    //
    //         return (totalBonus * CurStacks * f, totalFlat * CurStacks * f);
    //     }
    //     public void AddStack()
    //     {
    //         if (CurStacks < MaxStacks || MaxStacks < 0) CurStacks += 1;
    //         if (Duration > 0) Timer = Duration;
    //     }
    //     public bool RemoveStack()
    //     {
    //         CurStacks -= 1;
    //         if (CurStacks <= 0) return true;
    //
    //         return false;
    //     }
    //     public void Update(float dt)
    //     {
    //         if (IsEmpty()) return;
    //         if (Duration > 0f)
    //         {
    //             Timer -= dt;
    //             if (Timer <= 0f)
    //             {
    //                 if (clearAllStacksOnDurationEnd) CurStacks = 0;
    //                 else
    //                 {
    //                     CurStacks -= 1;
    //                     if (CurStacks > 0)
    //                     {
    //                         Timer = Duration;
    //                     }
    //                 }
    //             }
    //         }
    //     }
    //
    //     public virtual void DrawUI(Rect r, ColorRgba barColorRgba, ColorRgba bgColorRgba, ColorRgba textColorRgba) { }
    // }
    //
    // public class BuffSingle : IBuff
    // {
    //     protected BuffValue buffValue;
    //     private int id = -1;
    //     public int MaxStacks { get; private set; } = -1;
    //     public int CurStacks { get; private set; } = 1;
    //     public float Duration { get; private set; } = -1f;
    //     public float Timer { get; private set; } = 0f;
    //     public float TimerF
    //     {
    //         get
    //         {
    //             if (Duration <= 0f) return 0f;
    //             return 1f - (Timer / Duration);
    //         }
    //     }
    //     public float StackF
    //     {
    //         get
    //         {
    //             if (MaxStacks <= 0) return 0f;
    //             return (float)CurStacks / (float)MaxStacks;
    //         }
    //     }
    //     public string Name { get; set; } = "";
    //     public string Abbreviation { get; set; } = "";
    //     public bool clearAllStacksOnDurationEnd = false;
    //     public bool Degrading = false;
    //     public bool IsEmpty() { return CurStacks <= 0; }
    //     public bool DrawToUI() { return Abbreviation != ""; }
    //     public int GetID() { return id; }
    //
    //     public BuffSingle(int id, BuffValue buffValue, int maxStacks = -1, float duration = -1)
    //     {
    //         this.id = id;
    //         this.MaxStacks = maxStacks;
    //         this.Duration = duration;
    //         if (this.Duration > 0f) Timer = this.Duration;
    //         this.buffValue = buffValue;
    //     }
    //
    //     public virtual (float totalBonus, float totalFlat) Get(params int[] tags)
    //     {
    //         if (IsEmpty()) return new(0f, 0f);
    //         if(!tags.Contains(buffValue.id)) return new(0f, 0f);
    //         float f = 1f;
    //         if (Degrading && Duration > 0f) f = TimerF;
    //         return (buffValue.bonus * CurStacks * f, buffValue.flat * CurStacks * f);
    //     }
    //     public void AddStack()
    //     {
    //         if (CurStacks < MaxStacks || MaxStacks < 0) CurStacks += 1;
    //         if (Duration > 0) Timer = Duration;
    //     }
    //     public bool RemoveStack()
    //     {
    //         CurStacks -= 1;
    //         if (CurStacks <= 0) return true;
    //
    //         return false;
    //     }
    //     public void Update(float dt)
    //     {
    //         if (IsEmpty()) return;
    //         if (Duration > 0f)
    //         {
    //             Timer -= dt;
    //             if (Timer <= 0f)
    //             {
    //                 if (clearAllStacksOnDurationEnd) CurStacks = 0;
    //                 else
    //                 {
    //                     CurStacks -= 1;
    //                     if (CurStacks > 0)
    //                     {
    //                         Timer = Duration;
    //                     }
    //                 }
    //             }
    //         }
    //     }
    //
    //     public virtual void DrawUI(Rect r, ColorRgba barColorRgba, ColorRgba bgColorRgba, ColorRgba textColorRgba) { }
    // }
//deprecated--------------------------------

// public class StatContainer
//    {
//        public Dictionary<int, StatBonuses> stats = new();
//        //public event Action<StatBonuses>? StatChanged;
//        
//        public float ApplyBonuses(float baseValue, params int[] statIDs)
//        {
//            float bonusTotal = 1f;
//            float flatTotal = 0f;
//
//            foreach (var id in statIDs)
//            {
//                if (stats.ContainsKey(id))
//                {
//                    var stat = stats[id];
//                    bonusTotal += stat.BonusTotal;
//                    flatTotal += stat.FlatTotal;
//                }
//            }
//
//            return (baseValue + flatTotal) * bonusTotal;
//        }
//        public int ApplyBonuses(int baseValue, params int[] statIDs)
//        {
//            float bonusTotal = 1f;
//            float flatTotal = 0f;
//
//            foreach (var id in statIDs)
//            {
//                if (stats.ContainsKey(id))
//                {
//                    var stat = stats[id];
//                    bonusTotal += stat.BonusTotal;
//                    flatTotal += stat.FlatTotal;
//                }
//            }
//            float v = ((float)baseValue + flatTotal) * bonusTotal;
//            return (int)MathF.Ceiling(v);
//        }
//        
//        public void ApplyBonusesToStat(Stat baseStat)
//        {
//            float bonusTotal = 1f;
//            float flatTotal = 0f;
//
//            foreach (var id in baseStat.Tags)
//            {
//                if (stats.ContainsKey(id))
//                {
//                    var stat = stats[id];
//                    bonusTotal += stat.BonusTotal;
//                    flatTotal += stat.FlatTotal;
//                }
//            }
//            baseStat.UpdateCur(bonusTotal, flatTotal);
//        }
//        public void ApplyBonusesToStat(StatInt baseStat)
//        {
//            float bonusTotal = 1f;
//            float flatTotal = 0f;
//
//            foreach (var id in baseStat.Tags)
//            {
//                if (stats.ContainsKey(id))
//                {
//                    var stat = stats[id];
//                    bonusTotal += stat.BonusTotal;
//                    flatTotal += stat.FlatTotal;
//                }
//            }
//            baseStat.UpdateCur(bonusTotal, flatTotal);
//        }
//
//        public void ApplyBonusesToStats(params Stat[] stats)
//        {
//            foreach (var stat in stats)
//            {
//                ApplyBonusesToStat(stat);
//            }
//        }
//        public void ApplyBonusesToStats(params StatInt[] stats)
//        {
//            foreach (var stat in stats)
//            {
//                ApplyBonusesToStat(stat);
//            }
//        }
//        
//        public void Update(float dt)
//        {
//            foreach (var stat in stats.Values)
//            {
//                stat.Update(dt);
//            }
//        }
//
//        public void Add(StatBonuses stat)
//        {
//            if (!stats.ContainsKey(stat.ID))
//            {
//                stats.Add(stat.ID, stat);
//                //stat.Changed += OnStatChanged;
//            }
//        }
//        public void Remove(StatBonuses stat)
//        {
//            if (stats.ContainsKey(stat.ID))
//            {
//                //stat.Changed -= OnStatChanged;
//                stats.Remove(stat.ID);
//            }
//        }
//
//        public void Add(params int[] ids)
//        {
//            foreach (var id in ids)
//            {
//                Add(id);
//            }
//        }
//        public void Add(int id)
//        {
//            if (!stats.ContainsKey(id))
//            {
//                var stat = new StatBonuses(id);
//                //stat.Changed += OnStatChanged;
//                stats.Add(id, stat);
//            }
//        }
//        public void Remove(int id)
//        {
//            if (stats.ContainsKey(id))
//            {
//                //stats[id].Changed -= OnStatChanged;
//                stats.Remove(id);
//            }
//        }
//
//        public void ChangeStatBonus(int id, float bonus, float duration = -1, bool remove = false)
//        {
//            if (stats.ContainsKey(id))
//            {
//                if (remove)
//                {
//                    stats[id].RemoveBonus(bonus);
//                }
//                else stats[id].AddBonus(bonus, duration);
//            }
//        }
//        public void ChangeStatFlat(int id, float flat, float duration = -1, bool remove = false)
//        {
//            if (stats.ContainsKey(id))
//            {
//                if (remove)
//                {
//                    stats[id].RemoveFlat(flat);
//                }
//                else stats[id].AddFlat(flat, duration);
//            }
//        }
//
//        //private void OnStatChanged(StatBonuses stat) { StatChanged?.Invoke(stat); }
//    }
// public class StatBonuses
//    {
//        private float flatTotal = 0f;
//        private float bonusTotal = 1f;
//        private List<StatValue> timedBonuses = new();
//        private List<StatValue> timedFlats = new();
//
//        public int ID { get; set; } = -1;
//
//        public event Action<StatBonuses>? Changed;
//
//        public StatBonuses(int id) { this.ID = id; }
//        
//       
//        public float BonusTotal { get { return bonusTotal - 1f; } }
//        public float FlatTotal { get { return flatTotal; } }
//        
//        public float ApplyBonuses(float baseValue)
//        {
//           return (baseValue + flatTotal) * bonusTotal;
//        }
//        public int ApplyBonuses(int baseValue)
//        {
//            float v = ((float)baseValue + flatTotal) * bonusTotal;
//            return (int)MathF.Ceiling(v);
//        }
//        
//        public bool Update(float dt)
//        {
//            bool changed = false;
//            if (timedBonuses.Count > 0)
//            {
//                for (int i = timedBonuses.Count - 1; i >= 0; i--)
//                {
//                    if (timedBonuses[i].Update(dt))
//                    {
//                        bonusTotal -= timedBonuses[i].value;
//                        timedBonuses.RemoveAt(i);
//                        if (!changed) changed = true;
//                    }
//                }
//            }
//
//            if (timedFlats.Count > 0)
//            {
//                for (int i = timedFlats.Count - 1; i >= 0; i--)
//                {
//                    if (timedFlats[i].Update(dt))
//                    {
//                        flatTotal -= timedFlats[i].value;
//                        timedFlats.RemoveAt(i);
//                        if (!changed) changed = true;
//                    }
//                }
//            }
//
//            if (changed) Changed?.Invoke(this);
//
//            return changed;
//        }
//
//        public void Set(StatBonuses other)
//        {
//            flatTotal = other.flatTotal;
//            bonusTotal = other.bonusTotal;
//            foreach (var bonus in other.timedBonuses)
//            {
//                AddBonus(bonus.value, bonus.GetRemaining());
//            }
//            foreach (var flat in other.timedFlats)
//            {
//                AddFlat(flat.value, flat.GetRemaining());
//            }
//            Changed?.Invoke(this);
//        }
//        public void Add(StatBonuses other)
//        {
//            flatTotal += other.flatTotal;
//            bonusTotal += other.bonusTotal - 1f;
//
//            foreach (var bonus in other.timedBonuses)
//            {
//                AddBonus(bonus.value, bonus.GetRemaining());
//            }
//            foreach (var flat in other.timedFlats)
//            {
//                AddFlat(flat.value, flat.GetRemaining());
//            }
//
//            Changed?.Invoke(this);
//        }
//
//        public void AddBonuses(params float[] bonuses)
//        {
//            foreach (var bonus in bonuses)
//            {
//                bonusTotal += bonus;
//            }
//
//            Changed?.Invoke(this);
//        }
//        public void AddFlats(params float[] flats)
//        {
//            foreach (var flat in flats)
//            {
//                flatTotal += flat;
//            }
//            Changed?.Invoke(this);
//        }
//        
//        public void AddBonus(float bonus, float duration = -1f)
//        {
//            bonusTotal += bonus;
//            if (duration > 0f)
//            {
//                timedBonuses.Add(new(bonus, duration));
//            }
//            Changed?.Invoke(this);
//        }
//        public void AddFlat(float flat, float duration = -1f)
//        {
//            flatTotal += flat;
//            if (duration > 0f)
//            {
//                timedFlats.Add(new(flat, duration));
//            }
//            Changed?.Invoke(this);
//        }
//        public void RemoveBonus(float bonus)
//        {
//            bonusTotal -= bonus;
//            Changed?.Invoke(this);
//        }
//        public void RemoveFlat(float flat)
//        {
//            flatTotal -= flat;
//            Changed?.Invoke(this);
//        }
//        public void ResetBonus() { bonusTotal = 1f; timedBonuses.Clear(); Changed?.Invoke(this); }
//        public void ResetFlat() { flatTotal = 0f; timedFlats.Clear(); Changed?.Invoke(this); }
//        public void Reset()
//        {
//            bonusTotal = 1f;
//            timedBonuses.Clear();
//
//            flatTotal = 0f;
//            timedFlats.Clear();
//
//            Changed?.Invoke(this);
//        }
//    }
// public class StatNamed
//    {
//        private float prevTotal = 0f;
//        private float baseValue = 0f;
//        private float flatTotal = 0f;
//        private float bonusTotal = 1f;
//        private float total = 0f;
//
//        private Dictionary<string, StatValue> bonuses = new();
//        private Dictionary<string, StatValue> flatBonues = new();
//        public string ID { get; set; } = "";
//
//        public event Action<StatNamed>? Changed;
//        public StatNamed(float baseValue)
//        {
//            this.baseValue = baseValue;
//        }
//        public StatNamed(float baseValue, string id)
//        {
//            this.baseValue = baseValue;
//            this.ID = id;
//        }
//        public float BonusTotal { get { return bonusTotal; } }
//        public float FlatTotal { get { return flatTotal; } }
//        public float GetBase() { return baseValue; }
//        public float GetCur() { return total; }
//        public void Update(float dt)
//        {
//            if (bonuses.Count > 0)
//            {
//                var removeBonuses = bonuses.Where(kvp => kvp.Value.Update(dt));
//                
//                foreach (var kvp in removeBonuses)
//                {
//                    bonuses.Remove(kvp.Key);
//                }
//            }
//
//            if (flatBonues.Count > 0)
//            {
//                var removeFlats = flatBonues.Where(kvp => kvp.Value.Update(dt));
//                foreach (var kvp in removeFlats)
//                {
//                    flatBonues.Remove(kvp.Key);
//                }
//            }
//            UpdateTotal();
//        }
//        public void AddBonuses(params (string name, float value)[] bonuses)
//        {
//            foreach (var bonus in bonuses)
//            {
//                if (this.bonuses.ContainsKey(bonus.name)) this.bonuses[bonus.name] = new(bonus.value, -1f);// this.bonuses[bonus.name].Add(bonus.value);
//                else this.bonuses.Add(bonus.name, new(bonus.value, -1f));
//            }
//            UpdateTotal();
//        }
//        public void AddFlats(params (string name, float value)[] flats)
//        {
//            foreach (var flat in flats)
//            {
//                if (flatBonues.ContainsKey(flat.name)) flatBonues[flat.name] = new(flat.value, -1f);// flatBonues[flat.name].Add(flat.value);
//                else flatBonues.Add(flat.name, new(flat.value, -1f));
//            }
//            UpdateTotal();
//        }
//        public void AddBonus(string name, float bonus, float duration = -1f)
//        {
//            if (bonuses.ContainsKey(name)) { bonuses[name] = new(bonus, duration); }// bonuses[name].Add(bonus); }
//            else { bonuses.Add(name, new(bonus, duration)); }
//
//            UpdateTotal();
//        }
//        public void AddFlat(string name, float flat, float duration = -1f)
//        {
//            if (flatBonues.ContainsKey(name)) flatBonues[name] = new(flat, duration); // flatBonues[name] += flat;
//            else flatBonues.Add(name, new(flat, duration));
//
//            UpdateTotal();
//        }
//        public void RemoveBonuses(params string[] names)
//        {
//            foreach (var name in names)
//            {
//                if (!bonuses.ContainsKey(name)) continue;
//                bonuses.Remove(name);
//            }
//            UpdateTotal();
//        }
//        public void RemoveFlats(params string[] names)
//        {
//            foreach (var name in names)
//            {
//                if (!flatBonues.ContainsKey(name)) continue;
//                flatBonues.Remove(name);
//            }
//
//            UpdateTotal();
//        }
//        public void RemoveBonus(string name)
//        {
//            if (!bonuses.ContainsKey(name)) return;
//            bonuses.Remove(name);
//
//            UpdateTotal();
//        }
//        public void RemoveFlat(string name)
//        {
//            if (!flatBonues.ContainsKey(name)) return;
//            flatBonues.Remove(name);
//
//            UpdateTotal();
//        }
//        public void ClearBonuses()
//        {
//            bonuses.Clear();
//            UpdateTotal();
//        }
//        public void ClearFlat()
//        {
//            flatBonues.Clear();
//            UpdateTotal();
//        }
//        public void Clear()
//        {
//            bonuses.Clear();
//            flatBonues.Clear();
//            total = baseValue;
//            UpdateTotal();
//        }
//
//        private void UpdateTotal()
//        {
//            total = baseValue;
//            flatTotal = 0f;
//            bonusTotal = 1f;
//            foreach (var flat in flatBonues.Values)
//            {
//                flatTotal += flat.value;
//                total += flat.value;
//            }
//
//            float totalBonus = 1f;
//            foreach (var bonus in bonuses.Values)
//            {
//                bonusTotal += bonus.value;
//                totalBonus += bonus.value;
//            }
//
//            total *= totalBonus;
//
//            if(total != prevTotal) Changed?.Invoke(this);
//
//            prevTotal = total;
//        }
//    }
//

// Buff Value:
//     - bonus multiplier
//     - flat bonus
//     - int id
//
// Buff:
//     - Collection of Buff Values
//     - Stacks
//     - Duration
//     - Degrading
//     
//     Example:
//         - Freeze could be a negative Buff for movement & reload speed
//         - Buff Value for movement speed -5%
//         - Buff Value for reload speed +1 second
//         - each stack of freeze reduces movement speed & reload speed less -> how to do that?
//         - each stack could be applied to the cur value (cur value get smaller therefore bonus multiplier has less effect)
//         
//
// Stat:
//     - Base Value
//     - Cur Value = Base Value + All Flats * (1 + All Bonuses)
//     - Should it store all bonuses & flats or should bonuses & flats just be applied/ unapplied?
    
// public class BuffSimple : IBuff
// {
//     private Dictionary<int, BuffValue> buffValues = new();
//     private int id = -1;
//     public int CurStacks { get; private set; } = 1;
//     public string Name { get; set; } = "";
//     public string Abbreviation { get; set; } = "";
//     public bool IsEmpty() { return CurStacks <= 0; }
//     public bool DrawToUI() { return Abbreviation != ""; }
//     public int GetID() { return id; }
//
//     public BuffSimple(int id, params BuffValue[] buffValues)
//     {
//         this.id = id;
//         foreach (var buffValue in buffValues)
//         {
//             this.buffValues.Add(buffValue.id, buffValue);
//         }
//     }
//
//     public (float totalBonus, float totalFlat) Get(params int[] tags)
//     {
//         float totalBonus = 0f;
//         float totalFlat = 0f;
//         if (IsEmpty()) return new(0f, 0f);
//
//         foreach (var buffValue in buffValues.Values)
//         {
//             if (tags.Contains(buffValue.id))
//             {
//                 totalBonus += buffValue.bonus * CurStacks;
//                 totalFlat += buffValue.flat * CurStacks;
//             }
//         }
//         return (totalBonus, totalFlat);
//     }
//     public void AddStack()
//     {
//         CurStacks += 1;
//     }
//     public bool RemoveStack()
//     {
//         CurStacks -= 1;
//         if (CurStacks <= 0) return true;
//
//         return false;
//     }
//     public void Update(float dt) { }
//     public void DrawUI(Rectangle r, Color barColor, Color bgColor, Color textColor) { }
// }
// public class BuffPermanent : IBuff
// {
//     private Dictionary<int, BuffValue> buffValues = new();
//     private int id = -1;
//     public int MaxStacks { get; private set; } = -1;
//     public int CurStacks { get; private set; } = 1;
//     public float StackF
//     {
//         get
//         {
//             if (MaxStacks <= 0) return 0f;
//             return (float)CurStacks / (float)MaxStacks;
//         }
//     }
//     public string Name { get; set; } = "";
//     public string Abbreviation { get; set; } = "";
//     public bool IsEmpty() { return CurStacks <= 0; }
//     public bool DrawToUI() { return Abbreviation != ""; }
//     public int GetID() { return id; }
//
//     public BuffPermanent(int id, int maxStacks = -1, params BuffValue[] buffValues)
//     {
//         this.id = id;
//         this.MaxStacks = maxStacks;
//         foreach (var buffValue in buffValues)
//         {
//             this.buffValues.Add(buffValue.id, buffValue);
//         }
//     }
//
//     public (float totalBonus, float totalFlat) Get(params int[] tags)
//     {
//         float totalBonus = 0f;
//         float totalFlat = 0f;
//         if (IsEmpty()) return new(0f, 0f);
//
//         foreach (var buffValue in buffValues.Values)
//         {
//             if (tags.Contains(buffValue.id))
//             {
//                 totalBonus += buffValue.bonus * CurStacks;
//                 totalFlat += buffValue.flat * CurStacks;
//             }
//         }
//         return (totalBonus, totalFlat);
//     }
//     public void AddStack()
//     {
//         if (CurStacks < MaxStacks || MaxStacks < 0) CurStacks += 1;
//     }
//     public bool RemoveStack()
//     {
//         CurStacks -= 1;
//         if (CurStacks <= 0) return true;
//
//         return false;
//     }
//     public void Update(float dt) { }
//
//     public void DrawUI(Rectangle r, Color barColor, Color bgColor, Color textColor) { }
// }
// public class BuffSingleStack : IBuff
// {
//     private Dictionary<int, BuffValue> buffValues = new();
//     private int id = -1;
//     public int CurStacks { get; private set; } = 1;
//     public float Duration { get; private set; } = -1f;
//     public float Timer { get; private set; } = 0f;
//     public float TimerF
//     {
//         get
//         {
//             if (Duration <= 0f) return 0f;
//             return 1f - (Timer / Duration);
//         }
//     }
//     public string Name { get; set; } = "";
//     public string Abbreviation { get; set; } = "";
//     public bool clearAllStacksOnDurationEnd = false;
//     public bool IsEmpty() { return CurStacks <= 0; }
//     public bool DrawToUI() { return Abbreviation != ""; }
//     public int GetID() { return id; }
//
//     public BuffSingleStack(int id, float duration = -1, params BuffValue[] buffValues)
//     {
//         this.id = id;
//         this.Duration = duration;
//         foreach (var buffValue in buffValues)
//         {
//             this.buffValues.Add(buffValue.id, buffValue);
//         }
//     }
//
//     public (float totalBonus, float totalFlat) Get(params int[] tags)
//     {
//         float totalBonus = 0f;
//         float totalFlat = 0f;
//         if (IsEmpty()) return new(0f, 0f);
//
//         foreach (var buffValue in buffValues.Values)
//         {
//             if (tags.Contains(buffValue.id))
//             {
//                 totalBonus += buffValue.bonus * CurStacks;
//                 totalFlat += buffValue.flat * CurStacks;
//             }
//         }
//         return (totalBonus, totalFlat);
//     }
//     public void AddStack()
//     {
//         if (CurStacks < MaxStacks || MaxStacks < 0) CurStacks += 1;
//         if (Duration > 0) Timer = Duration;
//     }
//     public bool RemoveStack()
//     {
//         CurStacks -= 1;
//         if (CurStacks <= 0) return true;
//
//         return false;
//     }
//     public void Update(float dt)
//     {
//         if (IsEmpty()) return;
//         if (Duration > 0f)
//         {
//             Timer -= dt;
//             if (Timer <= 0f)
//             {
//                 if (clearAllStacksOnDurationEnd) CurStacks = 0;
//                 else
//                 {
//                     CurStacks -= 1;
//                     if (CurStacks > 0)
//                     {
//                         Timer = Duration;
//                     }
//                 }
//             }
//         }
//     }
//
//     public void DrawUI(Rectangle r, Color barColor, Color bgColor, Color textColor) { }
// }
// public class BuffDegrading : BuffSingleStack
// {
//
// }

// public class BuffContainer
    // {
    //     private Dictionary<int, IBuff> buffs = new();
    //     
    //     public List<IBuff> GetBuffs() { return buffs.Values.ToList(); }
    //     public List<IBuff> GetBuffs(Predicate<IBuff> match) { return GetBuffs().FindAll(match); }
    //     public List<IBuff> GetUIBuffs() { return GetBuffs(b => b.DrawToUI()); }
    //     
    //     public void AddBuff(IBuff buff)
    //     {
    //         if (buffs.ContainsKey(buff.GetID())) buffs[buff.GetID()].AddStack();
    //         else buffs.Add(buff.GetID(), buff);
    //     }
    //     public void RemoveBuff(int id)
    //     {
    //         if (buffs.ContainsKey(id))
    //         {
    //             buffs[id].RemoveStack();
    //             if (buffs[id].IsEmpty()) buffs.Remove(id);
    //         }
    //     }
    //     public void RemoveBuff(IBuff buff)
    //     {
    //         if (buffs.ContainsKey(buff.GetID()))
    //         {
    //             buffs[buff.GetID()].RemoveStack();
    //             if (buffs[buff.GetID()].IsEmpty()) buffs.Remove(buff.GetID());
    //         }
    //     }
    //    
    //     public void Update(float dt)
    //     {
    //         for (int i = buffs.Count - 1; i >= 0; i--)
    //         {
    //             var buff = buffs.ElementAt(i).Value;
    //             buff.Update(dt);
    //             if (buff.IsEmpty())
    //             {
    //                 buffs.Remove(buff.GetID());
    //             }
    //         }
    //     }
    //     
    //     public void Apply(params IStat[] stats)
    //     {
    //         if (buffs.Count <= 0) return;
    //         foreach (var stat in stats)
    //         {
    //             ApplyStat(stat);
    //         }
    //     }
    //     public void Apply(IStat stat)
    //     {
    //         if (buffs.Count <= 0) return;
    //         ApplyStat(stat);
    //     }
    //     
    //     private void ApplyStat(IStat stat)
    //     {
    //         float totalFlat = 0f;
    //         float totalBonus = 0f;
    //         foreach (var buff in buffs.Values)
    //         {
    //             var info = buff.Get(stat.Tags);
    //             totalBonus += info.totalBonus;
    //             totalFlat += info.totalFlat;
    //         }
    //
    //         stat.UpdateCur(totalBonus, totalFlat);
    //     }
    // }

// public interface IBuffable
    // {
    //     void AddBuff(IBuff buff);
    //     void RemoveBuff(IBuff buff);
    //     void RemoveBuff(int id);
    // }
// public interface IStat
    // {
    //     public int[] Tags { get; set; }
    //     public void UpdateCur(float totalBonus, float totalFlat);
    // }
    //
    // public class Stat : IStat
    // {
    //     public event Action<Stat, float>? CurChanged;
    //     public float Base { get; private set; } = 0f;
    //     public float Cur { get; private set; } = 0f;
    //     public float F
    //     {
    //         get
    //         {
    //             if (Base <= 0f) return 0f;
    //             return Cur / Base;
    //         }
    //     }
    //     public int[] Tags { get; set; }
    //     public Stat(float baseValue, params int[] tags) { Base = baseValue; Cur = baseValue; Tags = tags; }
    //     public void SetBase(float value)
    //     {
    //         Base = value;
    //         Cur = value;
    //     }
    //     public void UpdateCur(float totalBonuses, float totalFlats)
    //     {
    //         float old = Cur;
    //         if (totalBonuses >= 0f)
    //         {
    //             Cur = (Base + totalFlats) * (1f + totalBonuses);
    //         }
    //         else
    //         {
    //             Cur = (Base + totalFlats) / (1f + MathF.Abs(totalBonuses));
    //         }
    //
    //         if (Cur != old) CurChanged?.Invoke(this, old);
    //     }
    //
    // }
    //
    // public class StatInt : IStat
    // {
    //     public event Action<StatInt, int>? CurChanged;
    //     public int Base { get; private set; } = 0;
    //     public int Cur { get; private set; } = 0;
    //     public float F
    //     {
    //         get
    //         {
    //             if (Base <= 0f) return 0f;
    //             return (float)Cur / (float)Base;
    //         }
    //     }
    //     public int[] Tags { get; set; }
    //
    //     public StatInt(int baseValue, params int[] tags) { Base = baseValue; Cur = baseValue; this.Tags = tags; }
    //     public void SetBase(int value)
    //     {
    //         Base = value;
    //         Cur = value;
    //     }
    //     public void UpdateCur(float totalBonuses, float totalFlats)
    //     {
    //         int old = Cur;
    //         if (totalBonuses >= 0f)
    //         {
    //             float v = ((float)Base + totalFlats) * totalBonuses;
    //             Cur = (int)MathF.Ceiling(v);
    //         }
    //         else
    //         {
    //             float v = ((float)Base + totalFlats) / (1f + MathF.Abs(totalBonuses));
    //             Cur = (int)MathF.Ceiling(v);
    //         }
    //
    //         if (Cur != old) CurChanged?.Invoke(this, old);
    //     }
    //
    // }

    
    