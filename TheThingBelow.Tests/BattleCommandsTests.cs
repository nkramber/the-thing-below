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
/// The command menu of the battle screen: the action, then the lesson and its form or the item,
/// then the target (D-827, D-1027, D-1031).
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
        Assert.Equal(5, (int)Read(menu, "Cursor"));
        Move(menu, 1);
        Assert.Equal(0, (int)Read(menu, "Cursor"));
    }

    [Fact]
    public void TheRememberedCursorOpensOnTheLastActionOfTheCharacter()
    {
        // D-226: with the setting on, the menu opens on the last action of the character.
        Simulation run = OnFirstCommand();
        object first = Open(run);
        object memory = Memory(enabled: true);
        memory.GetType().GetMethod("Keep")!.Invoke(memory, [Read(first, "Actor"), BattleAction.Defend]);

        object menu = Type().GetMethod("Open")!.Invoke(null, [run.State, memory])!;

        Assert.Equal(2, (int)Read(menu, "Cursor"));
        Assert.Equal(BattleAction.Defend, (BattleAction)Read(menu, "Action"));
    }

    [Fact]
    public void WithTheSettingOffTheMenuOpensOnTheAttack()
    {
        // D-868: the remembered cursor starts off, and the memory keeps the action for a later switch.
        Simulation run = OnFirstCommand();
        object memory = Memory(enabled: false);
        memory.GetType().GetMethod("Keep")!.Invoke(memory, [Read(Open(run), "Actor"), BattleAction.Flee]);

        object menu = Type().GetMethod("Open")!.Invoke(null, [run.State, memory])!;

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
    public void TheTargetsOfAnAttackHoldNoWaitingEnemy()
    {
        // Exit test 4 of PR-98: a waiting enemy stands in the column, and it is not a target (D-954).
        Simulation run = BattleRuns.IntoBattle(11, "group.fixture_elite");
        run.TakeBattleEvents();
        Battle battle = BattleRuns.BattleOf(run);
        Assert.Equal(BattleSide.Party, battle.Next()?.Side);
        Assert.Contains(battle.Enemies, enemy => enemy.Place == CombatantPlace.Waiting);
        object menu = Open(run);
        Confirm(menu);

        Assert.NotEmpty(Targets(menu));
        foreach (object target in Targets(menu))
        {
            Assert.Equal(CombatantPlace.Field, battle.Enemies[((BattleTarget)target).Slot].Place);
        }
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
    [InlineData(2, "intent.battle_defend")]
    [InlineData(3, "intent.battle_step")]
    [InlineData(5, "intent.battle_flee")]
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
        for (int step = 0; step < 4; step += 1)
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
        Assert.Equal(first.Id, made.Item);
    }

    [Fact]
    public void EveryIntentOfTheMenuPassesTheRules()
    {
        // D-532: the menu reads each choice from the rules, so the rules never refuse one.
        Simulation run = OnFirstCommand();
        for (int cursor = 0; cursor < 6; cursor += 1)
        {
            object menu = Open(run);
            for (int step = 0; step < cursor; step += 1)
            {
                Move(menu, 1);
            }

            Intent? made = null;
            for (int press = 0; press < 4 && made is null; press += 1)
            {
                made = Confirm(menu);
            }

            Assert.NotNull(made);
            var choice = new BattleChoice(ActionOf(made!.Action), made.Target, made.Item, made.Lesson, made.Lesson is null ? null : made.Option);
            Assert.Null(BattleTurns.RefusalOf(run.State, choice));
        }
    }

    [Fact]
    public void UpAndDownMoveBetweenTheTwoRowsInOneColumnAndWrap()
    {
        // D-1034: the six commands stand in two rows of three.
        object menu = Open(OnFirstCommand());

        MoveRow(menu, 1);
        Assert.Equal(BattleAction.Step, ActionAt((int)Read(menu, "Cursor")));
        MoveRow(menu, 1);
        Assert.Equal(0, (int)Read(menu, "Cursor"));
        Move(menu, 1);
        MoveRow(menu, -1);
        Assert.Equal(BattleAction.Item, ActionAt((int)Read(menu, "Cursor")));
    }

    [Fact]
    public void ALessonTakesTheLessonThenTheFormThenATarget()
    {
        // D-1027, D-1031: the Lessons command sits after the attack, and it lists each lesson of
        // the slots, then each opened form of the lesson with its cost.
        Simulation run = OnFirstCommand();
        object menu = Open(run);
        Move(menu, 1);
        Assert.Equal(BattleAction.Lesson, ActionAt(1));

        Assert.Null(Confirm(menu));
        Assert.Equal("Lesson", Read(menu, "Stage").ToString());
        Assert.Equal(["lesson.fixture_hew", "lesson.fixture_cinder"], Values((IList)Read(menu, "Lessons")));

        Move(menu, 1);
        Assert.Null(Confirm(menu));
        Assert.Equal("Form", Read(menu, "Stage").ToString());
        IList forms = (IList)Read(menu, "Forms");
        Assert.Single(forms);
        Assert.Equal(4, ((LessonForm)forms[0]!).Mp);

        Assert.Null(Confirm(menu));
        Assert.Equal("Target", Read(menu, "Stage").ToString());
        Intent made = Confirm(menu) ?? throw new InvalidOperationException("The menu sent no intent.");

        Assert.Equal(IntentIds.BattleLesson.Value, made.Action.Value);
        Assert.Equal(("lesson.fixture_cinder", 0), (made.Lesson?.Value, made.Option));
        Assert.Equal(BattleSide.Enemy, made.Target?.Side);
    }

    [Fact]
    public void ACancelWalksBackFromTheTargetsToTheFormToTheLessonToTheAction()
    {
        object menu = Open(OnFirstCommand());
        Move(menu, 1);
        Confirm(menu);
        Move(menu, 1);
        Confirm(menu);
        Confirm(menu);

        Cancel(menu);
        Assert.Equal("Form", Read(menu, "Stage").ToString());
        Cancel(menu);
        Assert.Equal(("Lesson", 1), (Read(menu, "Stage").ToString(), (int)Read(menu, "Cursor")));
        Cancel(menu);
        Assert.Equal(("Action", 1), (Read(menu, "Stage").ToString(), (int)Read(menu, "Cursor")));
    }

    [Fact]
    public void ARiteOfASilencedCharacterStaysOnTheMenuAndRefusesTheConfirm()
    {
        // D-806: the cinder is a rite, and the menu reads the refusal of silence from the rules.
        Simulation run = OnFirstCommand();
        BattleTurns.GiveStatus(run.State, BattleRuns.BattleOf(run).Next()!.Target, StatusKind.Silence, run.State.Context("test"));
        object menu = Open(run);
        Move(menu, 1);
        Confirm(menu);
        Move(menu, 1);

        ContentId cinder = ContentId.Parse("lesson.fixture_cinder", "test", "lesson");
        Assert.False((bool)Type().GetMethod("AllowsLesson")!.Invoke(menu, [cinder])!);
        Assert.Null(Confirm(menu));
        Assert.Equal("Lesson", Read(menu, "Stage").ToString());
    }

    [Fact]
    public void TheMenuReadsTheRowOfTheCharacterWhoseTurnItIs()
    {
        // D-836: the step reads "Back up" in the front row and "Step forward" in the back row,
        // so the menu holds the row of the actor.
        Simulation run = OnFirstCommand();
        Combatant actor = BattleRuns.BattleOf(run).Next()!;

        Assert.Equal(actor.Row, (BattleRow)Read(Open(run), "ActorRow"));
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
        "intent.battle_lesson" => BattleAction.Lesson,
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

    private static BattleAction ActionAt(int index) =>
        ((IReadOnlyList<BattleAction>)Type().GetField("Actions")!.GetValue(null)!)[index];

    private static List<string> Values(IList ids)
    {
        List<string> values = [];
        foreach (object? id in ids)
        {
            values.Add(((ContentId)id!).Value);
        }

        return values;
    }

    private static object Open(Simulation run) =>
        Type().GetMethod("Open")!.Invoke(null, [run.State, Memory(enabled: false)])!;

    /// <summary>Makes a remembered cursor of the built Game assembly (D-226).</summary>
    private static object Memory(bool enabled) =>
        Activator.CreateInstance(GameAssemblyFile.Type("TheThingBelow.Game.Ui.CommandMemory"), [enabled])!;

    private static void Move(object menu, int step) => Type().GetMethod("Move")!.Invoke(menu, [step]);

    private static void MoveRow(object menu, int step) => Type().GetMethod("MoveRow")!.Invoke(menu, [step]);

    private static Intent? Confirm(object menu) => (Intent?)Type().GetMethod("Confirm")!.Invoke(menu, null);

    private static void Cancel(object menu) => Type().GetMethod("Cancel")!.Invoke(menu, null);

    private static IList Targets(object menu) => (IList)Read(menu, "Targets");

    private static object Read(object menu, string name) =>
        Type().GetProperty(name)!.GetValue(menu)
            ?? throw new InvalidOperationException($"The menu gave no value for '{name}' (T-2).");

    private static Type Type() => GameAssemblyFile.Type(CommandsTypeName);
}
