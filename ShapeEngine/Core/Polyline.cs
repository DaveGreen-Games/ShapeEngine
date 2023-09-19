
using System.Numerics;
using ShapeEngine.Lib;
using ShapeEngine.Random;

namespace ShapeEngine.Core
{
    public class Polyline : Points, IShape, IEquatable<Polyline>
    {
        #region Constructors
        public Polyline() { }
        /// <summary>
        /// Points should be in CCW order. Use Reverse if they are in CW order.
        /// </summary>
        /// <param name="points"></param>
        public Polyline(params Vector2[] points) { AddRange(points); }
        /// <summary>
        /// Points should be in CCW order. Use Reverse if they are in CW order.
        /// </summary>
        /// <param name="edges"></param>
        public Polyline(IEnumerable<Vector2> edges) { AddRange(edges); }
        /// <summary>
        /// Points should be in CCW order. Use Reverse if they are in CW order.
        /// </summary>
        /// <param name="shape"></param>
        public Polyline(IShape shape) { AddRange(shape.ToPolyline()); }
        public Polyline(Polyline polyLine) { AddRange(polyLine); }
        public Polyline(Polygon poly) { AddRange(poly); }
        #endregion

        #region Equals & HashCode
        public bool Equals(Polyline? other)
        {
            if (other == null) return false;
            if (Count != other.Count) return false;
            for (int i = 0; i < Count; i++)
            {
                if (this[i] != other[i]) return false;
            }
            return true;
        }
        public override int GetHashCode()
        {
            return SUtils.GetHashCode(this);
        }
        #endregion

        #region Getter Setter
        /// <summary>
        /// Flips the calculated normals for each segment. 
        /// false means default is used. (facing right)
        /// </summary>
        public bool FlippedNormals { get; set; }
        #endregion

        #region Public
        //public Polyline Copy() { return new(this); }
        //public void Floor() { Points.Floor(this); }
        //public void Ceiling() { Points.Ceiling(this); }
        //public void Truncate() { Points.Truncate(this); }
        //public void Round() { Points.Round(this); }
        #endregion

        #region Static

        #endregion

        #region IShape
        public Vector2 GetVertex(int index)
        {
            return this[SUtils.WrapIndex(Count, index)];
        }
        public Vector2 GetCentroidOnLine()
        {
            if (Count <= 0) return new(0f);
            else if (Count == 1) return this[0];
            float halfLengthSq = GetCircumferenceSquared() * 0.5f;
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
        public Vector2 GetCentroidMean()
        {
            if (Count <= 0) return new(0f);
            else if (Count == 1) return this[0];
            Vector2 total = new(0f);
            foreach (Vector2 p in this) { total += p; }
            return total / Count;
        }
        public Triangulation Triangulate() { return new(); }
       
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
                r = SRect.Enlarge(r, p);
            }
            return r;
        }
        public float GetCircumference() { return MathF.Sqrt(GetCircumferenceSquared()); }
        public float GetCircumferenceSquared()
        {
            if (this.Count < 2) return 0f;
            float lengthSq = 0f;
            for (int i = 0; i < Count - 1; i++)
            {
                Vector2 w = this[i+1] - this[i];
                lengthSq += w.LengthSquared();
            }
            return lengthSq;
        }
        public float GetArea() { return 0f; }
        public bool IsClockwise() { return false; }
        public bool IsConvex()
        {
            int num = this.Count;
            bool isPositive = false;

            for (int i = 0; i < num; i++)
            {
                int prevIndex = (i == 0) ? num - 1 : i - 1;
                int nextIndex = (i == num - 1) ? 0 : i + 1;
                var d0 = this[i] - this[prevIndex];
                var d1 = this[nextIndex] - this[i];
                var newIsP = d0.Cross(d1) > 0f;
                if (i == 0) isPositive = true;
                else if (isPositive != newIsP) return false;
            }
            return true;
        }


        public Points GetVertices() { return new(this); }
        
        
        public bool ContainsPoint(Vector2 p)
        {
            var segments = GetEdges();
            foreach (var segment in segments)
            {
                if (segment.ContainsPoint(p)) return true;
            }
            return false;
        }

