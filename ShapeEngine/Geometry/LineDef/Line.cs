using System.Numerics;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Random;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.LineDef;

/// <summary>
/// Represents an infinite line in 2D space, defined by a point on the line and a direction vector.
/// Includes methods for geometric operations such as intersections, overlaps, and closest point calculations.
/// </summary>
public readonly partial struct Line
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
}

