using System.Numerics;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;
using ShapeEngine.Random;

namespace ShapeEngine.Core.Shapes;

/// <summary>
/// Represents an infinite line in 2D space, defined by a point on the line and a direction vector.
/// Includes methods for geometric operations such as intersections, overlaps, and closest point calculations.
/// </summary>
public readonly struct Line
{
    /// <summary>
    /// The maximum length used for line calculations, primarily as a safeguard for infinite lines in geometric operations.
    /// </summary>
    public static float MaxLength = 250000;
    
    #region Members

    /// <summary>
    /// A point through which the line passes.
    /// </summary>
    /// <remarks>
    /// This point is used as the anchor for all geometric calculations involving the line.
    /// </remarks>
    public readonly Vector2 Point;

    /// <summary>
    /// The direction vector of the line, representing its orientation in 2D space.
    /// </summary>
    /// <remarks>
    /// This vector should be normalized. It determines the infinite extension of the line in both directions.
    /// </remarks>
    public readonly Vector2 Direction;

    /// <summary>
    /// The normal vector perpendicular to the line's direction.
    /// </summary>
    /// <remarks>
    /// The normal is always perpendicular to <see cref="Direction"/> and is used for various geometric operations such as collision detection and side tests.
    /// </remarks>
    public readonly Vector2 Normal;
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Line"/> struct with all members set to zero.
    /// </summary>
    /// <remarks>
    /// This constructor creates an invalid line, as both the direction and normal vectors are zero.
    /// </remarks>
    public Line()
    {
        Point = Vector2.Zero;
        Direction = Vector2.Zero;
        Normal = Vector2.Zero;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="Line"/> struct using explicit coordinates and direction components.
    /// </summary>
    /// <param name="x">The X coordinate of the point through which the line passes.</param>
    /// <param name="y">The Y coordinate of the point through which the line passes.</param>
    /// <param name="dx">The X component of the direction vector.</param>
    /// <param name="dy">The Y component of the direction vector.</param>
    /// <param name="flippedNormal">If true, the normal vector is flipped to the left of the direction; otherwise, it is to the right.</param>
    /// <remarks>
    /// The direction vector is normalized internally. The normal is computed as perpendicular to the direction.
    /// </remarks>
    public Line(float x, float y, float dx, float dy, bool flippedNormal = false)
    {
        Point = new Vector2(x, y);
        Direction = new Vector2(dx, dy).Normalize();
        Normal = flippedNormal ? new Vector2(Direction.Y, -Direction.X) : new Vector2(-Direction.Y, Direction.X);
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="Line"/> struct using a direction vector and an optional normal flip.
    /// </summary>
    /// <param name="direction">The direction vector of the line. It will be normalized.</param>
    /// <param name="flippedNormal">If true, the normal vector is flipped to the left of the direction; otherwise, it is to the right.</param>
    /// <remarks>
    /// The line passes through the origin (0,0).
    /// </remarks>
    public Line(Vector2 direction, bool flippedNormal = false)
    {
        Point = Vector2.Zero;
        Direction = direction.Normalize();
        Normal = flippedNormal ? new Vector2(Direction.Y, -Direction.X) : new Vector2(-Direction.Y, Direction.X);
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="Line"/> struct using a point and a direction vector.
    /// </summary>
    /// <param name="point">A point through which the line passes.</param>
    /// <param name="direction">The direction vector of the line. It will be normalized.</param>
    /// <param name="flippedNormal">If true, the normal vector is flipped to the left of the direction; otherwise, it is to the right.</param>
    /// <remarks>
    /// The normal is computed as perpendicular to the direction vector.
    /// </remarks>
    public Line(Vector2 point, Vector2 direction, bool flippedNormal = false)
    {
        Point = point;
        Direction = direction.Normalize();
        Normal = flippedNormal ? new Vector2(Direction.Y, -Direction.X) : new Vector2(-Direction.Y, Direction.X);
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="Line"/> struct using a point, direction, and explicit normal vector.
    /// </summary>
    /// <param name="point">A point through which the line passes.</param>
    /// <param name="direction">The direction vector of the line. It will be normalized.</param>
    /// <param name="normal">The normal vector, which should be perpendicular to the direction vector.</param>
    /// <remarks>
    /// This constructor is intended for internal use when the normal is already known and does not need to be computed.
    /// </remarks>
    internal Line(Vector2 point, Vector2 direction, Vector2 normal)
    {
        Point = point;
        Direction = direction.Normalize();
        Normal = normal;
    }

    #endregion
    
    #region Public Functions
    /// <summary>
    /// Gets a value indicating whether the line is valid.
    /// </summary>
    /// <remarks>
    /// A line is considered valid if both the direction and normal vectors are non-zero.
    /// </remarks>
    public bool IsValid => (Direction.X!= 0 || Direction.Y!= 0) && (Normal.X != 0 || Normal.Y != 0);

    /// <summary>
    /// Determines whether the normal vector is flipped relative to the direction vector.
    /// </summary>
    /// <returns>True if the normal is flipped (left-hand side of the direction vector); otherwise, false.</returns>
    /// <remarks>
    /// This method compares the normal to the perpendicular of the direction vector to determine its orientation.
    /// </remarks>
    public bool IsNormalFlipped()
    {
        if(!IsValid) return false;
        return Math.Abs(Normal.X - Direction.Y) < 0.0000001f && Math.Abs(Normal.Y - (-Direction.X)) < 0.0000001f;
    }

    /// <summary>
    /// Converts the infinite line to a finite segment of the specified length, centered on the line's point.
    /// </summary>
    /// <param name="length">The length of the resulting segment.</param>
    /// <returns>A <see cref="Segment"/> representing a finite portion of the line.</returns>
    /// <remarks>
    /// If the line is invalid, an empty segment is returned.
    /// </remarks>
    public Segment ToSegment(float length)
    {
        if (!IsValid) return new();
        return new Segment(Point - Direction * length * 0.5f, Point + Direction * length * 0.5f, Normal);
    }

    /// <summary>
    /// Converts the line to a ray starting at the line's point and extending in the direction of the line.
    /// </summary>
    /// <param name="reversed">If true, the ray points in the opposite direction.</param>
    /// <returns>A <see cref="Ray"/> representing the line as a ray.</returns>
    /// <remarks>
    /// The normal is also reversed if the direction is reversed.
    /// </remarks>
    public Ray ToRay(bool reversed = false) => reversed ? new Ray(Point, -Direction, -Normal) : new Ray(Point, Direction, Normal);

    /// <summary>
    /// Returns a new <see cref="Line"/> with the normal vector flipped.
    /// </summary>
    /// <returns>A new <see cref="Line"/> with the normal vector pointing in the opposite direction.</returns>
    /// <remarks>
    /// Flipping the normal does not affect the direction or point of the line.
    /// </remarks>
    public Line FlipNormal() => new Line(Point, Direction, Normal.Flip());

    /// <summary>
    /// Computes the normal vector for a given direction vector.
    /// </summary>
    /// <param name="direction">The direction vector for which to compute the normal.</param>
    /// <param name="flippedNormal">If true, returns the left-hand normal; otherwise, the right-hand normal.</param>
    /// <returns>The computed normal vector, normalized.</returns>
    /// <remarks>
    /// The normal is perpendicular to the direction vector and is normalized.
    /// </remarks>
    public static Vector2 GetNormal(Vector2 direction, bool flippedNormal)
    {
        if (flippedNormal) return direction.GetPerpendicularLeft().Normalize();
        return direction.GetPerpendicularRight().Normalize();
    }
    /// <summary>
    /// Returns the axis-aligned bounding box of the line, extending in both directions for a maximum length.
    /// </summary>
    /// <returns>A <see cref="Rect"/> representing the bounding box of the line.</returns>
    /// <remarks>
    /// The bounding box is computed as if the line were a finite segment of maximum length.
    /// <see cref="MaxLength"/> determines the length used for the bounding box calculation.
    /// </remarks>
    public Rect GetBoundingBox() { return new(Point - Direction * MaxLength * 0.5f, Point + Direction * MaxLength * 0.5f); }
    /// <summary>
    /// Returns the axis-aligned bounding box of a finite segment of the line with the specified length.
    /// </summary>
    /// <param name="length">The length of the segment to consider for the bounding box.</param>
    /// <returns>A <see cref="Rect"/> representing the bounding box of the segment.</returns>
    /// <remarks>
    /// The bounding box is centered on the line's point and extends in both directions according to the specified length.
    /// </remarks>
    public Rect GetBoundingBox(float length) { return new(Point - Direction * length * 0.5f, Point + Direction * length * 0.5f); }
    
    /// <summary>
    /// Generates a random line passing through the origin with a random direction between 0 and 359 degrees.
    /// </summary>
    /// <returns>A new <see cref="Line"/> with a random direction.</returns>
    public Line RandomLine() => RandomLine(0, 359);
    /// <summary>
    /// Generates a random line passing through the origin with a random direction between 0 and the specified maximum angle in degrees.
    /// </summary>
    /// <param name="maxAngleDeg">The maximum angle in degrees for the random direction.</param>
    /// <returns>A new <see cref="Line"/> with a random direction.</returns>
    public Line RandomLine(float maxAngleDeg) => RandomLine(0, maxAngleDeg);
    /// <summary>
    /// Generates a random line passing through the origin with a random direction between the specified minimum and maximum angles in degrees.
    /// </summary>
    /// <param name="minAngleDeg">The minimum angle in degrees for the random direction.</param>
    /// <param name="maxAngleDeg">The maximum angle in degrees for the random direction.</param>
    /// <returns>A new <see cref="Line"/> with a random direction.</returns>
    public Line RandomLine(float minAngleDeg, float maxAngleDeg) => RandomLine(Vector2.Zero, 0, 0, minAngleDeg, maxAngleDeg);
    /// <summary>
    /// Generates a random line passing through a random point near the specified origin, with a random direction and length.
    /// </summary>
    /// <param name="origin">The origin point around which the random point is generated.</param>
    /// <param name="minLength">The minimum distance from the origin for the random point.</param>
    /// <param name="maxLength">The maximum distance from the origin for the random point.</param>
    /// <param name="minAngleDeg">The minimum angle in degrees for the random direction.</param>
    /// <param name="maxAngleDeg">The maximum angle in degrees for the random direction.</param>
    /// <returns>A new <see cref="Line"/> with a random point and direction.</returns>
    /// <remarks>
    /// If the length parameters are invalid, the origin is used as the point.
    /// </remarks>
    public Line RandomLine(Vector2 origin, float minLength, float maxLength, float minAngleDeg, float maxAngleDeg)
    {
        Vector2 point;
        if(minLength < 0 || maxLength < 0 || minLength >= maxLength) point = origin;
        else point = origin + Rng.Instance.RandVec2(minLength, maxLength);
        return new(point, Rng.Instance.RandVec2(minAngleDeg, maxAngleDeg));
    }
    /// <summary>
    /// Returns a new <see cref="Line"/> with the point set to the specified value.
    /// </summary>
    /// <param name="newPoint">The new point through which the line will pass.</param>
    /// <returns>A new <see cref="Line"/> with the updated point.</returns>
    public Line SetPoint(Vector2 newPoint) => new (newPoint, Direction, Normal);
    /// <summary>
    /// Returns a new <see cref="Line"/> with the point moved by the specified amount.
    /// </summary>
    /// <param name="amount">The vector by which to move the point.</param>
    /// <returns>A new <see cref="Line"/> with the updated point.</returns>
    public Line ChangePoint(Vector2 amount) => new (Point + amount, Direction, Normal);
    
    /// <summary>
    /// Returns a new <see cref="Line"/> with the direction set to the specified value.
    /// </summary>
    /// <param name="newDirection">The new direction vector for the line.</param>
    /// <returns>A new <see cref="Line"/> with the updated direction and normal.</returns>
    /// <remarks>
    /// The normal is recalculated to maintain its original orientation (flipped or not).
    /// </remarks>
    public Line SetDirection(Vector2 newDirection)
    {
        var normalFlipped = IsNormalFlipped();
        return new (Point, newDirection, normalFlipped);
    }
    /// <summary>
    /// Returns a new <see cref="Line"/> with the direction changed by the specified amount.
    /// </summary>
    /// <param name="amount">The vector to add to the current direction.</param>
    /// <returns>A new <see cref="Line"/> with the updated direction.</returns>
    /// <remarks>
    /// The normal is recalculated to maintain its original orientation (flipped or not).
    /// </remarks>
    public Line ChangeDirection(Vector2 amount)
    {
        var normalFlipped = IsNormalFlipped();
        return new (Point, Direction + amount, normalFlipped);
    }
    /// <summary>
    /// Returns a new <see cref="Line"/> with the direction rotated by the specified angle in radians.
    /// </summary>
    /// <param name="angleRad">The angle in radians by which to rotate the direction vector.</param>
    /// <returns>A new <see cref="Line"/> with the rotated direction.</returns>
    /// <remarks>
    /// The normal is recalculated to maintain its original orientation (flipped or not).
    /// </remarks>
    public Line ChangeRotation(float angleRad)
    {
        var normalFlipped = IsNormalFlipped();
        var newDir = Direction.Rotate(angleRad);
        return new (Point, newDir, normalFlipped);
    }
    /// <summary>
    /// Returns a new <see cref="Line"/> with the direction set to the specified angle in radians.
    /// </summary>
    /// <param name="angleRad">The angle in radians for the new direction vector.</param>
    /// <returns>A new <see cref="Line"/> with the updated direction.</returns>
    /// <remarks>
    /// The normal is recalculated to maintain its original orientation (flipped or not).
    /// </remarks>
    public Line SetRotation(float angleRad)
    {
        var normalFlipped = IsNormalFlipped();
        var newDir = ShapeVec.VecFromAngleRad(angleRad);
        return new (Point, newDir, normalFlipped);
    }
    #endregion
   
    #region Closest Point
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
    public static (Vector2 self, Vector2 other) GetClosestPointLineLine(Vector2 line1Point, Vector2 line1Direction, Vector2 line2Point, Vector2 line2Direction, out float disSquared)
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
    public static (Vector2 self, Vector2 other) GetClosestPointLineRay(Vector2 linePoint, Vector2 lineDirection, Vector2 rayPoint, Vector2 rayDirection, out float disSquared)
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
    public static (Vector2 self, Vector2 other) GetClosestPointLineSegment(Vector2 linePoint, Vector2 lineDirection, Vector2 segmentStart, Vector2 segmentEnd, out float disSquared)
    {
        var result = Segment.GetClosestPointSegmentLine(segmentStart, segmentEnd, linePoint, lineDirection, out disSquared);
        return (result.other, result.self);
        // var d1 = lineDirection.Normalize();
        // var d2 = segmentEnd - segmentStart;
        //
        // float a = Vector2.Dot(d1, d1);
        // float b = Vector2.Dot(d1, d2);
        // float e = Vector2.Dot(d2, d2);
        // var r = linePoint - segmentStart;
        // float c = Vector2.Dot(d1, r);
        // float f = Vector2.Dot(d2, r);
        //
        // float denominator = a * e - b * b;
        // float t1 = (b * f - c * e) / denominator;
        // float t2 = Math.Max(0, Math.Min(1, (a * f - b * c) / denominator));
        //
        // var closestPoint1 = linePoint + t1 * d1;
        // var closestPoint2 = segmentStart + t2 * d2;
        // disSquared = (closestPoint1 - closestPoint2).LengthSquared();
        // return (closestPoint1, closestPoint2);
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
    public static (Vector2 self, Vector2 other) GetClosestPointLineCircle(Vector2 linePoint, Vector2 lineDirection, Vector2 circleCenter, float circleRadius, out float disSquared)
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
    
    
    /// <summary>
    /// Calculates the closest point on this line to a given point in 2D space.
    /// </summary>
    /// <param name="point">The point from which the closest point on the line is sought.</param>
    /// <param name="disSquared">Outputs the squared distance between the closest point on the line and the given point.</param>
    /// <returns>A <see cref="CollisionPoint"/> representing the closest point on the line and its normal.</returns>
    /// <remarks>
    /// The normal is oriented to face the point if it is on the same side as the line's normal, otherwise it is flipped.
    /// </remarks>
    public CollisionPoint GetClosestPoint(Vector2 point, out float disSquared)
    {
        // Normalize the direction vector of the line
        var normalizedLineDirection = Direction.Normalize();

        // Calculate the vector from the line's point to the given point
        var toPoint = point - Point;

        // Project the vector to the point onto the line direction
        float projectionLength = Vector2.Dot(toPoint, normalizedLineDirection);

        // Calculate the closest point on the line
        var closestPointOnLine = Point + projectionLength * normalizedLineDirection;
        disSquared = (closestPointOnLine - point).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
        var dir = (point - closestPointOnLine).Normalize();
        var dot = Vector2.Dot(dir, Normal);
        if (dot >= 0) return new(closestPointOnLine, Normal);
        return new(closestPointOnLine, -Normal);
    }
    /// <summary>
    /// Calculates the closest points between this line and another line.
    /// </summary>
    /// <param name="other">The other <see cref="Line"/> to which the closest point is sought.</param>
    /// <returns>
    /// A <see cref="ClosestPointResult"/> containing the closest points on both lines, their normals, and the squared distance between them.
    /// If the lines intersect, the points are identical and the distance is zero. If the lines are parallel, an empty result is returned.
    /// </returns>
    public ClosestPointResult GetClosestPoint(Line other)
    {
        var result = IntersectLineLine(Point, Direction, other.Point, other.Direction);
        if (result.Valid)
        {
            return new
                (
                    new(result.Point, Normal),
                    new(result.Point, other.Normal),
                    0f
                    );
        }
        return new();
        // var d1 = Direction;
        // var d2 = other.Direction;
        //
        // float a = Vector2.Dot(d1, d1);
        // float b = Vector2.Dot(d1, d2);
        // float e = Vector2.Dot(d2, d2);
        // var r = Point - other.Point;
        // float c = Vector2.Dot(d1, r);
        // float f = Vector2.Dot(d2, r);
        //
        // float denominator = a * e - b * b;
        // float t1 = (b * f - c * e) / denominator;
        // float t2 = (a * f - b * c) / denominator;
        //
        // var closestPoint1 = Point + t1 * d1;
        // var closestPoint2 = other.Point + t2 * d2;
        //
        // float disSquared = (closestPoint1 - closestPoint2).LengthSquared();
        // return new(
        //     new(closestPoint1, Normal), 
        //     new(closestPoint2, other.Normal),
        //     disSquared);
    }
    /// <summary>
    /// Calculates the closest points between this line and a ray.
    /// </summary>
    /// <param name="other">The <see cref="Ray"/> to which the closest point is sought.</param>
    /// <returns>
    /// A <see cref="ClosestPointResult"/> containing the closest points on the line and the ray, their normals, and the squared distance between them.
    /// </returns>
    public ClosestPointResult GetClosestPoint(Ray other)
    {
        var result = GetClosestPointLineRay(Point, Direction, other.Point, other.Direction, out float disSquared);
        return new(
            new(result.self, Normal),
            new(result.other, other.Normal),
            disSquared);
        // var d1 = Direction;
        // var d2 = other.Direction;
        //
        // float a = Vector2.Dot(d1, d1);
        // float b = Vector2.Dot(d1, d2);
        // float e = Vector2.Dot(d2, d2);
        // var r = Point - other.Point;
        // float c = Vector2.Dot(d1, r);
        // float f = Vector2.Dot(d2, r);
        //
        // float denominator = a * e - b * b;
        // float t1 = (b * f - c * e) / denominator;
        // float t2 = Math.Max(0, (a * f - b * c) / denominator);
        //
        // var closestPoint1 = Point + t1 * d1;
        // var closestPoint2 = other.Point + t2 * d2;
        // float disSquared = (closestPoint1 - closestPoint2).LengthSquared();
        // return new
        // (
        //     new(closestPoint1, Normal), 
        //     new(closestPoint2, other.Normal),
        //     disSquared
        // );
    }
    /// <summary>
    /// Calculates the closest points between this line and a segment.
    /// </summary>
    /// <param name="other">The <see cref="Segment"/> to which the closest point is sought.</param>
    /// <returns>
    /// A <see cref="ClosestPointResult"/> containing the closest points on the line and the segment, their normals, and the squared distance between them.
    /// </returns>
    public ClosestPointResult GetClosestPoint(Segment other)
    {
        var result = Segment.GetClosestPointSegmentLine(other.Start, other.End, Point, Direction, out var disSquared);
        return new(
            new(result.other, Normal), 
            new(result.self, other.Normal),
            disSquared);
        // var d1 = Direction;
        // var d2 = other.Displacement;
        //
        // float a = Vector2.Dot(d1, d1);
        // float b = Vector2.Dot(d1, d2);
        // float e = Vector2.Dot(d2, d2);
        // var r = Point - other.Start;
        // float c = Vector2.Dot(d1, r);
        // float f = Vector2.Dot(d2, r);
        //
        // float denominator = a * e - b * b;
        // float t1 = (b * f - c * e) / denominator;
        // float t2 = Math.Max(0, Math.Min(1, (a * f - b * c) / denominator));
        //
        // var closestPoint1 = Point + t1 * d1;
        // var closestPoint2 = other.Start + t2 * d2;
        //
        // float disSquared = (closestPoint1 - closestPoint2).LengthSquared();
        // return new(
        //     new(closestPoint1, Normal), 
        //     new(closestPoint2, other.Normal),
        //     disSquared);
    }
    /// <summary>
    /// Calculates the closest points between this line and a circle.
    /// </summary>
    /// <param name="other">The <see cref="Circle"/> to which the closest point is sought.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points on the line and the circle, their normals, and the squared distance between them.</returns>
    /// <remarks>
    /// The closest point on the circle is found by projecting the closest point on the line onto the circle's perimeter.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Circle other)
    {
        // var pointOnLine = GetClosestPointLinePoint(Point, Direction, other.Center, out float disSquared);
        //
        // var dir = (pointOnLine - other.Center).Normalize();
        // var pointOnCircle = other.Center + dir * other.Radius;
        // disSquared = (pointOnLine - pointOnCircle).LengthSquared();
        // return new(
        //     new(pointOnLine, Normal),
        //     new(pointOnCircle, dir),
        //     disSquared
        // );
        var d1 = Direction;
        
        var toCenter = other.Center - Point;
        float projectionLength = Vector2.Dot(toCenter, d1);
        var closestPointOnLine = Point + projectionLength * d1;
        
        var offset = (closestPointOnLine - other.Center).Normalize() * other.Radius;
        var closestPointOnCircle = other.Center + offset;
        
        float disSquared = (closestPointOnCircle - closestPointOnLine).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
        return new(
            new(closestPointOnLine, Normal),
            new(closestPointOnCircle, (closestPointOnCircle - other.Center).Normalize()),
            disSquared);
    }
    /// <summary>
    /// Calculates the closest points between this line and a triangle.
    /// </summary>
    /// <param name="other">The <see cref="Triangle"/> to which the closest point is sought.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points on the line and the triangle, their normals, the squared distance, and the index of the closest triangle edge.</returns>
    /// <remarks>
    /// The closest point is determined by evaluating all triangle edges.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Triangle other)
    {
        var closestResult = GetClosestPointLineSegment(Point, Direction, other.A, other.B, out float minDisSquared);
        
        var otherNormal = (other.B - other.A);
        var otherIndex = 0;
        var result = GetClosestPointLineSegment(Point, Direction, other.B, other.C, out float disSquared);
        
        if (disSquared < minDisSquared)
        {
            otherIndex = 1;
            minDisSquared = disSquared;
            closestResult = result;
            otherNormal = (other.C - other.B);
        }
        
        result = GetClosestPointLineSegment(Point, Direction, other.C, other.A, out disSquared);
        
        if (disSquared < minDisSquared)
        {
            var normal = (other.A - other.C).GetPerpendicularRight().Normalize();
            return new(
                new(result.self, Normal), 
                new(result.other, normal),
                disSquared,
                -1,
                2);
        }
        
        return new(
            new(closestResult.self, Normal), 
            new(closestResult.other, otherNormal.GetPerpendicularRight().Normalize()),
            minDisSquared,
            -1,
            otherIndex);
    }
    /// <summary>
    /// Calculates the closest points between this line and a quadrilateral.
    /// </summary>
    /// <param name="other">The <see cref="Quad"/> to which the closest point is sought.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points on the line and the quad, their normals, the squared distance, and the index of the closest quad edge.</returns>
    /// <remarks>
    /// The closest point is determined by evaluating all quad edges.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Quad other)
    {
        var closestResult = GetClosestPointLineSegment(Point, Direction, other.A, other.B, out float minDisSquared);
        var otherNormal = (other.B - other.A);
        var otherIndex = 0;
        
        var result = GetClosestPointLineSegment(Point, Direction, other.B, other.C, out float disSquared);
        if (disSquared < minDisSquared)
        {
            otherIndex = 1;
            minDisSquared = disSquared;
            closestResult = result;
            otherNormal = (other.C - other.B);
        }
        
        result = GetClosestPointLineSegment(Point, Direction, other.C, other.D, out disSquared);
        if (disSquared < minDisSquared)
        {
            otherIndex = 2;
            minDisSquared = disSquared;
            closestResult = result;
            otherNormal = (other.D - other.C);
        }
        
        result = GetClosestPointLineSegment(Point, Direction, other.D, other.A, out disSquared);
        if (disSquared < minDisSquared)
        {
            var normal = (other.A - other.D).GetPerpendicularRight().Normalize();
            return new(
                new(result.self, Normal), 
                new(result.other, normal),
                disSquared,
                -1,
                3);
        }

        return new(
            new(closestResult.self, Normal), 
            new(closestResult.other, otherNormal.GetPerpendicularRight().Normalize()),
            minDisSquared,
            -1,
            otherIndex);
    }
    /// <summary>
    /// Calculates the closest points between this line and a rectangle.
    /// </summary>
    /// <param name="other">The <see cref="Rect"/> to which the closest point is sought.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points on the line and the rectangle, their normals, the squared distance, and the index of the closest rectangle edge.</returns>
    /// <remarks>
    /// The closest point is determined by evaluating all rectangle edges.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Rect other)
    {
        var closestResult = GetClosestPointLineSegment(Point, Direction, other.A, other.B, out float minDisSquared);
        var otherNormal = (other.B - other.A);
        var otherIndex = 0;
        
        var result = GetClosestPointLineSegment(Point, Direction, other.B, other.C, out float disSquared);
        if (disSquared < minDisSquared)
        {
            otherIndex = 1;
            minDisSquared = disSquared;
            closestResult = result;
            otherNormal = (other.C - other.B);
        }
        
        result = GetClosestPointLineSegment(Point, Direction, other.C, other.D, out disSquared);
        if (disSquared < minDisSquared)
        {
            otherIndex = 2;
            minDisSquared = disSquared;
            closestResult = result;
            otherNormal = (other.D - other.C);
        }
        
        result = GetClosestPointLineSegment(Point, Direction, other.D, other.A, out disSquared);
        if (disSquared < minDisSquared)
        {
            var normal = (other.A - other.D).GetPerpendicularRight().Normalize();
            return new(
                new(result.self, Normal), 
                new(result.other, normal),
                disSquared,
                -1,
                3);
        }

        return new(
            new(closestResult.self, Normal), 
            new(closestResult.other, otherNormal.GetPerpendicularRight().Normalize()),
            minDisSquared,
            -1,
            otherIndex);
    }
    /// <summary>
    /// Calculates the closest points between this line and a polygon.
    /// </summary>
    /// <param name="other">The <see cref="Polygon"/> to which the closest point is sought.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points on the line and the polygon, their normals, the squared distance, and the index of the closest polygon edge.</returns>
    /// <remarks>
    /// The closest point is determined by evaluating all polygon edges.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Polygon other)
    {
        if (other.Count < 3) return new();
        
        var p1 = other[0];
        var p2 = other[1];
        var closestResult = GetClosestPointLineSegment(Point, Direction, p1, p2, out float minDisSquared);
        var otherNormal = (p2 - p1);
        var otherIndex = 0;
        for (var i = 1; i < other.Count; i++)
        {
            p1 = other[i];
            p2 = other[(i + 1) % other.Count];
            var result = GetClosestPointLineSegment(Point, Direction, p1, p2, out float disSquared);
            if (disSquared < minDisSquared)
            {
                otherIndex = i;
                minDisSquared = disSquared;
                closestResult = result;
                otherNormal = (p2 - p1);
            }
        }
        return new(
            new(closestResult.self, Normal), 
            new(closestResult.other, otherNormal.GetPerpendicularRight().Normalize()),
            minDisSquared,
            -1,
            otherIndex);
    }
    /// <summary>
    /// Calculates the closest points between this line and a polyline.
    /// </summary>
    /// <param name="other">The <see cref="Polyline"/> to which the closest point is sought.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points on the line and the polyline, their normals, the squared distance, and the index of the closest polyline segment.</returns>
    /// <remarks>
    /// The closest point is determined by evaluating all polyline segments.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Polyline other)
    {
        if (other.Count < 2) return new();
        
        var p1 = other[0];
        var p2 = other[1];
        var closestResult = GetClosestPointLineSegment(Point, Direction, p1, p2, out float minDisSquared);
        var otherNormal = (p2 - p1);
        var otherIndex = 0;
        for (var i = 1; i < other.Count - 1; i++)
        {
            p1 = other[i];
            p2 = other[i + 1];
            var result = GetClosestPointLineSegment(Point, Direction, p1, p2, out float disSquared);
            if (disSquared < minDisSquared)
            {
                otherIndex = i;
                minDisSquared = disSquared;
                closestResult = result;
                otherNormal = (p2 - p1);
            }
        }
        return new(
            new(closestResult.self, Normal), 
            new(closestResult.other, otherNormal.GetPerpendicularRight().Normalize()),
            minDisSquared,
            -1,
            otherIndex);
    }
    /// <summary>
    /// Calculates the closest points between this line and a collection of segments.
    /// </summary>
    /// <param name="segments">The <see cref="Segments"/> collection to which the closest point is sought.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points on the line and the closest segment, their normals, the squared distance, and the index of the closest segment.</returns>
    /// <remarks>
    /// The closest point is determined by evaluating all segments in the collection.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Segments segments)
    {
        if (segments.Count <= 0) return new();
        
        var curSegment = segments[0];
        var closestResult = GetClosestPoint(curSegment);
        var otherIndex = 0;
        for (var i = 1; i < segments.Count; i++)
        {
            curSegment = segments[i];
            var result = GetClosestPoint(curSegment);

            if (result.IsCloser(closestResult))
            {
                otherIndex = i;
                closestResult = result;
            }
        }
        return closestResult.SetOtherSegmentIndex(otherIndex);
    }
    #endregion
    
    #region Intersections
    
    public static bool IsPointOnLine(Vector2 point, Vector2 linePoint, Vector2 lineDirection)
    {
        // Calculate the vector from the line point to the given point
        var toPoint = point - linePoint;

        // Calculate the cross product of the direction vector and the vector to the point
        float crossProduct = toPoint.X * lineDirection.Y - toPoint.Y * lineDirection.X;

        // If the cross product is close to zero, the point is on the line
        return Math.Abs(crossProduct) < 1e-10;
    }
    
    public static (CollisionPoint p, float t) IntersectLineSegmentInfo(Vector2 linePoint, Vector2 lineDirection, Vector2 segmentStart, Vector2 segmentEnd)
    {
        // Line AB (infinite line) represented by linePoint and lineDirection
        // Line segment CD represented by segmentStart and segmentEnd

        // Calculate direction vector of the segment
        var segmentDirection = segmentEnd - segmentStart;

        // Calculate the denominator of the intersection formula
        float denominator = lineDirection.X * segmentDirection.Y - lineDirection.Y * segmentDirection.X;

        // Check if lines are parallel (denominator is zero)
        if (Math.Abs(denominator) < 1e-10)
        {
            return (new(), -1f);
        }

        // Calculate the intersection point using parameter t
        var difference = segmentStart - linePoint;
        float t = (difference.X * segmentDirection.Y - difference.Y * segmentDirection.X) / denominator;

        // Calculate the intersection point
        var intersection = linePoint + t * lineDirection;

        // Check if the intersection point is within the segment
        if (Segment.IsPointOnSegment(intersection, segmentStart, segmentEnd))
        {
            // The normal vector can be taken as perpendicular to the segment direction
            segmentDirection = segmentDirection.Normalize();
            var normal = new Vector2(-segmentDirection.Y, segmentDirection.X);

            return (new(intersection, normal), t);
        }

        return (new(), -1f);
    }
    
    public static (CollisionPoint p, float t) IntersectLineSegmentInfo(Vector2 linePoint, Vector2 lineDirection, Vector2 segmentStart, Vector2 segmentEnd, Vector2 segmentNormal)
    {
        var result = IntersectLineSegmentInfo(linePoint, lineDirection, segmentStart, segmentEnd);
        if (result.p.Valid)
        {
            return (new(result.p.Point, segmentNormal), result.t);
        }

        return (new(), -1f);
    }
    
    public static (CollisionPoint p, float t) IntersectLineLineInfo(Vector2 line1Point, Vector2 line1Direction, Vector2 line2Point, Vector2 line2Direction)
    {
        // Calculate the denominator of the intersection formula
        float denominator = line1Direction.X * line2Direction.Y - line1Direction.Y * line2Direction.X;

        // Check if lines are parallel (denominator is zero)
        if (Math.Abs(denominator) < 1e-10)
        {
            return (new(), -1f);
        }

        // Calculate the intersection point using parameter t
        var difference = line2Point - line1Point;
        float t = (difference.X * line2Direction.Y - difference.Y * line2Direction.X) / denominator;

        // Calculate the intersection point
        var intersection = line1Point + t * line1Direction;

        // Calculate the normal vector as perpendicular to the direction of the first line
        var normal = new Vector2(-line2Direction.Y, line2Direction.X).Normalize();

        return (new(intersection, normal), t);
    }
    
    public static (CollisionPoint p, float t) IntersectLineLineInfo(Vector2 line1Point, Vector2 line1Direction, Vector2 line2Point, Vector2 line2Direction, Vector2 line2Normal)
    {
        var result = IntersectLineLineInfo(line1Point, line1Direction, line2Point, line2Direction);
        if (result.p.Valid)
        {
            return (new(result.p.Point, line2Normal), result.t);
        }

        return (new(), -1f);
    }
    
    public static (CollisionPoint p, float t) IntersectLineRayInfo(Vector2 linePoint, Vector2 lineDirection, Vector2 rayPoint, Vector2 rayDirection)
    {
        // Calculate the denominator of the intersection formula
        float denominator = lineDirection.X * rayDirection.Y - lineDirection.Y * rayDirection.X;

        // Check if lines are parallel (denominator is zero)
        if (Math.Abs(denominator) < 1e-10)
        {
            return (new(), -1f);
        }

        // Calculate the intersection point using parameter t
        var difference = rayPoint - linePoint;
        float t = (difference.X * rayDirection.Y - difference.Y * rayDirection.X) / denominator;

        // Calculate the parameter u for the ray
        float u = (difference.X * lineDirection.Y - difference.Y * lineDirection.X) / denominator;

        // Check if the intersection point lies in the direction of the ray
        if (u >= 0)
        {
            // Calculate the intersection point
            var intersection = linePoint + t * lineDirection;

            // Calculate the normal vector as perpendicular to the direction of the line
            var normal = new Vector2(-rayDirection.Y, rayDirection.X).Normalize();

            return (new(intersection, normal), t);
        }
        
        return (new(), -1f);
    }
    
    public static (CollisionPoint p, float t) IntersectLineRayInfo(Vector2 linePoint, Vector2 lineDirection, Vector2 rayPoint, Vector2 rayDirection, Vector2 rayNormal)
    {
        var result = IntersectLineRayInfo(linePoint, lineDirection, rayPoint, rayDirection);
        if (result.p.Valid)
        {
            return (new(result.p.Point, rayNormal), result.t);
        }

        return (new(), -1f);
    }
    
    
    public static CollisionPoint IntersectLineSegment(Vector2 linePoint, Vector2 lineDirection, Vector2 segmentStart, Vector2 segmentEnd)
    {
        
        var result = Ray.IntersectRaySegment(linePoint, lineDirection, segmentStart, segmentEnd);
        if (result.Valid) return result;
        return Ray.IntersectRaySegment(linePoint, -lineDirection, segmentEnd, segmentStart);
        
        //for some reason the code below works perfectly for every shape except for line vs rect
        //something about the segments of a rect makes this not work correctly
        //the above code works just fine when a line is split into two rays....
        
        
        // // Line AB (infinite line) represented by linePoint and lineDirection
        // // Line segment CD represented by segmentStart and segmentEnd
        //
        // // Calculate direction vector of the segment
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
        //     // The normal vector can be taken as perpendicular to the segment direction
        //     segmentDirection = segmentDirection.Normalize();
        //     var normal = new Vector2(-segmentDirection.Y, segmentDirection.X);
        //
        //     return new(intersection, normal);
        // }
        //
        // return new();
    }
    public static CollisionPoint IntersectLineSegment(Vector2 linePoint, Vector2 lineDirection, Vector2 segmentStart, Vector2 segmentEnd, Vector2 segmentNormal)
    {
        var result = IntersectLineSegment(linePoint, lineDirection, segmentStart, segmentEnd);
        if (result.Valid)
        {
            return new(result.Point, segmentNormal);
        }

        return new();
    }
    public static CollisionPoint IntersectLineLine(Vector2 line1Point, Vector2 line1Direction, Vector2 line2Point, Vector2 line2Direction)
    {
        // Calculate the denominator of the intersection formula
        float denominator = line1Direction.X * line2Direction.Y - line1Direction.Y * line2Direction.X;

        // Check if lines are parallel (denominator is zero)
        if (Math.Abs(denominator) < 1e-10)
        {
            return new();
        }

        // Calculate the intersection point using parameter t
        var difference = line2Point - line1Point;
        float t = (difference.X * line2Direction.Y - difference.Y * line2Direction.X) / denominator;

        // Calculate the intersection point
        var intersection = line1Point + t * line1Direction;

        // Calculate the normal vector as perpendicular to the direction of the first line
        var normal = new Vector2(-line2Direction.Y, line2Direction.X).Normalize();

        return new(intersection, normal);
    }
    public static CollisionPoint IntersectLineLine(Vector2 line1Point, Vector2 line1Direction, Vector2 line2Point, Vector2 line2Direction, Vector2 line2Normal)
    {
        var result = IntersectLineLine(line1Point, line1Direction, line2Point, line2Direction);
        if (result.Valid)
        {
            return new(result.Point, line2Normal);
        }

        return new();
    }
    public static CollisionPoint IntersectLineRay(Vector2 linePoint, Vector2 lineDirection, Vector2 rayPoint, Vector2 rayDirection)
    {
        // Calculate the denominator of the intersection formula
        float denominator = lineDirection.X * rayDirection.Y - lineDirection.Y * rayDirection.X;

        // Check if lines are parallel (denominator is zero)
        if (Math.Abs(denominator) < 1e-10)
        {
            return new();
        }

        // Calculate the intersection point using parameter t
        Vector2 difference = rayPoint - linePoint;
        float t = (difference.X * rayDirection.Y - difference.Y * rayDirection.X) / denominator;

        // Calculate the parameter u for the ray
        float u = (difference.X * lineDirection.Y - difference.Y * lineDirection.X) / denominator;

        // Check if the intersection point lies in the direction of the ray
        if (u >= 0)
        {
            // Calculate the intersection point
            var intersection = linePoint + t * lineDirection;

            // Calculate the normal vector as perpendicular to the direction of the line
            var normal = new Vector2(-rayDirection.Y, rayDirection.X).Normalize();

            return new(intersection, normal);
        }
        
        return new();
    }
    public static CollisionPoint IntersectLineRay(Vector2 linePoint, Vector2 lineDirection, Vector2 rayPoint, Vector2 rayDirection, Vector2 rayNormal)
    {
        var result = IntersectLineRay(linePoint, lineDirection, rayPoint, rayDirection);
        if (result.Valid)
        {
            return new(result.Point, rayNormal);
        }

        return new();
    }
    
    
    public static (CollisionPoint a, CollisionPoint b) IntersectLineCircle(Vector2 linePoint, Vector2 lineDirection, Vector2 circleCenter, float circleRadius)
    {
        // Normalize the direction vector
        lineDirection = lineDirection.Normalize();

        // Vector from the line point to the circle center
        var toCircle = circleCenter - linePoint;
        
        // Projection of toCircle onto the line direction to find the closest approach
        float projectionLength = Vector2.Dot(toCircle, lineDirection);

        // Closest point on the line to the circle center
        var closestPoint = linePoint + projectionLength * lineDirection;

        // Distance from the closest point to the circle center
        float distanceToCenter = Vector2.Distance(closestPoint, circleCenter);

        // Check if the line intersects the circle
        if (distanceToCenter < circleRadius)
        {
            // Calculate the distance from the closest point to the intersection points
            var offset = (float)Math.Sqrt(circleRadius * circleRadius - distanceToCenter * distanceToCenter);

            // Intersection points
            var intersection1 = closestPoint - offset * lineDirection;
            var intersection2 = closestPoint + offset * lineDirection;

            // Normals at the intersection points
            var normal1 = (intersection1 - circleCenter).Normalize();
            var normal2 = (intersection2 - circleCenter).Normalize();

            var p1 = new CollisionPoint(intersection1, normal1);
            var p2 = new CollisionPoint(intersection2, normal2);
            return (p1, p2);
        }
        
        if (Math.Abs(distanceToCenter - circleRadius) < 1e-10)
        {
            var p = new CollisionPoint(closestPoint,(closestPoint - circleCenter).Normalize());
            return (p, new());
        }

        return (new(), new());
    }
    public static (CollisionPoint a, CollisionPoint b) IntersectLineTriangle(Vector2 linePoint, Vector2 lineDirection, Vector2 a, Vector2 b, Vector2 c)
    {
        CollisionPoint resultA = new();
        CollisionPoint resultB = new();
        
        var cp = IntersectLineSegment(linePoint, lineDirection,  a, b);
        if(cp.Valid) resultA = cp;
        
        cp = IntersectLineSegment(linePoint, lineDirection,  b, c);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }
        
        if(resultA.Valid && resultB.Valid) return (resultA, resultB);
       
        cp = IntersectLineSegment(linePoint, lineDirection,  c, a);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }
        
        return (resultA, resultB);
    }
    public static (CollisionPoint a, CollisionPoint b) IntersectLineQuad(Vector2 linePoint, Vector2 lineDirection, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        CollisionPoint resultA = new();
        CollisionPoint resultB = new();
        
        var cp = IntersectLineSegment(linePoint, lineDirection,  a, b);
        if (cp.Valid)
        {
            resultA = cp;
        }
        
        cp = IntersectLineSegment(linePoint, lineDirection,  b, c);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }

        if (resultA.Valid && resultB.Valid)
        {
            return (resultA, resultB);
        }
       
        cp = IntersectLineSegment(linePoint, lineDirection,  c, d);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }

        if (resultA.Valid && resultB.Valid)
        {
            return (resultA, resultB);
        }
        
        cp = IntersectLineSegment(linePoint, lineDirection,  d, a);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }
        return (resultA, resultB);
    }
    
    public static (CollisionPoint a, CollisionPoint b) IntersectLineRect(Vector2 linePoint, Vector2 lineDirection, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return IntersectLineQuad(linePoint, lineDirection, a, b, c, d);
    }
    public static CollisionPoints? IntersectLinePolygon(Vector2 linePoint, Vector2 lineDirection, List<Vector2> points, int maxCollisionPoints = -1)
    {
        if (points.Count < 3) return null;
        if (maxCollisionPoints == 0) return null;
        CollisionPoints? result = null;
        for (var i = 0; i < points.Count; i++)
        {
            var colPoint = IntersectLineSegment(linePoint, lineDirection, points[i], points[(i + 1) % points.Count]);
            if (colPoint.Valid)
            {
                result ??= new();
                result.Add(colPoint);
                if(maxCollisionPoints > 0 && result.Count >= maxCollisionPoints) return result;
            }
        }
        return result;
    }
    public static CollisionPoints? IntersectLinePolyline(Vector2 linePoint, Vector2 lineDirection, List<Vector2> points, int maxCollisionPoints = -1)
    {
        if (points.Count < 3) return null;
        if (maxCollisionPoints == 0) return null;
        CollisionPoints? result = null;
        for (var i = 0; i < points.Count - 1; i++)
        {
            var colPoint = IntersectLineSegment(linePoint, lineDirection, points[i], points[i + 1]);
            if (colPoint.Valid)
            {
                result ??= new();
                result.Add(colPoint);
                if(maxCollisionPoints > 0 && result.Count >= maxCollisionPoints) return result;
            }
        }
        return result;
    }
    public static CollisionPoints? IntersectLineSegments(Vector2 linePoint, Vector2 lineDirection, List<Segment> segments, int maxCollisionPoints = -1)
    {
        if (segments.Count <= 0) return null;
        if (maxCollisionPoints == 0) return null;
        CollisionPoints? result = null;

        foreach (var seg in segments)
        {
            var colPoint = IntersectLineSegment(linePoint, lineDirection, seg.Start, seg.End);
            if (colPoint.Valid)
            {
                result ??= new();
                result.AddRange(colPoint);
                if(maxCollisionPoints > 0 && result.Count >= maxCollisionPoints) return result;
            }
        }
        return result;
    }

    public static int IntersectLinePolygon(Vector2 linePoint, Vector2 lineDirection, List<Vector2> points, ref CollisionPoints result, bool returnAfterFirstValid = false)
    {
        if (points.Count < 3) return 0;
        var count = 0;
        for (var i = 0; i < points.Count; i++)
        {
            var cp = IntersectLineSegment(linePoint, lineDirection, points[i], points[(i + 1) % points.Count]);
            if (cp.Valid)
            {
                result.Add(cp);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }
        return count;
    }
    public CollisionPoint IntersectSegment(Vector2 segmentStart, Vector2 segmentEnd) => IntersectLineSegment(Point, Direction, segmentStart, segmentEnd);
    public CollisionPoint IntersectSegment(Segment segment) => IntersectLineSegment(Point, Direction, segment.Start, segment.End, segment.Normal);
    public CollisionPoint IntersectLine(Vector2 otherPoint, Vector2 otherDirection) => IntersectLineLine(Point, Direction, otherPoint, otherDirection);
    public CollisionPoint IntersectLine(Line otherLine) => IntersectLineLine(Point, Direction, otherLine.Point, otherLine.Direction, otherLine.Normal);
    public CollisionPoint IntersectRay(Vector2 otherPoint, Vector2 otherDirection) => IntersectLineRay(Point, Direction, otherPoint, otherDirection);
    public CollisionPoint IntersectRay(Ray otherRay) => IntersectLineRay(Point, Direction, otherRay.Point, otherRay.Direction, otherRay.Normal);
    public (CollisionPoint a, CollisionPoint b) IntersectCircle(Circle otherCircle) => IntersectLineCircle(Point, Direction, otherCircle.Center, otherCircle.Radius);
    public (CollisionPoint a, CollisionPoint b) IntersectCircle(Vector2 circleCenter, float circleRadius) => IntersectLineCircle(Point, Direction, circleCenter, circleRadius);
    public (CollisionPoint a, CollisionPoint b) IntersectTriangle(Vector2 a, Vector2 b, Vector2 c) => IntersectLineTriangle(Point, Direction, a, b, c);
    public (CollisionPoint a, CollisionPoint b) IntersectTriangle(Triangle triangle) => IntersectLineTriangle(Point, Direction, triangle.A, triangle.B, triangle.C);
    public (CollisionPoint a, CollisionPoint b) IntersectQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => IntersectLineQuad(Point, Direction, a, b, c, d);
    public (CollisionPoint a, CollisionPoint b) IntersectQuad(Quad quad) => IntersectLineQuad(Point, Direction, quad.A, quad.B, quad.C, quad.D);
    public (CollisionPoint a, CollisionPoint b) IntersectRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => IntersectLineQuad(Point, Direction, a, b, c, d);
    public (CollisionPoint a, CollisionPoint b) IntersectRect(Rect rect) => IntersectLineQuad(Point, Direction, rect.A, rect.B, rect.C, rect.D);
    
    public CollisionPoints? IntersectPolygon(List<Vector2> points, int maxCollisionPoints = -1) => IntersectLinePolygon(Point, Direction, points, maxCollisionPoints);
    public CollisionPoints? IntersectPolygon(Polygon polygon, int maxCollisionPoints = -1) => IntersectLinePolygon(Point, Direction, polygon, maxCollisionPoints);
    public CollisionPoints? IntersectPolyline(List<Vector2> points, int maxCollisionPoints = -1) => IntersectLinePolyline(Point, Direction, points, maxCollisionPoints);
    public CollisionPoints? IntersectPolyline(Polyline polyline, int maxCollisionPoints = -1) => IntersectLinePolyline(Point, Direction, polyline, maxCollisionPoints);
    public CollisionPoints? IntersectSegments(List<Segment> segments, int maxCollisionPoints = -1) => IntersectLineSegments(Point, Direction, segments, maxCollisionPoints);
    public CollisionPoints? IntersectSegments(Segments segments, int maxCollisionPoints = -1) => IntersectLineSegments(Point, Direction, segments, maxCollisionPoints);
    
    public CollisionPoints? Intersect(Collider collider)
    {
        if (!collider.Enabled) return null;

        switch (collider.GetShapeType())
        {
            case ShapeType.Circle:
                var c = collider.GetCircleShape();
                return IntersectShape(c);
            case ShapeType.Ray:
                var rayShape = collider.GetRayShape();
                return IntersectShape(rayShape);
            case ShapeType.Line:
                var l = collider.GetLineShape();
                return IntersectShape(l);
            case ShapeType.Segment:
                var s = collider.GetSegmentShape();
                return IntersectShape(s);
            case ShapeType.Triangle:
                var t = collider.GetTriangleShape();
                return IntersectShape(t);
            case ShapeType.Rect:
                var r = collider.GetRectShape();
                return IntersectShape(r);
            case ShapeType.Quad:
                var q = collider.GetQuadShape();
                return IntersectShape(q);
            case ShapeType.Poly:
                var p = collider.GetPolygonShape();
                return IntersectShape(p);
            case ShapeType.PolyLine:
                var pl = collider.GetPolylineShape();
                return IntersectShape(pl);
        }

        return null;
    }
    public CollisionPoints? IntersectShape(Segment segment)
    {
        var result = IntersectLineSegment(Point, Direction, segment.Start, segment.End, segment.Normal);
        if (result.Valid)
        {
            var colPoints = new CollisionPoints();
            colPoints.Add(result);
            return colPoints;
        }

        return null;
    }
    public CollisionPoints? IntersectShape(Line line)
    {
        var result = IntersectLineLine(Point, Direction, line.Point, line.Direction, line.Normal);
        if (result.Valid)
        {
            var colPoints = new CollisionPoints();
            colPoints.Add(result);
            return colPoints;
        }

        return null;
    }
    public CollisionPoints? IntersectShape(Ray ray)
    {
        var result = IntersectLineRay(Point, Direction, ray.Point, ray.Direction, ray.Normal);
        if (result.Valid)
        {
            var colPoints = new CollisionPoints();
            colPoints.Add(result);
            return colPoints;
        }

        return null;
    }
    public CollisionPoints? IntersectShape(Circle circle)
    {
        var result = IntersectLineCircle(Point, Direction, circle.Center, circle.Radius);
        if (result.a.Valid || result.b.Valid)
        {
            var colPoints = new CollisionPoints();
            if (result.a.Valid)
            {
                colPoints.Add(result.a);
            }
            if (result.b.Valid)
            {
                colPoints.Add(result.b);
            }
            return colPoints;
        }

        return null;
    }
    public CollisionPoints? IntersectShape(Triangle t)
    {
        var result = IntersectLineTriangle(Point, Direction, t.A, t.B, t.C);
        if (result.a.Valid || result.b.Valid)
        {
            var colPoints = new CollisionPoints();
            if (result.a.Valid)
            {
                colPoints.Add(result.a);
            }
            if (result.b.Valid)
            {
                colPoints.Add(result.b);
            }
            return colPoints;
        }

        return null;
    }
    public CollisionPoints? IntersectShape(Quad q)
    {
        var result = IntersectLineQuad(Point, Direction, q.A, q.B, q.C, q.D);
        if (result.a.Valid || result.b.Valid)
        {
            var colPoints = new CollisionPoints();
            if (result.a.Valid)
            {
                colPoints.Add(result.a);
            }
            if (result.b.Valid)
            {
                colPoints.Add(result.b);
            }
            return colPoints;
        }

        return null;
    }
    public CollisionPoints? IntersectShape(Rect r)
    {
        //a test to see if 2 rays in opposite directions work
        // var result1 = Ray.IntersectRayRect(Point, Direction, r.A, r.B, r.C, r.D);
        // var result2 =  Ray.IntersectRayRect(Point, -Direction, r.A, r.B, r.C, r.D);
        //
        // if (result1.a.Valid || result1.b.Valid || result2.a.Valid || result2.b.Valid)
        // {
        //     var colPoints = new CollisionPoints();
        //     if (result1.a.Valid)
        //     {
        //         colPoints.Add(result1.a);
        //     }
        //     if (result1.b.Valid)
        //     {
        //         colPoints.Add(result1.b);
        //     }
        //     if (result2.a.Valid)
        //     {
        //         colPoints.Add(result2.a);
        //     }
        //     if (result2.b.Valid)
        //     {
        //         colPoints.Add(result2.b);
        //     }
        //     return colPoints;
        // }
        //
        // return null;
       
        var result =  IntersectLineQuad(Point, Direction, r.A, r.B, r.C, r.D);
        if (result.a.Valid || result.b.Valid)
        {
            var colPoints = new CollisionPoints();
            if (result.a.Valid)
            {
                colPoints.Add(result.a);
            }
            if (result.b.Valid)
            {
                colPoints.Add(result.b);
            }
            return colPoints;
        }
        
        return null;
    }
    public CollisionPoints? IntersectShape(Polygon p, int maxCollisionPoints = -1) => IntersectLinePolygon(Point, Direction, p, maxCollisionPoints);
    public CollisionPoints? IntersectShape(Polyline pl, int maxCollisionPoints = -1) => IntersectLinePolyline(Point, Direction, pl, maxCollisionPoints);
    public CollisionPoints? IntersectShape(Segments segments, int maxCollisionPoints = -1) => IntersectLineSegments(Point, Direction, segments, maxCollisionPoints);
    
     public int Intersect(Collider collider, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (!collider.Enabled) return 0;

        switch (collider.GetShapeType())
        {
            case ShapeType.Circle:
                var c = collider.GetCircleShape();
                return IntersectShape(c, ref points, returnAfterFirstValid);
            case ShapeType.Ray:
                var rayShape = collider.GetRayShape();
                return IntersectShape(rayShape, ref points);
            case ShapeType.Line:
                var l = collider.GetLineShape();
                return IntersectShape(l, ref points);
            case ShapeType.Segment:
                var s = collider.GetSegmentShape();
                return IntersectShape(s, ref points);
            case ShapeType.Triangle:
                var t = collider.GetTriangleShape();
                return IntersectShape(t, ref points, returnAfterFirstValid);
            case ShapeType.Rect:
                var r = collider.GetRectShape();
                return IntersectShape(r, ref points, returnAfterFirstValid);
            case ShapeType.Quad:
                var q = collider.GetQuadShape();
                return IntersectShape(q, ref points, returnAfterFirstValid);
            case ShapeType.Poly:
                var p = collider.GetPolygonShape();
                return IntersectShape(p, ref points, returnAfterFirstValid);
            case ShapeType.PolyLine:
                var pl = collider.GetPolylineShape();
                return IntersectShape(pl, ref points, returnAfterFirstValid);
        }

        return 0;
    }
    public int IntersectShape(Ray r, ref CollisionPoints points)
    {
        var cp = IntersectLineRay(Point, Direction, r.Point, r.Direction, r.Normal);
        if (cp.Valid)
        {
            points.Add(cp);
            return 1;
        }

        return 0;
    }
    public int IntersectShape(Line l, ref CollisionPoints points)
    {
        var cp = IntersectLineLine(Point, Direction, l.Point, l.Direction, l.Normal);
        if (cp.Valid)
        {
            points.Add(cp);
            return 1;
        }

        return 0;
    }
    public int IntersectShape(Segment s, ref CollisionPoints points)
    {
        var cp = IntersectLineSegment(Point, Direction, s.Start, s.End);
        if (cp.Valid)
        {
            points.Add(cp);
            return 1;
        }

        return 0;
    }
    public int IntersectShape(Circle c, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var result = IntersectLineCircle(Point, Direction, c.Center, c.Radius);

        if (result.a.Valid && result.b.Valid)
        {
            if (returnAfterFirstValid)
            {
                points.Add(result.a);
                return 1;
            }
            points.Add(result.a);
            points.Add(result.b);
            return 2;
        }
        if (result.a.Valid)
        {
            points.Add(result.a);
            return 1;
        }
        
        if (result.b.Valid)
        {
           points.Add(result.b);
           return 1;
        }

        return 0;
    }
    public int IntersectShape(Triangle t, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var cp = IntersectLineSegment(Point, Direction, t.A, t.B);
        var count = 0;
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        cp = IntersectLineSegment(Point, Direction, t.B, t.C);
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        //intersecting a triangle with a segment can not result in more than 2 intersection points
        if (count >= 2) return count;
        
        cp = IntersectLineSegment(Point, Direction, t.C, t.A);
        if (cp.Valid)
        {
            points.Add(cp);
            count++;
        }

        return count;
    }
    public int IntersectShape(Quad q, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var cp = IntersectLineSegment(Point, Direction, q.A, q.B);
        var count = 0;
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        cp = IntersectLineSegment(Point, Direction, q.B, q.C);
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        //intersecting a quad with a segment can not result in more than 2 intersection points
        if (count >= 2) return count;
        
        cp = IntersectLineSegment(Point, Direction, q.C, q.D);
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        //intersecting a quad with a segment can not result in more than 2 intersection points
        if (count >= 2) return count;
        
        cp = IntersectLineSegment(Point, Direction, q.D, q.A);
        if (cp.Valid)
        {
            points.Add(cp);
            count++;
        }
        return count;
    }
    public int IntersectShape(Rect r, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var a = r.TopLeft;
        var b = r.BottomLeft;
        
        var cp = IntersectLineSegment(Point, Direction, a, b);
        var count = 0;
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        var c = r.BottomRight;
        cp = IntersectLineSegment(Point, Direction, b, c);
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        //intersecting a rect with a segment can not result in more than 2 intersection points
        if (count >= 2) return count;
        
        var d = r.TopRight;
        cp = IntersectLineSegment(Point, Direction, c, d);
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        //intersecting a rect with a segment can not result in more than 2 intersection points
        if (count >= 2) return count;
        
        cp = IntersectLineSegment(Point, Direction, d, a);
        if (cp.Valid)
        {
            points.Add(cp);
            count++;
        }
        return count;
    }
    public int IntersectShape(Polygon p, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (p.Count < 3) return 0;
        var count = 0;
        for (var i = 0; i < p.Count; i++)
        {
            var cp = IntersectLineSegment(Point, Direction, p[i], p[(i + 1) % p.Count]);
            if (cp.Valid)
            {
                points.Add(cp);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }
        return count;
    }
    public int IntersectShape(Polyline pl, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (pl.Count < 2) return 0;
        var count = 0;
        for (var i = 0; i < pl.Count - 1; i++)
        {
            var cp = IntersectLineSegment(Point, Direction, pl[i], pl[i + 1]);
            if (cp.Valid)
            {
                points.Add(cp);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }
        return count;
    }
    public int IntersectShape(Segments shape, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (shape.Count <= 0) return 0;
        var count = 0;

        foreach (var seg in shape)
        {
            var cp = IntersectLineSegment(Point, Direction, seg.Start, seg.End);
            if (cp.Valid)
            {
                points.Add(cp);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }
        return count;
    }
   
    #endregion

    #region Overlap

    public static bool OverlapLineSegment(Vector2 linePoint, Vector2 lineDirection, Vector2 segmentStart, Vector2 segmentEnd)
    {
        // Line AB (infinite line) represented by linePoint and lineDirection
        // Line segment CD represented by segmentStart and segmentEnd

        // Calculate direction vector of the segment
        var segmentDirection = segmentEnd - segmentStart;

        // Calculate the denominator of the intersection formula
        float denominator = lineDirection.X * segmentDirection.Y - lineDirection.Y * segmentDirection.X;

        // Check if lines are parallel (denominator is zero)
        if (Math.Abs(denominator) < 1e-10)
        {
            return false;
        }

        // Calculate the intersection point using parameter t
        var difference = segmentStart - linePoint;
        float t = (difference.X * segmentDirection.Y - difference.Y * segmentDirection.X) / denominator;

        // Calculate the intersection point
        var intersection = linePoint + t * lineDirection;
        
        return Segment.IsPointOnSegment(intersection, segmentStart, segmentEnd);
    }
    public static bool OverlapLineLine(Vector2 line1Point, Vector2 line1Direction, Vector2 line2Point, Vector2 line2Direction)
    {
        // Calculate the denominator of the intersection formula
        float denominator = line1Direction.X * line2Direction.Y - line1Direction.Y * line2Direction.X;

        // Check if lines are parallel (denominator is zero)
        if (Math.Abs(denominator) < 1e-10)
        {
            return false;
        }
        // Calculate the intersection point using parameter t
        var difference = line2Point - line1Point;
        float t = (difference.X * line2Direction.Y - difference.Y * line2Direction.X) / denominator;

        return true;
    }
    public static bool OverlapLineRay(Vector2 linePoint, Vector2 lineDirection, Vector2 rayPoint, Vector2 rayDirection)
    {
        // Calculate the denominator of the intersection formula
        float denominator = lineDirection.X * rayDirection.Y - lineDirection.Y * rayDirection.X;

        // Check if lines are parallel (denominator is zero)
        if (Math.Abs(denominator) < 1e-10)
        {
            return false;
        }

        var difference = rayPoint - linePoint;
        float u = (difference.X * lineDirection.Y - difference.Y * lineDirection.X) / denominator;

        return u >= 0;
    }
    public static bool OverlapLineCircle(Vector2 linePoint, Vector2 lineDirection, Vector2 circleCenter, float circleRadius)
    {
        // Normalize the direction vector
        if (Circle.ContainsCirclePoint(circleCenter, circleRadius, linePoint)) return true;

        lineDirection = lineDirection.Normalize();

        // Vector from the line point to the circle center
        var toCircle = circleCenter - linePoint;
        
        // Projection of toCircle onto the line direction to find the closest approach
        float projectionLength = Vector2.Dot(toCircle, lineDirection);

        // Closest point on the line to the circle center
        var closestPoint = linePoint + projectionLength * lineDirection;

        // Distance from the closest point to the circle center
        float distanceToCenter = Vector2.Distance(closestPoint, circleCenter);

        // Check if the line intersects the circle
        if (distanceToCenter < circleRadius) return true;

        if (Math.Abs(distanceToCenter - circleRadius) < 1e-10) return true;

        return false;
    }
    public static bool OverlapLineTriangle(Vector2 linePoint, Vector2 lineDirection, Vector2 a, Vector2 b, Vector2 c)
    {
        if (Triangle.ContainsTrianglePoint(a, b, c, linePoint)) return true;
        
        var cp = IntersectLineSegment(linePoint, lineDirection,  a, b);
        if (cp.Valid) return true;
        
        cp = IntersectLineSegment(linePoint, lineDirection,  b, c);
        if (cp.Valid) return true;
       
        cp= IntersectLineSegment(linePoint, lineDirection,  c, a);
        if (cp.Valid) return true;

        return false;
    }
    public static bool OverlapLineQuad(Vector2 linePoint, Vector2 lineDirection, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        if (Quad.ContainsQuadPoint(a, b, c, d,  linePoint)) return true;
        
        var cp = IntersectLineSegment(linePoint, lineDirection,  a, b);
        if (cp.Valid) return true;
        
        cp = IntersectLineSegment(linePoint, lineDirection,  b, c);
        if (cp.Valid) return true;
       
        cp = IntersectLineSegment(linePoint, lineDirection,  c, d);
        if (cp.Valid) return true;

        cp = IntersectLineSegment(linePoint, lineDirection,  d, a);
        if (cp.Valid) return true;
        
        return false;
    }
    
    public static bool OverlapLineRect(Vector2 linePoint, Vector2 lineDirection, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return OverlapLineQuad(linePoint, lineDirection, a, b, c, d);
    }
    public static bool OverlapLinePolygon(Vector2 linePoint, Vector2 lineDirection, List<Vector2> points)
    {
        if (points.Count < 3) return false;
        if (Polygon.ContainsPoint(points, linePoint)) return true;
        for (var i = 0; i < points.Count; i++)
        {
            var colPoint = IntersectLineSegment(linePoint, lineDirection, points[i], points[(i + 1) % points.Count]);
            if (colPoint.Valid) return true;
        }
        return false;
    }
    public static bool OverlapLinePolyline(Vector2 linePoint, Vector2 lineDirection, List<Vector2> points)
    {
        if (points.Count < 3) return false;
        for (var i = 0; i < points.Count - 1; i++)
        {
            var colPoint = IntersectLineSegment(linePoint, lineDirection, points[i], points[i + 1]);
            if (colPoint.Valid) return true;
        }
        return false;
    }
    public static bool OverlapLineSegments(Vector2 linePoint, Vector2 lineDirection, List<Segment> segments)
    {
        if (segments.Count <= 0) return false;

        foreach (var seg in segments)
        {
            var result = IntersectLineSegment(linePoint, lineDirection, seg.Start, seg.End);
            if (result.Valid) return true;
        }
        return false;
    }

    public bool OverlapPoint(Vector2 p) => IsPointOnLine(Point, Direction, p);
    public bool OverlapSegment(Vector2 segmentStart, Vector2 segmentEnd) => OverlapLineSegment(Point, Direction, segmentStart, segmentEnd);
    public bool OverlapLine(Vector2 linePoint, Vector2 lineDirection) => OverlapLineLine(Point, Direction, linePoint, lineDirection);
    public bool OverlapRay(Vector2 rayPoint, Vector2 rayDirection) => OverlapLineRay(Point, Direction, rayPoint, rayDirection);
    public bool OverlapCircle(Vector2 circleCenter, float circleRadius) => OverlapLineCircle(Point, Direction, circleCenter, circleRadius);
    public bool OverlapTriangle(Vector2 a, Vector2 b, Vector2 c) => OverlapLineTriangle(Point, Direction, a, b, c);
    public bool OverlapQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapLineQuad(Point, Direction, a, b, c, d);
    public bool OverlapRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapLineQuad(Point, Direction, a, b, c, d);
    public bool OverlapPolygon(List<Vector2> points) => OverlapLinePolygon(Point, Direction, points);
    public bool OverlapPolyline(List<Vector2> points) => OverlapLinePolyline(Point, Direction, points);
    public bool OverlapSegments(List<Segment> segments) => OverlapLineSegments(Point, Direction, segments);
    
    
    public bool Overlap(Collider collider)
    {
        if (!collider.Enabled) return false;

        switch (collider.GetShapeType())
        {
            case ShapeType.Circle:
                var c = collider.GetCircleShape();
                return OverlapShape(c);
            case ShapeType.Segment:
                var s = collider.GetSegmentShape();
                return OverlapShape(s);
            case ShapeType.Ray:
                var rayShape = collider.GetRayShape();
                return OverlapShape(rayShape);
            case ShapeType.Line:
                var l = collider.GetLineShape();
                return OverlapShape(l);
            case ShapeType.Triangle:
                var t = collider.GetTriangleShape();
                return OverlapShape(t);
            case ShapeType.Rect:
                var r = collider.GetRectShape();
                return OverlapShape(r);
            case ShapeType.Quad:
                var q = collider.GetQuadShape();
                return OverlapShape(q);
            case ShapeType.Poly:
                var p = collider.GetPolygonShape();
                return OverlapShape(p);
            case ShapeType.PolyLine:
                var pl = collider.GetPolylineShape();
                return OverlapShape(pl);
        }

        return false;
    }
    public bool OverlapShape(Line line) => OverlapLineLine(Point, Direction, line.Point, line.Direction);
    public bool OverlapShape(Ray ray) => OverlapLineRay(Point, Direction, ray.Point, ray.Direction);
    public bool OverlapShape(Segment segment) => OverlapLineSegment(Point, Direction, segment.Start, segment.End);
    public bool OverlapShape(Circle circle) => OverlapLineCircle(Point, Direction, circle.Center, circle.Radius);
    public bool OverlapShape(Triangle t) => OverlapLineTriangle(Point, Direction, t.A, t.B, t.C);
    public bool OverlapShape(Quad q) => OverlapLineQuad(Point, Direction, q.A, q.B, q.C, q.D);
    public bool OverlapShape(Rect r) => OverlapLineQuad(Point, Direction, r.A, r.B, r.C, r.D);
    public bool OverlapShape(Polygon p) => OverlapLinePolygon(Point, Direction, p);
    public bool OverlapShape(Polyline pl) => OverlapLinePolyline(Point, Direction, pl);
    public bool OverlapShape(Segments segments) => OverlapLineSegments(Point, Direction, segments);

    #endregion
    
}

