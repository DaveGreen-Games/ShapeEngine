using System.Numerics;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Geometry.Segment;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.Triangle;

public readonly partial struct Triangle
{
    public static Vector2 GetClosestPointTrianglePoint(Vector2 a, Vector2 b, Vector2 c, Vector2 p, out float disSquared)
    {
        var min = Segment.Segment.GetClosestPointSegmentPoint(a, b, p, out disSquared);

        var cp = Segment.Segment.GetClosestPointSegmentPoint(b, c, p, out float dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
        }

        cp = Segment.Segment.GetClosestPointSegmentPoint(c, a, p, out dis);
        if (dis < disSquared)
        {
            disSquared = dis;
            return cp;
        }

        return min;
    }

    public CollisionPoint GetClosestPoint(Vector2 p, out float disSquared)
    {
        var min = Segment.Segment.GetClosestPointSegmentPoint(A, B, p, out disSquared);
        var normal = B - A;

        var cp = Segment.Segment.GetClosestPointSegmentPoint(B, C, p, out float dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            normal = C - B;
        }

        cp = Segment.Segment.GetClosestPointSegmentPoint(C, A, p, out dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            normal = A - C;
        }

        return new(min, normal.GetPerpendicularRight().Normalize());
    }

    public CollisionPoint GetClosestPoint(Vector2 p, out float disSquared, out int index)
    {
        var min = Segment.Segment.GetClosestPointSegmentPoint(A, B, p, out disSquared);
        var normal = B - A;
        index = 0;

        var cp = Segment.Segment.GetClosestPointSegmentPoint(B, C, p, out float dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            normal = C - B;
            index = 1;
        }

        cp = Segment.Segment.GetClosestPointSegmentPoint(C, A, p, out dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            normal = A - C;
            index = 2;
        }

        return new(min, normal.GetPerpendicularRight().Normalize());
    }

    public ClosestPointResult GetClosestPoint(Line.Line other)
    {
        var closestResult = Segment.Segment.GetClosestPointSegmentLine(A, B, other.Point, other.Direction, out float disSquared);
        var curNormal = B - A;
        var index = 0;

        var result = Segment.Segment.GetClosestPointSegmentLine(B, C, other.Point, other.Direction, out float dis);
        if (dis < disSquared)
        {
            index = 1;
            closestResult = result;
            disSquared = dis;
            curNormal = C - B;
        }

        result = Segment.Segment.GetClosestPointSegmentLine(C, A, other.Point, other.Direction, out dis);
        if (dis < disSquared)
        {
            index = 2;
            closestResult = result;
            disSquared = dis;
            curNormal = A - C;
        }

        return new(
            new(closestResult.self, curNormal.GetPerpendicularRight().Normalize()),
            new(closestResult.other, other.Normal),
            disSquared,
            index);
    }

    public ClosestPointResult GetClosestPoint(Ray.Ray other)
    {
        var closestResult = Segment.Segment.GetClosestPointSegmentRay(A, B, other.Point, other.Direction, out float disSquared);
        var curNormal = B - A;
        var index = 0;

        var result = Segment.Segment.GetClosestPointSegmentRay(B, C, other.Point, other.Direction, out float dis);
        if (dis < disSquared)
        {
            index = 1;
            closestResult = result;
            disSquared = dis;
            curNormal = C - B;
        }

        result = Segment.Segment.GetClosestPointSegmentRay(C, A, other.Point, other.Direction, out dis);
        if (dis < disSquared)
        {
            index = 2;
            closestResult = result;
            disSquared = dis;
            curNormal = A - C;
        }

        return new(
            new(closestResult.self, curNormal.GetPerpendicularRight().Normalize()),
            new(closestResult.other, other.Normal),
            disSquared,
            index);
    }

    public ClosestPointResult GetClosestPoint(Segment.Segment other)
    {
        var closestResult = Segment.Segment.GetClosestPointSegmentSegment(A, B, other.Start, other.End, out float disSquared);
        var curNormal = B - A;
        var index = 0;

        var result = Segment.Segment.GetClosestPointSegmentSegment(B, C, other.Start, other.End, out float dis);
        if (dis < disSquared)
        {
            index = 1;
            closestResult = result;
            disSquared = dis;
            curNormal = C - B;
        }

        result = Segment.Segment.GetClosestPointSegmentSegment(C, A, other.Start, other.End, out dis);
        if (dis < disSquared)
        {
            index = 2;
            closestResult = result;
            disSquared = dis;
            curNormal = A - C;
        }

        return new(
            new(closestResult.self, curNormal.GetPerpendicularRight().Normalize()),
            new(closestResult.other, other.Normal),
            disSquared,
            index);
    }

    public ClosestPointResult GetClosestPoint(Circle.Circle other)
    {
        var closestResult = Segment.Segment.GetClosestPointSegmentCircle(A, B, other.Center, other.Radius, out float disSquared);
        var curNormal = B - A;
        var index = 0;
        var result = Segment.Segment.GetClosestPointSegmentCircle(B, C, other.Center, other.Radius, out float dis);
        if (dis < disSquared)
        {
            index = 1;
            closestResult = result;
            disSquared = dis;
            curNormal = C - B;
        }

        result = Segment.Segment.GetClosestPointSegmentCircle(C, A, other.Center, other.Radius, out dis);
        if (dis < disSquared)
        {
            index = 2;
            closestResult = result;
            disSquared = dis;
            curNormal = A - C;
        }

        return new(
            new(closestResult.self, curNormal.GetPerpendicularRight().Normalize()),
            new(closestResult.other, (closestResult.other - other.Center).Normalize()),
            disSquared,
            index);
    }

    public ClosestPointResult GetClosestPoint(Triangle other)
    {
        var closestResult = Segment.Segment.GetClosestPointSegmentSegment(A, B, other.A, other.B, out float disSquared);
        var curSelfNormal = B - A;
        var curOtherNormal = B - A;
        var selfIndex = 0;
        var otherIndex = 0;

        var result = Segment.Segment.GetClosestPointSegmentSegment(A, B, other.B, other.C, out float dis);
        if (dis < disSquared)
        {
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.C - other.B;
        }

        result = Segment.Segment.GetClosestPointSegmentSegment(A, B, other.C, other.A, out dis);
        if (dis < disSquared)
        {
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.A - other.C;
        }

        result = Segment.Segment.GetClosestPointSegmentSegment(B, C, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.B - other.A;
        }

        result = Segment.Segment.GetClosestPointSegmentSegment(B, C, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.C - other.B;
        }

        result = Segment.Segment.GetClosestPointSegmentSegment(B, C, other.C, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.A - other.C;
        }

        result = Segment.Segment.GetClosestPointSegmentSegment(C, A, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.B - other.A;
        }

        result = Segment.Segment.GetClosestPointSegmentSegment(C, A, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.C - other.B;
        }

        result = Segment.Segment.GetClosestPointSegmentSegment(C, A, other.C, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.A - other.C;
        }

        return new(
            new(closestResult.self, curSelfNormal.GetPerpendicularRight().Normalize()),
            new(closestResult.other, curOtherNormal.GetPerpendicularRight().Normalize()),
            disSquared,
            selfIndex,
            otherIndex);
    }

    public ClosestPointResult GetClosestPoint(Quad.Quad other)
    {
        var closestResult = Segment.Segment.GetClosestPointSegmentSegment(A, B, other.A, other.B, out float disSquared);
        var curSelfNormal = B - A;
        var curOtherNormal = B - A;
        var selfIndex = 0;
        var otherIndex = 0;
        var result = Segment.Segment.GetClosestPointSegmentSegment(A, B, other.B, other.C, out float dis);
        if (dis < disSquared)
        {
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.C - other.B;
        }

        result = Segment.Segment.GetClosestPointSegmentSegment(A, B, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.D - other.C;
        }

        result = Segment.Segment.GetClosestPointSegmentSegment(A, B, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.A - other.D;
        }

        result = Segment.Segment.GetClosestPointSegmentSegment(B, C, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.B - other.A;
        }

        result = Segment.Segment.GetClosestPointSegmentSegment(B, C, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.C - other.B;
        }

        result = Segment.Segment.GetClosestPointSegmentSegment(B, C, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.D - other.C;
        }

        result = Segment.Segment.GetClosestPointSegmentSegment(B, C, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.A - other.D;
        }

        result = Segment.Segment.GetClosestPointSegmentSegment(C, A, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.B - other.A;
        }

        result = Segment.Segment.GetClosestPointSegmentSegment(C, A, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.C - other.B;
        }

        result = Segment.Segment.GetClosestPointSegmentSegment(C, A, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.D - other.C;
        }

        result = Segment.Segment.GetClosestPointSegmentSegment(C, A, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.A - other.D;
        }

        return new(
            new(closestResult.self, curSelfNormal.GetPerpendicularRight().Normalize()),
            new(closestResult.other, curOtherNormal.GetPerpendicularRight().Normalize()),
            disSquared,
            selfIndex,
            otherIndex);
    }

    public ClosestPointResult GetClosestPoint(Rect.Rect other)
    {
        var closestResult = Segment.Segment.GetClosestPointSegmentSegment(A, B, other.A, other.B, out float disSquared);
        var curSelfNormal = B - A;
        var curOtherNormal = B - A;
        var selfIndex = 0;
        var otherIndex = 0;
        var result = Segment.Segment.GetClosestPointSegmentSegment(A, B, other.B, other.C, out float dis);
        if (dis < disSquared)
        {
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.C - other.B;
        }

        result = Segment.Segment.GetClosestPointSegmentSegment(A, B, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.D - other.C;
        }

        result = Segment.Segment.GetClosestPointSegmentSegment(A, B, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.A - other.D;
        }

        result = Segment.Segment.GetClosestPointSegmentSegment(B, C, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.B - other.A;
        }

        result = Segment.Segment.GetClosestPointSegmentSegment(B, C, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.C - other.B;
        }

        result = Segment.Segment.GetClosestPointSegmentSegment(B, C, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.D - other.C;
        }

        result = Segment.Segment.GetClosestPointSegmentSegment(B, C, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.A - other.D;
        }

        result = Segment.Segment.GetClosestPointSegmentSegment(C, A, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.B - other.A;
        }

        result = Segment.Segment.GetClosestPointSegmentSegment(C, A, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.C - other.B;
        }

        result = Segment.Segment.GetClosestPointSegmentSegment(C, A, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.D - other.C;
        }

        result = Segment.Segment.GetClosestPointSegmentSegment(C, A, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.A - other.D;
        }

        return new(
            new(closestResult.self, curSelfNormal.GetPerpendicularRight().Normalize()),
            new(closestResult.other, curOtherNormal.GetPerpendicularRight().Normalize()),
            disSquared,
            selfIndex,
            otherIndex);
    }

    public ClosestPointResult GetClosestPoint(Polygon.Polygon other)
    {
        if (other.Count < 3) return new();

        (Vector2 self, Vector2 other) closestResult = (Vector2.Zero, Vector2.Zero);
        var curSelfNormal = Vector2.Zero;
        var curOtherNormal = Vector2.Zero;
        var selfIndex = -1;
        var otherIndex = -1;
        var disSquared = -1f;

        for (var i = 0; i < other.Count; i++)
        {
            var p1 = other[i];
            var p2 = other[(i + 1) % other.Count];

            var result = Segment.Segment.GetClosestPointSegmentSegment(A, B, p1, p2, out float dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 0;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = B - A;
            }

            result = Segment.Segment.GetClosestPointSegmentSegment(B, C, p1, p2, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 1;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = C - B;
            }

            result = Segment.Segment.GetClosestPointSegmentSegment(C, A, p1, p2, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 2;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = A - C;
            }
        }

        return new(
            new(closestResult.self, curSelfNormal.GetPerpendicularRight().Normalize()),
            new(closestResult.other, curOtherNormal.GetPerpendicularRight().Normalize()),
            disSquared,
            selfIndex,
            otherIndex);
    }

    public ClosestPointResult GetClosestPoint(Polyline.Polyline other)
    {
        if (other.Count < 2) return new();

        (Vector2 self, Vector2 other) closestResult = (Vector2.Zero, Vector2.Zero);
        var curSelfNormal = Vector2.Zero;
        var curOtherNormal = Vector2.Zero;
        var selfIndex = -1;
        var otherIndex = -1;
        var disSquared = -1f;

        for (var i = 0; i < other.Count - 1; i++)
        {
            var p1 = other[i];
            var p2 = other[i + 1];

            var result = Segment.Segment.GetClosestPointSegmentSegment(A, B, p1, p2, out float dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 0;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = B - A;
            }

            result = Segment.Segment.GetClosestPointSegmentSegment(B, C, p1, p2, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 1;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = C - B;
            }

            result = Segment.Segment.GetClosestPointSegmentSegment(C, A, p1, p2, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 2;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = A - C;
            }
        }

        return new(
            new(closestResult.self, curSelfNormal.GetPerpendicularRight().Normalize()),
            new(closestResult.other, curOtherNormal.GetPerpendicularRight().Normalize()),
            disSquared,
            selfIndex,
            otherIndex);
    }

    public ClosestPointResult GetClosestPoint(Segments.Segments other)
    {
        if (other.Count <= 0) return new();

        ClosestPointResult closestResult = new();

        for (var i = 0; i < other.Count; i++)
        {
            var segment = other[i];
            var result = GetClosestPoint(segment);

            if (!closestResult.Valid || result.IsCloser(closestResult))
            {
                closestResult = result;
            }
        }

        return closestResult;
    }

    public (Segment.Segment segment, CollisionPoint segmentPoint) GetClosestSegment(Vector2 p, out float disSquared)
    {
        var closestSegment = SegmentAToB;
        var closestResult = closestSegment.GetClosestPoint(p, out disSquared);

        var currentSegment = SegmentBToC;
        var result = currentSegment.GetClosestPoint(p, out float dis);
        if (dis < disSquared)
        {
            closestSegment = currentSegment;
            disSquared = dis;
            closestResult = result;
        }

        currentSegment = SegmentCToA;
        result = currentSegment.GetClosestPoint(p, out dis);
        if (dis < disSquared)
        {
            closestSegment = currentSegment;
            disSquared = dis;
            closestResult = result;
        }

        return (closestSegment, closestResult);
    }

    public Vector2 GetClosestVertex(Vector2 p, out float disSquared, out int index)
    {
        var closest = A;
        disSquared = (A - p).LengthSquared();
        index = 0;
        float l = (B - p).LengthSquared();
        if (l < disSquared)
        {
            closest = B;
            disSquared = l;
            index = 1;
        }

        l = (C - p).LengthSquared();
        if (l < disSquared)
        {
            closest = C;
            disSquared = l;
            index = 2;
        }

        return closest;
    }

    public Vector2 GetFurthestVertex(Vector2 p, out float disSquared, out int index)
    {
        var furthest = A;
        disSquared = (A - p).LengthSquared();
        index = 0;
        float l = (B - p).LengthSquared();
        if (l > disSquared)
        {
            furthest = B;
            disSquared = l;
            index = 1;
        }

        l = (C - p).LengthSquared();
        if (l > disSquared)
        {
            furthest = C;
            disSquared = l;
            index = 2;
        }

        return furthest;
    }

}