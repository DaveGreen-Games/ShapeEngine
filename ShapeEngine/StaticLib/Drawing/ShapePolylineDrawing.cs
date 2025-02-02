using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.StaticLib.Drawing;

public static class ShapePolylineDrawing
{
    
    public static void Draw(this Polyline polyline, float thickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (polyline.Count < 2) return;
        for (var i = 0; i < polyline.Count - 1; i++)
        {
            var start = polyline[i];
            var end = polyline[i + 1];
            ShapeSegmentDrawing.DrawSegment(start, end, thickness, color, capType, capPoints);
        }
    }
    //not a closed shape therefore it does not get sideLengthFactor overload -> might change in the future...
    // public static void Draw(this Polyline polyline, float thickness, ColorRgba color, float sideLengthFactor,  LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    // {
    //     if (polyline.Count < 2) return;
    //     for (var i = 0; i < polyline.Count - 1; i++)
    //     {
    //         var start = polyline[i];
    //         var end = polyline[i + 1];
    //         SegmentDrawing.DrawSegment(start, end, thickness, color, sideLengthFactor, capType, capPoints);
    //     }
    // }
    public static void Draw(this Polyline polyline, LineDrawingInfo lineInfo)
    {
        if (polyline.Count < 2) return;
        for (var i = 0; i < polyline.Count - 1; i++)
        {
            var start = polyline[i];
            var end = polyline[i + 1];
            ShapeSegmentDrawing.DrawSegment(start, end, lineInfo);
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
            ShapeSegmentDrawing.DrawSegment(start, end, thickness, c, capType, capPoints);
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
            ShapeSegmentDrawing.DrawSegment(start, end, lineThickness, color, capType, capPoints);
        }
    }
    public static void Draw(this Polyline relative, Transform2D transform, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        Draw(relative, transform.Position, transform.ScaledSize.Length, transform.RotationDeg, lineThickness, color, capType, capPoints);
    }
    public static void Draw(this Polyline relative, Vector2 pos, float size, float rotDeg, LineDrawingInfo lineInfo) => Draw(relative, pos, size, rotDeg, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
    public static void Draw(this Polyline relative, Transform2D transform, LineDrawingInfo lineInfo) => Draw(relative, transform.Position, transform.ScaledSize.Length, transform.RotationDeg, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);

    
    public static void DrawPerimeter(this Polyline polyline, float perimeterToDraw, LineDrawingInfo lineInfo)
    {
        DrawPerimeter(polyline, perimeterToDraw, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
    }
    public static void DrawPercentage(this Polyline polyline, float f, LineDrawingInfo lineInfo)
    {
        DrawPercentage(polyline, f, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
    }

    /// <summary>
    /// Draws a certain amount of perimeter
    /// </summary>
    /// <param name="polyline"></param>
    /// <param name="perimeterToDraw">Determines how much of the outline is drawn.
    /// If perimeter is negative outline will be drawn in cw direction.</param>
    /// <param name="lineThickness"></param>
    /// <param name="color"></param>
    /// <param name="capType"></param>
    /// <param name="capPoints"></param>
    public static void DrawPerimeter(this Polyline polyline, float perimeterToDraw, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (polyline.Count < 3 || perimeterToDraw == 0) return;

        bool reverse = perimeterToDraw < 0;
        if (reverse) perimeterToDraw *= -1;

        int currentIndex = reverse ? polyline.Count - 1 : 0;
        
        for (var i = 0; i < polyline.Count - 1; i++)
        {
            var start = polyline[currentIndex];
            currentIndex = reverse ? currentIndex - 1 : currentIndex + 1;
            var end = polyline[currentIndex];
            var l = (end - start).Length();
            if (l < perimeterToDraw)
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
    /// <param name="polyline"></param>
    /// <param name="f">The percentage of the outline to draw. Negative value reverses the direction (cw).
    /// Integer part can be used to change starting corner.
    /// 0.35 would start at corner a go in ccw direction and draw 35% of the outline.
    /// -2.7 would start at b (the third corner in cw direction) and draw in cw direction 70% of the outline.</param>
    /// <param name="lineThickness"></param>
    /// <param name="color"></param>
    /// <param name="capType"></param>
    /// <param name="capPoints"></param>
    public static void DrawPercentage(this Polyline polyline, float f, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (polyline.Count < 3 || f == 0f) return;

        bool negative = false;
        if (f < 0)
        {
            negative = true;
            f *= -1;
        }
        if (f >= 1)
        {
            Draw(polyline, lineThickness, color, capType, capPoints);
            return;
        }
        
        float perimeter = 0f;
        for (var i = 0; i < polyline.Count - 1; i++)
        {
            var start = polyline[i];
            var end = polyline[i + 1];
            var l = (end - start).Length();
            perimeter += l;
        }

        f = ShapeMath.Clamp(f, 0f, 1f);
        DrawPerimeter(polyline, perimeter * f * (negative ? -1 : 1), lineThickness, color, capType, capPoints);
    }
    
    
    
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
            ShapeSegmentDrawing.DrawSegment(start, end, lineInfo, sideScaleFactor, sideScaleOrigin);
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
            ShapeSegmentDrawing.DrawSegment(start, end, lineInfo, sideScaleFactor, sideScaleOrigin);
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
            ShapeCircleDrawing.DrawCircle(p, vertexRadius, color, circleSegments);
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
            ShapeSegmentDrawing.DrawSegmentGlow(start, end, width, endWidth, color, endColorRgba, steps, capType, capPoints);
        }
        // polyline.GetEdges().DrawGlow(width, endWidth, color, endColor, steps);
    }

}