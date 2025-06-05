

namespace ShapeEngine.Timing;

/// <summary>
/// Represents a sequenceable timer that repeats an action for a specified number of times,
/// with a customizable duration for each repeat cycle.
/// </summary>
public class Repeater : ISequenceable
{
    /// <summary>
    /// Delegate that is called after each duration for every repeat.
    /// Takes the specified duration and returns the duration for the next cycle.
    /// </summary>
    /// <param name="duration">The current duration to be modified.</param>
    /// <returns>The duration for the next cycle.</returns>
    public delegate float RepeaterFunc(float duration);

    /// <summary>
    /// The function used to determine the duration for each repeat cycle.
    /// </summary>
    private readonly RepeaterFunc repeaterFunc;

    /// <summary>
    /// The current timer value, representing elapsed time in the current cycle.
    /// </summary>
    private float timer;

    /// <summary>
    /// The duration of the current repeat cycle.
    /// </summary>
    private float duration;

    /// <summary>
    /// The number of remaining repeats. If zero, the sequence is complete.
    /// </summary>
    private int remainingRepeats;

    /// <summary>
    /// Initializes a new instance of the <see cref="Repeater"/> class.
    /// </summary>
    /// <param name="repeaterFunc">The function to determine the next duration after each repeat.</param>
    /// <param name="duration">The initial duration for the repeat cycle.</param>
    /// <param name="repeats">The number of times to repeat the cycle. Default is 0.</param>
    public Repeater(RepeaterFunc repeaterFunc, float duration, int repeats = 0)
    {
        this.repeaterFunc = repeaterFunc;
        this.duration = duration;
        this.timer = 0f;
        this.remainingRepeats = repeats;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Repeater"/> class by copying another instance.
    /// </summary>
    /// <param name="repeater">The <see cref="Repeater"/> instance to copy.</param>
    public Repeater(Repeater repeater)
    {
        this.repeaterFunc = repeater.repeaterFunc;
        this.duration = repeater.duration;
        this.timer = repeater.duration;
        this.remainingRepeats = repeater.remainingRepeats;
    }

    /// <summary>
    /// Creates a copy of the current <see cref="Repeater"/> instance.
    /// </summary>
    /// <returns>A new <see cref="Repeater"/> instance with the same configuration.</returns>
    public ISequenceable Copy() => new Repeater(this);

    /// <summary>
    /// Updates the repeater's timer by the specified delta time.
    /// If the timer exceeds the duration, the repeat function is called and the cycle is reset.
    /// </summary>
    /// <param name="dt">The time elapsed since the last update, in seconds.</param>
    /// <returns>
    /// True if the sequence has completed (no remaining repeats and timer exceeds duration); otherwise, false.
    /// </returns>
    public bool Update(float dt)
    {
        if (duration <= 0f) return true;

        timer += dt;
        if (timer >= duration)
        {
            float dur = repeaterFunc(duration);
            if (remainingRepeats > 0)
            {
                timer = 0f; // timer - duration; //in case timer over shot
                duration = dur;
                remainingRepeats--;
            }
        }
        return timer >= duration && remainingRepeats <= 0;
    }
}


