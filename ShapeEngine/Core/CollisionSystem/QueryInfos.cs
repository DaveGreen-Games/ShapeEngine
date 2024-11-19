using System.Numerics;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.CollisionSystem;

public class IntersectSpaceEntry : CollisionPoints
{
    public readonly Collider Collider;
    public readonly Vector2 OtherVel;

    public IntersectSpaceEntry(Collider collider, int capacity) : base(capacity)
    {
        Collider = collider;
        OtherVel = collider.Velocity;
    }
    public IntersectSpaceEntry(Collider collider, List<CollisionPoint> points) : base(points.Count)
    {
        Collider = collider;
        OtherVel = collider.Velocity;
        AddRange(points);
    }
    
    
}
public class IntersectSpaceRegister : List<IntersectSpaceEntry>
{
    public readonly CollisionObject Object;

    public IntersectSpaceRegister(CollisionObject colObject, int capacity) : base(capacity)
    {
        Object = colObject;
    }

    public bool AddEntry(IntersectSpaceEntry entry)
    {
        if (entry.Count <= 0) return false;
        if (entry.Collider.Parent == null || entry.Collider.Parent == Object) return false;
        Add(entry);
        return true;
    }
    
    //TODO: Add filter / query methods here to get specific IntersectSpaceEntries
}

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



public class QueryInfos : List<QueryInfo>
{
    public QueryInfos(params QueryInfo[] infos) { AddRange(infos); }
    public QueryInfos(IEnumerable<QueryInfo> infos) { AddRange(infos); }


    public void AddRange(params QueryInfo[] newInfos) { AddRange(newInfos as IEnumerable<QueryInfo>); }
    public QueryInfos Copy() { return new(this); }
    public void SortClosest(Vector2 origin)
    {
        if (Count > 1)
        {
            Sort
            (
                (a, b) =>
                {
                    if (!a.Points.Valid) return 1;
                    else if (!b.Points.Valid) return -1;
                        
                    float la = (origin - a.Points.Closest.Point).LengthSquared();
                    float lb = (origin - b.Points.Closest.Point).LengthSquared();
            
                    if (la > lb) return 1;
                    else if (MathF.Abs(la - lb) < 0.01f) return 0;
                    else return -1;
                }
            );
        }
    }
}