using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.SegmentDef;

public readonly partial struct Segment
{
    /// <summary>
    /// Computes intersection points between this segment  and all colliders in the specified <see cref="CollisionObject"/>.
    /// </summary>
    /// <param name="collisionObject">The collision object containing colliders to test for intersection.</param>
    /// <returns>
    /// A <see cref="Dictionary{Collider, IntersectionPoints}"/> mapping each collider to its intersection points,
    /// or null if no colliders are present or no intersections are found.
    /// </returns>
    public Dictionary<Collider, IntersectionPoints>? Intersect(CollisionObject collisionObject)
    {
        if (!collisionObject.HasColliders) return null;

        Dictionary<Collider, IntersectionPoints>? intersections = null;
        foreach (var collider in collisionObject.Colliders)
        {
            var result = Intersect(collider);
            if(result == null) continue;
            intersections ??= new();
            intersections.Add(collider, result);
        }
        return intersections;
    }
    /// <summary>
    /// Computes all intersection points between this segment and a collider shape.
    /// </summary>
    /// <param name="collider">The collider whose shape will be tested for intersection. Must be enabled.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection containing intersection points, or null if there are no intersections or the collider is disabled.</returns>
    /// <remarks>
    /// The method dispatches to the appropriate shape-specific intersection method based on the collider's shape type.
    /// </remarks>
    public IntersectionPoints? Intersect(Collider collider)
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
    /// Computes all intersection points between this segment and another segment.
    /// </summary>
    /// <param name="s">The segment to test against.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection containing intersection points, or null if there are no intersections.</returns>
    /// <remarks>
    /// Returns a single intersection point if the segments intersect, otherwise null.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Segment s)
    {
        var cp = IntersectSegmentSegment(Start, End, s.Start, s.End, s.Normal);
        if (cp.Valid) return [cp];

        return null;
    }

    /// <summary>
    /// Computes all intersection points between this segment and a line.
    /// </summary>
    /// <param name="l">The line to test against.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection containing intersection points, or null if there are no intersections.</returns>
    /// <remarks>
    /// Returns a single intersection point if the segment and line intersect, otherwise null.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Line l)
    {
        var cp = IntersectSegmentLine(Start, End, l.Point, l.Direction, l.Normal);
        if (cp.Valid) return [cp];

        return null;
    }

    /// <summary>
    /// Computes all intersection points between this segment and a ray.
    /// </summary>
    /// <param name="r">The ray to test against.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection containing intersection points, or null if there are no intersections.</returns>
    /// <remarks>
    /// Returns a single intersection point if the segment and ray intersect, otherwise null.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Ray r)
    {
        var cp = IntersectSegmentRay(Start, End, r.Point, r.Direction, r.Normal);
        if (cp.Valid) return [cp];

        return null;
    }

    /// <summary>
    /// Computes all intersection points between this segment and a circle.
    /// </summary>
    /// <param name="c">The circle to test against.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection containing intersection points, or null if there are no intersections.</returns>
    /// <remarks>
    /// May return up to two intersection points depending on the segment's position relative to the circle.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Circle c)
    {
        var result = IntersectSegmentCircle(Start, End, c.Center, c.Radius);

        if (result.a.Valid && result.b.Valid)
        {
            var points = new IntersectionPoints
            {
                result.a,
                result.b
            };
            return points;
        }

        if (result.a.Valid)
        {
            var points = new IntersectionPoints { result.a };
            return points;
        }

        if (result.b.Valid)
        {
            var points = new IntersectionPoints { result.b };
            return points;
        }

        return null;
    }

    /// <summary>
    /// Computes all intersection points between this segment and a triangle.
    /// </summary>
    /// <param name="t">The triangle to test against.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection containing intersection points, or null if there are no intersections.</returns>
    /// <remarks>
    /// May return up to two intersection points, as a segment can intersect a triangle at most twice.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Triangle t)
    {
        IntersectionPoints? points = null;
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

    /// <summary>
    /// Computes all intersection points between this segment and a quadrilateral.
    /// </summary>
    /// <param name="q">The quadrilateral to test against.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection containing intersection points, or null if there are no intersections.</returns>
    /// <remarks>
    /// May return up to two intersection points, as a segment can intersect a quad at most twice.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Quad q)
    {
        IntersectionPoints? points = null;
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

    /// <summary>
    /// Computes all intersection points between this segment and a rectangle.
    /// </summary>
    /// <param name="r">The rectangle to test against.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection containing intersection points, or null if there are no intersections.</returns>
    /// <remarks>
    /// May return up to two intersection points, as a segment can intersect a rectangle at most twice.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Rect r)
    {
        IntersectionPoints? points = null;
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

    /// <summary>
    /// Computes all intersection points between this segment and a polygon.
    /// </summary>
    /// <param name="p">The polygon to test against. Must have at least 3 vertices.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection containing intersection points, or null if there are no intersections.</returns>
    /// <remarks>
    /// Iterates over all polygon edges and collects intersection points with the segment.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Polygon p)
    {
        if (p.Count < 3) return null;
        IntersectionPoints? points = null;
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

    /// <summary>
    /// Computes all intersection points between this segment and a polyline.
    /// </summary>
    /// <param name="pl">The polyline to test against. Must have at least 2 vertices.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection containing intersection points, or null if there are no intersections.</returns>
    /// <remarks>
    /// Iterates over all polyline segments and collects intersection points with the segment.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Polyline pl)
    {
        if (pl.Count < 2) return null;
        IntersectionPoints? points = null;
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

    /// <summary>
    /// Computes all intersection points between this segment and a set of segments.
    /// </summary>
    /// <param name="shape">The set of segments to test against.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection containing intersection points, or null if there are no intersections.</returns>
    /// <remarks>
    /// Iterates over all segments in the set and collects intersection points with the segment.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Segments shape)
    {
        if (shape.Count <= 0) return null;
        IntersectionPoints? points = null;

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

    /// <summary>
    /// Computes all intersection points between this segment and a collider shape, storing results in a provided collection.
    /// </summary>
    /// <param name="collider">The collider whose shape will be tested for intersection. Must be enabled.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection to store intersection points.</param>
    /// <param name="returnAfterFirstValid">If true, the method returns after the first valid intersection is found.</param>
    /// <returns>The number of intersection points found.</returns>
    /// <remarks>
    /// The method dispatches to the appropriate shape-specific intersection method based on the collider's shape type.
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
    /// Computes all intersection points between this segment and a ray, storing results in a provided collection.
    /// </summary>
    /// <param name="r">The ray to test against.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection to store intersection points.</param>
    /// <returns>The number of intersection points found (0 or 1).</returns>
    /// <remarks>
    /// Returns 1 if the segment and ray intersect, otherwise 0.
    /// </remarks>
    public int IntersectShape(Ray r, ref IntersectionPoints points)
    {
        var cp = IntersectSegmentRay(Start, End, r.Point, r.Direction, r.Normal);
        if (cp.Valid)
        {
            points.Add(cp);
            return 1;
        }

        return 0;
    }

    /// <summary>
    /// Computes all intersection points between this segment and a line, storing results in a provided collection.
    /// </summary>
    /// <param name="l">The line to test against.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection to store intersection points.</param>
    /// <returns>The number of intersection points found (0 or 1).</returns>
    /// <remarks>
    /// Returns 1 if the segment and line intersect, otherwise 0.
    /// </remarks>
    public int IntersectShape(Line l, ref IntersectionPoints points)
    {
        var cp = IntersectSegmentLine(Start, End, l.Point, l.Direction, l.Normal);
        if (cp.Valid)
        {
            points.Add(cp);
            return 1;
        }

        return 0;
    }

    /// <summary>
    /// Computes all intersection points between this segment and another segment, storing results in a provided collection.
    /// </summary>
    /// <param name="s">The segment to test against.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection to store intersection points.</param>
    /// <returns>The number of intersection points found (0 or 1).</returns>
    /// <remarks>
    /// Returns 1 if the segments intersect, otherwise 0.
    /// </remarks>
    public int IntersectShape(Segment s, ref IntersectionPoints points)
    {
        var cp = IntersectSegmentSegment(Start, End, s.Start, s.End);
        if (cp.Valid)
        {
            points.Add(cp);
            return 1;
        }

        return 0;
    }

    /// <summary>
    /// Computes all intersection points between this segment and a circle, storing results in a provided collection.
    /// </summary>
    /// <param name="c">The circle to test against.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection to store intersection points.</param>
    /// <param name="returnAfterFirstValid">If true, the method returns after the first valid intersection is found.</param>
    /// <returns>The number of intersection points found (0, 1, or 2).</returns>
    /// <remarks>
    /// May return up to two intersection points depending on the segment's position relative to the circle.
    /// </remarks>
    public int IntersectShape(Circle c, ref IntersectionPoints points, bool returnAfterFirstValid = false)
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

    /// <summary>
    /// Computes all intersection points between this segment and a triangle, storing results in a provided collection.
    /// </summary>
    /// <param name="t">The triangle to test against.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection to store intersection points.</param>
    /// <param name="returnAfterFirstValid">If true, the method returns after the first valid intersection is found.</param>
    /// <returns>The number of intersection points found (0, 1, or 2).</returns>
    /// <remarks>
    /// May return up to two intersection points, as a segment can intersect a triangle at most twice.
    /// </remarks>
    public int IntersectShape(Triangle t, ref IntersectionPoints points, bool returnAfterFirstValid = false)
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

    /// <summary>
    /// Computes all intersection points between this segment and a quadrilateral, storing results in a provided collection.
    /// </summary>
    /// <param name="q">The quadrilateral to test against.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection to store intersection points.</param>
    /// <param name="returnAfterFirstValid">If true, the method returns after the first valid intersection is found.</param>
    /// <returns>The number of intersection points found (0, 1, or 2).</returns>
    /// <remarks>
    /// May return up to two intersection points, as a segment can intersect a quad at most twice.
    /// </remarks>
    public int IntersectShape(Quad q, ref IntersectionPoints points, bool returnAfterFirstValid = false)
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

    /// <summary>
    /// Computes all intersection points between this segment and a rectangle, storing results in a provided collection.
    /// </summary>
    /// <param name="r">The rectangle to test against.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection to store intersection points.</param>
    /// <param name="returnAfterFirstValid">If true, the method returns after the first valid intersection is found.</param>
    /// <returns>The number of intersection points found (0, 1, or 2).</returns>
    /// <remarks>
    /// May return up to two intersection points, as a segment can intersect a rectangle at most twice.
    /// </remarks>
    public int IntersectShape(Rect r, ref IntersectionPoints points, bool returnAfterFirstValid = false)
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

    /// <summary>
    /// Computes all intersection points between this segment and a polygon, storing results in a provided collection.
    /// </summary>
    /// <param name="p">The polygon to test against. Must have at least 3 vertices.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection to store intersection points.</param>
    /// <param name="returnAfterFirstValid">If true, the method returns after the first valid intersection is found.</param>
    /// <returns>The number of intersection points found.</returns>
    /// <remarks>
    /// Iterates over all polygon edges and collects intersection points with the segment.
    /// </remarks>
    public int IntersectShape(Polygon p, ref IntersectionPoints points, bool returnAfterFirstValid = false)
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

    /// <summary>
    /// Computes all intersection points between this segment and a polyline, storing results in a provided collection.
    /// </summary>
    /// <param name="pl">The polyline to test against. Must have at least 2 vertices.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection to store intersection points.</param>
    /// <param name="returnAfterFirstValid">If true, the method returns after the first valid intersection is found.</param>
    /// <returns>The number of intersection points found.</returns>
    /// <remarks>
    /// Iterates over all polyline segments and collects intersection points with the segment.
    /// </remarks>
    public int IntersectShape(Polyline pl, ref IntersectionPoints points, bool returnAfterFirstValid = false)
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

    /// <summary>
    /// Computes all intersection points between this segment and a set of segments, storing results in a provided collection.
    /// </summary>
    /// <param name="shape">The set of segments to test against.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection to store intersection points.</param>
    /// <param name="returnAfterFirstValid">If true, the method returns after the first valid intersection is found.</param>
    /// <returns>The number of intersection points found.</returns>
    /// <remarks>
    /// Iterates over all segments in the set and collects intersection points with the segment.
    /// </remarks>
    public int IntersectShape(Segments shape, ref IntersectionPoints points, bool returnAfterFirstValid = false)
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
    
        /// <summary>
    /// Computes intersection points between this shape and a shape implementing <see cref="IShape"/>.
    /// </summary>
    /// <param name="shape">The shape to test against.</param>
    /// <returns>
    /// A <see cref="IntersectionPoints"/> collection of intersection points, or null if none.
    /// </returns>
    public IntersectionPoints? IntersectShape(IShape shape)
    {
        return shape.GetShapeType() switch
        {
            ShapeType.Circle => IntersectShape(shape.GetCircleShape()),
            ShapeType.Segment => IntersectShape(shape.GetSegmentShape()),
            ShapeType.Ray => IntersectShape(shape.GetRayShape()),
            ShapeType.Line => IntersectShape(shape.GetLineShape()),
            ShapeType.Triangle => IntersectShape(shape.GetTriangleShape()),
            ShapeType.Rect => IntersectShape(shape.GetRectShape()),
            ShapeType.Quad => IntersectShape(shape.GetQuadShape()),
            ShapeType.Poly => IntersectShape(shape.GetPolygonShape()),
            ShapeType.PolyLine => IntersectShape(shape.GetPolylineShape()),
            _ => null
        };
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
            ShapeType.Segment => IntersectShape(shape.GetSegmentShape(), ref points),
            ShapeType.Ray => IntersectShape(shape.GetRayShape(), ref points),
            ShapeType.Line => IntersectShape(shape.GetLineShape(), ref points),
            ShapeType.Triangle => IntersectShape(shape.GetTriangleShape(), ref points, returnAfterFirstValid),
            ShapeType.Rect => IntersectShape(shape.GetRectShape(), ref points, returnAfterFirstValid),
            ShapeType.Quad => IntersectShape(shape.GetQuadShape(), ref points, returnAfterFirstValid),
            ShapeType.Poly => IntersectShape(shape.GetPolygonShape(), ref points, returnAfterFirstValid),
            ShapeType.PolyLine => IntersectShape(shape.GetPolylineShape(), ref points, returnAfterFirstValid),
            _ => 0
        };
    }

}