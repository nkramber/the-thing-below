using System;
using System.Collections.Generic;
using TheThingBelow.Core.Story;

namespace TheThingBelow.Core.Maps;

/// <summary>The numbers of the map: the length of one step, and the sight range of the party.</summary>
/// <remarks>
/// A step takes a fixed count of ticks, and Game slides the sprite across them (D-164,
/// D-203). Thus every step of every direction costs the same, and a replay lands on the same
/// tile at the same tick (T-7, D-716).
/// </remarks>
public static class MapRules
{
    /// <summary>
    /// The count of world ticks that one step of the party takes (D-164, D-203, D-821). The
    /// loop runs 60 ticks a second, so the party walks 3.75 tiles a second.
    /// </summary>
    /// <remarks>
    /// A tile is 32 art pixels, so each tick of a step moves the lead by 2 art pixels, and the
    /// slide shows no uneven tick (D-228, D-821).
    /// </remarks>
    public const int TicksPerStep = 16;

    /// <summary>
    /// The count of world ticks of one step of the party that starts on deep snow or enters it
    /// (D-1233). Double time keeps the even slide: each tick moves the lead by 1 art pixel (D-821).
    /// </summary>
    public const int SnowStepTicks = 32;

    /// <summary>
    /// The count of world ticks between two harms of poison on the map or of bad air (D-1234,
    /// D-1235). The loop runs 60 ticks a second, so each harm comes once a second.
    /// </summary>
    /// <remarks>
    /// The harm comes on each world tick whose count divides by this number, so a snapshot needs
    /// no clock of its own, and a replay harms on the same tick (T-7). An open menu stops the
    /// count of world ticks, so no harm comes under a menu (D-650).
    /// </remarks>
    public const int HarmTicks = 60;

    /// <summary>
    /// The count of steps from the lead within which a trap shows, when a character who fights
    /// and stands carries a Theft drill (D-1228). The count is the steps of a walk around no wall:
    /// the columns apart plus the rows apart (D-716).
    /// </summary>
    public const int TrapShowRange = 2;

    /// <summary>Gives the count of world ticks of one step of the party (D-164, D-1233).</summary>
    /// <param name="map">The map.</param>
    /// <param name="from">The tile that the step starts on.</param>
    /// <param name="direction">The direction of the step.</param>
    /// <returns><see cref="SnowStepTicks"/> when the start tile or the next tile is deep snow, and <see cref="TicksPerStep"/> in every other case.</returns>
    /// <exception cref="ArgumentNullException">The map is null (T-2).</exception>
    /// <remarks>
    /// The count comes from the map and the step alone, so a snapshot holds no length of a step,
    /// and Game reads the same count for its slide (D-203). A next tile off the map counts as no
    /// snow, and no step goes there.
    /// </remarks>
    public static int PartyStepTicks(GameMap map, TilePoint from, StepDirection direction)
    {
        ArgumentNullException.ThrowIfNull(map);

        TilePoint to = from.Step(direction);
        bool snow = (map.Holds(from) && map.TileAt(from) == TileKind.Snow) || (map.Holds(to) && map.TileAt(to) == TileKind.Snow);
        return snow ? SnowStepTicks : TicksPerStep;
    }

    /// <summary>Gives the count of steps between two tiles on a walk around no wall (D-716, D-1228).</summary>
    /// <param name="one">The first tile.</param>
    /// <param name="other">The second tile.</param>
    /// <returns>The columns apart plus the rows apart.</returns>
    /// <exception cref="OverflowException">A count passes the range of an `int` (T-2).</exception>
    public static int StepsApart(TilePoint one, TilePoint other) =>
        checked(Math.Abs(checked(one.X - other.X)) + Math.Abs(checked(one.Y - other.Y)));

    /// <summary>
    /// The counts of world ticks that one step of an enemy can take (D-742, D-821). Each count
    /// divides the tile of 32 art pixels or is a multiple of it, so each tick moves the sprite
    /// by 2 pixels, by 1 pixel, or by 1 pixel every other tick. None is faster than the party.
    /// </summary>
    public static IReadOnlyList<int> EnemyStepTicks { get; } = [16, 32, 64];

