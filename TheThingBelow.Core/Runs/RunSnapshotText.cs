using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Notices;
using TheThingBelow.Core.Saves;
using TheThingBelow.Core.Shops;
using TheThingBelow.Core.Story;
using TheThingBelow.Core.Streams;

namespace TheThingBelow.Core.Runs;

/// <summary>
/// The text of one snapshot: one JSON object on one line (D-652). A run record holds it on
/// line 2, and a save file holds it on line 2, so one writer and one reader serve both
/// (D-494, D-655, T-1).
/// </summary>
/// <remarks>
/// Core makes the bytes of a snapshot and loads a state from them, and Storage writes those
/// bytes to a file (D-494). Thus no file code reads a field of a snapshot.
/// </remarks>
public static class RunSnapshotText
{
    /// <summary>Writes a snapshot as one JSON object with no line ending.</summary>
    /// <param name="snapshot">The snapshot.</param>
    /// <returns>The line.</returns>
    /// <exception cref="ArgumentNullException">The snapshot is null (T-2).</exception>
    public static string Write(RunSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        ArrayBufferWriter<byte> bytes = new();

        // The writer holds no indent, so one object takes one line, and it escapes every
        // text value itself (D-652).
        using (Utf8JsonWriter writer = new(bytes, new JsonWriterOptions { Indented = false, SkipValidation = false }))
        {
            writer.WriteStartObject();
            writer.WriteNumber("tick", snapshot.Tick);
            writer.WriteBoolean("menu", snapshot.MenuOpen);
            writer.WriteNumber("world", snapshot.WorldTick);
            WriteMap(writer, snapshot.Map);
            WriteParty(writer, snapshot.Characters);
            WriteBattle(writer, snapshot.Battle);
            WriteNotices(writer, snapshot.Notices);
            WriteStory(writer, snapshot.Story);
            WriteStock(writer, snapshot.Stock);
            WritePlaces(writer, snapshot.Places);
            writer.WriteStartArray("streams");
            foreach (StreamPosition position in snapshot.Streams)
            {
                writer.WriteStartObject();
                writer.WriteNumber("stream", (int)position.Stream);
                writer.WriteString("state", HexText.Of(position.State));
                writer.WriteString("increment", HexText.Of(position.Increment));
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(bytes.WrittenSpan);
    }

    /// <summary>
    /// Writes the notice log, oldest first (D-985). This build writes save format 8, so the field
    /// is always present, and it holds an empty array before the first notice that logs.
    /// </summary>
    private static void WriteNotices(Utf8JsonWriter writer, IReadOnlyList<ContentId>? notices)
    {
        if (notices is null)
        {
            throw new ArgumentException(
                "A snapshot that this build writes holds a notice log. A snapshot with none comes from save format 7 or older, and this build never writes one (T-2, D-166).",
                nameof(notices));
        }

        writer.WriteStartArray("notices");
        foreach (ContentId notice in notices)
        {
            writer.WriteStringValue(notice.Value);
        }

        writer.WriteEndArray();
    }

    /// <summary>
    /// Writes the story state (D-540). This build writes save format 9, so the field is always
    /// present.
    /// </summary>
    private static void WriteStory(Utf8JsonWriter writer, StoryValues? story)
    {
        if (story is null)
        {
            throw new ArgumentException(
                "A snapshot that this build writes holds a story state. A snapshot with none comes from save format 8 or older, and this build never writes one (T-2, D-166).",
                nameof(story));
        }

        StorySnapshotText.Write(writer, story);
    }

    /// <summary>
    /// Writes the party and the enemies on the map. A snapshot of save format 1 holds no
    /// map, and this build writes save format 5, so the field is always present in a file
    /// that this build writes (D-166, D-654, D-750).
    /// </summary>
    private static void WriteMap(Utf8JsonWriter writer, MapSnapshot? map)
    {
        if (map is null)
        {
            throw new ArgumentException(
                "A snapshot that this build writes holds a map. A snapshot with no map comes from save format 1, and this build never writes one (T-2, D-166).",
                nameof(map));
        }

        writer.WriteStartObject("map");
        writer.WriteString("id", map.Map.Value);
        writer.WriteNumber("x", map.LeadX);
        writer.WriteNumber("y", map.LeadY);
        writer.WriteString("facing", StepDirections.NameOf(map.Facing));
        if (map.Stepping is StepDirection stepping)
        {
            writer.WriteString("stepping", StepDirections.NameOf(stepping));
        }

        writer.WriteNumber("step_ticks", map.StepTicks);
        writer.WriteStartArray("walked");
        foreach (string row in map.Walked)
        {
            writer.WriteStringValue(row);
        }

        writer.WriteEndArray();
        WriteEnemies(writer, map.Enemies);
        WriteMark(writer, map.Mark);
        WriteEncounter(writer, map.Encounter);
        WriteNpcs(writer, map.Npcs);
        writer.WriteEndObject();
    }

    /// <summary>
    /// Writes the stored values of each enemy of the map (D-750). This build writes save
    /// format 5, so the field is always present, and it holds an empty array on a map that
    /// places no enemy.
    /// </summary>
    private static void WriteEnemies(Utf8JsonWriter writer, IReadOnlyList<PatrolValues>? enemies)
    {
        if (enemies is null)
        {
            throw new ArgumentException(
                "A snapshot that this build writes holds an enemy list. A snapshot with none comes from save format 2, and this build never writes one (T-2, D-166).",
                nameof(enemies));
        }

        writer.WriteStartArray("enemies");
        foreach (PatrolValues enemy in enemies)
        {
            writer.WriteStartObject();
            writer.WriteString("id", enemy.Enemy.Value);
            writer.WriteNumber("x", enemy.X);
            writer.WriteNumber("y", enemy.Y);
            writer.WriteString("facing", StepDirections.NameOf(enemy.Facing));
            if (enemy.Stepping is StepDirection stepping)
            {
                writer.WriteString("stepping", StepDirections.NameOf(stepping));
            }

            writer.WriteNumber("step_ticks", enemy.StepTicks);
            writer.WriteNumber("target", enemy.Target);
            writer.WriteBoolean("forward", enemy.Forward);
            writer.WriteNumber("grace_ticks", enemy.GraceTicks);
            writer.WriteBoolean("dead", enemy.Dead);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    /// <summary>
    /// Writes the stored values of each NPC of the map (D-1137). This build writes save format 15,
    /// so the field is always present, and it holds an empty array on a map that places no NPC.
    /// </summary>
    private static void WriteNpcs(Utf8JsonWriter writer, IReadOnlyList<NpcValues>? npcs)
    {
        if (npcs is null)
        {
            throw new ArgumentException(
                "A snapshot that this build writes holds an NPC list. A snapshot with none comes from save format 14 or older, and this build never writes one (T-2, D-166).",
                nameof(npcs));
        }

        writer.WriteStartArray("npcs");
        foreach (NpcValues npc in npcs)
        {
            writer.WriteStartObject();
            writer.WriteString("id", npc.Npc.Value);
            writer.WriteNumber("x", npc.X);
            writer.WriteNumber("y", npc.Y);
            writer.WriteString("facing", StepDirections.NameOf(npc.Facing));
            if (npc.Stepping is StepDirection stepping)
            {
                writer.WriteString("stepping", StepDirections.NameOf(stepping));
            }

            writer.WriteNumber("step_ticks", npc.StepTicks);
            writer.WriteNumber("target", npc.Target);
            writer.WriteBoolean("forward", npc.Forward);
            writer.WriteNumber("wait_ticks", npc.WaitTicks);
            writer.WriteBoolean("walks_home", npc.WalksHome);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    /// <summary>
    /// Writes the party. This build writes save format 5, so the field is always present in a
    /// file that this build writes (D-166, D-765).
    /// </summary>
    private static void WriteParty(Utf8JsonWriter writer, PartySnapshot? party)
    {
        if (party is null)
        {
            throw new ArgumentException(
                "A snapshot that this build writes holds a party. A snapshot with none comes from save format 3 or older, and this build never writes one (T-2, D-166).",
                nameof(party));
        }

        BattleSnapshotText.WriteParty(writer, party);
    }

    private static void WriteBattle(Utf8JsonWriter writer, BattleValues? battle)
    {
        if (battle is not null)
        {
            BattleSnapshotText.WriteBattle(writer, battle);
        }
    }

    private static void WriteMark(Utf8JsonWriter writer, SightMark? mark)
    {
        if (mark is null)
        {
            return;
        }

        writer.WriteStartObject("mark");
        writer.WriteString("enemy", mark.Enemy.Value);
        writer.WriteNumber("ticks_left", mark.TicksLeft);
        writer.WriteEndObject();
    }

    private static void WriteEncounter(Utf8JsonWriter writer, MapEncounter? encounter)
    {
        if (encounter is null)
        {
            return;
        }

        writer.WriteStartObject("encounter");
        writer.WriteString("enemy", encounter.Enemy.Value);
        writer.WriteString("group", encounter.Group.Value);
        writer.WriteString("behind", EncounterSides.NameOf(encounter.Behind));
        writer.WriteEndObject();
    }

    /// <summary>Reads a snapshot from the object under the reader, and checks it (T-2).</summary>
    /// <param name="reader">The reader of the line, which names the record or the save file.</param>
    /// <returns>The snapshot, which describes a state of a run.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, or malformed (T-2).</exception>
    /// <exception cref="ArgumentException">The values describe no state of a run (T-2).</exception>
    /// <remarks>
    /// The check runs here, inside the read of the line, so the caller names the line and
    /// the file of every fault of a snapshot. A check after the read loses both (T-2).
    /// </remarks>
    public static RunSnapshot Read(ref ContentReader reader) => ReadLine(ref reader, SaveFormat.Current, null);

    /// <summary>
    /// Reads a snapshot of save format 2, which holds no enemy (D-654, D-750). The migration
    /// runs in `MapState.Resume`, which puts each enemy of the map on the start tile of its
    /// station.
    /// </summary>
    /// <param name="reader">The reader of the line, which names the save file.</param>
    /// <param name="seed">The seed of the header, which opens the stream of the evaluator and the NPC stream (D-947, D-1137).</param>
    /// <returns>The snapshot, with no enemy list.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, or malformed (T-2).</exception>
    /// <exception cref="ArgumentException">The values describe no state of a run (T-2).</exception>
    public static RunSnapshot ReadFormatTwo(ref ContentReader reader, ulong seed) => ReadLine(ref reader, 2, seed);

    /// <summary>
    /// Reads a snapshot of save format 3, which holds no party and no battle (D-765). The
    /// migration runs in `RunState.Resume`, which starts the party of the fixture at full health.
    /// </summary>
    /// <param name="reader">The reader of the line, which names the save file.</param>
    /// <param name="seed">The seed of the header, which opens the stream of the evaluator and the NPC stream (D-947, D-1137).</param>
    /// <returns>The snapshot, with no party and no battle.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, or malformed (T-2).</exception>
    /// <exception cref="ArgumentException">The values describe no state of a run (T-2).</exception>
    public static RunSnapshot ReadFormatThree(ref ContentReader reader, ulong seed) => ReadLine(ref reader, 3, seed);

    /// <summary>
    /// Reads a snapshot of save format 4, which holds no status and holds a push rate for each
    /// combatant (D-792). Each character and each combatant then holds no status.
    /// </summary>
    /// <param name="reader">The reader of the line, which names the save file.</param>
    /// <param name="seed">The seed of the header, which opens the stream of the evaluator and the NPC stream (D-947, D-1137).</param>
    /// <returns>The snapshot, with no status.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, or malformed, or a push rate names haste or slow (T-2).</exception>
    /// <exception cref="ArgumentException">The values describe no state of a run (T-2).</exception>
    public static RunSnapshot ReadFormatFour(ref ContentReader reader, ulong seed) => ReadLine(ref reader, 4, seed);

    /// <summary>
    /// Reads a snapshot of save format 5, which holds no stream of the evaluator (D-947). The
    /// snapshot gains that stream at its first value, because no build before PR-11 drew from it.
    /// </summary>
    /// <param name="reader">The reader of the line, which names the save file.</param>
    /// <param name="seed">The seed of the header, which opens the stream of the evaluator and the NPC stream (D-947, D-1137).</param>
    /// <returns>The snapshot, with every stream of this build.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, or malformed (T-2).</exception>
    /// <exception cref="ArgumentException">The values describe no state of a run (T-2).</exception>
    public static RunSnapshot ReadFormatFive(ref ContentReader reader, ulong seed) => ReadLine(ref reader, 5, seed);

    /// <summary>
    /// Reads a snapshot of save format 6, which holds no level, no experience, and no AP (D-966).
    /// The resume starts each character at its join level with full AP (D-363).
    /// </summary>
    /// <param name="reader">The reader of the line, which names the save file.</param>
    /// <param name="seed">The seed of the header, which opens the NPC stream (D-1137).</param>
    /// <returns>The snapshot, with no level of a character.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, or malformed (T-2).</exception>
    /// <exception cref="ArgumentException">The values describe no state of a run (T-2).</exception>
    public static RunSnapshot ReadFormatSix(ref ContentReader reader, ulong seed) => ReadLine(ref reader, 6, seed);

    /// <summary>
    /// Reads a snapshot of save format 7, which holds no notice log (D-985). The resume starts
    /// the log empty.
    /// </summary>
    /// <param name="reader">The reader of the line, which names the save file.</param>
    /// <param name="seed">The seed of the header, which opens the NPC stream (D-1137).</param>
    /// <returns>The snapshot, with no notice log.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, or malformed (T-2).</exception>
    /// <exception cref="ArgumentException">The values describe no state of a run (T-2).</exception>
    public static RunSnapshot ReadFormatSeven(ref ContentReader reader, ulong seed) => ReadLine(ref reader, 7, seed);

    /// <summary>
    /// Reads a snapshot of save format 8, which holds no story state (D-540). The resume starts
    /// with no flag on and no story scene.
    /// </summary>
    /// <param name="reader">The reader of the line, which names the save file.</param>
    /// <param name="seed">The seed of the header, which opens the NPC stream (D-1137).</param>
    /// <returns>The snapshot, with no story state.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, or malformed (T-2).</exception>
    /// <exception cref="ArgumentException">The values describe no state of a run (T-2).</exception>
    public static RunSnapshot ReadFormatEight(ref ContentReader reader, ulong seed) => ReadLine(ref reader, 8, seed);

    /// <summary>
    /// Reads a snapshot of save format 9, which holds no lesson, no lesson pack, and no swap place
    /// (D-1018, D-1024, D-1030). The resume gives the start lessons of the fixture.
    /// </summary>
    /// <param name="reader">The reader of the line, which names the save file.</param>
    /// <param name="seed">The seed of the header, which opens the NPC stream (D-1137).</param>
    /// <returns>The snapshot, with no lesson.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, or malformed (T-2).</exception>
    /// <exception cref="ArgumentException">The values describe no state of a run (T-2).</exception>
    public static RunSnapshot ReadFormatNine(ref ContentReader reader, ulong seed) => ReadLine(ref reader, 9, seed);

    /// <summary>
    /// Reads a snapshot of save format 10, which holds no gear slot, no spare gear, and no gold
    /// (D-44, D-1038, D-1043). The resume gives the start gear of the fixture, and no gold.
    /// </summary>
    /// <param name="reader">The reader of the line, which names the save file.</param>
    /// <param name="seed">The seed of the header, which opens the NPC stream (D-1137).</param>
    /// <returns>The snapshot, with no gear.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, or malformed (T-2).</exception>
    /// <exception cref="ArgumentException">The values describe no state of a run (T-2).</exception>
    public static RunSnapshot ReadFormatTen(ref ContentReader reader, ulong seed) => ReadLine(ref reader, 10, seed);

    /// <summary>
    /// Reads a snapshot of save format 11, which holds the swap place of D-1030. The read checks
    /// the field and drops it, because a swap of lessons needs no place (D-1050).
    /// </summary>
    /// <param name="reader">The reader of the line, which names the save file.</param>
    /// <param name="seed">The seed of the header, which opens the NPC stream (D-1137).</param>
    /// <returns>The snapshot.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, or malformed (T-2).</exception>
    /// <exception cref="ArgumentException">The values describe no state of a run (T-2).</exception>
    public static RunSnapshot ReadFormatEleven(ref ContentReader reader, ulong seed) => ReadLine(ref reader, 11, seed);

    /// <summary>
    /// Reads a snapshot of save format 12, which holds no state of the torch (D-1064). The resume
    /// puts the torch away.
    /// </summary>
    /// <param name="reader">The reader of the line, which names the save file.</param>
    /// <param name="seed">The seed of the header, which opens the NPC stream (D-1137).</param>
    /// <returns>The snapshot, with no state of the torch.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, or malformed (T-2).</exception>
    /// <exception cref="ArgumentException">The values describe no state of a run (T-2).</exception>
    public static RunSnapshot ReadFormatTwelve(ref ContentReader reader, ulong seed) => ReadLine(ref reader, 12, seed);

    /// <summary>
    /// Reads a snapshot of save format 13, whose story scene holds no step id (D-1112). The
    /// resume reads the index of the step alone, as the build that wrote it did.
    /// </summary>
    /// <param name="reader">The reader of the line, which names the save file.</param>
    /// <param name="seed">The seed of the header, which opens the NPC stream (D-1137).</param>
    /// <returns>The snapshot, with no step id.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, or malformed (T-2).</exception>
    /// <exception cref="ArgumentException">The values describe no state of a run (T-2).</exception>
    public static RunSnapshot ReadFormatThirteen(ref ContentReader reader, ulong seed) => ReadLine(ref reader, 13, seed);

    /// <summary>
    /// Reads a snapshot of save format 14, which holds no NPC and no NPC stream (D-1137). The resume
    /// puts each NPC of the map on its start tile, and the NPC stream joins at its first value.
    /// </summary>
    /// <param name="reader">The reader of the line, which names the save file.</param>
    /// <param name="seed">The seed of the header, which opens the NPC stream (D-1137).</param>
    /// <returns>The snapshot, with no NPC list.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, or malformed (T-2).</exception>
    /// <exception cref="ArgumentException">The values describe no state of a run (T-2).</exception>
    public static RunSnapshot ReadFormatFourteen(ref ContentReader reader, ulong seed) => ReadLine(ref reader, 14, seed);

    /// <summary>
    /// Reads a snapshot of save format 15, which holds no stock of a shop (D-1152). The resume
    /// starts each stock at the count of its shop file.
    /// </summary>
    /// <param name="reader">The reader of the line, which names the save file.</param>
    /// <param name="seed">The seed of the header, which opens the NPC stream (D-1137).</param>
    /// <returns>The snapshot, with no stock.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, or malformed (T-2).</exception>
    /// <exception cref="ArgumentException">The values describe no state of a run (T-2).</exception>
    public static RunSnapshot ReadFormatFifteen(ref ContentReader reader, ulong seed) => ReadLine(ref reader, 15, seed);

    /// <summary>
    /// Reads a snapshot of save format 16, which names the pool of each character `mp` (D-1197).
    /// The read gives that value as the AP of the character.
    /// </summary>
    /// <param name="reader">The reader of the line, which names the save file.</param>
    /// <returns>The snapshot.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, or malformed (T-2).</exception>
    /// <exception cref="ArgumentException">The values describe no state of a run (T-2).</exception>
    public static RunSnapshot ReadFormatSixteen(ref ContentReader reader) => ReadLine(ref reader, 16, null);

    /// <summary>
    /// Reads a snapshot of save format 17, which holds no memory of a map (D-555). The resume
    /// starts the memory with the dead enemies of the map that the party stands on.
    /// </summary>
    /// <param name="reader">The reader of the line, which names the save file.</param>
    /// <returns>The snapshot, with no memory of a map.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, or malformed (T-2).</exception>
    /// <exception cref="ArgumentException">The values describe no state of a run (T-2).</exception>
    public static RunSnapshot ReadFormatSeventeen(ref ContentReader reader) => ReadLine(ref reader, 17, null);

    /// <summary>
    /// Reads a snapshot of save format 18, whose memory of a map holds no spent trap (D-1229). Each
    /// trap of such a save stays armed.
    /// </summary>
    /// <param name="reader">The reader of the line, which names the save file.</param>
    /// <returns>The snapshot, with no spent trap.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, or malformed (T-2).</exception>
    /// <exception cref="ArgumentException">The values describe no state of a run (T-2).</exception>
    public static RunSnapshot ReadFormatEighteen(ref ContentReader reader) => ReadLine(ref reader, 18, null);

    private static RunSnapshot ReadLine(ref ContentReader reader, int format, ulong? seed)
    {
        long? tick = null;
        bool? menu = null;
        long? world = null;
        MapSnapshot? map = null;
        PartySnapshot? party = null;
        BattleValues? battle = null;
        List<ContentId>? notices = null;
        StoryValues? story = null;
        List<StockValues>? stock = null;
        List<PlaceValues>? places = null;
        List<StreamPosition>? streams = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "tick":
                    tick = reader.ReadLong();
                    break;
                case "menu":
                    menu = reader.ReadBoolean();
                    break;
                case "world":
                    world = reader.ReadLong();
                    break;
                // Save format 7 and older predate the notice log (D-985).
                case "notices" when format < 8:
                    throw reader.Refuse(
                        $"the snapshot of save format {format} holds a notice log, and that format predates it (D-985)");
                case "notices":
                    notices = ReadNotices(ref reader);
                    break;
                // Save format 8 and older predate the story state (D-540).
                case "story" when format < 9:
                    throw reader.Refuse(
                        $"the snapshot of save format {format} holds a story state, and that format predates it (D-540)");
                case "story":
                    story = StorySnapshotText.Read(ref reader, format);
                    break;
                case "map":
                    map = ReadMap(ref reader, format);
                    break;
                // Save format 3 and older predate the party and the battle, so the read stops at
                // the field and names the format, before the fields inside it (D-765).
                case "party" or "battle" when format < 4:
                    throw reader.Refuse(
                        $"the snapshot of save format {format} holds a party or a battle, and that format predates both (D-765)");
                case "party":
                    party = BattleSnapshotText.ReadParty(ref reader, format);
                    break;
                case "battle":
                    battle = BattleSnapshotText.ReadBattle(ref reader, format);
                    break;
                // Save format 15 and older predate the stock of a shop (D-1152).
                case "stock" when format < 16:
                    throw reader.Refuse(
                        $"the snapshot of save format {format} holds a stock, and that format predates it (D-1152)");
                case "stock":
                    stock = ReadStock(ref reader);
                    break;
                // Save format 17 and older predate the memory of a map (D-555).
                case "places" when format < 18:
                    throw reader.Refuse(
                        $"the snapshot of save format {format} holds the memory of a map, and that format predates it (D-555)");
                case "places":
                    places = ReadPlaces(ref reader, format);
                    break;
                case "streams":
                    streams = ReadStreams(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        // Save format 3 and older predate the party and the battle. Each later format holds
        // the party (D-166, D-765).
        if (format >= 4)
        {
            _ = reader.Require(party, depth, "party");
        }
        else if (party is not null || battle is not null)
        {
            throw reader.Refuse(
                $"the snapshot of save format {format} holds a party or a battle, and that format predates both (D-765)");
        }

        // Save format 8 and each later format hold the notice log (D-985).
        if (format >= 8)
        {
            _ = reader.Require(notices, depth, "notices");
        }

        // Save format 9 and each later format hold the story state (D-540).
        if (format >= 9)
        {
            _ = reader.Require(story, depth, "story");
        }

        // Save format 16 and each later format hold the stock of each shop (D-1152).
        if (format >= 16)
        {
            _ = reader.Require(stock, depth, "stock");
        }

        // Save format 18 and each later format hold the memory of each map (D-555).
        if (format >= 18)
        {
            _ = reader.Require(places, depth, "places");
        }

        RunSnapshot snapshot = new(
            reader.RequireValue(tick, depth, "tick"),
            reader.RequireValue(menu, depth, "menu"),
            reader.RequireValue(world, depth, "world"),
            reader.Require(map, depth, "map"),
            party,
            battle,
            notices,
            story,
            StreamsOf(reader.Require(streams, depth, "streams"), format, seed),
            stock,
            places);

        snapshot.Check(reader.File);
        return snapshot;
    }

    /// <summary>
    /// Reads a snapshot of save format 1, which holds no map (D-166, D-654). The migration
    /// runs in `RunState.Resume`, which holds the content of this build and can find the
    /// spawn point of the first map.
    /// </summary>
    /// <param name="reader">The reader of the line, which names the save file.</param>
    /// <param name="seed">The seed of the header, which opens the stream of the evaluator and the NPC stream (D-947, D-1137).</param>
    /// <returns>The snapshot, with no map.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, or malformed (T-2).</exception>
    /// <exception cref="ArgumentException">The values describe no state of a run (T-2).</exception>
    public static RunSnapshot ReadFormatOne(ref ContentReader reader, ulong seed)
    {
        long? tick = null;
        bool? menu = null;
        long? world = null;
        int? beats = null;
        int? choice = null;
        List<StreamPosition>? streams = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "tick":
                    tick = reader.ReadLong();
                    break;
                case "menu":
                    menu = reader.ReadBoolean();
                    break;
                case "world":
                    world = reader.ReadLong();
                    break;
                case "beats":
                    beats = reader.ReadInt();
                    break;
                case "choice":
                    choice = reader.ReadInt();
                    break;
                case "streams":
                    streams = ReadStreams(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        // The patrol of the first world walked on a beat, and PR-7 replaced it with the
        // party on a tile map. The two counts take a read, so an unknown field still fails,
        // and the state of that patrol reaches no rule of this build (T-2).
        _ = reader.RequireInt(beats, depth, "beats");
        _ = reader.RequireInt(choice, depth, "choice");

        RunSnapshot snapshot = new(
            reader.RequireValue(tick, depth, "tick"),
            reader.RequireValue(menu, depth, "menu"),
            reader.RequireValue(world, depth, "world"),
            null,
            null,
            null,
            null,
            null,
            StreamsOf(reader.Require(streams, depth, "streams"), 1, seed),
            null,
            null);

        snapshot.Check(reader.File);
        return snapshot;
    }

    /// <summary>
    /// Writes the count that remains of each counted entry that a buy changed (D-1152). This build
    /// writes save format 16, so the field is always present, and it holds an empty array before
    /// the first buy of a counted entry.
    /// </summary>
    private static void WriteStock(Utf8JsonWriter writer, IReadOnlyList<StockValues>? stock)
    {
        if (stock is null)
        {
            throw new ArgumentException(
                "A snapshot that this build writes holds the stock of each shop. A snapshot with none comes from save format 15 or older, and this build never writes one (T-2, D-166).",
                nameof(stock));
        }

        writer.WriteStartArray("stock");
        foreach (StockValues value in stock)
        {
            writer.WriteStartObject();
            writer.WriteString("shop", value.Shop.Value);
            writer.WriteString("thing", value.Thing.Value);
            writer.WriteNumber("left", value.Left);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    /// <summary>Reads the stock of each shop. The resume checks each value against the shop file (D-1152, T-2).</summary>
    private static List<StockValues> ReadStock(ref ContentReader reader)
    {
        List<StockValues> stock = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, stock.Count))
        {
            ContentId? shop = null;
            ContentId? thing = null;
            int? left = null;
            int fields = reader.ReadObjectStart();
            while (reader.ReadNextField(fields, out string field))
            {
                switch (field)
                {
                    case "shop":
                        shop = reader.ReadContentId(ShopList.Kind);
                        break;
                    case "thing":
                        thing = reader.ReadContentId();
                        break;
                    case "left":
                        left = reader.ReadInt();
                        break;
                    default:
                        throw reader.UnknownField(field);
                }
            }

            stock.Add(new StockValues(reader.Require(shop, fields, "shop"), reader.Require(thing, fields, "thing"), reader.RequireInt(left, fields, "left")));
        }

        return stock;
    }

    /// <summary>
    /// Writes the memory of each map that holds something (D-385, D-555, D-1229). This build writes
    /// save format 19, so the field is always present, and it holds an empty array before the first
    /// kill, the first open door, the first opened chest, and the first spent trap.
    /// </summary>
    private static void WritePlaces(Utf8JsonWriter writer, IReadOnlyList<PlaceValues>? places)
    {
        if (places is null)
        {
            throw new ArgumentException(
                "A snapshot that this build writes holds the memory of each map. A snapshot with none comes from save format 17 or older, and this build never writes one (T-2, D-166).",
                nameof(places));
        }

        writer.WriteStartArray("places");
        foreach (PlaceValues place in places)
        {
            writer.WriteStartObject();
            writer.WriteString("map", place.Map.Value);
            WriteIds(writer, "dead", place.Dead);
            WriteIds(writer, "doors", place.Doors);
            writer.WriteStartArray("chests");
            foreach (ChestValues chest in place.Chests)
            {
                writer.WriteStartObject();
                writer.WriteString("chest", chest.Chest.Value);
                writer.WriteStartArray("left");
                foreach (ChestLeft left in chest.Left)
                {
                    writer.WriteStartObject();
                    writer.WriteString("thing", left.Thing.Value);
                    writer.WriteNumber("count", left.Count);
                    writer.WriteEndObject();
                }

                writer.WriteEndArray();
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            WriteIds(writer, "spent", place.Spent);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    private static void WriteIds(Utf8JsonWriter writer, string field, IReadOnlyList<ContentId> ids)
    {
        writer.WriteStartArray(field);
        foreach (ContentId id in ids)
        {
            writer.WriteStringValue(id.Value);
        }

        writer.WriteEndArray();
    }

    /// <summary>
    /// Reads the memory of each map. The resume checks each value against the maps of this build
    /// (D-555, T-2). Save format 18 holds no spent trap, so each trap of such a save stays armed (D-1229).
    /// </summary>
    private static List<PlaceValues> ReadPlaces(ref ContentReader reader, int format)
    {
        List<PlaceValues> places = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, places.Count))
        {
            ContentId? map = null;
            List<ContentId>? dead = null;
            List<ContentId>? doors = null;
            List<ChestValues>? chests = null;
            List<ContentId>? spent = null;
            int fields = reader.ReadObjectStart();
            while (reader.ReadNextField(fields, out string field))
            {
                switch (field)
                {
                    case "map":
                        map = reader.ReadContentId(GameMap.IdKind);
                        break;
                    case "dead":
                        dead = ReadIds(ref reader, null);
                        break;
                    case "doors":
                        doors = ReadIds(ref reader, MapThingKinds.NameOf(MapThingKind.Door));
                        break;
                    case "chests":
                        chests = ReadChests(ref reader);
                        break;
                    case "spent" when format < 19:
                        throw reader.Refuse($"the memory of a map in save format {format} holds spent traps, and that format predates them (D-1229)");
                    case "spent":
                        spent = ReadIds(ref reader, MapThingKinds.NameOf(MapThingKind.Trap));
                        break;
                    default:
                        throw reader.UnknownField(field);
                }
            }

            places.Add(new PlaceValues(
                reader.Require(map, fields, "map"),
                reader.Require(dead, fields, "dead"),
                reader.Require(doors, fields, "doors"),
                reader.Require(chests, fields, "chests"),
                format >= 19 ? reader.Require(spent, fields, "spent") : []));
        }

        return places;
    }

    private static List<ContentId> ReadIds(ref ContentReader reader, string? kind)
    {
        List<ContentId> ids = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, ids.Count))
        {
            ids.Add(kind is null ? reader.ReadContentId() : reader.ReadContentId(kind));
        }

        return ids;
    }

    private static List<ChestValues> ReadChests(ref ContentReader reader)
    {
        List<ChestValues> chests = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, chests.Count))
        {
            ContentId? chest = null;
            List<ChestLeft>? left = null;
            int fields = reader.ReadObjectStart();
            while (reader.ReadNextField(fields, out string field))
            {
                switch (field)
                {
                    case "chest":
                        chest = reader.ReadContentId(MapThingKinds.NameOf(MapThingKind.Chest));
                        break;
                    case "left":
                        left = ReadLeft(ref reader);
                        break;
                    default:
                        throw reader.UnknownField(field);
                }
            }

            chests.Add(new ChestValues(reader.Require(chest, fields, "chest"), reader.Require(left, fields, "left")));
        }

        return chests;
    }

    private static List<ChestLeft> ReadLeft(ref ContentReader reader)
    {
        List<ChestLeft> left = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, left.Count))
        {
            ContentId? thing = null;
            int? count = null;
            int fields = reader.ReadObjectStart();
            while (reader.ReadNextField(fields, out string field))
            {
                switch (field)
                {
                    case "thing":
                        thing = reader.ReadContentId();
                        break;
                    case "count":
                        count = reader.ReadInt();
                        break;
                    default:
                        throw reader.UnknownField(field);
                }
            }

            left.Add(new ChestLeft(reader.Require(thing, fields, "thing"), reader.RequireInt(count, fields, "count")));
        }

        return left;
    }

    private static List<ContentId> ReadNotices(ref ContentReader reader)
    {
        List<ContentId> notices = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, notices.Count))
        {
            notices.Add(reader.ReadContentId(NoticeList.Kind));
        }

        return notices;
    }

    private static MapSnapshot ReadMap(ref ContentReader reader, int format)
    {
        ContentId? id = null;
        List<PatrolValues>? enemies = null;
        SightMark? mark = null;
        MapEncounter? encounter = null;
        List<NpcValues>? npcs = null;
        int? x = null;
        int? y = null;
        string? facing = null;
        string? stepping = null;
        int? stepTicks = null;
        List<string>? walked = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "id":
                    id = reader.ReadContentId(GameMap.IdKind);
                    break;
                case "x":
                    x = reader.ReadInt();
                    break;
                case "y":
                    y = reader.ReadInt();
                    break;
                case "facing":
                    facing = reader.ReadString();
                    break;
                case "stepping":
                    stepping = reader.ReadString();
                    break;
                case "step_ticks":
                    stepTicks = reader.ReadInt();
                    break;
                case "walked":
                    walked = ReadWalked(ref reader);
                    break;
                case "enemies":
                    enemies = ReadEnemies(ref reader);
                    break;
                case "mark":
                    mark = ReadMark(ref reader);
                    break;
                case "encounter":
                    encounter = ReadEncounter(ref reader);
                    break;
                // Save format 14 and older predate the NPCs (D-1137).
                case "npcs" when format < 15:
                    throw reader.Refuse(
                        $"the map of save format {format} holds an NPC list, and that format predates the NPCs (D-1137)");
                case "npcs":
                    npcs = ReadNpcs(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        // Save format 2 predates the enemies, so its map object holds no enemy list, no
        // mark, and no encounter. Each later format holds the list (D-654, D-750).
        if (format >= 3)
        {
            _ = reader.Require(enemies, depth, "enemies");
        }
        else if (enemies is not null || mark is not null || encounter is not null)
        {
            throw reader.Refuse(
                $"the map of save format {format} holds an enemy list, a mark, or an encounter, and that format predates the enemies (D-750)");
        }

        // Save format 15 and each later format hold the NPC list (D-1137).
        if (format >= 15)
        {
            _ = reader.Require(npcs, depth, "npcs");
        }

        return new MapSnapshot(
            reader.Require(id, depth, "id"),
            reader.RequireInt(x, depth, "x"),
            reader.RequireInt(y, depth, "y"),
            ReadDirection(ref reader, reader.Require(facing, depth, "facing"), "facing"),
            stepping is null ? null : ReadDirection(ref reader, stepping, "stepping"),
            reader.RequireInt(stepTicks, depth, "step_ticks"),
            reader.Require(walked, depth, "walked"),
            enemies,
            mark,
            encounter,
            npcs);
    }

    private static List<NpcValues> ReadNpcs(ref ContentReader reader)
    {
        List<NpcValues> npcs = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, npcs.Count))
        {
            npcs.Add(ReadNpc(ref reader));
        }

        return npcs;
    }

