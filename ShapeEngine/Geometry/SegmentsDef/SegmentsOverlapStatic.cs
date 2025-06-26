using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.SegmentsDef;

public partial class Segments
{
    public static bool OverlapSegmentsSegment(List<Segment> segments, Vector2 segmentStart, Vector2 segmentEnd)
    {
        return Segment.OverlapSegmentSegments(segmentStart, segmentEnd, segments);
    }

    public static bool OverlapSegmentsLine(List<Segment> segments, Vector2 linePoint, Vector2 lineDirection)
    {
        return Line.OverlapLineSegments(linePoint, lineDirection, segments);
    }

    public static bool OverlapSegmentsRay(List<Segment> segments, Vector2 rayPoint, Vector2 rayDirection)
    {
        return Ray.OverlapRaySegments(rayPoint, rayDirection, segments);
    }

    public static bool OverlapSegmentsCircle(List<Segment> segments, Vector2 circleCenter, float circleRadius)
    {
        return Circle.OverlapCircleSegments(circleCenter, circleRadius, segments);
    }

    public static bool OverlapSegmentsTriangle(List<Segment> segments, Vector2 ta, Vector2 tb, Vector2 tc)
    {
        return Triangle.OverlapTriangleSegments(ta, tb, tc, segments);
    }

    public static bool OverlapSegmentsQuad(List<Segment> segments, Vector2 qa, Vector2 qb, Vector2 qc, Vector2 qd)
    {
        return Quad.OverlapQuadSegments(qa, qb, qc, qd, segments);
    }

    public static bool OverlapSegmentsRect(List<Segment> segments, Vector2 ra, Vector2 rb, Vector2 rc, Vector2 rd)
    {
        return Quad.OverlapQuadSegments(ra, rb, rc, rd, segments);
    }

    public static bool OverlapSegmentsPolygon(List<Segment> segments, List<Vector2> points)
    {
        return Polygon.OverlapPolygonSegments(points, segments);
    }

    public static bool OverlapSegmentsPolyline(List<Segment> segments, List<Vector2> points)
    {
        return Polyline.OverlapPolylineSegments(points, segments);
    }

    public static bool OverlapSegmentsSegments(List<Segment> segments1, List<Segment> segments2)
    {
        foreach (var seg in segments1)
        {
            foreach (var bSeg in segments2)
            {
                if (Segment.OverlapSegmentSegment(seg.Start, seg.End, bSeg.Start, bSeg.End)) return true;
            }
        }

        return false;
    }

}