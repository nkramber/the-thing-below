using System;
using System.Collections.Generic;
using System.IO;

namespace TheThingBelow.Tools.DetLint;

/// <summary>
/// The `det-lint` command. It reads the code of Core, of Game, of the debug commands, and of
/// each tool of D-502, and it gives one line for each finding: the file, the line, the rule id,
/// and what the rule saw (D-496, D-498, G-2, G-3, G-7). The command reads a build of the
/// solution, so `make verify` and the `det-lint` job build first (D-614).
/// </summary>
public static class DetLintCommand
{
    /// <summary>The name of the command on the command line.</summary>
    public const string Name = "det-lint";

    /// <summary>The option that names the root of the checkout.</summary>
    public const string RootOption = "--root";

    /// <summary>The option that names the build configuration of the output folders.</summary>
    public const string ConfigurationOption = "--configuration";

    /// <summary>The folder of the Core project, from the root of the checkout.</summary>
    public const string CoreProject = "TheThingBelow.Core";

    /// <summary>The folder of the Game project, from the root of the checkout.</summary>
    public const string GameProject = "TheThingBelow.Game";

    /// <summary>The folder of the Tools project, from the root of the checkout.</summary>
    public const string ToolsProject = "TheThingBelow.Tools";

    /// <summary>The folder of the debug assembly, from the root of the checkout (D-260).</summary>
    public const string DebugProject = "TheThingBelow.Debug";

    /// <summary>
    /// The folder of the debug commands, which run inside a tick of the rules (D-171, D-724).
    /// The console of the same assembly draws with the engine, and it lies outside this folder.
    /// </summary>
    public const string DebugCommandsFolder = $"{DebugProject}/Commands";

