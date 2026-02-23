using System.Drawing;
using System.Numerics;
using Clipper2Lib;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.CircleDef;

//TODO:
// - Upate all docs

/// <summary>
/// Provides static methods for drawing circles and circle-related shapes with various options,
/// including filled circles, outlines, sectors, and advanced line drawing with scaling and percentage.
/// </summary>
/// <remarks>
/// This class is intended for use with Raylib and ShapeEngine types.
/// It offers both simple and advanced
/// circle drawing utilities, including performance-optimized methods for small circles.
/// </remarks>
public static class CircleDrawing
{
    #region Draw

    /// <summary>
    /// Draws a filled circle using the specified <see cref="Circle"/> instance, color, rotation, and segment count.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="color">The color of the circle.</param>
    /// <param name="rotDeg">The rotation of the circle in degrees.</param>
    /// <param name="segments">The number of segments used to approximate the circle. Minimum is 3.</param>
    public static void Draw(this Circle c, float rotDeg, ColorRgba color, float smoothness = 0.5f)
    {
        if (!CalculateCircleDrawingParameters(c.Radius, smoothness, out int sides)) return;
        Raylib.DrawCircleSector(c.Center, c.Radius, rotDeg, 360 + rotDeg, sides, color.ToRayColor());
    }

    /// <summary>
    /// Draws a filled circle using the specified <see cref="Circle"/> instance and color.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="color">The color of the circle.</param>
    public static void Draw(this Circle c, ColorRgba color, float smoothness = 0.5f)
    {
        if (!CalculateCircleDrawingParameters(c.Radius, smoothness, out int sides)) return;
        Raylib.DrawCircleSector(c.Center, c.Radius, 0, 360, sides, color.ToRayColor());
    }
    
    /// <summary>
    /// Very usefull for drawing small/tiny circles. Drawing the circle as rect increases performance a lot.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="color">The color of the circle.</param>
    /// <remarks>
    /// This method is optimized for performance and is useful for drawing tiny circles. Draws a rect!
    /// </remarks>
    public static void DrawFast(this Circle c, ColorRgba color)
    {
        RectDrawing.DrawRect(c.Center - new Vector2(c.Radius, c.Radius), c.Center + new Vector2(c.Radius, c.Radius), color);
    }

    #endregion
    
    #region Draw Scaled

    /// <summary>
    /// Draws a filled circle with each side scaled towards the origin of the side.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="rotDeg">Rotation of the circle in degrees.</param>
    /// <param name="color">Color of the circle.</param>
    /// <param name="smoothness">Controls the number of sides used to approximate the circle.</param>
    /// <param name="sideScaleFactor">Scale factor for each side (0 = no circle, 1 = normal circle).</param>
    /// <param name="sideScaleOrigin">Point along each segment to scale from (0-1, default 0.5).</param>
    public static void DrawScaled(this Circle c, float rotDeg, ColorRgba color, float smoothness, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (c.Radius <= 0f || sideScaleFactor <= 0f) return;
        if (sideScaleFactor >= 1f)
        {
            c.Draw(color, smoothness);
            return;
        }
        
        if (!CalculateCircleDrawingParameters(c.Radius, smoothness, out float angleStep, out int sides)) return;
        
        var rotRad = rotDeg * ShapeMath.DEGTORAD;
        var rayColor = color.ToRayColor();
        for (int i = 0; i < sides; i++)
        {
            var nextIndex = (i + 1) % sides;
            var start = c.Center + new Vector2(c.Radius, 0f).Rotate(rotRad + angleStep * i);
            var end = c.Center + new Vector2(c.Radius, 0f).Rotate(rotRad + angleStep * nextIndex);

            var scaledSegment = new Segment(start, end).ScaleSegment(sideScaleFactor, sideScaleOrigin);
            
            Raylib.DrawTriangle(c.Center, scaledSegment.End, scaledSegment.Start, rayColor);
        }
    }
    #endregion
    
    #region Draw Percentage
    //TODO: Implement
    #endregion
    
    #region Draw Lines
    /// <summary>
    /// Draws the outline of a circle using the specified line thickness, number of sides, and color.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="sides">The number of sides used to approximate the circle.</param>
    /// <param name="color">The color of the outline.</param>
    public static void DrawLines(this Circle c, float lineThickness, ColorRgba color, float smoothness = 0.5f)
    {
        if (c.Radius < lineThickness)
        {
            var circle = c.SetRadius(lineThickness * 2); 
            circle.Draw(color, smoothness);
            return;
        }
        if (!CalculateCircleDrawingParameters(c.Radius, smoothness, out int sides)) return;
        Raylib.DrawPolyLinesEx(c.Center, sides, c.Radius + lineThickness, 0f, lineThickness * 2, color.ToRayColor());
    }
    /// <summary>
    /// Draws the outline of a circle using the specified line thickness, rotation, number of sides, and color.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="rotDeg">The rotation of the circle in degrees.</param>
    /// <param name="sides">The number of sides used to approximate the circle.</param>
    /// <param name="color">The color of the outline.</param>
    public static void DrawLines(this Circle c, float rotDeg, float lineThickness, ColorRgba color, float smoothness = 0.5f)
    {
        if (c.Radius < lineThickness)
        {
            var circle = c.SetRadius(lineThickness * 2); 
            circle.Draw(color, smoothness);
            return;
        }
        if (!CalculateCircleDrawingParameters(c.Radius, smoothness, out int sides)) return;
        Raylib.DrawPolyLinesEx(c.Center, sides, c.Radius + lineThickness, rotDeg, lineThickness * 2, color.ToRayColor());
    }
    /// <summary>
    /// Draws the outline of a circle using the specified line drawing info and number of sides.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="lineInfo">The line drawing parameters.</param>
    /// <param name="sides">The number of sides used to approximate the circle.</param>
    public static void DrawLines(this Circle c, LineDrawingInfo lineInfo, float smoothness = 0.5f)
    {
        if (c.Radius < lineInfo.Thickness)
        {
            var circle = c.SetRadius(lineInfo.Thickness * 2); 
            circle.Draw(lineInfo.Color, smoothness);
            return;
        }
        if (!CalculateCircleDrawingParameters(c.Radius, smoothness, out int sides)) return;
        var lineThickness = lineInfo.Thickness;
        Raylib.DrawPolyLinesEx(c.Center, sides, c.Radius + lineThickness, 0f, lineThickness * 2, lineInfo.Color.ToRayColor());
    }
    /// <summary>
    /// Draws the outline of a circle using the specified line drawing info, rotation, and number of sides.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="lineInfo">The line drawing parameters for the drawing the circle outline.
    /// Only <see cref="LineDrawingInfo.Thickness"/> and <see cref="LineDrawingInfo.Color"/> are used!</param>
    /// <param name="rotDeg">The rotation of the circle in degrees.</param>
    /// <param name="sides">The number of sides used to approximate the circle.</param>
    public static void DrawLines(this Circle c, float rotDeg, LineDrawingInfo lineInfo, float smoothness = 0.5f)
    {
        if (c.Radius < lineInfo.Thickness)
        {
            var circle = c.SetRadius(lineInfo.Thickness * 2); 
            circle.Draw(lineInfo.Color, smoothness);
            return;
        }
        if (!CalculateCircleDrawingParameters(c.Radius, smoothness, out int sides)) return;
        var lineThickness = lineInfo.Thickness;
        Raylib.DrawPolyLinesEx(c.Center, sides, c.Radius + lineThickness, rotDeg, lineThickness * 2, lineInfo.Color.ToRayColor());
    }
    #endregion
    
