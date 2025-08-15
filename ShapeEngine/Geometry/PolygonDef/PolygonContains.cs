using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.PolygonDef;


public partial class Polygon
{
    /// <summary>
    /// Determines whether this polygon fully contains a <see cref="CollisionObject"/> (all its colliders).
    /// </summary>
    /// <param name="collisionObject">The collision object to test.</param>
    /// <returns>True if all colliders are contained; otherwise, false.</returns>
    public bool ContainsCollisionObject(CollisionObject collisionObject)
    {
        if (!collisionObject.HasColliders) return false;
        foreach (var collider in collisionObject.Colliders)
        {
            if (!ContainsCollider(collider)) return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether this polygon fully contains a <see cref="Collider"/>.
    /// </summary>
    /// <param name="collider">The collider to test.</param>
    /// <returns>True if the collider is contained; otherwise, false.</returns>
    /// <remarks>
    /// The method dispatches to the appropriate shape-specific containment test.
    /// </remarks>
    public bool ContainsCollider(Collider collider)
    {
        if (!collider.Enabled) return false;
        switch (collider.GetShapeType())
        {
            case ShapeType.Circle: return ContainsShape(collider.GetCircleShape());
            case ShapeType.Segment: return ContainsShape(collider.GetSegmentShape());
            case ShapeType.Triangle: return ContainsShape(collider.GetTriangleShape());
            case ShapeType.Quad: return ContainsShape(collider.GetQuadShape());
            case ShapeType.Rect: return ContainsShape(collider.GetRectShape());
            case ShapeType.Poly: return ContainsShape(collider.GetPolygonShape());
            case ShapeType.PolyLine: return ContainsShape(collider.GetPolylineShape());

            case ShapeType.Line:
            case ShapeType.Ray:
            case ShapeType.None:
            default:
                break;
        }

        return false;
    }

    /// <summary>
    /// Determines whether this polygon contains a segment.
    /// </summary>
    /// <param name="segment">The segment to test.</param>
    /// <returns>True if the segment is fully contained; otherwise, false.</returns>
    public bool ContainsShape(Segment segment) => ContainsPolygonSegment(this, segment.Start, segment.End);
    /// <summary>
    /// Determines whether this polygon contains a circle.
    /// </summary>
    /// <param name="circle">The circle to test.</param>
    /// <returns>True if the circle is fully contained; otherwise, false.</returns>
    public bool ContainsShape(Circle circle) => ContainsPolygonCircle(this, circle.Center, circle.Radius);
    /// <summary>
    /// Determines whether this polygon contains a rectangle.
    /// </summary>
    /// <param name="rect">The rectangle to test.</param>
    /// <returns>True if the rectangle is fully contained; otherwise, false.</returns>
    public bool ContainsShape(Rect rect) => ContainsPolygonRect(this, rect.A, rect.B, rect.C, rect.D);
    /// <summary>
    /// Determines whether this polygon contains a triangle.
    /// </summary>
    /// <param name="triangle">The triangle to test.</param>
    /// <returns>True if the triangle is fully contained; otherwise, false.</returns>
    public bool ContainsShape(Triangle triangle) => ContainsPolygonTriangle(this, triangle.A, triangle.B, triangle.C);
    /// <summary>
    /// Determines whether this polygon contains a quad.
    /// </summary>
    /// <param name="quad">The quad to test.</param>
    /// <returns>True if the quad is fully contained; otherwise, false.</returns>
    public bool ContainsShape(Quad quad) => ContainsPolygonQuad(this, quad.A, quad.B, quad.C, quad.D);
    /// <summary>
    /// Determines whether this polygon contains a polyline.
    /// </summary>
    /// <param name="polyline">The polyline to test.</param>
    /// <returns>True if the polyline is fully contained; otherwise, false.</returns>
    public bool ContainsShape(Polyline polyline) => ContainsPolygonPolyline(this, polyline);
    /// <summary>
    /// Determines whether this polygon contains another polygon.
    /// </summary>
    /// <param name="polygon">The polygon to test.</param>
    /// <returns>True if the polygon is fully contained; otherwise, false.</returns>
    public bool ContainsShape(Polygon polygon) => ContainsPolygonPolygon(this, polygon);
    /// <summary>
    /// Determines whether this polygon contains a point.
    /// </summary>
    /// <param name="p">The point to test.</param>
    /// <returns>True if the point is inside the polygon; otherwise, false.</returns>
    public bool ContainsPoint(Vector2 p)
    {
        return ContainsPoint(this, p);
    }
    /// <summary>
    /// Determines whether this polygon contains a segment defined by two points.
    /// </summary>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <returns>True if the segment is fully contained; otherwise, false.</returns>
    public bool ContainsSegment(Vector2 segmentStart, Vector2 segmentEnd) => ContainsPolygonSegment(this, segmentStart, segmentEnd);
    /// <summary>
    /// Determines whether this polygon contains a triangle defined by three points.
    /// </summary>
    /// <param name="a">First vertex of the triangle.</param>
    /// <param name="b">Second vertex of the triangle.</param>
    /// <param name="c">Third vertex of the triangle.</param>
    /// <returns>True if the triangle is fully contained; otherwise, false.</returns>
    public bool ContainsTriangle(Vector2 a, Vector2 b, Vector2 c) => ContainsPolygonTriangle(this, a, b, c);
    /// <summary>
    /// Determines whether this polygon contains a quad defined by four points.
    /// </summary>
    /// <param name="a">First vertex of the quad.</param>
    /// <param name="b">Second vertex of the quad.</param>
    /// <param name="c">Third vertex of the quad.</param>
    /// <param name="d">Fourth vertex of the quad.</param>
    /// <returns>True if the quad is fully contained; otherwise, false.</returns>
    public bool ContainsQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => ContainsPolygonQuad(this, a, b, c, d);
    /// <summary>
    /// Determines whether this polygon contains two points.
    /// </summary>
    /// <param name="a">First point.</param>
    /// <param name="b">Second point.</param>
    /// <returns>True if both points are inside the polygon; otherwise, false.</returns>
    public bool ContainsPoints(Vector2 a, Vector2 b)
    {
        return ContainsPoints(this, a, b);
    }
    /// <summary>
    /// Determines whether this polygon contains three points.
    /// </summary>
    /// <param name="a">First point.</param>
    /// <param name="b">Second point.</param>
    /// <param name="c">Third point.</param>
    /// <returns>True if all points are inside the polygon; otherwise, false.</returns>
    public bool ContainsPoints(Vector2 a, Vector2 b, Vector2 c)
    {
        return ContainsPoints(this, a, b, c);
    }
    /// <summary>
    /// Determines whether this polygon contains four points.
    /// </summary>
    /// <param name="a">First point.</param>
    /// <param name="b">Second point.</param>
    /// <param name="c">Third point.</param>
    /// <param name="d">Fourth point.</param>
    /// <returns>True if all points are inside the polygon; otherwise, false.</returns>
    public bool ContainsPoints(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return ContainsPoints(this, a, b, c, d);
    }
    /// <summary>
    /// Determines whether this polygon contains all points in a set.
    /// </summary>
    /// <param name="points">The set of points to test.</param>
    /// <returns>True if all points are inside the polygon; otherwise, false.</returns>
    public bool ContainsPoints(Points points)
    {
        return ContainsPoints(this, points);
    }
    
    /// <summary>
    /// Determines whether this shape contains the specified <see cref="IShape"/>.
    /// </summary>
    /// <param name="shape">The shape to check.</param>
    /// <returns><c>true</c> if the shape is inside this shape; otherwise, <c>false</c>.</returns>
    public bool ContainsShape(IShape shape)
    {
        return shape.GetShapeType() switch
        {
            ShapeType.Circle => ContainsShape(shape.GetCircleShape()),
            ShapeType.Segment => ContainsShape(shape.GetSegmentShape()),
            ShapeType.Triangle => ContainsShape(shape.GetTriangleShape()),
            ShapeType.Rect => ContainsShape(shape.GetRectShape()),
            ShapeType.Quad => ContainsShape(shape.GetQuadShape()),
            ShapeType.Poly => ContainsShape(shape.GetPolygonShape()),
            ShapeType.PolyLine => ContainsShape(shape.GetPolylineShape()),
            _ => false
        };
    }

}