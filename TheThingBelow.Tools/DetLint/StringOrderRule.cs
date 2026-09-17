using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TheThingBelow.Tools.DetLint;

/// <summary>
/// DL 6: every string order in Core uses an ordinal comparison. The default order of .NET
/// follows the culture of the machine, and a new ICU version can change it (F-39, G-4).
/// The rule reads three shapes: a sorted collection with string keys and no comparer, a sort
/// or a comparison call with no ordinal comparison, and a comparer that follows a culture.
/// </summary>
public sealed class StringOrderRule : ILintRule
{
    private const string Reason =
        "The default string order of .NET follows the culture of the machine. Use an ordinal comparison (F-39, G-4, T-7).";

    private static readonly HashSet<string> SortedCollections = new(StringComparer.Ordinal)
    {
        "System.Collections.Generic.SortedDictionary",
        "System.Collections.Generic.SortedList",
        "System.Collections.Generic.SortedSet",
    };

    private static readonly HashSet<string> ComparerTypes = new(StringComparer.Ordinal)
    {
        "System.StringComparer",
        "System.Collections.Generic.IComparer",
        "System.Collections.Generic.IEqualityComparer",
    };

    private static readonly HashSet<string> OrderCalls = new(StringComparer.Ordinal)
    {
        "System.Linq.Enumerable.OrderBy",
        "System.Linq.Enumerable.OrderByDescending",
        "System.Linq.Enumerable.ThenBy",
        "System.Linq.Enumerable.ThenByDescending",
    };

    private static readonly HashSet<string> CultureComparisons = new(StringComparer.Ordinal)
    {
        "System.String.CompareTo",
        "System.String.Compare",
        "System.String.StartsWith",
        "System.String.EndsWith",
        "System.String.IndexOf",
        "System.String.LastIndexOf",
    };

    private static readonly HashSet<string> CultureMembers = new(StringComparer.Ordinal)
    {
        "System.StringComparer.CurrentCulture",
        "System.StringComparer.CurrentCultureIgnoreCase",
        "System.StringComparer.InvariantCulture",
        "System.StringComparer.InvariantCultureIgnoreCase",
        "System.StringComparison.CurrentCulture",
        "System.StringComparison.CurrentCultureIgnoreCase",
        "System.StringComparison.InvariantCulture",
        "System.StringComparison.InvariantCultureIgnoreCase",
    };

    /// <inheritdoc/>
    public string Id => "DL 6";

    /// <inheritdoc/>
    public IReadOnlyList<LintFinding> Check(SyntaxNode node, SemanticModel model, string path)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(model);

