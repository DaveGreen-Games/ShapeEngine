using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.PolygonDef;

public partial class Polygon
{
    public static bool OverlapPolygonSegment(List<Vector2> points, Vector2 segmentStart, Vector2 segmentEnd)
    {
        return Segment.OverlapSegmentPolygon(segmentStart, segmentEnd, points);
    }

    public static bool OverlapPolygonLine(List<Vector2> points, Vector2 linePoint, Vector2 lineDirection)
    {
        return Line.OverlapLinePolygon(linePoint, lineDirection, points);
    }

    public static bool OverlapPolygonRay(List<Vector2> points, Vector2 rayPoint, Vector2 rayDirection)
    {
        return Ray.OverlapRayPolygon(rayPoint, rayDirection, points);
    }

    public static bool OverlapPolygonCircle(List<Vector2> points, Vector2 circleCenter, float circleRadius)
    {
        return Circle.OverlapCirclePolygon(circleCenter, circleRadius, points);
    }

    public static bool OverlapPolygonTriangle(List<Vector2> points, Vector2 ta, Vector2 tb, Vector2 tc)
    {
        return Triangle.OverlapTrianglePolygon(ta, tb, tc, points);
    }

    public static bool OverlapPolygonQuad(List<Vector2> points, Vector2 qa, Vector2 qb, Vector2 qc, Vector2 qd)
    {
        return Quad.OverlapQuadPolygon(qa, qb, qc, qd, points);
    }

    public static bool OverlapPolygonRect(List<Vector2> points, Vector2 ra, Vector2 rb, Vector2 rc, Vector2 rd)
    {
        return Quad.OverlapQuadPolygon(ra, rb, rc, rd, points);
    }

    public static bool OverlapPolygonPolygon(List<Vector2> points1, List<Vector2> points2)
    {
        if (points1.Count < 3 || points2.Count < 3) return false;

        var oddNodes1 = false;
        var oddNodes2 = false;
        var containsPoints2CheckFinished = false;

        var pointToCeck1 = points1[0];
        var pointToCeck2 = points2[0];

        for (var i = 0; i < points1.Count; i++)
        {
            var start1 = points1[i];
            var end1 = points1[(i + 1) % points1.Count];

            for (var j = 0; j < points2.Count; j++)
            {
                var start2 = points2[j];
                var end2 = points2[(j + 1) % points2.Count];
                if (Segment.OverlapSegmentSegment(start1, end1, start2, end2)) return true;

                if (containsPoints2CheckFinished) continue;
                if (Polygon.ContainsPointCheck(start2, end2, pointToCeck1)) oddNodes2 = !oddNodes2;
            }

            if (!containsPoints2CheckFinished)
            {
                if (oddNodes2) return true;
                containsPoints2CheckFinished = true;
            }

            if (Polygon.ContainsPointCheck(start1, end1, pointToCeck2)) oddNodes1 = !oddNodes1;
        }

        return oddNodes1 || oddNodes2;
    }

    public static bool OverlapPolygonPolyline(List<Vector2> points1, List<Vector2> points2)
    {
        if (points1.Count < 3 || points2.Count < 2) return false;

        var oddNodes = false;
        var pointToCeck = points2[0];


        for (var i = 0; i < points1.Count; i++)
        {
            var start1 = points1[i];
            var end1 = points1[(i + 1) % points1.Count];

            for (var j = 0; j < points2.Count - 1; j++)
            {
                var start2 = points2[j];
                var end2 = points2[j + 1];
                if (Segment.OverlapSegmentSegment(start1, end1, start2, end2)) return true;
            }

            if (Polygon.ContainsPointCheck(start1, end1, pointToCeck)) oddNodes = !oddNodes;
        }

        return oddNodes;
    }

    public static bool OverlapPolygonSegments(List<Vector2> points, List<Segment> segments)
    {
        if (points.Count < 3 || segments.Count <= 0) return false;

        var oddNodes = false;
        var pointToCeck = segments[0].Start;


        for (var i = 0; i < points.Count; i++)
        {
            var start = points[i];
            var end = points[(i + 1) % points.Count];

            foreach (var seg in segments)
            {
                if (Segment.OverlapSegmentSegment(start, end, seg.Start, seg.End)) return true;
            }

            if (Polygon.ContainsPointCheck(start, end, pointToCeck)) oddNodes = !oddNodes;
        }

        return oddNodes;
    }

}