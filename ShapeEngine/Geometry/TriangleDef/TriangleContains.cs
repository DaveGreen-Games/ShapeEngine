using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;

namespace ShapeEngine.Geometry.TriangleDef;

public readonly partial struct Triangle
{
    public bool ContainsPoint(Vector2 p) => ContainsTrianglePoint(A, B, C, p);
    public bool ContainsPoints(Vector2 u, Vector2 v) => ContainsTrianglePoints(A, B, C, u, v);
    public bool ContainsPoints(Vector2 u, Vector2 v, Vector2 w) => ContainsTrianglePoints(A, B, C, u, v, w);
    public bool ContainsPoints(Vector2 u, Vector2 v, Vector2 w, Vector2 x) => ContainsTrianglePoints(A, B, C, u, v, w, x);
    public bool ContainsPoints(List<Vector2> points) => ContainsTrianglePoints(A, B, C, points);

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
        }

        return false;
    }

    public bool ContainsShape(Segment segment)
    {
        return ContainsTrianglePoints(A, B, C, segment.Start, segment.End);
    }

    public bool ContainsShape(Circle circle)
    {
        return ContainsTriangleCircle(A, B, C, circle.Center, circle.Radius);
    }

    public bool ContainsShape(Rect rect)
    {
        return ContainsTrianglePoints(A, B, C, rect.A, rect.B, rect.C, rect.D);
    }

    public bool ContainsShape(Triangle triangle)
    {
        return ContainsTrianglePoints(A, B, C, triangle.A, triangle.B, triangle.C);
    }

    public bool ContainsShape(Quad quad)
    {
        return ContainsTrianglePoints(A, B, C, quad.A, quad.B, quad.C, quad.D);
    }

    public bool ContainsShape(Polyline polyline)
    {
        return ContainsTrianglePoints(A, B, C, polyline);
    }

    public bool ContainsShape(Polygon polygon)
    {
        return ContainsTrianglePoints(A, B, C, polygon);
    }

    public bool ContainsShape(Points points)
    {
        return ContainsTrianglePoints(A, B, C, points);
    }

}