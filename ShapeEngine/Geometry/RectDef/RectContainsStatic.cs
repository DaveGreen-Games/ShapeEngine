using System.Numerics;
using ShapeEngine.Geometry.SegmentDef;

namespace ShapeEngine.Geometry.RectDef;

public readonly partial struct Rect
{
    public static bool ContainsRectPoint(Vector2 topLeft, Vector2 bottomRight, Vector2 point)
    {
        var left = topLeft.X;
        var right = bottomRight.X;
        var top = topLeft.Y;
        var bottom = bottomRight.Y;
        return left <= point.X && right >= point.X && top <= point.Y && bottom >= point.Y;
    }

    public static bool ContainsRectPoints(Vector2 topLeft, Vector2 bottomRight, Vector2 u, Vector2 v)
    {
        var left = topLeft.X;
        var right = bottomRight.X;
        var top = topLeft.Y;
        var bottom = bottomRight.Y;

        return left <= u.X && right >= u.X && top <= u.Y && bottom >= u.Y &&
               left <= v.X && right >= v.X && top <= v.Y && bottom >= v.Y;
    }

    public static bool ContainsRectPoints(Vector2 topLeft, Vector2 bottomRight, Vector2 u, Vector2 v, Vector2 w)
    {
        var left = topLeft.X;
        var right = bottomRight.X;
        var top = topLeft.Y;
        var bottom = bottomRight.Y;

        return left <= u.X && right >= u.X && top <= u.Y && bottom >= u.Y &&
               left <= v.X && right >= v.X && top <= v.Y && bottom >= v.Y &&
               left <= w.X && right >= w.X && top <= w.Y && bottom >= w.Y;
    }

    public static bool ContainsRectPoints(Vector2 topLeft, Vector2 bottomRight, Vector2 u, Vector2 v, Vector2 w, Vector2 x)
    {
        var left = topLeft.X;
        var right = bottomRight.X;
        var top = topLeft.Y;
        var bottom = bottomRight.Y;

        return left <= u.X && right >= u.X && top <= u.Y && bottom >= u.Y &&
               left <= v.X && right >= v.X && top <= v.Y && bottom >= v.Y &&
               left <= w.X && right >= w.X && top <= w.Y && bottom >= w.Y &&
               left <= x.X && right >= x.X && top <= x.Y && bottom >= x.Y;
    }

    public static bool ContainsRectPoints(Vector2 topLeft, Vector2 bottomRight, List<Vector2> points)
    {
        if (points.Count <= 0) return false;

        var left = topLeft.X;
        var right = bottomRight.X;
        var top = topLeft.Y;
        var bottom = bottomRight.Y;

        foreach (var point in points)
        {
            var contains = left <= point.X && right >= point.X && top <= point.Y && bottom >= point.Y;
            if (!contains) return false;
        }

        return true;
    }

    public static bool ContainsRectSegment(Vector2 topLeft, Vector2 bottomRight, Vector2 segmentStart, Vector2 segmentEnd)
    {
        return ContainsRectPoints(topLeft, bottomRight, segmentStart, segmentEnd);
    }

    public static bool ContainsRectCircle(Vector2 topLeft, Vector2 bottomRight, Vector2 circleCenter, float circleRadius)
    {
        if (!ContainsRectPoint(topLeft, bottomRight, circleCenter)) return false;
        var a = topLeft;
        var b = new Vector2(topLeft.X, bottomRight.Y);
        var c = bottomRight;
        var d = new Vector2(bottomRight.X, topLeft.Y);
        var result = Segment.IntersectSegmentCircle(a, b, circleCenter, circleRadius);
        if (result.a.Valid || result.b.Valid) return false;

        result = Segment.IntersectSegmentCircle(b, c, circleCenter, circleRadius);
        if (result.a.Valid || result.b.Valid) return false;

        result = Segment.IntersectSegmentCircle(c, d, circleCenter, circleRadius);
        if (result.a.Valid || result.b.Valid) return false;

        result = Segment.IntersectSegmentCircle(d, a, circleCenter, circleRadius);
        if (result.a.Valid || result.b.Valid) return false;
        return true;
    }

    public static bool ContainsRectTriangle(Vector2 topLeft, Vector2 bottomRight, Vector2 tA, Vector2 tB, Vector2 tc)
    {
        return ContainsRectPoints(topLeft, bottomRight, tA, tB, tc);
    }

    public static bool ContainsRectQuad(Vector2 topLeft, Vector2 bottomRight, Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD)
    {
        return ContainsRectPoints(topLeft, bottomRight, qA, qB, qC, qD);
    }

    public static bool ContainsRectRect(Vector2 topLeft1, Vector2 bottomRight1, Vector2 topLeft2, Vector2 bottomRight2)
    {
        var x = topLeft1.X;
        var y = topLeft1.Y;
        var width = bottomRight1.X - x;
        var height = bottomRight1.Y - x;

        var otherX = topLeft2.X;
        var otherY = topLeft2.Y;
        var otherWidth = bottomRight2.X - otherX;
        var otherHeight = bottomRight2.Y - otherY;
        return (x <= otherX) && (otherX + otherWidth <= x + width) &&
               (y <= otherY) && (otherY + otherHeight <= y + height);
    }

    public static bool ContainsRectPolyline(Vector2 topLeft, Vector2 bottomRight, List<Vector2> polyline)
    {
        return ContainsRectPoints(topLeft, bottomRight, polyline);
    }

    public static bool ContainsRectPolygon(Vector2 topLeft, Vector2 bottomRight, List<Vector2> polygon)
    {
        return ContainsRectPoints(topLeft, bottomRight, polygon);
    }
}