
using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib.Drawing;
using ShapeEngine.Random;
using Ray = ShapeEngine.Core.Shapes.Ray;

namespace ShapeEngine.StaticLib.Drawing;

public static class ShapeDrawing
{

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
            ShapeSegmentDrawing.DrawSegment(start, end, lineThickness, finalColorRgba, capType, capPoints);
        }
    }
    public static void DrawOutline(this List<Vector2> shapePoints, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (shapePoints.Count < 3) return;
        
        for (var i = 0; i < shapePoints.Count; i++)
        {
            var start = shapePoints[i];
            var end = shapePoints[(i + 1) % shapePoints.Count];
            ShapeSegmentDrawing.DrawSegment(start, end, lineThickness, color, capType, capPoints);
        }
    }
    public static void DrawOutline(this List<Vector2> shapePoints, float lineThickness, ColorRgba color, float sideLengthFactor,  LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (shapePoints.Count < 3) return;
        
        for (var i = 0; i < shapePoints.Count; i++)
        {
            var start = shapePoints[i];
            var end = shapePoints[(i + 1) % shapePoints.Count];
            ShapeSegmentDrawing.DrawSegment(start, end, lineThickness, color, sideLengthFactor,  capType, capPoints);
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
            ShapeSegmentDrawing.DrawSegment(start, end, lineThickness, color, capType, capPoints);
        }
    }
    public static void DrawOutline(this List<Vector2> relativePoints, Transform2D transform, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        DrawOutline(relativePoints, transform.Position, transform.ScaledSize.Length, transform.RotationDeg, lineThickness, color, capType, capPoints);
    }
    public static void DrawOutline(this List<Vector2> relativePoints, Vector2 pos, float size, float rotDeg, LineDrawingInfo lineInfo) => DrawOutline(relativePoints, pos, size, rotDeg, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
    public static void DrawOutline(this List<Vector2> relativePoints, Transform2D transform, LineDrawingInfo lineInfo) => DrawOutline(relativePoints, transform.Position, transform.ScaledSize.Length, transform.RotationDeg, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);

    /// <summary>
    /// Draws a certain amount of perimeter
    /// </summary>
    /// <param name="shapePoints"></param>
    /// <param name="perimeterToDraw">Determines how much of the outline is drawn.
    /// If perimeter is negative outline will be drawn in cw direction.</param>
    /// <param name="startIndex">Determines at which corner drawing starts.</param>
    /// <param name="lineThickness"></param>
    /// <param name="color"></param>
    /// <param name="capType"></param>
    /// <param name="capPoints"></param>
    public static void DrawOutlinePerimeter(this List<Vector2> shapePoints, float perimeterToDraw, int startIndex, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (shapePoints.Count < 3 || perimeterToDraw == 0) return;

        int currentIndex = ShapeMath.Clamp(startIndex, 0, shapePoints.Count - 1);

        bool reverse = perimeterToDraw < 0;
        if (reverse) perimeterToDraw *= -1;
        
        for (var i = 0; i < shapePoints.Count; i++)
        {
            var start = shapePoints[currentIndex];
            if (reverse) currentIndex = ShapeMath.WrapIndex(shapePoints.Count, currentIndex - 1); // (currentIndex - 1) % shapePoints.Count;
            else currentIndex = (currentIndex + 1) % shapePoints.Count;
            var end = shapePoints[currentIndex];
            var l = (end - start).Length();
            if (l <= perimeterToDraw)
            {
                perimeterToDraw -= l;
                ShapeSegmentDrawing.DrawSegment(start, end, lineThickness, color, capType, capPoints);
            }
            else
            {
                float f = perimeterToDraw / l;
                end = start.Lerp(end, f);
                ShapeSegmentDrawing.DrawSegment(start, end, lineThickness, color, capType, capPoints);
                return;
            }
                
        }
        
        
    }
    /// <summary>
    /// Draws a certain percentage of an outline.
    /// </summary>
    /// <param name="shapePoints"></param>
    /// <param name="f">The percentage of the outline to draw. Negative value reverses the direction (cw).
    /// Integer part can be used to change starting corner.
    /// 0.35 would start at corner a go in ccw direction and draw 35% of the outline.
    /// -2.7 would start at b (the third corner in cw direction) and draw in cw direction 70% of the outline.</param>
    /// <param name="lineThickness"></param>
    /// <param name="color"></param>
    /// <param name="capType"></param>
    /// <param name="capPoints"></param>
    public static void DrawOutlinePercentage(this List<Vector2> shapePoints, float f, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (shapePoints.Count < 3 || f == 0f) return;

        bool negative = false;
        if (f < 0)
        {
            negative = true;
            f *= -1;
        }
        int startIndex = (int)f;
        float percentage = f - startIndex;
        if (percentage <= 0)
        {
            return;
        }
        if (percentage >= 1)
        {
            DrawOutline(shapePoints, lineThickness, color, capType, capPoints);
            return;
        }

        float perimeter = 0f;
        for (var i = 0; i < shapePoints.Count; i++)
        {
            var start = shapePoints[i];
            var end = shapePoints[(i + 1) % shapePoints.Count];
            var l = (end - start).Length();
            perimeter += l;
        }
        
        DrawOutlinePerimeter(shapePoints, perimeter * f * (negative ? -1 : 1), startIndex, lineThickness, color, capType, capPoints);
    }
    

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
            ShapeSegmentDrawing.DrawSegment(start, end, lineInfo, sideScaleFactor, sideScaleOrigin);
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
            ShapeSegmentDrawing.DrawSegment(start, end, lineInfo, sideScaleFactor, sideScaleOrigin);
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
            ShapeSegmentDrawing.DrawSegment(cur, cur + next.Normalize() * cornerLength, lineThickness, color, capType, capPoints);
            ShapeSegmentDrawing.DrawSegment(cur, cur + prev.Normalize() * cornerLength, lineThickness, color, capType, capPoints);
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
            ShapeSegmentDrawing.DrawSegment(cur, cur.Lerp(next, cornerF), lineThickness, color, capType, capPoints);
            ShapeSegmentDrawing.DrawSegment(cur, cur.Lerp(prev, cornerF), lineThickness, color, capType, capPoints);
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
            ShapeSegmentDrawing.DrawSegment(cur, cur + next.Normalize() * cornerLength, lineInfo);
            ShapeSegmentDrawing.DrawSegment(cur, cur + prev.Normalize() * cornerLength, lineInfo);
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
            ShapeSegmentDrawing.DrawSegment(cur, cur.Lerp(next, cornerF), lineInfo);
            ShapeSegmentDrawing.DrawSegment(cur, cur.Lerp(prev, cornerF), lineInfo);
        }
    }

    public static void DrawOutlineCornered(this List<Vector2> points, float lineThickness, ColorRgba color, float cornerLength, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        for (var i = 0; i < points.Count; i++)
        {
            var prev = points[(i-1)%points.Count];
            var cur = points[i];
            var next = points[(i+1)%points.Count];
            ShapeSegmentDrawing.DrawSegment(cur, cur + next.Normalize() * cornerLength, lineThickness, color, capType, capPoints);
            ShapeSegmentDrawing.DrawSegment(cur, cur + prev.Normalize() * cornerLength, lineThickness, color, capType, capPoints);
        }
    }
    public static void DrawOutlineCorneredRelative(this List<Vector2> points, float lineThickness, ColorRgba color, float cornerF, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        for (var i = 0; i < points.Count; i++)
        {
            var prev = points[(i - 1) % points.Count];
            var cur = points[i];
            var next = points[(i + 1) % points.Count];
            ShapeSegmentDrawing.DrawSegment(cur, cur.Lerp(next, cornerF), lineThickness, color, capType, capPoints);
            ShapeSegmentDrawing.DrawSegment(cur, cur.Lerp(prev, cornerF), lineThickness, color, capType, capPoints);
        }
    }
    public static void DrawOutlineCornered(this List<Vector2> points, float cornerLength, LineDrawingInfo lineInfo)
    {
        for (var i = 0; i < points.Count; i++)
        {
            var prev = points[(i-1)%points.Count];
            var cur = points[i];
            var next = points[(i+1)%points.Count];
            ShapeSegmentDrawing.DrawSegment(cur, cur + next.Normalize() * cornerLength, lineInfo);
            ShapeSegmentDrawing.DrawSegment(cur, cur + prev.Normalize() * cornerLength, lineInfo);
        }
    }
    public static void DrawOutlineCorneredRelative(this List<Vector2> points, float cornerF, LineDrawingInfo lineInfo)
    {
        for (var i = 0; i < points.Count; i++)
        {
            var prev = points[(i - 1) % points.Count];
            var cur = points[i];
            var next = points[(i + 1) % points.Count];
            ShapeSegmentDrawing.DrawSegment(cur, cur.Lerp(next, cornerF), lineInfo);
            ShapeSegmentDrawing.DrawSegment(cur, cur.Lerp(prev, cornerF), lineInfo);
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
            ShapeCircleDrawing.DrawCircle(p, vertexRadius, color, circleSegments);
        }
    }

}

