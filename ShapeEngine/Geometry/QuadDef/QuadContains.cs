using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.QuadDef;

public readonly partial struct Quad
{
    /// <summary>
    /// Determines whether the specified point is contained within this quad.
    /// </summary>
    /// <param name="p">The point to test.</param>
    /// <returns><c>true</c> if the point is inside the quad; otherwise, <c>false</c>.</returns>
    /// <remarks>Uses the quad's current vertices for the containment test.</remarks>
    public bool ContainsPoint(Vector2 p) => ContainsQuadPoint(A, B, C, D, p);
    /// <summary>
    /// Determines whether both specified points are contained within this quad.
    /// </summary>
    /// <param name="u">The first point to test.</param>
    /// <param name="v">The second point to test.</param>
    /// <returns><c>true</c> if both points are inside the quad; otherwise, <c>false</c>.</returns>
    public bool ContainsPoints(Vector2 u, Vector2 v) => ContainsQuadPoints(A, B, C, D, u, v);
    /// <summary>
    /// Determines whether all three specified points are contained within this quad.
    /// </summary>
    /// <param name="u">The first point to test.</param>
    /// <param name="v">The second point to test.</param>
    /// <param name="w">The third point to test.</param>
    /// <returns><c>true</c> if all points are inside the quad; otherwise, <c>false</c>.</returns>
    public bool ContainsPoints(Vector2 u, Vector2 v, Vector2 w) => ContainsQuadPoints(A, B, C, D, u, v, w);
    /// <summary>
    /// Determines whether all four specified points are contained within this quad.
    /// </summary>
    /// <param name="u">The first point to test.</param>
    /// <param name="v">The second point to test.</param>
    /// <param name="w">The third point to test.</param>
    /// <param name="x">The fourth point to test.</param>
    /// <returns><c>true</c> if all points are inside the quad; otherwise, <c>false</c>.</returns>
    public bool ContainsPoints(Vector2 u, Vector2 v, Vector2 w, Vector2 x) => ContainsQuadPoints(A, B, C, D, u, v, w, x);
    /// <summary>
    /// Determines whether all points in the list are contained within this quad.
    /// </summary>
    /// <param name="points">The list of points to test.</param>
    /// <returns><c>true</c> if all points are inside the quad; otherwise, <c>false</c>.</returns>
    public bool ContainsPoints(List<Vector2> points) => ContainsQuadPoints(A, B, C, D, points);
    /// <summary>
    /// Determines whether all colliders in the specified collision object are contained within this quad.
    /// </summary>
    /// <param name="collisionObject">The collision object to test.</param>
    /// <returns><c>true</c> if all colliders are inside the quad; otherwise, <c>false</c>.</returns>
    /// <remarks>Returns false if the object has no colliders or any collider is not contained.</remarks>
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
    /// Determines whether the specified collider is contained within this quad.
    /// </summary>
    /// <param name="collider">The collider to test. Has to be enabled.</param>
    /// <returns><c>true</c> if the collider is inside the quad; otherwise, <c>false</c>.</returns>
    public bool ContainsCollider(Collider collider)
    {
        if (!collider.Enabled) return false;
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
    /// <summary>
    /// Determines whether the specified segment is contained within this quad.
    /// </summary>
    /// <param name="segment">The segment to test.</param>
    /// <returns><c>true</c> if the segment is inside the quad; otherwise, <c>false</c>.</returns>
    public bool ContainsShape(Segment segment)
    {
        return ContainsQuadPoints(A, B, C, D, segment.Start, segment.End);
    }
    /// <summary>
    /// Determines whether the specified circle is contained within this quad.
    /// </summary>
    /// <param name="circle">The circle to test.</param>
    /// <returns><c>true</c> if the circle is inside the quad; otherwise, <c>false</c>.</returns>
    public bool ContainsShape(Circle circle)
    {
        return ContainsQuadCircle(A, B, C, D, circle.Center, circle.Radius);
    }
    /// <summary>
    /// Determines whether the specified rectangle is contained within this quad.
    /// </summary>
    /// <param name="rect">The rectangle to test.</param>
    /// <returns><c>true</c> if the rectangle is inside the quad; otherwise, <c>false</c>.</returns>
    public bool ContainsShape(Rect rect)
    {
        return ContainsQuadPoints(A, B, C, D, rect.A, rect.B, rect.C, rect.D);
    }
    /// <summary>
    /// Determines whether the specified triangle is contained within this quad.
    /// </summary>
    /// <param name="triangle">The triangle to test.</param>
    /// <returns><c>true</c> if the triangle is inside the quad; otherwise, <c>false</c>.</returns>
    public bool ContainsShape(Triangle triangle)
    {
        return ContainsQuadPoints(A, B, C, D, triangle.A, triangle.B, triangle.C);
    }
    /// <summary>
    /// Determines whether the specified quad is contained within this quad.
    /// </summary>
    /// <param name="quad">The quad to test.</param>
    /// <returns><c>true</c> if the quad is inside this quad; otherwise, <c>false</c>.</returns>
    public bool ContainsShape(Quad quad)
    {
        return ContainsQuadPoints(A, B, C, D, quad.A, quad.B, quad.C, quad.D);
    }
    /// <summary>
    /// Determines whether the specified polyline is contained within this quad.
    /// </summary>
    /// <param name="polyline">The polyline to test.</param>
    /// <returns><c>true</c> if the polyline is inside the quad; otherwise, <c>false</c>.</returns>
    public bool ContainsShape(Polyline polyline)
    {
        return ContainsQuadPoints(A, B, C, D, polyline);
    }
    /// <summary>
    /// Determines whether the specified polygon is contained within this quad.
    /// </summary>
    /// <param name="polygon">The polygon to test.</param>
    /// <returns><c>true</c> if the polygon is inside the quad; otherwise, <c>false</c>.</returns>
    public bool ContainsShape(Polygon polygon)
    {
        return ContainsQuadPoints(A, B, C, D, polygon);
    }
    /// <summary>
    /// Determines whether the specified set of points is contained within this quad.
    /// </summary>
    /// <param name="points">The set of points to test.</param>
    /// <returns><c>true</c> if all points are inside the quad; otherwise, <c>false</c>.</returns>
    public bool ContainsShape(Points points)
    {
        return ContainsQuadPoints(A, B, C, D, points);
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