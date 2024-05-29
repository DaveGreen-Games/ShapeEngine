using System.Numerics;
using ShapeEngine.Core.Shapes;
namespace ShapeEngine.Core.Structs;

public readonly struct ClosestSegment
{
    public readonly Segment Segment;
    public readonly ClosestDistance ClosestDistance;
    public readonly bool Valid => ClosestDistance.Valid;
    public ClosestSegment()
    {
        Segment = new();
        ClosestDistance = new();
    }

    public ClosestSegment(Segment segment, ClosestDistance closestDistance)
    {
        Segment = segment;
        ClosestDistance = closestDistance;
    }
    public ClosestSegment(Segment segment, Vector2 segmentPoint, Vector2 point)
    {
        Segment = segment;
        ClosestDistance = new(segmentPoint, point);

    }
}