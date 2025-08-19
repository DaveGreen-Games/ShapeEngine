using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.CircleDef;

public readonly partial struct Circle
{
    /// <summary>
    /// Computes intersection points between this circle and all colliders in the specified <see cref="CollisionObject"/>.
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
    /// Calculates the intersection points between this circle and a collider.
    /// </summary>
    /// <param name="collider">The collider to test for intersection. Can represent various shapes.</param>
    /// <returns>
    /// A <see cref="IntersectionPoints"/> object containing the intersection points, or <c>null</c> if there is no intersection.
    /// </returns>
    /// <remarks>
    /// The method dispatches to the appropriate shape-specific intersection logic based on the collider's type.
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
    /// Calculates the intersection points between this circle and another circle.
    /// </summary>
    /// <param name="c">The other circle to test for intersection.</param>
    /// <returns>
    /// A <see cref="IntersectionPoints"/> object containing the intersection points, or <c>null</c> if there is no intersection.
    /// </returns>
    public IntersectionPoints? IntersectShape(Circle c)
    {
        var result = IntersectCircleCircle(Center, Radius, c.Center, c.Radius);
        if (result.a.Valid || result.b.Valid)
        {
            var points = new IntersectionPoints();
            if (result.a.Valid) points.Add(result.a);
            if (result.b.Valid) points.Add(result.b);
            return points;
        }

        return null;
    }

    /// <summary>
    /// Calculates the intersection points between this circle and a ray.
    /// </summary>
    /// <param name="r">The ray to test for intersection.</param>
    /// <returns>
    /// A <see cref="IntersectionPoints"/> object containing the intersection points, or <c>null</c> if there is no intersection.
    /// </returns>
    public IntersectionPoints? IntersectShape(Ray r)
    {
        var result = IntersectCircleRay(Center, Radius, r.Point, r.Direction, r.Normal);
        if (result.a.Valid || result.b.Valid)
        {
            var points = new IntersectionPoints();
            if (result.a.Valid) points.Add(result.a);
            if (result.b.Valid) points.Add(result.b);
            return points;
        }

        return null;
    }

    /// <summary>
    /// Calculates the intersection points between this circle and a line.
    /// </summary>
    /// <param name="l">The line to test for intersection.</param>
    /// <returns>
    /// A <see cref="IntersectionPoints"/> object containing the intersection points, or <c>null</c> if there is no intersection.
    /// </returns>
    public IntersectionPoints? IntersectShape(Line l)
    {
        var result = IntersectCircleLine(Center, Radius, l.Point, l.Direction, l.Normal);

        if (result.a.Valid || result.b.Valid)
        {
            var points = new IntersectionPoints();
            if (result.a.Valid) points.Add(result.a);
            if (result.b.Valid) points.Add(result.b);
            return points;
        }

        return null;
    }

    /// <summary>
    /// Calculates the intersection points between this circle and a segment.
    /// </summary>
    /// <param name="s">The segment to test for intersection.</param>
    /// <returns>
    /// A <see cref="IntersectionPoints"/> object containing the intersection points, or <c>null</c> if there is no intersection.
    /// </returns>
    public IntersectionPoints? IntersectShape(Segment s)
    {
        var result = IntersectCircleSegment(Center, Radius, s.Start, s.End);
        if (result.a.Valid || result.b.Valid)
        {
            var points = new IntersectionPoints();
            if (result.a.Valid) points.Add(result.a);
            if (result.b.Valid) points.Add(result.b);
            return points;
        }

        return null;
    }

    /// <summary>
    /// Calculates the intersection points between this circle and a triangle.
    /// </summary>
    /// <param name="t">The triangle to test for intersection.</param>
    /// <returns>
    /// A <see cref="IntersectionPoints"/> object containing the intersection points, or <c>null</c> if there is no intersection.
    /// </returns>
    public IntersectionPoints? IntersectShape(Triangle t)
    {
        IntersectionPoints? points = null;
        var result = IntersectCircleSegment(Center, Radius, t.A, t.B);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if (result.a.Valid) points.Add(result.a);
            if (result.b.Valid) points.Add(result.b);
        }

        result = IntersectCircleSegment(Center, Radius, t.B, t.C);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if (result.a.Valid) points.Add(result.a);
            if (result.b.Valid) points.Add(result.b);
        }

        result = IntersectCircleSegment(Center, Radius, t.C, t.A);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if (result.a.Valid) points.Add(result.a);
            if (result.b.Valid) points.Add(result.b);
        }

        return points;
    }

    /// <summary>
    /// Calculates the intersection points between this circle and a rectangle.
    /// </summary>
    /// <param name="r">The rectangle to test for intersection.</param>
    /// <returns>
    /// A <see cref="IntersectionPoints"/> object containing the intersection points, or <c>null</c> if there is no intersection.
    /// </returns>
    public IntersectionPoints? IntersectShape(Rect r)
    {
        IntersectionPoints? points = null;
        var a = r.TopLeft;
        var b = r.BottomLeft;

        var result = IntersectCircleSegment(Center, Radius, a, b);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if (result.a.Valid) points.Add(result.a);
            if (result.b.Valid) points.Add(result.b);
        }

        var c = r.BottomRight;
        result = IntersectCircleSegment(Center, Radius, b, c);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if (result.a.Valid) points.Add(result.a);
            if (result.b.Valid) points.Add(result.b);
        }

        var d = r.TopRight;
        result = IntersectCircleSegment(Center, Radius, c, d);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if (result.a.Valid) points.Add(result.a);
            if (result.b.Valid) points.Add(result.b);
        }

        result = IntersectCircleSegment(Center, Radius, d, a);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if (result.a.Valid) points.Add(result.a);
            if (result.b.Valid) points.Add(result.b);
        }

        return points;
    }

    /// <summary>
    /// Calculates the intersection points between this circle and a quadrilateral.
    /// </summary>
    /// <param name="q">The quad to test for intersection.</param>
    /// <returns>
    /// A <see cref="IntersectionPoints"/> object containing the intersection points, or <c>null</c> if there is no intersection.
    /// </returns>
    public IntersectionPoints? IntersectShape(Quad q)
    {
        IntersectionPoints? points = null;

        var result = IntersectCircleSegment(Center, Radius, q.A, q.B);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if (result.a.Valid) points.Add(result.a);
            if (result.b.Valid) points.Add(result.b);
        }

        result = IntersectCircleSegment(Center, Radius, q.B, q.C);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if (result.a.Valid) points.Add(result.a);
            if (result.b.Valid) points.Add(result.b);
        }

        result = IntersectCircleSegment(Center, Radius, q.C, q.D);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if (result.a.Valid) points.Add(result.a);
            if (result.b.Valid) points.Add(result.b);
        }

        result = IntersectCircleSegment(Center, Radius, q.D, q.A);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if (result.a.Valid) points.Add(result.a);
            if (result.b.Valid) points.Add(result.b);
        }

        return points;
    }

    /// <summary>
    /// Calculates the intersection points between this circle and a polygon.
    /// </summary>
    /// <param name="p">The polygon to test for intersection.</param>
    /// <returns>
    /// A <see cref="IntersectionPoints"/> object containing the intersection points, or <c>null</c> if there is no intersection.
    /// </returns>
    public IntersectionPoints? IntersectShape(Polygon p)
    {
        if (p.Count < 3) return null;

        IntersectionPoints? points = null;

        for (var i = 0; i < p.Count; i++)
        {
            var result = IntersectCircleSegment(Center, Radius, p[i], p[(i + 1) % p.Count]);
            if (result.a.Valid || result.b.Valid)
            {
                points ??= new();
                if (result.a.Valid) points.Add(result.a);
                if (result.b.Valid) points.Add(result.b);
            }
        }

        return points;
    }

    /// <summary>
    /// Calculates the intersection points between this circle and a polyline.
    /// </summary>
    /// <param name="pl">The polyline to test for intersection.</param>
    /// <returns>
    /// A <see cref="IntersectionPoints"/> object containing the intersection points, or <c>null</c> if there is no intersection.
    /// </returns>
    public IntersectionPoints? IntersectShape(Polyline pl)
    {
        if (pl.Count < 2) return null;

        IntersectionPoints? points = null;

        for (var i = 0; i < pl.Count - 1; i++)
        {
            var result = IntersectCircleSegment(Center, Radius, pl[i], pl[(i + 1) % pl.Count]);
            if (result.a.Valid || result.b.Valid)
            {
                points ??= new();
                if (result.a.Valid) points.Add(result.a);
                if (result.b.Valid) points.Add(result.b);
            }
        }

        return points;
    }

    /// <summary>
    /// Calculates the intersection points between this circle and a collection of segments.
    /// </summary>
    /// <param name="shape">The collection of segments to test for intersection.</param>
    /// <returns>
    /// A <see cref="IntersectionPoints"/> object containing the intersection points, or <c>null</c> if there is no intersection.
    /// </returns>
    public IntersectionPoints? IntersectShape(Segments shape)
    {
        IntersectionPoints? points = null;
        foreach (var seg in shape)
        {
            var result = IntersectCircleSegment(Center, Radius, seg.Start, seg.End);
            if (result.a.Valid || result.b.Valid)
            {
                points ??= new();
                if (result.a.Valid) points.Add(result.a);
                if (result.b.Valid) points.Add(result.b);
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
    /// Calculates and adds intersection points between this circle and a collider to the provided <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="collider">The collider to test for intersection.</param>
    /// <param name="points">The collection to which valid intersection points will be added.</param>
    /// <param name="returnAfterFirstValid">
    /// If <c>true</c>, the method returns after the first valid intersection is found.
    /// </param>
    /// <returns>The number of intersection points found and added.</returns> 
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
    /// Calculates and adds intersection points between this circle and a ray to the provided <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="r">The ray to test for intersection.</param>
    /// <param name="points">The collection to which valid intersection points will be added.</param>
    /// <param name="returnAfterFirstValid">
    /// If <c>true</c>, the method returns after the first valid intersection is found.
    /// </param>
    /// <returns>The number of intersection points found and added.</returns>
    public int IntersectShape(Ray r, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        var result = IntersectCircleRay(Center, Radius, r.Point, r.Direction, r.Normal);
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
    /// Calculates and adds intersection points between this circle and a line to the provided <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="l">The line to test for intersection.</param>
    /// <param name="points">The collection to which valid intersection points will be added.</param>
    /// <param name="returnAfterFirstValid">
    /// If <c>true</c>, the method returns after the first valid intersection is found.
    /// </param>
    /// <returns>The number of intersection points found and added.</returns>
    public int IntersectShape(Line l, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        var result = IntersectCircleLine(Center, Radius, l.Point, l.Direction, l.Normal);
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
    /// Calculates and adds intersection points between this circle and a segment to the provided <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="s">The segment to test for intersection.</param>
    /// <param name="points">The collection to which valid intersection points will be added.</param>
    /// <param name="returnAfterFirstValid">
    /// If <c>true</c>, the method returns after the first valid intersection is found.
    /// </param>
    /// <returns>The number of intersection points found and added.</returns>
    public int IntersectShape(Segment s, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        var result = IntersectCircleSegment(Center, Radius, s.Start, s.End);
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
    /// Calculates and adds intersection points between this circle and another circle to the provided <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="c">The other circle to test for intersection.</param>
    /// <param name="points">The collection to which valid intersection points will be added.</param>
    /// <param name="returnAfterFirstValid">
    /// If <c>true</c>, the method returns after the first valid intersection is found.
    /// </param>
    /// <returns>The number of intersection points found and added.</returns>
    public int IntersectShape(Circle c, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        var result = IntersectCircleCircle(Center, Radius, c.Center, c.Radius);
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
    /// Calculates and adds intersection points between this circle and a triangle to the provided <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="t">The triangle to test for intersection.</param>
    /// <param name="points">The collection to which valid intersection points will be added.</param>
    /// <param name="returnAfterFirstValid">
    /// If <c>true</c>, the method returns after the first valid intersection is found.
    /// </param>
    /// <returns>The number of intersection points found and added.</returns>
    public int IntersectShape(Triangle t, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        var result = IntersectCircleSegment(Center, Radius, t.A, t.B);
        var count = 0;
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

        result = IntersectCircleSegment(Center, Radius, t.B, t.C);
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

        result = IntersectCircleSegment(Center, Radius, t.C, t.A);
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
    /// Calculates and adds intersection points between this circle and a quadrilateral to the provided <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="q">The quad to test for intersection.</param>
    /// <param name="points">The collection to which valid intersection points will be added.</param>
    /// <param name="returnAfterFirstValid">
    /// If <c>true</c>, the method returns after the first valid intersection is found.
    /// </param>
    /// <returns>The number of intersection points found and added.</returns>
    public int IntersectShape(Quad q, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;

        var result = IntersectCircleSegment(Center, Radius, q.A, q.B);
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

        result = IntersectCircleSegment(Center, Radius, q.B, q.C);
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

        result = IntersectCircleSegment(Center, Radius, q.C, q.D);
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

        result = IntersectCircleSegment(Center, Radius, q.D, q.A);
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
    /// Calculates and adds intersection points between this circle and a rectangle to the provided <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="r">The rectangle to test for intersection.</param>
    /// <param name="points">The collection to which valid intersection points will be added.</param>
    /// <param name="returnAfterFirstValid">
    /// If <c>true</c>, the method returns after the first valid intersection is found.
    /// </param>
    /// <returns>The number of intersection points found and added.</returns>
    public int IntersectShape(Rect r, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        var a = r.TopLeft;
        var b = r.BottomLeft;

        var result = IntersectCircleSegment(Center, Radius, a, b);
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

        var c = r.BottomRight;
        result = IntersectCircleSegment(Center, Radius, b, c);
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

        var d = r.TopRight;
        result = IntersectCircleSegment(Center, Radius, c, d);
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

        result = IntersectCircleSegment(Center, Radius, d, a);
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
    /// Calculates and adds intersection points between this circle and a polygon to the provided <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="p">The polygon to test for intersection.</param>
    /// <param name="points">The collection to which valid intersection points will be added.</param>
    /// <param name="returnAfterFirstValid">
    /// If <c>true</c>, the method returns after the first valid intersection is found.
    /// </param>
    /// <returns>The number of intersection points found and added.</returns>
    public int IntersectShape(Polygon p, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (p.Count < 3) return 0;

        var count = 0;

        for (var i = 0; i < p.Count; i++)
        {
            var result = IntersectCircleSegment(Center, Radius, p[i], p[(i + 1) % p.Count]);
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

    /// <summary>
    /// Calculates and adds intersection points between this circle and a polyline to the provided <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="pl">The polyline to test for intersection.</param>
    /// <param name="points">The collection to which valid intersection points will be added.</param>
    /// <param name="returnAfterFirstValid">
    /// If <c>true</c>, the method returns after the first valid intersection is found.
    /// </param>
    /// <returns>The number of intersection points found and added.</returns>
    public int IntersectShape(Polyline pl, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (pl.Count < 2) return 0;

        var count = 0;

        for (var i = 0; i < pl.Count - 1; i++)
        {
            var result = IntersectCircleSegment(Center, Radius, pl[i], pl[(i + 1) % pl.Count]);
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

    /// <summary>
    /// Calculates and adds intersection points between this circle and a collection of segments to the provided <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="shape">The collection of segments to test for intersection.</param>
    /// <param name="points">The collection to which valid intersection points will be added.</param>
    /// <param name="returnAfterFirstValid">
    /// If <c>true</c>, the method returns after the first valid intersection is found.
    /// </param>
    /// <returns>The number of intersection points found and added.</returns>
    public int IntersectShape(Segments shape, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        foreach (var seg in shape)
        {
            var result = IntersectCircleSegment(Center, Radius, seg.Start, seg.End);
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