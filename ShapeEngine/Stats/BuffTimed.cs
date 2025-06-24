namespace ShapeEngine.Stats;

/// <summary>
/// Represents a timed buff that can degrade over its duration.
/// </summary>
/// <remarks>
/// BuffTimed extends Buff to provide duration and optional degrading behavior.
/// </remarks>
public class BuffTimed : Buff
{
    /// <summary>
    /// The total duration of the buff in seconds.
    /// </summary>
    public float Duration { get; private set; }
    /// <summary>
    /// The current timer value in seconds.
    /// </summary>
    public float Timer { get; protected set; }
    /// <summary>
    /// The fraction of time remaining
    /// <code>1f - (Timer / Duration) </code>
    /// </summary>
    public float TimerF
    {
        get
        {
            if (Duration <= 0f) return 0f;
            return 1f - (Timer / Duration);
        }
    }
    /// <summary>
    /// If true, the buff's effect degrades over time.
    /// </summary>
    public bool Degrading;

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

    /// <summary>
    /// Creates a copy of this timed buff.
    /// </summary>
    /// <returns>A new <see cref="BuffTimed"/> instance with the same properties.</returns>
    public override IBuff Clone() => new BuffTimed(Id, Duration, Degrading, Effects.ToArray());

    /// <summary>
    /// Adds stacks to this buff. Resets the timer if duration is set.
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
        }
    }
    /// <summary>
    /// Determines whether the buff is finished and should be removed.
    /// The buff is considered finished if its duration is greater than zero and the timer has reached zero or below.
    /// </summary>
    /// <returns>True if the buff is finished; otherwise, false.</returns>
    public override bool IsFinished() => Duration > 0f && Timer <= 0f;
    
    /// <summary>
    /// Gets the current value of a buff effect, factoring in degradation if enabled.
    /// </summary>
    /// <param name="effect">The effect to evaluate.</param>
    /// <returns>The current <see cref="BuffValue"/> for the effect.</returns>
    protected override BuffValue GetCurBuffValue(BuffEffect effect)
    {
        float f = 1f;
        if (Degrading && Duration > 0) f = TimerF;
    
        return new (effect.Bonus * f, effect.Flat * f);
    }
}