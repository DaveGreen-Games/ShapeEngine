using System.Numerics;
using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Core.Shapes;
using ShapeEngine.UI;

namespace ShapeEngine.Lib;

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
public static class ShapeText
{
    /// <summary>
    /// Automatically scales all text that is drawn with functions from ShapeText.
    /// 0.5 means text is draw at half the size, 2 means that text is drawn at twice the size.
    /// </summary>
    public static float FontSizeModifier = 1f;
    
    
    
    #region Text (Old System)

    
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
            var caretTextSize = GetTextSize(font, caretText, fontSize, fontSpacing);

            var caretTop = topLeft + new Vector2(caretTextSize.X + fontSpacing * 0.5f, 0f);
            var caretBottom = topLeft + new Vector2(caretTextSize.X + fontSpacing * 0.5f, fontSize);
            // DrawLineEx(caretTop, caretBottom, caretWidth, caretColor);
            DrawCaret(caretTop, caretBottom, caretWidth, caretColor);
        }

        public static void DrawCaret(Vector2 top, Vector2 bottom, float width, Raylib_CsLo.Color color)
        {
            DrawLineEx(top, bottom, width, color);
        }
        
        //a function that calculates caret position (Vector2) with any text and any wrap mode
        
        //drawing the caret with wrap mode is more complicated!
        //basically get substring to caret index and then calculate how far down & how far right the caret top point is
        
        
        
        
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

    

}



public class ED_Block : IEmphasisDrawer
{
    public void DrawBackground(Rect rect, Raylib_CsLo.Color color)
    {
        rect.Draw(color);
    }

    public void DrawForeground(Rect rect, Raylib_CsLo.Color color)
    {
    }
}
public class ED_Underline : IEmphasisDrawer
{
    public void DrawBackground(Rect rect, Raylib_CsLo.Color color)
    {
    }

    public void DrawForeground(Rect rect, Raylib_CsLo.Color color)
    {
        float lineThickness = rect.Size.Min() * 0.1f;
        Segment s = new(rect.BottomLeft, rect.BottomRight);
        s.Draw(lineThickness, color, LineCapType.Extended);

    }
}
public class ED_Transparent : IEmphasisDrawer
{
    public void DrawBackground(Rect rect, Raylib_CsLo.Color color)
    {
    }

    public void DrawForeground(Rect rect, Raylib_CsLo.Color color)
    {
    }
}
public interface IEmphasisDrawer
{
    public void DrawBackground(Rect rect, Raylib_CsLo.Color color);
    public void DrawForeground(Rect rect, Raylib_CsLo.Color color);
}
public class Emphasis
{
    private readonly IEmphasisDrawer drawer;
    public Raylib_CsLo.Color Color;
    public Raylib_CsLo.Color TextColor;
    
    public Emphasis(IEmphasisDrawer drawer, Raylib_CsLo.Color color, Raylib_CsLo.Color textColor)
    {
        this.drawer = drawer;
        this.Color = color;
        this.TextColor = textColor;
    }

    public void DrawForeground(Rect rect) => drawer.DrawForeground(rect, Color);
    public void DrawBackground(Rect rect) =>  drawer.DrawBackground(rect, Color);
}
public class TextEmphasis
{
    private readonly string[] keywords;
    public readonly Emphasis Emphasis;

    public TextEmphasis(Emphasis emphasis, params string[] keywords)
    {
        this.Emphasis = emphasis;
        this.keywords = keywords;
    }

    public bool HasKeyword(string word)
    {
        if (keywords.Length <= 0) return false;
        return keywords.Contains(word);
    }
}

public enum TextWrapType
{
    None = 0,
    Char = 1,
    Word = 2
}


public class TextBlock
{
    #region Members

    public RangeFloat FontSizeRange = new(5, 150);

    public UIMargins EmphasisRectMargins = new();
    
    public readonly List<TextEmphasis> Emphases = new();
    
    public Font Font;
    public float FontSpacing = 0;
    public float LineSpacing = 0;
    public Raylib_CsLo.Color Color = Raylib.WHITE;
    #endregion

    #region Main
    public TextBlock(Font font)
    {
        this.Font = font;
    }
    
    public void Draw(char c, Rect r, Vector2 alignement)
    {
        float f = r.Size.Y / Font.baseSize;
        float fontSize = Font.baseSize * f;
        var charSize = GetCharSize(c, fontSize);
        
        var uiPos = r.GetPoint(alignement);
        var charRect = new Rect(uiPos, charSize, alignement);
        
        Raylib.DrawTextCodepoint(Font, c, charRect.TopLeft, fontSize, Color);
    }
    public void Draw(string text, Rect rect, float rotDeg, Vector2 alignement)
    {
        var info = GetDynamicFontSize(text, rect.Size);
        Rect r = new(rect.GetPoint(alignement), info.textSize, alignement);
        var originOffset = alignement * info.textSize;
        DrawTextPro(Font, text, r.TopLeft + originOffset, originOffset, rotDeg, info.fontSize, info.fontSpacing, Color);
    }
    public void Draw(string text, Rect rect, Vector2 alignement, TextWrapType textWrapType = TextWrapType.None)
    {
        if(textWrapType == TextWrapType.None)
        {
            DrawTextWrapNone(text, rect, alignement);
        }
        else if (textWrapType == TextWrapType.Char)
        {
            DrawTextWrapChar(text, rect, alignement);
        }
        else
        {
            DrawTextWrapWord(text, rect, alignement);
        }
        
    }
    #endregion
    
