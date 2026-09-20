using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace TheThingBelow.Tools.DetLint;

/// <summary>
/// A rule that refuses a set of types, namespaces, and members. The rule reads the type that
/// the compiler gives each expression, and the type that the expression converts to, so a
/// literal with no suffix fails too, and so does a whole number that a call turns into a
/// float (D-498, F-38). `CoreRules` holds the six rules of this kind: the float types, the
/// clock, the OS random, the reflection, the hash paths, and the threads.
/// </summary>
public sealed class BannedSymbolRule : ILintRule
{
    private readonly HashSet<string> types;
    private readonly HashSet<string> namespaces;
    private readonly HashSet<string> members;
    private readonly HashSet<string> memberNames;
    private readonly string reason;

    /// <summary>Makes a rule from the names that it refuses.</summary>
    /// <param name="id">The id of the rule, such as `DL 1`.</param>
    /// <param name="reason">The sentence that each finding of this rule carries (T-2).</param>
    /// <param name="types">Full type names with no type argument, such as `System.Double`.</param>
    /// <param name="namespaces">Namespace names, such as `System.Reflection`.</param>
    /// <param name="members">Full member names, such as `System.Environment.TickCount`.</param>
    /// <param name="memberNames">Member names of any type, such as `GetHashCode`.</param>
    public BannedSymbolRule(
        string id,
        string reason,
        IReadOnlyCollection<string> types,
        IReadOnlyCollection<string> namespaces,
        IReadOnlyCollection<string> members,
        IReadOnlyCollection<string> memberNames)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        ArgumentException.ThrowIfNullOrEmpty(reason);
        Id = id;
        this.reason = reason;
        this.types = new HashSet<string>(types, StringComparer.Ordinal);
        this.namespaces = new HashSet<string>(namespaces, StringComparer.Ordinal);
        this.members = new HashSet<string>(members, StringComparer.Ordinal);
        this.memberNames = new HashSet<string>(memberNames, StringComparer.Ordinal);
    }

    /// <inheritdoc/>
    public string Id { get; }

    /// <inheritdoc/>
    public IReadOnlyList<LintFinding> Check(SyntaxNode node, SemanticModel model, string path)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(model);

        (string? typeName, string? convertedName) = ReadTypeNames(node, model);
        if (typeName is not null && types.Contains(typeName))
        {
            return [LintFinding.At(path, node, Id, $"the type `{typeName}` is here. {reason}")];
        }

        if (convertedName is not null && types.Contains(convertedName))
        {
            return [LintFinding.At(path, node, Id, $"the value converts to the type `{convertedName}` here. {reason}")];
        }

        ISymbol? symbol = model.GetSymbolInfo(node).Symbol;
        if (symbol is null)
        {
            return [];
        }

        foreach (string space in namespaces)
        {
            if (SymbolNames.IsInNamespace(symbol as ITypeSymbol ?? symbol.ContainingType, space))
            {
                return [LintFinding.At(path, node, Id, $"`{symbol.Name}` of the namespace `{space}` is here. {reason}")];
            }
        }

        if (memberNames.Contains(symbol.Name))
        {
            return [LintFinding.At(path, node, Id, $"the member `{symbol.Name}` is here. {reason}")];
        }

        string? memberName = SymbolNames.MemberFullName(symbol);
        if (memberName is not null && members.Contains(memberName))
        {
            return [LintFinding.At(path, node, Id, $"the member `{memberName}` is here. {reason}")];
        }

        return [];
    }

    /// <summary>
    /// Reads the type of an expression and the type that it converts to. A whole number that
    /// a call takes as a `double` converts, and the second name reads that (F-38).
    /// </summary>
    private static (string? Type, string? Converted) ReadTypeNames(SyntaxNode node, SemanticModel model)
    {
        if (node is not Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
        {
            return (null, null);
        }

        TypeInfo info = model.GetTypeInfo(expression);
        return (SymbolNames.FullName(info.Type), SymbolNames.FullName(info.ConvertedType));
    }
}
