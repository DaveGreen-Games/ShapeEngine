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