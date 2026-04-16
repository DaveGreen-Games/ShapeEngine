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

namespace ShapeEngine.Geometry.PolylineDef;

/// <summary>
/// Provides drawing methods for <see cref="Polyline"/> values.
/// </summary>
public partial class Polyline
{
    #region Draw
    
    public void Draw(float thickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, float miterLimit = 2f, bool beveled = false)
    {
        ShapeClipperDrawing2D.DrawPolyline(this, thickness, color, miterLimit, beveled, capType.ToShapeClipperEndType());
    }

    public void Draw(LineDrawingInfo lineInfo, float miterLimit = 2f, bool beveled = false)
    {
        ShapeClipperDrawing2D.DrawPolyline(this, lineInfo.Thickness, lineInfo.Color, miterLimit, beveled, lineInfo.CapType.ToShapeClipperEndType());
    }
    
    #endregion
    
    #region Draw Perimeter
    
    public void DrawPerimeter(float perimeterToDraw, LineDrawingInfo lineInfo, float miterLimit = 2f, bool beveled = false)
    {
        ShapeClipperDrawing2D.DrawPolylinePerimeter(this, perimeterToDraw, lineInfo.Thickness, lineInfo.Color, miterLimit, beveled, lineInfo.CapType.ToShapeClipperEndType(), false);
    }
    
    public void DrawPerimeter(float perimeterToDraw, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, float miterLimit = 2f, bool beveled = false)
    {
        ShapeClipperDrawing2D.DrawPolylinePerimeter(this, perimeterToDraw, lineThickness, color, miterLimit, beveled, capType.ToShapeClipperEndType(), false);
    }
    #endregion
    
    #region Draw Percentage
    
    public void DrawPercentage(float f, LineDrawingInfo lineInfo, float miterLimit = 2f, bool beveled = false)
    {
        ShapeClipperDrawing2D.DrawPolylinePercentage(this, f, lineInfo.Thickness, lineInfo.Color, miterLimit, beveled, lineInfo.CapType.ToShapeClipperEndType(), false);
    }
    
    public void DrawPercentage(float f, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, float miterLimit = 2f, bool beveled = false)
    {
        ShapeClipperDrawing2D.DrawPolylinePercentage(this, f, lineThickness, color, miterLimit, beveled, capType.ToShapeClipperEndType(), false);
    }
    #endregion

    #region Draw Scaled
    
    /// <summary>
    /// Draws the polyline with each side scaled towards its origin, allowing for variable side lengths.
    /// </summary>
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
    public void DrawLinesScaled(LineDrawingInfo lineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (Count < 2) return;
        if (sideScaleFactor <= 0) return;

        if (sideScaleFactor >= 1)
        {
            Draw(lineInfo);
            return;
        }
        for (var i = 0; i < Count - 1; i++)
        {
            var start = this[i];
            var end = this[(i + 1) % Count];
            Segment.DrawSegment(start, end, lineInfo, sideScaleFactor, sideScaleOrigin);
        }
    }
    
    #endregion
    
    #region Glow
    /// <summary>
    /// Draws the polyline as a layered glow by rendering multiple stroked passes
    /// with interpolated thickness and color values.
    /// </summary>
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
    public void DrawGlow(ValueRange thicknessRange, ValueRangeColor colorRange, int steps, 
        float miterLimit = 2f, bool beveled = false,  LineCapType capType = LineCapType.CappedExtended, bool useDelaunay = false)
    {
        ShapeClipperDrawing2D.DrawPolylineGlow(this, thicknessRange, colorRange, steps, miterLimit, beveled, capType.ToShapeClipperEndType(), useDelaunay);
    }
    
    #endregion
    
    #region Draw Masked
    
    /// <summary>
    /// Draws each segment of the polyline using a triangular mask.
    /// </summary>
    /// <param name="mask">The <see cref="Triangle"/> used as the clipping mask.</param>
    /// <param name="lineInfo">Drawing parameters such as thickness, color and cap type.</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public void DrawMasked(Triangle mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        if (Count < 2) return;
        
