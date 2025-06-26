using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.PolygonDef;

public partial class Polygon
{
    public bool ContainsCollisionObject(CollisionObject collisionObject)
    {
        if (!collisionObject.HasColliders) return false;
        foreach (var collider in collisionObject.Colliders)
        {
            if (!ContainsCollider(collider)) return false;
        }

        return true;
    }

    public bool ContainsCollider(Collider collider)
    {
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

    public bool ContainsShape(SegmentDef.Segment segment) => ContainsPolygonSegment(this, segment.Start, segment.End);
    public bool ContainsShape(Circle circle) => ContainsPolygonCircle(this, circle.Center, circle.Radius);
    public bool ContainsShape(Rect rect) => ContainsPolygonRect(this, rect.A, rect.B, rect.C, rect.D);
    public bool ContainsShape(Triangle triangle) => ContainsPolygonTriangle(this, triangle.A, triangle.B, triangle.C);
    public bool ContainsShape(Quad quad) => ContainsPolygonQuad(this, quad.A, quad.B, quad.C, quad.D);
    public bool ContainsShape(Polyline polyline) => ContainsPolygonPolyline(this, polyline);
    public bool ContainsShape(Polygon polygon) => ContainsPolygonPolygon(this, polygon);

    public bool ContainsPoint(Vector2 p)
    {
        return ContainsPoint(this, p);
    }

    public bool ContainsSegment(Vector2 segmentStart, Vector2 segmentEnd) => ContainsPolygonSegment(this, segmentStart, segmentEnd);
    public bool ContainsTriangle(Vector2 a, Vector2 b, Vector2 c) => ContainsPolygonTriangle(this, a, b, c);
    public bool ContainsQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => ContainsPolygonQuad(this, a, b, c, d);

    public bool ContainsPoints(Vector2 a, Vector2 b)
    {
        return ContainsPoints(this, a, b);
    }

    public bool ContainsPoints(Vector2 a, Vector2 b, Vector2 c)
    {
        return ContainsPoints(this, a, b, c);
    }

    public bool ContainsPoints(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return ContainsPoints(this, a, b, c, d);
    }

    public bool ContainsPoints(PointsDef.Points points)
    {
        return ContainsPoints(this, points);
    }

}