    #region TextSize
    public (float fontSize, float fontSpacing, Vector2 textSize) GetDynamicFontSize(string text, Vector2 size)
    {
        float fontSize = Font.baseSize;
        float fontSpacing = FontSpacing; // MathF.Min(FontSpacing, fontSize * FontSpacingMaxFactor);
        
        var fontDimensions = MeasureTextEx(Font, text, fontSize, fontSpacing);
        float fX = size.X / fontDimensions.X;
        float fY = size.Y / fontDimensions.Y;
        float f = MathF.Min(fX, fY);

        float scaledFontSize = FontSizeRange.Clamp(fontSize * f);// MathF.Max(fontSize * f, FontMinSize);
        float scaledFontSpacing = fontSpacing * f;
        return (scaledFontSize, scaledFontSpacing, GetTextSize(text, scaledFontSize));
    }
    public Vector2 GetTextSize(string text, float fontSize)
    {
        float totalWidth = 0f;

        for (int i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (c == '\n') continue;
            float w = GetCharSize(c, fontSize).X;
            totalWidth += w;
        }
        float fontSpacingWidth = (text.Length - 1) * FontSpacing;
        totalWidth += fontSpacingWidth;
        return new Vector2(totalWidth, fontSize);
    }
    public Vector2 GetTextSize(string text, float fontSize, float fontSpacing)
    {
        float totalWidth = 0f;

        for (int i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (c == '\n') continue;
            float w = GetCharSize(c, fontSize).X;
            totalWidth += w;
        }
        float fontSpacingWidth = (text.Length - 1) * fontSpacing;
        totalWidth += fontSpacingWidth;
        return new Vector2(totalWidth, fontSize);
    }
    public Vector2 GetCharSize(char c, float fontSize)
    {
        var baseSize = GetCharBaseSize(c);
        float f = fontSize / (float)Font.baseSize;
        return baseSize * f;
    }
    public Vector2 GetCharBaseSize(char c)
    {
        unsafe
        {
            int index = GetGlyphIndex(Font, c);
            // var glyphInfo = GetGlyphInfo(font, index);
            float glyphWidth = (Font.glyphs[index].advanceX == 0) ? Font.recs[index].width : Font.glyphs[index].advanceX;
            return new Vector2(glyphWidth, Font.baseSize);
        }
    }
    #endregion
    
    #region Private
    private Emphasis? GetEmphasis(string word)
    {
        foreach (var e in Emphases)
        {
            if (e.HasKeyword(word)) return e.Emphasis;
        }

        return null;
    }
    
    private void DrawWord(string text, Rect rect, Vector2 alignement, Emphasis emphasis)
    {
        var info = GetDynamicFontSize(text, rect.Size);
        Rect r = new(rect.GetPoint(alignement), info.textSize, alignement);
        
        // float margin = r.Size.Min() * EmphasisRectMargin;
        var emphasisRect = EmphasisRectMargins.Apply(r); // r.ApplyMarginsAbsolute(margin, margin, margin, margin);
        
        emphasis.DrawForeground(emphasisRect);
        DrawTextEx(Font, text, r.TopLeft, info.fontSize, info.fontSpacing, emphasis.TextColor);
        emphasis.DrawBackground(emphasisRect);
        
    }
    private void DrawWord(string text, Rect rect, Vector2 alignement)
    {
        var info = GetDynamicFontSize(text, rect.Size);
        Rect r = new(rect.GetPoint(alignement), info.textSize, alignement);
        
        DrawTextEx(Font, text, r.TopLeft, info.fontSize, info.fontSpacing, Color);
    }
   
    private void DrawWord(string text, float fontSize, float fontSpacing, Vector2 topLeft, float width, Emphasis emphasis)
    {
        Rect r = new(topLeft, new Vector2(width, fontSize), new());
        
        // float margin = -r.Size.Min() * EmphasisRectMargin;
        var emphasisRect = EmphasisRectMargins.Apply(r); // r.ApplyMarginsAbsolute(margin, margin, margin, margin);
        
        emphasis.DrawBackground(emphasisRect);
        DrawTextEx(Font, text, r.TopLeft, fontSize, fontSpacing, emphasis.TextColor);
        emphasis.DrawForeground(emphasisRect);
        
    }
    private void DrawWord(string text, float fontSize, float fontSpacing, Vector2 topLeft)
    {
        DrawTextEx(Font, text, topLeft, fontSize, fontSpacing, Color);
    }
    
