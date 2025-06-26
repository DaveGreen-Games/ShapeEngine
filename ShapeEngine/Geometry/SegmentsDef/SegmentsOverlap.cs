using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.SegmentsDef;

public partial class Segments
{
    public bool OverlapSegment(Vector2 segmentStart, Vector2 segmentEnd) => OverlapSegmentsSegment(this, segmentStart, segmentEnd);
    public bool OverlapLine(Vector2 linePoint, Vector2 lineDirection) => OverlapSegmentsLine(this, linePoint, lineDirection);
    public bool OverlapRay(Vector2 rayPoint, Vector2 rayDirection) => OverlapSegmentsRay(this, rayPoint, rayDirection);
    public bool OverlapCircle(Vector2 circleCenter, float circleRadius) => OverlapSegmentsCircle(this, circleCenter, circleRadius);
    public bool OverlapTriangle(Vector2 a, Vector2 b, Vector2 c) => OverlapSegmentsTriangle(this, a, b, c);
    public bool OverlapQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapSegmentsQuad(this, a, b, c, d);
    public bool OverlapRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapSegmentsQuad(this, a, b, c, d);
    public bool OverlapPolygon(List<Vector2> points) => OverlapSegmentsPolygon(this, points);
    public bool OverlapPolyline(List<Vector2> points) => OverlapSegmentsPolyline(this, points);
    public bool OverlapSegments(List<Segment> segments) => OverlapSegmentsSegments(this, segments);
    public bool OverlapShape(Line line) => OverlapSegmentsLine(this, line.Point, line.Direction);
    public bool OverlapShape(Ray ray) => OverlapSegmentsRay(this, ray.Point, ray.Direction);

    public bool OverlapShape(Segments b)
    {
        foreach (var seg in this)
        {
            foreach (var bSeg in b)
            {
                if (Segment.OverlapSegmentSegment(seg.Start, seg.End, bSeg.Start, bSeg.End)) return true;
            }
        }

        return false;
    }

    public bool OverlapShape(Segment s)
    {
        foreach (var seg in this)
        {
            if (Segment.OverlapSegmentSegment(seg.Start, seg.End, s.Start, s.End)) return true;
        }

        return false;
    }

    public bool OverlapShape(Circle c)
    {
        foreach (var seg in this)
        {
            if (Segment.OverlapSegmentCircle(seg.Start, seg.End, c.Center, c.Radius)) return true;
        }

        return false;
    }

    public bool OverlapShape(Triangle t) => t.OverlapShape(this);
    public bool OverlapShape(Quad q) => q.OverlapShape(this);
    public bool OverlapShape(Rect r) => r.OverlapShape(this);
    public bool OverlapShape(Polyline pl) => pl.OverlapShape(this);
    public bool OverlapShape(Polygon p) => p.OverlapShape(this);

}