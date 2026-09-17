using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using TheThingBelow.Tools.DetLint;

namespace TheThingBelow.Tests;

/// <summary>
/// The fixture sources of the det-lint tests. Each test gives one file of C#, and the fixture
/// compiles it with the rules of Core or with the text rule of Game. The reference lists are
/// static, because each one reads every assembly of the framework from disk.
/// </summary>
public static class DetLintFixture
{
    /// <summary>The path that a Core fixture takes in a finding.</summary>
    public const string CorePath = "TheThingBelow.Core/Fixture.cs";

    /// <summary>The path that a Game fixture takes in a finding.</summary>
    public const string GamePath = "TheThingBelow.Game/Fixture.cs";

    private static readonly Lazy<IReadOnlyList<MetadataReference>> FrameworkReferences =
        new(ReferenceSet.Framework);

    private static readonly Lazy<IReadOnlyList<MetadataReference>> GameReferences =
        new(() => ReferenceSet.WithOutputOf(GameOutputFolder(), "TheThingBelow.Game"));

    /// <summary>Reads one Core fixture with every rule of Core (DL 1 to DL 7).</summary>
    /// <param name="code">The C# of the fixture file.</param>
    /// <returns>Each finding, sorted by line and rule.</returns>
    public static IReadOnlyList<LintFinding> CheckCore(string code) =>
        SourceScan.Check(
            "det-lint.fixture.core",
            [new ScanSource(CorePath, code)],
            FrameworkReferences.Value,
            CoreRules.All());

    /// <summary>Reads one Game fixture with the text rule of Game (DL 8).</summary>
    /// <param name="code">The C# of the fixture file, which can use Godot types.</param>
    /// <returns>Each finding, sorted by line and rule.</returns>
    public static IReadOnlyList<LintFinding> CheckGame(string code) =>
        SourceScan.Check(
            "det-lint.fixture.game",
            [new ScanSource(GamePath, code)],
            GameReferences.Value,
            [new GodotTextRule()]);

    /// <summary>Gives the rule id of each finding, for a short assertion.</summary>
    /// <param name="findings">The findings of one scan.</param>
    /// <returns>The rule id of each finding, in the order of the findings.</returns>
    public static IReadOnlyList<string> RuleIds(IReadOnlyList<LintFinding> findings)
    {
        ArgumentNullException.ThrowIfNull(findings);
        List<string> ids = [];
        foreach (LintFinding finding in findings)
        {
            ids.Add(finding.Rule);
        }

        return ids;
    }

    /// <summary>
    /// Finds the build output folder of the Game project of this checkout, which holds
    /// `GodotSharp.dll` (D-614, F-65). The tests run after a build of the solution.
    /// </summary>
    /// <returns>The full path of the output folder of the configuration that the build wrote.</returns>
    /// <exception cref="InvalidOperationException">No configuration folder holds the assembly.</exception>
    public static string GameOutputFolder()
    {
        string builds = RepositoryRoot.PathTo("TheThingBelow.Game/.godot/mono/temp/bin");
        if (Directory.Exists(builds))
        {
            foreach (string folder in Directory.GetDirectories(builds))
            {
                if (File.Exists(Path.Combine(folder, "GodotSharp.dll")))
                {
                    return folder;
                }
            }
        }

        throw new InvalidOperationException(
            $"No folder under '{builds}' holds GodotSharp.dll. Run `dotnet build TheThingBelow.slnx` first (T-2).");
    }
}
