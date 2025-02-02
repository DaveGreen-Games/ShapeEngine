
using System.Numerics;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;
using ShapeEngine.Random;

namespace ShapeEngine.Core.Shapes;

public class Polyline : Points, IEquatable<Polyline>
{
    #region Constructors
    public Polyline() { }
    public Polyline(int capacity) : base(capacity) { }
    
    /// <summary>
    /// Points should be in CCW order. Use Reverse if they are in CW order.
    /// </summary>
    /// <param name="points"></param>
    public Polyline(IEnumerable<Vector2> points) { AddRange(points); }
    public Polyline(Points points) : base(points.Count) { AddRange(points); }
    public Polyline(Polyline polyLine) : base(polyLine.Count) { AddRange(polyLine); }
    public Polyline(Polygon poly) : base(poly.Count) { AddRange(poly); }
    #endregion

    #region Equals & HashCode
    public bool Equals(Polyline? other)
    {
        if (other == null) return false;
        if (Count != other.Count) return false;
        for (var i = 0; i < Count; i++)
        {
            if (!this[i].IsSimilar(other[i])) return false;
            //if (this[i] != other[i]) return false;
        }
        return true;
    }
    public override int GetHashCode() => Game.GetHashCode(this);

    #endregion
    
    #region Math
    public Points? GetProjectedShapePoints(Vector2 v)
    {
        if (v.LengthSquared() <= 0f) return null;
        var points = new Points(Count);
        for (var i = 0; i < Count; i++)
        {
            points.Add(this[i]);
            points.Add(this[i] + v);
        }
        return points;
    }
    public Polygon? ProjectShape(Vector2 v)
    {
        if (v.LengthSquared() <= 0f) return null;
        
        var points = new Points(Count);
        for (var i = 0; i < Count; i++)
        {
            points.Add(this[i]);
            points.Add(this[i] + v);
        }
        
        return Polygon.FindConvexHull(points);
    }
    
    public Vector2 GetCentroidOnLine()
    {
        return GetPoint(0.5f);
        // if (Count <= 0) return new(0f);
        // else if (Count == 1) return this[0];
        // float halfLengthSq = LengthSquared * 0.5f;
        // var segments = GetEdges();
        // float curLengthSq = 0f; 
        // foreach (var seg in segments)
        // {
        //     float segLengthSq = seg.LengthSquared;
        //     curLengthSq += segLengthSq;
        //     if (curLengthSq >= halfLengthSq)
        //     {
        //         float dif = curLengthSq - halfLengthSq;
        //         return seg.Center + seg.Dir * MathF.Sqrt(dif);
        //     }
        // }
        // return new Vector2();
    }
    public Vector2 GetCentroidMean()
    {
        if (Count <= 0) return new(0f);
        else if (Count == 1) return this[0];
        Vector2 total = new(0f);
        foreach (Vector2 p in this) { total += p; }
        return total / Count;
    }
    public Vector2 GetPoint(float f)
    {
        if (Count == 0) return new();
        if (Count == 1) return this[0];
        if (Count == 2) return this[0].Lerp(this[1], f);
        if (f <= 0f) return this[0];
        if (f >= 1f) return this[^1];
        
        var totalLengthSq = GetLengthSquared();
        var targetLengthSq = totalLengthSq * f;
        var curLengthSq = 0f;
        for (var i = 0; i < Count - 1; i++)
        {
            var start = this[i];
            var end = this[(i + 1) % Count];
            var lSq = (start - end).LengthSquared();
            if(lSq <= 0) continue;
            
            if (curLengthSq + lSq >= targetLengthSq)
            {
                var aF = curLengthSq / totalLengthSq;
                var bF = (curLengthSq + lSq) / totalLengthSq;
                var curF = ShapeMath.LerpInverseFloat(aF, bF, f);
                return start.Lerp(end, curF);
            }
            
            curLengthSq += lSq;
        }

        return new();
    }

    public float GetLength()
    {
        if (this.Count < 2) return 0f;
        var length = 0f;
        for (var i = 0; i < Count - 1; i++)
        {
            var w = this[i+1] - this[i];
            length += w.Length();
        }
        return length;
    }
    public float GetLengthSquared()
    {
        if (this.Count < 2) return 0f;
        var lengthSq = 0f;
        for (var i = 0; i < Count - 1; i++)
        {
            var w = this[i+1] - this[i];
            lengthSq += w.LengthSquared();
        }
        return lengthSq;
    }
    #endregion
    
    #region Shapes

    public Circle GetBoundingCircle()
    {
        float maxD = 0f;
        int num = this.Count;
        Vector2 origin = new();
        for (int i = 0; i < num; i++) { origin += this[i]; }
        origin *= (1f / num);
        for (int i = 0; i < num; i++)
        {
            float d = (origin - this[i]).LengthSquared();
            if (d > maxD) maxD = d;
        }

        return new Circle(origin, MathF.Sqrt(maxD));
    }
    public Rect GetBoundingBox()
    {
        if (Count < 2) return new();
        Vector2 start = this[0];
        Rect r = new(start.X, start.Y, 0, 0);

        foreach (var p in this)
        {
            r = r.Enlarge(p); // ShapeRect.Enlarge(r, p);
        }
        return r;
    }

    /// <summary>
    /// Return the segments of the polyline. If points are in ccw order the normals face to the right of the direction of the segments.
    /// If InsideNormals = true the normals face to the left of the direction of the segments.
    /// </summary>
    /// <returns></returns>
    public Segments GetEdges()
    {
        if (Count <= 1) return new();
        if (Count == 2) return new() { new(this[0], this[1]) };

        Segments segments = new();
        for (int i = 0; i < Count - 1; i++)
        {
            segments.Add(new(this[i], this[(i + 1) % Count]));
        }
        return segments;
    }
    
