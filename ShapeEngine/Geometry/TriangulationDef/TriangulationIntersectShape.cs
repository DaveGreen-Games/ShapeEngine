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
    /// Find all triangles that intersect the specified collider.
    /// </summary>
    /// <param name="collider">The collider to check the triangles against.</param>
    /// <returns>Returns a dictionary where the key represents the index of the triangle and the value represents the found collision points.</returns>
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
    /// Find all triangles that intersect the specified shape.
    /// </summary>
    /// <param name="shape">The shape to check the triangles against.</param>
    /// <returns>Returns a dictionary where the key represents the index of the triangle and the value represents the found collision points.</returns>
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
    /// Find all triangles that intersect the specified shape.
    /// </summary>
    /// <param name="shape">The shape to check the triangles against.</param>
    /// <returns>Returns a dictionary where the key represents the index of the triangle and the value represents the found collision points.</returns>
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
    /// Find all triangles that intersect the specified shape.
    /// </summary>
    /// <param name="shape">The shape to check the triangles against.</param>
    /// <returns>Returns a dictionary where the key represents the index of the triangle and the value represents the found collision points.</returns>
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
    /// Find all triangles that intersect the specified shape.
    /// </summary>
    /// <param name="shape">The shape to check the triangles against.</param>
    /// <returns>Returns a dictionary where the key represents the index of the triangle and the value represents the found collision points.</returns>
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
    /// Find all triangles that intersect the specified shape.
    /// </summary>
    /// <param name="shape">The shape to check the triangles against.</param>
    /// <returns>Returns a dictionary where the key represents the index of the triangle and the value represents the found collision points.</returns>
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
    /// Find all triangles that intersect the specified shape.
    /// </summary>
    /// <param name="shape">The shape to check the triangles against.</param>
    /// <returns>Returns a dictionary where the key represents the index of the triangle and the value represents the found collision points.</returns>
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
    /// Find all triangles that intersect the specified shape.
    /// </summary>
    /// <param name="shape">The shape to check the triangles against.</param>
    /// <returns>Returns a dictionary where the key represents the index of the triangle and the value represents the found collision points.</returns>
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
    /// Find all triangles that intersect the specified shape.
    /// </summary>
    /// <param name="shape">The shape to check the triangles against.</param>
    /// <returns>Returns a dictionary where the key represents the index of the triangle and the value represents the found collision points.</returns>
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
    /// Find all triangles that intersect the specified shape.
    /// </summary>
    /// <param name="shape">The shape to check the triangles against.</param>
    /// <returns>Returns a dictionary where the key represents the index of the triangle and the value represents the found collision points.</returns>
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
    /// Find all triangles that intersect the specified shape.
    /// </summary>
    /// <param name="shape">The shape to check the triangles against.</param>
    /// <returns>Returns a dictionary where the key represents the index of the triangle and the value represents the found collision points.</returns>
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