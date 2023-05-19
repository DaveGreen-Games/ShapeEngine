using ShapeCore;
using System.Numerics;

namespace ShapeLib
{
    
    public static class SCircle
    {
        public static Vector2 GetPoint(this Circle c, float angleRad, float f) { return c.center + new Vector2(c.radius * f, 0f).Rotate(angleRad); }
        public static Segments GetEdges(this Circle c, int pointCount = 16)
        {
            float angleStep = (MathF.PI * 2f) / pointCount;
            Segments segments = new();
            for (int i = 0; i < pointCount; i++)
            {
                Vector2 start = c.center + new Vector2(c.radius, 0f).Rotate(-angleStep * i);
                Vector2 end = c.center + new Vector2(c.radius, 0f).Rotate(-angleStep * ((i + 1) % pointCount));
                segments.Add(new Segment(start, end));
            }
            return segments;
        }
        public static Polygon GetPoints(this Circle c, int pointCount = 16)
        {
            float angleStep = (MathF.PI * 2f) / pointCount;
            Polygon points = new();
            for (int i = 0; i < pointCount; i++)
            {
                Vector2 p = c.center + new Vector2(c.radius, 0f).Rotate(angleStep * i);
                points.Add(p);
            }
            return points;
        }
        
        public static Circle ScaleRadius(this Circle c, float scale) { return new(c.center, c.radius * scale); }
        public static Circle ChangeRadius(this Circle c, float amount) { return new(c.center, c.radius + amount); }
        public static Circle Move(this Circle c, Vector2 offset) { return new(c.center + offset, c.radius); }
        
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
