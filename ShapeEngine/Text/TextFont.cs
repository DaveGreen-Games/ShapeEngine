using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Lib;
using ShapeEngine.UI;

namespace ShapeEngine.Text;

public struct TextFont
{
    #region Static Members

    public static UIMargins EmphasisRectMargins = new();
    public static RangeFloat FontSizeRange = new(10, 150);
    private static float fontSizeModifier = 1f;
    public static float FontSizeModifier
    {
        get => fontSizeModifier;
        set => fontSizeModifier = MathF.Max(value, 0.1f);
    }


    #endregion

    #region Members

    public Font Font;
    public float BaseSize => Font.BaseSize;
    public float FontSize;
    public float FontSpacing;
    public float LineSpacing;
    public ColorRgba ColorRgba;


    #endregion

    #region Constructors

    public TextFont(Font font)
    {
        Font = font;
        FontSpacing = 0f;
        LineSpacing = 0f;
        FontSize = font.BaseSize;
        ColorRgba = new(System.Drawing.Color.White);
    }
    public TextFont(Font font, ColorRgba colorRgba)
    {
        Font = font;
        FontSpacing = 0f;
        LineSpacing = 0f;
        FontSize = font.BaseSize;
        ColorRgba = colorRgba;
    }
    public TextFont(Font font, float fontSpacing, ColorRgba colorRgba)
    {
        
        Font = font;
        FontSpacing = fontSpacing;
        LineSpacing = 0f;
        FontSize = font.BaseSize;
        ColorRgba = colorRgba;
    }
    public TextFont(Font font, float fontSpacing, float lineSpacing, ColorRgba colorRgba)
    {
        Font = font;
        FontSpacing = fontSpacing;
        LineSpacing = lineSpacing;
        FontSize = font.BaseSize;
        ColorRgba = colorRgba;
    }
    public TextFont(Font font, float fontSize, float fontSpacing, float lineSpacing, ColorRgba colorRgba)
    {
        Font = font;
        FontSize = fontSize;
        FontSpacing = fontSpacing;
        LineSpacing = lineSpacing;
        ColorRgba = colorRgba;
    }


    #endregion
    
    #region Size

    // public TextFont ScaleDynamic(string text)
    // {
    //     var size = MeasureTextEx(Font, text, FontSize, FontSpacing);
    //     return ScaleDynamic(text, size);
    // }
    public TextFont ScaleDynamic(string text, Vector2 rectSize)
    {
        var fontDimensions = GetTextBaseSize(text);// MeasureTextEx(Font, text, BaseSize, FontSpacing);
        float fX = rectSize.X / fontDimensions.X;
        float fY = rectSize.Y / fontDimensions.Y;
        float f = MathF.Min(fX, fY);

        float scaledFontSize = FontSizeRange.Clamp(BaseSize * f);
        f = scaledFontSize / BaseSize;

        var newTextFont = new TextFont(Font, scaledFontSize, FontSpacing * f, LineSpacing * f, ColorRgba);

        return newTextFont;
    }
    public TextFont ScaleDynamicWrapMode(string text, Vector2 rectSize, float widthFactor = 1.3f)
    {
        var fontSpacing = FontSpacing;
        var lineSpacing = LineSpacing;
        var textSize = GetTextBaseSize(text);
        var lines = (int)MathF.Ceiling((textSize.X * widthFactor) / rectSize.X);
        var textHeight = lines * BaseSize;
        var lineSpacingHeight = lines <= 0 ? 0 : (lines - 1) * lineSpacing;
        var height = textHeight + lineSpacingHeight;

        var textArea = rectSize.X * height;
        var sizeF = MathF.Sqrt(rectSize.GetArea() / textArea);
        float fontSize = FontSizeRange.Clamp(BaseSize * sizeF);
        sizeF = fontSize / BaseSize;
        lineSpacing *= sizeF;
        fontSpacing *= sizeF;

        return new(Font, fontSize, fontSpacing, lineSpacing, ColorRgba);
    }
    
