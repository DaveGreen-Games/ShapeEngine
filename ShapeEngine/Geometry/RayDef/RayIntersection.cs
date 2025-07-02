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
    /// <summary>
    /// Computes the intersection point between this ray and a segment defined by two points.
    /// </summary>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <returns>A <see cref="CollisionPoint"/> representing the intersection, or an invalid point if there is no intersection.</returns>
    /// <remarks>
    /// Returns an invalid <see cref="CollisionPoint"/> if there is no intersection.
    /// </remarks>
    public CollisionPoint IntersectSegment(Vector2 segmentStart, Vector2 segmentEnd) => IntersectRaySegment(Point, Direction, segmentStart, segmentEnd);
    /// <summary>
    /// Computes the intersection point between this ray and a segment.
    /// </summary>
    /// <param name="segment">The segment to test for intersection.</param>
    /// <returns>A <see cref="CollisionPoint"/> representing the intersection, or an invalid point if there is no intersection.</returns>
    public CollisionPoint IntersectSegment(Segment segment) => IntersectRaySegment(Point, Direction, segment.Start, segment.End, segment.Normal);
    /// <summary>
    /// Computes the intersection point between this ray and a line defined by a point and direction.
    /// </summary>
    /// <param name="linePoint">A point on the line.</param>
    /// <param name="lineDirection">The direction vector of the line.</param>
    /// <returns>A <see cref="CollisionPoint"/> representing the intersection, or an invalid point if there is no intersection.</returns>
    public CollisionPoint IntersectLine(Vector2 linePoint, Vector2 lineDirection) => IntersectRayLine(Point, Direction, linePoint, lineDirection);
    /// <summary>
    /// Computes the intersection point between this ray and a line.
    /// </summary>
    /// <param name="line">The line to test for intersection.</param>
    /// <returns>A <see cref="CollisionPoint"/> representing the intersection, or an invalid point if there is no intersection.</returns>
    public CollisionPoint IntersectLine(Line line) => IntersectRayLine(Point, Direction, line.Point, line.Direction, line.Normal);
    /// <summary>
    /// Computes the intersection point between this ray and another ray defined by a point and direction.
    /// </summary>
    /// <param name="rayPoint">The origin point of the other ray.</param>
    /// <param name="rayDirection">The direction vector of the other ray.</param>
    /// <returns>A <see cref="CollisionPoint"/> representing the intersection, or an invalid point if there is no intersection.</returns>
    public CollisionPoint IntersectRay(Vector2 rayPoint, Vector2 rayDirection) => IntersectRayRay(Point, Direction, rayPoint, rayDirection);
    /// <summary>
    /// Computes the intersection point between this ray and another ray.
    /// </summary>
    /// <param name="ray">The other ray to test for intersection.</param>
    /// <returns>A <see cref="CollisionPoint"/> representing the intersection, or an invalid point if there is no intersection.</returns>
    public CollisionPoint IntersectRay(Ray ray) => IntersectRayRay(Point, Direction, ray.Point, ray.Direction, ray.Normal);
    /// <summary>
    /// Computes the intersection points between this ray and a circle defined by center and radius.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <returns>A tuple of two <see cref="CollisionPoint"/>s representing the intersection points, or invalid points if there is no intersection.</returns>
    public (CollisionPoint a, CollisionPoint b) IntersectCircle(Vector2 circleCenter, float circleRadius) =>
        IntersectRayCircle(Point, Direction, circleCenter, circleRadius);
    /// <summary>
    /// Computes the intersection points between this ray and a circle.
    /// </summary>
    /// <param name="circle">The circle to test for intersection.</param>
    /// <returns>A tuple of two <see cref="CollisionPoint"/>s representing the intersection points, or invalid points if there is no intersection.</returns>
    public (CollisionPoint a, CollisionPoint b) IntersectCircle(Circle circle) => IntersectRayCircle(Point, Direction, circle.Center, circle.Radius);
    /// <summary>
    /// Computes the intersection points between this ray and a triangle defined by three points.
    /// </summary>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <returns>A tuple of two <see cref="CollisionPoint"/>s representing the intersection points, or invalid points if there is no intersection.</returns>
    public (CollisionPoint a, CollisionPoint b) IntersectTriangle(Vector2 a, Vector2 b, Vector2 c) => IntersectRayTriangle(Point, Direction, a, b, c);
    /// <summary>
    /// Computes the intersection points between this ray and a triangle.
    /// </summary>
    /// <param name="triangle">The triangle to test for intersection.</param>
    /// <returns>A tuple of two <see cref="CollisionPoint"/>s representing the intersection points, or invalid points if there is no intersection.</returns>
    public (CollisionPoint a, CollisionPoint b) IntersectTriangle(Triangle triangle) =>
        IntersectRayTriangle(Point, Direction, triangle.A, triangle.B, triangle.C);
    /// <summary>
    /// Computes the intersection points between this ray and a quad defined by four points.
    /// </summary>
    /// <param name="a">The first vertex of the quad.</param>
    /// <param name="b">The second vertex of the quad.</param>
    /// <param name="c">The third vertex of the quad.</param>
    /// <param name="d">The fourth vertex of the quad.</param>
    /// <returns>A tuple of two <see cref="CollisionPoint"/>s representing the intersection points, or invalid points if there is no intersection.</returns>
    public (CollisionPoint a, CollisionPoint b) IntersectQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => IntersectRayQuad(Point, Direction, a, b, c, d);
    /// <summary>
    /// Computes the intersection points between this ray and a quad.
    /// </summary>
    /// <param name="quad">The quad to test for intersection.</param>
    /// <returns>A tuple of two <see cref="CollisionPoint"/>s representing the intersection points, or invalid points if there is no intersection.</returns>
    public (CollisionPoint a, CollisionPoint b) IntersectQuad(Quad quad) => IntersectRayQuad(Point, Direction, quad.A, quad.B, quad.C, quad.D);
    /// <summary>
    /// Computes the intersection points between this ray and a rectangle defined by four points.
    /// </summary>
    /// <param name="a">The first vertex of the rectangle.</param>
    /// <param name="b">The second vertex of the rectangle.</param>
    /// <param name="c">The third vertex of the rectangle.</param>
    /// <param name="d">The fourth vertex of the rectangle.</param>
    /// <returns>A tuple of two <see cref="CollisionPoint"/>s representing the intersection points, or invalid points if there is no intersection.</returns>
    public (CollisionPoint a, CollisionPoint b) IntersectRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => IntersectRayQuad(Point, Direction, a, b, c, d);
    /// <summary>
    /// Computes the intersection points between this ray and a rectangle.
    /// </summary>
    /// <param name="rect">The rectangle to test for intersection.</param>
    /// <returns>A tuple of two <see cref="CollisionPoint"/>s representing the intersection points, or invalid points if there is no intersection.</returns>
    public (CollisionPoint a, CollisionPoint b) IntersectRect(Rect rect) => IntersectRayQuad(Point, Direction, rect.A, rect.B, rect.C, rect.D);
    /// <summary>
    /// Computes all intersection points between this ray and a polygon defined by a list of points.
    /// </summary>
    /// <param name="points">The list of points defining the polygon.</param>
    /// <param name="maxCollisionPoints">The maximum number of collision points to return. Use -1 for no limit.</param>
    /// <returns>A <see cref="CollisionPoints"/> object containing all intersection points, or null if there are none.</returns>
    public CollisionPoints? IntersectPolygon(List<Vector2> points, int maxCollisionPoints = -1) =>
        IntersectRayPolygon(Point, Direction, points, maxCollisionPoints);
    /// <summary>
    /// Computes all intersection points between this ray and a polygon.
    /// </summary>
    /// <param name="polygon">The polygon to test for intersection.</param>
    /// <param name="maxCollisionPoints">The maximum number of collision points to return. Use -1 for no limit.</param>
    /// <returns>A <see cref="CollisionPoints"/> object containing all intersection points, or null if there are none.</returns>
    public CollisionPoints? IntersectPolygon(Polygon polygon, int maxCollisionPoints = -1) =>
        IntersectRayPolygon(Point, Direction, polygon, maxCollisionPoints);
    /// <summary>
    /// Computes all intersection points between this ray and a polyline defined by a list of points.
    /// </summary>
    /// <param name="points">The list of points defining the polyline.</param>
    /// <param name="maxCollisionPoints">The maximum number of collision points to return. Use -1 for no limit.</param>
    /// <returns>A <see cref="CollisionPoints"/> object containing all intersection points, or null if there are none.</returns>
    public CollisionPoints? IntersectPolyline(List<Vector2> points, int maxCollisionPoints = -1) =>
        IntersectRayPolyline(Point, Direction, points, maxCollisionPoints);
    /// <summary>
    /// Computes all intersection points between this ray and a polyline.
    /// </summary>
    /// <param name="polyline">The polyline to test for intersection.</param>
    /// <param name="maxCollisionPoints">The maximum number of collision points to return. Use -1 for no limit.</param>
    /// <returns>A <see cref="CollisionPoints"/> object containing all intersection points, or null if there are none.</returns>
    public CollisionPoints? IntersectPolyline(Polyline polyline, int maxCollisionPoints = -1) =>
        IntersectRayPolyline(Point, Direction, polyline, maxCollisionPoints);
    /// <summary>
    /// Computes all intersection points between this ray and a list of segments.
    /// </summary>
    /// <param name="segments">The list of segments to test for intersection.</param>
    /// <param name="maxCollisionPoints">The maximum number of collision points to return. Use -1 for no limit.</param>
    /// <returns>A <see cref="CollisionPoints"/> object containing all intersection points, or null if there are none.</returns>
    public CollisionPoints? IntersectSegments(List<Segment> segments, int maxCollisionPoints = -1) =>
        IntersectRaySegments(Point, Direction, segments, maxCollisionPoints);
    /// <summary>
    /// Computes all intersection points between this ray and a set of segments.
    /// </summary>
    /// <param name="segments">The set of segments to test for intersection.</param>
    /// <param name="maxCollisionPoints">The maximum number of collision points to return. Use -1 for no limit.</param>
    /// <returns>A <see cref="CollisionPoints"/> object containing all intersection points, or null if there are none.</returns>
    public CollisionPoints? IntersectSegments(Segments segments, int maxCollisionPoints = -1) =>
        IntersectRaySegments(Point, Direction, segments, maxCollisionPoints);

}