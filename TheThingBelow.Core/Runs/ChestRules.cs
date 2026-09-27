using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Notices;

namespace TheThingBelow.Core.Runs;

/// <summary>The rules of a chest: the confirm that takes its gold and its entries (D-385, D-1024, D-1161, D-1220).</summary>
/// <remarks>
/// A chest is solid, and the lead faces it and confirms (D-1131, D-1142). The first confirm adds
/// the gold of the chest to the party. Then each entry goes to the party up to the stack limit, a
/// lesson that the party owns gives its fallback item, and each copy that does not fit stays in
/// the chest (D-385, D-1024). A later confirm takes what stayed, up to the limit again.
/// <para>
/// Each copy posts one notice in the singular, and the gold posts one notice with its count
/// (D-1224). The memory of the map keeps what stays past the exit and in the save (D-385, D-555).
/// </para>
/// </remarks>
public static class ChestRules
{
    /// <summary>The source of the ids of this class, for the error of a malformed id (T-2).</summary>
    private const string Source = "TheThingBelow.Core/Runs/ChestRules.cs";

    /// <summary>The notice of one copy that the party took, which names the thing (D-1224).</summary>
    public static readonly ContentId FoundNotice = ContentId.Parse("notice.chest_found", Source, nameof(FoundNotice));

    /// <summary>The notice of the gold of a chest, which names the count (D-1161, D-1224).</summary>
    public static readonly ContentId GoldNotice = ContentId.Parse("notice.chest_gold", Source, nameof(GoldNotice));

    /// <summary>The notice of one copy that stays in the chest, which names the thing (D-385, D-1224).</summary>
    public static readonly ContentId LeftNotice = ContentId.Parse("notice.chest_left", Source, nameof(LeftNotice));

    /// <summary>The notice of a chest that holds nothing more (D-385).</summary>
    public static readonly ContentId EmptyNotice = ContentId.Parse("notice.chest_empty", Source, nameof(EmptyNotice));

    /// <summary>Every notice that these rules post, which the notice file of a build must hold (D-989, T-2).</summary>
    public static readonly IReadOnlyList<ContentId> Notices = [FoundNotice, GoldNotice, LeftNotice, EmptyNotice];

    /// <summary>Answers a confirm at one chest (D-385, D-1131, D-1220).</summary>
    /// <param name="state">The run, with the lead facing the chest.</param>
    /// <param name="chest">The chest.</param>
    /// <param name="log">The log entries of this tick (D-179).</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="SimulationException">The thing is not a chest, or the gold passes the largest gold (T-2).</exception>
    /// <exception cref="ContentException">An entry names an item, a piece, or a lesson that the content lacks (T-2).</exception>
    public static void Open(RunState state, MapThing chest, List<LogEntry> log)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(chest);
        ArgumentNullException.ThrowIfNull(log);

        RunContext context = state.Context($"chest/{chest.Id.Value}");
        ChestContents contents = chest.Contents
            ?? throw new SimulationException($"a confirm of the chest '{chest.Id.Value}', which holds no contents, and the reader of a map refuses such a chest (D-1220)", context);
        PlaceState place = state.Party.Place;
        IReadOnlyList<ChestLeft>? stayed = place.LeftIn(chest.Id);
        if (stayed is not null && stayed.Count == 0)
        {
            log.Add(Entry(state, "the lead confirmed at an emptied chest", chest));
            NoticeRules.Post(state, EmptyNotice, context, log);
            return;
        }

        List<ChestLeft> left = stayed is null
            ? FirstOpen(state, contents, context, log)
            : TakeAgain(state, stayed, context, log);
        place.Keep(chest.Id, left);
        log.Add(new LogEntry(
            LogLevel.Info,
            stayed is null ? "the party opened a chest" : "the party took from a chest again",
            state.Tick,
            LogSubsystems.World,
            [new LogField("chest", chest.Id.Value), new LogField("map", state.Party.Map.Id.Value), LogField.OfNumber("kinds-left", left.Count)]));
    }

    /// <summary>Takes the gold and each entry of a chest that the party never opened, and gives what stays.</summary>
    private static List<ChestLeft> FirstOpen(RunState state, ChestContents contents, RunContext context, List<LogEntry> log)
    {
        if (contents.Gold > 0)
        {
            state.Characters.AddGold(contents.Gold, context);
            NoticeRules.Post(state, GoldNotice, null, contents.Gold, context, log);
        }

        List<ChestLeft> left = [];
        foreach (ChestEntry entry in contents.Entries)
        {
            if (entry.Fallback is ContentId fallback)
            {
                TakeLesson(state, entry.Thing, fallback, left, context, log);
                continue;
            }

            Take(state, entry.Thing, entry.Count, left, context, log);
        }

        return left;
    }

    /// <summary>Takes what stayed in a chest, up to the stack limit, and gives what stays now (D-385).</summary>
    private static List<ChestLeft> TakeAgain(RunState state, IReadOnlyList<ChestLeft> stayed, RunContext context, List<LogEntry> log)
    {
        List<ChestLeft> left = [];
        foreach (ChestLeft entry in stayed)
        {
            Take(state, entry.Thing, entry.Count, left, context, log);
        }

        return left;
    }

    /// <summary>
    /// Adds a lesson to the lesson pack, or gives one copy of its fallback item when the party owns
    /// the lesson (D-1024). A fallback that does not fit stays in the chest.
    /// </summary>
    private static void TakeLesson(RunState state, ContentId lesson, ContentId fallback, List<ChestLeft> left, RunContext context, List<LogEntry> log)
    {
        if (state.Characters.Owns(lesson))
        {
            log.Add(new LogEntry(LogLevel.Info, "the party owns the lesson of a chest, and the chest gives its fallback item", state.Tick, LogSubsystems.World, [new LogField("lesson", lesson.Value), new LogField("fallback", fallback.Value)]));
            Take(state, fallback, 1, left, context, log);
            return;
        }

        state.Characters.AddLesson(state.BattleContent.Lessons.Lesson(lesson), context);
        NoticeRules.Post(state, FoundNotice, lesson, null, context, log);
    }

    /// <summary>Puts copies of an item or a piece in the pack, and notes each copy that stays (D-385, D-1224).</summary>
    private static void Take(RunState state, ContentId thing, int count, List<ChestLeft> left, RunContext context, List<LogEntry> log)
    {
        int stays = state.Characters.Pick(thing, count, state.BattleContent);
        for (int copy = 0; copy < count - stays; copy += 1)
        {
            NoticeRules.Post(state, FoundNotice, thing, null, context, log);
        }

        for (int copy = 0; copy < stays; copy += 1)
        {
            NoticeRules.Post(state, LeftNotice, thing, null, context, log);
        }

        if (stays > 0)
        {
            left.Add(new ChestLeft(thing, stays));
        }
    }

    private static LogEntry Entry(RunState state, string message, MapThing chest) =>
        new(LogLevel.Debug, message, state.Tick, LogSubsystems.World, [new LogField("chest", chest.Id.Value), new LogField("map", state.Party.Map.Id.Value)]);
}
