namespace ShapeEngine.Stats;

/// <summary>
/// Represents a buff that supports stacking and timing,
/// allowing for multiple stacks and optional duration-based degradation.
/// </summary>
/// <remarks>
/// <see cref="BuffStacked"/> extends <see cref="BuffTimed"/> to provide stack management and advanced duration handling.
/// <see cref="CurStacks"/> stores the actual active stack count.
/// </remarks>
public class BuffStacked : BuffTimed
{
    #region Public Properties
    
    /// <summary>
    /// The maximum number of stacks this buff can have.
    /// </summary>
    /// <remarks>
    /// Values below zero are treated as unlimited stacks.
    /// </remarks>
    public int MaxStacks { get; private set; }
   
    /// <summary>
    /// The current number of stacks.
    /// </summary>
    public int CurStacks { get; private set; }
    
    /// <summary>
    /// The current stack fraction (<see cref="CurStacks"/> / <see cref="MaxStacks"/>).
    /// </summary>
    /// <remarks>
    /// Returns <c>1</c> when stacks are unlimited and at least one stack is active.
    /// </remarks>
    public float StackF
    {
        get
        {
            if (MaxStacks < 0) return CurStacks > 0 ? 1f : 0f;
            if (MaxStacks == 0) return 0f;
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
    
    #endregion
    
    #region Constructors
    
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
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Creates a new stacked buff with the same identifier, stack settings, duration settings, and effects.
    /// </summary>
    /// <returns>A new <see cref="BuffStacked"/> instance with the same configuration.</returns>
    public override IBuff Clone() => new BuffStacked(Id, MaxStacks, Duration, Degrading, StacksReplenishDuration, ClearAllStacksOnDurationEnd, Effects.ToArray());
    
    /// <summary>
    /// Adds stacks to this buff, up to the configured maximum.
    /// When enabled, adding stacks also refreshes the remaining duration.
    /// </summary>
    /// <param name="amount">The number of stacks to add.</param>
    public override void AddStacks(int amount)
    {
        if (amount <= 0) return;

        CurStacks += amount;
        if (MaxStacks >= 0 && CurStacks > MaxStacks) CurStacks = MaxStacks;

        if (CurStacks > 0 && Duration > 0f && StacksReplenishDuration) Timer = Duration;
    }

    /// <summary>
    /// Removes stacks from this buff.
    /// If the stack count reaches zero, the buff is marked as finished.
    /// </summary>
    /// <param name="amount">The number of stacks to remove.</param>
    /// <returns>True if the buff should be removed; otherwise, false.</returns>
    public override bool RemoveStacks(int amount)
    {
        if (amount <= 0) return CurStacks <= 0;

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
    /// <remarks>
    /// When the duration expires, either all stacks are cleared or a single stack is consumed,
    /// depending on <see cref="ClearAllStacksOnDurationEnd"/>.
    /// </remarks>
    /// <param name="dt">The time delta since the last update.</param>
    public override void Update(float dt)
    {
        if (CurStacks <= 0)
        {
            CurStacks = 0;
            Timer = 0f;
            return;
        }

        if (Duration > 0f)
        {
            Timer -= dt;
            if (Timer <= 0f)
            {
                if (ClearAllStacksOnDurationEnd)
                {
                    CurStacks = 0;
                    Timer = 0f;
                }
                else
                {
                    CurStacks -= 1;
                    if (CurStacks > 0)
                    {
                        Timer = Duration;
                    }
                    else
                    {
                        CurStacks = 0;
                        Timer = 0f;
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Determines whether the buff is finished and should be removed.
    /// The buff is considered finished when no stacks remain.
    /// </summary>
    /// <returns>True if the buff is finished; otherwise, false.</returns>
    public override bool IsFinished() => CurStacks <= 0;
    
    /// <summary>
    /// Gets a string representing the current stack state.
    /// </summary>
    /// <returns>A string in the format <c>Stacks current/max</c>.</returns>
    public virtual string GetStackText() => $"Stacks {CurStacks}/{MaxStacks}";
    
    #endregion
    
    #region Protected Methods
    
    /// <summary>
    /// Gets the current value of a buff effect, factoring in stacks and degradation.
    /// </summary>
    /// <param name="effect">The effect to evaluate.</param>
    /// <returns>
    /// The current <see cref="BuffValue"/> for the effect after applying the active stack count
    /// and, when enabled, the remaining-time multiplier.
    /// </returns>
    protected override BuffValue GetCurBuffValue(BuffEffect effect)
    {
        if (CurStacks <= 0) return new();

        float f = 1f;
        if (Degrading && Duration > 0) f = RemainingFraction;

        var stacks = CurStacks;

        return new (effect.Bonus * stacks * f, effect.Flat * stacks * f);
    }
    
    #endregion
}