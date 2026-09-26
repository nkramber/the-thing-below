using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Story;
using TheThingBelow.Storage;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The story scene on screen: Game follows each step that Core runs, draws it from the ticks, and
/// sends one wait intent at its end (D-540, D-1000, PR-36). The tests drive the fixture story scenes
/// of the fixture hub through the built Game assembly, because Tests takes no reference to Game (D-614).
/// </summary>
public sealed class ScenePlayTests
{
    private const ulong Seed = 20260926;

    private const double OneTick = 1.0 / 60;

    /// <summary>The most frames that one part of a test runs before it fails (T-2).</summary>
    private const int FrameLimit = 6000;

    private static readonly Lazy<ContentSet> Content =
        new(() => ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find())));

    private static readonly ContentId Stranger = Id("npc.fixture_hub_stranger");

    private static readonly ContentId Guard = Id("npc.fixture_hub_guard");

    [Fact]
    public void TheStrangerSceneWalksTwoSpritesShowsALineWithAPortraitAndRecordsTheChoice()
    {
        // Exit test 1 of PR-36: a fixture story scene walks two sprites, shows a line with a
        // portrait, and records a choice (D-1006, D-1007, D-1175).
        Play play = Play.AtTheStranger();

        Assert.Equal("line.fixture_hub_barmaid_door", play.Line?.Value);
        Assert.Equal("npc.fixture_hub_barmaid", play.Speaker?.Id?.Value);
        ContentId art = (ContentId)GameValue.Static("ScenePlay", "ArtIdOf", play.Speaker)!;
        Assert.Equal("portraits", Content.Value.Atlas.Entry(art, "portrait").Page);
        Assert.Equal("name.fixture_hub_barmaid", ((ContentId)GameValue.Static("ScenePlay", "NameIdOf", play.Speaker)!).Value);

        var strangerX = new SortedSet<int>();
        var guardX = new SortedSet<int>();
        play.Until(() => play.Options is not null, () =>
        {
            play.Walked(Stranger, strangerX);
            play.Walked(Guard, guardX);
            play.PressWhenTyped();
        });

        Assert.True(strangerX.Count > 2, $"The stranger drew at {strangerX.Count} places of the walk.");
        Assert.True(guardX.Count > 2, $"The guard drew at {guardX.Count} places of the walk.");
        Assert.Equal("line.fixture_hub_stranger_ask", play.Line?.Value);
        Assert.Equal(2, play.Options!.Count);

        play.MoveCursor(1);
        play.Press();
        play.Frame();

        Assert.True(play.Run.State.Story.Flags.IsOn(Id("flag.fixture_hub_no")));
        Assert.False(play.Run.State.Story.Flags.IsOn(Id("flag.fixture_hub_yes")));
        play.Until(() => !play.Run.State.Story.Running && play.View is null, play.PressWhenTyped);
        Assert.Contains(play.Run.Record().Ticks.SelectMany(tick => tick.Intents), intent => intent.Option == 1 && string.CompareOrdinal(intent.Action.Value, IntentIds.StoryPick.Value) == 0);
    }

    [Fact]
    public void ASecondSpeakerOfTheSameLineDrawsItsPortraitAndItsNameAgain()
    {
        // A regression test of P2-1 of the review of PR-86 (D-223, D-997): the box drew the speaker
        // again on a new line id alone, so a second speaker of the same line kept the first portrait.
        // The content gives the line of the stranger to the barmaid on the next step.
        List<ContentFile> files = [.. ContentFolder.Read(RepositoryRoot.Find())];
        int place = files.FindIndex(file => file.Path.EndsWith("fixture-hub-stranger.json", StringComparison.Ordinal));
        string text = Encoding.UTF8.GetString(files[place].Bytes).Replace(
            """{ "id": "step.stranger_presses", "kind": "say", "speaker": "npc.fixture_hub_stranger", "line": "line.fixture_hub_stranger_ask" }""",
            """{ "id": "step.stranger_presses", "kind": "say", "speaker": "npc.fixture_hub_barmaid", "line": "line.fixture_hub_stranger_brother" }""",
            StringComparison.Ordinal);
        Assert.Contains("npc.fixture_hub_barmaid\", \"line\": \"line.fixture_hub_stranger_brother", text, StringComparison.Ordinal);
        files[place] = new ContentFile(files[place].Path, Encoding.UTF8.GetBytes(text));
        Play play = Play.AtTheStranger(ContentSet.Load(files));
        GameValue change = GameValue.New("DialogueChange");

        play.Until(() => play.Speaker?.Id?.Value == Stranger.Value, () =>
        {
            _ = change.Call("Take", play.Follower);
            play.PressWhenTyped();
        });
        Assert.Equal("line.fixture_hub_stranger_brother", play.Line?.Value);
        _ = change.Call("Take", play.Follower);

        play.Until(() => play.Speaker?.Id?.Value == "npc.fixture_hub_barmaid", () =>
        {
            play.PressWhenTyped();
        });
        object parts = change.Call("Take", play.Follower)!;

        Assert.Equal("line.fixture_hub_stranger_brother", play.Line?.Value);
        Assert.True((bool)parts.GetType().GetProperty("Speaker")!.GetValue(parts)!, "The second speaker of the same line drew no portrait and no name.");
        Assert.True((bool)parts.GetType().GetProperty("Line")!.GetValue(parts)!, "The new say step drew its line in no new type-out.");
    }

    [Fact]
    public void AStorySpriteMovesWithTheTicksOfTheRunAndNeverWithATimer()
    {
        // Exit test 3 of PR-36 (G-23, F-52): the slide reads the ticks of the run alone, and the
        // class holds no engine type and no clock.
        string code = File.ReadAllText(RepositoryRoot.PathTo("TheThingBelow.Game/scripts/Ui/ScenePlay.cs"));
        foreach (string refused in new[] { "Godot", "Timer", "Stopwatch", "DateTime", "delta" })
        {
            Assert.DoesNotContain(refused, code, StringComparison.Ordinal);
        }

        Play play = Play.AtTheStranger();
        play.Until(() => play.Walks(Stranger), play.PressWhenTyped);
        long start = play.Run.Tick;
        int firstX = play.WalkX(Stranger);

        for (int tick = 0; tick < 5; tick += 1)
        {
            play.Frame();
        }

        // A tile of 32 pixels takes 16 ticks (D-203).
        Assert.Equal(firstX + (int)((play.Run.Tick - start) * 2), play.WalkX(Stranger));
    }

    [Theory]
    [InlineData(30, 5)]
    [InlineData(60, 10)]
    [InlineData(120, 20)]
    public void TheLineTypesAtEachTextSpeed(int charactersPerSecond, int typed)
    {
        // Exit test 4 of PR-36 (D-709, D-864): after 10 ticks the line shows 5, 10, or 20 characters.
        Play play = Play.AtTheStranger();
        play.Speed = charactersPerSecond;
        int before = play.Characters ?? throw new InvalidOperationException("The line typed out at once (T-2).");
        long start = play.Run.Tick;
        while (play.Run.Tick < start + 10)
        {
            play.Frame();
        }

        Assert.Equal(before + typed, play.Characters);
    }

    [Fact]
    public void ConfirmShowsTheWholeLineThenEndsItAndTheChoiceStillWaits()
    {
        // Exit test 5 of PR-36 (D-864, D-1174): no input skips a story scene, and a burst of presses
        // ends one line alone, so the player never loses a choice.
        Play play = Play.AtTheStranger();
        play.Press();
        Assert.Null(play.Characters);
        Assert.Equal("line.fixture_hub_barmaid_door", play.Line?.Value);

        play.Until(() => play.Line?.Value == "line.fixture_hub_stranger_ask", play.PressWhenTyped);
        play.Press();
        for (int press = 0; press < 5; press += 1)
        {
            play.Press();
        }

        for (int frame = 0; frame < 120; frame += 1)
        {
            play.Frame();
        }

        Assert.Equal(ScenePhase.Pick, play.Run.State.Story.Phase);
        Assert.Equal(2, play.Options!.Count);
    }

    [Fact]
    public void TheMenuActionPausesTheStorySceneAndTheBackActionEndsThePause()
    {
        // Exit test 7 of PR-36 (D-1009, D-1010): the pause holds the type-out, and it takes no
        // confirm until it ends.
        Play play = Play.AtTheStranger();
        play.Queue(play.PauseOf(menu: true));
        play.Frame();
        int? shown = play.Characters;

        for (int frame = 0; frame < 30; frame += 1)
        {
            play.Frame();
        }

        Assert.True(play.Paused);
        Assert.Equal(shown, play.Characters);
        Assert.Null(play.Confirm());
        play.Queue(play.PauseOf(menu: false));
        play.Frame();
        Assert.False(play.Paused);
    }

    [Fact]
    public void TheRatsSceneStartsItsFightAndGoesOnAfterTheWin()
    {
        // Exit test 8 of PR-36 (D-998, D-999): the start battle step starts the fight, and the
        // story scene goes on to the line of the lead after the win.
        Play play = Play.AtHub();
        play.Walk(IntentIds.MoveNorth, 1);
        play.Walk(IntentIds.MoveEast, 6);
        play.Walk(IntentIds.MoveSouth, 5);
        play.Walk(IntentIds.MoveEast, 8);
        play.Until(() => play.Line is not null, () => { });
        Assert.Equal("line.fixture_hub_rats_straw", play.Line?.Value);
        Assert.Null(play.Speaker);

        play.Until(() => play.InBattle, play.PressWhenTyped);
        play.Until(() => !play.InBattle, play.AttackAtTheGate);
        play.Until(() => play.Line is not null, () => { });

        Assert.Equal("line.fixture_hub_rats_after", play.Line?.Value);
        Assert.True(play.Speaker?.IsLead);
        play.Until(() => !play.Run.State.Story.Running, play.PressWhenTyped);
        Assert.True(play.Run.State.Story.Flags.IsOn(Id("flag.fixture_hub_rats")));
    }

    [Fact]
    public void EachSpeakerAndActorOfEachStorySceneHasItsArtAndItsName()
    {
        // G-7, D-519: the dialogue box finds the portrait and the name plate of each speaker, and the
        // map finds the sprite of each shown actor, or the story scene would fail on screen.
        ContentSet content = Content.Value;
        foreach (StoryScene scene in content.Story.Scenes)
        {
            foreach (SceneStep step in scene.Steps)
            {
                if (step is SayStep { Speaker: SceneActor speaker })
                {
                    ContentId art = (ContentId)GameValue.Static("ScenePlay", "ArtIdOf", speaker)!;
                    _ = content.Atlas.Entry(art, "portrait");
                    _ = content.Strings.Text((ContentId)GameValue.Static("ScenePlay", "NameIdOf", speaker)!);
                }

                if (step is ShowStep show)
                {
                    _ = content.Atlas.Entry((ContentId)GameValue.Static("ScenePlay", "ArtIdOfActor", show.Actor)!, "map_front");
                }
            }
        }
    }

    private static ContentId Id(string value) => ContentId.Parse(value, "test", "id");

    /// <summary>A run of Game and the follower of its story scenes, through the built assembly.</summary>
    private sealed class Play
    {
        private readonly Type runType;
        private readonly object run;
        private readonly GameValue play;

        private Play(Type runType, object run, ContentSet content)
        {
            this.runType = runType;
            this.run = run;
            this.play = GameValue.New("ScenePlay", content.Strings, 640, 360);
        }

        /// <summary>The follower of the story scene, for a class of Game that reads it.</summary>
        public object Follower => this.play.Value;

        public RunView Run => new(this.runType, this.run);

        public ContentId? Line => this.play.Read<ContentId?>("Line");

        public SceneActor? Speaker => this.play.Read<SceneActor?>("Speaker");

        public IReadOnlyList<ChooseOption>? Options => this.play.Read<IReadOnlyList<ChooseOption>?>("Options");

        public int? Characters => this.play.Read<int?>("Characters");

        public bool Paused => this.play.Read<bool>("Paused");

        public object? View => this.play.Read<object?>("View");

        public bool InBattle => (bool)this.runType.GetProperty("InBattle")!.GetValue(this.run)!;

        public int Speed
        {
            set => this.play.Value.GetType().GetProperty("CharactersPerSecond")!.SetValue(this.play.Value, value);
        }

        /// <summary>Starts a run, and enters the fixture hub with the debug command `goto` (D-1133).</summary>
        public static Play AtHub() => AtHub(Content.Value);

        /// <summary>Starts a run on one content set, and enters the fixture hub with the debug command `goto` (D-1133).</summary>
        public static Play AtHub(ContentSet content)
        {
            Type type = GameAssemblyFile.Type("TheThingBelow.Game.GameRun");
            MethodInfo start = type.GetMethod("Start", [typeof(ContentSet), typeof(ulong), typeof(DebugIntentHandlers), typeof(MessageSpeed)])
                ?? throw new InvalidOperationException("The run holds no 'Start' method (T-2).");
            var made = new Play(type, start.Invoke(null, [content, Seed, DebugAssemblyFile.Handlers(), MessageSpeed.Normal])!, content);
            _ = DebugAssemblyFile.Run("goto map.fixture_hub", () => made.Run.State, made.Queue);
            made.Frame();
            Assert.Equal("map.fixture_hub", made.Run.State.Party.Map.Id.Value);
            return made;
        }

        /// <summary>Walks from the spawn point at (4, 6) to the trigger at (4, 7), and waits for the first line.</summary>
        public static Play AtTheStranger() => AtTheStranger(Content.Value);

        /// <summary>Walks from the spawn point at (4, 6) to the trigger at (4, 7) on one content set, and waits for the first line.</summary>
        public static Play AtTheStranger(ContentSet content)
        {
            Play made = AtHub(content);
            made.Walk(IntentIds.MoveSouth, 1);
            made.Until(() => made.Line is not null, () => { });
            return made;
        }

        public void Frame(Func<Intent?>? held = null)
        {
            _ = this.runType.GetMethod("Advance", [typeof(double), typeof(Func<Intent>)])!.Invoke(this.run, [OneTick, held]);
            _ = this.play.Call("Follow", this.run);
        }

        public void Queue(Intent? intent)
        {
            Assert.NotNull(intent);
            _ = this.runType.GetMethod("Queue", [typeof(Intent)])!.Invoke(this.run, [intent]);
        }

        public Intent? Confirm() => (Intent?)this.play.Call("Confirm", this.run);

        public Intent? PauseOf(bool menu) => (Intent?)this.play.Call("PauseOf", this.run, menu);

        public void MoveCursor(int by) => this.play.Call("MoveCursor", by);

        /// <summary>Presses confirm, and queues the intent that the press made.</summary>
        public void Press()
        {
            if (this.Confirm() is Intent made)
            {
                this.Queue(made);
            }
        }

        /// <summary>Presses confirm on a whole line of the box, as a player who reads each line does.</summary>
        public void PressWhenTyped()
        {
            if (this.Line is not null && this.Options is null && this.Characters is null)
            {
                this.Press();
            }
        }

        public void AttackAtTheGate()
        {
            if ((bool)this.runType.GetProperty("TakesBattleCommand")!.GetValue(this.run)!)
            {
                Battle battle = this.Run.State.Battle!;
                this.Queue(Intent.OfPlayer(IntentIds.BattleAttack, battle.MeleeTargets(BattleSide.Enemy)[0].Target, null));
            }
        }

        /// <summary>Runs frames, with one action before each, until the condition holds.</summary>
        public void Until(Func<bool> done, Action each)
        {
            for (int frame = 0; frame < FrameLimit && !done(); frame += 1)
            {
                each();
                this.Frame();
            }

            Assert.True(done(), $"The condition held in no frame of {FrameLimit}, at the tick {this.Run.Tick} (T-2).");
        }

        /// <summary>Walks the lead the count of tiles in one direction, and waits while an NPC holds the way.</summary>
        public void Walk(ContentId action, int tiles)
        {
            Intent step = Intent.OfPlayer(action);
            for (int tile = 0; tile < tiles; tile += 1)
            {
                TilePoint from = this.Run.State.Party.LeadAt;
                int frame = 0;
                while (this.Run.State.Party.LeadAt == from || this.Run.State.Party.Stepping is not null)
                {
                    Assert.True(frame < FrameLimit, $"The lead stood at {from} for {FrameLimit} frames on '{action.Value}' (T-2).");
                    bool standing = this.Run.State.Party.Stepping is null && this.Run.State.Party.LeadAt == from;
                    this.Frame(standing ? () => step : null);
                    frame += 1;
                }
            }
        }

        public bool Walks(ContentId actor) => this.TryWalk(actor, out _);

        public int WalkX(ContentId actor) =>
            this.TryWalk(actor, out int x) ? x : throw new InvalidOperationException($"No move step walks '{actor.Value}' (T-2).");

        /// <summary>Adds the pixel of the actor to the set when a move step walks it.</summary>
        public void Walked(ContentId actor, SortedSet<int> places)
        {
            if (this.TryWalk(actor, out int x))
            {
                places.Add(x);
            }
        }

        private bool TryWalk(ContentId actor, out int x)
        {
            x = 0;
            ActorValues? shown = this.Run.State.Story.Actors.FirstOrDefault(values => values.Actor.Value == actor.Value);
            if (shown is null)
            {
                return false;
            }

            object?[] arguments = [actor, shown.At, 0, 0];
            bool walks = (bool)this.play.Value.GetType().GetMethod("TryWalk")!.Invoke(this.play.Value, arguments)!;
            x = (int)arguments[2]!;
            return walks;
        }
    }

    /// <summary>The values of the run that the tests read.</summary>
    private sealed class RunView(Type type, object run)
    {
        public RunState State => (RunState)type.GetProperty("State")!.GetValue(run)!;

        public long Tick => (long)type.GetProperty("Tick")!.GetValue(run)!;

        public RunRecord Record() => (RunRecord)type.GetMethod("Record", Type.EmptyTypes)!.Invoke(run, [])!;
    }
}
