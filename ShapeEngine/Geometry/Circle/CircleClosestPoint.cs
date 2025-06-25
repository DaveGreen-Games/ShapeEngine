using System.Numerics;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.Segment;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.Circle;

public readonly partial struct Circle
{
    #region Closest Point

    /// <summary>
    /// Gets the closest point on the circle to a given point.
    /// </summary>
    /// <param name="p">The point to check.</param>
    /// <param name="disSquared">The squared distance between the circle and the point.</param>
    /// <returns>A <see cref="CollisionPoint"/> representing the closest point.</returns>
    public CollisionPoint GetClosestPoint(Vector2 p, out float disSquared)
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
    public ClosestPointResult GetClosestPoint(Line.Line other)
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
    public ClosestPointResult GetClosestPoint(Ray.Ray other)
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
    public ClosestPointResult GetClosestPoint(Segment.Segment other)
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
    public ClosestPointResult GetClosestPoint(Triangle.Triangle other)
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
    public ClosestPointResult GetClosestPoint(Quad.Quad other)
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
    public ClosestPointResult GetClosestPoint(Rect.Rect other)
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
    public ClosestPointResult GetClosestPoint(Polygon.Polygon other)
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
    public ClosestPointResult GetClosestPoint(Polyline.Polyline other)
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
    public ClosestPointResult GetClosestPoint(Segments.Segments segments)
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

    #endregion
}