namespace ShapeEngine.Stats;

/// <summary>
/// Represents a buff with an optional duration that can degrade over time.
/// </summary>
/// <remarks>
/// <see cref="BuffTimed"/> extends <see cref="Buff"/> to provide duration tracking and optional fade-out behavior.
/// A duration less than or equal to zero means the buff does not expire automatically.
/// </remarks>
public class BuffTimed : Buff
{
    #region Public Properties
    
    /// <summary>
    /// The total duration of the buff in seconds.
    /// </summary>
    public float Duration { get; private set; }
    
    /// <summary>
    /// The remaining lifetime of the buff in seconds.
    /// </summary>
    public float Timer { get; protected set; }
 
    /// <summary>
    /// The fraction of time remaining.
    /// <code>Timer / Duration</code>
    /// </summary>
    /// <remarks>
    /// This value starts at <c>1</c> when the buff begins and approaches <c>0</c> as the timer reaches zero.
    /// </remarks>
    public float RemainingFraction
    {
        get
        {
            if (Duration <= 0f) return 0f;

            var remainingFraction = Timer / Duration;
            if (remainingFraction < 0f) return 0f;
            if (remainingFraction > 1f) return 1f;
            return remainingFraction;
        }
    }
    
    /// <summary>
    /// The fraction of time elapsed.
    /// <code>1f - RemainingFraction</code>
    /// </summary>
    /// <remarks>
    /// This value starts at <c>0</c> when the buff begins and approaches <c>1</c> as the timer reaches zero.
    /// Use <see cref="RemainingFraction"/> when buff strength should fade out over time,
    /// and use <see cref="ElapsedFraction"/> when progress or charge-up behavior is needed.
    /// </remarks>
    public float ElapsedFraction => 1f - RemainingFraction;
    
    /// <summary>
    /// If true, the buff's effect degrades over time.
    /// </summary>
    /// <remarks>
    /// Degrading buffs scale their effect by <see cref="RemainingFraction"/>, so they start at full strength and fade out.
    /// </remarks>
    public bool Degrading;

    #endregion

    #region Constructors
    
    /// <summary>
    /// Initializes a new instance of the <see cref="BuffTimed"/> class with duration and degrading options.
    /// </summary>
    /// <param name="id">The unique identifier for the buff.</param>
    /// <param name="duration">The total duration of the buff in seconds.</param>
    /// <param name="degrading">If true, the buff's effect degrades over time.</param>
    public BuffTimed(uint id, float duration, bool degrading) : base(id)
    {
        Duration = duration;
        if (this.Duration > 0f) Timer = this.Duration;
        else Timer = 0f;
        Degrading = degrading;
    }
  
    /// <summary>
    /// Initializes a new instance with effects.
    /// </summary>
    /// <param name="id">The unique identifier for the buff.</param>
    /// <param name="duration">The total duration of the buff in seconds.</param>
    /// <param name="degrading">If true, the buff's effect degrades over time.</param>
    /// <param name="effects">The effects to apply.</param>
    public BuffTimed(uint id, float duration, bool degrading, params BuffEffect[] effects) : base(id, effects)
    {
        Duration = duration;
        if (this.Duration > 0f) Timer = this.Duration;
        else Timer = 0f;
        Degrading = degrading;
    }

    #endregion

    #region Public Methods
    
    /// <summary>
    /// Creates a new timed buff with the same identifier, duration, degradation setting, and effects.
    /// </summary>
    /// <returns>A new <see cref="BuffTimed"/> instance with the same configuration.</returns>
    public override IBuff Clone() => new BuffTimed(Id, Duration, Degrading, Effects.ToArray());

    /// <summary>
    /// Adds stacks to this buff.
    /// For timed buffs, this refreshes the remaining duration when the buff has a positive duration.
    /// </summary>
    /// <param name="amount">The number of stacks to add.</param>
    public override void AddStacks(int amount)
    {
        if (Duration > 0) Timer = Duration;
    }
   
    /// <summary>
    /// Updates the timer for this buff.
    /// </summary>
    /// <param name="dt">The time delta since the last update.</param>
    public override void Update(float dt)
    {
        if (Duration > 0f)
        {
            Timer -= dt;
            if (Timer < 0f) Timer = 0f;
        }
    }
 
    /// <summary>
    /// Determines whether the buff is finished and should be removed.
    /// The buff is considered finished when it has a positive duration and the timer has reached zero.
    /// </summary>
    /// <returns>True if the buff is finished; otherwise, false.</returns>
    public override bool IsFinished() => Duration > 0f && Timer <= 0f;
    
    #endregion
    
    #region Protected Methods
    
    /// <summary>
    /// Gets the current value of a buff effect, factoring in degradation if enabled.
    /// </summary>
    /// <param name="effect">The effect to evaluate.</param>
    /// <returns>The current <see cref="BuffValue"/> for the effect after applying the remaining-time multiplier when degrading.</returns>
    protected override BuffValue GetCurBuffValue(BuffEffect effect)
    {
        float f = 1f;
        if (Degrading && Duration > 0) f = RemainingFraction;
    
        return new (effect.Bonus * f, effect.Flat * f);
    }
    
    #endregion
}