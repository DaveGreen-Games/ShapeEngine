using System.Numerics;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Geometry.Segment;

namespace ShapeEngine.Geometry.Ray;

public readonly partial struct Ray
{
    public bool OverlapPoint(Vector2 p) => IsPointOnRay(Point, Direction, p);
    public bool OverlapSegment(Vector2 segmentStart, Vector2 segmentEnd) => OverlapRaySegment(Point, Direction, segmentStart, segmentEnd);
    public bool OverlapLine(Vector2 linePoint, Vector2 lineDirection) => OverlapRayLine(Point, Direction, linePoint, lineDirection);
    public bool OverlapRay(Vector2 rayPoint, Vector2 rayDirection) => OverlapRayRay(Point, Direction, rayPoint, rayDirection);
    public bool OverlapCircle(Vector2 circleCenter, float circleRadius) => OverlapRayCircle(Point, Direction, circleCenter, circleRadius);
    public bool OverlapTriangle(Vector2 a, Vector2 b, Vector2 c) => OverlapRayTriangle(Point, Direction, a, b, c);
    public bool OverlapQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapRayQuad(Point, Direction, a, b, c, d);
    public bool OverlapRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapRayQuad(Point, Direction, a, b, c, d);
    public bool OverlapPolygon(List<Vector2> points) => OverlapRayPolygon(Point, Direction, points);
    public bool OverlapPolyline(List<Vector2> points) => OverlapRayPolyline(Point, Direction, points);
    public bool OverlapSegments(List<Segment.Segment> segments) => OverlapRaySegments(Point, Direction, segments);

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
            case ShapeType.Ray:
                var rayShape = collider.GetRayShape();
                return OverlapShape(rayShape);
            case ShapeType.Line:
                var l = collider.GetLineShape();
                return OverlapShape(l);
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

    public bool OverlapShape(Segment.Segment segment) => OverlapRaySegment(Point, Direction, segment.Start, segment.End);
    public bool OverlapShape(Line.Line line) => OverlapRayLine(Point, Direction, line.Point, line.Direction);
    public bool OverlapShape(Ray ray) => OverlapRayRay(Point, Direction, ray.Point, ray.Direction);
    public bool OverlapShape(Circle.Circle circle) => OverlapRayCircle(Point, Direction, circle.Center, circle.Radius);
    public bool OverlapShape(Triangle.Triangle t) => OverlapRayTriangle(Point, Direction, t.A, t.B, t.C);
    public bool OverlapShape(Quad.Quad q) => OverlapRayQuad(Point, Direction, q.A, q.B, q.C, q.D);
    public bool OverlapShape(Rect.Rect r) => OverlapRayQuad(Point, Direction, r.A, r.B, r.C, r.D);
    public bool OverlapShape(Polygon.Polygon p) => OverlapRayPolygon(Point, Direction, p);
    public bool OverlapShape(Polyline.Polyline pl) => OverlapRayPolyline(Point, Direction, pl);
    public bool OverlapShape(Segments segments) => OverlapRaySegments(Point, Direction, segments);

}