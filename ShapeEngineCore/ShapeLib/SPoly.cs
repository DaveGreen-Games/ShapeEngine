using Raylib_CsLo;
using ShapeCore;
using System.Numerics;

namespace ShapeLib
{
    public static class SPoly
    {
        /*
        public static List<Vector2> GetShape(List<Vector2> points, Vector2 pos, float rotRad, Vector2 scale)
        {
            List<Vector2> poly = new();
            for (int i = 0; i < points.Count; i++)
            {
                poly.Add(pos + SVec.Rotate(points[i], rotRad) * scale);
            }
            return poly;
        }
        public static Rect GetBoundingBox(List<Vector2> poly)
        {
            if (poly.Count < 2) return new();
            Vector2 start = poly[0];
            Rect r = new(start.X, start.Y, 0, 0);

            foreach (var p in poly)
            {
                r = SRect.EnlargeRect(r, p);
            }
            return r;
        }

        public static List<Vector2> Scale(List<Vector2> poly, float scale)
        {
            var points = new List<Vector2>();
            for (int i = 0; i < poly.Count; i++)
            {
                points.Add(poly[i] * scale);
            }
            return points;
        }

        public static List<Vector2> ScaleUniform(List<Vector2> poly, float distance)
        {
            var points = new List<Vector2>();
            for (int i = 0; i < poly.Count; i++)
            {
                points.Add(SVec.ScaleUniform(poly[i], distance));
            }
            return points;
        }

        public static List<Vector2> GeneratePoints(int pointCount, Vector2 center, float minLength, float maxLength)
        {
            List<Vector2> points = new();
            float angleStep = RayMath.PI * 2.0f / pointCount;

            for (int i = 0; i < pointCount; i++)
            {
                float randLength = SRNG.randF(minLength, maxLength);
                Vector2 p = SVec.Rotate(SVec.Right(), angleStep * i) * randLength;
                p += center;
                points.Add(p);
            }
            return points;
        }

        public static List<(Vector2 start, Vector2 end)> GetSegments(List<Vector2> poly)
        {
            if (poly.Count <= 1) return new();
            else if (poly.Count == 2)
            {
                return new() { (poly[0], poly[1]) };
            }
            List<(Vector2 start, Vector2 end)> segments = new();
            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 start = poly[i];
                Vector2 end = poly[(i+1) % poly.Count];
                segments.Add((start, end));
            }
            return segments;
        }
    
        public static List<Vector2> GetSegmentAxis(List<Vector2> poly, bool normalized = false)
        {
            if (poly.Count <= 1) return new();
            else if(poly.Count == 2)
            {
                return new() { poly[1] - poly[0] };
            }
            List<Vector2> axis = new();
            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 start = poly[i];
                Vector2 end = poly[(i + 1) % poly.Count];
                Vector2 a = end - start;
                axis.Add(normalized ? SVec.Normalize(a) : a);
            }
            return axis;
        }
        */

