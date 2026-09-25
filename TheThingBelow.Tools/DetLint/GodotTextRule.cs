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
        "AddItem",
        "AddIconItem",
        "AddCheckItem",
        "AddIconCheckItem",
        "AddRadioCheckItem",
        "AddIconRadioCheckItem",
        "AddSubmenuItem",
        "AddSubmenuNodeItem",
        "AddSeparator",
        "AppendText",
        "ParseBbcode",
    };

    /// <summary>
    /// The members that give an English word of the engine, such as the name `QuoteLeft` of a
    /// key. Game shows the name of a key through a string id (D-1128, finding P3-24).
    /// </summary>
    private static readonly HashSet<string> EngineWordCalls = new(StringComparer.Ordinal)
    {
        "GetKeycodeString",
    };

    /// <summary>
    /// The members that set a property by its name. A call with a name that holds `text` or
    /// `title` draws player text without a member of the words (D-614).
    /// </summary>
    private static readonly HashSet<string> NamedCalls = new(StringComparer.Ordinal)
    {
        "Set",
        "SetDeferred",
        "SetIndexed",
        "Call",
        "CallDeferred",
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

        if (!SymbolNames.IsInNamespace(symbol.ContainingType, GodotNamespace))
        {
            return [];
        }

        if (!DrawsText(symbol.Name) && !NamesTextProperty(node, symbol))
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
        return DrawCalls.Contains(name) || EngineWordCalls.Contains(name) || HoldsWord(name, "Text") || HoldsWord(name, "Title");
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

    /// <summary>
    /// Tells whether a call sets a property by a name that holds `text` or `title`, such as
    /// `Set("text", value)`. The name is a string literal, and the rule reads it without case.
    /// </summary>
    private static bool NamesTextProperty(SyntaxNode node, ISymbol symbol)
    {
        if (!NamedCalls.Contains(symbol.Name) || node is not InvocationExpressionSyntax call)
        {
            return false;
        }

        if (call.ArgumentList.Arguments.Count == 0
            || call.ArgumentList.Arguments[0].Expression is not LiteralExpressionSyntax literal
            || literal.Token.Value is not string name)
        {
            return false;
        }

        return name.Contains("text", StringComparison.OrdinalIgnoreCase)
            || name.Contains("title", StringComparison.OrdinalIgnoreCase);
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
