using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.PolygonDef;

public partial class Polygon
{
    public bool OverlapSegment(Vector2 segmentStart, Vector2 segmentEnd) => OverlapPolygonSegment(this, segmentStart, segmentEnd);
    
    public bool OverlapLine(Vector2 linePoint, Vector2 lineDirection) => OverlapPolygonLine(this, linePoint, lineDirection);
    
    public bool OverlapRay(Vector2 rayPoint, Vector2 rayDirection) => OverlapPolygonRay(this, rayPoint, rayDirection);
    
    public bool OverlapCircle(Vector2 circleCenter, float circleRadius) => OverlapPolygonCircle(this, circleCenter, circleRadius);
    
    public bool OverlapTriangle(Vector2 a, Vector2 b, Vector2 c) => OverlapPolygonTriangle(this, a, b, c);
    
    public bool OverlapQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapPolygonQuad(this, a, b, c, d);
    
    public bool OverlapRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapPolygonQuad(this, a, b, c, d);
    
    public bool OverlapPolygon(List<Vector2> points) => OverlapPolygonPolygon(this, points);
    
    public bool OverlapPolyline(List<Vector2> points) => OverlapPolygonPolyline(this, points);
    
    public bool OverlapSegments(List<SegmentDef.Segment> segments) => OverlapPolygonSegments(this, segments);
    
    public bool OverlapShape(Line line) => OverlapPolygonLine(this, line.Point, line.Direction);
    
    public bool OverlapShape(Ray ray) => OverlapPolygonRay(this, ray.Point, ray.Direction);
    
    public bool Overlap(Collider collider)
    {
        if (!collider.Enabled) return false;

        switch (collider.GetShapeType())
        {
            case ShapeType.Circle:
                var c = collider.GetCircleShape();
                return OverlapShape(c);
            case ShapeType.Segment:
                var s = collider.GetSegmentShape();
                return OverlapShape(s);
            case ShapeType.Line:
                var l = collider.GetLineShape();
                return OverlapShape(l);
            case ShapeType.Ray:
                var rayShape = collider.GetRayShape();
                return OverlapShape(rayShape);
            case ShapeType.Triangle:
                var t = collider.GetTriangleShape();
                return OverlapShape(t);
            case ShapeType.Rect:
                var r = collider.GetRectShape();
                return OverlapShape(r);
            case ShapeType.Quad:
                var q = collider.GetQuadShape();
                return OverlapShape(q);
            case ShapeType.Poly:
                var p = collider.GetPolygonShape();
                return OverlapShape(p);
            case ShapeType.PolyLine:
                var pl = collider.GetPolylineShape();
                return OverlapShape(pl);
        }

        return false;
    }

    public bool OverlapShape(SegmentDef.Segment s) => s.OverlapShape(this);
    
    public bool OverlapShape(Circle c) => c.OverlapShape(this);
    
    public bool OverlapShape(Triangle t) => t.OverlapShape(this);
    
    public bool OverlapShape(Rect r) => r.OverlapShape(this);
    
    public bool OverlapShape(Quad q) => q.OverlapShape(this);

    public bool OverlapShape(Polygon b)
    {
        if (Count < 3 || b.Count < 3) return false;

        var oddNodesThis = false;
        var oddNodesB = false;
        var containsPointBCheckFinished = false;

        var pointToCeckThis = this[0];
        var pointToCeckB = b[0];

        for (var i = 0; i < Count; i++)
        {
            var start = this[i];
            var end = this[(i + 1) % Count];

            for (int j = 0; j < b.Count; j++)
            {
                var bStart = b[j];
                var bEnd = b[(j + 1) % b.Count];
                if (SegmentDef.Segment.OverlapSegmentSegment(start, end, bStart, bEnd)) return true;

                if (containsPointBCheckFinished) continue;
                if (Polygon.ContainsPointCheck(bStart, bEnd, pointToCeckThis)) oddNodesB = !oddNodesB;
            }

            if (!containsPointBCheckFinished)
            {
                if (oddNodesB) return true;
                containsPointBCheckFinished = true;
            }

            if (Polygon.ContainsPointCheck(start, end, pointToCeckB)) oddNodesThis = !oddNodesThis;
        }

        return oddNodesThis || oddNodesB;
    }

    public bool OverlapShape(Polyline pl)
    {
        if (Count < 3 || pl.Count < 2) return false;

        var oddNodes = false;
        var pointToCeck = pl[0];


        for (var i = 0; i < Count; i++)
        {
            var start = this[i];
            var end = this[(i + 1) % Count];

            for (var j = 0; j < pl.Count - 1; j++)
            {
                var bStart = pl[j];
                var bEnd = pl[j + 1];
                if (SegmentDef.Segment.OverlapSegmentSegment(start, end, bStart, bEnd)) return true;
            }

            if (Polygon.ContainsPointCheck(start, end, pointToCeck)) oddNodes = !oddNodes;
        }

        return oddNodes;
    }

    public bool OverlapShape(Segments segments)
    {
        if (Count < 3 || segments.Count <= 0) return false;

        var oddNodes = false;
        var pointToCeck = segments[0].Start;


        for (var i = 0; i < Count; i++)
        {
            var start = this[i];
            var end = this[(i + 1) % Count];

            foreach (var seg in segments)
            {
                if (SegmentDef.Segment.OverlapSegmentSegment(start, end, seg.Start, seg.End)) return true;
            }

            if (Polygon.ContainsPointCheck(start, end, pointToCeck)) oddNodes = !oddNodes;
        }

        return oddNodes;
    }

}