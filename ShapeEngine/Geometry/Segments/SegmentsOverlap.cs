using System.Numerics;

namespace ShapeEngine.Geometry.Segments;

public partial class Segments
{
    #region Overlap

    public bool OverlapSegment(Vector2 segmentStart, Vector2 segmentEnd) => OverlapSegmentsSegment(this, segmentStart, segmentEnd);
    public bool OverlapLine(Vector2 linePoint, Vector2 lineDirection) => OverlapSegmentsLine(this, linePoint, lineDirection);
    public bool OverlapRay(Vector2 rayPoint, Vector2 rayDirection) => OverlapSegmentsRay(this, rayPoint, rayDirection);
    public bool OverlapCircle(Vector2 circleCenter, float circleRadius) => OverlapSegmentsCircle(this, circleCenter, circleRadius);
    public bool OverlapTriangle(Vector2 a, Vector2 b, Vector2 c) => OverlapSegmentsTriangle(this, a, b, c);
    public bool OverlapQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapSegmentsQuad(this, a, b, c, d);
    public bool OverlapRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapSegmentsQuad(this, a, b, c, d);
    public bool OverlapPolygon(List<Vector2> points) => OverlapSegmentsPolygon(this, points);
    public bool OverlapPolyline(List<Vector2> points) => OverlapSegmentsPolyline(this, points);
    public bool OverlapSegments(List<Segment.Segment> segments) => OverlapSegmentsSegments(this, segments);
    public bool OverlapShape(Line.Line line) => OverlapSegmentsLine(this, line.Point, line.Direction);
    public bool OverlapShape(Ray.Ray ray) => OverlapSegmentsRay(this, ray.Point, ray.Direction);

    public bool OverlapShape(Segments b)
    {
        foreach (var seg in this)
        {
            foreach (var bSeg in b)
            {
                if (Segment.Segment.OverlapSegmentSegment(seg.Start, seg.End, bSeg.Start, bSeg.End)) return true;
            }
        }

        return false;
    }

    public bool OverlapShape(Segment.Segment s)
    {
        foreach (var seg in this)
        {
            if (Segment.Segment.OverlapSegmentSegment(seg.Start, seg.End, s.Start, s.End)) return true;
        }

        return false;
    }

    public bool OverlapShape(Circle.Circle c)
    {
        foreach (var seg in this)
        {
            if (Segment.Segment.OverlapSegmentCircle(seg.Start, seg.End, c.Center, c.Radius)) return true;
        }

        return false;
    }

    public bool OverlapShape(Triangle.Triangle t) => t.OverlapShape(this);
    public bool OverlapShape(Quad.Quad q) => q.OverlapShape(this);
    public bool OverlapShape(Rect.Rect r) => r.OverlapShape(this);
    public bool OverlapShape(Polyline.Polyline pl) => pl.OverlapShape(this);
    public bool OverlapShape(Polygon.Polygon p) => p.OverlapShape(this);

    #endregion
}