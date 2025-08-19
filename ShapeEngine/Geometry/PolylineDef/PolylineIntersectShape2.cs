using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.PolylineDef;

public partial class Polyline
{
    /// <summary>
    /// Computes the intersection points between this polyline and another collider's shape,
    /// storing results in a provided <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="collider">The collider whose shape will be tested for intersection with this polyline.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection
    /// where intersection points will be stored.</param>
    /// <param name="returnAfterFirstValid">If true, the method returns after finding the first valid intersection;
    /// otherwise, it finds all intersections.</param>
    /// <returns>The number of intersection points found.</returns>
    /// <remarks>
    /// The method dispatches to the appropriate intersection routine based on the collider's shape type.
    /// This overload allows for efficient accumulation of results.
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
    /// Computes the intersection points between this polyline and a <see cref="Ray"/>, storing results in a provided <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="r">The <see cref="Ray"/> to test for intersection.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection where intersection points will be stored.</param>
    /// <param name="returnAfterFirstValid">If true, the method returns after finding the first valid intersection; otherwise, it finds all intersections.</param>
    /// <returns>The number of intersection points found.</returns>
    /// <remarks>
    /// Iterates through each segment of the polyline and checks for intersection with the ray.
    /// This overload allows for efficient accumulation of results.
    /// </remarks>
    public int IntersectShape(Ray r, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 2) return 0;

