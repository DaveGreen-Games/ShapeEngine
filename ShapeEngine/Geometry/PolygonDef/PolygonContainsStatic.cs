using System.Numerics;

namespace ShapeEngine.Geometry.PolygonDef;

public partial class Polygon
{
    public static bool ContainsPoint(List<Vector2> polygon, Vector2 p)
    {
        var oddNodes = false;
        int num = polygon.Count;
        int j = num - 1;
        for (int i = 0; i < num; i++)
        {
            var vi = polygon[i];
            var vj = polygon[j];
            if (ContainsPointCheck(vi, vj, p)) oddNodes = !oddNodes;
            j = i;
        }

        return oddNodes;
    }

    public static bool ContainsPoints(List<Vector2> polygon, Vector2 a, Vector2 b)
    {
        var oddNodesA = false;
        var oddNodesB = false;
        int num = polygon.Count;
        int j = num - 1;
        for (var i = 0; i < num; i++)
        {
            var vi = polygon[i];
            var vj = polygon[j];
            if (ContainsPointCheck(vi, vj, a)) oddNodesA = !oddNodesA;
            if (ContainsPointCheck(vi, vj, b)) oddNodesB = !oddNodesB;

            j = i;
        }

        return oddNodesA && oddNodesB;
    }

    public static bool ContainsPoints(List<Vector2> polygon, Vector2 a, Vector2 b, Vector2 c)
    {
        var oddNodesA = false;
        var oddNodesB = false;
        var oddNodesC = false;
        int num = polygon.Count;
        int j = num - 1;
        for (int i = 0; i < num; i++)
        {
            var vi = polygon[i];
            var vj = polygon[j];
            if (ContainsPointCheck(vi, vj, a)) oddNodesA = !oddNodesA;
            if (ContainsPointCheck(vi, vj, b)) oddNodesB = !oddNodesB;
            if (ContainsPointCheck(vi, vj, c)) oddNodesC = !oddNodesC;

            j = i;
        }

        return oddNodesA && oddNodesB && oddNodesC;
    }

    public static bool ContainsPoints(List<Vector2> polygon, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        var oddNodesA = false;
        var oddNodesB = false;
        var oddNodesC = false;
        var oddNodesD = false;
        int num = polygon.Count;
        int j = num - 1;
        for (int i = 0; i < num; i++)
        {
            var vi = polygon[i];
            var vj = polygon[j];
            if (ContainsPointCheck(vi, vj, a)) oddNodesA = !oddNodesA;
            if (ContainsPointCheck(vi, vj, b)) oddNodesB = !oddNodesB;
            if (ContainsPointCheck(vi, vj, c)) oddNodesC = !oddNodesC;
            if (ContainsPointCheck(vi, vj, d)) oddNodesD = !oddNodesD;

            j = i;
        }

        return oddNodesA && oddNodesB && oddNodesC && oddNodesD;
    }

    public static bool ContainsPoints(List<Vector2> polygon, List<Vector2> points)
    {
        if (polygon.Count <= 0 || points.Count <= 0) return false;
        foreach (var p in points)
        {
            if (!ContainsPoint(polygon, p)) return false;
        }

        return true;
    }

    public static bool ContainsPolygonSegment(List<Vector2> polygon, Vector2 segmentStart, Vector2 segmentEnd)
    {
        if (!ContainsPoints(polygon, segmentStart, segmentEnd)) return false;

        for (int i = 0; i < polygon.Count; i++)
        {
            var polyStart = polygon[i];
            var polyEnd = polygon[(i + 1) % polygon.Count];
            if (SegmentDef.Segment.IntersectSegmentSegment(segmentStart, segmentEnd, polyStart, polyEnd).Valid)
            {
                return false;
            }
        }

        return true;
    }

    public static bool ContainsPolygonCircle(List<Vector2> polygon, Vector2 circleCenter, float circleRadius)
    {
        if (!ContainsPoint(polygon, circleCenter)) return false;

        for (int i = 0; i < polygon.Count; i++)
        {
            var polyStart = polygon[i];
            var polyEnd = polygon[(i + 1) % polygon.Count];
            var result = SegmentDef.Segment.IntersectSegmentCircle(polyStart, polyEnd, circleCenter, circleRadius);
            if (result.a.Valid || result.b.Valid)
            {
                return false;
            }
        }

        return true;
    }

    public static bool ContainsPolygonTriangle(List<Vector2> polygon, Vector2 a, Vector2 b, Vector2 c)
    {
        if (!ContainsPoints(polygon, a, b, c)) return false;

        for (int i = 0; i < polygon.Count; i++)
        {
            var polyStart = polygon[i];
            var polyEnd = polygon[(i + 1) % polygon.Count];
            if (SegmentDef.Segment.IntersectSegmentSegment(polyStart, polyEnd, a, b).Valid)
            {
                return false;
            }

            if (SegmentDef.Segment.IntersectSegmentSegment(polyStart, polyEnd, b, c).Valid)
            {
                return false;
            }

            if (SegmentDef.Segment.IntersectSegmentSegment(polyStart, polyEnd, c, a).Valid)
            {
                return false;
            }
        }

        return true;
    }

    public static bool ContainsPolygonQuad(List<Vector2> polygon, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        if (!ContainsPoints(polygon, a, b, c, d)) return false;

        for (int i = 0; i < polygon.Count; i++)
        {
            var polyStart = polygon[i];
            var polyEnd = polygon[(i + 1) % polygon.Count];
            if (SegmentDef.Segment.IntersectSegmentSegment(polyStart, polyEnd, a, b).Valid)
            {
                return false;
            }

            if (SegmentDef.Segment.IntersectSegmentSegment(polyStart, polyEnd, b, c).Valid)
            {
                return false;
            }

            if (SegmentDef.Segment.IntersectSegmentSegment(polyStart, polyEnd, c, d).Valid)
            {
                return false;
            }

            if (SegmentDef.Segment.IntersectSegmentSegment(polyStart, polyEnd, d, a).Valid)
            {
                return false;
            }
        }

        return true;
    }

    public static bool ContainsPolygonRect(List<Vector2> polygon, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return ContainsPolygonQuad(polygon, a, b, c, d);
    }

    public static bool ContainsPolygonPolyline(List<Vector2> polygon, List<Vector2> polyline)
    {
        if (!ContainsPoints(polygon, polyline)) return false;

        for (int i = 0; i < polygon.Count; i++)
        {
            var polyStart = polygon[i];
            var polyEnd = polygon[(i + 1) % polygon.Count];
            for (int j = 0; j < polyline.Count - 1; j++)
            {
                var polylineStart = polyline[j];
                var polylineEnd = polyline[j + 1];

                if (SegmentDef.Segment.IntersectSegmentSegment(polyStart, polyEnd, polylineStart, polylineEnd).Valid)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public static bool ContainsPolygonPolygon(List<Vector2> polygon, List<Vector2> other)
    {
        if (!ContainsPoints(polygon, other)) return false;

        for (int i = 0; i < polygon.Count; i++)
        {
            var polyStart = polygon[i];
            var polyEnd = polygon[(i + 1) % polygon.Count];
            for (int j = 0; j < other.Count; j++)
            {
                var polylineStart = other[j];
                var polylineEnd = other[(j + 1) % other.Count];

                if (SegmentDef.Segment.IntersectSegmentSegment(polyStart, polyEnd, polylineStart, polylineEnd).Valid)
                {
                    return false;
                }
            }
        }

        return true;
    }

}