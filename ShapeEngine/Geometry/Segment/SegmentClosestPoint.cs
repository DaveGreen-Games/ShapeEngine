using System.Numerics;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.Segment;

public readonly partial struct Segment
{
    public CollisionPoint GetClosestPoint(Vector2 p, out float disSquared)
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

    public ClosestPointResult GetClosestPoint(Line.Line other)
    {
        var result = GetClosestPointSegmentLine(Start, End, other.Point, other.Direction, out float disSquared);
        return new(
            new(result.self, Normal),
            new(result.other, other.Normal),
            disSquared);
        // var d1 = End - Start;
        // var d2 = other.Direction;
        // var r = Start - other.Point;
        //
        // float a = Vector2.Dot(d1, d1);
        // float b = Vector2.Dot(d1, d2);
        // float c = Vector2.Dot(d2, r);
        // float e = Vector2.Dot(d2, d2);
        //
        // float s = Math.Clamp((b * c - a * c) / (a * e - b * b), 0.0f, 1.0f);
        // float t = (b * s + c) / e;
        //
        // var closestPoint1 = Start + s * d1;
        // var closestPoint2 = other.Point + t * d2;
        // float disSquared = (closestPoint2 - closestPoint1).LengthSquared();
        // return new(
        //     new(closestPoint1, Normal), 
        //     new(closestPoint2, other.Normal),
        //     disSquared);
    }

    public ClosestPointResult GetClosestPoint(Ray.Ray other)
    {
        var result = GetClosestPointSegmentRay(Start, End, other.Point, other.Direction, out var disSquared);
        return new(
            new(result.self, Normal),
            new(result.other, other.Normal),
            disSquared);
        // var d1 = End - Start;
        // var d2 = other.Direction;
        // var r = Start - other.Point;
        //
        // float a = Vector2.Dot(d1, d1);
        // float b = Vector2.Dot(d1, d2);
        // float c = Vector2.Dot(d2, r);
        // float e = Vector2.Dot(d2, d2);
        //
        // float s = Math.Clamp((b * c - a * c) / (a * e - b * b), 0.0f, 1.0f);
        // float t = Math.Max(0.0f, (b * s + c) / e);
        //
        // var closestPoint1 = Start + s * d1;
        // var closestPoint2 = other.Point + t * d2;
        // float disSquared = (closestPoint2 - closestPoint1).LengthSquared();
    }

    public ClosestPointResult GetClosestPoint(Segment other)
    {
        var d1 = End - Start;
        var d2 = other.End - other.Start;
        var r = Start - other.Start;

        float a = Vector2.Dot(d1, d1);
        float e = Vector2.Dot(d2, d2);
        float f = Vector2.Dot(d2, r);

        float s, t;
        if (a <= 1e-10 && e <= 1e-10)
        {
            // Both segments degenerate into points
            // s = t = 0.0f;

            return new(
                new(Start, Normal),
                new(other.Start, other.Normal),
                ShapeMath.ClampToZero(r.LengthSquared()));
        }

        if (a <= 1e-10)
        {
            // First segment degenerates into a point
            s = 0.0f;
            t = Math.Clamp(f / e, 0.0f, 1.0f);
        }
        else
        {
            float c = Vector2.Dot(d1, r);
            if (e <= 1e-10)
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

    public ClosestPointResult GetClosestPoint(Circle.Circle other)
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

    public ClosestPointResult GetClosestPoint(Triangle.Triangle other)
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

    public ClosestPointResult GetClosestPoint(Quad.Quad other)
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

    public ClosestPointResult GetClosestPoint(Rect.Rect other)
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

    public ClosestPointResult GetClosestPoint(Polygon.Polygon other)
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

    public ClosestPointResult GetClosestPoint(Polyline.Polyline other)
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

}