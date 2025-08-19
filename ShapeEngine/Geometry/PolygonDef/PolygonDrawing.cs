using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.PolygonDef;

/// <summary>
/// Provides extension methods for drawing polygons with various styles and options.
/// </summary>
/// <remarks>
/// These methods extend the <see cref="Polygon"/> class to support a wide range of drawing operations,
/// including filled, outlined, cornered, and vertex-based renderings.
/// Many methods support relative transformations and color gradients.
/// </remarks>
public static class PolygonDrawing
{
    /// <summary>
    /// Draws a convex polygon filled with the specified color, using the polygon's centroid as the center.
    /// </summary>
    /// <param name="poly">The polygon to draw.</param>
    /// <param name="color">The fill color.</param>
    /// <param name="clockwise">If true, draws triangles in clockwise order; otherwise, counter-clockwise.</param>
    public static void DrawPolygonConvex(this Polygon poly, ColorRgba color, bool clockwise = false)
    {
        if (poly.Count < 3) return; // Polygon must have at least 3 points
        DrawPolygonConvex(poly, poly.GetCentroid(), color, clockwise);
    }

    /// <summary>
    /// Draws a convex polygon filled with the specified color, using a custom center point.
    /// </summary>
    /// <param name="poly">The polygon to draw.</param>
    /// <param name="center">The center point for triangulation.</param>
    /// <param name="color">The fill color.</param>
    /// <param name="clockwise">If true, draws triangles in clockwise order; otherwise, counter-clockwise.</param>
    public static void DrawPolygonConvex(this Polygon poly, Vector2 center, ColorRgba color, bool clockwise = false)
    {
        if (poly.Count < 3) return; // Polygon must have at least 3 points
        if (clockwise)
        {
            for (var i = 0; i < poly.Count - 1; i++)
            {
                Raylib.DrawTriangle(poly[i], center, poly[i + 1], color.ToRayColor());
            }
            Raylib.DrawTriangle(poly[^1], center, poly[0], color.ToRayColor());
        }
        else
        {
            for (var i = 0; i < poly.Count - 1; i++)
            {
                Raylib.DrawTriangle(poly[i], poly[i + 1], center, color.ToRayColor());
            }
            Raylib.DrawTriangle(poly[^1], poly[0], center, color.ToRayColor());
        }
    }

