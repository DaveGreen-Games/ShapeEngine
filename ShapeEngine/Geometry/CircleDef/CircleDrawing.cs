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
// - Remove all functions with sideLength parameter
// - Change all functions with sides/segments parameter to use smoothness instead
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
    
    #region Draw Full Circle

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
        if (!CalculateCircleDrawingParameters(c.Radius, smoothness, out int sides)) return;
        //TODO: Test performance
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
        DrawLines(c, 0f, lineInfo, smoothness);
    }

    /// <summary>
    /// Draws the outline of a circle using the specified line drawing info, rotation, and number of sides.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="lineInfo">The line drawing parameters for the drawing the circle outline.
    /// Only <see cref="LineDrawingInfo.Thickness"/> and <see cref="LineDrawingInfo.Color"/> are used!</param>
    /// <param name="rotDeg">The rotation of the circle in degrees.</param>
    /// <param name="sides">The number of sides used to approximate the circle.</param>
    public static void DrawLines(this Circle c, float rotDeg,LineDrawingInfo lineInfo, float smoothness = 0.5f)
    {
        //TODO: Why do I use this if I can use Raylib.DrawPolyLinesEx???
        DrawCircleLinesInternal(c.Center, c.Radius, rotDeg, lineInfo.Thickness, lineInfo.Color, smoothness);
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
        if (sideScaleFactor <= 0f) return;
        if (sideScaleFactor >= 1f)
        {
            DrawLines(c, lineInfo, smoothness);
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
    public static void DrawLinesPercentage(this Circle c, float f, float rotDeg, float lineThickness, ColorRgba color, LineCapType lineCapType, int capPoints, float smoothness = 0.5f)
    {
        if (f == 0) return;

        if (MathF.Abs(f) >= 1f)
        {
            DrawCircleLinesInternal(c.Center, c.Radius, rotDeg, lineThickness, color, smoothness);
            return;
        }
        
        if (TransformPercentageToAngles(f, out float startAngleDeg, out float endAngleDeg))
        {
            DrawCircleSectorLinesOpenInternal(c.Center, c.Radius, startAngleDeg + rotDeg, endAngleDeg + rotDeg, lineThickness, color, lineCapType, capPoints, smoothness);
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
        DrawLinesPercentage(c, f, lineInfo.Thickness, rotDeg, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints, smoothness);
    }

    #endregion
    
    #region Circle Sector
    
    /// <summary>
    /// Draws a filled sector of a circle using the specified <see cref="Circle"/> instance, start and end angles, segment count, and color.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="startAngleDeg">The starting angle of the sector in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the sector in degrees.</param>
    /// <param name="segments">The number of segments used to approximate the sector.</param>
    /// <param name="color">The color of the sector.</param>
    public static void DrawSector(this Circle c, float startAngleDeg, float endAngleDeg, ColorRgba color, float smoothness = 0.5f)
    {
        if (!CalculateCircleDrawingParameters(c.Radius, smoothness, out int sides)) return;
        Raylib.DrawCircleSector(c.Center, c.Radius, startAngleDeg, endAngleDeg, sides, color.ToRayColor());
    }
    
    #endregion
    
    #region Draw Circle Sector Lines Scaled
    /// <summary>
    /// Draws a sector outline where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="lineInfo">The line drawing parameters.</param>
    /// <param name="startAngleDeg">The starting angle of the sector in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the sector in degrees.</param>
    /// <param name="rotOffsetDeg">The rotation offset in degrees.</param>
    /// <param name="sides">The number of sides used to approximate the sector.</param>
    /// <param name="sideScaleFactor">The scale factor for each side (0 = no sector, 1 = normal sector).</param>
    /// <param name="sideScaleOrigin">The point along each circle segment to scale from in both directions (0-1, default 0.5).</param>
    /// <param name="closed">Whether the sector should be closed (connect to the center).</param>
    public static void DrawSectorLinesScaled(this Circle c, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, LineDrawingInfo lineInfo, float smoothness,
        float sideScaleFactor, float sideScaleOrigin = 0.5f, bool closed = true)
    {
        if (sideScaleFactor <= 0f) return;
        if (sideScaleFactor >= 1f)
        {
            DrawSectorLines(c, startAngleDeg, endAngleDeg, rotOffsetDeg, lineInfo, smoothness, closed);
            return;
        }

        if (!CalculateCircleDrawingParameters(c.Radius, startAngleDeg, endAngleDeg, smoothness, out float angleStep, out int sides)) return;
        
        float startAngleRad = (startAngleDeg + rotOffsetDeg) * ShapeMath.DEGTORAD;
        float endAngleRad = (endAngleDeg + rotOffsetDeg) * ShapeMath.DEGTORAD;
        // float anglePiece = endAngleRad - startAngleRad;
        // float angleStep = anglePiece / sides;
        if (closed)
        {
            var sectorStart = c.Center + (ShapeVec.Right() * c.Radius).Rotate(startAngleRad);
            SegmentDrawing.DrawSegment(c.Center, sectorStart, lineInfo, sideScaleFactor, sideScaleOrigin);

            var sectorEnd = c.Center + (ShapeVec.Right() * c.Radius).Rotate(endAngleRad);
            SegmentDrawing.DrawSegment(c.Center, sectorEnd, lineInfo, sideScaleFactor, sideScaleOrigin);
        }
        for (int i = 0; i < sides; i++)
        {
            var nextIndex = i + 1;
            var start = c.Center + new Vector2(c.Radius, 0f).Rotate(startAngleRad + angleStep * i);
            var end = c.Center + new Vector2(c.Radius, 0f).Rotate(startAngleRad + angleStep * nextIndex);

            SegmentDrawing.DrawSegment(start, end, lineInfo, sideScaleFactor, sideScaleOrigin);
        }
    }
    #endregion
    
    #region Draw Circle Sector Lines
    /// <summary>
    /// Draws the outline of a sector of a circle using the specified <see cref="Circle"/> instance, start and end angles, number of sides, line drawing info, and optional closure.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="startAngleDeg">The starting angle of the sector in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the sector in degrees.</param>
    /// <param name="sides">The number of sides used to approximate the sector.</param>
    /// <param name="lineInfo">The line drawing parameters.</param>
    /// <param name="closed">Whether the sector should be closed (connect to the center).</param>
    public static void DrawSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, LineDrawingInfo lineInfo, float smoothness = 0.5f, bool closed = false)
    {
        if (closed)
        {
            DrawCircleSectorLinesClosedInternal(c.Center, c.Radius, startAngleDeg, endAngleDeg, lineInfo.Thickness, lineInfo.Color, smoothness);
        }
        else
        {
            DrawCircleSectorLinesOpenInternal(c.Center, c.Radius, startAngleDeg, endAngleDeg, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints, smoothness);
        }
    }
    public static void DrawSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, float lineThickness, ColorRgba color, float smoothness = 0.5f, bool closed = false)
    {
        if (closed)
        {
            DrawCircleSectorLinesClosedInternal(c.Center, c.Radius, startAngleDeg, endAngleDeg, lineThickness, color, smoothness);
        }
        else
        {
            DrawCircleSectorLinesOpenInternal(c.Center, c.Radius, startAngleDeg, endAngleDeg, lineThickness, color, LineCapType.None, 0, smoothness);
        }
    }
    
    /// <summary>
    /// Draws the outline of a sector of a circle using the specified <see cref="Circle"/> instance, start and end angles, rotation offset, number of sides, line drawing info, and optional closure.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="startAngleDeg">The starting angle of the sector in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the sector in degrees.</param>
    /// <param name="rotOffsetDeg">The rotation offset in degrees.</param>
    /// <param name="sides">The number of sides used to approximate the sector.</param>
    /// <param name="lineInfo">The line drawing parameters.</param>
    /// <param name="closed">Whether the sector should be closed (connect to the center).</param>
    public static void DrawSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, LineDrawingInfo lineInfo, float smoothness = 0.5f, bool closed = false)
    {
        DrawSectorLines(c, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, lineInfo, smoothness, closed);
    }
    public static void DrawSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, float lineThickness, ColorRgba color, float smoothness = 0.5f, bool closed = false)
    {
        DrawSectorLines(c, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, lineThickness, color, smoothness, closed);
    }
    #endregion
    
    #region Internal
    private static void DrawCircleLinesInternal(Vector2 center, float radius, float rotDeg, float lineThickness, ColorRgba color, float smoothness)
    {
        if (!CalculateCircleDrawingParameters(radius, smoothness, out float angleStep, out int sides)) return;
        var rotRad = rotDeg * ShapeMath.DEGTORAD;
        for (int i = 0; i < sides; i++)
        {
            var nextIndex = (i + 1) % sides;
            var curOuter = center + new Vector2(radius + lineThickness, 0f).Rotate(rotRad + angleStep * i);
            var nextOuter = center + new Vector2(radius + lineThickness, 0f).Rotate(rotRad + angleStep * nextIndex);
            var curInner = center + new Vector2(radius - lineThickness, 0f).Rotate(rotRad + angleStep * i); 
            var nextInner = center + new Vector2(radius - lineThickness, 0f).Rotate(rotRad + angleStep * nextIndex);
            TriangleDrawing.DrawTriangle(curOuter, curInner, nextOuter, color);
            TriangleDrawing.DrawTriangle(curInner, nextInner, nextOuter, color);
        }
    }
    
    private static void DrawCircleSectorLinesOpenInternal(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, 
        float lineThickness, ColorRgba color, LineCapType lineCapType, int capPoints, float smoothness)
    {
        
        if (!CalculateCircleDrawingParameters(radius, startAngleDeg, endAngleDeg, smoothness, out float angleDifRad, out float angleStepRad, out int sides)) return;
        // float angleDifDeg = endAngleDeg - startAngleDeg;
        // float angleDifDegAbs = MathF.Abs(angleDifDeg);
        float angleDifRadAbs = MathF.Abs(angleDifRad);
        if (angleDifRadAbs < 0.0001f) return;
        
        if (angleDifRadAbs >= MathF.Tau)
        {
            DrawCircleLinesInternal(center, radius, startAngleDeg, lineThickness, color, smoothness);
            return;
        }
        
        
        
        if(lineCapType == LineCapType.Extended || (lineCapType == LineCapType.CappedExtended && capPoints > 0))
        {
            float arcLength = Circle.ArcLengthFromAngle(MathF.Tau - angleDifRadAbs, radius);
            if (arcLength < lineThickness * 2)
            {
                DrawCircleLinesInternal(center, radius, startAngleDeg, lineThickness, color, smoothness);
                return;
            }
        }

        if (lineCapType == LineCapType.Extended)//expand angle segment
        {
            var angleExtension = Circle.ArcLengthToAngle(lineThickness, radius, true) * ShapeMath.RADTODEG;
            if (angleDifRad >= 0)
            {
                startAngleDeg -= angleExtension;
                endAngleDeg += angleExtension;
            }
            else
            {
                startAngleDeg += angleExtension;
                endAngleDeg -= angleExtension;
            }
            var angleDifDeg = endAngleDeg - startAngleDeg;
            angleStepRad = (angleDifDeg * ShapeMath.DEGTORAD) / sides;
        }
        else if (lineCapType == LineCapType.Capped) //shrink angle segment
        {
            var angleExtension = Circle.ArcLengthToAngle(lineThickness, radius, true) * ShapeMath.RADTODEG;
            if (angleDifRad >= 0)
            {
                startAngleDeg += angleExtension;
                endAngleDeg -= angleExtension;
            }
            else
            {
                startAngleDeg -= angleExtension;
                endAngleDeg += angleExtension;
            }
            var angleDifDeg = endAngleDeg - startAngleDeg;
            angleStepRad = (angleDifDeg * ShapeMath.DEGTORAD) / sides;
        }
        
        // float angleStepRad = (angleDifDeg * ShapeMath.DEGTORAD) / sides;
        float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;

        bool drawCap = capPoints > 0 && (lineCapType == LineCapType.Capped || lineCapType == LineCapType.CappedExtended);
        
        for (var i = 0; i < sides; i++)
        {
            int nextIndex = i + 1;
            float curAngleRad = startAngleRad + angleStepRad * i;
            float nextAngleRad = startAngleRad + angleStepRad * nextIndex;
            
            Vector2 curOuter, nextOuter ,curInner, nextInner;
            
            if (angleStepRad < 0)
            {
                curInner = center + new Vector2(radius + lineThickness, 0f).Rotate(curAngleRad);
                curOuter = center + new Vector2(radius - lineThickness, 0f).Rotate(curAngleRad); 
                nextInner = center + new Vector2(radius + lineThickness, 0f).Rotate(nextAngleRad);
                nextOuter = center + new Vector2(radius - lineThickness, 0f).Rotate(nextAngleRad);
            }
            else
            {
                curOuter = center + new Vector2(radius + lineThickness, 0f).Rotate(curAngleRad);
                nextOuter = center + new Vector2(radius + lineThickness, 0f).Rotate(nextAngleRad);
                curInner = center + new Vector2(radius - lineThickness, 0f).Rotate(curAngleRad); 
                nextInner = center + new Vector2(radius - lineThickness, 0f).Rotate(nextAngleRad);
            }
            TriangleDrawing.DrawTriangle(curOuter, curInner, nextOuter, color);
            TriangleDrawing.DrawTriangle(curInner, nextInner, nextOuter, color);
            
            //Draw Caps
            if (drawCap && (i == 0 || i == sides - 1))
            {
                Vector2 capStart;
                Vector2 dir;
                if (i == 0) //first segment -> draw start cap
                {
                    capStart = center + new Vector2(radius, 0f).Rotate(curAngleRad);
                    dir = (curOuter - curInner).GetPerpendicularLeft().Normalize();

                }
                else //last segment -> draw end cap
                {
                    capStart = center + new Vector2(radius, 0f).Rotate(nextAngleRad);
                    dir = (nextOuter - nextInner).GetPerpendicularRight().Normalize();
                }
                SegmentDrawing.DrawRoundCap(capStart, dir, lineThickness, capPoints, color);
            }
        }
    }
    
    private static void DrawCircleSectorLinesClosedFastInternal(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, float lineThickness, ColorRgba color, float smoothness)
    {
        DrawCircleSectorLinesOpenInternal(center, radius, startAngleDeg, endAngleDeg, lineThickness, color, LineCapType.CappedExtended, 4, smoothness);
        
        float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
        float endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
        var startDir = Vector2.UnitX.Rotate(startAngleRad);
        var endDir = Vector2.UnitX.Rotate(endAngleRad);
        
        var startPoint = center + startDir * radius;
        var endPoint = center + endDir * radius;
        SegmentDrawing.DrawSegment(center, startPoint, lineThickness, color, LineCapType.CappedExtended, 8);
        SegmentDrawing.DrawSegment(center, endPoint, lineThickness, color, LineCapType.CappedExtended, 8);
        
    }

    private static void DrawCircleSectorLinesClosedInternal(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, 
        float lineThickness, ColorRgba color, float smoothness, float miterLimit = 2f, bool beveled = false)
    {
        if (lineThickness <= 0f) return;
        
        if (!CalculateCircleDrawingParameters(radius, startAngleDeg, endAngleDeg, smoothness, out float angleDifRad, out float angleStep, out int sides)) return;
        // float angleDifDeg = endAngleDeg - startAngleDeg;
        bool clockwise = angleStep < 0f; // angleDifDeg < 0f;
        
        // float angleDifDegAbs = MathF.Abs(angleDifDeg);
        
        if (MathF.Abs(angleDifRad) >= MathF.Tau)
        {
            if(!clockwise) DrawCircleLinesInternal(center, radius, lineThickness, startAngleDeg, color, smoothness);
            return;
        }
        
        if (color.A >= 255)
        {
            DrawCircleSectorLinesClosedFastInternal(center, radius, startAngleDeg, endAngleDeg, lineThickness, color, smoothness);
            return;
        }
        float angleDifRadAbs = MathF.Abs(angleDifRad);
        float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
        float endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
        
        var prev = center + new Vector2(radius, 0f).Rotate(endAngleRad);
        var cur = center;
        var next = center + new Vector2(radius, 0f).Rotate(startAngleRad);
        var wPrev = cur - prev;
        var wNext = next - cur;
        float lsPrev = wPrev.LengthSquared();
        float lsNext = wNext.LengthSquared();
        if (lsPrev <= 0 || lsNext <= 0) return;

        var dirPrev = wPrev.Normalize();
        var dirNext = wNext.Normalize();
        
        Vector2 normalPrev = dirPrev.GetPerpendicularLeft();
        Vector2 normalNext = dirNext.GetPerpendicularLeft();
        
        var prevOutsideOffset = prev + normalPrev * lineThickness;
        var nextOutsideOffset = next + normalNext * lineThickness;
        
        var prevInsideOffset = prev - normalPrev * lineThickness;
        var nextInsideOffset = next - normalNext * lineThickness;
        
        var outsideCenterIntersection = RayDef.Ray.IntersectRayRay(prevOutsideOffset, dirPrev, nextOutsideOffset, -dirNext);
        if (!outsideCenterIntersection.Valid)
        {
            DrawCircleLinesInternal(center, radius, lineThickness, startAngleDeg, color, smoothness);
            return;
        }
        var outsideCenter = outsideCenterIntersection.Point;
        
        var insideCenterIntersection = RayDef.Ray.IntersectRayRay(prevInsideOffset, dirPrev, nextInsideOffset, -dirNext);
        if (!insideCenterIntersection.Valid)
        {
            return;
        }
        var insideCenter = insideCenterIntersection.Point;
        
        var prevInsideCircleIntersection = RayDef.Ray.IntersectRayCircle(insideCenter, -dirPrev, center, radius - lineThickness);
        var nextInsideCircleIntersection = RayDef.Ray.IntersectRayCircle(insideCenter, dirNext, center, radius - lineThickness);
        
        var prevOutsideCircleIntersection = RayDef.Ray.IntersectRayCircle(outsideCenter, -dirPrev, center, radius + lineThickness);
        var nextOutsideCircleIntersection = RayDef.Ray.IntersectRayCircle(outsideCenter, dirNext, center, radius + lineThickness);
        
        if(!SetIntersectionPoint(prevInsideCircleIntersection, out var prevInside)) return;
        if(!SetIntersectionPoint(nextInsideCircleIntersection, out var nextInside)) return;
        if(!SetIntersectionPoint(prevOutsideCircleIntersection, out var prevOutside)) return;
        if(!SetIntersectionPoint(nextOutsideCircleIntersection, out var nextOutside)) return;

        if (angleDifRadAbs > MathF.PI)
        {
            var ls = (prevOutside - nextOutside).LengthSquared();
            var thickness = lineThickness;
            if (ls <= thickness * thickness)
            {
                if(!clockwise) DrawCircleLinesInternal(center, radius, lineThickness, startAngleDeg, color, smoothness);
                return;
            }
        }
        else
        {
            var ls = (prevInside - nextInside).LengthSquared();
            var thickness = lineThickness * 0.5f;
            if (ls <= thickness * thickness)
            {
                if(clockwise) DrawCircleLinesInternal(center, radius, lineThickness, startAngleDeg, color, smoothness);
                return;
            }
        }
        
        var rayColor = color.ToRayColor();
        float totalMiterLengthLimit = lineThickness * 0.5f * MathF.Max(2f, miterLimit);

        
        var dis = (cur - outsideCenter).Length();
        if (miterLimit >= 2f && dis > totalMiterLengthLimit)
        {
            if (beveled)
            {
                if (angleDifRadAbs > MathF.PI)
                {
                    if (clockwise)
                    {
                        Vector2 bevelPrev = cur + normalPrev * lineThickness;
                        Vector2 bevelNext = cur + normalNext * lineThickness;
                    
                        bevelNext.Draw(4f, ColorRgba.Green, smoothness);
                        bevelPrev.Draw(4f, ColorRgba.Orange, smoothness);
                    
                        Raylib.DrawTriangle(insideCenter, nextInside, bevelNext, rayColor);
                        Raylib.DrawTriangle(insideCenter, bevelPrev, prevInside, rayColor);
                    
                        Raylib.DrawTriangle(bevelNext, nextInside, nextOutside, rayColor);
                        Raylib.DrawTriangle(bevelPrev, prevOutside, prevInside, rayColor);
                        
                        Raylib.DrawTriangle(bevelNext, bevelPrev, insideCenter, rayColor);
                    }
                    else
                    {
                        Vector2 bevelPrev = cur - normalPrev * lineThickness;
                        Vector2 bevelNext = cur - normalNext * lineThickness;
                    
                        Raylib.DrawTriangle(outsideCenter, bevelNext, nextOutside, rayColor);
                        Raylib.DrawTriangle(outsideCenter, prevOutside, bevelPrev, rayColor);
                    
                        Raylib.DrawTriangle(bevelNext, nextInside, nextOutside, rayColor);
                        Raylib.DrawTriangle(bevelPrev, prevOutside, prevInside, rayColor);
                        Raylib.DrawTriangle(bevelPrev, bevelNext, outsideCenter, rayColor);
                    }
                    
                }
                else
                {
                    if (clockwise)
                    {
                        Vector2 bevelPrev = cur - normalPrev * lineThickness;
                        Vector2 bevelNext = cur - normalNext * lineThickness;
                        
                        Raylib.DrawTriangle(prevOutside, bevelPrev,  outsideCenter, rayColor);
                        Raylib.DrawTriangle(outsideCenter, bevelNext, nextOutside, rayColor);
                        
                        Raylib.DrawTriangle(bevelNext, nextInside, nextOutside, rayColor);
                        Raylib.DrawTriangle(bevelPrev, prevOutside, prevInside, rayColor);
                        Raylib.DrawTriangle(bevelNext, outsideCenter, bevelPrev, rayColor);
                    }
                    else
                    {
                        Vector2 bevelPrev = cur + normalPrev * lineThickness;
                        Vector2 bevelNext = cur + normalNext * lineThickness;
                    
                        Raylib.DrawTriangle(insideCenter, nextInside, nextOutside, rayColor);
                        Raylib.DrawTriangle(insideCenter, nextOutside, bevelNext, rayColor);
                        Raylib.DrawTriangle(insideCenter, bevelPrev, prevInside, rayColor);
                        Raylib.DrawTriangle(bevelPrev, prevOutside, prevInside, rayColor);
                        Raylib.DrawTriangle(bevelNext, bevelPrev, insideCenter, rayColor);
                    }
                    
                }
            }
            else
            {
                if (angleDifRadAbs > MathF.PI)
                {
                    if (clockwise)
                    {
                        var dir = (outsideCenter - cur).Normalize();
                        outsideCenter = cur + dir * totalMiterLengthLimit;
                        var perp = dir.GetPerpendicularLeft();
                        var intersection = RayDef.Ray.IntersectRayRay(prevOutside, dirPrev, outsideCenter, perp);
                        Vector2 bevelNext, bevelPrev;
                        if (!intersection.Valid)//bevel fallback
                        {
                            bevelPrev = cur + normalPrev * lineThickness;
                            bevelNext = cur + normalNext * lineThickness;
                        }
                        else
                        {
                            bevelPrev = intersection.Point;
                            bevelNext = outsideCenter - perp * (outsideCenter - intersection.Point).Length();
                        }
                        
                        Raylib.DrawTriangle(insideCenter, nextInside, bevelNext, rayColor);
                        Raylib.DrawTriangle(insideCenter, bevelPrev, prevInside, rayColor);
                        
                        Raylib.DrawTriangle(bevelNext, nextInside, nextOutside, rayColor);
                        Raylib.DrawTriangle(bevelPrev, prevOutside, prevInside, rayColor);
                        
                        Raylib.DrawTriangle(bevelNext, bevelPrev, insideCenter, rayColor);
                    }
                    else
                    {
                        var dir = (insideCenter - cur).Normalize();
                        insideCenter = cur + dir * totalMiterLengthLimit;
                        var perp = dir.GetPerpendicularRight();
                        var intersection = RayDef.Ray.IntersectRayRay(prevInside, dirPrev, insideCenter, perp);
                        Vector2 bevelNext, bevelPrev;
                        if (!intersection.Valid)//bevel fallback
                        {
                            bevelPrev = cur - normalPrev * lineThickness;
                            bevelNext = cur - normalNext * lineThickness;
                        }
                        else
                        {
                            bevelPrev = intersection.Point;
                            bevelNext = insideCenter - perp * (insideCenter - intersection.Point).Length();
                        }
                    
                        Raylib.DrawTriangle(outsideCenter, bevelNext, nextOutside, rayColor);
                        Raylib.DrawTriangle(outsideCenter, prevOutside, bevelPrev, rayColor);
                        Raylib.DrawTriangle(bevelNext, nextInside, nextOutside, rayColor);
                        Raylib.DrawTriangle(bevelPrev, prevOutside, prevInside, rayColor);
                        Raylib.DrawTriangle(bevelPrev, bevelNext, outsideCenter, rayColor);
                    }
                }
                else
                {
                    if (clockwise)
                    {
                        var dir = (insideCenter - cur).Normalize();
                        insideCenter = cur + dir * totalMiterLengthLimit;
                        var perp = dir.GetPerpendicularRight();
                        var intersection = RayDef.Ray.IntersectRayRay(prevInside, dirPrev, insideCenter, perp);
                        Vector2 bevelNext, bevelPrev;
                        if (!intersection.Valid)//bevel fallback
                        {
                            bevelPrev = cur - normalPrev * lineThickness;
                            bevelNext = cur - normalNext * lineThickness;
                        }
                        else
                        {
                            bevelPrev = intersection.Point;
                            bevelNext = insideCenter - perp * (insideCenter - intersection.Point).Length();
                        }
                        
                        Raylib.DrawTriangle(bevelPrev, outsideCenter, prevOutside, rayColor);
                        Raylib.DrawTriangle(bevelPrev, prevOutside, prevInside, rayColor);
                        
                        Raylib.DrawTriangle(bevelNext, nextInside, nextOutside, rayColor);
                        Raylib.DrawTriangle(outsideCenter, bevelNext, nextOutside, rayColor);
                        Raylib.DrawTriangle(bevelPrev, bevelNext, outsideCenter, rayColor);
                    }
                    else
                    {
                        var dir = (outsideCenter - cur).Normalize();
                        outsideCenter = cur + dir * totalMiterLengthLimit;
                        var perp = dir.GetPerpendicularLeft();
                        var intersection = RayDef.Ray.IntersectRayRay(prevOutside, dirPrev, outsideCenter, perp);
                        Vector2 bevelNext, bevelPrev;
                        if (!intersection.Valid)//bevel fallback
                        {
                            bevelPrev = cur + normalPrev * lineThickness;
                            bevelNext = cur + normalNext * lineThickness;
                        }
                        else
                        {
                            bevelPrev = intersection.Point;
                            bevelNext = outsideCenter - perp * (outsideCenter - intersection.Point).Length();
                        }
                    
                        Raylib.DrawTriangle(insideCenter, nextInside, nextOutside, rayColor);
                        Raylib.DrawTriangle(insideCenter, nextOutside, bevelNext, rayColor);
                        Raylib.DrawTriangle(insideCenter, bevelPrev, prevInside, rayColor);
                        Raylib.DrawTriangle(bevelPrev, prevOutside, prevInside, rayColor);
                        Raylib.DrawTriangle(bevelNext, bevelPrev, insideCenter, rayColor);
                    }
                }
            }
        }
        else
        {
            Raylib.DrawTriangle(insideCenter, nextInside, nextOutside, rayColor);
            Raylib.DrawTriangle(insideCenter, nextOutside, outsideCenter, rayColor);
            Raylib.DrawTriangle(insideCenter, outsideCenter, prevInside, rayColor);
            Raylib.DrawTriangle(outsideCenter, prevOutside, prevInside, rayColor);
        }
        
        
        var curDir = (prevInside - center).Normalize();
        var endDir = (nextInside - center).Normalize();
        
        var angleRad = ShapeVec.AngleRad(endDir, curDir);
        if (angleRad > 0)
        {
            angleRad *= -1;
        }
        else
        {
            angleRad = -MathF.Tau - angleRad;
        }
        
        var angleStepRad = angleRad / sides;
        
        var prevInner = prevInside;
        var prevOuter = center + curDir * (radius + lineThickness);

        TriangleDrawing.DrawTriangle(prevInside, prevOutside, prevOuter, color);
        
        for (var i = 0; i < sides; i++)
        {
            curDir = curDir.Rotate(angleStepRad);
            var curInner = center + curDir * (radius - lineThickness);
            var curOuter = center + curDir * (radius + lineThickness);
            
            TriangleDrawing.DrawTriangle(prevInner, prevOuter, curOuter, color);
            TriangleDrawing.DrawTriangle(prevInner, curOuter, curInner, color);

            prevInner = curInner;
            prevOuter = curOuter;
        }
        
        TriangleDrawing.DrawTriangle(prevInner, prevOuter, nextOutside, color);
        
    }
   
    #endregion
    
    #region Helper
    //TODO: Add docs

    //Q: This could be used for automatic ajustment of circle resolution (smoothness)? It tells the engine what a small circle and what a big circle is.
    // - If used how to enabled/disable? Globally in CircleDrawing maybe? (then it is hard to enabled for certain circles only)
    // - Everything smaller than min could use DrawFast?
    public static ValueRange RadiusRange = new ValueRange(4f, 1000f);
    
    
    
    //Q: If calculated sideLength is too big for arc length and results in 1 or less steps, half sideLength for better quality?
    //Q: Add global min/max circle sides ?
    //Q: Add min side parameter (cant go under that and affects angleStepRad) instead or alongside with global min/max circle sides?
    public static ValueRange CircleSideLengthRange = new ValueRange(4f, 75f);
    public static bool CalculateCircleDrawingParameters(float radius, float startAngleDeg, float endAngleDeg, float smoothness, out float angleDifRad, out float angleStepRad, out int steps)
    {
        angleStepRad = 0f;
        steps = 0;
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

        // Calculate steps (at least 1)
        steps = Math.Max(1, (int)MathF.Ceiling(arcLength / sideLength));

        // Angle step in radians (preserve direction)
        angleStepRad = (angleDiffDeg * ShapeMath.DEGTORAD) / steps;

        return true;
    }
    public static bool CalculateCircleDrawingParameters(float radius, float startAngleDeg, float endAngleDeg, float smoothness, out float angleStepRad, out int steps)
    {
        angleStepRad = 0f;
        steps = 0;
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

        // Calculate steps (at least 1)
        steps = Math.Max(1, (int)MathF.Ceiling(arcLength / sideLength));

        // Angle step in radians (preserve direction)
        angleStepRad = (angleDiffDeg * ShapeMath.DEGTORAD) / steps;

        return true;
    }
    public static bool CalculateCircleDrawingParameters(float radius, float startAngleDeg, float endAngleDeg, float smoothness, out int steps)
    {
        steps = 0;
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

        // Calculate steps (at least 1)
        steps = Math.Max(1, (int)MathF.Ceiling(arcLength / sideLength));
        
        return true;
    }
    public static bool CalculateCircleDrawingParameters(float radius, float smoothness, out float angleStepRad, out int steps)
    {
        steps = 0;
        angleStepRad = 0f;
        if(radius <= 0f) return false;
        
        smoothness = ShapeMath.Clamp(smoothness, 0f, 1f);
        float sideLength = CircleSideLengthRange.LerpInverse(smoothness);
        if(sideLength <= 0f) return false;
        
        float circumference = 2.0f * ShapeMath.PI * radius;
        steps = Math.Max(1, (int)MathF.Ceiling(circumference / sideLength));
        
        angleStepRad = MathF.Tau / steps;
        return true;
    }
    public static bool CalculateCircleDrawingParameters(float radius, float smoothness, out int steps)
    {
        steps = 0;
        if(radius <= 0f) return false;
        
        smoothness = ShapeMath.Clamp(smoothness, 0f, 1f);
        float sideLength = CircleSideLengthRange.LerpInverse(smoothness);
        if(sideLength <= 0f) return false;
        
        float circumference = 2.0f * ShapeMath.PI * radius;
        steps = Math.Max(1, (int)MathF.Ceiling(circumference / sideLength));
        Console.WriteLine($"Circumference: {circumference}, SideLength: {sideLength}, Steps: {steps}");
        return true;
    }
    
    //TODO: Remove? ----------------
    
    /// <summary>
    /// Calculates the number of sides needed to approximate a circle with the given radius and maximum side length.
    /// </summary>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="maxLength">The maximum length of each side. Default is 10.</param>
    /// <returns>The number of sides to use for the circle.</returns>
    public static int GetCircleSideCount(float radius, float maxLength = 10f)
    {
        float circumference = 2.0f * ShapeMath.PI * radius;
        return (int)MathF.Max(circumference / maxLength, 5);
    }

    /// <summary>
    /// Calculates the number of sides needed to approximate an arc of a circle with the given radius, angle, and maximum side length.
    /// </summary>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="angleDeg">The angle of the arc in degrees.</param>
    /// <param name="maxLength">The maximum length of each side. Default is 10.</param>
    /// <returns>The number of sides to use for the arc.</returns>
    public static int GetCircleArcSideCount(float radius, float angleDeg, float maxLength = 10f)
    {
        float circumference = 2.0f * ShapeMath.PI * radius * (angleDeg / 360f);
        return (int)MathF.Max(circumference / maxLength, 1);
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
    private static bool SetIntersectionPoint((IntersectionPoint a, IntersectionPoint b) intersection, out Vector2 result)
    {
        if (intersection.a.Valid)
        {
            result = intersection.a.Point;
            return true;
        }
        else if (intersection.b.Valid)
        {
            result = intersection.b.Point;
            return true;
        }
        else
        {
            result = Vector2.Zero;
            return false;
        }
    }

    //TODO: --------------------------
    #endregion
}