using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Tools.Bots;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

public sealed class SoftlockCheckTests
{
    private static readonly Lazy<ContentSet> Content =
        new(() => ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find())));

    [Fact]
    public void TheWalkIsNoSoftlock()
    {
        Simulation run = Start();

        Assert.False(SoftlockCheck.Holds(Content.Value, run, run.Accepted()));
    }

    [Fact]
    public void NoAcceptedIntentIsASoftlock()
    {
        // D-1179: with no intent, the player has nothing that changes the state.
        Simulation run = Start();

        Assert.True(SoftlockCheck.Holds(Content.Value, run, []));
    }

    [Fact]
    public void AnIntentThatChangesNothingButTheTickIsASoftlock()
    {
        // D-1179: at the start, the lead faces no NPC and no service, so a confirm ends with the
        // tick and changes nothing else (D-1131).
        Simulation run = Start();

        Assert.True(SoftlockCheck.Holds(Content.Value, run, [Intent.OfPlayer(IntentIds.Confirm)]));
    }

    [Fact]
    public void TheCheckChangesNoStateOfTheRun()
    {
        Simulation run = Start();
        ulong before = run.StateHash();

        _ = SoftlockCheck.Holds(Content.Value, run, run.Accepted());

        Assert.Equal(before, run.StateHash());
    }

    [Fact]
    public void TheToggleRuleGivesTheAnswerOfTheTrialAtEachSampledState()
    {
        // P2-1 of the review of PR #87: the check skips the trial when the state accepts a
        // toggle. A seed loop plays both policies over both start maps and compares the two
        // paths at each state, the battles of a story scene included.
        int trials = 0;
        for (ulong seed = 1; seed <= 12; seed += 1)
        {
            foreach (IBotPolicy policy in new IBotPolicy[] { new RandomPolicy(seed), new GreedyPolicy() })
            {
                Simulation run = Start(seed);
                for (int tick = 0; tick < 1200 && run.State.Battle is not Core.Battles.Battle { Outcome: Core.Battles.BattleOutcome.Wiped }; tick += 1)
                {
                    IReadOnlyList<Intent> accepted = run.Accepted();
                    bool quick = SoftlockCheck.Holds(Content.Value, run, accepted);
                    if (tick % 5 == 0 || !HoldsAToggle(accepted))
                    {
                        trials += 1;
                        Assert.True(
                            quick == SoftlockCheck.HoldsByTrial(Content.Value, run, accepted),
                            $"Seed {seed}, policy {BotPolicyKinds.NameOf(policy.Kind)}, tick {run.Tick}: the toggle rule and the trial disagree.");
                    }

                    Intent? chosen = policy.Choose(run.State, accepted);
                    run.Step(chosen is null ? [] : [chosen]);
                    _ = run.TakeBattleEvents();
                    _ = run.TakeSaveRequests();
                    _ = run.TakeNotices();
                    _ = run.TakeOpenedServices();
                }
            }
        }

        Assert.True(trials > 1000, $"The loop compared {trials} states, and the test needs more than 1000.");
    }

    private static bool HoldsAToggle(IReadOnlyList<Intent> accepted)
    {
        foreach (Intent intent in accepted)
        {
            foreach (ContentId toggle in SoftlockCheck.Toggles)
            {
                if (string.CompareOrdinal(intent.Action.Value, toggle.Value) == 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static Simulation Start() => Start(0);

    private static Simulation Start(ulong seed)
    {
        ContentSet content = Content.Value;
        return Simulation.Start(seed, MapSet.Of(content.Maps), content.Bots.StartOf(seed), content.Battle, content.Notices, content.Story, DebugIntentHandlers.None);
    }
}
