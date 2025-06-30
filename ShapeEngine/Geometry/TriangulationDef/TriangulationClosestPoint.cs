using System.Numerics;
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
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.TriangulationDef;


public partial class Triangulation
{
    /// <summary>
    /// Finds the closest point in the triangulation to the specified point.
    /// </summary>
    /// <param name="p">The point to which the closest point is sought.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point, direction, distance squared, and triangle index information.</returns>
    /// <remarks>If the triangulation is empty, returns a default <see cref="ClosestPointResult"/>.</remarks>
    public ClosestPointResult GetClosestPoint(Vector2 p)
    {
        if (Count <= 0) return new();

        var closestPoint = new CollisionPoint();
        var disSquared = -1f;
        var triangleIndex = -1;

        for (var i = 0; i < Count; i++)
        {
            var tri = this[i];
            var result = tri.GetClosestPoint(p, out float dis, out int index);

            if (dis < disSquared)
            {
                triangleIndex = index;
                disSquared = dis;
                closestPoint = result;
            }
        }

        return new(
            closestPoint,
            new CollisionPoint(p, (closestPoint.Point - p).Normalize()),
            disSquared,
            triangleIndex,
            -1);
    }

    /// <summary>
    /// Finds the triangle in the triangulation that is closest to the specified point.
    /// </summary>
    /// <param name="p">The point to which the closest triangle is sought.</param>
    /// <param name="disSquared">The squared distance to the closest point.</param>
    /// <param name="triangleIndex">The index of the closest triangle in the triangulation.</param>
    /// <returns>A tuple containing the closest <see cref="CollisionPoint"/> and the corresponding <see cref="Triangle"/>.</returns>
    /// <remarks>If the triangulation is empty, returns default values.</remarks>
    public (CollisionPoint point, Triangle triangle) GetClosestTriangle(Vector2 p, out float disSquared, out int triangleIndex)
    {
        disSquared = -1;
        triangleIndex = -1;
        if (Count <= 0) return (new(), new());

        var closestTriangle = new Triangle();
        var contained = false;
        var closestPoint = new CollisionPoint();

        for (var i = 0; i < Count; i++)
        {
            var tri = this[i];
            bool containsPoint = tri.ContainsPoint(p);
            var result = tri.GetClosestPoint(p, out float dis, out int index);

            if (dis < disSquared)
            {
                if (containsPoint || !contained)
                {
                    triangleIndex = index;
                    disSquared = dis;
                    closestTriangle = tri;
                    closestPoint = result;
                    if (containsPoint) contained = true;
                }
            }
            else
            {
                if (containsPoint && !contained)
                {
                    contained = true;
                    disSquared = dis;
                    closestTriangle = tri;
                    closestPoint = result;
                }
            }
        }

        return (closestPoint, closestTriangle);
    }

    /// <summary>
    /// Finds the closest point among a list of triangles to the specified point.
    /// </summary>
    /// <param name="triangles">The list of triangles to search.</param>
    /// <param name="p">The point to which the closest point is sought.</param>
    /// <param name="disSquared">The squared distance to the closest point.</param>
    /// <returns>The <see cref="CollisionPoint"/> in the triangles closest to the specified point.</returns>
    /// <remarks>If the list is empty, returns a default <see cref="CollisionPoint"/>.</remarks>
    public static CollisionPoint GetClosestPointTriangulationPoint(List<Triangle> triangles, Vector2 p, out float disSquared)
    {
        disSquared = -1;
        if (triangles.Count <= 0) return new();

        var curTriangle = triangles[0];
        var closest = curTriangle.GetClosestPoint(p, out disSquared);

        for (var i = 1; i < triangles.Count; i++)
        {
            curTriangle = triangles[i];
            var result = curTriangle.GetClosestPoint(p, out float dis);
            if (dis < disSquared)
            {
                closest = result;
                disSquared = dis;
            }
        }

        return closest;
    }

    /// <summary>
    /// Finds the closest point in the triangulation to the specified point.
    /// </summary>
    /// <param name="p">The point to which the closest point is sought.</param>
    /// <param name="disSquared">The squared distance to the closest point.</param>
    /// <returns>The closest <see cref="CollisionPoint"/> in the triangulation to the specified point.</returns>
    /// <remarks>If the triangulation is empty, returns a default <see cref="CollisionPoint"/>.</remarks>
    public CollisionPoint GetClosestPoint(Vector2 p, out float disSquared)
    {
        disSquared = -1;
        if (Count <= 0) return new();

        var curTriangle = this[0];
        var closestPoint = curTriangle.GetClosestPoint(p, out disSquared);

        for (var i = 1; i < Count; i++)
        {
            curTriangle = this[i];

            var cp = curTriangle.GetClosestPoint(p, out float dis);
            if (dis < disSquared)
            {
                closestPoint = cp;
                disSquared = dis;
            }
        }

        return closestPoint;
    }

