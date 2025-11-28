using System.Drawing;
using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.SegmentDef;

/// <summary>
/// Provides static methods for drawing line segments and collections of segments with various styles, thicknesses, and effects.
/// </summary>
/// <remarks>
/// This class contains extension methods for <see cref="Segment"/> and <see cref="Segments"/> to simplify drawing operations.
/// </remarks>
public static class SegmentDrawing
{
    
    /// <summary>
    /// Minimum length (in world units) that a segment must have to be drawn.
    /// Segments with a squared length less than <see cref="MinSegmentDrawLengthSquared"/> are skipped to avoid rendering
    /// extremely short or degenerate geometry that can cause visual artifacts or unnecessary draw calls.
    /// </summary>
    public static readonly float MinSegmentDrawLength = 0.1f;
    
    /// <summary>
    /// Squared minimum segment draw length used to avoid computing square roots when
    /// checking whether a segment is long enough to be drawn. This value is precomputed
    /// from <see cref="MinSegmentDrawLength"/> to allow length comparisons using
    /// squared distances.
    /// </summary>
    private static readonly float MinSegmentDrawLengthSquared = MinSegmentDrawLength * MinSegmentDrawLength;
    
    #region Draw Segment
    /// <summary>
    /// Draws a segment from <paramref name="start"/> to a point along the direction to <paramref name="end"/>, scaled by <paramref name="sideLengthFactor"/>.
    /// </summary>
    /// <param name="start">The starting point of the segment.</param>
    /// <param name="end">The ending point of the segment.</param>
    /// <param name="thickness">The thickness of the segment.</param>
    /// <param name="color">The color of the segment.</param>
    /// <param name="sideLengthFactor">The factor by which to scale the segment's length <c>(0 = no length, 1 = full length)</c>.</param>
    /// <param name="capType">The type of line cap to use at the ends of the segment.</param>
    /// <param name="capPoints">The number of points used to draw the cap (for rounded or custom caps).</param>
    public static void DrawSegment(Vector2 start, Vector2 end, float thickness, ColorRgba color, float sideLengthFactor, LineCapType capType = LineCapType.None, int capPoints = 0)
    {
        var dir = end - start;
        var newEnd = start + dir * sideLengthFactor;
        DrawSegment(start, newEnd, thickness, color, capType, capPoints);
    }

