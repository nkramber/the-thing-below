using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The message line of each battle event: every line comes from the string table, and each
/// one holds the limit of 40 characters with the longest names (D-213, D-241, G-7, G-20). The
/// tests read the built Game assembly, because Tests takes no reference to Game (D-614).
/// </summary>
public sealed class BattleMessagesTests
{
    private const string MessagesTypeName = "TheThingBelow.Game.Ui.BattleMessages";

    /// <summary>The limit of a battle message, from the `game-text-style` skill (D-241).</summary>
    private const int MessageLimit = 40;

    private static readonly Lazy<ContentSet> Content =
        new(() => ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find())));

    [Fact]
    public void EveryKindOfEventGivesALineFromTheTableOrNoneForATurn()
    {
        // Exit test 3 of PR-10. det-lint proves that Game shows no inline string (DL 8), and
        // this test proves that each event names an id of the table.
        foreach (BattleEvent played in EveryEvent())
        {
            object? line = LineOf(played, EnemyNamedFirst());
            if (played.Kind == BattleEventKind.Turn)
            {
                Assert.Null(line);
                continue;
            }

            Assert.NotNull(line);
            ContentId id = (ContentId)line!.GetType().GetProperty("Id")!.GetValue(line)!;
            Assert.True(Content.Value.Strings.Contains(id), $"The event '{played.Kind}' names '{id.Value}', which the string table lacks (G-7).");
        }
    }

    [Fact]
    public void EveryLineHoldsFortyCharactersWithTheLongestNames()
    {
        // D-241: a battle message holds 40 characters. The test fills each place with the
        // longest name of the table, the longest status, and the largest stat (D-775).
        StringTable strings = Content.Value.Strings;
        string longestName = Longest(strings, "name.");
        string longestStatus = Longest(strings, "status.");
        string amount = BattleFixture.MostStat.ToString(System.Globalization.CultureInfo.InvariantCulture);

        foreach (BattleEvent played in EveryEvent())
        {
            if (LineOf(played, EnemyNamedFirst()) is not object line)
            {
                continue;
            }

            ContentId id = (ContentId)line.GetType().GetProperty("Id")!.GetValue(line)!;
            string text = strings.Text(id)
                .Replace("{actor}", longestName, StringComparison.Ordinal)
                .Replace("{target}", longestName, StringComparison.Ordinal)
                .Replace("{status}", longestStatus, StringComparison.Ordinal)
                .Replace("{amount}", amount, StringComparison.Ordinal);
            Assert.True(text.Length <= MessageLimit, $"The line '{id.Value}' reads '{text}', {text.Length} characters, above {MessageLimit} (D-241).");
        }
    }

    [Fact]
    public void EveryCombatantAndItemOfTheContentHasAName()
    {
        // A name reads `name.` and the name part of the content id.
        BattleContent battle = Content.Value.Battle;
        var things = new List<ContentId>();
        foreach (CharacterRecord character in battle.Fixture.Characters)
        {
            things.Add(character.Id);
        }

        foreach (EnemyRecord enemy in battle.Enemies)
        {
            things.Add(enemy.Id);
        }

        foreach (ItemRecord item in battle.Fixture.Items)
        {
            things.Add(item.Id);
        }

        foreach (ContentId thing in things)
        {
            ContentId name = (ContentId)Method("NameIdOf").Invoke(null, [thing])!;
            Assert.True(Content.Value.Strings.Contains(name), $"The string table holds no name '{name.Value}' for '{thing.Value}' (G-7).");
        }
    }

    [Fact]
    public void EveryStatusHasANameAndALineOfItsOwn()
    {
        foreach (StatusKind status in Statuses.All)
        {
            ContentId name = (ContentId)Method("StatusIdOf").Invoke(null, [status])!;
            ContentId on = (ContentId)Method("StatusOnIdOf").Invoke(null, [status])!;
            Assert.True(Content.Value.Strings.Contains(name), $"The string table holds no '{name.Value}' (G-7).");
            Assert.True(Content.Value.Strings.Contains(on), $"The string table holds no '{on.Value}' (G-20).");
        }
    }

    [Fact]
    public void AStepLineReadsTheRowThatTheActorReached()
    {
        // D-380: a step to the back row falls back, and a step to the front row steps up.
        Assert.Equal("battle.step_back", ((ContentId)Method("StepIdOf").Invoke(null, [BattleRow.Back])!).Value);
        Assert.Equal("battle.step_front", ((ContentId)Method("StepIdOf").Invoke(null, [BattleRow.Front])!).Value);
    }

    [Fact]
    public void AHitLineNamesAWeakSpotAndAResist()
    {
        // D-794: the affinity of the target names the rate of the hit.
        Assert.Equal("battle.hit_weak", ((ContentId)Method("HitIdOf").Invoke(null, [Affinity.Weak])!).Value);
        Assert.Equal("battle.hit_resist", ((ContentId)Method("HitIdOf").Invoke(null, [Affinity.Resist])!).Value);
        Assert.Equal("battle.hit", ((ContentId)Method("HitIdOf").Invoke(null, [Affinity.Normal])!).Value);
    }

    /// <summary>Gives one event of each kind, each affinity of a hit, each status, and each side of a fall.</summary>
    private static List<BattleEvent> EveryEvent()
    {
        var party = new BattleTarget(BattleSide.Party, 0);
        var enemy = new BattleTarget(BattleSide.Enemy, 0);
        var events = new List<BattleEvent>();
        foreach (BattleEventKind kind in Enum.GetValues<BattleEventKind>())
        {
            events.Add(new BattleEvent(kind, party, enemy, 12, StatusKind.Poison));
            events.Add(new BattleEvent(kind, enemy, party, 12, StatusKind.Poison));
        }

        foreach (Affinity affinity in Elements.Affinities)
        {
            events.Add(new BattleEvent(BattleEventKind.Hit, party, enemy, 12, null, affinity));
        }

        foreach (StatusKind status in Statuses.All)
        {
            foreach (BattleEventKind kind in new[] { BattleEventKind.StatusOn, BattleEventKind.StatusOff, BattleEventKind.Immune, BattleEventKind.StatusHurt })
            {
                events.Add(new BattleEvent(kind, enemy, null, 12, status));
            }
        }

        return events;
    }

    /// <summary>Gives a view of a fight of the fixture content, with one character and the pair.</summary>
    private static object EnemyNamedFirst()
    {
        Simulation run = Simulation.Start(
            20260918,
            BattleRuns.Map("group.fixture_pair"),
            Content.Value.Battle,
            DebugIntentHandlers.None);
        run.Step([Intent.OfPlayer(IntentIds.MoveEast)]);
        return GameAssemblyFile.Type("TheThingBelow.Game.Ui.BattleView").GetMethod("AtStart")!.Invoke(null, [run.State])!;
    }

    private static object? LineOf(BattleEvent played, object view) =>
        Method("Of").Invoke(null, [played, view, Content.Value.Strings]);

    private static MethodInfo Method(string name) =>
        GameAssemblyFile.Type(MessagesTypeName).GetMethod(name, BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException($"The battle messages hold no '{name}' method (T-2).");

    private static string Longest(StringTable strings, string prefix)
    {
        string longest = string.Empty;
        foreach (string id in strings.Ids)
        {
            string text = strings.Text(ContentId.Parse(id, StringTable.Path, "id"));
            if (id.StartsWith(prefix, StringComparison.Ordinal) && text.Length > longest.Length)
            {
                longest = text;
            }
        }

        Assert.NotEmpty(longest);
        return longest;
    }
}
