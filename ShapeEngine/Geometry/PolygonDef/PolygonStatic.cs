using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.Geometry.TriangulationDef;
using ShapeEngine.Random;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.PolygonDef;

public partial class Polygon
{
    /// <summary>
    /// Intersects a ray with this polygon and returns all segments of the ray that lie inside the polygon.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray. Must not be zero.</param>
    /// <returns>A list of segments inside the polygon, or null if there are fewer than 3 vertices or no valid intersections.</returns>
    /// <remarks>Segments are sorted by distance from the ray origin.</remarks>
    public List<Segment>? CutRayWithPolygon(Vector2 rayPoint, Vector2 rayDirection)
    {
        if (Count < 3) return null;
        if (rayDirection.X == 0 && rayDirection.Y == 0) return null;

        rayDirection = rayDirection.Normalize();
        var intersectionPoints = IntersectPolygonRay(this, rayPoint, rayDirection);
        if (intersectionPoints == null || intersectionPoints.Count < 2) return null;

        intersectionPoints.SortClosestFirst(rayPoint);

        var segments = new List<Segment>();
        for (int i = 0; i < intersectionPoints.Count - 1; i += 2)
        {
            var segmentStart = intersectionPoints[i].Point;
            var segmentEnd = intersectionPoints[i + 1].Point;
            var segment = new Segment(segmentStart, segmentEnd);
            segments.Add(segment);
        }

        return segments;
    }

    /// <summary>
    /// Intersects a ray with this polygon and stores all segments of the ray that lie inside the polygon in the provided result list.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray. Must not be zero.</param>
    /// <param name="result">A list to store the resulting segments inside the polygon.</param>
    /// <returns>The number of segments added to the result list.</returns>
    /// <remarks>Segments are sorted by distance from the ray origin.</remarks>
    public int CutRayWithPolygon(Vector2 rayPoint, Vector2 rayDirection, ref List<Segment> result)
    {
        if (Count < 3) return 0;
        if (rayDirection.X == 0 && rayDirection.Y == 0) return 0;

        rayDirection = rayDirection.Normalize();
        var intersectionPoints = IntersectPolygonRay(this, rayPoint, rayDirection, ref intersectionPointsReference);
        if (intersectionPoints < 2) return 0;

        int count = result.Count;
        intersectionPointsReference.SortClosestFirst(rayPoint);

        for (int i = 0; i < intersectionPointsReference.Count - 1; i += 2)
        {
            var segmentStart = intersectionPointsReference[i].Point;
            var segmentEnd = intersectionPointsReference[i + 1].Point;
            var segment = new Segment(segmentStart, segmentEnd);
            result.Add(segment);
        }

        intersectionPointsReference.Clear();
        return result.Count - count;
    }

