using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.CircleDef;

//TODO: Check and cleanup based on CircleDrawing changes

/// <summary>
/// Provides static methods for drawing ring shapes and ring outlines with various customization options.
/// </summary>
/// <remarks>
/// This class contains utility methods for drawing rings, sector rings, and their outlines with support for line thickness, color, rotation, scaling, and side count.
/// </remarks>
public static class RingDrawing
{
    #region Draw Ring
    /// <summary>
    /// Draws a filled ring shape.
    /// </summary>
    /// <param name="center">The center position of the ring.</param>
    /// <param name="innerRadius">The radius of the inner circle.</param>
    /// <param name="outerRadius">The radius of the outer circle.</param>
    /// <param name="color">The color of the ring.</param>
    /// <param name="sideLength">The length of each side segment. Default is 8.</param>
    /// <remarks>
    /// This draws a complete ring (360 degrees).
    /// </remarks>
    public static void DrawRing(Vector2 center, float innerRadius, float outerRadius, ColorRgba color, float smoothness)
    {
        DrawSectorRing(center, innerRadius, outerRadius, 0, 360, color, smoothness);
    }
    #endregion
    
    //TODO: Use Raylib.DrawRing for all of those instead of circle.DrawLines!
    #region Draw Ring Lines
    /// <summary>
    /// Draws the outlines of a ring by drawing the inner and outer circles as lines.
    /// </summary>
    /// <param name="center">The center position of the ring.</param>
    /// <param name="innerRadius">The radius of the inner circle.</param>
    /// <param name="outerRadius">The radius of the outer circle.</param>
    /// <param name="lineThickness">The thickness of the ring lines.</param>
    /// <param name="color">The color of the ring lines.</param>
    /// <param name="sideLength">The length of each side segment. Default is 8.</param>
    /// <remarks>
    /// Both the inner and outer circles are drawn as lines to represent the ring's outline.
    /// </remarks>
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float lineThickness, ColorRgba color, float smoothness)
    {
        var circle = new Circle(center, innerRadius);
        circle.DrawLines(lineThickness, color, smoothness);
        circle = circle.SetRadius(outerRadius);
        circle.DrawLines(lineThickness, color, smoothness);
    }

    /// <summary>
    /// Draws the outlines of a ring using line drawing information for style.
    /// </summary>
    /// <param name="center">The center position of the ring.</param>
    /// <param name="innerRadius">The radius of the inner circle.</param>
    /// <param name="outerRadius">The radius of the outer circle.</param>
    /// <param name="lineInfo">The line drawing style information.</param>
    /// <param name="sideLength">The length of each side segment. Default is 8.</param>
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, LineDrawingInfo lineInfo, float smoothness)
    {
        var circle = new Circle(center, innerRadius);
        circle.DrawLines(lineInfo, smoothness);
        circle = circle.SetRadius(outerRadius);
        circle.DrawLines(lineInfo, smoothness);
    }

    /// <summary>
    /// Draws the outlines of a ring with separate rotation for inner and outer circles.
    /// </summary>
    /// <param name="center">The center position of the ring.</param>
    /// <param name="innerRadius">The radius of the inner circle.</param>
    /// <param name="outerRadius">The radius of the outer circle.</param>
    /// <param name="innerRotDeg">Rotation of the inner circle in degrees.</param>
    /// <param name="outerRotDeg">Rotation of the outer circle in degrees.</param>
    /// <param name="lineInfo">The line drawing style information.</param>
    /// <param name="sideLength">The length of each side segment. Default is 8.</param>
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float innerRotDeg, float outerRotDeg, LineDrawingInfo lineInfo, float smoothness)
    {
        var circle = new Circle(center, innerRadius);
        circle.DrawLines(innerRotDeg, lineInfo, smoothness);
        circle = circle.SetRadius(outerRadius);
        circle.DrawLines(outerRotDeg, lineInfo, smoothness);
    }

    /// <summary>
    /// Draws the outlines of a ring with separate rotation, side length, and line info for inner and outer circles.
    /// </summary>
    /// <param name="center">The center position of the ring.</param>
    /// <param name="innerRadius">The radius of the inner circle.</param>
    /// <param name="outerRadius">The radius of the outer circle.</param>
    /// <param name="innerRotDeg">Rotation of the inner circle in degrees.</param>
    /// <param name="outerRotDeg">Rotation of the outer circle in degrees.</param>
    /// <param name="innerSideLength">Side length for the inner circle.</param>
    /// <param name="outerSideLength">Side length for the outer circle.</param>
    /// <param name="innerLineInfo">Line drawing info for the inner circle.</param>
    /// <param name="outerLineInfo">Line drawing info for the outer circle.</param>
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float innerRotDeg, float outerRotDeg, LineDrawingInfo innerLineInfo, LineDrawingInfo outerLineInfo, float innerSmoothness, float outerSmoothness)
    {
        var circle = new Circle(center, innerRadius);
        circle.DrawLines(innerRotDeg, innerLineInfo, innerSmoothness);
        circle = circle.SetRadius(outerRadius);
        circle.DrawLines(outerRotDeg, outerLineInfo, outerSmoothness);
    }
   
    /// <summary>
    /// Draws a percentage of the outlines of a ring with separate rotation, side length, and line info for inner and outer circles.
    /// </summary>
    /// <param name="center">The center position of the ring.</param>
    /// <param name="innerRadius">The radius of the inner circle.</param>
    /// <param name="outerRadius">The radius of the outer circle.</param>
    /// <param name="f">The percentage of the ring to draw (0 to 1).</param>
    /// <param name="innerRotDeg">Rotation of the inner circle in degrees.</param>
    /// <param name="outerRotDeg">Rotation of the outer circle in degrees.</param>
    /// <param name="innerSideLength">Side length for the inner circle.</param>
    /// <param name="outerSideLength">Side length for the outer circle.</param>
    /// <param name="innerLineInfo">Line drawing info for the inner circle.</param>
    /// <param name="outerLineInfo">Line drawing info for the outer circle.</param>
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float f, float innerRotDeg, float outerRotDeg, 
        LineDrawingInfo innerLineInfo, LineDrawingInfo outerLineInfo, float innerSmoothness, float outerSmoothness)
    {
        var circle = new Circle(center, innerRadius);
        circle.DrawLinesPercentage(f, innerRotDeg, innerLineInfo, innerSmoothness);
        circle = circle.SetRadius(outerRadius);
        circle.DrawLinesPercentage(f, outerRotDeg, outerLineInfo, outerSmoothness);
    }

    
    
    #endregion
    
    #region Draw Ring Lines Percentage
    /// <summary>
    /// Draws a percentage of the outlines of a ring with separate rotation for inner and outer circles.
    /// </summary>
    /// <param name="center">The center position of the ring.</param>
    /// <param name="innerRadius">The radius of the inner circle.</param>
    /// <param name="outerRadius">The radius of the outer circle.</param>
    /// <param name="f">The percentage of the ring to draw (0 to 1).</param>
    /// <param name="innerRotDeg">Rotation of the inner circle in degrees.</param>
    /// <param name="outerRotDeg">Rotation of the outer circle in degrees.</param>
    /// <param name="lineInfo">The line drawing style information.</param>
    /// <param name="sideLength">The length of each side segment. Default is 8.</param>
    public static void DrawRingLinesPercentage(Vector2 center, float innerRadius, float outerRadius, float f, float innerRotDeg, float outerRotDeg, LineDrawingInfo lineInfo, float smoothness)
    {
        var circle = new Circle(center, innerRadius);
        circle.DrawLinesPercentage(f, innerRotDeg, lineInfo, smoothness);
        circle = circle.SetRadius(outerRadius);
        circle.DrawLinesPercentage(f, outerRotDeg, lineInfo, smoothness);
    }
    
    /// <summary>
    /// Draws a percentage of the outlines of a ring with separate rotation and side length for inner and outer circles.
    /// </summary>
    /// <param name="center">The center position of the ring.</param>
    /// <param name="innerRadius">The radius of the inner circle.</param>
    /// <param name="outerRadius">The radius of the outer circle.</param>
    /// <param name="f">The percentage of the ring to draw (0 to 1).</param>
    /// <param name="innerRotDeg">Rotation of the inner circle in degrees.</param>
    /// <param name="outerRotDeg">Rotation of the outer circle in degrees.</param>
    /// <param name="innerSideLength">Side length for the inner circle.</param>
    /// <param name="outerSideLength">Side length for the outer circle.</param>
    /// <param name="lineInfo">The line drawing style information.</param>
    public static void DrawRingLinesPercentage(Vector2 center, float innerRadius, float outerRadius, float f, float innerRotDeg, float outerRotDeg, 
        LineDrawingInfo lineInfo, float innerSmoothness, float outerSmoothness)
    {
        var circle = new Circle(center, innerRadius);
        circle.DrawLinesPercentage(f, innerRotDeg, lineInfo, innerSmoothness);
        circle = circle.SetRadius(outerRadius);
        circle.DrawLinesPercentage(f, outerRotDeg, lineInfo, outerSmoothness);
    }
    
    /// <summary>
    /// Draws a percentage of the outlines of a ring with separate side counts, rotation, and line info for inner and outer circles.
    /// </summary>
    /// <param name="center">The center position of the ring.</param>
    /// <param name="innerRadius">The radius of the inner circle.</param>
    /// <param name="outerRadius">The radius of the outer circle.</param>
    /// <param name="f">The percentage of the ring to draw (0 to 1).</param>
    /// <param name="innerRotDeg">Rotation of the inner circle in degrees.</param>
    /// <param name="outerRotDeg">Rotation of the outer circle in degrees.</param>
    /// <param name="innerSides">Number of sides for the inner circle.</param>
    /// <param name="outerSides">Number of sides for the outer circle.</param>
    /// <param name="innerLineInfo">Line drawing info for the inner circle.</param>
    /// <param name="outerLineInfo">Line drawing info for the outer circle.</param>
    public static void DrawRingLinesPercentage(Vector2 center, float innerRadius, float outerRadius, float f, float innerRotDeg, float outerRotDeg,
         LineDrawingInfo innerLineInfo, LineDrawingInfo outerLineInfo, float innerSmoothness, float outerSmoothness)
    {
        var circle = new Circle(center, innerRadius);
        circle.DrawLinesPercentage(f, innerRotDeg, innerLineInfo, innerSmoothness);
        circle = circle.SetRadius(outerRadius);
        circle.DrawLinesPercentage(f, outerRotDeg, outerLineInfo, outerSmoothness);
    }
    public static void DrawRingLinesPercentage(Vector2 center, float innerRadius, float outerRadius, float rotDeg, float f, float innerRotDeg, float outerRotDeg,
        LineDrawingInfo innerLineInfo, LineDrawingInfo outerLineInfo, float innerSmoothness, float outerSmoothness)
    {
        var circle = new Circle(center, innerRadius);
        circle.DrawLinesPercentage(f, innerRotDeg + rotDeg, innerLineInfo, innerSmoothness);
        circle = circle.SetRadius(outerRadius);
        circle.DrawLinesPercentage(f, outerRotDeg + rotDeg, outerLineInfo, outerSmoothness);
    }
    #endregion 

    #region Draw Ring Lines Scaled
    /// <summary>
    /// Draws a ring where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="center">The center position of the ring.</param>
    /// <param name="innerRadius">The radius of the inner circle.</param>
    /// <param name="outerRadius">The radius of the outer circle.</param>
    /// <param name="innerRotDeg">Rotation of the inner circle in degrees.</param>
    /// <param name="outerRotDeg">Rotation of the outer circle in degrees.</param>
    /// <param name="lineInfo">How to draw the lines.</param>
    /// <param name="sides">Number of sides for the ring.</param>
    /// <param name="sideScaleFactor">
    /// <para>The scale factor for each side.</para>
    /// <list type="bullet">
    /// <item><description>0: No ring is drawn.</description></item>
    /// <item><description>1: The normal ring is drawn.</description></item>
    /// <item><description>0.5: Each side is half as long.</description></item>
    /// </list>
    /// </param>
    /// <param name="sideScaleOrigin">
    /// The point along the line to scale from, in both directions (0 to 1).
    /// <list type="bullet">
    /// <item><description>0: Start of Segment</description></item>
    /// <item><description>0.5: Center of Segment</description></item>
    /// <item><description>1: End of Segment</description></item>
    /// </list>
    /// </param>
    /// <remarks>
    /// This method allows for creative effects such as dashed or rings.
    /// </remarks>
    public static void DrawRingLinesScaled(Vector2 center, float innerRadius, float outerRadius, float innerRotDeg, float outerRotDeg,  
        LineDrawingInfo lineInfo, float smoothness, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0f) return;
        if (sideScaleFactor >= 1f)
        {
            var circle = new Circle(center, innerRadius);
            circle.DrawLines(innerRotDeg, lineInfo, smoothness);
            circle = circle.SetRadius(outerRadius);
            circle.DrawLines(outerRotDeg, lineInfo, smoothness);
            return;
        }

        if (!CircleDrawing.CalculateCircleDrawingParameters(outerRadius, smoothness, out float angleStep, out int sides)) return;
        
        var rotRadInner = innerRotDeg * ShapeMath.DEGTORAD;
        var rotRadOuter = outerRotDeg * ShapeMath.DEGTORAD;
        
        for (int i = 0; i < sides; i++)
        {
            var nextIndexInner = (i + 1) % sides;
            var startInner = center + new Vector2(innerRadius, 0f).Rotate(rotRadInner + angleStep * i);
            var endInner = center + new Vector2(innerRadius, 0f).Rotate(rotRadInner + angleStep * nextIndexInner);
            
            SegmentDrawing.DrawSegment(startInner, endInner, lineInfo, sideScaleFactor, sideScaleOrigin);
            
            var nextIndexOuter = (i + 1) % sides;
            var startOuter = center + new Vector2(outerRadius, 0f).Rotate(rotRadOuter + angleStep * i);
            var endOuter = center + new Vector2(outerRadius, 0f).Rotate(rotRadOuter + angleStep * nextIndexOuter);
            
            SegmentDrawing.DrawSegment(startOuter, endOuter, lineInfo, sideScaleFactor, sideScaleOrigin);
            
        }
    }
    
    public static void DrawRingLinesScaled(Vector2 center, float innerRadius, float outerRadius, float innerRotDeg, float outerRotDeg,  
        LineDrawingInfo innerLineInfo, LineDrawingInfo outerLineInfo, float smoothness, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0f) return;
        if (sideScaleFactor >= 1f)
        {
            var circle = new Circle(center, innerRadius);
            circle.DrawLines(innerRotDeg, innerLineInfo, smoothness);
            circle = circle.SetRadius(outerRadius);
            circle.DrawLines(outerRotDeg, outerLineInfo, smoothness);
            return;
        }

        if (!CircleDrawing.CalculateCircleDrawingParameters(outerRadius, smoothness, out float angleStep, out int sides)) return;
        
        var rotRadInner = innerRotDeg * ShapeMath.DEGTORAD;
        var rotRadOuter = outerRotDeg * ShapeMath.DEGTORAD;
        
        for (int i = 0; i < sides; i++)
        {
            var nextIndexInner = (i + 1) % sides;
            var startInner = center + new Vector2(innerRadius, 0f).Rotate(rotRadInner + angleStep * i);
            var endInner = center + new Vector2(innerRadius, 0f).Rotate(rotRadInner + angleStep * nextIndexInner);
            
            SegmentDrawing.DrawSegment(startInner, endInner, innerLineInfo, sideScaleFactor, sideScaleOrigin);
            
            var nextIndexOuter = (i + 1) % sides;
            var startOuter = center + new Vector2(outerRadius, 0f).Rotate(rotRadOuter + angleStep * i);
            var endOuter = center + new Vector2(outerRadius, 0f).Rotate(rotRadOuter + angleStep * nextIndexOuter);
            
            SegmentDrawing.DrawSegment(startOuter, endOuter, outerLineInfo, sideScaleFactor, sideScaleOrigin);
            
        }
    }
    
    public static void DrawRingLinesScaled(Vector2 center, float innerRadius, float outerRadius, float innerRotDeg, float outerRotDeg, 
        LineDrawingInfo innerLineInfo, LineDrawingInfo outerLineInfo, float smoothness, float innerSideScaleFactor, float outerSideScaleFactor, float innerSideScaleOrigin, float outerSideScaleOrigin)
    {
        bool drawInner = true;
        bool drawOuter = true;
        if (innerSideScaleFactor >= 1f)
        {
            drawInner = false;
            var circle = new Circle(center, innerRadius);
            circle.DrawLines(innerRotDeg, innerLineInfo, smoothness);
        }
        if (outerSideScaleFactor >= 1f)
        {
            drawOuter = false;
            var circle = new Circle(center, innerRadius);
            circle.DrawLines(outerRotDeg, outerLineInfo, smoothness);
        }

        if (!drawInner && !drawOuter) return;
        

        if (!CircleDrawing.CalculateCircleDrawingParameters(innerRadius, smoothness, out float innerAngleStep, out int innerSides)) return;
        if (!CircleDrawing.CalculateCircleDrawingParameters(outerRadius, smoothness, out float outerAngleStep, out int outerSides)) return;
        
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
                SegmentDrawing.DrawSegment(startInner, endInner, innerLineInfo, innerSideScaleFactor, innerSideScaleOrigin);
            }
            
            if (drawOuter && i < outerSides)
            {
                var nextIndexOuter = (i + 1) % outerSides;
                var startOuter = center + new Vector2(outerRadius, 0f).Rotate(outerRotRad + outerAngleStep * i);
                var endOuter = center + new Vector2(outerRadius, 0f).Rotate(outerRotRad + outerAngleStep * nextIndexOuter);
                SegmentDrawing.DrawSegment(startOuter, endOuter, outerLineInfo, outerSideScaleFactor, outerSideScaleOrigin);
            }
            
        }
    }
    #endregion
    
    //TODO: Use Raylib.DrawRing (if circle drawing uses Raylib.DrawRing internally nothing needs to change here!)
    #region Draw Sector Ring Lines
    /// <summary>
    /// Draws the outlines of a sector ring (arc-shaped ring) with specified line thickness and color.
    /// </summary>
    /// <param name="center">The center position of the sector ring.</param>
    /// <param name="innerRadius">The radius of the inner arc.</param>
    /// <param name="outerRadius">The radius of the outer arc.</param>
    /// <param name="startAngleDeg">The starting angle of the sector in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the sector in degrees.</param>
    /// <param name="lineThickness">The thickness of the ring lines.</param>
    /// <param name="color">The color of the ring lines.</param>
    /// <param name="sideLength">The length of each side segment. Default is 8.</param>
    /// <remarks>
    /// This method also draws the connecting lines between the inner and outer arcs at the start and end angles.
    /// </remarks>
    public static void DrawSectorRingLines(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, LineDrawingInfo lineInfo, float smoothness)
    {
        if (innerRadius <= 0 && outerRadius <= 0) return;
        if (innerRadius <= 0)
        {
            var circle = new Circle(center, outerRadius);
            circle.DrawSectorLines(startAngleDeg, endAngleDeg, lineInfo, smoothness);
            return;
        }

        if (outerRadius <= 0)
        {
            var circle = new Circle(center, innerRadius);
            circle.DrawSectorLines(startAngleDeg, endAngleDeg, lineInfo, smoothness);
            return;
        }
        
        float angleDifDeg = endAngleDeg - startAngleDeg;
        float angleDifDegAbs = MathF.Abs(angleDifDeg);
        if (angleDifDegAbs < 0.0001f) return;
        
        if (angleDifDegAbs >= 360f)
        {
            var circle = new Circle(center, innerRadius);
            circle.DrawLines(startAngleDeg, lineInfo, smoothness);
            circle = circle.SetRadius(outerRadius);
            circle.DrawLines(startAngleDeg, lineInfo, smoothness);
            return;
        }

        float lineThickness = lineInfo.Thickness;
        var color = lineInfo.Color;
        
        float arcLength = Circle.ArcLengthFromAngle((360 - angleDifDegAbs) * ShapeMath.DEGTORAD, innerRadius - lineThickness);
        if (arcLength < lineThickness * 3)
        {
            var circle = new Circle(center, innerRadius);
            circle.DrawLines(startAngleDeg, lineInfo, smoothness);
            circle = circle.SetRadius(outerRadius);
            circle.DrawLines(startAngleDeg, lineInfo, smoothness);
            return;
        }
        
        var c = new Circle(center, innerRadius);
        c.DrawSectorLines(startAngleDeg, endAngleDeg, lineInfo, smoothness);
        c = c.SetRadius(outerRadius);
        c.DrawSectorLines(startAngleDeg, endAngleDeg, lineInfo, smoothness);
        
        float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
        float endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
        var startDir = ShapeVec.Right().Rotate(startAngleRad);
        var startPerp = angleDifDeg < 0 ? startDir.GetPerpendicularRight() : startDir.GetPerpendicularLeft();
        var innerStart = center + startDir * (innerRadius - lineThickness);
        var outerStart = center + startDir * (outerRadius + lineThickness);
        var innerStartExtended = innerStart + startPerp * lineThickness * 2;
        var outerStartExtended = outerStart + startPerp * lineThickness * 2;
        if (angleDifDeg < 0)
        {
            TriangleDrawing.DrawTriangle(outerStart, innerStart, innerStartExtended, color);
            TriangleDrawing.DrawTriangle(outerStart, innerStartExtended, outerStartExtended, color);
        }
        else
        {
            TriangleDrawing.DrawTriangle(innerStart, outerStart, innerStartExtended, color);
            TriangleDrawing.DrawTriangle(innerStartExtended, outerStart, outerStartExtended, color);
        }
        

        var endDir = ShapeVec.Right().Rotate(endAngleRad);
        var endPerp = angleDifDeg < 0 ? endDir.GetPerpendicularLeft() : endDir.GetPerpendicularRight();
        var innerEnd = center + endDir * (innerRadius - lineThickness);
        var outerEnd = center + endDir * (outerRadius + lineThickness);
        var innerEndExtended = innerEnd + endPerp * lineThickness * 2;
        var outerEndExtended = outerEnd + endPerp * lineThickness * 2;
        if (angleDifDeg < 0)
        {
            TriangleDrawing.DrawTriangle(outerEndExtended, innerEnd, outerEnd, color);
            TriangleDrawing.DrawTriangle(outerEndExtended, innerEndExtended, innerEnd, color);
        }
        else
        {
            TriangleDrawing.DrawTriangle(innerEnd, outerEndExtended, outerEnd, color);
            TriangleDrawing.DrawTriangle(innerEndExtended, outerEndExtended, innerEnd, color);
        }
        
    }
    
    /// <summary>
    /// Draws the outlines of a sector ring using line drawing information and an additional rotation offset.
    /// </summary>
    /// <param name="center">The center position of the sector ring.</param>
    /// <param name="innerRadius">The radius of the inner arc.</param>
    /// <param name="outerRadius">The radius of the outer arc.</param>
    /// <param name="startAngleDeg">The starting angle of the sector in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the sector in degrees.</param>
    /// <param name="rotOffsetDeg">The rotation offset in degrees.</param>
    /// <param name="lineInfo">The line drawing style information.</param>
    /// <param name="sideLength">The length of each side segment. Default is 8.</param>
    public static void DrawSectorRingLines(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, LineDrawingInfo lineInfo, float smoothness)
    {
        DrawSectorRingLines(center, innerRadius, outerRadius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, lineInfo, smoothness);
    }
    #endregion
    
    #region Draw Sector Ring
    /// <summary>
    /// Draws a filled sector ring (arc-shaped ring) with a specified color.
    /// </summary>
    /// <param name="center">The center position of the sector ring.</param>
    /// <param name="innerRadius">The radius of the inner arc.</param>
    /// <param name="outerRadius">The radius of the outer arc.</param>
    /// <param name="startAngleDeg">The starting angle of the sector in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the sector in degrees.</param>
    /// <param name="color">The color of the sector ring.</param>
    /// <param name="sideLength">The length of each side segment. Default is 10.</param>
    public static void DrawSectorRing(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, ColorRgba color, float smoothness)
    {
        //TODO: add angle fix
        if (!CircleDrawing.CalculateCircleDrawingParameters(outerRadius, startAngleDeg, endAngleDeg, smoothness, out int sides)) return;
        Raylib.DrawRing(center, innerRadius, outerRadius, startAngleDeg, endAngleDeg, sides, color.ToRayColor());
    }

    /// <summary>
    /// Draws a filled sector ring (arc-shaped ring) with a specified color and an additional rotation offset.
    /// </summary>
    /// <param name="center">The center position of the sector ring.</param>
    /// <param name="innerRadius">The radius of the inner arc.</param>
    /// <param name="outerRadius">The radius of the outer arc.</param>
    /// <param name="startAngleDeg">The starting angle of the sector in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the sector in degrees.</param>
    /// <param name="rotOffsetDeg">The rotation offset in degrees.</param>
    /// <param name="color">The color of the sector ring.</param>
    /// <param name="sideLength">The length of each side segment. Default is 10.</param>
    public static void DrawSectorRing(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, ColorRgba color, float smoothness)
    {
        DrawSectorRing(center, innerRadius, outerRadius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, color, smoothness);
    }
    #endregion
}