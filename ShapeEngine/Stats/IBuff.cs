namespace ShapeEngine.Stats;

/// <summary>
/// Defines the interface for a buff that can be applied to a stat.
/// </summary>
public interface IBuff
{
    /// <summary>
    /// Gets the unique identifier for this buff.
    /// </summary>
    /// <returns>The buff's unique ID.</returns>
    public uint GetId();
    /// <summary>
    /// Adds stacks to this buff.
    /// </summary>
    /// <param name="amount">The number of stacks to add.</param>
    public void AddStacks(int amount);
    /// <summary>
    /// Removes stacks from this buff.
    /// </summary>
    /// <param name="amount">The number of stacks to remove.</param>
    /// <returns>True if the buff should be removed; otherwise, false.</returns>
    public bool RemoveStacks(int amount);
    /// <summary>
    /// Applies this buff's effects to the specified stat.
    /// </summary>
    /// <param name="stat">The stat to apply effects to.</param>
    public void ApplyTo(IStat stat);
    /// <summary>
    /// Updates the buff's state.
    /// </summary>
    /// <param name="dt">The time delta since the last update.</param>
    public void Update(float dt);
    /// <summary>
    /// Determines whether the buff is finished and should be removed.
    /// </summary>
    /// <returns>True if the buff is finished; otherwise, false.</returns>
    public bool IsFinished();
    /// <summary>
    /// Creates a copy of this buff.
    /// </summary>
    /// <returns>A new <see cref="IBuff"/> instance with the same properties.</returns>
    public IBuff Clone();
}