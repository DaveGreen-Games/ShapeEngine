using System.Numerics;

namespace ShapeEngine.Core.CollisionSystem;

public class IntersectSpaceResult : List<IntersectSpaceRegister>
{
    public readonly Vector2 Origin;
    
    public IntersectSpaceResult(Vector2 origin, int capacity) : base(capacity)
    {
        Origin = origin;
    }

    public bool AddRegister(IntersectSpaceRegister reg)
    {
        if (reg.Count <= 0) return false;
        Add(reg);
        return true;
    }
    
    //TODO: Add filter / query methods here to get specific IntersectSpaceResults
}