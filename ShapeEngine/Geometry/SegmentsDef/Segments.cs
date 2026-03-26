using System.Numerics;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Random;
using ShapeEngine.StaticLib;
using Game = ShapeEngine.Core.GameDef.Game;

namespace ShapeEngine.Geometry.SegmentsDef;

/// <summary>
/// A list of segments.
/// </summary>
public partial class Segments : ShapeList<Segment>
{
    #region Helper
    
    private static HashSet<Vector2> hashSetVector2Buffer = new();
    private static HashSet<Segment> hashSetSegmentBuffer = new();
    private static List<Segment> segmentsBuffer = new();
    private static List<WeightedItem<Segment>> weightedSegmentBuffer = new();
    
    #endregion
    
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
        if (Count <= 0) return new Segment();
        var i =ShapeMath.WrapI(index, 0, Count - 1);
        return this[i];
    }
    
    /// <summary>
    /// Collects all unique segment endpoints in this collection and writes them into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the unique points.</param>
    /// <remarks>
    /// This method does not modify the current collection. Point uniqueness is determined by the equality comparer used by the internal <see cref="HashSet{T}"/>.
    /// </remarks>
    public void GetUniquePoints(Points result)
    {
        hashSetVector2Buffer.Clear();
        for (int i = 0; i < Count; i++)
        {
            var seg = this[i];
            hashSetVector2Buffer.Add(seg.Start);
            hashSetVector2Buffer.Add(seg.End);
        }

        result.Clear();
        result.EnsureCapacity(hashSetVector2Buffer.Count);
        result.AddRange(hashSetVector2Buffer);
    }
    
    /// <summary>
    /// Collects all unique segments in this collection and writes them into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the unique segments.</param>
    /// <remarks>
    /// This method does not modify the current collection. Segment uniqueness is determined by the equality comparer used by the internal <see cref="HashSet{T}"/>.
    /// </remarks>
    public void GetUniqueSegments(Segments result)
    {
        hashSetSegmentBuffer.Clear();
        for (int i = 0; i < Count; i++)
        {
            var seg = this[i];
            hashSetSegmentBuffer.Add(seg);
        }

        result.Clear();
        result.EnsureCapacity(hashSetSegmentBuffer.Count);
        result.AddRange(hashSetSegmentBuffer);
    }

    /// <summary>
    /// Gets a random segment from the list.
    /// </summary>
    /// <returns>A random segment from the list. The longer the segment, the higher the chance of being picked.</returns>
    public Segment GetRandomSegment()
    {
        weightedSegmentBuffer.Clear();
        weightedSegmentBuffer.EnsureCapacity(Count);
        for (var i = 0; i < Count; i++)
        {
            var seg = this[i];
            weightedSegmentBuffer.Add(new(seg, (int)seg.LengthSquared));
        }
        return Rng.Instance.PickRandomItem(weightedSegmentBuffer);
    }
    
    /// <summary>
    /// Gets a random point on a random segment from the list.
    /// </summary>
    /// <returns>A random point on a random segment.</returns>
    public Vector2 GetRandomPoint() => GetRandomSegment().GetRandomPoint();
   
    /// <summary>
    /// Writes random points sampled from randomly selected segments into <paramref name="result"/>.
    /// </summary>
    /// <param name="amount">The amount of random points to generate.</param>
    /// <param name="result">The destination collection that will be cleared and populated with the generated points.</param>
    /// <remarks>
    /// Segment selection is weighted by each segment's squared length, so longer segments are more likely to be chosen.
    /// </remarks>
    public void GetRandomPoints(int amount, Points result)
    {
        weightedSegmentBuffer.Clear();
        weightedSegmentBuffer.EnsureCapacity(Count);
        
        for (var i = 0; i < Count; i++)
        {
            var seg = this[i];
            weightedSegmentBuffer.Add(new(seg, (int)seg.LengthSquared));
        }
        
        Rng.Instance.PickRandomItems(segmentsBuffer, amount, weightedSegmentBuffer);
        result.Clear();
        result.EnsureCapacity(segmentsBuffer.Count);
        
        foreach (var seg in segmentsBuffer)
        {
            result.Add(seg.GetRandomPoint());
        }
    }
    
    /// <summary>
    /// Writes randomly selected segments from this collection into <paramref name="result"/>.
    /// </summary>
    /// <param name="amount">The number of segments to select.</param>
    /// <param name="result">The destination collection that will receive the selected segments.</param>
    /// <remarks>
    /// Segment selection is weighted by each segment's squared length, so longer segments are more likely to be chosen.
    /// </remarks>
    public void GetRandomSegments(int amount, Segments result)
    {
        weightedSegmentBuffer.Clear();
        weightedSegmentBuffer.EnsureCapacity(Count);
        
        for (var i = 0; i < Count; i++)
        {
            var seg = this[i];
            weightedSegmentBuffer.Add(new(seg, (int)seg.LengthSquared));
        }
        
        Rng.Instance.PickRandomItems(result, amount, weightedSegmentBuffer);
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
    
    /// <summary>
    /// Writes the direction vector of each segment into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination list that receives one direction vector per segment.</param>
    /// <param name="normalized"><c>true</c> to write normalized direction vectors; <c>false</c> to write each segment's full displacement vector.</param>
    public void GetSegmentDirections(List<Vector2> result, bool normalized = false)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        foreach (var seg in this)
        {
            result.Add(normalized ? seg.Dir : seg.Displacement);
        }
    }
    #endregion
}