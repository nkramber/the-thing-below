using System;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Story;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The services of a hub file: the reader, the host of each service, and each refusal of the
/// load (D-1131, D-1142). Each fault names the file and the service (T-2).
/// </summary>
public sealed class MapServiceTests
{
    [Fact]
    public void AHubReadsAServiceOnAnNpcAndAServiceOnAServicePoint()
    {
        GameMap map = HubMaps.Beds;

        Assert.Equal(MapKind.Hub, map.Kind);
        Assert.Equal(2, map.Services.Count);
        MapService rest = map.Services[0];
        Assert.Equal(("service.hub_rest", ServiceKind.Rest), (rest.Id.Value, rest.Kind));
        Assert.Equal("npc.hub_keeper", rest.Npc?.Value);
        Assert.Null(rest.Thing);
        Assert.Equal("npc.hub_keeper", rest.Host.Value);
        Assert.Equal(ConditionKind.Always, rest.Condition.Kind);
        MapService bed = map.Services[1];
        Assert.Equal(("service.hub_bed_rest", ServiceKind.Rest), (bed.Id.Value, bed.Kind));
        Assert.Null(bed.Npc);
        Assert.Equal("service_point.hub_bed", bed.Thing?.Value);
    }

    [Fact]
    public void AHostGivesItsServiceAndAnyOtherIdGivesNone()
    {
        GameMap map = HubMaps.Beds;

        Assert.Equal("service.hub_rest", map.ServiceOn(Id("npc.hub_keeper"))?.Id.Value);
        Assert.Equal("service.hub_bed_rest", map.ServiceOn(Id("service_point.hub_bed"))?.Id.Value);
        Assert.Null(map.ServiceOn(Id("spawn_point.hub_test_start")));
    }

    [Fact]
    public void AServiceReadsItsCondition()
    {
        // D-543: a story flag can close a service.
        GameMap map = HubMaps.Of(
            npcs: HubMaps.Keeper,
            services: """{ "id": "service.hub_rest", "kind": "rest", "price": 0, "npc": "npc.hub_keeper", "condition": { "not": { "flag": "flag.test_met" } } }""");

        MapService rest = Assert.Single(map.Services);
        Assert.Equal(ConditionKind.Not, rest.Condition.Kind);
    }