    private void DrawTextWrapNone(string text, Rect rect, Vector2 alignement)
    {
        if(Emphases.Count <= 0) DrawWord(text, rect, alignement);
        else
        {
            var info = GetDynamicFontSize(text, rect.Size);
            var uiPos = rect.GetPoint(alignement);
            var topLeft = uiPos - alignement * info.textSize;
            
            var curWordPos = topLeft;
            var curWord = string.Empty;
            var curWordWidth = 0f;
            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                float w = GetCharSize(c, info.fontSize).X + info.fontSpacing;
                curWordWidth += w;
                if (c == ' ')
                {
                    var wordEmphasis = GetEmphasis(curWord);

                    if (wordEmphasis != null) DrawWord(curWord, info.fontSize, info.fontSpacing, curWordPos, curWordWidth, wordEmphasis);
                    else DrawWord(curWord, info.fontSize, info.fontSpacing, curWordPos);
                    
                    curWord = string.Empty;
                    curWordPos += new Vector2(curWordWidth, 0f);
                    curWordWidth = 0f;
                }
                else curWord += c;

            }
            
            var lastWordEmphasis = GetEmphasis(curWord);
            if (lastWordEmphasis != null) DrawWord(curWord, info.fontSize, info.fontSpacing, curWordPos, curWordWidth, lastWordEmphasis);
            else DrawWord(curWord, info.fontSize, info.fontSpacing, curWordPos);
        }
        
    }

    private void DrawTextWrapChar(string text, Rect rect, Vector2 alignement)
    {
        float fontSpacing = FontSpacing;
        float lineSpacing = LineSpacing;
        var rectSize = rect.Size;
        var textSize = GetTextSize(text, Font.baseSize, fontSpacing);

        if (textSize.X < rectSize.X)//no wrapping needed
        {
            DrawTextWrapNone(text, rect, alignement);
        }
        else
        {
            var lines = (int)MathF.Ceiling((textSize.X * 1.2f) / rectSize.X);
            var textHeight = lines * Font.baseSize;
            var lineSpacingHeight = lines <= 0 ? 0 : (lines - 1) * lineSpacing;
            var height = textHeight + lineSpacingHeight;
            
            var textArea = rectSize.X * height;
            var sizeF = MathF.Sqrt(rectSize.GetArea() / textArea);
            float fontSize = FontSizeRange.Clamp(Font.baseSize * sizeF);
            lineSpacing *= sizeF;
            fontSpacing *= sizeF;

            // var words = text.Split(' ');
            
            
            var pos = rect.TopLeft;
            
            var curWord = string.Empty;
            var lineBreakInProcess = false;
            Emphasis? lineBreakEmphasis = null;
            var curWordWidth = 0f;
            var curLineWidth = 0f;
            
            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (c == '\n') continue;
            
                var charBaseSize = GetCharBaseSize(c);
                float glyphWidth = charBaseSize.X * sizeF;
            
                if (curLineWidth + curWordWidth + glyphWidth >= rect.Width)//break line
                {
                    
                    if (c == ' ') 
                    {
                        var emphasis = GetEmphasis(curWord);
                        if(emphasis != null) DrawWord(curWord, fontSize, fontSpacing, pos, curWordWidth, emphasis);
                        else  DrawWord(curWord, fontSize, fontSpacing, pos);
                    }
                    else
                    {
                        if (!lineBreakInProcess)
                        {
                            lineBreakInProcess = true;
                            string completeWord = curWord;
                            for (int j = i; j < text.Length; j++)
                            {
                                var nextChar = text[j];
                                if (j >= text.Length - 1)
                                {
                                    if (nextChar != ' ') completeWord += nextChar;
                                    lineBreakEmphasis = GetEmphasis(completeWord);
                                }
                                else
                                {
                                    if (nextChar == ' ')
                                    {
                                        lineBreakEmphasis = GetEmphasis(completeWord);
                                        break;
                                    }
                                    
                                    completeWord += nextChar;
                                }
                                
                            }
                        }
                        
                        if(lineBreakEmphasis != null) DrawWord(curWord, fontSize, fontSpacing, pos, curWordWidth, lineBreakEmphasis);
                        else  DrawWord(curWord, fontSize, fontSpacing, pos);
                    } 
                        
                    
                    curWord = string.Empty;
                    curWord += c;
                    pos.Y += fontSize + lineSpacing;
                    pos.X = rect.TopLeft.X;
                    curLineWidth = 0f;
                    curWordWidth = glyphWidth;// 0f;
                    
                    continue;
                }
            
                curWordWidth += glyphWidth + fontSpacing;
                if (c == ' ')
                {
                    if (lineBreakInProcess)
                    {
                        lineBreakInProcess = false;
                        if (lineBreakEmphasis != null) DrawWord(curWord, fontSize, fontSpacing, pos, curWordWidth, lineBreakEmphasis);
                        else DrawWord(curWord, fontSize, fontSpacing, pos);
                    }
                    else
                    {
                        var wordEmphasis = GetEmphasis(curWord);
                        if (wordEmphasis != null) DrawWord(curWord, fontSize, fontSpacing, pos, curWordWidth, wordEmphasis);
                        else DrawWord(curWord, fontSize, fontSpacing, pos);
                    }
            
                    curWord = string.Empty;
                    curLineWidth += curWordWidth;
                    pos.X = rect.TopLeft.X + curLineWidth;
                    curWordWidth = 0f;
                }
                else curWord += c;
            }
            
            if (lineBreakInProcess)
            {
                if (lineBreakEmphasis != null) DrawWord(curWord, fontSize, fontSpacing, pos, curWordWidth, lineBreakEmphasis);
                else DrawWord(curWord, fontSize, fontSpacing, pos);
            }
            else
            {
                var wordEmphasis = GetEmphasis(curWord);
                if (wordEmphasis != null) DrawWord(curWord, fontSize, fontSpacing, pos, curWordWidth, wordEmphasis);
                else DrawWord(curWord, fontSize, fontSpacing, pos);
            }
            
        }
        
        /* backup
         var pos = rect.TopLeft;
           
           var curWord = string.Empty;
           var lineBreakWord = string.Empty;
           List<char> overshotChars = new();
           List<float> overshotCharWidths = new();
           var curWordWidth = 0f;
           var curLineWidth = 0f;
           
           for (int i = 0; i < text.Length; i++)
           {
               var c = text[i];
               if (c == '\n') continue;
           
               var charBaseSize = GetCharBaseSize(c);
               float glyphWidth = charBaseSize.X * sizeF;
           
               if (curLineWidth + curWordWidth + glyphWidth >= rect.Width ||overshotChars.Count > 0)//break line
               {
                   if (c == ' ')
                   {
                       var wordEmphasis = GetEmphasis(curWord);

                       if (overshotChars.Count > 0)
                       {
                           if (wordEmphasis != null) DrawWord(lineBreakWord, fontSize, fontSpacing, pos, curWordWidth, wordEmphasis);
                           else DrawWord(lineBreakWord, fontSize, fontSpacing, pos);

                           pos.Y += fontSize + lineSpacing;
                           pos.X = rect.TopLeft.X;
                           curLineWidth = 0f;
                           curWordWidth = 0f;
                           curWord = string.Empty;
                           lineBreakWord = string.Empty;
                           
                           for (int j = 0; j < overshotChars.Count - 1; j++)
                           {
                               if (curWordWidth <= 0)
                               {
                                   curWord += overshotChars[j];
                                   curWordWidth += overshotCharWidths[j] + fontSpacing;
                                   if (curWordWidth >= rect.Width)
                                   {
                                       if (wordEmphasis != null) DrawWord(curWord, fontSize, fontSpacing, pos, curWordWidth, wordEmphasis);
                                       else DrawWord(curWord, fontSize, fontSpacing, pos);
                                       
                                       pos.Y += fontSize + lineSpacing;
                                       pos.X = rect.TopLeft.X;
                                       curWordWidth = 0f;
                                       curWord = string.Empty;
                                   }
                               }
                               else
                               {
                                   if (curWordWidth + overshotCharWidths[j] + fontSpacing >= rect.Width)
                                   {
                                       if (wordEmphasis != null) DrawWord(curWord, fontSize, fontSpacing, pos, curWordWidth, wordEmphasis);
                                       else DrawWord(curWord, fontSize, fontSpacing, pos);
                                       
                                       pos.Y += fontSize + lineSpacing;
                                       pos.X = rect.TopLeft.X;
                                       curWordWidth = 0f;
                                       curWord = string.Empty;
                                   }
                                   
                                   curWord += overshotChars[j];
                                   curWordWidth += overshotCharWidths[j] + fontSpacing;
                               }
                           }

                           if (curWordWidth > 0)
                           {
                               if (wordEmphasis != null) DrawWord(curWord, fontSize, fontSpacing, pos, curWordWidth, wordEmphasis);
                               else DrawWord(curWord, fontSize, fontSpacing, pos);

                               curLineWidth += curWordWidth;
                               pos.X += curWordWidth;
                               curWordWidth = 0f;
                               curWord = string.Empty;
                               overshotChars.Clear();
                               overshotCharWidths.Clear();
                           }
                       }
                       else
                       {
                           if (wordEmphasis != null) DrawWord(curWord, fontSize, fontSpacing, pos, curWordWidth, wordEmphasis);
                           else DrawWord(curWord, fontSize, fontSpacing, pos);
                           
                           curWord = string.Empty;
                           lineBreakWord = string.Empty;
                           overshotChars.Clear();
                           overshotCharWidths.Clear();
                           pos.Y += fontSize + lineSpacing;
                           pos.X = rect.TopLeft.X;
                           curLineWidth = 0f;
                           curWordWidth = 0f;
                       }
                       
                   }
                   else
                   {
                       if (overshotChars.Count <= 0)
                       {
                           lineBreakWord = curWord;
                       }
                       overshotChars.Add(c);
                       overshotCharWidths.Add(glyphWidth);
                       curWord += c;
                       // else
                       // {
                       //     curWordWidth += glyphWidth + fontSpacing;
                       // }
                       
                   }
                   
                   continue;
               }
           
               curWordWidth += glyphWidth + fontSpacing;
               if (c == ' ')
               {
                   var wordEmphasis = GetEmphasis(curWord);
                   if (wordEmphasis != null) DrawWord(curWord, fontSize, fontSpacing, pos, curWordWidth, wordEmphasis);
                   else DrawWord(curWord, fontSize, fontSpacing, pos);
           
                   curWord = string.Empty;
                   curLineWidth += curWordWidth;
                   pos.X = rect.TopLeft.X + curLineWidth; // curWordWidth;
                   curWordWidth = 0f;
               }
               else curWord += c;
           }
           
           var lastWordEmphasis = GetEmphasis(curWord);
           if (lastWordEmphasis != null) DrawWord(curWord, fontSize, fontSpacing, pos, curWordWidth, lastWordEmphasis);
           else DrawWord(curWord, fontSize, fontSpacing, pos);
         */
        
    }
    private void DrawTextWrapWord(string text, Rect rect, Vector2 alignement)
    {
        // float fontSpacing = FontSpacing;// MathF.Min(FontSpacing, Font.baseSize * FontSpacingMaxFactor);
        // float lineSpacing = 0f;
        // var rectSize = rect.Size;// * TextWrappingAutoFontSizeSafetyMargin;
        // var textSize = GetTextSize(text, Font.baseSize, fontSpacing);
        float fontSpacing = FontSpacing;
        float lineSpacing = LineSpacing;
        var rectSize = rect.Size;
        var textSize = GetTextSize(text, Font.baseSize, fontSpacing);

        if (textSize.X < rectSize.X)
        {
            DrawTextWrapNone(text, rect, alignement);
        }
        else
        {
            var lines = (int)MathF.Ceiling((textSize.X * 1.5f) / rectSize.X);
            var textHeight = lines * Font.baseSize;
            var lineSpacingHeight = lines <= 0 ? 0 : (lines - 1) * lineSpacing;
            var height = textHeight + lineSpacingHeight;
            
            var textArea = rectSize.X * height;
            var sizeF = MathF.Sqrt(rectSize.GetArea() / textArea);
            float fontSize = FontSizeRange.Clamp(Font.baseSize * sizeF);
            lineSpacing *= sizeF;
            fontSpacing *= sizeF;
            
            // var lines = (int)MathF.Ceiling((textSize.X * 1.4f) / rectSize.X);
            // var height = lines * Font.baseSize;
            // var f =  MathF.Sqrt(rectSize.Y / height);
            // fontSpacing *= f;
            // float fontSize = Font.baseSize * f;
            
            var pos = rect.TopLeft;
        
            var curWord = string.Empty;
            var curWordWidth = 0f;
            var curLineWidth = 0f;
            // Emphasis? charBreakEmphasis = null;
            
            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (c == '\n') continue;
            
                var charBaseSize = GetCharBaseSize(c);
                float glyphWidth = charBaseSize.X * sizeF;
                
                if (curLineWidth + curWordWidth + glyphWidth >= rect.Width)//break line
                {
                    bool charBreak = false;
                    
                    if (curLineWidth <= 0f)//break line on first word
                    {
                        var wordEmphasis = GetEmphasis(curWord);
                        
                        if (wordEmphasis != null) DrawWord(curWord, fontSize, fontSpacing, pos, curWordWidth, wordEmphasis);
                        else DrawWord(curWord, fontSize, fontSpacing, pos);
        
                        curWord = string.Empty;
                        curWordWidth = 0f;
                        charBreak = true;
                    }
                    
                    
                    pos.Y += fontSize + lineSpacing;
                    pos.X = rect.TopLeft.X;
                    curLineWidth = 0f;
                    
                    // if (pos.Y + fontSize >= rect.Bottom)
                    // {
                    //     return;
                    // }
                    
                    if (charBreak) 
                    {
                        if (c != ' ')
                        {
                            curWord += c;
                            curWordWidth += glyphWidth;
                        }
                        continue; 
                    }
                }
            
                curWordWidth += glyphWidth + fontSpacing;
                if (c == ' ')
                {
                    var wordEmphasis = GetEmphasis(curWord);

                    if (wordEmphasis != null) DrawWord(curWord, fontSize, fontSpacing, pos, curWordWidth, wordEmphasis);
                    else DrawWord(curWord, fontSize, fontSpacing, pos);
            
                    curWord = string.Empty;
                    curLineWidth += curWordWidth;
                    pos.X = rect.TopLeft.X + curLineWidth; // curWordWidth;
                    curWordWidth = 0f;
                }
                else curWord += c;
            }
            
            //draw last word
            var lastWordEmphasis = GetEmphasis(curWord);

            if (lastWordEmphasis != null) DrawWord(curWord, fontSize, fontSpacing, pos, curWordWidth, lastWordEmphasis);
            else DrawWord(curWord, fontSize, fontSpacing, pos);
        
        }

        
    }
    #endregion
}




