using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.RectDef;

public readonly partial struct Rect
{
    public static bool OverlapRectSegment(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 segmentStart, Vector2 segmentEnd)
    {
        return SegmentDef.Segment.OverlapSegmentQuad(segmentStart, segmentEnd, a, b, c, d);
    }

    public static bool OverlapRectLine(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 linePoint, Vector2 lineDirection)
    {
        return Line.OverlapLineQuad(linePoint, lineDirection, a, b, c, d);
    }

    public static bool OverlapRectRay(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 rayPoint, Vector2 rayDirection)
    {
        return Ray.OverlapRayQuad(rayPoint, rayDirection, a, b, c, d);
    }

    public static bool OverlapRectCircle(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 circleCenter, float circleRadius)
    {
        return Circle.OverlapCircleQuad(circleCenter, circleRadius, a, b, c, d);
    }

    public static bool OverlapRectTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 ta, Vector2 tb, Vector2 tc)
    {
        return Triangle.OverlapTriangleQuad(ta, tb, tc, a, b, c, d);
    }

    public static bool OverlapRectQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 qa, Vector2 qb, Vector2 qc, Vector2 qd)
    {
        return Quad.OverlapQuadQuad(a, b, c, d, qa, qb, qc, qd);
    }

    public static bool OverlapRectRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 ra, Vector2 rb, Vector2 rc, Vector2 rd)
    {
        return Quad.OverlapQuadQuad(a, b, c, d, ra, rb, rc, rd);
    }

    public static bool OverlapRectPolygon(Vector2 a, Vector2 b, Vector2 c, Vector2 d, List<Vector2> points)
    {
        return Quad.OverlapQuadPolygon(a, b, c, d, points);
    }

    public static bool OverlapRectPolyline(Vector2 a, Vector2 b, Vector2 c, Vector2 d, List<Vector2> points)
    {
        return Quad.OverlapQuadPolyline(a, b, c, d, points);
    }

    public static bool OverlapRectSegments(Vector2 a, Vector2 b, Vector2 c, Vector2 d, List<SegmentDef.Segment> segments)
    {
        return Quad.OverlapQuadSegments(a, b, c, d, segments);
    }
}