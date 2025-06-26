using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.PolylineDef;

public partial class Polyline
{
    public static bool OverlapPolylineSegment(List<Vector2> points, Vector2 segmentStart, Vector2 segmentEnd)
    {
        return SegmentDef.Segment.OverlapSegmentPolyline(segmentStart, segmentEnd, points);
    }

    public static bool OverlapPolylineLine(List<Vector2> points, Vector2 linePoint, Vector2 lineDirection)
    {
        return Line.OverlapLinePolyline(linePoint, lineDirection, points);
    }

    public static bool OverlapPolylineRay(List<Vector2> points, Vector2 rayPoint, Vector2 rayDirection)
    {
        return Ray.OverlapRayPolyline(rayPoint, rayDirection, points);
    }

    public static bool OverlapPolylineCircle(List<Vector2> points, Vector2 circleCenter, float circleRadius)
    {
        return Circle.OverlapCirclePolyline(circleCenter, circleRadius, points);
    }

    public static bool OverlapPolylineTriangle(List<Vector2> points, Vector2 ta, Vector2 tb, Vector2 tc)
    {
        return Triangle.OverlapTrianglePolyline(ta, tb, tc, points);
    }

    public static bool OverlapPolylineQuad(List<Vector2> points, Vector2 qa, Vector2 qb, Vector2 qc, Vector2 qd)
    {
        return Quad.OverlapQuadPolyline(qa, qb, qc, qd, points);
    }

    public static bool OverlapPolylineRect(List<Vector2> points, Vector2 ra, Vector2 rb, Vector2 rc, Vector2 rd)
    {
        return Quad.OverlapQuadPolyline(ra, rb, rc, rd, points);
    }

    public static bool OverlapPolylinePolygon(List<Vector2> points1, List<Vector2> points2)
    {
        return Polygon.OverlapPolygonPolyline(points2, points1);
    }

    public static bool OverlapPolylinePolyline(List<Vector2> points1, List<Vector2> points2)
    {
        if (points1.Count < 2 || points2.Count < 2) return false;

        for (var i = 0; i < points1.Count - 1; i++)
        {
            var start = points1[i];
            var end = points1[i + 1];

            for (var j = 0; j < points2.Count - 1; j++)
            {
                var bStart = points2[j];
                var bEnd = points2[j + 1];

                if (SegmentDef.Segment.OverlapSegmentSegment(start, end, bStart, bEnd)) return true;
            }
        }

        return false;
    }

    public static bool OverlapPolylineSegments(List<Vector2> points, List<SegmentDef.Segment> segments)
    {
        if (points.Count < 2 || segments.Count <= 0) return false;

        for (var i = 0; i < points.Count - 1; i++)
        {
            var start = points[i];
            var end = points[i + 1];

            foreach (var seg in segments)
            {
                if (SegmentDef.Segment.OverlapSegmentSegment(start, end, seg.Start, seg.End)) return true;
            }
        }

        return false;
    }

}