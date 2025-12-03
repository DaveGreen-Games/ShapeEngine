
using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.QuadDef;

/// <summary>
/// Provides static methods for drawing quads (quadrilaterals) and their outlines, including partial outlines and vertex markers.
/// </summary>
/// <remarks>
/// This class contains utility methods for rendering quads with various options such as line thickness, color, partial outlines, and scaling.
/// </remarks>
public static class QuadDrawing
{
    #region Draw Masked
    /// <summary>
    /// Draws the quad's outline segments constrained by a <see cref="Triangle"/> mask.
    /// </summary>
    /// <param name="quad">The quad whose sides will be drawn (extension receiver).</param>
    /// <param name="mask">Triangle mask used to clip each segment's drawing.</param>
    /// <param name="lineInfo">Line drawing parameters (thickness, color, cap type, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    /// <remarks>
    /// This extension method forwards the draw call to each segment's <c>DrawMasked</c> overload,
    /// allowing per-segment clipping by the provided triangle mask.
    /// </remarks>
    public static void DrawLinesMasked(this Quad quad, Triangle mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        quad.SegmentAToB.DrawMasked(mask, lineInfo, reversedMask);
        quad.SegmentBToC.DrawMasked(mask, lineInfo, reversedMask);
        quad.SegmentCToD.DrawMasked(mask, lineInfo, reversedMask);
        quad.SegmentDToA.DrawMasked(mask, lineInfo, reversedMask);
    }
    /// <summary>
    /// Draws the quad's outline segments constrained by a <see cref="Circle"/> mask.
    /// </summary>
    /// <param name="quad">The quad whose sides will be drawn (extension receiver).</param>
    /// <param name="mask">Circle mask used to clip each segment's drawing.</param>
    /// <param name="lineInfo">Line drawing parameters (thickness, color, cap type, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    /// <remarks>
    /// This extension method forwards the draw call to each segment's <c>DrawMasked</c> overload,
    /// allowing per-segment clipping by the provided circle mask.
    /// </remarks>
    public static void DrawLinesMasked(this Quad quad, Circle mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        quad.SegmentAToB.DrawMasked(mask, lineInfo, reversedMask);
        quad.SegmentBToC.DrawMasked(mask, lineInfo, reversedMask);
        quad.SegmentCToD.DrawMasked(mask, lineInfo, reversedMask);
        quad.SegmentDToA.DrawMasked(mask, lineInfo, reversedMask);
    }
    /// <summary>
    /// Draws the quad's outline segments constrained by a <see cref="Rect"/> mask.
    /// </summary>
    /// <param name="quad">The quad whose sides will be drawn (extension receiver).</param>
    /// <param name="mask">Rect mask used to clip each segment's drawing.</param>
    /// <param name="lineInfo">Line drawing parameters (thickness, color, cap type, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    /// <remarks>
    /// This extension method forwards the draw call to each segment's <c>DrawMasked</c> overload,
    /// allowing per-segment clipping by the provided rectangle mask.
    /// </remarks>
    public static void DrawLinesMasked(this Quad quad, Rect mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        quad.SegmentAToB.DrawMasked(mask, lineInfo, reversedMask);
        quad.SegmentBToC.DrawMasked(mask, lineInfo, reversedMask);
        quad.SegmentCToD.DrawMasked(mask, lineInfo, reversedMask);
        quad.SegmentDToA.DrawMasked(mask, lineInfo, reversedMask);
    }
    /// <summary>
    /// Draws the quad's outline segments constrained by a <see cref="Quad"/> mask.
    /// </summary>
    /// <param name="quad">The quad whose sides will be drawn (extension receiver).</param>
    /// <param name="mask">Quad mask used to clip each segment's drawing.</param>
    /// <param name="lineInfo">Line drawing parameters (thickness, color, cap type, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    /// <remarks>
    /// Forwards the draw call to each segment's <c>DrawMasked</c> overload,
    /// allowing per-segment clipping by the provided quad mask.
    /// </remarks>
    public static void DrawLinesMasked(this Quad quad, Quad mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        quad.SegmentAToB.DrawMasked(mask, lineInfo, reversedMask);
        quad.SegmentBToC.DrawMasked(mask, lineInfo, reversedMask);
        quad.SegmentCToD.DrawMasked(mask, lineInfo, reversedMask);
        quad.SegmentDToA.DrawMasked(mask, lineInfo, reversedMask);
    }
    /// <summary>
    /// Draws the quad's outline segments constrained by a <see cref="Polygon"/> mask.
    /// </summary>
    /// <param name="quad">The quad whose sides will be drawn (extension receiver).</param>
    /// <param name="mask">Polygon mask used to clip each segment's drawing.</param>
    /// <param name="lineInfo">Line drawing parameters (thickness, color, cap type, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    /// <remarks>
    /// Forwards the draw call to each segment's <c>DrawMasked</c> overload,
    /// allowing per-segment clipping by the provided polygon mask.
    /// </remarks>
    public static void DrawLinesMasked(this Quad quad, Polygon mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        quad.SegmentAToB.DrawMasked(mask, lineInfo, reversedMask);
        quad.SegmentBToC.DrawMasked(mask, lineInfo, reversedMask);
        quad.SegmentCToD.DrawMasked(mask, lineInfo, reversedMask);
        quad.SegmentDToA.DrawMasked(mask, lineInfo, reversedMask);
    }
    /// <summary>
    /// Draws the quad's outline segments constrained by a generic closed-shape mask.
    /// </summary>
    /// <typeparam name="T">
    /// The mask type. Must implement <see cref="IClosedShapeTypeProvider"/> to provide closed-shape semantics
    /// required for segment clipping.
    /// </typeparam>
    /// <param name="quad">The quad whose sides will be drawn (extension receiver).</param>
    /// <param name="mask">Mask used to clip each segment's drawing.</param>
    /// <param name="lineInfo">Line drawing parameters (thickness, color, cap type, etc.).</param>
    /// <param name="reversedMask">If true, draws the parts inside the mask instead of outside.</param>
    /// <remarks>
    /// Forwards the draw call to each segment's <c>DrawMasked</c> overload, allowing per-segment clipping
    /// by the provided mask. This generic overload enables using any closed-shape provider without
    /// adding a separate overload for each concrete shape type.
    /// </remarks>
    public static void DrawLinesMasked<T>(this Quad quad, T mask, LineDrawingInfo lineInfo, bool reversedMask = false) where T : IClosedShapeTypeProvider
    {
        quad.SegmentAToB.DrawMasked(mask, lineInfo, reversedMask);
        quad.SegmentBToC.DrawMasked(mask, lineInfo, reversedMask);
        quad.SegmentCToD.DrawMasked(mask, lineInfo, reversedMask);
        quad.SegmentDToA.DrawMasked(mask, lineInfo, reversedMask);
    }
    #endregion
    
