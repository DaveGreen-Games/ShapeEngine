using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.RectDef;

public readonly partial struct Rect
{
    /// <summary>
    /// Determines if a line segment overlaps with a rectangle defined by four corner points.
    /// </summary>
    /// <param name="a">The first corner of the rectangle (typically top-left).</param>
    /// <param name="b">The second corner of the rectangle (typically top-right).</param>
    /// <param name="c">The third corner of the rectangle (typically bottom-right).</param>
    /// <param name="d">The fourth corner of the rectangle (typically bottom-left).</param>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <returns>True if the segment overlaps the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// Uses the <see cref="Segment.OverlapSegmentQuad"/> method for the calculation.
    /// </remarks>
    public static bool OverlapRectSegment(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 segmentStart, Vector2 segmentEnd)
    {
        return Segment.OverlapSegmentQuad(segmentStart, segmentEnd, a, b, c, d);
    }

    /// <summary>
    /// Determines if a line overlaps with a rectangle defined by four corner points.
    /// </summary>
    /// <param name="a">The first corner of the rectangle.</param>
    /// <param name="b">The second corner of the rectangle.</param>
    /// <param name="c">The third corner of the rectangle.</param>
    /// <param name="d">The fourth corner of the rectangle.</param>
    /// <param name="linePoint">A point on the line.</param>
    /// <param name="lineDirection">The direction vector of the line.</param>
    /// <returns>True if the line overlaps the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// Uses the <see cref="Line.OverlapLineQuad"/> method for the calculation.
    /// </remarks>
    public static bool OverlapRectLine(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 linePoint, Vector2 lineDirection)
    {
        return Line.OverlapLineQuad(linePoint, lineDirection, a, b, c, d);
    }

    /// <summary>
    /// Determines if a ray overlaps with a rectangle defined by four corner points.
    /// </summary>
    /// <param name="a">The first corner of the rectangle.</param>
    /// <param name="b">The second corner of the rectangle.</param>
    /// <param name="c">The third corner of the rectangle.</param>
    /// <param name="d">The fourth corner of the rectangle.</param>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <returns>True if the ray overlaps the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// Uses the <see cref="Ray.OverlapRayQuad"/> method for the calculation.
    /// </remarks>
    public static bool OverlapRectRay(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 rayPoint, Vector2 rayDirection)
    {
        return Ray.OverlapRayQuad(rayPoint, rayDirection, a, b, c, d);
    }

    /// <summary>
    /// Determines if a circle overlaps with a rectangle defined by four corner points.
    /// </summary>
    /// <param name="a">The first corner of the rectangle.</param>
    /// <param name="b">The second corner of the rectangle.</param>
    /// <param name="c">The third corner of the rectangle.</param>
    /// <param name="d">The fourth corner of the rectangle.</param>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <returns>True if the circle overlaps the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// Uses the <see cref="Circle.OverlapCircleQuad"/> method for the calculation.
    /// </remarks>
    public static bool OverlapRectCircle(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 circleCenter, float circleRadius)
    {
        return Circle.OverlapCircleQuad(circleCenter, circleRadius, a, b, c, d);
    }

    /// <summary>
    /// Determines if a triangle overlaps with a rectangle defined by four corner points.
    /// </summary>
    /// <param name="a">The first corner of the rectangle.</param>
    /// <param name="b">The second corner of the rectangle.</param>
    /// <param name="c">The third corner of the rectangle.</param>
    /// <param name="d">The fourth corner of the rectangle.</param>
    /// <param name="ta">The first vertex of the triangle.</param>
    /// <param name="tb">The second vertex of the triangle.</param>
    /// <param name="tc">The third vertex of the triangle.</param>
    /// <returns>True if the triangle overlaps the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// Uses the <see cref="Triangle.OverlapTriangleQuad"/> method for the calculation.
    /// </remarks>
    public static bool OverlapRectTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 ta, Vector2 tb, Vector2 tc)
    {
        return Triangle.OverlapTriangleQuad(ta, tb, tc, a, b, c, d);
    }

    /// <summary>
    /// Determines if a quadrilateral overlaps with a rectangle defined by four corner points.
    /// </summary>
    /// <param name="a">The first corner of the rectangle.</param>
    /// <param name="b">The second corner of the rectangle.</param>
    /// <param name="c">The third corner of the rectangle.</param>
    /// <param name="d">The fourth corner of the rectangle.</param>
    /// <param name="qa">The first corner of the quadrilateral.</param>
    /// <param name="qb">The second corner of the quadrilateral.</param>
    /// <param name="qc">The third corner of the quadrilateral.</param>
    /// <param name="qd">The fourth corner of the quadrilateral.</param>
    /// <returns>True if the quadrilateral overlaps the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// Uses the <see cref="Quad.OverlapQuadQuad"/> method for the calculation.
    /// </remarks>
    public static bool OverlapRectQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 qa, Vector2 qb, Vector2 qc, Vector2 qd)
    {
        return Quad.OverlapQuadQuad(a, b, c, d, qa, qb, qc, qd);
    }

    /// <summary>
    /// Determines if another rectangle overlaps with this rectangle, both defined by four corner points.
    /// </summary>
    /// <param name="a">The first corner of this rectangle.</param>
    /// <param name="b">The second corner of this rectangle.</param>
    /// <param name="c">The third corner of this rectangle.</param>
    /// <param name="d">The fourth corner of this rectangle.</param>
    /// <param name="ra">The first corner of the other rectangle.</param>
    /// <param name="rb">The second corner of the other rectangle.</param>
    /// <param name="rc">The third corner of the other rectangle.</param>
    /// <param name="rd">The fourth corner of the other rectangle.</param>
    /// <returns>True if the rectangles overlap; otherwise, false.</returns>
    /// <remarks>
    /// Uses the <see cref="Quad.OverlapQuadQuad"/> method for the calculation.
    /// </remarks>
    public static bool OverlapRectRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 ra, Vector2 rb, Vector2 rc, Vector2 rd)
    {
        return Quad.OverlapQuadQuad(a, b, c, d, ra, rb, rc, rd);
    }

    /// <summary>
    /// Determines if a polygon overlaps with a rectangle defined by four corner points.
    /// </summary>
    /// <param name="a">The first corner of the rectangle.</param>
    /// <param name="b">The second corner of the rectangle.</param>
    /// <param name="c">The third corner of the rectangle.</param>
    /// <param name="d">The fourth corner of the rectangle.</param>
    /// <param name="points">A list of points representing the polygon.</param>
    /// <returns>True if the polygon overlaps the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// Uses the <see cref="Quad.OverlapQuadPolygon"/> method for the calculation.
    /// </remarks>
    public static bool OverlapRectPolygon(Vector2 a, Vector2 b, Vector2 c, Vector2 d, List<Vector2> points)
    {
        return points.Count >= 3 && Quad.OverlapQuadPolygon(a, b, c, d, points);
    }

    /// <summary>
    /// Determines if a polyline overlaps with a rectangle defined by four corner points.
    /// </summary>
    /// <param name="a">The first corner of the rectangle.</param>
    /// <param name="b">The second corner of the rectangle.</param>
    /// <param name="c">The third corner of the rectangle.</param>
    /// <param name="d">The fourth corner of the rectangle.</param>
    /// <param name="points">A list of points representing the polyline.</param>
    /// <returns>True if the polyline overlaps the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// Uses the <see cref="Quad.OverlapQuadPolyline"/> method for the calculation.
    /// </remarks>
    public static bool OverlapRectPolyline(Vector2 a, Vector2 b, Vector2 c, Vector2 d, List<Vector2> points)
    {
        return points.Count >= 2 && Quad.OverlapQuadPolyline(a, b, c, d, points);
    }

    /// <summary>
    /// Determines if a collection of segments overlaps with a rectangle defined by four corner points.
    /// </summary>
    /// <param name="a">The first corner of the rectangle.</param>
    /// <param name="b">The second corner of the rectangle.</param>
    /// <param name="c">The third corner of the rectangle.</param>
    /// <param name="d">The fourth corner of the rectangle.</param>
    /// <param name="segments">A list of <see cref="Segment"/> objects representing the segments.</param>
    /// <returns>True if any segment overlaps the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// Uses the <see cref="Quad.OverlapQuadSegments"/> method for the calculation.
    /// </remarks>
    public static bool OverlapRectSegments(Vector2 a, Vector2 b, Vector2 c, Vector2 d, List<Segment> segments)
    {
        return Quad.OverlapQuadSegments(a, b, c, d, segments);
    }
}