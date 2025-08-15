using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.PolygonDef;


public partial class Polygon
{
    /// <summary>
    /// Computes intersection points between a polygon and a ray.
    /// </summary>
    /// <param name="polygon">The polygon as a list of points.</param>
    /// <param name="rayPoint">The origin of the ray.</param>
    /// <param name="rayDirection">The direction of the ray.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection of intersection points, or null if none.</returns>
    public static IntersectionPoints? IntersectPolygonRay(List<Vector2> polygon, Vector2 rayPoint, Vector2 rayDirection)
    {
        if (polygon.Count < 3) return null;
        IntersectionPoints? points = null;
        for (var i = 0; i < polygon.Count; i++)
        {
            var result = Segment.IntersectSegmentRay(polygon[i], polygon[(i + 1) % polygon.Count], rayPoint, rayDirection);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
        }
        return points;
    }
    
    /// <summary>
    /// Computes intersection points between this polygon and all colliders in the specified <see cref="CollisionObject"/>.
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
    /// Computes intersection points between this polygon and a collider.
    /// </summary>
    /// <param name="collider">The collider to test against.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection of intersection points, or null if none.</returns>
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
    /// Computes intersection points between this polygon and a ray.
    /// </summary>
    /// <param name="r">The ray to test against.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection of intersection points, or null if none.</returns>
    public IntersectionPoints? IntersectShape(Ray r)
    {
        if (Count < 3) return null;
        IntersectionPoints? points = null;
        for (var i = 0; i < Count; i++)
        {
            var result = Segment.IntersectSegmentRay(this[i], this[(i + 1) % Count], r.Point, r.Direction, r.Normal);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
        }

        return points;
    }
    /// <summary>
    /// Computes intersection points between this polygon and a line.
    /// </summary>
    /// <param name="l">The line to test against.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection of intersection points, or null if none.</returns>
    public IntersectionPoints? IntersectShape(Line l)
    {
        if (Count < 3) return null;
        IntersectionPoints? points = null;
        for (var i = 0; i < Count; i++)
        {
            var result = Segment.IntersectSegmentLine(this[i], this[(i + 1) % Count], l.Point, l.Direction, l.Normal);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
        }

        return points;
    }
    /// <summary>
    /// Computes intersection points between this polygon and a segment.
    /// </summary>
    /// <param name="s">The segment to test against.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection of intersection points, or null if none.</returns>
    public IntersectionPoints? IntersectShape(Segment s)
    {
        if (Count < 3) return null;
        IntersectionPoints? points = null;
        for (var i = 0; i < Count; i++)
        {
            var result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], s.Start, s.End);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
        }

        return points;
    }
    /// <summary>
    /// Computes intersection points between this polygon and a circle.
    /// </summary>
    /// <param name="c">The circle to test against.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection of intersection points, or null if none.</returns>
    public IntersectionPoints? IntersectShape(Circle c)
    {
        if (Count < 3) return null;

        IntersectionPoints? points = null;

        for (var i = 0; i < Count; i++)
        {
            var result = Segment.IntersectSegmentCircle(this[i], this[(i + 1) % Count], c.Center, c.Radius);
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
    /// Computes intersection points between this polygon and a triangle.
    /// </summary>
    /// <param name="t">The triangle to test against.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection of intersection points, or null if none.</returns>
    public IntersectionPoints? IntersectShape(Triangle t)
    {
        if (Count < 3) return null;

        IntersectionPoints? points = null;
        for (var i = 0; i < Count; i++)
        {
            var result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], t.A, t.B);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], t.B, t.C);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], t.C, t.A);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
        }

        return points;
    }
    /// <summary>
    /// Computes intersection points between this polygon and a rectangle.
    /// </summary>
    /// <param name="r">The rectangle to test against.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection of intersection points, or null if none.</returns>
    public IntersectionPoints? IntersectShape(Rect r)
    {
        if (Count < 3) return null;

        IntersectionPoints? points = null;

        var a = r.TopLeft;
        var b = r.BottomLeft;
        var c = r.BottomRight;
        var d = r.TopRight;

        for (var i = 0; i < Count; i++)
        {
            var result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], a, b);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], b, c);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], c, d);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], d, a);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
        }

        return points;
    }
    /// <summary>
    /// Computes intersection points between this polygon and a quad.
    /// </summary>
    /// <param name="q">The quad to test against.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection of intersection points, or null if none.</returns>
    public IntersectionPoints? IntersectShape(Quad q)
    {
        if (Count < 3) return null;

        IntersectionPoints? points = null;
        for (var i = 0; i < Count; i++)
        {
            var result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], q.A, q.B);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], q.B, q.C);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], q.C, q.D);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }

            result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], q.D, q.A);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
        }

        return points;
    }
    /// <summary>
    /// Computes intersection points between this polygon and another polygon.
    /// </summary>
    /// <param name="p">The other polygon to test against.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection of intersection points, or null if none.</returns>
    public IntersectionPoints? IntersectShape(Polygon p)
    {
        if (Count < 3 || p.Count < 3) return null;
        IntersectionPoints? points = null;
        for (var i = 0; i < Count; i++)
        {
            for (var j = 0; j < p.Count; j++)
            {
                var result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], p[j], p[(j + 1) % p.Count]);
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
    /// Computes intersection points between this polygon and a polyline.
    /// </summary>
    /// <param name="pl">The polyline to test against.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection of intersection points, or null if none.</returns>
    public IntersectionPoints? IntersectShape(Polyline pl)
    {
        if (Count < 3 || pl.Count < 2) return null;
        IntersectionPoints? points = null;
        for (var i = 0; i < Count; i++)
        {
            for (var j = 0; j < pl.Count - 1; j++)
            {
                var result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], pl[j], pl[(j + 1) % pl.Count]);
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
    /// Computes intersection points between this polygon and a set of segments.
    /// </summary>
    /// <param name="segments">The segments to test against.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection of intersection points, or null if none.</returns>
    /// <remarks>
    /// Each segment in the set is tested against all edges of the polygon. All valid intersection points are collected and returned.
    /// </remarks>
    public IntersectionPoints? IntersectShape(Segments segments)
    {
        if (Count < 3 || segments.Count <= 0) return null;
        IntersectionPoints? points = null;
        for (var i = 0; i < Count; i++)
        {
            foreach (var seg in segments)
            {
                var result = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], seg.Start, seg.End);
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