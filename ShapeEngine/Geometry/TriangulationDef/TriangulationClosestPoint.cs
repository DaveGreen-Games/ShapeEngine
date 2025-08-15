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
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point on the triangulation and the point 'p'.</returns>
    /// <remarks>If the triangulation is empty, returns a default <see cref="ClosestPointResult"/>.</remarks>
    public ClosestPointResult GetClosestPoint(Vector2 p)
    {
        if (Count <= 0) return new();

        var closestPoint = new IntersectionPoint();
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

        return new(closestPoint, new IntersectionPoint(p, (closestPoint.Point - p).Normalize()), disSquared, triangleIndex);
    }

    /// <summary>
    /// Finds the triangle in the triangulation that is closest to the specified point.
    /// </summary>
    /// <param name="p">The point to which the closest triangle is sought.</param>
    /// <param name="disSquared">The squared distance to the closest point.</param>
    /// <param name="triangleIndex">The index of the closest triangle in the triangulation.</param>
    /// <returns>A tuple containing the closest <see cref="IntersectionPoint"/> and the corresponding <see cref="Triangle"/>.</returns>
    /// <remarks>If the triangulation is empty, returns default values.</remarks>
    public (IntersectionPoint point, Triangle triangle) GetClosestTriangle(Vector2 p, out float disSquared, out int triangleIndex)
    {
        disSquared = -1;
        triangleIndex = -1;
        if (Count <= 0) return (new(), new());

        var closestTriangle = new Triangle();
        var contained = false;
        var closestPoint = new IntersectionPoint();

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
    /// <returns>The <see cref="IntersectionPoint"/> in the triangles closest to the specified point.</returns>
    /// <remarks>If the list is empty, returns a default <see cref="IntersectionPoint"/>.</remarks>
    public static IntersectionPoint GetClosestPointTriangulationPoint(List<Triangle> triangles, Vector2 p, out float disSquared)
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
    /// <returns>The closest <see cref="IntersectionPoint"/> in the triangulation to the specified point.</returns>
    /// <remarks>If the triangulation is empty, returns a default <see cref="IntersectionPoint"/>.</remarks>
    public IntersectionPoint GetClosestPoint(Vector2 p, out float disSquared)
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
    /// <returns>The closest <see cref="IntersectionPoint"/> in the triangulation to the specified point.</returns>
    /// <remarks>If the triangulation is empty, returns a default <see cref="IntersectionPoint"/>.</remarks>
    public IntersectionPoint GetClosestPoint(Vector2 p, out float disSquared, out int triangleIndex, out int segmentIndex)
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
    /// Finds the closest point in the triangle collection to any collider in the given collision object.
    /// </summary>
    /// <param name="collisionObject">The collision object containing one or more colliders to compare against.</param>
    /// <param name="triangleIndex">The index of the triangle in the triangulation that is closest.</param>
    /// <returns>
    /// A <see cref="ClosestPointResult"/> containing the closest point information for the nearest collider.
    /// </returns>
    public ClosestPointResult GetClosestPoint(CollisionObject collisionObject, out int triangleIndex)
    {
        triangleIndex = -1;
        if (!collisionObject.HasColliders) return new();
        var closestPoint = new ClosestPointResult();
        foreach (var collider in collisionObject.Colliders)
        {
            var result = GetClosestPoint(collider, out int index);
            if(!result.Valid) continue;
            if (!closestPoint.Valid)
            {
                closestPoint = result;
                triangleIndex = index;
            }
            else
            {
                if (result.DistanceSquared < closestPoint.DistanceSquared)
                {
                    closestPoint = result;
                    triangleIndex = index;
                }
            }
        }
        return closestPoint;
    }

    /// <summary>
    /// Finds the closest point on the triangle collection to the given collider.
    /// </summary>
    /// <param name="collider">The collider to compare against.</param>
    /// <param name="triangleIndex">The index of the triangle in the triangulation that is closest to the collider.</param>
    /// <returns>
    /// A <see cref="ClosestPointResult"/> containing the closest point information for the collider.
    /// </returns>
    public ClosestPointResult GetClosestPoint(Collider collider, out int triangleIndex)
    {
        triangleIndex = -1;
        if (!collider.Enabled) return new();
        switch (collider.GetShapeType())
        {
            case ShapeType.Line: return GetClosestPoint(collider.GetLineShape(), out triangleIndex);
            case ShapeType.Ray: return GetClosestPoint(collider.GetRayShape(), out triangleIndex);
            case ShapeType.Circle: return GetClosestPoint(collider.GetCircleShape(), out triangleIndex);
            case ShapeType.Segment: return GetClosestPoint(collider.GetSegmentShape(), out triangleIndex);
            case ShapeType.Triangle: return GetClosestPoint(collider.GetTriangleShape(), out triangleIndex);
            case ShapeType.Rect: return GetClosestPoint(collider.GetRectShape(), out triangleIndex);
            case ShapeType.Quad: return GetClosestPoint(collider.GetQuadShape(), out triangleIndex);
            case ShapeType.Poly: return GetClosestPoint(collider.GetPolygonShape(),out triangleIndex);
            case ShapeType.PolyLine: return GetClosestPoint(collider.GetPolylineShape(),out triangleIndex);
        }

        return new();
    }
    
    
    /// <summary>
    /// Finds the closest point in the triangulation to the specified line.
    /// </summary>
    /// <param name="other">The line to find the closest point to.</param>
    /// <param name="triangleIndex">The index of the triangle in the triangulation that is closest to the line.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point on the triangulation and the closest point on the line.</returns>
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
    /// Finds the closest point in the triangulation to the specified ray.
    /// </summary>
    /// <param name="other">The ray to find the closest point to.</param>
    /// <param name="triangleIndex">The index of the triangle in the triangulation that is closest to the ray.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point on the triangulation and the closest point on the ray.</returns>
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
    /// Finds the closest point in the triangulation to the specified segment.
    /// </summary>
    /// <param name="other">The segment to find the closest point to.</param>
    /// <param name="triangleIndex">The index of the triangle in the triangulation that is closest to the segment.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point on the triangulation and the closest point on the segment.</returns>
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
    /// Finds the closest point in the triangulation to the specified circle.
    /// </summary>
    /// <param name="other">The circle to find the closest point to.</param>
    /// <param name="triangleIndex">The index of the triangle in the triangulation that is closest to the circle.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point on the triangulation and the closest point on the circle.</returns>
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
    /// Finds the closest point in the triangulation to the specified triangle.
    /// </summary>
    /// <param name="other">The triangle to find the closest point to.</param>
    /// <param name="triangleIndex">The index of the triangle in the triangulation that is closest to the other triangle.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point on the triangulation and the closest point on the other triangle.</returns>
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
    /// Finds the closest point in the triangulation to the specified quad.
    /// </summary>
    /// <param name="other">The quad to find the closest point to.</param>
    /// <param name="triangleIndex">The index of the triangle in the triangulation that is closest to the quad.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point on the triangulation and the closest point on the quad.</returns>
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
    /// Finds the closest point in the triangulation to the specified rectangle.
    /// </summary>
    /// <param name="other">The rectangle to find the closest point to.</param>
    /// <param name="triangleIndex">The index of the triangle in the triangulation that is closest to the rectangle.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point on the triangulation and the closest point on the rectangle.</returns>
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
    /// Finds the closest point in the triangulation to the specified polygon.
    /// </summary>
    /// <param name="other">The polygon to find the closest point to.</param>
    /// <param name="triangleIndex">The index of the triangle in the triangulation that is closest to the polygon.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point on the triangulation and the closest point on the polygon.</returns>
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
    /// Finds the closest point in the triangulation to the specified polyline.
    /// </summary>
    /// <param name="other">The polyline to find the closest point to.</param>
    /// <param name="triangleIndex">The index of the triangle in the triangulation that is closest to the polyline.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point on the triangulation and the closest point on the polyline.</returns>
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
    /// <returns>A tuple containing the closest <see cref="Segment"/> and the corresponding <see cref="IntersectionPoint"/>.</returns>
    /// <remarks>If the triangulation is empty, returns default values.</remarks>
    public (Segment segment, IntersectionPoint segmentPoint) GetClosestSegment(Vector2 p, out float disSquared, out int triangleIndex)
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
    
    /// <summary>
    /// Finds the closest point in the triangulation to the specified shape.
    /// </summary>
    /// <param name="shape">The shape to find the closest point to.</param>
    /// <param name="triangleIndex">The index of the triangle in the triangulation that is closest to the shape.</param>
    /// <returns>
    /// A <see cref="ClosestPointResult"/> containing the closest point information for the shape.
    /// </returns>
    public ClosestPointResult GetClosestPoint(IShape shape, out int triangleIndex)
    {
        triangleIndex = -1;
        return shape.GetShapeType() switch
        {
            ShapeType.Circle => GetClosestPoint(shape.GetCircleShape(), out triangleIndex),
            ShapeType.Segment => GetClosestPoint(shape.GetSegmentShape(), out triangleIndex),
            ShapeType.Ray => GetClosestPoint(shape.GetRayShape(), out triangleIndex),
            ShapeType.Line => GetClosestPoint(shape.GetLineShape(), out triangleIndex),
            ShapeType.Triangle => GetClosestPoint(shape.GetTriangleShape(), out triangleIndex),
            ShapeType.Rect => GetClosestPoint(shape.GetRectShape(), out triangleIndex),
            ShapeType.Quad => GetClosestPoint(shape.GetQuadShape(), out triangleIndex),
            ShapeType.Poly => GetClosestPoint(shape.GetPolygonShape(), out triangleIndex),
            ShapeType.PolyLine => GetClosestPoint(shape.GetPolylineShape(), out triangleIndex),
            _ => new()
        };
    }

}

