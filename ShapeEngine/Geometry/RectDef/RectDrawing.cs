
using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.RectDef;
/// <summary>
/// Provides static extension methods for drawing rectangles and grids, including advanced features such as nine-patch, rounded corners, slanted corners, partial outlines, and more.
/// </summary>
/// <remarks>
/// All methods are designed for use with Raylib and ShapeEngine types.
/// Methods are implemented as extensions for convenient usage.
/// </remarks>
public static class RectDrawing
{
    #region Draw
    
    /// <summary>
    /// Draws a filled rectangle using the specified top-left and bottom-right coordinates and color.
    /// </summary>
    /// <param name="topLeft">The top-left corner of the rectangle.</param>
    /// <param name="bottomRight">The bottom-right corner of the rectangle.</param>
    /// <param name="color">The fill color.</param>
    public static void DrawRect(Vector2 topLeft, Vector2 bottomRight, ColorRgba color)
    {
        Raylib.DrawRectangleV(topLeft, bottomRight - topLeft, color.ToRayColor());
    }
    
    //TODO: Docs
    public static void Draw(this Rect rect, ColorRgba color)
    {
        Raylib.DrawRectangleV(rect.TopLeft, rect.BottomRight - rect.TopLeft, color.ToRayColor());
    }
    #endregion

    #region Draw Rounded
    //TODO: Docs
    public static void DrawRounded(this Rect rect, ColorRgba color, float roundness, int segments)
    {
        Raylib.DrawRectangleRounded(rect.Rectangle, roundness, segments, color);
    }

    #endregion
    
    #region Draw Lines
    //TODO: Docs
    //TODO: Check vs quad draw lines (if it looks the same) and how performance is 
    public static void DrawLines(this Rect rect, float lineThickness, ColorRgba color)
    {
        Raylib.DrawRectangleLinesEx(rect.Rectangle, lineThickness, color);
    }
    
    //TODO: Docs
    //TODO: Check vs quad draw lines (if it looks the same) and how performance is 
    public static void DrawLines(this Rect rect, LineDrawingInfo lineInfo)
    {
        Raylib.DrawRectangleLinesEx(rect.Rectangle, lineInfo.Thickness, lineInfo.Color);
    }
    #endregion
    
    #region Draw Rounded Lines
    //TODO: Docs
    public static void DrawLines(this Rect rect, float lineThickness, ColorRgba color, float roundness, int segments)
    {
        Raylib.DrawRectangleRoundedLinesEx(rect.Rectangle, roundness, segments, lineThickness, color);
    }
    #endregion
    
    #region Draw Lines Scaled
  
