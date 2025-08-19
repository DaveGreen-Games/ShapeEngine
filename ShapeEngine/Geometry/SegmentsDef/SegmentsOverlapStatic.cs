using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.SegmentsDef;

public partial class Segments
{
    /// <summary>
    /// Checks if a list of segments overlaps with a segment.
    /// </summary>
    /// <param name="segments">The list of segments to check.</param>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <returns>True if an overlap is found; otherwise false.</returns>
    /// <remarks>This is a convenience method that calls Segment.OverlapSegmentSegments.</remarks>
    public static bool OverlapSegmentsSegment(List<Segment> segments, Vector2 segmentStart, Vector2 segmentEnd)
    {
        return Segment.OverlapSegmentSegments(segmentStart, segmentEnd, segments);
    }

    /// <summary>
    /// Checks if a list of segments overlaps with a line.
    /// </summary>
    /// <param name="segments">The list of segments to check.</param>
    /// <param name="linePoint">A point on the line.</param>
    /// <param name="lineDirection">The direction of the line.</param>
    /// <returns>True if an overlap is found; otherwise false.</returns>
    /// <remarks>This is a convenience method that calls Line.OverlapLineSegments.</remarks>
    public static bool OverlapSegmentsLine(List<Segment> segments, Vector2 linePoint, Vector2 lineDirection)
    {
        return Line.OverlapLineSegments(linePoint, lineDirection, segments);
    }

    /// <summary>
    /// Checks if a list of segments overlaps with a ray.
    /// </summary>
    /// <param name="segments">The list of segments to check.</param>
    /// <param name="rayPoint">The starting point of the ray.</param>
    /// <param name="rayDirection">The direction of the ray.</param>
    /// <returns>True if an overlap is found; otherwise false.</returns>
    /// <remarks>This is a convenience method that calls Ray.OverlapRaySegments.</remarks>
    public static bool OverlapSegmentsRay(List<Segment> segments, Vector2 rayPoint, Vector2 rayDirection)
    {
        return Ray.OverlapRaySegments(rayPoint, rayDirection, segments);
    }

    /// <summary>
    /// Checks if a list of segments overlaps with a circle.
    /// </summary>
    /// <param name="segments">The list of segments to check.</param>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <returns>True if an overlap is found; otherwise false.</returns>
    /// <remarks>This is a convenience method that calls Circle.OverlapCircleSegments.</remarks>
    public static bool OverlapSegmentsCircle(List<Segment> segments, Vector2 circleCenter, float circleRadius)
    {
        return Circle.OverlapCircleSegments(circleCenter, circleRadius, segments);
    }

    /// <summary>
    /// Checks if a list of segments overlaps with a triangle.
    /// </summary>
    /// <param name="segments">The list of segments to check.</param>
    /// <param name="ta">The first vertex of the triangle.</param>
    /// <param name="tb">The second vertex of the triangle.</param>
    /// <param name="tc">The third vertex of the triangle.</param>
    /// <returns>True if an overlap is found; otherwise false.</returns>
    /// <remarks>This is a convenience method that calls Triangle.OverlapTriangleSegments.</remarks>
    public static bool OverlapSegmentsTriangle(List<Segment> segments, Vector2 ta, Vector2 tb, Vector2 tc)
    {
        return Triangle.OverlapTriangleSegments(ta, tb, tc, segments);
    }

    /// <summary>
    /// Checks if a list of segments overlaps with a quad.
    /// </summary>
    /// <param name="segments">The list of segments to check.</param>
    /// <param name="qa">The first vertex of the quad.</param>
    /// <param name="qb">The second vertex of the quad.</param>
    /// <param name="qc">The third vertex of the quad.</param>
    /// <param name="qd">The fourth vertex of the quad.</param>
    /// <returns>True if an overlap is found; otherwise false.</returns>
    /// <remarks>This is a convenience method that calls Quad.OverlapQuadSegments.</remarks>
    public static bool OverlapSegmentsQuad(List<Segment> segments, Vector2 qa, Vector2 qb, Vector2 qc, Vector2 qd)
    {
        return Quad.OverlapQuadSegments(qa, qb, qc, qd, segments);
    }

    /// <summary>
    /// Checks if a list of segments overlaps with a rectangle.
    /// </summary>
    /// <param name="segments">The list of segments to check.</param>
    /// <param name="ra">The first vertex of the rectangle.</param>
    /// <param name="rb">The second vertex of the rectangle.</param>
    /// <param name="rc">The third vertex of the rectangle.</param>
    /// <param name="rd">The fourth vertex of the rectangle.</param>
    /// <returns>True if an overlap is found; otherwise false.</returns>
    /// <remarks>This is a convenience method that calls Quad.OverlapQuadSegments.</remarks>
    public static bool OverlapSegmentsRect(List<Segment> segments, Vector2 ra, Vector2 rb, Vector2 rc, Vector2 rd)
    {
        return Quad.OverlapQuadSegments(ra, rb, rc, rd, segments);
    }

    /// <summary>
    /// Checks if a list of segments overlaps with a polygon.
    /// </summary>
    /// <param name="segments">The list of segments to check.</param>
    /// <param name="points">The points of the polygon.</param>
    /// <returns>True if an overlap is found; otherwise false.</returns>
    /// <remarks>This is a convenience method that calls Polygon.OverlapPolygonSegments.</remarks>
    public static bool OverlapSegmentsPolygon(List<Segment> segments, List<Vector2> points)
    {
        return points.Count >= 3 && Polygon.OverlapPolygonSegments(points, segments);
    }

    /// <summary>
    /// Checks if a list of segments overlaps with a polyline.
    /// </summary>
    /// <param name="segments">The list of segments to check.</param>
    /// <param name="points">The points of the polyline.</param>
    /// <returns>True if an overlap is found; otherwise false.</returns>
    /// <remarks>This is a convenience method that calls Polyline.OverlapPolylineSegments.</remarks>
    public static bool OverlapSegmentsPolyline(List<Segment> segments, List<Vector2> points)
    {
        return points.Count >= 2 && Polyline.OverlapPolylineSegments(points, segments);
    }

    /// <summary>
    /// Checks if two lists of segments overlap.
    /// </summary>
    /// <param name="segments1">The first list of segments.</param>
    /// <param name="segments2">The second list of segments.</param>
    /// <returns>True if an overlap is found; otherwise false.</returns>
    /// <remarks>This method iterates through each pair of segments and checks for an overlap.</remarks>
    public static bool OverlapSegmentsSegments(List<Segment> segments1, List<Segment> segments2)
    {
        foreach (var seg in segments1)
        {
            foreach (var bSeg in segments2)
            {
                if (Segment.OverlapSegmentSegment(seg.Start, seg.End, bSeg.Start, bSeg.End)) return true;
            }
        }

        return false;
    }

}