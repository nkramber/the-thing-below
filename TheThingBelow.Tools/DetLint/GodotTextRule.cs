using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TheThingBelow.Tools.DetLint;

/// <summary>
/// DL 8: Game puts each player string on screen through one text helper, which takes a string
/// table id (G-7, D-499). The rule fails each member of a Godot type whose name holds the word
/// `Text` or the word `Title`, and each member of the committed list of D-614. Code inside the
/// text helper takes no finding.
/// </summary>
public sealed class GodotTextRule : ILintRule
{
    /// <summary>
    /// The one text helper of Game, which PR-61 writes (D-524, D-561). The lint holds the name
    /// until then, and PR-61 confirms it or changes this constant (G-16).
    /// </summary>
    public const string TextHelperType = "TheThingBelow.Game.Ui.TextHelper";

    /// <summary>The namespace of every Godot type, from the assembly of the Game build.</summary>
    public const string GodotNamespace = "Godot";

    private const string Reason =
        $"Game draws each player string through `{TextHelperType}`, which takes a string table id (G-7, D-499, D-614).";

    private static readonly HashSet<string> DrawCalls = new(StringComparer.Ordinal)
    {
        "DrawChar",
        "DrawCharOutline",
        "DrawMultilineString",
        "DrawMultilineStringOutline",
        "DrawString",
        "DrawStringOutline",
    };

    /// <inheritdoc/>
    public string Id => "DL 8";

    /// <inheritdoc/>
    public IReadOnlyList<LintFinding> Check(SyntaxNode node, SemanticModel model, string path)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(model);

        ISymbol? symbol = model.GetSymbolInfo(node).Symbol;
        if (symbol is not (IPropertySymbol or IMethodSymbol or IFieldSymbol))
        {
            return [];
        }

        if (!SymbolNames.IsInNamespace(symbol.ContainingType, GodotNamespace) || !DrawsText(symbol.Name))
        {
            return [];
        }

        if (IsInTextHelper(node, model))
        {
            return [];
        }

        string? name = SymbolNames.MemberFullName(symbol);
        return [LintFinding.At(path, node, "DL 8", $"`{name}` draws player text. {Reason}")];
    }

    /// <summary>Tells whether a member name holds the word `Text` or the word `Title`.</summary>
    /// <param name="name">The name of the member.</param>
    /// <returns>True for a text member, and false for a name such as `Texture`.</returns>
    public static bool DrawsText(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        return DrawCalls.Contains(name) || HoldsWord(name, "Text") || HoldsWord(name, "Title");
    }

    /// <summary>
    /// Tells whether a name holds a word. A lowercase letter after the word makes a longer
    /// word, so `Texture` does not hold the word `Text`.
    /// </summary>
    private static bool HoldsWord(string name, string word)
    {
        for (int start = name.IndexOf(word, StringComparison.Ordinal);
             start >= 0;
             start = name.IndexOf(word, start + 1, StringComparison.Ordinal))
        {
            int after = start + word.Length;
            if (after >= name.Length || !char.IsLower(name[after]))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsInTextHelper(SyntaxNode node, SemanticModel model)
    {
        foreach (SyntaxNode parent in node.Ancestors())
        {
            if (parent is BaseTypeDeclarationSyntax declaration
                && SymbolNames.FullName(model.GetDeclaredSymbol(declaration) as ITypeSymbol) == TextHelperType)
            {
                return true;
            }
        }

        return false;
    }
}
