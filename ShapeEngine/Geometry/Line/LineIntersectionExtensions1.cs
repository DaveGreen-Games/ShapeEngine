using System.Numerics;
using ShapeEngine.Core.CollisionSystem;

namespace ShapeEngine.Geometry.Line;

public static partial class LineIntersection
{
    /// <summary>
    /// Computes the intersection point between this line and a segment.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <returns>
    /// A <see cref="CollisionPoint"/> at the intersection, or an invalid <see cref="CollisionPoint"/> if no intersection exists.
    /// </returns>
    /// <remarks>
    /// This is a convenience method that uses the line's point and direction.
    /// </remarks>
    public static CollisionPoint IntersectSegment(this Line self, Vector2 segmentStart, Vector2 segmentEnd) =>
        IntersectLineSegment(self.Point, self.Direction, segmentStart, segmentEnd);

    /// <summary>
    /// Computes the intersection point between this line and a segment.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="segment">The <see cref="Segment"/> to check for intersection.</param>
    /// <returns>
    /// A <see cref="CollisionPoint"/> at the intersection, or an invalid <see cref="CollisionPoint"/> if no intersection exists.
    /// </returns>
    /// <remarks>
    /// This is a convenience method that uses the line's point and direction.
    /// </remarks>
    public static CollisionPoint IntersectSegment(this Line self, Segment.Segment segment) =>
        IntersectLineSegment(self.Point, self.Direction, segment.Start, segment.End, segment.Normal);

    /// <summary>
    /// Computes the intersection point between this line and another infinite line.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="otherPoint">A point through which the other line passes.</param>
    /// <param name="otherDirection">The direction vector of the other line.</param>
    /// <returns>
    /// A <see cref="CollisionPoint"/> at the intersection, or an invalid <see cref="CollisionPoint"/> if the lines are parallel.
    /// </returns>
    public static CollisionPoint IntersectLine(this Line self, Vector2 otherPoint, Vector2 otherDirection) =>
        IntersectLineLine(self.Point, self.Direction, otherPoint, otherDirection);

    /// <summary>
    /// Computes the intersection point between this line and another infinite line.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="otherLine">The <see cref="Line"/> to check for intersection.</param>
    /// <returns>
    /// A <see cref="CollisionPoint"/> at the intersection, or an invalid <see cref="CollisionPoint"/> if the lines are parallel.
    /// </returns>
    public static CollisionPoint IntersectLine(this Line self, Line otherLine) =>
        IntersectLineLine(self.Point, self.Direction, otherLine.Point, otherLine.Direction, otherLine.Normal);

    /// <summary>
    /// Computes the intersection point between this line and a ray.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="otherPoint">The origin point of the ray.</param>
    /// <param name="otherDirection">The direction vector of the ray.</param>
    /// <returns>
    /// A <see cref="CollisionPoint"/> at the intersection, or an invalid <see cref="CollisionPoint"/> if the intersection does not lie in the direction of the ray.
    /// </returns>
    public static CollisionPoint IntersectRay(this Line self, Vector2 otherPoint, Vector2 otherDirection) =>
        IntersectLineRay(self.Point, self.Direction, otherPoint, otherDirection);

    /// <summary>
    /// Computes the intersection point between this line and a ray.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="otherRay">The <see cref="Ray"/> to check for intersection.</param>
    /// <returns>
    /// A <see cref="CollisionPoint"/> at the intersection, or an invalid <see cref="CollisionPoint"/> if the intersection does not lie in the direction of the ray.
    /// </returns>
    public static CollisionPoint IntersectRay(this Line self, Ray.Ray otherRay) =>
        IntersectLineRay(self.Point, self.Direction, otherRay.Point, otherRay.Direction, otherRay.Normal);

    /// <summary>
    /// Computes the intersection points between this infinite line and a circle.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="otherCircle">The <see cref="Circle"/> to check for intersection.</param>
    /// <returns>
    /// A tuple containing up to two <see cref="CollisionPoint"/> objects representing the intersection points.
    /// If no intersection exists, both points are invalid.
    /// </returns>
    public static (CollisionPoint a, CollisionPoint b) IntersectCircle(this Line self, Circle.Circle otherCircle) =>
        IntersectLineCircle(self.Point, self.Direction, otherCircle.Center, otherCircle.Radius);

