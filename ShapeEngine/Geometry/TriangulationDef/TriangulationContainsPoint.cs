using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.TriangulationDef;


public partial class Triangulation
{
    /// <summary>
    /// Determines whether the specified point is contained within any triangle in the triangulation.
    /// </summary>
    /// <param name="p">The point to check for containment.</param>
    /// <returns><c>true</c> if the point is contained in any triangle; otherwise, <c>false</c>.</returns>
    /// <remarks>Returns as soon as a containing triangle is found.</remarks>
    public bool ContainsPoint(Vector2 p)
    {
        foreach (var tri in this)
        {
            if (tri.ContainsPoint(p)) return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the specified point is contained within any triangle in the triangulation and returns the index of the containing triangle.
    /// </summary>
    /// <param name="p">The point to check for containment.</param>
    /// <param name="triangleIndex">The index of the triangle that contains the point, or -1 if not found.</param>
    /// <returns><c>true</c> if the point is contained in any triangle; otherwise, <c>false</c>.</returns>
    /// <remarks>Returns as soon as a containing triangle is found. The index is set to -1 if the point is not contained in any triangle.</remarks>
    public bool ContainsPoint(Vector2 p, out int triangleIndex)
    {
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            if (tri.ContainsPoint(p))
            {
                triangleIndex = i;
                return true;
            }
        }

        triangleIndex = -1;
        return false;
    }
    
    /// <summary>
    /// Checks if any collider of a collision object is contained within any triangle of this triangulation.
    /// </summary>
    /// <param name="collisionObject">The collision object to check.</param>
    /// <param name="triangleIndex">The index of the triangle that contains a collider of the collision object.</param>
    /// <returns>True if any collider is contained within a triangle, false otherwise.</returns>
    /// <remarks>Stops at the first collider that is contained in a triangle.</remarks>
    public bool ContainsCollisionObject(CollisionObject collisionObject, out int triangleIndex)
    {
        triangleIndex = -1;
        if (!collisionObject.HasColliders) return false;
        foreach (var collider in collisionObject.Colliders)
        {
            if (ContainsCollider(collider, out int index))
            {
                triangleIndex = index;
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// Checks if a collider is contained within any triangle of this triangulation.
    /// </summary>
    /// <param name="collider">The collider to check.</param>
    /// <param name="triangleIndex">The index of the triangle that contains the collider.</param>
    /// <returns>True if the collider is contained within a triangle, false otherwise.</returns>
    /// <remarks>The collider is checked based on its shape type.</remarks>
    public bool ContainsCollider(Collider collider, out int triangleIndex)
    {
        triangleIndex = -1;
        if (!collider.Enabled) return false;
        
        switch (collider.GetShapeType())
        {
            case ShapeType.Circle: return ContainsShape(collider.GetCircleShape(), out triangleIndex);
            case ShapeType.Segment: return ContainsShape(collider.GetSegmentShape(), out triangleIndex);
            case ShapeType.Triangle: return ContainsShape(collider.GetTriangleShape(), out triangleIndex);
            case ShapeType.Quad: return ContainsShape(collider.GetQuadShape(), out triangleIndex);
            case ShapeType.Rect: return ContainsShape(collider.GetRectShape(), out triangleIndex);
            case ShapeType.Poly: return ContainsShape(collider.GetPolygonShape(), out triangleIndex);
            case ShapeType.PolyLine: return ContainsShape(collider.GetPolylineShape(), out triangleIndex);
        }
        return false;
    }
    /// <summary>
    /// Checks if a segment is contained within any triangle of this triangulation.
    /// </summary>
    /// <param name="segment">The segment to check.</param>
    /// <param name="triangleIndex">The index of the triangle that contains the segment.</param>
    /// <returns>True if the segment is contained in a triangle, false otherwise.</returns>
    /// <remarks>A segment is contained if both its start and end points are inside a triangle.</remarks>
    public bool ContainsShape(Segment segment, out int triangleIndex)
    {
        triangleIndex = -1;
        
        for (int i = 0; i < Count; i++)
        {
            var triangle = this[i];
            if (triangle.ContainsPoints(segment.Start, segment.End))
            {
                triangleIndex = i;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if a circle is contained within any triangle of this triangulation.
    /// </summary>
    /// <param name="circle">The circle to check.</param>
    /// <param name="triangleIndex">The index of the triangle that contains the circle.</param>
    /// <returns>True if the circle is contained in a triangle, false otherwise.</returns>
    /// <remarks>Stops at the first triangle that contains the circle.</remarks>
    public bool ContainsShape(Circle circle, out int triangleIndex)
    {
        triangleIndex = -1;
        
        for (int i = 0; i < Count; i++)
        {
            var triangle = this[i];
            if (triangle.ContainsShape(circle))
            {
                triangleIndex = i;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if a rectangle is contained within any triangle of this triangulation.
    /// </summary>
    /// <param name="rect">The rectangle to check.</param>
    /// <param name="triangleIndex">The index of the triangle that contains the rectangle.</param>
    /// <returns>True if the rectangle is contained in a triangle, false otherwise.</returns>
    /// <remarks>Stops at the first triangle that contains the rectangle.</remarks>
    public bool ContainsShape(Rect rect, out int triangleIndex)
    {
        triangleIndex = -1;
        
        for (int i = 0; i < Count; i++)
        {
            var triangle = this[i];
            if (triangle.ContainsShape(rect))
            {
                triangleIndex = i;
                return true;
            }
        }

        return false;
    }
    /// <summary>
    /// Checks if a triangle is contained within any triangle of this triangulation.
    /// </summary>
    /// <param name="triangle">The triangle to check.</param>
    /// <param name="triangleIndex">The index of the triangle that contains the given triangle.</param>
    /// <returns>True if the triangle is contained in a triangle, false otherwise.</returns>
    /// <remarks>Stops at the first triangle that contains the given triangle.</remarks>
    public bool ContainsShape(Triangle triangle, out int triangleIndex)
    {
        triangleIndex = -1;
        
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            if (tri.ContainsShape(triangle))
            {
                triangleIndex = i;
                return true;
            }
        }

        return false;
    }
    /// <summary>
    /// Checks if a quad is contained within any triangle of this triangulation.
    /// </summary>
    /// <param name="quad">The quad to check.</param>
    /// <param name="triangleIndex">The index of the triangle that contains the quad.</param>
    /// <returns>True if the quad is contained in a triangle, false otherwise.</returns>
    /// <remarks>Stops at the first triangle that contains the quad.</remarks>
    public bool ContainsShape(Quad quad, out int triangleIndex)
    {
        triangleIndex = -1;
        
        for (int i = 0; i < Count; i++)
        {
            var triangle = this[i];
            if (triangle.ContainsShape(quad))
            {
                triangleIndex = i;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if a polyline is contained within any triangle of this triangulation.
    /// </summary>
    /// <param name="polyline">The polyline to check.</param>
    /// <param name="triangleIndex">The index of the triangle that contains the polyline.</param>
    /// <returns>True if the polyline is contained in a triangle, false otherwise.</returns>
    /// <remarks>Stops at the first triangle that contains the polyline.</remarks>
    public bool ContainsShape(Polyline polyline, out int triangleIndex)
    {
        triangleIndex = -1;
        
        for (int i = 0; i < Count; i++)
        {
            var triangle = this[i];
            if (triangle.ContainsShape(polyline))
            {
                triangleIndex = i;
                return true;
            }
        }

        return false;
    }
    /// <summary>
    /// Checks if a polygon is contained within any triangle of this triangulation.
    /// </summary>
    /// <param name="polygon">The polygon to check.</param>
    /// <param name="triangleIndex">The index of the triangle that contains the polygon.</param>
    /// <returns>True if the polygon is contained in a triangle, false otherwise.</returns>
    /// <remarks>Stops at the first triangle that contains the polygon.</remarks>
    public bool ContainsShape(Polygon polygon, out int triangleIndex)
    {
        triangleIndex = -1;
        
        for (int i = 0; i < Count; i++)
        {
            var triangle = this[i];
            if (triangle.ContainsShape(polygon))
            {
                triangleIndex = i;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if a collection of points is contained within any triangle of this triangulation.
    /// </summary>
    /// <param name="points">The points to check.</param>
    /// <param name="triangleIndex">The index of the triangle that contains the points.</param>
    /// <returns>True if the points are contained in a triangle, false otherwise.</returns>
    /// <remarks>Stops at the first triangle that contains the points.</remarks>
    public bool ContainsShape(Points points, out int triangleIndex)
    {
        triangleIndex = -1;
        
        for (int i = 0; i < Count; i++)
        {
            var triangle = this[i];
            if (triangle.ContainsShape(points))
            {
                triangleIndex = i;
                return true;
            }
        }

        return false;
    }
    
    /// <summary>
    /// Determines whether the specified shape is contained within any triangle of this triangulation.
    /// </summary>
    /// <param name="shape">The shape to check for containment.</param>
    /// <param name="triangleIndex">The index of the triangle that contains the shape, or -1 if not found.</param>
    /// <returns>True if the shape is contained in a triangle, false otherwise.</returns>
    public bool ContainsShape(IShape shape, out int triangleIndex)
    {
        triangleIndex = -1;
        
        return shape.GetShapeType() switch
        {
            ShapeType.Circle => ContainsShape(shape.GetCircleShape(), out triangleIndex),
            ShapeType.Segment => ContainsShape(shape.GetSegmentShape(), out triangleIndex),
            ShapeType.Triangle => ContainsShape(shape.GetTriangleShape(), out triangleIndex),
            ShapeType.Rect => ContainsShape(shape.GetRectShape(), out triangleIndex),
            ShapeType.Quad => ContainsShape(shape.GetQuadShape(), out triangleIndex),
            ShapeType.Poly => ContainsShape(shape.GetPolygonShape(), out triangleIndex),
            ShapeType.PolyLine => ContainsShape(shape.GetPolylineShape(), out triangleIndex),
            _ => false
        };
    }
}