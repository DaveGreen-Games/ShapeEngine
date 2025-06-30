using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.SegmentDef;

namespace ShapeEngine.Geometry.TriangleDef;

public readonly partial struct Triangle
{
    /// <summary>
    /// Checks if a triangle and a segment overlap.
    /// </summary>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <returns>True if they overlap, false otherwise.</returns>
    public static bool OverlapTriangleSegment(Vector2 a, Vector2 b, Vector2 c, Vector2 segmentStart, Vector2 segmentEnd)
    {
        return Segment.OverlapSegmentTriangle(segmentStart, segmentEnd, a, b, c);
    }

    /// <summary>
    /// Checks if a triangle and a line overlap.
    /// </summary>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <param name="linePoint">A point on the line.</param>
    /// <param name="lineDirection">The direction of the line.</param>
    /// <returns>True if they overlap, false otherwise.</returns>
    public static bool OverlapTriangleLine(Vector2 a, Vector2 b, Vector2 c, Vector2 linePoint, Vector2 lineDirection)
    {
        return Line.OverlapLineTriangle(linePoint, lineDirection, a, b, c);
    }

    /// <summary>
    /// Checks if a triangle and a ray overlap.
    /// </summary>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <param name="rayPoint">The starting point of the ray.</param>
    /// <param name="rayDirection">The direction of the ray.</param>
    /// <returns>True if they overlap, false otherwise.</returns>
    public static bool OverlapTriangleRay(Vector2 a, Vector2 b, Vector2 c, Vector2 rayPoint, Vector2 rayDirection)
    {
        return Ray.OverlapRayTriangle(rayPoint, rayDirection, a, b, c);
    }

    /// <summary>
    /// Checks if a triangle and a circle overlap.
    /// </summary>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <returns>True if they overlap, false otherwise.</returns>
    public static bool OverlapTriangleCircle(Vector2 a, Vector2 b, Vector2 c, Vector2 circleCenter, float circleRadius)
    {
        return Circle.OverlapCircleTriangle(circleCenter, circleRadius, a, b, c);
    }

    /// <summary>
    /// Checks if two triangles overlap.
    /// </summary>
    /// <param name="a1">The first vertex of the first triangle.</param>
    /// <param name="b1">The second vertex of the first triangle.</param>
    /// <param name="c1">The third vertex of the first triangle.</param>
    /// <param name="a2">The first vertex of the second triangle.</param>
    /// <param name="b2">The second vertex of the second triangle.</param>
    /// <param name="c2">The third vertex of the second triangle.</param>
    /// <returns>True if they overlap, false otherwise.</returns>
    public static bool OverlapTriangleTriangle(Vector2 a1, Vector2 b1, Vector2 c1, Vector2 a2, Vector2 b2, Vector2 c2)
    {
        if (ContainsTrianglePoint(a1, b1, c1, a2)) return true;
        if (ContainsTrianglePoint(a2, b2, c2, a1)) return true;

        if (Segment.OverlapSegmentSegment(a1, b1, a2, b2)) return true;
        if (Segment.OverlapSegmentSegment(a1, b1, b2, c2)) return true;
        if (Segment.OverlapSegmentSegment(a1, b1, c2, a2)) return true;

        if (Segment.OverlapSegmentSegment(b1, c1, a2, b2)) return true;
        if (Segment.OverlapSegmentSegment(b1, c1, b2, c2)) return true;
        if (Segment.OverlapSegmentSegment(b1, c1, c2, a2)) return true;

        if (Segment.OverlapSegmentSegment(c1, a1, a2, b2)) return true;
        if (Segment.OverlapSegmentSegment(c1, a1, b2, c2)) return true;
        if (Segment.OverlapSegmentSegment(c1, a1, c2, a2)) return true;

        return false;
    }

    /// <summary>
    /// Checks if a triangle and a quad overlap.
    /// </summary>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <param name="qa">The first vertex of the quad.</param>
    /// <param name="qb">The second vertex of the quad.</param>
    /// <param name="qc">The third vertex of the quad.</param>
    /// <param name="qd">The fourth vertex of the quad.</param>
    /// <returns>True if they overlap, false otherwise.</returns>
    public static bool OverlapTriangleQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 qa, Vector2 qb, Vector2 qc, Vector2 qd)
    {
        if (ContainsTrianglePoint(a, b, c, qa)) return true;
        if (Quad.ContainsQuadPoint(qa, qb, qc, qd, a)) return true;

        if (Segment.OverlapSegmentSegment(a, b, qa, qb)) return true;
        if (Segment.OverlapSegmentSegment(a, b, qb, qc)) return true;
        if (Segment.OverlapSegmentSegment(a, b, qc, qd)) return true;
        if (Segment.OverlapSegmentSegment(a, b, qd, qa)) return true;

        if (Segment.OverlapSegmentSegment(b, c, qa, qb)) return true;
        if (Segment.OverlapSegmentSegment(b, c, qb, qc)) return true;
        if (Segment.OverlapSegmentSegment(b, c, qc, qd)) return true;
        if (Segment.OverlapSegmentSegment(b, c, qd, qa)) return true;

        if (Segment.OverlapSegmentSegment(c, a, qa, qb)) return true;
        if (Segment.OverlapSegmentSegment(c, a, qb, qc)) return true;
        if (Segment.OverlapSegmentSegment(c, a, qc, qd)) return true;
        if (Segment.OverlapSegmentSegment(c, a, qd, qa)) return true;

        return false;
    }

    /// <summary>
    /// Checks if a triangle and a rectangle overlap.
    /// </summary>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <param name="ra">The first vertex of the rectangle.</param>
    /// <param name="rb">The second vertex of the rectangle.</param>
    /// <param name="rc">The third vertex of the rectangle.</param>
    /// <param name="rd">The fourth vertex of the rectangle.</param>
    /// <returns>True if they overlap, false otherwise.</returns>
    public static bool OverlapTriangleRect(Vector2 a, Vector2 b, Vector2 c, Vector2 ra, Vector2 rb, Vector2 rc, Vector2 rd)
    {
        return OverlapTriangleQuad(a, b, c, ra, rb, rc, rd);
    }

    /// <summary>
    /// Checks if a triangle and a polygon overlap.
    /// </summary>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <param name="points">The vertices of the polygon.</param>
    /// <returns>True if they overlap, false otherwise.</returns>
    public static bool OverlapTrianglePolygon(Vector2 a, Vector2 b, Vector2 c, List<Vector2> points)
    {
        if (points.Count < 3) return false;
        if (ContainsTrianglePoint(a, b, c, points[0])) return true;

        var oddNodes = false;
        for (var i = 0; i < points.Count; i++)
        {
            var p1 = points[i];
            var p2 = points[(i + 1) % points.Count];
            if (Segment.OverlapSegmentSegment(a, b, p1, p2)) return true;
            if (Segment.OverlapSegmentSegment(b, c, p1, p2)) return true;
            if (Segment.OverlapSegmentSegment(c, a, p1, p2)) return true;

            if (Polygon.ContainsPointCheck(p1, p2, a)) oddNodes = !oddNodes;
        }

        return oddNodes;
    }

    /// <summary>
    /// Checks if a triangle and a polyline overlap.
    /// </summary>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <param name="points">The vertices of the polyline.</param>
    /// <returns>True if they overlap, false otherwise.</returns>
    public static bool OverlapTrianglePolyline(Vector2 a, Vector2 b, Vector2 c, List<Vector2> points)
    {
        if (points.Count < 2) return false;
        for (var i = 0; i < points.Count - 1; i++)
        {
            var p1 = points[i];
            var p2 = points[i + 1];
            if (Segment.OverlapSegmentSegment(a, b, p1, p2)) return true;
            if (Segment.OverlapSegmentSegment(b, c, p1, p2)) return true;
            if (Segment.OverlapSegmentSegment(c, a, p1, p2)) return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if a triangle and a list of segments overlap.
    /// </summary>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <param name="segments">The list of segments.</param>
    /// <returns>True if they overlap, false otherwise.</returns>
    public static bool OverlapTriangleSegments(Vector2 a, Vector2 b, Vector2 c, List<Segment> segments)
    {
        if (segments.Count < 3) return false;
        for (var i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];
            if (Segment.OverlapSegmentSegment(a, b, segment.Start, segment.End)) return true;
            if (Segment.OverlapSegmentSegment(b, c, segment.Start, segment.End)) return true;
            if (Segment.OverlapSegmentSegment(c, a, segment.Start, segment.End)) return true;
        }

        return false;
    }

}