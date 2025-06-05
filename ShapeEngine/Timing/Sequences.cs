namespace ShapeEngine.Timing;

/// <summary>
/// Represents a collection of sequences, where each sequence is a list of <typeparamref name="T"/> items
/// associated with a unique <c>uint</c> key. <typeparamref name="T"/> must implement <see cref="ISequenceable"/>.
/// </summary>
internal sealed class Sequences<T> : Dictionary<uint, List<T>> where T : ISequenceable
{
    /// <summary>
    /// Creates a deep copy of the current <see cref="Sequences{T}"/> instance,
    /// duplicating all sequences and their items.
    /// </summary>
    /// <returns>
    /// A new <see cref="Sequences{T}"/> object containing copies of all sequences and their items.
    /// </returns>
    public Sequences<T> Copy()
    {
        Sequences<T> returnValue = new();
        foreach (uint key in Keys)
        {
            var copy = new List<T>();
            foreach (var item in this[key])
            {
                copy.Add((T)item.Copy());
            }
            if(!returnValue.TryAdd(key, copy)) returnValue[key].AddRange(copy);
        }

        return returnValue;
    }
}