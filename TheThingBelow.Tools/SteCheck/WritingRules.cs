using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TheThingBelow.Tools.SteCheck;

/// <summary>
/// The writing rules of ASD-STE100 that a machine can test (D-10). The `ste-writing` skill
/// holds the same table, rule for rule.
/// </summary>
public sealed class WritingRules
{
    /// <summary>The word limit of a sentence in a numbered list item (rule 5.1, D-604).</summary>
    public const int NumberedItemWordLimit = 20;

    /// <summary>The word limit of every other sentence (rule 6.3).</summary>
    public const int SentenceWordLimit = 25;

    /// <summary>The sentence limit of a paragraph (rule 6.6).</summary>
    public const int ParagraphSentenceLimit = 6;

    private const RegexOptions Options = RegexOptions.CultureInvariant | RegexOptions.Compiled;
    // A list item starts with a dash, a star, or a number and a point, then a space. A prose
    // line that starts with a number, such as a size, is not a list item.
    private static readonly Regex ListItem = new Regex(@"^\s*(-|\*|\d+\.)\s", Options);
    private static readonly Regex NumberedItem = new Regex(@"^\s*\d+\.", Options);

    private readonly string path;
    private readonly List<Finding> findings = [];
    private readonly List<(int Line, string Prose)> heldLines = [];
    private int paragraphSentences;
    private bool inCodeBlock;
    private bool inFrontMatter;

    private WritingRules(string path) => this.path = path;

    /// <summary>Reads one document and gives every writing finding of it.</summary>
    /// <param name="path">The path of the document, relative to the root of the checkout.</param>
    /// <param name="lines">Each line of the document, in order.</param>
    /// <returns>Each finding, in the order of the lines.</returns>
    public static IReadOnlyList<Finding> Check(string path, IReadOnlyList<string> lines)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(lines);

