using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;
using ShapeEngine.Random;
namespace ShapeEngine.Core.Shapes;

public readonly struct Segment : IEquatable<Segment>
{
    #region Members
    public readonly Vector2 Start;
    public readonly Vector2 End;
    public readonly Vector2 Normal;
    #endregion

    #region Getter Setter
    public Vector2 Center => (Start + End) * 0.5f;
    public Vector2 Dir => Displacement.Normalize();
    public Vector2 Displacement => End - Start;
    public float Length => Displacement.Length();
    public float LengthSquared => Displacement.LengthSquared();

    #endregion

    #region Constructor
    public Segment(Vector2 start, Vector2 end, bool flippedNormal = false) 
    { 
        this.Start = start; 
        this.End = end;
        this.Normal = GetNormal(start, end, flippedNormal);
        // this.flippedNormals = flippedNormals;
    }
    internal Segment(Vector2 start, Vector2 end, Vector2 normal) 
    { 
        this.Start = start; 
        this.End = end;
        this.Normal = normal;
    }
    public Segment(float startX, float startY, float endX, float endY, bool flippedNormal = false) 
    { 
        this.Start = new(startX, startY); 
        this.End = new(endX, endY);
        this.Normal = GetNormal(Start, End, flippedNormal);
        // this.flippedNormals = flippedNormals;
    }

    public Segment(Vector2 origin, float length, float rotRad, float originOffset = 0.5f, bool flippedNormal = false)
    {
        var dir = ShapeVec.VecFromAngleRad(rotRad);
        this.Start = origin - dir * originOffset * length;
        this.End = origin + dir * (1f - originOffset) * length;
        this.Normal = GetNormal(Start, End, flippedNormal);
    }
    #endregion

    #region Math

    public Segment Floor()
    {
        return new(Start.Floor(), End.Floor());
    }
    public Segment Ceiling()
    {
        return new(Start.Ceiling(), End.Ceiling());
    }
    public Segment Round()
    {
        return new(Start.Round(), End.Round());
    }
    public Segment Truncate()
    {
        return new(Start.Truncate(), End.Truncate());
    }
    public Points? GetProjectedShapePoints(Vector2 v)
    {
        if (v.LengthSquared() <= 0f) return null;
        var points = new Points
        {
            Start,
            End,
            Start + v,
            End + v,
        };
        return points;
    }
    public Polygon? ProjectShape(Vector2 v)
    {
        if (v.LengthSquared() <= 0f) return null;
        var points = new Points
        {
            Start,
            End,
            Start + v,
            End + v,
        };
        return Polygon.FindConvexHull(points);
    }

    #endregion

    #region Shapes
    public readonly Rect GetBoundingBox() { return new(Start, End); }

    public Polyline ToPolyline() { return new Polyline() {Start, End}; }
    public Segments GetEdges() { return new Segments(){this}; }
    public Points Inflate(float thickness, float alignement = 0.5f)
    {
        var dir = Dir;
        var left = dir.GetPerpendicularLeft();
        var right = dir.GetPerpendicularRight();
        var a = Start + left * thickness * alignement;
        var b = Start + right * thickness * (1 - alignement);
        var c = End + right * thickness * (1 - alignement);
        var d = End + left * thickness * alignement;

        return new() { a, b, c, d };
    }

    public Segments Split(float f)
    {
        return Split(this.GetPoint(f));
    }
    public Segments Split(Vector2 splitPoint)
    {
        var a = new Segment(Start, splitPoint);
        var b = new Segment(splitPoint, End);
        return new() { a, b };
    }


    #endregion

    #region Point & Vertext

    public Vector2 GetPoint(float f) { return Start.Lerp(End, f); }
    public Points GetVertices()
    {
        var points = new Points
        {
            Start,
            End
        };
        return points;
    }

    public Vector2 GetRandomPoint() { return this.GetPoint(Rng.Instance.RandF()); }
    public Points GetRandomPoints(int amount)
    {
        var points = new Points();
        for (int i = 0; i < amount; i++)
        {
            points.Add(GetRandomPoint());
        }
        return points;
    }
    public Vector2 GetRandomVertex() { return Rng.Instance.Chance(0.5f) ? Start : End; }


    #endregion
    
    #region Transform
    public static (Vector2 newStart, Vector2 newEnd) ScaleLength(Vector2 start, Vector2 end, float scale, float originF = 0.5f)
    {
        var p = start.Lerp(end, originF);
        var s = start - p;
        var e = end - p;
        return new (p + s * scale, p + e * scale);
    }
    public Segment ScaleLength(float scale, float originF = 0.5f)
    {
        var p = GetPoint(originF);
        var s = Start - p;
        var e = End - p;
        return new Segment(p + s * scale, p + e * scale);
    }
    public Segment ScaleLength(Size scale, float originF = 0.5f)
    {
        var p = GetPoint(originF);
        var s = Start - p;
        var e = End - p;
        return new Segment(p + s * scale, p + e * scale);
    }

    private static Vector2 ChangeLength(Vector2 from, Vector2 to, float amount)
    {
        var w = (to - from);
        var lSq = w.LengthSquared();
        if (lSq <= 0) return from;
        var l = MathF.Sqrt(lSq);
        var dir = w / l;
        return from + dir * (l + amount);
    }
    public Segment ChangeLengthFromStart(float amount)
    {
        var newEnd = ChangeLength(Start, End, amount);
        return new(Start, newEnd);
        // var w = (End - Start);
        // var lSq = w.LengthSquared();
        // if (lSq <= 0) return new(Start, Start);
        // var l = MathF.Sqrt(lSq);
        // var dir = w / l;
        // return new(Start, Start + dir * (l + amount));
    }
    public Segment ChangeLengthFromEnd(float amount)
    {
        var newStart = ChangeLength(End, Start, amount);
        return new(newStart, End);
        // var w = (Start - End);
        // var lSq = w.LengthSquared();
        // if (lSq <= 0) return new(End, End);
        // var l = MathF.Sqrt(lSq);
        // var dir = w / l;
        // return new(End + dir * (l + amount), End);
    }
    /// <summary>
    /// Changes the length of the segment based on an origin point. OriginF 0 = Start, 0.5 = Center, 1 = End
    /// Splits the amount based on originF.
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="originF"></param>
    /// <returns></returns>
    public Segment ChangeLength(float amount, float originF = 0.5f)
    {
        if (amount == 0) return this;
        if (originF <= 0f) return ChangeLengthFromStart(amount);
        if (originF >= 1f) return ChangeLengthFromEnd(amount);
        
        var p = GetPoint(originF);
        var newStart = ChangeLength(p, Start, amount * (1f - originF));
        var newEnd = ChangeLength(p, End, amount * originF);
        return new(newStart, newEnd);
    }

    private static Vector2 SetLength(Vector2 from, Vector2 to, float length)
    {
        if (length <= 0f) return from;
        var w = (to - from);
        var lSq = w.LengthSquared();
        if (lSq <= 0) return from;
        var l = MathF.Sqrt(lSq);
        var dir = w / l;
        return from + dir * length;
    }
    public Segment SetLengthFromStart(float length)
    {
        var newEnd = SetLength(Start, End, length);
        return new(Start, newEnd);
    }
    public Segment SetLengthFromEnd(float length)
    {
        var newStart = SetLength(End, Start, length);
        return new(newStart, End);
    }
    
    /// <summary>
    /// Sets the length of the segment based on an origin point. OriginF 0 = Start, 0.5 = Center, 1 = End
    /// Splits the length based on originF.
    /// </summary>
    /// <param name="length"></param>
    /// <param name="originF"></param>
    /// <returns></returns>
    public Segment SetLength(float length, float originF = 0.5f)
    {
        if (originF <= 0f) return SetLengthFromStart(length);
        if (originF >= 1f) return SetLengthFromEnd(length);
        
        var p = GetPoint(originF);
        var newStart = SetLength(p, Start, length * (1f - originF));
        var newEnd = SetLength(p, End, length * originF);
        return new(newStart, newEnd);
        
    }
    
    public Segment SetStart(Vector2 position) { return new(position, End); }
    public Segment ChangeStart(Vector2 offset) { return new(Start + offset, End); }
    public Segment SetEnd(Vector2 position) { return new(Start, position); }
    public Segment ChangeEnd(Vector2 offset) { return new(Start, End + offset); }
    public Segment ChangePosition(Vector2 offset) { return new(Start + offset, End + offset); }
    public Segment ChangePosition(float x, float y) { return ChangePosition(new Vector2(x, y)); }
    public Segment ChangePosition(Vector2 offset, float f) { return new(Start + (offset * (1f - f)), End + (offset * f)); }
    public Segment SetPosition(Vector2 position, float originF = 0.5f)
    {
        var point = GetPoint(originF);
        var offset = position - point;
        return ChangePosition(offset);
    }
    public Segment ChangeRotation(float angleRad, float originF = 0.5f)
    {
        var p = GetPoint(originF);
        var s = Start - p;
        var e = End - p;
        return new Segment(p + s.Rotate(angleRad), p + e.Rotate(angleRad));
    }

    // public Segment RotateTo(float fromAngleRad, float toAngleRad, float originF = 0.5f)
    // {
    //     var amountRad = ShapeMath.GetShortestAngleRad(fromAngleRad, toAngleRad);
    //     return RotateBy(amountRad, originF);
    // }
    //
    public Segment SetRotation(float angleRad, float originF = 0.5f)
    {
        if (originF <= 0f) return RotateStartTo(angleRad);
        if (originF >= 1f) return RotateEndTo(angleRad);
        
        var origin = GetPoint(originF);
        var fromAngleRad = (origin - Start).AngleRad();
        var amountRad = ShapeMath.GetShortestAngleRad(fromAngleRad, angleRad);
        return ChangeRotation(amountRad, originF);
    }
    public Segment RotateStartTo(float toAngleRad)
    {
        var fromAngleRad = (Start - End).AngleRad();
        var amountRad = ShapeMath.GetShortestAngleRad(fromAngleRad, toAngleRad);
        return ChangeRotation(amountRad, 1f);
    }
    public Segment RotateEndTo(float toAngleRad)
    {
        var fromAngleRad = (End - Start).AngleRad();
        var amountRad = ShapeMath.GetShortestAngleRad(fromAngleRad, toAngleRad);
        return ChangeRotation(amountRad, 0f);
    }
   
    /// <summary>
    /// Moves the segment by transform.Position
    /// Rotates the moved segment by transform.RotationRad
    /// Changes length of the rotated segment by transform.Size.Width!
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="originF"></param>
    /// <returns></returns>
    public Segment ApplyOffset(Transform2D offset, float originF = 0.5f)
    {
        var newSegment = ChangePosition(offset.Position, originF);
        newSegment = newSegment.ChangeRotation(offset.RotationRad, originF);
        return newSegment.ChangeLength(offset.ScaledSize.Length, originF);
    }

    /// <summary>
    /// Moves the segment to transform.Position
    /// Rotates the moved segment to transform.RotationRad
    /// Set the length of the rotated segment to transform.Size.Width
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="originF"></param>
    /// <returns></returns>
    public Segment SetTransform(Transform2D transform, float originF = 0.5f)
    {
        var newSegment = SetPosition(transform.Position, originF);
        newSegment = newSegment.SetRotation(transform.RotationRad, originF);
        return newSegment.SetLength(transform.ScaledSize.Length, originF);
    }

    #endregion
    
    #region Static
    public static Vector2 GetNormal(Vector2 start, Vector2 end, bool flippedNormal)
    {
        if (flippedNormal) return (end - start).GetPerpendicularLeft().Normalize();
        else return (end - start).GetPerpendicularRight().Normalize();
    }
    public static bool IsPointOnSegment(Vector2 point, Vector2 start, Vector2 end)
    {
        float minX = Math.Min(start.X, end.X);
        float maxX = Math.Max(start.X, end.X);
        float minY = Math.Min(start.Y, end.Y);
        float maxY = Math.Max(start.Y, end.Y);

        return point.X >= minX && point.X <= maxX && point.Y >= minY && point.Y <= maxY;
        
        //old version
        // var d = end - start;
        // var lp = point - start;
        // var p = lp.Project(d);
        // return lp.IsSimilar(p) && p.LengthSquared() <= d.LengthSquared() && Vector2.Dot(p, d) >= 0.0f;
    }
    
    public static ValueRange ProjectSegment(Vector2 aPos, Vector2 aEnd, Vector2 onto)
    {
        var unitOnto = onto.Normalize();
        ValueRange r = new(unitOnto.Dot(aPos), unitOnto.Dot(aEnd));
        return r;
    }
    public static bool SegmentOnOneSide(Vector2 axisPos, Vector2 axisDir, Vector2 segmentStart, Vector2 segmentEnd)
    {
        var d1 = segmentStart - axisPos;
        var d2 = segmentEnd - axisPos;
        var n = axisDir.Rotate90CCW();// new(-axisDir.Y, axisDir.X);
        return Vector2.Dot(n, d1) * Vector2.Dot(n, d2) > 0.0f;
    }
 
    public static (CollisionPoint point, float time) IntersectSegmentSegmentInfo(Vector2 segment1Start, Vector2 segment1End, Vector2 segment2Start, Vector2 segment2End)
    {
        // Calculate the direction vectors of the segments
        var dir1 = segment1End - segment1Start;
        var dir2 = segment2End - segment2Start;

        float denominator = dir1.X * dir2.Y - dir1.Y * dir2.X;

        // Check if segments are parallel (denominator is zero)
        if (Math.Abs(denominator) < 1e-10)
        {
            return (new(), -1);
        }

        var diff = segment2Start - segment1Start;
        float t = (diff.X * dir2.Y - diff.Y * dir2.X) / denominator;
        float u = (diff.X * dir1.Y - diff.Y * dir1.X) / denominator;

        // Check if the intersection point is within both segments
        if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
        {
            var intersection = segment1Start + t * dir1;

            // Calculate the normal vector as perpendicular to the direction of the first segment
            var normal = new Vector2(-dir1.Y, dir1.X).Normalize();

            return (new CollisionPoint(intersection, normal), t);
        }

        return (new(), -1);
    }
    public static (CollisionPoint point, float time) IntersectSegmentSegmentInfo(Vector2 segment1Start, Vector2 segment1End, Vector2 segment2Start, Vector2 segment2End, Vector2 segment2Normal)
    {
        // Calculate the direction vectors of the segments
        var dir1 = segment1End - segment1Start;
        var dir2 = segment2End - segment2Start;

        float denominator = dir1.X * dir2.Y - dir1.Y * dir2.X;

        // Check if segments are parallel (denominator is zero)
        if (Math.Abs(denominator) < 1e-10)
        {
            return (new(), -1);
        }

        var diff = segment2Start - segment1Start;
        float t = (diff.X * dir2.Y - diff.Y * dir2.X) / denominator;
        float u = (diff.X * dir1.Y - diff.Y * dir1.X) / denominator;

        // Check if the intersection point is within both segments
        if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
        {
            var intersection = segment1Start + t * dir1;

            return (new CollisionPoint(intersection, segment2Normal), t);
        }

        return (new(), -1);
    }
    public static (CollisionPoint point, float time) IntersectSegmentLineInfo(Vector2 segmentStart, Vector2 segmentEnd, Vector2 linePoint, Vector2 lineDirection)
    {
        float denominator = (segmentEnd.X - segmentStart.X) * lineDirection.Y - (segmentEnd.Y - segmentStart.Y) * lineDirection.X;

        if (Math.Abs(denominator) < 1e-10)
        {
            return (new(), -1);
        }

        var difference = segmentStart - linePoint;
        float t = (difference.X * lineDirection.Y - difference.Y * lineDirection.X) / denominator;
        float u = (difference.X * (segmentEnd.Y - segmentStart.Y) - difference.Y * (segmentEnd.X - segmentStart.X)) / denominator;

        if (u >= 0 && u <= 1)
        {
            var intersection = segmentStart + t * (segmentEnd - segmentStart);
            var normal = new Vector2(-lineDirection.Y, lineDirection.X).Normalize();
            return (new(intersection, normal), t);
        }

        return (new(), -1);
    }
    public static (CollisionPoint point, float time) IntersectSegmentRayInfo(Vector2 segmentStart, Vector2 segmentEnd, Vector2 rayPoint, Vector2 rayDirection)
    {
        float denominator = (segmentEnd.X - segmentStart.X) * rayDirection.Y - (segmentEnd.Y - segmentStart.Y) * rayDirection.X;

        if (Math.Abs(denominator) < 1e-10)
        {
            return (new(), -1);
        }

        var difference = segmentStart - rayPoint;
        float t = (difference.X * rayDirection.Y - difference.Y * rayDirection.X) / denominator;
        float u = (difference.X * (segmentEnd.Y - segmentStart.Y) - difference.Y * (segmentEnd.X - segmentStart.X)) / denominator;

        if (u >= 0 && u <= 1 && t >= 0)
        {
            var intersection = segmentStart + t * (segmentEnd - segmentStart);
            var segmentDirection = (segmentEnd - segmentStart).Normalize();
            var normal = new Vector2(-segmentDirection.Y, segmentDirection.X);
            return (new(intersection, normal), t);
        }

        return (new(), -1);
    }
    public static (CollisionPoint point, float time) IntersectSegmentLineInfo(Vector2 segmentStart, Vector2 segmentEnd, Vector2 linePoint, Vector2 lineDirection, Vector2 lineNormal)
    {
        float denominator = (segmentEnd.X - segmentStart.X) * lineDirection.Y - (segmentEnd.Y - segmentStart.Y) * lineDirection.X;

        if (Math.Abs(denominator) < 1e-10)
        {
            return (new(), -1);
        }

        var difference = segmentStart - linePoint;
        float t = (difference.X * lineDirection.Y - difference.Y * lineDirection.X) / denominator;
        float u = (difference.X * (segmentEnd.Y - segmentStart.Y) - difference.Y * (segmentEnd.X - segmentStart.X)) / denominator;

        if (u >= 0 && u <= 1)
        {
            var intersection = segmentStart + t * (segmentEnd - segmentStart);
            return (new(intersection, lineNormal), t);
        }

        return (new(), -1);
    }
    public static (CollisionPoint point, float time) IntersectSegmentRayInfo(Vector2 segmentStart, Vector2 segmentEnd, Vector2 rayPoint, Vector2 rayDirection, Vector2 rayNormal)
    {
        float denominator = (segmentEnd.X - segmentStart.X) * rayDirection.Y - (segmentEnd.Y - segmentStart.Y) * rayDirection.X;

        if (Math.Abs(denominator) < 1e-10)
        {
            return (new(), -1);
        }

        var difference = segmentStart - rayPoint;
        float t = (difference.X * rayDirection.Y - difference.Y * rayDirection.X) / denominator;
        float u = (difference.X * (segmentEnd.Y - segmentStart.Y) - difference.Y * (segmentEnd.X - segmentStart.X)) / denominator;

        if (u >= 0 && u <= 1 && t >= 0)
        {
            var intersection = segmentStart + t * (segmentEnd - segmentStart);
            return (new(intersection, rayNormal), t);
        }

        return (new(), -1);
    }
    
    public static CollisionPoint IntersectSegmentLine(Vector2 segmentStart, Vector2 segmentEnd, Vector2 linePoint, Vector2 lineDirection)
    {
        float denominator = (segmentEnd.X - segmentStart.X) * lineDirection.Y - (segmentEnd.Y - segmentStart.Y) * lineDirection.X;

        if (Math.Abs(denominator) < 1e-10)
        {
            return new();
        }

        var difference = segmentStart - linePoint;
        float t = (difference.X * lineDirection.Y - difference.Y * lineDirection.X) / denominator;
        float u = (difference.X * (segmentEnd.Y - segmentStart.Y) - difference.Y * (segmentEnd.X - segmentStart.X)) / denominator;

        if (u >= 0 && u <= 1)
        {
            var intersection = segmentStart + t * (segmentEnd - segmentStart);
            var normal = new Vector2(-lineDirection.Y, lineDirection.X).Normalize();
            return new(intersection, normal);
        }

        return new();
    }
    public static CollisionPoint IntersectSegmentRay(Vector2 segmentStart, Vector2 segmentEnd, Vector2 rayPoint, Vector2 rayDirection)
    {
        float denominator = (segmentEnd.X - segmentStart.X) * rayDirection.Y - (segmentEnd.Y - segmentStart.Y) * rayDirection.X;

        if (Math.Abs(denominator) < 1e-10)
        {
            return new();
        }

        var difference = segmentStart - rayPoint;
        float t = (difference.X * rayDirection.Y - difference.Y * rayDirection.X) / denominator;
        float u = (difference.X * (segmentEnd.Y - segmentStart.Y) - difference.Y * (segmentEnd.X - segmentStart.X)) / denominator;

        if (u >= 0 && u <= 1 && t >= 0)
        {
            var intersection = segmentStart + t * (segmentEnd - segmentStart);
            var segmentDirection = (segmentEnd - segmentStart).Normalize();
            var normal = new Vector2(-segmentDirection.Y, segmentDirection.X);
            return new(intersection, normal);
        }

        return new();
    }
    public static CollisionPoint IntersectSegmentSegment(Vector2 segment1Start, Vector2 segment1End, Vector2 segment2Start, Vector2 segment2End)
    {
        //OLD VERSION
        // var info = IntersectSegmentSegmentInfo(aStart, aEnd, bStart, bEnd);
        // if (info.intersected)
        // {
        //     return new(info.intersectPoint, GetNormal(bStart, bEnd, false));
        // }
        // return new();
        // Calculate the direction vectors of the segments
        var dir1 = segment1End - segment1Start;
        var dir2 = segment2End - segment2Start;

        float denominator = dir1.X * dir2.Y - dir1.Y * dir2.X;

        // Check if segments are parallel (denominator is zero)
        if (Math.Abs(denominator) < 1e-10)
        {
            return new();
        }

        var diff = segment2Start - segment1Start;
        float t = (diff.X * dir2.Y - diff.Y * dir2.X) / denominator;
        float u = (diff.X * dir1.Y - diff.Y * dir1.X) / denominator;

        // Check if the intersection point is within both segments
        if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
        {
            var intersection = segment1Start + t * dir1;

            // Calculate the normal vector as perpendicular to the direction of the first segment
            var normal = new Vector2(-dir2.Y, dir2.X).Normalize();

            return new CollisionPoint(intersection, normal);
        }

        return new();
    }
    public static CollisionPoint IntersectSegmentLine(Vector2 segmentStart, Vector2 segmentEnd, Vector2 linePoint, Vector2 lineDirection, Vector2 lineNormal)
    {
        var result = IntersectSegmentLine(segmentStart, segmentEnd, linePoint, lineDirection);
        if (result.Valid)
        {
            return new(result.Point, lineNormal);
        }

        return new();
    }
    public static CollisionPoint IntersectSegmentRay(Vector2 segmentStart, Vector2 segmentEnd, Vector2 rayPoint, Vector2 rayDirection, Vector2 rayNormal)
    {
        var result = IntersectSegmentRay(segmentStart, segmentEnd, rayPoint, rayDirection);
        if (result.Valid)
        {
            return new(result.Point, rayNormal);
        }

        return new();
    }
    public static CollisionPoint IntersectSegmentSegment(Vector2 segment1Start, Vector2 segment1End, Vector2 segment2Start, Vector2 segment2End, Vector2 segment2Normal)
    {
        var result = IntersectSegmentSegment(segment1Start, segment1End, segment2Start, segment2End);
        if (result.Valid)
        {
            return new(result.Point, segment2Normal);
        }

        return new();
    }
    
    public static (CollisionPoint a, CollisionPoint b) IntersectSegmentCircle(Vector2 segmentStart, Vector2 segmentEnd, Vector2 circleCenter, float radius)
    {
        CollisionPoint a = new();
        CollisionPoint b = new();
        
        // Calculate the direction vector of the segment
        var segmentDirection = segmentEnd - segmentStart;
        var toCircle = circleCenter - segmentStart;

        // Projection of toCircle onto the segment direction to find the closest approach
        float projectionLength = Vector2.Dot(toCircle, segmentDirection) / segmentDirection.LengthSquared();
        var closestPoint = segmentStart + projectionLength * segmentDirection;

        // Distance from the closest point to the circle center
        float distanceToCenter = Vector2.Distance(closestPoint, circleCenter);

        if (distanceToCenter < radius)
        {
            // Calculate the distance from the closest point to the intersection points
            float offset = (float)Math.Sqrt(radius * radius - distanceToCenter * distanceToCenter);
            var intersection1 = closestPoint - offset * segmentDirection.Normalize();
            var intersection2 = closestPoint + offset * segmentDirection.Normalize();

            // Check if the intersection points are within the segment
            if (IsPointOnSegment(intersection1, segmentStart, segmentEnd))
            {
                var normal1 = (intersection1 - circleCenter).Normalize();
                a = new CollisionPoint(intersection1, normal1);
            }

            if (IsPointOnSegment(intersection2, segmentStart, segmentEnd))
            {
                var normal2 = (intersection2 - circleCenter).Normalize();
                if(a.Valid) b = new CollisionPoint(intersection2, normal2);
                else a = new CollisionPoint(intersection2, normal2);
                // results.Add((intersection2, normal2));
            }
        }
        else if (Math.Abs(distanceToCenter - radius) < 1e-10)
        {
            // The segment is tangent to the circle
            if (IsPointOnSegment(closestPoint, segmentStart, segmentEnd))
            {
                a = new(closestPoint, (closestPoint - circleCenter).Normalize());
            }
        }

        return (a, b);
    }
    
    public static (CollisionPoint a, CollisionPoint b) IntersectSegmentTriangle(Vector2 segmentStart, Vector2 segmentEnd, Vector2 a, Vector2 b, Vector2 c)
    {
        CollisionPoint resultA = new();
        CollisionPoint resultB = new();
        
        var cp = IntersectSegmentSegment(segmentStart, segmentEnd,  a, b);
        if(cp.Valid) resultA = cp;
        
        cp = IntersectSegmentSegment(segmentStart, segmentEnd,  b, c);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }
        
        if(resultA.Valid && resultB.Valid) return (resultA, resultB);
       
        cp = IntersectSegmentSegment(segmentStart, segmentEnd,  c, a);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }
        
        return (resultA, resultB);
    }
    public static (CollisionPoint a, CollisionPoint b) IntersectSegmentQuad(Vector2 segmentStart, Vector2 segmentEnd, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        CollisionPoint resultA = new();
        CollisionPoint resultB = new();
        
        var cp = IntersectSegmentSegment(segmentStart, segmentEnd,  a, b);
        if(cp.Valid) resultA = cp;
        
        cp = IntersectSegmentSegment(segmentStart, segmentEnd,  b, c);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }
        
        if(resultA.Valid && resultB.Valid) return (resultA, resultB);
       
        cp = IntersectSegmentSegment(segmentStart, segmentEnd,  c, d);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }
        
        if(resultA.Valid && resultB.Valid) return (resultA, resultB);
        
        cp = IntersectSegmentSegment(segmentStart, segmentEnd,  d, a);
        if (cp.Valid)
        {
            if (resultA.Valid) resultB = cp;
            else resultA = cp;
        }
        
        return (resultA, resultB);
    }
    public static (CollisionPoint a, CollisionPoint b) IntersectSegmentRect(Vector2 segmentStart, Vector2 segmentEnd, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return IntersectSegmentQuad(segmentStart, segmentEnd, a, b, c, d);
    }
    public static CollisionPoints? IntersectSegmentPolygon(Vector2 segmentStart, Vector2 segmentEnd, List<Vector2> points, int maxCollisionPoints = -1)
    {
        if (points.Count < 3) return null;
        if (maxCollisionPoints == 0) return null;
        CollisionPoints? result = null;
        for (var i = 0; i < points.Count; i++)
        {
            var colPoint = IntersectSegmentSegment(segmentStart, segmentEnd, points[i], points[(i + 1) % points.Count]);
            if (colPoint.Valid)
            {
                result ??= new();
                result.Add(colPoint);
                if(maxCollisionPoints > 0 && result.Count >= maxCollisionPoints) return result;
            }
            
        }
        return result;
    }
    public static CollisionPoints? IntersectSegmentPolyline(Vector2 segmentStart, Vector2 segmentEnd, List<Vector2> points, int maxCollisionPoints = -1)
    {
        if (points.Count < 3) return null;
        if (maxCollisionPoints == 0) return null;
        CollisionPoints? result = null;
        for (var i = 0; i < points.Count - 1; i++)
        {
            var colPoint = IntersectSegmentSegment(segmentStart, segmentEnd, points[i], points[i + 1]);
            if (colPoint.Valid)
            {
                result ??= new();
                result.Add(colPoint);
                if(maxCollisionPoints > 0 && result.Count >= maxCollisionPoints) return result;
            }
        }
        return result;
    }
    public static CollisionPoints? IntersectSegmentSegments(Vector2 segmentStart, Vector2 segmentEnd, List<Segment> segments, int maxCollisionPoints = -1)
    {
        if (segments.Count <= 0) return null;
        if (maxCollisionPoints == 0) return null;
        CollisionPoints? result = null;

        foreach (var seg in segments)
        {
            var colPoint = IntersectSegmentSegment(segmentStart, segmentEnd, seg.Start, seg.End);
            if (colPoint.Valid)
            {
                result ??= new();
                result.AddRange(colPoint);
                if(maxCollisionPoints > 0 && result.Count >= maxCollisionPoints) return result;
            }
        }
        return result;
    }
    
    public static bool OverlapSegmentSegment(Vector2 aStart, Vector2 aEnd, Vector2 bStart, Vector2 bEnd)
    {
        var axisAPos = aStart;
        var axisADir = aEnd - aStart;
        if (SegmentOnOneSide(axisAPos, axisADir, bStart, bEnd)) return false;

        var axisBPos = bStart;
        var axisBDir = bEnd - bStart;
        if (SegmentOnOneSide(axisBPos, axisBDir, aStart, aEnd)) return false;

        if (axisADir.Parallel(axisBDir))
        {
            var rangeA = ProjectSegment(aStart, aEnd, axisADir);
            var rangeB = ProjectSegment(bStart, bEnd, axisADir);
            return rangeA.OverlapValueRange(rangeB); // Rect.OverlappingRange(rangeA, rangeB);
        }
        return true;
    }
    public static bool OverlapSegmentLine(Vector2 segmentStart, Vector2 segmentEnd, Vector2 linePos, Vector2 lineDir)
    {
        return !SegmentOnOneSide(linePos, lineDir, segmentStart, segmentEnd);
    }

    public static bool OverlapSegmentRay(Vector2 segmentStart, Vector2 segmentEnd, Vector2 rayPoint, Vector2 rayDirection)
    {
        float denominator = (segmentEnd.X - segmentStart.X) * rayDirection.Y - (segmentEnd.Y - segmentStart.Y) * rayDirection.X;

        if (Math.Abs(denominator) < 1e-10)
        {
            return false;
        }

        var difference = segmentStart - rayPoint;
        float t = (difference.X * rayDirection.Y - difference.Y * rayDirection.X) / denominator;
        float u = (difference.X * (segmentEnd.Y - segmentStart.Y) - difference.Y * (segmentEnd.X - segmentStart.X)) / denominator;

        if (u >= 0 && u <= 1 && t >= 0)
        {
            return true;
        }

        return false;
    }
    public static bool OverlapSegmentCircle(Vector2 segStart, Vector2 segEnd, Vector2 circlePos, float circleRadius) => Circle.OverlapCircleSegment(circlePos, circleRadius, segStart, segEnd);
    
    #endregion

    #region Operators

    public static Segment operator +(Segment left, Segment right) => new(left.Start + right.Start, left.End + right.End);
    public static Segment operator -(Segment left, Segment right) => new(left.Start - right.Start, left.End - right.End);
    public static Segment operator *(Segment left, Segment right) => new(left.Start * right.Start, left.End * right.End);
    public static Segment operator /(Segment left, Segment right) => new(left.Start / right.Start, left.End / right.End);
    
    public static Segment operator +(Segment left, Vector2 right) => new(left.Start + right, left.End + right);
    public static Segment operator -(Segment left, Vector2 right) => new(left.Start - right, left.End - right);
    public static Segment operator *(Segment left, Vector2 right) => new(left.Start * right, left.End * right);
    public static Segment operator /(Segment left, Vector2 right) => new(left.Start / right, left.End / right);

    public static Segment operator +(Segment left, float right) => new(left.Start + new Vector2(right), left.End + new Vector2(right));
    public static Segment operator -(Segment left, float right) => new(left.Start - new Vector2(right), left.End - new Vector2(right));
    public static Segment operator *(Segment left, float right) => new(left.Start * right, left.End * right);
    public static Segment operator /(Segment left, float right) => new(left.Start / right, left.End / right);
    
    // public static Segment operator +(Segment left, float right)
    // {
    //     return right == 0 ? left : new(left.Start, left.End + left.Dir * right);
    // }
    // public static Segment operator -(Segment left, float right)
    // {
    //     return right == 0 ? left : new(left.Start, left.End - left.Dir * right);
    // }
    // public static Segment operator *(Segment left, float right)
    // {
    //     return right == 0 ? left : new(left.Start, left.Start + left.Displacement * right);
    // }
    // public static Segment operator /(Segment left, float right)
    // {
    //     return right == 0 ? left : new(left.Start, left.Start + left.Displacement / right);
    // }
    //
    #endregion
    
    #region Equality & HashCode
    /// <summary>
    /// Checks the equality of 2 segments without the direction.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool IsSimilar(Segment other)
    {
        return 
            (Start.IsSimilar(other.Start) && End.IsSimilar(other.End)) ||
            (Start.IsSimilar(other.End) && End.IsSimilar(other.Start));
        //return (Start == other.Start && End == other.End) || (Start == other.End && End == other.Start);
    }
    
    /// <summary>
    /// Checks the equality of 2 segments with the direction.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(Segment other)
    {
        return Start.IsSimilar(other.Start) && End.IsSimilar(other.End);// Start == other.Start && End == other.End;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(Start, End);
    }
    public static bool operator ==(Segment left, Segment right)
    {
        return left.Equals(right);
    }
    public static bool operator !=(Segment left, Segment right)
    {
        return !(left == right);
    }
    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (obj is Segment s) return Equals(s);
        return false;
    }
    #endregion

    #region Contains
    
    public readonly bool ContainsPoint(Vector2 p) { return IsPointOnSegment(p, Start, End); }
    
    #endregion
    
    #region Closest
    public static Vector2 GetClosestPointSegmentPoint(Vector2 segmentStart, Vector2 segmentEnd, Vector2 point)
    {
        // Calculate the direction vector of the segment
        var segmentDirection = segmentEnd - segmentStart;

        // Calculate the vector from the segment's start to the given point
        var toPoint = point - segmentStart;

        // Project the vector to the point onto the segment direction to find the closest approach
        float projectionLength = Vector2.Dot(toPoint, segmentDirection) / segmentDirection.LengthSquared();

        // Clamp the projection length to the range [0, 1] to ensure the closest point is on the segment
        projectionLength = Math.Clamp(projectionLength, 0.0f, 1.0f);

        // Calculate the closest point on the segment
        var closestPointOnSegment = segmentStart + projectionLength * segmentDirection;

        return closestPointOnSegment;
    }
    public static (Vector2 segment1Point, Vector2 segment2Point) GetClosestPointsSegmentSegment(Vector2 segment1Start, Vector2 segment1End, Vector2 segment2Start, Vector2 segment2End)
    {
        Vector2 d1 = segment1End - segment1Start;
        Vector2 d2 = segment2End - segment2Start;
        Vector2 r = segment1Start - segment2Start;

        float a = Vector2.Dot(d1, d1);
        float e = Vector2.Dot(d2, d2);
        float f = Vector2.Dot(d2, r);

        float s, t;
        if (a <= 1e-10 && e <= 1e-10)
        {
            // Both segments degenerate into points
            s = t = 0.0f;
            return (segment1Start, segment2Start);
        }
        if (a <= 1e-10)
        {
            // First segment degenerates into a point
            s = 0.0f;
            t = Math.Clamp(f / e, 0.0f, 1.0f);
        }
        else
        {
            float c = Vector2.Dot(d1, r);
            if (e <= 1e-10)
            {
                // Second segment degenerates into a point
                t = 0.0f;
                s = Math.Clamp(-c / a, 0.0f, 1.0f);
            }
            else
            {
                // The general nondegenerate case starts here
                float b = Vector2.Dot(d1, d2);
                float denom = a * e - b * b;

                if (denom != 0.0f)
                {
                    s = Math.Clamp((b * f - c * e) / denom, 0.0f, 1.0f);
                }
                else
                {
                    s = 0.0f;
                }

                t = (b * s + f) / e;

                if (t < 0.0f)
                {
                    t = 0.0f;
                    s = Math.Clamp(-c / a, 0.0f, 1.0f);
                }
                else if (t > 1.0f)
                {
                    t = 1.0f;
                    s = Math.Clamp((b - c) / a, 0.0f, 1.0f);
                }
            }
        }

        Vector2 closestPoint1 = segment1Start + s * d1;
        Vector2 closestPoint2 = segment2Start + t * d2;

        return (closestPoint1, closestPoint2);
    }
    public static (Vector2 segmentPoint, Vector2 linePoint) GetClosestPointsSegmentLine(Vector2 segmentStart, Vector2 segmentEnd, Vector2 linePoint, Vector2 lineDirection)
    {
        Vector2 d1 = segmentEnd - segmentStart;
        Vector2 d2 = Vector2.Normalize(lineDirection);
        Vector2 r = segmentStart - linePoint;

        float a = Vector2.Dot(d1, d1);
        float b = Vector2.Dot(d1, d2);
        float c = Vector2.Dot(d2, r);
        float e = Vector2.Dot(d2, d2);

        float s = Math.Clamp((b * c - a * c) / (a * e - b * b), 0.0f, 1.0f);
        float t = (b * s + c) / e;

        Vector2 closestPoint1 = segmentStart + s * d1;
        Vector2 closestPoint2 = linePoint + t * d2;

        return (closestPoint1, closestPoint2);
    }
    public static (Vector2 segmentPoint, Vector2 rayPoint) GetClosestPointsSegmentRay(Vector2 segmentStart, Vector2 segmentEnd, Vector2 rayPoint, Vector2 rayDirection)
    {
        Vector2 d1 = segmentEnd - segmentStart;
        Vector2 d2 = Vector2.Normalize(rayDirection);
        Vector2 r = segmentStart - rayPoint;

        float a = Vector2.Dot(d1, d1);
        float b = Vector2.Dot(d1, d2);
        float c = Vector2.Dot(d2, r);
        float e = Vector2.Dot(d2, d2);

        float s = Math.Clamp((b * c - a * c) / (a * e - b * b), 0.0f, 1.0f);
        float t = Math.Max(0.0f, (b * s + c) / e);

        Vector2 closestPoint1 = segmentStart + s * d1;
        Vector2 closestPoint2 = rayPoint + t * d2;

        return (closestPoint1, closestPoint2);
    }
    public static (Vector2 segmentPoint, Vector2 circlePoint) GetClosestPointsSegmentCircle(Vector2 segmentStart, Vector2 segmentEnd, Vector2 circleCenter, float radius)
    {
        Vector2 d1 = segmentEnd - segmentStart;
        Vector2 p1 = circleCenter;

        Vector2 toCenter = p1 - segmentStart;
        float projectionLength = Vector2.Dot(toCenter, d1) / d1.LengthSquared();
        projectionLength = Math.Clamp(projectionLength, 0.0f, 1.0f);
        Vector2 closestPointOnSegment = segmentStart + projectionLength * d1;

        Vector2 offset = Vector2.Normalize(closestPointOnSegment - p1) * radius;
        Vector2 closestPointOnCircle = p1 + offset;

        return (closestPointOnSegment, closestPointOnCircle);
    }
   
    //TODO: add remaining shapes
    //TODO: overhaul all closest distance functions

    public static Vector2 GetClosestPoint(Vector2 segmentStart, Vector2 segmentEnd, Vector2 p)
    {
        var w = (segmentEnd - segmentStart);
        float t = (p - segmentStart).Dot(w) / w.LengthSquared();
        if (t < 0f) return segmentStart;
        if (t > 1f) return segmentEnd;
        return segmentStart + w * t;
        
    }
    public ClosestDistance GetClosestDistanceTo(Vector2 p) => new(GetClosestPoint(Start, End, p), p);
    public ClosestDistance GetClosestDistanceTo(Segment segment)
    {
        var next = GetClosestPoint(Start, End, segment.Start);
        var disSq = (next - segment.Start).LengthSquared();
        var minDisSq = disSq;
        var cpSelf = next;
        var cpOther = segment.Start;
        
        
        next = GetClosestPoint(Start, End, segment.End);
        disSq = (next - segment.End).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpOther = segment.End;
            cpSelf = next;
        }
        
        next = GetClosestPoint(segment.Start, segment.End, Start);
        disSq = (next - Start).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpOther = next;
            cpSelf = Start;
        }
        
        next = GetClosestPoint(segment.Start, segment.End, End);
        disSq = (next - End).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpOther = next;
            cpSelf = End;
        }

        return new(cpSelf, cpOther);
        
        // var selfA = GetClosestDistanceTo(segment.Start);
        // var selfB = GetClosestDistanceTo(segment.End);
        // var otherA = segment.GetClosestDistanceTo(Start);
        // var otherB = segment.GetClosestDistanceTo(End);
        //
        // var min = selfA;
        // if (selfB.DistanceSquared < min.DistanceSquared) min = selfB;
        // if (otherA.DistanceSquared < min.DistanceSquared) min = otherA;
        // return otherB.DistanceSquared < min.DistanceSquared ? otherB : min;
    }
    public ClosestDistance GetClosestDistanceTo(Circle circle)
    {
        var segmentPoint = GetClosestPoint(Start, End, circle.Center);
        var dir = (segmentPoint - circle.Center).Normalize();
        var circlePoint = circle.Center + dir * circle.Radius;
        return new(segmentPoint, circlePoint);
    }
    public ClosestDistance GetClosestDistanceTo(Triangle triangle)
    {
        var next = GetClosestPoint(Start, End, triangle.A);
        var disSq = (next - triangle.A).LengthSquared();
        var minDisSq = disSq;
        var cpSelf = next;
        var cpOther = triangle.A;
        
        
        next = GetClosestPoint(Start, End, triangle.B);
        disSq = (next - triangle.B).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpOther = triangle.B;
            cpSelf = next;
        }
        
        next = GetClosestPoint(Start, End, triangle.C);
        disSq = (next - triangle.C).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpOther = triangle.C;
            cpSelf = next;
        }
        
        next = Triangle.GetClosestPoint(triangle.A, triangle.B, triangle.C, Start);
        disSq = (next - Start).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpOther = next;
            cpSelf = Start;
        }
        
        next = Triangle.GetClosestPoint(triangle.A, triangle.B, triangle.C, End);
        disSq = (next - Start).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpOther = next;
            cpSelf = End;
        }
        

        return new(cpSelf, cpOther);
        // var selfA = GetClosestDistanceTo(triangle.A);
        // var selfB = GetClosestDistanceTo(triangle.B);
        // var selfC = GetClosestDistanceTo(triangle.C);
        // var otherA = triangle.GetClosestDistanceTo(Start);
        // var otherB = triangle.GetClosestDistanceTo(End);
        //
        // var min = selfA;
        // if (selfB.DistanceSquared < min.DistanceSquared) min = selfB;
        // if (selfC.DistanceSquared < min.DistanceSquared) min = selfC;
        // if (otherA.DistanceSquared < min.DistanceSquared) min = otherA;
        // return otherB.DistanceSquared < min.DistanceSquared ? otherB : min;
    }
    public ClosestDistance GetClosestDistanceTo(Quad quad)
    {
        var next = GetClosestPoint(Start, End, quad.A);
        var disSq = (next - quad.A).LengthSquared();
        var minDisSq = disSq;
        var cpSelf = next;
        var cpOther = quad.A;
        
        
        next = GetClosestPoint(Start, End, quad.B);
        disSq = (next - quad.B).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpOther = quad.B;
            cpSelf = next;
        }
        
        next = GetClosestPoint(Start, End, quad.C);
        disSq = (next - quad.C).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpOther = quad.C;
            cpSelf = next;
        }
        
        next = GetClosestPoint(Start, End, quad.D);
        disSq = (next - quad.D).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpOther = quad.D;
            cpSelf = next;
        }
        
        next = Quad.GetClosestPoint(quad.A, quad.B, quad.C, quad.D, Start);
        disSq = (next - Start).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpOther = next;
            cpSelf = Start;
        }
        
        next = Quad.GetClosestPoint(quad.A, quad.B, quad.C, quad.D, End);
        disSq = (next - Start).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpOther = next;
            cpSelf = End;
        }
        

        return new(cpSelf, cpOther);
        
        // var selfA = GetClosestDistanceTo(quad.A);
        // var selfB = GetClosestDistanceTo(quad.B);
        // var selfC = GetClosestDistanceTo(quad.C);
        // var selfD = GetClosestDistanceTo(quad.D);
        // var otherA = quad.GetClosestDistanceTo(Start);
        // var otherB = quad.GetClosestDistanceTo(End);
        //
        // var min = selfA;
        // if (selfB.DistanceSquared < min.DistanceSquared) min = selfB;
        // if (selfC.DistanceSquared < min.DistanceSquared) min = selfC;
        // if (selfD.DistanceSquared < min.DistanceSquared) min = selfD;
        // if (otherA.DistanceSquared < min.DistanceSquared) min = otherA;
        // return otherB.DistanceSquared < min.DistanceSquared ? otherB : min;
    }
    public ClosestDistance GetClosestDistanceTo(Rect rect)
    {
        var next = GetClosestPoint(Start, End, rect.TopLeft);
        var disSq = (next - rect.TopLeft).LengthSquared();
        var minDisSq = disSq;
        var cpSelf = next;
        var cpOther = rect.TopLeft;
        
        
        next = GetClosestPoint(Start, End, rect.BottomLeft);
        disSq = (next - rect.BottomLeft).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpOther = rect.BottomLeft;
            cpSelf = next;
        }
        
        next = GetClosestPoint(Start, End, rect.BottomRight);
        disSq = (next - rect.BottomRight).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpOther = rect.BottomRight;
            cpSelf = next;
        }
        
        next = GetClosestPoint(Start, End, rect.TopRight);
        disSq = (next - rect.TopRight).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpOther = rect.TopRight;
            cpSelf = next;
        }
        
        next = Quad.GetClosestPoint(rect.TopLeft, rect.BottomLeft, rect.BottomRight, rect.TopRight, Start);
        disSq = (next - Start).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpOther = next;
            cpSelf = Start;
        }
        
        next = Quad.GetClosestPoint(rect.TopLeft, rect.BottomLeft, rect.BottomRight, rect.TopRight, End);
        disSq = (next - Start).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpOther = next;
            cpSelf = End;
        }
        

        return new(cpSelf, cpOther);
        
        // var selfA = GetClosestDistanceTo(rect.TopLeft);
        // var selfB = GetClosestDistanceTo(rect.BottomLeft);
        // var selfC = GetClosestDistanceTo(rect.BottomRight);
        // var selfD = GetClosestDistanceTo(rect.TopRight);
        // var otherA = rect.GetClosestDistanceTo(Start);
        // var otherB = rect.GetClosestDistanceTo(End);
        //
        // var min = selfA;
        // if (selfB.DistanceSquared < min.DistanceSquared) min = selfB;
        // if (selfC.DistanceSquared < min.DistanceSquared) min = selfC;
        // if (selfD.DistanceSquared < min.DistanceSquared) min = selfD;
        // if (otherA.DistanceSquared < min.DistanceSquared) min = otherA;
        // return otherB.DistanceSquared < min.DistanceSquared ? otherB : min;
    }
    public ClosestDistance GetClosestDistanceTo(Polygon polygon)
    {
        if (polygon.Count <= 0) return new();
        if (polygon.Count == 1) return GetClosestDistanceTo(polygon[0]);
        if (polygon.Count == 2) return GetClosestDistanceTo(new Segment(polygon[0], polygon[1]));
        if (polygon.Count == 3) return GetClosestDistanceTo(new Triangle(polygon[0], polygon[1], polygon[2]));
        if (polygon.Count == 4) return GetClosestDistanceTo(new Quad(polygon[0], polygon[1], polygon[2], polygon[3]));
        
        ClosestDistance closestDistance = new(new(), new(), float.PositiveInfinity);
        
        for (var i = 0; i < polygon.Count; i++)
        {
            var p1 = polygon[i];
            var p2 = polygon[(i + 1) % polygon.Count];
            
            var next = GetClosestPoint(Start, End, p1);
            var cd = new ClosestDistance(next, p1);
            if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
            
            next = GetClosestPoint(Start, End, p2);
            cd = new ClosestDistance(next, p2);
            if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
            
            next = GetClosestPoint(p1, p2, Start);
            cd = new ClosestDistance(Start, next);
            if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
            
            next = GetClosestPoint(p1, p2, End);
            cd = new ClosestDistance(End, next);
            if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
        }
        return closestDistance;
    }
    public ClosestDistance GetClosestDistanceTo(Polyline polyline)
    {
        if (polyline.Count <= 0) return new();
        if (polyline.Count == 1) return GetClosestDistanceTo(polyline[0]);
        if (polyline.Count == 2) return GetClosestDistanceTo(new Segment(polyline[0], polyline[1]));
        
        ClosestDistance closestDistance = new(new(), new(), float.PositiveInfinity);
        
        for (var i = 0; i < polyline.Count - 1; i++)
        {
            var p1 = polyline[i];
            var p2 = polyline[(i + 1) % polyline.Count];
            
            var next = GetClosestPoint(Start, End, p1);
            var cd = new ClosestDistance(next, p1);
            if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
            
            next = GetClosestPoint(Start, End, p2);
            cd = new ClosestDistance(next, p2);
            if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
            
            next = GetClosestPoint(p1, p2, Start);
            cd = new ClosestDistance(Start, next);
            if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
            
            next = GetClosestPoint(p1, p2, End);
            cd = new ClosestDistance(End, next);
            if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
        }
        return closestDistance;
    }
    
    
    
    public Vector2 GetClosestVertex(Vector2 p)
    {
        float disSqA = (p - Start).LengthSquared();
        float disSqB = (p - End).LengthSquared();
        return disSqA <= disSqB ? Start : End;
    }
    
    public CollisionPoint GetClosestCollisionPoint(Vector2 p)
    {
        CollisionPoint c;
        var w = Displacement;
        float t = (p - Start).Dot(w) / w.LengthSquared();
        if (t < 0f) c = new(Start, Normal); 
        else if (t > 1f) c = new(End, Normal);
        else c = new(Start + w * t, Normal);

        return c;
    }
    
    // internal ClosestPoint GetClosestPoint(Vector2 p)
    // {
    //     var cp = GetClosestCollisionPoint(p);
    //     return new(cp, (cp.Point - p).Length());
    // }

    #endregion
    
    #region Overlap
    public bool OverlapSegment(Vector2 segStart, Vector2 segEnd) => OverlapSegmentSegment(Start, End, segStart, segEnd);
    public bool OverlapLine(Vector2 linePoint, Vector2 lineDirection) => OverlapSegmentLine(Start, End, linePoint, lineDirection);
    public bool OverlapRay(Vector2 rayPoint, Vector2 rayDirection) => OverlapSegmentRay(Start, End, rayPoint, rayDirection);
    public bool OverlapCircle(Vector2 circlePoint, float circleRadius) => OverlapSegmentCircle(Start, End, circlePoint, circleRadius);
    
  
    
    public bool Overlap(Collider collider)
    {
        if (!collider.Enabled) return false;

        switch (collider.GetShapeType())
        {
            case ShapeType.Circle:
                var c = collider.GetCircleShape();
                return OverlapShape(c);
            case ShapeType.Segment:
                var s = collider.GetSegmentShape();
                return OverlapShape(s);
            case ShapeType.Triangle:
                var t = collider.GetTriangleShape();
                return OverlapShape(t);
            case ShapeType.Rect:
                var r = collider.GetRectShape();
                return OverlapShape(r);
            case ShapeType.Quad:
                var q = collider.GetQuadShape();
                return OverlapShape(q);
            case ShapeType.Poly:
                var p = collider.GetPolygonShape();
                return OverlapShape(p);
            case ShapeType.PolyLine:
                var pl = collider.GetPolylineShape();
                return OverlapShape(pl);
        }

        return false;
    }

    public bool OverlapShape(Segments segments)
    {
        foreach (var seg in segments)
        {
            if (seg.OverlapShape(this)) return true;
        }
        return false;
    }
    public bool OverlapShape(Segment b) => OverlapSegmentSegment(Start, End, b.Start, b.End);
    public bool OverlapShape(Line l) => OverlapSegmentLine(Start, End, l.Point, l.Direction);
    public bool OverlapShape(Ray r) => OverlapSegmentRay(Start, End, r.Point, r.Direction);
    public bool OverlapShape(Circle c) => OverlapSegmentCircle(Start, End, c.Center, c.Radius);
    public bool OverlapShape(Triangle t)
    {
        //we only need to check if 1 point is inside incase the entire segment is inside the shape
        if (t.ContainsPoint(Start)) return true;
        // if (t.ContainsPoint(End)) return true;

        if (OverlapSegmentSegment(Start, End, t.A, t.B)) return true;
        if (OverlapSegmentSegment(Start, End, t.B, t.C)) return true;
        return OverlapSegmentSegment(Start, End, t.C, t.A);
    }
    public bool OverlapShape(Quad q)
    {
        //we only need to check if 1 point is inside incase the entire segment is inside the shape
        if (q.ContainsPoint(Start)) return true;
        // if (q.ContainsPoint(End)) return true;

        if (OverlapSegmentSegment(Start, End, q.A, q.B)) return true;
        if (OverlapSegmentSegment(Start, End, q.B, q.C)) return true;
        if (OverlapSegmentSegment(Start, End, q.C, q.D)) return true;
        return OverlapSegmentSegment(Start, End, q.D, q.A);
    }
    public bool OverlapShape(Rect r)
    {
        if (!r.OverlapRectLine(Start, Displacement)) return false;
        var rectRange = new ValueRange
            (
                r.X,
                r.X + r.Width
            );
        var segmentRange = new ValueRange
            (
                Start.X,
                End.X
            );

        if (!rectRange.OverlapValueRange(segmentRange)) return false;

        rectRange = new(r.Y, r.Y + r.Height);
        // rectRange.Min = r.Y;
        // rectRange.Max = r.Y + r.Height;
        // rectRange.Sort();

        segmentRange = new(Start.Y, End.Y);
        // segmentRange.Min = Start.Y;
        // segmentRange.Max = End.Y;
        // segmentRange.Sort();

        return rectRange.OverlapValueRange(segmentRange);
    }
    public bool OverlapShape(Polygon poly)
    {
        if (poly.Count < 3) return false;
        //we only need to check if 1 point is inside incase the entire segment is inside the shape
        if (poly.ContainsPoint(Start)) return true;
        //if (poly.ContainsPoint(End)) return true;
        
        for (var i = 0; i < poly.Count; i++)
        {
            var a = poly[i];
            var b = poly[(i + 1) % poly.Count];
            if (OverlapSegmentSegment(Start, End, a, b)) return true;
        }
        return false;
    }

    public bool OverlapShape(Polyline pl)
    {
        if (pl.Count <= 1) return false;
        if (pl.Count == 2) return OverlapSegmentSegment(Start, End, pl[0], pl[1]);
        
        for (var i = 0; i < pl.Count - 1; i++)
        {
            var a = pl[i];
            var b = pl[i + 1];
            if (OverlapSegmentSegment(Start, End, a, b)) return true;
        }
        return false;
    }

    public bool OverlapSegmentLine(Vector2 linePos, Vector2 lineDir) =>
        OverlapSegmentLine(Start, End, linePos, lineDir);
    
    #endregion

    #region Intersection

    public CollisionPoint IntersectLine(Vector2 linePos, Vector2 lineDir) => IntersectSegmentLine(Start, End, linePos, lineDir);
    public CollisionPoint IntersectRay(Vector2 rayPos, Vector2 rayDir) => IntersectSegmentRay(Start, End, rayPos, rayDir);
    public CollisionPoint IntersectSegment(Vector2 segStart, Vector2 segEnd) => IntersectSegmentSegment(Start, End, segStart, segEnd);
    public (CollisionPoint a, CollisionPoint b) IntersectCircle(Vector2 circlePos, float circleRadius) => IntersectSegmentCircle(Start, End, circlePos, circleRadius);
    public (CollisionPoint a, CollisionPoint b) IntersectTriangle(Vector2 a, Vector2 b, Vector2 c) => IntersectSegmentTriangle(Start, End, a, b, c);
    public (CollisionPoint a, CollisionPoint b) IntersectQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => IntersectSegmentQuad(Start, End, a, b, c, d);
    public (CollisionPoint a, CollisionPoint b) IntersectRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => IntersectSegmentQuad(Start, End, a, b, c, d);
    public CollisionPoints? IntersectPolygon(List<Vector2> points, int maxCollisionPoints = -1) => IntersectSegmentPolygon(Start, End, points, maxCollisionPoints);
    public CollisionPoints? IntersectPolyline(List<Vector2> points, int maxCollisionPoints = -1) => IntersectSegmentPolyline(Start, End, points, maxCollisionPoints);
    public CollisionPoints? IntersectSegments(List<Segment> segments, int maxCollisionPoints = -1) => IntersectSegmentSegments(Start, End, segments, maxCollisionPoints);
    
    public CollisionPoint IntersectLine(Line line) => IntersectSegmentLine(Start, End, line.Point, line.Direction);
    public CollisionPoint IntersectRay(Ray ray) => IntersectSegmentRay(Start, End, ray.Point, ray.Direction);
    public CollisionPoint IntersectSegment(Segment segment) => IntersectSegmentSegment(Start, End, segment.Start, segment.End);
    public (CollisionPoint a, CollisionPoint b) IntersectCircle(Circle circle) => IntersectSegmentCircle(Start, End, circle.Center, circle.Radius);
    public (CollisionPoint a, CollisionPoint b) IntersectTriangle(Triangle triangle) => IntersectSegmentTriangle(Start, End, triangle.A, triangle.B, triangle.C);
    public (CollisionPoint a, CollisionPoint b) IntersectQuad(Quad quad) => IntersectSegmentQuad(Start, End, quad.A, quad.B, quad.C, quad.D);
    public (CollisionPoint a, CollisionPoint b) IntersectRect(Rect rect) => IntersectSegmentQuad(Start, End, rect.A, rect.B, rect.C, rect.D);
    public CollisionPoints? IntersectPolygon(Polygon polygon, int maxCollisionPoints = -1) => IntersectSegmentPolygon(Start, End, polygon, maxCollisionPoints);
    public CollisionPoints? IntersectPolyline(Polyline polyline, int maxCollisionPoints = -1) => IntersectSegmentPolyline(Start, End, polyline, maxCollisionPoints);
    public CollisionPoints? IntersectSegments(Segments segments, int maxCollisionPoints = -1) => IntersectSegmentSegments(Start, End, segments, maxCollisionPoints); 

    public CollisionPoints? Intersect(Collider collider)
    {
        if (!collider.Enabled) return null;

        switch (collider.GetShapeType())
        {
            case ShapeType.Circle:
                var c = collider.GetCircleShape();
                return IntersectShape(c);
            case ShapeType.Segment:
                var s = collider.GetSegmentShape();
                return IntersectShape(s);
            case ShapeType.Triangle:
                var t = collider.GetTriangleShape();
                return IntersectShape(t);
            case ShapeType.Rect:
                var r = collider.GetRectShape();
                return IntersectShape(r);
            case ShapeType.Quad:
                var q = collider.GetQuadShape();
                return IntersectShape(q);
            case ShapeType.Poly:
                var p = collider.GetPolygonShape();
                return IntersectShape(p);
            case ShapeType.PolyLine:
                var pl = collider.GetPolylineShape();
                return IntersectShape(pl);
        }

        return null;
    }
    public CollisionPoints? IntersectShape(Segment s)
    {
        var cp = IntersectSegmentSegment(Start, End, s.Start, s.End, s.Normal);
        if (cp.Valid) return [cp];

        return null;
    }
    public CollisionPoints? IntersectShape(Line l)
    {
        var cp = IntersectSegmentLine(Start, End, l.Point, l.Direction, l.Normal);
        if (cp.Valid) return [cp];

        return null;
    }
    public CollisionPoints? IntersectShape(Ray r)
    {
        var cp = IntersectSegmentRay(Start, End, r.Point, r.Direction, r.Normal);
        if (cp.Valid) return [cp];

        return null;
    }
    public CollisionPoints? IntersectShape(Circle c)
    {
        var result = IntersectSegmentCircle(Start, End, c.Center, c.Radius);

        if (result.a.Valid && result.b.Valid)
        {
            var points = new CollisionPoints
            {
                result.a,
                result.b
            };
            return points;
        }
        if (result.a.Valid)
        {
            var points = new CollisionPoints { result.a };
            return points;
        }
        
        if (result.b.Valid)
        {
            var points = new CollisionPoints { result.b };
            return points;
        }

        return null;
    }
    public CollisionPoints? IntersectShape(Triangle t)
    {
        CollisionPoints? points = null;
        var cp = IntersectSegmentSegment(Start, End, t.A, t.B);
        if (cp.Valid)
        {
            points ??= new();
            points.Add(cp);
        }
        
        cp = IntersectSegmentSegment(Start, End, t.B, t.C);
        if (cp.Valid)
        {
            points ??= new();
            points.Add(cp);
        }

        //intersecting a triangle with a segment can not result in more than 2 intersection points
        if (points is { Count: 2 }) return points;
        
        cp = IntersectSegmentSegment(Start, End, t.C, t.A);
        if (cp.Valid)
        {
            points ??= new();
            points.Add(cp);
        }

        return points;
    }
    public CollisionPoints? IntersectShape(Quad q)
    {
        CollisionPoints? points = null;
        var cp = IntersectSegmentSegment(Start, End, q.A, q.B);
        if (cp.Valid)
        {
            points ??= new();
            points.Add(cp);
        }
        
        cp = IntersectSegmentSegment(Start, End, q.B, q.C);
        if (cp.Valid)
        {
            points ??= new();
            points.Add(cp);
        }
        
        //intersecting a quad with a segment can not result in more than 2 intersection points
        if (points is { Count: 2 }) return points;
        
        cp = IntersectSegmentSegment(Start, End, q.C, q.D);
        if (cp.Valid)
        {
            points ??= new();
            points.Add(cp);
        }
        
        //intersecting a quad with a segment can not result in more than 2 intersection points
        if (points is { Count: 2 }) return points;
        
        cp = IntersectSegmentSegment(Start, End, q.D, q.A);
        if (cp.Valid)
        {
            points ??= new();
            points.Add(cp);
        }
        return points;
    }
    public CollisionPoints? IntersectShape(Rect r)
    {
        CollisionPoints? points = null;
        var a = r.TopLeft;
        var b = r.BottomLeft;
        
        var cp = IntersectSegmentSegment(Start, End, a, b);
        if (cp.Valid)
        {
            points ??= new();
            points.Add(cp);
        }
        
        var c = r.BottomRight;
        cp = IntersectSegmentSegment(Start, End, b, c);
        if (cp.Valid)
        {
            points ??= new();
            points.Add(cp);
        }
        
        //intersecting a rect with a segment can not result in more than 2 intersection points
        if (points is { Count: 2 }) return points;
        
        var d = r.TopRight;
        cp = IntersectSegmentSegment(Start, End, c, d);
        if (cp.Valid)
        {
            points ??= new();
            points.Add(cp);
        }
        
        //intersecting a rect with a segment can not result in more than 2 intersection points
        if (points is { Count: 2 }) return points;
        
        cp = IntersectSegmentSegment(Start, End, d, a);
        if (cp.Valid)
        {
            points ??= new();
            points.Add(cp);
        }
        return points;
    }
    public CollisionPoints? IntersectShape(Polygon p)
    {
        if (p.Count < 3) return null;
        CollisionPoints? points = null;
        for (var i = 0; i < p.Count; i++)
        {
            var colPoint = IntersectSegmentSegment(Start, End, p[i], p[(i + 1) % p.Count]);
            if (colPoint.Valid)
            {
                points ??= new();
                points.Add(colPoint);
            }
        }
        return points;
    }
    public CollisionPoints? IntersectShape(Polyline pl)
    {
        if (pl.Count < 2) return null;
        CollisionPoints? points = null;
        for (var i = 0; i < pl.Count - 1; i++)
        {
            var colPoint = IntersectSegmentSegment(Start, End, pl[i], pl[i + 1]);
            if (colPoint.Valid)
            {
                points ??= new();
                points.Add(colPoint);
            }
        }
        return points;
    }
    public CollisionPoints? IntersectShape(Segments shape)
    {
        if (shape.Count <= 0) return null;
        CollisionPoints? points = null;

        foreach (var seg in shape)
        {
            var result = IntersectSegmentSegment(Start, End, seg.Start, seg.End);
            if (result.Valid)
            {
                points ??= new();
                points.AddRange(result);
            }
        }
        return points;
    }

    
    public int Intersect(Collider collider, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (!collider.Enabled) return 0;

        switch (collider.GetShapeType())
        {
            case ShapeType.Circle:
                var c = collider.GetCircleShape();
                return IntersectShape(c, ref points, returnAfterFirstValid);
            case ShapeType.Segment:
                var s = collider.GetSegmentShape();
                return IntersectShape(s, ref points);
            case ShapeType.Triangle:
                var t = collider.GetTriangleShape();
                return IntersectShape(t, ref points, returnAfterFirstValid);
            case ShapeType.Rect:
                var r = collider.GetRectShape();
                return IntersectShape(r, ref points, returnAfterFirstValid);
            case ShapeType.Quad:
                var q = collider.GetQuadShape();
                return IntersectShape(q, ref points, returnAfterFirstValid);
            case ShapeType.Poly:
                var p = collider.GetPolygonShape();
                return IntersectShape(p, ref points, returnAfterFirstValid);
            case ShapeType.PolyLine:
                var pl = collider.GetPolylineShape();
                return IntersectShape(pl, ref points, returnAfterFirstValid);
        }

        return 0;
    }
    public int IntersectShape(Segment s, ref CollisionPoints points)
    {
        var cp = IntersectSegmentSegment(Start, End, s.Start, s.End);
        if (cp.Valid)
        {
            points.Add(cp);
            return 1;
        }

        return 0;
    }
    public int IntersectShape(Circle c, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var result = IntersectSegmentCircle(Start, End, c.Center, c.Radius);

        if (result.a.Valid && result.b.Valid)
        {
            if (returnAfterFirstValid)
            {
                points.Add(result.a);
                return 1;
            }
            points.Add(result.a);
            points.Add(result.b);
            return 2;
        }
        if (result.a.Valid)
        {
            points.Add(result.a);
            return 1;
        }
        
        if (result.b.Valid)
        {
           points.Add(result.b);
           return 1;
        }

        return 0;
    }
    public int IntersectShape(Triangle t, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var cp = IntersectSegmentSegment(Start, End, t.A, t.B);
        var count = 0;
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        cp = IntersectSegmentSegment(Start, End, t.B, t.C);
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        //intersecting a triangle with a segment can not result in more than 2 intersection points
        if (count >= 2) return count;
        
        cp = IntersectSegmentSegment(Start, End, t.C, t.A);
        if (cp.Valid)
        {
            points.Add(cp);
            count++;
        }

        return count;
    }
    public int IntersectShape(Quad q, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var cp = IntersectSegmentSegment(Start, End, q.A, q.B);
        var count = 0;
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        cp = IntersectSegmentSegment(Start, End, q.B, q.C);
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        //intersecting a quad with a segment can not result in more than 2 intersection points
        if (count >= 2) return count;
        
        cp = IntersectSegmentSegment(Start, End, q.C, q.D);
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        //intersecting a quad with a segment can not result in more than 2 intersection points
        if (count >= 2) return count;
        
        cp = IntersectSegmentSegment(Start, End, q.D, q.A);
        if (cp.Valid)
        {
            points.Add(cp);
            count++;
        }
        return count;
    }
    public int IntersectShape(Rect r, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var a = r.TopLeft;
        var b = r.BottomLeft;
        
        var cp = IntersectSegmentSegment(Start, End, a, b);
        var count = 0;
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        var c = r.BottomRight;
        cp = IntersectSegmentSegment(Start, End, b, c);
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        //intersecting a rect with a segment can not result in more than 2 intersection points
        if (count >= 2) return count;
        
        var d = r.TopRight;
        cp = IntersectSegmentSegment(Start, End, c, d);
        if (cp.Valid)
        {
            points.Add(cp);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        //intersecting a rect with a segment can not result in more than 2 intersection points
        if (count >= 2) return count;
        
        cp = IntersectSegmentSegment(Start, End, d, a);
        if (cp.Valid)
        {
            points.Add(cp);
            count++;
        }
        return count;
    }
    public int IntersectShape(Polygon p, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (p.Count < 3) return 0;
        var count = 0;
        for (var i = 0; i < p.Count; i++)
        {
            var cp = IntersectSegmentSegment(Start, End, p[i], p[(i + 1) % p.Count]);
            if (cp.Valid)
            {
                points.Add(cp);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }
        return count;
    }
    public int IntersectShape(Polyline pl, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (pl.Count < 2) return 0;
        var count = 0;
        for (var i = 0; i < pl.Count - 1; i++)
        {
            var cp = IntersectSegmentSegment(Start, End, pl[i], pl[i + 1]);
            if (cp.Valid)
            {
                points.Add(cp);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }
        return count;
    }
    public int IntersectShape(Segments shape, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (shape.Count <= 0) return 0;
        var count = 0;

        foreach (var seg in shape)
        {
            var cp = IntersectSegmentSegment(Start, End, seg.Start, seg.End);
            if (cp.Valid)
            {
                points.Add(cp);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }
        return count;
    }
   
    #endregion

    public void DrawNormal(float lineThickness, float length, ColorRgba colorRgba)
    {
        Segment n = new(Center, Center + Normal * length);
        n.Draw(lineThickness, colorRgba);
    }
}
