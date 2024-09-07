
using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Random;

namespace ShapeEngine.Lib;

public static class ShapeDrawing
{
    
    #region Gapped
    
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
    public static float DrawGappedLine(Vector2 start, Vector2 end, float length, LineDrawingInfo lineInfo, GappedOutlineDrawingInfo gapDrawingInfo)
    {
        if (gapDrawingInfo.Gaps <= 0 || gapDrawingInfo.GapPerimeterPercentage <= 0f)
        {
            DrawLine(start, end, lineInfo);
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
            DrawLine(curStart, curEnd, lineInfo);

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
                        DrawLine(points[0], points[1], lineInfo);
                    }
                    else
                    {
                        for (var i = 0; i < points.Count - 1; i++)
                        {
                            var p1 = points[i];
                            var p2 = points[(i + 1) % points.Count];
                            DrawLine(p1, p2, lineInfo);
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
                        DrawLine(points[0], points[1], lineInfo);
                    }
                    else
                    {
                        for (var i = 0; i < points.Count - 1; i++)
                        {
                            var p1 = points[i];
                            var p2 = points[(i + 1) % points.Count];
                            DrawLine(p1, p2, lineInfo);
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
        GappedOutlineDrawingInfo gapDrawingInfo) => DrawGappedLine(s.Start, s.End, length, lineInfo, gapDrawingInfo);
    
    
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
                        DrawLine(points[0], points[1], lineInfo);
                    }
                    else
                    {
                        for (var i = 0; i < points.Count - 1; i++)
                        {
                            var p1 = points[i];
                            var p2 = points[(i + 1) % points.Count];
                            DrawLine(p1, p2, lineInfo);
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
        
        
        int outerSides = GetCircleSideCount(outerRadius, sideLength);
        if (innerRadius <= 0)
        {
            DrawGappedOutline(new Circle(center, outerRadius), lineInfo, gapDrawingInfo, rotDeg, outerSides);
            return;
        }

        int innerSides = GetCircleSideCount(innerRadius, sideLength);
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
                        DrawLine(points[0], points[1], lineInfo);
                    }
                    else
                    {
                        for (var i = 0; i < points.Count - 1; i++)
                        {
                            var p1 = points[i];
                            var p2 = points[(i + 1) % points.Count];
                            DrawLine(p1, p2, lineInfo);
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
                        DrawLine(points[0], points[1], lineInfo);
                    }
                    else
                    {
                        for (var i = 0; i < points.Count - 1; i++)
                        {
                            var p1 = points[i];
                            var p2 = points[(i + 1) % points.Count];
                            DrawLine(p1, p2, lineInfo);
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
                        DrawLine(points[0], points[1], lineInfo);
                    }
                    else
                    {
                        for (var i = 0; i < points.Count - 1; i++)
                        {
                            var p1 = points[i];
                            var p2 = points[(i + 1) % points.Count];
                            DrawLine(p1, p2, lineInfo);
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
                        DrawLine(points[0], points[1], lineInfo);
                    }
                    else
                    {
                        for (var i = 0; i < points.Count - 1; i++)
                        {
                            var p1 = points[i];
                            var p2 = points[(i + 1) % points.Count];
                            DrawLine(p1, p2, lineInfo);
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
        // Console.WriteLine("----------------Loop Started------------------");
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
                        DrawLine(points[0], points[1], lineInfo);
                    }
                    else
                    {
                        for (var i = 0; i < points.Count - 1; i++)
                        {
                            var p1 = points[i];
                            var p2 = points[(i + 1) % points.Count];
                            DrawLine(p1, p2, lineInfo);
                        }
                    }
                    
                    // Console.WriteLine($"        > While Counter reduced by one from {whileCounter} to {whileCounter - 1}");
                    points.Clear();
                    whileCounter--;
                }

            }
            else
            {
                // Console.WriteLine("     > Change point started------------------");
                // Console.WriteLine($"        > Cur Index: {curIndex} | Polyline Count: {polyline.Count} | Remaining: {whileCounter}");
                // Console.WriteLine($"        > Cur Distance: {curDistance} | Next Distance: {nextDistance} | Segment Length: {curDis}");
                if (curIndex >= polyline.Count - 2) //last point
                {
                    if (points.Count > 0)
                    {
                        points.Add(nextPoint);
                        if (points.Count == 2)
                        {
                            DrawLine(points[0], points[1], lineInfo);
                        }
                        else
                        {
                            for (var i = 0; i < points.Count - 1; i++)
                            {
                                var p1 = points[i];
                                var p2 = points[(i + 1) % points.Count];
                                DrawLine(p1, p2, lineInfo);
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
                // Console.WriteLine("         > Change point ended------------------");
            }
            
        }

        // Console.WriteLine("----------------Loop Ended------------------");
        return perimeter;
    }
   
    #endregion
    
    #region Custom Line Drawing
    public static void DrawLine(Vector2 start, Vector2 end, float thickness, ColorRgba color, LineCapType capType = LineCapType.None, int capPoints = 0)
    {
        if (thickness < LineDrawingInfo.LineMinThickness) thickness = LineDrawingInfo.LineMinThickness;
        var w = end - start;
        float ls = w.X * w.X + w.Y * w.Y; // w.LengthSquared();
        if (ls <= 0f) return;
        
        var dir = w / MathF.Sqrt(ls);
        var pR = new Vector2(-dir.Y, dir.X);//perpendicular right
        var pL = new Vector2(dir.Y, -dir.X);//perpendicular left
        
        if (capType == LineCapType.Extended) //expand outwards
        {
            start -= dir * thickness;
            end += dir * thickness;
        }
        else if (capType == LineCapType.Capped)//shrink inwards so that the line with cap is the same length
        {
            start += dir * thickness;
            end -= dir * thickness;
        }
        
        var tl = start + pL * thickness;
        var bl = start + pR * thickness;
        var br = end + pR * thickness;
        var tr = end + pL * thickness;
        
        Raylib.DrawTriangle(tl, bl, br, color.ToRayColor());
        Raylib.DrawTriangle(tl, br, tr, color.ToRayColor());

        if (capType is LineCapType.None or LineCapType.Extended) return;
        if (capPoints <= 0) return;
        
        //Draw Cap
        if (capPoints == 1)
        {
            var capStart = start - dir * thickness;
            var capEnd = end + dir * thickness;
            
            Raylib.DrawTriangle(tl, capStart, bl, color.ToRayColor());
            Raylib.DrawTriangle(tr, br, capEnd, color.ToRayColor());
        }
        else
        {
            var curStart = tl;
            var curEnd = br;
            float angleStep = (180f / (capPoints + 1)) * ShapeMath.DEGTORAD;
                
            for (var i = 1; i <= capPoints; i++)
            {
                var pStart = start + pL.Rotate(- angleStep * i) * thickness;
                Raylib.DrawTriangle(pStart, start, curStart, color.ToRayColor());
                curStart = pStart;
                    
                var pEnd = end + pR.Rotate(- angleStep * i) * thickness;
                Raylib.DrawTriangle(pEnd, end, curEnd, color.ToRayColor());
                curEnd = pEnd;
            }
            Raylib.DrawTriangle(curStart, bl, start, color.ToRayColor());
            Raylib.DrawTriangle(curEnd, tr, end, color.ToRayColor());

        }
    }

    /// <summary>
    /// Draws part of a line from start to end depending on f.
    /// </summary>
    /// <param name="start">The start point.</param>
    /// <param name="end">The end point.</param>
    /// <param name="f">The percentage of the line to draw. A negative value goes from end to start.</param>
    /// <param name="thickness">The thickness of the line.</param>
    /// <param name="color">The color of the line.</param>
    /// <param name="capType">The cap type of the line.</param>
    /// <param name="capPoints">How many points are used to draw the cap.</param>
    public static void DrawLinePercentage(Vector2 start, Vector2 end, float f, float thickness, ColorRgba color,
        LineCapType capType = LineCapType.None, int capPoints = 0)
    {
        if (f == 0) return;
        if (f < 0)
        {
            var newEnd = end.Lerp(start, f * -1f);
            DrawLinePercentage(newEnd, newEnd, f, thickness, color, capType, capPoints);
        }
        else
        {
            var newEnd = start.Lerp(end, f);
            DrawLinePercentage(newEnd, newEnd, f, thickness, color, capType, capPoints);
        }
        
    }
    
    
    public static void DrawLine(float startX, float startY, float endX, float endY, float thickness, 
        ColorRgba color, LineCapType capType = LineCapType.None, int capPoints = 0) 
        => DrawLine(new(startX, startY), new(endX, endY), thickness, color, capType, capPoints);

    public static void DrawLine(Vector2 start, Vector2 end, LineDrawingInfo info) => DrawLine(start, end, info.Thickness, info.Color, info.CapType, info.CapPoints);
    /// <summary>
    /// Draws part of a line from start to end depending on f.
    /// </summary>
    /// <param name="start">The start point.</param>
    /// <param name="end">The end point.</param>
    /// <param name="f">The percentage of the line to draw. A negative value goes from end to start.</param>
    /// <param name="info">The line drawing info for how to draw the line.</param>
    public static void DrawLinePercentage(Vector2 start, Vector2 end, float f, LineDrawingInfo info) 
        => DrawLinePercentage(start, end, f, info.Thickness, info.Color, info.CapType, info.CapPoints);
    public static void DrawLine(Vector2 start, Vector2 end, LineDrawingInfo info, float scaleFactor, float scaleOrigin = 0.5f)
    {
        var p = start.Lerp(end, scaleOrigin);
        var s = start - p;
        var e = end - p;

        var newStart = p + s * scaleFactor;
        var newEnd = p + e * scaleFactor;
        DrawLine(newStart, newEnd, info);
    }

    public static void DrawLine(float startX, float startY, float endX, float endY, LineDrawingInfo info) 
        => DrawLine(new(startX, startY), new(endX, endY), info.Thickness, info.Color, info.CapType, info.CapPoints);
    
    
    // public static void DrawLineBackup(Vector2 start, Vector2 end, float thickness, ShapeColor color, LineEndCap lineEndCap = LineEndCap.None, int endCapPoints = 0)
    // {
    //     if (thickness < LineMinThickness) thickness = LineMinThickness;
    //     var w = (end - start);
    //     if (w.LengthSquared() <= 0f) return;
    //     
    //     var dir = w.Normalize();
    //     var pR = dir.GetPerpendicularRight();
    //     var pL = dir.GetPerpendicularLeft();
    //     
    //     if (lineEndCap == LineEndCap.Extended)
    //     {
    //         start -= dir * thickness;
    //         end += dir * thickness;
    //     }
    //     
    //     var tl = start + pL * thickness;
    //     var bl = start + pR * thickness;
    //     var br = end + pR * thickness;
    //     var tr = end + pL * thickness;
    //     Raylib.DrawTriangle(tl, bl, br, color);
    //     Raylib.DrawTriangle(tl, br, tr, color);
    //     
    //     if (lineEndCap == LineEndCap.Capped && endCapPoints > 0)
    //     {
    //         if (endCapPoints == 1)
    //         {
    //             var capStart = start - dir * thickness;
    //             var capEnd = end + dir * thickness;
    //         
    //             Raylib.DrawTriangle(tl, capStart, bl, color);
    //             Raylib.DrawTriangle(tr, br, capEnd, color);
    //         }
    //         else
    //         {
    //             var curStart = tl;
    //             var curEnd = br;
    //             float angleStep = (180f / (endCapPoints + 1)) * ShapeMath.DEGTORAD;
    //             
    //             // DrawCircleV(curEnd, 6f, GREEN);
    //             for (var i = 1; i <= endCapPoints; i++)
    //             {
    //                 var pStart = start + pL.Rotate(- angleStep * i) * thickness;
    //                 Raylib.DrawTriangle(pStart, start, curStart, color);
    //                 curStart = pStart;
    //                 
    //                 var pEnd = end + pR.Rotate(- angleStep * i) * thickness;
    //                 Raylib.DrawTriangle(pEnd, end, curEnd, color);
    //                 // DrawCircleV(pEnd, 6f, WHITE);
    //                 curEnd = pEnd;
    //             }
    //             Raylib.DrawTriangle(curStart, bl, start, color);
    //             Raylib.DrawTriangle(curEnd, tr, end, color);
    //             // DrawCircleV(tr, 6f, RED);
    //
    //         }
    //     }
    // }
    #endregion
    
    #region Intersection
    public static void Draw(this Intersection intersection, float lineThickness, ColorRgba intersectColorRgba, ColorRgba normalColorRgba)
    {
        if (intersection.ColPoints == null || intersection.ColPoints.Count <= 0) return;
        
        foreach (var i in intersection.ColPoints)
        {
            DrawCircle(i.Point, lineThickness * 2f, intersectColorRgba, 12);
            DrawLine(i.Point, i.Point + i.Normal * lineThickness * 10f, lineThickness, normalColorRgba);
            // Segment normal = new(i.Point, i.Point + i.Normal * lineThickness * 10f);
            // normal.Draw(lineThickness, normalColorRgba);
        }
    }

    #endregion
    
    #region Pixel
    public static void DrawPixel(Vector2 pos, ColorRgba color) => Raylib.DrawPixelV(pos, color.ToRayColor()); 
    public static void DrawPixel(float x, float y, ColorRgba color) => Raylib.DrawPixelV(new(x, y), color.ToRayColor());
    #endregion

    #region Point
    public static void Draw(this Vector2 p, float radius, ColorRgba color, int segments = 16) => DrawCircle(p, radius, color, segments);

    public static void Draw(this Points points, float r, ColorRgba color, int segments = 16)
    {
        foreach (var p in points)
        {
            p.Draw(r, color, segments);
        }
    }

    #endregion

    #region Segment
    
    public static void Draw(this Segment segment, float thickness, ColorRgba color, LineCapType capType = LineCapType.None, int capPoints = 0 ) 
        => DrawLine(segment.Start, segment.End, thickness, color, capType, capPoints);
    
    public static void DrawPercentage(this Segment segment, float f, float thickness, ColorRgba color, LineCapType capType = LineCapType.None, int capPoints = 0 ) 
        => DrawLinePercentage(segment.Start, segment.End, f, thickness, color, capType, capPoints);
    public static void Draw(this Segment segment, LineDrawingInfo lineInfo) 
        => DrawLine(segment.Start, segment.End, lineInfo);
    
    public static void DrawPercentage(this Segment segment, float f, LineDrawingInfo lineInfo) 
        => DrawLinePercentage(segment.Start, segment.End, f, lineInfo);
    public static void Draw(this Segment segment, float originF, float angleRad, LineDrawingInfo lineInfo)
    {
        if (angleRad != 0f)
        {
            segment.ChangeRotation(angleRad, originF).Draw(lineInfo);
            return;

        }
        
        DrawLine(segment.Start, segment.End, lineInfo);
    }

    public static void Draw(this Segments segments, LineDrawingInfo lineInfo)
    {
        if (segments.Count <= 0) return;
        foreach (var seg in segments)
        {
            seg.Draw(lineInfo);
        }
    }
    public static void Draw(this Segments segments, float thickness, List<ColorRgba> colors, LineCapType capType = LineCapType.None, int capPoints = 0)
    {
        if (segments.Count <= 0 || colors.Count <= 0) return;
        LineDrawingInfo info = new(thickness, ColorRgba.White, capType, capPoints);
        for (var i = 0; i < segments.Count; i++)
        {
            var c = colors[i % colors.Count];
            segments[i].Draw(thickness, c, capType, capPoints);
        }
    }
    
    public static void DrawVertices(this Segment segment, float vertexRadius, ColorRgba color, int vertexSegments = 16)
    {
        segment.Start.Draw( vertexRadius, color, vertexSegments);
        segment.End.Draw(vertexRadius, color, vertexSegments);
    }
   
    public static Segments CreateLightningLine(this Segment segment, int segments = 10, float maxSway = 80f)
    {
        Segments result = new();
        var w = segment.End - segment.Start;
        var dir = w.Normalize();
        var n = new Vector2(dir.Y, -dir.X);
        float length = w.Length();

        float prevDisplacement = 0;
        var cur = segment.Start;
        //result.Add(start);

        float segmentLength = length / segments;
        float remainingLength = length;
        List<Vector2> accumulator = new()
        {
            segment.Start
        };
        while (remainingLength > 0f)
        {
            float randSegmentLength = Rng.Instance.RandF() * segmentLength;
            remainingLength -= randSegmentLength;
            if (remainingLength <= 0f)
            {
                if(accumulator.Count == 1)
                {
                    result.Add(new(accumulator[0], segment.End));
                }
                else
                {
                    result.Add(new(result[result.Count - 1].End, segment.End));
                }
                break;
            }
            float scale = randSegmentLength / segmentLength;
            float displacement = Rng.Instance.RandF(-maxSway, maxSway);
            displacement -= (displacement - prevDisplacement) * (1 - scale);
            cur = cur + dir * randSegmentLength;
            var p = cur + displacement * n;
            accumulator.Add(p);
            if(accumulator.Count == 2)
            {
                result.Add(new(accumulator[0], accumulator[1]));
                accumulator.Clear();
            }
            prevDisplacement = displacement;
        }
        return result;
    }
    public static Segments CreateLightningLine(this Segment segment, float segmentLength = 5f, float maxSway = 80f)
    {
        Segments result = new();
        var w = segment.End - segment.Start;
        var dir = w.Normalize();
        var n = new Vector2(dir.Y, -dir.X);
        float length = w.Length();

        float prevDisplacement = 0;
        var cur = segment.Start;
        List<Vector2> accumulator = new()
        {
            segment.Start
        };
        float remainingLength = length;
        while (remainingLength > 0f)
        {
            float randSegmentLength = Rng.Instance.RandF() * segmentLength;
            remainingLength -= randSegmentLength;
            if (remainingLength <= 0f)
            {
                if (accumulator.Count == 1)
                {
                    result.Add(new(accumulator[0], segment.End));
                }
                else
                {
                    result.Add(new(result[result.Count - 1].End, segment.End));
                }
                break;
            }
            float scale = randSegmentLength / segmentLength;
            float displacement = Rng.Instance.RandF(-maxSway, maxSway);
            displacement -= (displacement - prevDisplacement) * (1 - scale);
            cur = cur + dir * randSegmentLength;
            var p = cur + displacement * n;
            accumulator.Add(p);
            if (accumulator.Count == 2)
            {
                result.Add(new(accumulator[0], accumulator[1]));
                accumulator.Clear();
            }
            prevDisplacement = displacement;
        }
        return result;
    }
    
    public static void DrawLineGlow(Vector2 start, Vector2 end, float width, float endWidth, ColorRgba color, ColorRgba endColorRgba, int steps, LineCapType capType = LineCapType.None, int capPoints = 0)
    {
        float wStep = (endWidth - width) / steps;

        int rStep = (endColorRgba.R - color.R) / steps;
        int gStep = (endColorRgba.G - color.G) / steps;
        int bStep = (endColorRgba.B - color.B) / steps;
        int aStep = (endColorRgba.A - color.A) / steps;

        for (int i = steps; i >= 0; i--)
        {
            DrawLine
            (
                start, end, width + wStep * i,
                new
                (
                    color.R + rStep * i,
                    color.G + gStep * i,
                    color.B + bStep * i,
                    color.A + aStep * i
                ),
                capType,
                capPoints
            );
        }
    }
    public static void DrawGlow(this Segment segment, float width, float endWidth, ColorRgba color, ColorRgba endColorRgba, int steps, LineCapType capType = LineCapType.None, int capPoints = 0)
    {
        DrawLineGlow(segment.Start, segment.End, width, endWidth, color, endColorRgba, steps, capType, capPoints);
    }
    public static void DrawGlow(this Segments segments, float width, float endWidth, ColorRgba color, ColorRgba endColorRgba, int steps, LineCapType capType = LineCapType.None, int capPoints = 0)
    {
        foreach (var seg in segments)
        {
            seg.DrawGlow(width, endWidth, color, endColorRgba, steps, capType, capPoints);
        }
    }

     
    /// <summary>
    /// Draws a segment scaled towards the origin.
    /// </summary>
    /// <param name="s">The segment to draw.</param>
    /// <param name="lineInfo">How to draw the lines.</param>
    /// <param name="sideScaleFactor">The scale factor for each side. 0f means no triangle is drawn, 1f means normal triangle is drawn,
    /// 0.5 means each side is half as long.</param>
    /// <param name="sideScaleOrigin">The point along the line to scale from in both directions.</param>
    public static void DrawScaled(this Segment s, LineDrawingInfo lineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0) return;
        if (sideScaleFactor >= 1)
        {
            s.Draw(lineInfo);
            return;
        }
        
        DrawLine(s.Start, s.End, lineInfo, sideScaleFactor, sideScaleOrigin);
    }
    /// <summary>
    /// Draws a segment scaled towards the origin.
    /// </summary>
    /// <param name="s">The segment to draw.</param>
    /// <param name="angleRad">The rotation of the segment.</param>
    /// <param name="originF">Point to rotate the segment around. Value between 0 - 1. (0 = Start, 1 = End)</param>
    /// <param name="lineInfo">How to draw the lines.</param>
    /// <param name="sideScaleFactor">The scale factor for each side. 0f means no triangle is drawn, 1f means normal triangle is drawn,
    /// 0.5 means each side is half as long.</param>
    /// <param name="sideScaleOrigin">The point along the line to scale from in both directions.</param>
    public static void DrawScaled(this Segment s, float originF, float angleRad, LineDrawingInfo lineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0) return;
        if (sideScaleFactor >= 1)
        {
            s.Draw(originF, angleRad, lineInfo);
            return;
        }
        
        if(angleRad == 0f) DrawLine(s.Start, s.End, lineInfo, sideScaleFactor, sideScaleOrigin);
        else
        {
            var origin = s.GetPoint(originF);
            var rStart = origin +  (s.Start - origin).Rotate(angleRad);
            var rEnd = origin + (s.End - origin).Rotate(angleRad);
            DrawLine(rStart, rEnd, lineInfo, sideScaleFactor, sideScaleOrigin);
        }
    }

    #endregion

    #region Circle
    public static void DrawCircle(Vector2 center, float radius, ColorRgba color, int segments = 16)
    {
        if (segments < 3) segments = 3;
        Raylib.DrawCircleSector(center, radius, 0, 360, segments, color.ToRayColor());
    }
    public static void DrawCircle(Vector2 center, float radius, ColorRgba color, float rotDeg, int segments = 16)
    {
        if (segments < 3) segments = 3;
        Raylib.DrawCircleSector(center, radius, rotDeg, 360 + rotDeg, segments, color.ToRayColor());
    }

    
    public static void Draw(this Circle c, ColorRgba color, float rotDeg, int segments = 16)
    {
        if (segments < 3) segments = 3;
        Raylib.DrawCircleSector(c.Center, c.Radius, rotDeg, 360 + rotDeg, segments, color.ToRayColor());
    }
    public static void Draw(this Circle c, ColorRgba color) => DrawCircle(c.Center, c.Radius, color);
    public static void Draw(this Circle c, ColorRgba color, int segments) => DrawCircle(c.Center, c.Radius, color, segments);
    
    
    public static void DrawLines(this Circle c, float lineThickness, int sides, ColorRgba color) => Raylib.DrawPolyLinesEx(c.Center, sides, c.Radius, 0f, lineThickness * 2, color.ToRayColor());
    public static void DrawLines(this Circle c, LineDrawingInfo lineInfo, int sides) => DrawLines(c, lineInfo, 0f, sides);
    public static void DrawLines(this Circle c, LineDrawingInfo lineInfo, float rotDeg, int sides)
    {
        if (sides < 3) return;
        var angleStep = (2f * ShapeMath.PI) / sides;
        var rotRad = rotDeg * ShapeMath.DEGTORAD;
        for (int i = 0; i < sides; i++)
        {
            var nextIndex = (i + 1) % sides;
            var curP = c.Center + new Vector2(c.Radius, 0f).Rotate(rotRad + angleStep * i);
            var nextP = c.Center + new Vector2(c.Radius, 0f).Rotate(rotRad + angleStep * nextIndex);
            
            DrawLine(curP, nextP, lineInfo);
            
        }
    }
    
    /// <summary>
    /// Draws part of a circle outline depending on f.
    /// </summary>
    /// <param name="c">The circle parameters.</param>
    /// <param name="f">The percentage of the outline to draw. A positive value goes counter-clockwise
    /// and a negative value goes clockwise.</param>
    /// <param name="lineThickness">The line drawing parameters.</param>
    /// <param name="rotDeg">The rotation of the circle. The lower the resolution of the circle the more visible is rotation</param>
    /// <param name="sides">The resolution of the circle. The more sides are used the closer it represents a circle.</param>
    /// <param name="color">The color of the line.</param>
    /// <param name="lineCapType">The end cap type of the line.</param>
    /// <param name="capPoints">How many points are used to draw the end cap.</param>
    public static void DrawLinesPercentage(this Circle c, float f, float lineThickness, float rotDeg, int sides, ColorRgba color, LineCapType lineCapType, int capPoints)
    {
        if (sides < 3 || f == 0) return;
        
        DrawCircleLinesPercentage(c.Center, c.Radius, f, lineThickness, rotDeg, sides, color, lineCapType, capPoints);
    }
    
    /// <summary>
    /// Draws part of a circle outline depending on f.
    /// </summary>
    /// <param name="c">The circle parameters.</param>
    /// <param name="f">The percentage of the outline to draw. A positive value goes counter-clockwise
    /// and a negative value goes clockwise.</param>
    /// <param name="lineInfo">The line drawing parameters.</param>
    /// <param name="rotDeg">The rotation of the circle. The lower the resolution of the circle the more visible is rotation</param>
    /// <param name="sides">The resolution of the circle. The more sides are used the closer it represents a circle.</param>
    public static void DrawLinesPercentage(this Circle c, float f, LineDrawingInfo lineInfo, float rotDeg, int sides)
    {
        DrawLinesPercentage(c, f, lineInfo.Thickness, rotDeg, sides, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
    }
    
    public static void DrawLines(this Circle c, float lineThickness, float rotDeg, int sides, ColorRgba color) => Raylib.DrawPolyLinesEx(c.Center, sides, c.Radius, rotDeg, lineThickness * 2, color.ToRayColor());
    public static void DrawLines(this Circle c, float lineThickness, ColorRgba color, float sideLength = 8f)
    {
        int sides = GetCircleSideCount(c.Radius, sideLength);
        Raylib.DrawPolyLinesEx(c.Center, sides, c.Radius, 0f, lineThickness * 2, color.ToRayColor());
    }
    public static void DrawLines(this Circle c, LineDrawingInfo lineInfo, float rotDeg, float sideLength = 8f)
    {
        int sides = GetCircleSideCount(c.Radius, sideLength);
        DrawLines(c, lineInfo, rotDeg, sides);
    }
    public static void DrawLinesPercentage(this Circle c, float f, float lineThickness, ColorRgba color, float sideLength = 8f)
    {
        if (f == 0) return;
        int sides = GetCircleSideCount(c.Radius, sideLength);
        DrawLinesPercentage(c, f, lineThickness, 0f, sides, color, LineCapType.None, 0);
    }
    public static void DrawLinesPercentage(this Circle c, float f, float lineThickness, float rotDeg, ColorRgba color, LineCapType capType, int capPoints, float sideLength = 8f)
    {
        if (f == 0) return;
        int sides = GetCircleSideCount(c.Radius, sideLength);
        DrawLinesPercentage(c, f, lineThickness, rotDeg, sides, color, capType, capPoints);
    }
    public static void DrawLinesPercentage(this Circle c, float f, LineDrawingInfo lineInfo, float rotDeg, float sideLength = 8f)
    {
        if (f == 0) return;
        int sides = GetCircleSideCount(c.Radius, sideLength);
        DrawLinesPercentage(c, f, lineInfo, rotDeg, sides);
    }
    
    /// <summary>
    /// Draws a circle where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="lineInfo">How to draw the lines.</param>
    /// <param name="sides">How many sides the circle should be drawn with.</param>
    /// <param name="sideScaleFactor">The scale factor for each side. 0f means no circle is drawn, 1f means normal circle is drawn,
    /// 0.5 means each side is half as long.</param>
    /// <param name="sideScaleOrigin">The point along the line to scale from in both directions.</param>
    public static void DrawLinesScaled(this Circle c, LineDrawingInfo lineInfo, int sides, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        DrawLinesScaled(c, lineInfo, 0f, sides, sideScaleFactor, sideScaleOrigin);
    }
    
    /// <summary>
    /// Draws a circle where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="lineInfo">How to draw the lines.</param>
    /// <param name="rotDeg">The rotation of the circle.</param>
    /// <param name="sides">How many sides the circle should be drawn with.</param>
    /// <param name="sideScaleFactor">The scale factor for each side. 0f means no circle is drawn, 1f means normal circle is drawn,
    /// 0.5 means each side is half as long.</param>
    /// <param name="sideScaleOrigin">The point along the line to scale from in both directions.</param>
    public static void DrawLinesScaled(this Circle c, LineDrawingInfo lineInfo, float rotDeg, int sides, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0f) return;
        if (sideScaleFactor >= 1f)
        {
            DrawLines(c, lineInfo, sides);
            return;
        }
        
        var angleStep = (2f * ShapeMath.PI) / sides;
        var rotRad = rotDeg * ShapeMath.DEGTORAD;
        
        for (int i = 0; i < sides; i++)
        {
            var nextIndex = (i + 1) % sides;
            var start = c.Center + new Vector2(c.Radius, 0f).Rotate(rotRad + angleStep * i);
            var end = c.Center + new Vector2(c.Radius, 0f).Rotate(rotRad + angleStep * nextIndex);
            
            DrawLine(start, end, lineInfo, sideScaleFactor, sideScaleOrigin);
            
        }
    }
    
    /// <summary>
    /// Draws part of a circle outline depending on f.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="f">The percentage of the outline to draw. A positive value goes counter-clockwise
    /// and a negative value goes clockwise.</param>
    /// <param name="lineThickness">The line drawing parameters.</param>
    /// <param name="rotDeg">The rotation of the circle. The lower the resolution of the circle the more visible is rotation</param>
    /// <param name="sides">The resolution of the circle. The more sides are used the closer it represents a circle.</param>
    /// <param name="color">The color of the line.</param>
    /// <param name="lineCapType">The end cap type of the line.</param>
    /// <param name="capPoints">How many points are used to draw the end cap.</param>
    public static void DrawCircleLinesPercentage(Vector2 center, float radius, float f, float lineThickness, float rotDeg, int sides, ColorRgba color, LineCapType lineCapType, int capPoints)
    {
        if (sides < 3 || f == 0) return;

        bool negative = f < 0;
        float percentage = ShapeMath.Clamp(negative ? f * -1 : f, 0f, 1f);

        var percentageStep = 1f / sides;
        var percentageToDraw = percentage;
        var angleStep = (2f * ShapeMath.PI) / sides;
        var currentAngle = rotDeg * ShapeMath.DEGTORAD;

        while (percentageToDraw > 0)
        {
            var curP = center + new Vector2(radius, 0f).Rotate(currentAngle);
            if(negative) currentAngle -= angleStep;
            else currentAngle += angleStep;
            
            var nextP = center + new Vector2(radius, 0f).Rotate(currentAngle);
            if(negative) currentAngle -= angleStep;
            else currentAngle += angleStep;
            
            if (percentageToDraw < percentageStep)
            {
                var sideP = percentageToDraw / percentage;
                DrawLinePercentage(curP, nextP, sideP, lineThickness, color, lineCapType, capPoints);
                percentageToDraw = 0f;
            }
            else
            {
                DrawLine(curP, nextP, lineThickness, color, lineCapType, capPoints);
                percentageToDraw -= percentageStep;
            }
        }

        
    }
    
    
    /// <summary>
    /// Very usefull for drawing small/tiny circles. Drawing the circle as rect increases performance a lot.
    /// </summary>
    /// <param name="center"></param>
    /// <param name="radius"></param>
    /// <param name="color"></param>
    public static void DrawCircleFast(Vector2 center, float radius, ColorRgba color)
    {
        // Rect r = new(center, new Vector2(radius * 2f), new Vector2(0.5f));
        // r.Draw(color);
        DrawRect(center - new Vector2(radius, radius), center + new Vector2(radius, radius), color);
    }
    
    public static void DrawCircleLines(Vector2 center, float radius, float lineThickness, int sides, ColorRgba color) 
        => Raylib.DrawPolyLinesEx(center, sides, radius, 0f, lineThickness * 2, color.ToRayColor());
    public static void DrawCircleLines(Vector2 center, float radius, LineDrawingInfo lineInfo, int sides) 
        => DrawCircleLines(center, radius, lineInfo, 0f, sides);
    public static void DrawCircleLinesPercentage(Vector2 center, float radius, float f, LineDrawingInfo lineInfo, float rotDeg, int sides) 
        => DrawCircleLinesPercentage(center, radius, f, lineInfo.Thickness, rotDeg, sides, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
    public static void DrawCircleLines(Vector2 center, float radius, float lineThickness, float rotDeg, int sides, ColorRgba color) 
        => Raylib.DrawPolyLinesEx(center, sides, radius, rotDeg, lineThickness * 2, color.ToRayColor());
    public static void DrawCircleLines(Vector2 center, float radius, LineDrawingInfo lineInfo, float rotDeg, int sides)
    {
        if (sides < 3) return;
        var angleStep = (2f * ShapeMath.PI) / sides;
        var rotRad = rotDeg * ShapeMath.DEGTORAD;
        
        for (int i = 0; i < sides; i++)
        {
            var nextIndex = (i + 1) % sides;
            var curP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * i);
            var nextP = center + new Vector2(radius, 0f).Rotate(rotRad + angleStep * nextIndex);
            
            DrawLine(curP, nextP, lineInfo);
            
        }
    }
    public static void DrawCircleLines(Vector2 center, float radius, float lineThickness, ColorRgba color, float sideLength = 8f)
    {
        int sides = GetCircleSideCount(radius, sideLength);
        Raylib.DrawPolyLinesEx(center, sides, radius, 0f, lineThickness * 2, color.ToRayColor());
    }
    public static void DrawCircleLines(Vector2 center, float radius, LineDrawingInfo lineInfo, float rotDeg, float sideLength = 8f)
    {
        int sides = GetCircleSideCount(radius, sideLength);
        DrawCircleLines(center, radius, lineInfo, rotDeg, sides);
    }
    public static void DrawCircleLinesPercentage(Vector2 center, float radius, float f, LineDrawingInfo lineInfo, float rotDeg, float sideLength = 8f)
    {
        int sides = GetCircleSideCount(radius, sideLength);
        DrawCircleLinesPercentage(center, radius, f, lineInfo, rotDeg, sides);
    }
    
    public static void DrawSector(this Circle c, float startAngleDeg, float endAngleDeg, int segments, ColorRgba color)
    {
        Raylib.DrawCircleSector(c.Center, c.Radius, TransformAngleDegToRaylib(startAngleDeg), TransformAngleDegToRaylib(endAngleDeg), segments, color.ToRayColor());
    }
    public static void DrawCircleSector(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, int segments, ColorRgba color)
    {
        Raylib.DrawCircleSector(center, radius, TransformAngleDegToRaylib(startAngleDeg), TransformAngleDegToRaylib(endAngleDeg), segments, color.ToRayColor());
    }
    
    public static void DrawSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, LineDrawingInfo lineInfo, bool closed = true, float sideLength = 8f)
    {
        DrawCircleSectorLines(c.Center, c.Radius, startAngleDeg, endAngleDeg, lineInfo, closed, sideLength);
    }
    public static void DrawSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, LineDrawingInfo lineInfo, bool closed = true, float sideLength = 8f)
    {
        DrawCircleSectorLines(c.Center, c.Radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, lineInfo, closed, sideLength);
    }
    public static void DrawSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, int sides, LineDrawingInfo lineInfo, bool closed = true)
    {
        DrawCircleSectorLines(c.Center, c.Radius, startAngleDeg, endAngleDeg, sides, lineInfo, closed);
    }
    public static void DrawSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, int sides, LineDrawingInfo lineInfo, bool closed = true)
    {
        DrawCircleSectorLines(c.Center, c.Radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, sides, lineInfo, closed);
    }
    
    /// <summary>
    /// Draws a circle where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="c">The circle to draw.</param>
    /// <param name="lineInfo">How to draw the lines.</param>
    /// <param name="startAngleDeg">The starting rotation of the sector.</param>
    /// <param name="endAngleDeg">The end rotation of the sector.</param>
    /// <param name="rotOffsetDeg">Rotation offset for the sector.</param>
    /// <param name="sides">How many sides the circle should be drawn with.</param>
    /// <param name="sideScaleFactor">The scale factor for each side. 0f means no circle is drawn, 1f means normal circle is drawn,
    /// 0.5 means each side is half as long.</param>
    /// <param name="sideScaleOrigin">The point along the line to scale from in both directions.</param>
    /// <param name="closed">Should the sector be closed.</param>
    public static void DrawSectorLinesScaled(this Circle c, LineDrawingInfo lineInfo, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, int sides, float sideScaleFactor, float sideScaleOrigin = 0.5f, bool closed = true)
    {
        if (sideScaleFactor <= 0f) return;
        if (sideScaleFactor >= 1f)
        {
            DrawSectorLines(c, startAngleDeg, endAngleDeg, rotOffsetDeg, sides, lineInfo, closed);
            return;
        }
        
        float startAngleRad = (startAngleDeg + rotOffsetDeg) * ShapeMath.DEGTORAD;
        float endAngleRad = (endAngleDeg + rotOffsetDeg) * ShapeMath.DEGTORAD;
        float anglePiece = endAngleRad - startAngleRad;
        float angleStep = anglePiece / sides;
        if (closed)
        {
            var sectorStart = c.Center + (ShapeVec.Right() * c.Radius).Rotate(startAngleRad);
            DrawLine(c.Center, sectorStart, lineInfo, sideScaleFactor, sideScaleOrigin);
        
            var sectorEnd = c.Center + (ShapeVec.Right() * c.Radius).Rotate(endAngleRad);
            DrawLine(c.Center, sectorEnd, lineInfo, sideScaleFactor, sideScaleOrigin);
        }
        for (int i = 0; i < sides; i++)
        {
            var nextIndex = (i + 1) % sides;
            var start = c.Center + new Vector2(c.Radius, 0f).Rotate(startAngleRad + angleStep * i);
            var end = c.Center + new Vector2(c.Radius, 0f).Rotate(startAngleRad + angleStep * nextIndex);
            
            DrawLine(start, end, lineInfo, sideScaleFactor, sideScaleOrigin);
            
        }
    }

    public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, LineDrawingInfo lineInfo, bool closed = true, float sideLength = 8f)
    {
        float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
        float endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
        float anglePiece = endAngleRad - startAngleRad;
        int sides = GetCircleArcSideCount(radius, MathF.Abs(anglePiece * ShapeMath.RADTODEG), sideLength);
        float angleStep = anglePiece / sides;
        if (closed)
        {
            var sectorStart = center + (ShapeVec.Right() * radius + new Vector2(lineInfo.Thickness / 2, 0)).Rotate(startAngleRad);
            DrawLine(center, sectorStart, lineInfo);

            var sectorEnd = center + (ShapeVec.Right() * radius + new Vector2(lineInfo.Thickness / 2, 0)).Rotate(endAngleRad);
            DrawLine(center, sectorEnd, lineInfo);
        }
        for (var i = 0; i < sides; i++)
        {
            var start = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * i);
            var end = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * (i + 1));
            DrawLine(start, end, lineInfo);
        }
    }
    public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, LineDrawingInfo lineInfo, bool closed = true, float sideLength = 8f)
    {
        DrawCircleSectorLines(center, radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, lineInfo, closed, sideLength);
    }
    public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, int sides, LineDrawingInfo lineInfo, bool closed = true)
    {
        float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
        float endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
        float anglePiece = endAngleDeg - startAngleRad;
        float angleStep = MathF.Abs(anglePiece) / sides;
        if (closed)
        {
            var sectorStart = center + (ShapeVec.Right() * radius + new Vector2(lineInfo.Thickness / 2, 0)).Rotate(startAngleRad);
            DrawLine(center, sectorStart, lineInfo);

            var sectorEnd = center + (ShapeVec.Right() * radius + new Vector2(lineInfo.Thickness / 2, 0)).Rotate(endAngleRad);
            DrawLine(center, sectorEnd, lineInfo);
        }
        for (var i = 0; i < sides; i++)
        {
            var start = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * i);
            var end = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * (i + 1));
            DrawLine(start, end, lineInfo);
        }
    }
    public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, int sides, LineDrawingInfo lineInfo, bool closed = true)
    {
        DrawCircleSectorLines(center, radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, sides, lineInfo, closed);
    }
    
    public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, float lineThickness, ColorRgba color, bool closed = true, float sideLength = 8f)
    {
        float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
        float endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
        float anglePiece = endAngleRad - startAngleRad;
        int sides = GetCircleArcSideCount(radius, MathF.Abs(anglePiece * ShapeMath.RADTODEG), sideLength);
        float angleStep = anglePiece / sides;
        if (closed)
        {
            var sectorStart = center + (ShapeVec.Right() * radius + new Vector2(lineThickness / 2, 0)).Rotate(startAngleRad);
            DrawLine(center, sectorStart, lineThickness, color, LineCapType.CappedExtended, 2);

            var sectorEnd = center + (ShapeVec.Right() * radius + new Vector2(lineThickness / 2, 0)).Rotate(endAngleRad);
            DrawLine(center, sectorEnd, lineThickness, color, LineCapType.CappedExtended, 2);
        }
        for (var i = 0; i < sides; i++)
        {
            var start = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * i);
            var end = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * (i + 1));
            DrawLine(start, end, lineThickness, color, LineCapType.CappedExtended, 2);
        }
    }
    public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, float lineThickness, ColorRgba color, bool closed = true, float sideLength = 8f)
    {
        DrawCircleSectorLines(center, radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, lineThickness, color, closed, sideLength);
    }
    public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, int sides, float lineThickness, ColorRgba color, bool closed = true)
    {
        float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
        float endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
        float anglePiece = endAngleDeg - startAngleRad;
        float angleStep = MathF.Abs(anglePiece) / sides;
        if (closed)
        {
            var sectorStart = center + (ShapeVec.Right() * radius + new Vector2(lineThickness / 2, 0)).Rotate(startAngleRad);
            DrawLine(center, sectorStart, lineThickness, color, LineCapType.CappedExtended, 2);

            var sectorEnd = center + (ShapeVec.Right() * radius + new Vector2(lineThickness / 2, 0)).Rotate(endAngleRad);
            DrawLine(center, sectorEnd, lineThickness, color, LineCapType.CappedExtended, 2);
        }
        for (var i = 0; i < sides; i++)
        {
            var start = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * i);
            var end = center + (ShapeVec.Right() * radius).Rotate(startAngleRad + angleStep * (i + 1));
            DrawLine(start, end, lineThickness, color, LineCapType.CappedExtended, 2);
        }
    }
    public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, int sides, float lineThickness, ColorRgba color, bool closed = true)
    {
        DrawCircleSectorLines(center, radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, sides, lineThickness, color, closed);
    }

    public static void DrawCircleCheckeredLines(Vector2 pos, AnchorPoint alignement, float radius, float spacing, float lineThickness, float angleDeg, ColorRgba lineColorRgba, ColorRgba bgColorRgba, int circleSegments)
    {

        float maxDimension = radius;
        var size = new Vector2(radius, radius) * 2f;
        var aVector = alignement.ToVector2() * size;
        var center = pos - aVector + size / 2;
        float rotRad = angleDeg * ShapeMath.DEGTORAD;

        if (bgColorRgba.A > 0) DrawCircle(center, radius, bgColorRgba, circleSegments);

        var cur = new Vector2(-spacing / 2, 0f);
        while (cur.X > -maxDimension)
        {
            var p = center + cur.Rotate(rotRad);

            //float y = MathF.Sqrt((radius * radius) - (cur.X * cur.X));
            float angle = MathF.Acos(cur.X / radius);
            float y = radius * MathF.Sin(angle);

            var up = new Vector2(0f, -y);
            var down = new Vector2(0f, y);
            var start = p + up.Rotate(rotRad);
            var end = p + down.Rotate(rotRad);
            DrawLine(start, end, lineThickness, lineColorRgba);
            cur.X -= spacing;
        }

        cur = new(spacing / 2, 0f);
        while (cur.X < maxDimension)
        {
            var p = center + cur.Rotate(rotRad);
            //float y = MathF.Sqrt((radius * radius) - (cur.X * cur.X));
            float angle = MathF.Acos(cur.X / radius);
            float y = radius * MathF.Sin(angle);

            var up = new Vector2(0f, -y);
            var down = new Vector2(0f, y);
            var start = p + up.Rotate(rotRad);
            var end = p + down.Rotate(rotRad);
            DrawLine(start, end, lineThickness, lineColorRgba);
            cur.X += spacing;
        }

    }
    
    private static int GetCircleSideCount(float radius, float maxLength = 10f)
    {
        float circumference = 2.0f * ShapeMath.PI * radius;
        return (int)MathF.Max(circumference / maxLength, 5);
    }
    private static int GetCircleArcSideCount(float radius, float angleDeg, float maxLength = 10f)
    {
        float circumference = 2.0f * ShapeMath.PI * radius * (angleDeg / 360f);
        return (int)MathF.Max(circumference / maxLength, 1);
    }
    private static float TransformAngleDegToRaylib(float angleDeg) { return 450f - angleDeg; }
    
    #endregion

    #region Ring
    
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float lineThickness, ColorRgba color, float sideLength = 8f)
    {
        DrawCircleLines(center, innerRadius, lineThickness, color, sideLength);
        DrawCircleLines(center, outerRadius, lineThickness, color, sideLength);
    }
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, LineDrawingInfo lineInfo, float sideLength = 8f)
    {
        DrawCircleLines(center, innerRadius, lineInfo, 0f, sideLength);
        DrawCircleLines(center, outerRadius, lineInfo, 0f, sideLength);
    }
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float innerRotDeg, float outerRotDeg, LineDrawingInfo lineInfo, float sideLength = 8f)
    {
        DrawCircleLines(center, innerRadius, lineInfo, innerRotDeg, sideLength);
        DrawCircleLines(center, outerRadius, lineInfo, outerRotDeg, sideLength);
    }
    public static void DrawRingLinesPercentage(Vector2 center, float innerRadius, float outerRadius, float f, float innerRotDeg, float outerRotDeg, LineDrawingInfo lineInfo, float sideLength = 8f)
    {
        DrawCircleLinesPercentage(center, innerRadius, f, lineInfo, innerRotDeg, sideLength);
        DrawCircleLinesPercentage(center, outerRadius, f, lineInfo, outerRotDeg, sideLength);
    }
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float innerRotDeg, float outerRotDeg, float innerSideLength, float outerSideLength, LineDrawingInfo lineInfo)
    {
        DrawCircleLines(center, innerRadius, lineInfo, innerRotDeg, innerSideLength);
        DrawCircleLines(center, outerRadius, lineInfo, outerRotDeg, outerSideLength);
    }
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float innerRotDeg, float outerRotDeg, float innerSideLength, float outerSideLength, LineDrawingInfo innerLineInfo, LineDrawingInfo outerLineInfo)
    {
        DrawCircleLines(center, innerRadius, innerLineInfo, innerRotDeg, innerSideLength);
        DrawCircleLines(center, outerRadius, outerLineInfo, outerRotDeg, outerSideLength);
    }
    public static void DrawRingLinesPercentage(Vector2 center, float innerRadius, float outerRadius, float f, float innerRotDeg, float outerRotDeg, float innerSideLength, float outerSideLength, LineDrawingInfo lineInfo)
    {
        DrawCircleLinesPercentage(center, innerRadius, f, lineInfo, innerRotDeg, innerSideLength);
        DrawCircleLinesPercentage(center, outerRadius, f, lineInfo, outerRotDeg, outerSideLength);
    }
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float f, float innerRotDeg, float outerRotDeg, float innerSideLength, float outerSideLength, LineDrawingInfo innerLineInfo, LineDrawingInfo outerLineInfo)
    {
        DrawCircleLinesPercentage(center, outerRadius, f, outerLineInfo, outerRotDeg, outerSideLength);
        DrawCircleLinesPercentage(center, innerRadius, f, innerLineInfo, innerRotDeg, innerSideLength);
    }
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float innerRotDeg, float outerRotDeg, int sides, LineDrawingInfo lineInfo)
    {
        DrawCircleLines(center, innerRadius, lineInfo, innerRotDeg, sides);
        DrawCircleLines(center, outerRadius, lineInfo, outerRotDeg, sides);
    }
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float innerRotDeg, float outerRotDeg, int innerSides, int outerSides, LineDrawingInfo lineInfo)
    {
        DrawCircleLines(center, innerRadius, lineInfo, innerRotDeg, innerSides);
        DrawCircleLines(center, outerRadius, lineInfo, outerRotDeg, outerSides);
    }
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float innerRotDeg, float outerRotDeg, int innerSides, int outerSides, LineDrawingInfo innerLineInfo, LineDrawingInfo outerLineInfo)
    {
        DrawCircleLines(center, innerRadius, innerLineInfo, innerRotDeg, innerSides);
        DrawCircleLines(center, outerRadius, outerLineInfo, outerRotDeg, outerSides);
    }
    public static void DrawRingLinesPercentage(Vector2 center, float innerRadius, float outerRadius, float f, float innerRotDeg, float outerRotDeg, int innerSides, int outerSides, LineDrawingInfo lineInfo)
    {
        DrawCircleLinesPercentage(center, innerRadius, f, lineInfo, innerRotDeg, innerSides);
        DrawCircleLinesPercentage(center, outerRadius, f, lineInfo, outerRotDeg, outerSides);
    }
    public static void DrawRingLinesPercentage(Vector2 center, float innerRadius, float outerRadius, float f, float innerRotDeg, float outerRotDeg, int innerSides, int outerSides, LineDrawingInfo innerLineInfo, LineDrawingInfo outerLineInfo)
    {
        DrawCircleLinesPercentage(center, innerRadius, f, innerLineInfo, innerRotDeg, innerSides);
        DrawCircleLinesPercentage(center, outerRadius, f, outerLineInfo, outerRotDeg, outerSides);
    }
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, int sides, LineDrawingInfo lineInfo)
    {
        DrawCircleLines(center, innerRadius, lineInfo, sides);
        DrawCircleLines(center, outerRadius, lineInfo, sides);
    }
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, int innerSides, int outerSides, LineDrawingInfo lineInfo)
    {
        DrawCircleLines(center, innerRadius, lineInfo, innerSides);
        DrawCircleLines(center, outerRadius, lineInfo, outerSides);
    }
    public static void DrawRingLinesPercentage(Vector2 center, float innerRadius, float outerRadius, float f, int innerSides, int outerSides, LineDrawingInfo lineInfo)
    {
        DrawCircleLinesPercentage(center, innerRadius, f, lineInfo, innerSides);
        DrawCircleLinesPercentage(center, outerRadius, f, lineInfo, outerSides);
    }
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, int innerSides, int outerSides, LineDrawingInfo innerLineInfo, LineDrawingInfo outerLineInfo)
    {
        DrawCircleLines(center, innerRadius, innerLineInfo, innerSides);
        DrawCircleLines(center, outerRadius, outerLineInfo, outerSides);
    }
    public static void DrawRingLinesPercentage(Vector2 center, float innerRadius, float outerRadius, float f, int innerSides, int outerSides, LineDrawingInfo innerLineInfo, LineDrawingInfo outerLineInfo)
    {
        DrawCircleLinesPercentage(center, innerRadius, f, innerLineInfo, innerSides);
        DrawCircleLinesPercentage(center, outerRadius, f, outerLineInfo, outerSides);
    }
    public static void DrawRing(Vector2 center, float innerRadius, float outerRadius, ColorRgba color, float sideLength = 8f)
    {
        DrawSectorRing(center, innerRadius, outerRadius, 0, 360, color, sideLength);
    }
    public static void DrawRing(Vector2 center, float innerRadius, float outerRadius, int sides, ColorRgba color)
    {
        DrawSectorRing(center, innerRadius, outerRadius, 0, 360, sides, color);
    }
    
    /// <summary>
    /// Draws a circle where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="center"></param>
    /// <param name="innerRadius"></param>
    /// <param name="outerRadius"></param>
    /// <param name="innerRotDeg">The rotation of the inner circle in degrees.</param>
    /// <param name="outerRotDeg">The rotation of the outer circle in degrees.</param>
    /// <param name="lineInfo">How to draw the lines.</param>
    /// <param name="sides">How many sides the circle should be drawn with.</param>
    /// <param name="sideScaleFactor">The scale factor for each side. 0f means no circle is drawn, 1f means normal circle is drawn,
    /// 0.5 means each side is half as long.</param>
    /// <param name="sideScaleOrigin">The point along the line to scale from in both directions.</param>
    public static void DrawRingLinesScaled(Vector2 center, float innerRadius, float outerRadius, float innerRotDeg, float outerRotDeg, int sides, LineDrawingInfo lineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0f) return;
        if (sideScaleFactor >= 1f)
        {
            DrawCircleLines(center, innerRadius, lineInfo, innerRotDeg, sides);
            DrawCircleLines(center, outerRadius, lineInfo, outerRotDeg, sides);
            return;
        }
        
        var angleStep = (2f * ShapeMath.PI) / sides;
        var rotRadInner = innerRotDeg * ShapeMath.DEGTORAD;
        var rotRadOuter = outerRotDeg * ShapeMath.DEGTORAD;
        
        for (int i = 0; i < sides; i++)
        {
            var nextIndexInner = (i + 1) % sides;
            var startInner = center + new Vector2(innerRadius, 0f).Rotate(rotRadInner + angleStep * i);
            var endInner = center + new Vector2(innerRadius, 0f).Rotate(rotRadInner + angleStep * nextIndexInner);
            
            DrawLine(startInner, endInner, lineInfo, sideScaleFactor, sideScaleOrigin);
            
            var nextIndexOuter = (i + 1) % sides;
            var startOuter = center + new Vector2(outerRadius, 0f).Rotate(rotRadOuter + angleStep * i);
            var endOuter = center + new Vector2(outerRadius, 0f).Rotate(rotRadOuter + angleStep * nextIndexOuter);
            
            DrawLine(startOuter, endOuter, lineInfo, sideScaleFactor, sideScaleOrigin);
            
        }
    }
    
    /// <summary>
    /// Draws a circle where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="center"></param>
    /// <param name="innerRadius"></param>
    /// <param name="outerRadius"></param>
    /// <param name="innerRotDeg">The rotation of the inner circle in degrees.</param>
    /// <param name="outerRotDeg">The rotation of the outer circle in degrees.</param>
    /// <param name="lineInfo">How to draw the lines.</param>
    /// <param name="innerSides">How many sides the inner circle should be drawn with.</param>
    /// <param name="outerSides">How many sides the outer circle should be drawn with.</param>
    /// <param name="sideScaleFactor">The scale factor for each side. 0f means no circle is drawn, 1f means normal circle is drawn,
    /// 0.5 means each side is half as long.</param>
    /// <param name="sideScaleOrigin">The point along the line to scale from in both directions.</param>
    public static void DrawRingLinesScaled(Vector2 center, float innerRadius, float outerRadius, float innerRotDeg, float outerRotDeg, int innerSides, int outerSides, LineDrawingInfo lineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0f) return;
        if (sideScaleFactor >= 1f)
        {
            DrawCircleLines(center, innerRadius, lineInfo, innerRotDeg, innerSides);
            DrawCircleLines(center, outerRadius, lineInfo, outerRotDeg, outerSides);
            return;
        }
        
        var innerAngleStep = (2f * ShapeMath.PI) / innerSides;
        var outerAngleStep = (2f * ShapeMath.PI) / outerSides;
        var innerRotRad = innerRotDeg * ShapeMath.DEGTORAD;
        var outerRotRad = outerRotDeg * ShapeMath.DEGTORAD;

        int maxSides = innerSides > outerSides ? innerSides : outerSides;
        
        for (int i = 0; i < maxSides; i++)
        {
            if (i < innerSides)
            {
                var nextIndexInner = (i + 1) % innerSides;
                var startInner = center + new Vector2(innerRadius, 0f).Rotate(innerRotRad + innerAngleStep * i);
                var endInner = center + new Vector2(innerRadius, 0f).Rotate(innerRotRad + innerAngleStep * nextIndexInner);
                DrawLine(startInner, endInner, lineInfo, sideScaleFactor, sideScaleOrigin);
            }
            
            if (i < outerSides)
            {
                var nextIndexOuter = (i + 1) % outerSides;
                var startOuter = center + new Vector2(outerRadius, 0f).Rotate(outerRotRad + outerAngleStep * i);
                var endOuter = center + new Vector2(outerRadius, 0f).Rotate(outerRotRad + outerAngleStep * nextIndexOuter);
                DrawLine(startOuter, endOuter, lineInfo, sideScaleFactor, sideScaleOrigin);
            }
            
        }
    }

    /// <summary>
    /// Draws a circle where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="center"></param>
    /// <param name="innerRadius"></param>
    /// <param name="outerRadius"></param>
    /// <param name="innerRotDeg">The rotation of the inner circle in degrees.</param>
    /// <param name="outerRotDeg">The rotation of the outer circle in degrees.</param>
    /// <param name="innerLineInfo">How to draw the inner lines.</param>
    /// <param name="outerLineInfo">How to draw the outer lines.</param>
    /// <param name="innerSides">How many sides the inner circle should be drawn with.</param>
    /// <param name="outerSides">How many sides the outer circle should be drawn with.</param>
    /// <param name="sideScaleFactor">The scale factor for each side. 0f means no circle is drawn, 1f means normal circle is drawn,
    /// 0.5 means each side is half as long.</param>
    /// <param name="sideScaleOrigin">The point along the line to scale from in both directions.</param>
    public static void DrawRingLinesScaled(Vector2 center, float innerRadius, float outerRadius, float innerRotDeg, float outerRotDeg, int innerSides, int outerSides, LineDrawingInfo innerLineInfo, LineDrawingInfo outerLineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0f) return;
        if (sideScaleFactor >= 1f)
        {
            DrawCircleLines(center, innerRadius, innerLineInfo, innerRotDeg, innerSides);
            DrawCircleLines(center, outerRadius, outerLineInfo, outerRotDeg, outerSides);
            return;
        }
        
        var innerAngleStep = (2f * ShapeMath.PI) / innerSides;
        var outerAngleStep = (2f * ShapeMath.PI) / outerSides;
        var innerRotRad = innerRotDeg * ShapeMath.DEGTORAD;
        var outerRotRad = outerRotDeg * ShapeMath.DEGTORAD;

        int maxSides = innerSides > outerSides ? innerSides : outerSides;
        
        for (int i = 0; i < maxSides; i++)
        {
            if (i < innerSides)
            {
                var nextIndexInner = (i + 1) % innerSides;
                var startInner = center + new Vector2(innerRadius, 0f).Rotate(innerRotRad + innerAngleStep * i);
                var endInner = center + new Vector2(innerRadius, 0f).Rotate(innerRotRad + innerAngleStep * nextIndexInner);
                DrawLine(startInner, endInner, innerLineInfo, sideScaleFactor, sideScaleOrigin);
            }
            
            if (i < outerSides)
            {
                var nextIndexOuter = (i + 1) % outerSides;
                var startOuter = center + new Vector2(outerRadius, 0f).Rotate(outerRotRad + outerAngleStep * i);
                var endOuter = center + new Vector2(outerRadius, 0f).Rotate(outerRotRad + outerAngleStep * nextIndexOuter);
                DrawLine(startOuter, endOuter, outerLineInfo, sideScaleFactor, sideScaleOrigin);
            }
            
        }
    }
    
    /// <summary>
    /// Draws a circle where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="center"></param>
    /// <param name="innerRadius"></param>
    /// <param name="outerRadius"></param>
    /// <param name="innerRotDeg">The rotation of the inner circle in degrees.</param>
    /// <param name="outerRotDeg">The rotation of the outer circle in degrees.</param>
    /// <param name="innerLineInfo">How to draw the inner lines.</param>
    /// <param name="outerLineInfo">How to draw the outer lines.</param>
    /// <param name="innerSides">How many sides the inner circle should be drawn with.</param>
    /// <param name="outerSides">How many sides the outer circle should be drawn with.</param>
    /// <param name="innerSideScaleFactor">The scale factor for each side on the inner circle. 0f means no circle is drawn, 1f means normal circle is drawn,
    /// 0.5 means each side is half as long.</param>
    /// <param name="outerSideScaleFactor">The scale factor for each side on the outer circle. 0f means no circle is drawn, 1f means normal circle is drawn,
    /// 0.5 means each side is half as long.</param>
    /// <param name="innerSideScaleOrigin">The point along the line to scale from in both directions.</param>
    /// <param name="outerSideScaleOrigin">The point along the line to scale from in both directions.</param>
    public static void DrawRingLinesScaled(Vector2 center, float innerRadius, float outerRadius, float innerRotDeg, float outerRotDeg, int innerSides, int outerSides, LineDrawingInfo innerLineInfo, LineDrawingInfo outerLineInfo, float innerSideScaleFactor, float outerSideScaleFactor, float innerSideScaleOrigin, float outerSideScaleOrigin)
    {
        bool drawInner = true;
        bool drawOuter = true;
        if (innerSideScaleFactor >= 1f)
        {
            drawInner = false;
            DrawCircleLines(center, innerRadius, innerLineInfo, innerRotDeg, innerSides);
        }
        if (outerSideScaleFactor >= 1f)
        {
            drawOuter = false;
            DrawCircleLines(center, outerRadius, outerLineInfo, outerRotDeg, outerSides);
        }

        if (!drawInner && !drawOuter) return;
        
        var innerAngleStep = (2f * ShapeMath.PI) / innerSides;
        var outerAngleStep = (2f * ShapeMath.PI) / outerSides;
        var innerRotRad = innerRotDeg * ShapeMath.DEGTORAD;
        var outerRotRad = outerRotDeg * ShapeMath.DEGTORAD;

        int maxSides;
        if (!drawInner) maxSides = outerSides;
        else if (!drawOuter) maxSides = innerSides;
        else maxSides = innerSides > outerSides ? innerSides : outerSides;
        
        for (int i = 0; i < maxSides; i++)
        {
            if (drawInner && i < innerSides)
            {
                var nextIndexInner = (i + 1) % innerSides;
                var startInner = center + new Vector2(innerRadius, 0f).Rotate(innerRotRad + innerAngleStep * i);
                var endInner = center + new Vector2(innerRadius, 0f).Rotate(innerRotRad + innerAngleStep * nextIndexInner);
                DrawLine(startInner, endInner, innerLineInfo, innerSideScaleFactor, innerSideScaleOrigin);
            }
            
            if (drawOuter && i < outerSides)
            {
                var nextIndexOuter = (i + 1) % outerSides;
                var startOuter = center + new Vector2(outerRadius, 0f).Rotate(outerRotRad + outerAngleStep * i);
                var endOuter = center + new Vector2(outerRadius, 0f).Rotate(outerRotRad + outerAngleStep * nextIndexOuter);
                DrawLine(startOuter, endOuter, outerLineInfo, outerSideScaleFactor, outerSideScaleOrigin);
            }
            
        }
    }

    #endregion

    #region Sector Ring

    public static void DrawSectorRingLines(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float lineThickness, ColorRgba color, float sideLength = 8f)
    {
        DrawCircleSectorLines(center, innerRadius, startAngleDeg, endAngleDeg, lineThickness, color, false, sideLength);
        DrawCircleSectorLines(center, outerRadius, startAngleDeg, endAngleDeg, lineThickness, color, false, sideLength);

        float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
        float endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
        var innerStart = center + (ShapeVec.Right() * innerRadius - new Vector2(lineThickness / 2, 0)).Rotate(startAngleRad);
        var outerStart = center + (ShapeVec.Right() * outerRadius + new Vector2(lineThickness / 2, 0)).Rotate(startAngleRad);
        DrawLine(innerStart, outerStart, lineThickness, color, LineCapType.CappedExtended, 2);

        var innerEnd = center + (ShapeVec.Right() * innerRadius - new Vector2(lineThickness / 2, 0)).Rotate(endAngleRad);
        var outerEnd = center + (ShapeVec.Right() * outerRadius + new Vector2(lineThickness / 2, 0)).Rotate(endAngleRad);
        DrawLine(innerEnd, outerEnd, lineThickness, color, LineCapType.CappedExtended, 2);
    }
    public static void DrawSectorRingLines(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, float lineThickness, ColorRgba color, float sideLength = 8f)
    {
        DrawSectorRingLines(center, innerRadius, outerRadius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, lineThickness, color, sideLength);
    }
    public static void DrawSectorRingLines(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, LineDrawingInfo lineInfo, float sideLength = 8f)
    {
        DrawCircleSectorLines(center, innerRadius, startAngleDeg, endAngleDeg, lineInfo, false, sideLength);
        DrawCircleSectorLines(center, outerRadius, startAngleDeg, endAngleDeg, lineInfo, false, sideLength);

        float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
        float endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
        var innerStart = center + (ShapeVec.Right() * innerRadius - new Vector2(lineInfo.Thickness / 2, 0)).Rotate(startAngleRad);
        var outerStart = center + (ShapeVec.Right() * outerRadius + new Vector2(lineInfo.Thickness / 2, 0)).Rotate(startAngleRad);
        DrawLine(innerStart, outerStart, lineInfo);

        var innerEnd = center + (ShapeVec.Right() * innerRadius - new Vector2(lineInfo.Thickness / 2, 0)).Rotate(endAngleRad);
        var outerEnd = center + (ShapeVec.Right() * outerRadius + new Vector2(lineInfo.Thickness / 2, 0)).Rotate(endAngleRad);
        DrawLine(innerEnd, outerEnd, lineInfo);
    }
    public static void DrawSectorRingLines(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, LineDrawingInfo lineInfo, float sideLength = 8f)
    {
        DrawSectorRingLines(center, innerRadius, outerRadius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, lineInfo, sideLength);
    }

    public static void DrawSectorRing(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, ColorRgba color, float sideLength = 10f)
    {
        float start = TransformAngleDegToRaylib(startAngleDeg);
        float end = TransformAngleDegToRaylib(endAngleDeg);
        float anglePiece = end - start;
        int sides = GetCircleArcSideCount(outerRadius, MathF.Abs(anglePiece), sideLength);
        Raylib.DrawRing(center, innerRadius, outerRadius, start, end, sides, color.ToRayColor());
    }
    public static void DrawSectorRing(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, int sides, ColorRgba color)
    {
        Raylib.DrawRing(center, innerRadius, outerRadius, TransformAngleDegToRaylib(startAngleDeg), TransformAngleDegToRaylib(endAngleDeg), sides, color.ToRayColor());
    }
    public static void DrawSectorRing(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, ColorRgba color, float sideLength = 10f)
    {
        DrawSectorRing(center, innerRadius, outerRadius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, color, sideLength);
    }
    public static void DrawSectorRing(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, int sides, ColorRgba color)
    {
        DrawSectorRing(center, innerRadius, outerRadius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, sides, color);
    }

    #endregion
    
    #region Rectangle

    public static void Draw(this NinePatchRect npr, ColorRgba color)
    {
        var rects = npr.Rects;
        foreach (var r in rects)
        {
            r.Draw(color);
        }
    }
    public static void Draw(this NinePatchRect npr, ColorRgba sourceColorRgba, ColorRgba patchColorRgba)
    {
        npr.Source.Draw(sourceColorRgba);
        var rects = npr.Rects;
        foreach (var r in rects)
        {
            r.Draw(patchColorRgba);
        }
    }
   
    public static void DrawLines(this NinePatchRect npr, float lineThickness, ColorRgba color)
    {
        var rects = npr.Rects;
        foreach (var r in rects)
        {
            r.DrawLines(lineThickness, color);
        }
    }
    public static void DrawLines(this NinePatchRect npr, float sourceLineThickness, float patchLineThickness, ColorRgba sourceColorRgba, ColorRgba patchColorRgba)
    {
        npr.Source.DrawLines(sourceLineThickness, sourceColorRgba);
        var rects = npr.Rects;
        foreach (var r in rects)
        {
            r.DrawLines(patchLineThickness, patchColorRgba);
        }
    }

    public static void Draw(this Grid grid, Rect bounds, float lineThickness, ColorRgba color)
    {
        Vector2 rowSpacing = new(0f, bounds.Height / grid.Rows);
        for (int row = 0; row < grid.Rows + 1; row++)
        {
            DrawLine(bounds.TopLeft + rowSpacing * row, bounds.TopRight + rowSpacing * row, lineThickness, color);
        }
        Vector2 colSpacing = new(bounds.Width / grid.Cols, 0f);
        for (int col = 0; col < grid.Cols + 1; col++)
        {
            DrawLine(bounds.TopLeft + colSpacing * col, bounds.BottomLeft + colSpacing * col, lineThickness, color);
        }
    }
    
    public static void DrawGrid(this Rect r, int lines, LineDrawingInfo lineInfo)
    {
        var xOffset = new Vector2(r.Width / lines, 0f);// * i;
        var yOffset = new Vector2(0f, r.Height / lines);// * i;
 
        var tl = r.TopLeft;
        var tr = tl + new Vector2(r.Width, 0);
        var bl = tl + new Vector2(0, r.Height);

        for (var i = 0; i < lines; i++)
        {
            DrawLine(tl + xOffset * i, bl + xOffset * i, lineInfo);
            DrawLine(tl + yOffset * i, tr + yOffset * i, lineInfo);
        }
    }

    
    public static void DrawRect(Vector2 topLeft, Vector2 bottomRight, ColorRgba color)
    {
        Raylib.DrawRectangleV(topLeft, bottomRight - topLeft, color.ToRayColor());
        // Raylib.DrawRectangleRec(new Rect(topLeft, bottomRight).Rectangle, color.ToRayColor());
    }
    public static void DrawRect(Vector2 topLeft, Vector2 bottomRight, Vector2 pivot, float rotDeg, ColorRgba color)
    {
        var a = pivot + (topLeft - pivot).RotateDeg(rotDeg);
        var b = pivot + (new Vector2(topLeft.X, bottomRight.Y) - pivot).RotateDeg(rotDeg);
        var c = pivot + (bottomRight - pivot).RotateDeg(rotDeg);
        var d = pivot + (new Vector2(bottomRight.X, topLeft.Y) -pivot).RotateDeg(rotDeg);
        DrawQuad(a,b,c,d, color);
        
        // Draw(new Rect(topLeft, bottomRight), pivot, rotDeg, color);
    }
    
    
    public static void DrawRectLines(Vector2 topLeft, Vector2 bottomRight, float lineThickness, ColorRgba color) => DrawLines(new Rect(topLeft, bottomRight),lineThickness,color);
    public static void DrawRectLines(Vector2 topLeft, Vector2 bottomRight, LineDrawingInfo lineInfo)
    {
        DrawLines(new Rect(topLeft, bottomRight), lineInfo);
    }
    public static void DrawRectLines(Vector2 topLeft, Vector2 bottomRight, Vector2 pivot, float rotDeg, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.Extended, int capPoints = 0)
    {
        var a = pivot + (topLeft - pivot).RotateDeg(rotDeg);
        var b = pivot + (new Vector2(topLeft.X, bottomRight.Y) - pivot).RotateDeg(rotDeg);
        var c = pivot + (bottomRight - pivot).RotateDeg(rotDeg);
        var d = pivot + (new Vector2(bottomRight.X, topLeft.Y) -pivot).RotateDeg(rotDeg);
        DrawQuadLines(a,b,c,d, lineThickness, color, capType, capPoints);
    }
    public static void DrawRectLines(Vector2 topLeft, Vector2 bottomRight, Vector2 pivot, float rotDeg, LineDrawingInfo lineInfo)
        => DrawRectLines(topLeft, bottomRight, pivot, rotDeg, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
    
    
    public static void Draw(this Rect rect, ColorRgba color) => Raylib.DrawRectangleRec(rect.Rectangle, color.ToRayColor());
    public static void Draw(this Rect rect, Vector2 pivot, float rotDeg, ColorRgba color) => DrawRect(rect.TopLeft, rect.BottomRight, pivot, rotDeg, color);
    
    
    public static void DrawLines(this Rect rect, float lineThickness, ColorRgba color) => Raylib.DrawRectangleLinesEx(rect.Rectangle, lineThickness * 2, color.ToRayColor());
    public static void DrawLines(this Rect rect, LineDrawingInfo lineInfo)
    {
        DrawLine(rect.TopLeft, rect.BottomLeft, lineInfo);
        DrawLine(rect.BottomLeft, rect.BottomRight, lineInfo);
        DrawLine(rect.BottomRight, rect.TopRight, lineInfo);
        DrawLine(rect.TopRight, rect.TopLeft, lineInfo);
    }
    public static void DrawLines(this Rect rect, Vector2 pivot, float rotDeg, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.Extended, int capPoints = 0)
    {
        DrawRectLines(rect.TopLeft, rect.BottomRight, pivot, rotDeg, lineThickness, color, capType, capPoints);
    }
    public static void DrawLines(this Rect rect, Vector2 pivot, float rotDeg, LineDrawingInfo lineInfo)
    {
        DrawRectLines(rect.TopLeft, rect.BottomRight, pivot, rotDeg, lineInfo);
    }
    
    
    /// <summary>
    /// Draws a certain percentage of an outline.
    /// </summary>
    /// <param name="topLeft"></param>
    /// <param name="bottomRight"></param>
    /// <param name="f">The percentage of the outline to draw. Negative value reverses the direction (cw).
    /// Integer part can be used to change starting corner.
    /// 0.35 would start at the top left corner go in ccw direction and draw 35% of the outline.
    /// -2.7 would start at the bottom left corner (the third corner in cw direction) and draw in cw direction 70% of the outline.</param>
    /// <param name="lineThickness"></param>
    /// <param name="color"></param>
    /// <param name="capType"></param>
    /// <param name="capPoints"></param>
    public static void DrawRectLinesPercentage(Vector2 topLeft, Vector2 bottomRight, float f, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (f == 0) return;
        var r = new Rect(topLeft, bottomRight);
        if(r.Width <= 0 || r.Height <= 0) return;

        bool negative = false;
        if (f < 0)
        {
            negative = true;
            f *= -1;
        }
        
        int startCorner = (int)f;
        float percentage = f - startCorner;
        if (percentage <= 0) return;
        
        startCorner = ShapeMath.Clamp(startCorner, 0, 3);

        var perimeter = r.Width * 2 + r.Height * 2;
        var perimeterToDraw = perimeter * percentage;
        
        if (startCorner == 0)
        {
            if (negative)
            {
               DrawRectLinesPercentageHelper(r.TopLeft, r.TopRight, r.BottomRight, r.BottomLeft, perimeterToDraw, r.Width, r.Height, lineThickness, color, capType, capPoints);
            }
            else
            {
                DrawRectLinesPercentageHelper(r.TopLeft, r.BottomLeft, r.BottomRight, r.TopRight, perimeterToDraw, r.Height, r.Width, lineThickness, color, capType, capPoints);
            }
        }
        else if (startCorner == 1)
        {
            if (negative)
            {
                DrawRectLinesPercentageHelper(r.TopRight, r.BottomRight, r.BottomLeft, r.TopLeft, perimeterToDraw, r.Height, r.Width, lineThickness, color, capType, capPoints);
            }
            else
            {
                DrawRectLinesPercentageHelper(r.BottomLeft, r.BottomRight, r.TopRight, r.TopLeft, perimeterToDraw, r.Width, r.Height, lineThickness, color, capType, capPoints);
            }
        }
        else if (startCorner == 2)
        {
            if (negative)
            {
                DrawRectLinesPercentageHelper(r.BottomRight, r.BottomLeft, r.TopLeft, r.TopRight, perimeterToDraw, r.Width, r.Height, lineThickness, color, capType, capPoints);
            }
            else
            {
                DrawRectLinesPercentageHelper(r.BottomRight, r.TopRight, r.TopLeft, r.BottomLeft, perimeterToDraw, r.Height, r.Width, lineThickness, color, capType, capPoints);
            }
        }
        else if (startCorner == 3)
        {
            if (negative)
            {
                DrawRectLinesPercentageHelper(r.BottomLeft, r.TopLeft, r.TopRight, r.BottomRight, perimeterToDraw, r.Height, r.Width, lineThickness, color, capType, capPoints);
            }
            else
            {
                DrawRectLinesPercentageHelper(r.TopRight, r.TopLeft, r.BottomLeft, r.BottomRight, perimeterToDraw, r.Width, r.Height, lineThickness, color, capType, capPoints);
            }
        }
        
    }
    
    /// <summary>
    /// Draws a certain percentage of an outline.
    /// </summary>
    /// <param name="topLeft"></param>
    /// <param name="bottomRight"></param>
    /// <param name="f">The percentage of the outline to draw. Negative value reverses the direction (cw).
    /// Integer part can be used to change starting corner.
    /// 0.35 would start at the top left corner go in ccw direction and draw 35% of the outline.
    /// -2.7 would start at the bottom left corner (the third corner in cw direction) and draw in cw direction 70% of the outline.</param>
    /// <param name="pivot"></param>
    /// <param name="rotDeg"></param>
    /// <param name="lineThickness"></param>
    /// <param name="color"></param>
    /// <param name="capType"></param>
    /// <param name="capPoints"></param>
    public static void DrawRectLinesPercentage(Vector2 topLeft, Vector2 bottomRight, float f, Vector2 pivot, float rotDeg, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.Extended, int capPoints = 0)
    {
        var a = pivot + (topLeft - pivot).RotateDeg(rotDeg);
        var b = pivot + (new Vector2(topLeft.X, bottomRight.Y) - pivot).RotateDeg(rotDeg);
        var c = pivot + (bottomRight - pivot).RotateDeg(rotDeg);
        var d = pivot + (new Vector2(bottomRight.X, topLeft.Y) -pivot).RotateDeg(rotDeg);
        
        DrawQuadLinesPercentage(a,b,c,d, f, lineThickness, color, capType, capPoints);
    }
    
    /// <summary>
    /// Draws a certain percentage of an outline.
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="f">The percentage of the outline to draw. Negative value reverses the direction (cw).
    /// Integer part can be used to change starting corner.
    /// 0.35 would start at the top left corner go in ccw direction and draw 35% of the outline.
    /// -2.7 would start at the bottom left corner (the third corner in cw direction) and draw in cw direction 70% of the outline.</param>
    /// <param name="pivot"></param>
    /// <param name="rotDeg"></param>
    /// <param name="lineThickness"></param>
    /// <param name="color"></param>
    /// <param name="capType"></param>
    /// <param name="capPoints"></param>
    public static void DrawLinesPercentage(this Rect rect, float f, Vector2 pivot, float rotDeg, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.Extended, int capPoints = 0)
    {
        DrawRectLinesPercentage(rect.TopLeft, rect.BottomRight, f, pivot, rotDeg, lineThickness, color, capType, capPoints);
    }

    /// <summary>
    /// Draws a certain percentage of an outline.
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="f">The percentage of the outline to draw. Negative value reverses the direction (cw).
    /// Integer part can be used to change starting corner.
    /// 0.35 would start at the top left corner go in ccw direction and draw 35% of the outline.
    /// -2.7 would start at the bottom left corner (the third corner in cw direction) and draw in cw direction 70% of the outline.</param>
    /// <param name="pivot"></param>
    /// <param name="rotDeg"></param>
    /// <param name="lineInfo"></param>
    public static void DrawLinesPercentage(this Rect rect, float f, Vector2 pivot, float rotDeg, LineDrawingInfo lineInfo)
    {
        DrawRectLinesPercentage(rect.TopLeft, rect.BottomRight, f, pivot, rotDeg, lineInfo);
    }
   
    /// <summary>
    /// Draws a certain percentage of an outline.
    /// </summary>
    /// <param name="topLeft"></param>
    /// <param name="bottomRight"></param>
    /// <param name="f">The percentage of the outline to draw. Negative value reverses the direction (cw).
    /// Integer part can be used to change starting corner.
    /// 0.35 would start at the top left corner go in ccw direction and draw 35% of the outline.
    /// -2.7 would start at the bottom left corner (the third corner in cw direction) and draw in cw direction 70% of the outline.</param>
    /// <param name="pivot"></param>
    /// <param name="rotDeg"></param>
    /// <param name="lineInfo"></param>
    public static void DrawRectLinesPercentage(Vector2 topLeft, Vector2 bottomRight, float f, Vector2 pivot, float rotDeg, LineDrawingInfo lineInfo)
        => DrawRectLinesPercentage(topLeft, bottomRight, f, pivot, rotDeg, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
    
    
    /// <summary>
    /// Draws a rect where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="r">The rect to draw.</param>
    /// <param name="lineInfo">How to draw the lines.</param>
    /// <param name="rotDeg">The rotation of the rect.</param>
    /// <param name="pivot">Point to rotate the rect around.</param>
    /// <param name="sideScaleFactor">The scale factor for each side. 0f means no rect is drawn, 1f means normal rect is drawn,
    /// 0.5 means each side is half as long.</param>
    /// <param name="sideScaleOrigin">The point along the line to scale from in both directions.</param>
    public static void DrawLinesScaled(this Rect r, LineDrawingInfo lineInfo, float rotDeg, Vector2 pivot, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0f) return;
        if (sideScaleFactor >= 1f)
        {
            r.DrawLines(pivot, rotDeg, lineInfo);
            return;
        }
        if (rotDeg == 0f)
        {
            DrawLine(r.TopLeft, r.BottomLeft, lineInfo, sideScaleFactor, sideScaleOrigin);
            DrawLine(r.BottomLeft, r.BottomRight, lineInfo, sideScaleFactor, sideScaleOrigin);
            DrawLine(r.BottomRight, r.TopRight, lineInfo, sideScaleFactor, sideScaleOrigin);
            DrawLine(r.TopRight, r.TopLeft, lineInfo, sideScaleFactor, sideScaleOrigin);
        }
        else
        {
            var corners = r.RotateCorners(pivot, rotDeg);
            DrawLine(corners.tl, corners.bl, lineInfo, sideScaleFactor, sideScaleOrigin);
            DrawLine(corners.bl, corners.br, lineInfo, sideScaleFactor, sideScaleOrigin);
            DrawLine(corners.br, corners.tr, lineInfo, sideScaleFactor, sideScaleOrigin);
            DrawLine(corners.tr, corners.tl, lineInfo, sideScaleFactor, sideScaleOrigin);
        }
    }
    
    
    public static void DrawVertices(this Rect rect, float vertexRadius, ColorRgba color, int circleSegments = 8)
    {
        DrawCircle(rect.TopLeft, vertexRadius, color    , circleSegments);
        DrawCircle(rect.TopRight, vertexRadius, color   , circleSegments);
        DrawCircle(rect.BottomLeft, vertexRadius, color , circleSegments);
        DrawCircle(rect.BottomRight, vertexRadius, color, circleSegments);
    }
    
    
    public static void DrawRounded(this Rect rect, float roundness, int segments, ColorRgba color) => Raylib.DrawRectangleRounded(rect.Rectangle, roundness, segments, color.ToRayColor());
    public static void DrawRoundedLines(this Rect rect, float roundness, float lineThickness, int segments, ColorRgba color) => Raylib.DrawRectangleRoundedLines(rect.Rectangle, roundness, segments, lineThickness * 2, color.ToRayColor());
    
    
    public static void DrawSlantedCorners(this Rect rect, ColorRgba color, float tlCorner, float trCorner, float brCorner, float blCorner)
    {
        var points = rect.GetSlantedCornerPoints(tlCorner, trCorner, brCorner, blCorner);
        points.DrawPolygonConvex(rect.Center, color);
    }
    public static void DrawSlantedCorners(this Rect rect, Vector2 pivot, float rotDeg, ColorRgba color, float tlCorner, float trCorner, float brCorner, float blCorner)
    {
        var poly = rect.GetSlantedCornerPoints(tlCorner, trCorner, brCorner, blCorner);
        poly.ChangeRotation(rotDeg * ShapeMath.DEGTORAD, pivot);
        poly.DrawPolygonConvex(rect.Center, color);
        //DrawPolygonConvex(poly, rect.Center, color);
        //var points = SPoly.Rotate(GetSlantedCornerPoints(rect, tlCorner, trCorner, brCorner, blCorner), pivot, rotDeg * SUtils.DEGTORAD);
        //DrawPolygonConvex(points, rect.Center, color);
    }
    
    
    public static void DrawSlantedCornersLines(this Rect rect, LineDrawingInfo lineInfo, float tlCorner, float trCorner, float brCorner, float blCorner)
    {
        var points = rect.GetSlantedCornerPoints(tlCorner, trCorner, brCorner, blCorner);
        points.DrawLines(lineInfo);
    }
    public static void DrawSlantedCornersLines(this Rect rect, Vector2 pivot, float rotDeg, LineDrawingInfo lineInfo, float tlCorner, float trCorner, float brCorner, float blCorner)
    {
        var poly = rect.GetSlantedCornerPoints(tlCorner, trCorner, brCorner, blCorner);
        poly.ChangeRotation(rotDeg * ShapeMath.DEGTORAD, pivot);
        poly.DrawLines(lineInfo);
    }
    
    
    public static void DrawCorners(this Rect rect, LineDrawingInfo lineInfo, float tlCorner, float trCorner, float brCorner, float blCorner)
    {
        var tl = rect.TopLeft;
        var tr = rect.TopRight;
        var br = rect.BottomRight;
        var bl = rect.BottomLeft;

        if (tlCorner > 0f)
        {
            //DrawCircle(tl, lineThickness / 2, color);
            DrawLine(tl, tl + new Vector2(MathF.Min(tlCorner, rect.Width), 0f), lineInfo);
            DrawLine(tl, tl + new Vector2(0f, MathF.Min(tlCorner, rect.Height)), lineInfo);
        }
        if (trCorner > 0f)
        {
            //DrawCircle(tr, lineThickness / 2, color);
            DrawLine(tr, tr - new Vector2(MathF.Min(trCorner, rect.Width), 0f), lineInfo);
            DrawLine(tr, tr + new Vector2(0f, MathF.Min(trCorner, rect.Height)), lineInfo);
        }
        if (brCorner > 0f)
        {
            //DrawCircle(br, lineThickness / 2, color);
            DrawLine(br, br - new Vector2(MathF.Min(brCorner, rect.Width), 0f), lineInfo);
            DrawLine(br, br - new Vector2(0f, MathF.Min(brCorner, rect.Height)), lineInfo);
        }
        if (blCorner > 0f)
        {
            //DrawCircle(bl, lineThickness / 2, color);
            DrawLine(bl, bl + new Vector2(MathF.Min(blCorner, rect.Width), 0f), lineInfo);
            DrawLine(bl, bl - new Vector2(0f, MathF.Min(blCorner, rect.Height)), lineInfo);
        }
    }
    public static void DrawCorners(this Rect rect, LineDrawingInfo lineInfo, float cornerLength)
        => DrawCorners(rect, lineInfo, cornerLength, cornerLength, cornerLength, cornerLength);
    
    
    public static void DrawCornersRelative(this Rect rect, LineDrawingInfo lineInfo, float tlCorner, float trCorner, float brCorner, float blCorner)
    {
        var tl = rect.TopLeft;
        var tr = rect.TopRight;
        var br = rect.BottomRight;
        var bl = rect.BottomLeft;

        if (tlCorner > 0f && tlCorner < 1f)
        {
            // DrawCircle(tl, lineThickness / 2, color);
            DrawLine(tl, tl + new Vector2(tlCorner * rect.Width, 0f), lineInfo);
            DrawLine(tl, tl + new Vector2(0f, tlCorner * rect.Height), lineInfo);
        }
        if (trCorner > 0f && trCorner < 1f)
        {
            // DrawCircle(tr, lineThickness / 2, color);
            DrawLine(tr, tr - new Vector2(tlCorner * rect.Width, 0f), lineInfo);
            DrawLine(tr, tr + new Vector2(0f, tlCorner * rect.Height), lineInfo);
        }
        if (brCorner > 0f && brCorner < 1f)
        {
            // DrawCircle(br, lineThickness / 2, color);
            DrawLine(br, br - new Vector2(tlCorner * rect.Width, 0f), lineInfo);
            DrawLine(br, br - new Vector2(0f, tlCorner * rect.Height), lineInfo);
        }
        if (blCorner > 0f && blCorner < 1f)
        {
            // DrawCircle(bl, lineThickness / 2, color);
            DrawLine(bl, bl + new Vector2(tlCorner * rect.Width, 0f), lineInfo);
            DrawLine(bl, bl - new Vector2(0f, tlCorner * rect.Height), lineInfo);
        }
    }
    public static void DrawCornersRelative(this Rect rect, LineDrawingInfo lineInfo, float cornerLengthFactor) 
        => DrawCornersRelative(rect, lineInfo, cornerLengthFactor, cornerLengthFactor, cornerLengthFactor, cornerLengthFactor);
    
    
    public static void DrawCheckered(this Rect rect, float spacing, float angleDeg, LineDrawingInfo checkered, LineDrawingInfo outline, ColorRgba bgColor)
    {
        var size = new Vector2(rect.Width, rect.Height);
        var center = new Vector2(rect.X, rect.Y) + size / 2;
        float maxDimension = MathF.Max(size.X, size.Y);
        float rotRad = angleDeg * ShapeMath.DEGTORAD;

        //var tl = new Vector2(rect.X, rect.Y);
        //var tr = new Vector2(rect.X + rect.Width, rect.Y);
        //var bl = new Vector2(rect.X, rect.Y + rect.Height);
        //var br = new Vector2(rect.X + rect.Width, rect.Y + rect.Height);

        if (bgColor.A > 0) rect.Draw(bgColor); 

        Vector2 cur = new(-spacing / 2, 0f);

        //safety for while loops
        int whileMaxCount = (int)(maxDimension / spacing) * 2;
        int whileCounter = 0;

        //left half of rectangle
        while (whileCounter < whileMaxCount)
        {
            var p = center + cur.Rotate(rotRad);
            var up = new Vector2(0f, -maxDimension * 2);//make sure that lines are going outside of the rectangle
            var down = new Vector2(0f, maxDimension * 2);
            var start = p + up.Rotate(rotRad);
            var end = p + down.Rotate(rotRad);
            var seg = new Segment(start, end);
            var collisionPoints = seg.IntersectShape(rect);

            
            
            if (collisionPoints != null && collisionPoints.Count >= 2) 
                DrawLine(collisionPoints[0].Point, collisionPoints[1].Point, checkered);
            else break;
            
            cur.X -= spacing;
            whileCounter++;
        }

        cur = new(spacing / 2, 0f);
        whileCounter = 0;
        //right half of rectangle
        while (whileCounter < whileMaxCount)
        {
            var p = center + ShapeVec.Rotate(cur, rotRad);
            var up = new Vector2(0f, -maxDimension * 2);
            var down = new Vector2(0f, maxDimension * 2);
            var start = p + ShapeVec.Rotate(up, rotRad);
            var end = p + ShapeVec.Rotate(down, rotRad);
            var seg = new Segment(start, end);
            var collisionPoints = seg.IntersectShape(rect); //SGeometry.IntersectionSegmentRect(center, start, end, tl, tr, br, bl).points;
            
            
            if (collisionPoints != null && collisionPoints.Count >= 2 ) 
                DrawLine(collisionPoints[0].Point, collisionPoints[1].Point, checkered);
            else break;
            cur.X += spacing;
            whileCounter++;
        }

        if (outline.Color.A > 0) DrawLines(rect, new Vector2(0.5f, 0.5f), 0f, outline);
    }

    #endregion

    #region Triangle
    public static void DrawTriangle(Vector2 a, Vector2 b, Vector2 c, ColorRgba color) => Raylib.DrawTriangle(a, b, c, color.ToRayColor());

    
    public static void DrawTriangleLines(Vector2 a, Vector2 b, Vector2 c, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        DrawLine(a, b, lineThickness, color, capType, capPoints);
        DrawLine(b, c, lineThickness, color, capType, capPoints);
        DrawLine(c, a, lineThickness, color, capType, capPoints);
        
        // new Triangle(a, b, c).GetEdges().Draw(lineThickness, color);
    }
    public static void DrawTriangleLines(Vector2 a, Vector2 b, Vector2 c, LineDrawingInfo lineInfo)
    {
        DrawLine(a, b, lineInfo);
        DrawLine(b, c, lineInfo);
        DrawLine(c, a, lineInfo);
        
    }

    
    public static void Draw(this Triangle t, ColorRgba color) => Raylib.DrawTriangle(t.A, t.B, t.C, color.ToRayColor());

    public static void DrawLines(this Triangle t, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        DrawTriangleLines(t.A, t.B, t.C, lineThickness, color, capType, capPoints);
        // t.GetEdges().Draw(lineThickness, color);
    }
    public static void DrawLines(this Triangle t, LineDrawingInfo lineInfo) => DrawTriangleLines(t.A, t.B, t.C, lineInfo);
    public static void DrawLines(this Triangle t, LineDrawingInfo lineInfo, float rotDeg, Vector2 rotOrigin)
    {
        t = t.ChangeRotation(rotDeg * ShapeMath.DEGTORAD, rotOrigin);
        DrawTriangleLines(t.A, t.B, t.C, lineInfo);
    }

    
    public static void DrawVertices(this Triangle t, float vertexRadius, ColorRgba color, int circleSegments = 8)
    {
        DrawCircle(t.A, vertexRadius, color, circleSegments);
        DrawCircle(t.B, vertexRadius, color, circleSegments);
        DrawCircle(t.C, vertexRadius, color, circleSegments);
    }
    
    public static void Draw(this Triangulation triangles, ColorRgba color) { foreach (var t in triangles) t.Draw(color); }

    
    public static void DrawLines(this Triangulation triangles, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        foreach (var t in triangles) t.DrawLines(lineThickness, color, capType, capPoints);
    }
    public static void DrawLines(this Triangulation triangles, LineDrawingInfo lineInfo)
    {
        foreach (var t in triangles) t.DrawLines(lineInfo);
    }

    /// <summary>
    /// Draws a certain percentage of an outline.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="f">The percentage of the outline to draw. Negative value reverses the direction (cw).
    /// Integer part can be used to change starting corner.
    /// 0.35 would start at corner a go in ccw direction and draw 35% of the outline.
    /// -2.7 would start at b (the third corner in cw direction) and draw in cw direction 70% of the outline.</param>
    /// <param name="lineThickness"></param>
    /// <param name="color"></param>
    /// <param name="capType"></param>
    /// <param name="capPoints"></param>
    public static void DrawTriangleLinesPercentage(Vector2 a, Vector2 b, Vector2 c, float f, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (f == 0) return;

        bool negative = false;
        if (f < 0)
        {
            negative = true;
            f *= -1;
        }
        
        int startCorner = (int)f;
        float percentage = f - startCorner;
        if (percentage <= 0) return;
        
        startCorner = ShapeMath.Clamp(startCorner, 0, 2);
        
        if (startCorner == 0)
        {
            if (negative)
            {
                DrawTriangleLinesPercentageHelper(a, c, b, percentage, lineThickness, color, capType, capPoints);
            }
            else
            {
                DrawTriangleLinesPercentageHelper(a, b, c, percentage, lineThickness, color, capType, capPoints);
            }
        }
        else if (startCorner == 1)
        {
            if (negative)
            {
                DrawTriangleLinesPercentageHelper(b, a, c,  percentage, lineThickness, color, capType, capPoints);
            }
            else
            {
                DrawTriangleLinesPercentageHelper(b, c, a,  percentage, lineThickness, color, capType, capPoints);
            }
        }
        else if (startCorner == 2)
        {
            if (negative)
            {
                DrawTriangleLinesPercentageHelper(c, b, a, percentage, lineThickness, color, capType, capPoints);
            }
            else
            {
                DrawTriangleLinesPercentageHelper(c, a, b, percentage, lineThickness, color, capType, capPoints);
            }
        }
    }
    /// <summary>
    /// Draws a certain percentage of an outline.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="f">The percentage of the outline to draw. Negative value reverses the direction (cw).
    /// Integer part can be used to change starting corner.
    /// 0.35 would start at corner a go in ccw direction and draw 35% of the outline.
    /// -2.7 would start at b (the third corner in cw direction) and draw in cw direction 70% of the outline.</param>
    /// <param name="lineInfo"></param>
    public static void DrawTriangleLinesPercentage(Vector2 a, Vector2 b, Vector2 c, float f, LineDrawingInfo lineInfo)
    {
        DrawTriangleLinesPercentage(a, b, c, f, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
    }
    
    /// <summary>
    /// Draws a certain percentage of an outline.
    /// </summary>
    /// <param name="t"></param>
    /// <param name="f">The percentage of the outline to draw. Negative value reverses the direction (cw).
    /// Integer part can be used to change starting corner.
    /// 0.35 would start at corner a go in ccw direction and draw 35% of the outline.
    /// -2.7 would start at b (the third corner in cw direction) and draw in cw direction 70% of the outline.</param>
    /// <param name="lineThickness"></param>
    /// <param name="color"></param>
    /// <param name="capType"></param>
    /// <param name="capPoints"></param>
    public static void DrawLinesPercentage(this Triangle t, float f, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        DrawTriangleLinesPercentage(t.A, t.B, t.C, f, lineThickness, color, capType, capPoints);
    }
    /// <summary>
    /// Draws a certain percentage of an outline.
    /// </summary>
    /// <param name="t"></param>
    /// <param name="f">The percentage of the outline to draw. Negative value reverses the direction (cw).
    /// Integer part can be used to change starting corner.
    /// 0.35 would start at corner a go in ccw direction and draw 35% of the outline.
    /// -2.7 would start at b (the third corner in cw direction) and draw in cw direction 70% of the outline.</param>
    /// <param name="lineInfo"></param>
    public static void DrawLinesPercentage(this Triangle t, float f, LineDrawingInfo lineInfo)
    {
        DrawTriangleLinesPercentage(t.A, t.B, t.C, f, lineInfo);
    }
    /// <summary>
    /// Draws a certain percentage of an outline.
    /// </summary>
    /// <param name="t"></param>
    /// <param name="f">The percentage of the outline to draw. Negative value reverses the direction (cw).
    /// Integer part can be used to change starting corner.
    /// 0.35 would start at corner a go in ccw direction and draw 35% of the outline.
    /// -2.7 would start at b (the third corner in cw direction) and draw in cw direction 70% of the outline.</param>
    /// <param name="lineInfo"></param>
    /// <param name="rotDeg"></param>
    /// <param name="rotOrigin"> Origin is in absolute space.</param>
    public static void DrawLinesPercentage(this Triangle t, float f, LineDrawingInfo lineInfo, float rotDeg, Vector2 rotOrigin)
    {
        t = t.ChangeRotation(rotDeg * ShapeMath.DEGTORAD, rotOrigin);
        DrawTriangleLinesPercentage(t.A, t.B, t.C, f, lineInfo);
    }

    
    
    /// <summary>
    /// Draws a rect where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="t">The triangle to draw.</param>
    /// <param name="lineInfo">How to draw the lines.</param>
    /// <param name="rotDeg">The rotation of the triangle.</param>
    /// <param name="rotOrigin">Point to rotate the triangle around.</param>
    /// <param name="sideScaleFactor">The scale factor for each side. 0f means no triangle is drawn, 1f means normal triangle is drawn,
    /// 0.5 means each side is half as long.</param>
    /// <param name="sideScaleOrigin">The point along the line to scale from in both directions.</param>
    public static void DrawLinesScaled(this Triangle t, LineDrawingInfo lineInfo, float rotDeg, Vector2 rotOrigin, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0) return;
        if (sideScaleFactor >= 1)
        {
            t.DrawLines(lineInfo, rotDeg, rotOrigin);
            return;
        }
        
        if(rotDeg != 0) t = t.ChangeRotation(rotDeg * ShapeMath.DEGTORAD, rotOrigin);
        
        DrawLine(t.A, t.B, lineInfo, sideScaleFactor, sideScaleOrigin);
        DrawLine(t.B, t.C, lineInfo, sideScaleFactor, sideScaleOrigin);
        DrawLine(t.C, t.A, lineInfo, sideScaleFactor, sideScaleOrigin);
        
    }

    #endregion

    #region Quad
    public static void DrawQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d, ColorRgba color)
    {
        Raylib.DrawTriangle(a, b, c, color.ToRayColor());
        Raylib.DrawTriangle(a, c, d, color.ToRayColor());
    }

    public static void DrawQuadLines(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        DrawLine(a, b, lineThickness, color, capType, capPoints);
        DrawLine(b, c, lineThickness, color, capType, capPoints);
        DrawLine(c, d, lineThickness, color, capType, capPoints);
        DrawLine(d, a, lineThickness, color, capType, capPoints);
    }
    
    /// <summary>
    /// Draws a certain percentage of an outline.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="d"></param>
    /// <param name="f">The percentage of the outline to draw. Negative value reverses the direction (cw).
    /// Integer part can be used to change starting corner.
    /// 0.35 would start at corner a go in ccw direction and draw 35% of the outline.
    /// -2.7 would start at b (the third corner in cw direction) and draw in cw direction 70% of the outline.</param>
    /// <param name="lineThickness"></param>
    /// <param name="color"></param>
    /// <param name="capType"></param>
    /// <param name="capPoints"></param>
    public static void DrawQuadLinesPercentage(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float f, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (f == 0) return;

        bool negative = false;
        if (f < 0)
        {
            negative = true;
            f *= -1;
        }
        
        int startCorner = (int)f;
        float percentage = f - startCorner;
        if (percentage <= 0) return;
        
        startCorner = ShapeMath.Clamp(startCorner, 0, 3);
        
        if (startCorner == 0)
        {
            if (negative)
            {
               DrawQuadLinesPercentageHelper(a, d, c, b, percentage, lineThickness, color, capType, capPoints);
            }
            else
            {
                DrawQuadLinesPercentageHelper(a, b, c, d, percentage, lineThickness, color, capType, capPoints);
            }
        }
        else if (startCorner == 1)
        {
            if (negative)
            {
                DrawQuadLinesPercentageHelper(d, c, b, a, percentage, lineThickness, color, capType, capPoints);
            }
            else
            {
                DrawQuadLinesPercentageHelper(b, c, d, a, percentage, lineThickness, color, capType, capPoints);
            }
        }
        else if (startCorner == 2)
        {
            if (negative)
            {
                DrawQuadLinesPercentageHelper(c, b, a, d, percentage, lineThickness, color, capType, capPoints);
            }
            else
            {
                DrawQuadLinesPercentageHelper(c, d, a, b, percentage, lineThickness, color, capType, capPoints);
            }
        }
        else if (startCorner == 3)
        {
            if (negative)
            {
                DrawQuadLinesPercentageHelper(b, a, d, c, percentage, lineThickness, color, capType, capPoints);
            }
            else
            {
                DrawQuadLinesPercentageHelper(d, a, b, c, percentage, lineThickness, color, capType, capPoints);
            }
        }
    }
    
    public static void DrawQuadLines(Vector2 a, Vector2 b, Vector2 c, Vector2 d, LineDrawingInfo lineInfo)
    {
        DrawLine(a, b, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
        DrawLine(b, c, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
        DrawLine(c, d, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
        DrawLine(d, a, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
        
        // new Triangle(a, b, c).GetEdges().Draw(lineThickness, color);
    }
    public static void Draw(this Quad q, ColorRgba color) => DrawQuad(q.A, q.B, q.C, q.D, color);

    public static void DrawLines(this Quad q, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        DrawQuadLines(q.A, q.B, q.C, q.D, lineThickness, color);
        // t.GetEdges().Draw(lineThickness, color);
    }
    public static void DrawLines(this Quad q, LineDrawingInfo lineInfo) => DrawQuadLines(q.A, q.B, q.C, q.D, lineInfo);

    /// <summary>
    /// Draws a certain percentage of an outline.
    /// </summary>
    /// <param name="q"></param>
    /// <param name="f">The percentage of the outline to draw. Negative value reverses the direction (cw).
    /// Integer part can be used to change starting corner.
    /// 0.35 would start at corner a go in ccw direction and draw 35% of the outline.
    /// -2.7 would start at b (the third corner in cw direction) and draw in cw direction 70% of the outline.</param>
    /// <param name="lineThickness"></param>
    /// <param name="color"></param>
    /// <param name="capType"></param>
    /// <param name="capPoints"></param>
    public static void DrawLinesPercentage(this Quad q, float f, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        DrawQuadLinesPercentage(q.A, q.B, q.C, q.D, f, lineThickness, color, capType, capPoints);
    }

    /// <summary>
    /// Draws a certain percentage of an outline.
    /// </summary>
    /// <param name="q"></param>
    /// <param name="f">The percentage of the outline to draw. Negative value reverses the direction (cw).
    /// Integer part can be used to change starting corner.
    /// 0.35 would start at corner a go in ccw direction and draw 35% of the outline.
    /// -2.7 would start at b (the third corner in cw direction) and draw in cw direction 70% of the outline.</param>
    /// <param name="lineInfo"></param>
    public static void DrawLinesPercentage(this Quad q, float f, LineDrawingInfo lineInfo)
    {
        DrawQuadLinesPercentage(q.A, q.B, q.C, q.D, f, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
    }

    
    public static void DrawVertices(this Quad q, float vertexRadius, ColorRgba color, int circleSegments = 8)
    {
        DrawCircle(q.A, vertexRadius, color, circleSegments);
        DrawCircle(q.B, vertexRadius, color, circleSegments);
        DrawCircle(q.C, vertexRadius, color, circleSegments);
        DrawCircle(q.D, vertexRadius, color, circleSegments);
    }

    /// <summary>
    /// Draws a rect where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="q">The quad to draw.</param>
    /// <param name="lineInfo">How to draw the lines.</param>
    /// <param name="rotDeg">The rotation of the quad.</param>
    /// <param name="alignement">Alignement to rotate the quad.</param>
    /// <param name="sideScaleFactor">The scale factor for each side. 0f means no quad is drawn, 1f means normal quad is drawn,
    /// 0.5 means each side is half as long.</param>
    /// <param name="sideScaleOrigin">The point along the line to scale from in both directions.</param>
    public static void DrawLinesScaled(this Quad q, LineDrawingInfo lineInfo, float rotDeg, AnchorPoint alignement, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0) return;
        
        if(rotDeg != 0) q = q.ChangeRotation(rotDeg * ShapeMath.DEGTORAD, alignement);
        
        if (sideScaleFactor >= 1)
        {
            q.DrawLines(lineInfo);
            return;
        }
        
        DrawLine(q.A, q.B, lineInfo, sideScaleFactor, sideScaleOrigin);
        DrawLine(q.B, q.C, lineInfo, sideScaleFactor, sideScaleOrigin);
        DrawLine(q.C, q.D, lineInfo, sideScaleFactor, sideScaleOrigin);
        DrawLine(q.D, q.A, lineInfo, sideScaleFactor, sideScaleOrigin);
        
    }

    #endregion

    #region Shape
    
    public static void DrawOutline(this List<Vector2> points, float lineThickness, ColorRgba startColorRgba, ColorRgba endColorRgba, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (points.Count < 3) return;

        int redStep = (endColorRgba.R - startColorRgba.R) / points.Count;
        int greenStep = (endColorRgba.G - startColorRgba.G) / points.Count;
        int blueStep = (endColorRgba.B - startColorRgba.B) / points.Count;
        int alphaStep = (endColorRgba.A - startColorRgba.A) / points.Count;
        for (var i = 0; i < points.Count; i++)
        {
            var start = points[i];
            var end = points[(i + 1) % points.Count];
            ColorRgba finalColorRgba = new
            (
                startColorRgba.R + redStep * i,
                startColorRgba.G + greenStep * i,
                startColorRgba.B + blueStep * i,
                startColorRgba.A + alphaStep * i
            );
            DrawLine(start, end, lineThickness, finalColorRgba, capType, capPoints);
        }
    }
    public static void DrawOutline(this List<Vector2> shapePoints, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (shapePoints.Count < 3) return;
        
        for (var i = 0; i < shapePoints.Count; i++)
        {
            var start = shapePoints[i];
            var end = shapePoints[(i + 1) % shapePoints.Count];
            DrawLine(start, end, lineThickness, color, capType, capPoints);
        }
    }
    public static void DrawOutline(this List<Vector2> shapePoints, LineDrawingInfo lineInfo)
    {
        DrawOutline(shapePoints, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
    }
    public static void DrawOutline(this List<Vector2> relativePoints, Vector2 pos, float size, float rotDeg, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (relativePoints.Count < 3) return;
        
        for (var i = 0; i < relativePoints.Count; i++)
        {
            var start = pos + (relativePoints[i] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
            var end = pos + (relativePoints[(i + 1) % relativePoints.Count] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
            DrawLine(start, end, lineThickness, color, capType, capPoints);
        }
    }
    public static void DrawOutline(this List<Vector2> relativePoints, Transform2D transform, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        DrawOutline(relativePoints, transform.Position, transform.ScaledSize.Length, transform.RotationDeg, lineThickness, color, capType, capPoints);
    }
    public static void DrawOutline(this List<Vector2> relativePoints, Vector2 pos, float size, float rotDeg, LineDrawingInfo lineInfo) => DrawOutline(relativePoints, pos, size, rotDeg, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
    public static void DrawOutline(this List<Vector2> relativePoints, Transform2D transform, LineDrawingInfo lineInfo) => DrawOutline(relativePoints, transform.Position, transform.ScaledSize.Length, transform.RotationDeg, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);

    
    
    /// <summary>
    /// Draws the points as a polygon where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="shapePoints">The points to draw.</param>
    /// <param name="lineInfo">How to draw the lines.</param>
    /// <param name="sideScaleFactor">The scale factor for each side. 0f means no polygon is drawn, 1f means normal polygon is drawn,
    /// 0.5 means each side is half as long.</param>
    /// <param name="sideScaleOrigin">The point along the line to scale from in both directions.</param>
    public static void DrawLinesScaled(this List<Vector2> shapePoints, LineDrawingInfo lineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (shapePoints.Count < 3) return;
        if (sideScaleFactor <= 0) return;
        
        if (sideScaleFactor >= 1)
        {
            shapePoints.DrawOutline(lineInfo);
            return;
        }
        for (var i = 0; i < shapePoints.Count; i++)
        {
            var start = shapePoints[i];
            var end = shapePoints[(i + 1) % shapePoints.Count];
            DrawLine(start, end, lineInfo, sideScaleFactor, sideScaleOrigin);
        }
        
    }

    /// <summary>
    /// Draws the points as a polygon where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="relativePoints">The relative points.</param>
    /// <param name="lineInfo">How to draw the lines.</param>
    /// <param name="rotDeg">The rotation of the polygon.</param>
    /// <param name="size">The size of the polygon.</param>
    /// <param name="pos">The center of the polygon.</param>
    /// <param name="sideScaleFactor">The scale factor for each side. 0f means no polygon is drawn, 1f means normal polygon is drawn,
    /// 0.5 means each side is half as long.</param>
    /// <param name="sideScaleOrigin">The point along the line to scale from in both directions.</param>
    public static void DrawLinesScaled(this List<Vector2> relativePoints, Vector2 pos, float size, float rotDeg, LineDrawingInfo lineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (relativePoints.Count < 3) return;
        if (sideScaleFactor <= 0) return;
        
        if (sideScaleFactor >= 1)
        {
            relativePoints.DrawOutline(pos, size, rotDeg, lineInfo);
            return;
        }
        
        for (var i = 0; i < relativePoints.Count; i++)
        {
            var start = pos + (relativePoints[i] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
            var end = pos + (relativePoints[(i + 1) % relativePoints.Count] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
            DrawLine(start, end, lineInfo, sideScaleFactor, sideScaleOrigin);
        }
        
    }
    
    /// <summary>
    /// Draws the points as a polygon where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="relativePoints">The relative points.</param>
    /// <param name="lineInfo">How to draw the lines.</param>
    /// <param name="transform">The transform of the polygon.</param>
    /// <param name="sideScaleFactor">The scale factor for each side. 0f means no polygon is drawn, 1f means normal polygon is drawn,
    /// 0.5 means each side is half as long.</param>
    /// <param name="sideScaleOrigin">The point along the line to scale from in both directions.</param>
    public static void DrawLinesScaled(this List<Vector2> relativePoints, Transform2D transform, LineDrawingInfo lineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        DrawLinesScaled(relativePoints, transform.Position, transform.ScaledSize.Length, transform.RotationDeg, lineInfo, sideScaleFactor, sideScaleOrigin);

    }

    
    public static void DrawOutlineCornered(this List<Vector2> points, float lineThickness, ColorRgba color, List<float> cornerLengths, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        for (var i = 0; i < points.Count; i++)
        {
            float cornerLength = cornerLengths[i%cornerLengths.Count];
            var prev = points[(i - 1) % points.Count];
            var cur = points[i];
            var next = points[(i + 1) % points.Count];
            DrawLine(cur, cur + next.Normalize() * cornerLength, lineThickness, color, capType, capPoints);
            DrawLine(cur, cur + prev.Normalize() * cornerLength, lineThickness, color, capType, capPoints);
        }
    }
    public static void DrawOutlineCorneredRelative(this List<Vector2> points, float lineThickness, ColorRgba color, List<float> cornerFactors, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        for (var i = 0; i < points.Count; i++)
        {
            float cornerF = cornerFactors[i%cornerFactors.Count];
            var prev = points[(i - 1) % points.Count];
            var cur = points[i];
            var next = points[(i + 1) % points.Count];
            DrawLine(cur, cur.Lerp(next, cornerF), lineThickness, color, capType, capPoints);
            DrawLine(cur, cur.Lerp(prev, cornerF), lineThickness, color, capType, capPoints);
        }
    }
    public static void DrawOutlineCornered(this List<Vector2> points, List<float> cornerLengths, LineDrawingInfo lineInfo)
    {
        for (var i = 0; i < points.Count; i++)
        {
            float cornerLength = cornerLengths[i%cornerLengths.Count];
            var prev = points[(i - 1) % points.Count];
            var cur = points[i];
            var next = points[(i + 1) % points.Count];
            DrawLine(cur, cur + next.Normalize() * cornerLength, lineInfo);
            DrawLine(cur, cur + prev.Normalize() * cornerLength, lineInfo);
        }
    }
    public static void DrawOutlineCorneredRelative(this List<Vector2> points, List<float> cornerFactors, LineDrawingInfo lineInfo)
    {
        for (var i = 0; i < points.Count; i++)
        {
            float cornerF = cornerFactors[i%cornerFactors.Count];
            var prev = points[(i - 1) % points.Count];
            var cur = points[i];
            var next = points[(i + 1) % points.Count];
            DrawLine(cur, cur.Lerp(next, cornerF), lineInfo);
            DrawLine(cur, cur.Lerp(prev, cornerF), lineInfo);
        }
    }

    public static void DrawOutlineCornered(this List<Vector2> points, float lineThickness, ColorRgba color, float cornerLength, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        for (var i = 0; i < points.Count; i++)
        {
            var prev = points[(i-1)%points.Count];
            var cur = points[i];
            var next = points[(i+1)%points.Count];
            DrawLine(cur, cur + next.Normalize() * cornerLength, lineThickness, color, capType, capPoints);
            DrawLine(cur, cur + prev.Normalize() * cornerLength, lineThickness, color, capType, capPoints);
        }
    }
    public static void DrawOutlineCorneredRelative(this List<Vector2> points, float lineThickness, ColorRgba color, float cornerF, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        for (var i = 0; i < points.Count; i++)
        {
            var prev = points[(i - 1) % points.Count];
            var cur = points[i];
            var next = points[(i + 1) % points.Count];
            DrawLine(cur, cur.Lerp(next, cornerF), lineThickness, color, capType, capPoints);
            DrawLine(cur, cur.Lerp(prev, cornerF), lineThickness, color, capType, capPoints);
        }
    }
    public static void DrawOutlineCornered(this List<Vector2> points, float cornerLength, LineDrawingInfo lineInfo)
    {
        for (var i = 0; i < points.Count; i++)
        {
            var prev = points[(i-1)%points.Count];
            var cur = points[i];
            var next = points[(i+1)%points.Count];
            DrawLine(cur, cur + next.Normalize() * cornerLength, lineInfo);
            DrawLine(cur, cur + prev.Normalize() * cornerLength, lineInfo);
        }
    }
    public static void DrawOutlineCorneredRelative(this List<Vector2> points, float cornerF, LineDrawingInfo lineInfo)
    {
        for (var i = 0; i < points.Count; i++)
        {
            var prev = points[(i - 1) % points.Count];
            var cur = points[i];
            var next = points[(i + 1) % points.Count];
            DrawLine(cur, cur.Lerp(next, cornerF), lineInfo);
            DrawLine(cur, cur.Lerp(prev, cornerF), lineInfo);
        }
    }

    public static void DrawOutlineCornered(this List<Vector2> points, LineDrawingInfo lineInfo, float cornerLength) => DrawOutlineCornered(points, lineInfo.Thickness,lineInfo.Color, cornerLength, lineInfo.CapType, lineInfo.CapPoints);

    public static void DrawOutlineCornered(this List<Vector2> points, LineDrawingInfo lineInfo, List<float> cornerLengths) => DrawOutlineCornered(points, lineInfo.Thickness,lineInfo.Color, cornerLengths, lineInfo.CapType, lineInfo.CapPoints);

    public static void DrawOutlineCorneredRelative(this List<Vector2> points, LineDrawingInfo lineInfo, float cornerF) => DrawOutlineCornered(points, lineInfo.Thickness,lineInfo.Color, cornerF, lineInfo.CapType, lineInfo.CapPoints);

    public static void DrawOutlineCorneredRelative(this List<Vector2> points, LineDrawingInfo lineInfo, List<float> cornerFactors) => DrawOutlineCornered(points, lineInfo.Thickness,lineInfo.Color, cornerFactors, lineInfo.CapType, lineInfo.CapPoints);

    public static void DrawVertices(this List<Vector2> points, float vertexRadius, ColorRgba color, int circleSegments)
    {
        foreach (var p in points)
        {
            DrawCircle(p, vertexRadius, color, circleSegments);
        }
    }

    #endregion
    
    #region Polygon

    public static void DrawPolygonConvex(this Polygon poly, ColorRgba color, bool clockwise = false) { DrawPolygonConvex(poly, poly.GetCentroid(), color, clockwise); }
    public static void DrawPolygonConvex(this Polygon poly, Vector2 center, ColorRgba color, bool clockwise = false)
    {
        if (clockwise)
        {
            for (var i = 0; i < poly.Count - 1; i++)
            {
                Raylib.DrawTriangle(poly[i], center, poly[i + 1], color.ToRayColor());
            }
            Raylib.DrawTriangle(poly[poly.Count - 1], center, poly[0], color.ToRayColor());
        }
        else
        {
            for (var i = 0; i < poly.Count - 1; i++)
            {
                Raylib.DrawTriangle(poly[i], poly[i + 1], center, color.ToRayColor());
            }
            Raylib.DrawTriangle(poly[poly.Count - 1], poly[0], center, color.ToRayColor());
        }
    }
    public static void DrawPolygonConvex(this Polygon relativePoly, Vector2 pos, float size, float rotDeg, ColorRgba color, bool clockwise = false)
    {
        if (clockwise)
        {
            for (int i = 0; i < relativePoly.Count - 1; i++)
            {
                var a = pos + ShapeVec.Rotate(relativePoly[i] * size, rotDeg * ShapeMath.DEGTORAD);
                var b = pos;
                var c = pos + ShapeVec.Rotate(relativePoly[i + 1] * size, rotDeg * ShapeMath.DEGTORAD);
                Raylib.DrawTriangle(a, b, c, color.ToRayColor());
            }

            var aFinal = pos + (relativePoly[relativePoly.Count - 1] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
            var bFinal = pos;
            var cFinal = pos + (relativePoly[0] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
            Raylib.DrawTriangle(aFinal, bFinal, cFinal, color.ToRayColor());
        }
        else
        {
            for (int i = 0; i < relativePoly.Count - 1; i++)
            {
                var a = pos + ShapeVec.Rotate(relativePoly[i] * size, rotDeg * ShapeMath.DEGTORAD);
                var b = pos + ShapeVec.Rotate(relativePoly[i + 1] * size, rotDeg * ShapeMath.DEGTORAD);
                var c = pos;
                Raylib.DrawTriangle(a, b, c, color.ToRayColor());
            }

            var aFinal = pos + (relativePoly[relativePoly.Count - 1] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
            var bFinal = pos + (relativePoly[0] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
            var cFinal = pos;
            Raylib.DrawTriangle(aFinal, bFinal, cFinal, color.ToRayColor());
        }
    }
    public static void DrawPolygonConvex(this Polygon relativePoly, Transform2D transform, ColorRgba color, bool clockwise = false)
    {
        DrawPolygonConvex(relativePoly, transform.Position, transform.ScaledSize.Length, transform.RotationDeg, color, clockwise);
    }

    public static void Draw(this Polygon poly, ColorRgba color)
    {
        if (poly.Count < 3) return;
        if (poly.Count == 3)
        {
            DrawTriangle(poly[0], poly[1], poly[2], color);
            return;
        }
        poly.Triangulate().Draw(color);
    }
    public static void DEBUG_DrawLinesCCW(this Polygon poly, float lineThickness, ColorRgba startColorRgba, ColorRgba endColorRgba)
    {
        if (poly.Count < 3) return;

        DrawLines(poly, lineThickness, startColorRgba, endColorRgba);
        ShapeDrawing.DrawCircle(poly[0], lineThickness * 2f, startColorRgba);
        ShapeDrawing.DrawCircle(poly[poly.Count - 1], lineThickness * 2f, endColorRgba);
        // var edges = poly.GetEdges();
        // int redStep =   (endColor.r - startColor.r) / edges.Count;
        // int greenStep = (endColor.g - startColor.g) / edges.Count;
        // int blueStep =  (endColor.b - startColor.b) / edges.Count;
        // int alphaStep = (endColor.a - startColor.a) / edges.Count;
        //
        // for (int i = 0; i < edges.Count; i++)
        // {
        //     var edge = edges[i];
        //     ShapeColor finalColor = new
        //         (
        //             startColor.r + redStep * i,
        //             startColor.g + greenStep * i,
        //             startColor.b + blueStep * i,
        //             startColor.a + alphaStep * i
        //         );
        //     edge.Draw(lineThickness, finalColor, LineCapType.CappedExtended, 2);
        // }
        
    }
    
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
            DrawLine(start, end, lineThickness, finalColorRgba, capType, capPoints);
        }
        
        
        // var edges = poly.GetEdges();
        // int redStep = (endColor.r - startColor.r) / edges.Count;
        // int greenStep = (endColor.g - startColor.g) / edges.Count;
        // int blueStep = (endColor.b - startColor.b) / edges.Count;
        // int alphaStep = (endColor.a - startColor.a) / edges.Count;

        // for (int i = 0; i < edges.Count; i++)
        // {
            // var edge = edges[i];
            // ShapeColor finalColor = new
                // (
                    // startColor.r + redStep * i,
                    // startColor.g + greenStep * i,
                    // startColor.b + blueStep * i,
                    // startColor.a + alphaStep * i
                // );
            //// if(cornerSegments > 5) DrawCircle(edge.Start, lineThickness * 0.5f, finalColor, cornerSegments);
            // edge.Draw(lineThickness, finalColor);
        // }
    }
    
    public static void DrawLines(this Polygon poly, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (poly.Count < 3) return;
        
        for (var i = 0; i < poly.Count; i++)
        {
            var start = poly[i];
            var end = poly[(i + 1) % poly.Count];
            DrawLine(start, end, lineThickness, color, capType, capPoints);
        }
    }
    
    public static void DrawLines(this Polygon relative, Vector2 pos, float size, float rotDeg, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (relative.Count < 3) return;
        
        for (var i = 0; i < relative.Count; i++)
        {
            var start = pos + (relative[i] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
            var end = pos + (relative[(i + 1) % relative.Count] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
            DrawLine(start, end, lineThickness, color, capType, capPoints);
        }
    }
    public static void DrawLines(this Polygon relative, Transform2D transform, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        DrawLines(relative, transform.Position, transform.ScaledSize.Length, transform.RotationDeg, lineThickness, color, capType, capPoints);
    }

    public static void DrawLines(this Polygon poly, LineDrawingInfo lineInfo) => DrawLines(poly, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);

    public static void DrawLines(this Polygon relative, Vector2 pos, float size, float rotDeg, LineDrawingInfo lineInfo) => DrawLines(relative, pos, size, rotDeg, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
    public static void DrawLines(this Polygon relative, Transform2D transform, LineDrawingInfo lineInfo) => DrawLines(relative, transform.Position, transform.ScaledSize.Length, transform.RotationDeg, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);


    public static void DrawVertices(this Polygon poly, float vertexRadius, ColorRgba color, int circleSegments)
    {
        foreach (var p in poly)
        {
            DrawCircle(p, vertexRadius, color, circleSegments);
        }
    }
    
    public static void DrawCornered(this Polygon poly, float lineThickness, ColorRgba color, List<float> cornerLengths, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        for (var i = 0; i < poly.Count; i++)
        {
            float cornerLength = cornerLengths[i%cornerLengths.Count];
            var prev = poly[(i - 1) % poly.Count];
            var cur = poly[i];
            var next = poly[(i + 1) % poly.Count];
            DrawLine(cur, cur + next.Normalize() * cornerLength, lineThickness, color, capType, capPoints);
            DrawLine(cur, cur + prev.Normalize() * cornerLength, lineThickness, color, capType, capPoints);
        }
    }
    public static void DrawCorneredRelative(this Polygon poly, float lineThickness, ColorRgba color, List<float> cornerFactors, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        for (var i = 0; i < poly.Count; i++)
        {
            float cornerF = cornerFactors[i%cornerFactors.Count];
            var prev = poly[(i - 1) % poly.Count];
            var cur = poly[i];
            var next = poly[(i + 1) % poly.Count];
            DrawLine(cur, cur.Lerp(next, cornerF), lineThickness, color, capType, capPoints);
            DrawLine(cur, cur.Lerp(prev, cornerF), lineThickness, color, capType, capPoints);
        }
    }
    public static void DrawCornered(this Polygon poly, List<float> cornerLengths, LineDrawingInfo lineInfo)
    {
        for (var i = 0; i < poly.Count; i++)
        {
            float cornerLength = cornerLengths[i%cornerLengths.Count];
            var prev = poly[(i - 1) % poly.Count];
            var cur = poly[i];
            var next = poly[(i + 1) % poly.Count];
            DrawLine(cur, cur + next.Normalize() * cornerLength, lineInfo);
            DrawLine(cur, cur + prev.Normalize() * cornerLength, lineInfo);
        }
    }
    public static void DrawCorneredRelative(this Polygon poly, List<float> cornerFactors, LineDrawingInfo lineInfo)
    {
        for (var i = 0; i < poly.Count; i++)
        {
            float cornerF = cornerFactors[i%cornerFactors.Count];
            var prev = poly[(i - 1) % poly.Count];
            var cur = poly[i];
            var next = poly[(i + 1) % poly.Count];
            DrawLine(cur, cur.Lerp(next, cornerF), lineInfo);
            DrawLine(cur, cur.Lerp(prev, cornerF), lineInfo);
        }
    }

    public static void DrawCornered(this Polygon poly, float lineThickness, ColorRgba color, float cornerLength, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        for (var i = 0; i < poly.Count; i++)
        {
            var prev = poly[(i-1)%poly.Count];
            var cur = poly[i];
            var next = poly[(i+1)%poly.Count];
            DrawLine(cur, cur + next.Normalize() * cornerLength, lineThickness, color, capType, capPoints);
            DrawLine(cur, cur + prev.Normalize() * cornerLength, lineThickness, color, capType, capPoints);
        }
    }
    public static void DrawCorneredRelative(this Polygon poly, float lineThickness, ColorRgba color, float cornerF, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        for (var i = 0; i < poly.Count; i++)
        {
            var prev = poly[(i - 1) % poly.Count];
            var cur = poly[i];
            var next = poly[(i + 1) % poly.Count];
            DrawLine(cur, cur.Lerp(next, cornerF), lineThickness, color, capType, capPoints);
            DrawLine(cur, cur.Lerp(prev, cornerF), lineThickness, color, capType, capPoints);
        }
    }
    public static void DrawCornered(this Polygon poly, float cornerLength, LineDrawingInfo lineInfo)
    {
        for (var i = 0; i < poly.Count; i++)
        {
            var prev = poly[(i-1)%poly.Count];
            var cur = poly[i];
            var next = poly[(i+1)%poly.Count];
            DrawLine(cur, cur + next.Normalize() * cornerLength, lineInfo);
            DrawLine(cur, cur + prev.Normalize() * cornerLength, lineInfo);
        }
    }
    public static void DrawCorneredRelative(this Polygon poly, float cornerF, LineDrawingInfo lineInfo)
    {
        for (var i = 0; i < poly.Count; i++)
        {
            var prev = poly[(i - 1) % poly.Count];
            var cur = poly[i];
            var next = poly[(i + 1) % poly.Count];
            DrawLine(cur, cur.Lerp(next, cornerF), lineInfo);
            DrawLine(cur, cur.Lerp(prev, cornerF), lineInfo);
        }
    }

    public static void DrawCornered(this Polygon poly, LineDrawingInfo lineInfo, float cornerLength) => DrawCornered(poly, lineInfo.Thickness,lineInfo.Color, cornerLength, lineInfo.CapType, lineInfo.CapPoints);

    public static void DrawCornered(this Polygon poly, LineDrawingInfo lineInfo, List<float> cornerLengths) => DrawCornered(poly, lineInfo.Thickness,lineInfo.Color, cornerLengths, lineInfo.CapType, lineInfo.CapPoints);

    public static void DrawCorneredRelative(this Polygon poly, LineDrawingInfo lineInfo, float cornerF) => DrawCornered(poly, lineInfo.Thickness,lineInfo.Color, cornerF, lineInfo.CapType, lineInfo.CapPoints);

    public static void DrawCorneredRelative(this Polygon poly, LineDrawingInfo lineInfo, List<float> cornerFactors) => DrawCornered(poly, lineInfo.Thickness,lineInfo.Color, cornerFactors, lineInfo.CapType, lineInfo.CapPoints);

    
    /// <summary>
    /// Draws a polygon where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="poly">The polygon to draw.</param>
    /// <param name="lineInfo">How to draw the lines.</param>
    /// <param name="sideScaleFactor">The scale factor for each side. 0f means no polygon is drawn, 1f means normal polygon is drawn,
    /// 0.5 means each side is half as long.</param>
    /// <param name="sideScaleOrigin">The point along the line to scale from in both directions.</param>
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
            DrawLine(start, end, lineInfo, sideScaleFactor, sideScaleOrigin);
        }
        
    }

    /// <summary>
    /// Draws a polygon where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="relative">The relative polygon points.</param>
    /// <param name="lineInfo">How to draw the lines.</param>
    /// <param name="rotDeg">The rotation of the polygon.</param>
    /// <param name="size">The size of the polygon.</param>
    /// <param name="pos">The center of the polygon.</param>
    /// <param name="sideScaleFactor">The scale factor for each side. 0f means no polygon is drawn, 1f means normal polygon is drawn,
    /// 0.5 means each side is half as long.</param>
    /// <param name="sideScaleOrigin">The point along the line to scale from in both directions.</param>
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
            DrawLine(start, end, lineInfo, sideScaleFactor, sideScaleOrigin);
        }
        
    }
    
    /// <summary>
    /// Draws a polygon where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="relative">The relative polygon points.</param>
    /// <param name="lineInfo">How to draw the lines.</param>
    /// <param name="transform">The transform of the polygon.</param>
    /// <param name="sideScaleFactor">The scale factor for each side. 0f means no polygon is drawn, 1f means normal polygon is drawn,
    /// 0.5 means each side is half as long.</param>
    /// <param name="sideScaleOrigin">The point along the line to scale from in both directions.</param>
    public static void DrawLinesScaled(this Polygon relative, Transform2D transform, LineDrawingInfo lineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        DrawLinesScaled(relative, transform.Position, transform.ScaledSize.Length, transform.RotationDeg, lineInfo, sideScaleFactor, sideScaleOrigin);

    }

    #endregion

    #region Polyline

    public static void Draw(this Polyline polyline, float thickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (polyline.Count < 2) return;
        for (var i = 0; i < polyline.Count - 1; i++)
        {
            var start = polyline[i];
            var end = polyline[i + 1];
            DrawLine(start, end, thickness, color, capType, capPoints);
        }
        // polyline.GetEdges().Draw(thickness, color);
    }
    public static void Draw(this Polyline polyline, LineDrawingInfo lineInfo)
    {
        if (polyline.Count < 2) return;
        for (var i = 0; i < polyline.Count - 1; i++)
        {
            var start = polyline[i];
            var end = polyline[i + 1];
            DrawLine(start, end, lineInfo);
        }
    }

    public static void Draw(this Polyline polyline, float thickness, List<ColorRgba> colors, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (polyline.Count < 2) return;
        for (var i = 0; i < polyline.Count - 1; i++)
        {
            var start = polyline[i];
            var end = polyline[i + 1];
            var c = colors[i % colors.Count];
            DrawLine(start, end, thickness, c, capType, capPoints);
        }
    }
    public static void Draw(this Polyline polyline, List<ColorRgba> colors, LineDrawingInfo lineInfo)
    {
        Draw(polyline, lineInfo.Thickness, colors, lineInfo.CapType, lineInfo.CapPoints);
    }

    public static void Draw(this Polyline relative, Vector2 pos, float size, float rotDeg, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (relative.Count < 2) return;
        
        for (var i = 0; i < relative.Count - 1; i++)
        {
            var start = pos + (relative[i] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
            var end = pos + (relative[(i + 1) % relative.Count] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
            DrawLine(start, end, lineThickness, color, capType, capPoints);
        }
    }
    public static void Draw(this Polyline relative, Transform2D transform, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        Draw(relative, transform.Position, transform.ScaledSize.Length, transform.RotationDeg, lineThickness, color, capType, capPoints);
    }
    public static void Draw(this Polyline relative, Vector2 pos, float size, float rotDeg, LineDrawingInfo lineInfo) => Draw(relative, pos, size, rotDeg, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
    public static void Draw(this Polyline relative, Transform2D transform, LineDrawingInfo lineInfo) => Draw(relative, transform.Position, transform.ScaledSize.Length, transform.RotationDeg, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);

     /// <summary>
    /// Draws a polyline where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="polyline">The polyline to draw.</param>
    /// <param name="lineInfo">How to draw the lines.</param>
    /// <param name="sideScaleFactor">The scale factor for each side. 0f means no polyline is drawn, 1f means normal polyline is drawn,
    /// 0.5 means each side is half as long.</param>
    /// <param name="sideScaleOrigin">The point along the line to scale from in both directions.</param>
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
            DrawLine(start, end, lineInfo, sideScaleFactor, sideScaleOrigin);
        }
        
    }

    /// <summary>
    /// Draws a polyline where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="relative">The relative polyline points.</param>
    /// <param name="lineInfo">How to draw the lines.</param>
    /// <param name="rotDeg">The rotation of the polyline.</param>
    /// <param name="size">The size of the polyline.</param>
    /// <param name="pos">The center of the polyline.</param>
    /// <param name="sideScaleFactor">The scale factor for each side. 0f means no polyline is drawn, 1f means normal polyline is drawn,
    /// 0.5 means each side is half as long.</param>
    /// <param name="sideScaleOrigin">The point along the line to scale from in both directions.</param>
    public static void DrawLinesScaled(this Polyline relative, Vector2 pos, float size, float rotDeg, LineDrawingInfo lineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (relative.Count < 2) return;
        if (sideScaleFactor <= 0) return;
        
        if (sideScaleFactor >= 1)
        {
            relative.Draw(pos, size, rotDeg, lineInfo);
            return;
        }
        
        for (var i = 0; i < relative.Count - 1; i++)
        {
            var start = pos + (relative[i] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
            var end = pos + (relative[(i + 1) % relative.Count] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
            DrawLine(start, end, lineInfo, sideScaleFactor, sideScaleOrigin);
        }
        
    }
    
    /// <summary>
    /// Draws a polyline where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="relative">The relative polyline points.</param>
    /// <param name="lineInfo">How to draw the lines.</param>
    /// <param name="transform">The transform of the polyline.</param>
    /// <param name="sideScaleFactor">The scale factor for each side. 0f means no polyline is drawn, 1f means normal polyline is drawn,
    /// 0.5 means each side is half as long.</param>
    /// <param name="sideScaleOrigin">The point along the line to scale from in both directions.</param>
    public static void DrawLinesScaled(this Polyline relative, Transform2D transform, LineDrawingInfo lineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        DrawLinesScaled(relative, transform.Position, transform.ScaledSize.Length, transform.RotationDeg, lineInfo, sideScaleFactor, sideScaleOrigin);

    }

    
    public static void DrawVertices(this Polyline polyline, float vertexRadius, ColorRgba color, int circleSegments)
    {
        foreach (var p in polyline)
        {
            DrawCircle(p, vertexRadius, color, circleSegments);
        }
    }
    
    
    public static void DrawGlow(this Polyline polyline, float width, float endWidth, ColorRgba color,
        ColorRgba endColorRgba, int steps, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (polyline.Count < 2) return;
        for (var i = 0; i < polyline.Count - 1; i++)
        {
            var start = polyline[i];
            var end = polyline[i + 1];
            DrawLineGlow(start, end, width, endWidth, color, endColorRgba, steps, capType, capPoints);
        }
        // polyline.GetEdges().DrawGlow(width, endWidth, color, endColor, steps);
    }

    #endregion
    
    #region UI
    public static void DrawOutlineBar(this Rect rect, float thickness, float f, ColorRgba color)
    {
        var thicknessOffsetX = new Vector2(thickness, 0f);
        var thicknessOffsetY = new Vector2(0f, thickness);

        var tl = new Vector2(rect.X, rect.Y);
        var br = tl + new Vector2(rect.Width, rect.Height);
        var tr = tl + new Vector2(rect.Width, 0);
        var bl = tl + new Vector2(0, rect.Height);

        var lines = (int)MathF.Ceiling(4 * ShapeMath.Clamp(f, 0f, 1f));
        float fMin = 0.25f * (lines - 1);
        float fMax = fMin + 0.25f;
        float newF = ShapeMath.RemapFloat(f, fMin, fMax, 0f, 1f);
        for (var i = 0; i < lines; i++)
        {
            Vector2 end;
            Vector2 start;
            if (i == 0)
            {
                start = tl - thicknessOffsetX / 2;
                end = tr - thicknessOffsetX / 2;
            }
            else if (i == 1)
            {
                start = tr - thicknessOffsetY / 2;
                end = br - thicknessOffsetY / 2;
            }
            else if (i == 2)
            {
                start = br + thicknessOffsetX / 2;
                end = bl + thicknessOffsetX / 2;
            }
            else
            {
                start = bl + thicknessOffsetY / 2;
                end = tl + thicknessOffsetY / 2;
            }

            //last line
            if (i == lines - 1) end = ShapeVec.Lerp(start, end, newF);
            ShapeDrawing.DrawLine(start, end, thickness, color);
            // DrawLineEx(start, end, thickness, color.ToRayColor());
        }
    }
    public static void DrawOutlineBar(this Rect rect, Vector2 pivot, float angleDeg, float thickness, float f, ColorRgba color)
    {
        var rr = rect.RotateCorners(pivot, angleDeg);
        //Vector2 thicknessOffsetX = new Vector2(thickness, 0f);
        //Vector2 thicknessOffsetY = new Vector2(0f, thickness);

        var leftExtension = new Vector2(-thickness / 2, 0f).Rotate(angleDeg * ShapeMath.DEGTORAD);
        var rightExtension = new Vector2(thickness / 2, 0f).Rotate(angleDeg * ShapeMath.DEGTORAD);

        var tl = rr.tl;
        var br = rr.br;
        var tr = rr.tr;
        var bl = rr.bl;

        int lines = (int)MathF.Ceiling(4 * ShapeMath.Clamp(f, 0f, 1f));
        float fMin = 0.25f * (lines - 1);
        float fMax = fMin + 0.25f;
        float newF = ShapeMath.RemapFloat(f, fMin, fMax, 0f, 1f);
        for (int i = 0; i < lines; i++)
        {
            Vector2 end;
            Vector2 start;
            if (i == 0)
            {
                start = tl + leftExtension;
                end = tr + rightExtension;
            }
            else if (i == 1)
            {
                start = tr;
                end = br;
            }
            else if (i == 2)
            {
                start = br + rightExtension;
                end = bl + leftExtension;
            }
            else
            {
                start = bl;
                end = tl;
            }

            //last line
            if (i == lines - 1) end = ShapeVec.Lerp(start, end, newF);
            ShapeDrawing.DrawLine(start, end, thickness, color);
            // Raylib.DrawLineEx(start, end, thickness, color.ToRayColor());
        }
    }

    public static void DrawOutlineBar(this Circle c, float thickness, float f, ColorRgba color) => DrawCircleSectorLines(c.Center, c.Radius, 0, 360 * f, thickness, color, false);
    public static void DrawOutlineBar(this Circle c, float startOffsetDeg, float thickness, float f, ColorRgba color) => DrawCircleSectorLines(c.Center, c.Radius, 0, 360 * f, startOffsetDeg, thickness, color, false);
    public static void DrawBar(this Rect rect, float f, ColorRgba barColorRgba, ColorRgba bgColorRgba, float left = 0f, float right = 1f, float top = 0f, float bottom = 0f)
    {
        f = 1.0f - f;
        Rect.Margins progressMargins = new(f * top, f * right, f * bottom, f * left);
        var progressRect = rect.ApplyMargins(progressMargins); // progressMargins.Apply(rect);
        rect.Draw(bgColorRgba);
        progressRect.Draw(barColorRgba);
    }
    public static void DrawBar(this Rect rect, Vector2 pivot, float angleDeg, float f, ColorRgba barColorRgba, ColorRgba bgColorRgba, float left = 0f, float right = 1f, float top = 0f, float bottom = 0f)
    {
        f = 1.0f - f;
        Rect.Margins progressMargins = new(f * top, f * right, f * bottom, f * left);
        var progressRect = rect.ApplyMargins(progressMargins); // progressMargins.Apply(rect);
        rect.Draw(pivot, angleDeg, bgColorRgba);
        progressRect.Draw(pivot, angleDeg, barColorRgba);
    }
    #endregion

    #region DrawLinePercentageHelpers
    
    private static void DrawTriangleLinesPercentageHelper(Vector2 p1, Vector2 p2, Vector2 p3, float percentage, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        var l1 = (p2 - p1).Length();
        var l2 = (p3 - p2).Length();
        var l3 = (p1 - p3).Length();
        var perimeterToDraw = (l1 + l2 + l3) * percentage;
        
        // Draw first segment
        var curP = p1;
        var nextP = p2;
        if (perimeterToDraw < l1)
        {
            float p = perimeterToDraw / l1;
            nextP = curP.Lerp(nextP, p);
            DrawLine(curP, nextP, lineThickness, color);
            return;
        }
                
        DrawLine(curP, nextP, lineThickness, color, capType, capPoints);
        perimeterToDraw -= l1;
                
        // Draw second segment
        curP = nextP;
        nextP = p3;
        if (perimeterToDraw < l2)
        {
            float p = perimeterToDraw / l2;
            nextP = curP.Lerp(nextP, p);
            DrawLine(curP, nextP, lineThickness, color, capType, capPoints);
            return;
        }
                
        DrawLine(curP, nextP, lineThickness, color, capType, capPoints);
        perimeterToDraw -= l2;
                
        // Draw third segment
        curP = nextP;
        nextP = p1;
        if (perimeterToDraw < l3)
        {
            float p = perimeterToDraw / l3;
            nextP = curP.Lerp(nextP, p);
        }
        
        DrawLine(curP, nextP, lineThickness, color, capType, capPoints);
       
    }
    private static void DrawQuadLinesPercentageHelper(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, float percentage, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        var l1 = (p2 - p1).Length();
        var l2 = (p3 - p2).Length();
        var l3 = (p4 - p3).Length();
        var l4 = (p1 - p4).Length();
        var perimeterToDraw = (l1 + l2 + l3 + l4) * percentage;
        
        // Draw first segment
        var curP = p1;
        var nextP = p2;
        if (perimeterToDraw < l1)
        {
            float p = perimeterToDraw / l1;
            nextP = curP.Lerp(nextP, p);
            DrawLine(curP, nextP, lineThickness, color);
            return;
        }
                
        DrawLine(curP, nextP, lineThickness, color, capType, capPoints);
        perimeterToDraw -= l1;
                
        // Draw second segment
        curP = nextP;
        nextP = p3;
        if (perimeterToDraw < l2)
        {
            float p = perimeterToDraw / l2;
            nextP = curP.Lerp(nextP, p);
            DrawLine(curP, nextP, lineThickness, color, capType, capPoints);
            return;
        }
                
        DrawLine(curP, nextP, lineThickness, color, capType, capPoints);
        perimeterToDraw -= l2;
                
        // Draw third segment
        curP = nextP;
        nextP = p4;
        if (perimeterToDraw < l3)
        {
            float p = perimeterToDraw / l3;
            nextP = curP.Lerp(nextP, p);
            DrawLine(curP, nextP, lineThickness, color, capType, capPoints);
            return;
        }
        
        DrawLine(curP, nextP, lineThickness, color, capType, capPoints);
        perimeterToDraw -= l3;
               
        // Draw fourth segment
        curP = nextP;
        nextP = p1;
        if (perimeterToDraw < l4)
        {
            float p = perimeterToDraw / l4;
            nextP = curP.Lerp(nextP, p);
        }
        DrawLine(curP, nextP, lineThickness, color, capType, capPoints);
    }
    private static void DrawRectLinesPercentageHelper(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, float perimeterToDraw, float size1, float size2, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        // Draw first segment
        var curP = p1;
        var nextP = p2;
        if (perimeterToDraw < size1)
        {
            float p = perimeterToDraw / size1;
            nextP = curP.Lerp(nextP, p);
            DrawLine(curP, nextP, lineThickness, color, capType, capPoints);
            return;
        }
                
        DrawLine(curP, nextP, lineThickness, color, capType, capPoints);
        perimeterToDraw -= size1;
                
        // Draw second segment
        curP = nextP;
        nextP = p3;
        if (perimeterToDraw < size2)
        {
            float p = perimeterToDraw / size2;
            nextP = curP.Lerp(nextP, p);
            DrawLine(curP, nextP, lineThickness, color, capType, capPoints);
            return;
        }
                
        DrawLine(curP, nextP, lineThickness, color, capType, capPoints);
        perimeterToDraw -= size2;
                
        // Draw third segment
        curP = nextP;
        nextP = p4;
        if (perimeterToDraw < size1)
        {
            float p = perimeterToDraw / size1;
            nextP = curP.Lerp(nextP, p);
            DrawLine(curP, nextP, lineThickness, color, capType, capPoints);
            return;
        }
        
        DrawLine(curP, nextP, lineThickness, color, capType, capPoints);
        perimeterToDraw -= size1;
               
        // Draw fourth segment
        curP = nextP;
        nextP = p1;
        if (perimeterToDraw < size2)
        {
            float p = perimeterToDraw / size2;
            nextP = curP.Lerp(nextP, p);
        }
        DrawLine(curP, nextP, lineThickness, color, capType, capPoints);
    }

    #endregion
}

