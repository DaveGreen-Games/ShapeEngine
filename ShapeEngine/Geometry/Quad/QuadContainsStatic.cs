using System.Numerics;

namespace ShapeEngine.Geometry.Quad;

public readonly partial struct Quad
{
    public static bool ContainsQuadPoint(Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD, Vector2 point)
    {
        var oddNodes = false;

        if (Polygon.Polygon.ContainsPointCheck(qD, qA, point)) oddNodes = !oddNodes;
        if (Polygon.Polygon.ContainsPointCheck(qA, qB, point)) oddNodes = !oddNodes;
        if (Polygon.Polygon.ContainsPointCheck(qB, qC, point)) oddNodes = !oddNodes;
        if (Polygon.Polygon.ContainsPointCheck(qC, qD, point)) oddNodes = !oddNodes;

        return oddNodes;
    }

    public static bool ContainsQuadPoints(Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD, Vector2 u, Vector2 v)
    {
        return ContainsQuadPoint(qA, qB, qC, qD, u) &&
               ContainsQuadPoint(qA, qB, qC, qD, v);
    }

    public static bool ContainsQuadPoints(Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD, Vector2 u, Vector2 v, Vector2 w)
    {
        return ContainsQuadPoint(qA, qB, qC, qD, u) &&
               ContainsQuadPoint(qA, qB, qC, qD, v) &&
               ContainsQuadPoint(qA, qB, qC, qD, w);
    }

    public static bool ContainsQuadPoints(Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD, Vector2 u, Vector2 v, Vector2 w, Vector2 x)
    {
        return ContainsQuadPoint(qA, qB, qC, qD, u) &&
               ContainsQuadPoint(qA, qB, qC, qD, v) &&
               ContainsQuadPoint(qA, qB, qC, qD, w) &&
               ContainsQuadPoint(qA, qB, qC, qD, x);
    }

    public static bool ContainsQuadPoints(Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD, List<Vector2> points)
    {
        if (points.Count <= 0) return false;

        foreach (var point in points)
        {
            if (!ContainsQuadPoint(qA, qB, qC, qD, point)) return false;
        }

        return true;
    }

    public static bool ContainsQuadSegment(Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD, Vector2 segmentStart, Vector2 segmentEnd)
    {
        return ContainsQuadPoints(qA, qB, qC, qD, segmentStart, segmentEnd);
    }

    public static bool ContainsQuadCircle(Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD, Vector2 circleCenter, float circleRadius)
    {
        if (!ContainsQuadPoint(qA, qB, qC, qD, circleCenter)) return false;

        var result = Segment.Segment.IntersectSegmentCircle(qA, qB, circleCenter, circleRadius);
        if (result.a.Valid || result.b.Valid) return false;

        result = Segment.Segment.IntersectSegmentCircle(qB, qC, circleCenter, circleRadius);
        if (result.a.Valid || result.b.Valid) return false;

        result = Segment.Segment.IntersectSegmentCircle(qC, qD, circleCenter, circleRadius);
        if (result.a.Valid || result.b.Valid) return false;

        result = Segment.Segment.IntersectSegmentCircle(qD, qA, circleCenter, circleRadius);
        if (result.a.Valid || result.b.Valid) return false;
        return true;
    }

    public static bool ContainsQuadTriangle(Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD, Vector2 tA, Vector2 tB, Vector2 tc)
    {
        return ContainsQuadPoints(qA, qB, qC, qD, tA, tB, tc);
    }

    public static bool ContainsQuadQuad(Vector2 qA1, Vector2 qB1, Vector2 qC1, Vector2 qD1, Vector2 qA2, Vector2 qB2, Vector2 qC2, Vector2 qD2)
    {
        return ContainsQuadPoints(qA1, qB1, qC1, qD1, qA2, qB2, qC2, qD2);
    }

    public static bool ContainsQuadRect(Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD, Vector2 rA, Vector2 rB, Vector2 rC, Vector2 rD)
    {
        return ContainsQuadPoints(qA, qB, qC, qD, rA, rB, rC, rD);
    }

    public static bool ContainsQuadPolyline(Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD, List<Vector2> polyline)
    {
        return ContainsQuadPoints(qA, qB, qC, qD, polyline);
    }

    public static bool ContainsQuadPolygon(Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD, List<Vector2> polygon)
    {
        return ContainsQuadPoints(qA, qB, qC, qD, polygon);
    }

}