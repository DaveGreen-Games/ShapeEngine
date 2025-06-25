using System.Numerics;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.Circle;

public readonly partial struct Circle
{
    #region Closest Point

    /// <summary>
    /// Gets the closest point on the circle to a given point.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="p">The point to check.</param>
    /// <param name="disSquared">The squared distance between the circle and the point.</param>
    /// <returns>The closest point on the circle.</returns>
    public static Vector2 GetClosestPointCirclePoint(Vector2 center, float radius, Vector2 p, out float disSquared)
    {
        var dir = (p - center).Normalize();
        var closestPoint = center + dir * radius;
        disSquared = (closestPoint - p).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
        return closestPoint;
    }

    /// <summary>
    /// Gets the closest points between the circle and a segment.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <param name="disSquared">The squared distance between the circle and the segment.</param>
    /// <returns>A tuple containing the closest points on the circle and the segment.</returns>
    public static (Vector2 self, Vector2 other) GetClosestPointCircleSegment(Vector2 circleCenter, float circleRadius, Vector2 segmentStart, Vector2 segmentEnd,
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
        return (closestPointOnCircle, closestPointOnSegment);
    }

    /// <summary>
    /// Gets the closest points between the circle and a line.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="linePoint">A point on the line.</param>
    /// <param name="lineDirection">The direction of the line.</param>
    /// <param name="disSquared">The squared distance between the circle and the line.</param>
    /// <returns>A tuple containing the closest points on the circle and the line.</returns>
    public static (Vector2 self, Vector2 other) GetClosestPointCircleLine(Vector2 circleCenter, float circleRadius, Vector2 linePoint, Vector2 lineDirection,
        out float disSquared)
    {
        var d1 = lineDirection.Normalize();

        var toCenter = circleCenter - linePoint;
        float projectionLength = Vector2.Dot(toCenter, d1);
        var closestPointOnLine = linePoint + projectionLength * d1;

        var offset = (closestPointOnLine - circleCenter).Normalize() * circleRadius;
        var closestPointOnCircle = circleCenter + offset;
        disSquared = (closestPointOnLine - closestPointOnCircle).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
        return (closestPointOnCircle, closestPointOnLine);
    }

    /// <summary>
    /// Gets the closest points between the circle and a ray.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction of the ray.</param>
    /// <param name="disSquared">The squared distance between the circle and the ray.</param>
    /// <returns>A tuple containing the closest points on the circle and the ray.</returns>
    public static (Vector2 self, Vector2 other) GetClosestPointCircleRay(Vector2 circleCenter, float circleRadius, Vector2 rayPoint, Vector2 rayDirection,
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
        return (closestPointOnCircle, closestPointOnRay);
    }

    /// <summary>
    /// Gets the closest points between two circles.
    /// </summary>
    /// <param name="circle1Center">The center of the first circle.</param>
    /// <param name="circle1Radius">The radius of the first circle.</param>
    /// <param name="circle2Center">The center of the second circle.</param>
    /// <param name="circle2Radius">The radius of the second circle.</param>
    /// <param name="disSquared">The squared distance between the two circles.</param>
    /// <returns>A tuple containing the closest points on both circles.</returns>
    public static (Vector2 self, Vector2 other) GetClosestPointCircleCircle(Vector2 circle1Center, float circle1Radius, Vector2 circle2Center,
        float circle2Radius, out float disSquared)
    {
        var w = circle1Center - circle2Center;
        var dir = w.Normalize();
        var a = circle1Center - dir * circle1Radius;
        var b = circle2Center + dir * circle2Radius;
        disSquared = (a - b).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
        return (a, b);
    }

    #endregion
}