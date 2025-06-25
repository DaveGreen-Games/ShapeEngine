using System.Numerics;
using ShapeEngine.StaticLib;
using ShapeEngine.Core.CollisionSystem;
namespace ShapeEngine.Geometry.Line;

/// <summary>
/// Provides static methods for determining overlap (intersection or collinearity) between an infinite line and various geometric shapes,
/// such as segments, lines, rays, circles, triangles, quadrilaterals, rectangles, polygons, polylines, and collections of segments.
/// </summary>
public static class LineOverlap
{
    /// <summary>
    /// Determines whether an infinite line and a finite segment overlap (i.e., intersect at any point).
    /// </summary>
    /// <param name="linePoint">A point through which the infinite line passes.</param>
    /// <param name="lineDirection">The direction vector of the infinite line (does not need to be normalized).</param>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <returns>True if the line and segment overlap (intersect); otherwise, false.</returns>
    /// <remarks>
    /// This method checks if the infinite line crosses the segment at any point, including at the endpoints.
    /// </remarks>
    public static bool OverlapLineSegment(Vector2 linePoint, Vector2 lineDirection, Vector2 segmentStart, Vector2 segmentEnd)
    {
        // Line AB (infinite line) represented by linePoint and lineDirection
        // Line segment CD represented by segmentStart and segmentEnd

        // Calculate direction vector of the segment
        var segmentDirection = segmentEnd - segmentStart;

        // Calculate the denominator of the intersection formula
        float denominator = lineDirection.X * segmentDirection.Y - lineDirection.Y * segmentDirection.X;

        // Check if lines are parallel (denominator is zero)
        if (Math.Abs(denominator) < 1e-10)
        {
            return false;
        }

        // Calculate the intersection point using parameter t
        var difference = segmentStart - linePoint;
        float t = (difference.X * segmentDirection.Y - difference.Y * segmentDirection.X) / denominator;

        // Calculate the intersection point
        var intersection = linePoint + t * lineDirection;
        
        return Segment.Segment.IsPointOnSegment(intersection, segmentStart, segmentEnd);
    }

    /// <summary>
    /// Determines whether two infinite lines overlap (i.e., are collinear).
    /// </summary>
    /// <param name="line1Point">A point on the first line.</param>
    /// <param name="line1Direction">The direction vector of the first line.</param>
    /// <param name="line2Point">A point on the second line.</param>
    /// <param name="line2Direction">The direction vector of the second line.</param>
    /// <returns>True if the lines overlap; otherwise, false.</returns>
    /// <remarks>
    /// This function checks for collinearity by comparing the cross product of the direction vectors.
    /// </remarks>
    public static bool OverlapLineLine(Vector2 line1Point, Vector2 line1Direction, Vector2 line2Point, Vector2 line2Direction)
    {
        // Calculate the denominator of the intersection formula
        float denominator = line1Direction.X * line2Direction.Y - line1Direction.Y * line2Direction.X;

        // Check if lines are parallel (denominator is zero)
        if (Math.Abs(denominator) < 1e-10)
        {
            return false;
        }
        // Calculate the intersection point using parameter t
        var difference = line2Point - line1Point;
        float t = (difference.X * line2Direction.Y - difference.Y * line2Direction.X) / denominator;

        return true;
    }

    /// <summary>
    /// Determines whether an infinite line and a ray overlap (i.e., are collinear and extend in the same direction).
    /// </summary>
    /// <param name="linePoint">A point on the infinite line.</param>
    /// <param name="lineDirection">The direction vector of the infinite line.</param>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <returns>True if the line and ray overlap; otherwise, false.</returns>
    /// <remarks>
    /// The function checks for collinearity and ensures the ray extends in the same direction as the line.
    /// </remarks>
    public static bool OverlapLineRay(Vector2 linePoint, Vector2 lineDirection, Vector2 rayPoint, Vector2 rayDirection)
    {
        // Calculate the denominator of the intersection formula
        float denominator = lineDirection.X * rayDirection.Y - lineDirection.Y * rayDirection.X;

        // Check if lines are parallel (denominator is zero)
        if (Math.Abs(denominator) < 1e-10)
        {
            return false;
        }

        var difference = rayPoint - linePoint;
        float u = (difference.X * lineDirection.Y - difference.Y * lineDirection.X) / denominator;

        return u >= 0;
    }

