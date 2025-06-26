using System.Numerics;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.TriangleDef;

public readonly partial struct Triangle
{
    public static bool ContainsTrianglePoint(Vector2 tA, Vector2 tB, Vector2 tC, Vector2 point)
    {
        var ab = tB - tA;
        var bc = tC - tB;
        var ca = tA - tC;

        var ap = point - tA;
        var bp = point - tB;
        var cp = point - tC;

        float c1 = ab.Cross(ap);
        float c2 = bc.Cross(bp);
        float c3 = ca.Cross(cp);

        return c1 < 0f && c2 < 0f && c3 < 0f;
    }

    public static bool ContainsTrianglePoints(Vector2 tA, Vector2 tB, Vector2 tC, Vector2 u, Vector2 v)
    {
        return ContainsTrianglePoint(tA, tB, tC, u) &&
               ContainsTrianglePoint(tA, tB, tC, v);
    }

    public static bool ContainsTrianglePoints(Vector2 tA, Vector2 tB, Vector2 tC, Vector2 u, Vector2 v, Vector2 w)
    {
        return ContainsTrianglePoint(tA, tB, tC, u) &&
               ContainsTrianglePoint(tA, tB, tC, v) &&
               ContainsTrianglePoint(tA, tB, tC, w);
    }

    public static bool ContainsTrianglePoints(Vector2 tA, Vector2 tB, Vector2 tC, Vector2 u, Vector2 v, Vector2 w, Vector2 x)
    {
        return ContainsTrianglePoint(tA, tB, tC, u) &&
               ContainsTrianglePoint(tA, tB, tC, v) &&
               ContainsTrianglePoint(tA, tB, tC, w) &&
               ContainsTrianglePoint(tA, tB, tC, x);
    }

    public static bool ContainsTrianglePoints(Vector2 tA, Vector2 tB, Vector2 tC, List<Vector2> points)
    {
        if (points.Count <= 0) return false;
        foreach (var point in points)
        {
            if (!ContainsTrianglePoint(tA, tB, tC, point)) return false;
        }

        return true;
    }

    public static bool ContainsTriangleSegment(Vector2 tA, Vector2 tB, Vector2 tC, Vector2 segmentStart, Vector2 segmentEnd)
    {
        return ContainsTrianglePoints(tA, tB, tC, segmentStart, segmentEnd);
    }

    public static bool ContainsTriangleCircle(Vector2 tA, Vector2 tB, Vector2 tC, Vector2 circleCenter, float circleRadius)
    {
        if (!ContainsTrianglePoint(tA, tB, tC, circleCenter)) return false;

        var result = SegmentDef.Segment.IntersectSegmentCircle(tA, tB, circleCenter, circleRadius);
        if (result.a.Valid || result.b.Valid) return false;

        result = SegmentDef.Segment.IntersectSegmentCircle(tB, tC, circleCenter, circleRadius);
        if (result.a.Valid || result.b.Valid) return false;

        result = SegmentDef.Segment.IntersectSegmentCircle(tC, tA, circleCenter, circleRadius);
        if (result.a.Valid || result.b.Valid) return false;

        return true;
    }

    public static bool ContainsTriangleTriangle(Vector2 tA1, Vector2 tB1, Vector2 tC1, Vector2 tA2, Vector2 tB2, Vector2 tC2)
    {
        return ContainsTrianglePoints(tA1, tB1, tC1, tA2, tB2, tC2);
    }

    public static bool ContainsTriangleQuad(Vector2 tA, Vector2 tB, Vector2 tC, Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD)
    {
        return ContainsTrianglePoints(tA, tB, tC, qA, qB, qC, qD);
    }

    public static bool ContainsTriangleRect(Vector2 tA, Vector2 tB, Vector2 tC, Vector2 rA, Vector2 rB, Vector2 rC, Vector2 rD)
    {
        return ContainsTrianglePoints(tA, tB, tC, rA, rB, rC, rD);
    }

    public static bool ContainsTrianglePolyline(Vector2 tA, Vector2 tB, Vector2 tC, List<Vector2> polyline)
    {
        return ContainsTrianglePoints(tA, tB, tC, polyline);
    }

    public static bool ContainsTrianglePolygon(Vector2 tA, Vector2 tB, Vector2 tC, List<Vector2> polygon)
    {
        return ContainsTrianglePoints(tA, tB, tC, polygon);
    }

}