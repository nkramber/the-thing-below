using System.Collections.Generic;
using TheThingBelow.Tools.DetLint;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The rules that Core takes: DL 1 to DL 7. Each test gives one fixture file that breaks one
/// rule, and one test gives a file that breaks none (G-16, D-496).
/// </summary>
public sealed class DetLintCoreRuleTests
{
    [Fact]
    public void ADoubleInCoreFails()
    {
        IReadOnlyList<LintFinding> findings = DetLintFixture.CheckCore(
            """
            namespace TheThingBelow.Core;
            public static class Fixture
            {
                public static double Rate() => 2;
            }
            """);

        LintFinding finding = Assert.Single(findings);
        Assert.Equal("DL 1", finding.Rule);
        Assert.Equal(DetLintFixture.CorePath, finding.File);
        Assert.Equal(4, finding.Line);
        Assert.Contains("System.Double", finding.Detail, System.StringComparison.Ordinal);
    }

    [Fact]
    public void ARealLiteralWithNoKeywordFails()
    {
        // A real literal with no suffix is a double, so a scan of words misses it (F-38).
        IReadOnlyList<LintFinding> findings = DetLintFixture.CheckCore(
            """
            namespace TheThingBelow.Core;
            public static class Fixture
            {
                public static object Rate()
                {
                    var rate = 0.5;
                    return rate;
                }
            }
            """);

        Assert.All(findings, finding => Assert.Equal("DL 1", finding.Rule));
        Assert.Equal(6, findings[0].Line);
    }

    [Theory]
    [InlineData("System.DateTime.UtcNow.Ticks", "DL 2")]
    [InlineData("System.Environment.TickCount", "DL 2")]
    [InlineData("System.Diagnostics.Stopwatch.GetTimestamp()", "DL 2")]
    [InlineData("new System.Random(1).Next()", "DL 3")]
    [InlineData("System.Guid.NewGuid().GetHashCode()", "DL 3")]
    [InlineData("typeof(Fixture).GetProperties().Length", "DL 4")]
    [InlineData("System.Activator.CreateInstance<int>()", "DL 4")]
    [InlineData("System.Text.Json.JsonSerializer.Serialize(1).Length", "DL 4")]
    [InlineData("\"a\".GetHashCode()", "DL 5")]
    [InlineData("System.Security.Cryptography.SHA256.HashData([]).Length", "DL 5")]
    [InlineData("System.IO.File.ReadAllText(\"x\").Length", "DL 11")]
    [InlineData("System.IO.Path.Combine(\"a\", \"b\").Length", "DL 11")]
    [InlineData("System.Environment.ProcessorCount", "DL 11")]
    [InlineData("System.Environment.NewLine.Length", "DL 11")]
    [InlineData("System.Environment.GetEnvironmentVariable(\"HOME\")!.Length", "DL 11")]
    [InlineData("System.OperatingSystem.IsWindows() ? 1 : 0", "DL 11")]
    [InlineData("(long)System.Runtime.InteropServices.RuntimeInformation.OSArchitecture", "DL 11")]
    [InlineData("System.AppContext.BaseDirectory.Length", "DL 11")]
    [InlineData("new System.Text.StringBuilder().AppendLine(\"a\").Length", "DL 11")]
    [InlineData("new System.Net.Http.HttpClient().Timeout.Ticks", "DL 11")]
    public void AForbiddenPathInCoreFails(string expression, string rule)
    {
        IReadOnlyList<LintFinding> findings = DetLintFixture.CheckCore(
            $$"""
            namespace TheThingBelow.Core;
            public static class Fixture
            {
                public static long Value()
                {
                    return {{expression}};
                }
            }
            """);

        Assert.Contains(rule, DetLintFixture.RuleIds(findings));
        Assert.All(findings, finding => Assert.Equal(6, finding.Line));
    }

    [Fact]
    public void AHandReaderOnUtf8JsonReaderInCorePasses()
    {
        // The content reader of D-647 is legal, and `JsonSerializer` above is not. The two
        // types live in one namespace, so the rule names the type and never the namespace.
        IReadOnlyList<LintFinding> findings = DetLintFixture.CheckCore(
            """
            using System.Text.Json;
            namespace TheThingBelow.Core;
            public static class Fixture
            {
                public static bool Read(System.ReadOnlySpan<byte> bytes)
                {
                    var reader = new Utf8JsonReader(bytes);
                    return reader.Read();
                }
            }
            """);

        Assert.Empty(findings);
    }

