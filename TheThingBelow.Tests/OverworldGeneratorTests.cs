using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Tools.Worldgen;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The generator of the overworld, its settings, and the `overworld` command (D-1294 to D-1296).
/// </summary>
public sealed class OverworldGeneratorTests
{
    private const string MapPath = "content/rules/maps/overworld.json";
    private const string PlanPath = "content/worldgen/overworld.json";

    [Fact]
    public void TheCommittedMapMatchesTheOutputOfItsSettings()
    {
        // D-1295: the map file is the output of its settings, as the atlas is of the drawings.
        string text = File.ReadAllText(RepositoryRoot.PathTo(MapPath), Encoding.UTF8);

        string written = OverworldCommand.Rewrite(text, OverworldGenerator.Generate(CheckoutPlan()), "rules/maps/overworld.json");

        Assert.True(string.CompareOrdinal(text, written) == 0, "The map differs from the output of its settings. Run `overworld --root .` and read the new map.");
    }

    [Fact]
    public void TheSameSettingsGiveTheSameMap()
    {
        // D-1296, T-7: integer math and one PCG stream, so each run and each CI leg agree.
        GeneratedOverworld first = OverworldGenerator.Generate(CheckoutPlan());
        GeneratedOverworld second = OverworldGenerator.Generate(CheckoutPlan());

        Assert.Equal(first.Terrain, second.Terrain);
        Assert.Equal(first.Zones, second.Zones);
        Assert.Equal(first.Things.Select(thing => (thing.Id.Value, thing.At)), second.Things.Select(thing => (thing.Id.Value, thing.At)));
    }

    [Fact]
    public void EachSeedGivesAMapOfItsSizeOrAnErrorThatNamesTheSeedAndTheRule()
    {
        // T-2: a seed loop. A seed that breaks a rule of the layout fails with its seed, and never
        // writes a map with a way around a gate.
        int made = 0;
        for (int seed = 1; seed <= 6; seed += 1)
        {
            OverworldPlan plan = PlanOf(PlanText().Replace("\"seed\": 1293,", $"\"seed\": {seed},", StringComparison.Ordinal));
            try
            {
                GeneratedOverworld map = OverworldGenerator.Generate(plan);
                Assert.True(map.Terrain.Count == plan.Height && map.Terrain.All(row => row.Length == plan.Width), $"Seed {seed}: the map is not {plan.Width} by {plan.Height}.");
                made += 1;
            }
            catch (InvalidOperationException error)
            {
                Assert.True(error.Message.Contains($"the seed {seed} breaks a rule", StringComparison.Ordinal), $"Seed {seed}: the error names no seed: {error.Message}");
            }
        }

        Assert.True(made > 0, "No seed of 1 to 6 made a map.");
    }

    [Fact]
    public void ATileFixSetsItsTileAfterEveryOtherStep()
    {
        // D-1295: a fix survives each regenerate.
        GeneratedOverworld plain = OverworldGenerator.Generate(CheckoutPlan());
        TilePoint rock = FirstTile(plain, TileKinds.MountainCharacter);

        GeneratedOverworld fixedMap = OverworldGenerator.Generate(PlanOf(WithFix(rock, TileKinds.SnowPeakCharacter)));

        Assert.Equal(TileKinds.SnowPeakCharacter, fixedMap.Terrain[rock.Y][rock.X]);
    }

    [Fact]
    public void AFixThatBlocksAPlaceFailsWithTheSeedAndTheRule()
    {
        // D-1281, T-2: the checks run after the fixes, so a fix cannot hide a broken rule.
        TilePoint mine = OverworldGenerator.Generate(CheckoutPlan()).Things.Single(thing => thing.Id.Value == "mark.overworld_mine").At;

        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => OverworldGenerator.Generate(PlanOf(WithFix(mine, TileKinds.MountainCharacter))));

