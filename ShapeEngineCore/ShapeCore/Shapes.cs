using System.Numerics;
using Raylib_CsLo;
using ShapeLib;
using ShapeRandom;

namespace ShapeCore
{

    public class Segments : List<Segment>
    {
        public Segments() { }
        public Segments(IShape shape) { AddRange(shape.GetEdges()); }
        public Segments(params Segment[] edges) { AddRange(edges); }
        public Segments(IEnumerable<Segment> edges) {  AddRange(edges); }
    }
    public class Triangulation : List<Triangle>
    {
        public Triangulation() { }
        public Triangulation(IShape shape) { AddRange(shape.Triangulate()); }
        public Triangulation(params Triangle[] triangles) { AddRange(triangles); }
        public Triangulation(IEnumerable<Triangle> triangles) { AddRange(triangles); }
    }
    public struct SegmentShape
    {
        public List<Segment> segments;
        public Vector2 referencePoint;
        public SegmentShape(List<Segment> segments, Vector2 referencePoint) { this.segments = segments; this.referencePoint = referencePoint; }
        public SegmentShape(Vector2 referencePoint, params Segment[] segments) { this.segments = segments.ToList(); this.referencePoint = referencePoint; }
    }
    
    public struct Segment : IShape
    {
        public Vector2 start;
        public Vector2 end;
        public Vector2 Center { get { return (start + end) * 0.5f; } }
        public Vector2 Dir { get { return Displacement.Normalize(); } }
        public Vector2 Displacement { get { return end - start; } }
        public float Length { get { return Displacement.Length(); } }
        public float LengthSquared { get { return Displacement.LengthSquared(); } }

        public Segment(Vector2 start, Vector2 end) { this.start = start; this.end = end; }
        public Segment(float startX, float startY, float endX, float endY) { this.start = new(startX, startY); this.end = new(endX, endY); }
        public Segment(Segment s) { start = s.start; end = s.end; }
        
        public Vector2 GetCentroid() { return Center; }
        public Vector2 GetReferencePoint() { return Center; }
        public float GetArea() { return 0f; }
        public float GetCircumference() { return Length; }
        public float GetCircumferenceSquared() { return LengthSquared; }
        public Polygon ToPolygon() { return new(start, end); }
        public Segments GetEdges() { return new(this); }
        public Triangulation Triangulate() { return new(); }
        public SegmentShape GetSegmentShape() { return new(start, this); }
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
        public Vector2 GetRandomVertex() { return SRNG.chance(0.5f) ? start : end; }
        public Segment GetRandomEdge() { return this; }
        public Vector2 GetRandomPointOnEdge() { return GetRandomPoint(); }
        public void DrawShape(float linethickness, Color color) => this.Draw(linethickness, color);





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

        public Circle(Vector2 center, float radius) { this.center = center; this.radius = radius; }
        public Circle(float x, float y, float radius) { this.center = new(x, y); this.radius = radius; }
        public Circle(Circle c) { center = c.center; radius = c.radius; }
        public Circle(Rect r) { center = r.Center; radius = MathF.Max(r.width, r.height); }

        public Vector2 GetCentroid() { return center; }
        public Segments GetEdges() { return this.GetEdges(16); }
        public Polygon ToPolygon() { return this.GetPoints(16); }
        public Triangulation Triangulate() { return ToPolygon().Triangulate(); }
        public SegmentShape GetSegmentShape() { return new(this.GetEdges(), center); }
        public Circle GetBoundingCircle() { return this; }
        public Vector2 GetReferencePoint() { return center; }
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
        public Vector2 GetRandomVertex() { return GetRandomPoint(); }
        public Segment GetRandomEdge() { return SRNG.randCollection(GetEdges(), false); }
        public Vector2 GetRandomPointOnEdge() { return GetRandomEdge().GetRandomPoint(); }

        public void DrawShape(float linethickness, Color color) => this.DrawLines(linethickness, color);


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

        /// <summary>
        /// Points should be in ccw order!
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        public Triangle(Vector2 a, Vector2 b, Vector2 c) { this.a = a; this.b = b; this.c = c; }
        public Triangle(Triangle t) { a = t.a; b = t.b; c = t.c; }

        public Vector2 GetCentroid() { return (a + b + c) / 3; }
        public Polygon ToPolygon() { return new(a, b, c); }
        public Segments GetEdges() { return new() { new(a, b), new(b, c), new(c, a) }; }
        public Triangulation Triangulate() { return this.Triangulate(GetCentroid()); }
        public SegmentShape GetSegmentShape() { return new(GetCentroid(), new(a, b), new(b, c), new(c, a) ); }
        public Circle GetBoundingCircle() { return ToPolygon().GetBoundingCircle(); }
        public Vector2 GetReferencePoint() { return GetCentroid(); }
        public float GetCircumference() { return MathF.Sqrt(GetCircumferenceSquared()); }
        public float GetCircumferenceSquared() { return A.LengthSquared() + B.LengthSquared() + C.LengthSquared(); }
        public float GetArea() { return (a.X - c.X) * (b.Y - c.Y) - (a.Y - c.Y) * (b.X - c.X); }
        public Rect GetBoundingBox() { return new Rect(a.X, a.Y, 0, 0).Enlarge(b).Enlarge(c); }
        public bool IsPointInside(Vector2 p) { return SGeometry.IsPointInTriangle(a, b, c, p); }
        public Vector2 GetClosestPoint(Vector2 p) { return ToPolygon().GetClosestPoint(p); }
        public Vector2 GetClosestVertex(Vector2 p) { return ToPolygon().GetClosestVertex(p); }
        public Vector2 GetRandomPoint() { return this.GetPoint(SRNG.randF(), SRNG.randF()); }
        public Vector2 GetRandomVertex() { return GetRandomPoint(); }
        public Segment GetRandomEdge() { return SRNG.randCollection(GetEdges(), false); }
        public Vector2 GetRandomPointOnEdge() { return GetRandomEdge().GetRandomPoint(); }
        public void DrawShape(float linethickness, Color color) => this.DrawLines(linethickness, color);

    }
    public struct Rect : IShape
    {
        public float x;
        public float y;
        public float width;
        public float height;

