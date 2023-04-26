using System.Numerics;
using System.Runtime.CompilerServices;
using Raylib_CsLo;
using ShapeLib;

namespace ShapeCore
{
    public interface IShape
    {
        public float GetArea();
        public float GetCircumference();
        public float GetCircumferenceSquared();
        public List<Line> GetSegments();
        public Rect GetBoundingBox();
        //public bool Equals(IShape other);
        //public Circle GetBoundingCircle();
        public void DrawShape(float linethickness, Color color);
    }


    public static class SLine
    {
        public static List<Line> Split(this Line l, float f)
        {
            Vector2 p = GetPoint(l, f);
            return new() { new(l.start, p), new(p, l.end) };
        }
        public static Vector2 GetPoint(this Line l, float f) { return l.start.Lerp(l.end, f); }
        public static Line Rotate(this Line l, float pivot, float rad)
        {
            Vector2 p = GetPoint(l, pivot);
            Vector2 s = l.start - p;
            Vector2 e = l.end - p;
            return new Line(p + s.Rotate(rad), p + e.Rotate(rad));


            //float len = l.Length;
            //Vector2 d = l.Dir;
            //
            //float startLength = len * pivot;
            //float endLength = len * (1f - pivot);
            //
            //Vector2 p = l.start + d * startLength;
            //Vector2 newStart = p - (d * startLength).Rotate(rad);
            //Vector2 newEnd = p + (d * endLength).Rotate(rad);
            //return new Line(newStart, newEnd);
        }
        
        public static Line Scale(this Line l, float scale) { return new(l.start * scale, l.end * scale); }
        public static Line Scale(this Line l, Vector2 scale) { return new(l.start * scale, l.end * scale); }
        public static Line Scale(this Line l, float startScale, float endScale) { return new(l.start * startScale, l.end * endScale); }
        public static Line ScaleF(this Line l, float scale, float f) 
        {
            Vector2 p = GetPoint(l, f);
            Vector2 s = l.start - p;
            Vector2 e = l.end - p;
            return new Line(p + s * scale, p + e * scale);

            //float len = l.Length;
            //Vector2 d = l.Dir;
            //
            //float startLength = len * f;
            //float endLength = len * (1f - f);
            //
            //Vector2 p = l.start + d * startLength;
            //Vector2 newStart = p - (d * startLength * scale);
            //Vector2 newEnd = p + (d * endLength * scale);
            //return new Line(newStart, newEnd);
        }
        public static Line ScaleF(this Line l, Vector2 scale, float f)
        {
            Vector2 p = GetPoint(l, f);
            Vector2 s = l.start - p;
            Vector2 e = l.end - p;
            return new Line(p + s * scale, p + e * scale);
        }
        public static Line Move(this Line l, Vector2 offset, float f) { return new(l.start + (offset * (1f - f)), l.end + (offset * (f))); }
        public static Line Move(this Line l,  Vector2 offset) { return new(l.start + offset, l.end + offset); }
        public static Line Move(this Line l, float x, float y) { return Move(l, new Vector2(x, y)); }
    }
    public static class SCircle
    {
        public static Vector2 GetPoint(this Circle c, float angleRad, float f) { return c.center + new Vector2(c.radius * f, 0f).Rotate(angleRad); }
        
