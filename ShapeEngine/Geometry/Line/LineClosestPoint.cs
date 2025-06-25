
using System.Numerics;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.Line;

/// <summary>
/// Provides static methods for calculating the closest points between an infinite line and various geometric shapes in 2D space.
/// </summary>
public static class LineClosestPoint
{
    /// <summary>
    /// Calculates the closest point on an infinite line to a given point in 2D space.
    /// </summary>
    /// <param name="linePoint">A point through which the line passes.</param>
    /// <param name="lineDirection">The direction vector of the line (does not need to be normalized).</param>
    /// <param name="point">The point from which the closest point on the line is sought.</param>
    /// <param name="disSquared">Outputs the squared distance between the closest point on the line and the given point.</param>
    /// <returns>The closest point on the line to the specified point.</returns>
    /// <remarks>
    /// The direction vector is normalized internally. The squared distance is clamped to zero to avoid negative values due to floating point errors.
    /// </remarks>
    public static Vector2 GetClosestPointLinePoint(Vector2 linePoint, Vector2 lineDirection, Vector2 point, out float disSquared)
    {
        // Normalize the direction vector of the line
        var normalizedLineDirection = lineDirection.Normalize();

        // Calculate the vector from the line's point to the given point
        var toPoint = point - linePoint;

        // Project the vector to the point onto the line direction
        float projectionLength = Vector2.Dot(toPoint, normalizedLineDirection);

        // Calculate the closest point on the line
        var closestPointOnLine = linePoint + projectionLength * normalizedLineDirection;
        disSquared = (closestPointOnLine - point).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
        return closestPointOnLine;
    }

    /// <summary>
    /// Calculates the closest points between two infinite lines in 2D space.
    /// </summary>
    /// <param name="line1Point">A point on the first line.</param>
    /// <param name="line1Direction">The direction vector of the first line.</param>
    /// <param name="line2Point">A point on the second line.</param>
    /// <param name="line2Direction">The direction vector of the second line.</param>
    /// <param name="disSquared">Outputs the squared distance between the closest points on the two lines. Returns 0 if the lines intersect, -1 if they are parallel.</param>
    /// <returns>A tuple containing the closest points on each line, respectively.</returns>
    /// <remarks>
    /// If the lines intersect, the returned points are identical and <paramref name="disSquared"/> is 0. If the lines are parallel, the original points are returned and <paramref name="disSquared"/> is -1.
    /// </remarks>
    public static (Vector2 self, Vector2 other) GetClosestPointLineLine(Vector2 line1Point, Vector2 line1Direction, Vector2 line2Point, Vector2 line2Direction, out float disSquared)
    {
        var result = LineIntersection.IntersectLineLine(line1Point, line1Direction, line2Point, line2Direction);
        if (result.Valid)
        {
            disSquared = 0f;
            return (result.Point, result.Point);
        }

        disSquared = -1f;
        return (line1Point, line2Point);
    }

    /// <summary>
    /// Calculates the closest points between an infinite line and a ray in 2D space.
    /// </summary>
    /// <param name="linePoint">A point on the line.</param>
    /// <param name="lineDirection">The direction vector of the line.</param>
    /// <param name="rayPoint">The starting point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="disSquared">Outputs the squared distance between the closest points on the line and the ray.</param>
    /// <returns>A tuple containing the closest point on the line and the closest point on the ray, respectively.</returns>
    /// <remarks>
    /// If the line and ray intersect, the returned points are identical and <paramref name="disSquared"/> is 0. Otherwise, the closest point on the line to the ray's origin is returned.
    /// </remarks>
    public static (Vector2 self, Vector2 other) GetClosestPointLineRay(Vector2 linePoint, Vector2 lineDirection, Vector2 rayPoint, Vector2 rayDirection, out float disSquared)
    {
        var intersection = LineIntersection.IntersectLineRay(linePoint, lineDirection, rayPoint, rayDirection);
        if (intersection.Valid)
        {
            disSquared = 0;
            return (intersection.Point, intersection.Point);
        }
    
        var cp = GetClosestPointLinePoint(linePoint, lineDirection, rayPoint, out disSquared);
        return (cp, rayPoint);
    }

