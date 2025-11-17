using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.CircleDef;

/// <summary>
/// Provides static methods for drawing ring shapes and ring outlines with various customization options.
/// </summary>
/// <remarks>
/// This class contains utility methods for drawing rings, sector rings, and their outlines with support for line thickness, color, rotation, scaling, and side count.
/// </remarks>
public static class RingDrawing
{
    //TODO: Look at all line drawing functions and how to optimize without using cap types that are expensive.
    
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
    public static void DrawRing(Vector2 center, float innerRadius, float outerRadius, ColorRgba color, float sideLength = 8f)
    {
        DrawSectorRing(center, innerRadius, outerRadius, 0, 360, color, sideLength);
    }

    /// <summary>
    /// Draws a filled ring shape with a specified number of sides.
    /// </summary>
    /// <param name="center">The center position of the ring.</param>
    /// <param name="innerRadius">The radius of the inner circle.</param>
    /// <param name="outerRadius">The radius of the outer circle.</param>
    /// <param name="sides">Number of sides for the ring.</param>
    /// <param name="color">The color of the ring.</param>
    /// <remarks>
    /// This draws a complete ring (360 degrees).
    /// </remarks>
    public static void DrawRing(Vector2 center, float innerRadius, float outerRadius, int sides, ColorRgba color)
    {
        DrawSectorRing(center, innerRadius, outerRadius, 0, 360, sides, color);
    }
    #endregion
    
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
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float lineThickness, ColorRgba color, float sideLength = 8f)
    {
        CircleDrawing.DrawCircleLines(center, innerRadius, lineThickness, color, sideLength);
        CircleDrawing.DrawCircleLines(center, outerRadius, lineThickness, color, sideLength);
    }

    /// <summary>
    /// Draws the outlines of a ring using line drawing information for style.
    /// </summary>
    /// <param name="center">The center position of the ring.</param>
    /// <param name="innerRadius">The radius of the inner circle.</param>
    /// <param name="outerRadius">The radius of the outer circle.</param>
    /// <param name="lineInfo">The line drawing style information.</param>
    /// <param name="sideLength">The length of each side segment. Default is 8.</param>
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, LineDrawingInfo lineInfo, float sideLength = 8f)
    {
        CircleDrawing.DrawCircleLines(center, innerRadius, lineInfo, 0f, sideLength);
        CircleDrawing.DrawCircleLines(center, outerRadius, lineInfo, 0f, sideLength);
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
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float innerRotDeg, float outerRotDeg, LineDrawingInfo lineInfo, float sideLength = 8f)
    {
        CircleDrawing.DrawCircleLines(center, innerRadius, lineInfo, innerRotDeg, sideLength);
        CircleDrawing.DrawCircleLines(center, outerRadius, lineInfo, outerRotDeg, sideLength);
    }
   
    /// <summary>
    /// Draws the outlines of a ring with separate rotation and side length for inner and outer circles.
    /// </summary>
    /// <param name="center">The center position of the ring.</param>
    /// <param name="innerRadius">The radius of the inner circle.</param>
    /// <param name="outerRadius">The radius of the outer circle.</param>
    /// <param name="innerRotDeg">Rotation of the inner circle in degrees.</param>
    /// <param name="outerRotDeg">Rotation of the outer circle in degrees.</param>
    /// <param name="innerSideLength">Side length for the inner circle.</param>
    /// <param name="outerSideLength">Side length for the outer circle.</param>
    /// <param name="lineInfo">The line drawing style information.</param>
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float innerRotDeg, float outerRotDeg, float innerSideLength, float outerSideLength, LineDrawingInfo lineInfo)
    {
        CircleDrawing.DrawCircleLines(center, innerRadius, lineInfo, innerRotDeg, innerSideLength);
        CircleDrawing.DrawCircleLines(center, outerRadius, lineInfo, outerRotDeg, outerSideLength);
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
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float innerRotDeg, float outerRotDeg, float innerSideLength, float outerSideLength, LineDrawingInfo innerLineInfo, LineDrawingInfo outerLineInfo)
    {
        CircleDrawing.DrawCircleLines(center, innerRadius, innerLineInfo, innerRotDeg, innerSideLength);
        CircleDrawing.DrawCircleLines(center, outerRadius, outerLineInfo, outerRotDeg, outerSideLength);
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
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float f, float innerRotDeg, float outerRotDeg, float innerSideLength, float outerSideLength, LineDrawingInfo innerLineInfo, LineDrawingInfo outerLineInfo)
    {
        CircleDrawing.DrawCircleLinesPercentage(center, outerRadius, f, outerLineInfo, outerRotDeg, outerSideLength);
        CircleDrawing.DrawCircleLinesPercentage(center, innerRadius, f, innerLineInfo, innerRotDeg, innerSideLength);
    }

    /// <summary>
    /// Draws the outlines of a ring with a specified number of sides and rotation for inner and outer circles.
    /// </summary>
    /// <param name="center">The center position of the ring.</param>
    /// <param name="innerRadius">The radius of the inner circle.</param>
    /// <param name="outerRadius">The radius of the outer circle.</param>
    /// <param name="innerRotDeg">Rotation of the inner circle in degrees.</param>
    /// <param name="outerRotDeg">Rotation of the outer circle in degrees.</param>
    /// <param name="sides">Number of sides for both circles.</param>
    /// <param name="lineInfo">The line drawing style information.</param>
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float innerRotDeg, float outerRotDeg, int sides, LineDrawingInfo lineInfo)
    {
        CircleDrawing.DrawCircleLines(center, innerRadius, lineInfo, innerRotDeg, sides);
        CircleDrawing.DrawCircleLines(center, outerRadius, lineInfo, outerRotDeg, sides);
    }

    /// <summary>
    /// Draws the outlines of a ring with separate side counts and rotation for inner and outer circles.
    /// </summary>
    /// <param name="center">The center position of the ring.</param>
    /// <param name="innerRadius">The radius of the inner circle.</param>
    /// <param name="outerRadius">The radius of the outer circle.</param>
    /// <param name="innerRotDeg">Rotation of the inner circle in degrees.</param>
    /// <param name="outerRotDeg">Rotation of the outer circle in degrees.</param>
    /// <param name="innerSides">Number of sides for the inner circle.</param>
    /// <param name="outerSides">Number of sides for the outer circle.</param>
    /// <param name="lineInfo">The line drawing style information.</param>
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float innerRotDeg, float outerRotDeg, int innerSides, int outerSides, LineDrawingInfo lineInfo)
    {
        CircleDrawing.DrawCircleLines(center, innerRadius, lineInfo, innerRotDeg, innerSides);
        CircleDrawing.DrawCircleLines(center, outerRadius, lineInfo, outerRotDeg, outerSides);
    }

    /// <summary>
    /// Draws the outlines of a ring with separate side counts, rotation, and line info for inner and outer circles.
    /// </summary>
    /// <param name="center">The center position of the ring.</param>
    /// <param name="innerRadius">The radius of the inner circle.</param>
    /// <param name="outerRadius">The radius of the outer circle.</param>
    /// <param name="innerRotDeg">Rotation of the inner circle in degrees.</param>
    /// <param name="outerRotDeg">Rotation of the outer circle in degrees.</param>
    /// <param name="innerSides">Number of sides for the inner circle.</param>
    /// <param name="outerSides">Number of sides for the outer circle.</param>
    /// <param name="innerLineInfo">Line drawing info for the inner circle.</param>
    /// <param name="outerLineInfo">Line drawing info for the outer circle.</param>
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float innerRotDeg, float outerRotDeg, int innerSides, int outerSides, LineDrawingInfo innerLineInfo, LineDrawingInfo outerLineInfo)
    {
        CircleDrawing.DrawCircleLines(center, innerRadius, innerLineInfo, innerRotDeg, innerSides);
        CircleDrawing.DrawCircleLines(center, outerRadius, outerLineInfo, outerRotDeg, outerSides);
    }
    /// <summary>
    /// Draws the outlines of a ring with a specified number of sides for both circles.
    /// </summary>
    /// <param name="center">The center position of the ring.</param>
    /// <param name="innerRadius">The radius of the inner circle.</param>
    /// <param name="outerRadius">The radius of the outer circle.</param>
    /// <param name="sides">Number of sides for both circles.</param>
    /// <param name="lineInfo">The line drawing style information.</param>
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, int sides, LineDrawingInfo lineInfo)
    {
        CircleDrawing.DrawCircleLines(center, innerRadius, lineInfo, sides);
        CircleDrawing.DrawCircleLines(center, outerRadius, lineInfo, sides);
    }

    /// <summary>
    /// Draws the outlines of a ring with separate side counts for inner and outer circles.
    /// </summary>
    /// <param name="center">The center position of the ring.</param>
    /// <param name="innerRadius">The radius of the inner circle.</param>
    /// <param name="outerRadius">The radius of the outer circle.</param>
    /// <param name="innerSides">Number of sides for the inner circle.</param>
    /// <param name="outerSides">Number of sides for the outer circle.</param>
    /// <param name="lineInfo">The line drawing style information.</param>
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, int innerSides, int outerSides, LineDrawingInfo lineInfo)
    {
        CircleDrawing.DrawCircleLines(center, innerRadius, lineInfo, innerSides);
        CircleDrawing.DrawCircleLines(center, outerRadius, lineInfo, outerSides);
    }

    /// <summary>
    /// Draws the outlines of a ring with separate side counts and line info for inner and outer circles.
    /// </summary>
    /// <param name="center">The center position of the ring.</param>
    /// <param name="innerRadius">The radius of the inner circle.</param>
    /// <param name="outerRadius">The radius of the outer circle.</param>
    /// <param name="innerSides">Number of sides for the inner circle.</param>
    /// <param name="outerSides">Number of sides for the outer circle.</param>
    /// <param name="innerLineInfo">Line drawing info for the inner circle.</param>
    /// <param name="outerLineInfo">Line drawing info for the outer circle.</param>
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, int innerSides, int outerSides, LineDrawingInfo innerLineInfo, LineDrawingInfo outerLineInfo)
    {
        CircleDrawing.DrawCircleLines(center, innerRadius, innerLineInfo, innerSides);
        CircleDrawing.DrawCircleLines(center, outerRadius, outerLineInfo, outerSides);
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
    public static void DrawRingLinesPercentage(Vector2 center, float innerRadius, float outerRadius, float f, float innerRotDeg, float outerRotDeg, LineDrawingInfo lineInfo, float sideLength = 8f)
    {
        CircleDrawing.DrawCircleLinesPercentage(center, innerRadius, f, lineInfo, innerRotDeg, sideLength);
        CircleDrawing.DrawCircleLinesPercentage(center, outerRadius, f, lineInfo, outerRotDeg, sideLength);
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
    public static void DrawRingLinesPercentage(Vector2 center, float innerRadius, float outerRadius, float f, float innerRotDeg, float outerRotDeg, float innerSideLength, float outerSideLength, LineDrawingInfo lineInfo)
    {
        CircleDrawing.DrawCircleLinesPercentage(center, innerRadius, f, lineInfo, innerRotDeg, innerSideLength);
        CircleDrawing.DrawCircleLinesPercentage(center, outerRadius, f, lineInfo, outerRotDeg, outerSideLength);
    }
    
    /// <summary>
    /// Draws a percentage of the outlines of a ring with separate side counts and rotation for inner and outer circles.
    /// </summary>
    /// <param name="center">The center position of the ring.</param>
    /// <param name="innerRadius">The radius of the inner circle.</param>
    /// <param name="outerRadius">The radius of the outer circle.</param>
    /// <param name="f">The percentage of the ring to draw (0 to 1).</param>
    /// <param name="innerRotDeg">Rotation of the inner circle in degrees.</param>
    /// <param name="outerRotDeg">Rotation of the outer circle in degrees.</param>
    /// <param name="innerSides">Number of sides for the inner circle.</param>
    /// <param name="outerSides">Number of sides for the outer circle.</param>
    /// <param name="lineInfo">The line drawing style information.</param>
    public static void DrawRingLinesPercentage(Vector2 center, float innerRadius, float outerRadius, float f, float innerRotDeg, float outerRotDeg, int innerSides, int outerSides, LineDrawingInfo lineInfo)
    {
        CircleDrawing.DrawCircleLinesPercentage(center, innerRadius, f, lineInfo, innerRotDeg, innerSides);
        CircleDrawing.DrawCircleLinesPercentage(center, outerRadius, f, lineInfo, outerRotDeg, outerSides);
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
    public static void DrawRingLinesPercentage(Vector2 center, float innerRadius, float outerRadius, float f, float innerRotDeg, float outerRotDeg, int innerSides, int outerSides, LineDrawingInfo innerLineInfo, LineDrawingInfo outerLineInfo)
    {
        CircleDrawing.DrawCircleLinesPercentage(center, innerRadius, f, innerLineInfo, innerRotDeg, innerSides);
        CircleDrawing.DrawCircleLinesPercentage(center, outerRadius, f, outerLineInfo, outerRotDeg, outerSides);
    }
    
    /// <summary>
    /// Draws a percentage of the outlines of a ring with separate side counts for inner and outer circles.
    /// </summary>
    /// <param name="center">The center position of the ring.</param>
    /// <param name="innerRadius">The radius of the inner circle.</param>
    /// <param name="outerRadius">The radius of the outer circle.</param>
    /// <param name="f">The percentage of the ring to draw (0 to 1).</param>
    /// <param name="innerSides">Number of sides for the inner circle.</param>
    /// <param name="outerSides">Number of sides for the outer circle.</param>
    /// <param name="lineInfo">The line drawing style information.</param>
    public static void DrawRingLinesPercentage(Vector2 center, float innerRadius, float outerRadius, float f, int innerSides, int outerSides, LineDrawingInfo lineInfo)
    {
        CircleDrawing.DrawCircleLinesPercentage(center, innerRadius, f, lineInfo, innerSides);
        CircleDrawing.DrawCircleLinesPercentage(center, outerRadius, f, lineInfo, outerSides);
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
    public static void DrawRingLinesScaled(Vector2 center, float innerRadius, float outerRadius, float innerRotDeg, float outerRotDeg, int sides, LineDrawingInfo lineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0f) return;
        if (sideScaleFactor >= 1f)
        {
            CircleDrawing.DrawCircleLines(center, innerRadius, lineInfo, innerRotDeg, sides);
            CircleDrawing.DrawCircleLines(center, outerRadius, lineInfo, outerRotDeg, sides);
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
            
            SegmentDrawing.DrawSegment(startInner, endInner, lineInfo, sideScaleFactor, sideScaleOrigin);
            
            var nextIndexOuter = (i + 1) % sides;
            var startOuter = center + new Vector2(outerRadius, 0f).Rotate(rotRadOuter + angleStep * i);
            var endOuter = center + new Vector2(outerRadius, 0f).Rotate(rotRadOuter + angleStep * nextIndexOuter);
            
            SegmentDrawing.DrawSegment(startOuter, endOuter, lineInfo, sideScaleFactor, sideScaleOrigin);
            
        }
    }
    
    /// <summary>
    /// Draws a ring where each side can be scaled towards the origin of the side, with separate side counts for inner and outer circles.
    /// </summary>
    /// <param name="center">The center position of the ring.</param>
    /// <param name="innerRadius">The radius of the inner circle.</param>
    /// <param name="outerRadius">The radius of the outer circle.</param>
    /// <param name="innerRotDeg">Rotation of the inner circle in degrees.</param>
    /// <param name="outerRotDeg">Rotation of the outer circle in degrees.</param>
    /// <param name="innerSides">Number of sides for the inner circle.</param>
    /// <param name="outerSides">Number of sides for the outer circle.</param>
    /// <param name="lineInfo">How to draw the lines.</param>
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
    /// This method allows for creative effects such as dashed rings.
    /// </remarks>
    public static void DrawRingLinesScaled(Vector2 center, float innerRadius, float outerRadius, float innerRotDeg, float outerRotDeg, int innerSides, int outerSides, LineDrawingInfo lineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0f) return;
        if (sideScaleFactor >= 1f)
        {
            CircleDrawing.DrawCircleLines(center, innerRadius, lineInfo, innerRotDeg, innerSides);
            CircleDrawing.DrawCircleLines(center, outerRadius, lineInfo, outerRotDeg, outerSides);
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
                SegmentDrawing.DrawSegment(startInner, endInner, lineInfo, sideScaleFactor, sideScaleOrigin);
            }
            
            if (i < outerSides)
            {
                var nextIndexOuter = (i + 1) % outerSides;
                var startOuter = center + new Vector2(outerRadius, 0f).Rotate(outerRotRad + outerAngleStep * i);
                var endOuter = center + new Vector2(outerRadius, 0f).Rotate(outerRotRad + outerAngleStep * nextIndexOuter);
                SegmentDrawing.DrawSegment(startOuter, endOuter, lineInfo, sideScaleFactor, sideScaleOrigin);
            }
            
        }
    }

    /// <summary>
    /// Draws a ring where each side can be scaled towards the origin of the side, with separate line info for inner and outer circles.
    /// </summary>
    /// <param name="center">The center position of the ring.</param>
    /// <param name="innerRadius">The radius of the inner circle.</param>
    /// <param name="outerRadius">The radius of the outer circle.</param>
    /// <param name="innerRotDeg">Rotation of the inner circle in degrees.</param>
    /// <param name="outerRotDeg">Rotation of the outer circle in degrees.</param>
    /// <param name="innerSides">Number of sides for the inner circle.</param>
    /// <param name="outerSides">Number of sides for the outer circle.</param>
    /// <param name="innerLineInfo">How to draw the inner lines.</param>
    /// <param name="outerLineInfo">How to draw the outer lines.</param>
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
    /// This method allows for creative effects such as dashed rings.
    /// </remarks>
    public static void DrawRingLinesScaled(Vector2 center, float innerRadius, float outerRadius, float innerRotDeg, float outerRotDeg, int innerSides, int outerSides, LineDrawingInfo innerLineInfo, LineDrawingInfo outerLineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0f) return;
        if (sideScaleFactor >= 1f)
        {
            CircleDrawing.DrawCircleLines(center, innerRadius, innerLineInfo, innerRotDeg, innerSides);
            CircleDrawing.DrawCircleLines(center, outerRadius, outerLineInfo, outerRotDeg, outerSides);
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
                SegmentDrawing.DrawSegment(startInner, endInner, innerLineInfo, sideScaleFactor, sideScaleOrigin);
            }
            
            if (i < outerSides)
            {
                var nextIndexOuter = (i + 1) % outerSides;
                var startOuter = center + new Vector2(outerRadius, 0f).Rotate(outerRotRad + outerAngleStep * i);
                var endOuter = center + new Vector2(outerRadius, 0f).Rotate(outerRotRad + outerAngleStep * nextIndexOuter);
                SegmentDrawing.DrawSegment(startOuter, endOuter, outerLineInfo, sideScaleFactor, sideScaleOrigin);
            }
            
        }
    }
    
    /// <summary>
    /// Draws a ring where each side can be scaled towards the origin of the side, with separate scale factors and origins for inner and outer circles.
    /// </summary>
    /// <param name="center">The center position of the ring.</param>
    /// <param name="innerRadius">The radius of the inner circle.</param>
    /// <param name="outerRadius">The radius of the outer circle.</param>
    /// <param name="innerRotDeg">Rotation of the inner circle in degrees.</param>
    /// <param name="outerRotDeg">Rotation of the outer circle in degrees.</param>
    /// <param name="innerSides">Number of sides for the inner circle.</param>
    /// <param name="outerSides">Number of sides for the outer circle.</param>
    /// <param name="innerLineInfo">How to draw the inner lines.</param>
    /// <param name="outerLineInfo">How to draw the outer lines.</param>
    /// <param name="innerSideScaleFactor">
    /// <para>The scale factor for each side on the inner circle.</para>
    /// <list type="bullet">
    /// <item><description>0: No inner circle is drawn.</description></item>
    /// <item><description>1: The normal inner circle is drawn.</description></item>
    /// <item><description>0.5: Each side on the inner ring is half as long.</description></item>
    /// </list>
    /// </param>
    /// <param name="innerSideScaleOrigin">
    /// The point along the line to scale from, in both directions (0 to 1).
    /// <list type="bullet">
    /// <item><description>0: Start of Segment</description></item>
    /// <item><description>0.5: Center of Segment</description></item>
    /// <item><description>1: End of Segment</description></item>
    /// </list>
    /// </param>
    /// <param name="outerSideScaleFactor">
    /// <para>The scale factor for each side on the outer circle.</para>
    /// <list type="bullet">
    /// <item><description>0: No outer circle is drawn.</description></item>
    /// <item><description>1: The normal outer circle is drawn.</description></item>
    /// <item><description>0.5: Each side on the outer circle is half as long.</description></item>
    /// </list>
    /// </param>
    /// <param name="outerSideScaleOrigin">
    /// The point along the line to scale from, in both directions (0 to 1).
    /// <list type="bullet">
    /// <item><description>0: Start of Segment</description></item>
    /// <item><description>0.5: Center of Segment</description></item>
    /// <item><description>1: End of Segment</description></item>
    /// </list>
    /// </param>
    /// <remarks>
    /// This method allows for creative effects such as dashed rings,
    /// with independent control for inner and outer circles.
    /// </remarks>
    public static void DrawRingLinesScaled(Vector2 center, float innerRadius, float outerRadius, float innerRotDeg, float outerRotDeg, int innerSides, int outerSides, LineDrawingInfo innerLineInfo, LineDrawingInfo outerLineInfo, float innerSideScaleFactor, float outerSideScaleFactor, float innerSideScaleOrigin, float outerSideScaleOrigin)
    {
        bool drawInner = true;
        bool drawOuter = true;
        if (innerSideScaleFactor >= 1f)
        {
            drawInner = false;
            CircleDrawing.DrawCircleLines(center, innerRadius, innerLineInfo, innerRotDeg, innerSides);
        }
        if (outerSideScaleFactor >= 1f)
        {
            drawOuter = false;
            CircleDrawing.DrawCircleLines(center, outerRadius, outerLineInfo, outerRotDeg, outerSides);
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
    public static void DrawSectorRingLines(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float lineThickness, ColorRgba color, float sideLength = 8f)
    {
        CircleDrawing.DrawCircleSectorLines(center, innerRadius, startAngleDeg, endAngleDeg, lineThickness, color, false, sideLength);
        CircleDrawing.DrawCircleSectorLines(center, outerRadius, startAngleDeg, endAngleDeg, lineThickness, color, false, sideLength);

        //TODO: Fix
        float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
        float endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
        var innerStart = center + (ShapeVec.Right() * innerRadius).Rotate(startAngleRad);
        var outerStart = center + (ShapeVec.Right() * outerRadius).Rotate(startAngleRad);
        SegmentDrawing.DrawSegment(innerStart, outerStart, lineThickness, color, LineCapType.CappedExtended, 4);

        var innerEnd = center + (ShapeVec.Right() * innerRadius).Rotate(endAngleRad);
        var outerEnd = center + (ShapeVec.Right() * outerRadius).Rotate(endAngleRad);
        SegmentDrawing.DrawSegment(innerEnd, outerEnd, lineThickness, color, LineCapType.CappedExtended, 4);
    }

    /// <summary>
    /// Draws the outlines of a sector ring with an additional rotation offset.
    /// </summary>
    /// <param name="center">The center position of the sector ring.</param>
    /// <param name="innerRadius">The radius of the inner arc.</param>
    /// <param name="outerRadius">The radius of the outer arc.</param>
    /// <param name="startAngleDeg">The starting angle of the sector in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the sector in degrees.</param>
    /// <param name="rotOffsetDeg">The rotation offset in degrees.</param>
    /// <param name="lineThickness">The thickness of the ring lines.</param>
    /// <param name="color">The color of the ring lines.</param>
    /// <param name="sideLength">The length of each side segment. Default is 8.</param>
    public static void DrawSectorRingLines(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, float lineThickness, ColorRgba color, float sideLength = 8f)
    {
        DrawSectorRingLines(center, innerRadius, outerRadius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, lineThickness, color, sideLength);
    }

    /// <summary>
    /// Draws the outlines of a sector ring using line drawing information for style.
    /// </summary>
    /// <param name="center">The center position of the sector ring.</param>
    /// <param name="innerRadius">The radius of the inner arc.</param>
    /// <param name="outerRadius">The radius of the outer arc.</param>
    /// <param name="startAngleDeg">The starting angle of the sector in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the sector in degrees.</param>
    /// <param name="lineInfo">The line drawing style information.</param>
    /// <param name="sideLength">The length of each side segment. Default is 8.</param>
    public static void DrawSectorRingLines(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, LineDrawingInfo lineInfo, float sideLength = 8f)
    {
        CircleDrawing.DrawCircleSectorLines(center, innerRadius, startAngleDeg, endAngleDeg, lineInfo, false, sideLength);
        CircleDrawing.DrawCircleSectorLines(center, outerRadius, startAngleDeg, endAngleDeg, lineInfo, false, sideLength);

        //TODO: Fix
        float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
        float endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
        var innerStart = center + (ShapeVec.Right() * innerRadius - new Vector2(lineInfo.Thickness / 2, 0)).Rotate(startAngleRad);
        var outerStart = center + (ShapeVec.Right() * outerRadius + new Vector2(lineInfo.Thickness / 2, 0)).Rotate(startAngleRad);
        SegmentDrawing.DrawSegment(innerStart, outerStart, lineInfo);

        var innerEnd = center + (ShapeVec.Right() * innerRadius - new Vector2(lineInfo.Thickness / 2, 0)).Rotate(endAngleRad);
        var outerEnd = center + (ShapeVec.Right() * outerRadius + new Vector2(lineInfo.Thickness / 2, 0)).Rotate(endAngleRad);
        SegmentDrawing.DrawSegment(innerEnd, outerEnd, lineInfo);
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
    public static void DrawSectorRingLines(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, LineDrawingInfo lineInfo, float sideLength = 8f)
    {
        DrawSectorRingLines(center, innerRadius, outerRadius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, lineInfo, sideLength);
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
    public static void DrawSectorRing(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, ColorRgba color, float sideLength = 10f)
    {
        float anglePiece = endAngleDeg - startAngleDeg;
        int sides = CircleDrawing.GetCircleArcSideCount(outerRadius, MathF.Abs(anglePiece), sideLength);
        Raylib.DrawRing(center, innerRadius, outerRadius, startAngleDeg, endAngleDeg, sides, color.ToRayColor());
    }

    /// <summary>
    /// Draws a filled sector ring (arc-shaped ring) with a specified number of sides and color.
    /// </summary>
    /// <param name="center">The center position of the sector ring.</param>
    /// <param name="innerRadius">The radius of the inner arc.</param>
    /// <param name="outerRadius">The radius of the outer arc.</param>
    /// <param name="startAngleDeg">The starting angle of the sector in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the sector in degrees.</param>
    /// <param name="sides">Number of sides for the sector ring.</param>
    /// <param name="color">The color of the sector ring.</param>
    public static void DrawSectorRing(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, int sides, ColorRgba color)
    {
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
    public static void DrawSectorRing(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, ColorRgba color, float sideLength = 10f)
    {
        DrawSectorRing(center, innerRadius, outerRadius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, color, sideLength);
    }

    /// <summary>
    /// Draws a filled sector ring (arc-shaped ring) with a specified number of sides, color, and an additional rotation offset.
    /// </summary>
    /// <param name="center">The center position of the sector ring.</param>
    /// <param name="innerRadius">The radius of the inner arc.</param>
    /// <param name="outerRadius">The radius of the outer arc.</param>
    /// <param name="startAngleDeg">The starting angle of the sector in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the sector in degrees.</param>
    /// <param name="rotOffsetDeg">The rotation offset in degrees.</param>
    /// <param name="sides">Number of sides for the sector ring.</param>
    /// <param name="color">The color of the sector ring.</param>
    public static void DrawSectorRing(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, int sides, ColorRgba color)
    {
        DrawSectorRing(center, innerRadius, outerRadius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, sides, color);
    }
    #endregion
}