using System.Numerics;

namespace ShapeEngine.Geometry.CircleDef;

public readonly partial struct Circle
{
    #region Contains

    /// <summary>
    /// Determines whether a given point lies inside or on the boundary of a circle.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="p">The point to check.</param>
    /// <returns><c>true</c> if the point is inside or on the circle; otherwise, <c>false</c>.</returns>
    public static bool ContainsCirclePoint(Vector2 circleCenter, float circleRadius, Vector2 p)
    {
        return (circleCenter - p).LengthSquared() <= circleRadius * circleRadius;
    }

    /// <summary>
    /// Determines whether both specified points lie inside or on the boundary of a circle.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="a">The first point to check.</param>
    /// <param name="b">The second point to check.</param>
    /// <returns><c>true</c> if both points are inside or on the circle; otherwise, <c>false</c>.</returns>
    public static bool ContainsCirclePoints(Vector2 circleCenter, float circleRadius, Vector2 a, Vector2 b)
    {
        return ContainsCirclePoint(circleCenter, circleRadius, a) &&
               ContainsCirclePoint(circleCenter, circleRadius, b);
    }

    /// <summary>
    /// Determines whether the specified three points all lie inside or on the boundary of a circle.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="a">The first point to check.</param>
    /// <param name="b">The second point to check.</param>
    /// <param name="c">The third point to check.</param>
    /// <returns><c>true</c> if all three points are inside or on the circle; otherwise, <c>false</c>.</returns>
    public static bool ContainsCirclePoints(Vector2 circleCenter, float circleRadius, Vector2 a, Vector2 b, Vector2 c)
    {
        return ContainsCirclePoint(circleCenter, circleRadius, a) &&
               ContainsCirclePoint(circleCenter, circleRadius, b) &&
               ContainsCirclePoint(circleCenter, circleRadius, c);
    }

    /// <summary>
    /// Determines whether the specified four points all lie inside or on the boundary of a circle.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="a">The first point to check.</param>
    /// <param name="b">The second point to check.</param>
    /// <param name="c">The third point to check.</param>
    /// <param name="d">The fourth point to check.</param>
    /// <returns><c>true</c> if all four points are inside or on the circle; otherwise, <c>false</c>.</returns>
    public static bool ContainsCirclePoints(Vector2 circleCenter, float circleRadius, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return ContainsCirclePoint(circleCenter, circleRadius, a) &&
               ContainsCirclePoint(circleCenter, circleRadius, b) &&
               ContainsCirclePoint(circleCenter, circleRadius, c) &&
               ContainsCirclePoint(circleCenter, circleRadius, d);
    }

    /// <summary>
    /// Determines whether all points in the provided list lie inside or on the boundary of a circle.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="points">The list of points to check.</param>
    /// <returns><c>true</c> if all points are inside or on the circle; otherwise, <c>false</c>.</returns>
    public static bool ContainsCirclePoints(Vector2 circleCenter, float circleRadius, List<Vector2> points)
    {
        if (points.Count <= 0) return false;
        foreach (var point in points)
        {
            if (!ContainsCirclePoint(circleCenter, circleRadius, point)) return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether a line segment,
    /// defined by its start and end points, lies entirely inside or on the boundary of a circle.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <returns><c>true</c> if both segment endpoints are inside or on the circle; otherwise, <c>false</c>.</returns>
    public static bool ContainsCircleSegment(Vector2 circleCenter, float circleRadius, Vector2 segmentStart, Vector2 segmentEnd)
    {
        if (!ContainsCirclePoints(circleCenter, circleRadius, segmentStart, segmentEnd)) return false;
        return true;
    }

    /// <summary>
    /// Determines whether one circle completely contains another circle.
    /// </summary>
    /// <param name="circle1Center">The center of the containing circle.</param>
    /// <param name="circle1Radius">The radius of the containing circle.</param>
    /// <param name="circle2Center">The center of the circle to check for containment.</param>
    /// <param name="circle2Radius">The radius of the circle to check for containment.</param>
    /// <returns><c>true</c> if the second circle is entirely within the first; otherwise, <c>false</c>.</returns>
    public static bool ContainsCircleCircle(Vector2 circle1Center, float circle1Radius, Vector2 circle2Center, float circle2Radius)
    {
        if (circle2Radius > circle1Radius) return false;
        var dis = (circle2Center - circle1Center).Length() + circle2Radius;
        return dis <= circle1Radius;
    }

    /// <summary>
    /// Determines whether the specified triangle (defined by points a, b, and c)
    /// lies entirely inside or on the boundary of a circle.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <returns><c>true</c> if all triangle vertices are inside or on the circle; otherwise, <c>false</c>.</returns>
    public static bool ContainsCircleTriangle(Vector2 circleCenter, float circleRadius, Vector2 a, Vector2 b, Vector2 c)
    {
        return ContainsCirclePoints(circleCenter, circleRadius, a, b, c);
    }

    /// <summary>
    /// Determines whether the specified quadrilateral (defined by points a, b, c, and d)
    /// lies entirely inside or on the boundary of a circle.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="a">The first vertex of the quadrilateral.</param>
    /// <param name="b">The second vertex of the quadrilateral.</param>
    /// <param name="c">The third vertex of the quadrilateral.</param>
    /// <param name="d">The fourth vertex of the quadrilateral.</param>
    /// <returns><c>true</c> if all quadrilateral vertices are inside or on the circle; otherwise, <c>false</c>.</returns>
    public static bool ContainsCircleQuad(Vector2 circleCenter, float circleRadius, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return ContainsCirclePoints(circleCenter, circleRadius, a, b, c, d);
    }

    /// <summary>
    /// Determines whether all four specified points (representing the corners of a rectangle)
    /// lie inside or on the boundary of a circle.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="a">The first corner of the rectangle.</param>
    /// <param name="b">The second corner of the rectangle.</param>
    /// <param name="c">The third corner of the rectangle.</param>
    /// <param name="d">The fourth corner of the rectangle.</param>
    /// <returns><c>true</c> if all four points are inside or on the circle; otherwise, <c>false</c>.</returns>
    public static bool ContainsCircleRect(Vector2 circleCenter, float circleRadius, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return ContainsCirclePoints(circleCenter, circleRadius, a, b, c, d);
    }

    /// <summary>
    /// Determines whether all points in the provided polyline lie inside or on the boundary of a circle.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="polyline">The list of points representing the polyline to check.</param>
    /// <returns><c>true</c> if all points are inside or on the circle; otherwise, <c>false</c>.</returns>
    public static bool ContainsCirclePolyline(Vector2 circleCenter, float circleRadius, List<Vector2> polyline)
    {
        return polyline.Count >= 2 && ContainsCirclePoints(circleCenter, circleRadius, polyline);
    }

    /// <summary>
    /// Determines whether all points in the provided polygon lie inside or on the boundary of a circle.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="polygon">The list of points representing the polygon to check.</param>
    /// <returns><c>true</c> if all points are inside or on the circle; otherwise, <c>false</c>.</returns>
    public static bool ContainsCirclePolygon(Vector2 circleCenter, float circleRadius, List<Vector2> polygon)
    {
        return ContainsCirclePoints(circleCenter, circleRadius, polygon);
    }

    #endregion
}