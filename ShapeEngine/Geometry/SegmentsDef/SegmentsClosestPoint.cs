using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.SegmentsDef;

public partial class Segments
{
    /// <summary>
    /// Gets the closest point on a series of connected segments to a given point.
    /// </summary>
    /// <param name="points">The list of vertices that define the segments. Each pair of consecutive points is treated as a segment.</param>
    /// <param name="p">The point to find the closest point to.</param>
    /// <param name="disSquared">The squared distance between the point and the closest point.</param>
    /// <returns>The closest point on the segments to the given point. Returns a new Vector2() if there are 2 or less points.</returns>
    public static Vector2 GetClosestPointSegmentsPoint(List<Vector2> points, Vector2 p, out float disSquared)
    {
        disSquared = -1;
        if (points.Count <= 2) return new();

        var first = points[0];
        var second = points[1];
        var closest = Segment.GetClosestPointSegmentPoint(first, second, p, out disSquared);

        for (var i = 1; i < points.Count; i++)
        {
            var p1 = points[i];
            var p2 = points[(i + 1) % points.Count];

            var cp = Segment.GetClosestPointSegmentPoint(p1, p2, p, out float dis);
            if (dis < disSquared)
            {
                closest = cp;
                disSquared = dis;
            }
        }

        return closest;
    }

    /// <summary>
    /// Gets the closest point on the segments to a given point.
    /// </summary>
    /// <param name="p">The point to find the closest point to.</param>
    /// <param name="disSquared">The squared distance between the point and the closest point.</param>
    /// <returns>A IntersectionPoint representing the closest point. Returns a new IntersectionPoint() if there are 2 or less segments.</returns>
    public IntersectionPoint GetClosestPoint(Vector2 p, out float disSquared)
    {
        disSquared = -1;
        if (Count <= 2) return new();

        var closestSegment = this[0];
        var closestPoint = Segment.GetClosestPointSegmentPoint(closestSegment.Start, closestSegment.End, p, out disSquared);

        for (var i = 1; i < Count; i++)
        {
            var curSegment = this[i];

            var cp = Segment.GetClosestPointSegmentPoint(curSegment.Start, curSegment.End, p, out float dis);
            if (dis < disSquared)
            {
                closestSegment = curSegment;
                closestPoint = cp;
                disSquared = dis;
            }
        }

        return new(closestPoint, closestSegment.Normal);
    }

    /// <summary>
    /// Gets the closest point on the segments to a given point.
    /// </summary>
    /// <param name="p">The point to find the closest point to.</param>
    /// <param name="disSquared">The squared distance between the point and the closest point.</param>
    /// <param name="index">The index of the segment that contains the closest point.</param>
    /// <returns>A IntersectionPoint representing the closest point. Returns a new IntersectionPoint() if there are 2 or less segments.</returns>
    public IntersectionPoint GetClosestPoint(Vector2 p, out float disSquared, out int index)
    {
        disSquared = -1;
        index = -1;
        if (Count <= 2) return new();

        var closestSegment = this[0];
        var closestPoint = Segment.GetClosestPointSegmentPoint(closestSegment.Start, closestSegment.End, p, out disSquared);
        index = 0;
        for (var i = 1; i < Count; i++)
        {
            var curSegment = this[i];

            var cp = Segment.GetClosestPointSegmentPoint(curSegment.Start, curSegment.End, p, out float dis);
            if (dis < disSquared)
            {
                index = i;
                closestSegment = curSegment;
                closestPoint = cp;
                disSquared = dis;
            }
        }

        return new(closestPoint, closestSegment.Normal);
    }

