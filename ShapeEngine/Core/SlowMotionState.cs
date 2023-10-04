namespace ShapeEngine.Core;

public sealed class SlowMotionState
{
    internal readonly List<SlowMotion.Container> Containers;
    internal SlowMotionState(IEnumerable<SlowMotion.Container> containers)
    {
        Containers = containers.ToList();
        
    }
}