    #region Draw Lines Scaled

    /// <summary>
    /// Draws a circle outline where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="lineInfo">The line drawing parameters.</param>
    /// <param name="sides">The number of sides used to approximate the circle.</param>
    /// <param name="sideScaleFactor">The scale factor for each side (0 = no circle, 1 = normal circle).</param>
    /// <param name="sideScaleOrigin">The point along each circle segment to scale from in both directions (0-1, default 0.5).</param>
    public static void DrawLinesScaled(this Circle c, LineDrawingInfo lineInfo, float smoothness, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        DrawLinesScaled(c, 0f, lineInfo, smoothness, sideScaleFactor, sideScaleOrigin);
    }

    /// <summary>
    /// Draws a circle outline where each side can be scaled towards the origin of the side, with rotation.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="lineInfo">The line drawing parameters.</param>
    /// <param name="rotDeg">The rotation of the circle in degrees.</param>
    /// <param name="sides">The number of sides used to approximate the circle.</param>
    /// <param name="sideScaleFactor">The scale factor for each side (0 = no circle, 1 = normal circle).</param>
    /// <param name="sideScaleOrigin">The point along each circle segment to scale from in both directions (0-1, default 0.5).</param>
    public static void DrawLinesScaled(this Circle c, float rotDeg, LineDrawingInfo lineInfo, float smoothness, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (c.Radius < lineInfo.Thickness)
        {
            var circle = c.SetRadius(lineInfo.Thickness * 2);
            circle.DrawScaled(rotDeg, lineInfo.Color, smoothness, sideScaleFactor, sideScaleOrigin);
            return;
        }
        if (sideScaleFactor <= 0f) return;
        if (sideScaleFactor >= 1f)
        {
            c.DrawLines(lineInfo, smoothness);
            return;
        }
        
        if (!CalculateCircleDrawingParameters(c.Radius, smoothness, out float angleStep, out int sides)) return;
        
        var rotRad = rotDeg * ShapeMath.DEGTORAD;

        for (int i = 0; i < sides; i++)
        {
            var nextIndex = (i + 1) % sides;
            var start = c.Center + new Vector2(c.Radius, 0f).Rotate(rotRad + angleStep * i);
            var end = c.Center + new Vector2(c.Radius, 0f).Rotate(rotRad + angleStep * nextIndex);

            SegmentDrawing.DrawSegment(start, end, lineInfo, sideScaleFactor, sideScaleOrigin);
        }
    }
    
    #endregion
    
    #region Draw Lines Percentage
    /// <summary>
    /// Draws part of a circle outline depending on f.
    /// </summary>
    /// <param name="c">The circle parameters.</param>
    /// <param name="f">The percentage of the outline to draw. A positive value goes counter-clockwise
    /// and a negative value goes clockwise.</param>
    /// <param name="lineThickness">The line drawing parameters.</param>
    /// <param name="rotDeg">The rotation of the circle. The lower the resolution of the circle the more visible is rotation</param>
    /// <param name="sides">The resolution of the circle. The more sides are used the closer it represents a circle.</param>
    /// <param name="color">The color of the line.</param>
    /// <param name="lineCapType">The end cap type of the line.</param>
    /// <param name="capPoints">How many points are used to draw the end cap.</param>
    /// <remarks>
    /// Useful for drawing progress arcs or partial circles.
    /// </remarks>
    public static void DrawLinesPercentage(this Circle c, float f, float rotDeg, float lineThickness, ColorRgba color,  float smoothness = 0.5f)
    {
        if (f == 0) return;

        if (MathF.Abs(f) >= 1f)
        {
            c.DrawLines(rotDeg, lineThickness, color, smoothness);
            return;
        }
        
        if (TransformPercentageToAngles(f, out float startAngleDeg, out float endAngleDeg))
        {
            c.DrawSectorLines(startAngleDeg + rotDeg, endAngleDeg + rotDeg, lineThickness, color, smoothness);
        }
    }

