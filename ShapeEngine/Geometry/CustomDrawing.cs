using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry;

/// <summary>
/// Provides custom drawing utilities for shapes, arrows, and collision points.
/// </summary>
/// <remarks>
/// This static class contains helper methods for drawing pixels, arrows, and visualizing collision points using Raylib.
/// </remarks>
public static class CustomDrawing
{
    #region Pixel

    /// <summary>
    /// Draws a single pixel at the specified position with the given color.
    /// </summary>
    /// <param name="pos">The position where the pixel will be drawn.</param>
    /// <param name="color">The color of the pixel.</param>
    public static void DrawPixel(Vector2 pos, ColorRgba color) => Raylib.DrawPixelV(pos, color.ToRayColor()); 

    /// <summary>
    /// Draws a single pixel at the specified coordinates with the given color.
    /// </summary>
    /// <param name="x">The X coordinate of the pixel.</param>
    /// <param name="y">The Y coordinate of the pixel.</param>
    /// <param name="color">The color of the pixel.</param>
    public static void DrawPixel(float x, float y, ColorRgba color) => Raylib.DrawPixelV(new(x, y), color.ToRayColor());
    #endregion
    
    #region Intersection

    /// <summary>
    /// Draws intersection points and their normals for a set of collision points.
    /// </summary>
    /// <param name="colPoints">The collection of collision points to draw.</param>
    /// <param name="lineThickness">The thickness of the lines and circles drawn.</param>
    /// <param name="intersectColorRgba">The color used for intersection points.</param>
    /// <param name="normalColorRgba">The color used for normal vectors.</param>
    /// <remarks>
    /// Each collision point is visualized as a small circle, and its normal is drawn as a line.
    /// </remarks>
    public static void Draw(this CollisionPoints colPoints, float lineThickness, ColorRgba intersectColorRgba, ColorRgba normalColorRgba)
    {
        if ( colPoints.Count <= 0) return;
        
        foreach (var i in colPoints)
        {
            CircleDrawing.DrawCircle(i.Point, lineThickness * 2f, intersectColorRgba, 12);
            SegmentDrawing.DrawSegment(i.Point, i.Point + i.Normal * lineThickness * 10f, lineThickness, normalColorRgba);
        }
    }
    #endregion

    #region Arrow Drawing

    /// <summary>
    /// Calculates the geometric points for drawing an arrow with a fixed head width and length.
    /// </summary>
    /// <param name="tailPoint">The starting point of the arrow (tail).</param>
    /// <param name="headPoint">The end point of the arrow (head).</param>
    /// <param name="headWidth">The width of the arrow head.</param>
    /// <param name="headLength">The length of the arrow head.</param>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    /// <item><description>The tail segment of the arrow.</description></item>
    /// <item><description>The triangle representing the arrow head.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// Returns empty shapes if parameters are invalid.
    /// </remarks>
    public static (Segment tail, Triangle head) CalculateArrowPoints(Vector2 tailPoint, Vector2 headPoint, float headWidth, float headLength)
    {
        if(headWidth <= 0 || headLength <= 0) return (new(), new());
        
        var v = headPoint - tailPoint;
        var l = v.Length();
        if(l <= 0) return (new(), new());

        var dir = v / l;
        var tailEnd = tailPoint;
        if (headLength < l)
        {
            var tailLength = l - headLength;
            tailEnd = tailPoint + dir * tailLength;
        }

        var pl = dir.GetPerpendicularLeft();
        var pr = -pl;

        var b = tailEnd + pl * headWidth * 0.5f;
        var c = tailEnd + pr * headWidth * 0.5f;
        return (new(tailPoint, tailEnd), new(headPoint, b, c));
    }

