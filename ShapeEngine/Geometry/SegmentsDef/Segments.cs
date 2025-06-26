using System.Numerics;
using ShapeEngine.Core;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Random;

namespace ShapeEngine.Geometry.SegmentsDef;

public partial class Segments : ShapeList<Segment>
{
    #region Constructors
    public Segments() { }
    public Segments(int capacity) : base(capacity) { }
    //public Segments(IShape shape) { AddRange(shape.GetEdges()); }
    public Segments(IEnumerable<Segment> edges) { AddRange(edges); }
    #endregion

    #region Equals & HashCode
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
    public override int GetHashCode() => Game.GetHashCode(this);

    #endregion

    #region Public
    
    public Segment GetSegment(int index)
    {
        if (index < 0) return new Segment();
        if (Count <= 0) return new Segment();
        var i = index % Count;
        return this[i];
    }
    
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
    public Vector2 GetRandomPoint() => GetRandomSegment().GetRandomPoint();
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
    /// <param name="seg"></param>
    /// <returns></returns>
    public int GetCount(Segment seg) { return this.Count((s) => s.Equals(seg)); }

    /// <summary>
    /// Counts how often the specified segment appears in the list disregarding the direction of each segment.
    /// </summary>
    /// <param name="seg"></param>
    /// <returns></returns>
    public int GetCountSimilar(Segment seg) { return this.Count((s) => s.IsSimilar(seg)); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="seg"></param>
    /// <returns>Returns true if seg is already in the list.</returns>
    public bool ContainsSegment(Segment seg)
    {
        foreach (var segment in this) { if (segment.Equals(seg)) return true; }
        return false;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="seg"></param>
    /// <returns>Returns true if similar segment is already in the list.</returns>
    public bool ContainsSegmentSimilar(Segment seg)
    {
        foreach (var segment in this) { if (segment.IsSimilar(seg)) return true; }
        return false;
    }
    #endregion
}