
using System.Drawing;
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

//NOTE: The best way is to:
// - Remove all non-extensions methods and clean up function overloads / parameters in general (keeping it simple, less overloads)
// - Clean up regions
// - Create copies of the rounded parameters function with *Rounded suffix
// - Remove all the rounded parameters from the current functions
// - Do the same things for Rect
// - Add / Update Docs

//NOTE: Draw SlantedCorner & DrawSlantedCornerLines is very similar draw scaled.
//Q: Does DrawSlanterCorner & DrawSlantedCornerLines draw the quad with slanted corners or does it only draw the slanted corners?
// - DrawSlanted/ DrawSlantedLines for the full shape version
// - DrawSlantedCorners/ DrawSlantedCornerLines for the corner-only version

/// <summary>
/// Provides static methods for drawing quads (quadrilaterals) and their outlines, including partial outlines and vertex markers.
/// </summary>
/// <remarks>
/// This class contains utility methods for rendering quads with various options such as line thickness, color, partial outlines, and scaling.
/// </remarks>
public static class QuadDrawing
{
    #region Draw
    
    public static void Draw(this Quad q, ColorRgba color)
    {
        Raylib.DrawTriangle(q.A, q.B, q.C, color.ToRayColor());
        Raylib.DrawTriangle(q.A, q.C, q.D, color.ToRayColor());
    }
    
    #endregion
    
    #region Draw Scaled
    /// <summary>
    /// Draws a quad with scaled sides based on a specific draw type.
    /// </summary>
    /// <param name="q">The quad to draw.</param>
    /// <param name="color">The color of the drawn shape.</param>
    /// <param name="sideScaleFactor">The scale factor of the sides (0 to 1). If >= 1, the full quad is drawn. If &lt;= 0, nothing is drawn.</param>
    /// <param name="sideScaleOrigin">The origin point for scaling the sides (0 = start, 1 = end, 0.5 = center).</param>
    /// <param name="drawType">
    /// The style of drawing:
    /// <list type="bullet">
    /// <item><description>0: [Filled] Drawn as 6 filled triangles, effectivly cutting of corners.</description></item>
    /// <item><description>1: [Sides] Each side is connected to the quad's center.</description></item>
    /// <item><description>2: [Sides Inverse] The start of 1 side is connected to the end of the next side and is connected to the quad's center.</description></item>
    /// </list>
    /// </param>
    public static void DrawScaled(this Quad q, ColorRgba color, float sideScaleFactor, float sideScaleOrigin, int drawType)
    {
        if (sideScaleFactor <= 0) return;
        if (sideScaleFactor >= 1)
        {
            q.Draw(color);
            return;
        }

        var s1 = new Segment(q.A, q.B).ScaleSegment(sideScaleFactor, sideScaleOrigin);
        var s2 = new Segment(q.B, q.C).ScaleSegment(sideScaleFactor, sideScaleOrigin);
        var s3 = new Segment(q.C, q.D).ScaleSegment(sideScaleFactor, sideScaleOrigin);
        var s4 = new Segment(q.D, q.A).ScaleSegment(sideScaleFactor, sideScaleOrigin);

        var rayColor = color.ToRayColor();
        if (drawType == 0)
        {
            Raylib.DrawTriangle(s1.Start, s1.End, s2.Start, rayColor);
            Raylib.DrawTriangle(s4.End, s1.Start, s2.Start, rayColor);
            
            Raylib.DrawTriangle(s4.End, s2.Start, s4.Start, rayColor);
            Raylib.DrawTriangle(s4.Start, s2.Start, s2.End, rayColor);
            
            Raylib.DrawTriangle(s4.Start, s2.End, s3.End, rayColor);
            Raylib.DrawTriangle(s3.End, s2.End, s3.Start, rayColor);
            
        }
        else if (drawType == 1)
        {
            var center = q.Center;
            Raylib.DrawTriangle(s1.Start, s1.End, center, rayColor);
            Raylib.DrawTriangle(s2.Start, s2.End, center, rayColor);
            Raylib.DrawTriangle(s3.Start, s3.End, center, rayColor);
            Raylib.DrawTriangle(s4.Start, s4.End, center, rayColor);
        }
        else
        {
            var center = q.Center;
            Raylib.DrawTriangle(s4.End, s1.Start, center, rayColor);
            Raylib.DrawTriangle(s1.End, s2.Start, center, rayColor);
            Raylib.DrawTriangle(s2.End, s3.Start, center, rayColor);
            Raylib.DrawTriangle(s3.End, s4.Start, center, rayColor);
        }
    }
    #endregion
    
    #region Draw Lines
    
    public static void DrawLines(this Quad q, LineDrawingInfo lineInfo)
    {
        DrawLinesHelper(q.A, q.B, q.C, q.D, lineInfo.Thickness, lineInfo.Color);
    }
    
    public static void DrawLines(this Quad q, float lineThickness, ColorRgba color)
    {
        DrawLinesHelper(q.A, q.B, q.C, q.D, lineThickness, color);
    }
    
    #endregion
    
    #region Draw Lines Scaled
    
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
    
    #region Draw Lines Percentage

    public static void DrawLinesPercentage(this Quad q, float f, int startIndex, LineDrawingInfo lineInfo)
    {
        DrawLinesPercentageHelper(q.A, q.B, q.C, q.D, f, startIndex, lineInfo.Thickness, lineInfo.Color);
    }
    
    public static void DrawLinesPercentage(this Quad q, float f, int startIndex, float lineThickness, ColorRgba color)
    {
        DrawLinesPercentageHelper(q.A, q.B, q.C, q.D, f, startIndex, lineThickness, color);
    }
    #endregion
 
    #region Draw Vignette
    /// <summary>
    /// Draws a "vignette" effect inside the quad, creating a circular hole in the center.
    /// The area between the inner circle and the quad's outer edges is filled with the specified color.
    /// </summary>
    /// <param name="q">The quad to draw the vignette within.</param>
    /// <param name="circleRadius">The radius of the inner circular hole.</param>
    /// <param name="circleRotDeg">The starting rotation angle of the inner circle in degrees.</param>
    /// <param name="color">The color of the filled area.</param>
    /// <param name="circleSmoothness">
    /// Determines the smoothness of the inner circle (0.0 to 1.0). 
    /// Higher values result in more segments and a smoother circle.
    /// </param>
    public static void DrawVignette(this Quad q, float circleRadius, float circleRotDeg, ColorRgba color, float circleSmoothness = 0.5f)
    {
        if (circleRadius <= 0)
        {
            q.Draw(color);
            return;
        }

        // Clamp radius to ensure at least some vignette is drawn
        var minDimension = q.Size.Min();
        var maxRadius = minDimension * 0.5f - 1f;
        if (circleRadius > maxRadius)
        {
            circleRadius = maxRadius;
        }

        if (!CircleDrawing.CalculateCircleDrawingParameters(circleRadius, circleSmoothness, out float angleStepRad, out int segments, true)) return;

        var center = q.Center;

        // 1. Calculate Basis Vectors for local coordinate projection
        // Assuming A=TopLeft, B=BottomLeft -> A->B is Down
        // Assuming B=BottomLeft, C=BottomRight -> B->C is Right
        Vector2 rightVec = q.C - q.B;
        float width = rightVec.Length();
        if (width <= 0) return;
        Vector2 rightAxis = rightVec / width;

        Vector2 downVec = q.B - q.A;
        float height = downVec.Length();
        if (height <= 0) return;
        Vector2 downAxis = downVec / height;

        float halfWidth = width * 0.5f;
        float halfHeight = height * 0.5f;
        
        // Map side indices to actual Quad corner vertices for filling gaps.
        // Side 0 (Right)  -> Side 1 (Bottom) crosses Corner BR (C)
        // Side 1 (Bottom) -> Side 2 (Left)   crosses Corner BL (B)
        // Side 2 (Left)   -> Side 3 (Top)    crosses Corner TL (A)
        // Side 3 (Top)    -> Side 0 (Right)  crosses Corner TR (D)
        Vector2[] cornerVertices = { q.C, q.B, q.A, q.D };
        
        var rayColor = color.ToRayColor();
        var circleRotRad = circleRotDeg * ShapeMath.DEGTORAD;

        // Helper to project a direction vector onto the Quad's outer boundary
        // Returns the world position on the edge and the side index (0=Right, 1=Bottom, 2=Left, 3=Top)
        (Vector2 point, int side) ProjectToEdge(Vector2 direction)
        {
            float dotRight = Vector2.Dot(direction, rightAxis);
            float dotDown = Vector2.Dot(direction, downAxis);

            // Avoid division by zero
            float denomX = MathF.Abs(dotRight) > 1e-6f ? dotRight : 1e-6f;
            float denomY = MathF.Abs(dotDown) > 1e-6f ? dotDown : 1e-6f;

            // Calculate distance to vertical (X) and horizontal (Y) boundaries
            float tX = MathF.Abs(halfWidth / denomX);
            float tY = MathF.Abs(halfHeight / denomY);

            if (tX < tY)
            {
                // Hits vertical side
                int side = dotRight > 0 ? 0 : 2; // 0: Right, 2: Left
                return (center + direction * tX, side);
            }
            else
            {
                // Hits horizontal side
                int side = dotDown > 0 ? 1 : 3; // 1: Bottom, 3: Top
                return (center + direction * tY, side);
            }
        }

        // 2. Initialize Starting Point
        Vector2 currentDir = new Vector2(MathF.Cos(circleRotRad), MathF.Sin(circleRotRad));
        Vector2 startInner = center + currentDir * circleRadius;
        var startProjection = ProjectToEdge(currentDir);
        
        Vector2 currentInner = startInner;
        Vector2 currentOuter = startProjection.point;
        int currentSide = startProjection.side;

        // 3. Iterate Segments
        for (int i = 0; i < segments; i++)
        {
            float nextAngle = circleRotRad + angleStepRad * (i + 1);
            Vector2 nextDir = new Vector2(MathF.Cos(nextAngle), MathF.Sin(nextAngle));
            
            // Calculate Next Vertices
            Vector2 nextInner = center + nextDir * circleRadius;
            var nextProjection = ProjectToEdge(nextDir);
            Vector2 nextOuter = nextProjection.point;
            int nextSide = nextProjection.side;

            // Draw Vignette Segment (2 Triangles forming a quad)
            // CCW Order: InnerStart -> EndOuter -> StartOuter
            Raylib.DrawTriangle(currentInner, nextOuter, currentOuter, rayColor);
            // CCW Order: InnerStart -> EndInner -> EndOuter
            Raylib.DrawTriangle(currentInner, nextInner, nextOuter, rayColor);

            // Fill Corner Gap if we transitioned between sides (e.g., Right to Bottom)
            if (currentSide != nextSide)
            {
                // The corner to fill corresponds to the current side index before the switch
                Raylib.DrawTriangle(currentOuter, nextOuter, cornerVertices[currentSide], rayColor);
            }

            // Advance
            currentInner = nextInner;
            currentOuter = nextOuter;
            currentSide = nextSide;
        }
    }
    #endregion
    