    public Points ToPoints() { return new(this); }

    #endregion
    
    #region Points & Vertex
    public Segment GetSegment(int index)
    { 
        if (index < 0) return new Segment();
        if (Count < 2) return new Segment();
        var first = index % (Count - 1);
        var second = first + 1;
        return new Segment(this[first], this[second]);
    }
    
    public Vector2 GetRandomVertex() { return Rng.Instance.RandCollection(this); }
    public Segment GetRandomEdge() => GetEdges().GetRandomSegment();
    #endregion

    #region Transform
    public void SetPosition(Vector2 newPosition)
    {
        var delta = newPosition - GetCentroidMean();
        ChangePosition(delta);
    }
    public void ChangeRotation(float rotRad)
    {
        if (Count < 2) return;
        var origin = GetCentroidMean();
        for (int i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w.Rotate(rotRad);
        }
    }
    public void SetRotation(float angleRad)
    {
        if (Count < 2) return;

        var origin = GetCentroidMean();
        var curAngle = (this[0] - origin).AngleRad();
        var rotRad = ShapeMath.GetShortestAngleRad(curAngle, angleRad);
        ChangeRotation(rotRad, origin);
    }
    public void ScaleSize(float scale)
    {
        if (Count < 2) return;
        var origin = GetCentroidMean();
        for (int i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w * scale;
        }
    }
    public void ChangeSize(float amount)
    {
        if (Count < 2) return;
        var origin = GetCentroidMean();
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w.ChangeLength(amount);
        }
        
    }
    public void SetSize(float size)
    {
        if (Count < 2) return;
        var origin = GetCentroidMean();
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w.SetLength(size);
        }

    }
    
    public Polyline? SetPositionCopy(Vector2 newPosition)
    {
        if (Count < 2) return null;
        var centroid = GetCentroidMean();
        var delta = newPosition - centroid;
        return ChangePositionCopy(delta);
    }
    public new Polyline? ChangePositionCopy(Vector2 offset)
    {
        if (Count < 2) return null;
        var newPolygon = new Polyline(this.Count);
        for (int i = 0; i < Count; i++)
        {
            newPolygon.Add(this[i] + offset);
        }

        return newPolygon;
    }
    public new Polyline? ChangeRotationCopy(float rotRad, Vector2 origin)
    {
        if (Count < 2) return null;
        var newPolygon = new Polyline(this.Count);
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolygon.Add(origin + w.Rotate(rotRad));
        }

        return newPolygon;
    }

    public Polyline? ChangeRotationCopy(float rotRad)
    {
        if (Count < 2) return null;
        return ChangeRotationCopy(rotRad, GetCentroidMean());
    }

    public new Polyline? SetRotationCopy(float angleRad, Vector2 origin)
    {
        if (Count < 2) return null;
        var curAngle = (this[0] - origin).AngleRad();
        var rotRad = ShapeMath.GetShortestAngleRad(curAngle, angleRad);
        return ChangeRotationCopy(rotRad, origin);
    }
    public Polyline? SetRotationCopy(float angleRad)
    {
        if (Count < 2) return null;

        var origin = GetCentroidMean();
        var curAngle = (this[0] - origin).AngleRad();
        var rotRad = ShapeMath.GetShortestAngleRad(curAngle, angleRad);
        return ChangeRotationCopy(rotRad, origin);
    }
    public Polyline? ScaleSizeCopy(float scale)
    {
        if (Count < 2) return null;
        return ScaleSizeCopy(scale, GetCentroidMean());
    }
    public new Polyline? ScaleSizeCopy(float scale, Vector2 origin)
    {
        if (Count < 2) return null;
        var newPolyline = new Polyline(this.Count);
        
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolyline.Add( origin + w * scale);
        }

        return newPolyline;
    }
    public new Polyline? ScaleSizeCopy(Vector2 scale, Vector2 origin)
    {
        if (Count < 2) return null;
        var newPolyline = new Polyline(this.Count);
        
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolyline.Add(origin + w * scale);
        }

        return newPolyline;
    }
    public new Polyline? ChangeSizeCopy(float amount, Vector2 origin)
    {
        if (Count < 2) return null;
        var newPolyline = new Polyline(this.Count);
        
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolyline.Add(origin + w.ChangeLength(amount));
        }

        return newPolyline;

    }
    public Polyline? ChangeSizeCopy(float amount)
    {
        if (Count < 3) return null;
        return ChangeSizeCopy(amount, GetCentroidMean());

    }

    public new Polyline? SetSizeCopy(float size, Vector2 origin)
    {
        if (Count < 2) return null;
        var newPolyline = new Polyline(this.Count);
        
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolyline.Add(origin + w.SetLength(size));
        }

        return newPolyline;
    }
    public Polyline? SetSizeCopy(float size)
    {
        if (Count < 2) return null;
        return SetSizeCopy(size, GetCentroidMean());

    }

    public new Polyline? SetTransformCopy(Transform2D transform, Vector2 origin)
    {
        if (Count < 2) return null;
        var newPolyline = SetPositionCopy(transform.Position);
        if (newPolyline == null) return null;
        newPolyline.SetRotation(transform.RotationRad, origin);
        newPolyline.SetSize(transform.ScaledSize.Length, origin);
        return newPolyline;
    }
    public new Polyline? ApplyOffsetCopy(Transform2D offset, Vector2 origin)
    {
        if (Count < 2) return null;
        
        var newPolyline = ChangePositionCopy(offset.Position);
        if (newPolyline == null) return null;
        newPolyline.ChangeRotation(offset.RotationRad, origin);
        newPolyline.ChangeSize(offset.ScaledSize.Length, origin);
        return newPolyline;
    }
    
    #endregion
    
    #region Closest Point
        public static Vector2 GetClosestPointPolylinePoint(List<Vector2> points, Vector2 p, out float disSquared)
        {
            disSquared = -1;
            if (points.Count < 2) return new();
            
            var first = points[0];
            var second = points[1];
            var closest = Segment.GetClosestPointSegmentPoint(first, second, p, out disSquared);
            
            for (var i = 1; i < points.Count - 1; i++)
            {
                var p1 = points[i];
                var p2 = points[i + 1];
                
                var cp = Segment.GetClosestPointSegmentPoint(p1, p2, p, out float dis);
                if (dis < disSquared)
                {
                    closest = cp;
                    disSquared = dis;
                }
            
            }
            return closest;
        }
        
        public new CollisionPoint GetClosestPoint(Vector2 p, out float disSquared)
        {
            disSquared = -1;
            if (Count < 2) return new();
            
            var first = this[0];
            var second = this[1];
            var normal = second - first;
            var closest = Segment.GetClosestPointSegmentPoint(first, second, p, out disSquared);
            
            for (var i = 1; i < Count - 1; i++)
            {
                var p1 = this[i];
                var p2 = this[i + 1];
                
                var cp = Segment.GetClosestPointSegmentPoint(p1, p2, p, out float dis);
                if (dis < disSquared)
                {
                    closest = cp;
                    disSquared = dis;
                    normal = p2 - p1;
                }
            
            }
            return new(closest, normal.GetPerpendicularRight().Normalize());
        }
        public new CollisionPoint GetClosestPoint(Vector2 p, out float disSquared, out int index)
        {
            disSquared = -1;
            index = -1;
            if (Count < 2) return new();
            
            var first = this[0];
            var second = this[1];
            index = 0;
            var normal = second - first;
            var closest = Segment.GetClosestPointSegmentPoint(first, second, p, out disSquared);
            
            for (var i = 1; i < Count - 1; i++)
            {
                var p1 = this[i];
                var p2 = this[i + 1];
                
                var cp = Segment.GetClosestPointSegmentPoint(p1, p2, p, out float dis);
                if (dis < disSquared)
                {
                    index = i;
                    normal = p2 - p1;
                    closest = cp;
                    disSquared = dis;
                }
            
            }
            return new(closest, normal.GetPerpendicularRight().Normalize());
        }
        public new Vector2 GetClosestVertex(Vector2 p, out float disSquared, out int index)
        {
            disSquared = -1;
            index = -1;
            if (Count < 2) return new();
            
            index = 0;
            var closest = this[index];
            disSquared = (closest - p).LengthSquared();
            
            for (var i = 1; i < Count; i++)
            {
                var cp = this[i];
                var dis = (cp - p).LengthSquared();
                if (dis < disSquared)
                {
                    index = i;
                    closest = cp;
                    disSquared = dis;
                }
            }
            return closest;
        }
        public new Vector2 GetFurthestVertex(Vector2 p, out float disSquared, out int index)
        {
            disSquared = -1;
            index = -1;
            if (Count < 2) return new();
            
            index = 0;
            var furthest = this[index];
            disSquared = (furthest - p).LengthSquared();
            
            for (var i = 1; i < Count; i++)
            {
                var cp = this[i];
                var dis = (cp - p).LengthSquared();
                if (dis > disSquared)
                {
                    index = i;
                    furthest = cp;
                    disSquared = dis;
                }
            }
            return furthest;
        }

        public new ClosestPointResult GetClosestPoint(Line other)
        {
            if (Count < 2) return new();
            var first = this[0];
            var second = this[1];
            var normal = second - first;
            var result = Segment.GetClosestPointSegmentLine(first, second, other.Point, other.Direction, out float disSquared);
            var selfIndex = 0;
            
            for (var i = 1; i < Count - 1; i++)
            {
                var p1 = this[i];
                var p2 = this[i + 1];
                
                var cp = Segment.GetClosestPointSegmentLine(p1, p2, other.Point, other.Direction, out float dis);
                if (dis < disSquared)
                {
                    selfIndex = i;
                    result = cp;
                    disSquared = dis;
                    normal = p2 - p1;
                }
            
            }

            return new(
                new(result.self, normal.GetPerpendicularRight().Normalize()), 
                new(result.other, other.Normal),
                disSquared,
                selfIndex);
        }
        public new ClosestPointResult GetClosestPoint(Ray other)
        {
            if (Count < 2) return new();
            
            var first = this[0];
            var second = this[1];
            var normal = second - first;
            var result = Segment.GetClosestPointSegmentRay(first, second, other.Point, other.Direction, out float disSquared);
            var selfIndex = 0;
            
            for (var i = 1; i < Count - 1; i++)
            {
                var p1 = this[i];
                var p2 = this[i + 1];
                
                var cp = Segment.GetClosestPointSegmentRay(p1, p2, other.Point, other.Direction, out float dis);
                if (dis < disSquared)
                {
                    selfIndex = i;
                    result = cp;
                    disSquared = dis;
                    normal = p2 - p1;
                }
            
            }

            return new(
                new(result.self, normal.GetPerpendicularRight().Normalize()), 
                new(result.other, other.Normal),
                disSquared,
                selfIndex);
        }
        public new ClosestPointResult GetClosestPoint(Segment other) 
        {
            if (Count < 2) return new();
            
            var first = this[0];
            var second = this[1];
            var normal = second - first;
            var result = Segment.GetClosestPointSegmentSegment(first, second, other.Start, other.End, out float disSquared);
            var selfIndex = 0;
            
            for (var i = 1; i < Count - 1; i++)
            {
                var p1 = this[i];
                var p2 = this[i + 1];
                
                var cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.Start, other.End, out float dis);
                if (dis < disSquared)
                {
                    selfIndex = i;
                    result = cp;
                    disSquared = dis;
                    normal = p2 - p1;
                }
            
            }

            return new (
                new(result.self, normal.GetPerpendicularRight().Normalize()), 
                new(result.other, other.Normal),
                disSquared,
                selfIndex);
        }
        public new ClosestPointResult GetClosestPoint(Circle other)
        {
            if (Count < 2) return new();
            
            var first = this[0];
            var second = this[1];
            var normal = second - first;
            var result = Segment.GetClosestPointSegmentCircle(first, second, other.Center, other.Radius, out float disSquared);
            var selfIndex = 0;
            
            for (var i = 1; i < Count - 1; i++)
            {
                var p1 = this[i];
                var p2 = this[i + 1];
                
                var cp = Segment.GetClosestPointSegmentCircle(p1, p2, other.Center, other.Radius, out float dis);
                if (dis < disSquared)
                {
                    selfIndex = i;
                    result = cp;
                    disSquared = dis;
                    normal = p2 - p1;
                }
            }

            return new (
                new(result.self, normal.GetPerpendicularRight().Normalize()), 
                new(result.other, (result.other - other.Center).Normalize()),
                disSquared,
                selfIndex
            );
        }
        public new ClosestPointResult GetClosestPoint(Triangle other)
        {
            if (Count < 2) return new();
            
            (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
            var selfNormal = Vector2.Zero;
            var otherNormal = Vector2.Zero;
            int selfIndex = -1;
            int otherIndex = -1;
            float disSquared = -1f;
            
            for (var i = 0; i < Count - 1; i++)
            {
                var p1 = this[i];
                var p2 = this[i + 1];
                
                var cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.A, other.B, out float dis);
                if (dis < disSquared || disSquared < 0)
                {
                    selfIndex = i;
                    otherIndex = 0;
                    result = cp;
                    disSquared = dis;
                    selfNormal = p2 - p1;
                    otherNormal = other.B - other.A;
                }
                
                cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.B, other.C, out dis);
                if (dis < disSquared || disSquared < 0)
                {
                    selfIndex = i;
                    otherIndex = 1;
                    result = cp;
                    disSquared = dis;
                    selfNormal = p2 - p1;
                    otherNormal = other.C - other.B;
                }
                
                cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.C, other.A, out dis);
                if (dis < disSquared || disSquared < 0)
                {
                    selfIndex = i;
                    otherIndex = 2;
                    result = cp;
                    disSquared = dis;
                    selfNormal = p2 - p1;
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
        public new ClosestPointResult GetClosestPoint(Quad other)
        {
            if (Count < 2) return new();
            
            (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
            var selfNormal = Vector2.Zero;
            var otherNormal = Vector2.Zero;
            int selfIndex = -1;
            int otherIndex = -1;
            float disSquared = -1f;
            
            for (var i = 0; i < Count - 1; i++)
            {
                var p1 = this[i];
                var p2 = this[i + 1];
                
                var cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.A, other.B, out float dis);
                if (dis < disSquared || disSquared < 0)
                {
                    selfIndex = i;
                    otherIndex = 0;
                    result = cp;
                    disSquared = dis;
                    selfNormal = p2 - p1;
                    otherNormal = other.B - other.A;
                }
                
                cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.B, other.C, out dis);
                if (dis < disSquared || disSquared < 0)
                {
                    selfIndex = i;
                    otherIndex = 1;
                    result = cp;
                    disSquared = dis;
                    selfNormal = p2 - p1;
                    otherNormal = other.C - other.B;
                }
                
                cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.C, other.D, out dis);
                if (dis < disSquared || disSquared < 0)
                {
                    selfIndex = i;
                    otherIndex = 2;
                    result = cp;
                    disSquared = dis;
                    selfNormal = p2 - p1;
                    otherNormal = other.D - other.C;
                }
                
                cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.D, other.A, out dis);
                if (dis < disSquared || disSquared < 0)
                {
                    selfIndex = i;
                    otherIndex = 3;
                    result = cp;
                    disSquared = dis;
                    selfNormal = p2 - p1;
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
        public new ClosestPointResult GetClosestPoint(Rect other)
        {
            if (Count < 2) return new();
            
            (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
            var selfNormal = Vector2.Zero;
            var otherNormal = Vector2.Zero;
            int selfIndex = -1;
            int otherIndex = -1;
            float disSquared = -1f;
            
            for (var i = 0; i < Count - 1; i++)
            {
                var p1 = this[i];
                var p2 = this[i + 1];
                
                var cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.A, other.B, out float dis);
                if (dis < disSquared || disSquared < 0)
                {
                    selfIndex = i;
                    otherIndex = 0;
                    result = cp;
                    disSquared = dis;
                    selfNormal = p2 - p1;
                    otherNormal = other.B - other.A;
                }
                
                cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.B, other.C, out dis);
                if (dis < disSquared || disSquared < 0)
                {
                    selfIndex = i;
                    otherIndex = 1;
                    result = cp;
                    disSquared = dis;
                    selfNormal = p2 - p1;
                    otherNormal = other.C - other.B;
                }
                
                cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.C, other.D, out dis);
                if (dis < disSquared || disSquared < 0)
                {
                    selfIndex = i;
                    otherIndex = 2;
                    result = cp;
                    disSquared = dis;
                    selfNormal = p2 - p1;
                    otherNormal = other.D - other.C;
                }
                
                cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.D, other.A, out dis);
                if (dis < disSquared || disSquared < 0)
                {
                    selfIndex = i;
                    otherIndex = 3;
                    result = cp;
                    disSquared = dis;
                    selfNormal = p2 - p1;
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
        public new ClosestPointResult GetClosestPoint(Polygon other)
        {
            if (Count < 2) return new();
            
            (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
            var selfNormal = Vector2.Zero;
            var otherNormal = Vector2.Zero;
            int selfIndex = -1;
            int otherIndex = -1;
            float disSquared = -1f;
            
            for (var i = 0; i < Count - 1; i++)
            {
                var p1 = this[i];
                var p2 = this[i + 1];

                for (var j = 0; j < other.Count; j++)
                {
                    var otherP1 = other[j];
                    var otherP2 = other[(j + 1) % other.Count];
                    var cp = Segment.GetClosestPointSegmentSegment(p1, p2, otherP1, otherP2, out float dis);
                    if (dis < disSquared || disSquared < 0)
                    {
                        selfIndex = i;
                        otherIndex = j;
                        result = cp;
                        disSquared = dis;
                        selfNormal = p2 - p1;
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
        public new ClosestPointResult GetClosestPoint(Polyline other)
        {
            if (Count < 2) return new();
            
            (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
            var selfNormal = Vector2.Zero;
            var otherNormal = Vector2.Zero;
            int selfIndex = -1;
            int otherIndex = -1;
            float disSquared = -1f;
            
            for (var i = 0; i < Count - 1; i++)
            {
                var p1 = this[i];
                var p2 = this[i + 1];

                for (var j = 0; j < other.Count - 1; j++)
                {
                    var otherP1 = other[j];
                    var otherP2 = other[j + 1];
                    var cp = Segment.GetClosestPointSegmentSegment(p1, p2, otherP1, otherP2, out float dis);
                    if (dis < disSquared || disSquared < 0)
                    {
                        selfIndex = i;
                        otherIndex = j;
                        result = cp;
                        disSquared = dis;
                        selfNormal = p2 - p1;
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
        public new ClosestPointResult GetClosestPoint(Segments other)
        {
            if (Count < 2) return new();
            
            (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
            var selfNormal = Vector2.Zero;
            var otherNormal = Vector2.Zero;
            int selfIndex = -1;
            int otherIndex = -1;
            float disSquared = -1f;
            
            for (var i = 0; i < Count - 1; i++)
            {
                var p1 = this[i];
                var p2 = this[i + 1];

                for (var j = 0; j < other.Count; j++)
                {
                    var otherSegment = other[j];
                    var cp = Segment.GetClosestPointSegmentSegment(p1, p2, otherSegment.Start, otherSegment.End, out float dis);
                    if (dis < disSquared || disSquared < 0)
                    {
                        selfIndex = i;
                        otherIndex = j;
                        result = cp;
                        disSquared = dis;
                        selfNormal = p2 - p1;
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
            if (Count < 2) return (new(), new());
            
            var closestSegment = new Segment(this[0], this[1]);
            var closest = closestSegment.GetClosestPoint(p, out disSquared);
            
            for (var i = 1; i < Count - 1; i++)
            {
                var p1 = this[i];
                var p2 = this[i + 1];
                
                var cp = Segment.GetClosestPointSegmentPoint(p1, p2, p, out float dis);
                if (dis < disSquared)
                {
                    var normal = (p2 - p1).GetPerpendicularRight().Normalize();
                    closest = new(cp, normal);
                    closestSegment = new Segment(p1, p2);
                    disSquared = dis;
                }
            
            }
            
            return new(closestSegment, closest);
        }
        
        
        #endregion
        
    #region Overlap
    public static bool OverlapPolylineSegment(List<Vector2> points, Vector2 segmentStart, Vector2 segmentEnd)
    {
        return Segment.OverlapSegmentPolyline(segmentStart, segmentEnd, points);
    }
    public static bool OverlapPolylineLine(List<Vector2> points, Vector2 linePoint, Vector2 lineDirection)
    {
        return Line.OverlapLinePolyline(linePoint, lineDirection, points);
    }
    public static bool OverlapPolylineRay(List<Vector2> points, Vector2 rayPoint, Vector2 rayDirection)
    {
        return Ray.OverlapRayPolyline(rayPoint, rayDirection, points);
    }
    public static bool OverlapPolylineCircle(List<Vector2> points, Vector2 circleCenter, float circleRadius)
    {
        return Circle.OverlapCirclePolyline(circleCenter, circleRadius, points);
    }
    public static bool OverlapPolylineTriangle(List<Vector2> points, Vector2 ta, Vector2 tb, Vector2 tc)
    {
        return Triangle.OverlapTrianglePolyline(ta, tb, tc, points);

    }
    public static bool OverlapPolylineQuad(List<Vector2> points, Vector2 qa, Vector2 qb, Vector2 qc, Vector2 qd)
    {
        return Quad.OverlapQuadPolyline(qa, qb, qc, qd, points);
    }
    public static bool OverlapPolylineRect(List<Vector2> points, Vector2 ra, Vector2 rb, Vector2 rc, Vector2 rd)
    {
        return Quad.OverlapQuadPolyline(ra, rb, rc, rd, points);
    }
    public static bool OverlapPolylinePolygon(List<Vector2> points1, List<Vector2> points2)
    {
        return Polygon.OverlapPolygonPolyline(points2, points1);
    }
    public static bool OverlapPolylinePolyline(List<Vector2> points1, List<Vector2> points2)
    {
        
        if (points1.Count < 2 || points2.Count < 2) return false;
        
        for (var i = 0; i < points1.Count - 1; i++)
        {
            var start = points1[i];
            var end = points1[i + 1];

            for (var j = 0; j < points2.Count - 1; j++)
            {
                var bStart = points2[j];
                var bEnd = points2[j + 1];

                if (Segment.OverlapSegmentSegment(start, end, bStart, bEnd)) return true;
            }
        }

        return false;
    }
    public static bool OverlapPolylineSegments(List<Vector2> points, List<Segment> segments)
    {
        if (points.Count < 2 || segments.Count <= 0) return false;
        
        for (var i = 0; i < points.Count - 1; i++)
        {
            var start = points[i];
            var end = points[i + 1];

            foreach (var seg in segments)
            {
                if (Segment.OverlapSegmentSegment(start, end, seg.Start, seg.End)) return true;
            }
        }

        return false;
    }

    public bool OverlapPoint(Vector2 p)
    {
        var segments = GetEdges();
        foreach (var segment in segments)
        {
            if (segment.OverlapPoint(p)) return true;
        }
        return false;
    }
    public bool OverlapSegment(Vector2 segmentStart, Vector2 segmentEnd) => OverlapPolylineSegment(this, segmentStart, segmentEnd);
    public bool OverlapLine(Vector2 linePoint, Vector2 lineDirection) => OverlapPolylineLine(this, linePoint, lineDirection);
    public bool OverlapRay(Vector2 rayPoint, Vector2 rayDirection) => OverlapPolylineRay(this, rayPoint, rayDirection);
    public bool OverlapCircle(Vector2 circleCenter, float circleRadius) => OverlapPolylineCircle(this, circleCenter, circleRadius);
    public bool OverlapTriangle(Vector2 a, Vector2 b, Vector2 c) => OverlapPolylineTriangle(this, a, b, c);
    public bool OverlapQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapPolylineQuad(this, a, b, c, d);
    public bool OverlapRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapPolylineQuad(this, a, b, c, d);
    public bool OverlapPolygon(List<Vector2> points) => OverlapPolylinePolygon(this, points);
    public bool OverlapPolyline(List<Vector2> points) => OverlapPolylinePolyline(this, points);
    public bool OverlapSegments(List<Segment> segments) => OverlapPolylineSegments(this, segments);
    
    public bool OverlapShape(Line line) => OverlapPolylineLine(this, line.Point, line.Direction);
    public bool OverlapShape(Ray ray) => OverlapPolylineRay(this, ray.Point, ray.Direction);
    public bool Overlap(Collider collider)
    {
        if (!collider.Enabled) return false;

        switch (collider.GetShapeType())
        {
            case ShapeType.Circle:
                var c = collider.GetCircleShape();
                return OverlapShape(c);
            case ShapeType.Segment:
                var s = collider.GetSegmentShape();
                return OverlapShape(s);
            case ShapeType.Ray:
                var rayShape = collider.GetRayShape();
                return OverlapShape(rayShape);
            case ShapeType.Line:
                var l = collider.GetLineShape();
                return OverlapShape(l);
            case ShapeType.Triangle:
                var t = collider.GetTriangleShape();
                return OverlapShape(t);
            case ShapeType.Rect:
                var r = collider.GetRectShape();
                return OverlapShape(r);
            case ShapeType.Quad:
                var q = collider.GetQuadShape();
                return OverlapShape(q);
            case ShapeType.Poly:
                var p = collider.GetPolygonShape();
                return OverlapShape(p);
            case ShapeType.PolyLine:
                var pl = collider.GetPolylineShape();
                return OverlapShape(pl);
        }

        return false;
    }
    public bool OverlapShape(Segments segments)
    {
        if (Count < 2 || segments.Count <= 0) return false;
        
        for (var i = 0; i < Count - 1; i++)
        {
            var start = this[i];
            var end = this[(i + 1) % Count];

            foreach (var seg in segments)
            {
                if (Segment.OverlapSegmentSegment(start, end, seg.Start, seg.End)) return true;
            }
        }

        return false;
    }
    public bool OverlapShape(Segment s) => s.OverlapShape(this);
    public bool OverlapShape(Circle c) => c.OverlapShape(this);
    public bool OverlapShape(Triangle t) => t.OverlapShape(this);
    public bool OverlapShape(Rect r) => r.OverlapShape(this);
    public bool OverlapShape(Quad q) => q.OverlapShape(this);
    public bool OverlapShape(Polygon p) => p.OverlapShape(this);
    public bool OverlapShape(Polyline b)
    {
        if (Count < 2 || b.Count < 2) return false;
        
        for (var i = 0; i < Count - 1; i++)
        {
            var start = this[i];
            var end = this[i + 1];

            for (var j = 0; j < b.Count - 1; j++)
            {
                var bStart = b[j];
                var bEnd = b[j + 1];

                if (Segment.OverlapSegmentSegment(start, end, bStart, bEnd)) return true;
            }
        }

        return false;
    }
    #endregion

    #region Intersection
    public CollisionPoints? Intersect(Collider collider)
    {
        if (!collider.Enabled) return null;

        switch (collider.GetShapeType())
        {
            case ShapeType.Circle:
                var c = collider.GetCircleShape();
                return IntersectShape(c);
            case ShapeType.Ray:
                var rayShape = collider.GetRayShape();
                return IntersectShape(rayShape);
            case ShapeType.Line:
                var l = collider.GetLineShape();
                return IntersectShape(l);
            case ShapeType.Segment:
                var s = collider.GetSegmentShape();
                return IntersectShape(s);
            case ShapeType.Triangle:
                var t = collider.GetTriangleShape();
                return IntersectShape(t);
            case ShapeType.Rect:
                var r = collider.GetRectShape();
                return IntersectShape(r);
            case ShapeType.Quad:
                var q = collider.GetQuadShape();
                return IntersectShape(q);
            case ShapeType.Poly:
                var p = collider.GetPolygonShape();
                return IntersectShape(p);
            case ShapeType.PolyLine:
                var pl = collider.GetPolylineShape();
                return IntersectShape(pl);
        }

        return null;
    }

    public CollisionPoints? IntersectShape(Ray ray)
    {
        if (Count < 2) return null;

        CollisionPoints? points = null;
        for (var i = 0; i < Count - 1; i++)
        {
            var result = Segment.IntersectSegmentRay(this[i], this[i + 1], ray.Point, ray.Direction, ray.Normal);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
        }
        return points;
    }

    public CollisionPoints? IntersectShape(Line l)
    {
        if (Count < 2) return null;

        CollisionPoints? points = null;
        for (var i = 0; i < Count - 1; i++)
        {
            var result = Segment.IntersectSegmentLine(this[i], this[i + 1], l.Point, l.Direction, l.Normal);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
        }
        return points;
    }

    public CollisionPoints? IntersectShape(Segment s)
    {
        if (Count < 2) return null;

        CollisionPoints? points = null;
        for (var i = 0; i < Count - 1; i++)
        {
            var result = Segment.IntersectSegmentSegment(this[i], this[i + 1], s.Start, s.End);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
        }
        return points;
    }
    
    public CollisionPoints? IntersectShape(Circle c)
    {
        if (Count < 2) return null;
        
        CollisionPoints? points = null;

        for (var i = 0; i < Count - 1; i++)
        {
            var result = Segment.IntersectSegmentCircle(this[i], this[i + 1], c.Center, c.Radius);
            if (result.a.Valid || result.b.Valid)
            {
                points ??= new();
                if(result.a.Valid) points.Add(result.a);
                if(result.b.Valid) points.Add(result.b);
            }
            
        }
        return points;
    }
    public CollisionPoints? IntersectShape(Triangle t)
    {
        if (Count < 2) return null;

        CollisionPoints? points = null;
        for (var i = 0; i < Count - 1; i++)
        {
            var p1 = this[i];
            var p2 = this[i + 1];
            var result = Segment.IntersectSegmentSegment(p1, p2, t.A, t.B);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
            result = Segment.IntersectSegmentSegment(p1, p2, t.B, t.C);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
            result = Segment.IntersectSegmentSegment(p1, p2, t.C, t.A);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
        }
        return points;
    }
    public CollisionPoints? IntersectShape(Rect r)
    {
        if (Count < 2) return null;

        CollisionPoints? points = null;
        var a = r.TopLeft;
        var b = r.BottomLeft;
        var c = r.BottomRight;
        var d = r.TopRight;
        for (var i = 0; i < Count - 1; i++)
        {
            var p1 = this[i];
            var p2 = this[i + 1];
            var result = Segment.IntersectSegmentSegment(p1, p2, a, b);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
            result = Segment.IntersectSegmentSegment(p1, p2, b, c);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
            result = Segment.IntersectSegmentSegment(p1, p2, c, d);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
            result = Segment.IntersectSegmentSegment(p1, p2, d, a);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
        }
        return points;
    }
    public CollisionPoints? IntersectShape(Quad q)
    {
        if (Count < 2) return null;

        CollisionPoints? points = null;
        for (var i = 0; i < Count - 1; i++)
        {
            var p1 = this[i];
            var p2 = this[i + 1];
            var result = Segment.IntersectSegmentSegment(p1, p2, q.A, q.B);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
            result = Segment.IntersectSegmentSegment(p1, p2, q.B, q.C);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
            result = Segment.IntersectSegmentSegment(p1, p2, q.C, q.D);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
            result = Segment.IntersectSegmentSegment(p1, p2, q.D, q.A);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
        }
        return points;
    }
    public CollisionPoints? IntersectShape(Polygon p)
    {
        if (p.Count < 3 || Count < 2) return null;
        CollisionPoints? points = null;
        for (var i = 0; i < Count - 1; i++)
        {
            for (var j = 0; j < p.Count; j++)
            {
                var result = Segment.IntersectSegmentSegment(this[i], this[i + 1],p[j], p[(j + 1) % p.Count]);
                if (result.Valid)
                {
                    points ??= new();
                    points.Add(result);
                }
            }
            
        }
        
        return points;
    }
    public CollisionPoints? IntersectShape(Polyline pl)
    {
        if (pl.Count < 2 || Count < 2) return null;
        CollisionPoints? points = null;
        for (var i = 0; i < Count - 1; i++)
        {
            for (var j = 0; j < pl.Count - 1; j++)
            {
                var result = Segment.IntersectSegmentSegment(this[i], this[i + 1],pl[j], pl[j + 1]);
                if (result.Valid)
                {
                    points ??= new();
                    points.Add(result);
                }
            }
            
        }
        
        return points;
    }
    public CollisionPoints? IntersectShape(Segments segments)
    {
        if (Count < 2 || segments.Count <= 0) return null;
        CollisionPoints? points = null;
        for (var i = 0; i < Count - 1; i++)
        {
            foreach (var seg in segments)
            {
                var result = Segment.IntersectSegmentSegment(this[i], this[i + 1],seg.Start, seg.End);
                if (result.Valid)
                {
                    points ??= new();
                    points.Add(result);
                }
            }
        }
        return points;
    }
    
    
    public int Intersect(Collider collider, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (!collider.Enabled) return 0;

        switch (collider.GetShapeType())
        {
            case ShapeType.Circle:
                var c = collider.GetCircleShape();
                return IntersectShape(c, ref points, returnAfterFirstValid);
            case ShapeType.Ray:
                var rayShape = collider.GetRayShape();
                return IntersectShape(rayShape, ref points);
            case ShapeType.Line:
                var l = collider.GetLineShape();
                return IntersectShape(l, ref points);
            case ShapeType.Segment:
                var s = collider.GetSegmentShape();
                return IntersectShape(s, ref points);
            case ShapeType.Triangle:
                var t = collider.GetTriangleShape();
                return IntersectShape(t, ref points, returnAfterFirstValid);
            case ShapeType.Rect:
                var r = collider.GetRectShape();
                return IntersectShape(r, ref points, returnAfterFirstValid);
            case ShapeType.Quad:
                var q = collider.GetQuadShape();
                return IntersectShape(q, ref points, returnAfterFirstValid);
            case ShapeType.Poly:
                var p = collider.GetPolygonShape();
                return IntersectShape(p, ref points, returnAfterFirstValid);
            case ShapeType.PolyLine:
                var pl = collider.GetPolylineShape();
                return IntersectShape(pl, ref points, returnAfterFirstValid);
        }

        return 0;
    }
    public int IntersectShape(Ray r, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 2) return 0;

        var count = 0;
        for (var i = 0; i < Count - 1; i++)
        {
            var result = Segment.IntersectSegmentRay(this[i], this[(i + 1) % Count], r.Point, r.Direction, r.Normal);
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
        if (Count < 2) return 0;

        var count = 0;
        for (var i = 0; i < Count - 1; i++)
        {
            var result = Segment.IntersectSegmentLine(this[i], this[(i + 1) % Count], l.Point, l.Direction, l.Normal);
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
        if (Count < 2) return 0;

        var count = 0;
        for (var i = 0; i < Count - 1; i++)
        {
            var result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], s.Start, s.End);
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
        if (Count < 2) return 0;

        var count = 0;

        for (var i = 0; i < Count - 1; i++)
        {
            var result = Segment.IntersectSegmentCircle(this[i], this[(i + 1) % Count], c.Center, c.Radius);
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
    public int IntersectShape(Triangle t, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 2) return 0;

        var count = 0;
        for (var i = 0; i < Count - 1; i++)
        {
            var result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], t.A, t.B);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
            result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], t.B, t.C);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
            result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], t.C, t.A);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
        }
        return count;
    }
    public int IntersectShape(Quad q, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 2) return 0;

        var count = 0;
        for (var i = 0; i < Count - 1; i++)
        {
            var result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], q.A, q.B);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
            result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], q.B, q.C);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
            result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], q.C, q.D);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
            result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], q.D, q.A);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }
        return count;
    }
    public int IntersectShape(Rect r, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 2) return 0;

        var count = 0;
        var a = r.TopLeft;
        var b = r.BottomLeft;
        var c = r.BottomRight;
        var d = r.TopRight;
        for (var i = 0; i < Count - 1; i++)
        {
            var result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], a, b);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
            result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], b, c);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
            result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], c, d);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
            result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], d, a);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
        }
        return count;
    }
    public int IntersectShape(Polygon p, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (p.Count < 3 || Count < 2) return 0;
        var count = 0;
        for (var i = 0; i < Count - 1; i++)
        {
            for (var j = 0; j < p.Count; j++)
            {
                var result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count],p[j], p[(j + 1) % p.Count]);
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
    public int IntersectShape(Polyline pl, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (pl.Count < 2 || Count < 2) return 0;
        var count = 0;
        for (var i = 0; i < Count - 1; i++)
        {
            for (var j = 0; j < pl.Count - 1; j++)
            {
                var result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count],pl[j], pl[(j + 1) % pl.Count]);
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
    public int IntersectShape(Segments shape, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 2 || shape.Count <= 0) return 0;
        var count = 0;
        for (var i = 0; i < Count - 1; i++)
        {
            foreach (var seg in shape)
            {
                var result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count],seg.Start, seg.End);
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

    #region Static
    public static Polyline GetShape(Points relative, Transform2D transform)
    {
        if (relative.Count < 3) return new();
        Polyline shape = new();
        for (int i = 0; i < relative.Count; i++)
        {
            shape.Add(transform.ApplyTransformTo(relative[i]));
            // shape.Add(transform.Position + relative[i].Rotate(transform.RotationRad) * transform.Scale);
        }
        return shape;
    }

    // public static Polyline Center(Polyline p, Vector2 newCenter)
    // {
    //     var centroid = p.GetCentroidMean();
    //     var delta = newCenter - centroid;
    //     return Move(p, delta);
    // }
    // public static Polyline Move(Polyline p, Vector2 translation)
    // {
    //     var result = new Polyline();
    //     for (int i = 0; i < p.Count; i++)
    //     {
    //         result.Add(p[i] + translation);
    //     }
    //     return result;
    // }
    #endregion
}


