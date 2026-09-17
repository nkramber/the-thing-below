using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace TheThingBelow.Tools.DetLint;

/// <summary>
/// One rule of the lint. The scan gives each node of each syntax tree to each rule of the
/// scan, with the semantic model of the tree that holds the node.
/// </summary>
public interface ILintRule
{
    /// <summary>The id of the rule, such as `DL 1`.</summary>
    string Id { get; }

    /// <summary>Reads one node and gives each violation that the node holds.</summary>
    /// <param name="node">The node of the syntax tree.</param>
    /// <param name="model">The semantic model of the tree that holds the node.</param>
    /// <param name="path">The path of the file, for the finding.</param>
    /// <returns>Each violation of this rule on this node, and an empty list for a clean node.</returns>
    IReadOnlyList<LintFinding> Check(SyntaxNode node, SemanticModel model, string path);
}
