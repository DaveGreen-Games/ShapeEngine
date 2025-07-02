using System.Numerics;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.RayDef;

public readonly partial struct Ray
{
    /// <summary>
    /// Determines whether a point lies on a given ray.
    /// </summary>
    /// <param name="point">The point to check.</param>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <returns>True if the point is on the ray; otherwise, false.</returns>
    /// <remarks>
    /// The point must be in the same direction as the ray and also lie on the infinite line defined by the ray.
    /// </remarks>
    public static bool IsPointOnRay(Vector2 point, Vector2 rayPoint, Vector2 rayDirection)
    {
        // Calculate the vector from the ray point to the given point
        var toPoint = point - rayPoint;

        // Calculate the dot product of the direction vector and the vector to the point
        float dotProduct = Vector2.Dot(toPoint, rayDirection);

        // Check if the point is in the same direction as the ray and on the line
        return dotProduct >= 0 && Line.IsPointOnLine(point, rayPoint, rayDirection);
    }

    /// <summary>
    /// Computes the intersection information between a ray and a segment defined by two points.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <returns>A tuple containing the <see cref="IntersectionPoint"/> and the parameter t along the ray, or -1 if no intersection.</returns>
    /// <remarks>
    /// The returned parameter t is the distance along the ray direction to the intersection point.
    /// </remarks>
    public static (IntersectionPoint p, float t) IntersectRaySegmentInfo(Vector2 rayPoint, Vector2 rayDirection, Vector2 segmentStart, Vector2 segmentEnd)
    {
        float denominator = rayDirection.X * (segmentEnd.Y - segmentStart.Y) - rayDirection.Y * (segmentEnd.X - segmentStart.X);

        if (Math.Abs(denominator) < ShapeMath.EpsilonF)
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

    /// <summary>
    /// Computes the intersection information between a ray and a segment, using a provided segment normal.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <param name="segmentNormal">The normal vector of the segment.</param>
    /// <returns>A tuple containing the <see cref="IntersectionPoint"/> (with the provided normal) and the parameter t along the ray, or -1 if no intersection.</returns>
    public static (IntersectionPoint p, float t) IntersectRaySegmentInfo(Vector2 rayPoint, Vector2 rayDirection, Vector2 segmentStart, Vector2 segmentEnd,
        Vector2 segmentNormal)
    {
        var result = IntersectRaySegmentInfo(rayPoint, rayDirection, segmentStart, segmentEnd);
        if (result.p.Valid)
        {
            return (new(result.p.Point, segmentNormal), result.t);
        }

        return (new(), -1f);
    }

    /// <summary>
    /// Computes the intersection information between a ray and a line defined by a point and direction.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="linePoint">A point on the line.</param>
    /// <param name="lineDirection">The direction vector of the line.</param>
    /// <returns>A tuple containing the <see cref="IntersectionPoint"/> and the parameter t along the ray, or -1 if no intersection.</returns>
    /// <remarks>
    /// The returned parameter t is the distance along the ray direction to the intersection point.
    /// </remarks>
    public static (IntersectionPoint p, float t) IntersectRayLineInfo(Vector2 rayPoint, Vector2 rayDirection, Vector2 linePoint, Vector2 lineDirection)
    {
        float denominator = rayDirection.X * lineDirection.Y - rayDirection.Y * lineDirection.X;

        if (Math.Abs(denominator) < ShapeMath.EpsilonF)
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

    /// <summary>
    /// Computes the intersection information between a ray and a line, using a provided line normal.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="linePoint">A point on the line.</param>
    /// <param name="lineDirection">The direction vector of the line.</param>
    /// <param name="lineNormal">The normal vector of the line.</param>
    /// <returns>A tuple containing the <see cref="IntersectionPoint"/> (with the provided normal) and the parameter t along the ray, or -1 if no intersection.</returns>
    public static (IntersectionPoint p, float t) IntersectRayLineInfo(Vector2 rayPoint, Vector2 rayDirection, Vector2 linePoint, Vector2 lineDirection,
        Vector2 lineNormal)
    {
        var result = IntersectRayLineInfo(rayPoint, rayDirection, linePoint, lineDirection);
        if (result.p.Valid)
        {
            return (new(result.p.Point, lineNormal), result.t);
        }

        return (new(), -1f);
    }

    /// <summary>
    /// Computes the intersection information between two rays.
    /// </summary>
    /// <param name="ray1Point">The origin point of the first ray.</param>
    /// <param name="ray1Direction">The direction vector of the first ray.</param>
    /// <param name="ray2Point">The origin point of the second ray.</param>
    /// <param name="ray2Direction">The direction vector of the second ray.</param>
    /// <returns>A tuple containing the <see cref="IntersectionPoint"/> and the parameter t along the first ray, or -1 if no intersection.</returns>
    /// <remarks>
    /// The returned parameter t is the distance along the first ray direction to the intersection point.
    /// </remarks>
    public static (IntersectionPoint p, float t) IntersectRayRayInfo(Vector2 ray1Point, Vector2 ray1Direction, Vector2 ray2Point, Vector2 ray2Direction)
    {
        float denominator = ray1Direction.X * ray2Direction.Y - ray1Direction.Y * ray2Direction.X;

        if (Math.Abs(denominator) < ShapeMath.EpsilonF)
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

    /// <summary>
    /// Computes the intersection information between two rays, using a provided normal for the second ray.
    /// </summary>
    /// <param name="ray1Point">The origin point of the first ray.</param>
    /// <param name="ray1Direction">The direction vector of the first ray.</param>
    /// <param name="ray2Point">The origin point of the second ray.</param>
    /// <param name="ray2Direction">The direction vector of the second ray.</param>
    /// <param name="ray2Normal">The normal vector of the second ray.</param>
    /// <returns>A tuple containing the <see cref="IntersectionPoint"/> (with the provided normal) and the parameter t along the first ray, or -1 if no intersection.</returns>
    public static (IntersectionPoint p, float t) IntersectRayRayInfo(Vector2 ray1Point, Vector2 ray1Direction, Vector2 ray2Point, Vector2 ray2Direction,
        Vector2 ray2Normal)
    {
        var result = IntersectRayRayInfo(ray1Point, ray1Direction, ray2Point, ray2Direction);
        if (result.p.Valid)
        {
            return (new(result.p.Point, ray2Normal), result.t);
        }

        return (new(), -1f);
    }

    /// <summary>
    /// Computes the intersection point between a ray and a segment defined by two points.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <returns>The <see cref="IntersectionPoint"/> representing the intersection point, or an invalid <see cref="IntersectionPoint"/> if no intersection occurs.</returns>
    public static IntersectionPoint IntersectRaySegment(Vector2 rayPoint, Vector2 rayDirection, Vector2 segmentStart, Vector2 segmentEnd)
    {
        float denominator = rayDirection.X * (segmentEnd.Y - segmentStart.Y) - rayDirection.Y * (segmentEnd.X - segmentStart.X);

        if (Math.Abs(denominator) < ShapeMath.EpsilonF)
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

    /// <summary>
    /// Computes the intersection point between a ray and a segment, using a provided normal for the segment.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <param name="segmentNormal">The normal vector of the segment.</param>
    /// <returns>The <see cref="IntersectionPoint"/> representing the intersection point (with the provided normal), or an invalid <see cref="IntersectionPoint"/> if no intersection occurs.</returns>
    public static IntersectionPoint IntersectRaySegment(Vector2 rayPoint, Vector2 rayDirection, Vector2 segmentStart, Vector2 segmentEnd, Vector2 segmentNormal)
    {
        var result = IntersectRaySegment(rayPoint, rayDirection, segmentStart, segmentEnd);
        if (result.Valid)
        {
            return new(result.Point, segmentNormal);
        }

        return new();
    }

    /// <summary>
    /// Computes the intersection point between a ray and a line defined by a point and direction.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="linePoint">A point on the line.</param>
    /// <param name="lineDirection">The direction vector of the line.</param>
    /// <returns>The <see cref="IntersectionPoint"/> representing the intersection point, or an invalid <see cref="IntersectionPoint"/> if no intersection occurs.</returns>
    public static IntersectionPoint IntersectRayLine(Vector2 rayPoint, Vector2 rayDirection, Vector2 linePoint, Vector2 lineDirection)
    {
        float denominator = rayDirection.X * lineDirection.Y - rayDirection.Y * lineDirection.X;

        if (Math.Abs(denominator) < ShapeMath.EpsilonF)
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

    /// <summary>
    /// Computes the intersection point between a ray and a line, using a provided normal for the line.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="linePoint">A point on the line.</param>
    /// <param name="lineDirection">The direction vector of the line.</param>
    /// <param name="lineNormal">The normal vector of the line.</param>
    /// <returns>The <see cref="IntersectionPoint"/> representing the intersection point (with the provided normal), or an invalid <see cref="IntersectionPoint"/> if no intersection occurs.</returns>
    public static IntersectionPoint IntersectRayLine(Vector2 rayPoint, Vector2 rayDirection, Vector2 linePoint, Vector2 lineDirection, Vector2 lineNormal)
    {
        var result = IntersectRayLine(rayPoint, rayDirection, linePoint, lineDirection);
        if (result.Valid)
        {
            return new(result.Point, lineNormal);
        }

        return new();
    }

    /// <summary>
    /// Computes the intersection point between two rays.
    /// </summary>
    /// <param name="ray1Point">The origin point of the first ray.</param>
    /// <param name="ray1Direction">The direction vector of the first ray.</param>
    /// <param name="ray2Point">The origin point of the second ray.</param>
    /// <param name="ray2Direction">The direction vector of the second ray.</param>
    /// <returns>The <see cref="IntersectionPoint"/> representing the intersection point, or an invalid <see cref="IntersectionPoint"/> if no intersection occurs.</returns>
    public static IntersectionPoint IntersectRayRay(Vector2 ray1Point, Vector2 ray1Direction, Vector2 ray2Point, Vector2 ray2Direction)
    {
        float denominator = ray1Direction.X * ray2Direction.Y - ray1Direction.Y * ray2Direction.X;

        if (Math.Abs(denominator) < ShapeMath.EpsilonF)
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

    /// <summary>
    /// Computes the intersection point between two rays, using a provided normal for the second ray.
    /// </summary>
    /// <param name="ray1Point">The origin point of the first ray.</param>
    /// <param name="ray1Direction">The direction vector of the first ray.</param>
    /// <param name="ray2Point">The origin point of the second ray.</param>
    /// <param name="ray2Direction">The direction vector of the second ray.</param>
    /// <param name="ray2Normal">The normal vector of the second ray.</param>
    /// <returns>The <see cref="IntersectionPoint"/> representing the intersection point (with the provided normal), or an invalid <see cref="IntersectionPoint"/> if no intersection occurs.</returns>
    public static IntersectionPoint IntersectRayRay(Vector2 ray1Point, Vector2 ray1Direction, Vector2 ray2Point, Vector2 ray2Direction, Vector2 ray2Normal)
    {
        var result = IntersectRayRay(ray1Point, ray1Direction, ray2Point, ray2Direction);
        if (result.Valid)
        {
            return new(result.Point, ray2Normal);
        }

        return new();
    }

    /// <summary>
    /// Computes the intersection points between a ray and a circle.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="circleCenter">The center point of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <returns>A tuple containing two <see cref="IntersectionPoint"/>s representing the intersection points, or two invalid <see cref="IntersectionPoint"/>s if no intersection occurs.</returns>
    /// <remarks>
    /// If the ray is tangent to the circle, one valid <see cref="IntersectionPoint"/> and one invalid <see cref="IntersectionPoint"/> will be returned.
    /// </remarks>
    public static (IntersectionPoint a, IntersectionPoint b) IntersectRayCircle(Vector2 rayPoint, Vector2 rayDirection, Vector2 circleCenter, float radius)
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

            IntersectionPoint a = new();
            IntersectionPoint b = new();
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
        else if (Math.Abs(distanceToCenter - radius) < ShapeMath.EpsilonF)
        {
            if (Vector2.Dot(closestPoint - rayPoint, rayDirection) >= 0)
            {
                var cp = new IntersectionPoint(closestPoint, (closestPoint - circleCenter).Normalize());
                return (cp, new());
            }
        }

        return (new(), new());
    }

    /// <summary>
    /// Computes the intersection points between a ray and a triangle defined by three points.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <returns>A tuple containing two <see cref="IntersectionPoint"/>s representing the intersection points, or two invalid <see cref="IntersectionPoint"/>s if no intersection occurs.</returns>
    public static (IntersectionPoint a, IntersectionPoint b) IntersectRayTriangle(Vector2 rayPoint, Vector2 rayDirection, Vector2 a, Vector2 b, Vector2 c)
    {
        IntersectionPoint resultA = new();
        IntersectionPoint resultB = new();

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

    /// <summary>
    /// Computes the intersection points between a ray and a quadrilateral defined by four points.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="a">The first vertex of the quadrilateral.</param>
    /// <param name="b">The second vertex of the quadrilateral.</param>
    /// <param name="c">The third vertex of the quadrilateral.</param>
    /// <param name="d">The fourth vertex of the quadrilateral.</param>
    /// <returns>A tuple containing two <see cref="IntersectionPoint"/>s representing the intersection points, or two invalid <see cref="IntersectionPoint"/>s if no intersection occurs.</returns>
    public static (IntersectionPoint a, IntersectionPoint b) IntersectRayQuad(Vector2 rayPoint, Vector2 rayDirection, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        IntersectionPoint resultA = new();
        IntersectionPoint resultB = new();

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

    /// <summary>
    /// Computes the intersection points between a ray and a rectangle defined by four points.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="a">The first vertex of the rectangle.</param>
    /// <param name="b">The second vertex of the rectangle.</param>
    /// <param name="c">The third vertex of the rectangle.</param>
    /// <param name="d">The fourth vertex of the rectangle.</param>
    /// <returns>A tuple containing two <see cref="IntersectionPoint"/>s representing the intersection points, or two invalid <see cref="IntersectionPoint"/>s if no intersection occurs.</returns>
    public static (IntersectionPoint a, IntersectionPoint b) IntersectRayRect(Vector2 rayPoint, Vector2 rayDirection, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return IntersectRayQuad(rayPoint, rayDirection, a, b, c, d);
    }

    /// <summary>
    /// Computes the intersection points between a ray and a polygon defined by a list of points.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="points">The list of points defining the polygon.</param>
    /// <param name="maxCollisionPoints">The maximum number of collision points to return, or -1 for no limit.</param>
    /// <returns>A list of <see cref="IntersectionPoint"/>s representing the intersection points, or null if the polygon has fewer than 3 points.</returns>
    /// <remarks>
    /// If <paramref name="maxCollisionPoints"/> is greater than 0, the method will return at most that many collision points.
    /// </remarks>
    public static IntersectionPoints? IntersectRayPolygon(Vector2 rayPoint, Vector2 rayDirection, List<Vector2> points, int maxCollisionPoints = -1)
    {
        if (points.Count < 3) return null;
        if (maxCollisionPoints == 0) return null;
        IntersectionPoints? result = null;
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

    /// <summary>
    /// Computes the intersection points between a ray and a polyline defined by a list of points.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="points">The list of points defining the polyline.</param>
    /// <param name="maxCollisionPoints">The maximum number of collision points to return, or -1 for no limit.</param>
    /// <returns>A list of <see cref="IntersectionPoint"/>s representing the intersection points, or null if the polyline has fewer than 2 points.</returns>
    /// <remarks>
    /// If <paramref name="maxCollisionPoints"/> is greater than 0, the method will return at most that many collision points.
    /// </remarks>
    public static IntersectionPoints? IntersectRayPolyline(Vector2 rayPoint, Vector2 rayDirection, List<Vector2> points, int maxCollisionPoints = -1)
    {
        if (points.Count < 2) return null;
        if (maxCollisionPoints == 0) return null;
        IntersectionPoints? result = null;
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

    /// <summary>
    /// Computes the intersection points between a ray and a list of segments.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="segments">The list of segments to test for intersection.</param>
    /// <param name="maxCollisionPoints">The maximum number of collision points to return, or -1 for no limit.</param>
    /// <returns>A list of <see cref="IntersectionPoint"/>s representing the intersection points, or null if the list of segments is empty.</returns>
    /// <remarks>
    /// If <paramref name="maxCollisionPoints"/> is greater than 0, the method will return at most that many collision points.
    /// </remarks>
    public static IntersectionPoints? IntersectRaySegments(Vector2 rayPoint, Vector2 rayDirection, List<Segment> segments, int maxCollisionPoints = -1)
    {
        if (segments.Count <= 0) return null;
        if (maxCollisionPoints == 0) return null;
        IntersectionPoints? result = null;

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

    /// <summary>
    /// Computes the intersection points between a ray and a polygon defined by a list of points, and stores the results in a provided list.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="points">The list of points defining the polygon.</param>
    /// <param name="result">The list to store the resulting collision points.</param>
    /// <param name="returnAfterFirstValid">If true, the method will return after finding the first valid intersection.</param>
    /// <returns>The total number of intersection points found.</returns>
    /// <remarks>
    /// The method will add valid collision points to the <paramref name="result"/> list.
    /// </remarks>
    public static int IntersectRayPolygon(Vector2 rayPoint, Vector2 rayDirection, List<Vector2> points, ref IntersectionPoints result,
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