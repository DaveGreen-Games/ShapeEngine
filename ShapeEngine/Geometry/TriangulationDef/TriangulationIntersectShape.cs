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
    /// Computes intersection points between the triangles and all colliders in the specified <see cref="CollisionObject"/>.
    /// </summary>
    /// <param name="collisionObject">The collision object containing colliders to test for intersection.</param>
    /// <returns>
    /// A <see cref="Dictionary{Collider, IntersectionPoints}"/> mapping each collider to its intersection points,
    /// or null if no colliders are present or no intersections are found.
    /// </returns>
    public Dictionary<Collider, Dictionary<int, IntersectionPoints>?>? Intersect(CollisionObject collisionObject)
    {
        if (!collisionObject.HasColliders) return null;

        Dictionary<Collider, Dictionary<int, IntersectionPoints>?>? intersections = null;
        foreach (var collider in collisionObject.Colliders)
        {
            var result = Intersect(collider);
            if(result == null) continue;
            intersections ??= new();
            intersections.Add(collider, result);
        }
        return intersections;
    }
    /// <summary>
    /// Checks for intersections between the triangles in this triangulation and the specified collider.
    /// </summary>
    /// <param name="collider">The collider to check for intersections.</param>
    /// <returns>A dictionary mapping the index of each intersecting triangle to the resulting <see cref="IntersectionPoints"/>. Returns <c>null</c> if no intersections are found.</returns>
    /// <remarks>Only triangles with valid intersections are included in the result.</remarks>
    public Dictionary<int, IntersectionPoints>? Intersect(Collider collider)
    {
        if (!collider.Enabled) return null;
        Dictionary<int, IntersectionPoints>? result = null;
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
    /// <returns>A dictionary mapping the index of each intersecting triangle to the resulting <see cref="IntersectionPoints"/>. Returns <c>null</c> if no intersections are found.</returns>
    /// <remarks>Only triangles with valid intersections are included in the result.</remarks>
    public Dictionary<int, IntersectionPoints>? IntersectShape(Line shape)
    {
        Dictionary<int, IntersectionPoints>? result = null;
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
    /// <returns>A dictionary mapping the index of each intersecting triangle to the resulting <see cref="IntersectionPoints"/>. Returns <c>null</c> if no intersections are found.</returns>
    /// <remarks>Only triangles with valid intersections are included in the result.</remarks>
    public Dictionary<int, IntersectionPoints>? IntersectShape(Ray shape)
    {
        Dictionary<int, IntersectionPoints>? result = null;
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
    /// <returns>A dictionary mapping the index of each intersecting triangle to the resulting <see cref="IntersectionPoints"/>. Returns <c>null</c> if no intersections are found.</returns>
    /// <remarks>Only triangles with valid intersections are included in the result.</remarks>
    public Dictionary<int, IntersectionPoints>? IntersectShape(Segment shape)
    {
        Dictionary<int, IntersectionPoints>? result = null;
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
    /// <returns>A dictionary mapping the index of each intersecting triangle to the resulting <see cref="IntersectionPoints"/>. Returns <c>null</c> if no intersections are found.</returns>
    /// <remarks>Only triangles with valid intersections are included in the result.</remarks>
    public Dictionary<int, IntersectionPoints>? IntersectShape(Circle shape)
    {
        Dictionary<int, IntersectionPoints>? result = null;
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
    /// <returns>A dictionary mapping the index of each intersecting triangle to the resulting <see cref="IntersectionPoints"/>. Returns <c>null</c> if no intersections are found.</returns>
    /// <remarks>Only triangles with valid intersections are included in the result.</remarks>
    public Dictionary<int, IntersectionPoints>? IntersectShape(Triangle shape)
    {
        Dictionary<int, IntersectionPoints>? result = null;
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
    /// <returns>A dictionary mapping the index of each intersecting triangle to the resulting <see cref="IntersectionPoints"/>. Returns <c>null</c> if no intersections are found.</returns>
    /// <remarks>Only triangles with valid intersections are included in the result.</remarks>
    public Dictionary<int, IntersectionPoints>? IntersectShape(Rect shape)
    {
        Dictionary<int, IntersectionPoints>? result = null;
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
    /// <returns>A dictionary mapping the index of each intersecting triangle to the resulting <see cref="IntersectionPoints"/>. Returns <c>null</c> if no intersections are found.</returns>
    /// <remarks>Only triangles with valid intersections are included in the result.</remarks>
    public Dictionary<int, IntersectionPoints>? IntersectShape(Quad shape)
    {
        Dictionary<int, IntersectionPoints>? result = null;
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
    /// <returns>A dictionary mapping the index of each intersecting triangle to the resulting <see cref="IntersectionPoints"/>. Returns <c>null</c> if no intersections are found.</returns>
    /// <remarks>Only triangles with valid intersections are included in the result.</remarks>
    public Dictionary<int, IntersectionPoints>? IntersectShape(Polygon shape)
    {
        if (shape.Count < 3) return null;
        Dictionary<int, IntersectionPoints>? result = null;
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
    /// <returns>A dictionary mapping the index of each intersecting triangle to the resulting <see cref="IntersectionPoints"/>. Returns <c>null</c> if no intersections are found.</returns>
    /// <remarks>Only triangles with valid intersections are included in the result.</remarks>
    public Dictionary<int, IntersectionPoints>? IntersectShape(Polyline shape)
    {
        if (shape.Count < 2) return null;
        Dictionary<int, IntersectionPoints>? result = null;
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
    /// <returns>A dictionary mapping the index of each intersecting triangle to the resulting <see cref="IntersectionPoints"/>. Returns <c>null</c> if no intersections are found.</returns>
    /// <remarks>Only triangles with valid intersections are included in the result.</remarks>
    public Dictionary<int, IntersectionPoints>? IntersectShape(Segments shape)
    {
        Dictionary<int, IntersectionPoints>? result = null;
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
    /// Checks for intersections between the triangles in this triangulation and the specified shape implementing <see cref="IShape"/>.
    /// </summary>
    /// <param name="shape">The shape to check for intersections.</param>
    /// <returns>
    /// A dictionary mapping the index of each intersecting triangle to the resulting <see cref="IntersectionPoints"/>.
    /// Returns <c>null</c> if no intersections are found or the shape type is not supported.
    /// </returns>
    public Dictionary<int, IntersectionPoints>? IntersectShape(IShape shape)
    {
        return shape.GetShapeType() switch
        {
            ShapeType.Circle => IntersectShape(shape.GetCircleShape()),
            ShapeType.Segment => IntersectShape(shape.GetSegmentShape()),
            ShapeType.Ray => IntersectShape(shape.GetRayShape()),
            ShapeType.Line => IntersectShape(shape.GetLineShape()),
            ShapeType.Triangle => IntersectShape(shape.GetTriangleShape()),
            ShapeType.Rect => IntersectShape(shape.GetRectShape()),
            ShapeType.Quad => IntersectShape(shape.GetQuadShape()),
            ShapeType.Poly => IntersectShape(shape.GetPolygonShape()),
            ShapeType.PolyLine => IntersectShape(shape.GetPolylineShape()),
            _ => null
        };
    }

}