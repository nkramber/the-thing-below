using System;
using System.Collections.Generic;
using System.Reflection;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// Proves that a bot makes the same intents that Game makes (D-493, PR-15 exit test 5). The bots
/// pick from the query of Core, so each test compares that list with the intents that a screen of
/// the built Game assembly makes from the same state.
/// </summary>
public sealed class BotIntentParityTests
{
    private const string CommandsTypeName = "TheThingBelow.Game.Ui.BattleCommands";

    private const string InputTypeName = "TheThingBelow.Game.Ui.InputActions";

    /// <summary>The most confirms of one path through the command menu: action, item or lesson, form, target.</summary>
    private const int MostDepth = 4;

    [Theory]
    [InlineData(11UL, "group.fixture_pair")]
    [InlineData(12UL, "group.fixture_pair")]
    [InlineData(13UL, "group.fixture_pair")]
    public void TheCommandMenuMakesEachBattleIntentOfTheList(ulong seed, string group)
    {
        // Each battle command in the list is an intent that the command menu of Game can send,
        // and the menu sends no other. The open of the pause of a fight comes from the menu
        // button, not from the command menu (D-162).
        Simulation run = BattleRuns.IntoBattle(seed, group);
        run.TakeBattleEvents();
        SortedSet<string> fromGame = new(StringComparer.Ordinal);
        Explore(run, [], fromGame);
        Assert.True(fromGame.Count >= 4, $"Seed {seed}: the command menu made {fromGame.Count} intents, and a fight offers an attack, a defend, a step, and a flee at least.");

        SortedSet<string> fromCore = new(StringComparer.Ordinal);
        foreach (Intent intent in run.Accepted())
        {
            if (string.CompareOrdinal(intent.Action.Value, IntentIds.OpenMenu.Value) != 0)
            {
                fromCore.Add(intent.Describe());
            }
        }

        Assert.Equal(fromGame, fromCore);
    }

    [Fact]
    public void TheInputOfTheWalkMakesEachWalkIntentOfTheList()
    {
        // Game makes each walk intent from an input action. The cancel reaches no rule, and
        // Game sends none (D-493), so the list holds the other actions alone.
        Simulation run = BattleRuns.IntoBattle(11, "group.fixture_pair");
        Simulation walk = WalkOf(run);
        MethodInfo intentOf = GameAssemblyFile.Type(InputTypeName).GetMethod("IntentOf")!;
        IReadOnlyList<string> names = (IReadOnlyList<string>)GameAssemblyFile.Type(InputTypeName).GetProperty("Names")!.GetValue(null)!;
        SortedSet<string> fromGame = new(StringComparer.Ordinal);
        foreach (string name in names)
        {
            ContentId action = (ContentId)intentOf.Invoke(null, [name, false, walk.State.Characters.TorchHeld])!;
            bool torch = string.CompareOrdinal(action.Value, IntentIds.HoldTorch.Value) == 0 || string.CompareOrdinal(action.Value, IntentIds.PutTorchAway.Value) == 0;
            if (string.CompareOrdinal(action.Value, IntentIds.Cancel.Value) != 0 && (!torch || walk.State.Characters.CountOf(TorchRules.Torch) > 0))
            {
                fromGame.Add(Intent.OfPlayer(action).Describe());
            }
        }

        SortedSet<string> fromCore = new(StringComparer.Ordinal);
        foreach (Intent intent in walk.Accepted())
        {
            fromCore.Add(intent.Describe());
        }

        Assert.Equal(fromGame, fromCore);
    }

    /// <summary>Walks each path of cursor positions through a fresh command menu, and keeps each intent that a confirm sends.</summary>
    private static void Explore(Simulation run, List<int> path, SortedSet<string> found)
    {
        object menu = Open(run);
        foreach (int entry in path)
        {
            Point(menu, entry);
            if (Confirm(menu) is Intent sent)
            {
                found.Add(sent.Describe());
                return;
            }
        }

        if (path.Count >= MostDepth)
        {
            throw new InvalidOperationException($"The command menu went deeper than {MostDepth} confirms on the path {string.Join(",", path)} (T-2).");
        }

        string stage = Read(menu, "Stage").ToString()!;
        int count = (int)Read(menu, "Count");
        for (int entry = 0; entry < count; entry += 1)
        {
            // A confirm on an action with no choice keeps the stage, and the path ends there.
            object probe = Open(run);
            foreach (int earlier in path)
            {
                Point(probe, earlier);
                _ = Confirm(probe);
            }

            Point(probe, entry);
            if (Confirm(probe) is Intent sent)
            {
                found.Add(sent.Describe());
                continue;
            }

            if (string.CompareOrdinal(Read(probe, "Stage").ToString(), stage) != 0)
            {
                Explore(run, [.. path, entry], found);
            }
        }
    }

    /// <summary>Gives a run of the fixture on the walk, after the fight of <paramref name="run"/> ends.</summary>
    private static Simulation WalkOf(Simulation run)
    {
        BattleOutcome outcome = BattleRuns.FightToEnd(run, 11);
        Assert.Equal(BattleOutcome.Won, outcome);
        run.Step([Intent.OfPlayer(IntentIds.WaitBattleEnd)]);
        Assert.Null(run.State.Battle);
        return run;
    }

    private static object Open(Simulation run) =>
        GameAssemblyFile.Type(CommandsTypeName).GetMethod("Open")!.Invoke(null, [run.State, Memory()])!;

    private static object Memory() =>
        Activator.CreateInstance(GameAssemblyFile.Type("TheThingBelow.Game.Ui.CommandMemory"), [false])!;

    /// <summary>Moves the cursor down until it stands on the entry. The cursor wraps at the end.</summary>
    private static void Point(object menu, int entry)
    {
        int count = (int)Read(menu, "Count");
        for (int step = 0; step < count && (int)Read(menu, "Cursor") != entry; step += 1)
        {
            GameAssemblyFile.Type(CommandsTypeName).GetMethod("Move")!.Invoke(menu, [1]);
        }

        Assert.Equal(entry, (int)Read(menu, "Cursor"));
    }

    private static Intent? Confirm(object menu) =>
        (Intent?)GameAssemblyFile.Type(CommandsTypeName).GetMethod("Confirm")!.Invoke(menu, null);

    private static object Read(object menu, string name) =>
        GameAssemblyFile.Type(CommandsTypeName).GetProperty(name)!.GetValue(menu)
            ?? throw new InvalidOperationException($"The menu gave no value for '{name}' (T-2).");
}