        Assert.Contains("the seed 1293 breaks a rule", error.Message, StringComparison.Ordinal);
        Assert.Contains("mark.overworld_mine", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ATreasureWhoseClosureCutsAPathFailsWithTheSeedAndTheRule()
    {
        // D-1306, T-2: a cairn is solid, so a fix that leaves the lead one way past it, from the
        // west side to the east side, fails the check with the id of the treasure.
        TilePoint at = OverworldGenerator.Generate(CheckoutPlan()).Things.Single(thing => thing.Id.Value == "chest.overworld_cairn_west_field").At;
        List<(TilePoint At, char Tile)> fixes = [];
        foreach (int dx in new[] { -1, 0, 1 })
        {
            fixes.Add((new TilePoint(at.X + dx, at.Y - 1), TileKinds.MountainCharacter));
            fixes.Add((new TilePoint(at.X + dx, at.Y + 1), TileKinds.MountainCharacter));
        }

        fixes.Add((new TilePoint(at.X - 1, at.Y), TileKinds.GrassCharacter));
        fixes.Add((new TilePoint(at.X + 1, at.Y), TileKinds.GrassCharacter));

        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => OverworldGenerator.Generate(PlanOf(WithFixes(fixes))));

        Assert.Contains("the seed 1293 breaks a rule", error.Message, StringComparison.Ordinal);
        Assert.Contains("the solid treasure 'chest.overworld_cairn_west_field'", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AFixThatShutsATreasureInFailsWithTheSeedAndTheRule()
    {
        // D-1306, T-2: a treasure that no lead reaches fails the check with its id.
        TilePoint at = OverworldGenerator.Generate(CheckoutPlan()).Things.Single(thing => thing.Id.Value == "chest.overworld_cairn_high_hollow").At;
        List<(TilePoint At, char Tile)> fixes = [];
        foreach (TilePoint step in new TilePoint[] { new(1, 0), new(-1, 0), new(0, 1), new(0, -1) })
        {
            fixes.Add((new TilePoint(at.X + step.X, at.Y + step.Y), TileKinds.MountainCharacter));
        }

        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => OverworldGenerator.Generate(PlanOf(WithFixes(fixes))));

        Assert.Contains("the lead cannot reach the treasure 'chest.overworld_cairn_high_hollow'", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("{ \"role\": \"mine_gate\", \"id\": \"gate.overworld_mine_mouth\" }", "{ \"role\": \"mine_gate\", \"id\": \"gate.overworld_mine_mouth\", \"x\": 1, \"y\": 1 }", "holds no field 'x'")]
    [InlineData("\"role\": \"fort\",", "\"role\": \"keep\",", "the role 'keep'")]
    [InlineData("\"role\": \"fort\",", "\"role\": \"town\",", "2 things take the role 'town'")]
    [InlineData("{ \"from\": \"low\", \"to\": \"valley\",", "{ \"from\": \"low\", \"to\": \"moor\",", "names a basin that the file lacks")]
    [InlineData("\"land\": \"pass\",", "\"land\": \"peak\",", "the land 'peak'")]
    [InlineData("\"fixes\": []", "\"fixes\": [{ \"x\": 160, \"y\": 1, \"tile\": \"^\" }]", "lies outside the map")]
    [InlineData("\"ridge\": 70,", "\"ridge\": 300,", "the ridge runs from 0 to 256")]
    [InlineData("\"fixes\": []", "\"fixes\": [{ \"x\": 1, \"y\": 1, \"tile\": \"?\" }]", "the tile '?'")]
    [InlineData("\"south_x\": 80,\n", "", "south_x")]
    [InlineData("{ \"id\": \"chest.overworld_cairn_west_field\", \"x\": 23,", "{ \"id\": \"chest.overworld_cairn_west_field\", \"x\": 160,", "names the tile (160, 87)")]
    [InlineData("{ \"id\": \"chest.overworld_cairn_high_hollow\", \"x\"", "{ \"id\": \"chest.overworld_cairn_west_field\", \"x\"", "takes an id that another thing of the file takes")]
    [InlineData("{ \"id\": \"chest.overworld_cairn_west_field\", \"x\"", "{ \"id\": \"mark.overworld_cairn_west_field\", \"x\"", "this file holds entries of the kind 'chest'")]
    public void ABrokenSettingsFileFailsWithTheReason(string old, string replacement, string reason)
    {
        // D-1295, G-6, T-2.
        string text = PlanText();
        Assert.Contains(old, text, StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => PlanOf(text.Replace(old, replacement, StringComparison.Ordinal)));

        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheCheckPassesOnTheCheckoutAndFailsOnAChangedMap()
    {
        // D-1295: the command compares the map file with the output, and writes it back.
        string root = CopyOfTheTwoFiles();
        try
        {
            Assert.Equal(0, Run(root, check: true, out _));

            string mapFile = Path.Combine(root, MapPath);
            string text = File.ReadAllText(mapFile, Encoding.UTF8);
            int row = text.IndexOf("\"terrain\": [", StringComparison.Ordinal);
            int tile = text.IndexOf('^', row);
            File.WriteAllText(mapFile, string.Concat(text.AsSpan(0, tile), "A", text.AsSpan(tile + 1)), new UTF8Encoding(false));

            Assert.Equal(1, Run(root, check: true, out string errors));
            Assert.Contains("rules/maps/overworld.json", errors, StringComparison.Ordinal);

            Assert.Equal(0, Run(root, check: false, out _));
            Assert.Equal(text, File.ReadAllText(mapFile, Encoding.UTF8));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static string PlanText() => File.ReadAllText(RepositoryRoot.PathTo(PlanPath), Encoding.UTF8);

    private static OverworldPlan PlanOf(string text) => OverworldPlan.Read(Encoding.UTF8.GetBytes(text), OverworldPlan.Path);

    private static OverworldPlan CheckoutPlan() => PlanOf(PlanText());

    private static string WithFix(TilePoint at, char tile) => WithFixes([(at, tile)]);

    private static string WithFixes(IEnumerable<(TilePoint At, char Tile)> fixes)
    {
        string lines = string.Join(", ", fixes.Select(fix => $"{{ \"x\": {fix.At.X}, \"y\": {fix.At.Y}, \"tile\": \"{fix.Tile}\" }}"));
        return PlanText().Replace("\"fixes\": []", $"\"fixes\": [{lines}]", StringComparison.Ordinal);
    }

    /// <summary>Gives the first tile of a kind in reading order that no thing stands on and no thing touches.</summary>
    private static TilePoint FirstTile(GeneratedOverworld map, char tile)
    {
        for (int y = 1; y < map.Terrain.Count - 1; y += 1)
        {
            for (int x = 1; x < map.Terrain[y].Length - 1; x += 1)
            {
                if (map.Terrain[y][x] == tile && map.Things.All(thing => Math.Abs(thing.At.X - x) > 1 || Math.Abs(thing.At.Y - y) > 1))
                {
                    return new TilePoint(x, y);
                }
            }
        }

        throw new InvalidOperationException($"The map holds no tile '{tile}' away from each thing.");
    }

    private static string CopyOfTheTwoFiles()
    {
        string root = Path.Combine(Path.GetTempPath(), $"overworld-{Guid.NewGuid():N}");
        foreach (string path in new[] { MapPath, PlanPath })
        {
            string target = Path.Combine(root, path);
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(RepositoryRoot.PathTo(path), target);
        }

        return root;
    }

    private static int Run(string root, bool check, out string errors)
    {
        using var output = new StringWriter();
        using var faults = new StringWriter();
        string[] args = check ? [OverworldCommand.RootOption, root, OverworldCommand.CheckOption] : [OverworldCommand.RootOption, root];
        int code = OverworldCommand.Run(args, output, faults);
        errors = faults.ToString();
        return code;
    }
}