    [Fact]
    public void AHubFileThatNamesAnAbsentServiceFailsWithTheFileAndTheService()
    {
        // Exit test 7 of PR-14: the kind names no service of this build.
        ContentException error = Assert.Throws<ContentException>(() => HubMaps.Of(
            npcs: HubMaps.Keeper,
            services: """{ "id": "service.hub_smith", "kind": "smith", "npc": "npc.hub_keeper", "condition": { "always": true } }"""));

        Assert.Contains("hub-test.json", error.Message, StringComparison.Ordinal);
        Assert.Contains("service.hub_smith", error.Message, StringComparison.Ordinal);
        Assert.Contains("the kind 'smith', and a service takes one of rest, shop", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("""{ "id": "service.a", "kind": "rest", "price": 0, "condition": { "always": true } }""", "holds no field 'npc' and no field 'thing'")]
    [InlineData("""{ "id": "service.a", "kind": "rest", "price": 0, "npc": "npc.hub_keeper", "thing": "service_point.hub_bed", "condition": { "always": true } }""", "holds the fields 'npc' and 'thing'")]
    [InlineData("""{ "id": "service.a", "kind": "rest", "price": 0, "npc": "npc.hub_keeper" }""", "condition)")]
    [InlineData("""{ "id": "service.a", "npc": "npc.hub_keeper", "condition": { "always": true } }""", "kind)")]
    [InlineData("""{ "kind": "rest", "price": 0, "npc": "npc.hub_keeper", "condition": { "always": true } }""", "id)")]
    [InlineData("""{ "id": "service.a", "kind": "rest", "price": 0, "npc": "npc.hub_keeper", "cost": 5, "condition": { "always": true } }""", "an unknown field")]
    [InlineData("""{ "id": "service.a", "kind": "rest", "npc": "npc.hub_keeper", "condition": { "always": true } }""", "the field is absent, and the rest 'service.a' needs it")]
    [InlineData("""{ "id": "service.a", "kind": "rest", "price": -1, "npc": "npc.hub_keeper", "condition": { "always": true } }""", "takes the price -1, and a price is 0 or more")]
    [InlineData("""{ "id": "service.a", "kind": "save", "npc": "npc.hub_keeper", "condition": { "always": true } }""", "the kind 'save', and a service takes one of rest, shop")]
    [InlineData("""{ "id": "service.a", "kind": "shop", "npc": "npc.hub_keeper", "condition": { "always": true } }""", "the field is absent, and the shop 'service.a' needs it")]
    [InlineData("""{ "id": "service.a", "kind": "rest", "price": 0, "shop": "shop.test_store", "npc": "npc.hub_keeper", "condition": { "always": true } }""", "the rest 'service.a' takes no field 'shop', which a shop alone reads")]
    [InlineData("""{ "id": "npc.a", "kind": "rest", "price": 0, "npc": "npc.hub_keeper", "condition": { "always": true } }""", "entries of the kind 'service'")]
    [InlineData("""{ "id": "service.a", "kind": "rest", "price": 0, "npc": "npc.hub_cook", "condition": { "always": true } }""", "sits on the NPC 'npc.hub_cook', and this map places no such NPC")]
    [InlineData("""{ "id": "service.a", "kind": "rest", "price": 0, "thing": "service_point.hub_well", "condition": { "always": true } }""", "sits on the thing 'service_point.hub_well', and this map holds no such thing")]
    [InlineData("""{ "id": "service.a", "kind": "rest", "price": 0, "thing": "marker.hub_corner", "condition": { "always": true } }""", "which is a marker, and a thing that holds a service is a service point")]
    [InlineData("""{ "id": "service.a", "kind": "rest", "price": 0, "npc": "npc.hub_keeper", "condition": { "always": true } }, { "id": "service.b", "kind": "rest", "price": 0, "npc": "npc.hub_keeper", "condition": { "always": true } }""", "the services 'service.a' and 'service.b' sit on 'npc.hub_keeper', and one host holds one service")]
    [InlineData("""{ "id": "service.a", "kind": "rest", "price": 0, "npc": "npc.hub_keeper", "condition": { "always": true } }, { "id": "service.a", "kind": "rest", "price": 0, "thing": "service_point.hub_bed", "condition": { "always": true } }""", "two services of this map take the id 'service.a'")]
    public void AMalformedServiceFailsWithTheFileAndTheReason(string services, string reason)
    {
        // The bed of the map holds a rest, so each case adds it beside the services of the case.
        string withBed = services.Contains("service_point.hub_bed", StringComparison.Ordinal) ? services : $"{services}, {HubMaps.RestOnBed}";

        ContentException error = Assert.Throws<ContentException>(
            () => HubMaps.Of(npcs: HubMaps.Keeper, services: withBed, things: $"{HubMaps.Bed}, {HubMaps.Marker}"));

        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
        Assert.Contains("hub-test.json", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AServicePointThatHoldsNoServiceFailsTheLoad()
    {
        // D-1142: a service point with no service is a solid tile that no player can use.
        ContentException error = Assert.Throws<ContentException>(() => HubMaps.Of(things: HubMaps.Bed));

        Assert.Contains("the service point 'service_point.hub_bed' holds no service", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ADungeonThatHoldsAServiceFailsTheLoad()
    {
        // D-1131: a hub alone holds services.
        ContentException error = Assert.Throws<ContentException>(
            () => HubMaps.Of(npcs: HubMaps.Keeper, services: HubMaps.RestOnKeeper, kind: "dungeon"));

        Assert.Contains("the map is a dungeon, and it holds the service 'service.hub_rest'", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AMapWithNoServicesFieldFailsTheLoad()
    {
        string text = HubMaps.Text().Replace(" \"services\": [],\n", string.Empty, StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => TestMaps.Of("no-services.json", text));

        Assert.Contains("services", error.Message, StringComparison.Ordinal);
        Assert.Contains("absent", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(ServiceKind.Rest, "rest")]
    [InlineData(ServiceKind.Shop, "shop")]
    public void EachKindReadsBackItsName(ServiceKind kind, string name)
    {
        Assert.Equal(name, ServiceKinds.NameOf(kind));
        Assert.True(ServiceKinds.TryOf(name, out ServiceKind parsed));
        Assert.Equal(kind, parsed);
    }

    [Fact]
    public void AValueThatNamesNoKindIsAnError()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ServiceKinds.NameOf((ServiceKind)9));
    }

    private static ContentId Id(string value) => ContentId.Parse(value, "test", "id");
}
