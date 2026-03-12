
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
using ChamferPoints = (System.Numerics.Vector2 Prev, System.Numerics.Vector2 Next);

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
    
    private readonly struct ChamferedCorner
    {
        #region Fields

        public readonly bool SharpCorner;
        public readonly Vector2 Corner;
        public readonly Vector2 Prev;
        public readonly Vector2 Next;
        public readonly Vector2 ChamferDir;
        public readonly Vector2 ChamferEdgeDir;
        public readonly Vector2 ChamferDirPrev;
        public readonly Vector2 ChamferDirNext;
        #endregion
        
        #region Constructors
        public ChamferedCorner(Vector2 p, float cornerLength, Vector2 nW, Vector2 nH)
        {
            Corner = p;
            if (cornerLength <= 0f)
            {
                Prev = p;
                Next = p;
                ChamferEdgeDir = Vector2.Zero;
                ChamferDir = -(nW + nH).Normalize();
                ChamferDirPrev = ChamferDir;
                ChamferDirNext = ChamferDir;
                SharpCorner = true;
            }
            else
            {
                Prev = p + nW * cornerLength;
                Next = p + nH * cornerLength;
                ChamferEdgeDir = (Next - Prev).Normalize();
                ChamferDir = ChamferEdgeDir.GetPerpendicularRight();
                ChamferDirPrev = (ChamferDir + (-nH)).Normalize();
                ChamferDirNext = (ChamferDir + (-nW)).Normalize();
                SharpCorner = false;
            }
            
        }
        public ChamferedCorner(Vector2 p, float cornerLengthW, float cornerLengthH, Vector2 nW, Vector2 nH)
        {
            Corner = p;
            Prev = p + nW * cornerLengthW;
            Next = p + nH * cornerLengthH;
            ChamferEdgeDir = (Next - Prev).Normalize();
            ChamferDir = ChamferEdgeDir.GetPerpendicularRight();
            ChamferDirPrev = (ChamferDir + (-nH)).Normalize();
            ChamferDirNext = (ChamferDir + (-nW)).Normalize();
            SharpCorner = false;
        }
        
        public ChamferedCorner(Vector2 p, float cornerLengthW, float cornerLengthH, Vector2 nW, Vector2 nH, Vector2 chamferDirPrev, Vector2 chamferDirNext)
        {
            Corner = p;
            Prev = p + nW * cornerLengthW;
            Next = p + nH * cornerLengthH;
            ChamferEdgeDir = (Next - Prev).Normalize();
            ChamferDir = ChamferEdgeDir.GetPerpendicularRight();
            ChamferDirPrev = chamferDirPrev;
            ChamferDirNext = chamferDirNext;
            SharpCorner = false;
        }
        #endregion
        
        #region Functions
        public Vector2 ChamferCenter => (Prev + Next) * 0.5f;
        public float GetCornerLength => (Next - Prev).Length();
        public float GetMaxChamferLength => SharpCorner ? float.MaxValue : Triangle.GetIsoscelesSideLength(GetCornerLength, ChamferDirNext, ChamferDirPrev);

        public Vector2 GetCenterTo(ChamferedCorner other) => (Next + other.Prev) * 0.5f;
        public float GetMaxChamferLengthTo(ChamferedCorner other, bool cornerClamped)
        {
            if (SharpCorner || other.SharpCorner) return 0f;
            if (cornerClamped)
            {
                var result = Geometry.RayDef.Ray.IntersectRayRay(ChamferCenter, -ChamferDir, other.Prev, -other.ChamferDirPrev);
                if (result.Valid)
                {
                    // return (result.Point - ChamferCenter).Length();
                    return (result.Point - other.Prev).Length();
                }

                return 0f;
                // float baseLength = (ChamferCenter - other.Prev).Length();
                // return Triangle.GetIsoscelesSideLength(baseLength, -ChamferDir, -other.ChamferDirPrev);
            }
            else
            {
                float baseLength = GetLengthTo(other);
                return Triangle.GetIsoscelesSideLength(baseLength, ChamferDirNext, other.ChamferDirPrev);
            }
            
        }
        public float GetChamferLength(Vector2 normal, float lineThickness)
        {
            if (SharpCorner)
            {
                return MathF.Sqrt(2f * lineThickness * lineThickness);
            }
            var angleRad = ShapeVec.AngleRad(ChamferDirNext, normal);
            return Triangle.RightTriangleGetHypotenuseFromAdjacent(angleRad, lineThickness);
        }
        public float GetLengthTo(ChamferedCorner other)
        {
            return (Next - other.Prev).Length();
        }
        public ChamferPoints GetOuterPoints(float chamferLength)
        {
            if (SharpCorner)
            {
                var outer = Prev + ChamferDir * chamferLength;
                return (outer, outer);
            }
            var outerPrev = Prev + ChamferDirPrev * chamferLength;
            var outerNext = Next + ChamferDirNext * chamferLength;
            return (outerPrev, outerNext);
        }
        public ChamferPoints GetOuterPoints(float chamferLengthH, float chamferLengthV)
        {
            var outerPrev = Prev + ChamferDirPrev * chamferLengthH;
            var outerNext = Next + ChamferDirNext * chamferLengthV;
            return (outerPrev, outerNext);
        }
        public Vector2 GetInnerPrev(float chamferLength) => Prev - ChamferDirPrev * chamferLength;
        public Vector2 GetInnerNext(float chamferLength) => Next - ChamferDirNext * chamferLength;
        public Vector2 GetInnerTo(ChamferedCorner other, float chamferLength, Vector2 dir)
        {
            var center = GetCenterTo(other);
            return center + dir * chamferLength;
        }
        public ChamferPoints GetInnerPoints(float chamferLength)
        {
            if (SharpCorner)
            {
                var p = Next - ChamferDir * chamferLength;
                return (p, p);
            }
            return (GetInnerPrev(chamferLength), GetInnerNext(chamferLength));
        }

        public ChamferPoints GetInnerPoints(float chamferLengthH, float chamferLengthV) => (GetInnerPrev(chamferLengthH), GetInnerNext(chamferLengthV));
        public ChamferPoints GetInnerPointsClamped(float chamferLength)
        {
            // var center = SharpCorner ? Next : ChamferCenter;
            // var prev = center - ChamferDir * chamferLength;
            // return (prev, prev);
            
            var prev = Corner - ChamferDir * chamferLength;
            return (prev, prev);
        }
        public float CalculateMiterLengthTo(ChamferedCorner other, Vector2 normal, float lineThickness, float sizeHalf)
        {
            var l = (other.Prev - Next).Length();
            var dir = -ChamferEdgeDir;
            var rad = dir.AngleRad(normal);
            var miterLength = Triangle.RightTriangleGetHypotenuseFromOpposite(rad, lineThickness);
            // var miterLength2 = Triangle.RightTriangleGetOppositeFromAdjacent(rad, l / 2f);
            var miterLength2 = Triangle.RightTriangleGetAdjacentFromOpposite(rad, l / 2f);
            miterLength = miterLength - miterLength2;
            return MathF.Min(miterLength, sizeHalf);
        }
        #endregion
    }
    private static void DrawChamferedCorner(ChamferPoints inner, ChamferPoints outer, bool clamped, Raylib_cs.Color rayColor)
    {
        if (clamped)
        {
            Raylib.DrawTriangle(inner.Prev, outer.Prev, outer.Next, rayColor);
        }
        else
        {
            Raylib.DrawTriangle(outer.Prev, outer.Next, inner.Prev, rayColor);
            Raylib.DrawTriangle(inner.Prev, outer.Next, inner.Next, rayColor);
        }
    }
    private static void DrawChamferedEdgeTo(ChamferPoints inner, ChamferPoints outer, ChamferPoints otherInner, ChamferPoints otherOther, 
        bool widthEdgeClamped, bool heightEdgeClamped,  bool widthEdge, Raylib_cs.Color rayColor)
    {
        if (widthEdgeClamped && heightEdgeClamped || (widthEdgeClamped && widthEdge) || (heightEdgeClamped && !widthEdge))
        {
            Raylib.DrawTriangle(inner.Next, outer.Next, otherOther.Prev, rayColor);
        }
        else
        {
            Raylib.DrawTriangle(outer.Next, otherInner.Prev, inner.Next, rayColor);
            Raylib.DrawTriangle(outer.Next, otherOther.Prev, otherInner.Prev, rayColor);
        }
    }
    
    //TODO: Docs
    public static void DrawChamferedCornersLines(this Quad quad, float lineThickness, ColorRgba color, float cornerLength)
    {
        if (lineThickness <= 0)
        {
            quad.DrawChamferedCorners(color, cornerLength);
            return;
        }
        
        if (cornerLength <= 0)
        {
            quad.DrawLines(lineThickness, color);
            return;
        }
        
        var size = quad.GetSize();
        if(size.Width <= 0 || size.Height <= 0) return;
        
        float halfWidth = size.Width * 0.5f;
        float halfHeight = size.Height * 0.5f;
        
        lineThickness = MathF.Min(lineThickness, MathF.Min(halfWidth, halfHeight));
        cornerLength = MathF.Min(cornerLength, MathF.Min(halfWidth, halfHeight));
        var cornerLengthW = cornerLength;// MathF.Min(cornerLength, halfWidth);
        var cornerLengthH = cornerLength;// MathF.Min(cornerLength, halfHeight);
        
        var nR = quad.NormalRight;
        var nD = quad.NormalDown;
        var nL = -nR;
        var nU = -nD;
        
        var rayColor = color.ToRayColor();
            
        var chamferA = new ChamferedCorner(quad.A, cornerLengthW, cornerLengthH, nR, nD);
        var chamferB = new ChamferedCorner(quad.B, cornerLengthH, cornerLengthW, nU, nR);
        var chamferC = new ChamferedCorner(quad.C, cornerLengthW, cornerLengthH, nL, nU, -chamferA.ChamferDirPrev, -chamferA.ChamferDirNext);
        var chamferD = new ChamferedCorner(quad.D, cornerLengthH, cornerLengthW, nD, nL, -chamferB.ChamferDirPrev, -chamferB.ChamferDirNext);
        
        var chamferLength = chamferA.GetChamferLength(nL, lineThickness);
        var maxChamferLength = chamferA.GetMaxChamferLength;
        var maxChamferLengthHeight = chamferA.GetMaxChamferLengthTo(chamferB, false);
        var maxChamferLengthWidth = chamferB.GetMaxChamferLengthTo(chamferC, false);
        
        bool insideCornerClamped = chamferLength > maxChamferLength;
        bool widthEdgeClamped = chamferLength > maxChamferLengthWidth;
        bool heightEdgeClamped = chamferLength > maxChamferLengthHeight;
        
        var chamferAOuter = chamferA.GetOuterPoints(chamferLength);
        var chamferBOuter = chamferB.GetOuterPoints(chamferLength);
        var chamferCOuter = chamferC.GetOuterPoints(chamferLength); 
        var chamferDOuter = chamferD.GetOuterPoints(chamferLength);
        
        ChamferPoints chamferAInner, chamferBInner, chamferCInner, chamferDInner;
        if (insideCornerClamped)
        {
            var offset = MathF.Sqrt(2f * lineThickness * lineThickness);
            chamferAInner = chamferA.GetInnerPointsClamped(offset);
            chamferBInner = chamferB.GetInnerPointsClamped(offset);
            chamferCInner = chamferC.GetInnerPointsClamped(offset);
            chamferDInner = chamferD.GetInnerPointsClamped(offset);
        }
        else if (widthEdgeClamped || heightEdgeClamped)
        {
            // if (widthEdgeClamped && heightEdgeClamped)
            // {
            //     float miterLengthWidth = chamferB.CalculateMiterLengthTo(chamferC, nU, lineThickness, halfHeight);
            //     float miterLengthHeight = chamferA.CalculateMiterLengthTo(chamferB, nR, lineThickness, halfWidth);
            //     
            //     var bNextInner = chamferB.GetInnerTo(chamferC, miterLengthWidth, nU);
            //     var dNextInner = chamferD.GetInnerTo(chamferA, miterLengthWidth, nD);
            //     var aNextInner = chamferA.GetInnerTo(chamferB, miterLengthHeight, nR);
            //     var cNextInner = chamferC.GetInnerTo(chamferD, miterLengthHeight, nL);
            //     
            //     chamferAInner = (dNextInner, aNextInner);
            //     chamferBInner = (aNextInner, bNextInner);
            //     chamferCInner = (bNextInner, cNextInner);
            //     chamferDInner = (cNextInner, dNextInner);
            // }
            // else
            if (widthEdgeClamped)
            {
                float miterLength = chamferB.CalculateMiterLengthTo(chamferC, nU, lineThickness, halfHeight);
                
                var bNextInner = chamferB.GetInnerTo(chamferC, miterLength, nU);
                chamferBInner = (chamferB.GetInnerPrev(chamferLength), bNextInner);
                chamferCInner = (bNextInner, chamferC.GetInnerNext(chamferLength));
                
                var dNextInner = chamferD.GetInnerTo(chamferA, miterLength, nD);
                chamferDInner = (chamferD.GetInnerPrev(chamferLength), dNextInner);
                chamferAInner = (dNextInner, chamferA.GetInnerNext(chamferLength));
            }
            else
            {
                float miterLength = chamferA.CalculateMiterLengthTo(chamferB, nR, lineThickness, halfWidth);
                
                var aNextInner = chamferA.GetInnerTo(chamferB, miterLength, nR);
                chamferAInner = (chamferA.GetInnerPrev(chamferLength), aNextInner);
                chamferBInner = (aNextInner, chamferB.GetInnerNext(chamferLength));
                
                var cNextInner = chamferC.GetInnerTo(chamferD, miterLength, nL);
                chamferCInner = (chamferC.GetInnerPrev(chamferLength), cNextInner);
                chamferDInner = (cNextInner, chamferD.GetInnerNext(chamferLength));
            }
        }
        else
        {
            chamferAInner = chamferA.GetInnerPoints(chamferLength);
            chamferBInner = chamferB.GetInnerPoints(chamferLength);
            chamferCInner = chamferC.GetInnerPoints(chamferLength);
            chamferDInner = chamferD.GetInnerPoints(chamferLength);
        }
        
        DrawChamferedCorner(chamferAInner, chamferAOuter, insideCornerClamped, rayColor);
        DrawChamferedCorner(chamferBInner, chamferBOuter, insideCornerClamped, rayColor);
        DrawChamferedCorner(chamferCInner, chamferCOuter, insideCornerClamped, rayColor);
        DrawChamferedCorner(chamferDInner, chamferDOuter, insideCornerClamped, rayColor);
        
        DrawChamferedEdgeTo(chamferAInner, chamferAOuter, chamferBInner, chamferBOuter, widthEdgeClamped, heightEdgeClamped, false, rayColor);
        DrawChamferedEdgeTo(chamferBInner, chamferBOuter, chamferCInner, chamferCOuter, widthEdgeClamped, heightEdgeClamped, true, rayColor);
        DrawChamferedEdgeTo(chamferCInner, chamferCOuter, chamferDInner, chamferDOuter, widthEdgeClamped, heightEdgeClamped, false, rayColor);
        DrawChamferedEdgeTo(chamferDInner, chamferDOuter, chamferAInner, chamferAOuter, widthEdgeClamped, heightEdgeClamped, true, rayColor);
    }
    
    
    //TODO: Docs
    public static void DrawChamferedCornersLines(this Quad quad, float lineThickness, ColorRgba color, float cornerLengthHorizontal, float cornerLengthVertical)
    {
        if(lineThickness <= 0 && cornerLengthHorizontal <= 0 && cornerLengthVertical <= 0)
        {
            quad.Draw(color);
            return;
        }
        
        if (cornerLengthHorizontal <= 0 && cornerLengthVertical <= 0)
        {
            quad.DrawLines(lineThickness, color);
            return;
        }
        else if (cornerLengthHorizontal <= 0)
        {
            quad.DrawChamferedCornersLines(lineThickness, color, cornerLengthVertical);
            return;
        }
        else if (cornerLengthVertical <= 0f)
        {
            quad.DrawChamferedCornersLines(lineThickness, color, cornerLengthHorizontal);
            return;
        }
        
        if (Math.Abs(cornerLengthHorizontal - cornerLengthVertical) < 0.0001f)
        {
            quad.DrawChamferedCornersLines(lineThickness, color, cornerLengthHorizontal);
            return;
        }
        
        if (lineThickness <= 0)
        {
            quad.DrawChamferedCorners(color, cornerLengthHorizontal, cornerLengthVertical);
            return;
        }
        
        var size = quad.GetSize();
        if(size.Width <= 0 || size.Height <= 0) return;
        
        float halfWidth = size.Width * 0.5f;
        float halfHeight = size.Height * 0.5f;
        var minHalfSize = MathF.Min(halfWidth, halfHeight);
        lineThickness = MathF.Min(lineThickness, minHalfSize);
        // cornerLengthHorizontal = MathF.Min(cornerLengthHorizontal, minHalfSize);
        // cornerLengthVertical = MathF.Min(cornerLengthVertical, minHalfSize);
        var cornerLengthW = MathF.Min(cornerLengthHorizontal, minHalfSize);// MathF.Min(cornerLength, halfWidth);
        var cornerLengthH = MathF.Min(cornerLengthVertical, minHalfSize);// MathF.Min(cornerLength, halfHeight);
        
        var nR = quad.NormalRight;
        var nD = quad.NormalDown;
        var nL = -nR;
        var nU = -nD;
        
        var rayColor = color.ToRayColor();
            
        var chamferA = new ChamferedCorner(quad.A, cornerLengthW, cornerLengthH, nR, nD);
        var chamferB = new ChamferedCorner(quad.B, cornerLengthH, cornerLengthW, nU, nR);
        var chamferC = new ChamferedCorner(quad.C, cornerLengthW, cornerLengthH, nL, nU);
        var chamferD = new ChamferedCorner(quad.D, cornerLengthH, cornerLengthW, nD, nL);
        
        var chamferLengthV = chamferA.GetChamferLength(nL, lineThickness);
        var chamferLengthH = chamferB.GetChamferLength(nD, lineThickness);
        var maxChamferLength = chamferA.GetMaxChamferLength;
        var maxChamferLengthHeight = chamferA.GetMaxChamferLengthTo(chamferB, false);
        var maxChamferLengthWidth = chamferB.GetMaxChamferLengthTo(chamferC, false);
        
        bool insideCornerClamped = chamferLengthH > maxChamferLength || chamferLengthV > maxChamferLength;
        bool widthEdgeClamped = chamferLengthH > maxChamferLengthWidth;
        bool heightEdgeClamped = chamferLengthV > maxChamferLengthHeight;
        
        var chamferAOuter = chamferA.GetOuterPoints(chamferLengthH, chamferLengthV);
        var chamferBOuter = chamferB.GetOuterPoints(chamferLengthV, chamferLengthH);
        var chamferCOuter = chamferC.GetOuterPoints(chamferLengthH, chamferLengthV); 
        var chamferDOuter = chamferD.GetOuterPoints(chamferLengthV, chamferLengthH);
        
        ChamferPoints chamferAInner, chamferBInner, chamferCInner, chamferDInner;
        if (insideCornerClamped)
        {
            // lineThickness = MathF.Min(lineThickness, minHalfSize);
            // var offset = MathF.Sqrt(2f * lineThickness * lineThickness);
            // var aDir = (nR + nD).Normalize();
            // var bDir = (nU + nR).Normalize();
            // var aInner = quad.A + aDir * offset;
            // var bInner = quad.B + bDir * offset;
            // var cInner = quad.C - aDir * offset;
            // var dInner = quad.D - bDir * offset;
            // chamferAInner = (aInner, aInner);
            // chamferBInner = (bInner, bInner);
            // chamferCInner = (cInner, cInner);
            // chamferDInner = (dInner, dInner);
            var offset = MathF.Sqrt(2f * lineThickness * lineThickness);
            chamferAInner = chamferA.GetInnerPointsClamped(offset);
            chamferBInner = chamferB.GetInnerPointsClamped(offset);
            chamferCInner = chamferC.GetInnerPointsClamped(offset);
            chamferDInner = chamferD.GetInnerPointsClamped(offset);
        }
        else if (widthEdgeClamped || heightEdgeClamped)
        {
            // if (widthEdgeClamped && heightEdgeClamped)
            // {
            //     float miterLengthWidth = chamferB.CalculateMiterLengthTo(chamferC, nU, lineThickness, halfHeight);
            //     float miterLengthHeight = chamferA.CalculateMiterLengthTo(chamferB, nR, lineThickness, halfWidth);
            //     
            //     var bNextInner = chamferB.GetInnerTo(chamferC, miterLengthWidth, nU);
            //     var dNextInner = chamferD.GetInnerTo(chamferA, miterLengthWidth, nD);
            //     var aNextInner = chamferA.GetInnerTo(chamferB, miterLengthHeight, nR);
            //     var cNextInner = chamferC.GetInnerTo(chamferD, miterLengthHeight, nL);
            //     
            //     chamferAInner = (dNextInner, aNextInner);
            //     chamferBInner = (aNextInner, bNextInner);
            //     chamferCInner = (bNextInner, cNextInner);
            //     chamferDInner = (cNextInner, dNextInner);
            // }
            // else
            if (widthEdgeClamped)
            {
                float miterLength = chamferB.CalculateMiterLengthTo(chamferC, nU, lineThickness, halfHeight);
                
                var bNextInner = chamferB.GetInnerTo(chamferC, miterLength, nU);
                chamferBInner = (chamferB.GetInnerPrev(chamferLengthV), bNextInner);
                chamferCInner = (bNextInner, chamferC.GetInnerNext(chamferLengthV));
                
                var dNextInner = chamferD.GetInnerTo(chamferA, miterLength, nD);
                chamferDInner = (chamferD.GetInnerPrev(chamferLengthV), dNextInner);
                chamferAInner = (dNextInner, chamferA.GetInnerNext(chamferLengthV));
            }
            else
            {
                float miterLength = chamferA.CalculateMiterLengthTo(chamferB, nR, lineThickness, halfWidth);
                
                var aNextInner = chamferA.GetInnerTo(chamferB, miterLength, nR);
                chamferAInner = (chamferA.GetInnerPrev(chamferLengthH), aNextInner);
                chamferBInner = (aNextInner, chamferB.GetInnerNext(chamferLengthH));
                
                var cNextInner = chamferC.GetInnerTo(chamferD, miterLength, nL);
                chamferCInner = (chamferC.GetInnerPrev(chamferLengthH), cNextInner);
                chamferDInner = (cNextInner, chamferD.GetInnerNext(chamferLengthH));
            }
        }
        else
        {
            chamferAInner = chamferA.GetInnerPoints(chamferLengthH, chamferLengthV);
            chamferBInner = chamferB.GetInnerPoints(chamferLengthV, chamferLengthH);
            chamferCInner = chamferC.GetInnerPoints(chamferLengthH, chamferLengthV);
            chamferDInner = chamferD.GetInnerPoints(chamferLengthV, chamferLengthH);
        }
        
        DrawChamferedCorner(chamferAInner, chamferAOuter, insideCornerClamped, rayColor);
        DrawChamferedCorner(chamferBInner, chamferBOuter, insideCornerClamped, rayColor);
        DrawChamferedCorner(chamferCInner, chamferCOuter, insideCornerClamped, rayColor);
        DrawChamferedCorner(chamferDInner, chamferDOuter, insideCornerClamped, rayColor);
        
        DrawChamferedEdgeTo(chamferAInner, chamferAOuter, chamferBInner, chamferBOuter, widthEdgeClamped, heightEdgeClamped, false, rayColor);
        DrawChamferedEdgeTo(chamferBInner, chamferBOuter, chamferCInner, chamferCOuter, widthEdgeClamped, heightEdgeClamped, true, rayColor);
        DrawChamferedEdgeTo(chamferCInner, chamferCOuter, chamferDInner, chamferDOuter, widthEdgeClamped, heightEdgeClamped, false, rayColor);
        DrawChamferedEdgeTo(chamferDInner, chamferDOuter, chamferAInner, chamferAOuter, widthEdgeClamped, heightEdgeClamped, true, rayColor);
    }
    
    
    //TODO: Implement + Docs
    public static void DrawChamferedCornersLines(this Quad quad, float lineThickness, ColorRgba color, float tlCorner, float blCorner, float brCorner, float trCorner)
    {
        tlCorner = MathF.Max(0, tlCorner);
        blCorner = MathF.Max(0, blCorner);
        brCorner = MathF.Max(0, brCorner);
        trCorner = MathF.Max(0, trCorner);
        var cornerSum = tlCorner + blCorner + brCorner + trCorner;
       
        if(lineThickness <= 0 && cornerSum <= 0)
        {
            quad.Draw(color);
            return;
        }
        
        if (cornerSum <= 0)
        {
            quad.DrawLines(lineThickness, color);
            return;
        }
        
        if (lineThickness <= 0)
        {
            quad.DrawChamferedCorners(color, tlCorner, blCorner, brCorner, trCorner);
            return;
        }
        
        var size = quad.GetSize();
        if(size.Width <= 0 || size.Height <= 0) return;
        
        float halfWidth = size.Width * 0.5f;
        float halfHeight = size.Height * 0.5f;
        var minHalfSize = MathF.Min(halfWidth, halfHeight);
        lineThickness = MathF.Min(lineThickness, minHalfSize);
        
        var cornerLengthTl = MathF.Min(tlCorner, minHalfSize);
        var cornerLengthBl = MathF.Min(blCorner, minHalfSize);
        var cornerLengthBr = MathF.Min(brCorner, minHalfSize);
        var cornerLengthTr = MathF.Min(trCorner, minHalfSize);
        
        var nR = quad.NormalRight;
        var nD = quad.NormalDown;
        var nL = -nR;
        var nU = -nD;
        
        var rayColor = color.ToRayColor();
            
        var chamferA = new ChamferedCorner(quad.A, cornerLengthTl, nR, nD);
        var chamferB = new ChamferedCorner(quad.B, cornerLengthBl, nU, nR);
        var chamferC = new ChamferedCorner(quad.C, cornerLengthBr, nL, nU);
        var chamferD = new ChamferedCorner(quad.D, cornerLengthTr, nD, nL);
        
        var maxChamferLengthA = chamferA.GetMaxChamferLength;
        var maxChamferLengthB = chamferB.GetMaxChamferLength;
        var maxChamferLengthC = chamferC.GetMaxChamferLength;
        var maxChamferLengthD = chamferD.GetMaxChamferLength;
        
        var chamferLengthA = chamferA.GetChamferLength(nL, lineThickness);
        var chamferLengthB = chamferB.GetChamferLength(nD, lineThickness);
        var chamferLengthC = chamferC.GetChamferLength(nR, lineThickness);
        var chamferLengthD = chamferD.GetChamferLength(nU, lineThickness);
        
        bool insideCornerClampedA = chamferLengthA > maxChamferLengthA;
        bool insideCornerClampedB = chamferLengthB > maxChamferLengthB;
        bool insideCornerClampedC = chamferLengthC > maxChamferLengthC;
        bool insideCornerClampedD = chamferLengthD > maxChamferLengthD;
        
        var clampedOffset = MathF.Sqrt(2f * lineThickness * lineThickness);
        
        //TODO: maxChamferLength of edges need to be calculated differently if insideCorner is clamped! (Isocoles triangle no longer works...)
        // - once a inside corner is clamped the direction changes to chamfer dir and the length to clampedOffset
        var maxChamferLengthAB = chamferA.GetMaxChamferLengthTo(chamferB, insideCornerClampedA);
        var maxChamferLengthBC = chamferB.GetMaxChamferLengthTo(chamferC, insideCornerClampedB);
        var maxChamferLengthCD = chamferC.GetMaxChamferLengthTo(chamferD, insideCornerClampedC);
        var maxChamferLengthDA = chamferD.GetMaxChamferLengthTo(chamferA, insideCornerClampedD);
        
        //Just a test
        // chamferLengthA = MathF.Min(MathF.Min(chamferLengthA, maxChamferLengthAB), maxChamferLengthDA);
        // chamferLengthB = MathF.Min(MathF.Min(chamferLengthB, maxChamferLengthAB), maxChamferLengthBC);
        // chamferLengthC = MathF.Min(MathF.Min(chamferLengthC, maxChamferLengthBC), maxChamferLengthCD);
        // chamferLengthD = MathF.Min(MathF.Min(chamferLengthD, maxChamferLengthCD), maxChamferLengthDA);
        // chamferLengthA = MathF.Min(chamferLengthA, MathF.Max(maxChamferLengthAB, maxChamferLengthDA) - lineThickness);
        // chamferLengthB = MathF.Min(chamferLengthB, MathF.Max(maxChamferLengthAB, maxChamferLengthBC) - lineThickness);
        // chamferLengthC = MathF.Min(chamferLengthC, MathF.Max(maxChamferLengthBC, maxChamferLengthCD) - lineThickness);
        // chamferLengthD = MathF.Min(chamferLengthD, MathF.Max(maxChamferLengthCD, maxChamferLengthDA) - lineThickness);

        // var maxLengthA = insideCornerClampedA ? clampedOffset : chamferLengthA; //MathF.Max(clampedOffset, chamferLengthA);
        // var maxLengthB = insideCornerClampedB ? clampedOffset : chamferLengthB; //MathF.Max(clampedOffset, chamferLengthB);
        // var maxLengthC = insideCornerClampedC ? clampedOffset : chamferLengthC; //MathF.Max(clampedOffset, chamferLengthC);
        // var maxLengthD = insideCornerClampedD ? clampedOffset : chamferLengthD; //MathF.Max(clampedOffset, chamferLengthD);
        
        bool edgeABClamped = chamferLengthA > maxChamferLengthAB || chamferLengthB > maxChamferLengthAB;
        bool edgeBCClamped = chamferLengthB > maxChamferLengthBC || chamferLengthC > maxChamferLengthBC;
        bool edgeCDClapmed = chamferLengthC > maxChamferLengthCD || chamferLengthD > maxChamferLengthCD;
        bool edgeDAClamped = chamferLengthD > maxChamferLengthDA || chamferLengthA > maxChamferLengthDA;
        
        var chamferAOuter = chamferA.GetOuterPoints(chamferLengthA);
        var chamferBOuter = chamferB.GetOuterPoints(chamferLengthB);
        var chamferCOuter = chamferC.GetOuterPoints(chamferLengthC); 
        var chamferDOuter = chamferD.GetOuterPoints(chamferLengthD);
        
        ChamferPoints chamferAInner, chamferBInner, chamferCInner, chamferDInner;

        ChamferPoints CalculateInner(ChamferedCorner corner, ChamferedCorner prev, ChamferedCorner next, 
            float chamferLength, float prevSize, float nextSize, Vector2 prevNormal, Vector2 nextNormal,
            float cornerLength, float cornerLengthPrev, float cornerLengthNext,
            bool prevEdgeClamped, bool nextEdgeClamped, bool cornerClamped)
        {
            if (cornerClamped)
            {
                return corner.GetInnerPointsClamped(clampedOffset);
            }
            else if (prevEdgeClamped || nextEdgeClamped)
            {
                if (prevEdgeClamped)
                {
                    if (cornerLength < cornerLengthPrev)
                    {
                        float miterLength = corner.CalculateMiterLengthTo(prev, prevNormal, lineThickness, prevSize);
                        var p = corner.GetInnerTo(prev, miterLength, prevNormal);
                        var innerNext = corner.GetInnerNext(chamferLength);
                        var toMiter = p - corner.Next;
                        var toInnerNext = innerNext - corner.Next;
                        bool shouldMerge = Vector2.Dot(toInnerNext, toMiter) > Vector2.Dot(toMiter, toMiter);
                        return shouldMerge ? (p, p) : (p, innerNext);
                    }
                    else
                    {
                        
                        float miterLength = prev.CalculateMiterLengthTo(corner, -prevNormal, lineThickness, prevSize);
                        var p = prev.GetInnerTo(corner, miterLength, -prevNormal);
                        var innerNext = corner.GetInnerNext(chamferLength);
                        var toMiter = p - corner.Next;
                        var toInnerNext = innerNext - corner.Next;
                        bool shouldMerge = Vector2.Dot(toInnerNext, toMiter) > Vector2.Dot(toMiter, toMiter);
                        return shouldMerge ? (p, p) : (p, innerNext);
                    }
                }
                else
                {
                    if (cornerLength < cornerLengthNext)
                    {
                        float miterLength = corner.CalculateMiterLengthTo(next, nextNormal, lineThickness, nextSize);
                        var p = corner.GetInnerTo(next, miterLength, nextNormal);
                        var innerPrev = corner.GetInnerPrev(chamferLength);
                        var toMiter = p - corner.Prev;
                        var toInnerPrev = innerPrev - corner.Prev;
                        bool shouldMerge = Vector2.Dot(toInnerPrev, toMiter) > Vector2.Dot(toMiter, toMiter);
                        return shouldMerge ? (p, p) : (innerPrev, p);
                    }
                    else
                    {
                        float miterLength = next.CalculateMiterLengthTo(corner, -nextNormal, lineThickness, nextSize);
                        var p = next.GetInnerTo(corner, miterLength, -nextNormal);
                        var innerPrev = corner.GetInnerPrev(chamferLength);
                        var toMiter = p - corner.Prev;
                        var toInnerPrev = innerPrev - corner.Prev;
                        bool shouldMerge = Vector2.Dot(toInnerPrev, toMiter) > Vector2.Dot(toMiter, toMiter);
                        return shouldMerge ? (p, p) : (innerPrev, p);
                    }
                    
                }
            }
            else
            {
                return corner.GetInnerPoints(chamferLength);
            }
        }
        
        chamferAInner = CalculateInner(chamferA, chamferD, chamferB, chamferLengthA, halfHeight, halfWidth, nU, nR, cornerLengthTl, cornerLengthTr, cornerLengthBl, edgeDAClamped, edgeABClamped, insideCornerClampedA);
        chamferBInner = CalculateInner(chamferB, chamferA, chamferC, chamferLengthB, halfWidth, halfHeight, nL, nU, cornerLengthBl, cornerLengthTl, cornerLengthBr, edgeABClamped, edgeBCClamped, insideCornerClampedB);
        chamferCInner = CalculateInner(chamferC, chamferB, chamferD, chamferLengthC, halfHeight, halfWidth, nD, nL, cornerLengthBr, cornerLengthBl, cornerLengthTr, edgeBCClamped, edgeCDClapmed, insideCornerClampedC);
        chamferDInner = CalculateInner(chamferD, chamferC, chamferA, chamferLengthD, halfWidth, halfHeight, nR, nD, cornerLengthTr, cornerLengthBr, cornerLengthTl, edgeCDClapmed, edgeDAClamped, insideCornerClampedD);
        
        DrawChamferedCorner(chamferAInner, chamferAOuter, insideCornerClampedA, rayColor);
        DrawChamferedCorner(chamferBInner, chamferBOuter, insideCornerClampedB, rayColor);
        DrawChamferedCorner(chamferCInner, chamferCOuter, insideCornerClampedC, rayColor);
        DrawChamferedCorner(chamferDInner, chamferDOuter, insideCornerClampedD, rayColor);
        
        DrawChamferedEdgeTo(chamferAInner, chamferAOuter, chamferBInner, chamferBOuter, edgeDAClamped, edgeABClamped, false, rayColor);
        DrawChamferedEdgeTo(chamferBInner, chamferBOuter, chamferCInner, chamferCOuter, edgeBCClamped, edgeABClamped, true, rayColor);
        DrawChamferedEdgeTo(chamferCInner, chamferCOuter, chamferDInner, chamferDOuter, edgeBCClamped, edgeCDClapmed, false, rayColor);
        DrawChamferedEdgeTo(chamferDInner, chamferDOuter, chamferAInner, chamferAOuter, edgeDAClamped, edgeCDClapmed, true, rayColor);
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
    //Probably move to quad.DrawLines and remove the helper function with 4 vectors as parameters
    private static void DrawLinesHelper(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float lineThickness, ColorRgba color)
    {
        var horizontal = c - b;
        var vertical = a - b;
        var w = horizontal.Length();
        var h = vertical.Length();
        if (w <= 0 || h <= 0) return;
        var bA = horizontal / w;
        var bC = vertical / h;

        lineThickness = MathF.Min(lineThickness, MathF.Min(w, h) * 0.5f);
        
        var offsetDistance = MathF.Sqrt(2f * lineThickness * lineThickness);
        
        // corner at b, adjacent vertices a and c
        // var bA = Vector2.Normalize(a - b); // direction from corner toward A
        // var bC = Vector2.Normalize(c - b); // direction from corner toward C

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


/*
        // var chamferAInnerPrev = CalculateInnerPrev(chamferA, chamferD, chamferB, chamferLengthA, lineThickness, edgeDAClamped, edgeABClamped, insideCornerClampedA);
        // var chamferAInnerNext = CalculateInnerNext(chamferA, chamferD, chamferB, chamferLengthA, lineThickness, edgeDAClamped, edgeABClamped, insideCornerClampedA);
        // chamferAInner = (chamferAInnerPrev, chamferAInnerNext);
        // Vector2 CalculateInnerPrev(ChamferedCorner corner, ChamferedCorner prev, ChamferedCorner next, float chamferLength, float thickness, bool prevEdgeClamped, bool nextEdgeClamped, bool cornerClamped)
        // {
        //     return new();
        // }
        // Vector2 CalculateInnerNext(ChamferedCorner corner, ChamferedCorner prev, ChamferedCorner next, float chamferLength, float thickness, bool prevEdgeClamped, bool nextEdgeClamped, bool cornerClamped)
        // {
        //     return new();
        // }
        //
        // ChamferPoints CalculateInner(ChamferedCorner corner, ChamferedCorner prev, ChamferedCorner next, float chamferLength, float thickness, bool prevEdgeClamped, bool nextEdgeClamped, bool cornerClamped)
        // {
        //     return (new(), new());
        // }
        // if (insideCornerClampedA && (edgeABClamped || edgeDAClamped))
        // {
        //     if (edgeABClamped)
        //     {
        //         var p = chamferC.Next - chamferC.ChamferDirNext * maxChamferLengthCD;
        //         chamferAInner = (p, p);
        //     }
        //     else
        //     {
        //         // var center = (chamferA.Prev + chamferD.ChamferCenter) * 0.5f;
        //         // var dir = (chamferA.ChamferDirPrev + chamferD.ChamferDir).Normalize();
        //         // var p = center - dir * lineThickness;
        //         var p = chamferA.Prev - chamferA.ChamferDirPrev * maxChamferLengthDA;
        //         chamferAInner = (p, p);
        //         // chamferAInner = (p, chamferAInner.Next);
        //     }
        // }
        // else 
        if (insideCornerClampedA)
        {
            chamferAInner = chamferA.GetInnerPointsClamped(clampedOffset);
        }
        else if (edgeABClamped || edgeDAClamped)
        {
            if (edgeDAClamped)
            {
                if (cornerLengthTl < cornerLengthTr)
                {
                    float miterLength = chamferA.CalculateMiterLengthTo(chamferD, nU, lineThickness, halfHeight);
                    var p = chamferA.GetInnerTo(chamferD, miterLength, nU);
                    var innerNext = chamferA.GetInnerNext(chamferLengthA);
                    var toMiter = p - chamferA.Next;
                    var toInnerNext = innerNext - chamferA.Next;
                    bool shouldMerge = Vector2.Dot(toInnerNext, toMiter) > Vector2.Dot(toMiter, toMiter);
                    chamferAInner = shouldMerge ? (p, p) : (p, innerNext);
                }
                else
                {
                    
                    float miterLength = chamferD.CalculateMiterLengthTo(chamferA, nD, lineThickness, halfHeight);
                    var p = chamferD.GetInnerTo(chamferA, miterLength, nD);
                    var innerNext = chamferA.GetInnerNext(chamferLengthA);
                    var toMiter = p - chamferA.Next;
                    var toInnerNext = innerNext - chamferA.Next;
                    bool shouldMerge = Vector2.Dot(toInnerNext, toMiter) > Vector2.Dot(toMiter, toMiter);
                    chamferAInner = shouldMerge ? (p, p) : (p, innerNext);
                }
            }
            else
            {
                if (cornerLengthTl < cornerLengthBl)
                {
                    float miterLength = chamferA.CalculateMiterLengthTo(chamferB, nR, lineThickness, halfWidth);
                    var p = chamferA.GetInnerTo(chamferB, miterLength, nR);
                    var innerPrev = chamferA.GetInnerPrev(chamferLengthA);
                    var toMiter = p - chamferA.Prev;
                    var toInnerPrev = innerPrev - chamferA.Prev;
                    bool shouldMerge = Vector2.Dot(toInnerPrev, toMiter) > Vector2.Dot(toMiter, toMiter);
                    chamferAInner = shouldMerge ? (p, p) : (innerPrev, p);
                }
                else
                {
                    float miterLength = chamferB.CalculateMiterLengthTo(chamferA, nL, lineThickness, halfWidth);
                    var p = chamferB.GetInnerTo(chamferA, miterLength, nL);
                    var innerPrev = chamferA.GetInnerPrev(chamferLengthA);
                    var toMiter = p - chamferA.Prev;
                    var toInnerPrev = innerPrev - chamferA.Prev;
                    bool shouldMerge = Vector2.Dot(toInnerPrev, toMiter) > Vector2.Dot(toMiter, toMiter);
                    chamferAInner = shouldMerge ? (p, p) : (innerPrev, p);
                }
                
            }
        }
        else
        {
            chamferAInner = chamferA.GetInnerPoints(chamferLengthA);
        }
        
        
        if (edgeABClamped || edgeBCClamped)
        {
            if (edgeABClamped)
            {
                if (cornerLengthBl < cornerLengthTl)
                {
                    float miterLength = chamferB.CalculateMiterLengthTo(chamferA, nL, lineThickness, halfWidth);
                    var p = chamferB.GetInnerTo(chamferA, miterLength, nL);
                    var innerNext = chamferB.GetInnerNext(chamferLengthB);
                    var toMiter = p - chamferB.Next;
                    var toInnerNext = innerNext - chamferB.Next;
                    bool shouldMerge = Vector2.Dot(toInnerNext, toMiter) > Vector2.Dot(toMiter, toMiter);
                    chamferBInner = shouldMerge ? (p, p) : (p, innerNext);
                }
                else
                {
                    float miterLength = chamferA.CalculateMiterLengthTo(chamferB, nR, lineThickness, halfWidth);
                    var p = chamferA.GetInnerTo(chamferB, miterLength, nR);
                    var innerNext = chamferB.GetInnerNext(chamferLengthB);
                    var toMiter = p - chamferB.Next;
                    var toInnerNext = innerNext - chamferB.Next;
                    bool shouldMerge = Vector2.Dot(toInnerNext, toMiter) > Vector2.Dot(toMiter, toMiter);
                    chamferBInner = shouldMerge ? (p, p) : (p, innerNext);
                }
                
            }
            else
            {
                if (cornerLengthBl < cornerLengthBr)
                {
                    float miterLength = chamferB.CalculateMiterLengthTo(chamferC, nU, lineThickness, halfHeight);
                    var p = chamferB.GetInnerTo(chamferC, miterLength, nU);
                    var innerPrev = chamferB.GetInnerPrev(chamferLengthB);
                    var toMiter = p - chamferB.Prev;
                    var toInnerPrev = innerPrev - chamferB.Prev;
                    bool shouldMerge = Vector2.Dot(toInnerPrev, toMiter) > Vector2.Dot(toMiter, toMiter);
                    chamferBInner = shouldMerge ? (p, p) : (innerPrev, p);
                }
                else
                {
                    float miterLength = chamferC.CalculateMiterLengthTo(chamferB, nD, lineThickness, halfHeight);
                    var p = chamferC.GetInnerTo(chamferB, miterLength, nD);
                    var innerPrev = chamferB.GetInnerPrev(chamferLengthB);
                    var toMiter = p - chamferB.Prev;
                    var toInnerPrev = innerPrev - chamferB.Prev;
                    bool shouldMerge = Vector2.Dot(toInnerPrev, toMiter) > Vector2.Dot(toMiter, toMiter);
                    chamferBInner = shouldMerge ? (p, p) : (innerPrev, p);
                }
                
            }
        }
        else if (insideCornerClampedB)
        {
            chamferBInner = chamferB.GetInnerPointsClamped(clampedOffset);
        }
        else
        {
            chamferBInner = chamferB.GetInnerPoints(chamferLengthB);
        }
        
        
        if (edgeBCClamped || edgeCDClapmed)
        {
            if (edgeBCClamped)
            {
                if (cornerLengthBr < cornerLengthBl)
                {
                    float miterLength = chamferC.CalculateMiterLengthTo(chamferB, nD, lineThickness, halfHeight);
                    var p = chamferC.GetInnerTo(chamferB, miterLength, nD);
                    var innerNext = chamferC.GetInnerNext(chamferLengthC);
                    var toMiter = p - chamferC.Next;
                    var toInnerNext = innerNext - chamferC.Next;
                    bool shouldMerge = Vector2.Dot(toInnerNext, toMiter) > Vector2.Dot(toMiter, toMiter);
                    chamferCInner = shouldMerge ? (p, p) : (p, innerNext);
                }
                else
                {
                    float miterLength = chamferB.CalculateMiterLengthTo(chamferC, nU, lineThickness, halfHeight);
                    var p = chamferB.GetInnerTo(chamferC, miterLength, nU);
                    var innerNext = chamferC.GetInnerNext(chamferLengthC);
                    var toMiter = p - chamferC.Next;
                    var toInnerNext = innerNext - chamferC.Next;
                    bool shouldMerge = Vector2.Dot(toInnerNext, toMiter) > Vector2.Dot(toMiter, toMiter);
                    chamferCInner = shouldMerge ? (p, p) : (p, innerNext);
                }
                
            }
            else
            {
                if (cornerLengthBr < cornerLengthTr)
                {
                    float miterLength = chamferC.CalculateMiterLengthTo(chamferD, nL, lineThickness, halfWidth);
                    var p = chamferC.GetInnerTo(chamferD, miterLength, nL);
                    var innerPrev = chamferC.GetInnerPrev(chamferLengthC);
                    var toMiter = p - chamferC.Prev;
                    var toInnerPrev = innerPrev - chamferC.Prev;
                    bool shouldMerge = Vector2.Dot(toInnerPrev, toMiter) > Vector2.Dot(toMiter, toMiter);
                    chamferCInner = shouldMerge ? (p, p) : (innerPrev, p);
                }
                else
                {
                    float miterLength = chamferD.CalculateMiterLengthTo(chamferC, nR, lineThickness, halfWidth);
                    var p = chamferD.GetInnerTo(chamferC, miterLength, nR);
                    var innerPrev = chamferC.GetInnerPrev(chamferLengthC);
                    var toMiter = p - chamferC.Prev;
                    var toInnerPrev = innerPrev - chamferC.Prev;
                    bool shouldMerge = Vector2.Dot(toInnerPrev, toMiter) > Vector2.Dot(toMiter, toMiter);
                    chamferCInner = shouldMerge ? (p, p) : (innerPrev, p);
                }
                
            }
        }
        else if (insideCornerClampedC)
        {
            chamferCInner = chamferC.GetInnerPointsClamped(clampedOffset);
        }
        else
        {
            chamferCInner = chamferC.GetInnerPoints(chamferLengthC);
        }

        // if (insideCornerClampedD && (edgeCDClapmed || edgeDAClamped))
        // {
        //     if (edgeCDClapmed)
        //     {
        //         var p = chamferC.Next - chamferC.ChamferDirNext * maxChamferLengthCD;
        //         chamferDInner = (p, p);
        //     }
        //     else
        //     {
        //         // var center = (chamferA.Prev + chamferD.ChamferCenter) * 0.5f;
        //         // var dir = (chamferA.ChamferDirPrev + chamferD.ChamferDir).Normalize();
        //         // var p = center - dir * lineThickness;
        //         var p = chamferA.Prev - chamferA.ChamferDirPrev * maxChamferLengthDA;
        //         chamferDInner = (p, p);
        //         // chamferAInner = (p, chamferAInner.Next);
        //     }
        // }
        // else 
        if (insideCornerClampedD)
        {
            chamferDInner = chamferD.GetInnerPointsClamped(clampedOffset);
        }
        else if (edgeCDClapmed || edgeDAClamped)
        {
            if (edgeCDClapmed)
            {
                if (cornerLengthTr < cornerLengthBr)
                {
                    float miterLength = chamferD.CalculateMiterLengthTo(chamferC, nR, lineThickness, halfWidth);
                    var p = chamferD.GetInnerTo(chamferC, miterLength, nR);
                    var innerNext = chamferD.GetInnerNext(chamferLengthD);
                    var toMiter = p - chamferD.Next;
                    var toInnerNext = innerNext - chamferD.Next;
                    bool shouldMerge = Vector2.Dot(toInnerNext, toMiter) > Vector2.Dot(toMiter, toMiter);
                    chamferDInner = shouldMerge ? (p, p) : (p, innerNext);
                }
                else
                {
                    float miterLength = chamferC.CalculateMiterLengthTo(chamferD, nL, lineThickness, halfWidth);
                    var p = chamferC.GetInnerTo(chamferD, miterLength, nL);
                    var innerNext = chamferD.GetInnerNext(chamferLengthD);
                    var toMiter = p - chamferD.Next;
                    var toInnerNext = innerNext - chamferD.Next;
                    bool shouldMerge = Vector2.Dot(toInnerNext, toMiter) > Vector2.Dot(toMiter, toMiter);
                    chamferDInner = shouldMerge ? (p, p) : (p, innerNext);
                }
                
            }
            else
            {
                if(cornerLengthTr < cornerLengthTl)
                {
                    float miterLength = chamferD.CalculateMiterLengthTo(chamferA, nD, lineThickness, halfHeight);
                    var p = chamferD.GetInnerTo(chamferA, miterLength, nD);
                    var innerPrev = chamferD.GetInnerPrev(chamferLengthD);
                    var toMiter = p - chamferD.Prev;
                    var toInnerPrev = innerPrev - chamferD.Prev;
                    bool shouldMerge = Vector2.Dot(toInnerPrev, toMiter) > Vector2.Dot(toMiter, toMiter);
                    chamferDInner = shouldMerge ? (p, p) : (innerPrev, p);
                }
                else
                {
                    float miterLength = chamferA.CalculateMiterLengthTo(chamferD, nU, lineThickness, halfHeight);
                    var p = chamferA.GetInnerTo(chamferD, miterLength, nU);
                    var innerPrev = chamferD.GetInnerPrev(chamferLengthD);
                    var toMiter = p - chamferD.Prev;
                    var toInnerPrev = innerPrev - chamferD.Prev;
                    bool shouldMerge = Vector2.Dot(toInnerPrev, toMiter) > Vector2.Dot(toMiter, toMiter);
                    chamferDInner = shouldMerge ? (p, p) : (innerPrev, p);
                }
            }
        }
        else
        {
            chamferDInner = chamferD.GetInnerPoints(chamferLengthD);
        }
*/