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
    /// Checks if a triangle in the collection overlaps with any collider in the given <see cref="CollisionObject"/>.
    /// </summary>
    /// <param name="collision">The collision object containing colliders to check for overlap.</param>
    /// <returns>True if any collider in the collision object overlaps any triangle in this collection; otherwise, false.</returns>
    public bool Overlap(CollisionObject collision)
    {
        if (!collision.HasColliders) return false;
        foreach (var collider in collision.Colliders)
        {
            if(Overlap(collider)) return true;
        }

        return false;
    }
    /// <summary>
    /// Determines if any triangle in this collection overlaps with the specified collider.
    /// </summary>
    /// <param name="collider">The collider to check for overlap.</param>
    /// <returns><c>true</c> if any triangle overlaps with the collider; otherwise, <c>false</c>.</returns>
    public bool Overlap(Collider collider)
    {
        if (!collider.Enabled) return false;
        for (int i = 0; i < Count; i++)
        {
            var tri = this[i];
            if (tri.Overlap(collider)) return true;
        }

        return false;
    }

    /// <summary>
    /// Determines if any triangle in this collection overlaps the specified line.
    /// </summary>
    /// <param name="shape">The line to check for overlap.</param>
    /// <returns><c>true</c> if any triangle overlaps with the line; otherwise, <c>false</c>.</returns>
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
    /// Determines if any triangle in this collection overlaps the specified ray.
    /// </summary>
    /// <param name="shape">The ray to check for overlap.</param>
    /// <returns><c>true</c> if any triangle overlaps with the ray; otherwise, <c>false</c>.</returns>
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
    /// Determines if any triangle in this collection overlaps the specified segment.
    /// </summary>
    /// <param name="shape">The segment to check for overlap.</param>
    /// <returns><c>true</c> if any triangle overlaps with the segment; otherwise, <c>false</c>.</returns>
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
    /// Determines if any triangle in this collection overlaps the specified circle.
    /// </summary>
    /// <param name="shape">The circle to check for overlap.</param>
    /// <returns><c>true</c> if any triangle overlaps with the circle; otherwise, <c>false</c>.</returns>
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
    /// <returns><c>true</c> if any triangle overlaps with the other triangle; otherwise, <c>false</c>.</returns>
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
    /// <returns><c>true</c> if any triangle overlaps with the rectangle; otherwise, <c>false</c>.</returns>
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
    /// <returns><c>true</c> if any triangle overlaps with the quadrilateral; otherwise, <c>false</c>.</returns>
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
    /// <returns><c>true</c> if any triangle overlaps with the polygon; otherwise, <c>false</c>.</returns>
    /// <remarks>Returns as soon as an overlapping triangle is found.</remarks>
    public bool OverlapShape(Polygon shape)
    {
        if (shape.Count < 3) return false;
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
    /// <returns><c>true</c> if any triangle overlaps with the polyline; otherwise, <c>false</c>.</returns>
    /// <remarks>Returns as soon as an overlapping triangle is found.</remarks>
    public bool OverlapShape(Polyline shape)
    {
        if (shape.Count < 2) return false;
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
    /// <returns><c>true</c> if any triangle overlaps with the segments; otherwise, <c>false</c>.</returns>
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
    /// Finds all triangles in this collection that overlap the specified collider.
    /// </summary>
    /// <param name="collider">The collider to check against.</param>
    /// <param name="triangleIndices">A list of indices of the triangles that overlap with the collider.</param>
    /// <returns><c>true</c> if at least one overlap was found; otherwise, <c>false</c>.</returns>
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
    /// Finds all triangles in this collection that overlap the specified line.
    /// </summary>
    /// <param name="shape">The line to check against.</param>
    /// <param name="triangleIndices">A list of indices of the triangles that overlap with the line.</param>
    /// <returns><c>true</c> if at least one overlap was found; otherwise, <c>false</c>.</returns>
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
    /// Finds all triangles in this collection that overlap the specified ray.
    /// </summary>
    /// <param name="shape">The ray to check against.</param>
    /// <param name="triangleIndices">A list of indices of the triangles that overlap with the ray.</param>
    /// <returns><c>true</c> if at least one overlap was found; otherwise, <c>false</c>.</returns>
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
    /// Finds all triangles in this collection that overlap the specified segment.
    /// </summary>
    /// <param name="shape">The segment to check against.</param>
    /// <param name="triangleIndices">A list of indices of the triangles that overlap with the segment.</param>
    /// <returns><c>true</c> if at least one overlap was found; otherwise, <c>false</c>.</returns>
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
    /// Finds all triangles in this collection that overlap the specified circle.
    /// </summary>
    /// <param name="shape">The circle to check against.</param>
    /// <param name="triangleIndices">A list of indices of the triangles that overlap with the circle.</param>
    /// <returns><c>true</c> if at least one overlap was found; otherwise, <c>false</c>.</returns>
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
    /// Finds all triangles in this collection that overlap the specified triangle.
    /// </summary>
    /// <param name="shape">The triangle to check against.</param>
    /// <param name="triangleIndices">A list of indices of the triangles that overlap with the other triangle.</param>
    /// <returns><c>true</c> if at least one overlap was found; otherwise, <c>false</c>.</returns>
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
    /// Finds all triangles in this collection that overlap the specified rectangle.
    /// </summary>
    /// <param name="shape">The rectangle to check against.</param>
    /// <param name="triangleIndices">A list of indices of the triangles that overlap with the rectangle.</param>
    /// <returns><c>true</c> if at least one overlap was found; otherwise, <c>false</c>.</returns>
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
    /// Finds all triangles in this collection that overlap the specified quad.
    /// </summary>
    /// <param name="shape">The quad to check against.</param>
    /// <param name="triangleIndices">A list of indices of the triangles that overlap with the quad.</param>
    /// <returns><c>true</c> if at least one overlap was found; otherwise, <c>false</c>.</returns>
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
    /// Finds all triangles in this collection that overlap the specified polygon.
    /// </summary>
    /// <param name="shape">The polygon to check against.</param>
    /// <param name="triangleIndices">A list of indices of the triangles that overlap with the polygon.</param>
    /// <returns><c>true</c> if at least one overlap was found; otherwise, <c>false</c>.</returns>
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
    /// Finds all triangles in this collection that overlap the specified polyline.
    /// </summary>
    /// <param name="shape">The polyline to check against.</param>
    /// <param name="triangleIndices">A list of indices of the triangles that overlap with the polyline.</param>
    /// <returns><c>true</c> if at least one overlap was found; otherwise, <c>false</c>.</returns>
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
    /// Finds all triangles in this collection that overlap the specified segments.
    /// </summary>
    /// <param name="segments">The segments to check against.</param>
    /// <param name="triangleIndices">A list of indices of the triangles that overlap with the segments.</param>
    /// <returns><c>true</c> if at least one overlap was found; otherwise, <c>false</c>.</returns>
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
    
    /// <summary>
    /// Determines whether this shape overlaps with the specified <see cref="IShape"/>.
    /// </summary>
    /// <param name="shape">The shape to test for overlap with this shape.
    /// The shape can be any supported type such as circle, segment, ray, line, triangle, rectangle, quad, polygon, or polyline.</param>
    /// <returns><c>true</c> if this shape overlaps with the specified shape; otherwise, <c>false</c>.</returns>
    public bool OverlapShape(IShape shape)
    {
        return shape.GetShapeType() switch
        {
            ShapeType.Circle => OverlapShape(shape.GetCircleShape()),
            ShapeType.Segment => OverlapShape(shape.GetSegmentShape()),
            ShapeType.Ray => OverlapShape(shape.GetRayShape()),
            ShapeType.Line => OverlapShape(shape.GetLineShape()),
            ShapeType.Triangle => OverlapShape(shape.GetTriangleShape()),
            ShapeType.Rect => OverlapShape(shape.GetRectShape()),
            ShapeType.Quad => OverlapShape(shape.GetQuadShape()),
            ShapeType.Poly => OverlapShape(shape.GetPolygonShape()),
            ShapeType.PolyLine => OverlapShape(shape.GetPolylineShape()),
            _ => false
        };
    }

}