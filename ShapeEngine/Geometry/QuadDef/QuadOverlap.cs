using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.QuadDef;

public readonly partial struct Quad
{
    /// <summary>
    /// Checks if the quad overlaps with a segment defined by two points.
    /// </summary>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <returns>True if the segment overlaps the quad; otherwise, false.</returns>
    /// <remarks>Uses the quad's current vertices for the overlap test.</remarks>
    public bool OverlapSegment(Vector2 segmentStart, Vector2 segmentEnd) => OverlapQuadSegment(A, B, C, D, segmentStart, segmentEnd);

    /// <summary>
    /// Checks if the quad overlaps with a line defined by a point and direction.
    /// </summary>
    /// <param name="linePoint">A point on the line.</param>
    /// <param name="lineDirection">The direction vector of the line.</param>
    /// <returns>True if the line overlaps the quad; otherwise, false.</returns>
    public bool OverlapLine(Vector2 linePoint, Vector2 lineDirection) => OverlapQuadLine(A, B, C, D, linePoint, lineDirection);

    /// <summary>
    /// Checks if the quad overlaps with a ray defined by a point and direction.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <returns>True if the ray overlaps the quad; otherwise, false.</returns>
    public bool OverlapRay(Vector2 rayPoint, Vector2 rayDirection) => OverlapQuadRay(A, B, C, D, rayPoint, rayDirection);

    /// <summary>
    /// Checks if the quad overlaps with a circle.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <returns>True if the circle overlaps the quad; otherwise, false.</returns>
    public bool OverlapCircle(Vector2 circleCenter, float circleRadius) => OverlapQuadCircle(A, B, C, D, circleCenter, circleRadius);

    /// <summary>
    /// Checks if the quad overlaps with a triangle defined by three points.
    /// </summary>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <returns>True if the triangle overlaps the quad; otherwise, false.</returns>
    public bool OverlapTriangle(Vector2 a, Vector2 b, Vector2 c) => OverlapQuadTriangle(A, B, C, D, a, b, c);

    /// <summary>
    /// Checks if the quad overlaps with another quad defined by four points.
    /// </summary>
    /// <param name="a">The first vertex of the other quad.</param>
    /// <param name="b">The second vertex of the other quad.</param>
    /// <param name="c">The third vertex of the other quad.</param>
    /// <param name="d">The fourth vertex of the other quad.</param>
    /// <returns>True if the quads overlap; otherwise, false.</returns>
    public bool OverlapQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapQuadQuad(A, B, C, D, a, b, c, d);

    /// <summary>
    /// Checks if the quad overlaps with a rectangle defined by four points.
    /// </summary>
    /// <param name="a">The first vertex of the rectangle.</param>
    /// <param name="b">The second vertex of the rectangle.</param>
    /// <param name="c">The third vertex of the rectangle.</param>
    /// <param name="d">The fourth vertex of the rectangle.</param>
    /// <returns>True if the rectangle overlaps the quad; otherwise, false.</returns>
    public bool OverlapRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapQuadQuad(A, B, C, D, a, b, c, d);

    /// <summary>
    /// Checks if the quad overlaps with a polygon defined by a list of points.
    /// </summary>
    /// <param name="points">The list of polygon vertices.</param>
    /// <returns>True if the polygon overlaps the quad; otherwise, false.</returns>
    public bool OverlapPolygon(List<Vector2> points) => OverlapQuadPolygon(A, B, C, D, points);

    /// <summary>
    /// Checks if the quad overlaps with a polyline defined by a list of points.
    /// </summary>
    /// <param name="points">The list of polyline points.</param>
    /// <returns>True if the polyline overlaps the quad; otherwise, false.</returns>
    public bool OverlapPolyline(List<Vector2> points) => OverlapQuadPolyline(A, B, C, D, points);

    /// <summary>
    /// Checks if the quad overlaps with a set of segments.
    /// </summary>
    /// <param name="segments">The list of segments to check.</param>
    /// <returns>True if any segment overlaps the quad; otherwise, false.</returns>
    public bool OverlapSegments(List<Segment> segments) => OverlapQuadSegments(A, B, C, D, segments);

    /// <summary>
    /// Checks if the quad overlaps with any collider in the given <see cref="CollisionObject"/>.
    /// </summary>
    /// <param name="collision">The collision object containing colliders to check for overlap.</param>
    /// <returns>True if any collider in the collision object overlaps the quad; otherwise, false.</returns>
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
    /// Checks if the quad overlaps with a collider's shape.
    /// </summary>
    /// <param name="collider">The collider whose shape to check. Has to be enabled.</param>
    /// <returns>True if the collider's shape overlaps the quad; otherwise, false.</returns>
    /// <remarks>Supports multiple shape types, including circle, segment, line,
    /// ray, triangle, rect, quad, polygon, and polyline.</remarks>
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
            case ShapeType.Line:
                var l = collider.GetLineShape();
                return OverlapShape(l);
            case ShapeType.Ray:
                var rayShape = collider.GetRayShape();
                return OverlapShape(rayShape);
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
    /// Checks if the quad overlaps with a line shape.
    /// </summary>
    /// <param name="line">The line shape to check.</param>
    /// <returns>True if the line overlaps the quad; otherwise, false.</returns>
    public bool OverlapShape(Line line) => OverlapQuadLine(A, B, C, D, line.Point, line.Direction);

    /// <summary>
    /// Checks if the quad overlaps with a ray shape.
    /// </summary>
    /// <param name="ray">The ray shape to check.</param>
    /// <returns>True if the ray overlaps the quad; otherwise, false.</returns>
    public bool OverlapShape(Ray ray) => OverlapQuadRay(A, B, C, D, ray.Point, ray.Direction);
    
    /// <summary>
    /// Checks if the quad overlaps with a set of segments.
    /// </summary>
    /// <param name="segments">The segments to check.</param>
    /// <returns>True if any segment overlaps the quad; otherwise, false.</returns>
    /// <remarks>Returns true if the first segment's start point is inside the quad or if any segment overlaps any edge of the quad.</remarks>
    public bool OverlapShape(Segments segments)
    {
        if (segments.Count <= 0) return false;
        if (ContainsPoint(segments[0].Start)) return true;

        foreach (var seg in segments)
        {
            if (Segment.OverlapSegmentSegment(A, B, seg.Start, seg.End)) return true;
            if (Segment.OverlapSegmentSegment(B, C, seg.Start, seg.End)) return true;
            if (Segment.OverlapSegmentSegment(C, D, seg.Start, seg.End)) return true;
            if (Segment.OverlapSegmentSegment(D, A, seg.Start, seg.End)) return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if the quad overlaps with a segment shape.
    /// </summary>
    /// <param name="s">The segment to check.</param>
    /// <returns>True if the segment overlaps the quad; otherwise, false.</returns>
    public bool OverlapShape(Segment s) => s.OverlapShape(this);

    /// <summary>
    /// Checks if the quad overlaps with a circle shape.
    /// </summary>
    /// <param name="c">The circle to check.</param>
    /// <returns>True if the circle overlaps the quad; otherwise, false.</returns>
    public bool OverlapShape(Circle c) => c.OverlapShape(this);

    /// <summary>
    /// Checks if the quad overlaps with a triangle shape.
    /// </summary>
    /// <param name="t">The triangle to check.</param>
    /// <returns>True if the triangle overlaps the quad; otherwise, false.</returns>
    public bool OverlapShape(Triangle t) => t.OverlapShape(this);

    /// <summary>
    /// Checks if the quad overlaps with another quad shape.
    /// </summary>
    /// <param name="q">The other quad to check.</param>
    /// <returns>True if the quads overlap; otherwise, false.</returns>
    /// <remarks>Returns true if any vertex of one quad is inside the other, or if any edges overlap.</remarks>
    public bool OverlapShape(Quad q)
    {
        if (ContainsPoint(q.A)) return true;
        if (q.ContainsPoint(A)) return true;

        if (Segment.OverlapSegmentSegment(A, B, q.A, q.B)) return true;
        if (Segment.OverlapSegmentSegment(A, B, q.B, q.C)) return true;
        if (Segment.OverlapSegmentSegment(A, B, q.C, q.D)) return true;
        if (Segment.OverlapSegmentSegment(A, B, q.D, q.A)) return true;

        if (Segment.OverlapSegmentSegment(B, C, q.A, q.B)) return true;
        if (Segment.OverlapSegmentSegment(B, C, q.B, q.C)) return true;
        if (Segment.OverlapSegmentSegment(B, C, q.C, q.D)) return true;
        if (Segment.OverlapSegmentSegment(B, C, q.D, q.A)) return true;

        if (Segment.OverlapSegmentSegment(C, D, q.A, q.B)) return true;
        if (Segment.OverlapSegmentSegment(C, D, q.B, q.C)) return true;
        if (Segment.OverlapSegmentSegment(C, D, q.C, q.D)) return true;
        if (Segment.OverlapSegmentSegment(C, D, q.D, q.A)) return true;

        if (Segment.OverlapSegmentSegment(D, A, q.A, q.B)) return true;
        if (Segment.OverlapSegmentSegment(D, A, q.B, q.C)) return true;
        if (Segment.OverlapSegmentSegment(D, A, q.C, q.D)) return true;
        return Segment.OverlapSegmentSegment(D, A, q.D, q.A);
    }

    /// <summary>
    /// Checks if the quad overlaps with a rectangle shape.
    /// </summary>
    /// <param name="r">The rectangle to check.</param>
    /// <returns>True if the rectangle overlaps the quad; otherwise, false.</returns>
    public bool OverlapShape(Rect r)
    {
        var a = r.TopLeft;
        if (ContainsPoint(a)) return true;
        if (r.ContainsPoint(A)) return true;

        var b = r.BottomLeft;
        if (Segment.OverlapSegmentSegment(A, B, a, b)) return true;
        var c = r.BottomRight;
        if (Segment.OverlapSegmentSegment(A, B, b, c)) return true;
        var d = r.TopRight;
        if (Segment.OverlapSegmentSegment(A, B, c, d)) return true;
        if (Segment.OverlapSegmentSegment(A, B, d, a)) return true;

        if (Segment.OverlapSegmentSegment(B, C, a, b)) return true;
        if (Segment.OverlapSegmentSegment(B, C, b, c)) return true;
        if (Segment.OverlapSegmentSegment(B, C, c, d)) return true;
        if (Segment.OverlapSegmentSegment(B, C, d, a)) return true;

        if (Segment.OverlapSegmentSegment(C, D, a, b)) return true;
        if (Segment.OverlapSegmentSegment(C, D, b, c)) return true;
        if (Segment.OverlapSegmentSegment(C, D, c, d)) return true;
        if (Segment.OverlapSegmentSegment(C, D, d, a)) return true;

        if (Segment.OverlapSegmentSegment(D, A, a, b)) return true;
        if (Segment.OverlapSegmentSegment(D, A, b, c)) return true;
        if (Segment.OverlapSegmentSegment(D, A, c, d)) return true;
        return Segment.OverlapSegmentSegment(D, A, d, a);
    }

    /// <summary>
    /// Checks if the quad overlaps with a polygon shape.
    /// </summary>
    /// <param name="poly">The polygon to check.</param>
    /// <returns>True if the polygon overlaps the quad; otherwise, false.</returns>
    public bool OverlapShape(Polygon poly)
    {
        if (poly.Count < 3) return false;

        if (ContainsPoint(poly[0])) return true;

        var oddNodes = false;

        for (var i = 0; i < poly.Count; i++)
        {
            var start = poly[i];
            var end = poly[(i + 1) % poly.Count];
            if (Segment.OverlapSegmentSegment(A, B, start, end)) return true;
            if (Segment.OverlapSegmentSegment(B, C, start, end)) return true;
            if (Segment.OverlapSegmentSegment(C, D, start, end)) return true;
            if (Segment.OverlapSegmentSegment(D, A, start, end)) return true;

            if (Polygon.ContainsPointCheck(start, end, A)) oddNodes = !oddNodes;
        }

        return oddNodes;
    }

    /// <summary>
    /// Checks if the quad overlaps with a polyline shape.
    /// </summary>
    /// <param name="pl">The polyline to check.</param>
    /// <returns>True if the polyline overlaps the quad; otherwise, false.</returns>
    public bool OverlapShape(Polyline pl)
    {
        if (pl.Count < 2) return false;

        if (ContainsPoint(pl[0])) return true;

        for (var i = 0; i < pl.Count - 1; i++)
        {
            var start = pl[i];
            var end = pl[(i + 1) % pl.Count];
            if (Segment.OverlapSegmentSegment(A, B, start, end)) return true;
            if (Segment.OverlapSegmentSegment(B, C, start, end)) return true;
            if (Segment.OverlapSegmentSegment(C, D, start, end)) return true;
            if (Segment.OverlapSegmentSegment(D, A, start, end)) return true;
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

}