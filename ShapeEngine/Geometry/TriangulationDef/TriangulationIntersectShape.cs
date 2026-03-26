using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.TriangulationDef;


public partial class Triangulation
{
    
    //TODO: Update docs
    /// <summary>
    /// Computes intersection points between the triangles and all colliders in the specified <see cref="CollisionObject"/>.
    /// </summary>
    /// <param name="collisionObject">The collision object containing colliders to test for intersection.</param>
    /// <returns>
    /// A <see cref="Dictionary{Collider, IntersectionPoints}"/> mapping each collider to its intersection points,
    /// or null if no colliders are present or no intersections are found.
    /// </returns>
    public void Intersect(Dictionary<Collider, Dictionary<int, IntersectionPoints>> result, CollisionObject collisionObject)
    {
        if (!collisionObject.HasColliders) return;
        result.Clear();

        var buffer = new Dictionary<int, IntersectionPoints>();
        foreach (var collider in collisionObject.Colliders)
        {
            Intersect(buffer, collider);
            if(buffer.Count <= 0) continue;
            result.Add(collider, new Dictionary<int, IntersectionPoints>(buffer));
        }
    }
  
    //TODO: Update docs
    /// <summary>
    /// Checks for intersections between the triangles in this triangulation and the specified collider.
    /// </summary>
    /// <param name="collider">The collider to check for intersections.</param>
    /// <returns>A dictionary mapping the index of each intersecting triangle to the resulting <see cref="IntersectionPoints"/>. Returns <c>null</c> if no intersections are found.</returns>
    /// <remarks>Only triangles with valid intersections are included in the result.</remarks>
    public void Intersect(Dictionary<int, IntersectionPoints> result, Collider collider)
    {
        if (!collider.Enabled) return;
        
        result.Clear();
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            var intersection = tri.Intersect(collider);
            if (intersection != null && intersection.Valid)
            {
                result.Add(i, intersection);
            }
        }
    }

    //TODO: Update docs
    /// <summary>
    /// Checks for intersections between the triangles in this triangulation and the specified line.
    /// </summary>
    /// <param name="shape">The line to check for intersections.</param>
    /// <returns>A dictionary mapping the index of each intersecting triangle to the resulting <see cref="IntersectionPoints"/>. Returns <c>null</c> if no intersections are found.</returns>
    /// <remarks>Only triangles with valid intersections are included in the result.</remarks>
    public void IntersectShape(Dictionary<int, IntersectionPoints> result, Line shape)
    {
        result.Clear();
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            var intersection = tri.IntersectShape(shape);
            if (intersection != null && intersection.Valid)
            {
                result.Add(i, intersection);
            }
        }
    }

    //TODO: Update docs
    /// <summary>
    /// Checks for intersections between the triangles in this triangulation and the specified ray.
    /// </summary>
    /// <param name="shape">The ray to check for intersections.</param>
    /// <returns>A dictionary mapping the index of each intersecting triangle to the resulting <see cref="IntersectionPoints"/>. Returns <c>null</c> if no intersections are found.</returns>
    /// <remarks>Only triangles with valid intersections are included in the result.</remarks>
    public void IntersectShape(Dictionary<int, IntersectionPoints> result, Ray shape)
    {
        result.Clear();
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            var intersection = tri.IntersectShape(shape);
            if (intersection != null && intersection.Valid)
            {
                result.Add(i, intersection);
            }
        }
    }

    //TODO: Update docs
    /// <summary>
    /// Checks for intersections between the triangles in this triangulation and the specified segment.
    /// </summary>
    /// <param name="shape">The segment to check for intersections.</param>
    /// <returns>A dictionary mapping the index of each intersecting triangle to the resulting <see cref="IntersectionPoints"/>. Returns <c>null</c> if no intersections are found.</returns>
    /// <remarks>Only triangles with valid intersections are included in the result.</remarks>
    public void IntersectShape(Dictionary<int, IntersectionPoints> result, Segment shape)
    {
        result.Clear();
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            var intersection = tri.IntersectShape(shape);
            if (intersection != null && intersection.Valid)
            {
                result.Add(i, intersection);
            }
        }
    }

    //TODO: Update docs
    /// <summary>
    /// Checks for intersections between the triangles in this triangulation and the specified circle.
    /// </summary>
    /// <param name="shape">The circle to check for intersections.</param>
    /// <returns>A dictionary mapping the index of each intersecting triangle to the resulting <see cref="IntersectionPoints"/>. Returns <c>null</c> if no intersections are found.</returns>
    /// <remarks>Only triangles with valid intersections are included in the result.</remarks>
    public void IntersectShape(Dictionary<int, IntersectionPoints> result, Circle shape)
    {
        result.Clear();
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            var intersection = tri.IntersectShape(shape);
            if (intersection != null && intersection.Valid)
            {
                result.Add(i, intersection);
            }
        }
    }

    //TODO: Update docs
    /// <summary>
    /// Checks for intersections between the triangles in this triangulation and the specified triangle.
    /// </summary>
    /// <param name="shape">The triangle to check for intersections.</param>
    /// <returns>A dictionary mapping the index of each intersecting triangle to the resulting <see cref="IntersectionPoints"/>. Returns <c>null</c> if no intersections are found.</returns>
    /// <remarks>Only triangles with valid intersections are included in the result.</remarks>
    public void IntersectShape(Dictionary<int, IntersectionPoints> result, Triangle shape)
    {
        result.Clear();
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            var intersection = tri.IntersectShape(shape);
            if (intersection != null && intersection.Valid)
            {
                result.Add(i, intersection);
            }
        }
    }

    //TODO: Update docs
    /// <summary>
    /// Checks for intersections between the triangles in this triangulation and the specified rectangle.
    /// </summary>
    /// <param name="shape">The rectangle to check for intersections.</param>
    /// <returns>A dictionary mapping the index of each intersecting triangle to the resulting <see cref="IntersectionPoints"/>. Returns <c>null</c> if no intersections are found.</returns>
    /// <remarks>Only triangles with valid intersections are included in the result.</remarks>
    public void IntersectShape(Dictionary<int, IntersectionPoints> result, Rect shape)
    {
        result.Clear();
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            var intersection = tri.IntersectShape(shape);
            if (intersection != null && intersection.Valid)
            {
                result.Add(i, intersection);
            }
        }
    }

    //TODO: Update docs
    /// <summary>
    /// Checks for intersections between the triangles in this triangulation and the specified quad.
    /// </summary>
    /// <param name="shape">The quad to check for intersections.</param>
    /// <returns>A dictionary mapping the index of each intersecting triangle to the resulting <see cref="IntersectionPoints"/>. Returns <c>null</c> if no intersections are found.</returns>
    /// <remarks>Only triangles with valid intersections are included in the result.</remarks>
    public void IntersectShape(Dictionary<int, IntersectionPoints> result, Quad shape)
    {
        result.Clear();
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            var intersection = tri.IntersectShape(shape);
            if (intersection != null && intersection.Valid)
            {
                result.Add(i, intersection);
            }
        }
    }

    //TODO: Update docs
    /// <summary>
    /// Checks for intersections between the triangles in this triangulation and the specified polygon.
    /// </summary>
    /// <param name="shape">The polygon to check for intersections.</param>
    /// <returns>A dictionary mapping the index of each intersecting triangle to the resulting <see cref="IntersectionPoints"/>. Returns <c>null</c> if no intersections are found.</returns>
    /// <remarks>Only triangles with valid intersections are included in the result.</remarks>
    public void IntersectShape(Dictionary<int, IntersectionPoints> result, Polygon shape)
    {
        if (shape.Count < 3) return;
        
        result.Clear();
        
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            var intersection = tri.IntersectShape(shape);
            if (intersection != null && intersection.Valid)
            {
                result.Add(i, intersection);
            }
        }
    }

    //TODO: Update docs
    /// <summary>
    /// Checks for intersections between the triangles in this triangulation and the specified polyline.
    /// </summary>
    /// <param name="shape">The polyline to check for intersections.</param>
    /// <returns>A dictionary mapping the index of each intersecting triangle to the resulting <see cref="IntersectionPoints"/>. Returns <c>null</c> if no intersections are found.</returns>
    /// <remarks>Only triangles with valid intersections are included in the result.</remarks>
    public void IntersectShape(Dictionary<int, IntersectionPoints> result, Polyline shape)
    {
        if (shape.Count < 2) return;
        
        result.Clear();
        
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            var intersection = tri.IntersectShape(shape);
            if (intersection != null && intersection.Valid)
            {
                result.Add(i, intersection);
            }
        }
    }

    //TODO: Update docs
    /// <summary>
    /// Checks for intersections between the triangles in this triangulation and the specified segments.
    /// </summary>
    /// <param name="shape">The segments to check for intersections.</param>
    /// <returns>A dictionary mapping the index of each intersecting triangle to the resulting <see cref="IntersectionPoints"/>. Returns <c>null</c> if no intersections are found.</returns>
    /// <remarks>Only triangles with valid intersections are included in the result.</remarks>
    public void IntersectShape(Dictionary<int, IntersectionPoints> result, Segments shape)
    {
        result.Clear();
        
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            var intersection = tri.IntersectShape(shape);
            if (intersection != null && intersection.Valid)
            {
                result.Add(i, intersection);
            }
        }
    }

    //TODO: Update docs
    /// <summary>
    /// Checks for intersections between the triangles in this triangulation and the specified shape implementing <see cref="IShape"/>.
    /// </summary>
    /// <param name="shape">The shape to check for intersections.</param>
    /// <returns>
    /// A dictionary mapping the index of each intersecting triangle to the resulting <see cref="IntersectionPoints"/>.
    /// Returns <c>null</c> if no intersections are found or the shape type is not supported.
    /// </returns>
    public void IntersectShape(Dictionary<int, IntersectionPoints> result, IShape shape)
    {
        switch (shape.GetShapeType())
        {
            case ShapeType.Circle:
                IntersectShape(result, shape.GetCircleShape());
                break;
            case ShapeType.Segment:
                IntersectShape(result, shape.GetSegmentShape());
                break;
            case ShapeType.Ray:
                IntersectShape(result, shape.GetRayShape());
                break;
            case ShapeType.Line:
                IntersectShape(result, shape.GetLineShape());
                break;
            case ShapeType.Triangle:
                IntersectShape(result, shape.GetTriangleShape());
                break;
            case ShapeType.Rect:
                IntersectShape(result, shape.GetRectShape());
                break;
            case ShapeType.Quad:
                IntersectShape(result, shape.GetQuadShape());
                break;
            case ShapeType.Poly:
                IntersectShape(result, shape.GetPolygonShape());
                break;
            case ShapeType.PolyLine:
                IntersectShape(result, shape.GetPolylineShape());
                break;
        }
    }

}