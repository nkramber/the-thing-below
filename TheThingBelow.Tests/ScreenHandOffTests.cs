using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using TheThingBelow.Core;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Effects;
using TheThingBelow.Core.Runs;
using TheThingBelow.Storage;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The hand-off between the map and a fight on screen: the transition into the fight, the fade
/// into it, and the fade back to the map before the wait intent (D-522, D-938, D-939, D-941). The
/// tests read the built Game assembly, because Tests takes no reference to Game (D-614).
/// </summary>
public sealed class ScreenHandOffTests
{
    private const string HandOffTypeName = "TheThingBelow.Game.ScreenHandOff";

    private const string RunTypeName = "TheThingBelow.Game.GameRun";

    private const string WalkTypeName = "TheThingBelow.Game.BattleWalk";

    /// <summary>The seed of the fixture runs, as the smoke session and the captures take it.</summary>
    private const ulong Seed = 20260918;

    private const int FadeTicks = 20;

    private static readonly Lazy<ContentSet> Content =
        new(() => ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find())));

    [Fact]
    public void EachPhaseLastsItsTicksAndThenMovesToTheNext()
    {
        // D-938, D-939, D-941: 60 ticks of the transition, 20 of the fade in, then no phase.
        HandOff handOff = HandOff.Make(FadeTicks);
        Transition shatter = Transition.Read("""{ "comment": "a test", "id": "transition.shatter", "look": "shatter", "ticks": 60, "cover": "k" }"""u8, "test.json");

        handOff.StartInto(shatter, 1000);
        Assert.Equal("Into", handOff.Phase);
        Assert.True(handOff.HoldsEvents);
        Assert.Equal(0, handOff.ProgressOf(1000));
        Assert.Equal(500, handOff.ProgressOf(1030));

        handOff.Follow(1059);
        Assert.Equal("Into", handOff.Phase);

        handOff.Follow(1060);
        Assert.Equal("FadeIn", handOff.Phase);
        Assert.Equal(1060, handOff.Since);
        Assert.False(handOff.HoldsEvents);

        handOff.Follow(1080);
        Assert.Equal("None", handOff.Phase);

        handOff.StartBack(2000);
        Assert.True(handOff.ShowsMapAgain);
        handOff.Follow(2019);
        Assert.Equal("Back", handOff.Phase);
        handOff.Follow(2020);
        Assert.Equal("Waiting", handOff.Phase);
        Assert.True(handOff.ShowsMapAgain);

        handOff.End(2021);
        Assert.Equal("None", handOff.Phase);
        Assert.False(handOff.ShowsMapAgain);
    }

    [Fact]
    public void AStartDuringAPhaseFailsWithThePhaseAndTheTick()
    {
        // T-2: a second transition over one that plays is a fault of the screen, never a restart.
        HandOff handOff = HandOff.Make(FadeTicks);
        handOff.StartBack(50);

        TargetInvocationException thrown = Assert.Throws<TargetInvocationException>(() => handOff.StartBack(55));

        InvalidOperationException error = Assert.IsType<InvalidOperationException>(thrown.InnerException);
        Assert.Contains("tick 55", error.Message, StringComparison.Ordinal);
        Assert.Contains("'Back'", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheHandOffCountsTicksOfTheRunAndNoTimer()
    {
        // G-23, D-522: the file of the hand-off reads no clock, no timer, and no frame time of
        // Godot, so only the ticks of the run move it.
        string text = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "TheThingBelow.Game", "scripts", "ScreenHandOff.cs"));
        string code = Regex.Replace(text, "//.*", string.Empty);

        Assert.DoesNotMatch(new Regex(@"\b(Godot|Timer|CreateTimer|Time\.|DateTime|Stopwatch|double|float|delta)\b"), code);
    }

    [Fact]
    public void TheMapWaitsForTheWaitIntentAfterTheFadeAndNeverForATimer()
    {
        // Exit test 4 of PR-60 (D-522, D-938, D-939, G-23). The walk meets the hall patrol. The
        // events of the fight wait for the 60 ticks of the transition, and the fight shows under
        // the fade. After the flee, the fight stays in the run through the 20 ticks of the fade
        // back, and a frame that runs no tick moves nothing. The wait intent then ends the fight,
        // and the record holds it, so a replay reaches the same state with no screen.
        GameRunProbe run = GameRunProbe.Start();
        run.WalkToFight();

        long start = run.Tick;
        Assert.Equal("Into", run.HandOff.Phase);
        Assert.Equal(start, run.HandOff.Since);
        Assert.Equal(60, run.HandOff.TransitionTicks);
        for (int tick = 1; tick < 60; tick += 1)
        {
            run.OneTick();
            Assert.Equal("Into", run.HandOff.Phase);
            Assert.False(run.ShowsBattle, $"tick {run.Tick}: the fight shows during the transition");
        }

        run.OneTick();
        Assert.Equal("FadeIn", run.HandOff.Phase);
        Assert.True(run.ShowsBattle);

        run.FightToFadeBack();
        long back = run.Tick;
        Assert.Equal(back, run.HandOff.Since);
        Assert.False(run.ShowsBattle);
        for (int tick = 1; tick < FadeTicks; tick += 1)
        {
            run.Advance(0.0);
            Assert.Equal(back + tick - 1, run.Tick);
            run.OneTick();
            Assert.Equal("Back", run.HandOff.Phase);
            Assert.NotNull(run.State.Battle);
        }

        run.OneTick();
        Assert.Equal("Waiting", run.HandOff.Phase);
        Assert.NotNull(run.State.Battle);

        run.OneTick();
        Assert.Null(run.State.Battle);
        Assert.Equal("None", run.HandOff.Phase);

        RunRecord record = run.Record();
        TickIntents wait = Assert.Single(record.Ticks, line => line.Intents.Count > 0 && line.Intents[0].Action.Value == IntentIds.WaitBattleEnd.Value);
        Assert.Equal(back + FadeTicks + 1, wait.Tick);

        RunState replayed = RunReplay.Play(record, Content.Value.Hash, Content.Value.Map(record.Snapshot.MapIdOrFirst), Content.Value.Battle, DebugIntentHandlers.None);
        Assert.Equal(run.StateHash(), replayed.StateHash());
    }

    /// <summary>The hand-off of the Game assembly, read by reflection (D-614).</summary>
    private sealed class HandOff(object instance)
    {
        private readonly Type type = instance.GetType();

        public string Phase => this.Get("Phase").ToString()!;

        public long Since => (long)this.Get("Since");

        public bool HoldsEvents => (bool)this.Get("HoldsEvents");

        public bool ShowsMapAgain => (bool)this.Get("ShowsMapAgain");

        public int TransitionTicks => ((Transition)this.Get("Transition")).Ticks;

        public static HandOff Make(int fadeTicks) =>
            new(Activator.CreateInstance(GameAssemblyFile.Type(HandOffTypeName), [fadeTicks])
                ?? throw new InvalidOperationException("The hand-off type made no value (T-2)."));

        public void StartInto(Transition transition, long tick) => this.Call("StartInto", transition, tick);

        public void StartBack(long tick) => this.Call("StartBack", tick);

        public void Follow(long tick) => this.Call("Follow", tick);

        public void End(long tick) => this.Call("End", tick);

        public int ProgressOf(long tick) => (int)this.Call("ProgressOf", tick)!;

        private object Get(string name) =>
            (this.type.GetProperty(name) ?? throw new InvalidOperationException($"The hand-off holds no '{name}' (T-2)."))
                .GetValue(instance) ?? throw new InvalidOperationException($"The value '{name}' of the hand-off is null (T-2).");

        private object? Call(string name, params object[] arguments) =>
            (this.type.GetMethod(name) ?? throw new InvalidOperationException($"The hand-off holds no '{name}' method (T-2)."))
                .Invoke(instance, arguments);
    }

    /// <summary>A run of the Game assembly, with the walk of the fixture fight (D-614, D-767).</summary>
    private sealed class GameRunProbe(object instance)
    {
        private readonly Type type = instance.GetType();
        private readonly Type walk = GameAssemblyFile.Type(WalkTypeName);

        public long Tick => (long)this.Get("Tick");

        public bool ShowsBattle => (bool)this.Get("ShowsBattle");

        public RunState State => (RunState)this.Get("State");

        public HandOff HandOff => new(this.Get("HandOff"));

        public static GameRunProbe Start()
        {
            MethodInfo start = GameAssemblyFile.Type(RunTypeName).GetMethod(
                "Start",
                [typeof(ContentSet), typeof(ulong), typeof(DebugIntentHandlers), typeof(MessageSpeed)])
                ?? throw new InvalidOperationException("The run holds no 'Start' method (T-2).");
            return new(start.Invoke(null, [Content.Value, Seed, DebugIntentHandlers.None, MessageSpeed.Normal])
                ?? throw new InvalidOperationException("The 'Start' method gave no run (T-2)."));
        }

        /// <summary>Walks into the hall patrol until the transition into the fight starts.</summary>
        public void WalkToFight()
        {
            for (int tick = 0; tick < 6000; tick += 1)
            {
                if (this.HandOff.Phase != "None")
                {
                    return;
                }

                if (!(bool)this.Get("InBattle"))
                {
                    this.Queue(Intent.OfPlayer((ContentId)this.walk.GetMethod("StepOf")!.Invoke(null, [this.State.Party])!));
                }

                this.OneTick();
            }

            throw new InvalidOperationException("The walk met no fight in 6000 ticks (T-2).");
        }

        /// <summary>Tries to flee on each command, until the fade back to the map starts. The party loses the hall fight at this seed, and a flee ends a fight as a win does (D-522, D-938).</summary>
        public void FightToFadeBack()
        {
            for (int tick = 0; tick < 12000; tick += 1)
            {
                if (this.HandOff.Phase == "Back")
                {
                    return;
                }

                if ((bool)this.Get("WipeReady"))
                {
                    throw new InvalidOperationException($"The party fell at tick {this.Tick}, and the test needs a flee (T-2).");
                }

                if ((bool)this.Get("TakesBattleCommand"))
                {
                    this.Queue(Intent.OfPlayer(IntentIds.BattleFlee));
                }

                this.OneTick();
            }

            throw new InvalidOperationException("The fight reached no fade back in 12000 ticks (T-2).");
        }

        public void OneTick() => this.walk.GetMethod("OneTick")!.Invoke(null, [instance]);

        public void Advance(double seconds) =>
            this.type.GetMethod("Advance", [typeof(double), typeof(Func<Intent>)])!.Invoke(instance, [seconds, null]);

        public RunRecord Record() => (RunRecord)this.type.GetMethod("Record", Type.EmptyTypes)!.Invoke(instance, null)!;

        public ulong StateHash() => (ulong)this.type.GetMethod("StateHash", Type.EmptyTypes)!.Invoke(instance, null)!;

        private void Queue(Intent intent) => this.type.GetMethod("Queue", [typeof(Intent)])!.Invoke(instance, [intent]);

        private object Get(string name) =>
            (this.type.GetProperty(name) ?? throw new InvalidOperationException($"The run holds no '{name}' (T-2)."))
                .GetValue(instance) ?? throw new InvalidOperationException($"The value '{name}' of the run is null (T-2).");
    }
}
