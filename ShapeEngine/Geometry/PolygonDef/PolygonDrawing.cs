using System.Numerics;
using System.Runtime.Intrinsics.X86;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.Geometry.TriangulationDef;
using ShapeEngine.ShapeClipper;
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

    // private static Triangulation drawHelperTriangulation = [];

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
            for (var i = 0; i < poly.Count - 1; i++) Raylib.DrawTriangle(poly[i], center, poly[i + 1], color.ToRayColor());
            Raylib.DrawTriangle(poly[^1], center, poly[0], color.ToRayColor());
        }
        else
        {
            for (var i = 0; i < poly.Count - 1; i++) Raylib.DrawTriangle(poly[i], poly[i + 1], center, color.ToRayColor());
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
        
        ClipperImmediate2D.DrawPolygon(poly, color, false);

        // drawHelperTriangulation.Clear();
        // poly.Triangulate(drawHelperTriangulation);
        // drawHelperTriangulation.Draw(color);
    }

    #endregion
    
    #region Draw Lines Perimeter & Percentage
    
    /// <summary>
    /// Draws a partial section of the polygon outline measured by perimeter distance.
    /// </summary>
    /// <param name="poly">The polygon whose outline section will be drawn.</param>
    /// <param name="perimeterToDraw">The perimeter distance to trace and render, starting at <paramref name="startIndex"/>.</param>
    /// <param name="startIndex">The index of the polygon vertex where tracing begins.</param>
    /// <param name="lineThickness">The outline thickness in world units.</param>
    /// <param name="color">The color used to draw the generated outline section.</param>
    /// <param name="capType">The cap style used at the open ends of the partial outline.</param>
    /// <param name="miterLimit">The maximum miter length factor used for sharp joins before falling back to a bevel or clipped join.</param>
    /// <param name="beveled">If true, forces beveled joins for corners that exceed the miter limit.</param>
    /// <remarks>
    /// This method renders an open outline segment rather than the full closed polygon outline.
    /// </remarks>
    public static void DrawLinesPerimeter(this Polygon poly, float perimeterToDraw, int startIndex, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, float miterLimit = 2f, bool beveled = false)
    {
        ClipperImmediate2D.DrawPolygonOutlinePerimeter(poly, perimeterToDraw, startIndex, lineThickness, color, miterLimit, beveled, capType.ToShapeClipperEndType(), false);
    }
    
    /// <summary>
    /// Draws a partial section of the polygon outline measured by perimeter distance using the specified <see cref="LineDrawingInfo"/>.
    /// </summary>
    /// <param name="poly">The polygon whose outline section will be drawn.</param>
    /// <param name="perimeterToDraw">The perimeter distance to trace and render, starting at <paramref name="startIndex"/>.</param>
    /// <param name="startIndex">The index of the polygon vertex where tracing begins.</param>
    /// <param name="lineInfo">The line drawing settings that define thickness, color, and cap style.</param>
    /// <param name="miterLimit">The maximum miter length factor used for sharp joins before falling back to a bevel or clipped join.</param>
    /// <param name="beveled">If true, forces beveled joins for corners that exceed the miter limit.</param>
    /// <remarks>
    /// This overload reads thickness, color, and cap configuration from <paramref name="lineInfo"/>.
    /// </remarks>
    public static void DrawLinesPerimeter(this Polygon poly, float perimeterToDraw, int startIndex,  LineDrawingInfo lineInfo, float miterLimit = 2f, bool beveled = false)
    {
        ClipperImmediate2D.DrawPolygonOutlinePerimeter(poly, perimeterToDraw, startIndex, lineInfo.Thickness, lineInfo.Color, miterLimit, beveled, lineInfo.CapType.ToShapeClipperEndType(), false);
    }
    
    /// <summary>
    /// Draws a partial section of the polygon outline based on a fraction of the polygon perimeter.
    /// </summary>
    /// <param name="poly">The polygon whose outline section will be drawn.</param>
    /// <param name="f">The fraction of the total perimeter to render, typically in the range from 0 to 1.</param>
    /// <param name="startIndex">The index of the polygon vertex where tracing begins.</param>
    /// <param name="lineThickness">The outline thickness in world units.</param>
    /// <param name="color">The color used to draw the generated outline section.</param>
    /// <param name="capType">The cap style used at the open ends of the partial outline.</param>
    /// <param name="miterLimit">The maximum miter length factor used for sharp joins before falling back to a bevel or clipped join.</param>
    /// <param name="beveled">If true, forces beveled joins for corners that exceed the miter limit.</param>
    /// <remarks>
    /// The traced section starts at <paramref name="startIndex"/> and proceeds along the polygon winding order.
    /// </remarks>
    public static void DrawLinesPercentage(this Polygon poly, float f, int startIndex, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, float miterLimit = 2f, bool beveled = false)
    {
        ClipperImmediate2D.DrawPolygonOutlinePercentage(poly, f, startIndex, lineThickness, color, miterLimit, beveled, capType.ToShapeClipperEndType(), false);
    }
    
    /// <summary>
    /// Draws a partial section of the polygon outline based on a fraction of the polygon perimeter using the specified <see cref="LineDrawingInfo"/>.
    /// </summary>
    /// <param name="poly">The polygon whose outline section will be drawn.</param>
    /// <param name="f">The fraction of the total perimeter to render, typically in the range from 0 to 1.</param>
    /// <param name="startIndex">The index of the polygon vertex where tracing begins.</param>
    /// <param name="lineInfo">The line drawing settings that define thickness, color, and cap style.</param>
    /// <param name="miterLimit">The maximum miter length factor used for sharp joins before falling back to a bevel or clipped join.</param>
    /// <param name="beveled">If true, forces beveled joins for corners that exceed the miter limit.</param>
    /// <remarks>
    /// This overload reads thickness, color, and cap configuration from <paramref name="lineInfo"/>.
    /// </remarks>
    public static void DrawLinesPercentage(this Polygon poly, float f, int startIndex, LineDrawingInfo lineInfo, float miterLimit = 2f, bool beveled = false)
    {
        ClipperImmediate2D.DrawPolygonOutlinePercentage(poly, f, startIndex, lineInfo.Thickness, lineInfo.Color, miterLimit, beveled, lineInfo.CapType.ToShapeClipperEndType(), false);
    }
    #endregion
    
    #region Draw Lines

    /// <summary>
    /// Draws the outline of a polygon with transparent color support.
    /// This method handles miter and beveled joins and allows for custom miter limits.
    /// </summary>
    /// <param name="poly">The polygon whose outline will be drawn.</param>
    /// <param name="lineThickness">The thickness of the outline in world units.</param>
    /// <param name="color">The color of the outline, including alpha for transparency.</param>
    /// <param name="miterLimit">
    /// The miter limit for joins. If the miter length exceeds this value times the line thickness, a bevel is used instead.
    /// Default is 2.0f.
    /// </param>
    /// <param name="beveled">
    /// If true, forces beveled joins instead of miters when the miter limit is exceeded.
    /// </param>
    public static void DrawLines(this Polygon poly, float lineThickness, ColorRgba color, float miterLimit = 2f, bool beveled = false)
    {
        ClipperImmediate2D.DrawPolygonOutline(poly, lineThickness, color, miterLimit, beveled, false);
    }

    /// <summary>
    /// Draws the outline of a polygon with transparent color support using the specified <see cref="LineDrawingInfo"/>.
    /// Handles miter and beveled joins, and allows for custom miter limits.
    /// </summary>
    /// <param name="poly">The polygon whose outline will be drawn.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <param name="miterLimit">
    /// The miter limit for joins. If the miter length exceeds this value times the line thickness, a bevel is used instead.
    /// Default is 2.0f.
    /// </param>
    /// <param name="beveled">
    /// If true, forces beveled joins instead of miters when the miter limit is exceeded.
    /// </param>
    public static void DrawLines(this Polygon poly, LineDrawingInfo lineInfo, float miterLimit = 2f, bool beveled = false)
    {
        ClipperImmediate2D.DrawPolygonOutline(poly, lineInfo.Thickness, lineInfo.Color, miterLimit, beveled, false);
    }

    #endregion

    #region Draw Lines Glow

    #region Glow
    /// <summary>
    /// Draws the polygon outline as a layered glow by rendering multiple outline passes
    /// with interpolated thickness and color values.
    /// </summary>
    /// <param name="polygon">The polygon whose outline will be drawn.</param>
    /// <param name="thicknessRange">The outline thickness range used from the first pass to the final pass.</param>
    /// <param name="colorRange">The color range used from the first pass to the final pass.</param>
    /// <param name="steps">The number of glow passes to render. A value of 1 draws a single pass using the maximum thickness and color.</param>
    /// <param name="miterLimit">The maximum miter length factor used for sharp joins before beveling is applied.</param>
    /// <param name="beveled">If true, sharp joins are beveled instead of using extended miters.</param>
    /// <param name="useDelaunay">If true, applies Delaunay refinement when triangulating the outline mesh.</param>
    /// <remarks>
    /// Useful for stylized outlines such as halos, selection rings, and expanding border effects.
    /// </remarks>
    public static void DrawGlow(this Polygon polygon, ValueRange thicknessRange, ValueRangeColor colorRange, int steps, 
        float miterLimit = 2f, bool beveled = false, bool useDelaunay = false)
    {
        ClipperImmediate2D.DrawPolygonOutlineGlow(polygon, thicknessRange, colorRange, steps, miterLimit, beveled, useDelaunay);
    }
    
    #endregion

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

    #region Draw Cornered Absolute Transparent

    /// <summary>
    /// Draws a polygon corner based on an absolute <paramref name="cornerLength"/>, handling miter/bevel joins, custom cap types and transparent colors are supported.
    /// </summary>
    /// <param name="prev">The previous vertex of the polygon.</param>
    /// <param name="corner">The current corner vertex.</param>
    /// <param name="next">The next vertex of the polygon.</param>
    /// <param name="cornerLength">The length of the corner segment to render.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color used for rendering, including alpha for transparency.</param>
    /// <param name="capType">The type of line cap to use.</param>
    /// <param name="capPoints">The number of points for the cap.</param>
    /// <param name="miterLimit">The miter limit for joins. If exceeded, the corner is either squared or beveled depeding on <paramref name="beveled"/>.</param>
    /// <param name="beveled">If true, forces beveled joins instead of squared joins when the miter limit is exceeded.
    /// Beveling is faster and simpler but does not look as good.</param>
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

        if (cornerType.type == 0) //collinear
        {
            SegmentDrawing.DrawSegment(prev, next, lineThickness, color, capType, capPoints);
            return;
        }

        var rayColor = color.ToRayColor();
        float totalMiterLengthLimit = lineThickness * 0.5f * MathF.Max(2f, miterLimit);
        var miterDir = (normalPrev + normalNext).Normalize();
        float miterAngleRad = MathF.Abs(miterDir.AngleRad(normalNext));
        float miterLength = lineThickness / MathF.Cos(miterAngleRad);

        if (miterLimit < 2f || miterLength < totalMiterLengthLimit)
        {
            var cornerOuter = corner + miterDir * miterLength;
            var prevOuter = prev + normalPrev * lineThickness;
            var prevInner = prev - normalPrev * lineThickness;
            var nextOuter = next + normalNext * lineThickness;
            var nextInner = next - normalNext * lineThickness;
            var intersection = Ray.IntersectRayRay(prevInner, dirPrev, nextInner, -dirNext);
            var cornerInner = intersection.Valid ? intersection.Point : corner - miterDir * miterLength;

            if (cornerType.type >= 0)
            {
                Raylib.DrawTriangle(cornerInner, prevInner, prevOuter, rayColor);
                Raylib.DrawTriangle(cornerInner, prevOuter, cornerOuter, rayColor);
                Raylib.DrawTriangle(cornerInner, cornerOuter, nextOuter, rayColor);
                Raylib.DrawTriangle(cornerInner, nextOuter, nextInner, rayColor);
            }
            else
            {
                Raylib.DrawTriangle(prevInner, cornerInner, prevOuter, rayColor);
                Raylib.DrawTriangle(prevOuter, cornerInner, cornerOuter, rayColor);
                Raylib.DrawTriangle(cornerOuter, cornerInner, nextOuter, rayColor);
                Raylib.DrawTriangle(nextOuter, cornerInner, nextInner, rayColor);
            }
        }
        else
        {
            miterLength = totalMiterLengthLimit;
            Vector2 cornerOuterPrev, cornerOuterNext;

            var prevOuter = prev + normalPrev * lineThickness;
            var prevInner = prev - normalPrev * lineThickness;
            var nextOuter = next + normalNext * lineThickness;
            var nextInner = next - normalNext * lineThickness;
            var intersection = Ray.IntersectRayRay(prevInner, dirPrev, nextInner, -dirNext);
            var cornerInner = intersection.Valid ? intersection.Point : corner - miterDir * miterLength;

            if (beveled)
            {
                cornerOuterPrev = corner + normalPrev * lineThickness;
                cornerOuterNext = corner + normalNext * lineThickness;
            }
            else
            {
                var cornerOuter = corner + miterDir * miterLength;
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
                Raylib.DrawTriangle(prevInner, cornerInner, prevOuter, rayColor);
                Raylib.DrawTriangle(prevOuter, cornerInner, cornerOuterPrev, rayColor);
                Raylib.DrawTriangle(cornerOuterNext, cornerInner, nextOuter, rayColor);
                Raylib.DrawTriangle(nextOuter, cornerInner, nextInner, rayColor);
                Raylib.DrawTriangle(cornerOuterPrev, cornerInner, cornerOuterNext, rayColor);
            }
        }

        if (capType is LineCapType.Capped or LineCapType.CappedExtended && capPoints > 0)
        {
            SegmentDrawing.DrawRoundCap(prev, -dirPrev, lineThickness, capPoints, color);
            SegmentDrawing.DrawRoundCap(next, dirNext, lineThickness, capPoints, color);
        }
    }

    /// <summary>
    /// Draws each corner of the polygon with an absolute <paramref name="cornerLength"/>, handling miter/bevel joins, custom cap types and transparent colors are supported.
    /// </summary>
    /// <param name="poly">The polygon whose corners will be drawn.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color used for rendering, including alpha for transparency.</param>
    /// <param name="cornerLength">The length of the corner segment to render.</param>
    /// <param name="capType">The type of line cap to use.</param>
    /// <param name="capPoints">The number of points for the cap.</param>
    /// <param name="miterLimit">The miter limit for joins. If exceeded, the corner is either squared or beveled depeding on <paramref name="beveled"/>.</param>
    /// <param name="beveled">If true, forces beveled joins instead of squared joins when the miter limit is exceeded.
    /// Beveling is faster and simpler but does not look as good.</param>
    public static void DrawCorneredAbsoluteTransparent(this Polygon poly, float lineThickness, ColorRgba color, float cornerLength,
        LineCapType capType = LineCapType.CappedExtended, int capPoints = 2,
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
            if (lsPrev <= 0 || lsNext <= 0) return;

            float minLength = MathF.Sqrt(MathF.Min(lsPrev, lsNext));
            float newLineThickness = MathF.Min(lineThickness, MathF.Min(cornerLength * 0.5f, minLength * 0.25f));
            float newCornerLength;

            if (capType is LineCapType.None)
                newCornerLength = MathF.Min(cornerLength, minLength * 0.5f);
            else if (capType is LineCapType.Extended)
                newCornerLength = MathF.Min(cornerLength + newLineThickness, minLength * 0.5f);
            else if (capType is LineCapType.Capped)
                newCornerLength = MathF.Min(cornerLength - newLineThickness, minLength * 0.5f - newLineThickness);
            else //Capped Extended
                newCornerLength = MathF.Min(cornerLength, minLength * 0.5f - newLineThickness);

            if (newCornerLength <= 0) continue;

            DrawCornerAbsoluteTransparent(prev, cur, next, newCornerLength, newLineThickness, color, capType, capPoints, miterLimit, beveled);
        }
    }

    /// <summary>
    /// Draws each corner of the polygon with an absolute <paramref name="cornerLength"/>, handling miter/bevel joins, custom cap types, and transparent colors.
    /// Uses <see cref="LineDrawingInfo"/> for line thickness, color, cap type, and cap points.
    /// </summary>
    /// <param name="poly">The polygon whose corners will be drawn.</param>
    /// <param name="cornerLength">The length of the corner segment to render.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <param name="miterLimit">The miter limit for joins. If exceeded, the corner is either squared or beveled depeding on <paramref name="beveled"/>.</param>
    /// <param name="beveled">If true, forces beveled joins instead of squared joins when the miter limit is exceeded.
    /// Beveling is faster and simpler but does not look as good.</param>
    public static void DrawCorneredAbsoluteTransparent(this Polygon poly, float cornerLength, LineDrawingInfo lineInfo, float miterLimit = 2f,
        bool beveled = false)
    {
        poly.DrawCorneredAbsoluteTransparent(lineInfo.Thickness, lineInfo.Color, cornerLength, lineInfo.CapType, lineInfo.CapPoints, miterLimit, beveled);
    }

    /// <summary>
    /// Draws each corner of the polygon with an absolute length specified per corner, handling miter/bevel joins, custom cap types, and transparent colors.
    /// </summary>
    /// <param name="poly">The polygon whose corners will be drawn.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color used for rendering, including alpha for transparency.</param>
    /// <param name="cornerLengths">A list of corner segment lengths to render for each corner.</param>
    /// <param name="capType">The type of line cap to use.</param>
    /// <param name="capPoints">The number of points for the cap.</param>
    /// <param name="miterLimit">The miter limit for joins. If exceeded, the corner is either squared or beveled depeding on <paramref name="beveled"/>.</param>
    /// <param name="beveled">If true, forces beveled joins instead of squared joins when the miter limit is exceeded.
    /// Beveling is faster and simpler but does not look as good.</param>
    public static void DrawCorneredAbsoluteTransparent(this Polygon poly, float lineThickness, ColorRgba color, List<float> cornerLengths,
        LineCapType capType = LineCapType.CappedExtended, int capPoints = 2,
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
            if (lsPrev <= 0 || lsNext <= 0) return;

            float minLength = MathF.Sqrt(MathF.Min(lsPrev, lsNext));
            float newLineThickness = MathF.Min(lineThickness, MathF.Min(cornerLength * 0.5f, minLength * 0.25f));
            float newCornerLength;

            if (capType is LineCapType.None)
                newCornerLength = MathF.Min(cornerLength, minLength * 0.5f);
            else if (capType is LineCapType.Extended)
                newCornerLength = MathF.Min(cornerLength + newLineThickness, minLength * 0.5f);
            else if (capType is LineCapType.Capped)
                newCornerLength = MathF.Min(cornerLength - newLineThickness, minLength * 0.5f - newLineThickness);
            else //Capped Extended
                newCornerLength = MathF.Min(cornerLength, minLength * 0.5f - newLineThickness);

            if (newCornerLength <= 0) continue;

            DrawCornerAbsoluteTransparent(prev, cur, next, newCornerLength, newLineThickness, color, capType, capPoints, miterLimit, beveled);
        }
    }

    /// <summary>
    /// Draws each corner of the polygon with an absolute length specified per corner, handling miter/bevel joins, custom cap types, and transparent colors.
    /// Uses <see cref="LineDrawingInfo"/> for line thickness, color, cap type, and cap points.
    /// </summary>
    /// <param name="poly">The polygon whose corners will be drawn.</param>
    /// <param name="cornerLength">A list of corner segment lengths to render for each corner.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <param name="miterLimit">The miter limit for joins. If exceeded, the corner is either squared or beveled depeding on <paramref name="beveled"/>.</param>
    /// <param name="beveled">If true, forces beveled joins instead of squared joins when the miter limit is exceeded.
    /// Beveling is faster and simpler but does not look as good.</param>
    public static void DrawCorneredAbsoluteTransparent(this Polygon poly, List<float> cornerLength, LineDrawingInfo lineInfo, float miterLimit = 2f,
        bool beveled = false)
    {
        poly.DrawCorneredAbsoluteTransparent(lineInfo.Thickness, lineInfo.Color, cornerLength, lineInfo.CapType, lineInfo.CapPoints, miterLimit, beveled);
    }

    /// <summary>
    /// Calculates parameters for drawing a polygon corner with an absolute <paramref name="cornerLength"/> and <paramref name="lineThickness"/>.
    /// Returns true if parameters are valid for drawing, otherwise false.
    /// </summary>
    /// <param name="poly">The polygon whose corner parameters will be calculated.</param>
    /// <param name="cornerLength">The length of the corner segment to render.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="capType">The type of line cap to use.</param>
    /// <param name="newCornerLength">The calculated corner length to use for drawing.</param>
    /// <param name="newLineThickness">The calculated line thickness to use for drawing.</param>
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
                curCornerLength = MathF.Min(cornerLength, minLength * 0.5f);
            else if (capType is LineCapType.Extended)
                curCornerLength = MathF.Min(cornerLength + curLineThickness, minLength * 0.5f);
            else if (capType is LineCapType.Capped)
                curCornerLength = MathF.Min(cornerLength - curLineThickness, minLength * 0.5f - curLineThickness);
            else //Capped Extended
                curCornerLength = MathF.Min(cornerLength, minLength * 0.5f - curLineThickness);

            if (curCornerLength <= 0 || curLineThickness <= 0)
            {
                newCornerLength = 0f;
                newLineThickness = 0f;
                return false;
            }

            if (curCornerLength < newCornerLength || newCornerLength < 0f) newCornerLength = curCornerLength;
            if (curLineThickness < newLineThickness || newLineThickness < 0f) newLineThickness = curLineThickness;
        }

        if (newCornerLength > 0 && newLineThickness > 0) return true;

        newCornerLength = 0;
        newLineThickness = 0;
        return false;
    }

    #endregion

    #region Draw Cornered Relative Transparent

    /// <summary>
    /// Draws each corner of the polygon with a relative length based on <paramref name="cornerLengthFactor"/>, handling miter/bevel joins, custom cap types, and transparent colors.
    /// </summary>
    /// <param name="poly">The polygon whose corners will be drawn.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color used for rendering, including alpha for transparency.</param>
    /// <param name="cornerLengthFactor">
    /// The factor (0-1) that determines the relative length of each corner segment to render.
    /// 0: No corner is drawn.
    /// 1: Maximum relative length.
    /// </param>
    /// <param name="capType">The type of line cap to use.</param>
    /// <param name="capPoints">The number of points for the cap.</param>
    /// <param name="miterLimit">The miter limit for joins. If exceeded, the corner is either squared or beveled depeding on <paramref name="beveled"/>.</param>
    /// <param name="beveled">If true, forces beveled joins instead of squared joins when the miter limit is exceeded.
    /// Beveling is faster and simpler but does not look as good.</param>
    public static void DrawCorneredRelativeTransparent(this Polygon poly, float lineThickness, ColorRgba color, float cornerLengthFactor,
        LineCapType capType = LineCapType.CappedExtended, int capPoints = 2,
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
            if (lsPrev <= 0 || lsNext <= 0) return;

            float minLength = MathF.Sqrt(MathF.Min(lsPrev, lsNext));
            float cornerLength = minLength * cornerLengthFactor;
            float newLineThickness = MathF.Min(lineThickness, MathF.Min(cornerLength * 0.5f, minLength * 0.25f));
            float newCornerLength;

            if (capType is LineCapType.None)
                newCornerLength = MathF.Min(cornerLength, minLength * 0.5f);
            else if (capType is LineCapType.Extended)
                newCornerLength = MathF.Min(cornerLength + newLineThickness, minLength * 0.5f);
            else if (capType is LineCapType.Capped)
                newCornerLength = MathF.Min(cornerLength - newLineThickness, minLength * 0.5f - newLineThickness);
            else //Capped Extended
                newCornerLength = MathF.Min(cornerLength, minLength * 0.5f - newLineThickness);

            if (newCornerLength <= 0) continue;

            DrawCornerAbsoluteTransparent(prev, cur, next, newCornerLength, newLineThickness, color, capType, capPoints, miterLimit, beveled);
        }
    }

    /// <summary>
    /// Draws each corner of the polygon with a relative length based on <paramref name="cornerLengthFactor"/>, handling miter/bevel joins, custom cap types, and transparent colors.
    /// Uses <see cref="LineDrawingInfo"/> for line thickness, color, cap type, and cap points.
    /// </summary>
    /// <param name="poly">The polygon whose corners will be drawn.</param>
    /// <param name="cornerLengthFactor">
    /// The factor (0-1) that determines the relative length of each corner segment to render.
    /// 0: No corner is drawn.
    /// 1: Maximum relative length.
    /// </param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <param name="miterLimit">The miter limit for joins. If exceeded, the corner is either squared or beveled depending on <paramref name="beveled"/>.</param>
    /// <param name="beveled">If true, forces beveled joins instead of squared joins when the miter limit is exceeded. Beveling is faster and simpler but does not look as good.</param>
    public static void DrawCorneredRelativeTransparent(this Polygon poly, float cornerLengthFactor, LineDrawingInfo lineInfo, float miterLimit = 2f,
        bool beveled = false)
    {
        poly.DrawCorneredRelativeTransparent(lineInfo.Thickness, lineInfo.Color, cornerLengthFactor, lineInfo.CapType, lineInfo.CapPoints, miterLimit, beveled);
    }

    /// <summary>
    /// Draws each corner of the polygon with a relative length specified per corner, handling miter/bevel joins, custom cap types, and transparent colors.
    /// </summary>
    /// <param name="poly">The polygon whose corners will be drawn.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color used for rendering, including alpha for transparency.</param>
    /// <param name="cornerLengthFactors">A list of relative corner length factors (0-1) for each corner.</param>
    /// <param name="capType">The type of line cap to use.</param>
    /// <param name="capPoints">The number of points for the cap.</param>
    /// <param name="miterLimit">The miter limit for joins. If exceeded, the corner is either squared or beveled depending on <paramref name="beveled"/>.</param>
    /// <param name="beveled">If true, forces beveled joins instead of squared joins when the miter limit is exceeded. Beveling is faster and simpler but does not look as good.</param>
    public static void DrawCorneredRelativeTransparent(this Polygon poly, float lineThickness, ColorRgba color, List<float> cornerLengthFactors,
        LineCapType capType = LineCapType.CappedExtended, int capPoints = 2,
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
            if (lsPrev <= 0 || lsNext <= 0) return;

            float minLength = MathF.Sqrt(MathF.Min(lsPrev, lsNext));
            float cornerLength = minLength * cornerLengthFactor;
            float newLineThickness = MathF.Min(lineThickness, MathF.Min(cornerLength * 0.5f, minLength * 0.25f));
            float newCornerLength;

            if (capType is LineCapType.None)
                newCornerLength = MathF.Min(cornerLength, minLength * 0.5f);
            else if (capType is LineCapType.Extended)
                newCornerLength = MathF.Min(cornerLength + newLineThickness, minLength * 0.5f);
            else if (capType is LineCapType.Capped)
                newCornerLength = MathF.Min(cornerLength - newLineThickness, minLength * 0.5f - newLineThickness);
            else //Capped Extended
                newCornerLength = MathF.Min(cornerLength, minLength * 0.5f - newLineThickness);

            if (newCornerLength <= 0) continue;

            DrawCornerAbsoluteTransparent(prev, cur, next, newCornerLength, newLineThickness, color, capType, capPoints, miterLimit, beveled);
        }
    }

    /// <summary>
    /// Draws each corner of the polygon with a relative length specified per corner, handling miter/bevel joins, custom cap types, and transparent colors.
    /// Uses <see cref="LineDrawingInfo"/> for line thickness, color, cap type, and cap points.
    /// </summary>
    /// <param name="poly">The polygon whose corners will be drawn.</param>
    /// <param name="cornerLengthFactors">A list of relative corner length factors (0-1) for each corner.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <param name="miterLimit">The miter limit for joins. If exceeded, the corner is either squared or beveled depending on <paramref name="beveled"/>.</param>
    /// <param name="beveled">If true, forces beveled joins instead of squared joins when the miter limit is exceeded. Beveling is faster and simpler but does not look as good.</param>
    public static void DrawCorneredRelativeTransparent(this Polygon poly, List<float> cornerLengthFactors, LineDrawingInfo lineInfo, float miterLimit = 2f,
        bool beveled = false)
    {
        poly.DrawCorneredRelativeTransparent(lineInfo.Thickness, lineInfo.Color, cornerLengthFactors, lineInfo.CapType, lineInfo.CapPoints, miterLimit,
            beveled);
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
    public static void DrawCornered(this Polygon poly, float lineThickness, ColorRgba color, float cornerLength,
        LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        color = color.SetAlpha(255);
        for (var i = 0; i < poly.Count; i++)
        {
            var prev = poly[ShapeMath.WrapIndex(poly.Count, i - 1)];
            var cur = poly[i];
            var next = poly[ShapeMath.WrapIndex(poly.Count, i + 1)];
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
        lineInfo = lineInfo.SetColor(lineInfo.Color.SetAlpha(255));
        for (var i = 0; i < poly.Count; i++)
        {
            var prev = poly[ShapeMath.WrapIndex(poly.Count, i - 1)];
            var cur = poly[i];
            var next = poly[ShapeMath.WrapIndex(poly.Count, i + 1)];
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
    public static void DrawCornered(this Polygon poly, float lineThickness, ColorRgba color, List<float> cornerLengths,
        LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        color = color.SetAlpha(255);
        for (var i = 0; i < poly.Count; i++)
        {
            float cornerLength = cornerLengths[i % cornerLengths.Count];
            var prev = poly[ShapeMath.WrapIndex(poly.Count, i - 1)];
            var cur = poly[i];
            var next = poly[ShapeMath.WrapIndex(poly.Count, i + 1)];
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
        lineInfo = lineInfo.SetColor(lineInfo.Color.SetAlpha(255));
        for (var i = 0; i < poly.Count; i++)
        {
            float cornerLength = cornerLengths[i % cornerLengths.Count];
            var prev = poly[ShapeMath.WrapIndex(poly.Count, i - 1)];
            var cur = poly[i];
            var next = poly[ShapeMath.WrapIndex(poly.Count, i + 1)];
            SegmentDrawing.DrawSegment(cur, cur + (next - cur).Normalize() * cornerLength, lineInfo);
            SegmentDrawing.DrawSegment(cur, cur + (prev - cur).Normalize() * cornerLength, lineInfo);
        }
    }

    #endregion
    
    #region Gapped
    /// <summary>
    /// Draws a gapped outline for a polygon, creating a dashed or segmented effect along the polygon's perimeter.
    /// </summary>
    /// <param name="poly">The polygon to draw.</param>
    /// <param name="perimeter">
    /// The total length of the polygon's perimeter.
    /// If zero or negative, the method calculates it automatically.
    /// Providing a known length avoids redundant calculations and improves performance, especially for static segments.
    /// </param>
    /// <param name="lineInfo">Parameters describing how to draw the outline.</param>
    /// <param name="gapDrawingInfo">Parameters describing the gap configuration.</param>
    /// <returns>
    /// The perimeter of the polygon if positive; otherwise, -1.
    /// If the shape does not change, the valid length can be reused in subsequent frames to avoid recalculating.
    /// </returns>
    /// <remarks>
    /// - If <paramref name="gapDrawingInfo.Gaps"/> is 0 or <paramref name="gapDrawingInfo.GapPerimeterPercentage"/> is 0, the outline is drawn solid.
    /// - If <paramref name="gapDrawingInfo.GapPerimeterPercentage"/> is 1 or greater, no outline is drawn.
    /// </remarks>
    public static float DrawGappedOutline(this Polygon poly, float perimeter, LineDrawingInfo lineInfo, GappedOutlineDrawingInfo gapDrawingInfo)
    {
        if (gapDrawingInfo.Gaps <= 0 || gapDrawingInfo.GapPerimeterPercentage <= 0f)
        {
            poly.DrawLines(lineInfo);
            return perimeter > 0f ? perimeter : -1f;
        }

        if (gapDrawingInfo.GapPerimeterPercentage >= 1f) return perimeter > 0f ? perimeter : -1f;

        var nonGapPercentage = 1f - gapDrawingInfo.GapPerimeterPercentage;

        var gapPercentageRange = gapDrawingInfo.GapPerimeterPercentage / gapDrawingInfo.Gaps;
        var nonGapPercentageRange = nonGapPercentage / gapDrawingInfo.Gaps;

        if (perimeter <= 0f)
        {
            for (int i = 0; i < poly.Count; i++)
            {
                var curP = poly[i];
                var nextP = poly[(i + 1) % poly.Count];

                perimeter += (nextP - curP).Length();
            }
        }

        var startDistance = perimeter * gapDrawingInfo.StartOffset;
        var curDistance = 0f;
        var nextDistance = startDistance;
        
        
        var curIndex = 0;
        var curPoint = poly[0];
        var nextPoint= poly[1 % poly.Count];
        var curW = nextPoint - curPoint;
        var curDis = curW.Length();
        
        var points = new List<Vector2>(3);

        int whileCounter = gapDrawingInfo.Gaps;
        
        while (whileCounter > 0)
        {
            if (curDistance + curDis >= nextDistance)
            {
                var p = curPoint + (curW / curDis) * (nextDistance - curDistance);
                
                
                if (points.Count == 0)
                {
                    nextDistance += nonGapPercentageRange * perimeter;
                    points.Add(p);

                }
                else
                {
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
                
                if(points.Count > 0) points.Add(nextPoint);
                
                curDistance += curDis;
                curIndex = (curIndex + 1) % poly.Count;
                curPoint = poly[curIndex];
                nextPoint = poly[(curIndex + 1) % poly.Count];
                curW = nextPoint - curPoint;
                curDis = curW.Length();
            }
            
        }

        return perimeter;
    }
   
    #endregion
}