    /// <summary>
    /// Calculates the geometric points for drawing an arrow with a fixed head width and a head length defined as a factor of the total arrow length.
    /// </summary>
    /// <param name="tailPoint">The starting point of the arrow (tail).</param>
    /// <param name="headPoint">The end point of the arrow (head).</param>
    /// <param name="headWidth">The width of the arrow head.</param>
    /// <param name="headLengthFactor">The length of the arrow head as a fraction of the total arrow length (0-1).</param>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    /// <item><description>The tail segment of the arrow.</description></item>
    /// <item><description>The triangle representing the arrow head.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// Returns empty shapes if parameters are invalid.
    /// </remarks>
    public static (Segment tail, Triangle head) CalculateArrowPoints2(Vector2 tailPoint, Vector2 headPoint, float headWidth, float headLengthFactor)
    {
        if(headWidth <= 0 || headLengthFactor <= 0) return (new(), new());
        
        var v = headPoint - tailPoint;
        var l = v.Length();
        if(l <= 0) return (new(), new());

        var dir = v / l;
        var tailLength = l * (1f - headLengthFactor);
        var tailEnd = tailPoint + dir * tailLength;

        var pl = dir.GetPerpendicularLeft();
        var pr = -pl;

        var b = tailEnd + pl * headWidth * 0.5f;
        var c = tailEnd + pr * headWidth * 0.5f;
        return (new(tailPoint, tailEnd), new(headPoint, b, c));
    }

    /// <summary>
    /// Calculates the geometric points for drawing an arrow with head width and length defined as factors of the total arrow length.
    /// </summary>
    /// <param name="tailPoint">The starting point of the arrow (tail).</param>
    /// <param name="headPoint">The end point of the arrow (head).</param>
    /// <param name="headWidthFactor">The width of the arrow head as a fraction of the total arrow length (0-1).</param>
    /// <param name="headLengthFactor">The length of the arrow head as a fraction of the total arrow length (0-1).</param>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    /// <item><description>The tail segment of the arrow.</description></item>
    /// <item><description>The triangle representing the arrow head.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// Returns empty shapes if parameters are invalid.
    /// </remarks>
    public static (Segment tail, Triangle head) CalculateArrowPoints3(Vector2 tailPoint, Vector2 headPoint, float headWidthFactor, float headLengthFactor)
    {
        if(headWidthFactor <= 0 || headLengthFactor <= 0) return (new(), new());
        
        var v = headPoint - tailPoint;
        var l = v.Length();
        if(l <= 0) return (new(), new());

        var dir = v / l;
        var tailLength = l * (1f - headLengthFactor);
        var tailEnd = tailPoint + dir * tailLength;

        var pl = dir.GetPerpendicularLeft();
        var pr = -pl;

        var headWidth = l * headWidthFactor;
        var b = tailEnd + pl * headWidth * 0.5f;
        var c = tailEnd + pr * headWidth * 0.5f;
        return (new(tailPoint, tailEnd), new(headPoint, b, c));
    }
    
    /// <summary>
    /// Draws an arrow from the tail point to the head point with a fixed head width and length.
    /// </summary>
    /// <param name="tailPoint">The starting point of the arrow (tail).</param>
    /// <param name="headPoint">The end point of the arrow (head).</param>
    /// <param name="headWidth">The width of the arrow head.</param>
    /// <param name="headLength">The length of the arrow head.</param>
    /// <param name="info">Drawing information for the arrow's tail (line color, thickness, etc.).</param>
    /// <param name="headFillColor">The fill color of the arrow head.</param>
    /// <remarks>
    /// The arrow's tail is drawn as a line, and the head as a filled triangle.
    /// </remarks>
    public static void DrawArrow(Vector2 tailPoint, Vector2 headPoint, float headWidth, float headLength, LineDrawingInfo info, ColorRgba headFillColor)
    {
        if(headWidth <= 0 || headLength <= 0) return;
        if(info.Color.A <= 0) return;
        
        var v = headPoint - tailPoint;
        var l = v.Length();
        if(l <= 0) return;

        var dir = v / l;
        var tailEnd = tailPoint;
        if (headLength < l)
        {
            var tailLength = l - headLength;
            tailEnd = tailPoint + dir * tailLength;
            SegmentDrawing.DrawSegment(tailPoint, tailEnd, info);
        }

        var pl = dir.GetPerpendicularLeft();
        var pr = -pl;

        var b = tailEnd + pl * headWidth * 0.5f;
        var c = tailEnd + pr * headWidth * 0.5f;
        if(headFillColor.A > 0) TriangleDrawing.DrawTriangle(headPoint, b, c, headFillColor);
        TriangleDrawing.DrawTriangleLines(headPoint, b, c, info);
    }

