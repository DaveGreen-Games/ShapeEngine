namespace ShapeEngine.Stats;

/// <summary>
/// Represents a modifier source that expires after a duration.
/// </summary>
public class TimedStatModifierSource : StatModifierSource
{
    /// <inheritdoc />
    public override float Duration { get; }

    /// <inheritdoc />
    public override float RemainingDuration { get; protected set; }

    /// <summary>
    /// The fraction of duration remaining from 0 to 1.
    /// </summary>
    public float RemainingFraction
    {
        get
        {
            if (Duration <= 0f) return 0f;
            return Math.Clamp(RemainingDuration / Duration, 0f, 1f);
        }
    }

    /// <inheritdoc />
    public override bool IsExpired => Duration > 0f && RemainingDuration <= 0f;

    /// <summary>
    /// Creates a new timed modifier source.
    /// </summary>
    /// <param name="id">The stable source id.</param>
    /// <param name="sourceType">The gameplay category of the source.</param>
    /// <param name="duration">The duration in seconds.</param>
    /// <param name="modifiers">The modifiers contributed by the source.</param>
    /// <param name="name">The display name.</param>
    /// <param name="description">The display description.</param>
    public TimedStatModifierSource(
        uint id,
        StatModifierSourceType sourceType,
        float duration,
        IEnumerable<StatModifier> modifiers,
        string name = "",
        string description = "") : base(id, sourceType, modifiers, name, description)
    {
        Duration = duration;
        RemainingDuration = duration > 0f ? duration : 0f;
    }

    /// <summary>
    /// Creates a new timed modifier source.
    /// </summary>
    /// <param name="id">The stable source id.</param>
    /// <param name="sourceType">The gameplay category of the source.</param>
    /// <param name="duration">The duration in seconds.</param>
    /// <param name="name">The display name.</param>
    /// <param name="description">The display description.</param>
    /// <param name="modifiers">The modifiers contributed by the source.</param>
    public TimedStatModifierSource(
        uint id,
        StatModifierSourceType sourceType,
        float duration,
        string name = "",
        string description = "",
        params StatModifier[] modifiers) : this(id, sourceType, duration, modifiers.AsEnumerable(), name, description)
    {
    }

    /// <inheritdoc />
    public override void Update(float dt)
    {
        if (Duration <= 0f || RemainingDuration <= 0f) return;
        RemainingDuration -= dt;
        if (RemainingDuration < 0f) RemainingDuration = 0f;
    }

    /// <inheritdoc />
    public override void Reapply(IStatModifierSource incoming)
    {
        if (Duration > 0f) RemainingDuration = Duration;
    }
}
