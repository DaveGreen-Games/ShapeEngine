using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;

namespace ShapeEngine.Geometry.TriangleDef;

public readonly partial struct Triangle
{
    /// <summary>
    /// Tests whether this triangle overlaps with a line segment defined by two points.
    /// </summary>
    /// <param name="segmentStart">The start point of the line segment.</param>
    /// <param name="segmentEnd">The end point of the line segment.</param>
    /// <returns>True if the triangle overlaps with the line segment; otherwise, false.</returns>
    /// <remarks>
    /// This method checks for any intersection or containment between the triangle and the line segment.
    /// Overlap includes cases where the segment intersects triangle edges or is completely contained within the triangle.
    /// </remarks>
    public bool OverlapSegment(Vector2 segmentStart, Vector2 segmentEnd) => OverlapTriangleSegment(A, B, C, segmentStart, segmentEnd);
    
    /// <summary>
    /// Tests whether this triangle overlaps with an infinite line defined by a point and direction.
    /// </summary>
    /// <param name="linePoint">A point on the infinite line.</param>
    /// <param name="lineDirection">The direction vector of the line.</param>
    /// <returns>True if the triangle overlaps with the line; otherwise, false.</returns>
    /// <remarks>
    /// This method checks for any intersection between the triangle and the infinite line.
    /// Since the line is infinite, this tests whether the line passes through any part of the triangle.
    /// </remarks>
    public bool OverlapLine(Vector2 linePoint, Vector2 lineDirection) => OverlapTriangleLine(A, B, C, linePoint, lineDirection);
    
    /// <summary>
    /// Tests whether this triangle overlaps with a ray (semi-infinite line) defined by an origin point and direction.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <returns>True if the triangle overlaps with the ray; otherwise, false.</returns>
    /// <remarks>
    /// This method checks for any intersection between the triangle and the ray.
    /// The ray extends infinitely in one direction from the origin point.
    /// </remarks>
    public bool OverlapRay(Vector2 rayPoint, Vector2 rayDirection) => OverlapTriangleRay(A, B, C, rayPoint, rayDirection);
    
    /// <summary>
    /// Tests whether this triangle overlaps with a circle defined by center and radius.
    /// </summary>
    /// <param name="circleCenter">The center point of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <returns>True if the triangle overlaps with the circle; otherwise, false.</returns>
    /// <remarks>
    /// This method checks for any intersection or containment between the triangle and circle.
    /// Overlap includes cases where they intersect, one contains the other, or they share boundary points.
    /// </remarks>
    public bool OverlapCircle(Vector2 circleCenter, float circleRadius) => OverlapTriangleCircle(A, B, C, circleCenter, circleRadius);
    
    /// <summary>
    /// Tests whether this triangle overlaps with another triangle defined by three vertices.
    /// </summary>
    /// <param name="a">The first vertex of the other triangle.</param>
    /// <param name="b">The second vertex of the other triangle.</param>
    /// <param name="c">The third vertex of the other triangle.</param>
    /// <returns>True if the triangles overlap; otherwise, false.</returns>
    /// <remarks>
    /// This method performs comprehensive triangle-triangle overlap testing, checking for
    /// intersection, containment, or shared boundaries between the two triangles.
    /// </remarks>
    public bool OverlapTriangle(Vector2 a, Vector2 b, Vector2 c) => OverlapTriangleTriangle(A, B, C, a, b, c);
    
    /// <summary>
    /// Tests whether this triangle overlaps with a quadrilateral defined by four vertices.
    /// </summary>
    /// <param name="a">The first vertex of the quadrilateral.</param>
    /// <param name="b">The second vertex of the quadrilateral.</param>
    /// <param name="c">The third vertex of the quadrilateral.</param>
    /// <param name="d">The fourth vertex of the quadrilateral.</param>
    /// <returns>True if the triangle overlaps with the quadrilateral; otherwise, false.</returns>
    /// <remarks>
    /// This method checks for any intersection or containment between the triangle and quadrilateral.
    /// </remarks>
    public bool OverlapQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapTriangleQuad(A, B, C, a, b, c, d);
    
    /// <summary>
    /// Tests whether this triangle overlaps with a rectangle defined by four vertices.
    /// </summary>
    /// <param name="a">The first vertex of the rectangle.</param>
    /// <param name="b">The second vertex of the rectangle.</param>
    /// <param name="c">The third vertex of the rectangle.</param>
    /// <param name="d">The fourth vertex of the rectangle.</param>
    /// <returns>True if the triangle overlaps with the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// This method checks for any intersection or containment between the triangle and rectangle.
    /// The rectangle is treated as a general quadrilateral for overlap testing.
    /// </remarks>
    public bool OverlapRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapTriangleQuad(A, B, C, a, b, c, d);
    
    /// <summary>
    /// Tests whether this triangle overlaps with a polygon defined by a list of vertices.
    /// </summary>
    /// <param name="points">The vertices of the polygon in order.</param>
    /// <returns>True if the triangle overlaps with the polygon; otherwise, false.</returns>
    /// <remarks>
    /// This method checks for any intersection or containment between the triangle and polygon.
    /// The polygon can have any number of vertices and can be convex or concave.
    /// </remarks>
    public bool OverlapPolygon(List<Vector2> points) => OverlapTrianglePolygon(A, B, C, points);
    
    /// <summary>
    /// Tests whether this triangle overlaps with a polyline (open polygon) defined by a list of vertices.
    /// </summary>
    /// <param name="points">The vertices of the polyline in order.</param>
    /// <returns>True if the triangle overlaps with the polyline; otherwise, false.</returns>
    /// <remarks>
    /// This method checks for any intersection between the triangle and the polyline.
    /// Unlike polygons, polylines are not closed shapes and represent a series of connected line segments.
    /// </remarks>
    public bool OverlapPolyline(List<Vector2> points) => OverlapTrianglePolyline(A, B, C, points);
    
    /// <summary>
    /// Tests whether this triangle overlaps with a collection of line segments.
    /// </summary>
    /// <param name="segments">The collection of line segments to test against.</param>
    /// <returns>True if the triangle overlaps with any of the segments; otherwise, false.</returns>
    /// <remarks>
    /// This method checks for intersection between the triangle and any segment in the collection.
    /// Returns true as soon as any overlap is found for performance efficiency.
    /// </remarks>
    public bool OverlapSegments(List<Segment> segments) => OverlapTriangleSegments(A, B, C, segments);
    
    /// <summary>
    /// Tests whether this triangle overlaps with a line shape.
    /// </summary>
    /// <param name="line">The line to test for overlap with.</param>
    /// <returns>True if the triangle overlaps with the line; otherwise, false.</returns>
    /// <remarks>
    /// This method provides a convenient overload for testing overlap with Line objects.
    /// </remarks>
    public bool OverlapShape(Line line) => OverlapTriangleLine(A, B, C, line.Point, line.Direction);
    
    /// <summary>
    /// Tests whether this triangle overlaps with a ray shape.
    /// </summary>
    /// <param name="ray">The ray to test for overlap with.</param>
    /// <returns>True if the triangle overlaps with the ray; otherwise, false.</returns>
    /// <remarks>
    /// This method provides a convenient overload for testing overlap with Ray objects.
    /// </remarks>
    public bool OverlapShape(Ray ray) => OverlapTriangleRay(A, B, C, ray.Point, ray.Direction);

    /// <summary>
    /// Checks if the triangle overlaps with any collider in the given <see cref="CollisionObject"/>.
    /// </summary>
    /// <param name="collision">The collision object containing colliders to check for overlap.</param>
    /// <returns>True if any collider in the collision object overlaps the triangle; otherwise, false.</returns>
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
    /// Tests whether this triangle overlaps with a collider of any supported shape type.
    /// </summary>
    /// <param name="collider">The collider to test for overlap with.</param>
    /// <returns>True if the triangle overlaps with the collider; otherwise, false.</returns>
    /// <remarks>
    /// This method determines the collider's shape type and delegates to the appropriate
    /// shape-specific overlap method. If the collider is disabled, this method returns false.
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
    /// Tests whether this triangle overlaps with a collection of line segments.
    /// </summary>
    /// <param name="segments">The segments collection to test for overlap with.</param>
    /// <returns>True if the triangle overlaps with any segment in the collection; otherwise, false.</returns>
    /// <remarks>
    /// This method iterates through all segments and returns true as soon as any overlap is found.
    /// This provides better performance than checking all segments when only overlap detection is needed.
    /// </remarks>
    public bool OverlapShape(Segments segments)
    {
        if (segments.Count <= 0) return false;

        if (ContainsPoint(segments[0].Start)) return true;

        foreach (var seg in segments)
        {
            if (Segment.OverlapSegmentSegment(A, B, seg.Start, seg.End)) return true;
            if (Segment.OverlapSegmentSegment(B, C, seg.Start, seg.End)) return true;
            if (Segment.OverlapSegmentSegment(C, A, seg.Start, seg.End)) return true;
        }

        return false;
    }

    /// <summary>
    /// Tests whether this triangle overlaps with a line segment.
    /// </summary>
    /// <param name="s">The segment to test for overlap with.</param>
    /// <returns>True if the triangle overlaps with the segment; otherwise, false.</returns>
    /// <remarks>
    /// This method delegates to the segment's overlap testing method for consistency and performance.
    /// </remarks>
    public bool OverlapShape(Segment s) => s.OverlapShape(this);
    
    /// <summary>
    /// Tests whether this triangle overlaps with a circle.
    /// </summary>
    /// <param name="c">The circle to test for overlap with.</param>
    /// <returns>True if the triangle overlaps with the circle; otherwise, false.</returns>
    /// <remarks>
    /// This method delegates to the circle's overlap testing method for consistency and performance.
    /// </remarks>
    public bool OverlapShape(Circle c) => c.OverlapShape(this);

    /// <summary>
    /// Tests whether this triangle overlaps with another triangle.
    /// </summary>
    /// <param name="b">The other triangle to test for overlap with.</param>
    /// <returns>True if the triangles overlap; otherwise, false.</returns>
    /// <remarks>
    /// This method performs comprehensive triangle-triangle overlap testing using the Separating Axis Theorem
    /// or similar geometric algorithms to determine if the triangles intersect, contain each other, or share boundaries.
    /// </remarks>
    public bool OverlapShape(Triangle b)
    {
        if (ContainsPoint(b.A)) return true;

        if (b.ContainsPoint(A)) return true;

        if (Segment.OverlapSegmentSegment(A, B, b.A, b.B)) return true;
        if (Segment.OverlapSegmentSegment(A, B, b.B, b.C)) return true;
        if (Segment.OverlapSegmentSegment(A, B, b.C, b.A)) return true;

        if (Segment.OverlapSegmentSegment(B, C, b.A, b.B)) return true;
        if (Segment.OverlapSegmentSegment(B, C, b.B, b.C)) return true;
        if (Segment.OverlapSegmentSegment(B, C, b.C, b.A)) return true;

        if (Segment.OverlapSegmentSegment(C, A, b.A, b.B)) return true;
        if (Segment.OverlapSegmentSegment(C, A, b.B, b.C)) return true;
        return Segment.OverlapSegmentSegment(C, A, b.C, b.A);
    }

    /// <summary>
    /// Tests whether this triangle overlaps with a rectangle.
    /// </summary>
    /// <param name="r">The rectangle to test for overlap with.</param>
    /// <returns>True if the triangle overlaps with the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// This method checks for any intersection or containment between the triangle and rectangle.
    /// The rectangle is converted to its four vertices for overlap testing.
    /// </remarks>
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

        if (Segment.OverlapSegmentSegment(C, A, a, b)) return true;
        if (Segment.OverlapSegmentSegment(C, A, b, c)) return true;
        if (Segment.OverlapSegmentSegment(C, A, c, d)) return true;
        return Segment.OverlapSegmentSegment(C, A, d, a);
    }

    /// <summary>
    /// Tests whether this triangle overlaps with a quadrilateral.
    /// </summary>
    /// <param name="q">The quadrilateral to test for overlap with.</param>
    /// <returns>True if the triangle overlaps with the quadrilateral; otherwise, false.</returns>
    /// <remarks>
    /// This method checks for any intersection or containment between the triangle and quadrilateral.
    /// Uses the quadrilateral's four vertices for comprehensive overlap testing.
    /// </remarks>
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

        if (Segment.OverlapSegmentSegment(C, A, q.A, q.B)) return true;
        if (Segment.OverlapSegmentSegment(C, A, q.B, q.C)) return true;
        if (Segment.OverlapSegmentSegment(C, A, q.C, q.D)) return true;
        return Segment.OverlapSegmentSegment(C, A, q.D, q.A);
    }
    /// <summary>
    /// Tests whether this triangle overlaps with a polygon.
    /// </summary>
    /// <param name="poly">The polygon to test for overlap with.</param>
    /// <returns>True if the triangle overlaps with the polygon; otherwise, false.</returns>
    /// <remarks>
    /// This method checks for any intersection or containment between the triangle and the polygon.
    /// The polygon can have any number of vertices and can be convex or concave.
    /// </remarks>
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
            if (Segment.OverlapSegmentSegment(C, A, start, end)) return true;

            if (Polygon.ContainsPointCheck(start, end, A)) oddNodes = !oddNodes;
        }

        return oddNodes;
    }

    /// <summary>
    /// Tests whether this triangle overlaps with a polyline.
    /// </summary>
    /// <param name="pl">The polyline to test for overlap with.</param>
    /// <returns>True if the triangle overlaps with the polyline; otherwise, false.</returns>
    /// <remarks>
    /// This method checks for intersection between the triangle and any segment of the polyline.
    /// Polylines are open shapes consisting of connected line segments.
    /// </remarks>
    public bool OverlapShape(Polyline pl)
    {
        if (pl.Count < 2) return false;

        if (ContainsPoint(pl[0])) return true;

        for (var i = 0; i < pl.Count - 1; i++)
        {
            var start = pl[i];
            var end = pl[i + 1];
            if (Segment.OverlapSegmentSegment(A, B, start, end)) return true;
            if (Segment.OverlapSegmentSegment(B, C, start, end)) return true;
            if (Segment.OverlapSegmentSegment(C, A, start, end)) return true;
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