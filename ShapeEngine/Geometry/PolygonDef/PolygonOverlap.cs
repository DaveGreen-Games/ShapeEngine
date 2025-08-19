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
    /// Checks if this polygon overlaps with a line segment.
    /// </summary>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <returns>True if the segment overlaps the polygon; otherwise, false.</returns>
    /// <remarks>Uses polygon-segment intersection logic.</remarks>
    public bool OverlapSegment(Vector2 segmentStart, Vector2 segmentEnd) => OverlapPolygonSegment(this, segmentStart, segmentEnd);
    
    /// <summary>
    /// Checks if this polygon overlaps with a line.
    /// </summary>
    /// <param name="linePoint">A point on the line.</param>
    /// <param name="lineDirection">The direction vector of the line.</param>
    /// <returns>True if the line overlaps the polygon; otherwise, false.</returns>
    /// <remarks>Line is infinite in both directions.</remarks>
    public bool OverlapLine(Vector2 linePoint, Vector2 lineDirection) => OverlapPolygonLine(this, linePoint, lineDirection);
    
    /// <summary>
    /// Checks if this polygon overlaps with a ray.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <returns>True if the ray overlaps the polygon; otherwise, false.</returns>
    /// <remarks>Ray is infinite in one direction from the origin.</remarks>
    public bool OverlapRay(Vector2 rayPoint, Vector2 rayDirection) => OverlapPolygonRay(this, rayPoint, rayDirection);
    
    /// <summary>
    /// Checks if this polygon overlaps with a circle.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <returns>True if the circle overlaps the polygon; otherwise, false.</returns>
    public bool OverlapCircle(Vector2 circleCenter, float circleRadius) => OverlapPolygonCircle(this, circleCenter, circleRadius);
    
    /// <summary>
    /// Checks if this polygon overlaps with a triangle.
    /// </summary>
    /// <param name="a">First vertex of the triangle.</param>
    /// <param name="b">Second vertex of the triangle.</param>
    /// <param name="c">Third vertex of the triangle.</param>
    /// <returns>True if the triangle overlaps the polygon; otherwise, false.</returns>
    public bool OverlapTriangle(Vector2 a, Vector2 b, Vector2 c) => OverlapPolygonTriangle(this, a, b, c);
    
    /// <summary>
    /// Checks if this polygon overlaps with a quadrilateral.
    /// </summary>
    /// <param name="a">First vertex of the quad.</param>
    /// <param name="b">Second vertex of the quad.</param>
    /// <param name="c">Third vertex of the quad.</param>
    /// <param name="d">Fourth vertex of the quad.</param>
    /// <returns>True if the quad overlaps the polygon; otherwise, false.</returns>
    public bool OverlapQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapPolygonQuad(this, a, b, c, d);
    
    /// <summary>
    /// Checks if this polygon overlaps with a rectangle (represented by four points).
    /// </summary>
    /// <param name="a">First vertex of the rectangle.</param>
    /// <param name="b">Second vertex of the rectangle.</param>
    /// <param name="c">Third vertex of the rectangle.</param>
    /// <param name="d">Fourth vertex of the rectangle.</param>
    /// <returns>True if the rectangle overlaps the polygon; otherwise, false.</returns>
    /// <remarks>Rectangle is treated as a quad.</remarks>
    public bool OverlapRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapPolygonQuad(this, a, b, c, d);
    
    /// <summary>
    /// Checks if this polygon overlaps with another polygon defined by a list of points.
    /// </summary>
    /// <param name="points">The vertices of the other polygon.</param>
    /// <returns>True if the polygons overlap; otherwise, false.</returns>
    public bool OverlapPolygon(List<Vector2> points) => OverlapPolygonPolygon(this, points);
    
    /// <summary>
    /// Checks if this polygon overlaps with a polyline.
    /// </summary>
    /// <param name="points">The points of the polyline.</param>
    /// <returns>True if the polyline overlaps the polygon; otherwise, false.</returns>
    public bool OverlapPolyline(List<Vector2> points) => OverlapPolygonPolyline(this, points);
    
    /// <summary>
    /// Checks if this polygon overlaps with a collection of segments.
    /// </summary>
    /// <param name="segments">The list of segments.</param>
    /// <returns>True if any segment overlaps the polygon; otherwise, false.</returns>
    public bool OverlapSegments(List<Segment> segments) => OverlapPolygonSegments(this, segments);
    
    /// <summary>
    /// Checks if this polygon overlaps with a line shape.
    /// </summary>
    /// <param name="line">The line shape.</param>
    /// <returns>True if the line overlaps the polygon; otherwise, false.</returns>
    public bool OverlapShape(Line line) => OverlapPolygonLine(this, line.Point, line.Direction);
    
    /// <summary>
    /// Checks if this polygon overlaps with a ray shape.
    /// </summary>
    /// <param name="ray">The ray shape.</param>
    /// <returns>True if the ray overlaps the polygon; otherwise, false.</returns>
    public bool OverlapShape(Ray ray) => OverlapPolygonRay(this, ray.Point, ray.Direction);
    
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
    /// Checks if this polygon overlaps with a generic collider shape.
    /// </summary>
    /// <param name="collider">The collider to test against.</param>
    /// <returns>True if the collider's shape overlaps the polygon; otherwise, false.</returns>
    /// <remarks>Supports multiple shape types via the collider interface.</remarks>
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
    /// Checks if this polygon overlaps with a segment shape.
    /// </summary>
    /// <param name="s">The segment shape to test against.</param>
    /// <returns>True if the segment overlaps the polygon; otherwise, false.</returns>
    /// <remarks>Delegates to the segment's overlap logic for polygons.</remarks>
    public bool OverlapShape(Segment s) => s.OverlapShape(this);
    
    /// <summary>
    /// Checks if this polygon overlaps with a circle shape.
    /// </summary>
    /// <param name="c">The circle shape to test against.</param>
    /// <returns>True if the circle overlaps the polygon; otherwise, false.</returns>
    /// <remarks>Delegates to the circle's overlap logic for polygons.</remarks>
    public bool OverlapShape(Circle c) => c.OverlapShape(this);
    
    /// <summary>
    /// Checks if this polygon overlaps with a triangle shape.
    /// </summary>
    /// <param name="t">The triangle shape to test against.</param>
    /// <returns>True if the triangle overlaps the polygon; otherwise, false.</returns>
    /// <remarks>Delegates to the triangle's overlap logic for polygons.</remarks>
    public bool OverlapShape(Triangle t) => t.OverlapShape(this);
    
    /// <summary>
    /// Checks if this polygon overlaps with a rectangle shape.
    /// </summary>
    /// <param name="r">The rectangle shape to test against.</param>
    /// <returns>True if the rectangle overlaps the polygon; otherwise, false.</returns>
    /// <remarks>Delegates to the rectangle's overlap logic for polygons.</remarks>
    public bool OverlapShape(Rect r) => r.OverlapShape(this);
    
    /// <summary>
    /// Checks if this polygon overlaps with a quadrilateral shape.
    /// </summary>
    /// <param name="q">The quadrilateral shape to test against.</param>
    /// <returns>True if the quadrilateral overlaps the polygon; otherwise, false.</returns>
    /// <remarks>Delegates to the quad's overlap logic for polygons.</remarks>
    public bool OverlapShape(Quad q) => q.OverlapShape(this);
    
    /// <summary>
    /// Checks if this polygon overlaps with another polygon.
    /// </summary>
    /// <param name="b">The other polygon to test against.</param>
    /// <returns>True if the polygons overlap; otherwise, false.</returns>
    /// <remarks>Uses edge intersection and point-in-polygon checks for both polygons.</remarks>
    public bool OverlapShape(Polygon b)
    {
        if (Count < 3 || b.Count < 3) return false;

        var oddNodesThis = false;
        var oddNodesB = false;
        var containsPointBCheckFinished = false;

        var pointToCeckThis = this[0];
        var pointToCeckB = b[0];

        for (var i = 0; i < Count; i++)
        {
            var start = this[i];
            var end = this[(i + 1) % Count];

            for (int j = 0; j < b.Count; j++)
            {
                var bStart = b[j];
                var bEnd = b[(j + 1) % b.Count];
                if (Segment.OverlapSegmentSegment(start, end, bStart, bEnd)) return true;

                if (containsPointBCheckFinished) continue;
                if (Polygon.ContainsPointCheck(bStart, bEnd, pointToCeckThis)) oddNodesB = !oddNodesB;
            }

            if (!containsPointBCheckFinished)
            {
                if (oddNodesB) return true;
                containsPointBCheckFinished = true;
            }

            if (Polygon.ContainsPointCheck(start, end, pointToCeckB)) oddNodesThis = !oddNodesThis;
        }

        return oddNodesThis || oddNodesB;
    }

    /// <summary>
    /// Checks if this polygon overlaps with a polyline shape.
    /// </summary>
    /// <param name="pl">The polyline shape to test against.</param>
    /// <returns>True if the polyline overlaps the polygon; otherwise, false.</returns>
    /// <remarks>Uses segment-segment intersection and point-in-polygon checks.</remarks>
    public bool OverlapShape(Polyline pl)
    {
        if (Count < 3 || pl.Count < 2) return false;

        var oddNodes = false;
        var pointToCeck = pl[0];


        for (var i = 0; i < Count; i++)
        {
            var start = this[i];
            var end = this[(i + 1) % Count];

            for (var j = 0; j < pl.Count - 1; j++)
            {
                var bStart = pl[j];
                var bEnd = pl[j + 1];
                if (Segment.OverlapSegmentSegment(start, end, bStart, bEnd)) return true;
            }

            if (Polygon.ContainsPointCheck(start, end, pointToCeck)) oddNodes = !oddNodes;
        }

        return oddNodes;
    }

    /// <summary>
    /// Checks if this polygon overlaps with a collection of segments.
    /// </summary>
    /// <param name="segments">The segments to test against.</param>
    /// <returns>True if any segment overlaps the polygon; otherwise, false.</returns>
    /// <remarks>Uses segment-segment intersection and point-in-polygon checks.</remarks>
    public bool OverlapShape(Segments segments)
    {
        if (Count < 3 || segments.Count <= 0) return false;

        var oddNodes = false;
        var pointToCeck = segments[0].Start;


        for (var i = 0; i < Count; i++)
        {
            var start = this[i];
            var end = this[(i + 1) % Count];

            foreach (var seg in segments)
            {
                if (Segment.OverlapSegmentSegment(start, end, seg.Start, seg.End)) return true;
            }

            if (Polygon.ContainsPointCheck(start, end, pointToCeck)) oddNodes = !oddNodes;
        }

        return oddNodes;
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