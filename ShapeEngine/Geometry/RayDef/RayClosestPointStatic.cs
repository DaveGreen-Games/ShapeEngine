using System.Numerics;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.RayDef;

public readonly partial struct Ray
{
    /// <summary>
    /// Finds the closest point on a ray to a given point.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray (not required to be normalized).</param>
    /// <param name="point">The point to find the closest point to.</param>
    /// <param name="disSquared">The squared distance from the point to the closest point on the ray.</param>
    /// <returns>The closest point on the ray to the given point.</returns>
    /// <remarks>
    /// If the projection of the point onto the ray is behind the ray's origin, the origin is returned as the closest point.
    /// </remarks>
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
    /// <summary>
    /// Finds the closest points between a ray and a line.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="linePoint">A point on the line.</param>
    /// <param name="lineDirection">The direction vector of the line.</param>
    /// <param name="disSquared">The squared distance between the closest points.</param>
    /// <returns>A tuple containing the closest point on the ray and the closest point on the line.</returns>
    /// <remarks>
    /// If the ray and line intersect, the intersection point is returned for both.
    /// </remarks>
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
    /// <summary>
    /// Finds the closest points between two rays.
    /// </summary>
    /// <param name="ray1Point">The origin point of the first ray.</param>
    /// <param name="ray1Direction">The direction vector of the first ray.</param>
    /// <param name="ray2Point">The origin point of the second ray.</param>
    /// <param name="ray2Direction">The direction vector of the second ray.</param>
    /// <param name="disSquared">The squared distance between the closest points.</param>
    /// <returns>A tuple containing the closest point on each ray.</returns>
    /// <remarks>
    /// If the rays intersect, the intersection point is returned for both.
    /// </remarks>
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
    /// <summary>
    /// Finds the closest points between a ray and a segment.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <param name="disSquared">The squared distance between the closest points.</param>
    /// <returns>A tuple containing the closest point on the ray and the closest point on the segment.</returns>
    /// <remarks>
    /// Uses a helper function to compute the closest points and distance squared.
    /// </remarks>
    public static (Vector2 self, Vector2 other) GetClosestPointRaySegment(Vector2 rayPoint, Vector2 rayDirection, Vector2 segmentStart, Vector2 segmentEnd,
        out float disSquared)
    {
        var result = Segment.GetClosestPointSegmentRay(segmentStart, segmentEnd, rayPoint, rayDirection, out disSquared);
        return (result.other, result.self);
    }
    /// <summary>
    /// Finds the closest points between a ray and a circle.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="disSquared">The squared distance between the closest points.</param>
    /// <returns>A tuple containing the closest point on the ray and the closest point on the circle.</returns>
    /// <remarks>
    /// The closest point on the circle is found by projecting the closest point on the ray to the circle's center and then moving out by the radius.
    /// </remarks>
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