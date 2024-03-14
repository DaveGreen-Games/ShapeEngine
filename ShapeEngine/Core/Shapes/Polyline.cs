
using System.Numerics;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Shapes
{
    public class Polyline : Points, IEquatable<Polyline>
    {
        #region Constructors
        public Polyline() { }
        
        /// <summary>
        /// Points should be in CCW order. Use Reverse if they are in CW order.
        /// </summary>
        /// <param name="edges"></param>
        public Polyline(IEnumerable<Vector2> edges) { AddRange(edges); }
        
        public Polyline(Polyline polyLine) { AddRange(polyLine); }
        public Polyline(Polygon poly) { AddRange(poly); }
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

        #region Getter Setter
        /// <summary>
        /// Flips the calculated normals for each segment. 
        /// false means default is used. (facing right)
        /// </summary>

        public float Length => MathF.Sqrt(LengthSquared);
        public float LengthSquared
        {
            get
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
        }
        #endregion

        #region Public
        public Polygon Project(Vector2 v)
        {
            if (v.LengthSquared() <= 0f) return ToPolygon();
            var translated = Move(this, v);
            var points = new Points();
            points.AddRange(this);
            points.AddRange(translated);
            return Polygon.FindConvexHull(points);
        }
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
        
        public Vector2 GetCentroidOnLine()
        {
            if (Count <= 0) return new(0f);
            else if (Count == 1) return this[0];
            float halfLengthSq = LengthSquared * 0.5f;
            var segments = GetEdges();
            float curLengthSq = 0f; 
            foreach (var seg in segments)
            {
                float segLengthSq = seg.LengthSquared;
                curLengthSq += segLengthSq;
                if (curLengthSq >= halfLengthSq)
                {
                    float dif = curLengthSq - halfLengthSq;
                    return seg.Center + seg.Dir * MathF.Sqrt(dif);
                }
            }
            return new Vector2();
        }
        public Vector2 GetCentroidMean()
        {
            if (Count <= 0) return new(0f);
            else if (Count == 1) return this[0];
            Vector2 total = new(0f);
            foreach (Vector2 p in this) { total += p; }
            return total / Count;
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

        public Vector2 GetRandomVertex() { return ShapeRandom.RandCollection(this); }
        public Segment GetRandomEdge() => GetEdges().GetRandomSegment();
        //public Vector2 GetRandomPoint() => GetRandomEdge().GetRandomPoint();
        //public Points GetRandomPoints(int amount) => GetEdges().GetRandomPoints(amount);

        #endregion

        #region Closest
        public ClosestDistance GetClosestDistanceTo(Vector2 p)
        {
            if (Count <= 0) return new();
            if (Count == 1) return new(this[0], p);
            if (Count == 2) return new(Segment.GetClosestPoint(this[0], this[1], p), p);
            if (Count == 3) return new(Triangle.GetClosestPoint(this[0], this[1], this[2], p), p);
            if (Count == 4) return new(Quad.GetClosestPoint(this[0], this[1], this[2], this[3], p), p);

            var cp = new Vector2();
            var minDisSq = float.PositiveInfinity;
            for (var i = 0; i < Count - 1; i++)
            {
                var start = this[i];
                var end = this[(i + 1) % Count];
                var next = Segment.GetClosestPoint(start, end, p);
                var disSq = (next - p).LengthSquared();
                if (disSq < minDisSq)
                {
                    minDisSq = disSq;
                    cp = next;
                }

            }

            return new(cp, p);
        }
        
        public int GetClosestIndexOnEdge(Vector2 p)
        {
            if (Count <= 0) return -1;
            if (Count == 1) return 0;

            float minD = float.PositiveInfinity;
            int closestIndex = -1;

            for (var i = 0; i < Count; i++)
            {
                var start = this[i];
                var end = this[(i + 1) % Count];
                var edge = new Segment(start, end);

                Vector2 closest = edge.GetClosestCollisionPoint(p).Point;
                float d = (closest - p).LengthSquared();
                if (d < minD)
                {
                    closestIndex = i;
                    minD = d;
                }
            }
            return closestIndex;
        }
        public ClosestPoint GetClosestPoint(Vector2 p)
        {
            var cp = GetEdges().GetClosestCollisionPoint(p);
            return new(cp, (cp.Point - p).Length());
        }
        public CollisionPoint GetClosestCollisionPoint(Vector2 p) => GetEdges().GetClosestCollisionPoint(p);
        public ClosestSegment GetClosestSegment(Vector2 p) => GetEdges().GetClosest(p);
        #endregion

        #region Contains
        public bool ContainsPoint(Vector2 p)
        {
            var segments = GetEdges();
            foreach (var segment in segments)
            {
                if (segment.ContainsPoint(p)) return true;
            }
            return false;
        }

        

        #endregion
        
        #region Overlap
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
                var end = this[(i + 1) % Count];

                for (var j = 0; j < Count - 1; j++)
                {
                    var bStart = b[j];
                    var bEnd = b[(j + 1) % b.Count];

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

        //other shape center is used for checking segment normal and if necessary normal is flipped
        public CollisionPoints? IntersectShape(Segment s)
        {
            if (Count < 2) return null;

            CollisionPoints? points = null;
            CollisionPoint? colPoint = null;
            for (var i = 0; i < Count - 1; i++)
            {
                colPoint = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], s.Start, s.End);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
            }
            return points;
        }
        public CollisionPoints? IntersectShape(Circle c)
        {
            if (Count < 2) return null;
            
            CollisionPoints? points = null;
            (CollisionPoint? a, CollisionPoint? b) result;

            for (var i = 0; i < Count - 1; i++)
            {
                result = Segment.IntersectSegmentCircle(this[i], this[(i + 1) % Count], c.Center, c.Radius);
                if (result.a != null || result.b != null)
                {
                    points ??= new();
                    if(result.a != null) points.Add((CollisionPoint)result.a);
                    if(result.b != null) points.Add((CollisionPoint)result.b);
                }
                
            }
            return points;
        }
        public CollisionPoints? IntersectShape(Triangle t)
        {
            if (Count < 2) return null;

            CollisionPoints? points = null;
            CollisionPoint? colPoint = null;
            for (var i = 0; i < Count - 1; i++)
            {
                colPoint = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], t.A, t.B);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
                colPoint = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], t.B, t.C);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
                colPoint = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], t.C, t.A);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
            }
            return points;
        }
        public CollisionPoints? IntersectShape(Rect r)
        {
            if (Count < 2) return null;

            CollisionPoints? points = null;
            CollisionPoint? colPoint = null;
            var a = r.TopLeft;
            var b = r.BottomLeft;
            var c = r.BottomRight;
            var d = r.TopRight;
            for (var i = 0; i < Count - 1; i++)
            {
                colPoint = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], a, b);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
                colPoint = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], b, c);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
                colPoint = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], c, d);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
                colPoint = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], d, a);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
            }
            return points;
        }
        public CollisionPoints? IntersectShape(Quad q)
        {
            if (Count < 2) return null;

            CollisionPoints? points = null;
            CollisionPoint? colPoint = null;
            for (var i = 0; i < Count - 1; i++)
            {
                colPoint = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], q.A, q.B);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
                colPoint = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], q.B, q.C);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
                colPoint = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], q.C, q.D);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
                colPoint = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], q.D, q.A);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
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
                    var colPoint = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count],p[j], p[(j + 1) % p.Count]);
                    if (colPoint != null)
                    {
                        points ??= new();
                        points.Add((CollisionPoint)colPoint);
                    }
                }
                
            }
            
            return points;
        }
        public CollisionPoints? IntersectShape(Polyline b)
        {
            if (b.Count < 2 || Count < 2) return null;
            CollisionPoints? points = null;
            for (var i = 0; i < Count - 1; i++)
            {
                for (var j = 0; j < b.Count - 1; j++)
                {
                    var colPoint = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count],b[j], b[(j + 1) % b.Count]);
                    if (colPoint != null)
                    {
                        points ??= new();
                        points.Add((CollisionPoint)colPoint);
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
                    var colPoint = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count],seg.Start, seg.End);
                    if (colPoint != null)
                    {
                        points ??= new();
                        points.Add((CollisionPoint)colPoint);
                    }
                }
            }
            return points;
        }
        #endregion

        #region Static
        public static Polyline GetShape(Points relative, Transform2D transform)
        {
            if (relative.Count < 3) return new();
            Polyline shape = new();
            for (int i = 0; i < relative.Count; i++)
            {
                shape.Add(transform.Apply(relative[i]));
                // shape.Add(transform.Position + relative[i].Rotate(transform.RotationRad) * transform.Scale);
            }
            return shape;
        }

        public static Polyline Center(Polyline p, Vector2 newCenter)
        {
            var centroid = p.GetCentroidMean();
            var delta = newCenter - centroid;
            return Move(p, delta);
        }
        public static Polyline Move(Polyline p, Vector2 translation)
        {
            var result = new Polyline();
            for (int i = 0; i < p.Count; i++)
            {
                result.Add(p[i] + translation);
            }
            return result;
        }
        #endregion
    }

}

