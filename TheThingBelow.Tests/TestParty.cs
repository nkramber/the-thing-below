using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Story;

namespace TheThingBelow.Tests;

/// <summary>Starts a run of the tests in the room with a changed party, such as a hurt or a down Marrek.</summary>
internal static class TestParty
{
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
        StoryContent story = StoryContent.Load(FlagList.Read(Encoding.UTF8.GetBytes(TestBattles.NoFlagsFile), FlagList.Path), [], battle);
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
}
