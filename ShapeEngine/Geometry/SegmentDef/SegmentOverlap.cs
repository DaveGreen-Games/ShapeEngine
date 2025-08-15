using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.SegmentDef;

public readonly partial struct Segment
{
    /// <summary>
    /// Determines whether the segment overlaps the specified point.
    /// </summary>
    /// <param name="p">The point to test.</param>
    /// <returns>True if the point lies on the segment; otherwise, false.</returns>
    public bool OverlapPoint(Vector2 p) => IsPointOnSegment(Start, End, p);
    /// <summary>
    /// Determines whether the segment overlaps another segment defined by two points.
    /// </summary>
    /// <param name="segStart">The start point of the other segment.</param>
    /// <param name="segEnd">The end point of the other segment.</param>
    /// <returns>True if the segments overlap; otherwise, false.</returns>
    public bool OverlapSegment(Vector2 segStart, Vector2 segEnd) => OverlapSegmentSegment(Start, End, segStart, segEnd);
    /// <summary>
    /// Determines whether the segment overlaps a line defined by a point and direction.
    /// </summary>
    /// <param name="linePoint">A point on the line.</param>
    /// <param name="lineDirection">The direction vector of the line.</param>
    /// <returns>True if the segment overlaps the line; otherwise, false.</returns>
    public bool OverlapLine(Vector2 linePoint, Vector2 lineDirection) => OverlapSegmentLine(Start, End, linePoint, lineDirection);
    /// <summary>
    /// Determines whether the segment overlaps a ray defined by a point and direction.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <returns>True if the segment overlaps the ray; otherwise, false.</returns>
    public bool OverlapRay(Vector2 rayPoint, Vector2 rayDirection) => OverlapSegmentRay(Start, End, rayPoint, rayDirection);
    /// <summary>
    /// Determines whether the segment overlaps a circle.
    /// </summary>
    /// <param name="circlePoint">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <returns>True if the segment overlaps the circle; otherwise, false.</returns>
    public bool OverlapCircle(Vector2 circlePoint, float circleRadius) => OverlapSegmentCircle(Start, End, circlePoint, circleRadius);
    /// <summary>
    /// Determines whether the segment overlaps a triangle defined by three points.
    /// </summary>
    /// <param name="a">First vertex of the triangle.</param>
    /// <param name="b">Second vertex of the triangle.</param>
    /// <param name="c">Third vertex of the triangle.</param>
    /// <returns>True if the segment overlaps the triangle; otherwise, false.</returns>
    public bool OverlapTriangle(Vector2 a, Vector2 b, Vector2 c) => OverlapSegmentTriangle(Start, End, a, b, c);
    /// <summary>
    /// Determines whether the segment overlaps a quadrilateral defined by four points.
    /// </summary>
    /// <param name="a">First vertex of the quad.</param>
    /// <param name="b">Second vertex of the quad.</param>
    /// <param name="c">Third vertex of the quad.</param>
    /// <param name="d">Fourth vertex of the quad.</param>
    /// <returns>True if the segment overlaps the quad; otherwise, false.</returns>
    public bool OverlapQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapSegmentQuad(Start, End, a, b, c, d);
    /// <summary>
    /// Determines whether the segment overlaps a rectangle defined by four points.
    /// </summary>
    /// <param name="a">First vertex of the rectangle.</param>
    /// <param name="b">Second vertex of the rectangle.</param>
    /// <param name="c">Third vertex of the rectangle.</param>
    /// <param name="d">Fourth vertex of the rectangle.</param>
    /// <returns>True if the segment overlaps the rectangle; otherwise, false.</returns>
    public bool OverlapRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapSegmentQuad(Start, End, a, b, c, d);
    /// <summary>
    /// Determines whether the segment overlaps a polygon defined by a list of points.
    /// </summary>
    /// <param name="points">The list of polygon vertices.</param>
    /// <returns>True if the segment overlaps the polygon; otherwise, false.</returns>
    public bool OverlapPolygon(List<Vector2> points) => OverlapSegmentPolygon(Start, End, points);
    /// <summary>
    /// Determines whether the segment overlaps a polyline defined by a list of points.
    /// </summary>
    /// <param name="points">The list of polyline vertices.</param>
    /// <returns>True if the segment overlaps the polyline; otherwise, false.</returns>
    public bool OverlapPolyline(List<Vector2> points) => OverlapSegmentPolyline(Start, End, points);
    /// <summary>
    /// Determines whether the segment overlaps any segment in a list of segments.
    /// </summary>
    /// <param name="segments">The list of segments to test against.</param>
    /// <returns>True if the segment overlaps any segment in the list; otherwise, false.</returns>
    public bool OverlapSegments(List<Segment> segments) => OverlapSegmentSegments(Start, End, segments);
  
    /// <summary>
    /// Checks if the segment overlaps with any collider in the given <see cref="CollisionObject"/>.
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
    /// Determines whether the segment overlaps a collider's shape.
    /// </summary>
    /// <param name="collider">The collider whose shape to test against. Must be enabled.</param>
    /// <returns>True if the segment overlaps the collider's shape; otherwise, false.</returns>
    /// <remarks>
    /// Dispatches to the appropriate shape-specific overlap method based on the collider's shape type.
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
    /// Determines whether the segment overlaps any segment in a <see cref="Segments"/> collection.
    /// </summary>
    /// <param name="segments">The collection of segments to test against.</param>
    /// <returns>True if the segment overlaps any segment in the collection; otherwise, false.</returns>
    public bool OverlapShape(Segments segments)
    {
        foreach (var seg in segments)
        {
            if (seg.OverlapShape(this)) return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the segment overlaps another segment.
    /// </summary>
    /// <param name="b">The other segment to test against.</param>
    /// <returns>True if the segments overlap; otherwise, false.</returns>
    public bool OverlapShape(Segment b) => OverlapSegmentSegment(Start, End, b.Start, b.End);
    /// <summary>
    /// Determines whether the segment overlaps a line.
    /// </summary>
    /// <param name="l">The line to test against.</param>
    /// <returns>True if the segment overlaps the line; otherwise, false.</returns>
    public bool OverlapShape(Line l) => OverlapSegmentLine(Start, End, l.Point, l.Direction);
    /// <summary>
    /// Determines whether the segment overlaps a ray.
    /// </summary>
    /// <param name="r">The ray to test against.</param>
    /// <returns>True if the segment overlaps the ray; otherwise, false.</returns>
    public bool OverlapShape(Ray r) => OverlapSegmentRay(Start, End, r.Point, r.Direction);
    /// <summary>
    /// Determines whether the segment overlaps a circle.
    /// </summary>
    /// <param name="c">The circle to test against.</param>
    /// <returns>True if the segment overlaps the circle; otherwise, false.</returns>
    public bool OverlapShape(Circle c) => OverlapSegmentCircle(Start, End, c.Center, c.Radius);
    /// <summary>
    /// Determines whether the segment overlaps a triangle.
    /// </summary>
    /// <param name="t">The triangle to test against.</param>
    /// <returns>True if the segment overlaps the triangle; otherwise, false.</returns>
    /// <remarks>
    /// Checks if the segment is inside the triangle or overlaps any of its edges.
    /// </remarks>
    public bool OverlapShape(Triangle t)
    {
        //we only need to check if 1 point is inside incase the entire segment is inside the shape
        if (t.ContainsPoint(Start)) return true;
        // if (t.ContainsPoint(End)) return true;

        if (OverlapSegmentSegment(Start, End, t.A, t.B)) return true;
        if (OverlapSegmentSegment(Start, End, t.B, t.C)) return true;
        return OverlapSegmentSegment(Start, End, t.C, t.A);
    }

    /// <summary>
    /// Determines whether the segment overlaps a quadrilateral.
    /// </summary>
    /// <param name="q">The quadrilateral to test against.</param>
    /// <returns>True if the segment overlaps the quad; otherwise, false.</returns>
    /// <remarks>
    /// Checks if the segment is inside the quad or overlaps any of its edges.
    /// </remarks>
    public bool OverlapShape(Quad q)
    {
        //we only need to check if 1 point is inside incase the entire segment is inside the shape
        if (q.ContainsPoint(Start)) return true;
        // if (q.ContainsPoint(End)) return true;

        if (OverlapSegmentSegment(Start, End, q.A, q.B)) return true;
        if (OverlapSegmentSegment(Start, End, q.B, q.C)) return true;
        if (OverlapSegmentSegment(Start, End, q.C, q.D)) return true;
        return OverlapSegmentSegment(Start, End, q.D, q.A);
    }

    /// <summary>
    /// Determines whether the segment overlaps a rectangle.
    /// </summary>
    /// <param name="r">The rectangle to test against.</param>
    /// <returns>True if the segment overlaps the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// Checks for overlap using axis-aligned bounding box and segment intersection tests.
    /// </remarks>
    public bool OverlapShape(Rect r)
    {
        if (!r.OverlapRectLine(Start, Displacement)) return false;
        var rectRange = new ValueRange
        (
            r.X,
            r.X + r.Width
        );
        var segmentRange = new ValueRange
        (
            Start.X,
            End.X
        );

        if (!rectRange.OverlapValueRange(segmentRange)) return false;

        rectRange = new(r.Y, r.Y + r.Height);
        // rectRange.Min = r.Y;
        // rectRange.Max = r.Y + r.Height;
        // rectRange.Sort();

        segmentRange = new(Start.Y, End.Y);
        // segmentRange.Min = Start.Y;
        // segmentRange.Max = End.Y;
        // segmentRange.Sort();

        return rectRange.OverlapValueRange(segmentRange);
    }

    /// <summary>
    /// Determines whether the segment overlaps a polygon.
    /// </summary>
    /// <param name="poly">The polygon to test against.</param>
    /// <returns>True if the segment overlaps the polygon; otherwise, false.</returns>
    /// <remarks>
    /// Checks for overlap with polygon edges and whether the segment start is inside the polygon.
    /// </remarks>
    public bool OverlapShape(Polygon poly)
    {
        if (poly.Count < 3) return false;
        //we only need to check if 1 point is inside incase the entire segment is inside the shape
        // if (poly.ContainsPoint(Start)) return true;
        var oddNodes = false;
        for (var i = 0; i < poly.Count; i++)
        {
            var a = poly[i];
            var b = poly[(i + 1) % poly.Count];
            if (OverlapSegmentSegment(Start, End, a, b)) return true;
            if (Polygon.ContainsPointCheck(a, b, Start)) oddNodes = !oddNodes;
        }

        return oddNodes;
    }

    /// <summary>
    /// Determines whether the segment overlaps a polyline.
    /// </summary>
    /// <param name="pl">The polyline to test against.</param>
    /// <returns>True if the segment overlaps the polyline; otherwise, false.</returns>
    /// <remarks>
    /// Checks for overlap with each polyline segment.
    /// </remarks>
    public bool OverlapShape(Polyline pl)
    {
        if (pl.Count <= 1) return false;
        if (pl.Count == 2) return OverlapSegmentSegment(Start, End, pl[0], pl[1]);

        for (var i = 0; i < pl.Count - 1; i++)
        {
            var a = pl[i];
            var b = pl[i + 1];
            if (OverlapSegmentSegment(Start, End, a, b)) return true;
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