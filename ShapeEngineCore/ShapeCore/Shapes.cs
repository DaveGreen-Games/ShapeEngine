using System.Diagnostics.CodeAnalysis;
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

        public Line Rotate(float pivot, float rad)
        {
            float l = Length;
            Vector2 d = Dir;

            float startLength = l * pivot;
            float endLength = l * (1f - pivot);

            Vector2 p = start + d * startLength;
            Vector2 newStart = p - (d * startLength).Rotate(rad);
            Vector2 newEnd = p + (d * endLength).Rotate(rad);
            return new Line(newStart, newEnd);
        }
        //scale
        //move

        public float GetArea() { return 0f; }
        public float GetCircumference() { return Length; }
        public float GetCircumferenceSquared() { return LengthSquared; }
        public List<Line> GetSegments() { return new() { this }; }
        public Rect GetBoundingBox() { return new(start, end); }
        //public Circle GetBoundingCircle() { return new(Center, Length / 2); }

        public void DrawShape(float linethickness, Color color)
        {
            throw new NotImplementedException();
        }
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
        //public Circle GetBoundingCircle() { return this; }

        public void DrawShape(float linethickness, Color color)
        {
            throw new NotImplementedException();
        }
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
        public void DrawShape(float linethickness, Color color)
        {
            throw new NotImplementedException();
        }
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

        public Vector2 GetPos(Vector2 alignement)
        {
            Vector2 offset = Size * alignement;
            return TopLeft + offset;
        }
        public float GetCircumference() { return width * 2 + height * 2; }
        public float GetCircumferenceSquared() { return GetCircumference() * GetCircumference(); }
        public float GetArea() { return width * height; }
        public List<Line> GetSegments() { return SRect.GetRectSegments(this); }
        public Rect GetBoundingBox() { return this; }
        public void DrawShape(float linethickness, Color color)
        {
            throw new NotImplementedException();
        }

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