    public Vector2 GetTextBaseSize(string text) => GetTextSize(text, BaseSize);
    
    
    public Vector2 GetTextSize(string text) => GetTextSize(text, FontSize);
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

    
    public Vector2 GetTextSizeLineBreak(string text) => GetTextSizeLineBreak(text, FontSize);
    public Vector2 GetTextSizeLineBreak(string text, float fontSize)
    {
        float curWidth = 0f;
        float totalWidth = 0f;
        float totalHeight = fontSize;
        
        for (int i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (c == '\n')
            {
                if (curWidth > totalWidth) totalWidth = curWidth;
                curWidth = 0f;
                totalHeight += fontSize + LineSpacing;
                continue;
            }
            float w = GetCharSize(c, fontSize).X;
            totalWidth += w;
        }
        float fontSpacingWidth = (text.Length - 1) * FontSpacing;
        totalWidth += fontSpacingWidth;
        return new Vector2(totalWidth, totalHeight);
    }
    
    public Vector2 GetCharSize(char c)
    {
        var baseSize = GetCharBaseSize(c);
        float f = FontSize / (float)BaseSize;
        return baseSize * f;
    }
    public Vector2 GetCharSize(char c, float fontSize)
    {
        var baseSize = GetCharBaseSize(c);
        float f = fontSize / (float)BaseSize;
        return baseSize * f;
    }
    public readonly Vector2 GetCharBaseSize(char c)
    {
        unsafe
        {
            int index = Raylib.GetGlyphIndex(Font, c);
            float glyphWidth = (Font.Glyphs[index].AdvanceX == 0) ? Font.Recs[index].Width : Font.Glyphs[index].AdvanceX;
            return new Vector2(glyphWidth, Font.BaseSize);
        }
    }


    #endregion

    #region Draw

    public void Draw(char c, Rect rect, Vector2 alignement)
    {
        if(Math.Abs(FontSizeModifier - 1f) > 0.0001f) rect = rect.ScaleSize(FontSizeModifier, alignement);
        float f = rect.Size.Y / BaseSize;
        float fontSize = BaseSize * f;
        var charSize = GetCharSize(c, fontSize);
        
        var uiPos = rect.GetPoint(alignement);
        var charRect = new Rect(uiPos, charSize, alignement);
        
        Raylib.DrawTextCodepoint(Font, c, charRect.TopLeft, fontSize, ColorRgba.ToRayColor());
    }
    public void Draw(string text, Rect rect, float rotDeg, Vector2 alignement)
    {
        if(Math.Abs(FontSizeModifier - 1f) > 0.0001f) rect = rect.ScaleSize(FontSizeModifier, alignement);
        var scaledFont = ScaleDynamic(text, rect.Size);
        var textSize = scaledFont.GetTextSize(text);
        Rect r = new(rect.GetPoint(alignement), textSize, alignement);
        var originOffset = alignement * textSize;
        Raylib.DrawTextPro(scaledFont.Font, text, r.TopLeft + originOffset, originOffset, rotDeg, scaledFont.FontSize, scaledFont.FontSpacing, scaledFont.ColorRgba.ToRayColor());
    }
    public void DrawWord(string word, Vector2 topLeft) => Raylib.DrawTextEx(Font, word, topLeft, FontSize, FontSpacing, ColorRgba.ToRayColor());
    public void DrawWord(string word, Vector2 topLeft, Vector2 alignement)
    {
        var size = GetTextSize(word);
        Rect r = new(topLeft, size, alignement);
        Raylib.DrawTextEx(Font, word, r.TopLeft, FontSize, FontSpacing, ColorRgba.ToRayColor());
    }
    public void DrawWord(string word, Vector2 topLeft, Vector2 alignement, Caret caret)
    {
        DrawWord(word, topLeft, alignement);
        
        if (caret.IsValid)
        {
            string caretText = word.Substring(0, caret.Index);
            var caretTextSize =  GetTextSize(caretText);
            Rect r = new(topLeft, caretTextSize, alignement);
        
            var caretTop = r.TopLeft + new Vector2(caretTextSize.X + FontSpacing * 0.5f, 0f);
            caret.Draw(caretTop, FontSize);
        }
    }
    public void DrawWord(string word, Rect rect, Vector2 alignement)
    {
        if(Math.Abs(FontSizeModifier - 1f) > 0.0001f) rect = rect.ScaleSize(FontSizeModifier, alignement);
        var scaledFont = ScaleDynamic(word, rect.Size);
        var textSize = scaledFont.GetTextSize(word);
        Rect r = new(rect.GetPoint(alignement), textSize, alignement);
        scaledFont.DrawWord(word, r.TopLeft);
    }
    public void DrawWord(string word, Rect rect, Vector2 alignement, Emphasis emphasis)
    {
        if(Math.Abs(FontSizeModifier - 1f) > 0.0001f) rect = rect.ScaleSize(FontSizeModifier, alignement);
        var scaledFont = ScaleDynamic(word, rect.Size);
        var textSize = scaledFont.GetTextSize(word);
        Rect r = new(rect.GetPoint(alignement), textSize, alignement);
        
        var emphasisRect = EmphasisRectMargins.Apply(r);
        
        emphasis.DrawForeground(emphasisRect);
        scaledFont.DrawWord(word, r.TopLeft, emphasis.TextColorRgba); //DrawTextEx(textFont.Font, word, r.TopLeft, info.fontSize, info.fontSpacing, emphasis.TextColor);
        emphasis.DrawBackground(emphasisRect);
        
    }

    
    
