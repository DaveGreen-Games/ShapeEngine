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

//NOTE:
// - [DONE] Remove all functions that are not extension methods (not  using this Triangle t)?
// - [DONE] Remove DrawLinesSideLengthFactor in favor of DrawLinesScaled? (What does DrawLinesSideLengthFactor do differently?)
// - [DONE] Remove rounded corner variations? (would simplify the API a lot...)
// - [DONE] Add miterLimit and beveled parameters to all outline drawing functions (If rounded corners are not removed, cornerPoints are an additional parameter that disable beveled/mitered corners when > 0)
// - [DONE] Add DrawScaled (full triangle version of DrawLinesScaled)
// - [DONE] Implement DrawLinesHelper
// - [] Implement DrawLinesPercentageHelper

//NOTE: After finishing check triangulation drawing as well (for extra parameters to add, etc.)

//TODO: Add/Update xml summaries

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
    public static void Draw(this Triangle t, ColorRgba color, float sideScaleFactor, float sideScaleOrigin = 0.5f)
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
        
        Raylib.DrawTriangle(s1.Start, s1.End, s2.Start, rayColor);
        Raylib.DrawTriangle(s1.Start, s2.Start, s3.End, rayColor);
        Raylib.DrawTriangle(s3.End, s2.Start, s2.End, rayColor);
        Raylib.DrawTriangle(s3.End, s2.End, s3.Start, rayColor);
    }
    #endregion
    
    #region Draw Lines

    public static void DrawLines(this Triangle t, float lineThickness, ColorRgba color, float miterLimit = 4f, bool beveled = true)
    {
        DrawLinesHelper(t, lineThickness, color, miterLimit, beveled);
    }
    
    public static void DrawLines(this Triangle t, LineDrawingInfo lineInfo, float miterLimit = 4f, bool beveled = true)
    {
        DrawLinesHelper(t, lineInfo.Thickness, lineInfo.Color, miterLimit, beveled);
    }

    #endregion
    
    #region Draw Lines Percentage
    
    public static void DrawLinesPercentage(this Triangle t, float f, LineDrawingInfo lineInfo, float miterLimit = 4f, bool beveled = true)
    {
        if (f == 0) return;
        
        bool negative = false;
        if (f < 0)
        {
            negative = true;
            f *= -1;
        }
        
        int startCorner = (int)f;
        float percentage = f - startCorner;
        if (percentage <= 0)//percentage = 0 means draw 100%
        {
            t.DrawLines(lineInfo, miterLimit, beveled);
            return;
        }

        startCorner = ShapeMath.WrapI(startCorner, 0, 3);
        
        Triangle triangle;
        
        if (startCorner == 0)
        {
            triangle = negative ? new(t.A, t.C, t.B) : new(t.A, t.B, t.C);
        }
        else if (startCorner == 1)
        {
            triangle = negative ? new(t.C, t.B, t.A) : new(t.B, t.C, t.A);
        }
        else
        {
            triangle = negative ? new(t.B, t.A, t.C) : new(t.C, t.A, t.B);
        }
        
        DrawLinesPercentageHelper(triangle, percentage, !negative, lineInfo, miterLimit, beveled);
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
    
    private static void DrawLinesPercentageHelper(Triangle triangle, float f, bool ccw, LineDrawingInfo lineInfo, float miterLimit = 4f, bool beveled = true)
    {
       //TODO: Implement
    }
    
    #endregion
}




//TODO: Remove

