using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.SegmentsDef;

public partial class Segments
{
    /// <summary>
    /// Computes intersection points between this segments  and all colliders in the specified <see cref="CollisionObject"/>.
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
    /// Computes all intersection points between this segments and a collider shape.
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
    /// Intersects a ray with the segments.
    /// </summary>
    /// <param name="r">The ray to intersect with.</param>
    /// <returns>A list of collision points if there are any intersections, otherwise null.</returns>
    public IntersectionPoints? IntersectShape(Ray r)
    {
        if (Count <= 0) return null;
        IntersectionPoints? points = null;

        foreach (var seg in this)
        {
            var result = Segment.IntersectSegmentRay(seg.Start, seg.End, r.Point, r.Direction, r.Normal);
            if (result.Valid)
            {
                points ??= [];
                points.AddRange(result);
            }
        }

        return points;
    }

    /// <summary>
    /// Intersects a line with the segments.
    /// </summary>
    /// <param name="l">The line to intersect with.</param>
    /// <returns>A list of collision points if there are any intersections, otherwise null.</returns>
    public IntersectionPoints? IntersectShape(Line l)
    {
        if (Count <= 0) return null;
        IntersectionPoints? points = null;

        foreach (var seg in this)
        {
            var result = Segment.IntersectSegmentLine(seg.Start, seg.End, l.Point, l.Direction, l.Normal);
            if (result.Valid)
            {
                points ??= [];
                points.AddRange(result);
            }
        }

        return points;
    }

    /// <summary>
    /// Intersects a segment with the segments.
    /// </summary>
    /// <param name="s">The segment to intersect with.</param>
    /// <returns>A list of collision points if there are any intersections, otherwise null.</returns>
    public IntersectionPoints? IntersectShape(Segment s)
    {
        if (Count <= 0) return null;
        IntersectionPoints? points = null;

        foreach (var seg in this)
        {
            var result = Segment.IntersectSegmentSegment(seg.Start, seg.End, s.Start, s.End);
            if (result.Valid)
            {
                points ??= [];
                points.AddRange(result);
            }
        }

        return points;
    }

