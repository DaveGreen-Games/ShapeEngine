using System.Numerics;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Shapes;

public readonly struct Line
{
    public readonly Vector2 Point;
    public readonly Vector2 Direction;
    public readonly Vector2 Normal;

    public Line()
    {
        Point = Vector2.Zero;
        Direction = Vector2.Zero;
        Normal = Vector2.Zero;
    }
    public Line(float x, float y, float dx, float dy, bool flippedNormal = false)
    {
        Point = new Vector2(x, y);
        Direction = new Vector2(dx, dy);
        Normal = flippedNormal ? new Vector2(Direction.Y, -Direction.X) : new Vector2(-Direction.Y, Direction.X);
    }
    public Line(Vector2 direction, bool flippedNormal = false)
    {
        Point = Vector2.Zero;
        Direction = direction;
        Normal = flippedNormal ? new Vector2(Direction.Y, -Direction.X) : new Vector2(-Direction.Y, Direction.X);
    }
    public Line(Vector2 point, Vector2 direction, bool flippedNormal = false)
    {
        Point = point;
        Direction = direction;
        Normal = flippedNormal ? new Vector2(Direction.Y, -Direction.X) : new Vector2(-Direction.Y, Direction.X);
    }
    internal Line(Vector2 point, Vector2 direction, Vector2 normal)
    {
        Point = point;
        Direction = direction;
        Normal = normal;
    }
    public bool IsValid => (Direction.X != 0 || Direction.Y!= 0) && (Normal.X != 0 || Normal.Y != 0);
    public Segment ToSegment(float length)
    {
        if (!IsValid) return new();
        return new Segment(Point - Direction * length, Point + Direction * length, Normal);
    }
    public Ray ToRay(bool reversed = false) => reversed ? new Ray(Point, -Direction, -Normal) : new Ray(Point, Direction, Normal);
    public Line FlipNormal() => new Line(Point, Direction, Normal.Flip());
    public static Vector2 GetNormal(Vector2 direction, bool flippedNormal)
    {
        if (flippedNormal) return direction.GetPerpendicularLeft().Normalize();
        return direction.GetPerpendicularRight().Normalize();
    }
    
    public static CollisionPoint IntersectLineSegment(Vector2 linePoint, Vector2 lineDirection, Vector2 segmentStart, Vector2 segmentEnd)
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
            return new();
        }

        // Calculate the intersection point using parameter t
        Vector2 difference = segmentStart - linePoint;
        float t = (difference.X * segmentDirection.Y - difference.Y * segmentDirection.X) / denominator;

        // Calculate the intersection point
        Vector2 intersection = linePoint + t * lineDirection;

        // Check if the intersection point is within the segment
        if (Segment.IsPointOnSegment(intersection, segmentStart, segmentEnd))
        {
            // The normal vector can be taken as perpendicular to the segment direction
            segmentDirection = Vector2.Normalize(segmentDirection);
            var normal = new Vector2(-segmentDirection.Y, segmentDirection.X);

            return new(intersection, normal);
        }

        return new();
    }
    public CollisionPoint IntersectSegment(Vector2 segmentStart, Vector2 segmentEnd) => IntersectLineSegment(Point, Direction, segmentStart, segmentEnd);
    public CollisionPoint IntersectSegment(Segment segment) => IntersectLineSegment(Point, Direction, segment.Start, segment.End);
    public CollisionPoints? IntersectShape(Segment segment)
    {
        var result = IntersectLineSegment(Point, Direction, segment.Start, segment.End);
        if (result.Valid)
        {
            var colPoints = new CollisionPoints();
            colPoints.Add(result);
            return colPoints;
        }

        return null;
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
        var normal = new Vector2(-line2Direction.Y, line2Direction.X);
        normal = Vector2.Normalize(normal);

        return new(intersection, normal);
    }
    public CollisionPoint IntersectLine(Vector2 otherPoint, Vector2 otherDirection) => IntersectLineLine(Point, Direction, otherPoint, otherDirection);
    public CollisionPoint IntersectLine(Line otherLine) => IntersectLineLine(Point, Direction, otherLine.Point, otherLine.Direction);
    public CollisionPoints? IntersectShape(Line line)
    {
        var result = IntersectLineLine(Point, Direction, line.Point, line.Direction);
        if (result.Valid)
        {
            var colPoints = new CollisionPoints();
            colPoints.Add(result);
            return colPoints;
        }

        return null;
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
            var normal = new Vector2(-rayDirection.Y, rayDirection.X);
            normal = Vector2.Normalize(normal);

            return new(intersection, normal);
        }
        
        return new();
    }
    public CollisionPoint IntersectRay(Vector2 otherPoint, Vector2 otherDirection) => IntersectLineRay(Point, Direction, otherPoint, otherDirection);
    public CollisionPoint IntersectRay(Ray otherRay) => IntersectLineRay(Point, Direction, otherRay.Point, otherRay.Direction);
    public CollisionPoints? IntersectShape(Ray ray)
    {
        var result = IntersectLineRay(Point, Direction, ray.Point, ray.Direction);
        if (result.Valid)
        {
            var colPoints = new CollisionPoints();
            colPoints.Add(result);
            return colPoints;
        }

        return null;
    }

    
    public static CollisionPoints? IntersectLineCircle(Vector2 linePoint, Vector2 lineDirection, Vector2 circleCenter, float circleRadius)
    {
        CollisionPoints? result = null;

        // Normalize the direction vector
        lineDirection = Vector2.Normalize(lineDirection);

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
            var normal1 = Vector2.Normalize(intersection1 - circleCenter);
            var normal2 = Vector2.Normalize(intersection2 - circleCenter);

            var p1 = new CollisionPoint(intersection1, normal1);
            var p2 = new CollisionPoint(intersection2, normal2);
            result ??= new();
            result.Add(p1);
            result.Add(p2);
        }
        else if (Math.Abs(distanceToCenter - circleRadius) < 1e-10)
        {
            result ??= new();
            var p = new CollisionPoint(closestPoint, Vector2.Normalize(closestPoint - circleCenter));
            result.Add(p);
        }

        return result;
    }
    public CollisionPoints? IntersectCircle(Circle otherCircle) => IntersectLineCircle(Point, Direction, otherCircle.Center, otherCircle.Radius);
    public CollisionPoints? IntersectCircle(Vector2 circleCenter, float circleRadius) => IntersectLineCircle(Point, Direction, circleCenter, circleRadius);
    public CollisionPoints? IntersectShape(Circle circle) => IntersectCircle(circle);
    
    
    //TODO: Implement overlaps functions!
    //TODO: Draw functions for line and ray that take a length as a parameter
    
    //TODO: implement for intersection
    // interect trectangle
    // intersect quad
    // intersect rect
    // intersect polygon
    // intersect polyline
    // intersect segments
    
    /*//Helper function to check if a point is on the segment
    private static bool IsPointOnSegment(Vector2 point, Vector2 segmentStart, Vector2 segmentEnd)
    {
        float minX = Math.Min(segmentStart.X, segmentEnd.X);
        float maxX = Math.Max(segmentStart.X, segmentEnd.X);
        float minY = Math.Min(segmentStart.Y, segmentEnd.Y);
        float maxY = Math.Max(segmentStart.Y, segmentEnd.Y);

        return point.X >= minX && point.X <= maxX && point.Y >= minY && point.Y <= maxY;
    }*/
}