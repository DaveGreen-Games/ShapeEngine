using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.PolylineDef;

public partial class Polyline
{
    /// <summary>
    /// Determines if a polyline overlaps with a line segment.
    /// </summary>
    /// <param name="points">The list of points defining the polyline. Must contain at least two points.</param>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <returns>True if the polyline overlaps with the segment; otherwise, false.</returns>
    /// <remarks>
    /// This method checks if any segment of the polyline intersects with the given segment.
    /// </remarks>
    public static bool OverlapPolylineSegment(List<Vector2> points, Vector2 segmentStart, Vector2 segmentEnd)
    {
        return points.Count >= 2 && Segment.OverlapSegmentPolyline(segmentStart, segmentEnd, points);
    }

    /// <summary>
    /// Determines if a polyline overlaps with a line.
    /// </summary>
    /// <param name="points">The list of points defining the polyline. Must contain at least two points.</param>
    /// <param name="linePoint">A point on the line.</param>
    /// <param name="lineDirection">The direction vector of the line.</param>
    /// <returns>True if the polyline overlaps with the line; otherwise, false.</returns>
    /// <remarks>
    /// This method checks if any segment of the polyline intersects with the given infinite line.
    /// </remarks>
    public static bool OverlapPolylineLine(List<Vector2> points, Vector2 linePoint, Vector2 lineDirection)
    {
        return points.Count >= 2 && Line.OverlapLinePolyline(linePoint, lineDirection, points);
    }

    /// <summary>
    /// Determines if a polyline overlaps with a ray.
    /// </summary>
    /// <param name="points">The list of points defining the polyline. Must contain at least two points.</param>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <returns>True if the polyline overlaps with the ray; otherwise, false.</returns>
    /// <remarks>
    /// This method checks if any segment of the polyline intersects with the given ray.
    /// </remarks>
    public static bool OverlapPolylineRay(List<Vector2> points, Vector2 rayPoint, Vector2 rayDirection)
    {
        return points.Count >= 2 && Ray.OverlapRayPolyline(rayPoint, rayDirection, points);
    }

    /// <summary>
    /// Determines if a polyline overlaps with a circle.
    /// </summary>
    /// <param name="points">The list of points defining the polyline. Must contain at least two points.</param>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <returns>True if the polyline overlaps with the circle; otherwise, false.</returns>
    /// <remarks>
    /// This method checks if any segment of the polyline intersects with the given circle.
    /// </remarks>
    public static bool OverlapPolylineCircle(List<Vector2> points, Vector2 circleCenter, float circleRadius)
    {
        return points.Count >= 2 && Circle.OverlapCirclePolyline(circleCenter, circleRadius, points);
    }

    /// <summary>
    /// Determines if a polyline overlaps with a triangle.
    /// </summary>
    /// <param name="points">The list of points defining the polyline. Must contain at least two points.</param>
    /// <param name="ta">The first vertex of the triangle.</param>
    /// <param name="tb">The second vertex of the triangle.</param>
    /// <param name="tc">The third vertex of the triangle.</param>
    /// <returns>True if the polyline overlaps with the triangle; otherwise, false.</returns>
    /// <remarks>
    /// This method checks if any segment of the polyline intersects with the given triangle.
    /// </remarks>
    public static bool OverlapPolylineTriangle(List<Vector2> points, Vector2 ta, Vector2 tb, Vector2 tc)
    {
        return points.Count >= 2 && Triangle.OverlapTrianglePolyline(ta, tb, tc, points);
    }

    /// <summary>
    /// Determines if a polyline overlaps with a quadrilateral.
    /// </summary>
    /// <param name="points">The list of points defining the polyline. Must contain at least two points.</param>
    /// <param name="qa">The first vertex of the quadrilateral.</param>
    /// <param name="qb">The second vertex of the quadrilateral.</param>
    /// <param name="qc">The third vertex of the quadrilateral.</param>
    /// <param name="qd">The fourth vertex of the quadrilateral.</param>
    /// <returns>True if the polyline overlaps with the quadrilateral; otherwise, false.</returns>
    /// <remarks>
    /// This method checks if any segment of the polyline intersects with the given quadrilateral.
    /// </remarks>
    public static bool OverlapPolylineQuad(List<Vector2> points, Vector2 qa, Vector2 qb, Vector2 qc, Vector2 qd)
    {
        return points.Count >= 2 && Quad.OverlapQuadPolyline(qa, qb, qc, qd, points);
    }

