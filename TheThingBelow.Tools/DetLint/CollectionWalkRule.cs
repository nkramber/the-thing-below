using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TheThingBelow.Tools.DetLint;

/// <summary>
/// DL 7: no walk of a `Dictionary` or a `HashSet` in Core. The order of a walk reaches the
/// state, and neither type gives a fixed order (G-4, T-7). A lookup by key stays legal, so
/// Core keeps its fast lookups (D-615). The rule reads three shapes: a `foreach` over the
/// collection, a read of `Keys` or `Values`, and a call of a LINQ method on the collection.
/// A walk inside a method that takes the collection as a parameter stays a matter for the
/// review, because the rule reads one expression at a time (D-615).
/// </summary>
public sealed class CollectionWalkRule : ILintRule
{
    private const string Reason =
        "Neither type gives a fixed order. Use a `List` or a `SortedDictionary` where the order reaches the state (G-4, T-7, D-615).";

    private static readonly HashSet<string> WalkedCollections = new(StringComparer.Ordinal)
    {
        "System.Collections.Generic.Dictionary",
        "System.Collections.Generic.HashSet",
    };

    private static readonly HashSet<string> QueryTypes = new(StringComparer.Ordinal)
    {
        "System.Linq.Enumerable",
        "System.Linq.Queryable",
    };

    /// <inheritdoc/>
    public string Id => "DL 7";

    /// <inheritdoc/>
    public IReadOnlyList<LintFinding> Check(SyntaxNode node, SemanticModel model, string path)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(model);

        return node switch
        {
            ForEachStatementSyntax loop => CheckLoop(loop, model, path),
            MemberAccessExpressionSyntax access => CheckKeysOrValues(access, model, path),
            InvocationExpressionSyntax call => CheckQuery(call, model, path),
            _ => [],
        };
    }

    /// <summary>Reads a `foreach` over a collection with no fixed order.</summary>
    private static IReadOnlyList<LintFinding> CheckLoop(
        ForEachStatementSyntax loop, SemanticModel model, string path)
    {
        string? name = SymbolNames.FullName(model.GetTypeInfo(loop.Expression).Type);
        if (name is null || !WalkedCollections.Contains(name))
        {
            return [];
        }

        return [LintFinding.At(path, loop, "DL 7", $"this `foreach` walks a `{name}`. {Reason}")];
    }

    /// <summary>Reads the `Keys` and the `Values` of a dictionary, which are two walks.</summary>
    private static IReadOnlyList<LintFinding> CheckKeysOrValues(
        MemberAccessExpressionSyntax access, SemanticModel model, string path)
    {
        ISymbol? symbol = model.GetSymbolInfo(access).Symbol;
        string? name = SymbolNames.MemberFullName(symbol);
        if (name != "System.Collections.Generic.Dictionary.Keys"
            && name != "System.Collections.Generic.Dictionary.Values")
        {
            return [];
        }

        return [LintFinding.At(path, access, "DL 7", $"`{name}` walks the whole collection. {Reason}")];
    }

    /// <summary>Reads a LINQ call over a collection with no fixed order.</summary>
    private static IReadOnlyList<LintFinding> CheckQuery(
        InvocationExpressionSyntax call, SemanticModel model, string path)
    {
        if (model.GetSymbolInfo(call).Symbol is not IMethodSymbol method)
        {
            return [];
        }

        string? queryType = SymbolNames.FullName(method.ContainingType);
        if (queryType is null || !QueryTypes.Contains(queryType))
        {
            return [];
        }

        string? sourceName = SymbolNames.FullName(ReadSource(call, method, model));
        if (sourceName is null || !WalkedCollections.Contains(sourceName))
        {
            return [];
        }

        return
        [
            LintFinding.At(path, call, "DL 7", $"`{queryType}.{method.Name}` walks a `{sourceName}`. {Reason}"),
        ];
    }

    private static ITypeSymbol? ReadSource(
        InvocationExpressionSyntax call, IMethodSymbol method, SemanticModel model)
    {
        if (method.ReducedFrom is not null)
        {
            return method.ReceiverType;
        }

        ArgumentSyntax? first = call.ArgumentList.Arguments.Count > 0 ? call.ArgumentList.Arguments[0] : null;
        return first is null ? null : model.GetTypeInfo(first.Expression).Type;
    }
}
