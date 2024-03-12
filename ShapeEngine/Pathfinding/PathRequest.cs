using System.Numerics;

namespace ShapeEngine.Pathfinding;


//TODO
//RectPath & NodePath 
//-> NodePath with NodePathType {Point, Rect} & GetPoint, GetRect function?
//-> INodePath interface with NodePathType {Point, Rect} & GetPoint, GetRect function? 
//how to combine both?


public class PathRequest // : IEquatable<PathRequest>, IEqualityComparer<PathRequest>
{
    public readonly IPathfinderAgent? Agent;
    public readonly Vector2 Start;
    public readonly Vector2 End;
    /// <summary>
    /// The higher the priority the sooner path requests are handled.
    /// </summary>
    public readonly int Priority;

    public bool Valid => Agent != null;
    public PathRequest()
    {
        Agent = null;
        Start = new();
        End = new();
        Priority = 0;
    }
    public PathRequest(IPathfinderAgent agent, Vector2 start, Vector2 end, int priority)
    {
        this.Agent = agent;
        this.Start = start;
        this.End = end;
        this.Priority = priority;
    }

    // public bool Equals(PathRequest other)
    // {
    //     if (Agent == null || other.Agent == null) return false;
    //     return Agent == other.Agent;
    // }
    //
    // public bool Equals(PathRequest x, PathRequest y)
    // {
    //     return x.Equals(y);
    // }
    //
    // public int GetHashCode(PathRequest obj)
    // {
    //     return HashCode.Combine(Agent, Start, End, Priority);
    // }
}