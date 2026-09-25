using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace TheThingBelow.Tools.DetLint;

/// <summary>One source file of a scan.</summary>
/// <param name="Path">The path of the file, relative to the root of the checkout.</param>
/// <param name="Text">The C# text of the file.</param>
public sealed record ScanSource(string Path, string Text);

/// <summary>
/// One compilation of a set of source files, and the rules that read it. The scan reads the
/// type that the compiler gives each expression, so a literal with no suffix fails too
/// (D-498, F-38).
/// </summary>
public static class SourceScan
{
    /// <summary>The id of the rule that reports a compilation error (T-2).</summary>
    public const string CompileRule = "DL 0";

    /// <summary>
    /// The id of the rule that reports text that a directive disables, such as a block under
    /// `#if DEBUG`. The compiler reads such a block as text alone, so no other rule reads it.
    /// </summary>
    public const string DisabledTextRule = "DL 12";

    /// <summary>
    /// The symbols that `Directory.Build.props` defines for every project. The scan parses
    /// with the same symbols, so a block behind `#if` takes the rules too. A test reads the
    /// props file and fails when the two lists differ (T-2).
    /// </summary>
    public static readonly IReadOnlyList<string> PreprocessorSymbols = ["CONTRACTS_FULL"];

    /// <summary>Compiles the sources and gives each finding of each rule.</summary>
    /// <param name="assemblyName">The name of the compilation, for the compiler.</param>
    /// <param name="sources">The source files of the scan.</param>
    /// <param name="references">The assemblies that the sources compile against.</param>
    /// <param name="rules">The rules that read each node of each file.</param>
    /// <returns>
    /// Each compilation error, or each finding of the rules when the compilation has no error.
    /// A compilation with an error gives no rule finding, because the types are not complete.
    /// </returns>
    public static IReadOnlyList<LintFinding> Check(
        string assemblyName,
        IReadOnlyList<ScanSource> sources,
        IReadOnlyList<MetadataReference> references,
        IReadOnlyList<ILintRule> rules)
    {
        ArgumentException.ThrowIfNullOrEmpty(assemblyName);
        ArgumentNullException.ThrowIfNull(sources);
        ArgumentNullException.ThrowIfNull(references);
        ArgumentNullException.ThrowIfNull(rules);

        List<SyntaxTree> trees = [];
        foreach (ScanSource source in sources)
        {
            trees.Add(CSharpSyntaxTree.ParseText(
                source.Text,
                new CSharpParseOptions(LanguageVersion.Latest, preprocessorSymbols: PreprocessorSymbols),
                source.Path));
        }

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName,
            trees,
            references,
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));

        List<LintFinding> errors = ReadCompileErrors(compilation);
        if (errors.Count > 0)
        {
            return Sorted(errors);
        }

        List<LintFinding> findings = [];
        foreach (SyntaxTree tree in trees)
        {
            findings.AddRange(ReadDisabledText(tree));
            SemanticModel model = compilation.GetSemanticModel(tree);
            foreach (SyntaxNode node in tree.GetRoot().DescendantNodesAndSelf())
            {
                foreach (ILintRule rule in rules)
                {
                    findings.AddRange(rule.Check(node, model, tree.FilePath));
                }
            }
        }

        return Sorted(findings);
    }

    /// <summary>
    /// Gives one finding for each block of text that a directive disables. The scan defines
    /// <see cref="PreprocessorSymbols"/> alone, so a block under another symbol, under a
    /// negation, or under `#else` holds code that no rule reads (G-1, T-2).
    /// </summary>
    private static List<LintFinding> ReadDisabledText(SyntaxTree tree)
    {
        List<LintFinding> findings = [];
        foreach (SyntaxTrivia trivia in tree.GetRoot().DescendantTrivia())
        {
            if (!trivia.IsKind(SyntaxKind.DisabledTextTrivia))
            {
                continue;
            }

            int line = trivia.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            findings.Add(new LintFinding(
                tree.FilePath,
                line,
                DisabledTextRule,
                $"a directive disables this text, so no rule of det-lint reads it. The scan defines {string.Join(", ", PreprocessorSymbols)} alone. Remove the directive (G-1, T-2)."));
        }

        return findings;
    }

    private static List<LintFinding> ReadCompileErrors(CSharpCompilation compilation)
    {
        List<LintFinding> errors = [];
        foreach (Diagnostic diagnostic in compilation.GetDiagnostics())
        {
            if (diagnostic.Severity != DiagnosticSeverity.Error)
            {
                continue;
            }

            FileLinePositionSpan span = diagnostic.Location.GetLineSpan();
            errors.Add(new LintFinding(
                span.Path.Length > 0 ? span.Path : compilation.AssemblyName ?? "the compilation",
                span.StartLinePosition.Line + 1,
                CompileRule,
                $"det-lint could not compile this code, so no rule read it: {diagnostic.Id}: {diagnostic.GetMessage()} (T-2)."));
        }

        return errors;
    }

    /// <summary>
    /// Sorts the findings by file, line, and rule, and gives one finding for each line of each
    /// rule. One line can hold the type and its value, and the reader needs one line alone.
    /// </summary>
    private static List<LintFinding> Sorted(List<LintFinding> findings)
    {
        findings.Sort(Compare);
        List<LintFinding> one = [];
        foreach (LintFinding finding in findings)
        {
            LintFinding? last = one.Count > 0 ? one[^1] : null;
            if (last is not null && last.File == finding.File && last.Line == finding.Line
                && last.Rule == finding.Rule)
            {
                continue;
            }

            one.Add(finding);
        }

        return one;
    }

    private static int Compare(LintFinding left, LintFinding right)
    {
        int byFile = string.CompareOrdinal(left.File, right.File);
        if (byFile != 0)
        {
            return byFile;
        }

        int byLine = left.Line.CompareTo(right.Line);
        return byLine != 0 ? byLine : string.CompareOrdinal(left.Rule, right.Rule);
    }
}
