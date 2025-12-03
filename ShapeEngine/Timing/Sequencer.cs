
namespace ShapeEngine.Timing;

internal static class SequencerIdGenerator
{
    /// <summary>
    /// Static counter for generating unique sequence IDs.
    /// </summary>
    private static uint idCounter;

    /// <summary>
    /// Gets the next unique sequence ID.
    /// </summary>
    internal static uint NextID => idCounter++;
}

/// <summary>
/// Manages and updates sequences of items implementing <see cref="ISequenceable"/>.
/// Supports starting, updating, cancelling, and restoring sequences.
/// </summary>
/// <typeparam name="T">Type of sequence item, must implement <see cref="ISequenceable"/>.</typeparam>
public class Sequencer<T> where T : ISequenceable
{

    /// <summary>
    /// Event triggered when a sequence finishes.
    /// </summary>
    public event Action<uint>? OnSequenceFinished;

    /// <summary>
    /// Event triggered when a sequence item is updated.
    /// </summary>
    public event Action<T>? OnItemUpdated;

    /// <summary>
    /// Stores the active sequences.
    /// </summary>
    private Sequences<T> sequences = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Sequencer{T}"/> class.
    /// </summary>
    public Sequencer() { }

    /// <summary>
    /// Gets the current state of the sequencer.
    /// </summary>
    /// <returns>A <see cref="SequencerState{T}"/> representing the current state.</returns>
    public SequencerState<T> GetState() => new(sequences);

    /// <summary>
    /// Clears all sequences and returns the previous state.
    /// </summary>
    /// <returns>The state before clearing.</returns>
    public SequencerState<T> Clear()
    {
        var state = new SequencerState<T>(sequences);
        sequences.Clear();
        return state;
    }

    /// <summary>
    /// Applies a previously saved sequencer state.
    /// </summary>
    /// <param name="state">The state to apply.</param>
    public void ApplyState(SequencerState<T> state)
    {
        sequences = state.state;
    }
    
    /// <summary>
    /// Checks if there are any active sequences.
    /// </summary>
    /// <returns>True if there are sequences, otherwise false.</returns>
    public bool HasSequences() => sequences.Count > 0;

    /// <summary>
    /// Checks if a sequence with the given ID exists.
    /// </summary>
    /// <param name="id">The sequence ID.</param>
    /// <returns>True if the sequence exists, otherwise false.</returns>
    public bool HasSequence(uint id) => sequences.ContainsKey(id);

    /// <summary>
    /// Starts a new sequence with the provided items.
    /// </summary>
    /// <param name="items">The sequence items.</param>
    /// <returns>The unique ID of the new sequence.</returns>
    public uint StartSequence(params T[] items)
    {
        var id = SequencerIdGenerator.NextID;
        var list = new List<T>(items);
        list.Reverse();
        sequences.Add(id, list);
        return id;
    }

    /// <summary>
    /// Cancels the sequence with the specified ID.
    /// </summary>
    /// <param name="id">The sequence ID.</param>
    public void CancelSequence(uint id)
    {
        if (sequences.ContainsKey(id)) sequences.Remove(id);
    }

    /// <summary>
    /// Stops all sequences.
    /// </summary>
    public void Stop() => sequences.Clear();

    /// <summary>
    /// Updates all active sequences.
    /// </summary>
    /// <param name="dt">Delta time since last update.</param>
    public void Update(float dt)
    {
        StartUpdate();
        List<uint> remove = new();
        foreach (uint id in sequences.Keys)
        {
            var sequenceList = sequences[id];
            if (sequenceList.Count > 0)
            {
                var sequence = sequenceList[sequenceList.Count - 1];//list is reversed
                var finished = UpdateSequence(sequence, dt);
                OnItemUpdated?.Invoke(sequence);
                if (finished) sequenceList.RemoveAt(sequenceList.Count - 1);
            }
            else
            {
                remove.Add(id);
                OnSequenceFinished?.Invoke(id);
            }
        }

        foreach (uint id in remove) sequences.Remove(id);
        EndUpdate();
    }
    
    /// <summary>
    /// Updates a single sequence item. Can be overridden for custom behavior.
    /// </summary>
    /// <param name="sequence">The sequence item.</param>
    /// <param name="dt">Delta time.</param>
    /// <returns>True if the sequence item is finished, otherwise false.</returns>
    protected virtual bool UpdateSequence(T sequence, float dt) { return sequence.Update(dt); }

    /// <summary>
    /// Called before updating all sequences. Can be overridden for custom behavior.
    /// </summary>
    protected virtual void StartUpdate() { }

    /// <summary>
    /// Called after updating all sequences. Can be overridden for custom behavior.
    /// </summary>
    protected virtual void EndUpdate() { }
}


    
