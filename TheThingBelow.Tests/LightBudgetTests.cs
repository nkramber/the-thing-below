using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Light;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The count of lights in the worst view and on one canvas item (D-842, F-46).</summary>
public sealed class LightBudgetTests
{
    [Fact]
    public void ALightReachesAWindowWhenTheSquareOfItsRangeMeetsIt()
    {
        List<MapLight> lights = [Light(100, 100, 10)];

        Assert.Equal(1, LightBudget.CountReaching(lights, 110, 100, 5, 5));
        Assert.Equal(0, LightBudget.CountReaching(lights, 111, 100, 5, 5));
        Assert.Equal(1, LightBudget.CountReaching(lights, 86, 86, 5, 5));
        Assert.Equal(0, LightBudget.CountReaching(lights, 85, 85, 5, 5));
    }

    [Fact]
    public void TheWorstViewFindsTheClusterOfLights()
    {
        // Three lights sit together at the east of a wide map, and one sits alone at the west.
        List<MapLight> lights = [Light(20, 20, 8), Light(1500, 100, 8), Light(1520, 120, 8), Light(1540, 140, 8)];

        LightCount worst = LightBudget.WorstWindow(lights, 1600, 400, LightBudget.ViewWidth, LightBudget.ViewHeight);

        Assert.Equal(3, worst.Count);
        Assert.Equal(3, LightBudget.CountReaching(lights, worst.Left, worst.Top, LightBudget.ViewWidth, LightBudget.ViewHeight));
    }

    [Fact]
    public void AMapSmallerThanTheViewCountsEveryLight()
    {
        List<MapLight> lights = [Light(5, 5, 4), Light(300, 200, 4)];

        LightCount worst = LightBudget.WorstWindow(lights, 320, 240, LightBudget.ViewWidth, LightBudget.ViewHeight);

        Assert.Equal(new LightCount(0, 0, 2), worst);
    }

    [Fact]
    public void TheWorstWindowMatchesAWalkOfEveryPlace()
    {
        // A seed loop: the tried places give the same highest count as a walk of each place.
        for (int seed = 0; seed < 200; seed += 1)
        {
            var random = new Random(seed);
            int mapWidth = random.Next(40, 160);
            int mapHeight = random.Next(40, 120);
            int width = random.Next(8, 48);
            int height = random.Next(8, 48);
            var lights = new List<MapLight>();
            int count = random.Next(0, 12);
            for (int index = 0; index < count; index += 1)
            {
                lights.Add(Light(random.Next(0, mapWidth), random.Next(0, mapHeight), random.Next(1, 20)));
            }

            LightCount found = LightBudget.WorstWindow(lights, mapWidth, mapHeight, width, height);

            int walked = 0;
            for (int left = 0; left <= Math.Max(0, mapWidth - width); left += 1)
            {
                for (int top = 0; top <= Math.Max(0, mapHeight - height); top += 1)
                {
                    walked = Math.Max(walked, LightBudget.CountReaching(lights, left, top, width, height));
                }
            }

            Assert.True(found.Count == walked, $"seed {seed}: the tried places gave {found.Count}, and the walk gave {walked}");
        }
    }

    [Fact]
    public void TheWorstQuadrantCountsEachQuadrantOfSixteenTiles()
    {
        const int Side = LightBudget.QuadrantTiles * AtlasPages.TileSize;
        List<MapLight> lights = [Light(10, 10, 4), Light(Side + 10, 10, 4), Light(Side + 20, 20, 4)];

        LightCount worst = LightBudget.WorstQuadrant(lights, 2 * Side, Side);

        Assert.Equal(new LightCount(Side, 0, 2), worst);
    }

    private static MapLight Light(int x, int y, int range) =>
        new(ContentId.Parse($"light.at_{x}_{y}", "test", "id"), x, y, new PointLightValues(new LightColor('j', 10000), range, 16));
}
