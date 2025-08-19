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
    /// Computes the number of intersection points between this rectangle and the specified collider, adding them to the provided <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="collider">The collider to test for intersection.</param>
    /// <param name="points">A reference to the <see cref="IntersectionPoints"/> collection to which intersection points will be added.</param>
    /// <param name="returnAfterFirstValid">If true, returns after the first valid intersection is found; otherwise, finds all intersections.</param>
    /// <returns>The number of intersection points found.</returns>
    /// <remarks>
    /// Dispatches to the appropriate shape-specific intersection method based on the collider's shape type.
    /// </remarks>
    public int Intersect(Collider collider, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (!collider.Enabled) return 0;

        switch (collider.GetShapeType())
        {
            case ShapeType.Circle:
                var c = collider.GetCircleShape();
                return IntersectShape(c, ref points, returnAfterFirstValid);
            case ShapeType.Ray:
                var rayShape = collider.GetRayShape();
                return IntersectShape(rayShape, ref points, returnAfterFirstValid);
            case ShapeType.Line:
                var l = collider.GetLineShape();
                return IntersectShape(l, ref points, returnAfterFirstValid);
            case ShapeType.Segment:
                var s = collider.GetSegmentShape();
                return IntersectShape(s, ref points, returnAfterFirstValid);
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

    /// <summary>
    /// Computes the number of intersection points between this rectangle and a ray, adding them to the provided <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="r">The ray to test for intersection.</param>
    /// <param name="points">A reference to the <see cref="IntersectionPoints"/> collection to which intersection points will be added.</param>
    /// <param name="returnAfterFirstValid">If true, returns after the first valid intersection is found; otherwise, finds all intersections.</param>
    /// <returns>The number of intersection points found.</returns>
    /// <remarks>
    /// Each side of the rectangle is tested against the ray.
    /// </remarks>
    public int IntersectShape(Ray r, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;

        var result = Segment.IntersectSegmentRay(a, b, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentRay(b, c, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (count >= 2) return count;

        result = Segment.IntersectSegmentRay(c, d, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (count >= 2) return count;

        result = Segment.IntersectSegmentRay(d, a, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }

        return count;
    }

    /// <summary>
    /// Computes the number of intersection points between this rectangle and a line, adding them to the provided <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="l">The line to test for intersection.</param>
    /// <param name="points">A reference to the <see cref="IntersectionPoints"/> collection to which intersection points will be added.</param>
    /// <param name="returnAfterFirstValid">If true, returns after the first valid intersection is found; otherwise, finds all intersections.</param>
    /// <returns>The number of intersection points found.</returns>
    /// <remarks>
    /// Each side of the rectangle is tested against the line.
    /// </remarks>
    public int IntersectShape(Line l, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;

        var result = Segment.IntersectSegmentLine(a, b, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentLine(b, c, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (count >= 2) return count;

        result = Segment.IntersectSegmentLine(c, d, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (count >= 2) return count;

        result = Segment.IntersectSegmentLine(d, a, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }

        return count;
    }

    /// <summary>
    /// Computes the number of intersection points between this rectangle and a segment, adding them to the provided <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="s">The segment to test for intersection.</param>
    /// <param name="points">A reference to the <see cref="IntersectionPoints"/> collection to which intersection points will be added.</param>
    /// <param name="returnAfterFirstValid">If true, returns after the first valid intersection is found; otherwise, finds all intersections.</param>
    /// <returns>The number of intersection points found.</returns>
    /// <remarks>
    /// Each side of the rectangle is tested against the segment.
    /// </remarks>
    public int IntersectShape(Segment s, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;

        var result = Segment.IntersectSegmentSegment(a, b, s.Start, s.End);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(b, c, s.Start, s.End);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (count >= 2) return count;

        result = Segment.IntersectSegmentSegment(c, d, s.Start, s.End);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (count >= 2) return count;

        result = Segment.IntersectSegmentSegment(d, a, s.Start, s.End);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }

        return count;
    }

    /// <summary>
    /// Computes the number of intersection points between this rectangle and a circle, adding them to the provided <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="circle">The circle to test for intersection.</param>
    /// <param name="points">A reference to the <see cref="IntersectionPoints"/> collection to which intersection points will be added.</param>
    /// <param name="returnAfterFirstValid">If true, returns after the first valid intersection is found; otherwise, finds all intersections.</param>
    /// <returns>The number of intersection points found.</returns>
    /// <remarks>
    /// Each side of the rectangle is tested against the circle.
    /// </remarks>
    public int IntersectShape(Circle circle, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;

        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;

        var result = Segment.IntersectSegmentCircle(a, b, circle.Center, circle.Radius);
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

        result = Segment.IntersectSegmentCircle(b, c, circle.Center, circle.Radius);
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

        result = Segment.IntersectSegmentCircle(c, d, circle.Center, circle.Radius);
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

        result = Segment.IntersectSegmentCircle(d, a, circle.Center, circle.Radius);
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

    /// <summary>
    /// Computes the number of intersection points between this rectangle and a triangle, adding them to the provided <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="t">The triangle to test for intersection.</param>
    /// <param name="points">A reference to the <see cref="IntersectionPoints"/> collection to which intersection points will be added.</param>
    /// <param name="returnAfterFirstValid">If true, returns after the first valid intersection is found; otherwise, finds all intersections.</param>
    /// <returns>The number of intersection points found.</returns>
    /// <remarks>
    /// Each side of the rectangle is tested against the triangle's edges.
    /// </remarks>
    public int IntersectShape(Triangle t, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;

        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;

        var result = Segment.IntersectSegmentSegment(a, b, t.A, t.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(a, b, t.B, t.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(a, b, t.C, t.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }


        result = Segment.IntersectSegmentSegment(b, c, t.A, t.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(b, c, t.B, t.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(b, c, t.C, t.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }


        result = Segment.IntersectSegmentSegment(c, d, t.A, t.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(c, d, t.B, t.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(c, d, t.C, t.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(d, a, t.A, t.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(d, a, t.B, t.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(d, a, t.C, t.A);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }

        return count;
    }

    /// <summary>
    /// Computes the number of intersection points between this rectangle and a quad, adding them to the provided <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="q">The quad to test for intersection.</param>
    /// <param name="points">A reference to the <see cref="IntersectionPoints"/> collection to which intersection points will be added.</param>
    /// <param name="returnAfterFirstValid">If true, returns after the first valid intersection is found; otherwise, finds all intersections.</param>
    /// <returns>The number of intersection points found.</returns>
    /// <remarks>
    /// Each side of the rectangle is tested against the quad's edges.
    /// </remarks>
    public int IntersectShape(Quad q, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;
        var result = Segment.IntersectSegmentSegment(a, b, q.A, q.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(a, b, q.B, q.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(a, b, q.C, q.D);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(a, b, q.D, q.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }


        result = Segment.IntersectSegmentSegment(b, c, q.A, q.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(b, c, q.B, q.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(b, c, q.C, q.D);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(b, c, q.D, q.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }


        result = Segment.IntersectSegmentSegment(c, d, q.A, q.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(c, d, q.B, q.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(c, d, q.C, q.D);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(c, d, q.D, q.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(d, a, q.A, q.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(d, a, q.B, q.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(d, a, q.C, q.D);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(d, a, q.D, q.A);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }

        return count;
    }

    /// <summary>
    /// Computes the number of intersection points between this rectangle and another rectangle, adding them to the provided <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="r">The rectangle to test for intersection.</param>
    /// <param name="points">A reference to the <see cref="IntersectionPoints"/> collection to which intersection points will be added.</param>
    /// <param name="returnAfterFirstValid">If true, returns after the first valid intersection is found; otherwise, finds all intersections.</param>
    /// <returns>The number of intersection points found.</returns>
    /// <remarks>
    /// Each side of the rectangle is tested against the other rectangle's sides.
    /// </remarks>
    public int IntersectShape(Rect r, ref IntersectionPoints points, bool returnAfterFirstValid = false)
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

        var result = Segment.IntersectSegmentSegment(a, b, rA, rB);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }


        result = Segment.IntersectSegmentSegment(a, b, rB, rC);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }


        result = Segment.IntersectSegmentSegment(a, b, rC, rD);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(a, b, rD, rA);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }


        result = Segment.IntersectSegmentSegment(b, c, rA, rB);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(b, c, rB, rC);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(b, c, rC, rD);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(b, c, rD, rA);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }


        result = Segment.IntersectSegmentSegment(c, d, rA, rB);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(c, d, rB, rC);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(c, d, rC, rD);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(c, d, rD, rA);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }


        result = Segment.IntersectSegmentSegment(d, a, rA, rB);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(d, a, rB, rC);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(d, a, rC, rD);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(d, a, rD, rA);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }

        return count;
    }

    /// <summary>
    /// Computes the number of intersection points between this rectangle and a polygon, adding them to the provided <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="p">The polygon to test for intersection.</param>
    /// <param name="points">A reference to the <see cref="IntersectionPoints"/> collection to which intersection points will be added.</param>
    /// <param name="returnAfterFirstValid">If true, returns after the first valid intersection is found; otherwise, finds all intersections.</param>
    /// <returns>The number of intersection points found.</returns>
    /// <remarks>
    /// Each side of the rectangle is tested against the polygon's edges.
    /// </remarks>
    public int IntersectShape(Polygon p, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (p.Count < 3) return 0;

        var count = 0;

        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;

        for (var i = 0; i < p.Count; i++)
        {
            var result = Segment.IntersectSegmentSegment(a, b, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.IntersectSegmentSegment(b, c, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.IntersectSegmentSegment(c, d, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.IntersectSegmentSegment(d, a, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Computes the number of intersection points between this rectangle and a polyline, adding them to the provided <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="pl">The polyline to test for intersection.</param>
    /// <param name="points">A reference to the <see cref="IntersectionPoints"/> collection to which intersection points will be added.</param>
    /// <param name="returnAfterFirstValid">If true, returns after the first valid intersection is found; otherwise, finds all intersections.</param>
    /// <returns>The number of intersection points found.</returns>
    /// <remarks>
    /// Each side of the rectangle is tested against the polyline's segments.
    /// </remarks>
    public int IntersectShape(Polyline pl, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (pl.Count < 2) return 0;

        var count = 0;

        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;
        for (var i = 0; i < pl.Count - 1; i++)
        {
            var result = Segment.IntersectSegmentSegment(a, b, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.IntersectSegmentSegment(b, c, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.IntersectSegmentSegment(c, d, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.IntersectSegmentSegment(d, a, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Computes the number of intersection points between this rectangle and a collection of segments, adding them to the provided <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="shape">The collection of segments to test for intersection.</param>
    /// <param name="points">A reference to the <see cref="IntersectionPoints"/> collection to which intersection points will be added.</param>
    /// <param name="returnAfterFirstValid">If true, returns after the first valid intersection is found; otherwise, finds all intersections.</param>
    /// <returns>The number of intersection points found.</returns>
    /// <remarks>
    /// Each side of the rectangle is tested against each segment in the collection.
    /// </remarks>
    public int IntersectShape(Segments shape, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (shape.Count <= 0) return 0;

        var count = 0;

        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;

        foreach (var seg in shape)
        {
            var result = Segment.IntersectSegmentSegment(a, b, seg.Start, seg.End);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.IntersectSegmentSegment(b, c, seg.Start, seg.End);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.IntersectSegmentSegment(c, d, seg.Start, seg.End);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.IntersectSegmentSegment(d, a, seg.Start, seg.End);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }

        return count;
    }
    
    /// <summary>
    /// Computes the number of intersection points between this shape and a shape implementing <see cref="IShape"/>.
    /// </summary>
    /// <param name="shape">The shape to test against.</param>
    /// <param name="points">A reference to an <see cref="IntersectionPoints"/> collection to store intersection points.</param>
    /// <param name="returnAfterFirstValid">
    /// If true, the method returns after finding the first valid intersection point; otherwise, it finds all intersections.
    /// </param>
    /// <returns>The number of valid intersection points found.</returns>
    public int IntersectShape(IShape shape, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        return shape.GetShapeType() switch
        {
            ShapeType.Circle => IntersectShape(shape.GetCircleShape(), ref points, returnAfterFirstValid),
            ShapeType.Segment => IntersectShape(shape.GetSegmentShape(), ref points, returnAfterFirstValid),
            ShapeType.Ray => IntersectShape(shape.GetRayShape(), ref points, returnAfterFirstValid),
            ShapeType.Line => IntersectShape(shape.GetLineShape(), ref points, returnAfterFirstValid),
            ShapeType.Triangle => IntersectShape(shape.GetTriangleShape(), ref points, returnAfterFirstValid),
            ShapeType.Rect => IntersectShape(shape.GetRectShape(), ref points, returnAfterFirstValid),
            ShapeType.Quad => IntersectShape(shape.GetQuadShape(), ref points, returnAfterFirstValid),
            ShapeType.Poly => IntersectShape(shape.GetPolygonShape(), ref points, returnAfterFirstValid),
            ShapeType.PolyLine => IntersectShape(shape.GetPolylineShape(), ref points, returnAfterFirstValid),
            _ => 0
        };
    }
}