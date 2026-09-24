using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TheThingBelow.Core;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The fight as the battle screen shows it: the view starts from the values of the start of
/// the fight and applies each event, and it reaches the state of the rules when the screen
/// played every event (D-532). The tests read the built Game assembly, because Tests takes no
/// reference to Game (D-614).
/// </summary>
/// <remarks>
/// The rules resolve each enemy turn at once, so the state stands ahead of the screen. A view
/// that read the state would show a hit before its blow, and a view that missed one event
/// would drift from the state for the rest of the fight (T-3).
/// </remarks>
public sealed class BattleViewTests
{
    private const string ViewTypeName = "TheThingBelow.Game.Ui.BattleView";

    /// <summary>The seeds of the property test (D-6).</summary>
    private const ulong SeedCount = 200;

    [Fact]
    public void TheViewMatchesTheStateAfterEveryTickOverTwoHundredSeeds()
    {
        // A seed loop over both fixture groups, one to three characters, and every action of
        // a turn. A status lands on a random combatant on some turns, so the status events
        // reach the view too (D-800).
        string[] groups = ["group.fixture_pair", "group.fixture_elite"];
        for (ulong seed = 0; seed < SeedCount; seed += 1)
        {
            BattleContent content = TestBattles.WithParty((int)(seed % 3) + 1);
            Simulation run = BattleRuns.IntoBattle(seed, groups[seed % 2], content);
            object view = AtStart(run.State);
            ApplyAll(view, run);
            AssertMatches(view, run, seed, 0);

            for (int turn = 0; BattleRuns.BattleOf(run).Outcome == BattleOutcome.Running; turn += 1)
            {
                Assert.True(turn < BattleRuns.TickLimit, $"Seed {seed}: the fight ran past {BattleRuns.TickLimit} turns.");
                GiveStatusOnSomeTurns(run, seed, turn);
                run.Step([ChoiceOf(run, seed, turn)]);
                ApplyAll(view, run);
                AssertMatches(view, run, seed, turn + 1);
            }
        }
    }

    [Fact]
    public void AViewOfAFightThatRunsReadsTheStateAsItStands()
    {
        // D-531: a run that loads a save inside a fight has no event to play.
        Simulation run = BattleRuns.IntoBattle(7, "group.fixture_pair");
        run.TakeBattleEvents();
        object view = Of(run.State);

        AssertMatches(view, run, 7, 0);
    }

    [Fact]
    public void AStartViewHoldsEveryEnemyAtFullHealthAndTheWaitingOnesOffTheField()
    {
        // D-535, D-760: the group entry names the row and the wait of each enemy.
        Simulation run = BattleRuns.IntoBattle(3, "group.fixture_elite");
        object view = AtStart(run.State);

        IReadOnlyList<object> enemies = Side(view, "Enemies");
        Assert.Equal("Field", Read<CombatantPlace>(enemies[0], "Place").ToString());
        Assert.Equal("Field", Read<CombatantPlace>(enemies[2], "Place").ToString());
        Assert.Equal("Waiting", Read<CombatantPlace>(enemies[3], "Place").ToString());
        Assert.Equal("Waiting", Read<CombatantPlace>(enemies[4], "Place").ToString());
        foreach (object enemy in enemies)
        {
            Assert.Equal(Read<int>(enemy, "FullHealth"), Read<int>(enemy, "Health"));
        }
    }

    [Fact]
    public void AHitTakesTheHealthToZeroAtMost()
    {
        // The amount of a hit is the damage of the blow, which can pass the health that the
        // target held. The rules stop at zero, and so does the view.
        Simulation run = BattleRuns.IntoBattle(3, "group.fixture_pair");
        object view = AtStart(run.State);
        var blow = new BattleEvent(BattleEventKind.Hit, new BattleTarget(BattleSide.Party, 0), new BattleTarget(BattleSide.Enemy, 0), 1_000_000);

        Apply(view, blow);

        Assert.Equal(0, Read<int>(Side(view, "Enemies")[0], "Health"));
    }

    [Fact]
    public void AnEventWithNoTargetWhereItNeedsOneIsAnError()
    {
        // T-2: a hit with no target names the kind and the actor.
        Simulation run = BattleRuns.IntoBattle(3, "group.fixture_pair");
        object view = AtStart(run.State);
        var broken = new BattleEvent(BattleEventKind.Hit, new BattleTarget(BattleSide.Party, 0), null, 5);

        TargetInvocationException thrown = Assert.Throws<TargetInvocationException>(() => Apply(view, broken));

        Assert.IsType<ArgumentException>(thrown.InnerException);
        Assert.Contains("hit", thrown.InnerException!.Message, StringComparison.Ordinal);
    }

    /// <summary>Gives a status to a random combatant on the field on some turns (D-800).</summary>
    private static void GiveStatusOnSomeTurns(Simulation run, ulong seed, int turn)
    {
        if ((seed + (ulong)turn) % 3 != 0)
        {
            return;
        }

        // The rules refuse a stun or a sleep on the combatant whose turn is open (D-802), so
        // the status goes to another one.
        Battle battle = BattleRuns.BattleOf(run);
        Combatant? next = battle.Next();
        var standing = new List<Combatant>();
        foreach (Combatant combatant in battle.All())
        {
            if (combatant.Place == CombatantPlace.Field && !ReferenceEquals(combatant, next))
            {
                standing.Add(combatant);
            }
        }

        if (standing.Count == 0)
        {
            return;
        }

        Combatant chosen = standing[(int)((seed * 7) + (ulong)turn) % standing.Count];
        StatusKind status = Statuses.All[(int)((seed + (ulong)(turn * 5)) % (ulong)Statuses.All.Count)];
        BattleTurns.GiveStatus(run.State, chosen.Target, status, run.State.Context("test/status"));
    }