    /// <summary>
    /// Draws a convex polygon filled with the specified color, applying position, size, and rotation.
    /// </summary>
    /// <param name="relativePoly">The polygon to draw, with points relative to the origin.</param>
    /// <param name="pos">The position of the polygon's center.</param>
    /// <param name="size">The scale factor for the polygon.</param>
    /// <param name="rotDeg">The rotation in degrees.</param>
    /// <param name="color">The fill color.</param>
    /// <param name="clockwise">If true, draws triangles in clockwise order; otherwise, counter-clockwise.</param>
    public static void DrawPolygonConvex(this Polygon relativePoly, Vector2 pos, float size, float rotDeg, ColorRgba color, bool clockwise = false)
    {
        if (relativePoly.Count < 3) return; // Polygon must have at least 3 points
        if (clockwise)
        {
            for (int i = 0; i < relativePoly.Count - 1; i++)
            {
                var a = pos + (relativePoly[i] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
                var b = pos;
                var c = pos + (relativePoly[i + 1] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
                Raylib.DrawTriangle(a, b, c, color.ToRayColor());
            }

            var aFinal = pos + (relativePoly[^1] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
            var bFinal = pos;
            var cFinal = pos + (relativePoly[0] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
            Raylib.DrawTriangle(aFinal, bFinal, cFinal, color.ToRayColor());
        }
        else
        {
            for (int i = 0; i < relativePoly.Count - 1; i++)
            {
                var a = pos + (relativePoly[i] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
                var b = pos + (relativePoly[i + 1] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
                var c = pos;
                Raylib.DrawTriangle(a, b, c, color.ToRayColor());
            }

            var aFinal = pos + (relativePoly[^1] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
            var bFinal = pos + (relativePoly[0] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
            var cFinal = pos;
            Raylib.DrawTriangle(aFinal, bFinal, cFinal, color.ToRayColor());
        }
    }

    /// <summary>
    /// Draws a convex polygon filled with the specified color, using a <see cref="Transform2D"/>.
    /// </summary>
    /// <param name="relativePoly">The polygon to draw, with points relative to the origin.</param>
    /// <param name="transform">The transform to apply (position, scale, rotation).</param>
    /// <param name="color">The fill color.</param>
    /// <param name="clockwise">If true, draws triangles in clockwise order; otherwise, counter-clockwise.</param>
    public static void DrawPolygonConvex(this Polygon relativePoly, Transform2D transform, ColorRgba color, bool clockwise = false)
    {
        DrawPolygonConvex(relativePoly, transform.Position, transform.ScaledSize.Length, transform.RotationDeg, color, clockwise);
    }

    /// <summary>
    /// Draws the polygon filled with the specified color. Uses triangulation for polygons with more than 3 vertices.
    /// </summary>
    /// <param name="poly">The polygon to draw.</param>
    /// <param name="color">The fill color.</param>
    /// <remarks>
    /// For polygons with exactly 3 vertices, an optimized triangle drawing method is used.
    /// For polygons with more than 3 vertices, the polygon is triangulated and each triangle is drawn.
    /// For best performance with static polygons, precompute the triangulation and draw the result directly.
    /// </remarks>
    public static void Draw(this Polygon poly, ColorRgba color)
    {
        if (poly.Count < 3) return;
        if (poly.Count == 3)
        {
            TriangleDrawing.DrawTriangle(poly[0], poly[1], poly[2], color);
            return;
        }
        poly.Triangulate().Draw(color);
    }

    /// <summary>
    /// Debug method: draws the polygon's outline with a color gradient from start to end, and highlights the first and last vertices.
    /// </summary>
    /// <param name="poly">The polygon to draw.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="startColorRgba">The color at the start vertex.</param>
    /// <param name="endColorRgba">The color at the end vertex.</param>
    /// <remarks>
    /// Useful for debugging polygon winding and vertex order.
    /// </remarks>
    public static void DEBUG_DrawLinesCCW(this Polygon poly, float lineThickness, ColorRgba startColorRgba, ColorRgba endColorRgba)
    {
        if (poly.Count < 3) return;

        DrawLines(poly, lineThickness, startColorRgba, endColorRgba);
        CircleDrawing.DrawCircle(poly[0], lineThickness * 2f, startColorRgba);
        CircleDrawing.DrawCircle(poly[^1], lineThickness * 2f, endColorRgba);
    }

    /// <summary>
    /// Draws the polygon's outline with a color gradient from start to end.
    /// </summary>
    /// <param name="poly">The polygon to draw.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="startColorRgba">The color at the start vertex.</param>
    /// <param name="endColorRgba">The color at the end vertex.</param>
    /// <param name="capType">The type of line cap to use.</param>
    /// <param name="capPoints">The number of points for the cap.</param>
    public static void DrawLines(this Polygon poly, float lineThickness, ColorRgba startColorRgba, ColorRgba endColorRgba, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (poly.Count < 3) return;

        int redStep = (endColorRgba.R - startColorRgba.R) / poly.Count;
        int greenStep = (endColorRgba.G - startColorRgba.G) / poly.Count;
        int blueStep = (endColorRgba.B - startColorRgba.B) / poly.Count;
        int alphaStep = (endColorRgba.A - startColorRgba.A) / poly.Count;
        for (var i = 0; i < poly.Count; i++)
        {
            var start = poly[i];
            var end = poly[(i + 1) % poly.Count];
            ColorRgba finalColorRgba = new
            (
                startColorRgba.R + redStep * i,
                startColorRgba.G + greenStep * i,
                startColorRgba.B + blueStep * i,
                startColorRgba.A + alphaStep * i
            );
            SegmentDrawing.DrawSegment(start, end, lineThickness, finalColorRgba, capType, capPoints);
        }
    }

    /// <summary>
    /// Draws the polygon's outline with a uniform color.
    /// </summary>
    /// <param name="poly">The polygon to draw.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="capType">The type of line cap to use.</param>
    /// <param name="capPoints">The number of points for the cap.</param>
    public static void DrawLines(this Polygon poly, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (poly.Count < 3) return;
        
        for (var i = 0; i < poly.Count; i++)
        {
            var start = poly[i];
            var end = poly[(i + 1) % poly.Count];
            SegmentDrawing.DrawSegment(start, end, lineThickness, color, capType, capPoints);
        }
    }

    /// <summary>
    /// Draws the polygon's outline with a uniform color, scaling each side by a factor.
    /// </summary>
    /// <param name="poly">The polygon to draw.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="sideLengthFactor">The factor by which to scale each side's length.</param>
    /// <param name="capType">The type of line cap to use.</param>
    /// <param name="capPoints">The number of points for the cap.</param>
    public static void DrawLines(this Polygon poly, float lineThickness, ColorRgba color, float sideLengthFactor,  LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (poly.Count < 3) return;
        
        for (var i = 0; i < poly.Count; i++)
        {
            var start = poly[i];
            var end = poly[(i + 1) % poly.Count];
            SegmentDrawing.DrawSegment(start, end, lineThickness, color, sideLengthFactor,  capType, capPoints);
        }
    }

    /// <summary>
    /// Draws the polygon's outline with a uniform color, applying position, size, and rotation.
    /// </summary>
    /// <param name="relative">The polygon to draw, with points relative to the origin.</param>
    /// <param name="pos">The position of the polygon's center.</param>
    /// <param name="size">The scale factor for the polygon.</param>
    /// <param name="rotDeg">The rotation in degrees.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="capType">The type of line cap to use.</param>
    /// <param name="capPoints">The number of points for the cap.</param>
    public static void DrawLines(this Polygon relative, Vector2 pos, float size, float rotDeg, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (relative.Count < 3) return;
        
        for (var i = 0; i < relative.Count; i++)
        {
            var start = pos + (relative[i] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
            var end = pos + (relative[(i + 1) % relative.Count] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
            SegmentDrawing.DrawSegment(start, end, lineThickness, color, capType, capPoints);
        }
    }

    /// <summary>
    /// Draws the polygon's outline with a uniform color, using a <see cref="Transform2D"/>.
    /// </summary>
    /// <param name="relative">The polygon to draw, with points relative to the origin.</param>
    /// <param name="transform">The transform to apply (position, scale, rotation).</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="capType">The type of line cap to use.</param>
    /// <param name="capPoints">The number of points for the cap.</param>
    public static void DrawLines(this Polygon relative, Transform2D transform, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        DrawLines(relative, transform.Position, transform.ScaledSize.Length, transform.RotationDeg, lineThickness, color, capType, capPoints);
    }

    /// <summary>
    /// Draws the polygon's outline using the provided <see cref="LineDrawingInfo"/>.
    /// </summary>
    /// <param name="poly">The polygon to draw.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    public static void DrawLines(this Polygon poly, LineDrawingInfo lineInfo) => DrawLines(poly, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);

    /// <summary>
    /// Draws the polygon's outline using the provided <see cref="LineDrawingInfo"/>, applying position, size, and rotation.
    /// </summary>
    /// <param name="relative">The polygon to draw, with points relative to the origin.</param>
    /// <param name="pos">The position of the polygon's center.</param>
    /// <param name="size">The scale factor for the polygon.</param>
    /// <param name="rotDeg">The rotation in degrees.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    public static void DrawLines(this Polygon relative, Vector2 pos, float size, float rotDeg, LineDrawingInfo lineInfo) => DrawLines(relative, pos, size, rotDeg, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);

    /// <summary>
    /// Draws the polygon's outline using the provided <see cref="LineDrawingInfo"/> and a <see cref="Transform2D"/>.
    /// </summary>
    /// <param name="relative">The polygon to draw, with points relative to the origin.</param>
    /// <param name="transform">The transform to apply (position, scale, rotation).</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    public static void DrawLines(this Polygon relative, Transform2D transform, LineDrawingInfo lineInfo) => DrawLines(relative, transform.Position, transform.ScaledSize.Length, transform.RotationDeg, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);

    /// <summary>
    /// Draws a certain amount of the polygon's perimeter as an outline.
    /// </summary>
    /// <param name="poly">The polygon to draw.</param>
    /// <param name="perimeterToDraw">
    /// The length of the perimeter to draw. If negative, draws in clockwise direction.
    /// </param>
    /// <param name="startIndex">The index of the vertex at which to start drawing.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="capType">The type of line cap to use.</param>
    /// <param name="capPoints">The number of points for the cap.</param>
    /// <remarks>
    /// Useful for animating outlines or drawing partial polygons.
    /// </remarks>
    public static void DrawLinesPerimeter(this Polygon poly, float perimeterToDraw, int startIndex, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        ShapeDrawing.DrawOutlinePerimeter(poly, perimeterToDraw, startIndex, lineThickness, color, capType, capPoints);
    }

    /// <summary>
    /// Draws a certain percentage of the polygon's outline.
    /// </summary>
    /// <param name="poly">The polygon to draw.</param>
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
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="capType">The type of line cap to use.</param>
    /// <param name="capPoints">The number of points for the cap.</param>
    /// <remarks>
    /// Useful for progress indicators or animated outlines.
    /// </remarks>
    public static void DrawLinesPercentage(this Polygon poly, float f, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        ShapeDrawing.DrawOutlinePercentage(poly, f, lineThickness, color, capType, capPoints);
    }

    /// <summary>
    /// Draws a certain percentage of the polygon's outline using <see cref="LineDrawingInfo"/>.
    /// </summary>
    /// <param name="poly">The polygon to draw.</param>
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
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    public static void DrawLinesPercentage(this Polygon poly, float f, LineDrawingInfo lineInfo)
    {
        ShapeDrawing.DrawOutlinePercentage(poly, f, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
    }

    /// <summary>
    /// Draws a circle at each vertex of the polygon.
    /// </summary>
    /// <param name="poly">The polygon whose vertices to draw.</param>
    /// <param name="vertexRadius">The radius of each vertex circle.</param>
    /// <param name="color">The color of the vertex circles.</param>
    /// <param name="circleSegments">The number of segments for each circle.</param>
    /// <remarks>
    /// Useful for debugging or highlighting polygon vertices.
    /// </remarks>
    public static void DrawVertices(this Polygon poly, float vertexRadius, ColorRgba color, int circleSegments)
    {
        foreach (var p in poly)
        {
            CircleDrawing.DrawCircle(p, vertexRadius, color, circleSegments);
        }
    }

    /// <summary>
    /// Draws lines from each corner of the polygon outward, with custom lengths for each corner.
    /// </summary>
    /// <param name="poly">The polygon to draw.</param>
    /// <param name="lineThickness">The thickness of the lines.</param>
    /// <param name="color">The color of the lines.</param>
    /// <param name="cornerLengths">A list of lengths for each corner. Cycles if fewer than corners.</param>
    /// <param name="capType">The type of line cap to use.</param>
    /// <param name="capPoints">The number of points for the cap.</param>
    /// <remarks>
    /// Useful for stylized or decorative polygon outlines.
    /// A corner is defined by three points: <c>previous [i-1]</c>, <c>current [i]</c>, and <c>next [i+1]</c>.
    /// </remarks>
    public static void DrawCornered(this Polygon poly, float lineThickness, ColorRgba color, List<float> cornerLengths, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        for (var i = 0; i < poly.Count; i++)
        {
            float cornerLength = cornerLengths[i%cornerLengths.Count];
            var prev = poly[(i - 1) % poly.Count];
            var cur = poly[i];
            var next = poly[(i + 1) % poly.Count];
            SegmentDrawing.DrawSegment(cur, cur + next.Normalize() * cornerLength, lineThickness, color, capType, capPoints);
            SegmentDrawing.DrawSegment(cur, cur + prev.Normalize() * cornerLength, lineThickness, color, capType, capPoints);
        }
    }

    /// <summary>
    /// Draws lines from each corner of the polygon outward, using relative factors for each corner.
    /// </summary>
    /// <param name="poly">The polygon to draw.</param>
    /// <param name="lineThickness">The thickness of the lines.</param>
    /// <param name="color">The color of the lines.</param>
    /// <param name="cornerFactors">A list of factors (0-1) for each corner. Cycles if fewer than corners.</param>
    /// <param name="capType">The type of line cap to use.</param>
    /// <param name="capPoints">The number of points for the cap.</param>
    /// <remarks>
    /// Each factor determines how far along the edge the corner line extends.
    /// A corner is defined by three points: <c>previous [i-1]</c>, <c>current [i]</c>, and <c>next [i+1]</c>.
    /// <c>CornerFactors</c> are used to interpolate from previous to current and from current to next point to form a corner.
    /// </remarks>
    public static void DrawCorneredRelative(this Polygon poly, float lineThickness, ColorRgba color, List<float> cornerFactors, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        for (var i = 0; i < poly.Count; i++)
        {
            float cornerF = cornerFactors[i%cornerFactors.Count];
            var prev = poly[(i - 1) % poly.Count];
            var cur = poly[i];
            var next = poly[(i + 1) % poly.Count];
            SegmentDrawing.DrawSegment(cur, cur.Lerp(next, cornerF), lineThickness, color, capType, capPoints);
            SegmentDrawing.DrawSegment(cur, cur.Lerp(prev, cornerF), lineThickness, color, capType, capPoints);
        }
    }

    /// <summary>
    /// Draws lines from each corner of the polygon outward, with custom lengths for each corner, using <see cref="LineDrawingInfo"/>.
    /// </summary>
    /// <param name="poly">The polygon to draw.</param>
    /// <param name="cornerLengths">A list of lengths for each corner. Cycles if fewer than corners.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <remarks>
    /// A corner is defined by three points: <c>previous [i-1]</c>, <c>current [i]</c>, and <c>next [i+1]</c>.
    /// </remarks>
    public static void DrawCornered(this Polygon poly, List<float> cornerLengths, LineDrawingInfo lineInfo)
    {
        for (var i = 0; i < poly.Count; i++)
        {
            float cornerLength = cornerLengths[i%cornerLengths.Count];
            var prev = poly[(i - 1) % poly.Count];
            var cur = poly[i];
            var next = poly[(i + 1) % poly.Count];
            SegmentDrawing.DrawSegment(cur, cur + next.Normalize() * cornerLength, lineInfo);
            SegmentDrawing.DrawSegment(cur, cur + prev.Normalize() * cornerLength, lineInfo);
        }
    }

    /// <summary>
    /// Draws lines from each corner of the polygon outward, using relative factors for each corner, with <see cref="LineDrawingInfo"/>.
    /// </summary>
    /// <param name="poly">The polygon to draw.</param>
    /// <param name="cornerFactors">A list of factors (0-1) for each corner. Cycles if fewer than corners.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <remarks>
    /// A corner is defined by three points: <c>previous [i-1]</c>, <c>current [i]</c>, and <c>next [i+1]</c>.
    /// <c>CornerFactors</c> are used to interpolate from previous to current and from current to next point to form a corner.
    /// </remarks>
    public static void DrawCorneredRelative(this Polygon poly, List<float> cornerFactors, LineDrawingInfo lineInfo)
    {
        for (var i = 0; i < poly.Count; i++)
        {
            float cornerF = cornerFactors[i%cornerFactors.Count];
            var prev = poly[(i - 1) % poly.Count];
            var cur = poly[i];
            var next = poly[(i + 1) % poly.Count];
            SegmentDrawing.DrawSegment(cur, cur.Lerp(next, cornerF), lineInfo);
            SegmentDrawing.DrawSegment(cur, cur.Lerp(prev, cornerF), lineInfo);
        }
    }

    /// <summary>
    /// Draws lines from each corner of the polygon outward, with a uniform length for all corners.
    /// </summary>
    /// <param name="poly">The polygon to draw.</param>
    /// <param name="lineThickness">The thickness of the lines.</param>
    /// <param name="color">The color of the lines.</param>
    /// <param name="cornerLength">The length of each corner line.</param>
    /// <param name="capType">The type of line cap to use.</param>
    /// <param name="capPoints">The number of points for the cap.</param>
    /// <remarks>
    /// A corner is defined by three points: <c>previous [i-1]</c>, <c>current [i]</c>, and <c>next [i+1]</c>.
    /// </remarks>
    public static void DrawCornered(this Polygon poly, float lineThickness, ColorRgba color, float cornerLength, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        for (var i = 0; i < poly.Count; i++)
        {
            var prev = poly[(i-1)%poly.Count];
            var cur = poly[i];
            var next = poly[(i+1)%poly.Count];
            SegmentDrawing.DrawSegment(cur, cur + next.Normalize() * cornerLength, lineThickness, color, capType, capPoints);
            SegmentDrawing.DrawSegment(cur, cur + prev.Normalize() * cornerLength, lineThickness, color, capType, capPoints);
        }
    }

    /// <summary>
    /// Draws lines from each corner of the polygon outward, using a uniform relative factor for all corners.
    /// </summary>
    /// <param name="poly">The polygon to draw.</param>
    /// <param name="lineThickness">The thickness of the lines.</param>
    /// <param name="color">The color of the lines.</param>
    /// <param name="cornerF">The factor (0-1) for all corners.</param>
    /// <param name="capType">The type of line cap to use.</param>
    /// <param name="capPoints">The number of points for the cap.</param>
    /// <remarks>
    /// A corner is defined by three points: <c>previous [i-1]</c>, <c>current [i]</c>, and <c>next [i+1]</c>.
    /// <c>CornerF</c> is used to interpolate from previous to current and from current to next point to form a corner.
    /// </remarks>
    public static void DrawCorneredRelative(this Polygon poly, float lineThickness, ColorRgba color, float cornerF, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        for (var i = 0; i < poly.Count; i++)
        {
            var prev = poly[(i - 1) % poly.Count];
            var cur = poly[i];
            var next = poly[(i + 1) % poly.Count];
            SegmentDrawing.DrawSegment(cur, cur.Lerp(next, cornerF), lineThickness, color, capType, capPoints);
            SegmentDrawing.DrawSegment(cur, cur.Lerp(prev, cornerF), lineThickness, color, capType, capPoints);
        }
    }

    /// <summary>
    /// Draws lines from each corner of the polygon outward, with a uniform length for all corners, using <see cref="LineDrawingInfo"/>.
    /// </summary>
    /// <param name="poly">The polygon to draw.</param>
    /// <param name="cornerLength">The length of each corner line.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <remarks>
    /// A corner is defined by three points: <c>previous [i-1]</c>, <c>current [i]</c>, and <c>next [i+1]</c>.
    /// </remarks>
    public static void DrawCornered(this Polygon poly, float cornerLength, LineDrawingInfo lineInfo)
    {
        for (var i = 0; i < poly.Count; i++)
        {
            var prev = poly[(i-1)%poly.Count];
            var cur = poly[i];
            var next = poly[(i+1)%poly.Count];
            SegmentDrawing.DrawSegment(cur, cur + next.Normalize() * cornerLength, lineInfo);
            SegmentDrawing.DrawSegment(cur, cur + prev.Normalize() * cornerLength, lineInfo);
        }
    }

    /// <summary>
    /// Draws lines from each corner of the polygon outward, using a uniform relative factor for all corners, with <see cref="LineDrawingInfo"/>.
    /// </summary>
    /// <param name="poly">The polygon to draw.</param>
    /// <param name="cornerF">The factor (0-1) for all corners.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <remarks>
    /// A corner is defined by three points: <c>previous [i-1]</c>, <c>current [i]</c>, and <c>next [i+1]</c>.
    /// <c>CornerF</c> is used to interpolate from previous to current and from current to next point to form a corner.
    /// </remarks>
    public static void DrawCorneredRelative(this Polygon poly, float cornerF, LineDrawingInfo lineInfo)
    {
        for (var i = 0; i < poly.Count; i++)
        {
            var prev = poly[(i - 1) % poly.Count];
            var cur = poly[i];
            var next = poly[(i + 1) % poly.Count];
            SegmentDrawing.DrawSegment(cur, cur.Lerp(next, cornerF), lineInfo);
            SegmentDrawing.DrawSegment(cur, cur.Lerp(prev, cornerF), lineInfo);
        }
    }

    /// <summary>
    /// Draws lines from each corner of the polygon outward, with a uniform length for all corners, using <see cref="LineDrawingInfo"/>.
    /// </summary>
    /// <param name="poly">The polygon to draw.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <param name="cornerLength">The length of each corner line.</param>
    /// <remarks>
    /// A corner is defined by three points: <c>previous [i-1]</c>, <c>current [i]</c>, and <c>next [i+1]</c>.
    /// </remarks>
    public static void DrawCornered(this Polygon poly, LineDrawingInfo lineInfo, float cornerLength) => DrawCornered(poly, lineInfo.Thickness,lineInfo.Color, cornerLength, lineInfo.CapType, lineInfo.CapPoints);

    /// <summary>
    /// Draws lines from each corner of the polygon outward, with custom lengths for each corner, using <see cref="LineDrawingInfo"/>.
    /// </summary>
    /// <param name="poly">The polygon to draw.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <param name="cornerLengths">A list of lengths for each corner. Cycles if fewer than corners.</param>
    /// <remarks>
    /// A corner is defined by three points: <c>previous [i-1]</c>, <c>current [i]</c>, and <c>next [i+1]</c>.
    /// </remarks>
    public static void DrawCornered(this Polygon poly, LineDrawingInfo lineInfo, List<float> cornerLengths) => DrawCornered(poly, lineInfo.Thickness,lineInfo.Color, cornerLengths, lineInfo.CapType, lineInfo.CapPoints);

    /// <summary>
    /// Draws lines from each corner of the polygon outward, using a uniform relative factor for all corners, with <see cref="LineDrawingInfo"/>.
    /// </summary>
    /// <param name="poly">The polygon to draw.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <param name="cornerF">The factor (0-1) for all corners.</param>
    /// <remarks>
    /// A corner is defined by three points: <c>previous [i-1]</c>, <c>current [i]</c>, and <c>next [i+1]</c>.
    /// <c>CornerF</c> is used to interpolate from previous to current and from current to next point to form a corner.
    /// </remarks>
    public static void DrawCorneredRelative(this Polygon poly, LineDrawingInfo lineInfo, float cornerF) => DrawCornered(poly, lineInfo.Thickness,lineInfo.Color, cornerF, lineInfo.CapType, lineInfo.CapPoints);

    /// <summary>
    /// Draws lines from each corner of the polygon outward, using relative factors for each corner, with <see cref="LineDrawingInfo"/>.
    /// </summary>
    /// <param name="poly">The polygon to draw.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <param name="cornerFactors">A list of factors (0-1) for each corner. Cycles if fewer than corners.</param>
    /// <remarks>
    /// A corner is defined by three points: <c>previous [i-1]</c>, <c>current [i]</c>, and <c>next [i+1]</c>.
    /// <c>CornerFactors</c> are used to interpolate from previous to current and from current to next point to form a corner.
    /// </remarks>
    public static void DrawCorneredRelative(this Polygon poly, LineDrawingInfo lineInfo, List<float> cornerFactors) => DrawCornered(poly, lineInfo.Thickness,lineInfo.Color, cornerFactors, lineInfo.CapType, lineInfo.CapPoints);

    /// <summary>
    /// Draws a polygon where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="poly">The polygon to draw.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
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
    /// Useful for creating stylized or animated polygon outlines.
    /// </remarks>
    public static void DrawLinesScaled(this Polygon poly, LineDrawingInfo lineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (poly.Count < 3) return;
        if (sideScaleFactor <= 0) return;
        
        if (sideScaleFactor >= 1)
        {
            poly.DrawLines(lineInfo);
            return;
        }
        for (var i = 0; i < poly.Count; i++)
        {
            var start = poly[i];
            var end = poly[(i + 1) % poly.Count];
            SegmentDrawing.DrawSegment(start, end, lineInfo, sideScaleFactor, sideScaleOrigin);
        }
        
    }

    /// <summary>
    /// Draws a polygon where each side can be scaled towards the origin of the side, applying position, size, and rotation.
    /// </summary>
    /// <param name="relative">The polygon to draw, with points relative to the origin.</param>
    /// <param name="pos">The position of the polygon's center.</param>
    /// <param name="size">The scale factor for the polygon.</param>
    /// <param name="rotDeg">The rotation in degrees.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
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
    public static void DrawLinesScaled(this Polygon relative, Vector2 pos, float size, float rotDeg, LineDrawingInfo lineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (relative.Count < 3) return;
        if (sideScaleFactor <= 0) return;
        
        if (sideScaleFactor >= 1)
        {
            relative.DrawLines(pos, size, rotDeg, lineInfo);
            return;
        }
        
        for (var i = 0; i < relative.Count; i++)
        {
            var start = pos + (relative[i] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
            var end = pos + (relative[(i + 1) % relative.Count] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
            SegmentDrawing.DrawSegment(start, end, lineInfo, sideScaleFactor, sideScaleOrigin);
        }
        
    }

    /// <summary>
    /// Draws a polygon where each side can be scaled towards the origin of the side, using a <see cref="Transform2D"/>.
    /// </summary>
    /// <param name="relative">The polygon to draw, with points relative to the origin.</param>
    /// <param name="transform">The transform to apply (position, scale, rotation).</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
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
    public static void DrawLinesScaled(this Polygon relative, Transform2D transform, LineDrawingInfo lineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        DrawLinesScaled(relative, transform.Position, transform.ScaledSize.Length, transform.RotationDeg, lineInfo, sideScaleFactor, sideScaleOrigin);

    }
    
    /// <summary>
    /// Draws the polygon with a glow effect, interpolating width and color along each segment.
    /// </summary>
    /// <param name="polygon">The polygon to draw.</param>
    /// <param name="width">The starting width of the glow. Should be bigger than <c>endWidth</c></param>
    /// <param name="endWidth">The ending width of the glow. Should be smaller than <c>width</c></param>
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
    public static void DrawGlow(this Polygon polygon, float width, float endWidth, ColorRgba color,
        ColorRgba endColorRgba, int steps, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (polygon.Count < 2 || steps <= 0) return;

        if (steps == 1)
        {
            DrawLines(polygon, width, color, capType, capPoints);
            return;
        }
    
        for (var s = 0; s < steps; s++)
        {
            var f = s / (float)(steps - 1);
            var currentWidth = ShapeMath.LerpFloat(width, endWidth, f);
            var currentColor = color.Lerp(endColorRgba, f);
            DrawLines(polygon, currentWidth, currentColor, capType, capPoints);
        }
    }
}