    public void DrawTextWrapNone(string text, Rect rect, Vector2 alignement)
    {
        var scaledFont = ScaleDynamic(text, rect.Size);
        var textSize = scaledFont.GetTextSize(text);
        Rect r = new(rect.GetPoint(alignement), textSize, alignement);
        scaledFont.DrawWord(text, r.TopLeft);
        //DrawTextEx(textFont.Font, text, r.TopLeft, info.fontSize, info.fontSpacing, textFont.Color);
    }
    public void DrawTextWrapNone(string text, Rect rect, Vector2 alignement, ColorRgba colorRgba)
    {
        var scaledFont = ScaleDynamic(text, rect.Size);
        var textSize = scaledFont.GetTextSize(text);
        Rect r = new(rect.GetPoint(alignement), textSize, alignement);
        scaledFont.DrawWord(text, r.TopLeft, colorRgba);
    }

    public void DrawTextWrapNone(string text, Rect rect, Vector2 alignement, Caret caret)
    {
        var scaledFont = ScaleDynamic(text, rect.Size);
        var textSize = scaledFont.GetTextSize(text);
        Rect r = new(rect.GetPoint(alignement), textSize, alignement);
        scaledFont.DrawWord(text, r.TopLeft);
        // DrawTextEx(textFont.Font, text, r.TopLeft, info.fontSize, info.fontSpacing, textFont.Color);

        if (caret.IsValid)
        {
            string caretText = text.Substring(0, caret.Index);
            var caretTextSize =  scaledFont.GetTextSize(caretText);
        
            var caretTop = r.TopLeft + new Vector2(caretTextSize.X + scaledFont.FontSpacing * 0.5f, 0f);
            caret.Draw(caretTop, scaledFont.FontSize);
        }
    }
    public void DrawTextWrapNone(string text, Rect rect, Vector2 alignement, Caret caret, List<TextEmphasis>? emphases)
    {
        if(Math.Abs(FontSizeModifier - 1f) > 0.0001f) rect = rect.ScaleSize(FontSizeModifier, alignement);
        
        var scaledFont = ScaleDynamic(text, rect.Size);
        var textSize = scaledFont.GetTextSize(text);
        var uiPos = rect.GetPoint(alignement);
        var topLeft = uiPos - alignement * textSize;
        
        var curWordPos = topLeft;
        var curWord = string.Empty;
        var curWordWidth = 0f;

        var caretTop = new Vector2();
        var caretFound = false;
        
        for (var i = 0; i < text.Length; i++)
        {
            if (!caretFound && caret.IsValid && i == caret.Index)
            {
                caretFound = true;
                var pos = curWordPos + new Vector2(curWordWidth + scaledFont.FontSpacing / 2, 0f);
                caretTop = pos;
            }
            
            var c = text[i];
            float w = scaledFont.GetCharSize(c).X; // GetCharSize(c, info.fontSize, textFont.Font).X;// + info.fontSpacing;
            curWordWidth += w;
            curWordWidth += scaledFont.FontSpacing;
            
            if (c == ' ')
            {
                var wordEmphasis = GetEmphasis(curWord, emphases);

                if (wordEmphasis != null) scaledFont.DrawWord(curWord, curWordPos, curWordWidth - w, wordEmphasis); //DrawWord(curWord, info.fontSize, info.fontSpacing, curWordPos, curWordWidth - w, wordEmphasis, textFont.Font);
                else scaledFont.DrawWord(curWord, curWordPos); //DrawWord(curWord, info.fontSize, info.fontSpacing, curWordPos, textFont.Color, textFont.Font);
                
                curWord = string.Empty;
                curWordPos += new Vector2(curWordWidth, 0f);
                curWordWidth = 0f;
            }
            else curWord += c;

            
        }
        
        var lastWordEmphasis = GetEmphasis(curWord, emphases);
        if (lastWordEmphasis != null) scaledFont.DrawWord(curWord, curWordPos, curWordWidth, lastWordEmphasis);
        else scaledFont.DrawWord(curWord, curWordPos);

        if (caretFound)
        {
            caret.Draw(caretTop, scaledFont.FontSize);
        }
        else
        {
            if (caret.IsValid)
            {
                var pos = curWordPos + new Vector2(curWordWidth + scaledFont.FontSpacing / 2, 0f);
                caret.Draw(pos, scaledFont.FontSize);
            }
        }
    }
    
    
    public void DrawTextWrapChar(string text, Rect rect, Vector2 alignement)
    {
        DrawTextWrapChar(text, rect, alignement, new(), null);
    }
    public void DrawTextWrapChar(string text, Rect rect, Vector2 alignement, Caret caret)
    {
        DrawTextWrapChar(text, rect, alignement, caret, null);
    }
    public void DrawTextWrapChar(string text, Rect rect, Vector2 alignement, Caret caret, List<TextEmphasis>? emphases)
    {
        if(Math.Abs(FontSizeModifier - 1f) > 0.0001f) rect = rect.ScaleSize(FontSizeModifier, alignement);
        // var rectSize = rect.Size;
        
        var textSize = GetTextBaseSize(text);

        if (textSize.X < rect.Size.X)//no wrapping needed
        {
            DrawTextWrapNone(text, rect, alignement, caret, emphases);
        }
        else
        {
            var scaledFont = ScaleDynamicWrapMode(text, rect.Size, 1.25f);
            // var fontSpacing = FontSpacing;
            // var lineSpacing = LineSpacing;
            // var lines = (int)MathF.Ceiling((textSize.X * 1.25f) / rect.Size.X);
            // var textHeight = lines * textFont.BaseSize;
            // var lineSpacingHeight = lines <= 0 ? 0 : (lines - 1) * lineSpacing;
            // var height = textHeight + lineSpacingHeight;
            //
            // var textArea = rect.Size.X * height;
            // var sizeF = MathF.Sqrt(rect.Size.GetArea() / textArea);
            // float fontSize = FontSizeRange.Clamp(textFont.BaseSize * sizeF);
            // sizeF = fontSize / textFont.BaseSize;
            // lineSpacing *= sizeF;
            // fontSpacing *= sizeF;
            var sizeF = scaledFont.FontSize / BaseSize;
            var pos = rect.TopLeft;
            var curWord = string.Empty;
            var lineBreakInProcess = false;
            Emphasis? lineBreakEmphasis = null;
            var curWordWidth = 0f;
            var curLineWidth = 0f;

            var caretTop = new Vector2();
            var caretFound = false;
            
            for (int i = 0; i < text.Length; i++)
            {
                if (!caretFound && caret.IsValid && i == caret.Index)
                {
                    caretFound = true;
                    caretTop = pos + new Vector2(curWordWidth + scaledFont.FontSpacing / 2, 0f);
                }
                
                var c = text[i];
                if (c == '\n') continue;
            
                var charBaseSize = scaledFont.GetCharBaseSize(c);
                float glyphWidth = charBaseSize.X * sizeF;
            
                if (curLineWidth + curWordWidth + glyphWidth >= rect.Size.X && curLineWidth > 0)//break line
                {
                    if (c == ' ') 
                    {
                        var emphasis = GetEmphasis(curWord, emphases);
                        if(emphasis != null) scaledFont.DrawWord(curWord, pos, curWordWidth, emphasis);
                        else  scaledFont.DrawWord(curWord, pos);
                        
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
                                    lineBreakEmphasis = GetEmphasis(completeWord, emphases);
                                }
                                else
                                {
                                    if (nextChar == ' ')
                                    {
                                        lineBreakEmphasis = GetEmphasis(completeWord, emphases);
                                        break;
                                    }
                                    
                                    completeWord += nextChar;
                                }
                                
                            }
                        }
                        
                        if(lineBreakEmphasis != null) scaledFont.DrawWord(curWord, pos, curWordWidth, lineBreakEmphasis);
                        else  scaledFont.DrawWord(curWord, pos);
                    } 
                        
                    
                    curWord = string.Empty;
                    if(c != ' ') curWord += c;
                    pos.Y += scaledFont.FontSize + scaledFont.LineSpacing;
                    pos.X = rect.TopLeft.X;
                    curLineWidth = 0f;
                    curWordWidth = glyphWidth;// 0f;
                    
                    continue;
                }
            
