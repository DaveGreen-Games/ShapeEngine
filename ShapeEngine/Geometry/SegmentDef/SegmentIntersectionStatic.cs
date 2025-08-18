using System.Numerics;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.SegmentDef;

public readonly partial struct Segment
{
    /// <summary>
    /// Computes the intersection point and time between two segments.
    /// </summary>
    /// <param name="segment1Start">The start point of the first segment.</param>
    /// <param name="segment1End">The end point of the first segment.</param>
    /// <param name="segment2Start">The start point of the second segment.</param>
    /// <param name="segment2End">The end point of the second segment.</param>
    /// <returns>A tuple containing the <see cref="IntersectionPoint"/> and the intersection time along the first segment. If there is no intersection, the intersection point is invalid and the time is -1.</returns>
    public static (IntersectionPoint point, float time) IntersectSegmentSegmentInfo(Vector2 segment1Start, Vector2 segment1End, Vector2 segment2Start,
        Vector2 segment2End)
    {
        // Calculate the direction vectors of the segments
        var dir1 = segment1End - segment1Start;
        var dir2 = segment2End - segment2Start;

        float denominator = dir1.X * dir2.Y - dir1.Y * dir2.X;

        // Check if segments are parallel (denominator is zero)
        if (Math.Abs(denominator) < ShapeMath.EpsilonF)
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

            return (new IntersectionPoint(intersection, normal), t);
        }

        return (new(), -1);
    }

    /// <summary>
    /// Computes the intersection point and time between two segments, using a provided normal for the second segment.
    /// </summary>
    /// <param name="segment1Start">The start point of the first segment.</param>
    /// <param name="segment1End">The end point of the first segment.</param>
    /// <param name="segment2Start">The start point of the second segment.</param>
    /// <param name="segment2End">The end point of the second segment.</param>
    /// <param name="segment2Normal">The normal vector to use for the second segment.</param>
    /// <returns>A tuple containing the <see cref="IntersectionPoint"/> and the intersection time along the first segment. If there is no intersection, the intersection point is invalid and the time is -1.</returns>
    public static (IntersectionPoint point, float time) IntersectSegmentSegmentInfo(Vector2 segment1Start, Vector2 segment1End, Vector2 segment2Start,
        Vector2 segment2End, Vector2 segment2Normal)
    {
        // Calculate the direction vectors of the segments
        var dir1 = segment1End - segment1Start;
        var dir2 = segment2End - segment2Start;

        float denominator = dir1.X * dir2.Y - dir1.Y * dir2.X;

        // Check if segments are parallel (denominator is zero)
        if (Math.Abs(denominator) < ShapeMath.EpsilonF)
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

            return (new IntersectionPoint(intersection, segment2Normal), t);
        }

        return (new(), -1);
    }

    /// <summary>
    /// Computes the intersection point and time between a segment and a line.
    /// </summary>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <param name="linePoint">A point on the line.</param>
    /// <param name="lineDirection">The direction vector of the line.</param>
    /// <returns>A tuple containing the <see cref="IntersectionPoint"/> and the intersection time along the segment. If there is no intersection, the intersection point is invalid and the time is -1.</returns>
    public static (IntersectionPoint point, float time) IntersectSegmentLineInfo(Vector2 segmentStart, Vector2 segmentEnd, Vector2 linePoint,
        Vector2 lineDirection)
    {
        float denominator = (segmentEnd.X - segmentStart.X) * lineDirection.Y - (segmentEnd.Y - segmentStart.Y) * lineDirection.X;

        if (Math.Abs(denominator) < ShapeMath.EpsilonF)
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

    /// <summary>
    /// Computes the intersection point and time between a segment and a ray.
    /// </summary>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <param name="rayPoint">The origin of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <returns>A tuple containing the <see cref="IntersectionPoint"/> and the intersection time along the segment. If there is no intersection, the intersection point is invalid and the time is -1.</returns>
    public static (IntersectionPoint point, float time) IntersectSegmentRayInfo(Vector2 segmentStart, Vector2 segmentEnd, Vector2 rayPoint, Vector2 rayDirection)
    {
        float denominator = (segmentEnd.X - segmentStart.X) * rayDirection.Y - (segmentEnd.Y - segmentStart.Y) * rayDirection.X;

        if (Math.Abs(denominator) < ShapeMath.EpsilonF)
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

    /// <summary>
    /// Computes the intersection point and time between a segment and a line, using a provided normal for the line.
    /// </summary>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <param name="linePoint">A point on the line.</param>
    /// <param name="lineDirection">The direction vector of the line.</param>
    /// <param name="lineNormal">The normal vector to use for the line.</param>
    /// <returns>A tuple containing the <see cref="IntersectionPoint"/> and the intersection time along the segment. If there is no intersection, the intersection point is invalid and the time is -1.</returns>
    public static (IntersectionPoint point, float time) IntersectSegmentLineInfo(Vector2 segmentStart, Vector2 segmentEnd, Vector2 linePoint,
        Vector2 lineDirection, Vector2 lineNormal)
    {
        float denominator = (segmentEnd.X - segmentStart.X) * lineDirection.Y - (segmentEnd.Y - segmentStart.Y) * lineDirection.X;

        if (Math.Abs(denominator) < ShapeMath.EpsilonF)
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

    /// <summary>
    /// Computes the intersection point and time between a segment and a ray, using a provided normal for the ray.
    /// </summary>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <param name="rayPoint">The origin of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="rayNormal">The normal vector to use for the ray.</param>
    /// <returns>A tuple containing the <see cref="IntersectionPoint"/> and the intersection time along the segment. If there is no intersection, the intersection point is invalid and the time is -1.</returns>
    public static (IntersectionPoint point, float time) IntersectSegmentRayInfo(Vector2 segmentStart, Vector2 segmentEnd, Vector2 rayPoint, Vector2 rayDirection,
        Vector2 rayNormal)
    {
        float denominator = (segmentEnd.X - segmentStart.X) * rayDirection.Y - (segmentEnd.Y - segmentStart.Y) * rayDirection.X;

        if (Math.Abs(denominator) < ShapeMath.EpsilonF)
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
    /// <summary>
    /// Computes the intersection point between a segment and a line.
    /// </summary>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <param name="linePoint">A point on the line.</param>
    /// <param name="lineDirection">The direction vector of the line.</param>
    /// <returns>A <see cref="IntersectionPoint"/> representing the intersection, or an invalid point if there is no intersection.</returns>
    public static IntersectionPoint IntersectSegmentLine(Vector2 segmentStart, Vector2 segmentEnd, Vector2 linePoint, Vector2 lineDirection)
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
        // if (Math.Abs(denominator) < ShapeMath.EpsilonF)
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
        // if (Math.Abs(denominator) < ShapeMath.EpsilonF)
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

    /// <summary>
    /// Computes the intersection point between a segment and a ray.
    /// </summary>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <param name="rayPoint">The origin of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <returns>A <see cref="IntersectionPoint"/> representing the intersection, or an invalid point if there is no intersection.</returns>
    public static IntersectionPoint IntersectSegmentRay(Vector2 segmentStart, Vector2 segmentEnd, Vector2 rayPoint, Vector2 rayDirection)
    {
        float denominator = rayDirection.X * (segmentEnd.Y - segmentStart.Y) - rayDirection.Y * (segmentEnd.X - segmentStart.X);

        if (Math.Abs(denominator) < ShapeMath.EpsilonF)
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

    /// <summary>
    /// Computes the intersection point between two segments.
    /// </summary>
    /// <param name="segment1Start">The start point of the first segment.</param>
    /// <param name="segment1End">The end point of the first segment.</param>
    /// <param name="segment2Start">The start point of the second segment.</param>
    /// <param name="segment2End">The end point of the second segment.</param>
    /// <returns>A <see cref="IntersectionPoint"/> representing the intersection, or an invalid point if there is no intersection.</returns>
    public static IntersectionPoint IntersectSegmentSegment(Vector2 segment1Start, Vector2 segment1End, Vector2 segment2Start, Vector2 segment2End)
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
        if (Math.Abs(denominator) < ShapeMath.EpsilonF)
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

            return new IntersectionPoint(intersection, normal);
        }

        return new();
    }

    /// <summary>
    /// Computes the intersection point between a segment and a line, using a provided normal for the line.
    /// </summary>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <param name="linePoint">A point on the line.</param>
    /// <param name="lineDirection">The direction vector of the line.</param>
    /// <param name="lineNormal">The normal vector to use for the line.</param>
    /// <returns>A <see cref="IntersectionPoint"/> representing the intersection, or an invalid point if there is no intersection.</returns>
    public static IntersectionPoint IntersectSegmentLine(Vector2 segmentStart, Vector2 segmentEnd, Vector2 linePoint, Vector2 lineDirection, Vector2 lineNormal)
    {
        var result = IntersectSegmentLine(segmentStart, segmentEnd, linePoint, lineDirection);
        if (result.Valid)
        {
            return new(result.Point, lineNormal);
        }

        return new();
    }

    /// <summary>
    /// Computes the intersection point between a segment and a ray, using a provided normal for the ray.
    /// </summary>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <param name="rayPoint">The origin of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="rayNormal">The normal vector to use for the ray.</param>
    /// <returns>A <see cref="IntersectionPoint"/> representing the intersection, or an invalid point if there is no intersection.</returns>
    public static IntersectionPoint IntersectSegmentRay(Vector2 segmentStart, Vector2 segmentEnd, Vector2 rayPoint, Vector2 rayDirection, Vector2 rayNormal)
    {
        var result = IntersectSegmentRay(segmentStart, segmentEnd, rayPoint, rayDirection);
        if (result.Valid)
        {
            return new(result.Point, rayNormal);
        }

        return new();
    }

    /// <summary>
    /// Computes the intersection point between two segments, using a provided normal for the second segment.
    /// </summary>
    /// <param name="segment1Start">The start point of the first segment.</param>
    /// <param name="segment1End">The end point of the first segment.</param>
    /// <param name="segment2Start">The start point of the second segment.</param>
    /// <param name="segment2End">The end point of the second segment.</param>
    /// <param name="segment2Normal">The normal vector to use for the second segment.</param>
    /// <returns>A <see cref="IntersectionPoint"/> representing the intersection, or an invalid point if there is no intersection.</returns>
    public static IntersectionPoint IntersectSegmentSegment(Vector2 segment1Start, Vector2 segment1End, Vector2 segment2Start, Vector2 segment2End,
        Vector2 segment2Normal)
    {
        var result = IntersectSegmentSegment(segment1Start, segment1End, segment2Start, segment2End);
        if (result.Valid)
        {
            return new(result.Point, segment2Normal);
        }

        return new();
    }

    /// <summary>
    /// Computes the intersection points between a segment and a circle.
    /// </summary>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <returns>A tuple of <see cref="IntersectionPoint"/> representing the intersection points (a, b).</returns>
    public static (IntersectionPoint a, IntersectionPoint b) IntersectSegmentCircle(Vector2 segmentStart, Vector2 segmentEnd, Vector2 circleCenter, float radius)
    {
        IntersectionPoint a = new();
        IntersectionPoint b = new();

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
                a = new IntersectionPoint(intersection1, normal1);
            }

            if (IsPointOnSegment(intersection2, segmentStart, segmentEnd))
            {
                var normal2 = (intersection2 - circleCenter).Normalize();
                if (a.Valid) b = new IntersectionPoint(intersection2, normal2);
                else a = new IntersectionPoint(intersection2, normal2);
                // results.Add((intersection2, normal2));
            }
        }
        else if (Math.Abs(distanceToCenter - radius) < ShapeMath.EpsilonF)
        {
            // The segment is tangent to the circle
            if (IsPointOnSegment(closestPoint, segmentStart, segmentEnd))
            {
                a = new(closestPoint, (closestPoint - circleCenter).Normalize());
            }
        }

        return (a, b);
    }
    /// <summary>
    /// Computes the intersection points between a segment and a triangle.
    /// </summary>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <param name="a">First vertex of the triangle.</param>
    /// <param name="b">Second vertex of the triangle.</param>
    /// <param name="c">Third vertex of the triangle.</param>
    /// <returns>A tuple of <see cref="IntersectionPoint"/> representing the intersection points (a, b).</returns>
    public static (IntersectionPoint a, IntersectionPoint b) IntersectSegmentTriangle(Vector2 segmentStart, Vector2 segmentEnd, Vector2 a, Vector2 b, Vector2 c)
    {
        IntersectionPoint resultA = new();
        IntersectionPoint resultB = new();

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

    /// <summary>
    /// Computes the intersection points between a segment and a quadrilateral.
    /// </summary>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <param name="a">First vertex of the quad.</param>
    /// <param name="b">Second vertex of the quad.</param>
    /// <param name="c">Third vertex of the quad.</param>
    /// <param name="d">Fourth vertex of the quad.</param>
    /// <returns>A tuple of <see cref="IntersectionPoint"/> representing the intersection points (a, b).</returns>
    public static (IntersectionPoint a, IntersectionPoint b) IntersectSegmentQuad(Vector2 segmentStart, Vector2 segmentEnd, Vector2 a, Vector2 b, Vector2 c,
        Vector2 d)
    {
        IntersectionPoint resultA = new();
        IntersectionPoint resultB = new();

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

    /// <summary>
    /// Computes the intersection points between a segment and a rectangle.
    /// </summary>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <param name="a">First vertex of the rectangle.</param>
    /// <param name="b">Second vertex of the rectangle.</param>
    /// <param name="c">Third vertex of the rectangle.</param>
    /// <param name="d">Fourth vertex of the rectangle.</param>
    /// <returns>A tuple of <see cref="IntersectionPoint"/> representing the intersection points (a, b).</returns>
    public static (IntersectionPoint a, IntersectionPoint b) IntersectSegmentRect(Vector2 segmentStart, Vector2 segmentEnd, Vector2 a, Vector2 b, Vector2 c,
        Vector2 d)
    {
        return IntersectSegmentQuad(segmentStart, segmentEnd, a, b, c, d);
    }

    /// <summary>
    /// Computes all intersection points between a segment and a polygon.
    /// </summary>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <param name="points">The list of polygon vertices.</param>
    /// <param name="maxCollisionPoints">The maximum number of collision points to return. -1 for unlimited.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection, or null if there are no intersections.</returns>
    public static IntersectionPoints? IntersectSegmentPolygon(Vector2 segmentStart, Vector2 segmentEnd, List<Vector2> points, int maxCollisionPoints = -1)
    {
        if (points.Count < 3) return null;
        if (maxCollisionPoints == 0) return null;
        IntersectionPoints? result = null;
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

    /// <summary>
    /// Computes all intersection points between a segment and a polyline.
    /// </summary>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <param name="points">The list of polyline vertices.</param>
    /// <param name="maxCollisionPoints">The maximum number of collision points to return. -1 for unlimited.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection, or null if there are no intersections.</returns>
    public static IntersectionPoints? IntersectSegmentPolyline(Vector2 segmentStart, Vector2 segmentEnd, List<Vector2> points, int maxCollisionPoints = -1)
    {
        if (points.Count < 2) return null;
        if (maxCollisionPoints == 0) return null;
        IntersectionPoints? result = null;
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

    /// <summary>
    /// Computes all intersection points between a segment and a set of segments.
    /// </summary>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <param name="segments">The set of segments to test against.</param>
    /// <param name="maxCollisionPoints">The maximum number of collision points to return. -1 for unlimited.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection, or null if there are no intersections.</returns>
    public static IntersectionPoints? IntersectSegmentSegments(Vector2 segmentStart, Vector2 segmentEnd, List<Segment> segments, int maxCollisionPoints = -1)
    {
        if (segments.Count <= 0) return null;
        if (maxCollisionPoints == 0) return null;
        IntersectionPoints? result = null;

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
