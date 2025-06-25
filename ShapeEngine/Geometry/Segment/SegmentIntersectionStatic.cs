using System.Numerics;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.Segment;

public readonly partial struct Segment
{
    public static (CollisionPoint point, float time) IntersectSegmentSegmentInfo(Vector2 segment1Start, Vector2 segment1End, Vector2 segment2Start,
        Vector2 segment2End)
    {
        // Calculate the direction vectors of the segments
        var dir1 = segment1End - segment1Start;
        var dir2 = segment2End - segment2Start;

        float denominator = dir1.X * dir2.Y - dir1.Y * dir2.X;

        // Check if segments are parallel (denominator is zero)
        if (Math.Abs(denominator) < 1e-10)
        {
            return (new(), -1);
        }

        var diff = segment2Start - segment1Start;
        float t = (diff.X * dir2.Y - diff.Y * dir2.X) / denominator;
        float u = (diff.X * dir1.Y - diff.Y * dir1.X) / denominator;

        // Check if the intersection point is within both segments
        if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
        {
            var intersection = segment1Start + t * dir1;

            // Calculate the normal vector as perpendicular to the direction of the first segment
            var normal = new Vector2(-dir1.Y, dir1.X).Normalize();

            return (new CollisionPoint(intersection, normal), t);
        }

        return (new(), -1);
    }

    public static (CollisionPoint point, float time) IntersectSegmentSegmentInfo(Vector2 segment1Start, Vector2 segment1End, Vector2 segment2Start,
        Vector2 segment2End, Vector2 segment2Normal)
    {
        // Calculate the direction vectors of the segments
        var dir1 = segment1End - segment1Start;
        var dir2 = segment2End - segment2Start;

        float denominator = dir1.X * dir2.Y - dir1.Y * dir2.X;

        // Check if segments are parallel (denominator is zero)
        if (Math.Abs(denominator) < 1e-10)
        {
            return (new(), -1);
        }

        var diff = segment2Start - segment1Start;
        float t = (diff.X * dir2.Y - diff.Y * dir2.X) / denominator;
        float u = (diff.X * dir1.Y - diff.Y * dir1.X) / denominator;

        // Check if the intersection point is within both segments
        if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
        {
            var intersection = segment1Start + t * dir1;

            return (new CollisionPoint(intersection, segment2Normal), t);
        }

        return (new(), -1);
    }

    public static (CollisionPoint point, float time) IntersectSegmentLineInfo(Vector2 segmentStart, Vector2 segmentEnd, Vector2 linePoint,
        Vector2 lineDirection)
    {
        float denominator = (segmentEnd.X - segmentStart.X) * lineDirection.Y - (segmentEnd.Y - segmentStart.Y) * lineDirection.X;

        if (Math.Abs(denominator) < 1e-10)
        {
            return (new(), -1);
        }

        var difference = segmentStart - linePoint;
        float t = (difference.X * lineDirection.Y - difference.Y * lineDirection.X) / denominator;
        float u = (difference.X * (segmentEnd.Y - segmentStart.Y) - difference.Y * (segmentEnd.X - segmentStart.X)) / denominator;

        if (u >= 0 && u <= 1)
        {
            var intersection = segmentStart + t * (segmentEnd - segmentStart);
            var normal = new Vector2(-lineDirection.Y, lineDirection.X).Normalize();
            return (new(intersection, normal), t);
        }

        return (new(), -1);
    }

    public static (CollisionPoint point, float time) IntersectSegmentRayInfo(Vector2 segmentStart, Vector2 segmentEnd, Vector2 rayPoint, Vector2 rayDirection)
    {
        float denominator = (segmentEnd.X - segmentStart.X) * rayDirection.Y - (segmentEnd.Y - segmentStart.Y) * rayDirection.X;

        if (Math.Abs(denominator) < 1e-10)
        {
            return (new(), -1);
        }

        var difference = segmentStart - rayPoint;
        float t = (difference.X * rayDirection.Y - difference.Y * rayDirection.X) / denominator;
        float u = (difference.X * (segmentEnd.Y - segmentStart.Y) - difference.Y * (segmentEnd.X - segmentStart.X)) / denominator;

        if (u >= 0 && u <= 1 && t >= 0)
        {
            var intersection = segmentStart + t * (segmentEnd - segmentStart);
            var segmentDirection = (segmentEnd - segmentStart).Normalize();
            var normal = new Vector2(-segmentDirection.Y, segmentDirection.X);
            return (new(intersection, normal), t);
        }

        return (new(), -1);
    }

    public static (CollisionPoint point, float time) IntersectSegmentLineInfo(Vector2 segmentStart, Vector2 segmentEnd, Vector2 linePoint,
        Vector2 lineDirection, Vector2 lineNormal)
    {
        float denominator = (segmentEnd.X - segmentStart.X) * lineDirection.Y - (segmentEnd.Y - segmentStart.Y) * lineDirection.X;

        if (Math.Abs(denominator) < 1e-10)
        {
            return (new(), -1);
        }

        var difference = segmentStart - linePoint;
        float t = (difference.X * lineDirection.Y - difference.Y * lineDirection.X) / denominator;
        float u = (difference.X * (segmentEnd.Y - segmentStart.Y) - difference.Y * (segmentEnd.X - segmentStart.X)) / denominator;

        if (u >= 0 && u <= 1)
        {
            var intersection = segmentStart + t * (segmentEnd - segmentStart);
            return (new(intersection, lineNormal), t);
        }

        return (new(), -1);
    }

    public static (CollisionPoint point, float time) IntersectSegmentRayInfo(Vector2 segmentStart, Vector2 segmentEnd, Vector2 rayPoint, Vector2 rayDirection,
        Vector2 rayNormal)
    {
        float denominator = (segmentEnd.X - segmentStart.X) * rayDirection.Y - (segmentEnd.Y - segmentStart.Y) * rayDirection.X;

        if (Math.Abs(denominator) < 1e-10)
        {
            return (new(), -1);
        }

        var difference = segmentStart - rayPoint;
        float t = (difference.X * rayDirection.Y - difference.Y * rayDirection.X) / denominator;
        float u = (difference.X * (segmentEnd.Y - segmentStart.Y) - difference.Y * (segmentEnd.X - segmentStart.X)) / denominator;

        if (u >= 0 && u <= 1 && t >= 0)
        {
            var intersection = segmentStart + t * (segmentEnd - segmentStart);
            return (new(intersection, rayNormal), t);
        }

        return (new(), -1);
    }

    public static CollisionPoint IntersectSegmentLine(Vector2 segmentStart, Vector2 segmentEnd, Vector2 linePoint, Vector2 lineDirection)
    {
        var result = IntersectSegmentRay(segmentStart, segmentEnd, linePoint, lineDirection);
        if (result.Valid) return result;
        return IntersectSegmentRay(segmentStart, segmentEnd, linePoint, -lineDirection);

        // var segmentDirection = segmentEnd - segmentStart;
        //
        // // Calculate the denominator of the intersection formula
        // float denominator = lineDirection.X * segmentDirection.Y - lineDirection.Y * segmentDirection.X;
        //
        // // Check if lines are parallel (denominator is zero)
        // if (Math.Abs(denominator) < 1e-10)
        // {
        //     return new();
        // }
        //
        // // Calculate the intersection point using parameter t
        // var difference = segmentStart - linePoint;
        // float t = (difference.X * segmentDirection.Y - difference.Y * segmentDirection.X) / denominator;
        //
        // // Calculate the intersection point
        // var intersection = linePoint + t * lineDirection;
        //
        // // Check if the intersection point is within the segment
        // if (Segment.IsPointOnSegment(intersection, segmentStart, segmentEnd))
        // {
        //     var normal = new Vector2(-lineDirection.Y, lineDirection.X).Normalize();
        //     return new(intersection, normal);
        // }
        //
        // return new();


        // float denominator = (segmentEnd.X - segmentStart.X) * lineDirection.Y - (segmentEnd.Y - segmentStart.Y) * lineDirection.X;
        //
        // if (Math.Abs(denominator) < 1e-10)
        // {
        //     return new();
        // }
        //
        // var difference = segmentStart - linePoint;
        // float t = (difference.X * lineDirection.Y - difference.Y * lineDirection.X) / denominator;
        // float u = (difference.X * (segmentEnd.Y - segmentStart.Y) - difference.Y * (segmentEnd.X - segmentStart.X)) / denominator;
        //
        // if (u >= 0 && u <= 1)
        // {
        //     var intersection = segmentStart + t * (segmentEnd - segmentStart);
        //     var normal = new Vector2(-lineDirection.Y, lineDirection.X).Normalize();
        //     return new(intersection, normal);
        // }
        //
        // return new();
    }

    public static CollisionPoint IntersectSegmentRay(Vector2 segmentStart, Vector2 segmentEnd, Vector2 rayPoint, Vector2 rayDirection)
    {
        float denominator = rayDirection.X * (segmentEnd.Y - segmentStart.Y) - rayDirection.Y * (segmentEnd.X - segmentStart.X);

        if (Math.Abs(denominator) < 1e-10)
        {
            return new();
        }

        var difference = segmentStart - rayPoint;
        float t = (difference.X * (segmentEnd.Y - segmentStart.Y) - difference.Y * (segmentEnd.X - segmentStart.X)) / denominator;
        float u = (difference.X * rayDirection.Y - difference.Y * rayDirection.X) / denominator;

        if (t >= 0 && u >= 0 && u <= 1)
        {
            var intersection = rayPoint + t * rayDirection;
            var normal = new Vector2(-rayDirection.Y, rayDirection.X);
            return new(intersection, normal);
        }

        return new();
    }

    public static CollisionPoint IntersectSegmentSegment(Vector2 segment1Start, Vector2 segment1End, Vector2 segment2Start, Vector2 segment2End)
    {
        //OLD VERSION
        // var info = IntersectSegmentSegmentInfo(aStart, aEnd, bStart, bEnd);
        // if (info.intersected)
        // {
        //     return new(info.intersectPoint, GetNormal(bStart, bEnd, false));
        // }
        // return new();
        // Calculate the direction vectors of the segments
        var dir1 = segment1End - segment1Start;
        var dir2 = segment2End - segment2Start;

        float denominator = dir1.X * dir2.Y - dir1.Y * dir2.X;

        // Check if segments are parallel (denominator is zero)
        if (Math.Abs(denominator) < 1e-10)
        {
            return new();
        }

        var diff = segment2Start - segment1Start;
        float t = (diff.X * dir2.Y - diff.Y * dir2.X) / denominator;
        float u = (diff.X * dir1.Y - diff.Y * dir1.X) / denominator;

        // Check if the intersection point is within both segments
        if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
        {
            var intersection = segment1Start + t * dir1;

            // Calculate the normal vector as perpendicular to the direction of the first segment
            var normal = new Vector2(-dir2.Y, dir2.X).Normalize();

            return new CollisionPoint(intersection, normal);
        }

        return new();
    }

    public static CollisionPoint IntersectSegmentLine(Vector2 segmentStart, Vector2 segmentEnd, Vector2 linePoint, Vector2 lineDirection, Vector2 lineNormal)
    {
        var result = IntersectSegmentLine(segmentStart, segmentEnd, linePoint, lineDirection);
        if (result.Valid)
        {
            return new(result.Point, lineNormal);
        }

        return new();
    }

    public static CollisionPoint IntersectSegmentRay(Vector2 segmentStart, Vector2 segmentEnd, Vector2 rayPoint, Vector2 rayDirection, Vector2 rayNormal)
    {
        var result = IntersectSegmentRay(segmentStart, segmentEnd, rayPoint, rayDirection);
        if (result.Valid)
        {
            return new(result.Point, rayNormal);
        }

        return new();
    }

    public static CollisionPoint IntersectSegmentSegment(Vector2 segment1Start, Vector2 segment1End, Vector2 segment2Start, Vector2 segment2End,
        Vector2 segment2Normal)
    {
        var result = IntersectSegmentSegment(segment1Start, segment1End, segment2Start, segment2End);
        if (result.Valid)
        {
            return new(result.Point, segment2Normal);
        }

        return new();
    }

    public static (CollisionPoint a, CollisionPoint b) IntersectSegmentCircle(Vector2 segmentStart, Vector2 segmentEnd, Vector2 circleCenter, float radius)
    {
        CollisionPoint a = new();
        CollisionPoint b = new();

        // Calculate the direction vector of the segment
        var segmentDirection = segmentEnd - segmentStart;
        var toCircle = circleCenter - segmentStart;

        // Projection of toCircle onto the segment direction to find the closest approach
        float projectionLength = Vector2.Dot(toCircle, segmentDirection) / segmentDirection.LengthSquared();
        var closestPoint = segmentStart + projectionLength * segmentDirection;

        // Distance from the closest point to the circle center
        float distanceToCenter = Vector2.Distance(closestPoint, circleCenter);

        if (distanceToCenter < radius)
        {
            // Calculate the distance from the closest point to the intersection points
            float offset = (float)Math.Sqrt(radius * radius - distanceToCenter * distanceToCenter);
            var intersection1 = closestPoint - offset * segmentDirection.Normalize();
            var intersection2 = closestPoint + offset * segmentDirection.Normalize();

            // Check if the intersection points are within the segment
            if (IsPointOnSegment(intersection1, segmentStart, segmentEnd))
            {
                var normal1 = (intersection1 - circleCenter).Normalize();
                a = new CollisionPoint(intersection1, normal1);
            }

            if (IsPointOnSegment(intersection2, segmentStart, segmentEnd))
            {
                var normal2 = (intersection2 - circleCenter).Normalize();
                if (a.Valid) b = new CollisionPoint(intersection2, normal2);
                else a = new CollisionPoint(intersection2, normal2);
                // results.Add((intersection2, normal2));
            }
        }
        else if (Math.Abs(distanceToCenter - radius) < 1e-10)
        {
            // The segment is tangent to the circle
            if (IsPointOnSegment(closestPoint, segmentStart, segmentEnd))
            {
                a = new(closestPoint, (closestPoint - circleCenter).Normalize());
            }
        }

        return (a, b);
    }

    public static (CollisionPoint a, CollisionPoint b) IntersectSegmentTriangle(Vector2 segmentStart, Vector2 segmentEnd, Vector2 a, Vector2 b, Vector2 c)
    {
        CollisionPoint resultA = new();
        CollisionPoint resultB = new();

        var cp = IntersectSegmentSegment(segmentStart, segmentEnd, a, b);
        if (cp.Valid) resultA = cp;

        cp = IntersectSegmentSegment(segmentStart, segmentEnd, b, c);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }

        if (resultA.Valid && resultB.Valid) return (resultA, resultB);

        cp = IntersectSegmentSegment(segmentStart, segmentEnd, c, a);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }

        return (resultA, resultB);
    }

    public static (CollisionPoint a, CollisionPoint b) IntersectSegmentQuad(Vector2 segmentStart, Vector2 segmentEnd, Vector2 a, Vector2 b, Vector2 c,
        Vector2 d)
    {
        CollisionPoint resultA = new();
        CollisionPoint resultB = new();

        var cp = IntersectSegmentSegment(segmentStart, segmentEnd, a, b);
        if (cp.Valid) resultA = cp;

        cp = IntersectSegmentSegment(segmentStart, segmentEnd, b, c);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }

        if (resultA.Valid && resultB.Valid) return (resultA, resultB);

        cp = IntersectSegmentSegment(segmentStart, segmentEnd, c, d);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }

        if (resultA.Valid && resultB.Valid) return (resultA, resultB);

        cp = IntersectSegmentSegment(segmentStart, segmentEnd, d, a);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }

        return (resultA, resultB);
    }

    public static (CollisionPoint a, CollisionPoint b) IntersectSegmentRect(Vector2 segmentStart, Vector2 segmentEnd, Vector2 a, Vector2 b, Vector2 c,
        Vector2 d)
    {
        return IntersectSegmentQuad(segmentStart, segmentEnd, a, b, c, d);
    }

    public static CollisionPoints? IntersectSegmentPolygon(Vector2 segmentStart, Vector2 segmentEnd, List<Vector2> points, int maxCollisionPoints = -1)
    {
        if (points.Count < 3) return null;
        if (maxCollisionPoints == 0) return null;
        CollisionPoints? result = null;
        for (var i = 0; i < points.Count; i++)
        {
            var colPoint = IntersectSegmentSegment(segmentStart, segmentEnd, points[i], points[(i + 1) % points.Count]);
            if (colPoint.Valid)
            {
                result ??= new();
                result.Add(colPoint);
                if (maxCollisionPoints > 0 && result.Count >= maxCollisionPoints) return result;
            }
        }

        return result;
    }

    public static CollisionPoints? IntersectSegmentPolyline(Vector2 segmentStart, Vector2 segmentEnd, List<Vector2> points, int maxCollisionPoints = -1)
    {
        if (points.Count < 3) return null;
        if (maxCollisionPoints == 0) return null;
        CollisionPoints? result = null;
        for (var i = 0; i < points.Count - 1; i++)
        {
            var colPoint = IntersectSegmentSegment(segmentStart, segmentEnd, points[i], points[i + 1]);
            if (colPoint.Valid)
            {
                result ??= new();
                result.Add(colPoint);
                if (maxCollisionPoints > 0 && result.Count >= maxCollisionPoints) return result;
            }
        }

        return result;
    }

    public static CollisionPoints? IntersectSegmentSegments(Vector2 segmentStart, Vector2 segmentEnd, List<Segment> segments, int maxCollisionPoints = -1)
    {
        if (segments.Count <= 0) return null;
        if (maxCollisionPoints == 0) return null;
        CollisionPoints? result = null;

        foreach (var seg in segments)
        {
            var colPoint = IntersectSegmentSegment(segmentStart, segmentEnd, seg.Start, seg.End);
            if (colPoint.Valid)
            {
                result ??= new();
                result.AddRange(colPoint);
                if (maxCollisionPoints > 0 && result.Count >= maxCollisionPoints) return result;
            }
        }

        return result;
    }

}