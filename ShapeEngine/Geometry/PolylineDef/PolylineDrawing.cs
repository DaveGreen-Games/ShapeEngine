using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.PolylineDef;

/// <summary>
/// Provides extension methods for drawing polylines with various styles, colors, and transformations.
/// </summary>
/// <remarks>
/// This static class contains a variety of drawing utilities for <see cref="Polyline"/> objects, including support for color gradients,
/// partial outlines, scaling, and glow effects. All methods are intended for rendering purposes and do not modify the polyline data.
/// </remarks>
public static class PolylineDrawing
{
    //TODO:
    // - Clean up and remove unused methods (relative polyline for instance)
    // - Add regions
    // - Copy structure of PolygonDrawing
    // - Use PolygonDrawing for functions that need to be overhauled here
    // - Add DrawFast methods that dont work with alpha color (just draw segments between vertices with endcaps)
    
    //TODO: Rework all below with new ClipperImmediate2d system!
    
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
            SegmentDrawing.DrawSegment(start, end, thickness, color, capType, capPoints);
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
            SegmentDrawing.DrawSegment(start, end, lineInfo);
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
            SegmentDrawing.DrawSegment(start, end, thickness, c, capType, capPoints);
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
            SegmentDrawing.DrawSegment(start, end, lineThickness, color, capType, capPoints);
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
                SegmentDrawing.DrawSegment(start, end, lineThickness, color, capType, capPoints);
            }
            else
            {
                float f = perimeterToDraw / l;
                end = start.Lerp(end, f);
                SegmentDrawing.DrawSegment(start, end, lineThickness, color, capType, capPoints);
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
            SegmentDrawing.DrawSegment(start, end, lineInfo, sideScaleFactor, sideScaleOrigin);
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
            SegmentDrawing.DrawSegment(start, end, lineInfo, sideScaleFactor, sideScaleOrigin);
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
    /// Draws the polyline with a glow effect, interpolating width and color along each segment.
    /// </summary>
    /// <param name="polyline">The polyline to draw.</param>
    /// <param name="width">The starting width of the glow. Should be bigger than <c>endWidth</c>.</param>
    /// <param name="endWidth">The ending width of the glow. Should be smaller than <c>width</c>.</param>
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
        if (polyline.Count < 2 || steps <= 0) return;

        if (steps == 1)
        {
            Draw(polyline, width, color, capType, capPoints);
            return;
        }
    
        for (var s = 0; s < steps; s++)
        {
            var f = s / (float)(steps - 1);
            var currentWidth = ShapeMath.LerpFloat(width, endWidth, f);
            var currentColor = color.Lerp(endColorRgba, f);
            Draw(polyline, currentWidth, currentColor, capType, capPoints);
        }
    }
    
    
    #region Draw Masked
    /// <summary>
    /// Draws each segment of the polyline using a triangular mask.
    /// </summary>
    /// <param name="polyline">The polyline whose segments will be drawn.</param>
    /// <param name="mask">The <see cref="Triangle"/> used as the clipping mask.</param>
    /// <param name="lineInfo">Drawing parameters such as thickness, color and cap type.</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawMasked(this Polyline polyline, Triangle mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        if (polyline.Count < 2) return;
        
        for (var i = 0; i < polyline.Count - 1; i++)
        {
            var start = polyline[i];
            var end = polyline[i + 1];
            var segment = new Segment(start, end);
            segment.DrawMasked(mask, lineInfo, reversedMask);
        }
    }
    /// <summary>
    /// Draws each segment of the polyline using a circular mask.
    /// </summary>
    /// <param name="polyline">The polyline whose segments will be drawn.</param>
    /// <param name="mask">The <see cref="Circle"/> used as the clipping mask.</param>
    /// <param name="lineInfo">Drawing parameters such as thickness, color and cap type.</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawMasked(this Polyline polyline, Circle mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        if (polyline.Count < 2) return;
        
        for (var i = 0; i < polyline.Count - 1; i++)
        {
            var start = polyline[i];
            var end = polyline[i + 1];
            var segment = new Segment(start, end);
            segment.DrawMasked(mask, lineInfo, reversedMask);
        }
    }
    /// <summary>
    /// Draws each segment of the polyline using a rectangular mask.
    /// </summary>
    /// <param name="polyline">The polyline whose segments will be drawn.</param>
    /// <param name="mask">The <see cref="Rect"/> used as the clipping mask.</param>
    /// <param name="lineInfo">Drawing parameters such as thickness, color and cap type.</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawMasked(this Polyline polyline, Rect mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        if (polyline.Count < 2) return;
        
        for (var i = 0; i < polyline.Count - 1; i++)
        {
            var start = polyline[i];
            var end = polyline[i + 1];
            var segment = new Segment(start, end);
            segment.DrawMasked(mask, lineInfo, reversedMask);
        }
    }
    /// <summary>
    /// Draws each segment of the polyline using a quadrilateral mask.
    /// </summary>
    /// <param name="polyline">The polyline whose segments will be drawn.</param>
    /// <param name="mask">The <see cref="Quad"/> used as the clipping mask.</param>
    /// <param name="lineInfo">Drawing parameters such as thickness, color and cap type.</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawMasked(this Polyline polyline, Quad mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        if (polyline.Count < 2) return;
        
        for (var i = 0; i < polyline.Count - 1; i++)
        {
            var start = polyline[i];
            var end = polyline[i + 1];
            var segment = new Segment(start, end);
            segment.DrawMasked(mask, lineInfo, reversedMask);
        }
    }
    
    /// <summary>
    /// Draws each segment of the polyline using a <see cref="Polygon"/> as the clipping mask.
    /// </summary>
    /// <param name="polyline">The polyline whose segments will be drawn.</param>
    /// <param name="mask">The <see cref="Polygon"/> used as the clipping mask.</param>
    /// <param name="lineInfo">Drawing parameters such as thickness, color and cap type.</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawMasked(this Polyline polyline, Polygon mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        if (polyline.Count < 2) return;
        
        for (var i = 0; i < polyline.Count - 1; i++)
        {
            var start = polyline[i];
            var end = polyline[i + 1];
            var segment = new Segment(start, end);
            segment.DrawMasked(mask, lineInfo, reversedMask);
        }
    }
    /// <summary>
    /// Draws each segment of the polyline using a mask of a generic closed shape type.
    /// </summary>
    /// <typeparam name="T">
    /// The mask type that implements <see cref="IClosedShapeTypeProvider"/> (for example <see cref="Circle"/>, <see cref="Rect"/>, <see cref="Polygon"/>, or <see cref="Quad"/>).
    /// </typeparam>
    /// <param name="polyline">The polyline whose segments will be drawn.</param>
    /// <param name="mask">The mask instance used for clipping each segment.</param>
    /// <param name="lineInfo">Drawing parameters such as thickness, color and cap type.</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawMasked<T>(this Polyline polyline, T mask, LineDrawingInfo lineInfo, bool reversedMask = false) where T : IClosedShapeTypeProvider
    {
        if (polyline.Count < 2) return;
        
        for (var i = 0; i < polyline.Count - 1; i++)
        {
            var start = polyline[i];
            var end = polyline[i + 1];
            var segment = new Segment(start, end);
            segment.DrawMasked(mask, lineInfo, reversedMask);
        }
    }
    #endregion
    
    #region Gapped
    /// <summary>
    /// Draws a gapped outline for a polyline (open or closed), creating a dashed or segmented effect along the polyline's length.
    /// </summary>
    /// <param name="polyline">The polyline to draw.</param>
    /// <param name="perimeter">
    /// The total length of the polyline.
    /// If zero or negative, the method calculates it automatically.
    /// Providing a known length avoids redundant calculations and improves performance, especially for static segments.
    /// </param>
    /// <param name="lineInfo">Parameters describing how to draw the polyline.</param>
    /// <param name="gapDrawingInfo">Parameters describing the gap configuration.</param>
    /// <returns>
    /// The perimeter of the polyline if positive; otherwise, -1.
    /// If the shape does not change, the valid length can be reused in subsequent frames to avoid recalculating.
    /// </returns>
    /// <remarks>
    /// - If <paramref name="gapDrawingInfo.Gaps"/> is 0 or <paramref name="gapDrawingInfo.GapPerimeterPercentage"/> is 0, the polyline is drawn solid.
    /// - If <paramref name="gapDrawingInfo.GapPerimeterPercentage"/> is 1 or greater, no polyline is drawn.
    /// </remarks>
    public static float DrawGappedOutline(this Polyline polyline, float perimeter, LineDrawingInfo lineInfo, GappedOutlineDrawingInfo gapDrawingInfo)
    {
        if (gapDrawingInfo.Gaps <= 0 || gapDrawingInfo.GapPerimeterPercentage <= 0f)
        {
            polyline.Draw(lineInfo);
            return perimeter > 0f ? perimeter : -1f;
        }

        if (gapDrawingInfo.GapPerimeterPercentage >= 1f) return perimeter > 0f ? perimeter : -1f;

        var nonGapPercentage = 1f - gapDrawingInfo.GapPerimeterPercentage;

        var gapPercentageRange = gapDrawingInfo.GapPerimeterPercentage / gapDrawingInfo.Gaps;
        var nonGapPercentageRange = nonGapPercentage / gapDrawingInfo.Gaps;

        if (perimeter <= 0f)
        {
            perimeter = polyline.GetLength();
        }

        var startDistance = perimeter * gapDrawingInfo.StartOffset;
        var curDistance = 0f;
        var nextDistance = startDistance;
        
        
        var curIndex = 0;
        var curPoint = polyline[0];
        var nextPoint= polyline[1];
        var curW = nextPoint - curPoint;
        var curDis = curW.Length();
        
        var points = new List<Vector2>(3);

        int whileCounter = gapDrawingInfo.Gaps;
        
        while (whileCounter > 0)
        {
            if (curDistance + curDis >= nextDistance) //as long as next distance in smaller than the distance to the next polyline point
            {
                var p = curPoint + (curW / curDis) * (nextDistance - curDistance);
                
                
                if (points.Count == 0)
                {
                    // var prevDistance = nextDistance;
                    nextDistance += nonGapPercentageRange * perimeter;
                    points.Add(p);

                }
                else
                {
                    // var prevDistance = nextDistance;
                    nextDistance += gapPercentageRange * perimeter;
                    points.Add(p);
                    
                    if (points.Count == 2)
                    {
                        SegmentDrawing.DrawSegment(points[0], points[1], lineInfo);
                    }
                    else
                    {
                        for (var i = 0; i < points.Count - 1; i++)
                        {
                            var p1 = points[i];
                            var p2 = points[(i + 1) % points.Count];
                            SegmentDrawing.DrawSegment(p1, p2, lineInfo);
                        }
                    }
                    points.Clear();
                    whileCounter--;
                }

            }
            else
            {
                if (curIndex >= polyline.Count - 2) //last point
                {
                    if (points.Count > 0)
                    {
                        points.Add(nextPoint);
                        if (points.Count == 2)
                        {
                            SegmentDrawing.DrawSegment(points[0], points[1], lineInfo);
                        }
                        else
                        {
                            for (var i = 0; i < points.Count - 1; i++)
                            {
                                var p1 = points[i];
                                var p2 = points[(i + 1) % points.Count];
                                SegmentDrawing.DrawSegment(p1, p2, lineInfo);
                            }
                        }
                        points.Clear();
                        points.Add(polyline[0]);
                    }
                    
                    curDistance += curDis;
                    curIndex = 0;
                    curPoint = polyline[curIndex];
                    nextPoint = polyline[(curIndex + 1) % polyline.Count];
                    curW = nextPoint - curPoint;
                    curDis = curW.Length();
                }
                else
                {
                    if(points.Count > 0) points.Add(nextPoint);

                    curDistance += curDis;
                    curIndex += 1;// (curIndex + 1) % polyline.Count;
                    curPoint = polyline[curIndex];
                    nextPoint = polyline[(curIndex + 1) % polyline.Count];
                    curW = nextPoint - curPoint;
                    curDis = curW.Length();
                }
            }
            
        }

        return perimeter;
    }
    #endregion
}

