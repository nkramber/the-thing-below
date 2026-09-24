using System;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Streams;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The sight of the party and of a patrol on a dark map, with the torch held out and put away
/// (D-1062, D-1063). The seed loops are the exit tests 1 to 4 of section 7.35 of
/// `phase-2-first-playable.md`.
/// </summary>
/// <remarks>
/// The dark room is the room of the patrol tests: 10 by 8 tiles with a block of wall in the
/// middle, so each loop meets tiles behind a wall (D-718).
/// </remarks>
public sealed class TorchSightTests
{
    private const int SeedCount = 1000;

    /// <summary>The sequence of the generator of these loops, apart from every stream of a run.</summary>
    private const ulong LoopSequence = 91;

    private static readonly GameMap DarkRoom = PatrolMaps.Of(PatrolMaps.Enemy(sightRange: MapRules.DarkSightRange), "night", dark: true);

    private static readonly GameMap NightRoom = PatrolMaps.Of(PatrolMaps.Enemy(sightRange: MapRules.DarkSightRange), "night");

    [Fact]
    public void ADarkMapGivesThePartyTwoTilesAndSixWithTheTorchHeldOut()
    {
        Assert.Equal(2, MapRules.PartySightRange(DarkRoom, torchHeld: false));
        Assert.Equal(6, MapRules.PartySightRange(DarkRoom, torchHeld: true));
    }

    [Fact]
    public void TheTorchChangesNoRangeOnAMapThatIsNotDark()
    {
        // D-1063: a map that is not dark keeps the range of its time of day (D-720).
        foreach (TimeOfDay time in TimesOfDay.All)
        {
            GameMap map = PatrolMaps.Of(PatrolMaps.Enemy(sightRange: 1), TimesOfDay.NameOf(time));
            Patrol patrol = Assert.Single(map.Patrols);

            Assert.Equal(MapRules.PartySightRange(time), MapRules.PartySightRange(map, torchHeld: false));
            Assert.Equal(MapRules.PartySightRange(time), MapRules.PartySightRange(map, torchHeld: true));
            Assert.Equal(1, MapRules.PatrolSightRange(map, patrol, torchHeld: true));
        }
    }

    [Fact]
    public void AHeldTorchGivesEachPatrolOfADarkMapTheBonusOfTheParty()
    {
        Patrol patrol = Assert.Single(DarkRoom.Patrols);

        Assert.Equal(MapRules.DarkSightRange, MapRules.PatrolSightRange(DarkRoom, patrol, torchHeld: false));
        Assert.Equal(MapRules.DarkSightRange + MapRules.TorchSightBonus, MapRules.PatrolSightRange(DarkRoom, patrol, torchHeld: true));
        Assert.Equal(
            MapRules.PartySightRange(DarkRoom, torchHeld: true) - MapRules.PartySightRange(DarkRoom, torchHeld: false),
            MapRules.PatrolSightRange(DarkRoom, patrol, torchHeld: true) - MapRules.PatrolSightRange(DarkRoom, patrol, torchHeld: false));
    }

    [Fact]
    public void ThePartySeesInsideItsRangeAndAWallStopsIt()
    {
        TilePoint lead = new(2, 3);

        Assert.True(MapSight.PartySees(DarkRoom, lead, 2, new TilePoint(3, 1)));
        Assert.False(MapSight.PartySees(DarkRoom, lead, 2, new TilePoint(2, 6)));
        Assert.True(MapSight.PartySees(DarkRoom, lead, 6, new TilePoint(2, 6)));

        // The block of wall at (4, 3) to (5, 4) stands between the lead and the tile (7, 3).
        Assert.False(MapSight.PartySees(DarkRoom, lead, 6, new TilePoint(7, 3)));
    }