    /// <summary>
    /// Computes the intersection points between this infinite line and a circle.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <returns>
    /// A tuple containing up to two <see cref="CollisionPoint"/> objects representing the intersection points.
    /// If no intersection exists, both points are invalid.
    /// </returns>
    public static (CollisionPoint a, CollisionPoint b) IntersectCircle(this Line self, Vector2 circleCenter, float circleRadius) =>
        IntersectLineCircle(self.Point, self.Direction, circleCenter, circleRadius);

    /// <summary>
    /// Computes the intersection points between this infinite line and a triangle defined by three vertices.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <returns>
    /// A tuple containing up to two <see cref="CollisionPoint"/> objects representing the intersection points.
    /// If no intersection exists, both points are invalid.
    /// </returns>
    public static (CollisionPoint a, CollisionPoint b) IntersectTriangle(this Line self, Vector2 a, Vector2 b, Vector2 c) =>
        IntersectLineTriangle(self.Point, self.Direction, a, b, c);

    /// <summary>
    /// Computes the intersection points between this infinite line and a triangle.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="triangle">The <see cref="Triangle"/> to check for intersection.</param>
    /// <returns>
    /// A tuple containing up to two <see cref="CollisionPoint"/> objects representing the intersection points.
    /// If no intersection exists, both points are invalid.
    /// </returns>
    public static (CollisionPoint a, CollisionPoint b) IntersectTriangle(this Line self, Triangle.Triangle triangle) =>
        IntersectLineTriangle(self.Point, self.Direction, triangle.A, triangle.B, triangle.C);

    /// <summary>
    /// Computes the intersection points between this infinite line and a quadrilateral defined by four vertices.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="a">The first vertex of the quadrilateral.</param>
    /// <param name="b">The second vertex of the quadrilateral.</param>
    /// <param name="c">The third vertex of the quadrilateral.</param>
    /// <param name="d">The fourth vertex of the quadrilateral.</param>
    /// <returns>
    /// A tuple containing up to two <see cref="CollisionPoint"/> objects representing the intersection points.
    /// If no intersection exists, both points are invalid.
    /// </returns>
    public static (CollisionPoint a, CollisionPoint b) IntersectQuad(this Line self, Vector2 a, Vector2 b, Vector2 c, Vector2 d) =>
        IntersectLineQuad(self.Point, self.Direction, a, b, c, d);

    /// <summary>
    /// Computes the intersection points between this infinite line and a quadrilateral.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="quad">The <see cref="Quad"/> to check for intersection.</param>
    /// <returns>
    /// A tuple containing up to two <see cref="CollisionPoint"/> objects representing the intersection points.
    /// If no intersection exists, both points are invalid.
    /// </returns>
    public static (CollisionPoint a, CollisionPoint b) IntersectQuad(this Line self, Quad.Quad quad) =>
        IntersectLineQuad(self.Point, self.Direction, quad.A, quad.B, quad.C, quad.D);

    /// <summary>
    /// Computes the intersection points between this infinite line and a rectangle defined by four vertices.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="a">The first vertex of the rectangle.</param>
    /// <param name="b">The second vertex of the rectangle.</param>
    /// <param name="c">The third vertex of the rectangle.</param>
    /// <param name="d">The fourth vertex of the rectangle.</param>
    /// <returns>
    /// A tuple containing up to two <see cref="CollisionPoint"/> objects representing the intersection points.
    /// If no intersection exists, both points are invalid.
    /// </returns>
    public static (CollisionPoint a, CollisionPoint b) IntersectRect(this Line self, Vector2 a, Vector2 b, Vector2 c, Vector2 d) =>
        IntersectLineQuad(self.Point, self.Direction, a, b, c, d);

