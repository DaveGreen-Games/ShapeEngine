using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.QuadDef;

public readonly partial struct Quad
{
    /// <summary>
    /// Computes intersection points between this quad and all colliders in the specified <see cref="CollisionObject"/>.
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
    /// Computes intersection points between this quad and the specified collider.
    /// </summary>
    /// <param name="collider">The collider to test for intersection.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection of intersection points, or null if none.</returns>
    /// <remarks>Returns null if the collider is disabled or no intersection occurs.</remarks>
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
    /// Computes intersection points between this quad and a set of segments.
    /// </summary>
    /// <param name="segments">The segments to test for intersection.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection of intersection points, or null if none.</returns>
    public IntersectionPoints? IntersectShape(Segments segments)
    {
        if (segments.Count <= 0) return null;

        IntersectionPoints? points = null;

        foreach (var seg in segments)
        {
            var result = Segment.IntersectSegmentSegment(A, B, seg.Start, seg.End);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(B, C, seg.Start, seg.End);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(C, D, seg.Start, seg.End);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(D, A, seg.Start, seg.End);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
        }

        return points;
    }
    /// <summary>
    /// Computes intersection points between this quad and a ray.
    /// </summary>
    /// <param name="r">The ray to test for intersection.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection of intersection points, or null if none.</returns>
    public IntersectionPoints? IntersectShape(Ray r)
    {
        IntersectionPoints? points = null;
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

        result = Segment.IntersectSegmentRay(C, D, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentRay(D, A, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        return points;
    }
    /// <summary>
    /// Computes intersection points between this quad and a line.
    /// </summary>
    /// <param name="l">The line to test for intersection.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection of intersection points, or null if none.</returns>
    public IntersectionPoints? IntersectShape(Line l)
    {
        IntersectionPoints? points = null;
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

        result = Segment.IntersectSegmentLine(C, D, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentLine(D, A, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        return points;
    }
    /// <summary>
    /// Computes intersection points between this quad and a segment.
    /// </summary>
    /// <param name="s">The segment to test for intersection.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection of intersection points, or null if none.</returns>
    public IntersectionPoints? IntersectShape(Segment s)
    {
        IntersectionPoints? points = null;
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

        result = Segment.IntersectSegmentSegment(C, D, s.Start, s.End);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(D, A, s.Start, s.End);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        return points;
    }
    /// <summary>
    /// Computes intersection points between this quad and a circle.
    /// </summary>
    /// <param name="c">The circle to test for intersection.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection of intersection points, or null if none.</returns>
    public IntersectionPoints? IntersectShape(Circle c)
    {
        IntersectionPoints? points = null;
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

        result = Segment.IntersectSegmentCircle(C, D, c.Center, c.Radius);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if (result.a.Valid) points.Add(result.a);
            if (result.b.Valid) points.Add(result.b);
        }

        result = Segment.IntersectSegmentCircle(D, A, c.Center, c.Radius);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if (result.a.Valid) points.Add(result.a);
            if (result.b.Valid) points.Add(result.b);
        }

        return points;
    }
    /// <summary>
    /// Computes intersection points between this quad and a triangle.
    /// </summary>
    /// <param name="t">The triangle to test for intersection.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection of intersection points, or null if none.</returns>
    public IntersectionPoints? IntersectShape(Triangle t)
    {
        IntersectionPoints? points = null;
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

        result = Segment.IntersectSegmentSegment(C, D, t.A, t.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(C, D, t.B, t.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(C, D, t.C, t.A);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(D, A, t.A, t.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(D, A, t.B, t.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(D, A, t.C, t.A);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        return points;
    }
    /// <summary>
    /// Computes intersection points between this quad and a rectangle.
    /// </summary>
    /// <param name="r">The rectangle to test for intersection.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection of intersection points, or null if none.</returns>
    public IntersectionPoints? IntersectShape(Rect r)
    {
        IntersectionPoints? points = null;
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


        result = Segment.IntersectSegmentSegment(C, D, a, b);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(C, D, b, c);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(C, D, c, d);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(C, D, d, a);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }


        result = Segment.IntersectSegmentSegment(D, A, a, b);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(D, A, b, c);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(D, A, c, d);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(D, A, d, a);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        return points;
    }
    /// <summary>
    /// Computes intersection points between this quad and another quad.
    /// </summary>
    /// <param name="q">The quad to test for intersection.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection of intersection points, or null if none.</returns>
    public IntersectionPoints? IntersectShape(Quad q)
    {
        IntersectionPoints? points = null;
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


        result = Segment.IntersectSegmentSegment(C, D, q.A, q.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(C, D, q.B, q.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(C, D, q.C, q.D);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(C, D, q.D, q.A);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(D, A, q.A, q.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(D, A, q.B, q.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(D, A, q.C, q.D);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        result = Segment.IntersectSegmentSegment(D, A, q.D, q.A);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        return points;
    }
    /// <summary>
    /// Computes intersection points between this quad and a polygon.
    /// </summary>
    /// <param name="p">The polygon to test for intersection.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection of intersection points, or null if none.</returns>
    public IntersectionPoints? IntersectShape(Polygon p)
    {
        if (p.Count < 3) return null;

        IntersectionPoints? points = null;
        for (var i = 0; i < p.Count; i++)
        {
            var result = Segment.IntersectSegmentSegment(A, B, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(B, C, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(C, D, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(D, A, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
        }

        return points;
    }
    /// <summary>
    /// Computes intersection points between this quad and a polyline.
    /// </summary>
    /// <param name="pl">The polyline to test for intersection.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection of intersection points, or null if none.</returns>
    public IntersectionPoints? IntersectShape(Polyline pl)
    {
        if (pl.Count < 2) return null;

        IntersectionPoints? points = null;
        for (var i = 0; i < pl.Count - 1; i++)
        {
            var result = Segment.IntersectSegmentSegment(A, B, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(B, C, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(C, D, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(D, A, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
        }

        return points;
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
    /// Computes intersection points between this quad and a collider, adding results to an existing <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="collider">The collider to test for intersection. Has to be enabled.</param>
    /// <param name="points">The collection to add intersection points to.</param>
    /// <param name="returnAfterFirstValid">If true, returns after the first intersection is found.</param>
    /// <returns>The number of intersection points found.</returns>
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
    /// Computes intersection points between this quad and a ray,
    /// adding results to an existing <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="r">The ray to test for intersection.</param>
    /// <param name="points">The collection to add intersection points to.</param>
    /// <param name="returnAfterFirstValid">If true, returns after the first intersection is found.</param>
    /// <returns>The number of intersection points found.</returns>
    public int IntersectShape(Ray r, ref IntersectionPoints points, bool returnAfterFirstValid = false)
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

        result = Segment.IntersectSegmentRay(C, D, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (count >= 2) return count;

        result = Segment.IntersectSegmentRay(D, A, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }

        return count;
    }
    /// <summary>
    /// Computes intersection points between this quad and a line, adding results to an existing <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="l">The line to test for intersection.</param>
    /// <param name="points">The collection to add intersection points to.</param>
    /// <param name="returnAfterFirstValid">If true, returns after the first intersection is found.</param>
    /// <returns>The number of intersection points found.</returns>
    public int IntersectShape(Line l, ref IntersectionPoints points, bool returnAfterFirstValid = false)
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

        result = Segment.IntersectSegmentLine(C, D, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (count >= 2) return count;

        result = Segment.IntersectSegmentLine(D, A, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }

        return count;
    }
    /// <summary>
    /// Computes intersection points between this quad and a segment, adding results to an existing <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="s">The segment to test for intersection.</param>
    /// <param name="points">The collection to add intersection points to.</param>
    /// <param name="returnAfterFirstValid">If true, returns after the first intersection is found.</param>
    /// <returns>The number of intersection points found.</returns>
    public int IntersectShape(Segment s, ref IntersectionPoints points, bool returnAfterFirstValid = false)
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

        result = Segment.IntersectSegmentSegment(C, D, s.Start, s.End);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (count >= 2) return count;

        result = Segment.IntersectSegmentSegment(D, A, s.Start, s.End);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }

        return count;
    }
    /// <summary>
    /// Computes intersection points between this quad and a circle, adding results to an existing <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="c">The circle to test for intersection.</param>
    /// <param name="points">The collection to add intersection points to.</param>
    /// <param name="returnAfterFirstValid">If true, returns after the first intersection is found.</param>
    /// <returns>The number of intersection points found.</returns>
    public int IntersectShape(Circle c, ref IntersectionPoints points, bool returnAfterFirstValid = false)
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

        result = Segment.IntersectSegmentCircle(C, D, c.Center, c.Radius);
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

        result = Segment.IntersectSegmentCircle(D, A, c.Center, c.Radius);
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
    /// Computes intersection points between this quad and a triangle, adding results to an existing <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="t">The triangle to test for intersection.</param>
    /// <param name="points">The collection to add intersection points to.</param>
    /// <param name="returnAfterFirstValid">If true, returns after the first intersection is found.</param>
    /// <returns>The number of intersection points found.</returns>
    public int IntersectShape(Triangle t, ref IntersectionPoints points, bool returnAfterFirstValid = false)
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

        result = Segment.IntersectSegmentSegment(C, D, t.A, t.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(C, D, t.B, t.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(C, D, t.C, t.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(D, A, t.A, t.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(D, A, t.B, t.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(D, A, t.C, t.A);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }

        return count;
    }
    /// <summary>
    /// Computes intersection points between this quad and another quad, adding results to an existing <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="q">The quad to test for intersection.</param>
    /// <param name="points">The collection to add intersection points to.</param>
    /// <param name="returnAfterFirstValid">If true, returns after the first intersection is found.</param>
    /// <returns>The number of intersection points found.</returns>
    public int IntersectShape(Quad q, ref IntersectionPoints points, bool returnAfterFirstValid = false)
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


        result = Segment.IntersectSegmentSegment(C, D, q.A, q.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(C, D, q.B, q.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(C, D, q.C, q.D);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(C, D, q.D, q.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(D, A, q.A, q.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(D, A, q.B, q.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(D, A, q.C, q.D);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(D, A, q.D, q.A);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }

        return count;
    }
    /// <summary>
    /// Computes intersection points between this quad and a rectangle,
    /// adding results to an existing <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="r">The rectangle to test for intersection.</param>
    /// <param name="points">The collection to add intersection points to.</param>
    /// <param name="returnAfterFirstValid">If true, returns after the first intersection is found.</param>
    /// <returns>The number of intersection points found.</returns>
    public int IntersectShape(Rect r, ref IntersectionPoints points, bool returnAfterFirstValid = false)
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


        result = Segment.IntersectSegmentSegment(C, D, a, b);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(C, D, b, c);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(C, D, c, d);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(C, D, d, a);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }


        result = Segment.IntersectSegmentSegment(D, A, a, b);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(D, A, b, c);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(D, A, c, d);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        result = Segment.IntersectSegmentSegment(D, A, d, a);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }

        return count;
    }
    /// <summary>
    /// Computes intersection points between this quad and a polygon, adding results to an existing <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="p">The polygon to test for intersection.</param>
    /// <param name="points">The collection to add intersection points to.</param>
    /// <param name="returnAfterFirstValid">If true, returns after the first intersection is found.</param>
    /// <returns>The number of intersection points found.</returns>
    public int IntersectShape(Polygon p, ref IntersectionPoints points, bool returnAfterFirstValid = false)
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

            result = Segment.IntersectSegmentSegment(C, D, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.IntersectSegmentSegment(D, A, p[i], p[(i + 1) % p.Count]);
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
    /// Computes intersection points between this quad and a polyline, adding results to an existing <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="pl">The polyline to test for intersection.</param>
    /// <param name="points">The collection to add intersection points to.</param>
    /// <param name="returnAfterFirstValid">If true, returns after the first intersection is found.</param>
    /// <returns>The number of intersection points found.</returns>
    public int IntersectShape(Polyline pl, ref IntersectionPoints points, bool returnAfterFirstValid = false)
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

            result = Segment.IntersectSegmentSegment(C, D, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.IntersectSegmentSegment(D, A, pl[i], pl[(i + 1) % pl.Count]);
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
    /// Computes intersection points between this quad and a set of segments, adding results to an existing <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="shape">The segments to test for intersection.</param>
    /// <param name="points">The collection to add intersection points to.</param>
    /// <param name="returnAfterFirstValid">If true, returns after the first intersection is found.</param>
    /// <returns>The number of intersection points found.</returns>
    public int IntersectShape(Segments shape, ref IntersectionPoints points, bool returnAfterFirstValid = false)
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

            result = Segment.IntersectSegmentSegment(C, D, seg.Start, seg.End);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.IntersectSegmentSegment(D, A, seg.Start, seg.End);
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