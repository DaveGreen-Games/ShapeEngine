using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.LineDef;

public readonly partial struct Line
{
    /// <summary>
    /// Computes the intersection point between this line and a segment.
    /// </summary>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <returns>
    /// A <see cref="IntersectionPoint"/> at the intersection, or an invalid <see cref="IntersectionPoint"/> if no intersection exists.
    /// </returns>
    /// <remarks>
    /// This is a convenience method that uses the line's point and direction.
    /// </remarks>
    public IntersectionPoint IntersectSegment(Vector2 segmentStart, Vector2 segmentEnd) => IntersectLineSegment(Point, Direction, segmentStart, segmentEnd);

    /// <summary>
    /// Computes the intersection point between this line and a segment.
    /// </summary>
    /// <param name="segment">The <see cref="Segment"/> to check for intersection.</param>
    /// <returns>
    /// A <see cref="IntersectionPoint"/> at the intersection, or an invalid <see cref="IntersectionPoint"/> if no intersection exists.
    /// </returns>
    /// <remarks>
    /// This is a convenience method that uses the line's point and direction.
    /// </remarks>
    public IntersectionPoint IntersectSegment(Segment segment) => IntersectLineSegment(Point, Direction, segment.Start, segment.End, segment.Normal);

    /// <summary>
    /// Computes the intersection point between this line and another infinite line.
    /// </summary>
    /// <param name="otherPoint">A point through which the other line passes.</param>
    /// <param name="otherDirection">The direction vector of the other line.</param>
    /// <returns>
    /// A <see cref="IntersectionPoint"/> at the intersection, or an invalid <see cref="IntersectionPoint"/> if the lines are parallel.
    /// </returns>
    public IntersectionPoint IntersectLine(Vector2 otherPoint, Vector2 otherDirection) => IntersectLineLine(Point, Direction, otherPoint, otherDirection);

    /// <summary>
    /// Computes the intersection point between this line and another infinite line.
    /// </summary>
    /// <param name="otherLine">The <see cref="Line"/> to check for intersection.</param>
    /// <returns>
    /// A <see cref="IntersectionPoint"/> at the intersection, or an invalid <see cref="IntersectionPoint"/> if the lines are parallel.
    /// </returns>
    public IntersectionPoint IntersectLine(Line otherLine) => IntersectLineLine(Point, Direction, otherLine.Point, otherLine.Direction, otherLine.Normal);

    /// <summary>
    /// Computes the intersection point between this line and a ray.
    /// </summary>
    /// <param name="otherPoint">The origin point of the ray.</param>
    /// <param name="otherDirection">The direction vector of the ray.</param>
    /// <returns>
    /// A <see cref="IntersectionPoint"/> at the intersection, or an invalid <see cref="IntersectionPoint"/> if the intersection does not lie in the direction of the ray.
    /// </returns>
    public IntersectionPoint IntersectRay(Vector2 otherPoint, Vector2 otherDirection) => IntersectLineRay(Point, Direction, otherPoint, otherDirection);

    /// <summary>
    /// Computes the intersection point between this line and a ray.
    /// </summary>
    /// <param name="otherRay">The <see cref="Ray"/> to check for intersection.</param>
    /// <returns>
    /// A <see cref="IntersectionPoint"/> at the intersection, or an invalid <see cref="IntersectionPoint"/> if the intersection does not lie in the direction of the ray.
    /// </returns>
    public IntersectionPoint IntersectRay(Ray otherRay) => IntersectLineRay(Point, Direction, otherRay.Point, otherRay.Direction, otherRay.Normal);

    /// <summary>
    /// Computes the intersection points between this infinite line and a circle.
    /// </summary>
    /// <param name="otherCircle">The <see cref="Circle"/> to check for intersection.</param>
    /// <returns>
    /// A tuple containing up to two <see cref="IntersectionPoint"/> objects representing the intersection points.
    /// If no intersection exists, both points are invalid.
    /// </returns>
    public (IntersectionPoint a, IntersectionPoint b) IntersectCircle(Circle otherCircle) =>
        IntersectLineCircle(Point, Direction, otherCircle.Center, otherCircle.Radius);

    /// <summary>
    /// Computes the intersection points between this infinite line and a circle.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <returns>
    /// A tuple containing up to two <see cref="IntersectionPoint"/> objects representing the intersection points.
    /// If no intersection exists, both points are invalid.
    /// </returns>
    public (IntersectionPoint a, IntersectionPoint b) IntersectCircle(Vector2 circleCenter, float circleRadius) =>
        IntersectLineCircle(Point, Direction, circleCenter, circleRadius);

    /// <summary>
    /// Computes the intersection points between this infinite line and a triangle defined by three vertices.
    /// </summary>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <returns>
    /// A tuple containing up to two <see cref="IntersectionPoint"/> objects representing the intersection points.
    /// If no intersection exists, both points are invalid.
    /// </returns>
    public (IntersectionPoint a, IntersectionPoint b) IntersectTriangle(Vector2 a, Vector2 b, Vector2 c) => IntersectLineTriangle(Point, Direction, a, b, c);

    /// <summary>
    /// Computes the intersection points between this infinite line and a triangle.
    /// </summary>
    /// <param name="triangle">The <see cref="Triangle"/> to check for intersection.</param>
    /// <returns>
    /// A tuple containing up to two <see cref="IntersectionPoint"/> objects representing the intersection points.
    /// If no intersection exists, both points are invalid.
    /// </returns>
    public (IntersectionPoint a, IntersectionPoint b) IntersectTriangle(Triangle triangle) =>
        IntersectLineTriangle(Point, Direction, triangle.A, triangle.B, triangle.C);

    /// <summary>
    /// Computes the intersection points between this infinite line and a quadrilateral defined by four vertices.
    /// </summary>
    /// <param name="a">The first vertex of the quadrilateral.</param>
    /// <param name="b">The second vertex of the quadrilateral.</param>
    /// <param name="c">The third vertex of the quadrilateral.</param>
    /// <param name="d">The fourth vertex of the quadrilateral.</param>
    /// <returns>
    /// A tuple containing up to two <see cref="IntersectionPoint"/> objects representing the intersection points.
    /// If no intersection exists, both points are invalid.
    /// </returns>
    public (IntersectionPoint a, IntersectionPoint b) IntersectQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => IntersectLineQuad(Point, Direction, a, b, c, d);

    /// <summary>
    /// Computes the intersection points between this infinite line and a quadrilateral.
    /// </summary>
    /// <param name="quad">The <see cref="Quad"/> to check for intersection.</param>
    /// <returns>
    /// A tuple containing up to two <see cref="IntersectionPoint"/> objects representing the intersection points.
    /// If no intersection exists, both points are invalid.
    /// </returns>
    public (IntersectionPoint a, IntersectionPoint b) IntersectQuad(Quad quad) => IntersectLineQuad(Point, Direction, quad.A, quad.B, quad.C, quad.D);

    /// <summary>
    /// Computes the intersection points between this infinite line and a rectangle defined by four vertices.
    /// </summary>
    /// <param name="a">The first vertex of the rectangle.</param>
    /// <param name="b">The second vertex of the rectangle.</param>
    /// <param name="c">The third vertex of the rectangle.</param>
    /// <param name="d">The fourth vertex of the rectangle.</param>
    /// <returns>
    /// A tuple containing up to two <see cref="IntersectionPoint"/> objects representing the intersection points.
    /// If no intersection exists, both points are invalid.
    /// </returns>
    public (IntersectionPoint a, IntersectionPoint b) IntersectRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => IntersectLineQuad(Point, Direction, a, b, c, d);

    /// <summary>
    /// Computes the intersection points between this infinite line and a rectangle.
    /// </summary>
    /// <param name="rect">The <see cref="Rect"/> to check for intersection.</param>
    /// <returns>
    /// A tuple containing up to two <see cref="IntersectionPoint"/> objects representing the intersection points.
    /// If no intersection exists, both points are invalid.
    /// </returns>
    public (IntersectionPoint a, IntersectionPoint b) IntersectRect(Rect rect) => IntersectLineQuad(Point, Direction, rect.A, rect.B, rect.C, rect.D);

    /// <summary>
    /// Computes the intersection points between this infinite line and a polygon defined by a list of vertices.
    /// </summary>
    /// <param name="points">A list of vertices defining the polygon.</param>
    /// <param name="maxCollisionPoints">
    /// The maximum number of collision points to return. Use -1 for no limit.
    /// </param>
    /// <returns>
    /// A <see cref="IntersectionPoints"/> object containing the intersection points, or null if no intersections exist.
    /// </returns>
    public IntersectionPoints? IntersectPolygon(List<Vector2> points, int maxCollisionPoints = -1)
    {
        if (points.Count < 3) return null;
        return IntersectLinePolygon(Point, Direction, points, maxCollisionPoints);
    }

    /// <summary>
    /// Computes the intersection points between this infinite line and a polygon.
    /// </summary>
    /// <param name="polygon">The <see cref="Polygon"/> to check for intersection.</param>
    /// <param name="maxCollisionPoints">
    /// The maximum number of collision points to return. Use -1 for no limit.
    /// </param>
    /// <returns>
    /// A <see cref="IntersectionPoints"/> object containing the intersection points, or null if no intersections exist.
    /// </returns>
    public IntersectionPoints? IntersectPolygon(Polygon polygon, int maxCollisionPoints = -1)
    {
        if (polygon.Count < 3) return null;
        return IntersectLinePolygon(Point, Direction, polygon, maxCollisionPoints);
    }

    /// <summary>
    /// Computes the intersection points between this infinite line and a polyline defined by a list of vertices.
    /// </summary>
    /// <param name="points">A list of vertices defining the polyline.</param>
    /// <param name="maxCollisionPoints">
    /// The maximum number of collision points to return. Use -1 for no limit.
    /// </param>
    /// <returns>
    /// A <see cref="IntersectionPoints"/> object containing the intersection points, or null if no intersections exist.
    /// </returns>
    public IntersectionPoints? IntersectPolyline(List<Vector2> points, int maxCollisionPoints = -1)
    {
        if (points.Count < 2) return null;
        return IntersectLinePolyline(Point, Direction, points, maxCollisionPoints);
    }

    /// <summary>
    /// Computes the intersection points between this infinite line and a polyline.
    /// </summary>
    /// <param name="polyline">The <see cref="Polyline"/> to check for intersection.</param>
    /// <param name="maxCollisionPoints">
    /// The maximum number of collision points to return. Use -1 for no limit.
    /// </param>
    /// <returns>
    /// A <see cref="IntersectionPoints"/> object containing the intersection points, or null if no intersections exist.
    /// </returns>
    public IntersectionPoints? IntersectPolyline(Polyline polyline, int maxCollisionPoints = -1)
    {
        if (polyline.Count < 2) return null;
        return IntersectLinePolyline(Point, Direction, polyline, maxCollisionPoints);
    }

    /// <summary>
    /// Computes the intersection points between this line and a collection of segments.
    /// </summary>
    /// <param name="segments">A list of <see cref="Segment"/> objects to check for intersections.</param>
    /// <param name="maxCollisionPoints">The maximum number of collision points to return. Use -1 for no limit.</param>
    /// <returns>
    /// A <see cref="IntersectionPoints"/> object containing the intersection points, or null if no intersections exist.
    /// </returns>
    /// <remarks>
    /// This is a convenience method that uses the line's point and direction.
    /// </remarks>
    public IntersectionPoints? IntersectSegments(List<Segment> segments, int maxCollisionPoints = -1) =>
        IntersectLineSegments(Point, Direction, segments, maxCollisionPoints);

    /// <summary>
    /// Computes the intersection points between this line and a collection of segments.
    /// </summary>
    /// <param name="segments">A <see cref="Segments"/> collection to check for intersections.</param>
    /// <param name="maxCollisionPoints">The maximum number of collision points to return. Use -1 for no limit.</param>
    /// <returns>
    /// A <see cref="IntersectionPoints"/> object containing the intersection points, or null if no intersections exist.
    /// </returns>
    /// <remarks>
    /// This is a convenience method that uses the line's point and direction.
    /// </remarks>
    public IntersectionPoints? IntersectSegments(Segments segments, int maxCollisionPoints = -1) =>
        IntersectLineSegments(Point, Direction, segments, maxCollisionPoints);

    

}