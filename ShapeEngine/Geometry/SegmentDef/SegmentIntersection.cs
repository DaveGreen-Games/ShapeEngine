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
    /// <summary>
    /// Computes the intersection point between this segment and a line.
    /// </summary>
    /// <param name="linePos">A point on the line.</param>
    /// <param name="lineDir">The direction vector of the line.</param>
    /// <returns>A <see cref="CollisionPoint"/> representing the intersection, or an invalid point if there is no intersection.</returns>
    public CollisionPoint IntersectLine(Vector2 linePos, Vector2 lineDir) => IntersectSegmentLine(Start, End, linePos, lineDir);
    /// <summary>
    /// Computes the intersection point between this segment and a ray.
    /// </summary>
    /// <param name="rayPos">The origin of the ray.</param>
    /// <param name="rayDir">The direction vector of the ray.</param>
    /// <returns>A <see cref="CollisionPoint"/> representing the intersection, or an invalid point if there is no intersection.</returns>
    public CollisionPoint IntersectRay(Vector2 rayPos, Vector2 rayDir) => IntersectSegmentRay(Start, End, rayPos, rayDir);
    /// <summary>
    /// Computes the intersection point between this segment and another segment.
    /// </summary>
    /// <param name="segStart">The start point of the other segment.</param>
    /// <param name="segEnd">The end point of the other segment.</param>
    /// <returns>A <see cref="CollisionPoint"/> representing the intersection, or an invalid point if there is no intersection.</returns>
    public CollisionPoint IntersectSegment(Vector2 segStart, Vector2 segEnd) => IntersectSegmentSegment(Start, End, segStart, segEnd);
    /// <summary>
    /// Computes the intersection points between this segment and a circle.
    /// </summary>
    /// <param name="circlePos">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <returns>A tuple of <see cref="CollisionPoint"/> representing the intersection points (a, b). If there is no intersection, both are invalid.</returns>
    public (CollisionPoint a, CollisionPoint b) IntersectCircle(Vector2 circlePos, float circleRadius) =>
        IntersectSegmentCircle(Start, End, circlePos, circleRadius);
    /// <summary>
    /// Computes the intersection points between this segment and a triangle.
    /// </summary>
    /// <param name="a">First vertex of the triangle.</param>
    /// <param name="b">Second vertex of the triangle.</param>
    /// <param name="c">Third vertex of the triangle.</param>
    /// <returns>A tuple of <see cref="CollisionPoint"/> representing the intersection points (a, b).</returns>
    public (CollisionPoint a, CollisionPoint b) IntersectTriangle(Vector2 a, Vector2 b, Vector2 c) => IntersectSegmentTriangle(Start, End, a, b, c);
    /// <summary>
    /// Computes the intersection points between this segment and a quadrilateral.
    /// </summary>
    /// <param name="a">First vertex of the quad.</param>
    /// <param name="b">Second vertex of the quad.</param>
    /// <param name="c">Third vertex of the quad.</param>
    /// <param name="d">Fourth vertex of the quad.</param>
    /// <returns>A tuple of <see cref="CollisionPoint"/> representing the intersection points (a, b).</returns>
    public (CollisionPoint a, CollisionPoint b) IntersectQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => IntersectSegmentQuad(Start, End, a, b, c, d);
    /// <summary>
    /// Computes the intersection points between this segment and a rectangle.
    /// </summary>
    /// <param name="a">First vertex of the rectangle.</param>
    /// <param name="b">Second vertex of the rectangle.</param>
    /// <param name="c">Third vertex of the rectangle.</param>
    /// <param name="d">Fourth vertex of the rectangle.</param>
    /// <returns>A tuple of <see cref="CollisionPoint"/> representing the intersection points (a, b).</returns>
    public (CollisionPoint a, CollisionPoint b) IntersectRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => IntersectSegmentQuad(Start, End, a, b, c, d);
    /// <summary>
    /// Computes all intersection points between this segment and a polygon.
    /// </summary>
    /// <param name="points">The list of polygon vertices.</param>
    /// <param name="maxCollisionPoints">The maximum number of collision points to return. -1 for unlimited.</param>
    /// <returns>A <see cref="CollisionPoints"/> collection, or null if there are no intersections.</returns>
    public CollisionPoints? IntersectPolygon(List<Vector2> points, int maxCollisionPoints = -1) =>
        IntersectSegmentPolygon(Start, End, points, maxCollisionPoints);
    /// <summary>
    /// Computes all intersection points between this segment and a polyline.
    /// </summary>
    /// <param name="points">The list of polyline vertices.</param>
    /// <param name="maxCollisionPoints">The maximum number of collision points to return. -1 for unlimited.</param>
    /// <returns>A <see cref="CollisionPoints"/> collection, or null if there are no intersections.</returns>
    public CollisionPoints? IntersectPolyline(List<Vector2> points, int maxCollisionPoints = -1) =>
        IntersectSegmentPolyline(Start, End, points, maxCollisionPoints);
    /// <summary>
    /// Computes all intersection points between this segment and a list of segments.
    /// </summary>
    /// <param name="segments">The list of segments to test against.</param>
    /// <param name="maxCollisionPoints">The maximum number of collision points to return. -1 for unlimited.</param>
    /// <returns>A <see cref="CollisionPoints"/> collection, or null if there are no intersections.</returns>
    public CollisionPoints? IntersectSegments(List<Segment> segments, int maxCollisionPoints = -1) =>
        IntersectSegmentSegments(Start, End, segments, maxCollisionPoints);
    /// <summary>
    /// Computes the intersection point between this segment and a line.
    /// </summary>
    /// <param name="line">The line to test against.</param>
    /// <returns>A <see cref="CollisionPoint"/> representing the intersection, or an invalid point if there is no intersection.</returns>
    public CollisionPoint IntersectLine(Line line) => IntersectSegmentLine(Start, End, line.Point, line.Direction);
    /// <summary>
    /// Computes the intersection point between this segment and a ray.
    /// </summary>
    /// <param name="ray">The ray to test against.</param>
    /// <returns>A <see cref="CollisionPoint"/> representing the intersection, or an invalid point if there is no intersection.</returns>
    public CollisionPoint IntersectRay(Ray ray) => IntersectSegmentRay(Start, End, ray.Point, ray.Direction);
    /// <summary>
    /// Computes the intersection point between this segment and another segment.
    /// </summary>
    /// <param name="segment">The segment to test against.</param>
    /// <returns>A <see cref="CollisionPoint"/> representing the intersection, or an invalid point if there is no intersection.</returns>
    public CollisionPoint IntersectSegment(Segment segment) => IntersectSegmentSegment(Start, End, segment.Start, segment.End);
    /// <summary>
    /// Computes the intersection points between this segment and a circle.
    /// </summary>
    /// <param name="circle">The circle to test against.</param>
    /// <returns>A tuple of <see cref="CollisionPoint"/> representing the intersection points (a, b).</returns>
    public (CollisionPoint a, CollisionPoint b) IntersectCircle(Circle circle) => IntersectSegmentCircle(Start, End, circle.Center, circle.Radius);
    /// <summary>
    /// Computes the intersection points between this segment and a triangle.
    /// </summary>
    /// <param name="triangle">The triangle to test against.</param>
    /// <returns>A tuple of <see cref="CollisionPoint"/> representing the intersection points (a, b).</returns>
    public (CollisionPoint a, CollisionPoint b) IntersectTriangle(Triangle triangle) =>
        IntersectSegmentTriangle(Start, End, triangle.A, triangle.B, triangle.C);
    /// <summary>
    /// Computes the intersection points between this segment and a quadrilateral.
    /// </summary>
    /// <param name="quad">The quadrilateral to test against.</param>
    /// <returns>A tuple of <see cref="CollisionPoint"/> representing the intersection points (a, b).</returns>
    public (CollisionPoint a, CollisionPoint b) IntersectQuad(Quad quad) => IntersectSegmentQuad(Start, End, quad.A, quad.B, quad.C, quad.D);
    /// <summary>
    /// Computes the intersection points between this segment and a rectangle.
    /// </summary>
    /// <param name="rect">The rectangle to test against.</param>
    /// <returns>A tuple of <see cref="CollisionPoint"/> representing the intersection points (a, b).</returns>
    public (CollisionPoint a, CollisionPoint b) IntersectRect(Rect rect) => IntersectSegmentQuad(Start, End, rect.A, rect.B, rect.C, rect.D);
    /// <summary>
    /// Computes all intersection points between this segment and a polygon.
    /// </summary>
    /// <param name="polygon">The polygon to test against.</param>
    /// <param name="maxCollisionPoints">The maximum number of collision points to return. -1 for unlimited.</param>
    /// <returns>A <see cref="CollisionPoints"/> collection, or null if there are no intersections.</returns>
    public CollisionPoints? IntersectPolygon(Polygon polygon, int maxCollisionPoints = -1) =>
        IntersectSegmentPolygon(Start, End, polygon, maxCollisionPoints);
    /// <summary>
    /// Computes all intersection points between this segment and a polyline.
    /// </summary>
    /// <param name="polyline">The polyline to test against.</param>
    /// <param name="maxCollisionPoints">The maximum number of collision points to return. -1 for unlimited.</param>
    /// <returns>A <see cref="CollisionPoints"/> collection, or null if there are no intersections.</returns>
    public CollisionPoints? IntersectPolyline(Polyline polyline, int maxCollisionPoints = -1) =>
        IntersectSegmentPolyline(Start, End, polyline, maxCollisionPoints);
    /// <summary>
    /// Computes all intersection points between this segment and a set of segments.
    /// </summary>
    /// <param name="segments">The set of segments to test against.</param>
    /// <param name="maxCollisionPoints">The maximum number of collision points to return. -1 for unlimited.</param>
    /// <returns>A <see cref="CollisionPoints"/> collection, or null if there are no intersections.</returns>
    public CollisionPoints? IntersectSegments(Segments segments, int maxCollisionPoints = -1) =>
        IntersectSegmentSegments(Start, End, segments, maxCollisionPoints);

}