using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Tools.Content;

namespace TheThingBelow.Tests;

/// <summary>
/// The content of this build with a group of up to five characters for the tests of Game: three in
/// the party and one or two for the reserve (D-1136). The shipped content keeps Marrek alone, so the
/// tests alone build a reserve (D-1144).
/// </summary>
/// <remarks>
/// Each added character copies the record of Marrek under its own id. The start party holds the
/// first three, and <see cref="WithReserve"/> puts one or two others in the reserve of a snapshot at
/// their join values, as <see cref="TestParty.StartFour(ulong, Core.Maps.GameMap)"/> does in Core.
/// </remarks>
internal static class ReserveContent
{
    /// <summary>The ids of the added characters, in the order of the fixture: two for the party, then two for the reserve.</summary>
    public static readonly string[] Added = ["character.test_second", "character.test_third", "character.test_fourth", "character.test_fifth"];

    private static readonly Lazy<ContentSet> Loaded = new(Load);

    /// <summary>The content set of this build with the group of five.</summary>
    public static ContentSet Content => Loaded.Value;

    /// <summary>Starts a run of the group on the first map of this build, with three characters in the party and the others in the reserve.</summary>
    /// <param name="seed">The seed of the run.</param>
    /// <param name="count">The count of characters in the reserve: 1 for the group of four of exit test 1, or 2.</param>
    /// <param name="downed">The reserve indexes of the characters who stand down.</param>
    /// <returns>The run at tick zero, with every map of the content to enter.</returns>
    public static Simulation Start(ulong seed, int count, params int[] downed)
    {
        ContentSet content = Content;
        MapSet maps = MapSet.Of(content.Maps);
        RunSnapshot start = Simulation.Start(seed, maps, MapIds.FirstMap, content.Battle, content.Notices, content.Story, DebugIntentHandlers.None).Snapshot();
        return Simulation.Resume(seed, WithReserve(start, count, downed), maps, content.Battle, content.Notices, content.Story, DebugIntentHandlers.None);
    }

    /// <summary>Adds the fourth character, and the fifth for a reserve of two, to the reserve of a snapshot at their join values.</summary>
    /// <param name="snapshot">A snapshot of a run of <see cref="Content"/>, with three characters in the party.</param>
    /// <param name="count">The count of characters in the reserve, 1 or 2.</param>
    /// <param name="downed">The reserve indexes of the characters who stand down, with no health.</param>
    /// <returns>The snapshot with the reserve.</returns>
    public static RunSnapshot WithReserve(RunSnapshot snapshot, int count, params int[] downed)
    {
        if (count < 1 || count > 2)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, "The group holds one or two reserve characters.");
        }

        PartySnapshot party = snapshot.Characters ?? throw new InvalidOperationException("The snapshot holds no party.");
        BattleContent battle = Content.Battle;
        List<CharacterValues> reserve = [];
        for (int index = 0; index < count; index += 1)
        {
            CharacterRecord record = battle.Character(ContentId.Parse(Added[2 + index], "test", "character"));
            StatRow full = record.At(record.JoinLevel);
            int health = Array.IndexOf(downed, index) >= 0 ? 0 : full.Health;
            reserve.Add(new CharacterValues(
                record.Id,
                health,
                record.Row,
                [],
                new GrowthValues(record.JoinLevel, battle.Rules.LevelExperience[record.JoinLevel - 1], full.Mp),
                new LessonValues(new ContentId?[battle.Rules.SlotsAt(record.JoinLevel)], []),
                new ContentId?[GearRules.SlotCount]));
        }

        return snapshot with { Characters = party with { Reserve = reserve } };
    }

    private static ContentSet Load()
    {
        List<ContentFile> files = [.. ContentFolder.Read(RepositoryRoot.Find())];
        Change(files, BattleFixture.Path, fixture =>
        {
            JsonArray characters = fixture["characters"]!.AsArray();
            string marrek = characters[0]!.ToJsonString();
            foreach (string id in Added)
            {
                JsonNode copy = JsonNode.Parse(marrek)!;
                copy["id"] = JsonValue.Create(id);
                characters.Add(copy);
            }

            fixture["start_party"] = new JsonArray(JsonValue.Create("character.marrek"), JsonValue.Create(Added[0]), JsonValue.Create(Added[1]));
        });

        // Each combatant takes one hit effect, and the blood of Marrek serves the added characters too (D-879).
        Change(files, "effects/hits/blood.json", blood =>
        {
            foreach (string id in Added)
            {
                blood["serves"]!.AsArray().Add(JsonValue.Create(id));
            }
        });

        return ContentSet.Load(files);
    }

    /// <summary>Changes the JSON of one content file in the list.</summary>
    private static void Change(List<ContentFile> files, string path, Action<JsonNode> change)
    {
        int place = files.FindIndex(file => string.CompareOrdinal(file.Path, path) == 0);
        if (place < 0)
        {
            throw new InvalidOperationException($"The content holds no file '{path}'.");
        }

        JsonNode root = JsonNode.Parse(files[place].Bytes) ?? throw new InvalidOperationException($"The file '{path}' is empty.");
        change(root);
        files[place] = new ContentFile(path, Encoding.UTF8.GetBytes(root.ToJsonString()));
    }
}
