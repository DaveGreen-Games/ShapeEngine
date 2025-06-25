using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.Polyline;

namespace ShapeEngine.StaticLib.Drawing;

/// <summary>
/// Provides extension methods for drawing polylines with various styles, colors, and transformations.
/// </summary>
/// <remarks>
/// This static class contains a variety of drawing utilities for <see cref="Polyline"/> objects, including support for color gradients,
/// partial outlines, scaling, and glow effects. All methods are intended for rendering purposes and do not modify the polyline data.
/// </remarks>
public static class ShapePolylineDrawing
{
    /// <summary>
    /// Draws the polyline using a single color and specified thickness.
    /// </summary>
    /// <param name="polyline">The polyline to draw.</param>
    /// <param name="thickness">The thickness of the line segments.</param>
    /// <param name="color">The color to use for the polyline.</param>
    /// <param name="capType">The type of line cap to use at segment ends.</param>
    /// <param name="capPoints">The number of points used for the cap rendering.</param>
    public static void Draw(this Polyline polyline, float thickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (polyline.Count < 2) return;
        for (var i = 0; i < polyline.Count - 1; i++)
        {
            var start = polyline[i];
            var end = polyline[i + 1];
            ShapeSegmentDrawing.DrawSegment(start, end, thickness, color, capType, capPoints);
        }
    }

    /// <summary>
    /// Draws the polyline using the specified <see cref="LineDrawingInfo"/>.
    /// </summary>
    /// <param name="polyline">The polyline to draw.</param>
    /// <param name="lineInfo">The drawing information, including thickness, color, and cap type.</param>
    public static void Draw(this Polyline polyline, LineDrawingInfo lineInfo)
    {
        if (polyline.Count < 2) return;
        for (var i = 0; i < polyline.Count - 1; i++)
        {
            var start = polyline[i];
            var end = polyline[i + 1];
            ShapeSegmentDrawing.DrawSegment(start, end, lineInfo);
        }
    }

    /// <summary>
    /// Draws the polyline using a list of colors, cycling through them for each segment.
    /// </summary>
    /// <param name="polyline">The polyline to draw.</param>
    /// <param name="thickness">The thickness of the line segments.</param>
    /// <param name="colors">A list of colors to use for each segment, cycled as needed.</param>
    /// <param name="capType">The type of line cap to use at segment ends.</param>
    /// <param name="capPoints">The number of points used for the cap rendering.</param>
    /// <remarks>
    /// If the number of segments exceeds the number of colors, colors are repeated in order.
    /// </remarks>
    public static void Draw(this Polyline polyline, float thickness, List<ColorRgba> colors, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (polyline.Count < 2) return;
        for (var i = 0; i < polyline.Count - 1; i++)
        {
            var start = polyline[i];
            var end = polyline[i + 1];
            var c = colors[i % colors.Count];
            ShapeSegmentDrawing.DrawSegment(start, end, thickness, c, capType, capPoints);
        }
    }

    /// <summary>
    /// Draws the polyline using a list of colors and the specified <see cref="LineDrawingInfo"/>.
    /// </summary>
    /// <param name="polyline">The polyline to draw.</param>
    /// <param name="colors">A list of colors to use for each segment, cycled as needed.</param>
    /// <param name="lineInfo">The drawing information, including thickness and cap type.</param>
    public static void Draw(this Polyline polyline, List<ColorRgba> colors, LineDrawingInfo lineInfo)
    {
        Draw(polyline, lineInfo.Thickness, colors, lineInfo.CapType, lineInfo.CapPoints);
    }

