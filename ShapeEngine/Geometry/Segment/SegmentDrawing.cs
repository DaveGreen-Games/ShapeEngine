using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.Segment;

/// <summary>
/// Provides static methods for drawing line segments and collections of segments with various styles, thicknesses, and effects.
/// </summary>
/// <remarks>
/// This class contains extension methods for <see cref="Segment"/> and <see cref="Segments"/> to simplify drawing operations.
/// </remarks>
public static class SegmentDrawing
{
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
    /// Draws the specified <see cref="Segment"/> using the provided <see cref="LineDrawingInfo"/>.
    /// </summary>
    /// <param name="segment">The segment to draw.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, cap points).</param>
    public static void Draw(this Segment segment, LineDrawingInfo lineInfo) 
        => DrawSegment(segment.Start, segment.End, lineInfo);

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

    /// <summary>
    /// Draws a glowing segment from <paramref name="start"/> to <paramref name="end"/> by interpolating width and color.
    /// </summary>
    /// <param name="start">The starting point of the segment.</param>
    /// <param name="end">The ending point of the segment.</param>
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

}