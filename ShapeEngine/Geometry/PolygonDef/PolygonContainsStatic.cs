using System.Numerics;
using ShapeEngine.Geometry.SegmentDef;

namespace ShapeEngine.Geometry.PolygonDef;


public partial class Polygon
{
    /// <summary>
    /// Determines whether a polygon contains a point.
    /// </summary>
    /// <param name="polygon">The polygon as a list of points.</param>
    /// <param name="p">The point to test.</param>
    /// <returns>True if the point is inside the polygon; otherwise, false.</returns>
    public static bool ContainsPoint(List<Vector2> polygon, Vector2 p)
    {
        var oddNodes = false;
        int num = polygon.Count;
        int j = num - 1;
        for (int i = 0; i < num; i++)
        {
            var vi = polygon[i];
            var vj = polygon[j];
            if (ContainsPointCheck(vi, vj, p)) oddNodes = !oddNodes;
            j = i;
        }

        return oddNodes;
    }
    /// <summary>
    /// Determines whether a polygon contains two points.
    /// </summary>
    /// <param name="polygon">The polygon as a list of points.</param>
    /// <param name="a">First point.</param>
    /// <param name="b">Second point.</param>
    /// <returns>True if both points are inside the polygon; otherwise, false.</returns>
    public static bool ContainsPoints(List<Vector2> polygon, Vector2 a, Vector2 b)
    {
        var oddNodesA = false;
        var oddNodesB = false;
        int num = polygon.Count;
        int j = num - 1;
        for (var i = 0; i < num; i++)
        {
            var vi = polygon[i];
            var vj = polygon[j];
            if (ContainsPointCheck(vi, vj, a)) oddNodesA = !oddNodesA;
            if (ContainsPointCheck(vi, vj, b)) oddNodesB = !oddNodesB;

            j = i;
        }

        return oddNodesA && oddNodesB;
    }
    /// <summary>
    /// Determines whether a polygon contains three points.
    /// </summary>
    /// <param name="polygon">The polygon as a list of points.</param>
    /// <param name="a">First point.</param>
    /// <param name="b">Second point.</param>
    /// <param name="c">Third point.</param>
    /// <returns>True if all points are inside the polygon; otherwise, false.</returns>
    public static bool ContainsPoints(List<Vector2> polygon, Vector2 a, Vector2 b, Vector2 c)
    {
        var oddNodesA = false;
        var oddNodesB = false;
        var oddNodesC = false;
        int num = polygon.Count;
        int j = num - 1;
        for (int i = 0; i < num; i++)
        {
            var vi = polygon[i];
            var vj = polygon[j];
            if (ContainsPointCheck(vi, vj, a)) oddNodesA = !oddNodesA;
            if (ContainsPointCheck(vi, vj, b)) oddNodesB = !oddNodesB;
            if (ContainsPointCheck(vi, vj, c)) oddNodesC = !oddNodesC;

            j = i;
        }

        return oddNodesA && oddNodesB && oddNodesC;
    }
    /// <summary>
    /// Determines whether a polygon contains four points.
    /// </summary>
    /// <param name="polygon">The polygon as a list of points.</param>
    /// <param name="a">First point.</param>
    /// <param name="b">Second point.</param>
    /// <param name="c">Third point.</param>
    /// <param name="d">Fourth point.</param>
    /// <returns>True if all points are inside the polygon; otherwise, false.</returns>
    public static bool ContainsPoints(List<Vector2> polygon, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        var oddNodesA = false;
        var oddNodesB = false;
        var oddNodesC = false;
        var oddNodesD = false;
        int num = polygon.Count;
        int j = num - 1;
        for (int i = 0; i < num; i++)
        {
            var vi = polygon[i];
            var vj = polygon[j];
            if (ContainsPointCheck(vi, vj, a)) oddNodesA = !oddNodesA;
            if (ContainsPointCheck(vi, vj, b)) oddNodesB = !oddNodesB;
            if (ContainsPointCheck(vi, vj, c)) oddNodesC = !oddNodesC;
            if (ContainsPointCheck(vi, vj, d)) oddNodesD = !oddNodesD;

            j = i;
        }

        return oddNodesA && oddNodesB && oddNodesC && oddNodesD;
    }
    /// <summary>
    /// Determines whether a polygon contains all points in a list.
    /// </summary>
    /// <param name="polygon">The polygon as a list of points.</param>
    /// <param name="points">The list of points to test.</param>
    /// <returns>True if all points are inside the polygon; otherwise, false.</returns>
    public static bool ContainsPoints(List<Vector2> polygon, List<Vector2> points)
    {
        if (polygon.Count <= 0 || points.Count <= 0) return false;
        foreach (var p in points)
        {
            if (!ContainsPoint(polygon, p)) return false;
        }

        return true;
    }
    /// <summary>
    /// Determines whether a polygon contains a segment defined by two points.
    /// </summary>
    /// <param name="polygon">The polygon as a list of points.</param>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <returns>True if the segment is fully contained; otherwise, false.</returns>
    public static bool ContainsPolygonSegment(List<Vector2> polygon, Vector2 segmentStart, Vector2 segmentEnd)
    {
        //if the polygon does not contain both segmentStart and segmentEnd points, the segment is not contained in the polygon
        //but even if both segmentStart and segmentEnd points are contained the segment could still go outside the polygon -> therefore the for loop check is neeeded
        //TODO: if there is a performant way to check if a polygon is convex (not self intersecting) than this could be used as an additional easy way out
        // the for loop is only needed for concave polygons!
        if (!ContainsPoints(polygon, segmentStart, segmentEnd)) return false;
        
        for (int i = 0; i < polygon.Count; i++)
        {
            var polyStart = polygon[i];
            var polyEnd = polygon[(i + 1) % polygon.Count];
            if (Segment.IntersectSegmentSegment(segmentStart, segmentEnd, polyStart, polyEnd).Valid)
            {
                return false;
            }
        }

        return true;
    }
    /// <summary>
    /// Determines whether a polygon contains a circle.
    /// </summary>
    /// <param name="polygon">The polygon as a list of points.</param>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <returns>True if the circle is fully contained; otherwise, false.</returns>
    public static bool ContainsPolygonCircle(List<Vector2> polygon, Vector2 circleCenter, float circleRadius)
    {
        if (!ContainsPoint(polygon, circleCenter)) return false;

        for (int i = 0; i < polygon.Count; i++)
        {
            var polyStart = polygon[i];
            var polyEnd = polygon[(i + 1) % polygon.Count];
            var result = Segment.IntersectSegmentCircle(polyStart, polyEnd, circleCenter, circleRadius);
            if (result.a.Valid || result.b.Valid)
            {
                return false;
            }
        }

        return true;
    }
    /// <summary>
    /// Determines whether a polygon contains a triangle defined by three points.
    /// </summary>
    /// <param name="polygon">The polygon as a list of points.</param>
    /// <param name="a">First vertex of the triangle.</param>
    /// <param name="b">Second vertex of the triangle.</param>
    /// <param name="c">Third vertex of the triangle.</param>
    /// <returns>True if the triangle is fully contained; otherwise, false.</returns>
    public static bool ContainsPolygonTriangle(List<Vector2> polygon, Vector2 a, Vector2 b, Vector2 c)
    {
        if (!ContainsPoints(polygon, a, b, c)) return false;

        for (int i = 0; i < polygon.Count; i++)
        {
            var polyStart = polygon[i];
            var polyEnd = polygon[(i + 1) % polygon.Count];
            if (Segment.IntersectSegmentSegment(polyStart, polyEnd, a, b).Valid)
            {
                return false;
            }

            if (Segment.IntersectSegmentSegment(polyStart, polyEnd, b, c).Valid)
            {
                return false;
            }

            if (Segment.IntersectSegmentSegment(polyStart, polyEnd, c, a).Valid)
            {
                return false;
            }
        }

        return true;
    }
    /// <summary>
    /// Determines whether a polygon contains a quad defined by four points.
    /// </summary>
    /// <param name="polygon">The polygon as a list of points.</param>
    /// <param name="a">First vertex of the quad.</param>
    /// <param name="b">Second vertex of the quad.</param>
    /// <param name="c">Third vertex of the quad.</param>
    /// <param name="d">Fourth vertex of the quad.</param>
    /// <returns>True if the quad is fully contained; otherwise, false.</returns>
    public static bool ContainsPolygonQuad(List<Vector2> polygon, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        if (!ContainsPoints(polygon, a, b, c, d)) return false;

        for (int i = 0; i < polygon.Count; i++)
        {
            var polyStart = polygon[i];
            var polyEnd = polygon[(i + 1) % polygon.Count];
            if (Segment.IntersectSegmentSegment(polyStart, polyEnd, a, b).Valid)
            {
                return false;
            }

            if (Segment.IntersectSegmentSegment(polyStart, polyEnd, b, c).Valid)
            {
                return false;
            }

            if (Segment.IntersectSegmentSegment(polyStart, polyEnd, c, d).Valid)
            {
                return false;
            }

            if (Segment.IntersectSegmentSegment(polyStart, polyEnd, d, a).Valid)
            {
                return false;
            }
        }

        return true;
    }
    /// <summary>
    /// Determines whether a polygon contains a rectangle defined by four points.
    /// </summary>
    /// <param name="polygon">The polygon as a list of points.</param>
    /// <param name="a">First vertex of the rectangle.</param>
    /// <param name="b">Second vertex of the rectangle.</param>
    /// <param name="c">Third vertex of the rectangle.</param>
    /// <param name="d">Fourth vertex of the rectangle.</param>
    /// <returns>True if the rectangle is fully contained; otherwise, false.</returns>
    public static bool ContainsPolygonRect(List<Vector2> polygon, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return ContainsPolygonQuad(polygon, a, b, c, d);
    }
    /// <summary>
    /// Determines whether a polygon contains a polyline.
    /// </summary>
    /// <param name="polygon">The polygon as a list of points.</param>
    /// <param name="polyline">The polyline as a list of points.</param>
    /// <returns>True if the polyline is fully contained; otherwise, false.</returns>
    public static bool ContainsPolygonPolyline(List<Vector2> polygon, List<Vector2> polyline)
    {
        if (!ContainsPoints(polygon, polyline)) return false;

        for (int i = 0; i < polygon.Count; i++)
        {
            var polyStart = polygon[i];
            var polyEnd = polygon[(i + 1) % polygon.Count];
            for (int j = 0; j < polyline.Count - 1; j++)
            {
                var polylineStart = polyline[j];
                var polylineEnd = polyline[j + 1];

                if (Segment.IntersectSegmentSegment(polyStart, polyEnd, polylineStart, polylineEnd).Valid)
                {
                    return false;
                }
            }
        }

        return true;
    }
    /// <summary>
    /// Determines whether a polygon contains another polygon.
    /// </summary>
    /// <param name="polygon">The polygon as a list of points.</param>
    /// <param name="other">The other polygon as a list of points.</param>
    /// <returns>True if the other polygon is fully contained; otherwise, false.</returns>
    public static bool ContainsPolygonPolygon(List<Vector2> polygon, List<Vector2> other)
    {
        if (!ContainsPoints(polygon, other)) return false;

        for (int i = 0; i < polygon.Count; i++)
        {
            var polyStart = polygon[i];
            var polyEnd = polygon[(i + 1) % polygon.Count];
            for (int j = 0; j < other.Count; j++)
            {
                var polylineStart = other[j];
                var polylineEnd = other[(j + 1) % other.Count];

                if (Segment.IntersectSegmentSegment(polyStart, polyEnd, polylineStart, polylineEnd).Valid)
                {
                    return false;
                }
            }
        }

        return true;
    }

}