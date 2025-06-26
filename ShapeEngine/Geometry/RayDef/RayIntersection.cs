using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.RayDef;

public readonly partial struct Ray
{
    public CollisionPoint IntersectSegment(Vector2 segmentStart, Vector2 segmentEnd) => IntersectRaySegment(Point, Direction, segmentStart, segmentEnd);
    public CollisionPoint IntersectSegment(SegmentDef.Segment segment) => IntersectRaySegment(Point, Direction, segment.Start, segment.End, segment.Normal);
    public CollisionPoint IntersectLine(Vector2 linePoint, Vector2 lineDirection) => IntersectRayLine(Point, Direction, linePoint, lineDirection);
    public CollisionPoint IntersectLine(Line line) => IntersectRayLine(Point, Direction, line.Point, line.Direction, line.Normal);
    public CollisionPoint IntersectRay(Vector2 rayPoint, Vector2 rayDirection) => IntersectRayRay(Point, Direction, rayPoint, rayDirection);
    public CollisionPoint IntersectRay(Ray ray) => IntersectRayRay(Point, Direction, ray.Point, ray.Direction, ray.Normal);

    public (CollisionPoint a, CollisionPoint b) IntersectCircle(Vector2 circleCenter, float circleRadius) =>
        IntersectRayCircle(Point, Direction, circleCenter, circleRadius);

    public (CollisionPoint a, CollisionPoint b) IntersectCircle(Circle circle) => IntersectRayCircle(Point, Direction, circle.Center, circle.Radius);
    public (CollisionPoint a, CollisionPoint b) IntersectTriangle(Vector2 a, Vector2 b, Vector2 c) => IntersectRayTriangle(Point, Direction, a, b, c);

    public (CollisionPoint a, CollisionPoint b) IntersectTriangle(Triangle triangle) =>
        IntersectRayTriangle(Point, Direction, triangle.A, triangle.B, triangle.C);

    public (CollisionPoint a, CollisionPoint b) IntersectQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => IntersectRayQuad(Point, Direction, a, b, c, d);
    public (CollisionPoint a, CollisionPoint b) IntersectQuad(Quad quad) => IntersectRayQuad(Point, Direction, quad.A, quad.B, quad.C, quad.D);
    public (CollisionPoint a, CollisionPoint b) IntersectRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => IntersectRayQuad(Point, Direction, a, b, c, d);
    public (CollisionPoint a, CollisionPoint b) IntersectRect(Rect rect) => IntersectRayQuad(Point, Direction, rect.A, rect.B, rect.C, rect.D);

    public CollisionPoints? IntersectPolygon(List<Vector2> points, int maxCollisionPoints = -1) =>
        IntersectRayPolygon(Point, Direction, points, maxCollisionPoints);

    public CollisionPoints? IntersectPolygon(Polygon polygon, int maxCollisionPoints = -1) =>
        IntersectRayPolygon(Point, Direction, polygon, maxCollisionPoints);

    public CollisionPoints? IntersectPolyline(List<Vector2> points, int maxCollisionPoints = -1) =>
        IntersectRayPolyline(Point, Direction, points, maxCollisionPoints);

    public CollisionPoints? IntersectPolyline(Polyline polyline, int maxCollisionPoints = -1) =>
        IntersectRayPolyline(Point, Direction, polyline, maxCollisionPoints);

    public CollisionPoints? IntersectSegments(List<SegmentDef.Segment> segments, int maxCollisionPoints = -1) =>
        IntersectRaySegments(Point, Direction, segments, maxCollisionPoints);

    public CollisionPoints? IntersectSegments(Segments segments, int maxCollisionPoints = -1) =>
        IntersectRaySegments(Point, Direction, segments, maxCollisionPoints);

}