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
    /// Computes intersection points between this polyline and all colliders in the specified <see cref="CollisionObject"/>.
    /// </summary>
    /// <param name="collisionObject">The collision object containing colliders to test for intersection.</param>
    /// <returns>
    /// A <see cref="Dictionary{Collider, IntersectionPoints}"/> mapping each collider to its intersection points,
    /// or null if no colliders are present or no intersections are found.
    /// </returns>
    public Dictionary<Collider, IntersectionPoints>? Intersect(CollisionObject collisionObject)
    {
        if (Count < 2) return null;
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
    /// Computes the intersection points between this polyline and another collider's shape.
    /// </summary>
    /// <param name="collider">The collider whose shape will be tested for intersection with this polyline.</param>
    /// <returns>
    /// A <see cref="IntersectionPoints"/> object containing intersection points,
    /// or <c>null</c> if there are no intersections or the collider is disabled.
    /// </returns>
    /// <remarks>
    /// The method dispatches to the appropriate intersection routine based on the collider's shape type.
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
    /// Computes the intersection points between this polyline and a <see cref="Ray"/>.
    /// </summary>
    /// <param name="ray">The <see cref="Ray"/> to test for intersection.</param>
    /// <returns>
    /// A <see cref="IntersectionPoints"/> object containing intersection points,
    /// or <c>null</c> if there are no intersections.
    /// </returns>
    /// <remarks>
    /// Iterates through each segment of the polyline and checks for intersection with the ray.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Ray ray)
    {
        if (Count < 2) return null;

        IntersectionPoints? points = null;
        for (var i = 0; i < Count - 1; i++)
        {
            var result = Segment.IntersectSegmentRay(this[i], this[i + 1], ray.Point, ray.Direction, ray.Normal);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
        }

        return points;
    }

    /// <summary>
    /// Computes the intersection points between this polyline and a <see cref="Line"/>.
    /// </summary>
    /// <param name="l">The <see cref="Line"/> to test for intersection.</param>
    /// <returns>
    /// A <see cref="IntersectionPoints"/> object containing intersection points,
    /// or <c>null</c> if there are no intersections.
    /// </returns>
    /// <remarks>
    /// Iterates through each segment of the polyline and checks for intersection with the line.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Line l)
    {
        if (Count < 2) return null;

        IntersectionPoints? points = null;
        for (var i = 0; i < Count - 1; i++)
        {
            var result = Segment.IntersectSegmentLine(this[i], this[i + 1], l.Point, l.Direction, l.Normal);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
        }

        return points;
    }

    /// <summary>
    /// Computes the intersection points between this polyline and a <see cref="Segment"/>.
    /// </summary>
    /// <param name="s">The <see cref="Segment"/> to test for intersection.</param>
    /// <returns>A <see cref="IntersectionPoints"/> object containing intersection points,
    /// or <c>null</c> if there are no intersections.</returns>
    /// <remarks>
    /// Iterates through each segment of the polyline and checks for intersection with the segment.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Segment s)
    {
        if (Count < 2) return null;

        IntersectionPoints? points = null;
        for (var i = 0; i < Count - 1; i++)
        {
            var result = Segment.IntersectSegmentSegment(this[i], this[i + 1], s.Start, s.End);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
        }

        return points;
    }

    /// <summary>
    /// Computes the intersection points between this polyline and a <see cref="Circle"/>.
    /// </summary>
    /// <param name="c">The <see cref="Circle"/> to test for intersection.</param>
    /// <returns>A <see cref="IntersectionPoints"/> object containing intersection points,
    /// or <c>null</c> if there are no intersections.</returns>
    /// <remarks>
    /// Iterates through each segment of the polyline and checks for intersection with the circle.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Circle c)
    {
        if (Count < 2) return null;

        IntersectionPoints? points = null;

        for (var i = 0; i < Count - 1; i++)
        {
            var result = Segment.IntersectSegmentCircle(this[i], this[i + 1], c.Center, c.Radius);
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
    /// Computes the intersection points between this polyline and a <see cref="Triangle"/>.
    /// </summary>
    /// <param name="t">The <see cref="Triangle"/> to test for intersection.</param>
    /// <returns>A <see cref="IntersectionPoints"/> object containing intersection points,
    /// or <c>null</c> if there are no intersections.</returns>
    /// <remarks>
    /// Iterates through each segment of the polyline and each edge of the triangle, checking for intersections.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Triangle t)
    {
        if (Count < 2) return null;

        IntersectionPoints? points = null;
        for (var i = 0; i < Count - 1; i++)
        {
            var p1 = this[i];
            var p2 = this[i + 1];
            var result = Segment.IntersectSegmentSegment(p1, p2, t.A, t.B);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(p1, p2, t.B, t.C);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(p1, p2, t.C, t.A);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
        }

        return points;
    }

    /// <summary>
    /// Computes the intersection points between this polyline and a <see cref="Rect"/>.
    /// </summary>
    /// <param name="r">The <see cref="Rect"/> to test for intersection.</param>
    /// <returns>A <see cref="IntersectionPoints"/> object containing intersection points,
    /// or <c>null</c> if there are no intersections.</returns>
    /// <remarks>
    /// Iterates through each segment of the polyline and each edge of the rectangle, checking for intersections.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Rect r)
    {
        if (Count < 2) return null;

        IntersectionPoints? points = null;
        var a = r.TopLeft;
        var b = r.BottomLeft;
        var c = r.BottomRight;
        var d = r.TopRight;
        for (var i = 0; i < Count - 1; i++)
        {
            var p1 = this[i];
            var p2 = this[i + 1];
            var result = Segment.IntersectSegmentSegment(p1, p2, a, b);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(p1, p2, b, c);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(p1, p2, c, d);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(p1, p2, d, a);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
        }

        return points;
    }

    /// <summary>
    /// Computes the intersection points between this polyline and a <see cref="Quad"/>.
    /// </summary>
    /// <param name="q">The <see cref="Quad"/> to test for intersection.</param>
    /// <returns>A <see cref="IntersectionPoints"/> object containing intersection points,
    /// or <c>null</c> if there are no intersections.</returns>
    /// <remarks>
    /// Iterates through each segment of the polyline and each edge of the quad, checking for intersections.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Quad q)
    {
        if (Count < 2) return null;

        IntersectionPoints? points = null;
        for (var i = 0; i < Count - 1; i++)
        {
            var p1 = this[i];
            var p2 = this[i + 1];
            var result = Segment.IntersectSegmentSegment(p1, p2, q.A, q.B);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(p1, p2, q.B, q.C);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(p1, p2, q.C, q.D);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(p1, p2, q.D, q.A);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
        }

        return points;
    }

    /// <summary>
    /// Computes the intersection points between this polyline and a <see cref="Polygon"/>.
    /// </summary>
    /// <param name="p">The <see cref="Polygon"/> to test for intersection.</param>
    /// <returns>A <see cref="IntersectionPoints"/> object containing intersection points,
    /// or <c>null</c> if there are no intersections.</returns>
    /// <remarks>
    /// Iterates through each segment of the polyline and each edge of the polygon, checking for intersections.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Polygon p)
    {
        if (p.Count < 3 || Count < 2) return null;
        IntersectionPoints? points = null;
        for (var i = 0; i < Count - 1; i++)
        {
            for (var j = 0; j < p.Count; j++)
            {
                var result = Segment.IntersectSegmentSegment(this[i], this[i + 1], p[j], p[(j + 1) % p.Count]);
                if (result.Valid)
                {
                    points ??= new();
                    points.Add(result);
                }
            }
        }

        return points;
    }

    /// <summary>
    /// Computes the intersection points between this polyline and another <see cref="Polyline"/>.
    /// </summary>
    /// <param name="pl">The <see cref="Polyline"/> to test for intersection.</param>
    /// <returns>A <see cref="IntersectionPoints"/> object containing intersection points,
    /// or <c>null</c> if there are no intersections.</returns>
    /// <remarks>
    /// Iterates through each segment of both polylines, checking for intersections between all segment pairs.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Polyline pl)
    {
        if (pl.Count < 2 || Count < 2) return null;
        IntersectionPoints? points = null;
        for (var i = 0; i < Count - 1; i++)
        {
            for (var j = 0; j < pl.Count - 1; j++)
            {
                var result = Segment.IntersectSegmentSegment(this[i], this[i + 1], pl[j], pl[j + 1]);
                if (result.Valid)
                {
                    points ??= new();
                    points.Add(result);
                }
            }
        }

        return points;
    }

    /// <summary>
    /// Computes the intersection points between this polyline and a <see cref="Segments"/> collection.
    /// </summary>
    /// <param name="segments">The <see cref="Segments"/> collection to test for intersection.</param>
    /// <returns>A <see cref="IntersectionPoints"/> object containing intersection points,
    /// or <c>null</c> if there are no intersections.</returns>
    /// <remarks>
    /// Iterates through each segment of the polyline and each segment in the collection, checking for intersections.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Segments segments)
    {
        if (Count < 2 || segments.Count <= 0) return null;
        IntersectionPoints? points = null;
        for (var i = 0; i < Count - 1; i++)
        {
            foreach (var seg in segments)
            {
                var result = Segment.IntersectSegmentSegment(this[i], this[i + 1], seg.Start, seg.End);
                if (result.Valid)
                {
                    points ??= new();
                    points.Add(result);
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
    


}