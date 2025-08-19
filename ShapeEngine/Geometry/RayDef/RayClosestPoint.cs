using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.RayDef;

public readonly partial struct Ray
{
    
    /// <summary>
    /// Finds the closest point on the ray to any collider in the given collision object.
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
    /// Finds the closest point on this ray to the given collider.
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
    /// Finds the closest point on this ray to a given point.
    /// </summary>
    /// <param name="point">The point to find the closest point to.</param>
    /// <param name="disSquared">The squared distance from the point to the closest point on the ray.</param>
    /// <returns>A <see cref="IntersectionPoint"/> representing the closest point and its normal.</returns>
    /// <remarks>
    /// If the projection of the point onto the ray is behind the ray's origin, the origin is returned as the closest point.
    /// </remarks>
    public IntersectionPoint GetClosestPoint(Vector2 point, out float disSquared)
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
    /// <summary>
    /// Finds the closest points between this ray and a line.
    /// </summary>
    /// <param name="other">The line to find the closest point to.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points and their normals.</returns>
    /// <remarks>
    /// Uses a helper function to compute the closest points and distance squared.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Line other)
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
    /// <summary>
    /// Finds the closest points between this ray and another ray.
    /// </summary>
    /// <param name="other">The other ray to find the closest point to.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points and their normals.</returns>
    /// <remarks>
    /// Uses a helper function to compute the closest points and distance squared.
    /// </remarks>
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
    /// <summary>
    /// Finds the closest points between this ray and a segment.
    /// </summary>
    /// <param name="other">The segment to find the closest point to.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points and their normals.</returns>
    /// <remarks>
    /// Uses a helper function to compute the closest points and distance squared.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Segment other)
    {
        var result = Segment.GetClosestPointSegmentRay(other.Start, other.End, Point, Direction, out var disSquared);
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
    /// <summary>
    /// Finds the closest points between this ray and a circle.
    /// </summary>
    /// <param name="other">The circle to find the closest point to.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points and their normals.</returns>
    /// <remarks>
    /// Computes the closest point on the ray to the circle's center, then finds the closest point on the circle's circumference.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Circle other)
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
    /// <summary>
    /// Finds the closest points between this ray and a triangle.
    /// </summary>
    /// <param name="other">The triangle to find the closest point to.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points and their normals.
    /// The <c>otherSegmentIndex</c> indicates the closest edge of the triangle.
    /// <list type="bullet">
    /// <item>0 = AB</item>
    /// <item>1 = BC</item>
    /// <item>2 = CA</item>
    /// </list></returns>
    /// <remarks>
    /// Checks each edge of the triangle for the closest point.
    /// </remarks>
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
    /// <summary>
    /// Finds the closest points between this ray and a quad.
    /// </summary>
    /// <param name="other">The quad to find the closest point to.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points and their normals.
    /// The <c>otherSegmentIndex</c> indicates the closest edge of the quad.
    /// <list type="bullet">
    /// <item>0 = AB</item>
    /// <item>1 = BC</item>
    /// <item>2 = CD</item>
    /// <item>3 = DA</item>
    /// </list></returns>
    /// <remarks>
    /// Checks each edge of the quad for the closest point.
    /// </remarks>
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
    /// <summary>
    /// Finds the closest points between this ray and a rectangle.
    /// </summary>
    /// <param name="other">The rectangle to find the closest point to.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points and their normals.
    /// The <c>otherSegmentIndex</c> indicates the closest edge of the rect.
    /// <list type="bullet">
    /// <item>0 = AB</item>
    /// <item>1 = BC</item>
    /// <item>2 = CD</item>
    /// <item>3 = DA</item>
    /// </list></returns>
    /// <remarks>
    /// Checks each edge of the rectangle for the closest point.
    /// </remarks>
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
    /// Finds the closest points between this ray and a polygon.
    /// </summary>
    /// <param name="other">The polygon to find the closest point to.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points and their normals.
    /// The <c>otherIndex</c> is the edge index of the polygon.</returns>
    /// <remarks>
    /// Checks each edge of the polygon for the closest point.
    /// </remarks>
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
    /// <summary>
    /// Finds the closest points between this ray and a polyline.
    /// </summary>
    /// <param name="other">The polyline to find the closest point to.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points and their normals.
    /// The <c>otherIndex</c> is the edge index of the polyline.</returns>
    /// <remarks>
    /// Checks each segment of the polyline for the closest point.
    /// </remarks>
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
    /// <summary>
    /// Finds the closest points between this ray and a set of segments.
    /// </summary>
    /// <param name="segments">The set of segments to find the closest point to.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points and their normals.
    /// The <c>otherIndex</c> is the index of the segment.</returns>
    /// <remarks>
    /// Iterates through each segment and finds the closest point.
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