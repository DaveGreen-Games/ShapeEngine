
using System.Numerics;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Shapes
{
    public class Polyline : Points, IShape, IEquatable<Polyline>
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
        public override int GetHashCode()
        {
            return ShapeUtils.GetHashCode(this);
        }
        #endregion

        #region Getter Setter
        /// <summary>
        /// Flips the calculated normals for each segment. 
        /// false means default is used. (facing right)
        /// </summary>
        public bool FlippedNormals { get; set; }

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
            else if (Count == 2)
            {
                Vector2 A = this[0];
                Vector2 B = this[1];
                return new() { new(A, B, FlippedNormals) };
                //if (AutomaticNormals)
                //{
                //    return new() { new(A, B) };
                //}
                //else
                //{
                //    return new() { new(A, B, FlippedNormals) };
                //}
            }

            Segments segments = new();
            for (int i = 0; i < Count - 1; i++)
            {
                Vector2 start = this[i];
                Vector2 end = this[(i + 1) % Count];
                segments.Add(new(start, end, FlippedNormals));

                //if (AutomaticNormals)
                //{
                //    segments.Add(new(start, end));
                //}
                //else
                //{
                //    segments.Add(new(start, end, FlippedNormals));
                //}

            }
            return segments;
        }
        
        public Points ToPoints() { return new(this); }

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

        public Vector2 GetRandomVertex() { return ShapeRandom.RandCollection(this); }
        public Segment GetRandomEdge() => GetEdges().GetRandomSegment();
        //public Vector2 GetRandomPoint() => GetRandomEdge().GetRandomPoint();
        //public Points GetRandomPoints(int amount) => GetEdges().GetRandomPoints(amount);

        #endregion

        #region IShape
        
        public Vector2 GetCentroid()
        {
            //if(Count < 2) return new Vector2();
            //
            //Vector2 c = new();
            //foreach (var p in this)
            //{
            //    c += p;
            //}
            //return c / Count;
            return GetCentroidMean();
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
                r = ShapeRect.Enlarge(r, p);
            }
            return r;
        }
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
        public bool OverlapShape(Segments segments) { return GetEdges().OverlapShape(segments); }
        public bool OverlapShape(Segment s) { return GetEdges().OverlapShape(s); }
        public bool OverlapShape(Circle c) { return GetEdges().OverlapShape(c); }
        public bool OverlapShape(Triangle t) { return GetEdges().OverlapShape(t); }
        public bool OverlapShape(Rect r) { return GetEdges().OverlapShape(r); }
        public bool OverlapShape(Polygon p)
        {
            for (int i = 0; i < Count - 1; i++)
            {
                Vector2 startPolyline = GetPoint(i); // pl[i];
                if (p.ContainsPoint(startPolyline)) return true;
                Vector2 endPolyline = GetPoint(i + 1); // pl[(i + 1)];
                Segment segPolyline = new(startPolyline, endPolyline);
                for (int j = 0; j < p.Count; j++)
                {
                    Vector2 startPoly = p.GetPoint(j); // p[j];
                    Vector2 endPoly = p.GetPoint(j); // p[(j + 1) % p.Count];
                    Segment segPoly = new(startPoly, endPoly);
                    if (segPolyline.OverlapShape(segPoly)) return true;
                }
            }
            return false;
        }
        public bool OverlapShape(Polyline b) { return GetEdges().OverlapShape(b.GetEdges()); }
        #endregion

        #region Intersection
        //other shape center is used for checking segment normal and if necessary normal is flipped
        public CollisionPoints IntersectShape(Segment s) { return GetEdges().IntersectShape(s); }
        public CollisionPoints IntersectShape(Circle c) { return GetEdges().IntersectShape(c); }
        public CollisionPoints IntersectShape(Triangle t) { return GetEdges().IntersectShape(t.GetEdges()); }
        public CollisionPoints IntersectShape(Rect r) { return GetEdges().IntersectShape(r.GetEdges()); }
        public CollisionPoints IntersectShape(Polygon p) { return GetEdges().IntersectShape(p.GetEdges()); }
        public CollisionPoints IntersectShape(Polyline b) { return GetEdges().IntersectShape(b.GetEdges()); }
        #endregion

        public static Polyline Center(Polyline p, Vector2 newCenter)
        {
            var centroid = p.GetCentroid();
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
