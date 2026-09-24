using System;
using System.Collections.Generic;
using TheThingBelow.Core;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The rule that holds the torch out or puts it away, and its effect on the sight of a guard
/// in a run (D-1063, D-1064, D-1071).
/// </summary>
public sealed class TorchRulesTests
{
    private const ulong Seed = 91;

    [Fact]
    public void ARunStartsWithTheTorchPutAway()
    {
        // D-1064: the party first gets the torch put away.
        Assert.False(StartWithTorch(TestMaps.Room).State.Characters.TorchHeld);
    }

    [Fact]
    public void TheIntentsHoldTheTorchOutAndPutItAway()
    {
        Simulation run = StartWithTorch(TestMaps.Room);
        ulong away = run.StateHash();

        IReadOnlyList<LogEntry> log = run.Step([Intent.OfPlayer(IntentIds.HoldTorch)]);
        Assert.True(run.State.Characters.TorchHeld);
        Assert.Contains(log, entry => entry.Message == "the party held the torch out");
        Assert.NotEqual(away, run.StateHash());

        _ = run.Step([Intent.OfPlayer(IntentIds.PutTorchAway)]);
        Assert.False(run.State.Characters.TorchHeld);
    }

    [Fact]
    public void AnIntentThatChangesNothingIsAnError()
    {
        // T-2: an intent that changes nothing points at a fault in the screen that made it.
        Simulation run = StartWithTorch(TestMaps.Room);

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Intent.OfPlayer(IntentIds.PutTorchAway)]));

        Assert.Contains("put away, which it already is", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheTorchWorksOnTheWalkAloneAndNeverInAMenu()
    {
        Simulation run = StartWithTorch(TestMaps.Room);
        _ = run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Intent.OfPlayer(IntentIds.HoldTorch)]));

        Assert.Contains("D-1071", error.Message, StringComparison.Ordinal);
        Assert.False(run.State.Characters.TorchHeld);
    }

    [Fact]
    public void TheTorchNeverWorksInAFight()
    {
        Simulation run = StartWithTorch(BattleRuns.Map("group.test_pair"));
        while (run.State.Battle is null)
        {
            _ = run.Step(run.State.Party.Patrols.Encounter is null ? [Intent.OfPlayer(IntentIds.MoveEast)] : []);
        }

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Intent.OfPlayer(IntentIds.HoldTorch)]));

        Assert.Contains("a battle holds the run", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePartyHoldsNoTorchThatThePackLacks()
    {
        Simulation run = Start(TestMaps.Room);

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Intent.OfPlayer(IntentIds.HoldTorch)]));

        Assert.Contains("the pack holds no 'item.torch'", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheStateOfTheTorchLastsThroughASnapshot()
    {
        Simulation run = StartWithTorch(TestMaps.Room);
        _ = run.Step([Intent.OfPlayer(IntentIds.HoldTorch)]);

        string line = RunSnapshotText.Write(run.Snapshot());
        ContentReader reader = new(System.Text.Encoding.UTF8.GetBytes(line), "the test");
        Simulation resumed = Simulation.Resume(Seed, RunSnapshotText.Read(ref reader), TestMaps.Room, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

        Assert.Contains("\"torch_held\":true", line, StringComparison.Ordinal);
        Assert.True(resumed.State.Characters.TorchHeld);
        Assert.Equal(run.StateHash(), resumed.StateHash());
    }

    [Fact]
    public void ASnapshotThatHoldsTheTorchOutWithNoTorchInThePackIsAnError()
    {
        Simulation run = StartWithTorch(TestMaps.Room);
        _ = run.Step([Intent.OfPlayer(IntentIds.HoldTorch)]);
        RunSnapshot snapshot = run.Snapshot();
        PartySnapshot party = snapshot.Characters ?? throw new InvalidOperationException("The snapshot holds no party (T-2).");
        var pack = new List<PackValues>();
        foreach (PackValues entry in party.Pack)
        {
            if (string.CompareOrdinal(entry.Id.Value, TorchRules.Torch.Value) != 0)
            {
                pack.Add(entry);
            }
        }

        ArgumentException error = Assert.Throws<ArgumentException>(() => Simulation.Resume(
            Seed, snapshot with { Characters = party with { Pack = pack } }, TestMaps.Room, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None));

        Assert.Contains("the pack holds no 'item.torch'", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(false, 2)]
    [InlineData(true, 6)]
    public void AGuardOfADarkMapSeesThePartyFromTheRangeOfTheTorch(bool held, int reach)
    {
        // D-1063: the guard at (12, 1) faces west and sees 2 tiles, and 6 tiles while the party
        // holds the torch out. The party walks east until the guard sees it.
        Simulation run = StartWithTorch(DarkHall);
        if (held)
        {
            _ = run.Step([Intent.OfPlayer(IntentIds.HoldTorch)]);
        }

        for (int tick = 0; tick < 600 && run.State.Party.Patrols.Mark is null; tick += 1)
        {
            _ = run.Step([Intent.OfPlayer(IntentIds.MoveEast)]);
        }

        PatrolState guard = Assert.Single(run.State.Party.Patrols.All);
        Assert.NotNull(run.State.Party.Patrols.Mark);
        Assert.Equal(reach, MapSight.Reach(run.State.Party.LeadAt, guard.At));
    }

    /// <summary>A dark hall with a guard that stands at the east end and faces west (D-1062).</summary>
    private static GameMap DarkHall { get; } = TestMaps.Of(
        "dark-hall.json",
        """
        {
         "comment": "a dark hall with one guard, for the tests of the torch",
         "id": "map.dark_hall",
         "region": "region.test",
         "label": "label.dark_hall",
         "time": "night",
         "dark": true,
         "terrain": [
          "################",
          "#..............#",
          "################"
         ],
         "things": [
          { "id": "spawn_point.dark_hall_start", "kind": "spawn_point", "x": 1, "y": 1 }
         ],
         "enemies": [
          {
           "id": "patrol.dark_hall_guard",
           "group": "group.test_pair",
           "size": "common",
           "facing": "west",
           "step_ticks": 32,
           "sight_range": 2,
           "routes": [
            { "times": ["dawn", "day", "dusk", "night"], "tiles": [{ "x": 12, "y": 1 }] }
           ]
          }
         ], "triggers": []
        }
        """);

    private static Simulation Start(GameMap map) =>
        Simulation.Start(Seed, map, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

    /// <summary>Starts a run whose pack holds the torch, which the fixture of the tests lacks.</summary>
    private static Simulation StartWithTorch(GameMap map)
    {
        Simulation run = Start(map);
        Assert.Equal(0, run.State.Characters.Pick(TorchRules.Torch, 1, TestBattles.Content));
        return run;
    }
}
