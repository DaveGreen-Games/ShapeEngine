using System.Numerics;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Geometry.Segment;
using ShapeEngine.Random;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.Ray;

public readonly partial struct Ray
{
    public static float MaxLength = 250000;
    
    #region Members
    
    public readonly Vector2 Point;
    public readonly Vector2 Direction;
    public readonly Vector2 Normal;

    #endregion
    
    #region Constructors
    public Ray()
    {
        Point = Vector2.Zero;
        Direction = Vector2.Zero;
        Normal = Vector2.Zero;
    }
    public Ray(float x, float y, float dx, float dy, bool flippedNormal = false)
    {
        Point = new Vector2(x, y);
        Direction = new Vector2(dx, dy).Normalize();
        Normal = flippedNormal ? new Vector2(Direction.Y, -Direction.X) : new Vector2(-Direction.Y, Direction.X);
    }
    public Ray(Vector2 direction, bool flippedNormal = false)
    {
        Point = Vector2.Zero;
        Direction = direction.Normalize();
        Normal = flippedNormal ? new Vector2(Direction.Y, -Direction.X) : new Vector2(-Direction.Y, Direction.X);
    }
    public Ray(Vector2 point, Vector2 direction, bool flippedNormal = false)
    {
        Point = point;
        Direction = direction.Normalize();
        Normal = flippedNormal ? new Vector2(Direction.Y, -Direction.X) : new Vector2(-Direction.Y, Direction.X);
    }

    internal Ray(Vector2 point, Vector2 direction, Vector2 normal)
    {
        Point = point;
        Direction = direction.Normalize();
        Normal = normal;
    }
    #endregion
    
    #region Public Functions
    public bool IsValid => (Direction.X!= 0 || Direction.Y!= 0) && (Normal.X != 0 || Normal.Y != 0);

    public bool IsNormalFlipped()
    {
        if(!IsValid) return false;
        return Math.Abs(Normal.X - Direction.Y) < 0.0000001f && Math.Abs(Normal.Y - (-Direction.X)) < 0.0000001f;
    }
    public Segment.Segment ToSegment(float length)
    {
        if(!IsValid) return new();
        return new Segment.Segment(Point, Point + Direction * length);
    }
    public Line.Line ToLine() => new Line.Line(Point, Direction);
    public Ray FlipNormal() => new Ray(Point, Direction, Normal.Flip());
    public static Vector2 GetNormal(Vector2 direction, bool flippedNormal)
    {
        if (flippedNormal) return direction.GetPerpendicularLeft().Normalize();
        return direction.GetPerpendicularRight().Normalize();
    }
    // public bool IsPointOnRay(Vector2 point) => IsPointOnRay(point, Point, Direction);
    
    public Rect.Rect GetBoundingBox() { return new(Point, Point + Direction * MaxLength); }
    public Rect.Rect GetBoundingBox(float length) { return new(Point, Point + Direction * length); }
    
    public Ray RandomRay() => RandomRay(0, 359);
    public Ray RandomRay(float maxAngleDeg) => RandomRay(0, maxAngleDeg);
    public Ray RandomRay(float minAngleDeg, float maxAngleDeg) => RandomRay(Vector2.Zero, 0, 0, minAngleDeg, maxAngleDeg);
    public Ray RandomRay(Vector2 origin, float minLength, float maxLength, float minAngleDeg, float maxAngleDeg)
    {
        Vector2 point;
        if(minLength < 0 || maxLength < 0 || minLength >= maxLength) point = origin;
        else point = origin + Rng.Instance.RandVec2(minLength, maxLength);
        return new(point, Rng.Instance.RandVec2(minAngleDeg, maxAngleDeg));
    }
    public Ray SetPoint(Vector2 newPoint) => new Ray(newPoint, Direction, Normal);
    public Ray ChangePoint(Vector2 amount) => new Ray(Point + amount, Direction, Normal);
    
    public Ray SetDirection(Vector2 newDirection)
    {
        var normalFlipped = IsNormalFlipped();
        return new Ray(Point, newDirection, normalFlipped);
    }
    public Ray ChangeDirection(Vector2 amount)
    {
        var normalFlipped = IsNormalFlipped();
        return new Ray(Point, Direction + amount, normalFlipped);
    }

    public Ray ChangeRotation(float angleRad)
    {
        var normalFlipped = IsNormalFlipped();
        var newDir = Direction.Rotate(angleRad);
        return new Ray(Point, newDir, normalFlipped);
    }
    public Ray SetRotation(float angleRad)
    {
        var normalFlipped = IsNormalFlipped();
        var newDir = ShapeVec.VecFromAngleRad(angleRad);
        return new Ray(Point, newDir, normalFlipped);
    }
    #endregion
}