        public static Circle ScaleRadius(this Circle c, float scale) { return new(c.center, c.radius * scale); }
        public static Circle ChangeRadius(this Circle c, float amount) { return new(c.center, c.radius + amount); }
        public static Circle Move(this Circle c, Vector2 offset) { return new(c.center + offset, c.radius); }
    }
    public static class STriangle
    {
        public static List<Triangle> Triangulate(this Triangle t)
        {
            return Triangulate(t, t.Centroid);
        }
        public static List<Triangle> Triangulate(this Triangle t, Vector2 p)
        {
            return new()
            {
                new(t.a, t.b, p),
                new(t.b, t.c, p),
                new(t.c, t.a, p)
            };
        }
        public static Triangle GetInsideTriangle(this Triangle t, float abF, float bcF, float caF)
        {
            Vector2 a = SVec.Lerp(t.a, t.b, abF);
            Vector2 b = SVec.Lerp(t.b, t.c, bcF);
            Vector2 c = SVec.Lerp(t.c, t.a, caF);
            return new(a, b, c);
        }
        public static Vector2 GetPoint(this Triangle t, float f1, float f2)
        {
            float f1Sq = MathF.Sqrt(f1);
            float x = (1f - f1Sq) * t.a.X + (f1Sq * (1f - f2)) * t.b.X + (f1Sq * f2) * t.c.X;
            float y = (1f - f1Sq) * t.a.Y + (f1Sq * (1f - f2)) * t.b.Y + (f1Sq * f2) * t.c.Y;
            return new(x, y);
        }
        public static List<Vector2> GetPoints(this Triangle t) { return new() { t.a, t.b, t.c }; }
        public static Polygon GetPointsPolygon(this Triangle t) { return new(t.Centroid, t.a, t.b, t.c); }
        public static Triangle Rotate(this Triangle t, float rad) { return Rotate(t, t.Centroid, rad); }
        public static Triangle Rotate(this Triangle t, Vector2 pivot, float rad)
        {
            Vector2 a = pivot + (t.a - pivot).Rotate(rad);
            Vector2 b = pivot + (t.b - pivot).Rotate(rad);
            Vector2 c = pivot + (t.c - pivot).Rotate(rad);
            return new(a, b, c);
        }
        public static Triangle Scale(this Triangle t, float scale) { return new(t.a * scale, t.b * scale, t.c * scale); }
        public static Triangle Scale(this Triangle t, Vector2 scale) { return new(t.a * scale, t.b * scale, t.c * scale); }
        public static Triangle Scale(this Triangle t, Vector2 pivot, float scale) 
        {
            Vector2 a = pivot + (t.a - pivot) * scale;
            Vector2 b = pivot + (t.b - pivot) * scale;
            Vector2 c = pivot + (t.c - pivot) * scale;
            return new(a, b, c);
        }
        public static Triangle Scale(this Triangle t, Vector2 pivot, Vector2 scale)
        {
            Vector2 a = pivot + (t.a - pivot) * scale;
            Vector2 b = pivot + (t.b - pivot) * scale;
            Vector2 c = pivot + (t.c - pivot) * scale;
            return new(a, b, c);
        }
        public static Triangle Scale(this Triangle t, float aF, float bF, float cF) { return new(t.a * aF, t.b * bF, t.c * cF); }
        public static Triangle Scale(this Triangle t, Vector2 aF, Vector2 bF, Vector2 cF) { return new(t.a * aF, t.b * bF, t.c * cF); }
        public static Triangle Move(this Triangle t, Vector2 offset) { return new(t.a + offset, t.b + offset, t.c + offset); }
        public static Triangle Move(this Triangle t, Vector2 aOffset, Vector2 bOffset, Vector2 cOffset) { return new(t.a + aOffset, t.b + bOffset, t.c + cOffset); }

        public static bool IsPointInside(this Triangle t, Vector2 p)
        {
            var triangles = Triangulate(t, p);
            float totalArea = triangles.Sum((Triangle t) => { return t.GetArea(); });
            return t.GetArea() >= totalArea;

        }
    }


    public struct Line : IShape
    {
        public Vector2 start;
        public Vector2 end;
        public Vector2 Center { get { return (start + end) * 0.5f; } }
        public Vector2 Dir { get { return Displacement.Normalize(); } }
        public Vector2 Displacement { get { return end - start; } }
        public float Length { get { return Displacement.Length(); } }
        public float LengthSquared { get { return Displacement.LengthSquared(); } }

        public Line(Vector2 start, Vector2 end) { this.start = start; this.end = end; }
        public Line(float startX, float startY, float endX, float endY) { this.start = new(startX, startY); this.end = new(endX, endY); }
        public Line(Line l) { start = l.start; end = l.end; }

       

        public float GetArea() { return 0f; }
        public float GetCircumference() { return Length; }
        public float GetCircumferenceSquared() { return LengthSquared; }
        public List<Line> GetSegments() { return new() { this }; }
        public Rect GetBoundingBox() { return new(start, end); }
        public void DrawShape(float linethickness, Color color) => this.Draw(linethickness, color);
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