// public class TextWord
// {
//     public int StartIndex;
//     public int Length;
//     
// }
// public class TextLine
// {
//     public readonly List<Emphasis> Emphases = new();
//     
//     public string Text;
//     public Rect Rect;
//     
//     public Font Font;
//     public float FontSpacing;
//
//     public Raylib_CsLo.Color Color;
//     
//     private bool dirty = false;
//
//
//     //split text into parts if emphasis exist and are found
//     //text block does the same
//     //string split function? 
//     //recalculate if emphasis change or text changes
//
//     public void Draw()
//     {
//         //if dirty
//             //calculate dynamic font size
//             //topleft position
//             
//         
//     }
// }
//
// public class TextBlock : TextLine
// {
//     public float LineSpacing;
//     //word wrap type Char/Word
// }


//struct for Font, FontSize, FontColor
//=> can calculate dynamic font size
//=> static shape text class has a Member for that struct that can be set to be used in all text drawing functions
//=> text drawing functions only need text, rect & emphasis anymore
//=>

// public struct Label
// {
//     public Rect Rect;
//     public Font Font;
//     public float FontSpacing;
//     public Raylib_CsLo.Color Color;
// }

//TODO could be used for text drawing functions
//public struct Label
//{
//    public Rect Area;
//    public string Text;
//    public Raylib_CsLo.Color Color;
//    public float FontSpacing;
//    public Label(Rect area, string text, Raylib_CsLo.Color color, float fontSpacing = 1)
//    {
//        Text = text;
//        Area = area;
//        Color = color;
//        FontSpacing = fontSpacing;
//    }
//}

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
// public readonly struct FontInfo
// {
//     public readonly Font Font;
//     public readonly float Size;
//     public readonly float Spacing;
//     public readonly float LineSpacing;
//
//     public FontInfo(Font font, float fontSize, float fontSpacing, float lineSpacing)
//     {
//         this.Font = font;
//         this.Size = fontSize;
//         this.Spacing = fontSpacing;
//         this.LineSpacing = lineSpacing;
//     }
// }

