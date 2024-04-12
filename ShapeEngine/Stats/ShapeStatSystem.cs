using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Stats;


/*
    Change Stat system (again...)
        -> stat system holds IBuffs
        -> stat system updates IBuffs
        -> stat system removes finished IBuffs
        -> stat system applies all buff effects of non finished IBuffs to stats
        -> Tag stays in buff effect
        -> Id moves from buff effect to IBuff
        
        -> IBuff has Draw(Rect rect) function
        -> IBuffEffect keeps name & get text functions
        -> IBuff receives Name & GetTextFunction
*/

//TESTING--------
public static class BuffTags
{
    public static readonly uint MovementSpeed = BitFlag.NextFlag;
    public static readonly uint ReloadSpeed = BitFlag.NextFlag;
}
public class FreezeNerf : IShapeBuff
{
    public static readonly uint MovementNerfId = ShapeID.NextID;
    public static readonly uint ReloadNerfId = ShapeID.NextID;
    public static BuffEffect MovementNerf => new BuffEffect(MovementNerfId, BuffTags.MovementSpeed, -0.1f, 0f, 5, 2f);
    public static BuffEffect ReloadNerf => new BuffEffect(ReloadNerfId, BuffTags.ReloadSpeed, 0f, 0.25f, 1, 4f);
    
    public void AddTo(ShapeStatSystem shapeStatSystem)
    {
        shapeStatSystem.AddBuffEffect(MovementNerf);
        shapeStatSystem.AddBuffEffect(ReloadNerf);
    }

    public void RemoveFrom(ShapeStatSystem shapeStatSystem)
    {
        shapeStatSystem.RemoveBuffEffect(MovementNerf);
        shapeStatSystem.RemoveBuffEffect(ReloadNerf);
    }
}
//---------------

public interface IShapeBuff
{
    public void AddTo(ShapeStatSystem shapeStatSystem);

    public void RemoveFrom(ShapeStatSystem shapeStatSystem);
}
public class BuffEffect
{
    public readonly struct Value
    {
        public readonly float Bonus;
        public readonly float Flat;
        public Value()
        {
            this.Bonus = 0f;
            this.Flat = 0f;
        }
        public Value(float bonus, float flat)
        {
            this.Bonus = bonus;
            this.Flat = flat;
        }

        public float Apply(float baseValue) => (baseValue + Flat) * (1f + Bonus);

        public Value Add(Value other) => new(Bonus + other.Bonus, Flat + other.Flat);
    }
    public uint Tag { get; private set; }
    public uint Id { get; private set; }
    
    //split into bonus & flat
    //buff value is just used for final value
    // public Buff2.Value BuffValue { get; private set; }
    public readonly float Bonus;
    public readonly float Flat;
    public int MaxStacks { get; private set; }
    public int CurStacks { get; private set; }
    public float Duration { get; private set; }
    public float Timer { get; private set; }
    public float TimerF
    {
        get
        {
            if (Duration <= 0f) return 0f;
            return 1f - (Timer / Duration);
        }
    }
    public float StackF
    {
        get
        {
            if (MaxStacks <= 0) return 0f;
            return (float)CurStacks / (float)MaxStacks;
        }
    }

    public bool StacksReplenishDuration = true;
    public bool ClearAllStacksOnDurationEnd = false;
    public bool Degrading = false;

    public BuffEffect(uint id, uint tag, float bonus = 0f, float flat = 0f, int maxStacks = -1, float duration = -1)
    {
        Id = id;
        Tag = tag;
        Bonus = bonus;
        Flat = flat;
        MaxStacks = maxStacks;
        Duration = duration;
        if (this.Duration > 0f) Timer = this.Duration;
        else Timer = 0f;
    }
    public void AddStacks(int amount)
    {
        if (CurStacks < MaxStacks || MaxStacks < 0) CurStacks += amount;
        if (Duration > 0 && StacksReplenishDuration) Timer = Duration;
    }
    public bool RemoveStacks(int amount)
    {
        CurStacks -= amount;
        if (CurStacks <= 0)
        {
            CurStacks = 0;
            return true;
        }

        return false;
    }
    public BuffEffect.Value GetCurBuffValue()
    {
        float f = 1f;
        if (Degrading && Duration > 0) f = TimerF;

        var stacks = CurStacks + 1;

        return new (Bonus * stacks * f, Flat * stacks * f);
    }
    public void Update(float dt)
    {
        if (Duration > 0f)
        {
            Timer -= dt;
            if (Timer <= 0f)
            {
                if (ClearAllStacksOnDurationEnd) CurStacks = 0;
                else
                {
                    CurStacks -= 1;
                    if (CurStacks > 0)
                    {
                        Timer = Duration;
                    }
                }
            }
        }
    }
    
    
    public virtual void Draw(Rect rect) { }
    
