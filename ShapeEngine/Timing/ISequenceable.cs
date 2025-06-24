namespace ShapeEngine.Timing;

/// <summary>
/// Represents an object that can be updated in a sequence and can create a copy of itself.
/// </summary>
public interface ISequenceable
{
    /// <summary>
    /// Updates the object with the given delta time.
    /// </summary>
    /// <param name="dt">The time elapsed since the last update, in seconds.</param>
    /// <returns>True if the sequence has completed; otherwise, false.</returns>
    public bool Update(float dt);

    /// <summary>
    /// Creates a copy of the current object.
    /// </summary>
    /// <returns>A new instance that is a copy of this object.</returns>
    public ISequenceable Copy();
}