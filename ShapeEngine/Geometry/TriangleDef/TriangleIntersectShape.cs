using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;

namespace ShapeEngine.Geometry.TriangleDef;

public readonly partial struct Triangle
{
    /// <summary>
    /// Tests for intersection between this triangle and a collider, returning collision points if found.
    /// </summary>
    /// <param name="collider">The collider to test intersection with.</param>
    /// <returns>A CollisionPoints object containing intersection data if intersections are found; otherwise, null.</returns>
    /// <remarks>
    /// This method determines the shape type of the collider and delegates to the appropriate
    /// shape-specific intersection method. If the collider is disabled, this method returns null.
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
    /// Tests for intersection between this triangle and a collection of line segments.
    /// </summary>
    /// <param name="segments">The collection of segments to test intersection with.</param>
    /// <returns>A CollisionPoints object containing intersection data if intersections are found; otherwise, null.</returns>
    /// <remarks>
    /// This method tests intersection with each segment in the collection and combines all intersection points.
    /// </remarks>
    public CollisionPoints? IntersectShape(Segments segments)
    {
        if (segments == null) throw new ArgumentNullException(nameof(segments));
        if (segments.Count <= 0) return null;

        CollisionPoints? points = null;

        foreach (var seg in segments)
        {
            var result = Segment.IntersectSegmentSegment(A, B, seg.Start, seg.End);
            if (result.Valid)
            {
                points ??= [];
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(B, C, seg.Start, seg.End);
            if (result.Valid)
            {
                points ??= [];
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(C, A, seg.Start, seg.End);
            if (result.Valid)
            {
                points ??= [];
                points.Add(result);
            }
        }

        return points;
    }

    /// <summary>
    /// Tests for intersection between this triangle and a ray.
    /// </summary>
    /// <param name="r">The ray to test intersection with.</param>
    /// <returns>A CollisionPoints object containing intersection data if intersections are found; otherwise, null.</returns>
    /// <remarks>
    /// This method checks intersection with the ray against all three edges of the triangle.
    /// A ray can intersect a triangle at most at two points.
    /// </remarks>
    public CollisionPoints? IntersectShape(Ray r)
    {
        CollisionPoints? points = null;
        var result = Segment.IntersectSegmentRay(A, B, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentRay(B, C, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentRay(C, A, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        return points;
    }

    /// <summary>
    /// Tests for intersection between this triangle and a line.
    /// </summary>
    /// <param name="l">The line to test intersection with.</param>
    /// <returns>A CollisionPoints object containing intersection data if intersections are found; otherwise, null.</returns>
    /// <remarks>
    /// This method checks intersection with the infinite line against all three edges of the triangle.
    /// A line can intersect a triangle at most at two points.
    /// </remarks>
    public CollisionPoints? IntersectShape(Line l)
    {
        CollisionPoints? points = null;
        var result = Segment.IntersectSegmentLine(A, B, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentLine(B, C, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentLine(C, A, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        return points;
    }

    /// <summary>
    /// Tests for intersection between this triangle and a line segment.
    /// </summary>
    /// <param name="s">The segment to test intersection with.</param>
    /// <returns>A CollisionPoints object containing intersection data if intersections are found; otherwise, null.</returns>
    /// <remarks>
    /// This method checks intersection with the line segment against all three edges of the triangle.
    /// A segment can intersect a triangle at most at two points.
    /// </remarks>
    public CollisionPoints? IntersectShape(Segment s)
    {
        CollisionPoints? points = null;
        var result = Segment.IntersectSegmentSegment(A, B, s.Start, s.End);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(B, C, s.Start, s.End);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(C, A, s.Start, s.End);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        return points;
    }

    /// <summary>
    /// Tests for intersection between this triangle and a circle.
    /// </summary>
    /// <param name="c">The circle to test intersection with.</param>
    /// <returns>A CollisionPoints object containing intersection data if intersections are found; otherwise, null.</returns>
    /// <remarks>
    /// This method checks intersection with the circle against all three edges of the triangle.
    /// A circle can intersect multiple edges of a triangle, potentially creating many intersection points.
    /// </remarks>
    public CollisionPoints? IntersectShape(Circle c)
    {
        CollisionPoints? points = null;
        var result = Segment.IntersectSegmentCircle(A, B, c.Center, c.Radius);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if (result.a.Valid) points.Add(result.a);
            if (result.b.Valid) points.Add(result.b);
        }

        result = Segment.IntersectSegmentCircle(B, C, c.Center, c.Radius);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if (result.a.Valid) points.Add(result.a);
            if (result.b.Valid) points.Add(result.b);
        }

        result = Segment.IntersectSegmentCircle(C, A, c.Center, c.Radius);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if (result.a.Valid) points.Add(result.a);
            if (result.b.Valid) points.Add(result.b);
        }

        return points;
    }

    /// <summary>
    /// Tests for intersection between this triangle and another triangle.
    /// </summary>
    /// <param name="t">The triangle to test intersection with.</param>
    /// <returns>A CollisionPoints object containing intersection data if intersections are found; otherwise, null.</returns>
    /// <remarks>
    /// This method performs a comprehensive intersection test by checking all edges of both triangles
    /// against each other. Two triangles can have complex intersection patterns with multiple points.
    /// </remarks>
    public CollisionPoints? IntersectShape(Triangle t)
    {
        CollisionPoints? points = null;
        var result = Segment.IntersectSegmentSegment(A, B, t.A, t.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(A, B, t.B, t.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(A, B, t.C, t.A);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }


        result = Segment.IntersectSegmentSegment(B, C, t.A, t.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(B, C, t.B, t.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(B, C, t.C, t.A);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(C, A, t.A, t.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(C, A, t.B, t.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(C, A, t.C, t.A);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        return points;
    }

    /// <summary>
    /// Tests for intersection between this triangle and a rectangle.
    /// </summary>
    /// <param name="r">The rectangle to test intersection with.</param>
    /// <returns>A CollisionPoints object containing intersection data if intersections are found; otherwise, null.</returns>
    /// <remarks>
    /// This method checks intersection by testing all triangle edges against all rectangle edges.
    /// </remarks>
    public CollisionPoints? IntersectShape(Rect r)
    {
        CollisionPoints? points = null;
        var a = r.TopLeft;
        var b = r.BottomLeft;
        var result = Segment.IntersectSegmentSegment(A, B, a, b);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        var c = r.BottomRight;
        result = Segment.IntersectSegmentSegment(A, B, b, c);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        var d = r.TopRight;
        result = Segment.IntersectSegmentSegment(A, B, c, d);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(A, B, d, a);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }


        result = Segment.IntersectSegmentSegment(B, C, a, b);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(B, C, b, c);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(B, C, c, d);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(B, C, d, a);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }


        result = Segment.IntersectSegmentSegment(C, A, a, b);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(C, A, b, c);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(C, A, c, d);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(C, A, d, a);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        return points;
    }

    /// <summary>
    /// Tests for intersection between this triangle and a quadrilateral.
    /// </summary>
    /// <param name="q">The quad to test intersection with.</param>
    /// <returns>A CollisionPoints object containing intersection data if intersections are found; otherwise, null.</returns>
    /// <remarks>
    /// This method checks intersection by testing all triangle edges against all quadrilateral edges.
    /// </remarks>
    public CollisionPoints? IntersectShape(Quad q)
    {
        CollisionPoints? points = null;
        var result = Segment.IntersectSegmentSegment(A, B, q.A, q.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(A, B, q.B, q.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(A, B, q.C, q.D);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(A, B, q.D, q.A);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }


        result = Segment.IntersectSegmentSegment(B, C, q.A, q.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(B, C, q.B, q.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(B, C, q.C, q.D);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(B, C, q.D, q.A);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }


        result = Segment.IntersectSegmentSegment(C, A, q.A, q.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(C, A, q.B, q.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(C, A, q.C, q.D);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(C, A, q.D, q.A);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        return points;
    }

    /// <summary>
    /// Tests for intersection between this triangle and a polygon.
    /// </summary>
    /// <param name="p">The polygon to test intersection with.</param>
    /// <returns>A CollisionPoints object containing intersection data if intersections are found; otherwise, null.</returns>
    /// <remarks>
    /// This method checks intersection by testing all triangle edges against all polygon edges.
    /// The polygon can have any number of vertices.
    /// </remarks>
    public CollisionPoints? IntersectShape(Polygon p)
    {
        if (p.Count < 3) return null;

        CollisionPoints? points = null;
        for (var i = 0; i < p.Count; i++)
        {
            var colPoint = Segment.IntersectSegmentSegment(A, B, p[i], p[(i + 1) % p.Count]);
            if (colPoint.Valid)
            {
                points ??= new();
                points.Add(colPoint);
            }

            colPoint = Segment.IntersectSegmentSegment(B, C, p[i], p[(i + 1) % p.Count]);
            if (colPoint.Valid)
            {
                points ??= new();
                points.Add(colPoint);
            }

            colPoint = Segment.IntersectSegmentSegment(C, A, p[i], p[(i + 1) % p.Count]);
            if (colPoint.Valid)
            {
                points ??= new();
                points.Add(colPoint);
            }
        }

        return points;
    }

    /// <summary>
    /// Tests for intersection between this triangle and a polyline.
    /// </summary>
    /// <param name="pl">The polyline to test intersection with.</param>
    /// <returns>A CollisionPoints object containing intersection data if intersections are found; otherwise, null.</returns>
    /// <remarks>
    /// This method checks intersection by testing all triangle edges against all polyline segments.
    /// Unlike polygons, polylines are not closed shapes.
    /// </remarks>
    public CollisionPoints? IntersectShape(Polyline pl)
    {
        if (pl.Count < 2) return null;

        CollisionPoints? points = null;
        for (var i = 0; i < pl.Count - 1; i++)
        {
            CollisionPoint colPoint = Segment.IntersectSegmentSegment(A, B, pl[i], pl[(i + 1) % pl.Count]);
            if (colPoint.Valid)
            {
                points ??= new();
                points.Add(colPoint);
            }

            colPoint = Segment.IntersectSegmentSegment(B, C, pl[i], pl[(i + 1) % pl.Count]);
            if (colPoint.Valid)
            {
                points ??= new();
                points.Add(colPoint);
            }

            colPoint = Segment.IntersectSegmentSegment(C, A, pl[i], pl[(i + 1) % pl.Count]);
            if (colPoint.Valid)
            {
                points ??= new();
                points.Add(colPoint);
            }
        }

        return points;
    }

    /// <summary>
    /// Tests for intersection between this triangle and a collider, adding intersection points to an existing collection.
    /// </summary>
    /// <param name="collider">The collider to test intersection with.</param>
    /// <param name="points">The collection to add intersection points to.</param>
    /// <param name="returnAfterFirstValid">If true, returns immediately after finding the first valid intersection.</param>
    /// <returns>The number of intersection points found and added to the collection.</returns>
    /// <remarks>
    /// This performance-optimized method avoids allocating new CollisionPoints objects by reusing an existing collection.
    /// Useful for high-frequency intersection testing where garbage collection pressure should be minimized.
    /// </remarks>
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

    /// <summary>
    /// Tests for intersection between this triangle and a ray, adding intersection points to an existing collection.
    /// </summary>
    /// <param name="r">The ray to test intersection with.</param>
    /// <param name="points">The collection to add intersection points to.</param>
    /// <param name="returnAfterFirstValid">If true, returns immediately after finding the first valid intersection.</param>
    /// <returns>The number of intersection points found and added to the collection.</returns>
    /// <remarks>
    /// This performance-optimized method avoids allocating new CollisionPoints objects.
    /// </remarks>
    public int IntersectShape(Ray r, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        var result = Segment.IntersectSegmentRay(A, B, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentRay(B, C, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (count >= 2) return count;

        result = Segment.IntersectSegmentRay(C, A, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }

        return count;
    }

    /// <summary>
    /// Tests for intersection between this triangle and a line, adding intersection points to an existing collection.
    /// </summary>
    /// <param name="l">The line to test intersection with.</param>
    /// <param name="points">The collection to add intersection points to.</param>
    /// <param name="returnAfterFirstValid">If true, returns immediately after finding the first valid intersection.</param>
    /// <returns>The number of intersection points found and added to the collection.</returns>
    /// <remarks>
    /// This performance-optimized method avoids allocating new CollisionPoints objects.
    /// </remarks>
    public int IntersectShape(Line l, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        var result = Segment.IntersectSegmentLine(A, B, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentLine(B, C, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (count >= 2) return count;

        result = Segment.IntersectSegmentLine(C, A, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }

        return count;
    }

    /// <summary>
    /// Tests for intersection between this triangle and a line segment, adding intersection points to an existing collection.
    /// </summary>
    /// <param name="s">The segment to test intersection with.</param>
    /// <param name="points">The collection to add intersection points to.</param>
    /// <param name="returnAfterFirstValid">If true, returns immediately after finding the first valid intersection.</param>
    /// <returns>The number of intersection points found and added to the collection.</returns>
    /// <remarks>
    /// This performance-optimized method avoids allocating new CollisionPoints objects.
    /// </remarks>
    public int IntersectShape(Segment s, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        var result = Segment.IntersectSegmentSegment(A, B, s.Start, s.End);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(B, C, s.Start, s.End);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (count >= 2) return count;

        result = Segment.IntersectSegmentSegment(C, A, s.Start, s.End);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }

        return count;
    }

    /// <summary>
    /// Tests for intersection between this triangle and a circle, adding intersection points to an existing collection.
    /// </summary>
    /// <param name="c">The circle to test intersection with.</param>
    /// <param name="points">The collection to add intersection points to.</param>
    /// <param name="returnAfterFirstValid">If true, returns immediately after finding the first valid intersection.</param>
    /// <returns>The number of intersection points found and added to the collection.</returns>
    /// <remarks>
    /// This performance-optimized method avoids allocating new CollisionPoints objects.
    /// </remarks>
    public int IntersectShape(Circle c, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        var result = Segment.IntersectSegmentCircle(A, B, c.Center, c.Radius);
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

        result = Segment.IntersectSegmentCircle(B, C, c.Center, c.Radius);
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

        result = Segment.IntersectSegmentCircle(C, A, c.Center, c.Radius);
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
    /// Tests for intersection between this triangle and another triangle, adding intersection points to an existing collection.
    /// </summary>
    /// <param name="t">The triangle to test intersection with.</param>
    /// <param name="points">The collection to add intersection points to.</param>
    /// <param name="returnAfterFirstValid">If true, returns immediately after finding the first valid intersection.</param>
    /// <returns>The number of intersection points found and added to the collection.</returns>
    /// <remarks>
    /// This performance-optimized method avoids allocating new CollisionPoints objects.
    /// </remarks>
    public int IntersectShape(Triangle t, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        var result = Segment.IntersectSegmentSegment(A, B, t.A, t.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(A, B, t.B, t.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(A, B, t.C, t.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }


        result = Segment.IntersectSegmentSegment(B, C, t.A, t.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(B, C, t.B, t.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(B, C, t.C, t.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(C, A, t.A, t.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(C, A, t.B, t.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(C, A, t.C, t.A);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }

        return count;
    }

    /// <summary>
    /// Tests for intersection between this triangle and a quadrilateral, adding intersection points to an existing collection.
    /// </summary>
    /// <param name="q">The quad to test intersection with.</param>
    /// <param name="points">The collection to add intersection points to.</param>
    /// <param name="returnAfterFirstValid">If true, returns immediately after finding the first valid intersection.</param>
    /// <returns>The number of intersection points found and added to the collection.</returns>
    /// <remarks>
    /// This performance-optimized method avoids allocating new CollisionPoints objects.
    /// </remarks>
    public int IntersectShape(Quad q, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        var result = Segment.IntersectSegmentSegment(A, B, q.A, q.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(A, B, q.B, q.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(A, B, q.C, q.D);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(A, B, q.D, q.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }


        result = Segment.IntersectSegmentSegment(B, C, q.A, q.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(B, C, q.B, q.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(B, C, q.C, q.D);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(B, C, q.D, q.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }


        result = Segment.IntersectSegmentSegment(C, A, q.A, q.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(C, A, q.B, q.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(C, A, q.C, q.D);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(C, A, q.D, q.A);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }

        return count;
    }

    /// <summary>
    /// Tests for intersection between this triangle and a rectangle, adding intersection points to an existing collection.
    /// </summary>
    /// <param name="r">The rectangle to test intersection with.</param>
    /// <param name="points">The collection to add intersection points to.</param>
    /// <param name="returnAfterFirstValid">If true, returns immediately after finding the first valid intersection.</param>
    /// <returns>The number of intersection points found and added to the collection.</returns>
    /// <remarks>
    /// This performance-optimized method avoids allocating new CollisionPoints objects.
    /// </remarks>
    public int IntersectShape(Rect r, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        var a = r.TopLeft;
        var b = r.BottomLeft;
        var result = Segment.IntersectSegmentSegment(A, B, a, b);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        var c = r.BottomRight;
        result = Segment.IntersectSegmentSegment(A, B, b, c);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        var d = r.TopRight;
        result = Segment.IntersectSegmentSegment(A, B, c, d);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(A, B, d, a);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }


        result = Segment.IntersectSegmentSegment(B, C, a, b);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(B, C, b, c);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(B, C, c, d);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(B, C, d, a);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }


        result = Segment.IntersectSegmentSegment(C, A, a, b);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(C, A, b, c);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(C, A, c, d);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(C, A, d, a);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }

        return count;
    }

    /// <summary>
    /// Tests for intersection between this triangle and a polygon, adding intersection points to an existing collection.
    /// </summary>
    /// <param name="p">The polygon to test intersection with.</param>
    /// <param name="points">The collection to add intersection points to.</param>
    /// <param name="returnAfterFirstValid">If true, returns immediately after finding the first valid intersection.</param>
    /// <returns>The number of intersection points found and added to the collection.</returns>
    /// <remarks>
    /// This performance-optimized method avoids allocating new CollisionPoints objects.
    /// </remarks>
    public int IntersectShape(Polygon p, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (p.Count < 3) return 0;

        var count = 0;
        for (var i = 0; i < p.Count; i++)
        {
            var result = Segment.IntersectSegmentSegment(A, B, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.IntersectSegmentSegment(B, C, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.IntersectSegmentSegment(C, A, p[i], p[(i + 1) % p.Count]);
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
    /// Tests for intersection between this triangle and a polyline, adding intersection points to an existing collection.
    /// </summary>
    /// <param name="pl">The polyline to test intersection with.</param>
    /// <param name="points">The collection to add intersection points to.</param>
    /// <param name="returnAfterFirstValid">If true, returns immediately after finding the first valid intersection.</param>
    /// <returns>The number of intersection points found and added to the collection.</returns>
    /// <remarks>
    /// This performance-optimized method avoids allocating new CollisionPoints objects.
    /// </remarks>
    public int IntersectShape(Polyline pl, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (pl.Count < 2) return 0;

        var count = 0;
        for (var i = 0; i < pl.Count - 1; i++)
        {
            var result = Segment.IntersectSegmentSegment(A, B, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.IntersectSegmentSegment(B, C, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.IntersectSegmentSegment(C, A, pl[i], pl[(i + 1) % pl.Count]);
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
    /// Tests for intersection between this triangle and a collection of line segments, adding intersection points to an existing collection.
    /// </summary>
    /// <param name="shape">The collection of segments to test intersection with.</param>
    /// <param name="points">The collection to add intersection points to.</param>
    /// <param name="returnAfterFirstValid">If true, returns immediately after finding the first valid intersection.</param>
    /// <returns>The number of intersection points found and added to the collection.</returns>
    /// <remarks>
    /// This performance-optimized method avoids allocating new CollisionPoints objects by reusing an existing collection.
    /// Useful for high-frequency intersection testing where garbage collection pressure should be minimized.
    /// </remarks>
    public int IntersectShape(Segments shape, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (shape.Count <= 0) return 0;

        var count = 0;

        foreach (var seg in shape)
        {
            var result = Segment.IntersectSegmentSegment(A, B, seg.Start, seg.End);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.IntersectSegmentSegment(B, C, seg.Start, seg.End);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.IntersectSegmentSegment(C, A, seg.Start, seg.End);
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