using System.Numerics;
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
    /// Checks if the segments overlap with a segment.
    /// </summary>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <returns>True if the segments overlap with the segment, false otherwise.</returns>
    public bool OverlapSegment(Vector2 segmentStart, Vector2 segmentEnd) => OverlapSegmentsSegment(this, segmentStart, segmentEnd);
    /// <summary>
    /// Checks if the segments overlap with a line.
    /// </summary>
    /// <param name="linePoint">A point on the line.</param>
    /// <param name="lineDirection">The direction of the line.</param>
    /// <returns>True if the segments overlap with the line, false otherwise.</returns>
    public bool OverlapLine(Vector2 linePoint, Vector2 lineDirection) => OverlapSegmentsLine(this, linePoint, lineDirection);
    /// <summary>
    /// Checks if the segments overlap with a ray.
    /// </summary>
    /// <param name="rayPoint">The starting point of the ray.</param>
    /// <param name="rayDirection">The direction of the ray.</param>
    /// <returns>True if the segments overlap with the ray, false otherwise.</returns>
    public bool OverlapRay(Vector2 rayPoint, Vector2 rayDirection) => OverlapSegmentsRay(this, rayPoint, rayDirection);
    /// <summary>
    /// Checks if the segments overlap with a circle.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <returns>True if the segments overlap with the circle, false otherwise.</returns>
    public bool OverlapCircle(Vector2 circleCenter, float circleRadius) => OverlapSegmentsCircle(this, circleCenter, circleRadius);
    /// <summary>
    /// Checks if the segments overlap with a triangle.
    /// </summary>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <returns>True if the segments overlap with the triangle, false otherwise.</returns>
    public bool OverlapTriangle(Vector2 a, Vector2 b, Vector2 c) => OverlapSegmentsTriangle(this, a, b, c);
    /// <summary>
    /// Checks if the segments overlap with a quad.
    /// </summary>
    /// <param name="a">The first vertex of the quad.</param>
    /// <param name="b">The second vertex of the quad.</param>
    /// <param name="c">The third vertex of the quad.</param>
    /// <param name="d">The fourth vertex of the quad.</param>
    /// <returns>True if the segments overlap with the quad, false otherwise.</returns>
    public bool OverlapQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapSegmentsQuad(this, a, b, c, d);
    /// <summary>
    /// Checks if the segments overlap with a rectangle.
    /// </summary>
    /// <param name="a">The first vertex of the rectangle.</param>
    /// <param name="b">The second vertex of the rectangle.</param>
    /// <param name="c">The third vertex of the rectangle.</param>
    /// <param name="d">The fourth vertex of the rectangle.</param>
    /// <returns>True if the segments overlap with the rectangle, false otherwise.</returns>
    public bool OverlapRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapSegmentsQuad(this, a, b, c, d);
    /// <summary>
    /// Checks if the segments overlap with a polygon.
    /// </summary>
    /// <param name="points">The vertices of the polygon.</param>
    /// <returns>True if the segments overlap with the polygon, false otherwise.</returns>
    public bool OverlapPolygon(List<Vector2> points) => OverlapSegmentsPolygon(this, points);
    /// <summary>
    /// Checks if the segments overlap with a polyline.
    /// </summary>
    /// <param name="points">The vertices of the polyline.</param>
    /// <returns>True if the segments overlap with the polyline, false otherwise.</returns>
    public bool OverlapPolyline(List<Vector2> points) => OverlapSegmentsPolyline(this, points);
    /// <summary>
    /// Checks if the segments overlap with another set of segments.
    /// </summary>
    /// <param name="segments">The other set of segments.</param>
    /// <returns>True if the segments overlap with the other set of segments, false otherwise.</returns>
    public bool OverlapSegments(List<Segment> segments) => OverlapSegmentsSegments(this, segments);
   
    /// <summary>
    /// Checks if any of the segments overlaps with any collider in the given <see cref="CollisionObject"/>.
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
    /// Determines whether any of the segments overlaps with a collider's shape.
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
    /// Checks if the segments overlap with a line.
    /// </summary>
    /// <param name="line">The line to check for overlap with.</param>
    /// <returns>True if the segments overlap with the line, false otherwise.</returns>
    public bool OverlapShape(Line line) => OverlapSegmentsLine(this, line.Point, line.Direction);
    /// <summary>
    /// Checks if the segments overlap with a ray.
    /// </summary>
    /// <param name="ray">The ray to check for overlap with.</param>
    /// <returns>True if the segments overlap with the ray, false otherwise.</returns>
    public bool OverlapShape(Ray ray) => OverlapSegmentsRay(this, ray.Point, ray.Direction);

    /// <summary>
    /// Checks if the segments overlap with another set of segments.
    /// </summary>
    /// <param name="b">The other set of segments.</param>
    /// <returns>True if the segments overlap with the other set of segments, false otherwise.</returns>
    public bool OverlapShape(Segments b)
    {
        foreach (var seg in this)
        {
            foreach (var bSeg in b)
            {
                if (Segment.OverlapSegmentSegment(seg.Start, seg.End, bSeg.Start, bSeg.End)) return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if the segments overlap with a segment.
    /// </summary>
    /// <param name="s">The segment to check for overlap with.</param>
    /// <returns>True if the segments overlap with the segment, false otherwise.</returns>
    public bool OverlapShape(Segment s)
    {
        foreach (var seg in this)
        {
            if (Segment.OverlapSegmentSegment(seg.Start, seg.End, s.Start, s.End)) return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if the segments overlap with a circle.
    /// </summary>
    /// <param name="c">The circle to check for overlap with.</param>
    /// <returns>True if the segments overlap with the circle, false otherwise.</returns>
    public bool OverlapShape(Circle c)
    {
        foreach (var seg in this)
        {
            if (Segment.OverlapSegmentCircle(seg.Start, seg.End, c.Center, c.Radius)) return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if the segments overlap with a triangle.
    /// </summary>
    /// <param name="t">The triangle to check for overlap with.</param>
    /// <returns>True if the segments overlap with the triangle, false otherwise.</returns>
    public bool OverlapShape(Triangle t) => t.OverlapShape(this);
    /// <summary>
    /// Checks if the segments overlap with a quad.
    /// </summary>
    /// <param name="q">The quad to check for overlap with.</param>
    /// <returns>True if the segments overlap with the quad, false otherwise.</returns>
    public bool OverlapShape(Quad q) => q.OverlapShape(this);
    /// <summary>
    /// Checks if the segments overlap with a rectangle.
    /// </summary>
    /// <param name="r">The rectangle to check for overlap with.</param>
    /// <returns>True if the segments overlap with the rectangle, false otherwise.</returns>
    public bool OverlapShape(Rect r) => r.OverlapShape(this);
    /// <summary>
    /// Checks if the segments overlap with a polyline.
    /// </summary>
    /// <param name="pl">The polyline to check for overlap with.</param>
    /// <returns>True if the segments overlap with the polyline, false otherwise.</returns>
    public bool OverlapShape(Polyline pl) => pl.OverlapShape(this);
    /// <summary>
    /// Checks if the segments overlap with a polygon.
    /// </summary>
    /// <param name="p">The polygon to check for overlap with.</param>
    /// <returns>True if the segments overlap with the polygon, false otherwise.</returns>
    public bool OverlapShape(Polygon p) => p.OverlapShape(this);
    
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