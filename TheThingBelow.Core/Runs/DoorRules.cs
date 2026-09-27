using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Notices;

namespace TheThingBelow.Core.Runs;

/// <summary>The rules of a door and its lock: the confirm that opens a door (D-41, D-386, D-1219).</summary>
/// <remarks>
/// A closed door is solid, and the lead faces it and confirms (D-1131, D-1142). A door with no lock
/// opens. A locked door opens with its key, which stays on the Keyring (D-1219). A pickable lock
/// also opens for a Theft drill that a standing character of the party carries, and a story lock
/// never does (D-386). The memory of the map keeps the door open past the exit (D-555).
/// </remarks>
public static class DoorRules
{
    /// <summary>The source of the ids of this class, for the error of a malformed id (T-2).</summary>
    private const string Source = "TheThingBelow.Core/Runs/DoorRules.cs";

    /// <summary>The notice of a lock that stays shut (D-386).</summary>
    public static readonly ContentId LockedNotice = ContentId.Parse("notice.door_locked", Source, nameof(LockedNotice));

    /// <summary>The notice of a lock that its key opened (D-1219).</summary>
    public static readonly ContentId KeyNotice = ContentId.Parse("notice.door_key", Source, nameof(KeyNotice));

    /// <summary>The notice of a lock that a Theft drill opened (D-386).</summary>
    public static readonly ContentId PickedNotice = ContentId.Parse("notice.door_picked", Source, nameof(PickedNotice));

    /// <summary>Every notice that these rules post, which the notice file of a build must hold (D-989, T-2).</summary>
    public static readonly IReadOnlyList<ContentId> Notices = [LockedNotice, KeyNotice, PickedNotice];

    /// <summary>Answers a confirm at the door on one tile (D-41, D-386, D-1131).</summary>
    /// <param name="state">The run, with the lead facing the tile.</param>
    /// <param name="at">The tile of the door.</param>
    /// <param name="log">The log entries of this tick (D-179).</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="SimulationException">The tile holds no door, or the pack names a key that the item file lacks (T-2).</exception>
    /// <remarks>A confirm at an open door does nothing, and a log line says so.</remarks>
    public static void Confirm(RunState state, TilePoint at, List<LogEntry> log)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(log);

        GameMap map = state.Party.Map;
        RunContext context = state.Context($"door/{at}");
        MapThing door = map.DoorAt(at)
            ?? throw new SimulationException($"a confirm of a door at {at}, and the map '{map.Id.Value}' holds no door there (D-41)", context);
        PlaceState place = state.Party.Place;
        if (place.IsOpen(door.Id))
        {
            log.Add(Entry(state, LogLevel.Debug, "the lead confirmed at an open door", door));
            return;
        }

        if (map.LockAt(at) is not MapThing locked)
        {
            Open(state, door, "the lead opened a door", log);
            return;
        }

        if (locked.Key is ContentId key && state.Characters.CountOf(key) > 0)
        {
            Open(state, door, "the key of a lock opened its door, and the key stays on the Keyring", log);
            NoticeRules.Post(state, KeyNotice, context, log);
            return;
        }

        if (locked.Pickable && CarriesTheft(state))
        {
            Open(state, door, "a Theft drill opened a pickable lock", log);
            NoticeRules.Post(state, PickedNotice, context, log);
            return;
        }

        log.Add(Entry(state, LogLevel.Info, locked.Pickable ? "a lock stayed shut, with no key and no Theft drill" : "a story lock stayed shut, and it needs its key", door));
        NoticeRules.Post(state, LockedNotice, context, log);
    }

    /// <summary>
    /// Tells whether a standing character of the party carries a lesson of Theft in a slot (D-386).
    /// The reserve does not fight, and a downed character picks no lock.
    /// </summary>
    /// <param name="state">The run.</param>
    /// <returns>True when a character of the party who stands carries a Theft drill.</returns>
    /// <exception cref="ArgumentNullException">The state is null (T-2).</exception>
    public static bool CarriesTheft(RunState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        LessonList lessons = state.BattleContent.Lessons;
        foreach (PartyMember member in state.Characters.Members)
        {
            if (member.Down)
            {
                continue;
            }

            foreach (ContentId? held in member.Slots)
            {
                if (held is ContentId lesson && lessons.Lesson(lesson).Kind == AptitudeKind.Theft)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static void Open(RunState state, MapThing door, string message, List<LogEntry> log)
    {
        state.Party.Place.Open(door.Id);
        log.Add(Entry(state, LogLevel.Info, message, door));
    }

    private static LogEntry Entry(RunState state, LogLevel level, string message, MapThing door) =>
        new(level, message, state.Tick, LogSubsystems.World, [new LogField("door", door.Id.Value), new LogField("map", state.Party.Map.Id.Value)]);
}
