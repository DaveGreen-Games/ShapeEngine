using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.StaticLib;
using Ray = ShapeEngine.Geometry.RayDef.Ray;

namespace ShapeEngine.Geometry.TriangleDef;

/// <summary>
/// Provides drawing helpers and instance drawing methods for <see cref="Triangle"/> values.
/// </summary>
public readonly partial struct Triangle
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
    /// <param name="color">The color to fill the triangle with.</param>
    public void Draw(ColorRgba color)
    {
        Raylib.DrawTriangle(A, B, C, color.ToRayColor());
    }
    
    #endregion

    #region Draw Scaled

    /// <summary>
    /// Draws a triangle with scaled sides based on a specific draw type.
    /// </summary>
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
    public void DrawScaled(ColorRgba color, float sideScaleFactor, float sideScaleOrigin, int drawType)
    {
        if (sideScaleFactor <= 0) return;
        if (sideScaleFactor >= 1)
        {
            Draw(color);
            return;
        }

        var s1 = new Segment(A, B).ScaleSegment(sideScaleFactor, sideScaleOrigin);
        var s2 = new Segment(B, C).ScaleSegment(sideScaleFactor, sideScaleOrigin);
        var s3 = new Segment(C, A).ScaleSegment(sideScaleFactor, sideScaleOrigin);

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
            var center = GetCentroid();
            Raylib.DrawTriangle(s1.Start, s1.End, center, rayColor);
            Raylib.DrawTriangle(s2.Start, s2.End, center, rayColor);
            Raylib.DrawTriangle(s3.Start, s3.End, center, rayColor);
        }
        else
        {
            var center = GetCentroid();
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
    /// <param name="lineThickness">The thickness of the outline lines.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="miterLimit">The limit for miter joins to prevent sharp spikes. Defaults to 4f.</param>
    /// <param name="beveled">If true, uses beveled joins when the miter limit is exceeded; otherwise, cuts off the miter point.</param>
    public void DrawLines(float lineThickness, ColorRgba color, float miterLimit = 4f, bool beveled = true)
    {
        DrawLinesHelper(this, lineThickness, color, miterLimit, beveled);
    }
    
    /// <summary>
    /// Draws the outline of the triangle using <see cref="LineDrawingInfo"/>.
    /// </summary>
    /// <param name="lineInfo">Contains line style information such as thickness and color.</param>
    /// <param name="miterLimit">The limit for miter joins to prevent sharp spikes. Defaults to 4f.</param>
    /// <param name="beveled">If true, uses beveled joins when the miter limit is exceeded; otherwise, cuts off the miter point.</param>
    public void DrawLines(LineDrawingInfo lineInfo, float miterLimit = 4f, bool beveled = true)
    {
        DrawLinesHelper(this, lineInfo.Thickness, lineInfo.Color, miterLimit, beveled);
    }

    #endregion
    
    #region Draw Lines Percentage
    
    /// <summary>
    /// Draws a partial outline of the triangle based on a percentage coverage.
    /// </summary>
    /// <param name="f">The percentage of the outline to draw (0 to 1). Negative values reverse the drawing direction.</param>
    /// <param name="startCorner">The index of the corner to start drawing from (0, 1, or 2).</param>
    /// <param name="lineInfo">Contains line style information such as thickness and color.</param>
    /// <param name="miterLimit">The limit for miter joins to prevent sharp spikes. Defaults to 4f.</param>
    /// <param name="beveled">If true, uses beveled joins when the miter limit is exceeded; otherwise, cuts off the miter point.</param>
    public void DrawLinesPercentage(float f, int startCorner, LineDrawingInfo lineInfo, float miterLimit = 4f, bool beveled = true)
    {
        DrawLinesPercentage(f, startCorner, lineInfo.Thickness, lineInfo.Color, miterLimit, beveled);
    }
    
    /// <summary>
    /// Draws a partial outline of the triangle based on a percentage coverage.
    /// </summary>
    /// <param name="f">The percentage of the outline to draw (0 to 1). Negative values reverse the drawing direction.</param>
    /// <param name="startCorner">The index of the corner to start drawing from (0, 1, or 2).</param>
    /// <param name="lineThickness">The thickness of the outline lines.</param>
    /// <param name="color">The color of the outline.</param>
    /// <param name="miterLimit">The limit for miter joins to prevent sharp spikes. Defaults to 4f.</param>
    /// <param name="beveled">If true, uses beveled joins when the miter limit is exceeded; otherwise, cuts off the miter point.</param>
    public void DrawLinesPercentage(float f, int startCorner, float lineThickness, ColorRgba color, float miterLimit = 4f, bool beveled = true)
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
            DrawLines(lineThickness, color, miterLimit, beveled);
            return;
        }

        startCorner = ShapeMath.WrapI(startCorner, 0, 3);
        
        Triangle triangle;
        
        if (startCorner == 0)
        {
            triangle = cw ? new(A, C, B) : new(A, B, C);
        }
        else if (startCorner == 1)
        {
            triangle = cw ? new(C, B, A) : new(B, C, A);
        }
        else
        {
            triangle = cw ? new(B, A, C) : new(C, A, B);
        }
        
        DrawLinesPercentageHelper(triangle, f, !cw, lineThickness, color, miterLimit, beveled);
    }
    
    #endregion
    
    #region Draw Lines Scaled

    /// <summary>
    /// Draws a triangle outline where each side can be scaled towards the origin of the side.
    /// </summary>
    /// <param name="lineInfo">The line drawing information (thickness, color, cap type, etc.).</param>
    /// <param name="sideScaleFactor">The scale factor for each side <c>(0 = No Side, 1 = Full Side).</c></param>
    /// <param name="sideScaleOrigin">The point along each side to scale from in both directions <c>(0 = Start, 1 = End)</c>.</param>
    /// <remarks>
    /// Allows for dynamic scaling of triangle sides, useful for effects or partial outlines.
    /// </remarks>
    public void DrawLinesScaled(LineDrawingInfo lineInfo, float sideScaleFactor, float sideScaleOrigin = 0.5f)
    {
        if (sideScaleFactor <= 0) return;
        if (sideScaleFactor >= 1)
        {
            DrawLines(lineInfo);
            return;
        }
        
        Segment.DrawSegment(A, B, lineInfo, sideScaleFactor, sideScaleOrigin);
        Segment.DrawSegment(B, C, lineInfo, sideScaleFactor, sideScaleOrigin);
        Segment.DrawSegment(C, A, lineInfo, sideScaleFactor, sideScaleOrigin);
    }
    
    #endregion
    
    #region Draw Corners
    
    /// <summary>
    /// Draws the corners of the triangle with a specific length along the edges.
    /// </summary>
    /// <param name="lineThickness">The thickness of the corner lines.</param>
    /// <param name="color">The color of the corners.</param>
    /// <param name="cornerLength">The absolute length of the corner segments along the edges from each vertex.</param>
    /// <param name="miterLimit">The limit for miter joins to prevent sharp spikes. Defaults to 4f.</param>
    /// <param name="beveled">If true, uses beveled joins when the miter limit is exceeded; otherwise, cuts off the miter point.</param>
    public void DrawCorners(float lineThickness, ColorRgba color, float cornerLength, float miterLimit = 4f, bool beveled = true)
    {
        if(IsCollinear()) return;
        
        if(cornerLength <= 0f) return;
        
        var edgeAB = B - A;
        var edgeBC = C - B;
        var edgeCA = A - C;
        
        var lengthAB = edgeAB.Length();
        if (lengthAB <= 0f) return;
        
        var lengthBC = edgeBC.Length();
        if (lengthBC <= 0f) return;
        
        var lengthCA = edgeCA.Length();
        if (lengthCA <= 0f) return;
        
        var minLength = MathF.Min(lengthAB, MathF.Min(lengthBC, lengthCA));
        if (minLength <= cornerLength)
        {
            DrawLines(lineThickness, color, miterLimit, beveled);
            return;
        }
        var cornerLengthFactor = ShapeMath.Clamp(0f, 1f, cornerLength / minLength);
        
        float totalMiterLengthLimit = lineThickness * 0.5f * MathF.Max(2f, miterLimit);
        
        float perimeter = lengthAB + lengthBC + lengthCA;
        Vector2 incenter = (A * lengthBC + B * lengthCA + C * lengthAB) / perimeter;
        
        var innerMaxLengthA = (incenter - A).Length();
        if(innerMaxLengthA <= 0f) return;
        
        var innerMaxLengthB = (incenter - B).Length();
        if(innerMaxLengthB <= 0f) return;
        
        var innerMaxLengthC = (incenter - C).Length();
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
        
        Vector2 aInner = A - miterDirA * MathF.Min(miterLengthA, innerMaxLengthA);
        Vector2 bInner = B - miterDirB * MathF.Min(miterLengthB, innerMaxLengthB);
        Vector2 cInner = C - miterDirC * MathF.Min(miterLengthC, innerMaxLengthC);
        
        var rayColor = color.ToRayColor();
        
        if (miterLimit < 2f || miterLengthA < totalMiterLengthLimit)
        {
            aOuterPrev = A + miterDirA * miterLengthA;
            aOuterNext = aOuterPrev;
        }
        else
        {
            if (beveled)
            {
                aOuterPrev = A + normalCA * lineThickness;
                aOuterNext = A + normalAB * lineThickness;
            }
            else
            {
                var p = A + miterDirA * totalMiterLengthLimit;
                var dir = (p - A).Normalize();
                var pr = dir.GetPerpendicularRight();
                var offsetC = C + normalCA * lineThickness;
        
                var ip = Ray.IntersectRayRay(p, pr, offsetC, edgeCADir);
                if (ip.Valid)
                {
                    aOuterPrev = ip.Point;
                    aOuterNext = p - pr * (p - ip.Point).Length();
                }
                else
                {
                    aOuterPrev = A + normalCA * lineThickness;
                    aOuterNext = A + normalAB * lineThickness;
                }
            }
            Raylib.DrawTriangle(aOuterPrev, aOuterNext, aInner, rayColor);
        }
        
        if (miterLimit < 2f || miterLengthB < totalMiterLengthLimit)
        {
            bOuterPrev = B + miterDirB * miterLengthB;
            bOuterNext = bOuterPrev;
        }
        else
        {
            if (beveled)
            {
                bOuterPrev = B + normalAB * lineThickness;
                bOuterNext = B + normalBC * lineThickness;
            }
            else
            {
                var p = B + miterDirB * totalMiterLengthLimit;
                var dir = (p - B).Normalize();
                var pr = dir.GetPerpendicularRight();
                var offsetA = A + normalAB * lineThickness;
        
                var ip = Ray.IntersectRayRay(p, pr, offsetA, edgeABDir);
                if (ip.Valid)
                {
                    bOuterPrev = ip.Point;
                    bOuterNext = p - pr * (p - ip.Point).Length();
                }
                else
                {
                    bOuterPrev = B + normalAB * lineThickness;
                    bOuterNext = B + normalBC * lineThickness;
                }
            }
            Raylib.DrawTriangle(bOuterPrev, bOuterNext, bInner, rayColor);
        }
        
        if (miterLimit < 2f || miterLengthC < totalMiterLengthLimit)
        {
            cOuterPrev = C + miterDirC * miterLengthC;
            cOuterNext = cOuterPrev;
        }
        else
        {
            if (beveled)
            {
                cOuterPrev = C + normalBC * lineThickness;
                cOuterNext = C + normalCA * lineThickness;
            }
            else
            {
                var p = C + miterDirC * totalMiterLengthLimit;
                var dir = (p - C).Normalize();
                var pr = dir.GetPerpendicularRight();
                var offsetB = B + normalBC * lineThickness;
        
                var ip = Ray.IntersectRayRay(p, pr, offsetB, edgeBCDir);
                if (ip.Valid)
                {
                    cOuterPrev = ip.Point;
                    cOuterNext = p - pr * (p - ip.Point).Length();
                }
                else
                {
                    cOuterPrev = C + normalBC * lineThickness;
                    cOuterNext = C + normalCA * lineThickness;
                }
            }
            Raylib.DrawTriangle(cOuterPrev, cOuterNext, cInner, rayColor);
        }
        
        Vector2 sideACenter = A + edgeABDir * lengthAB * 0.5f;
        Vector2 sideBCenter = B + edgeBCDir * lengthBC * 0.5f;
        Vector2 sideCCenter = C + edgeCADir * lengthCA * 0.5f;
        
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
    /// <param name="lineThickness">The thickness of the corner lines.</param>
    /// <param name="color">The color of the corners.</param>
    /// <param name="cornerLengthFactor">The length factor of the corner segments relative to the shortest edge (0 to 0.5 recommended).</param>
    /// <param name="miterLimit">The limit for miter joins to prevent sharp spikes. Defaults to 4f.</param>
    /// <param name="beveled">If true, uses beveled joins when the miter limit is exceeded; otherwise, cuts off the miter point.</param>
    public void DrawCornersRelative(float lineThickness, ColorRgba color, float cornerLengthFactor, float miterLimit = 4f, bool beveled = true)
    {
        if(IsCollinear()) return;

        if(cornerLengthFactor <= 0f) return;
        
        if(cornerLengthFactor >= 1f)
        {
            DrawLines(lineThickness, color, miterLimit, beveled);
            return;
        }
        
        float totalMiterLengthLimit = lineThickness * 0.5f * MathF.Max(2f, miterLimit);
        
        var edgeAB = B - A;
        var edgeBC = C - B;
        var edgeCA = A - C;
        
        var lengthAB = edgeAB.Length();
        if (lengthAB <= 0f) return;
        
        var lengthBC = edgeBC.Length();
        if (lengthBC <= 0f) return;
        
        var lengthCA = edgeCA.Length();
        if (lengthCA <= 0f) return;

        float perimeter = lengthAB + lengthBC + lengthCA;
        Vector2 incenter = (A * lengthBC + B * lengthCA + C * lengthAB) / perimeter;
        
        var innerMaxLengthA = (incenter - A).Length();
        if(innerMaxLengthA <= 0f) return;
        
        var innerMaxLengthB = (incenter - B).Length();
        if(innerMaxLengthB <= 0f) return;
        
        var innerMaxLengthC = (incenter - C).Length();
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
        
        Vector2 aInner = A - miterDirA * MathF.Min(miterLengthA, innerMaxLengthA);
        Vector2 bInner = B - miterDirB * MathF.Min(miterLengthB, innerMaxLengthB);
        Vector2 cInner = C - miterDirC * MathF.Min(miterLengthC, innerMaxLengthC);
        
        var rayColor = color.ToRayColor();
        
        if (miterLimit < 2f || miterLengthA < totalMiterLengthLimit)
        {
            aOuterPrev = A + miterDirA * miterLengthA;
            aOuterNext = aOuterPrev;
        }
        else
        {
            if (beveled)
            {
                aOuterPrev = A + normalCA * lineThickness;
                aOuterNext = A + normalAB * lineThickness;
            }
            else
            {
                var p = A + miterDirA * totalMiterLengthLimit;
                var dir = (p - A).Normalize();
                var pr = dir.GetPerpendicularRight();
                var offsetC = C + normalCA * lineThickness;

                var ip = Ray.IntersectRayRay(p, pr, offsetC, edgeCADir);
                if (ip.Valid)
                {
                    aOuterPrev = ip.Point;
                    aOuterNext = p - pr * (p - ip.Point).Length();
                }
                else
                {
                    aOuterPrev = A + normalCA * lineThickness;
                    aOuterNext = A + normalAB * lineThickness;
                }
            }
            Raylib.DrawTriangle(aOuterPrev, aOuterNext, aInner, rayColor);
        }

        if (miterLimit < 2f || miterLengthB < totalMiterLengthLimit)
        {
            bOuterPrev = B + miterDirB * miterLengthB;
            bOuterNext = bOuterPrev;
        }
        else
        {
            if (beveled)
            {
                bOuterPrev = B + normalAB * lineThickness;
                bOuterNext = B + normalBC * lineThickness;
            }
            else
            {
                var p = B + miterDirB * totalMiterLengthLimit;
                var dir = (p - B).Normalize();
                var pr = dir.GetPerpendicularRight();
                var offsetA = A + normalAB * lineThickness;

                var ip = Ray.IntersectRayRay(p, pr, offsetA, edgeABDir);
                if (ip.Valid)
                {
                    bOuterPrev = ip.Point;
                    bOuterNext = p - pr * (p - ip.Point).Length();
                }
                else
                {
                    bOuterPrev = B + normalAB * lineThickness;
                    bOuterNext = B + normalBC * lineThickness;
                }
            }
            Raylib.DrawTriangle(bOuterPrev, bOuterNext, bInner, rayColor);
        }
        
        if (miterLimit < 2f || miterLengthC < totalMiterLengthLimit)
        {
            cOuterPrev = C + miterDirC * miterLengthC;
            cOuterNext = cOuterPrev;
        }
        else
        {
            if (beveled)
            {
                cOuterPrev = C + normalBC * lineThickness;
                cOuterNext = C + normalCA * lineThickness;
            }
            else
            {
                var p = C + miterDirC * totalMiterLengthLimit;
                var dir = (p - C).Normalize();
                var pr = dir.GetPerpendicularRight();
                var offsetB = B + normalBC * lineThickness;

                var ip = Ray.IntersectRayRay(p, pr, offsetB, edgeBCDir);
                if (ip.Valid)
                {
                    cOuterPrev = ip.Point;
                    cOuterNext = p - pr * (p - ip.Point).Length();
                }
                else
                {
                    cOuterPrev = C + normalBC * lineThickness;
                    cOuterNext = C + normalCA * lineThickness;
                }
            }
            Raylib.DrawTriangle(cOuterPrev, cOuterNext, cInner, rayColor);
        }

        Vector2 sideACenter = A + edgeABDir * lengthAB * 0.5f;
        Vector2 sideBCenter = B + edgeBCDir * lengthBC * 0.5f;
        Vector2 sideCCenter = C + edgeCADir * lengthCA * 0.5f;

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
    /// <param name="vertexRadius">The radius of each vertex circle.</param>
    /// <param name="color">The color of the vertex circles.</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="Circle.CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    public void DrawVertices(float vertexRadius, ColorRgba color, float smoothness)
    {
        var circle = new Circle(A, vertexRadius);
        circle.Draw(color, smoothness);
        circle = circle.SetPosition(B);
        circle.Draw(color, smoothness);
        circle = circle.SetPosition(C);
        circle.Draw(color, smoothness);
    }
    
    #endregion
    
    #region Draw Masked
    
    /// <summary>
    /// Draws the triangle's three segments using the provided <see cref="LineDrawingInfo"/>,
    /// clipped by a triangular mask.
    /// </summary>
    /// <param name="mask">Triangle used as the clipping mask.
    /// Only portions outside (or inside when <paramref name="reversedMask"/> is true) will be drawn.</param>
    /// <param name="lineInfo">Styling and thickness information for the lines.</param>
    /// <param name="reversedMask">If true, inverts the mask so drawing occurs inside instead of outside the mask.</param>
    public void DrawLinesMasked(Triangle mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        SegmentAToB.DrawMasked(mask, lineInfo, reversedMask);
        SegmentBToC.DrawMasked(mask, lineInfo, reversedMask);
        SegmentCToA.DrawMasked(mask, lineInfo, reversedMask);
    }
    
    /// <summary>
    /// Draws the triangle's three segments using the provided <see cref="LineDrawingInfo"/>,
    /// clipped by a circular <see cref="Circle"/> mask.
    /// </summary>
    /// <param name="mask">Circle used as the clipping mask.
    /// Only portions outside (or inside when <paramref name="reversedMask"/> is true) will be drawn.</param>
    /// <param name="lineInfo">Styling and thickness information for the lines.</param>
    /// <param name="reversedMask">If true, inverts the mask so drawing occurs inside instead of outside the mask.</param>
    public void DrawLinesMasked(Circle mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        SegmentAToB.DrawMasked(mask, lineInfo, reversedMask);
        SegmentBToC.DrawMasked(mask, lineInfo, reversedMask);
        SegmentCToA.DrawMasked(mask, lineInfo, reversedMask);
    }
    
    /// <summary>
    /// Draws the triangle's three segments using the provided <see cref="LineDrawingInfo"/>,
    /// clipped by a rectangular <see cref="Rect"/> mask.
    /// </summary>
    /// <param name="mask">Rect used as the clipping mask.
    /// Only portions outside (or inside when <paramref name="reversedMask"/> is true) will be drawn.</param>
    /// <param name="lineInfo">Styling and thickness information for the lines.</param>
    /// <param name="reversedMask">If true, inverts the mask so drawing occurs inside instead of outside the mask.</param>
    public void DrawLinesMasked(Rect mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        SegmentAToB.DrawMasked(mask, lineInfo, reversedMask);
        SegmentBToC.DrawMasked(mask, lineInfo, reversedMask);
        SegmentCToA.DrawMasked(mask, lineInfo, reversedMask);
    }
    
    /// <summary>
    /// Draws the triangle's three segments using the provided <see cref="LineDrawingInfo"/>,
    /// clipped by a quadrilateral <see cref="Quad"/> mask.
    /// </summary>
    /// <param name="mask">Quad used as the clipping mask.
    /// Only portions outside (or inside when <paramref name="reversedMask"/> is true) will be drawn.</param>
    /// <param name="lineInfo">Styling and thickness information for the lines.</param>
    /// <param name="reversedMask">If true, inverts the mask so drawing occurs inside instead of outside the mask.</param>
    public void DrawLinesMasked(Quad mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        SegmentAToB.DrawMasked(mask, lineInfo, reversedMask);
        SegmentBToC.DrawMasked(mask, lineInfo, reversedMask);
        SegmentCToA.DrawMasked(mask, lineInfo, reversedMask);
    }
    
    /// <summary>
    /// Draws the triangle's three segments using the provided <see cref="LineDrawingInfo"/>,
    /// clipped by a polygonal <see cref="Polygon"/> mask.
    /// </summary>
    /// <param name="mask">Polygon used as the clipping mask.
    /// Only portions outside (or inside when <paramref name="reversedMask"/> is true) will be drawn.</param>
    /// <param name="lineInfo">Styling and thickness information for the lines.</param>
    /// <param name="reversedMask">If true, inverts the mask so drawing occurs inside instead of outside the mask.</param>
    public void DrawLinesMasked(Polygon mask, LineDrawingInfo lineInfo, bool reversedMask = false)
    {
        SegmentAToB.DrawMasked(mask, lineInfo, reversedMask);
        SegmentBToC.DrawMasked(mask, lineInfo, reversedMask);
        SegmentCToA.DrawMasked(mask, lineInfo, reversedMask);
    }
    
    /// <summary>
    /// Draws the triangle's three segments using the provided <see cref="LineDrawingInfo"/>,
    /// clipped by a closed-shape mask of the generic type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The mask type. Must implement <see cref="IClosedShapeTypeProvider"/> (for example: Triangle, Circle, Rect, Polygon, Quad).</typeparam>
    /// <param name="mask">The clipping mask instance.
    /// Only portions outside (or inside when <paramref name="reversedMask"/> is true) will be drawn.</param>
    /// <param name="lineInfo">Styling and thickness information for the lines.</param>
    /// <param name="reversedMask">If true, inverts the mask so drawing occurs inside instead of outside the mask.</param>
    /// <remarks>
    /// This generic overload delegates to the segment-level DrawMasked extension for each triangle edge.
    /// </remarks>
    public void DrawLinesMasked<T>(T mask, LineDrawingInfo lineInfo, bool reversedMask = false) where T : IClosedShapeTypeProvider
    {
        SegmentAToB.DrawMasked(mask, lineInfo, reversedMask);
        SegmentBToC.DrawMasked(mask, lineInfo, reversedMask);
        SegmentCToA.DrawMasked(mask, lineInfo, reversedMask);
    }
    
    #endregion
    
    #region Gapped
    /// <summary>
    /// Draws a gapped outline for a triangle, creating a dashed or segmented effect along the triangle's perimeter.
    /// </summary>
    /// <param name="perimeter">
    /// The total length of the triangle's perimeter.
    /// If zero or negative, the method calculates it automatically.
    /// Providing a known length avoids redundant calculations and improves performance, especially for static segments.
    /// </param>
    /// <param name="lineInfo">Parameters describing how to draw the outline.</param>
    /// <param name="gapDrawingInfo">Parameters describing the gap configuration.</param>
    /// <returns>
    /// The perimeter of the triangle if positive; otherwise, -1.
    /// If the shape does not change, the valid length can be reused in subsequent frames to avoid recalculating.
    /// </returns>
    /// <remarks>
    /// - If <paramref name="gapDrawingInfo.Gaps"/> is 0 or <paramref name="gapDrawingInfo.GapPerimeterPercentage"/> is 0, the outline is drawn solid.
    /// - If <paramref name="gapDrawingInfo.GapPerimeterPercentage"/> is 1 or greater, no outline is drawn.
    /// </remarks>
    public float DrawGappedOutline(float perimeter, LineDrawingInfo lineInfo, GappedOutlineDrawingInfo gapDrawingInfo)
    {
        if (gapDrawingInfo.Gaps <= 0 || gapDrawingInfo.GapPerimeterPercentage <= 0f)
        {
            DrawLines(lineInfo);
            return perimeter > 0f ? perimeter : -1f;
        }

        if (gapDrawingInfo.GapPerimeterPercentage >= 1f) return perimeter > 0f ? perimeter : -1f;

        var nonGapPercentage = 1f - gapDrawingInfo.GapPerimeterPercentage;

        var gapPercentageRange = gapDrawingInfo.GapPerimeterPercentage / gapDrawingInfo.Gaps;
        var nonGapPercentageRange = nonGapPercentage / gapDrawingInfo.Gaps;

        
        var shapePoints = new[] {A, B, C};
        int sides = shapePoints.Length;

        if (perimeter <= 0f)
        {
            perimeter = 0f;
            for (int i = 0; i < sides; i++)
            {
                var curP = shapePoints[i];
                var nextP = shapePoints[(i + 1) % sides];
                perimeter += (nextP - curP).Length();
            }
        }
        

        var startDistance = perimeter * gapDrawingInfo.StartOffset;
        var curDistance = 0f;
        var nextDistance = startDistance;
        
        var curIndex = 0;
        var curPoint = shapePoints[0];
        var nextPoint= shapePoints[1];
        var curW = nextPoint - curPoint;
        var curDis = curW.Length();
        
        var points = new List<Vector2>(3);

        int whileCounter = gapDrawingInfo.Gaps;
        
        while (whileCounter > 0)
        {
            if (curDistance + curDis >= nextDistance)
            {
                var p = curPoint + (curW / curDis) * (nextDistance - curDistance);
                
                
                if (points.Count == 0)
                {
                    nextDistance += nonGapPercentageRange * perimeter;
                    points.Add(p);

                }
                else
                {
                    nextDistance += gapPercentageRange * perimeter;
                    points.Add(p);
                    if (points.Count == 2)
                    {
                        Segment.DrawSegment(points[0], points[1], lineInfo);
                    }
                    else
                    {
                        for (var i = 0; i < points.Count - 1; i++)
                        {
                            var p1 = points[i];
                            var p2 = points[(i + 1) % points.Count];
                            Segment.DrawSegment(p1, p2, lineInfo);
                        }
                    }
                    
                    points.Clear();
                    whileCounter--;
                }

            }
            else
            {
                
                if(points.Count > 0) points.Add(nextPoint);
                
                curDistance += curDis;
                curIndex = (curIndex + 1) % sides;
                curPoint = shapePoints[curIndex];
                nextPoint = shapePoints[(curIndex + 1) % sides];
                curW = nextPoint - curPoint;
                curDis = curW.Length();
            }
            
        }

        return perimeter;
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

