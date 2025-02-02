using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Lib.Drawing;

public static class PolygonDrawing
{
    
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
            TriangleDrawing.DrawTriangle(poly[0], poly[1], poly[2], color);
            return;
        }
        poly.Triangulate().Draw(color);
    }
    public static void DEBUG_DrawLinesCCW(this Polygon poly, float lineThickness, ColorRgba startColorRgba, ColorRgba endColorRgba)
    {
        if (poly.Count < 3) return;

        DrawLines(poly, lineThickness, startColorRgba, endColorRgba);
        CircleDrawing.DrawCircle(poly[0], lineThickness * 2f, startColorRgba);
        CircleDrawing.DrawCircle(poly[poly.Count - 1], lineThickness * 2f, endColorRgba);
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
            SegmentDrawing.DrawSegment(start, end, lineThickness, finalColorRgba, capType, capPoints);
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
            SegmentDrawing.DrawSegment(start, end, lineThickness, color, capType, capPoints);
        }
    }
    public static void DrawLines(this Polygon poly, float lineThickness, ColorRgba color, float sideLengthFactor,  LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (poly.Count < 3) return;
        
        for (var i = 0; i < poly.Count; i++)
        {
            var start = poly[i];
            var end = poly[(i + 1) % poly.Count];
            SegmentDrawing.DrawSegment(start, end, lineThickness, color, sideLengthFactor,  capType, capPoints);
        }
    }
    public static void DrawLines(this Polygon relative, Vector2 pos, float size, float rotDeg, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        if (relative.Count < 3) return;
        
        for (var i = 0; i < relative.Count; i++)
        {
            var start = pos + (relative[i] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
            var end = pos + (relative[(i + 1) % relative.Count] * size).Rotate(rotDeg * ShapeMath.DEGTORAD);
            SegmentDrawing.DrawSegment(start, end, lineThickness, color, capType, capPoints);
        }
    }
    public static void DrawLines(this Polygon relative, Transform2D transform, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        DrawLines(relative, transform.Position, transform.ScaledSize.Length, transform.RotationDeg, lineThickness, color, capType, capPoints);
    }

    public static void DrawLines(this Polygon poly, LineDrawingInfo lineInfo) => DrawLines(poly, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);

    public static void DrawLines(this Polygon relative, Vector2 pos, float size, float rotDeg, LineDrawingInfo lineInfo) => DrawLines(relative, pos, size, rotDeg, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
    public static void DrawLines(this Polygon relative, Transform2D transform, LineDrawingInfo lineInfo) => DrawLines(relative, transform.Position, transform.ScaledSize.Length, transform.RotationDeg, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);

    
    /// <summary>
    /// Draws a certain amount of perimeter
    /// </summary>
    /// <param name="poly"></param>
    /// <param name="perimeterToDraw">Determines how much of the outline is drawn.
    /// If perimeter is negative outline will be drawn in cw direction.</param>
    /// <param name="startIndex">Determines at which corner drawing starts.</param>
    /// <param name="lineThickness"></param>
    /// <param name="color"></param>
    /// <param name="capType"></param>
    /// <param name="capPoints"></param>
    public static void DrawLinesPerimeter(this Polygon poly, float perimeterToDraw, int startIndex, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        ShapeDrawing.DrawOutlinePerimeter(poly, perimeterToDraw, startIndex, lineThickness, color, capType, capPoints);
    }
    /// <summary>
    /// Draws a certain percentage of an outline.
    /// </summary>
    /// <param name="poly"></param>
    /// <param name="f">The percentage of the outline to draw. Negative value reverses the direction (cw).
    /// Integer part can be used to change starting corner.
    /// 0.35 would start at corner a go in ccw direction and draw 35% of the outline.
    /// -2.7 would start at b (the third corner in cw direction) and draw in cw direction 70% of the outline.</param>
    /// <param name="lineThickness"></param>
    /// <param name="color"></param>
    /// <param name="capType"></param>
    /// <param name="capPoints"></param>
    public static void DrawLinesPercentage(this Polygon poly, float f, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        ShapeDrawing.DrawOutlinePercentage(poly, f, lineThickness, color, capType, capPoints);
    }
    /// <summary>
    /// Draws a certain percentage of an outline.
    /// </summary>
    /// <param name="poly"></param>
    /// <param name="f">The percentage of the outline to draw. Negative value reverses the direction (cw).
    /// Integer part can be used to change starting corner.
    /// 0.35 would start at corner a go in ccw direction and draw 35% of the outline.
    /// -2.7 would start at b (the third corner in cw direction) and draw in cw direction 70% of the outline.</param>
    /// <param name="lineInfo"></param>
    public static void DrawLinesPercentage(this Polygon poly, float f, LineDrawingInfo lineInfo)
    {
        ShapeDrawing.DrawOutlinePercentage(poly, f, lineInfo.Thickness, lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
    }


    public static void DrawVertices(this Polygon poly, float vertexRadius, ColorRgba color, int circleSegments)
    {
        foreach (var p in poly)
        {
            CircleDrawing.DrawCircle(p, vertexRadius, color, circleSegments);
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
            SegmentDrawing.DrawSegment(cur, cur + next.Normalize() * cornerLength, lineThickness, color, capType, capPoints);
            SegmentDrawing.DrawSegment(cur, cur + prev.Normalize() * cornerLength, lineThickness, color, capType, capPoints);
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
            SegmentDrawing.DrawSegment(cur, cur.Lerp(next, cornerF), lineThickness, color, capType, capPoints);
            SegmentDrawing.DrawSegment(cur, cur.Lerp(prev, cornerF), lineThickness, color, capType, capPoints);
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
            SegmentDrawing.DrawSegment(cur, cur + next.Normalize() * cornerLength, lineInfo);
            SegmentDrawing.DrawSegment(cur, cur + prev.Normalize() * cornerLength, lineInfo);
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
            SegmentDrawing.DrawSegment(cur, cur.Lerp(next, cornerF), lineInfo);
            SegmentDrawing.DrawSegment(cur, cur.Lerp(prev, cornerF), lineInfo);
        }
    }

    public static void DrawCornered(this Polygon poly, float lineThickness, ColorRgba color, float cornerLength, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        for (var i = 0; i < poly.Count; i++)
        {
            var prev = poly[(i-1)%poly.Count];
            var cur = poly[i];
            var next = poly[(i+1)%poly.Count];
            SegmentDrawing.DrawSegment(cur, cur + next.Normalize() * cornerLength, lineThickness, color, capType, capPoints);
            SegmentDrawing.DrawSegment(cur, cur + prev.Normalize() * cornerLength, lineThickness, color, capType, capPoints);
        }
    }
    public static void DrawCorneredRelative(this Polygon poly, float lineThickness, ColorRgba color, float cornerF, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        for (var i = 0; i < poly.Count; i++)
        {
            var prev = poly[(i - 1) % poly.Count];
            var cur = poly[i];
            var next = poly[(i + 1) % poly.Count];
            SegmentDrawing.DrawSegment(cur, cur.Lerp(next, cornerF), lineThickness, color, capType, capPoints);
            SegmentDrawing.DrawSegment(cur, cur.Lerp(prev, cornerF), lineThickness, color, capType, capPoints);
        }
    }
    public static void DrawCornered(this Polygon poly, float cornerLength, LineDrawingInfo lineInfo)
    {
        for (var i = 0; i < poly.Count; i++)
        {
            var prev = poly[(i-1)%poly.Count];
            var cur = poly[i];
            var next = poly[(i+1)%poly.Count];
            SegmentDrawing.DrawSegment(cur, cur + next.Normalize() * cornerLength, lineInfo);
            SegmentDrawing.DrawSegment(cur, cur + prev.Normalize() * cornerLength, lineInfo);
        }
    }
    public static void DrawCorneredRelative(this Polygon poly, float cornerF, LineDrawingInfo lineInfo)
    {
        for (var i = 0; i < poly.Count; i++)
        {
            var prev = poly[(i - 1) % poly.Count];
            var cur = poly[i];
            var next = poly[(i + 1) % poly.Count];
            SegmentDrawing.DrawSegment(cur, cur.Lerp(next, cornerF), lineInfo);
            SegmentDrawing.DrawSegment(cur, cur.Lerp(prev, cornerF), lineInfo);
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
            SegmentDrawing.DrawSegment(start, end, lineInfo, sideScaleFactor, sideScaleOrigin);
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
            SegmentDrawing.DrawSegment(start, end, lineInfo, sideScaleFactor, sideScaleOrigin);
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

}