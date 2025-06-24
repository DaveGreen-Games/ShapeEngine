namespace ShapeEngine.Stats;

/// <summary>
/// Defines the interface for a stat that can be affected by buffs.
/// </summary>
public interface IStat
{
    /// <summary>
    /// Resets the stat to its base value, removing all applied buffs.
    /// </summary>
    public void Reset();
    /// <summary>
    /// Determines whether this stat is affected by a given tag.
    /// </summary>
    /// <param name="tag">The tag to check for effect applicability.</param>
    /// <returns>True if the stat is affected by the tag; otherwise, false.</returns>
    public bool IsAffected(uint tag);
    /// <summary>
    /// Applies a buff value to this stat.
    /// </summary>
    /// <param name="buffValue">The buff value to apply.</param>
    public void Apply(BuffValue buffValue);
}