    /// <summary>
    /// Draws a segment from <paramref name="start"/> to <paramref name="end"/> with the specified thickness, color, and cap style.
    /// </summary>
    /// <param name="start">The starting point of the segment.</param>
    /// <param name="end">The ending point of the segment.</param>
    /// <param name="thickness">The thickness of the segment.</param>
    /// <param name="color">The color of the segment.</param>
    /// <param name="capType">The type of line cap to use at the ends of the segment.</param>
    /// <param name="capPoints">The number of points used to draw the cap (for rounded or custom caps).</param>
    /// <remarks>
    /// If <paramref name="thickness"/> is less than <see cref="LineDrawingInfo.LineMinThickness"/>, it will be clamped.
    /// </remarks>
    public static void DrawSegment(Vector2 start, Vector2 end, float thickness, ColorRgba color, LineCapType capType = LineCapType.None, int capPoints = 0)
    {
        if (thickness < LineDrawingInfo.LineMinThickness) thickness = LineDrawingInfo.LineMinThickness;
        var w = end - start;
        float ls = w.X * w.X + w.Y * w.Y; // w.LengthSquared();
        if (ls <= MinSegmentDrawLengthSquared) return;
        
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
    /// Draws a segment using float coordinates for start and end points.
    /// </summary>
    /// <param name="startX">The X coordinate of the start point.</param>
    /// <param name="startY">The Y coordinate of the start point.</param>
    /// <param name="endX">The X coordinate of the end point.</param>
    /// <param name="endY">The Y coordinate of the end point.</param>
    /// <param name="thickness">The thickness of the segment.</param>
    /// <param name="color">The color of the segment.</param>
    /// <param name="capType">The type of line cap to use at the ends of the segment.</param>
    /// <param name="capPoints">The number of points used to draw the cap (for rounded or custom caps).</param>
    public static void DrawSegment(float startX, float startY, float endX, float endY, float thickness, 
        ColorRgba color, LineCapType capType = LineCapType.None, int capPoints = 0) 
        => DrawSegment(new(startX, startY), new(endX, endY), thickness, color, capType, capPoints);

    /// <summary>
    /// Draws a segment using the provided <see cref="LineDrawingInfo"/>.
    /// </summary>
    /// <param name="start">The starting point of the segment.</param>
    /// <param name="end">The ending point of the segment.</param>
    /// <param name="info">The line drawing information (thickness, color, cap type, cap points).</param>
    public static void DrawSegment(Vector2 start, Vector2 end, LineDrawingInfo info) => DrawSegment(start, end, info.Thickness, info.Color, info.CapType, info.CapPoints);
    /// <summary>
    /// Draws a segment with scaling applied from a specified origin along the segment.
    /// </summary>
    /// <param name="start">The starting point of the segment.</param>
    /// <param name="end">The ending point of the segment.</param>
    /// <param name="info">The line drawing information (thickness, color, cap type, cap points).</param>
    /// <param name="scaleFactor">The factor by which to scale the segment's length <c>(0 = no length, 1 = full length)</c>.</param>
    /// <param name="scaleOrigin">The point along the segment <c>(0 = start, 1 = end)</c> to scale from.</param>
    public static void DrawSegment(Vector2 start, Vector2 end, LineDrawingInfo info, float scaleFactor, float scaleOrigin = 0.5f)
    {
        var p = start.Lerp(end, scaleOrigin);
        var s = start - p;
        var e = end - p;

        var newStart = p + s * scaleFactor;
        var newEnd = p + e * scaleFactor;
        DrawSegment(newStart, newEnd, info);
    }

    /// <summary>
    /// Draws a segment using float coordinates for start and end points and the provided <see cref="LineDrawingInfo"/>.
    /// </summary>
    /// <param name="startX">The X coordinate of the start point.</param>
    /// <param name="startY">The Y coordinate of the start point.</param>
    /// <param name="endX">The X coordinate of the end point.</param>
    /// <param name="endY">The Y coordinate of the end point.</param>
    /// <param name="info">The line drawing information (thickness, color, cap type, cap points).</param>
    public static void DrawSegment(float startX, float startY, float endX, float endY, LineDrawingInfo info) 
        => DrawSegment(new(startX, startY), new(endX, endY), info.Thickness, info.Color, info.CapType, info.CapPoints);
    
    /// <summary>
    /// Draws a segment from <paramref name="start"/> to <paramref name="end"/> with the specified thickness, color, and different cap styles for start and end.
    /// </summary>
    /// <param name="start">The starting point of the segment.</param>
    /// <param name="end">The ending point of the segment.</param>
    /// <param name="thickness">The thickness of the segment.</param>
    /// <param name="color">The color of the segment.</param>
    /// <param name="startCapType">The type of line cap to use at the start of the segment.</param>
    /// <param name="startCapPoints">The number of points used to draw the start cap (for rounded or custom caps).</param>
    /// <param name="endCapType">The type of line cap to use at the end of the segment.</param>
    /// <param name="endCapPoints">The number of points used to draw the end cap (for rounded or custom caps).</param>
    /// <remarks>
    /// If <paramref name="thickness"/> is less than <see cref="LineDrawingInfo.LineMinThickness"/>, it will be clamped.
    /// </remarks>
    public static void DrawSegmentSeparateCaps(Vector2 start, Vector2 end, float thickness, ColorRgba color, LineCapType startCapType = LineCapType.None, int startCapPoints = 0, LineCapType endCapType = LineCapType.None, int endCapPoints = 0)
    {
        if (thickness < LineDrawingInfo.LineMinThickness) thickness = LineDrawingInfo.LineMinThickness;
        var w = end - start;
        float ls = w.X * w.X + w.Y * w.Y; // w.LengthSquared();
        if (ls <= MinSegmentDrawLengthSquared) return;
        
        var dir = w / MathF.Sqrt(ls);
        var pR = new Vector2(-dir.Y, dir.X);//perpendicular right
        var pL = new Vector2(dir.Y, -dir.X);//perpendicular left
        
        if (startCapType == LineCapType.Extended) //expand outwards
        {
            start -= dir * thickness;
        }
        else if (startCapType == LineCapType.Capped) //shrink inwards so that the line with cap is the same length
        {
            start += dir * thickness;
        }
        
        if (endCapType == LineCapType.Extended) //expand outwards
        {
            end += dir * thickness;
        }
        else if (endCapType == LineCapType.Capped) //shrink inwards so that the line with cap is the same length
        {
            end -= dir * thickness;
        }
        
        var tl = start + pL * thickness;
        var bl = start + pR * thickness;
        var br = end + pR * thickness;
        var tr = end + pL * thickness;
        
        Raylib.DrawTriangle(tl, bl, br, color.ToRayColor());
        Raylib.DrawTriangle(tl, br, tr, color.ToRayColor());

        if ((startCapType is LineCapType.None or LineCapType.Extended && endCapType is LineCapType.None or LineCapType.Extended) || 
            (startCapPoints <= 0 && endCapPoints <= 0)) return;

        //Draw Start Cap
        if (startCapType is LineCapType.Capped or LineCapType.CappedExtended && startCapPoints > 0)
        {
            if (startCapPoints == 1)
            {
                var capStart = start - dir * thickness;
                Raylib.DrawTriangle(tl, capStart, bl, color.ToRayColor());
            }
            else
            {
                var curStart = tl;
                float angleStep = (180f / (startCapPoints + 1)) * ShapeMath.DEGTORAD;
                
                for (var i = 1; i <= startCapPoints; i++)
                {
                    var pStart = start + pL.Rotate(- angleStep * i) * thickness;
                    Raylib.DrawTriangle(pStart, start, curStart, color.ToRayColor());
                    curStart = pStart;
                }
                Raylib.DrawTriangle(curStart, bl, start, color.ToRayColor());

            }
        }
        
        
        //Draw End Cap
        if (endCapType is LineCapType.Capped or LineCapType.CappedExtended && endCapPoints > 0)
        {
            if (endCapPoints == 1)
            {
                var capEnd = end + dir * thickness;
                Raylib.DrawTriangle(tr, br, capEnd, color.ToRayColor());
            }
            else
            {
                var curEnd = br;
                float angleStep = (180f / (endCapPoints + 1)) * ShapeMath.DEGTORAD;
                
                for (var i = 1; i <= endCapPoints; i++)
                {
                    var pEnd = end + pR.Rotate(- angleStep * i) * thickness;
                    Raylib.DrawTriangle(pEnd, end, curEnd, color.ToRayColor());
                    curEnd = pEnd;
                }
                Raylib.DrawTriangle(curEnd, tr, end, color.ToRayColor());
            }
        }
        
    }
  
    #endregion
    
    #region Draw Masked
    /// <summary>
    /// Helper that draws the appropriate subsegments when a segment intersects a closed mask.
    /// When <paramref name="reversedMask"/> is true the portion between <paramref name="pointA"/> and <paramref name="pointB"/>
    /// is drawn (i.e. the inside of the mask). When false the portions outside the intersection interval are drawn
    /// (two subsegments determined by which intersection is closer to the segment start).
    /// </summary>
    /// <param name="start">Original segment start point.</param>
    /// <param name="end">Original segment end point.</param>
    /// <param name="pointA">First intersection point on the segment (may be one of the two intersection points).</param>
    /// <param name="pointB">Second intersection point on the segment.</param>
    /// <param name="lineInfo">Drawing parameters used for the subsegments.</param>
    /// <param name="reversedMask">If true draw the inner piece between the two intersections; otherwise draw the outer pieces.</param>
    private static void DrawMaskedHelper(Vector2 start, Vector2 end, Vector2 pointA, Vector2 pointB, LineDrawingInfo lineInfo,  bool reversedMask)
    {
        if (reversedMask)
        {
            var newSegment = new Segment(pointA, pointB);
            newSegment.Draw(lineInfo);
        }
        else
        {
            var aDisToStartSquared = (start - pointA).LengthSquared();
            var bDisToStartSquared = (start - pointB).LengthSquared();
            if (aDisToStartSquared < bDisToStartSquared)
            {
                var seg1 = new Segment(start, pointA);
                var seg2 = new Segment(pointB, end);
                seg1.Draw(lineInfo);
                seg2.Draw(lineInfo);
            }
            else
            {
                var seg1 = new Segment(start, pointB);
                var seg2 = new Segment(pointA, end);
                seg1.Draw(lineInfo);
                seg2.Draw(lineInfo);
            }
        }
    }
    /// <summary>
    /// Draws the portion(s) of <paramref name="segment"/> that are masked by the provided <paramref name="mask"/> triangle.
    /// </summary>
    /// <param name="segment">The segment to be drawn or clipped against the triangle.</param>
    /// <param name="mask">Triangle used as a closed mask for clipping the segment.</param>
    /// <param name="lineInfo">Drawing parameters (thickness, color, cap type and cap points).</param>
    /// <param name="reversedMask">
    /// If <c>false</c> draw the parts of the segment outside the triangle (default).
    /// If <c>true</c> draw the parts of the segment inside the triangle.
    /// </param>
    /// <remarks>
    /// This method handles the following cases:
    /// - Both endpoints inside the triangle (draws whole segment when <paramref name="reversedMask"/> is true).
    /// - Both endpoints outside with two intersection points (draws subsegments determined by intersections).
    /// - Single intersection point (draws the appropriate subsegment depending on which endpoint is inside).
    /// - No intersections (draws whole segment when outside and <paramref name="reversedMask"/> is false).
    /// Intersection computations are delegated to <see cref="Segment.IntersectTriangle(Triangle)"/>.
    /// </remarks>
    public static void DrawMasked(this Segment segment, Triangle mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        bool containsStart = mask.ContainsPoint(segment.Start);
        bool containsEnd = mask.ContainsPoint(segment.End);
            
        if (containsStart && containsEnd)
        {
            if(reversedMask) segment.Draw(lineInfo);
            return;
        }
        
        var result = segment.IntersectTriangle(mask);
        if (result.a.Valid && result.b.Valid)
        {
            DrawMaskedHelper(segment.Start, segment.End, result.a.Point, result.b.Point, lineInfo, reversedMask);
        }
        else if (result.a.Valid || result.b.Valid)
        {
            var p = result.a.Valid ? result.a.Point : result.b.Point;
            if (reversedMask)
            {
                var newSegment = containsStart ? new Segment(segment.Start, p) : new Segment(p, segment.End);
                newSegment.Draw(lineInfo);
            }
            else
            {
                var newSegment = containsStart ? new Segment(p, segment.End) : new Segment(segment.Start, p);
                newSegment.Draw(lineInfo);
            }
        }
        else
        {
            if(!reversedMask) segment.Draw(lineInfo);
        }
    }
    
    /// <summary>
    /// Draws the portion(s) of <paramref name="segment"/> that are masked by the provided <paramref name="mask"/> circle.
    /// </summary>
    /// <param name="segment">The segment to be drawn or clipped against the circle.</param>
    /// <param name="mask">Circle used as a closed mask for clipping the segment.</param>
    /// <param name="lineInfo">Drawing parameters (thickness, color, cap type and cap points).</param>
    /// <param name="reversedMask">
    /// If <c>false</c> draw the parts of the segment outside the circle (default).
    /// If <c>true</c> draw the parts of the segment inside the circle.
    /// </param>
    /// <remarks>
    /// Intersection computations are delegated to <see cref="Segment.IntersectCircle(Circle)"/>.
    /// Handles cases where both endpoints are inside, both outside with two intersections,
    /// a single intersection, or no intersections.
    /// </remarks>
    public static void DrawMasked(this Segment segment, Circle mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        bool containsStart = mask.ContainsPoint(segment.Start);
        bool containsEnd = mask.ContainsPoint(segment.End);
            
        if (containsStart && containsEnd)
        {
            if(reversedMask) segment.Draw(lineInfo);
            return;
        }
        
        var result = segment.IntersectCircle(mask);
        if (result.a.Valid && result.b.Valid)
        {
            DrawMaskedHelper(segment.Start, segment.End, result.a.Point, result.b.Point, lineInfo, reversedMask);
        }
        else if (result.a.Valid || result.b.Valid)
        {
            var p = result.a.Valid ? result.a.Point : result.b.Point;
            if (reversedMask)
            {
                var newSegment = containsStart ? new Segment(segment.Start, p) : new Segment(p, segment.End);
                newSegment.Draw(lineInfo);
            }
            else
            {
                var newSegment = containsStart ? new Segment(p, segment.End) : new Segment(segment.Start, p);
                newSegment.Draw(lineInfo);
            }
        }
        else
        {
            if(!reversedMask) segment.Draw(lineInfo);
        }
    }
    
    /// <summary>
    /// Draws the portion(s) of <paramref name="segment"/> that are masked by the provided <paramref name="mask"/> rectangle.
    /// </summary>
    /// <param name="segment">The segment to be drawn or clipped against the rectangle.</param>
    /// <param name="mask">Rectangle used as a closed mask for clipping the segment.</param>
    /// <param name="lineInfo">Drawing parameters (thickness, color, cap type and cap points).</param>
    /// <param name="reversedMask">
    /// If <c>false</c> draw the parts of the segment outside the rectangle (default).
    /// If <c>true</c> draw the parts of the segment inside the rectangle.
    /// </param>
    /// <remarks>
    /// Intersection computations are delegated to <see cref="Segment.IntersectRect(Rect)"/>.
    /// This method handles cases where both endpoints are inside, both outside with two intersections,
    /// a single intersection, or no intersections.
    /// </remarks>
    public static void DrawMasked(this Segment segment, Rect mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        bool containsStart = mask.ContainsPoint(segment.Start);
        bool containsEnd = mask.ContainsPoint(segment.End);
            
        if (containsStart && containsEnd)
        {
            if(reversedMask) segment.Draw(lineInfo);
            return;
        }
        
        var result = segment.IntersectRect(mask);
        if (result.a.Valid && result.b.Valid)
        {
            DrawMaskedHelper(segment.Start, segment.End, result.a.Point, result.b.Point, lineInfo, reversedMask);
        }
        else if (result.a.Valid || result.b.Valid)
        {
            var p = result.a.Valid ? result.a.Point : result.b.Point;
            if (reversedMask)
            {
                var newSegment = containsStart ? new Segment(segment.Start, p) : new Segment(p, segment.End);
                newSegment.Draw(lineInfo);
            }
            else
            {
                var newSegment = containsStart ? new Segment(p, segment.End) : new Segment(segment.Start, p);
                newSegment.Draw(lineInfo);
            }
        }
        else
        {
            if(!reversedMask) segment.Draw(lineInfo);
        }
    }
    
    /// <summary>
    /// Draws the portion(s) of <paramref name="segment"/> that are masked by the provided <paramref name="mask"/> quad.
    /// </summary>
    /// <param name="segment">The segment to be drawn or clipped against the quad.</param>
    /// <param name="mask">Quad used as a closed mask for clipping the segment.</param>
    /// <param name="lineInfo">Drawing parameters (thickness, color, cap type and cap points).</param>
    /// <param name="reversedMask">
    /// If <c>false</c> draw the parts of the segment outside the quad (default).
    /// If <c>true</c> draw the parts of the segment inside the quad.
    /// </param>
    /// <remarks>
    /// Intersection computations are delegated to <see cref="Segment.IntersectQuad(Quad)"/>.
    /// Handles cases where both endpoints are inside, both outside with two intersections,
    /// a single intersection, or no intersections.
    /// </remarks>
    public static void DrawMasked(this Segment segment, Quad mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        bool containsStart = mask.ContainsPoint(segment.Start);
        bool containsEnd = mask.ContainsPoint(segment.End);
            
        if (containsStart && containsEnd)
        {
            if(reversedMask) segment.Draw(lineInfo);
            return;
        }
        
        var result = segment.IntersectQuad(mask);
        if (result.a.Valid && result.b.Valid)
        {
            DrawMaskedHelper(segment.Start, segment.End, result.a.Point, result.b.Point, lineInfo, reversedMask);
        }
        else if (result.a.Valid || result.b.Valid)
        {
            var p = result.a.Valid ? result.a.Point : result.b.Point;
            if (reversedMask)
            {
                var newSegment = containsStart ? new Segment(segment.Start, p) : new Segment(p, segment.End);
                newSegment.Draw(lineInfo);
            }
            else
            {
                var newSegment = containsStart ? new Segment(p, segment.End) : new Segment(segment.Start, p);
                newSegment.Draw(lineInfo);
            }
        }
        else
        {
            if(!reversedMask) segment.Draw(lineInfo);
        }
    }
   
    /// <summary>
    /// Draws the portion(s) of <paramref name="segment"/> that are masked by the provided <paramref name="mask"/> polygon.
    /// </summary>
    /// <param name="segment">The segment to be drawn or clipped against the polygon.</param>
    /// <param name="mask">Polygon used as a closed mask for clipping the segment.</param>
    /// <param name="lineInfo">Drawing parameters (thickness, color, cap type and cap points).</param>
    /// <param name="reversedMask">
    /// If <c>false</c> draw the parts of the segment outside the polygon (default).
    /// If <c>true</c> draw the parts of the segment inside the polygon.
    /// </param>
    /// <remarks>
    /// Intersection computations are delegated to <see cref="Segment.IntersectPolygon(Polygon, int)"/>.
    /// Handles cases where there are zero, one, or multiple intersection points. When multiple intersections are returned,
    /// the method augments the intersection list with the segment endpoints and sorts them to determine alternating
    /// inside/outside intervals before drawing the appropriate subsegments.
    /// </remarks>
    public static void DrawMasked(this Segment segment, Polygon mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        bool containsStart = mask.ContainsPoint(segment.Start);

        var result = segment.IntersectPolygon(mask);
        if(result == null || result.Count <= 0)
        {
            // If reversedMask: draw when fully inside (containsStart == true)
            // If not reversedMask: draw when fully outside (containsStart == false)
            if ((reversedMask && containsStart) || (!reversedMask && !containsStart))
                segment.Draw(lineInfo);
            return;
        }
        if (result.Count == 1)
        {
            var p = result[0].Point;
            if (reversedMask)
            {
                var newSegment = containsStart ? new Segment(segment.Start, p) : new Segment(p, segment.End);
                newSegment.Draw(lineInfo);
            }
            else
            {
                var newSegment = containsStart ? new Segment(p, segment.End) : new Segment(segment.Start, p);
                newSegment.Draw(lineInfo);
            }

            return;
        }
        result.Add(new IntersectionPoint(segment.Start, segment.Normal));
        result.Add(new IntersectionPoint(segment.End, segment.Normal));
        if (result.SortClosestFirst(segment.Start))
        {
            if (reversedMask)
            {
                for (int i = containsStart ? 0 : 1; i < result.Count - 1; i += 2)
                {
                    var p1 = result[i].Point;
                    var p2 = result[i + 1].Point;
                    var s = new Segment(p1, p2);
                    s.Draw(lineInfo);
                }
            }
            else
            {
                for (int i = containsStart ? 1 : 0; i < result.Count - 1; i += 2)
                {
                    var p1 = result[i].Point;
                    var p2 = result[i + 1].Point;
                    var s = new Segment(p1, p2);
                    s.Draw(lineInfo);
                }
            }
        }
    }
    
    /// <summary>
    /// Draws the portion(s) of <paramref name="segment"/> that are masked by a generic closed-shape <paramref name="mask"/>.
    /// This generic overload dispatches to the concrete shape-specific overload (Circle, Triangle, Quad, Rect, or Polygon)
    /// based on <c>mask.GetClosedShapeType()</c>.
    /// </summary>
    /// <typeparam name="T">A type that implements <see cref="IClosedShapeTypeProvider"/> and represents the mask shape.</typeparam>
    /// <param name="segment">The segment to be drawn or clipped against the mask.</param>
    /// <param name="mask">The mask shape that provides its closed-shape type. The method will attempt to cast to the concrete shape type.</param>
    /// <param name="lineInfo">Drawing parameters (thickness, color, cap type and cap points).</param>
    /// <param name="reversedMask">
    /// If <c>false</c> draw the parts of the segment outside the mask (default).
    /// If <c>true</c> draw the parts of the segment inside the mask.
    /// </param>
    /// <remarks>
    /// If the concrete mask type is not one of the supported closed shapes, no drawing occurs.
    /// Use the concrete overloads when the exact mask type is known to avoid the runtime dispatch and cast.
    /// </remarks>
    public static void DrawMasked<T>(this Segment segment, T mask, LineDrawingInfo lineInfo, bool reversedMask = false) where T : IClosedShapeTypeProvider
    {
        switch (mask.GetClosedShapeType())
        {
            case ClosedShapeType.Circle:
                if (mask is Circle circle)
                {
                    segment.DrawMasked(circle, lineInfo, reversedMask);
                }
                break;
            case ClosedShapeType.Triangle:
                if (mask is Triangle triangle)
                {
                    segment.DrawMasked(triangle, lineInfo, reversedMask);
                }
                break;
            case ClosedShapeType.Quad:
                if (mask is Quad quad)
                {
                    segment.DrawMasked(quad, lineInfo, reversedMask);
                }
                break;
            case ClosedShapeType.Rect:
                if (mask is Rect rect)
                {
                    segment.DrawMasked(rect, lineInfo, reversedMask);
                }
                break;
            case ClosedShapeType.Poly:
                if (mask is Polygon poly)
                {
                    segment.DrawMasked(poly, lineInfo, reversedMask);
                }
                break;
        }
    }
    #endregion
    
    #region Draw

    /// <summary>
    /// Draws the specified <see cref="Segment"/> with the given thickness, color, and cap style.
    /// </summary>
    /// <param name="segment">The segment to draw.</param>
    /// <param name="thickness">The thickness of the segment.</param>
    /// <param name="color">The color of the segment.</param>
    /// <param name="capType">The type of line cap to use at the ends of the segment.</param>
    /// <param name="capPoints">The number of points used to draw the cap (for rounded or custom caps).</param>
    public static void Draw(this Segment segment, float thickness, ColorRgba color, LineCapType capType = LineCapType.None, int capPoints = 0 ) 
        => DrawSegment(segment.Start, segment.End, thickness, color, capType, capPoints);
    
    /// <summary>
    /// Draws the specified <see cref="Segment"/> using separate cap styles for the start and end.
    /// </summary>
    /// <param name="segment">The segment to draw.</param>
    /// <param name="thickness">The thickness of the segment. Values below <see cref="LineDrawingInfo.LineMinThickness"/> are clamped.</param>
    /// <param name="color">Color used to draw the segment.</param>
    /// <param name="startCapType">Cap type to apply at the segment start.</param>
    /// <param name="startCapPoints">Number of points for the start cap (rounded/custom). If &lt;= 0 a simple cap is used.</param>
    /// <param name="endCapType">Cap type to apply at the segment end.</param>
    /// <param name="endCapPoints">Number of points for the end cap (rounded/custom). If &lt;= 0 a simple cap is used.</param>
    public static void DrawSeparateCaps(this Segment segment, float thickness, ColorRgba color, LineCapType startCapType = LineCapType.None, int startCapPoints = 0, LineCapType endCapType = LineCapType.None, int endCapPoints = 0) 
        => DrawSegmentSeparateCaps(segment.Start, segment.End, thickness, color, startCapType, startCapPoints, endCapType, endCapPoints);
    
    /// <summary>
    /// Draws the specified <see cref="Segment"/> using the provided <see cref="LineDrawingInfo"/>.
    /// </summary>
    /// <param name="segment">The segment to draw.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, cap points).</param>
    public static void Draw(this Segment segment, LineDrawingInfo lineInfo) 
        => DrawSegment(segment.Start, segment.End, lineInfo);
    /// <summary>
    /// Draws the specified <see cref="Segment"/> with rotation and origin, using the provided <see cref="LineDrawingInfo"/>.
    /// </summary>
    /// <param name="segment">The segment to draw.</param>
    /// <param name="originF">The point to rotate the segment around (0 = start, 1 = end).</param>
    /// <param name="angleRad">The rotation angle in radians.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, cap points).</param>
    public static void Draw(this Segment segment, float originF, float angleRad, LineDrawingInfo lineInfo)
    {
        if (angleRad != 0f)
        {
            segment.ChangeRotation(angleRad, originF).Draw(lineInfo);
            return;

        }
        
        DrawSegment(segment.Start, segment.End, lineInfo);
    }
    
    #endregion

    #region Draw Segments
    /// <summary>
    /// Draws all segments in the specified <see cref="Segments"/> collection using the provided <see cref="LineDrawingInfo"/>.
    /// </summary>
    /// <param name="segments">The collection of segments to draw.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, cap points).</param>
    public static void Draw(this Segments segments, LineDrawingInfo lineInfo)
    {
        if (segments.Count <= 0) return;
        foreach (var seg in segments)
        {
            seg.Draw(lineInfo);
        }
    }

    /// <summary>
    /// Draws all segments in the specified <see cref="Segments"/> collection, cycling through the provided colors.
    /// </summary>
    /// <param name="segments">The collection of segments to draw.</param>
    /// <param name="thickness">The thickness of each segment.</param>
    /// <param name="colors">A list of colors to use for the segments.
    /// Colors are cycled if there are more segments than colors.</param>
    /// <param name="capType">The type of line cap to use at the ends of the segments.</param>
    /// <param name="capPoints">The number of points used to draw the cap (for rounded or custom caps).</param>
    public static void Draw(this Segments segments, float thickness, List<ColorRgba> colors, LineCapType capType = LineCapType.None, int capPoints = 0)
    {
        if (segments.Count <= 0 || colors.Count <= 0) return;
        // LineDrawingInfo info = new(thickness, ColorRgba.White, capType, capPoints);
        for (var i = 0; i < segments.Count; i++)
        {
            var c = colors[i % colors.Count];
            segments[i].Draw(thickness, c, capType, capPoints);
        }
    }

    #endregion
    
    #region Draw Percentage
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
    /// <remarks>
    /// Useful for animating the drawing of a segment.
    /// </remarks>
    public static void DrawSegmentPercentage(Vector2 start, Vector2 end, float f, float thickness, ColorRgba color,
        LineCapType capType = LineCapType.None, int capPoints = 0)
    {
        if (f == 0) return;
        if (f < 0)
        {
            var newStart = end.Lerp(start, f * -1);
            DrawSegment(newStart, end, thickness, color, capType, capPoints);
        }
        else
        {
            var newEnd = start.Lerp(end, f);
            DrawSegment(start, newEnd, thickness, color, capType, capPoints);
        }
        
        
    }
    
    /// <summary>
    /// Draws a portion of a segment using the provided <see cref="LineDrawingInfo"/> and percentage <paramref name="f"/>.
    /// </summary>
    /// <param name="start">The starting point of the segment.</param>
    /// <param name="end">The ending point of the segment.</param>
    /// <param name="f">The percentage of the segment to draw. Negative values draw from end to start.</param>
    /// <param name="info">The line drawing information (thickness, color, cap type, cap points).</param>
    /// <remarks>
    /// Useful for animating the drawing of a segment.
    /// </remarks>
    public static void DrawSegmentPercentage(Vector2 start, Vector2 end, float f, LineDrawingInfo info) 
        => DrawSegmentPercentage(start, end, f, info.Thickness, info.Color, info.CapType, info.CapPoints);
    
    /// <summary>
    /// Draws a portion of the specified <see cref="Segment"/> based on the percentage <paramref name="f"/>.
    /// </summary>
    /// <param name="segment">The segment to draw.</param>
    /// <param name="f">The percentage of the segment to draw. Negative values draw from end to start.</param>
    /// <param name="thickness">The thickness of the segment.</param>
    /// <param name="color">The color of the segment.</param>
    /// <param name="capType">The type of line cap to use at the ends of the segment.</param>
    /// <param name="capPoints">The number of points used to draw the cap (for rounded or custom caps).</param>
    /// <remarks>
    /// Useful for animating the drawing of a segment.
    /// </remarks>
    public static void DrawPercentage(this Segment segment, float f, float thickness, ColorRgba color, LineCapType capType = LineCapType.None, int capPoints = 0 ) 
        => DrawSegmentPercentage(segment.Start, segment.End, f, thickness, color, capType, capPoints);
    
    /// <summary>
    /// Draws a portion of the specified <see cref="Segment"/> using the provided <see cref="LineDrawingInfo"/> and percentage <paramref name="f"/>.
    /// </summary>
    /// <param name="segment">The segment to draw.</param>
    /// <param name="f">The percentage of the segment to draw. Negative values draw from end to start.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, cap points).</param>
    /// <remarks>
    /// Useful for animating the drawing of a segment.
    /// </remarks>
    public static void DrawPercentage(this Segment segment, float f, LineDrawingInfo lineInfo) 
        => DrawSegmentPercentage(segment.Start, segment.End, f, lineInfo);

    #endregion
    
    #region Draw Glow
    
    /// <summary>
    /// Draws a glowing segment from <paramref name="start"/> to <paramref name="end"/> by interpolating width and color.
    /// </summary>
    /// <param name="start">The starting point of the segment.</param>
    /// <param name="end">The ending point of the segment.</param>
    /// <param name="width">The starting width of the glow. Should be bigger than <c>endWidth</c>.</param>
    /// <param name="endWidth">The ending width of the glow. Should be smaller than <c>width</c>.</param>
    /// <param name="color">The starting color of the glow.</param>
    /// <param name="endColorRgba">The ending color of the glow.</param>
    /// <param name="steps">The number of steps to interpolate between start and end.</param>
    /// <param name="capType">The type of line cap to use at the ends of the segment.</param>
    /// <param name="capPoints">The number of points used to draw the cap (for rounded or custom caps).</param>
    /// <remarks>
    /// Creates a gradient glow effect along the segment.
    /// </remarks>
    public static void DrawSegmentGlow(Vector2 start, Vector2 end, float width, float endWidth, ColorRgba color, ColorRgba endColorRgba, int steps, LineCapType capType = LineCapType.None, int capPoints = 0)
    {
        float wStep = (endWidth - width) / steps;

        int rStep = (endColorRgba.R - color.R) / steps;
        int gStep = (endColorRgba.G - color.G) / steps;
        int bStep = (endColorRgba.B - color.B) / steps;
        int aStep = (endColorRgba.A - color.A) / steps;

        for (int i = steps; i >= 0; i--)
        {
            DrawSegment
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

    /// <summary>
    /// Draws a glowing effect along the specified <see cref="Segment"/>.
    /// </summary>
    /// <param name="segment">The segment to draw with a glow effect.</param>
    /// <param name="width">The starting width of the glow.</param>
    /// <param name="endWidth">The ending width of the glow.</param>
    /// <param name="color">The starting color of the glow.</param>
    /// <param name="endColorRgba">The ending color of the glow.</param>
    /// <param name="steps">The number of steps to interpolate between start and end.</param>
    /// <param name="capType">The type of line cap to use at the ends of the segment.</param>
    /// <param name="capPoints">The number of points used to draw the cap (for rounded or custom caps).</param>
    /// <remarks>
    /// Creates a gradient glow effect along the segment.
    /// </remarks>
    public static void DrawGlow(this Segment segment, float width, float endWidth, ColorRgba color, ColorRgba endColorRgba, int steps, LineCapType capType = LineCapType.None, int capPoints = 0)
    {
        DrawSegmentGlow(segment.Start, segment.End, width, endWidth, color, endColorRgba, steps, capType, capPoints);
    }

    /// <summary>
    /// Draws a glowing effect along all segments in the specified <see cref="Segments"/> collection.
    /// </summary>
    /// <param name="segments">The collection of segments to draw with a glow effect.</param>
    /// <param name="width">The starting width of the glow.</param>
    /// <param name="endWidth">The ending width of the glow.</param>
    /// <param name="color">The starting color of the glow.</param>
    /// <param name="endColorRgba">The ending color of the glow.</param>
    /// <param name="steps">The number of steps to interpolate between start and end.</param>
    /// <param name="capType">The type of line cap to use at the ends of the segments.</param>
    /// <param name="capPoints">The number of points used to draw the cap (for rounded or custom caps).</param>
    /// <remarks>
    /// Creates a gradient glow effect along each segment.
    /// </remarks>
    public static void DrawGlow(this Segments segments, float width, float endWidth, ColorRgba color, ColorRgba endColorRgba, int steps, LineCapType capType = LineCapType.None, int capPoints = 0)
    {
        foreach (var seg in segments)
        {
            seg.DrawGlow(width, endWidth, color, endColorRgba, steps, capType, capPoints);
        }
    }

    #endregion
    
    #region Draw Scaled
        /// <summary>
    /// Draws a segment scaled towards a specified origin along the segment.
    /// </summary>
    /// <param name="s">The segment to draw.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, capPoints).</param>
    /// <param name="sideScaleFactor">The scale factor for each side <c>(0 = No segment, 1 = Full Segment)</c></param>
    /// <param name="sideScaleOrigin">The point along the segment <c>(0 = Start, 1 = End) to scale from.</c></param>
    /// <remarks>
    /// Useful for creating dynamic or animated segment effects.
    /// </remarks>
    public static void DrawScaled(this Segment s, LineDrawingInfo lineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0) return;
        if (sideScaleFactor >= 1)
        {
            s.Draw(lineInfo);
            return;
        }
        
        DrawSegment(s.Start, s.End, lineInfo, sideScaleFactor, sideScaleOrigin);
    }

    /// <summary>
    /// Draws a segment scaled towards a specified origin and rotated by a given angle.
    /// </summary>
    /// <param name="s">The segment to draw.</param>
    /// <param name="originF">The point to rotate the segment around <c>(0 = Start, 1 = End)</c>.</param>
    /// <param name="angleRad">The rotation angle in radians.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, cap points).</param>
    /// <param name="sideScaleFactor">The scale factor for each side <c>(0 = No segment, 1 = Full Segment)</c></param>
    /// <param name="sideScaleOrigin">The point along the segment <c>(0 = Start, 1 = End) to scale from.</c></param>
    /// <remarks>
    /// Useful for creating dynamic or animated segment effects with rotation.
    /// </remarks>
    public static void DrawScaled(this Segment s, float originF, float angleRad, LineDrawingInfo lineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0) return;
        if (sideScaleFactor >= 1)
        {
            s.Draw(originF, angleRad, lineInfo);
            return;
        }
        
        if(angleRad == 0f) DrawSegment(s.Start, s.End, lineInfo, sideScaleFactor, sideScaleOrigin);
        else
        {
            var origin = s.GetPoint(originF);
            var rStart = origin +  (s.Start - origin).Rotate(angleRad);
            var rEnd = origin + (s.End - origin).Rotate(angleRad);
            DrawSegment(rStart, rEnd, lineInfo, sideScaleFactor, sideScaleOrigin);
        }
    }

    #endregion
    
    #region Draw Vertices
    /// <summary>
    /// Draws circles at the start and end vertices of the specified <see cref="Segment"/>.
    /// </summary>
    /// <param name="segment">The segment whose vertices to draw.</param>
    /// <param name="vertexRadius">The radius of the vertex circles.</param>
    /// <param name="color">The color of the vertex circles.</param>
    /// <param name="vertexSegments">The number of segments to use for drawing the circles (default is 16).</param>
    public static void DrawVertices(this Segment segment, float vertexRadius, ColorRgba color, int vertexSegments = 16)
    {
        segment.Start.Draw( vertexRadius, color, vertexSegments);
        segment.End.Draw(vertexRadius, color, vertexSegments);
    }

    #endregion
    
    #region Draw Cap

    //Useless - DrawRoundCap is all that is needed
    // public static void DrawSegmentCap(Vector2 p, Vector2 dir, float thickness, ColorRgba color, LineCapType capType = LineCapType.None, int capPoints = 0)
    // {
    //   
    //     if (capType == LineCapType.None || capPoints <= 0) return;
    //     if (thickness < LineDrawingInfo.LineMinThickness) thickness = LineDrawingInfo.LineMinThickness;
    //     float ls = dir.LengthSquared();
    //     if (ls <= MinSegmentDrawLengthSquared) return;
    //     
    //     var pR = new Vector2(-dir.Y, dir.X);//perpendicular right
    //     var pL = new Vector2(dir.Y, -dir.X);//perpendicular left
    //
    //     var capStart = p;
    //     var capEnd = p + dir * thickness;
    //     var capStartLeft = capStart + pL * thickness;
    //     var capStartRight = capStart + pR * thickness;
    //     
    //     if (capType == LineCapType.Extended) //expand outwards
    //     {
    //         var br = capEnd + pR * thickness;
    //         var tr = capEnd + pL * thickness;
    //         Raylib.DrawTriangle(capStartLeft, capStartRight, br, color.ToRayColor());
    //         Raylib.DrawTriangle(capStartLeft, br, tr, color.ToRayColor());
    //         return;
    //     }
    //     
    //     if (capType == LineCapType.Capped)//shrink inwards so that the line with cap is the same length
    //     {
    //         capEnd = capStart;
    //         capStart -= dir * thickness;
    //         capStartLeft -= dir * thickness;
    //         capStartRight -= dir * thickness;
    //         
    //     }
    //
    //     //Draw Cap
    //     if (capPoints == 1)
    //     {
    //         Raylib.DrawTriangle(capEnd, capStartLeft, capStart, color.ToRayColor());
    //         Raylib.DrawTriangle(capEnd, capStart, capStartRight, color.ToRayColor());
    //     }
    //     else
    //     {
    //         var curStart = capStartLeft;
    //         float angleStep = (180f / (capPoints + 1)) * ShapeMath.DEGTORAD;
    //             
    //         for (var i = 1; i <= capPoints; i++)
    //         {
    //             var pStart = capStart + pL.Rotate(- angleStep * i) * thickness;
    //             Raylib.DrawTriangle(pStart, capStart, curStart, color.ToRayColor());
    //             curStart = pStart;
    //         }
    //         Raylib.DrawTriangle(curStart, capStartRight, capStart, color.ToRayColor());
    //
    //     }
    // }

    public static void DrawRoundCap(Vector2 center, Vector2 dir, float radius, int capPoints, ColorRgba color)
    {
        if(capPoints <= 0) return;
        dir = -dir;
        var pR = new Vector2(-dir.Y, dir.X);//perpendicular right
        var pL = new Vector2(dir.Y, -dir.X);//perpendicular left
        
        var capStartLeft = center + pL * radius;
        var capStartRight = center + pR * radius;
        
        var curStart = capStartLeft;
        float angleStep = (180f / (capPoints + 1)) * ShapeMath.DEGTORAD;
                
        for (var i = 1; i <= capPoints; i++)
        {
            var pStart = center + pL.Rotate(- angleStep * i) * radius;
            Raylib.DrawTriangle(pStart, center, curStart, color.ToRayColor());
            curStart = pStart;
        }
        Raylib.DrawTriangle(curStart, capStartRight, center, color.ToRayColor());
    }
    #endregion
    
}