    private static NpcValues ReadNpc(ref ContentReader reader)
    {
        ContentId? id = null;
        int? x = null;
        int? y = null;
        string? facing = null;
        string? stepping = null;
        int? stepTicks = null;
        int? target = null;
        bool? forward = null;
        int? waitTicks = null;
        bool? walksHome = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "id":
                    id = reader.ReadContentId(Npc.IdKind);
                    break;
                case "x":
                    x = reader.ReadInt();
                    break;
                case "y":
                    y = reader.ReadInt();
                    break;
                case "facing":
                    facing = reader.ReadString();
                    break;
                case "stepping":
                    stepping = reader.ReadString();
                    break;
                case "step_ticks":
                    stepTicks = reader.ReadInt();
                    break;
                case "target":
                    target = reader.ReadInt();
                    break;
                case "forward":
                    forward = reader.ReadBoolean();
                    break;
                case "wait_ticks":
                    waitTicks = reader.ReadInt();
                    break;
                case "walks_home":
                    walksHome = reader.ReadBoolean();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new NpcValues(
            reader.Require(id, depth, "id"),
            reader.RequireInt(x, depth, "x"),
            reader.RequireInt(y, depth, "y"),
            ReadDirection(ref reader, reader.Require(facing, depth, "facing"), "facing"),
            stepping is null ? null : ReadDirection(ref reader, stepping, "stepping"),
            reader.RequireInt(stepTicks, depth, "step_ticks"),
            reader.RequireInt(target, depth, "target"),
            reader.RequireValue(forward, depth, "forward"),
            reader.RequireInt(waitTicks, depth, "wait_ticks"),
            reader.RequireValue(walksHome, depth, "walks_home"));
    }