    /// <summary>Picks the choice of one turn: an attack, a step, an item, a defend, or a lesson that the rules take (D-359, D-1027).</summary>
    private static Intent ChoiceOf(Simulation run, ulong seed, int turn)
    {
        int pick = (int)((seed + (ulong)turn) % 5);
        if (pick == 1)
        {
            return Intent.OfPlayer(IntentIds.BattleStep);
        }

        if (pick == 3)
        {
            return Intent.OfPlayer(IntentIds.BattleDefend);
        }

        if (pick == 2)
        {
            foreach (PackValues entry in run.State.Characters.Pack)
            {
                var target = new BattleTarget(BattleSide.Party, 0);
                if (entry.Count > 0 && BattleTurns.RefusalOf(run.State, new BattleChoice(BattleAction.Item, target, entry.Id)) is null)
                {
                    return Intent.OfPlayer(IntentIds.BattleItem, target, entry.Id);
                }
            }
        }

        if (pick == 4)
        {
            Battle battle = BattleRuns.BattleOf(run);
            foreach (string name in new[] { "cinder", "hew" })
            {
                var lesson = ContentId.Parse($"lesson.fixture_{name}", "test", "lesson");
                foreach (Combatant enemy in battle.Enemies)
                {
                    if (BattleTurns.RefusalOf(run.State, new BattleChoice(BattleAction.Lesson, enemy.Target, null, lesson, 0)) is null)
                    {
                        return Intent.OfBattleLesson(lesson, 0, enemy.Target);
                    }
                }
            }
        }

        return BattleRuns.AttackFirst(run);
    }

    private static void AssertMatches(object view, Simulation run, ulong seed, int turn)
    {
        Battle battle = BattleRuns.BattleOf(run);
        AssertSide(Side(view, "Party"), battle.Party, seed, turn);

        // The view spends the MP of each lesson event, as the rules do (D-1027).
        IReadOnlyList<object> party = Side(view, "Party");
        for (int slot = 0; slot < party.Count; slot += 1)
        {
            int held = run.State.Characters.Members[slot].Mp;
            Assert.True(held == Read<int>(party[slot], "Mp"), $"Seed {seed}, turn {turn}, party {slot}: the view shows MP {Read<int>(party[slot], "Mp")}, and the state holds {held}.");
        }

        AssertSide(Side(view, "Enemies"), battle.Enemies, seed, turn);
    }

    private static void AssertSide(IReadOnlyList<object> shown, IReadOnlyList<Combatant> state, ulong seed, int turn)
    {
        Assert.Equal(state.Count, shown.Count);
        for (int slot = 0; slot < state.Count; slot += 1)
        {
            Combatant combatant = state[slot];
            object view = shown[slot];
            string where = $"Seed {seed}, turn {turn}, {combatant.Target.Describe()}";
            Assert.True(combatant.Health == Read<int>(view, "Health"), $"{where}: the view shows health {Read<int>(view, "Health")}, and the state holds {combatant.Health}.");
            Assert.True(combatant.Row == Read<BattleRow>(view, "Row"), $"{where}: the view shows another row.");
            Assert.True(combatant.Place == Read<CombatantPlace>(view, "Place"), $"{where}: the view shows the place {Read<CombatantPlace>(view, "Place")}, and the state holds {combatant.Place}.");

            var held = new List<StatusKind>();
            foreach (StatusKind status in Statuses.All)
            {
                if (combatant.Statuses.Holds(status))
                {
                    held.Add(status);
                }
            }

            var viewed = new List<StatusKind>();
            foreach (object status in (IEnumerable)view.GetType().GetProperty("Statuses")!.GetValue(view)!)
            {
                viewed.Add((StatusKind)status);
            }

            Assert.True(held.Count == viewed.Count && held.TrueForAll(viewed.Contains), $"{where}: the view shows [{string.Join(", ", viewed)}], and the state holds [{string.Join(", ", held)}].");
        }
    }

    private static void ApplyAll(object view, Simulation run)
    {
        foreach (BattleEvent played in run.TakeBattleEvents())
        {
            Apply(view, played);
        }
    }

    private static object AtStart(RunState state) =>
        GameAssemblyFile.Type(ViewTypeName).GetMethod("AtStart")!.Invoke(null, [state])!;

    private static object Of(RunState state) =>
        GameAssemblyFile.Type(ViewTypeName).GetMethod("Of")!.Invoke(null, [state])!;

    private static void Apply(object view, BattleEvent played) =>
        view.GetType().GetMethod("Apply")!.Invoke(view, [played]);

    private static IReadOnlyList<object> Side(object view, string name)
    {
        var found = new List<object>();
        foreach (object shown in (IEnumerable)view.GetType().GetProperty(name)!.GetValue(view)!)
        {
            found.Add(shown);
        }

        return found;
    }

    private static T Read<T>(object instance, string name) =>
        (T)(instance.GetType().GetProperty(name)?.GetValue(instance)
            ?? throw new InvalidOperationException($"The Game type '{instance.GetType().Name}' holds no '{name}' (T-2)."));
}