        var count = 0;
        for (var i = 0; i < Count - 1; i++)
        {
            var result = Segment.IntersectSegmentRay(this[i], this[(i + 1) % Count], r.Point, r.Direction, r.Normal);
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
    /// Computes the intersection points between this polyline and a <see cref="Line"/>,
    /// storing results in a provided <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="l">The <see cref="Line"/> to test for intersection.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection where intersection points will be stored.</param>
    /// <param name="returnAfterFirstValid">If true, the method returns after finding the first valid intersection;
    /// otherwise, it finds all intersections.</param>
    /// <returns>The number of intersection points found.</returns>
    /// <remarks>
    /// Iterates through each segment of the polyline and checks for intersection with the line.
    /// This overload allows for efficient accumulation of results.
    /// </remarks>
    public int IntersectShape(Line l, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 2) return 0;

        var count = 0;
        for (var i = 0; i < Count - 1; i++)
        {
            var result = Segment.IntersectSegmentLine(this[i], this[(i + 1) % Count], l.Point, l.Direction, l.Normal);
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
    /// Computes the intersection points between this polyline and a <see cref="Segment"/>,
    /// storing results in a provided <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="s">The <see cref="Segment"/> to test for intersection.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection where intersection points will be stored.</param>
    /// <param name="returnAfterFirstValid">If true, the method returns after finding the first valid intersection;
    /// otherwise, it finds all intersections.</param>
    /// <returns>The number of intersection points found.</returns>
    /// <remarks>
    /// Iterates through each segment of the polyline and checks for intersection with the segment.
    /// This overload allows for efficient accumulation of results.
    /// </remarks>
    public int IntersectShape(Segment s, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 2) return 0;

        var count = 0;
        for (var i = 0; i < Count - 1; i++)
        {
            var result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], s.Start, s.End);
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
    /// Computes the intersection points between this polyline and a <see cref="Circle"/>,
    /// storing results in a provided <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="c">The <see cref="Circle"/> to test for intersection.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection where intersection points will be stored.</param>
    /// <param name="returnAfterFirstValid">If true, the method returns after finding the first valid intersection;
    /// otherwise, it finds all intersections.</param>
    /// <returns>The number of intersection points found.</returns>
    /// <remarks>
    /// Iterates through each segment of the polyline and checks for intersection with the circle.
    /// This overload allows for efficient accumulation of results.
    /// </remarks>
    public int IntersectShape(Circle c, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 2) return 0;

        var count = 0;

        for (var i = 0; i < Count - 1; i++)
        {
            var result = Segment.IntersectSegmentCircle(this[i], this[(i + 1) % Count], c.Center, c.Radius);
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
    /// Computes the intersection points between this polyline and a <see cref="Triangle"/>,
    /// storing results in a provided <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="t">The <see cref="Triangle"/> to test for intersection.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection where intersection points will be stored.</param>
    /// <param name="returnAfterFirstValid">If true, the method returns after finding the first valid intersection;
    /// otherwise, it finds all intersections.</param>
    /// <returns>The number of intersection points found.</returns>
    /// <remarks>
    /// Iterates through each segment of the polyline and each edge of the triangle, checking for intersections.
    /// This overload allows for efficient accumulation of results.
    /// </remarks>
    public int IntersectShape(Triangle t, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 2) return 0;

        var count = 0;
        for (var i = 0; i < Count - 1; i++)
        {
            var result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], t.A, t.B);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], t.B, t.C);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], t.C, t.A);
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
    /// Computes the intersection points between this polyline and a <see cref="Quad"/>,
    /// storing results in a provided <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="q">The <see cref="Quad"/> to test for intersection.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection where intersection points will be stored.</param>
    /// <param name="returnAfterFirstValid">If true, the method returns after finding the first valid intersection;
    /// otherwise, it finds all intersections.</param>
    /// <returns>The number of intersection points found.</returns>
    /// <remarks>
    /// Iterates through each segment of the polyline and each edge of the quad, checking for intersections.
    /// This overload allows for efficient accumulation of results.
    /// </remarks>
    public int IntersectShape(Quad q, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 2) return 0;

        var count = 0;
        for (var i = 0; i < Count - 1; i++)
        {
            var result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], q.A, q.B);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], q.B, q.C);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], q.C, q.D);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], q.D, q.A);
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
    /// Computes the intersection points between this polyline and a <see cref="Rect"/>, storing results in a provided <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="r">The <see cref="Rect"/> to test for intersection.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection
    /// where intersection points will be stored.</param>
    /// <param name="returnAfterFirstValid">If true, the method returns after finding the first valid intersection;
    /// otherwise, it finds all intersections.</param>
    /// <returns>The number of intersection points found.</returns>
    /// <remarks>
    /// Iterates through each segment of the polyline and each edge of the rectangle, checking for intersections.
    /// This overload allows for efficient accumulation of results.
    /// </remarks>
    public int IntersectShape(Rect r, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 2) return 0;

        var count = 0;
        var a = r.TopLeft;
        var b = r.BottomLeft;
        var c = r.BottomRight;
        var d = r.TopRight;
        for (var i = 0; i < Count - 1; i++)
        {
            var result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], a, b);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], b, c);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], c, d);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], d, a);
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
    /// Computes the intersection points between this polyline and a <see cref="Polygon"/>,
    /// storing results in a provided <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="p">The <see cref="Polygon"/> to test for intersection.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection
    /// where intersection points will be stored.</param>
    /// <param name="returnAfterFirstValid">If true, the method returns after finding the first valid intersection;
    /// otherwise, it finds all intersections.</param>
    /// <returns>The number of intersection points found.</returns>
    /// <remarks>
    /// Iterates through each segment of the polyline and each edge of the polygon, checking for intersections.
    /// This overload allows for efficient accumulation of results.
    /// </remarks>
    public int IntersectShape(Polygon p, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (p.Count < 3 || Count < 2) return 0;
        var count = 0;
        for (var i = 0; i < Count - 1; i++)
        {
            for (var j = 0; j < p.Count; j++)
            {
                var result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], p[j], p[(j + 1) % p.Count]);
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
    /// Computes the intersection points between this polyline and another <see cref="Polyline"/>,
    /// storing results in a provided <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="pl">The <see cref="Polyline"/> to test for intersection.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection
    /// where intersection points will be stored.</param>
    /// <param name="returnAfterFirstValid">If true, the method returns after finding the first valid intersection;
    /// otherwise, it finds all intersections.</param>
    /// <returns>The number of intersection points found.</returns>
    /// <remarks>
    /// Iterates through each segment of both polylines, checking for intersections between all segment pairs.
    /// This overload allows for efficient accumulation of results.
    /// </remarks>
    public int IntersectShape(Polyline pl, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (pl.Count < 2 || Count < 2) return 0;
        var count = 0;
        for (var i = 0; i < Count - 1; i++)
        {
            for (var j = 0; j < pl.Count - 1; j++)
            {
                var result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], pl[j], pl[(j + 1) % pl.Count]);
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
    /// Computes the intersection points between this polyline and a <see cref="Segments"/> collection,
    /// storing results in a provided <see cref="IntersectionPoints"/> collection.
    /// </summary>
    /// <param name="shape">The <see cref="Segments"/> collection to test for intersection.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection
    /// where intersection points will be stored.</param>
    /// <param name="returnAfterFirstValid">If true, the method returns after finding the first valid intersection;
    /// otherwise, it finds all intersections.</param>
    /// <returns>The number of intersection points found.</returns>
    /// <remarks>
    /// Iterates through each segment of the polyline and each segment in the collection, checking for intersections.
    /// This overload allows for efficient accumulation of results.
    /// </remarks>
    public int IntersectShape(Segments shape, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 2 || shape.Count <= 0) return 0;
        var count = 0;
        for (var i = 0; i < Count - 1; i++)
        {
            foreach (var seg in shape)
            {
                var result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], seg.Start, seg.End);
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