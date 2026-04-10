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
    #region Helper
    
    private static readonly HashSet<Segment> hashSetUndirectedSegmentBuffer = new(SegmentEndpointsUndirectedComparer.Instance);
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
        return Equals(other, DecimalPrecision.DefaultDecimalPlaces);
    }

    /// <summary>
    /// Checks if two lists of segments are equal using quantized segment comparison.
    /// </summary>
    /// <param name="other">The other list of segments to check.</param>
    /// <param name="decimalPlaces">The number of decimal places used to quantize coordinates before comparison.</param>
    /// <returns>True if the two lists are equal after quantization, false otherwise.</returns>
    public bool Equals(Segments? other, int decimalPlaces)
    {
        if (other == null) return false;
        if (Count != other.Count) return false;
        if (decimalPlaces < 0) decimalPlaces = DecimalPrecision.DefaultDecimalPlaces;

        for (var i = 0; i < Count; i++)
        {
            if (!this[i].Equals(other[i], decimalPlaces)) return false;
        }
        return true;
    }
    
    /// <summary>
    /// Creates a stable 64-bit hash key for the current segment collection by hashing segments in order.
    /// </summary>
    /// <param name="decimalPlaces">The number of decimal places used to quantize coordinates before hashing.</param>
    /// <returns>A 64-bit hash key suitable for cache keys and change detection.</returns>
    public ulong GetHashKey(int decimalPlaces = DecimalPrecision.DefaultDecimalPlaces)
    {
        if (decimalPlaces < 0) decimalPlaces = DecimalPrecision.DefaultDecimalPlaces;

        Fnv1aHashQuantizer hashQuantizer = new(decimalPlaces);
        ulong hash = hashQuantizer.StartHash(Count);
        for (int i = 0; i < Count; i++)
        {
            Segment segment = this[i];
            hash = hashQuantizer.Add(hash, segment.Start);
            hash = hashQuantizer.Add(hash, segment.End);
        }

        return hash;
    }

    /// <summary>
    /// Creates a fixed-width hexadecimal string representation of the current segment collection hash key.
    /// </summary>
    /// <param name="decimalPlaces">The number of decimal places used to quantize coordinates before hashing.</param>
    /// <returns>A 16-character uppercase hexadecimal hash key string.</returns>
    public string GetHashKeyHex(int decimalPlaces = DecimalPrecision.DefaultDecimalPlaces) => GetHashKey(decimalPlaces).ToString("X16");

    /// <summary>
    /// Creates a string representation of the current segment collection hash key.
    /// </summary>
    /// <param name="decimalPlaces">The number of decimal places used to quantize coordinates before hashing.</param>
    /// <returns>A stable hexadecimal hash key string.</returns>
    public string GetHashKeyString(int decimalPlaces = DecimalPrecision.DefaultDecimalPlaces) => GetHashKeyHex(decimalPlaces);

    /// <summary>
    /// Gets the hash code for the list of segments.
    /// </summary>
    /// <returns>The hash code for the list of segments.</returns>
    public override int GetHashCode()
    {
        ulong hashKey = GetHashKey();
        return unchecked((int)(hashKey ^ (hashKey >> 32)));
    }
    
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
    /// Uniqueness is directional, so Start of first segment must equal Start of second segment AND End of first segment must equal End of second segment for the segments to be considered equal.
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
    /// Collects all unique segments in this collection while ignoring segment direction and writes them into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the unique undirected segments.</param>
    /// <remarks>
    /// Two segments are considered equal if they share the same endpoints regardless of whether start and end are swapped.
    /// This method does not modify the current collection.
    /// </remarks>
    public void GetUniqueSegmentsUndirectional(Segments result)
    {
        hashSetUndirectedSegmentBuffer.Clear();
        hashSetUndirectedSegmentBuffer.EnsureCapacity(Count);
        for (int i = 0; i < Count; i++)
        {
            var seg = this[i];
            hashSetUndirectedSegmentBuffer.Add(seg);
        }
        
        result.Clear();
        result.EnsureCapacity(hashSetUndirectedSegmentBuffer.Count);
        result.AddRange(hashSetUndirectedSegmentBuffer);
        
        //Simple variant
        // result.Clear();
        // result.EnsureCapacity(Count);
        //
        // for (int i = 0; i < Count; i++)
        // {
        //     var seg = this[i];
        //     if (!result.ContainsSegmentSimilar(seg))
        //     {
        //         result.Add(seg);
        //     }
        // }
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
    
    private sealed class SegmentEndpointsUndirectedComparer : IEqualityComparer<Segment>
    {
        public static readonly SegmentEndpointsUndirectedComparer Instance = new();

        public bool Equals(Segment x, Segment y)
        {
            return (x.Start.Equals(y.Start) && x.End.Equals(y.End)) ||
                   (x.Start.Equals(y.End) && x.End.Equals(y.Start));
        }

        public int GetHashCode(Segment segment)
        {
            var start = segment.Start;
            var end = segment.End;

            if (Compare(start, end) > 0)
            {
                (start, end) = (end, start);
            }

            return HashCode.Combine(start, end);
        }

        private static int Compare(Vector2 a, Vector2 b)
        {
            int xCompare = a.X.CompareTo(b.X);
            return xCompare != 0 ? xCompare : a.Y.CompareTo(b.Y);
        }
    }
    
}