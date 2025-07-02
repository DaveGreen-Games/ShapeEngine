using System.Numerics;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.StripedDrawingDef;

public static partial class StripedDrawing
{
    /// <summary>
    /// Draw a striped ring. Draws lines across the circumference of the ring.
    /// Each line starts on the inner radius and ends on the outer radius.
    /// </summary>
    /// <param name="center">The center of the ring.</param>
    /// <param name="innerRadius">The inner radius of the ring.
    /// Should be positive and smaller than the outer radius.</param>
    /// <param name="outerRadius">The outer radius of the ring.
    /// Should be positive and bigger than the inner radius.</param>
    /// <param name="angleSpacingDeg">The spacing between each line in degrees.</param>
    /// <param name="striped">The line drawing info for how to draw the lines.</param>
    /// <param name="angleOffset">The start offset.
    /// Value is wrapped between 0 and 1 and multiplied with the angleSpacingDeg.
    /// So the min offset is zero and the max offset is angleSpacingDeg!</param>
    public static void DrawStripedRing(Vector2 center, float innerRadius, float outerRadius, float angleSpacingDeg, LineDrawingInfo striped,
        float angleOffset = 0f)
    {
        if (angleSpacingDeg <= 0) return;
        if (outerRadius < 0f) outerRadius = 0f;
        if (innerRadius < 0f) innerRadius = 0f;
        if (Math.Abs(outerRadius - innerRadius) < 0.00000001f) return;
        if (outerRadius < innerRadius)
        {
            (innerRadius, outerRadius) = (outerRadius, innerRadius);
        }

        var angleSpacingRad = angleSpacingDeg * ShapeMath.DEGTORAD;
        float curAngleRad = angleSpacingRad * ShapeMath.WrapF(angleOffset, 0f, 1f);
        // float curAngleRad = angleOffset * 2f * ShapeMath.PI;
        while (curAngleRad <= 2f * ShapeMath.PI - angleSpacingRad)
        {
            var dir = new Vector2(1, 0).Rotate(curAngleRad);
            var p1 = center + dir * innerRadius;
            var p2 = center + dir * outerRadius;
            SegmentDrawing.DrawSegment(p1, p2, striped);
            curAngleRad += angleSpacingRad;
        }
    }

    /// <summary>
    /// Draw an alternating striped ring. Draws lines across the circumference of the ring.
    /// Each line starts on the inner radius and ends on the outer radius.
    /// </summary>
    /// <param name="center">The center of the ring.</param>
    /// <param name="innerRadius">The inner radius of the ring.
    /// Should be positive and smaller than the outer radius.</param>
    /// <param name="outerRadius">The outer radius of the ring.
    /// Should be positive and bigger than the inner radius.</param>
    /// <param name="angleSpacingDeg">The spacing between each line in degrees.</param>
    /// <param name="striped">The first line drawing info for drawing even lines.</param>
    /// <param name="alternatingStriped">The second line drawing info for drawing odd lines.</param>
    /// <param name="angleOffset">The start offset.
    /// Value is wrapped between 0 and 1 and multiplied with the angleSpacingDeg.
    /// So the min offset is zero and the max offset is angleSpacingDeg!</param>
    public static void DrawStripedRing(Vector2 center, float innerRadius, float outerRadius, float angleSpacingDeg, LineDrawingInfo striped,
        LineDrawingInfo alternatingStriped, float angleOffset = 0f)
    {
        if (angleSpacingDeg <= 0) return;
        if (outerRadius < 0f) outerRadius = 0f;
        if (innerRadius < 0f) innerRadius = 0f;
        if (Math.Abs(outerRadius - innerRadius) < 0.00000001f) return;
        if (outerRadius < innerRadius)
        {
            (innerRadius, outerRadius) = (outerRadius, innerRadius);
        }

        var angleSpacingRad = angleSpacingDeg * ShapeMath.DEGTORAD;
        float curAngleRad = angleSpacingRad * ShapeMath.WrapF(angleOffset, 0f, 1f);
        int i = 0;
        while (curAngleRad <= 2f * ShapeMath.PI - angleSpacingRad)
        {
            var info = i % 2 == 0 ? striped : alternatingStriped;
            i++;
            var dir = new Vector2(1, 0).Rotate(curAngleRad);
            var p1 = center + dir * innerRadius;
            var p2 = center + dir * outerRadius;
            SegmentDrawing.DrawSegment(p1, p2, info);
            curAngleRad += angleSpacingRad;
        }
    }

