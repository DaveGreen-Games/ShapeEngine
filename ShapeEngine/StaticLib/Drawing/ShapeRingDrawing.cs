using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.StaticLib.Drawing;

public static class ShapeRingDrawing
{
    
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float lineThickness, ColorRgba color, float sideLength = 8f)
    {
        ShapeCircleDrawing.DrawCircleLines(center, innerRadius, lineThickness, color, sideLength);
        ShapeCircleDrawing.DrawCircleLines(center, outerRadius, lineThickness, color, sideLength);
    }
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, LineDrawingInfo lineInfo, float sideLength = 8f)
    {
        ShapeCircleDrawing.DrawCircleLines(center, innerRadius, lineInfo, 0f, sideLength);
        ShapeCircleDrawing.DrawCircleLines(center, outerRadius, lineInfo, 0f, sideLength);
    }
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float innerRotDeg, float outerRotDeg, LineDrawingInfo lineInfo, float sideLength = 8f)
    {
        ShapeCircleDrawing.DrawCircleLines(center, innerRadius, lineInfo, innerRotDeg, sideLength);
        ShapeCircleDrawing.DrawCircleLines(center, outerRadius, lineInfo, outerRotDeg, sideLength);
    }
    public static void DrawRingLinesPercentage(Vector2 center, float innerRadius, float outerRadius, float f, float innerRotDeg, float outerRotDeg, LineDrawingInfo lineInfo, float sideLength = 8f)
    {
        ShapeCircleDrawing.DrawCircleLinesPercentage(center, innerRadius, f, lineInfo, innerRotDeg, sideLength);
        ShapeCircleDrawing.DrawCircleLinesPercentage(center, outerRadius, f, lineInfo, outerRotDeg, sideLength);
    }
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float innerRotDeg, float outerRotDeg, float innerSideLength, float outerSideLength, LineDrawingInfo lineInfo)
    {
        ShapeCircleDrawing.DrawCircleLines(center, innerRadius, lineInfo, innerRotDeg, innerSideLength);
        ShapeCircleDrawing.DrawCircleLines(center, outerRadius, lineInfo, outerRotDeg, outerSideLength);
    }
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float innerRotDeg, float outerRotDeg, float innerSideLength, float outerSideLength, LineDrawingInfo innerLineInfo, LineDrawingInfo outerLineInfo)
    {
        ShapeCircleDrawing.DrawCircleLines(center, innerRadius, innerLineInfo, innerRotDeg, innerSideLength);
        ShapeCircleDrawing.DrawCircleLines(center, outerRadius, outerLineInfo, outerRotDeg, outerSideLength);
    }
    public static void DrawRingLinesPercentage(Vector2 center, float innerRadius, float outerRadius, float f, float innerRotDeg, float outerRotDeg, float innerSideLength, float outerSideLength, LineDrawingInfo lineInfo)
    {
        ShapeCircleDrawing.DrawCircleLinesPercentage(center, innerRadius, f, lineInfo, innerRotDeg, innerSideLength);
        ShapeCircleDrawing.DrawCircleLinesPercentage(center, outerRadius, f, lineInfo, outerRotDeg, outerSideLength);
    }
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float f, float innerRotDeg, float outerRotDeg, float innerSideLength, float outerSideLength, LineDrawingInfo innerLineInfo, LineDrawingInfo outerLineInfo)
    {
        ShapeCircleDrawing.DrawCircleLinesPercentage(center, outerRadius, f, outerLineInfo, outerRotDeg, outerSideLength);
        ShapeCircleDrawing.DrawCircleLinesPercentage(center, innerRadius, f, innerLineInfo, innerRotDeg, innerSideLength);
    }
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float innerRotDeg, float outerRotDeg, int sides, LineDrawingInfo lineInfo)
    {
        ShapeCircleDrawing.DrawCircleLines(center, innerRadius, lineInfo, innerRotDeg, sides);
        ShapeCircleDrawing.DrawCircleLines(center, outerRadius, lineInfo, outerRotDeg, sides);
    }
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float innerRotDeg, float outerRotDeg, int innerSides, int outerSides, LineDrawingInfo lineInfo)
    {
        ShapeCircleDrawing.DrawCircleLines(center, innerRadius, lineInfo, innerRotDeg, innerSides);
        ShapeCircleDrawing.DrawCircleLines(center, outerRadius, lineInfo, outerRotDeg, outerSides);
    }
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float innerRotDeg, float outerRotDeg, int innerSides, int outerSides, LineDrawingInfo innerLineInfo, LineDrawingInfo outerLineInfo)
    {
        ShapeCircleDrawing.DrawCircleLines(center, innerRadius, innerLineInfo, innerRotDeg, innerSides);
        ShapeCircleDrawing.DrawCircleLines(center, outerRadius, outerLineInfo, outerRotDeg, outerSides);
    }
    public static void DrawRingLinesPercentage(Vector2 center, float innerRadius, float outerRadius, float f, float innerRotDeg, float outerRotDeg, int innerSides, int outerSides, LineDrawingInfo lineInfo)
    {
        ShapeCircleDrawing.DrawCircleLinesPercentage(center, innerRadius, f, lineInfo, innerRotDeg, innerSides);
        ShapeCircleDrawing.DrawCircleLinesPercentage(center, outerRadius, f, lineInfo, outerRotDeg, outerSides);
    }
    public static void DrawRingLinesPercentage(Vector2 center, float innerRadius, float outerRadius, float f, float innerRotDeg, float outerRotDeg, int innerSides, int outerSides, LineDrawingInfo innerLineInfo, LineDrawingInfo outerLineInfo)
    {
        ShapeCircleDrawing.DrawCircleLinesPercentage(center, innerRadius, f, innerLineInfo, innerRotDeg, innerSides);
        ShapeCircleDrawing.DrawCircleLinesPercentage(center, outerRadius, f, outerLineInfo, outerRotDeg, outerSides);
    }
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, int sides, LineDrawingInfo lineInfo)
    {
        ShapeCircleDrawing.DrawCircleLines(center, innerRadius, lineInfo, sides);
        ShapeCircleDrawing.DrawCircleLines(center, outerRadius, lineInfo, sides);
    }
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, int innerSides, int outerSides, LineDrawingInfo lineInfo)
    {
        ShapeCircleDrawing.DrawCircleLines(center, innerRadius, lineInfo, innerSides);
        ShapeCircleDrawing.DrawCircleLines(center, outerRadius, lineInfo, outerSides);
    }
    public static void DrawRingLinesPercentage(Vector2 center, float innerRadius, float outerRadius, float f, int innerSides, int outerSides, LineDrawingInfo lineInfo)
    {
        ShapeCircleDrawing.DrawCircleLinesPercentage(center, innerRadius, f, lineInfo, innerSides);
        ShapeCircleDrawing.DrawCircleLinesPercentage(center, outerRadius, f, lineInfo, outerSides);
    }
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, int innerSides, int outerSides, LineDrawingInfo innerLineInfo, LineDrawingInfo outerLineInfo)
    {
        ShapeCircleDrawing.DrawCircleLines(center, innerRadius, innerLineInfo, innerSides);
        ShapeCircleDrawing.DrawCircleLines(center, outerRadius, outerLineInfo, outerSides);
    }
    public static void DrawRingLinesPercentage(Vector2 center, float innerRadius, float outerRadius, float f, int innerSides, int outerSides, LineDrawingInfo innerLineInfo, LineDrawingInfo outerLineInfo)
    {
        ShapeCircleDrawing.DrawCircleLinesPercentage(center, innerRadius, f, innerLineInfo, innerSides);
        ShapeCircleDrawing.DrawCircleLinesPercentage(center, outerRadius, f, outerLineInfo, outerSides);
    }
    public static void DrawRing(Vector2 center, float innerRadius, float outerRadius, ColorRgba color, float sideLength = 8f)
    {
        DrawSectorRing(center, innerRadius, outerRadius, 0, 360, color, sideLength);
    }
    public static void DrawRing(Vector2 center, float innerRadius, float outerRadius, int sides, ColorRgba color)
    {
        DrawSectorRing(center, innerRadius, outerRadius, 0, 360, sides, color);
    }
    
    /// <summary>
    /// Draws a circle where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="center"></param>
    /// <param name="innerRadius"></param>
    /// <param name="outerRadius"></param>
    /// <param name="innerRotDeg">The rotation of the inner circle in degrees.</param>
    /// <param name="outerRotDeg">The rotation of the outer circle in degrees.</param>
    /// <param name="lineInfo">How to draw the lines.</param>
    /// <param name="sides">How many sides the circle should be drawn with.</param>
    /// <param name="sideScaleFactor">The scale factor for each side. 0f means no circle is drawn, 1f means normal circle is drawn,
    /// 0.5 means each side is half as long.</param>
    /// <param name="sideScaleOrigin">The point along the line to scale from in both directions.</param>
    public static void DrawRingLinesScaled(Vector2 center, float innerRadius, float outerRadius, float innerRotDeg, float outerRotDeg, int sides, LineDrawingInfo lineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0f) return;
        if (sideScaleFactor >= 1f)
        {
            ShapeCircleDrawing.DrawCircleLines(center, innerRadius, lineInfo, innerRotDeg, sides);
            ShapeCircleDrawing.DrawCircleLines(center, outerRadius, lineInfo, outerRotDeg, sides);
            return;
        }
        
        var angleStep = (2f * ShapeMath.PI) / sides;
        var rotRadInner = innerRotDeg * ShapeMath.DEGTORAD;
        var rotRadOuter = outerRotDeg * ShapeMath.DEGTORAD;
        
        for (int i = 0; i < sides; i++)
        {
            var nextIndexInner = (i + 1) % sides;
            var startInner = center + new Vector2(innerRadius, 0f).Rotate(rotRadInner + angleStep * i);
            var endInner = center + new Vector2(innerRadius, 0f).Rotate(rotRadInner + angleStep * nextIndexInner);
            
            ShapeSegmentDrawing.DrawSegment(startInner, endInner, lineInfo, sideScaleFactor, sideScaleOrigin);
            
            var nextIndexOuter = (i + 1) % sides;
            var startOuter = center + new Vector2(outerRadius, 0f).Rotate(rotRadOuter + angleStep * i);
            var endOuter = center + new Vector2(outerRadius, 0f).Rotate(rotRadOuter + angleStep * nextIndexOuter);
            
            ShapeSegmentDrawing.DrawSegment(startOuter, endOuter, lineInfo, sideScaleFactor, sideScaleOrigin);
            
        }
    }
    
    /// <summary>
    /// Draws a circle where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="center"></param>
    /// <param name="innerRadius"></param>
    /// <param name="outerRadius"></param>
    /// <param name="innerRotDeg">The rotation of the inner circle in degrees.</param>
    /// <param name="outerRotDeg">The rotation of the outer circle in degrees.</param>
    /// <param name="lineInfo">How to draw the lines.</param>
    /// <param name="innerSides">How many sides the inner circle should be drawn with.</param>
    /// <param name="outerSides">How many sides the outer circle should be drawn with.</param>
    /// <param name="sideScaleFactor">The scale factor for each side. 0f means no circle is drawn, 1f means normal circle is drawn,
    /// 0.5 means each side is half as long.</param>
    /// <param name="sideScaleOrigin">The point along the line to scale from in both directions.</param>
    public static void DrawRingLinesScaled(Vector2 center, float innerRadius, float outerRadius, float innerRotDeg, float outerRotDeg, int innerSides, int outerSides, LineDrawingInfo lineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0f) return;
        if (sideScaleFactor >= 1f)
        {
            ShapeCircleDrawing.DrawCircleLines(center, innerRadius, lineInfo, innerRotDeg, innerSides);
            ShapeCircleDrawing.DrawCircleLines(center, outerRadius, lineInfo, outerRotDeg, outerSides);
            return;
        }
        
        var innerAngleStep = (2f * ShapeMath.PI) / innerSides;
        var outerAngleStep = (2f * ShapeMath.PI) / outerSides;
        var innerRotRad = innerRotDeg * ShapeMath.DEGTORAD;
        var outerRotRad = outerRotDeg * ShapeMath.DEGTORAD;

        int maxSides = innerSides > outerSides ? innerSides : outerSides;
        
        for (int i = 0; i < maxSides; i++)
        {
            if (i < innerSides)
            {
                var nextIndexInner = (i + 1) % innerSides;
                var startInner = center + new Vector2(innerRadius, 0f).Rotate(innerRotRad + innerAngleStep * i);
                var endInner = center + new Vector2(innerRadius, 0f).Rotate(innerRotRad + innerAngleStep * nextIndexInner);
                ShapeSegmentDrawing.DrawSegment(startInner, endInner, lineInfo, sideScaleFactor, sideScaleOrigin);
            }
            
            if (i < outerSides)
            {
                var nextIndexOuter = (i + 1) % outerSides;
                var startOuter = center + new Vector2(outerRadius, 0f).Rotate(outerRotRad + outerAngleStep * i);
                var endOuter = center + new Vector2(outerRadius, 0f).Rotate(outerRotRad + outerAngleStep * nextIndexOuter);
                ShapeSegmentDrawing.DrawSegment(startOuter, endOuter, lineInfo, sideScaleFactor, sideScaleOrigin);
            }
            
        }
    }

    /// <summary>
    /// Draws a circle where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="center"></param>
    /// <param name="innerRadius"></param>
    /// <param name="outerRadius"></param>
    /// <param name="innerRotDeg">The rotation of the inner circle in degrees.</param>
    /// <param name="outerRotDeg">The rotation of the outer circle in degrees.</param>
    /// <param name="innerLineInfo">How to draw the inner lines.</param>
    /// <param name="outerLineInfo">How to draw the outer lines.</param>
    /// <param name="innerSides">How many sides the inner circle should be drawn with.</param>
    /// <param name="outerSides">How many sides the outer circle should be drawn with.</param>
    /// <param name="sideScaleFactor">The scale factor for each side. 0f means no circle is drawn, 1f means normal circle is drawn,
    /// 0.5 means each side is half as long.</param>
    /// <param name="sideScaleOrigin">The point along the line to scale from in both directions.</param>
    public static void DrawRingLinesScaled(Vector2 center, float innerRadius, float outerRadius, float innerRotDeg, float outerRotDeg, int innerSides, int outerSides, LineDrawingInfo innerLineInfo, LineDrawingInfo outerLineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0f) return;
        if (sideScaleFactor >= 1f)
        {
            ShapeCircleDrawing.DrawCircleLines(center, innerRadius, innerLineInfo, innerRotDeg, innerSides);
            ShapeCircleDrawing.DrawCircleLines(center, outerRadius, outerLineInfo, outerRotDeg, outerSides);
            return;
        }
        
        var innerAngleStep = (2f * ShapeMath.PI) / innerSides;
        var outerAngleStep = (2f * ShapeMath.PI) / outerSides;
        var innerRotRad = innerRotDeg * ShapeMath.DEGTORAD;
        var outerRotRad = outerRotDeg * ShapeMath.DEGTORAD;

        int maxSides = innerSides > outerSides ? innerSides : outerSides;
        
        for (int i = 0; i < maxSides; i++)
        {
            if (i < innerSides)
            {
                var nextIndexInner = (i + 1) % innerSides;
                var startInner = center + new Vector2(innerRadius, 0f).Rotate(innerRotRad + innerAngleStep * i);
                var endInner = center + new Vector2(innerRadius, 0f).Rotate(innerRotRad + innerAngleStep * nextIndexInner);
                ShapeSegmentDrawing.DrawSegment(startInner, endInner, innerLineInfo, sideScaleFactor, sideScaleOrigin);
            }
            
            if (i < outerSides)
            {
                var nextIndexOuter = (i + 1) % outerSides;
                var startOuter = center + new Vector2(outerRadius, 0f).Rotate(outerRotRad + outerAngleStep * i);
                var endOuter = center + new Vector2(outerRadius, 0f).Rotate(outerRotRad + outerAngleStep * nextIndexOuter);
                ShapeSegmentDrawing.DrawSegment(startOuter, endOuter, outerLineInfo, sideScaleFactor, sideScaleOrigin);
            }
            
        }
    }
    
    /// <summary>
    /// Draws a circle where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="center"></param>
    /// <param name="innerRadius"></param>
    /// <param name="outerRadius"></param>
    /// <param name="innerRotDeg">The rotation of the inner circle in degrees.</param>
    /// <param name="outerRotDeg">The rotation of the outer circle in degrees.</param>
    /// <param name="innerLineInfo">How to draw the inner lines.</param>
    /// <param name="outerLineInfo">How to draw the outer lines.</param>
    /// <param name="innerSides">How many sides the inner circle should be drawn with.</param>
    /// <param name="outerSides">How many sides the outer circle should be drawn with.</param>
    /// <param name="innerSideScaleFactor">The scale factor for each side on the inner circle. 0f means no circle is drawn, 1f means normal circle is drawn,
    /// 0.5 means each side is half as long.</param>
    /// <param name="outerSideScaleFactor">The scale factor for each side on the outer circle. 0f means no circle is drawn, 1f means normal circle is drawn,
    /// 0.5 means each side is half as long.</param>
    /// <param name="innerSideScaleOrigin">The point along the line to scale from in both directions.</param>
    /// <param name="outerSideScaleOrigin">The point along the line to scale from in both directions.</param>
    public static void DrawRingLinesScaled(Vector2 center, float innerRadius, float outerRadius, float innerRotDeg, float outerRotDeg, int innerSides, int outerSides, LineDrawingInfo innerLineInfo, LineDrawingInfo outerLineInfo, float innerSideScaleFactor, float outerSideScaleFactor, float innerSideScaleOrigin, float outerSideScaleOrigin)
    {
        bool drawInner = true;
        bool drawOuter = true;
        if (innerSideScaleFactor >= 1f)
        {
            drawInner = false;
            ShapeCircleDrawing.DrawCircleLines(center, innerRadius, innerLineInfo, innerRotDeg, innerSides);
        }
        if (outerSideScaleFactor >= 1f)
        {
            drawOuter = false;
            ShapeCircleDrawing.DrawCircleLines(center, outerRadius, outerLineInfo, outerRotDeg, outerSides);
        }

        if (!drawInner && !drawOuter) return;
        
        var innerAngleStep = (2f * ShapeMath.PI) / innerSides;
        var outerAngleStep = (2f * ShapeMath.PI) / outerSides;
        var innerRotRad = innerRotDeg * ShapeMath.DEGTORAD;
        var outerRotRad = outerRotDeg * ShapeMath.DEGTORAD;

        int maxSides;
        if (!drawInner) maxSides = outerSides;
        else if (!drawOuter) maxSides = innerSides;
        else maxSides = innerSides > outerSides ? innerSides : outerSides;
        
        for (int i = 0; i < maxSides; i++)
        {
            if (drawInner && i < innerSides)
            {
                var nextIndexInner = (i + 1) % innerSides;
                var startInner = center + new Vector2(innerRadius, 0f).Rotate(innerRotRad + innerAngleStep * i);
                var endInner = center + new Vector2(innerRadius, 0f).Rotate(innerRotRad + innerAngleStep * nextIndexInner);
                ShapeSegmentDrawing.DrawSegment(startInner, endInner, innerLineInfo, innerSideScaleFactor, innerSideScaleOrigin);
            }
            
            if (drawOuter && i < outerSides)
            {
                var nextIndexOuter = (i + 1) % outerSides;
                var startOuter = center + new Vector2(outerRadius, 0f).Rotate(outerRotRad + outerAngleStep * i);
                var endOuter = center + new Vector2(outerRadius, 0f).Rotate(outerRotRad + outerAngleStep * nextIndexOuter);
                ShapeSegmentDrawing.DrawSegment(startOuter, endOuter, outerLineInfo, outerSideScaleFactor, outerSideScaleOrigin);
            }
            
        }
    }

    public static void DrawSectorRingLines(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float lineThickness, ColorRgba color, float sideLength = 8f)
    {
        ShapeCircleDrawing.DrawCircleSectorLines(center, innerRadius, startAngleDeg, endAngleDeg, lineThickness, color, false, sideLength);
        ShapeCircleDrawing.DrawCircleSectorLines(center, outerRadius, startAngleDeg, endAngleDeg, lineThickness, color, false, sideLength);

        float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
        float endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
        var innerStart = center + (ShapeVec.Right() * innerRadius).Rotate(startAngleRad);
        var outerStart = center + (ShapeVec.Right() * outerRadius).Rotate(startAngleRad);
        ShapeSegmentDrawing.DrawSegment(innerStart, outerStart, lineThickness, color, LineCapType.CappedExtended, 4);

        var innerEnd = center + (ShapeVec.Right() * innerRadius).Rotate(endAngleRad);
        var outerEnd = center + (ShapeVec.Right() * outerRadius).Rotate(endAngleRad);
        ShapeSegmentDrawing.DrawSegment(innerEnd, outerEnd, lineThickness, color, LineCapType.CappedExtended, 4);
    }
    public static void DrawSectorRingLines(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, float lineThickness, ColorRgba color, float sideLength = 8f)
    {
        DrawSectorRingLines(center, innerRadius, outerRadius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, lineThickness, color, sideLength);
    }
    public static void DrawSectorRingLines(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, LineDrawingInfo lineInfo, float sideLength = 8f)
    {
        ShapeCircleDrawing.DrawCircleSectorLines(center, innerRadius, startAngleDeg, endAngleDeg, lineInfo, false, sideLength);
        ShapeCircleDrawing.DrawCircleSectorLines(center, outerRadius, startAngleDeg, endAngleDeg, lineInfo, false, sideLength);

        float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
        float endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
        var innerStart = center + (ShapeVec.Right() * innerRadius - new Vector2(lineInfo.Thickness / 2, 0)).Rotate(startAngleRad);
        var outerStart = center + (ShapeVec.Right() * outerRadius + new Vector2(lineInfo.Thickness / 2, 0)).Rotate(startAngleRad);
        ShapeSegmentDrawing.DrawSegment(innerStart, outerStart, lineInfo);

        var innerEnd = center + (ShapeVec.Right() * innerRadius - new Vector2(lineInfo.Thickness / 2, 0)).Rotate(endAngleRad);
        var outerEnd = center + (ShapeVec.Right() * outerRadius + new Vector2(lineInfo.Thickness / 2, 0)).Rotate(endAngleRad);
        ShapeSegmentDrawing.DrawSegment(innerEnd, outerEnd, lineInfo);
    }
    public static void DrawSectorRingLines(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, LineDrawingInfo lineInfo, float sideLength = 8f)
    {
        DrawSectorRingLines(center, innerRadius, outerRadius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, lineInfo, sideLength);
    }

    public static void DrawSectorRing(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, ColorRgba color, float sideLength = 10f)
    {
        float anglePiece = endAngleDeg - startAngleDeg;
        int sides = ShapeCircleDrawing.GetCircleArcSideCount(outerRadius, MathF.Abs(anglePiece), sideLength);
        Raylib.DrawRing(center, innerRadius, outerRadius, startAngleDeg, endAngleDeg, sides, color.ToRayColor());
    }
    public static void DrawSectorRing(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, int sides, ColorRgba color)
    {
        Raylib.DrawRing(center, innerRadius, outerRadius, startAngleDeg, endAngleDeg, sides, color.ToRayColor());
    }
    public static void DrawSectorRing(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, ColorRgba color, float sideLength = 10f)
    {
        DrawSectorRing(center, innerRadius, outerRadius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, color, sideLength);
    }
    public static void DrawSectorRing(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, int sides, ColorRgba color)
    {
        DrawSectorRing(center, innerRadius, outerRadius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, sides, color);
    }

}