    [Fact]
    public void APartySightOutsideTheMapOrBelowZeroIsAnError()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MapSight.PartySees(DarkRoom, new TilePoint(2, 2), -1, new TilePoint(2, 3)));
        Assert.Throws<ArgumentOutOfRangeException>(() => MapSight.PartySees(DarkRoom, new TilePoint(2, 2), 2, new TilePoint(40, 3)));
    }

    [Fact]
    public void ThePartySightOfADarkMapFollowsTheTorchOverOneThousandSeeds()
    {
        // Exit test 1 of section 7.35 (D-1062, D-1063). A tile that the party sees with the
        // torch put away stays in sight with the torch held out, and a held torch reaches 6 tiles.
        for (ulong seed = 0; seed < SeedCount; seed += 1)
        {
            Pcg32 generator = Pcg32.FromSeed(seed, LoopSequence);
            TilePoint lead = FloorTile(generator, DarkRoom);
            TilePoint other = FloorTile(generator, DarkRoom);
            bool clear = MapSight.Clear(DarkRoom, lead, other);
            int reach = MapSight.Reach(lead, other);

            bool dark = MapSight.PartySees(DarkRoom, lead, MapRules.PartySightRange(DarkRoom, torchHeld: false), other);
            bool lit = MapSight.PartySees(DarkRoom, lead, MapRules.PartySightRange(DarkRoom, torchHeld: true), other);

            Assert.True((clear && reach <= 2) == dark, $"The seed {seed} gave the party with the torch put away a sight of {dark} from {lead} to {other} (D-1063).");
            Assert.True((clear && reach <= 6) == lit, $"The seed {seed} gave the party with the torch held out a sight of {lit} from {lead} to {other} (D-1063).");
            Assert.True(!dark || lit, $"The seed {seed} hid the tile {other} from the torch that the dark showed (D-1063).");
        }
    }

    [Fact]
    public void APatrolOfADarkMapSeesAHeldTorchFourTilesFartherOverOneThousandSeeds()
    {
        // Exit test 2 of section 7.35 (D-1063). The facing quarter and the walls of D-718 stay.
        Patrol patrol = Assert.Single(DarkRoom.Patrols);
        for (ulong seed = 0; seed < SeedCount; seed += 1)
        {
            Pcg32 generator = Pcg32.FromSeed(seed, LoopSequence);
            TilePoint from = FloorTile(generator, DarkRoom);
            TilePoint lead = FloorTile(generator, DarkRoom);
            StepDirection facing = StepDirections.All[(int)(generator.Next() % 4)];

            bool lit = MapSight.PatrolSees(DarkRoom, from, facing, MapRules.PatrolSightRange(DarkRoom, patrol, torchHeld: true), lead);

            Assert.Equal(MapSight.PatrolSees(DarkRoom, from, facing, patrol.SightRange + 4, lead), lit);
            Assert.True(
                !MapSight.PatrolSees(DarkRoom, from, facing, MapRules.PatrolSightRange(DarkRoom, patrol, torchHeld: false), lead) || lit,
                $"The seed {seed} let the patrol at {from} see the party at {lead} in the dark and not by the torch (D-1063).");
        }
    }

    [Fact]
    public void TheTorchChangesNoSightOnAMapThatIsNotDarkOverOneThousandSeeds()
    {
        // Exit test 3 of section 7.35 (D-1063).
        Patrol patrol = Assert.Single(NightRoom.Patrols);
        for (ulong seed = 0; seed < SeedCount; seed += 1)
        {
            Pcg32 generator = Pcg32.FromSeed(seed, LoopSequence);
            TilePoint from = FloorTile(generator, NightRoom);
            TilePoint lead = FloorTile(generator, NightRoom);
            StepDirection facing = StepDirections.All[(int)(generator.Next() % 4)];

            Assert.True(
                MapSight.PatrolSees(NightRoom, from, facing, MapRules.PatrolSightRange(NightRoom, patrol, torchHeld: false), lead) ==
                MapSight.PatrolSees(NightRoom, from, facing, MapRules.PatrolSightRange(NightRoom, patrol, torchHeld: true), lead),
                $"The seed {seed} let the torch change the sight of the patrol at {from} on a map that is not dark (D-1063).");
        }
    }

    [Fact]
    public void EachPatrolThatSeesThePartyIsInsideTheSightOfThePartyOverOneThousandSeeds()
    {
        // Exit test 4 of section 7.35 (D-720, D-1063): the player never loses to a thing off the
        // screen, in each state of the torch.
        Patrol patrol = Assert.Single(DarkRoom.Patrols);
        for (ulong seed = 0; seed < SeedCount; seed += 1)
        {
            Pcg32 generator = Pcg32.FromSeed(seed, LoopSequence);
            TilePoint from = FloorTile(generator, DarkRoom);
            TilePoint lead = FloorTile(generator, DarkRoom);
            StepDirection facing = StepDirections.All[(int)(generator.Next() % 4)];
            bool held = generator.Next() % 2 == 0;

            bool seen = MapSight.PatrolSees(DarkRoom, from, facing, MapRules.PatrolSightRange(DarkRoom, patrol, held), lead);
            bool shown = MapSight.PartySees(DarkRoom, lead, MapRules.PartySightRange(DarkRoom, held), from);

            Assert.True(!seen || shown, $"The seed {seed} let the patrol at {from} see the party at {lead} from outside the sight of the party, with the torch held {held} (D-720).");
        }
    }

    /// <summary>Draws a floor tile of a map, so each loop reads tiles that the party can stand on.</summary>
    private static TilePoint FloorTile(Pcg32 generator, GameMap map)
    {
        while (true)
        {
            TilePoint at = new((int)(generator.Next() % (uint)map.Width), (int)(generator.Next() % (uint)map.Height));
            if (map.TileAt(at) == TileKind.Floor)
            {
                return at;
            }
        }
    }
}
