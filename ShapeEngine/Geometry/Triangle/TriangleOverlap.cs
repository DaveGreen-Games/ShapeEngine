using System.Numerics;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.Segment;

namespace ShapeEngine.Geometry.Triangle;

public readonly partial struct Triangle
{
    public bool OverlapSegment(Vector2 segmentStart, Vector2 segmentEnd) => OverlapTriangleSegment(A, B, C, segmentStart, segmentEnd);
    public bool OverlapLine(Vector2 linePoint, Vector2 lineDirection) => OverlapTriangleLine(A, B, C, linePoint, lineDirection);
    public bool OverlapRay(Vector2 rayPoint, Vector2 rayDirection) => OverlapTriangleRay(A, B, C, rayPoint, rayDirection);
    public bool OverlapCircle(Vector2 circleCenter, float circleRadius) => OverlapTriangleCircle(A, B, C, circleCenter, circleRadius);
    public bool OverlapTriangle(Vector2 a, Vector2 b, Vector2 c) => OverlapTriangleTriangle(A, B, C, a, b, c);
    public bool OverlapQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapTriangleQuad(A, B, C, a, b, c, d);
    public bool OverlapRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapTriangleQuad(A, B, C, a, b, c, d);
    public bool OverlapPolygon(List<Vector2> points) => OverlapTrianglePolygon(A, B, C, points);
    public bool OverlapPolyline(List<Vector2> points) => OverlapTrianglePolyline(A, B, C, points);
    public bool OverlapSegments(List<Segment.Segment> segments) => OverlapTriangleSegments(A, B, C, segments);
    public bool OverlapShape(Line.Line line) => OverlapTriangleLine(A, B, C, line.Point, line.Direction);
    public bool OverlapShape(Ray.Ray ray) => OverlapTriangleRay(A, B, C, ray.Point, ray.Direction);

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
            case ShapeType.Line:
                var l = collider.GetLineShape();
                return OverlapShape(l);
            case ShapeType.Ray:
                var rayShape = collider.GetRayShape();
                return OverlapShape(rayShape);
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
        if (segments.Count <= 0) return false;

        if (ContainsPoint(segments[0].Start)) return true;

        foreach (var seg in segments)
        {
            if (Segment.Segment.OverlapSegmentSegment(A, B, seg.Start, seg.End)) return true;
            if (Segment.Segment.OverlapSegmentSegment(B, C, seg.Start, seg.End)) return true;
            if (Segment.Segment.OverlapSegmentSegment(C, A, seg.Start, seg.End)) return true;
        }

        return false;
    }

    public bool OverlapShape(Segment.Segment s) => s.OverlapShape(this);
    public bool OverlapShape(Circle.Circle c) => c.OverlapShape(this);

    public bool OverlapShape(Triangle b)
    {
        if (ContainsPoint(b.A)) return true;

        if (b.ContainsPoint(A)) return true;

        if (Segment.Segment.OverlapSegmentSegment(A, B, b.A, b.B)) return true;
        if (Segment.Segment.OverlapSegmentSegment(A, B, b.B, b.C)) return true;
        if (Segment.Segment.OverlapSegmentSegment(A, B, b.C, b.A)) return true;

        if (Segment.Segment.OverlapSegmentSegment(B, C, b.A, b.B)) return true;
        if (Segment.Segment.OverlapSegmentSegment(B, C, b.B, b.C)) return true;
        if (Segment.Segment.OverlapSegmentSegment(B, C, b.C, b.A)) return true;

        if (Segment.Segment.OverlapSegmentSegment(C, A, b.A, b.B)) return true;
        if (Segment.Segment.OverlapSegmentSegment(C, A, b.B, b.C)) return true;
        return Segment.Segment.OverlapSegmentSegment(C, A, b.C, b.A);
    }

    public bool OverlapShape(Rect.Rect r)
    {
        var a = r.TopLeft;
        if (ContainsPoint(a)) return true;

        if (r.ContainsPoint(A)) return true;

        var b = r.BottomLeft;
        if (Segment.Segment.OverlapSegmentSegment(A, B, a, b)) return true;

        var c = r.BottomRight;
        if (Segment.Segment.OverlapSegmentSegment(A, B, b, c)) return true;

        var d = r.TopRight;
        if (Segment.Segment.OverlapSegmentSegment(A, B, c, d)) return true;
        if (Segment.Segment.OverlapSegmentSegment(A, B, d, a)) return true;

        if (Segment.Segment.OverlapSegmentSegment(B, C, a, b)) return true;
        if (Segment.Segment.OverlapSegmentSegment(B, C, b, c)) return true;
        if (Segment.Segment.OverlapSegmentSegment(B, C, c, d)) return true;
        if (Segment.Segment.OverlapSegmentSegment(B, C, d, a)) return true;

        if (Segment.Segment.OverlapSegmentSegment(C, A, a, b)) return true;
        if (Segment.Segment.OverlapSegmentSegment(C, A, b, c)) return true;
        if (Segment.Segment.OverlapSegmentSegment(C, A, c, d)) return true;
        return Segment.Segment.OverlapSegmentSegment(C, A, d, a);
    }

    public bool OverlapShape(Quad.Quad q)
    {
        if (ContainsPoint(q.A)) return true;

        if (q.ContainsPoint(A)) return true;

        if (Segment.Segment.OverlapSegmentSegment(A, B, q.A, q.B)) return true;
        if (Segment.Segment.OverlapSegmentSegment(A, B, q.B, q.C)) return true;
        if (Segment.Segment.OverlapSegmentSegment(A, B, q.C, q.D)) return true;
        if (Segment.Segment.OverlapSegmentSegment(A, B, q.D, q.A)) return true;

        if (Segment.Segment.OverlapSegmentSegment(B, C, q.A, q.B)) return true;
        if (Segment.Segment.OverlapSegmentSegment(B, C, q.B, q.C)) return true;
        if (Segment.Segment.OverlapSegmentSegment(B, C, q.C, q.D)) return true;
        if (Segment.Segment.OverlapSegmentSegment(B, C, q.D, q.A)) return true;

        if (Segment.Segment.OverlapSegmentSegment(C, A, q.A, q.B)) return true;
        if (Segment.Segment.OverlapSegmentSegment(C, A, q.B, q.C)) return true;
        if (Segment.Segment.OverlapSegmentSegment(C, A, q.C, q.D)) return true;
        return Segment.Segment.OverlapSegmentSegment(C, A, q.D, q.A);
    }

    public bool OverlapShape(Polygon.Polygon poly)
    {
        if (poly.Count < 3) return false;

        if (ContainsPoint(poly[0])) return true;

        var oddNodes = false;

        for (var i = 0; i < poly.Count; i++)
        {
            var start = poly[i];
            var end = poly[(i + 1) % poly.Count];
            if (Segment.Segment.OverlapSegmentSegment(A, B, start, end)) return true;
            if (Segment.Segment.OverlapSegmentSegment(B, C, start, end)) return true;
            if (Segment.Segment.OverlapSegmentSegment(C, A, start, end)) return true;

            if (Polygon.Polygon.ContainsPointCheck(start, end, A)) oddNodes = !oddNodes;
        }

        return oddNodes;
    }

    public bool OverlapShape(Polyline.Polyline pl)
    {
        if (pl.Count < 2) return false;

        if (ContainsPoint(pl[0])) return true;

        for (var i = 0; i < pl.Count - 1; i++)
        {
            var start = pl[i];
            var end = pl[(i + 1) % pl.Count];
            if (Segment.Segment.OverlapSegmentSegment(A, B, start, end)) return true;
            if (Segment.Segment.OverlapSegmentSegment(B, C, start, end)) return true;
            if (Segment.Segment.OverlapSegmentSegment(C, A, start, end)) return true;
        }

        return false;
    }

}