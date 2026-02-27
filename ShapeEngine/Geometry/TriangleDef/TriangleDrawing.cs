using System.Numerics;
using System.Runtime.Intrinsics.X86;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.StaticLib;
using Ray = ShapeEngine.Geometry.RayDef.Ray;

namespace ShapeEngine.Geometry.TriangleDef;


/// <summary>
/// Provides static methods for drawing triangles and collections of triangles with various styles and options.
/// </summary>
/// <remarks>
/// This class contains extension methods for drawing <see cref="Triangle"/> objects,
/// as well as static methods for drawing triangles using raw vertex data. Supports filled, outlined, partial, and scaled outlines.
/// </remarks>
public static class TriangleDrawing
{
    #region Draw
    
    /// <summary>
    /// Draws a filled triangle using the specified vertices and color.
    /// </summary>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <param name="color">The color to fill the triangle with.</param>
    public static void DrawTriangle(Vector2 a, Vector2 b, Vector2 c, ColorRgba color)
    {
        Raylib.DrawTriangle(a, b, c, color.ToRayColor());
    }

    /// <summary>
    /// Draws a filled triangle using the specified <see cref="Triangle"/> and color.
    /// </summary>
    /// <param name="t">The triangle to draw.</param>
    /// <param name="color">The color to fill the triangle with.</param>
    public static void Draw(this Triangle t, ColorRgba color)
    {
        Raylib.DrawTriangle(t.A, t.B, t.C, color.ToRayColor());
    }
    
    #endregion

    #region Draw Scaled

    /// <summary>
    /// Draws a triangle with scaled sides based on a specific draw type.
    /// </summary>
    /// <param name="t">The triangle to draw.</param>
    /// <param name="color">The color of the drawn shape.</param>
    /// <param name="sideScaleFactor">The scale factor of the sides (0 to 1). If >= 1, the full triangle is drawn. If &lt;= 0, nothing is drawn.</param>
    /// <param name="sideScaleOrigin">The origin point for scaling the sides (0 = start, 1 = end, 0.5 = center).</param>
    /// <param name="drawType">
    /// The style of drawing:
    /// <list type="bullet">
    /// <item><description>0: [Filled] Drawn as 4 filled triangles, effectivly cutting of corners.</description></item>
    /// <item><description>1: [Sides] Each side is connected to the triangle's centroid.</description></item>
    /// <item><description>2: [Sides Inverse] The start of 1 side is connected to the end of the next side and is connected to the triangle's centroid.</description></item>
    /// </list>
    /// </param>
    public static void DrawScaled(this Triangle t, ColorRgba color, float sideScaleFactor, float sideScaleOrigin, int drawType)
    {
        if (sideScaleFactor <= 0) return;
        if (sideScaleFactor >= 1)
        {
            t.Draw(color);
            return;
        }

        var s1 = new Segment(t.A, t.B).ScaleSegment(sideScaleFactor, sideScaleOrigin);
        var s2 = new Segment(t.B, t.C).ScaleSegment(sideScaleFactor, sideScaleOrigin);
        var s3 = new Segment(t.C, t.A).ScaleSegment(sideScaleFactor, sideScaleOrigin);

        var rayColor = color.ToRayColor();
        if (drawType == 0)
        {
            Raylib.DrawTriangle(s1.Start, s1.End, s2.Start, rayColor);
            Raylib.DrawTriangle(s1.Start, s2.Start, s3.End, rayColor);
            Raylib.DrawTriangle(s3.End, s2.Start, s2.End, rayColor);
            Raylib.DrawTriangle(s3.End, s2.End, s3.Start, rayColor);
        }
        else if (drawType == 1)
        {
            var center = t.GetCentroid();
            Raylib.DrawTriangle(s1.Start, s1.End, center, rayColor);
            Raylib.DrawTriangle(s2.Start, s2.End, center, rayColor);
            Raylib.DrawTriangle(s3.Start, s3.End, center, rayColor);
        }
        else
        {
            var center = t.GetCentroid();
            Raylib.DrawTriangle(s3.End, s1.Start, center, rayColor);
            Raylib.DrawTriangle(s1.End, s2.Start, center, rayColor);
            Raylib.DrawTriangle(s2.End, s3.Start, center, rayColor);
        }
    }
    
    #endregion
    
    #region Draw Lines

    /// <summary>
    /// Draws the outline of the triangle with specified line thickness and color.
    /// </summary>
    /// <param name="t">The triangle to draw.</param>
    /// <param name="lineThickness">The thickness of the outline lines.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="miterLimit">The limit for miter joins to prevent sharp spikes. Defaults to 4f.</param>
    /// <param name="beveled">If true, uses beveled joins when the miter limit is exceeded; otherwise, cuts off the miter point.</param>
    public static void DrawLines(this Triangle t, float lineThickness, ColorRgba color, float miterLimit = 4f, bool beveled = true)
    {
        DrawLinesHelper(t, lineThickness, color, miterLimit, beveled);
    }
    
    /// <summary>
    /// Draws the outline of the triangle using <see cref="LineDrawingInfo"/>.
    /// </summary>
    /// <param name="t">The triangle to draw.</param>
    /// <param name="lineInfo">Contains line style information such as thickness and color.</param>
    /// <param name="miterLimit">The limit for miter joins to prevent sharp spikes. Defaults to 4f.</param>
    /// <param name="beveled">If true, uses beveled joins when the miter limit is exceeded; otherwise, cuts off the miter point.</param>
    public static void DrawLines(this Triangle t, LineDrawingInfo lineInfo, float miterLimit = 4f, bool beveled = true)
    {
        DrawLinesHelper(t, lineInfo.Thickness, lineInfo.Color, miterLimit, beveled);
    }

    #endregion
    
    #region Draw Lines Percentage
    
    /// <summary>
    /// Draws a partial outline of the triangle based on a percentage coverage.
    /// </summary>
    /// <param name="t">The triangle to draw.</param>
    /// <param name="f">The percentage of the outline to draw (0 to 1). Negative values reverse the drawing direction.</param>
    /// <param name="startCorner">The index of the corner to start drawing from (0, 1, or 2).</param>
    /// <param name="lineInfo">Contains line style information such as thickness and color.</param>
    /// <param name="miterLimit">The limit for miter joins to prevent sharp spikes. Defaults to 4f.</param>
    /// <param name="beveled">If true, uses beveled joins when the miter limit is exceeded; otherwise, cuts off the miter point.</param>
    public static void DrawLinesPercentage(this Triangle t, float f, int startCorner, LineDrawingInfo lineInfo, float miterLimit = 4f, bool beveled = true)
    {
        t.DrawLinesPercentage(f, startCorner, lineInfo.Thickness, lineInfo.Color, miterLimit, beveled);
    }
    
