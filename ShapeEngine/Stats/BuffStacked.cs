namespace ShapeEngine.Stats;

public class BuffStacked : BuffTimed
{
    public int MaxStacks { get; private set; }
    public int CurStacks { get; private set; }
    public float StackF
    {
        get
        {
            if (MaxStacks <= 0) return 0f;
            return (float)CurStacks / (float)MaxStacks;
        }
    }

    public bool StacksReplenishDuration;
    public bool ClearAllStacksOnDurationEnd;
    
    public BuffStacked(uint id, int maxStacks) : base(id, 0f, false)
    {
        MaxStacks = maxStacks;
        StacksReplenishDuration = true;
        ClearAllStacksOnDurationEnd = false;
    }
    public BuffStacked(uint id, int maxStacks, params BuffEffect[] effects) : base(id, 0f, false, effects)
    {
        MaxStacks = maxStacks;
        StacksReplenishDuration = true;
        ClearAllStacksOnDurationEnd = false;
    }
    public BuffStacked(uint id, int maxStacks, bool stacksReplenishDuration, bool clearAllStacksOnDurationEnd) : base(id, 0f, false)
    {
        MaxStacks = maxStacks;
        StacksReplenishDuration = stacksReplenishDuration;
        ClearAllStacksOnDurationEnd = clearAllStacksOnDurationEnd;
    }
    public BuffStacked(uint id, int maxStacks, bool stacksReplenishDuration, bool clearAllStacksOnDurationEnd, params BuffEffect[] effects) : base(id, 0f, false, effects)
    {
        MaxStacks = maxStacks;
        StacksReplenishDuration = stacksReplenishDuration;
        ClearAllStacksOnDurationEnd = clearAllStacksOnDurationEnd;
    }
    public BuffStacked(uint id, int maxStacks, float duration, bool degrading, params BuffEffect[] effects) : base(id, duration, degrading, effects)
    {
        MaxStacks = maxStacks;
        StacksReplenishDuration = true;
        ClearAllStacksOnDurationEnd = false;
    }
    public BuffStacked(uint id, int maxStacks, float duration, bool degrading, bool stacksReplenishDuration, bool clearAllStacksOnDurationEnd, params BuffEffect[] effects) : base(id, duration, degrading, effects)
    {
        MaxStacks = maxStacks;
        StacksReplenishDuration = stacksReplenishDuration;
        ClearAllStacksOnDurationEnd = clearAllStacksOnDurationEnd;
    }
    
    public override IBuff Clone() => new BuffStacked(Id, MaxStacks, Duration, Degrading, StacksReplenishDuration, ClearAllStacksOnDurationEnd, Effects.ToArray());
    
    public override void AddStacks(int amount)
    {
        if (CurStacks < MaxStacks || MaxStacks < 0) CurStacks += amount;
        if (Duration > 0 && StacksReplenishDuration) Timer = Duration;
    }
    public override bool RemoveStacks(int amount)
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
    public override void Update(float dt)
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
    public override bool IsFinished() => Duration > 0f && Timer <= 0f;
    public virtual string GetStackText() => $"Stacks {CurStacks}/{MaxStacks}";
    protected override BuffValue GetCurBuffValue(BuffEffect effect)
    {
        float f = 1f;
        if (Degrading && Duration > 0) f = TimerF;
    
        var stacks = CurStacks + 1;
    
        return new (effect.Bonus * stacks * f, effect.Flat * stacks * f);
    }
}