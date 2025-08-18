using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.RectDef;

public readonly partial struct Rect
{
    /// <summary>
    /// Checks if the rectangle overlaps with the given segment.
    /// </summary>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <returns>True if the segment overlaps the rectangle; otherwise, false.</returns>
    public bool OverlapSegment(Vector2 segmentStart, Vector2 segmentEnd) => OverlapRectSegment(A, B, C, D, segmentStart, segmentEnd);

    /// <summary>
    /// Checks if the rectangle overlaps with the given line.
    /// </summary>
    /// <param name="linePoint">A point on the line.</param>
    /// <param name="lineDirection">The direction of the line.</param>
    /// <returns>True if the line overlaps the rectangle; otherwise, false.</returns>
    public bool OverlapLine(Vector2 linePoint, Vector2 lineDirection) => OverlapRectLine(A, B, C, D, linePoint, lineDirection);

    /// <summary>
    /// Checks if the rectangle overlaps with the given ray.
    /// </summary>
    /// <param name="rayPoint">The origin of the ray.</param>
    /// <param name="rayDirection">The direction of the ray.</param>
    /// <returns>True if the ray overlaps the rectangle; otherwise, false.</returns>
    public bool OverlapRay(Vector2 rayPoint, Vector2 rayDirection) => OverlapRectRay(A, B, C, D, rayPoint, rayDirection);

    /// <summary>
    /// Checks if the rectangle overlaps with the given circle.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <returns>True if the circle overlaps the rectangle; otherwise, false.</returns>
    public bool OverlapCircle(Vector2 circleCenter, float circleRadius) => OverlapRectCircle(A, B, C, D, circleCenter, circleRadius);

    /// <summary>
    /// Checks if the rectangle overlaps with the given triangle.
    /// </summary>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <returns>True if the triangle overlaps the rectangle; otherwise, false.</returns>
    public bool OverlapTriangle(Vector2 a, Vector2 b, Vector2 c) => OverlapRectTriangle(A, B, C, D, a, b, c);

    /// <summary>
    /// Checks if the rectangle overlaps with the given quadrilateral.
    /// </summary>
    /// <param name="a">The first vertex of the quad.</param>
    /// <param name="b">The second vertex of the quad.</param>
    /// <param name="c">The third vertex of the quad.</param>
    /// <param name="d">The fourth vertex of the quad.</param>
    /// <returns>True if the quad overlaps the rectangle; otherwise, false.</returns>
    public bool OverlapQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapRectQuad(A, B, C, D, a, b, c, d);

    /// <summary>
    /// Checks if the rectangle overlaps with another rectangle defined by four corners.
    /// </summary>
    /// <param name="a">The first corner of the other rectangle.</param>
    /// <param name="b">The second corner of the other rectangle.</param>
    /// <param name="c">The third corner of the other rectangle.</param>
    /// <param name="d">The fourth corner of the other rectangle.</param>
    /// <returns>True if the rectangles overlap; otherwise, false.</returns>
    public bool OverlapRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapRectQuad(A, B, C, D, a, b, c, d);

    /// <summary>
    /// Checks if the rectangle overlaps with the given polygon.
    /// </summary>
    /// <param name="points">The list of polygon points.</param>
    /// <returns>True if the polygon overlaps the rectangle; otherwise, false.</returns>
    public bool OverlapPolygon(List<Vector2> points)
    {
        return points.Count >= 3 && OverlapRectPolygon(A, B, C, D, points);
    }

    /// <summary>
    /// Checks if the rectangle overlaps with the given polyline.
    /// </summary>
    /// <param name="points">The list of polyline points.</param>
    /// <returns>True if the polyline overlaps the rectangle; otherwise, false.</returns>
    public bool OverlapPolyline(List<Vector2> points)
    {
        return points.Count >= 2 && OverlapRectPolyline(A, B, C, D, points);
    }

    /// <summary>
    /// Checks if the rectangle overlaps with the given list of segments.
    /// </summary>
    /// <param name="segments">The list of segments.</param>
    /// <returns>True if any segment overlaps the rectangle; otherwise, false.</returns>
    public bool OverlapSegments(List<Segment> segments) => OverlapRectSegments(A, B, C, D, segments);

    /// <summary>
    /// Checks if the rectangle overlaps with the given line shape.
    /// </summary>
    /// <param name="line">The line shape.</param>
    /// <returns>True if the line overlaps the rectangle; otherwise, false.</returns>
    public bool OverlapShape(Line line) => OverlapRectLine(A, B, C, D, line.Point, line.Direction);

    /// <summary>
    /// Checks if the rectangle overlaps with the given ray shape.
    /// </summary>
    /// <param name="ray">The ray shape.</param>
    /// <returns>True if the ray overlaps the rectangle; otherwise, false.</returns>
    public bool OverlapShape(Ray ray) => OverlapRectRay(A, B, C, D, ray.Point, ray.Direction);

    /// <summary>
    /// Checks if the rect overlaps with any collider in the given <see cref="CollisionObject"/>.
    /// </summary>
    /// <param name="collision">The collision object containing colliders to check for overlap.</param>
    /// <returns>True if any collider in the collision object overlaps the rect; otherwise, false.</returns>
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
    /// Checks if the rectangle overlaps with the specified collider.
    /// </summary>
    /// <param name="collider">The collider to check for overlap.</param>
    /// <returns>True if the collider overlaps the rectangle; otherwise, false.</returns>
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
    /// Checks if the rectangle overlaps with a collection of segments.
    /// </summary>
    /// <param name="segments">The collection of segments.</param>
    /// <returns>True if any segment overlaps the rectangle; otherwise, false.</returns>
    public bool OverlapShape(Segments segments)
    {
        foreach (var seg in segments)
        {
            if (seg.OverlapShape(this)) return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if the rectangle overlaps with a segment shape.
    /// </summary>
    /// <param name="s">The segment shape.</param>
    /// <returns>True if the segment overlaps the rectangle; otherwise, false.</returns>
    public bool OverlapShape(Segment s) => s.OverlapShape(this);

    /// <summary>
    /// Checks if the rectangle overlaps with a circle shape.
    /// </summary>
    /// <param name="c">The circle shape.</param>
    /// <returns>True if the circle overlaps the rectangle; otherwise, false.</returns>
    public bool OverlapShape(Circle c) => c.OverlapShape(this);

    /// <summary>
    /// Checks if the rectangle overlaps with a triangle shape.
    /// </summary>
    /// <param name="t">The triangle shape.</param>
    /// <returns>True if the triangle overlaps the rectangle; otherwise, false.</returns>
    public bool OverlapShape(Triangle t) => t.OverlapShape(this);

    /// <summary>
    /// Checks if the rectangle overlaps with a quad shape.
    /// </summary>
    /// <param name="q">The quad shape.</param>
    /// <returns>True if the quad overlaps the rectangle; otherwise, false.</returns>
    public bool OverlapShape(Quad q) => q.OverlapShape(this);

    /// <summary>
    /// Checks if the rectangle overlaps with another rectangle.
    /// </summary>
    /// <param name="b">The other rectangle.</param>
    /// <returns>True if the rectangles overlap; otherwise, false.</returns>
    public bool OverlapShape(Rect b)
    {
        var aTopLeft = new Vector2(X, Y);
        var aBottomRight = aTopLeft + new Vector2(Width, Height);
        var bTopLeft = new Vector2(b.X, b.Y);
        var bBottomRight = bTopLeft + new Vector2(b.Width, b.Height);
        return
            ValueRange.OverlapValueRange(aTopLeft.X, aBottomRight.X, bTopLeft.X, bBottomRight.X) &&
            ValueRange.OverlapValueRange(aTopLeft.Y, aBottomRight.Y, bTopLeft.Y, bBottomRight.Y);
    }

    /// <summary>
    /// Checks if the rectangle overlaps with a polygon shape.
    /// </summary>
    /// <param name="poly">The polygon shape.</param>
    /// <returns>True if the polygon overlaps the rectangle; otherwise, false.</returns>
    public bool OverlapShape(Polygon poly)
    {
        if (poly.Count < 3) return false;

        if (ContainsPoint(poly[0])) return true;

        var oddNodes = false;
        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;
        for (var i = 0; i < poly.Count; i++)
        {
            var start = poly[i];
            var end = poly[(i + 1) % poly.Count];
            if (Segment.OverlapSegmentSegment(a, b, start, end)) return true;
            if (Segment.OverlapSegmentSegment(b, c, start, end)) return true;
            if (Segment.OverlapSegmentSegment(c, d, start, end)) return true;
            if (Segment.OverlapSegmentSegment(d, a, start, end)) return true;

            if (Polygon.ContainsPointCheck(start, end, a)) oddNodes = !oddNodes;
        }

        return oddNodes;
    }

    /// <summary>
    /// Checks if the rectangle overlaps with a polyline shape.
    /// </summary>
    /// <param name="pl">The polyline shape.</param>
    /// <returns>True if the polyline overlaps the rectangle; otherwise, false.</returns>
    public bool OverlapShape(Polyline pl)
    {
        if (pl.Count < 2) return false;

        if (ContainsPoint(pl[0])) return true;
        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;
        for (var i = 0; i < pl.Count - 1; i++)
        {
            var start = pl[i];
            var end = pl[(i + 1) % pl.Count];
            if (Segment.OverlapSegmentSegment(a, b, start, end)) return true;
            if (Segment.OverlapSegmentSegment(b, c, start, end)) return true;
            if (Segment.OverlapSegmentSegment(c, d, start, end)) return true;
            if (Segment.OverlapSegmentSegment(d, a, start, end)) return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if the rectangle overlaps with a line defined by a position and direction.
    /// </summary>
    /// <param name="linePos">A point on the line.</param>
    /// <param name="lineDir">The direction of the line.</param>
    /// <returns>True if the line overlaps the rectangle; otherwise, false.</returns>
    public bool OverlapRectLine(Vector2 linePos, Vector2 lineDir)
    {
        var n = lineDir.Rotate90CCW();

        var c1 = new Vector2(X, Y);
        var c2 = c1 + new Vector2(Width, Height);
        var c3 = new Vector2(c2.X, c1.Y);
        var c4 = new Vector2(c1.X, c2.Y);

        c1 -= linePos;
        c2 -= linePos;
        c3 -= linePos;
        c4 -= linePos;

        float dp1 = Vector2.Dot(n, c1);
        float dp2 = Vector2.Dot(n, c2);
        float dp3 = Vector2.Dot(n, c3);
        float dp4 = Vector2.Dot(n, c4);

        return dp1 * dp2 <= 0.0f || dp2 * dp3 <= 0.0f || dp3 * dp4 <= 0.0f;
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