
using System.Numerics;
using Raylib_CsLo;
using ShapeEngine.Color;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Shapes;
using ShapeEngine.UI;

namespace ShapeEngine.Lib;

    /// <summary>
    /// Determines how the end of a line is drawn.
    /// </summary>
    public enum LineCapType
    {
        /// <summary>
        /// Line is drawn exactly from start to end without any cap.
        /// </summary>
        None = 0,
        /// <summary>
        /// The line is extended by the thickness without any cap.
        /// </summary>
        Extended = 1,
        /// <summary>
        /// The line remains the same length and is drawn with a cap.
        /// Roundness is determined by the cap points.
        /// </summary>
        Capped = 2,
        /// <summary>
        /// The line is extended by the thickness and is drawn with a cap.
        /// Roundness is determined by the cap points.
        /// </summary>
        CappedExtended = 3
    }
    
   
    public static class ShapeDrawing
    {
        
        public static float LineMinThickness = 0.5f;

        
        #region Custom Line Drawing
        public static void DrawLine(Vector2 start, Vector2 end, float thickness, ColorRgba colorRgba, LineCapType capType = LineCapType.None, int capPoints = 0)
        {
            if (thickness < LineMinThickness) thickness = LineMinThickness;
            var w = end - start;
            float ls = w.X * w.X + w.Y * w.Y; // w.LengthSquared();
            if (ls <= 0f) return;
            
            var dir = w / MathF.Sqrt(ls);
            var pR = new Vector2(-dir.Y, dir.X);//perpendicular right
            var pL = new Vector2(dir.Y, -dir.X);//perpendicular left
            
            if (capType == LineCapType.Extended) //expand outwards
            {
                start -= dir * thickness;
                end += dir * thickness;
            }
            else if (capType == LineCapType.Capped)//shrink inwards so that the line with cap is the same length
            {
                start += dir * thickness;
                end -= dir * thickness;
            }
            
            var tl = start + pL * thickness;
            var bl = start + pR * thickness;
            var br = end + pR * thickness;
            var tr = end + pL * thickness;
            
            Raylib.DrawTriangle(tl, bl, br, colorRgba.ToRayColor());
            Raylib.DrawTriangle(tl, br, tr, colorRgba.ToRayColor());

            if (capType is LineCapType.None or LineCapType.Extended) return;
            if (capPoints <= 0) return;
            
            //Draw Cap
            if (capPoints == 1)
            {
                var capStart = start - dir * thickness;
                var capEnd = end + dir * thickness;
                
                Raylib.DrawTriangle(tl, capStart, bl, colorRgba.ToRayColor());
                Raylib.DrawTriangle(tr, br, capEnd, colorRgba.ToRayColor());
            }
            else
            {
                var curStart = tl;
                var curEnd = br;
                float angleStep = (180f / (capPoints + 1)) * ShapeMath.DEGTORAD;
                    
                for (var i = 1; i <= capPoints; i++)
                {
                    var pStart = start + pL.Rotate(- angleStep * i) * thickness;
                    Raylib.DrawTriangle(pStart, start, curStart, colorRgba.ToRayColor());
                    curStart = pStart;
                        
                    var pEnd = end + pR.Rotate(- angleStep * i) * thickness;
                    Raylib.DrawTriangle(pEnd, end, curEnd, colorRgba.ToRayColor());
                    curEnd = pEnd;
                }
                Raylib.DrawTriangle(curStart, bl, start, colorRgba.ToRayColor());
                Raylib.DrawTriangle(curEnd, tr, end, colorRgba.ToRayColor());

            }
        }

        public static void DrawLine(float startX, float startY, float endX, float endY, float thickness, ColorRgba colorRgba,
            LineCapType capType = LineCapType.None, int capPoints = 0)
        {
            DrawLine(new(startX, startY), new(endX, endY), thickness, colorRgba, capType, capPoints);
        }
        
        // public static void DrawLineBackup(Vector2 start, Vector2 end, float thickness, ShapeColor color, LineEndCap lineEndCap = LineEndCap.None, int endCapPoints = 0)
        // {
        //     if (thickness < LineMinThickness) thickness = LineMinThickness;
        //     var w = (end - start);
        //     if (w.LengthSquared() <= 0f) return;
        //     
        //     var dir = w.Normalize();
        //     var pR = dir.GetPerpendicularRight();
        //     var pL = dir.GetPerpendicularLeft();
        //     
        //     if (lineEndCap == LineEndCap.Extended)
        //     {
        //         start -= dir * thickness;
        //         end += dir * thickness;
        //     }
        //     
        //     var tl = start + pL * thickness;
        //     var bl = start + pR * thickness;
        //     var br = end + pR * thickness;
        //     var tr = end + pL * thickness;
        //     Raylib.DrawTriangle(tl, bl, br, color);
        //     Raylib.DrawTriangle(tl, br, tr, color);
        //     
        //     if (lineEndCap == LineEndCap.Capped && endCapPoints > 0)
        //     {
        //         if (endCapPoints == 1)
        //         {
        //             var capStart = start - dir * thickness;
        //             var capEnd = end + dir * thickness;
        //         
        //             Raylib.DrawTriangle(tl, capStart, bl, color);
        //             Raylib.DrawTriangle(tr, br, capEnd, color);
        //         }
        //         else
        //         {
        //             var curStart = tl;
        //             var curEnd = br;
        //             float angleStep = (180f / (endCapPoints + 1)) * ShapeMath.DEGTORAD;
        //             
        //             // DrawCircleV(curEnd, 6f, GREEN);
        //             for (var i = 1; i <= endCapPoints; i++)
        //             {
        //                 var pStart = start + pL.Rotate(- angleStep * i) * thickness;
        //                 Raylib.DrawTriangle(pStart, start, curStart, color);
        //                 curStart = pStart;
        //                 
        //                 var pEnd = end + pR.Rotate(- angleStep * i) * thickness;
        //                 Raylib.DrawTriangle(pEnd, end, curEnd, color);
        //                 // DrawCircleV(pEnd, 6f, WHITE);
        //                 curEnd = pEnd;
        //             }
        //             Raylib.DrawTriangle(curStart, bl, start, color);
        //             Raylib.DrawTriangle(curEnd, tr, end, color);
        //             // DrawCircleV(tr, 6f, RED);
        //
        //         }
        //     }
        // }
        #endregion
        
        #region Intersection
        public static void Draw(this Intersection intersection, float lineThickness, ColorRgba intersectColorRgba, ColorRgba normalColorRgba)
        {
            foreach (var i in intersection.ColPoints)
            {
                DrawCircle(i.Point, lineThickness * 2f, intersectColorRgba);
                Segment normal = new(i.Point, i.Point + i.Normal * lineThickness * 10f);
                normal.Draw(lineThickness, normalColorRgba);
            }
        }

        #endregion
        
        #region Pixel
        public static void DrawPixel(Vector2 pos, ColorRgba colorRgba) => Raylib.DrawPixelV(pos, colorRgba.ToRayColor()); 
        public static void DrawPixel(float x, float y, ColorRgba colorRgba) => Raylib.DrawPixelV(new(x, y), colorRgba.ToRayColor());
        #endregion

        #region Point
        public static void Draw(this Vector2 p, float radius, ColorRgba colorRgba, int segments = 16)
        {
            DrawCircle(p, radius, colorRgba, segments);
        }
        public static void Draw(this Points points, float r, ColorRgba colorRgba, int segments = 16)
        {
            foreach (var p in points)
            {
                p.Draw(r, colorRgba, segments);
            }
        }

        #endregion

        #region Segment
        
        public static void Draw(this Segment segment, float thickness, ColorRgba colorRgba, LineCapType capType = LineCapType.None, int capPoints = 0 ) 
            => DrawLine(segment.Start, segment.End, thickness, colorRgba, capType, capPoints);
        
        public static void Draw(this Segments segments, float thickness, ColorRgba colorRgba, LineCapType capType = LineCapType.None, int capPoints = 0)
        {
            if (segments.Count <= 0) return;
            foreach (var seg in segments)
            {
                seg.Draw(thickness, colorRgba, capType, capPoints);
            }
        }
        public static void Draw(this Segments segments, float thickness, List<ColorRgba> colors, LineCapType capType = LineCapType.None, int capPoints = 0)
        {
            if (segments.Count <= 0 || colors.Count <= 0) return;
            for (int i = 0; i < segments.Count; i++)
            {
                var c = colors[i % colors.Count];
                segments[i].Draw(thickness, c, capType, capPoints);
            }
        }
        
        public static void DrawVertices(this Segment segment, float vertexRadius, ColorRgba colorRgba, int vertexSegments = 16)
        {
            segment.Start.Draw( vertexRadius, colorRgba, vertexSegments);
            segment.End.Draw(vertexRadius, colorRgba, vertexSegments);
        }
       
        public static Segments CreateLightningLine(this Segment segment, int segments = 10, float maxSway = 80f)
        {
            Segments result = new();
            Vector2 w = segment.End - segment.Start;
            Vector2 dir = ShapeVec.Normalize(w);
            Vector2 n = new Vector2(dir.Y, -dir.X);
            float length = w.Length();

            float prevDisplacement = 0;
            Vector2 cur = segment.Start;
            //result.Add(start);

            float segmentLength = length / segments;
            float remainingLength = length;
            List<Vector2> accumulator = new()
            {
                segment.Start
            };
            while (remainingLength > 0f)
            {
                float randSegmentLength = ShapeRandom.RandF() * segmentLength;
                remainingLength -= randSegmentLength;
                if (remainingLength <= 0f)
                {
                    if(accumulator.Count == 1)
                    {
                        result.Add(new(accumulator[0], segment.End));
                    }
                    else
                    {
                        result.Add(new(result[result.Count - 1].End, segment.End));
                    }
                    break;
                }
                float scale = randSegmentLength / segmentLength;
                float displacement = ShapeRandom.RandF(-maxSway, maxSway);
                displacement -= (displacement - prevDisplacement) * (1 - scale);
                cur = cur + dir * randSegmentLength;
                Vector2 p = cur + displacement * n;
                accumulator.Add(p);
                if(accumulator.Count == 2)
                {
                    result.Add(new(accumulator[0], accumulator[1]));
                    accumulator.Clear();
                }
                prevDisplacement = displacement;
            }
            return result;
        }
        public static Segments CreateLightningLine(this Segment segment, float segmentLength = 5f, float maxSway = 80f)
        {
            Segments result = new();
            Vector2 w = segment.End - segment.Start;
            Vector2 dir = ShapeVec.Normalize(w);
            Vector2 n = new Vector2(dir.Y, -dir.X);
            float length = w.Length();

            float prevDisplacement = 0;
            Vector2 cur = segment.Start;
            List<Vector2> accumulator = new()
            {
                segment.Start
            };
            float remainingLength = length;
            while (remainingLength > 0f)
            {
                float randSegmentLength = ShapeRandom.RandF() * segmentLength;
                remainingLength -= randSegmentLength;
                if (remainingLength <= 0f)
                {
                    if (accumulator.Count == 1)
                    {
                        result.Add(new(accumulator[0], segment.End));
                    }
                    else
                    {
                        result.Add(new(result[result.Count - 1].End, segment.End));
                    }
                    break;
                }
                float scale = randSegmentLength / segmentLength;
                float displacement = ShapeRandom.RandF(-maxSway, maxSway);
                displacement -= (displacement - prevDisplacement) * (1 - scale);
                cur = cur + dir * randSegmentLength;
                Vector2 p = cur + displacement * n;
                accumulator.Add(p);
                if (accumulator.Count == 2)
                {
                    result.Add(new(accumulator[0], accumulator[1]));
                    accumulator.Clear();
                }
                prevDisplacement = displacement;
            }
            return result;
        }
        
        public static void DrawLineDotted(Vector2 start, Vector2 end, int gaps, float thickness, ColorRgba colorRgba, LineCapType capType = LineCapType.None, int capPoints = 0)
        {
            if (gaps <= 0) DrawLine(start, end, thickness, colorRgba, capType, capPoints);
            else
            {
                Vector2 w = end - start;
                float l = w.Length();
                Vector2 dir = w / l;
                int totalGaps = gaps * 2 + 1;
                float size = l / totalGaps;
                Vector2 offset = dir * size;

                Vector2 cur = start;
                for (int i = 0; i < totalGaps; i++)
                {
                    if (i % 2 == 0)
                    {
                        Vector2 next = cur + offset;
                        DrawLine(cur, next, thickness, colorRgba, capType, capPoints);
                        cur = next;

                    }
                    else
                    {
                        cur += offset; //gap
                    }
                }
            }
        }
        public static void DrawLineDotted(Vector2 start, Vector2 end, int gaps, float gapSizeF, float thickness, ColorRgba colorRgba, LineCapType capType = LineCapType.None, int capPoints = 0)
        {
            if (gaps <= 0) DrawLine(start, end, thickness, colorRgba, capType, capPoints);
            else
            {
                Vector2 w = end - start;
                float l = w.Length();
                Vector2 dir = w / l;

                float totalGapSize = l * gapSizeF;
                float remaining = l - totalGapSize;
                float gapSize = totalGapSize / gaps;
                float size = remaining / (gaps + 1);

                Vector2 gapOffset = dir * gapSize;
                Vector2 offset = dir * size;

                int totalGaps = gaps * 2 + 1;
                Vector2 cur = start;
                for (int i = 0; i < totalGaps; i++)
                {
                    if (i % 2 == 0)
                    {
                        Vector2 next = cur + offset;
                        DrawLine(cur, next, thickness, colorRgba, capType, capPoints);
                        cur = next;
                    }
                    else
                    {
                        cur += gapOffset; //gap
                    }
                }
            }
        }
        public static void DrawLineGlow(Vector2 start, Vector2 end, float width, float endWidth, ColorRgba colorRgba, ColorRgba endColorRgba, int steps, LineCapType capType = LineCapType.None, int capPoints = 0)
        {
            float wStep = (endWidth - width) / steps;

            int rStep = (endColorRgba.R - colorRgba.R) / steps;
            int gStep = (endColorRgba.G - colorRgba.G) / steps;
            int bStep = (endColorRgba.B - colorRgba.B) / steps;
            int aStep = (endColorRgba.A - colorRgba.A) / steps;

            for (int i = steps; i >= 0; i--)
            {
                DrawLine
                (
                    start, end, width + wStep * i,
                    new
                    (
                        colorRgba.R + rStep * i,
                        colorRgba.G + gStep * i,
                        colorRgba.B + bStep * i,
                        colorRgba.A + aStep * i
                    ),
                    capType,
                    capPoints
                );
            }
        }
        public static void DrawDotted(this Segment segment, int gaps, float thickness, ColorRgba colorRgba, LineCapType capType = LineCapType.None, int capPoints = 0)
        {
            DrawLineDotted(segment.Start, segment.End, gaps, thickness, colorRgba, capType, capPoints);
        }
        public static void DrawDotted(this Segment segment, int gaps, float gapSizeF, float thickness, ColorRgba colorRgba, LineCapType capType = LineCapType.None, int capPoints = 0)
        {
            DrawLineDotted(segment.Start, segment.End, gaps, gapSizeF, thickness, colorRgba, capType, capPoints);
        }
        public static void DrawDotted(this Segments segments, int gaps, float thickness, ColorRgba colorRgba, LineCapType capType = LineCapType.None, int capPoints = 0)
        {
            foreach (var seg in segments)
            {
                seg.DrawDotted(gaps, thickness, colorRgba, capType, capPoints);
            }
        }
        public static void DrawDotted(this Segments segments, int gaps, float gapSizeF, float thickness, ColorRgba colorRgba, LineCapType capType = LineCapType.None, int capPoints = 0)
        {
            foreach (var seg in segments)
            {
                seg.DrawDotted(gaps, gapSizeF, thickness, colorRgba, capType, capPoints);
            }
        }

        public static void DrawGlow(this Segment segment, float width, float endWidth, ColorRgba colorRgba, ColorRgba endColorRgba, int steps, LineCapType capType = LineCapType.None, int capPoints = 0)
        {
            DrawLineGlow(segment.Start, segment.End, width, endWidth, colorRgba, endColorRgba, steps, capType, capPoints);
        }
        public static void DrawGlow(this Segments segments, float width, float endWidth, ColorRgba colorRgba, ColorRgba endColorRgba, int steps, LineCapType capType = LineCapType.None, int capPoints = 0)
        {
            foreach (var seg in segments)
            {
                seg.DrawGlow(width, endWidth, colorRgba, endColorRgba, steps, capType, capPoints);
            }
        }

        
        #endregion

        #region Circle
        public static void DrawCircle(Vector2 center, float radius, ColorRgba colorRgba, int segments = 16)
        {
            if (segments < 6) segments = 6;
            Raylib.DrawCircleSector(center, radius, 0, 360, segments, colorRgba.ToRayColor());
        }

        public static void Draw(this Circle c, ColorRgba colorRgba)
        {
            DrawCircle(c.Center, c.Radius, colorRgba);
        } 
        public static void Draw(this Circle c, ColorRgba colorRgba, int segments)
        {
            DrawCircle(c.Center, c.Radius, colorRgba, segments);
        }
        public static void DrawLines(this Circle c, float lineThickness, int sides, ColorRgba colorRgba) => DrawPolyLinesEx(c.Center, sides, c.Radius, 0f, lineThickness, colorRgba.ToRayColor());
        public static void DrawLines(this Circle c, float lineThickness, float rotDeg, int sides, ColorRgba colorRgba) => DrawPolyLinesEx(c.Center, sides, c.Radius, rotDeg, lineThickness, colorRgba.ToRayColor());
        public static void DrawLines(this Circle c, float lineThickness, ColorRgba colorRgba, float sideLength = 8f)
        {
            int sides = GetCircleSideCount(c.Radius, sideLength);
            DrawPolyLinesEx(c.Center, sides, c.Radius, 0f, lineThickness, colorRgba.ToRayColor());
        }
        
        
        /// <summary>
        /// Very usefull for drawing small/tiny circles. Drawing the circle as rect increases performance a lot.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="colorRgba"></param>
        public static void DrawCircleFast(Vector2 center, float radius, ColorRgba colorRgba)
        {
            Rect r = new(center, new Vector2(radius * 2f), new Vector2(0.5f));
            r.Draw(colorRgba);
        }
        
        public static void DrawCircleLines(Vector2 center, float radius, float lineThickness, int sides, ColorRgba colorRgba) => DrawPolyLinesEx(center, sides, radius, 0f, lineThickness, colorRgba.ToRayColor());
        public static void DrawCircleLines(Vector2 center, float radius, float lineThickness, float rotDeg, int sides, ColorRgba colorRgba) => DrawPolyLinesEx(center, sides, radius, rotDeg, lineThickness, colorRgba.ToRayColor());
        public static void DrawCircleLines(Vector2 center, float radius, float lineThickness, ColorRgba colorRgba, float sideLength = 8f)
        {
            int sides = GetCircleSideCount(radius, sideLength);
            DrawPolyLinesEx(center, sides, radius, 0f, lineThickness, colorRgba.ToRayColor());
        }

        public static void DrawSector(this Circle c, float startAngleDeg, float endAngleDeg, int segments, ColorRgba colorRgba)
        {
            Raylib.DrawCircleSector(c.Center, c.Radius, TransformAngleDeg(startAngleDeg), TransformAngleDeg(endAngleDeg), segments, colorRgba.ToRayColor());
        }
        public static void DrawCircleSector(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, int segments, ColorRgba colorRgba)
        {
            Raylib.DrawCircleSector(center, radius, TransformAngleDeg(startAngleDeg), TransformAngleDeg(endAngleDeg), segments, colorRgba.ToRayColor());
        }
        
        public static void DrawSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, float lineThickness, ColorRgba colorRgba, bool closed = true, float sideLength = 8f)
        {
            DrawCircleSectorLines(c.Center, c.Radius, startAngleDeg, endAngleDeg, lineThickness, colorRgba, closed, sideLength);
        }
        public static void DrawSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, float lineThickness, ColorRgba colorRgba, bool closed = true, float sideLength = 8f)
        {
            DrawCircleSectorLines(c.Center, c.Radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, lineThickness, colorRgba, closed, sideLength); ;
        }
        public static void DrawSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, int sides, float lineThickness, ColorRgba colorRgba, bool closed = true)
        {
            DrawCircleSectorLines(c.Center, c.Radius, startAngleDeg, endAngleDeg, sides, lineThickness, colorRgba, closed);
        }
        public static void DrawSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, int sides, float lineThickness, ColorRgba colorRgba, bool closed = true)
        {
            DrawCircleSectorLines(c.Center, c.Radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, sides, lineThickness, colorRgba, closed);
        }
        
        
        public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, float lineThickness, ColorRgba colorRgba, bool closed = true, float sideLength = 8f)
        {
            float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
            float endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
            float anglePiece = endAngleRad - startAngleRad;
            int sides = GetCircleArcSideCount(radius, MathF.Abs(anglePiece * RAD2DEG), sideLength);
            float angleStep = anglePiece / sides;
            if (closed)
            {
                var sectorStart = center + ShapeVec.Rotate(ShapeVec.Right() * radius + new Vector2(lineThickness / 2, 0), startAngleRad);
                DrawLine(center, sectorStart, lineThickness, colorRgba, LineCapType.CappedExtended, 2);

                var sectorEnd = center + ShapeVec.Rotate(ShapeVec.Right() * radius + new Vector2(lineThickness / 2, 0), endAngleRad);
                DrawLine(center, sectorEnd, lineThickness, colorRgba, LineCapType.CappedExtended, 2);
            }
            for (int i = 0; i < sides; i++)
            {
                Vector2 start = center + ShapeVec.Rotate(ShapeVec.Right() * radius, startAngleRad + angleStep * i);
                Vector2 end = center + ShapeVec.Rotate(ShapeVec.Right() * radius, startAngleRad + angleStep * (i + 1));
                DrawLine(start, end, lineThickness, colorRgba, LineCapType.CappedExtended, 2);
            }
        }
        public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, float lineThickness, ColorRgba colorRgba, bool closed = true, float sideLength = 8f)
        {
            DrawCircleSectorLines(center, radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, lineThickness, colorRgba, closed, sideLength); ;
        }
        public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, int sides, float lineThickness, ColorRgba colorRgba, bool closed = true)
        {
            float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
            float endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
            float anglePiece = endAngleDeg - startAngleRad;
            float angleStep = MathF.Abs(anglePiece) / sides;
            if (closed)
            {
                Vector2 sectorStart = center + ShapeVec.Rotate(ShapeVec.Right() * radius + new Vector2(lineThickness / 2, 0), startAngleRad);
                DrawLine(center, sectorStart, lineThickness, colorRgba, LineCapType.CappedExtended, 2);

                Vector2 sectorEnd = center + ShapeVec.Rotate(ShapeVec.Right() * radius + new Vector2(lineThickness / 2, 0), endAngleRad);
                DrawLine(center, sectorEnd, lineThickness, colorRgba, LineCapType.CappedExtended, 2);
            }
            for (int i = 0; i < sides; i++)
            {
                Vector2 start = center + ShapeVec.Rotate(ShapeVec.Right() * radius, startAngleRad + angleStep * i);
                Vector2 end = center + ShapeVec.Rotate(ShapeVec.Right() * radius, startAngleRad + angleStep * (i + 1));
                DrawLine(start, end, lineThickness, colorRgba, LineCapType.CappedExtended, 2);
            }
        }
        public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, int sides, float lineThickness, ColorRgba colorRgba, bool closed = true)
        {
            DrawCircleSectorLines(center, radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, sides, lineThickness, colorRgba, closed);
        }

        public static void DrawCircleLinesDotted(Vector2 center, float radius, int sidesPerGap, float lineThickness, ColorRgba colorRgba, float sideLength = 8f, LineCapType capType = LineCapType.CappedExtended,  int capPoints = 2)
        {
            float anglePieceRad = 360 * ShapeMath.DEGTORAD;
            int sides = GetCircleArcSideCount(radius, MathF.Abs(anglePieceRad * RAD2DEG), sideLength);
            float angleStep = anglePieceRad / sides;

            //int totalGaps = gaps * 2 + 1;
            //float circum = 2f * PI * radius;
            //float size = circum / totalGaps;
            float size = sideLength * sidesPerGap;
            float remainingSize = size;
            bool gap = false;
            for (int i = 0; i < sides; i++)
            {
                if (!gap)
                {
                    Vector2 start = center + ShapeVec.Rotate(ShapeVec.Right() * radius, angleStep * i);
                    Vector2 end = center + ShapeVec.Rotate(ShapeVec.Right() * radius, angleStep * (i + 1));
                    DrawLine(start, end, lineThickness, colorRgba, capType, capPoints);
                }

                remainingSize -= sideLength;
                if (remainingSize <= 0f)
                {
                    gap = !gap;
                    remainingSize = size;
                }
            }
        }
        public static void DrawCircleCheckeredLines(Vector2 pos, Vector2 alignement, float radius, float spacing, float lineThickness, float angleDeg, ColorRgba lineColorRgba, ColorRgba bgColorRgba, int circleSegments)
        {

            float maxDimension = radius;
            Vector2 size = new Vector2(radius, radius) * 2f;
            Vector2 aVector = alignement * size;
            Vector2 center = pos - aVector + size / 2;
            float rotRad = angleDeg * ShapeMath.DEGTORAD;

            if (bgColorRgba.A > 0) DrawCircle(center, radius, bgColorRgba, circleSegments);

            Vector2 cur = new(-spacing / 2, 0f);
            while (cur.X > -maxDimension)
            {
                Vector2 p = center + ShapeVec.Rotate(cur, rotRad);

                //float y = MathF.Sqrt((radius * radius) - (cur.X * cur.X));
                float angle = MathF.Acos(cur.X / radius);
                float y = radius * MathF.Sin(angle);

                Vector2 up = new(0f, -y);
                Vector2 down = new(0f, y);
                Vector2 start = p + ShapeVec.Rotate(up, rotRad);
                Vector2 end = p + ShapeVec.Rotate(down, rotRad);
                DrawLine(start, end, lineThickness, lineColorRgba);
                cur.X -= spacing;
            }

            cur = new(spacing / 2, 0f);
            while (cur.X < maxDimension)
            {
                Vector2 p = center + ShapeVec.Rotate(cur, rotRad);
                //float y = MathF.Sqrt((radius * radius) - (cur.X * cur.X));
                float angle = MathF.Acos(cur.X / radius);
                float y = radius * MathF.Sin(angle);

                Vector2 up = new(0f, -y);
                Vector2 down = new(0f, y);
                Vector2 start = p + ShapeVec.Rotate(up, rotRad);
                Vector2 end = p + ShapeVec.Rotate(down, rotRad);
                DrawLine(start, end, lineThickness, lineColorRgba);
                cur.X += spacing;
            }

        }
        
        private static int GetCircleSideCount(float radius, float maxLength = 10f)
        {
            float circumference = 2.0f * PI * radius;
            return (int)MathF.Max(circumference / maxLength, 1);
        }
        private static int GetCircleArcSideCount(float radius, float angleDeg, float maxLength = 10f)
        {
            float circumference = 2.0f * PI * radius * (angleDeg / 360f);
            return (int)MathF.Max(circumference / maxLength, 1);
        }
        private static float TransformAngleDeg(float angleDeg) { return 450f - angleDeg; }
        private static float TransformAngleRad(float angleRad) { return 2.5f * PI - angleRad; }

        // private static Vector2 GetOffsetDir(Vector2 a, Vector2 b, Vector2 c, bool inside)
        // {
        //     var ab = b - a;
        //     var bc = b - c;
        //     var dir = (ab + bc).GetPerpendicularLeft().Normalize();
        //     // var dir = (ab.GetPerpendicularLeft() + bc.GetPerpendicularLeft()).Normalize();
        //     return inside ? dir : -dir;
        // }
        #endregion

        #region Ring
        public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float lineThickness, ColorRgba colorRgba, float sideLength = 8f)
        {
            DrawCircleLines(center, innerRadius, lineThickness, colorRgba, sideLength);
            DrawCircleLines(center, outerRadius, lineThickness, colorRgba, sideLength);
        }
        public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float lineThickness, ColorRgba colorRgba, float sideLength = 8f)
        {
            DrawCircleSectorLines(center, innerRadius, startAngleDeg, endAngleDeg, lineThickness, colorRgba, false, sideLength);
            DrawCircleSectorLines(center, outerRadius, startAngleDeg, endAngleDeg, lineThickness, colorRgba, false, sideLength);

            float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
            float endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
            Vector2 innerStart = center + ShapeVec.Rotate(ShapeVec.Right() * innerRadius - new Vector2(lineThickness / 2, 0), startAngleRad);
            Vector2 outerStart = center + ShapeVec.Rotate(ShapeVec.Right() * outerRadius + new Vector2(lineThickness / 2, 0), startAngleRad);
            DrawLine(innerStart, outerStart, lineThickness, colorRgba, LineCapType.CappedExtended, 2);

            Vector2 innerEnd = center + ShapeVec.Rotate(ShapeVec.Right() * innerRadius - new Vector2(lineThickness / 2, 0), endAngleRad);
            Vector2 outerEnd = center + ShapeVec.Rotate(ShapeVec.Right() * outerRadius + new Vector2(lineThickness / 2, 0), endAngleRad);
            DrawLine(innerEnd, outerEnd, lineThickness, colorRgba, LineCapType.CappedExtended, 2);
        }
        public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, float lineThickness, ColorRgba colorRgba, float sideLength = 8f)
        {
            DrawRingLines(center, innerRadius, outerRadius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, lineThickness, colorRgba, sideLength);
        }
        public static void DrawRing(Vector2 center, float innerRadius, float outerRadius, ColorRgba colorRgba, float sideLength = 8f)
        {
            DrawRing(center, innerRadius, outerRadius, 0, 360, colorRgba, sideLength);
        }
        public static void DrawRing(Vector2 center, float innerRadius, float outerRadius, int sides, ColorRgba colorRgba)
        {
            DrawRing(center, innerRadius, outerRadius, 0, 360, sides, colorRgba);
        }
        public static void DrawRing(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, ColorRgba colorRgba, float sideLength = 10f)
        {
            float start = TransformAngleDeg(startAngleDeg);
            float end = TransformAngleDeg(endAngleDeg);
            float anglePiece = end - start;
            int sides = GetCircleArcSideCount(outerRadius, MathF.Abs(anglePiece), sideLength);
            Raylib.DrawRing(center, innerRadius, outerRadius, start, end, sides, colorRgba.ToRayColor());
        }
        public static void DrawRing(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, int sides, ColorRgba colorRgba)
        {
            Raylib.DrawRing(center, innerRadius, outerRadius, TransformAngleDeg(startAngleDeg), TransformAngleDeg(endAngleDeg), sides, colorRgba.ToRayColor());
        }
        public static void DrawRing(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, ColorRgba colorRgba, float sideLength = 10f)
        {
            DrawRing(center, innerRadius, outerRadius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, colorRgba, sideLength);
        }
        public static void DrawRing(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, int sides, ColorRgba colorRgba)
        {
            DrawRing(center, innerRadius, outerRadius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, sides, colorRgba);
        }

        #endregion

        #region Rectangle

        public static void Draw(this NinePatchRect npr, ColorRgba colorRgba)
        {
            var rects = npr.Rects;
            foreach (var r in rects)
            {
                r.Draw(colorRgba);
            }
        }
        public static void Draw(this NinePatchRect npr, ColorRgba sourceColorRgba, ColorRgba patchColorRgba)
        {
            npr.Source.Draw(sourceColorRgba);
            var rects = npr.Rects;
            foreach (var r in rects)
            {
                r.Draw(patchColorRgba);
            }
        }
        public static void DrawLines(this NinePatchRect npr, float lineThickness, ColorRgba colorRgba)
        {
            var rects = npr.Rects;
            foreach (var r in rects)
            {
                r.DrawLines(lineThickness, colorRgba);
            }
        }
        public static void DrawLines(this NinePatchRect npr, float sourceLineThickness, float patchLineThickness, ColorRgba sourceColorRgba, ColorRgba patchColorRgba)
        {
            npr.Source.DrawLines(sourceLineThickness, sourceColorRgba);
            var rects = npr.Rects;
            foreach (var r in rects)
            {
                r.DrawLines(patchLineThickness, patchColorRgba);
            }
        }
        
        public static void DrawGrid(this Rect r, int lines, float lineThickness, ColorRgba colorRgba)
        {
            //float hGap = r.width / lines;
            //float vGap = r.height / lines;
            var xOffset = new Vector2(r.Width / lines, 0f);// * i;
            var yOffset = new Vector2(0f, r.Height / lines);// * i;
     
            var tl = r.TopLeft;
            var tr = tl + new Vector2(r.Width, 0);
            var bl = tl + new Vector2(0, r.Height);

            for (var i = 0; i < lines; i++)
            {
                Raylib.DrawLineEx(tl + xOffset * i, bl + xOffset * i, lineThickness, colorRgba.ToRayColor());
                Raylib.DrawLineEx(tl + yOffset * i, tr + yOffset * i, lineThickness, colorRgba.ToRayColor());
            }
        }

        public static void DrawRect(Vector2 topLeft, Vector2 bottomRight, ColorRgba colorRgba) => DrawRectangleRec(new Rect(topLeft, bottomRight).Rectangle, colorRgba.ToRayColor());
        public static void DrawRect(Vector2 topLeft, Vector2 bottomRight, Vector2 pivot, float rotDeg, ColorRgba colorRgba) => Draw(new Rect(topLeft, bottomRight),pivot, rotDeg, colorRgba);
        public static void DrawRectLines(Vector2 topLeft, Vector2 bottomRight, float lineThickness, ColorRgba colorRgba) => DrawLines(new Rect(topLeft, bottomRight),lineThickness,colorRgba);
        public static void DrawRectLines(Vector2 topLeft, Vector2 bottomRight, Vector2 pivot, float rotDeg, float lineThickness, ColorRgba colorRgba, LineCapType capType = LineCapType.Extended, int capPoints = 0)
        {
            DrawLines(new Rect(topLeft, bottomRight), pivot, rotDeg, lineThickness, colorRgba, capType, capPoints);

        }

        public static void Draw(this Rect rect, ColorRgba colorRgba) => DrawRectangleRec(rect.Rectangle, colorRgba.ToRayColor());
        public static void Draw(this Rect rect, Vector2 pivot, float rotDeg, ColorRgba colorRgba)
        {
            var rr = rect.Rotate(pivot, rotDeg); // SRect.RotateRect(rect, pivot, rotDeg);
            Raylib.DrawTriangle(rr.tl, rr.bl, rr.br, colorRgba.ToRayColor());
            Raylib.DrawTriangle(rr.br, rr.tr, rr.tl, colorRgba.ToRayColor());
        }
        public static void DrawLines(this Rect rect, float lineThickness, ColorRgba colorRgba) => Raylib.DrawRectangleLinesEx(rect.Rectangle, lineThickness, colorRgba.ToRayColor());
        public static void DrawLines(this Rect rect, Vector2 pivot, float rotDeg, float lineThickness, ColorRgba colorRgba, LineCapType capType = LineCapType.Extended, int capPoints = 0)
        {
            var rr = ShapeRect.Rotate(rect, pivot, rotDeg);

            DrawLine(rr.tl, rr.tr, lineThickness, colorRgba, capType, capPoints);
            DrawLine(rr.bl, rr.br, lineThickness, colorRgba, capType, capPoints);
            DrawLine(rr.tl, rr.bl, lineThickness, colorRgba, capType, capPoints);
            DrawLine(rr.tr, rr.br, lineThickness, colorRgba, capType, capPoints);
            // if (rounded)
            // {
            //     DrawCircle(rr.tl, lineThickness / 2, color);
            //     DrawCircle(rr.tr, lineThickness / 2, color);
            //     DrawCircle(rr.br, lineThickness / 2, color);
            //     DrawCircle(rr.bl, lineThickness / 2, color);
            //
            //     DrawLineEx(rr.tl, rr.tr, lineThickness, color);
            //     DrawLineEx(rr.bl, rr.br, lineThickness, color);
            //     DrawLineEx(rr.tl, rr.bl, lineThickness, color);
            //     DrawLineEx(rr.tr, rr.br, lineThickness, color);
            // }
            // else
            // {
            //     Vector2 leftExtension = ShapeVec.Rotate(new Vector2(-lineThickness / 2, 0f), rotDeg * ShapeMath.DEGTORAD);
            //     Vector2 rightExtension = ShapeVec.Rotate(new Vector2(lineThickness / 2, 0f), rotDeg * ShapeMath.DEGTORAD);
            //    
            //     DrawLineEx(rr.tl + leftExtension, rr.tr + rightExtension, lineThickness, color);
            //     DrawLineEx(rr.bl + leftExtension, rr.br + rightExtension, lineThickness, color);
            //     DrawLineEx(rr.tl, rr.bl, lineThickness, color);
            //     DrawLineEx(rr.tr, rr.br, lineThickness, color);
            // }
        }

        public static void DrawVertices(this Rect rect, float vertexRadius, ColorRgba colorRgba, int circleSegments = 8)
        {
            DrawCircle(rect.TopLeft, vertexRadius, colorRgba    , circleSegments);
            DrawCircle(rect.TopRight, vertexRadius, colorRgba   , circleSegments);
            DrawCircle(rect.BottomLeft, vertexRadius, colorRgba , circleSegments);
            DrawCircle(rect.BottomRight, vertexRadius, colorRgba, circleSegments);
        }
        public static void DrawRounded(this Rect rect, float roundness, int segments, ColorRgba colorRgba) => Raylib.DrawRectangleRounded(rect.Rectangle, roundness, segments, colorRgba.ToRayColor());
        public static void DrawRoundedLines(this Rect rect, float roundness, float lineThickness, int segments, ColorRgba colorRgba) => Raylib.DrawRectangleRoundedLines(rect.Rectangle, roundness, segments, lineThickness, colorRgba.ToRayColor());

        public static void DrawSlantedCorners(this Rect rect, ColorRgba colorRgba, float tlCorner, float trCorner, float brCorner, float blCorner)
        {
            var points = GetSlantedCornerPoints(rect, tlCorner, trCorner, brCorner, blCorner);
            points.DrawPolygonConvex(rect.Center, colorRgba);
        }
        public static void DrawSlantedCorners(this Rect rect, Vector2 pivot, float rotDeg, ColorRgba colorRgba, float tlCorner, float trCorner, float brCorner, float blCorner)
        {
            var poly = GetSlantedCornerPoints(rect, tlCorner, trCorner, brCorner, blCorner);
            poly.Rotate(pivot, rotDeg * ShapeMath.DEGTORAD);
            poly.DrawPolygonConvex(rect.Center, colorRgba);
            //DrawPolygonConvex(poly, rect.Center, color);
            //var points = SPoly.Rotate(GetSlantedCornerPoints(rect, tlCorner, trCorner, brCorner, blCorner), pivot, rotDeg * SUtils.DEGTORAD);
            //DrawPolygonConvex(points, rect.Center, color);
        }
        public static void DrawSlantedCornersLines(this Rect rect, float lineThickness, ColorRgba colorRgba, float tlCorner, float trCorner, float brCorner, float blCorner)
        {
            var points = GetSlantedCornerPoints(rect, tlCorner, trCorner, brCorner, blCorner);
            points.DrawLines(lineThickness, colorRgba);
            // DrawLines(points, lineThickness, color);
        }
        public static void DrawSlantedCornersLines(this Rect rect, Vector2 pivot, float rotDeg, float lineThickness, ColorRgba colorRgba, float tlCorner, float trCorner, float brCorner, float blCorner)
        {
            var poly = GetSlantedCornerPoints(rect, tlCorner, trCorner, brCorner, blCorner);
            poly.Rotate(pivot, rotDeg * ShapeMath.DEGTORAD);
            poly.DrawLines(lineThickness, colorRgba);
            // DrawLines(poly, lineThickness, color);
            //var points = SPoly.Rotate(GetSlantedCornerPoints(rect, tlCorner, trCorner, brCorner, blCorner), pivot, rotDeg * SUtils.DEGTORAD);
            //DrawLines(points, lineThickness, color);
        }

        /// <summary>
        /// Get the points to draw a rectangle with slanted corners.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="tlCorner"></param>
        /// <param name="trCorner"></param>
        /// <param name="brCorner"></param>
        /// <param name="blCorner"></param>
        /// <returns>Returns points in ccw order.</returns>
        private static Polygon GetSlantedCornerPoints(this Rect rect, float tlCorner, float trCorner, float brCorner, float blCorner)
        {
            Vector2 tl = rect.TopLeft;
            Vector2 tr = rect.TopRight;
            Vector2 br = rect.BottomRight;
            Vector2 bl = rect.BottomLeft;
            Polygon points = new();
            if (tlCorner > 0f && tlCorner < 1f)
            {
                points.Add(tl + new Vector2(MathF.Min(tlCorner, rect.Width), 0f));
                points.Add(tl + new Vector2(0f, MathF.Min(tlCorner, rect.Height)));
            }
            if (blCorner > 0f && blCorner < 1f)
            {
                points.Add(bl - new Vector2(0f, MathF.Min(tlCorner, rect.Height)));
                points.Add(bl + new Vector2(MathF.Min(tlCorner, rect.Width), 0f));
            }
            if (brCorner > 0f && brCorner < 1f)
            {
                points.Add(br - new Vector2(MathF.Min(tlCorner, rect.Width), 0f));
                points.Add(br - new Vector2(0f, MathF.Min(tlCorner, rect.Height)));
            }
            if (trCorner > 0f && trCorner < 1f)
            {
                points.Add(tr + new Vector2(0f, MathF.Min(tlCorner, rect.Height)));
                points.Add(tr - new Vector2(MathF.Min(tlCorner, rect.Width), 0f));
            }
            return points;
        }
        /// <summary>
        /// Get the points to draw a rectangle with slanted corners. The corner values are the percentage of the width/height of the rectange the should be used for the slant.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="tlCorner">Should be bewteen 0 - 1</param>
        /// <param name="trCorner">Should be bewteen 0 - 1</param>
        /// <param name="brCorner">Should be bewteen 0 - 1</param>
        /// <param name="blCorner">Should be bewteen 0 - 1</param>
        /// <returns>Returns points in ccw order.</returns>
        private static Polygon GetSlantedCornerPointsRelative(this Rect rect, float tlCorner, float trCorner, float brCorner, float blCorner)
        {
            Vector2 tl = rect.TopLeft;
            Vector2 tr = rect.TopRight;
            Vector2 br = rect.BottomRight;
            Vector2 bl = rect.BottomLeft;
            Polygon points = new();
            if (tlCorner > 0f && tlCorner < 1f)
            {
                points.Add(tl + new Vector2(tlCorner * rect.Width, 0f));
                points.Add(tl + new Vector2(0f, tlCorner * rect.Height));
            }
            if (blCorner > 0f && blCorner < 1f)
            {
                points.Add(bl - new Vector2(0f, tlCorner * rect.Height));
                points.Add(bl + new Vector2(tlCorner * rect.Width, 0f));
            }
            if (brCorner > 0f && brCorner < 1f)
            {
                points.Add(br - new Vector2(tlCorner * rect.Width, 0f));
                points.Add(br - new Vector2(0f, tlCorner * rect.Height));
            }
            if (trCorner > 0f && trCorner < 1f)
            {
                points.Add(tr + new Vector2(0f, tlCorner * rect.Height));
                points.Add(tr - new Vector2(tlCorner * rect.Width, 0f));
            }
            return points;
        }
        
        public static void DrawCorners(this Rect rect, float lineThickness, ColorRgba colorRgba, float tlCorner, float trCorner, float brCorner, float blCorner, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
        {
            var tl = rect.TopLeft;
            var tr = rect.TopRight;
            var br = rect.BottomRight;
            var bl = rect.BottomLeft;

            if (tlCorner > 0f)
            {
                //DrawCircle(tl, lineThickness / 2, color);
                DrawLine(tl, tl + new Vector2(MathF.Min(tlCorner, rect.Width), 0f), lineThickness, colorRgba, capType, capPoints);
                DrawLine(tl, tl + new Vector2(0f, MathF.Min(tlCorner, rect.Height)), lineThickness, colorRgba, capType, capPoints);
            }
            if (trCorner > 0f)
            {
                //DrawCircle(tr, lineThickness / 2, color);
                DrawLine(tr, tr - new Vector2(MathF.Min(trCorner, rect.Width), 0f), lineThickness, colorRgba, capType, capPoints);
                DrawLine(tr, tr + new Vector2(0f, MathF.Min(trCorner, rect.Height)), lineThickness, colorRgba, capType, capPoints);
            }
            if (brCorner > 0f)
            {
                //DrawCircle(br, lineThickness / 2, color);
                DrawLine(br, br - new Vector2(MathF.Min(brCorner, rect.Width), 0f), lineThickness, colorRgba, capType, capPoints);
                DrawLine(br, br - new Vector2(0f, MathF.Min(brCorner, rect.Height)), lineThickness, colorRgba, capType, capPoints);
            }
            if (blCorner > 0f)
            {
                //DrawCircle(bl, lineThickness / 2, color);
                DrawLine(bl, bl + new Vector2(MathF.Min(blCorner, rect.Width), 0f), lineThickness, colorRgba, capType, capPoints);
                DrawLine(bl, bl - new Vector2(0f, MathF.Min(blCorner, rect.Height)), lineThickness, colorRgba, capType, capPoints);
            }
        }
        public static void DrawCorners(this Rect rect, float lineThickness, ColorRgba colorRgba, float cornerLength, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
            => DrawCorners(rect, lineThickness, colorRgba, cornerLength, cornerLength, cornerLength, cornerLength, capType, capPoints);
        public static void DrawCornersRelative(this Rect rect, float lineThickness, ColorRgba colorRgba, float tlCorner, float trCorner, float brCorner, float blCorner, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
        {
            var tl = rect.TopLeft;
            var tr = rect.TopRight;
            var br = rect.BottomRight;
            var bl = rect.BottomLeft;

            if (tlCorner > 0f && tlCorner < 1f)
            {
                DrawCircle(tl, lineThickness / 2, colorRgba);
                DrawLine(tl, tl + new Vector2(tlCorner * rect.Width, 0f), lineThickness, colorRgba, capType, capPoints);
                DrawLine(tl, tl + new Vector2(0f, tlCorner * rect.Height), lineThickness, colorRgba, capType, capPoints);
            }
            if (trCorner > 0f && trCorner < 1f)
            {
                DrawCircle(tr, lineThickness / 2, colorRgba);
                DrawLine(tr, tr - new Vector2(tlCorner * rect.Width, 0f), lineThickness, colorRgba, capType, capPoints);
                DrawLine(tr, tr + new Vector2(0f, tlCorner * rect.Height), lineThickness, colorRgba, capType, capPoints);
            }
            if (brCorner > 0f && brCorner < 1f)
            {
                DrawCircle(br, lineThickness / 2, colorRgba);
                DrawLine(br, br - new Vector2(tlCorner * rect.Width, 0f), lineThickness, colorRgba, capType, capPoints);
                DrawLine(br, br - new Vector2(0f, tlCorner * rect.Height), lineThickness, colorRgba, capType, capPoints);
            }
            if (blCorner > 0f && blCorner < 1f)
            {
                DrawCircle(bl, lineThickness / 2, colorRgba);
                DrawLine(bl, bl + new Vector2(tlCorner * rect.Width, 0f), lineThickness, colorRgba, capType, capPoints);
                DrawLine(bl, bl - new Vector2(0f, tlCorner * rect.Height), lineThickness, colorRgba, capType, capPoints);
            }
        }
        public static void DrawCornersRelative(this Rect rect, float lineThickness, ColorRgba colorRgba, float cornerLengthFactor, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2) 
            => DrawCornersRelative(rect, lineThickness, colorRgba, cornerLengthFactor, cornerLengthFactor, cornerLengthFactor, cornerLengthFactor, capType, capPoints);
        
        public static void DrawCheckered(this Rect rect, float spacing, float lineThickness, float angleDeg, ColorRgba lineColorRgba, ColorRgba outlineColorRgba, ColorRgba bgColorRgba)
        {
            var size = new Vector2(rect.Width, rect.Height);
            var center = new Vector2(rect.X, rect.Y) + size / 2;
            float maxDimension = MathF.Max(size.X, size.Y);
            float rotRad = angleDeg * ShapeMath.DEGTORAD;

            //var tl = new Vector2(rect.X, rect.Y);
            //var tr = new Vector2(rect.X + rect.Width, rect.Y);
            //var bl = new Vector2(rect.X, rect.Y + rect.Height);
            //var br = new Vector2(rect.X + rect.Width, rect.Y + rect.Height);

            if (bgColorRgba.A > 0) DrawRectangleRec(rect.Rectangle, bgColorRgba.ToRayColor());

            Vector2 cur = new(-spacing / 2, 0f);

            //safety for while loops
            int whileMaxCount = (int)(maxDimension / spacing) * 2;
            int whileCounter = 0;

            //left half of rectangle
            while (whileCounter < whileMaxCount)
            {
                var p = center + ShapeVec.Rotate(cur, rotRad);
                var up = new Vector2(0f, -maxDimension * 2);//make sure that lines are going outside of the rectangle
                var down = new Vector2(0f, maxDimension * 2);
                var start = p + ShapeVec.Rotate(up, rotRad);
                var end = p + ShapeVec.Rotate(down, rotRad);
                var seg = new Segment(start, end);
                var collisionPoints = seg.IntersectShape(rect); // SGeometry.IntersectionSegmentRect(center, start, end, tl, tr, br, bl).points;

                if (collisionPoints.Count >= 2) 
                    DrawLine(collisionPoints[0].Point, collisionPoints[1].Point, lineThickness, lineColorRgba);
                else break;
                cur.X -= spacing;
                whileCounter++;
            }

            cur = new(spacing / 2, 0f);
            whileCounter = 0;
            //right half of rectangle
            while (whileCounter < whileMaxCount)
            {
                var p = center + ShapeVec.Rotate(cur, rotRad);
                var up = new Vector2(0f, -maxDimension * 2);
                var down = new Vector2(0f, maxDimension * 2);
                var start = p + ShapeVec.Rotate(up, rotRad);
                var end = p + ShapeVec.Rotate(down, rotRad);
                var seg = new Segment(start, end);
                var collisionPoints = seg.IntersectShape(rect); //SGeometry.IntersectionSegmentRect(center, start, end, tl, tr, br, bl).points;
                if (collisionPoints.Count >= 2) 
                    DrawLine(collisionPoints[0].Point, collisionPoints[1].Point, lineThickness, lineColorRgba);
                else break;
                cur.X += spacing;
                whileCounter++;
            }

            if (outlineColorRgba.A > 0) DrawLines(rect, new Vector2(0.5f, 0.5f), 0f, lineThickness, outlineColorRgba);
        }

        public static void DrawLinesDotted(this Rect rect, int gapsPerSide, float lineThickness, ColorRgba colorRgba, LineCapType capType = LineCapType.CappedExtended, int capPoints = 3)
        {
            // if (cornerCircleSectors > 5)
            // {
            //     var corners = ShapeRect.GetCorners(rect);
            //     float r = lineThickness * 0.5f;
            //     DrawCircle(corners.tl, r, color, cornerCircleSectors);
            //     DrawCircle(corners.tr, r, color, cornerCircleSectors);
            //     DrawCircle(corners.br, r, color, cornerCircleSectors);
            //     DrawCircle(corners.bl, r, color, cornerCircleSectors);
            // }


            var segments = rect.GetEdges();
            foreach (var s in segments)
            {
                DrawLineDotted(s.Start, s.End, gapsPerSide, lineThickness, colorRgba, capType, capPoints);
            }
        }
        public static void DrawLinesDotted(this Rect rect, int gapsPerSide, float gapSizeF, float lineThickness, ColorRgba colorRgba, LineCapType capType = LineCapType.CappedExtended, int capPoints = 3)
        {
            // if (cornerCircleSegments > 5)
            // {
            //     var corners = ShapeRect.GetCorners(rect);
            //     float r = lineThickness * 0.5f;
            //     DrawCircle(corners.tl, r, color, cornerCircleSegments);
            //     DrawCircle(corners.tr, r, color, cornerCircleSegments);
            //     DrawCircle(corners.br, r, color, cornerCircleSegments);
            //     DrawCircle(corners.bl, r, color, cornerCircleSegments);
            // }


            var segments = rect.GetEdges(); // SRect.GetEdges(rect);
            foreach (var s in segments)
            {
                DrawLineDotted(s.Start, s.End, gapsPerSide, gapSizeF, lineThickness, colorRgba, capType, capPoints);
            }
        }


        #endregion

        #region Triangle
        public static void DrawTriangle(Vector2 a, Vector2 b, Vector2 c, ColorRgba colorRgba) => Raylib.DrawTriangle(a, b, c, colorRgba.ToRayColor());

        public static void DrawTriangleLines(Vector2 a, Vector2 b, Vector2 c, float lineThickness,
            ColorRgba colorRgba, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
        {
            DrawLine(a, b, lineThickness, colorRgba, capType, capPoints);
            DrawLine(b, c, lineThickness, colorRgba, capType, capPoints);
            DrawLine(c, a, lineThickness, colorRgba, capType, capPoints);
            
            // new Triangle(a, b, c).GetEdges().Draw(lineThickness, color);
        }

        public static void Draw(this Triangle t, ColorRgba colorRgba) => Raylib.DrawTriangle(t.A, t.B, t.C, colorRgba.ToRayColor());

        public static void DrawLines(this Triangle t, float lineThickness, ColorRgba colorRgba, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
        {
            DrawTriangleLines(t.A, t.B, t.C, lineThickness, colorRgba);
            // t.GetEdges().Draw(lineThickness, color);
        }

        public static void DrawVertices(this Triangle t, float vertexRadius, ColorRgba colorRgba, int circleSegments = 8)
        {
            DrawCircle(t.A, vertexRadius, colorRgba, circleSegments);
            DrawCircle(t.B, vertexRadius, colorRgba, circleSegments);
            DrawCircle(t.C, vertexRadius, colorRgba, circleSegments);
        }
        public static void Draw(this Triangulation triangles, ColorRgba colorRgba) { foreach (var t in triangles) t.Draw(colorRgba); }

        public static void DrawLines(this Triangulation triangles, float lineThickness, ColorRgba colorRgba,
            LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
        {
            foreach (var t in triangles) t.DrawLines(lineThickness, colorRgba, capType, capPoints);
        }
        #endregion

        #region Polygon

        public static void DrawPolygonConvex(this Polygon poly, ColorRgba colorRgba, bool clockwise = false) { DrawPolygonConvex(poly, poly.GetCentroid(), colorRgba, clockwise); }
        public static void DrawPolygonConvex(this Polygon poly, Vector2 center, ColorRgba colorRgba, bool clockwise = false)
        {
            if (clockwise)
            {
                for (int i = 0; i < poly.Count - 1; i++)
                {
                    Raylib.DrawTriangle(poly[i], center, poly[i + 1], colorRgba.ToRayColor());
                }
                Raylib.DrawTriangle(poly[poly.Count - 1], center, poly[0], colorRgba.ToRayColor());
            }
            else
            {
                for (int i = 0; i < poly.Count - 1; i++)
                {
                    Raylib.DrawTriangle(poly[i], poly[i + 1], center, colorRgba.ToRayColor());
                }
                Raylib.DrawTriangle(poly[poly.Count - 1], poly[0], center, colorRgba.ToRayColor());
            }
        }
        public static void DrawPolygonConvex(this Polygon relativePoly, Vector2 pos, float scale, float rotDeg, ColorRgba colorRgba, bool clockwise = false)
        {
            if (clockwise)
            {
                for (int i = 0; i < relativePoly.Count - 1; i++)
                {
                    var a = pos + ShapeVec.Rotate(relativePoly[i] * scale, rotDeg * ShapeMath.DEGTORAD);
                    var b = pos;
                    var c = pos + ShapeVec.Rotate(relativePoly[i + 1] * scale, rotDeg * ShapeMath.DEGTORAD);
                    Raylib.DrawTriangle(a, b, c, colorRgba.ToRayColor());
                }

                var aFinal = pos + ShapeVec.Rotate(relativePoly[relativePoly.Count - 1] * scale, rotDeg * ShapeMath.DEGTORAD);
                var bFinal = pos;
                var cFinal = pos + ShapeVec.Rotate(relativePoly[0] * scale, rotDeg * ShapeMath.DEGTORAD);
                Raylib.DrawTriangle(aFinal, bFinal, cFinal, colorRgba.ToRayColor());
            }
            else
            {
                for (int i = 0; i < relativePoly.Count - 1; i++)
                {
                    var a = pos + ShapeVec.Rotate(relativePoly[i] * scale, rotDeg * ShapeMath.DEGTORAD);
                    var b = pos + ShapeVec.Rotate(relativePoly[i + 1] * scale, rotDeg * ShapeMath.DEGTORAD);
                    var c = pos;
                    Raylib.DrawTriangle(a, b, c, colorRgba.ToRayColor());
                }

                var aFinal = pos + ShapeVec.Rotate(relativePoly[relativePoly.Count - 1] * scale, rotDeg * ShapeMath.DEGTORAD);
                var bFinal = pos + ShapeVec.Rotate(relativePoly[0] * scale, rotDeg * ShapeMath.DEGTORAD);
                var cFinal = pos;
                Raylib.DrawTriangle(aFinal, bFinal, cFinal, colorRgba.ToRayColor());
            }
        }
        
        public static void Draw(this Polygon poly, ColorRgba colorRgba) { poly.Triangulate().Draw(colorRgba); }

        public static void DEBUG_DrawLinesCCW(this Polygon poly, float lineThickness, ColorRgba startColorRgba, ColorRgba endColorRgba)
        {
            if (poly.Count < 3) return;

            DrawLines(poly, lineThickness, startColorRgba, endColorRgba, LineCapType.CappedExtended, 2);
            ShapeDrawing.DrawCircle(poly[0], lineThickness * 2f, startColorRgba, 16);
            ShapeDrawing.DrawCircle(poly[poly.Count - 1], lineThickness * 2f, endColorRgba, 16);
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
                DrawLine(start, end, lineThickness, finalColorRgba, capType, capPoints);
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
        // public static void DrawLines(this Polygon poly, float lineThickness, ShapeColor color)
        // {
        //     poly.DrawLines(lineThickness, color, 2);
        // }
        public static void DrawLines(this Polygon poly, float lineThickness, ColorRgba colorRgba, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
        {
            if (poly.Count < 3) return;
            // for (int i = 0; i < poly.Count - 1; i++)
            // {
                // DrawLine(poly[i], poly[i + 1], lineThickness, color, capType, capPoints);
            // }
            // DrawLine(poly[poly.Count - 1], poly[0], lineThickness, color, capType, capPoints);
            
            for (var i = 0; i < poly.Count; i++)
            {
                var start = poly[i];
                var end = poly[(i + 1) % poly.Count];
                DrawLine(start, end, lineThickness, colorRgba, capType, capPoints);
            }
        }
        public static void DrawLines(this Polygon poly, Vector2 pos, Vector2 size, float rotDeg, float lineThickness, ColorRgba colorRgba, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
        {
            if (poly.Count < 3) return;
            
            for (var i = 0; i < poly.Count; i++)
            {
                var start = pos + ShapeVec.Rotate(poly[i] * size, rotDeg * ShapeMath.DEGTORAD);
                var end = pos + ShapeVec.Rotate(poly[(i + 1) % poly.Count] * size, rotDeg * ShapeMath.DEGTORAD);
                DrawLine(start, end, lineThickness, colorRgba, capType, capPoints);
            }
            
            // for (int i = 0; i < poly.Count - 1; i++)
            // {
                // Vector2 p1 = pos + ShapeVec.Rotate(poly[i] * size, rotDeg * ShapeMath.DEGTORAD);
                // Vector2 p2 = pos + ShapeVec.Rotate(poly[i + 1] * size, rotDeg * ShapeMath.DEGTORAD);
                // DrawLine(p1, p2, lineThickness, color, capType, capPoints);
            // }
            // DrawLineEx(pos + ShapeVec.Rotate(poly[poly.Count - 1] * size, rotDeg * ShapeMath.DEGTORAD), pos + ShapeVec.Rotate(poly[0] * size, rotDeg * ShapeMath.DEGTORAD), lineThickness, outlineColor);
        }
        public static void DrawVertices(this Polygon poly, float vertexRadius, ColorRgba colorRgba, int circleSegments)
        {
            foreach (var p in poly)
            {
                DrawCircle(p, vertexRadius, colorRgba, circleSegments);
            }
        }
        
        public static void DrawCornered(this Polygon poly, float lineThickness, ColorRgba colorRgba, float cornerLength, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
        {
            for (var i = 0; i < poly.Count; i++)
            {
                var prev = poly[(i-1)%poly.Count];
                var cur = poly[i];
                var next = poly[(i+1)%poly.Count];
                DrawLine(cur, cur + next.Normalize() * cornerLength, lineThickness, colorRgba, capType, capPoints);
                DrawLine(cur, cur + prev.Normalize() * cornerLength, lineThickness, colorRgba, capType, capPoints);
            }
        }
        public static void DrawCornered(this Polygon poly, float lineThickness, ColorRgba colorRgba, List<float> cornerLengths, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
        {
            for (var i = 0; i < poly.Count; i++)
            {
                float cornerLength = cornerLengths[i%cornerLengths.Count];
                var prev = poly[(i - 1) % poly.Count];
                var cur = poly[i];
                var next = poly[(i + 1) % poly.Count];
                DrawLine(cur, cur + next.Normalize() * cornerLength, lineThickness, colorRgba, capType, capPoints);
                DrawLine(cur, cur + prev.Normalize() * cornerLength, lineThickness, colorRgba, capType, capPoints);
            }
        }
        public static void DrawCorneredRelative(this Polygon poly, float lineThickness, ColorRgba colorRgba, float cornerF, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
        {
            for (var i = 0; i < poly.Count; i++)
            {
                var prev = poly[(i - 1) % poly.Count];
                var cur = poly[i];
                var next = poly[(i + 1) % poly.Count];
                DrawLine(cur, cur.Lerp(next, cornerF), lineThickness, colorRgba, capType, capPoints);
                DrawLine(cur, cur.Lerp(prev, cornerF), lineThickness, colorRgba, capType, capPoints);
            }
        }
        public static void DrawCorneredRelative(this Polygon poly, float lineThickness, ColorRgba colorRgba, List<float> cornerFactors, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
        {
            for (var i = 0; i < poly.Count; i++)
            {
                float cornerF = cornerFactors[i%cornerFactors.Count];
                var prev = poly[(i - 1) % poly.Count];
                var cur = poly[i];
                var next = poly[(i + 1) % poly.Count];
                DrawLine(cur, cur.Lerp(next, cornerF), lineThickness, colorRgba, capType, capPoints);
                DrawLine(cur, cur.Lerp(prev, cornerF), lineThickness, colorRgba, capType, capPoints);
            }
        }
        #endregion

        #region Polyline

        public static void Draw(this Polyline polyline, float thickness, ColorRgba colorRgba, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
        {
            if (polyline.Count < 2) return;
            for (var i = 0; i < polyline.Count - 1; i++)
            {
                var start = polyline[i];
                var end = polyline[i + 1];
                DrawLine(start, end, thickness, colorRgba, capType, capPoints);
            }
            // polyline.GetEdges().Draw(thickness, color);
        }

        public static void Draw(this Polyline polyline, float thickness, List<ColorRgba> colors, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
        {
            if (polyline.Count < 2) return;
            for (var i = 0; i < polyline.Count - 1; i++)
            {
                var start = polyline[i];
                var end = polyline[i + 1];
                var c = colors[i % colors.Count];
                DrawLine(start, end, thickness, c, capType, capPoints);
            }
            // polyline.GetEdges().Draw(thickness, colors);
        }
        public static void DrawVertices(this Polyline polyline, float vertexRadius, ColorRgba colorRgba, int circleSegments)
        {
            foreach (var p in polyline)
            {
                DrawCircle(p, vertexRadius, colorRgba, circleSegments);
            }
        }

        public static void DrawDotted(this Polyline polyline, int gaps, float thickness, ColorRgba colorRgba, LineCapType capType = LineCapType.Capped, int capPoints = 3)
        {
            if (polyline.Count < 2) return;
            for (var i = 0; i < polyline.Count - 1; i++)
            {
                var start = polyline[i];
                var end = polyline[i + 1];
                DrawLineDotted(start, end, gaps, thickness, colorRgba, capType, capPoints);
            }
             // polyline.GetEdges().DrawDotted(gaps, thickness, color, endCapSegments);
        }

        public static void DrawDotted(this Polyline polyline, int gaps, float gapSizeF, 
            float thickness, ColorRgba colorRgba, LineCapType capType = LineCapType.Capped, int capPoints = 3)
        {
            if (polyline.Count < 2) return;
            for (var i = 0; i < polyline.Count - 1; i++)
            {
                var start = polyline[i];
                var end = polyline[i + 1];
                DrawLineDotted(start, end, gaps, gapSizeF, thickness, colorRgba, capType, capPoints);
            }
            // polyline.GetEdges().DrawDotted(gaps, gapSizeF, thickness, color, endCapSegments);
        }

        public static void DrawGlow(this Polyline polyline, float width, float endWidth, ColorRgba colorRgba,
            ColorRgba endColorRgba, int steps, LineCapType capType = LineCapType.CappedExtended, int capPoints = 2)
        {
            if (polyline.Count < 2) return;
            for (var i = 0; i < polyline.Count - 1; i++)
            {
                var start = polyline[i];
                var end = polyline[i + 1];
                DrawLineGlow(start, end, width, endWidth, colorRgba, endColorRgba, steps, capType, capPoints);
            }
            // polyline.GetEdges().DrawGlow(width, endWidth, color, endColor, steps);
        }

        #endregion
        
        #region UI
        public static void DrawOutlineBar(this Rect rect, float thickness, float f, ColorRgba colorRgba)
        {
            Vector2 thicknessOffsetX = new Vector2(thickness, 0f);
            Vector2 thicknessOffsetY = new Vector2(0f, thickness);

            Vector2 tl = new(rect.X, rect.Y);
            Vector2 br = tl + new Vector2(rect.Width, rect.Height); ;
            Vector2 tr = tl + new Vector2(rect.Width, 0);
            Vector2 bl = tl + new Vector2(0, rect.Height);

            int lines = (int)MathF.Ceiling(4 * Clamp(f, 0f, 1f));
            float fMin = 0.25f * (lines - 1);
            float fMax = fMin + 0.25f;
            float newF = ShapeMath.RemapFloat(f, fMin, fMax, 0f, 1f);
            for (int i = 0; i < lines; i++)
            {
                Vector2 end;
                Vector2 start;
                if (i == 0)
                {
                    start = tl - thicknessOffsetX / 2;
                    end = tr - thicknessOffsetX / 2;
                }
                else if (i == 1)
                {
                    start = tr - thicknessOffsetY / 2;
                    end = br - thicknessOffsetY / 2;
                }
                else if (i == 2)
                {
                    start = br + thicknessOffsetX / 2;
                    end = bl + thicknessOffsetX / 2;
                }
                else
                {
                    start = bl + thicknessOffsetY / 2;
                    end = tl + thicknessOffsetY / 2;
                }

                //last line
                if (i == lines - 1) end = ShapeVec.Lerp(start, end, newF);
                DrawLineEx(start, end, thickness, colorRgba.ToRayColor());
            }
        }
        public static void DrawOutlineBar(this Rect rect, Vector2 pivot, float angleDeg, float thickness, float f, ColorRgba colorRgba)
        {
            var rr = ShapeRect.Rotate(rect, pivot, angleDeg);
            //Vector2 thicknessOffsetX = new Vector2(thickness, 0f);
            //Vector2 thicknessOffsetY = new Vector2(0f, thickness);

            Vector2 leftExtension = ShapeVec.Rotate(new Vector2(-thickness / 2, 0f), angleDeg * ShapeMath.DEGTORAD);
            Vector2 rightExtension = ShapeVec.Rotate(new Vector2(thickness / 2, 0f), angleDeg * ShapeMath.DEGTORAD);

            Vector2 tl = rr.tl;
            Vector2 br = rr.br;
            Vector2 tr = rr.tr;
            Vector2 bl = rr.bl;

            int lines = (int)MathF.Ceiling(4 * Clamp(f, 0f, 1f));
            float fMin = 0.25f * (lines - 1);
            float fMax = fMin + 0.25f;
            float newF = ShapeMath.RemapFloat(f, fMin, fMax, 0f, 1f);
            for (int i = 0; i < lines; i++)
            {
                Vector2 end;
                Vector2 start;
                if (i == 0)
                {
                    start = tl + leftExtension;
                    end = tr + rightExtension;
                }
                else if (i == 1)
                {
                    start = tr;
                    end = br;
                }
                else if (i == 2)
                {
                    start = br + rightExtension;
                    end = bl + leftExtension;
                }
                else
                {
                    start = bl;
                    end = tl;
                }

                //last line
                if (i == lines - 1) end = ShapeVec.Lerp(start, end, newF);
                DrawLineEx(start, end, thickness, colorRgba.ToRayColor());
            }
        }

        public static void DrawOutlineBar(this Circle c, float thickness, float f, ColorRgba colorRgba)
        {
            DrawCircleSectorLines(c.Center, c.Radius, 0, 360 * f, thickness, colorRgba, false, 8f);
        }
        public static void DrawOutlineBar(this Circle c, float startOffsetDeg, float thickness, float f, ColorRgba colorRgba)
        {
            DrawCircleSectorLines(c.Center, c.Radius, 0, 360 * f, startOffsetDeg, thickness, colorRgba, false, 8f);
        }

        public static void DrawBar(this Rect rect, float f, ColorRgba barColorRgba, ColorRgba bgColorRgba, float left = 0f, float right = 1f, float top = 0f, float bottom = 0f)
        {
            f = 1.0f - f;
            UIMargins progressMargins = new(f * top, f * right, f * bottom, f * left);
            var progressRect = progressMargins.Apply(rect);
            rect.Draw(bgColorRgba);
            progressRect.Draw(barColorRgba);
        }
        public static void DrawBar(this Rect rect, Vector2 pivot, float angleDeg, float f, ColorRgba barColorRgba, ColorRgba bgColorRgba, float left = 0f, float right = 1f, float top = 0f, float bottom = 0f)
        {
            f = 1.0f - f;
            UIMargins progressMargins = new(f * top, f * right, f * bottom, f * left);
            var progressRect = progressMargins.Apply(rect);
            rect.Draw(pivot, angleDeg, bgColorRgba);
            progressRect.Draw(pivot, angleDeg, barColorRgba);
        }
        
        #endregion

    }


    