using System.Numerics;
using ShapeEngine.Core.CollisionSystem;
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
    /*
    public static CollisionPoint GetAggregatedCollisionPoint(CollisionPoint cur, CollisionPoint next,  CollisionPointAggregationType aggregationType, Vector2 referenceV, float value, out float newValue)
    {
        newValue = 0f;
        if (!cur.Valid) return next;
        if (!next.Valid) return cur;
        
        switch (aggregationType)
        {
            case CollisionPointAggregationType.First: return new();
            
            case CollisionPointAggregationType.Closest:
                var minDisSquared = (next.Point - referenceV).LengthSquared();
                if (minDisSquared < value || value < 0)
                {
                    newValue = minDisSquared;
                    return next;
                }

                return cur;
                
            case CollisionPointAggregationType.Furthest:
                var maxDisSquared = (next.Point - referenceV).LengthSquared();
                if (maxDisSquared > value || value < 0)
                {
                    newValue = maxDisSquared;
                    return next;
                }

                return cur;
            
            case CollisionPointAggregationType.Combined: return cur.Combine(next);
            
            case CollisionPointAggregationType.PointingTowards:
                float minDot = next.Normal.Dot(referenceV);
                if (minDot < value || value == 0f)
                {
                    newValue = minDot;
                    return next;
                }
                return cur;
            case CollisionPointAggregationType.PointingAway:
                float maxDot = next.Normal.Dot(referenceV);
                if (maxDot > value || value == 0f)
                {
                    newValue = maxDot;
                    return next;
                }
                return cur;;
        }

        return new();
    }
    */

    public static bool IsCloser(CollisionPoint p, Vector2 referencePoint, float curMinDisSquared, out float newMinDisSquared)
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
    public static bool IsFurther(CollisionPoint p, Vector2 referencePoint, float curMaxDisSquared, out float newMaxDisSquared)
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
    /// Returns true if the reference direction is pointing in the same direction as the normal of the collision point. Dot values greater than 0 mean pointing towards.
    /// </summary>
    /// <param name="p">The collision point to check.</param>
    /// <param name="referenceDir">The reference direction to check against.</param>
    /// <param name="curDot">The cur maximum dot value from the previous collision points. If 0 returns true automatically with the new dot.</param>
    /// <param name="newDot">The new maximum dot value. If the normal of p is pointing more in the same direction of the reference direction than cur dot suggests. </param>
    /// <returns></returns>
    public static bool IsPointingTowards(CollisionPoint p, Vector2 referenceDir, float curDot, out float newDot)
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
    /// Returns true if the reference direction is pointing in the opposite  direction as the normal of the collision point. Dot values smaller than 0 mean pointing away.
    /// </summary>
    /// <param name="p">The collision point to check.</param>
    /// <param name="referenceDir">The reference direction to check against.</param>
    /// <param name="curDot">The cur minimum dot value from the previous collision points. If 0 return true automatically with the new dot.</param>
    /// <param name="newDot">The new minimum dot value. If the normal from p is pointing more in the opposite direction of the reference direction than cur dot suggests.</param>
    /// <returns></returns>
    public static bool IsPointingAway(CollisionPoint p, Vector2 referenceDir, float curDot, out float newDot)
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
    public CollisionPoint Combine(CollisionPoint other) => new((Point + other.Point) / 2, (Normal + other.Normal).Normalize());
    
    public static CollisionPoint Combine(CollisionPoint a, CollisionPoint b) => new((a.Point + b.Point) / 2, (a.Normal + b.Normal).Normalize());

    public static CollisionPoint Combine(params CollisionPoint[] points)
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