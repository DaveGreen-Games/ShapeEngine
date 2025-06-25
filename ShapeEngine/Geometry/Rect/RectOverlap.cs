using System.Numerics;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.Segment;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.Rect;

public readonly partial struct Rect
{
    public bool OverlapSegment(Vector2 segmentStart, Vector2 segmentEnd) => OverlapRectSegment(A, B, C, D, segmentStart, segmentEnd);
    public bool OverlapLine(Vector2 linePoint, Vector2 lineDirection) => OverlapRectLine(A, B, C, D, linePoint, lineDirection);
    public bool OverlapRay(Vector2 rayPoint, Vector2 rayDirection) => OverlapRectRay(A, B, C, D, rayPoint, rayDirection);
    public bool OverlapCircle(Vector2 circleCenter, float circleRadius) => OverlapRectCircle(A, B, C, D, circleCenter, circleRadius);
    public bool OverlapTriangle(Vector2 a, Vector2 b, Vector2 c) => OverlapRectTriangle(A, B, C, D, a, b, c);
    public bool OverlapQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapRectQuad(A, B, C, D, a, b, c, d);
    public bool OverlapRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapRectQuad(A, B, C, D, a, b, c, d);
    public bool OverlapPolygon(List<Vector2> points) => OverlapRectPolygon(A, B, C, D, points);
    public bool OverlapPolyline(List<Vector2> points) => OverlapRectPolyline(A, B, C, D, points);
    public bool OverlapSegments(List<Segment.Segment> segments) => OverlapRectSegments(A, B, C, D, segments);
    public bool OverlapShape(Line.Line line) => OverlapRectLine(A, B, C, D, line.Point, line.Direction);
    public bool OverlapShape(Ray.Ray ray) => OverlapRectRay(A, B, C, D, ray.Point, ray.Direction);

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
        foreach (var seg in segments)
        {
            if (seg.OverlapShape(this)) return true;
        }

        return false;
    }

    public bool OverlapShape(Segment.Segment s) => s.OverlapShape(this);
    public bool OverlapShape(Circle.Circle c) => c.OverlapShape(this);
    public bool OverlapShape(Triangle.Triangle t) => t.OverlapShape(this);
    public bool OverlapShape(Quad.Quad q) => q.OverlapShape(this);

    public bool OverlapShape(Rect b)
    {
        var aTopLeft = new Vector2(X, Y);
        var aBottomRight = aTopLeft + new Vector2(Width, Height);
        var bTopLeft = new Vector2(b.X, b.Y);
        var bBottomRight = bTopLeft + new Vector2(b.Width, b.Height);
        return
            ValueRange.OverlapValueRange(aTopLeft.X, aBottomRight.X, bTopLeft.X, bBottomRight.X) &&
            ValueRange.OverlapValueRange(aTopLeft.Y, aBottomRight.Y, bTopLeft.Y, bBottomRight.Y);
    }

    public bool OverlapShape(Polygon.Polygon poly)
    {
        if (poly.Count < 3) return false;

        if (ContainsPoint(poly[0])) return true;

        var oddNodes = false;
        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;
        for (var i = 0; i < poly.Count; i++)
        {
            var start = poly[i];
            var end = poly[(i + 1) % poly.Count];
            if (Segment.Segment.OverlapSegmentSegment(a, b, start, end)) return true;
            if (Segment.Segment.OverlapSegmentSegment(b, c, start, end)) return true;
            if (Segment.Segment.OverlapSegmentSegment(c, d, start, end)) return true;
            if (Segment.Segment.OverlapSegmentSegment(d, a, start, end)) return true;

            if (Polygon.Polygon.ContainsPointCheck(start, end, a)) oddNodes = !oddNodes;
        }

        return oddNodes;
    }

    public bool OverlapShape(Polyline.Polyline pl)
    {
        if (pl.Count < 2) return false;

        if (ContainsPoint(pl[0])) return true;
        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;
        for (var i = 0; i < pl.Count - 1; i++)
        {
            var start = pl[i];
            var end = pl[(i + 1) % pl.Count];
            if (Segment.Segment.OverlapSegmentSegment(a, b, start, end)) return true;
            if (Segment.Segment.OverlapSegmentSegment(b, c, start, end)) return true;
            if (Segment.Segment.OverlapSegmentSegment(c, d, start, end)) return true;
            if (Segment.Segment.OverlapSegmentSegment(d, a, start, end)) return true;
        }

        return false;
    }

    public bool OverlapRectLine(Vector2 linePos, Vector2 lineDir)
    {
        var n = lineDir.Rotate90CCW();

        var c1 = new Vector2(X, Y);
        var c2 = c1 + new Vector2(Width, Height);
        var c3 = new Vector2(c2.X, c1.Y);
        var c4 = new Vector2(c1.X, c2.Y);

        c1 -= linePos;
        c2 -= linePos;
        c3 -= linePos;
        c4 -= linePos;

        float dp1 = Vector2.Dot(n, c1);
        float dp2 = Vector2.Dot(n, c2);
        float dp3 = Vector2.Dot(n, c3);
        float dp4 = Vector2.Dot(n, c4);

        return dp1 * dp2 <= 0.0f || dp2 * dp3 <= 0.0f || dp3 * dp4 <= 0.0f;
    }
}