    #region Draw Corners
    /// <summary>
    /// Draws the corners of the quad with independent lengths for each corner.
    /// </summary>
    /// <param name="quad">The quad to draw corners for.</param>
    /// <param name="lineThickness">The thickness of the corner lines.</param>
    /// <param name="color">The color of the corner lines.</param>
    /// <param name="tlCorner">The length of the top-left corner.</param>
    /// <param name="trCorner">The length of the top-right corner.</param>
    /// <param name="brCorner">The length of the bottom-right corner.</param>
    /// <param name="blCorner">The length of the bottom-left corner.</param>
    public static void DrawCorners(this Quad quad, float lineThickness, ColorRgba color, float tlCorner, float trCorner, float brCorner, float blCorner)
    {
        if (lineThickness <= 0f || color.A <= 0) return;
        if(tlCorner <= 0 && trCorner <= 0 && brCorner <= 0 && blCorner <= 0) return;
        
        var a = quad.A;
        var b = quad.B;
        var c = quad.C;
        var d = quad.D;
        
        float w = (d - a).Length();
        float h = (b - a).Length();
        if(w <= 0 || h <= 0) return;

        var nL = (a - d).Normalize();
        var nD = (b - a).Normalize();
        var nR = (c - b).Normalize();
        var nU = (d - c).Normalize();
        
        float halfWidth = w * 0.5f;
        float halfHeight = h * 0.5f;
        
        bool lineThicknessBiggerThanWidthOrHeight = lineThickness >= halfWidth || lineThickness >= halfHeight;
        var rayColor = color.ToRayColor();
        
        if (tlCorner > 0f)
        {
            var cornerLength = tlCorner;
            if (lineThicknessBiggerThanWidthOrHeight)
            {
                var innerW = MathF.Min(MathF.Max(cornerLength, lineThickness), halfWidth);
                var innerH = MathF.Min(MathF.Max(cornerLength, lineThickness), halfHeight);
                
                var newA = a + nU * lineThickness + nL * lineThickness;
                var newB = a + nD * innerH + nL * lineThickness;
                var newC = a + nD * innerH + nR * innerW;
                var newD = a + nU * lineThickness + nR * innerW;
                Raylib.DrawTriangle(newA, newB, newC, rayColor);
                Raylib.DrawTriangle(newA, newC, newD, rayColor);
            }
            else if (cornerLength < lineThickness)
            {
                //just draw a square over the corner
                var tl = a + nU * lineThickness + nL * lineThickness;
                var bl = a + nD * lineThickness + nL * lineThickness;
                var br = a + nD * lineThickness + nR * lineThickness;
                var tr = a + nU * lineThickness + nR * lineThickness;
                TriangleDrawing.DrawTriangle(tl, bl, tr, rayColor);
                TriangleDrawing.DrawTriangle(tr, bl, br, rayColor);
                
            }
            else
            {
                var cornerLengthH = MathF.Min(cornerLength, halfWidth);
                var cornerLengthV = MathF.Min(cornerLength, halfHeight);
                
                var outer = a + nU * lineThickness + nL * lineThickness;
                var outerH = a + nU * lineThickness + nR * cornerLengthH;
                var outerV = a + nD * cornerLengthV + nL * lineThickness;
                var inner = a + nD * lineThickness + nR * lineThickness;
                var innerH = a + nD * lineThickness + nR * cornerLengthH;
                var innerV = a + nD * cornerLengthV + nR * lineThickness;
                
                TriangleDrawing.DrawTriangle(outer, inner, outerH, rayColor);
                TriangleDrawing.DrawTriangle(inner, innerH, outerH, rayColor);
                TriangleDrawing.DrawTriangle(outer, outerV, inner, rayColor);
                TriangleDrawing.DrawTriangle(inner, outerV, innerV, rayColor);
                
            }
        }

        if (trCorner > 0f)
        {
            var cornerLength = trCorner;
            if (lineThicknessBiggerThanWidthOrHeight)
            {
                var innerW = MathF.Min(MathF.Max(cornerLength, lineThickness), halfWidth);
                var innerH = MathF.Min(MathF.Max(cornerLength, lineThickness), halfHeight);
                
                var newA = d + nU * lineThickness + nL * innerW;
                var newB = d + nD * innerH + nL * innerW;
                var newC = d + nD * innerH + nR * lineThickness;
                var newD = d + nU * lineThickness + nR * lineThickness;
                Raylib.DrawTriangle(newA, newB, newC, rayColor);
                Raylib.DrawTriangle(newA, newC, newD, rayColor);
            }
            else if (cornerLength < lineThickness)
            {
                //just draw a square over the corner
                var tl = d + nU * lineThickness + nL * lineThickness;
                var bl = d + nD * lineThickness + nL * lineThickness;
                var br = d + nD * lineThickness + nR * lineThickness;
                var tr = d + nU * lineThickness + nR * lineThickness;
                TriangleDrawing.DrawTriangle(tl, bl, tr, rayColor);
                TriangleDrawing.DrawTriangle(tr, bl, br, rayColor);
                
            }
            else
            {
                var cornerLengthH = MathF.Min(cornerLength, halfWidth);
                var cornerLengthV = MathF.Min(cornerLength, halfHeight);
                
                var outer = d + nU * lineThickness + nR * lineThickness;
                var outerH = d + nU * lineThickness + nL * cornerLengthH;
                var outerV = d + nD * cornerLengthV + nR * lineThickness;
                var inner = d + nD * lineThickness + nL * lineThickness;
                var innerH = d + nD * lineThickness + nL * cornerLengthH;
                var innerV = d + nD * cornerLengthV + nL * lineThickness;
                
                TriangleDrawing.DrawTriangle(outerH, inner, outer, rayColor);
                TriangleDrawing.DrawTriangle(outerH, innerH, inner, rayColor);
                TriangleDrawing.DrawTriangle(outer, inner, outerV, rayColor);
                TriangleDrawing.DrawTriangle(inner, innerV, outerV, rayColor);
                
            }
        }

        if (brCorner > 0f)
        {
            var cornerLength = brCorner;
            if (lineThicknessBiggerThanWidthOrHeight)
            {
                var innerW = MathF.Min(MathF.Max(cornerLength, lineThickness), halfWidth);
                var innerH = MathF.Min(MathF.Max(cornerLength, lineThickness), halfHeight);
                
                var newA = c + nU * innerH + nL * innerW;
                var newB = c + nD * lineThickness + nL * innerW;
                var newC = c + nD * lineThickness + nR * lineThickness;
                var newD = c + nU * innerH + nR * lineThickness;
                Raylib.DrawTriangle(newA, newB, newC, rayColor);
                Raylib.DrawTriangle(newA, newC, newD, rayColor);
            }
            else if (cornerLength < lineThickness)
            {
                //just draw a square over the corner
                var tl = c + nU * lineThickness + nL * lineThickness;
                var bl = c + nD * lineThickness + nL * lineThickness;
                var br = c + nD * lineThickness + nR * lineThickness;
                var tr = c + nU * lineThickness + nR * lineThickness;
                TriangleDrawing.DrawTriangle(tl, bl, tr, rayColor);
                TriangleDrawing.DrawTriangle(tr, bl, br, rayColor);
                
            }
            else
            {
                var cornerLengthH = MathF.Min(cornerLength, halfWidth);
                var cornerLengthV = MathF.Min(cornerLength, halfHeight);
                
                var outer = c + nD * lineThickness + nR * lineThickness;
                var outerH = c + nD * lineThickness + nL * cornerLengthH;
                var outerV = c + nU * cornerLengthV + nR * lineThickness;
                var inner = c + nU * lineThickness + nL * lineThickness;
                var innerH = c + nU * lineThickness + nL * cornerLengthH;
                var innerV = c + nU * cornerLengthV + nL * lineThickness;
                
                TriangleDrawing.DrawTriangle(outerV, inner, outer, rayColor);
                TriangleDrawing.DrawTriangle(outerV, innerV, inner, rayColor);
                TriangleDrawing.DrawTriangle(outerH, outer, inner, rayColor);
                TriangleDrawing.DrawTriangle(inner, innerH, outerH, rayColor);
                
            }
        }

        if (blCorner > 0f)
        {
            var cornerLength = blCorner;
            
            if (lineThicknessBiggerThanWidthOrHeight)
            {
                var innerW = MathF.Min(MathF.Max(cornerLength, lineThickness), halfWidth);
                var innerH = MathF.Min(MathF.Max(cornerLength, lineThickness), halfHeight);
                
                var newA = b + nU * innerH + nL * lineThickness;
                var newB = b + nD * lineThickness + nL * lineThickness;
                var newC = b + nD * lineThickness + nR * innerW;
                var newD = b + nU * innerH + nR * innerW;
                Raylib.DrawTriangle(newA, newB, newC, rayColor);
                Raylib.DrawTriangle(newA, newC, newD, rayColor);
            }
            else if (cornerLength < lineThickness)
            {
                //just draw a square over the corner
                var tl = b + nU * lineThickness + nL * lineThickness;
                var bl = b + nD * lineThickness + nL * lineThickness;
                var br = b + nD * lineThickness + nR * lineThickness;
                var tr = b + nU * lineThickness + nR * lineThickness;
                TriangleDrawing.DrawTriangle(tl, bl, tr, rayColor);
                TriangleDrawing.DrawTriangle(tr, bl, br, rayColor);
                
            }
            else
            {
                var cornerLengthH = MathF.Min(cornerLength, halfWidth);
                var cornerLengthV = MathF.Min(cornerLength, halfHeight);
                
                var outer = b + nD * lineThickness + nL * lineThickness;
                var outerH = b + nD * lineThickness + nR * cornerLengthH;
                var outerV = b + nU * cornerLengthV + nL * lineThickness;
                var inner = b + nU * lineThickness + nR * lineThickness;
                var innerH = b + nU * lineThickness + nR * cornerLengthH;
                var innerV = b + nU * cornerLengthV + nR * lineThickness;
                
                TriangleDrawing.DrawTriangle(outerV, outer, inner, rayColor);
                TriangleDrawing.DrawTriangle(outerV, inner, innerV, rayColor);
                TriangleDrawing.DrawTriangle(outer, outerH, inner, rayColor);
                TriangleDrawing.DrawTriangle(inner, outerH, innerH, rayColor);
                
            }
        }
    }
    
