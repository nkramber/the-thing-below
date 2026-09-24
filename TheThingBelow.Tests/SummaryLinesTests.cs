using System;
using System.Collections;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The lines above each head after a won fight, and the view of a level-up (D-975, D-978,
/// D-979). The tests read the built Game assembly, because Tests takes no reference to Game (D-614).
/// </summary>
public sealed class SummaryLinesTests
{
    private const string ViewTypeName = "TheThingBelow.Game.Ui.BattleView";

    private const string LinesTypeName = "TheThingBelow.Game.Ui.SummaryLines";

    private static readonly Lazy<ContentSet> Content =
        new(() => ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find())));

    [Fact]
    public void ALevelUpShowsTheLevelThenEachStatThatRoseInOrder()
    {
        // D-975, D-979, D-1056: Marrek rises from level 1 to 2, which adds 6 health, 2 MP, 1
        // attack, 1 magic, 1 defense, and 1 resistance, and no speed, so the speed shows no line.
        (object view, List<BattleEvent> played) = FoughtToTheEnd();
        BattleEvent levelUp = played.Find(each => each.Kind == BattleEventKind.LevelUp && each.Actor.Slot == 0)
            ?? throw new InvalidOperationException("The fight gave Marrek no level-up.");

        List<(string Id, string Amount, string Stat)> lines = LinesOf(levelUp, view);

        Assert.Equal(
            [("battle.level_up", "", ""), ("battle.summary_gain", "6", "HP"), ("battle.summary_gain", "2", "MP"), ("battle.summary_gain", "1", "ATK"), ("battle.summary_gain", "1", "MAG"), ("battle.summary_gain", "1", "DEF"), ("battle.summary_gain", "1", "RES")],
            lines);
    }

    [Fact]
    public void AFlatCurveShowsTheLevelLineAlone()
    {
        // D-975: the second character of the tests holds the same stats at each level.
        (object view, List<BattleEvent> played) = FoughtToTheEnd();
        BattleEvent levelUp = played.Find(each => each.Kind == BattleEventKind.LevelUp && each.Actor.Slot == 1)
            ?? throw new InvalidOperationException("The fight gave the second character no level-up.");

        Assert.Equal([("battle.level_up", "", "")], LinesOf(levelUp, view));
    }

    [Fact]
    public void AnExperienceShowsOneLineWithTheAmount()
    {
        (object view, List<BattleEvent> played) = FoughtToTheEnd();
        BattleEvent gained = played.Find(each => each.Kind == BattleEventKind.Experience && each.Actor.Slot == 0)
            ?? throw new InvalidOperationException("The fight gave Marrek no experience.");

        Assert.Equal([("battle.summary_experience", "26", "")], LinesOf(gained, view));
    }

    [Fact]
    public void TheViewOfALevelUpHoldsTheNewLevelFullAndTheValuesBefore()
    {
        // D-973, D-975: the view fills the health and the MP, and keeps the values that the
        // fill of the bars starts from.
        (object view, List<BattleEvent> played) = FoughtToTheEnd();
        object marrek = Party(view)[0];

        Assert.Equal(2, Read<int>(marrek, "Level"));
        Assert.Equal(1, Read<int>(marrek, "LevelBefore"));
        Assert.Equal(TestBattles.MarrekAt(2).Health, Read<int>(marrek, "Health"));
        Assert.Equal(TestBattles.MarrekAt(2).Health, Read<int>(marrek, "FullHealth"));
        Assert.Equal(TestBattles.MarrekAt(2).Mp, Read<int>(marrek, "Mp"));
        Assert.Equal(TestBattles.MarrekAt(2).Mp, Read<int>(marrek, "FullMp"));
        Assert.Equal(TestBattles.MarrekAt(1).Mp, Read<int>(marrek, "MpBefore"));
        Assert.InRange(Read<int>(marrek, "HealthBefore"), 1, TestBattles.MarrekAt(1).Health);
        Assert.Contains(played, each => each.Kind == BattleEventKind.LevelUp);
    }

    /// <summary>Fights the elite group with three characters to a win, and applies each event to a view of the start.</summary>
    private static (object View, List<BattleEvent> Played) FoughtToTheEnd()
    {
        Simulation run = BattleRuns.IntoBattle(5, "group.test_elite", TestBattles.ExactWithParty(3));
        object view = GameAssemblyFile.Type(ViewTypeName).GetMethod("AtStart")!.Invoke(null, [run.State])!;
        List<BattleEvent> played = [.. run.TakeBattleEvents()];
        Assert.Equal(BattleOutcome.Won, BattleRuns.FightToEnd(run, 5));
        played.AddRange(run.TakeBattleEvents());
        foreach (BattleEvent each in played)
        {
            view.GetType().GetMethod("Apply")!.Invoke(view, [each]);
        }

        return (view, played);
    }

    private static List<(string Id, string Amount, string Stat)> LinesOf(BattleEvent played, object view)
    {
        object shown = Party(view)[played.Actor.Slot];
        var lines = new List<(string Id, string Amount, string Stat)>();
        object found = GameAssemblyFile.Type(LinesTypeName).GetMethod("Of")!.Invoke(null, [played, shown, Content.Value.Strings])!;
        foreach (object line in (IEnumerable)found)
        {
            ContentId id = Read<ContentId>(line, "Id");
            var values = Read<IReadOnlyDictionary<string, string>>(line, "Values");
            lines.Add((id.Value, values.GetValueOrDefault("amount", string.Empty), values.GetValueOrDefault("stat", string.Empty)));
        }

        return lines;
    }

    private static List<object> Party(object view)
    {
        var found = new List<object>();
        foreach (object shown in (IEnumerable)view.GetType().GetProperty("Party")!.GetValue(view)!)
        {
            found.Add(shown);
        }

        return found;
    }

    private static T Read<T>(object instance, string name) =>
        (T)(instance.GetType().GetProperty(name)?.GetValue(instance)
            ?? throw new InvalidOperationException($"The Game type '{instance.GetType().Name}' holds no '{name}' (T-2)."));
}