    /// <summary>
    /// Draws a rectangle outline where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="r">The rectangle to draw.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, etc.).</param>
    /// <param name="sideScaleFactor">
    /// <para>The scale factor for each side.</para>
    /// <list type="bullet">
    /// <item><description>0: No Rect is drawn.</description></item>
    /// <item><description>1: The normal Rect is drawn.</description></item>
    /// <item><description>0.5: Each side is half as long.</description></item>
    /// </list>
    /// </param>
    /// <param name="sideScaleOrigin">
    /// <para>The point along each side to scale from, in both directions (0 to 1).</para>
    /// <list type="bullet">
    /// <item><description>0: Start of Side</description></item>
    /// <item><description>0.5: Center of Side</description></item>
    /// <item><description>1: End of Side</description></item>
    /// </list>
    /// </param>
    /// <remarks>
    /// Useful for creating stylized or animated rectangles.
    /// </remarks>
    public static void DrawLinesScaled(this Rect r, LineDrawingInfo lineInfo,  float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0f) return;
        if (sideScaleFactor >= 1f)
        {
            r.DrawLines(lineInfo);
            return;
        }
        SegmentDrawing.DrawSegment(r.TopLeft, r.BottomLeft, lineInfo, sideScaleFactor, sideScaleOrigin);
        SegmentDrawing.DrawSegment(r.BottomLeft, r.BottomRight, lineInfo, sideScaleFactor, sideScaleOrigin);
        SegmentDrawing.DrawSegment(r.BottomRight, r.TopRight, lineInfo, sideScaleFactor, sideScaleOrigin);
        SegmentDrawing.DrawSegment(r.TopRight, r.TopLeft, lineInfo, sideScaleFactor, sideScaleOrigin);
    }

    #endregion
    
    #region Draw Lines Percentage
    //TODO: Docs
    public static void DrawLinesPercentage(this Rect rect, float f, int startIndex, float lineThickness, ColorRgba color)
    {
        var quad = new Quad(rect);
        quad.DrawLinesPercentage(f, startIndex, new LineDrawingInfo(lineThickness, color));
    }
    //TODO: Docs
    public static void DrawLinesPercentage(this Rect rect, float f, int startIndex, LineDrawingInfo lineInfo)
    {
        var quad = new Quad(rect);
        quad.DrawLinesPercentage(f, startIndex, lineInfo);
    }


    #endregion
    
    #region Draw Corners
    /// <summary>
    /// Draws the corners of the rectangle with independent lengths for each corner.
    /// </summary>
    /// <param name="rect">The rectangle to draw corners for.</param>
    /// <param name="lineThickness">The thickness of the corner lines.</param>
    /// <param name="color">The color of the corner lines.</param>
    /// <param name="tlCorner">The length of the top-left corner.</param>
    /// <param name="trCorner">The length of the top-right corner.</param>
    /// <param name="brCorner">The length of the bottom-right corner.</param>
    /// <param name="blCorner">The length of the bottom-left corner.</param>
    public static void DrawCorners(this Rect rect, float lineThickness, ColorRgba color, float tlCorner, float trCorner, float brCorner, float blCorner)
    {
        var quad = rect.ToQuad();
        quad.DrawCorners(lineThickness, color, tlCorner, trCorner, brCorner, blCorner);
    }
    
    /// <summary>
    /// Draws all corners of the rectangle with the same length.
    /// </summary>
    /// <param name="rect">The rectangle to draw corners for.</param>
    /// <param name="lineThickness">The thickness of the corner lines.</param>
    /// <param name="color">The color of the corner lines.</param>
    /// <param name="cornerLength">The length of the corner segments.</param>
    public static void DrawCorners(this Rect rect, float lineThickness, ColorRgba color, float cornerLength)
    {
        rect.DrawCorners(lineThickness, color, cornerLength, cornerLength, cornerLength, cornerLength);
    }
    
    #endregion
    
    #region Draw Corners Relative
    /// <summary>
    /// Draws the corners of the rectangle with independent lengths relative to the rectangle's minimum dimension.
    /// </summary>
    /// <param name="rect">The rectangle to draw corners for.</param>
    /// <param name="lineThickness">The thickness of the corner lines.</param>
    /// <param name="color">The color of the corner lines.</param>
    /// <param name="tlCornerFactor">Factor (0-1) for the top-left corner length relative to the rectangle's minimum size.</param>
    /// <param name="trCornerFactor">Factor (0-1) for the top-right corner length relative to the rectangle's minimum size.</param>
    /// <param name="brCornerFactor">Factor (0-1) for the bottom-right corner length relative to the rectangle's minimum size.</param>
    /// <param name="blCornerFactor">Factor (0-1) for the bottom-left corner length relative to the rectangle's minimum size.</param>
    public static void DrawCornersRelative(this Rect rect, float lineThickness, ColorRgba color, float tlCornerFactor, float trCornerFactor, float brCornerFactor, float blCornerFactor)
    {
        float minSize = MathF.Min(rect.Width, rect.Height);
        rect.DrawCorners(lineThickness, color, tlCornerFactor * minSize, trCornerFactor * minSize, brCornerFactor * minSize, blCornerFactor * minSize);
    }
    
    /// <summary>
    /// Draws all corners of the rectangle with the same length relative to the rectangle's minimum dimension.
    /// </summary>
    /// <param name="rect">The rectangle to draw corners for.</param>
    /// <param name="lineThickness">The thickness of the corner lines.</param>
    /// <param name="color">The color of the corner lines.</param>
    /// <param name="cornerLengthFactor">Factor (0-1) for the corner length relative to the rectangle's minimum size.</param>
    public static void DrawCornersRelative(this Rect rect, float lineThickness, ColorRgba color, float cornerLengthFactor)
    {
        rect.DrawCornersRelative(lineThickness, color, cornerLengthFactor, cornerLengthFactor, cornerLengthFactor, cornerLengthFactor);
    }

    #endregion
    
    
    //TODO: Clean up + Docs
    #region Draw Chamfered Corners
    
    public static void DrawChamferedCorners(this Rect rect, ColorRgba color, float cornerLength)
    {
        var q = rect.ToQuad();
        q.DrawChamferedCorners(color, cornerLength);
    }
    
    public static void DrawChamferedCorners(this Rect rect, ColorRgba color, float cornerLengthHorizontal, float cornerLengthVertical)
    {
        var q = rect.ToQuad();
        q.DrawChamferedCorners(color, cornerLengthHorizontal, cornerLengthVertical);
        // if(rect.Width <= 0 || rect.Height <= 0) return;
        // if (cornerLengthHorizontal <= 0 || cornerLengthVertical <= 0)
        // {
        //     rect.Draw(color);
        //     return;
        // }
        //
        // float halfWidth = rect.Width / 2f;
        // float halfHeight = rect.Height / 2f;
        //
        // var tl = rect.TopLeft;
        // var br = rect.BottomRight;
        //
        // if (cornerLengthHorizontal >= halfWidth && cornerLengthVertical >= halfHeight)
        // {
        //     var p1 = tl + new Vector2(halfWidth, 0f);
        //     var p2 = tl + new Vector2(0f, halfHeight);
        //     var p3 = br - new Vector2(halfWidth, 0f);
        //     var p4 = br - new Vector2(0f, halfHeight);
        //     TriangleDrawing.DrawTriangle(p1, p2, p3, color);
        //     TriangleDrawing.DrawTriangle(p1, p3, p4, color);
        //     return;
        // }
        //
        // var bl = rect.BottomLeft;
        // var tr = rect.TopRight;
        //
        //
        // if (cornerLengthHorizontal >= halfWidth)
        // {
        //     var h = new Vector2(halfWidth, 0f);
        //     var v = new Vector2(0f, cornerLengthVertical);
        //     var top = tl + h;
        //     var bottom = bl + h;
        //     var tlV = tl + v;
        //     var blV = bl - v;
        //     var brV = br - v;
        //     var trV = tr + v;
        //     
        //     TriangleDrawing.DrawTriangle(top, tlV, blV, color);
        //     TriangleDrawing.DrawTriangle(top, blV, bottom, color);
        //     
        //     TriangleDrawing.DrawTriangle(top, bottom, brV, color);
        //     TriangleDrawing.DrawTriangle(top, brV, trV, color);
        // }
        // else if (cornerLengthVertical >= halfHeight)
        // {
        //     var h = new Vector2(cornerLengthHorizontal, 0f);
        //     var v = new Vector2(0f, halfHeight);
        //     var left = tl + v;
        //     var right = tr + v;
        //     var tlH = tl + h;
        //     var blH = bl + h;
        //     var brH = br - h;
        //     var trH = tr - h;
        //     
        //     TriangleDrawing.DrawTriangle(tlH, left, blH, color);
        //     TriangleDrawing.DrawTriangle(tlH, blH, trH, color);
        //     
        //     TriangleDrawing.DrawTriangle(trH, blH, brH, color);
        //     TriangleDrawing.DrawTriangle(trH, brH, right, color);
        // }
        // else
        // {
        //     var cornerHorizontal = new Vector2(cornerLengthHorizontal, 0f);
        //     var cornerVertical = new Vector2(0f, cornerLengthVertical);
        //     var tlH = tl + cornerHorizontal;
        //     var tlV = tl + cornerVertical;
        //
        //     var blV = bl - cornerVertical;
        //     var blH = bl + cornerHorizontal;
        //
        //     var brH = br - cornerHorizontal;
        //     var brV = br - cornerVertical;
        //
        //     var trV = tr + cornerVertical;
        //     var trH = tr - cornerHorizontal;
        //
        //     //left triangles
        //     TriangleDrawing.DrawTriangle(tlV, blV, tlH, color);
        //     TriangleDrawing.DrawTriangle(tlH, blV, blH, color);
        //
        //     //center triangles
        //     TriangleDrawing.DrawTriangle(tlH, blH, trH, color);
        //     TriangleDrawing.DrawTriangle(trH, blH, brH, color);
        //
        //     //right triangles
        //     TriangleDrawing.DrawTriangle(trH, brH, trV, color);
        //     TriangleDrawing.DrawTriangle(trV, brH, brV, color);
        // }
    }
    
    public static void DrawChamferedCorners(this Rect rect, ColorRgba color, float tlCorner, float blCorner, float brCorner, float trCorner)
    {
        var q = rect.ToQuad();
        q.DrawChamferedCorners(color, tlCorner, blCorner, brCorner, trCorner);
    }
    #endregion
    
    //TODO: Clean up + Docs
    #region Draw Chamfered Corners Relative
    public static void DrawChamferedCornersRelative(this Rect rect, ColorRgba color, float cornerLengthFactor)
    {
        var q = rect.ToQuad();
        q.DrawChamferedCornersRelative(color, cornerLengthFactor);
    }
    
    public static void DrawChamferedCornersRelative(this Rect rect, ColorRgba color, float cornerLengthFactorHorizontal, float cornerLengthFactorVertical)
    {
        var q = rect.ToQuad();
        q.DrawChamferedCornersRelative(color, cornerLengthFactorHorizontal, cornerLengthFactorVertical);
    }
    
    public static void DrawChamferedCornersRelative(this Rect rect, ColorRgba color,float tlCornerFactor, float blCornerFactor, float brCornerFactor, float trCornerFactor)
    {
        var q = rect.ToQuad();
        q.DrawChamferedCornersRelative(color, tlCornerFactor, blCornerFactor, brCornerFactor, trCornerFactor);
    }
    #endregion
    
    //TODO: Clean up + Docs
    #region Draw Chamfered Corners Lines
    
    public static void DrawChamferedCornersLines(this Rect rect, float lineThickness, ColorRgba color, float cornerLength)
    {
        var q = rect.ToQuad();
        q.DrawChamferedCornersLines(lineThickness, color, cornerLength);
        // DrawChamferedCornersLines(rect, thickness, color, cornerLength, cornerLength);
    }
    
    public static void DrawChamferedCornersLines(this Rect rect, float lineThickness, ColorRgba color, float cornerLengthHorizontal, float cornerLengthVertical)
    {
        var q = rect.ToQuad();
        q.DrawChamferedCornersLines(lineThickness, color, cornerLengthHorizontal, cornerLengthVertical);
        // if(rect.Width <= 0 || rect.Height <= 0) return;
        // if (cornerLengthHorizontal <= 0 || cornerLengthVertical <= 0)
        // {
        //     rect.Draw(color);
        //     return;
        // }
        //
        // float halfWidth = rect.Width / 2f;
        // float halfHeight = rect.Height / 2f;
        //
        // var tl = rect.TopLeft;
        // var br = rect.BottomRight;
        //
        // if (cornerLengthHorizontal >= halfWidth && cornerLengthVertical >= halfHeight)
        // {
        //     polygonHelper.Clear();
        //     polygonHelper.Add(tl + new Vector2(halfWidth, 0f));
        //     polygonHelper.Add(tl + new Vector2(0f, halfHeight));
        //     polygonHelper.Add(br - new Vector2(halfWidth, 0f));
        //     polygonHelper.Add(br - new Vector2(0f, halfHeight));
        //     polygonHelper.DrawLines(thickness, color);
        //     return;
        // }
        //
        // var bl = rect.BottomLeft;
        // var tr = rect.TopRight;
        //
        // if (cornerLengthHorizontal >= halfWidth)
        // {
        //     var h = new Vector2(halfWidth, 0f);
        //     var v = new Vector2(0f, cornerLengthVertical);
        //     
        //     polygonHelper.Clear();
        //     polygonHelper.Add(tl + h);
        //     polygonHelper.Add(tl + v);
        //     polygonHelper.Add(bl - v);
        //     polygonHelper.Add(bl + h);
        //     polygonHelper.Add(br - v);
        //     polygonHelper.Add(tr + v);
        //     polygonHelper.DrawLines(thickness, color);
        //     
        //     
        // }
        // else if (cornerLengthVertical >= halfHeight)
        // {
        //     var h = new Vector2(cornerLengthHorizontal, 0f);
        //     var v = new Vector2(0f, halfHeight);
        //
        //     polygonHelper.Clear();
        //     polygonHelper.Add(tl + h);
        //     polygonHelper.Add(tl + v);
        //     polygonHelper.Add(bl + h);
        //     polygonHelper.Add(br - h);
        //     polygonHelper.Add(tr + v);
        //     polygonHelper.Add(tr - h);
        //     polygonHelper.DrawLines(thickness, color);
        //     
        // }
        // else
        // {
        //     var cornerHorizontal = new Vector2(cornerLengthHorizontal, 0f);
        //     var cornerVertical = new Vector2(0f, cornerLengthVertical);
        //
        //     polygonHelper.Clear();
        //     polygonHelper.Add(tl + cornerHorizontal);
        //     polygonHelper.Add(tl + cornerVertical);
        //     polygonHelper.Add(bl - cornerVertical);
        //     polygonHelper.Add(bl + cornerHorizontal);
        //     polygonHelper.Add(br - cornerHorizontal);
        //     polygonHelper.Add(br - cornerVertical);
        //     polygonHelper.Add(tr + cornerVertical);
        //     polygonHelper.Add(tr - cornerHorizontal);
        //     polygonHelper.DrawLines(thickness, color);
        //    
        // }
    }
    
    public static void DrawChamferedCornersLines(this Rect rect, float lineThickness, ColorRgba color, float tlCorner, float blCorner, float brCorner, float trCorner)
    {
        var q = rect.ToQuad();
        q.DrawChamferedCornersLines(lineThickness, color, tlCorner, blCorner, brCorner, trCorner);
        // polygonHelper.Clear();
        // FillSlantedCornerPoints(rect, tlCorner, trCorner, brCorner, blCorner, ref polygonHelper);
        // polygonHelper.DrawLines(lineInfo);
    }
    
    #endregion
    
    //TODO: Clean up + Docs
    #region Draw Chamfered Corners Relative Lines
    
    public static void DrawChamferedCornersRelativeLines(this Rect rect, float lineThickness, ColorRgba color, float cornerLengthFactor)
    {
        var q = rect.ToQuad();
        q.DrawChamferedCornersRelativeLines(lineThickness, color, cornerLengthFactor);
        // DrawChamferedCornersRelativeLines(rect, thickness, color, cornerLengthFactor, cornerLengthFactor);
    }
 
    public static void DrawChamferedCornersRelativeLines(this Rect rect, float lineThickness, ColorRgba color, float cornerLengthFactorHorizontal, float cornerLengthFactorVertical)
    {
        var q = rect.ToQuad();
        q.DrawChamferedCornersRelativeLines(lineThickness, color, cornerLengthFactorHorizontal, cornerLengthFactorVertical);
        // var halfWidth = rect.Width / 2f;
        // var halfHeight = rect.Height / 2f;
        // if(cornerLengthFactorHorizontal >= 1f) cornerLengthFactorHorizontal = 1f;
        // if(cornerLengthFactorVertical >= 1f) cornerLengthFactorVertical = 1f;
        // float cornerLengthH = cornerLengthFactorHorizontal * halfWidth;
        // float cornerLengthV = cornerLengthFactorVertical * halfHeight;
        // DrawChamferedCornersLines(rect, thickness, color, cornerLengthH, cornerLengthV);
    }
    
    public static void DrawChamferedCornersRelativeLines(this Rect rect, float lineThickness, ColorRgba color, float tlCornerFactor, float blCornerFactor, float brCornerFactor, float trCornerFactor)
    {
       var q = rect.ToQuad();
       q.DrawChamferedCornersRelativeLines(lineThickness, color, tlCornerFactor, blCornerFactor, brCornerFactor, trCornerFactor);
    }
    #endregion
    
    
    
    #region Draw Grid

    /// <summary>
    /// Draws a grid within the specified bounds using the given line thickness and color.
    /// </summary>
    /// <param name="grid">The grid definition specifying the number of rows and columns.</param>
    /// <param name="bounds">The rectangle bounds in which to draw the grid.</param>
    /// <param name="lineThickness">The thickness of the grid lines.</param>
    /// <param name="color">The color of the grid lines.</param>
    /// <remarks>
    /// The grid is drawn using horizontal and vertical lines spaced according to the number of rows and columns.
    /// </remarks>
    public static void Draw(this Grid grid, Rect bounds, float lineThickness, ColorRgba color)
    {
        Vector2 rowSpacing = new(0f, bounds.Height / grid.Rows);
        for (int row = 0; row < grid.Rows + 1; row++)
        {
            SegmentDrawing.DrawSegment(bounds.TopLeft + rowSpacing * row, bounds.TopRight + rowSpacing * row, lineThickness, color);
        }
        Vector2 colSpacing = new(bounds.Width / grid.Cols, 0f);
        for (int col = 0; col < grid.Cols + 1; col++)
        {
            SegmentDrawing.DrawSegment(bounds.TopLeft + colSpacing * col, bounds.BottomLeft + colSpacing * col, lineThickness, color);
        }
    }

    /// <summary>
    /// Draws a grid inside a rectangle, with the specified number of lines and line drawing information.
    /// </summary>
    /// <param name="r">The rectangle in which to draw the grid.</param>
    /// <param name="lines">The number of grid lines (both horizontal and vertical).</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, etc.).</param>
    public static void DrawGrid(this Rect r, int lines, LineDrawingInfo lineInfo)
    {
        var xOffset = new Vector2(r.Width / lines, 0f);// * i;
        var yOffset = new Vector2(0f, r.Height / lines);// * i;

        var tl = r.TopLeft;
        var tr = tl + new Vector2(r.Width, 0);
        var bl = tl + new Vector2(0, r.Height);

        for (var i = 0; i < lines; i++)
        {
            SegmentDrawing.DrawSegment(tl + xOffset * i, bl + xOffset * i, lineInfo);
            SegmentDrawing.DrawSegment(tl + yOffset * i, tr + yOffset * i, lineInfo);
        }
    }

    #endregion

    #region Draw Nine Patch Rect
    /// <summary>
    /// Draws a <see cref="NinePatchRect"/> using a single color for all patches.
    /// </summary>
    /// <param name="npr">The nine-patch rectangle to draw.</param>
    /// <param name="color">The color to use for all patches.</param>
    public static void Draw(this NinePatchRect npr, ColorRgba color)
    {
        var rects = npr.Rects;
        foreach (var r in rects)
        {
            r.Draw(color);
        }
    }

    /// <summary>
    /// Draws a <see cref="NinePatchRect"/> using separate colors for the source and patch rectangles.
    /// </summary>
    /// <param name="npr">The nine-patch rectangle to draw.</param>
    /// <param name="sourceColorRgba">The color for the source rectangle.</param>
    /// <param name="patchColorRgba">The color for the patch rectangles.</param>
    public static void Draw(this NinePatchRect npr, ColorRgba sourceColorRgba, ColorRgba patchColorRgba)
    {
        npr.Source.Draw(sourceColorRgba);
        var rects = npr.Rects;
        foreach (var r in rects)
        {
            r.Draw(patchColorRgba);
        }
    }
    /// <summary>
    /// Draws the outlines of a <see cref="NinePatchRect"/> using the specified line thickness and color.
    /// </summary>
    /// <param name="npr">The nine-patch rectangle to draw.</param>
    /// <param name="lineThickness">The thickness of the outline lines.</param>
    /// <param name="color">The color of the outline lines.</param>
    public static void DrawLines(this NinePatchRect npr, float lineThickness, ColorRgba color)
    {
        var rects = npr.Rects;
        foreach (var r in rects)
        {
            r.DrawLines(lineThickness, color);
        }
    }

    /// <summary>
    /// Draws the outlines of a <see cref="NinePatchRect"/> using separate line thickness and color for the source and patch rectangles.
    /// </summary>
    /// <param name="npr">The nine-patch rectangle to draw.</param>
    /// <param name="sourceLineThickness">The line thickness for the source rectangle.</param>
    /// <param name="patchLineThickness">The line thickness for the patch rectangles.</param>
    /// <param name="sourceColorRgba">The color for the source rectangle outline.</param>
    /// <param name="patchColorRgba">The color for the patch rectangles outlines.</param>
    public static void DrawLines(this NinePatchRect npr, float sourceLineThickness, float patchLineThickness, ColorRgba sourceColorRgba, ColorRgba patchColorRgba)
    {
        npr.Source.DrawLines(sourceLineThickness, sourceColorRgba);
        var rects = npr.Rects;
        foreach (var r in rects)
        {
            r.DrawLines(patchLineThickness, patchColorRgba);
        }
    }

    #endregion
    
    #region Draw Vertices
    /// <summary>
    /// Draws circles at each vertex of the rectangle.
    /// </summary>
    /// <param name="rect">The rectangle whose vertices to draw.</param>
    /// <param name="vertexRadius">The radius of each vertex circle.</param>
    /// <param name="color">The color of the vertex circles.</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleDrawing.CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    public static void DrawVertices(this Rect rect, float vertexRadius, ColorRgba color, float smoothness = 0.5f)
    {
        var circle = new Circle(rect.TopLeft, vertexRadius);
        circle.Draw(color, smoothness);
        circle = circle.SetPosition(rect.TopRight);
        circle.Draw(color, smoothness);
        circle = circle.SetPosition(rect.BottomLeft);
        circle.Draw(color, smoothness);
        circle = circle.SetPosition(rect.BottomRight);
        circle.Draw(color, smoothness);
    }
    #endregion
    
    #region Draw Masked
    
    /// <summary>
    /// Draws the rectangle's four side segments, but only where they intersect the given triangular mask.
    /// Each side is drawn by forwarding the call to the segment-level masked draw method.
    /// </summary>
    /// <param name="rect">The rectangle whose sides will be drawn.</param>
    /// <param name="mask">The triangular mask used to clip each side.</param>
    /// <param name="lineInfo">Line drawing parameters (thickness, color, cap style, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawLinesMasked(this Rect rect, Triangle mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        rect.TopSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.LeftSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.BottomSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.RightSegment.DrawMasked(mask, lineInfo, reversedMask);
    }
    /// <summary>
    /// Draws the rectangle's four side segments, but only where they intersect the given circular mask.
    /// Each side is drawn by forwarding the call to the segment-level masked draw method.
    /// </summary>
    /// <param name="rect">The rectangle whose sides will be drawn.</param>
    /// <param name="mask">The circular mask used to clip each side.</param>
    /// <param name="lineInfo">Line drawing parameters (thickness, color, cap style, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawLinesMasked(this Rect rect, Circle mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        rect.TopSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.LeftSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.BottomSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.RightSegment.DrawMasked(mask, lineInfo, reversedMask);
    }
    /// <summary>
    /// Draws the rectangle's four side segments, but only where they intersect the given rectangular mask.
    /// Each side is drawn by forwarding the call to the segment-level masked draw method which performs
    /// clipping against the provided <see cref="Rect"/> mask.
    /// </summary>
    /// <param name="rect">The rectangle whose sides will be drawn.</param>
    /// <param name="mask">The rectangular mask used to clip each side.</param>
    /// <param name="lineInfo">Line drawing parameters (thickness, color, cap style, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawLinesMasked(this Rect rect, Rect mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        rect.TopSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.LeftSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.BottomSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.RightSegment.DrawMasked(mask, lineInfo, reversedMask);
    }
    /// <summary>
    /// Draws the rectangle's four side segments, but only where they intersect the given quadrilateral mask.
    /// Each side is forwarded to the corresponding segment-level masked draw method which handles clipping against the provided <see cref="Quad"/> mask.
    /// </summary>
    /// <param name="rect">The rectangle whose sides will be drawn.</param>
    /// <param name="mask">The quadrilateral mask used to clip each side.</param>
    /// <param name="lineInfo">Line drawing parameters (thickness, color, cap style, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawLinesMasked(this Rect rect, Quad mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        rect.TopSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.LeftSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.BottomSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.RightSegment.DrawMasked(mask, lineInfo, reversedMask);
    }
    /// <summary>
    /// Draws the rectangle's four side segments, but only where they intersect the given polygon mask.
    /// Each side is drawn by forwarding the call to the segment-level masked draw method which performs
    /// clipping against the provided <see cref="Polygon"/> mask.
    /// </summary>
    /// <param name="rect">The rectangle whose sides will be drawn.</param>
    /// <param name="mask">The polygonal mask used to clip each side.</param>
    /// <param name="lineInfo">Line drawing parameters (thickness, color, cap style, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawLinesMasked(this Rect rect, Polygon mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        rect.TopSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.LeftSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.BottomSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.RightSegment.DrawMasked(mask, lineInfo, reversedMask);
    }
    /// <summary>
    /// Draws the rectangle's four side segments clipped against a generic closed-shape mask.
    /// </summary>
    /// <typeparam name="T">
    /// The mask type implementing <see cref="IClosedShapeTypeProvider"/> (for example <see cref="Circle"/>, <see cref="Polygon"/>, <see cref="Quad"/>, etc.).
    /// </typeparam>
    /// <param name="rect">The rectangle whose sides will be drawn.</param>
    /// <param name="mask">The mask used to clip each side.</param>
    /// <param name="lineInfo">Line drawing parameters (thickness, color, cap style, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    public static void DrawLinesMasked<T>(this Rect rect, T mask, LineDrawingInfo lineInfo, bool reversedMask = false) where T : IClosedShapeTypeProvider
    {
        rect.TopSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.LeftSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.BottomSegment.DrawMasked(mask, lineInfo, reversedMask);
        rect.RightSegment.DrawMasked(mask, lineInfo, reversedMask);
    }
    #endregion
    
    #region Draw Vignette

    /// <summary>
    /// Draws a "vignette" effect inside the rect, creating a circular hole in the center.
    /// The area between the inner circle and the rect's outer edges is filled with the specified color.
    /// </summary>
    /// <param name="r">The rect to draw the vignette within.</param>
    /// <param name="circleRadius">The radius of the inner circular hole.</param>
    /// <param name="circleRotDeg">The starting rotation angle of the inner circle in degrees.</param>
    /// <param name="color">The color of the filled area.</param>
    /// <param name="circleSmoothness">
    /// Determines the smoothness of the inner circle (0.0 to 1.0). 
    /// Higher values result in more segments and a smoother circle.
    /// </param>
    public static void DrawVignette(this Rect r, float circleRadius, float circleRotDeg, ColorRgba color, float circleSmoothness = 0.5f)
    {
        var q = r.ToQuad();
        q.DrawVignette(circleRadius, circleRotDeg, color, circleSmoothness);
    }
    #endregion
 
    
    #region Helper
    //TODO: Check all functions that use polygonHelper if it is still necessary
    private static Polygon polygonHelper = new(12);
    
    private static void FillSlantedCornerPoints(Rect rect, float tlCorner, float trCorner, float brCorner, float blCorner, ref Polygon points)
    {
        var halfWidth = rect.Width / 2f;
        var halfHeight = rect.Height / 2f;
        if (tlCorner <= 0)
        {
            points.Add(rect.TopLeft);
        }
        else
        {
            points.Add(rect.TopLeft + new Vector2(MathF.Min(tlCorner, halfWidth), 0f));
            points.Add(rect.TopLeft + new Vector2(0f, MathF.Min(tlCorner, halfHeight)));
        }
        
        if (blCorner <= 0)
        {
            points.Add(rect.BottomLeft);
        }
        else
        {
            if (blCorner < halfHeight)
            {
                points.Add(rect.BottomLeft - new Vector2(0f, MathF.Min(blCorner, halfHeight)));
            }
            
            if (blCorner < halfWidth)
            {
                points.Add(rect.BottomLeft + new Vector2(MathF.Min(blCorner, halfWidth), 0f));
            }
        }
        
        if (brCorner <= 0)
        {
            points.Add(rect.BottomRight);
        }
        else
        {
            points.Add(rect.BottomRight - new Vector2(MathF.Min(brCorner, halfWidth), 0f));
            points.Add(rect.BottomRight - new Vector2(0f, MathF.Min(brCorner, halfHeight)));
        }
        
        if (trCorner <= 0)
        {
            points.Add(rect.TopRight);
        }
        else
        {
            if (trCorner < halfHeight)
            {
                points.Add(rect.TopRight + new Vector2(0f, MathF.Min(trCorner, halfHeight)));
            }
            
            if (trCorner < halfWidth)
            {
                points.Add(rect.TopRight - new Vector2(MathF.Min(trCorner, halfWidth), 0f));
            }
        }
    }
    #endregion
    
}
    
   