    /// <summary>
    /// The folder of each tool whose output a test compares on every CI leg, and the PR that
    /// writes it (D-502, G-16). A later tool of that kind joins this list.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> ComparedOutputTools =
        new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [$"{ToolsProject}/Atlas"] = "PR-34",
            [$"{ToolsProject}/Audio"] = "PR-38",
            [$"{ToolsProject}/NormalMaps"] = "PR-48",
            [$"{ToolsProject}/Pictures"] = "PR-55",
            [$"{ToolsProject}/Png"] = "PR-47",
        };

    /// <summary>Reads every project of the lint and writes each finding.</summary>
    /// <param name="args">The arguments after the command name.</param>
    /// <param name="output">The writer that takes each finding and the count.</param>
    /// <param name="errors">The writer that takes each fault of the run.</param>
    /// <returns>0 when no rule found a fault, and 1 when one did.</returns>
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter errors)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        OptionParser? options = OptionParser.Read(Name, args, [RootOption, ConfigurationOption], [], errors);
        if (options is null)
        {
            return Program.FaultExitCode;
        }

        string root = options.ValueOr(RootOption, ".");
        string configuration = options.ValueOr(ConfigurationOption, "Debug");

        try
        {
            return Report(root, configuration, output, errors);
        }
        catch (Exception fault) when (fault is InvalidOperationException or IOException or UnauthorizedAccessException)
        {
            errors.WriteLine($"Error: {Name} stopped on the root '{root}'. {fault.Message}");
            return Program.FaultExitCode;
        }
    }

    /// <summary>Gives the build output folder of the Game project (D-614, F-65).</summary>
    /// <param name="root">The root of the checkout.</param>
    /// <param name="configuration">The build configuration, such as `Debug`.</param>
    /// <returns>The folder that holds `GodotSharp.dll` and the Game assembly.</returns>
    public static string GameOutputFolder(string root, string configuration)
    {
        ArgumentException.ThrowIfNullOrEmpty(root);
        ArgumentException.ThrowIfNullOrEmpty(configuration);

        // Godot.NET.Sdk writes the output of a Godot project here, and the `bin` folder of
        // the project stays empty (F-65).
        return Path.Combine(root, GameProject, ".godot", "mono", "temp", "bin", configuration);
    }

    private static int Report(string root, string configuration, TextWriter output, TextWriter errors)
    {
        if (!Directory.Exists(root))
        {
            errors.WriteLine($"Error: the root '{root}' does not exist.");
            return Program.FaultExitCode;
        }

        List<LintFinding> findings = [];
        findings.AddRange(SourceScan.Check(
            "det-lint.core",
            ProjectFiles.ReadCode(root, CoreProject),
            ReferenceSet.Framework(),
            CoreRules.All()));
        findings.AddRange(SourceScan.Check(
            "det-lint.game",
            ProjectFiles.ReadCode(root, GameProject),
            ReferenceSet.WithOutputOf(GameOutputFolder(root, configuration), $"{GameProject}"),
            [new GodotTextRule()]));
        findings.AddRange(CheckDebugCommands(root));
        findings.AddRange(CheckComparedOutputTools(root, output));
        findings.AddRange(CheckScenes(root));

        foreach (LintFinding finding in findings)
        {
            output.WriteLine(finding.ToString());
        }

        output.WriteLine($"{findings.Count} finding(s)");
        return findings.Count > 0 ? Program.FaultExitCode : 0;
    }

    /// <summary>
    /// Reads each tool of D-502 with the Core rules for the float types, the clock, and the OS
    /// random. The command names the PR of each folder that no PR wrote yet (G-16).
    /// </summary>
    private static IReadOnlyList<LintFinding> CheckComparedOutputTools(string root, TextWriter output)
    {
        List<string> live = [];
        foreach (KeyValuePair<string, string> tool in ComparedOutputTools)
        {
            if (Directory.Exists(Path.Combine(root, tool.Key)))
            {
                live.Add(tool.Key);
            }
            else
            {
                output.WriteLine($"{Name}: the folder '{tool.Key}' does not exist yet. {tool.Value} writes it (G-16).");
            }
        }

        if (live.Count == 0)
        {
            return [];
        }

        // This command is the Tools program, so the framework list of this process already
        // holds every assembly that Tools references, Core and the compiler library included.
        // A build output folder of Tools gives no reference beside that list, and the count of
        // added references was 0 (F-82, D-614).
        IReadOnlyList<LintFinding> findings = SourceScan.Check(
            "det-lint.tools",
            ProjectFiles.ReadCode(root, ToolsProject),
            ReferenceSet.Framework(),
            CoreRules.ComparedOutputTools());

        List<LintFinding> inside = [];
        foreach (LintFinding finding in findings)
        {
            // A compile error stops every rule of the scan, so it stays in the report whatever
            // file it names. A filter that dropped it would pass a scan that read nothing (T-2).
            if (string.Equals(finding.Rule, SourceScan.CompileRule, StringComparison.Ordinal))
            {
                inside.Add(finding);
                continue;
            }

            foreach (string folder in live)
            {
                if (ProjectFiles.IsInside(finding.File, folder))
                {
                    inside.Add(finding);
                    break;
                }
            }
        }

        return inside;
    }

    /// <summary>
    /// Reads the commands of the debug assembly with the float, clock, and OS random rules
    /// (D-171, D-724, T-7). A handler of a debug command changes the state of a run inside a
    /// tick, so it takes the determinism rules of a rule of Core (G-2, G-3).
    /// </summary>
    /// <remarks>
    /// The scan compiles the files of that folder alone, against the framework set of this
    /// process. That set holds Core and no Godot assembly, so a Godot type in this folder
    /// fails the compile and the report holds that fault. Thus the folder stays engine-free,
    /// and the console of the same assembly keeps every engine call (D-723, T-2).
    /// </remarks>
    private static IReadOnlyList<LintFinding> CheckDebugCommands(string root) =>
        SourceScan.Check(
            "det-lint.debug",
            ProjectFiles.ReadCode(root, DebugCommandsFolder),
            ReferenceSet.Framework(),
            CoreRules.ComparedOutputTools());

    /// <summary>Reads each Godot scene file of Game with the text rule (D-499).</summary>
    private static IReadOnlyList<LintFinding> CheckScenes(string root)
    {
        List<LintFinding> findings = [];
        foreach (string scene in ProjectFiles.ReadScenes(root, GameProject))
        {
            findings.AddRange(SceneTextRule.Check(scene, File.ReadAllLines(Path.Combine(root, scene))));
        }

        return findings;
    }
}
