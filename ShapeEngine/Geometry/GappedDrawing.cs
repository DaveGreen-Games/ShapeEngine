using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.Circle;
using ShapeEngine.Geometry.Points;
using ShapeEngine.Geometry.Polygon;
using ShapeEngine.Geometry.Polyline;
using ShapeEngine.Geometry.Quad;
using ShapeEngine.Geometry.Rect;
using ShapeEngine.Geometry.Segment;
using ShapeEngine.Geometry.Triangle;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry;

/// <summary>
/// Provides static methods for drawing shapes and outlines with configurable gaps (dashed or segmented effects).
/// These methods allow for drawing lines, outlines, and various shapes with gaps, based on the specified parameters.
/// </summary>
/// <remarks>
/// All methods in this class are static and intended for rendering shapes with customizable gaps.
/// Useful for visual effects such as dashed lines, segmented outlines, or highlighting.
/// </remarks>
public static class GappedDrawing
{
    /// <summary>
    /// Draws a line segment with gaps, creating a dashed or segmented effect.
    /// </summary>
    /// <param name="start">The starting point of the line segment.</param>
    /// <param name="end">The ending point of the line segment.</param>
    /// <param name="length">
    /// The length of the line.
    /// If zero or negative, the method calculates it automatically.
    /// Providing a known length avoids redundant calculations and improves performance, especially for static segments.</param>
    /// <param name="lineInfo">Parameters describing how to draw the line (e.g., color, thickness).</param>
    /// <param name="gapDrawingInfo">Parameters describing the gap configuration (number of gaps, gap percentage, etc.).</param>
    /// <returns>
    /// The length of the line if positive; otherwise, -1.
    /// If the shape does not change, the valid length can be reused in subsequent frames to avoid recalculating.
    /// </returns>
    /// <remarks>
    /// - If <paramref name="gapDrawingInfo.Gaps"/> is 0 or <paramref name="gapDrawingInfo.GapPerimeterPercentage"/> is 0, the line is drawn solid.
    /// - If <paramref name="gapDrawingInfo.GapPerimeterPercentage"/> is 1 or greater, no line is drawn.
    /// </remarks>
    public static float DrawGappedSegment(Vector2 start, Vector2 end, float length, LineDrawingInfo lineInfo, GappedOutlineDrawingInfo gapDrawingInfo)
    {
        if (gapDrawingInfo.Gaps <= 0 || gapDrawingInfo.GapPerimeterPercentage <= 0f)
        {
            SegmentDrawing.DrawSegment(start, end, lineInfo);
            return length > 0f ? length : -1f;
        }

        if (gapDrawingInfo.GapPerimeterPercentage >= 1f) return length > 0f ? length : -1f;

        var linePercentage = 1f - gapDrawingInfo.GapPerimeterPercentage;
        var lines = gapDrawingInfo.Gaps;
        var gapPercentageRange = gapDrawingInfo.GapPerimeterPercentage / gapDrawingInfo.Gaps;
        var linePercentageRange = linePercentage / lines;

        var w = end - start;
        if (length <= 0) length = w.Length();
        if (length <= 0) return -1f;
        var dir = w / length;

        var lineLength = length * linePercentageRange;
        var gapLength = length * gapPercentageRange;

        var curDistance = gapDrawingInfo.StartOffset < 1f ? gapDrawingInfo.StartOffset * length : 0f;
        var curStart = start + dir * curDistance;
        Vector2 curEnd;
        var remainingLineLength = 0f;
        if (curDistance + lineLength >= length)
        {
            curEnd = end;
            var tempLength = (curEnd - curStart).Length();
            curDistance = 0;
            remainingLineLength = lineLength - tempLength;
        }
        else
        {
            curEnd = curStart + dir * lineLength;
            curDistance += lineLength;
        }

        int drawnLines = 0;
        while (drawnLines <= lines)
        {
            SegmentDrawing.DrawSegment(curStart, curEnd, lineInfo);

            if (remainingLineLength > 0f)
            {
                curStart = start;
                curEnd = curStart + dir * remainingLineLength;
                curDistance = remainingLineLength;
                remainingLineLength = 0f;
                drawnLines++;
            }
            else
            {
                if (curDistance + gapLength >= length) //gap overshoots end
                {
                    var tempLength = (end - curEnd).Length();
                    var remaining = gapLength - tempLength;
                    curDistance = remaining;

                    curStart = start + dir * curDistance;
                }
                else //advance gap length to find new start
                {
                    curStart = curEnd + dir * gapLength;
                    curDistance += gapLength;
                }

                if (curDistance + lineLength >= length) //line overshoots end
                {
                    curEnd = end;
                    var tempLength = (curEnd - curStart).Length();
                    curDistance = 0;
                    remainingLineLength = lineLength - tempLength;
                }
                else //advance line length to find new end
                {
                    curEnd = curStart + dir * lineLength;
                    curDistance += lineLength;
                    drawnLines++;
                }
            }

        }

        return length;
    }

