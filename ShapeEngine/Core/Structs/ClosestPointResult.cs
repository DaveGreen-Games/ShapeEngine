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
    
    
    public ClosestPointResult Switch() => new(Other, Self, DistanceSquared, OtherSegmentIndex, SegmentIndex);
    public bool IsCloser(ClosestPointResult other) => DistanceSquared < other.DistanceSquared;
    public bool IsCloser(float distanceSquared) => DistanceSquared < distanceSquared;
    
    public ClosestPointResult SetSegmentIndex(int index) => new(Self, Other, DistanceSquared, index, OtherSegmentIndex);
    public ClosestPointResult SetOtherSegmentIndex(int index) => new(Self, Other, DistanceSquared, SegmentIndex, index);
}