    /// <summary>
    /// Gets the closest vertex on the segments to a given point.
    /// </summary>
    /// <param name="p">The point to find the closest vertex to.</param>
    /// <param name="disSquared">The squared distance between the point and the closest vertex.</param>
    /// <param name="index">The index of the segment that contains the closest vertex.</param>
    /// <returns>The closest vertex to the given point. Returns a new Vector2() if there are 2 or less segments.</returns>
    public Vector2 GetClosestVertex(Vector2 p, out float disSquared, out int index)
    {
        disSquared = -1;
        index = -1;
        if (Count <= 2) return new();

        index = 0;
        var segment = this[index];
        var closest = segment.Start;
        disSquared = (segment.Start - p).LengthSquared();

        var dis = (segment.End - p).LengthSquared();
        if (dis < disSquared)
        {
            disSquared = dis;
            closest = segment.End;
        }

        for (var i = 1; i < Count; i++)
        {
            segment = this[i];
            dis = (segment.Start - p).LengthSquared();
            if (dis < disSquared)
            {
                index = i;
                closest = segment.Start;
                disSquared = dis;
            }

            dis = (segment.End - p).LengthSquared();
            if (dis < disSquared)
            {
                index = i;
                closest = segment.End;
                disSquared = dis;
            }
        }

        return closest;
    }

    /// <summary>
    /// Finds the closest point on the segments to any collider in the given collision object.
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
    /// Finds the closest point on the segments to the given collider.
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
    /// Gets the closest point on the segments to a given line.
    /// </summary>
    /// <param name="other">The line to find the closest point to.</param>
    /// <returns>A ClosestPointResult containing the closest points on the segments and the line,
    /// the squared distance between them, and the index of the segment that contains the closest point.
    /// Returns a new ClosestPointResult() if there are 2 or less segments.</returns>
    public ClosestPointResult GetClosestPoint(Line other)
    {
        if (Count <= 2) return new();
        var closestSegment = this[0];
        var result = Segment.GetClosestPointSegmentLine(closestSegment.Start, closestSegment.End, other.Point, other.Direction, out float disSquared);
        var selfIndex = 0;

        for (var i = 1; i < Count; i++)
        {
            var curSegment = this[i];

            var cp = Segment.GetClosestPointSegmentLine(curSegment.Start, curSegment.End, other.Point, other.Direction, out float dis);
            if (dis < disSquared)
            {
                selfIndex = i;
                result = cp;
                disSquared = dis;
                closestSegment = curSegment;
            }
        }

        return new(
            new(result.self, closestSegment.Normal),
            new(result.other, other.Normal),
            disSquared,
            selfIndex);
    }

    /// <summary>
    /// Gets the closest point on the segments to a given ray.
    /// </summary>
    /// <param name="other">The ray to find the closest point to.</param>
    /// <returns>A ClosestPointResult containing the closest points on the segments and the ray,
    /// the squared distance between them, and the index of the segment that contains the closest point.
    /// Returns a new ClosestPointResult() if there are 2 or less segments.</returns>
    public ClosestPointResult GetClosestPoint(Ray other)
    {
        if (Count <= 2) return new();

        var closestSegment = this[0];
        var result = Segment.GetClosestPointSegmentRay(closestSegment.Start, closestSegment.End, other.Point, other.Direction, out float disSquared);
        var selfIndex = 0;

        for (var i = 1; i < Count; i++)
        {
            var segment = this[i];

            var cp = Segment.GetClosestPointSegmentRay(segment.Start, segment.End, other.Point, other.Direction, out float dis);
            if (dis < disSquared)
            {
                selfIndex = i;
                result = cp;
                disSquared = dis;
                closestSegment = segment;
            }
        }

        return new(
            new(result.self, closestSegment.Normal),
            new(result.other, other.Normal),
            disSquared,
            selfIndex);
    }

