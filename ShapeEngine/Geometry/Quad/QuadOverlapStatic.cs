using System.Numerics;

namespace ShapeEngine.Geometry.Quad;

public readonly partial struct Quad
{
    public static bool OverlapQuadSegment(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 segmentStart, Vector2 segmentEnd)
    {
        return Segment.Segment.OverlapSegmentQuad(segmentStart, segmentEnd, a, b, c, d);
    }

    public static bool OverlapQuadLine(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 linePoint, Vector2 lineDirection)
    {
        return Line.Line.OverlapLineQuad(linePoint, lineDirection, a, b, c, d);
    }

    public static bool OverlapQuadRay(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 rayPoint, Vector2 rayDirection)
    {
        return Ray.Ray.OverlapRayQuad(rayPoint, rayDirection, a, b, c, d);
    }

    public static bool OverlapQuadCircle(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 circleCenter, float circleRadius)
    {
        return Circle.Circle.OverlapCircleQuad(circleCenter, circleRadius, a, b, c, d);
    }

    public static bool OverlapQuadTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 ta, Vector2 tb, Vector2 tc)
    {
        return Triangle.Triangle.OverlapTriangleQuad(ta, tb, tc, a, b, c, d);
    }

    public static bool OverlapQuadQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 qa, Vector2 qb, Vector2 qc, Vector2 qd)
    {
        if (ContainsQuadPoint(a, b, c, d, qa)) return true;
        if (ContainsQuadPoint(qa, qb, qc, qd, a)) return true;

        if (Segment.Segment.OverlapSegmentSegment(a, b, qa, qb)) return true;
        if (Segment.Segment.OverlapSegmentSegment(a, b, qb, qc)) return true;
        if (Segment.Segment.OverlapSegmentSegment(a, b, qc, qd)) return true;
        if (Segment.Segment.OverlapSegmentSegment(a, b, qd, qa)) return true;
        if (Segment.Segment.OverlapSegmentSegment(b, c, qa, qb)) return true;
        if (Segment.Segment.OverlapSegmentSegment(b, c, qb, qc)) return true;
        if (Segment.Segment.OverlapSegmentSegment(b, c, qc, qd)) return true;
        if (Segment.Segment.OverlapSegmentSegment(b, c, qd, qa)) return true;
        if (Segment.Segment.OverlapSegmentSegment(c, d, qa, qb)) return true;
        if (Segment.Segment.OverlapSegmentSegment(c, d, qb, qc)) return true;
        if (Segment.Segment.OverlapSegmentSegment(c, d, qc, qd)) return true;
        if (Segment.Segment.OverlapSegmentSegment(c, d, qd, qa)) return true;
        if (Segment.Segment.OverlapSegmentSegment(d, a, qa, qb)) return true;
        if (Segment.Segment.OverlapSegmentSegment(d, a, qb, qc)) return true;
        if (Segment.Segment.OverlapSegmentSegment(d, a, qc, qd)) return true;
        if (Segment.Segment.OverlapSegmentSegment(d, a, qd, qa)) return true;

        return false;
    }

    public static bool OverlapQuadRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 ra, Vector2 rb, Vector2 rc, Vector2 rd)
    {
        return OverlapQuadQuad(a, b, c, d, ra, rb, rc, rd);
    }

    public static bool OverlapQuadPolygon(Vector2 a, Vector2 b, Vector2 c, Vector2 d, List<Vector2> points)
    {
        if (points.Count < 3) return false;
        // if (Polygon.ContainsPoints(points, a)) return true;
        var oddNodes = false;
        for (var i = 0; i < points.Count; i++)
        {
            var p1 = points[i];
            var p2 = points[(i + 1) % points.Count];
            if (Segment.Segment.OverlapSegmentSegment(a, b, p1, p2)) return true;
            if (Segment.Segment.OverlapSegmentSegment(b, c, p1, p2)) return true;
            if (Segment.Segment.OverlapSegmentSegment(c, d, p1, p2)) return true;
            if (Segment.Segment.OverlapSegmentSegment(d, a, p1, p2)) return true;
            if (Polygon.Polygon.ContainsPointCheck(p1, p2, a)) oddNodes = !oddNodes;
        }

        return oddNodes;
    }

    public static bool OverlapQuadPolyline(Vector2 a, Vector2 b, Vector2 c, Vector2 d, List<Vector2> points)
    {
        if (points.Count < 2) return false;
        for (var i = 0; i < points.Count - 1; i++)
        {
            var p1 = points[i];
            var p2 = points[i + 1];
            if (Segment.Segment.OverlapSegmentSegment(a, b, p1, p2)) return true;
            if (Segment.Segment.OverlapSegmentSegment(b, c, p1, p2)) return true;
            if (Segment.Segment.OverlapSegmentSegment(c, d, p1, p2)) return true;
            if (Segment.Segment.OverlapSegmentSegment(d, a, p1, p2)) return true;
        }

        return false;
    }

    public static bool OverlapQuadSegments(Vector2 a, Vector2 b, Vector2 c, Vector2 d, List<Segment.Segment> segments)
    {
        if (segments.Count < 3) return false;
        for (var i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];
            if (Segment.Segment.OverlapSegmentSegment(a, b, segment.Start, segment.End)) return true;
            if (Segment.Segment.OverlapSegmentSegment(b, c, segment.Start, segment.End)) return true;
            if (Segment.Segment.OverlapSegmentSegment(c, d, segment.Start, segment.End)) return true;
            if (Segment.Segment.OverlapSegmentSegment(d, a, segment.Start, segment.End)) return true;
        }

        return false;
    }

}