    private static List<PatrolValues> ReadEnemies(ref ContentReader reader)
    {
        List<PatrolValues> enemies = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, enemies.Count))
        {
            enemies.Add(ReadEnemy(ref reader));
        }

        return enemies;
    }

    private static PatrolValues ReadEnemy(ref ContentReader reader)
    {
        ContentId? id = null;
        int? x = null;
        int? y = null;
        string? facing = null;
        string? stepping = null;
        int? stepTicks = null;
        int? target = null;
        bool? forward = null;
        int? graceTicks = null;
        bool? dead = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "id":
                    id = reader.ReadContentId(Patrol.IdKind);
                    break;
                case "x":
                    x = reader.ReadInt();
                    break;
                case "y":
                    y = reader.ReadInt();
                    break;
                case "facing":
                    facing = reader.ReadString();
                    break;
                case "stepping":
                    stepping = reader.ReadString();
                    break;
                case "step_ticks":
                    stepTicks = reader.ReadInt();
                    break;
                case "target":
                    target = reader.ReadInt();
                    break;
                case "forward":
                    forward = reader.ReadBoolean();
                    break;
                case "grace_ticks":
                    graceTicks = reader.ReadInt();
                    break;
                case "dead":
                    dead = reader.ReadBoolean();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new PatrolValues(
            reader.Require(id, depth, "id"),
            reader.RequireInt(x, depth, "x"),
            reader.RequireInt(y, depth, "y"),
            ReadDirection(ref reader, reader.Require(facing, depth, "facing"), "facing"),
            stepping is null ? null : ReadDirection(ref reader, stepping, "stepping"),
            reader.RequireInt(stepTicks, depth, "step_ticks"),
            reader.RequireInt(target, depth, "target"),
            reader.RequireValue(forward, depth, "forward"),
            reader.RequireInt(graceTicks, depth, "grace_ticks"),
            reader.RequireValue(dead, depth, "dead"));
    }

    private static SightMark ReadMark(ref ContentReader reader)
    {
        ContentId? enemy = null;
        int? ticksLeft = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "enemy":
                    enemy = reader.ReadContentId(Patrol.IdKind);
                    break;
                case "ticks_left":
                    ticksLeft = reader.ReadInt();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new SightMark(
            reader.Require(enemy, depth, "enemy"),
            reader.RequireInt(ticksLeft, depth, "ticks_left"));
    }

    private static MapEncounter ReadEncounter(ref ContentReader reader)
    {
        ContentId? enemy = null;
        ContentId? group = null;
        string? behind = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "enemy":
                    enemy = reader.ReadContentId(Patrol.IdKind);
                    break;
                case "group":
                    group = reader.ReadContentId(Patrol.GroupKind);
                    break;
                case "behind":
                    behind = reader.ReadString();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        string name = reader.Require(behind, depth, "behind");
        if (!EncounterSides.TryOf(name, out EncounterSide side))
        {
            throw reader.Refuse(
                $"the encounter holds the side '{name}', and a side is one of {EncounterSides.EveryName} (D-746)");
        }

        return new MapEncounter(
            reader.Require(enemy, depth, "enemy"),
            reader.Require(group, depth, "group"),
            side);
    }

    private static StepDirection ReadDirection(ref ContentReader reader, string name, string field)
    {
        foreach (StepDirection candidate in StepDirections.All)
        {
            if (string.CompareOrdinal(StepDirections.NameOf(candidate), name) == 0)
            {
                return candidate;
            }
        }

        throw reader.Refuse($"the field '{field}' holds '{name}', and a direction is north, south, east, or west (D-716)");
    }

    private static List<string> ReadWalked(ref ContentReader reader)
    {
        List<string> rows = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, rows.Count))
        {
            rows.Add(reader.ReadString());
        }

        return rows;
    }

    /// <summary>
    /// Gives the streams of a snapshot of this build. Save format 5 and older predate the stream
    /// of the evaluator, and save format 14 and older predate the NPC stream. Each stream that a
    /// format predates joins at its first value, from the seed of the header, because no build of
    /// that format drew from it (D-947, D-1137). A snapshot of this build holds every stream, and
    /// the check of the snapshot refuses a count other than that of <see cref="RandomStreams.All"/> (T-2).
    /// </summary>
    private static IReadOnlyList<StreamPosition> StreamsOf(List<StreamPosition> streams, int format, ulong? seed)
    {
        if (format >= 15)
        {
            return streams;
        }

        ulong headerSeed = seed ?? throw new ArgumentException($"A snapshot of save format {format} needs the seed of its header to open the streams that it predates (D-947, D-1137).", nameof(seed));
        var migrated = new List<StreamPosition>(streams);
        if (format <= 5)
        {
            migrated.Add(FirstPositionOf(headerSeed, StreamId.Evaluator));
        }

        migrated.Add(FirstPositionOf(headerSeed, StreamId.Npc));
        return migrated;
    }

    private static StreamPosition FirstPositionOf(ulong seed, StreamId stream)
    {
        RandomStream opened = RandomStreams.Open(seed, stream);
        return new StreamPosition(opened.Stream, opened.Generator.State, opened.Generator.Increment);
    }

    private static List<StreamPosition> ReadStreams(ref ContentReader reader)
    {
        List<StreamPosition> streams = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, streams.Count))
        {
            streams.Add(ReadStream(ref reader));
        }

        return streams;
    }

    private static StreamPosition ReadStream(ref ContentReader reader)
    {
        int? number = null;
        ulong? state = null;
        ulong? increment = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "stream":
                    number = reader.ReadInt();
                    break;
                case "state":
                    state = reader.ReadHexUInt64();
                    break;
                case "increment":
                    increment = reader.ReadHexUInt64();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new StreamPosition(
            (StreamId)reader.RequireInt(number, depth, "stream"),
            reader.RequireValue(state, depth, "state"),
            reader.RequireValue(increment, depth, "increment"));
    }
}