    /// <summary>
    /// Draws part of a circle outline depending on f.
    /// </summary>
    /// <param name="c">The circle parameters.</param>
    /// <param name="f">The percentage of the outline to draw. A positive value goes counter-clockwise
    /// and a negative value goes clockwise.</param>
    /// <param name="lineInfo">The line drawing parameters.</param>
    /// <param name="rotDeg">The rotation of the circle. The lower the resolution of the circle the more visible is rotation</param>
    /// <param name="sides">The resolution of the circle. The more sides are used the closer it represents a circle.</param>
    public static void DrawLinesPercentage(this Circle c, float f, float rotDeg, LineDrawingInfo lineInfo, float smoothness = 0.5f)
    {
        // DrawLinesPercentage(c, f, lineInfo.Thickness, rotDeg, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints, smoothness);
        if (f == 0) return;

        if (MathF.Abs(f) >= 1f)
        {
            c.DrawLines(rotDeg, lineInfo, smoothness);
            return;
        }
        
        if (TransformPercentageToAngles(f, out float startAngleDeg, out float endAngleDeg))
        {
            c.DrawSectorLines(startAngleDeg + rotDeg, endAngleDeg + rotDeg, lineInfo, smoothness);
        }
    }

    #endregion
    
    
    #region Draw Sector
    public static void DrawSector(this Circle c, float startAngleDeg, float endAngleDeg, ColorRgba color, float smoothness = 0.5f)
    {
        if (!CalculateCircleDrawingParameters(c.Radius, startAngleDeg, endAngleDeg, smoothness, out float angleDifRad, out float angleStepRad, out int sides, false)) return;
        Raylib.DrawCircleSector(c.Center, c.Radius, startAngleDeg, startAngleDeg + (angleDifRad * ShapeMath.RADTODEG), sides, color.ToRayColor());
    }
    
    public static void DrawSector(this Circle c, float startAngleDeg, float endAngleDeg, float rotDeg, ColorRgba color, float smoothness = 0.5f)
    {
        c.DrawSector(startAngleDeg + rotDeg, endAngleDeg + rotDeg, color, smoothness);
    }
    #endregion
    
    #region Draw Sector Scaled

    public static void DrawSectorScaled(this Circle c, float startAngleDeg, float endAngleDeg, float rotDeg, ColorRgba color, float smoothness, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (c.Radius <= 0f || sideScaleFactor <= 0f) return;
        
        if (sideScaleFactor >= 1f)
        {
            c.DrawSector(startAngleDeg, endAngleDeg, rotDeg, color, smoothness);
            return;
        }
        
        startAngleDeg = startAngleDeg + rotDeg;
        if (!CalculateCircleDrawingParameters(c.Radius, startAngleDeg, endAngleDeg + rotDeg, smoothness, out float angleDifRad, out float angleStep, out int sides, false)) return;
        
        var absAngleDifRad = MathF.Abs(angleDifRad);
        if (absAngleDifRad < 0.00001f) return;
        
        if (absAngleDifRad >= MathF.Tau)
        {
            c.DrawScaled(rotDeg, color, smoothness, sideScaleFactor, sideScaleOrigin);
            return;
        }
        
        float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
        var rayColor = color.ToRayColor();
        for (int i = 0; i < sides; i++)
        {
            var nextIndex = i + 1;
            var start = c.Center + new Vector2(c.Radius, 0f).Rotate(startAngleRad + angleStep * i);
            var end = c.Center + new Vector2(c.Radius, 0f).Rotate(startAngleRad + angleStep * nextIndex);

            var scaledSegment = new Segment(start, end).ScaleSegment(sideScaleFactor, sideScaleOrigin);

            if (angleDifRad < 0)
            {
                Raylib.DrawTriangle(scaledSegment.Start, scaledSegment.End,c.Center, rayColor);
            }
            else
            {
                Raylib.DrawTriangle(c.Center, scaledSegment.End, scaledSegment.Start, rayColor);
            }
            
        }
    }
    #endregion
    
    #region Draw Sector Lines
   
    public static void DrawSectorLinesClosed(this Circle c, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, LineDrawingInfo lineInfo, float smoothness = 0.5f)
    {
        var color = lineInfo.Color.SetAlpha(255);
        if (c.Radius < lineInfo.Thickness)
        {
            var circle = c.SetRadius(lineInfo.Thickness * 2);
            circle.DrawSector(startAngleDeg, endAngleDeg, rotOffsetDeg, lineInfo.Color, smoothness);
            return;
        }
        
        startAngleDeg = startAngleDeg + rotOffsetDeg;
        if (!CalculateCircleDrawingParameters(c.Radius, startAngleDeg, endAngleDeg + rotOffsetDeg, smoothness, out float angleDifRad, out float angleStepRad, out int sides, false)) return;
        endAngleDeg = startAngleDeg + angleDifRad * ShapeMath.RADTODEG;
        
        var absAngleDifRad = MathF.Abs(angleDifRad);
        if (absAngleDifRad < 0.00001f) return;
        
        var start = c.Center + (ShapeVec.Right() * c.Radius).Rotate(startAngleDeg * ShapeMath.DEGTORAD);
        SegmentDrawing.DrawSegment(c.Center, start, lineInfo.Thickness, color, LineCapType.CappedExtended, lineInfo.CapPoints);
        
        if (absAngleDifRad >= MathF.Tau)
        {
            c.DrawLines(rotOffsetDeg, lineInfo.Thickness, color, smoothness);
            return;
        }
        
        var end = c.Center + (ShapeVec.Right() * c.Radius).Rotate(endAngleDeg * ShapeMath.DEGTORAD);
        SegmentDrawing.DrawSegment(c.Center, end, lineInfo.Thickness, color, LineCapType.CappedExtended, lineInfo.CapPoints);
        
        Raylib.DrawRing(c.Center, c.Radius - lineInfo.Thickness, c.Radius + lineInfo.Thickness, startAngleDeg, endAngleDeg, sides, color.ToRayColor());
    }
    
