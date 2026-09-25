using System;
using System.Collections.Generic;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// Exit test 13 of PR-14: a seed loop keeps each NPC in its rectangles, a route waits at each
/// tile, and a chaser closes on its target (D-1138). Each failure names its seed (T-3).
/// </summary>
/// <remarks>
/// Each run plays the yard of <see cref="HubMaps"/> through <see cref="Simulation"/>, with the
/// script of <see cref="RunScripts"/>: the lead walks at random, and the menu opens and closes,
/// so the lead blocks the NPCs and the world pauses (D-162, D-1139). The seed of the run picks
/// the draws of the dog, and the seed of the script picks the walk of the lead.
/// </remarks>
public sealed class NpcWalkPropertyTests
{
    /// <summary>The count of seeds of each property test of this class (T-3).</summary>
    private const int SeedCount = 200;

    /// <summary>The count of ticks of each run, which holds many paces and many route legs.</summary>
    private const int TickCount = 1500;

    private const int Barmaid = 1;
    private const int Dog = 2;
    private const int Child = 3;

    /// <summary>A target of a chase that stands: a route of one tile at (8, 5) (D-740).</summary>
    private const string Post = """{ "id": "npc.hub_post", "facing": "west", "step_ticks": 32, "move": "route", "tiles": [{ "x": 8, "y": 5, "wait_ticks": 0 }] }""";

    [Fact]
    public void EachWandererAndChaserStaysInsideItsRectangles()
    {
        for (ulong seed = 1; seed <= SeedCount; seed += 1)
        {
            Simulation run = Start(seed);
            foreach (IReadOnlyList<Intent> intents in RunScripts.Make(seed, TickCount))
            {
                run.Step(intents);
                foreach (NpcState npc in run.State.Party.Npcs.All)
                {
                    if (npc.Npc.Move == NpcMove.Route)
                    {
                        continue;
                    }

                    Assert.True(npc.Npc.RangeHolds(npc.At), $"Seed {seed}, tick {run.Tick}: the NPC '{npc.Npc.Id.Value}' stands at {npc.At}, outside its rectangles.");
                    Assert.True(
                        npc.StepEnd is not TilePoint end || npc.Npc.RangeHolds(end),
                        $"Seed {seed}, tick {run.Tick}: the NPC '{npc.Npc.Id.Value}' steps to {npc.StepEnd}, outside its rectangles.");
                }
            }
        }
    }

    [Fact]
    public void ARouteNpcWaitsAtLeastTheWaitOfEachRouteTile()
    {
        // D-1138: the barmaid waits 30 world ticks at (1, 2), none at (3, 2), and 60 at (3, 5). The
        // lead can stand in her way, and a blocked step tries again on the next tick, so a wait can
        // last longer and never shorter.
        for (ulong seed = 1; seed <= SeedCount; seed += 1)
        {
            Simulation run = Start(seed);
            RouteWatch watch = new(run.State.Party.Npcs.All[Barmaid]);
            foreach (IReadOnlyList<Intent> intents in RunScripts.Make(seed, TickCount))
            {
                run.Step(intents);
                if (watch.Read(run.State.Party.Npcs.All[Barmaid], run.State.WorldTick) is (int wanted, long waited))
                {
                    Assert.True(waited >= wanted, $"Seed {seed}, tick {run.Tick}: the barmaid left a route tile after {waited} world ticks, and the tile waits {wanted}.");
                }
            }

            Assert.True(watch.Departures > 4, $"Seed {seed}: the barmaid left only {watch.Departures} route tiles in {TickCount} ticks.");
        }
    }

    [Fact]
    public void ARouteNpcThatNothingBlocksWaitsExactlyTheWaitOfEachRouteTile()
    {
        // D-1138: with no input, the lead stands on the spawn point and blocks no leg, so each
        // wait lasts exactly its count of world ticks, the start tile included.
        Simulation run = Start(SaveRuns.Seed);
        RouteWatch watch = new(run.State.Party.Npcs.All[Barmaid]);
        List<(int Wanted, long Waited)> waits = [];

        for (int tick = 0; tick < TickCount; tick += 1)
        {
            run.Step([]);
            if (watch.Read(run.State.Party.Npcs.All[Barmaid], run.State.WorldTick) is (int wanted, long waited))
            {
                waits.Add((wanted, waited));
            }
        }

        Assert.True(waits.Count > 8, $"The barmaid left {waits.Count} route tiles.");
        Assert.Equal((30, 30), waits[0]);
        Assert.Equal((0, 0), waits[1]);
        Assert.Equal((60, 60), waits[2]);
        Assert.All(waits, pair => Assert.Equal(pair.Wanted, pair.Waited));
    }

