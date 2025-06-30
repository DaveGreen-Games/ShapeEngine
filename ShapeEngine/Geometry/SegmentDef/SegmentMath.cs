using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.SegmentDef;

public readonly partial struct Segment
{
    #region Math

    /// <summary>
    /// Returns a new segment with both endpoints floored (rounded down to the nearest integer values).
    /// </summary>
    /// <returns>A new <see cref="Segment"/> with floored endpoints.</returns>
    public Segment Floor()
    {
        return new(Start.Floor(), End.Floor());
    }

    /// <summary>
    /// Returns a new segment with both endpoints ceiled (rounded up to the nearest integer values).
    /// </summary>
    /// <returns>A new <see cref="Segment"/> with ceiled endpoints.</returns>
    public Segment Ceiling()
    {
        return new(Start.Ceiling(), End.Ceiling());
    }

    /// <summary>
    /// Returns a new segment with both endpoints rounded to the nearest integer values.
    /// </summary>
    /// <returns>A new <see cref="Segment"/> with rounded endpoints.</returns>
    public Segment Round()
    {
        return new(Start.Round(), End.Round());
    }

    /// <summary>
    /// Returns a new segment with both endpoints truncated (fractional part removed).
    /// </summary>
    /// <returns>A new <see cref="Segment"/> with truncated endpoints.</returns>
    public Segment Truncate()
    {
        return new(Start.Truncate(), End.Truncate());
    }

    /// <summary>
    /// Returns the four points that form the projected shape of the segment along a given vector.
    /// </summary>
    /// <param name="v">The vector along which to project the segment.</param>
    /// <returns>A <see cref="Points"/> collection of the projected shape, or null if the vector is zero.</returns>
    /// <remarks>
    /// The result is a quadrilateral formed by the segment and its projection.
    /// </remarks>
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

    /// <summary>
    /// Returns the convex hull polygon formed by projecting the segment along a given vector.
    /// </summary>
    /// <param name="v">The vector along which to project the segment.</param>
    /// <returns>A <see cref="Polygon"/> representing the convex hull, or null if the vector is zero.</returns>
    /// <remarks>
    /// The result is typically a quadrilateral, unless the vector is degenerate.
    /// </remarks>
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

    #region Transform

    /// <summary>
    /// Scales the length of a segment defined by two points, relative to an origin fraction.
    /// </summary>
    /// <param name="start">The start point of the segment.</param>
    /// <param name="end">The end point of the segment.</param>
    /// <param name="scale">The scale factor to apply to the segment's length.</param>
    /// <param name="originF">The origin fraction (0=start, 1=end, 0.5=center) about which to scale.</param>
    /// <returns>A tuple containing the new start and end points after scaling.</returns>
    public static (Vector2 newStart, Vector2 newEnd) ScaleLength(Vector2 start, Vector2 end, float scale, float originF = 0.5f)
    {
        var p = start.Lerp(end, originF);
        var s = start - p;
        var e = end - p;
        return new(p + s * scale, p + e * scale);
    }

    /// <summary>
    /// Returns a new segment with its length scaled by a given factor, relative to an origin fraction.
    /// </summary>
    /// <param name="scale">The scale factor to apply to the segment's length.</param>
    /// <param name="originF">The origin fraction (0=start, 1=end, 0.5=center) about which to scale.</param>
    /// <returns>A new <see cref="Segment"/> with the scaled length.</returns>
    public Segment ScaleLength(float scale, float originF = 0.5f)
    {
        var p = GetPoint(originF);
        var s = Start - p;
        var e = End - p;
        return new Segment(p + s * scale, p + e * scale);
    }

    /// <summary>
    /// Returns a new segment with its length scaled by a given size, relative to an origin fraction.
    /// </summary>
    /// <param name="scale">The <see cref="Size"/> to scale the segment's length.</param>
    /// <param name="originF">The origin fraction (0=start, 1=end, 0.5=center) about which to scale.</param>
    /// <returns>A new <see cref="Segment"/> with the scaled length.</returns>
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

    /// <summary>
    /// Returns a new segment with its length changed by a specified amount, extending or shortening from the start point.
    /// </summary>
    /// <param name="amount">The amount to change the segment's length by.</param>
    /// <returns>A new <see cref="Segment"/> with the changed length.</returns>
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

    /// <summary>
    /// Returns a new segment with its length changed by a specified amount, extending or shortening from the end point.
    /// </summary>
    /// <param name="amount">The amount to change the segment's length by.</param>
    /// <returns>A new <see cref="Segment"/> with the changed length.</returns>
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
    /// Changes the length of the segment based on an origin point. OriginF 0 = Start, 0.5 = Center, 1 = End.
    /// Splits the amount based on originF.
    /// </summary>
    /// <param name="amount">The amount to change the segment's length by.</param>
    /// <param name="originF">The origin fraction (0=start, 1=end, 0.5=center) about which to change length.</param>
    /// <returns>A new <see cref="Segment"/> with the changed length.</returns>
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

    /// <summary>
    /// Returns a new segment with its length set to a specified value, extending from the start point.
    /// </summary>
    /// <param name="length">The new length of the segment.</param>
    /// <returns>A new <see cref="Segment"/> with the set length.</returns>
    public Segment SetLengthFromStart(float length)
    {
        var newEnd = SetLength(Start, End, length);
        return new(Start, newEnd);
    }

    /// <summary>
    /// Returns a new segment with its length set to a specified value, extending from the end point.
    /// </summary>
    /// <param name="length">The new length of the segment.</param>
    /// <returns>A new <see cref="Segment"/> with the set length.</returns>
    public Segment SetLengthFromEnd(float length)
    {
        var newStart = SetLength(End, Start, length);
        return new(newStart, End);
    }

    /// <summary>
    /// Sets the length of the segment based on an origin point. OriginF 0 = Start, 0.5 = Center, 1 = End.
    /// Splits the length based on originF.
    /// </summary>
    /// <param name="length">The new length of the segment.</param>
    /// <param name="originF">The origin fraction (0=start, 1=end, 0.5=center) about which to set length.</param>
    /// <returns>A new <see cref="Segment"/> with the set length.</returns>
    public Segment SetLength(float length, float originF = 0.5f)
    {
        if (originF <= 0f) return SetLengthFromStart(length);
        if (originF >= 1f) return SetLengthFromEnd(length);

        var p = GetPoint(originF);
        var newStart = SetLength(p, Start, length * (1f - originF));
        var newEnd = SetLength(p, End, length * originF);
        return new(newStart, newEnd);
    }

    /// <summary>
    /// Returns a new segment with its start point set to a specified position.
    /// </summary>
    /// <param name="position">The new start position.</param>
    /// <returns>A new <see cref="Segment"/> with the updated start point.</returns>
    public Segment SetStart(Vector2 position)
    {
        return new(position, End);
    }

    /// <summary>
    /// Returns a new segment with its start point offset by a specified vector.
    /// </summary>
    /// <param name="offset">The offset to apply to the start point.</param>
    /// <returns>A new <see cref="Segment"/> with the updated start point.</returns>
    public Segment ChangeStart(Vector2 offset)
    {
        return new(Start + offset, End);
    }

    /// <summary>
    /// Returns a new segment with its end point set to a specified position.
    /// </summary>
    /// <param name="position">The new end position.</param>
    /// <returns>A new <see cref="Segment"/> with the updated end point.</returns>
    public Segment SetEnd(Vector2 position)
    {
        return new(Start, position);
    }

    /// <summary>
    /// Returns a new segment with its end point offset by a specified vector.
    /// </summary>
    /// <param name="offset">The offset to apply to the end point.</param>
    /// <returns>A new <see cref="Segment"/> with the updated end point.</returns>
    public Segment ChangeEnd(Vector2 offset)
    {
        return new(Start, End + offset);
    }

    /// <summary>
    /// Returns a new segment with both endpoints offset by a specified vector.
    /// </summary>
    /// <param name="offset">The offset to apply to both endpoints.</param>
    /// <returns>A new <see cref="Segment"/> with the updated position.</returns>
    public Segment ChangePosition(Vector2 offset)
    {
        return new(Start + offset, End + offset);
    }

    /// <summary>
    /// Returns a new segment with both endpoints offset by a specified (x, y) value.
    /// </summary>
    /// <param name="x">The x offset.</param>
    /// <param name="y">The y offset.</param>
    /// <returns>A new <see cref="Segment"/> with the updated position.</returns>
    public Segment ChangePosition(float x, float y)
    {
        return ChangePosition(new Vector2(x, y));
    }

    /// <summary>
    /// Returns a new segment with endpoints offset by a vector, interpolated by a fraction.
    /// </summary>
    /// <param name="offset">The offset vector.</param>
    /// <param name="f">The interpolation fraction (0=start, 1=end).</param>
    /// <returns>A new <see cref="Segment"/> with the updated position.</returns>
    public Segment ChangePosition(Vector2 offset, float f)
    {
        return new(Start + (offset * (1f - f)), End + (offset * f));
    }

    /// <summary>
    /// Returns a new segment with its position set so that the point at originF aligns with a specified position.
    /// </summary>
    /// <param name="position">The new position for the origin point.</param>
    /// <param name="originF">The origin fraction (0=start, 1=end, 0.5=center).</param>
    /// <returns>A new <see cref="Segment"/> with the updated position.</returns>
    public Segment SetPosition(Vector2 position, float originF = 0.5f)
    {
        var point = GetPoint(originF);
        var offset = position - point;
        return ChangePosition(offset);
    }

    /// <summary>
    /// Returns a new segment rotated by a specified angle (in radians) about an origin fraction.
    /// </summary>
    /// <param name="angleRad">The angle in radians to rotate.</param>
    /// <param name="originF">The origin fraction (0=start, 1=end, 0.5=center) about which to rotate.</param>
    /// <returns>A new <see cref="Segment"/> with the rotated endpoints.</returns>
    public Segment ChangeRotation(float angleRad, float originF = 0.5f)
    {
        var p = GetPoint(originF);
        var s = Start - p;
        var e = End - p;
        return new Segment(p + s.Rotate(angleRad), p + e.Rotate(angleRad));
    }

    /// <summary>
    /// Returns a new segment with its rotation set to a specified angle (in radians) about an origin fraction.
    /// </summary>
    /// <param name="angleRad">The target angle in radians.</param>
    /// <param name="originF">The origin fraction (0=start, 1=end, 0.5=center) about which to set rotation.</param>
    /// <returns>A new <see cref="Segment"/> with the set rotation.</returns>
    public Segment SetRotation(float angleRad, float originF = 0.5f)
    {
        if (originF <= 0f) return RotateStartTo(angleRad);
        if (originF >= 1f) return RotateEndTo(angleRad);

        var origin = GetPoint(originF);
        var fromAngleRad = (origin - Start).AngleRad();
        var amountRad = ShapeMath.GetShortestAngleRad(fromAngleRad, angleRad);
        return ChangeRotation(amountRad, originF);
    }

    /// <summary>
    /// Returns a new segment with its start rotated to a specified angle (in radians), keeping the end fixed.
    /// </summary>
    /// <param name="toAngleRad">The target angle in radians for the start point.</param>
    /// <returns>A new <see cref="Segment"/> with the rotated start point.</returns>
    public Segment RotateStartTo(float toAngleRad)
    {
        var fromAngleRad = (Start - End).AngleRad();
        var amountRad = ShapeMath.GetShortestAngleRad(fromAngleRad, toAngleRad);
        return ChangeRotation(amountRad, 1f);
    }

    /// <summary>
    /// Returns a new segment with its end rotated to a specified angle (in radians), keeping the start fixed.
    /// </summary>
    /// <param name="toAngleRad">The target angle in radians for the end point.</param>
    /// <returns>A new <see cref="Segment"/> with the rotated end point.</returns>
    public Segment RotateEndTo(float toAngleRad)
    {
        var fromAngleRad = (End - Start).AngleRad();
        var amountRad = ShapeMath.GetShortestAngleRad(fromAngleRad, toAngleRad);
        return ChangeRotation(amountRad, 0f);
    }

    /// <summary>
    /// Moves, rotates, and scales the segment according to a given <see cref="Transform2D"/> offset, relative to an origin fraction.
    /// </summary>
    /// <param name="offset">The <see cref="Transform2D"/> offset to apply.</param>
    /// <param name="originF">The origin fraction (0=start, 1=end, 0.5=center) about which to apply the transform.</param>
    /// <returns>A new <see cref="Segment"/> with the applied transform.</returns>
    /// <remarks>
    /// Moves the segment by offset.Position, rotates by offset.RotationRad, and changes length by offset.ScaledSize.Length.
    /// </remarks>
    public Segment ApplyOffset(Transform2D offset, float originF = 0.5f)
    {
        var newSegment = ChangePosition(offset.Position, originF);
        newSegment = newSegment.ChangeRotation(offset.RotationRad, originF);
        return newSegment.ChangeLength(offset.ScaledSize.Length, originF);
    }

    /// <summary>
    /// Sets the segment's position, rotation, and length according to a given <see cref="Transform2D"/>, relative to an origin fraction.
    /// </summary>
    /// <param name="transform">The <see cref="Transform2D"/> to apply.</param>
    /// <param name="originF">The origin fraction (0=start, 1=end, 0.5=center) about which to apply the transform.</param>
    /// <returns>A new <see cref="Segment"/> with the set transform.</returns>
    /// <remarks>
    /// Moves the segment to transform.Position, rotates to transform.RotationRad, and sets length to transform.ScaledSize.Length.
    /// </remarks>
    public Segment SetTransform(Transform2D transform, float originF = 0.5f)
    {
        var newSegment = SetPosition(transform.Position, originF);
        newSegment = newSegment.SetRotation(transform.RotationRad, originF);
        return newSegment.SetLength(transform.ScaledSize.Length, originF);
    }

    #endregion
}