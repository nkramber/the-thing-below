using System;
using System.Collections.Generic;
using TheThingBelow.Core;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Tools.Bots;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

public sealed class AcceptedIntentsTests
{
    private const int Seeds = 16;

    private const int Ticks = 2400;

    private const int SampleTicks = 20;

    private static readonly Lazy<ContentSet> Content =
        new(() => ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find())));

    /// <summary>Each intent with no field that a screen of Game makes (D-493).</summary>
    private static readonly ContentId[] PlainIntents =
    [
        IntentIds.OpenMenu,
        IntentIds.CloseMenu,
        IntentIds.Confirm,
        IntentIds.MoveNorth,
        IntentIds.MoveSouth,
        IntentIds.MoveEast,
        IntentIds.MoveWest,
        IntentIds.BattleDefend,
        IntentIds.BattleStep,
        IntentIds.BattleFlee,
        IntentIds.WaitBattleEnd,
        IntentIds.StoryStepEnd,
        IntentIds.StoryPause,
        IntentIds.StoryResume,
        IntentIds.HoldTorch,
        IntentIds.PutTorchAway,
        IntentIds.HubRest,
        IntentIds.HubSave,
    ];

    [Fact]
    public void EachListedIntentStepsWithNoRefusal()
    {
        // D-1179: the list holds the intents that the state accepts. A seed loop plays the random
        // bot over both start maps and steps each listed intent on a copy of each sampled state.
        foreach ((ulong seed, Simulation run) in SampledStates())
        {
            foreach (Intent intent in run.Accepted())
            {
                Simulation copy = CopyOf(run);
                Exception? fault = Record.Exception(() => copy.Step([intent]));
                Assert.True(fault is null, $"Seed {seed}, tick {run.Tick}: the listed intent {intent.Describe()} met a refusal: {fault?.Message}");
            }
        }
    }

    [Fact]
    public void EachPlainIntentOutsideTheListMeetsARefusal()
    {
        // D-1179: the list misses no intent with no field that the state accepts. A step in a
        // battle is the one exception: the rules read no step there, and Game never makes one
        // (the held step of Game stops in a battle).
        foreach ((ulong seed, Simulation run) in SampledStates())
        {
            List<string> listed = Describe(run.Accepted());
            foreach (ContentId action in PlainIntents)
            {
                Intent intent = Intent.OfPlayer(action);
                if (listed.Contains(intent.Describe()) || (run.State.Battle is not null && IsStep(action)))
                {
                    continue;
                }

                Simulation copy = CopyOf(run);
                Exception? fault = Record.Exception(() => copy.Step([intent]));
                Assert.True(fault is SimulationException, $"Seed {seed}, tick {run.Tick}: the rules took {intent.Describe()}, which the list misses. Accepted: {string.Join(", ", listed)}");
            }
        }
    }

    [Fact]
    public void TheWalkListsTheStepsTheConfirmTheMenuAndTheTorch()
    {
        // The party starts with the torch in the pack, so the walk lists its hold (D-1064).
        Simulation run = Start(0);

        List<string> listed = Describe(run.Accepted());

        Assert.Equal(
            ["intent.move_north", "intent.move_south", "intent.move_east", "intent.move_west", "intent.confirm", "intent.open_menu", "intent.hold_torch"],
            listed);
    }

    [Fact]
    public void AnOpenMenuListsTheCloseFirstAndNoStep()
    {
        Simulation run = Start(0);
        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);

        List<string> listed = Describe(run.Accepted());

        Assert.Equal("intent.close_menu", listed[0]);
        Assert.DoesNotContain("intent.move_north", listed);
        Assert.DoesNotContain("intent.confirm", listed);
    }

    [Fact]
    public void AWipeListsNoIntent()
    {
        // D-1181: the host reloads after a wipe, so the state offers the bot nothing.
        // The random bot wipes in some fights, so the first wipe of the seed loop gives the state.
        int wipes = 0;
        for (ulong seed = 0; seed < Seeds * 4 && wipes == 0; seed += 1)
        {
            Simulation run = Start(seed);
            RandomPolicy policy = new(seed);
            for (int tick = 0; tick < Ticks * 4; tick += 1)
            {
                if (run.State.Battle is Battle { Outcome: BattleOutcome.Wiped })
                {
                    Assert.True(run.Accepted().Count == 0, $"Seed {seed}, tick {run.Tick}: a wipe listed intents.");
                    wipes += 1;
                    break;
                }

                PlayOneTick(run, policy);
            }
        }

        Assert.True(wipes > 0, "No seed of the loop wiped, so the test read no wipe.");
    }

    [Fact]
    public void TheQueryChangesNoState()
    {
        // G-17: the query reads the state alone, so the simulation version stays.
        foreach ((ulong seed, Simulation run) in SampledStates())
        {
            ulong before = run.StateHash();
            _ = run.Accepted();
            Assert.True(run.StateHash() == before, $"Seed {seed}, tick {run.Tick}: the query changed the state hash.");
        }
    }

    /// <summary>Plays the random bot from each seed, and gives the run at each sample tick. A wipe ends the seed.</summary>
    private static IEnumerable<(ulong Seed, Simulation Run)> SampledStates()
    {
        for (ulong seed = 0; seed < Seeds; seed += 1)
        {
            Simulation run = Start(seed);
            RandomPolicy policy = new(seed);
            for (int tick = 0; tick < Ticks; tick += 1)
            {
                if (run.State.Battle is Battle { Outcome: BattleOutcome.Wiped })
                {
                    break;
                }

                if (tick % SampleTicks == 0)
                {
                    yield return (seed, run);
                }

                PlayOneTick(run, policy);
            }
        }
    }

    private static void PlayOneTick(Simulation run, RandomPolicy policy)
    {
        Intent? chosen = policy.Choose(run.State, run.Accepted());
        run.Step(chosen is null ? [] : [chosen]);
        _ = run.TakeBattleEvents();
        _ = run.TakeSaveRequests();
        _ = run.TakeNotices();
        _ = run.TakeOpenedServices();
    }

    private static Simulation Start(ulong seed)
    {
        ContentSet content = Content.Value;
        return Simulation.Start(seed, MapSet.Of(content.Maps), content.Bots.StartOf(seed), content.Battle, content.Notices, content.Story, DebugIntentHandlers.None);
    }

    private static Simulation CopyOf(Simulation run)
    {
        ContentSet content = Content.Value;
        return Simulation.Resume(run.State.Seed, run.Snapshot(), MapSet.Of(content.Maps), content.Battle, content.Notices, content.Story, DebugIntentHandlers.None);
    }

    private static bool IsStep(ContentId action) =>
        Array.Exists([IntentIds.MoveNorth, IntentIds.MoveSouth, IntentIds.MoveEast, IntentIds.MoveWest], step => string.CompareOrdinal(step.Value, action.Value) == 0);

    private static List<string> Describe(IReadOnlyList<Intent> intents)
    {
        List<string> names = [];
        foreach (Intent intent in intents)
        {
            names.Add(intent.Describe());
        }

        return names;
    }
}
