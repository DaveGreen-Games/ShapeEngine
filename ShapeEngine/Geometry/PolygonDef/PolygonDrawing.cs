
using System.Numerics;
using Clipper2Lib;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RectDef;
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
    #region Draw Masked
    /// <summary>
    /// Draws the polygon's edges while applying a triangular mask to each segment.
    /// </summary>
    /// <param name="poly">The polygon whose edges will be drawn. Must contain at least 3 points.</param>
    /// <param name="mask">The triangular mask used to clip each segment.</param>
    /// <param name="lineInfo">Line drawing options (thickness, color, cap type, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawLinesMasked(this Polygon poly, Triangle mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        if (poly.Count < 3) return;
        
        for (var i = 0; i < poly.Count; i++)
        {
            var start = poly[i];
            var end = poly[(i + 1) % poly.Count];
            var segment = new Segment(start, end);
            segment.DrawMasked(mask, lineInfo, reversedMask);
        }
    }
    /// <summary>
    /// Draws the polygon's edges while applying a circular mask to each segment.
    /// </summary>
    /// <param name="poly">The polygon whose edges will be drawn. Must contain at least 3 points.</param>
    /// <param name="mask">The circular mask used to clip each segment.</param>
    /// <param name="lineInfo">Line drawing options (thickness, color, cap type, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawLinesMasked(this Polygon poly, Circle mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        if (poly.Count < 3) return;
        
        for (var i = 0; i < poly.Count; i++)
        {
            var start = poly[i];
            var end = poly[(i + 1) % poly.Count];
            var segment = new Segment(start, end);
            segment.DrawMasked(mask, lineInfo, reversedMask);
        }
    }
    /// <summary>
    /// Draws the polygon's edges while applying a rectangular mask to each segment.
    /// </summary>
    /// <param name="poly">The polygon whose edges will be drawn. Must contain at least 3 points.</param>
    /// <param name="mask">The rectangular mask used to clip each segment.</param>
    /// <param name="lineInfo">Line drawing options (thickness, color, cap type, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawLinesMasked(this Polygon poly, Rect mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        if (poly.Count < 3) return;
        
        for (var i = 0; i < poly.Count; i++)
        {
            var start = poly[i];
            var end = poly[(i + 1) % poly.Count];
            var segment = new Segment(start, end);
            segment.DrawMasked(mask, lineInfo, reversedMask);
        }
    }
    /// <summary>
    /// Draws the polygon's edges while applying a quadrilateral mask to each segment.
    /// </summary>
    /// <param name="poly">The polygon whose edges will be drawn. Must contain at least 3 points.</param>
    /// <param name="mask">The quadrilateral mask used to clip each segment.</param>
    /// <param name="lineInfo">Line drawing options (thickness, color, cap type, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawLinesMasked(this Polygon poly, Quad mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        if (poly.Count < 3) return;
        
        for (var i = 0; i < poly.Count; i++)
        {
            var start = poly[i];
            var end = poly[(i + 1) % poly.Count];
            var segment = new Segment(start, end);
            segment.DrawMasked(mask, lineInfo, reversedMask);
        }
    }
    /// <summary>
    /// Draws the polygon's edges while applying a polygonal mask to each segment.
    /// </summary>
    /// <param name="poly">The polygon whose edges will be drawn. Must contain at least 3 points.</param>
    /// <param name="mask">The polygonal mask used to clip each segment.</param>
    /// <param name="lineInfo">Line drawing options (thickness, color, cap type, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawLinesMasked(this Polygon poly, Polygon mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        if (poly.Count < 3) return;
        
        for (var i = 0; i < poly.Count; i++)
        {
            var start = poly[i];
            var end = poly[(i + 1) % poly.Count];
            var segment = new Segment(start, end);
            segment.DrawMasked(mask, lineInfo, reversedMask);
        }
    }
    /// <summary>
    /// Draws the polygon's edges while applying a generic closed-shape mask to each segment.
    /// </summary>
    /// <typeparam name="T">Type of the mask implementing <see cref="IClosedShapeTypeProvider"/>.</typeparam>
    /// <param name="poly">The polygon whose edges will be drawn. Must contain at least 3 points.</param>
    /// <param name="mask">The mask used to clip each segment.</param>
    /// <param name="lineInfo">Line drawing options (thickness, color, cap type, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawLinesMasked<T>(this Polygon poly, T mask, LineDrawingInfo lineInfo, bool reversedMask = false) where T : IClosedShapeTypeProvider
    {
        if (poly.Count < 3) return;
        
        for (var i = 0; i < poly.Count; i++)
        {
            var start = poly[i];
            var end = poly[(i + 1) % poly.Count];
            var segment = new Segment(start, end);
            segment.DrawMasked(mask, lineInfo, reversedMask);
        }
    }
    #endregion
    
    #region Draw Convex
    /// <summary>
    /// Draws a convex non-intersecting polygon (pentagon, hexagon, etc.) filled with the specified color, using the polygon's centroid as the center.
    /// This function should be used when Polygon is known to be convex.
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
    /// Draws a convex non-intersecting polygon (pentagon, hexagon, etc.) filled with the specified color, using the polygon's centroid as the center.
    /// This function should be used when Polygon is known to be convex.
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
    #endregion
    
    #region Draw
    /// <summary>
    /// Draws the polygon filled with the provided color.
    /// </summary>
    /// <param name="poly">The polygon to draw. Polygons with fewer than 3 points are ignored; triangles are drawn directly.</param>
    /// <param name="color">Fill color used when rendering the polygon.</param>
    /// <remarks>
    /// Caution:This method will triangulate the polygon each call when the polygon contains more than 3 points,
    /// which can be performance-intensive for complex polygons.
    /// Precompute triangulation for best performance and then transform/draw the triangulation as needed.
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
    
    #endregion

    #region Draw Rounded
    
    /// <summary>
    /// Draws the polygon with rounded corners by generating a rounded copy and triangulating it (performance-intensive, see remarks!).
    /// </summary>
    /// <param name="poly">The source polygon. Must contain at least 3 points.</param>
    /// <param name="color">Fill color used to draw the rounded polygon.</param>
    /// <param name="cornerPoints">Number of interpolated points used for each rounded corner. If &lt;= 0, corners are not rounded.</param>
    /// <param name="cornerStrength">Controls how far the rounded corner deviates from the original corner. Range is [0-1] where 0 skips drawing and 1 draws max rounded corners; higher values produce larger arcs.</param>
    /// <param name="collinearAngleThresholdDeg">Angle in degrees below which adjacent edges are considered collinear and corner is not rounded (Corner Vertex is copied as is).</param>
    /// <param name="distanceThreshold">
    /// Minimum adjacent edge length required to attempt rounding. If either adjacent edge is shorter
    /// than this threshold the original vertex is preserved. Defaults to 1.0f.
    /// </param>
    /// <remarks>
    /// This method generates a new rounded polygon on each call which can be performance- and memory-intensive for complex polygons
    /// or high corner point counts. For best performance, precompute the rounded polygon and triangulation. Use <see cref="Polygon.RoundCopy"/> to create the rounded polygon,
    /// and <see cref="Polygon.Triangulate()"/> to create the triangulation. Then translate/rotate/scale/draw the triangulation as needed.
    /// </remarks>
    public static void DrawRounded(this Polygon poly, ColorRgba color, int cornerPoints, float cornerStrength = 0.5f, float collinearAngleThresholdDeg = 5f, float distanceThreshold = 1f)
    {
        if (poly.Count < 3) return;
        if (poly.Count == 3)
        {
            if (cornerPoints <= 0)
            {
                TriangleDrawing.DrawTriangle(poly[0], poly[1], poly[2], color);
            }
            else
            {
                TriangleDrawing.DrawTriangleRounded(poly[0], poly[1], poly[2], color, cornerPoints, cornerStrength);
            }
            return;
        }

        if (cornerPoints <= 0)
        {
            poly.Triangulate().Draw(color);
            return;
        }

        var roundedPoly = poly.RoundCopy(cornerPoints, cornerStrength, collinearAngleThresholdDeg, distanceThreshold);
        roundedPoly?.Triangulate().Draw(color);
    }
    
    #endregion
    
    #region Draw Lines Fast
    
    /// <summary>
    /// Draws each edge of the polygon using a fast segment renderer.
    /// This method is primarily optimized for performance and forces fully opaque colors
    /// (alpha is set to 255 before drawing).
    /// </summary>
    /// <param name="poly">The polygon whose edges will be drawn. Polygons with fewer than 3 points are ignored.</param>
    /// <param name="lineThickness">Thickness of the line in world units.</param>
    /// <param name="color">Color used to draw the lines. Alpha channel will be set to 255 internally.</param>
    /// <param name="capType">Specifies the style of the line caps (start/end).</param>
    /// <param name="capPoints">Number of points used to tessellate the caps.</param>
    public static void DrawLinesFast(this Polygon poly, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (poly.Count < 3) return;

        //Does not support transparent colors! Therefore, alpha is set to 255 for safety
        color = color.SetAlpha(255);
        
        for (var i = 0; i < poly.Count; i++)
        {
            var start = poly[i];
            var end = poly[(i + 1) % poly.Count];
            SegmentDrawing.DrawSegment(start, end, lineThickness, color, capType, capPoints);
        }
    }
    
    /// <summary>
    /// Draws each edge of the polygon using a fast segment renderer.
    /// This overload accepts a <see cref="LineDrawingInfo"/> to supply thickness, color and cap options.
    /// </summary>
    /// <param name="poly">The polygon whose edges will be drawn. Polygons with fewer than 3 points are ignored.</param>
    /// <param name="lineInfo">Line drawing options (thickness, color, cap type, etc.). Alpha will be forced to 255 internally because this renderer does not support transparency.</param>
    public static void DrawLinesFast(this Polygon poly, LineDrawingInfo lineInfo)
    {
        if (poly.Count < 3) return;
        
        //Does not support transparent colors! Therefore, alpha is set to 255 for safety
        var color = lineInfo.Color.SetAlpha(255);
        
        for (var i = 0; i < poly.Count; i++)
        {
            var start = poly[i];
            var end = poly[(i + 1) % poly.Count];
            SegmentDrawing.DrawSegment(start, end, lineInfo.Thickness, color, lineInfo.CapType, lineInfo.CapPoints);
        }
    }
    #endregion
    
    #region Draw Lines
    
    /// <summary>
    /// Draws the polygon outline with configurable thickness and corner style.
    /// <list type="bullet">
    /// <item>If <paramref name="drawFastForOpaque"/> is true and the color is fully opaque (alpha = 255) <see cref="DrawLinesFast(Polygon, float, ColorRgba, LineCapType, int)"/> is used instead.</item>
    /// <item>This function uses two complex functions (inflate, triangulate) to generate the outline (every call) for any type of polygon, which comes with performance costs and memory allocations downsides.</item>
    /// <item>Use <see cref="Polygon.GenerateOutlineTriangulation(float, int, float, bool, bool)"/> to generate the outline triangulation once and then translate/rotate/scale the triangulation as needed and draw it instead.</item>
    /// </list>
    /// </summary>
    /// <param name="poly">The polygon to draw. Polygons with fewer than 3 points are ignored.</param>
    /// <param name="lineThickness">Thickness of the outline in world units.</param>
    /// <param name="color">Color used to draw the outline.</param>
    /// <param name="cornerPoints">
    /// Number of interpolation points used for rounded corners:
    /// <list type="bullet">
    /// <item> 0: use non-rounded joins either mitered (<paramref name="miterLimit"/> &gt;= 2) or beveled (<paramref name="miterLimit"/> &lt; 2).</item>
    /// <item> &gt; 0: render rounded joins and control arc tessellation with this value.</item>
    /// </list>
    /// </param>
    /// <param name="miterLimit">
    /// This property sets the maximum distance in multiples of <paramref name="lineThickness"/> that vertices can be offset from their original positions before squaring is applied.
    /// Very acute angles can produce excessively long miters which may not be visually desirable. This limit helps control that.
    /// </param>
    /// <param name="drawFastForOpaque">
    /// When true (default) and the supplied color is fully opaque (alpha == 255),
    /// the method will use the optimized, non‑transparent renderer <see cref="DrawLinesFast(Polygon, float, ColorRgba, LineCapType, int)"/> for better performance.
    /// </param>
    public static void DrawLines(this Polygon poly, float lineThickness, ColorRgba color, int cornerPoints = 0, float miterLimit = 2f, bool drawFastForOpaque = true)
    {
        if (drawFastForOpaque && color.A >= 255)
        {
            poly.DrawLinesFast(lineThickness, color, LineCapType.CappedExtended, cornerPoints);
            return;
        }
        DrawLinesHelper(poly, lineThickness, color, cornerPoints, miterLimit, true);
    }
    
    /// <summary>
    /// Draws the polygon outline with configurable LineDrawingInfo and miterLimit.
    /// <list type="bullet">
    /// <item>If <paramref name="drawFastForOpaque"/> is true and the color is fully opaque (alpha = 255) <see cref="DrawLinesFast(Polygon, float, ColorRgba, LineCapType, int)"/> is used instead.</item>
    /// <item>This function uses two complex functions (inflate, triangulate) to generate the outline (every call) for any type of polygon, which comes with performance costs and memory allocations downsides.</item>
    /// <item>Use <see cref="Polygon.GenerateOutlineTriangulation(float, int, float, bool, bool)"/> to generate the outline triangulation once and then translate/rotate/scale the triangulation as needed and draw it instead.</item>
    /// </list>
    /// </summary>
    /// <param name="poly">The polygon to draw. Polygons with fewer than 3 points are ignored.</param>
    /// <param name="lineInfo">The <see cref="LineDrawingInfo"/> containing line drawing parameters.
    /// The <see cref="LineDrawingInfo.CapPoints"/> is used as corner points parameter:
    /// <list type="bullet">
    /// <item> 0: use non-rounded joins either mitered (<paramref name="miterLimit"/> &gt;= 2) or beveled (<paramref name="miterLimit"/> &lt; 2).</item>
    /// <item> &gt; 0: render rounded joins and control arc tessellation with this value.</item>
    /// </list>
    /// </param>
    /// <param name="miterLimit">
    /// This property sets the maximum distance in multiples of <paramref name="lineInfo.Thickness"/> that vertices can be offset from their original positions before squaring is applied.
    /// Very acute angles can produce excessively long miters which may not be visually desirable. This limit helps control that.
    /// </param>
    /// <param name="drawFastForOpaque">
    /// When true (default) and the supplied color is fully opaque (alpha == 255),
    /// the method will use the optimized, non‑transparent renderer <see cref="DrawLinesFast(Polygon, float, ColorRgba, LineCapType, int)"/> for better performance.
    /// </param>
    public static void DrawLines(this Polygon poly, LineDrawingInfo lineInfo, float miterLimit = 2f, bool drawFastForOpaque = true)
    {
        if (drawFastForOpaque && lineInfo.Color.A >= 255)
        {
            poly.DrawLinesFast(lineInfo);
            return;
        }
        DrawLinesHelper(poly, lineInfo.Thickness, lineInfo.Color, lineInfo.CapPoints, miterLimit, true);
    }
    
    #endregion
    
    #region Draw Lines Glow
    /// <summary>
    /// Draws a stacked set of outlines around the polygon to create a glow/halo effect.
    /// Each step interpolates width and color from the inner (width / color) to the outer (endWidth / endColorRgba) outline.
    /// </summary>
    /// <param name="polygon">The polygon to render the glow for. Nothing is drawn when the polygon has fewer than 3 points.</param>
    /// <param name="width">Starting thickness for the innermost outline.</param>
    /// <param name="endWidth">Ending thickness for the outermost outline.</param>
    /// <param name="color">Starting color for the innermost outline.</param>
    /// <param name="endColorRgba">Ending color for the outermost outline.</param>
    /// <param name="steps">Number of interpolation steps. If &lt;= 0 no drawing occurs; if 1 a single outline is drawn using <paramref name="width"/> and <paramref name="color"/>.</param>
    /// <param name="cornerPoints">Number of points used to tessellate rounded corners when drawing each outline. When &gt; 0 rounded joins are used.</param>
    /// <param name="miterLimit">Miter limit passed to the line drawing routine to control mitered joins.</param>
    public static void DrawLinesGlow(this Polygon polygon, float width, float endWidth, ColorRgba color, ColorRgba endColorRgba, int steps, int cornerPoints = 0, float miterLimit = 2f)
    {
        if (polygon.Count < 3 || steps <= 0) return;

        if (steps == 1)
        {
            polygon.DrawLines(width, color, cornerPoints);
            return;
        }
    
        for (var s = 0; s < steps; s++)
        {
            float f = s / (float)(steps - 1);
            float currentWidth = ShapeMath.LerpFloat(width, endWidth, f);
            var currentColor = color.Lerp(endColorRgba, f);
            polygon.DrawLines(currentWidth, currentColor, cornerPoints, miterLimit);
        }
    }
    #endregion
    
    
    
    #region Draw Lines Perimeter & Percentage
    //!!!: Keep the existing functions and call the fast (no transparency) for now - make performance tests later
    //NOTE: This is essentially a polyline
    // - Still needs full line drawing info for end caps
    //TODO: Add a function that returns last vertex index and interpolated point
    // - Have 1 DrawLinesPerimeter function that calls an internal polyline helper function that will use the new function to get last vertex index and interpolated point
    
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
    #endregion
    
    #region Draw Cornered
    //TODO: Use this version and call it fast (supports no transparency)
    //TODO: Make new version that supports transparency
    
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
    /// Draws lines from each corner of the polygon outward, with a uniform length for all corners, using <see cref="LineDrawingInfo"/>.
    /// </summary>
    /// <param name="poly">The polygon to draw.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <param name="cornerLength">The length of each corner line.</param>
    /// <remarks>
    /// A corner is defined by three points: <c>previous [i-1]</c>, <c>current [i]</c>, and <c>next [i+1]</c>.
    /// </remarks>
    public static void DrawCornered(this Polygon poly, LineDrawingInfo lineInfo, float cornerLength)
    {
        DrawCornered(poly, lineInfo.Thickness, lineInfo.Color, cornerLength, lineInfo.CapType, lineInfo.CapPoints);
    }

    /// <summary>
    /// Draws lines from each corner of the polygon outward, with custom lengths for each corner, using <see cref="LineDrawingInfo"/>.
    /// </summary>
    /// <param name="poly">The polygon to draw.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <param name="cornerLengths">A list of lengths for each corner. Cycles if fewer than corners.</param>
    /// <remarks>
    /// A corner is defined by three points: <c>previous [i-1]</c>, <c>current [i]</c>, and <c>next [i+1]</c>.
    /// </remarks>
    public static void DrawCornered(this Polygon poly, LineDrawingInfo lineInfo, List<float> cornerLengths)
    {
        DrawCornered(poly, lineInfo.Thickness, lineInfo.Color, cornerLengths, lineInfo.CapType, lineInfo.CapPoints);
    }

    #endregion

    
    
    #region Draw Vertices
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
    #endregion
    
    #region Draw Lines Scaled
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
    #endregion


    
    
    #region Helper
    private static void DrawLinesHelper(Polygon poly, float thickness, ColorRgba color, int cornerPoints = 0, float miterLimit = 2f, bool beveled = false)
    {
        if (poly.Count <= 2) return;
        if (poly.Count == 3)
        {
            TriangleDrawing.DrawTriangleLines(poly[0], poly[1], poly[2], thickness, color, cornerPoints);
            return;
        }

        ShapeClipperJoinType joinType;
        if (cornerPoints > 0)
        {
            joinType = ShapeClipperJoinType.Round;
        }
        else
        {
            if (miterLimit >= 2f)
            {
                joinType = ShapeClipperJoinType.Miter;
            }
            else
            {
                joinType = beveled ? ShapeClipperJoinType.Bevel : ShapeClipperJoinType.Square;
            }
        }
        double arcTolerance = cornerPoints <= 0 ? 0.0 : thickness / (cornerPoints * 2);
        var result = poly.Inflate(thickness, joinType, ShapeClipperEndType.Joined, miterLimit, 2, arcTolerance);
        if (result.Count <= 0) return;

        if (result.Count == 1)
        {
            var polygon = result[0].ToPolygon();
            polygon.Draw(color);
        }
        else
        {
            var triangulationResult = Clipper.Triangulate(result, 4, out var solution, false);
            if (triangulationResult == TriangulateResult.success)
            {
                var rayColor = color.ToRayColor();
                foreach (var path in solution)
                {
                    if(path.Count < 3) continue;
                    Raylib.DrawTriangle(path[0].ToVec2(), path[1].ToVec2(), path[2].ToVec2(), rayColor);
                }
            }
        }
    }
    #endregion
}




    