    #region Draw
    /// <summary>
    /// Draws a filled quadrilateral from four corner positions.
    /// </summary>
    /// <param name="a">First vertex of the quad.</param>
    /// <param name="b">Second vertex of the quad.</param>
    /// <param name="c">Third vertex of the quad.</param>
    /// <param name="d">Fourth vertex of the quad.</param>
    /// <param name="color">Fill color for the quad.</param>
    /// <param name="roundness">Corner roundness factor. If &lt;= 0 the quad is drawn with sharp corners.</param>
    /// <param name="cornerPoints">Number of points used to approximate rounded corners. Ignored when <paramref name="roundness"/> &lt;= 0.</param>
    /// <remarks>
    /// If <paramref name="roundness"/> &gt; 0 and <paramref name="cornerPoints"/> &gt; 0, rounded-corner helpers are used.
    /// Otherwise the quad is rendered as two triangles for sharp corners.
    /// </remarks>
    public static void DrawQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d, ColorRgba color, float roundness = 0f, int cornerPoints = 0)
    {
        if(roundness <= 0f || cornerPoints <= 0)
        {
            // Draw sharp-cornered quad as two triangles
            Raylib.DrawTriangle(a, b, c, color.ToRayColor());
            Raylib.DrawTriangle(a, c, d, color.ToRayColor());
            return;
        }
        
        DrawRoundedHelper(a, b, c, d, roundness, cornerPoints, color);
        
    }
    
    /// <summary>
    /// Draws this <see cref="Quad"/> filled with the specified <paramref name="color"/>.
    /// This is an extension convenience that forwards to the quad vertex overload.
    /// </summary>
    /// <param name="q">The quad instance to draw (extension receiver).</param>
    /// <param name="color">Fill color for the quad.</param>
    /// <param name="roundness">Optional corner roundness factor (0 = sharp corners).</param>
    /// <param name="cornerPoints">Number of points used to approximate rounded corners. Ignored when <paramref name="roundness"/> is 0.</param>
    public static void Draw(this Quad q, ColorRgba color, float roundness = 0f, int cornerPoints = 0)
    {
        DrawQuad(q.A, q.B, q.C, q.D, color, roundness, cornerPoints);
        
    }
    #endregion
    
    #region Draw Lines

    /// <summary>
    /// Draws the outline of a quadrilateral given four corner positions.
    /// Uses straight edges when <paramref name="roundness"/> is 0 or <paramref name="cornerPoints"/> is 0;
    /// otherwise may be used to draw rounded corners when supported by helpers.
    /// </summary>
    /// <param name="a">First vertex of the quad.</param>
    /// <param name="b">Second vertex of the quad.</param>
    /// <param name="c">Third vertex of the quad.</param>
    /// <param name="d">Fourth vertex of the quad.</param>
    /// <param name="lineThickness">Thickness of the outline in pixels.</param>
    /// <param name="color">Color used to draw the outline.</param>
    /// <param name="roundness">Optional corner roundness factor (0 = sharp corners).</param>
    /// <param name="cornerPoints">Optional number of points used to approximate rounded corners.</param>
    public static void DrawQuadLines(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float lineThickness, ColorRgba color, float roundness = 0, int cornerPoints = 0)
    {
        DrawQuadLinesInternal(a, b, c, d, lineThickness, color, roundness, cornerPoints);
    }

    /// <summary>
    /// Draws the outline of a quadrilateral using the given corner positions and a <see cref="LineDrawingInfo"/>
    /// that describes thickness, color and cap style.
    /// </summary>
    /// <param name="a">First vertex of the quad.</param>
    /// <param name="b">Second vertex of the quad.</param>
    /// <param name="c">Third vertex of the quad.</param>
    /// <param name="d">Fourth vertex of the quad.</param>
    /// <param name="lineInfo">Line drawing parameters (thickness, color, cap type, cap points).</param>
    /// <param name="roundness">Optional corner roundness factor (0 = sharp corners). Rounded corners are applied if supported by helpers.</param>
    public static void DrawQuadLines(Vector2 a, Vector2 b, Vector2 c, Vector2 d, LineDrawingInfo lineInfo, float roundness = 0)
    {
        DrawQuadLinesInternal(a, b, c, d, lineInfo.Thickness, lineInfo.Color, roundness, lineInfo.CapPoints);
    }

    /// <summary>
    /// Draws the outline of a <see cref="Quad"/> using a specified line thickness and color.
    /// </summary>
    /// <param name="q">The quad whose outline will be drawn.</param>
    /// <param name="lineThickness">Thickness of the outline in pixels.</param>
    /// <param name="color">Color used to draw the outline.</param>
    /// <param name="roundness">Optional corner roundness factor (0 = sharp corners). When non-zero, rounded corners may be used if supported by helpers.</param>
    /// <param name="cornerPoints">Number of points used to approximate rounded corners (ignored when <paramref name="roundness"/> is 0).</param>
    public static void DrawLines(this Quad q, float lineThickness, ColorRgba color, float roundness = 0, int cornerPoints = 0)
    {
        DrawQuadLines(q.A, q.B, q.C, q.D, lineThickness, color, roundness, cornerPoints);
    }

    /// <summary>
    /// Draws the outline of a <see cref="Quad"/> using the supplied <see cref="LineDrawingInfo"/>.
    /// The <see cref="LineDrawingInfo"/> provides line thickness, color and cap style. When
    /// <paramref name="roundness"/> is non-zero, rounded corners may be produced if supported by
    /// the underlying drawing helpers.
    /// </summary>
    /// <param name="q">The quad whose outline will be drawn.</param>
    /// <param name="lineInfo">Line drawing parameters (thickness, color, cap type, cap points).</param>
    /// <param name="roundness">Optional corner roundness factor (0 = sharp corners).</param>
    public static void DrawLines(this Quad q, LineDrawingInfo lineInfo, float roundness = 0)
    {
        DrawQuadLines(q.A, q.B, q.C, q.D, lineInfo, roundness);
    }

    #endregion
    
    #region Draw Lines Percentage
    public static void DrawQuadLinesPercentage(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float f, float lineThickness, ColorRgba color, float roundness = 0, int cornerPoints = 0)
    {
        DrawQuadLinesPercentageHelper(a, b, c, d,f, lineThickness, color, roundness, cornerPoints);
    }
    public static void DrawLinesPercentage(this Quad q, float f, float lineThickness, ColorRgba color, float roundness = 0, int cornerPoints = 0)
    {
        DrawQuadLinesPercentage(q.A, q.B, q.C, q.D, f, lineThickness, color, roundness, cornerPoints);
    }

    public static void DrawLinesPercentage(this Quad q, float f, LineDrawingInfo lineInfo, float roundness = 0)
    {
        DrawQuadLinesPercentage(q.A, q.B, q.C, q.D, f, lineInfo.Thickness, lineInfo.Color, roundness, lineInfo.CapPoints);
    }

    #endregion
    
    #region Draw Lines Scaled
    /// <summary>
    /// Draws the outline of a quadrilateral, scaling each side by a specified factor.
    /// </summary>
    /// <param name="a">The first vertex of the quad.</param>
    /// <param name="b">The second vertex of the quad.</param>
    /// <param name="c">The third vertex of the quad.</param>
    /// <param name="d">The fourth vertex of the quad.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="sideLengthFactor">The factor by which to scale each side (0 = no line, 1 = full length).</param>
    /// <param name="capType">The style of the line caps.</param>
    /// <param name="capPoints">The number of points used for the cap style.</param>
    /// <remarks>
    /// Each side is drawn from its starting vertex towards its ending vertex, scaled by <paramref name="sideLengthFactor"/>.
    /// </remarks>
    public static void DrawQuadLines(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float lineThickness, ColorRgba color, float sideLengthFactor, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        var side1 = b - a;
        var end1 = a + side1 * sideLengthFactor;
        
        var side2 = c - b;
        var end2 = b + side2 * sideLengthFactor;
        
        var side3 = d - c;
        var end3 = c + side3 * sideLengthFactor;
        
        var side4 = a - d;
        var end4 = d + side4 * sideLengthFactor;
        
        SegmentDrawing.DrawSegment(a, end1, lineThickness, color, capType, capPoints);
        SegmentDrawing.DrawSegment(b, end2, lineThickness, color, capType, capPoints);
        SegmentDrawing.DrawSegment(c, end3, lineThickness, color, capType, capPoints);
        SegmentDrawing.DrawSegment(d, end4, lineThickness, color, capType, capPoints);
    }
    /// <summary>
    /// Draws the outline of a <see cref="Quad"/>, scaling each side by a specified factor.
    /// </summary>
    /// <param name="q">The quad to outline.</param>
    /// <param name="lineThickness">The thickness of the outline.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="sideLengthFactor">The factor by which to scale each side (0 = no line, 1 = full length).</param>
    /// <param name="capType">The style of the line caps.</param>
    /// <param name="capPoints">The number of points used for the cap style.</param>
    /// <remarks>
    /// Each side is drawn from its starting vertex towards its ending vertex, scaled by <paramref name="sideLengthFactor"/>.
    /// </remarks>
    public static void DrawLines(this Quad q, float lineThickness, ColorRgba color, float sideLengthFactor, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        DrawQuadLines(q.A, q.B, q.C, q.D, lineThickness, color, sideLengthFactor, capType, capPoints);
    }
    /// <summary>
    /// Draws the outline of a <see cref="Quad"/> where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="q">The quad to outline.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <param name="sideScaleFactor">
    /// <para>The scale factor for each side.</para>
    /// <list type="bullet">
    /// <item><description>0: No quad is drawn.</description></item>
    /// <item><description>1: The normal quad is drawn.</description></item>
    /// <item><description>0.5: Each side is half as long.</description></item>
    /// </list>
    /// </param>
    /// <param name="sideScaleOrigin">
    /// The point along the line to scale from, in both directions (0 to 1).
    /// <list type="bullet">
    /// <item><description>0: Start of Segment</description></item>
    /// <item><description>0.5: Center of Segment</description></item>
    /// <item><description>1: End of Segment</description></item>
    /// </list>
    /// </param>
    /// <remarks>
    /// Allows for dynamic scaling and rotation of quad outlines, useful for effects and animations.
    /// </remarks>
    public static void DrawLinesScaled(this Quad q, LineDrawingInfo lineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0) return;
        
        if (sideScaleFactor >= 1)
        {
            q.DrawLines(lineInfo);
            return;
        }
        
        SegmentDrawing.DrawSegment(q.A, q.B, lineInfo, sideScaleFactor, sideScaleOrigin);
        SegmentDrawing.DrawSegment(q.B, q.C, lineInfo, sideScaleFactor, sideScaleOrigin);
        SegmentDrawing.DrawSegment(q.C, q.D, lineInfo, sideScaleFactor, sideScaleOrigin);
        SegmentDrawing.DrawSegment(q.D, q.A, lineInfo, sideScaleFactor, sideScaleOrigin);
        
    }
    
    #endregion
 
    #region Draw Corners

    public static void DrawQuadCorners(Vector2 a, Vector2 b, Vector2 c, Vector2 d, LineDrawingInfo lineInfo, float tlCorner, float trCorner, float brCorner, float blCorner)
    {
        float w = (d - a).Length();
        float h = (b - a).Length();
        var size = new Size(w, h);
        if(lineInfo.Thickness <= 0f || lineInfo.Color.A <= 0 || size.Width <= 0 || size.Height <= 0) return;

        var nL = (a - d).Normalize();
        var nD = (b - a).Normalize();
        var nR = (c - b).Normalize();
        var nU = (d - c).Normalize();
        
        float miterLength = MathF.Sqrt(lineInfo.Thickness * lineInfo.Thickness * 2f);
        float halfWidth = size.Width / 2f;
        float halfHeight = size.Height / 2f;

        if ((lineInfo.CapType == LineCapType.CappedExtended && lineInfo.CapPoints > 0) || lineInfo.CapType == LineCapType.Extended)
        {
            if (halfWidth > halfHeight)
            {
                halfHeight -= lineInfo.Thickness;
            }
            else if(halfWidth < halfHeight)
            {
                halfWidth -= lineInfo.Thickness;
            }
            else
            {
                halfWidth -= lineInfo.Thickness;
                halfHeight -= lineInfo.Thickness;
            }
        }

        if (tlCorner > 0f)
        {
            DrawCorner(a, nU, nL, MathF.Min(tlCorner, halfHeight), MathF.Min(tlCorner, halfWidth), lineInfo.Thickness, miterLength,
                lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
        }

        if (trCorner > 0f)
        {
            DrawCorner(d, nR, nU, MathF.Min(trCorner, halfWidth), MathF.Min(trCorner, halfHeight), lineInfo.Thickness, miterLength,
                lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
        }

        if (brCorner > 0f)
        {
            DrawCorner(c, nD, nR, MathF.Min(brCorner, halfHeight), MathF.Min(brCorner, halfWidth), lineInfo.Thickness, miterLength,
                lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
        }

        if (blCorner > 0f)
        {
            DrawCorner(b, nL, nD, MathF.Min(blCorner, halfWidth), MathF.Min(blCorner, halfHeight), lineInfo.Thickness, miterLength,
                lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
        }
    }
    public static void DrawQuadCornersRelative(Vector2 a, Vector2 b, Vector2 c, Vector2 d, LineDrawingInfo lineInfo, float tlCornerFactor, float trCornerFactor, float brCornerFactor, float blCornerFactor)
    {
        float w = (d - a).Length();
        float h = (b - a).Length();
        float minSize = MathF.Min(w, h);
        DrawQuadCorners(a, b, c, d, lineInfo, tlCornerFactor * minSize, trCornerFactor * minSize, brCornerFactor * minSize, blCornerFactor * minSize);
    }
    
    public static void DrawCorners(this Quad quad, LineDrawingInfo lineInfo, float tlCorner, float trCorner, float brCorner, float blCorner)
    {
        DrawQuadCorners(quad.A, quad.B, quad.C, quad.D, lineInfo, tlCorner, trCorner, brCorner, blCorner);
        // var size = quad.GetSize();
        // if(lineInfo.Thickness <= 0f || lineInfo.Color.A <= 0 || size.Width <= 0 || size.Height <= 0) return;
        //
        // var tl = quad.A;
        // var bl = quad.B;
        // var br = quad.C;
        // var tr = quad.D;
        //
        // var nL = quad.NormalLeft;
        // var nD = quad.NormalDown;
        // var nR = quad.NormalRight;
        // var nU = quad.NormalUp;
        //
        // var miterLength = MathF.Sqrt(lineInfo.Thickness * lineInfo.Thickness * 2f);
        // var halfWidth = size.Width / 2f;
        // var halfHeight = size.Height / 2f;
        //
        // if (tlCorner > 0f)
        // {
        //     DrawCorner(tl, nU, nL, MathF.Min(tlCorner, halfHeight), MathF.Min(tlCorner, halfWidth), lineInfo.Thickness, miterLength,
        //         lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
        // }
        //
        // if (trCorner > 0f)
        // {
        //     DrawCorner(tr, nR, nU, MathF.Min(trCorner, halfWidth), MathF.Min(trCorner, halfHeight), lineInfo.Thickness, miterLength,
        //         lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
        // }
        //
        // if (brCorner > 0f)
        // {
        //     DrawCorner(br, nD, nR, MathF.Min(brCorner, halfHeight), MathF.Min(brCorner, halfWidth), lineInfo.Thickness, miterLength,
        //         lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
        // }
        //
        // if (blCorner > 0f)
        // {
        //     DrawCorner(bl, nL, nD, MathF.Min(blCorner, halfWidth), MathF.Min(blCorner, halfHeight), lineInfo.Thickness, miterLength,
        //         lineInfo.Color, lineInfo.CapType, lineInfo.CapPoints);
        // }
    }
    public static void DrawCorners(this Quad quad, LineDrawingInfo lineInfo, float cornerLength)
    {
        DrawCorners(quad, lineInfo, cornerLength, cornerLength, cornerLength, cornerLength);
    }
    public static void DrawCornersRelative(this Quad quad, LineDrawingInfo lineInfo, float tlCornerFactor, float trCornerFactor, float brCornerFactor, float blCornerFactor)
    {
        float minSize = quad.GetSize().Min();
        DrawCorners(quad, lineInfo, tlCornerFactor * minSize, trCornerFactor * minSize, brCornerFactor * minSize, blCornerFactor * minSize);
    }
    public static void DrawCornersRelative(this Quad quad, LineDrawingInfo lineInfo, float cornerLengthFactor)
    {
        DrawCornersRelative(quad, lineInfo, cornerLengthFactor, cornerLengthFactor, cornerLengthFactor, cornerLengthFactor);
    }

    #endregion
    
    #region Draw Vertices
    /// <summary>
    /// Draws circles at each vertex of a <see cref="Quad"/>.
    /// </summary>
    /// <param name="q">The quad whose vertices to draw.</param>
    /// <param name="vertexRadius">The radius of each vertex circle.</param>
    /// <param name="color">The color of the vertex circles.</param>
    /// <param name="circleSegments">The number of segments to use for each circle (default is 8).</param>
    /// <remarks>
    /// Useful for visualizing or highlighting the corners of a quad.
    /// </remarks>
    public static void DrawVertices(this Quad q, float vertexRadius, ColorRgba color, int circleSegments = 8)
    {
        CircleDrawing.DrawCircle(q.A, vertexRadius, color, circleSegments);
        CircleDrawing.DrawCircle(q.B, vertexRadius, color, circleSegments);
        CircleDrawing.DrawCircle(q.C, vertexRadius, color, circleSegments);
        CircleDrawing.DrawCircle(q.D, vertexRadius, color, circleSegments);
    }
    #endregion
    
    #region Helper
    private static void DrawQuadLinesInternal(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float lineThickness, ColorRgba color, float roundness = 0f, int cornerPoints = 0)
    {
        if(cornerPoints > 0 && roundness > 0f)
        {
            DrawLinesRoundedHelper(a, b, c, d, roundness, cornerPoints, lineThickness, color);
            return;
        }
        var offsetDistance = MathF.Sqrt(2f * lineThickness * lineThickness);
        
        // corner at b, adjacent vertices a and c
        var bA = Vector2.Normalize(a - b); // direction from corner toward A
        var bC = Vector2.Normalize(c - b); // direction from corner toward C

        var internalBisectorB = bA + bC;
        if (internalBisectorB.LengthSquared() < 1e-8f)
        {
            // edges are colinear; pick a perpendicular as fallback
            internalBisectorB = new Vector2(-bA.Y, bA.X);
        }
        else
        {
            internalBisectorB = Vector2.Normalize(internalBisectorB);
        }
        
        var aB = Vector2.Normalize(b - a);
        var aD = Vector2.Normalize(d - a);

        var internalBisectorA = aB + aD;
        if (internalBisectorA.LengthSquared() < 1e-8f)
        {
            // edges are colinear; pick a perpendicular as fallback
            internalBisectorA = new Vector2(-aB.Y, aB.X);
        }
        else
        {
            internalBisectorA = Vector2.Normalize(internalBisectorA);
        }
        
        var outsideA = a - internalBisectorA * offsetDistance;
        var insideA = a + internalBisectorA * offsetDistance;
        
        var outsideB = b - internalBisectorB * offsetDistance;
        var insideB = b + internalBisectorB * offsetDistance;
        
        var outsideC = c + internalBisectorA * offsetDistance;
        var insideC = c - internalBisectorA * offsetDistance;
        
        var outsideD = d + internalBisectorB * offsetDistance;
        var insideD = d - internalBisectorB * offsetDistance;
        
        TriangleDrawing.DrawTriangle(outsideA, outsideB, insideA, color);
        TriangleDrawing.DrawTriangle(insideA, outsideB, insideB, color);
        
        TriangleDrawing.DrawTriangle(outsideB, outsideC, insideB, color);
        TriangleDrawing.DrawTriangle(insideB, outsideC, insideC, color);
        
        TriangleDrawing.DrawTriangle(outsideC, outsideD, insideC, color);
        TriangleDrawing.DrawTriangle(insideC, outsideD, insideD, color);
        
        TriangleDrawing.DrawTriangle(outsideD, outsideA, insideD, color);
        TriangleDrawing.DrawTriangle(insideD, outsideA, insideA, color);
    }
    
    
    private static readonly Vector2[] centerHelper = new Vector2[4];
    private static readonly float[] cornerStartAnglesHelper = new float[4];
    private static readonly List<Vector2> innerPointsHelper = [];
    private static readonly List<Vector2> outerPointsHelper = [];
    private static void DrawLinesRoundedHelper(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, float roundness, int segments, float lineThick, ColorRgba color)
    {
        //using raylibs C DrawRectangleRoundedLinesEx implementation as a base

        if (lineThick < 0f)
        {
            // lineThick = 0f;
            return;
        }
        
        if (roundness <= 0f)
        {
            DrawQuadLines(p1, p2, p3, p4, lineThick, color);
            return;
        }
    
        if (roundness >= 1.0f) roundness = 1.0f;
    
        var edge1 = p2 - p1;
        var edge2 = p3 - p2;
        var edge3 = p4 - p3;
        var edge4 = p1 - p4;
        float size1 = edge1.Length();
        float size2 = edge4.Length();
        
        // Calculate corner radius
        float radius = (size1 > size2) ? (size2 * roundness) / 2f : (size1 * roundness) / 2f;
        if (radius <= 0f) return;

        if(radius <= lineThick) radius = lineThick;

        if(segments < 2) segments = 2;
    
        //this function always goes ccw direction -> so stepLength is negative
        float stepLengthRad = (-MathF.PI * 0.5f) / (float)segments; // radians per segment on each corner
        float outerRadius = radius + lineThick;
        float innerRadius = radius - lineThick;

        var n1 = edge1.Normalize();
        var n2 = edge2.Normalize();
        var n3 = edge3.Normalize();
        var n4 = edge4.Normalize();

        var dir1 = (n1 - n4).Normalize();
        var dir2 = (n2 - n1).Normalize();
        var dir3 = (n3 - n2).Normalize();
        var dir4 = (n4 - n3).Normalize();
        
        var dis = MathF.Sqrt(radius * radius * 2f);
        
        centerHelper[0] = p1 + dir1 * dis;
        centerHelper[1] = p2 + dir2 * dis;
        centerHelper[2] = p3 + dir3 * dis;
        centerHelper[3] = p4 + dir4 * dis;
        cornerStartAnglesHelper[0] = (-n1).AngleRad();
        cornerStartAnglesHelper[1] = (-n2).AngleRad();
        cornerStartAnglesHelper[2] = (-n3).AngleRad();
        cornerStartAnglesHelper[3] = (-n4).AngleRad();
    
        innerPointsHelper.Clear();
        outerPointsHelper.Clear();

        for (var c = 0; c < 4; c++)
        {
            float startAngRad = cornerStartAnglesHelper[c];
            var center = centerHelper[c];
    
            for (int i = 0; i <= segments; i++) // inclusive to include corner endpoints
            {
                float angRad = startAngRad + i * stepLengthRad;
                var dir = ShapeVec.Right().Rotate(angRad);
    
                var outerP = center + dir * outerRadius;
                var innerP = center + dir * innerRadius;
    
                outerPointsHelper.Add(outerP);
                innerPointsHelper.Add(innerP);
            }
        }
    
        int count = innerPointsHelper.Count;
        if (count < 2) return;
        var rayColor = color.ToRayColor();
        // Thick outline: draw quads between outer and inner loops using two triangles per segment
        for (var i = 0; i < count; i++)
        {
            int next = (i + 1) % count;
    
            var o1 = outerPointsHelper[i];
            var o2 = outerPointsHelper[next];
            var i1 = innerPointsHelper[i];
            var i2 = innerPointsHelper[next];
    
            // Draw two triangles that form the quad between o1-o2-i2-i1
            Raylib.DrawTriangle(i1, o1 , i2, rayColor);
            Raylib.DrawTriangle(i2, o1 , o2, rayColor);
        }
        
    }
    private static void DrawRoundedHelper(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, float roundness, int segments, ColorRgba color)
    {
        if(roundness <= 0f) return;
        if(segments < 2) segments = 2;
        
        var edge1 = p2 - p1;
        var edge4 = p1 - p4;
        float size1 = edge1.Length();
        float size2 = edge4.Length();
        
        float radius = (size1 > size2) ? (size2 * roundness) / 2f : (size1 * roundness) / 2f;
        if (radius <= 0f) return;
        
        var edge2 = p3 - p2;
        var edge3 = p4 - p3;
        
        var n1 = edge1.Normalize();
        var n2 = edge2.Normalize();
        var n3 = edge3.Normalize();
        var n4 = edge4.Normalize();
        
        if (roundness >= 1)
        {
            if (Math.Abs(size1 - size2) < 0.00001f)
            {
                var center = p1 + (p3 - p1) * 0.5f;
                CircleDrawing.DrawCircle(center, size1 * 0.5f, color, segments * 4);
                return;
            }

            if (size1 < size2)
            {
                p1 = p1 + n2 * radius;
                p2 = p2 + n2 * radius;
                p3 = p3 + n4 * radius;
                p4 = p4 + n4 * radius;
                TriangleDrawing.DrawTriangle(p1, p2, p3, color);
                TriangleDrawing.DrawTriangle(p1, p3, p4, color);
                var cap1Center = p1 + n1 * size1 * 0.5f;
                var cap2Center = p4 + n1 * size1 * 0.5f;
                SegmentDrawing.DrawRoundCap(cap1Center, n4, radius, segments, color);
                SegmentDrawing.DrawRoundCap(cap2Center, n2, radius, segments, color);
            }
            else
            {
                p1 = p1 + n1 * radius;
                p2 = p2 - n1 * radius;
                p3 = p3 + n3 * radius;
                p4 = p4 - n3 * radius;
                TriangleDrawing.DrawTriangle(p1, p2, p3, color);
                TriangleDrawing.DrawTriangle(p1, p3, p4, color);
                var cap1Center = p4 + n4 * size2 * 0.5f;
                var cap2Center = p3 + n4 * size2 * 0.5f;
                SegmentDrawing.DrawRoundCap(cap1Center, n3, radius, segments, color);
                SegmentDrawing.DrawRoundCap(cap2Center, n1, radius, segments, color);
            }
            return;
        }
        
        // float stepLengthRad = (-MathF.PI * 0.5f) / (float)segments; // radians per segment on each corner
        
        var dir1 = (n1 - n4).Normalize();
        var dir2 = (n2 - n1).Normalize();
        var dir3 = (n3 - n2).Normalize();
        var dir4 = (n4 - n3).Normalize();
        
        var dis = MathF.Sqrt(radius * radius * 2f);
        
        var prev1 = p1 - n4 * radius;
        var center1 = p1 + dir1 * dis;
        var next1 = p1 + n1 * radius;
        
        var prev2 = p2 - n1 * radius;
        var center2 = p2 + dir2 * dis;
        var next2 = p2 + n2 * radius;
        
        var prev3 = p3 - n2 * radius;
        var center3 = p3 + dir3 * dis;
        var next3 = p3 + n3 * radius;
        
        var prev4 = p4 - n3 * radius;
        var center4 = p4 + dir4 * dis;
        var next4 = p4 + n4 * radius;
        
        TriangleDrawing.DrawTriangle(next1, prev2, center2, color);
        TriangleDrawing.DrawTriangle(next1, center2, center1, color);
        
        TriangleDrawing.DrawTriangle(center2, next2, prev3, color);
        TriangleDrawing.DrawTriangle(center2, prev3, center3, color);
        
        TriangleDrawing.DrawTriangle(center4, center3, next3, color);
        TriangleDrawing.DrawTriangle(center4, next3, prev4, color);
        
        TriangleDrawing.DrawTriangle(prev1, center1, center4, color);
        TriangleDrawing.DrawTriangle(prev1, center4, next4, color);
        
        TriangleDrawing.DrawTriangle(center1, center2, center3, color);
        TriangleDrawing.DrawTriangle(center1, center3, center4, color);
        
        DrawRoundedCornerHelper(center1, -n1, n4, radius, segments, color);
        DrawRoundedCornerHelper(center2, -n2, n1, radius, segments, color);
        DrawRoundedCornerHelper(center3, -n3, n2, radius, segments, color);
        DrawRoundedCornerHelper(center4, -n4, n3, radius, segments, color);
    }
    private static void DrawRoundedCornerHelper(Vector2 cornerCenter, Vector2 n1, Vector2 n2, float cornerRadius, int segments, ColorRgba color)
    {
        if (segments < 0) return;
    
        // float stepLengthRad = (MathF.PI * 0.5f) / (float)segments;
        
        float startAngRad = n1.AngleRad();
        float endAngRad = n2.AngleRad();
        float shortestAngleRad = ShapeMath.GetShortestAngleRad(startAngRad, endAngRad);
        float stepLengthRad = shortestAngleRad / (float)segments;
        float angSign = -MathF.Sign(shortestAngleRad);
        var rayColor = color.ToRayColor();
        for (var i = 0; i <= segments - 1; i++) // inclusive to include corner endpoints
        {
            float angRad = startAngRad + i * stepLengthRad * angSign;
            float angRadNext = startAngRad + (i + 1) * stepLengthRad * angSign;
            var dir = ShapeVec.Right().Rotate(angRad);
            var dirNext = ShapeVec.Right().Rotate(angRadNext);
    
            var prevPoint = cornerCenter + dir * cornerRadius;
            var nextPoint = cornerCenter + dirNext * cornerRadius;
            Raylib.DrawTriangle(nextPoint, cornerCenter, prevPoint, rayColor);
        }
    }
    
    //TODO: Test
    private static void DrawQuadLinesPercentageHelper(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, float percentage, float lineThickness, ColorRgba color, float roundness = 0, int cornerPoints = 0)
    {
        if (percentage == 0f || lineThickness <= 0) return;
        var order = GetDrawLinePercentageOrder(p1, p2, p3, p4, percentage);
        if(order.p <= 0f) return;
        if(order.p >= 1f)
        {
            DrawQuadLinesInternal(p1, p2, p3, p4, lineThickness, color, roundness, cornerPoints);
            return;
        }
        
        p1 = order.a;
        p2 = order.b;
        p3 = order.c;
        p4 = order.d;
        percentage = order.p;
        bool ccw = order.ccw;
        var edge1 = p2 - p1;
        var edge4 = p1 - p4;
        float size1 = edge1.Length();
        float size2 = edge4.Length();

        var rayColor = color.ToRayColor();
        
        if(cornerPoints <= 0 || roundness <= 0f)
        {
            float offsetDistance = MathF.Sqrt(2f * lineThickness * lineThickness);
            float totalPerimeter = (size1 + size2) * 2f;
            float perimeter = totalPerimeter * percentage;
            float perimeterRemaining = perimeter;
            
            var edge2 = p3 - p2;
            var edge3 = p4 - p3;
            var n1 = edge1.Normalize();
            var n2 = edge2.Normalize();
            var n3 = edge3.Normalize();
            var n4 = edge4.Normalize();
            
            var dir1 = (n1 - n4).Normalize();
            var dir2 = (n2 - n1).Normalize();
            var dir3 = (n3 - n2).Normalize();
            var dir4 = (n4 - n3).Normalize();

            var curPoint = p1;
            var curInner = curPoint + dir1 * offsetDistance;
            var curOuter = curPoint- dir1 * offsetDistance;

            var nextPoint = p2;
            var nextInner= nextPoint + dir2 * offsetDistance;
            var nextOuter = nextPoint - dir2 * offsetDistance;

            if (perimeterRemaining >= size1)
            {
                perimeterRemaining -= size1;
                if (ccw)
                {
                    Raylib.DrawTriangle(curOuter, nextInner, curInner, rayColor);
                    Raylib.DrawTriangle(curOuter, nextOuter, nextInner, rayColor);
                }
                else
                {
                    Raylib.DrawTriangle(nextInner, curOuter, curInner, rayColor);
                    Raylib.DrawTriangle(nextOuter, curOuter, nextInner, rayColor);
                }
            }
            else
            {
                float f = perimeterRemaining / size1;
                var innerEnd = Vector2.Lerp(curInner, nextInner, f);
                var outerEnd = Vector2.Lerp(curOuter, nextOuter, f);
                if (ccw)
                {
                    Raylib.DrawTriangle(curOuter, innerEnd, curInner, rayColor);
                    Raylib.DrawTriangle(curOuter, outerEnd, innerEnd, rayColor);
                }
                else
                {
                    Raylib.DrawTriangle( innerEnd,curOuter, curInner, rayColor);
                    Raylib.DrawTriangle( outerEnd,curOuter, innerEnd, rayColor);
                }
                return;
            }
            
            curInner = nextInner;
            curOuter = nextOuter;
            nextPoint = p3;
            nextInner= nextPoint + dir3 * offsetDistance;
            nextOuter = nextPoint - dir3 * offsetDistance;

            if (perimeterRemaining >= size2)
            {
                perimeterRemaining -= size2;
                if (ccw)
                {
                    Raylib.DrawTriangle(curOuter, nextInner, curInner, rayColor);
                    Raylib.DrawTriangle(curOuter, nextOuter, nextInner, rayColor);
                }
                else
                {
                    Raylib.DrawTriangle( nextInner,curOuter, curInner, rayColor);
                    Raylib.DrawTriangle( nextOuter,curOuter, nextInner, rayColor);
                }
            }
            else
            {
                float f = perimeterRemaining / size2;
                var innerEnd = Vector2.Lerp(curInner, nextInner, f);
                var outerEnd = Vector2.Lerp(curOuter, nextOuter, f);
                if (ccw)
                {
                    Raylib.DrawTriangle(curOuter, innerEnd, curInner, rayColor);
                    Raylib.DrawTriangle(curOuter, outerEnd, innerEnd, rayColor);
                }
                else
                {
                    Raylib.DrawTriangle( innerEnd,curOuter, curInner, rayColor);
                    Raylib.DrawTriangle( outerEnd,curOuter, innerEnd, rayColor);
                }
                
                return;
            }
            
            curInner = nextInner;
            curOuter = nextOuter;
            nextPoint = p4;
            nextInner= nextPoint + dir4 * offsetDistance;
            nextOuter = nextPoint - dir4 * offsetDistance;

            if (perimeterRemaining >= size1)
            {
                perimeterRemaining -= size1;
                if (ccw)
                {
                    Raylib.DrawTriangle(curOuter, nextInner, curInner, rayColor);
                    Raylib.DrawTriangle(curOuter, nextOuter, nextInner, rayColor);
                }
                else
                {
                    Raylib.DrawTriangle( nextInner,curOuter, curInner, rayColor);
                    Raylib.DrawTriangle( nextOuter,curOuter, nextInner, rayColor);
                }
                
            }
            else
            {
                float f = perimeterRemaining / size1;
                var innerEnd = Vector2.Lerp(curInner, nextInner, f);
                var outerEnd = Vector2.Lerp(curOuter, nextOuter, f);
                if (ccw)
                {
                    Raylib.DrawTriangle(curOuter, innerEnd, curInner, rayColor);
                    Raylib.DrawTriangle(curOuter, outerEnd, innerEnd, rayColor);
                }
                else
                {
                    Raylib.DrawTriangle( innerEnd,curOuter, curInner, rayColor);
                    Raylib.DrawTriangle( outerEnd,curOuter, innerEnd, rayColor);
                }
                
                return;
            }
            
            curInner = nextInner;
            curOuter = nextOuter;
            nextPoint = p1;
            nextInner= nextPoint + dir1 * offsetDistance;
            nextOuter = nextPoint - dir1 * offsetDistance;

            if (perimeterRemaining >= size2)
            {
                if (ccw)
                {
                    Raylib.DrawTriangle(curOuter, nextInner, curInner, rayColor);
                    Raylib.DrawTriangle(curOuter, nextOuter, nextInner, rayColor);
                }
                else
                {
                    Raylib.DrawTriangle(nextInner,curOuter,  curInner, rayColor);
                    Raylib.DrawTriangle(nextOuter,curOuter,  nextInner, rayColor);
                }
                
            }
            else
            {
                float f = perimeterRemaining / size2;
                var innerEnd = Vector2.Lerp(curInner, nextInner, f);
                var outerEnd = Vector2.Lerp(curOuter, nextOuter, f);
                if (ccw)
                {
                    Raylib.DrawTriangle(curOuter, innerEnd, curInner, rayColor);
                    Raylib.DrawTriangle(curOuter, outerEnd, innerEnd, rayColor);
                }
                else
                {
                    Raylib.DrawTriangle( innerEnd,curOuter, curInner, rayColor);
                    Raylib.DrawTriangle( outerEnd,curOuter, innerEnd, rayColor);
                }
                
                
            }
        }
        else
        {
        
            float radius = (size1 > size2) ? (size2 * roundness) / 2f : (size1 * roundness) / 2f;
            if (radius <= 0f) return;
        
            if(radius <= lineThickness) radius = lineThickness;

            if(cornerPoints < 2) cornerPoints = 2;
            
            float outerRadius = radius + lineThickness;
            float innerRadius = radius - lineThickness;
            
            float cornerArcLength = MathF.PI * 0.5f * outerRadius;
            float cornerLength = cornerArcLength * 4;
            float l1 = size1 - 2 * radius;
            float l2 = size2 - 2 * radius;
            float totalPerimeter = cornerLength + l1 * 2f + l2 * 2f;
            float perimeter = totalPerimeter * percentage;
            float perimeterRemaining = perimeter;
            
            var edge2 = p3 - p2;
            var edge3 = p4 - p3;
            var n1 = edge1.Normalize();
            var n2 = edge2.Normalize();
            var n3 = edge3.Normalize();
            var n4 = edge4.Normalize();
            
            var dir1 = (n1 - n4).Normalize();
            var dir2 = (n2 - n1).Normalize();
            var dir3 = (n3 - n2).Normalize();
            var dir4 = (n4 - n3).Normalize();
        
            var dis = MathF.Sqrt(radius * radius * 2f);
        
            //draw first half corner
            var curCenter = p1 + dir1 * dis;
            // perimeterRemaining = DrawRoundedCornerFractionHelper(curCenter, -dir1, n4, innerRadius, outerRadius, cornerPoints / 2, color, perimeterRemaining, ccw);
            perimeterRemaining = DrawRoundedCornerFractionHelper(curCenter, n3, n4, innerRadius, outerRadius, cornerPoints, color, perimeterRemaining, ccw);
            if (perimeterRemaining <= 0f) return;
            
            // draw first edge
            var curInner = curCenter + n4 * innerRadius;
            var curOuter = curCenter + n4 * outerRadius;
            
            var nextCenter = p2 + dir2 * dis;
            var nextInner = nextCenter + n4 * innerRadius;
            var nextOuter = nextCenter + n4 * outerRadius;

            if (perimeterRemaining >= l1)
            {
                perimeterRemaining -= l1;
                if (ccw)
                {
                    Raylib.DrawTriangle(curOuter, nextInner, curInner, rayColor);
                    Raylib.DrawTriangle(curOuter, nextOuter, nextInner, rayColor);
                }
                else
                {
                    Raylib.DrawTriangle(nextInner,curOuter,  curInner, rayColor);
                    Raylib.DrawTriangle(nextOuter,curOuter,  nextInner, rayColor);
                }
                
                //Draw edge
            }
            else
            {
                float f = perimeterRemaining / l1;
                var innerEnd = Vector2.Lerp(curInner, nextInner, f);
                var outerEnd = Vector2.Lerp(curOuter, nextOuter, f);
                if (ccw)
                {
                    Raylib.DrawTriangle(curOuter, innerEnd, curInner, rayColor);
                    Raylib.DrawTriangle(curOuter, outerEnd, innerEnd, rayColor);
                }
                else
                {
                    Raylib.DrawTriangle( innerEnd,curOuter, curInner, rayColor);
                    Raylib.DrawTriangle( outerEnd,curOuter, innerEnd, rayColor);
                }
                
                return;
            }
            
            //draw second corner
            curCenter = nextCenter;
            perimeterRemaining = DrawRoundedCornerFractionHelper(curCenter, n4, n1, innerRadius, outerRadius, cornerPoints, color, perimeterRemaining, ccw);
            if (perimeterRemaining <= 0f) return;
            
            // draw second edge
            curInner = curCenter + n1 * innerRadius;
            curOuter = curCenter + n1 * outerRadius;
            
            nextCenter = p3 + dir3 * dis;
            nextInner = nextCenter + n1 * innerRadius;
            nextOuter = nextCenter + n1 * outerRadius;

            if (perimeterRemaining >= l2)
            {
                perimeterRemaining -= l2;
                if (ccw)
                {
                    Raylib.DrawTriangle(curOuter, nextInner, curInner, rayColor);
                    Raylib.DrawTriangle(curOuter, nextOuter, nextInner, rayColor);
                }
                else
                {
                    Raylib.DrawTriangle(nextInner,curOuter,  curInner, rayColor);
                    Raylib.DrawTriangle(nextOuter,curOuter,  nextInner, rayColor);
                }
                
                //Draw edge
            }
            else
            {
                float f = perimeterRemaining / l2;
                var innerEnd = Vector2.Lerp(curInner, nextInner, f);
                var outerEnd = Vector2.Lerp(curOuter, nextOuter, f);
                if (ccw)
                {
                    Raylib.DrawTriangle(curOuter, innerEnd, curInner, rayColor);
                    Raylib.DrawTriangle(curOuter, outerEnd, innerEnd, rayColor);
                }
                else
                {
                    Raylib.DrawTriangle(innerEnd,curOuter, curInner, rayColor);
                    Raylib.DrawTriangle(outerEnd,curOuter, innerEnd, rayColor);
                }
                
                return;
            }
            //draw third corner
            curCenter = nextCenter;
            perimeterRemaining = DrawRoundedCornerFractionHelper(curCenter, n1, n2, innerRadius, outerRadius, cornerPoints, color, perimeterRemaining, ccw);
            if (perimeterRemaining <= 0f) return;
            
            // draw third edge
            curInner = curCenter + n2 * innerRadius;
            curOuter = curCenter + n2 * outerRadius;
            
            nextCenter = p4 + dir4 * dis;
            nextInner = nextCenter + n2 * innerRadius;
            nextOuter = nextCenter + n2 * outerRadius;

            if (perimeterRemaining >= l1)
            {
                perimeterRemaining -= l1;
                if (ccw)
                {
                    Raylib.DrawTriangle(curOuter, nextInner, curInner, rayColor);
                    Raylib.DrawTriangle(curOuter, nextOuter, nextInner, rayColor);
                }
                else
                {
                    Raylib.DrawTriangle(nextInner,curOuter, curInner, rayColor);
                    Raylib.DrawTriangle(nextOuter,curOuter, nextInner, rayColor);
                }
            }
            else
            {
                float f = perimeterRemaining / l1;
                var innerEnd = Vector2.Lerp(curInner, nextInner, f);
                var outerEnd = Vector2.Lerp(curOuter, nextOuter, f);
                if (ccw)
                {
                    Raylib.DrawTriangle(curOuter, innerEnd, curInner, rayColor);
                    Raylib.DrawTriangle(curOuter, outerEnd, innerEnd, rayColor);
                }
                else
                {
                    Raylib.DrawTriangle(innerEnd,curOuter, curInner, rayColor);
                    Raylib.DrawTriangle(outerEnd,curOuter, innerEnd, rayColor);
                }
                
                return;
            }
            //draw fourth corner
            curCenter = nextCenter;
            perimeterRemaining = DrawRoundedCornerFractionHelper(curCenter, n2, n3, innerRadius, outerRadius, cornerPoints, color, perimeterRemaining, ccw);
            if (perimeterRemaining <= 0f) return;
            
            // draw fourth edge
            curInner = curCenter + n3 * innerRadius;
            curOuter = curCenter + n3 * outerRadius;
            
            nextCenter = p1 + dir1 * dis;
            nextInner = nextCenter + n3 * innerRadius;
            nextOuter = nextCenter + n3 * outerRadius;

            if (perimeterRemaining >= l2)
            {
                perimeterRemaining -= l2;
                if (ccw)
                {
                    Raylib.DrawTriangle(curOuter, nextInner, curInner, rayColor);
                    Raylib.DrawTriangle(curOuter, nextOuter, nextInner, rayColor);
                }
                else
                {
                    Raylib.DrawTriangle( nextInner,curOuter, curInner, rayColor);
                    Raylib.DrawTriangle( nextOuter,curOuter, nextInner, rayColor);
                }
                
            }
            else
            {
                float f = perimeterRemaining / l2;
                var innerEnd = Vector2.Lerp(curInner, nextInner, f);
                var outerEnd = Vector2.Lerp(curOuter, nextOuter, f);
                if (ccw)
                {
                    Raylib.DrawTriangle(curOuter, innerEnd, curInner, rayColor);
                    Raylib.DrawTriangle(curOuter, outerEnd, innerEnd, rayColor);
                }
                else
                {
                    Raylib.DrawTriangle(innerEnd,curOuter, curInner, rayColor);
                    Raylib.DrawTriangle(outerEnd,curOuter, innerEnd, rayColor);
                }
                
                // return;
            }
            
            //Draw second half of first corner
            // curCenter = nextCenter;
            // DrawRoundedCornerFractionHelper(curCenter, n3, -dir1, innerRadius, outerRadius, cornerPoints / 2, color, perimeterRemaining, ccw);
        }
    }
    private static float DrawRoundedCornerFractionHelper(Vector2 cornerCenter, Vector2 n1, Vector2 n2, float innerRadius, float outerRadius, int segments, ColorRgba color, float remainingLength, bool ccw)
    {
        if (segments < 0 || remainingLength <= 0) return 0f;
    
        float angle = MathF.Acos(ShapeMath.Clamp(Vector2.Dot(n1, n2), -1f, 1f));
        float arcLength = angle * outerRadius;
        
        float startAngRad = n1.AngleRad();
        float endAngRad = n2.AngleRad();
        float shortestAngleRad = ShapeMath.GetShortestAngleRad(startAngRad, endAngRad);
        float stepLengthRad = shortestAngleRad / (float)segments;
        float stepArcLength = arcLength / (float)segments;
        float angSign = MathF.Sign(shortestAngleRad);
        if (ccw) angSign *= -1f;
        float curAngleRad = startAngRad;
        
        float lengthToDraw = MathF.Min(arcLength, remainingLength);
        var rayColor = color.ToRayColor();
        
        // cornerCenter.Draw(4f, ColorRgba.CreateKnowColor(KnownColor.DeepSkyBlue));
        // var s1 = new Segment(cornerCenter, cornerCenter + n1 * outerRadius);
        // var s2 = new Segment(cornerCenter, cornerCenter + n2 * outerRadius);
        // s1.Draw(2f, ColorRgba.CreateKnowColor(KnownColor.Yellow));
        // s2.Draw(2f, ColorRgba.CreateKnowColor(KnownColor.Orange));
        
        while (lengthToDraw > 0)
        {
            float curStepLengthRad = stepLengthRad;

            if (lengthToDraw < stepArcLength)
            {
                float f = lengthToDraw / stepArcLength;
                remainingLength -= lengthToDraw;
                lengthToDraw = 0;
                curStepLengthRad = stepLengthRad * f;
            }
            else
            {
                remainingLength -= stepArcLength;
                lengthToDraw -= stepArcLength;
            }
            
            float angRadNext = curAngleRad + curStepLengthRad * angSign;
            var curDir = ShapeVec.Right().Rotate(curAngleRad);
            var dirNext = ShapeVec.Right().Rotate(angRadNext);
            
            var curInnerPoint = cornerCenter + curDir * innerRadius;
            var curOuterPoint = cornerCenter + curDir * outerRadius;
            var nextInnerPoint = cornerCenter + dirNext * innerRadius;
            var nextOuterPoint = cornerCenter + dirNext * outerRadius;
            // curInnerPoint.Draw(4f, ColorRgba.CreateKnowColor(KnownColor.Red));
            // curOuterPoint.Draw(4f, ColorRgba.CreateKnowColor(KnownColor.Red));
            // nextInnerPoint.Draw(4f, ColorRgba.CreateKnowColor(KnownColor.Green));
            // nextOuterPoint.Draw(4f, ColorRgba.CreateKnowColor(KnownColor.Green));
            if (ccw)
            {
                Raylib.DrawTriangle(curOuterPoint, nextInnerPoint, curInnerPoint,  rayColor);
                Raylib.DrawTriangle(nextOuterPoint, nextInnerPoint, curOuterPoint, rayColor);
            }
            else
            {
                Raylib.DrawTriangle( nextInnerPoint,curOuterPoint,  curInnerPoint,  rayColor);
                Raylib.DrawTriangle( nextInnerPoint,nextOuterPoint, curOuterPoint, rayColor);
            }
            

            curAngleRad = angRadNext;
        }

        return remainingLength;
    }
    private static (Vector2 a, Vector2 b, Vector2 c, Vector2 d, float p, bool ccw) GetDrawLinePercentageOrder(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float percentage)
    {
        if (percentage == 0f) return (a, b, c, d, 0f, true);
        bool ccw = percentage > 0;
        float absPercentage = ShapeMath.Clamp(MathF.Abs(percentage), 0f, 4f);
        var corner = (int)absPercentage;
        float perc = absPercentage - corner;

        if (perc <= 0f)
        {
            perc = 1f;
            corner -= 1;
        }
        
        
        if (corner == 0)
        {
            return ccw ? (a, b, c, d, perc, ccw) : (a, d, c, b, perc, ccw);
        }
        if (corner == 1)
        {
            return ccw ? (b, c, d, a, perc, ccw) : (b, a, d, c, perc, ccw);
        }
        if (corner == 2)
        {
            return ccw ? (c, d, a, b, perc, ccw) : (c, b, a, d, perc, ccw);
        }
        
        return ccw ? (d, a, b, c, perc, ccw) : (d, c, b, a, perc, ccw);
    }
    
    private static void DrawCorner(Vector2 p, Vector2 n1, Vector2 n2, float cornerLength1, float cornerLength2, float thickness, float miterLength, ColorRgba color, LineCapType capType = LineCapType.None, int capPoints = 0)
    {
        if (capType == LineCapType.Extended || (capType is LineCapType.Capped or LineCapType.CappedExtended && capPoints > 0))
        {
            cornerLength1 = MathF.Max(thickness, cornerLength1);
            cornerLength2 = MathF.Max(thickness, cornerLength2);
        }
        var miterDir = (n1 + n2).Normalize();
        var maxMiterLength = MathF.Sqrt(cornerLength1 * cornerLength1 + cornerLength2 * cornerLength2);
        miterLength = MathF.Min(miterLength, maxMiterLength);
        
        var innerMiter = p - miterDir * miterLength;
        var end1 = p - n1 * cornerLength1;
        var end2 = p - n2 * cornerLength2;
        if (capType == LineCapType.Extended)
        {
            end1 -= n1 * thickness;
            end2 -= n2 * thickness;
        }
        else if (capType == LineCapType.Capped && capPoints > 0)
        {
            end1 += n1 * thickness;
            end2 += n2 * thickness;
        }
        
        var end1Inner = end1 - n2 * thickness;
        var end2Inner = end2 - n1 * thickness;
        
        var outerMiter = p + miterDir * miterLength;
        var end1Outer = end1 + n2 * thickness;
        var end2Outer = end2 + n1 * thickness;
        TriangleDrawing.DrawTriangle(outerMiter, end1Outer, innerMiter, color);
        TriangleDrawing.DrawTriangle(end1Outer, end1Inner, innerMiter, color);
        TriangleDrawing.DrawTriangle(outerMiter, innerMiter, end2Outer, color); 
        TriangleDrawing.DrawTriangle(innerMiter, end2Inner, end2Outer, color);

        if (capType is LineCapType.Capped or LineCapType.CappedExtended && capPoints > 0)
        {
            SegmentDrawing.DrawRoundCap(end1, -n1, thickness, capPoints, color);
            SegmentDrawing.DrawRoundCap(end2, -n2, thickness, capPoints, color);
        }
    }
    #endregion
}