    /// <summary>
    /// Draws the polyline at a specified position, size, and rotation, using a single color and thickness.
    /// </summary>
    /// <param name="relative">The polyline with relative points.</param>
    /// <param name="pos">The position to draw the polyline at (center).</param>
    /// <param name="size">The scale factor for the polyline.</param>
    /// <param name="rotDeg">The rotation in degrees.</param>
    /// <param name="lineThickness">The thickness of the line segments.</param>
    /// <param name="color">The color to use for the polyline.</param>
    /// <param name="capType">The type of line cap to use at segment ends.</param>
    /// <param name="capPoints">The number of points used for the cap rendering.</param>
    public static void Draw(this Polyline relative, Vector2 pos, float size, float rotDeg, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (relative.Count < 2) return;

        for (var i = 0; i < relative.Count - 1; i++)
        {
            var start = pos + (relative[i] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
            var end = pos + (relative[(i + 1) % relative.Count] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
            ShapeSegmentDrawing.DrawSegment(start, end, lineThickness, color, capType, capPoints);
        }
    }

    /// <summary>
    /// Draws the polyline using a <see cref="Transform2D"/> for position, scale, and rotation, with a single color and thickness.
    /// </summary>
    /// <param name="relative">The polyline with relative points.</param>
    /// <param name="transform">The transform to apply to the polyline.</param>
    /// <param name="lineThickness">The thickness of the line segments.</param>
    /// <param name="color">The color to use for the polyline.</param>
    /// <param name="capType">The type of line cap to use at segment ends.</param>
    /// <param name="capPoints">The number of points used for the cap rendering.</param>
    public static void Draw(this Polyline relative, Transform2D transform, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        Draw(relative, transform.Position, transform.ScaledSize.Length, transform.RotationDeg, lineThickness, color, capType, capPoints);
    }

    /// <summary>
    /// Draws the polyline at a specified position, size, and rotation, using the specified <see cref="LineDrawingInfo"/>.
    /// </summary>
    /// <param name="relative">The polyline with relative points.</param>
    /// <param name="pos">The position to draw the polyline at (center).</param>
    /// <param name="size">The scale factor for the polyline.</param>
    /// <param name="rotDeg">The rotation in degrees.</param>
    /// <param name="lineInfo">The drawing information, including thickness, color, and cap type.</param>
    public static void Draw(this Polyline relative, Vector2 pos, float size, float rotDeg, LineDrawingInfo lineInfo) => Draw(relative, pos, size, rotDeg, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);

    /// <summary>
    /// Draws the polyline using a <see cref="Transform2D"/> and the specified <see cref="LineDrawingInfo"/>.
    /// </summary>
    /// <param name="relative">The polyline with relative points.</param>
    /// <param name="transform">The transform to apply to the polyline.</param>
    /// <param name="lineInfo">The drawing information, including thickness, color, and cap type.</param>
    public static void Draw(this Polyline relative, Transform2D transform, LineDrawingInfo lineInfo) => Draw(relative, transform.Position, transform.ScaledSize.Length, transform.RotationDeg, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);

    /// <summary>
    /// Draws a portion of the polyline's perimeter, as specified by the perimeter length.
    /// </summary>
    /// <param name="polyline">The polyline to draw.</param>
    /// <param name="perimeterToDraw">
    /// The length of the perimeter to draw.
    /// Negative values draw in the clockwise direction.
    /// </param>
    /// <param name="lineInfo">The drawing information, including thickness, color, and cap type.</param>
    public static void DrawPerimeter(this Polyline polyline, float perimeterToDraw, LineDrawingInfo lineInfo)
    {
        DrawPerimeter(polyline, perimeterToDraw, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
    }

    /// <summary>
    /// Draws a portion of the polyline's perimeter, as a percentage of the total outline.
    /// </summary>
    /// <param name="polyline">The polyline to draw.</param>
    /// <param name="f">
    /// Specifies the starting corner and the percentage of the outline to draw.
    /// <list type="bullet">
    /// <item><description>The integer part selects the starting corner (0 = first corner, 1 = second, etc.).</description></item>
    /// <item><description>The decimal part specifies the percentage of the outline to draw, as a fraction (0.0 to 1.0).</description></item>
    /// <item><description>Negative values draw in the clockwise direction; positive values draw counter-clockwise.</description></item>
    /// <item><description>Example: <c>0.35</c> starts at corner 0, draws 35% of the outline counter-clockwise.</description></item>
    /// <item><description>Example: <c>-2.7</c> starts at corner 2, draws 70% of the outline clockwise.</description></item>
    /// </list>
    /// </param>
    /// <param name="lineInfo">The drawing information, including thickness, color, and cap type.</param>
    public static void DrawPercentage(this Polyline polyline, float f, LineDrawingInfo lineInfo)
    {
        DrawPercentage(polyline, f, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
    }

    /// <summary>
    /// Draws a specified length of the polyline's perimeter.
    /// </summary>
    /// <param name="polyline">The polyline to draw.</param>
    /// <param name="perimeterToDraw">The length of the perimeter to draw. Negative values draw in the clockwise direction.</param>
    /// <param name="lineThickness">The thickness of the line segments.</param>
    /// <param name="color">The color to use for the polyline.</param>
    /// <param name="capType">The type of line cap to use at segment ends.</param>
    /// <param name="capPoints">The number of points used for the cap rendering.</param>
    /// <remarks>
    /// Useful for animating outlines or drawing partial shapes.
    /// </remarks>
    public static void DrawPerimeter(this Polyline polyline, float perimeterToDraw, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (polyline.Count < 3 || perimeterToDraw == 0) return;

        bool reverse = perimeterToDraw < 0;
        if (reverse) perimeterToDraw *= -1;

        int currentIndex = reverse ? polyline.Count - 1 : 0;

        for (var i = 0; i < polyline.Count - 1; i++)
        {
            var start = polyline[currentIndex];
            currentIndex = reverse ? currentIndex - 1 : currentIndex + 1;
            var end = polyline[currentIndex];
            var l = (end - start).Length();
            if (l < perimeterToDraw)
            {
                perimeterToDraw -= l;
                ShapeSegmentDrawing.DrawSegment(start, end, lineThickness, color, capType, capPoints);
            }
            else
            {
                float f = perimeterToDraw / l;
                end = start.Lerp(end, f);
                ShapeSegmentDrawing.DrawSegment(start, end, lineThickness, color, capType, capPoints);
                return;
            }
        }
    }

    /// <summary>
    /// Draws a specified percentage of the polyline's outline.
    /// </summary>
    /// <param name="polyline">The polyline to draw.</param>
    /// <param name="f">
    /// Specifies the starting corner and the percentage of the outline to draw.
    /// <list type="bullet">
    /// <item><description>The integer part selects the starting corner (0 = first corner, 1 = second, etc.).</description></item>
    /// <item><description>The decimal part specifies the percentage of the outline to draw, as a fraction (0.0 to 1.0).</description></item>
    /// <item><description>Negative values draw in the clockwise direction; positive values draw counter-clockwise.</description></item>
    /// <item><description>Example: <c>0.35</c> starts at corner 0, draws 35% of the outline counter-clockwise.</description></item>
    /// <item><description>Example: <c>-2.7</c> starts at corner 2, draws 70% of the outline clockwise.</description></item>
    /// </list>
    /// </param>
    /// <param name="lineThickness">The thickness of the line segments.</param>
    /// <param name="color">The color to use for the polyline.</param>
    /// <param name="capType">The type of line cap to use at segment ends.</param>
    /// <param name="capPoints">The number of points used for the cap rendering.</param>
    /// <remarks>
    /// Useful for progress indicators or animated outlines.
    /// </remarks>
    public static void DrawPercentage(this Polyline polyline, float f, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (polyline.Count < 3 || f == 0f) return;

        bool negative = false;
        if (f < 0)
        {
            negative = true;
            f *= -1;
        }
        if (f >= 1)
        {
            Draw(polyline, lineThickness, color, capType, capPoints);
            return;
        }

        float perimeter = 0f;
        for (var i = 0; i < polyline.Count - 1; i++)
        {
            var start = polyline[i];
            var end = polyline[i + 1];
            var l = (end - start).Length();
            perimeter += l;
        }

        f = ShapeMath.Clamp(f, 0f, 1f);
        DrawPerimeter(polyline, perimeter * f * (negative ? -1 : 1), lineThickness, color, capType, capPoints);
    }

    /// <summary>
    /// Draws the polyline with each side scaled towards its origin, allowing for variable side lengths.
    /// </summary>
    /// <param name="polyline">The polyline to draw.</param>
    /// <param name="lineInfo">The drawing information, including thickness, color, and cap type.</param>
    /// <param name="sideScaleFactor">
    /// <para>The scale factor for each side.</para>
    /// <list type="bullet">
    /// <item><description>0: No polyline is drawn.</description></item>
    /// <item><description>1: The normal polyline is drawn.</description></item>
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
    /// This method is useful for creating effects where the polyline appears to shrink or grow from its sides.
    /// </remarks>
    public static void DrawLinesScaled(this Polyline polyline, LineDrawingInfo lineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (polyline.Count < 2) return;
        if (sideScaleFactor <= 0) return;

        if (sideScaleFactor >= 1)
        {
            polyline.Draw(lineInfo);
            return;
        }
        for (var i = 0; i < polyline.Count - 1; i++)
        {
            var start = polyline[i];
            var end = polyline[(i + 1) % polyline.Count];
            ShapeSegmentDrawing.DrawSegment(start, end, lineInfo, sideScaleFactor, sideScaleOrigin);
        }
    }

    /// <summary>
    /// Draws the polyline with each side scaled towards its origin, at a specified position, size, and rotation.
    /// </summary>
    /// <param name="relative">The polyline with relative points.</param>
    /// <param name="pos">The position to draw the polyline at (center).</param>
    /// <param name="size">The scale factor for the polyline.</param>
    /// <param name="rotDeg">The rotation in degrees.</param>
    /// <param name="lineInfo">The drawing information, including thickness, color, and cap type.</param>
    /// <param name="sideScaleFactor">
    /// <para>The scale factor for each side.</para>
    /// <list type="bullet">
    /// <item><description>0: No polyline is drawn.</description></item>
    /// <item><description>1: The normal polyline is drawn.</description></item>
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
    /// Useful for dynamic shape morphing and animation effects.
    /// </remarks>
    public static void DrawLinesScaled(this Polyline relative, Vector2 pos, float size, float rotDeg, LineDrawingInfo lineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (relative.Count < 2) return;
        if (sideScaleFactor <= 0) return;

        if (sideScaleFactor >= 1)
        {
            relative.Draw(pos, size, rotDeg, lineInfo);
            return;
        }

        for (var i = 0; i < relative.Count - 1; i++)
        {
            var start = pos + (relative[i] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
            var end = pos + (relative[(i + 1) % relative.Count] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
            ShapeSegmentDrawing.DrawSegment(start, end, lineInfo, sideScaleFactor, sideScaleOrigin);
        }
    }

    /// <summary>
    /// Draws the polyline with each side scaled towards its origin, using a <see cref="Transform2D"/>.
    /// </summary>
    /// <param name="relative">The polyline with relative points.</param>
    /// <param name="transform">The transform to apply to the polyline.</param>
    /// <param name="lineInfo">The drawing information, including thickness, color, and cap type.</param>
    /// <param name="sideScaleFactor">
    /// <para>The scale factor for each side.</para>
    /// <list type="bullet">
    /// <item><description>0: No polyline is drawn.</description></item>
    /// <item><description>1: The normal polyline is drawn.</description></item>
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
    public static void DrawLinesScaled(this Polyline relative, Transform2D transform, LineDrawingInfo lineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        DrawLinesScaled(relative, transform.Position, transform.ScaledSize.Length, transform.RotationDeg, lineInfo, sideScaleFactor, sideScaleOrigin);
    }

    /// <summary>
    /// Draws circles at each vertex of the polyline.
    /// </summary>
    /// <param name="polyline">The polyline whose vertices to draw.</param>
    /// <param name="vertexRadius">The radius of each vertex circle.</param>
    /// <param name="color">The color of the vertex circles.</param>
    /// <param name="circleSegments">The number of segments to use for each circle.</param>
    /// <remarks>
    /// Useful for debugging or highlighting polyline vertices.
    /// </remarks>
    public static void DrawVertices(this Polyline polyline, float vertexRadius, ColorRgba color, int circleSegments)
    {
        foreach (var p in polyline)
        {
            ShapeCircleDrawing.DrawCircle(p, vertexRadius, color, circleSegments);
        }
    }

    /// <summary>
    /// Draws the polyline with a glow effect, interpolating width and color along each segment.
    /// </summary>
    /// <param name="polyline">The polyline to draw.</param>
    /// <param name="width">The starting width of the glow.</param>
    /// <param name="endWidth">The ending width of the glow.</param>
    /// <param name="color">The starting color of the glow.</param>
    /// <param name="endColorRgba">The ending color of the glow.</param>
    /// <param name="steps">The number of interpolation steps for the glow effect.</param>
    /// <param name="capType">The type of line cap to use at segment ends.</param>
    /// <param name="capPoints">The number of points used for the cap rendering.</param>
    /// <remarks>
    /// This method creates a glowing outline effect by drawing multiple segments on top of each other, interpolating width and color across all steps.
    /// <list type="bullet">
    /// <item><description>The first step uses <paramref name="width"/> and <paramref name="color"/>.</description></item>
    /// <item><description>The last step uses <paramref name="endWidth"/> and <paramref name="endColorRgba"/>.</description></item>
    /// <item><description>Intermediate steps interpolate between <paramref name="width"/> / <paramref name="endWidth"/> and <paramref name="color"/> / <paramref name="endColorRgba"/>.</description></item>
    /// <item><description>Because steps are drawn on top of each other <paramref name="width"/> should be bigger than <paramref name="endWidth"/>.</description></item>
    /// </list>
    /// </remarks>
    public static void DrawGlow(this Polyline polyline, float width, float endWidth, ColorRgba color,
        ColorRgba endColorRgba, int steps, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        //TODO: shouldn't this function draw the entire polyline each step instead of drawing each segment with all the steps after one another
        if (polyline.Count < 2) return;
        for (var i = 0; i < polyline.Count - 1; i++)
        {
            var start = polyline[i];
            var end = polyline[i + 1];
            ShapeSegmentDrawing.DrawSegmentGlow(start, end, width, endWidth, color, endColorRgba, steps, capType, capPoints);
        }
    }
}