    /// <summary>
    /// Draws a partial outline of the triangle based on a percentage coverage.
    /// </summary>
    /// <param name="t">The triangle to draw.</param>
    /// <param name="f">The percentage of the outline to draw (0 to 1). Negative values reverse the drawing direction.</param>
    /// <param name="startCorner">The index of the corner to start drawing from (0, 1, or 2).</param>
    /// <param name="lineThickness">The thickness of the outline lines.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="miterLimit">The limit for miter joins to prevent sharp spikes. Defaults to 4f.</param>
    /// <param name="beveled">If true, uses beveled joins when the miter limit is exceeded; otherwise, cuts off the miter point.</param>
    public static void DrawLinesPercentage(this Triangle t, float f, int startCorner, float lineThickness, ColorRgba color, float miterLimit = 4f, bool beveled = true)
    {
        bool cw = false;
        if (f < 0)
        {
            cw = true;
            f *= -1;
        }

        f = ShapeMath.Clamp(0f, 1f, f);

        if (f <= 0) return;
        if (f >= 1)
        {
            t.DrawLines(lineThickness, color, miterLimit, beveled);
            return;
        }

        startCorner = ShapeMath.WrapI(startCorner, 0, 3);
        
        Triangle triangle;
        
        if (startCorner == 0)
        {
            triangle = cw ? new(t.A, t.C, t.B) : new(t.A, t.B, t.C);
        }
        else if (startCorner == 1)
        {
            triangle = cw ? new(t.C, t.B, t.A) : new(t.B, t.C, t.A);
        }
        else
        {
            triangle = cw ? new(t.B, t.A, t.C) : new(t.C, t.A, t.B);
        }
        
        DrawLinesPercentageHelper(triangle, f, !cw, lineThickness, color, miterLimit, beveled);
    }
    
    #endregion
    
    #region Draw Lines Scaled

    /// <summary>
    /// Draws a triangle outline where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="t">The triangle to draw.</param>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <param name="sideScaleFactor">The scale factor for each side <c>(0 = No Side, 1 = Full Side).</c></param>
    /// <param name="sideScaleOrigin">The point along each side to scale from in both directions <c>(0 = Start, 1 = End)</c>.</param>
    /// <remarks>
    /// Allows for dynamic scaling of triangle sides, useful for effects or partial outlines.
    /// </remarks>
    public static void DrawLinesScaled(this Triangle t, LineDrawingInfo lineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0) return;
        if (sideScaleFactor >= 1)
        {
            t.DrawLines(lineInfo);
            return;
        }
        