    /// <summary>
    /// Draw an alternating striped ring. Draws lines across the circumference of the ring.
    /// Each line starts on the inner radius and ends on the outer radius.
    /// </summary>
    /// <param name="center">The center of the ring.</param>
    /// <param name="innerRadius">The inner radius of the ring.
    /// Should be positive and smaller than the outer radius.</param>
    /// <param name="outerRadius">The outer radius of the ring.
    /// Should be positive and bigger than the inner radius.</param>
    /// <param name="angleSpacingDeg">The spacing between each line in degrees.</param>
    /// <param name="angleOffset">The start offset.
    /// Value is wrapped between 0 and 1 and multiplied with the angleSpacingDeg.
    /// So the min offset is zero and the max offset is angleSpacingDeg!</param>
    /// <param name="alternatingStriped">The line drawing infos for drawing each line.
    /// Each info is used in sequence and wraps around if there are more lines.</param>
    public static void DrawStripedRing(Vector2 center, float innerRadius, float outerRadius, float angleSpacingDeg, float angleOffset,
        params LineDrawingInfo[] alternatingStriped)
    {
        if (alternatingStriped.Length <= 0) return;
        if (angleSpacingDeg <= 0) return;
        if (outerRadius < 0f) outerRadius = 0f;
        if (innerRadius < 0f) innerRadius = 0f;
        if (Math.Abs(outerRadius - innerRadius) < 0.00000001f) return;
        if (outerRadius < innerRadius)
        {
            (innerRadius, outerRadius) = (outerRadius, innerRadius);
        }

        var angleSpacingRad = angleSpacingDeg * ShapeMath.DEGTORAD;
        float curAngleRad = angleSpacingRad * ShapeMath.WrapF(angleOffset, 0f, 1f);
        int i = 0;
        while (curAngleRad <= 2f * ShapeMath.PI - angleSpacingRad)
        {
            var index = i % alternatingStriped.Length;
            var info = alternatingStriped[index];
            i++;
            var dir = new Vector2(1, 0).Rotate(curAngleRad);
            var p1 = center + dir * innerRadius;
            var p2 = center + dir * outerRadius;
            SegmentDrawing.DrawSegment(p1, p2, info);
            curAngleRad += angleSpacingRad;
        }
    }

    /// <summary>
    /// Draw a sector of a striped ring. Draws lines across the circumference of the ring.
    /// Each line starts on the inner radius and ends on the outer radius.
    /// </summary>
    /// <param name="center">The center of the ring.</param>
    /// <param name="innerRadius">The inner radius of the ring.
    /// Should be positive and smaller than the outer radius.</param>
    /// <param name="outerRadius">The outer radius of the ring.
    /// Should be positive and bigger than the inner radius.</param>
    /// <param name="angleSpacingDeg">The spacing between each line in degrees.</param>
    /// <param name="minAngleDeg">The start of the sector in degrees.
    /// Can be negative and/or bigger than maxAngleDeg.</param>
    /// <param name="maxAngleDeg">The end of the sector in degrees.
    /// Can be negative and/or smaller than the minAngleDeg.</param>
    /// <param name="striped">The line drawing info for how to draw the lines.</param>
    /// <param name="angleOffset">The start offset.
    /// Value is wrapped between 0 and 1 and multiplied with the angleSpacingDeg.
    /// So the min offset is zero and the max offset is angleSpacingDeg!</param>
    public static void DrawStripedRing(Vector2 center, float innerRadius, float outerRadius, float angleSpacingDeg, float minAngleDeg, float maxAngleDeg,
        LineDrawingInfo striped, float angleOffset = 0f)
    {
        if (angleSpacingDeg <= 0) return;
        if (outerRadius < 0f) outerRadius = 0f;
        if (innerRadius < 0f) innerRadius = 0f;
        if (Math.Abs(outerRadius - innerRadius) < 0.00000001f) return;
        if (outerRadius < innerRadius)
        {
            (innerRadius, outerRadius) = (outerRadius, innerRadius);
        }

        if (Math.Abs(minAngleDeg - maxAngleDeg) < 0.00000001f) return;
        var dif = maxAngleDeg - minAngleDeg;
        int sign = MathF.Sign(dif);
        if (sign == 0) return;

        var angleSpacingRad = angleSpacingDeg * ShapeMath.DEGTORAD;
        var minAngleRad = minAngleDeg * ShapeMath.DEGTORAD;
        var maxAngleRad = maxAngleDeg * ShapeMath.DEGTORAD;
        float curAngleRad = minAngleRad + angleSpacingRad * ShapeMath.WrapF(angleOffset, 0f, 1f);

        if (sign < 0)
        {
            while (curAngleRad >= maxAngleRad - angleSpacingRad)
            {
                var dir = new Vector2(1, 0).Rotate(curAngleRad);
                var p1 = center + dir * innerRadius;
                var p2 = center + dir * outerRadius;
                SegmentDrawing.DrawSegment(p1, p2, striped);
                curAngleRad -= angleSpacingRad;
            }
        }
        else
        {
            while (curAngleRad <= maxAngleRad + angleSpacingRad)
            {
                var dir = new Vector2(1, 0).Rotate(curAngleRad);
                var p1 = center + dir * innerRadius;
                var p2 = center + dir * outerRadius;
                SegmentDrawing.DrawSegment(p1, p2, striped);
                curAngleRad += angleSpacingRad;
            }
        }
    }