// public class Word
// {
//     public string Text;
//     public int StartIndex;
//     public int WordIndex;
//
//     public Word(string text, int startIndex, int wordIndex)
//     {
//         this.Text = text;
//         this.StartIndex = startIndex;
//         this.WordIndex = wordIndex;
//     }
// }
// public class TextBlock
// {
//     public readonly List<Word> Words;
//     
//     public TextBlock(string text)
//     {
//         // text.Split(' ');
//         Words = new();
//         var curWord = string.Empty;
//         int curWordIndex = 0;
//         int curWordStartIndex = 0;
//
//         for (int i = 0; i < text.Length; i++)
//         {
//             var c = text[i];
//             if (c == ' ')
//             {
//                 if (curWord.Length > 0)
//                 {
//                     Word w = new(curWord, curWordStartIndex, curWordIndex);
//                     Words.Add(w);
//                     curWord = string.Empty;
//                     curWordStartIndex = 0;
//                     curWordIndex++;
//                 }
//             }
//             else
//             {
//                 if (curWord.Length <= 0) curWordStartIndex = i;
//                 curWord += c;
//
//             }
//         }
//     }
//
// }


// public class Word
// {
//     public readonly Vector2 TopLeft;
//     public readonly string Text;
//     public readonly float Width;
//     public readonly float Height;
//     public int Characters => Text.Length;
//
//     public Word(string text, Vector2 topLeft, Font font, float fontSize, float fontSpacing, float lineSpacing)
//     {
//         this.TopLeft = topLeft;
//         this.Text = text;
//         
//         var size = ShapeDrawing.GetTextSize(font, text, fontSize, fontSpacing);
//         Width = size.X;
//         Height = size.Y + lineSpacing;
//         
//
//     }
//     public Word(Vector2 topLeft, string text, float width, float height)
//     {
//         this.TopLeft = topLeft;
//         this.Text = text;
//         this.Width = width;
//         this.Height = height;
//     }
// }
// public class Sentence
// {
//     public bool Empty => Words.Count <= 0;
//     public Vector2 TopLeft;
//     public readonly List<Word> Words = new();
//     public float Width = 0;
//     public float Height = 0;
//     public int Characters = 0;
//
//     public Sentence() { }
//     public Sentence(params Word[] words)
//     {
//         foreach (var word in words)
//         {
//             Add(word);
//         }
//     }
//     public Sentence(IEnumerable<Word> words)
//     {
//         foreach (var word in words)
//         {
//             Add(word);
//         }
//     }
//
//     public void Add(Word word)
//     {
//         if (Empty) TopLeft = word.TopLeft;
//         Words.Add(word);
//         Width += word.Width;
//         Characters += word.Characters;
//         Height = MathF.Max(Height, word.Height);
//     }
//     
// }
//
// public class TextBlock
// {
//     public bool Empty => Sentences.Count <= 0;
//     public Vector2 TopLeft;
//     public float Width;
//     public float Height;
//     public float Length;
//     public int Characters;
//     
//     public readonly List<Sentence> Sentences = new();
//
//     public TextBlock()
//     {
//         
//     }
//
//     public void AddSentence(Sentence sentence)
//     {
//         if (Empty) TopLeft = sentence.TopLeft;
//         Sentences.Add(sentence);
//         Width = MathF.Max(Width, sentence.Width);
//         Height += sentence.Height;
//         Length += sentence.Characters;
//     }
// }
//