    /// <summary>
    /// Determines if a polyline overlaps with a rectangle defined by four points.
    /// </summary>
    /// <param name="points">The list of points defining the polyline. Must contain at least two points.</param>
    /// <param name="ra">The first vertex of the rectangle.</param>
    /// <param name="rb">The second vertex of the rectangle.</param>
    /// <param name="rc">The third vertex of the rectangle.</param>
    /// <param name="rd">The fourth vertex of the rectangle.</param>
    /// <returns>True if the polyline overlaps with the rectangle; otherwise, false.</returns>
    /// <remarks>
    /// Checks if any segment of the polyline intersects with the specified rectangle.
    /// Internally uses <c>Quad.OverlapQuadPolyline</c> for compatibility with rotated rectangles.
    /// </remarks>
    public static bool OverlapPolylineRect(List<Vector2> points, Vector2 ra, Vector2 rb, Vector2 rc, Vector2 rd)
    {
        return points.Count >= 2 && Quad.OverlapQuadPolyline(ra, rb, rc, rd, points);
    }

    /// <summary>
    /// Determines if a polyline overlaps with a polygon.
    /// </summary>
    /// <param name="points1">The list of points defining the polyline. Must contain at least two points.</param>
    /// <param name="points2">The list of points defining the polygon. Must contain at least three points.</param>
    /// <returns>True if the polyline overlaps with the polygon; otherwise, false.</returns>
    /// <remarks>
    /// This method checks if any segment of the polyline intersects with the given polygon.
    /// </remarks>
    public static bool OverlapPolylinePolygon(List<Vector2> points1, List<Vector2> points2)
    {
        if (points1.Count < 2 || points2.Count < 3) return false;
        return Polygon.OverlapPolygonPolyline(points2, points1);
    }

    /// <summary>
    /// Determines if two polylines overlap.
    /// </summary>
    /// <param name="points1">The list of points defining the first polyline. Must contain at least two points.</param>
    /// <param name="points2">The list of points defining the second polyline. Must contain at least two points.</param>
    /// <returns>True if the polylines overlap; otherwise, false.</returns>
    /// <remarks>
    /// This method checks if any segment of the first polyline intersects with any segment of the second polyline.
    /// </remarks>
    public static bool OverlapPolylinePolyline(List<Vector2> points1, List<Vector2> points2)
    {
        if (points1.Count < 2 || points2.Count < 2) return false;

        for (var i = 0; i < points1.Count - 1; i++)
        {
            var start = points1[i];
            var end = points1[i + 1];

            for (var j = 0; j < points2.Count - 1; j++)
            {
                var bStart = points2[j];
                var bEnd = points2[j + 1];

                if (Segment.OverlapSegmentSegment(start, end, bStart, bEnd)) return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines if a polyline overlaps with a collection of segments.
    /// </summary>
    /// <param name="points">The list of points defining the polyline. Must contain at least two points.</param>
    /// <param name="segments">The list of segments to check for overlap.</param>
    /// <returns>True if the polyline overlaps with any of the segments; otherwise, false.</returns>
    /// <remarks>
    /// This method checks if any segment of the polyline intersects with any segment in the provided collection.
    /// </remarks>
    public static bool OverlapPolylineSegments(List<Vector2> points, List<Segment> segments)
    {
        if (points.Count < 2 || segments.Count <= 0) return false;

        for (var i = 0; i < points.Count - 1; i++)
        {
            var start = points[i];
            var end = points[i + 1];

            foreach (var seg in segments)
            {
                if (Segment.OverlapSegmentSegment(start, end, seg.Start, seg.End)) return true;
            }
        }

        return false;
    }

}