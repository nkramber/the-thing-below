using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Maps;

/// <summary>
/// The stored values of one enemy on the map. A snapshot holds one of these for each enemy
/// of the map, and a load puts each enemy back where it stood (D-259, D-750).
/// </summary>
/// <remarks>
/// A load reads the record of the enemy from the map of this build, and it reads these values
/// from the snapshot. Thus a save never holds the route, the size, or the group of an enemy
/// (D-166, D-495). A save of another build matches each enemy by its id, and an enemy whose
/// stored place its edited station no longer takes starts on that station again (D-1111).
/// <para>
/// The station of the enemy comes from the time of day of the map, so the snapshot holds no
/// station either (D-743).
/// </para>
/// </remarks>
/// <param name="Enemy">The id of the enemy, which the map of this build holds (D-752).</param>
/// <param name="X">The column of the anchor tile of the body (D-737).</param>
/// <param name="Y">The row of the anchor tile of the body.</param>
/// <param name="Facing">The direction that the enemy faces (D-207, D-718).</param>
/// <param name="Stepping">The direction of the step that ran, or no value while it stood.</param>
/// <param name="StepTicks">The count of ticks of that step (D-742).</param>
/// <param name="Target">The index of the route tile that the enemy walks toward (D-739).</param>
/// <param name="Forward">True while the enemy walks up the list of route tiles (D-739).</param>
/// <param name="GraceTicks">The count of grace ticks that remain (D-381, D-748).</param>
/// <param name="Dead">True when the party killed this enemy (D-555).</param>
public sealed record PatrolValues(
    ContentId Enemy,
    int X,
    int Y,
    StepDirection Facing,
    StepDirection? Stepping,
    int StepTicks,
    int Target,
    bool Forward,
    int GraceTicks,
    bool Dead);
