
using System.Numerics;
using Raylib_CsLo;
using ShapeEngine.Lib;
using ShapeEngine.Random;

namespace ShapeEngine.Core
{
    public class Segments : List<Segment>
    {
        public Segments() { }
        public Segments(IShape shape) { AddRange(shape.GetEdges()); }
        public Segments(params Segment[] edges) { AddRange(edges); }
        public Segments(IEnumerable<Segment> edges) {  AddRange(edges); }


        public Segments GetUniqueSegments()
        {
            Segments uniqueEdges = new();
            for (int i = Count - 1; i >= 0; i--)
            {
                var edge = this[i];
                if (IsUnique(edge))
                {
                    uniqueEdges.Add(edge);
                }
            }
            return uniqueEdges;
        }

        /// <summary>
        /// Counts how often the specified segment appears in the list.
        /// </summary>
        /// <param name="seg"></param>
        /// <returns></returns>
        public int GetCount(Segment seg) { return this.Count((s) => s.IsSame(seg)); }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="seg">The segment to check.</param>
        /// <returns>Returns true if seg exactly exists once in the list.</returns>
        public bool IsUnique(Segment seg)
        {
            int counter = 0;
            foreach (var segment in this)
            {
                if(segment.IsSame(seg)) counter++;
                if (counter > 1) return false;
            }
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="seg"></param>
        /// <returns>Returns true if seg is already in the list.</returns>
        public bool ContainsSegment(Segment seg)
        {
            foreach (var segment in this) { if (segment.IsSame(seg)) return true; }
            return false;
        }
        
        
        
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
   
    public class Triangulation : List<Triangle>
    {
        public Triangulation() { }
        public Triangulation(IShape shape) { AddRange(shape.Triangulate()); }
        public Triangulation(params Triangle[] triangles) { AddRange(triangles); }
        public Triangulation(IEnumerable<Triangle> triangles) { AddRange(triangles); }

        /// <summary>
        /// Get the total area of all triangles in this triangulation.
        /// </summary>
        /// <returns></returns>
        public float GetArea()
        {
            float total = 0f;
            foreach (var t in this)
            {
                total += t.GetArea();
            }
            return total;
        }

        /// <summary>
        /// Remove all triangles with an area less than the threshold. If threshold is <= 0, nothing happens.
        /// </summary>
        /// <param name="areaThreshold"></param>
        /// <returns></returns>
        public int Remove(float areaThreshold)
        {
            if (areaThreshold <= 0f) return 0;

            int count = 0;
            for (int i = Count - 1; i >= 0; i--)
            {
                if (this[i].GetArea() < areaThreshold)
                {
                    RemoveAt(i);
                    count++;
                }
            }

            return count;
        }
        
        //public int RemoveNarrow(float margin)
        //{
        //    int count = 0;
        //    for (int i = Count - 1; i >= 0; i--)
        //    {
        //        float a = this[i].GetWidestAngle();
        //        if (a < margin)
        //        {
        //            RemoveAt(i);
        //            count++;
        //        }
        //    }
        //
        //    return count;
        //}

        /// <summary>
        /// Get a new triangulation with triangles with an area >= areaThreshold.
        /// </summary>
        /// <param name="areaThreshold"></param>
        /// <returns></returns>
        public Triangulation Get(float areaThreshold)
        {
            Triangulation newTriangulation = new();
            if (areaThreshold <= 0f) return newTriangulation;

            for (int i = Count - 1; i >= 0; i--)
            {
                var t = this[i];
                if (t.GetArea() >= areaThreshold)
                {
                    newTriangulation.Add(t);
                }
            }

            return newTriangulation;
        }
    }
    
    public struct Segment : IShape
    {
        public Vector2 start;
        public Vector2 end;
        public Vector2 n;
        public bool AutomaticNormals { get; private set; } = true;

        public bool FlippedNormals { get { return false; } set { } }
        public Vector2 Center { get { return (start + end) * 0.5f; } }
        public Vector2 Dir { get { return Displacement.Normalize(); } }
        public Vector2 Displacement { get { return end - start; } }
        public float Length { get { return Displacement.Length(); } }
        public float LengthSquared { get { return Displacement.LengthSquared(); } }


        public Segment(Vector2 start, Vector2 end) 
        { 
            this.start = start; 
            this.end = end; 
            this.n = (end - start).GetPerpendicularRight().Normalize(); 
        }
        //public Segment(Vector2 start, Vector2 end, bool automaticNormal = false)
        //{
        //    this.start = start;
        //    this.end = end;
        //    this.n = (end - start).GetPerpendicularRight().Normalize();
        //    this.automaticNormal = false;
        //}
        public Segment(Vector2 start, Vector2 end, Vector2 n) 
        { 
            this.start = start; 
            this.end = end; 
            this.n = n;
            this.AutomaticNormals = false;
        }
        public Segment(float startX, float startY, float endX, float endY) 
        { 
            this.start = new(startX, startY); 
            this.end = new(endX, endY); 
            this.n = (this.end - this.start).GetPerpendicularRight().Normalize(); 
        }
        public Segment(Segment s) { start = s.start; end = s.end; n = s.n; AutomaticNormals = s.AutomaticNormals; }

        
        //public static bool operator ==(Segment s1, Segment s2) { return s2.start == s1.start && s2.end == s1.end; }
        //public static bool operator !=(Segment s1, Segment s2) { return s2.start != s1.start || s2.end != s1.end; }
        //public override bool Equals([NotNullWhen(true)] object? obj)
        //{
        //    return base.Equals(obj);
        //}
        //public override int GetHashCode()
        //{
        //    return base.GetHashCode();
        //}
        public bool IsSame(Segment other) 
        {
            return Center == other.Center; 
        }
        public Vector2 GetCentroid() { return Center; }
        public float GetArea() { return 0f; }
        public float GetCircumference() { return Length; }
        public float GetCircumferenceSquared() { return LengthSquared; }
        public Polygon ToPolygon() { return new(start, end); }
        public Polyline ToPolyline() { return new(start, end); }
        public Segments GetEdges() { return new(this); }
        //public Segments GetEdges(Vector2 normalReferencePoint)
        //{
        //    var s = new Segment(this);
        //    s.FixNormal(normalReferencePoint);
        //    return new(s);
        //}
        public Triangulation Triangulate() { return new(); }
        public Circle GetBoundingCircle() { return ToPolygon().GetBoundingCircle(); }
        public Rect GetBoundingBox() { return new(start, end); }
        public bool IsPointInside(Vector2 p) { return SGeometry.IsPointOnSegment(p, start, end); }
        public Vector2 GetClosestPoint(Vector2 p)
        {
            var w = Displacement;
            float t = (p - start).Dot(w) / w.LengthSquared();
            if (t < 0f) return start;
            else if (t > 1f) return end;
            else return start + w * t;
        }
        public Vector2 GetClosestVertex(Vector2 p)
        {
            float disSqA = (p - start).LengthSquared();
            float disSqB = (p - end).LengthSquared();
            return disSqA <= disSqB ? start : end;
        }
        public Vector2 GetRandomPoint() { return this.GetPoint(SRNG.randF()); }
        public List<Vector2> GetRandomPoints(int amount)
        {
            var points = new List<Vector2>();
            for (int i = 0; i < amount; i++)
            {
                points.Add(GetRandomPoint());
            }
            return points;
        }
        public Vector2 GetRandomVertex() { return SRNG.chance(0.5f) ? start : end; }
        public Segment GetRandomEdge() { return this; }
        public Vector2 GetRandomPointOnEdge() { return GetRandomPoint(); }
        public List<Vector2> GetRandomPointsOnEdge(int amount)
        {
            var points = new List<Vector2>();
            for (int i = 0; i < amount; i++)
            {
                points.Add(GetRandomPointOnEdge());
            }
            return points;
        }
        public void DrawShape(float linethickness, Raylib_CsLo.Color color) => this.Draw(linethickness, color);



        //public SegmentShape GetSegmentShape() { return new(start, this); }
        //public Vector2 GetReferencePoint() { return Center; }
        //public Segment ChangePosition(Vector2 newPos) { return new(newPos, newPos + Displacement); }
        //public void SetPosition(Vector2 newPosition)
        //{
        //    Vector2 w = Displacement;
        //    start = newPosition;
        //    end = newPosition + w;
        //}
    }
    
    public struct Circle : IShape
    {
        public Vector2 center;
        public float radius;

        public float Diameter { get { return radius * 2f; } }
        public bool FlippedNormals { get; set; } = false;
        public Circle(Vector2 center, float radius) { this.center = center; this.radius = radius; }
        public Circle(float x, float y, float radius) { this.center = new(x, y); this.radius = radius; }
        public Circle(Circle c) { center = c.center; radius = c.radius; }
        public Circle(Rect r) { center = r.Center; radius = MathF.Max(r.width, r.height); }

        public Vector2 GetCentroid() { return center; }
        public Segments GetEdges() { return this.GetEdges(16, FlippedNormals); }
        public Polygon ToPolygon() { return this.GetPoints(16); }
        public Polyline ToPolyline() { return this.GetPolylinePoints(16); }
        public Triangulation Triangulate() { return ToPolygon().Triangulate(); }
        public Circle GetBoundingCircle() { return this; }
        public float GetArea() { return MathF.PI * radius * radius; }
        public float GetCircumference() { return MathF.PI * radius * 2f; }
        public float GetCircumferenceSquared() { return GetCircumference() * GetCircumference(); }
        public Rect GetBoundingBox() { return new Rect(center, new(radius, radius), new(0.5f)); }
        public bool IsPointInside(Vector2 p) { return SGeometry.IsPointInCircle(p, center, radius); }
        public Vector2 GetClosestPoint(Vector2 p) { return (p - center).Normalize() * radius; }
        public Vector2 GetClosestVertex(Vector2 p) { return (p - center).Normalize() * radius; }
        public Vector2 GetRandomPoint()
        {
            float randAngle = SRNG.randAngleRad();
            var randDir = SVec.VecFromAngleRad(randAngle);
            return center + randDir * SRNG.randF(0, radius);
        }
        public List<Vector2> GetRandomPoints(int amount)
        {
            var points = new List<Vector2>();
            for (int i = 0; i < amount; i++)
            {
                points.Add(GetRandomPoint());
            }
            return points;
        }
        public Vector2 GetRandomVertex() { return GetRandomPoint(); }
        public Segment GetRandomEdge() { return SRNG.randCollection(GetEdges(), false); }
        public Vector2 GetRandomPointOnEdge() { return GetRandomEdge().GetRandomPoint(); }
        public List<Vector2> GetRandomPointsOnEdge(int amount)
        {
            var points = new List<Vector2>();
            for (int i = 0; i < amount; i++)
            {
                points.Add(GetRandomPointOnEdge());
            }
            return points;
        }
        public void DrawShape(float linethickness, Raylib_CsLo.Color color) => this.DrawLines(linethickness, color);


        //public Vector2 GetReferencePoint() { return center; }
        //public SegmentShape GetSegmentShape() { return new(this.GetEdges(), center); }
    }
   
    /// <summary>
    /// Class that represents a triangle by holding three points. Points a, b, c should be in ccw order!
    /// </summary>
    public struct Triangle : IShape
    {
        public Vector2 a, b, c;

        //public Vector2 Centroid { get { return (a + b + c) / 3; } }
        public Vector2 A { get { return b - a; } }
        public Vector2 B { get { return c - b; } }
        public Vector2 C { get { return a - c; } }
        public bool FlippedNormals { get; set; } = false;

        /// <summary>
        /// Points should be in ccw order!
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        public Triangle(Vector2 a, Vector2 b, Vector2 c) { this.a = a; this.b = b; this.c = c; }
        public Triangle(Triangle t) { a = t.a; b = t.b; c = t.c; }
        public Triangle(Vector2 p, Segment s)
        {
            Vector2 w = s.Displacement;
            Vector2 v = p - s.start;
            float cross = w.Cross(v);
            if(cross >= 0f)
            {
                a = s.start;
                b = s.end;
                c = p;
            }
            else
            {
                a = s.end;
                b = s.start;
                c = p;
            }
        }

        public Circle GetCircumCircle()
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


        public bool SharesVertex(Vector2 p) { return a == p || b == p || c == p; }
        public bool SharesVertex(IEnumerable<Vector2> points)
        {
            foreach (var p in points)
            {
                if (SharesVertex(p)) return true;
            }
            return false;
        }
        public bool SharesVertex(Triangle t) { return SharesVertex(t.a) || SharesVertex(t.b) || SharesVertex(t.c); }
        
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

        public bool IsValid() { return GetArea() > 0f; }
        public Vector2 GetCentroid() { return (a + b + c) / 3; }
        public Polygon ToPolygon() { return new(a, b, c); }
        public Polyline ToPolyline() { return new(a, b, c); }
        public Segments GetEdges() 
        {
            if (FlippedNormals)
            {
                Segment A = new Segment(a, b, (b - a).GetPerpendicularLeft().Normalize());
                Segment B = new Segment(b, c, (c - b).GetPerpendicularLeft().Normalize());
                Segment C = new Segment(c, a, (a - c).GetPerpendicularLeft().Normalize());
                return new() { A, B, C };
            }
            else
            {
                Segment A = new Segment(a, b, (b - a).GetPerpendicularRight().Normalize());
                Segment B = new Segment(b, c, (c - b).GetPerpendicularRight().Normalize());
                Segment C = new Segment(c, a, (a - c).GetPerpendicularRight().Normalize());
                return new() { A, B, C };
            }
            
        }
        public Triangulation Triangulate() { return this.Triangulate(GetCentroid()); }
        public Circle GetBoundingCircle() { return ToPolygon().GetBoundingCircle(); }
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
        public Vector2 GetClosestPoint(Vector2 p) { return ToPolygon().GetClosestPoint(p); }
        public Vector2 GetClosestVertex(Vector2 p) { return ToPolygon().GetClosestVertex(p); }
        public Vector2 GetRandomPoint() { return this.GetPoint(SRNG.randF(), SRNG.randF()); }
        public List<Vector2> GetRandomPoints(int amount)
        {
            var points = new List<Vector2>();
            for (int i = 0; i < amount; i++)
            {
                points.Add(GetRandomPoint());
            }
            return points;
        }
        public Vector2 GetRandomVertex() { return GetRandomPoint(); }
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
        public List<Vector2> GetRandomPointsOnEdge(int amount)
        {
            List<WeightedItem<Segment>> items = new(amount);
            var edges = GetEdges();
            foreach (var edge in edges)
            {
                items.Add(new(edge, (int)edge.LengthSquared));
            }
            var pickedEdges = SRNG.PickRandomItems(amount, items.ToArray());
            var randomPoints = new List<Vector2>();
            foreach (var edge in pickedEdges)
            {
                randomPoints.Add(edge.GetRandomPoint());
            }
            return randomPoints;
        }
        
        public void DrawShape(float linethickness, Raylib_CsLo.Color color) => this.DrawLines(linethickness, color);

        //public SegmentShape GetSegmentShape() { return new(GetCentroid(), new(a, b), new(b, c), new(c, a) ); }
        //public Vector2 GetReferencePoint() { return GetCentroid(); }
    }
    
    public struct Rect : IShape
    {
        public float x;
        public float y;
        public float width;
        public float height;
        public bool FlippedNormals { get; set; } = false;
        public Vector2 TopLeft { get { return new Vector2(x, y); } }
        public Vector2 TopRight { get { return new Vector2(x + width, y); } }
        public Vector2 BottomRight { get { return new Vector2(x + width, y + height); } }
        public Vector2 BottomLeft { get { return new Vector2(x, y + height); } }
        public Vector2 Center { get { return new Vector2(x + width * 0.5f, y + height * 0.5f); } }

        public float Top { get { return y; } }
        public float Bottom { get { return y + height; } }
        public float Left { get { return x; } }
        public float Right { get { return x + width; } }
        public Vector2 Size { get { return new Vector2(width, height); } }
        public Rectangle Rectangle { get { return new(x, y, width, height); } }

        public Rect(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }
        public Rect(Rect r) { x = r.x; y = r.y; width = r.width; height = r.height; }
        public Rect(Vector2 topLeft, Vector2 bottomRight)
        {
            var final = SRect.Fix(topLeft, bottomRight);
            this.x = final.topLeft.X;
            this.y = final.topLeft.Y;
            this.width = final.bottomRight.X - this.x;
            this.height = final.bottomRight.Y - this.y;
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
            this.x = topLeft.X;
            this.y = topLeft.Y;
            this.width = size.X;
            this.height = size.Y;
        }
        public Rect(IShape shape) { this = shape.GetBoundingBox(); }// ???
        public Rect(Polygon p) { this = p.GetBoundingBox(); }// ???
        public Rect(Rectangle rect)
        {
            this.x = rect.X;
            this.y = rect.Y;
            this.width = rect.width;
            this.height = rect.height;
        }

        public Vector2 GetCentroid() { return Center; }
        public Polygon ToPolygon() { return new() { TopLeft, BottomLeft, BottomRight, TopRight }; }
        public Polyline ToPolyline() { return new() { TopLeft, BottomLeft, BottomRight, TopRight }; }
        public Segments GetEdges() 
        {
            Vector2 A = TopLeft;
            Vector2 B = BottomLeft;
            Vector2 C = BottomRight;
            Vector2 D = TopRight;

            if (FlippedNormals)
            {
                Segment left = new(A, B, (B - A).GetPerpendicularLeft().Normalize());
                Segment bottom = new(B, C, (C - B).GetPerpendicularLeft().Normalize());
                Segment right = new(C, D, (D - C).GetPerpendicularLeft().Normalize());
                Segment top = new(D, A, (A - D).GetPerpendicularLeft().Normalize());
                return new() { left, bottom, right, top };
            }
            else
            {
                Segment left = new(A, B, (B - A).GetPerpendicularRight().Normalize());
                Segment bottom = new(B, C, (C - B).GetPerpendicularRight().Normalize());
                Segment right = new(C, D, (D - C).GetPerpendicularRight().Normalize());
                Segment top = new(D, A, (A - D).GetPerpendicularRight().Normalize());
                return new() { left, bottom, right, top };
            }
        }
        public Triangulation Triangulate() { return ToPolygon().Triangulate(); }
        public Circle GetBoundingCircle() { return ToPolygon().GetBoundingCircle(); }
        public float GetCircumference() { return width * 2 + height * 2; }
        public float GetCircumferenceSquared() { return GetCircumference() * GetCircumference(); }
        public float GetArea() { return width * height; }
        public Rect GetBoundingBox() { return this; }
        public bool IsPointInside(Vector2 p) { return SGeometry.IsPointInRect(p, TopLeft, Size); }
        public Vector2 GetClosestPoint(Vector2 p) { return ToPolygon().GetClosestPoint(p); }
        public Vector2 GetClosestVertex(Vector2 p) { return ToPolygon().GetClosestVertex(p); }
        public Vector2 GetRandomPoint() { return new(SRNG.randF(x, x + width), SRNG.randF(y, y + height)); }
        public List<Vector2> GetRandomPoints(int amount)
        {
            var points = new List<Vector2>();
            for (int i = 0; i < amount; i++)
            {
                points.Add(GetRandomPoint());
            }
            return points;
        }
        public Vector2 GetRandomVertex() { return ToPolygon().GetRandomPoint(); }
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
        public List<Vector2> GetRandomPointsOnEdge(int amount)
        {
            List<WeightedItem<Segment>> items = new(amount);
            var edges = GetEdges();
            foreach (var edge in edges)
            {
                items.Add(new(edge, (int)edge.LengthSquared));
            }
            var pickedEdges = SRNG.PickRandomItems(amount, items.ToArray());
            var randomPoints = new List<Vector2>();
            foreach (var edge in pickedEdges)
            {
                randomPoints.Add(edge.GetRandomPoint());
            }
            return randomPoints;
        }

        public void DrawShape(float linethickness, Raylib_CsLo.Color color) => this.DrawLines(linethickness, color);

        //public Vector2 GetReferencePoint() { return Center; }
        //public SegmentShape GetSegmentShape() { return new(GetEdges(), Center); }
        //public void SetPosition(Vector2 newPosition) 
        //{
        //    Vector2 w = newPosition - Center;
        //    x += w.X;
        //    y += w.Y;
        //}
        /*
        public Vector2 GetPoint(Vector2 alignement)
        {
            Vector2 offset = Size * alignement;
            return TopLeft + offset;
        }
        */
        /*
        public static bool operator ==(Rect left, Rect right)
        {
            return (left.x == right.x)
                && (left.y == right.y)
                && (left.width == right.width)
                && (left.height == right.height);
        }
        public static bool operator !=(Rect left, Rect right)
        { 
            return !(left == right);
        }
        */
        /*
        public static Rect operator +(Rect left, Rect right)
        {
            return new Rect(
                new Vector2(left.x + right.x, left.y + right.y) / 2,
                new Vector2(left.width + right.width, left.height + right.height),
                new Vector2(0.5f)
            );
        }
        public static Rect operator -(Rect left, Rect right)
        {
            return new Rect(
                new Vector2(left.x - right.x, left.y - right.y) / 2,
                new Vector2(left.width - right.width, left.height - right.height),
                new Vector2(0.5f)
            );
        }
        */
        /*
        public static Rect operator +(Rect left, Vector2 right)
        {
            return new Rect(
                new Vector2(left.x + right.x, left.y + right.y) / 2,
                new Vector2(left.width + right.width, left.height + right.height),
                new Vector2(0.5f)
            );
        }
        public static Rect operator +(Rect left, float right)
        {
            return new Rect(
                left.x,
                left.y,
                left.width + right,
                left.height + right
            );
        }
        public static Rect operator -(Rect left, Vector2 right)
        {
            return new Rect(
                left.x - right.X,
                left.y - right.Y,
                left.width,
                left.height
            );
        }
        public static Rect operator -(Rect left, float right)
        {
            return new Rect(
                left.x,
                left.y,
                left.width - right,
                left.height - right
            );
        }
        public static Rect operator /(Rect left, Rect right)
        {
            return new Rect(
                left.x / right.x,
                left.y / right.y,
                left.width / right.width,
                left.height / right.height
            );
        }
        public static Rect operator /(Rect left, Vector2 right)
        {
            return new Rect(
                left.x / right.X,
                left.y / right.Y,
                left.width / right.X,
                left.height / right.Y
            );
        }
        public static Rect operator /(Rect value1, float value2)
        {
            return new Rect(
                value1.x / value2,
                value1.y / value2,
                value1.width,
                value1.height
            );
        }
        public static Rect operator *(Rect left, Rect right)
        {
            return new Rect(
                left.x * right.x,
                left.y * right.y,
                left.width * right.width,
                left.height * right.height
            );
        }
        public static Rect operator *(Rect left, Vector2 right)
        {
            return new Rect(
                left.x * right.X,
                left.y * right.Y,
                left.width * right.X,
                left.height * right.Y
            );
        }
        public static Rect operator *(Rect left, float right)
        {
            return new Rect(
                left.x,
                left.y,
                left.width * right,
                left.height * right
            );
        }
        public static Rect operator *(float left, Rect right)
        {
            return right * left;
        }
        //public static Rect operator -(Rect value)
        //{
        //    return new Rect(0,0,0,0) - value;
        //}
        //public bool IsPointInside(Vector2 p)
        //{
        //    Vector2 tl = TopLeft;
        //    Vector2 br = BottomRight;
        //    return p.X > tl.X && p.X < br.X && p.Y > tl.Y && p.Y < br.Y;
        //}
        */
    }
    
    /// <summary>
    /// Points shoud be in CCW order.
    /// </summary>
    public class Polygon : List<Vector2>, IShape
    {
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
        public Polygon(IEnumerable<Vector2> edges) { AddRange(edges); }
        /// <summary>
        /// Points should be in CCW order. Use Reverse if they are in CW order.
        /// </summary>
        /// <param name="points"></param>
        public Polygon(IShape shape) { AddRange(shape.ToPolygon()); }
        public Polygon(Polygon poly) { AddRange(poly); }
        public Polygon(Polyline polyLine) { AddRange(polyLine); }

        public bool FlippedNormals { get; set; } = false;

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

        

        public Vector2 GetCentroid()
        {
            Vector2 result = new();

            for (int i = 0; i < Count; i++)
            {
                Vector2 a = this[i];
                Vector2 b = this[(i + 1) % Count];
                float factor = a.X * b.Y - b.X * a.Y;
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
            while (vertices.Count > 3)
            {
                
                for (int count = 0; count < vertices.Count; count++)
                {
                    int i = SRNG.randI(0, vertices.Count);
                    Vector2 a = vertices[i];
                    Vector2 b = SUtils.GetItem(vertices, i + 1);
                    Vector2 c = SUtils.GetItem(vertices, i - 1);

                    Vector2 ba = b - a;
                    Vector2 ca = c - a;

                    if (ba.Cross(ca) > 0f) continue;

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
                        break;
                    }
                }
            }
            triangles.Add(new(vertices[0], vertices[1], vertices[2]));


            return triangles;
        }



        /// <summary>
        /// Triangulates and subdivides the triangulation until the area of triangles reaches the min area limit.
        /// </summary>
        /// <param name="areaThresholdFactor">Used to calculate the min area limit. The threshold factor is multiplied with the total area of the polygon to recieve the min area limit.</param>
        /// <returns></returns>
        public Triangulation Fracture(float areaThresholdFactor = 0f)
        {
            var triangulation = Triangulate();
            if(areaThresholdFactor <= 0f || areaThresholdFactor >= 1f) return triangulation;

            float totalArea = triangulation.GetArea();
            float minArea = totalArea * areaThresholdFactor;

            ////var1
            //Triangulation final = new();
            //foreach (var tri in triangulation)
            //{
            //    final.AddRange(Subdivide(tri, minArea));
            //}
            //return final;

            //var2
            return triangulation.Subdivide(minArea); // SPoly.Subdivide(triangulation, minArea);
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

                Vector2 n = (B - A);
                if (FlippedNormals) n = n.GetPerpendicularLeft().Normalize();
                else n = n.GetPerpendicularRight().Normalize();
                return new() { new(A, B, n) };
            }
            Segments segments = new();
            for (int i = 0; i < Count; i++)
            {
                Vector2 start = this[i];
                Vector2 end = this[(i + 1) % Count];
                Vector2 n = (end - start);
                if (FlippedNormals) n = n.GetPerpendicularLeft().Normalize();
                else n = n.GetPerpendicularRight().Normalize();
                segments.Add(new(start, end, n));
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
        public Triangle GetBoundingTriangle(float margin = 3f) { return SPoly.GetBoundingTriangle(this, margin); }
        public float GetCircumference() { return MathF.Sqrt(GetCircumferenceSquared()); }
        public float GetCircumferenceSquared()
        {
            if (this.Count < 3) return 0f;
            float lengthSq = 0f;
            for (int i = 0; i < Count - 1; i++)
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

        public Polygon ToPolygon() { return new( this ); }
        public Polyline ToPolyline() { return new(this); }

        public int GetClosestIndex(Vector2 p)
        {
            if (Count <= 0) return -1;
            if (Count == 1) return 0;

            float minD = float.PositiveInfinity;
            var edges = GetEdges();
            int closestIndex = -1;
            for (int i = 0; i < edges.Count; i++)
            {
                Vector2 c = edges[i].GetClosestPoint(p);
                float d = (c - p).LengthSquared();
                if (d < minD)
                {
                    closestIndex = i;
                    minD = d;
                }
            }
            return closestIndex;
        }

        public bool IsPointInside(Vector2 p) { return SGeometry.IsPointInPoly(p, this); }
        public Vector2 GetClosestPoint(Vector2 p)
        {
            float minD = float.PositiveInfinity;
            var edges = GetEdges();
            Vector2 closest = new();

            for (int i = 0; i < edges.Count; i++)
            {
                Vector2 c = edges[i].GetClosestPoint(p);
                float d = (c - p).LengthSquared();
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
        public List<Vector2> GetRandomPoints(int amount)
        {
            var triangles = Triangulate();
            WeightedItem<Triangle>[] items = new WeightedItem<Triangle>[triangles.Count];
            for (int i = 0; i < items.Length; i++)
            {
                var t = triangles[i];
                items[i] = new(t, (int)t.GetArea());
            }


            List<Triangle> pickedTriangles = SRNG.PickRandomItems(amount, items);
            List<Vector2> randomPoints = new();
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
        public List<Vector2> GetRandomPointsOnEdge(int amount)
        {
            List<WeightedItem<Segment>> items = new(amount);
            var edges = GetEdges();
            foreach (var edge in edges)
            {
                items.Add(new(edge, (int)edge.LengthSquared));
            }
            var pickedEdges = SRNG.PickRandomItems(amount, items.ToArray());
            var randomPoints = new List<Vector2>();
            foreach (var edge in pickedEdges)
            {
                randomPoints.Add(edge.GetRandomPoint());
            }
            return randomPoints;
        }

        public void DrawShape(float linethickness, Raylib_CsLo.Color color) => SDrawing.DrawLines(this, linethickness, color);
        
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
        
        
        //public Vector2 GetReferencePoint() { return GetCentroid(); }
        //public SegmentShape GetSegmentShape() { return new(GetEdges(), this.GetCentroid()); }
    }

    public class Polyline : List<Vector2>, IShape
    {
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

        
        public bool AutomaticNormals = true;
        /// <summary>
        /// Flips the calculated normals for each segment. 
        /// false means default is used. (facing right)
        /// </summary>
        public bool FlippedNormals { get; set; } = false;
        public Vector2 GetVertex(int index)
        {
            return this[SUtils.WrapIndex(Count, index)];
        }

        public Vector2 GetCentroid()
        {
            if(Count < 2) return new Vector2();
            float halfLengthSq = GetCircumferenceSquared() * 0.5f;
            var segments = GetEdges();
            float curLengthSq = 0f; 
            foreach (var seg in segments)
            {
                float segLengthSq = seg.LengthSquared;
                if (curLengthSq + segLengthSq >= halfLengthSq)
                {
                    float dif = halfLengthSq - curLengthSq;
                    return seg.start + seg.Dir * dif;
                }
                else curLengthSq += segLengthSq;
            }
            return new Vector2();
        }
        public Vector2 GetCentroidMean()
        {
            if (Count <= 0) return new(0f);
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
                if (AutomaticNormals)
                {
                    return new() { new(A, B) };
                }
                else
                {
                    Vector2 n = (B - A);
                    if (FlippedNormals) n = n.GetPerpendicularLeft().Normalize();
                    else n = n.GetPerpendicularRight().Normalize();
                    return new() { new(A, B, n) };
                }
                //Vector2 n = (B - A);
                //if (InsideNormals) n = n.GetPerpendicularLeft().Normalize();
                //else n = n.GetPerpendicularRight().Normalize();
                //return new() { new(A, B, n) };

            }

            Segments segments = new();
            for (int i = 0; i < Count - 1; i++)
            {
                Vector2 start = this[i];
                Vector2 end = this[(i + 1) % Count];

                if (AutomaticNormals)
                {
                    segments.Add(new(start, end));
                }
                else
                {
                    Vector2 n = (end - start);
                    if (FlippedNormals) n = n.GetPerpendicularLeft().Normalize();
                    else n = n.GetPerpendicularRight().Normalize();
                    segments.Add(new(start, end, n));
                }
                //Vector2 n = (end - start);
                //if (InsideNormals) n = n.GetPerpendicularLeft().Normalize();
                //else n = n.GetPerpendicularRight().Normalize();
                //segments.Add(new(start, end, n));

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
            if (this.Count < 3) return 0f;
            float lengthSq = 0f;
            for (int i = 0; i < Count - 1; i++)
            {
                Vector2 w = this[(i + 1) % Count] - this[i];
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


        public int GetClosestIndex(Vector2 p)
        {
            if (Count <= 0) return -1;
            if (Count == 1) return 0;

            float minD = float.PositiveInfinity;
            var edges = GetEdges();
            int closestIndex = -1;
            for (int i = 0; i < edges.Count; i++)
            {
                Vector2 c = edges[i].GetClosestPoint(p);
                float d = (c - p).LengthSquared();
                if (d < minD)
                {
                    closestIndex = i;
                    minD = d;
                }
            }
            return closestIndex;
        }


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
        public Vector2 GetClosestPoint(Vector2 p)
        {
            float minD = float.PositiveInfinity;
            var edges = GetEdges();
            Vector2 closest = new();

            for (int i = 0; i < edges.Count; i++)
            {
                Vector2 c = edges[i].GetClosestPoint(p);
                float d = (c - p).LengthSquared();
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
        public Vector2 GetRandomPoint() { return GetRandomPointOnEdge(); }
        public List<Vector2> GetRandomPoints(int amount)
        {
            var points = new List<Vector2>();
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
        public List<Vector2> GetRandomPointsOnEdge(int amount)
        {
            List<WeightedItem<Segment>> items = new(amount);
            var edges = GetEdges();
            foreach (var edge in edges)
            {
                items.Add(new(edge, (int)edge.LengthSquared));
            }
            var pickedEdges = SRNG.PickRandomItems(amount, items.ToArray());
            var randomPoints = new List<Vector2>();
            foreach (var edge in pickedEdges)
            {
                randomPoints.Add(edge.GetRandomPoint());
            }
            return randomPoints;
        }

        public void DrawShape(float linethickness, Raylib_CsLo.Color color) => this.Draw(linethickness, color);


    }
}





/*
    public struct SegmentShape
    {
        public Segments segments = new();
        public Vector2 referencePoint;
        public SegmentShape(IEnumerable<Segment> segments, Vector2 referencePoint) { this.segments.AddRange(segments); this.referencePoint = referencePoint; }
        public SegmentShape(Vector2 referencePoint, params Segment[] segments) { this.segments.AddRange(segments); this.referencePoint = referencePoint; }
    }
    */

//public struct PolygonShape
    //{
    //    public List<Vector2> points;
    //    public Vector2 referencePoint;
    //
    //    public PolygonShape(List<Vector2> points) { this.points = points; this.referencePoint = SPoly.GetCentroid(points); }
    //    public PolygonShape(params Vector2[] points) { this.points = points.ToList(); this.referencePoint = SPoly.GetCentroid(points.ToList()); }
    //    public PolygonShape(List<Vector2> points, Vector2 center) { this.points = points; this.referencePoint = center; }
    //    public PolygonShape(Vector2 center, params Vector2[] points) { this.points = points.ToList(); this.referencePoint = center; }
    //    public PolygonShape(Triangle t) { this.points = t.GetPolygon().points; this.referencePoint = t.Centroid; }
    //    public PolygonShape(Rect r) { this.points = r.GetPolygon().points; this.referencePoint = r.Center; }
    //}
/// <summary>
    /// Points should be in ccw order!
    /// </summary>
/*
    public struct Polygon : IShape
    {
        public PolygonPath points;
        public Vector2 center;

        public Polygon(List<Vector2> points) { this.points = new(points); this.center = SPoly.GetCentroid(this.points); }
        public Polygon(params Vector2[] points) { this.points = new(points); this.center = SPoly.GetCentroid(this.points); }
        public Polygon(List<Vector2> points, Vector2 center) { this.points = new(points); this.center = center; }
        public Polygon(Vector2 center, params Vector2[] points) { this.points = new(points); this.center = center; }
        public Polygon(Triangle t) { this.points = t.GetPolygon().points; this.center = t.Centroid; }
        public Polygon(Rect r) { this.points = r.GetPolygon().points; this.center = r.Center; }

        public Vector2 GetReferencePoint() { return center; }
        public float GetCircumference() { return points.GetCircumference(); }
        public float GetCircumferenceSquared() { return points.GetCircumferenceSquared(); }
        public float GetArea() { return points.GetArea(); }
        public SegmentShape GetSegmentShape() { return new(points.GetEdges(), center); }
        public Polygon GetPolygon() { return this; }
        public Rect GetBoundingBox() { return points.GetBoundingBox(); }
        public void DrawShape(float linethickness, Color color) => SDrawing.DrawPolygonLines(points, linethickness, color);
        public bool IsPointOnShape(Vector2 p) { return SGeometry.IsPointInPoly(p, points); }// this.IsPointInside(p); }
        
        //public void SetPosition(Vector2 newPosition) { center = newPosition; }
    }
    */
/*
    public class Poly
    {
        List<Vector2> displacements = new();
        private List<Vector2> shape = new();

        
        private Vector2 center;
        private Vector2 scale;
        private float rotRad;
        bool dirty;

        public Polygon GetShape()
        {
            if (dirty) UpdateShape();
            return new Polygon(shape, center);
        }
        
        public Vector2 GetPos() { return center; }
        public Vector2 GetScale() { return scale; }
        public float GetRotRad() { return rotRad; }

        public void SetPos(Vector2 pos)
        {
            if (!dirty) dirty = true;
            center = pos;
        }
        public void SetScale(Vector2 scale)
        {
            if (!dirty) dirty = true;
            this.scale = scale;
        }
        public void SetRotation(float radians)
        {
            if (!dirty) dirty = true;
            this.rotRad = radians;
        }
        private void UpdateShape()
        {
            dirty = false;
            shape = SPoly.GetShape(displacements, center, rotRad, scale);
        }
    }
    */
/*

    public struct Polygon : IShape
    {
        public List<Vector2> points;

        
        public float GetCircumference() { return SPoly.GetCircumference(points); }
        public float GetCircumferenceSquared() { return SPoly.GetCircumferenceSquared(points); }
        public float GetArea() { return width * height; }
        public List<Line> GetSegments() { return SRect.GetRectSegments(this); }
        public Rect GetBoundingBox() { return this; }
        //public Circle GetBoundingCircle() { return new(Center, Length / 2); }
        public void DrawShape(float linethickness, Color color)
        {
            throw new NotImplementedException();
        }
        //public Vector2 pos;
        //public float rotRad;
        //public Vector2 scale;

        public List<Vector2> Shape
        {
            get
            {
                if (points.Count < 3) return new();
                List<Vector2> shape = new();
                for (int i = 0; i < points.Count; i++)
                {
                    shape.Add(pos + SVec.Rotate(points[i], rotRad) * scale);
                }
                return shape;
            }
        }
        public float Circumference 
        {
            get
            {
                if (points.Count < 3) return 0f;
                float lengthSq = 0f;
                var shape = Shape;
                shape.Add(shape[0]);
                for (int i = 0; i < shape.Count - 1; i++)
                {
                    Vector2 w = shape[i + 1] - shape[i];
                    lengthSq += w.LengthSquared();
                }
                ///Vector2 final = shape[0] - shape[shape.Count - 1];
                ///lengthSq += final.LengthSquared();
                return MathF.Sqrt(lengthSq);
            } 
        }
        public float Area 
        { 
            get
            {
                if(points.Count < 3) return 0f;
                var triangles = Triangulate();
                float totalArea = 0f;
                foreach (var t in triangles)
                {
                    totalArea += t.Area;
                }
                return totalArea;
            } 
        }
        public List<Triangle> Triangulate()
        {
            if (points.Count < 3) return new();
            List<Triangle> triangles = new();
            var shape = Shape;
            shape.Add(shape[0]);
            for (int i = 0; i < shape.Count - 1; i++)
            {
                Vector2 a = shape[i];
                Vector2 b = pos;
                Vector2 c = shape[i + 1];
                triangles.Add(new(a,b,c));
            }
            return triangles;
        }
        public Polygon(Vector2 pos, float rotRad, Vector2 scale, params Vector2[] points)
        {
            this.pos = pos;
            this.rotRad = rotRad;
            this.scale = scale;
            this.points = points.ToList();
        }
        public Polygon(Vector2 pos, float rotRad, Vector2 scale, List<Vector2> points)
        {
            this.pos = pos;
            this.rotRad = rotRad;
            this.scale = scale;
            this.points = points;
        }

    }
        */