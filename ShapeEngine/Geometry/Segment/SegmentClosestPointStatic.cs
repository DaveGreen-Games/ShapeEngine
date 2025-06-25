using System.Numerics;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.Segment;

public readonly partial struct Segment
{
    public static Vector2 GetClosestPointSegmentPoint(Vector2 segmentStart, Vector2 segmentEnd, Vector2 p, out float disSquared)
    {
        var w = (segmentEnd - segmentStart);
        float t = (p - segmentStart).Dot(w) / w.LengthSquared();
        if (t < 0f)
        {
            disSquared = (p - segmentStart).LengthSquared();
            disSquared = ShapeMath.ClampToZero(disSquared);
            return segmentStart;
        }

        if (t > 1f)
        {
            disSquared = (p - segmentEnd).LengthSquared();
            disSquared = ShapeMath.ClampToZero(disSquared);
            return segmentEnd;
        }

        var result = segmentStart + w * t;
        disSquared = (p - result).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
        return result;
    }

    public static (Vector2 self, Vector2 other) GetClosestPointSegmentSegment(Vector2 segment1Start, Vector2 segment1End, Vector2 segment2Start,
        Vector2 segment2End, out float disSquared)
    {
        var d1 = segment1End - segment1Start;
        var d2 = segment2End - segment2Start;
        var r = segment1Start - segment2Start;

        float a = Vector2.Dot(d1, d1);
        float e = Vector2.Dot(d2, d2);
        float f = Vector2.Dot(d2, r);

        float s, t;
        if (a <= 1e-10 && e <= 1e-10)
        {
            // Both segments degenerate into points
            s = t = 0.0f;
            disSquared = (segment1Start - segment2Start).LengthSquared();
            disSquared = ShapeMath.ClampToZero(disSquared);
            return (segment1Start, segment2Start);
        }

        if (a <= 1e-10)
        {
            // First segment degenerates into a point
            s = 0.0f;
            t = Math.Clamp(f / e, 0.0f, 1.0f);
        }
        else
        {
            float c = Vector2.Dot(d1, r);
            if (e <= 1e-10)
            {
                // Second segment degenerates into a point
                t = 0.0f;
                s = Math.Clamp(-c / a, 0.0f, 1.0f);
            }
            else
            {
                // The general nondegenerate case starts here
                float b = Vector2.Dot(d1, d2);
                float denom = a * e - b * b;

                if (denom != 0.0f)
                {
                    s = Math.Clamp((b * f - c * e) / denom, 0.0f, 1.0f);
                }
                else
                {
                    s = 0.0f;
                }

                t = (b * s + f) / e;

                if (t < 0.0f)
                {
                    t = 0.0f;
                    s = Math.Clamp(-c / a, 0.0f, 1.0f);
                }
                else if (t > 1.0f)
                {
                    t = 1.0f;
                    s = Math.Clamp((b - c) / a, 0.0f, 1.0f);
                }
            }
        }

        var closestPoint1 = segment1Start + s * d1;
        var closestPoint2 = segment2Start + t * d2;
        disSquared = (closestPoint1 - closestPoint2).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
        return (closestPoint1, closestPoint2);
    }

    private static Vector2 ClosestPointOnLineHelper(Vector2 linePoint, Vector2 lineDirection, Vector2 point)
    {
        Vector2 lineDirectionNormalized = Vector2.Normalize(lineDirection);
        Vector2 toPoint = point - linePoint;
        float projectionLength = Vector2.Dot(toPoint, lineDirectionNormalized);
        return linePoint + projectionLength * lineDirectionNormalized;
    }

    public static (Vector2 self, Vector2 other) GetClosestPointSegmentLine(Vector2 segmentStart, Vector2 segmentEnd, Vector2 linePoint, Vector2 lineDirection,
        out float disSquared)
    {
        var segmentDirection = segmentEnd - segmentStart;
        var lineDirectionNormalized = Vector2.Normalize(lineDirection);

        var diff = segmentStart - linePoint;
        float a = Vector2.Dot(segmentDirection, segmentDirection);
        float b = Vector2.Dot(segmentDirection, lineDirectionNormalized);
        float c = Vector2.Dot(lineDirectionNormalized, lineDirectionNormalized);
        float d = Vector2.Dot(segmentDirection, diff);
        float e = Vector2.Dot(lineDirectionNormalized, diff);

        float denominator = a * c - b * b;

        float s, tLine;

        if (Math.Abs(denominator) < 1e-10)
        {
            // The segment and line are parallel
            s = Math.Clamp(-d / a, 0.0f, 1.0f);
            tLine = 0.0f;
        }
        else
        {
            s = (b * e - c * d) / denominator;
            tLine = (a * e - b * d) / denominator;

            // Clamp s to the segment range [0, 1]
            s = Math.Clamp(s, 0.0f, 1.0f);
        }

        var closestPointOnSegment = segmentStart + s * segmentDirection;
        var closestPointOnLine = linePoint + tLine * lineDirectionNormalized;

        // Check distances to segment endpoints
        var closestToSegmentStart = ClosestPointOnLineHelper(linePoint, lineDirectionNormalized, segmentStart);
        var closestToSegmentEnd = ClosestPointOnLineHelper(linePoint, lineDirectionNormalized, segmentEnd);

        float distance = (closestPointOnSegment - closestPointOnLine).LengthSquared();
        float distanceToSegmentStart = (segmentStart - closestToSegmentStart).LengthSquared();
        float distanceToSegmentEnd = (segmentEnd - closestToSegmentEnd).LengthSquared();

        if (distanceToSegmentStart < distance)
        {
            closestPointOnSegment = segmentStart;
            closestPointOnLine = closestToSegmentStart;
            distance = distanceToSegmentStart;
        }

        if (distanceToSegmentEnd < distance)
        {
            closestPointOnSegment = segmentEnd;
            closestPointOnLine = closestToSegmentEnd;
            distance = distanceToSegmentEnd;
        }

        disSquared = ShapeMath.ClampToZero(distance);
        return (closestPointOnSegment, closestPointOnLine);
    }

    private static Vector2 ClosestPointOnRayHelper(Vector2 rayStart, Vector2 rayDirection, Vector2 point)
    {
        Vector2 rayDirectionNormalized = Vector2.Normalize(rayDirection);
        Vector2 toPoint = point - rayStart;
        float projectionLength = Vector2.Dot(toPoint, rayDirectionNormalized);
        projectionLength = Math.Max(projectionLength, 0.0f); // Ensure the closest point is on the ray
        return rayStart + projectionLength * rayDirectionNormalized;
    }

    public static (Vector2 self, Vector2 other) GetClosestPointSegmentRay(Vector2 segmentStart, Vector2 segmentEnd, Vector2 rayPoint, Vector2 rayDirection,
        out float disSquared)
    {
        var segmentDirection = segmentEnd - segmentStart;
        var rayDirectionNormalized = Vector2.Normalize(rayDirection);

        var diff = segmentStart - rayPoint;
        float a = Vector2.Dot(segmentDirection, segmentDirection);
        float b = Vector2.Dot(segmentDirection, rayDirectionNormalized);
        float c = Vector2.Dot(rayDirectionNormalized, rayDirectionNormalized);
        float d = Vector2.Dot(segmentDirection, diff);
        float e = Vector2.Dot(rayDirectionNormalized, diff);

        float denominator = a * c - b * b;

        float s, tRay;

        if (Math.Abs(denominator) < 1e-10)
        {
            // The segment and ray are parallel
            s = Math.Clamp(-d / a, 0.0f, 1.0f);
            tRay = 0.0f;
        }
        else
        {
            s = (b * e - c * d) / denominator;
            tRay = (a * e - b * d) / denominator;

            // Clamp s to the segment range [0, 1]
            s = Math.Clamp(s, 0.0f, 1.0f);

            // Ensure t is non-negative for the ray
            if (tRay < 0.0f)
            {
                tRay = 0.0f;
                s = Math.Clamp(-d / a, 0.0f, 1.0f);
            }
        }

        var closestPointOnSegment = segmentStart + s * segmentDirection;
        var closestPointOnRay = rayPoint + tRay * rayDirectionNormalized;

        // Check if the closest point on the ray is actually far behind the ray start
        if (tRay < 0.0f)
        {
            closestPointOnRay = rayPoint;
        }

        // Check distances to segment endpoints
        var closestToSegmentStart = ClosestPointOnRayHelper(rayPoint, rayDirectionNormalized, segmentStart);
        var closestToSegmentEnd = ClosestPointOnRayHelper(rayPoint, rayDirectionNormalized, segmentEnd);

        float distance = (closestPointOnSegment - closestPointOnRay).LengthSquared();
        float distanceToSegmentStart = (segmentStart - closestToSegmentStart).LengthSquared();
        float distanceToSegmentEnd = (segmentEnd - closestToSegmentEnd).LengthSquared();

        if (distanceToSegmentStart < distance)
        {
            closestPointOnSegment = segmentStart;
            closestPointOnRay = closestToSegmentStart;
            distance = distanceToSegmentStart;
        }

        if (distanceToSegmentEnd < distance)
        {
            closestPointOnSegment = segmentEnd;
            closestPointOnRay = closestToSegmentEnd;
            distance = distanceToSegmentEnd;
        }

        disSquared = ShapeMath.ClampToZero(distance);
        return (closestPointOnSegment, closestPointOnRay);
    }

    public static (Vector2 self, Vector2 other) GetClosestPointSegmentCircle(Vector2 segmentStart, Vector2 segmentEnd, Vector2 circleCenter, float circleRadius,
        out float disSquared)
    {
        var d1 = segmentEnd - segmentStart;

        var toCenter = circleCenter - segmentStart;
        float projectionLength = Vector2.Dot(toCenter, d1) / d1.LengthSquared();
        projectionLength = Math.Clamp(projectionLength, 0.0f, 1.0f);
        var closestPointOnSegment = segmentStart + projectionLength * d1;

        var offset = Vector2.Normalize(closestPointOnSegment - circleCenter) * circleRadius;
        var closestPointOnCircle = circleCenter + offset;
        disSquared = (closestPointOnCircle - closestPointOnSegment).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
        return (closestPointOnSegment, closestPointOnCircle);
    }
}