    /// <summary>
    /// Finds the closest point in the triangulation to the specified point, including triangle and segment indices.
    /// </summary>
    /// <param name="p">The point to which the closest point is sought.</param>
    /// <param name="disSquared">The squared distance to the closest point.</param>
    /// <param name="triangleIndex">The index of the closest triangle in the triangulation.</param>
    /// <param name="segmentIndex">The index of the closest segment in the triangle.</param>
    /// <returns>The closest <see cref="CollisionPoint"/> in the triangulation to the specified point.</returns>
    /// <remarks>If the triangulation is empty, returns a default <see cref="CollisionPoint"/>.</remarks>
    public CollisionPoint GetClosestPoint(Vector2 p, out float disSquared, out int triangleIndex, out int segmentIndex)
    {
        disSquared = -1;
        triangleIndex = -1;
        segmentIndex = -1;
        if (Count <= 0) return new();

        var curTriangle = this[0];
        var closestPoint = curTriangle.GetClosestPoint(p, out disSquared, out segmentIndex);
        triangleIndex = 0;

        for (var i = 1; i < Count; i++)
        {
            curTriangle = this[i];

            var cp = curTriangle.GetClosestPoint(p, out float dis, out int index);
            if (dis < disSquared)
            {
                triangleIndex = i;
                segmentIndex = index;
                closestPoint = cp;
                disSquared = dis;
            }
        }

        return closestPoint;
    }

    /// <summary>
    /// Finds the closest vertex in the triangulation to the specified point.
    /// </summary>
    /// <param name="p">The point to which the closest vertex is sought.</param>
    /// <param name="disSquared">The squared distance to the closest vertex.</param>
    /// <param name="triangleIndex">The index of the triangle containing the closest vertex.</param>
    /// <param name="segmentIndex">The index of the segment containing the closest vertex.</param>
    /// <returns>The closest vertex <see cref="Vector2"/> in the triangulation to the specified point.</returns>
    /// <remarks>If the triangulation is empty, returns a default <see cref="Vector2"/>.</remarks>
    public Vector2 GetClosestVertex(Vector2 p, out float disSquared, out int triangleIndex, out int segmentIndex)
    {
        disSquared = -1;
        triangleIndex = -1;
        segmentIndex = -1;
        if (Count <= 0) return new();

        triangleIndex = 0;
        var curTriangle = this[0];
        var closestVertex = curTriangle.GetClosestVertex(p, out disSquared, out segmentIndex);

        for (var i = 1; i < Count; i++)
        {
            curTriangle = this[i];
            var vertex = curTriangle.GetClosestVertex(p, out float dis, out int index);

            if (dis < disSquared)
            {
                triangleIndex = i;
                segmentIndex = index;
                closestVertex = vertex;
                disSquared = dis;
            }
        }

        return closestVertex;
    }

    /// <summary>
    /// Finds the closest point on a line to the specified point.
    /// </summary>
    /// <param name="other">The line to which the closest point is sought.</param>
    /// <param name="triangleIndex">The index of the triangle in the triangulation.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point, direction, distance squared, and triangle index information.</returns>
    /// <remarks>If the triangulation is empty, returns a default <see cref="ClosestPointResult"/>.</remarks>
    public ClosestPointResult GetClosestPoint(Line other, out int triangleIndex)
    {
        triangleIndex = -1;
        if (Count <= 0) return new();
        var curTriangle = this[0];
        var closestResult = curTriangle.GetClosestPoint(other);

        for (var i = 1; i < Count; i++)
        {
            curTriangle = this[i];

            var result = curTriangle.GetClosestPoint(other);
            if (result.IsCloser(closestResult))
            {
                closestResult = result;
                triangleIndex = i;
            }
        }

        return closestResult;
    }

    /// <summary>
    /// Finds the closest point on a ray to the specified point.
    /// </summary>
    /// <param name="other">The ray to which the closest point is sought.</param>
    /// <param name="triangleIndex">The index of the triangle in the triangulation.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point, direction, distance squared, and triangle index information.</returns>
    /// <remarks>If the triangulation is empty, returns a default <see cref="ClosestPointResult"/>.</remarks>
    public ClosestPointResult GetClosestPoint(Ray other, out int triangleIndex)
    {
        triangleIndex = -1;
        if (Count <= 0) return new();
        var curTriangle = this[0];
        var closestResult = curTriangle.GetClosestPoint(other);

        for (var i = 1; i < Count; i++)
        {
            curTriangle = this[i];

            var result = curTriangle.GetClosestPoint(other);
            if (result.IsCloser(closestResult))
            {
                closestResult = result;
                triangleIndex = i;
            }
        }

        return closestResult;
    }

