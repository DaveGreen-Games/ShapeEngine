using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.Segment;

public readonly partial struct Segment
{
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

    public Polygon.Polygon? ProjectShape(Vector2 v)
    {
        if (v.LengthSquared() <= 0f) return null;
        var points = new Points
        {
            Start,
            End,
            Start + v,
            End + v,
        };
        return Polygon.Polygon.FindConvexHull(points);
    }

    #endregion

    #region Transform

    public static (Vector2 newStart, Vector2 newEnd) ScaleLength(Vector2 start, Vector2 end, float scale, float originF = 0.5f)
    {
        var p = start.Lerp(end, originF);
        var s = start - p;
        var e = end - p;
        return new(p + s * scale, p + e * scale);
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

    public Segment SetStart(Vector2 position)
    {
        return new(position, End);
    }

    public Segment ChangeStart(Vector2 offset)
    {
        return new(Start + offset, End);
    }

    public Segment SetEnd(Vector2 position)
    {
        return new(Start, position);
    }

    public Segment ChangeEnd(Vector2 offset)
    {
        return new(Start, End + offset);
    }

    public Segment ChangePosition(Vector2 offset)
    {
        return new(Start + offset, End + offset);
    }

    public Segment ChangePosition(float x, float y)
    {
        return ChangePosition(new Vector2(x, y));
    }

    public Segment ChangePosition(Vector2 offset, float f)
    {
        return new(Start + (offset * (1f - f)), End + (offset * f));
    }

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
}