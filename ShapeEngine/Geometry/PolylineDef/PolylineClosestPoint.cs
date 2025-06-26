using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.PolylineDef;

public partial class Polyline
{
    public static Vector2 GetClosestPointPolylinePoint(List<Vector2> points, Vector2 p, out float disSquared)
    {
        disSquared = -1;
        if (points.Count < 2) return new();

        var first = points[0];
        var second = points[1];
        var closest = Segment.GetClosestPointSegmentPoint(first, second, p, out disSquared);

        for (var i = 1; i < points.Count - 1; i++)
        {
            var p1 = points[i];
            var p2 = points[i + 1];

            var cp = Segment.GetClosestPointSegmentPoint(p1, p2, p, out float dis);
            if (dis < disSquared)
            {
                closest = cp;
                disSquared = dis;
            }
        }

        return closest;
    }

    public new CollisionPoint GetClosestPoint(Vector2 p, out float disSquared)
    {
        disSquared = -1;
        if (Count < 2) return new();

        var first = this[0];
        var second = this[1];
        var normal = second - first;
        var closest = Segment.GetClosestPointSegmentPoint(first, second, p, out disSquared);

        for (var i = 1; i < Count - 1; i++)
        {
            var p1 = this[i];
            var p2 = this[i + 1];

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

    public new CollisionPoint GetClosestPoint(Vector2 p, out float disSquared, out int index)
    {
        disSquared = -1;
        index = -1;
        if (Count < 2) return new();

        var first = this[0];
        var second = this[1];
        index = 0;
        var normal = second - first;
        var closest = Segment.GetClosestPointSegmentPoint(first, second, p, out disSquared);

        for (var i = 1; i < Count - 1; i++)
        {
            var p1 = this[i];
            var p2 = this[i + 1];

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

    public Vector2 GetClosestVertex(Vector2 p, out float disSquared, out int index)
    {
        disSquared = -1;
        index = -1;
        if (Count < 2) return new();

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

    public Vector2 GetFurthestVertex(Vector2 p, out float disSquared, out int index)
    {
        disSquared = -1;
        index = -1;
        if (Count < 2) return new();

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

    public new ClosestPointResult GetClosestPoint(Line other)
    {
        if (Count < 2) return new();
        var first = this[0];
        var second = this[1];
        var normal = second - first;
        var result = Segment.GetClosestPointSegmentLine(first, second, other.Point, other.Direction, out float disSquared);
        var selfIndex = 0;

        for (var i = 1; i < Count - 1; i++)
        {
            var p1 = this[i];
            var p2 = this[i + 1];

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

    public new ClosestPointResult GetClosestPoint(Ray other)
    {
        if (Count < 2) return new();

        var first = this[0];
        var second = this[1];
        var normal = second - first;
        var result = Segment.GetClosestPointSegmentRay(first, second, other.Point, other.Direction, out float disSquared);
        var selfIndex = 0;

        for (var i = 1; i < Count - 1; i++)
        {
            var p1 = this[i];
            var p2 = this[i + 1];

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

    public new ClosestPointResult GetClosestPoint(Segment other)
    {
        if (Count < 2) return new();

        var first = this[0];
        var second = this[1];
        var normal = second - first;
        var result = Segment.GetClosestPointSegmentSegment(first, second, other.Start, other.End, out float disSquared);
        var selfIndex = 0;

        for (var i = 1; i < Count - 1; i++)
        {
            var p1 = this[i];
            var p2 = this[i + 1];

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

    public new ClosestPointResult GetClosestPoint(Circle other)
    {
        if (Count < 2) return new();

        var first = this[0];
        var second = this[1];
        var normal = second - first;
        var result = Segment.GetClosestPointSegmentCircle(first, second, other.Center, other.Radius, out float disSquared);
        var selfIndex = 0;

        for (var i = 1; i < Count - 1; i++)
        {
            var p1 = this[i];
            var p2 = this[i + 1];

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

    public new ClosestPointResult GetClosestPoint(Triangle other)
    {
        if (Count < 2) return new();

        (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
        var selfNormal = Vector2.Zero;
        var otherNormal = Vector2.Zero;
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;

        for (var i = 0; i < Count - 1; i++)
        {
            var p1 = this[i];
            var p2 = this[i + 1];

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

    public new ClosestPointResult GetClosestPoint(Quad other)
    {
        if (Count < 2) return new();

        (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
        var selfNormal = Vector2.Zero;
        var otherNormal = Vector2.Zero;
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;

        for (var i = 0; i < Count - 1; i++)
        {
            var p1 = this[i];
            var p2 = this[i + 1];

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

    public new ClosestPointResult GetClosestPoint(Rect other)
    {
        if (Count < 2) return new();

        (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
        var selfNormal = Vector2.Zero;
        var otherNormal = Vector2.Zero;
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;

        for (var i = 0; i < Count - 1; i++)
        {
            var p1 = this[i];
            var p2 = this[i + 1];

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

    public new ClosestPointResult GetClosestPoint(Polygon other)
    {
        if (Count < 2) return new();

        (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
        var selfNormal = Vector2.Zero;
        var otherNormal = Vector2.Zero;
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;

        for (var i = 0; i < Count - 1; i++)
        {
            var p1 = this[i];
            var p2 = this[i + 1];

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

    public new ClosestPointResult GetClosestPoint(Polyline other)
    {
        if (Count < 2) return new();

        (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
        var selfNormal = Vector2.Zero;
        var otherNormal = Vector2.Zero;
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;

        for (var i = 0; i < Count - 1; i++)
        {
            var p1 = this[i];
            var p2 = this[i + 1];

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

    public new ClosestPointResult GetClosestPoint(Segments other)
    {
        if (Count < 2) return new();

        (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
        var selfNormal = Vector2.Zero;
        var otherNormal = Vector2.Zero;
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;

        for (var i = 0; i < Count - 1; i++)
        {
            var p1 = this[i];
            var p2 = this[i + 1];

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

    public (Segment segment, CollisionPoint segmentPoint) GetClosestSegment(Vector2 p, out float disSquared)
    {
        disSquared = -1;
        if (Count < 2) return (new(), new());

        var closestSegment = new Segment(this[0], this[1]);
        var closest = closestSegment.GetClosestPoint(p, out disSquared);

        for (var i = 1; i < Count - 1; i++)
        {
            var p1 = this[i];
            var p2 = this[i + 1];

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

}