                curWordWidth += glyphWidth + scaledFont.FontSpacing;
                if (c == ' ')
                {
                    if (lineBreakInProcess)
                    {
                        lineBreakInProcess = false;
                        if (lineBreakEmphasis != null) scaledFont.DrawWord(curWord, pos, curWordWidth - glyphWidth, lineBreakEmphasis);
                        else scaledFont.DrawWord(curWord, pos);
                    }
                    else
                    {
                        var wordEmphasis = GetEmphasis(curWord, emphases);
                        if (wordEmphasis != null) scaledFont.DrawWord(curWord, pos, curWordWidth - glyphWidth, wordEmphasis);
                        else scaledFont.DrawWord(curWord, pos);
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
                if (lineBreakEmphasis != null) scaledFont.DrawWord(curWord, pos, curWordWidth, lineBreakEmphasis);
                else scaledFont.DrawWord(curWord, pos);
            }
            else
            {
                var wordEmphasis = GetEmphasis(curWord, emphases);
                if (wordEmphasis != null) scaledFont.DrawWord(curWord, pos, curWordWidth, wordEmphasis);
                else scaledFont.DrawWord(curWord, pos);
            }
            
            
            if (caretFound)
            {
                caret.Draw(caretTop, scaledFont.FontSize);
            }
            else
            {
                if (caret.IsValid)
                {
                    var topLeft = pos + new Vector2(curWordWidth + scaledFont.FontSpacing / 2, 0f);
                    caret.Draw(topLeft, scaledFont.FontSize);
                }
            }
        }
    }
    
    
    public void DrawTextWrapWord(string text, Rect rect, Vector2 alignement)
    {
        DrawTextWrapWord(text, rect, alignement, new(), null);
    }
    public void DrawTextWrapWord(string text, Rect rect, Vector2 alignement, Caret caret)
    {
        DrawTextWrapWord(text, rect, alignement, new(), null);
    }
    public void DrawTextWrapWord(string text, Rect rect, Vector2 alignement, Caret caret, List<TextEmphasis>? emphases)
    {
        if(Math.Abs(FontSizeModifier - 1f) > 0.0001f) rect = rect.ScaleSize(FontSizeModifier, alignement);
        // var rectSize = rect.Size;
        var textSize = GetTextBaseSize(text);

        if (textSize.X < rect.Size.X)
        {
            DrawTextWrapNone(text, rect, alignement, caret, emphases);
        }
        else
        {
            // var fontSpacing = textFont.FontSpacing;
            // var lineSpacing = textFont.LineSpacing;
            // var lines = (int)MathF.Ceiling((textSize.X * 1.5f) / rect.Size.X);
            // var textHeight = lines * textFont.BaseSize;
            // var lineSpacingHeight = lines <= 0 ? 0 : (lines - 1) * lineSpacing;
            // var height = textHeight + lineSpacingHeight;
            //
            // var textArea = rect.Size.X * height;
            // var sizeF = MathF.Sqrt(rect.Size.GetArea() / textArea);
            // float fontSize = FontSizeRange.Clamp(textFont.BaseSize * sizeF);
            // sizeF = fontSize / textFont.BaseSize;
            // lineSpacing *= sizeF;
            // fontSpacing *= sizeF;
            var scaledFont = ScaleDynamicWrapMode(text, rect.Size, 1.5f);
            var sizeF = scaledFont.FontSize / BaseSize;
            var pos = rect.TopLeft;
        
            var curWord = string.Empty;
            var curWordWidth = 0f;
            var curLineWidth = 0f;
            
            var caretTop = new Vector2();
            var caretFound = false;
            var caretWordOffset = -1f;
            
            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (c == '\n') continue;
            
                var charBaseSize = scaledFont.GetCharBaseSize(c);
                float glyphWidth = charBaseSize.X * sizeF;
                
                if (curLineWidth + curWordWidth + glyphWidth >= rect.Width && curLineWidth > 0)//break line
                {
                    // if (curLineWidth <= 0) return;
                    pos.Y += scaledFont.FontSize + scaledFont.LineSpacing;
                    pos.X = rect.TopLeft.X;
                    curLineWidth = 0f;
                }
            
                curWordWidth += glyphWidth + scaledFont.FontSpacing;
                
                if (i == caret.Index - 1 && caret.IsValid)
                {
                    caretWordOffset = curWordWidth;
                }
                if (c == ' ')
                {
                    var wordEmphasis = GetEmphasis(curWord, emphases);

                    if (wordEmphasis != null) scaledFont.DrawWord(curWord, pos, curWordWidth - glyphWidth, wordEmphasis);
                    else scaledFont.DrawWord(curWord, pos);
            
                    if (caretWordOffset >= 0)
                    {
                        caretFound = true;
                        caretTop = pos + new Vector2(caretWordOffset + scaledFont.FontSpacing / 2, 0f);
                        caretWordOffset = -1;
                    }
                    
                    curWord = string.Empty;
                    curLineWidth += curWordWidth;
                    pos.X = rect.TopLeft.X + curLineWidth; // curWordWidth;
                    curWordWidth = 0f;
                }
                else  curWord += c;
            }
            
            //draw last word
            var lastWordEmphasis = GetEmphasis(curWord, emphases);

            if (lastWordEmphasis != null) scaledFont.DrawWord(curWord, pos, curWordWidth, lastWordEmphasis);
            else scaledFont.DrawWord(curWord, pos);
            
            if (caretFound)
            {
                caret.Draw(caretTop, scaledFont.FontSize);
            }
            else if (caretWordOffset >= 0)
            {
                caretTop = pos + new Vector2(caretWordOffset + scaledFont.FontSpacing / 2, 0f);
                caret.Draw(caretTop, scaledFont.FontSize);
            }
            else
            {
                if (caret.IsValid)
                {
                    caret.Draw(rect.TopLeft, scaledFont.FontSize);
                }
            }
        }
    }
    
    
    private void DrawWord(string word, Vector2 topLeft, float width, Emphasis emphasis)
    {
        Rect r = new(topLeft, new Vector2(width, FontSize), new());
        
        var emphasisRect = EmphasisRectMargins.Apply(r);
        
        emphasis.DrawBackground(emphasisRect);
        DrawWord(word, topLeft, emphasis.TextColorRgba);
        // DrawTextEx(font, word, r.TopLeft, fontSize, fontSpacing, emphasis.TextColor);
        emphasis.DrawForeground(emphasisRect);
        
    }
    private void DrawWord(string word, Vector2 topLeft, ColorRgba colorRgba) => Raylib.DrawTextEx(Font, word, topLeft, FontSize, FontSpacing, colorRgba.ToRayColor());

    #endregion
    
    public void SetFilter(TextureFilter textureFilter = TextureFilter.Bilinear)
    {
        Raylib.SetTextureFilter(Font.Texture, textureFilter);
    }
    public static Emphasis? GetEmphasis(string word, List<TextEmphasis>? emphases)
    {
        if (emphases == null || emphases.Count <= 0) return null;
        
        foreach (var e in emphases)
        {
            if (e.HasKeyword(word)) return e.Emphasis;
        }

        return null;
    }

}


