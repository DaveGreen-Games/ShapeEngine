using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.StaticLib.Drawing;

/// <summary>
/// Provides extension methods for drawing shapes and outlines using lists of <see cref="Vector2"/> points.
/// </summary>
/// <remarks>
/// This static class contains various helper methods for rendering polygons, outlines, and vertices with customizable styles and transformations.
/// </remarks>
public static class ShapeDrawing
{
    /// <summary>
    /// Draws the outline of a polygon with a color gradient between the start and end colors.
    /// </summary>
    /// <param name="points">The list of points defining the polygon.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="startColorRgba">The color at the start of the outline.</param>
    /// <param name="endColorRgba">The color at the end of the outline.</param>
    /// <param name="capType">The type of line cap to use at the ends of each segment.</param>
    /// <param name="capPoints">The number of points used for the cap.</param>
    /// <remarks>
    /// The outline is drawn by interpolating the color between the start and end colors for each segment.
    /// </remarks>
    public static void DrawOutline(this List<Vector2> points, float lineThickness, ColorRgba startColorRgba, ColorRgba endColorRgba, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (points.Count < 3) return;

        int redStep = (endColorRgba.R - startColorRgba.R) / points.Count;
        int greenStep = (endColorRgba.G - startColorRgba.G) / points.Count;
        int blueStep = (endColorRgba.B - startColorRgba.B) / points.Count;
        int alphaStep = (endColorRgba.A - startColorRgba.A) / points.Count;
        for (var i = 0; i < points.Count; i++)
        {
            var start = points[i];
            var end = points[(i + 1) % points.Count];
            ColorRgba finalColorRgba = new
            (
                startColorRgba.R + redStep * i,
                startColorRgba.G + greenStep * i,
                startColorRgba.B + blueStep * i,
                startColorRgba.A + alphaStep * i
            );
            ShapeSegmentDrawing.DrawSegment(start, end, lineThickness, finalColorRgba, capType, capPoints);
        }
    }

    /// <summary>
    /// Draws the outline of a polygon with a uniform color.
    /// </summary>
    /// <param name="shapePoints">The list of points defining the polygon.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="capType">The type of line cap to use at the ends of each segment.</param>
    /// <param name="capPoints">The number of points used for the cap.</param>
    public static void DrawOutline(this List<Vector2> shapePoints, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (shapePoints.Count < 3) return;

        for (var i = 0; i < shapePoints.Count; i++)
        {
            var start = shapePoints[i];
            var end = shapePoints[(i + 1) % shapePoints.Count];
            ShapeSegmentDrawing.DrawSegment(start, end, lineThickness, color, capType, capPoints);
        }
    }

    /// <summary>
    /// Draws the outline of a polygon with a uniform color and scales each side by a specified factor.
    /// </summary>
    /// <param name="shapePoints">The list of points defining the polygon.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="sideLengthFactor">The factor by which to scale the length of each side.</param>
    /// <param name="capType">The type of line cap to use at the ends of each segment.</param>
    /// <param name="capPoints">The number of points used for the cap.</param>
    public static void DrawOutline(this List<Vector2> shapePoints, float lineThickness, ColorRgba color, float sideLengthFactor,  LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (shapePoints.Count < 3) return;

        for (var i = 0; i < shapePoints.Count; i++)
        {
            var start = shapePoints[i];
            var end = shapePoints[(i + 1) % shapePoints.Count];
            ShapeSegmentDrawing.DrawSegment(start, end, lineThickness, color, sideLengthFactor,  capType, capPoints);
        }
    }

