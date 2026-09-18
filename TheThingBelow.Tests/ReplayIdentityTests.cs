using System;
using System.Collections.Generic;
using System.IO;
using TheThingBelow.Core;
using TheThingBelow.Core.Identity;
using TheThingBelow.Tools;
using TheThingBelow.Tools.Identity;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The replay-identity set, the identity file, and the command that compares them
/// (G-5, D-504). This machine is the fourth leg beside the three CI legs.
/// </summary>
public sealed class ReplayIdentityTests
{
    [Fact]
    public void EveryRunOfTheSetMatchesTheCommittedIdentityFile()
    {
        // Exit test 5 of section 7.11 of `phase-1-foundations.md`. The three CI legs run the
        // same compare through the `replay-identity` job.
        StringWriter output = new();
        StringWriter errors = new();

        int code = ReplayIdentityCommand.Run(
            [ReplayIdentityCommand.RootOption, RepositoryRoot.Find()], output, errors);

        Assert.Equal(0, code);
        Assert.Equal(string.Empty, errors.ToString());
    }

    [Fact]
    public void TheIdentityFileNamesEveryRunOfTheSetAndNoOther()
    {
        IReadOnlyList<KeyValuePair<string, ulong>> file = IdentityFile.Read(RepositoryRoot.Find());

        SortedSet<string> inFile = new(StringComparer.Ordinal);
        foreach (KeyValuePair<string, ulong> run in file)
        {
            Assert.True(inFile.Add(run.Key), $"The identity file names '{run.Key}' two times.");
        }

        SortedSet<string> inSet = new(IdentitySet.RunNames, StringComparer.Ordinal);
        Assert.Equal(inSet, inFile);
    }

    [Fact]
    public void AChangedHashFailsTheCompare()
    {
        // Exit test 6 of section 7.11 of `phase-1-foundations.md`. A moved hash must fail
        // the job until a PR writes the file again on purpose (G-17, D-504).
        using TemporaryCheckout checkout = TemporaryCheckout.FromRepository();
        checkout.ChangeFirstRunHash();

        StringWriter output = new();
        StringWriter errors = new();

        int code = ReplayIdentityCommand.Run(
            [ReplayIdentityCommand.RootOption, checkout.Root], output, errors);

        Assert.Equal(Program.FaultExitCode, code);
        string report = errors.ToString();
        Assert.Contains("expected 0x", report, StringComparison.Ordinal);
        Assert.Contains("actual 0x", report, StringComparison.Ordinal);
        Assert.Contains(ReplayIdentityCommand.Leg(), report, StringComparison.Ordinal);
    }

