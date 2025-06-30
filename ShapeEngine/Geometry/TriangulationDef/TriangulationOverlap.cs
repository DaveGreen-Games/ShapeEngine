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
    /// Find if any triangle in this collection overlaps the specified shape.
    /// </summary>
    /// <param name="collider"></param>
    /// <returns>Returns true after the first overlap is found. If no overlap is found, returns false.</returns>
    public bool Overlap(Collider collider)
    {
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            if (tri.Overlap(collider)) return true;
        }

        return false;
    }

    /// <summary>
    /// Find if any triangle in this collection overlaps the specified shape.
    /// </summary>
    /// <param name="shape"></param>
    /// <returns>Returns true after the first overlap is found. If no overlap is found, returns false.</returns>
    public bool OverlapShape(Line shape)
    {
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            if (tri.OverlapShape(shape)) return true;
        }

        return false;
    }

    /// <summary>
    /// Find if any triangle in this collection overlaps the specified shape.
    /// </summary>
    /// <param name="shape"></param>
    /// <returns>Returns true after the first overlap is found. If no overlap is found, returns false.</returns>
    public bool OverlapShape(Ray shape)
    {
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            if (tri.OverlapShape(shape)) return true;
        }

        return false;
    }

    /// <summary>
    /// Find if any triangle in this collection overlaps the specified shape.
    /// </summary>
    /// <param name="shape"></param>
    /// <returns>Returns true after the first overlap is found. If no overlap is found, returns false.</returns>
    public bool OverlapShape(Segment shape)
    {
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            if (tri.OverlapShape(shape)) return true;
        }

        return false;
    }

    /// <summary>
    /// Find if any triangle in this collection overlaps the specified shape.
    /// </summary>
    /// <param name="shape"></param>
    /// <returns>Returns true after the first overlap is found. If no overlap is found, returns false.</returns>
    public bool OverlapShape(Circle shape)
    {
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            if (tri.OverlapShape(shape)) return true;
        }

        return false;
    }

    /// <summary>
    /// Determines if any triangle in this collection overlaps the specified triangle shape.
    /// </summary>
    /// <param name="shape">The triangle to check for overlap.</param>
    /// <returns>Returns true after the first overlap is found. If no overlap is found, returns false.</returns>
    /// <remarks>Returns as soon as an overlapping triangle is found.</remarks>
    public bool OverlapShape(Triangle shape)
    {
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            if (tri.OverlapShape(shape)) return true;
        }

        return false;
    }

    /// <summary>
    /// Determines if any triangle in this collection overlaps the specified rectangle shape.
    /// </summary>
    /// <param name="shape">The rectangle to check for overlap.</param>
    /// <returns>Returns true after the first overlap is found. If no overlap is found, returns false.</returns>
    /// <remarks>Returns as soon as an overlapping triangle is found.</remarks>
    public bool OverlapShape(Rect shape)
    {
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            if (tri.OverlapShape(shape)) return true;
        }

        return false;
    }

    /// <summary>
    /// Determines if any triangle in this collection overlaps the specified quadrilateral shape.
    /// </summary>
    /// <param name="shape">The quadrilateral to check for overlap.</param>
    /// <returns>Returns true after the first overlap is found. If no overlap is found, returns false.</returns>
    /// <remarks>Returns as soon as an overlapping triangle is found.</remarks>
    public bool OverlapShape(Quad shape)
    {
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            if (tri.OverlapShape(shape)) return true;
        }

        return false;
    }

    /// <summary>
    /// Determines if any triangle in this collection overlaps the specified polygon shape.
    /// </summary>
    /// <param name="shape">The polygon to check for overlap.</param>
    /// <returns>Returns true after the first overlap is found. If no overlap is found, returns false.</returns>
    /// <remarks>Returns as soon as an overlapping triangle is found.</remarks>
    public bool OverlapShape(Polygon shape)
    {
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            if (tri.OverlapShape(shape)) return true;
        }

        return false;
    }

    /// <summary>
    /// Determines if any triangle in this collection overlaps the specified polyline shape.
    /// </summary>
    /// <param name="shape">The polyline to check for overlap.</param>
    /// <returns>Returns true after the first overlap is found. If no overlap is found, returns false.</returns>
    /// <remarks>Returns as soon as an overlapping triangle is found.</remarks>
    public bool OverlapShape(Polyline shape)
    {
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            if (tri.OverlapShape(shape)) return true;
        }

        return false;
    }

    /// <summary>
    /// Determines if any triangle in this collection overlaps the specified segments shape.
    /// </summary>
    /// <param name="shape">The segments to check for overlap.</param>
    /// <returns>Returns true after the first overlap is found. If no overlap is found, returns false.</returns>
    /// <remarks>Returns as soon as an overlapping triangle is found.</remarks>
    public bool OverlapShape(Segments shape)
    {
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            if (tri.OverlapShape(shape)) return true;
        }

        return false;
    }

    /// <summary>
    /// Find all triangles in this collection that overlap the specified shape.
    /// </summary>
    /// <param name="collider">The collider to check against.</param>
    /// <param name="triangleIndices">All triangle indices that overlap the specified collider.</param>
    /// <returns>Return true if at least 1 overlap was found, otherwise return false. Does not return early. All triangle have to be checked.</returns>
    public bool Overlap(Collider collider, out List<int>? triangleIndices)
    {
        triangleIndices = null;
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            if (!tri.Overlap(collider)) continue;
            triangleIndices ??= new List<int>();
            triangleIndices.Add(i);
        }

        return triangleIndices != null && triangleIndices.Count > 0;
    }

    /// <summary>
    /// Find all triangles in this collection that overlap the specified shape.
    /// </summary>
    /// <param name="shape">The shape to check against.</param>
    /// <param name="triangleIndices">All triangle indices that overlap the specified shape.</param>
    /// <returns>Return true if at least 1 overlap was found, otherwise return false. Does not return early. All triangle have to be checked.</returns>
    public bool OverlapShape(Line shape, out List<int>? triangleIndices)
    {
        triangleIndices = null;
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            if (!tri.OverlapShape(shape)) continue;
            triangleIndices ??= new List<int>();
            triangleIndices.Add(i);
        }

        return triangleIndices != null && triangleIndices.Count > 0;
    }

    /// <summary>
    /// Find all triangles in this collection that overlap the specified shape.
    /// </summary>
    /// <param name="shape">The shape to check against.</param>
    /// <param name="triangleIndices">All triangle indices that overlap the specified shape.</param>
    /// <returns>Return true if at least 1 overlap was found, otherwise return false. Does not return early. All triangle have to be checked.</returns>
    public bool OverlapShape(Ray shape, out List<int>? triangleIndices)
    {
        triangleIndices = null;
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            if (!tri.OverlapShape(shape)) continue;
            triangleIndices ??= new List<int>();
            triangleIndices.Add(i);
        }

        return triangleIndices != null && triangleIndices.Count > 0;
    }

    /// <summary>
    /// Find all triangles in this collection that overlap the specified shape.
    /// </summary>
    /// <param name="shape">The shape to check against.</param>
    /// <param name="triangleIndices">All triangle indices that overlap the specified shape.</param>
    /// <returns>Return true if at least 1 overlap was found, otherwise return false. Does not return early. All triangle have to be checked.</returns>
    public bool OverlapShape(Segment shape, out List<int>? triangleIndices)
    {
        triangleIndices = null;
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            if (!tri.OverlapShape(shape)) continue;
            triangleIndices ??= new List<int>();
            triangleIndices.Add(i);
        }

        return triangleIndices != null && triangleIndices.Count > 0;
    }

    /// <summary>
    /// Find all triangles in this collection that overlap the specified shape.
    /// </summary>
    /// <param name="shape">The shape to check against.</param>
    /// <param name="triangleIndices">All triangle indices that overlap the specified shape.</param>
    /// <returns>Return true if at least 1 overlap was found, otherwise return false. Does not return early. All triangle have to be checked.</returns>
    public bool OverlapShape(Circle shape, out List<int>? triangleIndices)
    {
        triangleIndices = null;
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            if (!tri.OverlapShape(shape)) continue;
            triangleIndices ??= new List<int>();
            triangleIndices.Add(i);
        }

        return triangleIndices != null && triangleIndices.Count > 0;
    }

    /// <summary>
    /// Find all triangles in this collection that overlap the specified shape.
    /// </summary>
    /// <param name="shape">The shape to check against.</param>
    /// <param name="triangleIndices">All triangle indices that overlap the specified shape.</param>
    /// <returns>Return true if at least 1 overlap was found, otherwise return false. Does not return early. All triangle have to be checked.</returns>
    public bool OverlapShape(Triangle shape, out List<int>? triangleIndices)
    {
        triangleIndices = null;
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            if (!tri.OverlapShape(shape)) continue;
            triangleIndices ??= new List<int>();
            triangleIndices.Add(i);
        }

        return triangleIndices != null && triangleIndices.Count > 0;
    }

    /// <summary>
    /// Find all triangles in this collection that overlap the specified shape.
    /// </summary>
    /// <param name="shape">The shape to check against.</param>
    /// <param name="triangleIndices">All triangle indices that overlap the specified shape.</param>
    /// <returns>Return true if at least 1 overlap was found, otherwise return false. Does not return early. All triangle have to be checked.</returns>
    public bool OverlapShape(Rect shape, out List<int>? triangleIndices)
    {
        triangleIndices = null;
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            if (!tri.OverlapShape(shape)) continue;
            triangleIndices ??= new List<int>();
            triangleIndices.Add(i);
        }

        return triangleIndices != null && triangleIndices.Count > 0;
    }

    /// <summary>
    /// Find all triangles in this collection that overlap the specified shape.
    /// </summary>
    /// <param name="shape">The shape to check against.</param>
    /// <param name="triangleIndices">All triangle indices that overlap the specified shape.</param>
    /// <returns>Return true if at least 1 overlap was found, otherwise return false. Does not return early. All triangle have to be checked.</returns>
    public bool OverlapShape(Quad shape, out List<int>? triangleIndices)
    {
        triangleIndices = null;
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            if (!tri.OverlapShape(shape)) continue;
            triangleIndices ??= new List<int>();
            triangleIndices.Add(i);
        }

        return triangleIndices != null && triangleIndices.Count > 0;
    }

    /// <summary>
    /// Find all triangles in this collection that overlap the specified shape.
    /// </summary>
    /// <param name="shape">The shape to check against.</param>
    /// <param name="triangleIndices">All triangle indices that overlap the specified shape.</param>
    /// <returns>Return true if at least 1 overlap was found, otherwise return false. Does not return early. All triangle have to be checked.</returns>
    public bool OverlapShape(Polygon shape, out List<int>? triangleIndices)
    {
        triangleIndices = null;
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            if (!tri.OverlapShape(shape)) continue;
            triangleIndices ??= new List<int>();
            triangleIndices.Add(i);
        }

        return triangleIndices != null && triangleIndices.Count > 0;
    }

    /// <summary>
    /// Find all triangles in this collection that overlap the specified shape.
    /// </summary>
    /// <param name="shape">The shape to check against.</param>
    /// <param name="triangleIndices">All triangle indices that overlap the specified shape.</param>
    /// <returns>Return true if at least 1 overlap was found, otherwise return false. Does not return early. All triangle have to be checked.</returns>
    public bool OverlapShape(Polyline shape, out List<int>? triangleIndices)
    {
        triangleIndices = null;
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            if (!tri.OverlapShape(shape)) continue;
            triangleIndices ??= new List<int>();
            triangleIndices.Add(i);
        }

        return triangleIndices != null && triangleIndices.Count > 0;
    }

    /// <summary>
    /// Find all triangles in this collection that overlap the specified segments.
    /// </summary>
    /// <param name="segments">The segments to check against.</param>
    /// <param name="triangleIndices">All triangle indices that overlap the specified segments.</param>
    /// <returns>Return true if at least 1 overlap was found, otherwise return false. Does not return early. All triangle have to be checked.</returns>
    public bool OverlapShape(Segments segments, out List<int>? triangleIndices)
    {
        triangleIndices = null;
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            if (!tri.OverlapShape(segments)) continue;
            triangleIndices ??= new List<int>();
            triangleIndices.Add(i);
        }

        return triangleIndices != null && triangleIndices.Count > 0;
    }

}