    /// <summary>
    /// Finds the closest point on a segment to the specified point.
    /// </summary>
    /// <param name="other">The segment to which the closest point is sought.</param>
    /// <param name="triangleIndex">The index of the triangle in the triangulation.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point, direction, distance squared, and triangle index information.</returns>
    /// <remarks>If the triangulation is empty, returns a default <see cref="ClosestPointResult"/>.</remarks>
    public ClosestPointResult GetClosestPoint(Segment other, out int triangleIndex)
    {
        triangleIndex = -1;
        if (Count <= 0) return new();
        var curTriangle = this[0];
        var closestResult = curTriangle.GetClosestPoint(other);

        for (var i = 1; i < Count; i++)
        {
            curTriangle = this[i];

            var result = curTriangle.GetClosestPoint(other);
            if (result.IsCloser(closestResult))
            {
                closestResult = result;
                triangleIndex = i;
            }
        }

        return closestResult;
    }

    /// <summary>
    /// Finds the closest point on a circle to the specified point.
    /// </summary>
    /// <param name="other">The circle to which the closest point is sought.</param>
    /// <param name="triangleIndex">The index of the triangle in the triangulation.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point, direction, distance squared, and triangle index information.</returns>
    /// <remarks>If the triangulation is empty, returns a default <see cref="ClosestPointResult"/>.</remarks>
    public ClosestPointResult GetClosestPoint(Circle other, out int triangleIndex)
    {
        triangleIndex = -1;
        if (Count <= 0) return new();
        var curTriangle = this[0];
        var closestResult = curTriangle.GetClosestPoint(other);

        for (var i = 1; i < Count; i++)
        {
            curTriangle = this[i];

            var result = curTriangle.GetClosestPoint(other);
            if (result.IsCloser(closestResult))
            {
                closestResult = result;
                triangleIndex = i;
            }
        }

        return closestResult;
    }

    /// <summary>
    /// Finds the closest point on a triangle to the specified point.
    /// </summary>
    /// <param name="other">The triangle to which the closest point is sought.</param>
    /// <param name="triangleIndex">The index of the triangle in the triangulation.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point, direction, distance squared, and triangle index information.</returns>
    /// <remarks>If the triangulation is empty, returns a default <see cref="ClosestPointResult"/>.</remarks>
    public ClosestPointResult GetClosestPoint(Triangle other, out int triangleIndex)
    {
        triangleIndex = -1;
        if (Count <= 0) return new();
        var curTriangle = this[0];
        var closestResult = curTriangle.GetClosestPoint(other);

        for (var i = 1; i < Count; i++)
        {
            curTriangle = this[i];

            var result = curTriangle.GetClosestPoint(other);
            if (result.IsCloser(closestResult))
            {
                closestResult = result;
                triangleIndex = i;
            }
        }

        return closestResult;
    }

    /// <summary>
    /// Finds the closest point on a quadrilateral to the specified point.
    /// </summary>
    /// <param name="other">The quadrilateral to which the closest point is sought.</param>
    /// <param name="triangleIndex">The index of the triangle in the triangulation.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point, direction, distance squared, and triangle index information.</returns>
    /// <remarks>If the triangulation is empty, returns a default <see cref="ClosestPointResult"/>.</remarks>
    public ClosestPointResult GetClosestPoint(Quad other, out int triangleIndex)
    {
        triangleIndex = -1;
        if (Count <= 0) return new();
        var curTriangle = this[0];
        var closestResult = curTriangle.GetClosestPoint(other);

        for (var i = 1; i < Count; i++)
        {
            curTriangle = this[i];

            var result = curTriangle.GetClosestPoint(other);
            if (result.IsCloser(closestResult))
            {
                closestResult = result;
                triangleIndex = i;
            }
        }

        return closestResult;
    }

    /// <summary>
    /// Finds the closest point on a rectangle to the specified point.
    /// </summary>
    /// <param name="other">The rectangle to which the closest point is sought.</param>
    /// <param name="triangleIndex">The index of the triangle in the triangulation.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point, direction, distance squared, and triangle index information.</returns>
    /// <remarks>If the triangulation is empty, returns a default <see cref="ClosestPointResult"/>.</remarks>
    public ClosestPointResult GetClosestPoint(Rect other, out int triangleIndex)
    {
        triangleIndex = -1;
        if (Count <= 0) return new();
        var curTriangle = this[0];
        var closestResult = curTriangle.GetClosestPoint(other);

        for (var i = 1; i < Count; i++)
        {
            curTriangle = this[i];

            var result = curTriangle.GetClosestPoint(other);
            if (result.IsCloser(closestResult))
            {
                closestResult = result;
                triangleIndex = i;
            }
        }

        return closestResult;
    }