        return node switch
        {
            BaseObjectCreationExpressionSyntax creation => CheckSortedCollection(creation, model, path),
            InvocationExpressionSyntax call => CheckCall(call, model, path),
            MemberAccessExpressionSyntax access => CheckCultureMember(access, model, path),
            _ => [],
        };
    }

    /// <summary>Reads a new sorted collection with string keys, which needs a comparer.</summary>
    private static IReadOnlyList<LintFinding> CheckSortedCollection(
        BaseObjectCreationExpressionSyntax creation, SemanticModel model, string path)
    {
        if (model.GetTypeInfo(creation).Type is not INamedTypeSymbol type)
        {
            return [];
        }

        string? name = SymbolNames.FullName(type);
        if (name is null || !SortedCollections.Contains(name) || !HoldsStringKey(type))
        {
            return [];
        }

        if (creation.ArgumentList is not null && HoldsComparer(creation.ArgumentList, model))
        {
            return [];
        }

        return [LintFinding.At(path, creation, "DL 6", $"`{name}` with string keys takes no comparer here. {Reason}")];
    }

    /// <summary>Reads a sort call and a comparison call that follows the culture.</summary>
    private static IReadOnlyList<LintFinding> CheckCall(
        InvocationExpressionSyntax call, SemanticModel model, string path)
    {
        if (model.GetSymbolInfo(call).Symbol is not IMethodSymbol method)
        {
            return [];
        }

        IMethodSymbol target = method.ReducedFrom ?? method.OriginalDefinition;
        string? name = SymbolNames.MemberFullName(target);
        if (name is null)
        {
            return [];
        }

        if (NeedsOrdinalComparison(name, method, target))
        {
            return [LintFinding.At(path, call, "DL 6", $"`{name}` reads no ordinal comparison here. {Reason}")];
        }

        return [];
    }

    /// <summary>Tells whether a call orders or compares strings with no ordinal comparison.</summary>
    /// <param name="name">The full name of the method, with no type argument.</param>
    /// <param name="call">The method as the compiler resolved it, with its type arguments.</param>
    /// <param name="target">The method before the reduction of an extension call.</param>
    /// <returns>True when the call needs an ordinal comparison and takes none.</returns>
    private static bool NeedsOrdinalComparison(string name, IMethodSymbol call, IMethodSymbol target)
    {
        bool orderByOfStrings = OrderCalls.Contains(name)
            && call.TypeArguments is [_, { SpecialType: SpecialType.System_String }]
            && target.Parameters.Length == 2;
        bool orderOfStrings = (name == "System.Linq.Enumerable.Order" || name == "System.Linq.Enumerable.OrderDescending")
            && call.TypeArguments is [{ SpecialType: SpecialType.System_String }]
            && target.Parameters.Length == 1;
        bool listSortOfStrings = name == "System.Collections.Generic.List.Sort"
            && call.ContainingType.TypeArguments is [{ SpecialType: SpecialType.System_String }]
            && target.Parameters.Length == 0;
        bool arraySortOfStrings = name == "System.Array.Sort"
            && call.TypeArguments is [{ SpecialType: SpecialType.System_String }]
            && target.Parameters.Length == 1;
        bool comparisonOfStrings = CultureComparisons.Contains(name)
            && TakesString(target)
            && !TakesComparison(target);

        return orderByOfStrings || orderOfStrings || listSortOfStrings || arraySortOfStrings
            || comparisonOfStrings;
    }

    /// <summary>Reads a comparer or a comparison value that follows the culture.</summary>
    private static IReadOnlyList<LintFinding> CheckCultureMember(
        MemberAccessExpressionSyntax access, SemanticModel model, string path)
    {
        ISymbol? symbol = model.GetSymbolInfo(access).Symbol;
        string? name = SymbolNames.MemberFullName(symbol);
        if (name is null)
        {
            return [];
        }

        bool defaultComparerOfStrings = name == "System.Collections.Generic.Comparer.Default"
            && symbol?.ContainingType.TypeArguments is [{ SpecialType: SpecialType.System_String }];
        if (CultureMembers.Contains(name) || defaultComparerOfStrings)
        {
            return [LintFinding.At(path, access, "DL 6", $"`{name}` follows a culture. {Reason}")];
        }

        return [];
    }

    private static bool HoldsStringKey(INamedTypeSymbol type) =>
        type.TypeArguments.Length > 0 && type.TypeArguments[0].SpecialType == SpecialType.System_String;

    private static bool HoldsComparer(BaseArgumentListSyntax arguments, SemanticModel model)
    {
        foreach (ArgumentSyntax argument in arguments.Arguments)
        {
            string? name = SymbolNames.FullName(model.GetTypeInfo(argument.Expression).ConvertedType);
            if (name is not null && ComparerTypes.Contains(name))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TakesComparison(IMethodSymbol method)
    {
        foreach (IParameterSymbol parameter in method.Parameters)
        {
            if (SymbolNames.FullName(parameter.Type) == "System.StringComparison")
            {
                return true;
            }
        }

        return false;
    }

    private static bool TakesString(IMethodSymbol method)
    {
        foreach (IParameterSymbol parameter in method.Parameters)
        {
            if (parameter.Type.SpecialType == SpecialType.System_String)
            {
                return true;
            }
        }

        return false;
    }
}