    /// <summary>
    /// Draws the outline of a polygon using the specified <see cref="LineDrawingInfo"/>.
    /// </summary>
    /// <param name="shapePoints">The list of points defining the polygon.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    public static void DrawOutline(this List<Vector2> shapePoints, LineDrawingInfo lineInfo)
    {
        DrawOutline(shapePoints, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
    }

    /// <summary>
    /// Draws the outline of a polygon transformed by position, size, and rotation.
    /// </summary>
    /// <param name="relativePoints">The list of relative points defining the polygon.</param>
    /// <param name="pos">The position of the polygon's center.</param>
    /// <param name="size">The scale factor for the polygon.</param>
    /// <param name="rotDeg">The rotation in degrees.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="capType">The type of line cap to use at the ends of each segment.</param>
    /// <param name="capPoints">The number of points used for the cap.</param>
    /// <remarks>
    /// Each point is transformed by the specified position, size, and rotation before drawing.
    /// </remarks>
    public static void DrawOutline(this List<Vector2> relativePoints, Vector2 pos, float size, float rotDeg, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (relativePoints.Count < 3) return;

        for (var i = 0; i < relativePoints.Count; i++)
        {
            var start = pos + (relativePoints[i] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
            var end = pos + (relativePoints[(i + 1) % relativePoints.Count] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
            ShapeSegmentDrawing.DrawSegment(start, end, lineThickness, color, capType, capPoints);
        }
    }

    /// <summary>
    /// Draws the outline of a polygon using a <see cref="Transform2D"/> for transformation.
    /// </summary>
    /// <param name="relativePoints">The list of relative points defining the polygon.</param>
    /// <param name="transform">The transformation to apply (position, scale, rotation).</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="capType">The type of line cap to use at the ends of each segment.</param>
    /// <param name="capPoints">The number of points used for the cap.</param>
    public static void DrawOutline(this List<Vector2> relativePoints, Transform2D transform, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        DrawOutline(relativePoints, transform.Position, transform.ScaledSize.Length, transform.RotationDeg, lineThickness, color, capType, capPoints);
    }

    /// <summary>
    /// Draws the outline of a polygon using a <see cref="LineDrawingInfo"/> and transformation parameters.
    /// </summary>
    /// <param name="relativePoints">The list of relative points defining the polygon.</param>
    /// <param name="pos">The position of the polygon's center.</param>
    /// <param name="size">The scale factor for the polygon.</param>
    /// <param name="rotDeg">The rotation in degrees.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    public static void DrawOutline(this List<Vector2> relativePoints, Vector2 pos, float size, float rotDeg, LineDrawingInfo lineInfo) 
        => DrawOutline(relativePoints, pos, size, rotDeg, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);

    /// <summary>
    /// Draws the outline of a polygon using a <see cref="LineDrawingInfo"/> and a <see cref="Transform2D"/>.
    /// </summary>
    /// <param name="relativePoints">The list of relative points defining the polygon.</param>
    /// <param name="transform">The transformation to apply (position, scale, rotation).</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    public static void DrawOutline(this List<Vector2> relativePoints, Transform2D transform, LineDrawingInfo lineInfo) 
        => DrawOutline(relativePoints, transform.Position, transform.ScaledSize.Length, transform.RotationDeg, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);

    /// <summary>
    /// Draws a specified amount of the polygon's perimeter as an outline.
    /// </summary>
    /// <param name="shapePoints">The list of points defining the polygon.</param>
    /// <param name="perimeterToDraw">
    /// The length of the perimeter to draw. 
    /// If negative, the outline is drawn in the clockwise direction.
    /// </param>
    /// <param name="startIndex">The index of the corner at which to start drawing.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="capType">The type of line cap to use at the ends of each segment.</param>
    /// <param name="capPoints">The number of points used for the cap.</param>
    /// <remarks>
    /// Useful for animating outlines or drawing partial polygons.
    /// </remarks>
    public static void DrawOutlinePerimeter(this List<Vector2> shapePoints, float perimeterToDraw, int startIndex, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (shapePoints.Count < 3 || perimeterToDraw == 0) return;

        int currentIndex = ShapeMath.Clamp(startIndex, 0, shapePoints.Count - 1);

        bool reverse = perimeterToDraw < 0;
        if (reverse) perimeterToDraw *= -1;

        for (var i = 0; i < shapePoints.Count; i++)
        {
            var start = shapePoints[currentIndex];
            if (reverse) currentIndex = ShapeMath.WrapIndex(shapePoints.Count, currentIndex - 1); // (currentIndex - 1) % shapePoints.Count;
            else currentIndex = (currentIndex + 1) % shapePoints.Count;
            var end = shapePoints[currentIndex];
            var l = (end - start).Length();
            if (l <= perimeterToDraw)
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
    /// Draws a specified percentage of the polygon's outline.
    /// </summary>
    /// <param name="shapePoints">The list of points defining the polygon.</param>
    /// <param name="f">
    /// The percentage of the outline to draw, with the following behavior:
    /// <list type="bullet">
    /// <item><description>If negative, the outline is drawn in the clockwise direction.</description></item>
    /// <item><description>The integer part determines the starting corner index.</description></item>
    /// <item><description>The fractional part determines the percentage of the outline to draw.</description></item>
    /// <item><description>Example: <c>0.35</c> starts at corner 0 and draws 35% in CCW; <c>-2.7</c> starts at corner 2 and draws 70% in CW.</description></item>
    /// </list>
    /// </param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="capType">The type of line cap to use at the ends of each segment.</param>
    /// <param name="capPoints">The number of points used for the cap.</param>
    /// <remarks>
    /// Useful for progress indicators or animated outlines.
    /// </remarks>
    public static void DrawOutlinePercentage(this List<Vector2> shapePoints, float f, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (shapePoints.Count < 3 || f == 0f) return;

        bool negative = false;
        if (f < 0)
        {
            negative = true;
            f *= -1;
        }
        int startIndex = (int)f;
        float percentage = f - startIndex;
        if (percentage <= 0)
        {
            return;
        }
        if (percentage >= 1)
        {
            DrawOutline(shapePoints, lineThickness, color, capType, capPoints);
            return;
        }

        float perimeter = 0f;
        for (var i = 0; i < shapePoints.Count; i++)
        {
            var start = shapePoints[i];
            var end = shapePoints[(i + 1) % shapePoints.Count];
            var l = (end - start).Length();
            perimeter += l;
        }

        DrawOutlinePerimeter(shapePoints, perimeter * f * (negative ? -1 : 1), startIndex, lineThickness, color, capType, capPoints);
    }

    /// <summary>
    /// Draws the polygon as a series of lines, scaling each side towards its origin by a specified factor.
    /// </summary>
    /// <param name="shapePoints">The list of points defining the polygon.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <param name="sideScaleFactor">
    /// The scale factor for each side:
    /// <list type="bullet">
    /// <item><description>0: no polygon is drawn</description></item>
    /// <item><description>1: the normal polygon is drawn</description></item>
    /// <item><description>0.5: each side is half as long</description></item>
    /// </list>
    /// </param>
    /// <param name="sideScaleOrigin">
    /// The point along the line to scale from, in both directions. 
    /// Default is 0.5 (midpoint).
    /// </param>
    /// <remarks>
    /// Allows for creative polygon effects such as shrinking or expanding sides.
    /// </remarks>
    public static void DrawLinesScaled(this List<Vector2> shapePoints, LineDrawingInfo lineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (shapePoints.Count < 3) return;
        if (sideScaleFactor <= 0) return;

        if (sideScaleFactor >= 1)
        {
            shapePoints.DrawOutline(lineInfo);
            return;
        }
        for (var i = 0; i < shapePoints.Count; i++)
        {
            var start = shapePoints[i];
            var end = shapePoints[(i + 1) % shapePoints.Count];
            ShapeSegmentDrawing.DrawSegment(start, end, lineInfo, sideScaleFactor, sideScaleOrigin);
        }

    }

    /// <summary>
    /// Draws the polygon as a series of lines, scaling each side towards its origin by a specified factor, with transformation.
    /// </summary>
    /// <param name="relativePoints">The list of relative points defining the polygon.</param>
    /// <param name="pos">The position of the polygon's center.</param>
    /// <param name="size">The scale factor for the polygon.</param>
    /// <param name="rotDeg">The rotation in degrees.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <param name="sideScaleFactor">
    /// The scale factor for each side:
    /// <list type="bullet">
    /// <item><description>0: no polygon is drawn</description></item>
    /// <item><description>1: the normal polygon is drawn</description></item>
    /// <item><description>0.5: each side is half as long</description></item>
    /// </list>
    /// </param>
    /// <param name="sideScaleOrigin">
    /// The point along the line to scale from, in both directions. 
    /// Default is 0.5 (midpoint).
    /// </param>
    public static void DrawLinesScaled(this List<Vector2> relativePoints, Vector2 pos, float size, float rotDeg, LineDrawingInfo lineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (relativePoints.Count < 3) return;
        if (sideScaleFactor <= 0) return;

        if (sideScaleFactor >= 1)
        {
            relativePoints.DrawOutline(pos, size, rotDeg, lineInfo);
            return;
        }

        for (var i = 0; i < relativePoints.Count; i++)
        {
            var start = pos + (relativePoints[i] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
            var end = pos + (relativePoints[(i + 1) % relativePoints.Count] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
            ShapeSegmentDrawing.DrawSegment(start, end, lineInfo, sideScaleFactor, sideScaleOrigin);
        }

    }

    /// <summary>
    /// Draws the polygon as a series of lines, scaling each side towards its origin by a specified factor, using a <see cref="Transform2D"/>.
    /// </summary>
    /// <param name="relativePoints">The list of relative points defining the polygon.</param>
    /// <param name="transform">The transformation to apply (position, scale, rotation).</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <param name="sideScaleFactor">
    /// The scale factor for each side:
    /// <list type="bullet">
    /// <item><description>0: no polygon is drawn</description></item>
    /// <item><description>1: the normal polygon is drawn</description></item>
    /// <item><description>0.5: each side is half as long</description></item>
    /// </list>
    /// </param>
    /// <param name="sideScaleOrigin">
    /// The point along the line to scale from, in both directions. 
    /// Default is 0.5 (midpoint).
    /// </param>
    public static void DrawLinesScaled(this List<Vector2> relativePoints, Transform2D transform, LineDrawingInfo lineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        DrawLinesScaled(relativePoints, transform.Position, transform.ScaledSize.Length, transform.RotationDeg, lineInfo, sideScaleFactor, sideScaleOrigin);

    }

    /// <summary>
    /// Draws cornered outlines for a polygon, using absolute corner lengths.
    /// </summary>
    /// <param name="points">The list of points defining the polygon.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="cornerLengths">A list of lengths for each corner.</param>
    /// <param name="capType">The type of line cap to use at the ends of each segment.</param>
    /// <param name="capPoints">The number of points used for the cap.</param>
    /// <remarks>
    /// Each corner is drawn with the specified length, allowing for custom corner effects.
    /// A corner is drawn from the <c>previous point [i-1]</c> to the <c>current point [i]</c> to the <c>next point [i+1]</c>.
    /// </remarks>
    public static void DrawOutlineCornered(this List<Vector2> points, float lineThickness, ColorRgba color, List<float> cornerLengths, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        for (var i = 0; i < points.Count; i++)
        {
            float cornerLength = cornerLengths[i%cornerLengths.Count];
            var prev = points[(i - 1) % points.Count];
            var cur = points[i];
            var next = points[(i + 1) % points.Count];
            ShapeSegmentDrawing.DrawSegment(cur, cur + next.Normalize() * cornerLength, lineThickness, color, capType, capPoints);
            ShapeSegmentDrawing.DrawSegment(cur, cur + prev.Normalize() * cornerLength, lineThickness, color, capType, capPoints);
        }
    }

    /// <summary>
    /// Draws cornered outlines for a polygon, using relative corner factors.
    /// </summary>
    /// <param name="points">The list of points defining the polygon.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="cornerFactors">
    /// A list of factors (0-1) for each corner, representing the relative length.
    /// <list type="bullet">
    /// <item><description>Previous to current is interpolated with the specified corner factor. </description></item>
    /// <item><description>Current to next is interpolated with the specified corner factor. </description></item>
    /// </list>
    /// </param>
    /// <param name="capType">The type of line cap to use at the ends of each segment.</param>
    /// <param name="capPoints">The number of points used for the cap.</param>
    /// <remarks>
    /// Each corner is drawn with a length relative to the side, allowing for proportional corner effects.
    /// A corner is drawn from the <c>previous point [i-1]</c> to the <c>current point [i]</c> to the <c>next point [i+1]</c>.
    /// </remarks>
    public static void DrawOutlineCorneredRelative(this List<Vector2> points, float lineThickness, ColorRgba color, List<float> cornerFactors, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        for (var i = 0; i < points.Count; i++)
        {
            float cornerF = cornerFactors[i%cornerFactors.Count];
            var prev = points[(i - 1) % points.Count];
            var cur = points[i];
            var next = points[(i + 1) % points.Count];
            ShapeSegmentDrawing.DrawSegment(cur, cur.Lerp(next, cornerF), lineThickness, color, capType, capPoints);
            ShapeSegmentDrawing.DrawSegment(cur, cur.Lerp(prev, cornerF), lineThickness, color, capType, capPoints);
        }
    }

    /// <summary>
    /// Draws cornered outlines for a polygon using absolute corner lengths and <see cref="LineDrawingInfo"/>.
    /// </summary>
    /// <param name="points">The list of points defining the polygon.</param>
    /// <param name="cornerLengths">A list of lengths for each corner.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <remarks>A corner is drawn from the <c>previous point [i-1]</c> to the <c>current point [i]</c> to the <c>next point [i+1]</c></remarks>
    public static void DrawOutlineCornered(this List<Vector2> points, List<float> cornerLengths, LineDrawingInfo lineInfo)
    {
        for (var i = 0; i < points.Count; i++)
        {
            float cornerLength = cornerLengths[i%cornerLengths.Count];
            var prev = points[(i - 1) % points.Count];
            var cur = points[i];
            var next = points[(i + 1) % points.Count];
            ShapeSegmentDrawing.DrawSegment(cur, cur + next.Normalize() * cornerLength, lineInfo);
            ShapeSegmentDrawing.DrawSegment(cur, cur + prev.Normalize() * cornerLength, lineInfo);
        }
    }

    /// <summary>
    /// Draws cornered outlines for a polygon using relative corner factors and <see cref="LineDrawingInfo"/>.
    /// </summary>
    /// <param name="points">The list of points defining the polygon.</param>
    /// <param name="cornerFactors">
    /// A list of factors (0-1) for each corner, representing the relative length.
    /// <list type="bullet">
    /// <item><description>Previous to current is interpolated with the specified corner factor. </description></item>
    /// <item><description>Current to next is interpolated with the specified corner factor. </description></item>
    /// </list>
    /// </param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <remarks>A corner is drawn from the <c>previous point [i-1]</c> to the <c>current point [i]</c> to the <c>next point [i+1]</c></remarks>
    public static void DrawOutlineCorneredRelative(this List<Vector2> points, List<float> cornerFactors, LineDrawingInfo lineInfo)
    {
        for (var i = 0; i < points.Count; i++)
        {
            float cornerF = cornerFactors[i%cornerFactors.Count];
            var prev = points[(i - 1) % points.Count];
            var cur = points[i];
            var next = points[(i + 1) % points.Count];
            ShapeSegmentDrawing.DrawSegment(cur, cur.Lerp(next, cornerF), lineInfo);
            ShapeSegmentDrawing.DrawSegment(cur, cur.Lerp(prev, cornerF), lineInfo);
        }
    }

    /// <summary>
    /// Draws cornered outlines for a polygon, using a uniform corner length.
    /// </summary>
    /// <param name="points">The list of points defining the polygon.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="cornerLength">The length for each corner.</param>
    /// <param name="capType">The type of line cap to use at the ends of each segment.</param>
    /// <param name="capPoints">The number of points used for the cap.</param>
    /// <remarks>A corner is drawn from the <c>previous point [i-1]</c> to the <c>current point [i]</c> to the <c>next point [i+1]</c></remarks>
    public static void DrawOutlineCornered(this List<Vector2> points, float lineThickness, ColorRgba color, float cornerLength, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        for (var i = 0; i < points.Count; i++)
        {
            var prev = points[(i-1)%points.Count];
            var cur = points[i];
            var next = points[(i+1)%points.Count];
            ShapeSegmentDrawing.DrawSegment(cur, cur + next.Normalize() * cornerLength, lineThickness, color, capType, capPoints);
            ShapeSegmentDrawing.DrawSegment(cur, cur + prev.Normalize() * cornerLength, lineThickness, color, capType, capPoints);
        }
    }

    /// <summary>
    /// Draws cornered outlines for a polygon, using a uniform relative corner factor.
    /// </summary>
    /// <param name="points">The list of points defining the polygon.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="cornerF">
    /// A uniform factor (0-1) for each corner, representing the relative length.
    /// <list type="bullet">
    /// <item><description>Previous to current is interpolated with <paramref name="cornerF"/>. </description></item>
    /// <item><description>Current to next is interpolated with <paramref name="cornerF"/>. </description></item>
    /// </list>
    /// </param>
    /// <param name="capType">The type of line cap to use at the ends of each segment.</param>
    /// <param name="capPoints">The number of points used for the cap.</param>
    /// <remarks>A corner is drawn from the <c>previous point [i-1]</c> to the <c>current point [i]</c> to the <c>next point [i+1]</c></remarks>
    public static void DrawOutlineCorneredRelative(this List<Vector2> points, float lineThickness, ColorRgba color, float cornerF, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        for (var i = 0; i < points.Count; i++)
        {
            var prev = points[(i - 1) % points.Count];
            var cur = points[i];
            var next = points[(i + 1) % points.Count];
            ShapeSegmentDrawing.DrawSegment(cur, cur.Lerp(next, cornerF), lineThickness, color, capType, capPoints);
            ShapeSegmentDrawing.DrawSegment(cur, cur.Lerp(prev, cornerF), lineThickness, color, capType, capPoints);
        }
    }

    /// <summary>
    /// Draws cornered outlines for a polygon, using a uniform corner length and <see cref="LineDrawingInfo"/>.
    /// </summary>
    /// <param name="points">The list of points defining the polygon.</param>
    /// <param name="cornerLength">The length for each corner.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <remarks>A corner is drawn from the <c>previous point [i-1]</c> to the <c>current point [i]</c> to the <c>next point [i+1]</c></remarks>
    public static void DrawOutlineCornered(this List<Vector2> points, float cornerLength, LineDrawingInfo lineInfo)
    {
        for (var i = 0; i < points.Count; i++)
        {
            var prev = points[(i-1)%points.Count];
            var cur = points[i];
            var next = points[(i+1)%points.Count];
            ShapeSegmentDrawing.DrawSegment(cur, cur + next.Normalize() * cornerLength, lineInfo);
            ShapeSegmentDrawing.DrawSegment(cur, cur + prev.Normalize() * cornerLength, lineInfo);
        }
    }

    /// <summary>
    /// Draws cornered outlines for a polygon, using a uniform relative corner factor and <see cref="LineDrawingInfo"/>.
    /// </summary>
    /// <param name="points">The list of points defining the polygon.</param>
    /// <param name="cornerF">
    /// A uniform factor (0-1) for each corner, representing the relative length.
    /// <list type="bullet">
    /// <item><description>Previous to current is interpolated with <paramref name="cornerF"/>. </description></item>
    /// <item><description>Current to next is interpolated with <paramref name="cornerF"/>. </description></item>
    /// </list>
    /// </param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <remarks>A corner is drawn from the <c>previous point [i-1]</c> to the <c>current point [i]</c> to the <c>next point [i+1]</c></remarks>
    public static void DrawOutlineCorneredRelative(this List<Vector2> points, float cornerF, LineDrawingInfo lineInfo)
    {
        for (var i = 0; i < points.Count; i++)
        {
            var prev = points[(i - 1) % points.Count];
            var cur = points[i];
            var next = points[(i + 1) % points.Count];
            ShapeSegmentDrawing.DrawSegment(cur, cur.Lerp(next, cornerF), lineInfo);
            ShapeSegmentDrawing.DrawSegment(cur, cur.Lerp(prev, cornerF), lineInfo);
        }
    }

    /// <summary>
    /// Draws cornered outlines for a polygon using <see cref="LineDrawingInfo"/> and a uniform corner length.
    /// </summary>
    /// <param name="points">The list of points defining the polygon.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <param name="cornerLength">The length for each corner.</param>
    /// <remarks>A corner is drawn from the <c>previous point [i-1]</c> to the <c>current point [i]</c> to the <c>next point [i+1]</c></remarks>
    public static void DrawOutlineCornered(this List<Vector2> points, LineDrawingInfo lineInfo, float cornerLength) 
        => DrawOutlineCornered(points, lineInfo.Thickness,lineInfo.Color, cornerLength, lineInfo.CapType, lineInfo.CapPoints);

    /// <summary>
    /// Draws cornered outlines for a polygon using <see cref="LineDrawingInfo"/> and a list of corner lengths.
    /// </summary>
    /// <param name="points">The list of points defining the polygon.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <param name="cornerLengths">A list of lengths for each corner.</param>
    public static void DrawOutlineCornered(this List<Vector2> points, LineDrawingInfo lineInfo, List<float> cornerLengths) 
        => DrawOutlineCornered(points, lineInfo.Thickness,lineInfo.Color, cornerLengths, lineInfo.CapType, lineInfo.CapPoints);

    /// <summary>
    /// Draws cornered outlines for a polygon using <see cref="LineDrawingInfo"/> and a uniform relative corner factor.
    /// </summary>
    /// <param name="points">The list of points defining the polygon.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <param name="cornerF">
    /// A uniform factor (0-1) for each corner, representing the relative length.
    /// <list type="bullet">
    /// <item><description>Previous to current is interpolated with <paramref name="cornerF"/>. </description></item>
    /// <item><description>Current to next is interpolated with <paramref name="cornerF"/>. </description></item>
    /// </list>
    /// </param>
    /// <remarks>A corner is drawn from the <c>previous point [i-1]</c> to the <c>current point [i]</c> to the <c>next point [i+1]</c></remarks>
    public static void DrawOutlineCorneredRelative(this List<Vector2> points, LineDrawingInfo lineInfo, float cornerF) 
        => DrawOutlineCornered(points, lineInfo.Thickness,lineInfo.Color, cornerF, lineInfo.CapType, lineInfo.CapPoints);

    /// <summary>
    /// Draws cornered outlines for a polygon using <see cref="LineDrawingInfo"/> and a list of relative corner factors.
    /// </summary>
    /// <param name="points">The list of points defining the polygon.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <param name="cornerFactors">
    /// A list of factors (0-1) for each corner, representing the relative length.
    /// <list type="bullet">
    /// <item><description>Previous to current is interpolated with the specified corner factor. </description></item>
    /// <item><description>Current to next is interpolated with the specified corner factor. </description></item>
    /// </list>
    /// </param>
    /// <remarks>A corner is drawn from the <c>previous point [i-1]</c> to the <c>current point [i]</c> to the <c>next point [i+1]</c></remarks>
    public static void DrawOutlineCorneredRelative(this List<Vector2> points, LineDrawingInfo lineInfo, List<float> cornerFactors) 
        => DrawOutlineCornered(points, lineInfo.Thickness,lineInfo.Color, cornerFactors, lineInfo.CapType, lineInfo.CapPoints);

    /// <summary>
    /// Draws circles at each vertex of the polygon.
    /// </summary>
    /// <param name="points">The list of points (vertices) to draw.</param>
    /// <param name="vertexRadius">The radius of each vertex circle.</param>
    /// <param name="color">The color of the vertex circles.</param>
    /// <param name="circleSegments">The number of segments to use for each circle.</param>
    /// <remarks>
    /// Useful for visualizing vertices or debugging polygon shapes.
    /// </remarks>
    public static void DrawVertices(this List<Vector2> points, float vertexRadius, ColorRgba color, int circleSegments)
    {
        foreach (var p in points)
        {
            ShapeCircleDrawing.DrawCircle(p, vertexRadius, color, circleSegments);
        }
    }
}