    /// <summary>
    /// Finds the closest point on a polygon to the specified point.
    /// </summary>
    /// <param name="other">The polygon to which the closest point is sought.</param>
    /// <param name="triangleIndex">The index of the triangle in the triangulation.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point, direction, distance squared, and triangle index information.</returns>
    /// <remarks>If the triangulation is empty, returns a default <see cref="ClosestPointResult"/>.</remarks>
    public ClosestPointResult GetClosestPoint(Polygon other, out int triangleIndex)
    {
        triangleIndex = -1;
        if (Count <= 0) return new();
        var curTriangle = this[0];
        var closestResult = curTriangle.GetClosestPoint(other);

        for (var i = 1; i < Count; i++)
        {
            curTriangle = this[i];

            var result = curTriangle.GetClosestPoint(other);
            if (result.IsCloser(closestResult))
            {
                closestResult = result;
                triangleIndex = i;
            }
        }

        return closestResult;
    }

    /// <summary>
    /// Finds the closest point on a polyline to the specified point.
    /// </summary>
    /// <param name="other">The polyline to which the closest point is sought.</param>
    /// <param name="triangleIndex">The index of the triangle in the triangulation.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point, direction, distance squared, and triangle index information.</returns>
    /// <remarks>If the triangulation is empty, returns a default <see cref="ClosestPointResult"/>.</remarks>
    public ClosestPointResult GetClosestPoint(Polyline other, out int triangleIndex)
    {
        triangleIndex = -1;
        if (Count <= 0) return new();
        var curTriangle = this[0];
        var closestResult = curTriangle.GetClosestPoint(other);

        for (var i = 1; i < Count; i++)
        {
            curTriangle = this[i];

            var result = curTriangle.GetClosestPoint(other);
            if (result.IsCloser(closestResult))
            {
                closestResult = result;
                triangleIndex = i;
            }
        }

        return closestResult;
    }

    /// <summary>
    /// Finds the closest point on a set of segments to the specified point.
    /// </summary>
    /// <param name="other">The set of segments to which the closest point is sought.</param>
    /// <param name="triangleIndex">The index of the triangle in the triangulation.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point, direction, distance squared, and triangle index information.</returns>
    /// <remarks>If the triangulation is empty, returns a default <see cref="ClosestPointResult"/>.</remarks>
    public ClosestPointResult GetClosestPoint(Segments other, out int triangleIndex)
    {
        triangleIndex = -1;
        if (Count <= 0) return new();
        var curTriangle = this[0];
        var closestResult = curTriangle.GetClosestPoint(other);

        for (var i = 1; i < Count; i++)
        {
            curTriangle = this[i];

            var result = curTriangle.GetClosestPoint(other);
            if (result.IsCloser(closestResult))
            {
                closestResult = result;
                triangleIndex = i;
            }
        }

        return closestResult;
    }

    /// <summary>
    /// Finds the closest segment in the triangulation to the specified point.
    /// </summary>
    /// <param name="p">The point to which the closest segment is sought.</param>
    /// <param name="disSquared">The squared distance to the closest segment.</param>
    /// <param name="triangleIndex">The index of the triangle containing the closest segment.</param>
    /// <returns>A tuple containing the closest <see cref="Segment"/> and the corresponding <see cref="CollisionPoint"/>.</returns>
    /// <remarks>If the triangulation is empty, returns default values.</remarks>
    public (Segment segment, CollisionPoint segmentPoint) GetClosestSegment(Vector2 p, out float disSquared, out int triangleIndex)
    {
        triangleIndex = -1;
        disSquared = -1f;
        if (Count <= 0) return (new(), new());

        var curTriangle = this[0];
        var closestResult = curTriangle.GetClosestPoint(p, out disSquared, out int segmentIndex);

        for (var i = 1; i < Count; i++)
        {
            curTriangle = this[i];

            var result = curTriangle.GetClosestPoint(p, out float dis, out int index);
            if (dis < disSquared)
            {
                disSquared = dis;
                closestResult = result;
                triangleIndex = i;
                segmentIndex = index;
            }
        }

        var triangle = this[triangleIndex];
        var segment = triangle.GetSegment(segmentIndex);
        return (segment, closestResult);
    }

}