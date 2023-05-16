using ShapeCore;
using System.Data;
using System.Numerics;

namespace ShapeLib
{
    public static class STriangle
    {
        public static Vector2 GetClosestPoint(this Triangle t, Vector2 p)
        {
            return SPoly.GetClosestPoint(t.GetPoints(), p);
        }
        public static Vector2 GetClosestVertex(this Triangle t, Vector2 p)
        {
            return SPoly.GetClosestVertex(t.GetPoints(), p);
        }
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
        public static Vector2 GetRandomPointOnEdge(this Triangle t)
        {
            var edges = GetEdges(t);
            var re = edges[SRNG.randI(edges.Count)];
            return re.start.Lerp(re.end, SRNG.randF());
        }
        public static Vector2 GetRandomPoint(this Triangle t) { return GetPoint(t, SRNG.randF(), SRNG.randF()); }

        public static PolygonPath GetPoints(this Triangle t) { return new() { t.a, t.b, t.c }; }
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
        public static List<Segment> GetEdges(this Triangle t) { return new() { new(t.a, t.b), new(t.b, t.c), new(t.c, t.a) }; }
        //public static SegmentShape GetSegmentsShape(this Triangle t) { return new SegmentShape(t.Centroid, new(t.a, t.b), new(t.b, t.c), new(t.c, t.a)); }

        //public static List<Vector2> GetPoints(this Triangle t) { return new() { t.a, t.b, t.c }; }
        //public static bool IsPointInside(this Triangle t, Vector2 p)
        //{
        //    var triangles = Triangulate(t, p);
        //    float totalArea = triangles.Sum((Triangle t) => { return t.GetArea(); });
        //    return t.GetArea() >= totalArea;
        //
        //}
    }

}
