using ShapeEngine.Core;

namespace ShapeEngine.StaticLib;

/// <summary>
/// Provides static methods for generating and managing unique IDs within ShapeEngine.
/// </summary>
public static class ShapeID
{
    /// <summary>
    /// Internal counter for generating unique IDs.
    /// </summary>
    private static IdCounter counter = new();
    /// <summary>
    /// Gets the value representing an invalid ID (always 0).
    /// </summary>
    public static uint InvalidId => IdCounter.InvalidId;
    /// <summary>
    /// Gets the next available unique ID.
    /// </summary>
    public static uint NextID => counter.NextId;
    /// <summary>
    /// Advances the internal counter to at least the specified ID value.
    /// </summary>
    /// <param name="id">The ID to advance to (if greater than the current counter).</param>
    public static void AdvanceTo(uint id) => counter.AdvanceTo(id);
    /// <summary>
    /// Resets the internal ID counter to its initial value (10).
    /// </summary>
    public static void Reset() => counter.Reset();
}