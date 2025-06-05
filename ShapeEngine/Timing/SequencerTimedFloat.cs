

namespace ShapeEngine.Timing;

/// <summary>
/// A sequencer for types implementing <see cref="ISequenceableTimedFloat"/>.
/// Accumulates and applies timed float values in sequence, updating the total accordingly.
/// </summary>
/// <typeparam name="T">
/// The type of sequenceable timed float, constrained to <see cref="ISequenceableTimedFloat"/>.
/// </typeparam>
public class SequencerTimedFloat<T> : Sequencer<T> where T : ISequenceableTimedFloat
{
    /// <summary>
    /// Gets the total accumulated value after the sequence update.
    /// </summary>
    public float Total { get; protected set; } = 1f;

    /// <summary>
    /// The running accumulated value during the update cycle.
    /// </summary>
    private float accumulated = 1f;

    /// <summary>
    /// Called at the start of the update cycle to reset the accumulator.
    /// </summary>
    protected override void StartUpdate()
    {
        accumulated = 1f;
    }

    /// <summary>
    /// Called at the end of the update cycle to set the total to the accumulated value.
    /// </summary>
    protected override void EndUpdate()
    {
        Total = accumulated;
    }

    /// <summary>
    /// Updates the sequence by applying the current sequence value to the accumulator.
    /// </summary>
    /// <param name="sequence">The sequenceable timed float to apply.</param>
    /// <param name="dt">The delta time for the update.</param>
    /// <returns>
    /// True if the sequence update is complete, as determined by the base implementation; otherwise, false.
    /// </returns>
    protected override bool UpdateSequence(T sequence, float dt)
    {
        accumulated = sequence.ApplyValue(accumulated);
        return base.UpdateSequence(sequence, dt);
    }
}