    /// <summary>
    /// Draw a sector of an alternating striped ring. Draws lines across the circumference of the ring.
    /// Each line starts on the inner radius and ends on the outer radius.
    /// </summary>
    /// <param name="center">The center of the ring.</param>
    /// <param name="innerRadius">The inner radius of the ring.
    /// Should be positive and smaller than the outer radius.</param>
    /// <param name="outerRadius">The outer radius of the ring.
    /// Should be positive and bigger than the inner radius.</param>
    /// <param name="angleSpacingDeg">The spacing between each line in degrees.</param>
    /// <param name="minAngleDeg">The start of the sector in degrees.
    /// Can be negative and/or bigger than maxAngleDeg.</param>
    /// <param name="maxAngleDeg">The end of the sector in degrees.
    /// Can be negative and/or smaller than the minAngleDeg.</param>
    /// <param name="striped">The first line drawing info for drawing even lines.</param>
    /// <param name="alternatingStriped">The second line drawing info for drawing odd lines.</param>
    /// <param name="angleOffset">The start offset.
    /// Value is wrapped between 0 and 1 and multiplied with the angleSpacingDeg.
    /// So the min offset is zero and the max offset is angleSpacingDeg!</param>
    public static void DrawStripedRing(Vector2 center, float innerRadius, float outerRadius, float angleSpacingDeg, float minAngleDeg, float maxAngleDeg,
        LineDrawingInfo striped, LineDrawingInfo alternatingStriped, float angleOffset = 0f)
    {
        if (angleSpacingDeg <= 0) return;
        if (outerRadius < 0f) outerRadius = 0f;
        if (innerRadius < 0f) innerRadius = 0f;
        if (Math.Abs(outerRadius - innerRadius) < 0.00000001f) return;
        if (outerRadius < innerRadius)
        {
            (innerRadius, outerRadius) = (outerRadius, innerRadius);
        }

        if (Math.Abs(minAngleDeg - maxAngleDeg) < 0.00000001f) return;
        var dif = maxAngleDeg - minAngleDeg;
        int sign = MathF.Sign(dif);
        if (sign == 0) return;

        var angleSpacingRad = angleSpacingDeg * ShapeMath.DEGTORAD;
        var minAngleRad = minAngleDeg * ShapeMath.DEGTORAD;
        var maxAngleRad = maxAngleDeg * ShapeMath.DEGTORAD;
        float curAngleRad = minAngleRad + angleSpacingRad * ShapeMath.WrapF(angleOffset, 0f, 1f);
        int i = 0;
        if (sign < 0)
        {
            while (curAngleRad >= maxAngleRad - angleSpacingRad)
            {
                var info = i % 2 == 0 ? striped : alternatingStriped;
                i++;
                var dir = new Vector2(1, 0).Rotate(curAngleRad);
                var p1 = center + dir * innerRadius;
                var p2 = center + dir * outerRadius;
                SegmentDrawing.DrawSegment(p1, p2, info);
                curAngleRad -= angleSpacingRad;
            }
        }
        else
        {
            while (curAngleRad <= maxAngleRad + angleSpacingRad)
            {
                var info = i % 2 == 0 ? striped : alternatingStriped;
                i++;
                var dir = new Vector2(1, 0).Rotate(curAngleRad);
                var p1 = center + dir * innerRadius;
                var p2 = center + dir * outerRadius;
                SegmentDrawing.DrawSegment(p1, p2, info);
                curAngleRad += angleSpacingRad;
            }
        }
    }

