using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.RayDef;

public readonly partial struct Ray
{
    
    /// <summary>
    /// Determines whether this ray overlaps the specified point.
    /// </summary>
    /// <param name="p">The point to check for overlap.</param>
    /// <returns>True if the point lies on the ray; otherwise, false.</returns>
    public bool OverlapPoint(Vector2 p) => IsPointOnRay(Point, Direction, p);
    /// <summary>
    /// Determines whether this ray overlaps the specified segment.
    /// </summary>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <returns>True if the ray overlaps the segment; otherwise, false.</returns>
    public bool OverlapSegment(Vector2 segmentStart, Vector2 segmentEnd) => OverlapRaySegment(Point, Direction, segmentStart, segmentEnd);
    /// <summary>
    /// Determines whether this ray overlaps the specified line.
    /// </summary>
    /// <param name="linePoint">A point on the line.</param>
    /// <param name="lineDirection">The direction vector of the line.</param>
    /// <returns>True if the ray overlaps the line; otherwise, false.</returns>
    public bool OverlapLine(Vector2 linePoint, Vector2 lineDirection) => OverlapRayLine(Point, Direction, linePoint, lineDirection);
    /// <summary>
    /// Determines whether this ray overlaps another ray.
    /// </summary>
    /// <param name="rayPoint">The origin point of the other ray.</param>
    /// <param name="rayDirection">The direction vector of the other ray.</param>
    /// <returns>True if the rays overlap; otherwise, false.</returns>
    public bool OverlapRay(Vector2 rayPoint, Vector2 rayDirection) => OverlapRayRay(Point, Direction, rayPoint, rayDirection);
    /// <summary>
    /// Determines whether this ray overlaps the specified circle.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <returns>True if the ray overlaps the circle; otherwise, false.</returns>
    public bool OverlapCircle(Vector2 circleCenter, float circleRadius) => OverlapRayCircle(Point, Direction, circleCenter, circleRadius);
    /// <summary>
    /// Determines whether this ray overlaps the specified triangle.
    /// </summary>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <returns>True if the ray overlaps the triangle; otherwise, false.</returns>
    public bool OverlapTriangle(Vector2 a, Vector2 b, Vector2 c) => OverlapRayTriangle(Point, Direction, a, b, c);
    /// <summary>
    /// Determines whether this ray overlaps the specified quad.
    /// </summary>
    /// <param name="a">The first vertex of the quad.</param>
    /// <param name="b">The second vertex of the quad.</param>
    /// <param name="c">The third vertex of the quad.</param>
    /// <param name="d">The fourth vertex of the quad.</param>
    /// <returns>True if the ray overlaps the quad; otherwise, false.</returns>
    public bool OverlapQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapRayQuad(Point, Direction, a, b, c, d);
    /// <summary>
    /// Determines whether this ray overlaps the specified rectangle.
    /// </summary>
    /// <param name="a">The first vertex of the rectangle.</param>
    /// <param name="b">The second vertex of the rectangle.</param>
    /// <param name="c">The third vertex of the rectangle.</param>
    /// <param name="d">The fourth vertex of the rectangle.</param>
    /// <returns>True if the ray overlaps the rectangle; otherwise, false.</returns>
    public bool OverlapRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapRayQuad(Point, Direction, a, b, c, d);
    /// <summary>
    /// Determines whether this ray overlaps the specified polygon.
    /// </summary>
    /// <param name="points">The list of points defining the polygon.</param>
    /// <returns>True if the ray overlaps the polygon; otherwise, false.</returns>
    public bool OverlapPolygon(List<Vector2> points)
    {
        return points.Count >= 3 && OverlapRayPolygon(Point, Direction, points);
    }
    /// <summary>
    /// Determines whether this ray overlaps the specified polyline.
    /// </summary>
    /// <param name="points">The list of points defining the polyline.</param>
    /// <returns>True if the ray overlaps the polyline; otherwise, false.</returns>
    public bool OverlapPolyline(List<Vector2> points)
    {
        return points.Count >= 2 && OverlapRayPolyline(Point, Direction, points);
    }
    /// <summary>
    /// Determines whether this ray overlaps the specified list of segments.
    /// </summary>
    /// <param name="segments">The list of segments to check for overlap.</param>
    /// <returns>True if the ray overlaps any of the segments; otherwise, false.</returns>
    public bool OverlapSegments(List<Segment> segments) => OverlapRaySegments(Point, Direction, segments);
    
    /// <summary>
    /// Checks if the ray overlaps with any collider in the given <see cref="CollisionObject"/>.
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
    /// Determines whether this ray overlaps the specified collider.
    /// </summary>
    /// <param name="collider">The collider to check for overlap. Must be enabled.</param>
    /// <returns>True if the ray overlaps the collider; otherwise, false.</returns>
    /// <remarks>
    /// The method dispatches to the appropriate shape-specific overlap method based on the collider's shape type.
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
    /// Determines whether this ray overlaps the specified segment.
    /// </summary>
    /// <param name="segment">The segment to check for overlap.</param>
    /// <returns>True if the ray overlaps the segment; otherwise, false.</returns>
    public bool OverlapShape(Segment segment) => OverlapRaySegment(Point, Direction, segment.Start, segment.End);
    /// <summary>
    /// Determines whether this ray overlaps the specified line.
    /// </summary>
    /// <param name="line">The line to check for overlap.</param>
    /// <returns>True if the ray overlaps the line; otherwise, false.</returns>
    public bool OverlapShape(Line line) => OverlapRayLine(Point, Direction, line.Point, line.Direction);
    /// <summary>
    /// Determines whether this ray overlaps another ray.
    /// </summary>
    /// <param name="ray">The other ray to check for overlap.</param>
    /// <returns>True if the rays overlap; otherwise, false.</returns>
    public bool OverlapShape(Ray ray) => OverlapRayRay(Point, Direction, ray.Point, ray.Direction);
    /// <summary>
    /// Determines whether this ray overlaps the specified circle.
    /// </summary>
    /// <param name="circle">The circle to check for overlap.</param>
    /// <returns>True if the ray overlaps the circle; otherwise, false.</returns>
    public bool OverlapShape(Circle circle) => OverlapRayCircle(Point, Direction, circle.Center, circle.Radius);
    /// <summary>
    /// Determines whether this ray overlaps the specified triangle.
    /// </summary>
    /// <param name="t">The triangle to check for overlap.</param>
    /// <returns>True if the ray overlaps the triangle; otherwise, false.</returns>
    public bool OverlapShape(Triangle t) => OverlapRayTriangle(Point, Direction, t.A, t.B, t.C);
    /// <summary>
    /// Determines whether this ray overlaps the specified quad.
    /// </summary>
    /// <param name="q">The quad to check for overlap.</param>
    /// <returns>True if the ray overlaps the quad; otherwise, false.</returns>
    public bool OverlapShape(Quad q) => OverlapRayQuad(Point, Direction, q.A, q.B, q.C, q.D);
    /// <summary>
    /// Determines whether this ray overlaps the specified rectangle.
    /// </summary>
    /// <param name="r">The rectangle to check for overlap.</param>
    /// <returns>True if the ray overlaps the rectangle; otherwise, false.</returns>
    public bool OverlapShape(Rect r) => OverlapRayQuad(Point, Direction, r.A, r.B, r.C, r.D);
    /// <summary>
    /// Determines whether this ray overlaps the specified polygon.
    /// </summary>
    /// <param name="p">The polygon to check for overlap.</param>
    /// <returns>True if the ray overlaps the polygon; otherwise, false.</returns>
    public bool OverlapShape(Polygon p) => OverlapRayPolygon(Point, Direction, p);
    /// <summary>
    /// Determines whether this ray overlaps the specified polyline.
    /// </summary>
    /// <param name="pl">The polyline to check for overlap.</param>
    /// <returns>True if the ray overlaps the polyline; otherwise, false.</returns>
    public bool OverlapShape(Polyline pl) => OverlapRayPolyline(Point, Direction, pl);
    /// <summary>
    /// Determines whether this ray overlaps the specified set of segments.
    /// </summary>
    /// <param name="segments">The set of segments to check for overlap.</param>
    /// <returns>True if the ray overlaps any of the segments; otherwise, false.</returns>
    public bool OverlapShape(Segments segments) => OverlapRaySegments(Point, Direction, segments);
    
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