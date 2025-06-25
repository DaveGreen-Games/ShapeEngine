using System.Numerics;
using ShapeEngine.Core.CollisionSystem;

namespace ShapeEngine.Geometry.Quad;

public readonly partial struct Quad
{
    public bool ContainsPoint(Vector2 p) => ContainsQuadPoint(A, B, C, D, p);
    public bool ContainsPoints(Vector2 u, Vector2 v) => ContainsQuadPoints(A, B, C, D, u, v);
    public bool ContainsPoints(Vector2 u, Vector2 v, Vector2 w) => ContainsQuadPoints(A, B, C, D, u, v, w);
    public bool ContainsPoints(Vector2 u, Vector2 v, Vector2 w, Vector2 x) => ContainsQuadPoints(A, B, C, D, u, v, w, x);
    public bool ContainsPoints(List<Vector2> points) => ContainsQuadPoints(A, B, C, D, points);

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
            case ShapeType.Rect: return ContainsShape(collider.GetRectShape());
            case ShapeType.Quad: return ContainsShape(collider.GetQuadShape());
            case ShapeType.Poly: return ContainsShape(collider.GetPolygonShape());
            case ShapeType.PolyLine: return ContainsShape(collider.GetPolylineShape());
        }

        return false;
    }

    public bool ContainsShape(Segment.Segment segment)
    {
        return ContainsQuadPoints(A, B, C, D, segment.Start, segment.End);
    }

    public bool ContainsShape(Circle.Circle circle)
    {
        return ContainsQuadCircle(A, B, C, D, circle.Center, circle.Radius);
    }

    public bool ContainsShape(Rect.Rect rect)
    {
        return ContainsQuadPoints(A, B, C, D, rect.A, rect.B, rect.C, rect.D);
    }

    public bool ContainsShape(Triangle.Triangle triangle)
    {
        return ContainsQuadPoints(A, B, C, D, triangle.A, triangle.B, triangle.C);
    }

    public bool ContainsShape(Quad quad)
    {
        return ContainsQuadPoints(A, B, C, D, quad.A, quad.B, quad.C, quad.D);
    }

    public bool ContainsShape(Polyline.Polyline polyline)
    {
        return ContainsQuadPoints(A, B, C, D, polyline);
    }

    public bool ContainsShape(Polygon.Polygon polygon)
    {
        return ContainsQuadPoints(A, B, C, D, polygon);
    }

    public bool ContainsShape(Points.Points points)
    {
        return ContainsQuadPoints(A, B, C, D, points);
    }

}