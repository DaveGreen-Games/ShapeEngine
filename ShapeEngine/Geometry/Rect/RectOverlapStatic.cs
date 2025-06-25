using System.Numerics;

namespace ShapeEngine.Geometry.Rect;

public readonly partial struct Rect
{
    public static bool OverlapRectSegment(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 segmentStart, Vector2 segmentEnd)
    {
        return Segment.Segment.OverlapSegmentQuad(segmentStart, segmentEnd, a, b, c, d);
    }

    public static bool OverlapRectLine(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 linePoint, Vector2 lineDirection)
    {
        return Line.Line.OverlapLineQuad(linePoint, lineDirection, a, b, c, d);
    }

    public static bool OverlapRectRay(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 rayPoint, Vector2 rayDirection)
    {
        return Ray.Ray.OverlapRayQuad(rayPoint, rayDirection, a, b, c, d);
    }

    public static bool OverlapRectCircle(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 circleCenter, float circleRadius)
    {
        return Circle.Circle.OverlapCircleQuad(circleCenter, circleRadius, a, b, c, d);
    }

    public static bool OverlapRectTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 ta, Vector2 tb, Vector2 tc)
    {
        return Triangle.Triangle.OverlapTriangleQuad(ta, tb, tc, a, b, c, d);
    }

    public static bool OverlapRectQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 qa, Vector2 qb, Vector2 qc, Vector2 qd)
    {
        return Quad.Quad.OverlapQuadQuad(a, b, c, d, qa, qb, qc, qd);
    }

    public static bool OverlapRectRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 ra, Vector2 rb, Vector2 rc, Vector2 rd)
    {
        return Quad.Quad.OverlapQuadQuad(a, b, c, d, ra, rb, rc, rd);
    }

    public static bool OverlapRectPolygon(Vector2 a, Vector2 b, Vector2 c, Vector2 d, List<Vector2> points)
    {
        return Quad.Quad.OverlapQuadPolygon(a, b, c, d, points);
    }

    public static bool OverlapRectPolyline(Vector2 a, Vector2 b, Vector2 c, Vector2 d, List<Vector2> points)
    {
        return Quad.Quad.OverlapQuadPolyline(a, b, c, d, points);
    }

    public static bool OverlapRectSegments(Vector2 a, Vector2 b, Vector2 c, Vector2 d, List<Segment.Segment> segments)
    {
        return Quad.Quad.OverlapQuadSegments(a, b, c, d, segments);
    }
}