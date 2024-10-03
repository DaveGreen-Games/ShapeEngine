using Raylib_cs;
using ShapeEngine.Core;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Text;

public readonly struct FontDimensions
{
    public static ValueRange FontSizeRange = new(10, 150);

    public readonly Font Font;
    public float BaseSize => Font.BaseSize;
    public readonly float Size;
    public readonly float Spacing;
    public readonly float LineSpacing;
    
    
    public FontDimensions(Font font)
    {
        Font = font;
        Size = font.BaseSize;
        Spacing = 0f;
        LineSpacing = 0f;
    }
    public FontDimensions(Font font, float size)
    {
        Font = font;
        Size = size;
        Spacing = 0f;
        LineSpacing = 0f;
    }
    public FontDimensions(Font font, float size, float spacing)
    {
        Font = font;
        Size = size;
        Spacing = spacing;
        LineSpacing = 0f;
    }
    public FontDimensions(Font font, float size, float spacing, float lineSpacing)
    {
        Font = font;
        Size = size;
        Spacing = spacing;
        LineSpacing = lineSpacing;
    }

    
    public FontDimensions SetFont(Font newFont) => new(newFont, Size, Spacing, LineSpacing);
    public FontDimensions SetFontSize(float newFontSize) => new(Font, newFontSize, Spacing, LineSpacing);
    public FontDimensions ChangeFontSize(float amount) => new(Font, Size + amount, Spacing, LineSpacing);
    public FontDimensions ScaleFontSize(float factor) => new(Font, Size * factor, Spacing, LineSpacing);
    public FontDimensions SetSpacing(float newSpacing) => new(Font, Size, newSpacing, LineSpacing);
    public FontDimensions ChangeSpacing(float amount) => new(Font, Size, Spacing + amount, LineSpacing);
    public FontDimensions ScaleSpacing(float factor) => new(Font, Size, Spacing * factor, LineSpacing);
    public FontDimensions SetLineSpacing(float newLineSpacing) => new(Font, Size, Spacing, newLineSpacing);
    public FontDimensions ChangeLineSpacing(float amount) => new(Font, Size, Spacing, LineSpacing + amount);
    public FontDimensions ScaleLineSpacing(float factor) => new(Font, Size, Spacing, LineSpacing * factor);
    

    #region Size
    public FontDimensions ScaleDynamic(string text, Size rectSize)
    {
        var fontDimensions = GetTextBaseSize(text);// MeasureTextEx(Font, text, BaseSize, FontSpacing);
        float fX = rectSize.Width / fontDimensions.Width;
        float fY = rectSize.Height / fontDimensions.Height;
        float f = MathF.Min(fX, fY);

        float scaledFontSize = FontSizeRange.Clamp(BaseSize * f);
        f = scaledFontSize / BaseSize;

        var scaledDimensions = new FontDimensions(Font, scaledFontSize, Spacing * f, LineSpacing * f);

        return scaledDimensions;
    }
    public FontDimensions ScaleDynamicWrapMode(string text, Size rectSize, float widthFactor = 1.3f)
    {
        var fontSpacing = Spacing;
        var lineSpacing = LineSpacing;
        var textSize = GetTextBaseSize(text);
        var lines = (int)MathF.Ceiling((textSize.Width * widthFactor) / rectSize.Width);
        var textHeight = lines * BaseSize;
        var lineSpacingHeight = lines <= 0 ? 0 : (lines - 1) * lineSpacing;
        var height = textHeight + lineSpacingHeight;

        var textArea = rectSize.Width * height;
        var sizeF = MathF.Sqrt(rectSize.Area / textArea);
        float fontSize = FontSizeRange.Clamp(BaseSize * sizeF);
        sizeF = fontSize / BaseSize;
        lineSpacing *= sizeF;
        fontSpacing *= sizeF;

        return new(Font, fontSize, fontSpacing, lineSpacing);
    }
    
    public Size GetTextBaseSize(string text) => GetTextSize(text, BaseSize);
    
    public Size GetTextSize(string text) => GetTextSize(text, Size);
    public Size GetTextSize(string text, float fontSize)
    {
        float totalWidth = 0f;

        for (int i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (c == '\n') continue;
            float w = GetCharSize(c, fontSize).Width;
            totalWidth += w;
        }
        float fontSpacingWidth = (text.Length - 1) * Spacing;
        totalWidth += fontSpacingWidth;
        return new Size(totalWidth, fontSize);
    }
    
    public Size GetTextSizeLineBreak(string text) => GetTextSizeLineBreak(text, Size);
    public Size GetTextSizeLineBreak(string text, float fontSize)
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
            float w = GetCharSize(c, fontSize).Width;
            totalWidth += w;
        }
        float fontSpacingWidth = (text.Length - 1) * Spacing;
        totalWidth += fontSpacingWidth;
        return new Size(totalWidth, totalHeight);
    }
    
    public Size GetCharSize(char c)
    {
        var baseSize = GetCharBaseSize(c);
        float f = Size / (float)BaseSize;
        return baseSize * f;
    }
    public Size GetCharSize(char c, float fontSize)
    {
        var baseSize = GetCharBaseSize(c);
        float f = fontSize / (float)BaseSize;
        return baseSize * f;
    }
    public Size GetCharBaseSize(char c)
    {
        unsafe
        {
            int index = Raylib.GetGlyphIndex(Font, c);
            float glyphWidth = (Font.Glyphs[index].AdvanceX == 0) ? Font.Recs[index].Width : Font.Glyphs[index].AdvanceX;
            return new Size(glyphWidth, Font.BaseSize);
        }
    }
    #endregion
}