    public static void DrawSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, LineDrawingInfo lineInfo, float smoothness = 0.5f)
    {
        c.DrawSectorLines(startAngleDeg, endAngleDeg, 0f, lineInfo, smoothness);
    }
    public static void DrawSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, float lineThickness, ColorRgba color, float smoothness = 0.5f)
    {
        c.DrawSectorLines(startAngleDeg, endAngleDeg, 0f, lineThickness, color, smoothness);
    }
    public static void DrawSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, LineDrawingInfo lineInfo, float smoothness = 0.5f)
    {
        if (c.Radius < lineInfo.Thickness)
        {
            var circle = c.SetRadius(lineInfo.Thickness * 2);
            circle.DrawSector(startAngleDeg, endAngleDeg, rotOffsetDeg, lineInfo.Color, smoothness);
            return;
        }
        
        startAngleDeg = startAngleDeg + rotOffsetDeg;
        if (!CalculateCircleDrawingParameters(c.Radius, startAngleDeg, endAngleDeg + rotOffsetDeg, smoothness, out float angleDifRad, out float angleStepRad, out int sides, false)) return;
        endAngleDeg = startAngleDeg + angleDifRad * ShapeMath.RADTODEG;
        
        var absAngleDifRad = MathF.Abs(angleDifRad);
        if (absAngleDifRad < 0.00001f) return;
        if (absAngleDifRad >= MathF.Tau)
        {
            c.DrawLines(rotOffsetDeg, lineInfo, smoothness);
            return;
        }
        
        var lineCapType = lineInfo.CapType;
        var capPoints = lineInfo.CapPoints;
        var drawCap = (lineCapType is LineCapType.Capped or LineCapType.CappedExtended) && capPoints > 0;

        if (drawCap) //shrink angle segment
        {
            // var angleExtensionRad = Circle.ArcLengthToAngle(lineInfo.Thickness, c.Radius, true);
            // var angleExtensionDeg = angleExtensionRad * ShapeMath.RADTODEG;
            // if (angleDifRad >= 0)
            // {
            //     // startAngleDeg += angleExtensionDeg;
            //     endAngleDeg -= angleExtensionDeg * 2;
            //     if(endAngleDeg < startAngleDeg) endAngleDeg = startAngleDeg;
            // }
            // else
            // {
            //     // startAngleDeg -= angleExtensionDeg;
            //     endAngleDeg += angleExtensionDeg * 2;
            //     if(endAngleDeg > startAngleDeg) endAngleDeg = startAngleDeg;
            // }
            //     
            // var angleDifDeg = endAngleDeg - startAngleDeg;
            // if (angleDifDeg > 360f) angleDifDeg %= 360f;
            // if (angleDifDeg < -360f) angleDifDeg %= 360f;
            // angleDifRad = angleDifDeg * ShapeMath.DEGTORAD;
            
            var startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
            var endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
            
            var p1Inner = c.Center + (ShapeVec.Right() * (c.Radius - lineInfo.Thickness)).Rotate(startAngleRad);
            var p1Outer = c.Center + (ShapeVec.Right() * (c.Radius + lineInfo.Thickness)).Rotate(startAngleRad);
        
            var p2Inner = c.Center + (ShapeVec.Right() * (c.Radius - lineInfo.Thickness)).Rotate(endAngleRad);
            var p2Outer = c.Center + (ShapeVec.Right() * (c.Radius + lineInfo.Thickness)).Rotate(endAngleRad);
            
            
            if (angleDifRad > 0)
            {
                SegmentDrawing.DrawRoundCap(p2Inner, p2Outer, lineInfo.CapPoints, lineInfo.Color);
                SegmentDrawing.DrawRoundCap(p1Outer, p1Inner, lineInfo.CapPoints, lineInfo.Color);
            }
            else
            {
                SegmentDrawing.DrawRoundCap(p2Outer, p2Inner, lineInfo.CapPoints, lineInfo.Color);
                SegmentDrawing.DrawRoundCap(p1Inner, p1Outer, lineInfo.CapPoints, lineInfo.Color);
            }
        }
        
        Raylib.DrawRing(c.Center, c.Radius - lineInfo.Thickness, c.Radius + lineInfo.Thickness, startAngleDeg, endAngleDeg, sides, lineInfo.Color.ToRayColor());
        
    }
    public static void DrawSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, float lineThickness, ColorRgba color, float smoothness = 0.5f)
    {
        if (c.Radius < lineThickness)
        {
            var circle = c.SetRadius(lineThickness * 2);
            circle.DrawSector(startAngleDeg, endAngleDeg, rotOffsetDeg, color, smoothness);
            return;
        }
        if (!CalculateCircleDrawingParameters(c.Radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, smoothness, out float angleDifRad, out float angleStepRad, out int sides, false)) return;
        Raylib.DrawRing(c.Center, c.Radius - lineThickness, c.Radius + lineThickness, startAngleDeg + rotOffsetDeg, startAngleDeg + rotOffsetDeg + (angleDifRad * ShapeMath.RADTODEG), sides, color.ToRayColor());
    }
    #endregion
    
    #region Draw Sector Lines Scaled
    public static void DrawSectorLinesScaledClosed(this Circle c, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, LineDrawingInfo lineInfo, float smoothness, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (c.Radius < lineInfo.Thickness)
        {
            var circle = c.SetRadius(lineInfo.Thickness * 2);
            circle.DrawSectorScaled(startAngleDeg, endAngleDeg, rotOffsetDeg, lineInfo.Color, smoothness, sideScaleFactor, sideScaleOrigin);
            return;
        }
        if (sideScaleFactor <= 0f) return;
        
        if (sideScaleFactor >= 1f)
        {
            DrawSectorLinesClosed(c, startAngleDeg, endAngleDeg, rotOffsetDeg, lineInfo, smoothness);
            return;
        }
        startAngleDeg = startAngleDeg + rotOffsetDeg;
        if (!CalculateCircleDrawingParameters(c.Radius, startAngleDeg, endAngleDeg + rotOffsetDeg, smoothness, out float angleDifRad, out float angleStep, out int sides, false)) return;
        endAngleDeg = startAngleDeg + angleDifRad * ShapeMath.RADTODEG;
        
        var absAngleDifRad = MathF.Abs(angleDifRad);
        if (absAngleDifRad < 0.00001f) return;
        
        float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
        float endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
        
        var sectorStart = c.Center + (ShapeVec.Right() * c.Radius).Rotate(startAngleRad);
        SegmentDrawing.DrawSegment(c.Center, sectorStart, lineInfo, sideScaleFactor, sideScaleOrigin);
        
        if (absAngleDifRad >= MathF.Tau)
        {
            c.DrawLinesScaled(rotOffsetDeg, lineInfo, smoothness, sideScaleFactor, sideScaleOrigin);
            return;
        }
        
        var sectorEnd = c.Center + (ShapeVec.Right() * c.Radius).Rotate(endAngleRad);
        SegmentDrawing.DrawSegment(c.Center, sectorEnd, lineInfo, sideScaleFactor, sideScaleOrigin);
        
        for (int i = 0; i < sides; i++)
        {
            var nextIndex = i + 1;
            var start = c.Center + new Vector2(c.Radius, 0f).Rotate(startAngleRad + angleStep * i);
            var end = c.Center + new Vector2(c.Radius, 0f).Rotate(startAngleRad + angleStep * nextIndex);

            SegmentDrawing.DrawSegment(start, end, lineInfo, sideScaleFactor, sideScaleOrigin);
        }
    }
    public static void DrawSectorLinesScaled(this Circle c, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, LineDrawingInfo lineInfo, float smoothness, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (c.Radius < lineInfo.Thickness)
        {
            var circle = c.SetRadius(lineInfo.Thickness * 2);
            circle.DrawSectorScaled(startAngleDeg, endAngleDeg, rotOffsetDeg, lineInfo.Color, smoothness, sideScaleFactor, sideScaleOrigin);
            return;
        }
        if (sideScaleFactor <= 0f) return;
        
        if (sideScaleFactor >= 1f)
        {
            c.DrawSectorLines(startAngleDeg, endAngleDeg, rotOffsetDeg, lineInfo, smoothness);
            return;
        }
        
        startAngleDeg = startAngleDeg + rotOffsetDeg;
        if (!CalculateCircleDrawingParameters(c.Radius, startAngleDeg, endAngleDeg + rotOffsetDeg, smoothness, out float angleDifRad, out float angleStep, out int sides, false)) return;
        
        var absAngleDifRad = MathF.Abs(angleDifRad);
        if (absAngleDifRad < 0.00001f) return;
        
        if (absAngleDifRad >= MathF.Tau)
        {
            c.DrawLinesScaled(rotOffsetDeg, lineInfo, smoothness, sideScaleFactor, sideScaleOrigin);
            return;
        }
        
        float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
        for (int i = 0; i < sides; i++)
        {
            var nextIndex = i + 1;
            var start = c.Center + new Vector2(c.Radius, 0f).Rotate(startAngleRad + angleStep * i);
            var end = c.Center + new Vector2(c.Radius, 0f).Rotate(startAngleRad + angleStep * nextIndex);

            SegmentDrawing.DrawSegment(start, end, lineInfo, sideScaleFactor, sideScaleOrigin);
        }
    }
    #endregion

    
    #region Draw Masked
    /// <summary>
    /// Draws the circle outline as individual line segments clipped by a <see cref="Triangle"/> mask.
    /// </summary>
    /// <param name="circle">The source <see cref="Circle"/> whose circumference will be approximated by segments.</param>
    /// <param name="mask">The <see cref="Triangle"/> used to mask (clip) each generated segment.</param>
    /// <param name="lineInfo">Parameters controlling line drawing (thickness, color, cap type, etc.).</param>
    /// <param name="rotDeg">Rotation offset in degrees applied to the entire approximated polygon.</param>
    /// <param name="sides">Number of sides used to approximate the circle. Values below 3 will be clamped to 3.</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawLinesMasked(this Circle circle, Triangle mask, LineDrawingInfo lineInfo, float rotDeg, float smoothness, bool reversedMask = false)
    {
        if (!CalculateCircleDrawingParameters(circle.Radius, smoothness, out float angleStep, out int sides)) return;
        var rotRad = rotDeg * ShapeMath.DEGTORAD;
        var radius = circle.Radius;
        var center = circle.Center;
        for (int i = 0; i < sides; i++)
        {
            var nextIndex = (i + 1) % sides;
            var curP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * i);
            var nextP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * nextIndex);
            var segment = new Segment(curP, nextP);
            segment.DrawMasked(mask, lineInfo, reversedMask);
        }
    }
    /// <summary>
    /// Draws the circle outline as individual line segments clipped by a <see cref="Circle"/> mask.
    /// </summary>
    /// <param name="circle">The source <see cref="Circle"/> whose circumference will be approximated by segments.</param>
    /// <param name="mask">The <see cref="Circle"/> used to mask (clip) each generated segment.</param>
    /// <param name="lineInfo">Parameters controlling line drawing (thickness, color, cap type, etc.).</param>
    /// <param name="rotDeg">Rotation offset in degrees applied to the entire approximated polygon.</param>
    /// <param name="sides">Number of sides used to approximate the circle. Values below 3 will be clamped to 3.</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawLinesMasked(this Circle circle, Circle mask, LineDrawingInfo lineInfo, float rotDeg, float smoothness, bool reversedMask = false)
    {
        if (!CalculateCircleDrawingParameters(circle.Radius, smoothness, out float angleStep, out int sides)) return;
        var rotRad = rotDeg * ShapeMath.DEGTORAD;
        var radius = circle.Radius;
        var center = circle.Center;
        for (int i = 0; i < sides; i++)
        {
            var nextIndex = (i + 1) % sides;
            var curP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * i);
            var nextP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * nextIndex);
            var segment = new Segment(curP, nextP);
            segment.DrawMasked(mask, lineInfo, reversedMask);
        }
    }
    /// <summary>
    /// Draws the circle outline as individual line segments clipped by a <see cref="Rect"/> mask.
    /// </summary>
    /// <param name="circle">The source <see cref="Circle"/> whose circumference will be approximated by segments.</param>
    /// <param name="mask">The <see cref="Rect"/> used to mask (clip) each generated segment.</param>
    /// <param name="lineInfo">Parameters controlling line drawing (thickness, color, cap type, etc.).</param>
    /// <param name="rotDeg">Rotation offset in degrees applied to the entire approximated polygon.</param>
    /// <param name="sides">Number of sides used to approximate the circle. Values below 3 will be clamped to 3.</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawLinesMasked(this Circle circle, Rect mask, LineDrawingInfo lineInfo, float rotDeg, float smoothness, bool reversedMask = false)
    {
        if (!CalculateCircleDrawingParameters(circle.Radius, smoothness, out float angleStep, out int sides)) return;
        var rotRad = rotDeg * ShapeMath.DEGTORAD;
        var radius = circle.Radius;
        var center = circle.Center;
        for (int i = 0; i < sides; i++)
        {
            var nextIndex = (i + 1) % sides;
            var curP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * i);
            var nextP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * nextIndex);
            var segment = new Segment(curP, nextP);
            segment.DrawMasked(mask, lineInfo, reversedMask);
        }
    }
    /// <summary>
    /// Draws the circle outline as individual line segments clipped by a <see cref="Quad"/> mask.
    /// </summary>
    /// <param name="circle">The source <see cref="Circle"/> whose circumference will be approximated by segments.</param>
    /// <param name="mask">The <see cref="Quad"/> used to mask (clip) each generated segment.</param>
    /// <param name="lineInfo">Parameters controlling line drawing (thickness, color, cap type, etc.).</param>
    /// <param name="rotDeg">Rotation offset in degrees applied to the entire approximated polygon.</param>
    /// <param name="sides">Number of sides used to approximate the circle. Values below 3 will be clamped to 3.</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawLinesMasked(this Circle circle, Quad mask, LineDrawingInfo lineInfo, float rotDeg, float smoothness, bool reversedMask = false)
    {
        if (!CalculateCircleDrawingParameters(circle.Radius, smoothness, out float angleStep, out int sides)) return;
        var rotRad = rotDeg * ShapeMath.DEGTORAD;
        var radius = circle.Radius;
        var center = circle.Center;
        for (int i = 0; i < sides; i++)
        {
            var nextIndex = (i + 1) % sides;
            var curP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * i);
            var nextP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * nextIndex);
            var segment = new Segment(curP, nextP);
            segment.DrawMasked(mask, lineInfo, reversedMask);
        }
    }
    /// <summary>
    /// Draws the circle outline as individual line segments clipped by a <see cref="Polygon"/> mask.
    /// </summary>
    /// <param name="circle">The source <see cref="Circle"/> whose circumference will be approximated by segments.</param>
    /// <param name="mask">The <see cref="Polygon"/> used to mask (clip) each generated segment.</param>
    /// <param name="lineInfo">Parameters controlling line drawing (thickness, color, cap type, etc.).</param>
    /// <param name="rotDeg">Rotation offset in degrees applied to the entire approximated polygon.</param>
    /// <param name="sides">Number of sides used to approximate the circle. Values below 3 will be clamped to 3.</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawLinesMasked(this Circle circle, Polygon mask, LineDrawingInfo lineInfo, float rotDeg, float smoothness, bool reversedMask = false)
    {
        if (!CalculateCircleDrawingParameters(circle.Radius, smoothness, out float angleStep, out int sides)) return;
        var rotRad = rotDeg * ShapeMath.DEGTORAD;
        var radius = circle.Radius;
        var center = circle.Center;
        for (int i = 0; i < sides; i++)
        {
            var nextIndex = (i + 1) % sides;
            var curP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * i);
            var nextP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * nextIndex);
            var segment = new Segment(curP, nextP);
            segment.DrawMasked(mask, lineInfo, reversedMask);
        }
    }
    /// <summary>
    /// Draws the circle outline as individual line segments clipped by a mask of any closed shape type.
    /// </summary>
    /// <typeparam name="T">The mask type implementing <see cref="IClosedShapeTypeProvider"/> (for example: <see cref="Triangle"/>, <see cref="Circle"/>, <see cref="Rect"/>, <see cref="Quad"/>, <see cref="Polygon"/>).</typeparam>
    /// <param name="circle">The source <see cref="Circle"/> whose circumference will be approximated by segments.</param>
    /// <param name="mask">The mask used to clip each generated segment.</param>
    /// <param name="lineInfo">Parameters controlling line drawing (thickness, color, cap type, etc.).</param>
    /// <param name="rotDeg">Rotation offset in degrees applied to the entire approximated polygon.</param>
    /// <param name="sides">Number of sides used to approximate the circle. Values below 3 will be clamped to 3.</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawLinesMasked<T>(this Circle circle, T mask, LineDrawingInfo lineInfo, float rotDeg, float smoothness, bool reversedMask = false) where T : IClosedShapeTypeProvider
    {
        if (!CalculateCircleDrawingParameters(circle.Radius, smoothness, out float angleStep, out int sides)) return;
        var rotRad = rotDeg * ShapeMath.DEGTORAD;
        var radius = circle.Radius;
        var center = circle.Center;
        for (int i = 0; i < sides; i++)
        {
            var nextIndex = (i + 1) % sides;
            var curP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * i);
            var nextP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * nextIndex);
            var segment = new Segment(curP, nextP);
            segment.DrawMasked(mask, lineInfo, reversedMask);
        }
    }
    #endregion
    
    #region Draw Checkered
    /// <summary>
    /// Draws a checkered pattern of lines inside a circle, optionally with a background color.
    /// </summary>
    /// <param name="pos">The position of the circle (before alignment).</param>
    /// <param name="alignment">The anchor point alignment for the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="spacing">The spacing between checkered lines.</param>
    /// <param name="lineThickness">The thickness of the checkered lines.</param>
    /// <param name="angleDeg">The rotation of the checkered pattern in degrees.</param>
    /// <param name="lineColorRgba">The color of the checkered lines.</param>
    /// <param name="bgColorRgba">The background color of the circle (drawn first if alpha &gt; 0).</param>
    /// <param name="circleSegments">The number of segments used to approximate the circle.</param>
    /// <remarks>
    /// Useful for visualizing grid or checkered overlays on circular shapes.
    /// </remarks>
    public static void DrawCircleCheckeredLines(Vector2 pos, AnchorPoint alignment, float radius, float spacing, 
        float lineThickness, float angleDeg, ColorRgba lineColorRgba, ColorRgba bgColorRgba, float smoothness = 0.5f)
    {

        float maxDimension = radius;
        var size = new Vector2(radius, radius) * 2f;
        var aVector = alignment.ToVector2() * size;
        var center = pos - aVector + size / 2;
        float rotRad = angleDeg * ShapeMath.DEGTORAD;

        if (bgColorRgba.A > 0)
        {
            var circle = new Circle(center, radius);
            circle.Draw(bgColorRgba, smoothness);
        }

        var cur = new Vector2(-spacing / 2, 0f);
        while (cur.X > -maxDimension)
        {
            var p = center + cur.Rotate(rotRad);

            //float y = MathF.Sqrt((radius * radius) - (cur.X * cur.X));
            float angle = MathF.Acos(cur.X / radius);
            float y = radius * MathF.Sin(angle);

            var up = new Vector2(0f, -y);
            var down = new Vector2(0f, y);
            var start = p + up.Rotate(rotRad);
            var end = p + down.Rotate(rotRad);
            SegmentDrawing.DrawSegment(start, end, lineThickness, lineColorRgba);
            cur.X -= spacing;
        }

        cur = new(spacing / 2, 0f);
        while (cur.X < maxDimension)
        {
            var p = center + cur.Rotate(rotRad);
            //float y = MathF.Sqrt((radius * radius) - (cur.X * cur.X));
            float angle = MathF.Acos(cur.X / radius);
            float y = radius * MathF.Sin(angle);

            var up = new Vector2(0f, -y);
            var down = new Vector2(0f, y);
            var start = p + up.Rotate(rotRad);
            var end = p + down.Rotate(rotRad);
            SegmentDrawing.DrawSegment(start, end, lineThickness, lineColorRgba);
            cur.X += spacing; 
        }
    }
    #endregion
    
    
    #region Helper

    public static ValueRange CircleSideLengthRange = new ValueRange(2f, 75f);
    public static ValueRangeInt CircleSideCountRange = new ValueRangeInt(8, 128);

    public static bool GetCircleSmoothness(float radius, int sides, out float smoothness)
    {
        smoothness = 0f;
        if (radius <= 0f) return false;
        sides = CircleSideCountRange.Clamp(sides);
        float circumference = 2.0f * ShapeMath.PI * radius;
        float sideLength = circumference / sides;
        float f = CircleSideLengthRange.Inverse(sideLength);
        smoothness = ShapeMath.Clamp(f, 0f, 1f);
        return true;
    }
    public static bool GetCircleSmoothness(float radius, float startAngleDeg, float endAngleDeg, int sides, out float smoothness)
    {
        smoothness = 0f;
        if (radius <= 0f) return false;
        
        // Normalize angle difference to [-360, 360], preserving sign
        float angleDiffDeg = endAngleDeg - startAngleDeg;
        if (angleDiffDeg > 360f) angleDiffDeg %= 360f;
        if (angleDiffDeg < -360f) angleDiffDeg %= 360f;
        float absAngleDiffDeg = MathF.Abs(angleDiffDeg);
        if (absAngleDiffDeg < 0.00001f) return false;
        
        sides = CircleSideCountRange.Clamp(sides);
        float arcLength = 2f * ShapeMath.PI * radius * (absAngleDiffDeg / 360f);
        float sideLength = arcLength / sides;
        float f = CircleSideLengthRange.Inverse(sideLength);
        smoothness = ShapeMath.Clamp(f, 0f, 1f);
        return true;
    }
    
    public static bool CalculateCircleDrawingParameters(float radius, float startAngleDeg, float endAngleDeg, float smoothness, out float angleDifRad, out float angleStepRad, out int sides, bool clampSides = true)
    {
        angleStepRad = 0f;
        sides = 0;
        angleDifRad = 0f;
        if (radius <= 0f) return false;
        
        // Normalize angle difference to [-360, 360], preserving sign
        float angleDiffDeg = endAngleDeg - startAngleDeg;
        if (angleDiffDeg > 360f) angleDiffDeg %= 360f;
        if (angleDiffDeg < -360f) angleDiffDeg %= 360f;

        float absAngleDiffDeg = MathF.Abs(angleDiffDeg);
        if (absAngleDiffDeg < 0.00001f) return false;

        angleDifRad = angleDiffDeg * ShapeMath.DEGTORAD;
        
        smoothness = ShapeMath.Clamp(smoothness, 0f, 1f);
        float sideLength = CircleSideLengthRange.LerpInverse(smoothness);
        if(sideLength <= 0f) return false;

        // Arc length for the given angle
        float arcLength = 2f * ShapeMath.PI * radius * (absAngleDiffDeg / 360f);

        sides = (int)MathF.Ceiling(arcLength / sideLength);
        if(clampSides) sides = CircleSideCountRange.Clamp(sides);

        // Angle step in radians (preserve direction)
        angleStepRad = angleDifRad / sides;

        return true;
    }
    public static bool CalculateCircleDrawingParameters(float radius, float startAngleDeg, float endAngleDeg, float smoothness, out float angleStepRad, out int sides, bool clampSides = true)
    {
        angleStepRad = 0f;
        sides = 0;
        if (radius <= 0f) return false;
        
        // Normalize angle difference to [-360, 360], preserving sign
        float angleDiffDeg = endAngleDeg - startAngleDeg;
        if (angleDiffDeg > 360f) angleDiffDeg %= 360f;
        if (angleDiffDeg < -360f) angleDiffDeg %= 360f;

        float absAngleDiffDeg = MathF.Abs(angleDiffDeg);
        if (absAngleDiffDeg < 0.00001f) return false;

        smoothness = ShapeMath.Clamp(smoothness, 0f, 1f);
        float sideLength = CircleSideLengthRange.LerpInverse(smoothness);
        if(sideLength <= 0f) return false;

        // Arc length for the given angle
        float arcLength = 2f * ShapeMath.PI * radius * (absAngleDiffDeg / 360f);

        sides = (int)MathF.Ceiling(arcLength / sideLength);
        if(clampSides) sides = CircleSideCountRange.Clamp(sides);

        // Angle step in radians (preserve direction)
        angleStepRad = (angleDiffDeg * ShapeMath.DEGTORAD) / sides;

        return true;
    }
    public static bool CalculateCircleDrawingParameters(float radius, float startAngleDeg, float endAngleDeg, float smoothness, out int sides, bool clampSides = true)
    {
        sides = 0;
        if (radius <= 0f) return false;
        
        // Normalize angle difference to [-360, 360], preserving sign
        float angleDiffDeg = endAngleDeg - startAngleDeg;
        if (angleDiffDeg > 360f) angleDiffDeg %= 360f;
        if (angleDiffDeg < -360f) angleDiffDeg %= 360f;

        float absAngleDiffDeg = MathF.Abs(angleDiffDeg);
        if (absAngleDiffDeg < 0.00001f) return false;

        smoothness = ShapeMath.Clamp(smoothness, 0f, 1f);
        float sideLength = CircleSideLengthRange.LerpInverse(smoothness);
        if(sideLength <= 0f) return false;

        // Arc length for the given angle
        float arcLength = 2f * ShapeMath.PI * radius * (absAngleDiffDeg / 360f);

        sides = (int)MathF.Ceiling(arcLength / sideLength);
        if(clampSides) sides = CircleSideCountRange.Clamp(sides);
        
        return true;
    }
    public static bool CalculateCircleDrawingParameters(float radius, float smoothness, out float angleStepRad, out int sides, bool clampSides = true)
    {
        sides = 0;
        angleStepRad = 0f;
        if(radius <= 0f) return false;
        
        smoothness = ShapeMath.Clamp(smoothness, 0f, 1f);
        float sideLength = CircleSideLengthRange.LerpInverse(smoothness);
        if(sideLength <= 0f) return false;
        
        float circumference = 2.0f * ShapeMath.PI * radius;
        sides = (int)MathF.Ceiling(circumference / sideLength);
        if(clampSides) sides = CircleSideCountRange.Clamp(sides);
        
        angleStepRad = MathF.Tau / sides;
        return true;
    }
    public static bool CalculateCircleDrawingParameters(float radius, float smoothness, out int sides, bool clampSides = true)
    {
        sides = 0;
        if(radius <= 0f) return false;
        
        smoothness = ShapeMath.Clamp(smoothness, 0f, 1f);
        float sideLength = CircleSideLengthRange.LerpInverse(smoothness);
        if(sideLength <= 0f) return false;
        
        float circumference = 2.0f * ShapeMath.PI * radius;
        sides = (int)MathF.Ceiling(circumference / sideLength);
        if(clampSides) sides = CircleSideCountRange.Clamp(sides);
        
        return true;
    }
    
    public static int GetCircleSideCount(float radius, float sideLength = 10f, bool clampSides = true)
    {
        if (radius <= 0f || sideLength <= 0f) return 0;
        float circumference = 2.0f * ShapeMath.PI * radius;
        var sides = (int)MathF.Ceiling(circumference / sideLength);
        if(clampSides) sides = CircleSideCountRange.Clamp(sides);
        return sides;
    }

    public static int GetCircleArcSideCount(float radius, float angleDeg, float sideLength = 10f, bool clampSides = true)
    {
        if (radius <= 0f || angleDeg <= 0f || sideLength <= 0f) return 0;
        
        float circumference = 2.0f * ShapeMath.PI * radius * (angleDeg / 360f);
        var sides = (int)MathF.Ceiling(circumference / sideLength);
        if(clampSides) sides = CircleSideCountRange.Clamp(sides);
        return sides;
    }
    
    private static bool TransformPercentageToAngles(float f, out float startAngleDeg, out float endAngleDeg)
    {
        startAngleDeg = 0f;
        endAngleDeg = 0f;
        
        if(f == 0) return false;
        
        if (f >= 0f)
        {
            startAngleDeg = 0f;
            endAngleDeg = 360f * ShapeMath.Clamp(f, 0f, 1f);
            return true;
        }

        startAngleDeg = 0f;
        endAngleDeg = 360f * ShapeMath.Clamp(f, -1f, 0f);
        return true;
    }
    #endregion
}
