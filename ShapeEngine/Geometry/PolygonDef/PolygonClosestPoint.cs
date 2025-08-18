using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.PolygonDef;


public partial class Polygon
{
    /// <summary>
    /// Finds the closest point on a polygon (defined by a list of points) to a given point.
    /// </summary>
    /// <param name="points">The polygon points.</param>
    /// <param name="p">The point to test.</param>
    /// <param name="disSquared">The squared distance to the closest point (output).</param>
    /// <returns>The closest point on the polygon.</returns>
    public static Vector2 GetClosestPointPolygonPoint(List<Vector2> points, Vector2 p, out float disSquared)
    {
        disSquared = -1;
        if (points.Count < 3) return new(); // Polygon must have at least 3 points

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
    /// Finds the closest point on this polygon to a given point.
    /// </summary>
    /// <param name="p">The point to test.</param>
    /// <param name="disSquared">The squared distance to the closest point (output).</param>
    /// <returns>The closest point and its normal as a <see cref="IntersectionPoint"/>.</returns>
    public new IntersectionPoint GetClosestPoint(Vector2 p, out float disSquared)
    {
        disSquared = -1;
        if (Count < 3) return new(); // Polygon must have at least 3 points

        var first = this[0];
        var second = this[1];
        var normal = second - first;
        var closest = Segment.GetClosestPointSegmentPoint(first, second, p, out disSquared);

        for (var i = 1; i < Count; i++)
        {
            var p1 = this[i];
            var p2 = this[(i + 1) % Count];

            var cp = Segment.GetClosestPointSegmentPoint(p1, p2, p, out float dis);
            if (dis < disSquared)
            {
                closest = cp;
                disSquared = dis;
                normal = p2 - p1;
            }
        }

        return new(closest, normal.GetPerpendicularRight().Normalize());
    }
    /// <summary>
    /// Finds the closest point on this polygon to a given point,
    /// and returns the index of the closest edge.
    /// </summary>
    /// <param name="p">The point to test.</param>
    /// <param name="disSquared">The squared distance to the closest point (output).</param>
    /// <param name="index">The index of the closest edge (output).</param>
    /// <returns>The closest point and its normal as a <see cref="IntersectionPoint"/>.</returns>
    public new IntersectionPoint GetClosestPoint(Vector2 p, out float disSquared, out int index)
    {
        disSquared = -1;
        index = -1;
        if (Count < 3) return new(); // Polygon must have at least 3 points

        var first = this[0];
        var second = this[1];
        index = 0;
        var normal = second - first;
        var closest = Segment.GetClosestPointSegmentPoint(first, second, p, out disSquared);

        for (var i = 1; i < Count; i++)
        {
            var p1 = this[i];
            var p2 = this[(i + 1) % Count];

            var cp = Segment.GetClosestPointSegmentPoint(p1, p2, p, out float dis);
            if (dis < disSquared)
            {
                index = i;
                normal = p2 - p1;
                closest = cp;
                disSquared = dis;
            }
        }

        return new(closest, normal.GetPerpendicularRight().Normalize());
    }
    /// <summary>
    /// Finds the closest vertex of this polygon to a given point.
    /// </summary>
    /// <param name="p">The point to test.</param>
    /// <param name="disSquared">The squared distance to the closest vertex (output).</param>
    /// <param name="index">The index of the closest vertex (output).</param>
    /// <returns>The closest vertex.</returns>
    public Vector2 GetClosestVertex(Vector2 p, out float disSquared, out int index)
    {
        disSquared = -1;
        index = -1;
        if (Count <= 2) return new();

        index = 0;
        var closest = this[index];
        disSquared = (closest - p).LengthSquared();

        for (var i = 1; i < Count; i++)
        {
            var cp = this[i];
            var dis = (cp - p).LengthSquared();
            if (dis < disSquared)
            {
                index = i;
                closest = cp;
                disSquared = dis;
            }
        }

        return closest;
    }
    /// <summary>
    /// Finds the furthest vertex of this polygon from a given point.
    /// </summary>
    /// <param name="p">The point to test.</param>
    /// <param name="disSquared">The squared distance to the furthest vertex (output).</param>
    /// <param name="index">The index of the furthest vertex (output).</param>
    /// <returns>The furthest vertex.</returns>
    public Vector2 GetFurthestVertex(Vector2 p, out float disSquared, out int index)
    {
        disSquared = -1;
        index = -1;
        if (Count <= 2) return new();

        index = 0;
        var furthest = this[index];
        disSquared = (furthest - p).LengthSquared();

        for (var i = 1; i < Count; i++)
        {
            var cp = this[i];
            var dis = (cp - p).LengthSquared();
            if (dis > disSquared)
            {
                index = i;
                furthest = cp;
                disSquared = dis;
            }
        }

        return furthest;
    }
    /// <summary>
    /// Finds the closest point between this polygon and a line.
    /// </summary>
    /// <param name="other">The line to test against.</param>
    /// <returns>The closest point result.</returns>
    public new ClosestPointResult GetClosestPoint(Line other)
    {
        if (Count <= 2) return new();
        var first = this[0];
        var second = this[1];
        var normal = second - first;
        var result = Segment.GetClosestPointSegmentLine(first, second, other.Point, other.Direction, out float disSquared);
        var selfIndex = 0;

        for (var i = 1; i < Count; i++)
        {
            var p1 = this[i];
            var p2 = this[(i + 1) % Count];

            var cp = Segment.GetClosestPointSegmentLine(p1, p2, other.Point, other.Direction, out float dis);
            if (dis < disSquared)
            {
                selfIndex = i;
                result = cp;
                disSquared = dis;
                normal = p2 - p1;
            }
        }

        return new(
            new(result.self, normal.GetPerpendicularRight().Normalize()),
            new(result.other, other.Normal),
            disSquared,
            selfIndex);
    }
    /// <summary>
    /// Finds the closest point between this polygon and a ray.
    /// </summary>
    /// <param name="other">The ray to test against.</param>
    /// <returns>The closest point result.</returns>
    public new ClosestPointResult GetClosestPoint(Ray other)
    {
        if (Count <= 2) return new();

        var first = this[0];
        var second = this[1];
        var normal = second - first;
        var result = Segment.GetClosestPointSegmentRay(first, second, other.Point, other.Direction, out float disSquared);
        var selfIndex = 0;

        for (var i = 1; i < Count; i++)
        {
            var p1 = this[i];
            var p2 = this[(i + 1) % Count];

            var cp = Segment.GetClosestPointSegmentRay(p1, p2, other.Point, other.Direction, out float dis);
            if (dis < disSquared)
            {
                selfIndex = i;
                result = cp;
                disSquared = dis;
                normal = p2 - p1;
            }
        }

        return new(
            new(result.self, normal.GetPerpendicularRight().Normalize()),
            new(result.other, other.Normal),
            disSquared,
            selfIndex);
    }
    /// <summary>
    /// Finds the closest point between this polygon and a segment.
    /// </summary>
    /// <param name="other">The segment to test against.</param>
    /// <returns>The closest point result.</returns>
    public new ClosestPointResult GetClosestPoint(Segment other)
    {
        if (Count <= 2) return new();

        var first = this[0];
        var second = this[1];
        var normal = second - first;
        var result = Segment.GetClosestPointSegmentSegment(first, second, other.Start, other.End, out float disSquared);
        var selfIndex = 0;

        for (var i = 1; i < Count; i++)
        {
            var p1 = this[i];
            var p2 = this[(i + 1) % Count];

            var cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.Start, other.End, out float dis);
            if (dis < disSquared)
            {
                selfIndex = i;
                result = cp;
                disSquared = dis;
                normal = p2 - p1;
            }
        }

        return new(
            new(result.self, normal.GetPerpendicularRight().Normalize()),
            new(result.other, other.Normal),
            disSquared,
            selfIndex);
    }
    /// <summary>
    /// Finds the closest point between this polygon and a circle.
    /// </summary>
    /// <param name="other">The circle to test against.</param>
    /// <returns>The closest point result.</returns>
    public new ClosestPointResult GetClosestPoint(Circle other)
    {
        if (Count <= 2) return new();

        var first = this[0];
        var second = this[1];
        var normal = second - first;
        var result = Segment.GetClosestPointSegmentCircle(first, second, other.Center, other.Radius, out float disSquared);
        var selfIndex = 0;

        for (var i = 1; i < Count; i++)
        {
            var p1 = this[i];
            var p2 = this[(i + 1) % Count];

            var cp = Segment.GetClosestPointSegmentCircle(p1, p2, other.Center, other.Radius, out float dis);
            if (dis < disSquared)
            {
                selfIndex = i;
                result = cp;
                disSquared = dis;
                normal = p2 - p1;
            }
        }

        return new(
            new(result.self, normal.GetPerpendicularRight().Normalize()),
            new(result.other, (result.other - other.Center).Normalize()),
            disSquared,
            selfIndex
        );
    }
    /// <summary>
    /// Finds the closest point between this polygon and a triangle.
    /// </summary>
    /// <param name="other">The triangle to test against.</param>
    /// <returns>The closest point result.</returns>
    public new ClosestPointResult GetClosestPoint(Triangle other)
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
            var p1 = this[i];
            var p2 = this[(i + 1) % Count];

