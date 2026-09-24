using System;
using System.Collections.Generic;
using System.IO;
using TheThingBelow.Tools.Atlas;
using TheThingBelow.Tools.ChangedPaths;
using TheThingBelow.Tools.CodexReview;
using TheThingBelow.Tools.Content;
using TheThingBelow.Tools.DetLint;
using TheThingBelow.Tools.Evaluator;
using TheThingBelow.Tools.Identity;
using TheThingBelow.Tools.Pictures;
using TheThingBelow.Tools.ReviewGate;
using TheThingBelow.Tools.Screens;
using TheThingBelow.Tools.Screenplay;
using TheThingBelow.Tools.SteCheck;

namespace TheThingBelow.Tools;

/// <summary>The command line of the tools project. Each command gets its own PR.</summary>
public static class Program
{
    /// <summary>The exit code of a run that found a fault (T-2).</summary>
    public const int FaultExitCode = 1;

    /// <summary>The commands that no PR has written yet, and the PR that adds each one.</summary>
    public static readonly IReadOnlyDictionary<string, string> PlannedCommands =
        new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["night-gate"] = "PR-49",
        };

    /// <summary>Reads the command name and runs it.</summary>
    /// <param name="args">The command name, then the arguments of that command.</param>
    /// <returns>The exit code of the process.</returns>
    public static int Main(string[] args) => Run(args, Console.Out, Console.Error);

    /// <summary>
    /// Reads the command name, writes each fault to <paramref name="errors"/>, and gives the
    /// exit code. A command that no PR has written yet names that PR (G-16).
    /// </summary>
    /// <param name="args">The command name, then the arguments of that command.</param>
    /// <param name="output">The writer that takes the output of the command.</param>
    /// <param name="errors">The writer that takes each error line.</param>
    /// <returns>The exit code of the run.</returns>
    public static int Run(string[] args, TextWriter output, TextWriter errors)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        if (args.Length == 0)
        {
            errors.WriteLine("Error: no command. The first argument names the command.");
            WriteCommands(errors);
            return FaultExitCode;
        }

        string command = args[0];
        if (command == SteCheckCommand.Name)
        {
            return SteCheckCommand.Run(args[1..], output, errors);
        }

        if (command == ReviewGateCommand.Name)
        {
            return ReviewGateCommand.Run(args[1..], output, errors);
        }

        if (command == DetLintCommand.Name)
        {
            return DetLintCommand.Run(args[1..], output, errors);
        }

        if (command == ReplayIdentityCommand.Name)
        {
            return ReplayIdentityCommand.Run(args[1..], output, errors);
        }

        if (command == EvaluatorCostCommand.Name)
        {
            return EvaluatorCostCommand.Run(args[1..], output, errors);
        }

        if (command == ContentHashCommand.Name)
        {
            return ContentHashCommand.Run(args[1..], output, errors);
        }

        if (command == AtlasCommand.Name)
        {
            return AtlasCommand.Run(args[1..], output, errors);
        }

        if (command == ScreensCommand.Name)
        {
            return ScreensCommand.Run(args[1..], output, errors);
        }

        if (command == PictureCommand.Name)
        {
            return PictureCommand.Run(args[1..], output, errors);
        }

        if (command == ChangedPathsCommand.Name)
        {
            return ChangedPathsCommand.Run(args[1..], output, errors);
        }

        if (command == CodexReviewCommand.Name)
        {
            return CodexReviewCommand.Run(args[1..], output, errors);
        }

        if (command == ScreenplayCommand.Name)
        {
            return ScreenplayCommand.Run(args[1..], output, errors);
        }

        if (PlannedCommands.TryGetValue(command, out string? pullRequest))
        {
            errors.WriteLine(
                $"Error: the command '{command}' does not exist yet. {pullRequest} adds it.");
            return FaultExitCode;
        }

        errors.WriteLine($"Error: unknown command '{command}'.");
        WriteCommands(errors);
        return FaultExitCode;
    }

    private static void WriteCommands(TextWriter errors)
    {
        errors.WriteLine("The commands that exist:");
        errors.WriteLine($"  {SteCheckCommand.Name}: ready");
        errors.WriteLine($"  {ReviewGateCommand.Name}: ready");
        errors.WriteLine($"  {DetLintCommand.Name}: ready");
        errors.WriteLine($"  {ReplayIdentityCommand.Name}: ready");
        errors.WriteLine($"  {ContentHashCommand.Name}: ready");
        errors.WriteLine($"  {AtlasCommand.Name}: ready");
        errors.WriteLine($"  {ScreensCommand.Name}: ready");
        errors.WriteLine($"  {PictureCommand.Name}: ready");
        errors.WriteLine($"  {ChangedPathsCommand.Name}: ready");
        errors.WriteLine($"  {CodexReviewCommand.Name}: ready");
        errors.WriteLine($"  {EvaluatorCostCommand.Name}: ready");
        errors.WriteLine($"  {ScreenplayCommand.Name}: ready");
        errors.WriteLine("The planned commands, with the PR that adds each one:");
        foreach (KeyValuePair<string, string> entry in PlannedCommands)
        {
            errors.WriteLine($"  {entry.Key}: {entry.Value}");
        }
    }
}
