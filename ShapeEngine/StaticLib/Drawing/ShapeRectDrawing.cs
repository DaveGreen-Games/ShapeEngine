using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.StaticLib.Drawing;

public static class ShapeRectDrawing
{
    
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
            ShapeSegmentDrawing.DrawSegment(bounds.TopLeft + rowSpacing * row, bounds.TopRight + rowSpacing * row, lineThickness, color);
        }
        Vector2 colSpacing = new(bounds.Width / grid.Cols, 0f);
        for (int col = 0; col < grid.Cols + 1; col++)
        {
            ShapeSegmentDrawing.DrawSegment(bounds.TopLeft + colSpacing * col, bounds.BottomLeft + colSpacing * col, lineThickness, color);
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
            ShapeSegmentDrawing.DrawSegment(tl + xOffset * i, bl + xOffset * i, lineInfo);
            ShapeSegmentDrawing.DrawSegment(tl + yOffset * i, tr + yOffset * i, lineInfo);
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
        ShapeQuadDrawing.DrawQuad(a,b,c,d, color);
        
        // Draw(new Rect(topLeft, bottomRight), pivot, rotDeg, color);
    }
    
    
    public static void DrawRectLines(Vector2 topLeft, Vector2 bottomRight, float lineThickness, ColorRgba color) => DrawLines(new Rect(topLeft, bottomRight),lineThickness,color);

    public static void DrawRectLines(Vector2 topLeft, Vector2 bottomRight, float lineThickness, ColorRgba color, float sideLengthFactor,
        LineCapType capType = LineCapType.Extended, int capPoints = 0)
    {
        var a = topLeft;
        var b = new Vector2(topLeft.X, bottomRight.Y);
        var c = bottomRight;
        var d = new Vector2(bottomRight.X, topLeft.Y);
        
        var side1 = b - a;
        var end1 = a + side1 * sideLengthFactor;
        
        var side2 = c - b;
        var end2 = b + side2 * sideLengthFactor;
        
        var side3 = d - c;
        var end3 = c + side3 * sideLengthFactor;
        
        var side4 = a - d;
        var end4 = d + side4 * sideLengthFactor;
        
        ShapeSegmentDrawing.DrawSegment(a, end1, lineThickness, color, capType, capPoints);
        ShapeSegmentDrawing.DrawSegment(b, end2, lineThickness, color, capType, capPoints);
        ShapeSegmentDrawing.DrawSegment(c, end3, lineThickness, color, capType, capPoints);
        ShapeSegmentDrawing.DrawSegment(d, end4, lineThickness, color, capType, capPoints);
    }
    
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
        ShapeQuadDrawing.DrawQuadLines(a,b,c,d, lineThickness, color, capType, capPoints);
    }
    public static void DrawRectLines(Vector2 topLeft, Vector2 bottomRight, Vector2 pivot, float rotDeg, LineDrawingInfo lineInfo)
        => DrawRectLines(topLeft, bottomRight, pivot, rotDeg, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
    
    
    public static void Draw(this Rect rect, ColorRgba color) => Raylib.DrawRectangleRec(rect.Rectangle, color.ToRayColor());
    public static void Draw(this Rect rect, Vector2 pivot, float rotDeg, ColorRgba color) => DrawRect(rect.TopLeft, rect.BottomRight, pivot, rotDeg, color);
    
    
    public static void DrawLines(this Rect rect, float lineThickness, ColorRgba color) => Raylib.DrawRectangleLinesEx(rect.Rectangle, lineThickness * 2, color.ToRayColor());
    public static void DrawLines(this Rect rect, LineDrawingInfo lineInfo)
    {
        ShapeSegmentDrawing.DrawSegment(rect.TopLeft, rect.BottomLeft, lineInfo);
        ShapeSegmentDrawing.DrawSegment(rect.BottomLeft, rect.BottomRight, lineInfo);
        ShapeSegmentDrawing.DrawSegment(rect.BottomRight, rect.TopRight, lineInfo);
        ShapeSegmentDrawing.DrawSegment(rect.TopRight, rect.TopLeft, lineInfo);
    }
    public static void DrawLines(this Rect rect, Vector2 pivot, float rotDeg, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.Extended, int capPoints = 0)
    {
        DrawRectLines(rect.TopLeft, rect.BottomRight, pivot, rotDeg, lineThickness, color, capType, capPoints);
    }
    public static void DrawLines(this Rect rect, float lineThickness, ColorRgba color, float sideLengthFactor, LineCapType capType = LineCapType.Extended, int capPoints = 0)
    {
        DrawRectLines(rect.TopLeft, rect.BottomRight, lineThickness, color, sideLengthFactor, capType, capPoints);
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
        
        ShapeQuadDrawing.DrawQuadLinesPercentage(a,b,c,d, f, lineThickness, color, capType, capPoints);
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
            ShapeSegmentDrawing.DrawSegment(r.TopLeft, r.BottomLeft, lineInfo, sideScaleFactor, sideScaleOrigin);
            ShapeSegmentDrawing.DrawSegment(r.BottomLeft, r.BottomRight, lineInfo, sideScaleFactor, sideScaleOrigin);
            ShapeSegmentDrawing.DrawSegment(r.BottomRight, r.TopRight, lineInfo, sideScaleFactor, sideScaleOrigin);
            ShapeSegmentDrawing.DrawSegment(r.TopRight, r.TopLeft, lineInfo, sideScaleFactor, sideScaleOrigin);
        }
        else
        {
            var corners = r.RotateCorners(pivot, rotDeg);
            ShapeSegmentDrawing.DrawSegment(corners.tl, corners.bl, lineInfo, sideScaleFactor, sideScaleOrigin);
            ShapeSegmentDrawing.DrawSegment(corners.bl, corners.br, lineInfo, sideScaleFactor, sideScaleOrigin);
            ShapeSegmentDrawing.DrawSegment(corners.br, corners.tr, lineInfo, sideScaleFactor, sideScaleOrigin);
            ShapeSegmentDrawing.DrawSegment(corners.tr, corners.tl, lineInfo, sideScaleFactor, sideScaleOrigin);
        }
    }
    
    
    public static void DrawVertices(this Rect rect, float vertexRadius, ColorRgba color, int circleSegments = 8)
    {
        ShapeCircleDrawing.DrawCircle(rect.TopLeft, vertexRadius, color    , circleSegments);
        ShapeCircleDrawing.DrawCircle(rect.TopRight, vertexRadius, color   , circleSegments);
        ShapeCircleDrawing.DrawCircle(rect.BottomLeft, vertexRadius, color , circleSegments);
        ShapeCircleDrawing.DrawCircle(rect.BottomRight, vertexRadius, color, circleSegments);
    }
    
    
    public static void DrawRounded(this Rect rect, float roundness, int segments, ColorRgba color) => Raylib.DrawRectangleRounded(rect.Rectangle, roundness, segments, color.ToRayColor());
    public static void DrawRoundedLines(this Rect rect, float roundness, float lineThickness, int segments, ColorRgba color) 
        => Raylib.DrawRectangleRoundedLinesEx(rect.Rectangle, roundness, segments, lineThickness * 2, color.ToRayColor());
    
    
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
            ShapeSegmentDrawing.DrawSegment(tl, tl + new Vector2(MathF.Min(tlCorner, rect.Width), 0f), lineInfo);
            ShapeSegmentDrawing.DrawSegment(tl, tl + new Vector2(0f, MathF.Min(tlCorner, rect.Height)), lineInfo);
        }
        if (trCorner > 0f)
        {
            //DrawCircle(tr, lineThickness / 2, color);
            ShapeSegmentDrawing.DrawSegment(tr, tr - new Vector2(MathF.Min(trCorner, rect.Width), 0f), lineInfo);
            ShapeSegmentDrawing.DrawSegment(tr, tr + new Vector2(0f, MathF.Min(trCorner, rect.Height)), lineInfo);
        }
        if (brCorner > 0f)
        {
            //DrawCircle(br, lineThickness / 2, color);
            ShapeSegmentDrawing.DrawSegment(br, br - new Vector2(MathF.Min(brCorner, rect.Width), 0f), lineInfo);
            ShapeSegmentDrawing.DrawSegment(br, br - new Vector2(0f, MathF.Min(brCorner, rect.Height)), lineInfo);
        }
        if (blCorner > 0f)
        {
            //DrawCircle(bl, lineThickness / 2, color);
            ShapeSegmentDrawing.DrawSegment(bl, bl + new Vector2(MathF.Min(blCorner, rect.Width), 0f), lineInfo);
            ShapeSegmentDrawing.DrawSegment(bl, bl - new Vector2(0f, MathF.Min(blCorner, rect.Height)), lineInfo);
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
            ShapeSegmentDrawing.DrawSegment(tl, tl + new Vector2(tlCorner * rect.Width, 0f), lineInfo);
            ShapeSegmentDrawing.DrawSegment(tl, tl + new Vector2(0f, tlCorner * rect.Height), lineInfo);
        }
        if (trCorner > 0f && trCorner < 1f)
        {
            // DrawCircle(tr, lineThickness / 2, color);
            ShapeSegmentDrawing.DrawSegment(tr, tr - new Vector2(tlCorner * rect.Width, 0f), lineInfo);
            ShapeSegmentDrawing.DrawSegment(tr, tr + new Vector2(0f, tlCorner * rect.Height), lineInfo);
        }
        if (brCorner > 0f && brCorner < 1f)
        {
            // DrawCircle(br, lineThickness / 2, color);
            ShapeSegmentDrawing.DrawSegment(br, br - new Vector2(tlCorner * rect.Width, 0f), lineInfo);
            ShapeSegmentDrawing.DrawSegment(br, br - new Vector2(0f, tlCorner * rect.Height), lineInfo);
        }
        if (blCorner > 0f && blCorner < 1f)
        {
            // DrawCircle(bl, lineThickness / 2, color);
            ShapeSegmentDrawing.DrawSegment(bl, bl + new Vector2(tlCorner * rect.Width, 0f), lineInfo);
            ShapeSegmentDrawing.DrawSegment(bl, bl - new Vector2(0f, tlCorner * rect.Height), lineInfo);
        }
    }
    public static void DrawCornersRelative(this Rect rect, LineDrawingInfo lineInfo, float cornerLengthFactor) 
        => DrawCornersRelative(rect, lineInfo, cornerLengthFactor, cornerLengthFactor, cornerLengthFactor, cornerLengthFactor);

    private static void DrawRectLinesPercentageHelper(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, float perimeterToDraw, float size1, float size2, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        // Draw first segment
        var curP = p1;
        var nextP = p2;
        if (perimeterToDraw < size1)
        {
            float p = perimeterToDraw / size1;
            nextP = curP.Lerp(nextP, p);
            ShapeSegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, capType, capPoints);
            return;
        }
                
        ShapeSegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, capType, capPoints);
        perimeterToDraw -= size1;
                
        // Draw second segment
        curP = nextP;
        nextP = p3;
        if (perimeterToDraw < size2)
        {
            float p = perimeterToDraw / size2;
            nextP = curP.Lerp(nextP, p);
            ShapeSegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, capType, capPoints);
            return;
        }
                
        ShapeSegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, capType, capPoints);
        perimeterToDraw -= size2;
                
        // Draw third segment
        curP = nextP;
        nextP = p4;
        if (perimeterToDraw < size1)
        {
            float p = perimeterToDraw / size1;
            nextP = curP.Lerp(nextP, p);
            ShapeSegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, capType, capPoints);
            return;
        }
        
        ShapeSegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, capType, capPoints);
        perimeterToDraw -= size1;
               
        // Draw fourth segment
        curP = nextP;
        nextP = p1;
        if (perimeterToDraw < size2)
        {
            float p = perimeterToDraw / size2;
            nextP = curP.Lerp(nextP, p);
        }
        ShapeSegmentDrawing.DrawSegment(curP, nextP, lineThickness, color, capType, capPoints);
    }

}