    /// <summary>
    /// Determines whether an infinite line and a circle overlap (i.e., the line passes through or touches the circle).
    /// </summary>
    /// <param name="linePoint">A point on the infinite line.</param>
    /// <param name="lineDirection">The direction vector of the infinite line.</param>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <returns>True if the line and circle overlap; otherwise, false.</returns>
    public static bool OverlapLineCircle(Vector2 linePoint, Vector2 lineDirection, Vector2 circleCenter, float circleRadius)
    {
        // Normalize the direction vector
        if (Circle.Circle.ContainsCirclePoint(circleCenter, circleRadius, linePoint)) return true;

        lineDirection = lineDirection.Normalize();

        // Vector from the line point to the circle center
        var toCircle = circleCenter - linePoint;
        
        // Projection of toCircle onto the line direction to find the closest approach
        float projectionLength = Vector2.Dot(toCircle, lineDirection);

        // Closest point on the line to the circle center
        var closestPoint = linePoint + projectionLength * lineDirection;

        // Distance from the closest point to the circle center
        float distanceToCenter = Vector2.Distance(closestPoint, circleCenter);

        // Check if the line intersects the circle
        if (distanceToCenter < circleRadius) return true;

        if (Math.Abs(distanceToCenter - circleRadius) < 1e-10) return true;

        return false;
    }

    /// <summary>
    /// Determines whether an infinite line and a triangle overlap (i.e., the line passes through or touches the triangle).
    /// </summary>
    /// <param name="linePoint">A point on the infinite line.</param>
    /// <param name="lineDirection">The direction vector of the infinite line.</param>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <returns>True if the line and triangle overlap; otherwise, false.</returns>
    public static bool OverlapLineTriangle(Vector2 linePoint, Vector2 lineDirection, Vector2 a, Vector2 b, Vector2 c)
    {
        if (Triangle.Triangle.ContainsTrianglePoint(a, b, c, linePoint)) return true;
        
        var cp = LineIntersection.IntersectLineSegment(linePoint, lineDirection,  a, b);
        if (cp.Valid) return true;
        
        cp = LineIntersection.IntersectLineSegment(linePoint, lineDirection,  b, c);
        if (cp.Valid) return true;
       
        cp= LineIntersection.IntersectLineSegment(linePoint, lineDirection,  c, a);
        if (cp.Valid) return true;

        return false;
    }

    /// <summary>
    /// Determines whether an infinite line and a quadrilateral overlap (i.e., the line passes through or touches the quadrilateral).
    /// </summary>
    /// <param name="linePoint">A point on the infinite line.</param>
    /// <param name="lineDirection">The direction vector of the infinite line.</param>
    /// <param name="a">The first vertex of the quadrilateral.</param>
    /// <param name="b">The second vertex of the quadrilateral.</param>
    /// <param name="c">The third vertex of the quadrilateral.</param>
    /// <param name="d">The fourth vertex of the quadrilateral.</param>
    /// <returns>True if the line and quadrilateral overlap; otherwise, false.</returns>
    public static bool OverlapLineQuad(Vector2 linePoint, Vector2 lineDirection, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        if (Quad.Quad.ContainsQuadPoint(a, b, c, d,  linePoint)) return true;
        
        var cp = LineIntersection.IntersectLineSegment(linePoint, lineDirection,  a, b);
        if (cp.Valid) return true;
        
        cp = LineIntersection.IntersectLineSegment(linePoint, lineDirection,  b, c);
        if (cp.Valid) return true;
       
        cp = LineIntersection.IntersectLineSegment(linePoint, lineDirection,  c, d);
        if (cp.Valid) return true;

        cp = LineIntersection.IntersectLineSegment(linePoint, lineDirection,  d, a);
        if (cp.Valid) return true;
        
        return false;
    }

    /// <summary>
    /// Determines whether an infinite line and a rectangle overlap (i.e., the line passes through or touches the rectangle).
    /// </summary>
    /// <param name="linePoint">A point on the infinite line.</param>
    /// <param name="lineDirection">The direction vector of the infinite line.</param>
    /// <param name="a">The first vertex of the rectangle.</param>
    /// <param name="b">The second vertex of the rectangle.</param>
    /// <param name="c">The third vertex of the rectangle.</param>
    /// <param name="d">The fourth vertex of the rectangle.</param>
    /// <returns>True if the line and rectangle overlap; otherwise, false.</returns>
    public static bool OverlapLineRect(Vector2 linePoint, Vector2 lineDirection, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return OverlapLineQuad(linePoint, lineDirection, a, b, c, d);
    }

