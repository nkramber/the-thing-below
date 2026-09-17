using Microsoft.CodeAnalysis;

namespace TheThingBelow.Tools.DetLint;

/// <summary>
/// The names that the rules match. Each name has no `global::` prefix and no type argument,
/// so `Dictionary&lt;string, int&gt;` gives `System.Collections.Generic.Dictionary`.
/// </summary>
public static class SymbolNames
{
    /// <summary>The name that `dynamic` gives, which no namespace holds.</summary>
    public const string DynamicType = "dynamic";

    private static readonly SymbolDisplayFormat NameFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.None);

    /// <summary>Gives the full name of a type, with no type argument.</summary>
    /// <param name="type">The type, or null when the compiler gave none.</param>
    /// <returns>The full name, and null for an absent type or an error type.</returns>
    public static string? FullName(ITypeSymbol? type)
    {
        if (type is null || type.TypeKind == TypeKind.Error)
        {
            return null;
        }

        return type.OriginalDefinition.ToDisplayString(NameFormat);
    }

    /// <summary>Gives the full name of a member, as the type name and the member name.</summary>
    /// <param name="symbol">The member, or null when the compiler gave none.</param>
    /// <returns>The full name, and null for a symbol that no type holds.</returns>
    public static string? MemberFullName(ISymbol? symbol)
    {
        if (symbol?.ContainingType is null)
        {
            return null;
        }

        string? typeName = FullName(symbol.ContainingType);
        return typeName is null ? null : $"{typeName}.{symbol.Name}";
    }

    /// <summary>Tells whether a namespace prefix holds a symbol.</summary>
    /// <param name="symbol">The symbol to read.</param>
    /// <param name="prefix">The namespace name, such as `System.Reflection`.</param>
    /// <returns>True when the namespace of the symbol is the prefix or is inside it.</returns>
    public static bool IsInNamespace(ISymbol? symbol, string prefix)
    {
        for (INamespaceSymbol? space = symbol?.ContainingNamespace; space is not null; space = space.ContainingNamespace)
        {
            if (space.IsGlobalNamespace)
            {
                return false;
            }

            if (space.ToDisplayString(NameFormat) == prefix)
            {
                return true;
            }
        }

        return false;
    }
}
