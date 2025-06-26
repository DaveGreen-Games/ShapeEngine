using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.PolygonDef;

public partial class Polygon
{
    public static CollisionPoints? IntersectPolygonRay(List<Vector2> polygon, Vector2 rayPoint, Vector2 rayDirection)
    {
        if (polygon.Count < 3) return null;
        CollisionPoints? points = null;
        for (var i = 0; i < polygon.Count; i++)
        {
            var result = SegmentDef.Segment.IntersectSegmentRay(polygon[i], polygon[(i + 1) % polygon.Count], rayPoint, rayDirection);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
        }
        return points;
    }

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

    public CollisionPoints? IntersectShape(Ray r)
    {
        if (Count < 3) return null;
        CollisionPoints? points = null;
        for (var i = 0; i < Count; i++)
        {
            var result = SegmentDef.Segment.IntersectSegmentRay(this[i], this[(i + 1) % Count], r.Point, r.Direction, r.Normal);
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
        if (Count < 3) return null;
        CollisionPoints? points = null;
        for (var i = 0; i < Count; i++)
        {
            var result = SegmentDef.Segment.IntersectSegmentLine(this[i], this[(i + 1) % Count], l.Point, l.Direction, l.Normal);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
        }

        return points;
    }

    public CollisionPoints? IntersectShape(SegmentDef.Segment s)
    {
        if (Count < 3) return null;
        CollisionPoints? points = null;
        for (var i = 0; i < Count; i++)
        {
            var result = SegmentDef.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], s.Start, s.End);
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
        if (Count < 3) return null;

        CollisionPoints? points = null;

        for (var i = 0; i < Count; i++)
        {
            var result = SegmentDef.Segment.IntersectSegmentCircle(this[i], this[(i + 1) % Count], c.Center, c.Radius);
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
        if (Count < 3) return null;

        CollisionPoints? points = null;
        for (var i = 0; i < Count; i++)
        {
            var result = SegmentDef.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], t.A, t.B);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = SegmentDef.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], t.B, t.C);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = SegmentDef.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], t.C, t.A);
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
        if (Count < 3) return null;

        CollisionPoints? points = null;

        var a = r.TopLeft;
        var b = r.BottomLeft;
        var c = r.BottomRight;
        var d = r.TopRight;

        for (var i = 0; i < Count; i++)
        {
            var result = SegmentDef.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], a, b);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = SegmentDef.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], b, c);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = SegmentDef.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], c, d);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = SegmentDef.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], d, a);
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
        if (Count < 3) return null;

        CollisionPoints? points = null;
        for (var i = 0; i < Count; i++)
        {
            var result = SegmentDef.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], q.A, q.B);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = SegmentDef.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], q.B, q.C);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = SegmentDef.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], q.C, q.D);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = SegmentDef.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], q.D, q.A);
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
        if (Count < 3 || p.Count < 3) return null;
        CollisionPoints? points = null;
        for (var i = 0; i < Count; i++)
        {
            for (var j = 0; j < p.Count; j++)
            {
                var result = SegmentDef.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], p[j], p[(j + 1) % p.Count]);
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
        if (Count < 3 || pl.Count < 2) return null;
        CollisionPoints? points = null;
        for (var i = 0; i < Count; i++)
        {
            for (var j = 0; j < pl.Count - 1; j++)
            {
                var result = SegmentDef.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], pl[j], pl[(j + 1) % pl.Count]);
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
        if (Count < 3 || segments.Count <= 0) return null;
        CollisionPoints? points = null;
        for (var i = 0; i < Count; i++)
        {
            foreach (var seg in segments)
            {
                var result = SegmentDef.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], seg.Start, seg.End);
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