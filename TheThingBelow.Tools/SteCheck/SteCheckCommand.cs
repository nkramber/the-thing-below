using System;
using System.Collections.Generic;
using System.IO;

namespace TheThingBelow.Tools.SteCheck;

/// <summary>
/// The `ste-check` command. It reads every live document that git tracks and gives one line
/// for each finding: the file, the line, the rule id, and what the rule saw (D-10, D-101, D-702).
/// </summary>
public static class SteCheckCommand
{
    /// <summary>The name of the command on the command line.</summary>
    public const string Name = "ste-check";

    /// <summary>The option that names the root of the checkout.</summary>
    public const string RootOption = "--root";

    /// <summary>Reads every live document and writes each finding.</summary>
    /// <param name="args">The arguments after the command name. `--root` names the checkout.</param>
    /// <param name="output">The writer that takes each finding and the count.</param>
    /// <param name="errors">The writer that takes each fault of the run.</param>
    /// <returns>0 when no rule found a fault, and 1 when one did.</returns>
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter errors)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        OptionParser? options = OptionParser.Read(Name, args, [RootOption], [], errors);
        if (options is null)
        {
            return Program.FaultExitCode;
        }

        string root = options.ValueOr(RootOption, ".");

        try
        {
            return Check(root, output, errors);
        }
        catch (Exception fault) when (fault is InvalidOperationException or IOException or UnauthorizedAccessException)
        {
            errors.WriteLine($"Error: {Name} stopped on the root '{root}'. {fault.Message}");
            return Program.FaultExitCode;
        }
    }

    private static int Check(string root, TextWriter output, TextWriter errors)
    {
        DocumentSet documents = DocumentSet.Read(root);
        if (documents.LiveDocuments.Count == 0)
        {
            errors.WriteLine($"Error: the checkout at '{documents.Root}' holds no live document (T-2).");
            return Program.FaultExitCode;
        }

        IdRegister register = IdRegister.Read(documents);
        List<Finding> findings = [];
        foreach (string document in documents.LiveDocuments)
        {
            findings.AddRange(WritingRules.Check(document, documents.ReadLines(document)));
            findings.AddRange(ReferenceRules.Check(document, documents, register));
        }

        findings.AddRange(SessionNumberRules.Check(documents));
        findings.AddRange(SizeRules.Check(documents));
        findings.AddRange(DocumentRowRules.Check(documents));
        findings.AddRange(AgentFileRules.Check(documents));
        findings.AddRange(AgentFileRules.CheckTestCommand(documents));

        findings.Sort(CompareFindings);
        foreach (Finding finding in findings)
        {
            output.WriteLine(finding.ToString());
        }

        output.WriteLine($"{findings.Count} finding(s)");
        return findings.Count > 0 ? Program.FaultExitCode : 0;
    }

    private static int CompareFindings(Finding left, Finding right)
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
