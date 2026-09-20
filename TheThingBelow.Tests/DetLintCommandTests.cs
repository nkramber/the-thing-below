using System;
using System.IO;
using TheThingBelow.Tools;
using TheThingBelow.Tools.DetLint;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The `det-lint` command from the command line: the options, the report, and the exit code
/// (D-496, D-614). Each test runs the command over a fixture checkout.
/// </summary>
public sealed class DetLintCommandTests
{
    [Fact]
    public void TheCommandWritesOneLinePerFindingAndGivesTheFaultCode()
    {
        using DetLintCheckout checkout = DetLintCheckout.Build();
        checkout.Write(
            "TheThingBelow.Core/Rate.cs",
            """
            namespace TheThingBelow.Core;
            public static class Rate
            {
                public static double Of(int value) => value;
            }
            """);
        checkout.Write(
            "TheThingBelow.Game/Screen.cs",
            """
            using Godot;
            namespace TheThingBelow.Game;
            public static class Screen
            {
                public static void Draw(Label label) => label.Text = "Enter the mine.";
            }
            """);
        checkout.Write(
            "TheThingBelow.Game/Screen.tscn",
            "[node name=\"Title\" type=\"Label\"]\ntext = \"Enter the mine.\"\n");
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = DetLintCommand.Run(Arguments(checkout), output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("TheThingBelow.Core/Rate.cs:4: rule DL 1:", output.ToString(), StringComparison.Ordinal);
        Assert.Contains("TheThingBelow.Game/Screen.cs:5: rule DL 8:", output.ToString(), StringComparison.Ordinal);
        Assert.Contains("TheThingBelow.Game/Screen.tscn:2: rule DL 9:", output.ToString(), StringComparison.Ordinal);
        Assert.Contains("3 finding(s)", output.ToString(), StringComparison.Ordinal);
        Assert.Equal(string.Empty, errors.ToString());
    }

    [Fact]
    public void ACleanCheckoutPassesAndNamesEachPlannedTool()
    {
        using DetLintCheckout checkout = DetLintCheckout.Build();
        checkout.Write(
            "TheThingBelow.Core/Rate.cs",
            """
            namespace TheThingBelow.Core;
            public static class Rate
            {
                public static int Of(int value) => checked(value * 2);
            }
            """);
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = DetLintCommand.Run(Arguments(checkout), output, errors);

        Assert.Equal(0, exitCode);
        Assert.Contains("0 finding(s)", output.ToString(), StringComparison.Ordinal);
        foreach (string folder in DetLintCommand.ComparedOutputTools.Keys)
        {
            // A folder that no PR wrote yet names that PR, and it fails nothing (G-16).
            Assert.Contains($"the folder '{folder}' does not exist yet", output.ToString(), StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// A folder of D-502 that exists takes the Core rules for the float types, the clock, and
    /// the OS random, and the code of Tools outside such a folder takes no rule (F-82).
    /// </summary>
    [Fact]
    public void AFloatInAToolOfComparedOutputIsAFindingAndAFloatOutsideItIsNot()
    {
        using DetLintCheckout checkout = DetLintCheckout.Build();
        checkout.Write(
            "TheThingBelow.Tools/Png/Scale.cs",
            """
            namespace TheThingBelow.Tools.Png;
            public static class Scale
            {
                public static double Half(int value) => value / 2.0;
            }
            """);
        checkout.Write(
            "TheThingBelow.Tools/SteCheck/Report.cs",
            """
            namespace TheThingBelow.Tools.SteCheck;
            public static class Report
            {
                public static double Share(int part, int whole) => (double)part / whole;
            }
            """);
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = DetLintCommand.Run(Arguments(checkout), output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("TheThingBelow.Tools/Png/Scale.cs:4: rule DL 1:", output.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("TheThingBelow.Tools/SteCheck/Report.cs", output.ToString(), StringComparison.Ordinal);
        Assert.Equal(string.Empty, errors.ToString());
    }

    /// <summary>
    /// A compile error of the Tools scan stays in the report whatever file it names. The old
    /// code filtered each finding by the folders of D-502, so an error in another folder of
    /// Tools dropped every finding, and the scan passed with nothing read (T-2).
    /// </summary>
    [Fact]
    public void ACompileErrorOutsideAToolOfComparedOutputStillFailsTheScan()
    {
        using DetLintCheckout checkout = DetLintCheckout.Build();
        checkout.Write(
            "TheThingBelow.Tools/SteCheck/Broken.cs",
            """
            namespace TheThingBelow.Tools.SteCheck;
            public static class Broken
            {
                public static int Count => ;
            }
            """);
        checkout.Write(
            "TheThingBelow.Tools/Png/Scale.cs",
            """
            namespace TheThingBelow.Tools.Png;
            public static class Scale
            {
                public static double Half(int value) => value / 2.0;
            }
            """);
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = DetLintCommand.Run(Arguments(checkout), output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("TheThingBelow.Tools/SteCheck/Broken.cs:4: rule DL 0:", output.ToString(), StringComparison.Ordinal);
        Assert.Equal(string.Empty, errors.ToString());
    }

    [Fact]
    public void AnAbsentGameBuildNamesTheBuildCommand()
    {
        using DetLintCheckout checkout = DetLintCheckout.Build();
        checkout.RemoveGameOutput();
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = DetLintCommand.Run(Arguments(checkout), output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("dotnet build TheThingBelow.slnx", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void AnUnknownOptionNamesTheOptionsOfTheCommand()
    {
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = DetLintCommand.Run(["--all"], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("--all", errors.ToString(), StringComparison.Ordinal);
        Assert.Contains(DetLintCommand.ConfigurationOption, errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void AnAbsentRootIsAnError()
    {
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = DetLintCommand.Run(
            [DetLintCommand.RootOption, Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))],
            output,
            errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("does not exist", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void TheCodeOfThisRepositoryPassesEveryRule()
    {
        string output = DetLintFixture.GameOutputFolder();
        using StringWriter report = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = DetLintCommand.Run(
            [
                DetLintCommand.RootOption,
                RepositoryRoot.Find(),
                DetLintCommand.ConfigurationOption,
                Path.GetFileName(output),
            ],
            report,
            errors);

        Assert.Equal(string.Empty, errors.ToString());
        Assert.Equal(0, exitCode);
    }

    private static string[] Arguments(DetLintCheckout checkout) =>
    [
        DetLintCommand.RootOption,
        checkout.Root,
        DetLintCommand.ConfigurationOption,
        DetLintCheckout.Configuration,
    ];
}
