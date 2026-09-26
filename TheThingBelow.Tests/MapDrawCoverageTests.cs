using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// Each thing that a map screen draws has its drawing in the atlas: each patrol, each NPC, and
/// each service point of every map, both drawings of the lead, and each tile kind (D-519,
/// D-1118, D-1137, D-1142).
/// </summary>
/// <remarks>
/// The smoke session builds the first map alone, so a later map with a patrol and no drawing
/// passed CI and failed in play. This test reads every map of the content with no engine.
/// </remarks>
public sealed class MapDrawCoverageTests
{
    /// <summary>The use of a map drawing, which `MapScreen.MapUse` of Game holds (D-519).</summary>
    private const string MapUse = "map_front";

    /// <summary>The use of the drawing of the lead with the torch, which `MapScreen.TorchUse` of Game holds (D-1069).</summary>
    private const string TorchUse = "map_torch";

    /// <summary>The use of the drawing of a thing with one view, which `MapScreen.ThingUse` of Game holds (D-1142).</summary>
    private const string ThingUse = "map";

    /// <summary>The id that the lead draws as, which `MapScreen.LeadContentId` of Game holds.</summary>
    private const string LeadId = "cast.marrek";

    private static readonly Lazy<ContentSet> Content =
        new(() => ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find())));

    [Fact]
    public void EachPatrolOfEachMapHasAMapDrawing()
    {
        List<string> missing = MissingPatrolDrawings(Content.Value.Maps, Content.Value.Atlas);

        Assert.True(missing.Count == 0, $"These patrols have no '{MapUse}' drawing in the atlas: {string.Join(", ", missing)} (D-519).");
    }

    [Fact]
    public void APatrolWithNoDrawingIsFound()
    {
        // The boundary: a map whose patrol id has no drawing fails the check that the test above runs.
        GameMap map = PatrolMaps.Of(PatrolMaps.Enemy(id: "patrol.undrawn"));

        List<string> missing = MissingPatrolDrawings([map], Content.Value.Atlas);

        Assert.Equal(["patrol.undrawn on map.patrol_test"], missing);
    }

    [Fact]
    public void EachNpcOfEachMapHasAMapDrawing()
    {
        List<string> missing = MissingNpcDrawings(Content.Value.Maps, Content.Value.Atlas);

        Assert.True(missing.Count == 0, $"These NPCs have no '{MapUse}' drawing in the atlas: {string.Join(", ", missing)} (D-519, D-1137).");
    }

    [Fact]
    public void AnNpcWithNoDrawingIsFound()
    {
        // The boundary: a map whose NPC id has no drawing fails the check that the test above runs.
        GameMap map = HubMaps.Of(npcs: HubMaps.Wanderer(id: "npc.undrawn"));

        List<string> missing = MissingNpcDrawings([map], Content.Value.Atlas);

        Assert.Equal(["npc.undrawn on map.hub_test"], missing);
    }

    [Fact]
    public void EachServicePointOfEachMapHasAMapDrawing()
    {
        List<string> missing = MissingPointDrawings(Content.Value.Maps, Content.Value.Atlas);

        Assert.True(missing.Count == 0, $"These service points have no '{ThingUse}' drawing in the atlas: {string.Join(", ", missing)} (D-519, D-1142).");
    }

    [Fact]
    public void AServicePointWithNoDrawingIsFound()
    {
        // The boundary: the bed of the test hub has no drawing, and the check finds it.
        GameMap map = HubMaps.Inn;

        List<string> missing = MissingPointDrawings([map], Content.Value.Atlas);

        Assert.Equal(["service_point.hub_bed on map.hub_test"], missing);
    }

    [Fact]
    public void TheShippedContentHoldsAnNpcAndAServicePoint()
    {
        // The two checks above read every map, so a content set with no NPC and no service point
        // would pass them with nothing to read. The fixture hub holds both (exit test 18 of PR-14).
        int npcs = 0;
        int points = 0;
        foreach (GameMap map in Content.Value.Maps)
        {
            npcs += map.Npcs.Count;
            points += ServicePointsOf(map).Count;
        }

        Assert.True(npcs > 0 && points > 0, $"The content holds {npcs} NPCs and {points} service points (D-1133).");
    }

    [Fact]
    public void TheLeadHasAPlainDrawingAndATorchDrawingOfOneSize()
    {
        AtlasIndex atlas = Content.Value.Atlas;
        ContentId lead = ContentId.Parse(LeadId, "test", "lead");

        AtlasEntry plain = atlas.Entry(lead, MapUse);
        AtlasEntry torch = atlas.Entry(lead, TorchUse);

        Assert.Equal((plain.Width, plain.Height), (torch.Width, torch.Height));
    }

    [Fact]
    public void EachTileKindHasATileDrawing()
    {
        foreach (TileKind kind in Enum.GetValues<TileKind>())
        {
            Assert.True(
                Content.Value.Atlas.Draws(TileIds.Of(kind), TileIds.MapUse),
                $"The tile kind '{kind}' has no '{TileIds.MapUse}' drawing in the atlas (D-519, D-667).");
        }
    }

    private static List<string> MissingPatrolDrawings(IEnumerable<GameMap> maps, AtlasIndex atlas)
    {
        List<string> missing = [];
        foreach (GameMap map in maps)
        {
            foreach (Patrol patrol in map.Patrols)
            {
                if (!atlas.Draws(patrol.Id, MapUse))
                {
                    missing.Add($"{patrol.Id.Value} on {map.Id.Value}");
                }
            }
        }

        return missing;
    }

    private static List<string> MissingNpcDrawings(IEnumerable<GameMap> maps, AtlasIndex atlas)
    {
        List<string> missing = [];
        foreach (GameMap map in maps)
        {
            foreach (Npc npc in map.Npcs)
            {
                if (!atlas.Draws(npc.Id, MapUse))
                {
                    missing.Add($"{npc.Id.Value} on {map.Id.Value}");
                }
            }
        }

        return missing;
    }

    private static List<string> MissingPointDrawings(IEnumerable<GameMap> maps, AtlasIndex atlas)
    {
        List<string> missing = [];
        foreach (GameMap map in maps)
        {
            foreach (MapThing point in ServicePointsOf(map))
            {
                if (!atlas.Draws(point.Id, ThingUse))
                {
                    missing.Add($"{point.Id.Value} on {map.Id.Value}");
                }
            }
        }

        return missing;
    }

    private static List<MapThing> ServicePointsOf(GameMap map)
    {
        List<MapThing> points = [];
        foreach (MapThing thing in map.Things)
        {
            if (thing.Kind == MapThingKind.ServicePoint)
            {
                points.Add(thing);
            }
        }

        return points;
    }
}
