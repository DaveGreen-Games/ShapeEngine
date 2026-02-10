
using System.Drawing;
using System.Numerics;
using Clipper2Lib;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.Geometry.TriangulationDef;
using ShapeEngine.StaticLib;
using Ray = ShapeEngine.Geometry.RayDef.Ray;

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
    #region Helper Members
    private static Triangulation drawHelperTriangulation = [];
    private static Polygon drawRoundedHelperPolygon = [];
    #endregion
    
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
    /// <param name="multiThreaded">If true, the method avoids shared helper collections and uses local allocations (calls that return new triangulations).
    /// This makes the call safe for concurrent threads but increases temporary allocations and CPU work.</param>
    /// <remarks>
    /// Caution:This method will triangulate the polygon each call when the polygon contains more than 3 points,
    /// which can be performance-intensive for complex polygons.
    /// Precompute triangulation for best performance and then transform/draw the triangulation as needed.
    /// </remarks>
    public static void Draw(this Polygon poly, ColorRgba color, bool multiThreaded = false)
    {
        if (poly.Count < 3) return;
        if (poly.Count == 3)
        {
            TriangleDrawing.DrawTriangle(poly[0], poly[1], poly[2], color);
            return;
        }

        if (multiThreaded)
        {
            poly.Triangulate().Draw(color);
        }
        else
        {
            drawHelperTriangulation.Clear();
            poly.Triangulate(ref drawHelperTriangulation);
            drawHelperTriangulation.Draw(color);
        }
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
    /// <param name="multiThreaded">If true, the method avoids shared helper collections and uses local allocations (calls that return new triangulations).
    /// This makes the call safe for concurrent threads but increases temporary allocations and CPU work.</param>
    /// <remarks>
    /// This method generates a new rounded polygon on each call which can be performance- and memory-intensive for complex polygons
    /// or high corner point counts. For best performance, precompute the rounded polygon and triangulation. Use <see cref="Polygon.RoundCopy(int, float, float, float)"/> to create the rounded polygon,
    /// and <see cref="Polygon.Triangulate()"/> to create the triangulation. Then translate/rotate/scale/draw the triangulation as needed.
    /// </remarks>
    public static void DrawRounded(this Polygon poly, ColorRgba color, int cornerPoints, float cornerStrength = 0.5f, float collinearAngleThresholdDeg = 5f, float distanceThreshold = 1f, bool multiThreaded = false)
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
            if (multiThreaded)
            {
                poly.Triangulate().Draw(color);
            }
            else
            {
                drawHelperTriangulation.Clear();
                poly.Triangulate(ref drawHelperTriangulation);
                drawHelperTriangulation.Draw(color);
            }
            return;
        }

        if (multiThreaded)
        {
            var success = poly.RoundCopy(ref drawRoundedHelperPolygon, cornerPoints, cornerStrength, collinearAngleThresholdDeg, distanceThreshold);
            if (!success) return;
        
            drawHelperTriangulation.Clear();
            drawRoundedHelperPolygon.Triangulate(ref drawHelperTriangulation);
            drawHelperTriangulation.Draw(color);
        }
        else
        {
            var roundedPoly = poly.RoundCopy(cornerPoints, cornerStrength, collinearAngleThresholdDeg, distanceThreshold);
            roundedPoly?.Triangulate().Draw(color);
        }
    }
    
    #endregion
    
    
    //TODO: Implement transparent version
    #region Draw Lines Perimeter & Percentage
    
    /// <summary>
    /// Draws a certain amount of the polygon's perimeter as an outline.
    /// This method is primarily optimized for performance and forces fully opaque colors
    /// (alpha is set to 255 before drawing).
    /// </summary>
    /// <param name="poly">The polygon to draw.</param>
    /// <param name="perimeterToDraw">
    /// The length of the perimeter to draw. If negative, draws in clockwise direction.
    /// </param>
    /// <param name="startIndex">The index of the vertex at which to start drawing.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline. Only fully opaque colors are supported. Alpha is set 255.</param>
    /// <param name="capType">The type of line cap to use.</param>
    /// <param name="capPoints">The number of points for the cap.</param>
    /// <remarks>
    /// Useful for animating outlines or drawing partial polygons.
    /// Use <see cref="Polygon.GenerateOutlinePerimeterTriangulation(float, int, float, int, float, bool, bool)"/> to create a triangulation that can be draw with transparent colors.
    /// </remarks>
    public static void DrawLinesPerimeter(this Polygon poly, float perimeterToDraw, int startIndex, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (poly.Count < 3 || perimeterToDraw == 0) return;

        color = color.SetAlpha(255);
        
        int currentIndex = ShapeMath.Clamp(startIndex, 0, poly.Count - 1);

        bool reverse = perimeterToDraw < 0;
        if (reverse) perimeterToDraw *= -1;

        for (var i = 0; i < poly.Count; i++)
        {
            var start = poly[currentIndex];
            if (reverse) currentIndex = ShapeMath.WrapIndex(poly.Count, currentIndex - 1);
            else currentIndex = (currentIndex + 1) % poly.Count;
            var end = poly[currentIndex];
            var l = (end - start).Length();
            if (l <= perimeterToDraw)
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
    /// Draws a certain percentage of the polygon's outline.
    /// This method is primarily optimized for performance and forces fully opaque colors
    /// (alpha is set to 255 before drawing).
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
    /// <param name="color">The color of the outline. Only fully opaque colors a supported. Sets alpha to 255. </param>
    /// <param name="capType">The type of line cap to use.</param>
    /// <param name="capPoints">The number of points for the cap.</param>
    /// <remarks>
    /// Useful for progress indicators or animated outlines.
    /// Use <see cref="Polygon.GenerateOutlinePercentageTriangulation(float, float, float, int, float, bool, bool)"/> to create a triangulation that can be draw with transparent colors.
    /// </remarks>
    public static void DrawLinesPercentage(this Polygon poly, float f, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (poly.Count < 3 || f == 0f) return;

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
            poly.DrawLines(lineThickness, color, capType, capPoints);
            return;
        }

        float perimeter = 0f;
        for (var i = 0; i < poly.Count; i++)
        {
            var start = poly[i];
            var end = poly[(i + 1) % poly.Count];
            var l = (end - start).Length();
            perimeter += l;
        }

        poly.DrawLinesPerimeter(perimeter * f * (negative ? -1 : 1), startIndex, lineThickness, color, capType, capPoints);
    }

    /// <summary>
    /// Draws a certain percentage of the polygon's outline using <see cref="LineDrawingInfo"/>.
    /// This method is primarily optimized for performance and forces fully opaque colors
    /// (alpha is set to 255 before drawing).
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
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).
    /// Only fully opaque colors are supported. Alpha is set to 255 internally.
    /// </param>
    /// <remarks>
    /// Useful for progress indicators or animated outlines.
    /// Use <see cref="Polygon.GenerateOutlinePercentageTriangulation(float, float, float, int, float, bool, bool)"/> to create a triangulation that can be draw with transparent colors.
    /// </remarks>
    public static void DrawLinesPercentage(this Polygon poly, float f, LineDrawingInfo lineInfo)
    {
        poly.DrawLinesPercentage(f, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
    }
    
    #endregion

    
    #region Draw Lines
    /// <summary>
    /// Draws each edge of the polygon using a fast segment renderer with an adjustable side length factor.
    /// This renderer forces fully opaque colors (alpha is set to 255 internally) and ignores polygons with fewer than 3 points.
    /// </summary>
    /// <param name="polygon">The polygon whose edges will be drawn.</param>
    /// <param name="lineThickness">Thickness of the line in world units.</param>
    /// <param name="color">Color used to draw the lines. Alpha channel will be forced to 255 internally.</param>
    /// <param name="sideLengthFactor">Scale factor applied to each side's length (0 = no line, 1 = full side length).</param>
    /// <param name="capType">Specifies the style of the line caps (start/end).</param>
    /// <param name="capPoints">Number of points used to tessellate the caps.</param>
    public static void DrawLines(this Polygon polygon, float lineThickness, ColorRgba color, float sideLengthFactor, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (polygon.Count < 3) return;
        
        //Does not support transparent colors! Therefore, alpha is set to 255 for safety
        color = color.SetAlpha(255);
        
        for (var i = 0; i < polygon.Count; i++)
        {
            var start = polygon[i];
            var end = polygon[(i + 1) % polygon.Count];
            SegmentDrawing.DrawSegment(start, end, lineThickness, color, sideLengthFactor,  capType, capPoints);
        }
    }
   
    /// <summary>
    /// Draws each edge of the polygon using a fast segment renderer with a color gradient from <paramref name="startColorRgba"/> to <paramref name="endColorRgba"/>.
    /// The colors are interpolated per edge across the polygon's perimeter. Polygons with fewer than 3 points are ignored.
    /// </summary>
    /// <param name="polygon">The polygon whose edges will be drawn.</param>
    /// <param name="lineThickness">Thickness of the line in world units.</param>
    /// <param name="startColorRgba">Color used for the first edge. The renderer forces the alpha channel to fully opaque (255).</param>
    /// <param name="endColorRgba">Color used for the last edge. The renderer forces the alpha channel to fully opaque (255).</param>
    /// <param name="capType">Specifies the style of the line caps (start/end).</param>
    /// <param name="capPoints">Number of points used to tessellate the caps.</param>
    public static void DrawLines(this Polygon polygon, float lineThickness, ColorRgba startColorRgba, ColorRgba endColorRgba, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (polygon.Count < 3) return;

        //Does not support transparent colors! Therefore, alpha is set to 255 for safety
        startColorRgba = startColorRgba.SetAlpha(255);
        endColorRgba = endColorRgba.SetAlpha(255);
        
        int redStep = (endColorRgba.R - startColorRgba.R) / polygon.Count;
        int greenStep = (endColorRgba.G - startColorRgba.G) / polygon.Count;
        int blueStep = (endColorRgba.B - startColorRgba.B) / polygon.Count;
        int alphaStep = (endColorRgba.A - startColorRgba.A) / polygon.Count;
        for (var i = 0; i < polygon.Count; i++)
        {
            var start = polygon[i];
            var end = polygon[(i + 1) % polygon.Count];
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
    /// Draws each edge of the polygon using a fast segment renderer.
    /// This method is primarily optimized for performance and forces fully opaque colors
    /// (alpha is set to 255 before drawing).
    /// </summary>
    /// <param name="poly">The polygon whose edges will be drawn. Polygons with fewer than 3 points are ignored.</param>
    /// <param name="lineThickness">Thickness of the line in world units.</param>
    /// <param name="color">Color used to draw the lines. Alpha channel will be set to 255 internally.</param>
    /// <param name="capType">Specifies the style of the line caps (start/end).</param>
    /// <param name="capPoints">Number of points used to tessellate the caps.</param>
    public static void DrawLines(this Polygon poly, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
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
    public static void DrawLines(this Polygon poly, LineDrawingInfo lineInfo)
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

    
    //TODO: Add Draw Lines functions for convex polygons that do not use Inflate + Triangulation!
    #region Draw Lines Convex

    

    #endregion
    
    #region Draw Lines Transparent
    
    //Todo: Try to use DrawCornerAbsoluteTransparent algorithm to replace inflate + triangulate here
    // - Generating outline triangulation will still be available!
    
    /// <summary>
    /// Draws the polygon outline with configurable thickness and corner style.
    /// <list type="bullet">
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
    public static void DrawLinesTransparent(this Polygon poly, float lineThickness, ColorRgba color, int cornerPoints = 0, float miterLimit = 2f)
    {
        DrawLinesHelper(poly, lineThickness, color, cornerPoints, miterLimit, true);
    }
    
    /// <summary>
    /// Draws the polygon outline with configurable LineDrawingInfo and miterLimit.
    /// <list type="bullet">
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
    public static void DrawLinesTransparent(this Polygon poly, LineDrawingInfo lineInfo, float miterLimit = 2f)
    {
        DrawLinesHelper(poly, lineInfo.Thickness, lineInfo.Color, lineInfo.CapPoints, miterLimit, true);
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
    
    
    //TODO: Add xml summaries
    #region Draw Cornered Absolute Transparent
    private static void DrawCornerAbsoluteTransparent(Vector2 prev, Vector2 corner, Vector2 next, float cornerLength, float lineThickness, ColorRgba color, 
        LineCapType capType, int capPoints, float miterLimit = 2f, bool beveled = false)
    {
        var wPrev = corner - prev;
        var wNext = next - corner;
        
        var dirPrev = wPrev.Normalize();
        var dirNext = wNext.Normalize();
        
        //flip based on corner type
        var cornerType = dirPrev.ClassifyCorner(dirNext);
        Vector2 normalPrev, normalNext;
        if (cornerType.type >= 0)
        {
            normalPrev = dirPrev.GetPerpendicularRight();
            normalNext = dirNext.GetPerpendicularRight();
        }
        else
        {
            normalPrev = dirPrev.GetPerpendicularLeft();
            normalNext = dirNext.GetPerpendicularLeft();
        }
        
        prev = corner - dirPrev * cornerLength;
        next = corner + dirNext * cornerLength;
        
        if (cornerType.type == 0)//collinear
        {
            SegmentDrawing.DrawSegment(prev, next, lineThickness, color, capType, capPoints);
            return ;
        }

        var rayColor = color.ToRayColor();
        float totalMiterLengthLimit = (lineThickness * 0.5f) * MathF.Max(2f, miterLimit);
        var miterDir = (normalPrev + normalNext).Normalize();
        float miterAngleRad = MathF.Abs(miterDir.AngleRad(normalNext));
        float miterLength = lineThickness / MathF.Cos(miterAngleRad);
        
        if (miterLimit < 2f || miterLength < totalMiterLengthLimit)
        {
            var cornerOuter = corner + miterDir * miterLimit;
            var prevOuter = prev + normalPrev * lineThickness;
            var prevInner = prev - normalPrev * lineThickness;
            var nextOuter = next + normalNext * lineThickness;
            var nextInner = next - normalNext * lineThickness;
            var intersection = Ray.IntersectRayRay(prevInner, dirPrev, nextInner, -dirNext);
            var cornerInner = intersection.Valid ? intersection.Point : corner - miterDir * miterLimit;

            if (cornerType.type >= 0)
            {
                Raylib.DrawTriangle(cornerInner, prevInner, prevOuter, rayColor);
                Raylib.DrawTriangle(cornerInner, prevOuter, cornerOuter, rayColor);
                Raylib.DrawTriangle(cornerInner, cornerOuter, nextOuter, rayColor);
                Raylib.DrawTriangle(cornerInner, nextOuter, nextInner, rayColor);
            }
            else
            {
                Raylib.DrawTriangle(prevInner,cornerInner, prevOuter, rayColor);
                Raylib.DrawTriangle(prevOuter,cornerInner, cornerOuter, rayColor);
                Raylib.DrawTriangle(cornerOuter,cornerInner, nextOuter, rayColor);
                Raylib.DrawTriangle(nextOuter,cornerInner, nextInner, rayColor);
            }
            
        }
        else
        {
            miterLimit = totalMiterLengthLimit;
            Vector2 cornerOuterPrev, cornerOuterNext;
            
            var prevOuter = prev + normalPrev * lineThickness;
            var prevInner = prev - normalPrev * lineThickness;
            var nextOuter = next + normalNext * lineThickness;
            var nextInner = next - normalNext * lineThickness;
            var intersection = Ray.IntersectRayRay(prevInner, dirPrev, nextInner, -dirNext);
            var cornerInner = intersection.Valid ? intersection.Point : corner - miterDir * miterLimit;
            
            if (beveled)
            {
                cornerOuterPrev = corner + normalPrev * lineThickness;
                cornerOuterNext = corner + normalNext * lineThickness;
            }
            else
            {
                
                var cornerOuter = corner + miterDir * miterLimit;
                var miterPerpRight = cornerType.type >= 0 ? miterDir.GetPerpendicularRight() : miterDir.GetPerpendicularLeft();
                intersection = Ray.IntersectRayRay(prevOuter, dirPrev, cornerOuter, miterPerpRight);
                if (intersection.Valid)
                {
                    cornerOuterPrev = intersection.Point;
                    float l = (cornerOuter - intersection.Point).Length();
                    cornerOuterNext = cornerOuter - miterPerpRight * l;
                }
                else //bevel fallback
                {
                    cornerOuterPrev = corner + normalPrev * lineThickness;
                    cornerOuterNext = corner + normalNext * lineThickness;
                }
                
            }

            if (cornerType.type >= 0)
            {
                Raylib.DrawTriangle(cornerInner, prevInner, prevOuter, rayColor);
                Raylib.DrawTriangle(cornerInner, prevOuter, cornerOuterPrev, rayColor);
                Raylib.DrawTriangle(cornerInner, cornerOuterNext, nextOuter, rayColor);
                Raylib.DrawTriangle(cornerInner, nextOuter, nextInner, rayColor);
                Raylib.DrawTriangle(cornerInner, cornerOuterPrev, cornerOuterNext, rayColor);
            }
            else
            {
                Raylib.DrawTriangle(prevInner,cornerInner, prevOuter, rayColor);
                Raylib.DrawTriangle(prevOuter,cornerInner, cornerOuterPrev, rayColor);
                Raylib.DrawTriangle(cornerOuterNext,cornerInner, nextOuter, rayColor);
                Raylib.DrawTriangle(nextOuter,cornerInner, nextInner, rayColor);
                Raylib.DrawTriangle(cornerOuterPrev,cornerInner, cornerOuterNext, rayColor);
            }
            
        }

        if (capType is LineCapType.Capped or LineCapType.CappedExtended && capPoints > 0)
        {
            SegmentDrawing.DrawRoundCap(prev, -dirPrev, lineThickness, capPoints, color);
            SegmentDrawing.DrawRoundCap(next, dirNext, lineThickness, capPoints, color);
        }
    }
    
    public static void DrawCorneredAbsoluteTransparent(this Polygon poly, float lineThickness, ColorRgba color, float cornerLength, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2,
        float miterLimit = 2f, bool beveled = false)
    {
        for (var i = 0; i < poly.Count; i++)
        {
            var prev = poly[ShapeMath.WrapIndex(poly.Count, i - 1)];
            var cur = poly[i];
            var next = poly[ShapeMath.WrapIndex(poly.Count, i + 1)];
            
            var wPrev = cur - prev;
            var wNext = next - cur;
            float lsPrev = wPrev.LengthSquared();
            float lsNext = wNext.LengthSquared();
            if(lsPrev <= 0 || lsNext <= 0) return;
        
            float minLength = MathF.Sqrt(MathF.Min(lsPrev, lsNext));
            float newLineThickness = MathF.Min(lineThickness, MathF.Min(cornerLength * 0.5f, minLength * 0.25f));
            float newCornerLength;
            
            if (capType is LineCapType.None)
            {
                newCornerLength = MathF.Min(cornerLength, minLength * 0.5f);
            }
            else if (capType is LineCapType.Extended)
            {
                newCornerLength = MathF.Min(cornerLength + newLineThickness, minLength * 0.5f);
            
            }
            else if (capType is LineCapType.Capped)
            {
                newCornerLength = MathF.Min(cornerLength - newLineThickness, minLength * 0.5f - newLineThickness);
            }
            else //Capped Extended
            {
                newCornerLength = MathF.Min(cornerLength, minLength * 0.5f - newLineThickness);
            }
        
            if(newCornerLength <= 0) continue;
            
            DrawCornerAbsoluteTransparent(prev, cur, next, newCornerLength, newLineThickness, color, capType, capPoints, miterLimit, beveled);
        }
    }
    public static void DrawCorneredAbsoluteTransparent(this Polygon poly, float cornerLength, LineDrawingInfo lineInfo, float miterLimit = 2f, bool beveled = false)
    {
        poly.DrawCorneredAbsoluteTransparent(lineInfo.Thickness, lineInfo.Color, cornerLength, lineInfo.CapType, lineInfo.CapPoints, miterLimit, beveled);
    }
   
    public static void DrawCorneredAbsoluteTransparent(this Polygon poly, float lineThickness, ColorRgba color, List<float> cornerLengths, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2,
        float miterLimit = 2f, bool beveled = false)
    {
        if (cornerLengths.Count <= 0) return;
        for (var i = 0; i < poly.Count; i++)
        {
            float cornerLength = cornerLengths[i % cornerLengths.Count];
            var prev = poly[ShapeMath.WrapIndex(poly.Count, i - 1)];
            var cur = poly[i];
            var next = poly[ShapeMath.WrapIndex(poly.Count, i + 1)];
            
            var wPrev = cur - prev;
            var wNext = next - cur;
            float lsPrev = wPrev.LengthSquared();
            float lsNext = wNext.LengthSquared();
            if(lsPrev <= 0 || lsNext <= 0) return;
        
            float minLength = MathF.Sqrt(MathF.Min(lsPrev, lsNext));
            float newLineThickness = MathF.Min(lineThickness, MathF.Min(cornerLength * 0.5f, minLength * 0.25f));
            float newCornerLength;
            
            if (capType is LineCapType.None)
            {
                newCornerLength = MathF.Min(cornerLength, minLength * 0.5f);
            }
            else if (capType is LineCapType.Extended)
            {
                newCornerLength = MathF.Min(cornerLength + newLineThickness, minLength * 0.5f);
            
            }
            else if (capType is LineCapType.Capped)
            {
                newCornerLength = MathF.Min(cornerLength - newLineThickness, minLength * 0.5f - newLineThickness);
            }
            else //Capped Extended
            {
                newCornerLength = MathF.Min(cornerLength, minLength * 0.5f - newLineThickness);
            }
        
            if(newCornerLength <= 0) continue;
            
            DrawCornerAbsoluteTransparent(prev, cur, next, newCornerLength, newLineThickness, color, capType, capPoints, miterLimit, beveled);
        }
    }
    public static void DrawCorneredAbsoluteTransparent(this Polygon poly, List<float> cornerLength, LineDrawingInfo lineInfo, float miterLimit = 2f, bool beveled = false)
    {
        poly.DrawCorneredAbsoluteTransparent(lineInfo.Thickness, lineInfo.Color, cornerLength, lineInfo.CapType, lineInfo.CapPoints, miterLimit, beveled);
    }
    
    
    public static bool CaluclateDrawCornerAbsoluteParameters(this Polygon poly, float cornerLength, float lineThickness, LineCapType capType, 
        out float newCornerLength, out float newLineThickness)
    {
        newCornerLength = -1f;
        newLineThickness = -1f;
        
        for (var i = 0; i < poly.Count; i++)
        {
            var cur = poly[i];
            var next = poly[ShapeMath.WrapIndex(poly.Count, i + 1)];
    
            var wNext = next - cur;
    
            float lsNext = wNext.LengthSquared();
            if (lsNext <= 0)
            {
                newCornerLength = 0f;
                newLineThickness = 0f;
                return false;
            }
        
            float minLength = MathF.Sqrt(lsNext);
            float curLineThickness = MathF.Min(lineThickness, MathF.Min(cornerLength * 0.5f, minLength * 0.25f));
            float curCornerLength;
            if (capType is LineCapType.None)
            {
                curCornerLength = MathF.Min(cornerLength, minLength * 0.5f);
            }
            else if (capType is LineCapType.Extended)
            {
                curCornerLength = MathF.Min(cornerLength + curLineThickness, minLength * 0.5f);
            
            }
            else if (capType is LineCapType.Capped)
            {
                curCornerLength = MathF.Min(cornerLength - curLineThickness, minLength * 0.5f - curLineThickness);
            }
            else //Capped Extended
            {
                curCornerLength = MathF.Min(cornerLength, minLength * 0.5f - curLineThickness);
            }
            
            if(curCornerLength <= 0 || curLineThickness <= 0)
            {
                newCornerLength = 0f;
                newLineThickness = 0f;
                return false;
            }
            
            if(curCornerLength < newCornerLength || newCornerLength < 0f) newCornerLength = curCornerLength;
            if(curLineThickness < newLineThickness || newLineThickness < 0f) newLineThickness = curLineThickness;
        }
        
        if (newCornerLength > 0 && newLineThickness > 0) return true;
        
        newCornerLength = 0;
        newLineThickness = 0;
        return false;
    }
    #endregion
    
    //TODO: Add xml summaries
    #region Draw Cornered Relative Transparent

    public static void DrawCorneredRelativeTransparent(this Polygon poly, float lineThickness, ColorRgba color, float cornerLengthFactor, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2,
        float miterLimit = 2f, bool beveled = false)
    {
        
        cornerLengthFactor = ShapeMath.Clamp(cornerLengthFactor, 0f, 1f);
        
        for (var i = 0; i < poly.Count; i++)
        {
            var prev = poly[ShapeMath.WrapIndex(poly.Count, i - 1)];
            var cur = poly[i];
            var next = poly[ShapeMath.WrapIndex(poly.Count, i + 1)];
            
            var wPrev = cur - prev;
            var wNext = next - cur;
            float lsPrev = wPrev.LengthSquared();
            float lsNext = wNext.LengthSquared();
            if(lsPrev <= 0 || lsNext <= 0) return;
        
            float minLength = MathF.Sqrt(MathF.Min(lsPrev, lsNext));
            float cornerLength = minLength * cornerLengthFactor;
            float newLineThickness = MathF.Min(lineThickness, MathF.Min(cornerLength * 0.5f, minLength * 0.25f));
            float newCornerLength;
            
            if (capType is LineCapType.None)
            {
                newCornerLength = MathF.Min(cornerLength, minLength * 0.5f);
            }
            else if (capType is LineCapType.Extended)
            {
                newCornerLength = MathF.Min(cornerLength + newLineThickness, minLength * 0.5f);
            
            }
            else if (capType is LineCapType.Capped)
            {
                newCornerLength = MathF.Min(cornerLength - newLineThickness, minLength * 0.5f - newLineThickness);
            }
            else //Capped Extended
            {
                newCornerLength = MathF.Min(cornerLength, minLength * 0.5f - newLineThickness);
            }
        
            if(newCornerLength <= 0) continue;
            
            DrawCornerAbsoluteTransparent(prev, cur, next, newCornerLength, newLineThickness, color, capType, capPoints, miterLimit, beveled);
        }
    }
    public static void DrawCorneredRelativeTransparent(this Polygon poly, float cornerLengthFactor, LineDrawingInfo lineInfo, float miterLimit = 2f, bool beveled = false)
    {
        poly.DrawCorneredRelativeTransparent(lineInfo.Thickness, lineInfo.Color, cornerLengthFactor, lineInfo.CapType, lineInfo.CapPoints, miterLimit, beveled);
    }
    
    public static void DrawCorneredRelativeTransparent(this Polygon poly, float lineThickness, ColorRgba color, List<float> cornerLengthFactors, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2,
        float miterLimit = 2f, bool beveled = false)
    {
        
        for (var i = 0; i < poly.Count; i++)
        {
            float cornerLengthFactor = cornerLengthFactors[i % cornerLengthFactors.Count];
            cornerLengthFactor = ShapeMath.Clamp(cornerLengthFactor, 0f, 1f);
            var prev = poly[ShapeMath.WrapIndex(poly.Count, i - 1)];
            var cur = poly[i];
            var next = poly[ShapeMath.WrapIndex(poly.Count, i + 1)];
            
            var wPrev = cur - prev;
            var wNext = next - cur;
            float lsPrev = wPrev.LengthSquared();
            float lsNext = wNext.LengthSquared();
            if(lsPrev <= 0 || lsNext <= 0) return;
        
            float minLength = MathF.Sqrt(MathF.Min(lsPrev, lsNext));
            float cornerLength = minLength * cornerLengthFactor;
            float newLineThickness = MathF.Min(lineThickness, MathF.Min(cornerLength * 0.5f, minLength * 0.25f));
            float newCornerLength;
            
            if (capType is LineCapType.None)
            {
                newCornerLength = MathF.Min(cornerLength, minLength * 0.5f);
            }
            else if (capType is LineCapType.Extended)
            {
                newCornerLength = MathF.Min(cornerLength + newLineThickness, minLength * 0.5f);
            
            }
            else if (capType is LineCapType.Capped)
            {
                newCornerLength = MathF.Min(cornerLength - newLineThickness, minLength * 0.5f - newLineThickness);
            }
            else //Capped Extended
            {
                newCornerLength = MathF.Min(cornerLength, minLength * 0.5f - newLineThickness);
            }
        
            if(newCornerLength <= 0) continue;
            
            DrawCornerAbsoluteTransparent(prev, cur, next, newCornerLength, newLineThickness, color, capType, capPoints, miterLimit, beveled);
        }
    }
    public static void DrawCorneredRelativeTransparent(this Polygon poly, List<float> cornerLengthFactors, LineDrawingInfo lineInfo, float miterLimit = 2f, bool beveled = false)
    {
        poly.DrawCorneredRelativeTransparent(lineInfo.Thickness, lineInfo.Color, cornerLengthFactors, lineInfo.CapType, lineInfo.CapPoints, miterLimit, beveled);
    }
    #endregion
    
    #region Draw Cornered
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
            var prev = poly[ShapeMath.WrapIndex(poly.Count, i - 1)];// poly[(i-1)%poly.Count];
            var cur = poly[i];
            var next = poly[ShapeMath.WrapIndex(poly.Count, i + 1)];//poly[(i+1)%poly.Count];
            SegmentDrawing.DrawSegment(cur, cur + (next - cur).Normalize() * cornerLength, lineThickness, color, capType, capPoints);
            SegmentDrawing.DrawSegment(cur, cur + (prev - cur).Normalize() * cornerLength, lineThickness, color, capType, capPoints);
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
            var prev = poly[ShapeMath.WrapIndex(poly.Count, i - 1)];// poly[(i-1)%poly.Count];
            var cur = poly[i];
            var next = poly[ShapeMath.WrapIndex(poly.Count, i + 1)];//poly[(i+1)%poly.Count];
            SegmentDrawing.DrawSegment(cur, cur + (next - cur).Normalize() * cornerLength, lineInfo);
            SegmentDrawing.DrawSegment(cur, cur + (prev - cur).Normalize() * cornerLength, lineInfo);
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
            SegmentDrawing.DrawSegment(cur, cur + (next - cur).Normalize() * cornerLength, lineThickness, color, capType, capPoints);
            SegmentDrawing.DrawSegment(cur, cur + (prev - cur).Normalize() * cornerLength, lineThickness, color, capType, capPoints);
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
            SegmentDrawing.DrawSegment(cur, cur + (next - cur).Normalize() * cornerLength, lineInfo);
            SegmentDrawing.DrawSegment(cur, cur + (prev - cur).Normalize() * cornerLength, lineInfo);
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




    

