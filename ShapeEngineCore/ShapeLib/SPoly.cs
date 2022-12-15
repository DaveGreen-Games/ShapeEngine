using System.Numerics;
using Raylib_CsLo;

namespace ShapeLib
{
    public static class SPoly
    {

        public static List<Vector2> ScalePolygon(List<Vector2> poly, float scale)
        {
            var points = new List<Vector2>();
            for (int i = 0; i < poly.Count; i++)
            {
                points.Add(SVec.Scale(poly[i], scale));
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
            List<(Vector2 start, Vector2 end)> segments = new();
            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 start = poly[i];
                Vector2 end = poly[(i+1) % poly.Count];
                segments.Add((start, end));
            }
            return segments;
        }
    }
}
