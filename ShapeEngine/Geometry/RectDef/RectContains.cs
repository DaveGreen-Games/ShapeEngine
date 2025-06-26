using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.RectDef;

public readonly partial struct Rect
{
    public bool ContainsPoint(Vector2 p) => ContainsRectPoint(TopLeft, BottomRight, p);
    public bool ContainsPoints(Vector2 u, Vector2 v) => ContainsRectPoints(TopLeft, BottomRight, u, v);
    public bool ContainsPoints(Vector2 u, Vector2 v, Vector2 w) => ContainsRectPoints(TopLeft, BottomRight, u, v, w);
    public bool ContainsPoints(Vector2 u, Vector2 v, Vector2 w, Vector2 x) => ContainsRectPoints(TopLeft, BottomRight, u, v, w, x);
    public bool ContainsPoints(List<Vector2> points) => ContainsRectPoints(TopLeft, BottomRight, points);

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
        return ContainsRectPoints(TopLeft, BottomRight, segment.Start, segment.End);
    }

    public bool ContainsShape(Circle circle)
    {
        return ContainsRectCircle(TopLeft, BottomRight, circle.Center, circle.Radius);
    }

    public bool ContainsShape(Rect rect)
    {
        return ContainsRectRect(TopLeft, BottomRight, rect.TopLeft, rect.BottomRight);
    }

    public bool ContainsShape(Triangle triangle)
    {
        return ContainsRectPoints(TopLeft, BottomRight, triangle.A, triangle.B, triangle.C);
    }

    public bool ContainsShape(Quad quad)
    {
        return ContainsRectPoints(TopLeft, BottomRight, quad.A, quad.B, quad.C, quad.D);
    }

    public bool ContainsShape(Polyline polyline)
    {
        return ContainsRectPoints(TopLeft, BottomRight, polyline);
    }

    public bool ContainsShape(Polygon polygon)
    {
        return ContainsRectPoints(TopLeft, BottomRight, polygon);
    }

    public bool ContainsShape(PointsDef.Points points)
    {
        return ContainsRectPoints(TopLeft, BottomRight, points);
    }
}