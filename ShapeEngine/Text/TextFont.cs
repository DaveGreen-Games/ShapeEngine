using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Text;

/// <summary>
/// Provides font and color settings,
/// as well as text rendering and measurement utilities for drawing text in the ShapeEngine.
/// </summary>
/// <remarks>
/// Supports drawing text, words, and characters with various alignments, emphasis, caret, and wrapping options. Also provides font scaling and measurement utilities.
/// </remarks>
public class TextFont
{
    #region Static Members

    /// <summary>
    /// Margins to apply to emphasis rectangles when drawing emphasized words.
    /// </summary>
    public static Rect.Margins EmphasisRectMargins = new();
    
    private static float fontSizeModifier = 1f;
    
    /// <summary>
    /// Gets or sets the global font size modifier for scaling text rendering.
    /// </summary>
    /// <remarks>Minimum value is 0.1.</remarks>
    public static float FontSizeModifier
    {
        get => fontSizeModifier;
        set => fontSizeModifier = MathF.Max(value, 0.1f);
    }

    #endregion

    #region Members
    /// <summary>
    /// The font, size, spacing, and line spacing settings for this text font.
    /// </summary>
    public FontDimensions FontDimensions;
    /// <summary>
    /// The color used for rendering text.
    /// </summary>
    public ColorRgba ColorRgba;
    /// <summary>
    /// Used in wrap char/word draw functions to determine mouse-based emphasis.
    /// </summary>
    public IMouseDetection? MouseDetection;
    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="TextFont"/> class with the specified font dimensions and default color (black).
    /// </summary>
    /// <param name="fontDimensions">The font, size, spacing, and line spacing settings.</param>
    public TextFont(FontDimensions fontDimensions)
    {
        FontDimensions = fontDimensions;
        ColorRgba = new ColorRgba(System.Drawing.Color.Black);
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="TextFont"/> class with the specified font dimensions and color.
    /// </summary>
    /// <param name="fontDimensions">The font, size, spacing, and line spacing settings.</param>
    /// <param name="colorRgba">The color to use for text rendering.</param>
    public TextFont(FontDimensions fontDimensions, ColorRgba colorRgba)
    {
        FontDimensions = fontDimensions;
        ColorRgba = colorRgba;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="TextFont"/> class with the specified font dimensions, color, and mouse detection handler.
    /// </summary>
    /// <param name="fontDimensions">The font, size, spacing, and line spacing settings.</param>
    /// <param name="colorRgba">The color to use for text rendering.</param>
    /// <param name="mouseDetection">The mouse detection handler for emphasis interaction.</param>
    public TextFont(FontDimensions fontDimensions, ColorRgba colorRgba, IMouseDetection mouseDetection)
    {
        FontDimensions = fontDimensions;
        ColorRgba = colorRgba;
        MouseDetection = mouseDetection;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="TextFont"/> class from a Raylib font, spacing, and color.
    /// </summary>
    /// <param name="font">The Raylib font.</param>
    /// <param name="fontSpacing">The spacing between characters.</param>
    /// <param name="colorRgba">The color to use for text rendering.</param>
    public TextFont(Font font, float fontSpacing, ColorRgba colorRgba)
    {
        FontDimensions = new(font, font.BaseSize, fontSpacing, 0f);
        ColorRgba = colorRgba;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="TextFont"/> class from a Raylib font, size, spacing, line spacing, and color.
    /// </summary>
    /// <param name="font">The Raylib font.</param>
    /// <param name="fontSize">The font size.</param>
    /// <param name="fontSpacing">The spacing between characters.</param>
    /// <param name="lineSpacing">The spacing between lines.</param>
    /// <param name="colorRgba">The color to use for text rendering.</param>
    public TextFont(Font font, float fontSize, float fontSpacing, float lineSpacing, ColorRgba colorRgba)
    {
        FontDimensions = new(font, fontSize, fontSpacing, lineSpacing);
        ColorRgba = colorRgba;
    }

    #endregion

    #region Getters & Setters

    /// <summary>
    /// Gets or sets the Raylib font.
    /// </summary>
    public Font Font
    {
        get => FontDimensions.Font;
        set => FontDimensions = FontDimensions.SetFont(value);
    }
    /// <summary>
    /// Gets the base font size (unscaled).
    /// </summary>
    public float BaseSize => FontDimensions.BaseSize;
    /// <summary>
    /// Gets or sets the font size.
    /// </summary>
    public float FontSize
    {
        get => FontDimensions.Size;
        set => FontDimensions = FontDimensions.SetFontSize(value);
    }
    /// <summary>
    /// Gets or sets the spacing between characters.
    /// </summary>
    public float FontSpacing
    {
        get => FontDimensions.Spacing;
        set => FontDimensions = FontDimensions.SetSpacing(value);
    }
    /// <summary>
    /// Gets or sets the spacing between lines.
    /// </summary>
    public float LineSpacing
    {
        get => FontDimensions.LineSpacing;
        set => FontDimensions = FontDimensions.SetLineSpacing(value);
    }

    #endregion
    
    #region Draw

    /// <summary>
    /// Draws a single character at the specified rectangle and alignment.
    /// </summary>
    /// <param name="c">The character to draw.</param>
    /// <param name="rect">The rectangle defining the position and size.</param>
    /// <param name="alignement">The alignment of the character within the rectangle.</param>
    public void Draw(char c, Rect rect, AnchorPoint alignement)
    {
        if(Math.Abs(FontSizeModifier - 1f) > 0.0001f) rect = rect.ScaleSize(FontSizeModifier, alignement);
        float f = rect.Size.Height / BaseSize;
        float fontSize = BaseSize * f;
        var charSize = FontDimensions.GetCharSize(c, fontSize);
        
        var uiPos = rect.GetPoint(alignement);
        var charRect = new Rect(uiPos, charSize, alignement);
        
        Raylib.DrawTextCodepoint(Font, c, charRect.TopLeft, fontSize, ColorRgba.ToRayColor());
    }
    /// <summary>
    /// Draws a string of text within a rectangle, rotating and aligning as specified.
    /// </summary>
    /// <remarks>
    /// The text is scaled and position according to the specified rect first and then rotated.
    /// </remarks>
    /// <param name="text">The text to draw.</param>
    /// <param name="rect">The rectangle defining the position and size.</param>
    /// <param name="rotDeg">The rotation angle in degrees.</param>
    /// <param name="alignement">The alignment of the text within the rectangle.</param>
    public void Draw(string text, Rect rect, float rotDeg, AnchorPoint alignement)
    {
        if(Math.Abs(FontSizeModifier - 1f) > 0.0001f) rect = rect.ScaleSize(FontSizeModifier, alignement);
        var scaledFont = FontDimensions.ScaleDynamic(text, rect.Size);
        var textSize = scaledFont.GetTextSize(text);
        Rect r = new(rect.GetPoint(alignement), textSize, alignement);
        var originOffset = (alignement * textSize).ToVector2();
        Raylib.DrawTextPro(scaledFont.Font, text, r.TopLeft + originOffset, originOffset, rotDeg, scaledFont.Size, scaledFont.Spacing, ColorRgba.ToRayColor());
    }
    /// <summary>
    /// Draws a word at the specified top-left position.
    /// </summary>
    /// <param name="word">The word to draw.</param>
    /// <param name="topLeft">The top-left position for the word.</param>
    public void DrawWord(string word, Vector2 topLeft) => Raylib.DrawTextEx(Font, word, topLeft, FontSize, FontSpacing, ColorRgba.ToRayColor());
    /// <summary>
    /// Draws a word with specified font dimensions at the given top-left position.
    /// </summary>
    /// <param name="word">The word to draw.</param>
    /// <param name="topLeft">The top-left position for the word.</param>
    /// <param name="fontDimensions">The font dimensions to use.</param>
    public void DrawWord(string word, Vector2 topLeft, FontDimensions fontDimensions) => Raylib.DrawTextEx(fontDimensions.Font, word, topLeft, fontDimensions.Size, fontDimensions.Spacing, ColorRgba.ToRayColor());
    /// <summary>
    /// Draws a word, aligning it within the specified rectangle.
    /// </summary>
    /// <param name="word">The word to draw.</param>
    /// <param name="topLeft">The top-left position for the word.</param>
    /// <param name="alignement">The alignment of the word within the rectangle.</param>
    public void DrawWord(string word, Vector2 topLeft, AnchorPoint alignement)
    {
        var size = FontDimensions.GetTextSize(word);
        Rect r = new(topLeft, size, alignement);
        Raylib.DrawTextEx(Font, word, r.TopLeft, FontSize, FontSpacing, ColorRgba.ToRayColor());
    }
    /// <summary>
    /// Draws a word with a caret, aligning it within the specified rectangle.
    /// </summary>
    /// <param name="word">The word to draw.</param>
    /// <param name="topLeft">The top-left position for the word.</param>
    /// <param name="alignement">The alignment of the word within the rectangle.</param>
    /// <param name="caret">The caret indicating the text cursor position.</param>
    public void DrawWord(string word, Vector2 topLeft, AnchorPoint alignement, Caret caret)
    {
        DrawWord(word, topLeft, alignement);
        
        if (caret.IsValid)
        {
            string caretText = word.Substring(0, caret.Index);
            var caretTextSize =  FontDimensions.GetTextSize(caretText);
            Rect r = new(topLeft, caretTextSize, alignement);
        
            var caretTop = r.TopLeft + new Vector2(caretTextSize.Width + FontSpacing * 0.5f, 0f);
            caret.Draw(caretTop, FontSize);
        }
    }
    /// <summary>
    /// Draws a word, aligning it within the specified rectangle.
    /// </summary>
    /// <param name="word">The word to draw.</param>
    /// <param name="rect">The rectangle defining the position and size.</param>
    /// <param name="alignement">The alignment of the word within the rectangle.</param>
    public void DrawWord(string word, Rect rect, AnchorPoint alignement)
    {
        if(Math.Abs(FontSizeModifier - 1f) > 0.0001f) rect = rect.ScaleSize(FontSizeModifier, alignement);
        var scaledFont = FontDimensions.ScaleDynamic(word, rect.Size);
        var textSize = scaledFont.GetTextSize(word);
        Rect r = new(rect.GetPoint(alignement), textSize, alignement);
        DrawWord(word, r.TopLeft, scaledFont);
    }
    /// <summary>
    /// Draws a word with emphasis, aligning it within the specified rectangle.
    /// </summary>
    /// <param name="word">The word to draw.</param>
    /// <param name="rect">The rectangle defining the position and size.</param>
    /// <param name="alignement">The alignment of the word within the rectangle.</param>
    /// <param name="emphasis">The emphasis settings for the word.</param>
    public void DrawWord(string word, Rect rect, AnchorPoint alignement, Emphasis emphasis)
    {
        if(Math.Abs(FontSizeModifier - 1f) > 0.0001f) rect = rect.ScaleSize(FontSizeModifier, alignement);
        var scaledFont = FontDimensions.ScaleDynamic(word, rect.Size);
        var textSize = scaledFont.GetTextSize(word);
        Rect r = new(rect.GetPoint(alignement), textSize, alignement);

        var emphasisRect = r.ApplyMargins(EmphasisRectMargins); // EmphasisRectMargins.Apply(r);
        
        emphasis.DrawForeground(emphasisRect);
        DrawWord(word, r.TopLeft, emphasis.TextColorRgba, scaledFont); //DrawTextEx(textFont.Font, word, r.TopLeft, info.fontSize, info.fontSpacing, emphasis.TextColor);
        emphasis.DrawBackground(emphasisRect);
        
    }

    /// <summary>
    /// Draws text without wrapping.
    /// </summary>
    /// <param name="text">The text to draw.</param>
    /// <param name="rect">The rectangle defining the position and size.</param>
    /// <param name="alignement">The alignment of the text within the rectangle.</param>
    public void DrawTextWrapNone(string text, Rect rect, AnchorPoint alignement)
    {
        var scaledFont = FontDimensions.ScaleDynamic(text, rect.Size);
        var textSize = scaledFont.GetTextSize(text);
        Rect r = new(rect.GetPoint(alignement), textSize, alignement);
        DrawWord(text, r.TopLeft, scaledFont);
    }
    /// <summary>
    /// Draws text without wrapping, using the specified color.
    /// </summary>
    /// <param name="text">The text to draw.</param>
    /// <param name="rect">The rectangle defining the position and size.</param>
    /// <param name="alignement">The alignment of the text within the rectangle.</param>
    /// <param name="colorRgba">The color to use for text rendering.</param>
    public void DrawTextWrapNone(string text, Rect rect, AnchorPoint alignement, ColorRgba colorRgba)
    {
        var scaledFont = FontDimensions.ScaleDynamic(text, rect.Size);
        var textSize = scaledFont.GetTextSize(text);
        Rect r = new(rect.GetPoint(alignement), textSize, alignement);
        DrawWord(text, r.TopLeft, colorRgba, scaledFont);
    }

    /// <summary>
    /// Draws text without wrapping, and shows the caret if valid.
    /// </summary>
    /// <param name="text">The text to draw.</param>
    /// <param name="rect">The rectangle defining the position and size.</param>
    /// <param name="alignement">The alignment of the text within the rectangle.</param>
    /// <param name="caret">The caret indicating the text cursor position.</param>
    public void DrawTextWrapNone(string text, Rect rect, AnchorPoint alignement, Caret caret)
    {
        var scaledFont = FontDimensions.ScaleDynamic(text, rect.Size);
        var textSize = scaledFont.GetTextSize(text);
        Rect r = new(rect.GetPoint(alignement), textSize, alignement);
        DrawWord(text, r.TopLeft, scaledFont);

        if (caret.IsValid)
        {
            string caretText = text.Substring(0, caret.Index);
            var caretTextSize =  scaledFont.GetTextSize(caretText);
        
            var caretTop = r.TopLeft + new Vector2(caretTextSize.Width + scaledFont.Spacing * 0.5f, 0f);
            caret.Draw(caretTop, scaledFont.Size);
        }
    }
    /// <summary>
    /// Draws text without wrapping, applying specified emphases, and shows the caret if valid.
    /// </summary>
    /// <param name="text">The text to draw.</param>
    /// <param name="rect">The rectangle defining the position and size.</param>
    /// <param name="alignement">The alignment of the text within the rectangle.</param>
    /// <param name="caret">The caret indicating the text cursor position.</param>
    /// <param name="emphases">The list of emphases to apply to the text.</param>
    public void DrawTextWrapNone(string text, Rect rect, AnchorPoint alignement, Caret caret, List<TextEmphasis>? emphases)
    {
        if(Math.Abs(FontSizeModifier - 1f) > 0.0001f) rect = rect.ScaleSize(FontSizeModifier, alignement);
        
        var scaledFont = FontDimensions.ScaleDynamic(text, rect.Size);
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
                var pos = curWordPos + new Vector2(curWordWidth + scaledFont.Spacing / 2, 0f);
                caretTop = pos;
            }
            
            var c = text[i];
            float w = scaledFont.GetCharSize(c).Width;
            curWordWidth += w;
            curWordWidth += scaledFont.Spacing;
            
            if (c == ' ')
            {
                var wordEmphasis = GetEmphasis(curWord, emphases);

                if (wordEmphasis != null) DrawWord(curWord, curWordPos, curWordWidth - w, wordEmphasis, scaledFont);
                else DrawWord(curWord, curWordPos, scaledFont);
                
                curWord = string.Empty;
                curWordPos += new Vector2(curWordWidth, 0f);
                curWordWidth = 0f;
            }
            else curWord += c;

            
        }
        
        var lastWordEmphasis = GetEmphasis(curWord, emphases);
        if (lastWordEmphasis != null) DrawWord(curWord, curWordPos, curWordWidth, lastWordEmphasis, scaledFont);
        else DrawWord(curWord, curWordPos, scaledFont);

        if (caretFound)
        {
            caret.Draw(caretTop, scaledFont.Size);
        }
        else
        {
            if (caret.IsValid)
            {
                var pos = curWordPos + new Vector2(curWordWidth + scaledFont.Spacing / 2, 0f);
                caret.Draw(pos, scaledFont.Size);
            }
        }
    }
    
    
    /// <summary>
    /// Draws text with character wrapping within the specified rectangle.
    /// </summary>
    /// <param name="text">The text to draw.</param>
    /// <param name="rect">The rectangle defining the position and size.</param>
    /// <param name="alignement">The alignment of the text within the rectangle.</param>
    public void DrawTextWrapChar(string text, Rect rect, AnchorPoint alignement)
    {
        DrawTextWrapChar(text, rect, alignement, new(), null);
    }
    /// <summary>
    /// Draws text with character wrapping and a caret, within the specified rectangle.
    /// </summary>
    /// <param name="text">The text to draw.</param>
    /// <param name="rect">The rectangle defining the position and size.</param>
    /// <param name="alignement">The alignment of the text within the rectangle.</param>
    /// <param name="caret">The caret indicating the text cursor position.</param>
    public void DrawTextWrapChar(string text, Rect rect, AnchorPoint alignement, Caret caret)
    {
        DrawTextWrapChar(text, rect, alignement, caret, null);
    }
    /// <summary>
    /// Draws text with character wrapping, a caret, and specified emphases, within the rectangle.
    /// </summary>
    /// <param name="text">The text to draw.</param>
    /// <param name="rect">The rectangle defining the position and size.</param>
    /// <param name="alignement">The alignment of the text within the rectangle.</param>
    /// <param name="caret">The caret indicating the text cursor position.</param>
    /// <param name="emphases">The list of emphases to apply to the text.</param>
    public void DrawTextWrapChar(string text, Rect rect, AnchorPoint alignement, Caret caret, List<TextEmphasis>? emphases)
    {
        if(Math.Abs(FontSizeModifier - 1f) > 0.0001f) rect = rect.ScaleSize(FontSizeModifier, alignement);
        // var rectSize = rect.Size;
        
        var textSize = FontDimensions.GetTextBaseSize(text);
    
        if (textSize.Width < rect.Size.Width)//no wrapping needed
        {
            DrawTextWrapNone(text, rect, alignement, caret, emphases);
        }
        else
        {
            var scaledFont = FontDimensions.ScaleDynamicWrapMode(text, rect.Size, 1.25f);
            var sizeF = scaledFont.Size / BaseSize;
            var pos = rect.TopLeft;
            var curWord = string.Empty;
            var lineBreakInProcess = false;
            Emphasis? lineBreakEmphasis = null;
            var curWordWidth = 0f;
            var curLineWidth = 0f;
    
            var caretTop = new Vector2();
            var caretFound = false;
            string completeWord = string.Empty;
            
            for (int i = 0; i < text.Length; i++)
            {
                if (!caretFound && caret.IsValid && i == caret.Index)
                {
                    caretFound = true;
                    caretTop = pos + new Vector2(curWordWidth + scaledFont.Spacing / 2, 0f);
                }
                
                var c = text[i];
                if (c == '\n') continue;
            
                var charBaseSize = scaledFont.GetCharBaseSize(c);
                float glyphWidth = charBaseSize.Width * sizeF;
            
                if (curLineWidth + curWordWidth + glyphWidth >= rect.Size.Width && curLineWidth > 0)//break line
                {
                    if (c == ' ') 
                    {
                        var emphasis = GetEmphasis(curWord, emphases);

                        if (MouseDetection != null)
                        {
                            var mouseRect = new Rect(pos, new Size(curWordWidth, scaledFont.Size));
                            if (mouseRect.ContainsPoint(MouseDetection.GetMousePosition()))
                            {
                                var mouseEmphasis = MouseDetection.OnMouseEntered(curWord, curWord, rect);
                                if(mouseEmphasis != null) DrawWord(curWord, pos, curWordWidth, mouseEmphasis, scaledFont);
                                else
                                {
                                    if (emphasis != null) DrawWord(curWord, pos, curWordWidth, emphasis, scaledFont);
                                    else DrawWord(curWord, pos, scaledFont);
                                }
                            }
                            else
                            {
                                if (emphasis != null) DrawWord(curWord, pos, curWordWidth, emphasis, scaledFont);
                                else DrawWord(curWord, pos, scaledFont);
                            }
                        }
                        else
                        {
                            if (emphasis != null) DrawWord(curWord, pos, curWordWidth, emphasis, scaledFont);
                            else DrawWord(curWord, pos, scaledFont);
                        }
                    }
                    else
                    {
                        if (!lineBreakInProcess)
                        {
                            lineBreakInProcess = true;
                            completeWord = curWord;
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
    
                        if (MouseDetection != null)
                        {
                            var mouseRect = new Rect(pos, new Size(curWordWidth, scaledFont.Size));
                            if (mouseRect.ContainsPoint(MouseDetection.GetMousePosition()))
                            {
                                var mouseEmphasis = MouseDetection.OnMouseEntered(curWord, completeWord, rect);
                                if(mouseEmphasis != null) DrawWord(curWord, pos, curWordWidth, mouseEmphasis, scaledFont);
                                else
                                {
                                    if (lineBreakEmphasis != null) DrawWord(curWord, pos, curWordWidth, lineBreakEmphasis, scaledFont);
                                    else DrawWord(curWord, pos, scaledFont);
                                }
                            }
                            else
                            {
                                if (lineBreakEmphasis != null) DrawWord(curWord, pos, curWordWidth, lineBreakEmphasis, scaledFont);
                                else DrawWord(curWord, pos, scaledFont);
                            }
                        }
                        else
                        {
                            if (lineBreakEmphasis != null) DrawWord(curWord, pos, curWordWidth, lineBreakEmphasis, scaledFont);
                            else DrawWord(curWord, pos, scaledFont);
                        }
                    } 
                    
                    
                    curWord = string.Empty;
                    if(c != ' ') curWord += c;
                    pos.Y += scaledFont.Size + scaledFont.LineSpacing;
                    pos.X = rect.TopLeft.X;
                    curLineWidth = 0f;
                    curWordWidth = glyphWidth;// 0f;
                    
                    continue;
                }
            
                curWordWidth += glyphWidth + scaledFont.Spacing;
                if (c == ' ')
                {
                    if (lineBreakInProcess)
                    {
                        lineBreakInProcess = false;
                        if (MouseDetection != null)
                        {
                            var mouseRect = new Rect(pos, new Size(curWordWidth - glyphWidth, scaledFont.Size));
                            if (mouseRect.ContainsPoint(MouseDetection.GetMousePosition()))
                            {
                                var mouseEmphasis = MouseDetection.OnMouseEntered(curWord, completeWord, rect);
                                if(mouseEmphasis != null) DrawWord(curWord, pos, curWordWidth - glyphWidth, mouseEmphasis, scaledFont);
                                else
                                {
                                    if (lineBreakEmphasis != null) DrawWord(curWord, pos, curWordWidth - glyphWidth, lineBreakEmphasis, scaledFont);
                                    else DrawWord(curWord, pos, scaledFont);
                                }
                            }
                            else
                            {
                                if (lineBreakEmphasis != null) DrawWord(curWord, pos, curWordWidth - glyphWidth, lineBreakEmphasis, scaledFont);
                                else DrawWord(curWord, pos, scaledFont);
                            }
                        }
                        else
                        {
                            if (lineBreakEmphasis != null) DrawWord(curWord, pos, curWordWidth - glyphWidth, lineBreakEmphasis, scaledFont);
                            else DrawWord(curWord, pos, scaledFont);
                        }
                    }
                    else
                    {
                        var wordEmphasis = GetEmphasis(curWord, emphases);
                        
                        if (MouseDetection != null)
                        {
                            var mouseRect = new Rect(pos, new Size(curWordWidth - glyphWidth, scaledFont.Size));
                            if (mouseRect.ContainsPoint(MouseDetection.GetMousePosition()))
                            {
                                var mouseEmphasis = MouseDetection.OnMouseEntered(curWord, curWord, rect);
                                if(mouseEmphasis != null) DrawWord(curWord, pos, curWordWidth - glyphWidth, mouseEmphasis, scaledFont);
                                else
                                {
                                    if (wordEmphasis != null) DrawWord(curWord, pos, curWordWidth - glyphWidth, wordEmphasis, scaledFont);
                                    else DrawWord(curWord, pos, scaledFont);
                                }
                            }
                            else
                            {
                                if (wordEmphasis != null) DrawWord(curWord, pos, curWordWidth - glyphWidth, wordEmphasis, scaledFont);
                                else DrawWord(curWord, pos, scaledFont);
                            }
                        }
                        else
                        {
                            if (wordEmphasis != null) DrawWord(curWord, pos, curWordWidth - glyphWidth, wordEmphasis, scaledFont);
                            else DrawWord(curWord, pos, scaledFont);
                        }
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
                if (MouseDetection != null)
                {
                    var mouseRect = new Rect(pos, new Size(curWordWidth, scaledFont.Size));
                    if (mouseRect.ContainsPoint(MouseDetection.GetMousePosition()))
                    {
                        var mouseEmphasis = MouseDetection.OnMouseEntered(curWord, completeWord, rect);
                        if(mouseEmphasis != null) DrawWord(curWord, pos, curWordWidth, mouseEmphasis, scaledFont);
                        else
                        {
                            if (lineBreakEmphasis != null) DrawWord(curWord, pos, curWordWidth, lineBreakEmphasis, scaledFont);
                            else DrawWord(curWord, pos, scaledFont);
                        }
                    }
                    else
                    {
                        if (lineBreakEmphasis != null) DrawWord(curWord, pos, curWordWidth, lineBreakEmphasis, scaledFont);
                        else DrawWord(curWord, pos, scaledFont);
                    }
                }
                else
                {
                    if (lineBreakEmphasis != null) DrawWord(curWord, pos, curWordWidth, lineBreakEmphasis, scaledFont);
                    else DrawWord(curWord, pos, scaledFont);
                }
            }
            else
            {
                var wordEmphasis = GetEmphasis(curWord, emphases);
                if (MouseDetection != null)
                {
                    var mouseRect = new Rect(pos, new Size(curWordWidth, scaledFont.Size));
                    if (mouseRect.ContainsPoint(MouseDetection.GetMousePosition()))
                    {
                        var mouseEmphasis = MouseDetection.OnMouseEntered(curWord, curWord, rect);
                        if(mouseEmphasis != null) DrawWord(curWord, pos, curWordWidth, mouseEmphasis, scaledFont);
                        else
                        {
                            if (wordEmphasis != null) DrawWord(curWord, pos, curWordWidth, wordEmphasis, scaledFont);
                            else DrawWord(curWord, pos, scaledFont);
                        }
                    }
                    else
                    {
                        if (wordEmphasis != null) DrawWord(curWord, pos, curWordWidth, wordEmphasis, scaledFont);
                        else DrawWord(curWord, pos, scaledFont);
                    }
                }
                else
                {
                    if (wordEmphasis != null) DrawWord(curWord, pos, curWordWidth, wordEmphasis, scaledFont);
                    else DrawWord(curWord, pos, scaledFont);
                }
            }
            
            
            if (caretFound)
            {
                caret.Draw(caretTop, scaledFont.Size);
            }
            else
            {
                if (caret.IsValid)
                {
                    var topLeft = pos + new Vector2(curWordWidth + scaledFont.Spacing / 2, 0f);
                    caret.Draw(topLeft, scaledFont.Size);
                }
            }
        }
    }
    
    /// <summary>
    /// Draws text with word wrapping within the specified rectangle.
    /// </summary>
    /// <param name="text">The text to draw.</param>
    /// <param name="rect">The rectangle defining the position and size.</param>
    /// <param name="alignement">The alignment of the text within the rectangle.</param>
    public void DrawTextWrapWord(string text, Rect rect, AnchorPoint alignement)
    {
        DrawTextWrapWord(text, rect, alignement, new(), null);
    }
    /// <summary>
    /// Draws text with word wrapping, and a caret within the specified rectangle.
    /// </summary>
    /// <param name="text">The text to draw.</param>
    /// <param name="rect">The rectangle defining the position and size.</param>
    /// <param name="alignement">The alignment of the text within the rectangle.</param>
    /// <param name="caret">The caret indicating the text cursor position.</param>
    public void DrawTextWrapWord(string text, Rect rect, AnchorPoint alignement, Caret caret)
    {
        DrawTextWrapWord(text, rect, alignement, new(), null);
    }
    /// <summary>
    /// Draws text with word wrapping, a caret, and specified emphases within the rectangle.
    /// </summary>
    /// <param name="text">The text to draw.</param>
    /// <param name="rect">The rectangle defining the position and size.</param>
    /// <param name="alignement">The alignment of the text within the rectangle.</param>
    /// <param name="caret">The caret indicating the text cursor position.</param>
    /// <param name="emphases">The list of emphases to apply to the text.</param>
    public void DrawTextWrapWord(string text, Rect rect, AnchorPoint alignement, Caret caret, List<TextEmphasis>? emphases)
    {
        if(Math.Abs(FontSizeModifier - 1f) > 0.0001f) rect = rect.ScaleSize(FontSizeModifier, alignement);
        // var rectSize = rect.Size;
        var textSize = FontDimensions.GetTextBaseSize(text);
    
        if (textSize.Width < rect.Size.Width)
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
            var scaledFont = FontDimensions.ScaleDynamicWrapMode(text, rect.Size, 1.5f);
            var sizeF = scaledFont.Size / BaseSize;
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
                float glyphWidth = charBaseSize.Width * sizeF;
                
                if (curLineWidth + curWordWidth + glyphWidth >= rect.Width && curLineWidth > 0)//break line
                {
                    // if (curLineWidth <= 0) return;
                    pos.Y += scaledFont.Size + scaledFont.LineSpacing;
                    pos.X = rect.TopLeft.X;
                    curLineWidth = 0f;
                }
            
                curWordWidth += glyphWidth + scaledFont.Spacing;
                
                if (i == caret.Index - 1 && caret.IsValid)
                {
                    caretWordOffset = curWordWidth;
                }
                if (c == ' ')
                {
                    var wordEmphasis = GetEmphasis(curWord, emphases);
    
                    if (MouseDetection != null)
                    {
                        var mouseRect = new Rect(pos, new Size(curWordWidth - glyphWidth, scaledFont.Size));
                        if (mouseRect.ContainsPoint(MouseDetection.GetMousePosition()))
                        {
                            var mouseEmphasis = MouseDetection.OnMouseEntered(curWord, curWord, rect);
                            if(mouseEmphasis != null) DrawWord(curWord, pos, curWordWidth - glyphWidth, mouseEmphasis, scaledFont);
                            else
                            {
                                if (wordEmphasis != null) DrawWord(curWord, pos, curWordWidth - glyphWidth, wordEmphasis, scaledFont);
                                else DrawWord(curWord, pos, scaledFont);
                            }
                        }
                        else
                        {
                            if (wordEmphasis != null) DrawWord(curWord, pos, curWordWidth - glyphWidth, wordEmphasis, scaledFont);
                            else DrawWord(curWord, pos, scaledFont);
                        }
                    }
                    else
                    {
                        if (wordEmphasis != null) DrawWord(curWord, pos, curWordWidth - glyphWidth, wordEmphasis, scaledFont);
                        else DrawWord(curWord, pos, scaledFont);
                    }
            
                    if (caretWordOffset >= 0)
                    {
                        caretFound = true;
                        caretTop = pos + new Vector2(caretWordOffset + scaledFont.Spacing / 2, 0f);
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
    
            if (MouseDetection != null)
            {
                var mouseRect = new Rect(pos, new Size(curWordWidth, scaledFont.Size));
                if (mouseRect.ContainsPoint(MouseDetection.GetMousePosition()))
                {
                    var mouseEmphasis = MouseDetection.OnMouseEntered(curWord, curWord, rect);
                    if(mouseEmphasis != null) DrawWord(curWord, pos, curWordWidth, mouseEmphasis, scaledFont);
                    else
                    {
                        if (lastWordEmphasis != null) DrawWord(curWord, pos, curWordWidth, lastWordEmphasis, scaledFont);
                        else DrawWord(curWord, pos, scaledFont);
                    }
                }
                else
                {
                    if (lastWordEmphasis != null) DrawWord(curWord, pos, curWordWidth, lastWordEmphasis, scaledFont);
                    else DrawWord(curWord, pos, scaledFont);
                }
            }
            else
            {
                if (lastWordEmphasis != null) DrawWord(curWord, pos, curWordWidth, lastWordEmphasis, scaledFont);
                else DrawWord(curWord, pos, scaledFont);
            }
            
            
            if (caretFound)
            {
                caret.Draw(caretTop, scaledFont.Size);
            }
            else if (caretWordOffset >= 0)
            {
                caretTop = pos + new Vector2(caretWordOffset + scaledFont.Spacing / 2, 0f);
                caret.Draw(caretTop, scaledFont.Size);
            }
            else
            {
                if (caret.IsValid)
                {
                    caret.Draw(rect.TopLeft, scaledFont.Size);
                }
            }
        }
    }
    
    /// <summary>
    /// Draws a word with emphasis, using the specified width, emphasis settings, and font dimensions.
    /// </summary>
    /// <param name="word">The word to draw.</param>
    /// <param name="topLeft">The top-left position for the word.</param>
    /// <param name="width">The width to use for the word's bounding rectangle.</param>
    /// <param name="emphasis">The emphasis settings to apply (background, foreground, color).</param>
    /// <param name="fontDimensions">The font dimensions to use for rendering.</param>
    protected void DrawWord(string word, Vector2 topLeft, float width, Emphasis emphasis, FontDimensions fontDimensions)
    {
        Rect r = new(topLeft, new Size(width, fontDimensions.Size), new());

        var emphasisRect = r.ApplyMargins(EmphasisRectMargins);
        
        emphasis.DrawBackground(emphasisRect);
        DrawWord(word, topLeft, emphasis.TextColorRgba, fontDimensions);
        emphasis.DrawForeground(emphasisRect);
        
    }

    /// <summary>
    /// Draws a word at the specified position, using the given color and font dimensions.
    /// </summary>
    /// <param name="word">The word to draw.</param>
    /// <param name="topLeft">The top-left position for the word.</param>
    /// <param name="colorRgba">The color to use for text rendering.</param>
    /// <param name="fontDimensions">The font dimensions to use for rendering.</param>
    protected void DrawWord(string word, Vector2 topLeft, ColorRgba colorRgba, FontDimensions fontDimensions) => Raylib.DrawTextEx(fontDimensions.Font, word, topLeft, fontDimensions.Size, fontDimensions.Spacing, colorRgba.ToRayColor());
       
    #endregion

    #region Public

    /// <summary>
    /// Creates a clone of the current <see cref="TextFont"/> instance.
    /// </summary>
    /// <returns>A new <see cref="TextFont"/> instance with the same settings.</returns>
    public TextFont Clone() => new(FontDimensions, ColorRgba);
    /// <summary>
    /// Sets the texture filter for the font's texture.
    /// </summary>
    /// <param name="textureFilter">The texture filter to apply.</param>
    public void SetFilter(TextureFilter textureFilter = TextureFilter.Bilinear)
    {
        Raylib.SetTextureFilter(FontDimensions.Font.Texture, textureFilter);
    }
    /// <summary>
    /// Gets the emphasis settings for a word from the list of emphases.
    /// </summary>
    /// <param name="word">The word to check for emphasis.</param>
    /// <param name="emphases">The list of emphases to search.</param>
    /// <returns>The emphasis settings if found, otherwise null.</returns>
    public static Emphasis? GetEmphasis(string word, List<TextEmphasis>? emphases)
    {
        if (emphases == null || emphases.Count <= 0) return null;
        
        foreach (var e in emphases)
        {
            if (e.HasKeyword(word)) return e.Emphasis;
        }

        return null;
    }
    #endregion
    
}