    /// <summary>
    /// Draws all corners of the quad with the same length.
    /// </summary>
    /// <param name="quad">The quad to draw corners for.</param>
    /// <param name="lineThickness">The thickness of the corner lines.</param>
    /// <param name="color">The color of the corner lines.</param>
    /// <param name="cornerLength">The length of the corner segments.</param>
    public static void DrawCorners(this Quad quad, float lineThickness, ColorRgba color, float cornerLength)
    {
        if (lineThickness <= 0f || color.A <= 0 || cornerLength <= 0) return;
        
        var a = quad.A;
        var b = quad.B;
        var c = quad.C;
        var d = quad.D;
        
        float w = (d - a).Length();
        float h = (b - a).Length();
        if(w <= 0 || h <= 0) return;

        var nL = (a - d).Normalize();
        var nD = (b - a).Normalize();
        var nR = (c - b).Normalize();
        var nU = (d - c).Normalize();
        
        float halfWidth = w * 0.5f;
        float halfHeight = h * 0.5f;
        bool widthDominant = w > h;
        float minHalf = widthDominant ? halfHeight : halfWidth;
        float maxHalf = widthDominant ? halfWidth : halfHeight;
        float maxCorner = MathF.Max(lineThickness, cornerLength);
        var rayColor = color.ToRayColor();
        
        if (lineThickness >= minHalf)
        {
            if (lineThickness >= maxHalf)
            {
                var newA = a + nU * lineThickness + nL * lineThickness;
                var newB = b + nD * lineThickness + nL * lineThickness;
                var newC = c + nD * lineThickness + nR * lineThickness;
                var newD = d + nU * lineThickness + nR * lineThickness;
                Raylib.DrawTriangle(newA, newB, newC, rayColor);
                Raylib.DrawTriangle(newA, newC, newD, rayColor);
            }
            else
            {
                if (widthDominant)
                {
                    if (maxCorner >= halfWidth)
                    {
                        var newA = a + nU * lineThickness + nL * lineThickness;
                        var newB = b + nD * lineThickness + nL * lineThickness;
                        var newC = c + nD * lineThickness + nR * lineThickness;
                        var newD = d + nU * lineThickness + nR * lineThickness;
                        Raylib.DrawTriangle(newA, newB, newC, rayColor);
                        Raylib.DrawTriangle(newA, newC, newD, rayColor);
                    }
                    else
                    {
                        var newA = a + nU * lineThickness + nL * lineThickness;
                        var newB = b + nD * lineThickness + nL * lineThickness;
                        var newC = newB + nR * (lineThickness + maxCorner);
                        var newD = newA + nR * (lineThickness + maxCorner);
                        
                        Raylib.DrawTriangle(newA, newB, newC, rayColor);
                        Raylib.DrawTriangle(newA, newC, newD, rayColor);
                        
                        newC = c + nD * lineThickness + nR * lineThickness;
                        newD = d + nU * lineThickness + nR * lineThickness;
                        newA = newD + nL * (lineThickness + maxCorner);
                        newB = newC + nL * (lineThickness + maxCorner);
                        
                        Raylib.DrawTriangle(newA, newB, newC, rayColor);
                        Raylib.DrawTriangle(newA, newC, newD, rayColor);
                    }
                }
                else
                {
                    if (maxCorner >= halfHeight)
                    {
                        var newA = a + nU * lineThickness + nL * lineThickness;
                        var newB = b + nD * lineThickness + nL * lineThickness;
                        var newC = c + nD * lineThickness + nR * lineThickness;
                        var newD = d + nU * lineThickness + nR * lineThickness;
                        Raylib.DrawTriangle(newA, newB, newC, rayColor);
                        Raylib.DrawTriangle(newA, newC, newD, rayColor);
                    }
                    else
                    {
                        var newA = a + nU * lineThickness + nL * lineThickness;
                        var newD = d + nU * lineThickness + nR * lineThickness;
                        var newB = newA + nD * (lineThickness + maxCorner);
                        var newC = newD + nD * (lineThickness + maxCorner);
                        
                        Raylib.DrawTriangle(newA, newB, newC, rayColor);
                        Raylib.DrawTriangle(newA, newC, newD, rayColor);
                        
                        newB = b + nD * lineThickness + nL * lineThickness;
                        newC = c + nD * lineThickness + nR * lineThickness;
                        newA = newB + nU * (lineThickness + maxCorner);
                        newD = newC + nU * (lineThickness + maxCorner);
                        
                        Raylib.DrawTriangle(newA, newB, newC, rayColor);
                        Raylib.DrawTriangle(newA, newC, newD, rayColor);
                    }
                }
            }
        }
        else
        {
            if (cornerLength < lineThickness)
            {
                //just draw a squares over each corner
                
                //tl
                var tl = a + nU * lineThickness + nL * lineThickness;
                var bl = a + nD * lineThickness + nL * lineThickness;
                var br = a + nD * lineThickness + nR * lineThickness;
                var tr = a + nU * lineThickness + nR * lineThickness;
                TriangleDrawing.DrawTriangle(tl, bl, tr, rayColor);
                TriangleDrawing.DrawTriangle(tr, bl, br, rayColor);
                
                //tr
                tl = d + nU * lineThickness + nL * lineThickness;
                bl = d + nD * lineThickness + nL * lineThickness;
                br = d + nD * lineThickness + nR * lineThickness;
                tr = d + nU * lineThickness + nR * lineThickness;
                TriangleDrawing.DrawTriangle(tl, bl, tr, rayColor);
                TriangleDrawing.DrawTriangle(tr, bl, br, rayColor);
                
                //br
                tl = c + nU * lineThickness + nL * lineThickness;
                bl = c + nD * lineThickness + nL * lineThickness;
                br = c + nD * lineThickness + nR * lineThickness;
                tr = c + nU * lineThickness + nR * lineThickness;
                TriangleDrawing.DrawTriangle(tl, bl, tr, rayColor);
                TriangleDrawing.DrawTriangle(tr, bl, br, rayColor);
                
                //bl
                tl = b + nU * lineThickness + nL * lineThickness;
                bl = b + nD * lineThickness + nL * lineThickness;
                br = b + nD * lineThickness + nR * lineThickness;
                tr = b + nU * lineThickness + nR * lineThickness;
                TriangleDrawing.DrawTriangle(tl, bl, tr, rayColor);
                TriangleDrawing.DrawTriangle(tr, bl, br, rayColor);
                    
            }
            else
            {
                //tl
                var cornerLengthH = MathF.Min(cornerLength, halfWidth);
                var cornerLengthV = MathF.Min(cornerLength, halfHeight);
                    
                var outer = a + nU * lineThickness + nL * lineThickness;
                var outerH = a + nU * lineThickness + nR * cornerLengthH;
                var outerV = a + nD * cornerLengthV + nL * lineThickness;
                var inner = a + nD * lineThickness + nR * lineThickness;
                var innerH = a + nD * lineThickness + nR * cornerLengthH;
                var innerV = a + nD * cornerLengthV + nR * lineThickness;
                    
                TriangleDrawing.DrawTriangle(outer, inner, outerH, rayColor);
                TriangleDrawing.DrawTriangle(inner, innerH, outerH, rayColor);
                TriangleDrawing.DrawTriangle(outer, outerV, inner, rayColor);
                TriangleDrawing.DrawTriangle(inner, outerV, innerV, rayColor);
                
                //tr
                outer = d + nU * lineThickness + nR * lineThickness;
                outerH = d + nU * lineThickness + nL * cornerLengthH;
                outerV = d + nD * cornerLengthV + nR * lineThickness;
                inner = d + nD * lineThickness + nL * lineThickness;
                innerH = d + nD * lineThickness + nL * cornerLengthH;
                innerV = d + nD * cornerLengthV + nL * lineThickness;
                    
                TriangleDrawing.DrawTriangle(outerH, inner, outer, rayColor);
                TriangleDrawing.DrawTriangle(outerH, innerH, inner, rayColor);
                TriangleDrawing.DrawTriangle(outer, inner, outerV, rayColor);
                TriangleDrawing.DrawTriangle(inner, innerV, outerV, rayColor);
                
                //br
                outer = c + nD * lineThickness + nR * lineThickness;
                outerH = c + nD * lineThickness + nL * cornerLengthH;
                outerV = c + nU * cornerLengthV + nR * lineThickness;
                inner = c + nU * lineThickness + nL * lineThickness;
                innerH = c + nU * lineThickness + nL * cornerLengthH;
                innerV = c + nU * cornerLengthV + nL * lineThickness;
                    
                TriangleDrawing.DrawTriangle(outerV, inner, outer, rayColor);
                TriangleDrawing.DrawTriangle(outerV, innerV, inner, rayColor);
                TriangleDrawing.DrawTriangle(outerH, outer, inner, rayColor);
                TriangleDrawing.DrawTriangle(inner, innerH, outerH, rayColor);
                
                //bl
                outer = b + nD * lineThickness + nL * lineThickness;
                outerH = b + nD * lineThickness + nR * cornerLengthH;
                outerV = b + nU * cornerLengthV + nL * lineThickness;
                inner = b + nU * lineThickness + nR * lineThickness;
                innerH = b + nU * lineThickness + nR * cornerLengthH;
                innerV = b + nU * cornerLengthV + nR * lineThickness;
                    
                TriangleDrawing.DrawTriangle(outerV, outer, inner, rayColor);
                TriangleDrawing.DrawTriangle(outerV, inner, innerV, rayColor);
                TriangleDrawing.DrawTriangle(outer, outerH, inner, rayColor);
                TriangleDrawing.DrawTriangle(inner, outerH, innerH, rayColor);
                    
            }
        }
    }
    #endregion
    
