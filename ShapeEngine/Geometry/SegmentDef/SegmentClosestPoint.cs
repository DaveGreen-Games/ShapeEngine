using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.SegmentDef;

public readonly partial struct Segment
{
    /// <summary>
    /// Finds the closest point on the segment to any collider in the given collision object.
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
    /// Finds the closest point on this segment to the given collider.
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
    /// Finds the closest point on this segment to a given point.
    /// </summary>
    /// <param name="p">The point to find the closest point to.</param>
    /// <param name="disSquared">The squared distance from the point to the closest point on the segment.</param>
    /// <returns>A <see cref="IntersectionPoint"/> representing the closest point and its normal.</returns>
    /// <remarks>
    /// If the closest point is at the segment's endpoint, the normal is chosen based on the direction to the point.
    /// </remarks>
    public IntersectionPoint GetClosestPoint(Vector2 p, out float disSquared)
    {
        Vector2 c;
        var w = Displacement;
        float t = (p - Start).Dot(w) / w.LengthSquared();
        if (t < 0f) c = Start;
        else if (t > 1f) c = End;
        else c = Start + w * t;

        var dir = p - c;
        disSquared = ShapeMath.ClampToZero(dir.LengthSquared());

        var dot = Vector2.Dot(dir.Normalize(), Normal);
        if (dot >= 0) return new(c, Normal);
        return new(c, -Normal);
    }

    /// <summary>
    /// Finds the closest points between this segment and a line.
    /// </summary>
    /// <param name="other">The line to compare with.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points and their normals.</returns>
    /// <remarks>
    /// The result includes the closest point on the segment and the closest point on the line, along with their normals and the squared distance between them.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Line other)
    {
        var result = GetClosestPointSegmentLine(Start, End, other.Point, other.Direction, out float disSquared);
        return new(
            new(result.self, Normal),
            new(result.other, other.Normal),
            disSquared);
    }

    /// <summary>
    /// Finds the closest points between this segment and a ray.
    /// </summary>
    /// <param name="other">The ray to compare with.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points and their normals.</returns>
    /// <remarks>
    /// The result includes the closest point on the segment and the closest point on the ray, along with their normals and the squared distance between them.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Ray other)
    {
        var result = GetClosestPointSegmentRay(Start, End, other.Point, other.Direction, out var disSquared);
        return new(
            new(result.self, Normal),
            new(result.other, other.Normal),
            disSquared);
    }

    /// <summary>
    /// Finds the closest points between this segment and another segment.
    /// </summary>
    /// <param name="other">The other segment to compare with.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points and their normals.</returns>
    /// <remarks>
    /// Handles degenerate cases where either or both segments are points.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Segment other)
    {
        var d1 = End - Start;
        var d2 = other.End - other.Start;
        var r = Start - other.Start;

        float a = Vector2.Dot(d1, d1);
        float e = Vector2.Dot(d2, d2);
        float f = Vector2.Dot(d2, r);

        float s, t;
        if (a <= ShapeMath.EpsilonF && e <= ShapeMath.EpsilonF)
        {
            // Both segments degenerate into points
            // s = t = 0.0f;

            return new(
                new(Start, Normal),
                new(other.Start, other.Normal),
                ShapeMath.ClampToZero(r.LengthSquared()));
        }

        if (a <= ShapeMath.EpsilonF)
        {
            // First segment degenerates into a point
            s = 0.0f;
            t = Math.Clamp(f / e, 0.0f, 1.0f);
        }
        else
        {
            float c = Vector2.Dot(d1, r);
            if (e <= ShapeMath.EpsilonF)
            {
                // Second segment degenerates into a point
                t = 0.0f;
                s = Math.Clamp(-c / a, 0.0f, 1.0f);
            }
            else
            {
                // The general nondegenerate case starts here
                float b = Vector2.Dot(d1, d2);
                float denom = a * e - b * b;

                if (denom != 0.0f)
                {
                    s = Math.Clamp((b * f - c * e) / denom, 0.0f, 1.0f);
                }
                else
                {
                    s = 0.0f;
                }

                t = (b * s + f) / e;

                if (t < 0.0f)
                {
                    t = 0.0f;
                    s = Math.Clamp(-c / a, 0.0f, 1.0f);
                }
                else if (t > 1.0f)
                {
                    t = 1.0f;
                    s = Math.Clamp((b - c) / a, 0.0f, 1.0f);
                }
            }
        }

        var closestPoint1 = Start + s * d1;
        var closestPoint2 = other.Start + t * d2;
        float disSquared = (closestPoint2 - closestPoint1).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);

        return new(
            new(closestPoint1, Normal),
            new(closestPoint2, other.Normal),
            disSquared);
    }

    /// <summary>
    /// Finds the closest points between this segment and a circle.
    /// </summary>
    /// <param name="other">The circle to compare with.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points and their normals.</returns>
    /// <remarks>
    /// The closest point on the circle is found along the direction from the circle's center to the segment.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Circle other)
    {
        var d1 = End - Start;
        var p1 = other.Center;

        var toCenter = p1 - Start;
        float projectionLength = Vector2.Dot(toCenter, d1) / d1.LengthSquared();
        projectionLength = Math.Clamp(projectionLength, 0.0f, 1.0f);
        Vector2 closestPointOnSegment = Start + projectionLength * d1;

        var offset = Vector2.Normalize(closestPointOnSegment - p1) * other.Radius;
        var closestPointOnCircle = p1 + offset;

        float disSquared = (closestPointOnCircle - closestPointOnSegment).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
        return new(
            new(closestPointOnSegment, Normal),
            new(closestPointOnCircle, (closestPointOnCircle - other.Center).Normalize()),
            disSquared);
    }

    /// <summary>
    /// Finds the closest points between this segment and a triangle.
    /// </summary>
    /// <param name="other">The triangle to compare with.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points and their normals.</returns>
    /// <remarks>
    /// The closest point is determined by checking all triangle edges.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Triangle other)
    {
        var closestResult = GetClosestPointSegmentSegment(Start, End, other.A, other.B, out float minDisSquared);
        var curNormal = (other.B - other.A);
        var otherIndex = 0;

        if (minDisSquared > 0)
        {
            var result = GetClosestPointSegmentSegment(Start, End, other.B, other.C, out float disSquared);
            if (disSquared < minDisSquared)
            {
                otherIndex = 1;
                minDisSquared = disSquared;
                closestResult = result;
                curNormal = (other.C - other.B);
            }
        }

        if (minDisSquared > 0)
        {
            var result = GetClosestPointSegmentSegment(Start, End, other.C, other.A, out float disSquared);
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
        }

        return new(
            new(closestResult.self, Normal),
            new(closestResult.other, curNormal.GetPerpendicularRight().Normalize()),
            minDisSquared,
            -1,
            otherIndex);
    }

    /// <summary>
    /// Finds the closest points between this segment and a quadrilateral.
    /// </summary>
    /// <param name="other">The quadrilateral to compare with.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points and their normals.</returns>
    /// <remarks>
    /// The closest point is determined by checking all quad edges.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Quad other)
    {
        var closestResult = GetClosestPointSegmentSegment(Start, End, other.A, other.B, out float minDisSquared);
        var curNormal = (other.B - other.A);
        var otherIndex = 0;
        var result = GetClosestPointSegmentSegment(Start, End, other.B, other.C, out float disSquared);
        if (disSquared < minDisSquared)
        {
            otherIndex = 1;
            minDisSquared = disSquared;
            closestResult = result;
            curNormal = (other.C - other.B);
        }

        result = GetClosestPointSegmentSegment(Start, End, other.C, other.D, out disSquared);
        if (disSquared < minDisSquared)
        {
            otherIndex = 2;
            minDisSquared = disSquared;
            closestResult = result;
            curNormal = (other.D - other.C);
        }

        result = GetClosestPointSegmentSegment(Start, End, other.D, other.A, out disSquared);
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
            new(closestResult.other, curNormal.GetPerpendicularRight().Normalize()),
            minDisSquared,
            -1,
            otherIndex);
    }

    /// <summary>
    /// Finds the closest points between this segment and a rectangle.
    /// </summary>
    /// <param name="other">The rectangle to compare with.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points and their normals.</returns>
    /// <remarks>
    /// The closest point is determined by checking all rectangle edges.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Rect other)
    {
        var closestResult = GetClosestPointSegmentSegment(Start, End, other.A, other.B, out float minDisSquared);
        var curNormal = (other.B - other.A);
        var otherIndex = 0;
        var result = GetClosestPointSegmentSegment(Start, End, other.B, other.C, out float disSquared);
        if (disSquared < minDisSquared)
        {
            otherIndex = 1;
            minDisSquared = disSquared;
            closestResult = result;
            curNormal = (other.C - other.B);
        }

        result = GetClosestPointSegmentSegment(Start, End, other.C, other.D, out disSquared);
        if (disSquared < minDisSquared)
        {
            otherIndex = 2;
            minDisSquared = disSquared;
            closestResult = result;
            curNormal = (other.D - other.C);
        }

        result = GetClosestPointSegmentSegment(Start, End, other.D, other.A, out disSquared);
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
            new(closestResult.other, curNormal.GetPerpendicularRight().Normalize()),
            minDisSquared,
            -1,
            otherIndex);
    }

    /// <summary>
    /// Finds the closest points between this segment and a polygon.
    /// </summary>
    /// <param name="other">The polygon to compare with.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points and their normals.</returns>
    /// <remarks>
    /// The closest point is determined by checking all polygon edges.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Polygon other)
    {
        if (other.Count < 3) return new();

        var p1 = other[0];
        var p2 = other[1];
        var closestResult = GetClosestPointSegmentSegment(Start, End, p1, p2, out float minDisSquared);
        var curNormal = (p2 - p1);
        var otherIndex = 0;
        for (var i = 1; i < other.Count; i++)
        {
            p1 = other[i];
            p2 = other[(i + 1) % other.Count];
            var result = GetClosestPointSegmentSegment(Start, End, p1, p2, out float disSquared);

            if (disSquared < minDisSquared)
            {
                otherIndex = i;
                minDisSquared = disSquared;
                closestResult = result;
                curNormal = (p2 - p1);
            }
        }

        return new(
            new(closestResult.self, Normal),
            new(closestResult.other, curNormal.GetPerpendicularRight().Normalize()),
            minDisSquared,
            -1,
            otherIndex);
    }

    /// <summary>
    /// Finds the closest points between this segment and a polyline.
    /// </summary>
    /// <param name="other">The polyline to compare with.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points and their normals.</returns>
    /// <remarks>
    /// The closest point is determined by checking all polyline segments.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Polyline other)
    {
        if (other.Count < 2) return new();

        var p1 = other[0];
        var p2 = other[1];
        var closestResult = GetClosestPointSegmentSegment(Start, End, p1, p2, out float minDisSquared);
        var curNormal = (p2 - p1);
        var otherIndex = 0;
        for (var i = 1; i < other.Count - 1; i++)
        {
            p1 = other[i];
            p2 = other[i + 1];
            var result = GetClosestPointSegmentSegment(Start, End, p1, p2, out float disSquared);

            if (disSquared < minDisSquared)
            {
                otherIndex = i;
                minDisSquared = disSquared;
                closestResult = result;
                curNormal = (p2 - p1);
            }
        }

        return new(
            new(closestResult.self, Normal),
            new(closestResult.other, curNormal.GetPerpendicularRight().Normalize()),
            minDisSquared,
            -1,
            otherIndex);
    }

    /// <summary>
    /// Finds the closest points between this segment and a set of segments.
    /// </summary>
    /// <param name="segments">The set of segments to compare with.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points and their normals.</returns>
    /// <remarks>
    /// Iterates through each segment in the set to find the closest points.
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
    /// Returns the vertex (Start or End) of the segment that is closest to the given point <paramref name="p"/>.
    /// </summary>
    /// <param name="p">The point to compare against the segment's vertices.</param>
    /// <param name="disSquared">The squared distance from <paramref name="p"/> to the closest vertex.</param>
    /// <returns>The closest vertex (Start or End) as a <see cref="Vector2"/>.</returns>
    public Vector2 GetClosestVertex(Vector2 p, out float disSquared)
    {
        float disSqA = (p - Start).LengthSquared();
        float disSqB = (p - End).LengthSquared();
        if (disSqA < disSqB)
        {
            disSquared = disSqA;
            return Start;
        }

        disSquared = disSqB;
        return End;
    }

    /// <summary>
    /// Returns the vertex (Start or End) of the segment that is furthest from the given point <paramref name="p"/>.
    /// </summary>
    /// <param name="p">The point to compare against the segment's vertices.</param>
    /// <param name="disSquared">The squared distance from <paramref name="p"/> to the furthest vertex.</param>
    /// <returns>The furthest vertex (Start or End) as a <see cref="Vector2"/>.</returns>
    public Vector2 GetFurthestVertex(Vector2 p, out float disSquared)
    {
        float disSqA = (p - Start).LengthSquared();
        float disSqB = (p - End).LengthSquared();
        if (disSqA > disSqB)
        {
            disSquared = disSqA;
            return Start;
        }

        disSquared = disSqB;
        return End;
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