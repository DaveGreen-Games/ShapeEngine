using ShapeEngine.Core.CollisionSystem;

namespace ShapeEngine.Geometry.Line;

public static partial class LineIntersection
{
    /// <summary>
    /// Computes the intersection points between this line and a collider shape, if any.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="collider">The <see cref="Collider"/> whose shape will be checked for intersection with this line.</param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing the intersection points, or null if no intersections exist or the collider is disabled.
    /// </returns>
    /// <remarks>
    /// The function dispatches to the appropriate intersection method based on the collider's shape type.
    /// </remarks>
    public static CollisionPoints? Intersect(this Line self, Collider collider)
    {
        if (!collider.Enabled) return null;

        switch (collider.GetShapeType())
        {
            case ShapeType.Circle:
                var c = collider.GetCircleShape();
                return IntersectShape(self, c);
            case ShapeType.Ray:
                var rayShape = collider.GetRayShape();
                return IntersectShape(self, rayShape);
            case ShapeType.Line:
                var l = collider.GetLineShape();
                return IntersectShape(self, l);
            case ShapeType.Segment:
                var s = collider.GetSegmentShape();
                return IntersectShape(self, s);
            case ShapeType.Triangle:
                var t = collider.GetTriangleShape();
                return IntersectShape(self, t);
            case ShapeType.Rect:
                var r = collider.GetRectShape();
                return IntersectShape(self, r);
            case ShapeType.Quad:
                var q = collider.GetQuadShape();
                return IntersectShape(self, q);
            case ShapeType.Poly:
                var p = collider.GetPolygonShape();
                return IntersectShape(self, p);
            case ShapeType.PolyLine:
                var pl = collider.GetPolylineShape();
                return IntersectShape(self, pl);
        }

        return null;
    }

    /// <summary>
    /// Computes the intersection points between this line and a <see cref="Segment"/> shape.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="segment">The <see cref="Segment"/> to check for intersection.</param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing the intersection point, or null if no intersection exists.
    /// </returns>
    /// <remarks>
    /// This is a convenience method that uses the line's point and direction.
    /// </remarks>
    public static CollisionPoints? IntersectShape(this Line self, Segment.Segment segment)
    {
        var result = IntersectLineSegment(self.Point, self.Direction, segment.Start, segment.End, segment.Normal);
        if (result.Valid)
        {
            var colPoints = new CollisionPoints();
            colPoints.Add(result);
            return colPoints;
        }

        return null;
    }

    /// <summary>
    /// Computes the intersection points between this line and another <see cref="Line"/> shape.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="line">The <see cref="Line"/> to check for intersection.</param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing the intersection point, or null if no intersection exists.
    /// </returns>
    /// <remarks>
    /// This is a convenience method that uses the line's point and direction.
    /// </remarks>
    public static CollisionPoints? IntersectShape(this Line self, Line line)
    {
        var result = IntersectLineLine(self.Point, self.Direction, line.Point, line.Direction, line.Normal);
        if (result.Valid)
        {
            var colPoints = new CollisionPoints();
            colPoints.Add(result);
            return colPoints;
        }

        return null;
    }

    /// <summary>
    /// Computes the intersection points between this line and a <see cref="Ray"/> shape.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="ray">The <see cref="Ray"/> to check for intersection.</param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing the intersection point, or null if no intersection exists.
    /// </returns>
    /// <remarks>
    /// This is a convenience method that uses the line's point and direction.
    /// </remarks>
    public static CollisionPoints? IntersectShape(this Line self, Ray.Ray ray)
    {
        var result = IntersectLineRay(self.Point, self.Direction, ray.Point, ray.Direction, ray.Normal);
        if (result.Valid)
        {
            var colPoints = new CollisionPoints();
            colPoints.Add(result);
            return colPoints;
        }

        return null;
    }

