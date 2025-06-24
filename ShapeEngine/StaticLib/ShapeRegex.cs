using System.Text;
using System.Text.RegularExpressions;

namespace ShapeEngine.StaticLib;

/// <summary>
/// Provides utility methods for working with regular expressions, including pattern combination and common regex patterns.
/// </summary>
public static class ShapeRegex
{
    /// <summary>
    /// Combines two regex expressions into a single alternation group.
    /// </summary>
    /// <param name="expressionLeft">The left regex expression.</param>
    /// <param name="expressionRight">The right regex expression.</param>
    /// <returns>A combined regex pattern string.</returns>
    public static string Combine(string expressionLeft, string expressionRight) =>
        $"({expressionLeft})|({expressionRight})";
    /// <summary>
    /// Combines multiple regex expressions into a single alternation group.
    /// </summary>
    /// <param name="expressions">The regex expressions to combine.</param>
    /// <returns>A combined regex pattern string.</returns>
    public static string Combine(params string[] expressions)
    {
        var expression = new StringBuilder(expressions.Length * 5);

        for (var i = 0; i < expression.Length - 1; i++)
        {
            expression.Append($"({expression[i]})|");
        }

        expression.Append($"({expression[^1]})"); //last entry is different
        
        return expression.ToString();
    }
    /// <summary>
    /// Checks if any of the provided regex expressions match the input string.
    /// </summary>
    /// <param name="check">The string to check.</param>
    /// <param name="regexExpressions">The regex expressions to test.</param>
    /// <returns>True if any regex matches; otherwise, false.</returns>
    public static bool MatchMultipleRegexAny(string check, params Regex[] regexExpressions)
    {
        foreach (var regex in regexExpressions)
        {
            if (regex.IsMatch(check)) return true;
        }

        return false;
    }
    /// <summary>
    /// Checks if all of the provided regex expressions match the input string.
    /// </summary>
    /// <param name="check">The string to check.</param>
    /// <param name="regexExpressions">The regex expressions to test.</param>
    /// <returns>True if all regexes match; otherwise, false.</returns>
    public static bool MatchMultipleRegexAll(string check, params Regex[] regexExpressions)
    {
        foreach (var regex in regexExpressions)
        {
            if (!regex.IsMatch(check)) return false;
        }

        return true;
    }
    /// <summary>
    /// Returns a regex pattern that matches all uppercase letters (A-Z).
    /// </summary>
    public static string MatchAllCaps() => "[A-Z]+$";
    /// <summary>
    /// Returns a regex pattern that matches all uppercase letters in a specified range.
    /// </summary>
    /// <param name="uppercaseStart">The starting uppercase character.</param>
    /// <param name="uppercaseEnd">The ending uppercase character.</param>
    public static string MatchAllCaps(char uppercaseStart, char uppercaseEnd) => $"[{uppercaseStart}-{uppercaseEnd}]+$";
    /// <summary>
    /// Returns a regex pattern that matches any whitespace character.
    /// </summary>
    public static string MatchAnyWhitespaceCharacter() => "\\s";
    /// <summary>
    /// Returns a regex pattern that matches any non-whitespace character.
    /// </summary>
    public static string MatchAnyNoneWhitespaceCharacter() => "\\S+";
    /// <summary>
    /// Returns a regex pattern that matches any word character (alphanumeric or underscore).
    /// </summary>
    public static string MatchAnyWordCharacter() => "[a-zA-Z0-9_]";
    /// <summary>
    /// Returns a regex pattern that matches any non-word character.
    /// </summary>
    public static string MatchAnyNonWordCharacter() => "[^a-zA-Z0-9_]";
    /// <summary>
    /// Returns a regex pattern that matches any digit.
    /// </summary>
    public static string MatchAnyDigit() => "[0-9]";
    /// <summary>
    /// Returns a regex pattern that matches any non-digit character.
    /// </summary>
    public static string MatchAnyNoneDigit() => "\\D+";
    /// <summary>
    /// Returns a regex pattern that matches a specific character.
    /// </summary>
    /// <param name="c">The character to match.</param>
    public static string MatchChar(char c) => $"[{c}]";
    /// <summary>
    /// Returns a regex pattern that matches any character except the specified one.
    /// </summary>
    /// <param name="c">The character to exclude.</param>
    public static string MatchButChar(char c) => $"[^{c}]";
    /// <summary>
    /// Returns a regex pattern that matches any of the specified characters.
    /// </summary>
    /// <param name="chars">The characters to match.</param>
    public static string MatchChars(string chars)
    {
        var expression = new StringBuilder(chars.Length);
        foreach (var c in chars)
        {
            expression.Append(c);
        }

        return $"[{expression}]";
    }
    /// <summary>
    /// Returns a regex pattern that matches any character except those specified.
    /// </summary>
    /// <param name="chars">The characters to exclude.</param>
    public static string MatchButChars(string chars)
    {
        var expression = new StringBuilder(chars.Length);
        foreach (var c in chars)
        {
            expression.Append(c);
        }

        return $"[^{expression}]";
    }
    /// <summary>
    /// Returns a regex pattern that matches a specific word.
    /// </summary>
    /// <param name="word">The word to match.</param>
    public static string MatchWord(string word) => $"({word})";
    /// <summary>
    /// Returns a regex pattern that matches any character in the specified range.
    /// </summary>
    /// <param name="start">The start character of the range.</param>
    /// <param name="end">The end character of the range.</param>
    public static string MatchCharInRange(char start, char end) => $"[{start}-{end}]";
    /// <summary>
    /// Returns a regex pattern that matches any character except those in the specified range.
    /// </summary>
    /// <param name="start">The start character of the range.</param>
    /// <param name="end">The end character of the range.</param>
    public static string MatchButCharInRange(char start, char end) => $"[^{start}-{end}]";
    /// <summary>
    /// Returns a regex pattern that matches any character in the specified ranges.
    /// </summary>
    /// <param name="ranges">The character ranges to match.</param>
    public static string MatchCharsInRanges(params (char start, char end)[] ranges)
    {
        var expression = new StringBuilder(ranges.Length * 3);
        foreach (var range in ranges)
        {
            expression.Append($"{range.start}-{range.end}");
        }

        return $"[{expression}]";
    }
    /// <summary>
    /// Returns a regex pattern that matches any of the specified words.
    /// </summary>
    /// <param name="words">The words to match.</param>
    public static string MatchWords(params string[] words)
    {
        var expression = new StringBuilder(words.Length * 5);

        for (var i = 0; i < words.Length - 1; i++)
        {
            expression.Append($"({words[i]})|");
        }

        expression.Append($"({words[^1]})"); //last entry is different
        
        return expression.ToString();
    }
    /// <summary>
    /// Creates a Regex object from a pattern string.
    /// </summary>
    /// <param name="pattern">The regex pattern.</param>
    /// <returns>A Regex object.</returns>
    public static Regex CreateRegex(string pattern) => new(pattern);
    /// <summary>
    /// Creates a Regex object from multiple pattern expressions combined as alternations.
    /// </summary>
    /// <param name="expressions">The regex expressions to combine.</param>
    /// <returns>A Regex object.</returns>
    public static Regex CreateRegex(params string[] expressions) => new(Combine(expressions));
}