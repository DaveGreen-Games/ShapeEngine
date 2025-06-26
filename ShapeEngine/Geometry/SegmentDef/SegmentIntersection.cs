using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.SegmentDef;

public readonly partial struct Segment
{
    public CollisionPoint IntersectLine(Vector2 linePos, Vector2 lineDir) => IntersectSegmentLine(Start, End, linePos, lineDir);
    public CollisionPoint IntersectRay(Vector2 rayPos, Vector2 rayDir) => IntersectSegmentRay(Start, End, rayPos, rayDir);
    public CollisionPoint IntersectSegment(Vector2 segStart, Vector2 segEnd) => IntersectSegmentSegment(Start, End, segStart, segEnd);

    public (CollisionPoint a, CollisionPoint b) IntersectCircle(Vector2 circlePos, float circleRadius) =>
        IntersectSegmentCircle(Start, End, circlePos, circleRadius);

    public (CollisionPoint a, CollisionPoint b) IntersectTriangle(Vector2 a, Vector2 b, Vector2 c) => IntersectSegmentTriangle(Start, End, a, b, c);
    public (CollisionPoint a, CollisionPoint b) IntersectQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => IntersectSegmentQuad(Start, End, a, b, c, d);
    public (CollisionPoint a, CollisionPoint b) IntersectRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => IntersectSegmentQuad(Start, End, a, b, c, d);

    public CollisionPoints? IntersectPolygon(List<Vector2> points, int maxCollisionPoints = -1) =>
        IntersectSegmentPolygon(Start, End, points, maxCollisionPoints);

    public CollisionPoints? IntersectPolyline(List<Vector2> points, int maxCollisionPoints = -1) =>
        IntersectSegmentPolyline(Start, End, points, maxCollisionPoints);

    public CollisionPoints? IntersectSegments(List<Segment> segments, int maxCollisionPoints = -1) =>
        IntersectSegmentSegments(Start, End, segments, maxCollisionPoints);

    public CollisionPoint IntersectLine(Line line) => IntersectSegmentLine(Start, End, line.Point, line.Direction);
    public CollisionPoint IntersectRay(Ray ray) => IntersectSegmentRay(Start, End, ray.Point, ray.Direction);
    public CollisionPoint IntersectSegment(Segment segment) => IntersectSegmentSegment(Start, End, segment.Start, segment.End);
    public (CollisionPoint a, CollisionPoint b) IntersectCircle(Circle circle) => IntersectSegmentCircle(Start, End, circle.Center, circle.Radius);

    public (CollisionPoint a, CollisionPoint b) IntersectTriangle(Triangle triangle) =>
        IntersectSegmentTriangle(Start, End, triangle.A, triangle.B, triangle.C);

    public (CollisionPoint a, CollisionPoint b) IntersectQuad(Quad quad) => IntersectSegmentQuad(Start, End, quad.A, quad.B, quad.C, quad.D);
    public (CollisionPoint a, CollisionPoint b) IntersectRect(Rect rect) => IntersectSegmentQuad(Start, End, rect.A, rect.B, rect.C, rect.D);

    public CollisionPoints? IntersectPolygon(Polygon polygon, int maxCollisionPoints = -1) =>
        IntersectSegmentPolygon(Start, End, polygon, maxCollisionPoints);

    public CollisionPoints? IntersectPolyline(Polyline polyline, int maxCollisionPoints = -1) =>
        IntersectSegmentPolyline(Start, End, polyline, maxCollisionPoints);

    public CollisionPoints? IntersectSegments(Segments segments, int maxCollisionPoints = -1) =>
        IntersectSegmentSegments(Start, End, segments, maxCollisionPoints);

}