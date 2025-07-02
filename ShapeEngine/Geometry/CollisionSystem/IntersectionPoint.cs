using System.Numerics;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.CollisionSystem;

/// <summary>
/// Represents a single intersection point, including its position and normal vector.
/// </summary>
/// <remarks>
/// Provides utility methods for combining, comparing, and manipulating collision points and their normals.
/// </remarks>
public readonly struct IntersectionPoint : IEquatable<IntersectionPoint>
{
    #region Members
    /// <summary>
    /// Gets whether this intersection point is valid (normal is not zero).
    /// </summary>
    public bool Valid => Normal.X != 0f || Normal.Y != 0f;
    /// <summary>
    /// The position of the intersection point.
    /// </summary>
    public readonly Vector2 Point;
    /// <summary>
    /// The normal vector at the intersection point.
    /// </summary>
    public readonly Vector2 Normal;

    #endregion
    
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="IntersectionPoint"/> struct with default values (zero point and zero normal).
    /// </summary>
    public IntersectionPoint() 
    { 
        Point = new(); 
        Normal = new();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IntersectionPoint"/> struct with the specified point and normal.
    /// </summary>
    /// <param name="p">The position of the intersection point.</param>
    /// <param name="n">The normal vector at the intersection point.</param>
    public IntersectionPoint(Vector2 p, Vector2 n)
    {
        Point = p; 
        Normal = n;
    }

    #endregion
    
    #region Public Functions
    /// <summary>
    /// Gets a segment representing the normal at this intersection point.
    /// </summary>
    /// <param name="length">The length of the segment.</param>
    /// <returns>A segment from the intersection point in the direction of the normal.</returns>
    public Segment GetNormalSegment(float length) => new Segment(Point, Point + Normal * length);
    /// <summary>
    /// Gets a ray representing the normal at this intersection point.
    /// </summary>
    /// <returns>A ray from the intersection point in the direction of the normal.</returns>
    public Ray GetNormalRay() => new Ray(Point, Normal);
    /// <summary>
    /// Gets a line representing the normal at this intersection point.
    /// </summary>
    /// <returns>A line from the intersection point in the direction of the normal.</returns>
    public Line GetNormalLine() => new Line(Point, Normal);
    
    /// <summary>
    /// Determines if this intersection point is closer to the reference point than the current minimum distance squared.
    /// </summary>
    /// <param name="p">The intersection point to check.</param>
    /// <param name="referencePoint">The reference point.</param>
    /// <param name="curMinDisSquared">The current minimum distance squared.</param>
    /// <param name="newMinDisSquared">The new minimum distance squared, if this point is closer.</param>
    /// <returns>True if this point is closer, false otherwise.</returns>
    public static bool IsCloser(IntersectionPoint p, Vector2 referencePoint, float curMinDisSquared, out float newMinDisSquared)
    {
        var disSquared = (p.Point - referencePoint).LengthSquared();
        if (disSquared < curMinDisSquared || curMinDisSquared < 0)
        {
            newMinDisSquared = disSquared;
            return true;
        }
        
        newMinDisSquared = curMinDisSquared;
        return false;
    }
    /// <summary>
    /// Determines if this intersection point is further from the reference point than the current maximum distance squared.
    /// </summary>
    /// <param name="p">The intersection point to check.</param>
    /// <param name="referencePoint">The reference point.</param>
    /// <param name="curMaxDisSquared">The current maximum distance squared.</param>
    /// <param name="newMaxDisSquared">The new maximum distance squared, if this point is further.</param>
    /// <returns>True if this point is further away, false otherwise.</returns>
    public static bool IsFurther(IntersectionPoint p, Vector2 referencePoint, float curMaxDisSquared, out float newMaxDisSquared)
    {
        var disSquared = (p.Point - referencePoint).LengthSquared();
        if (disSquared > curMaxDisSquared || curMaxDisSquared < 0)
        {
            newMaxDisSquared = disSquared;
            return true;
        }
        
        newMaxDisSquared = curMaxDisSquared;
        return false;
    }

    /// <summary>
    /// Returns true if the reference direction is pointing in the same direction as the normal of the intersection point. Dot values greater than 0 mean pointing towards.
    /// </summary>
    /// <param name="p">The intersection point to check.</param>
    /// <param name="referenceDir">The reference direction to check against.</param>
    /// <param name="curDot">The cur maximum dot value from the previous collision points. If 0 returns true automatically with the new dot.</param>
    /// <param name="newDot">The new maximum dot value. If the normal of p is pointing more in the same direction of the reference direction than cur dot suggests. </param>
    /// <returns></returns>
    public static bool IsPointingTowards(IntersectionPoint p, Vector2 referenceDir, float curDot, out float newDot)
    {
        var dot = p.Normal.Dot(referenceDir);
        if (dot > curDot || curDot == 0f)
        {
            newDot = dot;
            return true;
        }
        
        newDot = curDot;
        return false;
    }
    /// <summary>
    /// Returns true if the reference direction is pointing in the opposite  direction as the normal of the intersection point. Dot values smaller than 0 mean pointing away.
    /// </summary>
    /// <param name="p">The intersection point to check.</param>
    /// <param name="referenceDir">The reference direction to check against.</param>
    /// <param name="curDot">The cur minimum dot value from the previous collision points. If 0 return true automatically with the new dot.</param>
    /// <param name="newDot">The new minimum dot value. If the normal from p is pointing more in the opposite direction of the reference direction than cur dot suggests.</param>
    /// <returns></returns>
    public static bool IsPointingAway(IntersectionPoint p, Vector2 referenceDir, float curDot, out float newDot)
    {
        var dot = p.Normal.Dot(referenceDir);
        if (dot < curDot || curDot == 0f)
        {
            newDot = dot;
            return true;
        }
        
        newDot = curDot;
        return false;
    }
    /// <summary>
    /// Combines this intersection point with another by averaging their positions and normals.
    /// </summary>
    /// <param name="other">The other intersection point to combine with.</param>
    /// <returns>A new intersection point representing the combination of this point and the other.</returns>
    public IntersectionPoint Combine(IntersectionPoint other) => new((Point + other.Point) / 2, (Normal + other.Normal).Normalize());
    
    /// <summary>
    /// Static method to combine two collision points by averaging their positions and normals.
    /// </summary>
    /// <param name="a">The first intersection point.</param>
    /// <param name="b">The second intersection point.</param>
    /// <returns>A new intersection point representing the combination of the two points.</returns>
    public static IntersectionPoint Combine(IntersectionPoint a, IntersectionPoint b) => new((a.Point + b.Point) / 2, (a.Normal + b.Normal).Normalize());

    /// <summary>
    /// Static method to combine multiple collision points by averaging their positions and normals.
    /// </summary>
    /// <param name="points">The array of collision points to combine.</param>
    /// <returns>A new intersection point representing the combination of all provided points.</returns>
    public static IntersectionPoint Combine(params IntersectionPoint[] points)
    {
        if(points.Length == 0) return new();
        var avgPoint = Vector2.Zero;
        var avgNormal = Vector2.Zero;
        foreach (var point in points)
        {
            avgPoint += point.Point;
            avgNormal += point.Normal;
        }
        return new(avgPoint / points.Length, avgNormal.Normalize());
    }
    
    /// <summary>
    /// Checks for equality with another intersection point.
    /// </summary>
    /// <param name="other">The other intersection point to compare with.</param>
    /// <returns>True if the points and normals are equal, false otherwise.</returns>
    public bool Equals(IntersectionPoint other)
    {
        return other.Point == Point && other.Normal == Normal;
    }
    /// <summary>
    /// Gets the hash code for this intersection point.
    /// </summary>
    /// <returns>A hash code representing this intersection point.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Point, Normal);
    }

    /// <summary>
    /// Flips the normal of the intersection point, keeping the position the same.
    /// </summary>
    /// <returns>A new intersection point with the normal flipped.</returns>
    public IntersectionPoint FlipNormal()
    {
        return new(Point, Normal.Flip());
    }
    /// <summary>
    /// Flips the normal of the intersection point if the direction to the reference point is facing the opposite way of the normal.
    /// </summary>
    /// <param name="referencePoint">The reference point to check the direction against.</param>
    /// <returns>This intersection point if the normal is not facing the opposite direction of the reference point, otherwise a new intersection point with the normal flipped.</returns>
    public IntersectionPoint FlipNormal(Vector2 referencePoint)
    {
        Vector2 dir = referencePoint - Point;
        if (dir.IsFacingTheOppositeDirection(Normal)) return FlipNormal();

        return this;
    }
    
    /// <summary>
    /// Checks if the normal is facing the same direction as the reference direction.
    /// </summary>
    /// <param name="referenceDir">The reference direction to check.</param>
    /// <returns>True if the normal is facing the same direction, false otherwise.</returns>
    public bool IsNormalFacing(Vector2 referenceDir) => Normal.IsFacingTheSameDirection(referenceDir);
    /// <summary>
    /// Checks if the normal is facing towards the reference point.
    /// </summary>
    /// <param name="referencePoint">The reference point to check.</param>
    /// <returns>True if the normal is facing the reference point, false otherwise.</returns>
    public bool IsNormalFacingPoint(Vector2 referencePoint) => IsNormalFacing(referencePoint - Point);
    
    #endregion
    
    #region Math

    /// <summary>
    /// Rotates the normal of the intersection point by the given angle in radians.
    /// </summary>
    /// <param name="angleRad">The angle in radians to rotate the normal.</param>
    /// <returns>A new intersection point with the normal rotated by the given angle.</returns>
    public IntersectionPoint RotateNormal(float angleRad) => !Valid ? this : new(Point, Normal.Rotate(angleRad));

    /// <summary>
    /// Rotates the normal of the intersection point by the given angle in degrees.
    /// </summary>
    /// <param name="angleDeg">The angle in degrees to rotate the normal.</param>
    /// <returns>A new intersection point with the normal rotated by the given angle.</returns>
    public IntersectionPoint RotateNormalDeg(float angleDeg) => !Valid ? this : new(Point, Normal.Rotate(angleDeg * ShapeMath.DEGTORAD));

    /// <summary>
    /// Sets a new point for the intersection point, keeping the normal the same.
    /// </summary>
    /// <param name="newPoint">The new position for the intersection point.</param>
    /// <returns>A new intersection point with the updated position.</returns>
    public IntersectionPoint SetPoint(Vector2 newPoint) => new(newPoint, Normal);

    /// <summary>
    /// Sets a new normal for the intersection point.
    /// </summary>
    /// <param name="newNormal">The new normal vector for the intersection point.</param>
    /// <returns>A new intersection point with the updated normal.</returns>
    public IntersectionPoint SetNormal(Vector2 newNormal) => new(Point, newNormal.Normalize());

    #endregion
    
    #region Operators
    
    /// <summary>
    /// Add point a to point b, and add normal a to normal b and normalize it.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static IntersectionPoint operator +(IntersectionPoint a, IntersectionPoint b)
    {
        return new(a.Point + b.Point, (a.Normal + b.Normal).Normalize());
    }

    /// <summary>
    /// Subtract point b from point a, and subtract normal b from normal a and normalize it.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static IntersectionPoint operator -(IntersectionPoint a, IntersectionPoint b)
    {
        return new(a.Point - b.Point, (a.Normal - b.Normal).Normalize());
    }

    /// <summary>
    /// Multiply point a by scalar, and keep normal unchanged.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="scalar"></param>
    /// <returns></returns>
    public static IntersectionPoint operator *(IntersectionPoint a, float scalar)
    {
        return new(a.Point * scalar, a.Normal);
    }

    /// <summary>
    /// Divide point a by scalar, and keep normal unchanged.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="scalar"></param>
    /// <returns></returns>
    public static IntersectionPoint operator /(IntersectionPoint a, float scalar)
    {
        return new(a.Point / scalar, a.Normal);
    }
    
    /// <summary>
    /// Add vector b to point a, and keep normal unchanged.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static IntersectionPoint operator +(IntersectionPoint a, Vector2  b)
    {
        return new(a.Point + b, a.Normal);
    }

    /// <summary>
    /// Subtract vector b from point a, and keep normal unchanged.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static IntersectionPoint operator -(IntersectionPoint a, Vector2  b)
    {
        return new(a.Point - b, a.Normal);
    }

    /// <summary>
    /// Multiply point a by vector b, and keep normal unchanged.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static IntersectionPoint operator *(IntersectionPoint a, Vector2 b)
    {
        return new(a.Point * b, a.Normal);
    }

    /// <summary>
    /// Divide point a by vector b, and keep normal unchanged.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static IntersectionPoint operator /(IntersectionPoint a, Vector2 b)
    {
        return new(a.Point / b, a.Normal);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is IntersectionPoint point && Equals(point);
    }

    /// <summary>
    /// Determines whether two <see cref="IntersectionPoint"/> instances are equal by comparing their points and normals.
    /// </summary>
    /// <param name="left">The first <see cref="IntersectionPoint"/> to compare.</param>
    /// <param name="right">The second <see cref="IntersectionPoint"/> to compare.</param>
    /// <returns>True if both the point and normal are equal; otherwise, false.</returns>
    public static bool operator ==(IntersectionPoint left, IntersectionPoint right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two <see cref="IntersectionPoint"/> instances are not equal by comparing their points and normals.
    /// </summary>
    /// <param name="left">The first <see cref="IntersectionPoint"/> to compare.</param>
    /// <param name="right">The second <see cref="IntersectionPoint"/> to compare.</param>
    /// <returns>True if either the point or normal are not equal; otherwise, false.</returns>
    public static bool operator !=(IntersectionPoint left, IntersectionPoint right)
    {
        return !(left == right);
    }


    #endregion
}