using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.PolygonDef;

public partial class Polygon
{
    /// <summary>
    /// Checks if a polygon (defined by a list of points) overlaps with a line segment.
    /// </summary>
    /// <param name="points">The vertices of the polygon.</param>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <returns>True if the segment overlaps the polygon; otherwise, false.</returns>
    public static bool OverlapPolygonSegment(List<Vector2> points, Vector2 segmentStart, Vector2 segmentEnd)
    {
        if (points.Count < 3) return false; // Polygon must have at least 3 points
        return Segment.OverlapSegmentPolygon(segmentStart, segmentEnd, points);
    }

    /// <summary>
    /// Checks if a polygon overlaps with a line.
    /// </summary>
    /// <param name="points">The vertices of the polygon.</param>
    /// <param name="linePoint">A point on the line.</param>
    /// <param name="lineDirection">The direction vector of the line.</param>
    /// <returns>True if the line overlaps the polygon; otherwise, false.</returns>
    /// <remarks>The Line is infinite in both directions.</remarks>
    public static bool OverlapPolygonLine(List<Vector2> points, Vector2 linePoint, Vector2 lineDirection)
    {
        if (points.Count < 3) return false; // Polygon must have at least 3 points
        return Line.OverlapLinePolygon(linePoint, lineDirection, points);
    }

    /// <summary>
    /// Checks if a polygon overlaps with a ray.
    /// </summary>
    /// <param name="points">The vertices of the polygon.</param>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <returns>True if the ray overlaps the polygon; otherwise, false.</returns>
    /// <remarks>Ray is infinite in one direction from the origin.</remarks>
    public static bool OverlapPolygonRay(List<Vector2> points, Vector2 rayPoint, Vector2 rayDirection)
    {
        if (points.Count < 3) return false; // Polygon must have at least 3 points
        return Ray.OverlapRayPolygon(rayPoint, rayDirection, points);
    }

    /// <summary>
    /// Checks if a polygon overlaps with a circle.
    /// </summary>
    /// <param name="points">The vertices of the polygon.</param>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <returns>True if the circle overlaps the polygon; otherwise, false.</returns>
    public static bool OverlapPolygonCircle(List<Vector2> points, Vector2 circleCenter, float circleRadius)
    {
        if (points.Count < 3) return false; // Polygon must have at least 3 points
        return Circle.OverlapCirclePolygon(circleCenter, circleRadius, points);
    }

    /// <summary>
    /// Checks if a polygon overlaps with a triangle.
    /// </summary>
    /// <param name="points">The vertices of the polygon.</param>
    /// <param name="ta">First vertex of the triangle.</param>
    /// <param name="tb">Second vertex of the triangle.</param>
    /// <param name="tc">Third vertex of the triangle.</param>
    /// <returns>True if the triangle overlaps the polygon; otherwise, false.</returns>
    public static bool OverlapPolygonTriangle(List<Vector2> points, Vector2 ta, Vector2 tb, Vector2 tc)
    {
        if (points.Count < 3) return false; // Polygon must have at least 3 points
        return Triangle.OverlapTrianglePolygon(ta, tb, tc, points);
    }

    /// <summary>
    /// Checks if a polygon overlaps with a quadrilateral.
    /// </summary>
    /// <param name="points">The vertices of the polygon.</param>
    /// <param name="qa">First vertex of the quad.</param>
    /// <param name="qb">Second vertex of the quad.</param>
    /// <param name="qc">Third vertex of the quad.</param>
    /// <param name="qd">Fourth vertex of the quad.</param>
    /// <returns>True if the quad overlaps the polygon; otherwise, false.</returns>
    public static bool OverlapPolygonQuad(List<Vector2> points, Vector2 qa, Vector2 qb, Vector2 qc, Vector2 qd)
    {
        if (points.Count < 3) return false; // Polygon must have at least 3 points
        return Quad.OverlapQuadPolygon(qa, qb, qc, qd, points);
    }

