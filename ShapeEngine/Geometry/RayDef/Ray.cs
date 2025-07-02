using System.Numerics;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Random;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.RayDef;

/// <summary>
/// Represents a 2D ray with a point of origin, direction, and normal.
/// </summary>
public readonly partial struct Ray
{
    /// <summary>
    /// The maximum length used for bounding box and segment calculations.
    /// </summary>
    /// <remarks>
    /// This value is used as a default for methods that require a ray length.
    /// </remarks>
    public static float MaxLength = 250000;
    
    #region Members
    /// <summary>
    /// The origin point of the ray.
    /// </summary>
    public readonly Vector2 Point;
    /// <summary>
    /// The normalized direction vector of the ray.
    /// </summary>
    public readonly Vector2 Direction;
    /// <summary>
    /// The normal vector perpendicular to the direction of the ray.
    /// </summary>
    public readonly Vector2 Normal;
    #endregion
    
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Ray"/> struct with zero vectors.
    /// </summary>
    public Ray()
    {
        Point = Vector2.Zero;
        Direction = Vector2.Zero;
        Normal = Vector2.Zero;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="Ray"/> struct with specified coordinates and direction.
    /// </summary>
    /// <param name="x">The X coordinate of the origin point.</param>
    /// <param name="y">The Y coordinate of the origin point.</param>
    /// <param name="dx">The X component of the direction vector.</param>
    /// <param name="dy">The Y component of the direction vector.</param>
    /// <param name="flippedNormal">If true, the normal is flipped to the left of the direction.</param>
    /// <remarks>
    /// The direction vector is normalized automatically.
    /// </remarks>
    public Ray(float x, float y, float dx, float dy, bool flippedNormal = false)
    {
        Point = new Vector2(x, y);
        Direction = new Vector2(dx, dy).Normalize();
        Normal = flippedNormal ? new Vector2(Direction.Y, -Direction.X) : new Vector2(-Direction.Y, Direction.X);
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="Ray"/> struct with a direction vector.
    /// </summary>
    /// <param name="direction">The direction vector. Will be normalized.</param>
    /// <param name="flippedNormal">If true, the normal is flipped to the left of the direction.</param>
    public Ray(Vector2 direction, bool flippedNormal = false)
    {
        Point = Vector2.Zero;
        Direction = direction.Normalize();
        Normal = flippedNormal ? new Vector2(Direction.Y, -Direction.X) : new Vector2(-Direction.Y, Direction.X);
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="Ray"/> struct with a point and direction.
    /// </summary>
    /// <param name="point">The origin point of the ray.</param>
    /// <param name="direction">The direction vector. Will be normalized.</param>
    /// <param name="flippedNormal">If true, the normal is flipped to the left of the direction.</param>
    public Ray(Vector2 point, Vector2 direction, bool flippedNormal = false)
    {
        Point = point;
        Direction = direction.Normalize();
        Normal = flippedNormal ? new Vector2(Direction.Y, -Direction.X) : new Vector2(-Direction.Y, Direction.X);
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="Ray"/> struct with explicit normal.
    /// </summary>
    /// <param name="point">The origin point of the ray.</param>
    /// <param name="direction">The direction vector. Will be normalized.</param>
    /// <param name="normal">The normal vector (not normalized).</param>
    /// <remarks>
    /// This constructor is internal and allows explicit normal assignment.
    /// </remarks>
    internal Ray(Vector2 point, Vector2 direction, Vector2 normal)
    {
        Point = point;
        Direction = direction.Normalize();
        Normal = normal;
    }
    #endregion
    
    #region Public Functions
    /// <summary>
    /// Gets a value indicating whether the ray is valid (direction and normal are not zero vectors).
    /// </summary>
    public bool IsValid => (Direction.X!= 0 || Direction.Y!= 0) && (Normal.X != 0 || Normal.Y != 0);

    /// <summary>
    /// Determines if the normal is flipped relative to the direction.
    /// </summary>
    /// <returns>True if the normal is flipped; otherwise, false.</returns>
    /// <remarks>
    /// The normal is considered flipped if it is perpendicular to the direction on the left side.
    /// </remarks>
    public bool IsNormalFlipped()
    {
        if(!IsValid) return false;
        return Math.Abs(Normal.X - Direction.Y) < 0.0000001f && Math.Abs(Normal.Y - (-Direction.X)) < 0.0000001f;
    }
    /// <summary>
    /// Converts the ray to a segment of the specified length.
    /// </summary>
    /// <param name="length">The length of the segment.</param>
    /// <returns>A <see cref="Segment"/> representing the ray as a segment.</returns>
    /// <remarks>
    /// If the ray is invalid, returns a default segment.
    /// </remarks>
    public Segment ToSegment(float length)
    {
        if(!IsValid) return new();
        return new Segment(Point, Point + Direction * length);
    }
    /// <summary>
    /// Converts the ray to a line.
    /// </summary>
    /// <returns>A <see cref="Line"/> representing the ray as a line.</returns>
    public Line ToLine() => new Line(Point, Direction);
    /// <summary>
    /// Returns a new ray with the normal vector flipped.
    /// </summary>
    /// <returns>A new <see cref="Ray"/> with the normal flipped.</returns>
    public Ray FlipNormal() => new Ray(Point, Direction, Normal.Flip());
    /// <summary>
    /// Gets the normal vector for a given direction.
    /// </summary>
    /// <param name="direction">The direction vector.</param>
    /// <param name="flippedNormal">If true, returns the left perpendicular; otherwise, right.</param>
    /// <returns>The normalized normal vector.</returns>
    public static Vector2 GetNormal(Vector2 direction, bool flippedNormal)
    {
        if (flippedNormal) return direction.GetPerpendicularLeft().Normalize();
        return direction.GetPerpendicularRight().Normalize();
    }
    
    /// <summary>
    /// Gets the bounding box of the ray using <see cref="MaxLength"/>.
    /// </summary>
    /// <returns>A <see cref="Rect"/> representing the bounding box.</returns>
    public Rect GetBoundingBox() { return new(Point, Point + Direction * MaxLength); }
    /// <summary>
    /// Gets the bounding box of the ray for a specified length.
    /// </summary>
    /// <param name="length">The length to use for the bounding box.</param>
    /// <returns>A <see cref="Rect"/> representing the bounding box.</returns>
    public Rect GetBoundingBox(float length) { return new(Point, Point + Direction * length); }
    /// <summary>
    /// Generates a random ray with a random direction between 0 and 359 degrees.
    /// </summary>
    /// <returns>A random <see cref="Ray"/>.</returns>
    public Ray RandomRay() => RandomRay(0, 359);
    /// <summary>
    /// Generates a random ray with a random direction between 0 and <paramref name="maxAngleDeg"/> degrees.
    /// </summary>
    /// <param name="maxAngleDeg">The maximum angle in degrees.</param>
    /// <returns>A random <see cref="Ray"/>.</returns>
    public Ray RandomRay(float maxAngleDeg) => RandomRay(0, maxAngleDeg);
    /// <summary>
    /// Generates a random ray with a random direction between <paramref name="minAngleDeg"/> and <paramref name="maxAngleDeg"/> degrees.
    /// </summary>
    /// <param name="minAngleDeg">The minimum angle in degrees.</param>
    /// <param name="maxAngleDeg">The maximum angle in degrees.</param>
    /// <returns>A random <see cref="Ray"/>.</returns>
    public Ray RandomRay(float minAngleDeg, float maxAngleDeg) => RandomRay(Vector2.Zero, 0, 0, minAngleDeg, maxAngleDeg);
    /// <summary>
    /// Generates a random ray from a specified origin, length, and angle range.
    /// </summary>
    /// <param name="origin">The origin point for the ray.</param>
    /// <param name="minLength">The minimum length from the origin.</param>
    /// <param name="maxLength">The maximum length from the origin.</param>
    /// <param name="minAngleDeg">The minimum angle in degrees.</param>
    /// <param name="maxAngleDeg">The maximum angle in degrees.</param>
    /// <returns>A random <see cref="Ray"/>.</returns>
    /// <remarks>
    /// If minLength or maxLength are invalid, the origin is used as the point.
    /// </remarks>
    public Ray RandomRay(Vector2 origin, float minLength, float maxLength, float minAngleDeg, float maxAngleDeg)
    {
        Vector2 point;
        if(minLength < 0 || maxLength < 0 || minLength >= maxLength) point = origin;
        else point = origin + Rng.Instance.RandVec2(minLength, maxLength);
        return new(point, Rng.Instance.RandVec2(minAngleDeg, maxAngleDeg));
    }
    /// <summary>
    /// Returns a new ray with the specified point.
    /// </summary>
    /// <param name="newPoint">The new origin point.</param>
    /// <returns>A new <see cref="Ray"/> with the updated point.</returns>
    public Ray SetPoint(Vector2 newPoint) => new Ray(newPoint, Direction, Normal);
    /// <summary>
    /// Returns a new ray with the point changed by the specified amount.
    /// </summary>
    /// <param name="amount">The amount to change the point by.</param>
    /// <returns>A new <see cref="Ray"/> with the updated point.</returns>
    public Ray ChangePoint(Vector2 amount) => new Ray(Point + amount, Direction, Normal);
    /// <summary>
    /// Returns a new ray with the specified direction.
    /// </summary>
    /// <param name="newDirection">The new direction vector.</param>
    /// <returns>A new <see cref="Ray"/> with the updated direction.</returns>
    /// <remarks>
    /// The normal is recalculated based on whether it was previously flipped.
    /// </remarks>
    public Ray SetDirection(Vector2 newDirection)
    {
        var normalFlipped = IsNormalFlipped();
        return new Ray(Point, newDirection, normalFlipped);
    }
    /// <summary>
    /// Returns a new ray with the direction changed by the specified amount.
    /// </summary>
    /// <param name="amount">The amount to change the direction by.</param>
    /// <returns>A new <see cref="Ray"/> with the updated direction.</returns>
    public Ray ChangeDirection(Vector2 amount)
    {
        var normalFlipped = IsNormalFlipped();
        return new Ray(Point, Direction + amount, normalFlipped);
    }

    /// <summary>
    /// Returns a new ray with the rotation changed by the specified angle.
    /// </summary>
    /// <param name="angleRad">The angle in radians to change the rotation by.</param>
    /// <returns>A new <see cref="Ray"/> with the updated rotation.</returns>
    /// <remarks>
    /// The normal is recalculated based on whether it was previously flipped.
    /// </remarks>
    public Ray ChangeRotation(float angleRad)
    {
        var normalFlipped = IsNormalFlipped();
        var newDir = Direction.Rotate(angleRad);
        return new Ray(Point, newDir, normalFlipped);
    }
    /// <summary>
    /// Returns a new ray with the rotation set to the specified angle.
    /// </summary>
    /// <param name="angleRad">The angle in radians to set the rotation to.</param>
    /// <returns>A new <see cref="Ray"/> with the rotation set.</returns>
    /// <remarks>
    /// The normal is recalculated based on whether it was previously flipped.
    /// </remarks>
    public Ray SetRotation(float angleRad)
    {
        var normalFlipped = IsNormalFlipped();
        var newDir = ShapeVec.VecFromAngleRad(angleRad);
        return new Ray(Point, newDir, normalFlipped);
    }
    #endregion
}