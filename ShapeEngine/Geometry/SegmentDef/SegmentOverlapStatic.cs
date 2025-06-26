using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.SegmentDef;

public readonly partial struct Segment
{
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

    public static bool OverlapSegmentLine(Vector2 segmentStart, Vector2 segmentEnd, Vector2 linePos, Vector2 lineDir)
    {
        return !SegmentOnOneSide(linePos, lineDir, segmentStart, segmentEnd);
    }

    public static bool OverlapSegmentRay(Vector2 segmentStart, Vector2 segmentEnd, Vector2 rayPoint, Vector2 rayDirection)
    {
        float denominator = rayDirection.X * (segmentEnd.Y - segmentStart.Y) - rayDirection.Y * (segmentEnd.X - segmentStart.X);

        if (Math.Abs(denominator) < 1e-10)
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
        // if (Math.Abs(denominator) < 1e-10)
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

    public static bool OverlapSegmentCircle(Vector2 segStart, Vector2 segEnd, Vector2 circlePos, float circleRadius) =>
        Circle.OverlapCircleSegment(circlePos, circleRadius, segStart, segEnd);

    public static bool OverlapSegmentTriangle(Vector2 segStart, Vector2 segEnd, Vector2 a, Vector2 b, Vector2 c)
    {
        if (Triangle.ContainsTrianglePoint(a, b, c, segStart)) return true;

        if (OverlapSegmentSegment(segStart, segEnd, a, b)) return true;

        if (OverlapSegmentSegment(segStart, segEnd, b, c)) return true;

        if (OverlapSegmentSegment(segStart, segEnd, c, a)) return true;

        return false;
    }

    public static bool OverlapSegmentQuad(Vector2 segStart, Vector2 segEnd, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        if (Quad.ContainsQuadPoint(a, b, c, d, segStart)) return true;

        if (OverlapSegmentSegment(segStart, segEnd, a, b)) return true;
        if (OverlapSegmentSegment(segStart, segEnd, b, c)) return true;
        if (OverlapSegmentSegment(segStart, segEnd, c, d)) return true;
        if (OverlapSegmentSegment(segStart, segEnd, d, a)) return true;

        return false;
    }

    public static bool OverlapSegmentRect(Vector2 segStart, Vector2 segEnd, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return OverlapSegmentQuad(segStart, segEnd, a, b, c, d);
    }

    public static bool OverlapSegmentPolygon(Vector2 segStart, Vector2 segEnd, List<Vector2> points)
    {
        if (points.Count < 3) return false;
        // if (Polygon.ContainsPoints(points, segStart)) return true;
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

    public static bool OverlapSegmentPolyline(Vector2 segStart, Vector2 segEnd, List<Vector2> points)
    {
        if (points.Count < 3) return false;
        for (var i = 0; i < points.Count - 1; i++)
        {
            if (OverlapSegmentSegment(segStart, segEnd, points[i], points[i + 1])) return true;
        }

        return false;
    }

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