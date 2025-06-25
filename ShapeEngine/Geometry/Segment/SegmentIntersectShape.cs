using ShapeEngine.Core.CollisionSystem;

namespace ShapeEngine.Geometry.Segment;

public readonly partial struct Segment
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

    public CollisionPoints? IntersectShape(Segment s)
    {
        var cp = IntersectSegmentSegment(Start, End, s.Start, s.End, s.Normal);
        if (cp.Valid) return [cp];

        return null;
    }

    public CollisionPoints? IntersectShape(Line.Line l)
    {
        var cp = IntersectSegmentLine(Start, End, l.Point, l.Direction, l.Normal);
        if (cp.Valid) return [cp];

        return null;
    }

    public CollisionPoints? IntersectShape(Ray.Ray r)
    {
        var cp = IntersectSegmentRay(Start, End, r.Point, r.Direction, r.Normal);
        if (cp.Valid) return [cp];

        return null;
    }

    public CollisionPoints? IntersectShape(Circle.Circle c)
    {
        var result = IntersectSegmentCircle(Start, End, c.Center, c.Radius);

        if (result.a.Valid && result.b.Valid)
        {
            var points = new CollisionPoints
            {
                result.a,
                result.b
            };
            return points;
        }

        if (result.a.Valid)
        {
            var points = new CollisionPoints { result.a };
            return points;
        }

        if (result.b.Valid)
        {
            var points = new CollisionPoints { result.b };
            return points;
        }

        return null;
    }

    public CollisionPoints? IntersectShape(Triangle.Triangle t)
    {
        CollisionPoints? points = null;
        var cp = IntersectSegmentSegment(Start, End, t.A, t.B);
        if (cp.Valid)
        {
            points ??= new();
            points.Add(cp);
        }

        cp = IntersectSegmentSegment(Start, End, t.B, t.C);
        if (cp.Valid)
        {
            points ??= new();
            points.Add(cp);
        }

        //intersecting a triangle with a segment can not result in more than 2 intersection points
        if (points is { Count: 2 }) return points;

        cp = IntersectSegmentSegment(Start, End, t.C, t.A);
        if (cp.Valid)
        {
            points ??= new();
            points.Add(cp);
        }

        return points;
    }

    public CollisionPoints? IntersectShape(Quad.Quad q)
    {
        CollisionPoints? points = null;
        var cp = IntersectSegmentSegment(Start, End, q.A, q.B);
        if (cp.Valid)
        {
            points ??= new();
            points.Add(cp);
        }

        cp = IntersectSegmentSegment(Start, End, q.B, q.C);
        if (cp.Valid)
        {
            points ??= new();
            points.Add(cp);
        }

        //intersecting a quad with a segment can not result in more than 2 intersection points
        if (points is { Count: 2 }) return points;

        cp = IntersectSegmentSegment(Start, End, q.C, q.D);
        if (cp.Valid)
        {
            points ??= new();
            points.Add(cp);
        }

        //intersecting a quad with a segment can not result in more than 2 intersection points
        if (points is { Count: 2 }) return points;

        cp = IntersectSegmentSegment(Start, End, q.D, q.A);
        if (cp.Valid)
        {
            points ??= new();
            points.Add(cp);
        }

        return points;
    }

    public CollisionPoints? IntersectShape(Rect.Rect r)
    {
        CollisionPoints? points = null;
        var a = r.TopLeft;
        var b = r.BottomLeft;

        var cp = IntersectSegmentSegment(Start, End, a, b);
        if (cp.Valid)
        {
            points ??= new();
            points.Add(cp);
        }

        var c = r.BottomRight;
        cp = IntersectSegmentSegment(Start, End, b, c);
        if (cp.Valid)
        {
            points ??= new();
            points.Add(cp);
        }

        //intersecting a rect with a segment can not result in more than 2 intersection points
        if (points is { Count: 2 }) return points;

        var d = r.TopRight;
        cp = IntersectSegmentSegment(Start, End, c, d);
        if (cp.Valid)
        {
            points ??= new();
            points.Add(cp);
        }

        //intersecting a rect with a segment can not result in more than 2 intersection points
        if (points is { Count: 2 }) return points;

        cp = IntersectSegmentSegment(Start, End, d, a);
        if (cp.Valid)
        {
            points ??= new();
            points.Add(cp);
        }

        return points;
    }

    public CollisionPoints? IntersectShape(Polygon.Polygon p)
    {
        if (p.Count < 3) return null;
        CollisionPoints? points = null;
        for (var i = 0; i < p.Count; i++)
        {
            var colPoint = IntersectSegmentSegment(Start, End, p[i], p[(i + 1) % p.Count]);
            if (colPoint.Valid)
            {
                points ??= new();
                points.Add(colPoint);
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
            var colPoint = IntersectSegmentSegment(Start, End, pl[i], pl[i + 1]);
            if (colPoint.Valid)
            {
                points ??= new();
                points.Add(colPoint);
            }
        }

        return points;
    }

    public CollisionPoints? IntersectShape(Segments.Segments shape)
    {
        if (shape.Count <= 0) return null;
        CollisionPoints? points = null;

        foreach (var seg in shape)
        {
            var result = IntersectSegmentSegment(Start, End, seg.Start, seg.End);
            if (result.Valid)
            {
                points ??= new();
                points.AddRange(result);
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

    public int IntersectShape(Ray.Ray r, ref CollisionPoints points)
    {
        var cp = IntersectSegmentRay(Start, End, r.Point, r.Direction, r.Normal);
        if (cp.Valid)
        {
            points.Add(cp);
            return 1;
        }

        return 0;
    }

    public int IntersectShape(Line.Line l, ref CollisionPoints points)
    {
        var cp = IntersectSegmentLine(Start, End, l.Point, l.Direction, l.Normal);
        if (cp.Valid)
        {
            points.Add(cp);
            return 1;
        }

        return 0;
    }

    public int IntersectShape(Segment s, ref CollisionPoints points)
    {
        var cp = IntersectSegmentSegment(Start, End, s.Start, s.End);
        if (cp.Valid)
        {
            points.Add(cp);
            return 1;
        }

        return 0;
    }

    public int IntersectShape(Circle.Circle c, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var result = IntersectSegmentCircle(Start, End, c.Center, c.Radius);

        if (result.a.Valid && result.b.Valid)
        {
            if (returnAfterFirstValid)
            {
                points.Add(result.a);
                return 1;
            }

            points.Add(result.a);
            points.Add(result.b);
            return 2;
        }

        if (result.a.Valid)
        {
            points.Add(result.a);
            return 1;
        }

        if (result.b.Valid)
        {
            points.Add(result.b);
            return 1;
        }

        return 0;
    }

    public int IntersectShape(Triangle.Triangle t, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var cp = IntersectSegmentSegment(Start, End, t.A, t.B);
        var count = 0;
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        cp = IntersectSegmentSegment(Start, End, t.B, t.C);
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        //intersecting a triangle with a segment can not result in more than 2 intersection points
        if (count >= 2) return count;

        cp = IntersectSegmentSegment(Start, End, t.C, t.A);
        if (cp.Valid)
        {
            points.Add(cp);
            count++;
        }

        return count;
    }

    public int IntersectShape(Quad.Quad q, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var cp = IntersectSegmentSegment(Start, End, q.A, q.B);
        var count = 0;
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        cp = IntersectSegmentSegment(Start, End, q.B, q.C);
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        //intersecting a quad with a segment can not result in more than 2 intersection points
        if (count >= 2) return count;

        cp = IntersectSegmentSegment(Start, End, q.C, q.D);
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        //intersecting a quad with a segment can not result in more than 2 intersection points
        if (count >= 2) return count;

        cp = IntersectSegmentSegment(Start, End, q.D, q.A);
        if (cp.Valid)
        {
            points.Add(cp);
            count++;
        }

        return count;
    }

    public int IntersectShape(Rect.Rect r, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var a = r.TopLeft;
        var b = r.BottomLeft;

        var cp = IntersectSegmentSegment(Start, End, a, b);
        var count = 0;
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        var c = r.BottomRight;
        cp = IntersectSegmentSegment(Start, End, b, c);
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        //intersecting a rect with a segment can not result in more than 2 intersection points
        if (count >= 2) return count;

        var d = r.TopRight;
        cp = IntersectSegmentSegment(Start, End, c, d);
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        //intersecting a rect with a segment can not result in more than 2 intersection points
        if (count >= 2) return count;

        cp = IntersectSegmentSegment(Start, End, d, a);
        if (cp.Valid)
        {
            points.Add(cp);
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
            var cp = IntersectSegmentSegment(Start, End, p[i], p[(i + 1) % p.Count]);
            if (cp.Valid)
            {
                points.Add(cp);
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
            var cp = IntersectSegmentSegment(Start, End, pl[i], pl[i + 1]);
            if (cp.Valid)
            {
                points.Add(cp);
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
            var cp = IntersectSegmentSegment(Start, End, seg.Start, seg.End);
            if (cp.Valid)
            {
                points.Add(cp);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }

        return count;
    }

}