using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.RectDef;

public readonly partial struct Rect
{
    public static Vector2 GetClosestPointRectPoint(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 p, out float disSquared)
    {
        var min = SegmentDef.Segment.GetClosestPointSegmentPoint(a, b, p, out float minDisSq);

        var cp = SegmentDef.Segment.GetClosestPointSegmentPoint(b, c, p, out float dis);
        if (dis < minDisSq)
        {
            min = cp;
            minDisSq = dis;
        }

        cp = SegmentDef.Segment.GetClosestPointSegmentPoint(c, d, p, out dis);
        if (dis < minDisSq)
        {
            min = cp;
            minDisSq = dis;
        }

        cp = SegmentDef.Segment.GetClosestPointSegmentPoint(d, a, p, out dis);
        if (dis < minDisSq)
        {
            disSquared = dis;
            return cp;
        }

        disSquared = minDisSq;
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

        cp = SegmentDef.Segment.GetClosestPointSegmentPoint(C, D, p, out dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            normal = D - C;
        }

        cp = SegmentDef.Segment.GetClosestPointSegmentPoint(D, A, p, out dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            normal = A - D;
        }

        return new(min, normal.GetPerpendicularRight().Normalize());
    }

    public CollisionPoint GetClosestPoint(Vector2 p, out float disSquared, out int index)
    {
        var min = SegmentDef.Segment.GetClosestPointSegmentPoint(A, B, p, out disSquared);
        index = 0;
        var normal = B - A;

        var cp = SegmentDef.Segment.GetClosestPointSegmentPoint(B, C, p, out float dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            index = 1;
            normal = C - B;
        }

        cp = SegmentDef.Segment.GetClosestPointSegmentPoint(C, D, p, out dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            index = 2;
            normal = D - C;
        }

        cp = SegmentDef.Segment.GetClosestPointSegmentPoint(D, A, p, out dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            index = 3;
            normal = A - D;
        }

        return new(min, normal.GetPerpendicularRight().Normalize());
    }

    public ClosestPointResult GetClosestPoint(Line other)
    {
        var closestResult = SegmentDef.Segment.GetClosestPointSegmentLine(A, B, other.Point, other.Direction, out float disSquared);
        var curNormal = B - A;
        var selfIndex = 0;

        var result = SegmentDef.Segment.GetClosestPointSegmentLine(B, C, other.Point, other.Direction, out float dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            closestResult = result;
            disSquared = dis;
            curNormal = C - B;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentLine(C, D, other.Point, other.Direction, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            closestResult = result;
            disSquared = dis;
            curNormal = D - C;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentLine(D, A, other.Point, other.Direction, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            closestResult = result;
            disSquared = dis;
            curNormal = A - D;
        }

        return new(
            new(closestResult.self, curNormal.GetPerpendicularRight().Normalize()),
            new(closestResult.other, other.Normal),
            disSquared,
            selfIndex);
    }

    public ClosestPointResult GetClosestPoint(Ray other)
    {
        var closestResult = SegmentDef.Segment.GetClosestPointSegmentRay(A, B, other.Point, other.Direction, out float disSquared);
        var curNormal = B - A;
        var selfIndex = 0;
        var result = SegmentDef.Segment.GetClosestPointSegmentRay(B, C, other.Point, other.Direction, out float dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            closestResult = result;
            disSquared = dis;
            curNormal = C - B;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentRay(C, D, other.Point, other.Direction, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            closestResult = result;
            disSquared = dis;
            curNormal = D - C;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentRay(D, A, other.Point, other.Direction, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            closestResult = result;
            disSquared = dis;
            curNormal = A - D;
        }

        return new(
            new(closestResult.self, curNormal.GetPerpendicularRight().Normalize()),
            new(closestResult.other, other.Normal),
            disSquared,
            selfIndex);
    }

    public ClosestPointResult GetClosestPoint(SegmentDef.Segment other)
    {
        var closestResult = SegmentDef.Segment.GetClosestPointSegmentSegment(A, B, other.Start, other.End, out float disSquared);
        var curNormal = B - A;
        var selfIndex = 0;

        var result = SegmentDef.Segment.GetClosestPointSegmentSegment(B, C, other.Start, other.End, out float dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            closestResult = result;
            disSquared = dis;
            curNormal = C - B;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(C, D, other.Start, other.End, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            closestResult = result;
            disSquared = dis;
            curNormal = D - C;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(D, A, other.Start, other.End, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            closestResult = result;
            disSquared = dis;
            curNormal = A - D;
        }

        return new(
            new(closestResult.self, curNormal.GetPerpendicularRight().Normalize()),
            new(closestResult.other, other.Normal),
            disSquared,
            selfIndex);
    }

    public ClosestPointResult GetClosestPoint(Circle other)
    {
        var closestResult = SegmentDef.Segment.GetClosestPointSegmentCircle(A, B, other.Center, other.Radius, out float disSquared);
        var curNormal = B - A;
        var selfIndex = 0;

        var result = SegmentDef.Segment.GetClosestPointSegmentCircle(B, C, other.Center, other.Radius, out float dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            closestResult = result;
            disSquared = dis;
            curNormal = C - B;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentCircle(C, D, other.Center, other.Radius, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            closestResult = result;
            disSquared = dis;
            curNormal = D - C;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentCircle(D, A, other.Center, other.Radius, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            closestResult = result;
            disSquared = dis;
            curNormal = A - D;
        }

        return new(
            new(closestResult.self, curNormal.GetPerpendicularRight().Normalize()),
            new(closestResult.other, (closestResult.other - other.Center).Normalize()),
            disSquared,
            selfIndex);
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

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(C, D, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.B - other.A;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(C, D, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.C - other.B;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(C, D, other.C, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.A - other.C;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(D, A, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.B - other.A;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(D, A, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.C - other.B;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(D, A, other.C, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
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

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(C, D, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.B - other.A;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(C, D, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.C - other.B;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(C, D, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.D - other.C;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(C, D, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.A - other.D;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(D, A, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.B - other.A;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(D, A, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.C - other.B;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(D, A, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.D - other.C;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(D, A, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
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

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(C, D, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.B - other.A;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(C, D, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.C - other.B;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(C, D, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.D - other.C;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(C, D, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.A - other.D;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(D, A, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.B - other.A;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(D, A, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.C - other.B;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(D, A, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.D - other.C;
        }

        result = SegmentDef.Segment.GetClosestPointSegmentSegment(D, A, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
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
        float disSquared = -1;
        var selfIndex = -1;
        var otherIndex = -1;
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

            result = SegmentDef.Segment.GetClosestPointSegmentSegment(C, D, p1, p2, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 2;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = D - C;
            }

            result = SegmentDef.Segment.GetClosestPointSegmentSegment(A, D, p1, p2, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 3;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = A - D;
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
        var disSquared = -1f;
        var selfIndex = -1;
        var otherIndex = -1;
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

            result = SegmentDef.Segment.GetClosestPointSegmentSegment(C, D, p1, p2, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 2;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = D - C;
            }

            result = SegmentDef.Segment.GetClosestPointSegmentSegment(A, D, p1, p2, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 3;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = A - D;
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
        var closestSegment = TopSegment;
        var closestResult = closestSegment.GetClosestPoint(p, out float minDisSquared);

        var currentSegment = LeftSegment;
        var result = currentSegment.GetClosestPoint(p, out float dis);
        if (dis < minDisSquared)
        {
            closestSegment = currentSegment;
            minDisSquared = dis;
            closestResult = result;
        }

        currentSegment = BottomSegment;
        result = currentSegment.GetClosestPoint(p, out dis);
        if (dis < minDisSquared)
        {
            closestSegment = currentSegment;
            minDisSquared = dis;
            closestResult = result;
        }

        currentSegment = RightSegment;
        result = currentSegment.GetClosestPoint(p, out dis);
        if (dis < minDisSquared)
        {
            closestSegment = currentSegment;
            minDisSquared = dis;
            closestResult = result;
        }

        disSquared = minDisSquared;
        return (closestSegment, closestResult);
    }

    public Vector2 GetClosestVertex(Vector2 p, out float disSquared, out int index)
    {
        var closest = TopLeft;
        disSquared = (TopLeft - p).LengthSquared();
        index = 0;

        float l = (BottomLeft - p).LengthSquared();
        if (l < disSquared)
        {
            closest = BottomLeft;
            disSquared = l;
            index = 1;
        }

        l = (BottomRight - p).LengthSquared();
        if (l < disSquared)
        {
            closest = BottomRight;
            disSquared = l;
            index = 2;
        }

        l = (TopRight - p).LengthSquared();
        if (l < disSquared)
        {
            disSquared = l;
            closest = TopRight;
            index = 3;
        }

        return closest;
    }

    public Vector2 GetFurthestVertex(Vector2 p, out float disSquared, out int index)
    {
        var furthest = TopLeft;
        disSquared = (TopLeft - p).LengthSquared();
        index = 0;

        float l = (BottomLeft - p).LengthSquared();
        if (l > disSquared)
        {
            furthest = BottomLeft;
            disSquared = l;
            index = 1;
        }

        l = (BottomRight - p).LengthSquared();
        if (l > disSquared)
        {
            furthest = BottomRight;
            disSquared = l;
            index = 2;
        }

        l = (TopRight - p).LengthSquared();
        if (l > disSquared)
        {
            disSquared = l;
            furthest = TopRight;
            index = 3;
        }

        return furthest;
    }
}