        SegmentDrawing.DrawSegment(t.A, t.B, lineInfo, sideScaleFactor, sideScaleOrigin);
        SegmentDrawing.DrawSegment(t.B, t.C, lineInfo, sideScaleFactor, sideScaleOrigin);
        SegmentDrawing.DrawSegment(t.C, t.A, lineInfo, sideScaleFactor, sideScaleOrigin);
    }
    
    #endregion
    
    #region Draw Corners
    
    /// <summary>
    /// Draws the corners of the triangle with a specific length along the edges.
    /// </summary>
    /// <param name="triangle">The triangle to draw the corners for.</param>
    /// <param name="lineThickness">The thickness of the corner lines.</param>
    /// <param name="color">The color of the corners.</param>
    /// <param name="cornerLength">The absolute length of the corner segments along the edges from each vertex.</param>
    /// <param name="miterLimit">The limit for miter joins to prevent sharp spikes. Defaults to 4f.</param>
    /// <param name="beveled">If true, uses beveled joins when the miter limit is exceeded; otherwise, cuts off the miter point.</param>
    public static void DrawCorners(this Triangle triangle, float lineThickness, ColorRgba color, float cornerLength, float miterLimit = 4f, bool beveled = true)
    {
        if(triangle.IsCollinear()) return;
        
        if(cornerLength <= 0f) return;
        
        var edgeAB = triangle.B - triangle.A;
        var edgeBC = triangle.C - triangle.B;
        var edgeCA = triangle.A - triangle.C;
        
        var lengthAB = edgeAB.Length();
        if (lengthAB <= 0f) return;
        
        var lengthBC = edgeBC.Length();
        if (lengthBC <= 0f) return;
        
        var lengthCA = edgeCA.Length();
        if (lengthCA <= 0f) return;
        
        var minLength = MathF.Min(lengthAB, MathF.Min(lengthBC, lengthCA));
        if (minLength <= cornerLength)
        {
            triangle.DrawLines(lineThickness, color, miterLimit, beveled);
            return;
        }
        var cornerLengthFactor = ShapeMath.Clamp(0f, 1f, cornerLength / minLength);
        
        float totalMiterLengthLimit = lineThickness * 0.5f * MathF.Max(2f, miterLimit);
        
        float perimeter = lengthAB + lengthBC + lengthCA;
        Vector2 incenter = (triangle.A * lengthBC + triangle.B * lengthCA + triangle.C * lengthAB) / perimeter;
        
        var innerMaxLengthA = (incenter - triangle.A).Length();
        if(innerMaxLengthA <= 0f) return;
        
        var innerMaxLengthB = (incenter - triangle.B).Length();
        if(innerMaxLengthB <= 0f) return;
        
        var innerMaxLengthC = (incenter - triangle.C).Length();
        if(innerMaxLengthC <= 0f) return;
        
        var edgeABDir = edgeAB / lengthAB;
        var edgeBCDir = edgeBC / lengthBC;
        var edgeCADir = edgeCA / lengthCA;
        
        var normalAB = edgeABDir.GetPerpendicularRight();
        var normalBC = edgeBCDir.GetPerpendicularRight();
        var normalCA = edgeCADir.GetPerpendicularRight();
        
        var miterDirA = (normalCA + normalAB).Normalize();
        var miterDirB = (normalAB + normalBC).Normalize();
        var miterDirC = (normalBC + normalCA).Normalize();
        
        float miterAngleRadA = MathF.Abs(miterDirA.AngleRad(normalAB));
        float miterLengthA = lineThickness / MathF.Cos(miterAngleRadA);
        
        float miterAngleRadB = MathF.Abs(miterDirB.AngleRad(normalBC));
        float miterLengthB = lineThickness / MathF.Cos(miterAngleRadB);
        
        float miterAngleRadC = MathF.Abs(miterDirC.AngleRad(normalCA));
        float miterLengthC = lineThickness / MathF.Cos(miterAngleRadC);
        
        Vector2 aOuterPrev, aOuterNext;
        Vector2 bOuterPrev, bOuterNext;
        Vector2 cOuterPrev, cOuterNext;
        
        Vector2 aInner = triangle.A - miterDirA * MathF.Min(miterLengthA, innerMaxLengthA);
        Vector2 bInner = triangle.B - miterDirB * MathF.Min(miterLengthB, innerMaxLengthB);
        Vector2 cInner = triangle.C - miterDirC * MathF.Min(miterLengthC, innerMaxLengthC);
        
        var rayColor = color.ToRayColor();
        
        if (miterLimit < 2f || miterLengthA < totalMiterLengthLimit)
        {
            aOuterPrev = triangle.A + miterDirA * miterLengthA;
            aOuterNext = aOuterPrev;
        }
        else
        {
            if (beveled)
            {
                aOuterPrev = triangle.A + normalCA * lineThickness;
                aOuterNext = triangle.A + normalAB * lineThickness;
            }
            else
            {
                var p = triangle.A + miterDirA * totalMiterLengthLimit;
                var dir = (p - triangle.A).Normalize();
                var pr = dir.GetPerpendicularRight();
                var offsetC = triangle.C + normalCA * lineThickness;
        
                var ip = Ray.IntersectRayRay(p, pr, offsetC, edgeCADir);
                if (ip.Valid)
                {
                    aOuterPrev = ip.Point;
                    aOuterNext = p - pr * (p - ip.Point).Length();
                }
                else
                {
                    aOuterPrev = triangle.A + normalCA * lineThickness;
                    aOuterNext = triangle.A + normalAB * lineThickness;
                }
            }
            Raylib.DrawTriangle(aOuterPrev, aOuterNext, aInner, rayColor);
        }
        
        if (miterLimit < 2f || miterLengthB < totalMiterLengthLimit)
        {
            bOuterPrev = triangle.B + miterDirB * miterLengthB;
            bOuterNext = bOuterPrev;
        }
        else
        {
            if (beveled)
            {
                bOuterPrev = triangle.B + normalAB * lineThickness;
                bOuterNext = triangle.B + normalBC * lineThickness;
            }
            else
            {
                var p = triangle.B + miterDirB * totalMiterLengthLimit;
                var dir = (p - triangle.B).Normalize();
                var pr = dir.GetPerpendicularRight();
                var offsetA = triangle.A + normalAB * lineThickness;
        
                var ip = Ray.IntersectRayRay(p, pr, offsetA, edgeABDir);
                if (ip.Valid)
                {
                    bOuterPrev = ip.Point;
                    bOuterNext = p - pr * (p - ip.Point).Length();
                }
                else
                {
                    bOuterPrev = triangle.B + normalAB * lineThickness;
                    bOuterNext = triangle.B + normalBC * lineThickness;
                }
            }
            Raylib.DrawTriangle(bOuterPrev, bOuterNext, bInner, rayColor);
        }
        
        if (miterLimit < 2f || miterLengthC < totalMiterLengthLimit)
        {
            cOuterPrev = triangle.C + miterDirC * miterLengthC;
            cOuterNext = cOuterPrev;
        }
        else
        {
            if (beveled)
            {
                cOuterPrev = triangle.C + normalBC * lineThickness;
                cOuterNext = triangle.C + normalCA * lineThickness;
            }
            else
            {
                var p = triangle.C + miterDirC * totalMiterLengthLimit;
                var dir = (p - triangle.C).Normalize();
                var pr = dir.GetPerpendicularRight();
                var offsetB = triangle.B + normalBC * lineThickness;
        
                var ip = Ray.IntersectRayRay(p, pr, offsetB, edgeBCDir);
                if (ip.Valid)
                {
                    cOuterPrev = ip.Point;
                    cOuterNext = p - pr * (p - ip.Point).Length();
                }
                else
                {
                    cOuterPrev = triangle.C + normalBC * lineThickness;
                    cOuterNext = triangle.C + normalCA * lineThickness;
                }
            }
            Raylib.DrawTriangle(cOuterPrev, cOuterNext, cInner, rayColor);
        }
        
        Vector2 sideACenter = triangle.A + edgeABDir * lengthAB * 0.5f;
        Vector2 sideBCenter = triangle.B + edgeBCDir * lengthBC * 0.5f;
        Vector2 sideCCenter = triangle.C + edgeCADir * lengthCA * 0.5f;
        
        Vector2 sideACenterOutside = sideACenter + normalAB * lineThickness;
        Vector2 sideACenterInside = sideACenter - normalAB * lineThickness;
        
        Vector2 sideBCenterOutside = sideBCenter + normalBC * lineThickness;
        Vector2 sideBCenterInside = sideBCenter - normalBC * lineThickness;
        
        Vector2 sideCCenterOutside = sideCCenter + normalCA * lineThickness;
        Vector2 sideCCenterInside = sideCCenter - normalCA * lineThickness;
        
        var aInnerNextEnd = aInner.Lerp(sideACenterInside, cornerLengthFactor);
        var aInnerPrevEnd = aInner.Lerp(sideCCenterInside, cornerLengthFactor);
        var aOuterNextEnd = aOuterNext.Lerp(sideACenterOutside, cornerLengthFactor);
        var aOuterPrevEnd = aOuterPrev.Lerp(sideCCenterOutside, cornerLengthFactor);
        
        var bInnerNextEnd = bInner.Lerp(sideBCenterInside, cornerLengthFactor);
        var bInnerPrevEnd = bInner.Lerp(sideACenterInside, cornerLengthFactor);
        var bOuterNextEnd = bOuterNext.Lerp(sideBCenterOutside, cornerLengthFactor);
        var bOuterPrevEnd = bOuterPrev.Lerp(sideACenterOutside, cornerLengthFactor);
        
        var cInnerNextEnd = cInner.Lerp(sideCCenterInside, cornerLengthFactor);
        var cInnerPrevEnd = cInner.Lerp(sideBCenterInside, cornerLengthFactor);
        var cOuterNextEnd = cOuterNext.Lerp(sideCCenterOutside, cornerLengthFactor);
        var cOuterPrevEnd = cOuterPrev.Lerp(sideBCenterOutside, cornerLengthFactor);
        // var aInnerNextEnd = aInner.LerpByLength(sideACenterInside, cornerLength);
        // var aInnerPrevEnd = aInner.LerpByLength(sideCCenterInside, cornerLength);
        // var aOuterNextEnd = aOuterNext.LerpByLength(sideACenterOutside, cornerLength);
        // var aOuterPrevEnd = aOuterPrev.LerpByLength(sideCCenterOutside, cornerLength);
        //
        // var bInnerNextEnd = bInner.LerpByLength(sideBCenterInside, cornerLength);
        // var bInnerPrevEnd = bInner.LerpByLength(sideACenterInside, cornerLength);
        // var bOuterNextEnd = bOuterNext.LerpByLength(sideBCenterOutside, cornerLength);
        // var bOuterPrevEnd = bOuterPrev.LerpByLength(sideACenterOutside, cornerLength);
        //
        // var cInnerNextEnd = cInner.LerpByLength(sideCCenterInside, cornerLength);
        // var cInnerPrevEnd = cInner.LerpByLength(sideBCenterInside, cornerLength);
        // var cOuterNextEnd = cOuterNext.LerpByLength(sideCCenterOutside, cornerLength);
        // var cOuterPrevEnd = cOuterPrev.LerpByLength(sideBCenterOutside, cornerLength);
        
        Raylib.DrawTriangle(aOuterPrev, aInner, aOuterPrevEnd, rayColor);
        Raylib.DrawTriangle(aInner, aInnerPrevEnd, aOuterPrevEnd, rayColor);
        
        Raylib.DrawTriangle(aOuterNext, aOuterNextEnd, aInner, rayColor);
        Raylib.DrawTriangle(aOuterNextEnd, aInnerNextEnd, aInner, rayColor);
        
        Raylib.DrawTriangle(bOuterPrev, bInner, bOuterPrevEnd, rayColor);
        Raylib.DrawTriangle(bInner, bInnerPrevEnd, bOuterPrevEnd, rayColor);
        
        Raylib.DrawTriangle(bOuterNext, bOuterNextEnd, bInner, rayColor);
        Raylib.DrawTriangle(bOuterNextEnd, bInnerNextEnd, bInner, rayColor);
        
        Raylib.DrawTriangle(cOuterPrev, cInner, cOuterPrevEnd, rayColor);
        Raylib.DrawTriangle(cInner, cInnerPrevEnd, cOuterPrevEnd, rayColor);
        
        Raylib.DrawTriangle(cOuterNext, cOuterNextEnd, cInner, rayColor);
        Raylib.DrawTriangle(cOuterNextEnd, cInnerNextEnd, cInner, rayColor);
    }
    
    #endregion
    
    #region Draw Corners Relative
    
    /// <summary>
    /// Draws the corners of the triangle where the length of the corner segments is relative to the shortest edge.
    /// </summary>
    /// <param name="triangle">The triangle to draw the corners for.</param>
    /// <param name="lineThickness">The thickness of the corner lines.</param>
    /// <param name="color">The color of the corners.</param>
    /// <param name="cornerLengthFactor">The length factor of the corner segments relative to the shortest edge (0 to 0.5 recommended).</param>
    /// <param name="miterLimit">The limit for miter joins to prevent sharp spikes. Defaults to 4f.</param>
    /// <param name="beveled">If true, uses beveled joins when the miter limit is exceeded; otherwise, cuts off the miter point.</param>
    public static void DrawCornersRelative(this Triangle triangle, float lineThickness, ColorRgba color, float cornerLengthFactor, float miterLimit = 4f, bool beveled = true)
    {
        if(triangle.IsCollinear()) return;

        if(cornerLengthFactor <= 0f) return;
        
        if(cornerLengthFactor >= 1f)
        {
            triangle.DrawLines(lineThickness, color, miterLimit, beveled);
            return;
        }
        
        float totalMiterLengthLimit = lineThickness * 0.5f * MathF.Max(2f, miterLimit);
        
        var edgeAB = triangle.B - triangle.A;
        var edgeBC = triangle.C - triangle.B;
        var edgeCA = triangle.A - triangle.C;
        
        var lengthAB = edgeAB.Length();
        if (lengthAB <= 0f) return;
        
        var lengthBC = edgeBC.Length();
        if (lengthBC <= 0f) return;
        
        var lengthCA = edgeCA.Length();
        if (lengthCA <= 0f) return;

        float perimeter = lengthAB + lengthBC + lengthCA;
        Vector2 incenter = (triangle.A * lengthBC + triangle.B * lengthCA + triangle.C * lengthAB) / perimeter;
        
        var innerMaxLengthA = (incenter - triangle.A).Length();
        if(innerMaxLengthA <= 0f) return;
        
        var innerMaxLengthB = (incenter - triangle.B).Length();
        if(innerMaxLengthB <= 0f) return;
        
        var innerMaxLengthC = (incenter - triangle.C).Length();
        if(innerMaxLengthC <= 0f) return;
        
        var edgeABDir = edgeAB / lengthAB;
        var edgeBCDir = edgeBC / lengthBC;
        var edgeCADir = edgeCA / lengthCA;
        
        var normalAB = edgeABDir.GetPerpendicularRight();
        var normalBC = edgeBCDir.GetPerpendicularRight();
        var normalCA = edgeCADir.GetPerpendicularRight();
        
        var miterDirA = (normalCA + normalAB).Normalize();
        var miterDirB = (normalAB + normalBC).Normalize();
        var miterDirC = (normalBC + normalCA).Normalize();
        
        float miterAngleRadA = MathF.Abs(miterDirA.AngleRad(normalAB));
        float miterLengthA = lineThickness / MathF.Cos(miterAngleRadA);
        
        float miterAngleRadB = MathF.Abs(miterDirB.AngleRad(normalBC));
        float miterLengthB = lineThickness / MathF.Cos(miterAngleRadB);
        
        float miterAngleRadC = MathF.Abs(miterDirC.AngleRad(normalCA));
        float miterLengthC = lineThickness / MathF.Cos(miterAngleRadC);

        Vector2 aOuterPrev, aOuterNext;
        Vector2 bOuterPrev, bOuterNext;
        Vector2 cOuterPrev, cOuterNext;
        
        Vector2 aInner = triangle.A - miterDirA * MathF.Min(miterLengthA, innerMaxLengthA);
        Vector2 bInner = triangle.B - miterDirB * MathF.Min(miterLengthB, innerMaxLengthB);
        Vector2 cInner = triangle.C - miterDirC * MathF.Min(miterLengthC, innerMaxLengthC);
        
        var rayColor = color.ToRayColor();
        
        if (miterLimit < 2f || miterLengthA < totalMiterLengthLimit)
        {
            aOuterPrev = triangle.A + miterDirA * miterLengthA;
            aOuterNext = aOuterPrev;
        }
        else
        {
            if (beveled)
            {
                aOuterPrev = triangle.A + normalCA * lineThickness;
                aOuterNext = triangle.A + normalAB * lineThickness;
            }
            else
            {
                var p = triangle.A + miterDirA * totalMiterLengthLimit;
                var dir = (p - triangle.A).Normalize();
                var pr = dir.GetPerpendicularRight();
                var offsetC = triangle.C + normalCA * lineThickness;

                var ip = Ray.IntersectRayRay(p, pr, offsetC, edgeCADir);
                if (ip.Valid)
                {
                    aOuterPrev = ip.Point;
                    aOuterNext = p - pr * (p - ip.Point).Length();
                }
                else
                {
                    aOuterPrev = triangle.A + normalCA * lineThickness;
                    aOuterNext = triangle.A + normalAB * lineThickness;
                }
            }
            Raylib.DrawTriangle(aOuterPrev, aOuterNext, aInner, rayColor);
        }

        if (miterLimit < 2f || miterLengthB < totalMiterLengthLimit)
        {
            bOuterPrev = triangle.B + miterDirB * miterLengthB;
            bOuterNext = bOuterPrev;
        }
        else
        {
            if (beveled)
            {
                bOuterPrev = triangle.B + normalAB * lineThickness;
                bOuterNext = triangle.B + normalBC * lineThickness;
            }
            else
            {
                var p = triangle.B + miterDirB * totalMiterLengthLimit;
                var dir = (p - triangle.B).Normalize();
                var pr = dir.GetPerpendicularRight();
                var offsetA = triangle.A + normalAB * lineThickness;

                var ip = Ray.IntersectRayRay(p, pr, offsetA, edgeABDir);
                if (ip.Valid)
                {
                    bOuterPrev = ip.Point;
                    bOuterNext = p - pr * (p - ip.Point).Length();
                }
                else
                {
                    bOuterPrev = triangle.B + normalAB * lineThickness;
                    bOuterNext = triangle.B + normalBC * lineThickness;
                }
            }
            Raylib.DrawTriangle(bOuterPrev, bOuterNext, bInner, rayColor);
        }
        
        if (miterLimit < 2f || miterLengthC < totalMiterLengthLimit)
        {
            cOuterPrev = triangle.C + miterDirC * miterLengthC;
            cOuterNext = cOuterPrev;
        }
        else
        {
            if (beveled)
            {
                cOuterPrev = triangle.C + normalBC * lineThickness;
                cOuterNext = triangle.C + normalCA * lineThickness;
            }
            else
            {
                var p = triangle.C + miterDirC * totalMiterLengthLimit;
                var dir = (p - triangle.C).Normalize();
                var pr = dir.GetPerpendicularRight();
                var offsetB = triangle.B + normalBC * lineThickness;

                var ip = Ray.IntersectRayRay(p, pr, offsetB, edgeBCDir);
                if (ip.Valid)
                {
                    cOuterPrev = ip.Point;
                    cOuterNext = p - pr * (p - ip.Point).Length();
                }
                else
                {
                    cOuterPrev = triangle.C + normalBC * lineThickness;
                    cOuterNext = triangle.C + normalCA * lineThickness;
                }
            }
            Raylib.DrawTriangle(cOuterPrev, cOuterNext, cInner, rayColor);
        }

        Vector2 sideACenter = triangle.A + edgeABDir * lengthAB * 0.5f;
        Vector2 sideBCenter = triangle.B + edgeBCDir * lengthBC * 0.5f;
        Vector2 sideCCenter = triangle.C + edgeCADir * lengthCA * 0.5f;

        Vector2 sideACenterOutside = sideACenter + normalAB * lineThickness;
        Vector2 sideACenterInside = sideACenter - normalAB * lineThickness;
        
        Vector2 sideBCenterOutside = sideBCenter + normalBC * lineThickness;
        Vector2 sideBCenterInside = sideBCenter - normalBC * lineThickness;
        
        Vector2 sideCCenterOutside = sideCCenter + normalCA * lineThickness;
        Vector2 sideCCenterInside = sideCCenter - normalCA * lineThickness;

        var aInnerNextEnd = aInner.Lerp(sideACenterInside, cornerLengthFactor);
        var aInnerPrevEnd = aInner.Lerp(sideCCenterInside, cornerLengthFactor);
        var aOuterNextEnd = aOuterNext.Lerp(sideACenterOutside, cornerLengthFactor);
        var aOuterPrevEnd = aOuterPrev.Lerp(sideCCenterOutside, cornerLengthFactor);
        
        var bInnerNextEnd = bInner.Lerp(sideBCenterInside, cornerLengthFactor);
        var bInnerPrevEnd = bInner.Lerp(sideACenterInside, cornerLengthFactor);
        var bOuterNextEnd = bOuterNext.Lerp(sideBCenterOutside, cornerLengthFactor);
        var bOuterPrevEnd = bOuterPrev.Lerp(sideACenterOutside, cornerLengthFactor);
        
        var cInnerNextEnd = cInner.Lerp(sideCCenterInside, cornerLengthFactor);
        var cInnerPrevEnd = cInner.Lerp(sideBCenterInside, cornerLengthFactor);
        var cOuterNextEnd = cOuterNext.Lerp(sideCCenterOutside, cornerLengthFactor);
        var cOuterPrevEnd = cOuterPrev.Lerp(sideBCenterOutside, cornerLengthFactor);
        
        Raylib.DrawTriangle(aOuterPrev, aInner, aOuterPrevEnd, rayColor);
        Raylib.DrawTriangle(aInner, aInnerPrevEnd, aOuterPrevEnd, rayColor);
        
        Raylib.DrawTriangle(aOuterNext, aOuterNextEnd, aInner, rayColor);
        Raylib.DrawTriangle(aOuterNextEnd, aInnerNextEnd, aInner, rayColor);
        
        Raylib.DrawTriangle(bOuterPrev, bInner, bOuterPrevEnd, rayColor);
        Raylib.DrawTriangle(bInner, bInnerPrevEnd, bOuterPrevEnd, rayColor);
        
        Raylib.DrawTriangle(bOuterNext, bOuterNextEnd, bInner, rayColor);
        Raylib.DrawTriangle(bOuterNextEnd, bInnerNextEnd, bInner, rayColor);
        
        Raylib.DrawTriangle(cOuterPrev, cInner, cOuterPrevEnd, rayColor);
        Raylib.DrawTriangle(cInner, cInnerPrevEnd, cOuterPrevEnd, rayColor);
        
        Raylib.DrawTriangle(cOuterNext, cOuterNextEnd, cInner, rayColor);
        Raylib.DrawTriangle(cOuterNextEnd, cInnerNextEnd, cInner, rayColor);
    }
    
    #endregion
    
    #region Draw Vertices
    
    /// <summary>
    /// Draws circles at each vertex of the triangle.
    /// </summary>
    /// <param name="t">The triangle whose vertices to draw.</param>
    /// <param name="vertexRadius">The radius of each vertex circle.</param>
    /// <param name="color">The color of the vertex circles.</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleDrawing.CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    public static void DrawVertices(this Triangle t, float vertexRadius, ColorRgba color, float smoothness)
    {
        var circle = new Circle(t.A, vertexRadius);
        circle.Draw(color, smoothness);
        circle = circle.SetPosition(t.B);
        circle.Draw(color, smoothness);
        circle = circle.SetPosition(t.C);
        circle.Draw(color, smoothness);
    }
    
    #endregion
    
    #region Draw Masked
    
    /// <summary>
    /// Draws the triangle's three segments using the provided <see cref="LineDrawingInfo"/>,
    /// clipped by a triangular mask.
    /// </summary>
    /// <param name="triangle">The source triangle whose edges will be drawn.</param>
    /// <param name="mask">Triangle used as the clipping mask.
    /// Only portions outside (or inside when <paramref name="reversedMask"/> is true) will be drawn.</param>
    /// <param name="lineInfo">Styling and thickness information for the lines.</param>
    /// <param name="reversedMask">If true, inverts the mask so drawing occurs inside instead of outside the mask.</param>
    public static void DrawLinesMasked(this Triangle triangle, Triangle mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        triangle.SegmentAToB.DrawMasked(mask, lineInfo, reversedMask);
        triangle.SegmentBToC.DrawMasked(mask, lineInfo, reversedMask);
        triangle.SegmentCToA.DrawMasked(mask, lineInfo, reversedMask);
    }
    
    /// <summary>
    /// Draws the triangle's three segments using the provided <see cref="LineDrawingInfo"/>,
    /// clipped by a circular <see cref="Circle"/> mask.
    /// </summary>
    /// <param name="triangle">The source triangle whose edges will be drawn.</param>
    /// <param name="mask">Circle used as the clipping mask.
    /// Only portions outside (or inside when <paramref name="reversedMask"/> is true) will be drawn.</param>
    /// <param name="lineInfo">Styling and thickness information for the lines.</param>
    /// <param name="reversedMask">If true, inverts the mask so drawing occurs inside instead of outside the mask.</param>
    public static void DrawLinesMasked(this Triangle triangle, Circle mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        triangle.SegmentAToB.DrawMasked(mask, lineInfo, reversedMask);
        triangle.SegmentBToC.DrawMasked(mask, lineInfo, reversedMask);
        triangle.SegmentCToA.DrawMasked(mask, lineInfo, reversedMask);
    }
    
    /// <summary>
    /// Draws the triangle's three segments using the provided <see cref="LineDrawingInfo"/>,
    /// clipped by a rectangular <see cref="Rect"/> mask.
    /// </summary>
    /// <param name="triangle">The source triangle whose edges will be drawn.</param>
    /// <param name="mask">Rect used as the clipping mask.
    /// Only portions outside (or inside when <paramref name="reversedMask"/> is true) will be drawn.</param>
    /// <param name="lineInfo">Styling and thickness information for the lines.</param>
    /// <param name="reversedMask">If true, inverts the mask so drawing occurs inside instead of outside the mask.</param>
    public static void DrawLinesMasked(this Triangle triangle, Rect mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        triangle.SegmentAToB.DrawMasked(mask, lineInfo, reversedMask);
        triangle.SegmentBToC.DrawMasked(mask, lineInfo, reversedMask);
        triangle.SegmentCToA.DrawMasked(mask, lineInfo, reversedMask);
    }
    
    /// <summary>
    /// Draws the triangle's three segments using the provided <see cref="LineDrawingInfo"/>,
    /// clipped by a quadrilateral <see cref="Quad"/> mask.
    /// </summary>
    /// <param name="triangle">The source triangle whose edges will be drawn.</param>
    /// <param name="mask">Quad used as the clipping mask.
    /// Only portions outside (or inside when <paramref name="reversedMask"/> is true) will be drawn.</param>
    /// <param name="lineInfo">Styling and thickness information for the lines.</param>
    /// <param name="reversedMask">If true, inverts the mask so drawing occurs inside instead of outside the mask.</param>
    public static void DrawLinesMasked(this Triangle triangle, Quad mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        triangle.SegmentAToB.DrawMasked(mask, lineInfo, reversedMask);
        triangle.SegmentBToC.DrawMasked(mask, lineInfo, reversedMask);
        triangle.SegmentCToA.DrawMasked(mask, lineInfo, reversedMask);
    }
    
    /// <summary>
    /// Draws the triangle's three segments using the provided <see cref="LineDrawingInfo"/>,
    /// clipped by a polygonal <see cref="Polygon"/> mask.
    /// </summary>
    /// <param name="triangle">The source triangle whose edges will be drawn.</param>
    /// <param name="mask">Polygon used as the clipping mask.
    /// Only portions outside (or inside when <paramref name="reversedMask"/> is true) will be drawn.</param>
    /// <param name="lineInfo">Styling and thickness information for the lines.</param>
    /// <param name="reversedMask">If true, inverts the mask so drawing occurs inside instead of outside the mask.</param>
    public static void DrawLinesMasked(this Triangle triangle, Polygon mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        triangle.SegmentAToB.DrawMasked(mask, lineInfo, reversedMask);
        triangle.SegmentBToC.DrawMasked(mask, lineInfo, reversedMask);
        triangle.SegmentCToA.DrawMasked(mask, lineInfo, reversedMask);
    }
    
    /// <summary>
    /// Draws the triangle's three segments using the provided <see cref="LineDrawingInfo"/>,
    /// clipped by a closed-shape mask of the generic type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The mask type. Must implement <see cref="IClosedShapeTypeProvider"/> (for example: Triangle, Circle, Rect, Polygon, Quad).</typeparam>
    /// <param name="triangle">The source triangle whose edges will be drawn.</param>
    /// <param name="mask">The clipping mask instance.
    /// Only portions outside (or inside when <paramref name="reversedMask"/> is true) will be drawn.</param>
    /// <param name="lineInfo">Styling and thickness information for the lines.</param>
    /// <param name="reversedMask">If true, inverts the mask so drawing occurs inside instead of outside the mask.</param>
    /// <remarks>
    /// This generic overload delegates to the segment-level DrawMasked extension for each triangle edge.
    /// </remarks>
    public static void DrawLinesMasked<T>(this Triangle triangle, T mask, LineDrawingInfo lineInfo, bool reversedMask = false) where T : IClosedShapeTypeProvider
    {
        triangle.SegmentAToB.DrawMasked(mask, lineInfo, reversedMask);
        triangle.SegmentBToC.DrawMasked(mask, lineInfo, reversedMask);
        triangle.SegmentCToA.DrawMasked(mask, lineInfo, reversedMask);
    }
    
    #endregion
    
    #region Helper

    private static void DrawLinesHelper(Triangle triangle, float lineThickness, ColorRgba color, float miterLimit = 4f, bool beveled = true)
    {
        if (triangle.IsCollinear()) return;
        
        float totalMiterLengthLimit = lineThickness * 0.5f * MathF.Max(2f, miterLimit);
        
        var edgeAB = triangle.B - triangle.A;
        var edgeBC = triangle.C - triangle.B;
        var edgeCA = triangle.A - triangle.C;
        
        var lengthAB = edgeAB.Length();
        if (lengthAB <= 0f) return;
        
        var lengthBC = edgeBC.Length();
        if (lengthBC <= 0f) return;
        
        var lengthCA = edgeCA.Length();
        if (lengthCA <= 0f) return;

        float perimeter = lengthAB + lengthBC + lengthCA;
        Vector2 incenter = (triangle.A * lengthBC + triangle.B * lengthCA + triangle.C * lengthAB) / perimeter;
        
        var innerMaxLengthA = (incenter - triangle.A).Length();
        if(innerMaxLengthA <= 0f) return;
        
        var innerMaxLengthB = (incenter - triangle.B).Length();
        if(innerMaxLengthB <= 0f) return;
        
        var innerMaxLengthC = (incenter - triangle.C).Length();
        if(innerMaxLengthC <= 0f) return;
        
        var edgeABDir = edgeAB / lengthAB;
        var edgeBCDir = edgeBC / lengthBC;
        var edgeCADir = edgeCA / lengthCA;
        
        var normalAB = edgeABDir.GetPerpendicularRight();
        var normalBC = edgeBCDir.GetPerpendicularRight();
        var normalCA = edgeCADir.GetPerpendicularRight();
        
        var miterDirA = (normalCA + normalAB).Normalize();
        var miterDirB = (normalAB + normalBC).Normalize();
        var miterDirC = (normalBC + normalCA).Normalize();
        
        float miterAngleRadA = MathF.Abs(miterDirA.AngleRad(normalAB));
        float miterLengthA = lineThickness / MathF.Cos(miterAngleRadA);
        
        float miterAngleRadB = MathF.Abs(miterDirB.AngleRad(normalBC));
        float miterLengthB = lineThickness / MathF.Cos(miterAngleRadB);
        
        float miterAngleRadC = MathF.Abs(miterDirC.AngleRad(normalCA));
        float miterLengthC = lineThickness / MathF.Cos(miterAngleRadC);

        Vector2 aOuterPrev, aOuterNext;
        Vector2 bOuterPrev, bOuterNext;
        Vector2 cOuterPrev, cOuterNext;
        
        Vector2 aInner = triangle.A - miterDirA * MathF.Min(miterLengthA, innerMaxLengthA);
        Vector2 bInner = triangle.B - miterDirB * MathF.Min(miterLengthB, innerMaxLengthB);
        Vector2 cInner = triangle.C - miterDirC * MathF.Min(miterLengthC, innerMaxLengthC);
        
        var rayColor = color.ToRayColor();
        
        if (miterLimit < 2f || miterLengthA < totalMiterLengthLimit)
        {
            aOuterPrev = triangle.A + miterDirA * miterLengthA;
            aOuterNext = aOuterPrev;
            
        }
        else
        {
            if (beveled)
            {
                aOuterPrev = triangle.A + normalCA * lineThickness;
                aOuterNext = triangle.A + normalAB * lineThickness;
            }
            else
            {
                var p = triangle.A + miterDirA * totalMiterLengthLimit;
                var dir = (p - triangle.A).Normalize();
                var pr = dir.GetPerpendicularRight();
                var offsetC = triangle.C + normalCA * lineThickness;

                var ip = Ray.IntersectRayRay(p, pr, offsetC, edgeCADir);
                if (ip.Valid)
                {
                    aOuterPrev = ip.Point;
                    aOuterNext = p - pr * (p - ip.Point).Length();
                }
                else
                {
                    aOuterPrev = triangle.A + normalCA * lineThickness;
                    aOuterNext = triangle.A + normalAB * lineThickness;
                }
            }
            Raylib.DrawTriangle(aOuterPrev, aOuterNext, aInner, rayColor);
        }

        if (miterLimit < 2f || miterLengthB < totalMiterLengthLimit)
        {
            bOuterPrev = triangle.B + miterDirB * miterLengthB;
            bOuterNext = bOuterPrev;
        }
        else
        {
            if (beveled)
            {
                bOuterPrev = triangle.B + normalAB * lineThickness;
                bOuterNext = triangle.B + normalBC * lineThickness;
            }
            else
            {
                var p = triangle.B + miterDirB * totalMiterLengthLimit;
                var dir = (p - triangle.B).Normalize();
                var pr = dir.GetPerpendicularRight();
                var offsetA = triangle.A + normalAB * lineThickness;

                var ip = Ray.IntersectRayRay(p, pr, offsetA, edgeABDir);
                if (ip.Valid)
                {
                    bOuterPrev = ip.Point;
                    bOuterNext = p - pr * (p - ip.Point).Length();
                }
                else
                {
                    bOuterPrev = triangle.B + normalAB * lineThickness;
                    bOuterNext = triangle.B + normalBC * lineThickness;
                }
            }
            Raylib.DrawTriangle(bOuterPrev, bOuterNext, bInner, rayColor);
        }
        
        if (miterLimit < 2f || miterLengthC < totalMiterLengthLimit)
        {
            cOuterPrev = triangle.C + miterDirC * miterLengthC;
            cOuterNext = cOuterPrev;
        }
        else
        {
            if (beveled)
            {
                cOuterPrev = triangle.C + normalBC * lineThickness;
                cOuterNext = triangle.C + normalCA * lineThickness;
            }
            else
            {
                var p = triangle.C + miterDirC * totalMiterLengthLimit;
                var dir = (p - triangle.C).Normalize();
                var pr = dir.GetPerpendicularRight();
                var offsetB = triangle.B + normalBC * lineThickness;

                var ip = Ray.IntersectRayRay(p, pr, offsetB, edgeBCDir);
                if (ip.Valid)
                {
                    cOuterPrev = ip.Point;
                    cOuterNext = p - pr * (p - ip.Point).Length();
                }
                else
                {
                    cOuterPrev = triangle.C + normalBC * lineThickness;
                    cOuterNext = triangle.C + normalCA * lineThickness;
                }
            }
            Raylib.DrawTriangle(cOuterPrev, cOuterNext, cInner, rayColor);
        }
        
        Raylib.DrawTriangle(aOuterNext, bOuterPrev, aInner, rayColor);
        Raylib.DrawTriangle(aInner, bOuterPrev, bInner, rayColor);
        
        Raylib.DrawTriangle(bOuterNext, cOuterPrev, cInner, rayColor);
        Raylib.DrawTriangle(bInner, bOuterNext, cInner, rayColor);
        
        Raylib.DrawTriangle(cInner, cOuterNext, aOuterPrev, rayColor);
        Raylib.DrawTriangle(cInner, aOuterPrev, aInner, rayColor);

    }
    
    private static void DrawLinesPercentageHelper(Triangle triangle, float f, bool ccw, float lineThickness, ColorRgba color, float miterLimit = 4f, bool beveled = true)
    {
       if (triangle.IsCollinear()) return;
       
        var edgeAB = triangle.B - triangle.A;
        var edgeBC = triangle.C - triangle.B;
        var edgeCA = triangle.A - triangle.C;
        
        var lengthAB = edgeAB.Length();
        if (lengthAB <= 0f) return;
        
        var lengthBC = edgeBC.Length();
        if (lengthBC <= 0f) return;
        
        var lengthCA = edgeCA.Length();
        if (lengthCA <= 0f) return;

        float perimeter = lengthAB + lengthBC + lengthCA;
        Vector2 incenter = (triangle.A * lengthBC + triangle.B * lengthCA + triangle.C * lengthAB) / perimeter;
        
        var innerMaxLengthA = (incenter - triangle.A).Length();
        if(innerMaxLengthA <= 0f) return;
        
        var innerMaxLengthB = (incenter - triangle.B).Length();
        if(innerMaxLengthB <= 0f) return;
        
        var innerMaxLengthC = (incenter - triangle.C).Length();
        if(innerMaxLengthC <= 0f) return;
        
        var edgeABDir = edgeAB / lengthAB;
        var edgeBCDir = edgeBC / lengthBC;
        var edgeCADir = edgeCA / lengthCA;
        
        var normalAB = ccw ? edgeABDir.GetPerpendicularRight() : edgeABDir.GetPerpendicularLeft();
        var normalBC = ccw ? edgeBCDir.GetPerpendicularRight() : edgeBCDir.GetPerpendicularLeft();
        var normalCA = ccw ? edgeCADir.GetPerpendicularRight() : edgeCADir.GetPerpendicularLeft();
        
        var miterDirA = (normalCA + normalAB).Normalize();
        var miterDirB = (normalAB + normalBC).Normalize();
        var miterDirC = (normalBC + normalCA).Normalize();
        
        float miterAngleRadA = MathF.Abs(miterDirA.AngleRad(normalAB));
        float miterLengthA = lineThickness / MathF.Cos(miterAngleRadA);
        
        float miterAngleRadB = MathF.Abs(miterDirB.AngleRad(normalBC));
        float miterLengthB = lineThickness / MathF.Cos(miterAngleRadB);
        
        float miterAngleRadC = MathF.Abs(miterDirC.AngleRad(normalCA));
        float miterLengthC = lineThickness / MathF.Cos(miterAngleRadC);

        Vector2 aOuterPrev, aOuterNext;
        Vector2 bOuterPrev, bOuterNext;
        Vector2 cOuterPrev, cOuterNext;
        
        Vector2 aInner = triangle.A - miterDirA * MathF.Min(miterLengthA, innerMaxLengthA);
        Vector2 bInner = triangle.B - miterDirB * MathF.Min(miterLengthB, innerMaxLengthB);
        Vector2 cInner = triangle.C - miterDirC * MathF.Min(miterLengthC, innerMaxLengthC);
        
        float totalMiterLengthLimit = lineThickness * 0.5f * MathF.Max(2f, miterLimit);
        
        if (miterLimit < 2f || miterLengthA < totalMiterLengthLimit)
        {
            aOuterPrev = triangle.A + miterDirA * miterLengthA;
            aOuterNext = aOuterPrev;
            
        }
        else
        {
            if (beveled)
            {
                aOuterPrev = triangle.A + normalCA * lineThickness;
                aOuterNext = triangle.A + normalAB * lineThickness;
            }
            else
            {
                var p = triangle.A + miterDirA * totalMiterLengthLimit;
                var dir = (p - triangle.A).Normalize();
                var pr = dir.GetPerpendicularRight();
                var offsetC = triangle.C + normalCA * lineThickness;

                var ip = Ray.IntersectRayRay(p, pr, offsetC, edgeCADir);
                if (ip.Valid)
                {
                    aOuterPrev = ip.Point;
                    aOuterNext = p - pr * (p - ip.Point).Length();
                }
                else
                {
                    aOuterPrev = triangle.A + normalCA * lineThickness;
                    aOuterNext = triangle.A + normalAB * lineThickness;
                }
            }
        }

        if (miterLimit < 2f || miterLengthB < totalMiterLengthLimit)
        {
            bOuterPrev = triangle.B + miterDirB * miterLengthB;
            bOuterNext = bOuterPrev;
        }
        else
        {
            if (beveled)
            {
                bOuterPrev = triangle.B + normalAB * lineThickness;
                bOuterNext = triangle.B + normalBC * lineThickness;
            }
            else
            {
                var p = triangle.B + miterDirB * totalMiterLengthLimit;
                var dir = (p - triangle.B).Normalize();
                var pr = dir.GetPerpendicularRight();
                var offsetA = triangle.A + normalAB * lineThickness;

                var ip = Ray.IntersectRayRay(p, pr, offsetA, edgeABDir);
                if (ip.Valid)
                {
                    bOuterPrev = ip.Point;
                    bOuterNext = p - pr * (p - ip.Point).Length();
                }
                else
                {
                    bOuterPrev = triangle.B + normalAB * lineThickness;
                    bOuterNext = triangle.B + normalBC * lineThickness;
                }
            }
        }
        
        if (miterLimit < 2f || miterLengthC < totalMiterLengthLimit)
        {
            cOuterPrev = triangle.C + miterDirC * miterLengthC;
            cOuterNext = cOuterPrev;
        }
        else
        {
            if (beveled)
            {
                cOuterPrev = triangle.C + normalBC * lineThickness;
                cOuterNext = triangle.C + normalCA * lineThickness;
            }
            else
            {
                var p = triangle.C + miterDirC * totalMiterLengthLimit;
                var dir = (p - triangle.C).Normalize();
                var pr = dir.GetPerpendicularRight();
                var offsetB = triangle.B + normalBC * lineThickness;

                var ip = Ray.IntersectRayRay(p, pr, offsetB, edgeBCDir);
                if (ip.Valid)
                {
                    cOuterPrev = ip.Point;
                    cOuterNext = p - pr * (p - ip.Point).Length();
                }
                else
                {
                    cOuterPrev = triangle.C + normalBC * lineThickness;
                    cOuterNext = triangle.C + normalCA * lineThickness;
                }
            }
        }

        var rayColor = color.ToRayColor();
        
        // Calculate corner lengths (outer distance) to allow smooth interpolation
        float lenCornerA = (aOuterNext - aOuterPrev).Length();
        float lenCornerB = (bOuterNext - bOuterPrev).Length();
        float lenCornerC = (cOuterNext - cOuterPrev).Length();

        // Effective perimeter now includes the visual corners
        float effectivePerimeter = lengthAB + lengthBC + lengthCA + lenCornerA + lenCornerB + lenCornerC;
        float remainingLength = effectivePerimeter * f;

        // 1. Edge AB
        if (remainingLength > 0)
        {
            float drawLen = MathF.Min(remainingLength, lengthAB);
            float t = drawLen / lengthAB;
            
            Vector2 currentOuterEnd = Vector2.Lerp(aOuterNext, bOuterPrev, t);
            Vector2 currentInnerEnd = Vector2.Lerp(aInner, bInner, t);
            
            DrawQuad(aInner, currentInnerEnd, aOuterNext, currentOuterEnd);
            
            remainingLength -= lengthAB;
        }

        // 2. Corner B
        if (remainingLength > 0)
        {
            float drawLen = MathF.Min(remainingLength, lenCornerB);
            if(lenCornerB > 0)
            {
                DrawCornerPartial(bInner, bOuterPrev, bOuterNext, drawLen, lenCornerB);
            }
            remainingLength -= lenCornerB;
        }

        // 3. Edge BC
        if (remainingLength > 0)
        {
            float drawLen = MathF.Min(remainingLength, lengthBC);
            float t = drawLen / lengthBC;

            Vector2 currentOuterEnd = Vector2.Lerp(bOuterNext, cOuterPrev, t);
            Vector2 currentInnerEnd = Vector2.Lerp(bInner, cInner, t);

            DrawQuad(bInner, currentInnerEnd, bOuterNext, currentOuterEnd);
            
            remainingLength -= lengthBC;
        }

        // 4. Corner C
        if (remainingLength > 0)
        {
            float drawLen = MathF.Min(remainingLength, lenCornerC);
            if(lenCornerC > 0)
            {
                DrawCornerPartial(cInner, cOuterPrev, cOuterNext, drawLen, lenCornerC);
            }
            remainingLength -= lenCornerC;
        }

        // 5. Edge CA
        if (remainingLength > 0)
        {
            float drawLen = MathF.Min(remainingLength, lengthCA);
            float t = drawLen / lengthCA;

            Vector2 currentOuterEnd = Vector2.Lerp(cOuterNext, aOuterPrev, t);
            Vector2 currentInnerEnd = Vector2.Lerp(cInner, aInner, t);

            DrawQuad(cInner, currentInnerEnd, cOuterNext, currentOuterEnd);
            
            remainingLength -= lengthCA;
        }
        
        // 6. Corner A (Closing Loop)
        if (remainingLength > 0)
        {
             float drawLen = MathF.Min(remainingLength, lenCornerA);
             // Only draw closing corner if we completely finished the last edge
             if(lenCornerA > 0)
             {
                 DrawCornerPartial(aInner, aOuterPrev, aOuterNext, drawLen, lenCornerA);
             }
        }
        
        // Helper to draw a quad segment
        void DrawQuad(Vector2 startInner, Vector2 endInner, Vector2 startOuter, Vector2 endOuter)
        {
            if (ccw)
            {
                Raylib.DrawTriangle(startOuter, endOuter, startInner, rayColor);
                Raylib.DrawTriangle(startInner, endOuter, endInner, rayColor);
            }
            else
            {
                Raylib.DrawTriangle(startOuter, startInner, endOuter, rayColor);
                Raylib.DrawTriangle(startInner, endInner, endOuter, rayColor);
            }
            
        }

        // Helper to draw a corner partial
        void DrawCornerPartial(Vector2 pivotInner, Vector2 startOuter, Vector2 endOuter, float currentLen, float maxLen)
        {
            float t = currentLen / maxLen;
            Vector2 currentEndOuter = Vector2.Lerp(startOuter, endOuter, t);

            if (ccw)
            {
                Raylib.DrawTriangle(pivotInner, startOuter, currentEndOuter, rayColor);
            }
            else
            {
                Raylib.DrawTriangle(pivotInner, currentEndOuter, startOuter, rayColor);
            }
        }
        
    }
    
    #endregion
}

