using System.Numerics;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.LineDef;

public readonly partial struct Line
{
    /// <summary>
    /// Determines whether a given point lies on a specified infinite line.
    /// </summary>
    /// <param name="point">The point to test for collinearity with the line.</param>
    /// <param name="linePoint">A point through which the line passes.</param>
    /// <param name="lineDirection">The direction vector of the line.</param>
    /// <returns>True if the point lies on the line; otherwise, false.</returns>
    /// <remarks>
    /// Uses the cross product to check if the point is collinear with the line.
    /// </remarks>
    public static bool IsPointOnLine(Vector2 point, Vector2 linePoint, Vector2 lineDirection)
    {
        // Calculate the vector from the line point to the given point
        var toPoint = point - linePoint;

        // Calculate the cross product of the direction vector and the vector to the point
        float crossProduct = toPoint.X * lineDirection.Y - toPoint.Y * lineDirection.X;

        // If the cross product is close to zero, the point is on the line
        return Math.Abs(crossProduct) < ShapeMath.EpsilonF;
    }

    /// <summary>
    /// Computes the intersection point between an infinite line and a finite segment, if it exists.
    /// </summary>
    /// <param name="linePoint">A point through which the infinite line passes.</param>
    /// <param name="lineDirection">The direction vector of the infinite line.</param>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <returns>
    /// A tuple containing the <see cref="IntersectionPoint"/> at the intersection and the parameter t along the line.
    /// If no intersection exists, returns an invalid <see cref="IntersectionPoint"/> and t = -1.
    /// </returns>
    /// <remarks>
    /// The intersection is only valid if it lies within the segment bounds.
    /// </remarks>
    public static (IntersectionPoint p, float t) IntersectLineSegmentInfo(Vector2 linePoint, Vector2 lineDirection, Vector2 segmentStart, Vector2 segmentEnd)
    {
        // Line AB (infinite line) represented by linePoint and lineDirection
        // Line segment CD represented by segmentStart and segmentEnd

        // Calculate direction vector of the segment
        var segmentDirection = segmentEnd - segmentStart;

        // Calculate the denominator of the intersection formula
        float denominator = lineDirection.X * segmentDirection.Y - lineDirection.Y * segmentDirection.X;

        // Check if lines are parallel (denominator is zero)
        if (Math.Abs(denominator) < ShapeMath.EpsilonF)
        {
            return (new(), -1f);
        }

        // Calculate the intersection point using parameter t
        var difference = segmentStart - linePoint;
        float t = (difference.X * segmentDirection.Y - difference.Y * segmentDirection.X) / denominator;

        // Calculate the intersection point
        var intersection = linePoint + t * lineDirection;

        // Check if the intersection point is within the segment
        if (Segment.IsPointOnSegment(intersection, segmentStart, segmentEnd))
        {
            // The normal vector can be taken as perpendicular to the segment direction
            segmentDirection = segmentDirection.Normalize();
            var normal = new Vector2(-segmentDirection.Y, segmentDirection.X);

            return (new(intersection, normal), t);
        }

        return (new(), -1f);
    }

    /// <summary>
    /// Computes the intersection point between an infinite line and a finite segment, using an explicit segment normal.
    /// </summary>
    /// <param name="linePoint">A point through which the infinite line passes.</param>
    /// <param name="lineDirection">The direction vector of the infinite line.</param>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <param name="segmentNormal">The normal vector of the segment, used for the resulting <see cref="IntersectionPoint"/>.</param>
    /// <returns>
    /// A tuple containing the <see cref="IntersectionPoint"/> at the intersection (with the provided normal) and the parameter t along the line.
    /// If no intersection exists, returns an invalid <see cref="IntersectionPoint"/> and t = -1.
    /// </returns>
    /// <remarks>
    /// Use this overload when the segment's normal is already known.
    /// </remarks>
    public static (IntersectionPoint p, float t) IntersectLineSegmentInfo(Vector2 linePoint, Vector2 lineDirection, Vector2 segmentStart, Vector2 segmentEnd,
        Vector2 segmentNormal)
    {
        var result = IntersectLineSegmentInfo(linePoint, lineDirection, segmentStart, segmentEnd);
        if (result.p.Valid)
        {
            return (new(result.p.Point, segmentNormal), result.t);
        }

        return (new(), -1f);
    }

    /// <summary>
    /// Computes the intersection point between two infinite lines, if it exists.
    /// </summary>
    /// <param name="line1Point">A point through which the first line passes.</param>
    /// <param name="line1Direction">The direction vector of the first line.</param>
    /// <param name="line2Point">A point through which the second line passes.</param>
    /// <param name="line2Direction">The direction vector of the second line.</param>
    /// <returns>
    /// A tuple containing the <see cref="IntersectionPoint"/> at the intersection and the parameter t along the first line.
    /// If the lines are parallel, returns an invalid <see cref="IntersectionPoint"/> and t = -1.
    /// </returns>
    /// <remarks>
    /// The intersection is valid only if the lines are not parallel.
    /// </remarks>
    public static (IntersectionPoint p, float t) IntersectLineLineInfo(Vector2 line1Point, Vector2 line1Direction, Vector2 line2Point, Vector2 line2Direction)
    {
        // Calculate the denominator of the intersection formula
        float denominator = line1Direction.X * line2Direction.Y - line1Direction.Y * line2Direction.X;

        // Check if lines are parallel (denominator is zero)
        if (Math.Abs(denominator) < ShapeMath.EpsilonF)
        {
            return (new(), -1f);
        }

        // Calculate the intersection point using parameter t
        var difference = line2Point - line1Point;
        float t = (difference.X * line2Direction.Y - difference.Y * line2Direction.X) / denominator;

        // Calculate the intersection point
        var intersection = line1Point + t * line1Direction;

        // Calculate the normal vector as perpendicular to the direction of the first line
        var normal = new Vector2(-line2Direction.Y, line2Direction.X).Normalize();

        return (new(intersection, normal), t);
    }

    /// <summary>
    /// Computes the intersection point between two infinite lines, using an explicit normal for the second line.
    /// </summary>
    /// <param name="line1Point">A point through which the first line passes.</param>
    /// <param name="line1Direction">The direction vector of the first line.</param>
    /// <param name="line2Point">A point through which the second line passes.</param>
    /// <param name="line2Direction">The direction vector of the second line.</param>
    /// <param name="line2Normal">The normal vector of the second line, used for the resulting <see cref="IntersectionPoint"/>.</param>
    /// <returns>
    /// A tuple containing the <see cref="IntersectionPoint"/> at the intersection (with the provided normal) and the parameter t along the first line.
    /// If the lines are parallel, returns an invalid <see cref="IntersectionPoint"/> and t = -1.
    /// </returns>
    /// <remarks>
    /// Use this overload when the second line's normal is already known.
    /// </remarks>
    public static (IntersectionPoint p, float t) IntersectLineLineInfo(Vector2 line1Point, Vector2 line1Direction, Vector2 line2Point, Vector2 line2Direction,
        Vector2 line2Normal)
    {
        var result = IntersectLineLineInfo(line1Point, line1Direction, line2Point, line2Direction);
        if (result.p.Valid)
        {
            return (new(result.p.Point, line2Normal), result.t);
        }

        return (new(), -1f);
    }

    /// <summary>
    /// Computes the intersection point between an infinite line and a ray, if it exists.
    /// </summary>
    /// <param name="linePoint">A point through which the infinite line passes.</param>
    /// <param name="lineDirection">The direction vector of the infinite line.</param>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <returns>
    /// A tuple containing the <see cref="IntersectionPoint"/> at the intersection and the parameter t along the line.
    /// If the intersection does not lie in the direction of the ray, returns an invalid <see cref="IntersectionPoint"/> and t = -1.
    /// </returns>
    /// <remarks>
    /// The intersection is valid only if it lies in the positive direction of the ray.
    /// </remarks>
    public static (IntersectionPoint p, float t) IntersectLineRayInfo(Vector2 linePoint, Vector2 lineDirection, Vector2 rayPoint, Vector2 rayDirection)
    {
        // Calculate the denominator of the intersection formula
        float denominator = lineDirection.X * rayDirection.Y - lineDirection.Y * rayDirection.X;

        // Check if lines are parallel (denominator is zero)
        if (Math.Abs(denominator) < ShapeMath.EpsilonF)
        {
            return (new(), -1f);
        }

        // Calculate the intersection point using parameter t
        var difference = rayPoint - linePoint;
        float t = (difference.X * rayDirection.Y - difference.Y * rayDirection.X) / denominator;

        // Calculate the parameter u for the ray
        float u = (difference.X * lineDirection.Y - difference.Y * lineDirection.X) / denominator;

        // Check if the intersection point lies in the direction of the ray
        if (u >= 0)
        {
            // Calculate the intersection point
            var intersection = linePoint + t * lineDirection;

            // Calculate the normal vector as perpendicular to the direction of the line
            var normal = new Vector2(-rayDirection.Y, rayDirection.X).Normalize();

            return (new(intersection, normal), t);
        }

        return (new(), -1f);
    }

    /// <summary>
    /// Computes the intersection point between an infinite line and a ray, using an explicit normal for the ray.
    /// </summary>
    /// <param name="linePoint">A point through which the infinite line passes.</param>
    /// <param name="lineDirection">The direction vector of the infinite line.</param>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="rayNormal">The normal vector of the ray, used for the resulting <see cref="IntersectionPoint"/>.</param>
    /// <returns>
    /// A tuple containing the <see cref="IntersectionPoint"/> at the intersection (with the provided normal) and the parameter t along the line.
    /// If the intersection does not lie in the direction of the ray, returns an invalid <see cref="IntersectionPoint"/> and t = -1.
    /// </returns>
    /// <remarks>
    /// Use this overload when the ray's normal is already known.
    /// </remarks>
    public static (IntersectionPoint p, float t) IntersectLineRayInfo(Vector2 linePoint, Vector2 lineDirection, Vector2 rayPoint, Vector2 rayDirection,
        Vector2 rayNormal)
    {
        var result = IntersectLineRayInfo(linePoint, lineDirection, rayPoint, rayDirection);
        if (result.p.Valid)
        {
            return (new(result.p.Point, rayNormal), result.t);
        }

        return (new(), -1f);
    }

    /// <summary>
    /// Computes the intersection point between an infinite line and a finite segment, if it exists.
    /// </summary>
    /// <param name="linePoint">A point through which the infinite line passes.</param>
    /// <param name="lineDirection">The direction vector of the infinite line.</param>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <returns>
    /// A <see cref="IntersectionPoint"/> at the intersection, or an invalid <see cref="IntersectionPoint"/> if no intersection exists.
    /// </returns>
    /// <remarks>
    /// The intersection is valid only if it lies within the segment bounds.
    /// </remarks>
    public static IntersectionPoint IntersectLineSegment(Vector2 linePoint, Vector2 lineDirection, Vector2 segmentStart, Vector2 segmentEnd)
    {
        var result = Ray.IntersectRaySegment(linePoint, lineDirection, segmentStart, segmentEnd);
        if (result.Valid) return result;
        return Ray.IntersectRaySegment(linePoint, -lineDirection, segmentEnd, segmentStart);

        //for some reason the code below works perfectly for every shape except for line vs rect
        //something about the segments of a rect makes this not work correctly
        //the above code works just fine when a line is split into two rays....


        // // Line AB (infinite line) represented by linePoint and lineDirection
        // // Line segment CD represented by segmentStart and segmentEnd
        //
        // // Calculate direction vector of the segment
        // var segmentDirection = segmentEnd - segmentStart;
        //
        // // Calculate the denominator of the intersection formula
        // float denominator = lineDirection.X * segmentDirection.Y - lineDirection.Y * segmentDirection.X;
        //
        // // Check if lines are parallel (denominator is zero)
        // if (Math.Abs(denominator) < ShapeMath.EpsilonF)
        // {
        //     return new();
        // }
        //
        // // Calculate the intersection point using parameter t
        // var difference = segmentStart - linePoint;
        // float t = (difference.X * segmentDirection.Y - difference.Y * segmentDirection.X) / denominator;
        //
        // // Calculate the intersection point
        // var intersection = linePoint + t * lineDirection;
        //
        // // Check if the intersection point is within the segment
        // if (Segment.IsPointOnSegment(intersection, segmentStart, segmentEnd))
        // {
        //     // The normal vector can be taken as perpendicular to the segment direction
        //     segmentDirection = segmentDirection.Normalize();
        //     var normal = new Vector2(-segmentDirection.Y, segmentDirection.X);
        //
        //     return new(intersection, normal);
        // }
        //
        // return new();
    }

    /// <summary>
    /// Computes the intersection point between an infinite line and a finite segment, using an explicit segment normal.
    /// </summary>
    /// <param name="linePoint">A point through which the infinite line passes.</param>
    /// <param name="lineDirection">The direction vector of the infinite line.</param>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <param name="segmentNormal">The normal vector of the segment, used for the resulting <see cref="IntersectionPoint"/>.</param>
    /// <returns>
    /// A <see cref="IntersectionPoint"/> at the intersection (with the provided normal), or an invalid <see cref="IntersectionPoint"/> if no intersection exists.
    /// </returns>
    /// <remarks>
    /// Use this overload when the segment's normal is already known.
    /// </remarks>
    public static IntersectionPoint IntersectLineSegment(Vector2 linePoint, Vector2 lineDirection, Vector2 segmentStart, Vector2 segmentEnd, Vector2 segmentNormal)
    {
        var result = IntersectLineSegment(linePoint, lineDirection, segmentStart, segmentEnd);
        if (result.Valid)
        {
            return new(result.Point, segmentNormal);
        }

        return new();
    }

    /// <summary>
    /// Computes the intersection point between two infinite lines, if it exists.
    /// </summary>
    /// <param name="line1Point">A point through which the first line passes.</param>
    /// <param name="line1Direction">The direction vector of the first line.</param>
    /// <param name="line2Point">A point through which the second line passes.</param>
    /// <param name="line2Direction">The direction vector of the second line.</param>
    /// <returns>
    /// A <see cref="IntersectionPoint"/> at the intersection, or an invalid <see cref="IntersectionPoint"/> if the lines are parallel.
    /// </returns>
    /// <remarks>
    /// The intersection is valid only if the lines are not parallel.
    /// </remarks>
    public static IntersectionPoint IntersectLineLine(Vector2 line1Point, Vector2 line1Direction, Vector2 line2Point, Vector2 line2Direction)
    {
        // Calculate the denominator of the intersection formula
        float denominator = line1Direction.X * line2Direction.Y - line1Direction.Y * line2Direction.X;

        // Check if lines are parallel (denominator is zero)
        if (Math.Abs(denominator) < ShapeMath.EpsilonF)
        {
            return new();
        }

        // Calculate the intersection point using parameter t
        var difference = line2Point - line1Point;
        float t = (difference.X * line2Direction.Y - difference.Y * line2Direction.X) / denominator;

        // Calculate the intersection point
        var intersection = line1Point + t * line1Direction;

        // Calculate the normal vector as perpendicular to the direction of the first line
        var normal = new Vector2(-line2Direction.Y, line2Direction.X).Normalize();

        return new(intersection, normal);
    }

    /// <summary>
    /// Computes the intersection point between two infinite lines, using an explicit normal for the second line.
    /// </summary>
    /// <param name="line1Point">A point through which the first line passes.</param>
    /// <param name="line1Direction">The direction vector of the first line.</param>
    /// <param name="line2Point">A point through which the second line passes.</param>
    /// <param name="line2Direction">The direction vector of the second line.</param>
    /// <param name="line2Normal">The normal vector of the second line, used for the resulting <see cref="IntersectionPoint"/>.</param>
    /// <returns>
    /// A <see cref="IntersectionPoint"/> at the intersection (with the provided normal), or an invalid <see cref="IntersectionPoint"/> if the lines are parallel.
    /// </returns>
    public static IntersectionPoint IntersectLineLine(Vector2 line1Point, Vector2 line1Direction, Vector2 line2Point, Vector2 line2Direction, Vector2 line2Normal)
    {
        var result = IntersectLineLine(line1Point, line1Direction, line2Point, line2Direction);
        if (result.Valid)
        {
            return new(result.Point, line2Normal);
        }

        return new();
    }

    /// <summary>
    /// Computes the intersection point between an infinite line and a ray, if it exists.
    /// </summary>
    /// <param name="linePoint">A point through which the infinite line passes.</param>
    /// <param name="lineDirection">The direction vector of the infinite line.</param>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <returns>
    /// A <see cref="IntersectionPoint"/> at the intersection, or an invalid <see cref="IntersectionPoint"/> if the intersection does not lie in the direction of the ray.
    /// </returns>
    /// <remarks>
    /// The intersection is valid only if it lies in the positive direction of the ray.
    /// </remarks>
    public static IntersectionPoint IntersectLineRay(Vector2 linePoint, Vector2 lineDirection, Vector2 rayPoint, Vector2 rayDirection)
    {
        // Calculate the denominator of the intersection formula
        float denominator = lineDirection.X * rayDirection.Y - lineDirection.Y * rayDirection.X;

        // Check if lines are parallel (denominator is zero)
        if (Math.Abs(denominator) < ShapeMath.EpsilonF)
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
            var normal = new Vector2(-rayDirection.Y, rayDirection.X).Normalize();

            return new(intersection, normal);
        }

        return new();
    }

    /// <summary>
    /// Computes the intersection point between an infinite line and a ray, using an explicit normal for the ray.
    /// </summary>
    /// <param name="linePoint">A point through which the infinite line passes.</param>
    /// <param name="lineDirection">The direction vector of the infinite line.</param>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="rayNormal">The normal vector of the ray, used for the resulting <see cref="IntersectionPoint"/>.</param>
    /// <returns>
    /// A <see cref="IntersectionPoint"/> at the intersection (with the provided normal), or an invalid <see cref="IntersectionPoint"/> if the intersection does not lie in the direction of the ray.
    /// </returns>
    /// <remarks>
    /// Use this overload when the ray's normal is already known.
    /// </remarks>
    public static IntersectionPoint IntersectLineRay(Vector2 linePoint, Vector2 lineDirection, Vector2 rayPoint, Vector2 rayDirection, Vector2 rayNormal)
    {
        var result = IntersectLineRay(linePoint, lineDirection, rayPoint, rayDirection);
        if (result.Valid)
        {
            return new(result.Point, rayNormal);
        }

        return new();
    }

    /// <summary>
    /// Computes the intersection points between an infinite line and a circle, if they exist.
    /// </summary>
    /// <param name="linePoint">A point through which the infinite line passes.</param>
    /// <param name="lineDirection">The direction vector of the infinite line.</param>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <returns>
    /// A tuple containing two <see cref="IntersectionPoint"/> objects representing the intersection points.
    /// If no intersection exists, both points are invalid.
    /// </returns>
    /// <remarks>
    /// The function calculates the closest approach of the line to the circle and determines if the line intersects the circle.
    /// </remarks>
    public static (IntersectionPoint a, IntersectionPoint b) IntersectLineCircle(Vector2 linePoint, Vector2 lineDirection, Vector2 circleCenter, float circleRadius)
    {
        // Normalize the direction vector
        lineDirection = lineDirection.Normalize();

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
            var normal1 = (intersection1 - circleCenter).Normalize();
            var normal2 = (intersection2 - circleCenter).Normalize();

            var p1 = new IntersectionPoint(intersection1, normal1);
            var p2 = new IntersectionPoint(intersection2, normal2);
            return (p1, p2);
        }

        if (Math.Abs(distanceToCenter - circleRadius) < ShapeMath.EpsilonF)
        {
            var p = new IntersectionPoint(closestPoint, (closestPoint - circleCenter).Normalize());
            return (p, new());
        }

        return (new(), new());
    }

    /// <summary>
    /// Computes the intersection points between an infinite line and a triangle, if they exist.
    /// </summary>
    /// <param name="linePoint">A point through which the infinite line passes.</param>
    /// <param name="lineDirection">The direction vector of the infinite line.</param>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <returns>
    /// A tuple containing up to two <see cref="IntersectionPoint"/> objects representing the intersection points.
    /// If no intersection exists, both points are invalid.
    /// </returns>
    /// <remarks>
    /// The function checks each edge of the triangle for intersections with the line.
    /// </remarks>
    public static (IntersectionPoint a, IntersectionPoint b) IntersectLineTriangle(Vector2 linePoint, Vector2 lineDirection, Vector2 a, Vector2 b, Vector2 c)
    {
        IntersectionPoint resultA = new();
        IntersectionPoint resultB = new();

        var cp = IntersectLineSegment(linePoint, lineDirection, a, b);
        if (cp.Valid) resultA = cp;

        cp = IntersectLineSegment(linePoint, lineDirection, b, c);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }

        if (resultA.Valid && resultB.Valid) return (resultA, resultB);

        cp = IntersectLineSegment(linePoint, lineDirection, c, a);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }

        return (resultA, resultB);
    }

    /// <summary>
    /// Computes the intersection points between an infinite line and a quadrilateral, if they exist.
    /// </summary>
    /// <param name="linePoint">A point through which the infinite line passes.</param>
    /// <param name="lineDirection">The direction vector of the infinite line.</param>
    /// <param name="a">The first vertex of the quadrilateral.</param>
    /// <param name="b">The second vertex of the quadrilateral.</param>
    /// <param name="c">The third vertex of the quadrilateral.</param>
    /// <param name="d">The fourth vertex of the quadrilateral.</param>
    /// <returns>
    /// A tuple containing up to two <see cref="IntersectionPoint"/> objects representing the intersection points.
    /// If no intersection exists, both points are invalid.
    /// </returns>
    /// <remarks>
    /// The function checks each edge of the quadrilateral for intersections with the line.
    /// </remarks>
    public static (IntersectionPoint a, IntersectionPoint b) IntersectLineQuad(Vector2 linePoint, Vector2 lineDirection, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        IntersectionPoint resultA = new();
        IntersectionPoint resultB = new();

        var cp = IntersectLineSegment(linePoint, lineDirection, a, b);
        if (cp.Valid)
        {
            resultA = cp;
        }

        cp = IntersectLineSegment(linePoint, lineDirection, b, c);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }

        if (resultA.Valid && resultB.Valid)
        {
            return (resultA, resultB);
        }

        cp = IntersectLineSegment(linePoint, lineDirection, c, d);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }

        if (resultA.Valid && resultB.Valid)
        {
            return (resultA, resultB);
        }

        cp = IntersectLineSegment(linePoint, lineDirection, d, a);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }

        return (resultA, resultB);
    }

    /// <summary>
    /// Computes the intersection points between an infinite line and a rectangle, if they exist.
    /// </summary>
    /// <param name="linePoint">A point through which the infinite line passes.</param>
    /// <param name="lineDirection">The direction vector of the infinite line.</param>
    /// <param name="a">The first vertex of the rectangle.</param>
    /// <param name="b">The second vertex of the rectangle.</param>
    /// <param name="c">The third vertex of the rectangle.</param>
    /// <param name="d">The fourth vertex of the rectangle.</param>
    /// <returns>
    /// A tuple containing up to two <see cref="IntersectionPoint"/> objects representing the intersection points.
    /// If no intersection exists, both points are invalid.
    /// </returns>
    /// <remarks>
    /// This function is a specialized version of <see cref="IntersectLineQuad"/> for rectangles.
    /// </remarks>
    public static (IntersectionPoint a, IntersectionPoint b) IntersectLineRect(Vector2 linePoint, Vector2 lineDirection, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return IntersectLineQuad(linePoint, lineDirection, a, b, c, d);
    }

    /// <summary>
    /// Computes the intersection points between an infinite line and a polygon, if they exist.
    /// </summary>
    /// <param name="linePoint">A point through which the infinite line passes.</param>
    /// <param name="lineDirection">The direction vector of the infinite line.</param>
    /// <param name="points">A list of vertices defining the polygon.</param>
    /// <param name="maxCollisionPoints">The maximum number of collision points to return. Use -1 for no limit.</param>
    /// <returns>
    /// A <see cref="IntersectionPoints"/> object containing the intersection points, or null if no intersections exist.
    /// </returns>
    /// <remarks>
    /// The function iterates through all edges of the polygon to find intersections with the line.
    /// </remarks>
    public static IntersectionPoints? IntersectLinePolygon(Vector2 linePoint, Vector2 lineDirection, List<Vector2> points, int maxCollisionPoints = -1)
    {
        if (points.Count < 3) return null;
        if (maxCollisionPoints == 0) return null;
        IntersectionPoints? result = null;
        for (var i = 0; i < points.Count; i++)
        {
            var colPoint = IntersectLineSegment(linePoint, lineDirection, points[i], points[(i + 1) % points.Count]);
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
    /// Computes the intersection points between an infinite line and a polyline, if they exist.
    /// </summary>
    /// <param name="linePoint">A point through which the infinite line passes.</param>
    /// <param name="lineDirection">The direction vector of the infinite line.</param>
    /// <param name="points">A list of vertices defining the polyline.</param>
    /// <param name="maxCollisionPoints">The maximum number of collision points to return. Use -1 for no limit.</param>
    /// <returns>
    /// A <see cref="IntersectionPoints"/> object containing the intersection points, or null if no intersections exist.
    /// </returns>
    /// <remarks>
    /// The function iterates through all segments of the polyline to find intersections with the line.
    /// </remarks>
    public static IntersectionPoints? IntersectLinePolyline(Vector2 linePoint, Vector2 lineDirection, List<Vector2> points, int maxCollisionPoints = -1)
    {
        if (points.Count < 2) return null;
        if (maxCollisionPoints == 0) return null;
        IntersectionPoints? result = null;
        for (var i = 0; i < points.Count - 1; i++)
        {
            var colPoint = IntersectLineSegment(linePoint, lineDirection, points[i], points[i + 1]);
            if (colPoint.Valid)
            {
                result ??= [];
                result.Add(colPoint);
                if (maxCollisionPoints > 0 && result.Count >= maxCollisionPoints) return result;
            }
        }

        return result;
    }

    /// <summary>
    /// Computes the intersection points between an infinite line and a collection of segments.
    /// </summary>
    /// <param name="linePoint">A point through which the infinite line passes.</param>
    /// <param name="lineDirection">The direction vector of the infinite line.</param>
    /// <param name="segments">A list of segments to check for intersections.</param>
    /// <param name="maxCollisionPoints">The maximum number of collision points to return. Use -1 for no limit.</param>
    /// <returns>
    /// A <see cref="IntersectionPoints"/> object containing the intersection points, or null if no intersections exist.
    /// </returns>
    /// <remarks>
    /// The function iterates through all segments in the collection to find intersections with the line.
    /// </remarks>
    public static IntersectionPoints? IntersectLineSegments(Vector2 linePoint, Vector2 lineDirection, List<Segment> segments, int maxCollisionPoints = -1)
    {
        if (segments.Count <= 0) return null;
        if (maxCollisionPoints == 0) return null;
        IntersectionPoints? result = null;

        foreach (var seg in segments)
        {
            var colPoint = IntersectLineSegment(linePoint, lineDirection, seg.Start, seg.End);
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
    /// Computes the intersection points between an infinite line and a polygon, if they exist.
    /// </summary>
    /// <param name="linePoint">A point through which the infinite line passes.</param>
    /// <param name="lineDirection">The direction vector of the infinite line.</param>
    /// <param name="points">A list of vertices defining the polygon. The polygon is assumed to be closed and non-self-intersecting.</param>
    /// <param name="result">A reference to a <see cref="IntersectionPoints"/> object that will be populated with intersection points, if any.</param>
    /// <param name="returnAfterFirstValid">
    /// If true, the function returns after finding the first valid intersection point. If false, all intersection points are found.
    /// </param>
    /// <returns>The number of intersection points found between the line and the polygon.</returns>
    /// <remarks>
    /// The function iterates through all edges of the polygon and checks for intersections with the line.
    /// </remarks>
    public static int IntersectLinePolygon(Vector2 linePoint, Vector2 lineDirection, List<Vector2> points, ref IntersectionPoints result,
        bool returnAfterFirstValid = false)
    {
        if (points.Count < 3) return 0;
        var count = 0;
        for (var i = 0; i < points.Count; i++)
        {
            var cp = IntersectLineSegment(linePoint, lineDirection, points[i], points[(i + 1) % points.Count]);
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