    /// <summary>
    /// Gets the closest point on the segments to a given segment.
    /// </summary>
    /// <param name="other">The segment to find the closest point to.</param>
    /// <returns>A ClosestPointResult containing the closest points on the segments and the other segment,
    /// the squared distance between them, and the index of the segment that contains the closest point.
    /// Returns a new ClosestPointResult() if there are 2 or less segments.</returns>
    public ClosestPointResult GetClosestPoint(Segment other)
    {
        if (Count <= 2) return new();

        var closestSegment = this[0];
        var result = Segment.GetClosestPointSegmentSegment(closestSegment.Start, closestSegment.End, other.Start, other.End, out float disSquared);
        var selfIndex = 0;

        for (var i = 1; i < Count; i++)
        {
            var segment = this[i];

            var cp = Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.Start, other.End, out float dis);
            if (dis < disSquared)
            {
                selfIndex = i;
                result = cp;
                disSquared = dis;
                closestSegment = segment;
            }
        }

        return new(
            new(result.self, closestSegment.Normal),
            new(result.other, other.Normal),
            disSquared,
            selfIndex);
    }

    /// <summary>
    /// Gets the closest point on the segments to a given circle.
    /// </summary>
    /// <param name="other">The circle to find the closest point to.</param>
    /// <returns>A ClosestPointResult containing the closest points on the segments and the circle,
    /// the squared distance between them, and the index of the segment that contains the closest point.
    /// Returns a new ClosestPointResult() if there are 2 or less segments.</returns>
    public ClosestPointResult GetClosestPoint(Circle other)
    {
        if (Count <= 2) return new();

        var closestSegment = this[0];
        var result = Segment.GetClosestPointSegmentCircle(closestSegment.Start, closestSegment.End, other.Center, other.Radius, out float disSquared);
        var selfIndex = 0;

        for (var i = 1; i < Count; i++)
        {
            var segment = this[i];

            var cp = Segment.GetClosestPointSegmentCircle(segment.Start, segment.End, other.Center, other.Radius, out float dis);
            if (dis < disSquared)
            {
                selfIndex = i;
                result = cp;
                disSquared = dis;
                closestSegment = segment;
            }
        }

        return new(
            new(result.self, closestSegment.Normal),
            new(result.other, (result.other - other.Center).Normalize()),
            disSquared,
            selfIndex
        );
    }

    /// <summary>
    /// Gets the closest point on the segments to a given triangle.
    /// </summary>
    /// <param name="other">The triangle to find the closest point to.</param>
    /// <returns>A ClosestPointResult containing the closest points on the segments and the triangle,
    /// the squared distance between them, and the indices of the segments that contain the closest points.
    /// Returns a new ClosestPointResult() if there are 2 or less segments.</returns>
    public ClosestPointResult GetClosestPoint(Triangle other)
    {
        if (Count <= 2) return new();

        (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
        var selfNormal = Vector2.Zero;
        var otherNormal = Vector2.Zero;
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;

        for (var i = 0; i < Count; i++)
        {
            var segment = this[i];

            var cp = Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.A, other.B, out float dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 0;
                result = cp;
                disSquared = dis;
                selfNormal = segment.Normal;
                otherNormal = other.B - other.A;
            }

            cp = Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.B, other.C, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 1;
                result = cp;
                disSquared = dis;
                selfNormal = segment.Normal;
                otherNormal = other.C - other.B;
            }

            cp = Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.C, other.A, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 2;
                result = cp;
                disSquared = dis;
                selfNormal = segment.Normal;
                otherNormal = other.A - other.C;
            }
        }

        return new(
            new(result.self, selfNormal.GetPerpendicularRight().Normalize()),
            new(result.other, otherNormal.GetPerpendicularRight().Normalize()),
            disSquared,
            selfIndex,
            otherIndex);
    }

    /// <summary>
    /// Gets the closest point on the segments to a given quad.
    /// </summary>
    /// <param name="other">The quad to find the closest point to.</param>
    /// <returns>A ClosestPointResult containing the closest points on the segments and the quad,
    /// the squared distance between them, and the indices of the segments that contain the closest points.
    /// Returns a new ClosestPointResult() if there are 2 or less segments.</returns>
    public ClosestPointResult GetClosestPoint(Quad other)
    {
        if (Count <= 2) return new();

        (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
        var selfNormal = Vector2.Zero;
        var otherNormal = Vector2.Zero;
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;

        for (var i = 0; i < Count; i++)
        {
            var segment = this[i];

            var cp = Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.A, other.B, out float dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 0;
                result = cp;
                disSquared = dis;
                selfNormal = segment.Normal;
                otherNormal = other.B - other.A;
            }

            cp = Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.B, other.C, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 1;
                result = cp;
                disSquared = dis;
                selfNormal = segment.Normal;
                otherNormal = other.C - other.B;
            }

            cp = Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.C, other.D, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 2;
                result = cp;
                disSquared = dis;
                selfNormal = segment.Normal;
                otherNormal = other.D - other.C;
            }

            cp = Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.D, other.A, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 3;
                result = cp;
                disSquared = dis;
                selfNormal = segment.Normal;
                otherNormal = other.A - other.D;
            }
        }

        return new(
            new(result.self, selfNormal.GetPerpendicularRight().Normalize()),
            new(result.other, otherNormal.GetPerpendicularRight().Normalize()),
            disSquared,
            selfIndex,
            otherIndex);
    }

    /// <summary>
    /// Gets the closest point on the segments to a given rectangle.
    /// </summary>
    /// <param name="other">The rectangle to find the closest point to.</param>
    /// <returns>A ClosestPointResult containing the closest points on the segments and the rectangle,
    /// the squared distance between them, and the indices of the segments that contain the closest points.
    /// Returns a new ClosestPointResult() if there are 2 or less segments.</returns>
    public ClosestPointResult GetClosestPoint(Rect other)
    {
        if (Count <= 2) return new();

        (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
        var selfNormal = Vector2.Zero;
        var otherNormal = Vector2.Zero;
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;

        for (var i = 0; i < Count; i++)
        {
            var segment = this[i];

            var cp = Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.A, other.B, out float dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 0;
                result = cp;
                disSquared = dis;
                selfNormal = segment.Normal;
                otherNormal = other.B - other.A;
            }

            cp = Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.B, other.C, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 1;
                result = cp;
                disSquared = dis;
                selfNormal = segment.Normal;
                otherNormal = other.C - other.B;
            }

            cp = Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.C, other.D, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 2;
                result = cp;
                disSquared = dis;
                selfNormal = segment.Normal;
                otherNormal = other.D - other.C;
            }

            cp = Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.D, other.A, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 3;
                result = cp;
                disSquared = dis;
                selfNormal = segment.Normal;
                otherNormal = other.A - other.D;
            }
        }

        return new(
            new(result.self, selfNormal.GetPerpendicularRight().Normalize()),
            new(result.other, otherNormal.GetPerpendicularRight().Normalize()),
            disSquared,
            selfIndex,
            otherIndex);
    }

    /// <summary>
    /// Gets the closest point on the segments to a given polygon.
    /// </summary>
    /// <param name="other">The polygon to find the closest point to.</param>
    /// <returns>A ClosestPointResult containing the closest points on the segments and the polygon,
    /// the squared distance between them, and the indices of the segments that contain the closest points.
    /// Returns a new ClosestPointResult() if there are 2 or less segments.</returns>
    public ClosestPointResult GetClosestPoint(Polygon other)
    {
        if (Count <= 2) return new();

        (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
        var selfNormal = Vector2.Zero;
        var otherNormal = Vector2.Zero;
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;

        for (var i = 0; i < Count; i++)
        {
            var segment = this[i];

            for (var j = 0; j < other.Count; j++)
            {
                var otherP1 = other[j];
                var otherP2 = other[(j + 1) % Count];
                var cp = Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, otherP1, otherP2, out float dis);
                if (dis < disSquared || disSquared < 0)
                {
                    selfIndex = i;
                    otherIndex = j;
                    result = cp;
                    disSquared = dis;
                    selfNormal = segment.Normal;
                    otherNormal = otherP2 - otherP1;
                }
            }
        }

        return new(
            new(result.self, selfNormal.GetPerpendicularRight().Normalize()),
            new(result.other, otherNormal.GetPerpendicularRight().Normalize()),
            disSquared,
            selfIndex,
            otherIndex);
    }

    /// <summary>
    /// Gets the closest point on the segments to a given polyline.
    /// </summary>
    /// <param name="other">The polyline to find the closest point to.</param>
    /// <returns>A ClosestPointResult containing the closest points on the segments and the polyline,
    /// the squared distance between them, and the indices of the segments that contain the closest points.
    /// Returns a new ClosestPointResult() if there are 2 or less segments.</returns>
    public ClosestPointResult GetClosestPoint(Polyline other)
    {
        if (Count <= 2) return new();

        (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
        var selfNormal = Vector2.Zero;
        var otherNormal = Vector2.Zero;
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;

        for (var i = 0; i < Count; i++)
        {
            var segment = this[i];

            for (var j = 0; j < other.Count - 1; j++)
            {
                var otherP1 = other[j];
                var otherP2 = other[j + 1];
                var cp = Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, otherP1, otherP2, out float dis);
                if (dis < disSquared || disSquared < 0)
                {
                    selfIndex = i;
                    otherIndex = j;
                    result = cp;
                    disSquared = dis;
                    selfNormal = segment.Normal;
                    otherNormal = otherP2 - otherP1;
                }
            }
        }

        return new(
            new(result.self, selfNormal.GetPerpendicularRight().Normalize()),
            new(result.other, otherNormal.GetPerpendicularRight().Normalize()),
            disSquared,
            selfIndex,
            otherIndex);
    }

    /// <summary>
    /// Gets the closest point on the segments to a given set of segments.
    /// </summary>
    /// <param name="other">The set of segments to find the closest point to.</param>
    /// <returns>A ClosestPointResult containing the closest points on the segments and the other segments,
    /// the squared distance between them, and the indices of the segments that contain the closest points.
    /// Returns a new ClosestPointResult() if there are 2 or less segments.</returns>
    public ClosestPointResult GetClosestPoint(Segments other)
    {
        if (Count <= 2) return new();

        (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
        var selfNormal = Vector2.Zero;
        var otherNormal = Vector2.Zero;
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;

        for (var i = 0; i < Count; i++)
        {
            var segment = this[i];

            for (var j = 0; j < other.Count; j++)
            {
                var otherSegment = other[j];
                var cp = Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, otherSegment.Start, otherSegment.End, out float dis);
                if (dis < disSquared || disSquared < 0)
                {
                    selfIndex = i;
                    otherIndex = j;
                    result = cp;
                    disSquared = dis;
                    selfNormal = segment.Normal;
                    otherNormal = otherSegment.Normal;
                }
            }
        }

        return new(
            new(result.self, selfNormal.GetPerpendicularRight().Normalize()),
            new(result.other, otherNormal),
            disSquared,
            selfIndex,
            otherIndex);
    }

    /// <summary>
    /// Gets the closest segment and point on the segment to a given point.
    /// </summary>
    /// <param name="p">The point to find the closest segment and point to.</param>
    /// <param name="disSquared">The squared distance between the point and the closest point on the segment.</param>
    /// <returns>A tuple containing the closest segment and the closest point on the segment to the given point.
    /// Returns a new empty segment and intersection point if there are 2 or less segments.</returns>
    public (Segment segment, IntersectionPoint segmentPoint) GetClosestSegment(Vector2 p, out float disSquared)
    {
        disSquared = -1;
        if (Count <= 2) return (new(), new());

        var closestSegment = this[0];
        var closest = Segment.GetClosestPointSegmentPoint(closestSegment.Start, closestSegment.End, p, out disSquared);

        for (var i = 1; i < Count; i++)
        {
            var curSegment = this[i];

            var cp = Segment.GetClosestPointSegmentPoint(curSegment.Start, curSegment.End, p, out float dis);
            if (dis < disSquared)
            {
                closestSegment = curSegment;
                closest = cp;
                disSquared = dis;
            }
        }

        return new(closestSegment, new(closest, closestSegment.Normal));
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