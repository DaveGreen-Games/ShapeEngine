namespace ShapeEngine.Timing;

/// <summary>
/// Represents the state of a sequencer for a specific type implementing <see cref="ISequenceable"/>.
/// Holds a copy of the provided sequences.
/// </summary>
/// <typeparam name="T">The type of sequenceable object.</typeparam>
public sealed class SequencerState<T> where T : ISequenceable
{
    internal readonly Sequences<T> state = new();

    internal SequencerState(Sequences<T> sequences)
    {
        state = sequences.Copy();
    }
}