        for (var i = 0; i < Count - 1; i++)
        {
            var start = this[i];
            var end = this[i + 1];
            var segment = new Segment(start, end);
            segment.DrawMasked(mask, lineInfo, reversedMask);
        }
    }
    
    /// <summary>
    /// Draws each segment of the polyline using a circular mask.
    /// </summary>
    /// <param name="mask">The <see cref="Circle"/> used as the clipping mask.</param>
    /// <param name="lineInfo">Drawing parameters such as thickness, color and cap type.</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public void DrawMasked(Circle mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        if (Count < 2) return;
        
        for (var i = 0; i < Count - 1; i++)
        {
            var start = this[i];
            var end = this[i + 1];
            var segment = new Segment(start, end);
            segment.DrawMasked(mask, lineInfo, reversedMask);
        }
    }
    
    /// <summary>
    /// Draws each segment of the polyline using a rectangular mask.
    /// </summary>
    /// <param name="mask">The <see cref="Rect"/> used as the clipping mask.</param>
    /// <param name="lineInfo">Drawing parameters such as thickness, color and cap type.</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public void DrawMasked(Rect mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        if (Count < 2) return;
        
        for (var i = 0; i < Count - 1; i++)
        {
            var start = this[i];
            var end = this[i + 1];
            var segment = new Segment(start, end);
            segment.DrawMasked(mask, lineInfo, reversedMask);
        }
    }
  
    /// <summary>
    /// Draws each segment of the polyline using a quadrilateral mask.
    /// </summary>
    /// <param name="mask">The <see cref="Quad"/> used as the clipping mask.</param>
    /// <param name="lineInfo">Drawing parameters such as thickness, color and cap type.</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public void DrawMasked(Quad mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        if (Count < 2) return;
        
        for (var i = 0; i < Count - 1; i++)
        {
            var start = this[i];
            var end = this[i + 1];
            var segment = new Segment(start, end);
            segment.DrawMasked(mask, lineInfo, reversedMask);
        }
    }
    
    /// <summary>
    /// Draws each segment of the polyline using a <see cref="Polygon"/> as the clipping mask.
    /// </summary>
    /// <param name="mask">The <see cref="Polygon"/> used as the clipping mask.</param>
    /// <param name="lineInfo">Drawing parameters such as thickness, color and cap type.</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public void DrawMasked(Polygon mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        if (Count < 2) return;
        
        for (var i = 0; i < Count - 1; i++)
        {
            var start = this[i];
            var end = this[i + 1];
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
    /// <param name="mask">The mask instance used for clipping each segment.</param>
    /// <param name="lineInfo">Drawing parameters such as thickness, color and cap type.</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public void DrawMasked<T>(T mask, LineDrawingInfo lineInfo, bool reversedMask = false) where T : IClosedShapeTypeProvider
    {
        if (Count < 2) return;
        
        for (var i = 0; i < Count - 1; i++)
        {
            var start = this[i];
            var end = this[i + 1];
            var segment = new Segment(start, end);
            segment.DrawMasked(mask, lineInfo, reversedMask);
        }
    }
    
    #endregion
    
    #region Gapped
    
    /// <summary>
    /// Draws a gapped outline for a polyline (open or closed), creating a dashed or segmented effect along the polyline's length.
    /// </summary>
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
    public float DrawGappedOutline(float perimeter, LineDrawingInfo lineInfo, GappedOutlineDrawingInfo gapDrawingInfo)
    {
        if (gapDrawingInfo.Gaps <= 0 || gapDrawingInfo.GapPerimeterPercentage <= 0f)
        {
            Draw(lineInfo);
            return perimeter > 0f ? perimeter : -1f;
        }

        if (gapDrawingInfo.GapPerimeterPercentage >= 1f) return perimeter > 0f ? perimeter : -1f;

        var nonGapPercentage = 1f - gapDrawingInfo.GapPerimeterPercentage;

        var gapPercentageRange = gapDrawingInfo.GapPerimeterPercentage / gapDrawingInfo.Gaps;
        var nonGapPercentageRange = nonGapPercentage / gapDrawingInfo.Gaps;

        if (perimeter <= 0f)
        {
            perimeter = GetLength();
        }

        var startDistance = perimeter * gapDrawingInfo.StartOffset;
        var curDistance = 0f;
        var nextDistance = startDistance;
        
        
        var curIndex = 0;
        var curPoint = this[0];
        var nextPoint= this[1];
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
                        Segment.DrawSegment(points[0], points[1], lineInfo);
                    }
                    else
                    {
                        for (var i = 0; i < points.Count - 1; i++)
                        {
                            var p1 = points[i];
                            var p2 = points[(i + 1) % points.Count];
                            Segment.DrawSegment(p1, p2, lineInfo);
                        }
                    }
                    points.Clear();
                    whileCounter--;
                }

            }
            else
            {
                if (curIndex >= Count - 2) //last point
                {
                    if (points.Count > 0)
                    {
                        points.Add(nextPoint);
                        if (points.Count == 2)
                        {
                            Segment.DrawSegment(points[0], points[1], lineInfo);
                        }
                        else
                        {
                            for (var i = 0; i < points.Count - 1; i++)
                            {
                                var p1 = points[i];
                                var p2 = points[(i + 1) % points.Count];
                                Segment.DrawSegment(p1, p2, lineInfo);
                            }
                        }
                        points.Clear();
                        points.Add(this[0]);
                    }
                    
                    curDistance += curDis;
                    curIndex = 0;
                    curPoint = this[curIndex];
                    nextPoint = this[(curIndex + 1) % Count];
                    curW = nextPoint - curPoint;
                    curDis = curW.Length();
                }
                else
                {
                    if(points.Count > 0) points.Add(nextPoint);

                    curDistance += curDis;
                    curIndex += 1;// (curIndex + 1) % Count;
                    curPoint = this[curIndex];
                    nextPoint = this[(curIndex + 1) % Count];
                    curW = nextPoint - curPoint;
                    curDis = curW.Length();
                }
            }
            
        }

        return perimeter;
    }
    
    #endregion
}

