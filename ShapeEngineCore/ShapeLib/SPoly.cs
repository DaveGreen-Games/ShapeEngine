using System.Numerics;
using Raylib_CsLo;
using ShapeCore;

namespace ShapeLib
{
    public static class SPoly
    {
        public static Rect GetPolyBoundingBox(List<Vector2> poly)
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

        /*
        public static Rectangle GetPolyBoundingBox(Vector2 pos, List<Vector2> poly)
        {
            Rectangle r = new();
            foreach (var p in poly)
            {
                r = SRect.EnlargeRect(r, pos + p);
            }
            return r;
        }
        public static Rectangle GetPolyBoundingBox(Vector2 pos, float rotRad, List<Vector2> poly)
        {
            Rectangle r = new();
            foreach (var p in poly)
            {
                r = SRect.EnlargeRect(r, pos + SVec.Rotate(p, rotRad));
            }
            return r;
        }
    */

        public static List<Vector2> ScalePolygon(List<Vector2> poly, float scale)
        {
            var points = new List<Vector2>();
            for (int i = 0; i < poly.Count; i++)
            {
                points.Add(poly[i] * scale);
            }
            return points;
        }

        public static List<Vector2> ScalePolygonUniform(List<Vector2> poly, float distance)
        {
            var points = new List<Vector2>();
            for (int i = 0; i < poly.Count; i++)
            {
                points.Add(SVec.ScaleUniform(poly[i], distance));
            }
            return points;
        }

        public static List<Vector2> GeneratePolygon(int pointCount, Vector2 center, float minLength, float maxLength)
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

        public static List<(Vector2 start, Vector2 end)> GetPolySegments(List<Vector2> poly)
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
    
        public static List<Vector2> GetPolyAxis(List<Vector2> poly, bool normalized = false)
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
    }
}
