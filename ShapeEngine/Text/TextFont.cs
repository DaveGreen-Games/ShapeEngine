using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Text;

public class TextFont
{
    public event Action<string, Rect, Vector2>? OnMouseEnteredWord;
    
    #region Static Members

    public static Rect.Margins EmphasisRectMargins = new();
    // public static RangeFloat FontSizeRange = new(10, 150);
    private static float fontSizeModifier = 1f;
    public static float FontSizeModifier
    {
        get => fontSizeModifier;
        set => fontSizeModifier = MathF.Max(value, 0.1f);
    }


    #endregion

    #region Members
    public FontDimensions FontDimensions;
    public ColorRgba ColorRgba;
    public Vector2 MousePos = new();
    public Emphasis? MouseEmphasis = null;
    #endregion

    #region Constructors

    public TextFont(FontDimensions fontDimensions)
    {
        FontDimensions = fontDimensions;
        ColorRgba = new ColorRgba(System.Drawing.Color.Black);
    }
    public TextFont(FontDimensions fontDimensions, ColorRgba colorRgba)
    {
        FontDimensions = fontDimensions;
        ColorRgba = colorRgba;
    }
    public TextFont(FontDimensions fontDimensions, ColorRgba colorRgba, Emphasis mouseEmphasis)
    {
        FontDimensions = fontDimensions;
        ColorRgba = colorRgba;
        MouseEmphasis = mouseEmphasis;
    }
    public TextFont(Font font, float fontSpacing, ColorRgba colorRgba)
    {
        FontDimensions = new(font, font.BaseSize, fontSpacing, 0f);
        ColorRgba = colorRgba;
    }
    public TextFont(Font font, float fontSize, float fontSpacing, float lineSpacing, ColorRgba colorRgba)
    {
        FontDimensions = new(font, fontSize, fontSpacing, lineSpacing);
        ColorRgba = colorRgba;
    }

    // public TextFont(Font font)
    // {
    //     FontDimensions = new(font, font.BaseSize, 0f, 0f);
    //     ColorRgba = new(System.Drawing.Color.White);
    // }
    // public TextFont(Font font, ColorRgba colorRgba)
    // {
    //     FontDimensions = new(font, font.BaseSize, 0f, 0f);
    //     ColorRgba = colorRgba;
    // }
    // public TextFont(Font font, float fontSpacing, float lineSpacing, ColorRgba colorRgba)
    // {
    //     FontDimensions = new(font, font.BaseSize, fontSpacing, lineSpacing);
    //     ColorRgba = colorRgba;
    // }
    //
   
    

    #endregion

    #region Getters & Setters

    public Font Font
    {
        get => FontDimensions.Font;
        set => FontDimensions = FontDimensions.SetFont(value);
    }
    public float BaseSize => FontDimensions.BaseSize;
    public float FontSize
    {
        get => FontDimensions.Size;
        set => FontDimensions = FontDimensions.SetFontSize(value);
    }
    public float FontSpacing
    {
        get => FontDimensions.Spacing;
        set => FontDimensions = FontDimensions.SetSpacing(value);
    }
    public float LineSpacing
    {
        get => FontDimensions.LineSpacing;
        set => FontDimensions = FontDimensions.SetLineSpacing(value);
    }

    #endregion
    
    #region Draw

