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
    /// Checks for intersections between the triangles in this triangulation and the specified collider.
    /// </summary>
    /// <param name="collider">The collider to check for intersections.</param>
    /// <returns>A dictionary mapping the index of each intersecting triangle to the resulting <see cref="CollisionPoints"/>. Returns <c>null</c> if no intersections are found.</returns>
    /// <remarks>Only triangles with valid intersections are included in the result.</remarks>
    public Dictionary<int, CollisionPoints>? Intersect(Collider collider)
    {
        Dictionary<int, CollisionPoints>? result = null;
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            var intersection = tri.Intersect(collider);
            if (intersection != null && intersection.Valid)
            {
                result ??= new();
                result.Add(i, intersection);
            }
        }

        return result;
    }

    /// <summary>
    /// Checks for intersections between the triangles in this triangulation and the specified line.
    /// </summary>
    /// <param name="shape">The line to check for intersections.</param>
    /// <returns>A dictionary mapping the index of each intersecting triangle to the resulting <see cref="CollisionPoints"/>. Returns <c>null</c> if no intersections are found.</returns>
    /// <remarks>Only triangles with valid intersections are included in the result.</remarks>
    public Dictionary<int, CollisionPoints>? IntersectShape(Line shape)
    {
        Dictionary<int, CollisionPoints>? result = null;
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            var intersection = tri.IntersectShape(shape);
            if (intersection != null && intersection.Valid)
            {
                result ??= new();
                result.Add(i, intersection);
            }
        }

        return result;
    }

    /// <summary>
    /// Checks for intersections between the triangles in this triangulation and the specified ray.
    /// </summary>
    /// <param name="shape">The ray to check for intersections.</param>
    /// <returns>A dictionary mapping the index of each intersecting triangle to the resulting <see cref="CollisionPoints"/>. Returns <c>null</c> if no intersections are found.</returns>
    /// <remarks>Only triangles with valid intersections are included in the result.</remarks>
    public Dictionary<int, CollisionPoints>? IntersectShape(Ray shape)
    {
        Dictionary<int, CollisionPoints>? result = null;
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            var intersection = tri.IntersectShape(shape);
            if (intersection != null && intersection.Valid)
            {
                result ??= new();
                result.Add(i, intersection);
            }
        }

        return result;
    }

    /// <summary>
    /// Checks for intersections between the triangles in this triangulation and the specified segment.
    /// </summary>
    /// <param name="shape">The segment to check for intersections.</param>
    /// <returns>A dictionary mapping the index of each intersecting triangle to the resulting <see cref="CollisionPoints"/>. Returns <c>null</c> if no intersections are found.</returns>
    /// <remarks>Only triangles with valid intersections are included in the result.</remarks>
    public Dictionary<int, CollisionPoints>? IntersectShape(Segment shape)
    {
        Dictionary<int, CollisionPoints>? result = null;
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            var intersection = tri.IntersectShape(shape);
            if (intersection != null && intersection.Valid)
            {
                result ??= new();
                result.Add(i, intersection);
            }
        }

        return result;
    }

    /// <summary>
    /// Checks for intersections between the triangles in this triangulation and the specified circle.
    /// </summary>
    /// <param name="shape">The circle to check for intersections.</param>
    /// <returns>A dictionary mapping the index of each intersecting triangle to the resulting <see cref="CollisionPoints"/>. Returns <c>null</c> if no intersections are found.</returns>
    /// <remarks>Only triangles with valid intersections are included in the result.</remarks>
    public Dictionary<int, CollisionPoints>? IntersectShape(Circle shape)
    {
        Dictionary<int, CollisionPoints>? result = null;
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            var intersection = tri.IntersectShape(shape);
            if (intersection != null && intersection.Valid)
            {
                result ??= new();
                result.Add(i, intersection);
            }
        }

        return result;
    }

    /// <summary>
    /// Checks for intersections between the triangles in this triangulation and the specified triangle.
    /// </summary>
    /// <param name="shape">The triangle to check for intersections.</param>
    /// <returns>A dictionary mapping the index of each intersecting triangle to the resulting <see cref="CollisionPoints"/>. Returns <c>null</c> if no intersections are found.</returns>
    /// <remarks>Only triangles with valid intersections are included in the result.</remarks>
    public Dictionary<int, CollisionPoints>? IntersectShape(Triangle shape)
    {
        Dictionary<int, CollisionPoints>? result = null;
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            var intersection = tri.IntersectShape(shape);
            if (intersection != null && intersection.Valid)
            {
                result ??= new();
                result.Add(i, intersection);
            }
        }

        return result;
    }

    /// <summary>
    /// Checks for intersections between the triangles in this triangulation and the specified rectangle.
    /// </summary>
    /// <param name="shape">The rectangle to check for intersections.</param>
    /// <returns>A dictionary mapping the index of each intersecting triangle to the resulting <see cref="CollisionPoints"/>. Returns <c>null</c> if no intersections are found.</returns>
    /// <remarks>Only triangles with valid intersections are included in the result.</remarks>
    public Dictionary<int, CollisionPoints>? IntersectShape(Rect shape)
    {
        Dictionary<int, CollisionPoints>? result = null;
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            var intersection = tri.IntersectShape(shape);
            if (intersection != null && intersection.Valid)
            {
                result ??= new();
                result.Add(i, intersection);
            }
        }

        return result;
    }

    /// <summary>
    /// Checks for intersections between the triangles in this triangulation and the specified quad.
    /// </summary>
    /// <param name="shape">The quad to check for intersections.</param>
    /// <returns>A dictionary mapping the index of each intersecting triangle to the resulting <see cref="CollisionPoints"/>. Returns <c>null</c> if no intersections are found.</returns>
    /// <remarks>Only triangles with valid intersections are included in the result.</remarks>
    public Dictionary<int, CollisionPoints>? IntersectShape(Quad shape)
    {
        Dictionary<int, CollisionPoints>? result = null;
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            var intersection = tri.IntersectShape(shape);
            if (intersection != null && intersection.Valid)
            {
                result ??= new();
                result.Add(i, intersection);
            }
        }

        return result;
    }

    /// <summary>
    /// Checks for intersections between the triangles in this triangulation and the specified polygon.
    /// </summary>
    /// <param name="shape">The polygon to check for intersections.</param>
    /// <returns>A dictionary mapping the index of each intersecting triangle to the resulting <see cref="CollisionPoints"/>. Returns <c>null</c> if no intersections are found.</returns>
    /// <remarks>Only triangles with valid intersections are included in the result.</remarks>
    public Dictionary<int, CollisionPoints>? IntersectShape(Polygon shape)
    {
        Dictionary<int, CollisionPoints>? result = null;
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            var intersection = tri.IntersectShape(shape);
            if (intersection != null && intersection.Valid)
            {
                result ??= new();
                result.Add(i, intersection);
            }
        }

        return result;
    }

    /// <summary>
    /// Checks for intersections between the triangles in this triangulation and the specified polyline.
    /// </summary>
    /// <param name="shape">The polyline to check for intersections.</param>
    /// <returns>A dictionary mapping the index of each intersecting triangle to the resulting <see cref="CollisionPoints"/>. Returns <c>null</c> if no intersections are found.</returns>
    /// <remarks>Only triangles with valid intersections are included in the result.</remarks>
    public Dictionary<int, CollisionPoints>? IntersectShape(Polyline shape)
    {
        Dictionary<int, CollisionPoints>? result = null;
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            var intersection = tri.IntersectShape(shape);
            if (intersection != null && intersection.Valid)
            {
                result ??= new();
                result.Add(i, intersection);
            }
        }

        return result;
    }

    /// <summary>
    /// Checks for intersections between the triangles in this triangulation and the specified segments.
    /// </summary>
    /// <param name="shape">The segments to check for intersections.</param>
    /// <returns>A dictionary mapping the index of each intersecting triangle to the resulting <see cref="CollisionPoints"/>. Returns <c>null</c> if no intersections are found.</returns>
    /// <remarks>Only triangles with valid intersections are included in the result.</remarks>
    public Dictionary<int, CollisionPoints>? IntersectShape(Segments shape)
    {
        Dictionary<int, CollisionPoints>? result = null;
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            var intersection = tri.IntersectShape(shape);
            if (intersection != null && intersection.Valid)
            {
                result ??= new();
                result.Add(i, intersection);
            }
        }

        return result;
    }

}