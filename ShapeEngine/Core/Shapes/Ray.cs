using System.Numerics;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Shapes;

public readonly struct Ray
{
    #region Members
    
    public readonly Vector2 Point;
    public readonly Vector2 Direction;
    public readonly Vector2 Normal;

    #endregion
    
    #region Constructors
    public Ray()
    {
        Point = Vector2.Zero;
        Direction = Vector2.Zero;
        Normal = Vector2.Zero;
    }
    public Ray(float x, float y, float dx, float dy, bool flippedNormal = false)
    {
        Point = new Vector2(x, y);
        Direction = new Vector2(dx, dy);
        Normal = flippedNormal ? new Vector2(Direction.Y, -Direction.X) : new Vector2(-Direction.Y, Direction.X);
    }
    public Ray(Vector2 direction, bool flippedNormal = false)
    {
        Point = Vector2.Zero;
        Direction = direction;
        Normal = flippedNormal ? new Vector2(Direction.Y, -Direction.X) : new Vector2(-Direction.Y, Direction.X);
    }
    public Ray(Vector2 point, Vector2 direction, bool flippedNormal = false)
    {
        Point = point;
        Direction = direction;
        Normal = flippedNormal ? new Vector2(Direction.Y, -Direction.X) : new Vector2(-Direction.Y, Direction.X);
    }

    internal Ray(Vector2 point, Vector2 direction, Vector2 normal)
    {
        Point = point;
        Direction = direction;
        Normal = normal;
    }
    #endregion
    
    #region Public Functions
    public bool IsValid => (Direction.X!= 0 || Direction.Y!= 0) && (Normal.X != 0 || Normal.Y != 0);
    public Segment ToSegment(float length)
    {
        if(!IsValid) return new();
        return new Segment(Point, Point + Direction * length);
    }
    public Line ToLine() => new Line(Point, Direction);
    public Ray FlipNormal() => new Ray(Point, Direction, Normal.Flip());
    public static Vector2 GetNormal(Vector2 direction, bool flippedNormal)
    {
        if (flippedNormal) return direction.GetPerpendicularLeft().Normalize();
        return direction.GetPerpendicularRight().Normalize();
    }
    public bool IsPointOnRay(Vector2 point) => IsPointOnRay(point, Point, Direction);
    #endregion
   
    //TODO: fix all normalize calls -> to ShapeVector Normalize()!
    //TODO: finish all shapes
    #region Closest Point
    
    /*public static (Vector2 point1, Vector2 point2) ClosestPointsRayLine(Vector2 rayPoint, Vector2 rayDirection, Vector2 linePoint, Vector2 lineDirection)
    {
        return ClosestPointsLineRay(linePoint, lineDirection, rayPoint, rayDirection);
    }
    
    public static (Vector2 point1, Vector2 point2) ClosestPointsRayRay(Vector2 ray1Point, Vector2 ray1Direction, Vector2 ray2Point, Vector2 ray2Direction)
    {
        Vector2 p1 = ray1Point;
        Vector2 d1 = Vector2.Normalize(ray1Direction);
        Vector2 p2 = ray2Point;
        Vector2 d2 = Vector2.Normalize(ray2Direction);

        float a = Vector2.Dot(d1, d1);
        float b = Vector2.Dot(d1, d2);
        float e = Vector2.Dot(d2, d2);
        Vector2 r = p1 - p2;
        float c = Vector2.Dot(d1, r);
        float f = Vector2.Dot(d2, r);

        float denominator = a * e - b * b;
        float t1 = Math.Max(0, (b * f - c * e) / denominator);
        float t2 = Math.Max(0, (a * f - b * c) / denominator);

        Vector2 closestPoint1 = p1 + t1 * d1;
        Vector2 closestPoint2 = p2 + t2 * d2;

        return (closestPoint1, closestPoint2);
    }
    
    public static (Vector2 point1, Vector2 point2) ClosestPointsRaySegment(Vector2 rayPoint, Vector2 rayDirection, Vector2 segmentStart, Vector2 segmentEnd)
    {
        Vector2 p1 = rayPoint;
        Vector2 d1 = Vector2.Normalize(rayDirection);
        Vector2 p2 = segmentStart;
        Vector2 d2 = segmentEnd - segmentStart;

        float a = Vector2.Dot(d1, d1);
        float b = Vector2.Dot(d1, d2);
        float e = Vector2.Dot(d2, d2);
        Vector2 r = p1 - p2;
        float c = Vector2.Dot(d1, r);
        float f = Vector2.Dot(d2, r);

        float denominator = a * e - b * b;
        float t1 = Math.Max(0, (b * f - c * e) / denominator);
        float t2 = Math.Max(0, Math.Min(1, (a * f - b * c) / denominator));

        Vector2 closestPoint1 = p1 + t1 * d1;
        Vector2 closestPoint2 = p2 + t2 * d2;

        return (closestPoint1, closestPoint2);
    }
    
    public static (Vector2 point1, Vector2 point2) ClosestPointsRayCircle(Vector2 rayPoint, Vector2 rayDirection, Vector2 circleCenter, float radius)
    {
        Vector2 p1 = rayPoint;
        Vector2 d1 = Vector2.Normalize(rayDirection);
        Vector2 p2 = circleCenter;

        Vector2 toCenter = p2 - p1;
        float projectionLength = Math.Max(0, Vector2.Dot(toCenter, d1));
        Vector2 closestPointOnRay = p1 + projectionLength * d1;

        Vector2 offset = Vector2.Normalize(closestPointOnRay - p2) * radius;
        Vector2 closestPointOnCircle = p2 + offset;

        return (closestPointOnRay, closestPointOnCircle);
    }
    */
   
    #endregion

    #region Intersections

    public static bool IsPointOnRay(Vector2 point, Vector2 rayPoint, Vector2 rayDirection)
    {
        // Calculate the vector from the ray point to the given point
        var toPoint = point - rayPoint;

        // Calculate the dot product of the direction vector and the vector to the point
        float dotProduct = Vector2.Dot(toPoint, rayDirection);

        // Check if the point is in the same direction as the ray and on the line
        return dotProduct >= 0 && Line.IsPointOnLine(point, rayPoint, rayDirection);
    }
    
    
    public static (CollisionPoint p, float t) IntersectRaySegmentInfo(Vector2 rayPoint, Vector2 rayDirection, Vector2 segmentStart, Vector2 segmentEnd)
    {
        float denominator = rayDirection.X * (segmentEnd.Y - segmentStart.Y) - rayDirection.Y * (segmentEnd.X - segmentStart.X);

        if (Math.Abs(denominator) < 1e-10)
        {
            return (new(), -1f);
        }

        var difference = segmentStart - rayPoint;
        float t = (difference.X * (segmentEnd.Y - segmentStart.Y) - difference.Y * (segmentEnd.X - segmentStart.X)) / denominator;
        float u = (difference.X * rayDirection.Y - difference.Y * rayDirection.X) / denominator;

        if (t >= 0 && u >= 0 && u <= 1)
        {
            var intersection = rayPoint + t * rayDirection;
            var segmentDirection = Vector2.Normalize(segmentEnd - segmentStart);
            var normal = new Vector2(-segmentDirection.Y, segmentDirection.X);
            return (new(intersection, normal), t);
        }

        return (new(), -1f);
    }
    public static (CollisionPoint p, float t) IntersectRaySegmentInfo(Vector2 rayPoint, Vector2 rayDirection, Vector2 segmentStart, Vector2 segmentEnd, Vector2 segmentNormal)
    {
        var result = IntersectRaySegmentInfo(rayPoint, rayDirection, segmentStart, segmentEnd);
        if (result.p.Valid)
        {
            return (new(result.p.Point, segmentNormal), result.t);
        }

        return (new(), -1f);
    }
    public static (CollisionPoint p, float t) IntersectRayLineInfo(Vector2 rayPoint, Vector2 rayDirection, Vector2 linePoint, Vector2 lineDirection)
    {
        float denominator = rayDirection.X * lineDirection.Y - rayDirection.Y * lineDirection.X;

        if (Math.Abs(denominator) < 1e-10)
        {
            return (new(), -1f);
        }

        var difference = linePoint - rayPoint;
        float t = (difference.X * lineDirection.Y - difference.Y * lineDirection.X) / denominator;

        if (t >= 0)
        {
            var intersection = rayPoint + t * rayDirection;
            var normal = new Vector2(-lineDirection.Y, lineDirection.X);
            normal = Vector2.Normalize(normal);
            return (new(intersection, normal), t);
        }

        return (new(), -1f);
    }
    public static (CollisionPoint p, float t) IntersectRayLineInfo(Vector2 rayPoint, Vector2 rayDirection, Vector2 linePoint, Vector2 lineDirection, Vector2 lineNormal)
    {
        var result = IntersectRayLineInfo(rayPoint, rayDirection, linePoint, lineDirection);
        if (result.p.Valid)
        {
            return (new(result.p.Point, lineNormal), result.t);
        }

        return (new(), -1f);
    }
    public static (CollisionPoint p, float t) IntersectRayRayInfo(Vector2 ray1Point, Vector2 ray1Direction, Vector2 ray2Point, Vector2 ray2Direction)
    {
        float denominator = ray1Direction.X * ray2Direction.Y - ray1Direction.Y * ray2Direction.X;

        if (Math.Abs(denominator) < 1e-10)
        {
            return (new(), -1f);
        }

        var difference = ray2Point - ray1Point;
        float t = (difference.X * ray2Direction.Y - difference.Y * ray2Direction.X) / denominator;
        float u = (difference.X * ray1Direction.Y - difference.Y * ray1Direction.X) / denominator;

        if (t >= 0 && u >= 0)
        {
            var intersection = ray1Point + t * ray1Direction;
            var normal = new Vector2(-ray2Direction.Y, ray2Direction.X);
            normal = Vector2.Normalize(normal);
            return (new(intersection, normal), t);
        }

        return (new(), -1f);
    }
    public static (CollisionPoint p, float t) IntersectRayRayInfo(Vector2 ray1Point, Vector2 ray1Direction, Vector2 ray2Point, Vector2 ray2Direction, Vector2 ray2Normal)
    {
        var result = IntersectRayRayInfo(ray1Point, ray1Direction, ray2Point, ray2Direction);
        if (result.p.Valid)
        {
            return (new(result.p.Point, ray2Normal), result.t);
        }

        return (new(), -1f);
    }
    
    
    public static CollisionPoint IntersectRaySegment(Vector2 rayPoint, Vector2 rayDirection, Vector2 segmentStart, Vector2 segmentEnd)
    {
        float denominator = rayDirection.X * (segmentEnd.Y - segmentStart.Y) - rayDirection.Y * (segmentEnd.X - segmentStart.X);

        if (Math.Abs(denominator) < 1e-10)
        {
            return new();
        }

        var difference = segmentStart - rayPoint;
        float t = (difference.X * (segmentEnd.Y - segmentStart.Y) - difference.Y * (segmentEnd.X - segmentStart.X)) / denominator;
        float u = (difference.X * rayDirection.Y - difference.Y * rayDirection.X) / denominator;

        if (t >= 0 && u >= 0 && u <= 1)
        {
            var intersection = rayPoint + t * rayDirection;
            var segmentDirection = Vector2.Normalize(segmentEnd - segmentStart);
            var normal = new Vector2(-segmentDirection.Y, segmentDirection.X);
            return new(intersection, normal);
        }

        return new();
    }
    public static CollisionPoint IntersectRaySegment(Vector2 rayPoint, Vector2 rayDirection, Vector2 segmentStart, Vector2 segmentEnd, Vector2 segmentNormal)
    {
        var result = IntersectRaySegment(rayPoint, rayDirection, segmentStart, segmentEnd);
        if (result.Valid)
        {
            return new(result.Point, segmentNormal);
        }

        return new();
    }
    
    public static CollisionPoint IntersectRayLine(Vector2 rayPoint, Vector2 rayDirection, Vector2 linePoint, Vector2 lineDirection)
    {
        float denominator = rayDirection.X * lineDirection.Y - rayDirection.Y * lineDirection.X;

        if (Math.Abs(denominator) < 1e-10)
        {
            return new();
        }

        var difference = linePoint - rayPoint;
        float t = (difference.X * lineDirection.Y - difference.Y * lineDirection.X) / denominator;

        if (t >= 0)
        {
            var intersection = rayPoint + t * rayDirection;
            var normal = new Vector2(-lineDirection.Y, lineDirection.X);
            normal = Vector2.Normalize(normal);
            return new(intersection, normal);
        }

        return new();
    }
    public static CollisionPoint IntersectRayLine(Vector2 rayPoint, Vector2 rayDirection, Vector2 linePoint, Vector2 lineDirection, Vector2 lineNormal)
    {
        var result = IntersectRayLine(rayPoint, rayDirection, linePoint, lineDirection);
        if (result.Valid)
        {
            return new(result.Point, lineNormal);
        }

        return new();
    }
    
    public static CollisionPoint IntersectRayRay(Vector2 ray1Point, Vector2 ray1Direction, Vector2 ray2Point, Vector2 ray2Direction)
    {
        float denominator = ray1Direction.X * ray2Direction.Y - ray1Direction.Y * ray2Direction.X;

        if (Math.Abs(denominator) < 1e-10)
        {
            return new();
        }

        var difference = ray2Point - ray1Point;
        float t = (difference.X * ray2Direction.Y - difference.Y * ray2Direction.X) / denominator;
        float u = (difference.X * ray1Direction.Y - difference.Y * ray1Direction.X) / denominator;

        if (t >= 0 && u >= 0)
        {
            var intersection = ray1Point + t * ray1Direction;
            var normal = new Vector2(-ray2Direction.Y, ray2Direction.X);
            normal = Vector2.Normalize(normal);
            return new(intersection, normal);
        }

        return new();
    }
    public static CollisionPoint IntersectRayRay(Vector2 ray1Point, Vector2 ray1Direction, Vector2 ray2Point, Vector2 ray2Direction, Vector2 ray2Normal)
    {
        var result = IntersectRayRay(ray1Point, ray1Direction, ray2Point, ray2Direction);
        if (result.Valid)
        {
            return new(result.Point, ray2Normal);
        }

        return new();
    }
    
    public static (CollisionPoint a, CollisionPoint b) IntersectRayCircle(Vector2 rayPoint, Vector2 rayDirection, Vector2 circleCenter, float radius)
    {
        var toCircle = circleCenter - rayPoint;
        float projectionLength = Vector2.Dot(toCircle, rayDirection);
        var closestPoint = rayPoint + projectionLength * rayDirection;
        float distanceToCenter = Vector2.Distance(closestPoint, circleCenter);

        if (distanceToCenter < radius)
        {
            var offset = (float)Math.Sqrt(radius * radius - distanceToCenter * distanceToCenter);
            var intersection1 = closestPoint - offset * rayDirection;
            var intersection2 = closestPoint + offset * rayDirection;

            CollisionPoint a = new();
            CollisionPoint b = new();
            if (Vector2.Dot(intersection1 - rayPoint, rayDirection) >= 0)
            {
                var normal1 = Vector2.Normalize(intersection1 - circleCenter);
                a = new(intersection1, normal1);
            }

            if (Vector2.Dot(intersection2 - rayPoint, rayDirection) >= 0)
            {
                var normal2 = Vector2.Normalize(intersection2 - circleCenter);
                b = new(intersection2, normal2);
                
            }
            return (a, b);
        }
        else if (Math.Abs(distanceToCenter - radius) < 1e-10)
        {
            if (Vector2.Dot(closestPoint - rayPoint, rayDirection) >= 0)
            {
                var cp = new CollisionPoint(closestPoint, Vector2.Normalize(closestPoint - circleCenter));
                return (cp, new());
            }
        }

        return (new(), new());
    }
    public static (CollisionPoint a, CollisionPoint b) IntersectRayTriangle(Vector2 rayPoint, Vector2 rayDirection, Vector2 a, Vector2 b, Vector2 c)
    {
        CollisionPoint resultA = new();
        CollisionPoint resultB = new();
        
        var cp = IntersectRaySegment(rayPoint, rayDirection,  a, b);
        if(cp.Valid) resultA = cp;
        
        cp = IntersectRaySegment(rayPoint, rayDirection,  b, c);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }
        
        if(resultA.Valid && resultB.Valid) return (resultA, resultB);
       
        cp = IntersectRaySegment(rayPoint, rayDirection,  c, a);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }
        
        return (resultA, resultB);
    }
    public static (CollisionPoint a, CollisionPoint b) IntersectRayQuad(Vector2 rayPoint, Vector2 rayDirection, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        CollisionPoint resultA = new();
        CollisionPoint resultB = new();
        
        var cp = IntersectRaySegment(rayPoint, rayDirection,  a, b);
        if(cp.Valid) resultA = cp;
        
        cp = IntersectRaySegment(rayPoint, rayDirection,  b, c);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }
        
        if(resultA.Valid && resultB.Valid) return (resultA, resultB);
       
        cp = IntersectRaySegment(rayPoint, rayDirection,  c, d);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }
        
        if(resultA.Valid && resultB.Valid) return (resultA, resultB);
        
        cp = IntersectRaySegment(rayPoint, rayDirection,  d, a);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }
        
        return (resultA, resultB);
    }
    public static (CollisionPoint a, CollisionPoint b) IntersectRayRect(Vector2 rayPoint, Vector2 rayDirection, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return IntersectRayQuad(rayPoint, rayDirection, a, b, c, d);
    }
    public static CollisionPoints? IntersectRayPolygon(Vector2 rayPoint, Vector2 rayDirection, List<Vector2> points, int maxCollisionPoints = -1)
    {
        if (points.Count < 3) return null;
        if (maxCollisionPoints == 0) return null;
        CollisionPoints? result = null;
        for (var i = 0; i < points.Count; i++)
        {
            var colPoint = IntersectRaySegment(rayPoint, rayDirection, points[i], points[(i + 1) % points.Count]);
            if (colPoint.Valid)
            {
                result ??= new();
                result.Add(colPoint);
                if(maxCollisionPoints > 0 && result.Count >= maxCollisionPoints) return result;
            }
        }
        return result;
    }
    public static CollisionPoints? IntersectRayPolyline(Vector2 rayPoint, Vector2 rayDirection, List<Vector2> points, int maxCollisionPoints = -1)
    {
        if (points.Count < 3) return null;
        if (maxCollisionPoints == 0) return null;
        CollisionPoints? result = null;
        for (var i = 0; i < points.Count - 1; i++)
        {
            var colPoint = IntersectRaySegment(rayPoint, rayDirection, points[i], points[i + 1]);
            if (colPoint.Valid)
            {
                result ??= new();
                result.Add(colPoint);
                if(maxCollisionPoints > 0 && result.Count >= maxCollisionPoints) return result;
            }
        }
        return result;
    }
    public static CollisionPoints? IntersectRaySegments(Vector2 rayPoint, Vector2 rayDirection, List<Segment> segments, int maxCollisionPoints = -1)
    {
        if (segments.Count <= 0) return null;
        if (maxCollisionPoints == 0) return null;
        CollisionPoints? result = null;

        foreach (var seg in segments)
        {
            var colPoint = IntersectRaySegment(rayPoint, rayDirection, seg.Start, seg.End);
            if (colPoint.Valid)
            {
                result ??= new();
                result.AddRange(colPoint);
                if(maxCollisionPoints > 0 && result.Count >= maxCollisionPoints) return result;
            }
        }
        return result;
    }

    
    public CollisionPoint IntersectSegment(Vector2 segmentStart, Vector2 segmentEnd) => IntersectRaySegment(Point, Direction, segmentStart, segmentEnd);
    public CollisionPoint IntersectSegment(Segment segment) => IntersectRaySegment(Point, Direction, segment.Start, segment.End, segment.Normal);
    public CollisionPoint IntersectLine(Vector2 linePoint, Vector2 lineDirection) => IntersectRayLine(Point, Direction, linePoint, lineDirection);
    public CollisionPoint IntersectLine(Line line) => IntersectRayLine(Point, Direction, line.Point, line.Direction, line.Normal);
    public CollisionPoint IntersectRay(Vector2 rayPoint, Vector2 rayDirection) => IntersectRayRay(Point, Direction, rayPoint, rayDirection);
    public CollisionPoint IntersectRay(Ray ray) => IntersectRayRay(Point, Direction, ray.Point, ray.Direction, ray.Normal);
    public (CollisionPoint a, CollisionPoint b) IntersectCircle(Vector2 circleCenter, float circleRadius) => IntersectRayCircle(Point, Direction, circleCenter, circleRadius);
    public (CollisionPoint a, CollisionPoint b) IntersectCircle(Circle circle) => IntersectRayCircle(Point, Direction, circle.Center, circle.Radius);
    public (CollisionPoint a, CollisionPoint b) IntersectTriangle(Vector2 a, Vector2 b, Vector2 c) => IntersectRayTriangle(Point, Direction, a, b, c);
    public (CollisionPoint a, CollisionPoint b) IntersectTriangle(Triangle triangle) => IntersectRayTriangle(Point, Direction, triangle.A, triangle.B, triangle.C);
    public (CollisionPoint a, CollisionPoint b) IntersectQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => IntersectRayQuad(Point, Direction, a, b, c, d);
    public (CollisionPoint a, CollisionPoint b) IntersectQuad(Quad quad) => IntersectRayQuad(Point, Direction, quad.A, quad.B, quad.C, quad.D);
    public (CollisionPoint a, CollisionPoint b) IntersectRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => IntersectRayQuad(Point, Direction, a, b, c, d);
    public (CollisionPoint a, CollisionPoint b) IntersectRect(Rect rect) => IntersectRayQuad(Point, Direction, rect.A, rect.B, rect.C, rect.D);
    
    public CollisionPoints? IntersectPolygon(List<Vector2> points, int maxCollisionPoints = -1) => IntersectRayPolygon(Point, Direction, points, maxCollisionPoints);
    public CollisionPoints? IntersectPolygon(Polygon polygon, int maxCollisionPoints = -1) => IntersectRayPolygon(Point, Direction, polygon, maxCollisionPoints);
    public CollisionPoints? IntersectPolyline(List<Vector2> points, int maxCollisionPoints = -1) => IntersectRayPolyline(Point, Direction, points, maxCollisionPoints);
    public CollisionPoints? IntersectPolyline(Polyline polyline, int maxCollisionPoints = -1) => IntersectRayPolyline(Point, Direction, polyline, maxCollisionPoints);
    public CollisionPoints? IntersectSegments(List<Segment> segments, int maxCollisionPoints = -1) => IntersectRaySegments(Point, Direction, segments, maxCollisionPoints);
    public CollisionPoints? IntersectSegments(Segments segments, int maxCollisionPoints = -1) => IntersectRaySegments(Point, Direction, segments, maxCollisionPoints);
    
    
    public CollisionPoints? IntersectShape(Segment segment)
    {
        var result = IntersectRaySegment(Point, Direction, segment.Start, segment.End, segment.Normal);
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
        var result = IntersectRayLine(Point, Direction, line.Point, line.Direction, line.Normal);
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
        var result = IntersectRayRay(Point, Direction, ray.Point, ray.Direction, ray.Normal);
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
        var result = IntersectCircle(circle);
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
        var result = IntersectRayTriangle(Point, Direction, t.A, t.B, t.C);
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
        var result =  IntersectRayQuad(Point, Direction, q.A, q.B, q.C, q.D);
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
        var result = IntersectRayQuad(Point, Direction, r.A, r.B, r.C, r.D);
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
    
    public CollisionPoints? IntersectShape(Polygon p, int maxCollisionPoints = -1) => IntersectRayPolygon(Point, Direction, p, maxCollisionPoints);
    public CollisionPoints? IntersectShape(Polyline pl, int maxCollisionPoints = -1) => IntersectRayPolyline(Point, Direction, pl, maxCollisionPoints);
    public CollisionPoints? IntersectShape(Segments segments, int maxCollisionPoints = -1) => IntersectRaySegments(Point, Direction, segments, maxCollisionPoints);

    #endregion
    
    #region Overlap

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
            return true;
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
        if (Triangle.ContainsPoint(a, b, c, rayPoint)) return true;
        
        var cp = IntersectRaySegment(rayPoint, rayDirection,  a, b);
        if (cp.Valid) return true;
        
        cp = IntersectRaySegment(rayPoint, rayDirection,  b, c);
        if (cp.Valid) return true;
       
        cp= IntersectRaySegment(rayPoint, rayDirection,  c, a);
        if (cp.Valid) return true;

        return false;
    }
    public static bool OverlapRayQuad(Vector2 rayPoint, Vector2 rayDirection, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        if (Quad.ContainsPoint(a, b, c, d,  rayPoint)) return true;
        
        var cp = IntersectRaySegment(rayPoint, rayDirection,  a, b);
        if (cp.Valid) return true;
        
        cp = IntersectRaySegment(rayPoint, rayDirection,  b, c);
        if (cp.Valid) return true;
       
        cp = IntersectRaySegment(rayPoint, rayDirection,  c, d);
        if (cp.Valid) return true;

        cp = IntersectRaySegment(rayPoint, rayDirection,  d, a);
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
    public static bool OverlapRaySegments(Vector2 rayPoint, Vector2 rayDirection, List<Segment> segments)
    {
        if (segments.Count <= 0) return false;

        foreach (var seg in segments)
        {
            var result = IntersectRaySegment(rayPoint, rayDirection, seg.Start, seg.End);
            if (result.Valid) return true;
        }
        return false;
    }

    
    public bool OverlapShape(Segment segment) => OverlapRaySegment(Point, Direction, segment.Start, segment.End);
    public bool OverlapShape(Line line) => OverlapRayLine(Point, Direction, line.Point, line.Direction);
    public bool OverlapShape(Ray ray) => OverlapRayRay(Point, Direction, ray.Point, ray.Direction);
    public bool OverlapShape(Circle circle) => OverlapRayCircle(Point, Direction, circle.Center, circle.Radius);
    public bool OverlapShape(Triangle t) => OverlapRayTriangle(Point, Direction, t.A, t.B, t.C);
    public bool OverlapShape(Quad q) => OverlapRayQuad(Point, Direction, q.A, q.B, q.C, q.D);
    public bool OverlapShape(Rect r) => OverlapRayQuad(Point, Direction, r.A, r.B, r.C, r.D);
    public bool OverlapShape(Polygon p) => OverlapRayPolygon(Point, Direction, p);
    public bool OverlapShape(Polyline pl) => OverlapRayPolyline(Point, Direction, pl);
    public bool OverlapShape(Segments segments) => OverlapRaySegments(Point, Direction, segments);

    #endregion
}