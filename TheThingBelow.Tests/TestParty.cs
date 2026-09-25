using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Story;

namespace TheThingBelow.Tests;

/// <summary>
/// Starts a run of the tests in the room with a changed party, such as a hurt or a down Marrek,
/// or with the fixture group of four characters of PR-14: three in the party and the fourth in
/// the reserve (exit test 1, D-362, D-1136). The shipped content keeps Marrek alone, so the tests
/// alone build a reserve (D-1144).
/// </summary>
internal static class TestParty
{
    /// <summary>The id of the fourth character of the test fixture, who waits in the reserve.</summary>
    public static readonly ContentId Fourth = ContentId.Parse("character.test_fourth", "test", "character");

    /// <summary>The battle content of the tests, with the first three characters of the fixture in the party.</summary>
    public static BattleContent FourContent { get; } = TestBattles.WithParty(3);

    /// <summary>Starts a run of the group of four on one map, with the battle content of <see cref="FourContent"/>.</summary>
    /// <param name="seed">The seed of the run.</param>
    /// <param name="map">The map.</param>
    /// <returns>The run at tick zero, with three characters in the party and the fourth in the reserve.</returns>
    public static Simulation StartFour(ulong seed, GameMap map) => StartFour(seed, map, FourContent, (_, stored) => stored);

    /// <summary>
    /// Starts a run of the group of four on one map. The run starts with the three characters of
    /// the content, and a resume of its first snapshot adds the fourth character to the reserve at
    /// its join values: the join level, full health, full MP, no lesson, and no gear (D-363).
    /// </summary>
    /// <param name="seed">The seed of the run.</param>
    /// <param name="map">The map.</param>
    /// <param name="content">The battle content, whose start party holds three characters.</param>
    /// <param name="change">A change of the stored values of each character by its place: 0 to 2 in the party, and 3 for the fourth character in the reserve. A down is one such change.</param>
    /// <returns>The run at tick zero.</returns>
    public static Simulation StartFour(ulong seed, GameMap map, BattleContent content, Func<int, CharacterValues, CharacterValues> change)
    {
        StoryContent story = StoryOf(content);
        RunSnapshot start = Simulation.Start(seed, map, content, TestBattles.Notices, story, DebugIntentHandlers.None).Snapshot();
        PartySnapshot party = start.Characters ?? throw new InvalidOperationException("The start snapshot holds no party.");
        if (party.Characters.Count != BattleFixture.MostCharacters)
        {
            throw new InvalidOperationException($"The content starts a party of {party.Characters.Count} characters, and the group of four needs {BattleFixture.MostCharacters} in the party.");
        }

        CharacterRecord record = content.Character(Fourth);
        StatRow full = record.At(record.JoinLevel);
        var fourth = new CharacterValues(
            Fourth,
            full.Health,
            record.Row,
            [],
            new GrowthValues(record.JoinLevel, content.Rules.LevelExperience[record.JoinLevel - 1], full.Mp),
            new LessonValues(new ContentId?[content.Rules.SlotsAt(record.JoinLevel)], []),
            new ContentId?[GearRules.SlotCount]);
        List<CharacterValues> characters = [];
        for (int place = 0; place < party.Characters.Count; place += 1)
        {
            characters.Add(change(place, party.Characters[place]));
        }

        List<CharacterValues> reserve = [change(BattleFixture.MostCharacters, fourth)];
        return Simulation.Resume(seed, start with { Characters = party with { Characters = characters, Reserve = reserve } }, map, content, TestBattles.Notices, story, DebugIntentHandlers.None);
    }

    /// <summary>Starts a run of the tests in the room, and changes the stored values of Marrek before the resume.</summary>
    /// <param name="seed">The seed of the run.</param>
    /// <param name="change">The change of the stored values of Marrek.</param>
    /// <param name="content">The battle content, or the content of the tests.</param>
    /// <param name="map">The map, or the room with no enemy.</param>
    /// <returns>The run, with the menu closed.</returns>
    public static Simulation Start(ulong seed, Func<CharacterValues, CharacterValues> change, BattleContent? content = null, GameMap? map = null) =>
        StartEach(seed, (slot, stored) => slot == 0 ? change(stored) : stored, content, map);

    /// <summary>Starts a run of the tests, and changes the stored values of each character by its slot before the resume.</summary>
    /// <param name="seed">The seed of the run.</param>
    /// <param name="change">The change of the stored values of the character of each slot.</param>
    /// <param name="content">The battle content, or the content of the tests.</param>
    /// <param name="map">The map, or the room with no enemy.</param>
    /// <returns>The run, with the menu closed.</returns>
    public static Simulation StartEach(ulong seed, Func<int, CharacterValues, CharacterValues> change, BattleContent? content = null, GameMap? map = null)
    {
        BattleContent battle = content ?? TestBattles.Content;
        StoryContent story = StoryOf(battle);
        map ??= TestMaps.Room;
        RunSnapshot start = Simulation.Start(seed, map, battle, TestBattles.Notices, story, DebugIntentHandlers.None).Snapshot();
        PartySnapshot party = start.Characters ?? throw new InvalidOperationException("The start snapshot holds no party.");
        List<CharacterValues> characters = [];
        for (int slot = 0; slot < party.Characters.Count; slot += 1)
        {
            characters.Add(change(slot, party.Characters[slot]));
        }

        return Simulation.Resume(seed, start with { Characters = party with { Characters = characters } }, map, battle, TestBattles.Notices, story, DebugIntentHandlers.None);
    }

    /// <summary>Gives the story content with no story scene, against one battle content.</summary>
    private static StoryContent StoryOf(BattleContent battle) =>
        StoryContent.Load(FlagList.Read(Encoding.UTF8.GetBytes(TestBattles.NoFlagsFile), FlagList.Path), [], battle);
}
