using System.Numerics;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.TriangleDef;

public readonly partial struct Triangle
{
    /// <summary>
    /// Determines whether a triangle defined by three vertices contains the specified point using static triangle vertices.
    /// </summary>
    /// <param name="tA">The first vertex of the triangle.</param>
    /// <param name="tB">The second vertex of the triangle.</param>
    /// <param name="tC">The third vertex of the triangle.</param>
    /// <param name="point">The point to test for containment.</param>
    /// <returns>True if the point is inside or on the boundary of the triangle; otherwise, false.</returns>
    /// <remarks>
    /// This static method uses the cross product method to determine point containment by checking
    /// that the point is on the same side of all three triangle edges. This assumes the triangle
    /// vertices are in counter-clockwise order for proper containment testing.
    /// </remarks>
    public static bool ContainsTrianglePoint(Vector2 tA, Vector2 tB, Vector2 tC, Vector2 point)
    {
        var ab = tB - tA;
        var bc = tC - tB;
        var ca = tA - tC;

        var ap = point - tA;
        var bp = point - tB;
        var cp = point - tC;

        float c1 = ab.Cross(ap);
        float c2 = bc.Cross(bp);
        float c3 = ca.Cross(cp);

        //can handle CW and CCW
        bool allNonNegative = c1 >= 0f && c2 >= 0f && c3 >= 0f; //CCW order
        bool allNonPositive = c1 <= 0f && c2 <= 0f && c3 <= 0f; //CW order

        return allNonNegative || allNonPositive;
        
        // return c1 >= 0f && c2 >= 0f && c3 >= 0f;
    }

    /// <summary>
    /// Determines whether a triangle defined by three vertices contains both of the specified points.
    /// </summary>
    /// <param name="tA">The first vertex of the triangle.</param>
    /// <param name="tB">The second vertex of the triangle.</param>
    /// <param name="tC">The third vertex of the triangle.</param>
    /// <param name="u">The first point to test for containment.</param>
    /// <param name="v">The second point to test for containment.</param>
    /// <returns>True if both points are inside or on the boundary of the triangle; otherwise, false.</returns>
    /// <remarks>This method returns true only if all specified points are contained within the triangle.</remarks>
    public static bool ContainsTrianglePoints(Vector2 tA, Vector2 tB, Vector2 tC, Vector2 u, Vector2 v)
    {
        return ContainsTrianglePoint(tA, tB, tC, u) &&
               ContainsTrianglePoint(tA, tB, tC, v);
    }

    /// <summary>
    /// Determines whether a triangle defined by three vertices contains all three of the specified points.
    /// </summary>
    /// <param name="tA">The first vertex of the triangle.</param>
    /// <param name="tB">The second vertex of the triangle.</param>
    /// <param name="tC">The third vertex of the triangle.</param>
    /// <param name="u">The first point to test for containment.</param>
    /// <param name="v">The second point to test for containment.</param>
    /// <param name="w">The third point to test for containment.</param>
    /// <returns>True if all three points are inside or on the boundary of the triangle; otherwise, false.</returns>
    /// <remarks>This method returns true only if all specified points are contained within the triangle.</remarks>
    public static bool ContainsTrianglePoints(Vector2 tA, Vector2 tB, Vector2 tC, Vector2 u, Vector2 v, Vector2 w)
    {
        return ContainsTrianglePoint(tA, tB, tC, u) &&
               ContainsTrianglePoint(tA, tB, tC, v) &&
               ContainsTrianglePoint(tA, tB, tC, w);
    }

    /// <summary>
    /// Determines whether a triangle defined by three vertices contains all four of the specified points.
    /// </summary>
    /// <param name="tA">The first vertex of the triangle.</param>
    /// <param name="tB">The second vertex of the triangle.</param>
    /// <param name="tC">The third vertex of the triangle.</param>
    /// <param name="u">The first point to test for containment.</param>
    /// <param name="v">The second point to test for containment.</param>
    /// <param name="w">The third point to test for containment.</param>
    /// <param name="x">The fourth point to test for containment.</param>
    /// <returns>True if all four points are inside or on the boundary of the triangle; otherwise, false.</returns>
    /// <remarks>This method returns true only if all specified points are contained within the triangle.</remarks>
    public static bool ContainsTrianglePoints(Vector2 tA, Vector2 tB, Vector2 tC, Vector2 u, Vector2 v, Vector2 w, Vector2 x)
    {
        return ContainsTrianglePoint(tA, tB, tC, u) &&
               ContainsTrianglePoint(tA, tB, tC, v) &&
               ContainsTrianglePoint(tA, tB, tC, w) &&
               ContainsTrianglePoint(tA, tB, tC, x);
    }

    /// <summary>
    /// Determines whether a triangle defined by three vertices contains all points in the specified collection.
    /// </summary>
    /// <param name="tA">The first vertex of the triangle.</param>
    /// <param name="tB">The second vertex of the triangle.</param>
    /// <param name="tC">The third vertex of the triangle.</param>
    /// <param name="points">The collection of points to test for containment.</param>
    /// <returns>True if all points in the collection are inside or on the boundary of the triangle; otherwise, false.</returns>
    /// <remarks>
    /// This method returns true only if all points in the collection are contained within the triangle.
    /// If the collection is empty, this method returns false.
    /// </remarks>
    public static bool ContainsTrianglePoints(Vector2 tA, Vector2 tB, Vector2 tC, List<Vector2> points)
    {
        if (points.Count <= 0) return false;
        foreach (var point in points)
        {
            if (!ContainsTrianglePoint(tA, tB, tC, point)) return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether a triangle defined by three vertices completely contains the specified line segment.
    /// </summary>
    /// <param name="tA">The first vertex of the triangle.</param>
    /// <param name="tB">The second vertex of the triangle.</param>
    /// <param name="tC">The third vertex of the triangle.</param>
    /// <param name="segmentStart">The start point of the line segment.</param>
    /// <param name="segmentEnd">The end point of the line segment.</param>
    /// <returns>True if both endpoints of the segment are contained within the triangle; otherwise, false.</returns>
    /// <remarks>
    /// This method checks if both the start and end points of the segment are within the triangle.
    /// Since triangles are convex shapes, containment of both endpoints ensures containment of the entire segment.
    /// </remarks>
    public static bool ContainsTriangleSegment(Vector2 tA, Vector2 tB, Vector2 tC, Vector2 segmentStart, Vector2 segmentEnd)
    {
        return ContainsTrianglePoints(tA, tB, tC, segmentStart, segmentEnd);
    }

    /// <summary>
    /// Determines whether a triangle defined by three vertices completely contains the specified circle.
    /// </summary>
    /// <param name="tA">The first vertex of the triangle.</param>
    /// <param name="tB">The second vertex of the triangle.</param>
    /// <param name="tC">The third vertex of the triangle.</param>
    /// <param name="circleCenter">The center point of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <returns>True if the entire circle is contained within the triangle; otherwise, false.</returns>
    /// <remarks>
    /// This method checks if the circle's center is sufficiently far from all triangle edges
    /// such that the entire circle circumference is within the triangle boundaries.
    /// </remarks>
    public static bool ContainsTriangleCircle(Vector2 tA, Vector2 tB, Vector2 tC, Vector2 circleCenter, float circleRadius)
    {
        if (!ContainsTrianglePoint(tA, tB, tC, circleCenter)) return false;

        var result = Segment.IntersectSegmentCircle(tA, tB, circleCenter, circleRadius);
        if (result.a.Valid || result.b.Valid) return false;

        result = Segment.IntersectSegmentCircle(tB, tC, circleCenter, circleRadius);
        if (result.a.Valid || result.b.Valid) return false;

        result = Segment.IntersectSegmentCircle(tC, tA, circleCenter, circleRadius);
        if (result.a.Valid || result.b.Valid) return false;

        return true;
    }

    /// <summary>
    /// Determines whether one triangle completely contains another triangle.
    /// </summary>
    /// <param name="tA1">The first vertex of the containing triangle.</param>
    /// <param name="tB1">The second vertex of the containing triangle.</param>
    /// <param name="tC1">The third vertex of the containing triangle.</param>
    /// <param name="tA2">The first vertex of the triangle to test for containment.</param>
    /// <param name="tB2">The second vertex of the triangle to test for containment.</param>
    /// <param name="tC2">The third vertex of the triangle to test for containment.</param>
    /// <returns>True if all three vertices of the second triangle are contained within the first triangle; otherwise, false.</returns>
    /// <remarks>
    /// Since triangles are convex shapes, containment of all vertices ensures containment of the entire triangle.
    /// </remarks>
    public static bool ContainsTriangleTriangle(Vector2 tA1, Vector2 tB1, Vector2 tC1, Vector2 tA2, Vector2 tB2, Vector2 tC2)
    {
        return ContainsTrianglePoints(tA1, tB1, tC1, tA2, tB2, tC2);
    }

    /// <summary>
    /// Determines whether a triangle defined by three vertices completely contains the specified quadrilateral.
    /// </summary>
    /// <param name="tA">The first vertex of the triangle.</param>
    /// <param name="tB">The second vertex of the triangle.</param>
    /// <param name="tC">The third vertex of the triangle.</param>
    /// <param name="qA">The first vertex of the quadrilateral.</param>
    /// <param name="qB">The second vertex of the quadrilateral.</param>
    /// <param name="qC">The third vertex of the quadrilateral.</param>
    /// <param name="qD">The fourth vertex of the quadrilateral.</param>
    /// <returns>True if all four vertices of the quadrilateral are contained within the triangle; otherwise, false.</returns>
    /// <remarks>
    /// Since triangles are convex shapes, containment of all vertices ensures containment of the entire quadrilateral.
    /// </remarks>
    public static bool ContainsTriangleQuad(Vector2 tA, Vector2 tB, Vector2 tC, Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD)
    {
        return ContainsTrianglePoints(tA, tB, tC, qA, qB, qC, qD);
    }

    /// <summary>
    /// Determines whether a triangle defined by three vertices completely contains the specified rectangle.
    /// </summary>
    /// <param name="tA">The first vertex of the triangle.</param>
    /// <param name="tB">The second vertex of the triangle.</param>
    /// <param name="tC">The third vertex of the triangle.</param>
    /// <param name="rA">The first vertex of the rectangle.</param>
    /// <param name="rB">The second vertex of the rectangle.</param>
    /// <param name="rC">The third vertex of the rectangle.</param>
    /// <param name="rD">The fourth vertex of the rectangle.</param>
    /// <returns>True if all four vertices of the rectangle are contained within the triangle; otherwise, false.</returns>
    /// <remarks>
    /// Since triangles are convex shapes, containment of all vertices ensures containment of the entire rectangle.
    /// </remarks>
    public static bool ContainsTriangleRect(Vector2 tA, Vector2 tB, Vector2 tC, Vector2 rA, Vector2 rB, Vector2 rC, Vector2 rD)
    {
        return ContainsTrianglePoints(tA, tB, tC, rA, rB, rC, rD);
    }

    /// <summary>
    /// Determines whether a triangle defined by three vertices completely contains the specified polyline.
    /// </summary>
    /// <param name="tA">The first vertex of the triangle.</param>
    /// <param name="tB">The second vertex of the triangle.</param>
    /// <param name="tC">The third vertex of the triangle.</param>
    /// <param name="polyline">The collection of vertices that define the polyline.</param>
    /// <returns>True if all vertices of the polyline are contained within the triangle; otherwise, false.</returns>
    /// <remarks>
    /// Since triangles are convex shapes, containment of all vertices ensures containment of the entire polyline.
    /// If the polyline collection is empty, this method returns false.
    /// </remarks>
    public static bool ContainsTrianglePolyline(Vector2 tA, Vector2 tB, Vector2 tC, List<Vector2> polyline)
    {
        return polyline.Count >= 2 && ContainsTrianglePoints(tA, tB, tC, polyline);
    }

    /// <summary>
    /// Determines whether a triangle defined by three vertices completely contains the specified polygon.
    /// </summary>
    /// <param name="tA">The first vertex of the triangle.</param>
    /// <param name="tB">The second vertex of the triangle.</param>
    /// <param name="tC">The third vertex of the triangle.</param>
    /// <param name="polygon">The collection of vertices that define the polygon.</param>
    /// <returns>True if all vertices of the polygon are contained within the triangle; otherwise, false.</returns>
    /// <remarks>
    /// Since triangles are convex shapes, containment of all vertices ensures containment of the entire polygon.
    /// If the polygon collection is empty, this method returns false.
    /// </remarks>
    public static bool ContainsTrianglePolygon(Vector2 tA, Vector2 tB, Vector2 tC, List<Vector2> polygon)
    {
        if (polygon == null || polygon.Count < 3)
            return false;
        return ContainsTrianglePoints(tA, tB, tC, polygon);
    }

}