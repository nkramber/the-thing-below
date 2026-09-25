using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Hashing;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Notices;
using TheThingBelow.Core.Story;
using TheThingBelow.Core.Streams;

namespace TheThingBelow.Core.Runs;

/// <summary>
/// The live state of one run: the tick, the menu, the world values, and every random stream
/// (D-168). <see cref="Simulation"/> steps it, and <see cref="RunSnapshot"/> stores it.
/// </summary>
/// <remarks>
/// The tick is the one time line of a run, and it rises on every step, a step with a menu
/// open included (D-650). The world values rise only while no menu is open, because a menu
/// pauses the world (D-162).
/// <para>
/// The world of this run is the party on a tile map (D-100, D-106). The party walks one tile
/// at a time, and the record of the walked tiles grows with it (D-567, D-716).
/// </para>
/// </remarks>
public sealed class RunState
{
    // The streams sit in the order of `RandomStreams.All`, so a walk of them is the same on
    // every machine and a snapshot writes them in one order (G-4, T-7).
    private readonly RandomStream[] streams;

    // The events of the battle wait here until Game takes them. They are output, and not
    // state, so no snapshot and no hash reads them (D-168, D-532).
    private readonly List<BattleEvent> events = [];

    // The notices that a rule posted wait here until Game takes them. They are output, and not
    // state, so no snapshot and no hash reads them (D-168, D-221).
    private readonly List<NoticeRecord> posted = [];

    private RunState(
        ulong seed,
        RandomStream[] streams,
        long tick,
        bool menuOpen,
        long worldTick,
        MapState map,
        BattleContent battleContent,
        PartyState characters,
        Battle? battle,
        NoticeList notices,
        NoticeLog noticeLog,
        StoryState story)
    {
        this.Story = story;
        this.BattleContent = battleContent;
        this.Notices = notices;
        this.NoticeLog = noticeLog;
        this.Characters = characters;
        this.Battle = battle;
        this.Seed = seed;
        this.streams = streams;
        this.Tick = tick;
        this.MenuOpen = menuOpen;
        this.WorldTick = worldTick;
        this.Party = map;
    }

    /// <summary>The seed that started the run (G-3).</summary>
    public ulong Seed { get; }

    /// <summary>The count of ticks since the start of the run (D-164, D-650).</summary>
    public long Tick { get; private set; }

    /// <summary>True while a menu is open and the world waits (D-162).</summary>
    public bool MenuOpen { get; private set; }

    /// <summary>The count of ticks in which the world ran (D-650).</summary>
    public long WorldTick { get; private set; }

    /// <summary>The party on its map: the lead, the step that runs, and the walked tiles (D-106, D-567).</summary>
    public MapState Party { get; }

    /// <summary>The battle rules and the fixture of this run (D-757, D-766).</summary>
    public BattleContent BattleContent { get; }

    /// <summary>The characters of the party and their pack, which last between battles (D-36, D-765).</summary>
    public PartyState Characters { get; }

    /// <summary>The battle that runs, or that ended and waits for the screen, or no value (D-522, D-531).</summary>
    public Battle? Battle { get; private set; }

    /// <summary>The notice file of this build, which the rule of a notice reads (D-983, D-989).</summary>
    public NoticeList Notices { get; }

    /// <summary>The newest notices that content marks to log, oldest first (D-221, D-984).</summary>
    public NoticeLog NoticeLog { get; }

    /// <summary>The flags, the story scene that runs, and the events that fire a trigger (D-540, D-542).</summary>
    public StoryState Story { get; }

