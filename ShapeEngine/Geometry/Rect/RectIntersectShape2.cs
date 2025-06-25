using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Geometry.Segment;

namespace ShapeEngine.Geometry.Rect;

public readonly partial struct Rect
{
    public int Intersect(Collider collider, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (!collider.Enabled) return 0;

        switch (collider.GetShapeType())
        {
            case ShapeType.Circle:
                var c = collider.GetCircleShape();
                return IntersectShape(c, ref points, returnAfterFirstValid);
            case ShapeType.Ray:
                var rayShape = collider.GetRayShape();
                return IntersectShape(rayShape, ref points);
            case ShapeType.Line:
                var l = collider.GetLineShape();
                return IntersectShape(l, ref points);
            case ShapeType.Segment:
                var s = collider.GetSegmentShape();
                return IntersectShape(s, ref points);
            case ShapeType.Triangle:
                var t = collider.GetTriangleShape();
                return IntersectShape(t, ref points, returnAfterFirstValid);
            case ShapeType.Rect:
                var r = collider.GetRectShape();
                return IntersectShape(r, ref points, returnAfterFirstValid);
            case ShapeType.Quad:
                var q = collider.GetQuadShape();
                return IntersectShape(q, ref points, returnAfterFirstValid);
            case ShapeType.Poly:
                var p = collider.GetPolygonShape();
                return IntersectShape(p, ref points, returnAfterFirstValid);
            case ShapeType.PolyLine:
                var pl = collider.GetPolylineShape();
                return IntersectShape(pl, ref points, returnAfterFirstValid);
        }

        return 0;
    }

    public int IntersectShape(Ray.Ray r, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;

        var result = Segment.Segment.IntersectSegmentRay(a, b, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentRay(b, c, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (count >= 2) return count;

        result = Segment.Segment.IntersectSegmentRay(c, d, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (count >= 2) return count;

        result = Segment.Segment.IntersectSegmentRay(d, a, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }

        return count;
    }

    public int IntersectShape(Line.Line l, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;

        var result = Segment.Segment.IntersectSegmentLine(a, b, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentLine(b, c, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (count >= 2) return count;

        result = Segment.Segment.IntersectSegmentLine(c, d, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (count >= 2) return count;

        result = Segment.Segment.IntersectSegmentLine(d, a, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }

        return count;
    }

    public int IntersectShape(Segment.Segment s, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;

        var result = Segment.Segment.IntersectSegmentSegment(a, b, s.Start, s.End);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(b, c, s.Start, s.End);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (count >= 2) return count;

        result = Segment.Segment.IntersectSegmentSegment(c, d, s.Start, s.End);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (count >= 2) return count;

        result = Segment.Segment.IntersectSegmentSegment(d, a, s.Start, s.End);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }

        return count;
    }

    public int IntersectShape(Circle.Circle circle, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;

        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;

        var result = Segment.Segment.IntersectSegmentCircle(a, b, circle.Center, circle.Radius);
        if (result.a.Valid)
        {
            points.Add(result.a);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (result.b.Valid)
        {
            points.Add(result.b);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentCircle(b, c, circle.Center, circle.Radius);
        if (result.a.Valid)
        {
            points.Add(result.a);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (result.b.Valid)
        {
            points.Add(result.b);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentCircle(c, d, circle.Center, circle.Radius);
        if (result.a.Valid)
        {
            points.Add(result.a);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (result.b.Valid)
        {
            points.Add(result.b);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentCircle(d, a, circle.Center, circle.Radius);
        if (result.a.Valid)
        {
            points.Add(result.a);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (result.b.Valid)
        {
            points.Add(result.b);
            count++;
        }

        return count;
    }

    public int IntersectShape(Triangle.Triangle t, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;

        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;

        var result = Segment.Segment.IntersectSegmentSegment(a, b, t.A, t.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(a, b, t.B, t.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(a, b, t.C, t.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }


        result = Segment.Segment.IntersectSegmentSegment(b, c, t.A, t.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(b, c, t.B, t.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(b, c, t.C, t.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }


        result = Segment.Segment.IntersectSegmentSegment(c, d, t.A, t.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(c, d, t.B, t.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(c, d, t.C, t.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(d, a, t.A, t.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(d, a, t.B, t.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(d, a, t.C, t.A);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }

        return count;
    }

    public int IntersectShape(Quad.Quad q, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;
        var result = Segment.Segment.IntersectSegmentSegment(a, b, q.A, q.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(a, b, q.B, q.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(a, b, q.C, q.D);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(a, b, q.D, q.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }


        result = Segment.Segment.IntersectSegmentSegment(b, c, q.A, q.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(b, c, q.B, q.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(b, c, q.C, q.D);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(b, c, q.D, q.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }


        result = Segment.Segment.IntersectSegmentSegment(c, d, q.A, q.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(c, d, q.B, q.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(c, d, q.C, q.D);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(c, d, q.D, q.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(d, a, q.A, q.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(d, a, q.B, q.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(d, a, q.C, q.D);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(d, a, q.D, q.A);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }

        return count;
    }

    public int IntersectShape(Rect r, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;

        var rA = r.TopLeft;
        var rB = r.BottomLeft;
        var rC = r.BottomRight;
        var rD = r.TopRight;

        var result = Segment.Segment.IntersectSegmentSegment(a, b, rA, rB);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }


        result = Segment.Segment.IntersectSegmentSegment(a, b, rB, rC);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }


        result = Segment.Segment.IntersectSegmentSegment(a, b, rC, rD);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(a, b, rD, rA);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }


        result = Segment.Segment.IntersectSegmentSegment(b, c, rA, rB);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(b, c, rB, rC);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(b, c, rC, rD);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(b, c, rD, rA);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }


        result = Segment.Segment.IntersectSegmentSegment(c, d, rA, rB);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(c, d, rB, rC);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(c, d, rC, rD);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(c, d, rD, rA);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }


        result = Segment.Segment.IntersectSegmentSegment(d, a, rA, rB);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(d, a, rB, rC);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(d, a, rC, rD);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(d, a, rD, rA);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }

        return count;
    }

    public int IntersectShape(Polygon.Polygon p, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (p.Count < 3) return 0;

        var count = 0;

        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;

        for (var i = 0; i < p.Count; i++)
        {
            var result = Segment.Segment.IntersectSegmentSegment(a, b, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.Segment.IntersectSegmentSegment(b, c, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.Segment.IntersectSegmentSegment(c, d, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.Segment.IntersectSegmentSegment(d, a, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }

        return count;
    }

    public int IntersectShape(Polyline.Polyline pl, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (pl.Count < 2) return 0;

        var count = 0;

        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;
        for (var i = 0; i < pl.Count - 1; i++)
        {
            var result = Segment.Segment.IntersectSegmentSegment(a, b, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.Segment.IntersectSegmentSegment(b, c, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.Segment.IntersectSegmentSegment(c, d, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.Segment.IntersectSegmentSegment(d, a, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }

        return count;
    }

    public int IntersectShape(Segments.Segments shape, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (shape.Count <= 0) return 0;

        var count = 0;

        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;

        foreach (var seg in shape)
        {
            var result = Segment.Segment.IntersectSegmentSegment(a, b, seg.Start, seg.End);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.Segment.IntersectSegmentSegment(b, c, seg.Start, seg.End);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.Segment.IntersectSegmentSegment(c, d, seg.Start, seg.End);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.Segment.IntersectSegmentSegment(d, a, seg.Start, seg.End);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }

        return count;
    }
}