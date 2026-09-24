using System;
using System.IO;
using TheThingBelow.Tools;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The `content-hash` command loads every content file, computes the hash of the rule files,
/// and compares it with the committed file (G-5, D-495, D-648).
/// </summary>
public sealed class ContentHashCommandTests
{
    [Fact]
    public void TheCheckoutMatchesTheCommittedFile()
    {
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = ContentHashCommand.Run(["--root", RepositoryRoot.Find()], output, errors);

        Assert.Equal(string.Empty, errors.ToString());
        Assert.Equal(0, exitCode);
        Assert.Contains("matches the committed file", output.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void AChangedRuleFileFailsAndNamesBothValues()
    {
        using ContentCheckout checkout = ContentCheckout.Copy();
        checkout.WriteRuleFile("rules/fixtures/tools.json", ChangedWeight());
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = ContentHashCommand.Run(["--root", checkout.Root], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("the content hash is", errors.ToString(), StringComparison.Ordinal);
        Assert.Contains(ContentHashCommand.ReadCommitted(checkout.Root), errors.ToString(), StringComparison.Ordinal);
    }

    /// <summary>
    /// A hash file with no hash line gives the fault code and the path, and never a stack
    /// trace. `InvalidDataException` is not an `IOException`, so the catch names it (T-2).
    /// </summary>
    [Fact]
    public void AHashFileWithNoHashLineGivesTheFaultCodeAndNotACrash()
    {
        using ContentCheckout checkout = ContentCheckout.Copy();
        string path = Path.Combine(checkout.Root, ContentHashCommand.HashFilePath.Replace('/', Path.DirectorySeparatorChar));
        File.WriteAllText(path, "# a header alone\n");
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = ContentHashCommand.Run(["--root", checkout.Root], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("holds no hash line", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void TheWriteOptionMakesTheCompareRunPass()
    {
        using ContentCheckout checkout = ContentCheckout.Copy();
        checkout.WriteRuleFile("rules/fixtures/tools.json", ChangedWeight());
        using StringWriter output = new();
        using StringWriter errors = new();

        int written = ContentHashCommand.Run(["--root", checkout.Root, "--write"], output, errors);
        int compared = ContentHashCommand.Run(["--root", checkout.Root], output, errors);

        Assert.Equal(string.Empty, errors.ToString());
        Assert.Equal(0, written);
        Assert.Equal(0, compared);
    }

    [Fact]
    public void AFileOutsideTheRuleFolderNeverMovesTheHash()
    {
        using ContentCheckout checkout = ContentCheckout.Copy();
        string before = ContentHashCommand.ReadCommitted(checkout.Root);
        checkout.WriteRuleFile("strings/en.json", ChangedStringTable(checkout.Root));
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = ContentHashCommand.Run(["--root", checkout.Root], output, errors);

        Assert.Equal(string.Empty, errors.ToString());
        Assert.Equal(0, exitCode);
        Assert.Contains(before, output.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void ABrokenContentFileStopsTheRunBeforeTheHash()
    {
        using ContentCheckout checkout = ContentCheckout.Copy();
        checkout.WriteRuleFile("rules/fixtures/tools.json", """{ "comment": "a note" }""");
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = ContentHashCommand.Run(["--root", checkout.Root], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("fixtures", errors.ToString(), StringComparison.Ordinal);
        Assert.Contains("the field is absent", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void AnUnknownOptionNamesEveryOption()
    {
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = ContentHashCommand.Run(["--all"], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("--root", errors.ToString(), StringComparison.Ordinal);
        Assert.Contains("--write", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void ARootWithNoContentFolderNamesTheFolder()
    {
        using StringWriter output = new();
        using StringWriter errors = new();
        string empty = Path.Combine(Path.GetTempPath(), "content-hash-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(empty);

        try
        {
            int exitCode = ContentHashCommand.Run(["--root", empty], output, errors);

            Assert.Equal(Program.FaultExitCode, exitCode);
            Assert.Contains("content", errors.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(empty, recursive: true);
        }
    }

    [Fact]
    public void TheCommandRunsThroughTheCommandLine()
    {
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = Program.Run(
            [ContentHashCommand.Name, "--root", RepositoryRoot.Find()],
            output,
            errors);

        Assert.Equal(0, exitCode);
    }

    private static string ChangedWeight() =>
        """
        {
         "comment": "a note",
         "fixtures": [
          { "id": "fixture.lamp", "label": "label.lamp", "weight": 11 }
         ]
        }
        """;

    /// <summary>Gives the string table of the copy with one text changed. Each id stays, so each check of a string reads the same set (G-7).</summary>
    private static string ChangedStringTable(string root)
    {
        string table = File.ReadAllText(Path.Combine(root, ContentFolder.FolderName, "strings", "en.json")).Replace("\"Tin lamp\"", "\"Rusted lamp\"", StringComparison.Ordinal);
        Assert.Contains("Rusted lamp", table, StringComparison.Ordinal);
        return table.Replace("\r\n", "\n", StringComparison.Ordinal);
    }
}
