
using System.Numerics;
using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Shapes;
using ShapeEngine.UI;

namespace ShapeEngine.Lib
{
    /*//TODO could be used for text drawing functions
    public struct Label
    {
        public Rect Area;
        public string Text;
        public Raylib_CsLo.Color Color;
        public float FontSpacing;

        public Label(Rect area, string text, Raylib_CsLo.Color color, float fontSpacing = 1)
        {
            Text = text;
            Area = area;
            Color = color;
            FontSpacing = fontSpacing;
        }
    }
    */
    
    
    
    public enum TextEmphasisType
    {
        None = 0,
        Line = 1,
        Corner = 2,
        //Corner_Dot = 3
    }
    public enum TextEmphasisAlignement
    {
        TopLeft = 0,
        Top = 1,
        TopRight = 2,
        Right = 3,
        BottomRight = 4,
        Bottom = 5,
        BottomLeft = 6,
        Left = 7,
        Center = 8,
        Boxed = 9,
        TLBR = 10,
        BLTR = 11
    }
    //public enum EmphasisType
    //{
    //    None = 0,
    //    Underlined = 1,
    //    Strikethrough = 2,
    //    Boxed = 3,
    //    Cornered = 4,
    //    Cornered_Left = 5,
    //    Cornered_Right = 6,
    //    Cornered_Top = 7,
    //    Cornered_Bottom = 8,
    //    Cornered_TLBR = 9,
    //    Cornered_BLTR = 10
    //}
    public struct WordEmphasis
    {
        public Raylib_CsLo.Color Color;
        public List<int> WordIndices;
        public TextEmphasisType EmphasisType;
        public TextEmphasisAlignement EmphasisAlignement;
        /// <summary>
        /// Adjusts the thickness of all emphasis effects. The final thickness equals the fontSize * EmphasisThicknessFactor.
        /// </summary>
        public float LineThicknessFactor = 0.025f;
        /// <summary>
        /// Increases/ decreases the size of all emphasis effects.
        /// This is an absolute value, positive numbers increase, negative numbers decrease.
        /// </summary>
        public Vector2 SizeMargin = new Vector2(0f, 0f);
        public WordEmphasis(Raylib_CsLo.Color color, params int[] wordIndices)
        {
            this.Color = color;
            this.WordIndices = wordIndices.ToList();
            this.EmphasisType = TextEmphasisType.None;
            this.EmphasisAlignement = TextEmphasisAlignement.Boxed;//irelevant because type == none
        }
        public WordEmphasis(Raylib_CsLo.Color color, TextEmphasisType emphasisType, TextEmphasisAlignement alignement, params int[] wordIndices)
        {
            this.Color = color;
            this.WordIndices = wordIndices.ToList();
            this.EmphasisType = emphasisType;
            this.EmphasisAlignement = alignement;
        }
        public bool Contains(int index) { return WordIndices.Contains(index); }
        public (bool contains, bool connected) CheckIndex(int index)
        {
            bool contains = false;
            for (int i = 0; i < WordIndices.Count; i++)
            {
                int curIndex = WordIndices[i];
                if (!contains)
                {
                    if (curIndex == index) contains = true;
                }
                else
                {
                    if (curIndex == index + 1) return (contains, true);
                }
            }
            return (contains, false);
        }
    }

    //public struct TextCaret
    //{
    //    public bool Draw = true;
    //    public int Index;
    //    public float Width;
    //    public Raylib_CsLo.Color Color = WHITE;
    //
    //    public TextCaret()
    //    {
    //        Draw = false;
    //        Index = 0;
    //        Width = 0f;
    //        Color = WHITE;
    //    }
    //    public TextCaret(int indexPosition, float width, Raylib_CsLo.Color color)
    //    {
    //        Draw = true;
    //        Index = indexPosition;
    //        Width = width;
    //        Color = color;
    //    }
    //}

    public static class ShapeDrawing
    {
        /// <summary>
        /// The minimum font size SDrawing uses. Font sizes are clamped to this min size if they are lower.
        /// </summary>
        public static float FontMinSize = 5f;
        /// <summary>
        /// Factor based on the font size to determine the max line spacing. (1 means line spacing can not exceed the font size)
        /// </summary>
        public static float LineSpacingMaxFactor = 1f;
        /// <summary>
        /// Factor based on the font size to determine the max font spacing. (1 means font spacing can not exceed the font size)
        /// </summary>
        public static float FontSpacingMaxFactor = 1f;
        /// <summary>
        /// Text Wrapping Functions that automatically calculate the font size based on the text and a rect use
        /// this value to make sure the text fits into the rect. Lower values are more conservative, meaning the make sure
        /// no text overflows but the rect might not be completely filled. Value range should stay between 0 - 1!
        /// (Does not affect text drawing functions without word wrap functionality)
        /// </summary>
        public static float TextWrappingAutoFontSizeSafetyMargin = 0.7f;


        public static void Draw(this Intersection intersection, float lineThickness, Raylib_CsLo.Color intersectColor, Raylib_CsLo.Color normalColor)
        {
            foreach (var i in intersection.ColPoints)
            {
                DrawCircle(i.Point, lineThickness * 2f, intersectColor);
                Segment normal = new(i.Point, i.Point + i.Normal * lineThickness * 10f);
                normal.Draw(lineThickness, normalColor);
            }
        }
        public static void Draw(this Points points, float r, Raylib_CsLo.Color color)
        {
            foreach (var p in points)
            {
                DrawCircle(p, r, color);
            }
        }
        #region Pixel
        public static void DrawPixel(Vector2 pos, Raylib_CsLo.Color color) => Raylib.DrawPixelV(pos, color); 
        public static void DrawPixel(float x, float y, Raylib_CsLo.Color color) => Raylib.DrawPixelV(new(x, y), color);
        #endregion

        #region Point
        public static void Draw(this Vector2 p, float radius, Raylib_CsLo.Color color)
        {
            DrawCircle(p, radius, color, 16);
        }
        #endregion

