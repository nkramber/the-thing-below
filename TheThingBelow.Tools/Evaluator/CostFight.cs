using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Tools.Evaluator;

/// <summary>
/// The worst fight of D-961: six enemies on the field against three characters. Each enemy
/// holds a melee strike, a strike of any reach, and a heal, so each turn scores the most
/// legal actions that the rules of PR-11 give (D-955). The command holds its own copy, so a
/// content change never moves the size of the fight. The numbers of the rules come from the
/// checkout.
/// </summary>
public static class CostFight
{
    /// <summary>The group of the worst fight.</summary>
    public const string Group = "group.cost_worst";

    private static readonly string FixtureText = $$"""
    {
     "comment": "The party of the cost fight: three characters.",
     "characters": [
      { "id": "character.cost_first", "row": "front", "join_level": 1, "curve": {{StatCurve.FlatText(new StatRow(90, 20, 12, 4, 100))}} },
      { "id": "character.cost_second", "row": "front", "join_level": 1, "curve": {{StatCurve.FlatText(new StatRow(80, 20, 10, 3, 110))}} },
      { "id": "character.cost_third", "row": "back", "join_level": 1, "curve": {{StatCurve.FlatText(new StatRow(70, 20, 8, 2, 120))}} }
     ],
     "items": [{ "id": "item.cost_draught", "heal": 30, "delay": 100 }],
     "start_party": ["character.cost_first", "character.cost_second", "character.cost_third"],
     "pack": [{ "item": "item.cost_draught", "count": 3 }]
    }
    """;

    private const string EnemyText = """
    {
     "comment": "The enemy of the cost fight, with one move of each kind.",
     "id": "enemy.cost_raider",
     "size": "common",
     "level": 1,
     "experience": 10,
     "health": 40,
     "attack": 8,
     "defense": 2,
     "speed": 90,
     "abilities": ["ability.cost_bash", "ability.cost_shot", "ability.cost_mend"],
     "elements": { "fire": "normal", "ice": "normal", "lightning": "normal", "earth": "normal", "wind": "normal", "water": "normal", "holy": "normal", "dark": "normal" },
     "immune": []
    }
    """;

    private const string AbilityText = """
    {
     "comment": "The moves of the cost fight.",
     "abilities": [
      { "id": "ability.cost_bash", "kind": "strike", "delay": 130, "power": 14000, "element": "none", "reach": "melee" },
      { "id": "ability.cost_shot", "kind": "strike", "delay": 110, "power": 9000, "element": "fire", "reach": "any" },
      { "id": "ability.cost_mend", "kind": "heal", "delay": 110, "heal": 12 }
     ]
    }
    """;

    private const string GroupText = """
    {
     "comment": "The worst group of the cost fight: six on the field.",
     "region": "region.cost",
     "groups": [
      {
       "id": "group.cost_worst",
       "boss": false,
       "enemies": [
        { "enemy": "enemy.cost_raider", "row": "front", "waits": false, "profile": "profile.cost_careful" },
        { "enemy": "enemy.cost_raider", "row": "front", "waits": false, "profile": "profile.cost_careful" },
        { "enemy": "enemy.cost_raider", "row": "front", "waits": false, "profile": "profile.cost_careful" },
        { "enemy": "enemy.cost_raider", "row": "back", "waits": false, "profile": "profile.cost_careful" },
        { "enemy": "enemy.cost_raider", "row": "back", "waits": false, "profile": "profile.cost_careful" },
        { "enemy": "enemy.cost_raider", "row": "back", "waits": false, "profile": "profile.cost_careful" }
       ]
      }
     ]
    }
    """;

    private const string ProfileText = """
    {
     "comment": "The profile of the cost fight, with a weight on each term.",
     "id": "profile.cost_careful",
     "weights": { "damage": 100, "kills": 3, "threat": 60, "healing": 150, "timeline": 1, "row": 200 },
     "steal_chance": 0,
     "steal": []
    }
    """;

    private const string MapText = """
    {
     "comment": "The map of the cost fight: the party one tile west of the guard.",
     "id": "map.cost_fight",
     "region": "region.cost",
     "label": "label.cost_fight",
     "time": "day",
     "terrain": [
      "#######",
      "#.....#",
      "#######"
     ],
     "things": [
      { "id": "spawn_point.cost_fight_start", "kind": "spawn_point", "x": 1, "y": 1 }
     ],
     "enemies": [
      {
       "id": "patrol.cost_fight_guard",
       "group": "group.cost_worst",
       "size": "common",
       "facing": "east",
       "step_ticks": 16,
       "sight_range": 0,
       "routes": [
        { "times": ["dawn", "day", "dusk", "night"], "tiles": [{ "x": 2, "y": 1 }] }
       ]
      }
     ]
    }
    """;

    /// <summary>Builds the battle content of the cost fight, with the rules file of the checkout.</summary>
    /// <param name="root">The root of the checkout.</param>
    /// <returns>The battle content.</returns>
    /// <exception cref="IOException">The rules file of the checkout does not read (T-2).</exception>
    public static BattleContent Content(string root)
    {
        ArgumentException.ThrowIfNullOrEmpty(root);

        string rules = Path.Combine(root, "content", BattleRules.Path);
        return new BattleContent(
            BattleRules.Read(File.ReadAllBytes(rules), rules),
            BattleFixture.Read(Encoding.UTF8.GetBytes(FixtureText), "cost-fixture.json"),
            [EnemyRecord.Read(Encoding.UTF8.GetBytes(EnemyText), "cost-raider.json")],
            AbilityList.Read(Encoding.UTF8.GetBytes(AbilityText), "cost-abilities.json"),
            new List<GroupFile> { GroupFile.Read(Encoding.UTF8.GetBytes(GroupText), $"{GroupFile.Folder}cost.json") },
            [ProfileRecord.Read(Encoding.UTF8.GetBytes(ProfileText), "cost-profile.json")]);
    }

    /// <summary>Gives the map of the cost fight.</summary>
    /// <returns>The map.</returns>
    public static GameMap Map() => GameMap.Read(Encoding.UTF8.GetBytes(MapText), "cost-map.json");
}
