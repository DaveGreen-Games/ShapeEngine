using System.Numerics;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.Shapes;

public readonly struct Ray
{
    public readonly Vector2 Point;
    public readonly Vector2 Direction;
    //TODO: Normal here

    public Ray()
    {
        Point = Vector2.Zero;
        Direction = Vector2.Zero;
    }
    public Ray(float x, float y, float dx, float dy)
    {
        Point = new Vector2(x, y);
        Direction = new Vector2(dx, dy);
    }
    public Ray(Vector2 direction)
    {
        Point = Vector2.Zero;
        Direction = direction;
    }
    public Ray(Vector2 point, Vector2 direction)
    {
        Point = point;
        Direction = direction;
    }

    public bool IsValid => Direction.X!= 0 || Direction.Y!= 0;
    public Segment ToSegment(float length)
    {
        if(!IsValid) return new();
        return new Segment(Point, Point + Direction * length);
    }
    public Line ToLine() => new Line(Point, Direction);
    
    
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
            var segmentDirection = Vector2.Normalize(segmentEnd - segmentStart);
            var normal = new Vector2(-segmentDirection.Y, segmentDirection.X);
            return new(intersection, normal);
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
            var normal = new Vector2(-lineDirection.Y, lineDirection.X);
            normal = Vector2.Normalize(normal);
            return new(intersection, normal);
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
            var normal = new Vector2(-ray2Direction.Y, ray2Direction.X);
            normal = Vector2.Normalize(normal);
            return new(intersection, normal);
        }

        return new();
    }
    public static CollisionPoints? IntersectRayCircle(Vector2 rayPoint, Vector2 rayDirection, Vector2 circleCenter, float radius)
    {
        CollisionPoints? result = null;

        var toCircle = circleCenter - rayPoint;
        float projectionLength = Vector2.Dot(toCircle, rayDirection);
        var closestPoint = rayPoint + projectionLength * rayDirection;
        float distanceToCenter = Vector2.Distance(closestPoint, circleCenter);

        if (distanceToCenter < radius)
        {
            var offset = (float)Math.Sqrt(radius * radius - distanceToCenter * distanceToCenter);
            var intersection1 = closestPoint - offset * rayDirection;
            var intersection2 = closestPoint + offset * rayDirection;

            if (Vector2.Dot(intersection1 - rayPoint, rayDirection) >= 0)
            {
                var normal1 = Vector2.Normalize(intersection1 - circleCenter);
                result ??= new();
                result.Add(new(intersection1, normal1));
            }

            if (Vector2.Dot(intersection2 - rayPoint, rayDirection) >= 0)
            {
                var normal2 = Vector2.Normalize(intersection2 - circleCenter);
                result ??= new();
                result.Add(new(intersection2, normal2));
                
            }
        }
        else if (Math.Abs(distanceToCenter - radius) < 1e-10)
        {
            if (Vector2.Dot(closestPoint - rayPoint, rayDirection) >= 0)
            {
                result ??= new();
                result.Add(new(closestPoint, Vector2.Normalize(closestPoint - circleCenter)));
            }
        }

        return result;
    }
    
    //TODO: Implement overlaps functions!
    //TODO: Draw functions for line and ray that take a length as a parameter
    
    //TODO: implement for intersection
    // interect trectangle
    // intersect quad
    // intersect rect
    // intersect polygon
    // intersect polyline
    // intersect segments
}