    /// <summary>Tells whether an enemy can step in a count of ticks (D-821).</summary>
    /// <param name="ticks">The count of ticks of one step of the enemy.</param>
    /// <returns>True when the count is one of <see cref="EnemyStepTicks"/>.</returns>
    /// <remarks>An NPC steps in the same counts, for the same even slide (D-821, D-1137).</remarks>
    public static bool IsEnemyStep(int ticks)
    {
        foreach (int allowed in EnemyStepTicks)
        {
            if (ticks == allowed)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// The count of world ticks of the mark that a patrol shows before an encounter starts
    /// (D-208, D-745). The loop runs 60 ticks a second, so the mark lasts half a second.
    /// </summary>
    /// <remarks>
    /// Core counts the beat, and Game draws the mark over exactly these ticks. Thus the beat
    /// replays from the seed with no intent, and a bot meets the same map as a player
    /// (D-522, D-745, T-7). Nothing cancels the beat.
    /// </remarks>
    public const int BeatTicks = 30;

    /// <summary>
    /// The count of world ticks after a flee in which no battle with that enemy starts
    /// (D-381, D-748). The loop runs 60 ticks a second, so the grace time lasts 5 seconds.
    /// </summary>
    /// <remarks>
    /// The party walks 3.75 tiles a second, so 5 seconds carry it 18 tiles. That clears the
    /// longest sight of a patrol, because the 12 tiles of the party on a map set to day cap
    /// it (D-720).
    /// </remarks>
    public const int GraceTicks = 300;

    /// <summary>
    /// Gives the sight range of the party on a map of one time of day, in tiles (D-193,
    /// D-720).
    /// </summary>
    /// <param name="time">The time of day of the map (D-442).</param>
    /// <returns>The range, in tiles.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no time (T-2).</exception>
    /// <remarks>
    /// The range decides nothing that Game draws, because every live enemy draws at any
    /// distance (D-814). It sets the ceiling of the sight of each patrol on a map of that
    /// time, so a night map holds patrols that see less far (D-720).
    /// </remarks>
    public static int PartySightRange(TimeOfDay time) => time switch
    {
        TimeOfDay.Dawn => 8,
        TimeOfDay.Day => 12,
        TimeOfDay.Dusk => 8,
        TimeOfDay.Night => 5,
        _ => throw new ArgumentOutOfRangeException(nameof(time), time, "the value names no time of day (D-442)"),
    };

    /// <summary>The sight range of the party on a dark map with the torch put away, in tiles (D-1063).</summary>
    public const int DarkSightRange = 2;

    /// <summary>
    /// The tiles that the torch held out adds on a dark map: to the sight of the party, and the
    /// same tiles to the sight of each patrol (D-1063).
    /// </summary>
    public const int TorchSightBonus = 4;

    /// <summary>Gives the sight range of the party on one map, in tiles (D-720, D-1063).</summary>
    /// <param name="map">The map.</param>
    /// <param name="torchHeld">True while the party holds the torch out (D-1064).</param>
    /// <returns>
    /// On a dark map, 2 tiles, and 6 tiles with the torch held out. On any other map, the range
    /// of its time of day, and the torch changes nothing.
    /// </returns>
    /// <exception cref="ArgumentNullException">The map is null (T-2).</exception>
    public static int PartySightRange(GameMap map, bool torchHeld)
    {
        ArgumentNullException.ThrowIfNull(map);

        if (!map.Dark)
        {
            return PartySightRange(map.Time);
        }

        return torchHeld ? DarkSightRange + TorchSightBonus : DarkSightRange;
    }

    /// <summary>Gives the sight range of one patrol on its map, in tiles (D-718, D-1063).</summary>
    /// <param name="map">The map of the patrol.</param>
    /// <param name="patrol">The patrol.</param>
    /// <param name="torchHeld">True while the party holds the torch out (D-1064).</param>
    /// <returns>The range of the file, plus the torch bonus on a dark map while the party holds the torch out.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <remarks>
    /// The patrol gains what the party gains, so a range at the floor of D-720 stays at or under
    /// the sight of the party at each tick. Each patrol that sees the party is then inside the
    /// sight of the party (D-1063).
    /// </remarks>
    public static int PatrolSightRange(GameMap map, Patrol patrol, bool torchHeld)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(patrol);

        if (map.Dark && torchHeld)
        {
            return checked(patrol.SightRange + TorchSightBonus);
        }

        return patrol.SightRange;
    }

    /// <summary>Tells whether the party can step onto one tile of a map.</summary>
    /// <param name="map">The map.</param>
    /// <param name="at">The tile that the step reaches, which can lie outside the map.</param>
    /// <returns>
    /// True when the tile lies inside the map, its kind takes a step, and it holds no solid
    /// thing.
    /// </returns>
    /// <exception cref="ArgumentNullException">The map is null (T-2).</exception>
    /// <remarks>
    /// A service point, a door, a chest, and a save point are solid, so the lead faces each one
    /// and never walks onto it (D-1142, D-1222). A closed door blocks here, and the lead reads the
    /// open doors through the overload with the memory of the map. PR-64 adds the step onto a trap.
    /// </remarks>
    public static bool CanEnter(GameMap map, TilePoint at)
    {
        ArgumentNullException.ThrowIfNull(map);

        return map.Holds(at) && TileKinds.CanWalk(map.TileAt(at)) && !map.HoldsSolidThing(at);
    }

    /// <summary>Tells whether the lead can step onto one tile, with the doors that the party opened (D-41).</summary>
    /// <param name="map">The map.</param>
    /// <param name="place">The memory of the map, which holds each open door (D-555).</param>
    /// <param name="at">The tile that the step reaches, which can lie outside the map.</param>
    /// <returns>True when <see cref="CanEnter(GameMap, TilePoint)"/> holds, or when the tile holds a door that the party opened.</returns>
    /// <exception cref="ArgumentNullException">The map or the memory is null (T-2).</exception>
    /// <remarks>
    /// A door stands in a doorway, which takes a step, and its lock shares the tile. So an open
    /// door leaves the tile open to the lead. No patrol and no NPC reads this rule, so no enemy
    /// and no NPC walks through a door (D-1142).
    /// </remarks>
    public static bool CanEnter(GameMap map, PlaceState place, TilePoint at)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(place);

        return CanEnter(map, at) || (map.DoorAt(at) is MapThing door && place.IsOpen(door.Id));
    }

