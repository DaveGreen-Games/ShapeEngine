using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.CircleDef;

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
    /// <summary>
    /// Draws a filled circle at the specified center with the given radius and color.
    /// </summary>
    /// <param name="center">The center position of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="color">The color of the circle.</param>
    /// <param name="segments">The number of segments used to approximate the circle. Minimum is 3.</param>
    public static void DrawCircle(Vector2 center, float radius, ColorRgba color, int segments = 16)
    {
        if (segments < 3) segments = 3;
        Raylib.DrawCircleSector(center, radius, 0, 360, segments, color.ToRayColor());
    }

    /// <summary>
    /// Draws a filled circle at the specified center with the given radius, color, and rotation.
    /// </summary>
    /// <param name="center">The center position of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="color">The color of the circle.</param>
    /// <param name="rotDeg">The rotation of the circle in degrees.</param>
    /// <param name="segments">The number of segments used to approximate the circle. Minimum is 3.</param>
    public static void DrawCircle(Vector2 center, float radius, ColorRgba color, float rotDeg, int segments = 16)
    {
        if (segments < 3) segments = 3;
        Raylib.DrawCircleSector(center, radius, rotDeg, 360 + rotDeg, segments, color.ToRayColor());
    }

    /// <summary>
    /// Draws a filled circle using the specified <see cref="Circle"/> instance, color, rotation, and segment count.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="color">The color of the circle.</param>
    /// <param name="rotDeg">The rotation of the circle in degrees.</param>
    /// <param name="segments">The number of segments used to approximate the circle. Minimum is 3.</param>
    public static void Draw(this Circle c, ColorRgba color, float rotDeg, int segments = 16)
    {
        if (segments < 3) segments = 3;
        Raylib.DrawCircleSector(c.Center, c.Radius, rotDeg, 360 + rotDeg, segments, color.ToRayColor());
    }

    /// <summary>
    /// Draws a filled circle using the specified <see cref="Circle"/> instance and color.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="color">The color of the circle.</param>
    public static void Draw(this Circle c, ColorRgba color) => DrawCircle(c.Center, c.Radius, color);

    /// <summary>
    /// Draws a filled circle using the specified <see cref="Circle"/> instance, color, and segment count.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="color">The color of the circle.</param>
    /// <param name="segments">The number of segments used to approximate the circle.</param>
    public static void Draw(this Circle c, ColorRgba color, int segments) => DrawCircle(c.Center, c.Radius, color, segments);

    /// <summary>
    /// Draws the outline of a circle using the specified line thickness, number of sides, and color.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="sides">The number of sides used to approximate the circle.</param>
    /// <param name="color">The color of the outline.</param>
    public static void DrawLines(this Circle c, float lineThickness, int sides, ColorRgba color) => Raylib.DrawPolyLinesEx(c.Center, sides, c.Radius, 0f, lineThickness * 2, color.ToRayColor());

    /// <summary>
    /// Draws the outline of a circle using the specified line drawing info and number of sides.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="lineInfo">The line drawing parameters.</param>
    /// <param name="sides">The number of sides used to approximate the circle.</param>
    public static void DrawLines(this Circle c, LineDrawingInfo lineInfo, int sides) => DrawLines(c, lineInfo, 0f, sides);

    /// <summary>
    /// Draws the outline of a circle using the specified line drawing info, rotation, and number of sides.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="lineInfo">The line drawing parameters.</param>
    /// <param name="rotDeg">The rotation of the circle in degrees.</param>
    /// <param name="sides">The number of sides used to approximate the circle.</param>
    public static void DrawLines(this Circle c, LineDrawingInfo lineInfo, float rotDeg, int sides)
    {
        if (sides < 3) sides = 3;
        var angleStep = (2f * ShapeMath.PI) / sides;
        var rotRad = rotDeg * ShapeMath.DEGTORAD;
        for (int i = 0; i < sides; i++)
        {
            var nextIndex = (i + 1) % sides;
            var curP = c.Center + new Vector2(c.Radius, 0f).Rotate(rotRad + angleStep * i);
            var nextP = c.Center + new Vector2(c.Radius, 0f).Rotate(rotRad + angleStep * nextIndex);

            SegmentDrawing.DrawSegment(curP, nextP, lineInfo);
        }
    }

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
    public static void DrawLinesPercentage(this Circle c, float f, float lineThickness, float rotDeg, int sides, ColorRgba color, LineCapType lineCapType, int capPoints)
    {
        if (sides < 3 || f == 0) return;

        DrawCircleLinesPercentage(c.Center, c.Radius, f, lineThickness, rotDeg, sides, color, lineCapType, capPoints);
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
    public static void DrawLinesPercentage(this Circle c, float f, LineDrawingInfo lineInfo, float rotDeg, int sides)
    {
        DrawLinesPercentage(c, f, lineInfo.Thickness, rotDeg, sides, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
    }

    /// <summary>
    /// Draws the outline of a circle using the specified line thickness, rotation, number of sides, and color.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="rotDeg">The rotation of the circle in degrees.</param>
    /// <param name="sides">The number of sides used to approximate the circle.</param>
    /// <param name="color">The color of the outline.</param>
    public static void DrawLines(this Circle c, float lineThickness, float rotDeg, int sides, ColorRgba color) => Raylib.DrawPolyLinesEx(c.Center, sides, c.Radius, rotDeg, lineThickness * 2, color.ToRayColor());

    /// <summary>
    /// Draws the outline of a circle using the specified line thickness and color, automatically determining the number of sides based on side length.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="sideLength">The maximum length of each side. Default is 8.</param>
    public static void DrawLines(this Circle c, float lineThickness, ColorRgba color, float sideLength = 8f)
    {
        int sides = GetCircleSideCount(c.Radius, sideLength);
        Raylib.DrawPolyLinesEx(c.Center, sides, c.Radius, 0f, lineThickness * 2, color.ToRayColor());
    }

    /// <summary>
    /// Draws the outline of a circle using the specified line drawing info and rotation, automatically determining the number of sides based on side length.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="lineInfo">The line drawing parameters.</param>
    /// <param name="rotDeg">The rotation of the circle in degrees.</param>
    /// <param name="sideLength">The maximum length of each side. Default is 8.</param>
    public static void DrawLines(this Circle c, LineDrawingInfo lineInfo, float rotDeg, float sideLength = 8f)
    {
        int sides = GetCircleSideCount(c.Radius, sideLength);
        DrawLines(c, lineInfo, rotDeg, sides);
    }

    /// <summary>
    /// Draws a partial outline of a circle based on the specified percentage, line thickness, and color, automatically determining the number of sides.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="f">The percentage of the outline to draw.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="sideLength">The maximum length of each side. Default is 8.</param>
    public static void DrawLinesPercentage(this Circle c, float f, float lineThickness, ColorRgba color, float sideLength = 8f)
    {
        if (f == 0) return;
        int sides = GetCircleSideCount(c.Radius, sideLength);
        DrawLinesPercentage(c, f, lineThickness, 0f, sides, color, LineCapType.None, 0);
    }

    /// <summary>
    /// Draws a partial outline of a circle based on the specified percentage, line thickness, rotation, color, cap type, and cap points, automatically determining the number of sides.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="f">The percentage of the outline to draw.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="rotDeg">The rotation of the circle in degrees.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="capType">The type of line cap to use at the ends.</param>
    /// <param name="capPoints">The number of points used to draw the end cap.</param>
    /// <param name="sideLength">The maximum length of each side. Default is 8.</param>
    public static void DrawLinesPercentage(this Circle c, float f, float lineThickness, float rotDeg, ColorRgba color, LineCapType capType, int capPoints, float sideLength = 8f)
    {
        if (f == 0) return;
        int sides = GetCircleSideCount(c.Radius, sideLength);
        DrawLinesPercentage(c, f, lineThickness, rotDeg, sides, color, capType, capPoints);
    }

    /// <summary>
    /// Draws a partial outline of a circle based on the specified percentage, line drawing info, and rotation, automatically determining the number of sides.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="f">The percentage of the outline to draw.</param>
    /// <param name="lineInfo">The line drawing parameters.</param>
    /// <param name="rotDeg">The rotation of the circle in degrees.</param>
    /// <param name="sideLength">The maximum length of each side. Default is 8.</param>
    public static void DrawLinesPercentage(this Circle c, float f, LineDrawingInfo lineInfo, float rotDeg, float sideLength = 8f)
    {
        if (f == 0) return;
        int sides = GetCircleSideCount(c.Radius, sideLength);
        DrawLinesPercentage(c, f, lineInfo, rotDeg, sides);
    }

    /// <summary>
    /// Draws a circle outline where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="lineInfo">The line drawing parameters.</param>
    /// <param name="sides">The number of sides used to approximate the circle.</param>
    /// <param name="sideScaleFactor">The scale factor for each side (0 = no circle, 1 = normal circle).</param>
    /// <param name="sideScaleOrigin">The point along each circle segment to scale from in both directions (0-1, default 0.5).</param>
    public static void DrawLinesScaled(this Circle c, LineDrawingInfo lineInfo, int sides, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        DrawLinesScaled(c, lineInfo, 0f, sides, sideScaleFactor, sideScaleOrigin);
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
    public static void DrawLinesScaled(this Circle c, LineDrawingInfo lineInfo, float rotDeg, int sides, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0f) return;
        if (sideScaleFactor >= 1f)
        {
            DrawLines(c, lineInfo, sides);
            return;
        }

        var angleStep = (2f * ShapeMath.PI) / sides;
        var rotRad = rotDeg * ShapeMath.DEGTORAD;

        for (int i = 0; i < sides; i++)
        {
            var nextIndex = (i + 1) % sides;
            var start = c.Center + new Vector2(c.Radius, 0f).Rotate(rotRad + angleStep * i);
            var end = c.Center + new Vector2(c.Radius, 0f).Rotate(rotRad + angleStep * nextIndex);

            SegmentDrawing.DrawSegment(start, end, lineInfo, sideScaleFactor, sideScaleOrigin);
        }
    }

    /// <summary>
    /// Draws part of a circle outline depending on f.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
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
    public static void DrawCircleLinesPercentage(Vector2 center, float radius, float f, float lineThickness, float rotDeg, int sides, ColorRgba color, LineCapType lineCapType, int capPoints)
    {
        if (sides < 3 || f == 0 || radius <= 0) return;

        float angleStep; // = (2f * ShapeMath.PI) / sides;
        float percentage; // = ShapeMath.Clamp(negative ? f * -1 : f, 0f, 1f);
        if (f < 0)
        {
            angleStep = (-2f * ShapeMath.PI) / sides;
            percentage = ShapeMath.Clamp(-f, 0f, 1f);
        }
        else
        {
            angleStep = (2f * ShapeMath.PI) / sides;
            percentage = ShapeMath.Clamp(f, 0f, 1f);
        }

        var rotRad = rotDeg * ShapeMath.DEGTORAD;
        var perimeter = Circle.GetCircumference(radius);
        var sideLength = perimeter / sides;
        var perimeterToDraw = perimeter * percentage;
        for (int i = 0; i < sides; i++)
        {
            var nextIndex = (i + 1) % sides;
            var curP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * i);
            var nextP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * nextIndex);

            if (sideLength > perimeterToDraw)
            {
                nextP = curP.Lerp(nextP, perimeterToDraw / sideLength);
                SegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, lineCapType, capPoints);
                return;
            }
            else
            {
                SegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, lineCapType, capPoints);
                perimeterToDraw -= sideLength;
            }
        }
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
    public static void DrawCircleFast(Vector2 center, float radius, ColorRgba color)
    {
        RectDrawing.DrawRect(center - new Vector2(radius, radius), center + new Vector2(radius, radius), color);
    }

    /// <summary>
    /// Draws the outline of a circle at the specified center and radius using the given line thickness, number of sides, and color.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="sides">The number of sides used to approximate the circle.</param>
    /// <param name="color">The color of the outline.</param>
    public static void DrawCircleLines(Vector2 center, float radius, float lineThickness, int sides, ColorRgba color)
        => Raylib.DrawPolyLinesEx(center, sides, radius, 0f, lineThickness * 2, color.ToRayColor());

    /// <summary>
    /// Draws the outline of a circle at the specified center and radius using the given line drawing info and number of sides.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="lineInfo">The line drawing parameters.</param>
    /// <param name="sides">The number of sides used to approximate the circle.</param>
    public static void DrawCircleLines(Vector2 center, float radius, LineDrawingInfo lineInfo, int sides)
        => DrawCircleLines(center, radius, lineInfo, 0f, sides);

    /// <summary>
    /// Draws a partial outline of a circle at the specified center and radius based on the given percentage and line drawing info.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="f">The percentage of the outline to draw.</param>
    /// <param name="lineInfo">The line drawing parameters.</param>
    /// <param name="rotDeg">The rotation of the circle in degrees.</param>
    /// <param name="sides">The number of sides used to approximate the circle.</param>
    public static void DrawCircleLinesPercentage(Vector2 center, float radius, float f, LineDrawingInfo lineInfo, float rotDeg, int sides)
        => DrawCircleLinesPercentage(center, radius, f, lineInfo.Thickness, rotDeg, sides, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);

    /// <summary>
    /// Draws the outline of a circle at the specified center and radius using the given line thickness, rotation, number of sides, and color.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="rotDeg">The rotation of the circle in degrees.</param>
    /// <param name="sides">The number of sides used to approximate the circle.</param>
    /// <param name="color">The color of the outline.</param>
    public static void DrawCircleLines(Vector2 center, float radius, float lineThickness, float rotDeg, int sides, ColorRgba color)
        => Raylib.DrawPolyLinesEx(center, sides, radius, rotDeg, lineThickness * 2, color.ToRayColor());

    /// <summary>
    /// Draws the outline of a circle at the specified center and radius using the given line drawing info, rotation, and number of sides.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="lineInfo">The line drawing parameters.</param>
    /// <param name="rotDeg">The rotation of the circle in degrees.</param>
    /// <param name="sides">The number of sides used to approximate the circle.</param>
    public static void DrawCircleLines(Vector2 center, float radius, LineDrawingInfo lineInfo, float rotDeg, int sides)
    {
        if (sides < 3) return;
        var angleStep = (2f * ShapeMath.PI) / sides;
        var rotRad = rotDeg * ShapeMath.DEGTORAD;

        for (int i = 0; i < sides; i++)
        {
            var nextIndex = (i + 1) % sides;
            var curP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * i);
            var nextP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * nextIndex);

            SegmentDrawing.DrawSegment(curP, nextP, lineInfo);
        }
    }

    /// <summary>
    /// Draws the outline of a circle at the specified center and radius using the given line thickness and color, automatically determining the number of sides.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="sideLength">The maximum length of each side. Default is 8.</param>
    public static void DrawCircleLines(Vector2 center, float radius, float lineThickness, ColorRgba color, float sideLength = 8f)
    {
        int sides = GetCircleSideCount(radius, sideLength);
        Raylib.DrawPolyLinesEx(center, sides, radius, 0f, lineThickness * 2, color.ToRayColor());
    }

    /// <summary>
    /// Draws the outline of a circle at the specified center and radius using the given line drawing info and rotation, automatically determining the number of sides.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="lineInfo">The line drawing parameters.</param>
    /// <param name="rotDeg">The rotation of the circle in degrees.</param>
    /// <param name="sideLength">The maximum length of each side. Default is 8.</param>
    public static void DrawCircleLines(Vector2 center, float radius, LineDrawingInfo lineInfo, float rotDeg, float sideLength = 8f)
    {
        int sides = GetCircleSideCount(radius, sideLength);
        DrawCircleLines(center, radius, lineInfo, rotDeg, sides);
    }

    /// <summary>
    /// Draws a partial outline of a circle at the specified center and radius based on the given percentage, line drawing info, and rotation, automatically determining the number of sides.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="f">The percentage of the outline to draw.</param>
    /// <param name="lineInfo">The line drawing parameters.</param>
    /// <param name="rotDeg">The rotation of the circle in degrees.</param>
    /// <param name="sideLength">The maximum length of each side. Default is 8.</param>
    public static void DrawCircleLinesPercentage(Vector2 center, float radius, float f, LineDrawingInfo lineInfo, float rotDeg, float sideLength = 8f)
    {
        int sides = GetCircleSideCount(radius, sideLength);
        DrawCircleLinesPercentage(center, radius, f, lineInfo, rotDeg, sides);
    }

    /// <summary>
    /// Draws a filled sector of a circle using the specified <see cref="Circle"/> instance, start and end angles, segment count, and color.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="startAngleDeg">The starting angle of the sector in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the sector in degrees.</param>
    /// <param name="segments">The number of segments used to approximate the sector.</param>
    /// <param name="color">The color of the sector.</param>
    public static void DrawSector(this Circle c, float startAngleDeg, float endAngleDeg, int segments, ColorRgba color)
    {
        Raylib.DrawCircleSector(c.Center, c.Radius, startAngleDeg, endAngleDeg, segments, color.ToRayColor());
    }

    /// <summary>
    /// Draws a filled sector of a circle at the specified center and radius.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="startAngleDeg">The starting angle of the sector in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the sector in degrees.</param>
    /// <param name="segments">The number of segments used to approximate the sector.</param>
    /// <param name="color">The color of the sector.</param>
    public static void DrawCircleSector(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, int segments, ColorRgba color)
    {
        Raylib.DrawCircleSector(center, radius, startAngleDeg, endAngleDeg, segments, color.ToRayColor());
    }

    /// <summary>
    /// Draws the outline of a sector of a circle using the specified <see cref="Circle"/> instance, start and end angles, line drawing info, and optional closure and side length.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="startAngleDeg">The starting angle of the sector in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the sector in degrees.</param>
    /// <param name="lineInfo">The line drawing parameters.</param>
    /// <param name="closed">Whether the sector should be closed (connect to the center).</param>
    /// <param name="sideLength">The maximum length of each side. Default is 8.</param>
    public static void DrawSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, LineDrawingInfo lineInfo, bool closed = true, float sideLength = 8f)
    {
        DrawCircleSectorLines(c.Center, c.Radius, startAngleDeg, endAngleDeg, lineInfo, closed, sideLength);
    }

    /// <summary>
    /// Draws the outline of a sector of a circle using the specified <see cref="Circle"/> instance, start and end angles, rotation offset, line drawing info, and optional closure and side length.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="startAngleDeg">The starting angle of the sector in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the sector in degrees.</param>
    /// <param name="rotOffsetDeg">The rotation offset in degrees.</param>
    /// <param name="lineInfo">The line drawing parameters.</param>
    /// <param name="closed">Whether the sector should be closed (connect to the center).</param>
    /// <param name="sideLength">The maximum length of each side. Default is 8.</param>
    public static void DrawSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, LineDrawingInfo lineInfo, bool closed = true, float sideLength = 8f)
    {
        DrawCircleSectorLines(c.Center, c.Radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, lineInfo, closed, sideLength);
    }

    /// <summary>
    /// Draws the outline of a sector of a circle using the specified <see cref="Circle"/> instance, start and end angles, number of sides, line drawing info, and optional closure.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="startAngleDeg">The starting angle of the sector in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the sector in degrees.</param>
    /// <param name="sides">The number of sides used to approximate the sector.</param>
    /// <param name="lineInfo">The line drawing parameters.</param>
    /// <param name="closed">Whether the sector should be closed (connect to the center).</param>
    public static void DrawSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, int sides, LineDrawingInfo lineInfo, bool closed = true)
    {
        DrawCircleSectorLines(c.Center, c.Radius, startAngleDeg, endAngleDeg, sides, lineInfo, closed);
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
    public static void DrawSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, int sides, LineDrawingInfo lineInfo, bool closed = true)
    {
        DrawCircleSectorLines(c.Center, c.Radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, sides, lineInfo, closed);
    }

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
    public static void DrawSectorLinesScaled(this Circle c, LineDrawingInfo lineInfo, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, int sides, float sideScaleFactor, float sideScaleOrigin = 0.5f, bool closed = true)
    {
        if (sideScaleFactor <= 0f) return;
        if (sideScaleFactor >= 1f)
        {
            DrawSectorLines(c, startAngleDeg, endAngleDeg, rotOffsetDeg, sides, lineInfo, closed);
            return;
        }

        float startAngleRad = (startAngleDeg + rotOffsetDeg) * ShapeMath.DEGTORAD;
        float endAngleRad = (endAngleDeg + rotOffsetDeg) * ShapeMath.DEGTORAD;
        float anglePiece = endAngleRad - startAngleRad;
        float angleStep = anglePiece / sides;
        if (closed)
        {
            var sectorStart = c.Center + (ShapeVec.Right() * c.Radius).Rotate(startAngleRad);
            SegmentDrawing.DrawSegment(c.Center, sectorStart, lineInfo, sideScaleFactor, sideScaleOrigin);

            var sectorEnd = c.Center + (ShapeVec.Right() * c.Radius).Rotate(endAngleRad);
            SegmentDrawing.DrawSegment(c.Center, sectorEnd, lineInfo, sideScaleFactor, sideScaleOrigin);
        }
        for (int i = 0; i < sides; i++)
        {
            var nextIndex = (i + 1) % sides;
            var start = c.Center + new Vector2(c.Radius, 0f).Rotate(startAngleRad + angleStep * i);
            var end = c.Center + new Vector2(c.Radius, 0f).Rotate(startAngleRad + angleStep * nextIndex);

            SegmentDrawing.DrawSegment(start, end, lineInfo, sideScaleFactor, sideScaleOrigin);
        }
    }

    /// <summary>
    /// Draws the outline of a sector of a circle at the specified center and radius using the given line drawing info and optional closure and side length.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="startAngleDeg">The starting angle of the sector in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the sector in degrees.</param>
    /// <param name="lineInfo">The line drawing parameters.</param>
    /// <param name="closed">Whether the sector should be closed (connect to the center).</param>
    /// <param name="sideLength">The maximum length of each side. Default is 8.</param>
    public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, LineDrawingInfo lineInfo, bool closed = true, float sideLength = 8f)
    {
        float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
        float endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
        float anglePiece = endAngleRad - startAngleRad;
        int sides = GetCircleArcSideCount(radius, MathF.Abs(anglePiece * ShapeMath.RADTODEG), sideLength);
        float angleStep = anglePiece / sides;
        if (closed)
        {
            var sectorStart = center + (ShapeVec.Right() * radius + new Vector2(lineInfo.Thickness / 2, 0)).Rotate(startAngleRad);
            SegmentDrawing.DrawSegment(center, sectorStart, lineInfo);

            var sectorEnd = center + (ShapeVec.Right() * radius + new Vector2(lineInfo.Thickness / 2, 0)).Rotate(endAngleRad);
            SegmentDrawing.DrawSegment(center, sectorEnd, lineInfo);
        }
        for (var i = 0; i < sides; i++)
        {
            var start = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * i);
            var end = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * (i + 1));
            SegmentDrawing.DrawSegment(start, end, lineInfo);
        }
    }

    /// <summary>
    /// Draws the outline of a sector of a circle at the specified center and radius using the given rotation offset, line drawing info, and optional closure and side length.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="startAngleDeg">The starting angle of the sector in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the sector in degrees.</param>
    /// <param name="rotOffsetDeg">The rotation offset in degrees.</param>
    /// <param name="lineInfo">The line drawing parameters.</param>
    /// <param name="closed">Whether the sector should be closed (connect to the center).</param>
    /// <param name="sideLength">The maximum length of each side. Default is 8.</param>
    public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, LineDrawingInfo lineInfo, bool closed = true, float sideLength = 8f)
    {
        DrawCircleSectorLines(center, radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, lineInfo, closed, sideLength);
    }

    /// <summary>
    /// Draws the outline of a sector of a circle at the specified center and radius using the given number of sides, line drawing info, and optional closure.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="startAngleDeg">The starting angle of the sector in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the sector in degrees.</param>
    /// <param name="sides">The number of sides used to approximate the sector.</param>
    /// <param name="lineInfo">The line drawing parameters.</param>
    /// <param name="closed">Whether the sector should be closed (connect to the center).</param>
    public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, int sides, LineDrawingInfo lineInfo, bool closed = true)
    {
        float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
        float endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
        float anglePiece = endAngleDeg - startAngleRad;
        float angleStep = MathF.Abs(anglePiece) / sides;
        if (closed)
        {
            var sectorStart = center + (ShapeVec.Right() * radius + new Vector2(lineInfo.Thickness / 2, 0)).Rotate(startAngleRad);
            SegmentDrawing.DrawSegment(center, sectorStart, lineInfo);

            var sectorEnd = center + (ShapeVec.Right() * radius + new Vector2(lineInfo.Thickness / 2, 0)).Rotate(endAngleRad);
            SegmentDrawing.DrawSegment(center, sectorEnd, lineInfo);
        }
        for (var i = 0; i < sides; i++)
        {
            var start = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * i);
            var end = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * (i + 1));
            SegmentDrawing.DrawSegment(start, end, lineInfo);
        }
    }

    /// <summary>
    /// Draws the outline of a sector of a circle at the specified center and radius using the given rotation offset, number of sides, line drawing info, and optional closure.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="startAngleDeg">The starting angle of the sector in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the sector in degrees.</param>
    /// <param name="rotOffsetDeg">The rotation offset in degrees.</param>
    /// <param name="sides">The number of sides used to approximate the sector.</param>
    /// <param name="lineInfo">The line drawing parameters.</param>
    /// <param name="closed">Whether the sector should be closed (connect to the center).</param>
    public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, int sides, LineDrawingInfo lineInfo, bool closed = true)
    {
        DrawCircleSectorLines(center, radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, sides, lineInfo, closed);
    }

    /// <summary>
    /// Draws the outline of a sector of a circle at the specified center and radius using the given line thickness, color, and optional closure and side length.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="startAngleDeg">The starting angle of the sector in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the sector in degrees.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="closed">Whether the sector should be closed (connect to the center).</param>
    /// <param name="sideLength">The maximum length of each side. Default is 8.</param>
    public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, float lineThickness, ColorRgba color, bool closed = true, float sideLength = 8f)
    {
        float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
        float endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
        float anglePiece = endAngleRad - startAngleRad;
        int sides = GetCircleArcSideCount(radius, MathF.Abs(anglePiece * ShapeMath.RADTODEG), sideLength);
        float angleStep = anglePiece / sides;
        if (closed)
        {
            var sectorStart = center + (ShapeVec.Right() * radius + new Vector2(lineThickness / 2, 0)).Rotate(startAngleRad);
            SegmentDrawing.DrawSegment(center, sectorStart, lineThickness, color, LineCapType.CappedExtended, 4);

            var sectorEnd = center + (ShapeVec.Right() * radius + new Vector2(lineThickness / 2, 0)).Rotate(endAngleRad);
            SegmentDrawing.DrawSegment(center, sectorEnd, lineThickness, color, LineCapType.CappedExtended, 4);
        }
        for (var i = 0; i < sides; i++)
        {
            var start = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * i);
            var end = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * (i + 1));
            SegmentDrawing.DrawSegment(start, end, lineThickness, color, LineCapType.CappedExtended, 4);
        }
    }

    /// <summary>
    /// Draws the outline of a sector of a circle at the specified center and radius using the given rotation offset, line thickness, color, and optional closure and side length.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="startAngleDeg">The starting angle of the sector in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the sector in degrees.</param>
    /// <param name="rotOffsetDeg">The rotation offset in degrees.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="closed">Whether the sector should be closed (connect to the center).</param>
    /// <param name="sideLength">The maximum length of each side. Default is 8.</param>
    public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, float lineThickness, ColorRgba color, bool closed = true, float sideLength = 8f)
    {
        DrawCircleSectorLines(center, radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, lineThickness, color, closed, sideLength);
    }

    /// <summary>
    /// Draws the outline of a sector of a circle at the specified center and radius using the given number of sides, line thickness, color, and optional closure.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="startAngleDeg">The starting angle of the sector in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the sector in degrees.</param>
    /// <param name="sides">The number of sides used to approximate the sector.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="closed">Whether the sector should be closed (connect to the center).</param>
    public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, int sides, float lineThickness, ColorRgba color, bool closed = true)
    {
        float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
        float endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
        float anglePiece = endAngleDeg - startAngleRad;
        float angleStep = MathF.Abs(anglePiece) / sides;
        if (closed)
        {
            var sectorStart = center + (ShapeVec.Right() * radius + new Vector2(lineThickness / 2, 0)).Rotate(startAngleRad);
            SegmentDrawing.DrawSegment(center, sectorStart, lineThickness, color, LineCapType.CappedExtended, 2);

            var sectorEnd = center + (ShapeVec.Right() * radius + new Vector2(lineThickness / 2, 0)).Rotate(endAngleRad);
            SegmentDrawing.DrawSegment(center, sectorEnd, lineThickness, color, LineCapType.CappedExtended, 2);
        }
        for (var i = 0; i < sides; i++)
        {
            var start = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * i);
            var end = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * (i + 1));
            SegmentDrawing.DrawSegment(start, end, lineThickness, color, LineCapType.CappedExtended, 2);
        }
    }

    /// <summary>
    /// Draws the outline of a sector of a circle at the specified center and radius using the given rotation offset, number of sides, line thickness, color, and optional closure.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="startAngleDeg">The starting angle of the sector in degrees.</param>
    /// <param name="endAngleDeg">The ending angle of the sector in degrees.</param>
    /// <param name="rotOffsetDeg">The rotation offset in degrees.</param>
    /// <param name="sides">The number of sides used to approximate the sector.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="closed">Whether the sector should be closed (connect to the center).</param>
    public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, int sides, float lineThickness, ColorRgba color, bool closed = true)
    {
        DrawCircleSectorLines(center, radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, sides, lineThickness, color, closed);
    }

    /// <summary>
    /// Draws a checkered pattern of lines inside a circle, optionally with a background color.
    /// </summary>
    /// <param name="pos">The position of the circle (before alignment).</param>
    /// <param name="alignement">The anchor point alignment for the circle.</param>
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
    public static void DrawCircleCheckeredLines(Vector2 pos, AnchorPoint alignement, float radius, float spacing, float lineThickness, float angleDeg, ColorRgba lineColorRgba, ColorRgba bgColorRgba, int circleSegments)
    {

        float maxDimension = radius;
        var size = new Vector2(radius, radius) * 2f;
        var aVector = alignement.ToVector2() * size;
        var center = pos - aVector + size / 2;
        float rotRad = angleDeg * ShapeMath.DEGTORAD;

        if (bgColorRgba.A > 0) DrawCircle(center, radius, bgColorRgba, circleSegments);

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
}