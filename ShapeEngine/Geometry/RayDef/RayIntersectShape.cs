using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.RayDef;

public readonly partial struct Ray
{
    /// <summary>
    /// Computes intersection points between this ray and all colliders in the specified <see cref="CollisionObject"/>.
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
    /// Computes all intersection points between this ray and a generic collider shape.
    /// </summary>
    /// <param name="collider">The collider to test for intersection. Must be enabled.</param>
    /// <returns>A <see cref="IntersectionPoints"/> object containing all intersection points, or null if there are none or the collider is not enabled.</returns>
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
    /// Computes all intersection points between this ray and a segment.
    /// </summary>
    /// <param name="segment">The segment to test for intersection.</param>
    /// <returns>A <see cref="IntersectionPoints"/> object containing the intersection point, or null if there is no intersection.</returns>
    /// <remarks>
    /// Only a single intersection point is possible for a ray and a segment.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Segment segment)
    {
        var result = IntersectRaySegment(Point, Direction, segment.Start, segment.End, segment.Normal);
        Console.WriteLine($"Point of intersection: {result.Point} - Normal: {result.Normal} - Valid: {result.Valid}");
        if (result.Valid)
        {
            var colPoints = new IntersectionPoints();
            colPoints.Add(result);
            return colPoints;
        }

        return null;
    }
    /// <summary>
    /// Computes all intersection points between this ray and a line.
    /// </summary>
    /// <param name="line">The line to test for intersection.</param>
    /// <returns>A <see cref="IntersectionPoints"/> object containing the intersection point, or null if there is no intersection.</returns>
    /// <remarks>
    /// Only a single intersection point is possible for a ray and a line.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Line line)
    {
        var result = IntersectRayLine(Point, Direction, line.Point, line.Direction, line.Normal);
        if (result.Valid)
        {
            var colPoints = new IntersectionPoints();
            colPoints.Add(result);
            return colPoints;
        }

        return null;
    }
    /// <summary>
    /// Computes all intersection points between this ray and another ray.
    /// </summary>
    /// <param name="ray">The other ray to test for intersection.</param>
    /// <returns>A <see cref="IntersectionPoints"/> object containing the intersection point, or null if there is no intersection.</returns>
    /// <remarks>
    /// Only a single intersection point is possible for two rays.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Ray ray)
    {
        var result = IntersectRayRay(Point, Direction, ray.Point, ray.Direction, ray.Normal);
        if (result.Valid)
        {
            var colPoints = new IntersectionPoints();
            colPoints.Add(result);
            return colPoints;
        }

        return null;
    }
    /// <summary>
    /// Computes all intersection points between this ray and a circle.
    /// </summary>
    /// <param name="circle">The circle to test for intersection.</param>
    /// <returns>A <see cref="IntersectionPoints"/> object containing all intersection points, or null if there are none.</returns>
    /// <remarks>
    /// There can be zero, one, or two intersection points for a ray and a circle.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Circle circle)
    {
        var result = IntersectCircle(circle);
        if (result.a.Valid || result.b.Valid)
        {
            var colPoints = new IntersectionPoints();
            if (result.a.Valid)
            {
                colPoints.Add(result.a);
            }

            if (result.b.Valid)
            {
                colPoints.Add(result.b);
            }

            return colPoints;
        }

        return null;
    }
    /// <summary>
    /// Computes all intersection points between this ray and a triangle.
    /// </summary>
    /// <param name="t">The triangle to test for intersection.</param>
    /// <returns>A <see cref="IntersectionPoints"/> object containing all intersection points, or null if there are none.</returns>
    /// <remarks>
    /// There can be zero, one, or two intersection points for a ray and a triangle.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Triangle t)
    {
        var result = IntersectRayTriangle(Point, Direction, t.A, t.B, t.C);
        if (result.a.Valid || result.b.Valid)
        {
            var colPoints = new IntersectionPoints();
            if (result.a.Valid)
            {
                colPoints.Add(result.a);
            }

            if (result.b.Valid)
            {
                colPoints.Add(result.b);
            }

            return colPoints;
        }

        return null;
    }
    /// <summary>
    /// Computes all intersection points between this ray and a quad.
    /// </summary>
    /// <param name="q">The quad to test for intersection.</param>
    /// <returns>A <see cref="IntersectionPoints"/> object containing all intersection points, or null if there are none.</returns>
    /// <remarks>
    /// There can be zero, one, or two intersection points for a ray and a quad.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Quad q)
    {
        var result = IntersectRayQuad(Point, Direction, q.A, q.B, q.C, q.D);
        if (result.a.Valid || result.b.Valid)
        {
            var colPoints = new IntersectionPoints();
            if (result.a.Valid)
            {
                colPoints.Add(result.a);
            }

            if (result.b.Valid)
            {
                colPoints.Add(result.b);
            }

            return colPoints;
        }

        return null;
    }
    /// <summary>
    /// Computes all intersection points between this ray and a rectangle.
    /// </summary>
    /// <param name="r">The rectangle to test for intersection.</param>
    /// <returns>A <see cref="IntersectionPoints"/> object containing all intersection points, or null if there are none.</returns>
    /// <remarks>
    /// There can be zero, one, or two intersection points for a ray and a rectangle.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Rect r)
    {
        var result = IntersectRayQuad(Point, Direction, r.A, r.B, r.C, r.D);
        if (result.a.Valid || result.b.Valid)
        {
            var colPoints = new IntersectionPoints();
            if (result.a.Valid)
            {
                colPoints.Add(result.a);
            }

            if (result.b.Valid)
            {
                colPoints.Add(result.b);
            }

            return colPoints;
        }

        return null;
    }
    /// <summary>
    /// Computes all intersection points between this ray and a polygon.
    /// </summary>
    /// <param name="p">The polygon to test for intersection.</param>
    /// <param name="maxCollisionPoints">The maximum number of collision points to compute. Default is -1, which means no limit.</param>
    /// <returns>A <see cref="IntersectionPoints"/> object containing all intersection points, or null if there are none.</returns>
    /// <remarks>
    /// The method uses the ray-polygon intersection algorithm, which may return multiple intersection points depending on the polygon's shape and the ray's direction.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Polygon p, int maxCollisionPoints = -1)
    {
        return p.Count < 3 ? null : IntersectRayPolygon(Point, Direction, p, maxCollisionPoints);
    }

    /// <summary>
    /// Computes all intersection points between this ray and a polyline.
    /// </summary>
    /// <param name="pl">The polyline to test for intersection.</param>
    /// <param name="maxCollisionPoints">The maximum number of collision points to compute. Default is -1, which means no limit.</param>
    /// <returns>A <see cref="IntersectionPoints"/> object containing all intersection points, or null if there are none.</returns>
    /// <remarks>
    /// The method uses the ray-polyline intersection algorithm, which may return multiple intersection points depending on the polyline's shape and the ray's direction.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Polyline pl, int maxCollisionPoints = -1)
    {
        return pl.Count < 2 ? null : IntersectRayPolyline(Point, Direction, pl, maxCollisionPoints);
    }

    /// <summary>
    /// Computes all intersection points between this ray and a set of segments.
    /// </summary>
    /// <param name="segments">The set of segments to test for intersection.</param>
    /// <param name="maxCollisionPoints">The maximum number of collision points to compute. Default is -1, which means no limit.</param>
    /// <returns>A <see cref="IntersectionPoints"/> object containing all intersection points, or null if there are none.</returns>
    /// <remarks>
    /// The method uses the ray-segments intersection algorithm, which may return multiple intersection points depending on the segments' shapes and the ray's direction.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Segments segments, int maxCollisionPoints = -1) =>
        IntersectRaySegments(Point, Direction, segments, maxCollisionPoints);

    /// <summary>
    /// Computes all intersection points between this ray and a generic collider shape, and stores them in the provided <see cref="IntersectionPoints"/> object.
    /// </summary>
    /// <param name="collider">The collider to test for intersection. Must be enabled.</param>
    /// <param name="points">The <see cref="IntersectionPoints"/> object to store the intersection points.</param>
    /// <param name="returnAfterFirstValid">If true, the method returns after finding the first valid intersection point. Default is false.</param>
    /// <returns>The number of valid intersection points found.</returns>
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
    /// Computes all intersection points between this ray and another ray, and stores the result in the provided <see cref="IntersectionPoints"/> object.
    /// </summary>
    /// <param name="r">The other ray to test for intersection.</param>
    /// <param name="points">The <see cref="IntersectionPoints"/> object to store the intersection points.</param>
    /// <returns>The number of valid intersection points found.</returns>
    /// <remarks>
    /// Only a single intersection point is possible for two rays.
    /// </remarks>
    public int IntersectShape(Ray r, ref IntersectionPoints points)
    {
        var cp = IntersectRayRay(Point, Direction, r.Point, r.Direction, r.Normal);
        if (cp.Valid)
        {
            points.Add(cp);
            return 1;
        }

        return 0;
    }
    /// <summary>
    /// Computes all intersection points between this ray and a line, and stores the result in the provided <see cref="IntersectionPoints"/> object.
    /// </summary>
    /// <param name="l">The line to test for intersection.</param>
    /// <param name="points">The <see cref="IntersectionPoints"/> object to store the intersection points.</param>
    /// <returns>The number of valid intersection points found.</returns>
    /// <remarks>
    /// Only a single intersection point is possible for a ray and a line.
    /// </remarks>
    public int IntersectShape(Line l, ref IntersectionPoints points)
    {
        var cp = IntersectRayLine(Point, Direction, l.Point, l.Direction, l.Normal);
        if (cp.Valid)
        {
            points.Add(cp);
            return 1;
        }

        return 0;
    }
    /// <summary>
    /// Computes all intersection points between this ray and a segment, and stores the result in the provided <see cref="IntersectionPoints"/> object.
    /// </summary>
    /// <param name="s">The segment to test for intersection.</param>
    /// <param name="points">The <see cref="IntersectionPoints"/> object to store the intersection points.</param>
    /// <returns>The number of valid intersection points found.</returns>
    /// <remarks>
    /// Only a single intersection point is possible for a ray and a segment.
    /// </remarks>
    public int IntersectShape(Segment s, ref IntersectionPoints points)
    {
        var cp = IntersectRaySegment(Point, Direction, s.Start, s.End);
        if (cp.Valid)
        {
            points.Add(cp);
            return 1;
        }

        return 0;
    }
    /// <summary>
    /// Computes all intersection points between this ray and a circle, and stores the result in the provided <see cref="IntersectionPoints"/> object.
    /// </summary>
    /// <param name="c">The circle to test for intersection.</param>
    /// <param name="points">The <see cref="IntersectionPoints"/> object to store the intersection points.</param>
    /// <param name="returnAfterFirstValid">If true, the method returns after finding the first valid intersection point. Default is false.</param>
    /// <returns>The number of valid intersection points found.</returns>
    /// <remarks>
    /// There can be zero, one, or two intersection points for a ray and a circle.
    /// </remarks>
    public int IntersectShape(Circle c, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        var result = IntersectRayCircle(Point, Direction, c.Center, c.Radius);

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
    /// Computes all intersection points between this ray and a triangle, and stores the result in the provided <see cref="IntersectionPoints"/> object.
    /// </summary>
    /// <param name="t">The triangle to test for intersection.</param>
    /// <param name="points">The <see cref="IntersectionPoints"/> object to store the intersection points.</param>
    /// <param name="returnAfterFirstValid">If true, the method returns after finding the first valid intersection point. Default is false.</param>
    /// <returns>The number of valid intersection points found.</returns>
    /// <remarks>
    /// Intersecting a triangle with a ray can result in zero, one, or two intersection points.
    /// </remarks>
    public int IntersectShape(Triangle t, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        var cp = IntersectRaySegment(Point, Direction, t.A, t.B);
        var count = 0;
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        cp = IntersectRaySegment(Point, Direction, t.B, t.C);
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        //intersecting a triangle with a segment can not result in more than 2 intersection points
        if (count >= 2) return count;

        cp = IntersectRaySegment(Point, Direction, t.C, t.A);
        if (cp.Valid)
        {
            points.Add(cp);
            count++;
        }

        return count;
    }
    /// <summary>
    /// Computes all intersection points between this ray and a quad, and stores the result in the provided <see cref="IntersectionPoints"/> object.
    /// </summary>
    /// <param name="q">The quad to test for intersection.</param>
    /// <param name="points">The <see cref="IntersectionPoints"/> object to store the intersection points.</param>
    /// <param name="returnAfterFirstValid">If true, the method returns after finding the first valid intersection point. Default is false.</param>
    /// <returns>The number of valid intersection points found.</returns>
    /// <remarks>
    /// Intersecting a quad with a ray can result in zero, one, or two intersection points.
    /// </remarks>
    public int IntersectShape(Quad q, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        var cp = IntersectRaySegment(Point, Direction, q.A, q.B);
        var count = 0;
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        cp = IntersectRaySegment(Point, Direction, q.B, q.C);
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        //intersecting a quad with a segment can not result in more than 2 intersection points
        if (count >= 2) return count;

        cp = IntersectRaySegment(Point, Direction, q.C, q.D);
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        //intersecting a quad with a segment can not result in more than 2 intersection points
        if (count >= 2) return count;

        cp = IntersectRaySegment(Point, Direction, q.D, q.A);
        if (cp.Valid)
        {
            points.Add(cp);
            count++;
        }

        return count;
    }
    /// <summary>
    /// Computes all intersection points between this ray and a rectangle, and stores the result in the provided <see cref="IntersectionPoints"/> object.
    /// </summary>
    /// <param name="r">The rectangle to test for intersection.</param>
    /// <param name="points">The <see cref="IntersectionPoints"/> object to store the intersection points.</param>
    /// <param name="returnAfterFirstValid">If true, the method returns after finding the first valid intersection point. Default is false.</param>
    /// <returns>The number of valid intersection points found.</returns>
    /// <remarks>
    /// Intersecting a rectangle with a ray can result in zero, one, or two intersection points.
    /// </remarks>
    public int IntersectShape(Rect r, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        var a = r.TopLeft;
        var b = r.BottomLeft;

        var cp = IntersectRaySegment(Point, Direction, a, b);
        var count = 0;
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        var c = r.BottomRight;
        cp = IntersectRaySegment(Point, Direction, b, c);
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        //intersecting a rect with a segment can not result in more than 2 intersection points
        if (count >= 2) return count;

        var d = r.TopRight;
        cp = IntersectRaySegment(Point, Direction, c, d);
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        //intersecting a rect with a segment can not result in more than 2 intersection points
        if (count >= 2) return count;

        cp = IntersectRaySegment(Point, Direction, d, a);
        if (cp.Valid)
        {
            points.Add(cp);
            count++;
        }

        return count;
    }
    /// <summary>
    /// Computes all intersection points between this ray and a polygon, and stores the result in the provided <see cref="IntersectionPoints"/> object.
    /// </summary>
    /// <param name="p">The polygon to test for intersection.</param>
    /// <param name="points">The <see cref="IntersectionPoints"/> object to store the intersection points.</param>
    /// <param name="returnAfterFirstValid">If true, the method returns after finding the first valid intersection point. Default is false.</param>
    /// <returns>The number of valid intersection points found.</returns>
    /// <remarks>
    /// The method uses the ray-polygon intersection algorithm, which may return multiple intersection points depending on the polygon's shape and the ray's direction.
    /// </remarks>
    public int IntersectShape(Polygon p, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (p.Count < 3) return 0;
        var count = 0;
        for (var i = 0; i < p.Count; i++)
        {
            var cp = IntersectRaySegment(Point, Direction, p[i], p[(i + 1) % p.Count]);
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
    /// Computes all intersection points between this ray and a polyline, and stores the result in the provided <see cref="IntersectionPoints"/> object.
    /// </summary>
    /// <param name="pl">The polyline to test for intersection.</param>
    /// <param name="points">The <see cref="IntersectionPoints"/> object to store the intersection points.</param>
    /// <param name="returnAfterFirstValid">If true, the method returns after finding the first valid intersection point. Default is false.</param>
    /// <returns>The number of valid intersection points found.</returns>
    /// <remarks>
    /// The method uses the ray-polyline intersection algorithm, which may return multiple intersection points depending on the polyline's shape and the ray's direction.
    /// </remarks>
    public int IntersectShape(Polyline pl, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (pl.Count < 2) return 0;
        var count = 0;
        for (var i = 0; i < pl.Count - 1; i++)
        {
            var cp = IntersectRaySegment(Point, Direction, pl[i], pl[i + 1]);
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
    /// Computes all intersection points between this ray and a set of segments, and stores the result in the provided <see cref="IntersectionPoints"/> object.
    /// </summary>
    /// <param name="shape">The set of segments to test for intersection.</param>
    /// <param name="points">The <see cref="IntersectionPoints"/> object to store the intersection points.</param>
    /// <param name="returnAfterFirstValid">If true, the method returns after finding the first valid intersection point. Default is false.</param>
    /// <returns>The number of valid intersection points found.</returns>
    /// <remarks>
    /// The method uses the ray-segments intersection algorithm, which may return multiple intersection points depending on the segments' shapes and the ray's direction.
    /// </remarks>
    public int IntersectShape(Segments shape, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (shape.Count <= 0) return 0;
        var count = 0;

        foreach (var seg in shape)
        {
            var cp = IntersectRaySegment(Point, Direction, seg.Start, seg.End);
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