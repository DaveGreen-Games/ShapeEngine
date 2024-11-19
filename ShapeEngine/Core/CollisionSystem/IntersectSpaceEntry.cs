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
    
    
    
    
    //TODO: Add filter / query methods
    //--- Find Closest/ Furthest CollisionPoint
    //--- Get CollisionPoint facing towards collider
    //--- Validate points
    //--- Get average collision point
    //--- sort all points by distance
}