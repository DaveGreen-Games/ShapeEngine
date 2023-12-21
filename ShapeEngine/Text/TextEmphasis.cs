using System.Text.RegularExpressions;

namespace ShapeEngine.Text;

public class TextEmphasis
{
    // private readonly string[] keywords;
    private readonly Regex regex;
    public readonly Emphasis Emphasis;

    //params string[] keywords
    public TextEmphasis(Emphasis emphasis, string regexPattern)
    {
        this.Emphasis = emphasis;
        this.regex = new(regexPattern);
        // this.keywords = keywords;
    }

    public bool HasKeyword(string word)
    {
        return regex.IsMatch(word);

        // if (keywords.Length <= 0) return false;
        // var rx = new Regex("\\d", RegexOptions.IgnoreCase);

        // var isMatch = rx.IsMatch(word);

        // return isMatch || keywords.Contains(word);
    }
}