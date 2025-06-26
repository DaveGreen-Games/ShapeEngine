using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.Random;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.PolygonDef;

public partial class Polygon
{
    /// <summary>
    /// This function intersects a ray with a polygon and returns all segments that lie inside the polygon.
    /// </summary>
    /// <param name="rayPoint"></param>
    /// <param name="rayDirection"></param>
    /// <returns></returns>
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
    /// This function intersects a ray with a polygon and returns all segments that lie inside the polygon.
    /// </summary>
    /// <param name="rayPoint"></param>
    /// <param name="rayDirection"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public int CutRayWithPolygon(Vector2 rayPoint, Vector2 rayDirection, ref List<Segment> result)
    {
        if (Count < 3) return 0;
        if (rayDirection.X == 0 && rayDirection.Y == 0) return 0;

        rayDirection = rayDirection.Normalize();
        var intersectionPoints = IntersectPolygonRay(this, rayPoint, rayDirection, ref collisionPointsReference);
        if (intersectionPoints < 2) return 0;

        int count = result.Count;
        collisionPointsReference.SortClosestFirst(rayPoint);

        for (int i = 0; i < collisionPointsReference.Count - 1; i += 2)
        {
            var segmentStart = collisionPointsReference[i].Point;
            var segmentEnd = collisionPointsReference[i + 1].Point;
            var segment = new Segment(segmentStart, segmentEnd);
            result.Add(segment);
        }

        collisionPointsReference.Clear();
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
    /// Triangulates a set of points. Only works with non self intersecting shapes.
    /// </summary>
    /// <param name="points">The points to triangulate. Can be any set of points. (polygons as well) </param>
    /// <returns></returns>
    public static Triangulation TriangulateDelaunay(IEnumerable<Vector2> points)
    {
        var enumerable = points.ToList();
        var supraTriangle = GetBoundingTriangle(enumerable, 2f);
        return TriangulateDelaunay(enumerable, supraTriangle);
    }

    /// <summary>
    /// Triangulates a set of points. Only works with non self intersecting shapes.
    /// </summary>
    /// <param name="points">The points to triangulate. Can be any set of points. (polygons as well) </param>
    /// <param name="supraTriangle">The triangle that encapsulates all the points.</param>
    /// <returns></returns>
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
    /// Get a rect that encapsulates all points.
    /// </summary>
    /// <param name="points"></param>
    /// <returns></returns>
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
    /// Get a triangle the encapsulates all points.
    /// </summary>
    /// <param name="points"></param>
    /// <param name="marginFactor"> A factor for scaling the final triangle.</param>
    /// <returns></returns>
    public static Triangle GetBoundingTriangle(IEnumerable<Vector2> points, float marginFactor = 1f)
    {
        var bounds = GetBoundingBox(points);
        float
            dMax = bounds.Size.Max() *
                   marginFactor; // SVec.Max(bounds.BottomRight - bounds.BottomLeft) + margin; //  Mathf.Max(bounds.maxX - bounds.minX, bounds.maxY - bounds.minY) * Margin;
        Vector2 center = bounds.Center;

        ////The float 0.866 is an arbitrary value determined for optimum supra triangle conditions.
        //float x1 = center.X - 0.866f * dMax;
        //float x2 = center.X + 0.866f * dMax;
        //float x3 = center.X;
        //
        //float y1 = center.Y - 0.5f * dMax;
        //float y2 = center.Y - 0.5f * dMax;
        //float y3 = center.Y + dMax;
        //
        //Vector2 a = new(x1, y1);
        //Vector2 b = new(x2, y2);
        //Vector2 c = new(x3, y3);

        Vector2 a = new Vector2(center.X, bounds.BottomLeft.Y + dMax);
        Vector2 b = new Vector2(center.X - dMax * 1.25f, bounds.TopLeft.Y - dMax / 4);
        Vector2 c = new Vector2(center.X + dMax * 1.25f, bounds.TopLeft.Y - dMax / 4);


        return new Triangle(a, b, c);
    }

    public static List<Vector2> GetSegmentAxis(Polygon p, bool normalized = false)
    {
        if (p.Count <= 1) return new();
        else if (p.Count == 2)
        {
            return new() { p[1] - p[0] };
        }

        List<Vector2> axis = new();
        for (int i = 0; i < p.Count; i++)
        {
            Vector2 start = p[i];
            Vector2 end = p[(i + 1) % p.Count];
            Vector2 a = end - start;
            axis.Add(normalized ? ShapeVec.Normalize(a) : a);
        }

        return axis;
    }

    public static List<Vector2> GetSegmentAxis(Segments edges, bool normalized = false)
    {
        List<Vector2> axis = new();
        foreach (var seg in edges)
        {
            axis.Add(normalized ? seg.Dir : seg.Displacement);
        }

        return axis;
    }

    public static Polygon GetShape(PointsDef.Points relative, Transform2D transform)
    {
        if (relative.Count < 3) return new();
        Polygon shape = new();
        for (int i = 0; i < relative.Count; i++)
        {
            shape.Add(transform.ApplyTransformTo(relative[i]));
            // shape.Add(pos + ShapeVec.Rotate(relative[i], rotRad) * scale);
        }

        return shape;
    }

    public static Polygon GenerateRelative(int pointCount, float minLength, float maxLength)
    {
        Polygon poly = new();
        float angleStep = ShapeMath.PI * 2.0f / pointCount;

        for (int i = 0; i < pointCount; i++)
        {
            float randLength = Rng.Instance.RandF(minLength, maxLength);
            var p = ShapeVec.Rotate(ShapeVec.Right(), -angleStep * i) * randLength;
            poly.Add(p);
        }

        return poly;
    }

    public static Polygon Generate(Vector2 center, int pointCount, float minLength, float maxLength)
    {
        Polygon points = new();
        float angleStep = ShapeMath.PI * 2.0f / pointCount;

        for (int i = 0; i < pointCount; i++)
        {
            float randLength = Rng.Instance.RandF(minLength, maxLength);
            Vector2 p = ShapeVec.Rotate(ShapeVec.Right(), -angleStep * i) * randLength;
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
    /// <returns>Returns the a generated polygon.</returns>
    public static Polygon Generate(Segment segment, float magMin = 0.1f, float magMax = 0.25f, float minSectionLength = 0.025f,
        float maxSectionLength = 0.1f)
    {
        Polygon poly = new() { segment.Start };
        var dir = segment.Dir;
        var dirRight = dir.GetPerpendicularRight();
        var dirLeft = dir.GetPerpendicularLeft();
        float len = segment.Length;
        float minSectionLengthSq = (minSectionLength * len) * (minSectionLength * len);
        Vector2 cur = segment.Start;
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