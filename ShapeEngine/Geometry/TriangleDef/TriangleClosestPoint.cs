using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.TriangleDef;

public readonly partial struct Triangle
{
    public static Vector2 GetClosestPointTrianglePoint(Vector2 a, Vector2 b, Vector2 c, Vector2 p, out float disSquared)
    {
        var min = SegmentDef.Segment.GetClosestPointSegmentPoint(a, b, p, out disSquared);

        var cp = SegmentDef.Segment.GetClosestPointSegmentPoint(b, c, p, out float dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
        }

        cp = SegmentDef.Segment.GetClosestPointSegmentPoint(c, a, p, out dis);
        if (dis < disSquared)
        {
            disSquared = dis;
            return cp;
        }

        return min;
    }

    public CollisionPoint GetClosestPoint(Vector2 p, out float disSquared)
    {
        var min = SegmentDef.Segment.GetClosestPointSegmentPoint(A, B, p, out disSquared);
        var normal = B - A;

        var cp = SegmentDef.Segment.GetClosestPointSegmentPoint(B, C, p, out float dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            normal = C - B;
        }

        cp = SegmentDef.Segment.GetClosestPointSegmentPoint(C, A, p, out dis);
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
        var min = SegmentDef.Segment.GetClosestPointSegmentPoint(A, B, p, out disSquared);
        var normal = B - A;
        index = 0;

        var cp = SegmentDef.Segment.GetClosestPointSegmentPoint(B, C, p, out float dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            normal = C - B;
            index = 1;
        }

        cp = SegmentDef.Segment.GetClosestPointSegmentPoint(C, A, p, out dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            normal = A - C;
            index = 2;
        }

        return new(min, normal.GetPerpendicularRight().Normalize());
    }

    public ClosestPointResult GetClosestPoint(Line other)
    {
        var closestResult = SegmentDef.Segment.GetClosestPointSegmentLine(A, B, other.Point, other.Direction, out float disSquared);
        var curNormal = B - A;
        var index = 0;

        var result = SegmentDef.Segment.GetClosestPointSegmentLine(B, C, other.Point, other.Direction, out float dis);
        if (dis < disSquared)
        {
            index = 1;
            closestResult = result;
            disSquared = dis;
            curNormal = C - B;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentLine(C, A, other.Point, other.Direction, out dis);
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

    public ClosestPointResult GetClosestPoint(Ray other)
    {
        var closestResult = SegmentDef.Segment.GetClosestPointSegmentRay(A, B, other.Point, other.Direction, out float disSquared);
        var curNormal = B - A;
        var index = 0;

        var result = SegmentDef.Segment.GetClosestPointSegmentRay(B, C, other.Point, other.Direction, out float dis);
        if (dis < disSquared)
        {
            index = 1;
            closestResult = result;
            disSquared = dis;
            curNormal = C - B;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentRay(C, A, other.Point, other.Direction, out dis);
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

    public ClosestPointResult GetClosestPoint(SegmentDef.Segment other)
    {
        var closestResult = SegmentDef.Segment.GetClosestPointSegmentSegment(A, B, other.Start, other.End, out float disSquared);
        var curNormal = B - A;
        var index = 0;

        var result = SegmentDef.Segment.GetClosestPointSegmentSegment(B, C, other.Start, other.End, out float dis);
        if (dis < disSquared)
        {
            index = 1;
            closestResult = result;
            disSquared = dis;
            curNormal = C - B;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(C, A, other.Start, other.End, out dis);
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

    public ClosestPointResult GetClosestPoint(Circle other)
    {
        var closestResult = SegmentDef.Segment.GetClosestPointSegmentCircle(A, B, other.Center, other.Radius, out float disSquared);
        var curNormal = B - A;
        var index = 0;
        var result = SegmentDef.Segment.GetClosestPointSegmentCircle(B, C, other.Center, other.Radius, out float dis);
        if (dis < disSquared)
        {
            index = 1;
            closestResult = result;
            disSquared = dis;
            curNormal = C - B;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentCircle(C, A, other.Center, other.Radius, out dis);
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
        var closestResult = SegmentDef.Segment.GetClosestPointSegmentSegment(A, B, other.A, other.B, out float disSquared);
        var curSelfNormal = B - A;
        var curOtherNormal = B - A;
        var selfIndex = 0;
        var otherIndex = 0;

        var result = SegmentDef.Segment.GetClosestPointSegmentSegment(A, B, other.B, other.C, out float dis);
        if (dis < disSquared)
        {
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.C - other.B;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(A, B, other.C, other.A, out dis);
        if (dis < disSquared)
        {
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.A - other.C;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(B, C, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.B - other.A;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(B, C, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.C - other.B;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(B, C, other.C, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.A - other.C;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(C, A, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.B - other.A;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(C, A, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.C - other.B;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(C, A, other.C, other.A, out dis);
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

    public ClosestPointResult GetClosestPoint(Quad other)
    {
        var closestResult = SegmentDef.Segment.GetClosestPointSegmentSegment(A, B, other.A, other.B, out float disSquared);
        var curSelfNormal = B - A;
        var curOtherNormal = B - A;
        var selfIndex = 0;
        var otherIndex = 0;
        var result = SegmentDef.Segment.GetClosestPointSegmentSegment(A, B, other.B, other.C, out float dis);
        if (dis < disSquared)
        {
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.C - other.B;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(A, B, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.D - other.C;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(A, B, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.A - other.D;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(B, C, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.B - other.A;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(B, C, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.C - other.B;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(B, C, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.D - other.C;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(B, C, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.A - other.D;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(C, A, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.B - other.A;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(C, A, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.C - other.B;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(C, A, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.D - other.C;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(C, A, other.D, other.A, out dis);
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

    public ClosestPointResult GetClosestPoint(Rect other)
    {
        var closestResult = SegmentDef.Segment.GetClosestPointSegmentSegment(A, B, other.A, other.B, out float disSquared);
        var curSelfNormal = B - A;
        var curOtherNormal = B - A;
        var selfIndex = 0;
        var otherIndex = 0;
        var result = SegmentDef.Segment.GetClosestPointSegmentSegment(A, B, other.B, other.C, out float dis);
        if (dis < disSquared)
        {
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.C - other.B;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(A, B, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.D - other.C;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(A, B, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.A - other.D;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(B, C, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.B - other.A;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(B, C, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.C - other.B;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(B, C, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.D - other.C;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(B, C, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.A - other.D;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(C, A, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.B - other.A;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(C, A, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.C - other.B;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(C, A, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.D - other.C;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(C, A, other.D, other.A, out dis);
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

    public ClosestPointResult GetClosestPoint(Polygon other)
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

            var result = SegmentDef.Segment.GetClosestPointSegmentSegment(A, B, p1, p2, out float dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 0;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = B - A;
            }

            result = SegmentDef.Segment.GetClosestPointSegmentSegment(B, C, p1, p2, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 1;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = C - B;
            }

            result = SegmentDef.Segment.GetClosestPointSegmentSegment(C, A, p1, p2, out dis);
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

    public ClosestPointResult GetClosestPoint(Polyline other)
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

            var result = SegmentDef.Segment.GetClosestPointSegmentSegment(A, B, p1, p2, out float dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 0;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = B - A;
            }

            result = SegmentDef.Segment.GetClosestPointSegmentSegment(B, C, p1, p2, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 1;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = C - B;
            }

            result = SegmentDef.Segment.GetClosestPointSegmentSegment(C, A, p1, p2, out dis);
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

    public ClosestPointResult GetClosestPoint(Segments other)
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

    public (SegmentDef.Segment segment, CollisionPoint segmentPoint) GetClosestSegment(Vector2 p, out float disSquared)
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