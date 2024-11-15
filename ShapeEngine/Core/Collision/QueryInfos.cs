using System.Numerics;

namespace ShapeEngine.Core.CollisionSystem;

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