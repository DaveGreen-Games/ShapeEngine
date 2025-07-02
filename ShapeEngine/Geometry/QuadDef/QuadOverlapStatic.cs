using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.QuadDef;

public readonly partial struct Quad
{
    /// <summary>
    /// Determines if a quad overlaps with a segment.
    /// </summary>
    /// <param name="a">First vertex of the quad.</param>
    /// <param name="b">Second vertex of the quad.</param>
    /// <param name="c">Third vertex of the quad.</param>
    /// <param name="d">Fourth vertex of the quad.</param>
    /// <param name="segmentStart">Start point of the segment.</param>
    /// <param name="segmentEnd">End point of the segment.</param>
    /// <returns>True if the segment overlaps the quad; otherwise, false.</returns>
    public static bool OverlapQuadSegment(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 segmentStart, Vector2 segmentEnd)
    {
        return Segment.OverlapSegmentQuad(segmentStart, segmentEnd, a, b, c, d);
    }

    /// <summary>
    /// Determines if a quad overlaps with a line.
    /// </summary>
    /// <param name="a">First vertex of the quad.</param>
    /// <param name="b">Second vertex of the quad.</param>
    /// <param name="c">Third vertex of the quad.</param>
    /// <param name="d">Fourth vertex of the quad.</param>
    /// <param name="linePoint">A point on the line.</param>
    /// <param name="lineDirection">The direction vector of the line.</param>
    /// <returns>True if the line overlaps the quad; otherwise, false.</returns>
    public static bool OverlapQuadLine(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 linePoint, Vector2 lineDirection)
    {
        return Line.OverlapLineQuad(linePoint, lineDirection, a, b, c, d);
    }

    /// <summary>
    /// Determines if a quad overlaps with a ray.
    /// </summary>
    /// <param name="a">First vertex of the quad.</param>
    /// <param name="b">Second vertex of the quad.</param>
    /// <param name="c">Third vertex of the quad.</param>
    /// <param name="d">Fourth vertex of the quad.</param>
    /// <param name="rayPoint">Origin point of the ray.</param>
    /// <param name="rayDirection">Direction vector of the ray.</param>
    /// <returns>True if the ray overlaps the quad; otherwise, false.</returns>
    public static bool OverlapQuadRay(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 rayPoint, Vector2 rayDirection)
    {
        return Ray.OverlapRayQuad(rayPoint, rayDirection, a, b, c, d);
    }

    /// <summary>
    /// Determines if a quad overlaps with a circle.
    /// </summary>
    /// <param name="a">First vertex of the quad.</param>
    /// <param name="b">Second vertex of the quad.</param>
    /// <param name="c">Third vertex of the quad.</param>
    /// <param name="d">Fourth vertex of the quad.</param>
    /// <param name="circleCenter">Center of the circle.</param>
    /// <param name="circleRadius">Radius of the circle.</param>
    /// <returns>True if the circle overlaps the quad; otherwise, false.</returns>
    public static bool OverlapQuadCircle(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 circleCenter, float circleRadius)
    {
        return Circle.OverlapCircleQuad(circleCenter, circleRadius, a, b, c, d);
    }

    /// <summary>
    /// Determines if a quad overlaps with a triangle.
    /// </summary>
    /// <param name="a">First vertex of the quad.</param>
    /// <param name="b">Second vertex of the quad.</param>
    /// <param name="c">Third vertex of the quad.</param>
    /// <param name="d">Fourth vertex of the quad.</param>
    /// <param name="ta">First vertex of the triangle.</param>
    /// <param name="tb">Second vertex of the triangle.</param>
    /// <param name="tc">Third vertex of the triangle.</param>
    /// <returns>True if the triangle overlaps the quad; otherwise, false.</returns>
    public static bool OverlapQuadTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 ta, Vector2 tb, Vector2 tc)
    {
        return Triangle.OverlapTriangleQuad(ta, tb, tc, a, b, c, d);
    }

    /// <summary>
    /// Determines if two quads overlap.
    /// </summary>
    /// <param name="a">First vertex of the first quad.</param>
    /// <param name="b">Second vertex of the first quad.</param>
    /// <param name="c">Third vertex of the first quad.</param>
    /// <param name="d">Fourth vertex of the first quad.</param>
    /// <param name="qa">First vertex of the second quad.</param>
    /// <param name="qb">Second vertex of the second quad.</param>
    /// <param name="qc">Third vertex of the second quad.</param>
    /// <param name="qd">Fourth vertex of the second quad.</param>
    /// <returns>True if the quads overlap; otherwise, false.</returns>
    public static bool OverlapQuadQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 qa, Vector2 qb, Vector2 qc, Vector2 qd)
    {
        if (ContainsQuadPoint(a, b, c, d, qa)) return true;
        if (ContainsQuadPoint(qa, qb, qc, qd, a)) return true;

        if (Segment.OverlapSegmentSegment(a, b, qa, qb)) return true;
        if (Segment.OverlapSegmentSegment(a, b, qb, qc)) return true;
        if (Segment.OverlapSegmentSegment(a, b, qc, qd)) return true;
        if (Segment.OverlapSegmentSegment(a, b, qd, qa)) return true;
        if (Segment.OverlapSegmentSegment(b, c, qa, qb)) return true;
        if (Segment.OverlapSegmentSegment(b, c, qb, qc)) return true;
        if (Segment.OverlapSegmentSegment(b, c, qc, qd)) return true;
        if (Segment.OverlapSegmentSegment(b, c, qd, qa)) return true;
        if (Segment.OverlapSegmentSegment(c, d, qa, qb)) return true;
        if (Segment.OverlapSegmentSegment(c, d, qb, qc)) return true;
        if (Segment.OverlapSegmentSegment(c, d, qc, qd)) return true;
        if (Segment.OverlapSegmentSegment(c, d, qd, qa)) return true;
        if (Segment.OverlapSegmentSegment(d, a, qa, qb)) return true;
        if (Segment.OverlapSegmentSegment(d, a, qb, qc)) return true;
        if (Segment.OverlapSegmentSegment(d, a, qc, qd)) return true;
        if (Segment.OverlapSegmentSegment(d, a, qd, qa)) return true;

        return false;
    }

    /// <summary>
    /// Determines if a quad overlaps with a rectangle.
    /// </summary>
    /// <param name="a">First vertex of the quad.</param>
    /// <param name="b">Second vertex of the quad.</param>
    /// <param name="c">Third vertex of the quad.</param>
    /// <param name="d">Fourth vertex of the quad.</param>
    /// <param name="ra">First vertex of the rectangle.</param>
    /// <param name="rb">Second vertex of the rectangle.</param>
    /// <param name="rc">Third vertex of the rectangle.</param>
    /// <param name="rd">Fourth vertex of the rectangle.</param>
    /// <returns>True if the rectangle overlaps the quad; otherwise, false.</returns>
    public static bool OverlapQuadRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 ra, Vector2 rb, Vector2 rc, Vector2 rd)
    {
        return OverlapQuadQuad(a, b, c, d, ra, rb, rc, rd);
    }

    /// <summary>
    /// Determines if a quad overlaps with a polygon.
    /// </summary>
    /// <param name="a">First vertex of the quad.</param>
    /// <param name="b">Second vertex of the quad.</param>
    /// <param name="c">Third vertex of the quad.</param>
    /// <param name="d">Fourth vertex of the quad.</param>
    /// <param name="points">List of polygon vertices.</param>
    /// <returns>True if the polygon overlaps the quad; otherwise, false.</returns>
    /// <remarks>Checks for edge intersection and uses the odd-even rule for containment.</remarks>
    public static bool OverlapQuadPolygon(Vector2 a, Vector2 b, Vector2 c, Vector2 d, List<Vector2> points)
    {
        if (points.Count < 3) return false;
        
        var oddNodes = false;
        for (var i = 0; i < points.Count; i++)
        {
            var p1 = points[i];
            var p2 = points[(i + 1) % points.Count];
            if (Segment.OverlapSegmentSegment(a, b, p1, p2)) return true;
            if (Segment.OverlapSegmentSegment(b, c, p1, p2)) return true;
            if (Segment.OverlapSegmentSegment(c, d, p1, p2)) return true;
            if (Segment.OverlapSegmentSegment(d, a, p1, p2)) return true;
            if (Polygon.ContainsPointCheck(p1, p2, a)) oddNodes = !oddNodes;
        }

        return oddNodes;
    }

    /// <summary>
    /// Determines if a quad overlaps with a polyline.
    /// </summary>
    /// <param name="a">First vertex of the quad.</param>
    /// <param name="b">Second vertex of the quad.</param>
    /// <param name="c">Third vertex of the quad.</param>
    /// <param name="d">Fourth vertex of the quad.</param>
    /// <param name="points">List of polyline points.</param>
    /// <returns>True if the polyline overlaps the quad; otherwise, false.</returns>
    public static bool OverlapQuadPolyline(Vector2 a, Vector2 b, Vector2 c, Vector2 d, List<Vector2> points)
    {
        if (points.Count < 2) return false;
        for (var i = 0; i < points.Count - 1; i++)
        {
            var p1 = points[i];
            var p2 = points[i + 1];
            if (Segment.OverlapSegmentSegment(a, b, p1, p2)) return true;
            if (Segment.OverlapSegmentSegment(b, c, p1, p2)) return true;
            if (Segment.OverlapSegmentSegment(c, d, p1, p2)) return true;
            if (Segment.OverlapSegmentSegment(d, a, p1, p2)) return true;
        }

        return false;
    }

    /// <summary>
    /// Determines if a quad overlaps with a set of segments.
    /// </summary>
    /// <param name="a">First vertex of the quad.</param>
    /// <param name="b">Second vertex of the quad.</param>
    /// <param name="c">Third vertex of the quad.</param>
    /// <param name="d">Fourth vertex of the quad.</param>
    /// <param name="segments">List of segments to check for overlap.</param>
    /// <returns>True if any segment overlaps the quad; otherwise, false.</returns>
    public static bool OverlapQuadSegments(Vector2 a, Vector2 b, Vector2 c, Vector2 d, List<Segment> segments)
    {
        if (segments.Count < 3) return false;
        for (var i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];
            if (Segment.OverlapSegmentSegment(a, b, segment.Start, segment.End)) return true;
            if (Segment.OverlapSegmentSegment(b, c, segment.Start, segment.End)) return true;
            if (Segment.OverlapSegmentSegment(c, d, segment.Start, segment.End)) return true;
            if (Segment.OverlapSegmentSegment(d, a, segment.Start, segment.End)) return true;
        }

        return false;
    }

}