    /// <summary>Starts a new run from a seed, at tick zero, on one map.</summary>
    /// <param name="seed">The seed of the run (G-3, G-4).</param>
    /// <param name="map">The map that the run opens, with the party on its spawn point (D-528).</param>
    /// <param name="battleContent">The battle rules and the fixture, which hold every group that the map names (D-766).</param>
    /// <param name="notices">The notice file of this build (D-989).</param>
    /// <param name="story">The story content of this build, which holds every story scene that a trigger of the map starts (D-1004).</param>
    /// <returns>The state, with every stream at its first value, an empty notice log, no flag on, and the entry of the map to read.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ContentException">A group, a trigger, or a service of the map names content that this build lacks (T-2).</exception>
    public static RunState Start(ulong seed, GameMap map, BattleContent battleContent, NoticeList notices, StoryContent story)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(battleContent);
        ArgumentNullException.ThrowIfNull(notices);
        ArgumentNullException.ThrowIfNull(story);
        battleContent.RequireGroupsOf(map);
        story.RequireScenesOf(map);
        story.RequireServicesOf(map);

        RandomStream[] streams = new RandomStream[RandomStreams.All.Count];
        for (int index = 0; index < streams.Length; index += 1)
        {
            streams[index] = RandomStreams.Open(seed, RandomStreams.All[index]);
        }

