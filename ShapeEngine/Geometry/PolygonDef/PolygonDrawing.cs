using System.Numerics;
using System.Runtime.Intrinsics.X86;
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
    
    #region Draw Lines Perimeter & Percentage
    
    /// <summary>
    /// Draws a portion of the polygon's perimeter as a thick line, starting at a given index.
    /// Handles miter and beveled joins, and allows for custom miter limits.
    /// </summary>
    /// <param name="poly">The polygon whose perimeter will be drawn.</param>
    /// <param name="perimeterToDraw">
    /// The length of the perimeter to draw. If negative, draws in reverse order.
    /// </param>
    /// <param name="startIndex">
    /// The index of the starting vertex. Drawing proceeds forward or backward depending on the sign of <paramref name="perimeterToDraw"/>.
    /// </param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline, including alpha for transparency.</param>
    /// <param name="miterLimit">
    /// The miter limit for joins. If the miter length exceeds this value times the line thickness, a bevel or squared join is used instead. Default is 2.0f.
    /// </param>
    /// <param name="beveled">
    /// If true, forces beveled joins instead of squared joins when the miter limit is exceeded.
    /// </param>
    public static void DrawLinesPerimeter(this Polygon poly, float perimeterToDraw, int startIndex, float lineThickness, ColorRgba color, float miterLimit = 2f, bool beveled = false)
    {
        if (poly.Count < 3 || perimeterToDraw == 0) return;
        
        var rayColor = color.ToRayColor();
        float totalMiterLengthLimit = lineThickness * 0.5f * MathF.Max(2f, miterLimit);
        float f = -1f;
        
        bool reverse = perimeterToDraw < 0;
        if (reverse) perimeterToDraw *= -1;
        
        int i = ShapeMath.Clamp(startIndex, 0, poly.Count - 1);
        int steps = poly.Count;
        
        var lastCornerType = 1;
        var prev = poly[reverse ? ShapeMath.WrapIndex(poly.Count, i + 1) : ShapeMath.WrapIndex(poly.Count, i - 1)];
        var cur = poly[i];
        var next = poly[reverse ? ShapeMath.WrapIndex(poly.Count, i - 1) : ShapeMath.WrapIndex(poly.Count, i + 1)];
        var wPrev = cur - prev;
        var wNext = next - cur;
        float lsPrev = wPrev.LengthSquared();
        float lsNext = wNext.LengthSquared();
        if (lsPrev <= 0 || lsNext <= 0) return;

        var dirPrev = wPrev.Normalize();
        var dirNext = wNext.Normalize();
        
        //flip based on corner type
        var cornerType = dirPrev.ClassifyCorner(dirNext);
        if (cornerType.type == 0)//collinear
        {
            //we dont treat collinear differently so we set it to 1 (ccw outwards corner)
            cornerType = (1, cornerType.angle);
        }
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
        
        var miterDir = (normalPrev + normalNext).Normalize();
        float miterAngleRad = MathF.Abs(miterDir.AngleRad(normalNext));
        float miterLength = lineThickness / MathF.Cos(miterAngleRad);

        Vector2 lastInner, lastOuter;
        if (miterLimit < 2f || miterLength < totalMiterLengthLimit)
        {
            lastOuter = cur + miterDir * miterLength;
            lastInner = cur - miterDir * miterLength;
        }
        else
        {
            miterLength = totalMiterLengthLimit;

            var prevOuter = prev + normalPrev * lineThickness;
            var prevInner = prev - normalPrev * lineThickness;
            var nextInner = next - normalNext * lineThickness;
            var intersection = Ray.IntersectRayRay(prevInner, dirPrev, nextInner, -dirNext);
            lastInner = intersection.Valid ? intersection.Point : cur - miterDir * miterLength;
            
            if (beveled)
            {
                // lastOuter = cur + normalPrev * lineThickness;
                lastOuter = cur + normalNext * lineThickness;
            }
            else
            {
                var cornerOuter = cur + miterDir * miterLength;
                var miterPerpRight = cornerType.type >= 0 ? miterDir.GetPerpendicularRight() : miterDir.GetPerpendicularLeft();
                intersection = Ray.IntersectRayRay(prevOuter, dirPrev, cornerOuter, miterPerpRight);
                if (intersection.Valid)
                {
                    // lastOuter = intersection.Point;
                    float l = (cornerOuter - intersection.Point).Length();
                    lastOuter = cornerOuter - miterPerpRight * l;
                }
                else //bevel fallback
                {
                    // lastOuter = cur + normalPrev * lineThickness;
                    lastOuter = cur + normalNext * lineThickness;
                }
            }
        }

        if (cornerType.type < 0)
        {
            (lastInner, lastOuter) = (lastOuter, lastInner);
        }

        // var dir = (nextPoint - curPoint).Normalize();
        //
        // var perp = dir.GetPerpendicularRight();
        // var lastOuter = curPoint + perp * lineThickness;
        // var lastInner = curPoint - perp * lineThickness;
        
        i = reverse ? i - 1 : i + 1;
        
        while(steps > 0 && f < 0f)
        {
            prev = poly[reverse ? ShapeMath.WrapIndex(poly.Count, i + 1) : ShapeMath.WrapIndex(poly.Count, i - 1)];
            cur = poly[ShapeMath.WrapIndex(poly.Count, i)];
            next = poly[reverse ? ShapeMath.WrapIndex(poly.Count, i - 1) : ShapeMath.WrapIndex(poly.Count, i + 1)];
            
            i = reverse ? i - 1 : i + 1;
            steps--;
            
            float length = (next - cur).Length();
            if (length <= perimeterToDraw)
            {
                perimeterToDraw -= length;
            }
            else
            {
                f = ShapeMath.Clamp(perimeterToDraw / length, 0f, 1f);
            }
            
            wPrev = cur - prev;
            wNext = next - cur;
            lsPrev = wPrev.LengthSquared();
            lsNext = wNext.LengthSquared();
            if (lsPrev <= 0 || lsNext <= 0) return;

            dirPrev = wPrev.Normalize();
            dirNext = wNext.Normalize();
            
            //flip based on corner type
            cornerType = dirPrev.ClassifyCorner(dirNext);
            if (cornerType.type == 0)//collinear
            {
                //we dont treat collinear differently so we set it to 1 (ccw outwards corner)
                cornerType = (1, cornerType.angle);
            }
            
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

            miterDir = (normalPrev + normalNext).Normalize();
            miterAngleRad = MathF.Abs(miterDir.AngleRad(normalNext));
            miterLength = lineThickness / MathF.Cos(miterAngleRad);

            if (miterLimit < 2f || miterLength < totalMiterLengthLimit)
            {
                var cornerOuter = cur + miterDir * miterLength;
                var cornerInner = cur - miterDir * miterLength;

                if (f >= 0f)
                {
                    if (cornerType.type == lastCornerType)
                    {
                        cornerOuter = lastOuter.Lerp(cornerOuter, f);
                        cornerInner = lastInner.Lerp(cornerInner, f);
                    }
                    else
                    {
                        cornerOuter = lastInner.Lerp(cornerOuter, f);
                        cornerInner = lastOuter.Lerp(cornerInner, f);
                    }
                }
                
                if (cornerType.type >= 0)
                {
                    if (lastCornerType >= 0)
                    {
                        Raylib.DrawTriangle(cornerInner, lastInner, lastOuter, rayColor);
                    }
                    else
                    {
                        Raylib.DrawTriangle(cornerOuter, lastOuter, lastInner, rayColor);
                    }

                    Raylib.DrawTriangle(cornerInner, lastOuter, cornerOuter, rayColor);
                }
                else
                {
                    if (lastCornerType >= 0)
                    {
                        Raylib.DrawTriangle(cornerOuter, lastInner, lastOuter, rayColor);
                    }
                    else
                    {
                        Raylib.DrawTriangle(lastInner, cornerInner, lastOuter, rayColor);
                    }

                    Raylib.DrawTriangle(lastOuter, cornerInner, cornerOuter, rayColor);
                }

                lastInner = cornerInner;
                lastOuter = cornerOuter;
                lastCornerType = cornerType.type;
            }
            else
            {
                miterLength = totalMiterLengthLimit;
                Vector2 cornerOuterPrev, cornerOuterNext;

                var prevOuter = prev + normalPrev * lineThickness;
                var prevInner = prev - normalPrev * lineThickness;
                var nextInner = next - normalNext * lineThickness;
                var intersection = Ray.IntersectRayRay(prevInner, dirPrev, nextInner, -dirNext);
                var cornerInner = intersection.Valid ? intersection.Point : cur - miterDir * miterLength;
                
                if (beveled)
                {
                    cornerOuterPrev = cur + normalPrev * lineThickness;
                    cornerOuterNext = cur + normalNext * lineThickness;
                }
                else
                {
                    var cornerOuter = cur + miterDir * miterLength;
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
                        cornerOuterPrev = cur + normalPrev * lineThickness;
                        cornerOuterNext = cur + normalNext * lineThickness;
                    }
                }
                
                bool drawCorner = true;
                if (f >= 0f)
                {
                    // Calculate segment lengths
                    float lenA = (cornerOuterPrev - lastOuter).Length();
                    float lenB = (cornerOuterNext - cornerOuterPrev).Length();
                    float totalLen = lenA + lenB;

                    // Calculate normalized thresholds
                    float tA = lenA / totalLen; // The fraction of the stroke spent on the first segment
                    
                    if (cornerType.type == lastCornerType)
                    {
                        if (f <= tA)
                        {
                            // Lerp from lastOuter to cornerOuterPrev
                            float localF = f / tA;
                            cornerOuterPrev = Vector2.Lerp(lastOuter, cornerOuterPrev, localF);
                            cornerInner = lastInner.Lerp(cornerInner, localF);
                            drawCorner = false;
                        }
                        else
                        {
                            // Lerp from cornerOuterPrev to cornerOuterNext
                            float localF = (f - tA) / (1f - tA);
                            cornerOuterNext = Vector2.Lerp(cornerOuterPrev, cornerOuterNext, localF);
                        }
                    }
                    else
                    {
                        if (f <= tA)
                        {
                            // Lerp from lastOuter to cornerOuterPrev
                            float localF = f / tA;
                            cornerOuterPrev = Vector2.Lerp(lastInner, cornerOuterPrev, localF);
                            cornerInner = lastOuter.Lerp(cornerInner, localF);
                            drawCorner = false;
                        }
                        else
                        {
                            // Lerp from cornerOuterPrev to cornerOuterNext
                            float localF = (f - tA) / (1f - tA);
                            cornerOuterNext = Vector2.Lerp(cornerOuterPrev, cornerOuterNext, localF);
                        }
                    }
                }
                
                if (cornerType.type >= 0)
                {
                    if (lastCornerType >= 0)
                    {
                        Raylib.DrawTriangle(cornerInner, lastInner, lastOuter, rayColor);
                        Raylib.DrawTriangle(cornerInner, lastOuter, cornerOuterPrev, rayColor);
                    }
                    else
                    {
                        Raylib.DrawTriangle(cornerInner, lastOuter, lastInner, rayColor);
                        Raylib.DrawTriangle(cornerInner, lastInner, cornerOuterPrev, rayColor);
                    }

                    if(drawCorner) Raylib.DrawTriangle(cornerOuterPrev, cornerOuterNext, cornerInner, rayColor);
                }
                else
                {
                    if (lastCornerType >= 0)
                    {
                        Raylib.DrawTriangle(cornerInner, lastInner, lastOuter, rayColor);
                        Raylib.DrawTriangle(cornerInner, cornerOuterPrev, lastInner, rayColor);
                    }
                    else
                    {
                        Raylib.DrawTriangle(cornerInner, lastOuter, lastInner, rayColor);
                        Raylib.DrawTriangle(cornerInner, cornerOuterPrev, lastOuter, rayColor);
                    }

                    if(drawCorner) Raylib.DrawTriangle(cornerOuterNext, cornerOuterPrev, cornerInner, rayColor);
                }
                
                lastInner = cornerInner;
                lastOuter = cornerOuterNext;
                lastCornerType = cornerType.type;
            }
        }
    }
    
    /// <summary>
    /// Draws a portion of the polygon's perimeter as a thick line, starting at a given index, using <see cref="LineDrawingInfo"/> for line options.
    /// Handles miter and beveled joins, and allows for custom miter limits.
    /// </summary>
    /// <param name="poly">The polygon whose perimeter will be drawn.</param>
    /// <param name="perimeterToDraw">
    /// The length of the perimeter to draw. If negative, draws in reverse order.
    /// </param>
    /// <param name="startIndex">
    /// The index of the starting vertex. Drawing proceeds forward or backward depending on the sign of <paramref name="perimeterToDraw"/>.
    /// </param>
    /// <param name="lineInfo">The line drawing information (thickness & color only).</param>
    /// <param name="miterLimit">
    /// The miter limit for joins. If the miter length exceeds this value times the line thickness, a bevel or squared join is used instead. Default is 2.0f.
    /// </param>
    /// <param name="beveled">
    /// If true, forces beveled joins instead of squared joins when the miter limit is exceeded.
    /// </param>
    public static void DrawLinesPerimeter(this Polygon poly, float perimeterToDraw, int startIndex,  LineDrawingInfo lineInfo, float miterLimit = 2f, bool beveled = false)
    {
        poly.DrawLinesPerimeter(perimeterToDraw, startIndex,  lineInfo.Thickness, lineInfo.Color,  miterLimit, beveled);
    }
    
    /// <summary>
    /// Draws a portion of the polygon's perimeter as a thick line, starting at a given index, based on a percentage of the total perimeter.
    /// </summary>
    /// <param name="poly">The polygon whose perimeter will be drawn.</param>
    /// <param name="f">
    /// The percentage (0-1) of the perimeter to draw. If negative, draws in reverse order.
    /// </param>
    /// <param name="startIndex">
    /// The index of the starting vertex. Drawing proceeds forward or backward depending on the sign of <paramref name="f"/>.
    /// </param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline, including alpha for transparency.</param>
    /// <param name="miterLimit">
    /// The miter limit for joins. If the miter length exceeds this value times the line thickness, a bevel or squared join is used instead. Default is 2.0f.
    /// </param>
    /// <param name="beveled">
    /// If true, forces beveled joins instead of squared joins when the miter limit is exceeded.
    /// </param>
    public static void DrawLinesPercentage(this Polygon poly, float f, int startIndex, float lineThickness, ColorRgba color, float miterLimit = 2f, bool beveled = false)
    {
        if (poly.Count < 3 || f == 0f) return;

        var negative = false;
        if (f < 0)
        {
            negative = true;
            f *= -1;
        }
        
        f = ShapeMath.WrapF(f, 0f, 1f);
        startIndex = ShapeMath.WrapIndex(startIndex, poly.Count);
        if (f <= 0f) return;
        if (f >= 1f)
        {
            poly.DrawLines(lineThickness, color, miterLimit, beveled);
            return;
        }
        
        var perimeter = 0f;
        for (var i = 0; i < poly.Count; i++)
        {
            var start = poly[i];
            var end = poly[(i + 1) % poly.Count];
            float l = (end - start).Length();
            perimeter += l;
        }

        poly.DrawLinesPerimeter(perimeter * f * (negative ? -1 : 1), startIndex, lineThickness, color, miterLimit, beveled);
    }
    
    /// <summary>
    /// Draws a portion of the polygon's perimeter as a thick line, starting at a given index, based on a percentage of the total perimeter,
    /// using <see cref="LineDrawingInfo"/> for line options.
    /// </summary>
    /// <param name="poly">The polygon whose perimeter will be drawn.</param>
    /// <param name="f">
    /// The percentage (0-1) of the perimeter to draw. If negative, draws in reverse order.
    /// </param>
    /// <param name="startIndex">
    /// The index of the starting vertex. Drawing proceeds forward or backward depending on the sign of <paramref name="f"/>.
    /// </param>
    /// <param name="lineInfo">The line drawing information (thickness & color only).</param>
    /// <param name="miterLimit">
    /// The miter limit for joins. If the miter length exceeds this value times the line thickness, a bevel or squared join is used instead. Default is 2.0f.
    /// </param>
    /// <param name="beveled">
    /// If true, forces beveled joins instead of squared joins when the miter limit is exceeded.
    /// </param>
    public static void DrawLinesPercentage(this Polygon poly, float f, int startIndex, LineDrawingInfo lineInfo, float miterLimit = 2f, bool beveled = false)
    {
        poly.DrawLinesPercentage(f, startIndex, lineInfo.Thickness, lineInfo.Color, miterLimit, beveled);
    }
    #endregion
    
    #region Draw Lines Perimeter & Percentage Capped

    /// <summary>
    /// Draws a portion of the polygon's perimeter as a thick line with capped ends, starting at a given index.
    /// Supports custom line thickness, color, cap type, and number of cap points.
    /// This function is slower than <see cref="DrawLinesPerimeter(Polygon, float, int, float, ColorRgba, float, bool)"/>
    /// due to drawing more triangles for the caps.
    /// </summary>
    /// <param name="poly">The polygon whose perimeter will be drawn.</param>
    /// <param name="perimeterToDraw">
    /// The length of the perimeter to draw. If negative, draws in reverse order.
    /// </param>
    /// <param name="startIndex">
    /// The index of the starting vertex. Drawing proceeds forward or backward depending on the sign of <paramref name="perimeterToDraw"/>.
    /// </param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline. Does not support transparency, alpha will be set to 255 internally.</param>
    /// <param name="capType">The type of line cap to use at the ends of the drawn segment.</param>
    /// <param name="capPoints">The number of points for the cap (used for round caps).</param>
    public static void DrawLinesPerimeterCapped(this Polygon poly, float perimeterToDraw, int startIndex, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
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
            float l = (end - start).Length();
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
    /// Draws a portion of the polygon's perimeter as a thick line with capped ends, starting at a given index,
    /// based on a percentage of the total perimeter.
    /// This function is slower than <see cref="DrawLinesPercentage(Polygon, float, int, float, ColorRgba, float, bool)"/>
    /// due to drawing more triangles for the caps.
    /// </summary>
    /// <param name="poly">The polygon whose perimeter will be drawn.</param>
    /// <param name="f">
    /// The percentage (0-1) of the perimeter to draw. If negative, draws in reverse order.
    /// </param>
    /// <param name="startIndex">
    /// The index of the starting vertex. Drawing proceeds forward or backward depending on the sign of <paramref name="f"/>.
    /// </param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">
    /// The color of the outline. Does not support transparency, alpha will be set to 255 internally.
    /// </param>
    /// <param name="capType">The type of line cap to use at the ends of the drawn segment.</param>
    /// <param name="capPoints">The number of points for the cap (used for round caps).</param>
    public static void DrawLinesPercentageCapped(this Polygon poly, float f, int startIndex, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (poly.Count < 3 || f == 0f) return;

        var negative = false;
        if (f < 0)
        {
            negative = true;
            f *= -1;
        }
        
        f = ShapeMath.WrapF(f, 0f, 1f);
        startIndex = ShapeMath.WrapIndex(startIndex, poly.Count);
        if (f <= 0f) return;
        if (f >= 1f)
        {
            poly.DrawLines(lineThickness, color);
            return;
        }

        var perimeter = 0f;
        for (var i = 0; i < poly.Count; i++)
        {
            var start = poly[i];
            var end = poly[(i + 1) % poly.Count];
            float l = (end - start).Length();
            perimeter += l;
        }

        poly.DrawLinesPerimeterCapped(perimeter * f * (negative ? -1 : 1), startIndex, lineThickness, color, capType, capPoints);
    }

    /// <summary>
    /// Draws a portion of the polygon's perimeter as a thick line with capped ends, starting at a given index,
    /// using <see cref="LineDrawingInfo"/> for line options. Supports custom cap types and cap points.
    /// This function is slower than <see cref="DrawLinesPercentage(Polygon, float, int, LineDrawingInfo, float, bool)"/> due to drawing more triangles for the caps.
    /// </summary>
    /// <param name="poly">The polygon whose perimeter will be drawn.</param>
    /// <param name="f">
    /// The percentage (0-1) of the perimeter to draw. If negative, draws in reverse order.
    /// </param>
    /// <param name="startIndex">
    /// The index of the starting vertex. Drawing proceeds forward or backward depending on the sign of <paramref name="f"/>.
    /// </param>
    /// <param name="lineInfo">
    /// The line drawing information (thickness, color, cap type, cap points).
    /// Color is expected to be opaque (alpha will be set to 255 internally).
    /// </param>
    public static void DrawLinesPercentageCapped(this Polygon poly, float f, int startIndex, LineDrawingInfo lineInfo)
    {
        poly.DrawLinesPercentageCapped(f, startIndex, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
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
        Vector2 lastOuter = Vector2.Zero, lastInner = Vector2.Zero;
        var lastCornerType = 0;
        var initialized = false;

        var rayColor = color.ToRayColor();
        float totalMiterLengthLimit = lineThickness * 0.5f * MathF.Max(2f, miterLimit);

        for (var i = 0; i <= poly.Count; i++)
        {
            var prev = poly[ShapeMath.WrapIndex(poly.Count, i - 1)];
            var cur = poly[ShapeMath.WrapIndex(poly.Count, i)];
            var next = poly[ShapeMath.WrapIndex(poly.Count, i + 1)];

            var wPrev = cur - prev;
            var wNext = next - cur;
            float lsPrev = wPrev.LengthSquared();
            float lsNext = wNext.LengthSquared();
            if (lsPrev <= 0 || lsNext <= 0) return;

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

            if (cornerType.type == 0) //collinear
            {
                var cornerOuter = cur + normalNext * lineThickness;
                var cornerInner = cur - normalNext * lineThickness;

                if (!initialized)
                {
                    lastInner = cornerInner;
                    lastOuter = cornerOuter;
                    lastCornerType = cornerType.type;
                    initialized = true;
                    continue;
                }

                Raylib.DrawTriangle(cornerInner, lastInner, lastOuter, rayColor);
                Raylib.DrawTriangle(cornerInner, lastOuter, cornerOuter, rayColor);

                lastInner = cornerInner;
                lastOuter = cornerOuter;
                lastCornerType = cornerType.type;
                continue;
            }

            var miterDir = (normalPrev + normalNext).Normalize();
            float miterAngleRad = MathF.Abs(miterDir.AngleRad(normalNext));
            float miterLength = lineThickness / MathF.Cos(miterAngleRad);

            if (miterLimit < 2f || miterLength < totalMiterLengthLimit)
            {
                var cornerOuter = cur + miterDir * miterLength;
                var cornerInner = cur - miterDir * miterLength;

                if (!initialized)
                {
                    lastInner = cornerInner;
                    lastOuter = cornerOuter;
                    lastCornerType = cornerType.type;
                    initialized = true;
                    continue;
                }

                if (cornerType.type >= 0)
                {
                    if (lastCornerType >= 0)
                    {
                        Raylib.DrawTriangle(cornerInner, lastInner, lastOuter, rayColor);
                    }
                    else
                    {
                        Raylib.DrawTriangle(cornerOuter, lastOuter, lastInner, rayColor);
                    }

                    Raylib.DrawTriangle(cornerInner, lastOuter, cornerOuter, rayColor);
                }
                else
                {
                    if (lastCornerType >= 0)
                    {
                        Raylib.DrawTriangle(cornerOuter, lastInner, lastOuter, rayColor);
                    }
                    else
                    {
                        Raylib.DrawTriangle(lastInner, cornerInner, lastOuter, rayColor);
                    }

                    Raylib.DrawTriangle(lastOuter, cornerInner, cornerOuter, rayColor);
                }

                lastInner = cornerInner;
                lastOuter = cornerOuter;
                lastCornerType = cornerType.type;
            }
            else
            {
                miterLength = totalMiterLengthLimit;
                Vector2 cornerOuterPrev, cornerOuterNext;

                var prevOuter = prev + normalPrev * lineThickness;
                var prevInner = prev - normalPrev * lineThickness;
                var nextInner = next - normalNext * lineThickness;
                var intersection = Ray.IntersectRayRay(prevInner, dirPrev, nextInner, -dirNext);
                var cornerInner = intersection.Valid ? intersection.Point : cur - miterDir * miterLength;


                if (beveled)
                {
                    cornerOuterPrev = cur + normalPrev * lineThickness;
                    cornerOuterNext = cur + normalNext * lineThickness;

                    // cornerOuterPrev.Draw(2, new ColorRgba(System.Drawing.Color.Orange));
                    // cornerOuterNext.Draw(2, new ColorRgba(System.Drawing.Color.Orange));
                }
                else
                {
                    var cornerOuter = cur + miterDir * miterLength;
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
                        cornerOuterPrev = cur + normalPrev * lineThickness;
                        cornerOuterNext = cur + normalNext * lineThickness;
                    }
                }

                if (!initialized)
                {
                    lastInner = cornerInner;
                    lastOuter = cornerOuterNext;
                    lastCornerType = cornerType.type;
                    initialized = true;
                    continue;
                }


                if (cornerType.type >= 0)
                {
                    if (lastCornerType >= 0)
                    {
                        Raylib.DrawTriangle(cornerInner, lastInner, lastOuter, rayColor);
                        Raylib.DrawTriangle(cornerInner, lastOuter, cornerOuterPrev, rayColor);
                    }
                    else
                    {
                        Raylib.DrawTriangle(cornerInner, lastOuter, lastInner, rayColor);
                        Raylib.DrawTriangle(cornerInner, lastInner, cornerOuterPrev, rayColor);
                    }

                    Raylib.DrawTriangle(cornerOuterPrev, cornerOuterNext, cornerInner, rayColor);
                }
                else
                {
                    if (lastCornerType >= 0)
                    {
                        Raylib.DrawTriangle(cornerInner, lastInner, lastOuter, rayColor);
                        Raylib.DrawTriangle(cornerInner, cornerOuterPrev, lastInner, rayColor);
                    }
                    else
                    {
                        Raylib.DrawTriangle(cornerInner, lastOuter, lastInner, rayColor);
                        Raylib.DrawTriangle(cornerInner, cornerOuterPrev, lastOuter, rayColor);
                    }

                    Raylib.DrawTriangle(cornerOuterNext, cornerOuterPrev, cornerInner, rayColor);
                }

                lastInner = cornerInner;
                lastOuter = cornerOuterNext;
                lastCornerType = cornerType.type;
            }
        }
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
        poly.DrawLines(lineInfo.Thickness, lineInfo.Color, miterLimit, beveled);
    }

    #endregion

    #region Draw Lines Convex

    /// <summary>
    /// Draws the outline of a convex polygon with the specified line thickness and color.
    /// This method is optimized for convex polygons and supports miter and beveled joins.
    /// </summary>
    /// <param name="poly">The convex polygon to draw. Must have at least 3 points.</param>
    /// <param name="lineThickness">The thickness of the outline in world units.</param>
    /// <param name="color">The color of the outline..</param>
    /// <param name="miterLimit">
    /// The miter limit for joins. If the miter length exceeds this value times the line thickness, a bevel is used instead.
    /// Default is 2.0f.
    /// </param>
    /// <param name="beveled">
    /// If true, forces beveled joins instead of miters when the miter limit is exceeded.
    /// </param>
    public static void DrawLinesConvex(this Polygon poly, float lineThickness, ColorRgba color, float miterLimit = 2f, bool beveled = false)
    {
        if (poly.Count < 3) return;

        Vector2 outsidePrev = Vector2.Zero, insidePrev = Vector2.Zero;
        var initialized = false;

        var rayColor = color.ToRayColor();
        float totalMiterLengthLimit = lineThickness * 0.5f * MathF.Max(2f, miterLimit);

        for (var i = 0; i <= poly.Count; i++)
        {
            var prev = poly[ShapeMath.WrapIndex(poly.Count, i - 1)];
            var cur = poly[ShapeMath.WrapIndex(poly.Count, i)];
            var next = poly[ShapeMath.WrapIndex(poly.Count, i + 1)];

            var wPrev = cur - prev;
            var wNext = next - cur;
            float lsPrev = wPrev.LengthSquared();
            float lsNext = wNext.LengthSquared();
            if (lsPrev <= 0 || lsNext <= 0) continue;

            var dirPrev = wPrev.Normalize();
            var dirNext = wNext.Normalize();

            var normalPrev = dirPrev.GetPerpendicularRight();
            var normalNext = dirNext.GetPerpendicularRight();

            var miterDir = (normalPrev + normalNext).Normalize();
            float miterAngleRad = MathF.Abs(miterDir.AngleRad(normalNext));
            float miterLength = lineThickness / MathF.Cos(miterAngleRad);

            if (miterLimit < 2f || miterLength < totalMiterLengthLimit)
            {
                if (!initialized)
                {
                    insidePrev = cur - miterDir * miterLength;
                    outsidePrev = cur + miterDir * miterLength;
                    initialized = true;
                    continue;
                }

                var outsideCur = cur + miterDir * miterLength;
                var insideCur = cur - miterDir * miterLength;

                Raylib.DrawTriangle(outsidePrev, outsideCur, insideCur, rayColor);
                Raylib.DrawTriangle(outsidePrev, insideCur, insidePrev, rayColor);

                outsidePrev = outsideCur;
                insidePrev = insideCur;
            }
            else
            {
                miterLength = totalMiterLengthLimit;

                var insideCur = cur - miterDir * miterLength;

                Vector2 outsideLeftCur, outsideRightCur;

                if (beveled)
                {
                    outsideLeftCur = cur + normalNext * lineThickness;

                    if (!initialized)
                    {
                        insidePrev = insideCur;
                        outsidePrev = outsideLeftCur;
                        initialized = true;
                        continue;
                    }

                    outsideRightCur = cur + normalPrev * lineThickness;
                }
                else
                {
                    var p = cur + miterDir * miterLength;
                    var dir = (p - cur).Normalize();
                    var perp = dir.GetPerpendicularRight();

                    var start = next + normalNext * lineThickness;
                    var intersection = Ray.IntersectRayRay(start, -dirNext, p, -perp);
                    if (intersection.Valid)
                    {
                        outsideLeftCur = intersection.Point;
                    }
                    else //fallback bevel
                    {
                        outsideLeftCur = cur + normalNext * lineThickness;
                    }

                    if (!initialized)
                    {
                        insidePrev = insideCur;
                        outsidePrev = outsideLeftCur;
                        initialized = true;
                        continue;
                    }

                    start = prev + normalPrev * lineThickness;
                    intersection = Ray.IntersectRayRay(start, dirPrev, p, perp);
                    if (intersection.Valid)
                    {
                        outsideRightCur = intersection.Point;
                    }
                    else //fallback bevel
                    {
                        outsideRightCur = cur + normalPrev * lineThickness;
                    }
                }

                Raylib.DrawTriangle(outsidePrev, outsideRightCur, insideCur, rayColor);
                Raylib.DrawTriangle(outsidePrev, insideCur, insidePrev, rayColor);
                Raylib.DrawTriangle(outsideRightCur, outsideLeftCur, insideCur, rayColor);

                insidePrev = insideCur;
                outsidePrev = outsideLeftCur;
            }
        }
    }

    /// <summary>
    /// Draws the outline of a convex polygon using the specified <see cref="LineDrawingInfo"/>.
    /// This method is optimized for convex polygons and supports miter and beveled joins.
    /// </summary>
    /// <param name="poly">The convex polygon to draw. Must have at least 3 points.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <param name="miterLimit">
    /// The miter limit for joins. If the miter length exceeds this value times the line thickness, a bevel is used instead.
    /// Default is 2.0f.
    /// </param>
    /// <param name="beveled">
    /// If true, forces beveled joins instead of miters when the miter limit is exceeded.
    /// </param>
    public static void DrawLinesConvex(this Polygon poly, LineDrawingInfo lineInfo, float miterLimit = 2f, bool beveled = false)
    {
        if (poly.Count < 3) return;
        poly.DrawLinesConvex(lineInfo.Thickness, lineInfo.Color, miterLimit, beveled);
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
        lineInfo = lineInfo.ChangeColor(lineInfo.Color.SetAlpha(255));
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
        lineInfo = lineInfo.ChangeColor(lineInfo.Color.SetAlpha(255));
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
}