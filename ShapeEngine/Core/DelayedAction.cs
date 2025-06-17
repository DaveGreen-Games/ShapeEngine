using ShapeEngine.Timing;

namespace ShapeEngine.Core;

/// <summary>
/// Represents an action that is executed after a specified delay.
/// Implements the <see cref="ISequenceable"/> interface for sequencing.
/// </summary>
internal class DelayedAction : ISequenceable
{
    /// <summary>
    /// The action to execute after the delay.
    /// </summary>
    private readonly Action action;

    /// <summary>
    /// The remaining time before the action is executed.
    /// </summary>
    private float timer;

    /// <summary>
    /// Initializes a new instance of the <see cref="DelayedAction"/> class.
    /// If the delay is less than or equal to zero, the action is executed immediately.
    /// </summary>
    /// <param name="delay">The delay in seconds before executing the action.</param>
    /// <param name="action">The action to execute.</param>
    public DelayedAction(float delay, Action action)
    {
        if (delay <= 0)
        {
            this.timer = 0f;
            this.action = action;
            this.action();
        }
        else
        {
            this.timer = delay;
            this.action = action;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DelayedAction"/> class by copying another instance.
    /// </summary>
    /// <param name="action">The <see cref="DelayedAction"/> instance to copy.</param>
    private DelayedAction(DelayedAction action)
    {
        this.timer = action.timer;
        this.action = action.action;
    }

    /// <summary>
    /// Creates a copy of this <see cref="DelayedAction"/> instance.
    /// </summary>
    /// <returns>A new <see cref="ISequenceable"/> copy.</returns>
    public ISequenceable Copy() => new DelayedAction(this);

    /// <summary>
    /// Updates the timer and executes the action if the delay has elapsed.
    /// </summary>
    /// <param name="dt">The time delta to subtract from the timer.</param>
    /// <returns>
    /// True if the action has been executed and the sequence is complete; otherwise, false.
    /// </returns>
    public bool Update(float dt)
    {
        if (timer <= 0f) return true;
        
        timer -= dt;
        if (timer > 0f) return false;
        action.Invoke();
        return true;
    }
}