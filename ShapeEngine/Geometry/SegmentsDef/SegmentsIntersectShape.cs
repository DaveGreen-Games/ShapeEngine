using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.SegmentDef;

namespace ShapeEngine.Geometry.SegmentsDef;

public partial class Segments
{
    /// <summary>
    /// Intersects a ray with the segments.
    /// </summary>
    /// <param name="r">The ray to intersect with.</param>
    /// <returns>A list of collision points if there are any intersections, otherwise null.</returns>
    public CollisionPoints? IntersectShape(Ray r)
    {
        if (Count <= 0) return null;
        CollisionPoints? points = null;

        foreach (var seg in this)
        {
            var result = Segment.IntersectSegmentRay(seg.Start, seg.End, r.Point, r.Direction, r.Normal);
            if (result.Valid)
            {
                points ??= [];
                points.AddRange(result);
            }
        }

        return points;
    }

    /// <summary>
    /// Intersects a line with the segments.
    /// </summary>
    /// <param name="l">The line to intersect with.</param>
    /// <returns>A list of collision points if there are any intersections, otherwise null.</returns>
    public CollisionPoints? IntersectShape(Line l)
    {
        if (Count <= 0) return null;
        CollisionPoints? points = null;

        foreach (var seg in this)
        {
            var result = Segment.IntersectSegmentLine(seg.Start, seg.End, l.Point, l.Direction, l.Normal);
            if (result.Valid)
            {
                points ??= [];
                points.AddRange(result);
            }
        }

        return points;
    }

    /// <summary>
    /// Intersects a segment with the segments.
    /// </summary>
    /// <param name="s">The segment to intersect with.</param>
    /// <returns>A list of collision points if there are any intersections, otherwise null.</returns>
    public CollisionPoints? IntersectShape(Segment s)
    {
        if (Count <= 0) return null;
        CollisionPoints? points = null;

        foreach (var seg in this)
        {
            var result = Segment.IntersectSegmentSegment(seg.Start, seg.End, s.Start, s.End);
            if (result.Valid)
            {
                points ??= [];
                points.AddRange(result);
            }
        }

        return points;
    }

    /// <summary>
    /// Intersects a circle with the segments.
    /// </summary>
    /// <param name="c">The circle to intersect with.</param>
    /// <returns>A list of collision points if there are any intersections, otherwise null.</returns>
    public CollisionPoints? IntersectShape(Circle c)
    {
        if (Count <= 0) return null;
        CollisionPoints? points = null;
        foreach (var seg in this)
        {
            var result = Segment.IntersectSegmentCircle(seg.Start, seg.End, c.Center, c.Radius);
            if (result.a.Valid || result.b.Valid)
            {
                points ??= new();
                if (result.a.Valid) points.Add(result.a);
                if (result.b.Valid) points.Add(result.b);
            }
        }

        return points;
    }

    /// <summary>
    /// Intersects a set of segments with the segments.
    /// </summary>
    /// <param name="shape">The segments to intersect with.</param>
    /// <returns>A list of collision points if there are any intersections, otherwise null.</returns>
    public CollisionPoints? IntersectShape(Segments shape)
    {
        if (Count <= 0) return null;
        CollisionPoints? points = null;

        foreach (var seg in this)
        {
            foreach (var bSeg in shape)
            {
                var result = Segment.IntersectSegmentSegment(seg.Start, seg.End, bSeg.Start, bSeg.End);
                if (result.Valid)
                {
                    points ??= new();
                    points.AddRange(result);
                }
            }
        }

        return points;
    }

    /// <summary>
    /// Intersects a ray with the segments.
    /// </summary>
    /// <param name="r">The ray to intersect with.</param>
    /// <param name="points">The list of collision points to add to.</param>
    /// <param name="returnAfterFirstValid">If true, the method will return after the first valid intersection is found.</param>
    /// <returns>The number of intersections found.</returns>
    public int IntersectShape(Ray r, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count <= 0) return 0;
        var count = 0;

        foreach (var seg in this)
        {
            var result = Segment.IntersectSegmentRay(seg.Start, seg.End, r.Point, r.Direction, r.Normal);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Intersects a line with the segments.
    /// </summary>
    /// <param name="l">The line to intersect with.</param>
    /// <param name="points">The list of collision points to add to.</param>
    /// <param name="returnAfterFirstValid">If true, the method will return after the first valid intersection is found.</param>
    /// <returns>The number of intersections found.</returns>
    public int IntersectShape(Line l, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count <= 0) return 0;
        var count = 0;

        foreach (var seg in this)
        {
            var result = Segment.IntersectSegmentLine(seg.Start, seg.End, l.Point, l.Direction, l.Normal);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Intersects a segment with the segments.
    /// </summary>
    /// <param name="s">The segment to intersect with.</param>
    /// <param name="points">The list of collision points to add to.</param>
    /// <param name="returnAfterFirstValid">If true, the method will return after the first valid intersection is found.</param>
    /// <returns>The number of intersections found.</returns>
    public int IntersectShape(Segment s, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count <= 0) return 0;
        var count = 0;

        foreach (var seg in this)
        {
            var result = Segment.IntersectSegmentSegment(seg.Start, seg.End, s.Start, s.End);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Intersects a circle with the segments.
    /// </summary>
    /// <param name="c">The circle to intersect with.</param>
    /// <param name="points">The list of collision points to add to.</param>
    /// <param name="returnAfterFirstValid">If true, the method will return after the first valid intersection is found.</param>
    /// <returns>The number of intersections found.</returns>
    public int IntersectShape(Circle c, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count <= 0) return 0;
        var count = 0;
        foreach (var seg in this)
        {
            var result = Segment.IntersectSegmentCircle(seg.Start, seg.End, c.Center, c.Radius);
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

    /// <summary>
    /// Intersects a set of segments with the segments.
    /// </summary>
    /// <param name="shape">The segments to intersect with.</param>
    /// <param name="points">The list of collision points to add to.</param>
    /// <param name="returnAfterFirstValid">If true, the method will return after the first valid intersection is found.</param>
    /// <returns>The number of intersections found.</returns>
    public int IntersectShape(Segments shape, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count <= 0) return 0;
        var count = 0;

        foreach (var seg in this)
        {
            foreach (var bSeg in shape)
            {
                var result = Segment.IntersectSegmentSegment(seg.Start, seg.End, bSeg.Start, bSeg.End);
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

}