    #region Draw Corners Relative
    /// <summary>
    /// Draws the corners of the quad with independent lengths relative to the quad's minimum dimension.
    /// </summary>
    /// <param name="quad">The quad to draw corners for.</param>
    /// <param name="lineThickness">The thickness of the corner lines.</param>
    /// <param name="color">The color of the corner lines.</param>
    /// <param name="tlCornerFactor">Factor (0-1) for the top-left corner length relative to the quad's minimum size.</param>
    /// <param name="trCornerFactor">Factor (0-1) for the top-right corner length relative to the quad's minimum size.</param>
    /// <param name="brCornerFactor">Factor (0-1) for the bottom-right corner length relative to the quad's minimum size.</param>
    /// <param name="blCornerFactor">Factor (0-1) for the bottom-left corner length relative to the quad's minimum size.</param>
    public static void DrawCornersRelative(this Quad quad, float lineThickness, ColorRgba color, float tlCornerFactor, float trCornerFactor, float brCornerFactor, float blCornerFactor)
    {
        float minSize = quad.GetSize().Min();
        quad.DrawCorners(lineThickness, color, tlCornerFactor * minSize, trCornerFactor * minSize, brCornerFactor * minSize, blCornerFactor * minSize);
    }
    
    /// <summary>
    /// Draws all corners of the quad with the same length relative to the quad's minimum dimension.
    /// </summary>
    /// <param name="quad">The quad to draw corners for.</param>
    /// <param name="lineThickness">The thickness of the corner lines.</param>
    /// <param name="color">The color of the corner lines.</param>
    /// <param name="cornerLengthFactor">Factor (0-1) for the corner length relative to the quad's minimum size.</param>
    public static void DrawCornersRelative(this Quad quad, float lineThickness, ColorRgba color, float cornerLengthFactor)
    {
        quad.DrawCornersRelative(lineThickness, color, cornerLengthFactor, cornerLengthFactor, cornerLengthFactor, cornerLengthFactor);
    }
    #endregion
    
    #region Draw Chamfered Corners
    /// <summary>
    /// Draws the quad with chamfered corners of equal length.
    /// </summary>
    /// <param name="quad">The quad to draw.</param>
    /// <param name="color">The fill color of the shape.</param>
    /// <param name="cornerLength">The length of the slant for all corners.</param>
    public static void DrawChamferedCorners(this Quad quad, ColorRgba color, float cornerLength)
    {
        DrawChamferedCorners(quad, color, cornerLength, cornerLength);
    }
    
    /// <summary>
    /// Draws the quad with chamfered corners with separate horizontal and vertical slant lengths.
    /// </summary>
    /// <param name="quad">The quad to draw.</param>
    /// <param name="color">The fill color of the shape.</param>
    /// <param name="cornerLengthHorizontal">The horizontal length of the slant.</param>
    /// <param name="cornerLengthVertical">The vertical length of the slant.</param>
    public static void DrawChamferedCorners(this Quad quad, ColorRgba color, float cornerLengthHorizontal, float cornerLengthVertical)
    {
        var size = quad.GetSize();
        if(size.Width <= 0 || size.Height <= 0) return;
        if (cornerLengthHorizontal <= 0 || cornerLengthVertical <= 0)
        {
            quad.Draw(color);
            return;
        }

        float halfWidth = size.Width / 2f;
        float halfHeight = size.Height / 2f;
        
        var nD = quad.NormalDown;
        var nR = quad.NormalRight;
        var nL = -nR;
        var nU = -nD;
        
        var tl = quad.TopLeft;
        var br = quad.BottomRight;
        
        if (cornerLengthHorizontal >= halfWidth && cornerLengthVertical >= halfHeight)
        {
            var p1 = tl + nR * halfWidth;
            var p2 = tl + nD * halfHeight;
            var p3 = br + nL * halfWidth;
            var p4 = br + nU * halfHeight;
            TriangleDrawing.DrawTriangle(p1, p2, p3, color);
            TriangleDrawing.DrawTriangle(p1, p3, p4, color);
            return;
        }
        
        var bl = quad.BottomLeft;
        var tr = quad.TopRight;


        if (cornerLengthHorizontal >= halfWidth)
        {
            var top = tl + nR * halfWidth;
            var bottom = bl + nR * halfWidth;
            var tlV = tl + nD * cornerLengthVertical;
            var blV = bl + nU * cornerLengthVertical;
            var brV = br + nU * cornerLengthVertical;
            var trV = tr + nD * cornerLengthVertical;
            
            TriangleDrawing.DrawTriangle(top, tlV, blV, color);
            TriangleDrawing.DrawTriangle(top, blV, bottom, color);
            
            TriangleDrawing.DrawTriangle(top, bottom, brV, color);
            TriangleDrawing.DrawTriangle(top, brV, trV, color);
        }
        else if (cornerLengthVertical >= halfHeight)
        {
            var left = tl + nD * halfHeight;
            var right = tr + nD * halfHeight;
            var tlH = tl + nR * cornerLengthHorizontal;
            var blH = bl + nR * cornerLengthHorizontal;
            var brH = br + nL * cornerLengthHorizontal;
            var trH = tr + nL * cornerLengthHorizontal;
            
            TriangleDrawing.DrawTriangle(tlH, left, blH, color);
            TriangleDrawing.DrawTriangle(tlH, blH, trH, color);
            
            TriangleDrawing.DrawTriangle(trH, blH, brH, color);
            TriangleDrawing.DrawTriangle(trH, brH, right, color);
        }
        else
        {
            
            var tlH = tl + nR * cornerLengthHorizontal;
            var tlV = tl + nD * cornerLengthVertical;
        
            var blV = bl + nU * cornerLengthVertical;
            var blH = bl + nR * cornerLengthHorizontal;
        
            var brH = br + nL * cornerLengthHorizontal;
            var brV = br + nU * cornerLengthVertical;
       
            var trV = tr + nD * cornerLengthVertical;
            var trH = tr + nL * cornerLengthHorizontal;

            //left triangles
            TriangleDrawing.DrawTriangle(tlV, blV, tlH, color);
            TriangleDrawing.DrawTriangle(tlH, blV, blH, color);
        
            //center triangles
            TriangleDrawing.DrawTriangle(tlH, blH, trH, color);
            TriangleDrawing.DrawTriangle(trH, blH, brH, color);
        
            //right triangles
            TriangleDrawing.DrawTriangle(trH, brH, trV, color);
            TriangleDrawing.DrawTriangle(trV, brH, brV, color);
        }
    }
    
