using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.RectDef;

public readonly partial struct Rect
{
    /// <summary>
    /// Determines whether the specified point is contained within this rectangle.
    /// </summary>
    /// <param name="p">The point to test for containment.</param>
    /// <returns>True if the point is inside the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// The check is inclusive of the rectangle's edges.
    /// </remarks>
    public bool ContainsPoint(Vector2 p) => ContainsRectPoint(TopLeft, BottomRight, p);

    /// <summary>
    /// Determines whether both specified points are contained within this rectangle.
    /// </summary>
    /// <param name="u">The first point to test.</param>
    /// <param name="v">The second point to test.</param>
    /// <returns>True if both points are inside the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// Both points must be inside or on the edge of the rectangle.
    /// </remarks>
    public bool ContainsPoints(Vector2 u, Vector2 v) => ContainsRectPoints(TopLeft, BottomRight, u, v);

    /// <summary>
    /// Determines whether all three specified points are contained within this rectangle.
    /// </summary>
    /// <param name="u">The first point to test.</param>
    /// <param name="v">The second point to test.</param>
    /// <param name="w">The third point to test.</param>
    /// <returns>True if all points are inside the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// All points must be inside or on the edge of the rectangle.
    /// </remarks>
    public bool ContainsPoints(Vector2 u, Vector2 v, Vector2 w) => ContainsRectPoints(TopLeft, BottomRight, u, v, w);

    /// <summary>
    /// Determines whether all four specified points are contained within this rectangle.
    /// </summary>
    /// <param name="u">The first point to test.</param>
    /// <param name="v">The second point to test.</param>
    /// <param name="w">The third point to test.</param>
    /// <param name="x">The fourth point to test.</param>
    /// <returns>True if all points are inside the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// All points must be inside or on the edge of the rectangle.
    /// </remarks>
    public bool ContainsPoints(Vector2 u, Vector2 v, Vector2 w, Vector2 x) => ContainsRectPoints(TopLeft, BottomRight, u, v, w, x);

    /// <summary>
    /// Determines whether all points in the specified list are contained within this rectangle.
    /// </summary>
    /// <param name="points">The list of points to test.</param>
    /// <returns>True if all points are inside the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// All points must be inside or on the edge of the rectangle.
    /// </remarks>
    public bool ContainsPoints(List<Vector2> points) => ContainsRectPoints(TopLeft, BottomRight, points);

    /// <summary>
    /// Determines whether the specified collision object is fully contained within this rectangle.
    /// </summary>
    /// <param name="collisionObject">The collision object to test.</param>
    /// <returns>True if all colliders of the object are inside the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// Returns false if the object has no colliders or any collider is not fully contained.
    /// </remarks>
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
    /// Determines whether the specified collider is fully contained within this rectangle.
    /// </summary>
    /// <param name="collider">The collider to test.</param>
    /// <returns>True if the collider is inside the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// The method dispatches to the appropriate shape-specific containment check.
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
        }

        return false;
    }

    /// <summary>
    /// Determines whether the specified segment is fully contained within this rectangle.
    /// </summary>
    /// <param name="segment">The segment to test.</param>
    /// <returns>True if the segment is inside the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// Both endpoints of the segment must be inside or on the edge of the rectangle.
    /// </remarks>
    public bool ContainsShape(Segment segment)
    {
        return ContainsRectPoints(TopLeft, BottomRight, segment.Start, segment.End);
    }

    /// <summary>
    /// Determines whether the specified circle is fully contained within this rectangle.
    /// </summary>
    /// <param name="circle">The circle to test.</param>
    /// <returns>True if the circle is inside the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// The entire area of the circle must be inside or on the edge of the rectangle.
    /// </remarks>
    public bool ContainsShape(Circle circle)
    {
        return ContainsRectCircle(TopLeft, BottomRight, circle.Center, circle.Radius);
    }

    /// <summary>
    /// Determines whether the specified rectangle is fully contained within this rectangle.
    /// </summary>
    /// <param name="rect">The rectangle to test.</param>
    /// <returns>True if the rectangle is inside the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// All corners of the rectangle must be inside or on the edge of this rectangle.
    /// </remarks>
    public bool ContainsShape(Rect rect)
    {
        return ContainsRectRect(TopLeft, BottomRight, rect.TopLeft, rect.BottomRight);
    }

    /// <summary>
    /// Determines whether the specified triangle is fully contained within this rectangle.
    /// </summary>
    /// <param name="triangle">The triangle to test.</param>
    /// <returns>True if the triangle is inside the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// All vertices of the triangle must be inside or on the edge of the rectangle.
    /// </remarks>
    public bool ContainsShape(Triangle triangle)
    {
        return ContainsRectPoints(TopLeft, BottomRight, triangle.A, triangle.B, triangle.C);
    }

    /// <summary>
    /// Determines whether the specified quadrilateral is fully contained within this rectangle.
    /// </summary>
    /// <param name="quad">The quadrilateral to test.</param>
    /// <returns>True if the quadrilateral is inside the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// All vertices of the quadrilateral must be inside or on the edge of the rectangle.
    /// </remarks>
    public bool ContainsShape(Quad quad)
    {
        return ContainsRectPoints(TopLeft, BottomRight, quad.A, quad.B, quad.C, quad.D);
    }

    /// <summary>
    /// Determines whether the specified polyline is fully contained within this rectangle.
    /// </summary>
    /// <param name="polyline">The polyline to test.</param>
    /// <returns>True if all points of the polyline are inside the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// All points of the polyline must be inside or on the edge of the rectangle.
    /// </remarks>
    public bool ContainsShape(Polyline polyline)
    {
        return ContainsRectPoints(TopLeft, BottomRight, polyline);
    }

    /// <summary>
    /// Determines whether the specified polygon is fully contained within this rectangle.
    /// </summary>
    /// <param name="polygon">The polygon to test.</param>
    /// <returns>True if all points of the polygon are inside the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// All points of the polygon must be inside or on the edge of the rectangle.
    /// </remarks>
    public bool ContainsShape(Polygon polygon)
    {
        return ContainsRectPoints(TopLeft, BottomRight, polygon);
    }

    /// <summary>
    /// Determines whether the specified set of points is fully contained within this rectangle.
    /// </summary>
    /// <param name="points">The set of points to test.</param>
    /// <returns>True if all points are inside the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// All points must be inside or on the edge of the rectangle.
    /// </remarks>
    public bool ContainsShape(Points points)
    {
        return ContainsRectPoints(TopLeft, BottomRight, points);
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