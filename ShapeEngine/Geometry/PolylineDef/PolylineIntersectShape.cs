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

namespace ShapeEngine.Geometry.PolylineDef;

public partial class Polyline
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

    public CollisionPoints? IntersectShape(Ray ray)
    {
        if (Count < 2) return null;

        CollisionPoints? points = null;
        for (var i = 0; i < Count - 1; i++)
        {
            var result = Segment.IntersectSegmentRay(this[i], this[i + 1], ray.Point, ray.Direction, ray.Normal);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
        }

        return points;
    }

    public CollisionPoints? IntersectShape(Line l)
    {
        if (Count < 2) return null;

        CollisionPoints? points = null;
        for (var i = 0; i < Count - 1; i++)
        {
            var result = Segment.IntersectSegmentLine(this[i], this[i + 1], l.Point, l.Direction, l.Normal);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
        }

        return points;
    }

    public CollisionPoints? IntersectShape(Segment s)
    {
        if (Count < 2) return null;

        CollisionPoints? points = null;
        for (var i = 0; i < Count - 1; i++)
        {
            var result = Segment.IntersectSegmentSegment(this[i], this[i + 1], s.Start, s.End);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
        }

        return points;
    }

    public CollisionPoints? IntersectShape(Circle c)
    {
        if (Count < 2) return null;

        CollisionPoints? points = null;

        for (var i = 0; i < Count - 1; i++)
        {
            var result = Segment.IntersectSegmentCircle(this[i], this[i + 1], c.Center, c.Radius);
            if (result.a.Valid || result.b.Valid)
            {
                points ??= new();
                if (result.a.Valid) points.Add(result.a);
                if (result.b.Valid) points.Add(result.b);
            }
        }

        return points;
    }

    public CollisionPoints? IntersectShape(Triangle t)
    {
        if (Count < 2) return null;

        CollisionPoints? points = null;
        for (var i = 0; i < Count - 1; i++)
        {
            var p1 = this[i];
            var p2 = this[i + 1];
            var result = Segment.IntersectSegmentSegment(p1, p2, t.A, t.B);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(p1, p2, t.B, t.C);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(p1, p2, t.C, t.A);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
        }

        return points;
    }

    public CollisionPoints? IntersectShape(Rect r)
    {
        if (Count < 2) return null;

        CollisionPoints? points = null;
        var a = r.TopLeft;
        var b = r.BottomLeft;
        var c = r.BottomRight;
        var d = r.TopRight;
        for (var i = 0; i < Count - 1; i++)
        {
            var p1 = this[i];
            var p2 = this[i + 1];
            var result = Segment.IntersectSegmentSegment(p1, p2, a, b);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(p1, p2, b, c);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(p1, p2, c, d);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(p1, p2, d, a);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
        }

        return points;
    }

    public CollisionPoints? IntersectShape(Quad q)
    {
        if (Count < 2) return null;

        CollisionPoints? points = null;
        for (var i = 0; i < Count - 1; i++)
        {
            var p1 = this[i];
            var p2 = this[i + 1];
            var result = Segment.IntersectSegmentSegment(p1, p2, q.A, q.B);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(p1, p2, q.B, q.C);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(p1, p2, q.C, q.D);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(p1, p2, q.D, q.A);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
        }

        return points;
    }

    public CollisionPoints? IntersectShape(Polygon p)
    {
        if (p.Count < 3 || Count < 2) return null;
        CollisionPoints? points = null;
        for (var i = 0; i < Count - 1; i++)
        {
            for (var j = 0; j < p.Count; j++)
            {
                var result = Segment.IntersectSegmentSegment(this[i], this[i + 1], p[j], p[(j + 1) % p.Count]);
                if (result.Valid)
                {
                    points ??= new();
                    points.Add(result);
                }
            }
        }

        return points;
    }

    public CollisionPoints? IntersectShape(Polyline pl)
    {
        if (pl.Count < 2 || Count < 2) return null;
        CollisionPoints? points = null;
        for (var i = 0; i < Count - 1; i++)
        {
            for (var j = 0; j < pl.Count - 1; j++)
            {
                var result = Segment.IntersectSegmentSegment(this[i], this[i + 1], pl[j], pl[j + 1]);
                if (result.Valid)
                {
                    points ??= new();
                    points.Add(result);
                }
            }
        }

        return points;
    }

    public CollisionPoints? IntersectShape(Segments segments)
    {
        if (Count < 2 || segments.Count <= 0) return null;
        CollisionPoints? points = null;
        for (var i = 0; i < Count - 1; i++)
        {
            foreach (var seg in segments)
            {
                var result = Segment.IntersectSegmentSegment(this[i], this[i + 1], seg.Start, seg.End);
                if (result.Valid)
                {
                    points ??= new();
                    points.Add(result);
                }
            }
        }

        return points;
    }

}