using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Streams;

namespace TheThingBelow.Core.Runs;

/// <summary>
/// The invisible fights of the overworld: the step counter and the zones (D-1247, D-1249,
/// D-1250, D-1251).
/// </summary>
/// <remarks>
/// Each arrival on a tile of a live zone adds the rate of the zone to the danger count, and the
/// step then draws a number from 0 to 9999 on the encounter stream. A draw under the count starts
/// a fight, sets the count to zero, and draws the group from the weights of the zone (D-1261). A
/// zone at rate zero, or a zone whose condition fails, keeps the count and draws nothing (D-1263).
/// <para>
/// Every draw comes from the encounter stream alone, so a walk of a patrol or an NPC moves no
/// fight of the overworld (G-4). A zone fight has no side from behind (D-1265).
/// </para>
/// </remarks>
public static class EncounterRules
{
    /// <summary>Reads the zone of a tile that the party reached, and starts its fight when the draw falls under the count.</summary>
    /// <param name="state">The run, whose party reached the tile on this tick and holds no battle and no encounter.</param>
    /// <param name="at">The tile that the party reached.</param>
    /// <param name="log">The log entries of this tick (D-179).</param>
    /// <returns>True when a fight of the zone started on this tick.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="SimulationException">A walkable tile of the overworld holds no zone, which the load refuses (T-2).</exception>
    public static bool Arrive(RunState state, TilePoint at, List<LogEntry> log)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(log);

        GameMap map = state.Party.Map;
        if (map.Kind != MapKind.Overworld)
        {
            return false;
        }

        EncounterZone zone = map.ZoneAt(at)
            ?? throw new SimulationException($"an arrival on the tile {at} of the overworld '{map.Id.Value}', which holds no zone. The load gives each walkable tile a zone (D-1262)", state.Context("encounter"));
        if (zone.Rate == 0 || !zone.Condition.Holds(state.Story.Flags))
        {
            return false;
        }

        state.AddDanger(zone.Rate);
        RandomStream stream = state.Stream(StreamId.Encounter);
        RunContext context = state.Context($"encounter/{zone.Id.Value}");
        int draw = stream.NextInt(BasisPoints.One, context);
        if (draw >= state.Danger)
        {
            return false;
        }

        ContentId group = zone.GroupAt(stream.NextInt(zone.TotalWeight, context));
        log.Add(new LogEntry(
            LogLevel.Info,
            "a step onto a zone of the overworld started a fight",
            state.Tick,
            LogSubsystems.World,
            [
                new LogField("zone", zone.Id.Value),
                new LogField("group", group.Value),
                LogField.OfNumber("danger", state.Danger),
                LogField.OfNumber("draw", draw),
            ]));
        state.ClearDanger();
        BattleTurns.BeginZone(state, zone.Id, group, log);
        return true;
    }
}
