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
    /// Finds all triangles that intersect the specified collider.
    /// </summary>
    /// <param name="collider">The collider to check the triangles against.</param>
    /// <returns>A dictionary where the key is the index of the triangle and the value is the found collision points, or <c>null</c> if no intersections are found.</returns>
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
    /// Finds all triangles that intersect the specified line shape.
    /// </summary>
    /// <param name="shape">The line to check the triangles against.</param>
    /// <returns>A dictionary where the key is the index of the triangle and the value is the found collision points, or <c>null</c> if no intersections are found.</returns>
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
    /// Finds all triangles that intersect the specified ray shape.
    /// </summary>
    /// <param name="shape">The ray to check the triangles against.</param>
    /// <returns>A dictionary where the key is the index of the triangle and the value is the found collision points, or <c>null</c> if no intersections are found.</returns>
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
    /// Finds all triangles that intersect the specified segment shape.
    /// </summary>
    /// <param name="shape">The segment to check the triangles against.</param>
    /// <returns>A dictionary where the key is the index of the triangle and the value is the found collision points, or <c>null</c> if no intersections are found.</returns>
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
    /// Finds all triangles that intersect the specified circle shape.
    /// </summary>
    /// <param name="shape">The circle to check the triangles against.</param>
    /// <returns>A dictionary where the key is the index of the triangle and the value is the found collision points, or <c>null</c> if no intersections are found.</returns>
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
    /// Finds all triangles that intersect the specified triangle shape.
    /// </summary>
    /// <param name="shape">The triangle to check the triangles against.</param>
    /// <returns>A dictionary where the key is the index of the triangle and the value is the found collision points, or <c>null</c> if no intersections are found.</returns>
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
    /// Finds all triangles that intersect the specified rectangle shape.
    /// </summary>
    /// <param name="shape">The rectangle to check the triangles against.</param>
    /// <returns>A dictionary where the key is the index of the triangle and the value is the found collision points, or <c>null</c> if no intersections are found.</returns>
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
    /// Finds all triangles that intersect the specified quadrilateral shape.
    /// </summary>
    /// <param name="shape">The quadrilateral to check the triangles against.</param>
    /// <returns>A dictionary where the key is the index of the triangle and the value is the found collision points, or <c>null</c> if no intersections are found.</returns>
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
    /// Finds all triangles that intersect the specified polygon shape.
    /// </summary>
    /// <param name="shape">The polygon to check the triangles against.</param>
    /// <returns>A dictionary where the key is the index of the triangle and the value is the found collision points, or <c>null</c> if no intersections are found.</returns>
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
    /// Finds all triangles that intersect the specified polyline shape.
    /// </summary>
    /// <param name="shape">The polyline to check the triangles against.</param>
    /// <returns>A dictionary where the key is the index of the triangle and the value is the found collision points, or <c>null</c> if no intersections are found.</returns>
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
    /// Finds all triangles that intersect the specified segments shape.
    /// </summary>
    /// <param name="shape">The segments to check the triangles against.</param>
    /// <returns>A dictionary where the key is the index of the triangle and the value is the found collision points, or <c>null</c> if no intersections are found.</returns>
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