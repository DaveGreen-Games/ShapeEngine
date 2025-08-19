using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.LineDef;

public readonly partial struct Line
{
    /// <summary>
    /// Determines whether the specified point lies on this infinite line.
    /// </summary>
    /// <param name="p">The point to check.</param>
    /// <returns>True if the point lies on the line; otherwise, false.</returns>
    public bool OverlapPoint(Vector2 p) => IsPointOnLine(Point, Direction, p);

    /// <summary>
    /// Determines whether this infinite line and a finite segment overlap (i.e., intersect at any point).
    /// </summary>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <returns>True if the line and segment overlap (intersect); otherwise, false.</returns>
    public bool OverlapSegment(Vector2 segmentStart, Vector2 segmentEnd) => OverlapLineSegment(Point, Direction, segmentStart, segmentEnd);

    /// <summary>
    /// Determines whether this infinite line and another infinite line overlap (i.e., are collinear).
    /// </summary>
    /// <param name="linePoint">A point on the other line.</param>
    /// <param name="lineDirection">The direction vector of the other line.</param>
    /// <returns>True if the lines overlap; otherwise, false.</returns>
    public bool OverlapLine(Vector2 linePoint, Vector2 lineDirection) => OverlapLineLine(Point, Direction, linePoint, lineDirection);

    /// <summary>
    /// Determines whether this infinite line and a ray overlap (i.e., are collinear and extend in the same direction).
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <returns>True if the line and ray overlap; otherwise, false.</returns>
    public bool OverlapRay(Vector2 rayPoint, Vector2 rayDirection) => OverlapLineRay(Point, Direction, rayPoint, rayDirection);

    /// <summary>
    /// Determines whether this infinite line and a circle overlap (i.e., the line passes through or touches the circle).
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <returns>True if the line and circle overlap; otherwise, false.</returns>
    public bool OverlapCircle(Vector2 circleCenter, float circleRadius) => OverlapLineCircle(Point, Direction, circleCenter, circleRadius);

    /// <summary>
    /// Determines whether this infinite line and a triangle defined by three vertices overlap (i.e., the line passes through or touches the triangle).
    /// </summary>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <returns>True if the line and triangle overlap; otherwise, false.</returns>
    public bool OverlapTriangle(Vector2 a, Vector2 b, Vector2 c) => OverlapLineTriangle(Point, Direction, a, b, c);

    /// <summary>
    /// Determines whether this infinite line and a quadrilateral defined by four vertices overlap (i.e., the line passes through or touches the quadrilateral).
    /// </summary>
    /// <param name="a">The first vertex of the quadrilateral.</param>
    /// <param name="b">The second vertex of the quadrilateral.</param>
    /// <param name="c">The third vertex of the quadrilateral.</param>
    /// <param name="d">The fourth vertex of the quadrilateral.</param>
    /// <returns>True if the line and quadrilateral overlap; otherwise, false.</returns>
    public bool OverlapQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapLineQuad(Point, Direction, a, b, c, d);

    /// <summary>
    /// Determines whether this infinite line and a rectangle defined by four vertices overlap (i.e., the line passes through or touches the rectangle).
    /// </summary>
    /// <param name="a">The first vertex of the rectangle.</param>
    /// <param name="b">The second vertex of the rectangle.</param>
    /// <param name="c">The third vertex of the rectangle.</param>
    /// <param name="d">The fourth vertex of the rectangle.</param>
    /// <returns>True if the line and rectangle overlap; otherwise, false.</returns>
    public bool OverlapRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapLineQuad(Point, Direction, a, b, c, d);

    /// <summary>
    /// Determines whether this infinite line and a polygon overlap (i.e., the line passes through or touches the polygon).
    /// </summary>
    /// <param name="points">A list of vertices defining the polygon. The polygon is assumed to be closed and non-self-intersecting.</param>
    /// <returns>True if the line and polygon overlap; otherwise, false.</returns>
    public bool OverlapPolygon(List<Vector2> points)
    {
        if (points.Count < 3) return false;
        return OverlapLinePolygon(Point, Direction, points);
    }

    /// <summary>
    /// Determines whether this infinite line and a polyline overlap (i.e., the line passes through or touches any segment of the polyline).
    /// </summary>
    /// <param name="points">A list of vertices defining the polyline. The polyline is assumed to be open and non-self-intersecting.</param>
    /// <returns>True if the line and polyline overlap; otherwise, false.</returns>
    public bool OverlapPolyline(List<Vector2> points)
    {
        if (points.Count < 2) return false;
        return OverlapLinePolyline(Point, Direction, points);
    }

    /// <summary>
    /// Determines whether this infinite line and any segment in the provided list overlap (i.e., intersect at any point).
    /// </summary>
    /// <param name="segments">A list of <see cref="Segment"/> objects to check for overlap with the line.</param>
    /// <returns>True if the line overlaps with any segment in the list; otherwise, false.</returns>
    public bool OverlapSegments(List<Segment> segments) => OverlapLineSegments(Point, Direction, segments);

    /// <summary>
    /// Checks if the line overlaps with any collider in the given <see cref="CollisionObject"/>.
    /// </summary>
    /// <param name="collision">The collision object containing colliders to check for overlap.</param>
    /// <returns>True if any collider in the collision object overlaps the line; otherwise, false.</returns>
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
    /// Determines whether this infinite line overlaps (intersects) with the shape of the specified <see cref="Collider"/>.
    /// </summary>
    /// <param name="collider">The <see cref="Collider"/> whose shape will be checked for overlap with this line.</param>
    /// <returns>True if the line overlaps with the collider's shape; otherwise, false.</returns>
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
    /// Determines whether this infinite line and another <see cref="Line"/> overlap (i.e., are collinear).
    /// </summary>
    /// <param name="line">The <see cref="Line"/> to check for overlap.</param>
    /// <returns>True if the lines overlap; otherwise, false.</returns>
    public bool OverlapShape(Line line) => OverlapLineLine(Point, Direction, line.Point, line.Direction);

    /// <summary>
    /// Determines whether this infinite line and a <see cref="Ray"/> overlap (i.e., are collinear and extend in the same direction).
    /// </summary>
    /// <param name="ray">The <see cref="Ray"/> to check for overlap.</param>
    /// <returns>True if the line and ray overlap; otherwise, false.</returns>
    public bool OverlapShape(Ray ray) => OverlapLineRay(Point, Direction, ray.Point, ray.Direction);

    /// <summary>
    /// Determines whether this infinite line and the specified <see cref="Segment"/> overlap (i.e., intersect at any point).
    /// </summary>
    /// <param name="segment">The <see cref="Segment"/> to check for overlap with this line.</param>
    /// <returns>True if the line and segment overlap (intersect); otherwise, false.</returns>
    public bool OverlapShape(Segment segment) => OverlapLineSegment(Point, Direction, segment.Start, segment.End);

    /// <summary>
    /// Determines whether this infinite line and the specified <see cref="Circle"/> overlap (i.e., the line passes through or touches the circle).
    /// </summary>
    /// <param name="circle">The <see cref="Circle"/> to check for overlap with this line.</param>
    /// <returns>True if the line and circle overlap; otherwise, false.</returns>
    public bool OverlapShape(Circle circle) => OverlapLineCircle(Point, Direction, circle.Center, circle.Radius);

    /// <summary>
    /// Determines whether this infinite line and the specified <see cref="Triangle"/> overlap (i.e., the line passes through or touches the triangle).
    /// </summary>
    /// <param name="t">The <see cref="Triangle"/> to check for overlap with this line.</param>
    /// <returns>True if the line and triangle overlap; otherwise, false.</returns>
    public bool OverlapShape(Triangle t) => OverlapLineTriangle(Point, Direction, t.A, t.B, t.C);

    /// <summary>
    /// Determines whether this infinite line and the specified <see cref="Quad"/> overlap (i.e., the line passes through or touches the quadrilateral).
    /// </summary>
    /// <param name="q">The <see cref="Quad"/> to check for overlap with this line.</param>
    /// <returns>True if the line and quad overlap; otherwise, false.</returns>
    public bool OverlapShape(Quad q) => OverlapLineQuad(Point, Direction, q.A, q.B, q.C, q.D);

    /// <summary>
    /// Determines whether this infinite line and the specified <see cref="Rect"/> overlap (i.e., the line passes through or touches the rectangle).
    /// </summary>
    /// <param name="r">The <see cref="Rect"/> to check for overlap with this line.</param>
    /// <returns>True if the line and rectangle overlap; otherwise, false.</returns>
    public bool OverlapShape(Rect r) => OverlapLineQuad(Point, Direction, r.A, r.B, r.C, r.D);

    /// <summary>
    /// Determines whether this infinite line and the specified <see cref="Polygon"/> overlap (i.e., the line passes through or touches the polygon).
    /// </summary>
    /// <param name="p">The <see cref="Polygon"/> to check for overlap with this line.</param>
    /// <returns>True if the line and polygon overlap; otherwise, false.</returns>
    public bool OverlapShape(Polygon p) => OverlapLinePolygon(Point, Direction, p);

    /// <summary>
    /// Determines whether this infinite line and the specified <see cref="Polyline"/> overlap (i.e., the line passes through or touches any segment of the polyline).
    /// </summary>
    /// <param name="pl">The <see cref="Polyline"/> to check for overlap with this line.</param>
    /// <returns>True if the line and polyline overlap; otherwise, false.</returns>
    public bool OverlapShape(Polyline pl) => OverlapLinePolyline(Point, Direction, pl);

    /// <summary>
    /// Determines whether this infinite line overlaps (intersects) with any segment in the provided <see cref="Segments"/> collection.
    /// </summary>
    /// <param name="segments">A <see cref="Segments"/> collection to check for overlap with this line.</param>
    /// <returns>True if the line overlaps with any segment in the collection; otherwise, false.</returns>
    public bool OverlapShape(Segments segments) => OverlapLineSegments(Point, Direction, segments);
    
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