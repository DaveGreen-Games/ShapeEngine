using System.Numerics;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.LineDef;

public readonly partial struct Line
{
    /// <summary>
    /// Calculates the closest point on an infinite line to a given point in 2D space.
    /// </summary>
    /// <param name="linePoint">A point through which the line passes.</param>
    /// <param name="lineDirection">The direction vector of the line (does not need to be normalized).</param>
    /// <param name="point">The point from which the closest point on the line is sought.</param>
    /// <param name="disSquared">Outputs the squared distance between the closest point on the line and the given point.</param>
    /// <returns>The closest point on the line to the specified point.</returns>
    /// <remarks>
    /// The direction vector is normalized internally. The squared distance is clamped to zero to avoid negative values due to floating point errors.
    /// </remarks>
    public static Vector2 GetClosestPointLinePoint(Vector2 linePoint, Vector2 lineDirection, Vector2 point, out float disSquared)
    {
        // Normalize the direction vector of the line
        var normalizedLineDirection = lineDirection.Normalize();

        // Calculate the vector from the line's point to the given point
        var toPoint = point - linePoint;

        // Project the vector to the point onto the line direction
        float projectionLength = Vector2.Dot(toPoint, normalizedLineDirection);

        // Calculate the closest point on the line
        var closestPointOnLine = linePoint + projectionLength * normalizedLineDirection;
        disSquared = (closestPointOnLine - point).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
        return closestPointOnLine;
    }

    /// <summary>
    /// Calculates the closest points between two infinite lines in 2D space.
    /// </summary>
    /// <param name="line1Point">A point on the first line.</param>
    /// <param name="line1Direction">The direction vector of the first line.</param>
    /// <param name="line2Point">A point on the second line.</param>
    /// <param name="line2Direction">The direction vector of the second line.</param>
    /// <param name="disSquared">Outputs the squared distance between the closest points on the two lines. Returns 0 if the lines intersect, -1 if they are parallel.</param>
    /// <returns>A tuple containing the closest points on each line, respectively.</returns>
    /// <remarks>
    /// If the lines intersect, the returned points are identical and <paramref name="disSquared"/> is 0. If the lines are parallel, the original points are returned and <paramref name="disSquared"/> is -1.
    /// </remarks>
    public static (Vector2 self, Vector2 other) GetClosestPointLineLine(Vector2 line1Point, Vector2 line1Direction, Vector2 line2Point, Vector2 line2Direction,
        out float disSquared)
    {
        var result = IntersectLineLine(line1Point, line1Direction, line2Point, line2Direction);
        if (result.Valid)
        {
            disSquared = 0f;
            return (result.Point, result.Point);
        }

        disSquared = -1f;
        return (line1Point, line2Point);
        // var d1 = line1Direction.Normalize();
        // var d2 = line2Direction.Normalize();
        //
        // float a = Vector2.Dot(d1, d1);
        // float b = Vector2.Dot(d1, d2);
        // float e = Vector2.Dot(d2, d2);
        // var r = line1Point - line2Point;
        // float c = Vector2.Dot(d1, r);
        // float f = Vector2.Dot(d2, r);
        //
        // float denominator = a * e - b * b;
        // float t1 = (b * f - c * e) / denominator;
        // float t2 = (a * f - b * c) / denominator;
        //
        // var closestPoint1 = line1Point + t1 * d1;
        // var closestPoint2 = line2Point + t2 * d2;
        // disSquared = (closestPoint1 - closestPoint2).LengthSquared();
        // return (closestPoint1, closestPoint2);
    }

    /// <summary>
    /// Calculates the closest points between an infinite line and a ray in 2D space.
    /// </summary>
    /// <param name="linePoint">A point on the line.</param>
    /// <param name="lineDirection">The direction vector of the line.</param>
    /// <param name="rayPoint">The starting point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="disSquared">Outputs the squared distance between the closest points on the line and the ray.</param>
    /// <returns>A tuple containing the closest point on the line and the closest point on the ray, respectively.</returns>
    /// <remarks>
    /// If the line and ray intersect, the returned points are identical and <paramref name="disSquared"/> is 0. Otherwise, the closest point on the line to the ray's origin is returned.
    /// </remarks>
    public static (Vector2 self, Vector2 other) GetClosestPointLineRay(Vector2 linePoint, Vector2 lineDirection, Vector2 rayPoint, Vector2 rayDirection,
        out float disSquared)
    {
        var intersection = IntersectLineRay(linePoint, lineDirection, rayPoint, rayDirection);
        if (intersection.Valid)
        {
            disSquared = 0;
            return (intersection.Point, intersection.Point);
        }

        var cp = GetClosestPointLinePoint(linePoint, lineDirection, rayPoint, out disSquared);
        return (cp, rayPoint);
        // var d1 = lineDirection.Normalize();
        // var d2 = rayDirection.Normalize();
        //
        // float a = Vector2.Dot(d1, d1);
        // float b = Vector2.Dot(d1, d2);
        // float e = Vector2.Dot(d2, d2);
        // var r = linePoint - rayPoint;
        // float c = Vector2.Dot(d1, r);
        // float f = Vector2.Dot(d2, r);
        //
        // float denominator = a * e - b * b;
        // float t1 = (b * f - c * e) / denominator;
        // float t2 = Math.Max(0, (a * f - b * c) / denominator);
        //
        // var closestPoint1 = linePoint + t1 * d1;
        // var closestPoint2 = rayPoint + t2 * d2;
        // disSquared = (closestPoint1 - closestPoint2).LengthSquared();
        // // disSquared = ShapeMath.ClampToZero(disSquared);
        // return (closestPoint1, closestPoint2);
    }

    /// <summary>
    /// Calculates the closest points between an infinite line and a finite segment in 2D space.
    /// </summary>
    /// <param name="linePoint">A point on the line.</param>
    /// <param name="lineDirection">The direction vector of the line.</param>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <param name="disSquared">Outputs the squared distance between the closest points on the line and the segment.</param>
    /// <returns>A tuple containing the closest point on the line and the closest point on the segment, respectively.</returns>
    /// <remarks>
    /// Uses the segment's own closest point calculation for accuracy. The squared distance is clamped to zero to avoid negative values due to floating point errors.
    /// </remarks>
    public static (Vector2 self, Vector2 other) GetClosestPointLineSegment(Vector2 linePoint, Vector2 lineDirection, Vector2 segmentStart, Vector2 segmentEnd,
        out float disSquared)
    {
        var result = Segment.GetClosestPointSegmentLine(segmentStart, segmentEnd, linePoint, lineDirection, out disSquared);
        return (result.other, result.self);
    }

    /// <summary>
    /// Calculates the closest points between an infinite line and a circle in 2D space.
    /// </summary>
    /// <param name="linePoint">A point on the line.</param>
    /// <param name="lineDirection">The direction vector of the line.</param>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="disSquared">Outputs the squared distance between the closest points on the line and the circle.</param>
    /// <returns>A tuple containing the closest point on the line and the closest point on the circle, respectively.</returns>
    /// <remarks>
    /// The closest point on the circle is found by projecting the closest point on the line onto the circle's perimeter. The squared distance is clamped to zero to avoid negative values due to floating point errors.
    /// </remarks>
    public static (Vector2 self, Vector2 other) GetClosestPointLineCircle(Vector2 linePoint, Vector2 lineDirection, Vector2 circleCenter, float circleRadius,
        out float disSquared)
    {
        // var d1 = lineDirection.Normalize();
        //
        // var toCenter = circleCenter - linePoint;
        // float projectionLength = Vector2.Dot(toCenter, d1);
        // var closestPointOnLine = linePoint + projectionLength * d1;
        //
        // var offset = (closestPointOnLine - circleCenter).Normalize() * circleRadius;
        // var closestPointOnCircle = circleCenter + offset;
        // disSquared = (closestPointOnLine - closestPointOnCircle).LengthSquared();
        // return (closestPointOnLine, closestPointOnCircle);
        var result = GetClosestPointLinePoint(linePoint, lineDirection, circleCenter, out disSquared);
        var other = circleCenter + (result - circleCenter).Normalize() * circleRadius;
        disSquared = (result - other).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
        return (result, other);
    }
}