        public int GetClosestIndex(Vector2 p)
        {
            if (Count <= 0) return -1;
            if (Count == 1) return 0;

            float minD = float.PositiveInfinity;
            int closestIndex = -1;

            for (int i = 0; i < Count - 1; i++)
            {
                Vector2 start = this[i];
                Vector2 end = this[i + 1];
                Segment edge = new Segment(start, end);

                Vector2 closest = edge.GetClosestPoint(p).Point;
                float d = (closest - p).LengthSquared();
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
            if (Count < 2) return new();
            float minD = float.PositiveInfinity;
            Vector2 closestPoint = new();

            for (int i = 0; i < Count - 1; i++)
            {
                Vector2 start = this[i];
                Vector2 end = this[i + 1];
                Segment edge = new Segment(start, end);

                Vector2 closest = edge.GetClosestPoint(p).Point;
                float d = (closest - p).LengthSquared();
                if (d < minD)
                {
                    closestPoint = closest;
                    minD = d;
                }
            }
            return closestPoint;
        }
        public Segment GetClosestSegment(Vector2 p)
        {
            if (Count < 2) return new();
            float minD = float.PositiveInfinity;
            Segment closestSegment = new();

            for (int i = 0; i < Count - 1; i++)
            {
                Vector2 start = this[i];
                Vector2 end = this[i + 1];
                Segment edge = new Segment(start, end);

                Vector2 closest = edge.GetClosestPoint(p).Point;
                float d = (closest - p).LengthSquared();
                if (d < minD)
                {
                    closestSegment = edge;
                    minD = d;
                }
            }
            return closestSegment;
        }
        
        public CollisionPoint GetClosestPoint(Vector2 p)
        {
            float minD = float.PositiveInfinity;
            var edges = GetEdges();
            CollisionPoint closest = new();

            for (int i = 0; i < edges.Count; i++)
            {
                CollisionPoint c = edges[i].GetClosestPoint(p);
                float d = (c.Point - p).LengthSquared();
                if (d < minD)
                {
                    closest = c;
                    minD = d;
                }
            }
            return closest;
        }
        
        public Vector2 GetRandomPoint() { return GetRandomPointOnEdge(); }
        public Points GetRandomPoints(int amount)
        {
            var points = new Points();
            for (int i = 0; i < amount; i++)
            {
                points.Add(GetRandomPoint());
            }
            return points;
        }
        public Vector2 GetRandomVertex() { return SRNG.randCollection(this); }
        public Segment GetRandomEdge()
        {
            var edges = GetEdges();
            List<WeightedItem<Segment>> items = new(edges.Count);
            foreach (var edge in edges)
            {
                items.Add(new(edge, (int)edge.LengthSquared));
            }
            return SRNG.PickRandomItem(items.ToArray());
            //return SRNG.randCollection(GetEdges(), false); 
        }
        public Vector2 GetRandomPointOnEdge() { return GetRandomEdge().GetRandomPoint(); }
        public Points GetRandomPointsOnEdge(int amount)
        {
            List<WeightedItem<Segment>> items = new(amount);
            var edges = GetEdges();
            foreach (var edge in edges)
            {
                items.Add(new(edge, (int)edge.LengthSquared));
            }
            var pickedEdges = SRNG.PickRandomItems(amount, items.ToArray());
            var randomPoints = new Points();
            foreach (var edge in pickedEdges)
            {
                randomPoints.Add(edge.GetRandomPoint());
            }
            return randomPoints;
        }

        public void DrawShape(float linethickness, Raylib_CsLo.Color color) => this.Draw(linethickness, color);
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
                Vector2 startPolyline = Get(i); // pl[i];
                if (p.ContainsPoint(startPolyline)) return true;
                Vector2 endPolyline = Get(i + 1); // pl[(i + 1)];
                Segment segPolyline = new(startPolyline, endPolyline);
                for (int j = 0; j < p.Count; j++)
                {
                    Vector2 startPoly = p.Get(j); // p[j];
                    Vector2 endPoly = p.Get(j); // p[(j + 1) % p.Count];
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
    }

}