        public float GetArea() { return MathF.PI * radius * radius; }
        public float GetCircumference() { return MathF.PI * radius * 2f; }
        public float GetCircumferenceSquared() { return GetCircumference() * GetCircumference(); }
        public Rect GetBoundingBox() { return new Rect(center, new(radius, radius), new(0.5f)); }
        public List<Line> GetSegments() 
        {
            int points = 16;
            float angleStep = (MathF.PI * 2f) / points;
            List<Line> segments = new();
            for (int i = 0; i < points; i++)
            {
                Vector2 start = center + new Vector2(radius, 0f).Rotate(angleStep * i);
                Vector2 end =  center + new Vector2(radius, 0f).Rotate(angleStep * (( i + 1 ) % points));
                segments.Add(new Line(start, end));
            }
            return segments;
        }

        public void DrawShape(float linethickness, Color color) => this.DrawLines(linethickness, color);
    }
    public struct Triangle : IShape
    {
        public Vector2 a, b, c;

        public Vector2 Centroid { get { return (a + b + c) / 3; } }
        public Vector2 A { get { return b - a; } }
        public Vector2 B { get { return c - b; } }
        public Vector2 C { get { return a - c; } }

        public Triangle(Vector2 a, Vector2 b, Vector2 c) { this.a = a; this.b = b; this.c = c; }
        public Triangle(Triangle t) { a = t.a; b = t.b; c = t.c; }

        
        public float GetCircumference() { return MathF.Sqrt(GetCircumferenceSquared()); }
        public float GetCircumferenceSquared() { return A.LengthSquared() + B.LengthSquared() + C.LengthSquared(); }
        public float GetArea() { return (a.X - c.X) * (b.Y - c.Y) - (a.Y - c.Y) * (b.X - c.X); }
        public List<Line> GetSegments() { return new() { new(a, b), new(b, c), new(c, a) }; }
        public Rect GetBoundingBox() { return new Rect(a.X, a.Y, 0, 0).EnlargeRect(b).EnlargeRect(c); }
        //public Circle GetBoundingCircle() { return new(Center, Length / 2); }
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
        public Rect(List<Vector2> points) { this = SPoly.GetBoundingBox(points); }// ???
        public Rect(Rectangle rect)
        {
            this.x = rect.X;
            this.y = rect.Y;
            this.width = rect.width;
            this.height = rect.height;
        }

        public float GetCircumference() { return width * 2 + height * 2; }
        public float GetCircumferenceSquared() { return GetCircumference() * GetCircumference(); }
        public float GetArea() { return width * height; }
        public List<Line> GetSegments() { return SRect.GetRectSegments(this); }
        public Rect GetBoundingBox() { return this; }
        public void DrawShape(float linethickness, Color color) => this.DrawLines(linethickness, color);

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
    public struct Polygon : IShape
    {
        public List<Vector2> points;
        public Vector2 center;

        public Polygon(List<Vector2> points) 
        { this.points = points; this.center = SPoly.GetCentroid(points); }
        public Polygon(params Vector2[] points) { this.points = points.ToList(); this.center = SPoly.GetCentroid(points.ToList()); }
        public Polygon(List<Vector2> points, Vector2 center) { this.points = points; this.center = center; }
        public Polygon(Vector2 center, params Vector2[] points) { this.points = points.ToList(); this.center = center; }
        public Polygon(Triangle t) { this.points = t.GetPoints(); this.center = t.Centroid; }
        public Polygon(Rect r) { this.points = r.GetPoints(); this.center = r.Center; }

        

        public float GetCircumference() { return this.GetCircumference(); }
        public float GetCircumferenceSquared() { return this.GetCircumferenceSquared(); }
        public float GetArea() { return this.GetArea(); }
        public List<Line> GetSegments() { return this.GetSegments(); }
        public Rect GetBoundingBox() { return this.GetBoundingBox(); }
        public void DrawShape(float linethickness, Color color) => SDrawing.DrawPolygonLines(points, linethickness, color);
    }



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