        WritingRules rules = new WritingRules(path);
        rules.ReadDocument(lines);
        return rules.findings;
    }

    private void ReadDocument(IReadOnlyList<string> lines)
    {
        int number = 0;
        for (int index = 0; index < lines.Count; index++)
        {
            number = index + 1;
            ReadLine(number, MarkdownText.Straighten(lines[index]));
        }

        // The last paragraph of a file ends with no empty line after it.
        EndParagraph(number);
    }

    private void ReadLine(int number, string line)
    {
        string trimmed = line.Trim();
        if (number == 1 && trimmed == "---")
        {
            inFrontMatter = true;
            return;
        }

        if (inFrontMatter)
        {
            inFrontMatter = trimmed != "---";
            return;
        }

        if (trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            inCodeBlock = !inCodeBlock;
            return;
        }

        // A table, a heading, and a fenced block carry no prose (the `ste-writing` skill).
        if (inCodeBlock
            || trimmed.StartsWith('|')
            || trimmed.StartsWith('#'))
        {
            return;
        }

        if (trimmed.Length == 0 || trimmed == ">")
        {
            EndParagraph(number);
            return;
        }

        // A block quote and a bold entry title each start a new paragraph.
        if (line.StartsWith('>') || line.StartsWith("**", StringComparison.Ordinal))
        {
            EndParagraph(number);
        }

        string prose = MarkdownText.ToProse(line);
        ReadComment(number, prose);
        ReadPunctuation(number, prose);
        ReadVerbForms(number, prose);

        if (ListItem.IsMatch(line))
        {
            // A list item is one unit, and it is not part of the paragraph count.
            FlushHeldLines();
            bool numbered = NumberedItem.IsMatch(line);
            int limit = numbered ? NumberedItemWordLimit : SentenceWordLimit;
            string rule = numbered ? "STE 5.1" : "STE 6.3";
            foreach (string sentence in MarkdownText.SplitSentences(prose))
            {
                ReadLength(number, sentence, limit, rule);
            }

            return;
        }

        heldLines.Add((number, prose));
    }

    /// <summary>
    /// A comment that spans lines hides its text from every rule, which F-11 found. The removal
    /// of a one-line comment leaves no marker, so a marker here names a comment across lines.
    /// </summary>
    private void ReadComment(int number, string prose)
    {
        if (prose.Contains("<!--", StringComparison.Ordinal)
            || prose.Contains("-->", StringComparison.Ordinal))
        {
            findings.Add(new Finding(
                path, number, "MD 1", "an HTML comment across lines. Keep each comment on one line (F-11)"));
        }
    }

    private void ReadPunctuation(int number, string prose)
    {
        if (prose.Contains(';', StringComparison.Ordinal))
        {
            findings.Add(new Finding(path, number, "STE 8.1", "semicolon"));
        }

        foreach (Match match in EnglishWords.Contraction.Matches(prose))
        {
            findings.Add(new Finding(path, number, "STE 4.2", $"contraction '{match.Value}'"));
        }
    }

    private void ReadVerbForms(int number, string prose)
    {
        foreach (Match match in EnglishWords.Modal.Matches(prose))
        {
            findings.Add(new Finding(path, number, "STE 3.2/3.4", $"modal verb '{match.Value}'"));
        }

        foreach (Match match in EnglishWords.Perfect.Matches(prose))
        {
            string word = match.Groups[2].Value;
            bool perfect = string.Equals(word, "been", StringComparison.OrdinalIgnoreCase)
                || (EnglishWords.IsParticiple(word) && !EnglishWords.IsStateAdjective(word));
            if (perfect)
            {
                findings.Add(new Finding(path, number, "STE 3.2/3.4", $"perfect tense '{match.Value}'"));
            }
        }

        foreach (Match match in EnglishWords.Passive.Matches(prose))
        {
            // "is run 2" names a run, and "is set 3" names a set. A number after the word is a name.
            string rest = prose[(match.Index + match.Length)..];
            string afterSpace = rest.TrimStart();
            if (rest.Length != afterSpace.Length && afterSpace.Length > 0 && char.IsDigit(afterSpace[0]))
            {
                continue;
            }

            string word = match.Groups[2].Value;
            if (EnglishWords.IsParticiple(word) && !EnglishWords.IsStateAdjective(word))
            {
                findings.Add(new Finding(path, number, "STE 3.6", $"passive voice '{match.Value}'"));
            }
        }

        foreach (Match match in EnglishWords.IngAfterWord.Matches(prose))
        {
            string word = match.Groups[2].Value;
            if (!EnglishWords.IsIngTechnicalName(word))
            {
                findings.Add(new Finding(path, number, "STE 3.5", $"-ing form '{match.Value}'"));
            }
        }

        foreach (string sentence in MarkdownText.SplitSentences(prose))
        {
            Match match = EnglishWords.IngAtStart.Match(sentence);
            if (match.Success && !EnglishWords.IsIngTechnicalName(match.Groups[1].Value))
            {
                findings.Add(new Finding(
                    path, number, "STE 3.5", $"-ing form starts a sentence '{match.Groups[1].Value}'"));
            }
        }
    }

    private void ReadLength(int number, string sentence, int limit, string rule)
    {
        int words = MarkdownText.CountWords(sentence);
        if (words > limit && !MarkdownText.IsNameList(sentence))
        {
            string start = sentence.Length > 70 ? sentence[..70] : sentence;
            findings.Add(new Finding(path, number, rule, $"{words} words: {start}..."));
        }
    }

    /// <summary>
    /// Joins the held lines of a paragraph and reads them as one text. A sentence can span
    /// hard-wrapped lines, so a wrap is not the end of a sentence.
    /// </summary>
    private void FlushHeldLines()
    {
        if (heldLines.Count == 0)
        {
            return;
        }

        int first = heldLines[0].Line;
        string joined = string.Join(' ', heldLines.ConvertAll(held => held.Prose));
        foreach (string sentence in MarkdownText.SplitSentences(joined))
        {
            ReadLength(first, sentence, SentenceWordLimit, "STE 6.3");
            paragraphSentences++;
        }

        heldLines.Clear();
    }

    private void EndParagraph(int number)
    {
        FlushHeldLines();
        if (paragraphSentences > ParagraphSentenceLimit)
        {
            findings.Add(new Finding(
                path, number, "STE 6.6", $"paragraph has {paragraphSentences} sentences"));
        }

        paragraphSentences = 0;
    }
}