    /// <summary>
    /// Draws the quad with independently chamfered corners.
    /// </summary>
    /// <param name="quad">The quad to draw.</param>
    /// <param name="color">The fill color of the shape.</param>
    /// <param name="tlCorner">The slant length for the top-left corner.</param>
    /// <param name="trCorner">The slant length for the top-right corner.</param>
    /// <param name="brCorner">The slant length for the bottom-right corner.</param>
    /// <param name="blCorner">The slant length for the bottom-left corner.</param>
    public static void DrawChamferedCorners(this Quad quad, ColorRgba color, float tlCorner, float blCorner, float brCorner, float trCorner)
    {
        if (tlCorner <= 0 && trCorner <= 0 && brCorner <= 0 && blCorner <= 0)
        {
            quad.Draw(color);
            return;
        }
        
        Vector2 nU = quad.NormalUp;
        Vector2 nR = quad.NormalRight;
        
        Vector2 center = quad.Center;
        
        var size = quad.GetSize();
        var halfWidth = size.Width * 0.5f;
        var halfHeight = size.Height * 0.5f;
        
        Vector2 prev, start;
        bool startMaxed, prevMaxed;
        
        var rayColor = color.ToRayColor();
        
        if (tlCorner > 0)
        {
            var cornerLengthH = MathF.Min(tlCorner, halfWidth);
            var cornerLengthV = MathF.Min(tlCorner, halfHeight);
            Vector2 a = quad.TopLeft + nR * cornerLengthH;
            Vector2 b = quad.TopLeft - nU * cornerLengthV;

            start = a;
            prev = b;
            
            Raylib.DrawTriangle(a, b, center, rayColor);

            startMaxed = tlCorner >= halfWidth;
            prevMaxed = tlCorner >= halfHeight;
        }
        else
        {
            Vector2 b = quad.TopLeft;
            Vector2 a = b + nR * halfWidth;
            Vector2 c = b - nU * halfHeight;
            
            start = a;
            prev = c;
            
            Raylib.DrawTriangle(a, b, c, rayColor);
            Raylib.DrawTriangle(a, c, center, rayColor);
            
            startMaxed = true;
            prevMaxed = true;
        }
        
        if (blCorner > 0)
        {
            var cornerLengthH = MathF.Min(blCorner, halfWidth);
            var cornerLengthV = MathF.Min(blCorner, halfHeight);
            Vector2 a = quad.BottomLeft + nU * cornerLengthV;
            Vector2 b = quad.BottomLeft + nR * cornerLengthH;

            if (!prevMaxed || blCorner < halfHeight)
            {
                Raylib.DrawTriangle(prev, a, center, rayColor);
            }
            
            prevMaxed = blCorner >= halfWidth;
            
            prev = b;
            
            Raylib.DrawTriangle(a, b, center, rayColor);
        }
        else
        {
            Vector2 b = quad.BottomLeft;
            Vector2 a = b + nU * halfHeight;
            Vector2 c = b + nR * halfWidth;

            if (!prevMaxed)
            {
                Raylib.DrawTriangle(prev, a, center, rayColor);
            }

            prevMaxed = true;
            
            prev = c;
            
            Raylib.DrawTriangle(a, b, c, rayColor);
            Raylib.DrawTriangle(a, c, center, rayColor);
        }

        if (brCorner > 0)
        {
            var cornerLengthH = MathF.Min(brCorner, halfWidth);
            var cornerLengthV = MathF.Min(brCorner, halfHeight);
            Vector2 a = quad.BottomRight - nR * cornerLengthH;
            Vector2 b = quad.BottomRight + nU * cornerLengthV;

            if (!prevMaxed || brCorner < halfWidth)
            {
                Raylib.DrawTriangle(prev, a, center, rayColor);
            }
            
            prevMaxed = brCorner >= halfHeight;
            
            prev = b;
            
            Raylib.DrawTriangle(a, b, center, rayColor);
        }
        else
        {
            Vector2 b = quad.BottomRight;
            Vector2 a = b - nR * halfWidth;
            Vector2 c = b + nU * halfHeight;

            if (!prevMaxed)
            {
                Raylib.DrawTriangle(prev, a, center, rayColor);
            }

            prevMaxed = true;
            
            prev = c;
            
            Raylib.DrawTriangle(a, b, c, rayColor);
            Raylib.DrawTriangle(a, c, center, rayColor);
        }
        
        if (trCorner > 0)
        {
            var cornerLengthH = MathF.Min(trCorner, halfWidth);
            var cornerLengthV = MathF.Min(trCorner, halfHeight);
            Vector2 a = quad.TopRight - nU * cornerLengthV;
            Vector2 b = quad.TopRight - nR * cornerLengthH;

            if (!prevMaxed || trCorner < halfHeight)
            {
                Raylib.DrawTriangle(prev, a, center, rayColor);
            }
            
            Raylib.DrawTriangle(a, b, center, rayColor);
            
            if(!startMaxed || trCorner < halfWidth)
            {
                Raylib.DrawTriangle(b, start, center, rayColor);
            }
        }
        else
        {
            Vector2 b = quad.TopRight;
            Vector2 a = b - nU * halfHeight;
            Vector2 c = b - nR * halfWidth;

            if (!prevMaxed)
            {
                Raylib.DrawTriangle(prev, a, center, rayColor);
            }
            
            Raylib.DrawTriangle(a, b, c, rayColor);
            Raylib.DrawTriangle(a, c, center, rayColor);

            if (!startMaxed)
            {
                Raylib.DrawTriangle(c, start, center, rayColor);
            }
        }
    }
    
    #endregion

    #region Draw Chamfered Corners Relative
    /// <summary>
    /// Draws the quad with slanted (chamfered) corners relative to the quad's dimensions.
    /// </summary>
    /// <param name="quad">The quad to draw.</param>
    /// <param name="color">The fill color of the shape.</param>
    /// <param name="cornerLengthFactor">The slant factor (0-1) relative to half the quad's size.</param>
    public static void DrawChamferedCornersRelative(this Quad quad, ColorRgba color, float cornerLengthFactor)
    {
        var size = quad.GetSize();
        if(size.Width <= 0 || size.Height <= 0) return;
        if (cornerLengthFactor <= 0)
        {
            quad.Draw(color);
            return;
        }
        
        float halfWidth = size.Width / 2f;
        float halfHeight = size.Height / 2f;

        if (cornerLengthFactor >= 1f) cornerLengthFactor = 1f;
        DrawChamferedCorners(quad, color, halfWidth * cornerLengthFactor, halfHeight * cornerLengthFactor);
    }
    
    /// <summary>
    /// Draws the quad with slanted (chamfered) corners relative to the quad's dimensions, specifying horizontal and vertical factors.
    /// </summary>
    /// <param name="quad">The quad to draw.</param>
    /// <param name="color">The fill color of the shape.</param>
    /// <param name="cornerLengthFactorHorizontal">The horizontal slant factor (0-1) relative to half the quad's width.</param>
    /// <param name="cornerLengthFactorVertical">The vertical slant factor (0-1) relative to half the quad's height.</param>
    public static void DrawChamferedCornersRelative(this Quad quad, ColorRgba color, float cornerLengthFactorHorizontal, float cornerLengthFactorVertical)
    {
        var size = quad.GetSize();
        if(size.Width <= 0 || size.Height <= 0) return;
        if (cornerLengthFactorHorizontal <= 0 && cornerLengthFactorVertical <= 0)
        {
            quad.Draw(color);
            return;
        }

        if (cornerLengthFactorHorizontal >= 1f) cornerLengthFactorHorizontal = 1f;
        if(cornerLengthFactorVertical >= 1f) cornerLengthFactorVertical = 1f;
        
        float cornerLengthH = cornerLengthFactorHorizontal * size.Width * 0.5f;
        float cornerLengthV = cornerLengthFactorVertical * size.Height * 0.5f;
        DrawChamferedCorners(quad, color, cornerLengthH, cornerLengthV);
    }

    /// <summary>
    /// Draws the quad with independently slanted (chamfered) corners relative to the quad's dimensions.
    /// </summary>
    /// <param name="quad">The quad to draw.</param>
    /// <param name="color">The fill color of the shape.</param>
    /// <param name="tlCornerFactor">Top-left corner slant factor (0-1) relative to half the quad's width.</param>
    /// <param name="trCornerFactor">Top-right corner slant factor (0-1) relative to half the quad's width.</param>
    /// <param name="brCornerFactor">Bottom-right corner slant factor (0-1) relative to half the quad's width.</param>
    /// <param name="blCornerFactor">Bottom-left corner slant factor (0-1) relative to half the quad's width.</param>
    public static void DrawChamferedCornersRelative(this Quad quad, ColorRgba color, float tlCornerFactor, float blCornerFactor, float brCornerFactor, float trCornerFactor)
    {
        if (tlCornerFactor <= 0 && trCornerFactor <= 0 && brCornerFactor <= 0 && blCornerFactor <= 0)
        {
            quad.Draw(color);
            return;
        }

        if(tlCornerFactor >= 1f) tlCornerFactor = 1f;
        if(trCornerFactor >= 1f) trCornerFactor = 1f;
        if(brCornerFactor >= 1f) brCornerFactor = 1f;
        if(blCornerFactor >= 1f) blCornerFactor = 1f;
        
        Vector2 nU = quad.NormalUp;
        Vector2 nR = quad.NormalRight;
        
        Vector2 center = quad.Center;
        
        var size = quad.GetSize();
        var halfWidth = size.Width * 0.5f;
        var halfHeight = size.Height * 0.5f;
        
        Vector2 prev, start;
        bool startMaxed, prevMaxed;
        
        var rayColor = color.ToRayColor();
        
        if (tlCornerFactor > 0)
        {
            var cornerLengthH = tlCornerFactor * halfWidth;
            var cornerLengthV = tlCornerFactor * halfHeight;
            Vector2 a = quad.TopLeft + nR * cornerLengthH;
            Vector2 b = quad.TopLeft - nU * cornerLengthV;

            start = a;
            prev = b;
            
            Raylib.DrawTriangle(a, b, center, rayColor);

            startMaxed = prevMaxed = tlCornerFactor >= 1f;
        }
        else
        {
            Vector2 b = quad.TopLeft;
            Vector2 a = b + nR * halfWidth;
            Vector2 c = b - nU * halfHeight;
            
            start = a;
            prev = c;
            
            Raylib.DrawTriangle(a, b, c, rayColor);
            Raylib.DrawTriangle(a, c, center, rayColor);
            
            startMaxed = true;
            prevMaxed = true;
        }
        
        if (blCornerFactor > 0)
        {
            var cornerLengthH = blCornerFactor * halfWidth;
            var cornerLengthV = blCornerFactor * halfHeight;
            Vector2 a = quad.BottomLeft + nU * cornerLengthV;
            Vector2 b = quad.BottomLeft + nR * cornerLengthH;

            if (!prevMaxed || blCornerFactor < 1f)
            {
                Raylib.DrawTriangle(prev, a, center, rayColor);
            }
            
            prevMaxed = blCornerFactor >= 1f;
            
            prev = b;
            
            Raylib.DrawTriangle(a, b, center, rayColor);
        }
        else
        {
            Vector2 b = quad.BottomLeft;
            Vector2 a = b + nU * halfHeight;
            Vector2 c = b + nR * halfWidth;

            if (!prevMaxed)
            {
                Raylib.DrawTriangle(prev, a, center, rayColor);
            }

            prevMaxed = true;
            
            prev = c;
            
            Raylib.DrawTriangle(a, b, c, rayColor);
            Raylib.DrawTriangle(a, c, center, rayColor);
        }

        if (brCornerFactor > 0)
        {
            var cornerLengthH = brCornerFactor * halfWidth;
            var cornerLengthV = brCornerFactor * halfHeight;
            Vector2 a = quad.BottomRight - nR * cornerLengthH;
            Vector2 b = quad.BottomRight + nU * cornerLengthV;

            if (!prevMaxed || brCornerFactor < 1f)
            {
                Raylib.DrawTriangle(prev, a, center, rayColor);
            }
            
            prevMaxed = brCornerFactor >= 1f;
            
            prev = b;
            
            Raylib.DrawTriangle(a, b, center, rayColor);
        }
        else
        {
            Vector2 b = quad.BottomRight;
            Vector2 a = b - nR * halfWidth;
            Vector2 c = b + nU * halfHeight;

            if (!prevMaxed)
            {
                Raylib.DrawTriangle(prev, a, center, rayColor);
            }

            prevMaxed = true;
            
            prev = c;
            
            Raylib.DrawTriangle(a, b, c, rayColor);
            Raylib.DrawTriangle(a, c, center, rayColor);
        }
        
        if (trCornerFactor > 0)
        {
            var cornerLengthH = trCornerFactor * halfWidth;
            var cornerLengthV = trCornerFactor * halfHeight;
            Vector2 a = quad.TopRight - nU * cornerLengthV;
            Vector2 b = quad.TopRight - nR * cornerLengthH;

            if (!prevMaxed || trCornerFactor < 1f)
            {
                Raylib.DrawTriangle(prev, a, center, rayColor);
            }
            
            Raylib.DrawTriangle(a, b, center, rayColor);
            
            if(!startMaxed || trCornerFactor < 1f)
            {
                Raylib.DrawTriangle(b, start, center, rayColor);
            }
        }
        else
        {
            Vector2 b = quad.TopRight;
            Vector2 a = b - nU * halfHeight;
            Vector2 c = b - nR * halfWidth;

            if (!prevMaxed)
            {
                Raylib.DrawTriangle(prev, a, center, rayColor);
            }
            
            Raylib.DrawTriangle(a, b, c, rayColor);
            Raylib.DrawTriangle(a, c, center, rayColor);

            if (!startMaxed)
            {
                Raylib.DrawTriangle(c, start, center, rayColor);
            }
        }
    }
    #endregion
    
    

