using System;
using Microsoft.CodeAnalysis;

namespace TheThingBelow.Tools.DetLint;

/// <summary>One violation of a lint rule, in one file, on one line.</summary>
/// <param name="File">The path of the file, relative to the root of the checkout.</param>
/// <param name="Line">The 1-based number of the line that holds the violation.</param>
/// <param name="Rule">The id of the rule, from `DL 0` to `DL 9`.</param>
/// <param name="Detail">What the rule saw, for the reader who must correct the line (T-2).</param>
public sealed record LintFinding(string File, int Line, string Rule, string Detail)
{
    /// <summary>Makes a finding on the line that holds the start of a node.</summary>
    /// <param name="path">The path of the file that holds the node.</param>
    /// <param name="node">The node that the rule read.</param>
    /// <param name="rule">The id of the rule.</param>
    /// <param name="detail">What the rule saw.</param>
    /// <returns>The finding, with the 1-based line number of the node.</returns>
    public static LintFinding At(string path, SyntaxNode node, string rule, string detail)
    {
        ArgumentNullException.ThrowIfNull(node);
        int line = node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
        return new LintFinding(path, line, rule, detail);
    }

    /// <summary>Gives the one output line of this finding.</summary>
    /// <returns>The path, the line number, the rule id, and the detail.</returns>
    public override string ToString() => $"{File}:{Line}: rule {Rule}: {Detail}";
}
