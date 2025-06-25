using System.Numerics;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Geometry.Segment;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.Ray;

public readonly partial struct Ray
{
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
        disSquared = ShapeMath.ClampToZero(disSquared);
        return closestPointOnRay;
    }

    public static (Vector2 self, Vector2 other) GetClosestPointRayLine(Vector2 rayPoint, Vector2 rayDirection, Vector2 linePoint, Vector2 lineDirection,
        out float disSquared)
    {
        var intersection = IntersectRayLine(rayPoint, rayDirection, linePoint, lineDirection);
        if (intersection.Valid)
        {
            disSquared = 0;
            return (intersection.Point, intersection.Point);
        }

        var cp = Line.Line.GetClosestPointLinePoint(linePoint, lineDirection, rayPoint, out disSquared);
        return (rayPoint, cp);
    }

    public static (Vector2 self, Vector2 other) GetClosestPointRayRay(Vector2 ray1Point, Vector2 ray1Direction, Vector2 ray2Point, Vector2 ray2Direction,
        out float disSquared)
    {
        var intersection = IntersectRayRay(ray1Point, ray1Direction, ray2Point, ray2Direction);
        if (intersection.Valid)
        {
            disSquared = 0;
            return (intersection.Point, intersection.Point);
        }

        var cp1 = GetClosestPointRayPoint(ray1Point, ray1Direction, ray2Point, out float disSquared1);
        var cp2 = GetClosestPointRayPoint(ray2Point, ray2Direction, ray1Point, out float disSquared2);

        if (disSquared1 < disSquared2)
        {
            disSquared = disSquared1;
            return (cp1, ray2Point);
        }

        disSquared = disSquared2;
        return (cp2, ray1Point);
    }

    public static (Vector2 self, Vector2 other) GetClosestPointRaySegment(Vector2 rayPoint, Vector2 rayDirection, Vector2 segmentStart, Vector2 segmentEnd,
        out float disSquared)
    {
        var result = Segment.Segment.GetClosestPointSegmentRay(segmentStart, segmentEnd, rayPoint, rayDirection, out disSquared);
        return (result.other, result.self);

        // var d1 = rayDirection.Normalize();
        // var d2 = segmentEnd - segmentStart;
        //
        // float a = Vector2.Dot(d1, d1);
        // float b = Vector2.Dot(d1, d2);
        // float e = Vector2.Dot(d2, d2);
        // var r = rayPoint - segmentStart;
        // float c = Vector2.Dot(d1, r);
        // float f = Vector2.Dot(d2, r);
        //
        // float denominator = a * e - b * b;
        // float t1 = Math.Max(0, (b * f - c * e) / denominator);
        // float t2 = Math.Max(0, Math.Min(1, (a * f - b * c) / denominator));
        //
        // var closestPoint1 = rayPoint + t1 * d1;
        // var closestPoint2 = segmentStart + t2 * d2;
        // disSquared = (closestPoint1 - closestPoint2).LengthSquared();
        // return (closestPoint1, closestPoint2);
    }

    public static (Vector2 self, Vector2 other) GetClosestPointRayCircle(Vector2 rayPoint, Vector2 rayDirection, Vector2 circleCenter, float circleRadius,
        out float disSquared)
    {
        var d1 = rayDirection.Normalize();

        var toCenter = circleCenter - rayPoint;
        float projectionLength = Math.Max(0, Vector2.Dot(toCenter, d1));
        var closestPointOnRay = rayPoint + projectionLength * d1;

        var offset = (closestPointOnRay - circleCenter).Normalize() * circleRadius;
        var closestPointOnCircle = circleCenter + offset;
        disSquared = (closestPointOnRay - closestPointOnCircle).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
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
        disSquared = ShapeMath.ClampToZero(disSquared);
        var dir = (point - closestPointOnRay).Normalize();
        var dot = Vector2.Dot(dir, Normal);
        if (dot >= 0) return new(closestPointOnRay, Normal);
        return new(closestPointOnRay, -Normal);
    }

    public ClosestPointResult GetClosestPoint(Line.Line other)
    {
        // var result = other.GetClosestPoint(this);
        // return result.Switch();
        var result = GetClosestPointRayLine(Point, Direction, other.Point, other.Direction, out float disSquared);
        return new(
            new(result.self, Normal),
            new(result.other, other.Normal),
            disSquared
        );
    }

    public ClosestPointResult GetClosestPoint(Ray other)
    {
        var result = GetClosestPointRayRay(Point, Direction, other.Point, other.Direction, out var disSquared);
        return new
        (
            new(result.self, Normal),
            new(result.other, other.Normal),
            disSquared
        );


        // var d1 = Direction;
        // var d2 = other.Direction;
        //
        // float a = Vector2.Dot(d1, d1);
        // float b = Vector2.Dot(d1, d2);
        // float e = Vector2.Dot(d2, d2);
        // var r = Point - other.Point;
        // float c = Vector2.Dot(d1, r);
        // float f = Vector2.Dot(d2, r);
        //
        // float denominator = a * e - b * b;
        // float t1 = Math.Max(0, (b * f - c * e) / denominator);
        // float t2 = Math.Max(0, (a * f - b * c) / denominator);
        //
        // var closestPoint1 = Point + t1 * d1;
        // var closestPoint2 = other.Point + t2 * d2;
        //
        // float disSquared = (closestPoint1 - closestPoint2).LengthSquared();
        // return new(
        //     new(closestPoint1, Normal), 
        //     new(closestPoint2, other.Normal),
        //     disSquared
        // );
    }

    public ClosestPointResult GetClosestPoint(Segment.Segment other)
    {
        var result = Segment.Segment.GetClosestPointSegmentRay(other.Start, other.End, Point, Direction, out var disSquared);
        return new(
            new(result.other, Normal),
            new(result.self, other.Normal),
            disSquared);
        // var d1 = Direction;
        // var d2 = other.Displacement;
        //
        // float a = Vector2.Dot(d1, d1);
        // float b = Vector2.Dot(d1, d2);
        // float e = Vector2.Dot(d2, d2);
        // var r = Point - other.Start;
        // float c = Vector2.Dot(d1, r);
        // float f = Vector2.Dot(d2, r);
        //
        // float denominator = a * e - b * b;
        // float t1 = Math.Max(0, (b * f - c * e) / denominator);
        // float t2 = Math.Max(0, Math.Min(1, (a * f - b * c) / denominator));
        //
        // var closestPoint1 = Point + t1 * d1;
        // var closestPoint2 = other.Start + t2 * d2;
        //
        // float disSquared = (closestPoint1 - closestPoint2).LengthSquared();
        // return new(
        //     new(closestPoint1, Normal), 
        //     new(closestPoint2, other.Normal),
        //     disSquared
        //     );
    }

    public ClosestPointResult GetClosestPoint(Circle.Circle other)
    {
        var d1 = Direction;

        var toCenter = other.Center - Point;
        float projectionLength = Math.Max(0, Vector2.Dot(toCenter, d1));
        var closestPointOnRay = Point + projectionLength * d1;

        var offset = (closestPointOnRay - other.Center).Normalize() * other.Radius;
        var closestPointOnCircle = other.Center + offset;

        float disSquared = (closestPointOnRay - closestPointOnCircle).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
        return new(
            new(closestPointOnRay, Normal),
            new(closestPointOnCircle, (closestPointOnCircle - other.Center).Normalize()),
            disSquared
        );
    }

    public ClosestPointResult GetClosestPoint(Triangle.Triangle other)
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
                new(result.other, normal),
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

    public ClosestPointResult GetClosestPoint(Quad.Quad other)
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
                new(result.other, normal),
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

    public ClosestPointResult GetClosestPoint(Rect.Rect other)
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
                new(result.other, normal),
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

    public ClosestPointResult GetClosestPoint(Polygon.Polygon other)
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

    public ClosestPointResult GetClosestPoint(Polyline.Polyline other)
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
}