    #region Draw Chamfered Corners Lines
    //TODO: Docs
    public static void DrawChamferedCornersLines(this Quad quad, float lineThickness, ColorRgba color, float cornerLength)
    {
        if (cornerLength <= 0 && lineThickness <= 0)
        {
            quad.Draw(color);
            return;
        }
        
        if (cornerLength <= 0)
        {
            quad.DrawLines(lineThickness, ColorRgba.AliceBlue);
            return;
        }

        if (lineThickness <= 0)
        {
            quad.DrawChamferedCorners(color, cornerLength);
            return;
        }
        
        var size = quad.GetSize();
        if(size.Width <= 0 || size.Height <= 0) return;
        
        float halfWidth = size.Width * 0.5f;
        float halfHeight = size.Height * 0.5f;
        
        lineThickness = MathF.Min(lineThickness, MathF.Min(halfWidth, halfHeight));
        var cornerLengthW = MathF.Min(cornerLength, halfWidth);
        var cornerLengthH = MathF.Min(cornerLength, halfHeight);
        
        var nR = quad.NormalRight;
        var nD = quad.NormalDown;
        var nL = -nR;
        var nU = -nD;
        
        var rayColor = color.ToRayColor();
            
        var aPrev = quad.A + nR * cornerLengthW;
        var aNext = quad.A + nD * cornerLengthH;
        var chamferDirA = (aNext - aPrev).Normalize().GetPerpendicularRight();
        var chamferDirAPrev = (chamferDirA + nU).Normalize();
        var chamferDirANext = (chamferDirA + nL).Normalize();

        var bPrev = quad.B + nU * cornerLengthH;
        var bNext = quad.B + nR * cornerLengthW;
        var chamferDirB = (bNext - bPrev).Normalize().GetPerpendicularRight();
        var chamferDirBPrev = (chamferDirB + nL).Normalize();
        var chamferDirBNext = (chamferDirB + nD).Normalize();

        var cPrev = quad.C + nL * cornerLengthW;
        var cNext = quad.C + nU * cornerLengthH;
        var chamferDirCPrev = -chamferDirAPrev;
        var chamferDirCNext = -chamferDirANext;

        var dPrev = quad.D + nD * cornerLengthH;
        var dNext = quad.D + nL * cornerLengthW;
        var chamferDirDPrev = -chamferDirBPrev;
        var chamferDirDNext = -chamferDirBNext; 

        var angleRad = ShapeVec.AngleRad(chamferDirANext, nL);
        var chamferLength = GetHypotenuseLength(lineThickness, angleRad);
        
        var baseLengthCorner = (aPrev - aNext).Length();
        var maxChamferLength = GetIsoscelesSideLength(baseLengthCorner, chamferDirANext, chamferDirAPrev);
        
        var baseLengthHeight = (aNext - bPrev).Length();
        var baseLengthWidth = (bNext - cPrev).Length();
        var maxChamferLengthHeight = GetIsoscelesSideLength(baseLengthHeight, chamferDirANext, chamferDirBPrev);
        var maxChamferLengthWidth = GetIsoscelesSideLength(baseLengthWidth, chamferDirBNext, chamferDirCPrev);
        
        bool insideCornerClamped = chamferLength > maxChamferLength;
        
        bool widthEdgeClamped = chamferLength > maxChamferLengthWidth;
        bool heightEdgeClamped = chamferLength > maxChamferLengthHeight;
        
        var aPrevOuter = aPrev + chamferDirAPrev * chamferLength;
        var aNextOuter = aNext + chamferDirANext * chamferLength;
        
        var bPrevOuter = bPrev + chamferDirBPrev * chamferLength;
        var bNextOuter = bNext + chamferDirBNext * chamferLength;

        var cPrevOuter = cPrev + chamferDirCPrev * chamferLength;
        var cNextOuter = cNext + chamferDirCNext * chamferLength;

        var dPrevOuter = dPrev + chamferDirDPrev * chamferLength;
        var dNextOuter = dNext + chamferDirDNext * chamferLength;
        
        
        Vector2 aPrevInner, aNextInner, bPrevInner, bNextInner, cPrevInner, cNextInner, dPrevInner, dNextInner;
        if (insideCornerClamped)
        {
            var aChamferCenter = (aPrev + aNext) * 0.5f;
            var bChamferCenter = (bPrev + bNext) * 0.5f;
            var cChamferCenter = (cPrev + cNext) * 0.5f;
            var dChamferCenter = (dPrev + dNext) * 0.5f;
            
            aPrevInner = aChamferCenter - chamferDirA * lineThickness;
            aNextInner = aPrevInner;
            
            bPrevInner = bChamferCenter - chamferDirB * lineThickness;
            bNextInner = bPrevInner;
            
            cPrevInner = cChamferCenter + chamferDirA * lineThickness;
            cNextInner = cPrevInner;
            
            dPrevInner = dChamferCenter + chamferDirB * lineThickness;
            dNextInner = dPrevInner;
            
            
        }
        else if (widthEdgeClamped || heightEdgeClamped)
        {
            if (heightEdgeClamped)
            {
                var abL = (bPrev - aNext).Length();
                var dir = (aPrev - aNext).Normalize();
                var rad = dir.AngleRad(nR);
                var miterLength = Triangle.RightTriangleGetHypotenuseFromOpposite(rad, lineThickness);
                var miterLength2 = Triangle.RightTriangleGetOppositeFromAdjacent(rad, abL / 2f);
                miterLength = miterLength - miterLength2;
                miterLength = MathF.Min(miterLength, halfWidth);
                //a to b
                var abCenter = (aNext + bPrev) * 0.5f;
                aNextInner = abCenter + nR * miterLength;
                bPrevInner = aNextInner;

                //c to d
                var cdCenter = (cNext + dPrev) * 0.5f;
                cNextInner = cdCenter + nL * miterLength;
                dPrevInner = cNextInner;
            }
            else
            {
                //a to b
                aNextInner = aNext - chamferDirANext * chamferLength;
                bPrevInner = bPrev - chamferDirBPrev * chamferLength;
                
                //c to d
                cNextInner = cNext - chamferDirCNext * chamferLength;
                dPrevInner = dPrev - chamferDirDPrev * chamferLength;
            }
            
            if (widthEdgeClamped)
            {
                var bcL = (cPrev - bNext).Length();
                var dir = (bPrev - bNext).Normalize();
                var rad = dir.AngleRad(nU);
                var miterLength = Triangle.RightTriangleGetHypotenuseFromOpposite(rad, lineThickness);
                var miterLength2 = Triangle.RightTriangleGetOppositeFromAdjacent(rad, bcL / 2f);
                miterLength = miterLength - miterLength2;
                miterLength = MathF.Min(miterLength, halfHeight);
                
                //b to c
                var bcCenter = (bNext + cPrev) * 0.5f;
                bNextInner = bcCenter + nU * miterLength;
                cPrevInner = bNextInner;

                
                //d to a
                var daCenter = (dNext + aPrev) * 0.5f;
                dNextInner = daCenter + nD * miterLength;
                aPrevInner = dNextInner;
            }
            else
            {
                //b to c
                bNextInner = bNext - chamferDirBNext * chamferLength;
                cPrevInner = cPrev - chamferDirCPrev * chamferLength;
                
                //d to a
                aPrevInner = aPrev - chamferDirAPrev * chamferLength;
                dNextInner = dNext - chamferDirDNext * chamferLength;
            }
        }
        else
        {
            aPrevInner = aPrev - chamferDirAPrev * chamferLength;
            aNextInner = aNext - chamferDirANext * chamferLength;
            bPrevInner = bPrev - chamferDirBPrev * chamferLength;
            bNextInner = bNext - chamferDirBNext * chamferLength;
            cPrevInner = cPrev - chamferDirCPrev * chamferLength;
            cNextInner = cNext - chamferDirCNext * chamferLength;
            dPrevInner = dPrev - chamferDirDPrev * chamferLength;
            dNextInner = dNext - chamferDirDNext * chamferLength;
            
        }
        
        //Draw corners
        if (insideCornerClamped)
        {
            //a corner
            Raylib.DrawTriangle(aPrevInner, aPrevOuter, aNextOuter, rayColor);
            
            //b corner
            Raylib.DrawTriangle(bPrevInner, bPrevOuter, bNextOuter, rayColor);
            
            //c corner
            Raylib.DrawTriangle(cPrevInner, cPrevOuter, cNextOuter, rayColor);
            
            //d corner
            Raylib.DrawTriangle(dPrevInner, dPrevOuter, dNextOuter, rayColor);
        }
        else
        {
            //a corner
            Raylib.DrawTriangle(aPrevOuter, aNextOuter, aPrevInner, rayColor);
            Raylib.DrawTriangle(aPrevInner, aNextOuter, aNextInner, rayColor);
        
            //b corner
            Raylib.DrawTriangle(bPrevOuter, bNextOuter, bPrevInner, rayColor);
            Raylib.DrawTriangle(bPrevInner, bNextOuter, bNextInner, rayColor);
        
            //c corner
            Raylib.DrawTriangle(cPrevOuter, cNextOuter, cPrevInner, rayColor);
            Raylib.DrawTriangle(cPrevInner, cNextOuter, cNextInner, rayColor);
        
            //d corner
            Raylib.DrawTriangle(dPrevOuter, dNextOuter, dPrevInner, rayColor);
            Raylib.DrawTriangle(dPrevInner, dNextOuter, dNextInner, rayColor);
        }

        //Draw Edges
        if (heightEdgeClamped || widthEdgeClamped)
        {
            if (heightEdgeClamped && widthEdgeClamped)
            {
                //all edges are affected
                
                //edge a to b
                Raylib.DrawTriangle(aNextInner, aNextOuter, bPrevOuter, rayColor);
                
                //edge c to d
                Raylib.DrawTriangle(cNextInner, cNextOuter, dPrevOuter, rayColor);
                
                //edge b to c
                Raylib.DrawTriangle(bNextInner, bNextOuter, cPrevOuter, rayColor);
                
                //edge d to a
                Raylib.DrawTriangle(dNextInner, dNextOuter, aPrevOuter, rayColor);
            }
            else if (heightEdgeClamped)
            {
                //a to b and c to d is affected
                
                //edge a to b
                Raylib.DrawTriangle(aNextInner, aNextOuter, bPrevOuter, rayColor);
                
                //edge c to d
                Raylib.DrawTriangle(cNextInner, cNextOuter, dPrevOuter, rayColor);
                
                //edge b to c
                Raylib.DrawTriangle(bNextOuter, cPrevInner, bNextInner, rayColor);
                Raylib.DrawTriangle(bNextOuter, cPrevOuter, cPrevInner, rayColor);
                
                //edge d to a
                Raylib.DrawTriangle(dNextOuter, aPrevInner, dNextInner, rayColor);
                Raylib.DrawTriangle(dNextOuter, aPrevOuter, aPrevInner, rayColor); 
            }
            else
            {
                //b to c and d to a is affected
                
                //edge b to c
                Raylib.DrawTriangle(bNextInner, bNextOuter, cPrevOuter, rayColor);
                
                //edge d to a
                Raylib.DrawTriangle(dNextInner, dNextOuter, aPrevOuter, rayColor);
                
                //edge a to b
                Raylib.DrawTriangle(aNextOuter, bPrevInner, aNextInner, rayColor);
                Raylib.DrawTriangle(aNextOuter, bPrevOuter, bPrevInner, rayColor);
                
                //edge c to d
                Raylib.DrawTriangle(cNextOuter, dPrevInner, cNextInner, rayColor);
                Raylib.DrawTriangle(cNextOuter, dPrevOuter, dPrevInner, rayColor);
            }
            
        }
        else
        {
           //edge a to b
           Raylib.DrawTriangle(aNextOuter, bPrevInner, aNextInner, rayColor);
           Raylib.DrawTriangle(aNextOuter, bPrevOuter, bPrevInner, rayColor);
           
           //edge b to c
           Raylib.DrawTriangle(bNextOuter, cPrevInner, bNextInner, rayColor);
           Raylib.DrawTriangle(bNextOuter, cPrevOuter, cPrevInner, rayColor);
           
           //edge c to d
           Raylib.DrawTriangle(cNextOuter, dPrevInner, cNextInner, rayColor);
           Raylib.DrawTriangle(cNextOuter, dPrevOuter, dPrevInner, rayColor);
           
           //edge d to a
           Raylib.DrawTriangle(dNextOuter, aPrevInner, dNextInner, rayColor);
           Raylib.DrawTriangle(dNextOuter, aPrevOuter, aPrevInner, rayColor); 
        }
        
        float GetHypotenuseLength(float adjacentLength, float angleRadians)
        {
            // Ensure the angle is not 90 degrees (PI/2) to avoid division by zero
            // Hypotenuse = Adjacent / Cos(angle)
            return adjacentLength / MathF.Cos(angleRadians);
        }
        
        float GetIsoscelesSideLength(float baseLength, Vector2 dir1, Vector2 dir2)
        {
            float dot = Vector2.Dot(dir1, dir2);
            // Identity: 2 * sin(theta/2) = sqrt(2 * (1 - cos(theta)))
            float denom = MathF.Sqrt(2f * (1f - dot)); 
            
            // Avoid division by zero
            if (denom < 0.0001f) return 0;
            
            return baseLength / denom;
        }
        
        float GetIsoscelesBaseLength(float sideLength, Vector2 dir1, Vector2 dir2)
        {
            float dot = Vector2.Dot(dir1, dir2);
            // Identity: 2 * sin(theta/2) = sqrt(2 * (1 - cos(theta)))
            float denom = MathF.Sqrt(2f * (1f - dot)); 
            
            return sideLength * denom;
        }
    }
    