        return new RunState(
            seed,
            streams,
            0,
            false,
            0,
            MapState.Enter(map),
            battleContent,
            PartyState.Start(battleContent),
            null,
            notices,
            NoticeLog.Empty(),
            StoryState.Start(story));
    }

    /// <summary>Starts a run again from a snapshot (D-259, D-651).</summary>
    /// <param name="seed">The seed of the run, which the record header holds (G-5).</param>
    /// <param name="snapshot">The snapshot, which the caller checked (T-2).</param>
    /// <param name="map">
    /// The map of the snapshot, which the caller read from its content by
    /// <see cref="RunSnapshot.MapIdOrFirst"/> (D-166).
    /// </param>
    /// <param name="battleContent">The battle rules and the fixture, which hold every group that the map names (D-766).</param>
    /// <param name="notices">The notice file of this build, which each entry of the stored log must name (D-985).</param>
    /// <param name="story">The story content of this build, which each stored flag and story scene must name (D-166).</param>
    /// <returns>The state, with every stream at the position of the snapshot.</returns>
    /// <exception cref="ArgumentNullException">The snapshot or the map is null (T-2).</exception>
    /// <exception cref="ArgumentException">The snapshot is not a state of a run, or the map is another map (T-2).</exception>
    /// <remarks>
    /// A snapshot of save format 1 holds no map, because it predates the tile map. Its
    /// migration puts the party on the spawn point of the first map, with that tile walked
    /// and no other (D-166, D-654). A snapshot of save format 2 holds no enemy, and its
    /// migration puts each enemy of the map on the start tile of its station (D-750). A
    /// snapshot before save format 8 holds no notice log, and its migration starts the log
    /// empty (D-985). A snapshot before save format 9 holds no story state, and its migration
    /// starts with no flag on, no story scene, and no entry to read, because a load is not an
    /// entry to the map (D-1004).
    /// </remarks>
    public static RunState Resume(ulong seed, RunSnapshot snapshot, GameMap map, BattleContent battleContent, NoticeList notices, StoryContent story)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        return Resume(seed, snapshot, map, battleContent, notices, story, ResumeDrift.Of(SnapshotOrigin.ThisBuild, snapshot.Tick));
    }

    /// <summary>
    /// Builds the state of a run from a snapshot that this build or another build wrote
    /// (D-166, D-259, D-1111).
    /// </summary>
    /// <param name="seed">The seed of the run.</param>
    /// <param name="snapshot">The snapshot.</param>
    /// <param name="map">The map of the snapshot, from the content of this build.</param>
    /// <param name="battleContent">The battle rules and the fixture of this build (D-766).</param>
    /// <param name="notices">The notice file of this build (D-985).</param>
    /// <param name="story">The story content of this build (D-166).</param>
    /// <param name="drift">
    /// The build of the snapshot. A snapshot of another build can follow an edit of a map or a
    /// story scene, and the drift logs each change (D-1111, D-1112). The party takes no change
    /// (D-1110).
    /// </param>
    /// <returns>The state, with every stream at the position of the snapshot.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ArgumentException">The snapshot is not a state of a run, or the map is another map (T-2).</exception>
    public static RunState Resume(ulong seed, RunSnapshot snapshot, GameMap map, BattleContent battleContent, NoticeList notices, StoryContent story, ResumeDrift drift)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(battleContent);
        ArgumentNullException.ThrowIfNull(notices);
        ArgumentNullException.ThrowIfNull(story);
        ArgumentNullException.ThrowIfNull(drift);
        battleContent.RequireGroupsOf(map);
        story.RequireScenesOf(map);
        story.RequireServicesOf(map);
        snapshot.Check("this run");

        RandomStream[] streams = new RandomStream[snapshot.Streams.Count];
        for (int index = 0; index < streams.Length; index += 1)
        {
            StreamPosition position = snapshot.Streams[index];
            streams[index] = new RandomStream(
                position.Stream,
                Pcg32.FromSnapshot(position.State, position.Increment));
        }

        MapState party = ResumeMap(snapshot, map, drift);
        PartyState characters = ResumeCharacters(snapshot, battleContent);
        StoryState storyState = ResumeStory(snapshot, story, map, drift);

        // Core refuses every intent but the few of a story scene while one runs, the close of the
        // menu included, so the pair would hold the run for good (D-1009, P3-18).
        if (snapshot.MenuOpen && storyState.Running)
        {
            throw new ArgumentException(
                "The snapshot of this run holds an open menu and a running story scene, and no rule makes both (D-1009, T-2).",
                nameof(snapshot));
        }
        return new RunState(
            seed,
            streams,
            snapshot.Tick,
            snapshot.MenuOpen,
            snapshot.WorldTick,
            party,
            battleContent,
            characters,
            ResumeBattle(snapshot, party, characters, battleContent, storyState),
            notices,
            snapshot.Notices is null ? NoticeLog.Empty() : NoticeLog.Resume(snapshot.Notices, notices, "this run"),
            storyState);
    }

    private static MapState ResumeMap(RunSnapshot snapshot, GameMap map, ResumeDrift drift)
    {
        if (string.CompareOrdinal(snapshot.MapIdOrFirst.Value, map.Id.Value) != 0)
        {
            throw new ArgumentException(
                $"The snapshot names the map '{snapshot.MapIdOrFirst.Value}', and the caller gave the map '{map.Id.Value}' (T-2, D-166).",
                nameof(map));
        }

        if (snapshot.Map is null)
        {
            return MapState.Enter(map);
        }

        MapSnapshot party = snapshot.Map;

        // A snapshot of save format 2 predates the enemies, so its enemy list is absent and
        // each enemy of the map starts on the start tile of its station (D-654, D-750). A
        // snapshot before save format 15 predates the NPCs, and each NPC starts on its start
        // tile (D-1137).
        return MapState.Resume(
            map,
            new LeadValues(new TilePoint(party.LeadX, party.LeadY), party.Facing, party.Stepping, party.StepTicks),
            WalkedTiles.OfRows(party.Walked, "this run"),
            party.Enemies,
            party.Mark,
            party.Encounter,
            party.Npcs,
            "this run",
            drift);
    }

    /// <summary>
    /// Gives the party of a snapshot. A snapshot before save format 4 holds no party, and its
    /// migration starts the party of the fixture at full health (D-166, D-765).
    /// </summary>
    private static PartyState ResumeCharacters(RunSnapshot snapshot, BattleContent battleContent)
    {
        if (snapshot.Characters is not PartySnapshot stored)
        {
            return PartyState.Start(battleContent);
        }

        return PartyState.Resume(battleContent, stored.Characters, stored.Reserve, stored.Pack, stored.LessonPack, stored.Gold, stored.TorchHeld, "this run");
    }

    /// <summary>
    /// Gives the story state of a snapshot. A snapshot before save format 9 holds none, and its
    /// migration starts with no flag on, no story scene, and no entry to read (D-166, D-1004).
    /// </summary>
    private static StoryState ResumeStory(RunSnapshot snapshot, StoryContent story, GameMap map, ResumeDrift drift)
    {
        StoryValues stored = snapshot.Story ?? new StoryValues([], null, false, false, null);
        return StoryState.Resume(story, stored, map, "this run", drift);
    }

    /// <summary>
    /// Gives the battle of a snapshot. A battle of a story scene must match the start battle
    /// step of the story scene that runs, and any other battle must match the encounter of its
    /// map (D-531, D-998, T-2).
    /// </summary>
    private static Battle? ResumeBattle(RunSnapshot snapshot, MapState party, PartyState characters, BattleContent battleContent, StoryState story)
    {
        bool storyWaits = story.Phase == ScenePhase.Battle;
        if (snapshot.Battle is not BattleValues stored)
        {
            if (storyWaits)
            {
                throw new ArgumentException("The snapshot holds a story scene that waits for its battle, and it holds no battle (T-2, D-999).", nameof(snapshot));
            }

            return null;
        }

        if (string.CompareOrdinal(stored.Enemy.Kind, StoryScene.Kind) == 0)
        {
            if (!storyWaits
                || story.Scene is not StoryScene scene
                || string.CompareOrdinal(scene.Id.Value, stored.Enemy.Value) != 0
                || scene.Steps[story.Step] is not StartBattleStep start
                || string.CompareOrdinal(start.Group.Value, stored.Group.Value) != 0)
            {
                throw new ArgumentException(
                    $"The snapshot holds a battle of the story scene '{stored.Enemy.Value}' and the group '{stored.Group.Value}', and no start battle step of that story scene waits for it (T-2, D-998).",
                    nameof(snapshot));
            }

            return Battle.Resume(battleContent, stored, characters, "this run");
        }

        if (storyWaits)
        {
            throw new ArgumentException($"The snapshot holds a story scene that waits for its battle, and the battle is one of the map enemy '{stored.Enemy.Value}' (T-2, D-999).", nameof(snapshot));
        }

        if (party.Patrols.Encounter is not MapEncounter encounter
            || string.CompareOrdinal(encounter.Enemy.Value, stored.Enemy.Value) != 0
            || string.CompareOrdinal(encounter.Group.Value, stored.Group.Value) != 0)
        {
            throw new ArgumentException(
                $"The snapshot holds a battle of the enemy '{stored.Enemy.Value}' and the group '{stored.Group.Value}', and its map holds no encounter of both (T-2, D-531).",
                nameof(snapshot));
        }

        return Battle.Resume(battleContent, stored, characters, "this run");
    }

    /// <summary>Gives the stream of one subsystem (G-4).</summary>
    /// <param name="stream">The number of the stream.</param>
    /// <returns>The stream of that subsystem.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The run holds no such stream (T-2).</exception>
    public RandomStream Stream(StreamId stream)
    {
        foreach (RandomStream open in this.streams)
        {
            if (open.Stream == stream)
            {
                return open;
            }
        }

        throw new ArgumentOutOfRangeException(
            nameof(stream),
            stream,
            $"The run holds no stream with that number. It holds {this.streams.Length} streams.");
    }

    /// <summary>Makes the context that an error of this tick carries (T-2, G-18).</summary>
    /// <param name="subject">The ids of the thing that the rule works on.</param>
    /// <returns>The seed, the tick, and the subject.</returns>
    public RunContext Context(string subject) => new(this.Seed, this.Tick, subject);

    /// <summary>Stores the whole state, so a replay can start again from here (F-10, D-651).</summary>
    /// <returns>The snapshot.</returns>
    public RunSnapshot Snapshot() =>
        new(
            this.Tick,
            this.MenuOpen,
            this.WorldTick,
            new MapSnapshot(
                this.Party.Map.Id,
                this.Party.LeadAt.X,
                this.Party.LeadAt.Y,
                this.Party.Facing,
                this.Party.Stepping,
                this.Party.StepTicks,
                this.Party.Walked.Rows(),
                this.Party.Patrols.Values(),
                this.Party.Patrols.Mark,
                this.Party.Patrols.Encounter,
                this.Party.Npcs.Values()),
            new PartySnapshot(this.Characters.CharacterValues(), this.Characters.PackValues(), this.Characters.LessonPackValues(), this.Characters.Gold, this.Characters.TorchHeld, this.Characters.ReserveValues()),
            this.Battle?.Values(),
            this.NoticeLog.Values(),
            this.Story.Values(),
            ReadPositions(this.streams));

    /// <summary>Computes the state hash that a replay and the identity job compare (G-5).</summary>
    /// <returns>The hash of every value of this state.</returns>
    /// <remarks>
    /// The hash holds the simulation version, so a build with other rules never gives the
    /// hash of this build by accident (G-17, D-504).
    /// </remarks>
    public ulong StateHash()
    {
        StateHasher hasher = new();

        hasher.AddInt32(SimulationVersion.Current);
        hasher.AddUInt64(this.Seed);
        hasher.AddInt64(this.Tick);
        hasher.AddBoolean(this.MenuOpen);
        hasher.AddInt64(this.WorldTick);
        this.Party.Hash(hasher);
        this.Characters.Hash(hasher);
        hasher.AddBoolean(this.Battle is not null);
        this.Battle?.Hash(hasher);
        this.NoticeLog.Hash(hasher);
        this.Story.Hash(hasher);

        foreach (RandomStream stream in this.streams)
        {
            hasher.AddInt32((int)stream.Stream);
            hasher.AddUInt64(stream.Generator.State);
            hasher.AddUInt64(stream.Generator.Increment);
        }

        return hasher.Finish();
    }

    /// <summary>Opens or closes the menu, which starts or ends the pause of the world (D-162).</summary>
    /// <param name="open">True to open the menu, and false to close it.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <exception cref="SimulationException">The menu already has that state (T-2).</exception>
    /// <remarks>
    /// A second open of an open menu is an error and never a value that the rule drops. An
    /// intent that changes nothing points at a fault in the screen that made it (T-2).
    /// </remarks>
    public void SetMenuOpen(bool open, RunContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (this.MenuOpen == open)
        {
            string state = open ? "open" : "closed";
            throw new SimulationException($"an intent that makes the menu {state}, which it already is", context);
        }

        this.MenuOpen = open;
    }

    /// <summary>Reads a move intent of this tick, which the world step then applies (D-493).</summary>
    /// <param name="direction">The direction of the step that the player asked for.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <exception cref="ArgumentNullException">The context is null (T-2).</exception>
    /// <exception cref="SimulationException">A menu is open, and a menu pauses the world (T-2).</exception>
    /// <remarks>
    /// A move intent while a menu is open is an error and never a value that the rule drops.
    /// The menu screen takes every input of the player, so a step intent from it points at a
    /// fault in the screen that made it (D-162, T-2).
    /// </remarks>
    public void WantStep(StepDirection direction, RunContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (this.MenuOpen)
        {
            throw new SimulationException(
                $"a step to the {StepDirections.NameOf(direction)} while the menu is open, and a menu pauses the world (D-162)",
                context);
        }

        this.Party.Want(direction);
    }

    /// <summary>Takes every battle event since the last take, in the order of the rules (D-532).</summary>
    /// <returns>The events, which the run no longer holds.</returns>
    public IReadOnlyList<BattleEvent> TakeEvents()
    {
        BattleEvent[] taken = [.. this.events];
        this.events.Clear();
        return taken;
    }

    /// <summary>Takes every notice that a rule posted since the last take, in the order of the posts (D-221).</summary>
    /// <returns>The notices, which the run no longer holds.</returns>
    public IReadOnlyList<NoticeRecord> TakeNotices()
    {
        NoticeRecord[] taken = [.. this.posted];
        this.posted.Clear();
        return taken;
    }

    /// <summary>Moves one character of the party to the other row, from the party window of a menu (D-377, D-558).</summary>
    /// <param name="target">The side and the slot of the character, which must be the party side.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <exception cref="ArgumentNullException">The context is null (T-2).</exception>
    /// <exception cref="SimulationException">
    /// No menu is open, a battle holds the run, the target is an enemy, or the party holds no
    /// character in the slot (T-2).
    /// </exception>
    /// <remarks>
    /// The next battle starts each character in its row (D-558). The row step of a fight stays
    /// the control inside a fight, so the party window changes a row outside one alone (D-380).
    /// </remarks>
    public void SwapRow(BattleTarget target, RunContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!this.MenuOpen)
        {
            throw new SimulationException("a row change while no menu is open, and the party window of a menu makes it (D-558)", context);
        }

        if (this.Battle is not null)
        {
            throw new SimulationException("a row change while a battle holds the run, and the row step of a fight makes it there (D-380)", context);
        }

        if (target.Side != BattleSide.Party || target.Slot < 0 || target.Slot >= this.Characters.Members.Count)
        {
            throw new SimulationException(
                $"a row change of {target.Describe()}, and the party holds {this.Characters.Members.Count} characters (D-558)",
                context);
        }

        PartyMember member = this.Characters.Members[target.Slot];
        member.Row = BattleSides.Other(member.Row);
    }

    /// <summary>
    /// Swaps one character of the party with one character of the reserve, from the Party window
    /// of a menu, anywhere outside a fight (D-1134, D-1136). The map stays as it is: the lead
    /// walks the map in the party or in the reserve (D-292, D-306).
    /// </summary>
    /// <param name="slot">The party slot of the character who goes to the reserve.</param>
    /// <param name="reserve">The reserve index of the character who comes in.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <exception cref="ArgumentNullException">The context is null (T-2).</exception>
    /// <exception cref="SimulationException">
    /// No menu is open, a battle holds the run, an encounter leads into a battle, or the party
    /// rules refuse the swap, such as a downed reserve character (D-1134, D-1135, T-2).
    /// </exception>
    public void SwapReserve(int slot, int reserve, RunContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!this.MenuOpen)
        {
            throw new SimulationException("a party swap while no menu is open, and the Party window of a menu makes it (D-1134)", context);
        }

        if (this.Battle is not null)
        {
            throw new SimulationException("a party swap while a battle holds the run, and no swap happens inside a battle (D-1134)", context);
        }

        if (this.Party.Patrols.Encounter is not null)
        {
            throw new SimulationException("a party swap while an encounter leads into a battle, and no swap happens inside a battle (D-1134)", context);
        }

        this.Characters.SwapReserve(slot, reserve, context);
    }

    /// <summary>Holds one posted notice for Game, and adds it to the log when content marks it (D-221, D-983).</summary>
    internal void AddNotice(NoticeRecord notice)
    {
        this.posted.Add(notice);
        if (notice.Logs)
        {
            this.NoticeLog.Add(notice.Id);
        }
    }

    /// <summary>Sets the battle, or ends it with null (D-531).</summary>
    internal void SetBattle(Battle? battle) => this.Battle = battle;

    /// <summary>Adds one battle event for Game (D-168, D-532).</summary>
    internal void AddEvent(BattleEvent battleEvent) => this.events.Add(battleEvent);

    /// <summary>
    /// Counts one step of the loop (D-164, D-650). The tick is the one time line of the run,
    /// so <see cref="Simulation"/> alone counts it and no debug command moves it (D-171).
    /// </summary>
    /// <exception cref="SimulationException">The tick passes the range of a `long` (T-2).</exception>
    internal void CountTick()
    {
        if (this.Tick == long.MaxValue)
        {
            throw new SimulationException(
                $"a step after tick {long.MaxValue}, which passes the range of the tick",
                this.Context("run"));
        }

        this.Tick += 1;
    }

    /// <summary>Counts one tick in which the world ran, which a menu skips (D-650).</summary>
    internal void CountWorldTick() => this.WorldTick += 1;

    private static StreamPosition[] ReadPositions(RandomStream[] streams)
    {
        StreamPosition[] positions = new StreamPosition[streams.Length];
        for (int index = 0; index < streams.Length; index += 1)
        {
            RandomStream stream = streams[index];
            positions[index] = new StreamPosition(
                stream.Stream,
                stream.Generator.State,
                stream.Generator.Increment);
        }

        return positions;
    }
}