        public Vector2 TopLeft { get { return new Vector2(x, y); } }
        public Vector2 TopRight { get { return new Vector2(x + width, 0); } }
        public Vector2 BottomRight { get { return new Vector2(x + width, y + height); } }
        public Vector2 BottomLeft { get { return new Vector2(x, y + height); } }
        public Vector2 Center { get { return new Vector2(x + width * 0.5f, y + height * 0.5f); } }

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
            if (topLeft.X > bottomRight.X)
            {
                this.x = bottomRight.X;
                this.width = topLeft.X - bottomRight.X;
            }
            else
            {
                this.x = topLeft.X;
                this.width = bottomRight.X - topLeft.X;
            }

            if (topLeft.Y > bottomRight.Y)
            {
                this.y = bottomRight.Y;
                this.height = topLeft.Y - bottomRight.Y;
            }
            else
            {
                this.y = topLeft.Y;
                this.height = bottomRight.Y - topLeft.Y;
            }
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
        public Segments GetEdges() { return new() { new(TopLeft, BottomLeft), new(BottomLeft, BottomRight), new(BottomRight, TopRight), new(TopRight, TopLeft) }; }
        public Triangulation Triangulate() { return ToPolygon().Triangulate(); }
        public SegmentShape GetSegmentShape() { return new(GetEdges(), Center); }
        public Circle GetBoundingCircle() { return ToPolygon().GetBoundingCircle(); }
        public Vector2 GetReferencePoint() { return Center; }
        public float GetCircumference() { return width * 2 + height * 2; }
        public float GetCircumferenceSquared() { return GetCircumference() * GetCircumference(); }
        public float GetArea() { return width * height; }
        public Rect GetBoundingBox() { return this; }
        public bool IsPointInside(Vector2 p) { return SGeometry.IsPointInRect(p, TopLeft, Size); }
        public Vector2 GetClosestPoint(Vector2 p) { return ToPolygon().GetClosestPoint(p); }
        public Vector2 GetClosestVertex(Vector2 p) { return ToPolygon().GetClosestVertex(p); }
        public Vector2 GetRandomPoint() { return new(SRNG.randF(x, x + width), SRNG.randF(y, y + height)); }
        public Vector2 GetRandomVertex() { return ToPolygon().GetRandomPoint(); }
        public Segment GetRandomEdge() { return SRNG.randCollection(GetEdges(), false); }
        public Vector2 GetRandomPointOnEdge() { return GetRandomEdge().GetRandomPoint(); }
        
        public void DrawShape(float linethickness, Color color) => this.DrawLines(linethickness, color);

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
    public class Polygon : List<Vector2>, IShape
    {
        public Polygon() { }
        public Polygon(params Vector2[] points) { AddRange(points); }
        //public Polygon(List<Vector2> points) { AddRange(points); }
        public Polygon(IEnumerable<Vector2> edges) { AddRange(edges); }
        public Polygon(IShape shape) { AddRange(shape.ToPolygon()); }
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

        public Vector2 GetReferencePoint() { return GetCentroid(); }
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
                for (int i = 0; i < vertices.Count; i++)
                {
                    Vector2 a = vertices[i];
                    Vector2 b = SUtils.GetItem(vertices, i + 1);
                    Vector2 c = SUtils.GetItem(vertices, i - 1);

                    Vector2 ba = b - a;
                    Vector2 ca = c - a;

                    if (ba.Cross(ca) < 0f) continue;

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
        public Segments GetEdges()
        {
            if (Count <= 1) return new();
            else if (Count == 2)
            {
                return new() { new(this[0], this[1]) };
            }
            Segments segments = new();
            for (int i = 0; i < Count; i++)
            {
                Vector2 start = this[i];
                Vector2 end = this[(i + 1) % Count];
                segments.Add(new(start, end));
            }
            return segments;
        }
        public SegmentShape GetSegmentShape() { return new(GetEdges(), this.GetCentroid()); }
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

        public Polygon ToPolygon() { return this; }
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
        public Vector2 GetRandomVertex() { return SRNG.randCollection(this, false); }
        public Segment GetRandomEdge() { return SRNG.randCollection(GetEdges(), false); }
        public Vector2 GetRandomPointOnEdge() { return GetRandomEdge().GetRandomPoint(); }

        public void DrawShape(float linethickness, Color color) => SDrawing.DrawLines(this, linethickness, color);
        
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
    }


}






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