//public bool IsConvex()
//{
//    int num = this.Count;
//    bool isPositive = false;
//    for (int i = 0; i < num; i++)
//    {
//        int prevIndex = (i == 0) ? num - 1 : i - 1;
//        int nextIndex = (i == num - 1) ? 0 : i + 1;
//        var d0 = this[i] - this[prevIndex];
//        var d1 = this[nextIndex] - this[i];
//        var newIsP = d0.Cross(d1) > 0f;
//        if (i == 0) isPositive = true;
//        else if (isPositive != newIsP) return false;
//    }
//    return true;
//}

/*
        //old
        public int GetClosestIndex(Vector2 p)
        {
            if (Count <= 0) return -1;
            if (Count == 1) return 0;

            float minD = float.PositiveInfinity;
            var edges = GetEdges();
            int closestIndex = -1;
            for (int i = 0; i < edges.Count; i++)
            {
                Vector2 c = edges[i].GetClosestPoint(p).Point;
                float d = (c - p).LengthSquared();
                if (d < minD)
                {
                    closestIndex = i;
                    minD = d;
                }
            }
            return closestIndex;
        }
        public Vector2 GetClosestVertex(Vector2 p)
        {
            float minD = float.PositiveInfinity;
            Vector2 closest = new();
            for (int i = 0; i < Count; i++)
            {
                float d = (this[i] - p).LengthSquared();
                if (d < minD)
                {
                    closest = this[i];
                    minD = d;
                }
            }
            return closest;
        }
        */
