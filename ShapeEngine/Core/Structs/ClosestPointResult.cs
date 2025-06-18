//TODO: Move to CollisionSystem namespace!
namespace ShapeEngine.Core.Structs;

/// <summary>
/// Represents the result of a closest point calculation between two shapes or segments, including the points, distance, and segment indices.
/// </summary>
/// <remarks>
/// Used to store the closest points and related information for collision or proximity queries.
/// </remarks>
public readonly struct ClosestPointResult
{
    /// <summary>
    /// The closest point on the 'self' shape or segment.
    /// </summary>
    public readonly CollisionPoint Self;
    /// <summary>
    /// The closest point on the 'other' shape or segment.
    /// </summary>
    public readonly CollisionPoint Other;
    /// <summary>
    /// The squared distance between <see cref="Self"/> and <see cref="Other"/>.
    /// </summary>
    public readonly float DistanceSquared;
    /// <summary>
    /// The index of the segment on the 'self' shape, or -1 if not applicable.
    /// </summary>
    public readonly int SegmentIndex;
    /// <summary>
    /// The index of the segment on the 'other' shape, or -1 if not applicable.
    /// </summary>
    public readonly int OtherSegmentIndex;
    /// <summary>
    /// Gets whether this result is valid (distance is non-negative).
    /// </summary>
    public bool Valid => DistanceSquared >= 0;

    /// <summary>
    /// Initializes a new invalid <see cref="ClosestPointResult"/>.
    /// </summary>
    public ClosestPointResult()
    {
        Self = new();
        Other = new();
        DistanceSquared = -1;
        SegmentIndex = -1;
        OtherSegmentIndex = -1;
    }
    /// <summary>
    /// Initializes a new <see cref="ClosestPointResult"/> with the specified points, distance, and segment indices.
    /// </summary>
    /// <param name="self">The closest point on the 'self' shape.</param>
    /// <param name="other">The closest point on the 'other' shape.</param>
    /// <param name="distanceSquared">The squared distance between the points.</param>
    /// <param name="segmentIndex">The segment index on the 'self' shape (default -1).</param>
    /// <param name="otherSegmentIndex">The segment index on the 'other' shape (default -1).</param>
    public ClosestPointResult(CollisionPoint self, CollisionPoint other, float distanceSquared, int segmentIndex = -1, int otherSegmentIndex = -1)
    {
        Self = self;
        Other = other;
        DistanceSquared = distanceSquared;
        SegmentIndex = segmentIndex;
        OtherSegmentIndex = otherSegmentIndex;
    }
    /// <summary>
    /// Returns a new <see cref="ClosestPointResult"/> with the self/other points and segment indices swapped.
    /// </summary>
    /// <returns>A new <see cref="ClosestPointResult"/> with swapped points and indices.</returns>
    public ClosestPointResult Switch() => new(Other, Self, DistanceSquared, OtherSegmentIndex, SegmentIndex);
    /// <summary>
    /// Determines if this result is closer than another <see cref="ClosestPointResult"/>.
    /// </summary>
    /// <param name="other">The other result to compare to.</param>
    /// <returns>True if this result is closer (smaller distance squared), false otherwise.</returns>
    public bool IsCloser(ClosestPointResult other) => DistanceSquared < other.DistanceSquared;
    /// <summary>
    /// Determines if this result is closer than a given distance squared.
    /// </summary>
    /// <param name="distanceSquared">The distance squared to compare to.</param>
    /// <returns>True if this result is closer, false otherwise.</returns>
    public bool IsCloser(float distanceSquared) => DistanceSquared < distanceSquared;
    /// <summary>
    /// Returns a new <see cref="ClosestPointResult"/> with the segment index for 'self' set to the specified value.
    /// </summary>
    /// <param name="index">The new segment index for 'self'.</param>
    /// <returns>A new <see cref="ClosestPointResult"/> with the updated segment index.</returns>
    public ClosestPointResult SetSegmentIndex(int index) => new(Self, Other, DistanceSquared, index, OtherSegmentIndex);
    /// <summary>
    /// Returns a new <see cref="ClosestPointResult"/> with the segment index for 'other' set to the specified value.
    /// </summary>
    /// <param name="index">The new segment index for 'other'.</param>
    /// <returns>A new <see cref="ClosestPointResult"/> with the updated other segment index.</returns>
    public ClosestPointResult SetOtherSegmentIndex(int index) => new(Self, Other, DistanceSquared, SegmentIndex, index);
}