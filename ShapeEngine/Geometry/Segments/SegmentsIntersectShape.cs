using ShapeEngine.Geometry.CollisionSystem;

namespace ShapeEngine.Geometry.Segments;

public partial class Segments
{
    #region Intersection

    public CollisionPoints? IntersectShape(Ray.Ray r)
    {
        if (Count <= 0) return null;
        CollisionPoints? points = null;

        foreach (var seg in this)
        {
            var result = Segment.Segment.IntersectSegmentRay(seg.Start, seg.End, r.Point, r.Direction, r.Normal);
            if (result.Valid)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
        }

        return points;
    }

    public CollisionPoints? IntersectShape(Line.Line l)
    {
        if (Count <= 0) return null;
        CollisionPoints? points = null;

        foreach (var seg in this)
        {
            var result = Segment.Segment.IntersectSegmentLine(seg.Start, seg.End, l.Point, l.Direction, l.Normal);
            if (result.Valid)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
        }

        return points;
    }

    public CollisionPoints? IntersectShape(Segment.Segment s)
    {
        if (Count <= 0) return null;
        CollisionPoints? points = null;

        foreach (var seg in this)
        {
            var result = Segment.Segment.IntersectSegmentSegment(seg.Start, seg.End, s.Start, s.End);
            if (result.Valid)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
        }

        return points;
    }

    public CollisionPoints? IntersectShape(Circle.Circle c)
    {
        if (Count <= 0) return null;
        CollisionPoints? points = null;
        foreach (var seg in this)
        {
            var result = Segment.Segment.IntersectSegmentCircle(seg.Start, seg.End, c.Center, c.Radius);
            if (result.a.Valid || result.b.Valid)
            {
                points ??= new();
                if (result.a.Valid) points.Add(result.a);
                if (result.b.Valid) points.Add(result.b);
            }
        }

        return points;
    }

    public CollisionPoints? IntersectShape(Segments shape)
    {
        if (Count <= 0) return null;
        CollisionPoints? points = null;

        foreach (var seg in this)
        {
            foreach (var bSeg in shape)
            {
                var result = Segment.Segment.IntersectSegmentSegment(seg.Start, seg.End, bSeg.Start, bSeg.End);
                if (result.Valid)
                {
                    points ??= new();
                    points.AddRange(result);
                }
            }
        }

        return points;
    }

    public int IntersectShape(Ray.Ray r, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count <= 0) return 0;
        var count = 0;

        foreach (var seg in this)
        {
            var result = Segment.Segment.IntersectSegmentRay(seg.Start, seg.End, r.Point, r.Direction, r.Normal);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }

        return count;
    }

    public int IntersectShape(Line.Line l, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count <= 0) return 0;
        var count = 0;

        foreach (var seg in this)
        {
            var result = Segment.Segment.IntersectSegmentLine(seg.Start, seg.End, l.Point, l.Direction, l.Normal);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }

        return count;
    }

    public int IntersectShape(Segment.Segment s, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count <= 0) return 0;
        var count = 0;

        foreach (var seg in this)
        {
            var result = Segment.Segment.IntersectSegmentSegment(seg.Start, seg.End, s.Start, s.End);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }

        return count;
    }

    public int IntersectShape(Circle.Circle c, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count <= 0) return 0;
        var count = 0;
        foreach (var seg in this)
        {
            var result = Segment.Segment.IntersectSegmentCircle(seg.Start, seg.End, c.Center, c.Radius);
            if (result.a.Valid)
            {
                points.Add(result.a);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            if (result.b.Valid)
            {
                points.Add(result.b);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }

        return count;
    }

    public int IntersectShape(Segments shape, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count <= 0) return 0;
        var count = 0;

        foreach (var seg in this)
        {
            foreach (var bSeg in shape)
            {
                var result = Segment.Segment.IntersectSegmentSegment(seg.Start, seg.End, bSeg.Start, bSeg.End);
                if (result.Valid)
                {
                    points.Add(result);
                    if (returnAfterFirstValid) return 1;
                    count++;
                }
            }
        }

        return count;
    }

    #endregion
}