    /// <summary>
    /// Computes the intersection points between this line and a <see cref="Circle"/> shape.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="circle">The <see cref="Circle"/> to check for intersection.</param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing the intersection points, or null if no intersection exists.
    /// </returns>
    /// <remarks>
    /// This is a convenience method that uses the line's point and direction.
    /// </remarks>
    public static CollisionPoints? IntersectShape(this Line self, Circle.Circle circle)
    {
        var result = IntersectLineCircle(self.Point, self.Direction, circle.Center, circle.Radius);
        if (result.a.Valid || result.b.Valid)
        {
            var colPoints = new CollisionPoints();
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
    /// <param name="self"> The line to use.</param>
    /// <param name="t">The <see cref="Triangle"/> to check for intersection.</param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing the intersection points, or null if no intersection exists.
    /// </returns>
    /// <remarks>
    /// This is a convenience method that uses the line's point and direction.
    /// </remarks>
    public static CollisionPoints? IntersectShape(this Line self, Triangle.Triangle t)
    {
        var result = IntersectLineTriangle(self.Point, self.Direction, t.A, t.B, t.C);
        if (result.a.Valid || result.b.Valid)
        {
            var colPoints = new CollisionPoints();
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
    /// <param name="self"> The line to use.</param>
    /// <param name="q">The <see cref="Quad"/> to check for intersection.</param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing the intersection points, or null if no intersection exists.
    /// </returns>
    /// <remarks>
    /// This is a convenience method that uses the line's point and direction.
    /// </remarks>
    public static CollisionPoints? IntersectShape(this Line self, Quad.Quad q)
    {
        var result = IntersectLineQuad(self.Point, self.Direction, q.A, q.B, q.C, q.D);
        if (result.a.Valid || result.b.Valid)
        {
            var colPoints = new CollisionPoints();
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
    /// <param name="self"> The line to use.</param>
    /// <param name="r">The <see cref="Rect"/> to check for intersection.</param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing the intersection points, or null if no intersection exists.
    /// </returns>
    /// <remarks>
    /// This is a convenience method that uses the line's point and direction.
    /// </remarks>
    public static CollisionPoints? IntersectShape(this Line self, Rect.Rect r)
    {
        //a test to see if 2 rays in opposite directions work
        // var result1 = Ray.IntersectRayRect(Point, Direction, r.A, r.B, r.C, r.D);
        // var result2 =  Ray.IntersectRayRect(Point, -Direction, r.A, r.B, r.C, r.D);
        //
        // if (result1.a.Valid || result1.b.Valid || result2.a.Valid || result2.b.Valid)
        // {
        //     var colPoints = new CollisionPoints();
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

        var result = IntersectLineQuad(self.Point, self.Direction, r.A, r.B, r.C, r.D);
        if (result.a.Valid || result.b.Valid)
        {
            var colPoints = new CollisionPoints();
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
    /// <param name="self"> The line to use.</param>
    /// <param name="p">The <see cref="Polygon"/> to check for intersection.</param>
    /// <param name="maxCollisionPoints">
    /// The maximum number of collision points to return. Use -1 for no limit.
    /// </param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing the intersection points, or null if no intersections exist.
    /// </returns>
    /// <remarks>
    /// This method iterates through all edges of the polygon and checks for intersections with the line.
    /// </remarks>
    public static CollisionPoints? IntersectShape(this Line self, Polygon.Polygon p, int maxCollisionPoints = -1) =>
        IntersectLinePolygon(self.Point, self.Direction, p, maxCollisionPoints);

    /// <summary>
    /// Computes the intersection points between this line and a <see cref="Polyline"/> shape.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="pl">The <see cref="Polyline"/> to check for intersection.</param>
    /// <param name="maxCollisionPoints">
    /// The maximum number of collision points to return. Use -1 for no limit.
    /// </param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing the intersection points, or null if no intersections exist.
    /// </returns>
    /// <remarks>
    /// This method iterates through all segments of the polyline and checks for intersections with the line.
    /// </remarks>
    public static CollisionPoints? IntersectShape(this Line self, Polyline.Polyline pl, int maxCollisionPoints = -1) =>
        IntersectLinePolyline(self.Point, self.Direction, pl, maxCollisionPoints);

    /// <summary>
    /// Computes the intersection points between this line and a <see cref="Segment.Segments"/> collection.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="segments">The <see cref="Segment.Segments"/> collection to check for intersections.</param>
    /// <param name="maxCollisionPoints">
    /// The maximum number of collision points to return. Use -1 for no limit.
    /// </param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing the intersection points, or null if no intersections exist.
    /// </returns>
    /// <remarks>
    /// This method iterates through all segments in the collection and checks for intersections with the line.
    /// </remarks>
    public static CollisionPoints? IntersectShape(this Line self, Segment.Segments segments, int maxCollisionPoints = -1) =>
        IntersectLineSegments(self.Point, self.Direction, segments, maxCollisionPoints);

    /// <summary>
    /// Computes the intersection points between this line and a collider shape, if any.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="collider">The <see cref="Collider"/> whose shape will be checked for intersection with this line.</param>
    /// <param name="points">A reference to a <see cref="CollisionPoints"/> collection to be populated with intersection points.</param>
    /// <param name="returnAfterFirstValid">
    /// If true, the function returns after finding the first valid intersection point. If false, all intersection points are found.
    /// </param>
    /// <returns>The number of intersection points found between the line and the collider's shape.</returns>
    /// <remarks>
    /// The function dispatches to the appropriate intersection method based on the collider's shape type.
    /// </remarks>
    public static int Intersect(this Line self, Collider collider, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (!collider.Enabled) return 0;

        switch (collider.GetShapeType())
        {
            case ShapeType.Circle:
                var c = collider.GetCircleShape();
                return IntersectShape(self, c, ref points, returnAfterFirstValid);
            case ShapeType.Ray:
                var rayShape = collider.GetRayShape();
                return IntersectShape(self, rayShape, ref points);
            case ShapeType.Line:
                var l = collider.GetLineShape();
                return IntersectShape(self, l, ref points);
            case ShapeType.Segment:
                var s = collider.GetSegmentShape();
                return IntersectShape(self, s, ref points);
            case ShapeType.Triangle:
                var t = collider.GetTriangleShape();
                return IntersectShape(self, t, ref points, returnAfterFirstValid);
            case ShapeType.Rect:
                var r = collider.GetRectShape();
                return IntersectShape(self, r, ref points, returnAfterFirstValid);
            case ShapeType.Quad:
                var q = collider.GetQuadShape();
                return IntersectShape(self, q, ref points, returnAfterFirstValid);
            case ShapeType.Poly:
                var p = collider.GetPolygonShape();
                return IntersectShape(self, p, ref points, returnAfterFirstValid);
            case ShapeType.PolyLine:
                var pl = collider.GetPolylineShape();
                return IntersectShape(self, pl, ref points, returnAfterFirstValid);
        }

        return 0;
    }

    /// <summary>
    /// Computes the intersection points between this line and a <see cref="Ray"/> shape.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="r">The <see cref="Ray"/> to check for intersection.</param>
    /// <param name="points">A reference to a <see cref="CollisionPoints"/> collection to be populated with intersection points.</param>
    /// <returns>The number of intersection points found between the line and the ray.</returns>
    /// <remarks>
    /// This method uses the line's point and direction to compute intersections with the given ray.
    /// </remarks>
    public static int IntersectShape(this Line self, Ray.Ray r, ref CollisionPoints points)
    {
        var cp = IntersectLineRay(self.Point, self.Direction, r.Point, r.Direction, r.Normal);
        if (cp.Valid)
        {
            points.Add(cp);
            return 1;
        }

        return 0;
    }

    /// <summary>
    /// Computes the intersection point between this line and another <see cref="Line"/> shape,
    /// populating a <see cref="CollisionPoints"/> collection if an intersection exists.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="l">The <see cref="Line"/> to check for intersection.</param>
    /// <param name="points">A reference to a <see cref="CollisionPoints"/> collection to be populated with intersection points.</param>
    /// <returns>The number of intersection points found between the two lines (0 or 1).</returns>
    public static int IntersectShape(this Line self, Line l, ref CollisionPoints points)
    {
        var cp = IntersectLineLine(self.Point, self.Direction, l.Point, l.Direction, l.Normal);
        if (cp.Valid)
        {
            points.Add(cp);
            return 1;
        }

        return 0;
    }

    /// <summary>
    /// Computes the intersection points between this line and a <see cref="Segment"/> shape, populating a <see cref="CollisionPoints"/> collection.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="s">The <see cref="Segment"/> to check for intersection.</param>
    /// <param name="points">A reference to a <see cref="CollisionPoints"/> collection to be populated with intersection points.</param>
    /// <returns>The number of intersection points found between the line and the segment.</returns>
    /// <remarks>
    /// This method checks if the line intersects the segment and, if so, adds the intersection point to the collection.
    /// </remarks>
    public static int IntersectShape(this Line self, Segment.Segment s, ref CollisionPoints points)
    {
        var cp = IntersectLineSegment(self.Point, self.Direction, s.Start, s.End);
        if (cp.Valid)
        {
            points.Add(cp);
            return 1;
        }

        return 0;
    }

    /// <summary>
    /// Computes the intersection points between this line and a <see cref="Circle"/> shape,
    /// populating a <see cref="CollisionPoints"/> collection.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="c">The <see cref="Circle"/> to check for intersection.</param>
    /// <param name="points">A reference to a <see cref="CollisionPoints"/> collection to be populated with intersection points.</param>
    /// <param name="returnAfterFirstValid">
    /// If true, the function returns after finding the first valid intersection point. If false, all intersection points are found.
    /// </param>
    /// <returns>The number of intersection points found between the line and the circle.</returns>
    /// <remarks>
    /// This method uses the line's point and direction to compute intersections with the given circle.
    /// </remarks>
    public static int IntersectShape(this Line self, Circle.Circle c, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var result = IntersectLineCircle(self.Point, self.Direction, c.Center, c.Radius);

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
    /// populating a <see cref="CollisionPoints"/> collection.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="t">The <see cref="Triangle"/> to check for intersection.</param>
    /// <param name="points">A reference to a <see cref="CollisionPoints"/> collection to be populated with intersection points.</param>
    /// <param name="returnAfterFirstValid">
    /// If true, the function returns after finding the first valid intersection point. If false, all intersection points are found.
    /// </param>
    /// <returns>The number of intersection points found between the line and the triangle.</returns>
    /// <remarks>
    /// This method uses the line's point and direction to compute intersections with the given triangle.
    /// </remarks>
    public static int IntersectShape(this Line self, Triangle.Triangle t, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var cp = IntersectLineSegment(self.Point, self.Direction, t.A, t.B);
        var count = 0;
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        cp = IntersectLineSegment(self.Point, self.Direction, t.B, t.C);
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        //intersecting a triangle with a segment can not result in more than 2 intersection points
        if (count >= 2) return count;

        cp = IntersectLineSegment(self.Point, self.Direction, t.C, t.A);
        if (cp.Valid)
        {
            points.Add(cp);
            count++;
        }

        return count;
    }

    /// <summary>
    /// Computes the intersection points between this line and a <see cref="Quad"/> shape,
    /// populating a <see cref="CollisionPoints"/> collection.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="q">The <see cref="Quad"/> to check for intersection.</param>
    /// <param name="points">A reference to a <see cref="CollisionPoints"/> collection to be populated with intersection points.</param>
    /// <param name="returnAfterFirstValid">
    /// If true, the function returns after finding the first valid intersection point. If false, all intersection points are found.
    /// </param>
    /// <returns>The number of intersection points found between the line and the quad.</returns>
    /// <remarks>
    /// This method uses the line's point and direction to compute intersections with the given quad.
    /// </remarks>
    public static int IntersectShape(this Line self, Quad.Quad q, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var cp = IntersectLineSegment(self.Point, self.Direction, q.A, q.B);
        var count = 0;
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        cp = IntersectLineSegment(self.Point, self.Direction, q.B, q.C);
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        //intersecting a quad with a segment can not result in more than 2 intersection points
        if (count >= 2) return count;

        cp = IntersectLineSegment(self.Point, self.Direction, q.C, q.D);
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        //intersecting a quad with a segment can not result in more than 2 intersection points
        if (count >= 2) return count;

        cp = IntersectLineSegment(self.Point, self.Direction, q.D, q.A);
        if (cp.Valid)
        {
            points.Add(cp);
            count++;
        }

        return count;
    }

    /// <summary>
    /// Computes the intersection points between this line and a <see cref="Rect"/> shape,
    /// populating a <see cref="CollisionPoints"/> collection.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="r">The <see cref="Rect"/> to check for intersection.</param>
    /// <param name="points">A reference to a <see cref="CollisionPoints"/> collection to be populated with intersection points.</param>
    /// <param name="returnAfterFirstValid">
    /// If true, the function returns after finding the first valid intersection point. If false, all intersection points are found.
    /// </param>
    /// <returns>The number of intersection points found between the line and the rectangle.</returns>
    /// <remarks>
    /// This method uses the line's point and direction to compute intersections with the given rectangle.
    /// </remarks>
    public static int IntersectShape(this Line self, Rect.Rect r, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var a = r.TopLeft;
        var b = r.BottomLeft;

        var cp = IntersectLineSegment(self.Point, self.Direction, a, b);
        var count = 0;
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        var c = r.BottomRight;
        cp = IntersectLineSegment(self.Point, self.Direction, b, c);
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        //intersecting a rect with a segment can not result in more than 2 intersection points
        if (count >= 2) return count;

        var d = r.TopRight;
        cp = IntersectLineSegment(self.Point, self.Direction, c, d);
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        //intersecting a rect with a segment can not result in more than 2 intersection points
        if (count >= 2) return count;

        cp = IntersectLineSegment(self.Point, self.Direction, d, a);
        if (cp.Valid)
        {
            points.Add(cp);
            count++;
        }

        return count;
    }

    /// <summary>
    /// Computes the intersection points between this line and a <see cref="Polygon"/> shape, populating a <see cref="CollisionPoints"/> collection.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="p">The <see cref="Polygon"/> to check for intersection.</param>
    /// <param name="points">A reference to a <see cref="CollisionPoints"/> collection to be populated with intersection points.</param>
    /// <param name="returnAfterFirstValid">
    /// If true, the function returns after finding the first valid intersection point. If false, all intersection points are found.
    /// </param>
    /// <returns>The number of intersection points found between the line and the polygon.</returns>
    /// <remarks>
    /// This method iterates through all edges of the polygon and checks for intersections with the line.
    /// </remarks>
    public static int IntersectShape(this Line self, Polygon.Polygon p, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (p.Count < 3) return 0;
        var count = 0;
        for (var i = 0; i < p.Count; i++)
        {
            var cp = IntersectLineSegment(self.Point, self.Direction, p[i], p[(i + 1) % p.Count]);
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
    /// Computes the intersection points between this line and a <see cref="Polyline"/> shape, populating a <see cref="CollisionPoints"/> collection.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="pl">The <see cref="Polyline"/> to check for intersection.</param>
    /// <param name="points">A reference to a <see cref="CollisionPoints"/> collection to be populated with intersection points.</param>
    /// <param name="returnAfterFirstValid">
    /// If true, the function returns after finding the first valid intersection point. If false, all intersection points are found.
    /// </param>
    /// <returns>The number of intersection points found between the line and the polyline.</returns>
    /// <remarks>
    /// This method iterates through all segments of the polyline and checks for intersections with the line.
    /// </remarks>
    public static int IntersectShape(this Line self, Polyline.Polyline pl, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (pl.Count < 2) return 0;
        var count = 0;
        for (var i = 0; i < pl.Count - 1; i++)
        {
            var cp = IntersectLineSegment(self.Point, self.Direction, pl[i], pl[i + 1]);
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
    /// Computes the intersection points between this line and a <see cref="Segment.Segments"/> collection,
    /// populating a <see cref="CollisionPoints"/> collection.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="shape">The <see cref="Segment.Segments"/> collection to check for intersections.</param>
    /// <param name="points">A reference to a <see cref="CollisionPoints"/> collection to be populated with intersection points.</param>
    /// <param name="returnAfterFirstValid">
    /// If true, the function returns after finding the first valid intersection point. If false, all intersection points are found.
    /// </param>
    /// <returns>The number of intersection points found between the line and the segments.</returns>
    /// <remarks>
    /// This method iterates through all segments in the collection and checks for intersections with the line.
    /// </remarks>
    public static int IntersectShape(this Line self, Segment.Segments shape, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (shape.Count <= 0) return 0;
        var count = 0;

        foreach (var seg in shape)
        {
            var cp = IntersectLineSegment(self.Point, self.Direction, seg.Start, seg.End);
            if (cp.Valid)
            {
                points.Add(cp);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }

        return count;
    }
}