using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.ShapeClipper;
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
    //CHECK: Check if *Faster methods are even faster.
    
    
    #region Draw
    
    public static void Draw(this Polyline polyline, float thickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, float miterLimit = 2f, bool beveled = false)
    {
        ClipperImmediate2D.DrawPolyline(polyline, thickness, color, miterLimit, beveled, capType.ToShapeClipperEndType(), false);
    }

    public static void Draw(this Polyline polyline, LineDrawingInfo lineInfo, float miterLimit = 2f, bool beveled = false)
    {
        ClipperImmediate2D.DrawPolyline(polyline, lineInfo.Thickness, lineInfo.Color, miterLimit, beveled, lineInfo.CapType.ToShapeClipperEndType(), false);
    }
    
    
    public static void DrawFast(this Polyline polyline, float thickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (polyline.Count < 2) return;
        for (var i = 0; i < polyline.Count - 1; i++)
        {
            var start = polyline[i];
            var end = polyline[i + 1];
            SegmentDrawing.DrawSegment(start, end, thickness, color, capType, capPoints);
        }
    }
   
    public static void DrawFast(this Polyline polyline, LineDrawingInfo lineInfo)
    {
        if (polyline.Count < 2) return;
        for (var i = 0; i < polyline.Count - 1; i++)
        {
            var start = polyline[i];
            var end = polyline[i + 1];
            SegmentDrawing.DrawSegment(start, end, lineInfo);
        }
    }    
    
    #endregion
    
    #region Draw Perimeter
    
    public static void DrawPerimeter(this Polyline polyline, float perimeterToDraw, LineDrawingInfo lineInfo, float miterLimit = 2f, bool beveled = false)
    {
        ClipperImmediate2D.DrawPolylinePerimeter(polyline, perimeterToDraw, lineInfo.Thickness, lineInfo.Color, miterLimit, beveled, lineInfo.CapType.ToShapeClipperEndType(), false);
    }
    
    public static void DrawPerimeter(this Polyline polyline, float perimeterToDraw, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, float miterLimit = 2f, bool beveled = false)
    {
        ClipperImmediate2D.DrawPolylinePerimeter(polyline, perimeterToDraw, lineThickness, color, miterLimit, beveled, capType.ToShapeClipperEndType(), false);
    }
    
    
    public static void DrawPerimeterFast(this Polyline polyline, float perimeterToDraw, LineDrawingInfo lineInfo)
    {
        DrawPerimeterFast(polyline, perimeterToDraw, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
    }
    
    public static void DrawPerimeterFast(this Polyline polyline, float perimeterToDraw, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
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

    #endregion
    
    #region Draw Percentage
    
    public static void DrawPercentage(this Polyline polyline, float f, LineDrawingInfo lineInfo, float miterLimit = 2f, bool beveled = false)
    {
        ClipperImmediate2D.DrawPolylinePercentage(polyline, f, lineInfo.Thickness, lineInfo.Color, miterLimit, beveled, lineInfo.CapType.ToShapeClipperEndType(), false);
    }
    
    public static void DrawPercentage(this Polyline polyline, float f, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, float miterLimit = 2f, bool beveled = false)
    {
        ClipperImmediate2D.DrawPolylinePercentage(polyline, f, lineThickness, color, miterLimit, beveled, capType.ToShapeClipperEndType(), false);
    }

    
    
    public static void DrawPercentageFast(this Polyline polyline, float f, LineDrawingInfo lineInfo)
    {
        DrawPercentageFast(polyline, f, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
    }
    
    public static void DrawPercentageFast(this Polyline polyline, float f, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
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

    #endregion

    #region Draw Scaled
    
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
    
    #endregion


    #region Glow
    /// <summary>
    /// Draws the polyline as a layered glow by rendering multiple stroked passes
    /// with interpolated thickness and color values.
    /// </summary>
    /// <param name="polyline">The polyline to draw.</param>
    /// <param name="thicknessRange">The stroke thickness range used from the first pass to the final pass.</param>
    /// <param name="colorRange">The color range used from the first pass to the final pass.</param>
    /// <param name="steps">The number of glow passes to render. A value of 1 draws a single pass using the maximum thickness and color.</param>
    /// <param name="miterLimit">The maximum miter length factor used for sharp joins before beveling is applied.</param>
    /// <param name="beveled">If true, sharp joins are beveled instead of using extended miters.</param>
    /// <param name="capType">The cap style used for the open ends of the polyline.</param>
    /// <param name="useDelaunay">If true, applies Delaunay refinement when triangulating the stroke mesh.</param>
    /// <remarks>
    /// This is useful for soft outlines, energy trails, and other expanding stroke effects.
    /// </remarks>
    public static void DrawGlow(this Polyline polyline, ValueRange thicknessRange, ValueRangeColor colorRange, int steps, 
        float miterLimit = 2f, bool beveled = false,  LineCapType capType = LineCapType.CappedExtended, bool useDelaunay = false)
    {
        ClipperImmediate2D.DrawPolylineGlow(polyline, thicknessRange, colorRange, steps, miterLimit, beveled, capType.ToShapeClipperEndType(), useDelaunay);
    }
    
    #endregion
    
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

