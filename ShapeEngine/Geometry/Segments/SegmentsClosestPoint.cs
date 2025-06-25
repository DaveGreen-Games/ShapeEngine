using System.Numerics;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.Segments;

public partial class Segments
{
    #region Closest Point

    public static Vector2 GetClosestPointSegmentsPoint(List<Vector2> points, Vector2 p, out float disSquared)
    {
        disSquared = -1;
        if (points.Count <= 2) return new();

        var first = points[0];
        var second = points[1];
        var closest = Segment.Segment.GetClosestPointSegmentPoint(first, second, p, out disSquared);

        for (var i = 1; i < points.Count; i++)
        {
            var p1 = points[i];
            var p2 = points[(i + 1) % points.Count];

            var cp = Segment.Segment.GetClosestPointSegmentPoint(p1, p2, p, out float dis);
            if (dis < disSquared)
            {
                closest = cp;
                disSquared = dis;
            }
        }

        return closest;
    }

    public CollisionPoint GetClosestPoint(Vector2 p, out float disSquared)
    {
        disSquared = -1;
        if (Count <= 2) return new();

        var closestSegment = this[0];
        var closestPoint = Segment.Segment.GetClosestPointSegmentPoint(closestSegment.Start, closestSegment.End, p, out disSquared);

        for (var i = 1; i < Count; i++)
        {
            var curSegment = this[i];

            var cp = Segment.Segment.GetClosestPointSegmentPoint(curSegment.Start, curSegment.End, p, out float dis);
            if (dis < disSquared)
            {
                closestSegment = curSegment;
                closestPoint = cp;
                disSquared = dis;
            }
        }

        return new(closestPoint, closestSegment.Normal);
    }

    public CollisionPoint GetClosestPoint(Vector2 p, out float disSquared, out int index)
    {
        disSquared = -1;
        index = -1;
        if (Count <= 2) return new();

        var closestSegment = this[0];
        var closestPoint = Segment.Segment.GetClosestPointSegmentPoint(closestSegment.Start, closestSegment.End, p, out disSquared);
        index = 0;
        for (var i = 1; i < Count; i++)
        {
            var curSegment = this[i];

            var cp = Segment.Segment.GetClosestPointSegmentPoint(curSegment.Start, curSegment.End, p, out float dis);
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

    public ClosestPointResult GetClosestPoint(Line.Line other)
    {
        if (Count <= 2) return new();
        var closestSegment = this[0];
        var result = Segment.Segment.GetClosestPointSegmentLine(closestSegment.Start, closestSegment.End, other.Point, other.Direction, out float disSquared);
        var selfIndex = 0;

        for (var i = 1; i < Count; i++)
        {
            var curSegment = this[i];

            var cp = Segment.Segment.GetClosestPointSegmentLine(curSegment.Start, curSegment.End, other.Point, other.Direction, out float dis);
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

    public ClosestPointResult GetClosestPoint(Ray.Ray other)
    {
        if (Count <= 2) return new();

        var closestSegment = this[0];
        var result = Segment.Segment.GetClosestPointSegmentRay(closestSegment.Start, closestSegment.End, other.Point, other.Direction, out float disSquared);
        var selfIndex = 0;

        for (var i = 1; i < Count; i++)
        {
            var segment = this[i];

            var cp = Segment.Segment.GetClosestPointSegmentRay(segment.Start, segment.End, other.Point, other.Direction, out float dis);
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

    public ClosestPointResult GetClosestPoint(Segment.Segment other)
    {
        if (Count <= 2) return new();

        var closestSegment = this[0];
        var result = Segment.Segment.GetClosestPointSegmentSegment(closestSegment.Start, closestSegment.End, other.Start, other.End, out float disSquared);
        var selfIndex = 0;

        for (var i = 1; i < Count; i++)
        {
            var segment = this[i];

            var cp = Segment.Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.Start, other.End, out float dis);
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

    public ClosestPointResult GetClosestPoint(Circle.Circle other)
    {
        if (Count <= 2) return new();

        var closestSegment = this[0];
        var result = Segment.Segment.GetClosestPointSegmentCircle(closestSegment.Start, closestSegment.End, other.Center, other.Radius, out float disSquared);
        var selfIndex = 0;

        for (var i = 1; i < Count; i++)
        {
            var segment = this[i];

            var cp = Segment.Segment.GetClosestPointSegmentCircle(segment.Start, segment.End, other.Center, other.Radius, out float dis);
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

    public ClosestPointResult GetClosestPoint(Triangle.Triangle other)
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

            var cp = Segment.Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.A, other.B, out float dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 0;
                result = cp;
                disSquared = dis;
                selfNormal = segment.Normal;
                otherNormal = other.B - other.A;
            }

            cp = Segment.Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.B, other.C, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 1;
                result = cp;
                disSquared = dis;
                selfNormal = segment.Normal;
                otherNormal = other.C - other.B;
            }

            cp = Segment.Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.C, other.A, out dis);
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

    public ClosestPointResult GetClosestPoint(Quad.Quad other)
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

            var cp = Segment.Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.A, other.B, out float dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 0;
                result = cp;
                disSquared = dis;
                selfNormal = segment.Normal;
                otherNormal = other.B - other.A;
            }

            cp = Segment.Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.B, other.C, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 1;
                result = cp;
                disSquared = dis;
                selfNormal = segment.Normal;
                otherNormal = other.C - other.B;
            }

            cp = Segment.Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.C, other.D, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 2;
                result = cp;
                disSquared = dis;
                selfNormal = segment.Normal;
                otherNormal = other.D - other.C;
            }

            cp = Segment.Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.D, other.A, out dis);
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

    public ClosestPointResult GetClosestPoint(Rect.Rect other)
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

            var cp = Segment.Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.A, other.B, out float dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 0;
                result = cp;
                disSquared = dis;
                selfNormal = segment.Normal;
                otherNormal = other.B - other.A;
            }

            cp = Segment.Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.B, other.C, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 1;
                result = cp;
                disSquared = dis;
                selfNormal = segment.Normal;
                otherNormal = other.C - other.B;
            }

            cp = Segment.Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.C, other.D, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 2;
                result = cp;
                disSquared = dis;
                selfNormal = segment.Normal;
                otherNormal = other.D - other.C;
            }

            cp = Segment.Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, other.D, other.A, out dis);
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

    public ClosestPointResult GetClosestPoint(Polygon.Polygon other)
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
                var cp = Segment.Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, otherP1, otherP2, out float dis);
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

    public ClosestPointResult GetClosestPoint(Polyline.Polyline other)
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
                var cp = Segment.Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, otherP1, otherP2, out float dis);
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
                var cp = Segment.Segment.GetClosestPointSegmentSegment(segment.Start, segment.End, otherSegment.Start, otherSegment.End, out float dis);
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

    public (Segment.Segment segment, CollisionPoint segmentPoint) GetClosestSegment(Vector2 p, out float disSquared)
    {
        disSquared = -1;
        if (Count <= 2) return (new(), new());

        var closestSegment = this[0];
        var closest = Segment.Segment.GetClosestPointSegmentPoint(closestSegment.Start, closestSegment.End, p, out disSquared);

        for (var i = 1; i < Count; i++)
        {
            var curSegment = this[i];

            var cp = Segment.Segment.GetClosestPointSegmentPoint(curSegment.Start, curSegment.End, p, out float dis);
            if (dis < disSquared)
            {
                closestSegment = curSegment;
                closest = cp;
                disSquared = dis;
            }
        }

        return new(closestSegment, new(closest, closestSegment.Normal));
    }

    #endregion
}