    /// <summary>
    /// Calculates the closest points between an infinite line and a finite segment in 2D space.
    /// </summary>
    /// <param name="linePoint">A point on the line.</param>
    /// <param name="lineDirection">The direction vector of the line.</param>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <param name="disSquared">Outputs the squared distance between the closest points on the line and the segment.</param>
    /// <returns>A tuple containing the closest point on the line and the closest point on the segment, respectively.</returns>
    /// <remarks>
    /// Uses the segment's own closest point calculation for accuracy. The squared distance is clamped to zero to avoid negative values due to floating point errors.
    /// </remarks>
    public static (Vector2 self, Vector2 other) GetClosestPointLineSegment(Vector2 linePoint, Vector2 lineDirection, Vector2 segmentStart, Vector2 segmentEnd, out float disSquared)
    {
        var result = Segment.Segment.GetClosestPointSegmentLine(segmentStart, segmentEnd, linePoint, lineDirection, out disSquared);
        return (result.other, result.self);
    }

    /// <summary>
    /// Calculates the closest points between an infinite line and a circle in 2D space.
    /// </summary>
    /// <param name="linePoint">A point on the line.</param>
    /// <param name="lineDirection">The direction vector of the line.</param>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="disSquared">Outputs the squared distance between the closest points on the line and the circle.</param>
    /// <returns>A tuple containing the closest point on the line and the closest point on the circle, respectively.</returns>
    /// <remarks>
    /// The closest point on the circle is found by projecting the closest point on the line onto the circle's perimeter. The squared distance is clamped to zero to avoid negative values due to floating point errors.
    /// </remarks>
    public static (Vector2 self, Vector2 other) GetClosestPointLineCircle(Vector2 linePoint, Vector2 lineDirection, Vector2 circleCenter, float circleRadius, out float disSquared)
    {
        var result = GetClosestPointLinePoint(linePoint, lineDirection, circleCenter, out disSquared);
        var other = circleCenter + (result - circleCenter).Normalize() * circleRadius;
        disSquared = (result - other).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
        return (result, other);

    }
    
    
    