/*
private static void DrawTriangleLinesPercentageHelper(Vector2 p1, Vector2 p2, Vector2 p3, float percentage, bool ccw, float lineThickness, ColorRgba color, int capPoints)
{
    if (lineThickness <= 0 || percentage <= 0 || percentage >= 1) return;
    
    if (capPoints <= 0)
    {
        DrawTriangleLinesPercentageHelperMitered(p1, p2, p3, percentage, lineThickness, color);
    }
    else
    {
        DrawTriangleLinesPercentageHelperCapped(p1, p2, p3, percentage, ccw, lineThickness, color, capPoints);
    }
}
private static void DrawTriangleLinesPercentageHelperMitered(Vector2 p1, Vector2 p2, Vector2 p3, float percentage, float lineThickness, ColorRgba color)
{
    if (lineThickness <= 0 || percentage <= 0 || percentage >= 1) return;

    float maxThickness = CalculateMaxLineThickness(p1, p2, p3);
    float thickness = MathF.Min(lineThickness, maxThickness);

    var edge1 = p2 - p1;
    var edge2 = p3 - p2;
    var edge3 = p1 - p3;

    var normal1 = new Vector2(-edge1.Y, edge1.X);
    if (normal1.LengthSquared() > 0) normal1 = Vector2.Normalize(normal1);

    var normal2 = new Vector2(-edge2.Y, edge2.X);
    if (normal2.LengthSquared() > 0) normal2 = Vector2.Normalize(normal2);

    var normal3 = new Vector2(-edge3.Y, edge3.X);
    if (normal3.LengthSquared() > 0) normal3 = Vector2.Normalize(normal3);

    float l1 = edge1.Length();
    float l2 = edge2.Length();
    float l3 = edge3.Length();
    float totalPerimeter = l1 + l2 + l3;
    float perimeterToDraw = totalPerimeter * percentage;

    var miter1Inner = CalculateMiterPoint(p1, normal3, normal1, thickness, false);
    var miter1Outer = CalculateMiterPoint(p1, normal3, normal1, thickness, true);
    
    var miter2Inner = CalculateMiterPoint(p2, normal1, normal2, thickness, false);
    var miter2Outer = CalculateMiterPoint(p2, normal1, normal2, thickness, true);

    Vector2 a, b, c, d;
    float f = 1f;
    if (l1 > perimeterToDraw)
    {
        f = perimeterToDraw / l1;
    }
    
    a = miter1Inner;
    b = miter1Outer;
    c = f >= 1f ? miter2Outer : b.Lerp(miter2Outer, f);
    d = f >= 1f ? miter2Inner : a.Lerp(miter2Inner, f);
    DrawTriangle(a,b,c,color);
    DrawTriangle(a,c,d,color);
    
    if(f < 1f) return;
    f = 1f;
    perimeterToDraw -= l1;
    
    var miter3Inner = CalculateMiterPoint(p3, normal2, normal3, thickness, false);
    var miter3Outer = CalculateMiterPoint(p3, normal2, normal3, thickness, true);
    
    if (l2 > perimeterToDraw)
    {
        f = perimeterToDraw / l2;
    }
    
    a = miter2Inner;
    b = miter2Outer;
    c = f >= 1f ? miter3Outer : b.Lerp(miter3Outer, f);
    d = f >= 1f ? miter3Inner : a.Lerp(miter3Inner, f);
    DrawTriangle(a,b,c,color);
    DrawTriangle(a,c,d,color);
    
    if(f < 1f) return;
    f = 1f;
    perimeterToDraw -= l2;
    
    if (l3 > perimeterToDraw)
    {
        f = perimeterToDraw / l3;
    }
    
    a = miter3Inner;
    b = miter3Outer;
    c = f >= 1f ? miter1Outer : b.Lerp(miter1Outer, f);
    d = f >= 1f ? miter1Inner : a.Lerp(miter1Inner, f);
    DrawTriangle(a,b,c,color);
    DrawTriangle(a,c,d,color);
}
private static void DrawTriangleLinesPercentageHelperCapped(Vector2 p1, Vector2 p2, Vector2 p3, float percentage, bool ccw, float lineThickness, ColorRgba color, int cornerPoints = 2)
{
    if (MathF.Abs(percentage) < 0.001f || lineThickness <= 0f || cornerPoints <= 0) return;

    if (MathF.Abs(percentage) >= 1f)
    {
        DrawTriangleLinesHelper(p1, p2, p3, lineThickness, color, cornerPoints);
        return;
    }

    float maxThickness = CalculateMaxLineThickness(p1, p2, p3);
    float thickness = MathF.Min(lineThickness, maxThickness);
    
    var e1 = p2 - p1;
    var e2 = p3 - p2;
    var e3 = p1 - p3;

    var n1 = Vector2.Normalize(new(-e1.Y, e1.X));
    var n2 = Vector2.Normalize(new(-e2.Y, e2.X));
    var n3 = Vector2.Normalize(new(-e3.Y, e3.X));
    
    if (!ccw)
    {
        n1 = -n1;
        n2 = -n2;
        n3 = -n3;
    }
    
    var m1Inner = CalculateMiterPoint(p1, n3, n1, thickness, false);
    var m2Inner = CalculateMiterPoint(p2, n1, n2, thickness, false);
    var m3Inner = CalculateMiterPoint(p3, n2, n3, thickness, false);
    
    float aN1 = MathF.Atan2(n1.Y, n1.X);
    float aN2 = MathF.Atan2(n2.Y, n2.X);
    float aN3 = MathF.Atan2(n3.Y, n3.X);

    float arcAngle1 = AngleDelta(aN3, aN1);
    float arcAngle2 = AngleDelta(aN1, aN2);
    float arcAngle3 = AngleDelta(aN2, aN3);

    float l1 = e1.Length();
    float l2 = e2.Length();
    float l3 = e3.Length();

    float arcLen1 = MathF.Abs(arcAngle1) * thickness;
    float arcLen2 = MathF.Abs(arcAngle2) * thickness;
    float arcLen3 = MathF.Abs(arcAngle3) * thickness;

    float totalPerimeter = l1 + l2 + l3 + arcLen1 + arcLen2 + arcLen3;
    float targetLen = totalPerimeter * percentage;
    float remaining = targetLen;

    // Unified drawing logic starts here
    var prevOuter = p1 + ShapeVec.VecFromAngleRad(aN3 + arcAngle1 * 0.5f) * thickness;
    remaining = EmitArc(p1, aN3 + arcAngle1 * 0.5f, arcAngle1 * 0.5f, m1Inner, prevOuter, remaining, arcAngle1);
    if (remaining <= 0) return;

    remaining = EmitEdge(p1 + n1 * thickness, p2 + n1 * thickness, m1Inner, m2Inner, remaining);
    if (remaining <= 0) return;

    remaining = EmitArc(p2, aN1, arcAngle2, m2Inner, p2 + n1 * thickness, remaining, arcAngle2);
    if (remaining <= 0) return;

    remaining = EmitEdge(p2 + n2 * thickness, p3 + n2 * thickness, m2Inner, m3Inner, remaining);
    if (remaining <= 0) return;

    remaining = EmitArc(p3, aN2, arcAngle3, m3Inner, p3 + n2 * thickness, remaining, arcAngle3);
    if (remaining <= 0) return;

    remaining = EmitEdge(p3 + n3 * thickness, p1 + n3 * thickness, m3Inner, m1Inner, remaining);
    if (remaining <= 0) return;

    EmitArc(p1, aN3, arcAngle1 * 0.5f, m1Inner, p1 + n3 * thickness, remaining, arcAngle1);


    float EmitArc(Vector2 corner, float startAngle, float angle, Vector2 inner, Vector2 currentOuter, float rem, float baseArcAngle)
    {
        if (rem <= 0f || angle == 0) return rem;
        // int steps = (int)MathF.Ceiling(cornerPoints * (MathF.Abs(angle) / MathF.Abs(baseArcAngle)));
        float divisor = MathF.Abs(baseArcAngle) > 0.0001f ? MathF.Abs(baseArcAngle) : 1f;
        int steps = (int)MathF.Ceiling(cornerPoints * (MathF.Abs(angle) / divisor));
        
        if (steps <= 0) steps = 1;

        float stepAngle = angle / steps;
        float stepLen = MathF.Abs(stepAngle) * thickness;
        
        for (int i = 1; i <= steps; i++)
        {
            if (rem <= 0) break;
            var nextOuter = corner + ShapeVec.VecFromAngleRad(startAngle + stepAngle * i) * thickness;
            if (rem >= stepLen)
            {
                if(ccw) DrawTriangle(inner, currentOuter, nextOuter, color);
                else DrawTriangle(inner, nextOuter, currentOuter, color);
                rem -= stepLen;
            }
            else
            {
                float f = rem / stepLen;
                var partialOuter = currentOuter.Lerp(nextOuter, f);
                if(ccw) DrawTriangle(inner, currentOuter, partialOuter, color);
                else DrawTriangle(inner, partialOuter, currentOuter, color);
                rem = 0;
            }
            currentOuter = nextOuter;
        }
        return rem;
    }

    float EmitEdge(Vector2 start, Vector2 end, Vector2 innerStart, Vector2 innerEnd, float rem)
    {
        if (rem <= 0f) return rem;
        float len = (end - start).Length();
        if (len <= 0) return rem;

        if (rem >= len)
        {
            if (ccw)
            {
                DrawTriangle(innerStart, start, end, color);
                DrawTriangle(innerStart, end, innerEnd, color);
            }
            else
            {
                DrawTriangle(innerStart, end, start, color);
                DrawTriangle(innerStart, innerEnd, end, color);
            }
            
            rem -= len;
        }
        else
        {
            float f = rem / len;
            var pOuter = start.Lerp(end, f);
            var pInner = innerStart.Lerp(innerEnd, f);
            if (ccw)
            {
                DrawTriangle(innerStart, start, pOuter, color);
                DrawTriangle(innerStart, pOuter, pInner, color);
            }
            else
            {
                DrawTriangle(innerStart, pOuter, start, color);
                DrawTriangle(innerStart, pInner, pOuter, color);
            }
            rem = 0;
        }
        return rem;
    }

    float AngleDelta(float a0, float a1)
    {
        float d = a1 - a0;
        while (d > MathF.PI) d -= 2f * MathF.PI;
        while (d < -MathF.PI) d += 2f * MathF.PI;
        return d;
    }
}
*/

