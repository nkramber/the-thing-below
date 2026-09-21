using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The seam between Game and the debug assembly (D-260, D-492, D-723). Game names no type of
/// that assembly, so these tests read the names that `DebugSeam` holds and find each one in
/// the assembly that the build wrote.
/// </summary>
/// <remarks>
/// A rename on one side alone gives no compile error, because the seam is text and reflection.
/// Thus a test of each name stands in the place of the compiler (T-2, T-3).
/// </remarks>
public sealed class DebugSeamTests
{
    /// <summary>The type of Game that holds the seam.</summary>
    private const string SeamTypeName = "TheThingBelow.Game.DebugSeam";

    /// <summary>The project file of Game, which carries the reference of the assembly.</summary>
    private const string GameProjectPath = "TheThingBelow.Game/TheThingBelow.Game.csproj";

    /// <summary>The workflow that exports each build and reads it (D-449, D-503).</summary>
    private const string ExportWorkflowPath = ".github/workflows/export.yml";

    /// <summary>The configuration that the Godot export builds (D-726).</summary>
    private const string ExportReleaseConfiguration = "ExportRelease";

    [Fact]
    public void TheSeamNamesTheAssemblyThatTheBuildWrote()
    {
        Assert.Equal(
            Seam("AssemblyName"),
            DebugAssemblyFile.Load().GetName().Name);
    }

    [Fact]
    public void TheSeamNamesTheEntryTypeOfThatAssembly()
    {
        Assert.Equal(Seam("EntryTypeName"), DebugAssemblyFile.EntryType().FullName);
    }

    [Theory]
    [InlineData("HandlersMember")]
    [InlineData("ConsoleMember")]
    [InlineData("ShownLinesMember")]
    [InlineData("RunMember")]
    [InlineData("CommandNamesMember")]
    public void EachMemberThatTheSeamNamesIsAPublicStaticMemberOfTheEntry(string constant)
    {
        MethodInfo found = DebugAssemblyFile.Member(Seam(constant));

        Assert.True(found.IsStatic);
        Assert.True(found.IsPublic);
    }

    [Fact]
    public void TheHandlersMemberGivesTheHandlerSetOfCoreAndTakesNothing()
    {
        MethodInfo found = DebugAssemblyFile.Member(Seam("HandlersMember"));

        Assert.Equal(typeof(DebugIntentHandlers), found.ReturnType);
        Assert.Empty(found.GetParameters());
    }

    [Fact]
    public void TheConsoleMemberTakesTheThreeDelegatesAndGivesAGodotControl()
    {
        // The return type is an engine type, so this test reads its name and never the type.
        // Tests loads no Godot assembly (D-614).
        MethodInfo found = DebugAssemblyFile.Member(Seam("ConsoleMember"));

        Assert.Equal("Godot.Control", found.ReturnType.FullName);
        Assert.Equal(
            [typeof(Func<RunState>), typeof(Action<Intent>), typeof(Func<bool>)],
            found.GetParameters().Select(parameter => parameter.ParameterType));
    }

    [Fact]
    public void TheShownLinesMemberTakesTheConsoleNode()
    {
        // The console owns its nodes, so the read of its lines lives in the debug assembly and
        // Game reads no node of the console (D-723).
        MethodInfo found = DebugAssemblyFile.Member(Seam("ShownLinesMember"));

        Assert.Equal(
            ["Godot.Control"],
            found.GetParameters().Select(parameter => parameter.ParameterType.FullName));
        Assert.Equal(typeof(IReadOnlyList<string>), found.ReturnType);
    }

    [Fact]
    public void TheRunMemberTakesTheLineAndTheThreeDelegates()
    {
        MethodInfo found = DebugAssemblyFile.Member(Seam("RunMember"));

        Assert.Equal(
            [typeof(string), typeof(Func<RunState>), typeof(Action<Intent>), typeof(Func<bool>)],
            found.GetParameters().Select(parameter => parameter.ParameterType));
    }

    [Fact]
    public void TheGameProjectKeepsTheDebugReferenceOutOfTheExportReleaseConfiguration()
    {
        // Exit test 2 of section 7.4 of `phase-2-first-playable.md`, on the fast leg. The
        // Godot export runs `dotnet publish` in the `ExportRelease` configuration, so a
        // reference with this condition never reaches a release export (D-492, D-726).
        XElement reference = DebugReference();
        string condition = ConditionOf(reference);

        Assert.Contains(ExportReleaseConfiguration, condition, StringComparison.Ordinal);
        Assert.Contains("!=", condition, StringComparison.Ordinal);
        Assert.Contains("$(Configuration)", condition, StringComparison.Ordinal);
    }

    [Fact]
    public void TheDebugReferenceIsTheOneReferenceUnderThatCondition()
    {
        // A second reference in the same group would leave the export with no Core and no
        // Storage, and the export would fail with no word of this condition (T-2).
        XElement group = DebugReference().Parent
            ?? throw new InvalidOperationException($"The reference has no group in '{GameProjectPath}' (T-2).");

        Assert.Single(group.Elements());
    }

    [Fact]
    public void TheExportJobReadsEachBuildForTheDebugAssembly()
    {
        // The second check of D-726, on the real export of each system. The step names the
        // assembly, and the job fails when a build holds it (D-492).
        string workflow = File.ReadAllText(RepositoryRoot.PathTo(ExportWorkflowPath));

        Assert.Contains("TheThingBelow.Debug", workflow, StringComparison.Ordinal);
    }

    /// <summary>Reads one text constant of the seam from the built Game assembly (D-614).</summary>
    /// <param name="constant">The name of the constant, such as `AssemblyName`.</param>
    /// <returns>The value of the constant.</returns>
    /// <exception cref="InvalidOperationException">The seam holds no such constant (T-2).</exception>
    private static string Seam(string constant)
    {
        FieldInfo found = GameAssemblyFile.Type(SeamTypeName)
            .GetField(constant, BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException(
                $"The type '{SeamTypeName}' holds no public constant '{constant}' (T-2).");

        return found.GetRawConstantValue() as string
            ?? throw new InvalidOperationException(
                $"The constant '{SeamTypeName}.{constant}' holds no text (T-2).");
    }

    /// <summary>Finds the reference of the debug assembly in the project file of Game.</summary>
    /// <returns>The `ProjectReference` element.</returns>
    /// <exception cref="InvalidOperationException">The project file holds no such reference (T-2).</exception>
    private static XElement DebugReference()
    {
        XDocument project = XDocument.Load(RepositoryRoot.PathTo(GameProjectPath));
        IEnumerable<XElement> references = project.Descendants("ProjectReference");

        foreach (XElement reference in references)
        {
            string include = reference.Attribute("Include")?.Value ?? string.Empty;
            if (include.Contains("TheThingBelow.Debug", StringComparison.Ordinal))
            {
                return reference;
            }
        }

        throw new InvalidOperationException(
            $"The project file '{GameProjectPath}' holds no reference of the debug assembly (D-260, T-2).");
    }

    /// <summary>
    /// Gives the condition of an element or of the group that holds it. MSBuild reads both,
    /// so a test that read one alone would pass a project file that fails the rule.
    /// </summary>
    private static string ConditionOf(XElement element)
    {
        string own = element.Attribute("Condition")?.Value ?? string.Empty;
        string group = element.Parent?.Attribute("Condition")?.Value ?? string.Empty;
        return $"{own} {group}";
    }
}