        #region Segment
        public static void DrawSegment(float x1, float y1, float x2, float y2, float thickness, Raylib_CsLo.Color color) => Raylib.DrawLineEx(new(x1, y1), new(x2, y2), thickness, color);
        public static void DrawSegment(Vector2 start, Vector2 end, float thickness, Raylib_CsLo.Color color) => Raylib.DrawLineEx(start, end, thickness, color);
        public static void Draw(this Segment segment, float thickness, Raylib_CsLo.Color color) => Raylib.DrawLineEx(segment.Start, segment.End, thickness, color);
        public static void Draw(this Segment segment, float thickness, Raylib_CsLo.Color color, int endCapSegments)
        {
            segment.Draw(thickness, color);
            segment.DrawVertices(thickness * 0.5f, color, endCapSegments);
        }
        public static void Draw(this Segments segments, float thickness, Raylib_CsLo.Color color) 
        {
            segments.Draw(thickness, color, 8);
        }
        public static void Draw(this Segments segments, float thickness, Raylib_CsLo.Color color, int cornerSegments)
        {
            if (segments.Count <= 0) return;
            foreach (var seg in segments)
            {
                if (cornerSegments > 5) DrawCircle(seg.Start, thickness * 0.5f, color, cornerSegments);
                seg.Draw(thickness, color);
            }
            if (cornerSegments > 5) DrawCircle(segments[segments.Count - 1].End, thickness * 0.5f, color, cornerSegments);
        }
        public static void Draw(this Segments segments, float thickness, List<Raylib_CsLo.Color> colors, int cornerSegments = 8)
        {
            if (segments.Count <= 0 || colors.Count <= 0) return;
            for (int i = 0; i < segments.Count; i++)
            {
                Raylib_CsLo.Color c = colors[i % colors.Count];
                if(cornerSegments > 5) DrawCircle(segments[i].Start, thickness * 0.5f, c, cornerSegments);
                segments[i].Draw(thickness, c);
            }
            if(cornerSegments > 5) DrawCircle(segments[segments.Count - 1].End, thickness * 0.5f, colors[(segments.Count - 1) % colors.Count], cornerSegments);
        }
        public static void DrawVertices(this Segment segment, float vertexRadius, Raylib_CsLo.Color color)
        {
            segment.DrawVertices(vertexRadius, color, 8);
        }
        public static void DrawVertices(this Segment segment, float vertexRadius, Raylib_CsLo.Color color, int endCapSegments)
        {
            if (endCapSegments > 5)
            {
                DrawCircle(segment.Start, vertexRadius, color, endCapSegments);
                DrawCircle(segment.End, vertexRadius, color, endCapSegments);
            }
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
                float randSegmentLength = ShapeRandom.randF() * segmentLength;
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
                float displacement = ShapeRandom.randF(-maxSway, maxSway);
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
                float randSegmentLength = ShapeRandom.randF() * segmentLength;
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
                float displacement = ShapeRandom.randF(-maxSway, maxSway);
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
        
        public static void DrawSegmentDotted(Vector2 start, Vector2 end, int gaps, float thickness, Raylib_CsLo.Color color, int endCapSegments = 8)
        {
            if (gaps <= 0) DrawLineEx(start, end, thickness, color);
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
                        if (endCapSegments > 5)
                        {
                            DrawCircle(cur, thickness * 0.5f, color , endCapSegments);
                            DrawCircle(next, thickness * 0.5f, color, endCapSegments);
                        }
                        DrawLineEx(cur, next, thickness, color);
                        cur = next;

                    }
                    else
                    {
                        cur = cur + offset; //gap
                    }
                }
            }
        }
        public static void DrawSegmentDotted(Vector2 start, Vector2 end, int gaps, float gapSizeF, float thickness, Raylib_CsLo.Color color, int endCapSegments = 8)
        {
            if (gaps <= 0) DrawLineEx(start, end, thickness, color);
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
                        if (endCapSegments > 5)
                        {
                            DrawCircle(cur, thickness * 0.5f, color , endCapSegments);
                            DrawCircle(next, thickness * 0.5f, color, endCapSegments);
                        }
                        DrawLineEx(cur, next, thickness, color);
                        cur = next;
                    }
                    else
                    {
                        cur = cur + gapOffset; //gap
                    }
                }
            }
        }
        public static void DrawLineGlow(Vector2 start, Vector2 end, float width, float endWidth, Raylib_CsLo.Color color, Raylib_CsLo.Color endColor, int steps)
        {
            float wStep = (endWidth - width) / steps;

            float rStep = (endColor.r - color.r) / steps;
            float gStep = (endColor.g - color.g) / steps;
            float bStep = (endColor.b - color.b) / steps;
            float aStep = (endColor.a - color.a) / steps;

            for (int i = steps; i >= 0; i--)
            {
                DrawLineEx
                (
                    start, end, width + wStep * i,
                    new
                    (
                        (int)(color.r + rStep * i),
                        (int)(color.g + gStep * i),
                        (int)(color.b + bStep * i),
                        (int)(color.a + aStep * i)
                    )
                );
            }
        }
        public static void DrawDotted(this Segment segment, int gaps, float thickness, Raylib_CsLo.Color color, int endCapSegments = 8)
        {
            DrawSegmentDotted(segment.Start, segment.End, gaps, thickness, color, endCapSegments);
        }
        public static void DrawDotted(this Segment segment, int gaps, float gapSizeF, float thickness, Raylib_CsLo.Color color, int endCapSegments = 8)
        {
            DrawSegmentDotted(segment.Start, segment.End, gaps, gapSizeF, thickness, color, endCapSegments);
        }
        public static void DrawDotted(this Segments segments, int gaps, float thickness, Raylib_CsLo.Color color, int endCapSegments = 8)
        {
            foreach (var seg in segments)
            {
                seg.DrawDotted(gaps, thickness, color, endCapSegments);
            }
        }
        public static void DrawDotted(this Segments segments, int gaps, float gapSizeF, float thickness, Raylib_CsLo.Color color, int endCapSegments = 8)
        {
            foreach (var seg in segments)
            {
                seg.DrawDotted(gaps, gapSizeF, thickness, color, endCapSegments);
            }
        }

        public static void DrawGlow(this Segment segment, float width, float endWidth, Raylib_CsLo.Color color, Raylib_CsLo.Color endColor, int steps)
        {
            DrawLineGlow(segment.Start, segment.End, width, endWidth, color, endColor, steps);
        }
        public static void DrawGlow(this Segments segments, float width, float endWidth, Raylib_CsLo.Color color, Raylib_CsLo.Color endColor, int steps)
        {
            foreach (var seg in segments)
            {
                seg.DrawGlow(width, endWidth, color, endColor, steps);
            }
        }

        
        #endregion

        #region Circle
        public static void Draw(this Circle c, Raylib_CsLo.Color color)
        {
            DrawCircle(c.Center, c.Radius, color);
        } 
        public static void Draw(this Circle c, Raylib_CsLo.Color color, int segments)
        {
            DrawCircle(c.Center, c.Radius, color, segments);
        }
        public static void DrawLines(this Circle c, float lineThickness, int sides, Raylib_CsLo.Color color) => DrawPolyLinesEx(c.Center, sides, c.Radius, 0f, lineThickness, color);
        public static void DrawLines(this Circle c, float lineThickness, float rotDeg, int sides, Raylib_CsLo.Color color) => DrawPolyLinesEx(c.Center, sides, c.Radius, rotDeg, lineThickness, color);
        public static void DrawLines(this Circle c, float lineThickness, Raylib_CsLo.Color color, float sideLength = 8f)
        {
            int sides = GetCircleSideCount(c.Radius, sideLength);
            DrawPolyLinesEx(c.Center, sides, c.Radius, 0f, lineThickness, color);
        }
        
        public static void DrawCircle(Vector2 center, float radius, Raylib_CsLo.Color color)
        {
            Raylib.DrawCircleSector(center, radius, 0, 360, 18, color);
        }
        public static void DrawCircle(Vector2 center, float radius, Raylib_CsLo.Color color, int segments)
        {
            if (segments < 6) segments = 6;
            Raylib.DrawCircleSector(center, radius, 0, 360, segments, color);
        }
        /// <summary>
        /// Very usefull for drawing small/tiny circles. Drawing the circle as rect increases performance a lot.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        public static void DrawCircleFast(Vector2 center, float radius, Raylib_CsLo.Color color)
        {
            Rect r = new(center, new Vector2(radius * 2f), new Vector2(0.5f));
            r.Draw(color);
        }
        
        public static void DrawCircleLines(Vector2 center, float radius, float lineThickness, int sides, Raylib_CsLo.Color color) => DrawPolyLinesEx(center, sides, radius, 0f, lineThickness, color);
        public static void DrawCircleLines(Vector2 center, float radius, float lineThickness, float rotDeg, int sides, Raylib_CsLo.Color color) => DrawPolyLinesEx(center, sides, radius, rotDeg, lineThickness, color);
        public static void DrawCircleLines(Vector2 center, float radius, float lineThickness, Raylib_CsLo.Color color, float sideLength = 8f)
        {
            int sides = GetCircleSideCount(radius, sideLength);
            DrawPolyLinesEx(center, sides, radius, 0f, lineThickness, color);
        }

        public static void DrawSector(this Circle c, float startAngleDeg, float endAngleDeg, int segments, Raylib_CsLo.Color color)
        {
            Raylib.DrawCircleSector(c.Center, c.Radius, TransformAngleDeg(startAngleDeg), TransformAngleDeg(endAngleDeg), segments, color);
        }
        public static void DrawCircleSector(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, int segments, Raylib_CsLo.Color color)
        {
            Raylib.DrawCircleSector(center, radius, TransformAngleDeg(startAngleDeg), TransformAngleDeg(endAngleDeg), segments, color);
        }
        
        public static void DrawSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, float lineThickness, Raylib_CsLo.Color color, bool closed = true, float sideLength = 8f)
        {
            DrawCircleSectorLines(c.Center, c.Radius, startAngleDeg, endAngleDeg, lineThickness, color, closed, sideLength);
        }
        public static void DrawSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, float lineThickness, Raylib_CsLo.Color color, bool closed = true, float sideLength = 8f)
        {
            DrawCircleSectorLines(c.Center, c.Radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, lineThickness, color, closed, sideLength); ;
        }
        public static void DrawSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, int sides, float lineThickness, Raylib_CsLo.Color color, bool closed = true)
        {
            DrawCircleSectorLines(c.Center, c.Radius, startAngleDeg, endAngleDeg, sides, lineThickness, color, closed);
        }
        public static void DrawSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, int sides, float lineThickness, Raylib_CsLo.Color color, bool closed = true)
        {
            DrawCircleSectorLines(c.Center, c.Radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, sides, lineThickness, color, closed);
        }

        public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, float lineThickness, Raylib_CsLo.Color color, bool closed = true, float sideLength = 8f)
        {
            float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
            float endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
            float anglePiece = endAngleRad - startAngleRad;
            int sides = GetCircleArcSideCount(radius, MathF.Abs(anglePiece * RAD2DEG), sideLength);
            float angleStep = anglePiece / sides;
            if (closed)
            {
                Vector2 sectorStart = center + ShapeVec.Rotate(ShapeVec.Right() * radius + new Vector2(lineThickness / 2, 0), startAngleRad);
                DrawLineEx(center, sectorStart, lineThickness, color);

                Vector2 sectorEnd = center + ShapeVec.Rotate(ShapeVec.Right() * radius + new Vector2(lineThickness / 2, 0), endAngleRad);
                DrawLineEx(center, sectorEnd, lineThickness, color);
            }
            for (int i = 0; i < sides; i++)
            {
                Vector2 start = center + ShapeVec.Rotate(ShapeVec.Right() * radius, startAngleRad + angleStep * i);
                Vector2 end = center + ShapeVec.Rotate(ShapeVec.Right() * radius, startAngleRad + angleStep * (i + 1));
                DrawLineEx(start, end, lineThickness, color);
            }
        }
        public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, float lineThickness, Raylib_CsLo.Color color, bool closed = true, float sideLength = 8f)
        {
            DrawCircleSectorLines(center, radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, lineThickness, color, closed, sideLength); ;
        }
        public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, int sides, float lineThickness, Raylib_CsLo.Color color, bool closed = true)
        {
            float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
            float endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
            float anglePiece = endAngleDeg - startAngleRad;
            float angleStep = MathF.Abs(anglePiece) / sides;
            if (closed)
            {
                Vector2 sectorStart = center + ShapeVec.Rotate(ShapeVec.Right() * radius + new Vector2(lineThickness / 2, 0), startAngleRad);
                DrawLineEx(center, sectorStart, lineThickness, color);

                Vector2 sectorEnd = center + ShapeVec.Rotate(ShapeVec.Right() * radius + new Vector2(lineThickness / 2, 0), endAngleRad);
                DrawLineEx(center, sectorEnd, lineThickness, color);
            }
            for (int i = 0; i < sides; i++)
            {
                Vector2 start = center + ShapeVec.Rotate(ShapeVec.Right() * radius, startAngleRad + angleStep * i);
                Vector2 end = center + ShapeVec.Rotate(ShapeVec.Right() * radius, startAngleRad + angleStep * (i + 1));
                DrawLineEx(start, end, lineThickness, color);
            }
        }
        public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, int sides, float lineThickness, Raylib_CsLo.Color color, bool closed = true)
        {
            DrawCircleSectorLines(center, radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, sides, lineThickness, color, closed);
        }

        public static void DrawCircleLinesDotted(Vector2 center, float radius, int sidesPerGap, float lineThickness, Raylib_CsLo.Color color, float sideLength = 8f, int endCapSegments = 8)
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
                    if (endCapSegments > 5)
                    {
                        DrawCircle(start, lineThickness * 0.5f, color, endCapSegments);
                        DrawCircle(end, lineThickness * 0.5f, color, endCapSegments);
                    }
                    DrawLineEx(start, end, lineThickness, color);
                }

                remainingSize -= sideLength;
                if (remainingSize <= 0f)
                {
                    gap = !gap;
                    remainingSize = size;
                }
            }
        }
        public static void DrawCircleCheckeredLines(Vector2 pos, Vector2 alignement, float radius, float spacing, float lineThickness, float angleDeg, Raylib_CsLo.Color lineColor, Raylib_CsLo.Color bgColor, int circleSegments)
        {

            float maxDimension = radius;
            Vector2 size = new Vector2(radius, radius) * 2f;
            Vector2 aVector = alignement * size;
            Vector2 center = pos - aVector + size / 2;
            float rotRad = angleDeg * ShapeMath.DEGTORAD;

            if (bgColor.a > 0) DrawCircle(center, radius, bgColor, circleSegments);

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
                DrawLineEx(start, end, lineThickness, lineColor);
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
                DrawLineEx(start, end, lineThickness, lineColor);
                cur.X += spacing;
            }

        }
        
        public static int GetCircleSideCount(float radius, float maxLength = 10f)
        {
            float circumference = 2.0f * PI * radius;
            return (int)MathF.Max(circumference / maxLength, 1);
        }
        public static int GetCircleArcSideCount(float radius, float angleDeg, float maxLength = 10f)
        {
            float circumference = 2.0f * PI * radius * (angleDeg / 360f);
            return (int)MathF.Max(circumference / maxLength, 1);
        }
        public static float TransformAngleDeg(float angleDeg) { return 450f - angleDeg; }
        public static float TransformAngleRad(float angleRad) { return 2.5f * PI - angleRad; }

        #endregion

        #region Ring
        public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float lineThickness, Raylib_CsLo.Color color, float sideLength = 8f)
        {
            DrawCircleLines(center, innerRadius, lineThickness, color, sideLength);
            DrawCircleLines(center, outerRadius, lineThickness, color, sideLength);
        }
        public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float lineThickness, Raylib_CsLo.Color color, float sideLength = 8f)
        {
            DrawCircleSectorLines(center, innerRadius, startAngleDeg, endAngleDeg, lineThickness, color, false, sideLength);
            DrawCircleSectorLines(center, outerRadius, startAngleDeg, endAngleDeg, lineThickness, color, false, sideLength);

            float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
            float endAngleRad = endAngleDeg * ShapeMath.DEGTORAD;
            Vector2 innerStart = center + ShapeVec.Rotate(ShapeVec.Right() * innerRadius - new Vector2(lineThickness / 2, 0), startAngleRad);
            Vector2 outerStart = center + ShapeVec.Rotate(ShapeVec.Right() * outerRadius + new Vector2(lineThickness / 2, 0), startAngleRad);
            DrawLineEx(innerStart, outerStart, lineThickness, color);

            Vector2 innerEnd = center + ShapeVec.Rotate(ShapeVec.Right() * innerRadius - new Vector2(lineThickness / 2, 0), endAngleRad);
            Vector2 outerEnd = center + ShapeVec.Rotate(ShapeVec.Right() * outerRadius + new Vector2(lineThickness / 2, 0), endAngleRad);
            DrawLineEx(innerEnd, outerEnd, lineThickness, color);
        }
        public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, float lineThickness, Raylib_CsLo.Color color, float sideLength = 8f)
        {
            DrawRingLines(center, innerRadius, outerRadius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, lineThickness, color, sideLength);
        }
        public static void DrawRing(Vector2 center, float innerRadius, float outerRadius, Raylib_CsLo.Color color, float sideLength = 8f)
        {
            DrawRing(center, innerRadius, outerRadius, 0, 360, color, sideLength);
        }
        public static void DrawRing(Vector2 center, float innerRadius, float outerRadius, int sides, Raylib_CsLo.Color color)
        {
            DrawRing(center, innerRadius, outerRadius, 0, 360, sides, color);
        }
        public static void DrawRing(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, Raylib_CsLo.Color color, float sideLength = 10f)
        {
            float start = TransformAngleDeg(startAngleDeg);
            float end = TransformAngleDeg(endAngleDeg);
            float anglePiece = end - start;
            int sides = GetCircleArcSideCount(outerRadius, MathF.Abs(anglePiece), sideLength);
            Raylib.DrawRing(center, innerRadius, outerRadius, start, end, sides, color);
        }
        public static void DrawRing(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, int sides, Raylib_CsLo.Color color)
        {
            Raylib.DrawRing(center, innerRadius, outerRadius, TransformAngleDeg(startAngleDeg), TransformAngleDeg(endAngleDeg), sides, color);
        }
        public static void DrawRing(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, Raylib_CsLo.Color color, float sideLength = 10f)
        {
            DrawRing(center, innerRadius, outerRadius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, color, sideLength);
        }
        public static void DrawRing(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, int sides, Raylib_CsLo.Color color)
        {
            DrawRing(center, innerRadius, outerRadius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, sides, color);
        }

        #endregion

        #region Rectangle

        public static void Draw(this NinePatchRect npr, Raylib_CsLo.Color color)
        {
            var rects = npr.Rects;
            foreach (var r in rects)
            {
                r.Draw(color);
            }
        }
        public static void Draw(this NinePatchRect npr, Raylib_CsLo.Color sourceColor, Raylib_CsLo.Color patchColor)
        {
            npr.Source.Draw(sourceColor);
            var rects = npr.Rects;
            foreach (var r in rects)
            {
                r.Draw(patchColor);
            }
        }
        public static void DrawLines(this NinePatchRect npr, float lineThickness, Raylib_CsLo.Color color)
        {
            var rects = npr.Rects;
            foreach (var r in rects)
            {
                r.DrawLines(lineThickness, color);
            }
        }
        public static void DrawLines(this NinePatchRect npr, float sourceLineThickness, float patchLineThickness, Raylib_CsLo.Color sourceColor, Raylib_CsLo.Color patchColor)
        {
            npr.Source.DrawLines(sourceLineThickness, sourceColor);
            var rects = npr.Rects;
            foreach (var r in rects)
            {
                r.DrawLines(patchLineThickness, patchColor);
            }
        }
        
        public static void DrawGrid(this Rect r, int lines, float lineThickness, Raylib_CsLo.Color color)
        {
            //float hGap = r.width / lines;
            //float vGap = r.height / lines;
            Vector2 xOffset = new Vector2(r.Width / lines, 0f);// * i;
            Vector2 yOffset = new Vector2(0f, r.Height / lines);// * i;
            
            Vector2 tl = r.TopLeft;
            Vector2 tr = tl + new Vector2(r.Width, 0);
            Vector2 bl = tl + new Vector2(0, r.Height);

            for (int i = 0; i < lines; i++)
            {
                DrawLineEx(tl + xOffset * i, bl + xOffset * i, lineThickness, color);
                DrawLineEx(tl + yOffset * i, tr + yOffset * i, lineThickness, color);
            }
        }

        public static void DrawRect(Vector2 topLeft, Vector2 bottomRight, Raylib_CsLo.Color color) => DrawRectangleRec(new Rect(topLeft, bottomRight).Rectangle, color);
        public static void DrawRect(Vector2 topLeft, Vector2 bottomRight, Vector2 pivot, float rotDeg, Raylib_CsLo.Color color) => Draw(new Rect(topLeft, bottomRight),pivot, rotDeg, color);
        public static void DrawRectLines(Vector2 topLeft, Vector2 bottomRight, float lineThickness, Raylib_CsLo.Color color) => DrawLines(new Rect(topLeft, bottomRight),lineThickness,color);
        public static void DrawRectLines(Vector2 topLeft, Vector2 bottomRight, Vector2 pivot, float rotDeg, float lineThickness, Raylib_CsLo.Color color, bool rounded = false)
        {
            DrawLines(new Rect(topLeft, bottomRight), pivot, rotDeg, lineThickness, color);

        }

        public static void Draw(this Rect rect, Raylib_CsLo.Color color) => DrawRectangleRec(rect.Rectangle, color);
        public static void Draw(this Rect rect, Vector2 pivot, float rotDeg, Raylib_CsLo.Color color)
        {
            var rr = rect.Rotate(pivot, rotDeg); // SRect.RotateRect(rect, pivot, rotDeg);
            Raylib.DrawTriangle(rr.tl, rr.bl, rr.br, color);
            Raylib.DrawTriangle(rr.br, rr.tr, rr.tl, color);
        }
        public static void DrawLines(this Rect rect, float lineThickness, Raylib_CsLo.Color color) => Raylib.DrawRectangleLinesEx(rect.Rectangle, lineThickness, color);
        public static void DrawLines(this Rect rect, Vector2 pivot, float rotDeg, float lineThickness, Raylib_CsLo.Color color, bool rounded = false)
        {
            var rr = ShapeRect.Rotate(rect, pivot, rotDeg);

            if (rounded)
            {
                DrawCircle(rr.tl, lineThickness / 2, color);
                DrawCircle(rr.tr, lineThickness / 2, color);
                DrawCircle(rr.br, lineThickness / 2, color);
                DrawCircle(rr.bl, lineThickness / 2, color);

                DrawLineEx(rr.tl, rr.tr, lineThickness, color);
                DrawLineEx(rr.bl, rr.br, lineThickness, color);
                DrawLineEx(rr.tl, rr.bl, lineThickness, color);
                DrawLineEx(rr.tr, rr.br, lineThickness, color);
            }
            else
            {
                Vector2 leftExtension = ShapeVec.Rotate(new Vector2(-lineThickness / 2, 0f), rotDeg * ShapeMath.DEGTORAD);
                Vector2 rightExtension = ShapeVec.Rotate(new Vector2(lineThickness / 2, 0f), rotDeg * ShapeMath.DEGTORAD);
               
                DrawLineEx(rr.tl + leftExtension, rr.tr + rightExtension, lineThickness, color);
                DrawLineEx(rr.bl + leftExtension, rr.br + rightExtension, lineThickness, color);
                DrawLineEx(rr.tl, rr.bl, lineThickness, color);
                DrawLineEx(rr.tr, rr.br, lineThickness, color);
            }
        }

        public static void DrawVertices(this Rect rect, float vertexRadius, Raylib_CsLo.Color color)
        {
            rect.DrawVertices(vertexRadius, color, 8);
        }
        public static void DrawVertices(this Rect rect, float vertexRadius, Raylib_CsLo.Color color, int circleSegments = 8)
        {
            DrawCircle(rect.TopLeft, vertexRadius, color    , circleSegments);
            DrawCircle(rect.TopRight, vertexRadius, color   , circleSegments);
            DrawCircle(rect.BottomLeft, vertexRadius, color , circleSegments);
            DrawCircle(rect.BottomRight, vertexRadius, color, circleSegments);
        }
        public static void DrawRounded(this Rect rect, float roundness, int segments, Raylib_CsLo.Color color) => Raylib.DrawRectangleRounded(rect.Rectangle, roundness, segments, color);
        public static void DrawRoundedLines(this Rect rect, float roundness, float lineThickness, int segments, Raylib_CsLo.Color color) => Raylib.DrawRectangleRoundedLines(rect.Rectangle, roundness, segments, lineThickness, color);

        public static void DrawSlantedCorners(this Rect rect, Raylib_CsLo.Color color, float tlCorner, float trCorner, float brCorner, float blCorner)
        {
            var points = GetSlantedCornerPoints(rect, tlCorner, trCorner, brCorner, blCorner);
            DrawPolygonConvex(points, rect.Center, color);
        }
        public static void DrawSlantedCorners(this Rect rect, Vector2 pivot, float rotDeg, Raylib_CsLo.Color color, float tlCorner, float trCorner, float brCorner, float blCorner)
        {
            var poly = GetSlantedCornerPoints(rect, tlCorner, trCorner, brCorner, blCorner);
            poly.Rotate(pivot, rotDeg * ShapeMath.DEGTORAD);
            DrawPolygonConvex(poly, rect.Center, color);
            //var points = SPoly.Rotate(GetSlantedCornerPoints(rect, tlCorner, trCorner, brCorner, blCorner), pivot, rotDeg * SUtils.DEGTORAD);
            //DrawPolygonConvex(points, rect.Center, color);
        }
        public static void DrawSlantedCornersLines(this Rect rect, float lineThickness, Raylib_CsLo.Color color, float tlCorner, float trCorner, float brCorner, float blCorner)
        {
            var points = GetSlantedCornerPoints(rect, tlCorner, trCorner, brCorner, blCorner);
            DrawLines(points, lineThickness, color);
        }
        public static void DrawSlantedCornersLines(this Rect rect, Vector2 pivot, float rotDeg, float lineThickness, Raylib_CsLo.Color color, float tlCorner, float trCorner, float brCorner, float blCorner)
        {
            var poly = GetSlantedCornerPoints(rect, tlCorner, trCorner, brCorner, blCorner);
            poly.Rotate(pivot, rotDeg * ShapeMath.DEGTORAD);
            DrawLines(poly, lineThickness, color);
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
        public static Polygon GetSlantedCornerPoints(this Rect rect, float tlCorner, float trCorner, float brCorner, float blCorner)
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
        public static Polygon GetSlantedCornerPointsRelative(this Rect rect, float tlCorner, float trCorner, float brCorner, float blCorner)
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
        
        public static void DrawCorners(this Rect rect, float lineThickness, Raylib_CsLo.Color color, float tlCorner, float trCorner, float brCorner, float blCorner)
        {
            Vector2 tl = rect.TopLeft;
            Vector2 tr = rect.TopRight;
            Vector2 br = rect.BottomRight;
            Vector2 bl = rect.BottomLeft;

            if (tlCorner > 0f)
            {
                DrawCircle(tl, lineThickness / 2, color);
                DrawSegment(tl, tl + new Vector2(MathF.Min(tlCorner, rect.Width), 0f), lineThickness, color);
                DrawSegment(tl, tl + new Vector2(0f, MathF.Min(tlCorner, rect.Height)), lineThickness, color);
            }
            if (trCorner > 0f)
            {
                DrawCircle(tr, lineThickness / 2, color);
                DrawSegment(tr, tr - new Vector2(MathF.Min(trCorner, rect.Width), 0f), lineThickness, color);
                DrawSegment(tr, tr + new Vector2(0f, MathF.Min(trCorner, rect.Height)), lineThickness, color);
            }
            if (brCorner > 0f)
            {
                DrawCircle(br, lineThickness / 2, color);
                DrawSegment(br, br - new Vector2(MathF.Min(brCorner, rect.Width), 0f), lineThickness, color);
                DrawSegment(br, br - new Vector2(0f, MathF.Min(brCorner, rect.Height)), lineThickness, color);
            }
            if (blCorner > 0f)
            {
                DrawCircle(bl, lineThickness / 2, color);
                DrawSegment(bl, bl + new Vector2(MathF.Min(blCorner, rect.Width), 0f), lineThickness, color);
                DrawSegment(bl, bl - new Vector2(0f, MathF.Min(blCorner, rect.Height)), lineThickness, color);
            }
        }
        public static void DrawCorners(this Rect rect, float lineThickness, Raylib_CsLo.Color color, float cornerLength) => DrawCorners(rect, lineThickness, color, cornerLength, cornerLength, cornerLength, cornerLength);
        public static void DrawCornersRelative(this Rect rect, float lineThickness, Raylib_CsLo.Color color, float tlCorner, float trCorner, float brCorner, float blCorner)
        {
            Vector2 tl = rect.TopLeft;
            Vector2 tr = rect.TopRight;
            Vector2 br = rect.BottomRight;
            Vector2 bl = rect.BottomLeft;

            if (tlCorner > 0f && tlCorner < 1f)
            {
                DrawCircle(tl, lineThickness / 2, color);
                DrawSegment(tl, tl + new Vector2(tlCorner * rect.Width, 0f), lineThickness, color);
                DrawSegment(tl, tl + new Vector2(0f, tlCorner * rect.Height), lineThickness, color);
            }
            if (trCorner > 0f && trCorner < 1f)
            {
                DrawCircle(tr, lineThickness / 2, color);
                DrawSegment(tr, tr - new Vector2(tlCorner * rect.Width, 0f), lineThickness, color);
                DrawSegment(tr, tr + new Vector2(0f, tlCorner * rect.Height), lineThickness, color);
            }
            if (brCorner > 0f && brCorner < 1f)
            {
                DrawCircle(br, lineThickness / 2, color);
                DrawSegment(br, br - new Vector2(tlCorner * rect.Width, 0f), lineThickness, color);
                DrawSegment(br, br - new Vector2(0f, tlCorner * rect.Height), lineThickness, color);
            }
            if (blCorner > 0f && blCorner < 1f)
            {
                DrawCircle(bl, lineThickness / 2, color);
                DrawSegment(bl, bl + new Vector2(tlCorner * rect.Width, 0f), lineThickness, color);
                DrawSegment(bl, bl - new Vector2(0f, tlCorner * rect.Height), lineThickness, color);
            }
        }
        public static void DrawCornersRelative(this Rect rect, float lineThickness, Raylib_CsLo.Color color, float cornerLengthFactor) => DrawCornersRelative(rect, lineThickness, color, cornerLengthFactor, cornerLengthFactor, cornerLengthFactor, cornerLengthFactor);
        
        public static void DrawCheckered(this Rect rect, float spacing, float lineThickness, float angleDeg, Raylib_CsLo.Color lineColor, Raylib_CsLo.Color outlineColor, Raylib_CsLo.Color bgColor)
        {
            Vector2 size = new Vector2(rect.Width, rect.Height);
            Vector2 center = new Vector2(rect.X, rect.Y) + size / 2;
            float maxDimension = MathF.Max(size.X, size.Y);
            float rotRad = angleDeg * ShapeMath.DEGTORAD;

            Vector2 tl = new(rect.X, rect.Y);
            Vector2 tr = new(rect.X + rect.Width, rect.Y);
            Vector2 bl = new(rect.X, rect.Y + rect.Height);
            Vector2 br = new(rect.X + rect.Width, rect.Y + rect.Height);

            if (bgColor.a > 0) DrawRectangleRec(rect.Rectangle, bgColor);

            Vector2 cur = new(-spacing / 2, 0f);

            //safety for while loops
            int whileMaxCount = (int)(maxDimension / spacing) * 2;
            int whileCounter = 0;

            //left half of rectangle
            while (whileCounter < whileMaxCount)
            {
                Vector2 p = center + ShapeVec.Rotate(cur, rotRad);
                Vector2 up = new(0f, -maxDimension * 2);//make sure that lines are going outside of the rectangle
                Vector2 down = new(0f, maxDimension * 2);
                Vector2 start = p + ShapeVec.Rotate(up, rotRad);
                Vector2 end = p + ShapeVec.Rotate(down, rotRad);
                var seg = new Segment(start, end);
                CollisionPoints collisionPoints = seg.IntersectShape(rect); // SGeometry.IntersectionSegmentRect(center, start, end, tl, tr, br, bl).points;

                if (collisionPoints.Count >= 2) DrawLineEx(collisionPoints[0].Point, collisionPoints[1].Point, lineThickness, lineColor);
                else break;
                cur.X -= spacing;
                whileCounter++;
            }

            cur = new(spacing / 2, 0f);
            whileCounter = 0;
            //right half of rectangle
            while (whileCounter < whileMaxCount)
            {
                Vector2 p = center + ShapeVec.Rotate(cur, rotRad);
                Vector2 up = new(0f, -maxDimension * 2);
                Vector2 down = new(0f, maxDimension * 2);
                Vector2 start = p + ShapeVec.Rotate(up, rotRad);
                Vector2 end = p + ShapeVec.Rotate(down, rotRad);
                var seg = new Segment(start, end);
                CollisionPoints collisionPoints = seg.IntersectShape(rect); //SGeometry.IntersectionSegmentRect(center, start, end, tl, tr, br, bl).points;
                if (collisionPoints.Count >= 2) DrawLineEx(collisionPoints[0].Point, collisionPoints[1].Point, lineThickness, lineColor);
                else break;
                cur.X += spacing;
                whileCounter++;
            }

            if (outlineColor.a > 0) DrawLines(rect, new Vector2(0.5f, 0.5f), 0f, lineThickness, outlineColor);
        }

        public static void DrawLinesDotted(this Rect rect, int gapsPerSide, float lineThickness, Raylib_CsLo.Color color, int cornerCircleSectors = 8)
        {
            if (cornerCircleSectors > 5)
            {
                var corners = ShapeRect.GetCorners(rect);
                float r = lineThickness * 0.5f;
                DrawCircle(corners.tl, r, color, cornerCircleSectors);
                DrawCircle(corners.tr, r, color, cornerCircleSectors);
                DrawCircle(corners.br, r, color, cornerCircleSectors);
                DrawCircle(corners.bl, r, color, cornerCircleSectors);
            }


            var segments = rect.GetEdges();// SRect.GetEdges(rect);
            foreach (var s in segments)
            {
                DrawSegmentDotted(s.Start, s.End, gapsPerSide, lineThickness, color, cornerCircleSectors);
            }
        }
        public static void DrawLinesDotted(this Rect rect, int gapsPerSide, float gapSizeF, float lineThickness, Raylib_CsLo.Color color, int cornerCircleSegments = 8)
        {
            if (cornerCircleSegments > 5)
            {
                var corners = ShapeRect.GetCorners(rect);
                float r = lineThickness * 0.5f;
                DrawCircle(corners.tl, r, color, cornerCircleSegments);
                DrawCircle(corners.tr, r, color, cornerCircleSegments);
                DrawCircle(corners.br, r, color, cornerCircleSegments);
                DrawCircle(corners.bl, r, color, cornerCircleSegments);
            }


            var segments = rect.GetEdges(); // SRect.GetEdges(rect);
            foreach (var s in segments)
            {
                DrawSegmentDotted(s.Start, s.End, gapsPerSide, gapSizeF, lineThickness, color, cornerCircleSegments);
            }
        }


        #endregion

        #region Triangle
        public static void DrawTriangle(Vector2 a, Vector2 b, Vector2 c, Raylib_CsLo.Color color) => Raylib.DrawTriangle(a, b, c, color);
        public static void DrawTriangleLines(Vector2 a, Vector2 b, Vector2 c, float lineThickness, Raylib_CsLo.Color color) { new Triangle(a, b, c).GetEdges().Draw(lineThickness, color); }

        public static void Draw(this Triangle t, Raylib_CsLo.Color color) => Raylib.DrawTriangle(t.A, t.B, t.C, color);
        public static void DrawLines(this Triangle t, float lineThickness, Raylib_CsLo.Color color) { t.GetEdges().Draw(lineThickness, color); }

        public static void DrawVertices(this Triangle t, float vertexRadius, Raylib_CsLo.Color color)
        {
            t.DrawVertices(vertexRadius, color, 8);
        }
        public static void DrawVertices(this Triangle t, float vertexRadius, Raylib_CsLo.Color color, int circleSegments = 8)
        {
            DrawCircle(t.A, vertexRadius, color, circleSegments);
            DrawCircle(t.B, vertexRadius, color, circleSegments);
            DrawCircle(t.C, vertexRadius, color, circleSegments);
        }
        public static void Draw(this Triangulation triangles, Raylib_CsLo.Color color) { foreach (var t in triangles) t.Draw(color); }
        public static void DrawLines(this Triangulation triangles, float lineThickness, Raylib_CsLo.Color color) { foreach (var t in triangles) t.DrawLines(lineThickness, color); }
        #endregion

        #region Polygon

        public static void DrawPolygonConvex(this Polygon poly, Raylib_CsLo.Color color, bool clockwise = false) { DrawPolygonConvex(poly, poly.GetCentroid(), color, clockwise); }
        public static void DrawPolygonConvex(this Polygon poly, Vector2 center, Raylib_CsLo.Color color, bool clockwise = false)
        {
            if (clockwise)
            {
                for (int i = 0; i < poly.Count - 1; i++)
                {
                    Raylib.DrawTriangle(poly[i], center, poly[i + 1], color);
                }
                Raylib.DrawTriangle(poly[poly.Count - 1], center, poly[0], color);
            }
            else
            {
                for (int i = 0; i < poly.Count - 1; i++)
                {
                    Raylib.DrawTriangle(poly[i], poly[i + 1], center, color);
                }
                Raylib.DrawTriangle(poly[poly.Count - 1], poly[0], center, color);
            }
        }
        public static void DrawPolygonConvex(this Polygon relativePoly, Vector2 pos, float scale, float rotDeg, Raylib_CsLo.Color color, bool clockwise = false)
        {
            if (clockwise)
            {
                for (int i = 0; i < relativePoly.Count - 1; i++)
                {
                    Vector2 a = pos + ShapeVec.Rotate(relativePoly[i] * scale, rotDeg * ShapeMath.DEGTORAD);
                    Vector2 b = pos;
                    Vector2 c = pos + ShapeVec.Rotate(relativePoly[i + 1] * scale, rotDeg * ShapeMath.DEGTORAD);
                    Raylib.DrawTriangle(a, b, c, color);
                }

                Vector2 aFinal = pos + ShapeVec.Rotate(relativePoly[relativePoly.Count - 1] * scale, rotDeg * ShapeMath.DEGTORAD);
                Vector2 bFinal = pos;
                Vector2 cFinal = pos + ShapeVec.Rotate(relativePoly[0] * scale, rotDeg * ShapeMath.DEGTORAD);
                Raylib.DrawTriangle(aFinal, bFinal, cFinal, color);
            }
            else
            {
                for (int i = 0; i < relativePoly.Count - 1; i++)
                {
                    Vector2 a = pos + ShapeVec.Rotate(relativePoly[i] * scale, rotDeg * ShapeMath.DEGTORAD);
                    Vector2 b = pos + ShapeVec.Rotate(relativePoly[i + 1] * scale, rotDeg * ShapeMath.DEGTORAD);
                    Vector2 c = pos;
                    Raylib.DrawTriangle(a, b, c, color);
                }

                Vector2 aFinal = pos + ShapeVec.Rotate(relativePoly[relativePoly.Count - 1] * scale, rotDeg * ShapeMath.DEGTORAD);
                Vector2 bFinal = pos + ShapeVec.Rotate(relativePoly[0] * scale, rotDeg * ShapeMath.DEGTORAD);
                Vector2 cFinal = pos;
                Raylib.DrawTriangle(aFinal, bFinal, cFinal, color);
            }
        }
        
        public static void Draw(this Polygon poly, Raylib_CsLo.Color color) { poly.Triangulate().Draw(color); }

        public static void DEBUG_DrawLinesCCW(this Polygon poly, float lineThickness, Raylib_CsLo.Color startColor, Raylib_CsLo.Color endColor)
        {
            if (poly.Count < 3) return;

            var edges = poly.GetEdges();
            int redStep =   (endColor.r - startColor.r) / edges.Count;
            int greenStep = (endColor.g - startColor.g) / edges.Count;
            int blueStep =  (endColor.b - startColor.b) / edges.Count;
            int alphaStep = (endColor.a - startColor.a) / edges.Count;

            for (int i = 0; i < edges.Count; i++)
            {
                var edge = edges[i];
                Raylib_CsLo.Color finalColor = new
                    (
                        startColor.r + redStep * i,
                        startColor.g + greenStep * i,
                        startColor.b + blueStep * i,
                        startColor.a + alphaStep * i
                    );
                edge.Draw(lineThickness, finalColor);
            }
            ShapeDrawing.DrawCircle(poly[0], lineThickness * 2f, startColor);
            ShapeDrawing.DrawCircle(poly[poly.Count - 1], lineThickness * 2f, endColor);
        }
        
        public static void DrawLines(this Polygon poly, float lineThickness, Raylib_CsLo.Color startColor, Raylib_CsLo.Color endColor, int cornerSegments)
        {
            if (poly.Count < 3) return;

            var edges = poly.GetEdges();
            int redStep = (endColor.r - startColor.r) / edges.Count;
            int greenStep = (endColor.g - startColor.g) / edges.Count;
            int blueStep = (endColor.b - startColor.b) / edges.Count;
            int alphaStep = (endColor.a - startColor.a) / edges.Count;

            for (int i = 0; i < edges.Count; i++)
            {
                var edge = edges[i];
                Raylib_CsLo.Color finalColor = new
                    (
                        startColor.r + redStep * i,
                        startColor.g + greenStep * i,
                        startColor.b + blueStep * i,
                        startColor.a + alphaStep * i
                    );
                if(cornerSegments > 5) DrawCircle(edge.Start, lineThickness * 0.5f, finalColor, cornerSegments);
                edge.Draw(lineThickness, finalColor);
            }
        }
        public static void DrawLines(this Polygon poly, float lineThickness, Raylib_CsLo.Color color)
        {
            poly.DrawLines(lineThickness, color, 8);
        }
        public static void DrawLines(this Polygon poly, float lineThickness, Raylib_CsLo.Color color, int cornerSegments)
        {
            for (int i = 0; i < poly.Count - 1; i++)
            {
                //Raylib.DrawCircleSector(poly[i], lineThickness * 0.5f, 0, 360, 6, color);
                if(cornerSegments > 5) DrawCircle(poly[i], lineThickness * 0.5f, color, cornerSegments);
                DrawLineEx(poly[i], poly[i + 1], lineThickness, color);
            }
            if (cornerSegments > 5) DrawCircle(poly[poly.Count - 1], lineThickness * 0.5f, color, cornerSegments);
            DrawLineEx(poly[poly.Count - 1], poly[0], lineThickness, color);
        }
        
        public static void DrawLines(this Polygon poly, Vector2 pos, Vector2 size, float rotDeg, float lineThickness, Raylib_CsLo.Color outlineColor)
        {
            poly.DrawLines(pos, size, rotDeg, lineThickness, outlineColor, 8);
        }
        public static void DrawLines(this Polygon poly, Vector2 pos, Vector2 size, float rotDeg, float lineThickness, Raylib_CsLo.Color outlineColor, int cornerSegments)
        {
            for (int i = 0; i < poly.Count - 1; i++)
            {
                Vector2 p1 = pos + ShapeVec.Rotate(poly[i] * size, rotDeg * ShapeMath.DEGTORAD);
                Vector2 p2 = pos + ShapeVec.Rotate(poly[i + 1] * size, rotDeg * ShapeMath.DEGTORAD);
                if(cornerSegments > 5) DrawCircle(p1, lineThickness * 0.5f, outlineColor, 8);
                DrawLineEx(p1, p2, lineThickness, outlineColor);
            }
            if (cornerSegments > 5) DrawCircle(pos + ShapeVec.Rotate(poly[poly.Count - 1] * size, rotDeg * ShapeMath.DEGTORAD), lineThickness * 0.5f, outlineColor, 8);
            DrawLineEx(pos + ShapeVec.Rotate(poly[poly.Count - 1] * size, rotDeg * ShapeMath.DEGTORAD), pos + ShapeVec.Rotate(poly[0] * size, rotDeg * ShapeMath.DEGTORAD), lineThickness, outlineColor);
        }
        public static void DrawVertices(this Polygon poly, float vertexRadius, Raylib_CsLo.Color color)
        {
            poly.DrawVertices(vertexRadius, color, 18);
        }
        public static void DrawVertices(this Polygon poly, float vertexRadius, Raylib_CsLo.Color color, int circleSegments)
        {
            foreach (var p in poly)
            {
                DrawCircle(p, vertexRadius, color, circleSegments);
            }
        }
        
        public static void DrawCornered(this Polygon poly, float lineThickness, Raylib_CsLo.Color color, float cornerLength)
        {
            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 prev = poly[(i-1)%poly.Count];
                Vector2 cur = poly[i];
                Vector2 next = poly[(i+1)%poly.Count];
                DrawSegment(cur, cur + next.Normalize() * cornerLength, lineThickness, color);
                DrawSegment(cur, cur + prev.Normalize() * cornerLength, lineThickness, color);
            }
        }
        public static void DrawCornered(this Polygon poly, float lineThickness, Raylib_CsLo.Color color, List<float> cornerLengths)
        {
            for (int i = 0; i < poly.Count; i++)
            {
                float cornerLength = cornerLengths[i%cornerLengths.Count];
                Vector2 prev = poly[(i - 1) % poly.Count];
                Vector2 cur = poly[i];
                Vector2 next = poly[(i + 1) % poly.Count];
                DrawSegment(cur, cur + next.Normalize() * cornerLength, lineThickness, color);
                DrawSegment(cur, cur + prev.Normalize() * cornerLength, lineThickness, color);
            }
        }
        public static void DrawCorneredRelative(this Polygon poly, float lineThickness, Raylib_CsLo.Color color, float cornerF)
        {
            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 prev = poly[(i - 1) % poly.Count];
                Vector2 cur = poly[i];
                Vector2 next = poly[(i + 1) % poly.Count];
                DrawSegment(cur, cur.Lerp(next, cornerF), lineThickness, color);
                DrawSegment(cur, cur.Lerp(prev, cornerF), lineThickness, color);
            }
        }
        public static void DrawCorneredRelative(this Polygon poly, float lineThickness, Raylib_CsLo.Color color, List<float> cornerFactors)
        {
            for (int i = 0; i < poly.Count; i++)
            {
                float cornerF = cornerFactors[i%cornerFactors.Count];
                Vector2 prev = poly[(i - 1) % poly.Count];
                Vector2 cur = poly[i];
                Vector2 next = poly[(i + 1) % poly.Count];
                DrawSegment(cur, cur.Lerp(next, cornerF), lineThickness, color);
                DrawSegment(cur, cur.Lerp(prev, cornerF), lineThickness, color);
            }
        }


        #endregion

        #region Polyline
        public static void Draw(this Polyline polyline, float thickness, Raylib_CsLo.Color color) { polyline.GetEdges().Draw(thickness, color); }
        public static void Draw(this Polyline polyline, float thickness, List<Raylib_CsLo.Color> colors) { polyline.GetEdges().Draw(thickness, colors); }

        public static void DrawVertices(this Polyline polyline, float vertexRadius, Raylib_CsLo.Color color)
        {
            polyline.DrawVertices(vertexRadius, color, 18);
        }
        public static void DrawVertices(this Polyline polyline, float vertexRadius, Raylib_CsLo.Color color, int circleSegments)
        {
            foreach (var p in polyline)
            {
                DrawCircle(p, vertexRadius, color, circleSegments);
            }
        }

        public static void DrawDotted(this Polyline polyline, int gaps, float thickness, Raylib_CsLo.Color color, int endCapSegments = 8) { polyline.GetEdges().DrawDotted(gaps, thickness, color, endCapSegments); }
        public static void DrawDotted(this Polyline polyline, int gaps, float gapSizeF, float thickness, Raylib_CsLo.Color color, int endCapSegments = 8) { polyline.GetEdges().DrawDotted(gaps, gapSizeF, thickness, color, endCapSegments); }
        public static void DrawGlow(this Polyline polyline, float width, float endWidth, Raylib_CsLo.Color color, Raylib_CsLo.Color endColor, int steps) { polyline.GetEdges().DrawGlow(width, endWidth, color, endColor, steps); }

        #endregion

        #region Text


        public static (float fontSize, float fontSpacing, Vector2 textSize) GetDynamicFontSize(this Font font, string text, Vector2 size, float fontSpacing)
        {
            
            float fontSize = font.baseSize;
            fontSpacing = MathF.Min(fontSpacing, fontSize * FontSpacingMaxFactor);
            //float fontSpacingWidth = (text.Length - 1) * fontSpacing;
            //
            //if (fontSpacingWidth > size.X * 0.2f)
            //{
            //    float fontSpacingFactor = (size.X * 0.2f) / fontSpacingWidth;
            //    //finalSizeX = size.X - fontSpacingWidth * fontSpacingFactor;
            //    fontSpacingWidth *= fontSpacingFactor;
            //    fontSpacing *= fontSpacingFactor;
            //}
            //float finalSizeX = size.X - fontSpacingWidth;

            Vector2 fontDimensions = MeasureTextEx(font, text, fontSize, fontSpacing);
            float fX = size.X / fontDimensions.X;
            float fY = size.Y / fontDimensions.Y;
            float f = MathF.Min(fX, fY);

            float scaledFontSize = MathF.Max(fontSize * f, FontMinSize);
            float scaledFontSpacing = fontSpacing * f;
            return (scaledFontSize, scaledFontSpacing, font.GetTextSize(text, scaledFontSize, scaledFontSpacing));
        }
        public static Vector2 GetTextSize(this Font font, string text, float fontSize, float fontSpacing)
        {
            float totalWidth = 0f;

            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (c == '\n') continue;
                float w = font.GetCharWidth(c, fontSize);
                totalWidth += w;
            }
            float fontSpacingWidth = (text.Length - 1) * fontSpacing;
            totalWidth += fontSpacingWidth;
            return new Vector2(totalWidth, fontSize);
            //return MeasureTextEx(font, text, fontSize, fontSpacing);
        }
        public static float GetCharWidth(this Font font, Char c, float fontSize)
        {
            float baseWidth = font.GetCharBaseWidth(c);
            float f = fontSize / (float)font.baseSize;
            return baseWidth * f;
        }
        public static Vector2 GetCharSize(this Font font, Char c, float fontSize)
        {
            //unsafe
            //{
            //    float f = fontSize / (float)font.baseSize;
            //    int index = GetGlyphIndex(font, c);
            //    var glyphInfo = GetGlyphInfo(font, index);
            //    float glyphWidth = (font.glyphs[index].advanceX == 0) ? font.recs[index].width * f : font.glyphs[index].advanceX * f;
            //    return new Vector2(glyphWidth, fontSize);
            //}
            var baseSize = font.GetCharBaseSize(c);
            float f = fontSize / (float)font.baseSize;
            return baseSize * f;
        }
        public static float GetCharBaseWidth(this Font font, Char c)
        {
            unsafe
            {
                int index = GetGlyphIndex(font, c);
                var glyphInfo = GetGlyphInfo(font, index);
                float glyphWidth = (font.glyphs[index].advanceX == 0) ? font.recs[index].width : font.glyphs[index].advanceX;
                return glyphWidth;
            }
        }
        public static Vector2 GetCharBaseSize(this Font font, Char c)
        {
            unsafe
            {
                int index = GetGlyphIndex(font, c);
                var glyphInfo = GetGlyphInfo(font, index);
                float glyphWidth = (font.glyphs[index].advanceX == 0) ? font.recs[index].width : font.glyphs[index].advanceX;
                return new Vector2(glyphWidth, font.baseSize);
            }
        }
        
        private static (int emphasisIndex, bool connected) CheckWordEmphasis(int index, params WordEmphasis[] wordEmphasis)
        {
            for (int i = 0; i < wordEmphasis.Length; i++)
            {
                var emphasis = wordEmphasis[i];
                var checkResult = emphasis.CheckIndex(index);
                if (checkResult.contains) return (i, checkResult.connected);
                
            }
            return (-1, false);
        }
        private static void DrawEmphasisLine(Rect rect, TextEmphasisAlignement alignement, float lineThickness, Raylib_CsLo.Color color)
        {
            float radius = lineThickness * 0.5f;

            if(alignement == TextEmphasisAlignement.TopLeft)
            {
                Segment top = new(rect.TopLeft, rect.TopRight);
                top.Draw(lineThickness, color);

                Segment left = new(rect.TopLeft, rect.BottomLeft);
                left.Draw(lineThickness, color);

                Circle topLeft = new(rect.TopLeft, radius);
                Circle topEnd = new(top.End, radius);
                Circle leftEnd = new(left.End, radius);
                topLeft.Draw(color);
                topEnd.Draw(color);
                leftEnd.Draw(color);
            }
            else if (alignement == TextEmphasisAlignement.Top)
            {
                Segment s = new(rect.TopLeft, rect.TopRight);
                s.Draw(lineThickness, color);

                Circle start = new(s.Start, radius);
                Circle end = new(s.End, radius);

                start.Draw(color);
                end.Draw(color);
            }
            if (alignement == TextEmphasisAlignement.TopRight)
            {
                Segment top = new(rect.TopRight, rect.TopLeft);
                top.Draw(lineThickness, color);

                Segment right = new(rect.TopRight, rect.BottomRight);
                right.Draw(lineThickness, color);

                Circle topRight = new(rect.TopRight, radius);
                Circle topEnd = new(top.End, radius);
                Circle rightEnd = new(right.End, radius);
                topRight.Draw(color);
                topEnd.Draw(color);
                rightEnd.Draw(color);
            }
            else if (alignement == TextEmphasisAlignement.Right)
            {
                Segment s = new(rect.TopRight, rect.BottomRight);
                s.Draw(lineThickness, color);

                Circle start = new(s.Start, radius);
                Circle end = new(s.End, radius);

                start.Draw(color);
                end.Draw(color);
            }
            if (alignement == TextEmphasisAlignement.BottomRight)
            {
                Segment bottom = new(rect.BottomRight, rect.BottomLeft);
                bottom.Draw(lineThickness, color);

                Segment right = new(rect.BottomRight, rect.TopRight);
                right.Draw(lineThickness, color);

                Circle bottomRight = new(rect.BottomRight, radius);
                Circle bottomEnd = new(bottom.End, radius);
                Circle rightEnd = new(right.End, radius);
                bottomRight.Draw(color);
                bottomEnd.Draw(color);
                rightEnd.Draw(color);
            }
            else if (alignement == TextEmphasisAlignement.Bottom)
            {
                Segment s = new(rect.BottomLeft, rect.BottomRight);
                s.Draw(lineThickness, color);

                Circle start = new(s.Start, radius);
                Circle end = new(s.End, radius);

                start.Draw(color);
                end.Draw(color);
            }
            if (alignement == TextEmphasisAlignement.BottomLeft)
            {
                Segment bottom = new(rect.BottomLeft, rect.BottomRight);
                bottom.Draw(lineThickness, color);

                Segment left = new(rect.BottomLeft, rect.TopLeft);
                left.Draw(lineThickness, color);

                Circle bottomLeft = new(rect.BottomLeft, radius);
                Circle bottomEnd = new(bottom.End, radius);
                Circle leftEnd = new(left.End, radius);
                bottomLeft.Draw(color);
                bottomEnd.Draw(color);
                leftEnd.Draw(color);
            }
            else if (alignement == TextEmphasisAlignement.Left)
            {
                Segment s = new(rect.TopLeft, rect.BottomLeft);
                s.Draw(lineThickness, color);

                Circle start = new(s.Start, radius);
                Circle end = new(s.End, radius);

                start.Draw(color);
                end.Draw(color);
            }
            else if (alignement == TextEmphasisAlignement.Center)
            {
                Segment s = new(rect.GetPoint(new Vector2(0f, 0.5f)), rect.GetPoint(new Vector2(1f, 0.5f)));
                s.Draw(lineThickness, color);

                Circle start = new(s.Start, radius);
                Circle end = new(s.End, radius);

                start.Draw(color);
                end.Draw(color);
            }
            else if(alignement == TextEmphasisAlignement.Boxed)
            {
                rect.DrawLines(lineThickness, color);
            }

            
        }
        private static void DrawEmphasisCorner(Rect rect, TextEmphasisAlignement alignement, float lineThickness, Raylib_CsLo.Color color)
        {
            float cornerSize = lineThickness * 12f;

            if (alignement == TextEmphasisAlignement.TopLeft)
            {
                rect.DrawCorners(lineThickness, color, cornerSize, 0f, 0f, 0f);
            }
            else if (alignement == TextEmphasisAlignement.Top)
            {
                rect.DrawCorners(lineThickness, color, cornerSize, cornerSize, 0f, 0f);
            }
            else if (alignement == TextEmphasisAlignement.TopRight)
            {
                rect.DrawCorners(lineThickness, color, 0f, cornerSize, 0f, 0f);
            }
            else if (alignement == TextEmphasisAlignement.Right)
            {
                rect.DrawCorners(lineThickness, color, 0f, cornerSize, cornerSize, 0f);
            }
            else if (alignement == TextEmphasisAlignement.BottomRight)
            {
                rect.DrawCorners(lineThickness, color, 0f, 0f, cornerSize, 0f);
            }
            else if (alignement == TextEmphasisAlignement.Bottom)
            {
                rect.DrawCorners(lineThickness, color, 0f, 0f, cornerSize, cornerSize);
            }
            else if (alignement == TextEmphasisAlignement.BottomLeft)
            {
                rect.DrawCorners(lineThickness, color, 0f, 0f, 0f, cornerSize);
            }
            else if (alignement == TextEmphasisAlignement.Left)
            {
                rect.DrawCorners(lineThickness, color, cornerSize, 0f, 0f, cornerSize);
            }
            else if(alignement == TextEmphasisAlignement.Boxed)
            {
                rect.DrawCorners(lineThickness, color, cornerSize);
            }
            else if(alignement == TextEmphasisAlignement.TLBR)
            {
                rect.DrawCorners(lineThickness, color, cornerSize, 0f, cornerSize, 0f);
            }
            else if(alignement == TextEmphasisAlignement.BLTR)
            {
                rect.DrawCorners(lineThickness, color, 0f, cornerSize, 0f, cornerSize);
            }
        }
        private static void DrawEmphasis(Rect rect, WordEmphasis emphasis)
        {
            if (emphasis.EmphasisType == TextEmphasisType.None) return;

            rect = rect.ChangeSize(emphasis.SizeMargin, new Vector2(0.5f));
            float thickness = rect.Size.Y * emphasis.LineThicknessFactor;

            if (emphasis.EmphasisType == TextEmphasisType.Line) DrawEmphasisLine(rect, emphasis.EmphasisAlignement, thickness, emphasis.Color);
            else if (emphasis.EmphasisType == TextEmphasisType.Corner) DrawEmphasisCorner(rect, emphasis.EmphasisAlignement, thickness, emphasis.Color);
        }



        public static void DrawChar(this Font font, Char c, float fontSize, Vector2 topLeft, Raylib_CsLo.Color color)
        {
            Raylib.DrawTextCodepoint(font, c, topLeft, fontSize, color);
        }
        public static void DrawChar(this Font font, Char c, float fontSize, Vector2 pos, Vector2 alignement, WordEmphasis wordEmphasis)
        {
            Vector2 charSize = font.GetCharSize(c, fontSize);
            var r = new Rect(pos, charSize, alignement);
            Raylib.DrawTextCodepoint(font, c, r.TopLeft, fontSize, wordEmphasis.Color);
            if(wordEmphasis.EmphasisType != TextEmphasisType.None)
            {
                DrawEmphasis(r, wordEmphasis);
            }
        }
        public static void DrawChar(this Font font, Char c, Rect r, Vector2 alignement, WordEmphasis wordEmphasis)
        {
            float f = r.Size.Y / font.baseSize;
            float fontSize = font.baseSize * f;
            Vector2 charSize = font.GetCharSize(c, fontSize);
            
            Vector2 uiPos = r.GetPoint(alignement);
            Rect charRect = new Rect(uiPos, charSize, alignement);
            //Vector2 topLeft = uiPos - alignement * charSize;
            
            Raylib.DrawTextCodepoint(font, c, charRect.TopLeft, fontSize, wordEmphasis.Color);
            if (wordEmphasis.EmphasisType != TextEmphasisType.None)
            {
                DrawEmphasis(charRect, wordEmphasis);
            }
        }


        public static void DrawWord(this Font font, string word, Rect rect, float fontSpacing, Vector2 alignement, WordEmphasis wordEmphasis)
        {
            var info = font.GetDynamicFontSize(word, rect.Size, fontSpacing);
            Rect r = new(rect.GetPoint(alignement), info.textSize, alignement);
            DrawTextEx(font, word, r.TopLeft, info.fontSize, info.fontSpacing, wordEmphasis.Color);
            if (wordEmphasis.EmphasisType != TextEmphasisType.None)
            {
                DrawEmphasis(r, wordEmphasis);
            }
        }
        public static void DrawWord(this Font font, string word, float fontSize, float fontSpacing, Vector2 pos, Vector2 alignement, WordEmphasis wordEmphasis)
        {
            fontSize = MathF.Max(fontSize, FontMinSize);
            fontSpacing = MathF.Min(fontSpacing, fontSize * FontSpacingMaxFactor);

            Vector2 size = font.GetTextSize(word, fontSize, fontSpacing);
            Rect r = new(pos, size, alignement);
            DrawTextEx(font, word, r.TopLeft, fontSize, fontSpacing, wordEmphasis.Color);
            if (wordEmphasis.EmphasisType != TextEmphasisType.None)
            {
                DrawEmphasis(r, wordEmphasis);
            }
        }
        public static void DrawWord(this Font font, string word, float fontSize, float fontSpacing, Vector2 topLeft, WordEmphasis wordEmphasis)
        {
            DrawWord(font, word, fontSize, fontSpacing, topLeft, new Vector2(0f), wordEmphasis);
            //fontSize = MathF.Max(fontSize, FontMinSize);
            //fontSpacing = MathF.Min(fontSpacing, fontSize * FontSpacingMaxFactor);
            //
            //Vector2 size = font.GetTextSize(word, fontSize, fontSpacing);
            //Rect r = new(topLeft, size, new Vector2(0f));
            //DrawTextEx(font, word, r.TopLeft, fontSize, fontSpacing, wordEmphasis.Color);
            //if (wordEmphasis.EmphasisType != TextEmphasisType.None)
            //{
            //    DrawEmphasis(r, wordEmphasis.EmphasisType, wordEmphasis.EmphasisAlignement, wordEmphasis.Color);
            //}
        }


        public static void DrawText(this Font font, string text, float fontSize, float fontSpacing, Vector2 topleft, Raylib_CsLo.Color color)
        {
            DrawText(font, text, fontSize, fontSpacing, topleft, new Vector2(0f), color);
        }
        public static void DrawText(this Font font, string text, float fontSize, float fontSpacing, Vector2 pos, Vector2 alignement, Raylib_CsLo.Color color)
        {
            fontSize = MathF.Max(fontSize, FontMinSize);
            fontSpacing = MathF.Min(fontSpacing, fontSize * FontSpacingMaxFactor);
            Vector2 size = font.GetTextSize(text, fontSize, fontSpacing);
            Rect r = new(pos, size, alignement);
            DrawTextEx(font, text, r.TopLeft, fontSize, fontSpacing, color);
        }
        public static void DrawText(this Font font, string text, Rect rect, float fontSpacing, Vector2 alignement, Raylib_CsLo.Color color)
        {
            var info = font.GetDynamicFontSize(text, rect.Size, fontSpacing);
            Rect r = new(rect.GetPoint(alignement), info.textSize, alignement);
            //Vector2 uiPos = rect.GetPoint(alignement);
            //Vector2 topLeft = uiPos - alignement * info.textSize;
            DrawTextEx(font, text, r.TopLeft, info.fontSize, info.fontSpacing, color);
        }

        public static void DrawText(this Font font, string text, float fontSize, float fontSpacing, Vector2 pos, float rotDeg, Vector2 alignement, Raylib_CsLo.Color color)
        {
            fontSize = MathF.Max(fontSize, FontMinSize);
            fontSpacing = MathF.Min(fontSpacing, fontSize * FontSpacingMaxFactor);
            Vector2 size = font.GetTextSize(text, fontSize, fontSpacing);
            Vector2 originOffset = alignement * size;
            Rect r = new(pos, size, alignement);
            
            DrawTextPro(font, text, r.TopLeft + originOffset, originOffset, rotDeg, fontSize, fontSpacing, color);
        }
        public static void DrawText(this Font font, string text, Rect rect, float fontSpacing, float rotDeg, Vector2 alignement, Raylib_CsLo.Color color)
        {
            var info = font.GetDynamicFontSize(text, rect.Size, fontSpacing);
            Rect r = new(rect.GetPoint(alignement), info.textSize, alignement);
            Vector2 originOffset = alignement * info.textSize;
            DrawTextPro(font, text, r.TopLeft + originOffset, originOffset, rotDeg, info.fontSize, info.fontSpacing, color);
        }

        public static void DrawText(this Font font, string text, float fontSize, float fontSpacing, Vector2 topleft, WordEmphasis baseEmphasis, params WordEmphasis[] wordEmphasis)
        {
            DrawText(font, text, fontSize, fontSpacing, topleft, new Vector2(0f), baseEmphasis, wordEmphasis);
        }
        public static void DrawText(this Font font, string text, float fontSize, float fontSpacing, Vector2 pos, Vector2 alignement, WordEmphasis baseEmphasis, params WordEmphasis[] wordEmphasis)
        {
            Vector2 textSize = font.GetTextSize(text, fontSize, fontSpacing);
            Vector2 topLeft = pos - alignement * textSize;

            Vector2 curWordPos = topLeft;
            string curWord = string.Empty;
            int curWordIndex = 0;
            float curWordWidth = 0f;
            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];
                float w = GetCharWidth(font, c, fontSize) + fontSpacing;
                curWordWidth += w;
                if (c == ' ')
                {
                    var result = CheckWordEmphasis(curWordIndex, wordEmphasis);
                    
                    if (result.emphasisIndex != -1 && result.connected)
                    {
                        curWord += c;
                        curWordIndex++;
                        continue;
                    }

                    DrawWord(font, curWord, fontSize, fontSpacing, curWordPos, result.emphasisIndex < 0 ? baseEmphasis : wordEmphasis[result.emphasisIndex]);

                    curWord = string.Empty;
                    curWordIndex++;
                    curWordPos += new Vector2(curWordWidth, 0f);
                    curWordWidth = 0f;
                    
                    
                }
                else curWord += c;

            }
            var resultLast = CheckWordEmphasis(curWordIndex, wordEmphasis);
            DrawWord(font, curWord, fontSize, fontSpacing, curWordPos, resultLast.emphasisIndex < 0 ? baseEmphasis : wordEmphasis[resultLast.emphasisIndex]);
        }
        public static void DrawText(this Font font, string text, Rect rect, float fontSpacing, Vector2 alignement, WordEmphasis baseEmphasis, params WordEmphasis[] wordEmphasis)
        {
            var info = font.GetDynamicFontSize(text, rect.Size, fontSpacing);
            Vector2 uiPos = rect.GetPoint(alignement);
            Vector2 topLeft = uiPos - alignement * info.textSize;
            DrawText(font, text, info.fontSize, info.fontSpacing, topLeft, alignement, baseEmphasis, wordEmphasis);

        }


        public static void DrawTextWrappedChar(this Font font, string text, Rect rect, float fontSpacing, WordEmphasis baseEmphasis, params WordEmphasis[] wordEmphasis)
        {
            fontSpacing = MathF.Min(fontSpacing, font.baseSize * FontSpacingMaxFactor);

            float safetyMargin = TextWrappingAutoFontSizeSafetyMargin;
            Vector2 rectSize = rect.Size;
            Vector2 textSize = font.GetTextSize(text, font.baseSize, fontSpacing);
            float rectArea = rectSize.GetArea() * safetyMargin;
            float textArea = textSize.GetArea();

            float f = MathF.Sqrt(rectArea / textArea);
            fontSpacing *= f;
            float fontSize = font.baseSize * f;
            fontSize = MathF.Max(fontSize, FontMinSize);

            DrawTextWrappedChar(font, text, rect, fontSize, fontSpacing, 0f, baseEmphasis, wordEmphasis);
            //int curWordIndex = 0;
            //
            //Vector2 pos = rect.TopLeft;
            //for (int i = 0; i < text.Length; i++)
            //{
            //    var c = text[i];
            //    var charBaseSize = font.GetCharBaseSize(c);
            //    float glyphWidth = charBaseSize.X * f;
            //    if (pos.X + glyphWidth >= rect.TopLeft.X + rectSize.X)
            //    {
            //        pos.X = rect.TopLeft.X;
            //        pos.Y += fontSize;
            //    }
            //    if (c == ' ')
            //    {
            //        curWordIndex++;
            //    }
            //    else
            //    {
            //        var result = CheckWordEmphasis(curWordIndex, wordEmphasis);
            //        Raylib_CsLo.Color color = result.emphasisIndex < 0 ? baseEmphasis.Color : wordEmphasis[result.emphasisIndex].Color;
            //
            //        font.DrawChar(c, fontSize, pos, color);
            //    }
            //    
            //    pos.X += glyphWidth + fontSpacing;
            //}
        }
        public static void DrawTextWrappedChar(this Font font, string text, Rect rect, float fontSize, float fontSpacing, float lineSpacing, WordEmphasis baseEmphasis, params WordEmphasis[] wordEmphasis)
        {
            if (rect.Height < FontMinSize) return;

            fontSize = MathF.Max(fontSize, FontMinSize);
            fontSpacing = MathF.Min(fontSpacing, fontSize * FontSpacingMaxFactor);
            lineSpacing = MathF.Min(lineSpacing, fontSize * LineSpacingMaxFactor);

            if (rect.Height < fontSize) fontSize *= (rect.Height / fontSize);

            float f = fontSize / (float)font.baseSize;
            Vector2 pos = rect.TopLeft;

            int curWordIndex = 0;
            string curWord = string.Empty;
            string backlog = string.Empty;
            float curWordWidth = 0f;
            float curLineWidth = 0f;

            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (c == '\n') continue;

                var charBaseSize = font.GetCharBaseSize(c);
                float glyphWidth = charBaseSize.X * f;

                if (curLineWidth + curWordWidth + glyphWidth >= rect.Width)//break line
                {
                    var backlogResult = CheckWordEmphasis(curWordIndex, wordEmphasis);
                    DrawWord(font, backlog + curWord, fontSize, fontSpacing, pos, backlogResult.emphasisIndex < 0 ? baseEmphasis : wordEmphasis[backlogResult.emphasisIndex]);
                    backlog = string.Empty;
                    curWord = string.Empty;

                    pos.Y += fontSize + lineSpacing;
                    pos.X = rect.TopLeft.X;
                    curLineWidth = 0f;
                    curWordWidth = 0f;

                    if (pos.Y + fontSize >= rect.Bottom)
                    {
                        return;
                    }

                    if (c == ' ') curWordIndex++;
                    else
                    {
                        curWord += c;
                        curWordWidth += glyphWidth + fontSpacing;
                    }

                    continue;
                }

                curWordWidth += glyphWidth + fontSpacing;
                if (c == ' ')
                {
                    var result = CheckWordEmphasis(curWordIndex, wordEmphasis);

                    if (result.emphasisIndex != -1 && result.connected)
                    {
                        curLineWidth += curWordWidth;
                        curWordWidth = 0f;

                        curWord += c;
                        backlog += curWord;
                        curWord = string.Empty;

                        curWordIndex++;
                        continue;
                    }
                    DrawWord(font, backlog + curWord, fontSize, fontSpacing, pos, result.emphasisIndex < 0 ? baseEmphasis : wordEmphasis[result.emphasisIndex]);

                    curWord = string.Empty;
                    backlog = string.Empty;
                    curWordIndex++;
                    curLineWidth += curWordWidth;
                    pos.X = rect.TopLeft.X + curLineWidth; // curWordWidth;
                    curWordWidth = 0f;
                }
                else curWord += c;
            }

            //draw last word
            var resultLast = CheckWordEmphasis(curWordIndex, wordEmphasis);
            DrawWord(font, backlog + curWord, fontSize, fontSpacing, pos, resultLast.emphasisIndex < 0 ? baseEmphasis : wordEmphasis[resultLast.emphasisIndex]);

        }
        public static void DrawTextWrappedWord(this Font font, string text, Rect rect, float fontSpacing, WordEmphasis baseEmphasis, params WordEmphasis[] wordEmphasis)
        {
            fontSpacing = MathF.Min(fontSpacing, font.baseSize * FontSpacingMaxFactor);

            float safetyMargin = TextWrappingAutoFontSizeSafetyMargin;
            Vector2 rectSize = rect.Size;
            Vector2 textSize = font.GetTextSize(text, font.baseSize, fontSpacing);
            float rectArea = rectSize.GetArea() * safetyMargin;
            float textArea = textSize.GetArea();

            float f = MathF.Sqrt(rectArea / textArea);
            fontSpacing *= f;
            float fontSize = font.baseSize * f;
            fontSize = MathF.Max(fontSize, FontMinSize);
            DrawTextWrappedWord(font, text, rect, fontSize, fontSpacing, 0f, baseEmphasis, wordEmphasis);
        }
        public static void DrawTextWrappedWord(this Font font, string text, Rect rect, float fontSize, float fontSpacing, float lineSpacing, WordEmphasis baseEmphasis, params WordEmphasis[] wordEmphasis)
        {
            if (rect.Height < FontMinSize) return;

            fontSize = MathF.Max(fontSize, FontMinSize);
            fontSpacing = MathF.Min(fontSpacing, fontSize * FontSpacingMaxFactor);
            lineSpacing = MathF.Min(lineSpacing, fontSize * LineSpacingMaxFactor);

            if (rect.Height < fontSize) fontSize *= (rect.Height / fontSize);

            float f = fontSize / (float)font.baseSize;
            Vector2 pos = rect.TopLeft;

            int curWordIndex = 0;
            string curWord = string.Empty;
            string backlog = string.Empty;
            float curWordWidth = 0f;
            float curLineWidth = 0f;

            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (c == '\n') continue;

                var charBaseSize = font.GetCharBaseSize(c);
                float glyphWidth = charBaseSize.X * f;
                
                if (curLineWidth + curWordWidth + glyphWidth >= rect.Width)//break line
                {
                    bool charBreak = false;
                    if(backlog != string.Empty)
                    {
                        var result = CheckWordEmphasis(curWordIndex, wordEmphasis);
                        DrawWord(font, backlog, fontSize, fontSpacing, pos, result.emphasisIndex < 0 ? baseEmphasis : wordEmphasis[result.emphasisIndex]);
                        backlog = string.Empty;
                    }
                    else
                    {
                        if (curLineWidth <= 0f)//break line on first word
                        {
                            var result = CheckWordEmphasis(curWordIndex, wordEmphasis);

                            DrawWord(font, curWord, fontSize, fontSpacing, pos, result.emphasisIndex < 0 ? baseEmphasis : wordEmphasis[result.emphasisIndex]);

                            curWord = string.Empty;
                            curWordWidth = 0f;
                            charBreak = true;
                        }
                    }
                    
                    pos.Y += fontSize + lineSpacing;
                    pos.X = rect.TopLeft.X;
                    curLineWidth = 0f;
                    
                    if (pos.Y + fontSize >= rect.Bottom)
                    {
                        return;
                    }
                    
                    if (charBreak) 
                    {
                        if (c != ' ')
                        {
                            curWord += c;
                            curWordWidth += glyphWidth;
                        }
                        else curWordIndex++;

                        continue; 
                    }
                }

                curWordWidth += glyphWidth + fontSpacing;
                if (c == ' ')
                {
                    var result = CheckWordEmphasis(curWordIndex, wordEmphasis);

                    if (result.emphasisIndex != -1 && result.connected)
                    {
                        curLineWidth += curWordWidth;
                        curWordWidth = 0f;

                        curWord += c;
                        backlog += curWord;
                        curWord = string.Empty;

                        curWordIndex++;
                        continue;
                    }
                    DrawWord(font, backlog + curWord, fontSize, fontSpacing, pos, result.emphasisIndex < 0 ? baseEmphasis : wordEmphasis[result.emphasisIndex]);

                    curWord = string.Empty;
                    backlog = string.Empty;
                    curWordIndex++;
                    curLineWidth += curWordWidth;
                    pos.X = rect.TopLeft.X + curLineWidth; // curWordWidth;
                    curWordWidth = 0f;
                }
                else curWord += c;
            }

            //draw last word
            var resultLast = CheckWordEmphasis(curWordIndex, wordEmphasis);
            DrawWord(font, backlog + curWord, fontSize, fontSpacing, pos, resultLast.emphasisIndex < 0 ? baseEmphasis : wordEmphasis[resultLast.emphasisIndex]);

        }

        
        public static void DrawTextWrappedChar(this Font font, string text, Rect rect, float fontSpacing, Raylib_CsLo.Color color)
        {
            fontSpacing = MathF.Min(fontSpacing, font.baseSize * FontSpacingMaxFactor);

            float safetyMargin = 0.85f;
            Vector2 rectSize = rect.Size;
            Vector2 textSize = font.GetTextSize(text, font.baseSize, fontSpacing);
            float rectArea = rectSize.GetArea() * safetyMargin;
            float textArea = textSize.GetArea();
            
            float f = MathF.Sqrt(rectArea / textArea);
            fontSpacing *= f;
            float fontSize = font.baseSize * f;
            fontSize = MathF.Max(fontSize, FontMinSize);

            Vector2 pos = rect.TopLeft;
            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];
                var charBaseSize = font.GetCharBaseSize(c);
                float glyphWidth = charBaseSize.X * f;
                if (pos.X + glyphWidth >= rect.TopLeft.X + rectSize.X)
                {
                    pos.X = rect.TopLeft.X;
                    pos.Y += fontSize;
                }
                font.DrawChar(c, fontSize, pos, color);
                pos.X += glyphWidth + fontSpacing;
            }
        }
        public static void DrawTextWrappedChar(this Font font, string text, Rect rect, float fontSize, float fontSpacing, float lineSpacing, Raylib_CsLo.Color color)
        {
            fontSize = MathF.Max(fontSize, FontMinSize);
            fontSpacing = MathF.Min(fontSpacing, fontSize * FontSpacingMaxFactor);
            lineSpacing = MathF.Min(lineSpacing, fontSize * LineSpacingMaxFactor);

            float f = fontSize / (float)font.baseSize;
            Vector2 pos = rect.TopLeft;
            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];
                var charBaseSize = font.GetCharBaseSize(c);
                float glyphWidth = charBaseSize.X * f;
                if (pos.X + glyphWidth >= rect.Right)
                {
                    pos.X = rect.TopLeft.X;
                    pos.Y += fontSize + lineSpacing;
                    if(pos.Y + fontSize >= rect.Bottom)
                    {
                        return;
                    }
                }
                font.DrawChar(c, fontSize, pos, color);
                pos.X += glyphWidth + fontSpacing;
            }
        }
        public static void DrawTextWrappedWord(this Font font, string text, Rect rect, float fontSpacing, Raylib_CsLo.Color color)
        {
            fontSpacing = MathF.Min(fontSpacing, font.baseSize * FontSpacingMaxFactor);

            float safetyMargin = 0.75f;
            Vector2 rectSize = rect.Size;
            Vector2 textSize = font.GetTextSize(text, font.baseSize, fontSpacing);
            float rectArea = rectSize.GetArea() * safetyMargin;
            float textArea = textSize.GetArea();

            float f = MathF.Sqrt(rectArea / textArea);
            fontSpacing *= f;
            float fontSize = font.baseSize * f;
            fontSize = MathF.Max(fontSize, FontMinSize);
            DrawTextWrappedWord(font, text, rect, fontSize, fontSpacing, 0f, color);
        }
        public static void DrawTextWrappedWord(this Font font, string text, Rect rect, float fontSize, float fontSpacing, float lineSpacing, Raylib_CsLo.Color color)
        {
            if (rect.Height < FontMinSize) return;

            fontSize = MathF.Max(fontSize, FontMinSize);
            fontSpacing = MathF.Min(fontSpacing, fontSize * FontSpacingMaxFactor);
            lineSpacing = MathF.Min(lineSpacing, fontSize * LineSpacingMaxFactor);

            if (rect.Height < fontSize) fontSize *= (rect.Height / fontSize);
            
            float f = fontSize / (float)font.baseSize;
            Vector2 pos = rect.TopLeft;

            string curLine = string.Empty;
            string curWord = string.Empty;
            float curWidth = 0f;

            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];
                
                if(c != '\n')
                {
                    curWord += c;
                    if (c == ' ')
                    {
                        curLine += curWord;
                        curWord = "";
                    }
                    var charBaseSize = font.GetCharBaseSize(c);
                    float glyphWidth = charBaseSize.X * f;

                    if (curWidth + glyphWidth >= rect.Width)
                    {
                        if (curLine == string.Empty)//width was overshot within the first word
                        {
                            curWord = curWord.Remove(curWord.Length - 1);
                            curLine = curWord;
                            i--;
                        }
                        else i -= curWord.Length;

                        curLine = curLine.Trim();
                        Raylib.DrawTextEx(font, curLine, pos, fontSize, fontSpacing, color);

                        curWidth = 0;
                        
                        curWord = string.Empty;
                        curLine = string.Empty;
                        
                        pos.Y += fontSize + lineSpacing;
                        if (pos.Y + fontSize >= rect.Bottom)
                        {
                            return;
                        }
                    }
                    else curWidth += glyphWidth + fontSpacing;
                }
                else
                {
                    curLine += curWord;
                    curLine = curLine.Trim();
                    Raylib.DrawTextEx(font, curLine, pos, fontSize, fontSpacing, color);

                    curWidth = 0f;
                    curLine = string.Empty;
                    curWord = string.Empty;

                    pos.Y += fontSize + lineSpacing;
                    if (pos.Y + fontSize >= rect.Bottom)
                    {
                        return;
                    }
                }
            }

            
            curLine += curWord;
            curLine = curLine.Trim();
            Raylib.DrawTextEx(font, curLine, pos, fontSize, fontSpacing, color);

        }


        public static void DrawCaret(this Font font, string text, Rect rect, float fontSpacing, Vector2 textAlignment, int caretIndex, float caretWidth, Raylib_CsLo.Color caretColor)
        {
            var info = font.GetDynamicFontSize(text, rect.Size, fontSpacing);
            Vector2 uiPos = rect.GetPoint(textAlignment);
            Vector2 topLeft = uiPos - textAlignment * info.textSize;
            DrawCaret(font, text, topLeft, info.fontSize, info.fontSpacing, caretIndex, caretWidth, caretColor);

            //Vector2 caretTextSize = SDrawing.GetTextSize(font, text, info.fontSize, info.fontSpacing);
            //
            //Vector2 caretTop = topLeft + new Vector2(caretTextSize.X + fontSpacing * 0.5f, 0f);
            //Vector2 caretBottom = topLeft + new Vector2(caretTextSize.X + fontSpacing * 0.5f, info.textSize.Y);
            //DrawLineEx(caretTop, caretBottom, caretWidth, caretColor);

        }
        public static void DrawCaret(this Font font, string text, Rect rect, float fontSize, float fontSpacing, Vector2 textAlignment, int caretIndex, float caretWidth, Raylib_CsLo.Color caretColor)
        {
            Vector2 uiPos = rect.GetPoint(textAlignment);
            Vector2 topLeft = uiPos - textAlignment * GetTextSize(font, text, fontSize, fontSpacing);
            DrawCaret(font, text, topLeft, fontSize, fontSpacing, caretIndex, caretWidth, caretColor);
        }
        public static void DrawCaret(this Font font, string text, float fontSize, float fontSpacing, Vector2 pos, Vector2 textAlignment, int caretIndex, float caretWidth, Raylib_CsLo.Color caretColor)
        {
            Vector2 topLeft = pos - textAlignment * GetTextSize(font, text, fontSize, fontSpacing);
            DrawCaret(font, text, topLeft, fontSize, fontSpacing, caretIndex, caretWidth, caretColor);
        }
        public static void DrawCaret(this Font font, string text, Vector2 topLeft, float fontSize, float fontSpacing, int caretIndex, float caretWidth, Raylib_CsLo.Color caretColor)
        {
            string caretText = text.Substring(0, caretIndex);
            Vector2 caretTextSize = ShapeDrawing.GetTextSize(font, caretText, fontSize, fontSpacing);

            Vector2 caretTop = topLeft + new Vector2(caretTextSize.X + fontSpacing * 0.5f, 0f);
            Vector2 caretBottom = topLeft + new Vector2(caretTextSize.X + fontSpacing * 0.5f, fontSize);
            DrawLineEx(caretTop, caretBottom, caretWidth, caretColor);
        }
        
        public static void DrawTextBox(this Font font, string emptyText, string text, Rect rect, float fontSpacing, Vector2 textAlignment, Raylib_CsLo.Color textColor, int caretIndex, float caretWidth, Raylib_CsLo.Color caretColor)
        {
            string textBoxText = text.Length <= 0 ? emptyText : text;
            font.DrawText(textBoxText, rect, fontSpacing, textAlignment, textColor);
            font.DrawCaret(textBoxText, rect, fontSpacing, textAlignment, caretIndex, caretWidth, caretColor);
        }
        public static void DrawTextBox(this Font font, string emptyText, string text, Rect rect, float fontSize, float fontSpacing, Vector2 textAlignment, Raylib_CsLo.Color textColor, int caretIndex, float caretWidth, Raylib_CsLo.Color caretColor)
        {
            string textBoxText = text.Length <= 0 ? emptyText : text;
            font.DrawText(textBoxText, fontSize, fontSpacing, rect.GetPoint(textAlignment), textAlignment, textColor);
            font.DrawCaret(textBoxText, fontSize, fontSpacing, rect.GetPoint(textAlignment), textAlignment, caretIndex, caretWidth, caretColor);
        }
        public static void DrawTextBox(this Font font, string emptyText, string text, float fontSize, float fontSpacing, Vector2 pos, Vector2 textAlignment, Raylib_CsLo.Color textColor, int caretIndex, float caretWidth, Raylib_CsLo.Color caretColor)
        {
            string textBoxText = text.Length <= 0 ? emptyText : text;
            font.DrawText(textBoxText, fontSize, fontSpacing,  pos, textAlignment, textColor);
            font.DrawCaret(textBoxText, fontSize, fontSpacing, pos, textAlignment, caretIndex, caretWidth, caretColor);
        }

        #endregion

        #region UI
        public static void DrawOutlineBar(this Rect rect, float thickness, float f, Raylib_CsLo.Color color)
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
                DrawLineEx(start, end, thickness, color);
            }
        }
        public static void DrawOutlineBar(this Rect rect, Vector2 pivot, float angleDeg, float thickness, float f, Raylib_CsLo.Color color)
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
                DrawLineEx(start, end, thickness, color);
            }
        }

        public static void DrawOutlineBar(this Circle c, float thickness, float f, Raylib_CsLo.Color color)
        {
            DrawCircleSectorLines(c.Center, c.Radius, 0, 360 * f, thickness, color, false, 8f);
        }
        public static void DrawOutlineBar(this Circle c, float startOffsetDeg, float thickness, float f, Raylib_CsLo.Color color)
        {
            DrawCircleSectorLines(c.Center, c.Radius, 0, 360 * f, startOffsetDeg, thickness, color, false, 8f);
        }

        public static void DrawBar(this Rect rect, float f, Raylib_CsLo.Color barColor, Raylib_CsLo.Color bgColor, float left = 0f, float right = 1f, float top = 0f, float bottom = 0f)
        {
            f = 1.0f - f;
            UIMargins progressMargins = new(f * top, f * right, f * bottom, f * left);
            var progressRect = progressMargins.Apply(rect);
            ShapeDrawing.Draw(rect, bgColor);
            ShapeDrawing.Draw(progressRect, barColor);
        }
        public static void DrawBar(this Rect rect, Vector2 pivot, float angleDeg, float f, Raylib_CsLo.Color barColor, Raylib_CsLo.Color bgColor, float left = 0f, float right = 1f, float top = 0f, float bottom = 0f)
        {
            f = 1.0f - f;
            UIMargins progressMargins = new(f * top, f * right, f * bottom, f * left);
            var progressRect = progressMargins.Apply(rect);
            ShapeDrawing.Draw(rect, pivot, angleDeg, bgColor);
            ShapeDrawing.Draw(progressRect, pivot, angleDeg, barColor);
        }
        
        #endregion

    }
}