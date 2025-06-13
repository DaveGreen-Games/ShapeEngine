namespace ShapeEngine.Core;

/// <summary>
/// Represents a counter for generating unique IDs, with support for advancing and resetting.
/// </summary>
public class IdCounter
{
    /// <summary>
    /// The value representing an invalid ID (always 0).
    /// </summary>
    public static readonly uint InvalidId = 0;
    /// <summary>
    /// The current counter value (starts at 10).
    /// </summary>
    private uint count = 10;
    /// <summary>
    /// Gets the next available unique ID and increments the counter.
    /// </summary>
    public uint NextId => count++;
    /// <summary>
    /// Advances the counter to at least the specified ID value.
    /// </summary>
    /// <param name="id">The ID to advance to (if greater than the current counter).</param>
    public void AdvanceTo(uint id)
    {
        if(id >= count) count = id + 1;
    }
    /// <summary>
    /// Resets the counter to its initial value (10).
    /// </summary>
    public void Reset() => count = 10;
}