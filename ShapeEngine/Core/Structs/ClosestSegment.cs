using System.Numerics;
using ShapeEngine.Core.Shapes;
namespace ShapeEngine.Core.Structs;

public readonly struct ClosestSegment
{
    public readonly Segment Segment;
    public readonly ClosestPoint Point;
    public readonly bool Valid => Point.Valid;
    public ClosestSegment()
    {
        Segment = new();
        Point = new();
    }

    public ClosestSegment(Segment segment, CollisionPoint point, float distance)
    {
        Segment = segment;
        Point = new(point, distance);
    }
    public ClosestSegment(Segment segment, Vector2 point, Vector2 normal, float distance)
    {
        Segment = segment;
        Point = new(point, normal, distance);
            
    }
    public ClosestSegment(Segment segment, Vector2 point, float distance)
    {
        Segment = segment;
        Point = new(point, segment.Normal, distance);
    }
}