    public bool IsFinished() => Duration > 0f && Timer <= 0f;

    public string GetValueText()
    {
        var cur = GetCurBuffValue();
        float bonusPercentage = (1 + cur.Bonus) * 100;
        return $"+{(int)bonusPercentage}% +{(int)cur.Flat}";
    }

    public string GetStackText() => $"Stacks {CurStacks}/{MaxStacks}";
}
public class ShapeStat
{
    private readonly BitFlag mask;
    public uint Id { get; private set; }
    public string Name = "";
    public string NameAbbreviation = "";
    public float BaseValue { get; set; }
    public float CurValue => total.Apply(BaseValue);
    public bool Locked
    {
        get => locked;
        set
        {
            if (value && !locked) Reset();
            locked = value;
        }
    }
    
    private bool locked = false;
    private BuffEffect.Value total = new();
    
    public ShapeStat(uint id, float baseValue, BitFlag tagMask)
    {
        Id = id;
        BaseValue = baseValue;
        mask = tagMask;
    }
    
    
    public virtual void Draw(Rect rect) { }

    public string GetText(bool abbreviated)
    {
        float bonusPercent = (1 + total.Bonus) * 100;
        return $"{(abbreviated ? NameAbbreviation : Name)}: {CurValue} [+{(int)bonusPercent}% +{(int)total.Flat}]";
    }
    public bool IsAffected(uint tag) => mask.Has(tag);

    public void Reset()
    {
        total = new();
    }
    public void Apply(BuffEffect.Value buffValue)
    {
        if (Locked) return;
        total = total.Add(buffValue);
    }
}
public class ShapeStatSystem
{
    public event Action<BuffEffect>? OnBuffEffectRemoved;

    private readonly Dictionary<uint, ShapeStat> stats;
    private readonly Dictionary<uint, BuffEffect> buffEffects;

    public ShapeStatSystem()
    {
        stats = new(16);
        buffEffects = new(24);
    }

    public void Update(float dt)
    {
        var statValues = this.stats.Values;
        foreach (var stat in statValues)
        {
            stat.Reset();
        }
        foreach (var kvp in buffEffects)
        {
            var buff = kvp.Value;
            buff.Update(dt);
            if (buff.IsFinished())
            {
                buffEffects.Remove(kvp.Key);
                ResolveOnBuffEffectRemoved(buff);
            }
            else
            {
                foreach (var stat in statValues)
                {
                    if(stat.IsAffected(buff.Tag))
                    {
                        stat.Apply(buff.GetCurBuffValue());
                    }
                }
            }
        }
    }
    
    public void AddStat(ShapeStat stat)
    {
        stats[stat.Id] = stat;
    }
    public bool RemoveStat(ShapeStat stat)
    {
        var removed = stats.Remove(stat.Id);
        if(removed) stat.Reset();
        return removed;
    }
    public void AddBuffEffect(BuffEffect buffEffect)
    {
        if (buffEffects.ContainsKey(buffEffect.Id))
        {
            buffEffects[buffEffect.Id].AddStacks(1);
        }
        else buffEffects.Add(buffEffect.Id, buffEffect);
    }
    public bool RemoveBuffEffect(BuffEffect buffEffect)
    {
        if (buffEffects.ContainsKey(buffEffect.Id))
        {
            if (buffEffects[buffEffect.Id].RemoveStacks(1))
            {
                buffEffects.Remove(buffEffect.Id);
                ResolveOnBuffEffectRemoved(buffEffect);
            }

            return true;
        }

        return false;
    }

    public void AddBuff(IShapeBuff buff)
    {
        buff.AddTo(this);
    }

    public void RemoveBuff(IShapeBuff buff)
    {
        buff.RemoveFrom(this);
    }
    protected virtual void BuffEffectWasRemoved(BuffEffect buff) { }
    private void ResolveOnBuffEffectRemoved(BuffEffect buff)
    {
        BuffEffectWasRemoved(buff);
        OnBuffEffectRemoved?.Invoke(buff);
    }
}