    [Fact]
    public void AReflectionTypeInCoreNamesTheNamespace()
    {
        IReadOnlyList<LintFinding> findings = DetLintFixture.CheckCore(
            """
            using System.Reflection;
            namespace TheThingBelow.Core;
            public static class Fixture
            {
                public static Assembly Self() => typeof(Fixture).Assembly;
            }
            """);

        Assert.Equal(["DL 4"], DetLintFixture.RuleIds(findings));
        Assert.Contains("System.Reflection", findings[0].Detail, System.StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("new System.Collections.Generic.SortedDictionary<string, int>().Count")]
    [InlineData("\"a\".CompareTo(\"b\")")]
    [InlineData("System.StringComparer.InvariantCulture.Compare(\"a\", \"b\")")]
    [InlineData("System.Collections.Generic.Comparer<string>.Default.Compare(\"a\", \"b\")")]
    [InlineData("System.Linq.Enumerable.Count(System.Linq.Enumerable.OrderBy(new string[0], word => word))")]
    public void AStringOrderWithNoOrdinalComparisonFails(string expression)
    {
        IReadOnlyList<LintFinding> findings = DetLintFixture.CheckCore(
            $$"""
            namespace TheThingBelow.Core;
            public static class Fixture
            {
                public static int Value()
                {
                    return {{expression}};
                }
            }
            """);

        Assert.Contains("DL 6", DetLintFixture.RuleIds(findings));
    }

    [Fact]
    public void ASortedDictionaryWithAnOrdinalComparerPasses()
    {
        IReadOnlyList<LintFinding> findings = DetLintFixture.CheckCore(
            """
            using System;
            using System.Collections.Generic;
            namespace TheThingBelow.Core;
            public static class Fixture
            {
                public static int Count()
                {
                    SortedDictionary<string, int> values = new(StringComparer.Ordinal);
                    return values.Count;
                }
            }
            """);

        Assert.Empty(findings);
    }

    [Theory]
    [InlineData("foreach (var entry in values) { count += entry.Value; }")]
    [InlineData("foreach (var key in values.Keys) { count += key.Length; }")]
    [InlineData("count += System.Linq.Enumerable.Count(values);")]
    public void AWalkOfADictionaryInCoreFails(string statement)
    {
        IReadOnlyList<LintFinding> findings = DetLintFixture.CheckCore(
            $$"""
            using System.Collections.Generic;
            namespace TheThingBelow.Core;
            public static class Fixture
            {
                public static int Count(Dictionary<string, int> values)
                {
                    int count = 0;
                    {{statement}}
                    return count;
                }
            }
            """);

        Assert.Contains("DL 7", DetLintFixture.RuleIds(findings));
    }

    /// <summary>A field or a parameter of an interface type hides the collection behind it.</summary>
    [Fact]
    public void AWalkOfADictionaryInterfaceInCoreFails()
    {
        IReadOnlyList<LintFinding> findings = DetLintFixture.CheckCore(
            """
            using System.Collections.Generic;
            namespace TheThingBelow.Core;
            public static class Fixture
            {
                public static int Sum(IReadOnlyDictionary<string, int> values)
                {
                    int total = 0;
                    foreach (KeyValuePair<string, int> pair in values)
                    {
                        total += pair.Value;
                    }

                    return total;
                }
            }
            """);

        Assert.Contains("DL 7", DetLintFixture.RuleIds(findings));
    }

    [Fact]
    public void ATaskInCoreFails()
    {
        IReadOnlyList<LintFinding> findings = DetLintFixture.CheckCore(
            """
            using System.Threading.Tasks;
            namespace TheThingBelow.Core;
            public static class Fixture
            {
                public static Task Wait() => Task.Delay(1);
            }
            """);

        Assert.Contains("DL 10", DetLintFixture.RuleIds(findings));
    }

    [Fact]
    public void AVectorInCoreFails()
    {
        IReadOnlyList<LintFinding> findings = DetLintFixture.CheckCore(
            """
            using System.Numerics;
            namespace TheThingBelow.Core;
            public static class Fixture
            {
                public static int Lanes() => Vector<int>.Count;
            }
            """);

        Assert.Contains("DL 10", DetLintFixture.RuleIds(findings));
    }

    /// <summary>A whole number that a call takes as a `double` converts, and DL 1 reads the conversion (F-38).</summary>
    [Fact]
    public void AWholeNumberThatACallConvertsToADoubleFails()
    {
        // The constructor of `Complex` takes two `double` values, and the type itself is on no
        // list, so the conversion of the two whole numbers is the one finding.
        IReadOnlyList<LintFinding> findings = DetLintFixture.CheckCore(
            """
            namespace TheThingBelow.Core;
            public static class Fixture
            {
                public static System.Numerics.Complex Point() => new System.Numerics.Complex(1, 2);
            }
            """);

        Assert.Contains("DL 1", DetLintFixture.RuleIds(findings));
    }

    [Fact]
    public void AMinOfStringsWithNoComparerInCoreFails()
    {
        IReadOnlyList<LintFinding> findings = DetLintFixture.CheckCore(
            """
            using System.Collections.Generic;
            using System.Linq;
            namespace TheThingBelow.Core;
            public static class Fixture
            {
                public static string? First(List<string> names) => names.Min();
            }
            """);

        Assert.Contains("DL 6", DetLintFixture.RuleIds(findings));
    }

    [Fact]
    public void AnArraySortOfStringKeysWithNoComparerInCoreFails()
    {
        IReadOnlyList<LintFinding> findings = DetLintFixture.CheckCore(
            """
            using System;
            namespace TheThingBelow.Core;
            public static class Fixture
            {
                public static void Order(string[] keys, int[] items) => Array.Sort(keys, items);
            }
            """);

        Assert.Contains("DL 6", DetLintFixture.RuleIds(findings));
    }

    /// <summary>The scan parses with the symbols of the build, so a block behind `#if` takes the rules.</summary>
    [Fact]
    public void ABlockBehindABuildSymbolTakesTheRules()
    {
        IReadOnlyList<LintFinding> findings = DetLintFixture.CheckCore(
            """
            namespace TheThingBelow.Core;
            public static class Fixture
            {
            #if CONTRACTS_FULL
                public static double Rate() => 2;
            #endif
            }
            """);

        Assert.Contains("DL 1", DetLintFixture.RuleIds(findings));
    }

    [Fact]
    public void TheBuildPropsDefineEverySymbolOfTheScan()
    {
        string props = System.IO.File.ReadAllText(RepositoryRoot.PathTo("Directory.Build.props"));

        foreach (string symbol in SourceScan.PreprocessorSymbols)
        {
            Assert.Contains(symbol, props, System.StringComparison.Ordinal);
        }
    }

    [Fact]
    public void ALookupByKeyInCorePasses()
    {
        // D-615 keeps the fast lookup legal, and it fails the walk alone.
        IReadOnlyList<LintFinding> findings = DetLintFixture.CheckCore(
            """
            using System.Collections.Generic;
            namespace TheThingBelow.Core;
            public static class Fixture
            {
                public static int Value(Dictionary<string, int> values, HashSet<string> names)
                {
                    return values.TryGetValue("a", out int found) && names.Contains("a") ? found : 0;
                }
            }
            """);

        Assert.Empty(findings);
    }

    [Fact]
    public void ACleanFixturePassesEveryRule()
    {
        IReadOnlyList<LintFinding> findings = DetLintFixture.CheckCore(
            """
            using System;
            using System.Collections.Generic;
            namespace TheThingBelow.Core;
            public static class Fixture
            {
                public const int BasisPoints = 10000;

                public static int Scale(int value, int rate) => checked(value * rate / BasisPoints);

                public static IReadOnlyList<string> Names(SortedDictionary<string, int> values)
                {
                    List<string> names = [];
                    foreach (KeyValuePair<string, int> entry in values)
                    {
                        names.Add(entry.Key);
                    }

                    names.Sort(StringComparer.Ordinal);
                    return names;
                }
            }
            """);

        Assert.Empty(findings);
    }

    [Fact]
    public void ACompilationErrorGivesAFindingAndNoRuleResult()
    {
        // A broken compilation gives no type, so a rule result would be silent and wrong (T-2).
        IReadOnlyList<LintFinding> findings = DetLintFixture.CheckCore(
            """
            namespace TheThingBelow.Core;
            public static class Fixture
            {
                public static double Rate() => Absent.Value;
            }
            """);

        Assert.Equal([SourceScan.CompileRule], DetLintFixture.RuleIds(findings));
        Assert.Contains("could not compile", findings[0].Detail, System.StringComparison.Ordinal);
    }

    [Fact]
    public void ACallOfAFileTypeThroughAUsingStaticFails()
    {
        // D-1117: the file rule reads the type that holds a member, so a `using static` of
        // `System.IO.File` fails as a qualified call does.
        IReadOnlyList<LintFinding> findings = DetLintFixture.CheckCore(
            """
            using static System.IO.File;
            namespace TheThingBelow.Core;
            public static class Fixture
            {
                public static bool Here() => Exists("x");
            }
            """);

        Assert.Contains("DL 11", DetLintFixture.RuleIds(findings));
    }

    [Fact]
    public void CodeUnderAnUndefinedSymbolFailsAndCodeUnderTheDefinedSymbolTakesTheRules()
    {
        // D-1117: the scan defines `CONTRACTS_FULL` alone, and the compiler reads a block
        // under another symbol as text. Such a block once passed every rule with a double in it.
        IReadOnlyList<LintFinding> findings = DetLintFixture.CheckCore(
            """
            namespace TheThingBelow.Core;
            public static class Fixture
            {
            #if DEBUG
                public static double Hidden() => 2;
            #endif
            #if CONTRACTS_FULL
                public static double Seen() => 2;
            #else
                public static int Other() => 2;
            #endif
            }
            """);

        Assert.Equal(["DL 12", "DL 1", "DL 12"], DetLintFixture.RuleIds(findings));
        Assert.Equal([5, 8, 10], [findings[0].Line, findings[1].Line, findings[2].Line]);
    }
}
