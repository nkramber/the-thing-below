using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Maps;

/// <summary>
/// The stored values of one NPC on the map. A snapshot of save format 15 or later holds one of
/// these for each NPC of the map, and a load puts each NPC back where it stood (D-259, D-1137).
/// </summary>
/// <remarks>
/// A load reads the record of the NPC from the map of this build, and it reads these values
/// from the snapshot. Thus a save never holds the move, the range, or the route of an NPC
/// (D-166, D-495). A save of another build matches each NPC by its id, as it matches each
/// enemy (D-1111).
/// </remarks>
/// <param name="Npc">The id of the NPC, which the map of this build holds (D-166).</param>
/// <param name="X">The column of the tile of the NPC (D-203).</param>
/// <param name="Y">The row of the tile of the NPC.</param>
/// <param name="Facing">The direction that the NPC faces (D-207).</param>
/// <param name="Stepping">The direction of the step that ran, or no value while it stood.</param>
/// <param name="StepTicks">The count of ticks of that step (D-821).</param>
/// <param name="Target">The index of the route tile that a route NPC walks toward (D-739).</param>
/// <param name="Forward">True while a route NPC walks up the list of route tiles (D-739).</param>
/// <param name="WaitTicks">The count of world ticks before the next choice or the next leg (D-1138).</param>
public sealed record NpcValues(
    ContentId Npc,
    int X,
    int Y,
    StepDirection Facing,
    StepDirection? Stepping,
    int StepTicks,
    int Target,
    bool Forward,
    int WaitTicks);
