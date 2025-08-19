using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.SegmentDef;

public readonly partial struct Segment
{
    /// <summary>
    /// Determines whether two segments overlap (intersect or touch).
    /// </summary>
    /// <param name="aStart">Start point of the first segment.</param>
    /// <param name="aEnd">End point of the first segment.</param>
    /// <param name="bStart">Start point of the second segment.</param>
    /// <param name="bEnd">End point of the second segment.</param>
    /// <returns>True if the segments overlap; otherwise, false.</returns>
    /// <remarks>
    /// Handles both parallel and non-parallel segments.
    /// </remarks>
    public static bool OverlapSegmentSegment(Vector2 aStart, Vector2 aEnd, Vector2 bStart, Vector2 bEnd)
    {
        var axisAPos = aStart;
        var axisADir = aEnd - aStart;
        if (SegmentOnOneSide(axisAPos, axisADir, bStart, bEnd)) return false;

        var axisBPos = bStart;
        var axisBDir = bEnd - bStart;
        if (SegmentOnOneSide(axisBPos, axisBDir, aStart, aEnd)) return false;

        if (axisADir.Parallel(axisBDir))
        {
            var rangeA = ProjectSegment(aStart, aEnd, axisADir);
            var rangeB = ProjectSegment(bStart, bEnd, axisADir);
            return rangeA.OverlapValueRange(rangeB);
        }

        return true;
    }

    /// <summary>
    /// Determines whether a segment overlaps a line.
    /// </summary>
    /// <param name="segmentStart">Start point of the segment.</param>
    /// <param name="segmentEnd">End point of the segment.</param>
    /// <param name="linePos">A point on the line.</param>
    /// <param name="lineDir">The direction vector of the line.</param>
    /// <returns>True if the segment overlaps the line; otherwise, false.</returns>
    public static bool OverlapSegmentLine(Vector2 segmentStart, Vector2 segmentEnd, Vector2 linePos, Vector2 lineDir)
    {
        return !SegmentOnOneSide(linePos, lineDir, segmentStart, segmentEnd);
    }

    /// <summary>
    /// Determines whether a segment overlaps a ray.
    /// </summary>
    /// <param name="segmentStart">Start point of the segment.</param>
    /// <param name="segmentEnd">End point of the segment.</param>
    /// <param name="rayPoint">Origin point of the ray.</param>
    /// <param name="rayDirection">Direction vector of the ray.</param>
    /// <returns>True if the segment overlaps the ray; otherwise, false.</returns>
    public static bool OverlapSegmentRay(Vector2 segmentStart, Vector2 segmentEnd, Vector2 rayPoint, Vector2 rayDirection)
    {
        float denominator = rayDirection.X * (segmentEnd.Y - segmentStart.Y) - rayDirection.Y * (segmentEnd.X - segmentStart.X);

        if (Math.Abs(denominator) < ShapeMath.EpsilonF)
        {
            return false;
        }

        var difference = segmentStart - rayPoint;
        float t = (difference.X * (segmentEnd.Y - segmentStart.Y) - difference.Y * (segmentEnd.X - segmentStart.X)) / denominator;
        float u = (difference.X * rayDirection.Y - difference.Y * rayDirection.X) / denominator;

        if (t >= 0 && u >= 0 && u <= 1)
        {
            return true;
        }

        return false;

        // float denominator = (segmentEnd.X - segmentStart.X) * rayDirection.Y - (segmentEnd.Y - segmentStart.Y) * rayDirection.X;
        //
        // if (Math.Abs(denominator) < ShapeMath.EpsilonF)
        // {
        //     return false;
        // }
        //
        // var difference = segmentStart - rayPoint;
        // float t = (difference.X * rayDirection.Y - difference.Y * rayDirection.X) / denominator;
        // float u = (difference.X * (segmentEnd.Y - segmentStart.Y) - difference.Y * (segmentEnd.X - segmentStart.X)) / denominator;
        //
        // if (u >= 0 && u <= 1 && t >= 0)
        // {
        //     return true;
        // }
        //
        // return false;
    }

    /// <summary>
    /// Determines whether a segment overlaps a circle.
    /// </summary>
    /// <param name="segStart">Start point of the segment.</param>
    /// <param name="segEnd">End point of the segment.</param>
    /// <param name="circlePos">Center of the circle.</param>
    /// <param name="circleRadius">Radius of the circle.</param>
    /// <returns>True if the segment overlaps the circle; otherwise, false.</returns>
    public static bool OverlapSegmentCircle(Vector2 segStart, Vector2 segEnd, Vector2 circlePos, float circleRadius) =>
        Circle.OverlapCircleSegment(circlePos, circleRadius, segStart, segEnd);

    /// <summary>
    /// Determines whether a segment overlaps a triangle.
    /// </summary>
    /// <param name="segStart">Start point of the segment.</param>
    /// <param name="segEnd">End point of the segment.</param>
    /// <param name="a">First vertex of the triangle.</param>
    /// <param name="b">Second vertex of the triangle.</param>
    /// <param name="c">Third vertex of the triangle.</param>
    /// <returns>True if the segment overlaps the triangle; otherwise, false.</returns>
    /// <remarks>
    /// Checks if the segment is inside the triangle or overlaps any of its edges.
    /// </remarks>
    public static bool OverlapSegmentTriangle(Vector2 segStart, Vector2 segEnd, Vector2 a, Vector2 b, Vector2 c)
    {
        if (Triangle.ContainsTrianglePoint(a, b, c, segStart)) return true;

        if (OverlapSegmentSegment(segStart, segEnd, a, b)) return true;

        if (OverlapSegmentSegment(segStart, segEnd, b, c)) return true;

        if (OverlapSegmentSegment(segStart, segEnd, c, a)) return true;

        return false;
    }

    /// <summary>
    /// Determines whether a segment overlaps a quadrilateral.
    /// </summary>
    /// <param name="segStart">Start point of the segment.</param>
    /// <param name="segEnd">End point of the segment.</param>
    /// <param name="a">First vertex of the quad.</param>
    /// <param name="b">Second vertex of the quad.</param>
    /// <param name="c">Third vertex of the quad.</param>
    /// <param name="d">Fourth vertex of the quad.</param>
    /// <returns>True if the segment overlaps the quad; otherwise, false.</returns>
    /// <remarks>
    /// Checks if the segment is inside the quad or overlaps any of its edges.
    /// </remarks>
    public static bool OverlapSegmentQuad(Vector2 segStart, Vector2 segEnd, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        if (Quad.ContainsQuadPoint(a, b, c, d, segStart)) return true;

        if (OverlapSegmentSegment(segStart, segEnd, a, b)) return true;
        if (OverlapSegmentSegment(segStart, segEnd, b, c)) return true;
        if (OverlapSegmentSegment(segStart, segEnd, c, d)) return true;
        if (OverlapSegmentSegment(segStart, segEnd, d, a)) return true;

        return false;
    }

    /// <summary>
    /// Determines whether a segment overlaps a rectangle (alias for quad overlap).
    /// </summary>
    /// <param name="segStart">Start point of the segment.</param>
    /// <param name="segEnd">End point of the segment.</param>
    /// <param name="a">First vertex of the rectangle.</param>
    /// <param name="b">Second vertex of the rectangle.</param>
    /// <param name="c">Third vertex of the rectangle.</param>
    /// <param name="d">Fourth vertex of the rectangle.</param>
    /// <returns>True if the segment overlaps the rectangle; otherwise, false.</returns>
    public static bool OverlapSegmentRect(Vector2 segStart, Vector2 segEnd, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return OverlapSegmentQuad(segStart, segEnd, a, b, c, d);
    }

    /// <summary>
    /// Determines whether a segment overlaps a polygon.
    /// </summary>
    /// <param name="segStart">Start point of the segment.</param>
    /// <param name="segEnd">End point of the segment.</param>
    /// <param name="points">List of polygon vertices.</param>
    /// <returns>True if the segment overlaps the polygon; otherwise, false.</returns>
    /// <remarks>
    /// Checks for overlap with polygon edges and whether the segment start is inside the polygon.
    /// </remarks>
    public static bool OverlapSegmentPolygon(Vector2 segStart, Vector2 segEnd, List<Vector2> points)
    {
        if (points.Count < 3) return false;
        var oddNodes = false;
        for (var i = 0; i < points.Count; i++)
        {
            var p1 = points[i];
            var p2 = points[(i + 1) % points.Count];
            if (OverlapSegmentSegment(segStart, segEnd, p1, p2)) return true;
            if (Polygon.ContainsPointCheck(p1, p2, segStart)) oddNodes = !oddNodes;
        }

        return oddNodes;
    }

    /// <summary>
    /// Determines whether a segment overlaps a polyline.
    /// </summary>
    /// <param name="segStart">Start point of the segment.</param>
    /// <param name="segEnd">End point of the segment.</param>
    /// <param name="points">List of polyline vertices.</param>
    /// <returns>True if the segment overlaps the polyline; otherwise, false.</returns>
    /// <remarks>
    /// Checks for overlap with each polyline segment.
    /// </remarks>
    public static bool OverlapSegmentPolyline(Vector2 segStart, Vector2 segEnd, List<Vector2> points)
    {
        if (points.Count < 2) return false;
        for (var i = 0; i < points.Count - 1; i++)
        {
            if (OverlapSegmentSegment(segStart, segEnd, points[i], points[i + 1])) return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether a segment overlaps any segment in a list of segments.
    /// </summary>
    /// <param name="segStart">Start point of the segment.</param>
    /// <param name="segEnd">End point of the segment.</param>
    /// <param name="segments">List of segments to test against.</param>
    /// <returns>True if the segment overlaps any segment in the list; otherwise, false.</returns>
    public static bool OverlapSegmentSegments(Vector2 segStart, Vector2 segEnd, List<Segment> segments)
    {
        if (segments.Count <= 0) return false;

        foreach (var seg in segments)
        {
            if (OverlapSegmentSegment(segStart, segEnd, seg.Start, seg.End)) return true;
        }

        return false;
    }

}