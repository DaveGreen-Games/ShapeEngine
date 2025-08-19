using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.LineDef;

public readonly partial struct Line
{
    /// <summary>
    /// Finds the closest point on the Line's perimeter to any collider in the given collision object.
    /// </summary>
    /// <param name="collisionObject">The collision object containing one or more colliders to compare against.</param>
    /// <returns>
    /// A <see cref="ClosestPointResult"/> containing the closest point information for the nearest collider.
    /// </returns>
    public ClosestPointResult GetClosestPoint(CollisionObject collisionObject)
    {
        if (!collisionObject.HasColliders) return new();
        var closestPoint = new ClosestPointResult();
        foreach (var collider in collisionObject.Colliders)
        {
            var result = GetClosestPoint(collider);
            if(!result.Valid) continue;
            if (!closestPoint.Valid) closestPoint = result;
            else
            {
                if (result.DistanceSquared < closestPoint.DistanceSquared) closestPoint = result;
            }
        }
        return closestPoint;
    }
  
    /// <summary>
    /// Finds the closest point on this Line's perimeter to the given collider.
    /// </summary>
    /// <param name="collider">The collider to compare against.</param>
    /// <returns>
    /// A <see cref="ClosestPointResult"/> containing the closest point information for the collider.
    /// </returns>
    public ClosestPointResult GetClosestPoint(Collider collider)
    {
        if (!collider.Enabled) return new();
        switch (collider.GetShapeType())
        {
            case ShapeType.Line: return GetClosestPoint(collider.GetLineShape());
            case ShapeType.Ray: return GetClosestPoint(collider.GetRayShape());
            case ShapeType.Circle: return GetClosestPoint(collider.GetCircleShape());
            case ShapeType.Segment: return GetClosestPoint(collider.GetSegmentShape());
            case ShapeType.Triangle: return GetClosestPoint(collider.GetTriangleShape());
            case ShapeType.Rect: return GetClosestPoint(collider.GetRectShape());
            case ShapeType.Quad: return GetClosestPoint(collider.GetQuadShape());
            case ShapeType.Poly: return GetClosestPoint(collider.GetPolygonShape());
            case ShapeType.PolyLine: return GetClosestPoint(collider.GetPolylineShape());
        }

        return new();
    }
    
    /// <summary>
    /// Calculates the closest point on this line to a given point in 2D space.
    /// </summary>
    /// <param name="point">The point from which the closest point on the line is sought.</param>
    /// <param name="disSquared">Outputs the squared distance between the closest point on the line and the given point.</param>
    /// <returns>A <see cref="IntersectionPoint"/> representing the closest point on the line and its normal.</returns>
    /// <remarks>
    /// The normal is oriented to face the point if it is on the same side as the line's normal, otherwise it is flipped.
    /// </remarks>
    public IntersectionPoint GetClosestPoint(Vector2 point, out float disSquared)
    {
        // Normalize the direction vector of the line
        var normalizedLineDirection = Direction.Normalize();

        // Calculate the vector from the line's point to the given point
        var toPoint = point - Point;

        // Project the vector to the point onto the line direction
        float projectionLength = Vector2.Dot(toPoint, normalizedLineDirection);

        // Calculate the closest point on the line
        var closestPointOnLine = Point + projectionLength * normalizedLineDirection;
        disSquared = (closestPointOnLine - point).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
        var dir = (point - closestPointOnLine).Normalize();
        var dot = Vector2.Dot(dir, Normal);
        if (dot >= 0) return new(closestPointOnLine, Normal);
        return new(closestPointOnLine, -Normal);
    }

    /// <summary>
    /// Calculates the closest points between this line and another line.
    /// </summary>
    /// <param name="other">The other <see cref="Line"/> to which the closest point is sought.</param>
    /// <returns>
    /// A <see cref="ClosestPointResult"/> containing the closest points on both lines, their normals, and the squared distance between them.
    /// If the lines intersect, the points are identical and the distance is zero. If the lines are parallel, an empty result is returned.
    /// </returns>
    public ClosestPointResult GetClosestPoint(Line other)
    {
        var result = IntersectLineLine(Point, Direction, other.Point, other.Direction);
        if (result.Valid)
        {
            return new
            (
                new(result.Point, Normal),
                new(result.Point, other.Normal),
                0f
            );
        }

        return new();
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
        // float t1 = (b * f - c * e) / denominator;
        // float t2 = (a * f - b * c) / denominator;
        //
        // var closestPoint1 = Point + t1 * d1;
        // var closestPoint2 = other.Point + t2 * d2;
        //
        // float disSquared = (closestPoint1 - closestPoint2).LengthSquared();
        // return new(
        //     new(closestPoint1, Normal), 
        //     new(closestPoint2, other.Normal),
        //     disSquared);
    }

    /// <summary>
    /// Calculates the closest points between this line and a ray.
    /// </summary>
    /// <param name="other">The <see cref="Ray"/> to which the closest point is sought.</param>
    /// <returns>
    /// A <see cref="ClosestPointResult"/> containing the closest points on the line and the ray, their normals, and the squared distance between them.
    /// </returns>
    public ClosestPointResult GetClosestPoint(Ray other)
    {
        var result = GetClosestPointLineRay(Point, Direction, other.Point, other.Direction, out float disSquared);
        return new(
            new(result.self, Normal),
            new(result.other, other.Normal),
            disSquared);
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
        // float t1 = (b * f - c * e) / denominator;
        // float t2 = Math.Max(0, (a * f - b * c) / denominator);
        //
        // var closestPoint1 = Point + t1 * d1;
        // var closestPoint2 = other.Point + t2 * d2;
        // float disSquared = (closestPoint1 - closestPoint2).LengthSquared();
        // return new
        // (
        //     new(closestPoint1, Normal), 
        //     new(closestPoint2, other.Normal),
        //     disSquared
        // );
    }

    /// <summary>
    /// Calculates the closest points between this line and a segment.
    /// </summary>
    /// <param name="other">The <see cref="Segment"/> to which the closest point is sought.</param>
    /// <returns>
    /// A <see cref="ClosestPointResult"/> containing the closest points on the line and the segment, their normals, and the squared distance between them.
    /// </returns>
    public ClosestPointResult GetClosestPoint(Segment other)
    {
        var result = Segment.GetClosestPointSegmentLine(other.Start, other.End, Point, Direction, out var disSquared);
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
        // float t1 = (b * f - c * e) / denominator;
        // float t2 = Math.Max(0, Math.Min(1, (a * f - b * c) / denominator));
        //
        // var closestPoint1 = Point + t1 * d1;
        // var closestPoint2 = other.Start + t2 * d2;
        //
        // float disSquared = (closestPoint1 - closestPoint2).LengthSquared();
        // return new(
        //     new(closestPoint1, Normal), 
        //     new(closestPoint2, other.Normal),
        //     disSquared);
    }

    /// <summary>
    /// Calculates the closest points between this line and a circle.
    /// </summary>
    /// <param name="other">The <see cref="Circle"/> to which the closest point is sought.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points on the line and the circle, their normals, and the squared distance between them.</returns>
    /// <remarks>
    /// The closest point on the circle is found by projecting the closest point on the line onto the circle's perimeter.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Circle other)
    {
        // var pointOnLine = GetClosestPointLinePoint(Point, Direction, other.Center, out float disSquared);
        //
        // var dir = (pointOnLine - other.Center).Normalize();
        // var pointOnCircle = other.Center + dir * other.Radius;
        // disSquared = (pointOnLine - pointOnCircle).LengthSquared();
        // return new(
        //     new(pointOnLine, Normal),
        //     new(pointOnCircle, dir),
        //     disSquared
        // );
        var d1 = Direction;

        var toCenter = other.Center - Point;
        float projectionLength = Vector2.Dot(toCenter, d1);
        var closestPointOnLine = Point + projectionLength * d1;

        var offset = (closestPointOnLine - other.Center).Normalize() * other.Radius;
        var closestPointOnCircle = other.Center + offset;

        float disSquared = (closestPointOnCircle - closestPointOnLine).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
        return new(
            new(closestPointOnLine, Normal),
            new(closestPointOnCircle, (closestPointOnCircle - other.Center).Normalize()),
            disSquared);
    }

    /// <summary>
    /// Calculates the closest points between this line and a triangle.
    /// </summary>
    /// <param name="other">The <see cref="Triangle"/> to which the closest point is sought.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points on the line and the triangle, their normals, the squared distance, and the index of the closest triangle edge.</returns>
    /// <remarks>
    /// The closest point is determined by evaluating all triangle edges.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Triangle other)
    {
        var closestResult = GetClosestPointLineSegment(Point, Direction, other.A, other.B, out float minDisSquared);

        var otherNormal = (other.B - other.A);
        var otherIndex = 0;
        var result = GetClosestPointLineSegment(Point, Direction, other.B, other.C, out float disSquared);

        if (disSquared < minDisSquared)
        {
            otherIndex = 1;
            minDisSquared = disSquared;
            closestResult = result;
            otherNormal = (other.C - other.B);
        }

        result = GetClosestPointLineSegment(Point, Direction, other.C, other.A, out disSquared);

        if (disSquared < minDisSquared)
        {
            var normal = (other.A - other.C).GetPerpendicularRight().Normalize();
            return new(
                new(result.self, Normal),
                new(result.other, normal),
                disSquared,
                -1,
                2);
        }

        return new(
            new(closestResult.self, Normal),
            new(closestResult.other, otherNormal.GetPerpendicularRight().Normalize()),
            minDisSquared,
            -1,
            otherIndex);
    }

    /// <summary>
    /// Calculates the closest points between this line and a quadrilateral.
    /// </summary>
    /// <param name="other">The <see cref="Quad"/> to which the closest point is sought.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points on the line and the quad, their normals, the squared distance, and the index of the closest quad edge.</returns>
    /// <remarks>
    /// The closest point is determined by evaluating all quad edges.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Quad other)
    {
        var closestResult = GetClosestPointLineSegment(Point, Direction, other.A, other.B, out float minDisSquared);
        var otherNormal = (other.B - other.A);
        var otherIndex = 0;

        var result = GetClosestPointLineSegment(Point, Direction, other.B, other.C, out float disSquared);
        if (disSquared < minDisSquared)
        {
            otherIndex = 1;
            minDisSquared = disSquared;
            closestResult = result;
            otherNormal = (other.C - other.B);
        }

        result = GetClosestPointLineSegment(Point, Direction, other.C, other.D, out disSquared);
        if (disSquared < minDisSquared)
        {
            otherIndex = 2;
            minDisSquared = disSquared;
            closestResult = result;
            otherNormal = (other.D - other.C);
        }

        result = GetClosestPointLineSegment(Point, Direction, other.D, other.A, out disSquared);
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

    /// <summary>
    /// Calculates the closest points between this line and a rectangle.
    /// </summary>
    /// <param name="other">The <see cref="Rect"/> to which the closest point is sought.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points on the line and the rectangle, their normals, the squared distance, and the index of the closest rectangle edge.</returns>
    /// <remarks>
    /// The closest point is determined by evaluating all rectangle edges.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Rect other)
    {
        var closestResult = GetClosestPointLineSegment(Point, Direction, other.A, other.B, out float minDisSquared);
        var otherNormal = (other.B - other.A);
        var otherIndex = 0;

        var result = GetClosestPointLineSegment(Point, Direction, other.B, other.C, out float disSquared);
        if (disSquared < minDisSquared)
        {
            otherIndex = 1;
            minDisSquared = disSquared;
            closestResult = result;
            otherNormal = (other.C - other.B);
        }

        result = GetClosestPointLineSegment(Point, Direction, other.C, other.D, out disSquared);
        if (disSquared < minDisSquared)
        {
            otherIndex = 2;
            minDisSquared = disSquared;
            closestResult = result;
            otherNormal = (other.D - other.C);
        }

        result = GetClosestPointLineSegment(Point, Direction, other.D, other.A, out disSquared);
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

    /// <summary>
    /// Calculates the closest points between this line and a polygon.
    /// </summary>
    /// <param name="other">The <see cref="Polygon"/> to which the closest point is sought.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points on the line and the polygon, their normals, the squared distance, and the index of the closest polygon edge.</returns>
    /// <remarks>
    /// The closest point is determined by evaluating all polygon edges.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Polygon other)
    {
        if (other.Count < 3) return new();

        var p1 = other[0];
        var p2 = other[1];
        var closestResult = GetClosestPointLineSegment(Point, Direction, p1, p2, out float minDisSquared);
        var otherNormal = (p2 - p1);
        var otherIndex = 0;
        for (var i = 1; i < other.Count; i++)
        {
            p1 = other[i];
            p2 = other[(i + 1) % other.Count];
            var result = GetClosestPointLineSegment(Point, Direction, p1, p2, out float disSquared);
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

    /// <summary>
    /// Calculates the closest points between this line and a polyline.
    /// </summary>
    /// <param name="other">The <see cref="Polyline"/> to which the closest point is sought.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points on the line and the polyline, their normals, the squared distance, and the index of the closest polyline segment.</returns>
    /// <remarks>
    /// The closest point is determined by evaluating all polyline segments.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Polyline other)
    {
        if (other.Count < 2) return new();

        var p1 = other[0];
        var p2 = other[1];
        var closestResult = GetClosestPointLineSegment(Point, Direction, p1, p2, out float minDisSquared);
        var otherNormal = (p2 - p1);
        var otherIndex = 0;
        for (var i = 1; i < other.Count - 1; i++)
        {
            p1 = other[i];
            p2 = other[i + 1];
            var result = GetClosestPointLineSegment(Point, Direction, p1, p2, out float disSquared);
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

    /// <summary>
    /// Calculates the closest points between this line and a collection of segments.
    /// </summary>
    /// <param name="segments">The <see cref="Segments"/> collection to which the closest point is sought.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points on the line and the closest segment, their normals, the squared distance, and the index of the closest segment.</returns>
    /// <remarks>
    /// The closest point is determined by evaluating all segments in the collection.
    /// </remarks>
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
    
    /// <summary>
    /// Finds the closest point on this shapes perimeter to the given <see cref="IShape"/>.
    /// </summary>
    /// <param name="shape">The shape to compare against.</param>
    /// <returns>
    /// A <see cref="ClosestPointResult"/> containing the closest point information for the shape.
    /// </returns>
    public ClosestPointResult GetClosestPoint(IShape shape)
    {
        return shape.GetShapeType() switch
        {
            ShapeType.Circle => GetClosestPoint(shape.GetCircleShape()),
            ShapeType.Segment => GetClosestPoint(shape.GetSegmentShape()),
            ShapeType.Ray => GetClosestPoint(shape.GetRayShape()),
            ShapeType.Line => GetClosestPoint(shape.GetLineShape()),
            ShapeType.Triangle => GetClosestPoint(shape.GetTriangleShape()),
            ShapeType.Rect => GetClosestPoint(shape.GetRectShape()),
            ShapeType.Quad => GetClosestPoint(shape.GetQuadShape()),
            ShapeType.Poly => GetClosestPoint(shape.GetPolygonShape()),
            ShapeType.PolyLine => GetClosestPoint(shape.GetPolylineShape()),
            _ => new()
        };
    }

}