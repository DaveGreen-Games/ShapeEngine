using System.Numerics;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.RayDef;

public readonly partial struct Ray
{
    public static bool OverlapRaySegment(Vector2 rayPoint, Vector2 rayDirection, Vector2 segmentStart, Vector2 segmentEnd)
    {
        float denominator = rayDirection.X * (segmentEnd.Y - segmentStart.Y) - rayDirection.Y * (segmentEnd.X - segmentStart.X);

        if (Math.Abs(denominator) < 1e-10)
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

    public static bool OverlapRayLine(Vector2 rayPoint, Vector2 rayDirection, Vector2 linePoint, Vector2 lineDirection)
    {
        float denominator = rayDirection.X * lineDirection.Y - rayDirection.Y * lineDirection.X;

        if (Math.Abs(denominator) < 1e-10)
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

    public static bool OverlapRayRay(Vector2 ray1Point, Vector2 ray1Direction, Vector2 ray2Point, Vector2 ray2Direction)
    {
        float denominator = ray1Direction.X * ray2Direction.Y - ray1Direction.Y * ray2Direction.X;

        if (Math.Abs(denominator) < 1e-10)
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

        if (Math.Abs(distanceToCenter - circleRadius) < 1e-10)
        {
            if (Vector2.Dot(closestPoint - rayPoint, rayDirection) >= 0)
            {
                return true;
            }
        }

        return false;
    }

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

    public static bool OverlapRayRect(Vector2 rayPoint, Vector2 rayDirection, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return OverlapRayQuad(rayPoint, rayDirection, a, b, c, d);
    }

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

    public static bool OverlapRayPolyline(Vector2 rayPoint, Vector2 rayDirection, List<Vector2> points)
    {
        if (points.Count < 3) return false;
        for (var i = 0; i < points.Count - 1; i++)
        {
            var colPoint = IntersectRaySegment(rayPoint, rayDirection, points[i], points[i + 1]);
            if (colPoint.Valid) return true;
        }

        return false;
    }

    public static bool OverlapRaySegments(Vector2 rayPoint, Vector2 rayDirection, List<SegmentDef.Segment> segments)
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