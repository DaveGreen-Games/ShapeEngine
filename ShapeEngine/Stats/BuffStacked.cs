namespace ShapeEngine.Stats;

/// <summary>
/// Represents a buff that supports stacking and timing,
/// allowing for multiple stacks and optional duration-based degradation.
/// </summary>
/// <remarks>
/// BuffStacked extends BuffTimed to provide stack management and advanced duration handling.
/// </remarks>
public class BuffStacked : BuffTimed
{
    /// <summary>
    /// The maximum number of stacks this buff can have.
    /// </summary>
    public int MaxStacks { get; private set; }
    /// <summary>
    /// The current number of stacks.
    /// </summary>
    public int CurStacks { get; private set; }
    /// <summary>
    /// The current stack fraction (CurStacks / MaxStacks).
    /// </summary>
    public float StackF
    {
        get
        {
            if (MaxStacks <= 0) return 0f;
            return CurStacks / (float)MaxStacks;
        }
    }

    /// <summary>
    /// If true, adding stacks replenishes the buff's duration.
    /// </summary>
    public bool StacksReplenishDuration;
    /// <summary>
    /// If true, all stacks are cleared when the duration ends.
    /// </summary>
    public bool ClearAllStacksOnDurationEnd;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="BuffStacked"/> class with a maximum stack count.
    /// </summary>
    /// <param name="id">The unique identifier for the buff.</param>
    /// <param name="maxStacks">The maximum number of stacks.</param>
    public BuffStacked(uint id, int maxStacks) : base(id, 0f, false)
    {
        MaxStacks = maxStacks;
        StacksReplenishDuration = true;
        ClearAllStacksOnDurationEnd = false;
    }
    /// <summary>
    /// Initializes a new instance with effects.
    /// </summary>
    /// <param name="id">The unique identifier for the buff.</param>
    /// <param name="maxStacks">The maximum number of stacks.</param>
    /// <param name="effects">The effects to apply.</param>
    public BuffStacked(uint id, int maxStacks, params BuffEffect[] effects) : base(id, 0f, false, effects)
    {
        MaxStacks = maxStacks;
        StacksReplenishDuration = true;
        ClearAllStacksOnDurationEnd = false;
    }
    /// <summary>
    /// Initializes a new instance with stack and duration options.
    /// </summary>
    /// <param name="id">The unique identifier for the buff.</param>
    /// <param name="maxStacks">The maximum number of stacks.</param>
    /// <param name="stacksReplenishDuration">If true, stacks replenish duration.</param>
    /// <param name="clearAllStacksOnDurationEnd">If true, all stacks are cleared when duration ends.</param>
    public BuffStacked(uint id, int maxStacks, bool stacksReplenishDuration, bool clearAllStacksOnDurationEnd) : base(id, 0f, false)
    {
        MaxStacks = maxStacks;
        StacksReplenishDuration = stacksReplenishDuration;
        ClearAllStacksOnDurationEnd = clearAllStacksOnDurationEnd;
    }
    /// <summary>
    /// Initializes a new instance with stack, duration, and effects options.
    /// </summary>
    /// <param name="id">The unique identifier for the buff.</param>
    /// <param name="maxStacks">The maximum number of stacks.</param>
    /// <param name="stacksReplenishDuration">If true, stacks replenish duration.</param>
    /// <param name="clearAllStacksOnDurationEnd">If true, all stacks are cleared when duration ends.</param>
    /// <param name="effects">The effects to apply.</param>
    public BuffStacked(uint id, int maxStacks, bool stacksReplenishDuration, bool clearAllStacksOnDurationEnd, params BuffEffect[] effects) : base(id, 0f, false, effects)
    {
        MaxStacks = maxStacks;
        StacksReplenishDuration = stacksReplenishDuration;
        ClearAllStacksOnDurationEnd = clearAllStacksOnDurationEnd;
    }
    /// <summary>
    /// Initializes a new instance with stack, duration, degrading, and effects options.
    /// </summary>
    /// <param name="id">The unique identifier for the buff.</param>
    /// <param name="maxStacks">The maximum number of stacks.</param>
    /// <param name="duration">The duration of the buff.</param>
    /// <param name="degrading">If true, the buff degrades over time.</param>
    /// <param name="effects">The effects to apply.</param>
    public BuffStacked(uint id, int maxStacks, float duration, bool degrading, params BuffEffect[] effects) : base(id, duration, degrading, effects)
    {
        MaxStacks = maxStacks;
        StacksReplenishDuration = true;
        ClearAllStacksOnDurationEnd = false;
    }
    /// <summary>
    /// Initializes a new instance with all options.
    /// </summary>
    /// <param name="id">The unique identifier for the buff.</param>
    /// <param name="maxStacks">The maximum number of stacks.</param>
    /// <param name="duration">The duration of the buff.</param>
    /// <param name="degrading">If true, the buff degrades over time.</param>
    /// <param name="stacksReplenishDuration">If true, stacks replenish duration.</param>
    /// <param name="clearAllStacksOnDurationEnd">If true, all stacks are cleared when duration ends.</param>
    /// <param name="effects">The effects to apply.</param>
    public BuffStacked(uint id, int maxStacks, float duration, bool degrading, bool stacksReplenishDuration, bool clearAllStacksOnDurationEnd, params BuffEffect[] effects) : base(id, duration, degrading, effects)
    {
        MaxStacks = maxStacks;
        StacksReplenishDuration = stacksReplenishDuration;
        ClearAllStacksOnDurationEnd = clearAllStacksOnDurationEnd;
    }
    