    /// <summary>Tells whether the gate on one tile lets the lead pass (D-1243).</summary>
    /// <param name="map">The map.</param>
    /// <param name="flags">The story flags of the run.</param>
    /// <param name="at">The tile, which can lie outside the map.</param>
    /// <returns>True when the tile holds no gate, or when the condition of its gate holds.</returns>
    /// <exception cref="ArgumentNullException">The map or the flags are null (T-2).</exception>
    /// <remarks>
    /// A gate is not a solid thing, so no rule of the map load and no NPC reads the flags. The lead
    /// reads this rule through `MapState` alone (D-543, D-1243).
    /// </remarks>
    public static bool GateOpen(GameMap map, FlagSet flags, TilePoint at)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(flags);

        return map.GateAt(at) is not MapThing { Gate: MapGate gate } || gate.Condition.Holds(flags);
    }

    /// <summary>Tells whether the body of an enemy can stand on one map (D-206, D-209).</summary>
    /// <param name="map">The map.</param>
    /// <param name="body">The body, which can lie outside the map.</param>
    /// <returns>True when every tile of the body lies inside the map and takes a step.</returns>
    /// <exception cref="ArgumentNullException">The map is null (T-2).</exception>
    /// <exception cref="OverflowException">A count passes the range of an `int` (T-2).</exception>
    /// <remarks>
    /// The load of a map reads this rule for each tile of a route and for each anchor tile of
    /// an area, and the walk of an enemy reads it for each step (D-739, D-741).
    /// </remarks>
    public static bool CanPlace(GameMap map, EnemyBody body)
    {
        ArgumentNullException.ThrowIfNull(map);

        int side = body.Side;
        for (int row = 0; row < side; row += 1)
        {
            for (int column = 0; column < side; column += 1)
            {
                var at = new TilePoint(checked(body.Anchor.X + column), checked(body.Anchor.Y + row));
                if (!CanEnter(map, at))
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>Tells whether two bodies hold one tile in common (D-206).</summary>
    /// <param name="one">The first body.</param>
    /// <param name="other">The second body.</param>
    /// <returns>True when the two bodies share a tile.</returns>
    /// <exception cref="OverflowException">A count passes the range of an `int` (T-2).</exception>
    public static bool BodiesOverlap(EnemyBody one, EnemyBody other)
    {
        int side = other.Side;
        for (int row = 0; row < side; row += 1)
        {
            for (int column = 0; column < side; column += 1)
            {
                if (one.Holds(new TilePoint(checked(other.Anchor.X + column), checked(other.Anchor.Y + row))))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
