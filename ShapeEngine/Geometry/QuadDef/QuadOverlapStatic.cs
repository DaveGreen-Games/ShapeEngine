using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.QuadDef;

public readonly partial struct Quad
{
    public static bool OverlapQuadSegment(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 segmentStart, Vector2 segmentEnd)
    {
        return SegmentDef.Segment.OverlapSegmentQuad(segmentStart, segmentEnd, a, b, c, d);
    }

    public static bool OverlapQuadLine(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 linePoint, Vector2 lineDirection)
    {
        return Line.OverlapLineQuad(linePoint, lineDirection, a, b, c, d);
    }

    public static bool OverlapQuadRay(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 rayPoint, Vector2 rayDirection)
    {
        return Ray.OverlapRayQuad(rayPoint, rayDirection, a, b, c, d);
    }

    public static bool OverlapQuadCircle(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 circleCenter, float circleRadius)
    {
        return Circle.OverlapCircleQuad(circleCenter, circleRadius, a, b, c, d);
    }

    public static bool OverlapQuadTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 ta, Vector2 tb, Vector2 tc)
    {
        return Triangle.OverlapTriangleQuad(ta, tb, tc, a, b, c, d);
    }

    public static bool OverlapQuadQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 qa, Vector2 qb, Vector2 qc, Vector2 qd)
    {
        if (ContainsQuadPoint(a, b, c, d, qa)) return true;
        if (ContainsQuadPoint(qa, qb, qc, qd, a)) return true;

        if (SegmentDef.Segment.OverlapSegmentSegment(a, b, qa, qb)) return true;
        if (SegmentDef.Segment.OverlapSegmentSegment(a, b, qb, qc)) return true;
        if (SegmentDef.Segment.OverlapSegmentSegment(a, b, qc, qd)) return true;
        if (SegmentDef.Segment.OverlapSegmentSegment(a, b, qd, qa)) return true;
        if (SegmentDef.Segment.OverlapSegmentSegment(b, c, qa, qb)) return true;
        if (SegmentDef.Segment.OverlapSegmentSegment(b, c, qb, qc)) return true;
        if (SegmentDef.Segment.OverlapSegmentSegment(b, c, qc, qd)) return true;
        if (SegmentDef.Segment.OverlapSegmentSegment(b, c, qd, qa)) return true;
        if (SegmentDef.Segment.OverlapSegmentSegment(c, d, qa, qb)) return true;
        if (SegmentDef.Segment.OverlapSegmentSegment(c, d, qb, qc)) return true;
        if (SegmentDef.Segment.OverlapSegmentSegment(c, d, qc, qd)) return true;
        if (SegmentDef.Segment.OverlapSegmentSegment(c, d, qd, qa)) return true;
        if (SegmentDef.Segment.OverlapSegmentSegment(d, a, qa, qb)) return true;
        if (SegmentDef.Segment.OverlapSegmentSegment(d, a, qb, qc)) return true;
        if (SegmentDef.Segment.OverlapSegmentSegment(d, a, qc, qd)) return true;
        if (SegmentDef.Segment.OverlapSegmentSegment(d, a, qd, qa)) return true;

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
            if (SegmentDef.Segment.OverlapSegmentSegment(a, b, p1, p2)) return true;
            if (SegmentDef.Segment.OverlapSegmentSegment(b, c, p1, p2)) return true;
            if (SegmentDef.Segment.OverlapSegmentSegment(c, d, p1, p2)) return true;
            if (SegmentDef.Segment.OverlapSegmentSegment(d, a, p1, p2)) return true;
            if (Polygon.ContainsPointCheck(p1, p2, a)) oddNodes = !oddNodes;
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
            if (SegmentDef.Segment.OverlapSegmentSegment(a, b, p1, p2)) return true;
            if (SegmentDef.Segment.OverlapSegmentSegment(b, c, p1, p2)) return true;
            if (SegmentDef.Segment.OverlapSegmentSegment(c, d, p1, p2)) return true;
            if (SegmentDef.Segment.OverlapSegmentSegment(d, a, p1, p2)) return true;
        }

        return false;
    }

    public static bool OverlapQuadSegments(Vector2 a, Vector2 b, Vector2 c, Vector2 d, List<SegmentDef.Segment> segments)
    {
        if (segments.Count < 3) return false;
        for (var i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];
            if (SegmentDef.Segment.OverlapSegmentSegment(a, b, segment.Start, segment.End)) return true;
            if (SegmentDef.Segment.OverlapSegmentSegment(b, c, segment.Start, segment.End)) return true;
            if (SegmentDef.Segment.OverlapSegmentSegment(c, d, segment.Start, segment.End)) return true;
            if (SegmentDef.Segment.OverlapSegmentSegment(d, a, segment.Start, segment.End)) return true;
        }

        return false;
    }

}