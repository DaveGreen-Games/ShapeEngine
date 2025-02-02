using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Lib.Drawing;

public static class CustomDrawing
{
    #region Pixel
    public static void DrawPixel(Vector2 pos, ColorRgba color) => Raylib.DrawPixelV(pos, color.ToRayColor()); 
    public static void DrawPixel(float x, float y, ColorRgba color) => Raylib.DrawPixelV(new(x, y), color.ToRayColor());
    #endregion
    
    #region Intersection
    public static void Draw(this CollisionPoints colPoints, float lineThickness, ColorRgba intersectColorRgba, ColorRgba normalColorRgba)
    {
        if ( colPoints.Count <= 0) return;
        
        foreach (var i in colPoints)
        {
            CircleDrawing.DrawCircle(i.Point, lineThickness * 2f, intersectColorRgba, 12);
            SegmentDrawing.DrawSegment(i.Point, i.Point + i.Normal * lineThickness * 10f, lineThickness, normalColorRgba);
            // Segment normal = new(i.Point, i.Point + i.Normal * lineThickness * 10f);
            // normal.Draw(lineThickness, normalColorRgba);
        }
    }
    #endregion

    #region Arrow Drawing
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