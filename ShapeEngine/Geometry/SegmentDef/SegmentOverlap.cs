using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.SegmentDef;

public readonly partial struct Segment
{
    public bool OverlapPoint(Vector2 p) => IsPointOnSegment(Start, End, p);
    public bool OverlapSegment(Vector2 segStart, Vector2 segEnd) => OverlapSegmentSegment(Start, End, segStart, segEnd);
    public bool OverlapLine(Vector2 linePoint, Vector2 lineDirection) => OverlapSegmentLine(Start, End, linePoint, lineDirection);
    public bool OverlapRay(Vector2 rayPoint, Vector2 rayDirection) => OverlapSegmentRay(Start, End, rayPoint, rayDirection);
    public bool OverlapCircle(Vector2 circlePoint, float circleRadius) => OverlapSegmentCircle(Start, End, circlePoint, circleRadius);
    public bool OverlapTriangle(Vector2 a, Vector2 b, Vector2 c) => OverlapSegmentTriangle(Start, End, a, b, c);
    public bool OverlapQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapSegmentQuad(Start, End, a, b, c, d);
    public bool OverlapRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapSegmentQuad(Start, End, a, b, c, d);
    public bool OverlapPolygon(List<Vector2> points) => OverlapSegmentPolygon(Start, End, points);
    public bool OverlapPolyline(List<Vector2> points) => OverlapSegmentPolyline(Start, End, points);
    public bool OverlapSegments(List<Segment> segments) => OverlapSegmentSegments(Start, End, segments);

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

    public bool OverlapShape(Segments segments)
    {
        foreach (var seg in segments)
        {
            if (seg.OverlapShape(this)) return true;
        }

        return false;
    }

    public bool OverlapShape(Segment b) => OverlapSegmentSegment(Start, End, b.Start, b.End);
    public bool OverlapShape(Line l) => OverlapSegmentLine(Start, End, l.Point, l.Direction);
    public bool OverlapShape(Ray r) => OverlapSegmentRay(Start, End, r.Point, r.Direction);
    public bool OverlapShape(Circle c) => OverlapSegmentCircle(Start, End, c.Center, c.Radius);

    public bool OverlapShape(Triangle t)
    {
        //we only need to check if 1 point is inside incase the entire segment is inside the shape
        if (t.ContainsPoint(Start)) return true;
        // if (t.ContainsPoint(End)) return true;

        if (OverlapSegmentSegment(Start, End, t.A, t.B)) return true;
        if (OverlapSegmentSegment(Start, End, t.B, t.C)) return true;
        return OverlapSegmentSegment(Start, End, t.C, t.A);
    }

    public bool OverlapShape(Quad q)
    {
        //we only need to check if 1 point is inside incase the entire segment is inside the shape
        if (q.ContainsPoint(Start)) return true;
        // if (q.ContainsPoint(End)) return true;

        if (OverlapSegmentSegment(Start, End, q.A, q.B)) return true;
        if (OverlapSegmentSegment(Start, End, q.B, q.C)) return true;
        if (OverlapSegmentSegment(Start, End, q.C, q.D)) return true;
        return OverlapSegmentSegment(Start, End, q.D, q.A);
    }

    public bool OverlapShape(Rect r)
    {
        if (!r.OverlapRectLine(Start, Displacement)) return false;
        var rectRange = new ValueRange
        (
            r.X,
            r.X + r.Width
        );
        var segmentRange = new ValueRange
        (
            Start.X,
            End.X
        );

        if (!rectRange.OverlapValueRange(segmentRange)) return false;

        rectRange = new(r.Y, r.Y + r.Height);
        // rectRange.Min = r.Y;
        // rectRange.Max = r.Y + r.Height;
        // rectRange.Sort();

        segmentRange = new(Start.Y, End.Y);
        // segmentRange.Min = Start.Y;
        // segmentRange.Max = End.Y;
        // segmentRange.Sort();

        return rectRange.OverlapValueRange(segmentRange);
    }

    public bool OverlapShape(Polygon poly)
    {
        if (poly.Count < 3) return false;
        //we only need to check if 1 point is inside incase the entire segment is inside the shape
        // if (poly.ContainsPoint(Start)) return true;
        var oddNodes = false;
        for (var i = 0; i < poly.Count; i++)
        {
            var a = poly[i];
            var b = poly[(i + 1) % poly.Count];
            if (OverlapSegmentSegment(Start, End, a, b)) return true;
            if (Polygon.ContainsPointCheck(a, b, Start)) oddNodes = !oddNodes;
        }

        return oddNodes;
    }

    public bool OverlapShape(Polyline pl)
    {
        if (pl.Count <= 1) return false;
        if (pl.Count == 2) return OverlapSegmentSegment(Start, End, pl[0], pl[1]);

        for (var i = 0; i < pl.Count - 1; i++)
        {
            var a = pl[i];
            var b = pl[i + 1];
            if (OverlapSegmentSegment(Start, End, a, b)) return true;
        }

        return false;
    }

}