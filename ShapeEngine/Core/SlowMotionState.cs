namespace ShapeEngine.Core;

/// <summary>
/// Represents the state of all active slow motion effects at a given moment.
/// </summary>
/// <remarks>
/// Used to save and restore the set of slow motion effects, for example when pausing or temporarily disabling slow motion.
/// </remarks>
public sealed class SlowMotionState
{
    /// <summary>
    /// The list of slow motion effect containers (grouped by tag) in this state.
    /// </summary>
    internal readonly List<SlowMotion.Container> Containers;
    /// <summary>
    /// Initializes a new instance of the <see cref="SlowMotionState"/> class with the specified containers.
    /// </summary>
    /// <param name="containers">The collection of slow motion containers to include in this state.</param>
    internal SlowMotionState(IEnumerable<SlowMotion.Container> containers)
    {
        Containers = containers.ToList();
        
    }
}