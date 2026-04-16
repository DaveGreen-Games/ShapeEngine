using System.Numerics;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.CircleDef;

public readonly partial struct Circle
{
    #region Generate Striped Segments
    
    /// <summary>
    /// Appends radial stripe segments for a full ring (annulus) around this circle.
    /// </summary>
    /// <param name="result">The <see cref="Segments"/> collection that receives the generated stripe segments. Existing contents are preserved.</param>
    /// <param name="ringThickness">The radial thickness of the ring centered on this circle's radius.</param>
    /// <param name="angleSpacingDeg">The angular spacing in degrees between consecutive stripe segments.</param>
    /// <param name="angleOffset">A normalized offset in the range [0,1] that shifts the starting angle by a fraction of <paramref name="angleSpacingDeg"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="result"/> contains at least one segment after the call; otherwise, <see langword="false"/>.</returns>
    public bool GenerateStripedRing(Segments result, float ringThickness, float angleSpacingDeg, float angleOffset = 0f)
    {
        if (angleSpacingDeg <= 0) return false;
        
        if(ringThickness <= 0 || Radius <= 0 || Radius - ringThickness <= 0) return false;

        var center = Center;
        var innerRadius = Radius - ringThickness * 0.5f;
        var outerRadius = Radius + ringThickness * 0.5f;

        float angleSpacingRad = angleSpacingDeg * ShapeMath.DEGTORAD;
        float curAngleRad = angleSpacingRad * ShapeMath.WrapF(angleOffset, 0f, 1f);
        while (curAngleRad <= 2f * ShapeMath.PI - angleSpacingRad)
        {
            var dir = new Vector2(1, 0).Rotate(curAngleRad);
            var p1 = center + dir * innerRadius;
            var p2 = center + dir * outerRadius;
            var segment = new Segment(p1, p2);
            result.Add(segment);
            curAngleRad += angleSpacingRad;
        }

        return result.Count > 0;
    }
  
    /// <summary>
    /// Appends radial stripe segments for a ring sector between two angles.
    /// </summary>
    /// <param name="result">The <see cref="Segments"/> collection that receives the generated stripe segments. Existing contents are preserved.</param>
    /// <param name="ringThickness">The radial thickness of the ring centered on this circle's radius.</param>
    /// <param name="angleSpacingDeg">The angular spacing in degrees between consecutive stripe segments.</param>
    /// <param name="minAngleDeg">The starting angle of the striped sector in degrees.</param>
    /// <param name="maxAngleDeg">The ending angle of the striped sector in degrees.</param>
    /// <param name="angleOffset">A normalized offset in the range [0,1] that shifts the first stripe by a fraction of <paramref name="angleSpacingDeg"/> from <paramref name="minAngleDeg"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="result"/> contains at least one segment after the call; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// If <paramref name="maxAngleDeg"/> is less than <paramref name="minAngleDeg"/>, stripes are generated in the negative angular direction.
    /// </remarks>
    public bool GenerateStripedRing(Segments result, float ringThickness, float angleSpacingDeg, float minAngleDeg, float maxAngleDeg, float angleOffset = 0f)
    {
        if (angleSpacingDeg <= 0) return false;
        
        if(ringThickness <= 0 || Radius <= 0 || Radius - ringThickness <= 0) return false;

        var center = Center;
        var innerRadius = Radius - ringThickness * 0.5f;
        var outerRadius = Radius + ringThickness * 0.5f;

        if (Math.Abs(minAngleDeg - maxAngleDeg) < 0.00000001f) return false;
        var dif = maxAngleDeg - minAngleDeg;
        int sign = MathF.Sign(dif);
        if (sign == 0) return false;

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
                var segment = new Segment(p1, p2);
                result.Add(segment);
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
                var segment = new Segment(p1, p2);
                result.Add(segment);
                curAngleRad += angleSpacingRad;
            }
        }
        
        return result.Count > 0;
    }
    
    #endregion
    
    #region Draw Striped Ring

    /// <summary>
    /// Draws a full striped ring using a single line style for every stripe.
    /// </summary>
    /// <param name="ringThickness">The radial thickness of the ring centered on this circle's radius.</param>
    /// <param name="angleSpacingDeg">The angular spacing in degrees between consecutive stripe segments.</param>
    /// <param name="striped">The drawing settings used for each stripe segment.</param>
    /// <param name="angleOffset">A normalized offset in the range [0,1] that shifts the starting angle by a fraction of <paramref name="angleSpacingDeg"/>.</param>
    public void DrawStripedRing(float ringThickness, float angleSpacingDeg, LineDrawingInfo striped, float angleOffset = 0f)
    {
        if (angleSpacingDeg <= 0) return;
        if(ringThickness <= 0 || Radius <= 0 || Radius - ringThickness <= 0) return;

        var center = Center;
        var innerRadius = Radius - ringThickness * 0.5f;
        var outerRadius = Radius + ringThickness * 0.5f;

        var angleSpacingRad = angleSpacingDeg * ShapeMath.DEGTORAD;
        float curAngleRad = angleSpacingRad * ShapeMath.WrapF(angleOffset, 0f, 1f);
        // float curAngleRad = angleOffset * 2f * ShapeMath.PI;
        while (curAngleRad <= 2f * ShapeMath.PI - angleSpacingRad)
        {
            var dir = new Vector2(1, 0).Rotate(curAngleRad);
            var p1 = center + dir * innerRadius;
            var p2 = center + dir * outerRadius;
            Segment.DrawSegment(p1, p2, striped);
            curAngleRad += angleSpacingRad;
        }
    }
    
    /// <summary>
    /// Draws a full striped ring using two alternating line styles.
    /// </summary>
    /// <param name="ringThickness">The radial thickness of the ring centered on this circle's radius.</param>
    /// <param name="angleSpacingDeg">The angular spacing in degrees between consecutive stripe segments.</param>
    /// <param name="striped">The drawing settings used for even-indexed stripes.</param>
    /// <param name="alternatingStriped">The drawing settings used for odd-indexed stripes.</param>
    /// <param name="angleOffset">A normalized offset in the range [0,1] that shifts the starting angle by a fraction of <paramref name="angleSpacingDeg"/>.</param>
    public void DrawStripedRing(float ringThickness, float angleSpacingDeg, LineDrawingInfo striped, LineDrawingInfo alternatingStriped, float angleOffset = 0f)
    {
        if (angleSpacingDeg <= 0) return;
        if(ringThickness <= 0 || Radius <= 0 || Radius - ringThickness <= 0) return;

        var center = Center;
        var innerRadius = Radius - ringThickness * 0.5f;
        var outerRadius = Radius + ringThickness * 0.5f;

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
            Segment.DrawSegment(p1, p2, info);
            curAngleRad += angleSpacingRad;
        }
    }
    
    /// <summary>
    /// Draws a full striped ring cycling through the provided line styles.
    /// </summary>
    /// <param name="ringThickness">The radial thickness of the ring centered on this circle's radius.</param>
    /// <param name="angleSpacingDeg">The angular spacing in degrees between consecutive stripe segments.</param>
    /// <param name="angleOffset">A normalized offset in the range [0,1] that shifts the starting angle by a fraction of <paramref name="angleSpacingDeg"/>.</param>
    /// <param name="alternatingStriped">The line styles to cycle through for successive stripes.</param>
    public void DrawStripedRing(float ringThickness, float angleSpacingDeg, float angleOffset, params LineDrawingInfo[] alternatingStriped)
    {
        if (alternatingStriped.Length <= 0) return;
        if (angleSpacingDeg <= 0) return;
        
        if(ringThickness <= 0 || Radius <= 0 || Radius - ringThickness <= 0) return;

        var center = Center;
        var innerRadius = Radius - ringThickness * 0.5f;
        var outerRadius = Radius + ringThickness * 0.5f;

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
            Segment.DrawSegment(p1, p2, info);
            curAngleRad += angleSpacingRad;
        }
    }
    
    /// <summary>
    /// Draws a striped ring sector using a single line style for every stripe.
    /// </summary>
    /// <param name="ringThickness">The radial thickness of the ring centered on this circle's radius.</param>
    /// <param name="angleSpacingDeg">The angular spacing in degrees between consecutive stripe segments.</param>
    /// <param name="minAngleDeg">The starting angle of the striped sector in degrees.</param>
    /// <param name="maxAngleDeg">The ending angle of the striped sector in degrees.</param>
    /// <param name="striped">The drawing settings used for each stripe segment.</param>
    /// <param name="angleOffset">A normalized offset in the range [0,1] that shifts the first stripe by a fraction of <paramref name="angleSpacingDeg"/> from <paramref name="minAngleDeg"/>.</param>
    /// <remarks>
    /// If <paramref name="maxAngleDeg"/> is less than <paramref name="minAngleDeg"/>, stripes are drawn in the negative angular direction.
    /// </remarks>
    public void DrawStripedRing(float ringThickness, float angleSpacingDeg, float minAngleDeg, float maxAngleDeg, LineDrawingInfo striped, float angleOffset = 0f)
    {
        if (angleSpacingDeg <= 0) return;
        
        if(ringThickness <= 0 || Radius <= 0 || Radius - ringThickness <= 0) return;

        var center = Center;
        var innerRadius = Radius - ringThickness * 0.5f;
        var outerRadius = Radius + ringThickness * 0.5f;

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
                Segment.DrawSegment(p1, p2, striped);
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
                Segment.DrawSegment(p1, p2, striped);
                curAngleRad += angleSpacingRad;
            }
        }
    }
    
    /// <summary>
    /// Draws a striped ring sector using two alternating line styles.
    /// </summary>
    /// <param name="ringThickness">The radial thickness of the ring centered on this circle's radius.</param>
    /// <param name="angleSpacingDeg">The angular spacing in degrees between consecutive stripe segments.</param>
    /// <param name="minAngleDeg">The starting angle of the striped sector in degrees.</param>
    /// <param name="maxAngleDeg">The ending angle of the striped sector in degrees.</param>
    /// <param name="striped">The drawing settings used for even-indexed stripes.</param>
    /// <param name="alternatingStriped">The drawing settings used for odd-indexed stripes.</param>
    /// <param name="angleOffset">A normalized offset in the range [0,1] that shifts the first stripe by a fraction of <paramref name="angleSpacingDeg"/> from <paramref name="minAngleDeg"/>.</param>
    /// <remarks>
    /// If <paramref name="maxAngleDeg"/> is less than <paramref name="minAngleDeg"/>, stripes are drawn in the negative angular direction.
    /// </remarks>
    public void DrawStripedRing(float ringThickness, float angleSpacingDeg, float minAngleDeg, float maxAngleDeg, LineDrawingInfo striped, LineDrawingInfo alternatingStriped, float angleOffset = 0f)
    {
        if (angleSpacingDeg <= 0) return;
        
        if(ringThickness <= 0 || Radius <= 0 || Radius - ringThickness <= 0) return;

        var center = Center;
        var innerRadius = Radius - ringThickness * 0.5f;
        var outerRadius = Radius + ringThickness * 0.5f;

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
                Segment.DrawSegment(p1, p2, info);
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
                Segment.DrawSegment(p1, p2, info);
                curAngleRad += angleSpacingRad;
            }
        }
    }
    
    /// <summary>
    /// Draws a striped ring sector cycling through the provided line styles.
    /// </summary>
    /// <param name="ringThickness">The radial thickness of the ring centered on this circle's radius.</param>
    /// <param name="angleSpacingDeg">The angular spacing in degrees between consecutive stripe segments.</param>
    /// <param name="minAngleDeg">The starting angle of the striped sector in degrees.</param>
    /// <param name="maxAngleDeg">The ending angle of the striped sector in degrees.</param>
    /// <param name="angleOffset">A normalized offset in the range [0,1] that shifts the first stripe by a fraction of <paramref name="angleSpacingDeg"/> from <paramref name="minAngleDeg"/>.</param>
    /// <param name="alternatingStriped">The line styles to cycle through for successive stripes.</param>
    /// <remarks>
    /// If <paramref name="maxAngleDeg"/> is less than <paramref name="minAngleDeg"/>, stripes are drawn in the negative angular direction.
    /// </remarks>
    public void DrawStripedRing(float ringThickness, float angleSpacingDeg, float minAngleDeg, float maxAngleDeg, float angleOffset, params LineDrawingInfo[] alternatingStriped)
    {
        if (alternatingStriped.Length <= 0) return;
        if (angleSpacingDeg <= 0) return;
        
        if(ringThickness <= 0 || Radius <= 0 || Radius - ringThickness <= 0) return;

        var center = Center;
        var innerRadius = Radius - ringThickness * 0.5f;
        var outerRadius = Radius + ringThickness * 0.5f;

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
                Segment.DrawSegment(p1, p2, info);
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
                Segment.DrawSegment(p1, p2, info);
                curAngleRad += angleSpacingRad;
            }
        }
    }

    #endregion
}

