using System.Numerics;
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
    /// Checks if the circle overlaps with any collider in the given <see cref="CollisionObject"/>.
    /// </summary>
    /// <param name="collision">The collision object containing colliders to check for overlap.</param>
    /// <returns>True if any collider in the collision object overlaps the circle; otherwise, false.</returns>
    public bool Overlap(CollisionObject collision)
    {
        if (!collision.HasColliders) return false;
        foreach (var collider in collision.Colliders)
        {
            if(Overlap(collider)) return true;
        }

        return false;
    }
    /// <summary>
    /// Determines whether this circle overlaps with the specified collider.
    /// </summary>
    /// <param name="collider">The collider to test for overlap with this circle.
    /// The collider can represent various shapes such as circles, segments, rays, lines, triangles, rectangles, quads, polygons, or polylines.</param>
    /// <returns><c>true</c> if this circle overlaps with the specified collider; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// The overlap test is performed based on the shape type of the collider.
    /// If the collider is not enabled, the function returns <c>false</c> immediately.
    /// The appropriate shape-specific overlap method is called depending on the collider's shape type.
    /// </remarks>
    public bool Overlap(Collider collider)
    {
        if (!collider.Enabled) return false;

        switch (collider.GetShapeType())
        {
            case ShapeType.Circle:
                var c = collider.GetCircleShape();
                return OverlapShape(c);
            case ShapeType.Segment:
                var s = collider.GetSegmentShape();
                return OverlapShape(s);
            case ShapeType.Ray:
                var rayShape = collider.GetRayShape();
                return OverlapShape(rayShape);
            case ShapeType.Line:
                var l = collider.GetLineShape();
                return OverlapShape(l);
            case ShapeType.Triangle:
                var t = collider.GetTriangleShape();
                return OverlapShape(t);
            case ShapeType.Rect:
                var r = collider.GetRectShape();
                return OverlapShape(r);
            case ShapeType.Quad:
                var q = collider.GetQuadShape();
                return OverlapShape(q);
            case ShapeType.Poly:
                var p = collider.GetPolygonShape();
                return OverlapShape(p);
            case ShapeType.PolyLine:
                var pl = collider.GetPolylineShape();
                return OverlapShape(pl);
        }

        return false;
    }

    /// <summary>
    /// Determines whether this circle overlaps with any of the provided segments.
    /// </summary>
    /// <param name="segments">A collection of segments to test for overlap with this circle. Each segment is defined by its start and end points.</param>
    /// <returns><c>true</c> if this circle overlaps with any of the segments; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// The function iterates through each segment in the collection and checks for overlap with the circle using the OverlapCircleSegment method.
    /// The check returns <c>true</c> as soon as an overlap is found; otherwise, it returns <c>false</c> after all segments are checked.
    /// </remarks>
    public bool OverlapShape(Segments segments)
    {
        foreach (var seg in segments)
        {
            if (OverlapCircleSegment(Center, Radius, seg.Start, seg.End)) return true;
            // if (seg.OverlapShape(this)) return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether this circle overlaps with a segment.
    /// </summary>
    /// <param name="s">The segment to check for overlap.</param>
    /// <returns><c>true</c> if the circle overlaps with the segment; otherwise, <c>false</c>.</returns>
    public bool OverlapShape(Segment s) => OverlapCircleSegment(Center, Radius, s.Start, s.End);

    /// <summary>
    /// Determines whether this circle overlaps with a line.
    /// </summary>
    /// <param name="l">The line to check for overlap.</param>
    /// <returns><c>true</c> if the circle overlaps with the line; otherwise, <c>false</c>.</returns>
    public bool OverlapShape(Line l) => Line.OverlapLineCircle(l.Point, l.Direction, Center, Radius);

    /// <summary>
    /// Determines whether this circle overlaps with a ray.
    /// </summary>
    /// <param name="r">The ray to check for overlap.</param>
    /// <returns><c>true</c> if the circle overlaps with the ray; otherwise, <c>false</c>.</returns>
    public bool OverlapShape(Ray r) => Ray.OverlapRayCircle(r.Point, r.Direction, Center, Radius);

    /// <summary>
    /// Determines whether this circle overlaps with another circle.
    /// </summary>
    /// <param name="b">The other circle to check for overlap.</param>
    /// <returns><c>true</c> if the circles overlap; otherwise, <c>false</c>.</returns>
    public bool OverlapShape(Circle b) => OverlapCircleCircle(Center, Radius, b.Center, b.Radius);

    /// <summary>
    /// Determines whether this circle overlaps with a triangle.
    /// </summary>
    /// <param name="t">The triangle to check for overlap.</param>
    /// <returns><c>true</c> if the circle overlaps with the triangle; otherwise, <c>false</c>.</returns>
    public bool OverlapShape(Triangle t)
    {
        if (ContainsPoint(t.A)) return true;
        if (t.ContainsPoint(Center)) return true;

        if (Segment.OverlapSegmentCircle(t.A, t.B, Center, Radius)) return true;
        if (Segment.OverlapSegmentCircle(t.B, t.C, Center, Radius)) return true;
        return Segment.OverlapSegmentCircle(t.C, t.A, Center, Radius);
    }

    /// <summary>
    /// Determines whether this circle overlaps with a quadrilateral.
    /// </summary>
    /// <param name="q">The quad to check for overlap.</param>
    /// <returns><c>true</c> if the circle overlaps with the quad; otherwise, <c>false</c>.</returns>
    public bool OverlapShape(Quad q)
    {
        if (ContainsPoint(q.A)) return true;
        if (q.ContainsPoint(Center)) return true;

        if (Segment.OverlapSegmentCircle(q.A, q.B, Center, Radius)) return true;
        if (Segment.OverlapSegmentCircle(q.B, q.C, Center, Radius)) return true;
        if (Segment.OverlapSegmentCircle(q.C, q.D, Center, Radius)) return true;
        return Segment.OverlapSegmentCircle(q.D, q.A, Center, Radius);
    }

    /// <summary>
    /// Determines whether this circle overlaps with a rectangle.
    /// </summary>
    /// <param name="r">The rectangle to check for overlap.</param>
    /// <returns><c>true</c> if the circle overlaps with the rectangle; otherwise, <c>false</c>.</returns>
    public bool OverlapShape(Rect r)
    {
        if (Radius <= 0.0f) return r.ContainsPoint(Center);
        return ContainsPoint(r.ClampOnRect(Center));
    }

    /// <summary>
    /// Determines whether this circle overlaps with a polygon.
    /// </summary>
    /// <param name="poly">The polygon to check for overlap.</param>
    /// <returns><c>true</c> if the circle overlaps with the polygon; otherwise, <c>false</c>.</returns>
    public bool OverlapShape(Polygon poly)
    {
        if (poly.Count < 3) return false;
        if (ContainsPoint(poly[0])) return true;

        var oddNodes = false;
        for (var i = 0; i < poly.Count; i++)
        {
            var start = poly[i];
            var end = poly[(i + 1) % poly.Count];
            if (Circle.OverlapCircleSegment(Center, Radius, start, end)) return true;
            if (Polygon.ContainsPointCheck(start, end, Center)) oddNodes = !oddNodes;
        }

        return oddNodes;
    }

    /// <summary>
    /// Determines whether this circle overlaps with a polyline.
    /// </summary>
    /// <param name="pl">The polyline to check for overlap.</param>
    /// <returns><c>true</c> if the circle overlaps with the polyline; otherwise, <c>false</c>.</returns>
    public bool OverlapShape(Polyline pl)
    {
        if (pl.Count <= 0) return false;
        if (pl.Count == 1) return ContainsPoint(pl[0]);

        if (ContainsPoint(pl[0])) return true;

        for (var i = 0; i < pl.Count - 1; i++)
        {
            var start = pl[i];
            var end = pl[(i + 1) % pl.Count];
            if (OverlapCircleSegment(Center, Radius, start, end)) return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether this shape overlaps with the specified <see cref="IShape"/>.
    /// </summary>
    /// <param name="shape">The shape to test for overlap with this shape.
    /// The shape can be any supported type such as circle, segment, ray, line, triangle, rectangle, quad, polygon, or polyline.</param>
    /// <returns><c>true</c> if this shape overlaps with the specified shape; otherwise, <c>false</c>.</returns>
    public bool OverlapShape(IShape shape)
    {
        return shape.GetShapeType() switch
        {
            ShapeType.Circle => OverlapShape(shape.GetCircleShape()),
            ShapeType.Segment => OverlapShape(shape.GetSegmentShape()),
            ShapeType.Ray => OverlapShape(shape.GetRayShape()),
            ShapeType.Line => OverlapShape(shape.GetLineShape()),
            ShapeType.Triangle => OverlapShape(shape.GetTriangleShape()),
            ShapeType.Rect => OverlapShape(shape.GetRectShape()),
            ShapeType.Quad => OverlapShape(shape.GetQuadShape()),
            ShapeType.Poly => OverlapShape(shape.GetPolygonShape()),
            ShapeType.PolyLine => OverlapShape(shape.GetPolylineShape()),
            _ => false
        };
    }
    
    
    /// <summary>
    /// Determines whether this circle overlaps with a segment defined by two points.
    /// </summary>
    /// <param name="start">The start point of the segment.</param>
    /// <param name="end">The end point of the segment.</param>
    /// <returns><c>true</c> if the circle overlaps with the segment; otherwise, <c>false</c>.</returns>
    public bool OverlapSegment(Vector2 start, Vector2 end) => OverlapCircleSegment(Center, Radius, start, end);

    /// <summary>
    /// Determines whether this circle overlaps with a line defined by a point and direction.
    /// </summary>
    /// <param name="linePos">A point on the line.</param>
    /// <param name="lineDir">The direction of the line.</param>
    /// <returns><c>true</c> if the circle overlaps with the line; otherwise, <c>false</c>.</returns>
    public bool OverlapLine(Vector2 linePos, Vector2 lineDir) => OverlapCircleLine(Center, Radius, linePos, lineDir);

    /// <summary>
    /// Determines whether this circle overlaps with a ray defined by a point and direction.
    /// </summary>
    /// <param name="rayPos">The origin point of the ray.</param>
    /// <param name="rayDir">The direction of the ray.</param>
    /// <returns><c>true</c> if the circle overlaps with the ray; otherwise, <c>false</c>.</returns>
    public bool OverlapRay(Vector2 rayPos, Vector2 rayDir) => OverlapCircleRay(Center, Radius, rayPos, rayDir);

    /// <summary>
    /// Determines whether this circle overlaps with another circle defined by center and radius.
    /// </summary>
    /// <param name="center">The center of the other circle.</param>
    /// <param name="radius">The radius of the other circle.</param>
    /// <returns><c>true</c> if the circles overlap; otherwise, <c>false</c>.</returns>
    public bool OverlapCircle(Vector2 center, float radius) => OverlapCircleCircle(Center, Radius, center, radius);

    /// <summary>
    /// Determines whether this circle overlaps with a triangle defined by three points.
    /// </summary>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <returns><c>true</c> if the circle overlaps with the triangle; otherwise, <c>false</c>.</returns>
    public bool OverlapTriangle(Vector2 a, Vector2 b, Vector2 c) => OverlapCircleTriangle(Center, Radius, a, b, c);

    /// <summary>
    /// Determines whether this circle overlaps with a quadrilateral defined by four points.
    /// </summary>
    /// <param name="a">The first vertex of the quad.</param>
    /// <param name="b">The second vertex of the quad.</param>
    /// <param name="c">The third vertex of the quad.</param>
    /// <param name="d">The fourth vertex of the quad.</param>
    /// <returns><c>true</c> if the circle overlaps with the quad; otherwise, <c>false</c>.</returns>
    public bool OverlapQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapCircleQuad(Center, Radius, a, b, c, d);

    /// <summary>
    /// Determines whether this circle overlaps with a rectangle defined by four points.
    /// </summary>
    /// <param name="a">The top-left corner of the rectangle.</param>
    /// <param name="b">The top-right corner of the rectangle.</param>
    /// <param name="c">The bottom-right corner of the rectangle.</param>
    /// <param name="d">The bottom-left corner of the rectangle.</param>
    /// <returns><c>true</c> if the circle overlaps with the rectangle; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method is an alias for <see cref="OverlapQuad"/>.
    /// </remarks>
    public bool OverlapRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapCircleQuad(Center, Radius, a, b, c, d);

    /// <summary>
    /// Determines whether this circle overlaps with a polygon defined by a list of points.
    /// </summary>
    /// <param name="points">The vertices of the polygon.</param>
    /// <returns><c>true</c> if the circle overlaps with the polygon; otherwise, <c>false</c>.</returns>
    public bool OverlapPolygon(List<Vector2> points) => OverlapCirclePolygon(Center, Radius, points);

    /// <summary>
    /// Determines whether this circle overlaps with a polyline defined by a list of points.
    /// </summary>
    /// <param name="points">The vertices of the polyline.</param>
    /// <returns><c>true</c> if the circle overlaps with the polyline; otherwise, <c>false</c>.</returns>
    public bool OverlapPolyline(List<Vector2> points) => OverlapCirclePolyline(Center, Radius, points);

    /// <summary>
    /// Determines whether this circle overlaps with a collection of segments.
    /// </summary>
    /// <param name="segments">The list of segments to check for overlap.</param>
    /// <returns><c>true</c> if the circle overlaps with any of the segments; otherwise, <c>false</c>.</returns>
    public bool OverlapSegments(List<Segment> segments) => OverlapCircleSegments(Center, Radius, segments);

}