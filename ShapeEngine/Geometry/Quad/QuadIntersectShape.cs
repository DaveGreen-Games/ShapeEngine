using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.Segment;

namespace ShapeEngine.Geometry.Quad;

public readonly partial struct Quad
{
    public CollisionPoints? Intersect(Collider collider)
    {
        if (!collider.Enabled) return null;

        switch (collider.GetShapeType())
        {
            case ShapeType.Circle:
                var c = collider.GetCircleShape();
                return IntersectShape(c);
            case ShapeType.Ray:
                var rayShape = collider.GetRayShape();
                return IntersectShape(rayShape);
            case ShapeType.Line:
                var l = collider.GetLineShape();
                return IntersectShape(l);
            case ShapeType.Segment:
                var s = collider.GetSegmentShape();
                return IntersectShape(s);
            case ShapeType.Triangle:
                var t = collider.GetTriangleShape();
                return IntersectShape(t);
            case ShapeType.Rect:
                var r = collider.GetRectShape();
                return IntersectShape(r);
            case ShapeType.Quad:
                var q = collider.GetQuadShape();
                return IntersectShape(q);
            case ShapeType.Poly:
                var p = collider.GetPolygonShape();
                return IntersectShape(p);
            case ShapeType.PolyLine:
                var pl = collider.GetPolylineShape();
                return IntersectShape(pl);
        }

        return null;
    }

    public CollisionPoints? IntersectShape(Segments.Segments segments)
    {
        if (segments.Count <= 0) return null;

        CollisionPoints? points = null;

        foreach (var seg in segments)
        {
            var result = Segment.Segment.IntersectSegmentSegment(A, B, seg.Start, seg.End);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.Segment.IntersectSegmentSegment(B, C, seg.Start, seg.End);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.Segment.IntersectSegmentSegment(C, D, seg.Start, seg.End);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.Segment.IntersectSegmentSegment(D, A, seg.Start, seg.End);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
        }

        return points;
    }

    public CollisionPoints? IntersectShape(Ray.Ray r)
    {
        CollisionPoints? points = null;
        var result = Segment.Segment.IntersectSegmentRay(A, B, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentRay(B, C, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentRay(C, D, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentRay(D, A, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        return points;
    }

    public CollisionPoints? IntersectShape(Line.Line l)
    {
        CollisionPoints? points = null;
        var result = Segment.Segment.IntersectSegmentLine(A, B, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentLine(B, C, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentLine(C, D, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentLine(D, A, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        return points;
    }

    public CollisionPoints? IntersectShape(Segment.Segment s)
    {
        CollisionPoints? points = null;
        var result = Segment.Segment.IntersectSegmentSegment(A, B, s.Start, s.End);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(B, C, s.Start, s.End);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(C, D, s.Start, s.End);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(D, A, s.Start, s.End);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        return points;
    }

    public CollisionPoints? IntersectShape(Circle.Circle c)
    {
        CollisionPoints? points = null;
        var result = Segment.Segment.IntersectSegmentCircle(A, B, c.Center, c.Radius);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if (result.a.Valid) points.Add(result.a);
            if (result.b.Valid) points.Add(result.b);
        }

        result = Segment.Segment.IntersectSegmentCircle(B, C, c.Center, c.Radius);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if (result.a.Valid) points.Add(result.a);
            if (result.b.Valid) points.Add(result.b);
        }

        result = Segment.Segment.IntersectSegmentCircle(C, D, c.Center, c.Radius);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if (result.a.Valid) points.Add(result.a);
            if (result.b.Valid) points.Add(result.b);
        }

        result = Segment.Segment.IntersectSegmentCircle(D, A, c.Center, c.Radius);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if (result.a.Valid) points.Add(result.a);
            if (result.b.Valid) points.Add(result.b);
        }

        return points;
    }

    public CollisionPoints? IntersectShape(Triangle.Triangle t)
    {
        CollisionPoints? points = null;
        var result = Segment.Segment.IntersectSegmentSegment(A, B, t.A, t.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(A, B, t.B, t.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(A, B, t.C, t.A);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }


        result = Segment.Segment.IntersectSegmentSegment(B, C, t.A, t.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(B, C, t.B, t.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(B, C, t.C, t.A);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(C, D, t.A, t.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(C, D, t.B, t.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(C, D, t.C, t.A);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(D, A, t.A, t.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(D, A, t.B, t.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(D, A, t.C, t.A);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        return points;
    }

    public CollisionPoints? IntersectShape(Rect.Rect r)
    {
        CollisionPoints? points = null;
        var a = r.TopLeft;
        var b = r.BottomLeft;
        var result = Segment.Segment.IntersectSegmentSegment(A, B, a, b);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        var c = r.BottomRight;
        result = Segment.Segment.IntersectSegmentSegment(A, B, b, c);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        var d = r.TopRight;
        result = Segment.Segment.IntersectSegmentSegment(A, B, c, d);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(A, B, d, a);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }


        result = Segment.Segment.IntersectSegmentSegment(B, C, a, b);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(B, C, b, c);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(B, C, c, d);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(B, C, d, a);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }


        result = Segment.Segment.IntersectSegmentSegment(C, D, a, b);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(C, D, b, c);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(C, D, c, d);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(C, D, d, a);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }


        result = Segment.Segment.IntersectSegmentSegment(D, A, a, b);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(D, A, b, c);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(D, A, c, d);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(D, A, d, a);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        return points;
    }

    public CollisionPoints? IntersectShape(Quad q)
    {
        CollisionPoints? points = null;
        var result = Segment.Segment.IntersectSegmentSegment(A, B, q.A, q.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(A, B, q.B, q.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(A, B, q.C, q.D);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(A, B, q.D, q.A);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }


        result = Segment.Segment.IntersectSegmentSegment(B, C, q.A, q.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(B, C, q.B, q.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(B, C, q.C, q.D);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(B, C, q.D, q.A);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }


        result = Segment.Segment.IntersectSegmentSegment(C, D, q.A, q.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(C, D, q.B, q.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(C, D, q.C, q.D);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(C, D, q.D, q.A);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(D, A, q.A, q.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(D, A, q.B, q.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(D, A, q.C, q.D);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.Segment.IntersectSegmentSegment(D, A, q.D, q.A);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        return points;
    }

    public CollisionPoints? IntersectShape(Polygon.Polygon p)
    {
        if (p.Count < 3) return null;

        CollisionPoints? points = null;
        for (var i = 0; i < p.Count; i++)
        {
            var result = Segment.Segment.IntersectSegmentSegment(A, B, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.Segment.IntersectSegmentSegment(B, C, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.Segment.IntersectSegmentSegment(C, D, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.Segment.IntersectSegmentSegment(D, A, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
        }

        return points;
    }

    public CollisionPoints? IntersectShape(Polyline.Polyline pl)
    {
        if (pl.Count < 2) return null;

        CollisionPoints? points = null;
        for (var i = 0; i < pl.Count - 1; i++)
        {
            var result = Segment.Segment.IntersectSegmentSegment(A, B, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.Segment.IntersectSegmentSegment(B, C, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.Segment.IntersectSegmentSegment(C, D, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.Segment.IntersectSegmentSegment(D, A, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
        }

        return points;
    }

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
        var result = Segment.Segment.IntersectSegmentRay(A, B, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentRay(B, C, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (count >= 2) return count;

        result = Segment.Segment.IntersectSegmentRay(C, D, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (count >= 2) return count;

        result = Segment.Segment.IntersectSegmentRay(D, A, r.Point, r.Direction, r.Normal);
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
        var result = Segment.Segment.IntersectSegmentLine(A, B, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentLine(B, C, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (count >= 2) return count;

        result = Segment.Segment.IntersectSegmentLine(C, D, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (count >= 2) return count;

        result = Segment.Segment.IntersectSegmentLine(D, A, l.Point, l.Direction, l.Normal);
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
        var result = Segment.Segment.IntersectSegmentSegment(A, B, s.Start, s.End);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(B, C, s.Start, s.End);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (count >= 2) return count;

        result = Segment.Segment.IntersectSegmentSegment(C, D, s.Start, s.End);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (count >= 2) return count;

        result = Segment.Segment.IntersectSegmentSegment(D, A, s.Start, s.End);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }

        return count;
    }

    public int IntersectShape(Circle.Circle c, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        var result = Segment.Segment.IntersectSegmentCircle(A, B, c.Center, c.Radius);
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

        result = Segment.Segment.IntersectSegmentCircle(B, C, c.Center, c.Radius);
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

        result = Segment.Segment.IntersectSegmentCircle(C, D, c.Center, c.Radius);
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

        result = Segment.Segment.IntersectSegmentCircle(D, A, c.Center, c.Radius);
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
        var result = Segment.Segment.IntersectSegmentSegment(A, B, t.A, t.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(A, B, t.B, t.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(A, B, t.C, t.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }


        result = Segment.Segment.IntersectSegmentSegment(B, C, t.A, t.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(B, C, t.B, t.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(B, C, t.C, t.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(C, D, t.A, t.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(C, D, t.B, t.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(C, D, t.C, t.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(D, A, t.A, t.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(D, A, t.B, t.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(D, A, t.C, t.A);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }

        return count;
    }

    public int IntersectShape(Quad q, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        var result = Segment.Segment.IntersectSegmentSegment(A, B, q.A, q.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(A, B, q.B, q.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(A, B, q.C, q.D);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(A, B, q.D, q.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }


        result = Segment.Segment.IntersectSegmentSegment(B, C, q.A, q.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(B, C, q.B, q.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(B, C, q.C, q.D);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(B, C, q.D, q.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }


        result = Segment.Segment.IntersectSegmentSegment(C, D, q.A, q.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(C, D, q.B, q.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(C, D, q.C, q.D);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(C, D, q.D, q.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(D, A, q.A, q.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(D, A, q.B, q.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(D, A, q.C, q.D);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(D, A, q.D, q.A);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }

        return count;
    }

    public int IntersectShape(Rect.Rect r, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        var a = r.TopLeft;
        var b = r.BottomLeft;
        var result = Segment.Segment.IntersectSegmentSegment(A, B, a, b);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        var c = r.BottomRight;
        result = Segment.Segment.IntersectSegmentSegment(A, B, b, c);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        var d = r.TopRight;
        result = Segment.Segment.IntersectSegmentSegment(A, B, c, d);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(A, B, d, a);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }


        result = Segment.Segment.IntersectSegmentSegment(B, C, a, b);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(B, C, b, c);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(B, C, c, d);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(B, C, d, a);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }


        result = Segment.Segment.IntersectSegmentSegment(C, D, a, b);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(C, D, b, c);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(C, D, c, d);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(C, D, d, a);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }


        result = Segment.Segment.IntersectSegmentSegment(D, A, a, b);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(D, A, b, c);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(D, A, c, d);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.Segment.IntersectSegmentSegment(D, A, d, a);
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
        for (var i = 0; i < p.Count; i++)
        {
            var result = Segment.Segment.IntersectSegmentSegment(A, B, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.Segment.IntersectSegmentSegment(B, C, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.Segment.IntersectSegmentSegment(C, D, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.Segment.IntersectSegmentSegment(D, A, p[i], p[(i + 1) % p.Count]);
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
        for (var i = 0; i < pl.Count - 1; i++)
        {
            var result = Segment.Segment.IntersectSegmentSegment(A, B, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.Segment.IntersectSegmentSegment(B, C, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.Segment.IntersectSegmentSegment(C, D, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.Segment.IntersectSegmentSegment(D, A, pl[i], pl[(i + 1) % pl.Count]);
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

        foreach (var seg in shape)
        {
            var result = Segment.Segment.IntersectSegmentSegment(A, B, seg.Start, seg.End);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.Segment.IntersectSegmentSegment(B, C, seg.Start, seg.End);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.Segment.IntersectSegmentSegment(C, D, seg.Start, seg.End);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.Segment.IntersectSegmentSegment(D, A, seg.Start, seg.End);
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