/*
private static void DrawTriangleLinesHelper(Vector2 p1, Vector2 p2, Vector2 p3, float lineThickness, ColorRgba color, int cornerPoints = 0)
{
    if (lineThickness <= 0) return;

    // Calculate maximum safe thickness based on inradius
    float maxThickness = CalculateMaxLineThickness(p1, p2, p3);
    float thickness = MathF.Min(lineThickness, maxThickness);

    // Calculate edge vectors and perpendicular normals
    var edge1 = p2 - p1;
    var edge2 = p3 - p2;
    var edge3 = p1 - p3;

    var normal1 = new Vector2(-edge1.Y, edge1.X);
    if (normal1.LengthSquared() > 0) normal1 = Vector2.Normalize(normal1);

    var normal2 = new Vector2(-edge2.Y, edge2.X);
    if (normal2.LengthSquared() > 0) normal2 = Vector2.Normalize(normal2);

    var normal3 = new Vector2(-edge3.Y, edge3.X);
    if (normal3.LengthSquared() > 0) normal3 = Vector2.Normalize(normal3);

    // Calculate inner miter points (always sharp)
    var miter1Inner = CalculateMiterPoint(p1, normal3, normal1, thickness, false);
    var miter2Inner = CalculateMiterPoint(p2, normal1, normal2, thickness, false);
    var miter3Inner = CalculateMiterPoint(p3, normal2, normal3, thickness, false);

    if (cornerPoints > 0)
    {
        // Rounded corners: use simple outer edge points, not miters
        var p1Outer = p1 + normal1 * thickness;
        var p2Outer1 = p2 + normal1 * thickness;
        var p2Outer2 = p2 + normal2 * thickness;
        var p3Outer2 = p3 + normal2 * thickness;
        var p3Outer3 = p3 + normal3 * thickness;
        var p1Outer3 = p1 + normal3 * thickness;

        // Draw straight edge segments (no miter)
        DrawEdgeQuad(miter1Inner, miter2Inner, p1Outer, p2Outer1, color);
        DrawEdgeQuad(miter2Inner, miter3Inner, p2Outer2, p3Outer2, color);
        DrawEdgeQuad(miter3Inner, miter1Inner, p3Outer3, p1Outer3, color);

        // Draw rounded corners to fill the gaps
        DrawOuterCorner(p1, normal3, normal1, miter1Inner, thickness, color, cornerPoints);
        DrawOuterCorner(p2, normal1, normal2, miter2Inner, thickness, color, cornerPoints);
        DrawOuterCorner(p3, normal2, normal3, miter3Inner, thickness, color, cornerPoints);
    }
    else
    {
        // Sharp corners: use miter points
        var miter1Outer = CalculateMiterPoint(p1, normal3, normal1, thickness, true);
        var miter2Outer = CalculateMiterPoint(p2, normal1, normal2, thickness, true);
        var miter3Outer = CalculateMiterPoint(p3, normal2, normal3, thickness, true);

        DrawEdgeQuad(miter1Inner, miter2Inner, miter1Outer, miter2Outer, color);
        DrawEdgeQuad(miter2Inner, miter3Inner, miter2Outer, miter3Outer, color);
        DrawEdgeQuad(miter3Inner, miter1Inner, miter3Outer, miter1Outer, color);
    }
}
private static Vector2 CalculateMiterPoint(Vector2 corner, Vector2 normalPrev, Vector2 normalNext, float halfThickness, bool outer)
{
    // Calculate miter direction (average of normals)
    var miterDir = Vector2.Normalize(normalPrev + normalNext);
    
    // Calculate miter length based on angle
    float dot = Vector2.Dot(normalPrev, normalNext);
    float miterLength = halfThickness / MathF.Sqrt((1f + dot) * 0.5f);
    
    return corner + miterDir * (outer ? miterLength : -miterLength);
}
private static void DrawEdgeQuad(Vector2 innerStart, Vector2 innerEnd, Vector2 outerStart, Vector2 outerEnd, ColorRgba color)
{
    DrawTriangle(innerStart, outerStart, innerEnd, color);
    DrawTriangle(outerStart, outerEnd, innerEnd, color);
}
private static void DrawOuterCorner(Vector2 corner, Vector2 normalPrev, Vector2 normalNext, Vector2 innerCorner, float halfThickness, ColorRgba color, int cornerPoints)
{
    // Calculate angle between normals
    float anglePrev = MathF.Atan2(normalPrev.Y, normalPrev.X);
    float angleNext = MathF.Atan2(normalNext.Y, normalNext.X);
    
    // Ensure we sweep in the correct direction
    float angleDiff = angleNext - anglePrev;
    if (angleDiff > MathF.PI) angleDiff -= 2 * MathF.PI;
    if (angleDiff < -MathF.PI) angleDiff += 2 * MathF.PI;

    var prevOuter = corner + normalPrev * halfThickness;

    for (int i = 1; i <= cornerPoints + 1; i++)
    {
        float t = i / (float)(cornerPoints + 1);
        float angle = anglePrev + angleDiff * t;
        var normal = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
        var curOuter = corner + normal * halfThickness;

        DrawTriangle(innerCorner, prevOuter, curOuter, color);

        prevOuter = curOuter;
    }
}
*/

