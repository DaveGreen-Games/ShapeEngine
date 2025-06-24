using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Text;

namespace ShapeEngine.Effects;

/// <summary>
/// Represents an effect that displays text.
/// </summary>
public class TextEffect : Effect
{
    /// <summary>
    /// Gets or sets the text to display.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TextEffect"/> class.
    /// </summary>
    /// <param name="pos">The position of the text effect.</param>
    /// <param name="size">The size of the text effect.</param>
    /// <param name="rotRad">The rotation in radians.</param>
    /// <param name="text">The text to display.</param>
    public TextEffect(Vector2 pos, Size size, float rotRad, string text) : base(pos, size, rotRad) { this.Text = text; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TextEffect"/> class with a specified lifetime.
    /// </summary>
    /// <param name="pos">The position of the text effect.</param>
    /// <param name="size">The size of the text effect.</param>
    /// <param name="rotRad">The rotation in radians.</param>
    /// <param name="text">The text to display.</param>
    /// <param name="lifeTime">The lifetime of the text effect in seconds.</param>
    public TextEffect(Vector2 pos, Size size, float rotRad, string text, float lifeTime) : base(pos, size, rotRad, lifeTime) { this.Text = text; }

    /// <summary>
    /// Draws the text using the specified font and alignment.
    /// </summary>
    /// <param name="textFont">The font to use for drawing the text.</param>
    /// <param name="alignement">The anchor point for text alignment.</param>
    protected void DrawText(TextFont textFont, AnchorPoint alignement)
    {
        textFont.DrawTextWrapNone(Text, GetBoundingBox(), alignement);
    }
}

