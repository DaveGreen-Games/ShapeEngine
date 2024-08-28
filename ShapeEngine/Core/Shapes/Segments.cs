using System.Numerics;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;
using ShapeEngine.Random;

namespace ShapeEngine.Core.Shapes;

public class Segments : ShapeList<Segment>
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
    public ClosestSegment GetClosest(Vector2 p)
    {
        if (Count <= 0) return new();


        var closestSegment = this[0];
        var closestDistance = closestSegment.GetClosestDistanceTo(p);

        for (int i = 1; i < Count - 1; i++)
        {
            var segment = this[i];
            var cd = segment.GetClosestDistanceTo(p);
            if (cd.DistanceSquared < closestDistance.DistanceSquared)
            {
                closestSegment = segment;
                closestDistance = cd;
            }
        }

        return new(closestSegment, closestDistance);
        // float minDisSquared = float.PositiveInfinity;
        // Segment closestSegment = new();
        // Vector2 closestSegmentPoint = new();
        // for (var i = 0; i < Count; i++)
        // {
        //     var seg = this[i];
        //     var closestPoint = seg.GetClosestCollisionPoint(p).Point;
        //     float disSquared = (closestPoint - p).LengthSquared();
        //     if(disSquared < minDisSquared)
        //     {
        //         minDisSquared = disSquared;
        //         closestSegment = seg;
        //         closestSegmentPoint = closestPoint;
        //     }
        // }
        //
        // return new(closestSegment, closestSegmentPoint, MathF.Sqrt(minDisSquared));
    }
    public CollisionPoint GetClosestCollisionPoint(Vector2 p)
    {
        float minD = float.PositiveInfinity;
            
        CollisionPoint closest = new();

        for (int i = 0; i < Count; i++)
        {
            CollisionPoint c = this[i].GetClosestCollisionPoint(p);
            float d = (c.Point - p).LengthSquared();
            if (d < minD)
            {
                closest = c;
                minD = d;
            }
        }
        return closest;
    }
    public int GetClosestIndexOnEdge(Vector2 p)
    {
        if (Count <= 0) return -1;
        if (Count == 1) return 0;

        float minD = float.PositiveInfinity;
        int closestIndex = -1;

        for (var i = 0; i < Count; i++)
        {
            var edge = this[i];
            var closest = edge.GetClosestCollisionPoint(p).Point;
            float d = (closest - p).LengthSquared();
            if (d < minD)
            {
                closestIndex = i;
                minD = d;
            }
        }
        return closestIndex;
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

    #region Transform

    
    public void ChangeRotation(float rad, float originF = 0.5f)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ChangeRotation(rad, originF);
        }
    }
    public void SetRotation(float rad, float originF = 0.5f)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].SetRotation(rad, originF);
        }
    }

    public void ScaleLength(float scale, float originF = 0.5f)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ScaleLength(scale, originF);
        }
    }
    public void ScaleLength(Size scale, float originF = 0.5f)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ScaleLength(scale, originF);
        }
    }
    
    public void ChangeLength(float amount, float originF = 0.5f)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ChangeLength(amount, originF);
        }
    }
    public void SetSize(float length, float originF = 0.5f)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].SetLength(length, originF);
        }
    }
    
    public void ChangePosition(Vector2 offset)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ChangePosition(offset);
        }
    }
    public void SetPosition(Vector2 position, float originF = 0.5f)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].SetPosition(position, originF);
        }
    }

    public void ApplyTransform(Transform2D transform, float originF = 0.5f)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ApplyTransform(transform, originF);
        }
    }
    public void SetTransform(Transform2D transform, float originF = 0.5f)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].SetTransform(transform, originF);
        }
    }

    
    
    public Segments ChangeRotationCopy(float rad, float originF = 0.5f)
    {
        var newSegments = new Segments(Count);
        for (var i = 0; i < Count; i++)
        {
            newSegments.Add(this[i].ChangeRotation(rad, originF));
        }

        return newSegments;
    }
    
    public Segments SetRotationCopy(float rad, float originF = 0.5f)
    {
        var newSegments = new Segments(Count);
        for (var i = 0; i < Count; i++)
        {
            newSegments.Add(this[i].SetRotation(rad, originF));
        }

        return newSegments;
    }

    public Segments ScaleLengthCopy(float scale, float originF = 0.5f)
    {
        var newSegments = new Segments(Count);
        for (var i = 0; i < Count; i++)
        {
            newSegments.Add(this[i].ScaleLength(scale, originF));
        }

        return newSegments;
    }
    public Segments ScaleLengthCopy(Size scale, float originF = 0.5f)
    {
        var newSegments = new Segments(Count);
        for (var i = 0; i < Count; i++)
        {
            newSegments.Add(this[i].ScaleLength(scale, originF));
        }

        return newSegments;
    }
    
    public Segments ChangeLengthCopy(float amount, float originF = 0.5f)
    {
        var newSegments = new Segments(Count);
        for (var i = 0; i < Count; i++)
        {
            newSegments.Add(this[i].ChangeLength(amount, originF));
        }

        return newSegments;
    }
    
    public Segments SetLengthCopy(float size, float originF = 0.5f)
    {
        var newSegments = new Segments(Count);
        for (var i = 0; i < Count; i++)
        {
            newSegments.Add(this[i].SetLength(size, originF));
        }

        return newSegments;
    }
    
    public Segments ChangePositionCopy(Vector2 offset)
    {
        var newSegments = new Segments(Count);
        for (var i = 0; i < Count; i++)
        {
            newSegments.Add(this[i].ChangePosition(offset));
        }

        return newSegments;
    }
    public Segments SetPositionCopy(Vector2 position, float originF = 0.5f)
    {
        var newSegments = new Segments(Count);
        for (var i = 0; i < Count; i++)
        {
            newSegments.Add(this[i].SetPosition(position, originF));
        }

        return newSegments;
    }

    public Segments ApplyTransformCopy(Transform2D transform, float originF = 0.5f)
    {
        var newSegments = new Segments(Count);
        for (var i = 0; i < Count; i++)
        {
            newSegments.Add(this[i].ApplyTransform(transform, originF));
        }

        return newSegments;
    }
    public Segments SetTransformCopy(Transform2D transform, float originF = 0.5f)
    {
        var newSegments = new Segments(Count);
        for (var i = 0; i < Count; i++)
        {
            newSegments.Add(this[i].SetTransform(transform, originF));
        }

        return newSegments;
    }

    #endregion
    
    #region Overlap
    public bool OverlapShape(Segments b)
    {
        foreach (var seg in this)
        {
            foreach (var bSeg in b)
            {
                if(Segment.OverlapSegmentSegment(seg.Start, seg.End, bSeg.Start, bSeg.End)) return true;
            }
            
        }
        return false;
    }
    public bool OverlapShape(Segment s)
    {
        foreach (var seg in this)
        {
            if(Segment.OverlapSegmentSegment(seg.Start, seg.End, s.Start, s.End)) return true;
        }
        return false;
    }

    public bool OverlapShape(Circle c)
    {
        foreach (var seg in this)
        {
            if (Segment.OverlapSegmentCircle(seg.Start, seg.End, c.Center, c.Radius)) return true;
        }
        return false;
    }
    public bool OverlapShape(Triangle t) => t.OverlapShape(this);
    public bool OverlapShape(Quad q) => q.OverlapShape(this);
    public bool OverlapShape(Rect r) => r.OverlapShape(this);
    public bool OverlapShape(Polyline pl) => pl.OverlapShape(this);
    public bool OverlapShape(Polygon p) => p.OverlapShape(this);

    #endregion

    #region Intersection
    public CollisionPoints? IntersectShape(Segment s)
    {
        CollisionPoints? points = null;

        foreach (var seg in this)
        {
            var result = Segment.IntersectSegmentSegment(seg.Start, seg.End, s.Start, s.End);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
        }
        return points;
        
        // CollisionPoints? points = null;
        //
        // foreach (var seg in this)
        // {
        //     var collisionPoints = seg.IntersectShape(s);
        //     if (collisionPoints != null && collisionPoints.Valid)
        //     {
        //         points ??= new();
        //         points.AddRange(collisionPoints);
        //     }
        // }
        // return points;
    }
    public CollisionPoints? IntersectShape(Circle c)
    {
        CollisionPoints? points = null;
        foreach (var seg in this)
        {
            var result = Segment.IntersectSegmentCircle(seg.Start, seg.End, c.Center, c.Radius);
            if (result.a != null || result.b != null)
            {
                points ??= new();
                if(result.a != null) points.Add((CollisionPoint)result.a);
                if(result.b != null) points.Add((CollisionPoint)result.b);
            }
            // if(intersectPoints == null) continue;
            // foreach (var p in intersectPoints)
            // {
                // var n = ShapeVec.Normalize(p - c.Center);
                // points ??= new();
                // points.Add(new(p, n));
            // }
        }
        return points;
    }
    public CollisionPoints? IntersectShape(Segments b)
    {
        CollisionPoints? points = null;

        foreach (var seg in this)
        {
            foreach (var bSeg in b)
            {
                var result = Segment.IntersectSegmentSegment(seg.Start, seg.End, bSeg.Start, bSeg.End);
                if (result != null)
                {
                    points ??= new();
                    points.AddRange((CollisionPoint)result);
                }
            }
            
        }
        return points;
        
        // CollisionPoints? points = null;
        // foreach (var seg in this)
        // {
            // var collisionPoints = seg.IntersectShape(b);
            // if (collisionPoints != null && collisionPoints.Valid)
            // {
                // points ??= new();
                // points.AddRange(collisionPoints);
            // }
        // }
        // return points;
    }

    #endregion

    /*
        /// <summary>
        /// Only add the segment if it not already contained in the list.
        /// </summary>
        /// <param name="seg"></param>
        public void AddUnique(Segment seg)
        {
            if (!ContainsSegment(seg)) Add(seg);
        }
        /// <summary>
        /// Only add the segments that are not already contained in the list.
        /// </summary>
        /// <param name="edges"></param>
        public void AddUnique(IEnumerable<Segment> edges)
        {
            foreach (var edge in edges)
            {
                AddUnique(edge);
            }
        }
        */
}