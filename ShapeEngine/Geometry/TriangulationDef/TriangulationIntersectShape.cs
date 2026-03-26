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
    
    /// <summary>
    /// Computes intersections between this triangulation and all colliders in the specified <see cref="CollisionObject"/>, writing the results into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination dictionary that will be cleared and populated with one entry per intersecting collider.</param>
    /// <param name="collisionObject">The collision object containing colliders to test for intersection.</param>
    /// <remarks>
    /// If <paramref name="collisionObject"/> has no colliders, the method returns immediately without modifying <paramref name="result"/>. Only colliders that produce at least one valid triangle intersection are added.
    /// </remarks>
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
  
    /// <summary>
    /// Computes intersections between the triangles in this triangulation and the specified <paramref name="collider"/>, writing them into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination dictionary that will be cleared and populated with one entry per intersecting triangle index.</param>
    /// <param name="collider">The collider to check for intersections.</param>
    /// <remarks>If <paramref name="collider"/> is not enabled, the method returns immediately without modifying <paramref name="result"/>. Only triangles with valid intersections are included.</remarks>
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

    /// <summary>
    /// Computes intersections between the triangles in this triangulation and the specified line, writing them into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination dictionary that will be cleared and populated with one entry per intersecting triangle index.</param>
    /// <param name="shape">The line to check for intersections.</param>
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

    /// <summary>
    /// Computes intersections between the triangles in this triangulation and the specified ray, writing them into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination dictionary that will be cleared and populated with one entry per intersecting triangle index.</param>
    /// <param name="shape">The ray to check for intersections.</param>
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

    /// <summary>
    /// Computes intersections between the triangles in this triangulation and the specified segment, writing them into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination dictionary that will be cleared and populated with one entry per intersecting triangle index.</param>
    /// <param name="shape">The segment to check for intersections.</param>
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

    /// <summary>
    /// Computes intersections between the triangles in this triangulation and the specified circle, writing them into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination dictionary that will be cleared and populated with one entry per intersecting triangle index.</param>
    /// <param name="shape">The circle to check for intersections.</param>
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

    /// <summary>
    /// Computes intersections between the triangles in this triangulation and the specified triangle, writing them into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination dictionary that will be cleared and populated with one entry per intersecting triangle index.</param>
    /// <param name="shape">The triangle to check for intersections.</param>
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

    /// <summary>
    /// Computes intersections between the triangles in this triangulation and the specified rectangle, writing them into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination dictionary that will be cleared and populated with one entry per intersecting triangle index.</param>
    /// <param name="shape">The rectangle to check for intersections.</param>
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

    /// <summary>
    /// Computes intersections between the triangles in this triangulation and the specified quad, writing them into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination dictionary that will be cleared and populated with one entry per intersecting triangle index.</param>
    /// <param name="shape">The quad to check for intersections.</param>
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

    /// <summary>
    /// Computes intersections between the triangles in this triangulation and the specified polygon, writing them into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination dictionary that will be cleared and populated with one entry per intersecting triangle index.</param>
    /// <param name="shape">The polygon to check for intersections.</param>
    /// <remarks>If <paramref name="shape"/> contains fewer than three points, the method returns immediately without modifying <paramref name="result"/>. Otherwise, only triangles with valid intersections are included.</remarks>
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

    /// <summary>
    /// Computes intersections between the triangles in this triangulation and the specified polyline, writing them into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination dictionary that will be cleared and populated with one entry per intersecting triangle index.</param>
    /// <param name="shape">The polyline to check for intersections.</param>
    /// <remarks>If <paramref name="shape"/> contains fewer than two points, the method returns immediately without modifying <paramref name="result"/>. Otherwise, only triangles with valid intersections are included.</remarks>
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

    /// <summary>
    /// Computes intersections between the triangles in this triangulation and the specified segments collection, writing them into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination dictionary that will be cleared and populated with one entry per intersecting triangle index.</param>
    /// <param name="shape">The segments to check for intersections.</param>
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

    /// <summary>
    /// Computes intersections between the triangles in this triangulation and the specified <see cref="IShape"/>, writing them into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination dictionary that will be populated by the shape-specific intersection routine.</param>
    /// <param name="shape">The shape to check for intersections.</param>
    /// <remarks>
    /// This method dispatches to the corresponding <c>IntersectShape</c> overload based on <paramref name="shape"/>'s runtime shape type. If the shape type is not handled by the switch, <paramref name="result"/> is left unchanged by this method.
    /// </remarks>
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