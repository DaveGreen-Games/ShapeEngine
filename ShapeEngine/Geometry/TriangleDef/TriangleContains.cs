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
    /// <summary>
    /// Determines whether the triangle contains the specified point.
    /// </summary>
    /// <param name="p">The point to test for containment.</param>
    /// <returns>True if the point is inside or on the boundary of the triangle; otherwise, false.</returns>
    /// <remarks>
    /// This method uses the cross product method to determine point containment by checking
    /// that the point is on the same side of all three triangle edges. This assumes the triangle
    /// vertices are in counter-clockwise order for proper containment testing.
    /// Points on the triangle's edges are considered to be contained within the triangle.
    /// </remarks>
    public bool ContainsPoint(Vector2 p) => ContainsTrianglePoint(A, B, C, p);
    
    /// <summary>
    /// Determines whether the triangle contains both of the specified points.
    /// </summary>
    /// <param name="u">The first point to test for containment.</param>
    /// <param name="v">The second point to test for containment.</param>
    /// <returns>True if both points are inside or on the boundary of the triangle; otherwise, false.</returns>
    /// <remarks>This method returns true only if all specified points are contained within the triangle.</remarks>
    public bool ContainsPoints(Vector2 u, Vector2 v) => ContainsTrianglePoints(A, B, C, u, v);
    
    /// <summary>
    /// Determines whether the triangle contains all three of the specified points.
    /// </summary>
    /// <param name="u">The first point to test for containment.</param>
    /// <param name="v">The second point to test for containment.</param>
    /// <param name="w">The third point to test for containment.</param>
    /// <returns>True if all three points are inside or on the boundary of the triangle; otherwise, false.</returns>
    /// <remarks>This method returns true only if all specified points are contained within the triangle.</remarks>
    public bool ContainsPoints(Vector2 u, Vector2 v, Vector2 w) => ContainsTrianglePoints(A, B, C, u, v, w);
    
    /// <summary>
    /// Determines whether the triangle contains all four of the specified points.
    /// </summary>
    /// <param name="u">The first point to test for containment.</param>
    /// <param name="v">The second point to test for containment.</param>
    /// <param name="w">The third point to test for containment.</param>
    /// <param name="x">The fourth point to test for containment.</param>
    /// <returns>True if all four points are inside or on the boundary of the triangle; otherwise, false.</returns>
    /// <remarks>This method returns true only if all specified points are contained within the triangle.</remarks>
    public bool ContainsPoints(Vector2 u, Vector2 v, Vector2 w, Vector2 x) => ContainsTrianglePoints(A, B, C, u, v, w, x);
    
    /// <summary>
    /// Determines whether the triangle contains all points in the specified collection.
    /// </summary>
    /// <param name="points">The collection of points to test for containment.</param>
    /// <returns>True if all points in the collection are inside or on the boundary of the triangle; otherwise, false.</returns>
    /// <remarks>
    /// This method returns true only if all points in the collection are contained within the triangle.
    /// If the collection is empty, this method returns true.
    /// </remarks>
    public bool ContainsPoints(List<Vector2> points) => ContainsTrianglePoints(A, B, C, points);

    /// <summary>
    /// Determines whether the triangle completely contains the specified collision object.
    /// </summary>
    /// <param name="collisionObject">The collision object to test for containment.</param>
    /// <returns>True if all colliders of the collision object are completely contained within the triangle; otherwise, false.</returns>
    /// <remarks>
    /// This method checks all colliders attached to the collision object and returns true only if
    /// every collider is completely contained within the triangle. If the collision object has no colliders,
    /// this method returns false.
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
    /// Determines whether the triangle completely contains the specified collider.
    /// </summary>
    /// <param name="collider">The collider to test for containment.</param>
    /// <returns>True if the collider is completely contained within the triangle; otherwise, false.</returns>
    /// <remarks>
    /// This method determines the shape type of the collider and delegates to the appropriate
    /// shape-specific containment method. The entire collider must be within the triangle for this to return true.
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
    /// Determines whether the triangle completely contains the specified line segment.
    /// </summary>
    /// <param name="segment">The segment to test for containment.</param>
    /// <returns>True if both endpoints of the segment are contained within the triangle; otherwise, false.</returns>
    /// <remarks>
    /// This method checks if both the start and end points of the segment are within the triangle.
    /// Note that this does not guarantee that the entire line segment is within the triangle if the triangle is concave,
    /// but since triangles are always convex, containment of both endpoints ensures containment of the entire segment.
    /// </remarks>
    public bool ContainsShape(Segment segment)
    {
        return ContainsTrianglePoints(A, B, C, segment.Start, segment.End);
    }

    /// <summary>
    /// Determines whether the triangle completely contains the specified circle.
    /// </summary>
    /// <param name="circle">The circle to test for containment.</param>
    /// <returns>True if the entire circle is contained within the triangle; otherwise, false.</returns>
    /// <remarks>
    /// This method checks if the circle's center is sufficiently far from all triangle edges
    /// such that the entire circle circumference is within the triangle boundaries.
    /// </remarks>
    public bool ContainsShape(Circle circle)
    {
        return ContainsTriangleCircle(A, B, C, circle.Center, circle.Radius);
    }

    /// <summary>
    /// Determines whether the triangle completely contains the specified rectangle.
    /// </summary>
    /// <param name="rect">The rectangle to test for containment.</param>
    /// <returns>True if all four corners of the rectangle are contained within the triangle; otherwise, false.</returns>
    /// <remarks>
    /// This method checks containment by verifying that all four vertices of the rectangle are within the triangle.
    /// Since triangles are convex shapes, containment of all vertices ensures containment of the entire rectangle.
    /// </remarks>
    public bool ContainsShape(Rect rect)
    {
        return ContainsTrianglePoints(A, B, C, rect.A, rect.B, rect.C, rect.D);
    }

    /// <summary>
    /// Determines whether this triangle completely contains another triangle.
    /// </summary>
    /// <param name="triangle">The triangle to test for containment.</param>
    /// <returns>True if all three vertices of the other triangle are contained within this triangle; otherwise, false.</returns>
    /// <remarks>
    /// This method checks containment by verifying that all three vertices of the other triangle are within this triangle.
    /// Since triangles are convex shapes, containment of all vertices ensures containment of the entire triangle.
    /// </remarks>
    public bool ContainsShape(Triangle triangle)
    {
        return ContainsTrianglePoints(A, B, C, triangle.A, triangle.B, triangle.C);
    }

    /// <summary>
    /// Determines whether the triangle completely contains the specified quadrilateral.
    /// </summary>
    /// <param name="quad">The quadrilateral to test for containment.</param>
    /// <returns>True if all four vertices of the quadrilateral are contained within the triangle; otherwise, false.</returns>
    /// <remarks>
    /// This method checks containment by verifying that all four vertices of the quadrilateral are within the triangle.
    /// Since triangles are convex shapes, containment of all vertices ensures containment of the entire quadrilateral.
    /// </remarks>
    public bool ContainsShape(Quad quad)
    {
        return ContainsTrianglePoints(A, B, C, quad.A, quad.B, quad.C, quad.D);
    }

    /// <summary>
    /// Determines whether the triangle completely contains the specified polyline.
    /// </summary>
    /// <param name="polyline">The polyline to test for containment.</param>
    /// <returns>True if all vertices of the polyline are contained within the triangle; otherwise, false.</returns>
    /// <remarks>
    /// This method checks containment by verifying that all vertices of the polyline are within the triangle.
    /// Since triangles are convex shapes, containment of all vertices ensures containment of the entire polyline.
    /// </remarks>
    public bool ContainsShape(Polyline polyline)
    {
        return ContainsTrianglePoints(A, B, C, polyline);
    }

    /// <summary>
    /// Determines whether the triangle completely contains the specified polygon.
    /// </summary>
    /// <param name="polygon">The polygon to test for containment.</param>
    /// <returns>True if all vertices of the polygon are contained within the triangle; otherwise, false.</returns>
    /// <remarks>
    /// This method checks containment by verifying that all vertices of the polygon are within the triangle.
    /// Since triangles are convex shapes, containment of all vertices ensures containment of the entire polygon.
    /// </remarks>
    public bool ContainsShape(Polygon polygon)
    {
        return polygon.Count >= 3 && ContainsTrianglePoints(A, B, C, polygon);
    }

    /// <summary>
    /// Determines whether the triangle contains all points in the specified points collection.
    /// </summary>
    /// <param name="points">The points collection to test for containment.</param>
    /// <returns>True if all points in the collection are contained within the triangle; otherwise, false.</returns>
    /// <remarks>
    /// This method checks containment by verifying that every point in the collection is within the triangle.
    /// If the points collection is empty, this method returns true.
    /// </remarks>
    public bool ContainsShape(Points points)
    {
        return ContainsTrianglePoints(A, B, C, points);
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