/*
#region Draw Rounded

/// <summary>
/// Draws a filled triangle with rounded outer corners.
/// </summary>
/// <param name="a">The first vertex of the triangle.</param>
/// <param name="b">The second vertex of the triangle.</param>
/// <param name="c">The third vertex of the triangle.</param>
/// <param name="color">The color used to fill the triangle.</param>
/// <param name="cornerPoints">
/// Number of extra points used to approximate rounded corners.
/// 0 = sharp corners (no rounding). Higher values increase smoothness. Default: 5.
/// </param>
/// <param name="cornerStrength">
/// Controls how rounded the corners are: 0 = maximum roundness, 1 = no roundness (sharp).
/// Values between 0 and 1 interpolate between fully rounded and sharp corners. Default: 0.5.
/// </param>
public static void DrawTriangleRounded(Vector2 a, Vector2 b, Vector2 c, ColorRgba color, int cornerPoints = 5, float cornerStrength = 0.5f)
{
    DrawTriangleRoundedHelper(a, b, c, color, cornerPoints, cornerStrength); 
}
/// <summary>
/// Draws the triangle with rounded outer corners using the provided color and rounding parameters.
/// </summary>
/// <param name="t">The triangle instance to draw.</param>
/// <param name="color">The fill color used to render the rounded triangle.</param>
/// <param name="cornerPoints">
/// Number of extra points used to approximate each rounded corner.
/// 0 = sharp corners (no rounding). Higher values produce smoother rounded corners. Default: 5.
/// </param>
/// <param name="cornerStrength">
/// Controls the strength (radius) of the corner rounding:
/// 0 = maximum roundness, 1 = no roundness (sharp corners).
/// Values between 0 and 1 interpolate between fully rounded and sharp corners. Default: 0.5.
/// </param>
public static void DrawRounded(this Triangle t, ColorRgba color, int cornerPoints = 5, float cornerStrength = 0.5f)
{
    DrawTriangleRoundedHelper(t.A, t.B, t.C, color, cornerPoints, cornerStrength);
}

private static void DrawTriangleRoundedHelper(Vector2 p1, Vector2 p2, Vector2 p3, ColorRgba color, int cornerPoints, float cornerStrength)
{
    if (cornerPoints <= 0 || cornerStrength >= 1f)
    {
        DrawTriangle(p1, p2, p3, color);
        return;
    }

    // Calculate maximum safe thickness based on inradius
    float maxThickness = CalculateMaxLineThickness(p1, p2, p3);
    float thickness = maxThickness * cornerStrength; // MathF.Min(lineThickness, maxThickness);

    // Calculate edge vectors and perpendicular normals
    var edge1 = p2 - p1;
    var edge2 = p3 - p2;
    var edge3 = p1 - p3;

    var normal1 = new Vector2(-edge1.Y, edge1.X);
    if (normal1.LengthSquared() > 0) normal1 = Vector2.Normalize(normal1);

    var normal2 = new Vector2(-edge2.Y, edge2.X);
    if (normal2.LengthSquared() > 0) normal2 = Vector2.Normalize(normal2);

    var normal3 = new Vector2(-edge3.Y, edge3.X);
    if (normal3.LengthSquared() > 0) normal3 = Vector2.Normalize(normal3);

    // Calculate inner miter points (always sharp)
    var miter1Inner = CalculateMiterPoint(p1, normal3, normal1, thickness, false);
    var miter2Inner = CalculateMiterPoint(p2, normal1, normal2, thickness, false);
    var miter3Inner = CalculateMiterPoint(p3, normal2, normal3, thickness, false);

    // Rounded corners: use simple outer edge points, not miters
    var p1Outer = miter1Inner + normal1 * thickness;
    var p2Outer1 = miter2Inner + normal1 * thickness;
    var p2Outer2 = miter2Inner + normal2 * thickness;
    var p3Outer2 = miter3Inner + normal2 * thickness;
    var p3Outer3 = miter3Inner + normal3 * thickness;
    var p1Outer3 = miter1Inner + normal3 * thickness;

    // Draw straight edge segments (no miter)
    DrawEdgeQuad(miter1Inner, miter2Inner, p1Outer, p2Outer1, color);
    DrawEdgeQuad(miter2Inner, miter3Inner, p2Outer2, p3Outer2, color);
    DrawEdgeQuad(miter3Inner, miter1Inner, p3Outer3, p1Outer3, color);

    // Draw rounded corners to fill the gaps
    DrawOuterCorner(miter1Inner, normal3, normal1, miter1Inner, thickness, color, cornerPoints);
    DrawOuterCorner(miter2Inner, normal1, normal2, miter2Inner, thickness, color, cornerPoints);
    DrawOuterCorner(miter3Inner, normal2, normal3, miter3Inner, thickness, color, cornerPoints);

    //Fill center for full triangle
    DrawTriangle(miter1Inner, miter2Inner, miter3Inner, color);
    
}

#endregion
*/

/*public static void DrawLines2(this Triangle triangle, float lineThickness, ColorRgba color, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
    {
        var a = triangle.A;
        var b = triangle.B;
        var c = triangle.C;

        SegmentDrawing.DrawSegment(a, b, lineThickness, color, capType, capPoints);
        SegmentDrawing.DrawSegment(b, c, lineThickness, color, capType, capPoints);
        SegmentDrawing.DrawSegment(c, a, lineThickness, color, capType, capPoints);
    }
    public static void DrawLines2(this Triangle triangle, LineDrawingInfo lineInfo)
    {
        var a = triangle.A;
        var b = triangle.B;
        var c = triangle.C;

        SegmentDrawing.DrawSegment(a, b, lineInfo);
        SegmentDrawing.DrawSegment(b, c, lineInfo);
        SegmentDrawing.DrawSegment(c, a, lineInfo);
    }*/

    
    