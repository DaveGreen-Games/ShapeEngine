using System.Numerics;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.SegmentDef;

namespace ShapeEngine.Geometry.QuadDef;

public readonly partial struct Quad
{
    /// <summary>
    /// Determines whether a point is contained within a quadrilateral defined by four vertices.
    /// </summary>
    /// <param name="qA">First vertex of the quad (CCW order).</param>
    /// <param name="qB">Second vertex of the quad (CCW order).</param>
    /// <param name="qC">Third vertex of the quad (CCW order).</param>
    /// <param name="qD">Fourth vertex of the quad (CCW order).</param>
    /// <param name="point">The point to test.</param>
    /// <returns><c>true</c> if the point is inside the quad; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Uses the even-odd rule for point-in-polygon testing.
    /// This rule is a common algorithm to determine if a point is inside a polygon:
    /// you draw a ray from the point and count how many times it crosses the polygon's edges.
    /// If the number of crossings is odd, the point is inside; if even, it's outside.
    /// This method is robust for convex and concave polygons, including quadrilaterals.
    /// </remarks>
    public static bool ContainsQuadPoint(Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD, Vector2 point)
    {
        var oddNodes = false;

        if (Polygon.ContainsPointCheck(qD, qA, point)) oddNodes = !oddNodes;
        if (Polygon.ContainsPointCheck(qA, qB, point)) oddNodes = !oddNodes;
        if (Polygon.ContainsPointCheck(qB, qC, point)) oddNodes = !oddNodes;
        if (Polygon.ContainsPointCheck(qC, qD, point)) oddNodes = !oddNodes;

        return oddNodes;
    }

    /// <summary>
    /// Determines whether two points are contained within a quadrilateral defined by four vertices.
    /// </summary>
    /// <param name="qA">First vertex of the quad.</param>
    /// <param name="qB">Second vertex of the quad.</param>
    /// <param name="qC">Third vertex of the quad.</param>
    /// <param name="qD">Fourth vertex of the quad.</param>
    /// <param name="u">First point to test.</param>
    /// <param name="v">Second point to test.</param>
    /// <returns><c>true</c> if both points are inside the quad; otherwise, <c>false</c>.</returns>
    public static bool ContainsQuadPoints(Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD, Vector2 u, Vector2 v)
    {
        return ContainsQuadPoint(qA, qB, qC, qD, u) &&
               ContainsQuadPoint(qA, qB, qC, qD, v);
    }

    /// <summary>
    /// Determines whether three points are contained within a quadrilateral defined by four vertices.
    /// </summary>
    /// <param name="qA">First vertex of the quad.</param>
    /// <param name="qB">Second vertex of the quad.</param>
    /// <param name="qC">Third vertex of the quad.</param>
    /// <param name="qD">Fourth vertex of the quad.</param>
    /// <param name="u">First point to test.</param>
    /// <param name="v">Second point to test.</param>
    /// <param name="w">Third point to test.</param>
    /// <returns><c>true</c> if all points are inside the quad; otherwise, <c>false</c>.</returns>
    public static bool ContainsQuadPoints(Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD, Vector2 u, Vector2 v, Vector2 w)
    {
        return ContainsQuadPoint(qA, qB, qC, qD, u) &&
               ContainsQuadPoint(qA, qB, qC, qD, v) &&
               ContainsQuadPoint(qA, qB, qC, qD, w);
    }

    /// <summary>
    /// Determines whether four points are contained within a quadrilateral defined by four vertices.
    /// </summary>
    /// <param name="qA">First vertex of the quad.</param>
    /// <param name="qB">Second vertex of the quad.</param>
    /// <param name="qC">Third vertex of the quad.</param>
    /// <param name="qD">Fourth vertex of the quad.</param>
    /// <param name="u">First point to test.</param>
    /// <param name="v">Second point to test.</param>
    /// <param name="w">Third point to test.</param>
    /// <param name="x">Fourth point to test.</param>
    /// <returns><c>true</c> if all points are inside the quad; otherwise, <c>false</c>.</returns>
    public static bool ContainsQuadPoints(Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD, Vector2 u, Vector2 v, Vector2 w, Vector2 x)
    {
        return ContainsQuadPoint(qA, qB, qC, qD, u) &&
               ContainsQuadPoint(qA, qB, qC, qD, v) &&
               ContainsQuadPoint(qA, qB, qC, qD, w) &&
               ContainsQuadPoint(qA, qB, qC, qD, x);
    }

    /// <summary>
    /// Determines whether all points in a list are contained within a quadrilateral defined by four vertices.
    /// </summary>
    /// <param name="qA">First vertex of the quad.</param>
    /// <param name="qB">Second vertex of the quad.</param>
    /// <param name="qC">Third vertex of the quad.</param>
    /// <param name="qD">Fourth vertex of the quad.</param>
    /// <param name="points">The list of points to test.</param>
    /// <returns><c>true</c> if all points are inside the quad; otherwise, <c>false</c>.</returns>
    public static bool ContainsQuadPoints(Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD, List<Vector2> points)
    {
        if (points.Count <= 0) return false;

        foreach (var point in points)
        {
            if (!ContainsQuadPoint(qA, qB, qC, qD, point)) return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether a segment is contained within a quadrilateral defined by four vertices.
    /// </summary>
    /// <param name="qA">First vertex of the quad.</param>
    /// <param name="qB">Second vertex of the quad.</param>
    /// <param name="qC">Third vertex of the quad.</param>
    /// <param name="qD">Fourth vertex of the quad.</param>
    /// <param name="segmentStart">Start point of the segment.</param>
    /// <param name="segmentEnd">End point of the segment.</param>
    /// <returns><c>true</c> if the segment is inside the quad; otherwise, <c>false</c>.</returns>
    public static bool ContainsQuadSegment(Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD, Vector2 segmentStart, Vector2 segmentEnd)
    {
        return ContainsQuadPoints(qA, qB, qC, qD, segmentStart, segmentEnd);
    }

    /// <summary>
    /// Determines whether a circle is fully contained within a quadrilateral defined by four vertices.
    /// </summary>
    /// <param name="qA">First vertex of the quad.</param>
    /// <param name="qB">Second vertex of the quad.</param>
    /// <param name="qC">Third vertex of the quad.</param>
    /// <param name="qD">Fourth vertex of the quad.</param>
    /// <param name="circleCenter">Center of the circle.</param>
    /// <param name="circleRadius">Radius of the circle.</param>
    /// <returns><c>true</c> if the circle is fully inside the quad; otherwise, <c>false</c>.</returns>
    /// <remarks>Returns false if the circle center is outside or if any edge intersects the circle.</remarks>
    public static bool ContainsQuadCircle(Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD, Vector2 circleCenter, float circleRadius)
    {
        if (!ContainsQuadPoint(qA, qB, qC, qD, circleCenter)) return false;

        var result = Segment.IntersectSegmentCircle(qA, qB, circleCenter, circleRadius);
        if (result.a.Valid || result.b.Valid) return false;

        result = Segment.IntersectSegmentCircle(qB, qC, circleCenter, circleRadius);
        if (result.a.Valid || result.b.Valid) return false;

        result = Segment.IntersectSegmentCircle(qC, qD, circleCenter, circleRadius);
        if (result.a.Valid || result.b.Valid) return false;

        result = Segment.IntersectSegmentCircle(qD, qA, circleCenter, circleRadius);
        return !result.a.Valid && !result.b.Valid;
    }

    /// <summary>
    /// Determines whether a triangle is contained within a quadrilateral defined by four vertices.
    /// </summary>
    /// <param name="qA">First vertex of the quad.</param>
    /// <param name="qB">Second vertex of the quad.</param>
    /// <param name="qC">Third vertex of the quad.</param>
    /// <param name="qD">Fourth vertex of the quad.</param>
    /// <param name="tA">First vertex of the triangle.</param>
    /// <param name="tB">Second vertex of the triangle.</param>
    /// <param name="tc">Third vertex of the triangle.</param>
    /// <returns><c>true</c> if the triangle is inside the quad; otherwise, <c>false</c>.</returns>
    public static bool ContainsQuadTriangle(Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD, Vector2 tA, Vector2 tB, Vector2 tc)
    {
        return ContainsQuadPoints(qA, qB, qC, qD, tA, tB, tc);
    }

    /// <summary>
    /// Determines whether a quadrilateral is contained within another quadrilateral.
    /// </summary>
    /// <param name="qA1">First vertex of the containing quad.</param>
    /// <param name="qB1">Second vertex of the containing quad.</param>
    /// <param name="qC1">Third vertex of the containing quad.</param>
    /// <param name="qD1">Fourth vertex of the containing quad.</param>
    /// <param name="qA2">First vertex of the contained quad.</param>
    /// <param name="qB2">Second vertex of the contained quad.</param>
    /// <param name="qC2">Third vertex of the contained quad.</param>
    /// <param name="qD2">Fourth vertex of the contained quad.</param>
    /// <returns><c>true</c> if the second quad is inside the first; otherwise, <c>false</c>.</returns>
    public static bool ContainsQuadQuad(Vector2 qA1, Vector2 qB1, Vector2 qC1, Vector2 qD1, Vector2 qA2, Vector2 qB2, Vector2 qC2, Vector2 qD2)
    {
        return ContainsQuadPoints(qA1, qB1, qC1, qD1, qA2, qB2, qC2, qD2);
    }

    /// <summary>
    /// Determines whether a rectangle is contained within a quadrilateral defined by four vertices.
    /// </summary>
    /// <param name="qA">First vertex of the quad.</param>
    /// <param name="qB">Second vertex of the quad.</param>
    /// <param name="qC">Third vertex of the quad.</param>
    /// <param name="qD">Fourth vertex of the quad.</param>
    /// <param name="rA">First vertex of the rectangle.</param>
    /// <param name="rB">Second vertex of the rectangle.</param>
    /// <param name="rC">Third vertex of the rectangle.</param>
    /// <param name="rD">Fourth vertex of the rectangle.</param>
    /// <returns><c>true</c> if the rectangle is inside the quad; otherwise, <c>false</c>.</returns>
    public static bool ContainsQuadRect(Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD, Vector2 rA, Vector2 rB, Vector2 rC, Vector2 rD)
    {
        return ContainsQuadPoints(qA, qB, qC, qD, rA, rB, rC, rD);
    }

    /// <summary>
    /// Determines whether a polyline is contained within a quadrilateral defined by four vertices.
    /// </summary>
    /// <param name="qA">First vertex of the quad.</param>
    /// <param name="qB">Second vertex of the quad.</param>
    /// <param name="qC">Third vertex of the quad.</param>
    /// <param name="qD">Fourth vertex of the quad.</param>
    /// <param name="polyline">The polyline to test.</param>
    /// <returns><c>true</c> if the polyline is inside the quad; otherwise, <c>false</c>.</returns>
    public static bool ContainsQuadPolyline(Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD, List<Vector2> polyline)
    {
        return polyline.Count >= 2 && ContainsQuadPoints(qA, qB, qC, qD, polyline);
    }

    /// <summary>
    /// Determines whether a polygon is contained within a quadrilateral defined by four vertices.
    /// </summary>
    /// <param name="qA">First vertex of the quad.</param>
    /// <param name="qB">Second vertex of the quad.</param>
    /// <param name="qC">Third vertex of the quad.</param>
    /// <param name="qD">Fourth vertex of the quad.</param>
    /// <param name="polygon">The polygon to test.</param>
    /// <returns><c>true</c> if the polygon is inside the quad; otherwise, <c>false</c>.</returns>
    public static bool ContainsQuadPolygon(Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD, List<Vector2> polygon)
    {
        return ContainsQuadPoints(qA, qB, qC, qD, polygon);
    }

}