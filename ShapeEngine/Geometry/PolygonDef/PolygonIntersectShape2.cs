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
    /// Computes intersection points between a polygon and a ray, adding results to the provided collection.
    /// </summary>
    /// <param name="polygon">The polygon as a list of points.</param>
    /// <param name="rayPoint">The origin of the ray.</param>
    /// <param name="rayDirection">The direction of the ray.</param>
    /// <param name="result">The collection to which intersection points are added.</param>
    /// <returns>The number of new intersection points added.</returns>
    public static int IntersectPolygonRay(List<Vector2> polygon, Vector2 rayPoint, Vector2 rayDirection, ref IntersectionPoints result)
    {
        if (polygon.Count < 3) return 0;
        int count = result.Count;
        for (var i = 0; i < polygon.Count; i++)
        {
            var point = Segment.IntersectSegmentRay(polygon[i], polygon[(i + 1) % polygon.Count], rayPoint, rayDirection);
            if (point.Valid)
            {
                result.Add(point);
            }
        }

        return result.Count - count;
    }
    /// <summary>
    /// Computes intersection points between this polygon and a collider, adding results to the provided collection.
    /// </summary>
    /// <param name="collider">The collider to test against.</param>
    /// <param name="points">The collection to which intersection points are added.</param>
    /// <param name="returnAfterFirstValid">If true, returns after the first valid intersection is found.</param>
    /// <returns>The number of new intersection points added.</returns>
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
    /// Computes intersection points between this polygon and a ray, adding results to the provided collection.
    /// </summary>
    /// <param name="r">The ray to test against.</param>
    /// <param name="points">The collection to which intersection points are added.</param>
    /// <param name="returnAfterFirstValid">If true, returns after the first valid intersection is found.</param>
    /// <returns>The number of new intersection points added.</returns>
    public int IntersectShape(Ray r, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 3) return 0;
        var count = 0;
        for (var i = 0; i < Count; i++)
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
    /// Computes intersection points between this polygon and a line, adding results to the provided collection.
    /// </summary>
    /// <param name="l">The line to test against.</param>
    /// <param name="points">The collection to which intersection points are added.</param>
    /// <param name="returnAfterFirstValid">If true, returns after the first valid intersection is found.</param>
    /// <returns>The number of new intersection points added.</returns>
    public int IntersectShape(Line l, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 3) return 0;
        var count = 0;
        for (var i = 0; i < Count; i++)
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
    /// Computes intersection points between this polygon and a segment, adding results to the provided collection.
    /// </summary>
    /// <param name="s">The segment to test against.</param>
    /// <param name="points">The collection to which intersection points are added.</param>
    /// <param name="returnAfterFirstValid">If true, returns after the first valid intersection is found.</param>
    /// <returns>The number of new intersection points added.</returns>
    public int IntersectShape(Segment s, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 3) return 0;
        var count = 0;
        for (var i = 0; i < Count; i++)
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
    /// Computes intersection points between this polygon and a circle, adding results to the provided collection.
    /// </summary>
    /// <param name="c">The circle to test against.</param>
    /// <param name="points">The collection to which intersection points are added.</param>
    /// <param name="returnAfterFirstValid">If true, returns after the first valid intersection is found.</param>
    /// <returns>The number of new intersection points added.</returns>
    public int IntersectShape(Circle c, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 3) return 0;

        var count = 0;

        for (var i = 0; i < Count; i++)
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
    /// Computes intersection points between this polygon and a triangle, adding results to the provided collection.
    /// </summary>
    /// <param name="t">The triangle to test against.</param>
    /// <param name="points">The collection to which intersection points are added.</param>
    /// <param name="returnAfterFirstValid">If true, returns after the first valid intersection is found.</param>
    /// <returns>The number of new intersection points added.</returns>
    public int IntersectShape(Triangle t, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 3) return 0;

        var count = 0;
        for (var i = 0; i < Count; i++)
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
    /// Computes intersection points between this polygon and a quad, adding results to the provided collection.
    /// </summary>
    /// <param name="q">The quad to test against.</param>
    /// <param name="points">The collection to which intersection points are added.</param>
    /// <param name="returnAfterFirstValid">If true, returns after the first valid intersection is found.</param>
    /// <returns>The number of new intersection points added.</returns>
    public int IntersectShape(Quad q, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 3) return 0;

        var count = 0;
        for (var i = 0; i < Count; i++)
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
    /// Computes intersection points between this polygon and a rectangle, adding results to the provided collection.
    /// </summary>
    /// <param name="r">The rectangle to test against.</param>
    /// <param name="points">The collection to which intersection points are added.</param>
    /// <param name="returnAfterFirstValid">If true, returns after the first valid intersection is found.</param>
    /// <returns>The number of new intersection points added.</returns>
    public int IntersectShape(Rect r, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 3) return 0;

        var count = 0;

        var a = r.TopLeft;
        var b = r.BottomLeft;
        var c = r.BottomRight;
        var d = r.TopRight;

        for (var i = 0; i < Count; i++)
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
    /// Computes intersection points between this polygon and another polygon, adding results to the provided collection.
    /// </summary>
    /// <param name="p">The other polygon to test against.</param>
    /// <param name="points">The collection to which intersection points are added.</param>
    /// <param name="returnAfterFirstValid">If true, returns after the first valid intersection is found.</param>
    /// <returns>The number of new intersection points added.</returns>
    public int IntersectShape(Polygon p, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 3 || p.Count < 3) return 0;
        var count = 0;
        for (var i = 0; i < Count; i++)
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
    /// Computes intersection points between this polygon and a polyline, adding results to the provided collection.
    /// </summary>
    /// <param name="pl">The polyline to test against.</param>
    /// <param name="points">The collection to which intersection points are added.</param>
    /// <param name="returnAfterFirstValid">If true, returns after the first valid intersection is found.</param>
    /// <returns>The number of new intersection points added.</returns>
    public int IntersectShape(Polyline pl, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 3 || pl.Count < 2) return 0;
        var count = 0;
        for (var i = 0; i < Count; i++)
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
    /// Computes intersection points between this polygon and a set of segments, adding results to the provided collection.
    /// </summary>
    /// <param name="shape">The segments to test against.</param>
    /// <param name="points">The collection to which intersection points are added.</param>
    /// <param name="returnAfterFirstValid">If true, returns after the first valid intersection is found.</param>
    /// <returns>The number of new intersection points added.</returns>
    /// <remarks>
    /// Each segment in the set is tested against all edges of the polygon. All valid intersection points are collected and returned unless <paramref name="returnAfterFirstValid"/> is true.
    /// </remarks>
    public int IntersectShape(Segments shape, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 3 || shape.Count <= 0) return 0;
        var count = 0;
        for (var i = 0; i < Count; i++)
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