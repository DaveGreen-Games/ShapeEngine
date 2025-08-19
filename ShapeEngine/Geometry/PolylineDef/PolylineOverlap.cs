using System.Numerics;
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
    /// Determines whether the polyline overlaps the specified point.
    /// </summary>
    /// <param name="p">The point to test for overlap.</param>
    /// <returns><c>true</c> if the point overlaps any segment of the polyline; otherwise, <c>false</c>.</returns>
    public bool OverlapPoint(Vector2 p)
    {
        var segments = GetEdges();
        foreach (var segment in segments)
        {
            if (segment.OverlapPoint(p)) return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the polyline overlaps the specified segment.
    /// </summary>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <returns><c>true</c> if the polyline overlaps the segment; otherwise, <c>false</c>.</returns>
    public bool OverlapSegment(Vector2 segmentStart, Vector2 segmentEnd) => OverlapPolylineSegment(this, segmentStart, segmentEnd);

    /// <summary>
    /// Determines whether the polyline overlaps the specified line.
    /// </summary>
    /// <param name="linePoint">A point on the line.</param>
    /// <param name="lineDirection">The direction vector of the line.</param>
    /// <returns><c>true</c> if the polyline overlaps the line; otherwise, <c>false</c>.</returns>
    public bool OverlapLine(Vector2 linePoint, Vector2 lineDirection) => OverlapPolylineLine(this, linePoint, lineDirection);

    /// <summary>
    /// Determines whether the polyline overlaps the specified ray.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <returns><c>true</c> if the polyline overlaps the ray; otherwise, <c>false</c>.</returns>
    public bool OverlapRay(Vector2 rayPoint, Vector2 rayDirection) => OverlapPolylineRay(this, rayPoint, rayDirection);

    /// <summary>
    /// Determines whether the polyline overlaps the specified circle.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <returns><c>true</c> if the polyline overlaps the circle; otherwise, <c>false</c>.</returns>
    public bool OverlapCircle(Vector2 circleCenter, float circleRadius) => OverlapPolylineCircle(this, circleCenter, circleRadius);

    /// <summary>
    /// Determines whether the polyline overlaps the specified triangle.
    /// </summary>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <returns><c>true</c> if the polyline overlaps the triangle; otherwise, <c>false</c>.</returns>
    public bool OverlapTriangle(Vector2 a, Vector2 b, Vector2 c) => OverlapPolylineTriangle(this, a, b, c);

    /// <summary>
    /// Determines whether the polyline overlaps the specified quadrilateral.
    /// </summary>
    /// <param name="a">The first vertex of the quad.</param>
    /// <param name="b">The second vertex of the quad.</param>
    /// <param name="c">The third vertex of the quad.</param>
    /// <param name="d">The fourth vertex of the quad.</param>
    /// <returns><c>true</c> if the polyline overlaps the quad; otherwise, <c>false</c>.</returns>
    public bool OverlapQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapPolylineQuad(this, a, b, c, d);

    /// <summary>
    /// Determines whether the polyline overlaps the specified rectangle (as a quad).
    /// </summary>
    /// <param name="a">The first vertex of the rectangle.</param>
    /// <param name="b">The second vertex of the rectangle.</param>
    /// <param name="c">The third vertex of the rectangle.</param>
    /// <param name="d">The fourth vertex of the rectangle.</param>
    /// <returns><c>true</c> if the polyline overlaps the rectangle; otherwise, <c>false</c>.</returns>
    public bool OverlapRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapPolylineQuad(this, a, b, c, d);

    /// <summary>
    /// Determines whether the polyline overlaps the specified polygon.
    /// </summary>
    /// <param name="points">The vertices of the polygon.</param>
    /// <returns><c>true</c> if the polyline overlaps the polygon; otherwise, <c>false</c>.</returns>
    public bool OverlapPolygon(List<Vector2> points)
    {
        if (points.Count < 3) return false;
        return OverlapPolylinePolygon(this, points);
    }

    /// <summary>
    /// Determines whether the polyline overlaps another polyline.
    /// </summary>
    /// <param name="points">The vertices of the other polyline.</param>
    /// <returns><c>true</c> if the polylines overlap; otherwise, <c>false</c>.</returns>
    public bool OverlapPolyline(List<Vector2> points) => OverlapPolylinePolyline(this, points);

    /// <summary>
    /// Determines whether the polyline overlaps the specified segments.
    /// </summary>
    /// <param name="segments">The list of segments to test for overlap.</param>
    /// <returns><c>true</c> if the polyline overlaps any of the segments; otherwise, <c>false</c>.</returns>
    public bool OverlapSegments(List<Segment> segments) => OverlapPolylineSegments(this, segments);

    /// <summary>
    /// Determines whether the polyline overlaps the specified line shape.
    /// </summary>
    /// <param name="line">The <see cref="Line"/> shape to test for overlap.</param>
    /// <returns><c>true</c> if the polyline overlaps the line; otherwise, <c>false</c>.</returns>
    public bool OverlapShape(Line line) => OverlapPolylineLine(this, line.Point, line.Direction);

    /// <summary>
    /// Determines whether the polyline overlaps the specified ray shape.
    /// </summary>
    /// <param name="ray">The <see cref="Ray"/> shape to test for overlap.</param>
    /// <returns><c>true</c> if the polyline overlaps the ray; otherwise, <c>false</c>.</returns>
    public bool OverlapShape(Ray ray) => OverlapPolylineRay(this, ray.Point, ray.Direction);

    /// <summary>
    /// Checks if the polyline overlaps with any collider in the given <see cref="CollisionObject"/>.
    /// </summary>
    /// <param name="collision">The collision object containing colliders to check for overlap.</param>
    /// <returns>True if any collider in the collision object overlaps the polyline; otherwise, false.</returns>
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
    /// Determines whether the polyline overlaps the specified <see cref="Collider"/>.
    /// </summary>
    /// <param name="collider">The collider to test for overlap.</param>
    /// <returns><c>true</c> if the polyline overlaps the collider; otherwise, <c>false</c>.</returns>
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
    /// Determines whether the polyline overlaps the specified segments collection.
    /// </summary>
    /// <param name="segments">The <see cref="Segments"/> collection to test for overlap.</param>
    /// <returns><c>true</c> if the polyline overlaps any segment in the collection; otherwise, <c>false</c>.</returns>
    public bool OverlapShape(Segments segments)
    {
        if (Count < 2 || segments.Count <= 0) return false;

        for (var i = 0; i < Count - 1; i++)
        {
            var start = this[i];
            var end = this[(i + 1) % Count];

            foreach (var seg in segments)
            {
                if (Segment.OverlapSegmentSegment(start, end, seg.Start, seg.End)) return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the polyline overlaps the specified segment shape.
    /// </summary>
    /// <param name="s">The <see cref="Segment"/> shape to test for overlap.</param>
    /// <returns><c>true</c> if the polyline overlaps the segment; otherwise, <c>false</c>.</returns>
    public bool OverlapShape(Segment s) => s.OverlapShape(this);

    /// <summary>
    /// Determines whether the polyline overlaps the specified circle shape.
    /// </summary>
    /// <param name="c">The <see cref="Circle"/> shape to test for overlap.</param>
    /// <returns><c>true</c> if the polyline overlaps the circle; otherwise, <c>false</c>.</returns>
    public bool OverlapShape(Circle c) => c.OverlapShape(this);

    /// <summary>
    /// Determines whether the polyline overlaps the specified triangle shape.
    /// </summary>
    /// <param name="t">The <see cref="Triangle"/> shape to test for overlap.</param>
    /// <returns><c>true</c> if the polyline overlaps the triangle; otherwise, <c>false</c>.</returns>
    public bool OverlapShape(Triangle t) => t.OverlapShape(this);

    /// <summary>
    /// Determines whether the polyline overlaps the specified rectangle shape.
    /// </summary>
    /// <param name="r">The <see cref="Rect"/> shape to test for overlap.</param>
    /// <returns><c>true</c> if the polyline overlaps the rectangle; otherwise, <c>false</c>.</returns>
    public bool OverlapShape(Rect r) => r.OverlapShape(this);

    /// <summary>
    /// Determines whether the polyline overlaps the specified quadrilateral shape.
    /// </summary>
    /// <param name="q">The <see cref="Quad"/> shape to test for overlap.</param>
    /// <returns><c>true</c> if the polyline overlaps the quad; otherwise, <c>false</c>.</returns>
    public bool OverlapShape(Quad q) => q.OverlapShape(this);

    /// <summary>
    /// Determines whether the polyline overlaps the specified polygon shape.
    /// </summary>
    /// <param name="p">The <see cref="Polygon"/> shape to test for overlap.</param>
    /// <returns><c>true</c> if the polyline overlaps the polygon; otherwise, <c>false</c>.</returns>
    public bool OverlapShape(Polygon p) => p.OverlapShape(this);

    /// <summary>
    /// Determines whether the polyline overlaps another <see cref="Polyline"/> shape.
    /// </summary>
    /// <param name="b">The other <see cref="Polyline"/> to test for overlap.</param>
    /// <returns><c>true</c> if the polylines overlap; otherwise, <c>false</c>.</returns>
    public bool OverlapShape(Polyline b)
    {
        if (Count < 2 || b.Count < 2) return false;

        for (var i = 0; i < Count - 1; i++)
        {
            var start = this[i];
            var end = this[i + 1];

            for (var j = 0; j < b.Count - 1; j++)
            {
                var bStart = b[j];
                var bEnd = b[j + 1];

                if (Segment.OverlapSegmentSegment(start, end, bStart, bEnd)) return true;
            }
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