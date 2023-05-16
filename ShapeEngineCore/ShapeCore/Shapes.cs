using System.ComponentModel.DataAnnotations;
using System.Numerics;
using Raylib_CsLo;
using ShapeLib;

namespace ShapeCore
{
    public struct SegmentShape
    {
        public List<Segment> segments;
        public Vector2 referencePoint;
        public SegmentShape(List<Segment> segments, Vector2 referencePoint) { this.segments = segments; this.referencePoint = referencePoint; }
        public SegmentShape(Vector2 referencePoint, params Segment[] segments) { this.segments = segments.ToList(); this.referencePoint = referencePoint; }
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
        
        public Vector2 GetReferencePoint() { return Center; }
        public float GetArea() { return 0f; }
        public float GetCircumference() { return Length; }
        public float GetCircumferenceSquared() { return LengthSquared; }
        public SegmentShape GetSegmentShape() { return new(start, this); }
        public Polygon GetPolygon() { return new(Center, start, end); }
        public Rect GetBoundingBox() { return new(start, end); }
        public bool IsPointOnShape(Vector2 p) { return this.IsPointInside(p); }
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

        public Vector2 GetReferencePoint() { return center; }
        public float GetArea() { return MathF.PI * radius * radius; }
        public float GetCircumference() { return MathF.PI * radius * 2f; }
        public float GetCircumferenceSquared() { return GetCircumference() * GetCircumference(); }
        public Rect GetBoundingBox() { return new Rect(center, new(radius, radius), new(0.5f)); }
        public SegmentShape GetSegmentShape() { return new(this.GetEdges(), center); }
        public Polygon GetPolygon() { return new(this.GetPoints(), center); }
        public void DrawShape(float linethickness, Color color) => this.DrawLines(linethickness, color);
        public bool IsPointOnShape(Vector2 p) { return this.IsPointInside(p); }
        
        //public void SetPosition(Vector2 newPosition) { center = newPosition; }
    }
    
    /// <summary>
    /// Class that represents a triangle by holding three points. Points a, b, c should be in ccw order!
    /// </summary>
    public struct Triangle : IShape
    {
        public Vector2 a, b, c;

        public Vector2 Centroid { get { return (a + b + c) / 3; } }
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
        
        public Vector2 GetReferencePoint() { return Centroid; }
        public float GetCircumference() { return MathF.Sqrt(GetCircumferenceSquared()); }
        public float GetCircumferenceSquared() { return A.LengthSquared() + B.LengthSquared() + C.LengthSquared(); }
        public float GetArea() { return (a.X - c.X) * (b.Y - c.Y) - (a.Y - c.Y) * (b.X - c.X); }
        public SegmentShape GetSegmentShape() { return new(Centroid, new(a, b), new(b, c), new(c, a) ); }
        public Polygon GetPolygon() { return new(Centroid, a, b, c); }
        public Rect GetBoundingBox() { return new Rect(a.X, a.Y, 0, 0).Enlarge(b).Enlarge(c); }
        public void DrawShape(float linethickness, Color color) => this.DrawLines(linethickness, color);
        public bool IsPointOnShape(Vector2 p) { return this.IsPointInside(p); }
        
        //public Circle GetBoundingCircle() { return new(Center, Length / 2); }
        //public void SetPosition(Vector2 newPosition) 
        //{
        //    Vector2 w = newPosition - Centroid;//displacement
        //    a += w;
        //    b += w;
        //    c += w;
        //}
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
        public Rect(PolygonPath points) { this = SPoly.GetBoundingBox(points); }// ???
        public Rect(Rectangle rect)
        {
            this.x = rect.X;
            this.y = rect.Y;
            this.width = rect.width;
            this.height = rect.height;
        }

        public Vector2 GetReferencePoint() { return Center; }
        public float GetCircumference() { return width * 2 + height * 2; }
        public float GetCircumferenceSquared() { return GetCircumference() * GetCircumference(); }
        public float GetArea() { return width * height; }
        public SegmentShape GetSegmentShape() { return new(SRect.GetEdges(this), Center); }
        public Polygon GetPolygon() { return new(center : Center, TopLeft, BottomLeft, BottomRight, TopRight); }
        public Rect GetBoundingBox() { return this; }
        public bool IsPointOnShape(Vector2 p) { return this.IsPointInside(p); }
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
    
    /// <summary>
    /// Points should be in ccw order!
    /// </summary>
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

    public class PolygonPath : List<Vector2>//, IShape
    {
        public PolygonPath() { }
        public PolygonPath(params Vector2[] points) { AddRange(points); }
        public PolygonPath(List<Vector2> points) { AddRange(points); }
        public PolygonPath(Triangle t) { AddRange(t.GetPoints()); }
        public PolygonPath(Rect rect) { AddRange(rect.GetPoints());}

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
        
        
        /*
        public Vector2 GetReferencePoint() { return this.GetCentroid(); }
        public float GetCircumference() { return this.GetCircumference(); }
        public float GetCircumferenceSquared() { return this.GetCircumferenceSquared(); }
        public float GetArea() { return this.GetArea(); }
        public SegmentShape GetSegmentShape() { return new(SPoly.GetEdges(this), this.GetCentroid()); }
        public Polygon GetPolygon() { return new(this); }
        public Rect GetBoundingBox() { return this.GetBoundingBox(); }
        public void DrawShape(float linethickness, Color color) => SDrawing.DrawPolygonLines(this, linethickness, color);
        public bool IsPointOnShape(Vector2 p) { return SGeometry.IsPointInPoly(p, this); }
        */
    }
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

}