    /// <summary>
    /// Intersects a circle with the segments.
    /// </summary>
    /// <param name="c">The circle to intersect with.</param>
    /// <returns>A list of collision points if there are any intersections, otherwise null.</returns>
    public IntersectionPoints? IntersectShape(Circle c)
    {
        if (Count <= 0) return null;
        IntersectionPoints? points = null;
        foreach (var seg in this)
        {
            var result = Segment.IntersectSegmentCircle(seg.Start, seg.End, c.Center, c.Radius);
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
        foreach (var segment in this)
        {
            var result = Segment.IntersectSegmentTriangle(segment.Start, segment.End, t.A, t.B, t.C);
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
        foreach (var segment in this)
        {
            var result = Segment.IntersectSegmentQuad(segment.Start, segment.End, q.A, q.B, q.C, q.D);
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
        foreach (var segment in this)
        {
            var result = Segment.IntersectSegmentRect(segment.Start, segment.End, r.A, r.B, r.C, r.D);
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
        foreach (var segment in this)
        {
            for (var i = 0; i < p.Count; i++)
            {
                var colPoint = Segment.IntersectSegmentSegment(segment.Start, segment.End, p[i], p[(i + 1) % p.Count]);
                if (colPoint.Valid)
                {
                    points ??= new();
                    points.Add(colPoint);
                }
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
        foreach (var segment in this)
        {
            for (var i = 0; i < pl.Count - 1; i++)
            {
                var colPoint = Segment.IntersectSegmentSegment(segment.Start, segment.End, pl[i], pl[i + 1]);
                if (colPoint.Valid)
                {
                    points ??= new();
                    points.Add(colPoint);
                }
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
    /// Intersects a set of segments with the segments.
    /// </summary>
    /// <param name="shape">The segments to intersect with.</param>
    /// <returns>A list of collision points if there are any intersections, otherwise null.</returns>
    public IntersectionPoints? IntersectShape(Segments shape)
    {
        if (Count <= 0) return null;
        IntersectionPoints? points = null;

        foreach (var seg in this)
        {
            foreach (var bSeg in shape)
            {
                var result = Segment.IntersectSegmentSegment(seg.Start, seg.End, bSeg.Start, bSeg.End);
                if (result.Valid)
                {
                    points ??= new();
                    points.AddRange(result);
                }
            }
        }

        return points;
    }

    /// <summary>
    /// Intersects a ray with the segments.
    /// </summary>
    /// <param name="r">The ray to intersect with.</param>
    /// <param name="points">The list of collision points to add to.</param>
    /// <param name="returnAfterFirstValid">If true, the method will return after the first valid intersection is found.</param>
    /// <returns>The number of intersections found.</returns>
    public int IntersectShape(Ray r, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count <= 0) return 0;
        var count = 0;

        foreach (var seg in this)
        {
            var result = Segment.IntersectSegmentRay(seg.Start, seg.End, r.Point, r.Direction, r.Normal);
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
    /// Intersects a line with the segments.
    /// </summary>
    /// <param name="l">The line to intersect with.</param>
    /// <param name="points">The list of collision points to add to.</param>
    /// <param name="returnAfterFirstValid">If true, the method will return after the first valid intersection is found.</param>
    /// <returns>The number of intersections found.</returns>
    public int IntersectShape(Line l, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count <= 0) return 0;
        var count = 0;

        foreach (var seg in this)
        {
            var result = Segment.IntersectSegmentLine(seg.Start, seg.End, l.Point, l.Direction, l.Normal);
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
    /// Intersects a segment with the segments.
    /// </summary>
    /// <param name="s">The segment to intersect with.</param>
    /// <param name="points">The list of collision points to add to.</param>
    /// <param name="returnAfterFirstValid">If true, the method will return after the first valid intersection is found.</param>
    /// <returns>The number of intersections found.</returns>
    public int IntersectShape(Segment s, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count <= 0) return 0;
        var count = 0;

        foreach (var seg in this)
        {
            var result = Segment.IntersectSegmentSegment(seg.Start, seg.End, s.Start, s.End);
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
    /// Intersects a circle with the segments.
    /// </summary>
    /// <param name="c">The circle to intersect with.</param>
    /// <param name="points">The list of collision points to add to.</param>
    /// <param name="returnAfterFirstValid">If true, the method will return after the first valid intersection is found.</param>
    /// <returns>The number of intersections found.</returns>
    public int IntersectShape(Circle c, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count <= 0) return 0;
        var count = 0;
        foreach (var seg in this)
        {
            var result = Segment.IntersectSegmentCircle(seg.Start, seg.End, c.Center, c.Radius);
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
    /// Intersects a set of segments with the segments.
    /// </summary>
    /// <param name="shape">The segments to intersect with.</param>
    /// <param name="points">The list of collision points to add to.</param>
    /// <param name="returnAfterFirstValid">If true, the method will return after the first valid intersection is found.</param>
    /// <returns>The number of intersections found.</returns>
    public int IntersectShape(Segments shape, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count <= 0) return 0;
        var count = 0;

        foreach (var seg in this)
        {
            foreach (var bSeg in shape)
            {
                var result = Segment.IntersectSegmentSegment(seg.Start, seg.End, bSeg.Start, bSeg.End);
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
    /// <summary>
    /// Computes all intersection points between this segments and a triangle.
    /// </summary>
    /// <param name="shape">The triangle to test against.</param>
    /// <param name="points">A reference to an <see cref="IntersectionPoints"/> collection to store intersection points.</param>
    /// <param name="returnAfterFirstValid">
    /// If true, the method returns after finding the first valid intersection point; otherwise, it finds all intersections.
    /// </param>
    /// <returns>The number of valid intersection points found.</returns>
    public int IntersectShape(Triangle shape, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if(Count <= 0) return 0;
        var count = 0;
        foreach (var segment in this)
        {
            var result = Segment.IntersectSegmentTriangle(segment.Start, segment.End, shape.A, shape.B, shape.C);
            if (result.a.Valid || result.b.Valid)
            {
                if (result.a.Valid)
                {
                    points.Add(result.a);
                    if(returnAfterFirstValid) return 1;
                    count++;
                }

                if (result.b.Valid)
                {
                    points.Add(result.b);
                    count++;
                }
            }
        }
        return count;
    }
    /// <summary>
    /// Computes all intersection points between this segments and a rectangle.
    /// </summary>
    /// <param name="shape">The rectangle to test against.</param>
    /// <param name="points">A reference to an <see cref="IntersectionPoints"/> collection to store intersection points.</param>
    /// <param name="returnAfterFirstValid">
    /// If true, the method returns after finding the first valid intersection point; otherwise, it finds all intersections.
    /// </param>
    /// <returns>The number of valid intersection points found.</returns>
    public int IntersectShape(Rect shape, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if(Count <= 0) return 0;
        var count = 0;
        foreach (var segment in this)
        {
            var result = Segment.IntersectSegmentRect(segment.Start, segment.End, shape.A, shape.B, shape.C, shape.D);
            if (result.a.Valid || result.b.Valid)
            {
                if (result.a.Valid)
                {
                    points.Add(result.a);
                    if(returnAfterFirstValid) return 1;
                    count++;
                }

                if (result.b.Valid)
                {
                    points.Add(result.b);
                    count++;
                }
            }
        }
        return count;
    }
    /// <summary>
    /// Computes all intersection points between this segments and a quadrilateral.
    /// </summary>
    /// <param name="shape">The quadrilateral to test against.</param>
    /// <param name="points">A reference to an <see cref="IntersectionPoints"/> collection to store intersection points.</param>
    /// <param name="returnAfterFirstValid">
    /// If true, the method returns after finding the first valid intersection point; otherwise, it finds all intersections.
    /// </param>
    /// <returns>The number of valid intersection points found.</returns>
    public int IntersectShape(Quad shape, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if(Count <= 0) return 0;
        var count = 0;
        foreach (var segment in this)
        {
            var result = Segment.IntersectSegmentQuad(segment.Start, segment.End, shape.A, shape.B, shape.C, shape.D);
            if (result.a.Valid || result.b.Valid)
            {
                if (result.a.Valid)
                {
                    points.Add(result.a);
                    if(returnAfterFirstValid) return 1;
                    count++;
                }

                if (result.b.Valid)
                {
                    points.Add(result.b);
                    count++;
                }
            }
        }
        return count;
    }
    /// <summary>
    /// Computes all intersection points between this segments and a polygon.
    /// </summary>
    /// <param name="shape">The polygon to test against. Must have at least 3 vertices.</param>
    /// <param name="points">A reference to an <see cref="IntersectionPoints"/> collection to store intersection points.</param>
    /// <param name="returnAfterFirstValid">
    /// If true, the method returns after finding the first valid intersection point; otherwise, it finds all intersections.
    /// </param>
    /// <returns>The number of valid intersection points found.</returns>
    public int IntersectShape(Polygon shape, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count <= 0 || shape.Count < 3) return 0;
        var count = 0;
        foreach (var segment in this)
        {
            for (var i = 0; i < shape.Count; i++)
            {
                var colPoint = Segment.IntersectSegmentSegment(segment.Start, segment.End, shape[i], shape[(i + 1) % shape.Count]);
                if (colPoint.Valid)
                {
                    points.Add(colPoint);
                    if (returnAfterFirstValid) return 1;
                    count++;
                }
            }
        }
        return count;
    }
    /// <summary>
    /// Computes all intersection points between this segments and a polyline.
    /// </summary>
    /// <param name="shape">The polyline to test against. Must have at least 2 vertices.</param>
    /// <param name="points">A reference to an <see cref="IntersectionPoints"/> collection to store intersection points.</param>
    /// <param name="returnAfterFirstValid">
    /// If true, the method returns after finding the first valid intersection point; otherwise, it finds all intersections.
    /// </param>
    /// <returns>The number of valid intersection points found.</returns>
    public int IntersectShape(Polyline shape, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count <= 0 || shape.Count < 2) return 0;
        var count = 0;
        foreach (var segment in this)
        {
            for (var i = 0; i < shape.Count - 1; i++)
            {
                var colPoint = Segment.IntersectSegmentSegment(segment.Start, segment.End, shape[i], shape[i + 1]);
                if (colPoint.Valid)
                {
                    points.Add(colPoint);
                    if (returnAfterFirstValid) return 1;
                    count++;
                }
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