    /// <summary>
    /// Creates a copy of this buff, including stack and duration settings.
    /// </summary>
    /// <returns>A new <see cref="BuffStacked"/> instance with the same properties.</returns>
    public override IBuff Clone() => new BuffStacked(Id, MaxStacks, Duration, Degrading, StacksReplenishDuration, ClearAllStacksOnDurationEnd, Effects.ToArray());
    
    /// <summary>
    /// Adds stacks to this buff, up to the maximum. Optionally replenishes duration.
    /// </summary>
    /// <param name="amount">The number of stacks to add.</param>
    public override void AddStacks(int amount)
    {
        if (CurStacks < MaxStacks || MaxStacks < 0) CurStacks += amount;
        if (Duration > 0 && StacksReplenishDuration) Timer = Duration;
    }
    /// <summary>
    /// Removes stacks from this buff. If stacks reach zero, the buff is finished.
    /// </summary>
    /// <param name="amount">The number of stacks to remove.</param>
    /// <returns>True if the buff should be removed; otherwise, false.</returns>
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
    /// <summary>
    /// Updates the buff's timer and stack state.
    /// </summary>
    /// <param name="dt">The time delta since the last update.</param>
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
    /// <summary>
    /// Determines whether the buff is finished and should be removed.
    /// The buff is considered finished if its duration is greater than zero and the timer has reached zero or below.
    /// </summary>
    /// <returns>True if the buff is finished; otherwise, false.</returns>
    public override bool IsFinished() => Duration > 0f && Timer <= 0f;
    /// <summary>
    /// Gets a string representing the current stack state.
    /// </summary>
    /// <returns>A string in the format "Stacks CurStacks/MaxStacks".</returns>
    public virtual string GetStackText() => $"Stacks {CurStacks}/{MaxStacks}";
    /// <summary>
    /// Gets the current value of a buff effect, factoring in stacks and degradation.
    /// </summary>
    /// <param name="effect">The effect to evaluate.</param>
    /// <returns>The current <see cref="BuffValue"/> for the effect.</returns>
    protected override BuffValue GetCurBuffValue(BuffEffect effect)
    {
        float f = 1f;
        if (Degrading && Duration > 0) f = TimerF;
    
        var stacks = CurStacks + 1;
    
        return new (effect.Bonus * stacks * f, effect.Flat * stacks * f);
    }
}