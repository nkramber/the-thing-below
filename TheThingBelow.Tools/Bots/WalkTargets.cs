using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Story;

namespace TheThingBelow.Tools.Bots;

/// <summary>How the greedy policy reaches a target (D-1183).</summary>
public enum TargetReach
{
    /// <summary>The lead stands next to the target, faces it, and confirms: an NPC, a door, or a service.</summary>
    Confirm,

    /// <summary>The lead steps onto the tile: the tile of a trigger of a story scene.</summary>
    StandOn,

    /// <summary>The lead steps into the body from the next tile: the mark of an enemy.</summary>
    Bump,
}

/// <summary>One target of the walk of the greedy policy (D-1183).</summary>
/// <param name="Key">The id of the target, such as the NPC id, which the policy keeps when it reached the target.</param>
/// <param name="At">The tile of the target.</param>
/// <param name="Reach">How the lead reaches it.</param>
public sealed record WalkTarget(string Key, TilePoint At, TargetReach Reach);

/// <summary>
/// The targets of the walk of the greedy policy on the map of the party, and the shortest path to
/// each one (D-1183). The path reads the map, the NPCs, and the enemies of the state alone.
/// </summary>
public static class WalkTargets
{
    private static readonly StepDirection[] Directions = [StepDirection.North, StepDirection.South, StepDirection.East, StepDirection.West];

    /// <summary>
    /// Gives each target of the map: each NPC, each door, each service point, each tile of a
    /// trigger whose condition holds, and each enemy that lives, in that order (D-1183).
    /// </summary>
    /// <param name="state">The state of the run.</param>
    /// <returns>The targets.</returns>
    public static IReadOnlyList<WalkTarget> Of(RunState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        MapState party = state.Party;
        List<WalkTarget> targets = [];
        foreach (NpcState npc in party.Npcs.All)
        {
            targets.Add(new WalkTarget(npc.Npc.Id.Value, npc.At, TargetReach.Confirm));
        }

        foreach (MapThing thing in party.Map.Things)
        {
            if (thing.Kind is MapThingKind.Door or MapThingKind.ServicePoint)
            {
                targets.Add(new WalkTarget(thing.Id.Value, thing.At, TargetReach.Confirm));
            }
        }

        foreach (SceneTrigger trigger in party.Map.Triggers)
        {
            if (trigger.Kind == TriggerKind.Tile && trigger.At is TilePoint at && trigger.Condition.Holds(state.Story.Flags))
            {
                targets.Add(new WalkTarget(trigger.Id.Value, at, TargetReach.StandOn));
            }
        }

        foreach (PatrolState patrol in party.Patrols.All)
        {
            if (!patrol.Dead)
            {
                targets.Add(new WalkTarget(patrol.Patrol.Id.Value, patrol.At, TargetReach.Bump));
            }
        }

        return targets;
    }

    /// <summary>
    /// Gives the first step of the shortest path from the lead to the tile where it reaches the
    /// target, or no value when no path exists. A step toward a target at the next tile turns the
    /// lead to face it.
    /// </summary>
    /// <param name="party">The map state of the party.</param>
    /// <param name="target">The target.</param>
    /// <param name="length">The count of steps of the path, or -1 when no path exists.</param>
    /// <returns>The direction of the first step, or no value when the lead stands where it reaches the target, or when no path exists.</returns>
    public static StepDirection? FirstStep(MapState party, WalkTarget target, out int length)
    {
        ArgumentNullException.ThrowIfNull(party);
        ArgumentNullException.ThrowIfNull(target);

        // A search of the tiles in the order of the distance from the lead. Each tile keeps the
        // first step of the path that reached it first, so the order of the directions breaks a
        // tie the same way on every run (T-7).
        Dictionary<TilePoint, StepDirection?> firstSteps = new() { [party.LeadAt] = null };
        Dictionary<TilePoint, int> distances = new() { [party.LeadAt] = 0 };
        Queue<TilePoint> open = new();
        open.Enqueue(party.LeadAt);
        while (open.Count > 0)
        {
            TilePoint at = open.Dequeue();
            if (Reaches(at, target))
            {
                length = distances[at];
                return firstSteps[at];
            }

            foreach (StepDirection direction in Directions)
            {
                TilePoint next = at.Step(direction);
                if (distances.ContainsKey(next) || !Walkable(party, next))
                {
                    continue;
                }

                distances[next] = distances[at] + 1;
                firstSteps[next] = firstSteps[at] ?? direction;
                open.Enqueue(next);
            }
        }

        length = -1;
        return null;
    }

    /// <summary>Gives the direction from a tile to the next tile, or no value when the two tiles do not touch.</summary>
    /// <param name="from">The tile of the lead.</param>
    /// <param name="to">The tile of the target.</param>
    /// <returns>The direction.</returns>
    public static StepDirection? Toward(TilePoint from, TilePoint to)
    {
        foreach (StepDirection direction in Directions)
        {
            if (from.Step(direction) == to)
            {
                return direction;
            }
        }

        return null;
    }

    private static bool Reaches(TilePoint at, WalkTarget target) =>
        target.Reach == TargetReach.StandOn ? at == target.At : Toward(at, target.At) is not null;

    private static bool Walkable(MapState party, TilePoint at) =>
        MapRules.CanEnter(party.Map, at)
        && !party.Npcs.TryNpcAt(at, out _)
        && !party.Patrols.TryEnemyAt(at, out _);
}
