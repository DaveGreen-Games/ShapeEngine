namespace ShapeEngine.Timing;

/// <summary>
/// Represents a timed float value that can be used in a sequence.
/// Implements <see cref="ISequenceableTimedFloat"/> to allow for value application and timed updates.
/// </summary>
public class TimedFloat : ISequenceableTimedFloat
{
    /// <summary>
    /// The remaining time for this timed float.
    /// </summary>
    private float timer = 0f;

    /// <summary>
    /// The value to be applied to the total.
    /// </summary>
    private float value = 0f;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimedFloat"/> class with a specified duration and value.
    /// </summary>
    /// <param name="duration">The duration for which this value is active.</param>
    /// <param name="value">The value to be applied.</param>
    public TimedFloat(float duration, float value)
    {
        this.timer = duration;
        this.value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TimedFloat"/> class by copying another instance.
    /// </summary>
    /// <param name="timed">The <see cref="TimedFloat"/> instance to copy.</param>
    public TimedFloat(TimedFloat timed)
    {
        this.timer = timed.timer;
        this.value = timed.value;
    }

    /// <summary>
    /// Creates a copy of this <see cref="TimedFloat"/> instance.
    /// </summary>
    /// <returns>A new <see cref="TimedFloat"/> instance with the same values.</returns>
    public ISequenceable Copy() => new TimedFloat(this);

    /// <summary>
    /// Applies the stored value to the given total.
    /// </summary>
    /// <param name="total">The current total value.</param>
    /// <returns>The result of multiplying the total by this instance's value.</returns>
    public float ApplyValue(float total) { return total * value; }

    /// <summary>
    /// Updates the timer by subtracting the given delta time.
    /// </summary>
    /// <param name="dt">The delta time to subtract from the timer.</param>
    /// <returns>True if the timer has expired (is less than or equal to zero), otherwise false.</returns>
    public bool Update(float dt)
    {
        if (timer <= 0f) return true;
        timer -= dt;
        return timer <= 0f;
    }
}