    /// <summary>
    /// Checks if a polygon overlaps with a rectangle (represented by four points).
    /// </summary>
    /// <param name="points">The vertices of the polygon.</param>
    /// <param name="ra">First vertex of the rectangle.</param>
    /// <param name="rb">Second vertex of the rectangle.</param>
    /// <param name="rc">Third vertex of the rectangle.</param>
    /// <param name="rd">Fourth vertex of the rectangle.</param>
    /// <returns>True if the rectangle overlaps the polygon; otherwise, false.</returns>
   /// <remarks>
   /// The rectangle is treated as a quadrilateral for overlap checks.
   /// Internally, this method calls <see cref="Quad.OverlapQuadPolygon"/>.
   /// This allows rectangles of any orientation (not just axis-aligned) to be handled correctly.
   /// </remarks>
    public static bool OverlapPolygonRect(List<Vector2> points, Vector2 ra, Vector2 rb, Vector2 rc, Vector2 rd)
    {
        if (points.Count < 3) return false; // Polygon must have at least 3 points
        return Quad.OverlapQuadPolygon(ra, rb, rc, rd, points);
    }

    /// <summary>
    /// Checks if two polygons overlap.
    /// </summary>
    /// <param name="points1">The vertices of the first polygon.</param>
    /// <param name="points2">The vertices of the second polygon.</param>
    /// <returns>True if the polygons overlap; otherwise, false.</returns>
    public static bool OverlapPolygonPolygon(List<Vector2> points1, List<Vector2> points2)
    {
        if (points1.Count < 3 || points2.Count < 3) return false;

        var oddNodes1 = false;
        var oddNodes2 = false;
        var containsPoints2CheckFinished = false;

        var pointToCeck1 = points1[0];
        var pointToCeck2 = points2[0];

        for (var i = 0; i < points1.Count; i++)
        {
            var start1 = points1[i];
            var end1 = points1[(i + 1) % points1.Count];

            for (var j = 0; j < points2.Count; j++)
            {
                var start2 = points2[j];
                var end2 = points2[(j + 1) % points2.Count];
                if (Segment.OverlapSegmentSegment(start1, end1, start2, end2)) return true;

                if (containsPoints2CheckFinished) continue;
                if (Polygon.ContainsPointCheck(start2, end2, pointToCeck1)) oddNodes2 = !oddNodes2;
            }

            if (!containsPoints2CheckFinished)
            {
                if (oddNodes2) return true;
                containsPoints2CheckFinished = true;
            }

            if (Polygon.ContainsPointCheck(start1, end1, pointToCeck2)) oddNodes1 = !oddNodes1;
        }

        return oddNodes1 || oddNodes2;
    }

    /// <summary>
    /// Checks if a polygon overlaps with a polyline.
    /// </summary>
    /// <param name="points1">The vertices of the polygon.</param>
    /// <param name="points2">The vertices of the polyline.</param>
    /// <returns>True if the polygon overlaps the polyline; otherwise, false.</returns>
    public static bool OverlapPolygonPolyline(List<Vector2> points1, List<Vector2> points2)
    {
        if (points1.Count < 3 || points2.Count < 2) return false;

        var oddNodes = false;
        var pointToCeck = points2[0];


        for (var i = 0; i < points1.Count; i++)
        {
            var start1 = points1[i];
            var end1 = points1[(i + 1) % points1.Count];

            for (var j = 0; j < points2.Count - 1; j++)
            {
                var start2 = points2[j];
                var end2 = points2[j + 1];
                if (Segment.OverlapSegmentSegment(start1, end1, start2, end2)) return true;
            }

            if (Polygon.ContainsPointCheck(start1, end1, pointToCeck)) oddNodes = !oddNodes;
        }

        return oddNodes;
    }

    /// <summary>
    /// Checks if a polygon overlaps with a list of segments.
    /// </summary>
    /// <param name="points">The vertices of the polygon.</param>
    /// <param name="segments">The list of segments.</param>
    /// <returns>True if the polygon overlaps any of the segments; otherwise, false.</returns>
    public static bool OverlapPolygonSegments(List<Vector2> points, List<Segment> segments)
    {
        if (points.Count < 3 || segments.Count <= 0) return false;

        var oddNodes = false;
        var pointToCeck = segments[0].Start;


        for (var i = 0; i < points.Count; i++)
        {
            var start = points[i];
            var end = points[(i + 1) % points.Count];

            foreach (var seg in segments)
            {
                if (Segment.OverlapSegmentSegment(start, end, seg.Start, seg.End)) return true;
            }

            if (Polygon.ContainsPointCheck(start, end, pointToCeck)) oddNodes = !oddNodes;
        }

        return oddNodes;
    }

}