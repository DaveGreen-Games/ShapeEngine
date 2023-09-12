using ShapeEngine.Core;
using System.Numerics;

namespace ShapeEngine.Lib
{
    
    public static class SCircle
    {
        /*
        public static Vector2 GetPoint(this Circle c, float angleRad, float f) { return c.Center + new Vector2(c.Radius * f, 0f).Rotate(angleRad); }
        public static Segments GetEdges(this Circle c, int pointCount = 16, bool insideNormals = false)
        {
            float angleStep = (MathF.PI * 2f) / pointCount;
            Segments segments = new();
            for (int i = 0; i < pointCount; i++)
            {
                Vector2 start = c.Center + new Vector2(c.Radius, 0f).Rotate(-angleStep * i);
                Vector2 end = c.Center + new Vector2(c.Radius, 0f).Rotate(-angleStep * ((i + 1) % pointCount));

                segments.Add(new Segment(start, end, insideNormals));
            }
            return segments;
        }
        public static Points GetVertices(this Circle c, int pointCount = 16)
        {
            float angleStep = (MathF.PI * 2f) / pointCount;
            Points points = new();
            for (int i = 0; i < pointCount; i++)
            {
                Vector2 p = c.Center + new Vector2(c.Radius, 0f).Rotate(angleStep * i);
                points.Add(p);
            }
            return points;
        }
        public static Polygon GetPolygonPoints(this Circle c, int pointCount = 16)
        {
            float angleStep = (MathF.PI * 2f) / pointCount;
            Polygon points = new();
            for (int i = 0; i < pointCount; i++)
            {
                Vector2 p = c.Center + new Vector2(c.Radius, 0f).Rotate(angleStep * i);
                points.Add(p);
            }
            return points;
        }
        public static Polyline GetPolylinePoints(this Circle c, int pointCount = 16)
        {
            float angleStep = (MathF.PI * 2f) / pointCount;
            Polyline points = new();
            for (int i = 0; i < pointCount; i++)
            {
                Vector2 p = c.Center + new Vector2(c.Radius, 0f).Rotate(angleStep * i);
                points.Add(p);
            }
            return points;
        }

        public static Circle ScaleRadius(this Circle c, float scale) { return new(c.Center, c.Radius * scale); }
        public static Circle ChangeRadius(this Circle c, float amount) { return new(c.Center, c.Radius + amount); }
        public static Circle Move(this Circle c, Vector2 offset) { return new(c.Center + offset, c.Radius); }
        */
        //public static Polygon ToPolygon(this Circle c, int pointCount = 16)
        //{
        //    float angleStep = (MathF.PI * 2f) / pointCount;
        //    List<Vector2> points = new();
        //    for (int i = 0; i < pointCount; i++)
        //    {
        //        Vector2 p = c.center + new Vector2(c.radius, 0f).Rotate(angleStep * i);
        //        points.Add(p);
        //    }
        //    return new(points, c.center);
        //}
        //public static SegmentShape GetSegmentsShape(this Circle c, int pointCount = 16)
        //{
        //    float angleStep = (MathF.PI * 2f) / pointCount;
        //    List<Segment> segments = new();
        //    for (int i = 0; i < pointCount; i++)
        //    {
        //        Vector2 start = c.center + new Vector2(c.radius, 0f).Rotate(angleStep * i);
        //        Vector2 end = c.center + new Vector2(c.radius, 0f).Rotate(angleStep * ((i + 1) % pointCount));
        //        segments.Add(new Segment(start, end));
        //    }
        //    return new(segments, c.center);
        //}
    }

}