        public static List<Vector2> GetShape(List<Vector2> points, Vector2 pos, float rotRad, Vector2 scale)
        {
            if (points.Count < 3) return new();
            List<Vector2> shape = new();
            for (int i = 0; i < points.Count; i++)
            {
                shape.Add(pos + SVec.Rotate(points[i], rotRad) * scale);
            }
            return shape;
        }
        public static Rect GetBoundingBox(List<Vector2> shape)
        {
            if (shape.Count < 2) return new();
            Vector2 start = shape[0];
            Rect r = new(start.X, start.Y, 0, 0);

            foreach (var p in shape)
            {
                r = SRect.EnlargeRect(r, p);
            }
            return r;
        }
        public static List<Vector2> Scale(List<Vector2> points, float scale)
        {
            var points = new List<Vector2>();
            for (int i = 0; i < points.Count; i++)
            {
                points.Add(points[i] * scale);
            }
            return points;
        }
        public static List<Vector2> ScaleUniform(List<Vector2> points, float distance)
        {
            var points = new List<Vector2>();
            for (int i = 0; i < points.Count; i++)
            {
                points.Add(SVec.ScaleUniform(points[i], distance));
            }
            return points;
        }
        public static List<Vector2> GeneratePoints(Vector2 center, int pointCount, float minLength, float maxLength)
        {
            List<Vector2> points = new();
            float angleStep = RayMath.PI * 2.0f / pointCount;

            for (int i = 0; i < pointCount; i++)
            {
                float randLength = SRNG.randF(minLength, maxLength);
                Vector2 p = SVec.Rotate(SVec.Right(), angleStep * i) * randLength;
                p += center;
                points.Add(p);
            }
            return points;
        }
        public static List<Vector2> GeneratePoints(int pointCount, float minLength, float maxLength)
        {
            List<Vector2> points = new();
            float angleStep = RayMath.PI * 2.0f / pointCount;

            for (int i = 0; i < pointCount; i++)
            {
                float randLength = SRNG.randF(minLength, maxLength);
                Vector2 p = SVec.Rotate(SVec.Right(), angleStep * i) * randLength;
                points.Add(p);
            }
            return points;
        }
        public static List<Line> GetSegments(List<Vector2> shape)
        {
            if (shape.Count <= 1) return new();
            else if (shape.Count == 2)
            {
                return new() { new(shape[0], shape[1]) };
            }
            List<Line> segments = new();
            for (int i = 0; i < shape.Count; i++)
            {
                Vector2 start = shape[i];
                Vector2 end = shape[(i + 1) % shape.Count];
                segments.Add(new(start, end));
            }
            return segments;
        }
        public static List<Vector2> GetSegmentAxis(List<Vector2> shape, bool normalized = false)
        {
            if (shape.Count <= 1) return new();
            else if (shape.Count == 2)
            {
                return new() { shape[1] - shape[0] };
            }
            List<Vector2> axis = new();
            for (int i = 0; i < shape.Count; i++)
            {
                Vector2 start = shape[i];
                Vector2 end = shape[(i + 1) % shape.Count];
                Vector2 a = end - start;
                axis.Add(normalized ? SVec.Normalize(a) : a);
            }
            return axis;
        }
        public static List<Vector2> GetSegmentAxis(List<Line> segments, bool normalized = false)
        {
            List<Vector2> axis = new();
            foreach (var seg in segments)
            {
                axis.Add(normalized ? seg.Dir : seg.Displacement);
            }
            return axis;
        }
        public static List<Triangle> Triangulate(List<Vector2> shape, Vector2 center)
        {
            if (shape.Count < 3) return new();
            List<Triangle> triangles = new();
            shape.Add(shape[0]);
            for (int i = 0; i < shape.Count - 1; i++)
            {
                Vector2 a = shape[i];
                Vector2 b = center;
                Vector2 c = shape[i + 1];
                triangles.Add(new(a, b, c));
            }
            return triangles;
        }
        public static float GetArea(List<Vector2> shape, Vector2 center)
        {
            if (shape.Count < 3) return 0f;
            var triangles = Triangulate(shape, center);
            float totalArea = 0f;
            foreach (var t in triangles)
            {
                totalArea += t.Area;
            }
            return totalArea;
        }
        public static float GetCircumference(List<Vector2> shape)
        {
            return MathF.Sqrt(GetCircumferenceSquared(shape));
        }
        public static float GetCircumferenceSquared(List<Vector2> shape)
        {
            if (shape.Count < 3) return 0f;
            float lengthSq = 0f;
            shape.Add(shape[0]);
            for (int i = 0; i < shape.Count - 1; i++)
            {
                Vector2 w = shape[i + 1] - shape[i];
                lengthSq += w.LengthSquared();
            }
            return lengthSq;
        }
        
        
        /*
        public static List<Vector2> GetShape(this Polygon p)
        {
            return GetShape(p.points, p.pos, p.rotRad, p.scale);
        }
        public static Rect GetBoundingBox(this Polygon p)
        {
            return GetBoundingBox(p.GetShape());
        }
        public static List<Line> GetSegments(this Polygon p)
        {
            return GetSegments(p.GetShape());
        }
        public static List<Vector2> GetSegmentAxis(this Polygon p, bool normalized = false)
        {
            return GetSegmentAxis(p.GetShape(), normalized);
        }
        public static List<Triangle> Triangulate(this Polygon p) { return Triangulate(p.GetShape(), p.pos); }
        public static float GetArea(this Polygon p) { return GetArea(p.GetShape(), p.pos); }
        public static float GetCircumference(this Polygon p) { return GetCircumference(p.GetShape()); }
        public static float GetCircumferenceSquared(this Polygon p) { return GetCircumferenceSquared(p.GetShape()); }
        */
    }
}
