using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Story;

namespace TheThingBelow.Core.Runs;

/// <summary>The confirm rule of the map: the player confirms while the lead faces a tile (D-1131).</summary>
/// <remarks>
/// The faced tile is the tile in front of the lead. An NPC that stands there ends its step and
/// turns to face the lead, and then its talk trigger fires when one holds (D-1005, D-1139). With
/// no such trigger, the service of the NPC opens. With neither, a log line says so, and PR-36
/// adds the hub lines that the dialogue box shows.
/// <para>
/// With no NPC there, the first thing of the tile that a confirm reads decides: a service point
/// opens its service (D-1142). PR-16 adds the door, the lock, the chest, and the save point to
/// this rule, and until then a confirm of one logs that (D-1131). A trap, a spawn point, and a
/// marker take no confirm.
/// </para>
/// <para>
/// The world step calls this rule once in a tick, only while the lead stands and no battle, no
/// encounter, no story scene, and no menu holds the world (D-1131).
/// </para>
/// </remarks>
public static class ConfirmRules
{
    /// <summary>Applies the confirm of this tick to the faced tile (D-1131).</summary>
    /// <param name="state">The run, with the lead standing on the map.</param>
    /// <param name="log">The log entries of this tick (D-179).</param>
    /// <returns>True when a story scene started or a service opened, which holds the world from this tick.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="SimulationException">The lead steps, or a step of the story scene breaks a rule (T-2).</exception>
    public static bool Confirm(RunState state, List<LogEntry> log)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(log);

        MapState party = state.Party;
        if (party.Stepping is not null)
        {
            throw new SimulationException("a confirm of the map while the lead steps, and the world step reads a confirm while the lead stands alone (D-1131)", state.Context("confirm"));
        }

        TilePoint faced = party.LeadAt.Step(party.Facing);
        if (party.Npcs.TryNpcStandingOn(faced, out NpcState? npc))
        {
            return Talk(state, npc!, log);
        }

        foreach (MapThing thing in party.Map.ThingsAt(faced))
        {
            switch (thing.Kind)
            {
                case MapThingKind.ServicePoint:
                    MapService service = party.Map.ServiceOn(thing.Id)
                        ?? throw new InvalidOperationException($"The service point '{thing.Id.Value}' holds no service, and the load of a map refuses such a service point (D-1142, T-2).");
                    return ServiceRules.Open(state, service, log);
                case MapThingKind.Door:
                case MapThingKind.Lock:
                case MapThingKind.Chest:
                case MapThingKind.SavePoint:
                    // PR-16 adds the confirm of each of these kinds here (D-1131).
                    log.Add(Entry(state, LogLevel.Debug, $"the lead confirmed at a {MapThingKinds.NameOf(thing.Kind)}, and PR-16 adds its rule", [new LogField("thing", thing.Id.Value)]));
                    return false;
                case MapThingKind.Trap:
                case MapThingKind.SpawnPoint:
                case MapThingKind.Marker:
                    break;
                default:
                    throw new SimulationException($"the thing '{thing.Id.Value}' takes the kind {thing.Kind}, which the confirm rule does not read (D-1131)", state.Context("confirm"));
            }
        }

        log.Add(Entry(state, LogLevel.Debug, "the lead confirmed at a tile with nothing to confirm", [LogField.OfNumber("x", faced.X), LogField.OfNumber("y", faced.Y)]));
        return false;
    }

    /// <summary>
    /// Talks with one NPC: it ends its step and turns to the lead, and then its talk trigger or its
    /// service answers (D-1005, D-1131, D-1139).
    /// </summary>
    private static bool Talk(RunState state, NpcState npc, List<LogEntry> log)
    {
        npc.FaceTalker(StepDirections.Opposite(state.Party.Facing));
        ContentId id = npc.Npc.Id;
        log.Add(Entry(state, LogLevel.Info, "the lead talked with an NPC, and the NPC turned to the lead", [new LogField("npc", id.Value), new LogField("facing", StepDirections.NameOf(npc.Facing))]));

        if (StoryRules.FireTalk(state, id, log))
        {
            return true;
        }

        if (state.Party.Map.ServiceOn(id) is MapService service)
        {
            return ServiceRules.Open(state, service, log);
        }

        // PR-36 adds the hub lines that the dialogue box shows here.
        log.Add(Entry(state, LogLevel.Debug, "the NPC has no talk that holds and no service, and PR-36 adds its hub lines", [new LogField("npc", id.Value)]));
        return false;
    }

    private static LogEntry Entry(RunState state, LogLevel level, string message, IReadOnlyList<LogField> fields) =>
        new(level, message, state.Tick, LogSubsystems.World, fields);
}
