namespace ShapeEngine.Core.Structs;

public readonly struct ClosestPointResult
{
    public readonly CollisionPoint Self;
    public readonly CollisionPoint Other;
        
    public readonly float DistanceSquared;
        
    public readonly int SegmentIndex;
    public readonly int OtherSegmentIndex;
        
    public bool Valid => DistanceSquared >= 0;

    public ClosestPointResult()
    {
        Self = new();
        Other = new();
        DistanceSquared = -1;
        SegmentIndex = -1;
        OtherSegmentIndex = -1;
    }
    public ClosestPointResult(CollisionPoint self, CollisionPoint other, float distanceSquared, int segmentIndex = -1, int otherSegmentIndex = -1)
    {
        Self = self;
        Other = other;
        DistanceSquared = distanceSquared;
        SegmentIndex = segmentIndex;
        OtherSegmentIndex = otherSegmentIndex;
    }
}