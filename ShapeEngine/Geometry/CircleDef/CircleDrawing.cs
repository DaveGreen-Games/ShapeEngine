using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;
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
    public static void DrawLinesMasked(this Circle circle, Triangle mask, LineDrawingInfo lineInfo, float rotDeg, int sides, bool reversedMask = false)
    {
        if (sides < 3) sides = 3;
        var angleStep = (2f * ShapeMath.PI) / sides;
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
    public static void DrawLinesMasked(this Circle circle, Circle mask, LineDrawingInfo lineInfo, float rotDeg, int sides, bool reversedMask = false)
    {
        if (sides < 3) sides = 3;
        var angleStep = (2f * ShapeMath.PI) / sides;
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
    public static void DrawLinesMasked(this Circle circle, Rect mask, LineDrawingInfo lineInfo, float rotDeg, int sides, bool reversedMask = false)
    {
        if (sides < 3) sides = 3;
        var angleStep = (2f * ShapeMath.PI) / sides;
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
    public static void DrawLinesMasked(this Circle circle, Quad mask, LineDrawingInfo lineInfo, float rotDeg, int sides, bool reversedMask = false)
    {
        if (sides < 3) sides = 3;
        var angleStep = (2f * ShapeMath.PI) / sides;
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
    public static void DrawLinesMasked(this Circle circle, Polygon mask, LineDrawingInfo lineInfo, float rotDeg, int sides, bool reversedMask = false)
    {
        if (sides < 3) sides = 3;
        var angleStep = (2f * ShapeMath.PI) / sides;
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
    public static void DrawLinesMasked<T>(this Circle circle, T mask, LineDrawingInfo lineInfo, float rotDeg, int sides, bool reversedMask = false) where T : IClosedShapeTypeProvider
    {
        if (sides < 3) sides = 3;
        var angleStep = (2f * ShapeMath.PI) / sides;
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

    #endregion
    
    #region Draw Lines
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
    /// <param name="lineInfo">The line drawing parameters for the drawing the circle outline.
    /// Only <see cref="LineDrawingInfo.Thickness"/> and <see cref="LineDrawingInfo.Color"/> are used!</param>
    /// <param name="rotDeg">The rotation of the circle in degrees.</param>
    /// <param name="sides">The number of sides used to approximate the circle.</param>
    public static void DrawLines(this Circle c, LineDrawingInfo lineInfo, float rotDeg, int sides)
    {
        DrawCircleLinesInternal(c.Center, c.Radius, lineInfo.Thickness, rotDeg, sides, lineInfo.Color);
        // if (sides < 3) sides = 3;
        // var angleStep = (2f * ShapeMath.PI) / sides;
        // var rotRad = rotDeg * ShapeMath.DEGTORAD;
        // for (int i = 0; i < sides; i++)
        // {
        //     var nextIndex = (i + 1) % sides;
        //     var curP = c.Center + new Vector2(c.Radius, 0f).Rotate(rotRad + angleStep * i);
        //     var nextP = c.Center + new Vector2(c.Radius, 0f).Rotate(rotRad + angleStep * nextIndex);
        //
        //     SegmentDrawing.DrawSegment(curP, nextP, lineInfo);
        // }
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
    /// <param name="lineInfo">The line drawing parameters for the drawing the circle outline.
    /// Only <see cref="LineDrawingInfo.Thickness"/> and <see cref="LineDrawingInfo.Color"/> are used!</param>
    /// <param name="rotDeg">The rotation of the circle in degrees.</param>
    /// <param name="sides">The number of sides used to approximate the circle.</param>
    public static void DrawCircleLines(Vector2 center, float radius, LineDrawingInfo lineInfo, float rotDeg, int sides)
    {
        DrawCircleLinesInternal(center, radius, lineInfo.Thickness, rotDeg, sides, lineInfo.Color);
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
        float lineThickness, float angleDeg, ColorRgba lineColorRgba, ColorRgba bgColorRgba, int circleSegments)
    {

        float maxDimension = radius;
        var size = new Vector2(radius, radius) * 2f;
        var aVector = alignment.ToVector2() * size;
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
        // DrawCircleLinesPercentageInternal(center, radius, f, lineThickness, rotDeg, sides, color, lineCapType, capPoints);
        if (TransformPercentageToAngles(f, out float startAngleDeg, out float endAngleDeg))
        {
            DrawCircleSectorLinesOpenInternal(center, radius, startAngleDeg + rotDeg, endAngleDeg + rotDeg, lineThickness, sides, color, lineCapType, capPoints);
        }
        // if (sides < 3 || f == 0 || radius <= 0) return;
        //
        // float angleStep;
        // float percentage;
        // if (f < 0)
        // {
        //     angleStep = (-2f * ShapeMath.PI) / sides;
        //     percentage = ShapeMath.Clamp(-f, 0f, 1f);
        // }
        // else
        // {
        //     angleStep = (2f * ShapeMath.PI) / sides;
        //     percentage = ShapeMath.Clamp(f, 0f, 1f);
        // }
        //
        // var rotRad = rotDeg * ShapeMath.DEGTORAD;
        // var perimeter = Circle.GetCircumference(radius);
        // var sideLength = perimeter / sides;
        // var perimeterToDraw = perimeter * percentage;
        // for (int i = 0; i < sides; i++)
        // {
        //     var nextIndex = (i + 1) % sides;
        //     var curP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * i);
        //     var nextP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * nextIndex);
        //
        //     if (sideLength > perimeterToDraw)
        //     {
        //         nextP = curP.Lerp(nextP, perimeterToDraw / sideLength);
        //         SegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, lineCapType, capPoints);
        //         return;
        //     }
        //     else
        //     {
        //         SegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, lineCapType, capPoints);
        //         perimeterToDraw -= sideLength;
        //     }
        // }
    }
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
        DrawCircleSectorLines(c.Center, c.Radius, startAngleDeg, endAngleDeg, lineInfo.Thickness, lineInfo.Color, closed, sideLength);
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
        DrawCircleSectorLines(c.Center, c.Radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, lineInfo.Thickness, lineInfo.Color, closed, sideLength);
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
        // DrawCircleSectorLinesInternal(c.Center, c.Radius, startAngleDeg, endAngleDeg, 0f, sides, lineInfo.Thickness, lineInfo.Color);
        DrawCircleSectorLines(c.Center, c.Radius, startAngleDeg, endAngleDeg, sides, lineInfo.Thickness, lineInfo.Color, closed);
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
        DrawCircleSectorLines(c.Center, c.Radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, sides, lineInfo.Thickness, lineInfo.Color, closed);
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
        DrawCircleSectorLines(center, radius, startAngleDeg, endAngleDeg, lineInfo.Thickness, lineInfo.Color, closed, sideLength);
        // float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
        // float endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
        // float anglePiece = endAngleRad - startAngleRad;
        // int sides = GetCircleArcSideCount(radius, MathF.Abs(anglePiece * ShapeMath.RADTODEG), sideLength);
        // float angleStep = anglePiece / sides;
        // if (closed)
        // {
        //     var sectorStart = center + (ShapeVec.Right() * radius + new Vector2(lineInfo.Thickness / 2, 0)).Rotate(startAngleRad);
        //     SegmentDrawing.DrawSegment(center, sectorStart, lineInfo);
        //
        //     var sectorEnd = center + (ShapeVec.Right() * radius + new Vector2(lineInfo.Thickness / 2, 0)).Rotate(endAngleRad);
        //     SegmentDrawing.DrawSegment(center, sectorEnd, lineInfo);
        // }
        // for (var i = 0; i < sides; i++)
        // {
        //     var start = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * i);
        //     var end = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * (i + 1));
        //     SegmentDrawing.DrawSegment(start, end, lineInfo);
        // }
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
        DrawCircleSectorLines(center, radius, startAngleDeg, endAngleDeg, rotOffsetDeg, lineInfo.Thickness, lineInfo.Color, closed, sideLength);
        // DrawCircleSectorLines(center, radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, lineInfo, closed, sideLength);
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
        DrawCircleSectorLines(center, radius, startAngleDeg, endAngleDeg, sides, lineInfo.Thickness, lineInfo.Color, closed);
        // float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
        // float endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
        // float anglePiece = endAngleRad - startAngleRad;
        // float angleStep = MathF.Abs(anglePiece) / sides;
        // if (closed)
        // {
        //     var sectorStart = center + (ShapeVec.Right() * radius + new Vector2(lineInfo.Thickness / 2, 0)).Rotate(startAngleRad);
        //     SegmentDrawing.DrawSegment(center, sectorStart, lineInfo);
        //
        //     var sectorEnd = center + (ShapeVec.Right() * radius + new Vector2(lineInfo.Thickness / 2, 0)).Rotate(endAngleRad);
        //     SegmentDrawing.DrawSegment(center, sectorEnd, lineInfo);
        // }
        // for (var i = 0; i < sides; i++)
        // {
        //     var start = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * i);
        //     var end = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * (i + 1));
        //     SegmentDrawing.DrawSegment(start, end, lineInfo);
        // }
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
        DrawCircleSectorLines(center, radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, sides, lineInfo.Thickness, lineInfo.Color, closed);
        // DrawCircleSectorLines(center, radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, sides, lineInfo, closed);
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
        
        if (closed)
        {
            DrawCircleSectorLinesClosedInternal(center, radius, startAngleDeg, endAngleDeg, sides, lineThickness, color);
        }
        else
        {
            DrawCircleSectorLinesOpenInternal(center, radius, startAngleDeg, endAngleDeg, sides, lineThickness, color);
        }
        // float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
        // float endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
        // float anglePiece = endAngleRad - startAngleRad;
        // int sides = GetCircleArcSideCount(radius, MathF.Abs(anglePiece * ShapeMath.RADTODEG), sideLength);
        // float angleStep = anglePiece / sides;
        // if (closed)
        // {
        //     var sectorStart = center + (ShapeVec.Right() * radius + new Vector2(lineThickness / 2, 0)).Rotate(startAngleRad);
        //     SegmentDrawing.DrawSegment(center, sectorStart, lineThickness, color, LineCapType.CappedExtended, 4);
        //
        //     var sectorEnd = center + (ShapeVec.Right() * radius + new Vector2(lineThickness / 2, 0)).Rotate(endAngleRad);
        //     SegmentDrawing.DrawSegment(center, sectorEnd, lineThickness, color, LineCapType.CappedExtended, 4);
        // }
        // for (var i = 0; i < sides; i++)
        // {
        //     var start = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * i);
        //     var end = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * (i + 1));
        //     SegmentDrawing.DrawSegment(start, end, lineThickness, color, LineCapType.CappedExtended, 4);
        // }
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
        if (closed)
        {
            DrawCircleSectorLinesClosedInternal(center, radius, startAngleDeg, endAngleDeg, sides, lineThickness, color);
        }
        else
        {
            DrawCircleSectorLinesOpenInternal(center, radius, startAngleDeg, endAngleDeg, sides, lineThickness, color);
        }
        // float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
        // float endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
        // float anglePiece = endAngleRad - startAngleRad;
        // float angleStep = MathF.Abs(anglePiece) / sides;
        // if (closed)
        // {
        //     var sectorStart = center + (ShapeVec.Right() * radius + new Vector2(lineThickness / 2, 0)).Rotate(startAngleRad);
        //     SegmentDrawing.DrawSegment(center, sectorStart, lineThickness, color, LineCapType.CappedExtended, 2);
        //
        //     var sectorEnd = center + (ShapeVec.Right() * radius + new Vector2(lineThickness / 2, 0)).Rotate(endAngleRad);
        //     SegmentDrawing.DrawSegment(center, sectorEnd, lineThickness, color, LineCapType.CappedExtended, 2);
        // }
        // for (var i = 0; i < sides; i++)
        // {
        //     var start = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * i);
        //     var end = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * (i + 1));
        //     SegmentDrawing.DrawSegment(start, end, lineThickness, color, LineCapType.CappedExtended, 2);
        // }
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
    
    private static void DrawCircleLinesInternal(Vector2 center, float radius, float lineThickness, float rotDeg, int sides, ColorRgba color)
    {
        if (sides < 3) sides = 3;
        var angleStep = (2f * ShapeMath.PI) / sides;
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
        int sides, float lineThickness, ColorRgba color)
    {
        float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
        float endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
        
        var startDir = Vector2.UnitX.Rotate(startAngleRad);
        var endDir = Vector2.UnitX.Rotate(endAngleRad);
        
        var start = center + startDir * radius;
        var end = center + endDir * radius;
        
        var gapDistanceSquared = (start - end).LengthSquared();
        if (gapDistanceSquared < lineThickness * lineThickness)
        {
            DrawCircleLines(center, radius, lineThickness, startAngleDeg, sides, color);
            return;
        }
        
        if(sides < 3) sides = 3;
        float anglePieceRad = endAngleRad - startAngleRad;

        float angleStepRad = anglePieceRad / sides;
        
        var startOuter = center + startDir * (radius + lineThickness);;
        var startInner = center + startDir * (radius - lineThickness);
        var endOuter = center + endDir * (radius + lineThickness);
        var endInner = center + endDir * (radius - lineThickness);
        
        for (var i = 0; i < sides; i++)
        {
            if (i == 0)
            {
                var nextOuter = center + new Vector2(radius + lineThickness, 0f).Rotate(startAngleRad + angleStepRad);//index 1
                var nextInner = center + new Vector2(radius - lineThickness, 0f).Rotate(startAngleRad  + angleStepRad);//index 1
                TriangleDrawing.DrawTriangle(startOuter, startInner, nextOuter, color);
                TriangleDrawing.DrawTriangle(startInner, nextInner, nextOuter, color);
            }
            else if (i == sides - 1)
            {
                var curOuter = center + new Vector2(radius + lineThickness, 0f).Rotate(startAngleRad + angleStepRad * i);
                var curInner = center + new Vector2(radius - lineThickness, 0f).Rotate(startAngleRad + angleStepRad * i);
                TriangleDrawing.DrawTriangle(curOuter, curInner, endOuter, color);
                TriangleDrawing.DrawTriangle(curInner, endInner, endOuter, color);
            }
            else
            {
                int nextIndex = i + 1;
                var curOuter = center + new Vector2(radius + lineThickness, 0f).Rotate(startAngleRad + angleStepRad * i);
                var curInner = center + new Vector2(radius - lineThickness, 0f).Rotate(startAngleRad + angleStepRad * i); 
                var nextOuter = center + new Vector2(radius + lineThickness, 0f).Rotate(startAngleRad + angleStepRad * nextIndex);
                var nextInner = center + new Vector2(radius - lineThickness, 0f).Rotate(startAngleRad  + angleStepRad * nextIndex);
                TriangleDrawing.DrawTriangle(curOuter, curInner, nextOuter, color);
                TriangleDrawing.DrawTriangle(curInner, nextInner, nextOuter, color);
            }
        }
    }

    private static void DrawCircleSectorLinesClosedInternal(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, 
        int sides, float lineThickness, ColorRgba color)
    {
        //TODO: Implement using polygon version of outline drawing
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

    private static void DrawCircleSectorLinesOpenInternal(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, float lineThickness, int sides, ColorRgba color,
        LineCapType lineCapType, int capPoints)
    {
        if (sides < 3 ||  radius <= 0) return;
        var angleDifDeg = endAngleDeg - startAngleDeg;
        if (MathF.Abs(angleDifDeg) < 0.0001f) return;
        
        float arcLength = Circle.ArcLengthFromAngle((360 - angleDifDeg) * ShapeMath.DEGTORAD, radius);
        if (arcLength < lineThickness * 2)
        {
            DrawCircleLinesInternal(center, radius, lineThickness, startAngleDeg, sides, color);
            return;
        }
        
        if(lineCapType == LineCapType.None || (lineCapType == LineCapType.Capped && capPoints <= 0) || (lineCapType == LineCapType.CappedExtended && capPoints <= 0))
        {
            DrawCircleSectorLinesOpenInternal(center, radius, startAngleDeg, endAngleDeg, sides, lineThickness, color);
            return;
        }

        if (lineCapType == LineCapType.Extended)//expand angle segment
        {
            var angleExtension = Circle.ArcLengthToAngle(lineThickness, radius, true) * ShapeMath.RADTODEG;
            if (angleDifDeg >= 0)
            {
                startAngleDeg -= angleExtension;
                endAngleDeg += angleExtension;
            }
            else
            {
                startAngleDeg += angleExtension;
                endAngleDeg -= angleExtension;
            }
        }
        else if (lineCapType == LineCapType.Capped) //shrink angle segment
        {
            var angleExtension = Circle.ArcLengthToAngle(lineThickness, radius, true) * ShapeMath.RADTODEG;
            if (angleDifDeg >= 0)
            {
                startAngleDeg += angleExtension;
                endAngleDeg -= angleExtension;
            }
            else
            {
                startAngleDeg -= angleExtension;
                endAngleDeg += angleExtension;
            }
        }
        
        float angleStepRad = (angleDifDeg * ShapeMath.DEGTORAD) / sides;
        float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
        for (var i = 0; i < sides; i++)
        {
            int nextIndex = i + 1;
            float curAngleRad = startAngleRad + angleStepRad * i;
            float nextAngleRad = startAngleRad + angleStepRad * nextIndex;
            
            //Draw Caps
            if ((i == 0 || i == sides - 1) && capPoints > 0 &&  (lineCapType == LineCapType.Capped || lineCapType == LineCapType.CappedExtended))
            {
                Vector2 capStart, capEnd;
                
                if (i == 0) //first segment -> draw start cap
                {
                    capStart = center + new Vector2(radius, 0f).Rotate(curAngleRad);
                    float arcAngle = Circle.ArcLengthToAngle(lineThickness, radius, true);
                    float endAngle = angleStepRad > 0 ? curAngleRad - arcAngle : curAngleRad + arcAngle;
                    capEnd = center + new Vector2(radius, 0f).Rotate(endAngle);
                    
                }
                else //last segment -> draw end cap
                {
                    capStart = center + new Vector2(radius, 0f).Rotate(nextAngleRad);
                    float arcAngle = Circle.ArcLengthToAngle(lineThickness, radius, true);
                    float endAngle = angleStepRad > 0 ? nextAngleRad + arcAngle : nextAngleRad - arcAngle;
                    capEnd = center + new Vector2(radius, 0f).Rotate(endAngle);
                }
                var dir = (capEnd - capStart).Normalize();
                SegmentDrawing.DrawRoundCap(capStart, dir, lineThickness, capPoints, color);
            }

            //TODO: Clean up
            var curOuter = center + new Vector2(radius + lineThickness, 0f).Rotate(curAngleRad);
            var nextOuter = center + new Vector2(radius + lineThickness, 0f).Rotate(nextAngleRad);
            var curInner = center + new Vector2(radius - lineThickness, 0f).Rotate(curAngleRad); 
            var nextInner = center + new Vector2(radius - lineThickness, 0f).Rotate(nextAngleRad);
            if (angleStepRad < 0)
            {
                (curOuter, curInner) = (curInner, curOuter);
                (nextOuter, nextInner) = (nextInner, nextOuter);
            }
            TriangleDrawing.DrawTriangle(curOuter, curInner, nextOuter, color);
            TriangleDrawing.DrawTriangle(curInner, nextInner, nextOuter, color);
        }
    }
    
    
    // private static void DrawCircleLinesPercentageInternal_OLD(Vector2 center, float radius, float f, float lineThickness, float rotDeg, int sides, ColorRgba color,
    //     LineCapType lineCapType, int capPoints)
    // {
    //     if (sides < 3 || f == 0 || radius <= 0) return;
    //
    //     float absF = MathF.Abs(f);
    //     if (absF >= 1f)
    //     {
    //         DrawCircleLinesInternal(center, radius, lineThickness, rotDeg, sides, color);
    //         return;
    //     }
    //     
    //     if (absF > 0.5f && (lineCapType == LineCapType.Extended || (lineCapType == LineCapType.CappedExtended && capPoints > 0)))
    //     {
    //         float angleSegmentRad = float.Pi * 2f * absF;
    //         float angleSegmentRadDif = float.Pi * 2f - angleSegmentRad;
    //         float arcLength = Circle.ArcLengthFromAngle(angleSegmentRadDif, radius);
    //         if (arcLength < lineThickness * 2)
    //         {
    //             DrawCircleLinesInternal(center, radius, lineThickness, rotDeg, sides, color);
    //             return;
    //         }
    //     }
    //     
    //     float angleStep = (2f * ShapeMath.PI * f) / sides;
    //     float percentage = f < 0 ? ShapeMath.Clamp(-f, 0f, 1f) : ShapeMath.Clamp(f, 0f, 1f);
    //
    //     float rotRad = rotDeg * ShapeMath.DEGTORAD;
    //     float perimeter = Circle.GetCircumference(radius);
    //     float perimeterToDraw = perimeter * percentage;
    //     float sideLength = perimeterToDraw / sides;
    //     
    //     for (var i = 0; i < sides; i++)
    //     {
    //         int nextIndex = i + 1;
    //         float curAngleStep = angleStep;
    //         if (sideLength > perimeterToDraw)//last segment
    //         {
    //             curAngleStep = angleStep * (perimeterToDraw / sideLength);
    //             perimeterToDraw = 0;
    //         }
    //         else perimeterToDraw -= sideLength;
    //
    //         float curAngleRad = rotRad + angleStep * i;
    //         float nextAngleRad = rotRad + curAngleStep * nextIndex;
    //         
    //         //Draw Caps
    //         if ((i == 0 || i == sides - 1) && lineCapType != LineCapType.None && (capPoints > 0 || lineCapType == LineCapType.Extended))
    //         {
    //             Vector2 capStart, capStartInside, capStartOutside;
    //             Vector2 capEnd, capEndInside, capEndOutside;
    //             
    //             if (i == 0) //first segment -> draw start cap
    //             {
    //                 capStart = center + new Vector2(radius, 0f).Rotate(curAngleRad);
    //                 capStartInside = center + new Vector2(radius - lineThickness, 0f).Rotate(curAngleRad);
    //                 capStartOutside = center + new Vector2(radius + lineThickness, 0f).Rotate(curAngleRad);
    //                 var arcAngle = Circle.ArcLengthToAngle(lineThickness, radius, true);
    //                 var endAngle = f > 0 ? curAngleRad - arcAngle : curAngleRad + arcAngle;
    //                 capEnd = center + new Vector2(radius, 0f).Rotate(endAngle);
    //                 capEndInside = center + new Vector2(radius + lineThickness, 0f).Rotate(endAngle);
    //                 capEndOutside = center + new Vector2(radius - lineThickness, 0f).Rotate(endAngle);
    //                 
    //             }
    //             else //last segment -> draw end cap
    //             {
    //                 capStart = center + new Vector2(radius, 0f).Rotate(nextAngleRad);
    //                 capStartInside = center + new Vector2(radius + lineThickness, 0f).Rotate(nextAngleRad);
    //                 capStartOutside = center + new Vector2(radius - lineThickness, 0f).Rotate(nextAngleRad);
    //                 var arcAngle = Circle.ArcLengthToAngle(lineThickness, radius, true);
    //                 var endAngle = f > 0 ? nextAngleRad + arcAngle : nextAngleRad - arcAngle;
    //                 capEnd = center + new Vector2(radius, 0f).Rotate(endAngle);
    //                 capEndInside = center + new Vector2(radius - lineThickness, 0f).Rotate(endAngle);
    //                 capEndOutside = center + new Vector2(radius + lineThickness, 0f).Rotate(endAngle);
    //             }
    //         
    //             if (lineCapType == LineCapType.Extended)
    //             {
    //                 if (f > 0)
    //                 {
    //                     Raylib.DrawTriangle(capStartInside, capStartOutside,capEndInside, color.ToRayColor());
    //                     Raylib.DrawTriangle(capStartInside, capEndInside, capEndOutside, color.ToRayColor());
    //                 }
    //                 else
    //                 {
    //                     Raylib.DrawTriangle(capStartOutside,capStartInside,capEndInside, color.ToRayColor());
    //                     Raylib.DrawTriangle(capEndInside, capStartInside, capEndOutside, color.ToRayColor());
    //                 }
    //                 
    //             }
    //             else
    //             {
    //                 //TODO: Add again
    //                 
    //                 // if (lineCapType == LineCapType.Capped)//shrink inwards so that the line with cap is the same length
    //                 // {
    //                 //     capEnd = capStart;
    //                 //     capStart -= capDir * lineThickness;
    //                 //     capStartLeft -= capDir * lineThickness;
    //                 //     capStartRight -= capDir * lineThickness;
    //                 //
    //                 // }
    //             
    //                 //Draw Cap
    //                 if (capPoints == 1)
    //                 {
    //                     if(f > 0) Raylib.DrawTriangle(capStartInside, capStartOutside, capEnd, color.ToRayColor());
    //                     else Raylib.DrawTriangle(capStartOutside,capStartInside, capEnd, color.ToRayColor());
    //                 }
    //                 else
    //                 {
    //                     var curStart = f < 0 ? capStartInside : capStartOutside;
    //                     float capAngleStep = (180f / (capPoints + 1)) * ShapeMath.DEGTORAD;
    //                     var dir = (curStart - capStart).Normalize();
    //                     for (var j = 1; j <= capPoints; j++)
    //                     {
    //                         var pStart = capStart + dir.Rotate(-capAngleStep * j) * lineThickness;
    //                         Raylib.DrawTriangle(pStart, capStart, curStart, color.ToRayColor());
    //                         curStart = pStart;
    //                     }
    //                     Raylib.DrawTriangle(curStart, f < 0 ? capStartOutside : capStartInside, capStart, color.ToRayColor());
    //                 }
    //             }
    //         }
    //
    //         //TODO: Clean up
    //         var curOuter = center + new Vector2(radius + lineThickness, 0f).Rotate(curAngleRad);
    //         var nextOuter = center + new Vector2(radius + lineThickness, 0f).Rotate(nextAngleRad);
    //         var curInner = center + new Vector2(radius - lineThickness, 0f).Rotate(curAngleRad); 
    //         var nextInner = center + new Vector2(radius - lineThickness, 0f).Rotate(nextAngleRad);
    //         if (f < 0)
    //         {
    //             (curOuter, curInner) = (curInner, curOuter);
    //             (nextOuter, nextInner) = (nextInner, nextOuter);
    //         }
    //         TriangleDrawing.DrawTriangle(curOuter, curInner, nextOuter, color);
    //         TriangleDrawing.DrawTriangle(curInner, nextInner, nextOuter, color);
    //     }
    // }
    //
    #endregion
    
    #region Helper
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
    #endregion
}