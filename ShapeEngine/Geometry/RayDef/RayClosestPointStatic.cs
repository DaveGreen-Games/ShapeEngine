using System.Numerics;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.RayDef;

public readonly partial struct Ray
{
    public static Vector2 GetClosestPointRayPoint(Vector2 rayPoint, Vector2 rayDirection, Vector2 point, out float disSquared)
    {
        // Normalize the direction vector of the ray
        var normalizedRayDirection = rayDirection.Normalize();

        // Calculate the vector from the ray's origin to the given point
        var toPoint = point - rayPoint;

        // Project the vector to the point onto the ray direction
        float projectionLength = Vector2.Dot(toPoint, normalizedRayDirection);

        // If the projection is negative, the closest point is the ray's origin
        if (projectionLength < 0)
        {
            disSquared = (rayPoint - point).LengthSquared();
            return rayPoint;
        }

        // Calculate the closest point on the ray
        var closestPointOnRay = rayPoint + projectionLength * normalizedRayDirection;

        disSquared = (closestPointOnRay - point).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
        return closestPointOnRay;
    }

    public static (Vector2 self, Vector2 other) GetClosestPointRayLine(Vector2 rayPoint, Vector2 rayDirection, Vector2 linePoint, Vector2 lineDirection,
        out float disSquared)
    {
        var intersection = IntersectRayLine(rayPoint, rayDirection, linePoint, lineDirection);
        if (intersection.Valid)
        {
            disSquared = 0;
            return (intersection.Point, intersection.Point);
        }

        var cp = Line.GetClosestPointLinePoint(linePoint, lineDirection, rayPoint, out disSquared);
        return (rayPoint, cp);
    }

    public static (Vector2 self, Vector2 other) GetClosestPointRayRay(Vector2 ray1Point, Vector2 ray1Direction, Vector2 ray2Point, Vector2 ray2Direction,
        out float disSquared)
    {
        var intersection = IntersectRayRay(ray1Point, ray1Direction, ray2Point, ray2Direction);
        if (intersection.Valid)
        {
            disSquared = 0;
            return (intersection.Point, intersection.Point);
        }

        var cp1 = GetClosestPointRayPoint(ray1Point, ray1Direction, ray2Point, out float disSquared1);
        var cp2 = GetClosestPointRayPoint(ray2Point, ray2Direction, ray1Point, out float disSquared2);

        if (disSquared1 < disSquared2)
        {
            disSquared = disSquared1;
            return (cp1, ray2Point);
        }

        disSquared = disSquared2;
        return (cp2, ray1Point);
    }

    public static (Vector2 self, Vector2 other) GetClosestPointRaySegment(Vector2 rayPoint, Vector2 rayDirection, Vector2 segmentStart, Vector2 segmentEnd,
        out float disSquared)
    {
        var result = Segment.GetClosestPointSegmentRay(segmentStart, segmentEnd, rayPoint, rayDirection, out disSquared);
        return (result.other, result.self);
    }

    public static (Vector2 self, Vector2 other) GetClosestPointRayCircle(Vector2 rayPoint, Vector2 rayDirection, Vector2 circleCenter, float circleRadius,
        out float disSquared)
    {
        var d1 = rayDirection.Normalize();

        var toCenter = circleCenter - rayPoint;
        float projectionLength = Math.Max(0, Vector2.Dot(toCenter, d1));
        var closestPointOnRay = rayPoint + projectionLength * d1;

        var offset = (closestPointOnRay - circleCenter).Normalize() * circleRadius;
        var closestPointOnCircle = circleCenter + offset;
        disSquared = (closestPointOnRay - closestPointOnCircle).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
        return (closestPointOnRay, closestPointOnCircle);
    }
}