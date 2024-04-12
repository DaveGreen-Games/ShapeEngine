using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

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

public interface IBuffFactory
{
    public IBuff2 Create();
}
public interface IBuff2
{
    public void AddStacks(int amount);
    public bool RemoveStacks(int amount);
    public void ApplyTo(ShapeStat stat);
    public void Update(float dt);
    public bool IsFinished();
}


public class BuffCreator
{
    public class Buff
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
            // public Value Add(Effect other) => new(Bonus + other.Bonus, Flat + other.Flat);
            
            public string ToText()
            {
                float bonusPercentage = (1 + Bonus) * 100;
                return $"+{(int)bonusPercentage}% +{(int)Flat}";
            }
        }
        public readonly struct Effect
        {
        
            public readonly uint Tag;
            public readonly float Bonus;
            public readonly float Flat;
            public readonly string TagName;
            
            public Effect(uint tag, float bonus = 0f, float flat = 0f, string tagName = "")
            {
                Tag = tag;
                Bonus = bonus;
                Flat = flat;
                TagName = tagName;
            }
        
            public string ToText()
            {
                float bonusPercentage = (1 + Bonus) * 100;
                return $"{TagName} +{(int)bonusPercentage}% +{(int)Flat}";
            }
        
        }

        protected readonly List<Effect> Effects;
        public uint Id { get; private set; }
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

        
        
        internal Buff(uint id, int maxStacks = -1, float duration = -1)
        {
            Id = id;
            MaxStacks = maxStacks;
            Duration = duration;
            if (this.Duration > 0f) Timer = this.Duration;
            else Timer = 0f;

            Effects = new();
        }
        internal Buff(uint id, int maxStacks, float duration, params Effect[] effects)
        {
            Id = id;
            MaxStacks = maxStacks;
            Duration = duration;
            if (this.Duration > 0f) Timer = this.Duration;
            else Timer = 0f;

            this.Effects = new(effects.Length);
            this.Effects.AddRange(effects);
        }
        

        // public Buff2 Clone() => new(Id, MaxStacks, Duration, Effects.ToArray());

        public void AddEffect(Effect effect)
        {
            Effects.Add(effect);
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
        public void ApplyTo(ShapeStat stat)
        {
            if (Effects.Count <= 0) return;
            foreach (var effect in Effects)
            {
                if (stat.IsAffected(effect.Tag))
                {
                    stat.Apply(GetCurBuffValue(effect));
                }
            }
        }
        public virtual void Update(float dt)
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
        public virtual bool IsFinished() => Duration > 0f && Timer <= 0f;
        public virtual void GetEffectTexts(ref List<string> result)
        {
            foreach (var effect in Effects)
            {
                var v = GetCurBuffValue(effect);
                result.Add(v.ToText());
            }
        }
        public virtual string GetStackText() => $"Stacks {CurStacks}/{MaxStacks}";
        
        protected virtual Buff.Value GetCurBuffValue(Buff.Effect effect)
        {
            float f = 1f;
            if (Degrading && Duration > 0) f = TimerF;
        
            var stacks = CurStacks + 1;
        
            return new (effect.Bonus * stacks * f, effect.Flat * stacks * f);
        }

    }

    private readonly Buff.Effect[] effects;
    private readonly uint id;
    private readonly int stacks;
    private readonly float duration;
    private readonly bool stacksReplenishDuration;
    private readonly bool clearAllStacksOnDurationEnd;
    private readonly bool degrading;

    public BuffCreator(uint id, int stacks, float duration, bool stacksReplenishDuration,
        bool clearAllStacksOnDurationEnd, bool degrading, params Buff.Effect[] effects)
    {
        this.id = id;
        this.stacks = stacks;
        this.duration = duration;
        this.stacksReplenishDuration = stacksReplenishDuration;
        this.clearAllStacksOnDurationEnd = clearAllStacksOnDurationEnd;
        this.degrading = degrading;
        this.effects = effects;
    }
    
    public BuffCreator(uint id, int stacks, float duration, params Buff.Effect[] effects)
    {
        this.id = id;
        this.stacks = stacks;
        this.duration = duration;
        this.stacksReplenishDuration = true;
        this.clearAllStacksOnDurationEnd = false;
        this.degrading = false;
        this.effects = effects;
    }
    public BuffCreator(uint id, params Buff.Effect[] effects)
    {
        this.id = id;
        this.stacks = -1;
        this.duration = -1;
        this.stacksReplenishDuration = true;
        this.clearAllStacksOnDurationEnd = false;
        this.degrading = false;
        this.effects = effects;
    }
    
    public Buff Clone()
    {
        var b = new Buff(id, stacks, duration, effects);
        b.Degrading = degrading;
        b.StacksReplenishDuration = stacksReplenishDuration;
        b.ClearAllStacksOnDurationEnd = clearAllStacksOnDurationEnd;
        return b;
    }
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
    private BuffCreator.Buff.Value total = new();
    
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
    public void Apply(BuffCreator.Buff.Value buffValue)
    {
        if (Locked) return;
        total = total.Add(buffValue);
    }
}
public class ShapeStatSystem
{
    public event Action<BuffCreator.Buff>? OnBuffRemoved;

    private readonly Dictionary<uint, ShapeStat> stats;
    private readonly Dictionary<uint, BuffCreator.Buff> buffEffects;

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
                ResolveOnBuffRemoved(buff);
            }
            else
            {
                foreach (var stat in statValues)
                {
                    buff.ApplyTo(stat);
                    // if(stat.IsAffected(buff.Tag))
                    // {
                    //     stat.Apply(buff.GetCurBuffValue());
                    // }
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
    public void AddBuff(BuffCreator.Buff buffEffect)
    {
        if (buffEffects.ContainsKey(buffEffect.Id))
        {
            buffEffects[buffEffect.Id].AddStacks(1);
        }
        else buffEffects.Add(buffEffect.Id, buffEffect);
    }
    public bool RemoveBuff(BuffCreator.Buff buffEffect)
    {
        if (buffEffects.ContainsKey(buffEffect.Id))
        {
            if (buffEffects[buffEffect.Id].RemoveStacks(1))
            {
                buffEffects.Remove(buffEffect.Id);
                ResolveOnBuffRemoved(buffEffect);
            }

            return true;
        }

        return false;
    }

    protected virtual void BuffWasRemoved(BuffCreator.Buff buff) { }
    private void ResolveOnBuffRemoved(BuffCreator.Buff buff)
    {
        BuffWasRemoved(buff);
        OnBuffRemoved?.Invoke(buff);
    }
}