    internal static bool ContainsPointCheck(Vector2 a, Vector2 b, Vector2 pointToCheck)
    {
        if (a.Y < pointToCheck.Y && b.Y >= pointToCheck.Y || b.Y < pointToCheck.Y && a.Y >= pointToCheck.Y)
        {
            if (a.X + (pointToCheck.Y - a.Y) / (b.Y - a.Y) * (b.X - a.X) < pointToCheck.X)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Triangulates a set of points using Delaunay triangulation. Only works with non-self-intersecting shapes.
    /// </summary>
    /// <param name="points">The points to triangulate. Can be any set of points, including polygons.</param>
    /// <returns>A triangulation of the input points.</returns>
    public static Triangulation TriangulateDelaunay(IEnumerable<Vector2> points)
    {
        var enumerable = points.ToList();
        var supraTriangle = GetBoundingTriangle(enumerable, 2f);
        return TriangulateDelaunay(enumerable, supraTriangle);
    }

    /// <summary>
    /// Triangulates a set of points using Delaunay triangulation, with a custom bounding triangle. Only works with non-self-intersecting shapes.
    /// </summary>
    /// <param name="points">The points to triangulate. Can be any set of points, including polygons.</param>
    /// <param name="supraTriangle">A triangle that encapsulates all the points.</param>
    /// <returns>A triangulation of the input points.</returns>
    public static Triangulation TriangulateDelaunay(IEnumerable<Vector2> points, Triangle supraTriangle)
    {
        Triangulation triangles = new() { supraTriangle };

        foreach (var p in points)
        {
            Triangulation badTriangles = new();

            //Identify 'bad triangles'
            for (int triIndex = triangles.Count - 1; triIndex >= 0; triIndex--)
            {
                Triangle triangle = triangles[triIndex];

                //A 'bad triangle' is defined as a triangle who's CircumCentre contains the current point
                var circumCircle = triangle.GetCircumCircle();
                float distSq = Vector2.DistanceSquared(p, circumCircle.Center);
                if (distSq < circumCircle.Radius * circumCircle.Radius)
                {
                    badTriangles.Add(triangle);
                    triangles.RemoveAt(triIndex);
                }
            }

            Segments allEdges = new();
            foreach (var badTriangle in badTriangles)
            {
                allEdges.AddRange(badTriangle.GetEdges());
            }

            Segments uniqueEdges = GetUniqueSegmentsDelaunay(allEdges);
            //Create new triangles
            for (int i = 0; i < uniqueEdges.Count; i++)
            {
                var edge = uniqueEdges[i];
                triangles.Add(new(p, edge));
            }
        }

        //Remove all triangles that share a vertex with the supra triangle to recieve the final triangulation
        for (int i = triangles.Count - 1; i >= 0; i--)
        {
            var t = triangles[i];
            if (t.SharesVertex(supraTriangle)) triangles.RemoveAt(i);
        }


        return triangles;
    }

    private static Segments GetUniqueSegmentsDelaunay(Segments segments)
    {
        Segments uniqueEdges = new();
        for (int i = segments.Count - 1; i >= 0; i--)
        {
            var edge = segments[i];
            if (IsSimilar(segments, edge))
            {
                uniqueEdges.Add(edge);
            }
        }

        return uniqueEdges;
    }

    private static bool IsSimilar(Segments segments, Segment seg)
    {
        var counter = 0;
        foreach (var segment in segments)
        {
            if (segment.IsSimilar(seg)) counter++;
            if (counter > 1) return false;
        }

        return true;
    }

    /// <summary>
    /// Gets a bounding rectangle that encapsulates all points.
    /// </summary>
    /// <param name="points">The points to encapsulate.</param>
    /// <returns>A rectangle that contains all the points, or an empty rectangle if fewer than 2 points.</returns>
    public static Rect GetBoundingBox(IEnumerable<Vector2> points)
    {
        var enumerable = points as Vector2[] ?? points.ToArray();
        if (enumerable.Length < 2) return new();
        var start = enumerable.First();
        Rect r = new(start.X, start.Y, 0, 0);

        foreach (var p in enumerable)
        {
            r = r.Enlarge(p);
        }

        return r;
    }

    /// <summary>
    /// Gets a triangle that encapsulates all points, with an optional margin factor.
    /// </summary>
    /// <param name="points">The points to encapsulate.</param>
    /// <param name="marginFactor">A factor for scaling the final triangle. Default is 1.</param>
    /// <returns>A triangle that contains all the points.</returns>
    public static Triangle GetBoundingTriangle(IEnumerable<Vector2> points, float marginFactor = 1f)
    {
        var bounds = GetBoundingBox(points);
        float dMax = bounds.Size.Max() * marginFactor; 
        var center = bounds.Center;
        var a = new Vector2(center.X, bounds.BottomLeft.Y + dMax);
        var b = new Vector2(center.X - dMax * 1.25f, bounds.TopLeft.Y - dMax / 4);
        var c = new Vector2(center.X + dMax * 1.25f, bounds.TopLeft.Y - dMax / 4);
        
        return new Triangle(a, b, c);
    }

    /// <summary>
    /// Gets the axis vectors (edge directions) of a polygon's segments.
    /// </summary>
    /// <param name="p">The polygon to analyze.</param>
    /// <param name="normalized">If true, returns normalized direction vectors;</param>
    /// <returns>A list of axis vectors for each segment.</returns>
    public static List<Vector2> GetSegmentAxis(Polygon p, bool normalized = false)
    {
        if (p.Count <= 1) return new();
        if (p.Count == 2)
        {
            return new() { p[1] - p[0] };
        }

        List<Vector2> axis = [];
        for (var i = 0; i < p.Count; i++)
        {
            var start = p[i];
            var end = p[(i + 1) % p.Count];
            var a = end - start;
            axis.Add(normalized ? a.Normalize() : a);
        }

        return axis;
    }

    /// <summary>
    /// Gets the axis vectors (edge directions) of a collection of segments.
    /// </summary>
    /// <param name="edges">The segments to analyze.</param>
    /// <param name="normalized">If true, returns normalized direction vectors;</param>
    /// <returns>A list of axis vectors for each segment.</returns>
    public static List<Vector2> GetSegmentAxis(Segments edges, bool normalized = false)
    {
        List<Vector2> axis = [];
        foreach (var seg in edges)
        {
            axis.Add(normalized ? seg.Dir : seg.Displacement);
        }

        return axis;
    }

    /// <summary>
    /// Creates a polygon from a set of relative points and a transform.
    /// </summary>
    /// <param name="relative">The relative points defining the shape.</param>
    /// <param name="transform">The transform to apply to each point.</param>
    /// <returns>A polygon with transformed points, or an empty polygon if fewer than 3 points.</returns>
    public static Polygon GetShape(Points relative, Transform2D transform)
    {
        if (relative.Count < 3) return [];
        Polygon shape = [];
        for (var i = 0; i < relative.Count; i++)
        {
            shape.Add(transform.ApplyTransformTo(relative[i]));
        }

        return shape;
    }

    /// <summary>
    /// Generates a random polygon in relative space.
    /// </summary>
    /// <param name="pointCount">Number of points (vertices) in the polygon. Must be at least 3.</param>
    /// <param name="minLength">Minimum distance from the origin for each point.</param>
    /// <param name="maxLength">Maximum distance from the origin for each point.</param>
    /// <returns>A randomly generated polygon.</returns>
    public static Polygon? GenerateRelative(int pointCount, float minLength, float maxLength)
    {
        if (pointCount < 3) return null;
        if (Math.Abs(minLength - maxLength) < ShapeMath.Epsilon) return null;
        if (minLength > maxLength)
        {
            //swap
            (minLength, maxLength) = (maxLength, minLength);
        }
        Polygon poly = [];
        float angleStep = ShapeMath.PI * 2.0f / pointCount;

        for (var i = 0; i < pointCount; i++)
        {
            float randLength = Rng.Instance.RandF(minLength, maxLength);
            var p = ShapeVec.Right().Rotate(-angleStep * i) * randLength;
            poly.Add(p);
        }

        return poly;
    }

    /// <summary>
    /// Generates a random polygon centered at a given position.
    /// </summary>
    /// <param name="center">The center position of the polygon.</param>
    /// <param name="pointCount">Number of points (vertices) in the polygon. Must be at least 3.</param>
    /// <param name="minLength">Minimum distance from the center for each point.</param>
    /// <param name="maxLength">Maximum distance from the center for each point.</param>
    /// <returns>A randomly generated polygon centered at the specified position.</returns>
    public static Polygon? Generate(Vector2 center, int pointCount, float minLength, float maxLength)
    {
        if (pointCount < 3) return null;
        if (Math.Abs(minLength - maxLength) < ShapeMath.Epsilon) return null;
        if (minLength > maxLength)
        {
            //swap
            (minLength, maxLength) = (maxLength, minLength);
        }
        Polygon points = [];
        float angleStep = ShapeMath.PI * 2.0f / pointCount;

        for (int i = 0; i < pointCount; i++)
        {
            float randLength = Rng.Instance.RandF(minLength, maxLength);
            var p = ShapeVec.Right().Rotate(-angleStep * i) * randLength;
            p += center;
            points.Add(p);
        }

        return points;
    }

    /// <summary>
    /// Generates a polygon around the given segment. Points are generated ccw around the segment beginning with the segment start.
    /// </summary>
    /// <param name="segment">The segment to build a polygon around.</param>
    /// <param name="magMin">The minimum perpendicular magnitude factor for generating a point. (0-1)</param>
    /// <param name="magMax">The maximum perpendicular magnitude factor for generating a point. (0-1)</param>
    /// <param name="minSectionLength">The minimum factor of the length between points along the line.(0-1)</param>
    /// <param name="maxSectionLength">The maximum factor of the length between points along the line.(0-1)</param>
    /// <returns>Returns the generated polygon.</returns>
    public static Polygon? Generate(Segment segment, float magMin = 0.1f, float magMax = 0.25f, float minSectionLength = 0.025f, float maxSectionLength = 0.1f)
    {
        if (segment.LengthSquared <= 0) return null;
        if (magMin <= 0 || magMax <= 0) return null;
        if (minSectionLength <= 0 || maxSectionLength <= 0) return null;
        if (magMin > magMax)
        {
            (magMin, magMax) = (magMax, magMin);
        }

        if (minSectionLength > maxSectionLength)
        {
            (minSectionLength, maxSectionLength) = (maxSectionLength, minSectionLength);       
        }
        Polygon poly = [segment.Start];
        var dir = segment.Dir;
        var dirRight = dir.GetPerpendicularRight();
        var dirLeft = dir.GetPerpendicularLeft();
        float len = segment.Length;
        float minSectionLengthSq = (minSectionLength * len) * (minSectionLength * len);
        var cur = segment.Start;
        while (true)
        {
            cur += dir * Rng.Instance.RandF(minSectionLength, maxSectionLength) * len;
            if ((cur - segment.End).LengthSquared() < minSectionLengthSq) break;
            poly.Add(cur + dirRight * Rng.Instance.RandF(magMin, magMax));
        }

        cur = segment.End;
        poly.Add(cur);
        while (true)
        {
            cur -= dir * Rng.Instance.RandF(minSectionLength, maxSectionLength) * len;
            if ((cur - segment.Start).LengthSquared() < minSectionLengthSq) break;
            poly.Add(cur + dirLeft * Rng.Instance.RandF(magMin, magMax));
        }

        return poly;
    }
    
}