    /// <summary>
    /// Draws an arrow from the tail point to the head point with a fixed head width and a head length defined as a factor of the total arrow length.
    /// </summary>
    /// <param name="tailPoint">The starting point of the arrow (tail).</param>
    /// <param name="headPoint">The end point of the arrow (head).</param>
    /// <param name="headWidth">The width of the arrow head.</param>
    /// <param name="headLengthFactor">The length of the arrow head as a fraction of the total arrow length (0-1).</param>
    /// <param name="info">Drawing information for the arrow's tail (line color, thickness, etc.).</param>
    /// <param name="headFillColor">The fill color of the arrow head.</param>
    /// <remarks>
    /// The arrow's tail is drawn as a line, and the head as a filled triangle.
    /// </remarks>
    public static void DrawArrow2(Vector2 tailPoint, Vector2 headPoint, float headWidth, float headLengthFactor, LineDrawingInfo info, ColorRgba headFillColor)
    {
        if(headWidth <= 0 || headLengthFactor <= 0) return;
        if(info.Color.A <= 0) return;
        var v = headPoint - tailPoint;
        var l = v.Length();
        if(l <= 0) return;

        var dir = v / l;
        var tailLength = l * (1f - headLengthFactor);
        var tailEnd = tailPoint + dir * tailLength;
        SegmentDrawing.DrawSegment(tailPoint, tailEnd, info);

        var pl = dir.GetPerpendicularLeft();
        var pr = -pl;

        var b = tailEnd + pl * headWidth * 0.5f;
        var c = tailEnd + pr * headWidth * 0.5f;
        if(headFillColor.A > 0) TriangleDrawing.DrawTriangle(headPoint, b, c, headFillColor);
        TriangleDrawing.DrawTriangleLines(headPoint, b, c, info);
    }

    /// <summary>
    /// Draws an arrow from the tail point to the head point with head width and length defined as factors of the total arrow length.
    /// </summary>
    /// <param name="tailPoint">The starting point of the arrow (tail).</param>
    /// <param name="headPoint">The end point of the arrow (head).</param>
    /// <param name="headWidthFactor">The width of the arrow head as a fraction of the total arrow length (0-1).</param>
    /// <param name="headLengthFactor">The length of the arrow head as a fraction of the total arrow length (0-1).</param>
    /// <param name="info">Drawing information for the arrow's tail (line color, thickness, etc.).</param>
    /// <param name="headFillColor">The fill color of the arrow head.</param>
    /// <remarks>
    /// The arrow's tail is drawn as a line, and the head as a filled triangle.
    /// </remarks>
    public static void DrawArrow3(Vector2 tailPoint, Vector2 headPoint, float headWidthFactor, float headLengthFactor, LineDrawingInfo info, ColorRgba headFillColor)
    {
        if(headWidthFactor <= 0 || headLengthFactor <= 0) return;
        if(info.Color.A <= 0) return;
        
        var v = headPoint - tailPoint;
        var l = v.Length();
        if(l <= 0) return;

        var dir = v / l;
        var tailLength = l * (1f - headLengthFactor);
        var tailEnd = tailPoint + dir * tailLength;
        SegmentDrawing.DrawSegment(tailPoint, tailEnd, info);

        var pl = dir.GetPerpendicularLeft();
        var pr = -pl;

        var headWidth = l * headWidthFactor;
        var b = tailEnd + pl * headWidth * 0.5f;
        var c = tailEnd + pr * headWidth * 0.5f;
        if(headFillColor.A > 0) TriangleDrawing.DrawTriangle(headPoint, b, c, headFillColor);
        TriangleDrawing.DrawTriangleLines(headPoint, b, c, info);
    }
    #endregion
}