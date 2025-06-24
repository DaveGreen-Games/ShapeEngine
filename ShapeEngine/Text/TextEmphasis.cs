using System.Text.RegularExpressions;

namespace ShapeEngine.Text;

/// <summary>
/// Associates an <see cref="Emphasis"/> style with a regular expression for keyword-based text emphasis.
/// </summary>
public class TextEmphasis
{
    /// <summary>
    /// The regular expression used to match keywords for emphasis.
    /// </summary>
    private readonly Regex regex;
    /// <summary>
    /// The emphasis style to apply when a match is found.
    /// </summary>
    public readonly Emphasis Emphasis;

    /// <summary>
    /// Initializes a new instance of the <see cref="TextEmphasis"/> class.
    /// </summary>
    /// <param name="emphasis">The emphasis style to apply.</param>
    /// <param name="regexPattern">The regular expression pattern to match keywords.</param>
    public TextEmphasis(Emphasis emphasis, string regexPattern)
    {
        this.Emphasis = emphasis;
        this.regex = new(regexPattern);
        // this.keywords = keywords;
    }

    /// <summary>
    /// Determines whether the specified word matches the emphasis keyword pattern.
    /// </summary>
    /// <param name="word">The word to check for emphasis.</param>
    /// <returns>True if the word matches the pattern; otherwise, false.</returns>
    public bool HasKeyword(string word)
    {
        return regex.IsMatch(word);
    }
}