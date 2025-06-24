namespace ShapeEngine.Text;

/// <summary>
/// Specifies the type of text wrapping to apply.
/// </summary>
public enum TextWrapType
{
    /// <summary>
    /// No text wrapping.
    /// </summary>
    None = 0,
    /// <summary>
    /// Wrap text at the character level.
    /// </summary>
    Char = 1,
    /// <summary>
    /// Wrap text at the word level.
    /// </summary>
    Word = 2
}