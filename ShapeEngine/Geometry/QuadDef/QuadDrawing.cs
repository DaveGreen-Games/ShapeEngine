
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
        var minHalfSize = MathF.Min(halfWidth, halfHeight);
        lineThickness = MathF.Min(lineThickness, minHalfSize);
        cornerLength = MathF.Min(cornerLength, minHalfSize);
        
        var nR = quad.NormalRight;
        var nD = quad.NormalDown;
        var nL = -nR;
        var nU = -nD;
        
        var chamferA = new ChamferedCorner(quad.A, cornerLength, cornerLength, nR, nD);
        var chamferB = new ChamferedCorner(quad.B, cornerLength, cornerLength, nU, nR);
        var chamferC = new ChamferedCorner(quad.C, cornerLength, cornerLength, nL, nU, -chamferA.ChamferDirPrev, -chamferA.ChamferDirNext);
        var chamferD = new ChamferedCorner(quad.D, cornerLength, cornerLength, nD, nL, -chamferB.ChamferDirPrev, -chamferB.ChamferDirNext);
        
        var chamferLength = chamferA.GetChamferLength(nL, lineThickness);
        
        outerChamferPointsHelper.Clear();
        innerChamferPointsHelper.Clear();

        chamferA.AddToList(innerChamferPointsHelper);
        chamferB.AddToList(innerChamferPointsHelper);
        chamferC.AddToList(innerChamferPointsHelper);
        chamferD.AddToList(innerChamferPointsHelper);
        
        chamferA.AddOuterToList(outerChamferPointsHelper, chamferLength);
        chamferB.AddOuterToList(outerChamferPointsHelper, chamferLength);
        chamferC.AddOuterToList(outerChamferPointsHelper, chamferLength);
        chamferD.AddOuterToList(outerChamferPointsHelper, chamferLength);
        
        DrawChamferedOutline(lineThickness, color);
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
        
        var cornerLengthW = MathF.Min(cornerLengthHorizontal, halfWidth);
        var cornerLengthH = MathF.Min(cornerLengthVertical, halfHeight);
        
        var nR = quad.NormalRight;
        var nD = quad.NormalDown;
        var nL = -nR;
        var nU = -nD;
        
        var chamferA = new ChamferedCorner(quad.A, cornerLengthW, cornerLengthH, nR, nD);
        var chamferB = new ChamferedCorner(quad.B, cornerLengthH, cornerLengthW, nU, nR);
        var chamferC = new ChamferedCorner(quad.C, cornerLengthW, cornerLengthH, nL, nU);
        var chamferD = new ChamferedCorner(quad.D, cornerLengthH, cornerLengthW, nD, nL);
        
        var chamferLengthV = chamferA.GetChamferLength(nL, lineThickness);
        var chamferLengthH = chamferB.GetChamferLength(nD, lineThickness);
        
        outerChamferPointsHelper.Clear();
        innerChamferPointsHelper.Clear();

        chamferA.AddToList(innerChamferPointsHelper);
        chamferB.AddToList(innerChamferPointsHelper);
        chamferC.AddToList(innerChamferPointsHelper);
        chamferD.AddToList(innerChamferPointsHelper);
        
        chamferA.AddOuterToList(outerChamferPointsHelper, chamferLengthH, chamferLengthV);
        chamferB.AddOuterToList(outerChamferPointsHelper, chamferLengthV, chamferLengthH);
        chamferC.AddOuterToList(outerChamferPointsHelper, chamferLengthH, chamferLengthV);
        chamferD.AddOuterToList(outerChamferPointsHelper, chamferLengthV, chamferLengthH);
        
        DrawChamferedOutline(lineThickness, color);
    }
    
    //TODO: Docs
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
        
            
        var chamferA = new ChamferedCorner(quad.A, cornerLengthTl, nR, nD);
        var chamferB = new ChamferedCorner(quad.B, cornerLengthBl, nU, nR);
        var chamferC = new ChamferedCorner(quad.C, cornerLengthBr, nL, nU);
        var chamferD = new ChamferedCorner(quad.D, cornerLengthTr, nD, nL);
        
        var chamferLengthA = chamferA.GetChamferLength(nL, lineThickness);
        var chamferLengthB = chamferB.GetChamferLength(nD, lineThickness);
        var chamferLengthC = chamferC.GetChamferLength(nR, lineThickness);
        var chamferLengthD = chamferD.GetChamferLength(nU, lineThickness);
        
        outerChamferPointsHelper.Clear();
        innerChamferPointsHelper.Clear();

        chamferA.AddToList(innerChamferPointsHelper);
        chamferB.AddToList(innerChamferPointsHelper);
        chamferC.AddToList(innerChamferPointsHelper);
        chamferD.AddToList(innerChamferPointsHelper);
        
        chamferA.AddOuterToList(outerChamferPointsHelper, chamferLengthA);
        chamferB.AddOuterToList(outerChamferPointsHelper, chamferLengthB);
        chamferC.AddOuterToList(outerChamferPointsHelper, chamferLengthC);
        chamferD.AddOuterToList(outerChamferPointsHelper, chamferLengthD);
        
        DrawChamferedOutline(lineThickness, color);
    }
    
    #endregion

    #region Draw Chamfered Corners Relative Lines
    //TODO: Docs
    public static void DrawChamferedCornersLinesRelative(this Quad quad, float lineThickness, ColorRgba color, float cornerLengthFactor)
    {
        var size = quad.GetSize();
        float halfWidth = size.Width * 0.5f;
        float halfHeight = size.Height * 0.5f;
        cornerLengthFactor = ShapeMath.Clamp(cornerLengthFactor, 0f, 1f);
        
        DrawChamferedCornersLines(quad, lineThickness, color, halfWidth * cornerLengthFactor, halfHeight * cornerLengthFactor);
    }
    
    //TODO: Docs
    public static void DrawChamferedCornersLinesRelative(this Quad quad, float lineThickness, ColorRgba color, float cornerLengthFactorHorizontal, float cornerLengthFactorVertical)
    {
        var size = quad.GetSize();
        float halfWidth = size.Width * 0.5f;
        float halfHeight = size.Height * 0.5f;
        cornerLengthFactorHorizontal = ShapeMath.Clamp(cornerLengthFactorHorizontal, 0f, 1f);
        cornerLengthFactorVertical = ShapeMath.Clamp(cornerLengthFactorVertical, 0f, 1f);
        float cornerLengthH = cornerLengthFactorHorizontal * halfWidth;
        float cornerLengthV = cornerLengthFactorVertical * halfHeight;
        DrawChamferedCornersLines(quad, lineThickness, color, cornerLengthH, cornerLengthV);
    }

    //TODO: Docs
    public static void DrawChamferedCornersLinesRelative(this Quad quad, float lineThickness, ColorRgba color, float tlCornerFactor, float blCornerFactor, float brCornerFactor, float trCornerFactor)
    {
        tlCornerFactor = ShapeMath.Clamp(tlCornerFactor, 0f, 1f);
        blCornerFactor = ShapeMath.Clamp(blCornerFactor, 0f, 1f);
        brCornerFactor = ShapeMath.Clamp(brCornerFactor, 0f, 1f);
        trCornerFactor = ShapeMath.Clamp(trCornerFactor, 0f, 1f);
        
        var cornerFactorSum = tlCornerFactor + blCornerFactor + brCornerFactor + trCornerFactor;
       
        if(lineThickness <= 0 && cornerFactorSum <= 0)
        {
            quad.Draw(color);
            return;
        }
        
        if (cornerFactorSum <= 0)
        {
            quad.DrawLines(lineThickness, color);
            return;
        }
        
        if (lineThickness <= 0)
        {
            quad.DrawChamferedCornersRelative(color, tlCornerFactor, blCornerFactor, brCornerFactor, trCornerFactor);
            return;
        }
        
        var size = quad.GetSize();
        if(size.Width <= 0 || size.Height <= 0) return;
        
        float halfWidth = size.Width * 0.5f;
        float halfHeight = size.Height * 0.5f;
        var minHalfSize = MathF.Min(halfWidth, halfHeight);
        lineThickness = MathF.Min(lineThickness, minHalfSize);
        
        var cornerLengthPrevTl = MathF.Min(halfWidth * tlCornerFactor, halfWidth);
        var cornerLengthNextTl = MathF.Min(halfHeight * tlCornerFactor, halfHeight);
        
        var cornerLengthPrevBl = MathF.Min(halfHeight * blCornerFactor, halfHeight);
        var cornerLengthNextBl = MathF.Min(halfWidth * blCornerFactor, halfWidth);
        
        var cornerLengthPrevBr = MathF.Min(halfWidth * brCornerFactor, halfWidth);
        var cornerLengthNextBr = MathF.Min(halfHeight * brCornerFactor, halfHeight);
        
        var cornerLengthPrevTr = MathF.Min(halfHeight * trCornerFactor, halfHeight);
        var cornerLengthNextTr = MathF.Min(halfWidth * trCornerFactor, halfWidth);
        
        var nR = quad.NormalRight;
        var nD = quad.NormalDown;
        var nL = -nR;
        var nU = -nD;
        
            
        var chamferA = new ChamferedCorner(quad.A, cornerLengthPrevTl, cornerLengthNextTl, nR, nD);
        var chamferB = new ChamferedCorner(quad.B, cornerLengthPrevBl, cornerLengthNextBl, nU, nR);
        var chamferC = new ChamferedCorner(quad.C, cornerLengthPrevBr, cornerLengthNextBr, nL, nU);
        var chamferD = new ChamferedCorner(quad.D, cornerLengthPrevTr, cornerLengthNextTr, nD, nL);
        
        var chamferLengthA = chamferA.GetChamferLength(nL, lineThickness);
        var chamferLengthB = chamferB.GetChamferLength(nD, lineThickness);
        var chamferLengthC = chamferC.GetChamferLength(nR, lineThickness);
        var chamferLengthD = chamferD.GetChamferLength(nU, lineThickness);
        
        outerChamferPointsHelper.Clear();
        innerChamferPointsHelper.Clear();

        chamferA.AddToList(innerChamferPointsHelper);
        chamferB.AddToList(innerChamferPointsHelper);
        chamferC.AddToList(innerChamferPointsHelper);
        chamferD.AddToList(innerChamferPointsHelper);
        
        chamferA.AddOuterToList(outerChamferPointsHelper, chamferLengthA);
        chamferB.AddOuterToList(outerChamferPointsHelper, chamferLengthB);
        chamferC.AddOuterToList(outerChamferPointsHelper, chamferLengthC);
        chamferD.AddOuterToList(outerChamferPointsHelper, chamferLengthD);
        
        DrawChamferedOutline(lineThickness, color);
        
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
    #endregion
    
    #region Helper Chamfered Corner Lines
    private static List<Vector2> innerChamferPointsHelper = new(16);
    private static List<Vector2> outerChamferPointsHelper = new(16);
    private static List<Vector2> innerChamferPointsInsetResult = new(16);
    private static List<Vector2> innerChamferPointsInsetBuffer = new(16);
    private static List<Vector2> chamferedOutlineTriangulationResultVertices = new(64);
    private static List<int> chamferedOutlineTriangulationResultIndices = new(64);
    
    private readonly struct ChamferedCorner
    {
        #region Fields

        public readonly bool SharpCorner;
        public readonly Vector2 Corner;
        public readonly Vector2 Prev;
        public readonly Vector2 Next;
        public readonly Vector2 ChamferDir;
        public readonly Vector2 ChamferDirPrev;
        public readonly Vector2 ChamferDirNext;
        // public readonly Vector2 ChamferEdgeDir;
        #endregion
        
        #region Constructors
        public ChamferedCorner(Vector2 p, float cornerLength, Vector2 normalPrev, Vector2 normalNext)
        {
            Corner = p;
            if (cornerLength <= 0f)
            {
                Prev = p;
                Next = p;
                // ChamferEdgeDir = Vector2.Zero;
                ChamferDir = -(normalPrev + normalNext).Normalize();
                ChamferDirPrev = ChamferDir;
                ChamferDirNext = ChamferDir;
                SharpCorner = true;
            }
            else
            {
                Prev = p + normalPrev * cornerLength;
                Next = p + normalNext * cornerLength;
                // ChamferEdgeDir = (Next - Prev).Normalize();
                var chamferEdgeDir = (Next - Prev).Normalize();
                ChamferDir = chamferEdgeDir.GetPerpendicularRight();
                ChamferDirPrev = (ChamferDir + (-normalNext)).Normalize();
                ChamferDirNext = (ChamferDir + (-normalPrev)).Normalize();
                SharpCorner = false;
            }
            
        }
        public ChamferedCorner(Vector2 p, float cornerLengthPrev, float cornerLengthNext, Vector2 normalPrev, Vector2 normalNext)
        {
            Corner = p;
            Prev = p + normalPrev * cornerLengthPrev;
            Next = p + normalNext * cornerLengthNext;
            // ChamferEdgeDir = (Next - Prev).Normalize();#
            var chamferEdgeDir = (Next - Prev).Normalize();
            ChamferDir = chamferEdgeDir.GetPerpendicularRight();
            ChamferDirPrev = (ChamferDir + (-normalNext)).Normalize();
            ChamferDirNext = (ChamferDir + (-normalPrev)).Normalize();
            SharpCorner = false;
        }
        public ChamferedCorner(Vector2 p, float cornerLengthPrev, float cornerLengthNext, Vector2 normalPrev, Vector2 normalNext, Vector2 chamferDirPrev, Vector2 chamferDirNext)
        {
            Corner = p;
            Prev = p + normalPrev * cornerLengthPrev;
            Next = p + normalNext * cornerLengthNext;
            // ChamferEdgeDir = (Next - Prev).Normalize();
            var chamferEdgeDir = (Next - Prev).Normalize();
            ChamferDir = chamferEdgeDir.GetPerpendicularRight();
            ChamferDirPrev = chamferDirPrev;
            ChamferDirNext = chamferDirNext;
            SharpCorner = false;
        }
        #endregion
        
        #region Functions
        public float GetChamferLength(Vector2 normal, float lineThickness)
        {
            if (SharpCorner)
            {
                return MathF.Sqrt(2f * lineThickness * lineThickness);
            }
            var angleRad = ShapeVec.AngleRad(ChamferDirNext, normal);
            return Triangle.RightTriangleGetHypotenuseFromAdjacent(angleRad, lineThickness);
        }
        public void AddToList(List<Vector2> list)
        {
            if(SharpCorner) list.Add(Corner);
            else
            {
                list.Add(Prev);
                list.Add(Next);
            }
        }
        public void AddOuterToList(List<Vector2> list, float chamferLength)
        {
            if (SharpCorner)
            {
                var outer = Prev + ChamferDir * chamferLength;
                list.Add(outer);
                return;
            }
            var outerPrev = Prev + ChamferDirPrev * chamferLength;
            var outerNext = Next + ChamferDirNext * chamferLength;
            list.Add(outerPrev);
            list.Add(outerNext);
        }
        public void AddOuterToList(List<Vector2> list, float chamferLengthPrev, float chamferLengthNext)
        {
            var outerPrev = Prev + ChamferDirPrev * chamferLengthPrev;
            var outerNext = Next + ChamferDirNext * chamferLengthNext;
            list.Add(outerPrev);
            list.Add(outerNext);
        }
        #endregion
    }
    
    private static void DrawChamferedOutline(float lineThickness, ColorRgba color)
    {
        CleanInPlace(innerChamferPointsHelper, 0.01f, 0.01f);
        CleanInPlace(outerChamferPointsHelper, 0.01f, 0.01f);
        
        innerChamferPointsInsetResult.Clear();
        Polygon.InsetConvex(innerChamferPointsHelper, lineThickness, innerChamferPointsInsetResult, innerChamferPointsInsetBuffer);

        if (innerChamferPointsInsetResult.Count <= 0) return;

        var rayColor = color.ToRayColor();
        if (innerChamferPointsInsetResult.Count < 3)
        {
            var center = innerChamferPointsInsetResult.Count == 1 ? innerChamferPointsInsetResult[0] : (innerChamferPointsHelper[0] + innerChamferPointsHelper[1]) * 0.5f;
            for (int i = 0; i < outerChamferPointsHelper.Count; i++)
            {
                var p1 = outerChamferPointsHelper[i];
                var p2 = outerChamferPointsHelper[(i + 1) % outerChamferPointsHelper.Count];
                Raylib.DrawTriangle(p1, p2, center, rayColor);
            }

            return;
        }
        
        chamferedOutlineTriangulationResultIndices.Clear();
        chamferedOutlineTriangulationResultVertices.Clear();
        Polygon.TriangulateConvexOutline(outerChamferPointsHelper, innerChamferPointsInsetResult, chamferedOutlineTriangulationResultVertices, chamferedOutlineTriangulationResultIndices);
        
        for (int t = 0; t < chamferedOutlineTriangulationResultIndices.Count; t += 3)
        {
            int i0 = chamferedOutlineTriangulationResultIndices[t + 0];
            int i1 = chamferedOutlineTriangulationResultIndices[t + 1];
            int i2 = chamferedOutlineTriangulationResultIndices[t + 2];
        
            Vector2 v0 = chamferedOutlineTriangulationResultVertices[i0];
            Vector2 v1 = chamferedOutlineTriangulationResultVertices[i1];
            Vector2 v2 = chamferedOutlineTriangulationResultVertices[i2];
            
            Raylib.DrawTriangle(v0, v1, v2, rayColor);
        }
    }
    private static bool CleanInPlace(List<Vector2> poly, float mergeEps = 1e-3f, float collinearEps = 1e-5f)
    {
        if (poly.Count < 3) return false;

        float mergeEps2 = mergeEps * mergeEps;

        // 1) remove consecutive near-duplicates
        for (int i = poly.Count - 1; i > 0; i--)
        {
            if (Vector2.DistanceSquared(poly[i], poly[i - 1]) <= mergeEps2)
                poly.RemoveAt(i);
        }

        // 2) remove closing near-duplicate
        if (poly.Count > 1 && Vector2.DistanceSquared(poly[0], poly[^1]) <= mergeEps2)
            poly.RemoveAt(poly.Count - 1);

        if (poly.Count < 3) return false;

        // 3) remove near-collinear points
        for (int i = poly.Count - 1; poly.Count >= 3 && i >= 0; i--)
        {
            int i0 = (i - 1 + poly.Count) % poly.Count;
            int i1 = i;
            int i2 = (i + 1) % poly.Count;

            Vector2 a = poly[i0];
            Vector2 b = poly[i1];
            Vector2 c = poly[i2];

            Vector2 ab = b - a;
            Vector2 bc = c - b;

            // If angle is ~180°, cross ~ 0
            float cross = ab.X * bc.Y - ab.Y * bc.X;
            if (MathF.Abs(cross) <= collinearEps)
                poly.RemoveAt(i1);
        }

        return poly.Count >= 3;
    }

    #endregion
    
}