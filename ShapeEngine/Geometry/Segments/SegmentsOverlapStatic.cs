using System.Numerics;

namespace ShapeEngine.Geometry.Segments;

public partial class Segments
{
    #region Overlap

    public static bool OverlapSegmentsSegment(List<Segment.Segment> segments, Vector2 segmentStart, Vector2 segmentEnd)
    {
        return Segment.Segment.OverlapSegmentSegments(segmentStart, segmentEnd, segments);
    }

    public static bool OverlapSegmentsLine(List<Segment.Segment> segments, Vector2 linePoint, Vector2 lineDirection)
    {
        return Line.Line.OverlapLineSegments(linePoint, lineDirection, segments);
    }

    public static bool OverlapSegmentsRay(List<Segment.Segment> segments, Vector2 rayPoint, Vector2 rayDirection)
    {
        return Ray.Ray.OverlapRaySegments(rayPoint, rayDirection, segments);
    }

    public static bool OverlapSegmentsCircle(List<Segment.Segment> segments, Vector2 circleCenter, float circleRadius)
    {
        return Circle.Circle.OverlapCircleSegments(circleCenter, circleRadius, segments);
    }

    public static bool OverlapSegmentsTriangle(List<Segment.Segment> segments, Vector2 ta, Vector2 tb, Vector2 tc)
    {
        return Triangle.Triangle.OverlapTriangleSegments(ta, tb, tc, segments);
    }

    public static bool OverlapSegmentsQuad(List<Segment.Segment> segments, Vector2 qa, Vector2 qb, Vector2 qc, Vector2 qd)
    {
        return Quad.Quad.OverlapQuadSegments(qa, qb, qc, qd, segments);
    }

    public static bool OverlapSegmentsRect(List<Segment.Segment> segments, Vector2 ra, Vector2 rb, Vector2 rc, Vector2 rd)
    {
        return Quad.Quad.OverlapQuadSegments(ra, rb, rc, rd, segments);
    }

    public static bool OverlapSegmentsPolygon(List<Segment.Segment> segments, List<Vector2> points)
    {
        return Polygon.Polygon.OverlapPolygonSegments(points, segments);
    }

    public static bool OverlapSegmentsPolyline(List<Segment.Segment> segments, List<Vector2> points)
    {
        return Polyline.Polyline.OverlapPolylineSegments(points, segments);
    }

    public static bool OverlapSegmentsSegments(List<Segment.Segment> segments1, List<Segment.Segment> segments2)
    {
        foreach (var seg in segments1)
        {
            foreach (var bSeg in segments2)
            {
                if (Segment.Segment.OverlapSegmentSegment(seg.Start, seg.End, bSeg.Start, bSeg.End)) return true;
            }
        }

        return false;
    }

    #endregion
}