    [Fact]
    public void EachStepOfAChaserBringsItCloserToItsTarget()
    {
        // D-1138: the child takes a step only when the step brings it closer to the tile of the dog
        // on that tick. The dog walks before the child in the order of the file, so the tile of the
        // dog at the end of the tick is the tile that the child read.
        for (ulong seed = 1; seed <= SeedCount; seed += 1)
        {
            Simulation run = Start(seed);
            int steps = 0;
            foreach (IReadOnlyList<Intent> intents in RunScripts.Make(seed, TickCount))
            {
                run.Step(intents);
                NpcState child = run.State.Party.Npcs.All[Child];
                if (child.StepEnd is not TilePoint end || child.StepTicks != 0)
                {
                    continue;
                }

                TilePoint dog = run.State.Party.Npcs.All[Dog].At;
                steps += 1;
                Assert.True(
                    Distance(end, dog) < Distance(child.At, dog),
                    $"Seed {seed}, tick {run.Tick}: the child steps from {child.At} to {end}, and the dog stands at {dog}.");
            }

            Assert.True(steps > 0, $"Seed {seed}: the child took no step in {TickCount} ticks.");
        }
    }

    [Fact]
    public void AChaserReachesAStandingTargetFromEachTileOfItsRange()
    {
        // D-1138: the child chases the keeper, who stands at (8, 5). The range is the two south rows
        // of the room, so the child needs no step around a wall, and it reaches the tile beside the
        // keeper within one pace for each step of the distance.
        for (int x = 1; x <= 8; x += 1)
        {
            for (int y = 5; y <= 6; y += 1)
            {
                if ((x, y) == (8, 5))
                {
                    continue;
                }

                GameMap map = HubMaps.Of(npcs: $"{Post}, {ChaserOfPost(x, y)}");
                Simulation run = Simulation.Start(SaveRuns.Seed, map, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);
                var goal = new TilePoint(8, 5);
                int ticks = checked(Distance(new TilePoint(x, y), goal) * 32);
                for (int tick = 0; tick <= ticks; tick += 1)
                {
                    run.Step([]);
                }

                NpcState child = run.State.Party.Npcs.All[1];
                Assert.True(Distance(child.At, goal) == 1, $"The child from ({x}, {y}) stands at {child.At} after {ticks} ticks.");
                Assert.Null(child.Stepping);
            }
        }
    }

    private static string ChaserOfPost(int x, int y) =>
        $$"""{ "id": "npc.hub_child", "facing": "west", "step_ticks": 32, "move": "chase", "x": {{x}}, "y": {{y}}, "areas": [{ "x": 1, "y": 5, "width": 8, "height": 2 }], "pace_ticks": 32, "target": "npc.hub_post" }""";

    private static Simulation Start(ulong seed) =>
        Simulation.Start(seed, HubMaps.Yard, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

    private static int Distance(TilePoint from, TilePoint to) => Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y);

    /// <summary>
    /// Follows one route NPC from tick to tick: the world tick of each arrival on a route tile, and
    /// the world tick of the step that leaves it. The NPC enters the map on its first route tile at
    /// world tick 0, as if it arrived there.
    /// </summary>
    private sealed class RouteWatch
    {
        private TilePoint lastAt;
        private int waitOfTile;
        private long arrivedAt;
        private bool onTile;

        public RouteWatch(NpcState npc)
        {
            this.lastAt = npc.At;
            this.waitOfTile = npc.Npc.Route[0].WaitTicks;
            this.arrivedAt = 0;
            this.onTile = true;
        }

        public int Departures { get; private set; }

        /// <summary>Reads the NPC after one tick, and gives the wait of a route tile that it left on this tick.</summary>
        public (int Wanted, long Waited)? Read(NpcState npc, long worldTick)
        {
            if (npc.At != this.lastAt)
            {
                this.lastAt = npc.At;
                this.onTile = TryWaitOf(npc, npc.At, out this.waitOfTile);
                this.arrivedAt = worldTick;
            }

            if (!this.onTile || npc.Stepping is null || npc.StepTicks != 0)
            {
                return null;
            }

            this.onTile = false;
            this.Departures += 1;
            return (this.waitOfTile, worldTick - this.arrivedAt);
        }

        private static bool TryWaitOf(NpcState npc, TilePoint at, out int wait)
        {
            foreach (RouteStop stop in npc.Npc.Route)
            {
                if (stop.At == at)
                {
                    wait = stop.WaitTicks;
                    return true;
                }
            }

            wait = 0;
            return false;
        }
    }
}
