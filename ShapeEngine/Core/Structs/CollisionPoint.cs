using System.Numerics;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Structs;

public readonly struct CollisionPoint : IEquatable<CollisionPoint>
{
    #region Members
    
    public bool Valid => Normal.X != 0f || Normal.Y != 0f;
    public readonly Vector2 Point;
    public readonly Vector2 Normal;

    #endregion
    
    #region Constructors
    public CollisionPoint() 
    { 
        Point = new(); 
        Normal = new();
    }

    public CollisionPoint(Vector2 p, Vector2 n)
    {
        Point = p; 
        Normal = n;
    }
    
    #endregion
    
    #region Public Functions
    public CollisionPoint Average(CollisionPoint other) => new((Point + other.Point) / 2, (Normal + other.Normal).Normalize());
    
    public static CollisionPoint Average(CollisionPoint a, CollisionPoint b) => new((a.Point + b.Point) / 2, (a.Normal + b.Normal).Normalize());

    public static CollisionPoint Average(params CollisionPoint[] points)
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
    
    public bool Equals(CollisionPoint other)
    {
        return other.Point == Point && other.Normal == Normal;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(Point, Normal);
    }

    public CollisionPoint FlipNormal()
    {
        return new(Point, Normal.Flip());
    }
    public CollisionPoint FlipNormal(Vector2 referencePoint)
    {
        Vector2 dir = referencePoint - Point;
        if (dir.IsFacingTheOppositeDirection(Normal)) return FlipNormal();

        return this;
    }
    
    public bool IsNormalFacing(Vector2 referenceDir) => Normal.IsFacingTheSameDirection(referenceDir);
    public bool IsNormalFacingPoint(Vector2 referencePoint) => IsNormalFacing(referencePoint - Point);
    
    #endregion
    
    #region Math

    public CollisionPoint RotateNormal(float angleRad) => !Valid ? this : new(Point, Normal.Rotate(angleRad));

    public CollisionPoint RotateNormalDeg(float angleDeg) => !Valid ? this : new(Point, Normal.Rotate(angleDeg * ShapeMath.DEGTORAD));

    public CollisionPoint SetPoint(Vector2 newPoint) => new(newPoint, Normal);

    public CollisionPoint SetNormal(Vector2 newNormal) => new(Point, newNormal.Normalize());

    #endregion
    
    #region Operators
    
    /// <summary>
    /// Add point a to point b, and add normal a to normal b and normailze it.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static CollisionPoint operator +(CollisionPoint a, CollisionPoint b)
    {
        return new(a.Point + b.Point, (a.Normal + b.Normal).Normalize());
    }

    /// <summary>
    /// Subtract point b from point a, and subtract normal b from normal a and normailze it.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static CollisionPoint operator -(CollisionPoint a, CollisionPoint b)
    {
        return new(a.Point - b.Point, (a.Normal - b.Normal).Normalize());
    }

    /// <summary>
    /// Multiply point a by scalar, and keep normal unchanged.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="scalar"></param>
    /// <returns></returns>
    public static CollisionPoint operator *(CollisionPoint a, float scalar)
    {
        return new(a.Point * scalar, a.Normal);
    }

    /// <summary>
    /// Divide point a by scalar, and keep normal unchanged.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="scalar"></param>
    /// <returns></returns>
    public static CollisionPoint operator /(CollisionPoint a, float scalar)
    {
        return new(a.Point / scalar, a.Normal);
    }
    
    /// <summary>
    /// Add vector b to point a, and keep normal unchanged.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static CollisionPoint operator +(CollisionPoint a, Vector2  b)
    {
        return new(a.Point + b, a.Normal);
    }

    /// <summary>
    /// Subtract vector b from point a, and keep normal unchanged.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static CollisionPoint operator -(CollisionPoint a, Vector2  b)
    {
        return new(a.Point - b, a.Normal);
    }

    /// <summary>
    /// Multiply point a by vector b, and keep normal unchanged.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static CollisionPoint operator *(CollisionPoint a, Vector2 b)
    {
        return new(a.Point * b, a.Normal);
    }

    /// <summary>
    /// Divide point a by vector b, and keep normal unchanged.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static CollisionPoint operator /(CollisionPoint a, Vector2 b)
    {
        return new(a.Point / b, a.Normal);
    }
    
    
    #endregion
}