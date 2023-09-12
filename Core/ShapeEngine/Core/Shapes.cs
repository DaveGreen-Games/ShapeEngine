
using System.Numerics;
using Raylib_CsLo;
using ShapeEngine.Lib;
using ShapeEngine.Random;

namespace ShapeEngine.Core
{
    public readonly struct Segment : IShape, IEquatable<Segment>
    {
        #region Members
        public readonly Vector2 Start;
        public readonly Vector2 End;
        public readonly Vector2 Normal;
        private readonly bool flippedNormals = false;
        #endregion

        #region Getter Setter
        //maybe needs to be cached
        //if it is cached segment needs to be immutable... so normal is always correct
        //public Vector2 Normal 
        //{ 
        //    get 
        //    {
        //        return GetNormal();
        //    } 
        //}
        public bool FlippedNormals { get { return flippedNormals; } readonly set { } }
        public Vector2 Center { get { return (Start + End) * 0.5f; } }
        public Vector2 Dir { get { return Displacement.Normalize(); } }
        public Vector2 Displacement { get { return End - Start; } }
        public float Length { get { return Displacement.Length(); } }
        public float LengthSquared { get { return Displacement.LengthSquared(); } }
        #endregion

        #region Constructor
        public Segment(Vector2 start, Vector2 end, bool flippedNormals = false) 
        { 
            this.Start = start; 
            this.End = end;
            this.Normal = GetNormal(start, end, flippedNormals);
            this.flippedNormals = flippedNormals;
        }
        
        public Segment(float startX, float startY, float endX, float endY, bool flippedNormals = false) 
        { 
            this.Start = new(startX, startY); 
            this.End = new(endX, endY);
            this.Normal = GetNormal(Start, End, flippedNormals);
            this.flippedNormals = flippedNormals;
        }
        #endregion

        #region Public
        public readonly Segment Floor()
        {
            return new(Start.Floor(), End.Floor(), FlippedNormals);
        }
        public readonly Segment Ceiling()
        {
            return new(Start.Ceiling(), End.Ceiling(), FlippedNormals);
        }
        public readonly Segment Round()
        {
            return new(Start.Round(), End.Round(), FlippedNormals);
        }
        public readonly Segment Truncate()
        {
            return new(Start.Truncate(), End.Truncate(), FlippedNormals);
        }

        public readonly Segments Split(float f)
        {
            return Split(this.GetPoint(f));
        }
        public readonly Segments Split(Vector2 splitPoint)
        {
            Segment A = new(Start, splitPoint, FlippedNormals);
            Segment B = new(splitPoint, End, FlippedNormals);
            return new() { A, B };
        }

        public readonly Segment SetStart(Vector2 newStart) { return new(newStart, End, FlippedNormals); }
        public readonly Segment MoveStart(Vector2 translation) { return new(Start + translation, End, FlippedNormals); }
        public readonly Segment SetEnd(Vector2 newEnd) { return new(Start, newEnd, FlippedNormals); }
        public readonly Segment MoveEnd(Vector2 translation) { return new(Start, End + translation, FlippedNormals); }
        
        public readonly Vector2 GetPoint(float f) { return Start.Lerp(End, f); }
        public readonly Segment Rotate(float pivot, float rad)
        {
            Vector2 p = GetPoint(pivot);
            Vector2 s = Start - p;
            Vector2 e = End - p;
            return new Segment(p + s.Rotate(rad), p + e.Rotate(rad));
        }
        public readonly Segment Scale(float scale) { return new(Start * scale, End * scale); }
        public readonly Segment Scale(Vector2 scale) { return new(Start * scale, End * scale); }
        public readonly Segment Scale(float startScale, float endScale) { return new(Start * startScale, End * endScale); }
        public readonly Segment ScaleF(float scale, float f)
        {
            Vector2 p = GetPoint(f);
            Vector2 s = Start - p;
            Vector2 e = End - p;
            return new Segment(p + s * scale, p + e * scale);
        }
        public readonly Segment ScaleF(Vector2 scale, float f)
        {
            Vector2 p = GetPoint(f);
            Vector2 s = Start - p;
            Vector2 e = End - p;
            return new Segment(p + s * scale, p + e * scale);
        }
        public readonly Segment Move(Vector2 offset, float f) { return new(Start + (offset * (1f - f)), End + (offset * (f))); }
        public readonly Segment Move(Vector2 offset) { return new(Start + offset, End + offset); }
        public readonly Segment Move(float x, float y) { return Move(new Vector2(x, y)); }
        public readonly Points Inflate(float thickness, float alignement = 0.5f)
        {
            float w = thickness;
            Vector2 dir = Dir;
            Vector2 left = dir.GetPerpendicularLeft();
            Vector2 right = dir.GetPerpendicularRight();
            Vector2 a = Start + left * w * alignement;
            Vector2 b = Start + right * w * (1 - alignement);
            Vector2 c = End + right * w * (1 - alignement);
            Vector2 d = End + left * w * alignement;

            return new() { a, b, c, d };
        }

        #endregion

        #region Private
        private static Vector2 GetNormal(Vector2 start, Vector2 end, bool flippedNormals)
        {
            if (flippedNormals) return (end - start).GetPerpendicularLeft().Normalize();
            else return (end - start).GetPerpendicularRight().Normalize();
        }
        #endregion

        #region IShape
        public Vector2 GetCentroid() { return Center; }
        public float GetArea() { return 0f; }
        public float GetCircumference() { return Length; }
        public float GetCircumferenceSquared() { return LengthSquared; }
        public Points GetVertices() { return new(Start, End); }
        public Polygon ToPolygon() { return new(Start, End); }
        public Polyline ToPolyline() { return new(Start, End); }
        public Segments GetEdges() { return new(this); }
        
        public Triangulation Triangulate() { return new(); }
        public Circle GetBoundingCircle() { return ToPolygon().GetBoundingCircle(); }
        public Rect GetBoundingBox() { return new(Start, End); }
        public bool IsPointInside(Vector2 p) { return SGeometry.IsPointOnSegment(p, Start, End); }
        public CollisionPoint GetClosestPoint(Vector2 p)
        {
            CollisionPoint c;
            var w = Displacement;
            float t = (p - Start).Dot(w) / w.LengthSquared();
            if (t < 0f) c = new(Start, Normal); 
            else if (t > 1f) c = new(End, Normal);
            else c = new(Start + w * t, Normal);

            //if (AutomaticNormals) return c.FlipNormal(p);
            return c;
        }
        public Vector2 GetClosestVertex(Vector2 p)
        {
            float disSqA = (p - Start).LengthSquared();
            float disSqB = (p - End).LengthSquared();
            return disSqA <= disSqB ? Start : End;
        }
        public Vector2 GetRandomPoint() { return this.GetPoint(SRNG.randF()); }
        public Points GetRandomPoints(int amount)
        {
            var points = new Points();
            for (int i = 0; i < amount; i++)
            {
                points.Add(GetRandomPoint());
            }
            return points;
        }
        public Vector2 GetRandomVertex() { return SRNG.chance(0.5f) ? Start : End; }
        public Segment GetRandomEdge() { return this; }
        public Vector2 GetRandomPointOnEdge() { return GetRandomPoint(); }
        public Points GetRandomPointsOnEdge(int amount)
        {
            var points = new Points();
            for (int i = 0; i < amount; i++)
            {
                points.Add(GetRandomPointOnEdge());
            }
            return points;
        }
        public void DrawShape(float linethickness, Raylib_CsLo.Color color) => this.Draw(linethickness, color);
        #endregion

        #region Equality & HashCode
        public bool Equals(Segment other)
        {
            return Start == other.Start && End == other.End;
        }
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Start, End);
        }
        public static bool operator ==(Segment left, Segment right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(Segment left, Segment right)
        {
            return !(left == right);
        }
        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is Segment s) return Equals(s);
            return false;
        }
        #endregion
    }

    public struct Circle : IShape, IEquatable<Circle>
    {
        #region Members
        public Vector2 Center;
        public float Radius;
        #endregion

        #region Getter Setter
        public float Diameter { get { return Radius * 2f; } }
        public bool FlippedNormals { get; set; } = false;
        #endregion
        
        #region Constructors
        public Circle(Vector2 center, float radius, bool flippedNormals = false) { this.Center = center; this.Radius = radius; this.FlippedNormals = flippedNormals; }
        public Circle(float x, float y, float radius, bool flippedNormals = false) { this.Center = new(x, y); this.Radius = radius; this.FlippedNormals = flippedNormals; }
        public Circle(Circle c, float radius) { Center = c.Center; Radius = radius; FlippedNormals = c.FlippedNormals; }
        public Circle(Circle c, Vector2 center) { Center = center; Radius = c.Radius; FlippedNormals = c.FlippedNormals; }
        public Circle(Rect r) { Center = r.Center; Radius = MathF.Max(r.Width, r.Height); FlippedNormals = r.FlippedNormals; }
        #endregion

        #region Equality & Hashcode
        public bool Equals(Circle other)
        {
            return Center == other.Center && Radius == other.Radius;
        }
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Center, Radius);
        }

        public static bool operator ==(Circle left, Circle right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(Circle left, Circle right)
        {
            return !(left == right);
        }
        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is Circle c) return Equals(c);
            return false;
        }
        #endregion

        #region Public
        public readonly Circle Floor() { return new(Center.Floor(), MathF.Floor(Radius)); }
        public readonly Circle Ceiling() { return new(Center.Ceiling(), MathF.Ceiling(Radius)); }
        public readonly Circle Round() { return new(Center.Round(), MathF.Round(Radius)); }
        public readonly Circle Truncate() { return new(Center.Truncate(), MathF.Truncate(Radius)); }
                
        public readonly Vector2 GetPoint(float angleRad, float f) { return Center + new Vector2(Radius * f, 0f).Rotate(angleRad); }
        public readonly Segments GetEdges(int pointCount = 16, bool insideNormals = false)
        {
            float angleStep = (MathF.PI * 2f) / pointCount;
            Segments segments = new();
            for (int i = 0; i < pointCount; i++)
            {
                Vector2 start = Center + new Vector2(Radius, 0f).Rotate(-angleStep * i);
                Vector2 end = Center + new Vector2(Radius, 0f).Rotate(-angleStep * ((i + 1) % pointCount));

                segments.Add(new Segment(start, end, insideNormals));
            }
            return segments;
        }
        public readonly Points GetVertices(int pointCount = 16)
        {
            float angleStep = (MathF.PI * 2f) / pointCount;
            Points points = new();
            for (int i = 0; i < pointCount; i++)
            {
                Vector2 p = Center + new Vector2(Radius, 0f).Rotate(angleStep * i);
                points.Add(p);
            }
            return points;
        }
        public readonly Polygon ToPolygon(int pointCount = 16)
        {
            float angleStep = (MathF.PI * 2f) / pointCount;
            Polygon poly = new();
            for (int i = 0; i < pointCount; i++)
            {
                Vector2 p = Center + new Vector2(Radius, 0f).Rotate(angleStep * i);
                poly.Add(p);
            }
            return poly;
        }
        public readonly Polyline ToPolyline(int pointCount = 16)
        {
            float angleStep = (MathF.PI * 2f) / pointCount;
            Polyline polyLine = new();
            for (int i = 0; i < pointCount; i++)
            {
                Vector2 p = Center + new Vector2(Radius, 0f).Rotate(angleStep * i);
                polyLine.Add(p);
            }
            return polyLine;
        }

        /*
        public Circle ScaleRadius(float scale) { return new(Center, Radius * scale); }
        public Circle ChangeRadius(float amount) { return new(Center, Radius + amount); }
        public Circle SetRadius(float newRadius) { return new(Center, newRadius); }
        public Circle MoveCenter(Vector2 offset) { return new(Center + offset, Radius); }
        public Circle SetCenter(Vector2 newCenter) { return new(newCenter, Radius); }
        */
        public readonly Circle Combine(Circle other)
        {
            return new
                (
                    (Center + other.Center) / 2,
                    Radius + other.Radius
                );
        }
        public static Circle Combine(params Circle[] circles)
        {
            if (circles.Length <= 0) return new();
            Vector2 combinedCenter = new();
            float totalRadius = 0f;
            for (int i = 0; i < circles.Length; i++)
            {
                var circle = circles[i];
                combinedCenter += circle.Center;
                totalRadius += circle.Radius;
            }
            return new(combinedCenter / circles.Length, totalRadius);
        }
        #endregion

        #region IShape
        public Vector2 GetCentroid() { return Center; }
        public Segments GetEdges() { return GetEdges(16, FlippedNormals); }

        public Points GetVertices() { return GetVertices(16); }
        public Polygon ToPolygon() { return ToPolygon(16); }
        public Polyline ToPolyline() { return ToPolyline(16); }
        public Triangulation Triangulate() { return ToPolygon().Triangulate(); }
        public Circle GetBoundingCircle() { return this; }
        public float GetArea() { return MathF.PI * Radius * Radius; }
        public float GetCircumference() { return MathF.PI * Radius * 2f; }
        public float GetCircumferenceSquared() { return GetCircumference() * GetCircumference(); }
        public Rect GetBoundingBox() { return new Rect(Center, new Vector2(Radius, Radius) * 2f, new(0.5f)); }
        public bool IsPointInside(Vector2 p) { return SGeometry.IsPointInCircle(p, Center, Radius); }
        public CollisionPoint GetClosestPoint(Vector2 p) 
        {
            Vector2 normal = (p - Center).Normalize();
            Vector2 point = Center + normal * Radius;
            return new(point, normal);
        }
        public Vector2 GetClosestVertex(Vector2 p) { return Center + (p - Center).Normalize() * Radius; }
        public Vector2 GetRandomPoint()
        {
            float randAngle = SRNG.randAngleRad();
            var randDir = SVec.VecFromAngleRad(randAngle);
            return Center + randDir * SRNG.randF(0, Radius);
        }
        public Points GetRandomPoints(int amount)
        {
            var points = new Points();
            for (int i = 0; i < amount; i++)
            {
                points.Add(GetRandomPoint());
            }
            return points;
        }
        public Vector2 GetRandomVertex() { return SRNG.randCollection(GetVertices(), false); }
        public Segment GetRandomEdge() { return SRNG.randCollection(GetEdges(), false); }
        public Vector2 GetRandomPointOnEdge() { return GetRandomEdge().GetRandomPoint(); }
        public Points GetRandomPointsOnEdge(int amount)
        {
            var points = new Points();
            for (int i = 0; i < amount; i++)
            {
                points.Add(GetRandomPointOnEdge());
            }
            return points;
        }
        public void DrawShape(float linethickness, Raylib_CsLo.Color color) => this.DrawLines(linethickness, color);
        #endregion

        //public Vector2 GetReferencePoint() { return center; }
        //public SegmentShape GetSegmentShape() { return new(this.GetEdges(), center); }
    }

    /// <summary>
    /// Class that represents a triangle by holding three points. Points a, b, c should be in ccw order!
    /// </summary>
    public struct Triangle : IShape, IEquatable<Triangle>
    {
        #region Members
        public Vector2 a;
        public Vector2 b;
        public Vector2 c;
        #endregion

        #region Getter Setter
        public Vector2 A { get { return b - a; } }
        public Vector2 B { get { return c - b; } }
        public Vector2 C { get { return a - c; } }
        public bool FlippedNormals { get; set; } = false;
        #endregion

        #region Constructors
        /// <summary>
        /// Points should be in ccw order!
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        public Triangle(Vector2 a, Vector2 b, Vector2 c, bool flippedNormals = false) 
        { 
            this.a = a; 
            this.b = b; 
            this.c = c;
            this.FlippedNormals = flippedNormals;
        }
        public Triangle(Vector2 p, Segment s, bool flippedNormals = false)
        {
            Vector2 w = s.Displacement;
            Vector2 v = p - s.Start;
            float cross = w.Cross(v);
            if(cross <= 0f)
            {
                a = s.Start;
                b = s.End;
                c = p;
            }
            else
            {
                a = s.End;
                b = s.Start;
                c = p;
            }
            this.FlippedNormals = flippedNormals;
        }
        #endregion

        #region Public
        public readonly Triangle Floor() { return new(a.Floor(), b.Floor(), c.Floor(), FlippedNormals); }
        public readonly Triangle Ceiling() { return new(a.Ceiling(), b.Ceiling(), c.Ceiling(), FlippedNormals); }
        public readonly Triangle Round() { return new(a.Round(), b.Round(), c.Round(), FlippedNormals); }
        public readonly Triangle Truncate() { return new(a.Truncate(), b.Truncate(), c.Truncate(), FlippedNormals); }
        
        public readonly bool SharesVertex(Vector2 p) { return a == p || b == p || c == p; }
        public readonly bool SharesVertex(IEnumerable<Vector2> points)
        {
            foreach (var p in points)
            {
                if (SharesVertex(p)) return true;
            }
            return false;
        }
        public readonly bool SharesVertex(Triangle t) { return SharesVertex(t.a) || SharesVertex(t.b) || SharesVertex(t.c); }
        
        public bool IsValid() { return GetArea() > 0f; }
        public readonly bool IsNarrow(float narrowValue = 0.2f)
        {
            Points points = new() { a, b, c };
            for (int i = 0; i < 3; i++)
            {
                Vector2 a = points[i];
                Vector2 b = SUtils.GetItem(points, i + 1);
                Vector2 c = SUtils.GetItem(points, i - 1);

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
        /// <param name="t">The triangle to find a point in.</param>
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
            Vector2 ac = (c - a) * f1;
            Vector2 ab = (b - a) * f2;
            return a + ac + ab;
            //float f1Sq = MathF.Sqrt(f1);
            //float x = (1f - f1Sq) * t.a.X + (f1Sq * (1f - f2)) * t.b.X + (f1Sq * f2) * t.c.X;
            //float y = (1f - f1Sq) * t.a.Y + (f1Sq * (1f - f2)) * t.b.Y + (f1Sq * f2) * t.c.Y;
            //return new(x, y);
        }
        public readonly Circle GetCircumCircle()
        {
            Vector2 SqrA = new Vector2(a.X * a.X, a.Y * a.Y);
            Vector2 SqrB = new Vector2(b.X * b.X, b.Y * b.Y); 
            Vector2 SqrC = new Vector2(c.X * c.X, c.Y * c.Y);

            float D = (a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y)) * 2f;
            float x = ((SqrA.X + SqrA.Y) * (b.Y - c.Y) + (SqrB.X + SqrB.Y) * (c.Y - a.Y) + (SqrC.X + SqrC.Y) * (a.Y - b.Y)) / D;
            float y = ((SqrA.X + SqrA.Y) * (c.X - b.X) + (SqrB.X + SqrB.Y) * (a.X - c.X) + (SqrC.X + SqrC.Y) * (b.X - a.X)) / D;

            Vector2 center = new Vector2(x, y);
            float r = (a - center).Length();
            return new(center, r);
        }

        public readonly Triangulation Triangulate(int pointCount)
        {
            if (pointCount < 0) return new() { new(a, b, c, FlippedNormals) };

            Points points = new() { a, b, c };

            for (int i = 0; i < pointCount; i++)
            {
                float f1 = SRNG.randF();
                float f2 = SRNG.randF();
                Vector2 randPoint = GetPoint(f1, f2);
                points.Add(randPoint);
            }

            return Polygon.TriangulateDelaunay(points);
        }
        public Triangulation Triangulate(float minArea)
        {
            if (minArea <= 0) return new() { new(a,b,c,FlippedNormals) };

            float triArea = GetArea();
            float pieceCount = triArea / minArea;
            int points = (int)MathF.Floor((pieceCount - 1f) * 0.5f);
            return Triangulate(points);
        }
        public readonly Triangulation Triangulate(Vector2 p)
        {
            return new()
            {
                new(a, b, p),
                new(b, c, p),
                new(c, a, p)
            };
        }
        
        public readonly Triangle GetInsideTriangle(float abF, float bcF, float caF)
        {
            Vector2 newA = SVec.Lerp(a, b, abF);
            Vector2 newB = SVec.Lerp(b, c, bcF);
            Vector2 newC = SVec.Lerp(c, a, caF);
            return new(newA, newB, newC);
        }

        
        public Triangle Rotate(float rad) { return Rotate(GetCentroid(), rad); }
        public readonly Triangle Rotate(Vector2 pivot, float rad)
        {
            Vector2 newA = pivot + (a - pivot).Rotate(rad);
            Vector2 newB = pivot + (b - pivot).Rotate(rad);
            Vector2 newC = pivot + (c - pivot).Rotate(rad);
            return new(newA, newB, newC, FlippedNormals);
        }
        public readonly Triangle Scale(float scale) { return new(a * scale, b * scale, c * scale, FlippedNormals); }
        public readonly Triangle Scale(Vector2 scale) { return new(a * scale, b * scale, c * scale, FlippedNormals); }
        public readonly Triangle Scale(Vector2 pivot, float scale)
        {
            Vector2 newA = pivot + (a - pivot) * scale;
            Vector2 newB = pivot + (b - pivot) * scale;
            Vector2 newC = pivot + (c - pivot) * scale;
            return new(newA, newB, newC, FlippedNormals);
        }
        public readonly Triangle Scale(Vector2 pivot, Vector2 scale)
        {
            Vector2 newA = pivot + (a - pivot) * scale;
            Vector2 newB = pivot + (b - pivot) * scale;
            Vector2 newC = pivot + (c - pivot) * scale;
            return new(newA, newB, newC, FlippedNormals);
        }
        public readonly Triangle Move(Vector2 offset) { return new(a + offset, b + offset, c + offset, FlippedNormals); }
        #endregion

        #region Equality & HashCode
        public bool Equals(Triangle other)
        {
            return a == other.a && b == other.b && c == other.c;
        }
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(a, b, c);
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

        #region IShape
        public Vector2 GetCentroid() { return (a + b + c) / 3; }
        public Points GetVertices() { return new(a, b, c); }
        public Polygon ToPolygon() { return new(a, b, c); }
        public Polyline ToPolyline() { return new(a, b, c); }
        public Segments GetEdges() 
        {
            Segment A = new Segment(a, b, FlippedNormals);
            Segment B = new Segment(b, c, FlippedNormals);
            Segment C = new Segment(c, a, FlippedNormals);
            return new() { A, B, C };
        }
        public Triangulation Triangulate() { return this.Triangulate(GetCentroid()); }
        public Circle GetBoundingCircle() { return GetCircumCircle(); } // ToPolygon().GetBoundingCircle(); }
        public float GetCircumference() { return MathF.Sqrt(GetCircumferenceSquared()); }
        public float GetCircumferenceSquared() { return A.LengthSquared() + B.LengthSquared() + C.LengthSquared(); }
        public float GetArea() 
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

            return MathF.Abs((a.X - c.X) * (b.Y - c.Y) - (a.Y - c.Y) * (b.X - c.X)) / 2f;
        }
        public Rect GetBoundingBox() { return new Rect(a.X, a.Y, 0, 0).Enlarge(b).Enlarge(c); }
        public bool IsPointInside(Vector2 p) { return SGeometry.IsPointInTriangle(a, b, c, p); }
        public CollisionPoint GetClosestPoint(Vector2 p) { return ToPolygon().GetClosestPoint(p); }
        public Vector2 GetClosestVertex(Vector2 p) { return ToPolygon().GetClosestVertex(p); }
        public Vector2 GetRandomPoint() { return this.GetPoint(SRNG.randF(), SRNG.randF()); }
        public Points GetRandomPoints(int amount)
        {
            var points = new Points();
            for (int i = 0; i < amount; i++)
            {
                points.Add(GetRandomPoint());
            }
            return points;
        }
        public Vector2 GetRandomVertex() { return SRNG.randCollection(GetVertices(), false); }
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
        public void DrawShape(float linethickness, Raylib_CsLo.Color color) => this.DrawLines(linethickness, color);
        #endregion




        //public float GetWidestAngle()
        //{
        //    float angleA = MathF.Abs((b - a).Cross(c - a));
        //    float angleB = MathF.Abs((c - b).Cross(a - b));
        //    float angleC = MathF.Abs((a - c).Cross(b - c));
        //    if(angleA < angleB)
        //    {
        //        if (angleA < angleC) return angleA;
        //        else return angleC;
        //    }
        //    else
        //    {
        //        if (angleB < angleC) return angleB;
        //        else return angleC;
        //    }
        //}
        //public SegmentShape GetSegmentShape() { return new(GetCentroid(), new(a, b), new(b, c), new(c, a) ); }
        //public Vector2 GetReferencePoint() { return GetCentroid(); }
    }

    public struct Rect : IShape, IEquatable<Rect>
    {
        #region Members
        public float X;
        public float Y;
        public float Width;
        public float Height;
        #endregion

        #region Getter Setter
        public bool FlippedNormals { get; set; } = false;
        public Vector2 TopLeft { get { return new Vector2(X, Y); } }
        public Vector2 TopRight { get { return new Vector2(X + Width, Y); } }
        public Vector2 BottomRight { get { return new Vector2(X + Width, Y + Height); } }
        public Vector2 BottomLeft { get { return new Vector2(X, Y + Height); } }
        public Vector2 Center { get { return new Vector2(X + Width * 0.5f, Y + Height * 0.5f); } }

        public float Top { get { return Y; } }
        public float Bottom { get { return Y + Height; } }
        public float Left { get { return X; } }
        public float Right { get { return X + Width; } }
        public Vector2 Size { get { return new Vector2(Width, Height); } }
        public Rectangle Rectangle { get { return new(X, Y, Width, Height); } }
        #endregion

        #region Constructors
        public Rect(float x, float y, float width, float height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }
        public Rect(Vector2 topLeft, Vector2 bottomRight)
        {
            var final = SRect.Fix(topLeft, bottomRight);
            this.X = final.topLeft.X;
            this.Y = final.topLeft.Y;
            this.Width = final.bottomRight.X - this.X;
            this.Height = final.bottomRight.Y - this.Y;
            //if (topLeft.X > bottomRight.X)
            //{
            //    this.x = bottomRight.X;
            //    this.width = topLeft.X - bottomRight.X;
            //}
            //else
            //{
            //    this.x = topLeft.X;
            //    this.width = bottomRight.X - topLeft.X;
            //}
            //
            //if (topLeft.Y > bottomRight.Y)
            //{
            //    this.y = bottomRight.Y;
            //    this.height = topLeft.Y - bottomRight.Y;
            //}
            //else
            //{
            //    this.y = topLeft.Y;
            //    this.height = bottomRight.Y - topLeft.Y;
            //}
        }
        public Rect(Vector2 pos, Vector2 size, Vector2 alignement)
        {
            Vector2 offset = size * alignement;
            Vector2 topLeft = pos - offset;
            this.X = topLeft.X;
            this.Y = topLeft.Y;
            this.Width = size.X;
            this.Height = size.Y;
        }
        public Rect(Rectangle rect)
        {
            this.X = rect.X;
            this.Y = rect.Y;
            this.Width = rect.width;
            this.Height = rect.height;
        }
        #endregion

        #region Equality & HashCode
        public bool Equals(Rect other)
        {
            return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
        }
        public static bool operator ==(Rect left, Rect right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(Rect left, Rect right)
        {
            return !(left == right);
        }
        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is Rect r) return Equals(r);
            return false;
        }
        public override readonly int GetHashCode() => HashCode.Combine(X, Y, Width, Height);
        #endregion

        #region Public
        public readonly Rect Floor()
        {
            unchecked
            {
                return new Rect(
                    MathF.Floor(X),
                    MathF.Floor(Y),
                    MathF.Floor(Width),
                    MathF.Floor(Height));
            }
        }
        public readonly Rect Ceiling()
        {
            unchecked
            {
                return new Rect(
                    MathF.Ceiling(X),
                    MathF.Ceiling(Y),
                    MathF.Ceiling(Width),
                    MathF.Ceiling(Height));
            }
        }
        public readonly Rect Truncate()
        {
            unchecked
            {
                return new Rect(
                    MathF.Truncate(X),
                    MathF.Truncate(Y),
                    MathF.Truncate(Width),
                    MathF.Truncate(Height));
            }
        }
        public readonly Rect Round()
        {
            unchecked
            {
                return new Rect(
                    MathF.Round(X),
                    MathF.Round(Y),
                    MathF.Round(Width),
                    MathF.Round(Height));
            }
        }
        
        public readonly bool ContainsRect(Rect rect) =>
            (X <= rect.X) && (rect.X + rect.Width <= X + Width) &&
            (Y <= rect.Y) && (rect.Y + rect.Height <= Y + Height);
        /// <summary>
        /// Creates a rect that represents the intersection between a and b. If there is no intersection, an
        /// empty rect is returned.
        /// </summary>
        public Rect IntersectWith(Rect rect)
        {

            float x1 = MathF.Max(X, rect.X);
            float x2 = MathF.Min(Right, rect.Right);
            float y1 = MathF.Max(Y, rect.Y);
            float y2 = MathF.Min(Bottom, rect.Bottom);

            if (x2 >= x1 && y2 >= y1)
            {
                return new Rect(x1, y1, x2 - x1, y2 - y1);
            }

            return new();
        }
        /// <summary>
        /// Creates a rectangle that represents the union between a and b.
        /// </summary>
        public Rect CombineWith(Rect rect)
        {
            float x1 = MathF.Min(X, rect.X);
            float x2 = MathF.Max(Right, rect.Right);
            float y1 = MathF.Min(Y, rect.Y);
            float y2 = MathF.Max(Bottom, rect.Bottom);

            return new Rect(x1, y1, x2 - x1, y2 - y1);
        }

        public (Rect left, Rect right) SplitVertical(float f)
        {
            Vector2 topPoint = TopLeft.Lerp(TopRight, f);
            Vector2 bottomPoint = BottomLeft.Lerp(BottomRight, f);
            Rect left = new(TopLeft, bottomPoint);
            Rect right = new(topPoint, BottomRight);
            return (left, right);
        }
        public (Rect top, Rect bottom) SplitHorizontal(float f)
        {
            Vector2 leftPoint = TopLeft.Lerp(BottomLeft, f);
            Vector2 rightPoint = TopRight.Lerp(BottomRight, f);
            Rect top = new(TopLeft, rightPoint);
            Rect bottom = new(leftPoint, BottomRight);
            return (top, bottom);
        }
        public (Rect topLeft, Rect bottomLeft, Rect bottomRight, Rect TopRight) Split(float horizontal, float vertical)
        {
            var hor = SplitHorizontal(horizontal);
            var top = hor.top.SplitVertical(vertical);
            var bottom = hor.bottom.SplitVertical(vertical);
            return (top.left, bottom.left, bottom.right, top.right);
        }

        public Segment GetLeftSegment()
        {
            return new(TopLeft, BottomLeft, FlippedNormals);
        }
        public Segment GetBotomSegment()
        {
            return new(BottomLeft, BottomRight, FlippedNormals);
        }
        public Segment GetRightSegment()
        {
            return new(BottomRight, TopRight, FlippedNormals);
        }
        public Segment GetTopSegment()
        {
            return new(TopRight, TopLeft, FlippedNormals);
        }
        #endregion

        #region IShape
        public Vector2 GetCentroid() { return Center; }
        public Points GetVertices() { return new() { TopLeft, BottomLeft, BottomRight, TopRight }; }
        public Polygon ToPolygon() { return new() { TopLeft, BottomLeft, BottomRight, TopRight }; }
        public Polyline ToPolyline() { return new() { TopLeft, BottomLeft, BottomRight, TopRight }; }
        public Segments GetEdges() 
        {
            Vector2 A = TopLeft;
            Vector2 B = BottomLeft;
            Vector2 C = BottomRight;
            Vector2 D = TopRight;

            Segment left = new(A, B, FlippedNormals);
            Segment bottom = new(B, C, FlippedNormals);
            Segment right = new(C, D, FlippedNormals);
            Segment top = new(D, A, FlippedNormals);
            return new() { left, bottom, right, top };
        }
        public Triangulation Triangulate() { return ToPolygon().Triangulate(); }
        public Circle GetBoundingCircle() { return ToPolygon().GetBoundingCircle(); }
        public float GetCircumference() { return Width * 2 + Height * 2; }
        public float GetCircumferenceSquared() { return GetCircumference() * GetCircumference(); }
        public float GetArea() { return Width * Height; }
        public Rect GetBoundingBox() { return this; }
        public bool IsPointInside(Vector2 p) { return SGeometry.IsPointInRect(p, TopLeft, Size); }
        public CollisionPoint GetClosestPoint(Vector2 p) { return ToPolygon().GetClosestPoint(p); }
        public Vector2 GetClosestVertex(Vector2 p) { return ToPolygon().GetClosestVertex(p); }
        public Vector2 GetRandomPoint() { return new(SRNG.randF(X, X + Width), SRNG.randF(Y, Y + Height)); }
        public Points GetRandomPoints(int amount)
        {
            var points = new Points();
            for (int i = 0; i < amount; i++)
            {
                points.Add(GetRandomPoint());
            }
            return points;
        }
        public Vector2 GetRandomVertex() { return SRNG.randCollection(ToPolygon(), false); }
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

        public void DrawShape(float linethickness, Raylib_CsLo.Color color) => this.DrawLines(linethickness, color);
        #endregion

    }

    /// <summary>
    /// Points shoud be in CCW order.
    /// </summary>
    public class Polygon : List<Vector2>, IShape, IEquatable<Polygon>
    {
        #region Constructors
        public Polygon() { }
        /// <summary>
        /// Points should be in CCW order. Use Reverse if they are in CW order.
        /// </summary>
        /// <param name="points"></param>
        public Polygon(params Vector2[] points) { AddRange(points); }
        /// <summary>
        /// Points should be in CCW order. Use Reverse if they are in CW order.
        /// </summary>
        /// <param name="points"></param>
        public Polygon(IEnumerable<Vector2> points) { AddRange(points); }
        /// <summary>
        /// Points should be in CCW order. Use Reverse if they are in CW order.
        /// </summary>
        /// <param name="points"></param>
        public Polygon(IShape shape) { AddRange(shape.ToPolygon()); }
        public Polygon(Polygon poly) { AddRange(poly); }
        public Polygon(Polyline polyLine) { AddRange(polyLine); }
        #endregion

        #region Equals & Hashcode
        public bool Equals(Polygon? other)
        {
            if (other == null) return false;
            if (Count != other.Count) return false;
            for (int i = 0; i < Count; i++)
            {
                if (this[i] != other[i]) return false;
            }
            return true;
        }
        public override int GetHashCode() { return SUtils.GetHashCode(this); }
        #endregion

        #region Getter Setter
        public bool FlippedNormals { get; set; } = false;
        #endregion

        #region Public
        public Polygon Copy() { return new(this); }
        public void FixWindingOrder() { if (this.IsClockwise()) this.Reverse(); }
        public void ReduceVertexCount(int newCount)
        {
            if (newCount < 3) Clear();//no points left to form a polygon

            while (Count > newCount)
            {
                float minD = 0f;
                int shortestID = 0;
                for (int i = 0; i < Count; i++)
                {
                    float d = (this[i] - this[(i + 1) % Count]).LengthSquared();
                    if (d > minD)
                    {
                        minD = d;
                        shortestID = i;
                    }
                }
                RemoveAt(shortestID);
            }

        }
        public void ReduceVertexCount(float factor) { ReduceVertexCount(Count - (int)Count * factor); }
        public void IncreaseVertexCount(int newCount)
        {
            if (newCount <= Count) return;

            while (Count < newCount)
            {
                float maxD = 0f;
                int longestID = 0;
                for (int i = 0; i < Count; i++)
                {
                    float d = (this[i] - this[(i + 1) % Count]).LengthSquared();
                    if (d > maxD)
                    {
                        maxD = d;
                        longestID = i;
                    }
                }
                Vector2 m = (this[longestID] + this[(longestID + 1) % Count]) * 0.5f;
                this.Insert(longestID + 1, m);
            }
        }
        public Vector2 GetVertex(int index)
        {
            return this[SUtils.WrapIndex(Count, index)];
        }

        public void Floor() { Points.Floor(this); }
        public void Ceiling() { Points.Ceiling(this); }
        public void Truncate() { Points.Truncate(this); }
        public void Round() { Points.Round(this); }

        /// <summary>
        /// Computes the length of this polygon's apothem. This will only be valid if
        /// the polygon is regular. More info: http://en.wikipedia.org/wiki/Apothem
        /// </summary>
        /// <param name="p"></param>
        /// <returns>Return the length of the apothem.</returns>
        public float GetApothem()
        {
            return (this.GetCentroid() - (this[0].Lerp(this[1], 0.5f))).Length();
        }
        public Vector2 GetRandomPointConvex()
        {
            var edges = GetEdges();
            var ea = SRNG.randCollection(edges, true);
            var eb = SRNG.randCollection(edges);

            var pa = ea.Start.Lerp(ea.End, SRNG.randF());
            var pb = eb.Start.Lerp(eb.End, SRNG.randF());
            return pa.Lerp(pb, SRNG.randF());
        }

        public void Center(Vector2 newCenter)
        {
            var centroid = GetCentroid();
            var delta = newCenter - centroid;
            Move(delta);
        }
        public void Move(Vector2 translation)
        {
            for (int i = 0; i < Count; i++)
            {
                this[i] += translation;
            }
            //return path;
        }
        public void Rotate(Vector2 pivot, float rotRad)
        {
            if (Count < 3) return;
            for (int i = 0; i < Count; i++)
            {
                Vector2 w = this[i] - pivot;
                this[i] = pivot + w.Rotate(rotRad);
            }
            //return path;
        }
        public void Rotate(float rotRad)
        {
            if (Count < 3) return;// new();
            for (int i = 0; i < Count; i++)
            {
                this[i] = this[i].Rotate(rotRad);
            }
            //return path;
        }
        public void Scale(float scale)
        {
            for (int i = 0; i < Count; i++)
            {
                this[i] *= scale;
            }
            //return path;
        }
        public void Scale(Vector2 pivot, float scale)
        {
            if (Count < 3) return;
            for (int i = 0; i < Count; i++)
            {
                Vector2 w = this[i] - pivot;
                this[i] = pivot + w * scale;
            }
        }
        public void Scale(Vector2 pivot, Vector2 scale)
        {
            if (Count < 3) return;// new();
            for (int i = 0; i < Count; i++)
            {
                Vector2 w = this[i] - pivot;
                this[i] = pivot + w * scale;
            }
            //return path;
        }
        public void ScaleUniform(float distance)
        {
            for (int i = 0; i < Count; i++)
            {
                this[i] = SVec.ScaleUniform(this[i], distance);
            }
        }

        public void RemoveColinearVertices()
        {
            if (Count < 3) return;
            Points result = new();
            for (int i = 0; i < Count; i++)
            {
                Vector2 cur = this[i];
                Vector2 prev = SUtils.GetItem(this, i - 1);
                Vector2 next = SUtils.GetItem(this, i + 1);

                Vector2 prevCur = prev - cur;
                Vector2 nextCur = next - cur;
                if (prevCur.Cross(nextCur) != 0f) result.Add(cur);
            }
            Clear();
            AddRange(result);
        }
        public void RemoveDuplicates(float toleranceSquared = 0.001f)
        {
            if (Count < 3) return;
            Points result = new();

            for (int i = 0; i < Count; i++)
            {
                Vector2 cur = this[i];
                Vector2 next = SUtils.GetItem(this, i + 1);
                if ((cur - next).LengthSquared() > toleranceSquared) result.Add(cur);
            }
            Clear();
            AddRange(result);
        }
        public void Smooth(float amount, float baseWeight)
        {
            if (Count < 3) return;
            Points result = new();
            Vector2 centroid = GetCentroid();
            for (int i = 0; i < Count; i++)
            {
                Vector2 cur = this[i];
                Vector2 prev = this[SUtils.WrapIndex(Count, i - 1)];
                Vector2 next = this[SUtils.WrapIndex(Count, i + 1)];
                Vector2 dir = (prev - cur) + (next - cur) + ((cur - centroid) * baseWeight);
                result.Add(cur + dir * amount);
            }

            Clear();
            AddRange(result);
        }

        public (Polygons newShapes, Polygons cutOuts) Cut(Polygon cutShape)
        {
            var cutOuts = SClipper.Intersect(this, cutShape).ToPolygons(true);
            var newShapes = SClipper.Difference(this, cutShape).ToPolygons(true);

            return (newShapes, cutOuts);
        }
        public (Polygons newShapes, Polygons cutOuts) CutMany(Polygons cutShapes)
        {
            var cutOuts = SClipper.IntersectMany(this, cutShapes).ToPolygons(true);
            var newShapes = SClipper.DifferenceMany(this, cutShapes).ToPolygons(true);
            return (newShapes, cutOuts);
        }
        public (Polygons newShapes, Polygons overlaps) Combine(Polygon other)
        {
            var overlaps = SClipper.Intersect(this, other).ToPolygons(true);
            var newShapes = SClipper.Union(this, other).ToPolygons(true);
            return (newShapes, overlaps);
        }
        public (Polygons newShapes, Polygons overlaps) Combine(Polygons others)
        {
            var overlaps = SClipper.IntersectMany(this, others).ToPolygons(true);
            var newShapes = SClipper.UnionMany(this, others).ToPolygons(true);
            return (newShapes, overlaps);
        }
        public (Polygons newShapes, Polygons cutOuts) CutSimple(Vector2 cutPos, float minCutRadius, float maxCutRadius, int pointCount = 16)
        {
            var cut = Generate(cutPos, pointCount, minCutRadius, maxCutRadius);
            return this.Cut(cut);
        }
        public (Polygons newShapes, Polygons cutOuts) CutSimple(Segment cutLine, float minSectionLength = 0.025f, float maxSectionLength = 0.1f, float minMagnitude = 0.05f, float maxMagnitude = 0.25f)
        {
            var cut = Generate(cutLine, minMagnitude, maxMagnitude, minSectionLength, maxSectionLength);
            return this.Cut(cut);
        }
       
        #endregion

        #region Static
        /// <summary>
        /// Triangulates a set of points. Only works with non self intersecting shapes.
        /// </summary>
        /// <param name="points">The points to triangulate. Can be any set of points. (polygons as well) </param>
        /// <returns></returns>
        public static Triangulation TriangulateDelaunay(IEnumerable<Vector2> points)
        {
            Triangle supraTriangle = GetBoundingTriangle(points, 2f);
            return TriangulateDelaunay(points, supraTriangle);
        }
        /// <summary>
        /// Triangulates a set of points. Only works with non self intersecting shapes.
        /// </summary>
        /// <param name="points">The points to triangulate. Can be any set of points. (polygons as well) </param>
        /// <param name="supraTriangle">The triangle that encapsulates all the points.</param>
        /// <returns></returns>
        public static Triangulation TriangulateDelaunay(IEnumerable<Vector2> points, Triangle supraTriangle)
        {
            Triangulation triangles = new();

            triangles.Add(supraTriangle);

            foreach (var p in points)
            {
                Triangulation badTriangles = new();

                //Identify 'bad triangles'
                for (int triIndex = triangles.Count - 1; triIndex >= 0; triIndex--)
                {
                    Triangle triangle = triangles[triIndex];

                    //A 'bad triangle' is defined as a triangle who's CircumCentre contains the current point
                    var circumCircle = triangle.GetCircumCircle();
                    float distSq = Vector2.DistanceSquared(p, circumCircle.Center);
                    if (distSq < circumCircle.Radius * circumCircle.Radius)
                    {
                        badTriangles.Add(triangle);
                        triangles.RemoveAt(triIndex);
                    }
                }

                Segments allEdges = new();
                foreach (var badTriangle in badTriangles) { allEdges.AddRange(badTriangle.GetEdges()); }

                Segments uniqueEdges = allEdges.GetUniqueSegments();
                //Create new triangles
                for (int i = 0; i < uniqueEdges.Count; i++)
                {
                    var edge = uniqueEdges[i];
                    triangles.Add(new(p, edge));
                }
            }

            //Remove all triangles that share a vertex with the supra triangle to recieve the final triangulation
            for (int i = triangles.Count - 1; i >= 0; i--)
            {
                var t = triangles[i];
                if (t.SharesVertex(supraTriangle)) triangles.RemoveAt(i);
            }


            return triangles;
        }
        /// <summary>
        /// Get a rect that encapsulates all points.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static Rect GetBoundingBox(IEnumerable<Vector2> points)
        {
            if (points.Count() < 2) return new();
            Vector2 start = points.First();
            Rect r = new(start.X, start.Y, 0, 0);

            foreach (var p in points)
            {
                r = SRect.Enlarge(r, p);
            }
            return r;
        }
        /// <summary>
        /// Get a triangle the encapsulates all points.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="marginFactor"> A factor for scaling the final triangle.</param>
        /// <returns></returns>
        public static Triangle GetBoundingTriangle(IEnumerable<Vector2> points, float marginFactor = 1f)
        {
            var bounds = GetBoundingBox(points);
            float dMax = SVec.Max(bounds.Size) * marginFactor; // SVec.Max(bounds.BottomRight - bounds.BottomLeft) + margin; //  Mathf.Max(bounds.maxX - bounds.minX, bounds.maxY - bounds.minY) * Margin;
            Vector2 center = bounds.Center;

            ////The float 0.866 is an arbitrary value determined for optimum supra triangle conditions.
            //float x1 = center.X - 0.866f * dMax;
            //float x2 = center.X + 0.866f * dMax;
            //float x3 = center.X;
            //
            //float y1 = center.Y - 0.5f * dMax;
            //float y2 = center.Y - 0.5f * dMax;
            //float y3 = center.Y + dMax;
            //
            //Vector2 a = new(x1, y1);
            //Vector2 b = new(x2, y2);
            //Vector2 c = new(x3, y3);

            Vector2 a = new Vector2(center.X, bounds.BottomLeft.Y + dMax);
            Vector2 b = new Vector2(center.X - dMax * 1.25f, bounds.TopLeft.Y - dMax / 4);
            Vector2 c = new Vector2(center.X + dMax * 1.25f, bounds.TopLeft.Y - dMax / 4);


            return new Triangle(a, b, c);
        }
        
        
        public static List<Vector2> GetSegmentAxis(Polygon p, bool normalized = false)
        {
            if (p.Count <= 1) return new();
            else if (p.Count == 2)
            {
                return new() { p[1] - p[0] };
            }
            List<Vector2> axis = new();
            for (int i = 0; i < p.Count; i++)
            {
                Vector2 start = p[i];
                Vector2 end = p[(i + 1) % p.Count];
                Vector2 a = end - start;
                axis.Add(normalized ? SVec.Normalize(a) : a);
            }
            return axis;
        }
        public static List<Vector2> GetSegmentAxis(Segments edges, bool normalized = false)
        {
            List<Vector2> axis = new();
            foreach (var seg in edges)
            {
                axis.Add(normalized ? seg.Dir : seg.Displacement);
            }
            return axis;
        }

        public static Polygon GetShape(Points relative, Vector2 pos, float rotRad, Vector2 scale)
        {
            if (relative.Count < 3) return new();
            Polygon shape = new();
            for (int i = 0; i < relative.Count; i++)
            {
                shape.Add(pos + SVec.Rotate(relative[i], rotRad) * scale);
            }
            return shape;
        }
        public static Points GenerateRelative(int pointCount, float minLength, float maxLength)
        {
            Points points = new();
            float angleStep = RayMath.PI * 2.0f / pointCount;

            for (int i = 0; i < pointCount; i++)
            {
                float randLength = SRNG.randF(minLength, maxLength);
                Vector2 p = SVec.Rotate(SVec.Right(), -angleStep * i) * randLength;
                points.Add(p);
            }
            return points;
        }
        
        public static Polygon Generate(Vector2 center, int pointCount, float minLength, float maxLength)
        {
            Polygon points = new();
            float angleStep = RayMath.PI * 2.0f / pointCount;

            for (int i = 0; i < pointCount; i++)
            {
                float randLength = SRNG.randF(minLength, maxLength);
                Vector2 p = SVec.Rotate(SVec.Right(), -angleStep * i) * randLength;
                p += center;
                points.Add(p);
            }
            return points;
        }
        /// <summary>
        /// Generates a polygon around the given segment. Points are generated ccw around the segment beginning with the segment start.
        /// </summary>
        /// <param name="segment">The segment to build a polygon around.</param>
        /// <param name="magMin">The minimum perpendicular magnitude factor for generating a point. (0-1)</param>
        /// <param name="magMax">The maximum perpendicular magnitude factor for generating a point. (0-1)</param>
        /// <param name="minSectionLength">The minimum factor of the length between points along the line.(0-1)</param>
        /// <param name="maxSectionLength">The maximum factor of the length between points along the line.(0-1)</param>
        /// <returns>Returns the a generated polygon.</returns>
        public static Polygon Generate(Segment segment, float magMin = 0.1f, float magMax = 0.25f, float minSectionLength = 0.025f, float maxSectionLength = 0.1f)
        {
            Polygon poly = new() { segment.Start };
            var dir = segment.Dir;
            var dirRight = dir.GetPerpendicularRight();
            var dirLeft = dir.GetPerpendicularLeft();
            float len = segment.Length;
            float minSectionLengthSq = (minSectionLength * len) * (minSectionLength * len);
            Vector2 cur = segment.Start;
            while (true)
            {
                cur += dir * SRNG.randF(minSectionLength, maxSectionLength) * len;
                if ((cur - segment.End).LengthSquared() < minSectionLengthSq) break;
                poly.Add(cur + dirRight * SRNG.randF(magMin, magMax));
            }
            cur = segment.End;
            poly.Add(cur);
            while (true)
            {
                cur -= dir * SRNG.randF(minSectionLength, maxSectionLength) * len;
                if ((cur - segment.Start).LengthSquared() < minSectionLengthSq) break;
                poly.Add(cur + dirLeft * SRNG.randF(magMin, magMax));
            }
            return poly;
        }

        public static Polygon Center(Polygon p, Vector2 newCenter)
        {
            var centroid = p.GetCentroid();
            var delta = newCenter - centroid;
            return Move(p, delta);
        }
        public static Polygon Move(Polygon p, Vector2 translation)
        {
            Polygon result = new();
            for (int i = 0; i < p.Count; i++)
            {
                result.Add(p[i] + translation);
            }
            return result;
        }
        public static Polygon Rotate(Polygon p, Vector2 pivot, float rotRad)
        {
            if (p.Count < 3) return new();
            Polygon rotated = new();
            for (int i = 0; i < p.Count; i++)
            {
                Vector2 w = p[i] - pivot;
                rotated.Add(pivot + w.Rotate(rotRad));
            }
            return rotated;
        }
        public static Polygon Scale(Polygon p, float scale)
        {
            Polygon shape = new();
            for (int i = 0; i < p.Count; i++)
            {
                shape.Add(p[i] * scale);
            }
            return shape;
        }
        public static Polygon Scale(Polygon p, Vector2 pivot, float scale)
        {
            if (p.Count < 3) return new();
            Polygon scaled = new();
            for (int i = 0; i < p.Count; i++)
            {
                Vector2 w = p[i] - pivot;
                scaled.Add(pivot + w * scale);
            }
            return scaled;
        }
        public static Polygon Scale(Polygon p, Vector2 pivot, Vector2 scale)
        {
            if (p.Count < 3) return new();
            Polygon scaled = new();
            for (int i = 0; i < p.Count; i++)
            {
                Vector2 w = p[i] - pivot;
                scaled.Add(pivot + w * scale);
            }
            return scaled;
        }
        public static Polygon ScaleUniform(Polygon p, float distance)
        {
            Polygon shape = new();
            for (int i = 0; i < p.Count; i++)
            {
                shape.Add(SVec.ScaleUniform(p[i], distance));
            }
            return shape;
        }
        #endregion

        #region IShape
        public Vector2 GetCentroid()
        {
            //return GetCentroidMean();
            Vector2 result = new();
            
            for (int i = 0; i < Count; i++)
            {
                Vector2 a = this[i];
                Vector2 b = this[(i + 1) % Count];
                //float factor = a.X * b.Y - b.X * a.Y; //clockwise 
                float factor = a.Y * b.X - a.X * b.Y; //counter clockwise
                result.X += (a.X + b.X) * factor;
                result.Y += (a.Y + b.Y) * factor;
            }
            
            return result * (1f / (GetArea() * 6f));
        }
        public Vector2 GetCentroidMean()
        {
            if (Count <= 0) return new(0f);
            Vector2 total = new(0f);
            foreach (Vector2 p in this) { total += p; }
            return total / Count;
        }
        public Triangulation Triangulate()
        {
            if (Count < 3) return new();
            else if (Count == 3) return new() { new(this[0], this[1], this[2]) };

            Triangulation triangles = new();
            List<Vector2> vertices = new();
            vertices.AddRange(this);
            List<int> validIndices = new();
            for (int i = 0; i < vertices.Count; i++)
            {
                validIndices.Add(i);
            }
            while (vertices.Count > 3)
            {
                if (validIndices.Count <= 0) 
                    break;

                int i = validIndices[SRNG.randI(0, validIndices.Count)];
                Vector2 a = vertices[i];
                Vector2 b = SUtils.GetItem(vertices, i + 1);
                Vector2 c = SUtils.GetItem(vertices, i - 1);

                Vector2 ba = b - a;
                Vector2 ca = c - a;
                float cross = ba.Cross(ca);
                if (cross >= 0f)//makes sure that ear is not self intersecting
                {
                    validIndices.Remove(i);
                    continue;
                }

                Triangle t = new(a, b, c);

                bool isValid = true;
                foreach (var p in this)
                {
                    if (p == a || p == b || p == c) continue;
                    if (t.IsPointInside(p))
                    {
                        isValid = false;
                        break;
                    }
                }

                if (isValid)
                {
                    triangles.Add(t);
                    vertices.RemoveAt(i);

                    validIndices.Clear();
                    for (int j = 0; j < vertices.Count; j++)
                    {
                        validIndices.Add(j);
                    }
                    //break;
                }
            }


            triangles.Add(new(vertices[0], vertices[1], vertices[2]));


            return triangles;
        }

        //private Triangulation Subdivide(Triangle triangle, float minArea)
        //{
        //    var area = triangle.GetArea();
        //    Triangulation final = new();
        //    if (minArea > area / 3)
        //    {
        //        final.Add(triangle);
        //    }
        //    else
        //    {
        //        var triangulation = triangle.Triangulate();
        //        foreach (var tri in triangulation)
        //        {
        //            final.AddRange(Subdivide(tri, minArea));
        //        }
        //    }
        //    return final;
        //}
        
        ///// <summary>
        ///// Triangulate this polygon. 
        ///// </summary>
        ///// <param name="minArea">The minimum area a triangle must have to be further subdivided. Does not affect the initial triangulation.</param>
        ///// <param name="subdivisions">A subdivision triangulates all triangles from the previous triangulation. (Do not go big!) </param>
        ///// <returns></returns>
        //public Triangulation Fracture(float minArea = -1, int subdivisions = 0)
        //{
        //    var triangulation = Triangulate();
        //    if (subdivisions <= 0) return triangulation;
        //    else
        //    {
        //        return Subdivide(triangulation, subdivisions, minArea);
        //    }
        //}
        //private Triangulation Subdivide(Triangulation triangles, int remaining, float minArea = -1)
        //{
        //    if(remaining <= 0) return triangles;
        //    Triangulation subdivision = new();
        //    foreach (var tri in triangles)
        //    {
        //        var area = tri.GetArea();
        //        //tri.GetRandomPoint()
        //        if(minArea <= 0 || tri.GetArea() >= minArea) subdivision.AddRange(tri.Triangulate());
        //        else subdivision.Add(tri);
        //
        //    }
        //    return Subdivide(subdivision, remaining - 1, minArea);
        //}
        /*
        //only works with simple polygons that is why the ear clipper algorithm is used.
        public Triangulation Fracture(int fractureComplexity = 0)//fix delauny triangulation
        {
            if (fractureComplexity <= 0) return SPoly.TriangulateDelaunay(this);

            List<Vector2> points = new();
            points.AddRange(this);
            points.AddRange(GetRandomPoints(fractureComplexity));
            return SPoly.TriangulateDelaunay(points);
        }*/

        /// <summary>
        /// Return the segments of the polygon. If the points are in ccw winding order the normals face outward when InsideNormals = false 
        /// and face inside otherwise.
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
            }
            Segments segments = new();
            for (int i = 0; i < Count; i++)
            {
                Vector2 start = this[i];
                Vector2 end = this[(i + 1) % Count];
                segments.Add(new(start, end, FlippedNormals));
            }
            return segments;
        }
        public Circle GetBoundingCircle()
        {
            float maxD = 0f;
            int num = this.Count;
            Vector2 origin = new();
            for (int i = 0; i < num; i++) { origin += this[i]; }
            origin = origin / num;
            //origin *= (1f / (float)num);
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
        public Triangle GetBoundingTriangle(float margin = 3f) { return Polygon.GetBoundingTriangle(this, margin); }
        public float GetCircumference() { return MathF.Sqrt(GetCircumferenceSquared()); }
        public float GetCircumferenceSquared()
        {
            if (this.Count < 3) return 0f;
            float lengthSq = 0f;
            for (int i = 0; i < Count; i++)
            {
                Vector2 w = this[(i + 1)%Count] - this[i];
                lengthSq += w.LengthSquared();
            }
            return lengthSq;
        }
        public float GetArea() { return MathF.Abs(GetAreaSigned()); }
        public bool IsClockwise() { return GetAreaSigned() > 0f; }
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
        public Polygon ToPolygon() { return new( this ); }
        public Polyline ToPolyline() { return new(this); }


        public int GetClosestIndex(Vector2 p)
        {
            //if (Count <= 0) return -1;
            //if (Count == 1) return 0;
            //
            //float minD = float.PositiveInfinity;
            //var edges = GetEdges();
            //int closestIndex = -1;
            //for (int i = 0; i < edges.Count; i++)
            //{
            //    Vector2 c = edges[i].GetClosestPoint(p).Point;
            //    float d = (c - p).LengthSquared();
            //    if (d < minD)
            //    {
            //        closestIndex = i;
            //        minD = d;
            //    }
            //}
            //return closestIndex;

            if (Count <= 0) return -1;
            if (Count == 1) return 0;

            float minD = float.PositiveInfinity;
            int closestIndex = -1;

            for (int i = 0; i < Count; i++)
            {
                Vector2 start = this[i];
                Vector2 end = this[(i + 1) % Count];
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
        public Vector2 GetClosestVertex(Vector2 p)
        {
            //float minD = float.PositiveInfinity;
            //Vector2 closest = new();
            //for (int i = 0; i < Count; i++)
            //{
            //    float d = (this[i] - p).LengthSquared();
            //    if (d < minD)
            //    {
            //        closest = this[i];
            //        minD = d;
            //    }
            //}
            //return closest;

            if (Count < 2) return new();
            float minD = float.PositiveInfinity;
            Vector2 closestPoint = new();

            for (int i = 0; i < Count; i++)
            {
                Vector2 start = this[i];
                Vector2 end = this[(i + 1) % Count];
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

            for (int i = 0; i < Count; i++)
            {
                Vector2 start = this[i];
                Vector2 end = this[(i + 1) % Count];
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

        public bool IsPointInside(Vector2 p) { return SGeometry.IsPointInPoly(p, this); }
        public Vector2 GetRandomPoint()
        {
            var triangles = Triangulate();
            List<WeightedItem<Triangle>> items = new();
            foreach (var t in triangles)
            {
                items.Add(new(t, (int)t.GetArea()));
            }
            var item = SRNG.PickRandomItem(items.ToArray());
            return item.GetRandomPoint();
        }
        public Points GetRandomPoints(int amount)
        {
            var triangles = Triangulate();
            WeightedItem<Triangle>[] items = new WeightedItem<Triangle>[triangles.Count];
            for (int i = 0; i < items.Length; i++)
            {
                var t = triangles[i];
                items[i] = new(t, (int)t.GetArea());
            }


            List<Triangle> pickedTriangles = SRNG.PickRandomItems(amount, items);
            Points randomPoints = new();
            foreach (var tri in pickedTriangles) randomPoints.Add(tri.GetRandomPoint());

            return randomPoints;
        }
        public Vector2 GetRandomVertex() { return SRNG.randCollection(this, false); }
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

        public void DrawShape(float linethickness, Raylib_CsLo.Color color) => SDrawing.DrawLines(this, linethickness, color);
        #endregion

        #region Private
        private float GetAreaSigned()
        {
            float totalArea = 0f;

            for (int i = 0; i < this.Count; i++)
            {
                Vector2 a = this[i];
                Vector2 b = this[(i + 1) % this.Count];

                float dy = (a.Y + b.Y) / 2f;
                float dx = b.X - a.X;

                float area = dy * dx;
                totalArea += area;
            }

            return totalArea;
        }
        #endregion

        //public Vector2 GetReferencePoint() { return GetCentroid(); }
        //public SegmentShape GetSegmentShape() { return new(GetEdges(), this.GetCentroid()); }
    }

    public class Polyline : List<Vector2>, IShape, IEquatable<Polyline>
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
        /// <param name="points"></param>
        public Polyline(IEnumerable<Vector2> edges) { AddRange(edges); }
        /// <summary>
        /// Points should be in CCW order. Use Reverse if they are in CW order.
        /// </summary>
        /// <param name="points"></param>
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
        public bool FlippedNormals { get; set; } = false;
        #endregion

        #region Public
        public Polyline Copy() { return new(this); }
        public void Floor() { Points.Floor(this); }
        public void Ceiling() { Points.Ceiling(this); }
        public void Truncate() { Points.Truncate(this); }
        public void Round() { Points.Round(this); }
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
            origin *= (1f / (float)num);
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
        public Polyline ToPolyline() { return this; }
        public Polygon ToPolygon()
        {
            var polygon = new Polygon();
            polygon.AddRange(this);
            return polygon;
        }
        
        public bool IsPointInside(Vector2 p)
        {
            var segments = GetEdges();
            foreach (var segment in segments)
            {
                if (segment.IsPointInside(p)) return true;
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
        public Vector2 GetRandomVertex() { return SRNG.randCollection(this, false); }
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

