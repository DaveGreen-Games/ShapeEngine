namespace ShapeEngine.Pathfinding;

public interface IPathfinderAgent
{
    public event Action<PathRequest> OnRequestPath;

    public void ReceiveRequestedPath(Path? path, PathRequest request);
    public uint GetLayer();
    public void AddedToPathfinder(Pathfinder pathfinder);
    public void RemovedFromPathfinder();

}