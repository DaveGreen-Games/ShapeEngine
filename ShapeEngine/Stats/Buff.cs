using ShapeEngine.Core.Shapes;

namespace ShapeEngine.Stats;


/*
 Buff Hierarchy
    - Buff
    - BuffStacked
    - BuffTimed
    - BuffStackedTimed? (better name)
*/


public class BuffSimple : IBuff
{
     protected readonly List<BuffEffect> Effects;
    public uint Id { get; private set; }
    public uint GetId() => Id;

    
    internal BuffSimple(uint id)
    {
        Id = id;
        Effects = new();
    }
    internal BuffSimple(uint id, params BuffEffect[] effects)
    {
        Id = id;
        this.Effects = new(effects.Length);
        this.Effects.AddRange(effects);
    }

    public IBuff Clone() => new BuffSimple(Id, Effects.ToArray());

    public void AddEffect(BuffEffect buffEffect)
    {
        Effects.Add(buffEffect);
    }

    public void AddStacks(int amount) { }

    public bool RemoveStacks(int amount) => true;
    public void ApplyTo(IStat stat)
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
    public virtual void Update(float dt) { }
    public virtual void Draw(Rect rect) { }
    public virtual bool IsFinished() => false;
    public virtual void GetEffectTexts(ref List<string> result)
    {
        foreach (var effect in Effects)
        {
            var v = GetCurBuffValue(effect);
            result.Add(v.ToText());
        }
    }
    protected virtual BuffValue GetCurBuffValue(BuffEffect effect)
    {
        return new (effect.Bonus, effect.Flat);
    }
}
public class Buff : IBuff
{
    protected readonly List<BuffEffect> Effects;
    public uint Id { get; private set; }
    public uint GetId() => Id;
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
    internal Buff(uint id, int maxStacks, float duration, params BuffEffect[] effects)
    {
        Id = id;
        MaxStacks = maxStacks;
        Duration = duration;
        if (this.Duration > 0f) Timer = this.Duration;
        else Timer = 0f;

        this.Effects = new(effects.Length);
        this.Effects.AddRange(effects);
    }

    // public Buff Clone() => new(Id, MaxStacks, Duration, Effects.ToArray());
    public IBuff Clone() => new Buff(Id, MaxStacks, Duration, Effects.ToArray());

    public void AddEffect(BuffEffect buffEffect)
    {
        Effects.Add(buffEffect);
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
            Timer = 0f;
            return true;
        }

        return false;
    }
    public void ApplyTo(IStat stat)
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
    
    protected virtual BuffValue GetCurBuffValue(BuffEffect effect)
    {
        float f = 1f;
        if (Degrading && Duration > 0) f = TimerF;
    
        var stacks = CurStacks + 1;
    
        return new (effect.Bonus * stacks * f, effect.Flat * stacks * f);
    }

}