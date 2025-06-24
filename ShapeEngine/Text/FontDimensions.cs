using Raylib_cs;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Text;

/// <summary>
/// Represents font, size, spacing, and line spacing settings,
/// and provides text measurement and scaling utilities.
/// </summary>
/// <remarks>
/// Used for text rendering and layout calculations in ShapeEngine.
/// Supports dynamic scaling and measurement for both single-line and multi-line text.
/// </remarks>
public readonly struct FontDimensions
{
    /// <summary>
    /// The default allowed range for font sizes.
    /// </summary>
    public static readonly ValueRange FontSizeRangeDefault = new(10, 250);
    /// <summary>
    /// The current  allowed range for font sizes.
    /// </summary>
    public static ValueRange FontSizeRange = FontSizeRangeDefault;

    /// <summary>
    /// The Raylib font used for rendering.
    /// </summary>
    public readonly Font Font;
    /// <summary>
    /// The base size of the font (unscaled).
    /// </summary>
    public float BaseSize => Font.BaseSize;
    /// <summary>
    /// The current font size.
    /// </summary>
    public readonly float Size;
    /// <summary>
    /// The spacing between characters.
    /// </summary>
    public readonly float Spacing;
    /// <summary>
    /// The spacing between lines.
    /// </summary>
    public readonly float LineSpacing;
    
    
    /// <summary>
    /// Initializes a new instance of the <see cref="FontDimensions"/> struct with the specified font.
    /// Sets size to the font's base size, and spacing and line spacing to 0.
    /// </summary>
    /// <param name="font">The font to use.</param>
    public FontDimensions(Font font)
    {
        Font = font;
        Size = font.BaseSize;
        Spacing = 0f;
        LineSpacing = 0f;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FontDimensions"/> struct with the specified font and size.
    /// Sets spacing and line spacing to 0.
    /// </summary>
    /// <param name="font">The font to use.</param>
    /// <param name="size">The font size.</param>
    public FontDimensions(Font font, float size)
    {
        Font = font;
        Size = size;
        Spacing = 0f;
        LineSpacing = 0f;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FontDimensions"/> struct with the specified font, size, and spacing.
    /// Sets line spacing to 0.
    /// </summary>
    /// <param name="font">The font to use.</param>
    /// <param name="size">The font size.</param>
    /// <param name="spacing">The spacing between characters.</param>
    public FontDimensions(Font font, float size, float spacing)
    {
        Font = font;
        Size = size;
        Spacing = spacing;
        LineSpacing = 0f;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FontDimensions"/> struct with the specified font, size, spacing, and line spacing.
    /// </summary>
    /// <param name="font">The font to use.</param>
    /// <param name="size">The font size.</param>
    /// <param name="spacing">The spacing between characters.</param>
    /// <param name="lineSpacing">The spacing between lines.</param>
    public FontDimensions(Font font, float size, float spacing, float lineSpacing)
    {
        Font = font;
        Size = size;
        Spacing = spacing;
        LineSpacing = lineSpacing;
    }

    /// <summary>
    /// Returns a new <see cref="FontDimensions"/> instance with the specified font.
    /// </summary>
    /// <param name="newFont">The new font to use.</param>
    public FontDimensions SetFont(Font newFont) => new(newFont, Size, Spacing, LineSpacing);

    /// <summary>
    /// Returns a new <see cref="FontDimensions"/> instance with the specified font size.
    /// </summary>
    /// <param name="newFontSize">The new font size.</param>
    public FontDimensions SetFontSize(float newFontSize) => new(Font, newFontSize, Spacing, LineSpacing);

    /// <summary>
    /// Returns a new <see cref="FontDimensions"/> instance with the font size changed by the specified amount.
    /// </summary>
    /// <param name="amount">The amount to change the font size by.</param>
    public FontDimensions ChangeFontSize(float amount) => new(Font, Size + amount, Spacing, LineSpacing);

    /// <summary>
    /// Returns a new <see cref="FontDimensions"/> instance with the font size scaled by the specified factor.
    /// </summary>
    /// <param name="factor">The factor to scale the font size by.</param>
    public FontDimensions ScaleFontSize(float factor) => new(Font, Size * factor, Spacing, LineSpacing);

    /// <summary>
    /// Returns a new <see cref="FontDimensions"/> instance with the specified character spacing.
    /// </summary>
    /// <param name="newSpacing">The new character spacing.</param>
    public FontDimensions SetSpacing(float newSpacing) => new(Font, Size, newSpacing, LineSpacing);

    /// <summary>
    /// Returns a new <see cref="FontDimensions"/> instance with the character spacing changed by the specified amount.
    /// </summary>
    /// <param name="amount">The amount to change the character spacing by.</param>
    public FontDimensions ChangeSpacing(float amount) => new(Font, Size, Spacing + amount, LineSpacing);

    /// <summary>
    /// Returns a new <see cref="FontDimensions"/> instance with the character spacing scaled by the specified factor.
    /// </summary>
    /// <param name="factor">The factor to scale the character spacing by.</param>
    public FontDimensions ScaleSpacing(float factor) => new(Font, Size, Spacing * factor, LineSpacing);

    /// <summary>
    /// Returns a new <see cref="FontDimensions"/> instance with the specified line spacing.
    /// </summary>
    /// <param name="newLineSpacing">The new line spacing.</param>
    public FontDimensions SetLineSpacing(float newLineSpacing) => new(Font, Size, Spacing, newLineSpacing);

    /// <summary>
    /// Returns a new <see cref="FontDimensions"/> instance with the line spacing changed by the specified amount.
    /// </summary>
    /// <param name="amount">The amount to change the line spacing by.</param>
    public FontDimensions ChangeLineSpacing(float amount) => new(Font, Size, Spacing, LineSpacing + amount);

    /// <summary>
    /// Returns a new <see cref="FontDimensions"/> instance with the line spacing scaled by the specified factor.
    /// </summary>
    /// <param name="factor">The factor to scale the line spacing by.</param>
    public FontDimensions ScaleLineSpacing(float factor) => new(Font, Size, Spacing, LineSpacing * factor);
    

    #region Size
    /// <summary>
    /// Scales the font size dynamically based on the given text and rectangle size, maintaining the aspect ratio.
    /// </summary>
    /// <param name="text">The text to measure.</param>
    /// <param name="rectSize">The available size for the text.</param>
    /// <returns>A new <see cref="FontDimensions"/> instance with the scaled font size.</returns>
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
    /// <summary>
    /// Scales the font size for dynamic wrap mode based on the given text and rectangle size.
    /// </summary>
    /// <param name="text">The text to measure.</param>
    /// <param name="rectSize">The available size for the text.</param>
    /// <param name="widthFactor">An optional factor to adjust the width calculation (default is 1.3).</param>
    /// <returns>A new <see cref="FontDimensions"/> instance with the scaled font size and adjusted spacing.</returns>
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
    
    /// <summary>
    /// Measures the base size (width and height) of the given text using the current font settings.
    /// </summary>
    /// <param name="text">The text to measure.</param>
    /// <returns>A <see cref="Size"/> struct representing the width and height of the text.</returns>
    public Size GetTextBaseSize(string text) => GetTextSize(text, BaseSize);
    
    /// <summary>
    /// Measures the size of the given text using the current font size.
    /// </summary>
    /// <param name="text">The text to measure.</param>
    /// <returns>A <see cref="Size"/> struct representing the width and height of the text.</returns>
    public Size GetTextSize(string text) => GetTextSize(text, Size);
    /// <summary>
    /// Measures the size of the given text using the specified font size.
    /// </summary>
    /// <param name="text">The text to measure.</param>
    /// <param name="fontSize">The font size to use for measurement.</param>
    /// <returns>A <see cref="Size"/> struct representing the width and height of the text.</returns>
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
    
    /// <summary>
    /// Measures the size of the given text with line breaks using the current font size.
    /// </summary>
    /// <param name="text">The text to measure.</param>
    /// <returns>A <see cref="Size"/> struct representing the width and height of the text with line breaks.</returns>
    public Size GetTextSizeLineBreak(string text) => GetTextSizeLineBreak(text, Size);
    /// <summary>
    /// Measures the size of the given text with line breaks using the specified font size.
    /// </summary>
    /// <param name="text">The text to measure.</param>
    /// <param name="fontSize">The font size to use for measurement.</param>
    /// <returns>A <see cref="Size"/> struct representing the width and height of the text with line breaks.</returns>
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
    
    /// <summary>
    /// Gets the size (width and height) of the specified character at the current font size.
    /// </summary>
    /// <param name="c">The character to measure.</param>
    /// <returns>A <see cref="Size"/> struct representing the width and height of the character.</returns>
    public Size GetCharSize(char c)
    {
        var baseSize = GetCharBaseSize(c);
        float f = Size / (float)BaseSize;
        return baseSize * f;
    }
    /// <summary>
    /// Gets the size (width and height) of the specified character at the given font size.
    /// </summary>
    /// <param name="c">The character to measure.</param>
    /// <param name="fontSize">The font size to use for measurement.</param>
    /// <returns>A <see cref="Size"/> struct representing the width and height of the character.</returns>
    public Size GetCharSize(char c, float fontSize)
    {
        var baseSize = GetCharBaseSize(c);
        float f = fontSize / (float)BaseSize;
        return baseSize * f;
    }
    /// <summary>
    /// Gets the base size (width and height) of the specified character from the font's glyph data.
    /// </summary>
    /// <param name="c">The character to measure.</param>
    /// <returns>A <see cref="Size"/> struct representing the width and height of the character's glyph.</returns>
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