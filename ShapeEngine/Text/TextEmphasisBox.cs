using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.Rect;

namespace ShapeEngine.Text;

/// <summary>
/// Provides a text box with support for word emphasis and caret rendering.
/// </summary>
/// <remarks>
/// Supports drawing text with optional emphasis and caret, using a specified <see cref="TextFont"/>.
/// </remarks>
public class TextEmphasisBox
{
    #region Members
    /// <summary>
    /// The list of text emphases to apply to words.
    /// </summary>
    public readonly List<TextEmphasis> Emphases = new();
    /// <summary>
    /// The font and color settings for text rendering.
    /// </summary>
    public TextFont TextFont;
    /// <summary>
    /// The caret to display in the text box.
    /// </summary>
    public Caret Caret = new();
    /// <summary>
    /// Whether to use emphasis highlighting for words.
    /// </summary>
    public bool UseEmphasis = true;
    #endregion

    #region Main
    /// <summary>
    /// Initializes a new instance of the <see cref="TextEmphasisBox"/> class.
    /// </summary>
    /// <param name="textFont">The font and color settings for text rendering.</param>
    public TextEmphasisBox(TextFont textFont)
    {
        this.TextFont = textFont;
    }

    /// <summary>
    /// Determines whether any emphasis is enabled and available.
    /// </summary>
    /// <returns>True if emphasis is enabled and there are emphases defined; otherwise, false.</returns>
    public bool HasEmphasis() => UseEmphasis && Emphases.Count > 0;

    /// <summary>
    /// Draws the text with optional emphasis and caret.
    /// </summary>
    /// <param name="text">The text to draw.</param>
    /// <param name="rect">The rectangle area to draw the text in.</param>
    /// <param name="alignement">The anchor point for text alignment.</param>
    /// <param name="mousePos">The current mouse position (for emphasis detection).</param>
    /// <param name="textWrapType">The type of text wrapping to use.</param>
    public void Draw(string text, Rect rect, AnchorPoint alignement, Vector2 mousePos, TextWrapType textWrapType = TextWrapType.None)
    {
        if(textWrapType == TextWrapType.None)
        {
            TextFont.DrawTextWrapNone(text, rect ,alignement, Caret, Emphases);
        }
        else if (textWrapType == TextWrapType.Char)
        {
            TextFont.DrawTextWrapChar(text, rect, alignement, Caret, Emphases);
        }
        else
        {
            TextFont.DrawTextWrapWord(text, rect, alignement, Caret, Emphases);
        }
    }
    #endregion
}