//---------------OLD TEXT BOX SYSTEM---------------

// public struct TextBox
// {
//     public string Text;
//     public int CaretIndex;
//     public bool Active;
//     
//     public TextBox(string text, int caretIndex, bool Active)
//     {
//         this.Text = text;
//         this.CaretIndex = caretIndex;
//         this.Active = Active;
//     }
// }
// public struct TextBoxKeys
// {
//     public KeyboardKey KeyActivate = KeyboardKey.KEY_ENTER;
//     public KeyboardKey KeyFinish = KeyboardKey.KEY_ENTER;
//     public KeyboardKey KeyCancel = KeyboardKey.KEY_ESCAPE;
//     public KeyboardKey KeyRight = KeyboardKey.KEY_RIGHT;
//     public KeyboardKey KeyLeft = KeyboardKey.KEY_LEFT;
//     public KeyboardKey KeyDelete = KeyboardKey.KEY_DELETE;
//     public KeyboardKey KeyBackspace = KeyboardKey.KEY_BACKSPACE;
//     public TextBoxKeys() { }
// }
// public static TextBox UpdateTextBox(this TextBox textBox, TextBoxKeys keys)
    // {
    //     if (!textBox.Active)
    //     {
    //         if (IsKeyPressed(keys.KeyActivate))
    //         {
    //             textBox.Active = true;
    //         }
    //         return textBox;
    //     }
    //     else
    //     {
    //         if (IsKeyPressed(keys.KeyCancel))
    //         {
    //             textBox.Active = false;
    //             textBox.Text = string.Empty;
    //             textBox.CaretIndex = 0;
    //         }
    //         else if (IsKeyPressed(keys.KeyFinish))
    //         {
    //             textBox.Active = false;
    //         }
    //         else if (IsKeyPressed(keys.KeyDelete))
    //         {
    //             var info = ShapeText.TextDelete(textBox.Text, textBox.CaretIndex);
    //             textBox.Text = info.text;
    //             textBox.CaretIndex = info.caretIndex;
    //         }
    //         else if (IsKeyPressed(keys.KeyBackspace))
    //         {
    //             var info = ShapeText.TextBackspace(textBox.Text, textBox.CaretIndex);
    //             textBox.Text = info.text;
    //             textBox.CaretIndex = info.caretIndex;
    //         }
    //         else if (IsKeyPressed(keys.KeyLeft))
    //         {
    //             textBox.CaretIndex = ShapeText.DecreaseCaretIndex(textBox.CaretIndex, textBox.Text.Length);
    //         }
    //         else if (IsKeyPressed(keys.KeyRight))
    //         {
    //             textBox.CaretIndex = ShapeText.IncreaseCaretIndex(textBox.CaretIndex, textBox.Text.Length);
    //         }
    //         else
    //         {
    //             var info = ShapeText.GetTextInput(textBox.Text, textBox.CaretIndex);
    //             textBox.Text = info.text;
    //             textBox.CaretIndex = info.newCaretPosition;
    //         }
    //         return textBox;
    //     }
    //
    //     
    // }
    //
    // public static TextBox IncreaseCaretIndex(this TextBox textBox) { return ChangeCaretIndex(textBox, 1); }
    // public static TextBox DecreaseCaretIndex(this TextBox textBox) { return ChangeCaretIndex(textBox, -1); }
    // public static TextBox ChangeCaretIndex(this TextBox textBox, int amount)
    // {
    //     textBox.CaretIndex += amount;
    //     if (textBox.CaretIndex < 0) textBox.CaretIndex = textBox.Text.Length;
    //     else if (textBox.CaretIndex > textBox.Text.Length) textBox.CaretIndex = 0;
    //     return textBox;
    // }
    // public static TextBox TextBackspace(this TextBox textBox)
    // {
    //     if (textBox.CaretIndex < 0 || textBox.CaretIndex > textBox.Text.Length) return textBox;
    //     if (textBox.Text.Length <= 0) return textBox;
    //
    //     if (textBox.CaretIndex > 0)
    //     {
    //         textBox.CaretIndex -= 1;
    //         textBox.Text = textBox.Text.Remove(textBox.CaretIndex, 1);
    //     }
    //     else
    //     {
    //         textBox.Text = textBox.Text.Remove(textBox.CaretIndex, 1);
    //     }
    //     return textBox;
    // }
    // public static TextBox TextDelete(this TextBox textBox)
    // {
    //     if (textBox.CaretIndex < 0 || textBox.CaretIndex > textBox.Text.Length) return textBox;
    //     if (textBox.Text.Length <= 0) return textBox;
    //
    //     if (textBox.CaretIndex < textBox.Text.Length)
    //     {
    //         textBox.Text = textBox.Text.Remove(textBox.CaretIndex, 1);
    //     }
    //     else
    //     {
    //         textBox.CaretIndex -= 1;
    //         textBox.Text = textBox.Text.Remove(textBox.CaretIndex, 1);
    //     }
    //     return textBox;
    // }
    // public static TextBox GetTextInput(this TextBox textBox)
    // {
    //     List<Char> characters = textBox.Text.ToList();
    //     int unicode = Raylib.GetCharPressed();
    //     while (unicode != 0)
    //     {
    //         var c = (char)unicode;
    //         if (textBox.CaretIndex < 0 || textBox.CaretIndex >= characters.Count) characters.Add(c);
    //         else
    //         {
    //             characters.Insert(textBox.CaretIndex, c);
    //
    //         }
    //         textBox.CaretIndex++;
    //         unicode = Raylib.GetCharPressed();
    //     }
    //     textBox.Text = new string(characters.ToArray());
    //     return textBox;
    // }
    // public static int IncreaseCaretIndex(int caretIndex, int textLength) { return ChangeCaretIndex(caretIndex, 1, textLength); }
    // public static int DecreaseCaretIndex(int caretIndex, int textLength) { return ChangeCaretIndex(caretIndex, -1, textLength); }
    // public static int ChangeCaretIndex(int caretIndex, int amount, int textLength)
    // {
    //     caretIndex += amount;
    //     if (caretIndex < 0) caretIndex = textLength;
    //     else if (caretIndex > textLength) caretIndex = 0;
    //     return caretIndex;
    // }
    //
    // public static (string text, int caretIndex) TextBackspace(string text, int caretIndex)
    // {
    //     if (caretIndex < 0 || caretIndex > text.Length) return (text, 0);
    //     if (text.Length <= 0) return (text, 0);
    //
    //     if (caretIndex > 0)
    //     {
    //         return (text.Remove(caretIndex - 1, 1), caretIndex - 1);
    //
    //     }
    //     else
    //     {
    //         return (text.Remove(caretIndex, 1), caretIndex);
    //     }
    // }
    // public static (string text, int caretIndex) TextDelete(string text, int caretIndex)
    // {
    //     if (caretIndex < 0 || caretIndex > text.Length) return (text, 0);
    //     if (text.Length <= 0) return (text, 0);
    //     if (caretIndex < text.Length)
    //     {
    //         return (text.Remove(caretIndex, 1), caretIndex);
    //     }
    //     else
    //     {
    //         return (text.Remove(caretIndex - 1, 1), caretIndex - 1);
    //     }
    // }
    //
    // public static (string text, int newCaretPosition) GetTextInput(string curText, int caretIndex)
    // {
    //     List<Char> characters = curText.ToList();
    //     int unicode = Raylib.GetCharPressed();
    //     while (unicode != 0)
    //     {
    //         var c = (char)unicode;
    //         if (caretIndex < 0 || caretIndex >= characters.Count) characters.Add(c);
    //         else
    //         {
    //             characters.Insert(caretIndex, c);
    //
    //         }
    //         caretIndex++;
    //         unicode = Raylib.GetCharPressed();
    //     }
    //     return (new string(characters.ToArray()), caretIndex);
    // }
    // public static string GetTextInput(string curText)
    // {
    //     if (IsKeyPressed(KeyboardKey.KEY_BACKSPACE)) curText = curText.Remove(curText.Length - 1);
    //     int unicode = Raylib.GetCharPressed();
    //     while (unicode != 0)
    //     {
    //         var c = (char)unicode;
    //         curText += c;
    //
    //         unicode = Raylib.GetCharPressed();
    //     }
    //     return curText;
    // }