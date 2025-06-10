using System.Numerics;

namespace ShapeEngine.Pathfinding;

/// <summary>
/// Represents a request for a path from an agent, including start/end positions and priority.
/// </summary>
public class PathRequest
{
    /// <summary>
    /// The agent making the path request.
    /// </summary>
    public readonly IPathfinderAgent? Agent;
    /// <summary>
    /// The starting position for the path request.
    /// </summary>
    public readonly Vector2 Start;
    /// <summary>
    /// The ending position for the path request.
    /// </summary>
    public readonly Vector2 End;
    /// <summary>
    /// The priority of the request. Higher values are handled sooner.
    /// </summary>
    public readonly int Priority;
    /// <summary>
    /// Gets whether the request is valid (agent is not null).
    /// </summary>
    public bool Valid => Agent != null;
    /// <summary>
    /// Initializes a new default instance of <see cref="PathRequest"/>.
    /// </summary>
    public PathRequest()
    {
        Agent = null;
        Start = new();
        End = new();
        Priority = 0;
    }
    /// <summary>
    /// Initializes a new instance of <see cref="PathRequest"/> with agent, start, end, and priority.
    /// </summary>
    /// <param name="agent">The requesting agent.</param>
    /// <param name="start">The start position.</param>
    /// <param name="end">The end position.</param>
    /// <param name="priority">The request priority.</param>
    public PathRequest(IPathfinderAgent agent, Vector2 start, Vector2 end, int priority)
    {
        this.Agent = agent;
        this.Start = start;
        this.End = end;
        this.Priority = priority;
    }
}