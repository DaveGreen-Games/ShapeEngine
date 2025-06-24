namespace ShapeEngine.Pathfinding;

/// <summary>
/// Interface for agents that can request and receive paths from a Pathfinder.
/// </summary>
public interface IPathfinderAgent
{
    /// <summary>
    /// Event triggered when the agent requests a path.
    /// </summary>
    public event Action<PathRequest> OnRequestPath;
    /// <summary>
    /// Called when the agent receives a requested path.
    /// </summary>
    /// <param name="path">The path found, or null if no path was found.</param>
    /// <param name="request">The original path request.</param>
    public void ReceiveRequestedPath(Path? path, PathRequest request);
    /// <summary>
    /// Gets the layer this agent uses for pathfinding.
    /// </summary>
    public uint GetLayer();
    /// <summary>
    /// Called when the agent is added to a Pathfinder.
    /// </summary>
    /// <param name="pathfinder">The Pathfinder instance.</param>
    public void AddedToPathfinder(Pathfinder pathfinder);
    /// <summary>
    /// Called when the agent is removed from a Pathfinder.
    /// </summary>
    public void RemovedFromPathfinder();

}