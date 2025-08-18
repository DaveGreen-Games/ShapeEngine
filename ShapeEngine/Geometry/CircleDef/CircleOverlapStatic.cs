using System.Numerics;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.CircleDef;

public readonly partial struct Circle
{
    /// <summary>
    /// Determines whether two circles overlap.
    /// </summary>
    /// <param name="aPos">The center of the first circle.</param>
    /// <param name="aRadius">The radius of the first circle.</param>
    /// <param name="bPos">The center of the second circle.</param>
    /// <param name="bRadius">The radius of the second circle.</param>
    /// <returns><c>true</c> if the circles overlap; otherwise, <c>false</c>.</returns>
    public static bool OverlapCircleCircle(Vector2 aPos, float aRadius, Vector2 bPos, float bRadius)
    {
        if (aRadius <= 0.0f && bRadius > 0.0f) return ContainsCirclePoint(bPos, bRadius, aPos);
        if (bRadius <= 0.0f && aRadius > 0.0f) return ContainsCirclePoint(aPos, aRadius, bPos);
        if (aRadius <= 0.0f && bRadius <= 0.0f) return aPos == bPos;

        float rSum = aRadius + bRadius;
        return (aPos - bPos).LengthSquared() < rSum * rSum;
    }

    /// <summary>
    /// Determines whether a circle overlaps with a segment.
    /// </summary>
    /// <param name="cPos">The center of the circle.</param>
    /// <param name="cRadius">The radius of the circle.</param>
    /// <param name="segStart">The start point of the segment.</param>
    /// <param name="segEnd">The end point of the segment.</param>
    /// <returns><c>true</c> if the circle overlaps with the segment; otherwise, <c>false</c>.</returns>
    public static bool OverlapCircleSegment(Vector2 cPos, float cRadius, Vector2 segStart, Vector2 segEnd)
    {
        if (cRadius <= 0.0f) return Segment.IsPointOnSegment(cPos, segStart, segEnd);
        if (ContainsCirclePoint(cPos, cRadius, segStart)) return true;
        // if (ContainsCirclePoint(cPos, cRadius, segEnd)) return true;

        var d = segEnd - segStart;
        var lc = cPos - segStart;
        var p = lc.Project(d);
        var nearest = segStart + p;

        return
            ContainsCirclePoint(cPos, cRadius, nearest) &&
            p.LengthSquared() <= d.LengthSquared() &&
            Vector2.Dot(p, d) >= 0.0f;
    }

    /// <summary>
    /// Determines whether a circle overlaps with a line.
    /// </summary>
    /// <param name="cPos">The center of the circle.</param>
    /// <param name="cRadius">The radius of the circle.</param>
    /// <param name="linePos">A point on the line.</param>
    /// <param name="lineDir">The direction of the line.</param>
    /// <returns><c>true</c> if the circle overlaps with the line; otherwise, <c>false</c>.</returns>
    public static bool OverlapCircleLine(Vector2 cPos, float cRadius, Vector2 linePos, Vector2 lineDir)
    {
        var lc = cPos - linePos;
        var p = lc.Project(lineDir);
        var nearest = linePos + p;
        return ContainsCirclePoint(cPos, cRadius, nearest);
    }

    /// <summary>
    /// Determines whether a circle overlaps with a ray.
    /// </summary>
    /// <param name="cPos">The center of the circle.</param>
    /// <param name="cRadius">The radius of the circle.</param>
    /// <param name="rayPos">The origin point of the ray.</param>
    /// <param name="rayDir">The direction of the ray.</param>
    /// <returns><c>true</c> if the circle overlaps with the ray; otherwise, <c>false</c>.</returns>
    public static bool OverlapCircleRay(Vector2 cPos, float cRadius, Vector2 rayPos, Vector2 rayDir)
    {
        var w = cPos - rayPos;
        float p = w.X * rayDir.Y - w.Y * rayDir.X;
        if (p < -cRadius || p > cRadius) return false;
        float t = w.X * rayDir.X + w.Y * rayDir.Y;
        if (t < 0.0f)
        {
            float d = w.LengthSquared();
            if (d > cRadius * cRadius) return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether a circle overlaps with a triangle.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <returns><c>true</c> if the circle overlaps with the triangle; otherwise, <c>false</c>.</returns>
    public static bool OverlapCircleTriangle(Vector2 center, float radius, Vector2 a, Vector2 b, Vector2 c)
    {
        if (Triangle.ContainsTrianglePoint(a, b, c, center)) return true;

        if (OverlapCircleSegment(center, radius, a, b)) return true;
        if (OverlapCircleSegment(center, radius, b, c)) return true;
        if (OverlapCircleSegment(center, radius, c, a)) return true;

        return false;
    }

    /// <summary>
    /// Determines whether a circle overlaps with a quad.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="a">The first vertex of the quad.</param>
    /// <param name="b">The second vertex of the quad.</param>
    /// <param name="c">The third vertex of the quad.</param>
    /// <param name="d">The fourth vertex of the quad.</param>
    /// <returns><c>true</c> if the circle overlaps with the quad; otherwise, <c>false</c>.</returns>
    public static bool OverlapCircleQuad(Vector2 center, float radius, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        if (Quad.ContainsQuadPoint(a, b, c, d, center)) return true;

        if (OverlapCircleSegment(center, radius, a, b)) return true;
        if (OverlapCircleSegment(center, radius, b, c)) return true;
        if (OverlapCircleSegment(center, radius, c, d)) return true;
        if (OverlapCircleSegment(center, radius, d, a)) return true;

        return false;
    }

    /// <summary>
    /// Determines whether a circle overlaps with a rectangle.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="a">The top-left corner of the rectangle.</param>
    /// <param name="b">The top-right corner of the rectangle.</param>
    /// <param name="c">The bottom-right corner of the rectangle.</param>
    /// <param name="d">The bottom-left corner of the rectangle.</param>
    /// <returns><c>true</c> if the circle overlaps with the rectangle; otherwise, <c>false</c>.</returns>
    public static bool OverlapCircleRect(Vector2 center, float radius, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return OverlapCircleQuad(center, radius, a, b, c, d);
    }

    /// <summary>
    /// Determines whether a circle overlaps with a polygon.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="points">The vertices of the polygon.</param>
    /// <returns><c>true</c> if the circle overlaps with the polygon; otherwise, <c>false</c>.</returns>
    public static bool OverlapCirclePolygon(Vector2 center, float radius, List<Vector2> points)
    {
        if (points.Count < 3) return false;
        if (Circle.ContainsCirclePoint(center, radius, points[0])) return true;

        var oddNodes = false;
        for (var i = 0; i < points.Count; i++)
        {
            var p1 = points[i];
            var p2 = points[(i + 1) % points.Count];
            if (OverlapCircleSegment(center, radius, p1, p2)) return true;
            if (Polygon.ContainsPointCheck(p1, p2, center)) oddNodes = !oddNodes;
        }

        return oddNodes;
    }

    /// <summary>
    /// Determines whether a circle overlaps with a polyline.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="points">The vertices of the polyline.</param>
    /// <returns><c>true</c> if the circle overlaps with the polyline; otherwise, <c>false</c>.</returns>
    public static bool OverlapCirclePolyline(Vector2 center, float radius, List<Vector2> points)
    {
        if (points.Count < 2) return false;
        for (var i = 0; i < points.Count - 1; i++)
        {
            if (OverlapCircleSegment(center, radius, points[i], points[i + 1])) return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether a circle overlaps with a collection of segments.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="segments">The collection of segments.</param>
    /// <returns><c>true</c> if the circle overlaps with the segments; otherwise, <c>false</c>.</returns>
    public static bool OverlapCircleSegments(Vector2 center, float radius, List<Segment> segments)
    {
        if (segments.Count <= 0) return false;

        foreach (var seg in segments)
        {
            if (OverlapCircleSegment(center, radius, seg.Start, seg.End)) return true;
        }

        return false;
    }

}