/*
public class FontHandler
       {
           private Dictionary<uint, Font> fonts = new();
           private Font defaultFont = GetFontDefault();
           /*
           //private Dictionary<string, float> fontSizes = new();
   
           public void AddFontSize(string name, float size)
           {
               if (fontSizes.ContainsKey(name)) fontSizes[name] = size;
               else fontSizes.Add(name, size);
           }
           public void RemoveFontSize(string name)
           {
               fontSizes.Remove(name);
           }
           public float GetFontSize(string name)
           {
               if (!fontSizes.ContainsKey(name)) return -1;
               else return fontSizes[name];
           }
           * /
           public void SetFontFilter(uint id, TextureFilter textureFilter = TextureFilter.TEXTURE_FILTER_BILINEAR)
           {
               Font font = GetFont(id);
               SetTextureFilter(font.texture, textureFilter);
           }
           //public void AddFont(uint id, string fileName, int fontSize = 100, TextureFilter textureFilter = TextureFilter.TEXTURE_FILTER_BILINEAR)
           //{
           //    if (fileName == "" || fonts.ContainsKey(id)) return;
           //    Font font = ResourceManager.LoadFontFromRaylib(fileName, fontSize);
           //
           //    SetTextureFilter(font.texture, textureFilter);
           //    fonts.Add(id, font);
           //}
           public void AddFont(uint id, Font font, TextureFilter textureFilter = TextureFilter.TEXTURE_FILTER_BILINEAR)
           {
               if (fonts.ContainsKey(id)) return;
               SetTextureFilter(font.texture, textureFilter);
               fonts.Add(id, font);
           }
           public Font GetFont(uint id)
           {
               if (!fonts.ContainsKey(id)) return defaultFont;
               return fonts[id];
           }
           public void SetDefaultFont(uint id)
           {
               if (!fonts.ContainsKey(id)) return;
               defaultFont = fonts[id];
           }
           public void Close()
           {
               foreach (Font font in fonts.Values)
               {
                   UnloadFont(font);
               }
               fonts.Clear();
           }
           
           /*
           public (float fontSize, float fontSpacing) GetDynamicFontInfo(uint fontID, string text, Vector2 size, float fontSpacing)
           {
               return GetDynamicFontInfo(GetFont(fontID), text, size, fontSpacing);
           }
           public Vector2 GetTextSize(string text, float fontSize, float fontSpacing, uint fontID)
           {
               return MeasureTextEx(GetFont(fontID), text, fontSize, fontSpacing);
           }
           * /
           /*
           public float CalculateDynamicFontSize(string text, Vector2 size, uint fontID, float fontSpacing = 1f)
           {
               float baseSize = GetFont(fontID).baseSize;
               return GetFontScalingFactor(text, size, fontID, fontSpacing) * baseSize;
           }
           public float GetFontScalingFactor(string text, Vector2 size, uint fontID, float fontSpacing = 1)
           {
               float baseSize = GetFont(fontID).baseSize;
               float scalingFactor = size.Y / baseSize;
               Vector2 textSize = MeasureTextEx(GetFont(fontID), text, baseSize * scalingFactor, fontSpacing);
               float correctionFactor = MathF.Min(size.X / textSize.X, 1f);
               return scalingFactor * correctionFactor;
           }
           public static float CalculateDynamicFontSize(string text, Vector2 size, Font font, float fontSpacing = 1f)
           {
               float baseSize = font.baseSize;
   
               return GetFontScalingFactor(text, size, font, fontSpacing) * baseSize;
           }
           public static float GetFontScalingFactor(string text, Vector2 size, Font font, float fontSpacing = 1)
           {
               float baseSize = font.baseSize;
               float scalingFactor = size.Y / baseSize;
               Vector2 textSize = MeasureTextEx(font, text, baseSize * scalingFactor, fontSpacing);
               float correctionFactor = MathF.Min(size.X / textSize.X, 1f);
               return scalingFactor * correctionFactor;
           }
           * /
           /*
           public float CalculateDynamicFontSize(float height, uint fontID)
           {
               return CalculateDynamicFontSize(height, GetFont(fontID));
           }
           public float CalculateDynamicFontSize(string text, float width, uint fontID, float fontSpacing = 1f)
           {
               return CalculateDynamicFontSize(text, width, GetFont(fontID), fontSpacing);
           }
           public float GetFontScalingFactor(float height, uint fontID) { return GetFontScalingFactor(height, GetFont(fontID)); }
           public float GetFontScalingFactor(string text, float width, uint fontID, float fontSpacing = 1)
           {
               float baseSize = GetFont(fontID).baseSize;
               Vector2 textSize = MeasureTextEx(GetFont(fontID), text, baseSize, fontSpacing);
               float scalingFactor = width / textSize.X;
               return scalingFactor;
           }
   
           public static float CalculateDynamicFontSize(float height, Font font)
           {
               float baseSize = font.baseSize;
   
               return GetFontScalingFactor(height, font) * baseSize;
           }
           public static float CalculateDynamicFontSize(string text, float width, Font font, float fontSpacing = 1f)
           {
               float baseSize = font.baseSize;
   
               return GetFontScalingFactor(text, width, font, fontSpacing) * baseSize;
           }
           public static float GetFontScalingFactor(float height, Font font)
           {
               float baseSize = font.baseSize;
               return height / baseSize;
           }
           public static float GetFontScalingFactor(string text, float width, Font font, float fontSpacing = 1)
           {
               float baseSize = font.baseSize;
               Vector2 textSize = MeasureTextEx(font, text, baseSize, fontSpacing);
               float scalingFactor = width / textSize.X;
               return scalingFactor;
           }
           * /
       }
   
   }
*/