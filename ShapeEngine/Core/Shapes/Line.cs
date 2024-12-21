using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Shapes;

public readonly struct Line
{
    #region Members

    public readonly Vector2 Point;
    public readonly Vector2 Direction;
    public readonly Vector2 Normal;
    #endregion

    #region Constructors
    public Line()
    {
        Point = Vector2.Zero;
        Direction = Vector2.Zero;
        Normal = Vector2.Zero;
    }
    public Line(float x, float y, float dx, float dy, bool flippedNormal = false)
    {
        Point = new Vector2(x, y);
        Direction = new Vector2(dx, dy).Normalize();
        Normal = flippedNormal ? new Vector2(Direction.Y, -Direction.X) : new Vector2(-Direction.Y, Direction.X);
    }
    public Line(Vector2 direction, bool flippedNormal = false)
    {
        Point = Vector2.Zero;
        Direction = direction.Normalize();
        Normal = flippedNormal ? new Vector2(Direction.Y, -Direction.X) : new Vector2(-Direction.Y, Direction.X);
    }
    public Line(Vector2 point, Vector2 direction, bool flippedNormal = false)
    {
        Point = point;
        Direction = direction.Normalize();
        Normal = flippedNormal ? new Vector2(Direction.Y, -Direction.X) : new Vector2(-Direction.Y, Direction.X);
    }
    internal Line(Vector2 point, Vector2 direction, Vector2 normal)
    {
        Point = point;
        Direction = direction.Normalize();
        Normal = normal;
    }

    #endregion
    
    #region Public Functions
    public bool IsValid => Direction.IsNormalized() && Normal.IsNormalized(); // (Direction.X != 0 || Direction.Y!= 0) && (Normal.X != 0 || Normal.Y != 0);
    public Segment ToSegment(float length)
    {
        if (!IsValid) return new();
        return new Segment(Point - Direction * length * 0.5f, Point + Direction * length * 0.5f, Normal);
    }
    public Ray ToRay(bool reversed = false) => reversed ? new Ray(Point, -Direction, -Normal) : new Ray(Point, Direction, Normal);
    public Line FlipNormal() => new Line(Point, Direction, Normal.Flip());
    public static Vector2 GetNormal(Vector2 direction, bool flippedNormal)
    {
        if (flippedNormal) return direction.GetPerpendicularLeft().Normalize();
        return direction.GetPerpendicularRight().Normalize();
    }
    public bool IsPointOnLine(Vector2 point) => IsPointOnLine(point, Point, Direction);
    #endregion
   
    #region Closest Point
    
    public static (Vector2 point1, Vector2 point2) GetClosestPointsLineLine(Vector2 line1Point, Vector2 line1Direction, Vector2 line2Point, Vector2 line2Direction)
    {
        var d1 = line1Direction.Normalize();
        var d2 = line2Direction.Normalize();

        float a = Vector2.Dot(d1, d1);
        float b = Vector2.Dot(d1, d2);
        float e = Vector2.Dot(d2, d2);
        var r = line1Point - line2Point;
        float c = Vector2.Dot(d1, r);
        float f = Vector2.Dot(d2, r);

        float denominator = a * e - b * b;
        float t1 = (b * f - c * e) / denominator;
        float t2 = (a * f - b * c) / denominator;

        var closestPoint1 = line1Point + t1 * d1;
        var closestPoint2 = line2Point + t2 * d2;

        return (closestPoint1, closestPoint2);
    }
    public static (Vector2 linePoint, Vector2 rayPoint) GetClosestPointsLineRay(Vector2 linePoint, Vector2 lineDirection, Vector2 rayPoint, Vector2 rayDirection)
    {
        var d1 = lineDirection.Normalize();
        var d2 = rayDirection.Normalize();

        float a = Vector2.Dot(d1, d1);
        float b = Vector2.Dot(d1, d2);
        float e = Vector2.Dot(d2, d2);
        var r = linePoint - rayPoint;
        float c = Vector2.Dot(d1, r);
        float f = Vector2.Dot(d2, r);

        float denominator = a * e - b * b;
        float t1 = (b * f - c * e) / denominator;
        float t2 = Math.Max(0, (a * f - b * c) / denominator);

        var closestPoint1 = linePoint + t1 * d1;
        var closestPoint2 = rayPoint + t2 * d2;

        return (closestPoint1, closestPoint2);
    }
    public static (Vector2 linePoint, Vector2 segmentPoint) GetClosestPointsLineSegment(Vector2 linePoint, Vector2 lineDirection, Vector2 segmentStart, Vector2 segmentEnd)
    {
        var d1 = lineDirection.Normalize();
        var d2 = segmentEnd - segmentStart;

        float a = Vector2.Dot(d1, d1);
        float b = Vector2.Dot(d1, d2);
        float e = Vector2.Dot(d2, d2);
        var r = linePoint - segmentStart;
        float c = Vector2.Dot(d1, r);
        float f = Vector2.Dot(d2, r);

        float denominator = a * e - b * b;
        float t1 = (b * f - c * e) / denominator;
        float t2 = Math.Max(0, Math.Min(1, (a * f - b * c) / denominator));

        var closestPoint1 = linePoint + t1 * d1;
        var closestPoint2 = segmentStart + t2 * d2;

        return (closestPoint1, closestPoint2);
    }
    public static (Vector2 linePoint, Vector2 circlePoint) GetClosestPointsLineCircle(Vector2 linePoint, Vector2 lineDirection, Vector2 circleCenter, float radius)
    {
        var d1 = lineDirection.Normalize();

        var toCenter = circleCenter - linePoint;
        float projectionLength = Vector2.Dot(toCenter, d1);
        var closestPointOnLine = linePoint + projectionLength * d1;

        var offset = (closestPointOnLine - circleCenter).Normalize() * radius;
        var closestPointOnCircle = circleCenter + offset;

        return (closestPointOnLine, closestPointOnCircle);
    }
    
    //TODO: add remaining shapes here
    
    #endregion
    
    #region Intersections

    public static bool IsPointOnLine(Vector2 point, Vector2 linePoint, Vector2 lineDirection)
    {
        // Calculate the vector from the line point to the given point
        var toPoint = point - linePoint;

        // Calculate the cross product of the direction vector and the vector to the point
        float crossProduct = toPoint.X * lineDirection.Y - toPoint.Y * lineDirection.X;

        // If the cross product is close to zero, the point is on the line
        return Math.Abs(crossProduct) < 1e-10;
    }
    
    public static (CollisionPoint p, float t) IntersectLineSegmentInfo(Vector2 linePoint, Vector2 lineDirection, Vector2 segmentStart, Vector2 segmentEnd)
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
            return (new(), -1f);
        }

        // Calculate the intersection point using parameter t
        var difference = segmentStart - linePoint;
        float t = (difference.X * segmentDirection.Y - difference.Y * segmentDirection.X) / denominator;

        // Calculate the intersection point
        var intersection = linePoint + t * lineDirection;

        // Check if the intersection point is within the segment
        if (Segment.IsPointOnSegment(intersection, segmentStart, segmentEnd))
        {
            // The normal vector can be taken as perpendicular to the segment direction
            segmentDirection = segmentDirection.Normalize();
            var normal = new Vector2(-segmentDirection.Y, segmentDirection.X);

            return (new(intersection, normal), t);
        }

        return (new(), -1f);
    }
    public static (CollisionPoint p, float t) IntersectLineSegmentInfo(Vector2 linePoint, Vector2 lineDirection, Vector2 segmentStart, Vector2 segmentEnd, Vector2 segmentNormal)
    {
        var result = IntersectLineSegmentInfo(linePoint, lineDirection, segmentStart, segmentEnd);
        if (result.p.Valid)
        {
            return (new(result.p.Point, segmentNormal), result.t);
        }

        return (new(), -1f);
    }
    public static (CollisionPoint p, float t) IntersectLineLineInfo(Vector2 line1Point, Vector2 line1Direction, Vector2 line2Point, Vector2 line2Direction)
    {
        // Calculate the denominator of the intersection formula
        float denominator = line1Direction.X * line2Direction.Y - line1Direction.Y * line2Direction.X;

        // Check if lines are parallel (denominator is zero)
        if (Math.Abs(denominator) < 1e-10)
        {
            return (new(), -1f);
        }

        // Calculate the intersection point using parameter t
        var difference = line2Point - line1Point;
        float t = (difference.X * line2Direction.Y - difference.Y * line2Direction.X) / denominator;

        // Calculate the intersection point
        var intersection = line1Point + t * line1Direction;

        // Calculate the normal vector as perpendicular to the direction of the first line
        var normal = new Vector2(-line2Direction.Y, line2Direction.X).Normalize();

        return (new(intersection, normal), t);
    }
    public static (CollisionPoint p, float t) IntersectLineLineInfo(Vector2 line1Point, Vector2 line1Direction, Vector2 line2Point, Vector2 line2Direction, Vector2 line2Normal)
    {
        var result = IntersectLineLineInfo(line1Point, line1Direction, line2Point, line2Direction);
        if (result.p.Valid)
        {
            return (new(result.p.Point, line2Normal), result.t);
        }

        return (new(), -1f);
    }
    public static (CollisionPoint p, float t) IntersectLineRayInfo(Vector2 linePoint, Vector2 lineDirection, Vector2 rayPoint, Vector2 rayDirection)
    {
        // Calculate the denominator of the intersection formula
        float denominator = lineDirection.X * rayDirection.Y - lineDirection.Y * rayDirection.X;

        // Check if lines are parallel (denominator is zero)
        if (Math.Abs(denominator) < 1e-10)
        {
            return (new(), -1f);
        }

        // Calculate the intersection point using parameter t
        var difference = rayPoint - linePoint;
        float t = (difference.X * rayDirection.Y - difference.Y * rayDirection.X) / denominator;

        // Calculate the parameter u for the ray
        float u = (difference.X * lineDirection.Y - difference.Y * lineDirection.X) / denominator;

        // Check if the intersection point lies in the direction of the ray
        if (u >= 0)
        {
            // Calculate the intersection point
            var intersection = linePoint + t * lineDirection;

            // Calculate the normal vector as perpendicular to the direction of the line
            var normal = new Vector2(-rayDirection.Y, rayDirection.X).Normalize();

            return (new(intersection, normal), t);
        }
        
        return (new(), -1f);
    }
    public static (CollisionPoint p, float t) IntersectLineRayInfo(Vector2 linePoint, Vector2 lineDirection, Vector2 rayPoint, Vector2 rayDirection, Vector2 rayNormal)
    {
        var result = IntersectLineRayInfo(linePoint, lineDirection, rayPoint, rayDirection);
        if (result.p.Valid)
        {
            return (new(result.p.Point, rayNormal), result.t);
        }

        return (new(), -1f);
    }
    
    
    public static CollisionPoint IntersectLineSegment(Vector2 linePoint, Vector2 lineDirection, Vector2 segmentStart, Vector2 segmentEnd)
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
            return new();
        }

        // Calculate the intersection point using parameter t
        var difference = segmentStart - linePoint;
        float t = (difference.X * segmentDirection.Y - difference.Y * segmentDirection.X) / denominator;

        // Calculate the intersection point
        var intersection = linePoint + t * lineDirection;

        // Check if the intersection point is within the segment
        if (Segment.IsPointOnSegment(intersection, segmentStart, segmentEnd))
        {
            // The normal vector can be taken as perpendicular to the segment direction
            segmentDirection = segmentDirection.Normalize();
            var normal = new Vector2(-segmentDirection.Y, segmentDirection.X);

            return new(intersection, normal);
        }

        return new();
    }
    public static CollisionPoint IntersectLineSegment(Vector2 linePoint, Vector2 lineDirection, Vector2 segmentStart, Vector2 segmentEnd, Vector2 segmentNormal)
    {
        var result = IntersectLineSegment(linePoint, lineDirection, segmentStart, segmentEnd);
        if (result.Valid)
        {
            return new(result.Point, segmentNormal);
        }

        return new();
    }
    public static CollisionPoint IntersectLineLine(Vector2 line1Point, Vector2 line1Direction, Vector2 line2Point, Vector2 line2Direction)
    {
        // Calculate the denominator of the intersection formula
        float denominator = line1Direction.X * line2Direction.Y - line1Direction.Y * line2Direction.X;

        // Check if lines are parallel (denominator is zero)
        if (Math.Abs(denominator) < 1e-10)
        {
            return new();
        }

        // Calculate the intersection point using parameter t
        var difference = line2Point - line1Point;
        float t = (difference.X * line2Direction.Y - difference.Y * line2Direction.X) / denominator;

        // Calculate the intersection point
        var intersection = line1Point + t * line1Direction;

        // Calculate the normal vector as perpendicular to the direction of the first line
        var normal = new Vector2(-line2Direction.Y, line2Direction.X).Normalize();

        return new(intersection, normal);
    }
    public static CollisionPoint IntersectLineLine(Vector2 line1Point, Vector2 line1Direction, Vector2 line2Point, Vector2 line2Direction, Vector2 line2Normal)
    {
        var result = IntersectLineLine(line1Point, line1Direction, line2Point, line2Direction);
        if (result.Valid)
        {
            return new(result.Point, line2Normal);
        }

        return new();
    }
    public static CollisionPoint IntersectLineRay(Vector2 linePoint, Vector2 lineDirection, Vector2 rayPoint, Vector2 rayDirection)
    {
        // Calculate the denominator of the intersection formula
        float denominator = lineDirection.X * rayDirection.Y - lineDirection.Y * rayDirection.X;

        // Check if lines are parallel (denominator is zero)
        if (Math.Abs(denominator) < 1e-10)
        {
            return new();
        }

        // Calculate the intersection point using parameter t
        Vector2 difference = rayPoint - linePoint;
        float t = (difference.X * rayDirection.Y - difference.Y * rayDirection.X) / denominator;

        // Calculate the parameter u for the ray
        float u = (difference.X * lineDirection.Y - difference.Y * lineDirection.X) / denominator;

        // Check if the intersection point lies in the direction of the ray
        if (u >= 0)
        {
            // Calculate the intersection point
            var intersection = linePoint + t * lineDirection;

            // Calculate the normal vector as perpendicular to the direction of the line
            var normal = new Vector2(-rayDirection.Y, rayDirection.X).Normalize();

            return new(intersection, normal);
        }
        
        return new();
    }
    public static CollisionPoint IntersectLineRay(Vector2 linePoint, Vector2 lineDirection, Vector2 rayPoint, Vector2 rayDirection, Vector2 rayNormal)
    {
        var result = IntersectLineRay(linePoint, lineDirection, rayPoint, rayDirection);
        if (result.Valid)
        {
            return new(result.Point, rayNormal);
        }

        return new();
    }
    
    public static (CollisionPoint a, CollisionPoint b) IntersectLineCircle(Vector2 linePoint, Vector2 lineDirection, Vector2 circleCenter, float circleRadius)
    {
        // Normalize the direction vector
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
        if (distanceToCenter < circleRadius)
        {
            // Calculate the distance from the closest point to the intersection points
            var offset = (float)Math.Sqrt(circleRadius * circleRadius - distanceToCenter * distanceToCenter);

            // Intersection points
            var intersection1 = closestPoint - offset * lineDirection;
            var intersection2 = closestPoint + offset * lineDirection;

            // Normals at the intersection points
            var normal1 = (intersection1 - circleCenter).Normalize();
            var normal2 = (intersection2 - circleCenter).Normalize();

            var p1 = new CollisionPoint(intersection1, normal1);
            var p2 = new CollisionPoint(intersection2, normal2);
            return (p1, p2);
        }
        
        if (Math.Abs(distanceToCenter - circleRadius) < 1e-10)
        {
            var p = new CollisionPoint(closestPoint,(closestPoint - circleCenter).Normalize());
            return (p, new());
        }

        return (new(), new());
    }
    public static (CollisionPoint a, CollisionPoint b) IntersectLineTriangle(Vector2 linePoint, Vector2 lineDirection, Vector2 a, Vector2 b, Vector2 c)
    {
        CollisionPoint resultA = new();
        CollisionPoint resultB = new();
        
        var cp = IntersectLineSegment(linePoint, lineDirection,  a, b);
        if(cp.Valid) resultA = cp;
        
        cp = IntersectLineSegment(linePoint, lineDirection,  b, c);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }
        
        if(resultA.Valid && resultB.Valid) return (resultA, resultB);
       
        cp = IntersectLineSegment(linePoint, lineDirection,  c, a);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }
        
        return (resultA, resultB);
    }
    public static (CollisionPoint a, CollisionPoint b) IntersectLineQuad(Vector2 linePoint, Vector2 lineDirection, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        CollisionPoint resultA = new();
        CollisionPoint resultB = new();
        
        var cp = IntersectLineSegment(linePoint, lineDirection,  a, b);
        if(cp.Valid) resultA = cp;
        
        cp = IntersectLineSegment(linePoint, lineDirection,  b, c);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }
        
        if(resultA.Valid && resultB.Valid) return (resultA, resultB);
       
        cp = IntersectLineSegment(linePoint, lineDirection,  c, d);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }
        
        if(resultA.Valid && resultB.Valid) return (resultA, resultB);
        
        cp = IntersectLineSegment(linePoint, lineDirection,  d, a);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }
        
        return (resultA, resultB);
    }
    
    public static (CollisionPoint a, CollisionPoint b) IntersectLineRect(Vector2 linePoint, Vector2 lineDirection, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return IntersectLineQuad(linePoint, lineDirection, a, b, c, d);
    }
    public static CollisionPoints? IntersectLinePolygon(Vector2 linePoint, Vector2 lineDirection, List<Vector2> points, int maxCollisionPoints = -1)
    {
        if (points.Count < 3) return null;
        if (maxCollisionPoints == 0) return null;
        CollisionPoints? result = null;
        for (var i = 0; i < points.Count; i++)
        {
            var colPoint = IntersectLineSegment(linePoint, lineDirection, points[i], points[(i + 1) % points.Count]);
            if (colPoint.Valid)
            {
                result ??= new();
                result.Add(colPoint);
                if(maxCollisionPoints > 0 && result.Count >= maxCollisionPoints) return result;
            }
        }
        return result;
    }
    public static CollisionPoints? IntersectLinePolyline(Vector2 linePoint, Vector2 lineDirection, List<Vector2> points, int maxCollisionPoints = -1)
    {
        if (points.Count < 3) return null;
        if (maxCollisionPoints == 0) return null;
        CollisionPoints? result = null;
        for (var i = 0; i < points.Count - 1; i++)
        {
            var colPoint = IntersectLineSegment(linePoint, lineDirection, points[i], points[i + 1]);
            if (colPoint.Valid)
            {
                result ??= new();
                result.Add(colPoint);
                if(maxCollisionPoints > 0 && result.Count >= maxCollisionPoints) return result;
            }
        }
        return result;
    }
    public static CollisionPoints? IntersectLineSegments(Vector2 linePoint, Vector2 lineDirection, List<Segment> segments, int maxCollisionPoints = -1)
    {
        if (segments.Count <= 0) return null;
        if (maxCollisionPoints == 0) return null;
        CollisionPoints? result = null;

        foreach (var seg in segments)
        {
            var colPoint = IntersectLineSegment(linePoint, lineDirection, seg.Start, seg.End);
            if (colPoint.Valid)
            {
                result ??= new();
                result.AddRange(colPoint);
                if(maxCollisionPoints > 0 && result.Count >= maxCollisionPoints) return result;
            }
        }
        return result;
    }

    
    public CollisionPoint IntersectSegment(Vector2 segmentStart, Vector2 segmentEnd) => IntersectLineSegment(Point, Direction, segmentStart, segmentEnd);
    public CollisionPoint IntersectSegment(Segment segment) => IntersectLineSegment(Point, Direction, segment.Start, segment.End, segment.Normal);
    public CollisionPoint IntersectLine(Vector2 otherPoint, Vector2 otherDirection) => IntersectLineLine(Point, Direction, otherPoint, otherDirection);
    public CollisionPoint IntersectLine(Line otherLine) => IntersectLineLine(Point, Direction, otherLine.Point, otherLine.Direction, otherLine.Normal);
    public CollisionPoint IntersectRay(Vector2 otherPoint, Vector2 otherDirection) => IntersectLineRay(Point, Direction, otherPoint, otherDirection);
    public CollisionPoint IntersectRay(Ray otherRay) => IntersectLineRay(Point, Direction, otherRay.Point, otherRay.Direction, otherRay.Normal);
    public (CollisionPoint a, CollisionPoint b) IntersectCircle(Circle otherCircle) => IntersectLineCircle(Point, Direction, otherCircle.Center, otherCircle.Radius);
    public (CollisionPoint a, CollisionPoint b) IntersectCircle(Vector2 circleCenter, float circleRadius) => IntersectLineCircle(Point, Direction, circleCenter, circleRadius);
    public (CollisionPoint a, CollisionPoint b) IntersectTriangle(Vector2 a, Vector2 b, Vector2 c) => IntersectLineTriangle(Point, Direction, a, b, c);
    public (CollisionPoint a, CollisionPoint b) IntersectTriangle(Triangle triangle) => IntersectLineTriangle(Point, Direction, triangle.A, triangle.B, triangle.C);
    public (CollisionPoint a, CollisionPoint b) IntersectQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => IntersectLineQuad(Point, Direction, a, b, c, d);
    public (CollisionPoint a, CollisionPoint b) IntersectQuad(Quad quad) => IntersectLineQuad(Point, Direction, quad.A, quad.B, quad.C, quad.D);
    public (CollisionPoint a, CollisionPoint b) IntersectRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => IntersectLineQuad(Point, Direction, a, b, c, d);
    public (CollisionPoint a, CollisionPoint b) IntersectRect(Rect rect) => IntersectLineQuad(Point, Direction, rect.A, rect.B, rect.C, rect.D);
    
    public CollisionPoints? IntersectPolygon(List<Vector2> points, int maxCollisionPoints = -1) => IntersectLinePolygon(Point, Direction, points, maxCollisionPoints);
    public CollisionPoints? IntersectPolygon(Polygon polygon, int maxCollisionPoints = -1) => IntersectLinePolygon(Point, Direction, polygon, maxCollisionPoints);
    public CollisionPoints? IntersectPolyline(List<Vector2> points, int maxCollisionPoints = -1) => IntersectLinePolyline(Point, Direction, points, maxCollisionPoints);
    public CollisionPoints? IntersectPolyline(Polyline polyline, int maxCollisionPoints = -1) => IntersectLinePolyline(Point, Direction, polyline, maxCollisionPoints);
    public CollisionPoints? IntersectSegments(List<Segment> segments, int maxCollisionPoints = -1) => IntersectLineSegments(Point, Direction, segments, maxCollisionPoints);
    public CollisionPoints? IntersectSegments(Segments segments, int maxCollisionPoints = -1) => IntersectLineSegments(Point, Direction, segments, maxCollisionPoints);
    
    public CollisionPoints? IntersectShape(Segment segment)
    {
        var result = IntersectLineSegment(Point, Direction, segment.Start, segment.End, segment.Normal);
        if (result.Valid)
        {
            var colPoints = new CollisionPoints();
            colPoints.Add(result);
            return colPoints;
        }

        return null;
    }
    public CollisionPoints? IntersectShape(Line line)
    {
        var result = IntersectLineLine(Point, Direction, line.Point, line.Direction, line.Normal);
        if (result.Valid)
        {
            var colPoints = new CollisionPoints();
            colPoints.Add(result);
            return colPoints;
        }

        return null;
    }
    public CollisionPoints? IntersectShape(Ray ray)
    {
        var result = IntersectLineRay(Point, Direction, ray.Point, ray.Direction, ray.Normal);
        if (result.Valid)
        {
            var colPoints = new CollisionPoints();
            colPoints.Add(result);
            return colPoints;
        }

        return null;
    }
    public CollisionPoints? IntersectShape(Circle circle)
    {
        var result = IntersectLineCircle(Point, Direction, circle.Center, circle.Radius);
        if (result.a.Valid || result.b.Valid)
        {
            var colPoints = new CollisionPoints();
            if (result.a.Valid)
            {
                colPoints.Add(result.a);
            }
            if (result.b.Valid)
            {
                colPoints.Add(result.b);
            }
            return colPoints;
        }

        return null;
    }
    public CollisionPoints? IntersectShape(Triangle t)
    {
        var result = IntersectLineTriangle(Point, Direction, t.A, t.B, t.C);
        if (result.a.Valid || result.b.Valid)
        {
            var colPoints = new CollisionPoints();
            if (result.a.Valid)
            {
                colPoints.Add(result.a);
            }
            if (result.b.Valid)
            {
                colPoints.Add(result.b);
            }
            return colPoints;
        }

        return null;
    }
    public CollisionPoints? IntersectShape(Quad q)
    {
        var result = IntersectLineQuad(Point, Direction, q.A, q.B, q.C, q.D);
        if (result.a.Valid || result.b.Valid)
        {
            var colPoints = new CollisionPoints();
            if (result.a.Valid)
            {
                colPoints.Add(result.a);
            }
            if (result.b.Valid)
            {
                colPoints.Add(result.b);
            }
            return colPoints;
        }

        return null;
    }
    public CollisionPoints? IntersectShape(Rect r)
    {
        var result =  IntersectLineQuad(Point, Direction, r.A, r.B, r.C, r.D);
        if (result.a.Valid || result.b.Valid)
        {
            var colPoints = new CollisionPoints();
            if (result.a.Valid)
            {
                colPoints.Add(result.a);
            }
            if (result.b.Valid)
            {
                colPoints.Add(result.b);
            }
            return colPoints;
        }

        return null;
    }
    public CollisionPoints? IntersectShape(Polygon p, int maxCollisionPoints = -1) => IntersectLinePolygon(Point, Direction, p, maxCollisionPoints);
    public CollisionPoints? IntersectShape(Polyline pl, int maxCollisionPoints = -1) => IntersectLinePolyline(Point, Direction, pl, maxCollisionPoints);
    public CollisionPoints? IntersectShape(Segments segments, int maxCollisionPoints = -1) => IntersectLineSegments(Point, Direction, segments, maxCollisionPoints);
    #endregion

    #region Overlap

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
        
        return Segment.IsPointOnSegment(intersection, segmentStart, segmentEnd);
    }
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
    public static bool OverlapLineCircle(Vector2 linePoint, Vector2 lineDirection, Vector2 circleCenter, float circleRadius)
    {
        // Normalize the direction vector
        if (Circle.ContainsCirclePoint(circleCenter, circleRadius, linePoint)) return true;

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
    public static bool OverlapLineTriangle(Vector2 linePoint, Vector2 lineDirection, Vector2 a, Vector2 b, Vector2 c)
    {
        if (Triangle.ContainsPoint(a, b, c, linePoint)) return true;
        
        var cp = IntersectLineSegment(linePoint, lineDirection,  a, b);
        if (cp.Valid) return true;
        
        cp = IntersectLineSegment(linePoint, lineDirection,  b, c);
        if (cp.Valid) return true;
       
        cp= IntersectLineSegment(linePoint, lineDirection,  c, a);
        if (cp.Valid) return true;

        return false;
    }
    public static bool OverlapLineQuad(Vector2 linePoint, Vector2 lineDirection, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        if (Quad.ContainsPoint(a, b, c, d,  linePoint)) return true;
        
        var cp = IntersectLineSegment(linePoint, lineDirection,  a, b);
        if (cp.Valid) return true;
        
        cp = IntersectLineSegment(linePoint, lineDirection,  b, c);
        if (cp.Valid) return true;
       
        cp = IntersectLineSegment(linePoint, lineDirection,  c, d);
        if (cp.Valid) return true;

        cp = IntersectLineSegment(linePoint, lineDirection,  d, a);
        if (cp.Valid) return true;
        
        return false;
    }
    
    public static bool OverlapLineRect(Vector2 linePoint, Vector2 lineDirection, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return OverlapLineQuad(linePoint, lineDirection, a, b, c, d);
    }
    public static bool OverlapLinePolygon(Vector2 linePoint, Vector2 lineDirection, List<Vector2> points)
    {
        if (points.Count < 3) return false;
        
        for (var i = 0; i < points.Count; i++)
        {
            var colPoint = IntersectLineSegment(linePoint, lineDirection, points[i], points[(i + 1) % points.Count]);
            if (colPoint.Valid) return true;
        }
        return false;
    }
    public static bool OverlapLinePolyline(Vector2 linePoint, Vector2 lineDirection, List<Vector2> points)
    {
        if (points.Count < 3) return false;
        for (var i = 0; i < points.Count - 1; i++)
        {
            var colPoint = IntersectLineSegment(linePoint, lineDirection, points[i], points[i + 1]);
            if (colPoint.Valid) return true;
        }
        return false;
    }
    public static bool OverlapLineSegments(Vector2 linePoint, Vector2 lineDirection, List<Segment> segments)
    {
        if (segments.Count <= 0) return false;

        foreach (var seg in segments)
        {
            var result = IntersectLineSegment(linePoint, lineDirection, seg.Start, seg.End);
            if (result.Valid) return true;
        }
        return false;
    }

    
    public bool OverlapShape(Segment segment) => OverlapLineSegment(Point, Direction, segment.Start, segment.End);
    public bool OverlapShape(Line line) => OverlapLineLine(Point, Direction, line.Point, line.Direction);
    public bool OverlapShape(Ray ray) => OverlapLineRay(Point, Direction, ray.Point, ray.Direction);
    public bool OverlapShape(Circle circle) => OverlapLineCircle(Point, Direction, circle.Center, circle.Radius);
    public bool OverlapShape(Triangle t) => OverlapLineTriangle(Point, Direction, t.A, t.B, t.C);
    public bool OverlapShape(Quad q) => OverlapLineQuad(Point, Direction, q.A, q.B, q.C, q.D);
    public bool OverlapShape(Rect r) => OverlapLineQuad(Point, Direction, r.A, r.B, r.C, r.D);
    public bool OverlapShape(Polygon p) => OverlapLinePolygon(Point, Direction, p);
    public bool OverlapShape(Polyline pl) => OverlapLinePolyline(Point, Direction, pl);
    public bool OverlapShape(Segments segments) => OverlapLineSegments(Point, Direction, segments);

    #endregion
    
}