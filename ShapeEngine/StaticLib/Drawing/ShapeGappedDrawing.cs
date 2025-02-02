using System.Numerics;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.StaticLib.Drawing;

public static  class ShapeGappedDrawing
{
    
    /// <summary>
    /// Draws a line that is interrupted by gaps specified by the parameters.
    /// 1 gap with 0.5 gap percentage would result in half of the line visible and the other not visible.
    /// </summary>
    /// <param name="start">The start of the line.</param>
    /// <param name="end">The end of the line.</param>
    /// <param name="length">The length of the line. If zero or negative the function will calculate the length and return it. </param>
    /// <param name="lineInfo">The parameters for how to draw the line.</param>
    /// <param name="gapDrawingInfo">Info for how to draw the gaps.</param>
    /// <returns>Returns the length of the line if positive otherwise -1. </returns>
    public static float DrawGappedSegment(Vector2 start, Vector2 end, float length, LineDrawingInfo lineInfo, GappedOutlineDrawingInfo gapDrawingInfo)
    {
        if (gapDrawingInfo.Gaps <= 0 || gapDrawingInfo.GapPerimeterPercentage <= 0f)
        {
            ShapeSegmentDrawing.DrawSegment(start, end, lineInfo);
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
            ShapeSegmentDrawing.DrawSegment(curStart, curEnd, lineInfo);

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
    /// Draws an outline that is interrupted by gaps specified by the parameters.
    /// 1 gap with 0.5 gap percentage would result in half of the outline visible and the other not visible.
    /// </summary>
    /// <param name="shapePoints">The points for the outline.</param>
    /// <param name="lineInfo">The parameters for how to draw the line.</param>
    /// <param name="perimeter">The total length of the perimeter. If less than 0 the functions calculates this (more expensive).</param>
    /// <param name="gapDrawingInfo">Info for how to draw the gaps.</param>
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
                        ShapeSegmentDrawing.DrawSegment(points[0], points[1], lineInfo);
                    }
                    else
                    {
                        for (var i = 0; i < points.Count - 1; i++)
                        {
                            var p1 = points[i];
                            var p2 = points[(i + 1) % points.Count];
                            ShapeSegmentDrawing.DrawSegment(p1, p2, lineInfo);
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
    /// Draws an outline that is interrupted by gaps specified by the parameters.
    /// 1 gap with 0.5 gap percentage would result in half of the outline visible and the other not visible.
    /// </summary>
    /// <param name="shapePoints">The points for the outline.</param>
    /// <param name="lineInfo">The parameters for how to draw the line.</param>
    /// <param name="perimeter">The total length of the perimeter. If less than 0 the functions calculates this (more expensive).</param>
    /// <param name="gapDrawingInfo">Info for how to draw the gaps.</param>
    public static float DrawGappedOutline(this Points shapePoints, float perimeter, LineDrawingInfo lineInfo, GappedOutlineDrawingInfo gapDrawingInfo)
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
                        ShapeSegmentDrawing.DrawSegment(points[0], points[1], lineInfo);
                    }
                    else
                    {
                        for (var i = 0; i < points.Count - 1; i++)
                        {
                            var p1 = points[i];
                            var p2 = points[(i + 1) % points.Count];
                            ShapeSegmentDrawing.DrawSegment(p1, p2, lineInfo);
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
    /// Draws a segment that is interrupted by gaps specified by the parameters.
    /// 1 gap with 0.5 gap percentage would result in half of the segment visible and the other not visible.
    /// </summary>
    /// <param name="s">The segment to draw.</param>
    /// <param name="length">The length of the segment. If zero or negative the function will calculate the length and return it. </param>
    /// <param name="lineInfo">The parameters for how to draw the segment.</param>
    /// <param name="gapDrawingInfo">Info for how to draw the gaps.</param>
    /// <returns>Returns the length of the segment if positive otherwise -1. </returns>
    public static float DrawGapped(this Segment s, float length, LineDrawingInfo lineInfo,
        GappedOutlineDrawingInfo gapDrawingInfo) => DrawGappedSegment(s.Start, s.End, length, lineInfo, gapDrawingInfo);
    
    
    /// <summary>
    /// Draws an outline that is interrupted by gaps specified by the parameters.
    /// 1 gap with 0.5 gap percentage would result in half of the outline visible and the other not visible.
    /// </summary>
    /// <param name="circle">The circle to use for drawing.</param>
    /// <param name="lineInfo">The parameters for how to draw the line.</param>
    /// <param name="gapDrawingInfo">Info for how to draw the gaps.</param>
    /// <param name="rotDeg">The rotation of the circle.</param>
    /// <param name="sides">With how many sides should the circle be drawn.</param>
    public static void DrawGappedOutline(this Circle circle, LineDrawingInfo lineInfo, GappedOutlineDrawingInfo gapDrawingInfo, float rotDeg, int sides = 18)
    {
        //const float sideLength = 4f;
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
        
        //int sides = GetCircleSideCount(circle.Radius, sideLength);
        
        var curIndex = 0;
        var curPoint = circlePoints[0]; //GetCirclePoint(circle, angleRad, angleStep, 0);
        var nextPoint= circlePoints[1]; //GetCirclePoint(circle, angleRad, angleStep, 1);;
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
                        ShapeSegmentDrawing.DrawSegment(points[0], points[1], lineInfo);
                    }
                    else
                    {
                        for (var i = 0; i < points.Count - 1; i++)
                        {
                            var p1 = points[i];
                            var p2 = points[(i + 1) % points.Count];
                            ShapeSegmentDrawing.DrawSegment(p1, p2, lineInfo);
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
                curPoint = circlePoints[curIndex]; //GetCirclePoint(circle, angleRad, angleStep, curIndex);
                nextPoint = circlePoints[(curIndex + 1) % sides]; //GetCirclePoint(circle, angleRad, angleStep, (curIndex + 1) % sides);
                curW = nextPoint - curPoint;
                curDis = curW.Length();
            }
            
        }

        return;
    }
   
    /// <summary>
    /// Draws an outline that is interrupted by gaps specified by the parameters.
    /// 1 gap with 0.5 gap percentage would result in half of the outline visible and the other not visible.
    /// </summary>
    /// <param name="center">The center of the ring.</param>
    /// <param name="innerRadius">The radius of the inner ring.</param>
    /// <param name="outerRadius">The radius of the outer ring.</param>
    /// <param name="lineInfo">The parameters for how to draw the line.</param>
    /// <param name="gapDrawingInfo">Info for how to draw the gaps.</param>
    /// <param name="rotDeg">The rotation of the circle.</param>
    /// <param name="sideLength">The side lengths of the circle.</param>
    public static void DrawGappedRing(Vector2 center, float innerRadius, float outerRadius, LineDrawingInfo lineInfo, GappedOutlineDrawingInfo gapDrawingInfo, float rotDeg, float sideLength = 8f)
    {
        if (innerRadius <= 0 && outerRadius <= 0) return;
        
        
        int outerSides = ShapeCircleDrawing.GetCircleSideCount(outerRadius, sideLength);
        if (innerRadius <= 0)
        {
            DrawGappedOutline(new Circle(center, outerRadius), lineInfo, gapDrawingInfo, rotDeg, outerSides);
            return;
        }

        int innerSides = ShapeCircleDrawing.GetCircleSideCount(innerRadius, sideLength);
        if (outerRadius <= 0)
        {
            DrawGappedOutline(new Circle(center, innerRadius), lineInfo, gapDrawingInfo, rotDeg, innerSides);
            return;
        }
        
        DrawGappedOutline(new Circle(center, innerRadius), lineInfo, gapDrawingInfo, rotDeg, innerSides);
        DrawGappedOutline(new Circle(center, outerRadius), lineInfo, gapDrawingInfo, rotDeg, outerSides);
    }
   
    
    /// <summary>
    /// Draws an outline that is interrupted by gaps specified by the parameters.
    /// 1 gap with 0.5 gap percentage would result in half of the outline visible and the other not visible.
    /// </summary>
    /// <param name="triangle">The triangle for drawing the outline.</param>
    /// <param name="lineInfo">The parameters for how to draw the line.</param>
    /// <param name="perimeter">The total length of the perimeter. If less than 0 the functions calculates this (more expensive).</param>
    /// <param name="gapDrawingInfo">Info for how to draw the gaps.</param>
    public static float DrawGappedOutline(this Triangle triangle, float perimeter, LineDrawingInfo lineInfo, GappedOutlineDrawingInfo gapDrawingInfo)
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
        
        //int sides = GetCircleSideCount(circle.Radius, sideLength);
        
        var curIndex = 0;
        var curPoint = shapePoints[0]; //GetCirclePoint(circle, angleRad, angleStep, 0);
        var nextPoint= shapePoints[1]; //GetCirclePoint(circle, angleRad, angleStep, 1);;
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
                        ShapeSegmentDrawing.DrawSegment(points[0], points[1], lineInfo);
                    }
                    else
                    {
                        for (var i = 0; i < points.Count - 1; i++)
                        {
                            var p1 = points[i];
                            var p2 = points[(i + 1) % points.Count];
                            ShapeSegmentDrawing.DrawSegment(p1, p2, lineInfo);
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
    /// Draws an outline that is interrupted by gaps specified by the parameters.
    /// 1 gap with 0.5 gap percentage would result in half of the outline visible and the other not visible.
    /// </summary>
    /// <param name="rect">The rect for drawing the outline.</param>
    /// <param name="lineInfo">The parameters for how to draw the line.</param>
    /// <param name="perimeter">The total length of the perimeter. If less than 0 the functions calculates this (more expensive).</param>
    /// <param name="gapDrawingInfo">Info for how to draw the gaps.</param>
    public static float DrawGappedOutline(this Rect rect, float perimeter, LineDrawingInfo lineInfo, GappedOutlineDrawingInfo gapDrawingInfo)
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
                        ShapeSegmentDrawing.DrawSegment(points[0], points[1], lineInfo);
                    }
                    else
                    {
                        for (var i = 0; i < points.Count - 1; i++)
                        {
                            var p1 = points[i];
                            var p2 = points[(i + 1) % points.Count];
                            ShapeSegmentDrawing.DrawSegment(p1, p2, lineInfo);
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
    /// Draws an outline that is interrupted by gaps specified by the parameters.
    /// 1 gap with 0.5 gap percentage would result in half of the outline visible and the other not visible.
    /// </summary>
    /// <param name="quad">The quad for drawing the outline.</param>
    /// <param name="lineInfo">The parameters for how to draw the line.</param>
    /// <param name="perimeter">The total length of the perimeter. If less than 0 the functions calculates this (more expensive).</param>
    /// <param name="gapDrawingInfo">Info for how to draw the gaps.</param>
    public static float DrawGappedOutline(this Quad quad, float perimeter, LineDrawingInfo lineInfo, GappedOutlineDrawingInfo gapDrawingInfo)
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
                        ShapeSegmentDrawing.DrawSegment(points[0], points[1], lineInfo);
                    }
                    else
                    {
                        for (var i = 0; i < points.Count - 1; i++)
                        {
                            var p1 = points[i];
                            var p2 = points[(i + 1) % points.Count];
                            ShapeSegmentDrawing.DrawSegment(p1, p2, lineInfo);
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
    /// Draws an outline that is interrupted by gaps specified by the parameters.
    /// 1 gap with 0.5 gap percentage would result in half of the outline visible and the other not visible.
    /// </summary>
    /// <param name="poly">The polygon outline.</param>
    /// <param name="lineInfo">The parameters for how to draw the line.</param>
    /// <param name="perimeter">The total length of the perimeter. If less than 0 the functions calculates this (more expensive).</param>
    /// <param name="gapDrawingInfo">Info for how to draw the gaps.</param>
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
                        ShapeSegmentDrawing.DrawSegment(points[0], points[1], lineInfo);
                    }
                    else
                    {
                        for (var i = 0; i < points.Count - 1; i++)
                        {
                            var p1 = points[i];
                            var p2 = points[(i + 1) % points.Count];
                            ShapeSegmentDrawing.DrawSegment(p1, p2, lineInfo);
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
    /// Draws an outline that is interrupted by gaps specified by the parameters.
    /// 1 gap with 0.5 gap percentage would result in half of the outline visible and the other not visible.
    /// </summary>
    /// <param name="polyline">The polyline for drawing the outline.</param>
    /// <param name="lineInfo">The parameters for how to draw the line.</param>
    /// <param name="perimeter">The total length of the perimeter. If less than 0 the functions calculates this (more expensive).</param>
    /// <param name="gapDrawingInfo">Info for how to draw the gaps.</param>
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
        // Console.WriteLine($"    > Gaps: {gapDrawingInfo.Gaps} | Start Offset: {(int)(gapDrawingInfo.StartOffset * 100)}% | Start Distance {startDistance}");
        // Console.WriteLine($"    > Gap percentage: {(int)(gapDrawingInfo.GapPerimeterPercentage * 100)}%");
        // Console.WriteLine($"    > Gap Range: {(int)(gapPercentageRange * 100)}% | Line Range: {(int)(nonGapPercentageRange * 100)}%");
        // Console.WriteLine($"    > Gap size: {gapPercentageRange * perimeter} | Line Size: {nonGapPercentageRange * perimeter}");
        // Console.WriteLine($"    > Perimeter {perimeter} | Cur Dis: {curDis} | Next Dis: {nextDistance}");
        
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
                    // Console.WriteLine($"        > First Point added | Point Count: {points.Count}");
                    // Console.WriteLine($"            > Next Distance changed from {prevDistance} to {nextDistance}");

                }
                else
                {
                    var prevDistance = nextDistance;
                    nextDistance += gapPercentageRange * perimeter;
                    points.Add(p);
                    // Console.WriteLine($"        > Point added | Point Count: {points.Count}");
                    // Console.WriteLine($"            > Next Distance changed from {prevDistance} to {nextDistance}");
                    
                    if (points.Count == 2)
                    {
                        ShapeSegmentDrawing.DrawSegment(points[0], points[1], lineInfo);
                    }
                    else
                    {
                        for (var i = 0; i < points.Count - 1; i++)
                        {
                            var p1 = points[i];
                            var p2 = points[(i + 1) % points.Count];
                            ShapeSegmentDrawing.DrawSegment(p1, p2, lineInfo);
                        }
                    }
                    
                    // Console.WriteLine($"        > While Counter reduced by one from {whileCounter} to {whileCounter - 1}");
                    points.Clear();
                    whileCounter--;
                }

            }
            else
            {
                // Console.WriteLine($"        > Cur Index: {curIndex} | Polyline Count: {polyline.Count} | Remaining: {whileCounter}");
                // Console.WriteLine($"        > Cur Distance: {curDistance} | Next Distance: {nextDistance} | Segment Length: {curDis}");
                if (curIndex >= polyline.Count - 2) //last point
                {
                    if (points.Count > 0)
                    {
                        points.Add(nextPoint);
                        if (points.Count == 2)
                        {
                            ShapeSegmentDrawing.DrawSegment(points[0], points[1], lineInfo);
                        }
                        else
                        {
                            for (var i = 0; i < points.Count - 1; i++)
                            {
                                var p1 = points[i];
                                var p2 = points[(i + 1) % points.Count];
                                ShapeSegmentDrawing.DrawSegment(p1, p2, lineInfo);
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
                    
                    // Console.WriteLine("             > End Point Reached");
                    // Console.WriteLine($"                > New Index: {curIndex} | Next Index: {(curIndex + 1) % polyline.Count}");
                    // Console.WriteLine($"                > Cur Distance: {curDistance} | Next Distance: {nextDistance} | Segment Length: {curDis}");
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
                    
                    // Console.WriteLine("             > Mid Point Reached");
                    // Console.WriteLine($"                > New Index: {curIndex} | Next Index: {(curIndex + 1) % polyline.Count}");
                    // Console.WriteLine($"                > Cur Distance: {curDistance} | Next Distance: {nextDistance} | Segment Length: {curDis}");
                }
            }
            
        }

        return perimeter;
    }
   
}