using System;
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

    private static Simulation Start()
    {
        ContentSet content = Content.Value;
        return Simulation.Start(0, MapSet.Of(content.Maps), content.Bots.StartOf(0), content.Battle, content.Notices, content.Story, DebugIntentHandlers.None);
    }
}
