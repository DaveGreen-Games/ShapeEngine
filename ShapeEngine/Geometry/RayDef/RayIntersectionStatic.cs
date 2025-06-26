using System.Numerics;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.RayDef;

public readonly partial struct Ray
{
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

    public static (CollisionPoint p, float t) IntersectRaySegmentInfo(Vector2 rayPoint, Vector2 rayDirection, Vector2 segmentStart, Vector2 segmentEnd,
        Vector2 segmentNormal)
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

    public static (CollisionPoint p, float t) IntersectRayLineInfo(Vector2 rayPoint, Vector2 rayDirection, Vector2 linePoint, Vector2 lineDirection,
        Vector2 lineNormal)
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

    public static (CollisionPoint p, float t) IntersectRayRayInfo(Vector2 ray1Point, Vector2 ray1Direction, Vector2 ray2Point, Vector2 ray2Direction,
        Vector2 ray2Normal)
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

        var cp = IntersectRaySegment(rayPoint, rayDirection, a, b);
        if (cp.Valid) resultA = cp;

        cp = IntersectRaySegment(rayPoint, rayDirection, b, c);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }

        if (resultA.Valid && resultB.Valid) return (resultA, resultB);

        cp = IntersectRaySegment(rayPoint, rayDirection, c, a);
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

        var cp = IntersectRaySegment(rayPoint, rayDirection, a, b);
        if (cp.Valid) resultA = cp;

        cp = IntersectRaySegment(rayPoint, rayDirection, b, c);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }

        if (resultA.Valid && resultB.Valid) return (resultA, resultB);

        cp = IntersectRaySegment(rayPoint, rayDirection, c, d);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }

        if (resultA.Valid && resultB.Valid) return (resultA, resultB);

        cp = IntersectRaySegment(rayPoint, rayDirection, d, a);
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
                if (maxCollisionPoints > 0 && result.Count >= maxCollisionPoints) return result;
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
                if (maxCollisionPoints > 0 && result.Count >= maxCollisionPoints) return result;
            }
        }

        return result;
    }

    public static CollisionPoints? IntersectRaySegments(Vector2 rayPoint, Vector2 rayDirection, List<SegmentDef.Segment> segments, int maxCollisionPoints = -1)
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
                if (maxCollisionPoints > 0 && result.Count >= maxCollisionPoints) return result;
            }
        }

        return result;
    }

    public static int IntersectRayPolygon(Vector2 rayPoint, Vector2 rayDirection, List<Vector2> points, ref CollisionPoints result,
        bool returnAfterFirstValid = false)
    {
        if (points.Count < 3) return 0;
        var count = 0;
        for (var i = 0; i < points.Count; i++)
        {
            var cp = IntersectRaySegment(rayPoint, rayDirection, points[i], points[(i + 1) % points.Count]);
            if (cp.Valid)
            {
                result.Add(cp);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }

        return count;
    }

}