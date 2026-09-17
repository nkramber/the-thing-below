using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TheThingBelow.Tools.SteCheck;

/// <summary>
/// Turns one line of Markdown into the prose that the grammar rules read. The counting rules
/// of the standard make a quotation, a parenthesis, a link, and a code span one word each
/// (rules 8.5, 8.6).
/// </summary>
public static class MarkdownText
{
    private const RegexOptions Options = RegexOptions.CultureInvariant | RegexOptions.Compiled;

    private static readonly Regex OneLineComment = new Regex(@"<!--.*?-->", Options);
    private static readonly Regex CodeSpan = new Regex(@"`[^`]*`", Options);
    private static readonly Regex Link = new Regex(@"\[([^\]]*)\]\([^)]*\)", Options);
    private static readonly Regex Url = new Regex(@"https?://\S+", Options);
    private static readonly Regex Quotation = new Regex("\"[^\"]*\"", Options);
    private static readonly Regex Parenthesis = new Regex(@"\([^)]*\)", Options);
    private static readonly Regex Emphasis = new Regex(@"[*_>#]+", Options);
    private static readonly Regex SentenceBreak = new Regex(@"(?<=[.!?])\s+(?=[A-Z0-9""'(])", Options);
    private static readonly char[] WordBreaks = [' ', '\t'];

    /// <summary>Replaces each curly quotation mark and apostrophe with the straight form.</summary>
    /// <param name="line">One line of the document.</param>
    /// <returns>The line with straight marks alone.</returns>
    /// <remarks>A curly apostrophe hides a contraction from rule 4.2.</remarks>
    public static string Straighten(string line)
    {
        ArgumentNullException.ThrowIfNull(line);
        return line
            .Replace('’', '\'')
            .Replace('‘', '\'')
            .Replace('“', '"')
            .Replace('”', '"');
    }

    /// <summary>Removes the Markdown markup and shortens each construct that counts as one word.</summary>
    /// <param name="line">One line of the document, with straight marks.</param>
    /// <returns>The prose of the line, for the grammar rules and the length rules.</returns>
    public static string ToProse(string line)
    {
        ArgumentNullException.ThrowIfNull(line);
        string prose = OneLineComment.Replace(line, string.Empty);
        prose = CodeSpan.Replace(prose, "X");
        prose = Link.Replace(prose, "$1");
        prose = Url.Replace(prose, "URL");
        prose = Quotation.Replace(prose, "QUOTE");
        prose = Parenthesis.Replace(prose, "(X)");
        return Emphasis.Replace(prose, string.Empty);
    }

    /// <summary>Cuts prose into sentences. A period, a question mark, and an exclamation mark end one.</summary>
    /// <param name="prose">The prose of one line, or of a paragraph.</param>
    /// <returns>Each sentence, with no empty entry.</returns>
    public static IReadOnlyList<string> SplitSentences(string prose)
    {
        ArgumentNullException.ThrowIfNull(prose);
        List<string> sentences = [];
        foreach (string part in SentenceBreak.Split(prose))
        {
            string sentence = part.Trim();
            if (sentence.Length > 0)
            {
                sentences.Add(sentence);
            }
        }

        return sentences;
    }

    /// <summary>Counts the words of a sentence, as rule 8.6 counts them.</summary>
    /// <param name="sentence">One sentence of prose.</param>
    /// <returns>The number of words.</returns>
    public static int CountWords(string sentence)
    {
        ArgumentNullException.ThrowIfNull(sentence);
        return sentence.Split(WordBreaks, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    /// <summary>
    /// Reads a run of short comma-separated items as a list of names, not as a sentence.
    /// Rules 4.3 and 8.6 keep such a list out of the length rules.
    /// </summary>
    /// <param name="sentence">One sentence of prose.</param>
    /// <returns>True when the sentence is a list of names.</returns>
    public static bool IsNameList(string sentence)
    {
        ArgumentNullException.ThrowIfNull(sentence);
        string[] items = sentence.Split(',');
        if (items.Length < 6)
        {
            return false;
        }

        int words = 0;
        foreach (string item in items)
        {
            words += CountWords(item.Trim());
        }

        return words <= 3 * items.Length;
    }
}
