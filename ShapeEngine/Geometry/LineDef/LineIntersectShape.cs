using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.LineDef;

public readonly partial struct Line
{
    /// <summary>
    /// Computes intersection points between this line and all colliders in the specified <see cref="CollisionObject"/>.
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
    /// Computes the intersection points between this line and a collider shape, if any.
    /// </summary>
    /// <param name="collider">The <see cref="Collider"/> whose shape will be checked for intersection with this line.</param>
    /// <returns>
    /// A <see cref="IntersectionPoints"/> object containing the intersection points, or null if no intersections exist or the collider is disabled.
    /// </returns>
    /// <remarks>
    /// The function dispatches to the appropriate intersection method based on the collider's shape type.
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
    /// Computes the intersection points between this line and a <see cref="Segment"/> shape.
    /// </summary>
    /// <param name="segment">The <see cref="Segment"/> to check for intersection.</param>
    /// <returns>
    /// A <see cref="IntersectionPoints"/> object containing the intersection point, or null if no intersection exists.
    /// </returns>
    /// <remarks>
    /// This is a convenience method that uses the line's point and direction.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Segment segment)
    {
        var result = IntersectLineSegment(Point, Direction, segment.Start, segment.End, segment.Normal);
        if (result.Valid)
        {
            var colPoints = new IntersectionPoints();
            colPoints.Add(result);
            return colPoints;
        }

        return null;
    }

    /// <summary>
    /// Computes the intersection points between this line and another <see cref="Line"/> shape.
    /// </summary>
    /// <param name="line">The <see cref="Line"/> to check for intersection.</param>
    /// <returns>
    /// A <see cref="IntersectionPoints"/> object containing the intersection point, or null if no intersection exists.
    /// </returns>
    /// <remarks>
    /// This is a convenience method that uses the line's point and direction.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Line line)
    {
        var result = IntersectLineLine(Point, Direction, line.Point, line.Direction, line.Normal);
        if (result.Valid)
        {
            var colPoints = new IntersectionPoints();
            colPoints.Add(result);
            return colPoints;
        }

        return null;
    }

    /// <summary>
    /// Computes the intersection points between this line and a <see cref="Ray"/> shape.
    /// </summary>
    /// <param name="ray">The <see cref="Ray"/> to check for intersection.</param>
    /// <returns>
    /// A <see cref="IntersectionPoints"/> object containing the intersection point, or null if no intersection exists.
    /// </returns>
    /// <remarks>
    /// This is a convenience method that uses the line's point and direction.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Ray ray)
    {
        var result = IntersectLineRay(Point, Direction, ray.Point, ray.Direction, ray.Normal);
        if (result.Valid)
        {
            var colPoints = new IntersectionPoints();
            colPoints.Add(result);
            return colPoints;
        }

        return null;
    }

    /// <summary>
    /// Computes the intersection points between this line and a <see cref="Circle"/> shape.
    /// </summary>
    /// <param name="circle">The <see cref="Circle"/> to check for intersection.</param>
    /// <returns>
    /// A <see cref="IntersectionPoints"/> object containing the intersection points, or null if no intersection exists.
    /// </returns>
    /// <remarks>
    /// This is a convenience method that uses the line's point and direction.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Circle circle)
    {
        var result = IntersectLineCircle(Point, Direction, circle.Center, circle.Radius);
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
    /// Computes the intersection points between this line and a <see cref="Triangle"/> shape.
    /// </summary>
    /// <param name="t">The <see cref="Triangle"/> to check for intersection.</param>
    /// <returns>
    /// A <see cref="IntersectionPoints"/> object containing the intersection points, or null if no intersection exists.
    /// </returns>
    /// <remarks>
    /// This is a convenience method that uses the line's point and direction.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Triangle t)
    {
        var result = IntersectLineTriangle(Point, Direction, t.A, t.B, t.C);
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
    /// Computes the intersection points between this line and a <see cref="Quad"/> shape.
    /// </summary>
    /// <param name="q">The <see cref="Quad"/> to check for intersection.</param>
    /// <returns>
    /// A <see cref="IntersectionPoints"/> object containing the intersection points, or null if no intersection exists.
    /// </returns>
    /// <remarks>
    /// This is a convenience method that uses the line's point and direction.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Quad q)
    {
        var result = IntersectLineQuad(Point, Direction, q.A, q.B, q.C, q.D);
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
    /// Computes the intersection points between this line and a <see cref="Rect"/> shape.
    /// </summary>
    /// <param name="r">The <see cref="Rect"/> to check for intersection.</param>
    /// <returns>
    /// A <see cref="IntersectionPoints"/> object containing the intersection points, or null if no intersection exists.
    /// </returns>
    /// <remarks>
    /// This is a convenience method that uses the line's point and direction.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Rect r)
    {
        //a test to see if 2 rays in opposite directions work
        // var result1 = Ray.IntersectRayRect(Point, Direction, r.A, r.B, r.C, r.D);
        // var result2 =  Ray.IntersectRayRect(Point, -Direction, r.A, r.B, r.C, r.D);
        //
        // if (result1.a.Valid || result1.b.Valid || result2.a.Valid || result2.b.Valid)
        // {
        //     var colPoints = new IntersectionPoints();
        //     if (result1.a.Valid)
        //     {
        //         colPoints.Add(result1.a);
        //     }
        //     if (result1.b.Valid)
        //     {
        //         colPoints.Add(result1.b);
        //     }
        //     if (result2.a.Valid)
        //     {
        //         colPoints.Add(result2.a);
        //     }
        //     if (result2.b.Valid)
        //     {
        //         colPoints.Add(result2.b);
        //     }
        //     return colPoints;
        // }
        //
        // return null;

        var result = IntersectLineQuad(Point, Direction, r.A, r.B, r.C, r.D);
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
    /// Computes the intersection points between this line and a <see cref="Polygon"/> shape.
    /// </summary>
    /// <param name="p">The <see cref="Polygon"/> to check for intersection.</param>
    /// <param name="maxCollisionPoints">
    /// The maximum number of collision points to return. Use -1 for no limit.
    /// </param>
    /// <returns>
    /// A <see cref="IntersectionPoints"/> object containing the intersection points, or null if no intersections exist.
    /// </returns>
    /// <remarks>
    /// This method iterates through all edges of the polygon and checks for intersections with the line.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Polygon p, int maxCollisionPoints = -1) => IntersectLinePolygon(Point, Direction, p, maxCollisionPoints);

    /// <summary>
    /// Computes the intersection points between this line and a <see cref="Polyline"/> shape.
    /// </summary>
    /// <param name="pl">The <see cref="Polyline"/> to check for intersection.</param>
    /// <param name="maxCollisionPoints">
    /// The maximum number of collision points to return. Use -1 for no limit.
    /// </param>
    /// <returns>
    /// A <see cref="IntersectionPoints"/> object containing the intersection points, or null if no intersections exist.
    /// </returns>
    /// <remarks>
    /// This method iterates through all segments of the polyline and checks for intersections with the line.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Polyline pl, int maxCollisionPoints = -1) =>
        IntersectLinePolyline(Point, Direction, pl, maxCollisionPoints);

    /// <summary>
    /// Computes the intersection points between this line and a <see cref="Segments"/> collection.
    /// </summary>
    /// <param name="segments">The <see cref="Segments"/> collection to check for intersections.</param>
    /// <param name="maxCollisionPoints">
    /// The maximum number of collision points to return. Use -1 for no limit.
    /// </param>
    /// <returns>
    /// A <see cref="IntersectionPoints"/> object containing the intersection points, or null if no intersections exist.
    /// </returns>
    /// <remarks>
    /// This method iterates through all segments in the collection and checks for intersections with the line.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Segments segments, int maxCollisionPoints = -1) =>
        IntersectLineSegments(Point, Direction, segments, maxCollisionPoints);

    /// <summary>
    /// Computes the intersection points between this line and a collider shape, populating a <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="collider">The <see cref="Collider"/> whose shape will be checked for intersection with this line.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection to be populated with intersection points.</param>
    /// <param name="returnAfterFirstValid">
    /// If true, the function returns after finding the first valid intersection point. If false, all intersection points are found.
    /// </param>
    /// <returns>The number of intersection points found between the line and the collider's shape.</returns>
    /// <remarks>
    /// The function dispatches to the appropriate intersection method based on the collider's shape type.
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
    /// Computes the intersection points between this line and a <see cref="Ray"/> shape.
    /// </summary>
    /// <param name="r">The <see cref="Ray"/> to check for intersection.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection to be populated with intersection points.</param>
    /// <returns>The number of intersection points found between the line and the ray.</returns>
    /// <remarks>
    /// This method uses the line's point and direction to compute intersections with the given ray.
    /// </remarks>
    public int IntersectShape(Ray r, ref IntersectionPoints points)
    {
        var cp = IntersectLineRay(Point, Direction, r.Point, r.Direction, r.Normal);
        if (cp.Valid)
        {
            points.Add(cp);
            return 1;
        }

        return 0;
    }

    /// <summary>
    /// Computes the intersection point between this line and another <see cref="Line"/> shape,
    /// populating a <see cref="IntersectionPoints"/> collection if an intersection exists.
    /// </summary>
    /// <param name="l">The <see cref="Line"/> to check for intersection.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection to be populated with intersection points.</param>
    /// <returns>The number of intersection points found between the two lines (0 or 1).</returns>
    public int IntersectShape(Line l, ref IntersectionPoints points)
    {
        var cp = IntersectLineLine(Point, Direction, l.Point, l.Direction, l.Normal);
        if (cp.Valid)
        {
            points.Add(cp);
            return 1;
        }

        return 0;
    }

    /// <summary>
    /// Computes the intersection points between this line and a <see cref="Segment"/> shape, populating a <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="s">The <see cref="Segment"/> to check for intersection.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection to be populated with intersection points.</param>
    /// <returns>The number of intersection points found between the line and the segment.</returns>
    /// <remarks>
    /// This method checks if the line intersects the segment and, if so, adds the intersection point to the collection.
    /// </remarks>
    public int IntersectShape(Segment s, ref IntersectionPoints points)
    {
        var cp = IntersectLineSegment(Point, Direction, s.Start, s.End);
        if (cp.Valid)
        {
            points.Add(cp);
            return 1;
        }

        return 0;
    }

    /// <summary>
    /// Computes the intersection points between this line and a <see cref="Circle"/> shape,
    /// populating a <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="c">The <see cref="Circle"/> to check for intersection.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection to be populated with intersection points.</param>
    /// <param name="returnAfterFirstValid">
    /// If true, the function returns after finding the first valid intersection point. If false, all intersection points are found.
    /// </param>
    /// <returns>The number of intersection points found between the line and the circle.</returns>
    /// <remarks>
    /// This method uses the line's point and direction to compute intersections with the given circle.
    /// </remarks>
    public int IntersectShape(Circle c, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        var result = IntersectLineCircle(Point, Direction, c.Center, c.Radius);

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
    /// Computes the intersection points between this line and a <see cref="Triangle"/> shape,
    /// populating a <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="t">The <see cref="Triangle"/> to check for intersection.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection to be populated with intersection points.</param>
    /// <param name="returnAfterFirstValid">
    /// If true, the function returns after finding the first valid intersection point. If false, all intersection points are found.
    /// </param>
    /// <returns>The number of intersection points found between the line and the triangle.</returns>
    /// <remarks>
    /// This method uses the line's point and direction to compute intersections with the given triangle.
    /// </remarks>
    public int IntersectShape(Triangle t, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        var cp = IntersectLineSegment(Point, Direction, t.A, t.B);
        var count = 0;
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        cp = IntersectLineSegment(Point, Direction, t.B, t.C);
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        //intersecting a triangle with a segment can not result in more than 2 intersection points
        if (count >= 2) return count;

        cp = IntersectLineSegment(Point, Direction, t.C, t.A);
        if (cp.Valid)
        {
            points.Add(cp);
            count++;
        }

        return count;
    }

    /// <summary>
    /// Computes the intersection points between this line and a <see cref="Quad"/> shape,
    /// populating a <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="q">The <see cref="Quad"/> to check for intersection.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection to be populated with intersection points.</param>
    /// <param name="returnAfterFirstValid">
    /// If true, the function returns after finding the first valid intersection point. If false, all intersection points are found.
    /// </param>
    /// <returns>The number of intersection points found between the line and the quad.</returns>
    /// <remarks>
    /// This method uses the line's point and direction to compute intersections with the given quad.
    /// </remarks>
    public int IntersectShape(Quad q, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        var cp = IntersectLineSegment(Point, Direction, q.A, q.B);
        var count = 0;
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        cp = IntersectLineSegment(Point, Direction, q.B, q.C);
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        //intersecting a quad with a segment can not result in more than 2 intersection points
        if (count >= 2) return count;

        cp = IntersectLineSegment(Point, Direction, q.C, q.D);
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        //intersecting a quad with a segment can not result in more than 2 intersection points
        if (count >= 2) return count;

        cp = IntersectLineSegment(Point, Direction, q.D, q.A);
        if (cp.Valid)
        {
            points.Add(cp);
            count++;
        }

        return count;
    }

    /// <summary>
    /// Computes the intersection points between this line and a <see cref="Rect"/> shape,
    /// populating a <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="r">The <see cref="Rect"/> to check for intersection.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection to be populated with intersection points.</param>
    /// <param name="returnAfterFirstValid">
    /// If true, the function returns after finding the first valid intersection point. If false, all intersection points are found.
    /// </param>
    /// <returns>The number of intersection points found between the line and the rectangle.</returns>
    /// <remarks>
    /// This method uses the line's point and direction to compute intersections with the given rectangle.
    /// </remarks>
    public int IntersectShape(Rect r, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        var a = r.TopLeft;
        var b = r.BottomLeft;

        var cp = IntersectLineSegment(Point, Direction, a, b);
        var count = 0;
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        var c = r.BottomRight;
        cp = IntersectLineSegment(Point, Direction, b, c);
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        //intersecting a rect with a segment can not result in more than 2 intersection points
        if (count >= 2) return count;

        var d = r.TopRight;
        cp = IntersectLineSegment(Point, Direction, c, d);
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        //intersecting a rect with a segment can not result in more than 2 intersection points
        if (count >= 2) return count;

        cp = IntersectLineSegment(Point, Direction, d, a);
        if (cp.Valid)
        {
            points.Add(cp);
            count++;
        }

        return count;
    }

    /// <summary>
    /// Computes the intersection points between this line and a <see cref="Polygon"/> shape, populating a <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="p">The <see cref="Polygon"/> to check for intersection.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection to be populated with intersection points.</param>
    /// <param name="returnAfterFirstValid">
    /// If true, the function returns after finding the first valid intersection point. If false, all intersection points are found.
    /// </param>
    /// <returns>The number of intersection points found between the line and the polygon.</returns>
    /// <remarks>
    /// This method iterates through all edges of the polygon and checks for intersections with the line.
    /// </remarks>
    public int IntersectShape(Polygon p, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (p.Count < 3) return 0;
        var count = 0;
        for (var i = 0; i < p.Count; i++)
        {
            var cp = IntersectLineSegment(Point, Direction, p[i], p[(i + 1) % p.Count]);
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
    /// Computes the intersection points between this line and a <see cref="Polyline"/> shape, populating a <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="pl">The <see cref="Polyline"/> to check for intersection.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection to be populated with intersection points.</param>
    /// <param name="returnAfterFirstValid">
    /// If true, the function returns after finding the first valid intersection point. If false, all intersection points are found.
    /// </param>
    /// <returns>The number of intersection points found between the line and the polyline.</returns>
    /// <remarks>
    /// This method iterates through all segments of the polyline and checks for intersections with the line.
    /// </remarks>
    public int IntersectShape(Polyline pl, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (pl.Count < 2) return 0;
        var count = 0;
        for (var i = 0; i < pl.Count - 1; i++)
        {
            var cp = IntersectLineSegment(Point, Direction, pl[i], pl[i + 1]);
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
    /// Computes the intersection points between this line and a <see cref="Segments"/> collection,
    /// populating a <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="shape">The <see cref="Segments"/> collection to check for intersections.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection to be populated with intersection points.</param>
    /// <param name="returnAfterFirstValid">
    /// If true, the function returns after finding the first valid intersection point. If false, all intersection points are found.
    /// </param>
    /// <returns>The number of intersection points found between the line and the segments.</returns>
    /// <remarks>
    /// This method iterates through all segments in the collection and checks for intersections with the line.
    /// </remarks>
    public int IntersectShape(Segments shape, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (shape.Count <= 0) return 0;
        var count = 0;

        foreach (var seg in shape)
        {
            var cp = IntersectLineSegment(Point, Direction, seg.Start, seg.End);
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