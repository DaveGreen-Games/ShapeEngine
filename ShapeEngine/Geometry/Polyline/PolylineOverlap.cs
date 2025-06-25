using System.Numerics;
using ShapeEngine.Geometry.CollisionSystem;

namespace ShapeEngine.Geometry.Polyline;

public partial class Polyline
{
    public bool OverlapPoint(Vector2 p)
    {
        var segments = GetEdges();
        foreach (var segment in segments)
        {
            if (segment.OverlapPoint(p)) return true;
        }

        return false;
    }

    public bool OverlapSegment(Vector2 segmentStart, Vector2 segmentEnd) => OverlapPolylineSegment(this, segmentStart, segmentEnd);
    public bool OverlapLine(Vector2 linePoint, Vector2 lineDirection) => OverlapPolylineLine(this, linePoint, lineDirection);
    public bool OverlapRay(Vector2 rayPoint, Vector2 rayDirection) => OverlapPolylineRay(this, rayPoint, rayDirection);
    public bool OverlapCircle(Vector2 circleCenter, float circleRadius) => OverlapPolylineCircle(this, circleCenter, circleRadius);
    public bool OverlapTriangle(Vector2 a, Vector2 b, Vector2 c) => OverlapPolylineTriangle(this, a, b, c);
    public bool OverlapQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapPolylineQuad(this, a, b, c, d);
    public bool OverlapRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapPolylineQuad(this, a, b, c, d);
    public bool OverlapPolygon(List<Vector2> points) => OverlapPolylinePolygon(this, points);
    public bool OverlapPolyline(List<Vector2> points) => OverlapPolylinePolyline(this, points);
    public bool OverlapSegments(List<Segment.Segment> segments) => OverlapPolylineSegments(this, segments);
    public bool OverlapShape(Line.Line line) => OverlapPolylineLine(this, line.Point, line.Direction);
    public bool OverlapShape(Ray.Ray ray) => OverlapPolylineRay(this, ray.Point, ray.Direction);

    public bool Overlap(Collider collider)
    {
        if (!collider.Enabled) return false;

        switch (collider.GetShapeType())
        {
            case ShapeType.Circle:
                var c = collider.GetCircleShape();
                return OverlapShape(c);
            case ShapeType.Segment:
                var s = collider.GetSegmentShape();
                return OverlapShape(s);
            case ShapeType.Ray:
                var rayShape = collider.GetRayShape();
                return OverlapShape(rayShape);
            case ShapeType.Line:
                var l = collider.GetLineShape();
                return OverlapShape(l);
            case ShapeType.Triangle:
                var t = collider.GetTriangleShape();
                return OverlapShape(t);
            case ShapeType.Rect:
                var r = collider.GetRectShape();
                return OverlapShape(r);
            case ShapeType.Quad:
                var q = collider.GetQuadShape();
                return OverlapShape(q);
            case ShapeType.Poly:
                var p = collider.GetPolygonShape();
                return OverlapShape(p);
            case ShapeType.PolyLine:
                var pl = collider.GetPolylineShape();
                return OverlapShape(pl);
        }

        return false;
    }

    public bool OverlapShape(Segments.Segments segments)
    {
        if (Count < 2 || segments.Count <= 0) return false;

        for (var i = 0; i < Count - 1; i++)
        {
            var start = this[i];
            var end = this[(i + 1) % Count];

            foreach (var seg in segments)
            {
                if (Segment.Segment.OverlapSegmentSegment(start, end, seg.Start, seg.End)) return true;
            }
        }

        return false;
    }

    public bool OverlapShape(Segment.Segment s) => s.OverlapShape(this);
    public bool OverlapShape(Circle.Circle c) => c.OverlapShape(this);
    public bool OverlapShape(Triangle.Triangle t) => t.OverlapShape(this);
    public bool OverlapShape(Rect.Rect r) => r.OverlapShape(this);
    public bool OverlapShape(Quad.Quad q) => q.OverlapShape(this);
    public bool OverlapShape(Polygon.Polygon p) => p.OverlapShape(this);

    public bool OverlapShape(Polyline b)
    {
        if (Count < 2 || b.Count < 2) return false;

        for (var i = 0; i < Count - 1; i++)
        {
            var start = this[i];
            var end = this[i + 1];

            for (var j = 0; j < b.Count - 1; j++)
            {
                var bStart = b[j];
                var bEnd = b[j + 1];

                if (Segment.Segment.OverlapSegmentSegment(start, end, bStart, bEnd)) return true;
            }
        }

        return false;
    }

}