using System.Numerics;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.CircleDef;

public readonly partial struct Circle
{
    /// <summary>
    /// Finds the closest point on the circle's perimeter to any collider in the given collision object.
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
    /// Finds the closest point on this circle's perimeter to the given collider.
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
    /// Gets the closest point on the circle to a given point.
    /// </summary>
    /// <param name="p">The point to check.</param>
    /// <param name="disSquared">The squared distance between the circle and the point.</param>
    /// <returns>A <see cref="IntersectionPoint"/> representing the closest point.</returns>
    public IntersectionPoint GetClosestPoint(Vector2 p, out float disSquared)
    {
        var dir = (p - Center).Normalize();
        var closestPoint = Center + dir * Radius;
        var normal = (closestPoint - Center).Normalize();
        disSquared = (closestPoint - p).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
        return new(closestPoint, normal);
    }

    /// <summary>
    /// Gets the closest point between the circle and a line.
    /// </summary>
    /// <param name="other">The line to check.</param>
    /// <returns>A <see cref="ClosestPointResult"/> representing the closest point.</returns>
    public ClosestPointResult GetClosestPoint(Line other)
    {
        var d1 = other.Direction;

        var toCenter = Center - other.Point;
        float projectionLength = Vector2.Dot(toCenter, d1);
        var closestPointOnLine = other.Point + projectionLength * d1;

        var offset = (closestPointOnLine - Center).Normalize() * Radius;
        var closestPointOnCircle = Center + offset;
        float disSquared = (closestPointOnLine - closestPointOnCircle).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
        var circleNormal = (closestPointOnCircle - Center).Normalize();

        return new(
            new(closestPointOnCircle, circleNormal),
            new(closestPointOnLine, other.Normal),
            disSquared);
    }

    /// <summary>
    /// Gets the closest point between the circle and a ray.
    /// </summary>
    /// <param name="other">The ray to check.</param>
    /// <returns>A <see cref="ClosestPointResult"/> representing the closest point.</returns>
    public ClosestPointResult GetClosestPoint(Ray other)
    {
        var d1 = other.Direction;

        var toCenter = Center - other.Point;
        float projectionLength = Math.Max(0, Vector2.Dot(toCenter, d1));
        var closestPointOnRay = other.Point + projectionLength * d1;

        var offset = (closestPointOnRay - Center).Normalize() * Radius;
        var closestPointOnCircle = Center + offset;
        var circleNormal = (closestPointOnCircle - Center).Normalize();
        float disSquared = (closestPointOnRay - closestPointOnCircle).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
        return new(
            new(closestPointOnCircle, circleNormal),
            new(closestPointOnRay, other.Normal),
            disSquared);
    }

    /// <summary>
    /// Gets the closest point between the circle and a segment.
    /// </summary>
    /// <param name="other">The segment to check.</param>
    /// <returns>A <see cref="ClosestPointResult"/> representing the closest point.</returns>
    public ClosestPointResult GetClosestPoint(Segment other)
    {
        var d1 = other.End - other.Start;

        var toCenter = Center - other.Start;
        float projectionLength = Vector2.Dot(toCenter, d1) / d1.LengthSquared();
        projectionLength = Math.Clamp(projectionLength, 0.0f, 1.0f);
        var closestPointOnSegment = other.Start + projectionLength * d1;

        var offset = Vector2.Normalize(closestPointOnSegment - Center) * Radius;
        var closestPointOnCircle = Center + offset;
        var circleNormal = (closestPointOnCircle - Center).Normalize();
        float disSquared = (closestPointOnCircle - closestPointOnSegment).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
        return new(
            new(closestPointOnCircle, circleNormal),
            new(closestPointOnSegment, other.Normal),
            disSquared);
    }

    /// <summary>
    /// Gets the closest point between two circles.
    /// </summary>
    /// <param name="other">The other circle to check.</param>
    /// <returns>A <see cref="ClosestPointResult"/> representing the closest point.</returns>
    public ClosestPointResult GetClosestPoint(Circle other)
    {
        var w = Center - other.Center;
        var dir = w.Normalize();
        var a = Center - dir * Radius;
        var aNormal = (a - Center).Normalize();
        var b = other.Center + dir * other.Radius;
        var bNormal = (b - other.Center).Normalize();
        float disSquared = (a - b).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
        return new(
            new(a, aNormal),
            new(b, bNormal),
            disSquared);
    }

    /// <summary>
    /// Gets the closest point between the circle and a triangle.
    /// </summary>
    /// <param name="other">The triangle to check.</param>
    /// <returns>A <see cref="ClosestPointResult"/> representing the closest point.</returns>
    public ClosestPointResult GetClosestPoint(Triangle other)
    {
        var closestResult = GetClosestPointCircleSegment(Center, Radius, other.A, other.B, out float minDisSquared);
        var otherNormal = (other.B - other.A);
        var otherIndex = 0;

        var result = GetClosestPointCircleSegment(Center, Radius, other.B, other.C, out float disSquared);
        if (disSquared < minDisSquared)
        {
            otherIndex = 1;
            minDisSquared = disSquared;
            closestResult = result;
            otherNormal = (other.C - other.B);
        }

        result = GetClosestPointCircleSegment(Center, Radius, other.C, other.A, out disSquared);
        if (disSquared < minDisSquared)
        {
            var normal = (other.A - other.C).GetPerpendicularRight().Normalize();
            var circleNormal = (result.self - Center).Normalize();
            return new(
                new(result.self, circleNormal),
                new(result.other, normal),
                disSquared,
                -1,
                2);
        }

        var selfNormal = (closestResult.self - Center).Normalize();
        return new(
            new(closestResult.self, selfNormal),
            new(closestResult.other, otherNormal.GetPerpendicularRight().Normalize()),
            minDisSquared,
            -1,
            otherIndex);
    }

    /// <summary>
    /// Gets the closest point between the circle and a quad.
    /// </summary>
    /// <param name="other">The quad to check.</param>
    /// <returns>A <see cref="ClosestPointResult"/> representing the closest point.</returns>
    public ClosestPointResult GetClosestPoint(Quad other)
    {
        var closestResult = GetClosestPointCircleSegment(Center, Radius, other.A, other.B, out float minDisSquared);
        var otherNormal = (other.B - other.A);
        var otherIndex = 0;

        var result = GetClosestPointCircleSegment(Center, Radius, other.B, other.C, out float disSquared);
        if (disSquared < minDisSquared)
        {
            otherIndex = 1;
            minDisSquared = disSquared;
            closestResult = result;
            otherNormal = (other.C - other.B);
        }

        result = GetClosestPointCircleSegment(Center, Radius, other.C, other.D, out disSquared);
        if (disSquared < minDisSquared)
        {
            otherIndex = 2;
            minDisSquared = disSquared;
            closestResult = result;
            otherNormal = (other.D - other.C);
        }

        result = GetClosestPointCircleSegment(Center, Radius, other.D, other.A, out disSquared);
        if (disSquared < minDisSquared)
        {
            var normal = (other.A - other.D).GetPerpendicularRight().Normalize();
            var circleNormal = (result.self - Center).Normalize();
            return new(
                new(result.self, circleNormal),
                new(result.other, normal),
                disSquared,
                -1,
                3);
        }

        var selfNormal = (closestResult.self - Center).Normalize();
        return new(
            new(closestResult.self, selfNormal),
            new(closestResult.other, otherNormal.GetPerpendicularRight().Normalize()),
            minDisSquared,
            -1,
            otherIndex);
    }

    /// <summary>
    /// Gets the closest point between the circle and a rectangle.
    /// </summary>
    /// <param name="other">The rectangle to check.</param>
    /// <returns>A <see cref="ClosestPointResult"/> representing the closest point.</returns>
    public ClosestPointResult GetClosestPoint(Rect other)
    {
        var closestResult = GetClosestPointCircleSegment(Center, Radius, other.A, other.B, out float minDisSquared);
        var otherNormal = (other.B - other.A);
        var otherIndex = 0;

        var result = GetClosestPointCircleSegment(Center, Radius, other.B, other.C, out float disSquared);
        if (disSquared < minDisSquared)
        {
            otherIndex = 1;
            minDisSquared = disSquared;
            closestResult = result;
            otherNormal = (other.C - other.B);
        }

        result = GetClosestPointCircleSegment(Center, Radius, other.C, other.D, out disSquared);
        if (disSquared < minDisSquared)
        {
            otherIndex = 2;
            minDisSquared = disSquared;
            closestResult = result;
            otherNormal = (other.D - other.C);
        }

        result = GetClosestPointCircleSegment(Center, Radius, other.D, other.A, out disSquared);
        if (disSquared < minDisSquared)
        {
            var normal = (other.A - other.D).GetPerpendicularRight().Normalize();
            var circleNormal = (result.self - Center).Normalize();
            return new(
                new(result.self, circleNormal),
                new(result.other, normal),
                disSquared,
                -1,
                3);
        }

        var selfNormal = (closestResult.self - Center).Normalize();
        return new(
            new(closestResult.self, selfNormal),
            new(closestResult.other, otherNormal.GetPerpendicularRight().Normalize()),
            minDisSquared,
            -1,
            otherIndex);
    }

    /// <summary>
    /// Gets the closest point between the circle and a polygon.
    /// </summary>
    /// <param name="other">The polygon to check.</param>
    /// <returns>A <see cref="ClosestPointResult"/> representing the closest point.</returns>
    public ClosestPointResult GetClosestPoint(Polygon other)
    {
        if (other.Count < 3) return new();

        var p1 = other[0];
        var p2 = other[1];
        var closestResult = GetClosestPointCircleSegment(Center, Radius, p1, p2, out float minDisSquared);
        var otherNormal = (p2 - p1);
        var otherIndex = 0;
        for (var i = 1; i < other.Count; i++)
        {
            p1 = other[i];
            p2 = other[(i + 1) % other.Count];
            var result = GetClosestPointCircleSegment(Center, Radius, p1, p2, out float disSquared);
            if (disSquared < minDisSquared)
            {
                otherIndex = i;
                minDisSquared = disSquared;
                closestResult = result;
                otherNormal = (p2 - p1);
            }
        }

        var selfNormal = (closestResult.self - Center).Normalize();
        return new(
            new(closestResult.self, selfNormal),
            new(closestResult.other, otherNormal.GetPerpendicularRight().Normalize()),
            minDisSquared,
            -1,
            otherIndex);
    }

    /// <summary>
    /// Gets the closest point between the circle and a polyline.
    /// </summary>
    /// <param name="other">The polyline to check.</param>
    /// <returns>A <see cref="ClosestPointResult"/> representing the closest point.</returns>
    public ClosestPointResult GetClosestPoint(Polyline other)
    {
        if (other.Count < 2) return new();

        var p1 = other[0];
        var p2 = other[1];
        var closestResult = GetClosestPointCircleSegment(Center, Radius, p1, p2, out float minDisSquared);
        var otherNormal = (p2 - p1);
        var otherIndex = 0;
        for (var i = 1; i < other.Count - 1; i++)
        {
            p1 = other[i];
            p2 = other[i + 1];
            var result = GetClosestPointCircleSegment(Center, Radius, p1, p2, out float disSquared);
            if (disSquared < minDisSquared)
            {
                otherIndex = i;
                minDisSquared = disSquared;
                closestResult = result;
                otherNormal = (p2 - p1);
            }
        }

        var selfNormal = (closestResult.self - Center).Normalize();
        return new(
            new(closestResult.self, selfNormal),
            new(closestResult.other, otherNormal.GetPerpendicularRight().Normalize()),
            minDisSquared,
            -1,
            otherIndex);
    }

    /// <summary>
    /// Gets the closest point between the circle and a collection of segments.
    /// </summary>
    /// <param name="segments">The segments to check.</param>
    /// <returns>A <see cref="ClosestPointResult"/> representing the closest point.</returns>
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
    /// Gets the closest vertex on the circle to a given point.
    /// </summary>
    /// <param name="p">The point to check.</param>
    /// <param name="disSquared">The squared distance between the circle and the vertex.</param>
    /// <returns>The closest vertex on the circle.</returns>
    public Vector2 GetClosestVertex(Vector2 p, out float disSquared)
    {
        var vertex = Center + (p - Center).Normalize() * Radius;
        disSquared = (vertex - p).LengthSquared();
        return vertex;
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