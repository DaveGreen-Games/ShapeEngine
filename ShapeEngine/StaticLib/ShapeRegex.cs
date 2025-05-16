using System.Text;
using System.Text.RegularExpressions;

namespace ShapeEngine.StaticLib;

public static class ShapeRegex
{
    public static string Combine(string expressionLeft, string expressionRight) =>
        $"({expressionLeft})|({expressionRight})";
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

    public static bool MatchMultipleRegexAny(string check, params Regex[] regexExpressions)
    {
        foreach (var regex in regexExpressions)
        {
            if (regex.IsMatch(check)) return true;
        }

        return false;
    }
    public static bool MatchMultipleRegexAll(string check, params Regex[] regexExpressions)
    {
        foreach (var regex in regexExpressions)
        {
            if (!regex.IsMatch(check)) return false;
        }

        return true;
    }

    public static string MatchAllCaps() => "[A-Z]+$";
    public static string MatchAllCaps(char uppercaseStart, char uppercaseEnd) => $"[{uppercaseStart}-{uppercaseEnd}]+$";
    
    public static string MatchAnyWhitespaceCharacter() => "\\s";
    public static string MatchAnyNoneWhitespaceCharacter() => "\\S+";
    public static string MatchAnyWordCharacter() => "[a-zA-Z0-9_]";
    public static string MatchAnyNonWordCharacter() => "[^a-zA-Z0-9_]";
    public static string MatchAnyDigit() => "[0-9]";
    public static string MatchAnyNoneDigit() => "\\D+";
    
    public static string MatchChar(char c) => $"[{c}]";
    public static string MatchButChar(char c) => $"[^{c}]";
    public static string MatchChars(string chars)
    {
        var expression = new StringBuilder(chars.Length);
        foreach (var c in chars)
        {
            expression.Append(c);
        }

        return $"[{expression.ToString()}]";
    }
    public static string MatchButChars(string chars)
    {
        var expression = new StringBuilder(chars.Length);
        foreach (var c in chars)
        {
            expression.Append(c);
        }

        return $"[^{expression.ToString()}]";
    }
    public static string MatchWord(string word) => $"({word})";
    public static string MatchCharInRange(char start, char end) => $"[{start}-{end}]";
    public static string MatchButCharInRange(char start, char end) => $"[^{start}-{end}]";
    public static string MatchCharsInRanges(params (char start, char end)[] ranges)
    {
        var expression = new StringBuilder(ranges.Length * 3);
        foreach (var range in ranges)
        {
            expression.Append($"{range.start}-{range.end}");
        }

        return $"[{expression}]";
    }

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
    
    public static Regex CreateRegex(string pattern) => new(pattern);
    public static Regex CreateRegex(params string[] expressions) => new(Combine(expressions));
}