    //TODO: Docs
    public static void DrawChamferedCornersLines(this Quad quad, float lineThickness, ColorRgba color, float cornerLengthHorizontal, float cornerLengthVertical)
    {
        var size = quad.GetSize();
        if(size.Width <= 0 || size.Height <= 0) return;
        if (cornerLengthHorizontal <= 0 || cornerLengthVertical <= 0)
        {
            quad.Draw(color);
            return;
        }

        float halfWidth = size.Width / 2f;
        float halfHeight = size.Height / 2f;
        
        var tl = quad.TopLeft;
        var br = quad.BottomRight;
        
        var nD = quad.NormalDown;
        var nR = quad.NormalRight;
        var nL = -nR;
        var nU = -nD;
        
        if (cornerLengthHorizontal >= halfWidth && cornerLengthVertical >= halfHeight)
        {
            polygonHelper.Clear();
            polygonHelper.Add(tl + nR * halfWidth);
            polygonHelper.Add(tl + nD * halfHeight);
            polygonHelper.Add(br + nL * halfWidth);
            polygonHelper.Add(br + nU * halfHeight);
            polygonHelper.DrawLines(lineThickness, color);
            return;
        }
        
        var bl = quad.BottomLeft;
        var tr = quad.TopRight;

        polygonHelper.Clear();
        
        if (cornerLengthHorizontal >= halfWidth)
        {
            polygonHelper.Add(tl + nR * halfWidth);
            polygonHelper.Add(tl + nD * cornerLengthVertical);
            polygonHelper.Add(bl + nU * cornerLengthVertical);
            polygonHelper.Add(bl + nR * halfWidth);
            polygonHelper.Add(br + nU * halfWidth);
            polygonHelper.Add(tr + nD * halfWidth);
        }
        else if (cornerLengthVertical >= halfHeight)
        {
            polygonHelper.Add(tl + nR * cornerLengthHorizontal);
            polygonHelper.Add(tl + nD * halfHeight);
            polygonHelper.Add(bl + nR * cornerLengthHorizontal);
            polygonHelper.Add(br + nL * cornerLengthHorizontal);
            polygonHelper.Add(tr + nD * halfHeight);
            polygonHelper.Add(tr + nL * cornerLengthHorizontal);
        }
        else
        {
            polygonHelper.Add(tl + nR * cornerLengthHorizontal);
            polygonHelper.Add(tl + nD * cornerLengthVertical);
            polygonHelper.Add(bl + nU * cornerLengthVertical);
            polygonHelper.Add(bl + nR * cornerLengthHorizontal);
            polygonHelper.Add(br + nL * cornerLengthHorizontal);
            polygonHelper.Add(br + nU *  cornerLengthVertical);
            polygonHelper.Add(tr + nD * cornerLengthVertical);
            polygonHelper.Add(tr + nL * cornerLengthHorizontal);
        }
    
        //TODO: Use DrawLinesConvex?
        polygonHelper.DrawLines(lineThickness, color);
    }
    
    //TODO: Docs
    public static void DrawChamferedCornersLines(this Quad quad, float lineThickness, ColorRgba color, float tlCorner, float blCorner, float brCorner, float trCorner)
    {
        if(tlCorner <= 0f && trCorner <= 0f && brCorner <= 0f && blCorner <= 0f)
        {
            quad.DrawLines(lineThickness, color);
            return;
        }
        //TODO: Use DrawLinesConvex?
        polygonHelper.Clear();
        FillSlantedCornerPoints(quad, tlCorner, trCorner, brCorner, blCorner, ref polygonHelper);
        polygonHelper.DrawLines(lineThickness, color);
    }
    
    #endregion

    #region Draw Chamfered Corners Relative Lines
    //NOTE: Correct (still needs testing)
    
    //TODO: Docs
    public static void DrawChamferedCornersRelativeLines(this Quad quad, float lineThickness, ColorRgba color, float cornerLengthFactor)
    {
        var size = quad.GetSize();
        if(size.Width <= 0 || size.Height <= 0) return;
        if (cornerLengthFactor <= 0)
        {
            quad.DrawLines(lineThickness, color);
            return;
        }
        
        float halfWidth = size.Width / 2f;
        float halfHeight = size.Height / 2f;

        if (cornerLengthFactor >= 1f) cornerLengthFactor = 1f;
        DrawChamferedCornersLines(quad, lineThickness, color, halfWidth * cornerLengthFactor, halfHeight * cornerLengthFactor);
    }
    
    //NOTE: Correct (still needs testing)
    