    /// <summary>
    /// Determines whether an infinite line and a polygon overlap (i.e., the line passes through or touches the polygon).
    /// </summary>
    /// <param name="linePoint">A point on the infinite line.</param>
    /// <param name="lineDirection">The direction vector of the infinite line.</param>
    /// <param name="points">A list of vertices defining the polygon. The polygon is assumed to be closed and non-self-intersecting.</param>
    /// <returns>True if the line and polygon overlap; otherwise, false.</returns>
    public static bool OverlapLinePolygon(Vector2 linePoint, Vector2 lineDirection, List<Vector2> points)
    {
        if (points.Count < 3) return false;
        if (Polygon.Polygon.ContainsPoint(points, linePoint)) return true;
        for (var i = 0; i < points.Count; i++)
        {
            var colPoint = LineIntersection.IntersectLineSegment(linePoint, lineDirection, points[i], points[(i + 1) % points.Count]);
            if (colPoint.Valid) return true;
        }
        return false;
    }

    /// <summary>
    /// Determines whether an infinite line and a polyline overlap (i.e., the line passes through or touches any segment of the polyline).
    /// </summary>
    /// <param name="linePoint">A point on the infinite line.</param>
    /// <param name="lineDirection">The direction vector of the infinite line.</param>
    /// <param name="points">A list of vertices defining the polyline. The polyline is assumed to be open and non-self-intersecting.</param>
    /// <returns>True if the line and polyline overlap; otherwise, false.</returns>
    public static bool OverlapLinePolyline(Vector2 linePoint, Vector2 lineDirection, List<Vector2> points)
    {
        if (points.Count < 3) return false;
        for (var i = 0; i < points.Count - 1; i++)
        {
            var colPoint = LineIntersection.IntersectLineSegment(linePoint, lineDirection, points[i], points[i + 1]);
            if (colPoint.Valid) return true;
        }
        return false;
    }

