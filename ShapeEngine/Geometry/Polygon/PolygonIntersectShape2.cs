using System.Numerics;
using ShapeEngine.Geometry.CollisionSystem;

namespace ShapeEngine.Geometry.Polygon;

public partial class Polygon
{
    public static int IntersectPolygonRay(List<Vector2> polygon, Vector2 rayPoint, Vector2 rayDirection, ref CollisionPoints result)
    {
        if (polygon.Count < 3) return 0;
        int count = result.Count;
        for (var i = 0; i < polygon.Count; i++)
        {
            var point = Segment.Segment.IntersectSegmentRay(polygon[i], polygon[(i + 1) % polygon.Count], rayPoint, rayDirection);
            if (point.Valid)
            {
                result.Add(point);
            }
        }

        return result.Count - count;
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
        if (Count < 3) return 0;
        var count = 0;
        for (var i = 0; i < Count; i++)
        {
            var result = Segment.Segment.IntersectSegmentRay(this[i], this[(i + 1) % Count], r.Point, r.Direction, r.Normal);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }

        return count;
    }

    public int IntersectShape(Line.Line l, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 3) return 0;
        var count = 0;
        for (var i = 0; i < Count; i++)
        {
            var result = Segment.Segment.IntersectSegmentLine(this[i], this[(i + 1) % Count], l.Point, l.Direction, l.Normal);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }

        return count;
    }

    public int IntersectShape(Segment.Segment s, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 3) return 0;
        var count = 0;
        for (var i = 0; i < Count; i++)
        {
            var result = Segment.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], s.Start, s.End);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }

        return count;
    }

    public int IntersectShape(Circle.Circle c, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 3) return 0;

        var count = 0;

        for (var i = 0; i < Count; i++)
        {
            var result = Segment.Segment.IntersectSegmentCircle(this[i], this[(i + 1) % Count], c.Center, c.Radius);
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
        }

        return count;
    }

    public int IntersectShape(Triangle.Triangle t, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 3) return 0;

        var count = 0;
        for (var i = 0; i < Count; i++)
        {
            var result = Segment.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], t.A, t.B);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], t.B, t.C);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], t.C, t.A);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }

        return count;
    }

    public int IntersectShape(Quad.Quad q, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 3) return 0;

        var count = 0;
        for (var i = 0; i < Count; i++)
        {
            var result = Segment.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], q.A, q.B);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], q.B, q.C);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], q.C, q.D);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], q.D, q.A);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }

        return count;
    }

    public int IntersectShape(Rect.Rect r, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 3) return 0;

        var count = 0;

        var a = r.TopLeft;
        var b = r.BottomLeft;
        var c = r.BottomRight;
        var d = r.TopRight;

        for (var i = 0; i < Count; i++)
        {
            var result = Segment.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], a, b);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], b, c);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], c, d);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], d, a);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }

        return count;
    }

    public int IntersectShape(Polygon p, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 3 || p.Count < 3) return 0;
        var count = 0;
        for (var i = 0; i < Count; i++)
        {
            for (var j = 0; j < p.Count; j++)
            {
                var result = Segment.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], p[j], p[(j + 1) % p.Count]);
                if (result.Valid)
                {
                    points.Add(result);
                    if (returnAfterFirstValid) return 1;
                    count++;
                }
            }
        }

        return count;
    }

    public int IntersectShape(Polyline.Polyline pl, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 3 || pl.Count < 2) return 0;
        var count = 0;
        for (var i = 0; i < Count; i++)
        {
            for (var j = 0; j < pl.Count - 1; j++)
            {
                var result = Segment.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], pl[j], pl[(j + 1) % pl.Count]);
                if (result.Valid)
                {
                    points.Add(result);
                    if (returnAfterFirstValid) return 1;
                    count++;
                }
            }
        }

        return count;
    }

    public int IntersectShape(Segments.Segments shape, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 3 || shape.Count <= 0) return 0;
        var count = 0;
        for (var i = 0; i < Count; i++)
        {
            foreach (var seg in shape)
            {
                var result = Segment.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], seg.Start, seg.End);
                if (result.Valid)
                {
                    points.Add(result);
                    if (returnAfterFirstValid) return 1;
                    count++;
                }
            }
        }

        return count;
    }

}