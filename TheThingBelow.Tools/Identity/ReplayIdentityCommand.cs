using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using TheThingBelow.Core.Identity;

namespace TheThingBelow.Tools.Identity;

/// <summary>
/// The `replay-identity` command. It computes the state hash of every run of the identity
/// set and compares each one with the identity file (G-5, D-504). The `--write` option
/// writes the file again after an intended change of Core behavior (G-17).
/// </summary>
public static class ReplayIdentityCommand
{
    /// <summary>The name of the command on the command line.</summary>
    public const string Name = "replay-identity";

    /// <summary>The option that names the root of the checkout.</summary>
    public const string RootOption = "--root";

    /// <summary>The option that writes the identity file again.</summary>
    public const string WriteOption = "--write";

    /// <summary>Compares each hash with the identity file, or writes the file again.</summary>
    /// <param name="args">The arguments after the command name.</param>
    /// <param name="output">The writer that takes each line of the report.</param>
    /// <param name="errors">The writer that takes each fault.</param>
    /// <returns>0 when every hash matches, and 1 when one does not.</returns>
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter errors)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        string root = ".";
        bool write = false;
        for (int index = 0; index < args.Count; index += 1)
        {
            string option = args[index];
            if (option == WriteOption)
            {
                write = true;
                continue;
            }

            if (option != RootOption)
            {
                errors.WriteLine(
                    $"Error: the option '{option}' is unknown. {Name} takes {RootOption} <path> and {WriteOption}.");
                return Program.FaultExitCode;
            }

            if (index + 1 >= args.Count)
            {
                errors.WriteLine($"Error: the option {RootOption} needs a value after it.");
                return Program.FaultExitCode;
            }

            root = args[index + 1];
            index += 1;
        }

        try
        {
            return write ? WriteFile(root, output) : Compare(root, output, errors);
        }
        catch (Exception fault) when (fault is IOException or UnauthorizedAccessException)
        {
            // The command never hides a file fault. The message names the file (T-2).
            errors.WriteLine($"Error: {fault.Message}");
            return Program.FaultExitCode;
        }
    }

    /// <summary>The name of this machine, which a mismatch report names (T-2, D-504).</summary>
    /// <returns>The runtime identifier, such as `linux-x64`.</returns>
    public static string Leg() => RuntimeInformation.RuntimeIdentifier;

    /// <summary>Computes the hash of every run of the set, in the order of the set.</summary>
    /// <returns>Each run name with the hash that this machine computed.</returns>
    public static IReadOnlyList<KeyValuePair<string, ulong>> ComputeEveryRun()
    {
        List<KeyValuePair<string, ulong>> runs = [];
        foreach (string name in IdentitySet.RunNames)
        {
            runs.Add(new KeyValuePair<string, ulong>(name, IdentitySet.Compute(name)));
        }

        return runs;
    }

    private static int WriteFile(string root, TextWriter output)
    {
        IReadOnlyList<KeyValuePair<string, ulong>> runs = ComputeEveryRun();
        IdentityFile.Write(root, runs);

        output.WriteLine($"{Name}: wrote {runs.Count} run(s) to {IdentityFile.Path} on {Leg()}.");
        foreach (KeyValuePair<string, ulong> run in runs)
        {
            output.WriteLine($"  {run.Key} {IdentityFile.Format(run.Value)}");
        }

        output.WriteLine(
            $"{Name}: a changed hash needs a higher simulation version, and the review reads each one (G-17).");
        return 0;
    }

    private static int Compare(string root, TextWriter output, TextWriter errors)
    {
        string leg = Leg();
        IReadOnlyList<KeyValuePair<string, ulong>> actual = ComputeEveryRun();
        IReadOnlyList<KeyValuePair<string, ulong>> expected = IdentityFile.Read(root);

        SortedDictionary<string, ulong> expectedByRun = new(StringComparer.Ordinal);
        foreach (KeyValuePair<string, ulong> run in expected)
        {
            if (!expectedByRun.TryAdd(run.Key, run.Value))
            {
                errors.WriteLine($"{Name}: {IdentityFile.Path} names the run '{run.Key}' two times.");
                return Program.FaultExitCode;
            }
        }

        int faults = 0;
        foreach (KeyValuePair<string, ulong> run in actual)
        {
            if (!expectedByRun.Remove(run.Key, out ulong want))
            {
                errors.WriteLine(
                    $"{Name}: leg {leg}: the set holds the run '{run.Key}', and {IdentityFile.Path} does not. " +
                    $"Run {Name} {WriteOption} and read each new line.");
                faults += 1;
                continue;
            }

            if (want != run.Value)
            {
                errors.WriteLine(
                    $"{Name}: leg {leg}: run {run.Key}: expected {IdentityFile.Format(want)}, " +
                    $"actual {IdentityFile.Format(run.Value)}.");
                faults += 1;
            }
        }

        foreach (KeyValuePair<string, ulong> left in expectedByRun)
        {
            errors.WriteLine(
                $"{Name}: leg {leg}: {IdentityFile.Path} names the run '{left.Key}', and the set does not.");
            faults += 1;
        }

        if (faults > 0)
        {
            errors.WriteLine(
                $"{Name}: {faults} fault(s). A change of Core behavior raises the simulation " +
                $"version and writes the file again (G-17, D-504).");
            return Program.FaultExitCode;
        }

        output.WriteLine($"{Name}: leg {leg}: {actual.Count} run(s) match {IdentityFile.Path}.");
        return 0;
    }
}
