using System.Drawing;
using System.Numerics;
using Clipper2Lib;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.StaticLib;
using Size = ShapeEngine.Core.Structs.Size;

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
    #endregion
    
    #region Draw
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
    
    #endregion

    #region Draw Rounded
    //TODO: Add Draw function with corner points and DrawRounded helper function
    
    //Q: Add Draw overload with cornerPoints parameter or add DrawRounded function?
    
    #endregion
    
    #region Draw Lines
    public static void DrawLines(this Polygon poly, float lineThickness, ColorRgba color, int cornerPoints = 0)
    {
        DrawLinesHelper(poly, lineThickness, color, cornerPoints);
    }
    public static void DrawLines(this Polygon poly, LineDrawingInfo lineInfo)
    {
        DrawLinesHelper(poly, lineInfo.Thickness, lineInfo.Color, lineInfo.CapPoints);
    }
    #endregion
    
    #region Draw Glow

    public static void DrawGlow(this Polygon polygon, float width, float endWidth, ColorRgba color, ColorRgba endColorRgba, int steps, int cornerPoints = 0)
    {
        if (polygon.Count < 2 || steps <= 0) return;

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
            polygon.DrawLines(currentWidth, currentColor, cornerPoints);
        }
    }
    #endregion
    
    
    
    #region Draw Lines Perimeter & Percentage
    //TODO: Implement new helper function
    //TODO: Update with new helper function 
    
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
    //TODO: Implement new helper function
    //TODO: Update with new helper function
    
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
    
    public static void DrawLinesHelper(Polygon poly, float thickness, ColorRgba color, int cornerPoints = 0, float miterLimit = 2f)
    {
        if (poly.Count <= 2) return;
        if (poly.Count == 3)
        {
            TriangleDrawing.DrawTriangleLines(poly[0], poly[1], poly[2], thickness, color, cornerPoints);
            return;
        }
        
        if(cornerPoints <= 0) DrawLinesMiteredHelper(poly, thickness, color, miterLimit);
        else DrawLinesRoundedHelper(poly, thickness, color, cornerPoints);
    }
    
    private static void DrawLinesMiteredHelper(Polygon poly, float lineThickness, ColorRgba color, float miterLimit = 2f)
    {
        int count = poly.Count;
        if (count < 3 || lineThickness <= 0f) return;

        float totalMiterLengthLimit = (lineThickness * 0.5f) * MathF.Max(2f, miterLimit);
        var rayColor = color.ToRayColor();
        var prev = poly[^2];
        var cur = poly[^1];
        var next = poly[0];

        var dirPrev = (cur - prev).Normalize();
        var dirNext = (next - cur).Normalize();
        var normalPrev = dirPrev.GetPerpendicularRight();
        var normalNext = dirNext.GetPerpendicularRight();

        Vector2 miterDir;
        float miterLength;
        float angleRad;
        Vector2 prevInside, prevOutside;
        
        var corner = dirPrev.ClassifyCorner(dirNext);
        if (corner.type == 0) //collinear
        {
            miterDir = dirNext;
            miterLength = lineThickness;
            prevInside = cur - miterDir * miterLength;
            prevOutside = cur + miterDir * miterLength;
        }
        else
        {
            miterDir = (normalPrev + normalNext).Normalize();
            angleRad = MathF.Abs(miterDir.AngleRad(normalNext));
            miterLength = lineThickness / MathF.Cos(angleRad);
            
            if (totalMiterLengthLimit > 0 && miterLength > totalMiterLengthLimit)
            {
                var bevelDir = corner.type < 0 ? -miterDir : miterDir;
                var limitPoint = cur + bevelDir * totalMiterLengthLimit;
                
                var miterDirPRight = corner.type < 0 ? miterDir.GetPerpendicularLeft() : miterDir.GetPerpendicularRight();
                var miterDirPLeft = -miterDirPRight;
                var p2 = next + (corner.type < 0 ? -normalNext : normalNext) * lineThickness;
                var bevelLeft = TryIntersectLines(limitPoint, miterDirPLeft, p2, -dirNext, out var intersectionLeft)
                    ? intersectionLeft
                    : cur + normalNext * lineThickness;
                
                var bevelInside = cur - bevelDir * miterLength;
                
                if (corner.type < 0) //ccw inwards corner
                {
                    prevOutside = bevelInside;
                    prevInside = bevelLeft;
                }
                else
                {
                    prevInside = bevelInside;
                    prevOutside = bevelLeft;
                }
            }
            else
            {
                prevInside = cur - miterDir * miterLength;
                prevOutside = cur + miterDir * miterLength;
            }
        }
        
        
        dirPrev = dirNext;
        prev = cur;
        
        for (var i = 0; i < count; i++)
        {
            cur = poly[i];
            next = poly[(i + 1) % count];
            
            dirNext = (next - cur).Normalize();
            normalPrev = normalNext;
            normalNext = dirNext.GetPerpendicularRight();
            corner = dirPrev.ClassifyCorner(dirNext);
            
            if (corner.type == 0) //collinear
            {
                miterDir = dirNext;
                miterLength = lineThickness;
            }
            else
            {
                miterDir = (normalPrev + normalNext).Normalize();
                angleRad = MathF.Abs(miterDir.AngleRad(normalNext));
                miterLength = lineThickness / MathF.Cos(angleRad); //right triangle formula for hypotenuse = adjacent / cos(angle) -> angle between adjacent and hypotenuse

                if (totalMiterLengthLimit > 0 && miterLength > totalMiterLengthLimit)
                {
                    var bevelDir = corner.type < 0 ? -miterDir : miterDir;
                    var limitPoint = cur + bevelDir * totalMiterLengthLimit;
                    
                    var miterDirPRight = corner.type < 0 ? miterDir.GetPerpendicularLeft() : miterDir.GetPerpendicularRight();
                    var miterDirPLeft = -miterDirPRight;
                    var p1 = prev + (corner.type < 0 ? -normalPrev : normalPrev) * lineThickness;
                    var p2 = next + (corner.type < 0 ? -normalNext : normalNext) * lineThickness;
                    var bevelRight = TryIntersectLines(limitPoint, miterDirPRight, p1, dirPrev, out var intersectionRight)
                        ? intersectionRight
                        : cur + normalPrev * lineThickness;
                    var bevelLeft = TryIntersectLines(limitPoint, miterDirPLeft, p2, -dirNext, out var intersectionLeft)
                        ? intersectionLeft
                        : cur + normalNext * lineThickness;
                    
                    var bevelInside = cur - bevelDir * miterLength;
                    

                    if (corner.type < 0) //ccw inwards corner
                    {
                        (bevelLeft, bevelRight) = (bevelRight, bevelLeft);
                        Raylib.DrawTriangle(bevelInside, bevelLeft, prevInside, rayColor);
                        Raylib.DrawTriangle(bevelInside, prevInside, prevOutside, rayColor);
                        Raylib.DrawTriangle(bevelInside, bevelRight, bevelLeft, rayColor);
                        prevOutside = bevelInside;
                        prevInside = bevelRight;
                    }
                    else
                    {
                        Raylib.DrawTriangle(bevelRight, prevInside, prevOutside, rayColor);
                        Raylib.DrawTriangle(bevelRight, bevelInside, prevInside, rayColor);
                        Raylib.DrawTriangle(bevelLeft, bevelInside, bevelRight,  rayColor);
                        prevInside = bevelInside;
                        prevOutside = bevelLeft;
                    }
                    
                    dirPrev = dirNext;
                    prev = cur;
                    continue;
                }
            }
            
            var curInside = cur - miterDir * miterLength;
            var curOutside = cur + miterDir * miterLength;
            
            //Draw 2 triangles for the edge quad
            Raylib.DrawTriangle(curOutside, curInside, prevOutside, rayColor);
            Raylib.DrawTriangle(curInside, prevInside, prevOutside, rayColor);
            
            prevInside = curInside;
            prevOutside = curOutside;
            dirPrev = dirNext;
            prev = cur;
        }
    }
    
    private static void DrawLinesRoundedHelper(Polygon poly, float lineThickness, ColorRgba color, int cornerPoints)
    {
        if (poly.Count < 3 || lineThickness <= 0f || cornerPoints <= 0) return;
    
        float halfThickness = lineThickness * 1f;
        var rayColor = color.ToRayColor();
    
        for (int i = 0; i < poly.Count; i++)
        {
            var p1 = poly.GetPoint(i - 1);
            var p2 = poly.GetPoint(i);
            var p3 = poly.GetPoint(i + 1);
            int nextI = (i + 1) % poly.Count;
            var p4 = poly.GetPoint(nextI + 1);
    
            var edge1 = p2 - p1;
            var edge2 = p3 - p2;
            var edge3 = p4 - p3;
    
            var n1 = Vector2.Normalize(new(-edge1.Y, edge1.X));
            var n2 = Vector2.Normalize(new(-edge2.Y, edge2.X));
            var n3 = Vector2.Normalize(new(-edge3.Y, edge3.X));
    
            var cornerTypeCur = edge1.ClassifyCorner(edge2);
            var cornerTypeNext = edge2.ClassifyCorner(edge3);
            
            Vector2 inner1, inner2, outer1, outer2;
    
            if (cornerTypeCur.type > 0) // Convex (CCW)
            {
                inner1 = CalculateMiterPoint(p2, n1, n2, halfThickness, false);
                outer1 = p2 + n2 * halfThickness;
                DrawCornerFan(p2, n1, n2, inner1, halfThickness, rayColor, cornerPoints, true);
            }
            else // Concave or Collinear
            {
                outer1 = CalculateMiterPoint(p2, n1, n2, halfThickness, true);
                if (cornerTypeCur.type < 0)
                {
                    inner1 = p2 - n2 * halfThickness;
                    DrawCornerFan(p2, -n2, -n1, outer1, halfThickness, rayColor, cornerPoints, false);
                }
                else
                {
                    inner1 = CalculateMiterPoint(p2, n1, n2, halfThickness, false);
                }
            }
    
            if (cornerTypeNext.type > 0) // Convex (CCW)
            {
                inner2 = CalculateMiterPoint(p3, n2, n3, halfThickness, false);
                outer2 = p3 + n2 * halfThickness;
            }
            else // Concave or Collinear
            {
                outer2 = CalculateMiterPoint(p3, n2, n3, halfThickness, true);
                if (cornerTypeNext.type < 0)
                {
                    inner2 = p3 - n2 * halfThickness;
                }
                else
                {
                    inner2 = CalculateMiterPoint(p3, n2, n3, halfThickness, false);
                }
            }
            
            Raylib.DrawTriangle(inner1, outer1, inner2, rayColor);
            Raylib.DrawTriangle(outer1, outer2, inner2, rayColor);
        }
    }
    private static Vector2 CalculateMiterPoint(Vector2 corner, Vector2 normalPrev, Vector2 normalNext, float halfThickness, bool outer)
    {
        var miterDir = Vector2.Normalize(normalPrev + normalNext);
        float dot = Vector2.Dot(miterDir, normalPrev);
        if (MathF.Abs(dot) < 0.0001f) return corner + normalPrev * (outer ? halfThickness : -halfThickness);
        float miterLength = halfThickness / dot;
    
        return corner + miterDir * (outer ? miterLength : -miterLength);
    }
    private static void DrawCornerFan(Vector2 corner, Vector2 n1, Vector2 n2, Vector2 innerCorner, float radius, Raylib_cs.Color color, int segments, bool isConvex)
    {
        float startAngle = MathF.Atan2(n1.Y, n1.X);
        float endAngle = MathF.Atan2(n2.Y, n2.X);
    
        float angleDiff = endAngle - startAngle;
        if (isConvex)
        {
            if (angleDiff <= -MathF.PI) angleDiff += 2 * MathF.PI;
            if (angleDiff > MathF.PI) angleDiff -= 2 * MathF.PI;
        }
        else
        {
            if (angleDiff >= MathF.PI) angleDiff -= 2 * MathF.PI;
            if (angleDiff < -MathF.PI) angleDiff += 2 * MathF.PI;
        }
        
        int numSegments = Math.Max(1, segments);
        float angleStep = angleDiff / numSegments;
    
        var p1 = corner + n1 * radius;
    
        for (int i = 1; i <= numSegments; i++)
        {
            float angle = startAngle + angleStep * i;
            var p2 = corner + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
            Raylib.DrawTriangle(innerCorner, p1, p2, color);
            p1 = p2;
        }
    }

    
    public static bool TryIntersectLines(Vector2 pointA, Vector2 dirA, Vector2 pointB, Vector2 dirB, out Vector2 intersection)
    {
        intersection = default;
        float denom = dirA.X * dirB.Y - dirA.Y * dirB.X;
        if (MathF.Abs(denom) <= float.Epsilon) return false; // parallel

        var diff = pointB - pointA;
        float t = (diff.X * dirB.Y - diff.Y * dirB.X) / denom;
        intersection = pointA + dirA * t;
        return true;
    }
    #endregion
}




    

