using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TheThingBelow.Tools;
using TheThingBelow.Tools.Edges;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The `edges` command on the checkout and on a small copy (D-501, D-1326, D-1327).</summary>
public sealed class EdgesCommandTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), $"edges-{Guid.NewGuid():N}");

    /// <summary>Removes the copy of the test.</summary>
    public void Dispose()
    {
        if (Directory.Exists(this.root))
        {
            Directory.Delete(this.root, recursive: true);
        }
    }

    [Fact]
    public void EachCommittedEdgeFileMatchesItsMapAndTheEdgeRules()
    {
        // Exit test 1 of PR-53: a stale edge file fails until the command runs again (D-501).
        (int code, string output, string errors) = Run(RepositoryRoot.Find(), EdgesCommand.CheckOption);

        Assert.True(code == 0, $"The check failed: {errors}");
        Assert.Contains("matches its map and the edge rules", output, StringComparison.Ordinal);
    }

    [Fact]
    public void TheCommandWritesTheEdgeFileOfEachMapUnderTheNameOfItsMapFile()
    {
        this.WriteCopy();

        (int code, string output, string errors) = Run(this.root);

        Assert.True(code == 0, $"The command failed: {errors}");
        Assert.Contains("edges: maps 1.", output, StringComparison.Ordinal);
        string written = File.ReadAllText(this.PathOf("edges/maps/edge-test.json"), Encoding.UTF8);
        Assert.Contains("{ \"x\": 1, \"y\": 1, \"pieces\": [\"edge.water_north\", \"edge.water_south\", \"edge.water_west\"] }", written, StringComparison.Ordinal);
        Assert.Equal(0, Run(this.root, EdgesCommand.CheckOption).Code);
    }

    [Fact]
    public void AStaleEdgeFileFailsTheCheckWithItsPath()
    {
        this.WriteCopy();
        Assert.Equal(0, Run(this.root).Code);
        File.WriteAllText(this.PathOf("rules/maps/edge-test.json"), EdgeFixtures.MapText(",,,,", ",,,,", ",,,,"), new UTF8Encoding(false));

        (int code, _, string errors) = Run(this.root, EdgesCommand.CheckOption);

        Assert.Equal(Program.FaultExitCode, code);
        Assert.Contains("'edges/maps/edge-test.json' differs", errors, StringComparison.Ordinal);
    }

    [Fact]
    public void AnAbsentEdgeFileFailsTheCheckWithItsPath()
    {
        this.WriteCopy();

        (int code, _, string errors) = Run(this.root, EdgesCommand.CheckOption);

        Assert.Equal(Program.FaultExitCode, code);
        Assert.Contains("'edges/maps/edge-test.json' does not exist", errors, StringComparison.Ordinal);
    }

    [Fact]
    public void AnEdgeFileWithNoMapFileFailsWithItsPath()
    {
        this.WriteCopy();
        Assert.Equal(0, Run(this.root).Code);
        File.WriteAllText(this.PathOf("edges/maps/gone.json"), EdgeFixtures.FileText(string.Empty), new UTF8Encoding(false));

        (int code, _, string errors) = Run(this.root);

        Assert.Equal(Program.FaultExitCode, code);
        Assert.Contains("'edges/maps/gone.json' names no map file", errors, StringComparison.Ordinal);
    }

    [Fact]
    public void ARuleWithAnAbsentPieceStopsTheCommandWithTheFile()
    {
        this.WriteCopy();
        string text = EdgeFixtures.WaterRuleText.Replace("  \"west\": \"edge.water_west\",\n", string.Empty, StringComparison.Ordinal);
        File.WriteAllText(this.PathOf(EdgeFixtures.WaterRulePath), text, new UTF8Encoding(false));

        (int code, _, string errors) = Run(this.root);

        Assert.Equal(Program.FaultExitCode, code);
        Assert.Contains(EdgeFixtures.WaterRulePath, errors, StringComparison.Ordinal);
        Assert.False(File.Exists(this.PathOf("edges/maps/edge-test.json")), "The command wrote an edge file from a broken rule.");
    }

    [Fact]
    public void ARootWithNoMapFails()
    {
        Directory.CreateDirectory(Path.Combine(this.root, "content", "edges", "kinds"));

        (int code, _, string errors) = Run(this.root);

        Assert.Equal(Program.FaultExitCode, code);
        Assert.Contains("found no map", errors, StringComparison.Ordinal);
    }

    private static (int Code, string Output, string Errors) Run(string root, params string[] more)
    {
        var args = new List<string> { EdgesCommand.Name, EdgesCommand.RootOption, root };
        args.AddRange(more);
        var output = new StringWriter();
        var errors = new StringWriter();

        int code = Program.Run([.. args], output, errors);
        return (code, output.ToString(), errors.ToString());
    }

    /// <summary>Writes a copy with the two edge rules and one map with a lake of two tiles.</summary>
    private void WriteCopy()
    {
        this.Write(EdgeFixtures.WaterRulePath, EdgeFixtures.WaterRuleText);
        this.Write(EdgeFixtures.GorgeRulePath, EdgeFixtures.GorgeRuleText);
        this.Write("rules/maps/edge-test.json", EdgeFixtures.MapText([",,,,", ",--,", ",,,,"]));
    }

    private void Write(string path, string text)
    {
        string full = this.PathOf(path);
        Directory.CreateDirectory(Path.GetDirectoryName(full) ?? throw new InvalidOperationException($"The path '{full}' has no folder."));
        File.WriteAllText(full, text, new UTF8Encoding(false));
    }

    private string PathOf(string path) => Path.Combine(this.root, "content", path.Replace('/', Path.DirectorySeparatorChar));
}