    /// <summary>
    /// Computes the intersection points between this infinite line and a rectangle.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="rect">The <see cref="Rect"/> to check for intersection.</param>
    /// <returns>
    /// A tuple containing up to two <see cref="CollisionPoint"/> objects representing the intersection points.
    /// If no intersection exists, both points are invalid.
    /// </returns>
    public static (CollisionPoint a, CollisionPoint b) IntersectRect(this Line self, Rect.Rect rect) =>
        IntersectLineQuad(self.Point, self.Direction, rect.A, rect.B, rect.C, rect.D);

    /// <summary>
    /// Computes the intersection points between this infinite line and a polygon, if they exist.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="points">A list of vertices defining the polygon.</param>
    /// <param name="maxCollisionPoints">
    /// The maximum number of collision points to return. Use -1 for no limit.
    /// </param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing the intersection points, or null if no intersections exist.
    /// </returns>
    public static CollisionPoints? IntersectPolygon(this Line self, List<Vector2> points, int maxCollisionPoints = -1) =>
        IntersectLinePolygon(self.Point, self.Direction, points, maxCollisionPoints);

    /// <summary>
    /// Computes the intersection points between this infinite line and a polygon.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="polygon">The <see cref="Polygon"/> to check for intersection.</param>
    /// <param name="maxCollisionPoints">
    /// The maximum number of collision points to return. Use -1 for no limit.
    /// </param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing the intersection points, or null if no intersections exist.
    /// </returns>
    public static CollisionPoints? IntersectPolygon(this Line self, Polygon.Polygon polygon, int maxCollisionPoints = -1) =>
        IntersectLinePolygon(self.Point, self.Direction, polygon, maxCollisionPoints);

    /// <summary>
    /// Computes the intersection points between this infinite line and a polyline, if they exist.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="points">A list of vertices defining the polyline.</param>
    /// <param name="maxCollisionPoints">
    /// The maximum number of collision points to return. Use -1 for no limit.
    /// </param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing the intersection points, or null if no intersections exist.
    /// </returns>
    public static CollisionPoints? IntersectPolyline(this Line self, List<Vector2> points, int maxCollisionPoints = -1) =>
        IntersectLinePolyline(self.Point, self.Direction, points, maxCollisionPoints);

    /// <summary>
    /// Computes the intersection points between this infinite line and a polyline.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="polyline">The <see cref="Polyline"/> to check for intersection.</param>
    /// <param name="maxCollisionPoints">
    /// The maximum number of collision points to return. Use -1 for no limit.
    /// </param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing the intersection points, or null if no intersections exist.
    /// </returns>
    public static CollisionPoints? IntersectPolyline(this Line self, Polyline.Polyline polyline, int maxCollisionPoints = -1) =>
        IntersectLinePolyline(self.Point, self.Direction, polyline, maxCollisionPoints);

    /// <summary>
    /// Computes the intersection points between this infinite line and a collection of segments.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="segments">A list of segments to check for intersections.</param>
    /// <param name="maxCollisionPoints">The maximum number of collision points to return. Use -1 for no limit.</param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing the intersection points, or null if no intersections exist.
    /// </returns>
    /// <remarks>
    /// The function iterates through all segments in the collection to find intersections with the line.
    /// </remarks>
    public static CollisionPoints? IntersectSegments(this Line self, List<Segment.Segment> segments, int maxCollisionPoints = -1) =>
        IntersectLineSegments(self.Point, self.Direction, segments, maxCollisionPoints);

    /// <summary>
    /// Computes the intersection points between this infinite line and a collection of segments.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="segments">A <see cref="Segment.Segments"/> collection to check for intersections.</param>
    /// <param name="maxCollisionPoints">The maximum number of collision points to return. Use -1 for no limit.</param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing the intersection points, or null if no intersections exist.
    /// </returns>
    /// <remarks>
    /// The function iterates through all segments in the collection to find intersections with the line.
    /// </remarks>
    public static CollisionPoints? IntersectSegments(this Line self, Segment.Segments segments, int maxCollisionPoints = -1) =>
        IntersectLineSegments(self.Point, self.Direction, segments, maxCollisionPoints);
}