    public void Draw(char c, Rect rect, Vector2 alignement)
    {
        if(Math.Abs(FontSizeModifier - 1f) > 0.0001f) rect = rect.ScaleSize(FontSizeModifier, alignement);
        float f = rect.Size.Height / BaseSize;
        float fontSize = BaseSize * f;
        var charSize = FontDimensions.GetCharSize(c, fontSize);
        
        var uiPos = rect.GetPoint(alignement);
        var charRect = new Rect(uiPos, charSize, alignement);
        
        Raylib.DrawTextCodepoint(Font, c, charRect.TopLeft, fontSize, ColorRgba.ToRayColor());
    }
    public void Draw(string text, Rect rect, float rotDeg, Vector2 alignement)
    {
        if(Math.Abs(FontSizeModifier - 1f) > 0.0001f) rect = rect.ScaleSize(FontSizeModifier, alignement);
        var scaledFont = FontDimensions.ScaleDynamic(text, rect.Size);
        var textSize = scaledFont.GetTextSize(text);
        Rect r = new(rect.GetPoint(alignement), textSize, alignement);
        var originOffset = alignement * textSize;
        Raylib.DrawTextPro(scaledFont.Font, text, r.TopLeft + originOffset, originOffset, rotDeg, scaledFont.Size, scaledFont.Spacing, ColorRgba.ToRayColor());
    }
    public void DrawWord(string word, Vector2 topLeft) => Raylib.DrawTextEx(Font, word, topLeft, FontSize, FontSpacing, ColorRgba.ToRayColor());
    public void DrawWord(string word, Vector2 topLeft, FontDimensions fontDimensions) => Raylib.DrawTextEx(fontDimensions.Font, word, topLeft, fontDimensions.Size, fontDimensions.Spacing, ColorRgba.ToRayColor());
    public void DrawWord(string word, Vector2 topLeft, Vector2 alignement)
    {
        var size = FontDimensions.GetTextSize(word);
        Rect r = new(topLeft, size, alignement);
        Raylib.DrawTextEx(Font, word, r.TopLeft, FontSize, FontSpacing, ColorRgba.ToRayColor());
    }
    public void DrawWord(string word, Vector2 topLeft, Vector2 alignement, Caret caret)
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
    public void DrawWord(string word, Rect rect, Vector2 alignement)
    {
        if(Math.Abs(FontSizeModifier - 1f) > 0.0001f) rect = rect.ScaleSize(FontSizeModifier, alignement);
        var scaledFont = FontDimensions.ScaleDynamic(word, rect.Size);
        var textSize = scaledFont.GetTextSize(word);
        Rect r = new(rect.GetPoint(alignement), textSize, alignement);
        DrawWord(word, r.TopLeft, scaledFont);
    }
    public void DrawWord(string word, Rect rect, Vector2 alignement, Emphasis emphasis)
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

    public void DrawTextWrapNone(string text, Rect rect, Vector2 alignement)
    {
        var scaledFont = FontDimensions.ScaleDynamic(text, rect.Size);
        var textSize = scaledFont.GetTextSize(text);
        Rect r = new(rect.GetPoint(alignement), textSize, alignement);
        DrawWord(text, r.TopLeft, scaledFont);
    }
    public void DrawTextWrapNone(string text, Rect rect, Vector2 alignement, ColorRgba colorRgba)
    {
        var scaledFont = FontDimensions.ScaleDynamic(text, rect.Size);
        var textSize = scaledFont.GetTextSize(text);
        Rect r = new(rect.GetPoint(alignement), textSize, alignement);
        DrawWord(text, r.TopLeft, colorRgba, scaledFont);
    }

    public void DrawTextWrapNone(string text, Rect rect, Vector2 alignement, Caret caret)
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
    public void DrawTextWrapNone(string text, Rect rect, Vector2 alignement, Caret caret, List<TextEmphasis>? emphases)
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
        
        var textSize = FontDimensions.GetTextBaseSize(text);

