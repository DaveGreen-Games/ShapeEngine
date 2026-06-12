namespace ShapeEngine.Stats;

/// <summary>
/// Defines an object that contributes stat modifiers.
/// </summary>
public interface IStatModifierSource
{
    /// <summary>
    /// The stable source id. Adding a source with the same id replaces or reapplies the existing source.
    /// </summary>
    public uint Id { get; }

    /// <summary>
    /// The gameplay category of the source.
    /// </summary>
    public StatModifierSourceType SourceType { get; }

    /// <summary>
    /// The display name of the source.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The display description of the source.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// The current number of stacks supplied by this source.
    /// </summary>
    public int Stacks { get; }

    /// <summary>
    /// The maximum number of stacks, or a value below zero for unlimited stacks.
    /// </summary>
    public int MaxStacks { get; }

    /// <summary>
    /// The configured duration in seconds. Values less than or equal to zero do not expire automatically.
    /// </summary>
    public float Duration { get; }

    /// <summary>
    /// The remaining duration in seconds.
    /// </summary>
    public float RemainingDuration { get; }

    /// <summary>
    /// True if this source has expired and should be removed.
    /// </summary>
    public bool IsExpired { get; }

    /// <summary>
    /// Enumerates the currently active modifiers produced by this source.
    /// </summary>
    /// <returns>The active stat modifiers.</returns>
    public IEnumerable<StatModifier> GetModifiers();

    /// <summary>
    /// Advances any runtime state for this source.
    /// </summary>
    /// <param name="dt">The elapsed time in seconds.</param>
    public void Update(float dt);

    /// <summary>
    /// Called when a matching source is added again.
    /// </summary>
    /// <param name="incoming">The incoming source.</param>
    public void Reapply(IStatModifierSource incoming);

    /// <summary>
    /// Adds stacks to the source.
    /// </summary>
    /// <param name="amount">The number of stacks to add.</param>
    /// <returns>The number of stacks actually added.</returns>
    public int AddStacks(int amount);

    /// <summary>
    /// Removes stacks from the source.
    /// </summary>
    /// <param name="amount">The number of stacks to remove.</param>
    /// <returns>The number of stacks actually removed.</returns>
    public int RemoveStacks(int amount);
}