    [Fact]
    public void ARunThatTheFileDoesNotNameFailsTheCompare()
    {
        using TemporaryCheckout checkout = TemporaryCheckout.FromRepository();
        checkout.RemoveFirstRun();

        StringWriter output = new();
        StringWriter errors = new();

        int code = ReplayIdentityCommand.Run(
            [ReplayIdentityCommand.RootOption, checkout.Root], output, errors);

        Assert.Equal(Program.FaultExitCode, code);
        Assert.Contains("and TheThingBelow.Tests/identity", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void ARunThatTheSetDoesNotHoldFailsTheCompare()
    {
        using TemporaryCheckout checkout = TemporaryCheckout.FromRepository();
        checkout.AddRun("a-run-that-core-lost", 0x0102030405060708UL);

        StringWriter output = new();
        StringWriter errors = new();

        int code = ReplayIdentityCommand.Run(
            [ReplayIdentityCommand.RootOption, checkout.Root], output, errors);

        Assert.Equal(Program.FaultExitCode, code);
        Assert.Contains("a-run-that-core-lost", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void TheWriteOptionWritesAFileThatTheCompareAccepts()
    {
        using TemporaryCheckout checkout = TemporaryCheckout.FromRepository();
        checkout.ChangeFirstRunHash();

        StringWriter output = new();
        StringWriter errors = new();
        int written = ReplayIdentityCommand.Run(
            [ReplayIdentityCommand.RootOption, checkout.Root, ReplayIdentityCommand.WriteOption],
            output,
            errors);

        Assert.Equal(0, written);
        Assert.Equal(
            0,
            ReplayIdentityCommand.Run(
                [ReplayIdentityCommand.RootOption, checkout.Root], new StringWriter(), errors));
        Assert.Equal(string.Empty, errors.ToString());
    }

    [Fact]
    public void TheWrittenFileMatchesTheCommittedFileByByte()
    {
        // The `--write` option must give the bytes that this PR committed, so no later run
        // of the command makes a diff that changes nothing (T-1).
        using TemporaryCheckout checkout = TemporaryCheckout.FromRepository();

        ReplayIdentityCommand.Run(
            [ReplayIdentityCommand.RootOption, checkout.Root, ReplayIdentityCommand.WriteOption],
            new StringWriter(),
            new StringWriter());

        Assert.Equal(
            File.ReadAllBytes(Path.Combine(RepositoryRoot.Find(), IdentityFile.Path)),
            File.ReadAllBytes(Path.Combine(checkout.Root, IdentityFile.Path)));
    }

    [Fact]
    public void AMissingIdentityFileIsAnError()
    {
        using TemporaryCheckout checkout = TemporaryCheckout.FromRepository();
        File.Delete(Path.Combine(checkout.Root, IdentityFile.Path));

        Assert.Throws<FileNotFoundException>(() => IdentityFile.Read(checkout.Root));
    }

    [Theory]
    [InlineData("basis-points")]
    [InlineData("basis-points 0x00 extra")]
    [InlineData("basis-points 12345")]
    [InlineData("basis-points 0xZZZZZZZZZZZZZZZZ")]
    public void ALineThatTakesNoLegalFormIsAnError(string line)
    {
        using TemporaryCheckout checkout = TemporaryCheckout.FromRepository();
        File.WriteAllText(Path.Combine(checkout.Root, IdentityFile.Path), line + "\n");

        InvalidDataException error = Assert.Throws<InvalidDataException>(
            () => IdentityFile.Read(checkout.Root));

        Assert.Contains(IdentityFile.Path, error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("basis-points")]
    [InlineData("basis-points 12345")]
    [InlineData("basis-points 0xZZZZZZZZZZZZZZZZ")]
    public void AMalformedIdentityFileGivesTheFaultCodeAndNotACrash(string line)
    {
        // A regression test. The command caught `IOException` alone, and the
        // `InvalidDataException` of a malformed line escaped as an unhandled exception
        // with a stack trace, in place of the one-line fault report (T-2).
        using TemporaryCheckout checkout = TemporaryCheckout.FromRepository();
        File.WriteAllText(Path.Combine(checkout.Root, IdentityFile.Path), line + "\n");

        StringWriter errors = new();
        int code = ReplayIdentityCommand.Run(
            [ReplayIdentityCommand.RootOption, checkout.Root], new StringWriter(), errors);

        Assert.Equal(Program.FaultExitCode, code);
        Assert.Contains(IdentityFile.Path, errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void AMissingIdentityFileGivesTheFaultCodeAndNotACrash()
    {
        using TemporaryCheckout checkout = TemporaryCheckout.FromRepository();
        File.Delete(Path.Combine(checkout.Root, IdentityFile.Path));

        StringWriter errors = new();
        int code = ReplayIdentityCommand.Run(
            [ReplayIdentityCommand.RootOption, checkout.Root], new StringWriter(), errors);

        Assert.Equal(Program.FaultExitCode, code);
        Assert.Contains(IdentityFile.Path, errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void AnUnknownRunNameIsAnError()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => IdentitySet.Compute("no-such-run"));
    }

    [Fact]
    public void EveryRunOfTheSetGivesItsOwnHash()
    {
        SortedSet<ulong> hashes = [];

        foreach (string name in IdentitySet.RunNames)
        {
            Assert.True(
                hashes.Add(IdentitySet.Compute(name)),
                $"The run '{name}' gives the hash of another run.");
        }
    }

    [Fact]
    public void ARunGivesTheSameHashEveryTime()
    {
        foreach (string name in IdentitySet.RunNames)
        {
            Assert.Equal(IdentitySet.Compute(name), IdentitySet.Compute(name));
        }
    }

    [Fact]
    public void TheRunNamesAreInOrdinalOrder()
    {
        for (int index = 1; index < IdentitySet.RunNames.Count; index += 1)
        {
            Assert.True(
                string.CompareOrdinal(IdentitySet.RunNames[index - 1], IdentitySet.RunNames[index]) < 0,
                $"The run '{IdentitySet.RunNames[index]}' is not after the run before it.");
        }
    }

    [Fact]
    public void TheSimulationVersionIsAboveZero()
    {
        Assert.True(SimulationVersion.Current >= 1);
    }

    [Fact]
    public void AnUnknownOptionIsAFault()
    {
        StringWriter errors = new();

        int code = ReplayIdentityCommand.Run(["--nope"], new StringWriter(), errors);

        Assert.Equal(Program.FaultExitCode, code);
        Assert.Contains("--nope", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void TheRootOptionWithNoValueIsAFault()
    {
        StringWriter errors = new();

        int code = ReplayIdentityCommand.Run(
            [ReplayIdentityCommand.RootOption], new StringWriter(), errors);

        Assert.Equal(Program.FaultExitCode, code);
    }

    /// <summary>A copy of the identity file of this checkout, in a temporary folder.</summary>
    private sealed class TemporaryCheckout : IDisposable
    {
        private TemporaryCheckout(string root) => this.Root = root;

        public string Root { get; }

        public static TemporaryCheckout FromRepository()
        {
            string root = Path.Combine(Path.GetTempPath(), "identity-" + Guid.NewGuid().ToString("N"));
            string path = Path.Combine(root, IdentityFile.Path);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.Copy(Path.Combine(RepositoryRoot.Find(), IdentityFile.Path), path);
            return new TemporaryCheckout(root);
        }

        public void ChangeFirstRunHash()
        {
            List<KeyValuePair<string, ulong>> runs = [.. IdentityFile.Read(this.Root)];
            runs[0] = new KeyValuePair<string, ulong>(runs[0].Key, runs[0].Value + 1);
            IdentityFile.Write(this.Root, runs);
        }

        public void RemoveFirstRun()
        {
            List<KeyValuePair<string, ulong>> runs = [.. IdentityFile.Read(this.Root)];
            runs.RemoveAt(0);
            IdentityFile.Write(this.Root, runs);
        }

        public void AddRun(string name, ulong hash)
        {
            List<KeyValuePair<string, ulong>> runs = [.. IdentityFile.Read(this.Root)];
            runs.Add(new KeyValuePair<string, ulong>(name, hash));
            IdentityFile.Write(this.Root, runs);
        }

        public void Dispose()
        {
            if (Directory.Exists(this.Root))
            {
                Directory.Delete(this.Root, recursive: true);
            }
        }
    }
}
