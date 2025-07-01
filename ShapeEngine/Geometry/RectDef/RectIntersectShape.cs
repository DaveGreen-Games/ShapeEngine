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

namespace ShapeEngine.Geometry.RectDef;

public readonly partial struct Rect
{
    /// <summary>
    /// Computes intersection points between this rectangle and the specified collider.
    /// </summary>
    /// <param name="collider">The collider to test for intersection.</param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing intersection points, or null if there are no intersections or the collider is disabled.
    /// </returns>
    /// <remarks>
    /// Dispatches to the appropriate shape-specific intersection method based on the collider's shape type.
    /// </remarks>
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

    /// <summary>
    /// Computes intersection points between this rectangle and a set of segments.
    /// </summary>
    /// <param name="segments">The segments to test for intersection.</param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing intersection points, or null if there are no intersections.
    /// </returns>
    /// <remarks>
    /// Each segment is tested against all four sides of the rectangle.
    /// </remarks>
    public CollisionPoints? IntersectShape(Segments segments)
    {
        if (segments.Count <= 0) return null;

        CollisionPoints? points = null;

        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;

        foreach (var seg in segments)
        {
            var result = Segment.IntersectSegmentSegment(a, b, seg.Start, seg.End);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(b, c, seg.Start, seg.End);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(c, d, seg.Start, seg.End);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(d, a, seg.Start, seg.End);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
        }

        return points;
    }

    /// <summary>
    /// Computes intersection points between this rectangle and a ray.
    /// </summary>
    /// <param name="r">The ray to test for intersection.</param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing intersection points, or null if there are no intersections.
    /// </returns>
    /// <remarks>
    /// Each side of the rectangle is tested against the ray.
    /// </remarks>
    public CollisionPoints? IntersectShape(Ray r)
    {
        CollisionPoints? points = null;
        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;

        var result = Segment.IntersectSegmentRay(a, b, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentRay(b, c, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentRay(c, d, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentRay(d, a, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        return points;
    }

    /// <summary>
    /// Computes intersection points between this rectangle and a line.
    /// </summary>
    /// <param name="l">The line to test for intersection.</param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing intersection points, or null if there are no intersections.
    /// </returns>
    /// <remarks>
    /// Each side of the rectangle is tested against the line.
    /// </remarks>
    public CollisionPoints? IntersectShape(Line l)
    {
        CollisionPoints? points = null;
        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;

        var result = Segment.IntersectSegmentLine(a, b, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentLine(b, c, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentLine(c, d, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentLine(d, a, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        return points;
    }

    /// <summary>
    /// Computes intersection points between this rectangle and a segment.
    /// </summary>
    /// <param name="s">The segment to test for intersection.</param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing intersection points, or null if there are no intersections.
    /// </returns>
    /// <remarks>
    /// Each side of the rectangle is tested against the segment.
    /// </remarks>
    public CollisionPoints? IntersectShape(Segment s)
    {
        CollisionPoints? points = null;
        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;

        var result = Segment.IntersectSegmentSegment(a, b, s.Start, s.End);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(b, c, s.Start, s.End);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(c, d, s.Start, s.End);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(d, a, s.Start, s.End);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        return points;
    }

    /// <summary>
    /// Computes intersection points between this rectangle and a circle.
    /// </summary>
    /// <param name="circle">The circle to test for intersection.</param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing intersection points, or null if there are no intersections.
    /// </returns>
    /// <remarks>
    /// Each side of the rectangle is tested against the circle.
    /// </remarks>
    public CollisionPoints? IntersectShape(Circle circle)
    {
        CollisionPoints? points = null;

        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;

        var result = Segment.IntersectSegmentCircle(a, b, circle.Center, circle.Radius);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if (result.a.Valid) points.Add(result.a);
            if (result.b.Valid) points.Add(result.b);
        }

        result = Segment.IntersectSegmentCircle(b, c, circle.Center, circle.Radius);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if (result.a.Valid) points.Add(result.a);
            if (result.b.Valid) points.Add(result.b);
        }

        result = Segment.IntersectSegmentCircle(c, d, circle.Center, circle.Radius);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if (result.a.Valid) points.Add(result.a);
            if (result.b.Valid) points.Add(result.b);
        }

        result = Segment.IntersectSegmentCircle(d, a, circle.Center, circle.Radius);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if (result.a.Valid) points.Add(result.a);
            if (result.b.Valid) points.Add(result.b);
        }

        return points;
    }

    /// <summary>
    /// Computes intersection points between this rectangle and a triangle.
    /// </summary>
    /// <param name="t">The triangle to test for intersection.</param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing intersection points, or null if there are no intersections.
    /// </returns>
    /// <remarks>
    /// Each side of the rectangle is tested against the triangle's edges.
    /// </remarks>
    public CollisionPoints? IntersectShape(Triangle t)
    {
        CollisionPoints? points = null;

        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;

        var result = Segment.IntersectSegmentSegment(a, b, t.A, t.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(a, b, t.B, t.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(a, b, t.C, t.A);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }


        result = Segment.IntersectSegmentSegment(b, c, t.A, t.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(b, c, t.B, t.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(b, c, t.C, t.A);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }


        result = Segment.IntersectSegmentSegment(c, d, t.A, t.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(c, d, t.B, t.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(c, d, t.C, t.A);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(d, a, t.A, t.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(d, a, t.B, t.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(d, a, t.C, t.A);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        return points;
    }

    /// <summary>
    /// Computes intersection points between this rectangle and another rectangle.
    /// </summary>
    /// <param name="r">The rectangle to test for intersection.</param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing intersection points, or null if there are no intersections.
    /// </returns>
    /// <remarks>
    /// Each side of the rectangle is tested against all four sides of the other rectangle.
    /// </remarks>
    public CollisionPoints? IntersectShape(Rect r)
    {
        CollisionPoints? points = null;
        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;

        var rA = r.TopLeft;
        var rB = r.BottomLeft;
        var rC = r.BottomRight;
        var rD = r.TopRight;

        var result = Segment.IntersectSegmentSegment(a, b, rA, rB);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }


        result = Segment.IntersectSegmentSegment(a, b, rB, rC);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }


        result = Segment.IntersectSegmentSegment(a, b, rC, rD);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(a, b, rD, rA);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }


        result = Segment.IntersectSegmentSegment(b, c, rA, rB);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(b, c, rB, rC);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(b, c, rC, rD);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(b, c, rD, rA);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }


        result = Segment.IntersectSegmentSegment(c, d, rA, rB);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(c, d, rB, rC);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(c, d, rC, rD);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(c, d, rD, rA);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }


        result = Segment.IntersectSegmentSegment(d, a, rA, rB);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(d, a, rB, rC);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(d, a, rC, rD);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(d, a, rD, rA);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        return points;
    }

    /// <summary>
    /// Computes intersection points between this rectangle and a quadrilateral.
    /// </summary>
    /// <param name="q">The quadrilateral to test for intersection.</param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing intersection points, or null if there are no intersections.
    /// </returns>
    /// <remarks>
    /// Each side of the rectangle is tested against all four sides of the quadrilateral.
    /// </remarks>
    public CollisionPoints? IntersectShape(Quad q)
    {
        CollisionPoints? points = null;
        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;
        var result = Segment.IntersectSegmentSegment(a, b, q.A, q.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(a, b, q.B, q.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(a, b, q.C, q.D);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(a, b, q.D, q.A);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }


        result = Segment.IntersectSegmentSegment(b, c, q.A, q.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(b, c, q.B, q.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(b, c, q.C, q.D);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(b, c, q.D, q.A);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }


        result = Segment.IntersectSegmentSegment(c, d, q.A, q.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(c, d, q.B, q.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(c, d, q.C, q.D);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(c, d, q.D, q.A);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(d, a, q.A, q.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(d, a, q.B, q.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(d, a, q.C, q.D);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(d, a, q.D, q.A);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        return points;
    }

    /// <summary>
    /// Computes intersection points between this rectangle and a polygon.
    /// </summary>
    /// <param name="p">The polygon to test for intersection.</param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing intersection points, or null if there are no intersections.
    /// </returns>
    /// <remarks>
    /// Each side of the rectangle is tested against each edge of the polygon.
    /// </remarks>
    public CollisionPoints? IntersectShape(Polygon p)
    {
        if (p.Count < 3) return null;

        CollisionPoints? points = null;

        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;

        for (var i = 0; i < p.Count; i++)
        {
            var result = Segment.IntersectSegmentSegment(a, b, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(b, c, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(c, d, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(d, a, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
        }

        return points;
    }

    /// <summary>
    /// Computes intersection points between this rectangle and a polyline.
    /// </summary>
    /// <param name="pl">The polyline to test for intersection.</param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing intersection points, or null if there are no intersections.
    /// </returns>
    /// <remarks>
    /// Each side of the rectangle is tested against each segment of the polyline.
    /// </remarks>
    public CollisionPoints? IntersectShape(Polyline pl)
    {
        if (pl.Count < 2) return null;

        CollisionPoints? points = null;

        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;
        for (var i = 0; i < pl.Count - 1; i++)
        {
            var result = Segment.IntersectSegmentSegment(a, b, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(b, c, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(c, d, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(d, a, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
        }

        return points;
    }
}