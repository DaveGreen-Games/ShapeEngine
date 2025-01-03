using System.Numerics;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;
using ShapeEngine.Random;

namespace ShapeEngine.Core.Shapes;

public readonly struct Ray
{
    public static float MaxLength = float.MaxValue;
    
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
    public Segment ToSegment(float length)
    {
        if(!IsValid) return new();
        return new Segment(Point, Point + Direction * length);
    }
    public Line ToLine() => new Line(Point, Direction);
    public Ray FlipNormal() => new Ray(Point, Direction, Normal.Flip());
    public static Vector2 GetNormal(Vector2 direction, bool flippedNormal)
    {
        if (flippedNormal) return direction.GetPerpendicularLeft().Normalize();
        return direction.GetPerpendicularRight().Normalize();
    }
    public bool IsPointOnRay(Vector2 point) => IsPointOnRay(point, Point, Direction);
    
    public Rect GetBoundingBox() { return new(Point, Point + Direction * MaxLength); }
    public Rect GetBoundingBox(float length) { return new(Point, Point + Direction * length); }
    
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

    #endregion
    
    #region Closest Point
    
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
        return closestPointOnRay;
    }
    public static (Vector2 self, Vector2 other) GetClosestPointRayLine(Vector2 rayPoint, Vector2 rayDirection, Vector2 linePoint, Vector2 lineDirection, out float disSquared)
    {
        var result = Line.GetClosestPointLineRay(linePoint, lineDirection, rayPoint, rayDirection, out disSquared);
        return (result.other, result.self);
    }
    public static (Vector2 self, Vector2 other) GetClosestPointRayRay(Vector2 ray1Point, Vector2 ray1Direction, Vector2 ray2Point, Vector2 ray2Direction, out float disSquared)
    {
        var d1 = ray1Direction.Normalize();
        var d2 = ray2Direction.Normalize();

        float a = Vector2.Dot(d1, d1);
        float b = Vector2.Dot(d1, d2);
        float e = Vector2.Dot(d2, d2);
        var r = ray1Point - ray2Point;
        float c = Vector2.Dot(d1, r);
        float f = Vector2.Dot(d2, r);

        float denominator = a * e - b * b;
        float t1 = Math.Max(0, (b * f - c * e) / denominator);
        float t2 = Math.Max(0, (a * f - b * c) / denominator);

        var closestPoint1 = ray1Point + t1 * d1;
        var closestPoint2 = ray2Point + t2 * d2;
        disSquared = (closestPoint1 - closestPoint2).LengthSquared();
        return (closestPoint1, closestPoint2);
    }
    public static (Vector2 self, Vector2 other) GetClosestPointRaySegment(Vector2 rayPoint, Vector2 rayDirection, Vector2 segmentStart, Vector2 segmentEnd, out float disSquared)
    {
        var d1 = rayDirection.Normalize();
        var d2 = segmentEnd - segmentStart;

        float a = Vector2.Dot(d1, d1);
        float b = Vector2.Dot(d1, d2);
        float e = Vector2.Dot(d2, d2);
        var r = rayPoint - segmentStart;
        float c = Vector2.Dot(d1, r);
        float f = Vector2.Dot(d2, r);

        float denominator = a * e - b * b;
        float t1 = Math.Max(0, (b * f - c * e) / denominator);
        float t2 = Math.Max(0, Math.Min(1, (a * f - b * c) / denominator));

        var closestPoint1 = rayPoint + t1 * d1;
        var closestPoint2 = segmentStart + t2 * d2;
        disSquared = (closestPoint1 - closestPoint2).LengthSquared();
        return (closestPoint1, closestPoint2);
    }
    
    public static (Vector2 self, Vector2 other) GetClosestPointRayCircle(Vector2 rayPoint, Vector2 rayDirection, Vector2 circleCenter, float circleRadius, out float disSquared)
    {
        var d1 = rayDirection.Normalize();

        var toCenter = circleCenter - rayPoint;
        float projectionLength = Math.Max(0, Vector2.Dot(toCenter, d1));
        var closestPointOnRay = rayPoint + projectionLength * d1;

        var offset = (closestPointOnRay - circleCenter).Normalize() * circleRadius;
        var closestPointOnCircle = circleCenter + offset;
        disSquared = (closestPointOnRay - closestPointOnCircle).LengthSquared();
        return (closestPointOnRay, closestPointOnCircle);
    }
    
   
    public CollisionPoint GetClosestPoint(Vector2 point, out float disSquared)
    {
        var toPoint = point - Point;

        float projectionLength = Vector2.Dot(toPoint, Direction);

        if (projectionLength < 0)
        {
            disSquared = (Point - point).LengthSquared();
            return new(Point, Normal);
        }

        var closestPointOnRay = Point + projectionLength * Direction;

        disSquared = (closestPointOnRay - point).LengthSquared();
        
        var dir = (point - closestPointOnRay).Normalize();
        var dot = Vector2.Dot(dir, Normal);
        if (dot >= 0) return new(closestPointOnRay, Normal);
        return new(closestPointOnRay, -Normal);
        
    }
    public ClosestPointResult GetClosestPoint(Line other)
    {
        var result = other.GetClosestPoint(this);
        return result.Switch();
    }
    public ClosestPointResult GetClosestPoint(Ray other)
    {
        var d1 = Direction;
        var d2 = other.Direction;

        float a = Vector2.Dot(d1, d1);
        float b = Vector2.Dot(d1, d2);
        float e = Vector2.Dot(d2, d2);
        var r = Point - other.Point;
        float c = Vector2.Dot(d1, r);
        float f = Vector2.Dot(d2, r);

        float denominator = a * e - b * b;
        float t1 = Math.Max(0, (b * f - c * e) / denominator);
        float t2 = Math.Max(0, (a * f - b * c) / denominator);

        var closestPoint1 = Point + t1 * d1;
        var closestPoint2 = other.Point + t2 * d2;

        float disSquared = (closestPoint1 - closestPoint2).LengthSquared();
        return new(
            new(closestPoint1, Normal), 
            new(closestPoint2, other.Normal),
            disSquared
        );
    }
    public ClosestPointResult GetClosestPoint(Segment other)
    {
        var d1 = Direction;
        var d2 = other.Displacement;

        float a = Vector2.Dot(d1, d1);
        float b = Vector2.Dot(d1, d2);
        float e = Vector2.Dot(d2, d2);
        var r = Point - other.Start;
        float c = Vector2.Dot(d1, r);
        float f = Vector2.Dot(d2, r);

        float denominator = a * e - b * b;
        float t1 = Math.Max(0, (b * f - c * e) / denominator);
        float t2 = Math.Max(0, Math.Min(1, (a * f - b * c) / denominator));

        var closestPoint1 = Point + t1 * d1;
        var closestPoint2 = other.Start + t2 * d2;

        float disSquared = (closestPoint1 - closestPoint2).LengthSquared();
        return new(
            new(closestPoint1, Normal), 
            new(closestPoint2, other.Normal),
            disSquared
            );
    }
    public ClosestPointResult GetClosestPoint(Circle other)
    {
        var d1 = Direction;

        var toCenter = other.Center - Point;
        float projectionLength = Math.Max(0, Vector2.Dot(toCenter, d1));
        var closestPointOnRay = Point + projectionLength * d1;

        var offset = (closestPointOnRay - other.Center).Normalize() * other.Radius;
        var closestPointOnCircle = other.Center + offset;

        float disSquared = (closestPointOnRay - closestPointOnCircle).LengthSquared();
        return new(
            new(closestPointOnRay, Normal), 
            new(closestPointOnCircle, (closestPointOnCircle - other.Center).Normalize()),
            disSquared
            );
    }
    
    public ClosestPointResult GetClosestPoint(Triangle other)
    {
        var closestResult = GetClosestPointRaySegment(Point, Direction, other.A, other.B, out float minDisSquared);
        var otherNormal = (other.B - other.A);
        var otherIndex = 0;
        
        var result = GetClosestPointRaySegment(Point, Direction, other.B, other.C, out float disSquared);
        if (disSquared < minDisSquared)
        {
            minDisSquared = disSquared;
            closestResult = result;
            otherNormal = (other.C - other.B);
            otherIndex = 1;
        }
        
        result = GetClosestPointRaySegment(Point, Direction, other.C, other.A, out disSquared);
        if (disSquared < minDisSquared)
        {
            var normal = (other.A - other.C).GetPerpendicularRight().Normalize();
            return new(
                new(result.self, Normal), 
                new(result.self, normal), 
                disSquared,
                -1,
                2
                );
        }

        return new(
            new(closestResult.self, Normal), 
            new(closestResult.other, otherNormal.GetPerpendicularRight().Normalize()),
            minDisSquared,
            -1,
            otherIndex
            );
    }
    public ClosestPointResult GetClosestPoint(Quad other)
    {
        var closestResult = GetClosestPointRaySegment(Point, Direction, other.A, other.B, out float minDisSquared);
        var otherNormal = (other.B - other.A);
        var otherIndex = 0;
        
        var result = GetClosestPointRaySegment(Point, Direction, other.B, other.C, out float disSquared);
        if (disSquared < minDisSquared)
        {
            otherIndex = 1;
            minDisSquared = disSquared;
            closestResult = result;
            otherNormal = (other.C - other.B);
        }
        
        result = GetClosestPointRaySegment(Point, Direction, other.C, other.D, out disSquared);
        if (disSquared < minDisSquared)
        {
            otherIndex = 2;
            minDisSquared = disSquared;
            closestResult = result;
            otherNormal = (other.D - other.C);
        }
        
        result = GetClosestPointRaySegment(Point, Direction, other.D, other.A, out disSquared);
        if (disSquared < minDisSquared)
        {
            var normal = (other.A - other.D).GetPerpendicularRight().Normalize();
            return new(
                new(result.self, Normal), 
                new(result.self, normal),
                disSquared,
                -1,
                3
                );
        }

        return new(
            new(closestResult.self, Normal), 
            new(closestResult.other, otherNormal.GetPerpendicularRight().Normalize()),
            minDisSquared,
            -1,
            otherIndex
            );
    }
    public ClosestPointResult GetClosestPoint(Rect other)
    {
        var closestResult = GetClosestPointRaySegment(Point, Direction, other.A, other.B, out float minDisSquared);
        var otherNormal = (other.B - other.A);
        var otherIndex = 0;
        
        var result = GetClosestPointRaySegment(Point, Direction, other.B, other.C, out float disSquared);
        if (disSquared < minDisSquared)
        {
            otherIndex = 1;
            minDisSquared = disSquared;
            closestResult = result;
            otherNormal = (other.C - other.B);
        }
        
        result = GetClosestPointRaySegment(Point, Direction, other.C, other.D, out disSquared);
        if (disSquared < minDisSquared)
        {
            otherIndex = 2;
            minDisSquared = disSquared;
            closestResult = result;
            otherNormal = (other.D - other.C);
        }
        
        result = GetClosestPointRaySegment(Point, Direction, other.D, other.A, out disSquared);
        if (disSquared < minDisSquared)
        {
            var normal = (other.A - other.D).GetPerpendicularRight().Normalize();
            return new(
                new(result.self, Normal), 
                new(result.self, normal),
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
    public ClosestPointResult GetClosestPoint(Polygon other)
    {
        if (other.Count < 3) return new();
        
        var p1 = other[0];
        var p2 = other[1];
        var closestResult = GetClosestPointRaySegment(Point, Direction, p1, p2, out float minDisSquared);
        var otherNormal = (p2 - p1);
        var otherIndex = 0;
        
        for (var i = 1; i < other.Count; i++)
        {
            p1 = other[i];
            p2 = other[(i + 1) % other.Count];
            var result = GetClosestPointRaySegment(Point, Direction, p1, p2, out float disSquared);
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
    public ClosestPointResult GetClosestPoint(Polyline other)
    {
        if (other.Count < 2) return new();
        
        var p1 = other[0];
        var p2 = other[1];
        var closestResult = GetClosestPointRaySegment(Point, Direction, p1, p2, out float minDisSquared);
        var otherNormal = (p2 - p1);
        var otherIndex = 0;
        for (var i = 1; i < other.Count - 1; i++)
        {
            p1 = other[i];
            p2 = other[i + 1];
            var result = GetClosestPointRaySegment(Point, Direction, p1, p2, out float disSquared);
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

    public static bool IsPointOnRay(Vector2 point, Vector2 rayPoint, Vector2 rayDirection)
    {
        // Calculate the vector from the ray point to the given point
        var toPoint = point - rayPoint;

        // Calculate the dot product of the direction vector and the vector to the point
        float dotProduct = Vector2.Dot(toPoint, rayDirection);

        // Check if the point is in the same direction as the ray and on the line
        return dotProduct >= 0 && Line.IsPointOnLine(point, rayPoint, rayDirection);
    }
    
    
    public static (CollisionPoint p, float t) IntersectRaySegmentInfo(Vector2 rayPoint, Vector2 rayDirection, Vector2 segmentStart, Vector2 segmentEnd)
    {
        float denominator = rayDirection.X * (segmentEnd.Y - segmentStart.Y) - rayDirection.Y * (segmentEnd.X - segmentStart.X);

        if (Math.Abs(denominator) < 1e-10)
        {
            return (new(), -1f);
        }

        var difference = segmentStart - rayPoint;
        float t = (difference.X * (segmentEnd.Y - segmentStart.Y) - difference.Y * (segmentEnd.X - segmentStart.X)) / denominator;
        float u = (difference.X * rayDirection.Y - difference.Y * rayDirection.X) / denominator;

        if (t >= 0 && u >= 0 && u <= 1)
        {
            var intersection = rayPoint + t * rayDirection;
            var segmentDirection = (segmentEnd - segmentStart).Normalize();
            var normal = new Vector2(-segmentDirection.Y, segmentDirection.X);
            return (new(intersection, normal), t);
        }

        return (new(), -1f);
    }
    public static (CollisionPoint p, float t) IntersectRaySegmentInfo(Vector2 rayPoint, Vector2 rayDirection, Vector2 segmentStart, Vector2 segmentEnd, Vector2 segmentNormal)
    {
        var result = IntersectRaySegmentInfo(rayPoint, rayDirection, segmentStart, segmentEnd);
        if (result.p.Valid)
        {
            return (new(result.p.Point, segmentNormal), result.t);
        }

        return (new(), -1f);
    }
    public static (CollisionPoint p, float t) IntersectRayLineInfo(Vector2 rayPoint, Vector2 rayDirection, Vector2 linePoint, Vector2 lineDirection)
    {
        float denominator = rayDirection.X * lineDirection.Y - rayDirection.Y * lineDirection.X;

        if (Math.Abs(denominator) < 1e-10)
        {
            return (new(), -1f);
        }

        var difference = linePoint - rayPoint;
        float t = (difference.X * lineDirection.Y - difference.Y * lineDirection.X) / denominator;

        if (t >= 0)
        {
            var intersection = rayPoint + t * rayDirection;
            var normal = new Vector2(-lineDirection.Y, lineDirection.X).Normalize();
            return (new(intersection, normal), t);
        }

        return (new(), -1f);
    }
    public static (CollisionPoint p, float t) IntersectRayLineInfo(Vector2 rayPoint, Vector2 rayDirection, Vector2 linePoint, Vector2 lineDirection, Vector2 lineNormal)
    {
        var result = IntersectRayLineInfo(rayPoint, rayDirection, linePoint, lineDirection);
        if (result.p.Valid)
        {
            return (new(result.p.Point, lineNormal), result.t);
        }

        return (new(), -1f);
    }
    public static (CollisionPoint p, float t) IntersectRayRayInfo(Vector2 ray1Point, Vector2 ray1Direction, Vector2 ray2Point, Vector2 ray2Direction)
    {
        float denominator = ray1Direction.X * ray2Direction.Y - ray1Direction.Y * ray2Direction.X;

        if (Math.Abs(denominator) < 1e-10)
        {
            return (new(), -1f);
        }

        var difference = ray2Point - ray1Point;
        float t = (difference.X * ray2Direction.Y - difference.Y * ray2Direction.X) / denominator;
        float u = (difference.X * ray1Direction.Y - difference.Y * ray1Direction.X) / denominator;

        if (t >= 0 && u >= 0)
        {
            var intersection = ray1Point + t * ray1Direction;
            var normal = new Vector2(-ray2Direction.Y, ray2Direction.X).Normalize();
            return (new(intersection, normal), t);
        }

        return (new(), -1f);
    }
    public static (CollisionPoint p, float t) IntersectRayRayInfo(Vector2 ray1Point, Vector2 ray1Direction, Vector2 ray2Point, Vector2 ray2Direction, Vector2 ray2Normal)
    {
        var result = IntersectRayRayInfo(ray1Point, ray1Direction, ray2Point, ray2Direction);
        if (result.p.Valid)
        {
            return (new(result.p.Point, ray2Normal), result.t);
        }

        return (new(), -1f);
    }
    
    
    public static CollisionPoint IntersectRaySegment(Vector2 rayPoint, Vector2 rayDirection, Vector2 segmentStart, Vector2 segmentEnd)
    {
        float denominator = rayDirection.X * (segmentEnd.Y - segmentStart.Y) - rayDirection.Y * (segmentEnd.X - segmentStart.X);

        if (Math.Abs(denominator) < 1e-10)
        {
            return new();
        }

        var difference = segmentStart - rayPoint;
        float t = (difference.X * (segmentEnd.Y - segmentStart.Y) - difference.Y * (segmentEnd.X - segmentStart.X)) / denominator;
        float u = (difference.X * rayDirection.Y - difference.Y * rayDirection.X) / denominator;

        if (t >= 0 && u >= 0 && u <= 1)
        {
            var intersection = rayPoint + t * rayDirection;
            var segmentDirection = (segmentEnd - segmentStart).Normalize();
            var normal = new Vector2(-segmentDirection.Y, segmentDirection.X);
            return new(intersection, normal);
        }

        return new();
    }
    public static CollisionPoint IntersectRaySegment(Vector2 rayPoint, Vector2 rayDirection, Vector2 segmentStart, Vector2 segmentEnd, Vector2 segmentNormal)
    {
        var result = IntersectRaySegment(rayPoint, rayDirection, segmentStart, segmentEnd);
        if (result.Valid)
        {
            return new(result.Point, segmentNormal);
        }

        return new();
    }
    
    public static CollisionPoint IntersectRayLine(Vector2 rayPoint, Vector2 rayDirection, Vector2 linePoint, Vector2 lineDirection)
    {
        float denominator = rayDirection.X * lineDirection.Y - rayDirection.Y * lineDirection.X;

        if (Math.Abs(denominator) < 1e-10)
        {
            return new();
        }

        var difference = linePoint - rayPoint;
        float t = (difference.X * lineDirection.Y - difference.Y * lineDirection.X) / denominator;

        if (t >= 0)
        {
            var intersection = rayPoint + t * rayDirection;
            var normal = new Vector2(-lineDirection.Y, lineDirection.X).Normalize();
            return new(intersection, normal);
        }

        return new();
    }
    public static CollisionPoint IntersectRayLine(Vector2 rayPoint, Vector2 rayDirection, Vector2 linePoint, Vector2 lineDirection, Vector2 lineNormal)
    {
        var result = IntersectRayLine(rayPoint, rayDirection, linePoint, lineDirection);
        if (result.Valid)
        {
            return new(result.Point, lineNormal);
        }

        return new();
    }
    
    public static CollisionPoint IntersectRayRay(Vector2 ray1Point, Vector2 ray1Direction, Vector2 ray2Point, Vector2 ray2Direction)
    {
        float denominator = ray1Direction.X * ray2Direction.Y - ray1Direction.Y * ray2Direction.X;

        if (Math.Abs(denominator) < 1e-10)
        {
            return new();
        }

        var difference = ray2Point - ray1Point;
        float t = (difference.X * ray2Direction.Y - difference.Y * ray2Direction.X) / denominator;
        float u = (difference.X * ray1Direction.Y - difference.Y * ray1Direction.X) / denominator;

        if (t >= 0 && u >= 0)
        {
            var intersection = ray1Point + t * ray1Direction;
            var normal = new Vector2(-ray2Direction.Y, ray2Direction.X).Normalize();
            return new(intersection, normal);
        }

        return new();
    }
    public static CollisionPoint IntersectRayRay(Vector2 ray1Point, Vector2 ray1Direction, Vector2 ray2Point, Vector2 ray2Direction, Vector2 ray2Normal)
    {
        var result = IntersectRayRay(ray1Point, ray1Direction, ray2Point, ray2Direction);
        if (result.Valid)
        {
            return new(result.Point, ray2Normal);
        }

        return new();
    }
    
    public static (CollisionPoint a, CollisionPoint b) IntersectRayCircle(Vector2 rayPoint, Vector2 rayDirection, Vector2 circleCenter, float radius)
    {
        var toCircle = circleCenter - rayPoint;
        float projectionLength = Vector2.Dot(toCircle, rayDirection);
        var closestPoint = rayPoint + projectionLength * rayDirection;
        float distanceToCenter = Vector2.Distance(closestPoint, circleCenter);

        if (distanceToCenter < radius)
        {
            var offset = (float)Math.Sqrt(radius * radius - distanceToCenter * distanceToCenter);
            var intersection1 = closestPoint - offset * rayDirection;
            var intersection2 = closestPoint + offset * rayDirection;

            CollisionPoint a = new();
            CollisionPoint b = new();
            if (Vector2.Dot(intersection1 - rayPoint, rayDirection) >= 0)
            {
                var normal1 = (intersection1 - circleCenter).Normalize();
                a = new(intersection1, normal1);
            }

            if (Vector2.Dot(intersection2 - rayPoint, rayDirection) >= 0)
            {
                var normal2 = (intersection2 - circleCenter).Normalize();
                b = new(intersection2, normal2);
                
            }
            return (a, b);
        }
        else if (Math.Abs(distanceToCenter - radius) < 1e-10)
        {
            if (Vector2.Dot(closestPoint - rayPoint, rayDirection) >= 0)
            {
                var cp = new CollisionPoint(closestPoint, (closestPoint - circleCenter).Normalize());
                return (cp, new());
            }
        }

        return (new(), new());
    }
    public static (CollisionPoint a, CollisionPoint b) IntersectRayTriangle(Vector2 rayPoint, Vector2 rayDirection, Vector2 a, Vector2 b, Vector2 c)
    {
        CollisionPoint resultA = new();
        CollisionPoint resultB = new();
        
        var cp = IntersectRaySegment(rayPoint, rayDirection,  a, b);
        if(cp.Valid) resultA = cp;
        
        cp = IntersectRaySegment(rayPoint, rayDirection,  b, c);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }
        
        if(resultA.Valid && resultB.Valid) return (resultA, resultB);
       
        cp = IntersectRaySegment(rayPoint, rayDirection,  c, a);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }
        
        return (resultA, resultB);
    }
    public static (CollisionPoint a, CollisionPoint b) IntersectRayQuad(Vector2 rayPoint, Vector2 rayDirection, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        CollisionPoint resultA = new();
        CollisionPoint resultB = new();
        
        var cp = IntersectRaySegment(rayPoint, rayDirection,  a, b);
        if(cp.Valid) resultA = cp;
        
        cp = IntersectRaySegment(rayPoint, rayDirection,  b, c);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }
        
        if(resultA.Valid && resultB.Valid) return (resultA, resultB);
       
        cp = IntersectRaySegment(rayPoint, rayDirection,  c, d);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }
        
        if(resultA.Valid && resultB.Valid) return (resultA, resultB);
        
        cp = IntersectRaySegment(rayPoint, rayDirection,  d, a);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }
        
        return (resultA, resultB);
    }
    public static (CollisionPoint a, CollisionPoint b) IntersectRayRect(Vector2 rayPoint, Vector2 rayDirection, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return IntersectRayQuad(rayPoint, rayDirection, a, b, c, d);
    }
    public static CollisionPoints? IntersectRayPolygon(Vector2 rayPoint, Vector2 rayDirection, List<Vector2> points, int maxCollisionPoints = -1)
    {
        if (points.Count < 3) return null;
        if (maxCollisionPoints == 0) return null;
        CollisionPoints? result = null;
        for (var i = 0; i < points.Count; i++)
        {
            var colPoint = IntersectRaySegment(rayPoint, rayDirection, points[i], points[(i + 1) % points.Count]);
            if (colPoint.Valid)
            {
                result ??= new();
                result.Add(colPoint);
                if(maxCollisionPoints > 0 && result.Count >= maxCollisionPoints) return result;
            }
        }
        return result;
    }
    public static CollisionPoints? IntersectRayPolyline(Vector2 rayPoint, Vector2 rayDirection, List<Vector2> points, int maxCollisionPoints = -1)
    {
        if (points.Count < 3) return null;
        if (maxCollisionPoints == 0) return null;
        CollisionPoints? result = null;
        for (var i = 0; i < points.Count - 1; i++)
        {
            var colPoint = IntersectRaySegment(rayPoint, rayDirection, points[i], points[i + 1]);
            if (colPoint.Valid)
            {
                result ??= new();
                result.Add(colPoint);
                if(maxCollisionPoints > 0 && result.Count >= maxCollisionPoints) return result;
            }
        }
        return result;
    }
    public static CollisionPoints? IntersectRaySegments(Vector2 rayPoint, Vector2 rayDirection, List<Segment> segments, int maxCollisionPoints = -1)
    {
        if (segments.Count <= 0) return null;
        if (maxCollisionPoints == 0) return null;
        CollisionPoints? result = null;

        foreach (var seg in segments)
        {
            var colPoint = IntersectRaySegment(rayPoint, rayDirection, seg.Start, seg.End);
            if (colPoint.Valid)
            {
                result ??= new();
                result.AddRange(colPoint);
                if(maxCollisionPoints > 0 && result.Count >= maxCollisionPoints) return result;
            }
        }
        return result;
    }

    
    public CollisionPoint IntersectSegment(Vector2 segmentStart, Vector2 segmentEnd) => IntersectRaySegment(Point, Direction, segmentStart, segmentEnd);
    public CollisionPoint IntersectSegment(Segment segment) => IntersectRaySegment(Point, Direction, segment.Start, segment.End, segment.Normal);
    public CollisionPoint IntersectLine(Vector2 linePoint, Vector2 lineDirection) => IntersectRayLine(Point, Direction, linePoint, lineDirection);
    public CollisionPoint IntersectLine(Line line) => IntersectRayLine(Point, Direction, line.Point, line.Direction, line.Normal);
    public CollisionPoint IntersectRay(Vector2 rayPoint, Vector2 rayDirection) => IntersectRayRay(Point, Direction, rayPoint, rayDirection);
    public CollisionPoint IntersectRay(Ray ray) => IntersectRayRay(Point, Direction, ray.Point, ray.Direction, ray.Normal);
    public (CollisionPoint a, CollisionPoint b) IntersectCircle(Vector2 circleCenter, float circleRadius) => IntersectRayCircle(Point, Direction, circleCenter, circleRadius);
    public (CollisionPoint a, CollisionPoint b) IntersectCircle(Circle circle) => IntersectRayCircle(Point, Direction, circle.Center, circle.Radius);
    public (CollisionPoint a, CollisionPoint b) IntersectTriangle(Vector2 a, Vector2 b, Vector2 c) => IntersectRayTriangle(Point, Direction, a, b, c);
    public (CollisionPoint a, CollisionPoint b) IntersectTriangle(Triangle triangle) => IntersectRayTriangle(Point, Direction, triangle.A, triangle.B, triangle.C);
    public (CollisionPoint a, CollisionPoint b) IntersectQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => IntersectRayQuad(Point, Direction, a, b, c, d);
    public (CollisionPoint a, CollisionPoint b) IntersectQuad(Quad quad) => IntersectRayQuad(Point, Direction, quad.A, quad.B, quad.C, quad.D);
    public (CollisionPoint a, CollisionPoint b) IntersectRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => IntersectRayQuad(Point, Direction, a, b, c, d);
    public (CollisionPoint a, CollisionPoint b) IntersectRect(Rect rect) => IntersectRayQuad(Point, Direction, rect.A, rect.B, rect.C, rect.D);
    
    public CollisionPoints? IntersectPolygon(List<Vector2> points, int maxCollisionPoints = -1) => IntersectRayPolygon(Point, Direction, points, maxCollisionPoints);
    public CollisionPoints? IntersectPolygon(Polygon polygon, int maxCollisionPoints = -1) => IntersectRayPolygon(Point, Direction, polygon, maxCollisionPoints);
    public CollisionPoints? IntersectPolyline(List<Vector2> points, int maxCollisionPoints = -1) => IntersectRayPolyline(Point, Direction, points, maxCollisionPoints);
    public CollisionPoints? IntersectPolyline(Polyline polyline, int maxCollisionPoints = -1) => IntersectRayPolyline(Point, Direction, polyline, maxCollisionPoints);
    public CollisionPoints? IntersectSegments(List<Segment> segments, int maxCollisionPoints = -1) => IntersectRaySegments(Point, Direction, segments, maxCollisionPoints);
    public CollisionPoints? IntersectSegments(Segments segments, int maxCollisionPoints = -1) => IntersectRaySegments(Point, Direction, segments, maxCollisionPoints);
    
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
        var result = IntersectRaySegment(Point, Direction, segment.Start, segment.End, segment.Normal);
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
        var result = IntersectRayLine(Point, Direction, line.Point, line.Direction, line.Normal);
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
        var result = IntersectRayRay(Point, Direction, ray.Point, ray.Direction, ray.Normal);
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
        var result = IntersectCircle(circle);
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
        var result = IntersectRayTriangle(Point, Direction, t.A, t.B, t.C);
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
        var result =  IntersectRayQuad(Point, Direction, q.A, q.B, q.C, q.D);
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
        var result = IntersectRayQuad(Point, Direction, r.A, r.B, r.C, r.D);
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
    
    public CollisionPoints? IntersectShape(Polygon p, int maxCollisionPoints = -1) => IntersectRayPolygon(Point, Direction, p, maxCollisionPoints);
    public CollisionPoints? IntersectShape(Polyline pl, int maxCollisionPoints = -1) => IntersectRayPolyline(Point, Direction, pl, maxCollisionPoints);
    public CollisionPoints? IntersectShape(Segments segments, int maxCollisionPoints = -1) => IntersectRaySegments(Point, Direction, segments, maxCollisionPoints);

    
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
        var cp = IntersectRayRay(Point, Direction, r.Point, r.Direction, r.Normal);
        if (cp.Valid)
        {
            points.Add(cp);
            return 1;
        }

        return 0;
    }
    public int IntersectShape(Line l, ref CollisionPoints points)
    {
        var cp = IntersectRayLine(Point, Direction, l.Point, l.Direction, l.Normal);
        if (cp.Valid)
        {
            points.Add(cp);
            return 1;
        }

        return 0;
    }
    public int IntersectShape(Segment s, ref CollisionPoints points)
    {
        var cp = IntersectRaySegment(Point, Direction, s.Start, s.End);
        if (cp.Valid)
        {
            points.Add(cp);
            return 1;
        }

        return 0;
    }
    public int IntersectShape(Circle c, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var result = IntersectRayCircle(Point, Direction, c.Center, c.Radius);

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
        var cp = IntersectRaySegment(Point, Direction, t.A, t.B);
        var count = 0;
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        cp = IntersectRaySegment(Point, Direction, t.B, t.C);
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        //intersecting a triangle with a segment can not result in more than 2 intersection points
        if (count >= 2) return count;
        
        cp = IntersectRaySegment(Point, Direction, t.C, t.A);
        if (cp.Valid)
        {
            points.Add(cp);
            count++;
        }

        return count;
    }
    public int IntersectShape(Quad q, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var cp = IntersectRaySegment(Point, Direction, q.A, q.B);
        var count = 0;
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        cp = IntersectRaySegment(Point, Direction, q.B, q.C);
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        //intersecting a quad with a segment can not result in more than 2 intersection points
        if (count >= 2) return count;
        
        cp = IntersectRaySegment(Point, Direction, q.C, q.D);
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        //intersecting a quad with a segment can not result in more than 2 intersection points
        if (count >= 2) return count;
        
        cp = IntersectRaySegment(Point, Direction, q.D, q.A);
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
        
        var cp = IntersectRaySegment(Point, Direction, a, b);
        var count = 0;
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        var c = r.BottomRight;
        cp = IntersectRaySegment(Point, Direction, b, c);
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        //intersecting a rect with a segment can not result in more than 2 intersection points
        if (count >= 2) return count;
        
        var d = r.TopRight;
        cp = IntersectRaySegment(Point, Direction, c, d);
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        //intersecting a rect with a segment can not result in more than 2 intersection points
        if (count >= 2) return count;
        
        cp = IntersectRaySegment(Point, Direction, d, a);
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
            var cp = IntersectRaySegment(Point, Direction, p[i], p[(i + 1) % p.Count]);
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
            var cp = IntersectRaySegment(Point, Direction, pl[i], pl[i + 1]);
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
            var cp = IntersectRaySegment(Point, Direction, seg.Start, seg.End);
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

    public static bool OverlapRaySegment(Vector2 rayPoint, Vector2 rayDirection, Vector2 segmentStart, Vector2 segmentEnd)
    {
        float denominator = rayDirection.X * (segmentEnd.Y - segmentStart.Y) - rayDirection.Y * (segmentEnd.X - segmentStart.X);

        if (Math.Abs(denominator) < 1e-10)
        {
            return false;
        }

        var difference = segmentStart - rayPoint;
        float t = (difference.X * (segmentEnd.Y - segmentStart.Y) - difference.Y * (segmentEnd.X - segmentStart.X)) / denominator;
        float u = (difference.X * rayDirection.Y - difference.Y * rayDirection.X) / denominator;

        if (t >= 0 && u >= 0 && u <= 1)
        {
            return true;
        }

        return false;
    }
    public static bool OverlapRayLine(Vector2 rayPoint, Vector2 rayDirection, Vector2 linePoint, Vector2 lineDirection)
    {
        float denominator = rayDirection.X * lineDirection.Y - rayDirection.Y * lineDirection.X;

        if (Math.Abs(denominator) < 1e-10)
        {
            return false;
        }

        var difference = linePoint - rayPoint;
        float t = (difference.X * lineDirection.Y - difference.Y * lineDirection.X) / denominator;

        if (t >= 0)
        {
            return true;
        }

        return false;
    }
    public static bool OverlapRayRay(Vector2 ray1Point, Vector2 ray1Direction, Vector2 ray2Point, Vector2 ray2Direction)
    {
        float denominator = ray1Direction.X * ray2Direction.Y - ray1Direction.Y * ray2Direction.X;

        if (Math.Abs(denominator) < 1e-10)
        {
            return false;
        }

        var difference = ray2Point - ray1Point;
        float t = (difference.X * ray2Direction.Y - difference.Y * ray2Direction.X) / denominator;
        float u = (difference.X * ray1Direction.Y - difference.Y * ray1Direction.X) / denominator;

        if (t >= 0 && u >= 0)
        {
            return true;
        }

        return false;
    }
    public static bool OverlapRayCircle(Vector2 rayPoint, Vector2 rayDirection, Vector2 circleCenter, float circleRadius)
    {
        var toCircle = circleCenter - rayPoint;
        float projectionLength = Vector2.Dot(toCircle, rayDirection);
        var closestPoint = rayPoint + projectionLength * rayDirection;
        float distanceToCenter = Vector2.Distance(closestPoint, circleCenter);

        if (distanceToCenter < circleRadius)
        {
            return true;
        }
        
        if (Math.Abs(distanceToCenter - circleRadius) < 1e-10)
        {
            if (Vector2.Dot(closestPoint - rayPoint, rayDirection) >= 0)
            {
                return true;
            }
        }

        return false;
    }
    public static bool OverlapRayTriangle(Vector2 rayPoint, Vector2 rayDirection, Vector2 a, Vector2 b, Vector2 c)
    {
        if (Triangle.ContainsPoint(a, b, c, rayPoint)) return true;
        
        var cp = IntersectRaySegment(rayPoint, rayDirection,  a, b);
        if (cp.Valid) return true;
        
        cp = IntersectRaySegment(rayPoint, rayDirection,  b, c);
        if (cp.Valid) return true;
       
        cp= IntersectRaySegment(rayPoint, rayDirection,  c, a);
        if (cp.Valid) return true;

        return false;
    }
    public static bool OverlapRayQuad(Vector2 rayPoint, Vector2 rayDirection, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        if (Quad.ContainsPoint(a, b, c, d,  rayPoint)) return true;
        
        var cp = IntersectRaySegment(rayPoint, rayDirection,  a, b);
        if (cp.Valid) return true;
        
        cp = IntersectRaySegment(rayPoint, rayDirection,  b, c);
        if (cp.Valid) return true;
       
        cp = IntersectRaySegment(rayPoint, rayDirection,  c, d);
        if (cp.Valid) return true;

        cp = IntersectRaySegment(rayPoint, rayDirection,  d, a);
        if (cp.Valid) return true;
        
        return false;
    }
    
    public static bool OverlapRayRect(Vector2 rayPoint, Vector2 rayDirection, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return OverlapRayQuad(rayPoint, rayDirection, a, b, c, d);
    }
    public static bool OverlapRayPolygon(Vector2 rayPoint, Vector2 rayDirection, List<Vector2> points)
    {
        if (points.Count < 3) return false;
        if (Polygon.ContainsPoints(points, rayPoint)) return true;
        for (var i = 0; i < points.Count; i++)
        {
            var colPoint = IntersectRaySegment(rayPoint, rayDirection, points[i], points[(i + 1) % points.Count]);
            if (colPoint.Valid) return true;
        }
        return false;
    }
    public static bool OverlapRayPolyline(Vector2 rayPoint, Vector2 rayDirection, List<Vector2> points)
    {
        if (points.Count < 3) return false;
        for (var i = 0; i < points.Count - 1; i++)
        {
            var colPoint = IntersectRaySegment(rayPoint, rayDirection, points[i], points[i + 1]);
            if (colPoint.Valid) return true;
        }
        return false;
    }
    public static bool OverlapRaySegments(Vector2 rayPoint, Vector2 rayDirection, List<Segment> segments)
    {
        if (segments.Count <= 0) return false;

        foreach (var seg in segments)
        {
            var result = IntersectRaySegment(rayPoint, rayDirection, seg.Start, seg.End);
            if (result.Valid) return true;
        }
        return false;
    }

    public bool OverlapSegment(Vector2 segmentStart, Vector2 segmentEnd) => OverlapRaySegment(Point, Direction, segmentStart, segmentEnd);
    public bool OverlapLine(Vector2 linePoint, Vector2 lineDirection) => OverlapRayLine(Point, Direction, linePoint, lineDirection);
    public bool OverlapRay(Vector2 rayPoint, Vector2 rayDirection) => OverlapRayRay(Point, Direction, rayPoint, rayDirection);
    public bool OverlapCircle(Vector2 circleCenter, float circleRadius) => OverlapRayCircle(Point, Direction, circleCenter, circleRadius);
    public bool OverlapTriangle(Vector2 a, Vector2 b, Vector2 c) => OverlapRayTriangle(Point, Direction, a, b, c);
    public bool OverlapQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapRayQuad(Point, Direction, a, b, c, d);
    public bool OverlapRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapRayQuad(Point, Direction, a, b, c, d);
    public bool OverlapPolygon(List<Vector2> points) => OverlapRayPolygon(Point, Direction, points);
    public bool OverlapPolyline(List<Vector2> points) => OverlapRayPolyline(Point, Direction, points);
    public bool OverlapSegments(List<Segment> segments) => OverlapRaySegments(Point, Direction, segments);
    
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
    public bool OverlapShape(Segment segment) => OverlapRaySegment(Point, Direction, segment.Start, segment.End);
    public bool OverlapShape(Line line) => OverlapRayLine(Point, Direction, line.Point, line.Direction);
    public bool OverlapShape(Ray ray) => OverlapRayRay(Point, Direction, ray.Point, ray.Direction);
    public bool OverlapShape(Circle circle) => OverlapRayCircle(Point, Direction, circle.Center, circle.Radius);
    public bool OverlapShape(Triangle t) => OverlapRayTriangle(Point, Direction, t.A, t.B, t.C);
    public bool OverlapShape(Quad q) => OverlapRayQuad(Point, Direction, q.A, q.B, q.C, q.D);
    public bool OverlapShape(Rect r) => OverlapRayQuad(Point, Direction, r.A, r.B, r.C, r.D);
    public bool OverlapShape(Polygon p) => OverlapRayPolygon(Point, Direction, p);
    public bool OverlapShape(Polyline pl) => OverlapRayPolyline(Point, Direction, pl);
    public bool OverlapShape(Segments segments) => OverlapRaySegments(Point, Direction, segments);

    #endregion
}