        if (textSize.Width < rect.Size.Width)//no wrapping needed
        {
            DrawTextWrapNone(text, rect, alignement, caret, emphases);
        }
        else
        {
            var scaledFont = FontDimensions.ScaleDynamicWrapMode(text, rect.Size, 1.25f);
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
            var sizeF = scaledFont.Size / BaseSize;
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

                        if (MouseEmphasis != null)
                        {
                            var mouseRect = new Rect(pos, new Size(curWordWidth, scaledFont.Size));
                            if (mouseRect.ContainsPoint(MousePos))
                            {
                                DrawWord(curWord, pos, curWordWidth, MouseEmphasis, scaledFont);
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
                        
                        
                        // if (emphasis != null)
                        // {
                        //     if (MouseEmphasis != null)
                        //     {
                        //         var mouseRect = new Rect(pos, new Size(curWordWidth, scaledFont.Size));
                        //         if (mouseRect.ContainsPoint(MousePos))
                        //         {
                        //             DrawWord(curWord, pos, curWordWidth, MouseEmphasis, scaledFont);
                        //         }
                        //         else DrawWord(curWord, pos, curWordWidth, emphasis, scaledFont);
                        //     }
                        //     else DrawWord(curWord, pos, curWordWidth, emphasis, scaledFont); 
                        //     
                        // }
                        // else
                        // {
                        //     if (MouseEmphasis != null)
                        //     {
                        //         var wordSize = scaledFont.GetTextSize(curWord);
                        //         var mouseRect = new Rect(pos, wordSize);
                        //         if (mouseRect.ContainsPoint(MousePos))
                        //         {
                        //             DrawWord(curWord, pos, wordSize.Width, MouseEmphasis, scaledFont);
                        //         }
                        //         else DrawWord(curWord, pos, scaledFont);
                        //     }
                        //     else DrawWord(curWord, pos, scaledFont);
                        //     
                        // }
                        //
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

                        if (MouseEmphasis != null)
                        {
                            var mouseRect = new Rect(pos, new Size(curWordWidth, scaledFont.Size));
                            if (mouseRect.ContainsPoint(MousePos))
                            {
                                DrawWord(curWord, pos, curWordWidth, MouseEmphasis, scaledFont);
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
                        // if (lineBreakEmphasis != null)
                        // {
                        //     if (MouseEmphasis != null)
                        //     {
                        //         var mouseRect = new Rect(pos, new Size(curWordWidth, scaledFont.Size));
                        //         if (mouseRect.ContainsPoint(MousePos))
                        //         {
                        //             DrawWord(curWord, pos, curWordWidth, MouseEmphasis, scaledFont);
                        //         }
                        //         else DrawWord(curWord, pos, curWordWidth, lineBreakEmphasis, scaledFont);
                        //     }
                        //     else DrawWord(curWord, pos, curWordWidth, lineBreakEmphasis, scaledFont); 
                        // }
                        // else
                        // {
                        //     if (MouseEmphasis != null)
                        //     {
                        //         var wordSize = scaledFont.GetTextSize(curWord);
                        //         var mouseRect = new Rect(pos, wordSize);
                        //         if (mouseRect.ContainsPoint(MousePos))
                        //         {
                        //             DrawWord(curWord, pos, wordSize.Width, MouseEmphasis, scaledFont);
                        //         }
                        //         else DrawWord(curWord, pos, scaledFont);
                        //     }
                        //     else DrawWord(curWord, pos, scaledFont);
                        // }
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
                        if (MouseEmphasis != null)
                        {
                            var mouseRect = new Rect(pos, new Size(curWordWidth - glyphWidth, scaledFont.Size));
                            if (mouseRect.ContainsPoint(MousePos))
                            {
                                DrawWord(curWord, pos, curWordWidth - glyphWidth, MouseEmphasis, scaledFont);
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
                        // if (lineBreakEmphasis != null)
                        // {
                        //     if (MouseEmphasis != null)
                        //     {
                        //         var mouseRect = new Rect(pos, new Size(curWordWidth, scaledFont.Size));
                        //         if (mouseRect.ContainsPoint(MousePos))
                        //         {
                        //             DrawWord(curWord, pos, curWordWidth, MouseEmphasis, scaledFont);
                        //         }
                        //         else DrawWord(curWord, pos, curWordWidth, lineBreakEmphasis, scaledFont);
                        //     }
                        //     else DrawWord(curWord, pos, curWordWidth, lineBreakEmphasis, scaledFont); 
                        // }
                        // else
                        // {
                        //     if (MouseEmphasis != null)
                        //     {
                        //         var wordSize = scaledFont.GetTextSize(curWord);
                        //         var mouseRect = new Rect(pos, wordSize);
                        //         if (mouseRect.ContainsPoint(MousePos))
                        //         {
                        //             DrawWord(curWord, pos, wordSize.Width, MouseEmphasis, scaledFont);
                        //         }
                        //         else DrawWord(curWord, pos, scaledFont);
                        //     }
                        //     else DrawWord(curWord, pos, scaledFont);
                        // }
                        //
                    }
                    else
                    {
                        var wordEmphasis = GetEmphasis(curWord, emphases);
                        
                        if (MouseEmphasis != null)
                        {
                            var mouseRect = new Rect(pos, new Size(curWordWidth - glyphWidth, scaledFont.Size));
                            if (mouseRect.ContainsPoint(MousePos))
                            {
                                DrawWord(curWord, pos, curWordWidth - glyphWidth, MouseEmphasis, scaledFont);
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
                        // if (wordEmphasis != null)
                        // {
                        //     if (MouseEmphasis != null)
                        //     {
                        //         var mouseRect = new Rect(pos, new Size(curWordWidth, scaledFont.Size));
                        //         if (mouseRect.ContainsPoint(MousePos))
                        //         {
                        //             DrawWord(curWord, pos, curWordWidth, MouseEmphasis, scaledFont);
                        //         }
                        //         else DrawWord(curWord, pos, curWordWidth, wordEmphasis, scaledFont);
                        //     }
                        //     else DrawWord(curWord, pos, curWordWidth, wordEmphasis, scaledFont); 
                        // }
                        // else
                        // {
                        //     if (MouseEmphasis != null)
                        //     {
                        //         var wordSize = scaledFont.GetTextSize(curWord);
                        //         var mouseRect = new Rect(pos, wordSize);
                        //         if (mouseRect.ContainsPoint(MousePos))
                        //         {
                        //             DrawWord(curWord, pos, wordSize.Width, MouseEmphasis, scaledFont);
                        //         }
                        //         else DrawWord(curWord, pos, scaledFont);
                        //     }
                        //     else DrawWord(curWord, pos, scaledFont);
                        // }
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
                if (MouseEmphasis != null)
                {
                    var mouseRect = new Rect(pos, new Size(curWordWidth, scaledFont.Size));
                    if (mouseRect.ContainsPoint(MousePos))
                    {
                        DrawWord(curWord, pos, curWordWidth, MouseEmphasis, scaledFont);
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
                // if (lineBreakEmphasis != null)
                // {
                //     if (MouseEmphasis != null)
                //     {
                //         var mouseRect = new Rect(pos, new Size(curWordWidth, scaledFont.Size));
                //         if (mouseRect.ContainsPoint(MousePos))
                //         {
                //             DrawWord(curWord, pos, curWordWidth, MouseEmphasis, scaledFont);
                //         }
                //         else DrawWord(curWord, pos, curWordWidth, lineBreakEmphasis, scaledFont);
                //     }
                //     else DrawWord(curWord, pos, curWordWidth, lineBreakEmphasis, scaledFont); 
                // }
                // else
                // {
                //     if (MouseEmphasis != null)
                //     {
                //         var wordSize = scaledFont.GetTextSize(curWord);
                //         var mouseRect = new Rect(pos, wordSize);
                //         if (mouseRect.ContainsPoint(MousePos))
                //         {
                //             DrawWord(curWord, pos, wordSize.Width, MouseEmphasis, scaledFont);
                //         }
                //         else DrawWord(curWord, pos, scaledFont);
                //     }
                //     else DrawWord(curWord, pos, scaledFont);
                // }
            }
            else
            {
                var wordEmphasis = GetEmphasis(curWord, emphases);
                if (MouseEmphasis != null)
                {
                    var mouseRect = new Rect(pos, new Size(curWordWidth, scaledFont.Size));
                    if (mouseRect.ContainsPoint(MousePos))
                    {
                        DrawWord(curWord, pos, curWordWidth, MouseEmphasis, scaledFont);
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
                // if (wordEmphasis != null)
                // {
                //     if (MouseEmphasis != null)
                //     {
                //         var mouseRect = new Rect(pos, new Size(curWordWidth, scaledFont.Size));
                //         if (mouseRect.ContainsPoint(MousePos))
                //         {
                //             DrawWord(curWord, pos, curWordWidth, MouseEmphasis, scaledFont);
                //         }
                //         else DrawWord(curWord, pos, curWordWidth, wordEmphasis, scaledFont);
                //     }
                //     else DrawWord(curWord, pos, curWordWidth, wordEmphasis, scaledFont); 
                // }
                // else
                // {
                //     if (MouseEmphasis != null)
                //     {
                //         var wordSize = scaledFont.GetTextSize(curWord);
                //         var mouseRect = new Rect(pos, wordSize);
                //         if (mouseRect.ContainsPoint(MousePos))
                //         {
                //             DrawWord(curWord, pos, wordSize.Width, MouseEmphasis, scaledFont);
                //         }
                //         else DrawWord(curWord, pos, scaledFont);
                //     }
                //     else DrawWord(curWord, pos, scaledFont);
                // }
                //
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

                    // if (wordEmphasis != null) scaledFont.DrawWord(curWord, pos, curWordWidth - glyphWidth, wordEmphasis);
                    // else scaledFont.DrawWord(curWord, pos);
                    if (MouseEmphasis != null)
                    {
                        var mouseRect = new Rect(pos, new Size(curWordWidth - glyphWidth, scaledFont.Size));
                        if (mouseRect.ContainsPoint(MousePos))
                        {
                            DrawWord(curWord, pos, curWordWidth - glyphWidth, MouseEmphasis, scaledFont);
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

            // if (lastWordEmphasis != null) scaledFont.DrawWord(curWord, pos, curWordWidth, lastWordEmphasis);
            // else scaledFont.DrawWord(curWord, pos);
            
            if (MouseEmphasis != null)
            {
                var mouseRect = new Rect(pos, new Size(curWordWidth, scaledFont.Size));
                if (mouseRect.ContainsPoint(MousePos))
                {
                    DrawWord(curWord, pos, curWordWidth, MouseEmphasis, scaledFont);
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
    
    // private void DrawWord(string word, Vector2 topLeft, ColorRgba colorRgba) => Raylib.DrawTextEx(Font, word, topLeft, FontSize, FontSpacing, colorRgba.ToRayColor());
    // private void DrawWord(string word, Vector2 topLeft, float width, Emphasis emphasis)
    // {
    //     Rect r = new(topLeft, new Size(width, FontSize), new());
    //
    //     var emphasisRect = r.ApplyMargins(EmphasisRectMargins);
    //     
    //     emphasis.DrawBackground(emphasisRect);
    //     DrawWord(word, topLeft, emphasis.TextColorRgba);
    //     emphasis.DrawForeground(emphasisRect);
    //     
    // }
    //
    private void DrawWord(string word, Vector2 topLeft, float width, Emphasis emphasis, FontDimensions fontDimensions)
    {
        Rect r = new(topLeft, new Size(width, fontDimensions.Size), new());

        var emphasisRect = r.ApplyMargins(EmphasisRectMargins);
        
        emphasis.DrawBackground(emphasisRect);
        DrawWord(word, topLeft, emphasis.TextColorRgba, fontDimensions);
        emphasis.DrawForeground(emphasisRect);
        
    }
    private void DrawWord(string word, Vector2 topLeft, ColorRgba colorRgba, FontDimensions fontDimensions) => Raylib.DrawTextEx(fontDimensions.Font, word, topLeft, fontDimensions.Size, fontDimensions.Spacing, colorRgba.ToRayColor());

    #endregion

    #region Public

    public TextFont Clone() => new(FontDimensions, ColorRgba);
    public void SetFilter(TextureFilter textureFilter = TextureFilter.Bilinear)
    {
        Raylib.SetTextureFilter(FontDimensions.Font.Texture, textureFilter);
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


    #endregion
    
    #region MouseEntered

    protected virtual void MouseHasEnteredWord(string word, Rect rect, Vector2 mousePos) { }
    private void ResolveMouseHasEnteredWord(string word, Rect rect, Vector2 mousePos)
    {
        MouseHasEnteredWord(word, rect, mousePos);
        OnMouseEnteredWord?.Invoke(word, rect, mousePos);
    }


    #endregion
    
}


 /*
    #region Size

    // public TextFont ScaleDynamic(string text)
    // {
    //     var size = MeasureTextEx(Font, text, FontSize, FontSpacing);
    //     return ScaleDynamic(text, size);
    // }
    public TextFont ScaleDynamic(string text, Size rectSize)
    {
        var fontDimensions = GetTextBaseSize(text);// MeasureTextEx(Font, text, BaseSize, FontSpacing);
        float fX = rectSize.Width / fontDimensions.Width;
        float fY = rectSize.Height / fontDimensions.Height;
        float f = MathF.Min(fX, fY);

        float scaledFontSize = FontSizeRange.Clamp(BaseSize * f);
        f = scaledFontSize / BaseSize;

        var newTextFont = new TextFont(Font, scaledFontSize, FontSpacing * f, LineSpacing * f, ColorRgba);

        return newTextFont;
    }
    public TextFont ScaleDynamicWrapMode(string text, Size rectSize, float widthFactor = 1.3f)
    {
        var fontSpacing = FontSpacing;
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

        return new(Font, fontSize, fontSpacing, lineSpacing, ColorRgba);
    }
    
    public Size GetTextBaseSize(string text) => GetTextSize(text, BaseSize);
    
    
    public Size GetTextSize(string text) => GetTextSize(text, FontSize);
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
        float fontSpacingWidth = (text.Length - 1) * FontSpacing;
        totalWidth += fontSpacingWidth;
        return new Size(totalWidth, fontSize);
    }

    
    public Size GetTextSizeLineBreak(string text) => GetTextSizeLineBreak(text, FontSize);
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
        float fontSpacingWidth = (text.Length - 1) * FontSpacing;
        totalWidth += fontSpacingWidth;
        return new Size(totalWidth, totalHeight);
    }
    
    public Size GetCharSize(char c)
    {
        var baseSize = GetCharBaseSize(c);
        float f = FontSize / (float)BaseSize;
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
    */

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