using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The command menu of the battle screen: the action, then the item or the target (D-827).
/// Each move of the cursor stays in Game, and a whole choice makes one intent (D-493). The
/// tests read the built Game assembly, because Tests takes no reference to Game (D-614).
/// </summary>
public sealed class BattleCommandsTests
{
    private const string CommandsTypeName = "TheThingBelow.Game.Ui.BattleCommands";

    [Fact]
    public void TheMenuOpensOnTheAttackAndTheCursorWrapsAtEachEnd()
    {
        object menu = Open(OnFirstCommand());

        Assert.Equal("Action", Read(menu, "Stage").ToString());
        Assert.Equal(0, (int)Read(menu, "Cursor"));
        Move(menu, -1);
        Assert.Equal(4, (int)Read(menu, "Cursor"));
        Move(menu, 1);
        Assert.Equal(0, (int)Read(menu, "Cursor"));
    }

    [Fact]
    public void AMoveOfTheCursorMakesNoIntent()
    {
        // D-493: the record holds the choice alone. The move methods give nothing back, and a
        // confirm on the attack opens the targets and gives no intent yet.
        object menu = Open(OnFirstCommand());

        Assert.Null(Confirm(menu));
        Assert.Equal("Target", Read(menu, "Stage").ToString());
    }

    [Fact]
    public void AnAttackTargetsEachEnemyThatMeleeReachesAndSendsOneIntent()
    {
        // D-377: the menu offers the targets that the rules take.
        Simulation run = OnFirstCommand();
        object menu = Open(run);
        Confirm(menu);

        IReadOnlyList<Combatant> reached = BattleRuns.BattleOf(run).MeleeTargets(BattleSide.Enemy);
        Assert.Equal(reached.Count, Targets(menu).Count);
        Move(menu, 1);
        Intent made = Confirm(menu) ?? throw new InvalidOperationException("The menu sent no intent.");

        Assert.Equal(IntentIds.BattleAttack.Value, made.Action.Value);
        Assert.Equal(reached[1 % reached.Count].Target, made.Target);
        Assert.Null(made.Item);
    }

    [Fact]
    public void ACancelOnTheTargetsGoesBackToTheAttack()
    {
        object menu = Open(OnFirstCommand());
        Confirm(menu);

        Cancel(menu);

        Assert.Equal("Action", Read(menu, "Stage").ToString());
        Assert.Equal(0, (int)Read(menu, "Cursor"));
    }

    [Theory]
    [InlineData(1, "intent.battle_defend")]
    [InlineData(2, "intent.battle_step")]
    [InlineData(4, "intent.battle_flee")]
    public void AnActionWithNoTargetSendsItsIntentAtOnce(int cursor, string intent)
    {
        object menu = Open(OnFirstCommand());
        for (int step = 0; step < cursor; step += 1)
        {
            Move(menu, 1);
        }

        Intent made = Confirm(menu) ?? throw new InvalidOperationException("The menu sent no intent.");

        Assert.Equal(intent, made.Action.Value);
        Assert.Null(made.Target);
    }

    [Fact]
    public void AnItemTakesTheItemThenACharacter()
    {
        // D-382, D-780: the item stage lists the pack, and the target stage lists the party.
        Simulation run = OnFirstCommand();
        object menu = Open(run);
        for (int step = 0; step < 3; step += 1)
        {
            Move(menu, 1);
        }

        Assert.Null(Confirm(menu));
        Assert.Equal("Item", Read(menu, "Stage").ToString());
        PackValues first = run.State.Characters.Pack[0];
        IList items = (IList)Read(menu, "Items");
        Assert.Equal(first, items[0]);

        Assert.Null(Confirm(menu));
        Intent made = Confirm(menu) ?? throw new InvalidOperationException("The menu sent no intent.");

        Assert.Equal(IntentIds.BattleItem.Value, made.Action.Value);
        Assert.Equal(new BattleTarget(BattleSide.Party, 0), made.Target);
        Assert.Equal(first.Item, made.Item);
    }

    [Fact]
    public void EveryIntentOfTheMenuPassesTheRules()
    {
        // D-532: the menu reads each choice from the rules, so the rules never refuse one.
        Simulation run = OnFirstCommand();
        for (int cursor = 0; cursor < 5; cursor += 1)
        {
            object menu = Open(run);
            for (int step = 0; step < cursor; step += 1)
            {
                Move(menu, 1);
            }

            Intent? made = null;
            for (int press = 0; press < 3 && made is null; press += 1)
            {
                made = Confirm(menu);
            }

            Assert.NotNull(made);
            var choice = new BattleChoice(ActionOf(made!.Action), made.Target, made.Item);
            Assert.Null(BattleTurns.RefusalOf(run.State, choice));
        }
    }

    [Fact]
    public void TheMenuOfAFightThatEndedIsAnError()
    {
        // T-2: a menu with no fight to command is an error, never an empty menu. The last
        // character to act can still stand first in the order after the win.
        Simulation run = OnFirstCommand();
        BattleRuns.FightToEnd(run, 11);

        TargetInvocationException thrown = Assert.Throws<TargetInvocationException>(() => Open(run));
        Assert.IsType<InvalidOperationException>(thrown.InnerException);
    }

    private static BattleAction ActionOf(ContentId intent) => intent.Value switch
    {
        "intent.battle_attack" => BattleAction.Attack,
        "intent.battle_defend" => BattleAction.Defend,
        "intent.battle_step" => BattleAction.Step,
        "intent.battle_item" => BattleAction.Item,
        "intent.battle_flee" => BattleAction.Flee,
        _ => throw new InvalidOperationException($"The menu sent '{intent.Value}', which names no battle action."),
    };

    /// <summary>Gives a run of the test fixture at the first command of a character (D-532).</summary>
    private static Simulation OnFirstCommand()
    {
        Simulation run = BattleRuns.IntoBattle(11, "group.fixture_pair");
        run.TakeBattleEvents();
        Assert.Equal(BattleSide.Party, BattleRuns.BattleOf(run).Next()?.Side);
        return run;
    }

    private static object Open(Simulation run) =>
        Type().GetMethod("Open")!.Invoke(null, [run.State])!;

    private static void Move(object menu, int step) => Type().GetMethod("Move")!.Invoke(menu, [step]);

    private static Intent? Confirm(object menu) => (Intent?)Type().GetMethod("Confirm")!.Invoke(menu, null);

    private static void Cancel(object menu) => Type().GetMethod("Cancel")!.Invoke(menu, null);

    private static IList Targets(object menu) => (IList)Read(menu, "Targets");

    private static object Read(object menu, string name) =>
        Type().GetProperty(name)!.GetValue(menu)
            ?? throw new InvalidOperationException($"The menu gave no value for '{name}' (T-2).");

    private static Type Type() => GameAssemblyFile.Type(CommandsTypeName);
}
