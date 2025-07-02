using System.Numerics;
using ShapeEngine.Core;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Random;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.SegmentsDef;

/// <summary>
/// A list of segments.
/// </summary>
public partial class Segments : ShapeList<Segment>
{
    #region Constructors
    /// <summary>
    /// Creates an empty list of segments.
    /// </summary>
    public Segments() { }
    /// <summary>
    /// Creates an empty list of segments with the specified capacity.
    /// </summary>
    /// <param name="capacity">The initial capacity of the list.</param>
    public Segments(int capacity) : base(capacity) { }
    //public Segments(IShape shape) { AddRange(shape.GetEdges()); }
    /// <summary>
    /// Creates a new list of segments from the given enumerable.
    /// </summary>
    /// <param name="edges">The segments to add to the list.</param>
    public Segments(IEnumerable<Segment> edges) { AddRange(edges); }
    #endregion

    #region Equals & HashCode
    /// <summary>
    /// Checks if two lists of segments are equal.
    /// </summary>
    /// <param name="other">The other list of segments to check.</param>
    /// <returns>True if the two lists are equal, false otherwise.</returns>
    public bool Equals(Segments? other)
    {
        if (other == null) return false;
        if (Count != other.Count) return false;
        for (var i = 0; i < Count; i++)
        {
            if (this[i] != other[i]) return false;
        }
        return true;
    }
    /// <summary>
    /// Gets the hash code for the list of segments.
    /// </summary>
    /// <returns>The hash code for the list of segments.</returns>
    public override int GetHashCode() => Game.GetHashCode(this);

    #endregion

    #region Public
    
    /// <summary>
    /// Gets the segment at the specified index.
    /// </summary>
    /// <param name="index">The index of the segment to get.</param>
    /// <returns>The segment at the specified index, with the index wrapped if out of bounds.</returns>
    public Segment GetSegment(int index)
    {
        // if (index < 0) return new Segment();
        if (Count <= 0) return new Segment();
        var i =ShapeMath.WrapI(index, 0, Count - 1);
        // var i = ((index % Count) + Count) % Count;
        // var i = index % Count;
        return this[i];
    }
    
    /// <summary>
    /// Gets a list of unique points from all the segments in the list.
    /// </summary>
    /// <returns>A list of unique points.</returns>
    public Points GetUniquePoints()
    {
        var uniqueVertices = new HashSet<Vector2>();
        for (int i = 0; i < Count; i++)
        {
            var seg = this[i];
            uniqueVertices.Add(seg.Start);
            uniqueVertices.Add(seg.End);
        }

        return new(uniqueVertices);
    }
    /// <summary>
    /// Gets a list of unique segments from the list.
    /// </summary>
    /// <returns>A new list of segments containing only the unique segments from the original list.</returns>
    public Segments GetUniqueSegments()
    {
        var uniqueSegments = new HashSet<Segment>();
        for (int i = 0; i < Count; i++)
        {
            var seg = this[i];
            uniqueSegments.Add(seg);
        }

        return new(uniqueSegments);
    }

    /// <summary>
    /// Gets a random segment from the list.
    /// </summary>
    /// <returns>A random segment from the list. The longer the segment, the higher the chance of being picked.</returns>
    public Segment GetRandomSegment()
    {
        var items = new WeightedItem<Segment>[Count];
        for (var i = 0; i < Count; i++)
        {
            var seg = this[i];
            items[i] = new(seg, (int)seg.LengthSquared);
        }
        return Rng.Instance.PickRandomItem(items);
    }
    /// <summary>
    /// Gets a random point on a random segment from the list.
    /// </summary>
    /// <returns>A random point on a random segment.</returns>
    public Vector2 GetRandomPoint() => GetRandomSegment().GetRandomPoint();
    /// <summary>
    /// Gets a list of random points on random segments from the list.
    /// </summary>
    /// <param name="amount">The amount of random points to generate.</param>
    /// <returns>A list of random points.</returns>
    public Points GetRandomPoints(int amount)
    {
        var items = new WeightedItem<Segment>[Count];
        for (var i = 0; i < Count; i++)
        {
            var seg = this[i];
            items[i] = new(seg, (int)seg.LengthSquared);
        }
        var pickedSegments = Rng.Instance.PickRandomItems(amount, items);
        var randomPoints = new Points();
        foreach (var seg in pickedSegments)
        {
            randomPoints.Add(seg.GetRandomPoint());
        }
        return randomPoints;
    }
        
    /// <summary>
    /// Counts how often the specified segment appears in the list.
    /// </summary>
    /// <param name="seg">The segment to count.</param>
    /// <returns>The amount of times the specified segment appears in the list.</returns>
    public int GetCount(Segment seg) { return this.Count((s) => s.Equals(seg)); }

    /// <summary>
    /// Counts how often the specified segment appears in the list disregarding the direction of each segment.
    /// </summary>
    /// <param name="seg">The segment to count.</param>
    /// <returns>The amount of times the specified segment appears in the list.</returns>
    public int GetCountSimilar(Segment seg) { return this.Count((s) => s.IsSimilar(seg)); }

    /// <summary>
    /// Checks if the specified segment is in the list.
    /// </summary>
    /// <param name="seg">The segment to check for.</param>
    /// <returns>Returns true if seg is already in the list.</returns>
    public bool ContainsSegment(Segment seg)
    {
        foreach (var segment in this) { if (segment.Equals(seg)) return true; }
        return false;
    }
    /// <summary>
    /// Checks if a similar segment is in the list.
    /// </summary>
    /// <param name="seg">The segment to check for.</param>
    /// <returns>Returns true if similar segment is already in the list.</returns>
    public bool ContainsSegmentSimilar(Segment seg)
    {
        foreach (var segment in this) { if (segment.IsSimilar(seg)) return true; }
        return false;
    }
    #endregion
}