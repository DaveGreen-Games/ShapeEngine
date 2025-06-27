using System.Numerics;
using ShapeEngine.Geometry.SegmentDef;

namespace ShapeEngine.Geometry.RectDef;

public readonly partial struct Rect
{
    /// <summary>
    /// Determines whether a point is contained within the rectangle defined by the given top-left and bottom-right corners.
    /// </summary>
    /// <param name="topLeft">The top-left corner of the rectangle.</param>
    /// <param name="bottomRight">The bottom-right corner of the rectangle.</param>
    /// <param name="point">The point to test for containment.</param>
    /// <returns>True if the point is inside the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// The check is inclusive of the rectangle's edges.
    /// </remarks>
    public static bool ContainsRectPoint(Vector2 topLeft, Vector2 bottomRight, Vector2 point)
    {
        var left = topLeft.X;
        var right = bottomRight.X;
        var top = topLeft.Y;
        var bottom = bottomRight.Y;
        return left <= point.X && right >= point.X && top <= point.Y && bottom >= point.Y;
    }

    /// <summary>
    /// Determines whether both points are contained within the rectangle defined by the given top-left and bottom-right corners.
    /// </summary>
    /// <param name="topLeft">The top-left corner of the rectangle.</param>
    /// <param name="bottomRight">The bottom-right corner of the rectangle.</param>
    /// <param name="u">The first point to test.</param>
    /// <param name="v">The second point to test.</param>
    /// <returns>True if both points are inside the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// Both points must be inside or on the edge of the rectangle.
    /// </remarks>
    public static bool ContainsRectPoints(Vector2 topLeft, Vector2 bottomRight, Vector2 u, Vector2 v)
    {
        var left = topLeft.X;
        var right = bottomRight.X;
        var top = topLeft.Y;
        var bottom = bottomRight.Y;

        return left <= u.X && right >= u.X && top <= u.Y && bottom >= u.Y &&
               left <= v.X && right >= v.X && top <= v.Y && bottom >= v.Y;
    }

    /// <summary>
    /// Determines whether all three points are contained within the rectangle defined by the given top-left and bottom-right corners.
    /// </summary>
    /// <param name="topLeft">The top-left corner of the rectangle.</param>
    /// <param name="bottomRight">The bottom-right corner of the rectangle.</param>
    /// <param name="u">The first point to test.</param>
    /// <param name="v">The second point to test.</param>
    /// <param name="w">The third point to test.</param>
    /// <returns>True if all points are inside the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// All points must be inside or on the edge of the rectangle.
    /// </remarks>
    public static bool ContainsRectPoints(Vector2 topLeft, Vector2 bottomRight, Vector2 u, Vector2 v, Vector2 w)
    {
        var left = topLeft.X;
        var right = bottomRight.X;
        var top = topLeft.Y;
        var bottom = bottomRight.Y;

        return left <= u.X && right >= u.X && top <= u.Y && bottom >= u.Y &&
               left <= v.X && right >= v.X && top <= v.Y && bottom >= v.Y &&
               left <= w.X && right >= w.X && top <= w.Y && bottom >= w.Y;
    }

    /// <summary>
    /// Determines whether all four points are contained within the rectangle defined by the given top-left and bottom-right corners.
    /// </summary>
    /// <param name="topLeft">The top-left corner of the rectangle.</param>
    /// <param name="bottomRight">The bottom-right corner of the rectangle.</param>
    /// <param name="u">The first point to test.</param>
    /// <param name="v">The second point to test.</param>
    /// <param name="w">The third point to test.</param>
    /// <param name="x">The fourth point to test.</param>
    /// <returns>True if all points are inside the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// All points must be inside or on the edge of the rectangle.
    /// </remarks>
    public static bool ContainsRectPoints(Vector2 topLeft, Vector2 bottomRight, Vector2 u, Vector2 v, Vector2 w, Vector2 x)
    {
        var left = topLeft.X;
        var right = bottomRight.X;
        var top = topLeft.Y;
        var bottom = bottomRight.Y;

        return left <= u.X && right >= u.X && top <= u.Y && bottom >= u.Y &&
               left <= v.X && right >= v.X && top <= v.Y && bottom >= v.Y &&
               left <= w.X && right >= w.X && top <= w.Y && bottom >= w.Y &&
               left <= x.X && right >= x.X && top <= x.Y && bottom >= x.Y;
    }

    /// <summary>
    /// Determines whether all points in the list are contained within the rectangle defined by the given top-left and bottom-right corners.
    /// </summary>
    /// <param name="topLeft">The top-left corner of the rectangle.</param>
    /// <param name="bottomRight">The bottom-right corner of the rectangle.</param>
    /// <param name="points">The list of points to test.</param>
    /// <returns>True if all points are inside the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// All points must be inside or on the edge of the rectangle. Returns false if the list is empty.
    /// </remarks>
    public static bool ContainsRectPoints(Vector2 topLeft, Vector2 bottomRight, List<Vector2> points)
    {
        if (points.Count <= 0) return false;

        var left = topLeft.X;
        var right = bottomRight.X;
        var top = topLeft.Y;
        var bottom = bottomRight.Y;

        foreach (var point in points)
        {
            var contains = left <= point.X && right >= point.X && top <= point.Y && bottom >= point.Y;
            if (!contains) return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether a segment is fully contained within the rectangle defined by the given top-left and bottom-right corners.
    /// </summary>
    /// <param name="topLeft">The top-left corner of the rectangle.</param>
    /// <param name="bottomRight">The bottom-right corner of the rectangle.</param>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <returns>True if both endpoints are inside the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// Both endpoints must be inside or on the edge of the rectangle.
    /// </remarks>
    public static bool ContainsRectSegment(Vector2 topLeft, Vector2 bottomRight, Vector2 segmentStart, Vector2 segmentEnd)
    {
        return ContainsRectPoints(topLeft, bottomRight, segmentStart, segmentEnd);
    }

    /// <summary>
    /// Determines whether a circle is fully contained within the rectangle defined by the given top-left and bottom-right corners.
    /// </summary>
    /// <param name="topLeft">The top-left corner of the rectangle.</param>
    /// <param name="bottomRight">The bottom-right corner of the rectangle.</param>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <returns>True if the entire circle is inside the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// The circle must not intersect any edge of the rectangle and its center must be inside the rectangle.
    /// </remarks>
    public static bool ContainsRectCircle(Vector2 topLeft, Vector2 bottomRight, Vector2 circleCenter, float circleRadius)
    {
        if (!ContainsRectPoint(topLeft, bottomRight, circleCenter)) return false;
        var a = topLeft;
        var b = new Vector2(topLeft.X, bottomRight.Y);
        var c = bottomRight;
        var d = new Vector2(bottomRight.X, topLeft.Y);
        var result = Segment.IntersectSegmentCircle(a, b, circleCenter, circleRadius);
        if (result.a.Valid || result.b.Valid) return false;

        result = Segment.IntersectSegmentCircle(b, c, circleCenter, circleRadius);
        if (result.a.Valid || result.b.Valid) return false;

        result = Segment.IntersectSegmentCircle(c, d, circleCenter, circleRadius);
        if (result.a.Valid || result.b.Valid) return false;

        result = Segment.IntersectSegmentCircle(d, a, circleCenter, circleRadius);
        if (result.a.Valid || result.b.Valid) return false;
        return true;
    }

    /// <summary>
    /// Determines whether a triangle is fully contained within the rectangle defined by the given top-left and bottom-right corners.
    /// </summary>
    /// <param name="topLeft">The top-left corner of the rectangle.</param>
    /// <param name="bottomRight">The bottom-right corner of the rectangle.</param>
    /// <param name="tA">The first vertex of the triangle.</param>
    /// <param name="tB">The second vertex of the triangle.</param>
    /// <param name="tc">The third vertex of the triangle.</param>
    /// <returns>True if all triangle vertices are inside the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// All triangle vertices must be inside or on the edge of the rectangle.
    /// </remarks>
    public static bool ContainsRectTriangle(Vector2 topLeft, Vector2 bottomRight, Vector2 tA, Vector2 tB, Vector2 tc)
    {
        return ContainsRectPoints(topLeft, bottomRight, tA, tB, tc);
    }

    /// <summary>
    /// Determines whether a quadrilateral is fully contained within the rectangle defined by the given top-left and bottom-right corners.
    /// </summary>
    /// <param name="topLeft">The top-left corner of the rectangle.</param>
    /// <param name="bottomRight">The bottom-right corner of the rectangle.</param>
    /// <param name="qA">The first vertex of the quadrilateral.</param>
    /// <param name="qB">The second vertex of the quadrilateral.</param>
    /// <param name="qC">The third vertex of the quadrilateral.</param>
    /// <param name="qD">The fourth vertex of the quadrilateral.</param>
    /// <returns>True if all quadrilateral vertices are inside the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// All quadrilateral vertices must be inside or on the edge of the rectangle.
    /// </remarks>
    public static bool ContainsRectQuad(Vector2 topLeft, Vector2 bottomRight, Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD)
    {
        return ContainsRectPoints(topLeft, bottomRight, qA, qB, qC, qD);
    }

    /// <summary>
    /// Determines whether a rectangle is fully contained within another rectangle defined by their top-left and bottom-right corners.
    /// </summary>
    /// <param name="topLeft1">The top-left corner of the containing rectangle.</param>
    /// <param name="bottomRight1">The bottom-right corner of the containing rectangle.</param>
    /// <param name="topLeft2">The top-left corner of the rectangle to test.</param>
    /// <param name="bottomRight2">The bottom-right corner of the rectangle to test.</param>
    /// <returns>True if the second rectangle is fully inside the first; otherwise, false.</returns>
    /// <remarks>
    /// All corners of the second rectangle must be inside or on the edge of the first rectangle.
    /// </remarks>
    public static bool ContainsRectRect(Vector2 topLeft1, Vector2 bottomRight1, Vector2 topLeft2, Vector2 bottomRight2)
    {
        var x = topLeft1.X;
        var y = topLeft1.Y;
        var width = bottomRight1.X - x;
        var height = bottomRight1.Y - x;

        var otherX = topLeft2.X;
        var otherY = topLeft2.Y;
        var otherWidth = bottomRight2.X - otherX;
        var otherHeight = bottomRight2.Y - otherY;
        return (x <= otherX) && (otherX + otherWidth <= x + width) &&
               (y <= otherY) && (otherY + otherHeight <= y + height);
    }

    /// <summary>
    /// Determines whether all points of a polyline are fully contained within the rectangle defined by the given top-left and bottom-right corners.
    /// </summary>
    /// <param name="topLeft">The top-left corner of the rectangle.</param>
    /// <param name="bottomRight">The bottom-right corner of the rectangle.</param>
    /// <param name="polyline">The list of points representing the polyline.</param>
    /// <returns>True if all points of the polyline are inside the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// All points must be inside or on the edge of the rectangle. This function is typically used for open shapes.
    /// </remarks>
    public static bool ContainsRectPolyline(Vector2 topLeft, Vector2 bottomRight, List<Vector2> polyline)
    {
        return ContainsRectPoints(topLeft, bottomRight, polyline);
    }

    /// <summary>
    /// Determines whether all points of a polygon are fully contained within the rectangle defined by the given top-left and bottom-right corners.
    /// </summary>
    /// <param name="topLeft">The top-left corner of the rectangle.</param>
    /// <param name="bottomRight">The bottom-right corner of the rectangle.</param>
    /// <param name="polygon">The list of points representing the polygon.</param>
    /// <returns>True if all points of the polygon are inside the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// All points must be inside or on the edge of the rectangle. This function is typically used for closed shapes.
    /// </remarks>
    public static bool ContainsRectPolygon(Vector2 topLeft, Vector2 bottomRight, List<Vector2> polygon)
    {
        return ContainsRectPoints(topLeft, bottomRight, polygon);
    }
}