    /// <summary>
    /// Calculates the closest point on this line to a given point in 2D space.
    /// </summary>
    /// <param name="self">The line to use.</param>
    /// <param name="point">The point from which the closest point on the line is sought.</param>
    /// <param name="disSquared">Outputs the squared distance between the closest point on the line and the given point.</param>
    /// <returns>A <see cref="CollisionPoint"/> representing the closest point on the line and its normal.</returns>
    /// <remarks>
    /// The normal is oriented to face the point if it is on the same side as the line's normal, otherwise it is flipped.
    /// </remarks>
    public static CollisionPoint GetClosestPoint(this Line self, Vector2 point, out float disSquared)
    {
        // Normalize the direction vector of the line
        var normalizedLineDirection = self.Direction.Normalize();

        // Calculate the vector from the line's point to the given point
        var toPoint = point - self.Point;

        // Project the vector to the point onto the line direction
        float projectionLength = Vector2.Dot(toPoint, normalizedLineDirection);

        // Calculate the closest point on the line
        var closestPointOnLine = self.Point + projectionLength * normalizedLineDirection;
        disSquared = (closestPointOnLine - point).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
        var dir = (point - closestPointOnLine).Normalize();
        var dot = Vector2.Dot(dir, self.Normal);
        if (dot >= 0) return new(closestPointOnLine, self.Normal);
        return new(closestPointOnLine, -self.Normal);
    }
    /// <summary>
    /// Calculates the closest points between this line and another line.
    /// </summary>
    /// <param name="self">The line to use.</param>
    /// <param name="other">The other <see cref="Line"/> to which the closest point is sought.</param>
    /// <returns>
    /// A <see cref="ClosestPointResult"/> containing the closest points on both lines, their normals, and the squared distance between them.
    /// If the lines intersect, the points are identical and the distance is zero. If the lines are parallel, an empty result is returned.
    /// </returns>
    public static ClosestPointResult GetClosestPoint(this Line self, Line other)
    {
        var result = LineIntersection.IntersectLineLine(self.Point, self.Direction, other.Point, other.Direction);
        if (result.Valid)
        {
            return new
                (
                    new(result.Point, self.Normal),
                    new(result.Point, other.Normal),
                    0f
                    );
        }
        return new();
    }
    /// <summary>
    /// Calculates the closest points between this line and a ray.
    /// </summary>
    /// <param name="self">The line to use.</param>
    /// <param name="other">The <see cref="Ray"/> to which the closest point is sought.</param>
    /// <returns>
    /// A <see cref="ClosestPointResult"/> containing the closest points on the line and the ray, their normals, and the squared distance between them.
    /// </returns>
    public static ClosestPointResult GetClosestPoint(this Line self, Ray.Ray other)
    {
        var result = GetClosestPointLineRay(self.Point, self.Direction, other.Point, other.Direction, out float disSquared);
        return new(
            new(result.self, self.Normal),
            new(result.other, other.Normal),
            disSquared);
    }
    /// <summary>
    /// Calculates the closest points between this line and a segment.
    /// </summary>
    /// <param name="self">The line to use.</param>
    /// <param name="other">The <see cref="Segment"/> to which the closest point is sought.</param>
    /// <returns>
    /// A <see cref="ClosestPointResult"/> containing the closest points on the line and the segment, their normals, and the squared distance between them.
    /// </returns>
    public static ClosestPointResult GetClosestPoint(this Line self, Segment.Segment other)
    {
        var result = Segment.Segment.GetClosestPointSegmentLine(other.Start, other.End, self.Point, self.Direction, out var disSquared);
        return new(
            new(result.other, self.Normal),
            new(result.self, other.Normal),
            disSquared);
    }
    /// <summary>
    /// Calculates the closest points between this line and a circle.
    /// </summary>
    /// <param name="self">The line to use.</param>
    /// <param name="other">The <see cref="Circle"/> to which the closest point is sought.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points on the line and the circle, their normals, and the squared distance between them.</returns>
    /// <remarks>
    /// The closest point on the circle is found by projecting the closest point on the line onto the circle's perimeter.
    /// </remarks>
    public static ClosestPointResult GetClosestPoint(this Line self, Circle.Circle other)
    {
        var d1 = self.Direction;
        
        var toCenter = other.Center - self.Point;
        float projectionLength = Vector2.Dot(toCenter, d1);
        var closestPointOnLine = self.Point + projectionLength * d1;
        
        var offset = (closestPointOnLine - other.Center).Normalize() * other.Radius;
        var closestPointOnCircle = other.Center + offset;
        
        float disSquared = (closestPointOnCircle - closestPointOnLine).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
        return new(
            new(closestPointOnLine, self.Normal),
            new(closestPointOnCircle, (closestPointOnCircle - other.Center).Normalize()),
            disSquared);
    }
    /// <summary>
    /// Calculates the closest points between this line and a triangle.
    /// </summary>
    /// <param name="self">The line to use.</param>
    /// <param name="other">The <see cref="Triangle"/> to which the closest point is sought.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points on the line and the triangle, their normals, the squared distance, and the index of the closest triangle edge.</returns>
    /// <remarks>
    /// The closest point is determined by evaluating all triangle edges.
    /// </remarks>
    public static ClosestPointResult GetClosestPoint(this Line self, Triangle.Triangle other)
    {
        var closestResult = GetClosestPointLineSegment(self.Point, self.Direction, other.A, other.B, out float minDisSquared);
        
        var otherNormal = (other.B - other.A);
        var otherIndex = 0;
        var result = GetClosestPointLineSegment(self.Point, self.Direction, other.B, other.C, out float disSquared);
        
        if (disSquared < minDisSquared)
        {
            otherIndex = 1;
            minDisSquared = disSquared;
            closestResult = result;
            otherNormal = (other.C - other.B);
        }
        
        result = GetClosestPointLineSegment(self.Point, self.Direction, other.C, other.A, out disSquared);
        
        if (disSquared < minDisSquared)
        {
            var normal = (other.A - other.C).GetPerpendicularRight().Normalize();
            return new(
                new(result.self, self.Normal),
                new(result.other, normal),
                disSquared,
                -1,
                2);
        }
        
        return new(
            new(closestResult.self, self.Normal),
            new(closestResult.other, otherNormal.GetPerpendicularRight().Normalize()),
            minDisSquared,
            -1,
            otherIndex);
    }
    /// <summary>
    /// Calculates the closest points between this line and a quadrilateral.
    /// </summary>
    /// <param name="self">The line to use.</param>
    /// <param name="other">The <see cref="Quad"/> to which the closest point is sought.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points on the line and the quad, their normals, the squared distance, and the index of the closest quad edge.</returns>
    /// <remarks>
    /// The closest point is determined by evaluating all quad edges.
    /// </remarks>
    public static ClosestPointResult GetClosestPoint(this Line self, Quad.Quad other)
    {
        var closestResult = GetClosestPointLineSegment(self.Point, self.Direction, other.A, other.B, out float minDisSquared);
        var otherNormal = (other.B - other.A);
        var otherIndex = 0;
        
        var result = GetClosestPointLineSegment(self.Point, self.Direction, other.B, other.C, out float disSquared);
        if (disSquared < minDisSquared)
        {
            otherIndex = 1;
            minDisSquared = disSquared;
            closestResult = result;
            otherNormal = (other.C - other.B);
        }
        
        result = GetClosestPointLineSegment(self.Point, self.Direction, other.C, other.D, out disSquared);
        if (disSquared < minDisSquared)
        {
            otherIndex = 2;
            minDisSquared = disSquared;
            closestResult = result;
            otherNormal = (other.D - other.C);
        }
        
        result = GetClosestPointLineSegment(self.Point, self.Direction, other.D, other.A, out disSquared);
        if (disSquared < minDisSquared)
        {
            var normal = (other.A - other.D).GetPerpendicularRight().Normalize();
            return new(
                new(result.self, self.Normal),
                new(result.other, normal),
                disSquared,
                -1,
                3);
        }

        return new(
            new(closestResult.self, self.Normal),
            new(closestResult.other, otherNormal.GetPerpendicularRight().Normalize()),
            minDisSquared,
            -1,
            otherIndex);
    }
    /// <summary>
    /// Calculates the closest points between this line and a rectangle.
    /// </summary>
    /// <param name="self">The line to use.</param>
    /// <param name="other">The <see cref="Rect"/> to which the closest point is sought.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points on the line and the rectangle, their normals, the squared distance, and the index of the closest rectangle edge.</returns>
    /// <remarks>
    /// The closest point is determined by evaluating all rectangle edges.
    /// </remarks>
    public static ClosestPointResult GetClosestPoint(this Line self, Rect.Rect other)
    {
        var closestResult = GetClosestPointLineSegment(self.Point, self.Direction, other.A, other.B, out float minDisSquared);
        var otherNormal = (other.B - other.A);
        var otherIndex = 0;
        
        var result = GetClosestPointLineSegment(self.Point, self.Direction, other.B, other.C, out float disSquared);
        if (disSquared < minDisSquared)
        {
            otherIndex = 1;
            minDisSquared = disSquared;
            closestResult = result;
            otherNormal = (other.C - other.B);
        }
        
        result = GetClosestPointLineSegment(self.Point, self.Direction, other.C, other.D, out disSquared);
        if (disSquared < minDisSquared)
        {
            otherIndex = 2;
            minDisSquared = disSquared;
            closestResult = result;
            otherNormal = (other.D - other.C);
        }
        
        result = GetClosestPointLineSegment(self.Point, self.Direction, other.D, other.A, out disSquared);
        if (disSquared < minDisSquared)
        {
            var normal = (other.A - other.D).GetPerpendicularRight().Normalize();
            return new(
                new(result.self, self.Normal),
                new(result.other, normal),
                disSquared,
                -1,
                3);
        }

        return new(
            new(closestResult.self, self.Normal),
            new(closestResult.other, otherNormal.GetPerpendicularRight().Normalize()),
            minDisSquared,
            -1,
            otherIndex);
    }
    /// <summary>
    /// Calculates the closest points between this line and a polygon.
    /// </summary>
    /// <param name="self">The line to use.</param>
    /// <param name="other">The <see cref="Polygon"/> to which the closest point is sought.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points on the line and the polygon, their normals, the squared distance, and the index of the closest polygon edge.</returns>
    /// <remarks>
    /// The closest point is determined by evaluating all polygon edges.
    /// </remarks>
    public static ClosestPointResult GetClosestPoint(this Line self, Polygon.Polygon other)
    {
        if (other.Count < 3) return new();
        
        var p1 = other[0];
        var p2 = other[1];
        var closestResult = GetClosestPointLineSegment(self.Point, self.Direction, p1, p2, out float minDisSquared);
        var otherNormal = (p2 - p1);
        var otherIndex = 0;
        for (var i = 1; i < other.Count; i++)
        {
            p1 = other[i];
            p2 = other[(i + 1) % other.Count];
            var result = GetClosestPointLineSegment(self.Point, self.Direction, p1, p2, out float disSquared);
            if (disSquared < minDisSquared)
            {
                otherIndex = i;
                minDisSquared = disSquared;
                closestResult = result;
                otherNormal = (p2 - p1);
            }
        }
        return new(
            new(closestResult.self, self.Normal),
            new(closestResult.other, otherNormal.GetPerpendicularRight().Normalize()),
            minDisSquared,
            -1,
            otherIndex);
    }
    /// <summary>
    /// Calculates the closest points between this line and a polyline.
    /// </summary>
    /// <param name="self">The line to use.</param>
    /// <param name="other">The <see cref="Polyline"/> to which the closest point is sought.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points on the line and the polyline, their normals, the squared distance, and the index of the closest polyline segment.</returns>
    /// <remarks>
    /// The closest point is determined by evaluating all polyline segments.
    /// </remarks>
    public static ClosestPointResult GetClosestPoint(this Line self, Polyline.Polyline other)
    {
        if (other.Count < 2) return new();
        
        var p1 = other[0];
        var p2 = other[1];
        var closestResult = GetClosestPointLineSegment(self.Point, self.Direction, p1, p2, out float minDisSquared);
        var otherNormal = (p2 - p1);
        var otherIndex = 0;
        for (var i = 1; i < other.Count - 1; i++)
        {
            p1 = other[i];
            p2 = other[i + 1];
            var result = GetClosestPointLineSegment(self.Point, self.Direction, p1, p2, out float disSquared);
            if (disSquared < minDisSquared)
            {
                otherIndex = i;
                minDisSquared = disSquared;
                closestResult = result;
                otherNormal = (p2 - p1);
            }
        }
        return new(
            new(closestResult.self, self.Normal),
            new(closestResult.other, otherNormal.GetPerpendicularRight().Normalize()),
            minDisSquared,
            -1,
            otherIndex);
    }
    /// <summary>
    /// Calculates the closest points between this line and a collection of segments.
    /// </summary>
    /// <param name="self">The line to use.</param>
    /// <param name="segments">The <see cref="Segment.Segments"/> collection to which the closest point is sought.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points on the line and the closest segment, their normals, the squared distance, and the index of the closest segment.</returns>
    /// <remarks>
    /// The closest point is determined by evaluating all segments in the collection.
    /// </remarks>
    public static ClosestPointResult GetClosestPoint(this Line self, Segment.Segments segments)
    {
        if (segments.Count <= 0) return new();
        
        var curSegment = segments[0];
        var closestResult = self.GetClosestPoint(curSegment);
        var otherIndex = 0;
        for (var i = 1; i < segments.Count; i++)
        {
            curSegment = segments[i];
            var result = self.GetClosestPoint(curSegment);

            if (result.IsCloser(closestResult))
            {
                otherIndex = i;
                closestResult = result;
            }
        }
        return closestResult.SetOtherSegmentIndex(otherIndex);
    }
    
}