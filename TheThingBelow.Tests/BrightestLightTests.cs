using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Light;
using TheThingBelow.Core.Maps;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The bound of the lit art below the glow threshold (D-910, F-47).</summary>
public sealed class BrightestLightTests
{
    private static readonly FlickerLevel Steady = new(10000, 10000);

    [Fact]
    public void AMapWithNoLightGivesTheAmbientAndTheCarriedLightOnEachTile()
    {
        // D-847: the carried light can stand on any tile, so each tile takes its full strength.
        LitPeak peak = BrightestLight.OnMap([], White(5000), Light(White(10000), 64), Steady, Palette(), 4, 3);

        Assert.Equal(15000, peak.Level);
    }

    [Fact]
    public void ALightGivesItsFullStrengthOnItsOwnTile()
    {
        LitPeak peak = BrightestLight.OnMap([Lamp(80, 80, 20000, 64)], White(0), Light(White(0), 64), Steady, Palette(), 5, 5);

        Assert.Equal(new LitPeak(2, 2, 20000), peak);
    }

    [Fact]
    public void TwoLightsThatOverlapAddUpBetweenThem()
    {
        // F-47: Godot adds each light with no clamp. The lights stand on the tiles 2 and 4 of
        // one row, and the column 3 between them takes 15000 from each: 48 of 64 pixels of reach.
        IReadOnlyList<MapLight> lights = [Lamp(80, 80, 20000, 64), Lamp(144, 80, 20000, 64)];

        LitPeak peak = BrightestLight.OnMap(lights, White(0), Light(White(0), 64), Steady, Palette(), 7, 5);

        Assert.Equal(3, peak.Column);
        Assert.Equal(30000, peak.Level);
    }

    [Fact]
    public void TheStrongestLevelOfAFireRaisesEachLight()
    {
        // D-891: a torch steps up to 120 percent of its file, and the bound takes that level.
        LitPeak peak = BrightestLight.OnMap([Lamp(80, 80, 20000, 64)], White(0), Light(White(0), 64), new FlickerLevel(12000, 10000), Palette(), 5, 5);

        Assert.Equal(24000, peak.Level);
    }

    [Fact]
    public void ALightCountsItsBrightestChannel()
    {
        // The gray of the palette holds 128 of 255 in each channel.
        LitPeak peak = BrightestLight.OnMap([Lamp(80, 80, 20000, 64, 'g')], White(0), Light(White(0), 64), Steady, Palette(), 5, 5);

        Assert.Equal(10040, peak.Level);
    }

    [Fact]
    public void AFightTakesTheAmbientLightAndTheKeyLightAtFullStrength()
    {
        // D-850: 5000 of white and 20000 of gray at 128 of 255, rounded up.
        int level = BrightestLight.InFight(White(5000), Light(new LightColor('g', 20000), 720), Palette());

        Assert.Equal(15040, level);
    }

    [Fact]
    public void TheBoundNeverFallsBelowTheLightOfGodotOnAnyPixel()
    {
        // A seed loop: random lights on a small map. The light of Godot at each pixel is the
        // strength times the light texture of Game, (1 - distance / range) to the power 1.5, at
        // the middle of the pixel (`WorldLights.BuildTexture`). The bound must hold every pixel.
        const int Columns = 6;
        const int Rows = 4;
        for (int seed = 1; seed <= 40; seed += 1)
        {
            var random = new Random(seed);
            var lights = new List<MapLight>();
            int count = random.Next(1, 5);
            for (int index = 0; index < count; index += 1)
            {
                lights.Add(Lamp(random.Next(0, Columns * 32), random.Next(0, Rows * 32), random.Next(1000, 40001), random.Next(8, 200)));
            }

            LitPeak peak = BrightestLight.OnMap(lights, White(0), Light(White(0), 64), Steady, Palette(), Columns, Rows);
            double brightest = BrightestPixelOf(lights, Columns * 32, Rows * 32);

            Assert.True(brightest <= peak.Level, $"seed {seed}: a pixel takes {brightest:0.0} basis points, above the bound {peak.Level} (D-910, F-47)");
        }
    }

    [Fact]
    public void TheCheckoutKeepsItsLitArtBelowTheGlowThreshold()
    {
        // F-47: a bright light on a pale sprite never passes the threshold. The torches of the
        // fixture dungeon and the carried light reach more than full white, so the check reads
        // real light.
        ContentSet set = ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find()));
        ContentId dungeon = ContentId.Parse("map.fixture_dungeon", "test", "id");
        GameMap map = set.Map(dungeon);
        LightSetup setup = set.Light.SetupOf(dungeon, TimeOfDay.Night);

        LitPeak peak = BrightestLight.OnMap(
            set.Light.LightsOf(dungeon, TimeOfDay.Night),
            setup.Ambient,
            set.Light.Carried.Light,
            BrightestLevelOf(set.Light),
            set.Palette,
            map.Width,
            map.Height);

        Assert.True(peak.Level > 10000, $"the brightest lit art is {peak.Level}, and the torches reach more than full white");
        Assert.True(peak.Level < set.Light.Glow.Threshold, $"the brightest lit art is {peak.Level}, at or above the threshold {set.Light.Glow.Threshold} (D-910)");
    }

    /// <summary>Gives the strongest strength and the widest range of every fire of the checkout, as the load reads them (D-891).</summary>
    private static FlickerLevel BrightestLevelOf(LightContent light)
    {
        var fires = new List<TorchFire> { light.Carried.Fire };
        foreach (DecorKind kind in light.Kinds)
        {
            fires.Add(kind.Fire);
        }

        int strength = 0;
        int range = 0;
        foreach (TorchFire fire in fires)
        {
            foreach (FlickerLevel level in fire.Levels)
            {
                strength = Math.Max(strength, level.Strength);
                range = Math.Max(range, level.Range);
            }
        }

        return new FlickerLevel(strength, range);
    }

    /// <summary>Gives the brightest light of Godot on any pixel of the map, with the curve of the light texture of Game.</summary>
    private static double BrightestPixelOf(List<MapLight> lights, int width, int height)
    {
        double brightest = 0;
        for (int y = 0; y < height; y += 1)
        {
            for (int x = 0; x < width; x += 1)
            {
                double level = 0;
                foreach (MapLight light in lights)
                {
                    double dx = x + 0.5 - light.X;
                    double dy = y + 0.5 - light.Y;
                    double edge = Math.Max(0, 1 - (Math.Sqrt((dx * dx) + (dy * dy)) / light.Light.Range));
                    level += light.Light.Color.Strength * Math.Pow(edge, 1.5);
                }

                brightest = Math.Max(brightest, level);
            }
        }

        return brightest;
    }

    private static MapLight Lamp(int x, int y, int strength, int range, char key = 'w') =>
        new(ContentId.Parse("light.lamp", "test", "id"), x, y, Light(new LightColor(key, strength), range));

    private static PointLightValues Light(LightColor color, int range) => new(color, range, 16);

    private static LightColor White(int strength) => new('w', strength);

    /// <summary>A palette of full white and a gray of 128 in each channel.</summary>
    private static Palette Palette() => Core.Content.Palette.Read(
        Encoding.UTF8.GetBytes(
            """
            {
             "comment": "a test palette",
             "colors": [
              { "index": 0, "key": "w", "hex": "ffffff", "name": "white", "height": 0 },
              { "index": 1, "key": "g", "hex": "808080", "name": "gray", "height": 0 }
             ]
            }
            """),
        Core.Content.Palette.Path);
}
