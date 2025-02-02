using System.Numerics;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;
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

    public void ApplyOffset(Transform2D offset, float originF = 0.5f)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ApplyOffset(offset, originF);
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

    public Segments ApplyOffsetCopy(Transform2D offset, float originF = 0.5f)
    {
        var newSegments = new Segments(Count);
        for (var i = 0; i < Count; i++)
        {
            newSegments.Add(this[i].ApplyOffset(offset, originF));
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
    
    #region Closest Point
    
    public static Vector2 GetClosestPointSegmentsPoint(List<Vector2> points, Vector2 p, out float disSquared)
    {
        disSquared = -1;
        if (points.Count <= 2) return new();
        
        var first = points[0];
        var second = points[1];
        var closest = Segment.GetClosestPointSegmentPoint(first, second, p, out disSquared);
        
        for (var i = 1; i < points.Count; i++)
        {
            var p1 = points[i];
            var p2 = points[(i + 1) % points.Count];
            
            var cp = Segment.GetClosestPointSegmentPoint(p1, p2, p, out float dis);
            if (dis < disSquared)
            {
                closest = cp;
                disSquared = dis;
            }
        
        }
        return closest;
    }
    
    public CollisionPoint GetClosestPoint(Vector2 p, out float disSquared)
    {
        disSquared = -1;
        if (Count <= 2) return new();
        
        var closestSegment = this[0];
        var closestPoint = Segment.GetClosestPointSegmentPoint(closestSegment.Start, closestSegment.End, p, out disSquared);
        
        for (var i = 1; i < Count; i++)
        {
            var curSegment = this[i];
            
            var cp = Segment.GetClosestPointSegmentPoint(curSegment.Start, curSegment.End, p, out float dis);
            if (dis < disSquared)
            {
                closestSegment = curSegment;
                closestPoint = cp;
                disSquared = dis;
            }
        
        }

        return new(closestPoint, closestSegment.Normal);
    }
    public CollisionPoint GetClosestPoint(Vector2 p, out float disSquared, out int index)
    {
        disSquared = -1;
        index = -1;
        if (Count <= 2) return new();
        
        var closestSegment = this[0];
        var closestPoint = Segment.GetClosestPointSegmentPoint(closestSegment.Start, closestSegment.End, p, out disSquared);
        index = 0;
        for (var i = 1; i < Count; i++)
        {
            var curSegment = this[i];
            
            var cp = Segment.GetClosestPointSegmentPoint(curSegment.Start, curSegment.End, p, out float dis);
            if (dis < disSquared)
            {
                index = i;
                closestSegment = curSegment;
                closestPoint = cp;
                disSquared = dis;
            }
        
        }

        return new(closestPoint, closestSegment.Normal);
    }
    public Vector2 GetClosestVertex(Vector2 p, out float disSquared, out int index)
    {
        disSquared = -1;
        index = -1;
        if (Count <= 2) return new();
        
        index = 0;
        var segment = this[index];
        var closest = segment.Start;
        disSquared = (segment.Start - p).LengthSquared();
        
        var dis = (segment.End - p).LengthSquared();
        if (dis < disSquared)
        {
            disSquared = dis;
            closest = segment.End;
        }
        
        for (var i = 1; i < Count; i++)
        {
            segment = this[i];
            dis = (segment.Start - p).LengthSquared();
            if (dis < disSquared)
            {
                index = i;
                closest = segment.Start;
                disSquared = dis;
            }
            
            dis = (segment.End - p).LengthSquared();
            if (dis < disSquared)
            {
                index = i;
                closest = segment.End;
                disSquared = dis;
            }
        }
        return closest;
    }

    public ClosestPointResult GetClosestPoint(Line other)
    {
        if (Count <= 2) return new();
        var closestSegment = this[0];
        var result = Segment.GetClosestPointSegmentLine(closestSegment.Start, closestSegment.End, other.Point, other.Direction, out float disSquared);
        var selfIndex = 0;
        
        for (var i = 1; i < Count; i++)
        {
            var curSegment = this[i];
            
            var cp = Segment.GetClosestPointSegmentLine(curSegment.Start, curSegment.End, other.Point, other.Direction, out float dis);
            if (dis < disSquared)
            {
                selfIndex = i;
                result = cp;
                disSquared = dis;
                closestSegment = curSegment;
            }
        
        }

        return new(
            new(result.self, closestSegment.Normal), 
            new(result.other, other.Normal),
            disSquared,
            selfIndex);
    }
    public ClosestPointResult GetClosestPoint(Ray other)
    {
        if (Count <= 2) return new();
        
        var closestSegment = this[0];
        var result = Segment.GetClosestPointSegmentRay(closestSegment.Start, closestSegment.End, other.Point, other.Direction, out float disSquared);
        var selfIndex = 0;
        
        for (var i = 1; i < Count; i++)
        {
            var segment = this[i];
            
            var cp = Segment.GetClosestPointSegmentRay(segment.Start, segment.End, other.Point, other.Direction, out float dis);
            if (dis < disSquared)
            {
                selfIndex = i;
                result = cp;
                disSquared = dis;
                closestSegment = segment;
            }
        
        }

        return new(
            new(result.self, closestSegment.Normal), 
            new(result.other, other.Normal),
            disSquared,
            selfIndex);
    }
    public ClosestPointResult GetClosestPoint(Segment other)
    {
        if (Count <= 2) return new();
        
        var closestSegment = this[0];
        var result = Segment.GetClosestPointSegmentSegment(closestSegment.Start, closestSegment.End, other.Start, other.End, out float disSquared);
        var selfIndex = 0;
        
        for (var i = 1; i < Count; i++)
        {
            var segment = this[i];
            
            var cp = Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.Start, other.End, out float dis);
            if (dis < disSquared)
            {
                selfIndex = i;
                result = cp;
                disSquared = dis;
                closestSegment = segment;
            }
        
        }

        return new (
            new(result.self, closestSegment.Normal), 
            new(result.other, other.Normal),
            disSquared,
            selfIndex);
    }
    public ClosestPointResult GetClosestPoint(Circle other)
    {
        if (Count <= 2) return new();
        
        var closestSegment = this[0];
        var result = Segment.GetClosestPointSegmentCircle(closestSegment.Start, closestSegment.End, other.Center, other.Radius, out float disSquared);
        var selfIndex = 0;
        
        for (var i = 1; i < Count; i++)
        {
            var segment = this[i];
            
            var cp = Segment.GetClosestPointSegmentCircle(segment.Start, segment.End, other.Center, other.Radius, out float dis);
            if (dis < disSquared)
            {
                selfIndex = i;
                result = cp;
                disSquared = dis;
                closestSegment = segment;
            }
        }

        return new (
            new(result.self, closestSegment.Normal), 
            new(result.other, (result.other - other.Center).Normalize()),
            disSquared,
            selfIndex
        );
    }
    public ClosestPointResult GetClosestPoint(Triangle other)
    {
        if (Count <= 2) return new();
        
        (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
        var selfNormal = Vector2.Zero;
        var otherNormal = Vector2.Zero;
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;
        
        for (var i = 0; i < Count; i++)
        {
            var segment = this[i];
            
            var cp = Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.A, other.B, out float dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 0;
                result = cp;
                disSquared = dis;
                selfNormal = segment.Normal;
                otherNormal = other.B - other.A;
            }
            
            cp = Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.B, other.C, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 1;
                result = cp;
                disSquared = dis;
                selfNormal = segment.Normal;
                otherNormal = other.C - other.B;
            }
            
            cp = Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.C, other.A, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 2;
                result = cp;
                disSquared = dis;
                selfNormal = segment.Normal;
                otherNormal = other.A - other.C;
            }
        
        }

        return new(
            new(result.self, selfNormal.GetPerpendicularRight().Normalize()), 
            new(result.other, otherNormal.GetPerpendicularRight().Normalize()),
            disSquared,
            selfIndex,
            otherIndex);
    }
    public ClosestPointResult GetClosestPoint(Quad other)
    {
        if (Count <= 2) return new();
        
        (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
        var selfNormal = Vector2.Zero;
        var otherNormal = Vector2.Zero;
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;
        
        for (var i = 0; i < Count; i++)
        {
            var segment = this[i];
            
            var cp = Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.A, other.B, out float dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 0;
                result = cp;
                disSquared = dis;
                selfNormal = segment.Normal;
                otherNormal = other.B - other.A;
            }
            
            cp = Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.B, other.C, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 1;
                result = cp;
                disSquared = dis;
                selfNormal = segment.Normal;
                otherNormal = other.C - other.B;
            }
            
            cp = Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.C, other.D, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 2;
                result = cp;
                disSquared = dis;
                selfNormal = segment.Normal;
                otherNormal = other.D - other.C;
            }
            
            cp = Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.D, other.A, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 3;
                result = cp;
                disSquared = dis;
                selfNormal = segment.Normal;
                otherNormal = other.A - other.D;
            }
        
        }

        return new(
            new(result.self, selfNormal.GetPerpendicularRight().Normalize()), 
            new(result.other, otherNormal.GetPerpendicularRight().Normalize()),
            disSquared,
            selfIndex,
            otherIndex);
    }
    public ClosestPointResult GetClosestPoint(Rect other)
    {
        if (Count <= 2) return new();
        
        (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
        var selfNormal = Vector2.Zero;
        var otherNormal = Vector2.Zero;
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;
        
        for (var i = 0; i < Count; i++)
        {
            var segment = this[i];
            
            var cp = Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.A, other.B, out float dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 0;
                result = cp;
                disSquared = dis;
                selfNormal = segment.Normal;
                otherNormal = other.B - other.A;
            }
            
            cp = Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.B, other.C, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 1;
                result = cp;
                disSquared = dis;
                selfNormal = segment.Normal;
                otherNormal = other.C - other.B;
            }
            
            cp = Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.C, other.D, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 2;
                result = cp;
                disSquared = dis;
                selfNormal = segment.Normal;
                otherNormal = other.D - other.C;
            }
            
            cp = Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.D, other.A, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 3;
                result = cp;
                disSquared = dis;
                selfNormal = segment.Normal;
                otherNormal = other.A - other.D;
            }
        
        }

        return new(
            new(result.self, selfNormal.GetPerpendicularRight().Normalize()), 
            new(result.other, otherNormal.GetPerpendicularRight().Normalize()),
            disSquared,
            selfIndex,
            otherIndex);
    }
    public ClosestPointResult GetClosestPoint(Polygon other)
    {
        if (Count <= 2) return new();
        
        (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
        var selfNormal = Vector2.Zero;
        var otherNormal = Vector2.Zero;
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;
        
        for (var i = 0; i < Count; i++)
        {
            var segment = this[i];

            for (var j = 0; j < other.Count; j++)
            {
                var otherP1 = other[j];
                var otherP2 = other[(j + 1) % Count];
                var cp = Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, otherP1, otherP2, out float dis);
                if (dis < disSquared || disSquared < 0)
                {
                    selfIndex = i;
                    otherIndex = j;
                    result = cp;
                    disSquared = dis;
                    selfNormal = segment.Normal;
                    otherNormal = otherP2 - otherP1;
                }
            }
        
        }

        return new(
            new(result.self, selfNormal.GetPerpendicularRight().Normalize()), 
            new(result.other, otherNormal.GetPerpendicularRight().Normalize()),
            disSquared,
            selfIndex,
            otherIndex);
    }
    public ClosestPointResult GetClosestPoint(Polyline other)
    {
        if (Count <= 2) return new();
        
        (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
        var selfNormal = Vector2.Zero;
        var otherNormal = Vector2.Zero;
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;
        
        for (var i = 0; i < Count; i++)
        {
            var segment = this[i];

            for (var j = 0; j < other.Count - 1; j++)
            {
                var otherP1 = other[j];
                var otherP2 = other[j + 1];
                var cp = Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, otherP1, otherP2, out float dis);
                if (dis < disSquared || disSquared < 0)
                {
                    selfIndex = i;
                    otherIndex = j;
                    result = cp;
                    disSquared = dis;
                    selfNormal = segment.Normal;
                    otherNormal = otherP2 - otherP1;
                }
            }
        
        }

        return new(
            new(result.self, selfNormal.GetPerpendicularRight().Normalize()), 
            new(result.other, otherNormal.GetPerpendicularRight().Normalize()),
            disSquared,
            selfIndex,
            otherIndex);
    }
    public ClosestPointResult GetClosestPoint(Segments other)
    {
        if (Count <= 2) return new();
        
        (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
        var selfNormal = Vector2.Zero;
        var otherNormal = Vector2.Zero;
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;
        
        for (var i = 0; i < Count; i++)
        {
            var segment = this[i];

            for (var j = 0; j < other.Count; j++)
            {
                var otherSegment = other[j];
                var cp = Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, otherSegment.Start, otherSegment.End, out float dis);
                if (dis < disSquared || disSquared < 0)
                {
                    selfIndex = i;
                    otherIndex = j;
                    result = cp;
                    disSquared = dis;
                    selfNormal = segment.Normal;
                    otherNormal = otherSegment.Normal;
                }
            }
        
        }

        return new(
            new(result.self, selfNormal.GetPerpendicularRight().Normalize()), 
            new(result.other, otherNormal),
            disSquared,
            selfIndex,
            otherIndex);
    }

    public (Segment segment, CollisionPoint segmentPoint) GetClosestSegment(Vector2 p, out float disSquared)
    {
        disSquared = -1;
        if (Count <= 2) return (new(), new());

        var closestSegment = this[0];
        var closest = Segment.GetClosestPointSegmentPoint(closestSegment.Start, closestSegment.End, p, out disSquared);
        
        for (var i = 1; i < Count; i++)
        {
            var curSegment = this[i];
            
            var cp = Segment.GetClosestPointSegmentPoint(curSegment.Start, curSegment.End, p, out float dis);
            if (dis < disSquared)
            {
                closestSegment = curSegment;
                closest = cp;
                disSquared = dis;
            }
        
        }
        
        return new(closestSegment, new(closest, closestSegment.Normal));
    }
    
    #endregion
    
    #region Overlap
    public static bool OverlapSegmentsSegment(List<Segment> segments, Vector2 segmentStart, Vector2 segmentEnd)
    {
        return Segment.OverlapSegmentSegments(segmentStart, segmentEnd, segments);
    }
    public static bool OverlapSegmentsLine(List<Segment> segments, Vector2 linePoint, Vector2 lineDirection)
    {
        return Line.OverlapLineSegments(linePoint, lineDirection, segments);
    }
    public static bool OverlapSegmentsRay(List<Segment> segments, Vector2 rayPoint, Vector2 rayDirection)
    {
        return Ray.OverlapRaySegments(rayPoint, rayDirection, segments);
    }
    public static bool OverlapSegmentsCircle(List<Segment> segments, Vector2 circleCenter, float circleRadius)
    {
        return Circle.OverlapCircleSegments(circleCenter, circleRadius, segments);
    }
    public static bool OverlapSegmentsTriangle(List<Segment> segments, Vector2 ta, Vector2 tb, Vector2 tc)
    {
        return Triangle.OverlapTriangleSegments(ta, tb, tc, segments);

    }
    public static bool OverlapSegmentsQuad(List<Segment> segments, Vector2 qa, Vector2 qb, Vector2 qc, Vector2 qd)
    {
        return Quad.OverlapQuadSegments(qa, qb, qc, qd, segments);
    }
    public static bool OverlapSegmentsRect(List<Segment> segments, Vector2 ra, Vector2 rb, Vector2 rc, Vector2 rd)
    {
        return Quad.OverlapQuadSegments(ra, rb, rc, rd, segments);
    }
    public static bool OverlapSegmentsPolygon(List<Segment> segments, List<Vector2> points)
    {
        return Polygon.OverlapPolygonSegments(points, segments);
    }
    public static bool OverlapSegmentsPolyline(List<Segment> segments, List<Vector2> points)
    {
        return Polyline.OverlapPolylineSegments(points, segments);
    }
    public static bool OverlapSegmentsSegments(List<Segment> segments1, List<Segment> segments2)
    {
        foreach (var seg in segments1)
        {
            foreach (var bSeg in segments2)
            {
                if(Segment.OverlapSegmentSegment(seg.Start, seg.End, bSeg.Start, bSeg.End)) return true;
            }
            
        }
        return false;
    }

    public bool OverlapSegment(Vector2 segmentStart, Vector2 segmentEnd) => OverlapSegmentsSegment(this, segmentStart, segmentEnd);
    public bool OverlapLine(Vector2 linePoint, Vector2 lineDirection) => OverlapSegmentsLine(this, linePoint, lineDirection);
    public bool OverlapRay(Vector2 rayPoint, Vector2 rayDirection) => OverlapSegmentsRay(this, rayPoint, rayDirection);
    public bool OverlapCircle(Vector2 circleCenter, float circleRadius) => OverlapSegmentsCircle(this, circleCenter, circleRadius);
    public bool OverlapTriangle(Vector2 a, Vector2 b, Vector2 c) => OverlapSegmentsTriangle(this, a, b, c);
    public bool OverlapQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapSegmentsQuad(this, a, b, c, d);
    public bool OverlapRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapSegmentsQuad(this, a, b, c, d);
    public bool OverlapPolygon(List<Vector2> points) => OverlapSegmentsPolygon(this, points);
    public bool OverlapPolyline(List<Vector2> points) => OverlapSegmentsPolyline(this, points);
    public bool OverlapSegments(List<Segment> segments) => OverlapSegmentsSegments(this, segments);
    
    public bool OverlapShape(Line line) => OverlapSegmentsLine(this, line.Point, line.Direction);
    public bool OverlapShape(Ray ray) => OverlapSegmentsRay(this, ray.Point, ray.Direction);
    
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
    public CollisionPoints? IntersectShape(Ray r)
    {
        if(Count <= 0) return null;
        CollisionPoints? points = null;

        foreach (var seg in this)
        {
            var result = Segment.IntersectSegmentRay(seg.Start, seg.End, r.Point, r.Direction, r.Normal);
            if (result.Valid)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
        }
        return points;
    }
    public CollisionPoints? IntersectShape(Line l)
    {
        if(Count <= 0) return null;
        CollisionPoints? points = null;

        foreach (var seg in this)
        {
            var result = Segment.IntersectSegmentLine(seg.Start, seg.End, l.Point, l.Direction, l.Normal);
            if (result.Valid)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
        }
        return points;
    }
    public CollisionPoints? IntersectShape(Segment s)
    {
        if(Count <= 0) return null;
        CollisionPoints? points = null;

        foreach (var seg in this)
        {
            var result = Segment.IntersectSegmentSegment(seg.Start, seg.End, s.Start, s.End);
            if (result.Valid)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
        }
        return points;
    }
    
    public CollisionPoints? IntersectShape(Circle c)
    {
        if(Count <= 0) return null;
        CollisionPoints? points = null;
        foreach (var seg in this)
        {
            var result = Segment.IntersectSegmentCircle(seg.Start, seg.End, c.Center, c.Radius);
            if (result.a.Valid || result.b.Valid)
            {
                points ??= new();
                if(result.a.Valid) points.Add(result.a);
                if(result.b.Valid) points.Add(result.b);
            }
        }
        return points;
    }
    public CollisionPoints? IntersectShape(Segments shape)
    {
        if(Count <= 0) return null;
        CollisionPoints? points = null;

        foreach (var seg in this)
        {
            foreach (var bSeg in shape)
            {
                var result = Segment.IntersectSegmentSegment(seg.Start, seg.End, bSeg.Start, bSeg.End);
                if (result.Valid)
                {
                    points ??= new();
                    points.AddRange(result);
                }
            }
            
        }
        return points;
    }

    public int IntersectShape(Ray r, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count <= 0) return 0;
        var count = 0;

        foreach (var seg in this)
        {
            var result = Segment.IntersectSegmentRay(seg.Start, seg.End, r.Point, r.Direction, r.Normal);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }
        return count;
    }
    public int IntersectShape(Line l, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count <= 0) return 0;
        var count = 0;

        foreach (var seg in this)
        {
            var result = Segment.IntersectSegmentLine(seg.Start, seg.End, l.Point, l.Direction, l.Normal);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }
        return count;
    }
    public int IntersectShape(Segment s, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count <= 0) return 0;
        var count = 0;

        foreach (var seg in this)
        {
            var result = Segment.IntersectSegmentSegment(seg.Start, seg.End, s.Start, s.End);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }
        return count;
    }
    public int IntersectShape(Circle c, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if(Count <= 0) return 0;
        var count = 0;
        foreach (var seg in this)
        {
            var result = Segment.IntersectSegmentCircle(seg.Start, seg.End, c.Center, c.Radius);
            if (result.a.Valid)
            {
                points.Add(result.a);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            if (result.b.Valid)
            {
                points.Add(result.b);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }
        return count;
    }
    public int IntersectShape(Segments shape, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if(Count <= 0) return 0;
        var count = 0;

        foreach (var seg in this)
        {
            foreach (var bSeg in shape)
            {
                var result = Segment.IntersectSegmentSegment(seg.Start, seg.End, bSeg.Start, bSeg.End);
                if (result.Valid)
                {
                    points.Add(result);
                    if (returnAfterFirstValid) return 1;
                    count++;
                }
            }
            
        }
        return count;
    }
   
    #endregion

}