    //TODO: Docs
    public static void DrawChamferedCornersRelativeLines(this Quad quad, float lineThickness, ColorRgba color, float cornerLengthFactorHorizontal, float cornerLengthFactorVertical)
    {
        var size = quad.GetSize();
        if(size.Width <= 0 || size.Height <= 0) return;
        if (cornerLengthFactorHorizontal <= 0 || cornerLengthFactorVertical <= 0)
        {
            quad.DrawLines(lineThickness, color);
            return;
        }
        float halfWidth = size.Width / 2f;
        float halfHeight = size.Height / 2f;
        if(cornerLengthFactorHorizontal >= 1f) cornerLengthFactorHorizontal = 1f;
        if(cornerLengthFactorVertical >= 1f) cornerLengthFactorVertical = 1f;
        float cornerLengthH = cornerLengthFactorHorizontal * halfWidth;
        float cornerLengthV = cornerLengthFactorVertical * halfHeight;
        DrawChamferedCornersLines(quad, lineThickness, color, cornerLengthH, cornerLengthV);
    }

    //TODO: Docs
    public static void DrawChamferedCornersRelativeLines(this Quad quad, float lineThickness, ColorRgba color, float tlCornerFactor, float blCornerFactor, float brCornerFactor, float trCornerFactor)
    {
        //TODO: Implement
    }
    #endregion
    
    
    
    #region Draw Vertices
    /// <summary>
    /// Draws circles at each vertex of a <see cref="Quad"/>.
    /// </summary>
    /// <param name="q">The quad whose vertices to draw.</param>
    /// <param name="vertexRadius">The radius of each vertex circle.</param>
    /// <param name="color">The color of the vertex circles.</param>
    /// <param name="smoothness">
    /// The smoothness value (0-1). This controls the visual quality of the circle by inversely interpolating the current <see cref="CircleDrawing.CircleSideLengthRange"/>.
    /// A value of 0 uses the maximum side length (fewer sides, less smooth), while 1 uses the minimum side length (more sides, smoother).
    /// The resulting side length determines the number of polygon sides used to approximate the circle.
    /// </param>
    /// <remarks>
    /// Useful for visualizing or highlighting the corners of a quad.
    /// </remarks>
    public static void DrawVertices(this Quad q, float vertexRadius, ColorRgba color, float smoothness = 0.5f)
    {
        var circle = new Circle(q.A, vertexRadius);
        circle.Draw(color, smoothness);
        circle = circle.SetPosition(q.B);
        circle.Draw(color, smoothness);
        circle = circle.SetPosition(q.C);
        circle.Draw(color, smoothness);
        circle = circle.SetPosition(q.D);
        circle.Draw(color, smoothness);
    }
    #endregion
    
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
    
    
    #region Helper
    
    //TODO: Rework and Check if it can be implemented easier
    //TODO: Does not properly check if lineThickness is to big for the inside of the quad!
    private static void DrawLinesHelper(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float lineThickness, ColorRgba color)
    {
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
    
    //TODO: Rework and Check if it can be implemented easier    
    private static void DrawLinesPercentageHelper(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, float percentage, int startIndex, float lineThickness, ColorRgba color)
    {
        if (percentage == 0f || lineThickness <= 0) return;
        var order = GetDrawLinePercentageOrder(p1, p2, p3, p4, percentage, startIndex);
        if(order.p <= 0f) return;
        if(order.p >= 1f)
        {
            DrawLinesHelper(p1, p2, p3, p4, lineThickness, color);
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
    
    //TODO: Possibly Remove
    private static (Vector2 a, Vector2 b, Vector2 c, Vector2 d, float p, bool ccw) GetDrawLinePercentageOrder(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float percentage, int startIndex)
    {
        if (percentage == 0f) return (a, b, c, d, 0f, true);
        bool ccw = true;
        if (percentage < 0f)
        {
            percentage *= -1f;
            ccw = false;
        }
        float perc = ShapeMath.Clamp(percentage, 0f, 1f);
        var corner = ShapeMath.WrapI(startIndex, 0, 4);
        
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
    


    //TODO: Check all functions that use polygonHelper if it is still needed
    private static Polygon polygonHelper = new(12);
    private static void FillSlantedCornerPoints(Quad quad, float tlCorner, float trCorner, float brCorner, float blCorner, ref Polygon points)
    {
        var size = quad.GetSize();
        
        if(size.Width <= 0 || size.Height <= 0) return;
        
        float halfWidth = size.Width / 2f;
        float halfHeight = size.Height / 2f;

        var nD = quad.NormalDown;
        var nR = quad.NormalRight;
        var nL = -nR;
        var nU = -nD;
        
        if (tlCorner <= 0)
        {
            points.Add(quad.TopLeft);
        }
        else
        {
            points.Add(quad.TopLeft + nR * MathF.Min(tlCorner, halfWidth));
            points.Add(quad.TopLeft + nD * MathF.Min(tlCorner, halfHeight));
        }
        
        if (blCorner <= 0)
        {
            points.Add(quad.BottomLeft);
        }
        else
        {
            if (blCorner < halfHeight)
            {
                points.Add(quad.BottomLeft + nU * MathF.Min(blCorner, halfHeight));
            }
            
            if (blCorner < halfWidth)
            {
                points.Add(quad.BottomLeft + nR * MathF.Min(blCorner, halfWidth));
            }
        }
        
        if (brCorner <= 0)
        {
            points.Add(quad.BottomRight);
        }
        else
        {
            points.Add(quad.BottomRight + nL * MathF.Min(brCorner, halfWidth));
            points.Add(quad.BottomRight + nU * MathF.Min(brCorner, halfHeight));
        }
        
        if (trCorner <= 0)
        {
            points.Add(quad.TopRight);
        }
        else
        {
            if (trCorner < halfHeight)
            {
                points.Add(quad.TopRight + nD * MathF.Min(trCorner, halfHeight));
            }
            
            if (trCorner < halfWidth)
            {
                points.Add(quad.TopRight + nL * MathF.Min(trCorner, halfWidth));
            }
        }
    }
    
    
    #endregion
    
}


 /*#region Rounded
    public static void DrawRounded(this Quad q, ColorRgba color, float roundness = 0f, int cornerPoints = 0)
    {
        DrawRoundedHelper(q.A, q.B, q.C, q.D, roundness, cornerPoints, color);
    }
    public static void DrawRoundedLines(this Quad q, float lineThickness, ColorRgba color, float roundness = 0, int cornerPoints = 0)
    {
        DrawRoundedLinesHelper(q.A, q.B, q.C, q.D, lineThickness, color, roundness, cornerPoints);
    }
    public static void DrawRoundedLines(this Quad q, LineDrawingInfo lineInfo, float roundness = 0, int cornerPoints = 0)
    {
        DrawRoundedLinesHelper(q.A, q.B, q.C, q.D, lineInfo.Thickness, lineInfo.Color, roundness, lineInfo.CapPoints);
    }
    public static void DrawRoundedLinesPercentage(this Quad q, float f, int startIndex, LineDrawingInfo lineInfo, float roundness, int cornerPoints)
    {
        DrawRoundedLinesPercentageHelper(q.A, q.B, q.C, q.D, f, startIndex, lineInfo.Thickness, lineInfo.Color, roundness, cornerPoints);
    }
    

    
    private static void DrawRoundedLinesPercentageHelper(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, float percentage, int startIndex, float lineThickness, ColorRgba color, float roundness = 0, int cornerPoints = 0)
    {
        if (percentage == 0f || lineThickness <= 0) return;
        
        if(roundness <= 0 || cornerPoints <= 0)
        {
            DrawLinesPercentageHelper(p1, p2, p3, p4, percentage, startIndex, lineThickness, color);
            return;
        }
        
        var order = GetDrawLinePercentageOrder(p1, p2, p3, p4, percentage, startIndex);
        if(order.p <= 0f) return;
        if(order.p >= 1f)
        {
            DrawRoundedLinesHelper(p1, p2, p3, p4, lineThickness, color, roundness, cornerPoints);
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
    
        private static readonly Vector2[] centerHelper = new Vector2[4];
    private static readonly float[] cornerStartAnglesHelper = new float[4];
    private static readonly List<Vector2> innerPointsHelper = [];
    private static readonly List<Vector2> outerPointsHelper = [];
    private static void DrawRoundedLinesHelper(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, float lineThick, ColorRgba color, float roundness, int segments)
    {
        //using raylibs C DrawRectangleRoundedLinesEx implementation as a base

        if (lineThick < 0f)
        {
            // lineThick = 0f;
            return;
        }
        
        if (roundness <= 0f)
        {
            DrawLinesHelper(p1, p2, p3, p4, lineThick, color);
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
                var circle = new Circle(center, size1 * 0.5f);
                float smoothness = CircleDrawing.CircleSideLengthRange.Inverse(segments);
                circle.Draw(color, smoothness);
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
    #endregion*/
    
 /*
private static void DrawCorner(Vector2 p, Vector2 n1, Vector2 n2, float cornerLength1, float cornerLength2, float thickness, float miterLength, ColorRgba color)
{
 var miterDir = (n1 + n2).Normalize();
 var maxMiterLength = MathF.Sqrt(cornerLength1 * cornerLength1 + cornerLength2 * cornerLength2);
 miterLength = MathF.Min(miterLength, maxMiterLength);

 var innerMiter = p - miterDir * miterLength;
 var end1 = p - n1 * cornerLength1;
 var end2 = p - n2 * cornerLength2;

 var end1Inner = end1 - n2 * thickness;
 var end2Inner = end2 - n1 * thickness;

 var outerMiter = p + miterDir * miterLength;
 var end1Outer = end1 + n2 * thickness;
 var end2Outer = end2 + n1 * thickness;

 TriangleDrawing.DrawTriangle(outerMiter, end1Outer, innerMiter, color);
 TriangleDrawing.DrawTriangle(end1Outer, end1Inner, innerMiter, color);
 TriangleDrawing.DrawTriangle(outerMiter, innerMiter, end2Outer, color);
 TriangleDrawing.DrawTriangle(innerMiter, end2Inner, end2Outer, color);
}
*/