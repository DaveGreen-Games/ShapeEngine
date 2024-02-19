
using System.Numerics;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;
using ShapeEngine.Random;

namespace ShapeEngine.Core.Shapes
{
    /// <summary>
    /// Class that represents a triangle by holding three points. Points a, b, c should be in ccw order!
    /// </summary>
    public readonly struct Triangle : IEquatable<Triangle>
    {
        #region Members
        public readonly Vector2 A;
        public readonly Vector2 B;
        public readonly Vector2 C;
        #endregion

        #region Getter Setter
        public readonly Vector2 SideA => B - A;
        public readonly Vector2 SideB => C - B;
        public readonly Vector2 SideC => A - C;
        public readonly Segment SegmentAToB => new(A, B, FlippedNormals);
        public readonly Segment SegmentBToC => new(B, C, FlippedNormals);
        public readonly Segment SegmentCToA => new(C, A, FlippedNormals);

        public bool FlippedNormals { get; init; } = false;
        #endregion

        #region Constructors
        /// <summary>
        /// Points should be in ccw order!
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="flippedNormals"></param>
        public Triangle(Vector2 a, Vector2 b, Vector2 c, bool flippedNormals = false) 
        { 
            this.A = a; 
            this.B = b; 
            this.C = c;
            this.FlippedNormals = flippedNormals;
        }
        public Triangle(Vector2 p, Segment s, bool flippedNormals = false)
        {
            Vector2 w = s.Displacement;
            Vector2 v = p - s.Start;
            float cross = w.Cross(v);
            if(cross <= 0f)
            {
                A = s.Start;
                B = s.End;
                C = p;
            }
            else
            {
                A = s.End;
                B = s.Start;
                C = p;
            }
            this.FlippedNormals = flippedNormals;
        }
        #endregion

        #region Public
        public readonly Polygon Project(Vector2 v)
        {
            if (v.LengthSquared() <= 0f) return ToPolygon();
            var points = new Points
            {
                A,
                B,
                C,
                A + v,
                B + v,
                C + v
            };
            return Polygon.FindConvexHull(points);
        }
        public readonly Vector2 GetCentroid() { return (A + B + C) / 3; }
        public readonly Rect GetBoundingBox() { return new Rect(A.X, A.Y, 0, 0).Enlarge(B).Enlarge(C); }
        public readonly bool ContainsPoint(Vector2 p)
        {
            var ab = B - A;
            var bc = C - B;
            var ca = A - C;

            var ap = p - A;
            var bp = p - B;
            var cp = p - C;

            float c1 = ab.Cross(ap);
            float c2 = bc.Cross(bp);
            float c3 = ca.Cross(cp);

            return c1 < 0f && c2 < 0f && c3 < 0f;
        }
        // public static bool IsPointInside(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
        // {
        //     var ab = b - a;
        //     var bc = c - b;
        //     var ca = a - c;
        //
        //     var ap = p - a;
        //     var bp = p - b;
        //     var cp = p - c;
        //
        //     float c1 = ShapeVec.Cross(ab, ap);
        //     float c2 = ShapeVec.Cross(bc, bp);
        //     float c3 = ShapeVec.Cross(ca, cp);
        //
        //     if (c1 < 0f && c2 < 0f && c3 < 0f)
        //     {
        //         return true;
        //     }
        //
        //     return false;
        // }

        
        public readonly bool ContainsShape(Segment other)
        {
            return ContainsPoint(other.Start) && ContainsPoint(other.End);
        }
        public readonly bool ContainsShape(Circle other)
        {
            var points = other.GetVertices(8);
            return ContainsShape(points);
        }
        public readonly bool ContainsShape(Rect other)
        {
            return ContainsPoint(other.TopLeft) &&
                ContainsPoint(other.BottomLeft) &&
                ContainsPoint(other.BottomRight) &&
                ContainsPoint(other.TopRight);
        }
        public readonly bool ContainsShape(Triangle other)
        {
            return ContainsPoint(other.A) &&
                ContainsPoint(other.B) &&
                ContainsPoint(other.C);
        }
        public readonly bool ContainsShape(Points points)
        {
            if (points.Count <= 0) return false;
            foreach (var p in points)
            {
                if (!ContainsPoint(p)) return false;
            }
            return true;
        }

        /// <summary>
        /// Construct an adjacent triangle on the closest side to the point p. If p is inside the triangle, the triangle is returned.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public readonly Triangle ConstructAdjacentTriangle(Vector2 p)
        {
            if(ContainsPoint(p)) return this;

            var closest = GetClosestSegment(p);
            return new Triangle(p, closest.Segment);
        }

        public readonly Triangle Floor() { return new(A.Floor(), B.Floor(), C.Floor(), FlippedNormals); }
        public readonly Triangle Ceiling() { return new(A.Ceiling(), B.Ceiling(), C.Ceiling(), FlippedNormals); }
        public readonly Triangle Round() { return new(A.Round(), B.Round(), C.Round(), FlippedNormals); }
        public readonly Triangle Truncate() { return new(A.Truncate(), B.Truncate(), C.Truncate(), FlippedNormals); }
        
        public readonly bool SharesVertex(Vector2 p) { return A == p || B == p || C == p; }
        public readonly bool SharesVertex(IEnumerable<Vector2> points)
        {
            foreach (var p in points)
            {
                if (SharesVertex(p)) return true;
            }
            return false;
        }
        public readonly bool SharesVertex(Triangle t) { return SharesVertex(t.A) || SharesVertex(t.B) || SharesVertex(t.C); }
        
        public readonly bool IsValid() { return GetArea() > 0f; }
        public readonly bool IsNarrow(float narrowValue = 0.2f)
        {
            Points points = new() { A, B, C };
            for (int i = 0; i < 3; i++)
            {
                Vector2 a = points[i];
                Vector2 b = ShapeUtils.GetItem(points, i + 1);
                Vector2 c = ShapeUtils.GetItem(points, i - 1);

                Vector2 ba = (b - a).Normalize();
                Vector2 ca = (c - a).Normalize();
                float cross = ba.Cross(ca);
                if (MathF.Abs(cross) < narrowValue) return true;
            }
            return false;
        }
        /// <summary>
        /// Returns a point inside the triangle.
        /// </summary>
        /// <param name="f1">First value in the range 0 - 1.</param>
        /// <param name="f2">Second value in the range 0 - 1.</param>
        /// <returns></returns>
        public readonly Vector2 GetPoint(float f1, float f2)
        {
            if ((f1 + f2) > 1)
            {
                f1 = 1f - f1;
                f2 = 1f - f2;
            }
            Vector2 ac = (C - A) * f1;
            Vector2 ab = (B - A) * f2;
            return A + ac + ab;
            //float f1Sq = MathF.Sqrt(f1);
            //float x = (1f - f1Sq) * t.a.X + (f1Sq * (1f - f2)) * t.b.X + (f1Sq * f2) * t.c.X;
            //float y = (1f - f1Sq) * t.a.Y + (f1Sq * (1f - f2)) * t.b.Y + (f1Sq * f2) * t.c.Y;
            //return new(x, y);
        }
        public readonly Circle GetCircumCircle()
        {
            Vector2 SqrA = new Vector2(A.X * A.X, A.Y * A.Y);
            Vector2 SqrB = new Vector2(B.X * B.X, B.Y * B.Y); 
            Vector2 SqrC = new Vector2(C.X * C.X, C.Y * C.Y);

            float D = (A.X * (B.Y - C.Y) + B.X * (C.Y - A.Y) + C.X * (A.Y - B.Y)) * 2f;
            float x = ((SqrA.X + SqrA.Y) * (B.Y - C.Y) + (SqrB.X + SqrB.Y) * (C.Y - A.Y) + (SqrC.X + SqrC.Y) * (A.Y - B.Y)) / D;
            float y = ((SqrA.X + SqrA.Y) * (C.X - B.X) + (SqrB.X + SqrB.Y) * (A.X - C.X) + (SqrC.X + SqrC.Y) * (B.X - A.X)) / D;

            Vector2 center = new Vector2(x, y);
            float r = (A - center).Length();
            return new(center, r);
        }

        public readonly Triangulation Triangulate(int pointCount)
        {
            if (pointCount < 0) return new() { new(A, B, C, FlippedNormals) };

            Points points = new() { A, B, C };

            for (int i = 0; i < pointCount; i++)
            {
                float f1 = ShapeRandom.RandF();
                float f2 = ShapeRandom.RandF();
                Vector2 randPoint = GetPoint(f1, f2);
                points.Add(randPoint);
            }

            return Polygon.TriangulateDelaunay(points);
        }
        public readonly Triangulation Triangulate(float minArea)
        {
            if (minArea <= 0) return new() { new(A,B,C,FlippedNormals) };

            float triArea = GetArea();
            float pieceCount = triArea / minArea;
            int points = (int)MathF.Floor((pieceCount - 1f) * 0.5f);
            return Triangulate(points);
        }
        public readonly Triangulation Triangulate(Vector2 p)
        {
            return new()
            {
                new(A, B, p),
                new(B, C, p),
                new(C, A, p)
            };
        }
        
        public readonly Triangle GetInsideTriangle(float abF, float bcF, float caF)
        {
            Vector2 newA = ShapeVec.Lerp(A, B, abF);
            Vector2 newB = ShapeVec.Lerp(B, C, bcF);
            Vector2 newC = ShapeVec.Lerp(C, A, caF);
            return new(newA, newB, newC);
        }

        
        public readonly Triangle Rotate(float rad) { return Rotate(GetCentroid(), rad); }
        public readonly Triangle Rotate(Vector2 pivot, float rad)
        {
            Vector2 newA = pivot + (A - pivot).Rotate(rad);
            Vector2 newB = pivot + (B - pivot).Rotate(rad);
            Vector2 newC = pivot + (C - pivot).Rotate(rad);
            return new(newA, newB, newC, FlippedNormals);
        }
        public readonly Triangle Scale(float scale) { return new(A * scale, B * scale, C * scale, FlippedNormals); }
        public readonly Triangle Scale(Vector2 scale) { return new(A * scale, B * scale, C * scale, FlippedNormals); }
        public readonly Triangle Scale(Vector2 pivot, float scale)
        {
            Vector2 newA = pivot + (A - pivot) * scale;
            Vector2 newB = pivot + (B - pivot) * scale;
            Vector2 newC = pivot + (C - pivot) * scale;
            return new(newA, newB, newC, FlippedNormals);
        }
        public readonly Triangle Scale(Vector2 pivot, Vector2 scale)
        {
            Vector2 newA = pivot + (A - pivot) * scale;
            Vector2 newB = pivot + (B - pivot) * scale;
            Vector2 newC = pivot + (C - pivot) * scale;
            return new(newA, newB, newC, FlippedNormals);
        }
        public readonly Triangle Move(Vector2 offset) { return new(A + offset, B + offset, C + offset, FlippedNormals); }
        
        public readonly Points ToPoints() => new() {A, B, C};
        public readonly Polygon ToPolygon() => new() {A, B, C};

        public readonly Polyline ToPolyline() => new() { A, B, C };
        public readonly Segments GetEdges() => new() { SegmentAToB, SegmentBToC, SegmentCToA };
        public readonly Triangulation Triangulate() => this.Triangulate(GetCentroid());
        public readonly float GetCircumference() => MathF.Sqrt(GetCircumferenceSquared());
        public readonly float GetCircumferenceSquared() => SideA.LengthSquared() + SideB.LengthSquared() + SideC.LengthSquared();

        public readonly float GetArea() 
        {
            //float al = A.Length();
            //float bl = B.Length();
            //float cl = C.Length();
            //
            //
            //float i = (al + bl + cl) / 2f;
            //float area1 = MathF.Sqrt(i * (i - al) * (i - bl) * (i - cl));
            //float area2 = MathF.Abs((a.X - c.X) * (b.Y - c.Y) - (a.Y - c.Y) * (b.X - c.X)) / 2f;
            //if(MathF.Abs(area1 - area2) > 1)
            //{
            //    int breakpoint = 0;
            //}

            return MathF.Abs((A.X - C.X) * (B.Y - C.Y) - (A.Y - C.Y) * (B.X - C.X)) / 2f;
        }

        public readonly Vector2 GetClosestVertex(Vector2 p)
        {
            var closest = A;
            float minDisSquared = (A - p).LengthSquared();

            float l = (B - p).LengthSquared();
            if (l < minDisSquared)
            {
                closest = B;
                minDisSquared = l;
            }

            l = (C - p).LengthSquared();
            if (l < minDisSquared) closest = C;

            return closest;
        }

        public readonly ClosestPoint GetClosestPoint(Vector2 p)
        {
            var cp = GetClosestCollisionPoint(p);
            return new(cp, (cp.Point - p).Length());
        }
        public readonly ClosestSegment GetClosestSegment(Vector2 p)
        {
            var closestSegment = SegmentAToB;
            var cp = SegmentAToB.GetClosestCollisionPoint(p);
            float minDisSquared = (cp.Point - p).LengthSquared();

            var curCP = SegmentBToC.GetClosestCollisionPoint(p);
            float l = (curCP.Point - p).LengthSquared();
            if (l < minDisSquared)
            {
                minDisSquared = l;
                closestSegment = SegmentBToC;
                cp = curCP;
            }
            curCP = SegmentCToA.GetClosestCollisionPoint(p);
            l = (curCP.Point - p).LengthSquared();
            if (l < minDisSquared)
            {
                minDisSquared = l;
                closestSegment = SegmentCToA;
                cp = curCP;
            }
            return new(closestSegment, cp, MathF.Sqrt(minDisSquared));

        }
        public readonly CollisionPoint GetClosestCollisionPoint(Vector2 p)
        {
            var cp = SegmentAToB.GetClosestCollisionPoint(p);
            float minDisSquared = (cp.Point - p).LengthSquared();

            var curCP = SegmentBToC.GetClosestCollisionPoint(p);
            float l = (curCP.Point - p).LengthSquared();
            if (l < minDisSquared)
            {
                minDisSquared = l;
                cp = curCP;
            }
            curCP = SegmentCToA.GetClosestCollisionPoint(p);
            l = (curCP.Point - p).LengthSquared();
            if (l < minDisSquared)
            {
                minDisSquared = l;
                cp = curCP;
            }

            return cp;
        }

        
        public readonly Vector2 GetRandomPointInside() => this.GetPoint(ShapeRandom.RandF(), ShapeRandom.RandF());

        public readonly Points GetRandomPointsInside(int amount)
        {
            var points = new Points();
            for (int i = 0; i < amount; i++)
            {
                points.Add(GetRandomPointInside());
            }
            return points;
        }

        public readonly Vector2 GetRandomVertex()
        {
            var randIndex = ShapeRandom.RandI(0, 2);
            if (randIndex == 0) return A;
            else if (randIndex == 1) return B;
            else return C;
        }
        public Segment GetRandomEdge() => GetEdges().GetRandomSegment();
        public Vector2 GetRandomPointOnEdge() => GetRandomEdge().GetRandomPoint();
        public Points GetRandomPointsOnEdge(int amount) => GetEdges().GetRandomPoints(amount);

        #endregion


        #region Equality & HashCode
        public bool IsSimilar(Triangle other)
        {
            return 
                (A.IsSimilar(A) && B.IsSimilar(B) && C.IsSimilar(C) ) || 
                (C.IsSimilar(A) && A.IsSimilar(B) && B.IsSimilar(C) ) || 
                (B.IsSimilar(A) && C.IsSimilar(B) && A.IsSimilar(C) ) ||
                (B.IsSimilar(A) && A.IsSimilar(B) && C.IsSimilar(C) ) ||
                (C.IsSimilar(A) && B.IsSimilar(B) && A.IsSimilar(C) ) ||
                (A.IsSimilar(A) && C.IsSimilar(B) && B.IsSimilar(C) );
            
            //return 
            //    (A == other.A && B == other.B && C == other.C) || 
            //    (C == other.A && A == other.B && B == other.C) || 
            //    (B == other.A && C == other.B && A == other.C) ||
            //    (B == other.A && A == other.B && C == other.C) ||
            //    (C == other.A && B == other.B && A == other.C) ||
            //    (A == other.A && C == other.B && B == other.C);
        }
        public bool Equals(Triangle other)
        {
            return A.IsSimilar(other.A) && B.IsSimilar(other.B) && C.IsSimilar(other.C);
        }
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(A, B, C);
        }
        public static bool operator ==(Triangle left, Triangle right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(Triangle left, Triangle right)
        {
            return !(left == right);
        }
        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is Triangle t) return Equals(t);
            return false;
        }
        #endregion

        #region Static

        public static float AreaSigned(Vector2 a, Vector2 b, Vector2 c) { return (a.X - c.X) * (b.Y - c.Y) - (a.Y - c.Y) * (b.X - c.X); }


        #endregion

        #region Overlap
        public readonly bool Overlap(Collider collider)
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
        public readonly bool OverlapShape(Segments segments)
        {
            foreach (var seg in segments)
            {
                if (OverlapShape(seg)) return true;
            }
            return false;
        }

        public readonly bool OverlapShape(Segment s) => s.OverlapShape(this);
        public readonly bool OverlapShape(Circle c) => c.OverlapShape(this);
        public readonly bool OverlapShape(Triangle b)
        {
            if (ContainsPoint(b.A)) return true;
            // if (ContainsPoint(b.B)) return true;
            // if (ContainsPoint(b.C)) return true;
            
            if (b.ContainsPoint(A)) return true;
            // if (b.ContainsPoint(B)) return true;
            // if (b.ContainsPoint(C)) return true;
            
            if (Segment.OverlapSegmentSegment(A, B, b.A, b.B)) return true;
            if (Segment.OverlapSegmentSegment(A, B, b.B, b.C)) return true;
            if (Segment.OverlapSegmentSegment(A, B, b.C, b.A)) return true;
            
            if (Segment.OverlapSegmentSegment(B, C, b.A, b.B)) return true;
            if (Segment.OverlapSegmentSegment(B, C, b.B, b.C)) return true;
            if (Segment.OverlapSegmentSegment(B, C, b.C, b.A)) return true;
            
            if (Segment.OverlapSegmentSegment(C, A, b.A, b.B)) return true;
            if (Segment.OverlapSegmentSegment(C, A, b.B, b.C)) return true;
            return Segment.OverlapSegmentSegment(C, A, b.C, b.A);
        }
        public readonly bool OverlapShape(Rect r)
        {
            var a = r.TopLeft;
            if (ContainsPoint(a)) return true;
            
            // if (ContainsPoint(b)) return true;
            // if (ContainsPoint(c)) return true;
            // if (ContainsPoint(d)) return true;
            
            if (r.ContainsPoint(A)) return true;
            // if (r.ContainsPoint(B)) return true;
            // if (r.ContainsPoint(C)) return true;
            
            var b = r.BottomLeft;
            if (Segment.OverlapSegmentSegment(A, B, a, b)) return true;
            
            var c = r.BottomRight;
            if (Segment.OverlapSegmentSegment(A, B, b, c)) return true;
            
            var d = r.TopRight;
            if (Segment.OverlapSegmentSegment(A, B, c, d)) return true;
            if (Segment.OverlapSegmentSegment(A, B, d, a)) return true;
            
            if (Segment.OverlapSegmentSegment(B, C, a, b)) return true;
            if (Segment.OverlapSegmentSegment(B, C, b, c)) return true;
            if (Segment.OverlapSegmentSegment(B, C, c, d)) return true;
            if (Segment.OverlapSegmentSegment(B, C, d, a)) return true;
            
            if (Segment.OverlapSegmentSegment(C, A, a, b)) return true;
            if (Segment.OverlapSegmentSegment(C, A, b, c)) return true;
            if (Segment.OverlapSegmentSegment(C, A, c, d)) return true;
            return Segment.OverlapSegmentSegment(C, A, d, a);
        }
        public readonly bool OverlapShape(Quad q)
        {
            if (ContainsPoint(q.A)) return true;
            // if (ContainsPoint(q.B)) return true;
            // if (ContainsPoint(q.C)) return true;
            // if (ContainsPoint(q.D)) return true;
            
            if (q.ContainsPoint(A)) return true;
            // if (q.ContainsPoint(B)) return true;
            // if (q.ContainsPoint(C)) return true;
            
            if (Segment.OverlapSegmentSegment(A, B, q.A, q.B)) return true;
            if (Segment.OverlapSegmentSegment(A, B, q.B, q.C)) return true;
            if (Segment.OverlapSegmentSegment(A, B, q.C, q.D)) return true;
            if (Segment.OverlapSegmentSegment(A, B, q.D, q.A)) return true;
            
            if (Segment.OverlapSegmentSegment(B, C, q.A, q.B)) return true;
            if (Segment.OverlapSegmentSegment(B, C, q.B, q.C)) return true;
            if (Segment.OverlapSegmentSegment(B, C, q.C, q.D)) return true;
            if (Segment.OverlapSegmentSegment(B, C, q.D, q.A)) return true;
            
            if (Segment.OverlapSegmentSegment(C, A, q.A, q.B)) return true;
            if (Segment.OverlapSegmentSegment(C, A, q.B, q.C)) return true;
            if (Segment.OverlapSegmentSegment(C, A, q.C, q.D)) return true;
            return Segment.OverlapSegmentSegment(C, A, q.D, q.A);
        }
        public readonly bool OverlapShape(Polygon poly)
        {
            if (poly.Count <= 0) return false;
            if (poly.Count == 1) return ContainsPoint(poly[0]);
            
            if (ContainsPoint(poly[0])) return true;
            
            var oddNodes = false;
            
            for (var i = 0; i < poly.Count; i++)
            {
                var start = poly[i];
                var end = poly[(i + 1) % poly.Count];
                if (Segment.OverlapSegmentSegment(A, B, start, end)) return true;
                if (Segment.OverlapSegmentSegment(B, C, start, end)) return true;
                if (Segment.OverlapSegmentSegment(C, A, start, end)) return true;
                
                if(Polygon.ContainsPointCheck(start, end, A)) oddNodes = !oddNodes;
            }

            return oddNodes;
        }
        public readonly bool OverlapShape(Polyline pl)
        {
            if (pl.Count <= 0) return false;
            if (pl.Count == 1) return ContainsPoint(pl[0]);
            
            if (ContainsPoint(pl[0])) return true;
            
            for (var i = 0; i < pl.Count - 1; i++)
            {
                var start = pl[i];
                var end = pl[(i + 1) % pl.Count];
                if (Segment.OverlapSegmentSegment(A, B, start, end)) return true;
                if (Segment.OverlapSegmentSegment(B, C, start, end)) return true;
                if (Segment.OverlapSegmentSegment(C, A, start, end)) return true;
                
            }

            return false;
        }


        #endregion

        #region Intersect
        
        public readonly CollisionPoints? Intersect(Collider collider)
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


        public readonly CollisionPoints? IntersectShape(Segment s) { return GetEdges().IntersectShape(s); }
        public readonly CollisionPoints? IntersectShape(Circle c) { return ToPolygon().IntersectShape(c); }
        public readonly CollisionPoints? IntersectShape(Triangle b) { return ToPolygon().IntersectShape(b.ToPolygon()); }
        public readonly CollisionPoints? IntersectShape(Rect r) { return ToPolygon().IntersectShape(r.ToPolygon()); }
        public readonly CollisionPoints? IntersectShape(Quad q) { return ToPolygon().IntersectShape(q.ToPolygon()); }
        public readonly CollisionPoints? IntersectShape(Polygon p) { return ToPolygon().IntersectShape(p); }
        public readonly CollisionPoints? IntersectShape(Polyline pl) { return GetEdges().IntersectShape(pl.GetEdges()); }
        
        #endregion
    }
}

