using System.Numerics;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.RayDef;

public readonly partial struct Ray
{
    /// <summary>
    /// Determines whether a ray overlaps a segment defined by two points.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <returns>True if the ray overlaps the segment; otherwise, false.</returns>
    /// <remarks>
    /// Returns true if the ray and segment intersect at a point on both the ray and the segment.
    /// </remarks>
    public static bool OverlapRaySegment(Vector2 rayPoint, Vector2 rayDirection, Vector2 segmentStart, Vector2 segmentEnd)
    {
        float denominator = rayDirection.X * (segmentEnd.Y - segmentStart.Y) - rayDirection.Y * (segmentEnd.X - segmentStart.X);

        if (Math.Abs(denominator) < ShapeMath.EpsilonF)
        {
            return false;
        }

        var difference = segmentStart - rayPoint;
        float t = (difference.X * (segmentEnd.Y - segmentStart.Y) - difference.Y * (segmentEnd.X - segmentStart.X)) / denominator;
        float u = (difference.X * rayDirection.Y - difference.Y * rayDirection.X) / denominator;

        if (t >= 0 && u >= 0 && u <= 1)
        {
            return true;
        }

        return false;
    }
    /// <summary>
    /// Determines whether a ray overlaps a line defined by a point and direction.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="linePoint">A point on the line.</param>
    /// <param name="lineDirection">The direction vector of the line.</param>
    /// <returns>True if the ray overlaps the line; otherwise, false.</returns>
    /// <remarks>
    /// Returns true if the ray and line intersect at a point on the ray.
    /// </remarks>
    public static bool OverlapRayLine(Vector2 rayPoint, Vector2 rayDirection, Vector2 linePoint, Vector2 lineDirection)
    {
        float denominator = rayDirection.X * lineDirection.Y - rayDirection.Y * lineDirection.X;

        if (Math.Abs(denominator) < ShapeMath.EpsilonF)
        {
            return false;
        }

        var difference = linePoint - rayPoint;
        float t = (difference.X * lineDirection.Y - difference.Y * lineDirection.X) / denominator;

        if (t >= 0)
        {
            return true;
        }

        return false;
    }
    /// <summary>
    /// Determines whether two rays overlap (intersect at a point on both rays).
    /// </summary>
    /// <param name="ray1Point">The origin point of the first ray.</param>
    /// <param name="ray1Direction">The direction vector of the first ray.</param>
    /// <param name="ray2Point">The origin point of the second ray.</param>
    /// <param name="ray2Direction">The direction vector of the second ray.</param>
    /// <returns>True if the rays overlap; otherwise, false.</returns>
    /// <remarks>
    /// Returns true if the rays intersect at a point on both rays.
    /// </remarks>
    public static bool OverlapRayRay(Vector2 ray1Point, Vector2 ray1Direction, Vector2 ray2Point, Vector2 ray2Direction)
    {
        float denominator = ray1Direction.X * ray2Direction.Y - ray1Direction.Y * ray2Direction.X;

        if (Math.Abs(denominator) < ShapeMath.EpsilonF)
        {
            return false;
        }

        var difference = ray2Point - ray1Point;
        float t = (difference.X * ray2Direction.Y - difference.Y * ray2Direction.X) / denominator;
        float u = (difference.X * ray1Direction.Y - difference.Y * ray1Direction.X) / denominator;

        if (t >= 0 && u >= 0)
        {
            return true;
        }

        return false;
    }
    /// <summary>
    /// Determines whether a ray overlaps a circle.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <returns>True if the ray overlaps the circle; otherwise, false.</returns>
    /// <remarks>
    /// Returns true if the ray intersects the circle at one or two points.
    /// </remarks>
    public static bool OverlapRayCircle(Vector2 rayPoint, Vector2 rayDirection, Vector2 circleCenter, float circleRadius)
    {
        var toCircle = circleCenter - rayPoint;
        float projectionLength = Vector2.Dot(toCircle, rayDirection);
        var closestPoint = rayPoint + projectionLength * rayDirection;
        float distanceToCenter = Vector2.Distance(closestPoint, circleCenter);

        if (distanceToCenter < circleRadius)
        {
            var offset = (float)Math.Sqrt(circleRadius * circleRadius - distanceToCenter * distanceToCenter);
            var intersection1 = closestPoint - offset * rayDirection;
            var intersection2 = closestPoint + offset * rayDirection;

            if (Vector2.Dot(intersection1 - rayPoint, rayDirection) >= 0)
            {
                return true;
            }

            if (Vector2.Dot(intersection2 - rayPoint, rayDirection) >= 0)
            {
                return true;
            }
        }

        if (Math.Abs(distanceToCenter - circleRadius) < ShapeMath.EpsilonF)
        {
            if (Vector2.Dot(closestPoint - rayPoint, rayDirection) >= 0)
            {
                return true;
            }
        }

        return false;
    }
    /// <summary>
    /// Determines whether a ray overlaps a triangle defined by three points.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <returns>True if the ray overlaps the triangle; otherwise, false.</returns>
    /// <remarks>
    /// Returns true if the ray intersects the triangle at a point on the ray and inside the triangle.
    /// </remarks>
    public static bool OverlapRayTriangle(Vector2 rayPoint, Vector2 rayDirection, Vector2 a, Vector2 b, Vector2 c)
    {
        if (Triangle.ContainsTrianglePoint(a, b, c, rayPoint)) return true;

        var cp = IntersectRaySegment(rayPoint, rayDirection, a, b);
        if (cp.Valid) return true;

        cp = IntersectRaySegment(rayPoint, rayDirection, b, c);
        if (cp.Valid) return true;

        cp = IntersectRaySegment(rayPoint, rayDirection, c, a);
        if (cp.Valid) return true;

        return false;
    }
    /// <summary>
    /// Determines whether a ray overlaps a quadrilateral defined by four points.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="a">The first vertex of the quadrilateral.</param>
    /// <param name="b">The second vertex of the quadrilateral.</param>
    /// <param name="c">The third vertex of the quadrilateral.</param>
    /// <param name="d">The fourth vertex of the quadrilateral.</param>
    /// <returns>True if the ray overlaps the quadrilateral; otherwise, false.</returns>
    /// <remarks>
    /// Returns true if the ray intersects the quadrilateral at a point on the ray and inside the quadrilateral.
    /// </remarks>
    public static bool OverlapRayQuad(Vector2 rayPoint, Vector2 rayDirection, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        if (Quad.ContainsQuadPoint(a, b, c, d, rayPoint)) return true;

        var cp = IntersectRaySegment(rayPoint, rayDirection, a, b);
        if (cp.Valid) return true;

        cp = IntersectRaySegment(rayPoint, rayDirection, b, c);
        if (cp.Valid) return true;

        cp = IntersectRaySegment(rayPoint, rayDirection, c, d);
        if (cp.Valid) return true;

        cp = IntersectRaySegment(rayPoint, rayDirection, d, a);
        if (cp.Valid) return true;

        return false;
    }
    /// <summary>
    /// Determines whether a ray overlaps a rectangle defined by four points.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="a">The first vertex of the rectangle.</param>
    /// <param name="b">The second vertex of the rectangle.</param>
    /// <param name="c">The third vertex of the rectangle.</param>
    /// <param name="d">The fourth vertex of the rectangle.</param>
    /// <returns>True if the ray overlaps the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// Returns true if the ray intersects the rectangle at a point on the ray and inside the rectangle.
    /// </remarks>
    public static bool OverlapRayRect(Vector2 rayPoint, Vector2 rayDirection, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return OverlapRayQuad(rayPoint, rayDirection, a, b, c, d);
    }
    /// <summary>
    /// Determines whether a ray overlaps a polygon defined by a list of points.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="points">The list of points defining the polygon.</param>
    /// <returns>True if the ray overlaps the polygon; otherwise, false.</returns>
    /// <remarks>
    /// Returns true if the ray intersects the polygon at a point on the ray and inside the polygon.
    /// </remarks>
    public static bool OverlapRayPolygon(Vector2 rayPoint, Vector2 rayDirection, List<Vector2> points)
    {
        if (points.Count < 3) return false;
        if (Polygon.ContainsPoint(points, rayPoint)) return true;
        for (var i = 0; i < points.Count; i++)
        {
            var colPoint = IntersectRaySegment(rayPoint, rayDirection, points[i], points[(i + 1) % points.Count]);
            if (colPoint.Valid) return true;
        }

        return false;
    }
    /// <summary>
    /// Determines whether a ray overlaps a polyline defined by a list of points.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="points">The list of points defining the polyline.</param>
    /// <returns>True if the ray overlaps the polyline; otherwise, false.</returns>
    /// <remarks>
    /// Returns true if the ray intersects the polyline at a point on the ray and on the polyline.
    /// </remarks>
    public static bool OverlapRayPolyline(Vector2 rayPoint, Vector2 rayDirection, List<Vector2> points)
    {
        if (points.Count < 2) return false;
        for (var i = 0; i < points.Count - 1; i++)
        {
            var colPoint = IntersectRaySegment(rayPoint, rayDirection, points[i], points[i + 1]);
            if (colPoint.Valid) return true;
        }

        return false;
    }
    /// <summary>
    /// Determines whether a ray overlaps any of the segments in a list of segments.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="segments">The list of segments.</param>
    /// <returns>True if the ray overlaps any segment; otherwise, false.</returns>
    /// <remarks>
    /// Returns true if the ray intersects any segment at a point on the ray and on the segment.
    /// </remarks>
    public static bool OverlapRaySegments(Vector2 rayPoint, Vector2 rayDirection, List<Segment> segments)
    {
        if (segments.Count <= 0) return false;

        foreach (var seg in segments)
        {
            var result = IntersectRaySegment(rayPoint, rayDirection, seg.Start, seg.End);
            if (result.Valid) return true;
        }

        return false;
    }
}