    /// <summary>
    /// Draws an outline (closed polyline) with gaps, creating a dashed or segmented effect.
    /// </summary>
    /// <param name="shapePoints">The list of points defining the outline.</param>
    /// <param name="perimeter">
    /// The total length of the outline.
    /// If zero or negative, the method calculates it automatically.
    /// Providing a known length avoids redundant calculations and improves performance, especially for static segments.
    /// </param>
    /// <param name="lineInfo">Parameters describing how to draw the outline.</param>
    /// <param name="gapDrawingInfo">Parameters describing the gap configuration.</param>
    /// <returns>
    /// The perimeter of the outline if positive; otherwise, -1.
    /// If the shape does not change, the valid length can be reused in subsequent frames to avoid recalculating.
    /// </returns>
    /// <remarks>
    /// - If <paramref name="gapDrawingInfo.Gaps"/> is 0 or <paramref name="gapDrawingInfo.GapPerimeterPercentage"/> is 0, the outline is drawn solid.
    /// - If <paramref name="gapDrawingInfo.GapPerimeterPercentage"/> is 1 or greater, no outline is drawn.
    /// </remarks>
    public static float DrawGappedOutline(this List<Vector2> shapePoints, float perimeter, LineDrawingInfo lineInfo, GappedOutlineDrawingInfo gapDrawingInfo)
    {
        if (gapDrawingInfo.Gaps <= 0 || gapDrawingInfo.GapPerimeterPercentage <= 0f)
        {
            shapePoints.DrawOutline(lineInfo);
            return perimeter > 0f ? perimeter : -1f;
        }

        if (gapDrawingInfo.GapPerimeterPercentage >= 1f) return perimeter > 0f ? perimeter : -1f;

        var nonGapPercentage = 1f - gapDrawingInfo.GapPerimeterPercentage;

        var gapPercentageRange = gapDrawingInfo.GapPerimeterPercentage / gapDrawingInfo.Gaps;
        var nonGapPercentageRange = nonGapPercentage / gapDrawingInfo.Gaps;

        if (perimeter <= 0f)
        {
            for (int i = 0; i < shapePoints.Count; i++)
            {
                var curP = shapePoints[i];
                var nextP = shapePoints[(i + 1) % shapePoints.Count];

                perimeter += (nextP - curP).Length();
            }
        }

        var startDistance = perimeter * gapDrawingInfo.StartOffset;
        var curDistance = 0f;
        var nextDistance = startDistance;
        
        
        var curIndex = 0;
        var curPoint = shapePoints[0];
        var nextPoint= shapePoints[1 % shapePoints.Count];
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
                curIndex = (curIndex + 1) % shapePoints.Count;
                curPoint = shapePoints[curIndex];
                nextPoint = shapePoints[(curIndex + 1) % shapePoints.Count];
                curW = nextPoint - curPoint;
                curDis = curW.Length();
            }
            
        }

        return perimeter;
    }

    /// <summary>
    /// Draws an outline (closed polyline) with gaps for a <see cref="Points"/> collection.
    /// </summary>
    /// <param name="shapePoints">The points defining the outline.</param>
    /// <param name="perimeter">
    /// The total length of the outline.
    /// If zero or negative, the method calculates it automatically.
    /// Providing a known length avoids redundant calculations and improves performance, especially for static segments.
    /// </param>
    /// <param name="lineInfo">Parameters describing how to draw the outline.</param>
    /// <param name="gapDrawingInfo">Parameters describing the gap configuration.</param>
    /// <returns>
    /// The perimeter of the outline if positive; otherwise, -1.
    /// If the shape does not change, the valid length can be reused in subsequent frames to avoid recalculating.
    /// </returns>
    /// <remarks>
    /// - If <paramref name="gapDrawingInfo.Gaps"/> is 0 or <paramref name="gapDrawingInfo.GapPerimeterPercentage"/> is 0, the outline is drawn solid.
    /// - If <paramref name="gapDrawingInfo.GapPerimeterPercentage"/> is 1 or greater, no outline is drawn.
    /// </remarks>
    public static float DrawGappedOutline(this Points.Points shapePoints, float perimeter, LineDrawingInfo lineInfo, GappedOutlineDrawingInfo gapDrawingInfo)
    {
        if (gapDrawingInfo.Gaps <= 0 || gapDrawingInfo.GapPerimeterPercentage <= 0f)
        {
            shapePoints.DrawOutline(lineInfo);
            return perimeter > 0f ? perimeter : -1f;
        }

        if (gapDrawingInfo.GapPerimeterPercentage >= 1f) return perimeter > 0f ? perimeter : -1f;

        var nonGapPercentage = 1f - gapDrawingInfo.GapPerimeterPercentage;

        var gapPercentageRange = gapDrawingInfo.GapPerimeterPercentage / gapDrawingInfo.Gaps;
        var nonGapPercentageRange = nonGapPercentage / gapDrawingInfo.Gaps;

        if (perimeter <= 0f)
        {
            for (int i = 0; i < shapePoints.Count; i++)
            {
                var curP = shapePoints[i];
                var nextP = shapePoints[(i + 1) % shapePoints.Count];

                perimeter += (nextP - curP).Length();
            }
        }

        var startDistance = perimeter * gapDrawingInfo.StartOffset;
        var curDistance = 0f;
        var nextDistance = startDistance;
        
        
        var curIndex = 0;
        var curPoint = shapePoints[0];
        var nextPoint= shapePoints[1 % shapePoints.Count];
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
                curIndex = (curIndex + 1) % shapePoints.Count;
                curPoint = shapePoints[curIndex];
                nextPoint = shapePoints[(curIndex + 1) % shapePoints.Count];
                curW = nextPoint - curPoint;
                curDis = curW.Length();
            }
            
        }

        return perimeter;
    }
   
    
    /// <summary>
    /// Draws a segment with gaps, creating a dashed or segmented effect.
    /// </summary>
    /// <param name="s">The segment to draw.</param>
    /// <param name="length">
    /// The length of the segment.
    /// If zero or negative, the method calculates it automatically.
    /// Providing a known length avoids redundant calculations and improves performance, especially for static segments.
    /// </param>
    /// <param name="lineInfo">Parameters describing how to draw the segment.</param>
    /// <param name="gapDrawingInfo">Parameters describing the gap configuration.</param>
    /// <returns>
    /// Returns the segment length if positive; otherwise, returns -1.
    /// If the shape does not change, the valid length can be reused in subsequent frames to avoid recalculating.
    /// </returns>
    /// <remarks>
    /// This is a convenience wrapper for <see cref="DrawGappedSegment"/>.
    /// </remarks>
    public static float DrawGapped(this Segment.Segment s, float length, LineDrawingInfo lineInfo,
        GappedOutlineDrawingInfo gapDrawingInfo) => DrawGappedSegment(s.Start, s.End, length, lineInfo, gapDrawingInfo);

    /// <summary>
    /// Draws a gapped outline for a circle, creating a dashed or segmented circular outline.
    /// </summary>
    /// <param name="circle">The circle to draw.</param>
    /// <param name="lineInfo">Parameters describing how to draw the outline.</param>
    /// <param name="gapDrawingInfo">Parameters describing the gap configuration.</param>
    /// <param name="rotDeg">The rotation of the circle in degrees.</param>
    /// <param name="sides">The number of sides to approximate the circle (minimum 3).</param>
    /// <remarks>
    /// - If <paramref name="gapDrawingInfo.Gaps"/> is 0 or <paramref name="gapDrawingInfo.GapPerimeterPercentage"/> is 0, the outline is drawn solid.
    /// - If <paramref name="gapDrawingInfo.GapPerimeterPercentage"/> is 1 or greater, no outline is drawn.
    /// - The <paramref name="sides"/> parameter controls the smoothness of the circle.
    /// </remarks>
    public static void DrawGappedOutline(this Circle.Circle circle, LineDrawingInfo lineInfo, GappedOutlineDrawingInfo gapDrawingInfo, float rotDeg, int sides = 18)
    {
        if (sides < 3) return;
        if (gapDrawingInfo.Gaps <= 0 || gapDrawingInfo.GapPerimeterPercentage <= 0f)
        {
            circle.DrawLines(lineInfo, rotDeg, sides);
            return;
        }

        if (gapDrawingInfo.GapPerimeterPercentage >= 1f) return;

        var nonGapPercentage = 1f - gapDrawingInfo.GapPerimeterPercentage;

        var gapPercentageRange = gapDrawingInfo.GapPerimeterPercentage / gapDrawingInfo.Gaps;
        var nonGapPercentageRange = nonGapPercentage / gapDrawingInfo.Gaps;
        
        float angleStep = (MathF.PI * 2) / sides;
        float angleRad = rotDeg * ShapeMath.DEGTORAD;
        Vector2[] circlePoints = new Vector2[sides];
        
        float circumference = 0f;
        for (int i = 0; i < sides; i++)
        {
            var curP = circle.GetVertex(angleRad, angleStep, i);
            circlePoints[i] = curP;
            var nextP = circle.GetVertex(angleRad, angleStep, (i + 1) % sides); 
            circumference += (nextP - curP).Length();
        }

        var startDistance = circumference * gapDrawingInfo.StartOffset;
        var curDistance = 0f;
        var nextDistance = startDistance;
        
        var curIndex = 0;
        var curPoint = circlePoints[0];
        var nextPoint= circlePoints[1];
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
                    nextDistance += nonGapPercentageRange * circumference;
                    points.Add(p);

                }
                else
                {
                    nextDistance += gapPercentageRange * circumference;
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
                curIndex = (curIndex + 1) % sides;
                curPoint = circlePoints[curIndex];
                nextPoint = circlePoints[(curIndex + 1) % sides];
                curW = nextPoint - curPoint;
                curDis = curW.Length();
            }
            
        }
    }
   
    /// <summary>
    /// Draws a gapped outline for a ring (annulus), creating dashed or segmented outlines for both inner and outer circles.
    /// </summary>
    /// <param name="center">The center of the ring.</param>
    /// <param name="innerRadius">The radius of the inner circle. If zero or negative, only the outer circle is drawn.</param>
    /// <param name="outerRadius">The radius of the outer circle. If zero or negative, only the inner circle is drawn.</param>
    /// <param name="lineInfo">Parameters describing how to draw the outlines.</param>
    /// <param name="gapDrawingInfo">Parameters describing the gap configuration.</param>
    /// <param name="rotDeg">The rotation of the ring in degrees.</param>
    /// <param name="sideLength">The approximate length of each side used to approximate the circles.</param>
    /// <remarks>
    /// - If both radii are zero or negative, nothing is drawn.
    /// - The number of sides for each circle is determined by the radius and <paramref name="sideLength"/>.
    /// </remarks>
    public static void DrawGappedRing(Vector2 center, float innerRadius, float outerRadius, LineDrawingInfo lineInfo, GappedOutlineDrawingInfo gapDrawingInfo, float rotDeg, float sideLength = 8f)
    {
        if (innerRadius <= 0 && outerRadius <= 0) return;
        
        
        int outerSides = CircleDrawing.GetCircleSideCount(outerRadius, sideLength);
        if (innerRadius <= 0)
        {
            DrawGappedOutline(new Circle.Circle(center, outerRadius), lineInfo, gapDrawingInfo, rotDeg, outerSides);
            return;
        }

        int innerSides = CircleDrawing.GetCircleSideCount(innerRadius, sideLength);
        if (outerRadius <= 0)
        {
            DrawGappedOutline(new Circle.Circle(center, innerRadius), lineInfo, gapDrawingInfo, rotDeg, innerSides);
            return;
        }
        
        DrawGappedOutline(new Circle.Circle(center, innerRadius), lineInfo, gapDrawingInfo, rotDeg, innerSides);
        DrawGappedOutline(new Circle.Circle(center, outerRadius), lineInfo, gapDrawingInfo, rotDeg, outerSides);
    }
   
    
    /// <summary>
    /// Draws a gapped outline for a triangle, creating a dashed or segmented effect along the triangle's perimeter.
    /// </summary>
    /// <param name="triangle">The triangle to draw.</param>
    /// <param name="perimeter">
    /// The total length of the triangle's perimeter.
    /// If zero or negative, the method calculates it automatically.
    /// Providing a known length avoids redundant calculations and improves performance, especially for static segments.
    /// </param>
    /// <param name="lineInfo">Parameters describing how to draw the outline.</param>
    /// <param name="gapDrawingInfo">Parameters describing the gap configuration.</param>
    /// <returns>
    /// The perimeter of the triangle if positive; otherwise, -1.
    /// If the shape does not change, the valid length can be reused in subsequent frames to avoid recalculating.
    /// </returns>
    /// <remarks>
    /// - If <paramref name="gapDrawingInfo.Gaps"/> is 0 or <paramref name="gapDrawingInfo.GapPerimeterPercentage"/> is 0, the outline is drawn solid.
    /// - If <paramref name="gapDrawingInfo.GapPerimeterPercentage"/> is 1 or greater, no outline is drawn.
    /// </remarks>
    public static float DrawGappedOutline(this Triangle.Triangle triangle, float perimeter, LineDrawingInfo lineInfo, GappedOutlineDrawingInfo gapDrawingInfo)
    {
        if (gapDrawingInfo.Gaps <= 0 || gapDrawingInfo.GapPerimeterPercentage <= 0f)
        {
            triangle.DrawLines(lineInfo);
            return perimeter > 0f ? perimeter : -1f;
        }

        if (gapDrawingInfo.GapPerimeterPercentage >= 1f) return perimeter > 0f ? perimeter : -1f;

        var nonGapPercentage = 1f - gapDrawingInfo.GapPerimeterPercentage;

        var gapPercentageRange = gapDrawingInfo.GapPerimeterPercentage / gapDrawingInfo.Gaps;
        var nonGapPercentageRange = nonGapPercentage / gapDrawingInfo.Gaps;

        
        var shapePoints = new[] {triangle.A, triangle.B, triangle.C};
        int sides = shapePoints.Length;

        if (perimeter <= 0f)
        {
            perimeter = 0f;
            for (int i = 0; i < sides; i++)
            {
                var curP = shapePoints[i];
                var nextP = shapePoints[(i + 1) % sides];
                perimeter += (nextP - curP).Length();
            }
        }
        

        var startDistance = perimeter * gapDrawingInfo.StartOffset;
        var curDistance = 0f;
        var nextDistance = startDistance;
        
        var curIndex = 0;
        var curPoint = shapePoints[0];
        var nextPoint= shapePoints[1];
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
                curIndex = (curIndex + 1) % sides;
                curPoint = shapePoints[curIndex];
                nextPoint = shapePoints[(curIndex + 1) % sides];
                curW = nextPoint - curPoint;
                curDis = curW.Length();
            }
            
        }

        return perimeter;
    }
   
    /// <summary>
    /// Draws a gapped outline for a rectangle, creating a dashed or segmented effect along the rectangle's perimeter.
    /// </summary>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="perimeter">
    /// The total length of the rectangle's perimeter.
    /// If zero or negative, the method calculates it automatically.
    /// Providing a known length avoids redundant calculations and improves performance, especially for static segments.
    /// </param>
    /// <param name="lineInfo">Parameters describing how to draw the outline.</param>
    /// <param name="gapDrawingInfo">Parameters describing the gap configuration.</param>
    /// <returns>
    /// The perimeter of the rectangle if positive; otherwise, -1.
    /// If the shape does not change, the valid length can be reused in subsequent frames to avoid recalculating.
    /// </returns>
    /// <remarks>
    /// - If <paramref name="gapDrawingInfo.Gaps"/> is 0 or <paramref name="gapDrawingInfo.GapPerimeterPercentage"/> is 0, the outline is drawn solid.
    /// - If <paramref name="gapDrawingInfo.GapPerimeterPercentage"/> is 1 or greater, no outline is drawn.
    /// </remarks>
    public static float DrawGappedOutline(this Rect.Rect rect, float perimeter, LineDrawingInfo lineInfo, GappedOutlineDrawingInfo gapDrawingInfo)
    {
        if (gapDrawingInfo.Gaps <= 0 || gapDrawingInfo.GapPerimeterPercentage <= 0f)
        {
            rect.DrawLines(lineInfo);
            return perimeter > 0f ? perimeter : -1f;
        }

        if (gapDrawingInfo.GapPerimeterPercentage >= 1f) return perimeter > 0f ? perimeter : -1f;

        var nonGapPercentage = 1f - gapDrawingInfo.GapPerimeterPercentage;

        var gapPercentageRange = gapDrawingInfo.GapPerimeterPercentage / gapDrawingInfo.Gaps;
        var nonGapPercentageRange = nonGapPercentage / gapDrawingInfo.Gaps;

        
        var shapePoints = new[] {rect.A, rect.B, rect.C, rect.D};
        int sides = shapePoints.Length;

        if (perimeter <= 0f)
        {
            perimeter = 0f;
            for (int i = 0; i < sides; i++)
            {
                var curP = shapePoints[i];
                var nextP = shapePoints[(i + 1) % sides];
                perimeter += (nextP - curP).Length();
            }
        }

        var startDistance = perimeter * gapDrawingInfo.StartOffset;
        var curDistance = 0f;
        var nextDistance = startDistance;
        
        var curIndex = 0;
        var curPoint = shapePoints[0];
        var nextPoint= shapePoints[1];
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
                curIndex = (curIndex + 1) % sides;
                curPoint = shapePoints[curIndex];
                nextPoint = shapePoints[(curIndex + 1) % sides];
                curW = nextPoint - curPoint;
                curDis = curW.Length();
            }
            
        }

        return perimeter;
    }
   
    /// <summary>
    /// Draws a gapped outline for a quadrilateral, creating a dashed or segmented effect along the quad's perimeter.
    /// </summary>
    /// <param name="quad">The quadrilateral to draw.</param>
    /// <param name="perimeter">
    /// The total length of the quad's perimeter.
    /// If zero or negative, the method calculates it automatically.
    /// Providing a known length avoids redundant calculations and improves performance, especially for static segments.
    /// </param>
    /// <param name="lineInfo">Parameters describing how to draw the outline.</param>
    /// <param name="gapDrawingInfo">Parameters describing the gap configuration.</param>
    /// <returns>
    /// The perimeter of the quad if positive; otherwise, -1.
    /// If the shape does not change, the valid length can be reused in subsequent frames to avoid recalculating.
    /// </returns>
    /// <remarks>
    /// - If <paramref name="gapDrawingInfo.Gaps"/> is 0 or <paramref name="gapDrawingInfo.GapPerimeterPercentage"/> is 0, the outline is drawn solid.
    /// - If <paramref name="gapDrawingInfo.GapPerimeterPercentage"/> is 1 or greater, no outline is drawn.
    /// </remarks>
    public static float DrawGappedOutline(this Quad.Quad quad, float perimeter, LineDrawingInfo lineInfo, GappedOutlineDrawingInfo gapDrawingInfo)
    {
        if (gapDrawingInfo.Gaps <= 0 || gapDrawingInfo.GapPerimeterPercentage <= 0f)
        {
            quad.DrawLines(lineInfo);
            return perimeter > 0f ? perimeter : -1f;
        }

        if (gapDrawingInfo.GapPerimeterPercentage >= 1f) return perimeter > 0f ? perimeter : -1f;

        var nonGapPercentage = 1f - gapDrawingInfo.GapPerimeterPercentage;

        var gapPercentageRange = gapDrawingInfo.GapPerimeterPercentage / gapDrawingInfo.Gaps;
        var nonGapPercentageRange = nonGapPercentage / gapDrawingInfo.Gaps;

        
        var shapePoints = new[] {quad.A, quad.B, quad.C, quad.D};
        int sides = shapePoints.Length;

        if (perimeter <= 0f)
        {
            perimeter = 0f;
            for (int i = 0; i < sides; i++)
            {
                var curP = shapePoints[i];
                var nextP = shapePoints[(i + 1) % sides];
                perimeter += (nextP - curP).Length();
            }
        }
        

        var startDistance = perimeter * gapDrawingInfo.StartOffset;
        var curDistance = 0f;
        var nextDistance = startDistance;
        
        var curIndex = 0;
        var curPoint = shapePoints[0]; 
        var nextPoint= shapePoints[1]; 
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
                curIndex = (curIndex + 1) % sides;
                curPoint = shapePoints[curIndex];
                nextPoint = shapePoints[(curIndex + 1) % sides];
                curW = nextPoint - curPoint;
                curDis = curW.Length();
            }
            
        }

        return perimeter;
    }
   
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
    public static float DrawGappedOutline(this Polygon.Polygon poly, float perimeter, LineDrawingInfo lineInfo, GappedOutlineDrawingInfo gapDrawingInfo)
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
    public static float DrawGappedOutline(this Polyline.Polyline polyline, float perimeter, LineDrawingInfo lineInfo, GappedOutlineDrawingInfo gapDrawingInfo)
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
                    var prevDistance = nextDistance;
                    nextDistance += nonGapPercentageRange * perimeter;
                    points.Add(p);

                }
                else
                {
                    var prevDistance = nextDistance;
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
   
}