    /// <summary>
    /// Determines whether an infinite line overlaps (intersects) with any segment in a list of segments.
    /// </summary>
    /// <param name="linePoint">A point on the infinite line.</param>
    /// <param name="lineDirection">The direction vector of the infinite line.</param>
    /// <param name="segments">A list of <see cref="Segment"/> objects to check for overlap with the line.</param>
    /// <returns>True if the line overlaps with any segment in the list; otherwise, false.</returns>
    public static bool OverlapLineSegments(Vector2 linePoint, Vector2 lineDirection, List<Segment.Segment> segments)
    {
        if (segments.Count <= 0) return false;

        foreach (var seg in segments)
        {
            var result = LineIntersection.IntersectLineSegment(linePoint, lineDirection, seg.Start, seg.End);
            if (result.Valid) return true;
        }
        return false;
    }
    
    
    /// <summary>
    /// Determines whether the specified point lies on this infinite line.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="p">The point to check.</param>
    /// <returns>True if the point lies on the line; otherwise, false.</returns>
    public static bool OverlapPoint(this Line self, Vector2 p) => LineIntersection.IsPointOnLine(self.Point, self.Direction, p);
    /// <summary>
    /// Determines whether this infinite line and a finite segment overlap (i.e., intersect at any point).
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <returns>True if the line and segment overlap (intersect); otherwise, false.</returns>
    public static bool OverlapSegment(this Line self, Vector2 segmentStart, Vector2 segmentEnd) => OverlapLineSegment(self.Point, self.Direction, segmentStart, segmentEnd);
    /// <summary>
    /// Determines whether this infinite line and another infinite line overlap (i.e., are collinear).
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="linePoint">A point on the other line.</param>
    /// <param name="lineDirection">The direction vector of the other line.</param>
    /// <returns>True if the lines overlap; otherwise, false.</returns>
    public static bool OverlapLine(this Line self, Vector2 linePoint, Vector2 lineDirection) => OverlapLineLine(self.Point, self.Direction, linePoint, lineDirection);
    /// <summary>
    /// Determines whether this infinite line and a ray overlap (i.e., are collinear and extend in the same direction).
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <returns>True if the line and ray overlap; otherwise, false.</returns>
    public static bool OverlapRay(this Line self, Vector2 rayPoint, Vector2 rayDirection) => OverlapLineRay(self.Point, self.Direction, rayPoint, rayDirection);
    /// <summary>
    /// Determines whether this infinite line and a circle overlap (i.e., the line passes through or touches the circle).
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <returns>True if the line and circle overlap; otherwise, false.</returns>
    public static bool OverlapCircle(this Line self, Vector2 circleCenter, float circleRadius) => OverlapLineCircle(self.Point, self.Direction, circleCenter, circleRadius);
    /// <summary>
    /// Determines whether this infinite line and a triangle defined by three vertices overlap (i.e., the line passes through or touches the triangle).
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <returns>True if the line and triangle overlap; otherwise, false.</returns>
    public static bool OverlapTriangle(this Line self, Vector2 a, Vector2 b, Vector2 c) => OverlapLineTriangle(self.Point, self.Direction, a, b, c);
    /// <summary>
    /// Determines whether this infinite line and a quadrilateral defined by four vertices overlap (i.e., the line passes through or touches the quadrilateral).
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="a">The first vertex of the quadrilateral.</param>
    /// <param name="b">The second vertex of the quadrilateral.</param>
    /// <param name="c">The third vertex of the quadrilateral.</param>
    /// <param name="d">The fourth vertex of the quadrilateral.</param>
    /// <returns>True if the line and quadrilateral overlap; otherwise, false.</returns>
    public static bool OverlapQuad(this Line self, Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapLineQuad(self.Point, self.Direction, a, b, c, d);
    /// <summary>
    /// Determines whether this infinite line and a rectangle defined by four vertices overlap (i.e., the line passes through or touches the rectangle).
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="a">The first vertex of the rectangle.</param>
    /// <param name="b">The second vertex of the rectangle.</param>
    /// <param name="c">The third vertex of the rectangle.</param>
    /// <param name="d">The fourth vertex of the rectangle.</param>
    /// <returns>True if the line and rectangle overlap; otherwise, false.</returns>
    public static bool OverlapRect(this Line self, Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapLineQuad(self.Point, self.Direction, a, b, c, d);
    /// <summary>
    /// Determines whether this infinite line and a polygon overlap (i.e., the line passes through or touches the polygon).
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="points">A list of vertices defining the polygon. The polygon is assumed to be closed and non-self-intersecting.</param>
    /// <returns>True if the line and polygon overlap; otherwise, false.</returns>
    public static bool OverlapPolygon(this Line self, List<Vector2> points) => OverlapLinePolygon(self.Point, self.Direction, points);
    /// <summary>
    /// Determines whether this infinite line and a polyline overlap (i.e., the line passes through or touches any segment of the polyline).
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="points">A list of vertices defining the polyline. The polyline is assumed to be open and non-self-intersecting.</param>
    /// <returns>True if the line and polyline overlap; otherwise, false.</returns>
    public static bool OverlapPolyline(this Line self, List<Vector2> points) => OverlapLinePolyline(self.Point, self.Direction, points);
    /// <summary>
    /// Determines whether this infinite line and any segment in the provided list overlap (i.e., intersect at any point).
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="segments">A list of <see cref="Segment"/> objects to check for overlap with the line.</param>
    /// <returns>True if the line overlaps with any segment in the list; otherwise, false.</returns>
    public static bool OverlapSegments(this Line self, List<Segment.Segment> segments) => OverlapLineSegments(self.Point, self.Direction, segments);
    /// <summary>
    /// Determines whether this infinite line overlaps (intersects) with the shape of the specified <see cref="Collider"/>.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="collider">The <see cref="Collider"/> whose shape will be checked for overlap with this line.</param>
    /// <returns>True if the line overlaps with the collider's shape; otherwise, false.</returns>
    public static bool Overlap(this Line self, Collider collider)
    {
        if (collider == null) throw new ArgumentNullException(nameof(collider));
        if (!collider.Enabled) return false;

        switch (collider.GetShapeType())
        {
            case ShapeType.Circle:
                var c = collider.GetCircleShape();
                return OverlapShape(self, c);
            case ShapeType.Segment:
                var s = collider.GetSegmentShape();
                return OverlapShape(self, s);
            case ShapeType.Ray:
                var rayShape = collider.GetRayShape();
                return OverlapShape(self, rayShape);
            case ShapeType.Line:
                var l = collider.GetLineShape();
                return OverlapShape(self, l);
            case ShapeType.Triangle:
                var t = collider.GetTriangleShape();
                return OverlapShape(self, t);
            case ShapeType.Rect:
                var r = collider.GetRectShape();
                return OverlapShape(self, r);
            case ShapeType.Quad:
                var q = collider.GetQuadShape();
                return OverlapShape(self, q);
            case ShapeType.Poly:
                var p = collider.GetPolygonShape();
                return OverlapShape(self, p);
            case ShapeType.PolyLine:
                var pl = collider.GetPolylineShape();
                return OverlapShape(self, pl);
        }

        return false;
    }
    /// <summary>
    /// Determines whether this infinite line and another <see cref="Line"/> overlap (i.e., are collinear).
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="line">The <see cref="Line"/> to check for overlap.</param>
    /// <returns>True if the lines overlap; otherwise, false.</returns>
    public static bool OverlapShape(this Line self, Line line) => OverlapLineLine(self.Point, self.Direction, line.Point, line.Direction);
    /// <summary>
    /// Determines whether this infinite line and a <see cref="Ray"/> overlap (i.e., are collinear and extend in the same direction).
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="ray">The <see cref="Ray"/> to check for overlap.</param>
    /// <returns>True if the line and ray overlap; otherwise, false.</returns>
    public static bool OverlapShape(this Line self, Ray.Ray ray) => OverlapLineRay(self.Point, self.Direction, ray.Point, ray.Direction);
    /// <summary>
    /// Determines whether this infinite line and the specified <see cref="Segment"/> overlap (i.e., intersect at any point).
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="segment">The <see cref="Segment"/> to check for overlap with this line.</param>
    /// <returns>True if the line and segment overlap (intersect); otherwise, false.</returns>
    public static bool OverlapShape(this Line self, Segment.Segment segment) => OverlapLineSegment(self.Point, self.Direction, segment.Start, segment.End);
    /// <summary>
    /// Determines whether this infinite line and the specified <see cref="Circle"/> overlap (i.e., the line passes through or touches the circle).
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="circle">The <see cref="Circle"/> to check for overlap with this line.</param>
    /// <returns>True if the line and circle overlap; otherwise, false.</returns>
    public static bool OverlapShape(this Line self, Circle.Circle circle) => OverlapLineCircle(self.Point, self.Direction, circle.Center, circle.Radius);
    /// <summary>
    /// Determines whether this infinite line and the specified <see cref="Triangle"/> overlap (i.e., the line passes through or touches the triangle).
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="t">The <see cref="Triangle"/> to check for overlap with this line.</param>
    /// <returns>True if the line and triangle overlap; otherwise, false.</returns>
    public static bool OverlapShape(this Line self, Triangle.Triangle t) => OverlapLineTriangle(self.Point, self.Direction, t.A, t.B, t.C);
    /// <summary>
    /// Determines whether this infinite line and the specified <see cref="Quad"/> overlap (i.e., the line passes through or touches the quadrilateral).
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="q">The <see cref="Quad"/> to check for overlap with this line.</param>
    /// <returns>True if the line and quad overlap; otherwise, false.</returns>
    public static bool OverlapShape(this Line self, Quad.Quad q) => OverlapLineQuad(self.Point, self.Direction, q.A, q.B, q.C, q.D);
    /// <summary>
    /// Determines whether this infinite line and the specified <see cref="Rect"/> overlap (i.e., the line passes through or touches the rectangle).
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="r">The <see cref="Rect"/> to check for overlap with this line.</param>
    /// <returns>True if the line and rectangle overlap; otherwise, false.</returns>
    public static bool OverlapShape(this Line self, Rect.Rect r) => OverlapLineQuad(self.Point, self.Direction, r.A, r.B, r.C, r.D);
    /// <summary>
    /// Determines whether this infinite line and the specified <see cref="Polygon"/> overlap (i.e., the line passes through or touches the polygon).
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="p">The <see cref="Polygon"/> to check for overlap with this line.</param>
    /// <returns>True if the line and polygon overlap; otherwise, false.</returns>
    public static bool OverlapShape(this Line self, Polygon.Polygon p) => OverlapLinePolygon(self.Point, self.Direction, p);
    /// <summary>
    /// Determines whether this infinite line and the specified <see cref="Polyline"/> overlap (i.e., the line passes through or touches any segment of the polyline).
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="pl">The <see cref="Polyline"/> to check for overlap with this line.</param>
    /// <returns>True if the line and polyline overlap; otherwise, false.</returns>
    public static bool OverlapShape(this Line self, Polyline.Polyline pl) => OverlapLinePolyline(self.Point, self.Direction, pl);
    /// <summary>
    /// Determines whether this infinite line overlaps (intersects) with any segment in the provided <see cref="Segment.Segments"/> collection.
    /// </summary>
    /// <param name="self"> The line to use.</param>
    /// <param name="segments">A <see cref="Segment.Segments"/> collection to check for overlap with this line.</param>
    /// <returns>True if the line overlaps with any segment in the collection; otherwise, false.</returns>
    public static bool OverlapShape(this Line self, Segment.Segments segments) => OverlapLineSegments(self.Point, self.Direction, segments);

}