    /// <summary>
    /// Draw a sector of an alternating striped ring. Draws lines across the circumference of the ring.
    /// Each line starts on the inner radius and ends on the outer radius.
    /// </summary>
    /// <param name="center">The center of the ring.</param>
    /// <param name="innerRadius">The inner radius of the ring.
    /// Should be positive and smaller than the outer radius.</param>
    /// <param name="outerRadius">The outer radius of the ring.
    /// Should be positive and bigger than the inner radius.</param>
    /// <param name="angleSpacingDeg">The spacing between each line in degrees.</param>
    /// <param name="minAngleDeg">The start of the sector in degrees.
    /// Can be negative and/or bigger than maxAngleDeg.</param>
    /// <param name="maxAngleDeg">The end of the sector in degrees.
    /// Can be negative and/or smaller than the minAngleDeg.</param>
    /// <param name="angleOffset">The start offset.
    /// Value is wrapped between 0 and 1 and multiplied with the angleSpacingDeg.
    /// So the min offset is zero and the max offset is angleSpacingDeg!</param>
    /// <param name="alternatingStriped">The line drawing infos for how to draw each of the lines.
    /// Each info is used in sequence and wraps around if there are more lines.</param>
    public static void DrawStripedRing(Vector2 center, float innerRadius, float outerRadius, float angleSpacingDeg, float minAngleDeg, float maxAngleDeg,
        float angleOffset, params LineDrawingInfo[] alternatingStriped)
    {
        if (alternatingStriped.Length <= 0) return;
        if (angleSpacingDeg <= 0) return;
        if (outerRadius < 0f) outerRadius = 0f;
        if (innerRadius < 0f) innerRadius = 0f;
        if (Math.Abs(outerRadius - innerRadius) < 0.00000001f) return;
        if (outerRadius < innerRadius)
        {
            (innerRadius, outerRadius) = (outerRadius, innerRadius);
        }

        if (Math.Abs(minAngleDeg - maxAngleDeg) < 0.00000001f) return;
        var dif = maxAngleDeg - minAngleDeg;
        int sign = MathF.Sign(dif);
        if (sign == 0) return;

        var angleSpacingRad = angleSpacingDeg * ShapeMath.DEGTORAD;
        var minAngleRad = minAngleDeg * ShapeMath.DEGTORAD;
        var maxAngleRad = maxAngleDeg * ShapeMath.DEGTORAD;
        float curAngleRad = minAngleRad + angleSpacingRad * ShapeMath.WrapF(angleOffset, 0f, 1f);
        int i = 0;
        if (sign < 0)
        {
            while (curAngleRad >= maxAngleRad - angleSpacingRad)
            {
                var index = i % alternatingStriped.Length;
                var info = alternatingStriped[index];
                i++;
                var dir = new Vector2(1, 0).Rotate(curAngleRad);
                var p1 = center + dir * innerRadius;
                var p2 = center + dir * outerRadius;
                SegmentDrawing.DrawSegment(p1, p2, info);
                curAngleRad -= angleSpacingRad;
            }
        }
        else
        {
            while (curAngleRad <= maxAngleRad + angleSpacingRad)
            {
                var index = i % alternatingStriped.Length;
                var info = alternatingStriped[index];
                i++;
                var dir = new Vector2(1, 0).Rotate(curAngleRad);
                var p1 = center + dir * innerRadius;
                var p2 = center + dir * outerRadius;
                SegmentDrawing.DrawSegment(p1, p2, info);
                curAngleRad += angleSpacingRad;
            }
        }
    }

}