            var cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.A, other.B, out float dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 0;
                result = cp;
                disSquared = dis;
                selfNormal = p2 - p1;
                otherNormal = other.B - other.A;
            }

            cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.B, other.C, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 1;
                result = cp;
                disSquared = dis;
                selfNormal = p2 - p1;
                otherNormal = other.C - other.B;
            }

            cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.C, other.A, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 2;
                result = cp;
                disSquared = dis;
                selfNormal = p2 - p1;
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
    /// Finds the closest point between this polygon and a quad.
    /// </summary>
    /// <param name="other">The quad to test against.</param>
    /// <returns>The closest point result.</returns>
    public new ClosestPointResult GetClosestPoint(Quad other)
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
            var p1 = this[i];
            var p2 = this[(i + 1) % Count];

            var cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.A, other.B, out float dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 0;
                result = cp;
                disSquared = dis;
                selfNormal = p2 - p1;
                otherNormal = other.B - other.A;
            }

            cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.B, other.C, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 1;
                result = cp;
                disSquared = dis;
                selfNormal = p2 - p1;
                otherNormal = other.C - other.B;
            }

            cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.C, other.D, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 2;
                result = cp;
                disSquared = dis;
                selfNormal = p2 - p1;
                otherNormal = other.D - other.C;
            }

            cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.D, other.A, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 3;
                result = cp;
                disSquared = dis;
                selfNormal = p2 - p1;
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
    /// Finds the closest point between this polygon and a rectangle.
    /// </summary>
    /// <param name="other">The rectangle to test against.</param>
    /// <returns>The closest point result.</returns>
    public new ClosestPointResult GetClosestPoint(Rect other)
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
            var p1 = this[i];
            var p2 = this[(i + 1) % Count];

            var cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.A, other.B, out float dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 0;
                result = cp;
                disSquared = dis;
                selfNormal = p2 - p1;
                otherNormal = other.B - other.A;
            }

            cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.B, other.C, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 1;
                result = cp;
                disSquared = dis;
                selfNormal = p2 - p1;
                otherNormal = other.C - other.B;
            }

            cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.C, other.D, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 2;
                result = cp;
                disSquared = dis;
                selfNormal = p2 - p1;
                otherNormal = other.D - other.C;
            }

            cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.D, other.A, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 3;
                result = cp;
                disSquared = dis;
                selfNormal = p2 - p1;
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
    /// Finds the closest point between this polygon and another polygon.
    /// </summary>
    /// <param name="other">The other polygon to test against.</param>
    /// <returns>The closest point result.</returns>
    public new ClosestPointResult GetClosestPoint(Polygon other)
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
            var p1 = this[i];
            var p2 = this[(i + 1) % Count];

            for (var j = 0; j < other.Count; j++)
            {
                var otherP1 = other[j];
                var otherP2 = other[(j + 1) % other.Count];
                var cp = Segment.GetClosestPointSegmentSegment(p1, p2, otherP1, otherP2, out float dis);
                if (dis < disSquared || disSquared < 0)
                {
                    selfIndex = i;
                    otherIndex = j;
                    result = cp;
                    disSquared = dis;
                    selfNormal = p2 - p1;
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
    /// Finds the closest point between this polygon and a polyline.
    /// </summary>
    /// <param name="other">The polyline to test against.</param>
    /// <returns>The closest point result.</returns>
    public new ClosestPointResult GetClosestPoint(Polyline other)
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
            var p1 = this[i];
            var p2 = this[(i + 1) % Count];

            for (var j = 0; j < other.Count - 1; j++)
            {
                var otherP1 = other[j];
                var otherP2 = other[j + 1];
                var cp = Segment.GetClosestPointSegmentSegment(p1, p2, otherP1, otherP2, out float dis);
                if (dis < disSquared || disSquared < 0)
                {
                    selfIndex = i;
                    otherIndex = j;
                    result = cp;
                    disSquared = dis;
                    selfNormal = p2 - p1;
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
    /// Finds the closest point between this polygon and a set of segments.
    /// </summary>
    /// <param name="other">The segments to test against.</param>
    /// <returns>The closest point result.</returns>
    public new ClosestPointResult GetClosestPoint(Segments other)
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
            var p1 = this[i];
            var p2 = this[(i + 1) % Count];

            for (var j = 0; j < other.Count; j++)
            {
                var otherSegment = other[j];
                var cp = Segment.GetClosestPointSegmentSegment(p1, p2, otherSegment.Start, otherSegment.End, out float dis);
                if (dis < disSquared || disSquared < 0)
                {
                    selfIndex = i;
                    otherIndex = j;
                    result = cp;
                    disSquared = dis;
                    selfNormal = p2 - p1;
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
    /// Finds the closest segment of this polygon to a given point.
    /// </summary>
    /// <param name="p">The point to test.</param>
    /// <param name="disSquared">The squared distance to the closest segment (output).</param>
    /// <returns>The closest segment and its closest point as a <see cref="IntersectionPoint"/>.</returns>
    public (Segment segment, IntersectionPoint segmentPoint) GetClosestSegment(Vector2 p, out float disSquared)
    {
        disSquared = -1;
        if (Count <= 2) return (new(), new());

        var closestSegment = new Segment(this[0], this[1]);
        var closest = closestSegment.GetClosestPoint(p, out disSquared);

        for (var i = 1; i < Count; i++)
        {
            var p1 = this[i];
            var p2 = this[(i + 1) % Count];

            var cp = Segment.GetClosestPointSegmentPoint(p1, p2, p, out float dis);
            if (dis < disSquared)
            {
                var normal = (p2 - p1).GetPerpendicularRight().Normalize();
                closest = new(cp, normal);
                closestSegment = new Segment(p1, p2);
                disSquared = dis;
            }
        }

        return new(closestSegment, closest);
    }
    
    /// <summary>
    /// Finds the closest point on this shapes perimeter to the given <see cref="IShape"/>.
    /// </summary>
    /// <param name="shape">The shape to compare against.</param>
    /// <returns>
    /// A <see cref="ClosestPointResult"/> containing the closest point information for the shape.
    /// </returns>
    public new ClosestPointResult GetClosestPoint(IShape shape)
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
